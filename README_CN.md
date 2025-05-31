# FastPeopleSearch 爬虫工具 - 专业C#联系人提取工具 | 电话号码网络映射系统

![FastPeopleSearch Scraper Banner](images/banner_scrapedo.gif)

---

**🔍 专业联系人发现与潜在客户生成工具** | 使用先进的C#爬虫技术，通过FastPeopleSearch.com集成和Scrape.do代理服务，从电话号码中提取全面的联系人网络。

**关键词：** C#爬虫, 电话号码查询, 联系人提取, 潜客生成, 数据挖掘, FastPeopleSearch自动化, 联系人发现, 网络映射, Excel导出, CRM集成, 销售线索, 客户数据库

---

## 🌍 Language / 语言

[**English**](README.md) | **中文 (Chinese)**

---

## 🚀 核心功能 - 高级联系人智能平台

- **📞 电话到联系人管道**: 将电话号码转换为完整的联系人档案，包含家庭和商业网络
- **⚡ 高性能爬取**: 可配置并发处理（1-200请求），企业级代理集成
- **📊 商业智能导出**: Excel就绪的CSV输出，适用于CRM系统、销售团队和营销自动化
- **🎯 潜客生成**: 从单个电话号码发现扩展网络 - 指数级扩展您的潜在客户数据库
- **🛡️ 企业就绪**: 专业错误处理、重试逻辑、速率限制和全面日志记录
- **🌐 全球兼容性**: 支持美国电话号码格式，具备国际扩展能力

## 💼 商业应用与使用场景

### 🎯 销售与营销自动化
- **潜客生成**: 将电话列表转换为全面的联系人数据库
- **客户研究**: 发现决策者及其专业网络
- **CRM增强**: 用家庭/商业联系丰富现有联系人数据
- **冷联系**: 构建经过验证的联系信息的目标客户列表

### 📈 市场研究与分析
- **人口统计分析**: 年龄分布、就业状况、商业关联
- **地理映射**: 基于位置的联系人聚类和市场渗透
- **网络分析**: 社交媒体和影响力营销的关系映射
- **竞争情报**: 市场研究和分析的联系人发现

### 🏢 企业数据解决方案
- **客户尽职调查**: 背景验证和关系映射
- **欺诈检测**: 将可疑联系人与已知网络交叉引用
- **数据丰富**: 用额外的联系点增强现有数据库
- **合规报告**: 为联系人数据来源生成审计记录

## 🚀 功能特性

- **三阶段处理**：电话 → 姓名 → 详情 → 亲属
- **高性能**：可配置的并发请求数量（1-200）
- **实时指标**：请求时间、成功率和详细性能分析
- **清洁数据输出**：Excel兼容的CSV格式结构化数据
- **强大的错误处理**：重试逻辑、指数退避和速率限制
- **Scrape.do 集成**：专业代理服务确保可靠爬取

## 📁 项目结构

```
FastPeopleSearchScraper/
├── Models/
│   └── Person.cs                    # 数据模型（Person, Stage1Result, Relation）
├── Services/
│   ├── ScraperService.cs           # HTML获取、解析和指标收集
│   ├── CsvService.cs               # CSV读写操作
│   └── FastPeopleSearchOrchestrator.cs # 主要工作流管理
├── Program.cs                      # 主入口点和CLI界面
├── appsettings.json               # 配置文件
├── phones.txt                     # 输入电话号码
├── FastPeopleSearchScraper.csproj # 项目文件
└── README.md
```

## 🛠️ 安装和配置

### 系统要求

- .NET 8.0 SDK 或更高版本
- Scrape.do 账户和 API 令牌
- 纯文本格式的电话号码列表

### 安装步骤

1. **下载/克隆项目文件**

2. **安装依赖项**
   ```bash
   dotnet restore
   ```

3. **准备电话号码**
   
   创建 `phones.txt` 文件（每行一个号码）：
   ```
   2563904540
   2565047008
   2518025256
   ```

## 💻 使用方法

### 基本用法

```bash
# 构建项目
dotnet build

# 运行（交互式提示）
dotnet run

# 使用自定义文件运行
dotnet run phones.txt output_directory
```

### 交互式配置

运行程序时，您将被提示配置：

1. **API Token**: 输入您的 scrape.do API token
   ```
   🔑 Enter your scrape.do API token: your_token_here
   ```

2. **最大并发请求数**：选择 1-200 之间（推荐：50-100）
   ```
   🚀 Enter maximum concurrent requests (1-200) [default: 10]: 100
   ```

### 命令行参数

- `phones.txt`：输入电话号码文件（默认：phones.txt）
- `output_directory`：输出文件夹（默认：result_YYYY-MM-DD_HH-mm-ss）

## 📊 输出文件

爬虫在指定输出文件夹中生成四个CSV文件（例如：`result_2025-05-30_15-33-11/`）：

### 1. `output_stage1.csv` - 电话到档案映射 📞→👤
**目的**：初始电话号码查找结果  
**用途**：快速电话验证、联系人发现、线索验证  
```csv
Phone,Name,DetailUrl
2563904540,Miranda Cruz,https://www.fastpeoplesearch.com/miranda-cruz_id_G-1460032510038131659
2565047008,Alex Bowden,https://www.fastpeoplesearch.com/alex-bowden_id_G8632832918333662670
```
**Excel分析**：按姓名排序、筛选有效结果、识别未匹配的电话

### 2. `people.csv` - 完整数据集 📊
**目的**：包含所有发现人员的主数据集（主要+亲属）  
**用途**：综合联系人数据库、网络分析、CRM导入  
```csv
phone,name,age,phonetype,work,business,source
2563904540,Miranda Cruz,28,Wireless,FALSE,,2563904540
,David Cruz,45,LandLine,TRUE,Construction,2563904540
```
**Excel分析**：按年龄组创建数据透视表、按工作状态筛选、跟踪源电话号码

### 3. `output_stage2.csv` - 仅亲属数据 👨‍👩‍👧‍👦
**目的**：扩展网络联系人（家庭、关联人员、连接）  
**用途**：家谱构建、关联人员发现、背景调查  
```csv
phone,name,age,phonetype,work,business,source
,David Cruz,45,LandLine,TRUE,Construction,2563904540
,Maria Cruz,42,Wireless,FALSE,,2563904540
```
**Excel分析**：按源电话分组、分析年龄分布、识别商业连接

### 4. `relations.csv` - 关系映射 🔗
**目的**：源到亲属URL映射，用于关系跟踪  
**用途**：网络可视化、关系验证、数据溯源  
```csv
Source Name,Source Phone,Relative URL
Miranda Cruz,2563904540,https://www.fastpeoplesearch.com/david-cruz_id_G123456789
Miranda Cruz,2563904540,https://www.fastpeoplesearch.com/maria-cruz_id_G987654321
```
**Excel分析**：统计每人的关系数量、识别高连接度个人、跟踪发现路径

### 📈 实际应用

#### **商业智能：**
- 📊 **销售线索**：使用 `people.csv` 扩展联系人数据库
- 🎯 **市场研究**：分析 `output_stage2.csv` 中的人口统计
- 🔍 **尽职调查**：交叉引用 `relations.csv` 中的连接

#### **数据分析：**
- 📈 **Excel数据透视表**：年龄组、工作状态、电话类型
- 📋 **CRM集成**：将 `people.csv` 导入Salesforce、HubSpot
- 🗺️ **网络映射**：使用 `relations.csv` 可视化关系

#### **研究与调查：**
- 🔍 **背景调查**：验证连接和家庭成员
- 📞 **联系人验证**：验证电话号码和身份
- 🤝 **社交网络分析**：映射关系网络

## 🔄 数据处理工作流程

### 阶段1：电话号码处理 📞→👤
**输入**：`phones.txt` 文件中的电话号码列表  
**输出**：姓名和档案URL  
**流程**：
1. 从 `phones.txt` 文件加载电话号码
2. 对每个电话号码：
   - 构建URL：`https://www.fastpeoplesearch.com/{phone}`
   - 通过 scrape.do API 获取HTML
   - 从 `<title>` 标签提取人员姓名
   - 从页面链接提取详细档案URL
3. 将结果导出到 `output_stage1.csv`

**示例**：`2563904540` → `Miranda Cruz` + `https://www.fastpeoplesearch.com/miranda-cruz_id_G123`

### 阶段2：档案详情处理 👤→📋
**输入**：阶段1的档案URL  
**输出**：完整人员详情 + 亲属URL  
**流程**：
1. 处理阶段1中的每个档案URL
2. 对每个档案页面：
   - 提取详细信息（年龄、电话类型、工作、企业、地址）
   - 提取亲属URL（家庭成员、关联人员）
   - 构建关系映射
3. 将主要人员数据添加到数据集
4. 创建 relations.csv 文件记录亲属连接

**示例**：`Miranda Cruz` 档案 → 年龄：28，工作：否，亲属：David Cruz，Maria Cruz

### 阶段3：亲属处理（非递归） 👨‍👩‍👧‍👦→📊
**输入**：阶段2的亲属URL  
**输出**：扩展的人员网络  
**流程**：
1. 对阶段2中找到的每个唯一亲属URL：
   - 提取人员详情（与阶段2相同）
   - **不处理亲属的亲属**（防止无限扩展）
   - 跟踪源电话号码以维护数据谱系
2. 将所有数据集导出到相应的CSV文件

**示例**：David Cruz，Maria Cruz 档案 → 额外家庭成员，但不处理他们的亲属

### 🎯 网络扩展结果
**1个电话号码** → **50+个关联人员**
- ✅ 从电话查找获得的主要人员
- ✅ 直系家庭成员  
- ✅ 关联人员和连接
- ✅ 完整的关系映射
- ❌ 无无限递归（受控扩展）

## ⚙️ 配置选项

### `appsettings.json` 设置

```json
{
  "ScrapeDo": {
    "Token": "您的_SCRAPE_DO_令牌",
    "BaseUrl": "http://api.scrape.do"
  },
  "FastPeopleSearch": {
    "BaseUrl": "https://www.fastpeoplesearch.com"
  },
  "Settings": {
    "DelayBetweenRequests": 1000,    // 请求间延迟（毫秒）
    "MaxRetries": 3                  // 失败请求的重试次数
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## 📈 性能指标

程序完成时提供详细指标：

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

### 实时日志

每个请求的详细日志：
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

## 🛡️ 错误处理和恢复能力

- **重试逻辑**：失败请求的指数退避自动重试
- **速率限制**：可配置延迟以遵守服务器限制
- **重复检测**：防止多次处理相同URL
- **优雅失败**：即使个别请求失败也继续处理
- **全面指标**：详细的性能跟踪用于监控和调试

## 🚀 性能建议

### 推荐设置
- **并发请求**：50-100 以获得最佳平衡
- **延迟**：1000ms+ 以防止速率限制
- **高性能**：100+ 并发请求可达到每秒2-3个请求

### 扩展提示
- 监控响应时间并相应调整并发性
- 如果遇到速率限制（HTTP 429错误），增加延迟
- 使用更高级别的 scrape.do 计划以获得更好的性能和可靠性

## 🔧 技术实现

### 核心技术
- **HtmlAgilityPack**：HTML解析和数据提取
- **CsvHelper**：强大的CSV文件处理
- **Microsoft.Extensions**：依赖注入、日志记录和配置
- **System.Diagnostics**：性能测量和秒表
- **Concurrent Collections**：线程安全的数据结构

### 数据格式优化
- **电话号码**：移除破折号，仅数字（2563904540）
- **来源跟踪**：每个数据条目跟踪其原始电话号码
- **Excel兼容性**：CSV文件可直接在Excel中打开
- **编码**：UTF-8支持国际字符

## 📋 测试结果

**最新测试数据（10个电话号码）**：
- **总时间**：26.6秒（100个并发请求）
- **总请求数**：524个请求
- **成功率**：100%
- **平均响应时间**：1,978毫秒
- **找到的人员**：524总计（10个主要 + 514个亲属）
- **关系映射**：514个亲属连接

## 🤝 贡献

该项目正在积极开发中。欢迎错误报告和改进建议。

## ⚠️ 法律声明

此工具仅用于教育和研究目的。使用前请检查相关网站的服务条款和当地法律。用户对使用此工具可能产生的任何法律后果负责。 