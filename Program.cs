using FastPeopleSearchScraper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FastPeopleSearchScraper;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== FastPeopleSearch Scraper ===");
        Console.WriteLine("🚀 Ready to extract contact networks from phone numbers!");
        Console.WriteLine();
        
        // Start total time measurement
        var totalStopwatch = Stopwatch.StartNew();

        try
        {
            // Build configuration
            var configuration = BuildConfiguration(args);
            
            // Get scrape.do token from user input
            Console.WriteLine("📋 Setup Required:");
            var scrapeDoToken = await GetScrapeDoTokenAsync(configuration);
            if (string.IsNullOrEmpty(scrapeDoToken))
            {
                Console.WriteLine("❌ Error: scrape.do token is required!");
                return;
            }

            // Get concurrency setting from user
            var maxConcurrency = await GetConcurrencyFromUserAsync();
            
            // Setup dependency injection
            var serviceProvider = BuildServiceProvider(configuration, maxConcurrency, scrapeDoToken);
            
            // Get main orchestrator service
            var orchestrator = serviceProvider.GetRequiredService<FastPeopleSearchOrchestrator>();
            
            // Determine paths
            var phoneListPath = args.Length > 0 ? args[0] : "phones.txt";
            
            // Create timestamped output directory
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var defaultOutputDir = $"result_{timestamp}";
            var outputDir = args.Length > 1 ? args[1] : defaultOutputDir;
            
            // Check if phone list file exists
            var fileExists = await Task.Run(() => File.Exists(phoneListPath));
            if (!fileExists)
            {
                Console.WriteLine($"Error: Phone list file not found: {phoneListPath}");
                Console.WriteLine("Usage: FastPeopleSearchScraper [phone_list.txt] [output_directory]");
                return;
            }

            Console.WriteLine($"📁 Phone list: {phoneListPath}");
            Console.WriteLine($"📂 Output directory: {outputDir}");
            Console.WriteLine($"🚀 Max concurrency: {maxConcurrency}");
            Console.WriteLine($"🔑 Scrape.do token: {scrapeDoToken.Substring(0, Math.Min(8, scrapeDoToken.Length))}...");
            Console.WriteLine();
            
            Console.WriteLine("🎯 Starting scraping process...");
            Console.WriteLine("⏱️  This may take a while depending on the number of phone numbers.");
            Console.WriteLine();

            // Run the full process
            await orchestrator.RunFullProcessAsync(phoneListPath, outputDir);
            
            // Stop total time measurement
            totalStopwatch.Stop();
            
            Console.WriteLine();
            Console.WriteLine("=== Scraping completed successfully! ===");
            Console.WriteLine($"=== TOTAL ELAPSED TIME: {totalStopwatch.Elapsed.TotalMinutes:F2} minutes ({totalStopwatch.Elapsed.TotalSeconds:F1} seconds) ===");
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"=== TOTAL ELAPSED TIME (with error): {totalStopwatch.Elapsed.TotalMinutes:F2} minutes ({totalStopwatch.Elapsed.TotalSeconds:F1} seconds) ===");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static IConfiguration BuildConfiguration(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        return builder.Build();
    }

    private static async Task<string> GetScrapeDoTokenAsync(IConfiguration configuration)
    {
        // Always ask user for token interactively
        Console.WriteLine("ℹ️  Sign up at https://scrape.do/ to get your API token");
        Console.WriteLine();
        
        while (true)
        {
            Console.Write("🔑 Enter your scrape.do API token: ");
            
            var input = await Task.Run(() => Console.ReadLine());
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"✅ Token accepted: {input.Substring(0, Math.Min(8, input.Length))}...");
                Console.WriteLine();
                return input.Trim();
            }
            
            Console.WriteLine("❌ Token cannot be empty. Please enter a valid token.");
            Console.WriteLine();
        }
    }

    private static async Task<int> GetConcurrencyFromUserAsync()
    {
        Console.WriteLine("⚙️  Performance Settings:");
        Console.WriteLine("   • Recommended: 50 for optimal scraping experience");
        Console.WriteLine();
        
        while (true)
        {
            Console.Write("🚀 Enter maximum concurrent requests (1-200) [default: 10]: ");
            
            // Console.ReadLine() async değil, ama Task.Run ile async context'e çevirebiliriz
            var input = await Task.Run(() => Console.ReadLine());
            
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✅ Using default concurrency: 10");
                Console.WriteLine();
                return 10;
            }
            
            if (int.TryParse(input, out int concurrency) && concurrency >= 1 && concurrency <= 200)
            {
                Console.WriteLine($"✅ Concurrency set to: {concurrency}");
                Console.WriteLine();
                return concurrency;
            }
            
            Console.WriteLine("❌ Please enter a valid number between 1 and 200.");
            Console.WriteLine();
        }
    }

    private static ServiceProvider BuildServiceProvider(IConfiguration configuration, int maxConcurrency, string scrapeDoToken)
    {
        var services = new ServiceCollection();
        
        // Configuration - add runtime token
        var configDict = new Dictionary<string, string?>
        {
            ["ScrapeDo:Token"] = scrapeDoToken
        };
        var runtimeConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .AddConfiguration(configuration)
            .Build();
            
        services.AddSingleton<IConfiguration>(runtimeConfig);
        
        // Logging - minimal setup
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning); // Only show warnings and errors
        });
        
        // HTTP Client
        services.AddSingleton<HttpClient>();
        
        // Services
        services.AddSingleton<ScraperService>(provider =>
        {
            var httpClient = provider.GetRequiredService<HttpClient>();
            var logger = provider.GetRequiredService<ILogger<ScraperService>>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            return new ScraperService(httpClient, logger, configuration, scrapeDoToken);
        });
        services.AddSingleton<CsvService>();
        services.AddSingleton<FastPeopleSearchOrchestrator>(provider =>
        {
            var scraperService = provider.GetRequiredService<ScraperService>();
            var csvService = provider.GetRequiredService<CsvService>();
            var logger = provider.GetRequiredService<ILogger<FastPeopleSearchOrchestrator>>();
            return new FastPeopleSearchOrchestrator(scraperService, csvService, logger, maxConcurrency);
        });
        
        return services.BuildServiceProvider();
    }
} 