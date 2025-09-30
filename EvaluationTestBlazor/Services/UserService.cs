// Services/UserService.cs
using EvaluationTesst.Data;
using EvaluationTesst.Models;
using EvaluationTestBlazor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EvaluationTesst.Services
{
    /// <summary>
    /// Service that handles all user-related operations like saving, checking duplicates, and retrieving users
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Saves a list of valid users to the database
        /// Only saves users that pass validation and don't have duplicate usernames/emails
        /// </summary>
        /// <param name="validUsers">List of users from CSV file</param>
        /// <returns>The number of users successfully saved</returns>
        public async Task<int> SaveUsersAsync(List<CsvUser> validUsers)
        {
            if (!validUsers.Any())
                return 0;

            // This list will hold users that are ready to be saved to the database
            var users = new List<User>();

            // Loop through each user that passed validation
            foreach (var userDto in validUsers.Where(u => u.IsValid))
            {
                var usernameExists = await IsUsernameExistsAsync(userDto.Username);

                var emailExists = await IsEmailExistsAsync(userDto.Email);

                // If either username or email already exists, skip this user
                if (usernameExists || emailExists)
                {
                    continue;
                }

                // Create a new User object from the CSV data
                var user = new User
                {
                    FullName = userDto.FullName,
                    Username = userDto.Username,
                    Email = userDto.Email,
                    CreatedAt = DateTime.UtcNow
                };

                user.Password = HashPassword(user, userDto.Password);

                users.Add(user);
            }

            if (users.Any())
            {
                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();
            }

            return users.Count;
        }

        /// <summary>
        /// Checks if a username already exists in the database
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>True if username exists, false if it doesn't</returns>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        /// <summary>
        /// Checks if an email already exists in the database
        /// </summary>
        /// <param name="email">The email to check</param>
        /// <returns>True if email exists, false if it doesn't</returns>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Gets all users from the database, ordered by when they were created
        /// </summary>
        /// <returns>List of all users, oldest first</returns>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Hashes a password for secure storage
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The hashed password string</returns>
        private string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }
    }
}