using System.ComponentModel.DataAnnotations;

namespace EvaluationTesst.Models;

public class User
{
    public int Id { get; set; }
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
    public DateTime CreatedAt { get; set; } = DateTime.Now;

}
