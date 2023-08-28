using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvTool;
using CustomizableMessageBox;
using DataTransformer.Helper;
using DynamicScriptExecutor;
using GlobalObjects;
using GongSolutions.Wpf.DragDrop;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CodeAnalysis;
using ModernWpf;
using Newtonsoft.Json;
using PowerThreadPool;
using PowerThreadPool.Option;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using static CustomizableMessageBox.MessageBox;
using Border = System.Windows.Controls.Border;
using FileHelper = DataTransformer.Helper.FileHelper;

namespace DataTransformer.ViewModel
{
    [SupportedOSPlatform("windows7.0")]
    class MainWindowViewModel : ObservableObject, IDropTarget
    {
        private PowerPool powerPool;
        private enum ReadFileReturnType { ANALYZER, FILEPATH };
        private Thread runningThread;
        private Thread runBeforeAnalyzeCsvThread;
        private Thread runEndThread;
        private Thread fileSystemWatcherInvokeThread;
        private ConcurrentDictionary<string, long> currentAnalizingDictionary;
        private Dictionary<FileSystemWatcher, string> fileSystemWatcherDic;
        private Stopwatch stopwatchBeforeFileSystemWatcherInvoke;
        private int analyzeCsvInvokeCount;
        private int maxThreadCount;
        private bool enableTimeoutSetting;
        private int totalTimeoutLimitAnalyze;
        private int perTimeoutLimitAnalyze;
        private bool runNotSuccessed;
        private Scanner scanner;
        private int fileSystemWatcherInvokeDalay;
        private int freshInterval;
        private string language;
        private bool teParamsFocused;
        private bool windowsClosing;

        private TextEditor teLog = new TextEditor();
        public TextEditor TeLog => teLog;

        private TextEditor teParams = new TextEditor();
        public TextEditor TeParams => teParams;

        private Brush themeBackground;
        public Brush ThemeBackground
        {
            get { return themeBackground; }
            set
            {
                SetProperty<Brush>(ref themeBackground, value);
            }
        }

        private Brush themeControlBackground;
        public Brush ThemeControlBackground
        {
            get { return themeControlBackground; }
            set
            {
                SetProperty<Brush>(ref themeControlBackground, value);
            }
        }

        private Brush themeControlFocusBackground;
        public Brush ThemeControlFocusBackground
        {
            get { return themeControlFocusBackground; }
            set
            {
                SetProperty<Brush>(ref themeControlFocusBackground, value);
            }
        }

        private Brush themeControlForeground;
        public Brush ThemeControlForeground
        {
            get { return themeControlForeground; }
            set
            {
                SetProperty<Brush>(ref themeControlForeground, value);
            }
        }

        private Brush themeControlBorderBrush;
        public Brush ThemeControlBorderBrush
        {
            get { return themeControlBorderBrush; }
            set
            {
                SetProperty<Brush>(ref themeControlBorderBrush, value);
            }
        }

        private Brush themeControlHoverBorderBrush;
        public Brush ThemeControlHoverBorderBrush
        {
            get { return themeControlHoverBorderBrush; }
            set
            {
                SetProperty<Brush>(ref themeControlHoverBorderBrush, value);
            }
        }

        private double windowWidth;
        public double WindowWidth
        {
            get { return windowWidth; }
            set
            {
                SetProperty<double>(ref windowWidth, value);
            }
        }

        private double windowHeight;
        public double WindowHeight
        {
            get { return windowHeight; }
            set
            {
                SetProperty<double>(ref windowHeight, value);
            }
        }

        private string windowName = "MainWindow";
        public string WindowName
        {
            get { return windowName; }
            set
            {
                SetProperty<string>(ref windowName, value);
            }
        }

        private List<string> csvExplainersItems = new List<string>();
        public List<string> CsvExplainersItems
        {
            get { return csvExplainersItems; }
            set
            {
                SetProperty<List<string>>(ref csvExplainersItems, value);
            }
        }

        private string selectedCsvExplainersItem = null;
        public string SelectedCsvExplainersItem
        {
            get { return selectedCsvExplainersItem; }
            set
            {
                SetProperty<string>(ref selectedCsvExplainersItem, value);
            }
        }

        private int selectedCsvExplainersIndex = 0;
        public int SelectedCsvExplainersIndex
        {
            get { return selectedCsvExplainersIndex; }
            set
            {
                SetProperty<int>(ref selectedCsvExplainersIndex, value);
            }
        }

        private List<string> analyzersItems = new List<string>();
        public List<string> AnalyzersItems
        {
            get { return analyzersItems; }
            set
            {
                SetProperty<List<string>>(ref analyzersItems, value);
            }
        }

        private string selectedAnalyzersItem = null;
        public string SelectedAnalyzersItem
        {
            get { return selectedAnalyzersItem; }
            set
            {
                SetProperty<string>(ref selectedAnalyzersItem, value);
            }
        }

        private int selectedAnalyzersIndex = 0;
        public int SelectedAnalyzersIndex
        {
            get { return selectedAnalyzersIndex; }
            set
            {
                SetProperty<int>(ref selectedAnalyzersIndex, value);
            }
        }

        private ICSharpCode.AvalonEdit.Document.TextDocument teCsvExplainersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument();
        public ICSharpCode.AvalonEdit.Document.TextDocument TeCsvExplainersDocument
        {
            get => teCsvExplainersDocument;
            set
            {
                SetProperty<ICSharpCode.AvalonEdit.Document.TextDocument>(ref teCsvExplainersDocument, value);
            }
        }

        private ICSharpCode.AvalonEdit.Document.TextDocument teAnalyzersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument();
        public ICSharpCode.AvalonEdit.Document.TextDocument TeAnalyzersDocument
        {
            get => teAnalyzersDocument;
            set
            {
                SetProperty<ICSharpCode.AvalonEdit.Document.TextDocument>(ref teAnalyzersDocument, value);
            }
        }

        private bool cbExecuteInSequenceIsChecked = false;
        public bool CbExecuteInSequenceIsChecked
        {
            get => cbExecuteInSequenceIsChecked;
            set
            {
                SetProperty<bool>(ref cbExecuteInSequenceIsChecked, value);
            }
        }

        private List<string> paramsItems = new List<string>();
        public List<string> ParamsItems
        {
            get { return paramsItems; }
            set
            {
                SetProperty<List<string>>(ref paramsItems, value);
            }
        }

        private string selectedParamsItem = null;
        public string SelectedParamsItem
        {
            get { return selectedParamsItem; }
            set
            {
                SetProperty<string>(ref selectedParamsItem, value);
            }
        }

        private int selectedParamssIndex = 0;
        public int SelectedParamsIndex
        {
            get { return selectedParamssIndex; }
            set
            {
                SetProperty<int>(ref selectedParamssIndex, value);
            }
        }

        private Visibility btnLockVisibility = Visibility.Visible;
        public Visibility BtnLockVisibility
        {
            get { return btnLockVisibility; }
            set
            {
                SetProperty<Visibility>(ref btnLockVisibility, value);
            }
        }

        private Visibility btnUnlockVisibility = Visibility.Hidden;
        public Visibility BtnUnlockVisibility
        {
            get { return btnUnlockVisibility; }
            set
            {
                SetProperty<Visibility>(ref btnUnlockVisibility, value);
            }
        }

        private string tbBasePathText = null;
        public string TbBasePathText
        {
            get => tbBasePathText;
            set
            {
                SetProperty<string>(ref tbBasePathText, value);
            }
        }

        private string tbOutputPathText = null;
        public string TbOutputPathText
        {
            get => tbOutputPathText;
            set
            {
                SetProperty<string>(ref tbOutputPathText, value);
            }
        }

        private string tbOutputNameText = null;
        public string TbOutputNameText
        {
            get => tbOutputNameText;
            set
            {
                SetProperty<string>(ref tbOutputNameText, value);
            }
        }

        private List<string> ruleItems = null;
        public List<string> RuleItems
        {
            get { return ruleItems; }
            set
            {
                SetProperty<List<string>>(ref ruleItems, value);
            }
        }

        private string selectedRulesItem = null;
        public string SelectedRulesItem
        {
            get { return selectedRulesItem; }
            set
            {
                SetProperty<string>(ref selectedRulesItem, value);
            }
        }

        private int selectedRulesIndex = 0;
        public int SelectedRulesIndex
        {
            get { return selectedRulesIndex; }
            set
            {
                SetProperty<int>(ref selectedRulesIndex, value);
            }
        }

        private Visibility btnSaveRuleVisibility = Visibility.Hidden;
        public Visibility BtnSaveRuleVisibility
        {
            get { return btnSaveRuleVisibility; }
            set
            {
                SetProperty<Visibility>(ref btnSaveRuleVisibility, value);
            }
        }

        private Visibility btnDeleteRuleVisibility = Visibility.Visible;
        public Visibility BtnDeleteRuleVisibility
        {
            get { return btnDeleteRuleVisibility; }
            set
            {
                SetProperty<Visibility>(ref btnDeleteRuleVisibility, value);
            }
        }

        private bool btnDeleteRuleIsEnabled;
        public bool BtnDeleteRuleIsEnabled
        {
            get { return btnDeleteRuleIsEnabled; }
            set
            {
                SetProperty<bool>(ref btnDeleteRuleIsEnabled, value);
            }
        }

        private Visibility btnSetAutoVisibility = Visibility.Hidden;
        public Visibility BtnSetAutoVisibility
        {
            get { return btnSetAutoVisibility; }
            set
            {
                SetProperty<Visibility>(ref btnSetAutoVisibility, value);
            }
        }

        private Visibility btnUnsetAutoVisibility = Visibility.Hidden;
        public Visibility BtnUnsetAutoVisibility
        {
            get { return btnUnsetAutoVisibility; }
            set
            {
                SetProperty<Visibility>(ref btnUnsetAutoVisibility, value);
            }
        }

        private string tbStatusText = null;
        public string TbStatusText
        {
            get { return tbStatusText; }
            set
            {
                SetProperty<string>(ref tbStatusText, value);
            }
        }

        private string lProcessContent;
        public string LProcessContent
        {
            get { return lProcessContent; }
            set
            {
                SetProperty<string>(ref lProcessContent, value);
            }
        }

        private bool cbIsAutoOpenIsChecked;
        public bool CbIsAutoOpenIsChecked
        {
            get { return cbIsAutoOpenIsChecked; }
            set
            {
                SetProperty<bool>(ref cbIsAutoOpenIsChecked, value);
            }
        }

        private bool cbIsShowSavedMessageBox;
        public bool CbIsShowSavedMessageBox
        {
            get { return cbIsShowSavedMessageBox; }
            set
            {
                SetProperty<bool>(ref cbIsShowSavedMessageBox, value);
            }
        }

        private bool btnStopIsEnabled = false;
        public bool BtnStopIsEnabled
        {
            get { return btnStopIsEnabled; }
            set
            {
                SetProperty<bool>(ref btnStopIsEnabled, value);
            }
        }

        private bool btnStartIsEnabled = true;
        public bool BtnStartIsEnabled
        {
            get { return btnStartIsEnabled; }
            set
            {
                SetProperty<bool>(ref btnStartIsEnabled, value);
            }
        }

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand MenuOpenCommand { get; set; }
        public ICommand ChangeThemeCommand { get; set; }
        public ICommand ChangeLanguageCommand { get; set; }
        public ICommand MenuSetStrCommand { get; set; }
        public ICommand MenuSetCheckboxCommand { get; set; }
        public ICommand OpenSourceCodeUrlCommand { get; set; }
        public ICommand BtnOpenCsvExplainerEditorClickCommand { get; set; }
        public ICommand BtnOpenAnalyzerEditorClickCommand { get; set; }
        public ICommand CbCsvExplainersPreviewMouseLeftButtonDownCommand { get; set; }
        public ICommand CbCsvExplainersSelectionChangedCommand { get; set; }
        public ICommand CbAnalyzersPreviewMouseLeftButtonDownCommand { get; set; }
        public ICommand CbAnalyzersSelectionChangedCommand { get; set; }
        public ICommand TeCsvexplainersDropCommand { get; set; }
        public ICommand TeAnalyzersDropCommand { get; set; }
        public ICommand CbParamsPreviewMouseLeftButtonDownCommand { get; set; }
        public ICommand CbParamsSelectionChangedCommand { get; set; }
        public ICommand EditParamCommand { get; set; }
        public ICommand LockParamCommand { get; set; }
        public ICommand UnlockParamCommand { get; set; }
        public ICommand TbParamsLostFocusCommand { get; set; }
        public ICommand SelectPathCommand { get; set; }
        public ICommand OpenPathCommand { get; set; }
        public ICommand SelectNameCommand { get; set; }
        public ICommand OpenOutputCommand { get; set; }
        public ICommand CbRulesChangedCommand { get; set; }
        public ICommand CbRulesPreviewMouseLeftButtonDownCommand { get; set; }
        public ICommand SaveRuleCommand { get; set; }
        public ICommand DeleteRuleCommand { get; set; }
        public ICommand SetAutoCommand { get; set; }
        public ICommand UnsetAutoCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand StartCommand { get; set; }
        public MainWindowViewModel()
        {
            GlobalObjects.GlobalObjects.ProgramCurrentStatus = ProgramStatus.Default;

            ICSharpCode.AvalonEdit.Search.SearchPanel.Install(TeLog);

            currentAnalizingDictionary = new ConcurrentDictionary<string, long>();
            fileSystemWatcherDic = new Dictionary<FileSystemWatcher, string>();
            scanner = new Scanner();

            analyzeCsvInvokeCount = 0;

            Running.NowRunning = false;
            Running.UserStop = false;
            runNotSuccessed = false;

            scanner.Input += Scanner.UserInput;

            TeLog.Padding = new Thickness(5);
            TeLog.ShowLineNumbers = false;
            TeLog.WordWrap = true;
            TeLog.IsReadOnly = true;
            TeLog.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            TeLog.PreviewKeyDown += TeLogPreviewKeyDown;
            TeLog.TextChanged += TeLogTextChanged;
            teParamsFocused = false;
            windowsClosing = false;
            TeParams.ShowLineNumbers = false;
            TeParams.WordWrap = false;
            TeParams.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            TeParams.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            TeParams.TextChanged += TeParamsTextChanged;
            TeParams.GotFocus += TeParamsGotFocus;
            TeParams.LostFocus += TeParamsLostFocus;
            TeParams.MouseEnter += TeParamsMouseEnter;
            TeParams.MouseLeave += TeParamsMouseLeave;
            TeParams.PreviewKeyDown += TeParamsPreviewKeyDown;
            TeParams.Padding = new Thickness(8, 5, 8, 5);
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            MenuOpenCommand = new RelayCommand<object>(MenuOpen);
            ChangeThemeCommand = new RelayCommand(ChangeTheme);
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
            MenuSetStrCommand = new RelayCommand<object>(MenuSetStr);
            MenuSetCheckboxCommand = new RelayCommand<object>(MenuSetCheckbox);
            OpenSourceCodeUrlCommand = new RelayCommand(OpenSourceCodeUrl);
            BtnOpenCsvExplainerEditorClickCommand = new RelayCommand(BtnOpenCsvExplainerEditorClick);
            BtnOpenAnalyzerEditorClickCommand = new RelayCommand(BtnOpenAnalyzerEditorClick);
            CbCsvExplainersPreviewMouseLeftButtonDownCommand = new RelayCommand(CbCsvExplainersPreviewMouseLeftButtonDown);
            CbCsvExplainersSelectionChangedCommand = new RelayCommand(CbCsvExplainersSelectionChanged);
            CbAnalyzersPreviewMouseLeftButtonDownCommand = new RelayCommand(CbAnalyzersPreviewMouseLeftButtonDown);
            CbAnalyzersSelectionChangedCommand = new RelayCommand(CbAnalyzersSelectionChanged);
            TeCsvexplainersDropCommand = new RelayCommand<DragEventArgs>(TeCsvexplainersDrop);
            TeAnalyzersDropCommand = new RelayCommand<DragEventArgs>(TeAnalyzersDrop);
            CbParamsPreviewMouseLeftButtonDownCommand = new RelayCommand(CbParamsPreviewMouseLeftButtonDown);
            CbParamsSelectionChangedCommand = new RelayCommand(CbParamsSelectionChanged);
            EditParamCommand = new RelayCommand(EditParam);
            LockParamCommand = new RelayCommand(LockParam);
            UnlockParamCommand = new RelayCommand(UnlockParam);
            TbParamsLostFocusCommand = new RelayCommand(TbParamsLostFocus);
            SelectPathCommand = new RelayCommand<object>(SelectPath);
            OpenPathCommand = new RelayCommand<object>(OpenPath);
            SelectNameCommand = new RelayCommand(SelectName);
            OpenOutputCommand = new RelayCommand(OpenOutput);
            CbRulesChangedCommand = new RelayCommand(CbRulesChanged);
            CbRulesPreviewMouseLeftButtonDownCommand = new RelayCommand(CbRulesPreviewMouseLeftButtonDown);
            SaveRuleCommand = new RelayCommand(SaveRule);
            DeleteRuleCommand = new RelayCommand(DeleteRule);
            SetAutoCommand = new RelayCommand(SetAuto);
            UnsetAutoCommand = new RelayCommand(UnsetAuto);
            StopCommand = new RelayCommand(Stop);
            StartCommand = new RelayCommand(Start);

            ModernWpf.ThemeManager.Current.ActualApplicationThemeChanged += ActualApplicationThemeChanged;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            Window window = (Window)eventArgs.Source;

            language = IniHelper.GetLanguage();
            if (String.IsNullOrWhiteSpace(language))
            {
                language = Thread.CurrentThread.CurrentUICulture.Name;
            }
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            ChangeLanguage(language, dictionaryList);

            if (IniHelper.GetTheme() == "Light")
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else if(IniHelper.GetTheme() == "Dark")
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
            ActualApplicationThemeChanged(null, null);

            IniHelper.CheckAndCreateIniFile();

            double width = IniHelper.GetWindowSize(WindowName).X;
            double height = IniHelper.GetWindowSize(WindowName).Y;
            if (width > 0)
            {
                window.Dispatcher.Invoke(() =>
                {
                    window.Width = width;
                });
            }

            if (height > 0)
            {
                window.Dispatcher.Invoke(() =>
                {
                    window.Height = height;
                });
            }

            TbBasePathText = IniHelper.GetBasePath();
            TbOutputPathText = IniHelper.GetOutputPath();
            TbOutputNameText = IniHelper.GetOutputFileName();

            maxThreadCount = IniHelper.GetMaxThreadCount();
            enableTimeoutSetting = IniHelper.GetEnableTimeoutSetting();
            totalTimeoutLimitAnalyze = IniHelper.GetTotalTimeoutLimitAnalyze();
            perTimeoutLimitAnalyze = IniHelper.GetPerTimeoutLimitAnalyze();

            CbExecuteInSequenceIsChecked = IniHelper.GetIsExecuteInSequence();
            CbIsAutoOpenIsChecked = IniHelper.GetIsAutoOpen();
            CbIsShowSavedMessageBox = IniHelper.GetIsShowSavedMessageBox();

            fileSystemWatcherInvokeDalay = IniHelper.GetFileSystemWatcherInvokeDalay();
            freshInterval = IniHelper.GetFreshInterval();

            FileHelper.CheckAndCreateFolders();
            LoadFiles();

            SetAutoStatusAll();


            IHighlightingDefinition chParam;
            using (Stream s = new FileStream(@"Highlighting\ParamHighlighting.xshd", FileMode.Open))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    //解析xshd
                    chParam = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            //注册数据
            HighlightingManager.Instance.RegisterHighlighting("param", new string[] { }, chParam);
            TeParams.Dispatcher.Invoke(() =>
            {
                TeParams.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("param");
            });

            IHighlightingDefinition chLog;
            using (Stream s = new FileStream(@"Highlighting\LogHighlighting.xshd", FileMode.Open))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    //解析xshd
                    chLog = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            //注册数据
            HighlightingManager.Instance.RegisterHighlighting("log", new string[] { }, chLog);
            TeLog.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("log");

            runningThread = new Thread(() =>
            {
                try 
                {
                    WhenRunning();
                }
                catch
                {
                    // DO NOTHING
                }
            });
            runningThread.Start();
        }
        private void WindowClosing(CancelEventArgs eventArgs)
        {
            if (GlobalObjects.GlobalObjects.ProgramCurrentStatus == ProgramStatus.Default && Application.Current.Windows.Count > 1)
            {
                MessageBoxResult result = CustomizableMessageBox.MessageBox.Show(Application.Current.FindResource("ProgramClosingCheck").ToString(), Application.Current.FindResource("Warning").ToString(), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    eventArgs.Cancel = true;
                    return;
                }
            }

            windowsClosing = true;
            foreach (Window currentWindow in Application.Current.Windows)
            {
                if (Application.Current.MainWindow != currentWindow)
                {
                    currentWindow.Close();
                }
            }

            IniHelper.SetWindowSize(WindowName, new Point(WindowWidth, WindowHeight));
            IniHelper.SetBasePath(TbBasePathText);
            IniHelper.SetOutputPath(TbOutputPathText);
            IniHelper.SetOutputFileName(TbOutputNameText);
            IniHelper.SetLanguage(language);

            if (CbExecuteInSequenceIsChecked == true)
            {
                IniHelper.SetIsExecuteInSequence(true);
            }
            else
            {
                IniHelper.SetIsExecuteInSequence(false);
            }
            if (CbIsAutoOpenIsChecked == true)
            {
                IniHelper.SetIsAutoOpen(true);
            }
            else
            {
                IniHelper.SetIsAutoOpen(false);
            }
            if (CbIsShowSavedMessageBox == true)
            {
                IniHelper.SetIsShowSavedMessageBox(true);
            }
            else
            {
                IniHelper.SetIsShowSavedMessageBox(false);
            }
        }

        private void WindowClosed()
        {
            CheckAndCloseThreads(true);
            if (GlobalObjects.GlobalObjects.ProgramCurrentStatus == ProgramStatus.Restart)
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("DataTransformer.exe");
                processStartInfo.UseShellExecute = true;
                Process.Start(processStartInfo);
            }
        }

        private void MenuOpen(object sender)
        {
            if (((MenuItem)sender).Name == "menu_csv_explainer_folder")
            {
                Process.Start("Explorer", ".\\CsvExplainers");
            }
            else if (((MenuItem)sender).Name == "menu_analyzer_folder")
            {
                Process.Start("Explorer", ".\\Analyzers");
            }
            else if (((MenuItem)sender).Name == "menu_dll_folder")
            {
                Process.Start("Explorer", ".\\Dlls");
            }
            else if (((MenuItem)sender).Name == "menu_rule_folder")
            {
                Process.Start("Explorer", ".\\Rules");
            }
            else if (((MenuItem)sender).Name == "menu_work_folder")
            {
                System.Diagnostics.Process.Start("Explorer", TbBasePathText);
            }
            else if (((MenuItem)sender).Name == "menu_output_folder")
            {
                string resPath = TbOutputPathText.Replace("\\", "/");
                string filePath;
                if (TbOutputPathText.EndsWith("/"))
                {
                    filePath = $"{resPath}{TbOutputNameText}.csv";
                }
                else
                {
                    filePath = $"{resPath}/{TbOutputNameText}.csv";
                }
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("Explorer", $"/e,/select,{filePath.Replace("/", "\\")}");
                }
                else
                {
                    filePath = filePath.Replace("/", "\\");
                    System.Diagnostics.Process.Start("Explorer", $"{filePath.Substring(0, filePath.LastIndexOf('\\'))}");
                }
            }
            else if (((MenuItem)sender).Name == "menu_output_file")
            {
                OpenOutput();
            }
        }

        private void ChangeTheme()
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                IniHelper.SetTheme("Dark");
            }
            else if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                IniHelper.SetTheme("Light");
            }
        }

        private void ChangeLanguage()
        {
            List<string> items = new List<string>();
            Dictionary<string, string> cultureDic = new Dictionary<string, string>();
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source != null)
                {
                    string originalString = dictionary.Source.OriginalString;
                    CultureInfo cultureInfo = new CultureInfo(originalString.Substring(originalString.IndexOf(".") + 1, (originalString.LastIndexOf(".") - originalString.IndexOf(".") - 1)), false);
                    string nativeName = cultureInfo.NativeName;
                    items.Add(nativeName);
                    cultureDic.Add(nativeName, cultureInfo.Name);
                    dictionaryList.Add(dictionary);
                }
            }

            ComboBox comboBox = new ComboBox();
            comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            comboBox.Margin = new Thickness(5);
            comboBox.ItemsSource = items;
            comboBox.SelectionChanged += (s, e) => 
            {
                string language = cultureDic[comboBox.SelectedItem.ToString()];
                ChangeLanguage(language, dictionaryList);
                CustomizableMessageBox.MessageBox.TitleText = Application.Current.FindResource("Setting").ToString();
                CustomizableMessageBox.MessageBox.MessageText = Application.Current.FindResource("SetLanguage").ToString();
                RefreshList refreshList = CustomizableMessageBox.MessageBox.ButtonList;
                refreshList[2] = Application.Current.FindResource("Ok").ToString();
                refreshList[3] = Application.Current.FindResource("Cancel").ToString();
                CustomizableMessageBox.MessageBox.ButtonList = refreshList;
            };
            int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { comboBox, new ButtonSpacer(1, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("SetLanguage").ToString(), Application.Current.FindResource("Setting").ToString());
            if (res == 3)
            {
                ChangeLanguage(language, dictionaryList);
                return;
            }
            
            language = cultureDic[comboBox.SelectedItem.ToString()];
            IniHelper.SetLanguage(language);
        }

        private void MenuSetStr(object sender)
        {
            TextBox textBox = new TextBox();
            textBox.Margin = new Thickness(5);
            textBox.Height = 30;
            textBox.VerticalContentAlignment = VerticalAlignment.Center;

            if (((MenuItem)sender).Name == "menu_max_thread_count")
            {
                textBox.Text = IniHelper.GetMaxThreadCount().ToString();
                int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { textBox, new ButtonSpacer(1, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("MaxThreadCount").ToString(), Application.Current.FindResource("Setting").ToString());
                if (res == 3)
                {
                    return;
                }
                try
                {
                    maxThreadCount = int.Parse(textBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                }
                IniHelper.SetMaxThreadCount(maxThreadCount);
            }
            else if (((MenuItem)sender).Name == "menu_total_timeout_limit_analyze")
            {
                textBox.Text = IniHelper.GetTotalTimeoutLimitAnalyze().ToString();
                int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { textBox, new ButtonSpacer(1, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("TotalTimeoutLimitAnalyze").ToString(), Application.Current.FindResource("Setting").ToString());
                if (res == 3)
                {
                    return;
                }
                try
                {
                    totalTimeoutLimitAnalyze = int.Parse(textBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                }
                IniHelper.SetTotalTimeoutLimitAnalyze(totalTimeoutLimitAnalyze);
            }
            else if (((MenuItem)sender).Name == "menu_per_timeout_limit_analyze")
            {
                textBox.Text = IniHelper.GetPerTimeoutLimitAnalyze().ToString();
                int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { textBox, new ButtonSpacer(1, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("PerTimeoutLimitAnalyze").ToString(), Application.Current.FindResource("Setting").ToString());
                if (res == 3)
                {
                    return;
                }
                try
                {
                    perTimeoutLimitAnalyze = int.Parse(textBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                }
                IniHelper.SetPerTimeoutLimitAnalyze(perTimeoutLimitAnalyze);
            }
            else if (((MenuItem)sender).Name == "menu_file_system_watcher_invoke_dalay")
            {
                textBox.Text = IniHelper.GetFileSystemWatcherInvokeDalay().ToString();
                int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { textBox, new ButtonSpacer(1, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("FileSystemWatcherInvokeDalay").ToString(), Application.Current.FindResource("Setting").ToString());
                if (res == 3)
                {
                    return;
                }
                try
                {
                    fileSystemWatcherInvokeDalay = int.Parse(textBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                }
                IniHelper.SetFileSystemWatcherInvokeDalay(fileSystemWatcherInvokeDalay);
            }
            else if (((MenuItem)sender).Name == "menu_fresh_interval")
            {
                textBox.Text = IniHelper.GetFreshInterval().ToString();
                int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { textBox, new ButtonSpacer(1, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("FreshInterval").ToString(), Application.Current.FindResource("Setting").ToString());
                if (res == 3)
                {
                    return;
                }
                try
                {
                    freshInterval = int.Parse(textBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                }
                IniHelper.SetFreshInterval(freshInterval);
            }
        }

        private void MenuSetCheckbox(object sender)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Margin = new Thickness(5);
            checkBox.Height = 30;
            checkBox.VerticalContentAlignment = VerticalAlignment.Center;
            checkBox.Content = Application.Current.FindResource("IsEnable").ToString();

            if (((MenuItem)sender).Name == "menu_enable_timeout_setting")
            {
                checkBox.IsChecked = IniHelper.GetEnableTimeoutSetting();
                int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { checkBox, new ButtonSpacer(1, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("EnableTimeoutSetting").ToString(), Application.Current.FindResource("Setting").ToString());
                if (res == 3)
                {
                    return;
                }
                try
                {
                    enableTimeoutSetting = (bool)checkBox.IsChecked;
                }
                catch (Exception ex)
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                }
                IniHelper.SetEnableTimeoutSetting(enableTimeoutSetting);
            }
        }

        private void OpenSourceCodeUrl()
        {
            var psi = new ProcessStartInfo
            {
                FileName = @"https:\\www.github.com\ZjzMisaka\DataTransformer",
                UseShellExecute = true
            };

            int res = CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(2, GridUnitType.Star, true), Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("OpenBrowser").ToString(), Application.Current.FindResource("Setting").ToString(), MessageBoxImage.Warning);
            if (res == 2)
            {
                return;
            }

            Process.Start(psi);
        }

        private void BtnOpenCsvExplainerEditorClick()
        {
            CsvExplainerEditor csvExplainerEditor = new CsvExplainerEditor();
            try
            {
                csvExplainerEditor.Show();
            }
            catch
            {
                // Do nothing
            }
        }

        private void BtnOpenAnalyzerEditorClick()
        {
            AnalyzerEditor analyzerEditor = new AnalyzerEditor();
            try
            {
                analyzerEditor.Show();
            }
            catch
            { 
                // Do nothing
            }
        }

        private void CbCsvExplainersPreviewMouseLeftButtonDown()
        {
            CsvExplainersItems = FileHelper.GetCsvExplainersList();
        }

        private void CbCsvExplainersSelectionChanged()
        {
            if (SelectedCsvExplainersIndex == 0)
            {
                return;
            }
            TeCsvExplainersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument($"{TeCsvExplainersDocument.Text}{SelectedCsvExplainersItem}\n");
            SelectedCsvExplainersIndex = 0;
        }

        private void CbAnalyzersPreviewMouseLeftButtonDown()
        {
            AnalyzersItems = FileHelper.GetAnalyzersList();
        }

        private void CbAnalyzersSelectionChanged()
        {
            if (SelectedAnalyzersIndex == 0)
            {
                return;
            }
            TeAnalyzersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument($"{TeAnalyzersDocument.Text}{SelectedAnalyzersItem}\n");
            SelectedAnalyzersIndex = 0;
        }

        private void TeCsvexplainersDrop(DragEventArgs obj)
        {
            if (!obj.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            Array paths = (Array)obj.Data.GetData(DataFormats.FileDrop);

            List<string> jsonList = new List<string>();
            List<string> csvList = new List<string>();
            List<string> folderList = new List<string>();

            List<string> addList = new List<string>();

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    folderList.Add(path);
                }
                else if (File.Exists(path))
                {
                    Regex rgx = new Regex("[\\s\\S]*.csv", RegexOptions.IgnoreCase);
                    if (new FileInfo(path).Extension == ".json")
                    {
                        jsonList.Add(path);
                    }
                    else if (rgx.IsMatch(new FileInfo(path).Name))
                    {
                        csvList.Add(path);
                    }
                }
            }

            if (jsonList.Count > 0)
            {
                foreach (string file in jsonList)
                {
                    string toFileName = $".\\CsvExplainers\\{new FileInfo(file).Name}";

                    string fromFull = Path.GetFullPath(file);
                    string toFull = Path.GetFullPath(toFileName);

                    addList.Add(Path.GetFileNameWithoutExtension(fromFull));

                    if (Path.GetFullPath(file) == Path.GetFullPath(toFileName))
                    {
                        continue;
                    }

                    if (File.Exists(toFileName))
                    {
                        MessageBoxResult result = CustomizableMessageBox.MessageBox.Show(Application.Current.FindResource("ReplaceFile").ToString().Replace("{0}", $"\n{fromFull}\n{toFull}"), Application.Current.FindResource("Warning").ToString(), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            File.Copy(fromFull, toFull, true);
                        }
                    }
                    else
                    {
                        File.Copy(fromFull, toFull);
                    }
                }
            }

            if (csvList.Count > 0)
            {
                CsvExplainer csvExplainer = new CsvExplainer();

                csvExplainer.pathes = new List<string>();
                csvExplainer.fileNames = new KeyValuePair<FindingMethod, List<string>>(FindingMethod.SAME, new List<string>());

                foreach (string csv in csvList)
                {
                    csvExplainer.fileNames.Value.Add(csv);
                }

                string fileName = $"Temp_Csv_Explainer_{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}_{DateTime.Now.Millisecond}";
                fileName = FileHelper.SavaCsvExplainerJson(fileName, csvExplainer, false);

                addList.Add(fileName);
            }

            if (folderList.Count > 0)
            {
                CsvExplainer csvExplainer = new CsvExplainer();

                foreach (string folder in folderList)
                {
                    csvExplainer.pathes.Add(folder);
                }

                csvExplainer.fileNames = new KeyValuePair<FindingMethod, List<string>>(FindingMethod.ALL, new List<string>());

                string fileName = $"Temp_Csv_Explainer_{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}_{DateTime.Now.Millisecond}";
                fileName = FileHelper.SavaCsvExplainerJson(fileName, csvExplainer, false);

                addList.Add(fileName);
            }

            foreach (string needAdd in addList)
            {
                TeCsvExplainersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument($"{TeCsvExplainersDocument.Text}{needAdd}\n");
            }
        }

        private void TeAnalyzersDrop(DragEventArgs obj)
        {
            if (!obj.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            Array paths = (Array)obj.Data.GetData(DataFormats.FileDrop);

            List<string> jsonList = new List<string>();

            List<string> addList = new List<string>();

            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    if (new FileInfo(path).Extension == ".json")
                    {
                        jsonList.Add(path);
                    }
                }
            }

            if (jsonList.Count > 0)
            {
                foreach (string file in jsonList)
                {
                    string toFileName = $".\\Analyzers\\{new FileInfo(file).Name}";

                    string fromFull = Path.GetFullPath(file);
                    string toFull = Path.GetFullPath(toFileName);

                    addList.Add(Path.GetFileNameWithoutExtension(fromFull));

                    if (Path.GetFullPath(file) == Path.GetFullPath(toFileName))
                    {
                        continue;
                    }

                    if (File.Exists(toFileName))
                    {
                        MessageBoxResult result = CustomizableMessageBox.MessageBox.Show(Application.Current.FindResource("ReplaceFile").ToString().Replace("{0}", $"\n{fromFull}\n{toFull}"), Application.Current.FindResource("Warning").ToString(), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            File.Copy(fromFull, toFull, true);
                        }
                    }
                    else
                    {
                        File.Copy(fromFull, toFull);
                    }
                }
            }

            foreach (string needAdd in addList)
            {
                TeAnalyzersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument($"{TeAnalyzersDocument.Text}{needAdd}\n");
            }
        }

        private void CbParamsPreviewMouseLeftButtonDown()
        {
            ParamsItems = FileHelper.GetParamsList();
        }

        private void CbParamsSelectionChanged()
        {
            if (CbParamsSelectionChangedCommand != null)
            {
                List<string> paramsList = FileHelper.GetParamsList(true);
                if (paramsList.IndexOf("[Lock]") == -1 || SelectedParamsIndex < paramsList.IndexOf("[Lock]"))
                {
                    BtnLockVisibility = Visibility.Visible;
                    BtnUnlockVisibility = Visibility.Hidden;
                }
                else 
                {
                    BtnLockVisibility = Visibility.Hidden;
                    BtnUnlockVisibility = Visibility.Visible;
                }

                TeParams.Dispatcher.Invoke(() =>
                {
                    TeParams.TextChanged -= TeParamsTextChanged;
                    if (SelectedParamsItem == null)
                    {
                        TeParams.Document = new ICSharpCode.AvalonEdit.Document.TextDocument("");
                    }
                    else 
                    {
                        TeParams.Document = new ICSharpCode.AvalonEdit.Document.TextDocument(SelectedParamsItem);
                    }
                    TeParams.TextChanged += TeParamsTextChanged;
                });
            }
        }

        private void EditParam()
        {
            int groupIndex = 0;

            string paramStr = FormatTeParams();
            paramStr = ParamHelper.EncodeFromEscaped(paramStr);

            Dictionary<string, Dictionary<string, string>> paramDicEachAnalyzer = ParamHelper.GetParamDicEachAnalyzer(paramStr);
            if (paramDicEachAnalyzer == null) 
            {
                return;
            }

            List<String> analyzersList = TeAnalyzersDocument.Text.Split('\n').Where(str => str.Trim() != "").ToList();

            List<string> addedAnalyzerNameList = new List<string>();

            AnalyzersItems = FileHelper.GetAnalyzersList();
            foreach (string analyzer in analyzersList)
            {
                if (!AnalyzersItems.Contains(analyzer))
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, Application.Current.FindResource("FileNotFound").ToString().Replace("{0}", $"{analyzer}.json"), Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                    return;
                }
            }

            ParamSetter paramSetter = new ParamSetter();

            TabControl tab = new TabControl();
            paramSetter.g_main.Children.Add(tab);
            foreach (string analyzerName in analyzersList)
            {
                int rowNum = -1;
                double maxTextlength = 0;

                TabItem tabItem = new TabItem();
                tabItem.Header = analyzerName;
                tab.Items.Add(tabItem);

                ScrollViewer sv = new ScrollViewer();
                sv.Padding = new Thickness(4, 0, 4, 0);
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                tabItem.Content = sv;

                Grid newGrid = new Grid();
                newGrid.Margin = new Thickness(20, 10, 20, 10);
                sv.Content = newGrid;

                ColumnDefinition columnDefinitionL = new ColumnDefinition();
                newGrid.ColumnDefinitions.Add(columnDefinitionL);
                ColumnDefinition columnDefinitionM = new ColumnDefinition();
                columnDefinitionM.Width = new GridLength(1, GridUnitType.Auto);
                newGrid.ColumnDefinitions.Add(columnDefinitionM);
                ColumnDefinition columnDefinitionR = new ColumnDefinition();
                newGrid.ColumnDefinitions.Add(columnDefinitionR);
                if (addedAnalyzerNameList.Contains(analyzerName))
                {
                    continue;
                }
                else
                {
                    addedAnalyzerNameList.Add(analyzerName);
                }

                Analyzer analyzer = JsonHelper.TryDeserializeObject<Analyzer>($".\\Analyzers\\{analyzerName}.json");
                if (analyzer == null)
                {
                    return;
                }

                Dictionary<string, string> paramDic = null;
                if (paramDicEachAnalyzer.ContainsKey(analyzerName))
                {
                    paramDic = paramDicEachAnalyzer[analyzerName];
                }

                if (analyzer.paramDic == null || analyzer.paramDic.Keys == null || analyzer.paramDic.Keys.Count == 0)
                {
                    continue;
                }

                GridSplitter gridSplitter = new GridSplitter();
                gridSplitter.HorizontalAlignment = HorizontalAlignment.Center;
                gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
                Panel.SetZIndex(gridSplitter, 9999);
                Style style = new Style();
                style.TargetType = typeof(GridSplitter);
                style.Setters.Add(new Setter(GridSplitter.WidthProperty, 4d));
                style.Setters.Add(new Setter(GridSplitter.BorderBrushProperty, Brushes.DarkGray));
                style.Setters.Add(new Setter(GridSplitter.BorderThicknessProperty, new Thickness(1, 0, 1, 0)));
                style.Setters.Add(new Setter(GridSplitter.BackgroundProperty, ThemeBackground));
                MultiDataTrigger triggerIsDragging = new MultiDataTrigger();
                Condition conditionIsDragging = new Condition();
                conditionIsDragging.Binding = new Binding() { Path = new PropertyPath("IsDragging"), RelativeSource = RelativeSource.Self };
                conditionIsDragging.Value = true;
                triggerIsDragging.Conditions.Add(conditionIsDragging);
                triggerIsDragging.Setters.Add(new Setter(GridSplitter.WidthProperty, 4d));
                triggerIsDragging.Setters.Add(new Setter(GridSplitter.BorderBrushProperty, Brushes.DarkGray));
                triggerIsDragging.Setters.Add(new Setter(GridSplitter.BorderThicknessProperty, new Thickness(1, 0, 1, 0)));
                triggerIsDragging.Setters.Add(new Setter(GridSplitter.BackgroundProperty, Brushes.DarkGray));
                style.Triggers.Add(triggerIsDragging);
                gridSplitter.Style = style;
                Grid.SetRow(gridSplitter, rowNum + 1);
                Grid.SetRowSpan(gridSplitter, analyzer.paramDic.Keys.Count);
                Grid.SetColumn(gridSplitter, 1);
                newGrid.Children.Add(gridSplitter);

                foreach (string key in analyzer.paramDic.Keys)
                {
                    RowDefinition rowDefinitionKv = new RowDefinition();
                    newGrid.RowDefinitions.Add(rowDefinitionKv);
                    ++rowNum;

                    TextBlock textBlockKey = new TextBlock();
                    textBlockKey.Margin = new Thickness(0, 5, 0, 0);
                    textBlockKey.Height = 25;
                    string desCribeText = analyzer.paramDic[key].describe;
                    if (analyzer.globalizationSetter != null && analyzer.globalizationSetter.enableGlobalizationForParamSetter)
                    {
                        desCribeText = analyzer.globalizationSetter.Find(language, analyzer.paramDic[key].describe);
                    }
                    if (String.IsNullOrWhiteSpace(desCribeText))
                    {
                        textBlockKey.Text = ParamHelper.DecodeForDisplay(key);
                    }
                    else
                    {
                        textBlockKey.Text = ParamHelper.DecodeForDisplay(desCribeText);
                    }
                    if (analyzer.paramDic[key].describe != null)
                    {
                        FormattedText ft = new FormattedText(desCribeText, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(tabItem.FontFamily, tabItem.FontStyle, tabItem.FontWeight, tabItem.FontStretch), tabItem.FontSize, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(tabItem).PixelsPerDip);
                        if (ft.Width > maxTextlength)
                        {
                            maxTextlength = ft.Width;
                        }
                    }
                    textBlockKey.HorizontalAlignment = HorizontalAlignment.Stretch;
                    textBlockKey.VerticalAlignment = VerticalAlignment.Top;
                    Grid.SetRow(textBlockKey, rowNum);
                    Grid.SetColumn(textBlockKey, 0);
                    if (analyzer.paramDic[key].describe != null)
                    {
                        textBlockKey.MouseEnter += (s, ex) =>
                        {
                            textBlockKey.Text = ParamHelper.DecodeForDisplay(key);
                        };
                        textBlockKey.TouchEnter += (s, ex) =>
                        {
                            textBlockKey.Text = ParamHelper.DecodeForDisplay(key);
                        };
                        textBlockKey.MouseLeave += (s, ex) =>
                        {
                            textBlockKey.Text = ParamHelper.DecodeForDisplay(desCribeText);
                        };
                        textBlockKey.TouchLeave += (s, ex) =>
                        {
                            textBlockKey.Text = ParamHelper.DecodeForDisplay(desCribeText);
                        };
                    }
                    newGrid.Children.Add(textBlockKey);

                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Margin = new Thickness(5, 5, 0, 5);
                    Grid.SetRow(stackPanel, rowNum);
                    Grid.SetColumn(stackPanel, 2);

                    if (analyzer.paramDic[key].possibleValues != null && analyzer.paramDic[key].possibleValues.Count > 0)
                    {
                        if (analyzer.paramDic[key].type == ParamType.Single)
                        {
                            ++groupIndex;
                            foreach (PossibleValue possibleValue in analyzer.paramDic[key].possibleValues)
                            {
                                RadioButton radioButton = new RadioButton();
                                radioButton.Height = 30;
                                radioButton.HorizontalAlignment = HorizontalAlignment.Stretch;
                                radioButton.VerticalContentAlignment = VerticalAlignment.Top;
                                radioButton.GroupName = $"{analyzerName}_{key}_{groupIndex}";
                                string desCribeTextRb = possibleValue.describe;
                                if (analyzer.globalizationSetter != null && analyzer.globalizationSetter.enableGlobalizationForParamSetter)
                                {
                                    desCribeTextRb = analyzer.globalizationSetter.Find(language, possibleValue.value);
                                }
                                if (String.IsNullOrWhiteSpace(desCribeTextRb))
                                {
                                    radioButton.Content = ParamHelper.DecodeForDisplay(possibleValue.value);
                                }
                                else
                                {
                                    radioButton.Content = ParamHelper.DecodeForDisplay(desCribeTextRb);
                                }
                                stackPanel.Children.Add(radioButton);

                                if (possibleValue.describe != null)
                                {
                                    radioButton.MouseEnter += (s, ex) =>
                                    {
                                        radioButton.Content = ParamHelper.DecodeForDisplay(possibleValue.value);
                                    };
                                    radioButton.TouchEnter += (s, ex) =>
                                    {
                                        radioButton.Content = ParamHelper.DecodeForDisplay(possibleValue.value);
                                    };
                                    radioButton.MouseLeave += (s, ex) =>
                                    {
                                        radioButton.Content = ParamHelper.DecodeForDisplay(desCribeTextRb);
                                    };
                                    radioButton.TouchLeave += (s, ex) =>
                                    {
                                        radioButton.Content = ParamHelper.DecodeForDisplay(desCribeTextRb);
                                    };
                                }
                                bool justChecked = false;
                                radioButton.Click += (s, ex) =>
                                {
                                    if (justChecked)
                                    {
                                        justChecked = false;
                                        ex.Handled = true;
                                        return;
                                    }
                                    if ((bool)radioButton.IsChecked)
                                        radioButton.IsChecked = false;
                                };
                                radioButton.Checked += (s, ex) =>
                                {
                                    if (!paramDicEachAnalyzer.ContainsKey(analyzerName))
                                    {
                                        paramDicEachAnalyzer.Add(analyzerName, new Dictionary<string, string>());
                                    }
                                    paramDicEachAnalyzer[analyzerName][key] = possibleValue.value;

                                    justChecked = true;
                                };
                                radioButton.Unchecked += (s, ex) =>
                                {
                                    paramDicEachAnalyzer[analyzerName].Remove(key);
                                    if (paramDicEachAnalyzer[analyzerName].Keys.Count == 0)
                                    {
                                        paramDicEachAnalyzer.Remove(analyzerName);
                                    }
                                };

                                if (paramDic != null && paramDic.ContainsKey(key))
                                {
                                    if (possibleValue.value == paramDic[key])
                                    {
                                        radioButton.IsChecked = true;
                                        justChecked = false;
                                    }
                                }
                                else if (paramDicEachAnalyzer.ContainsKey("public") && paramDicEachAnalyzer["public"] != null && paramDicEachAnalyzer["public"].ContainsKey(key))
                                {
                                    if (possibleValue.value == paramDicEachAnalyzer["public"][key])
                                    {
                                        radioButton.IsChecked = true;
                                        justChecked = false;
                                    }
                                }
                            }
                        }
                        else if (analyzer.paramDic[key].type == ParamType.Multiple)
                        {
                            foreach (PossibleValue possibleValue in analyzer.paramDic[key].possibleValues)
                            {
                                CheckBox checkBox = new CheckBox();
                                checkBox.Height = 30;
                                checkBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                                checkBox.VerticalContentAlignment = VerticalAlignment.Top;
                                string desCribeTextCb = possibleValue.describe;
                                if (analyzer.globalizationSetter != null && analyzer.globalizationSetter.enableGlobalizationForParamSetter)
                                {
                                    desCribeTextCb = analyzer.globalizationSetter.Find(language, possibleValue.value);
                                }
                                checkBox.Content = ParamHelper.DecodeForDisplay(desCribeTextCb);
                                if (String.IsNullOrWhiteSpace(desCribeTextCb))
                                {
                                    checkBox.Content = ParamHelper.DecodeForDisplay(possibleValue.value);
                                }
                                else
                                {
                                    checkBox.Content = ParamHelper.DecodeForDisplay(desCribeTextCb);
                                }
                                stackPanel.Children.Add(checkBox);

                                if (possibleValue.describe != null)
                                {
                                    checkBox.MouseEnter += (s, ex) =>
                                    {
                                        checkBox.Content = ParamHelper.DecodeForDisplay(possibleValue.value);
                                    };
                                    checkBox.TouchEnter += (s, ex) =>
                                    {
                                        checkBox.Content = ParamHelper.DecodeForDisplay(possibleValue.value);
                                    };
                                    checkBox.MouseLeave += (s, ex) =>
                                    {
                                        checkBox.Content = ParamHelper.DecodeForDisplay(desCribeTextCb);
                                    };
                                    checkBox.TouchLeave += (s, ex) =>
                                    {
                                        checkBox.Content = ParamHelper.DecodeForDisplay(desCribeTextCb);
                                    };
                                }
                                checkBox.Checked += (s, ex) =>
                                {
                                    if (!paramDicEachAnalyzer.ContainsKey(analyzerName))
                                    {
                                        paramDicEachAnalyzer.Add(analyzerName, new Dictionary<string, string>());
                                    }
                                    if (!paramDicEachAnalyzer[analyzerName].ContainsKey(key))
                                    {
                                        paramDicEachAnalyzer[analyzerName].Add(key, possibleValue.value);
                                    }
                                    else
                                    {
                                        if (paramDicEachAnalyzer[analyzerName][key] == "")
                                        {
                                            paramDicEachAnalyzer[analyzerName][key] += possibleValue.value;
                                        }
                                        else
                                        {
                                            if (!paramDicEachAnalyzer[analyzerName][key].Split('+').ToArray().Contains(possibleValue.value))
                                            {
                                                paramDicEachAnalyzer[analyzerName][key] += $"+{possibleValue.value}";
                                            }
                                        }
                                    }
                                };
                                checkBox.Unchecked += (s, ex) =>
                                {
                                    string valueTemp = paramDicEachAnalyzer[analyzerName][key];
                                    if (valueTemp.Contains($"+{possibleValue.value}"))
                                    {
                                        paramDicEachAnalyzer[analyzerName][key] = valueTemp.Replace($"+{possibleValue.value}", "");
                                    }
                                    else if (valueTemp.Contains($"{possibleValue.value}+"))
                                    {
                                        paramDicEachAnalyzer[analyzerName][key] = valueTemp.Replace($"{possibleValue.value}+", "");
                                    }
                                    else
                                    {
                                        paramDicEachAnalyzer[analyzerName].Remove(key);
                                        if (paramDicEachAnalyzer[analyzerName].Keys.Count == 0)
                                        {
                                            paramDicEachAnalyzer.Remove(analyzerName);
                                        }
                                    }
                                };

                                if (paramDic != null && paramDic.ContainsKey(key))
                                {
                                    List<string> valueList = paramDic[key].Split('+').ToList();
                                    if (valueList.Contains(possibleValue.value))
                                    {
                                        checkBox.IsChecked = true;
                                    }
                                }
                                else if (paramDicEachAnalyzer.ContainsKey("public") && paramDicEachAnalyzer["public"] != null && paramDicEachAnalyzer["public"].ContainsKey(key))
                                {
                                    List<string> valueList = paramDicEachAnalyzer["public"][key].Split('+').ToList();
                                    if (valueList.Contains(possibleValue.value))
                                    {
                                        checkBox.IsChecked = true;
                                    }
                                }
                            }
                        }
                        rowDefinitionKv.Height = new GridLength(30 * analyzer.paramDic[key].possibleValues.Count + 10);
                    }
                    else
                    {
                        TextBox tbValue = new TextBox();
                        tbValue.HorizontalAlignment = HorizontalAlignment.Stretch;
                        tbValue.VerticalAlignment = VerticalAlignment.Top;
                        tbValue.VerticalContentAlignment = VerticalAlignment.Center;

                        tbValue.TextChanged += (s, ex) =>
                        {
                            if (tbValue.Text == "")
                            {
                                paramDicEachAnalyzer[analyzerName].Remove(key);
                                if (paramDicEachAnalyzer[analyzerName].Keys.Count == 0)
                                {
                                    paramDicEachAnalyzer.Remove(analyzerName);
                                }
                            }
                            else
                            {
                                if (!paramDicEachAnalyzer.ContainsKey(analyzerName))
                                {
                                    paramDicEachAnalyzer.Add(analyzerName, new Dictionary<string, string>());
                                }
                                paramDicEachAnalyzer[analyzerName][key] = ParamHelper.EncodeFromDisplay(tbValue.Text);
                            }
                        };

                        if (paramDic != null && paramDic.ContainsKey(key))
                        {
                            tbValue.Text = ParamHelper.DecodeForDisplay(paramDic[key]);
                        }
                        else if (paramDicEachAnalyzer.ContainsKey("public") && paramDicEachAnalyzer["public"] != null && paramDicEachAnalyzer["public"].ContainsKey(key))
                        {
                            tbValue.Text = ParamHelper.DecodeForDisplay(paramDicEachAnalyzer["public"][key]);
                        }

                        stackPanel.Children.Add(tbValue);

                        rowDefinitionKv.Height = new GridLength(50);
                    }

                    newGrid.Children.Add(stackPanel);
                }

                if (maxTextlength + 10 < 200)
                {
                    columnDefinitionL.Width = new GridLength(maxTextlength + 10);
                }
                else
                {
                    columnDefinitionL.Width = new GridLength(200);
                }
            }

            RowDefinition rowDefinitionOk = new RowDefinition();
            rowDefinitionOk.Height = new GridLength(35);
            paramSetter.g_btn.RowDefinitions.Add(rowDefinitionOk);
            Button btnOk = new Button();
            btnOk.VerticalAlignment = VerticalAlignment.Center;
            btnOk.HorizontalAlignment = HorizontalAlignment.Stretch;
            btnOk.Margin = new Thickness(0, 5, 0, 0);
            btnOk.Content = Application.Current.FindResource("Ok").ToString();
            btnOk.Click += (s, ex) =>
            {
                TeParams.Text = ParamHelper.GetParamStr(paramDicEachAnalyzer);
                paramSetter.Close();
            };
            Grid.SetRow(btnOk, 0);
            paramSetter.g_btn.Children.Add(btnOk);

            paramSetter.ShowDialog();
        }

        private void LockParam()
        {
            string paramStr = FormatTeParams();
            paramStr = ParamHelper.EncodeFromEscaped(paramStr);
            SaveParam(paramStr, false, true);

            ParamsItems = FileHelper.GetParamsList();
            SelectedParamsItem = TeParams.Text;
        }

        private void UnlockParam()
        {
            String paramStrFromFile = File.ReadAllText(".\\Params.txt");
            List<string> paramsList = paramStrFromFile.Split('\n').ToList<string>();
            for (int i = 0; i < paramsList.Count; ++i)
            {
                paramsList[i] = paramsList[i].Trim();
            }
            paramsList.Remove(ParamHelper.EncodeFromEscaped(SelectedParamsItem));
            paramsList.RemoveAll(s => s == "");

            ParamsItems.Remove(SelectedParamsItem);

            try
            {
                File.WriteAllLines(".\\Params.txt", paramsList);
            }
            catch (Exception ex)
            {
                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
            }

            ParamsItems = FileHelper.GetParamsList();
            CbParamsSelectionChangedCommand = null;
            SelectedParamsIndex = 0;
            CbParamsSelectionChangedCommand = new RelayCommand(CbParamsSelectionChanged);
        }

        private void TeParamsTextChanged(object sender, EventArgs e)
        {
            BtnLockVisibility = Visibility.Visible;
            BtnUnlockVisibility = Visibility.Hidden;

            CbParamsSelectionChangedCommand = null;
            SelectedParamsIndex = 0;
            CbParamsSelectionChangedCommand = new RelayCommand(CbParamsSelectionChanged);
        }

        private void TeParamsGotFocus(object sender, EventArgs e)
        {
            teParams.Background = ThemeControlFocusBackground;
            teParams.Foreground = ThemeControlForeground;
            Border border = (Border)((ContentControl)((TextEditor)sender).Parent).Parent;
            border.BorderBrush = ThemeControlBorderBrush;
            border.BorderThickness = new Thickness(2);
            border.Background = ThemeControlFocusBackground;
            teParamsFocused = true;
        }

        private void TeParamsLostFocus(object sender, EventArgs e)
        {
            teParams.Background = ThemeControlBackground;
            teParams.Foreground = ThemeControlForeground;
            Border border = (Border)((ContentControl)((TextEditor)sender).Parent).Parent;
            border.BorderBrush = Brushes.DimGray;
            border.BorderThickness = new Thickness(1);
            border.Background = ThemeControlBackground;
            teParamsFocused = false;
        }

        private void TeParamsMouseEnter(object sender, EventArgs e)
        {
            if (!teParamsFocused)
            {
                teParams.Background = ThemeControlBackground;
                teParams.Foreground = ThemeControlForeground;
                Border border = (Border)((ContentControl)((TextEditor)sender).Parent).Parent;
                border.BorderBrush = ThemeControlHoverBorderBrush;
                border.Background = ThemeControlBackground;
                border.BorderThickness = new Thickness(1);
            }
        }

        private void TeParamsMouseLeave(object sender, EventArgs e)
        {
            if (!teParamsFocused)
            {
                teParams.Background = ThemeControlBackground;
                teParams.Foreground = ThemeControlForeground;
                Border border = (Border)((ContentControl)((TextEditor)sender).Parent).Parent;
                border.BorderBrush = Brushes.DimGray;
                border.Background = ThemeControlBackground;
                border.BorderThickness = new Thickness(1);
            }
        }

        private void TeParamsPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        private void TbParamsLostFocus()
        {
             FormatTeParams();
        }

        public void DragEnter(IDropInfo dropInfo)
        {

        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Copy;
        }

        public void DragLeave(IDropInfo dropInfo)
        {

        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo?.VisualSource is null
                && dropInfo.Data is DataObject dataObject
                && dataObject.GetDataPresent(DataFormats.FileDrop)
                && dataObject.ContainsFileDropList())
            {
                string parentName = ((TextBox)((Grid)dropInfo.TargetScrollViewer.Parent).TemplatedParent).Name;
                if (parentName == "tb_base_path")
                {
                    TbBasePathText = dataObject.GetFileDropList()[0];
                }
                else if (parentName == "tb_output_path")
                {
                    TbOutputPathText = dataObject.GetFileDropList()[0];
                }
                else if (parentName == "tb_output_name")
                {
                    TbOutputNameText = Path.GetFileNameWithoutExtension(dataObject.GetFileDropList()[0]);
                }
            }
            else
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }

        private void SelectPath(object sender)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog();

            string btnName = ((Button)sender).Name;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (btnName == "btn_select_base_path")
                {
                    TbBasePathText = openFileDialog.SelectedPath;
                }
                else if (btnName == "btn_select_output_path")
                {
                    TbOutputPathText = openFileDialog.SelectedPath;
                }
            }
        }

        private void OpenPath(object sender)
        {
            string btnName = ((Button)sender).Name;

            if (btnName == "btn_open_output_path")
            {
                string resPath = TbOutputPathText.Replace("\\", "/");
                string filePath;
                if (TbOutputPathText.EndsWith("/"))
                {
                    filePath = $"{resPath}{TbOutputNameText}.csv";
                }
                else
                {
                    filePath = $"{resPath}/{TbOutputNameText}.csv";
                }
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("Explorer", $"/e,/select,{filePath.Replace("/", "\\")}");
                }
                else
                {
                    filePath = filePath.Replace("/", "\\");
                    System.Diagnostics.Process.Start("Explorer", $"{filePath.Substring(0, filePath.LastIndexOf('\\'))}");
                }
            }
            else if (btnName == "btn_open_base_path")
            {
                System.Diagnostics.Process.Start("Explorer", TbBasePathText);
            }
        }

        private void SelectName()
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = Application.Current.FindResource("SelectFile").ToString();
            fileDialog.Filter = $"Csv{Application.Current.FindResource("File").ToString()}|*.csv|All files(*.*)|*.*";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] names = fileDialog.FileNames;
                TbOutputNameText = Path.GetFileNameWithoutExtension(names[0]);
            }
        }

        private void OpenOutput()
        {
            string resPath = TbOutputPathText.Replace("\\", "/");
            string filePath;
            if (TbOutputPathText.EndsWith("/"))
            {
                filePath = $"{resPath}{TbOutputNameText}.csv";
            }
            else
            {
                filePath = $"{resPath}/{TbOutputNameText}.csv";
            }
            if (File.Exists(filePath))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(filePath);
                processStartInfo.UseShellExecute = true;
                Process.Start(processStartInfo);
            }
            else
            {
                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, Application.Current.FindResource("FileNotFound").ToString().Replace("{0}", filePath), Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
            }
        }

        private void CbRulesChanged()
        {

            RunningRule rule = null;

            if (SelectedRulesIndex >= 1)
            {
                rule = JsonHelper.TryDeserializeObject<RunningRule>($".\\Rules\\{SelectedRulesItem}.json");

                if (rule == null || !CheckRule(rule))
                {
                    SelectedRulesIndex = 0;
                    return;
                }

                BtnSaveRuleVisibility = Visibility.Hidden;
                BtnDeleteRuleVisibility = Visibility.Visible;

                if (rule.watchPath != null && rule.watchPath != "")
                {
                    BtnSetAutoVisibility = Visibility.Hidden;
                    BtnUnsetAutoVisibility = Visibility.Visible;
                    BtnDeleteRuleIsEnabled = false;
                }
                else
                {
                    BtnSetAutoVisibility = Visibility.Visible;
                    BtnUnsetAutoVisibility = Visibility.Hidden;
                    BtnDeleteRuleIsEnabled = true;
                }

                TeAnalyzersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument(rule.analyzers);
                TeCsvExplainersDocument = new ICSharpCode.AvalonEdit.Document.TextDocument(rule.csvExplainers);
                CbExecuteInSequenceIsChecked = rule.executeInSequence;
                TeParams.Dispatcher.Invoke(() =>
                {
                    TeParams.Text = rule.param;
                });

                TbBasePathText = rule.basePath;
                TbOutputPathText = rule.outputPath;
                TbOutputNameText = rule.outputName;
            }
            else
            {
                BtnSaveRuleVisibility = Visibility.Visible;
                BtnDeleteRuleVisibility = Visibility.Hidden;

                BtnSetAutoVisibility = Visibility.Hidden;
                BtnUnsetAutoVisibility = Visibility.Hidden;
            }
        }

        private void CbRulesPreviewMouseLeftButtonDown()
        {
            RuleItems = FileHelper.GetRulesList();
        }
        private void SaveRule()
        {
            TextBox tbName = new TextBox();
            tbName.Margin = new Thickness(5);
            tbName.Height = 30;
            tbName.VerticalContentAlignment = VerticalAlignment.Center;
            if (SelectedRulesIndex >= 1)
            {
                tbName.Text = $"Copy Of {SelectedRulesItem}";
            }
            int result = CustomizableMessageBox.MessageBox.Show(new RefreshList() { tbName, Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("Name").ToString(), Application.Current.FindResource("Saving").ToString(), MessageBoxImage.Information);
            if (result == 1)
            {
                if (tbName.Text == "")
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, Application.Current.FindResource("FileNameEmptyError").ToString(), Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                    return;
                }

                RunningRule runningRule = new RunningRule();
                runningRule.analyzers = TeAnalyzersDocument.Text;
                runningRule.csvExplainers = TeCsvExplainersDocument.Text;
                runningRule.executeInSequence = CbExecuteInSequenceIsChecked;
                TeParams.Dispatcher.Invoke(() =>
                {
                    runningRule.param = TeParams.Text;
                });
                runningRule.basePath = TbBasePathText;
                runningRule.outputPath = TbOutputPathText;
                runningRule.outputName = TbOutputNameText;

                FileHelper.SavaRunningRuleJson(tbName.Text, runningRule, true);
            }
        }

        private void DeleteRule()
        {
            String path = $"{System.Environment.CurrentDirectory}\\Rules\\{SelectedRulesItem}.json";
            MessageBoxResult result = CustomizableMessageBox.MessageBox.Show($"{Application.Current.FindResource("Delete").ToString()}\n{path}", Application.Current.FindResource("Warning").ToString(), MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
            {
                File.Delete(path);
                SelectedAnalyzersIndex = 0;

                RuleItems = FileHelper.GetRulesList();
                SelectedRulesIndex = 0;
            }
        }

        private void SetAuto()
        {
            string ruleName = SelectedRulesItem;
            RunningRule rule = JsonHelper.TryDeserializeObject<RunningRule>($".\\Rules\\{ruleName}.json");
            if (rule == null)
            {
                return;
            }
            
            TextBox tbPath = new TextBox();
            tbPath.Margin = new Thickness(5);
            tbPath.Height = 30;
            tbPath.VerticalContentAlignment = VerticalAlignment.Center;
            TextBox tbFilter = new TextBox();
            tbFilter.Margin = new Thickness(5);
            tbFilter.Height = 30;
            tbFilter.VerticalContentAlignment = VerticalAlignment.Center;
            Button buttonCancel = new Button();
            buttonCancel.Height = 30;
            buttonCancel.HorizontalAlignment = HorizontalAlignment.Stretch;
            buttonCancel.Margin = new Thickness(5);
            buttonCancel.Content = Application.Current.FindResource("Cancel").ToString();
            buttonCancel.Click += (sender, e) =>
            {
                CustomizableMessageBox.MessageBox.CloseNow();
            };
            Button buttonOkPath = new Button();
            buttonOkPath.Height = 30;
            buttonOkPath.HorizontalAlignment = HorizontalAlignment.Stretch;
            buttonOkPath.Margin = new Thickness(5);
            buttonOkPath.Content = Application.Current.FindResource("Ok").ToString();
            buttonOkPath.Click += (sender, e) =>
            {
                rule.watchPath = tbPath.Text;
                CustomizableMessageBox.MessageBox.MessageText = Application.Current.FindResource("WatchFilter").ToString();
                Button buttonOkFilter = new Button();
                buttonOkFilter.Height = 30;
                buttonOkFilter.HorizontalAlignment = HorizontalAlignment.Stretch;
                buttonOkFilter.Margin = new Thickness(5);
                buttonOkFilter.Content = Application.Current.FindResource("Ok").ToString();
                buttonOkFilter.Click += (senderF, eF) =>
                {
                    rule.filter = tbFilter.Text;
                    CustomizableMessageBox.MessageBox.CloseNow();
                };
                CustomizableMessageBox.MessageBox.ButtonList = new RefreshList() { tbFilter, new ButtonSpacer(1, GridUnitType.Star, true), buttonOkFilter, buttonCancel };
            };
            CustomizableMessageBox.MessageBox.Show(new RefreshList() { tbPath, new ButtonSpacer(1, GridUnitType.Star, true), buttonOkPath, buttonCancel }, Application.Current.FindResource("WatchPath").ToString(), Application.Current.FindResource("Saving").ToString(), MessageBoxImage.Information);

            if (rule.watchPath == null || rule.filter == null)
            {
                return;
            }

            try
            {
                SetAuto(rule, ruleName);
            }
            catch (Exception ex)
            {
                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                return;
            }

            string json = JsonConvert.SerializeObject(rule);
            string fileName = $".\\Rules\\{SelectedRulesItem}.json";
            FileStream fs = null;
            try
            {
                fs = File.Create(fileName);
                fs.Close();
                StreamWriter sw = File.CreateText(fileName);
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
            }
        }

        private void UnsetAuto()
        {
            string fileName = $".\\Rules\\{SelectedRulesItem}.json";

            RunningRule rule = JsonHelper.TryDeserializeObject<RunningRule>(fileName);
            if (rule == null)
            {
                return;
            }
            rule.watchPath = null;
            rule.filter = null;

            string json = JsonConvert.SerializeObject(rule);
            FileStream fs = null;
            try
            {
                fs = File.Create(fileName);
                fs.Close();
                StreamWriter sw = File.CreateText(fileName);
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
            }

            FileSystemWatcher fileSystemWatcher = fileSystemWatcherDic.FirstOrDefault(q => q.Value == SelectedRulesItem).Key;
            if (fileSystemWatcher != null)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcherDic.Remove(fileSystemWatcher);
            }

            BtnSetAutoVisibility = Visibility.Visible;
            BtnUnsetAutoVisibility = Visibility.Hidden;
            BtnDeleteRuleIsEnabled = true;
        }
        private void TeLogPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !TeLog.IsReadOnly)
            {
                string text = TeLog.Text;
                int lastChangeLineIndex = text.LastIndexOf("\n");
                int subIndex = -1;
                if (text.Length > lastChangeLineIndex + 1)
                {
                    subIndex = lastChangeLineIndex + 1;
                }
                else
                {
                    subIndex = lastChangeLineIndex;
                }
                scanner.UpdateInput(text.Substring(subIndex).Replace(Scanner.CurrentInputMessage, ""));

                TeLog.Text += " <\n";

                TeLog.IsReadOnly = true;
            }

            if ((e.Key == Key.Back || e.Key == Key.Left) && !TeLog.IsReadOnly)
            {
                if (TeLog.SelectionStart == TeLog.Text.LastIndexOf('\n') + Scanner.CurrentInputMessage.Length + 1)
                {
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up && !TeLog.IsReadOnly)
            {
                e.Handled = true;
            }
        }

        private void TeLogTextChanged(object sender, EventArgs e)
        {
            TeLog.ScrollToEnd();
        }

        private void Stop()
        {
            Running.UserStop = true;
            TeLog.Dispatcher.Invoke(() =>
            {
                TeLog.Text += Logger.Get();
            });
        }

        private async void Start()
        {
            string paramStr = FormatTeParams();
            paramStr = ParamHelper.EncodeFromEscaped(paramStr);
            Dictionary<string, Dictionary<string, string>> paramDicEachAnalyzer = ParamHelper.GetParamDicEachAnalyzer(paramStr);
            if (paramDicEachAnalyzer == null)
            {
                return;
            }

            List<CsvExplainer> csvExplainers = new List<CsvExplainer>();
            List<Analyzer> analyzer = new List<Analyzer>();
            if (!GetCsvExplainersAndAnalyzers(TeCsvExplainersDocument.Text, TeAnalyzersDocument.Text, true, ref csvExplainers, ref analyzer))
            {
                return;
            }

            ResetLog(false);

            if (!CbExecuteInSequenceIsChecked)
            {
                _ = StartLogic(csvExplainers, analyzer, paramDicEachAnalyzer, TbBasePathText, TbOutputPathText, TbOutputNameText, false, CbExecuteInSequenceIsChecked);
                while (Running.NowRunning)
                {
                    await Task.Delay(freshInterval);
                }
            }
            else
            {
                for (int i = 0; i < csvExplainers.Count; ++i)
                {
                    if (!await StartLogic(new List<CsvExplainer> { csvExplainers[i] }, new List<Analyzer> { analyzer[i] }, paramDicEachAnalyzer, TbBasePathText, TbOutputPathText, TbOutputNameText, false, CbExecuteInSequenceIsChecked))
                    {
                        break;
                    }
                    while (Running.NowRunning)
                    {
                        await Task.Delay(freshInterval);
                    }
                }
            }
            SaveParam(paramStr, false);
        }

        // ---------------------------------------------------- Common Logic

        private string FormatTeParams()
        {
            string newStr = "";
            TeParams.Dispatcher.Invoke(() =>
            {
                newStr = $"{TeParams.Text.Replace("\r\n", "").Replace("\n", "")}";
                if (newStr != TeParams.Text)
                {
                    TeParams.Text = newStr;
                }
            });

            return newStr;
        }

        private bool CheckRule(RunningRule rule)
        {
            if (rule.watchPath != null && rule.watchPath != "" && !Directory.Exists(rule.watchPath))
            {
                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"{Application.Current.FindResource("WatchPathNotExists").ToString()}: {rule.watchPath}", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                return false;
            }
            foreach (string analyzer in rule.analyzers.Split('\n'))
            {
                if (!AnalyzersItems.Contains(analyzer))
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"{Application.Current.FindResource("Unknown").ToString()}{Application.Current.FindResource("Analyzer").ToString()}: {analyzer}", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                    return false;
                }
            }
            foreach (string csvExplainer in rule.csvExplainers.Split('\n'))
            {
                if (!CsvExplainersItems.Contains(csvExplainer))
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"{Application.Current.FindResource("Unknown").ToString()}{Application.Current.FindResource("CsvExplainer").ToString()}: {csvExplainer}", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                    return false;
                }

            }
            return true;
        }
        private void SetAuto(RunningRule rule, string ruleName)
        {
            if (!fileSystemWatcherDic.ContainsValue(ruleName))
            {
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
                fileSystemWatcher.Path = rule.watchPath;
                fileSystemWatcher.Filter = rule.filter;
                fileSystemWatcher.Deleted += FileSystemWatcherInvoke;
                fileSystemWatcher.Created += FileSystemWatcherInvoke;
                fileSystemWatcher.Changed += FileSystemWatcherInvoke;
                fileSystemWatcher.Renamed += FileSystemWatcherInvoke;
                fileSystemWatcher.IncludeSubdirectories = true;
                fileSystemWatcher.EnableRaisingEvents = true;
                fileSystemWatcherDic.Add(fileSystemWatcher, ruleName);

                BtnSetAutoVisibility = Visibility.Hidden;
                BtnUnsetAutoVisibility = Visibility.Visible;
                BtnDeleteRuleIsEnabled = false;
            }
        }

        private void FileSystemWatcherInvoke(object sender, FileSystemEventArgs e)
        {
            fileSystemWatcherInvokeThread = new Thread(async () =>
            {
                if (stopwatchBeforeFileSystemWatcherInvoke == null)
                {
                    stopwatchBeforeFileSystemWatcherInvoke = new Stopwatch();
                    stopwatchBeforeFileSystemWatcherInvoke.Start();

                    while (stopwatchBeforeFileSystemWatcherInvoke.ElapsedMilliseconds <= fileSystemWatcherInvokeDalay)
                    {
                        Thread.Sleep(freshInterval);
                    }

                    FileSystemWatcher fileSystemWatcher = (FileSystemWatcher)sender;
                    if (fileSystemWatcherDic.ContainsKey(fileSystemWatcher))
                    {
                        try
                        {
                            fileSystemWatcher.EnableRaisingEvents = false;

                            string ruleName = fileSystemWatcherDic[fileSystemWatcher];

                            RunningRule rule = JsonHelper.TryDeserializeObject<RunningRule>($".\\Rules\\{ruleName}.json");

                            if (rule == null || !CheckRule(rule))
                            {
                                SelectedRulesIndex = 0;
                                return;
                            }

                            List<CsvExplainer> csvExplainers = new List<CsvExplainer>();
                            List<Analyzer> analyzer = new List<Analyzer>();
                            if (!GetCsvExplainersAndAnalyzers(rule.csvExplainers, rule.analyzers, true, ref csvExplainers, ref analyzer))
                            {
                                return;
                            }

                            string paramStr = ParamHelper.EncodeFromEscaped(rule.param);
                            Dictionary<string, Dictionary<string, string>> paramDicEachAnalyzer = ParamHelper.GetParamDicEachAnalyzer(paramStr);
                            if (paramDicEachAnalyzer == null)
                            {
                                return;
                            }

                            ResetLog(true);

                            if (!rule.executeInSequence)
                            {
                                _ = StartLogic(csvExplainers, analyzer, paramDicEachAnalyzer, rule.basePath, rule.outputPath, rule.outputName, true, rule.executeInSequence);
                                while (Running.NowRunning)
                                {
                                    Thread.Sleep(freshInterval);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < csvExplainers.Count; ++i)
                                {
                                    if (!await StartLogic(new List<CsvExplainer> { csvExplainers[i] }, new List<Analyzer> { analyzer[i] }, paramDicEachAnalyzer, rule.basePath, rule.outputPath, rule.outputName, true, rule.executeInSequence))
                                    {
                                        break;
                                    }
                                    while (Running.NowRunning)
                                    {
                                        Thread.Sleep(freshInterval);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            while (Running.NowRunning)
                            {
                                Thread.Sleep(freshInterval);
                            }

                            stopwatchBeforeFileSystemWatcherInvoke = null;
                            fileSystemWatcher.EnableRaisingEvents = true;
                        }
                    }
                }
                else
                {
                    stopwatchBeforeFileSystemWatcherInvoke.Restart();
                }
            });
            fileSystemWatcherInvokeThread.Start();
        }

        private bool GetCsvExplainersAndAnalyzers(string csvExplainersStr, string analyzersStr, bool isAuto, ref List<CsvExplainer> csvExplainers, ref List<Analyzer> analyzers)
        {
            List<String> csvExplainersList = csvExplainersStr.Split('\n').Where(str => str.Trim() != "").ToList();
            List<String> analyzersList = analyzersStr.Split('\n').Where(str => str.Trim() != "").ToList();
            if (csvExplainersList.Count != analyzersList.Count || csvExplainersList.Count == 0)
            {
                return false;
            }
            foreach (string name in csvExplainersList)
            {
                CsvExplainer csvExplainer = null;
                csvExplainer = JsonHelper.TryDeserializeObject<CsvExplainer>($".\\CsvExplainers\\{name}.json", isAuto);
                if (csvExplainer == null)
                {
                    return false;
                }

                if (csvExplainer.pathes == null || csvExplainer.pathes.Count == 0)
                {
                    csvExplainer.pathes = new List<string>();
                    csvExplainer.pathes.Add("");
                }
                csvExplainers.Add(csvExplainer);
            }

            string resPath = TbOutputPathText.Replace("\\", "/");
            string resFileName = TbOutputNameText;
            string fullPath;
            if (resPath.EndsWith("/"))
            {
                fullPath = $"{resPath}{resFileName}.csv";
            }
            else
            {
                fullPath = $"{resPath}/{resFileName}.csv";
            }
            foreach (string name in analyzersList)
            {
                Analyzer analyzer = null;
                analyzer = JsonHelper.TryDeserializeObject<Analyzer>($".\\Analyzers\\{name}.json", isAuto);
                if (analyzer == null)
                {
                    return false;
                }
                analyzer.outputter.defaultPath = fullPath;
                analyzers.Add(analyzer);
            }
            return true;
        }

        private void ResetLog(bool isAuto)
        {
            if (!isAuto)
            {
                TeLog.Dispatcher.Invoke(() =>
                {
                    TeLog.Text = "";
                });
                Logger.Clear();
            }
            else
            {
                Logger.Print("---- AUTO ANALYZE ----");
            }
        }

        private void ThreadCallback<T>(ExecuteResult<T> res)
        {
            ConcurrentDictionary<ReadFileReturnType, Object> methodResult = res.Result as ConcurrentDictionary<ReadFileReturnType, Object>;

            if (methodResult.ContainsKey(ReadFileReturnType.FILEPATH))
            {
                string filePath = methodResult[ReadFileReturnType.FILEPATH].ToString();

                currentAnalizingDictionary.TryRemove(filePath, out _);
            }
        }


        private async Task<bool> StartLogic(List<CsvExplainer> csvExplainers, List<Analyzer> analyzers, Dictionary<string, Dictionary<string, string>> paramDicEachAnalyzer, string basePath, string outputPath, string outputName, bool isAuto, bool isExecuteInSequence)
        {
            Dictionary<CsvExplainer, List<string>> filePathListDic = new Dictionary<CsvExplainer, List<string>>();

            Running.NowRunning = true;
            Running.UserStop = false;

            analyzeCsvInvokeCount = 0;
            currentAnalizingDictionary = new ConcurrentDictionary<string, long>();
            GlobalObjects.GlobalObjects.ClearGlobalParamDic();

            GlobalDic.Reset();
            Logger.IsOutputMethodNotFoundWarning = true;
            Scanner.ResetAll();

            runNotSuccessed = false;

            RenewSmartThreadPoolAnalyze();

            Dictionary<Analyzer, Tuple<ExecOption, CsvExplainer>> compilerDic = new Dictionary<Analyzer, Tuple<ExecOption, CsvExplainer>>();
            int totalCount = 0;
            for (int i = 0; i < csvExplainers.Count; ++i)
            {
                long startTime = GetNowSs();

                CsvExplainer csvExplainer = csvExplainers[i];
                Analyzer analyzer = analyzers[i];

                if (analyzer.globalizationSetter != null)
                {
                    analyzer.globalizationSetter.currentLanguageName = language;
                }

                List<string> allFilePathList = new List<string>();
                foreach (string str in csvExplainer.pathes)
                {
                    List<string> filePathList = new List<string>();
                    string basePathTemp = "";
                    if (Path.IsPathRooted(str))
                    {
                        basePathTemp = str.Trim();
                    }
                    else
                    {
                        basePathTemp = Path.Combine(basePath.Trim(), str.Trim());
                    }

                    if (basePathTemp.Trim() != "" && !Directory.Exists(basePathTemp))
                    {
                        if (!isAuto)
                        {
                            CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, Application.Current.FindResource("PathNotExists").ToString(), Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                        }
                        else
                        {
                            Logger.Error(Application.Current.FindResource("PathNotExists").ToString());
                        }
                        FinishRunning(true);
                        return false;
                    }

                    FileHelper.FileTraverse(isAuto, basePathTemp, csvExplainer, filePathList);
                    allFilePathList.AddRange(filePathList);
                }
                if (csvExplainer.fileNames.Key == FindingMethod.SAME)
                {
                    foreach (string filename in csvExplainer.fileNames.Value)
                    {
                        if (File.Exists(filename) && !allFilePathList.Contains(filename))
                        {
                            allFilePathList.Add(filename);
                        }
                    }
                }
                filePathListDic.Add(csvExplainer, allFilePathList);

                ExecOption execOption = new ExecOption();
                execOption.ExtraDllFolderList = new List<string>
                {
                    Path.Combine(Environment.CurrentDirectory, "Dlls")
                };
                execOption.ClassName = "AnalyzeCode.Analyze";
                InstanceObject instanceObj = null;
                try
                {
                    instanceObj = new InstanceObject(analyzer.code, execOption);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    runNotSuccessed = true;
                    Stop();
                }
                
                if (instanceObj == null)
                {
                    FinishRunning(true);
                    return false;
                }

                execOption.InstanceObject = instanceObj;

                GlobalObjects.GlobalObjects.SetGlobalParam(execOption, new Object());
                Tuple<ExecOption, CsvExplainer> cresultTuple = new Tuple<ExecOption, CsvExplainer>(execOption, csvExplainer);
                compilerDic.Add(analyzer, cresultTuple);

                runBeforeAnalyzeCsvThread = new Thread(() => RunBeforeAnalyzeCsv(execOption, ParamHelper.MergePublicParam(paramDicEachAnalyzer, analyzer.name), analyzer, allFilePathList, isExecuteInSequence));
                runBeforeAnalyzeCsvThread.Start();

                while (runBeforeAnalyzeCsvThread.IsAlive)
                {
                    if (Running.UserStop)
                    {
                        runBeforeAnalyzeCsvThread.Interrupt();
                        runBeforeAnalyzeCsvThread.Join();
                        FinishRunning(true);
                        return false;
                    }
                    long timeCostSs = GetNowSs() - startTime;
                    if (enableTimeoutSetting && perTimeoutLimitAnalyze > 0 && timeCostSs >= perTimeoutLimitAnalyze)
                    {
                        runBeforeAnalyzeCsvThread.Interrupt();
                        runBeforeAnalyzeCsvThread.Join();
                        if (!isAuto)
                        {
                            CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"RunBeforeAnalyzeCsv\n{Application.Current.FindResource("Timeout").ToString()}. \n{perTimeoutLimitAnalyze / 1000.0}(s)", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                        }
                        else
                        {
                            Logger.Error($"RunBeforeAnalyzeCsv\n{Application.Current.FindResource("Timeout").ToString()}. \n{perTimeoutLimitAnalyze / 1000.0}(s)");
                        }
                        FinishRunning(true);
                        return false;
                    }
                    await Task.Delay(freshInterval);
                }

                List<string> filePathListFromCsvExplainer = filePathListDic[csvExplainer];
                totalCount += filePathListFromCsvExplainer.Count;

                int filesCount = filePathListFromCsvExplainer.Count;
                foreach (string filePath in filePathListFromCsvExplainer)
                {
                    List<object> readFileParams = new List<object>();
                    readFileParams.Add(filePath);
                    readFileParams.Add(i);
                    readFileParams.Add(csvExplainer);
                    readFileParams.Add(analyzer);
                    readFileParams.Add(ParamHelper.MergePublicParam(paramDicEachAnalyzer, analyzer.name));
                    readFileParams.Add(isExecuteInSequence);
                    readFileParams.Add(execOption);
                    powerPool.QueueWorkItem(new Func<List<object>, object>(ReadFile), readFileParams, ThreadCallback);
                }
            }

            long startSs = GetNowSs();
            // powerPool.Wait();
            while (powerPool.RunningThreadCount > 0)
            {
                try
                {
                    long nowSs = GetNowSs();
                    long totalTimeCostSs = nowSs - startSs;
                    if (Running.UserStop || (enableTimeoutSetting && totalTimeoutLimitAnalyze > 0 && totalTimeCostSs >= totalTimeoutLimitAnalyze))
                    {
                        powerPool.Stop();
                        if (enableTimeoutSetting && totalTimeoutLimitAnalyze > 0 && totalTimeCostSs >= totalTimeoutLimitAnalyze)
                        {
                            if (!isAuto)
                            {
                                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"{Application.Current.FindResource("TotalTimeout").ToString()}. \n{totalTimeoutLimitAnalyze / 1000.0}(s)", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                            }
                            else
                            {
                                Logger.Error($"{Application.Current.FindResource("TotalTimeout").ToString()}. \n{totalTimeoutLimitAnalyze / 1000.0}(s)");
                            }

                        }
                        FinishRunning(true);
                        return false;
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{Application.Current.FindResource("Analyzing")}\n");

                    ICollection<string> keys = currentAnalizingDictionary.Keys;
                    foreach (string key in keys)
                    {
                        long value = new long();
                        if (currentAnalizingDictionary.TryGetValue(key, out value))
                        {
                            long timeCostSs = GetNowSs() - value;
                            if (enableTimeoutSetting && perTimeoutLimitAnalyze > 0 && timeCostSs >= perTimeoutLimitAnalyze)
                            {
                                powerPool.Stop();
                                if (!isAuto)
                                {
                                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"{key.Split('|')[1]}\n{Application.Current.FindResource("Timeout").ToString()}. \n{perTimeoutLimitAnalyze / 1000.0}(s)", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                                }
                                else
                                {
                                    Logger.Error($"{key.Split('|')[1]}\n{Application.Current.FindResource("Timeout").ToString()}. \n{perTimeoutLimitAnalyze / 1000.0}(s)");
                                }
                                FinishRunning(true);
                                return false;
                            }
                            sb.Append(key.Substring(key.LastIndexOf('\\') + 1)).Append(" [").Append((timeCostSs / 1000.0).ToString("0.0")).Append("s]\n");
                        }
                    }

                    LProcessContent = $"{Application.Current.FindResource("RunningThreads").ToString()}: {powerPool.RunningThreadCount} | {Application.Current.FindResource("WaitingThreads").ToString()}: {powerPool.WaitingThreadCount}";
                    TbStatusText = $"{sb}";
                    await Task.Delay(freshInterval);
                }
                catch (Exception e)
                {
                    powerPool.Stop();
                    Logger.Error($"{Application.Current.FindResource("ExceptionHasBeenThrowed").ToString()} \n{e.Message}");
                    FinishRunning(true);
                    return false;
                }
            }
            powerPool.Stop();

            foreach (Analyzer analyzer in compilerDic.Keys)
            {
                long startTime = GetNowSs();

                ExecOption execOption = compilerDic[analyzer].Item1;
                CsvExplainer csvExplainer = compilerDic[analyzer].Item2;
                runEndThread = new Thread(() => RunEnd(execOption, ParamHelper.MergePublicParam(paramDicEachAnalyzer, analyzer.name), analyzer, filePathListDic[csvExplainer], isExecuteInSequence));
                runEndThread.Start();
                while (runEndThread.IsAlive)
                {
                    if (Running.UserStop)
                    {
                        runEndThread.Interrupt();
                        runEndThread.Join();
                        FinishRunning(true);
                        return false;
                    }
                    long timeCostSs = GetNowSs() - startTime;
                    if (enableTimeoutSetting && perTimeoutLimitAnalyze > 0 && timeCostSs >= perTimeoutLimitAnalyze)
                    {
                        runEndThread.Interrupt();
                        runEndThread.Join();

                        if (!isAuto)
                        {
                            CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"RunEnd\n{Application.Current.FindResource("Timeout").ToString()}. \n{perTimeoutLimitAnalyze / 1000.0}(s)", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                        }
                        else
                        {
                            Logger.Error($"RunEnd\n{Application.Current.FindResource("Timeout").ToString()}. \n{perTimeoutLimitAnalyze / 1000.0}(s)");
                        }
                        FinishRunning(true);
                        return false;
                    }
                    await Task.Delay(freshInterval);
                }
            }

            foreach (Analyzer analyzer in compilerDic.Keys)
            {
                CsvWriter csvWriter = new CsvWriter(analyzer.outputter.csvOption);
                foreach (string key in analyzer.outputter.csvDatasDic.Keys)
                {
                    FileHelper.SaveCsv(isAuto, key, csvWriter, CbIsAutoOpenIsChecked, isExecuteInSequence, CbIsShowSavedMessageBox, analyzer.outputter.csvDatasDic[key]);
                }
            }

            if (runNotSuccessed)
            {
                FinishRunning(true);
                return false;
            }
            TbStatusText = "";

            TeLog.Dispatcher.Invoke(() =>
            {
                TeLog.Text += Logger.Get();
            });

            FinishRunning(false);

            return true;
        }

        private void SaveParam(string paramStr, bool isAuto, bool isLocked = false)
        {
            String paramStrFromFile = File.ReadAllText(".\\Params.txt");
            List<string> paramsList = paramStrFromFile.Split('\n').ToList<string>();
            List<string> unLockedList = new List<string>();
            List<string> lockedList = new List<string>();
            List<string> addList = unLockedList;
            foreach (string param in paramsList)
            {
                string trimedParam = param.Trim();
                if (trimedParam == "[Lock]")
                {
                    addList = lockedList;
                    continue;
                }
                addList.Add(trimedParam);
            }
            
            if (unLockedList.Count >= 5 && !paramsList.Contains(paramStr) && !isLocked)
            {
                unLockedList.RemoveAt(4);
            }
            if (unLockedList.Contains(paramStr))
            {
                unLockedList.Remove(paramStr);
            }
            if (!lockedList.Contains(paramStr) && !isLocked)
            {
                unLockedList.Insert(0, paramStr);
            }
            if (!lockedList.Contains(paramStr) && isLocked)
            {
                lockedList.Insert(0, paramStr);
            }

            List<string> newParamsList = new List<string>(unLockedList);
            newParamsList.Add("[Lock]");
            newParamsList.AddRange(lockedList);
            newParamsList.RemoveAll(s => s == "");

            try
            {
                File.WriteAllLines(".\\Params.txt", newParamsList);
            }
            catch (Exception ex)
            {
                if (!isAuto)
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, ex.Message, Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                }
                else
                {
                    Logger.Error(ex.Message);
                }
            }
        }

        private void WhenRunning()
        {
            while (!windowsClosing)
            {
                TeLog.Dispatcher.Invoke(() =>
                {
                    string logTemp = Logger.Get();
                    if (!String.IsNullOrEmpty(logTemp))
                    {
                        if (TeLog.Text.Length > TeLog.Text.LastIndexOf('\n') + 1)
                        {
                            TeLog.Text = TeLog.Text.Remove(TeLog.Text.LastIndexOf('\n') + 1) + logTemp;
                        }
                        else
                        {
                            TeLog.Text += logTemp;
                        }
                    }

                    if (Running.NowRunning && Scanner.InputLock)
                    {
                        string message = Scanner.CurrentInputMessage;
                        if (message != "")
                        {
                            string logText = TeLog.Text;
                            int index = logText.LastIndexOf(message);
                            if (index != -1 && logText.IndexOf('\n', index) != -1
                            && (index - 1 < 0 || (index - 1 >= 0 && logText[index - 1] == '\n') && (logText.Length - 1 < index + logText.Length || (logText.Length - 1 >= index + logText.Length && logText[index + logText.Length] == '\n'))))
                            {
                                TeLog.Text += message;
                                TeLog.Select(TeLog.Text.Length, 0);
                            }
                            else if (index == -1)
                            {
                                TeLog.Text += message;
                                TeLog.Select(TeLog.Text.Length, 0);
                            }
                        }

                        TeLog.IsReadOnly = false;
                        if (TeLog.SelectionStart <= TeLog.Text.LastIndexOf('\n') + Scanner.CurrentInputMessage.Length)
                        {
                            TeLog.Select(TeLog.Text.Length, 0);
                        }
                    }
                    else
                    {
                        TeLog.IsReadOnly = true;
                    }

                    if (this.powerPool != null)
                    {
                        if (Running.NowRunning || this.powerPool.RunningThreadCount > 0)
                        {
                            BtnStartIsEnabled = false;
                            BtnStopIsEnabled = true;
                        }
                        else
                        {
                            BtnStartIsEnabled = true;
                            BtnStopIsEnabled = false;
                        }
                    }
                });

                Thread.Sleep(freshInterval);
            }
        }

        private void FinishRunning(bool isInterrupted)
        {
            CheckAndCloseThreads(false);
            Running.NowRunning = false;
            if (isInterrupted)
            {
                Logger.Error(Application.Current.FindResource("RunFailed").ToString());
            }

            TbStatusText = "";
            LProcessContent = "";
        }

        private void RunFunction(ExecOption execOption, string analyzerName, string className, string functionName, object[] objList, int globalParamIndex)
        {
            try
            {
                ExecOption execOptionTemp = execOption.Copy();
                execOptionTemp.MethodName = functionName;
                execOptionTemp.ParamList = objList;
                ScriptExecutor.Exec(execOptionTemp);
                GlobalObjects.GlobalObjects.SetGlobalParam(execOption, objList[globalParamIndex]);
            }
            catch (MissingMethodException e)
            {
                if (Logger.IsOutputMethodNotFoundWarning)
                {
                    Logger.Warn(Application.Current.FindResource("MethodNotFound").ToString().Replace("{0}", functionName));
                }
                return;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Logger.Error($"\n    {e.InnerException.Message}\n    {analyzerName}.{functionName}(): \n{e.InnerException.StackTrace}");
                }
                else
                {
                    Logger.Error($"\n    {e.Message}\n    {analyzerName}.{functionName}(): \n{e.StackTrace}");
                }
                runNotSuccessed = true;
                Stop();
            }
        }

        private void RunBeforeAnalyzeCsv(ExecOption execOption, Param param, Analyzer analyzer, List<String> allFilePathList, bool isExecuteInSequence)
        {
            object[] objList = new object[] { param, GlobalObjects.GlobalObjects.GetGlobalParam(execOption), allFilePathList, analyzer.globalizationSetter, isExecuteInSequence, analyzer.outputter };
            RunFunction(execOption, analyzer.name, "AnalyzeCode.Analyze", "RunBeforeAnalyzeCsv", objList, 1);
        }

        private void RunEnd(ExecOption execOption, Param param, Analyzer analyzer, List<String> allFilePathList, bool isExecuteInSequence)
        {
            object[] objList = new object[] { param, GlobalObjects.GlobalObjects.GetGlobalParam(execOption), allFilePathList, analyzer.globalizationSetter, isExecuteInSequence, analyzer.outputter };
            RunFunction(execOption, analyzer.name, "AnalyzeCode.Analyze", "RunEnd", objList, 1);
        }

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        public static bool IsFileInUse(string fileName)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = CreateFile(fileName, 0, 0, IntPtr.Zero, 3, 0x80, IntPtr.Zero);
                return handle.ToInt32() == -1;
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    CloseHandle(handle);
                }
            }
        }

        private ConcurrentDictionary<ReadFileReturnType, object> ReadFile(List<object> readFileParams)
        {
            string filePath = (string)readFileParams[0];
            int csvExplainerIndex = (int)readFileParams[1];
            CsvExplainer csvExplainer = (CsvExplainer)readFileParams[2];
            Analyzer analyzer = (Analyzer)readFileParams[3];
            Param param = (Param)readFileParams[4];
            bool isExecuteInSequence = (bool)readFileParams[5];
            ExecOption execOption = (ExecOption)readFileParams[6];

            ConcurrentDictionary<ReadFileReturnType, object> methodResult = new ConcurrentDictionary<ReadFileReturnType, object>();
            methodResult.AddOrUpdate(ReadFileReturnType.ANALYZER, analyzer, (key, oldValue) => null);

            if (IsFileInUse(filePath))
            {
                Logger.Error(Application.Current.FindResource("FileIsInUse").ToString().Replace("{0}", filePath));
                return methodResult;
            }

            currentAnalizingDictionary.AddOrUpdate(csvExplainerIndex + "|" + filePath, GetNowSs(), (key, oldValue) => GetNowSs());

            try
            {
                CsvReader csv = new CsvReader(csvExplainer.inputOption, filePath);

                Dictionary<string, string> recordDic;
                int count = 0;
                while ((recordDic = csv.ReadNext()) != null)
                {
                    Analyze(recordDic, filePath, analyzer, param, isExecuteInSequence, execOption);
                    ++count;
                }

                AnalyzeRecords(csv.GetLists(), filePath, analyzer, param, isExecuteInSequence, execOption);

                csv.Dispose();
            }
            catch
            {
                currentAnalizingDictionary.TryRemove(csvExplainerIndex + "|" + filePath, out _);
                Logger.Error(Application.Current.FindResource("FileIsDamaged").ToString().Replace("{0}", filePath));
                return methodResult;
            }

            methodResult.AddOrUpdate(ReadFileReturnType.FILEPATH, csvExplainerIndex + "|" + filePath, (key, oldValue) => null);
            return methodResult;
        }

        private void LoadFiles()
        {
            CsvExplainersItems = FileHelper.GetCsvExplainersList();
            AnalyzersItems = FileHelper.GetAnalyzersList();
            RuleItems = FileHelper.GetRulesList();
            ParamsItems = FileHelper.GetParamsList();
        }

        private long GetNowSs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private void Analyze(Dictionary<string, string> record, string filePath, Analyzer analyzer, Param param, bool isExecuteInSequence, ExecOption execOption)
        {
            ++analyzeCsvInvokeCount;
            object[] objList = new object[] { param, record, filePath, GlobalObjects.GlobalObjects.GetGlobalParam(execOption), analyzer.globalizationSetter, isExecuteInSequence, analyzeCsvInvokeCount, analyzer.outputter };
            RunFunction(execOption, analyzer.name, "AnalyzeCode.Analyze", "AnalyzePerRecord", objList, 3);
        }

        private void AnalyzeRecords(IEnumerable<IEnumerable<string>> records, string filePath, Analyzer analyzer, Param param, bool isExecuteInSequence, ExecOption execOption)
        {
            ++analyzeCsvInvokeCount;
            object[] objList = new object[] { param, records, filePath, GlobalObjects.GlobalObjects.GetGlobalParam(execOption), analyzer.globalizationSetter, isExecuteInSequence, analyzeCsvInvokeCount, analyzer.outputter };
            RunFunction(execOption, analyzer.name, "AnalyzeCode.Analyze", "AnalyzeRecords", objList, 3);
        }

        private void RenewSmartThreadPoolAnalyze()
        {
            if (powerPool != null)
            {
                powerPool.Stop();
            }

            ThreadPoolOption threadPoolOption = new ThreadPoolOption();
            if (maxThreadCount > 0)
            {
                threadPoolOption.MaxThreads = maxThreadCount;
            }
            powerPool = new PowerPool(threadPoolOption);

            Running.Controller = new PowerPoolController(powerPool);
        }

        private void SetAutoStatusAll()
        {
            List<String> rulesList = Directory.GetFiles(".\\Rules", "*.json").ToList();
            foreach (string path in rulesList)
            {
                RunningRule rule = JsonHelper.TryDeserializeObject<RunningRule>(path);
                if (rule == null)
                {
                    continue;
                }

                if (rule.watchPath != null && rule.watchPath != "")
                {
                    if (!CheckRule(rule))
                    {
                        continue;
                    }

                    SetAuto(rule, Path.GetFileNameWithoutExtension(path));
                }
            }
        }

        private void CheckAndCloseThreads(bool isCloseWindow)
        {
            if (powerPool != null)
            {
                try
                {
                    powerPool.Stop();
                }
                catch
                {
                    // DO NOTHING
                }
            }
            if (runBeforeAnalyzeCsvThread != null && runBeforeAnalyzeCsvThread.IsAlive)
            {
                runBeforeAnalyzeCsvThread.Interrupt();
                runBeforeAnalyzeCsvThread.Join();
            }
            if (runEndThread != null && runEndThread.IsAlive)
            {
                runEndThread.Interrupt();
                runEndThread.Join();
            }
            if (isCloseWindow)
            {
                if (fileSystemWatcherInvokeThread != null && fileSystemWatcherInvokeThread.IsAlive)
                {
                    fileSystemWatcherInvokeThread.Interrupt();
                    fileSystemWatcherInvokeThread.Join();
                }
                runningThread.Interrupt();
                runningThread.Join();
            }
        }

        private void ChangeLanguage(string language, List<ResourceDictionary> dictionaryList)
        {
            string requestedCulture = string.Format(@"Resources\StringResource.{0}.xaml", language);
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Equals(requestedCulture));
            if (resourceDictionary == null)
            {
                requestedCulture = @"Resources\StringResource.zh-CN.xaml";
                resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            }
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }

        private void ActualApplicationThemeChanged(ThemeManager themeManager, object obj)
        {
            GlobalObjects.GlobalObjects.ClearPropertiesSetter();

            GlobalObjects.Theme.SetTheme();
            ThemeBackground = Theme.ThemeBackground;
            ThemeControlBackground = Theme.ThemeControlBackground;
            ThemeControlFocusBackground = Theme.ThemeControlFocusBackground;
            ThemeControlForeground = Theme.ThemeControlForeground;
            ThemeControlBorderBrush = Theme.ThemeControlBorderBrush;
            ThemeControlHoverBorderBrush = Theme.ThemeControlHoverBorderBrush;

            teParams.Background = ThemeControlBackground;
            teParams.Foreground = ThemeControlForeground;
            Border border = (Border)((ContentControl)((TextEditor)teParams).Parent).Parent;
            border.Background = ThemeControlBackground;

            teLog.Background = ThemeBackground;
            teLog.Foreground = ThemeControlForeground;
        }
    }

}
