using System.ComponentModel.DataAnnotations;

public class UpdateUserDTO
{
    public Optional<string>? FirstName { get; set; }
    public Optional<string>? LastName { get; set; }
    public Optional<string>? Email { get; set; }
    public Optional<string?>? PhoneNumber { get; set; }
    public Optional<DateOnly?>? Dob { get; set; }

    public string Print()
    {
        return @$"FirstName:{FirstName?.value}
        LastName:{LastName?.value}
        Email:{Email?.value}
        PhoneNumber:{PhoneNumber?.value}
        Dob:{Dob?.value}";
    }
}