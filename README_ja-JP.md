# DataTransformer
![ICON](https://raw.githubusercontent.com/ZjzMisaka/DataTransformer/main/DataTransformer/DataTransformer.ico)  
事前に作成したc#スクリプトを実行することにより、CSVの一括読み取り、分析、出力操作を行う。  
[中文ReadMe](README_zh-CN.md) | [English ReadMe](README.md)  

### 多言語
- [x] 简体中文
- [x] 日本語
- [x] English

### メインインターフェース
- ドロップダウンボックスから検索情報と処理ロジック（プラグイン）を選択し、1つずつ対応させます。 
- 着信パラメータの設定、検索ルートディレクトリとデフォルトの出力ディレクトリ、出力ファイル名の設定が可能です。
- 実行モードには、順次実行と同時実行の2つがあります。 
- 上記のコンテンツは、ルールのドロップダウンボックスから保存されたルールを選択することで自動的に入力できます。
    - ルールを選択した後、いくつかのフォルダとファイルを監視するように監視を設定し、変更があったときにこのルールを自動的に実行できます。 

### 検索情報設定インターフェース
**指定されたパスで指定されたCSVファイルを設定するために使用されます**
- 検索方法は、すべて、完全一致、部分包含、正規表現を選択できます
- CSV入力の形式を設定する
    - ヘッダーリスト
        - 空白または実際の列数と一致しない場合、indexをスクリプト入力パラメータのキーとして使用します
        - 実際の列数と一致する場合、実際の列名をスクリプト入力パラメータのキーとして使用します。ただし、ヘッダーリストに空白が含まれている場合、対応する列はindexをスクリプト入力パラメータのキーとして使用します
    - 列の区切り文字
    - ダブルクォートを使用するかどうか
    - ヘッダー行を表示するかどうか

### 処理ロジック（プラグインコーディング）インターフェース
**特定の種類のCSVファイルの処理ロジックと処理後の出力ロジックを設定するために使用されます**
- エディターでコードを書くと、操作中に順番に実行されます。 
- パラメータを設定でき、プラグインユーザーはメインインターフェイスでパラメータを編集し、実行時に使用するコードにそれらを渡すことができます。 
- パラメータ記述と実行中のlog出力は多言語化可能。 
- CSV出力の形式を設定する
    - ヘッダーリスト
    - 列の区切り文字
    - ダブルクォートを使用するかどうか
    - ヘッダー行を表示するかどうか
#### コーディング関連
- プロセス全体の自動完了と色付け、dllファイルを自分でDllsフォルダーに追加でき、追加後に直接参照できます。 
- 追加の提供された関数とプロパティを使用して、オンザフライで実行できます。 
    - メインインターフェイスのログ領域にあるログのリアルタイム出力。 
    - ハングして待って、ユーザー入力を読む。 
    - 追加のExcelファイル操作処理。 
- コンパイルエラーまたは実行エラーが発生すると、関連するデバッグ情報がメインインターフェイスの下部にあるログ領域に表示されます。 

##### GlobalDic (静的クラス)
```c#
// ---- セーブデータ ----
// セーブデータ. 
void SetObj(string key, object value);
// キーが存在するかどうかを確認します. 
bool ContainsKey(string key)
// データを取得する. 
object GetObj(string key)
// すべてのデータをリセット.
void Reset()
```

##### Logger (静的クラス)
```c#
// ---- 出力ログ機能 ----
// 出力ログの種類に応じて、色の違いが異なります. 
void Info(string info);
void Warn(string warn);
void Error(string error);
void Print(string str);

// ---- 関数が見つからない場合に警告するかどうか ----
bool IsOutputMethodNotFoundWarning { get => isOutputMethodNotFoundWarning; set => isOutputMethodNotFoundWarning = value; }
```

##### Scanner (静的クラス)
```c#
// ---- 入力を取得 ----
// パラメータは入力を取得するためのプロンプトであり、実行後、ユーザーが入力するまで待機します. 
// 入力を待機している他のスレッドがある場合、最初に前のスレッドが入力を取得するのを待ってから、このステートメントの内容を実行します.  
string GetInput();
string GetInput(string value);

// ---- 入力を待ち ----
// おそらく役に立たない機能です。他のスレッドが入力を実行している間、ユーザー入力が取得されるまで待つことができます。
// 最新のユーザー入力を返します。 
string WaitInput();

// ---- 最近の入力内容 ----
string LastInputValue { get => lastInputValue; set => lastInputValue = value; }
```

##### Outputter (クラス)
```c#
// ---- CSVファイル操作 ----
// 出力設定
CsvOption csvOption;
// 出力するデータ
IEnumerable<IEnumerable<string>> CsvDatas
// データ行を追加する
void SetData(IEnumerable<string> data)
// ヘッダー-データの辞書に基づいてデータ行を追加する
void SetData(Dictionary<string, string> dataWithHeader)
```

##### Param (クラス)
```c#
// パラメータを取得する
List<string> Get(string key);
string GetOne(string key);
// パラメータキーのコレクションを取得する
IEnumerable<String> GetKeys();
// パラメータが含まれているかどうかを確認する
bool ContainsKey(string key);
```

##### Running (静的クラス)
```c#
// ---- 実行状況 ----
// 時間のかかるロジックを実行する場合、ユーザーが実行を停止したかどうかを判断して、時間内にロジックを終了することができます。
bool UserStop { get => userStop; set => userStop = value; }
// 現在のロジックが実行されているかどうかを判断します。
bool NowRunning { get => nowRunning; set => nowRunning = value; }
```

##### RunBeforeAnalyzeCsv関数
|引数|型|説明|備考|
|----|----|----|----|
|param|Param|パラメータを渡す||
|globalObjects|Object|グローバルに存在し、他の呼び出しで使用するデータを保存できます、例えば現在の行番号など||
|allFilePathList|List\<string>|処理されるすべてのファイルパスのリスト||
|globalizationSetter|GlobalizationSetter|国際化文字列を取得|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|順序に実行するかどうか||
|outputter|Outputter|CSVデータを出力するためのもの||

##### AnalyzePerRecord関数
|引数|型|説明|備考|
|----|----|----|----|
|param|Param|パラメータを渡す||
|record|Dictionary\<string, string>|現在処理されているシート||
|filePath|string|ファイルパス||
|globalObjects|Object|グローバルに存在し、他の呼び出しで使用するデータを保存できます、例えば現在の行番号など||
|globalizationSetter|GlobalizationSetter|国際化文字列を取得|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|順序に実行するかどうか||
|invokeCount|int|この処理関数が呼び出された回数|初めて呼び出されたときの値は1|
|outputter|Outputter|CSVデータを出力するためのもの||

##### AnalyzeRecords関数
|引数|型|説明|備考|
|----|----|----|----|
|param|Param|パラメータを渡す||
|records|IEnumerable\<IEnumerable\<string>>|現在処理されているシート||
|filePath|string|ファイルパス||
|globalObjects|Object|グローバルに存在し、他の呼び出しで使用するデータを保存できます、例えば現在の行番号など||
|globalizationSetter|GlobalizationSetter|国際化文字列を取得|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|順序に実行するかどうか||
|invokeCount|int|この処理関数が呼び出された回数|初めて呼び出されたときの値は1|
|outputter|Outputter|CSVデータを出力するためのもの||

##### RunEnd関数
|引数|型|説明|備考|
|----|----|----|----|
|param|Param|パラメータを渡す||
|globalObjects|Object|グローバルに存在し、他の呼び出しで使用するデータを保存できます、例えば現在の行番号など||
|allFilePathList|List\<string>|処理されたすべてのファイルパスのリスト||
|globalizationSetter|GlobalizationSetter|国際化文字列を取得|globalizationSetter.Find("Code");|
|isExecuteInSequence|bool|順序に実行するかどうか||
|outputter|Outputter|CSVデータを出力するためのもの||

# 使用されるオープンソースライブラリ
|オープンソースライブラリ|オープンソースプロトコル|
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