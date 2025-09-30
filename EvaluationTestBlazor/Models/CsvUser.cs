using System.ComponentModel.DataAnnotations;

namespace EvaluationTesst.Models;

public class CsvUser
{
    [StringLength(100)]
    public string? FullName { get; set; }
    [StringLength(100)]
    public string? Username { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Password must be 8+ characters with uppercase, lowercase, number, and special character")]
    public string? Password { get; set; }
    public int RowNumber { get; set; }

    // Validation results
    public List<string> ValidationErrors { get; set; } = new();
    public bool IsValid => !ValidationErrors.Any();
    public void ValidateFields()
    {
        ValidationErrors.Clear();

        // Full Name validation
        if (string.IsNullOrWhiteSpace(FullName))
            ValidationErrors.Add("Full name is required");
        else if (FullName.Length > 100)
            ValidationErrors.Add("Full name must be 100 characters or less");

        // Username validation
        if (string.IsNullOrWhiteSpace(Username))
            ValidationErrors.Add("Username is required");
        else if (Username.Length > 100)
            ValidationErrors.Add("Username must be 100 characters or less");

        // Email validation
        if (string.IsNullOrWhiteSpace(Email))
            ValidationErrors.Add("Email is required");
        else if (!IsValidEmail(Email))
            ValidationErrors.Add("Email must be in valid format (e.g., user@example.com)");

        // Password validation
        if (string.IsNullOrWhiteSpace(Password))
            ValidationErrors.Add("Password is required");
        else if (!IsValidPassword(Password))
            ValidationErrors.Add("Password must be longer than 8 characters and contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPassword(string password)
    {
        if (password.Length < 8) return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

}
