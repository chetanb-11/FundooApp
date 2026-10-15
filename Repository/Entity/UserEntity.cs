using System.ComponentModel.DataAnnotations;

namespace Repository.Entity;

public class UserEntity
{
    [Required]
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public string FirstName { get; set; }
    
    [Required, MaxLength(50)]
    public string LastName { get; set; }
    
    [Required, MaxLength(50)]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required, RegularExpression("^[0-9]{10}$")] 
    public string ContactNumber { get; set; }
    
    [Required]
    public string Password { get; set; }
}
