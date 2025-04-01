using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AstralAlignment.Models;

namespace AstralAlignment.Services
{
    public class UserDataService
    {
        private readonly string _dataFilePath;

        public UserDataService()
        {
            // Create a data directory in the application folder
            string dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AstralAlignment");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
            _dataFilePath = Path.Combine(dataDirectory, "users.json");
        }

        public async Task<List<User>> LoadUsersAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                return new List<User>();
            }

            try
            {
                string jsonString = await File.ReadAllTextAsync(_dataFilePath);
                return JsonSerializer.Deserialize<List<User>>(jsonString) ?? new List<User>();
            }
            catch (Exception)
            {
                // If loading fails, return an empty list
                return new List<User>();
            }
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(users, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(_dataFilePath, jsonString);
            }
            catch (Exception)
            {
                // Consider logging the exception or handling it more gracefully
                // Silently failing might hide important issues
            }
        }
    }
}