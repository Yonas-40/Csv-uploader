using EvaluationTesst.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace EvaluationTestBlazor.Services;

public interface IUserService
{
    Task<int> SaveUsersAsync(List<CsvUser> validUsers);
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task<List<User>> GetAllUsersAsync();

}

