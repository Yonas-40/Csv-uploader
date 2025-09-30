# CSV User Uploader

A Blazor Server application for uploading and managing users via CSV files.

## Features
- CSV file upload with validation
- Real-time progress tracking
- Bulk user import with duplicate detection
- Input validation for email, password strength, and required fields

## Technologies Used
- .NET 8 / Blazor Server
- Entity Framework Core
- CsvHelper
- Bootstrap 5

## Setup
1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Run the application: `dotnet run`

## CSV Format
Required columns: Full Name, Username, Email, Password
