using FastPeopleSearchScraper.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace FastPeopleSearchScraper.Services;

public class FastPeopleSearchOrchestrator
{
    private readonly ScraperService _scraperService;
    private readonly CsvService _csvService;
    private readonly ILogger<FastPeopleSearchOrchestrator> _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly HashSet<string> _processedUrls = new();
    private readonly object _lockObject = new();
    
    // Progress tracking
    private int _totalItems;
    private int _processedItems;
    private string _currentStage = "";

    public FastPeopleSearchOrchestrator(
        ScraperService scraperService, 
        CsvService csvService, 
        ILogger<FastPeopleSearchOrchestrator> logger,
        int maxConcurrency = 5)
    {
        _scraperService = scraperService;
        _csvService = csvService;
        _logger = logger;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    private void InitializeProgress(string stageName, int totalItems)
    {
        _currentStage = stageName;
        _totalItems = totalItems;
        _processedItems = 0;
    }
    
    private string GetProgressString()
    {
        lock (_lockObject)
        {
            _processedItems++;
            var percentage = (_processedItems * 100.0) / _totalItems;
            return $"{_currentStage} Progress: {_processedItems}/{_totalItems} ({percentage:F1}%)";
        }
    }

    public async Task<List<Stage1Result>> RunStage1Async(List<string> phoneNumbers)
    {
        Console.WriteLine($"Starting Stage 1 - Processing {phoneNumbers.Count} phone numbers");
        InitializeProgress("Stage 1", phoneNumbers.Count);
        
        var results = new ConcurrentBag<Stage1Result>();
        var tasks = phoneNumbers.Select(async phone =>
        {
            await _semaphore.WaitAsync();
            try
            {
                var progressInfo = GetProgressString();
                await ProcessPhoneNumberAsync(phone, results, progressInfo);
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        
        var resultList = results.ToList();
        Console.WriteLine($"Stage 1 completed - Found {resultList.Count} results");
        return resultList;
    }

    private async Task ProcessPhoneNumberAsync(string phone, ConcurrentBag<Stage1Result> results, string progressInfo)
    {
        try
        {
            var phoneUrl = $"https://www.fastpeoplesearch.com/{phone}";
            var html = await _scraperService.FetchHtmlAsync(phoneUrl, progressInfo);
            
            if (string.IsNullOrEmpty(html))
            {
                return;
            }

            var (name, detailUrl) = _scraperService.ParsePhonePage(html);
            
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(detailUrl))
            {
                results.Add(new Stage1Result
                {
                    Phone = phone,
                    Name = name,
                    DetailUrl = detailUrl
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing phone {phone}: {ex.Message}");
        }
    }

    public async Task<(List<Person> people, List<Relation> relations)> RunStage2Async(List<Stage1Result> stage1Results)
    {
        Console.WriteLine($"Starting Stage 2 - Processing {stage1Results.Count} detail URLs");
        InitializeProgress("Stage 2", stage1Results.Count);
        
        var allPeople = new ConcurrentBag<Person>();
        var allRelations = new ConcurrentBag<Relation>();

        // Initialize processed URLs with stage 1 URLs
        foreach (var result in stage1Results)
        {
            lock (_lockObject)
            {
                _processedUrls.Add(result.DetailUrl);
            }
        }

        // Process ONLY stage 1 results - no recursive expansion
        var initialTasks = stage1Results.Select(async result =>
        {
            await _semaphore.WaitAsync();
            try
            {
                var progressInfo = GetProgressString();
                // Remove dashes from phone number for storage and pass original phone as source
                var cleanPhone = result.Phone.Replace("-", "");
                var cleanSourcePhone = result.Phone.Replace("-", ""); // Remove dashes from source too
                var person = await ProcessDetailUrlAsync(result.DetailUrl, result.Name, cleanPhone, cleanSourcePhone, progressInfo);
                if (person != null)
                {
                    allPeople.Add(person);
                    
                    // Add relations for this person
                    foreach (var relativeUrl in person.RelativeUrls)
                    {
                        allRelations.Add(new Relation
                        {
                            SourceName = person.Name,
                            SourcePhone = cleanSourcePhone,
                            RelativeUrl = relativeUrl
                        });
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(initialTasks);

        var people = allPeople.ToList();
        var relations = allRelations.ToList();
        
        Console.WriteLine($"Stage 2 completed - Found {people.Count} people and {relations.Count} relations");
        
        return (people, relations);
    }

    public async Task<List<Person>> RunStage3Async(List<Relation> relations)
    {
        Console.WriteLine($"Starting Stage 3 - Processing {relations.Count} relative URLs");
        
        var allPeople = new ConcurrentBag<Person>();
        
        // Get unique relative URLs that haven't been processed yet
        var uniqueRelativeUrls = relations
            .Select(r => r.RelativeUrl)
            .Distinct()
            .Where(url => !_processedUrls.Contains(url))
            .ToList();

        Console.WriteLine($"Stage 3 - Found {uniqueRelativeUrls.Count} unique unprocessed relative URLs");
        InitializeProgress("Stage 3", uniqueRelativeUrls.Count);

        var tasks = uniqueRelativeUrls.Select(async relativeUrl =>
        {
            await _semaphore.WaitAsync();
            try
            {
                var progressInfo = GetProgressString();
                // Find the source phone number for this relative URL
                var sourceRelation = relations.FirstOrDefault(r => r.RelativeUrl == relativeUrl);
                var sourcePhone = sourceRelation?.SourcePhone ?? "Unknown"; // Already clean without dashes
                
                var person = await ProcessDetailUrlAsync(relativeUrl, "", "", sourcePhone, progressInfo);
                if (person != null)
                {
                    allPeople.Add(person);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        var people = allPeople.ToList();
        Console.WriteLine($"Stage 3 completed - Found {people.Count} additional people from relatives");
        
        return people;
    }

    private async Task<Person?> ProcessDetailUrlAsync(string url, string fallbackName, string phone, string source, string progressInfo)
    {
        try
        {
            var (htmlName, age, phoneType, work, business, address, relativeUrls) = await _scraperService.GetDetailPageDataAsync(url, progressInfo);
            
            // HTML'den gelen ismi kullan, eğer boşsa fallback name'i kullan
            var finalName = !string.IsNullOrEmpty(htmlName) ? htmlName : fallbackName;
            
            if (string.IsNullOrEmpty(finalName))
            {
                return null;
            }

            var person = new Person
            {
                Phone = phone,
                Name = finalName,
                Age = age,
                PhoneType = phoneType,
                Work = work,
                Business = business,
                Source = source,
                DetailUrl = url,
                Address = address,
                RelativeUrls = relativeUrls,
                IsProcessed = true
            };

            // Store processed URL
            lock (_lockObject)
            {
                _processedUrls.Add(url);
            }

            return person;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing detail URL: {url}");
            return null;
        }
    }

    public async Task RunFullProcessAsync(string phoneListPath, string outputDir)
    {
        try
        {
            Console.WriteLine("Starting full FastPeopleSearch scraping process");
            
            // Create output directory if it doesn't exist - async operation
            await Task.Run(() => Directory.CreateDirectory(outputDir));
            
            // Stage 1: Phone numbers to names and detail URLs
            var phoneNumbers = await _csvService.ReadPhoneListAsync(phoneListPath);
            var stage1Results = await RunStage1Async(phoneNumbers);
            
            var stage1OutputPath = Path.Combine(outputDir, "output_stage1.csv");
            await _csvService.WriteStage1ResultsAsync(stage1Results, stage1OutputPath);
            
            // Stage 2: Detail URLs to addresses and relatives
            var (people, relations) = await RunStage2Async(stage1Results);
            
            // Stage 3: Additional people from relatives
            var stage3People = await RunStage3Async(relations);
            
            // Combine all people for the main people.csv
            var allPeople = people.Concat(stage3People).ToList();
            
            var peopleOutputPath = Path.Combine(outputDir, "people.csv");
            var relationsOutputPath = Path.Combine(outputDir, "relations.csv");
            var stage3OutputPath = Path.Combine(outputDir, "output_stage2.csv");
            
            await _csvService.WritePeopleAsync(allPeople, peopleOutputPath);
            await _csvService.WriteRelationsAsync(relations, relationsOutputPath);
            await _csvService.WritePeopleAsync(stage3People, stage3OutputPath);
            
            Console.WriteLine($"Process completed successfully!");
            Console.WriteLine($"Output files:");
            Console.WriteLine($"  - Stage 1: {stage1OutputPath}");
            Console.WriteLine($"  - People (All): {peopleOutputPath} ({allPeople.Count} records)");
            Console.WriteLine($"  - Relations: {relationsOutputPath} ({relations.Count} records)");
            Console.WriteLine($"  - Stage 2 (Relatives only): {stage3OutputPath} ({stage3People.Count} records)");
            
            // Log scraping metrics
            _scraperService.LogMetrics();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in full process: {ex.Message}");
            throw;
        }
    }
} 