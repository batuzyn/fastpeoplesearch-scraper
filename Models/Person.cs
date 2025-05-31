using System.Collections.Generic;

namespace FastPeopleSearchScraper.Models;

public class Person
{
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; } = 0;
    public string PhoneType { get; set; } = string.Empty; // LandLine/Wireless
    public bool Work { get; set; } = false;
    public string Business { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    
    // Internal processing fields
    public string DetailUrl { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsProcessed { get; set; } = false;
    public List<string> RelativeUrls { get; set; } = new();
}

public class Stage1Result
{
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DetailUrl { get; set; } = string.Empty;
}

public class Relation
{
    public string SourceName { get; set; } = string.Empty;
    public string SourcePhone { get; set; } = string.Empty;
    public string RelativeUrl { get; set; } = string.Empty;
} 