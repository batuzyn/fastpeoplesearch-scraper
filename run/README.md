# FastPeopleSearch Scraper - Ready to Run

## 🚀 Quick Start

This folder contains ready-to-run executables for both Windows and macOS.

### Windows Users
```cmd
# Open Command Prompt or PowerShell in this folder
FastPeopleSearchScraper-Windows.exe
```

### macOS Users
```bash
# Open Terminal in this folder
chmod +x FastPeopleSearchScraper-macOS
./FastPeopleSearchScraper-macOS
```

## 📁 Files Included

- **FastPeopleSearchScraper-Windows.exe** - Windows executable (self-contained)
- **FastPeopleSearchScraper-macOS** - macOS executable (self-contained)
- **phones.txt** - Sample phone numbers for testing (1,000+ entries)
- **appsettings.json** - Configuration file
- **README.md** - This instruction file

## ⚙️ Setup Steps

### 1. Get API Token
- Sign up at [scrape.do](https://scrape.do/)
- Copy your API token from dashboard

### 2. Prepare Phone Numbers
- Edit `phones.txt` or create your own
- One phone number per line (10 digits, no formatting)

### 3. Run the Tool
- Run the executable (see Quick Start above)
- **Enter your API token** when prompted
- **Enter concurrency level** (recommended: 50)
- Wait for processing to complete

### 4. Check Results
Results will be saved in a timestamped folder like `result_2025-05-30_15-33-11/`

## 💡 Tips

- **Concurrency**: Use 50 for optimal performance
- **Phone Format**: Use clean 10-digit US numbers
- **File Size**: Process 100-500 numbers at a time
- **Cost**: Monitor usage on scrape.do dashboard

## 🔧 Troubleshooting

- **Permission Denied (macOS)**: Run `chmod +x FastPeopleSearchScraper-macOS`
- **Token Invalid**: Double-check your API token from scrape.do dashboard
- **Slow Performance**: Reduce concurrency level
- **No .NET Required**: These are self-contained executables

## 📊 Expected Results

**Input**: 10 phone numbers  
**Output**: 300-500 total contacts  
**Time**: 1-3 minutes  
**Files**: 4 CSV files ready for Excel

---
**Ready to discover contact networks from phone numbers!** 