# DataTransformer
![ICON](https://raw.githubusercontent.com/ZjzMisaka/DataTransformer/main/DataTransformer/DataTransformer.ico)  
通过执行预编写好的c#脚本, 进行CSV的批量读取, 分析, 输出操作.  
[English ReadMe](README.md) | [日本語ReadMe](README_ja-JP.md)  

### 多语言
- [x] 简体中文
- [x] 日本語
- [x] English

### 主界面
- 通过下拉框选择检索信息和处理逻辑 (插件), 使它们一一对应. 
- 可以设定传入参数, 设定检索根目录和默认输出目录, 输出文件名. 
- 有顺序执行与同时执行两种执行方式. 
- 通过规则下拉框选择保存过的规则可以自动填充以上内容. 
    - 选择规则后可以设定监视对一些文件夹和文件进行监视, 出现变动后自动执行这项规则. 

### 检索信息设定界面
**用于设定需要查找指定路径下指定的CSV文件**
- 查找的方式可以有选择全部, 完整匹配, 部分包含和正则表达式. 
- 设置输入CSV的格式
    - 标头列表
        - 如果为空或数量与实际列数不符, 则用index作为脚本输入参数的key
        - 如果数量与实际列数相符, 则用实际列名作为脚本输入参数的key, 如果标头列表中包含空白, 则将对应列用index作为脚本输入参数的key
    - 列分隔符
    - 是否使用双引号
    - 是否显示标头行
    - 读取文件编码

### 处理逻辑 (插件编码) 界面
**用于设定对某一类CSV文件进行的处理逻辑以及处理完毕后的输出逻辑**
- 在编辑器中编写代码, 运行中会依次执行.  
- 可以设定参数, 插件使用者可以在主界面中编辑参数, 并且在运行中传递给代码使用.  
- 参数描述与运行中log输出可以多语言化. 
- 设置输出CSV的格式
    - 标头列表
    - 列分隔符
    - 是否使用双引号
    - 是否显示标头行
    - 输出文件编码

#### 编码相关
- 全程自动补全与着色, 可以自行向Dlls文件夹添加dll文件, 添加后可以直接引用. 
- 可以使用额外提供的函数与属性, 在运行中进行. 
    - 实时在主界面Log区域输出Log. 
    - 挂起并等待, 读取用户输入. 
    - 额外的Excel文件操作. 
- 当产生编译错误或者运行错误时, 相关调试信息会出现在主界面最下方的log区域中. 

##### GlobalDic (静态类)
```c#
// ---- 保存数据 ----
// 保存数据
void SetObj(string key, object value);
// 判断存在
bool ContainsKey(string key)
// 获取数据
object GetObj(string key)
// 重置所有
void Reset()
```

##### Logger (静态类)
```c#
// ---- 输出Log函数 ----
// 根据输出log类型不同, 会有不同的着色区分. 
void Info(string info);
void Warn(string warn);
void Error(string error);
void Print(string str);

// ---- 当找不到函数时是否报出警告属性 ----
bool IsOutputMethodNotFoundWarning { get => isOutputMethodNotFoundWarning; set => isOutputMethodNotFoundWarning = value; }
```

##### Scanner (静态类)
```c#
// ---- 获取输入函数 ----
// 参数是获取输入的提示语, 执行后会等待直到用户进行输入. 
// 如果有其他线程正在等待输入中, 则会先等待排在前面的线程获取完毕, 再执行此语句的内容.  
string GetInput();
string GetInput(string value);

// ---- 等待输入函数 ----
// 可能是无用函数. 可以在其他线程正在执行输入时等待直到用户输入被获取. 
// 返回最近用户输入的内容. 
string WaitInput();

// ---- 最近输入内容属性 ----
string LastInputValue { get => lastInputValue; set => lastInputValue = value; }
```

##### Outputter (类)
```c#
// ---- CSV文件操作 ----
// 输出设置
CsvOption csvOption;
// 输出的数据
IEnumerable<IEnumerable<string>> CsvDatas
// 添加一行数据
void SetData(IEnumerable<string> data)
// 根据标头-数据的字典添加一行数据
void SetData(Dictionary<string, string> dataWithHeader)
// 将一行数据的List转换为标头-数据的字典 [需要正确设置输出设置中的标头列表]
Dictionary<string, string> ToDataWithHeaderDictionary(IEnumerable<string> rowData)
// 将标头-数据的字典转换为一行数据的List [需要正确设置输出设置中的标头列表]
IEnumerable<string> ToRowDataList(Dictionary<string, string> dataWithHeader)
```

##### Param (类)
```c#
// 获取参数
List<string> Get(string key);
string GetOne(string key);
// 获取参数键的集合
IEnumerable<String> GetKeys();
// 判断是否包含参数
bool ContainsKey(string key);
```

##### Running (静态类)
```c#
// ---- 运行状况 ----
// 在执行耗时逻辑时可以判断用户有没有停止运行, 以便及时退出逻辑. 
bool UserStop { get => userStop; set => userStop = value; }
// 判断当前逻辑是不是正在执行. 
bool NowRunning { get => nowRunning; set => nowRunning = value; }
```

##### RunBeforeAnalyzeCsv函数
|参数|类型|描述|备注|
|----|----|----|----|
|param|Param|传入的参数||
|globalObjects|Object|全局存在, 可以保存需要在其他调用时使用的数据, 如当前行号等||
|allFilePathList|List\<string>|将会处理的所有文件路径列表||
|globalizationSetter|GlobalizationSetter|获取国际化字符串|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|是否顺序执行||
|outputter|Outputter|用于输出CSV数据||

##### AnalyzePerRecord函数
|参数|类型|描述|备注|
|----|----|----|----|
|param|Param|传入的参数||
|record|Dictionary\<string, string>|当前被处理的sheet||
|filePath|string|文件路径||
|globalObjects|Object|全局存在, 可以保存需要在其他调用时使用的数据, 如当前行号等||
|globalizationSetter|GlobalizationSetter|获取国际化字符串|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|是否顺序执行||
|invokeCount|int|此处理函数被调用的次数|第一次调用时值为1|
|outputter|Outputter|用于输出CSV数据||

##### AnalyzeRecords函数
|参数|类型|描述|备注|
|----|----|----|----|
|param|Param|传入的参数||
|records|IEnumerable\<IEnumerable\<string>>|当前被处理的sheet||
|filePath|string|文件路径||
|globalObjects|Object|全局存在, 可以保存需要在其他调用时使用的数据, 如当前行号等||
|globalizationSetter|GlobalizationSetter|获取国际化字符串|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|是否顺序执行||
|invokeCount|int|此处理函数被调用的次数|第一次调用时值为1|
|outputter|Outputter|用于输出CSV数据||

##### RunEnd函数
|参数|类型|描述|备注|
|----|----|----|----|
|param|Param|传入的参数||
|globalObjects|Object|全局存在, 可以保存需要在其他调用时使用的数据, 如当前行号等||
|allFilePathList|List\<string>|处理的所有文件路径列表||
|globalizationSetter|GlobalizationSetter|获取国际化字符串|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|是否顺序执行||
|outputter|Outputter|用于输出CSV数据||

# 使用的开源库
|开源库|开源协议|
|----|----|
|[roslynpad/roslynpad](https://github.com/roslynpad/roslynpad)|MIT|
|[icsharpcode/AvalonEdit](https://github.com/icsharpcode/AvalonEdit)|MIT|
|[JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)|MIT|
|[~~rickyah/ini-parser~~ rickyah/ini-parser-netstandard](https://github.com/rickyah/ini-parser)|MIT|
|[amibar/SmartThreadPool](https://github.com/amibar/SmartThreadPool)|MS-PL|
|[punker76/gong-wpf-dragdrop](https://github.com/punker76/gong-wpf-dragdrop)|BSD-3-Clause|
|[Kinnara/ModernWpf](https://github.com/Kinnara/ModernWpf)|MIT|
|[~~Microsoft.Toolkit.Mvvm~~ CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)|MIT|
|[microsoft/XamlBehaviorsWpf](https://github.com/microsoft/XamlBehaviorsWpf)|MIT|
|[ZjzMisaka/CustomizableMessageBox](https://github.com/ZjzMisaka/CustomizableMessageBox)|WTFPL|
|[ZjzMisaka/DynamicScriptExecutor](https://github.com/ZjzMisaka/DynamicScriptExecutor)|[OMSPL](https://github.com/ZjzMisaka/OMSPL/blob/main/LICENSE)|