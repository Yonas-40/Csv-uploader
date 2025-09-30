using CsvHelper;
using CsvHelper.Configuration;
using EvaluationTesst.Models;
using System.Globalization;
using System.Text;

namespace EvaluationTestBlazor.Services
{
    /// <summary>
    /// Service that handles reading and parsing CSV files containing user data
    /// </summary>
    public class CsvService : ICsvService
    {
        /// <summary>
        /// Reads a CSV file and converts it into a list of CsvUser objects
        /// Also validates each user and adds any validation errors
        /// </summary>
        /// <param name="csvStream">The CSV file data as a stream</param>
        /// <returns>List of users from the CSV, including validation status</returns>
        public async Task<List<CsvUser>> ParseCsvAsync(Stream csvStream)
        {
            var users = new List<CsvUser>();

            try
            {
                // Create a reader to read the CSV file
                using var reader = new StreamReader(csvStream, Encoding.UTF8);

                // Configure how we want to read the CSV
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,      
                    MissingFieldFound = null,     
                    HeaderValidated = null,        
                    TrimOptions = TrimOptions.Trim 
                };

                using var csv = new CsvReader(reader, config);

                await csv.ReadAsync();
                csv.ReadHeader();

                var headers = csv.HeaderRecord;
                if (headers == null)
                {
                    throw new InvalidOperationException("CSV file must have headers");
                }

                var fullNameIndex = FindColumnIndex(headers, "fullname", "full_name", "FullName");
                var usernameIndex = FindColumnIndex(headers, "Username", "user_name");
                var emailIndex = FindColumnIndex(headers, "Email", "email_address");
                var passwordIndex = FindColumnIndex(headers, "Password", "pwd");

                int rowNumber = 0;

                while (await csv.ReadAsync())
                {
                    rowNumber++;

                    var fullName = GetFieldValue(csv, fullNameIndex);
                    var username = GetFieldValue(csv, usernameIndex);
                    var email = GetFieldValue(csv, emailIndex);
                    var password = GetFieldValue(csv, passwordIndex);

                    var user = new CsvUser
                    {
                        RowNumber = rowNumber,
                        FullName = fullName,
                        Username = username,
                        Email = email,
                        Password = password
                    };

                    user.ValidateFields();

                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                // If something goes wrong reading the CSV, add an error user
                // RowNumber 0 means this is a parsing error, not a data error
                users.Add(new CsvUser
                {
                    RowNumber = 0,
                    ValidationErrors = new List<string> { $"CSV parsing error: {ex.Message}" }
                });
            }

            return users;
        }

        /// <summary>
        /// Finds which column number contains a specific field
        /// Checks multiple possible names because users might name columns differently
        /// For example: "Full Name", "FullName", "full_name" should all work
        /// </summary>
        /// <param name="headers">Array of column names from the CSV</param>
        /// <param name="possibleNames">Different names that this column might have</param>
        /// <returns>The index (position) of the column, or -1 if not found</returns>
        private int FindColumnIndex(string[] headers, params string[] possibleNames)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var headerLower = headers[i].ToLower().Replace(" ", "").Replace("_", "");

                foreach (var name in possibleNames)
                {
                    var nameLower = name.Replace("_", "").ToLower();

                    if (headerLower.Equals(nameLower, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the value from a specific column in the current CSV row
        /// Handles cases where the column doesn't exist or has no value
        /// </summary>
        /// <param name="csv">The CSV reader</param>
        /// <param name="index">Which column to read from (position number)</param>
        /// <returns>The value from that column, or empty string if not found</returns>
        private string GetFieldValue(CsvReader csv, int index)
        {
            if (index < 0)
            {
                return string.Empty;
            }

            if (index >= csv.Parser.Count)
            {
                return string.Empty;
            }

            var value = csv.Parser[index]?.Trim() ?? string.Empty;
            return value;
        }
    }
}