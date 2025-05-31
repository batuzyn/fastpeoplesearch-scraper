using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Net.Http;
using System.Diagnostics;

namespace FastPeopleSearchScraper.Services;

public class ScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScraperService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _scrapeDoToken;
    private readonly string _scrapeDoBaseUrl;
    private readonly string _fastPeopleSearchBaseUrl;
    private readonly int _delayBetweenRequests;
    private readonly int _maxRetries;

    // Metrics
    private int _totalRequests = 0;
    private int _successfulRequests = 0;
    private int _failedRequests = 0;
    private readonly List<long> _responseTimes = new();
    private readonly object _metricsLock = new();

    public ScraperService(HttpClient httpClient, ILogger<ScraperService> logger, IConfiguration configuration, string scrapeDoToken)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _scrapeDoToken = scrapeDoToken;
        _scrapeDoBaseUrl = _configuration["ScrapeDo:BaseUrl"] ?? "http://api.scrape.do";
        _fastPeopleSearchBaseUrl = _configuration["FastPeopleSearch:BaseUrl"] ?? "https://www.fastpeoplesearch.com";
        _delayBetweenRequests = _configuration.GetValue<int>("Settings:DelayBetweenRequests", 1000);
        _maxRetries = _configuration.GetValue<int>("Settings:MaxRetries", 3);
    }

    public async Task<string?> FetchHtmlAsync(string url, string? progressInfo = null)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        var scrapeDoUrl = $"{_scrapeDoBaseUrl}?token={_scrapeDoToken}&super=true&url={encodedUrl}";

        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Increment total requests counter
                lock (_metricsLock)
                {
                    _totalRequests++;
                }
                
                var response = await _httpClient.GetAsync(scrapeDoUrl);
                
                stopwatch.Stop();
                
                // Check for permanent errors (4xx status codes) - don't retry these
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    var logMessage = string.IsNullOrEmpty(progressInfo) 
                        ? $"Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url} - PERMANENT ERROR: {response.StatusCode}"
                        : $"{progressInfo} - Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url} - PERMANENT ERROR: {response.StatusCode}";
                    Console.WriteLine(logMessage);
                    
                    // Record failed request metrics
                    lock (_metricsLock)
                    {
                        _failedRequests++;
                        _responseTimes.Add(stopwatch.ElapsedMilliseconds);
                    }
                    
                    return null; // Don't retry on permanent errors
                }
                
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync();
                
                // Log with response time and progress info
                var successLogMessage = string.IsNullOrEmpty(progressInfo) 
                    ? $"Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url}"
                    : $"{progressInfo} - Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url}";
                Console.WriteLine(successLogMessage);
                
                // Record successful request metrics
                lock (_metricsLock)
                {
                    _successfulRequests++;
                    _responseTimes.Add(stopwatch.ElapsedMilliseconds);
                }
                
                // Add delay between requests
                if (_delayBetweenRequests > 0)
                    await Task.Delay(_delayBetweenRequests);
                
                return html;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                
                // Log failed attempt with response time and progress info
                var failLogMessage = string.IsNullOrEmpty(progressInfo) 
                    ? $"Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url} - FAILED: {ex.Message}"
                    : $"{progressInfo} - Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url} - FAILED: {ex.Message}";
                Console.WriteLine(failLogMessage);
                
                // Record failed request metrics (only on final attempt)
                if (attempt == _maxRetries - 1)
                {
                    lock (_metricsLock)
                    {
                        _failedRequests++;
                        _responseTimes.Add(stopwatch.ElapsedMilliseconds);
                    }
                }
                
                _logger.LogError($"Error fetching URL {url} (attempt {attempt + 1}): {ex.Message}");
                
                if (attempt == _maxRetries - 1)
                    return null; // Don't throw, just return null
                
                // Exponential backoff for retryable errors
                await Task.Delay((attempt + 1) * 2000);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log failed attempt with progress info
                var generalFailLogMessage = string.IsNullOrEmpty(progressInfo) 
                    ? $"Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url} - FAILED: {ex.Message}"
                    : $"{progressInfo} - Fetching URL (attempt {attempt + 1}) ({stopwatch.ElapsedMilliseconds}ms): {url} - FAILED: {ex.Message}";
                Console.WriteLine(generalFailLogMessage);
                
                // Record failed request metrics (only on final attempt)
                if (attempt == _maxRetries - 1)
                {
                    lock (_metricsLock)
                    {
                        _failedRequests++;
                        _responseTimes.Add(stopwatch.ElapsedMilliseconds);
                    }
                }
                
                _logger.LogError($"Error fetching URL {url} (attempt {attempt + 1}): {ex.Message}");
                
                if (attempt == _maxRetries - 1)
                    return null; // Don't throw, just return null
                
                // Exponential backoff for other errors
                await Task.Delay((attempt + 1) * 2000);
            }
        }
        
        return null;
    }

    public (string name, string detailUrl) ParsePhonePage(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Extract name from title tag
        var titleNode = doc.DocumentNode.SelectSingleNode("//title");
        var name = string.Empty;
        
        if (titleNode != null)
        {
            var titleText = titleNode.InnerText;
            // Title format: "(314)660-5265 | Zachery Hill in Saint Louis, MO | Free Reverse Phone Lookup"
            var match = Regex.Match(titleText, @"\|\s*(.+?)\s+in\s+");
            if (match.Success)
            {
                name = match.Groups[1].Value.Trim();
            }
        }

        // Extract detail URL - look for profile links
        var detailUrl = string.Empty;
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
        
        if (linkNodes != null)
        {
            foreach (var link in linkNodes)
            {
                var href = link.GetAttributeValue("href", "");
                
                // Look for pattern like "/zachery-hill_id_G5586227592044970357"
                if (href.Contains("_id_G") && !href.StartsWith("http"))
                {
                    detailUrl = _fastPeopleSearchBaseUrl + href;
                    break;
                }
            }
        }

        return (name, detailUrl);
    }

    public (string name, int age, string phoneType, bool work, string business, string address, List<string> relativeUrls) ParseDetailPage(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var name = string.Empty;
        var age = 0;
        var phoneType = string.Empty;
        var work = false;
        var business = string.Empty;
        var address = string.Empty;
        var relativeUrls = new List<string>();

        // Extract name and age from title tag
        var titleNode = doc.DocumentNode.SelectSingleNode("//title");
        if (titleNode != null)
        {
            var titleText = titleNode.InnerText;
            // Title format: "Marcel Clay(72) Birmingham, AL (205)317-8011 | FastPeopleSearch"
            var match = Regex.Match(titleText, @"^([^(]+)\((\d+)\)");
            if (match.Success)
            {
                name = match.Groups[1].Value.Trim();
                if (int.TryParse(match.Groups[2].Value, out int parsedAge))
                {
                    age = parsedAge;
                }
            }
            else
            {
                // Fallback: just name without age
                var nameMatch = Regex.Match(titleText, @"^([^|]+)");
                if (nameMatch.Success)
                {
                    name = nameMatch.Groups[1].Value.Trim();
                }
            }
        }

        // Extract phone type from page content
        var phoneTypeNodes = doc.DocumentNode.SelectNodes("//text()[contains(., 'LandLine') or contains(., 'Wireless') or contains(., 'Mobile')]");
        if (phoneTypeNodes != null)
        {
            foreach (var node in phoneTypeNodes)
            {
                var text = node.InnerText.ToLower();
                if (text.Contains("wireless") || text.Contains("mobile"))
                {
                    phoneType = "Wireless";
                    break;
                }
                else if (text.Contains("landline"))
                {
                    phoneType = "LandLine";
                    break;
                }
            }
        }

        // Extract work/business information from page content
        var workNodes = doc.DocumentNode.SelectNodes("//text()[contains(., 'work') or contains(., 'business') or contains(., 'company') or contains(., 'employer')]");
        if (workNodes != null)
        {
            work = true;
            // Try to extract business/job title
            foreach (var node in workNodes)
            {
                var text = node.InnerText.Trim();
                if (text.Length > 0 && text.Length < 100) // Reasonable length for business title
                {
                    business = text;
                    break;
                }
            }
        }

        // Extract current address from FAQ section
        var addressNodes = doc.DocumentNode.SelectNodes("//p[contains(text(), 'current address is')]//a[contains(@href, '/address/')]");
        if (addressNodes != null && addressNodes.Count > 0)
        {
            var addressText = addressNodes[0].InnerText.Trim();
            address = addressText;
        }

        // Extract relatives URLs
        var relativeNodes = doc.DocumentNode.SelectNodes("//p[contains(text(), 'likely related to')]//a[contains(@href, '_id_')]");
        if (relativeNodes != null)
        {
            foreach (var relativeNode in relativeNodes)
            {
                var href = relativeNode.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href) && href.Contains("_id_"))
                {
                    var fullUrl = href.StartsWith("http") ? href : _fastPeopleSearchBaseUrl + href;
                    if (!relativeUrls.Contains(fullUrl))
                    {
                        relativeUrls.Add(fullUrl);
                    }
                }
            }
        }

        return (name, age, phoneType, work, business, address, relativeUrls);
    }

    public async Task<(string name, int age, string phoneType, bool work, string business, string address, List<string> relativeUrls)> GetDetailPageDataAsync(string detailUrl, string? progressInfo = null)
    {
        try
        {
            var html = await FetchHtmlAsync(detailUrl, progressInfo);
            if (string.IsNullOrEmpty(html))
            {
                _logger.LogWarning($"Failed to fetch detail page: {detailUrl}");
                return (string.Empty, 0, string.Empty, false, string.Empty, string.Empty, new List<string>());
            }

            return ParseDetailPage(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting detail page data for: {detailUrl}");
            return (string.Empty, 0, string.Empty, false, string.Empty, string.Empty, new List<string>());
        }
    }

    public void LogMetrics()
    {
        lock (_metricsLock)
        {
            if (_totalRequests == 0)
            {
                Console.WriteLine("=== SCRAPING METRICS ===");
                Console.WriteLine("No requests were made.");
                return;
            }

            var successRate = (_successfulRequests * 100.0) / _totalRequests;
            var failureRate = (_failedRequests * 100.0) / _totalRequests;
            
            var avgResponseTime = _responseTimes.Count > 0 ? _responseTimes.Average() : 0;
            var minResponseTime = _responseTimes.Count > 0 ? _responseTimes.Min() : 0;
            var maxResponseTime = _responseTimes.Count > 0 ? _responseTimes.Max() : 0;

            Console.WriteLine("=== SCRAPING METRICS ===");
            Console.WriteLine($"Total Requests: {_totalRequests}");
            Console.WriteLine($"Successful Requests: {_successfulRequests} ({successRate:F1}%)");
            Console.WriteLine($"Failed Requests: {_failedRequests} ({failureRate:F1}%)");
            Console.WriteLine($"Response Times (ms):");
            Console.WriteLine($"  - Average: {avgResponseTime:F0} ms");
            Console.WriteLine($"  - Minimum: {minResponseTime} ms");
            Console.WriteLine($"  - Maximum: {maxResponseTime} ms");
            Console.WriteLine("========================");
        }
    }
} 