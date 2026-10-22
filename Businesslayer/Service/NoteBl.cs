// namespace Businesslayer;

using System.Text.Json;
using BusinessLayer.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Service;

using Microsoft.Extensions.Caching.StackExchangeRedis;
using BusinessLayer.Interface;
using ModelLayer.Model;
using Repository.Entity;
using Repository.Interface;

public class NoteBl(IUserBl userBl, INoteRl noteRl, IDistributedCache cache, ILogger<NoteBl> logger) : INoteBl
{
    private readonly IUserBl _userBl = userBl;
    private readonly INoteRl _noteRl = noteRl;

    private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(600)
    };

    public async Task<NoteResponseDto?> CreateAsync(NoteRequestModel noteRequestModel, int userId)
    {
        if (noteRequestModel == null || userId <= 0)
        {
            logger.LogWarning("Invalid note request or UserId {UserId}", userId);
            return null;
        }

        var note = new NoteEntity
        {
            UserId = userId,
            Title = noteRequestModel.Title,
            Description = noteRequestModel.Description
        };

        var createdNote = await _noteRl.CreateAsync(note);
        if (createdNote == null) return null;

        await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated notes list cache for user {UserId}", userId);

        return new NoteResponseDto
        {
            Id = createdNote.Id,
            Title = createdNote.Title,
            Description = createdNote.Description
        };
    }

    public async Task<bool> DeleteAsync(int noteId, int userId)
    {
        if (noteId <= 0 || userId <= 0)
        {
            logger.LogWarning("Invalid NoteId {NoteId} or UserId {UserId}", noteId, userId);
            return false;
        }

        var userExists = await _userBl.UserExistsByIdAsync(userId);
        if (!userExists) return false;

        var isDeleted = await _noteRl.DeleteAsync(noteId, userId);
        if (!isDeleted) return isDeleted;

        await SafeRemoveCacheAsync($"Notes:User:{userId}:{noteId}");
        await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);

        return isDeleted;
    }

    public async Task<bool> UpdateAsync(NoteDomainModel? noteDomainModel)
    {
        if (noteDomainModel == null || noteDomainModel.Id <= 0 || noteDomainModel.UserId <= 0)
        {
            logger.LogWarning("Invalid NoteDomainModel: NoteId={NoteId}, UserId={UserId}", noteDomainModel?.Id, noteDomainModel?.UserId);
            return false;
        }

        var noteId = noteDomainModel.Id;
        var userId = noteDomainModel.UserId;
        var userExists = await _userBl.UserExistsByIdAsync(userId);
        if (!userExists) return false;

        var noteEntity = new NoteEntity
        {
            Id = noteDomainModel.Id,
            Title = noteDomainModel.Title,
            Description = noteDomainModel.Description,
            UserId = noteDomainModel.UserId
        };

        var result = await _noteRl.UpdateAsync(noteEntity);
        if (result)
        {
            await SafeRemoveCacheAsync($"Notes:User:{userId}:{noteId}");
            await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
            logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);
        }

        return result;
    }

    public async Task<IEnumerable<NoteResponseDto>?> GetAllNotesByUser(int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("Invalid UserId {UserId}", userId);
            return null;
        }

        var key = $"Notes:User:{userId}:All";
        var cached = await SafeGetStringAsync(key);
        if (cached is not null)
        {
            logger.LogInformation("All notes found in Redis cache with key {Key}", key);
            return JsonSerializer.Deserialize<IEnumerable<NoteResponseDto>>(cached);
        }

        var notes = _noteRl.GetAllNotesByUser(userId)
            .Select(note => new NoteResponseDto() { Id = note.Id, Title = note.Title, Description = note.Description })
            .ToList();

        logger.LogInformation("Cached {Count} notes for user {UserId}", notes.Count, userId);
        await SafeSetStringAsync(key, JsonSerializer.Serialize(notes), DefaultCacheOptions);

        return notes;
    }

    public async Task<NoteResponseDto?> GetNoteById(int noteId, int userId)
    {
        if (userId <= 0 || noteId <= 0)
        {
            logger.LogWarning("Invalid NoteId {NoteId} or UserId {UserId}", noteId, userId);
            return null;
        }

        var key = $"Notes:User:{userId}:{noteId}";
        var cached = await SafeGetStringAsync(key);
        if (cached is not null)
        {
            logger.LogInformation("Note found in Redis cache with key {Key}", key);
            return JsonSerializer.Deserialize<NoteResponseDto>(cached);
        }

        var noteEntity = await _noteRl.GetNoteById(noteId, userId);
        if (noteEntity == null) return null;

        var note = ToDto(noteEntity);

        logger.LogInformation("Cached note with key {Key}", key);
        await SafeSetStringAsync(key, JsonSerializer.Serialize(note), DefaultCacheOptions);
        return note;
    }

    public Task<NoteResponseDto?> ArchiveNote(int noteId, int userId)
        => SetArchived(noteId, userId, true);

    public Task<NoteResponseDto?> UnArchiveNote(int noteId, int userId)
        => SetArchived(noteId, userId, false);

    private async Task<NoteResponseDto?> SetArchived(int noteId, int userId, bool isArchived)
    {
        if (noteId <= 0 || userId <= 0)
        {
            logger.LogWarning("Invalid NoteId {NoteId} or UserId {UserId}", noteId, userId);
            return null;
        }

        var note = await _noteRl.SetArchived(noteId, userId, isArchived);
        if (note is null) return null;

        await SafeRemoveCacheAsync($"Notes:User:{userId}:{noteId}");
        await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);

        return ToDto(note);
    }

    public async Task<List<NoteResponseDto>> GetArchivedNotes(int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("Invalid UserId {UserId}", userId);
            return [];
        }

        var note = await _noteRl.GetArchived(userId);

        return note.Select(ToDto).ToList();
    }

    private async Task SafeRemoveCacheAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache unavailable. Could not remove key {Key}", key);
        }
    }

    private async Task<string?> SafeGetStringAsync(string key)
    {
        try
        {
            return await cache.GetStringAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache unavailable. Could not read key {Key}", key);
            return null;
        }
    }

    private async Task SafeSetStringAsync(string key, string value, DistributedCacheEntryOptions options)
    {
        try
        {
            await cache.SetStringAsync(key, value, options);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache unavailable. Could not set key {Key}", key);
        }
    }

    private static NoteResponseDto ToDto(NoteEntity n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Description = n.Description,
        IsArchived = n.IsArchived,
        IsTrashed = n.IsTrashed
    };
}
