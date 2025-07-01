using System.ComponentModel.DataAnnotations;

// Allows us to filter out any columns that we dont want to share with the client
// For example, a password hash
public class ResponseUserDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateOnly? Dob { get; set; }
}