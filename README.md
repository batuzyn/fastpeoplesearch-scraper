# FastPeopleSearch Scraper - Professional C# Contact Extraction Tool | Phone Number to Network Mapping

![FastPeopleSearch Scraper Banner](@banner_scrapedo.gif)

---

**🔍 Professional Contact Discovery & Lead Generation Tool** | Extract comprehensive contact networks from phone numbers using advanced C# scraping technology with FastPeopleSearch.com integration and Scrape.do proxy services.

**Keywords:** C# scraper, phone number lookup, contact extraction, lead generation, data mining, FastPeopleSearch automation, contact discovery, network mapping, Excel export, CRM integration

---

## 🌍 Language / 语言

**English** | [**中文 (Chinese)**](README_CN.md)

---

## 🚀 Key Features - Advanced Contact Intelligence Platform

- **📞 Phone-to-Contact Pipeline**: Transform phone numbers into complete contact profiles with family & business networks
- **⚡ High-Performance Scraping**: Configurable concurrent processing (1-200 requests) with enterprise-grade proxy integration  
- **📊 Business Intelligence Export**: Excel-ready CSV outputs for CRM systems, sales teams, and marketing automation
- **🎯 Lead Generation**: Discover extended networks from single phone numbers - expand your prospect database exponentially
- **🛡️ Enterprise-Ready**: Professional error handling, retry logic, rate limiting, and comprehensive logging
- **🌐 Global Compatibility**: Support for US phone number formats with international expansion capabilities

## 💼 Business Applications & Use Cases

### 🎯 Sales & Marketing Automation
- **Lead Generation**: Convert phone lists into comprehensive contact databases  
- **Prospect Research**: Discover decision-makers and their professional networks
- **CRM Enhancement**: Enrich existing contact data with family/business connections
- **Cold Outreach**: Build targeted prospect lists with verified contact information

### 📈 Market Research & Analytics  
- **Demographic Analysis**: Age distribution, employment status, business affiliations
- **Geographic Mapping**: Location-based contact clustering and market penetration
- **Network Analysis**: Relationship mapping for social media and influence marketing
- **Competitive Intelligence**: Contact discovery for market research and analysis

### 🏢 Enterprise Data Solutions
- **Customer Due Diligence**: Background verification and relationship mapping
- **Fraud Detection**: Cross-reference suspicious contacts with known networks  
- **Data Enrichment**: Enhance existing databases with additional contact points
- **Compliance Reporting**: Generate audit trails for contact data sourcing

## 📁 Project Structure

```
FastPeopleSearchScraper/
├── Models/
│   └── Person.cs                    # Data models (Person, Stage1Result, Relation)
├── Services/
│   ├── ScraperService.cs           # HTML fetching, parsing, and metrics collection
│   ├── CsvService.cs               # CSV read/write operations
│   └── FastPeopleSearchOrchestrator.cs # Main workflow management
├── Program.cs                      # Main entry point and CLI interface
├── appsettings.json               # Configuration file
├── phones.txt                     # Input phone numbers
├── FastPeopleSearchScraper.csproj # Project file
└── README.md
```

## 🛠️ Setup and Configuration

### Requirements

- .NET 8.0 SDK or higher
- Scrape.do account and API token
- Phone numbers list in plain text format

### Installation Steps

1. **Download/clone project files**

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Prepare phone numbers**
   
   Create `phones.txt` file (one number per line):
   ```
   2563904540
   2565047008
   2518025256
   ```

## 💻 Usage

### Basic Usage

```bash
# Build the project
dotnet build

# Run with interactive prompts
dotnet run

# Run with custom files
dotnet run phones.txt output_directory
```

### Interactive Configuration

When you run the program, you'll be prompted to configure:

1. **API Token**: Enter your scrape.do API token
   ```
   🔑 Enter your scrape.do API token: your_token_here
   ```

2. **Maximum Concurrent Requests**: Select between 1-200 (recommended: 50-100)
   ```
   🚀 Enter maximum concurrent requests (1-200) [default: 10]: 100
   ```

### Command Line Arguments

- `phones.txt`: Input phone numbers file (default: phones.txt)
- `output_directory`: Output folder (default: result_YYYY-MM-DD_HH-mm-ss)

## 📊 Output Files

The scraper generates four CSV files in the specified output folder (e.g., `result_2025-05-30_15-33-11/`):

### 1. `output_stage1.csv` - Phone-to-Profile Mapping 📞→👤
**Purpose**: Initial phone number lookup results  
**Use Cases**: Quick phone validation, contact discovery, lead verification  
```csv
Phone,Name,DetailUrl
2563904540,Miranda Cruz,https://www.fastpeoplesearch.com/miranda-cruz_id_G-1460032510038131659
2565047008,Alex Bowden,https://www.fastpeoplesearch.com/alex-bowden_id_G8632832918333662670
```
**Excel Analysis**: Sort by name, filter valid results, identify unmatched phones

### 2. `people.csv` - Complete Dataset 📊
**Purpose**: Master dataset with all discovered people (main + relatives)  
**Use Cases**: Comprehensive contact database, network analysis, CRM import  
```csv
phone,name,age,phonetype,work,business,source
2563904540,Miranda Cruz,28,Wireless,FALSE,,2563904540
,David Cruz,45,LandLine,TRUE,Construction,2563904540
```
**Excel Analysis**: Pivot tables by age groups, filter by work status, track source phone numbers

### 3. `output_stage2.csv` - Relatives Only 👨‍👩‍👧‍👦
**Purpose**: Extended network contacts (family, associates, connections)  
**Use Cases**: Family tree building, associate discovery, background research  
```csv
phone,name,age,phonetype,work,business,source
,David Cruz,45,LandLine,TRUE,Construction,2563904540
,Maria Cruz,42,Wireless,FALSE,,2563904540
```
**Excel Analysis**: Group by source phone, analyze age distributions, identify business connections

### 4. `relations.csv` - Relationship Mapping 🔗
**Purpose**: Source-to-relative URL mapping for relationship tracking  
**Use Cases**: Network visualization, relationship verification, data lineage  
```csv
Source Name,Source Phone,Relative URL
Miranda Cruz,2563904540,https://www.fastpeoplesearch.com/david-cruz_id_G123456789
Miranda Cruz,2563904540,https://www.fastpeoplesearch.com/maria-cruz_id_G987654321
```
**Excel Analysis**: Count relationships per person, identify highly connected individuals, track discovery paths

### 📈 Practical Applications

#### **Business Intelligence:**
- 📊 **Sales Leads**: Use `people.csv` for contact database expansion
- 🎯 **Market Research**: Analyze demographics in `output_stage2.csv`
- 🔍 **Due Diligence**: Cross-reference connections in `relations.csv`

#### **Data Analysis:**
- 📈 **Excel Pivot Tables**: Age groups, work status, phone types
- 📋 **CRM Integration**: Import `people.csv` into Salesforce, HubSpot
- 🗺️ **Network Mapping**: Visualize relationships using `relations.csv`

#### **Research & Investigation:**
- 🔍 **Background Checks**: Verify connections and family members
- 📞 **Contact Verification**: Validate phone numbers and identities
- 🌐 **Social Network Analysis**: Map relationship networks

## 🔄 Data Processing Workflow

### Stage 1: Phone Number Processing 📞→👤
**Input**: List of phone numbers from `phones.txt`  
**Output**: Names and profile URLs  
**Process**:
1. Load phone numbers from `phones.txt` file
2. For each phone number:
   - Build URL: `https://www.fastpeoplesearch.com/{phone}`
   - Fetch HTML via scrape.do API
   - Extract person name from `<title>` tag
   - Extract detailed profile URL from page links
3. Export results to `output_stage1.csv`

**Example**: `2563904540` → `Miranda Cruz` + `https://www.fastpeoplesearch.com/miranda-cruz_id_G123`

### Stage 2: Profile Detail Processing 👤→📋
**Input**: Profile URLs from Stage 1  
**Output**: Complete person details + relative URLs  
**Process**:
1. Process each profile URL from Stage 1
2. For each profile page:
   - Extract detailed information (age, phone type, work, business, address)
   - Extract relative URLs (family members, associates)
   - Build relationship mapping
3. Add main person data to dataset
4. Create relations.csv with relative connections

**Example**: `Miranda Cruz` profile → Age: 28, Work: FALSE, Relatives: David Cruz, Maria Cruz

### Stage 3: Relatives Processing (Non-Recursive) 👨‍👩‍👧‍👦→📊
**Input**: Relative URLs from Stage 2  
**Output**: Extended network of people  
**Process**:
1. For each unique relative URL found in Stage 2:
   - Extract person details (same as Stage 2)
   - **Do NOT process relatives of relatives** (prevents infinite expansion)
   - Track source phone number for data lineage
2. Export all datasets to respective CSV files

**Example**: David Cruz, Maria Cruz profiles → Additional family members without their relatives

### 🎯 Network Expansion Result
**1 Phone Number** → **50+ Connected People**
- ✅ Main person from phone lookup
- ✅ Immediate family members  
- ✅ Associates and connections
- ✅ Complete relationship mapping
- ❌ No infinite recursion (controlled expansion)

## ⚙️ Configuration Options

### `appsettings.json` Settings

```json
{
  "ScrapeDo": {
    "Token": "YOUR_SCRAPE_DO_TOKEN_HERE",
    "BaseUrl": "http://api.scrape.do"
  },
  "FastPeopleSearch": {
    "BaseUrl": "https://www.fastpeoplesearch.com"
  },
  "Settings": {
    "DelayBetweenRequests": 1000,    // Delay between requests (ms)
    "MaxRetries": 3                  // Retry attempts for failed requests
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## 📈 Performance Metrics

The program provides detailed metrics at completion:

```
=== SCRAPING METRICS ===
Total Requests: 524
Successful Requests: 524 (100.0%)
Failed Requests: 0 (0.0%)
Response Times (ms):
  - Average: 1978 ms
  - Minimum: 1132 ms
  - Maximum: 6097 ms
========================

=== TOTAL ELAPSED TIME: 0.44 minutes (26.6 seconds) ===
```

### Real-Time Logging

Detailed logging for each request:
```
Fetching URL (attempt 1): https://www.fastpeoplesearch.com/miranda-cruz_id_G123 (1,234ms)
Fetching URL (attempt 1): https://www.fastpeoplesearch.com/alex-bowden_id_G456 (2,156ms)
Starting Stage 1 - Processing 10 phone numbers
Stage 1 completed - Found 10 results
Starting Stage 2 - Processing 10 detail URLs
Stage 2 completed - Found 89 total people (10 main + 79 relatives)
Starting Stage 3 - Processing 79 relative URLs
Stage 3 completed - Processed 79 relatives
```

## 🛡️ Error Handling and Resilience

- **Retry Logic**: Automatic retry with exponential backoff for failed requests
- **Rate Limiting**: Configurable delays to respect server limits
- **Duplicate Detection**: Prevents processing the same URL multiple times
- **Graceful Failures**: Continues processing even if individual requests fail
- **Comprehensive Metrics**: Detailed performance tracking for monitoring and debugging

## 🚀 Performance Recommendations

### Recommended Settings
- **Concurrent Requests**: 50 for optimal performance
- **Delay**: 1000ms+ to prevent rate limiting
- **High Performance**: 100+ concurrent requests for 2-3 requests per second

### Scaling Tips
- Monitor response times and adjust concurrency accordingly
- Increase delays if encountering rate limiting (HTTP 429 errors)
- Use higher-tier scrape.do plans for better performance and reliability

## 🔧 Technical Implementation

### Core Technologies
- **HtmlAgilityPack**: HTML parsing and data extraction
- **CsvHelper**: Robust CSV file handling
- **Microsoft.Extensions**: Dependency injection, logging, and configuration
- **System.Diagnostics**: Performance measurement and stopwatch
- **Concurrent Collections**: Thread-safe data structures

### Data Format Optimizations
- **Phone Numbers**: Dashes removed, digits only (2563904540)
- **Source Tracking**: Each data entry tracks its originating phone number
- **Excel Compatibility**: CSV files can be opened directly in Excel
- **Encoding**: UTF-8 support for international characters

## 📋 Test Results

**Latest Test Data (10 Phone Numbers)**:
- **Total Time**: 26.6 seconds (with 100 concurrent requests)
- **Total Requests**: 524 requests
- **Success Rate**: 100%
- **Average Response Time**: 1,978ms
- **People Found**: 524 total (10 main + 514 relatives)
- **Relationship Map**: 514 relative connections

## 🤝 Contributing

This project is under active development. Bug reports and improvement suggestions are welcome.

## ⚠️ Legal Notice

This tool is developed for educational and research purposes only. Please check the terms of service of relevant websites and local laws before use. Users are responsible for any legal consequences arising from the use of this tool.

## 🚀 Quick Start

1. **Get API Token**: Sign up at [scrape.do](https://scrape.do/) and get your API token
2. **Prepare Phone Numbers**: Add phone numbers to `phones.txt` (one per line, 10 digits)
3. **Run the Tool**:
   ```bash
   dotnet run
   ```
   - Enter your scrape.do API token when prompted
   - Enter concurrency level (recommended: 50-100)
   - Wait for processing to complete

4. **Check Results**: Find output CSV files in the timestamped `result_YYYY-MM-DD_HH-MM-SS/` folder 