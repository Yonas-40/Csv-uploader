using EvaluationTesst.Data;
using EvaluationTesst.Models;
using EvaluationTesst.Services;
using EvaluationTestBlazor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace EvaluationTestt.Tests;
// Tests for UserService - This service handles saving users to the database
public class UserServiceTests : IDisposable
{
    // These are the objects we need for testing
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher;

    // This runs before each test
    public UserServiceTests()
    {
        // Create a fake in-memory database for testing
        // Each test gets a fresh database with a unique name
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Create a fake password hasher so we don't hash passwords for real in tests
        _mockPasswordHasher = new Mock<IPasswordHasher<User>>();

        // Make the fake hasher always return "hashedpassword" when we hash
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                          .Returns("hashedpassword");

        // Make the fake hasher always say passwords match
        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(PasswordVerificationResult.Success);

        // Create the actual service we're testing
        _userService = new UserService(_context, _mockPasswordHasher.Object);
    }

    // Test: Can we save valid users to the database?
    [Fact]
    public async Task SaveUsersAsync_WithTwoValidUsers_SavesBothToDatabase()
    {
        // Step 1: Arrange - Set up the test data
        var users = new List<CsvUser>
        {
            new CsvUser
            {
                FullName = "John Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "Password123!",
                ValidationErrors = new List<string>()
            },
            new CsvUser
            {
                FullName = "Jane Smith",
                Username = "janesmith",
                Email = "jane@example.com",
                Password = "SecurePass456@",
                ValidationErrors = new List<string>()
            }
        };

        // Step 2: Act - Do the thing we're testing
        var result = await _userService.SaveUsersAsync(users);

        // Step 3: Assert - Check if it worked correctly
        // Check that we saved 2 users
        Assert.Equal(2, result);

        // Get all users from the database
        var savedUsers = await _context.Users.ToListAsync();
        Assert.Equal(2, savedUsers.Count);

        // Check if John Doe was saved correctly
        var johnDoe = savedUsers.FirstOrDefault(u => u.Username == "johndoe");
        Assert.NotNull(johnDoe);
        Assert.Equal("John Doe", johnDoe.FullName);
        Assert.Equal("john@example.com", johnDoe.Email);
        Assert.Equal("hashedpassword", johnDoe.Password); // Password should be hashed
    }

    // Test: Invalid users should not be saved
    [Fact]
    public async Task SaveUsersAsync_WithInvalidUser_DoesNotSaveToDatabase()
    {
        // Arrange: Create a user with validation errors (empty full name)
        var users = new List<CsvUser>
        {
            new CsvUser
            {
                FullName = "", // This is invalid!
                Username = "johndoe",
                Email = "john@example.com",
                Password = "Password123!",
                ValidationErrors = new List<string> { "Full name is required" }
            }
        };

        // Act: Try to save the invalid user
        var result = await _userService.SaveUsersAsync(users);

        // Assert: Nothing should be saved
        Assert.Equal(0, result);
        var savedUsers = await _context.Users.ToListAsync();
        Assert.Empty(savedUsers); // Database should be empty
    }

    // Test: Empty list should return 0
    [Fact]
    public async Task SaveUsersAsync_WithEmptyList_ReturnsZero()
    {
        // Arrange: Empty list
        var users = new List<CsvUser>();

        // Act: Save empty list
        var result = await _userService.SaveUsersAsync(users);

        // Assert: Should return 0
        Assert.Equal(0, result);
    }

    // Test: Check if username already exists in database
    [Fact]
    public async Task IsUsernameExistsAsync_WhenUsernameExists_ReturnsTrue()
    {
        // Arrange: Add a user to the database first
        var user = new User
        {
            FullName = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "hashedpassword"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Check if the username exists
        var result = await _userService.IsUsernameExistsAsync("johndoe");

        // Assert: Should be true
        Assert.True(result);
    }

    // Test: Check if username does NOT exist
    [Fact]
    public async Task IsUsernameExistsAsync_WhenUsernameDoesNotExist_ReturnsFalse()
    {
        // Act: Check for a username that doesn't exist
        var result = await _userService.IsUsernameExistsAsync("nonexistentuser");

        // Assert: Should be false
        Assert.False(result);
    }

    // Test: Check if email already exists in database
    [Fact]
    public async Task IsEmailExistsAsync_WhenEmailExists_ReturnsTrue()
    {
        // Arrange: Add a user with an email
        var user = new User
        {
            FullName = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "hashedpassword"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Check if the email exists
        var result = await _userService.IsEmailExistsAsync("john@example.com");

        // Assert: Should be true
        Assert.True(result);
    }

    // Test: Check if email does NOT exist
    [Fact]
    public async Task IsEmailExistsAsync_WhenEmailDoesNotExist_ReturnsFalse()
    {
        // Act: Check for an email that doesn't exist
        var result = await _userService.IsEmailExistsAsync("nonexistent@example.com");

        // Assert: Should be false
        Assert.False(result);
    }

    // Test: Get all users from database
    [Fact]
    public async Task GetAllUsersAsync_WithTwoUsers_ReturnsBothUsers()
    {
        // Arrange: Add two users to the database
        var user1 = new User
        {
            FullName = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "hashedpassword",
            CreatedAt = DateTime.UtcNow.AddDays(-1) // Created yesterday
        };

        var user2 = new User
        {
            FullName = "Jane Smith",
            Username = "janesmith",
            Email = "jane@example.com",
            Password = "hashedpassword",
            CreatedAt = DateTime.UtcNow // Created today
        };

        _context.Users.Add(user1);
        _context.Users.Add(user2);
        await _context.SaveChangesAsync();

        // Act: Get all users
        var result = await _userService.GetAllUsersAsync();

        // Assert: Should get 2 users, ordered by creation date (oldest first)
        Assert.Equal(2, result.Count);
        Assert.Equal("johndoe", result[0].Username); // John created first
        Assert.Equal("janesmith", result[1].Username); // Jane created second
    }

    // This runs after each test to clean up
    public void Dispose()
    {
        _context.Dispose();
    }
}
