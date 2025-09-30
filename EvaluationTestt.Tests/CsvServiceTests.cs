using EvaluationTesst.Data;
using EvaluationTesst.Models;
using EvaluationTesst.Services;
using EvaluationTestBlazor.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvaluationTestt.Tests;
// Tests for CsvService - This service reads and validates CSV files
public class CsvServiceTests
{
    private readonly ICsvService _csvService;

    // This runs before each test
    public CsvServiceTests()
    {
        _csvService = new CsvService();
    }

    // Test: Can we read a valid CSV file?
    [Fact]
    public async Task ParseCsvAsync_WithValidCsv_ReturnsCorrectUsers()
    {
        // Arrange: Create a fake CSV file as text
        var csvContent = "FullName,Username,Email,Password\n" +
                        "John Doe,johndoe,john@example.com,Password123!\n" +
                        "Jane Smith,janesmith,jane@example.com,SecurePass456@";

        // Convert the text to a stream (like reading a file)
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act: Parse the CSV
        var result = await _csvService.ParseCsvAsync(stream);

        // Assert: Check we got 2 users
        Assert.Equal(2, result.Count);

        // Check first user details
        var firstUser = result[0];
        Assert.Equal("John Doe", firstUser.FullName);
        Assert.Equal("johndoe", firstUser.Username);
        Assert.Equal("john@example.com", firstUser.Email);
        Assert.Equal("Password123!", firstUser.Password);
        Assert.Equal(1, firstUser.RowNumber);
        Assert.True(firstUser.IsValid);

        // Check second user details
        var secondUser = result[1];
        Assert.Equal("Jane Smith", secondUser.FullName);
        Assert.Equal("janesmith", secondUser.Username);
        Assert.Equal("jane@example.com", secondUser.Email);
        Assert.Equal("SecurePass456@", secondUser.Password);
        Assert.Equal(2, secondUser.RowNumber);
        Assert.True(secondUser.IsValid);
    }

    // Test: Invalid email should add error
    [Fact]
    public async Task ParseCsvAsync_WithInvalidEmail_AddsValidationError()
    {
        // Arrange: CSV with bad email
        var csvContent = "FullName,Username,Email,Password\n" +
                        "John Doe,johndoe,invalid-email,Password123!";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act: Parse the CSV
        var result = await _csvService.ParseCsvAsync(stream);

        // Assert: User should be invalid with error message
        Assert.Single(result); // Only 1 user
        var user = result[0];
        Assert.False(user.IsValid);
        Assert.Contains(user.ValidationErrors, e => e.Contains("Email must be in valid format"));
    }

    // Test: Weak password should add error
    [Fact]
    public async Task ParseCsvAsync_WithWeakPassword_AddsValidationError()
    {
        // Arrange: CSV with weak password
        var csvContent = "FullName,Username,Email,Password\n" +
                        "John Doe,johndoe,john@example.com,weak";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act: Parse the CSV
        var result = await _csvService.ParseCsvAsync(stream);

        // Assert: User should be invalid
        Assert.Single(result);
        var user = result[0];
        Assert.False(user.IsValid);
        Assert.Contains(user.ValidationErrors, e => e.Contains("Password must be longer than 8 characters"));
    }

    // Test: Missing required fields should add errors
    [Fact]
    public async Task ParseCsvAsync_WithMissingFields_AddsValidationErrors()
    {
        // Arrange: CSV with empty full name and email
        var csvContent = "FullName,Username,Email,Password\n" +
                        ",johndoe,,Password123!";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act: Parse the CSV
        var result = await _csvService.ParseCsvAsync(stream);

        // Assert: Should have multiple validation errors
        Assert.Single(result);
        var user = result[0];
        Assert.False(user.IsValid);
        Assert.Contains(user.ValidationErrors, e => e.Contains("Full name is required"));
        Assert.Contains(user.ValidationErrors, e => e.Contains("Email is required"));
    }

    // Test: Fields that are too long should add errors
    [Fact]
    public async Task ParseCsvAsync_WithTooLongFields_AddsValidationErrors()
    {
        // Arrange: Create really long strings (more than 100 characters)
        var longName = new string('a', 101); // 101 'a' characters
        var longUsername = new string('b', 101); // 101 'b' characters
        var csvContent = $"FullName,Username,Email,Password\n" +
                        $"{longName},{longUsername},john@example.com,Password123!";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act: Parse the CSV
        var result = await _csvService.ParseCsvAsync(stream);

        // Assert: Should have length errors
        Assert.Single(result);
        var user = result[0];
        Assert.False(user.IsValid);
        Assert.Contains(user.ValidationErrors, e => e.Contains("Full name must be 100 characters or less"));
        Assert.Contains(user.ValidationErrors, e => e.Contains("Username must be 100 characters or less"));
    }

    // Test: Empty CSV file (only headers)
    [Fact]
    public async Task ParseCsvAsync_WithEmptyCsv_ReturnsEmptyList()
    {
        // Arrange: CSV with only headers, no data
        var csvContent = "FullName,Username,Email,Password\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act: Parse the CSV
        var result = await _csvService.ParseCsvAsync(stream);

        // Assert: Should return empty list
        Assert.Empty(result);
    }

    // Test: CSV with different column names should still work
    [Fact]
    public async Task ParseCsvAsync_WithAlternativeColumnNames_ParsesCorrectly()
    {
        // Arrange: CSV with spaces in column names
        var csvContent = "Full Name,User Name,Email Address,Password\n" +
                        "John Doe,johndoe,john@example.com,Password123!";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act: Parse the CSV
        var result = await _csvService.ParseCsvAsync(stream);

        // Assert: Should still parse correctly
        Assert.Single(result);
        var user = result[0];
        Assert.Equal("John Doe", user.FullName);
        Assert.Equal("johndoe", user.Username);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal("Password123!", user.Password);
        Assert.True(user.IsValid);
    }
}