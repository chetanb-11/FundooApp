namespace ModelLayer.Model;

public class UserDomainModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public byte[]? Password { get; set; }
    public byte[]? Salt { get; set; }
}
