using CsvHelper;
using CsvHelper.Configuration;
using FastPeopleSearchScraper.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FastPeopleSearchScraper.Services;

public class CsvService
{
    private readonly ILogger<CsvService> _logger;

    public CsvService(ILogger<CsvService> logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> ReadPhoneListAsync(string filePath)
    {
        try
        {
            var fileExists = await Task.Run(() => File.Exists(filePath));
            if (!fileExists)
            {
                return new List<string>();
            }

            var phones = await File.ReadAllLinesAsync(filePath);
            var formattedPhones = new List<string>();
            
            foreach (var phone in phones)
            {
                var cleanPhone = phone.Trim();
                if (string.IsNullOrWhiteSpace(cleanPhone))
                    continue;
                    
                // Remove any existing formatting characters
                cleanPhone = cleanPhone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");
                
                // Format as XXX-XXX-XXXX if it's 10 digits
                if (cleanPhone.Length == 10 && cleanPhone.All(char.IsDigit))
                {
                    var formattedPhone = $"{cleanPhone.Substring(0, 3)}-{cleanPhone.Substring(3, 3)}-{cleanPhone.Substring(6, 4)}";
                    formattedPhones.Add(formattedPhone);
                }
                else
                {
                    _logger.LogWarning($"Invalid phone number format (not 10 digits): {cleanPhone}");
                }
            }
            
            return formattedPhones;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading phone list: {ex.Message}");
            throw;
        }
    }

    public async Task WriteStage1ResultsAsync(List<Stage1Result> results, string filePath)
    {
        try
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            csv.WriteField("Phone");
            csv.WriteField("Name");
            csv.WriteField("DetailUrl");
            csv.NextRecord();

            foreach (var result in results)
            {
                // Remove dashes from phone number
                var cleanPhone = result.Phone.Replace("-", "");
                csv.WriteField(cleanPhone);
                csv.WriteField(result.Name);
                csv.WriteField(result.DetailUrl);
                csv.NextRecord();
            }

            await File.WriteAllTextAsync(filePath, writer.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing stage 1 results: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Stage1Result>> ReadStage1ResultsAsync(string filePath)
    {
        try
        {
            var fileExists = await Task.Run(() => File.Exists(filePath));
            if (!fileExists)
            {
                return new List<Stage1Result>();
            }

            using var reader = new StringReader(await File.ReadAllTextAsync(filePath));
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var results = new List<Stage1Result>();
            csv.Read();
            csv.ReadHeader();
            
            while (csv.Read())
            {
                results.Add(new Stage1Result
                {
                    Phone = csv.GetField("Phone") ?? string.Empty,
                    Name = csv.GetField("Name") ?? string.Empty,
                    DetailUrl = csv.GetField("DetailUrl") ?? string.Empty
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading stage 1 results: {ex.Message}");
            throw;
        }
    }

    public async Task WritePeopleAsync(List<Person> people, string filePath)
    {
        try
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            // Headers matching the Excel format
            csv.WriteField("phone");
            csv.WriteField("name");
            csv.WriteField("age");
            csv.WriteField("phonetype");
            csv.WriteField("work");
            csv.WriteField("business");
            csv.WriteField("source");
            csv.NextRecord();

            foreach (var person in people)
            {
                csv.WriteField(person.Phone);
                csv.WriteField(person.Name);
                csv.WriteField(person.Age);
                csv.WriteField(person.PhoneType);
                csv.WriteField(person.Work ? "TRUE" : "FALSE");
                csv.WriteField(person.Business);
                csv.WriteField(person.Source);
                csv.NextRecord();
            }

            await File.WriteAllTextAsync(filePath, writer.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing people: {ex.Message}");
            throw;
        }
    }

    public async Task WriteRelationsAsync(List<Relation> relations, string filePath)
    {
        try
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            csv.WriteField("Source Name");
            csv.WriteField("Source Phone");
            csv.WriteField("Relative URL");
            csv.NextRecord();

            foreach (var relation in relations)
            {
                csv.WriteField(relation.SourceName);
                csv.WriteField(relation.SourcePhone);
                csv.WriteField(relation.RelativeUrl);
                csv.NextRecord();
            }

            await File.WriteAllTextAsync(filePath, writer.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing relations: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Person>> ReadPeopleAsync(string filePath)
    {
        try
        {
            var fileExists = await Task.Run(() => File.Exists(filePath));
            if (!fileExists)
            {
                return new List<Person>();
            }

            using var reader = new StringReader(await File.ReadAllTextAsync(filePath));
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var people = new List<Person>();
            csv.Read();
            csv.ReadHeader();
            
            while (csv.Read())
            {
                people.Add(new Person
                {
                    Phone = csv.GetField("phone") ?? string.Empty,
                    Name = csv.GetField("name") ?? string.Empty,
                    Age = int.Parse(csv.GetField("age") ?? "0"),
                    PhoneType = csv.GetField("phonetype") ?? string.Empty,
                    Work = (csv.GetField("work") ?? "FALSE").ToUpper() == "TRUE",
                    Business = csv.GetField("business") ?? string.Empty,
                    Source = csv.GetField("source") ?? string.Empty
                });
            }

            return people;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading people: {ex.Message}");
            throw;
        }
    }
} 