using CsvTool;
using DataTransformer;
using DataTransformer.Helper;
using GlobalObjects;
using ICSharpCode.AvalonEdit;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.VisualBasic.Logging;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DataTransformer.ViewModel
{
    [SupportedOSPlatform("windows7.0")]
    internal class CsvOptionViewModel : ObservableObject
    {
        private Window window;
        public CsvOption inputOption { get; set; }
        public CsvOption outputOption { get; set; }

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

        private bool isInputOption;

        public bool IsInputOption
        {
            get
            {
                return isInputOption;
            }
            set
            {
                SetProperty<bool>(ref isInputOption, value);
            }
        }

        private bool isOutputOption;

        public bool IsOutputOption
        {
            get
            {
                return isOutputOption;
            }
            set
            {
                SetProperty<bool>(ref isOutputOption, value);
            }
        }

        private ICSharpCode.AvalonEdit.Document.TextDocument inputOptionHeaderListDocument;

        public ICSharpCode.AvalonEdit.Document.TextDocument InputOptionHeaderListDocument
        {
            get 
            {
                return inputOptionHeaderListDocument; 
            }
            set
            {
                SetProperty<ICSharpCode.AvalonEdit.Document.TextDocument>(ref inputOptionHeaderListDocument, value);
            }
        }

        private string inputOptionSpliterValue;

        public string InputOptionSpliterValue
        {
            get
            {
                return inputOptionSpliterValue;
            }
            set
            {
                SetProperty<string>(ref inputOptionSpliterValue, value);
            }
        }

        private bool inputOptionHasQuotes;

        public bool InputOptionHasQuotes
        {
            get
            {
                return inputOptionHasQuotes;
            }
            set
            {
                SetProperty<bool>(ref inputOptionHasQuotes, value);
            }
        }

        private bool inputOptionShowHeader;

        public bool InputOptionShowHeader
        {
            get
            {
                return inputOptionShowHeader;
            }
            set
            {
                SetProperty<bool>(ref inputOptionShowHeader, value);
            }
        }

        private string inputOptionEncodingValue;

        public string InputOptionEncodingValue
        {
            get
            {
                return inputOptionEncodingValue;
            }
            set
            {
                SetProperty<string>(ref inputOptionEncodingValue, value);
            }
        }

        private ICSharpCode.AvalonEdit.Document.TextDocument outputOptionHeaderListDocument;

        public ICSharpCode.AvalonEdit.Document.TextDocument OutputOptionHeaderListDocument
        {
            get
            {
                return outputOptionHeaderListDocument;
            }
            set
            {
                SetProperty<ICSharpCode.AvalonEdit.Document.TextDocument>(ref outputOptionHeaderListDocument, value);
            }
        }

        private string outputOptionSpliterValue;

        public string OutputOptionSpliterValue
        {
            get
            {
                return outputOptionSpliterValue;
            }
            set
            {
                SetProperty<string>(ref outputOptionSpliterValue, value);
            }
        }

        private bool outputOptionHasQuotes;

        public bool OutputOptionHasQuotes
        {
            get
            {
                return outputOptionHasQuotes;
            }
            set
            {
                SetProperty<bool>(ref outputOptionHasQuotes, value);
            }
        }

        private bool outputOptionShowHeader;

        public bool OutputOptionShowHeader
        {
            get
            {
                return outputOptionShowHeader;
            }
            set
            {
                SetProperty<bool>(ref outputOptionShowHeader, value);
            }
        }

        private string outputOptionEncodingValue;

        public string OutputOptionEncodingValue
        {
            get
            {
                return outputOptionEncodingValue;
            }
            set
            {
                SetProperty<string>(ref outputOptionEncodingValue, value);
            }
        }

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand OkCommand { get; set; }

        public CsvOptionViewModel(CsvOption csvOption, bool isInputOption)
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);

            OkCommand = new RelayCommand(OkBtnClicked);

            if (csvOption == null)
            {
                csvOption = new CsvOption();
            }

            this.IsInputOption = isInputOption;
            this.IsOutputOption = !isInputOption;

            if (isInputOption)
            {
                this.inputOption = csvOption;

                if (inputOption.headerList == null)
                {
                    inputOptionHeaderListDocument = new ICSharpCode.AvalonEdit.Document.TextDocument("");
                }
                else
                {
                    inputOptionHeaderListDocument = new ICSharpCode.AvalonEdit.Document.TextDocument(string.Join('\n', inputOption.headerList));
                }

                inputOptionSpliterValue = inputOption.spliter;

                inputOptionHasQuotes = inputOption.hasQuotes;

                inputOptionShowHeader = inputOption.showHeader;

                inputOptionEncodingValue = inputOption.encoding;
            }
            else
            {
                this.outputOption = csvOption;

                if (outputOption.headerList == null)
                {
                    outputOptionHeaderListDocument = new ICSharpCode.AvalonEdit.Document.TextDocument("");
                }
                else
                {
                    outputOptionHeaderListDocument = new ICSharpCode.AvalonEdit.Document.TextDocument(string.Join('\n', outputOption.headerList));
                }

                outputOptionSpliterValue = outputOption.spliter;

                outputOptionHasQuotes = outputOption.hasQuotes;

                outputOptionShowHeader = outputOption.showHeader;

                outputOptionEncodingValue = outputOption.encoding;
            }

            ModernWpf.ThemeManager.Current.ActualApplicationThemeChanged += ActualApplicationThemeChanged;
        }

        private void WindowLoaded(RoutedEventArgs e)
        {
            this.window = (Window)e.Source;

            ActualApplicationThemeChanged(null, null);
        }

        private void ActualApplicationThemeChanged(ThemeManager themeManager, object obj)
        {
            GlobalObjects.GlobalObjects.ClearPropertiesSetter();

            GlobalObjects.Theme.SetTheme();
            ThemeControlFocusBackground = Theme.ThemeControlFocusBackground;
            ThemeControlForeground = Theme.ThemeControlForeground;
        }


        private void OkBtnClicked()
        {
            if (inputOption != null)
            {
                inputOption.headerList = inputOptionHeaderListDocument.Text.Replace("\r", "").Split('\n').ToList();
                inputOption.spliter = inputOptionSpliterValue;
                inputOption.hasQuotes = inputOptionHasQuotes;
                inputOption.showHeader = inputOptionShowHeader;
                inputOption.encoding = inputOptionEncodingValue;
            }
            else
            {
                outputOption.headerList = outputOptionHeaderListDocument.Text.Replace("\r", "").Split('\n').ToList();
                outputOption.spliter = outputOptionSpliterValue;
                outputOption.hasQuotes = outputOptionHasQuotes;
                outputOption.showHeader = outputOptionShowHeader;
                outputOption.encoding = outputOptionEncodingValue;
            }

            this.window.DialogResult = true;
        }
    }
}
