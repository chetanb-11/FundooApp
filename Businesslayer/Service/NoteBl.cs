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
    // private readonly ILogger<NoteBl> logger;

    public async Task<NoteResponseDto?> CreateAsync(NoteRequestModel noteRequestModel, int userId)
    {
        if (noteRequestModel == null || userId <= 0)
        {
            logger.LogWarning("note req or userId not valid");
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

        await cache.RemoveAsync($"Notes:User:{userId}:All");
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
            logger.LogWarning("noteId or userId not valid");
            return false;
        }

        var userExists = await _userBl.UserExistsByIdAsync(userId);
        if (!userExists) return false;


        var isDeleted = await _noteRl.DeleteAsync(noteId, userId);
        if (!isDeleted) return isDeleted;
        
        await cache.RemoveAsync($"Notes:User:{userId}:{noteId}");
        await cache.RemoveAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);

        return isDeleted;
    }

    public async Task<bool> UpdateAsync(NoteDomainModel? noteDomainModel)
    {
        if (noteDomainModel == null || noteDomainModel.Id <= 0 || noteDomainModel.UserId <= 0)
        {
            logger.LogWarning("note doman model not valid");
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

        await cache.RemoveAsync($"Notes:User:{userId}:{noteId}");
        await cache.RemoveAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);
        return await _noteRl.UpdateAsync(noteEntity);
    }

    public async Task<IEnumerable<NoteResponseDto>?> GetAllNotesByUser(int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("userId not valid");
            return null;
        }

        var key = $"Notes:User:{userId}:All";
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
        {
            logger.LogInformation("all notes found in redis with key: {key}", key);
            return JsonSerializer.Deserialize<IEnumerable<NoteResponseDto>>(cached);
        }

        var notes = _noteRl.GetAllNotesByUser(userId)
            .Select(note => new NoteResponseDto() { Id = note.Id, Title = note.Title, Description = note.Description })
            .ToList();

        logger.LogInformation("Cached {Count} notes for user {UserId}", notes.Count, userId);
        await cache.SetStringAsync(key, JsonSerializer.Serialize(notes),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(600)
            });

        return notes;
    }

    public async Task<NoteResponseDto?> GetNoteById(int noteId, int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("userId not valid");
            return null;
        }

        var key = $"Notes:User:{userId}:{noteId}";
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
        {
            logger.LogInformation("data found in redis with key: {key}", key);
            return JsonSerializer.Deserialize<NoteResponseDto>(cached);
        }

        var noteEntity = await _noteRl.GetNoteById(noteId, userId);
        if (noteEntity == null) return null;

        var note = ToDto(noteEntity);

        logger.LogInformation("data saving in redis with key: {key}", key);
        await cache.SetStringAsync(key, JsonSerializer.Serialize(note)
            , new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(600)
            }
        );
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
            logger.LogWarning("noteId or userId not valid");
            return null;
        }

        var note = await _noteRl.SetArchived(noteId, userId, isArchived);
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);
        await cache.RemoveAsync($"Notes:User:{userId}:{noteId}");
        await cache.RemoveAsync($"Notes:User:{userId}:All");
        return note is null ? null : ToDto(note);
    }

    public async Task<List<NoteResponseDto>> GetArchivedNotes(int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("userId not valid");
            return [];
        };

        var note = await _noteRl.GetArchived(userId);

        return note.Select(ToDto)
            .ToList();
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