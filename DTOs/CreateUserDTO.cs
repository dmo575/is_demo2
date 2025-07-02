using System.ComponentModel.DataAnnotations;

public class CreateUserDTO
{
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    public Optional<string?>? PhoneNumber { get; set; }
    public Optional<DateOnly?>? Dob { get; set; }
}