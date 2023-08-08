# DataTransformer
![ICON](https://raw.githubusercontent.com/ZjzMisaka/DataTransformer/main/DataTransformer/DataTransformer.ico)  
Perform batch reading, analysis, and output operations of CSV by executing pre-written c# scripts.  
[中文ReadMe](README_zh-CN.md) | [日本語ReadMe](README_ja-JP.md)  

### Multi-language
- [x] 简体中文
- [x] 日本語
- [x] English

### Main interface
- Select retrieval information and processing logic (plug-in) through the drop-down box, make sure they are correspond one by one. 
- Can set incoming parameters, set search root directory and default output directory, output file name. 
- There are two execution modes: sequential execution and simultaneous execution. 
- The above content can be automatically filled in by selecting a saved rule from the rule drop-down box. 
    - After selecting a rule, you can set monitoring to monitor some folders and files, and automatically execute this rule when there is a change. 

### Search information setting interface
**Used to find the specified CSV file in the specified path**
- The search method can be selected all, complete match, partial contain and regular expression. 
- Set the format of the input CSV
    - Header list
        - If it is blank or the quantity does not match the actual number of columns, use the index as the key for the script input parameters
        - If the quantity matches the actual number of columns, use the actual column name as the key for the script input parameters. However, if the header list contains blanks, use the index as the key for the corresponding column's script input parameters
    - Column separator
    - Whether to use double quotes
    - Whether to display the header row
    - Encoding of input file

### Processing logic (plug-in coding) interface
**Used to set the processing logic for a certain type of CSV file and the output logic after processing**
- Write code in the editor, and it will be executed in sequence during operation.
- Parameters can be set, plug-in users can edit the parameters in the main interface, and pass them to the code to use at runtime.  
- Parameter description and running log output can be multilingualized. 
- Set the format of the output CSV
    - Header list
    - Column separator
    - Whether to use double quotes
    - Whether to display the header row
    - Encoding of output file

#### Coding related
- Automatic completion and coloring throughout the process, you can add dll files to the Dlls folder by yourself, and you can directly reference them after adding. 
- Can use additional provided functions and properties to perform on-the-fly. 
    - Real-time output of Log in the Log area of the main interface. 
    - Hang and wait, read user input. 
    - Additional Excel file operations. 
- When a compilation error or a running error occurs, the relevant debugging information will appear in the log area at the bottom of the main interface. 

##### GlobalDic (static class)
```c#
// ---- Save data function ----
// Save data.
void SetObj(string key, object value);
// Check if key is exist.
bool ContainsKey(string key)
// Get data.
object GetObj(string key)
// Reset all data.
void Reset()
```

##### Logger (static class)
```c#
// ---- Output Log function ----
// Depending on the output log type, there will be different coloring distinctions.
void Info(string info);
void Warn(string warn);
void Error(string error);
void Print(string str);

// ---- Whether to warn when a function is not found ----
bool IsOutputMethodNotFoundWarning { get => isOutputMethodNotFoundWarning; set => isOutputMethodNotFoundWarning = value; }
```

##### Scanner (static class)
```c#
// ---- Get the input function ----
// The parameter is the prompt to get the input, and after execution, it will wait until the user enters it.  
// If there are other threads waiting for input, it will first wait for the thread in front to get it, and then execute the content of this statement.  
string GetInput();
string GetInput(string value);

// ---- Waiting for input function ----
// Possibly a useless function. Can wait until user input is obtained while other threads are performing input. 
// Returns the most recent user input. 
string WaitInput();

// ---- Recently entered content ----
string LastInputValue { get => lastInputValue; set => lastInputValue = value; }
```

##### Outputter (class)
```c#
// ---- CSV file operations ----
// Output settings
CsvOption csvOption;
// Data to be output
IEnumerable<IEnumerable<string>> CsvDatas
// Add a row of data
void SetData(IEnumerable<string> data)
// Add a row of data based on a dictionary of headers-data
void SetData(Dictionary<string, string> dataWithHeader)
```

##### Param (class)
```c#
// Get parameters
List<string> Get(string key);
string GetOne(string key);
// Get a collection of parameter keys
IEnumerable<String> GetKeys();
// Check whether parameter is included
bool ContainsKey(string key);
```

##### Running (static class)
```c#
// ---- operating status ----
// When executing time-consuming logic, it can be judged whether the user has stopped running, so as to exit the logic in time.
bool UserStop { get => userStop; set => userStop = value; }
// Determine whether the current logic is being executed. 
bool NowRunning { get => nowRunning; set => nowRunning = value; }
```

##### RunBeforeAnalyzeCsv Function
|Parameter|Type|Description|Remarks|
|----|----|----|----|
|param|Param|The parameter passed in||
|globalObjects|Object|Globally existing, can save data that needs to be used in other calls, such as the current line number, etc.||
|allFilePathList|List\<string>|The list of all file paths that will be processed||
|globalizationSetter|GlobalizationSetter|Get internationalized strings|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|Whether to execute in sequence||
|outputter|Outputter|Used to output CSV data||

##### AnalyzePerRecord Function
|Parameter|Type|Description|Remarks|
|----|----|----|----|
|param|Param|The parameter passed in||
|record|Dictionary\<string, string>|The sheet currently being processed||
|filePath|string|File path||
|globalObjects|Object|Globally existing, can save data that needs to be used in other calls, such as the current line number, etc.||
|globalizationSetter|GlobalizationSetter|Get internationalized strings|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|Whether to execute in sequence||
|invokeCount|int|The number of times this processing function has been called|Value is 1 when called for the first time|
|outputter|Outputter|Used to output CSV data||

##### AnalyzeRecords Function
|Parameter|Type|Description|Remarks|
|----|----|----|----|
|param|Param|The parameter passed in||
|records|IEnumerable\<IEnumerable\<string>>|The sheet currently being processed||
|filePath|string|File path||
|globalObjects|Object|Globally existing, can save data that needs to be used in other calls, such as the current line number, etc.||
|globalizationSetter|GlobalizationSetter|Get internationalized strings|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|Whether to execute in sequence||
|invokeCount|int|The number of times this processing function has been called|Value is 1 when called for the first time|
|outputter|Outputter|Used to output CSV data||

##### RunEnd Function
|Parameter|Type|Description|Remarks|
|----|----|----|----|
|param|Param|The parameter passed in||
|globalObjects|Object|Globally existing, can save data that needs to be used in other calls, such as the current line number, etc.||
|allFilePathList|List\<string>|The list of all file paths processed||
|globalizationSetter|GlobalizationSetter|Get internationalized strings|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|Whether to execute in sequence||
|outputter|Outputter|Used to output CSV data||

# Open Source Libraries Used
|Open source library|Open source protocol|
|----|----|
|[roslynpad/roslynpad](https://github.com/roslynpad/roslynpad)|MIT|
|[icsharpcode/AvalonEdit](https://github.com/icsharpcode/AvalonEdit)|MIT|
|[JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)|MIT|
|[~~rickyah/ini-parser~~ rickyah/ini-parser-netstandard](https://github.com/rickyah/ini-parser)|MIT|
|[amibar/SmartThreadPool](https://github.com/amibar/SmartThreadPool)|MS-PL|
|[punker76/gong-wpf-dragdrop](https://github.com/punker76/gong-wpf-dragdrop)|BSD-3-Clause|
|[Kinnara/ModernWpf](https://github.com/Kinnara/ModernWpf)|MIT|
|[CommunityToolkit/WindowsCommunityToolkit (Microsoft.Toolkit.Mvvm)](https://github.com/CommunityToolkit/WindowsCommunityToolkit)|MIT|
|[microsoft/XamlBehaviorsWpf](https://github.com/microsoft/XamlBehaviorsWpf)|MIT|
|[ZjzMisaka/CustomizableMessageBox](https://github.com/ZjzMisaka/CustomizableMessageBox)|WTFPL|
|[ZjzMisaka/RoslynScriptRunner](https://github.com/ZjzMisaka/RoslynScriptRunner)|[OMSPL](https://github.com/ZjzMisaka/OMSPL/blob/main/LICENSE)|
