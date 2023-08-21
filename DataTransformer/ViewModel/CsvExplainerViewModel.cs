using CustomizableMessageBox;
using DataTransformer.View;
using DataTransformer.ViewModel;
using DataTransformer.Helper;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static CustomizableMessageBox.MessageBox;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DataTransformer.ViewModel
{
    [SupportedOSPlatform("windows7.0")]
    class CsvExplainerViewModel : ObservableObject, IDropTarget
    {
        private CsvExplainer csvExplainer;

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

        private string windowName = "CsvExplainerEditor";
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

        private int selectedCsvExplainersIndex = 0;
        public int SelectedCsvExplainersIndex
        {
            get { return selectedCsvExplainersIndex; }
            set
            {
                SetProperty<int>(ref selectedCsvExplainersIndex, value);
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

        private bool btnDeleteIsEnabled = false;
        public bool BtnDeleteIsEnabled
        {
            get { return btnDeleteIsEnabled; }
            set
            {
                SetProperty<bool>(ref btnDeleteIsEnabled, value);
            }
        }

        private string tbPathsText = "";
        public string TbPathsText
        {
            get { return tbPathsText; }
            set
            {
                SetProperty<string>(ref tbPathsText, value);
            }
        }

        private int selectedFileNamesTypeIndex = 0;
        public int SelectedFileNamesTypeIndex
        {
            get { return selectedFileNamesTypeIndex; }
            set
            {
                SetProperty<int>(ref selectedFileNamesTypeIndex, value);
            }
        }

        private string tbFileNamesText = "";
        public string TbFileNamesText
        {
            get { return tbFileNamesText; }
            set
            {
                SetProperty<string>(ref tbFileNamesText, value);
            }
        }

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand KeyBindingSaveCommand { get; set; }
        public ICommand KeyBindingRenameSaveCommand { get; set; }
        public ICommand CbCsvExplainersPreviewMouseLeftButtonDownCommand { get; set; }
        public ICommand CbCsvExplainersSelectionChangedCommand { get; set; }
        public ICommand BtnDeleteClickCommand { get; set; }
        public ICommand BtnSetOptionClickCommand { get; set; }
        public ICommand BtnClearTempClickCommand { get; set; }
        public ICommand BtnSaveClickCommand { get; set; }
        public CsvExplainerViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            KeyBindingSaveCommand = new RelayCommand(SaveByKeyDown);
            KeyBindingRenameSaveCommand = new RelayCommand(RenameSaveByKeyDown);
            CbCsvExplainersPreviewMouseLeftButtonDownCommand = new RelayCommand(CbCsvExplainersPreviewMouseLeftButtonDown);
            CbCsvExplainersSelectionChangedCommand = new RelayCommand(CbCsvExplainersSelectionChanged);
            BtnDeleteClickCommand = new RelayCommand(BtnDeleteClick);
            BtnSetOptionClickCommand = new RelayCommand(BtnSetOptionClick);
            BtnClearTempClickCommand = new RelayCommand(BtnClearTempClick);
            BtnSaveClickCommand = new RelayCommand(BtnSaveClick);

            this.csvExplainer = new CsvExplainer();
        }

        private void WindowLoaded(RoutedEventArgs e)
        {
            Window window = (Window)e.Source;

            double width = IniHelper.GetWindowSize(WindowName).X;
            double height = IniHelper.GetWindowSize(WindowName).Y;
            if (width > 0)
            {
                window.Width = width;
            }

            if (height > 0)
            {
                window.Height = height;
            }
        }

        private void WindowClosing(CancelEventArgs e)
        {
            IniHelper.SetWindowSize(WindowName, new Point(WindowWidth, WindowHeight));
        }

        private void SaveByKeyDown()
        {
            if (SelectedCsvExplainersIndex >= 1)
            {
                Save(false);
            }
            else
            {
                Save(true);
            }
        }

        private void RenameSaveByKeyDown()
        {
            Save(true);
        }

        private void CbCsvExplainersPreviewMouseLeftButtonDown()
        {
            try
            {
                if (!Directory.Exists(".\\CsvExplainers"))
                {
                    Directory.CreateDirectory(".\\CsvExplainers");
                }
            }
            catch (Exception ex)
            {
                CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, $"{Application.Current.FindResource("FailedToCreateANewFolder").ToString()}\n{ex.Message}", Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                return;
            }

            List<String> csvExplainersList = Directory.GetFiles(".\\CsvExplainers", "*.json").ToList();
            csvExplainersList.Insert(0, "");
            for (int i = 0; i < csvExplainersList.Count; ++i)
            {
                String str = csvExplainersList[i];
                csvExplainersList[i] = str.Substring(str.LastIndexOf('\\') + 1);
                if (csvExplainersList[i].Contains('.'))
                {
                    csvExplainersList[i] = csvExplainersList[i].Substring(0, csvExplainersList[i].LastIndexOf('.'));
                }
            }

            CsvExplainersItems = csvExplainersList;
        }

        private void CbCsvExplainersSelectionChanged()
        {
            BtnDeleteIsEnabled = SelectedCsvExplainersIndex >= 1 ? true : false;

            TbPathsText = "";
            SelectedFileNamesTypeIndex = 0;
            TbFileNamesText = "";

            if (SelectedCsvExplainersIndex == 0)
            {
                return;
            }
            this.csvExplainer = JsonHelper.TryDeserializeObject<CsvExplainer>($".\\CsvExplainers\\{SelectedCsvExplainersItem}.json");
            if (csvExplainer == null)
            {
                SelectedCsvExplainersIndex = 0;
                return;
            }
            foreach (String str in csvExplainer.pathes)
            {
                TbPathsText += $"{str}\n";
            }
            if (csvExplainer.fileNames.Key == FindingMethod.SAME)
            {
                SelectedFileNamesTypeIndex = 0;
            }
            else if (csvExplainer.fileNames.Key == FindingMethod.CONTAIN)
            {
                SelectedFileNamesTypeIndex = 1;
            }
            else if (csvExplainer.fileNames.Key == FindingMethod.REGEX)
            {
                SelectedFileNamesTypeIndex = 2;
            }
            else if (csvExplainer.fileNames.Key == FindingMethod.ALL)
            {
                SelectedFileNamesTypeIndex = 3;
            }
            foreach (String str in csvExplainer.fileNames.Value)
            {
                TbFileNamesText += $"{str}\n";
            }
        }

        private void BtnDeleteClick()
        {
            String path = $"{System.Environment.CurrentDirectory}\\CsvExplainers\\{SelectedCsvExplainersItem}.json";
            MessageBoxResult result = CustomizableMessageBox.MessageBox.Show($"{Application.Current.FindResource("Delete").ToString()}\n{path}", Application.Current.FindResource("Warning").ToString(), MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
            {
                File.Delete(path);
                SelectedCsvExplainersIndex = 0;
            }
        }

        private void BtnSetOptionClick()
        {
            CsvOptionEditor csvOptionEditor = new CsvOptionEditor(this.csvExplainer.inputOption, true);
            if (this.csvExplainer != null)
            {
                if ((bool)csvOptionEditor.ShowDialog())
                {
                    this.csvExplainer.inputOption = ((CsvOptionViewModel)csvOptionEditor.DataContext).inputOption;
                }
            }
        }

        private void BtnClearTempClick()
        {
            String path = $"{System.Environment.CurrentDirectory}\\CsvExplainers";
            string[] files = Directory.GetFiles(path, "*.json");
            List<string> deleteList = new List<string>();
            string deleteStr = "";
            foreach (string file in files)
            {
                if (new FileInfo(file).Name.StartsWith("Temp_Csv_Explainer_"))
                {
                    deleteList.Add(file);
                    deleteStr = $"{deleteStr}\n{file}";
                }
            }

            if (deleteList.Count == 0)
            {
                return;
            }

            MessageBoxResult result = CustomizableMessageBox.MessageBox.Show(Application.Current.FindResource("ToBeDeletedSoon").ToString().Replace("{0}", deleteStr), Application.Current.FindResource("Warning").ToString(), MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            foreach (string deletePath in deleteList)
            {
                if (Path.GetFileNameWithoutExtension(deletePath) == SelectedCsvExplainersItem)
                {
                    SelectedCsvExplainersIndex = 0;
                }
            }

            foreach (string deletePath in deleteList)
            {
                File.Delete(deletePath);
            }
        }

        private void BtnSaveClick()
        {
            Save(true);
        }

        public void DragEnter(IDropInfo dropInfo)
        {
            // throw new NotImplementedException();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Copy;
        }

        public void DragLeave(IDropInfo dropInfo)
        {
            // throw new NotImplementedException();
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo?.VisualSource is null
                && dropInfo.Data is DataObject dataObject
                && dataObject.GetDataPresent(DataFormats.FileDrop)
                && dataObject.ContainsFileDropList())
            {
                string parentName = ((TextBox)((Grid)dropInfo.TargetScrollViewer.Parent).TemplatedParent).Name;
                if (parentName == "tb_paths" || parentName == "tb_filenames")
                {
                    string msg = dataObject.GetFileDropList()[0];
                    Clipboard.SetText(msg);
                    CustomizableMessageBox.MessageBox.Show(GlobalObjects.GlobalObjects.GetPropertiesSetterWithTimmer(), new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, msg, Application.Current.FindResource("Copied").ToString(), MessageBoxImage.Information);
                }
            }
            else
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }

        // ---------------------------------------------------- Common Logic
        private List<String> StringListDeteleBlank(List<String> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                list[i] = list[i].Trim();
                if (list[i].Length == 0)
                {
                    list.RemoveAt(i);
                    --i;
                }
            }
            return list;
        }

        private void Save(bool isRename)
        {
            TextBox tbName = new TextBox();
            tbName.Margin = new Thickness(5);
            tbName.Height = 30;
            tbName.VerticalContentAlignment = VerticalAlignment.Center;

            string newName = "";
            if (SelectedCsvExplainersIndex >= 1)
            {
                newName = SelectedCsvExplainersItem;
            }
            if (isRename)
            {
                if (SelectedCsvExplainersIndex >= 1)
                {
                    tbName.Text = $"Copy Of {SelectedCsvExplainersItem}";
                }
                int result = CustomizableMessageBox.MessageBox.Show(new RefreshList() { tbName, Application.Current.FindResource("Ok").ToString(), Application.Current.FindResource("Cancel").ToString() }, Application.Current.FindResource("Name").ToString(), Application.Current.FindResource("Saving").ToString(), MessageBoxImage.Information);

                if (result != 1)
                {
                    return;
                }

                if (tbName.Text == "")
                {
                    CustomizableMessageBox.MessageBox.Show(new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, Application.Current.FindResource("FileNameEmptyError").ToString(), Application.Current.FindResource("Error").ToString(), MessageBoxImage.Error);
                    return;
                }

                newName = tbName.Text;
            }

            CsvExplainer csvExplainer = new CsvExplainer();
            csvExplainer.pathes = StringListDeteleBlank(TbPathsText.Split('\n').ToList());

            FindingMethod fileNamesFindingMethod = new FindingMethod();
            if (SelectedFileNamesTypeIndex == 0)
            {
                fileNamesFindingMethod = FindingMethod.SAME;
            }
            else if (SelectedFileNamesTypeIndex == 1)
            {
                fileNamesFindingMethod = FindingMethod.CONTAIN;
            }
            else if (SelectedFileNamesTypeIndex == 2)
            {
                fileNamesFindingMethod = FindingMethod.REGEX;
            }
            else if (SelectedFileNamesTypeIndex == 3)
            {
                fileNamesFindingMethod = FindingMethod.ALL;
            }
            KeyValuePair<FindingMethod, List<string>> fileNames = new KeyValuePair<FindingMethod, List<string>>(fileNamesFindingMethod, StringListDeteleBlank(TbFileNamesText.Split('\n').ToList()));
            csvExplainer.fileNames = fileNames;

            csvExplainer.inputOption = this.csvExplainer.inputOption;

            FileHelper.SavaCsvExplainerJson(newName, csvExplainer, true);

            CustomizableMessageBox.MessageBox.Show(GlobalObjects.GlobalObjects.GetPropertiesSetterWithTimmer(), new RefreshList { new ButtonSpacer(), Application.Current.FindResource("Ok").ToString() }, Application.Current.FindResource("SuccessfullySaved").ToString(), Application.Current.FindResource("Save").ToString(), MessageBoxImage.Information);
        }
    }
}
