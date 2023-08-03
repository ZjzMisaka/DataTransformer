using CsvTool;
using DataTransformer;
using DataTransformer.Helper;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DataTransformer.ViewModel
{
    [SupportedOSPlatform("windows7.0")]
    internal class CsvOptionViewModel : ObservableObject
    {
        private Window window;
        public CsvOption inputOption { get; set; }
        public CsvOption outputOption { get; set; }

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

        private string inputOptionHeaderList;

        public string InputOptionHeaderList 
        {
            get 
            {
                return inputOptionHeaderList; 
            }
            set
            {
                SetProperty<string>(ref inputOptionHeaderList, value);
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

        private string outputOptionHeaderList;

        public string OutputOptionHeaderList
        {
            get
            {
                return outputOptionHeaderList;
            }
            set
            {
                SetProperty<string>(ref outputOptionHeaderList, value);
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

        private string outputOptionOutputPath;

        public string OutputOptionOutputPath
        {
            get
            {
                return outputOptionOutputPath;
            }
            set
            {
                SetProperty<string>(ref outputOptionOutputPath, value);
            }
        }

        private string outputOptionOutputFileName;

        public string OutputOptionOutputFileName
        {
            get
            {
                return outputOptionOutputFileName;
            }
            set
            {
                SetProperty<string>(ref outputOptionOutputFileName, value);
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
                    inputOptionHeaderList = "";
                }
                else
                {
                    inputOptionHeaderList = string.Join('\n', inputOption.headerList);
                }

                InputOptionSpliterValue = inputOption.spliter;

                inputOptionHasQuotes = inputOption.hasQuotes;

                inputOptionShowHeader = inputOption.showHeader;
            }
            else
            {
                this.outputOption = csvOption;

                if (outputOption.headerList == null)
                {
                    outputOptionHeaderList = "";
                }
                else
                {
                    outputOptionHeaderList = string.Join('\n', outputOption.headerList);
                }

                outputOptionSpliterValue = outputOption.spliter;

                outputOptionHasQuotes = outputOption.hasQuotes;

                outputOptionShowHeader = outputOption.showHeader;

                outputOptionOutputPath = outputOption.outputPath;

                outputOptionOutputFileName = outputOption.outputFileName;
            }
            
        }

        private void WindowLoaded(RoutedEventArgs e)
        {
            this.window = (Window)e.Source;
        }


        private void OkBtnClicked()
        {
            if (inputOption != null)
            {
                inputOption.headerList = inputOptionHeaderList.Replace("\r", "").Split('\n').Where(str => str.Trim() != "").ToList();
                inputOption.spliter = inputOptionSpliterValue;
                inputOption.hasQuotes = inputOptionHasQuotes;
                inputOption.showHeader = inputOptionShowHeader;
            }
            else
            {
                outputOption.headerList = outputOptionHeaderList.Replace("\r", "").Split('\n').Where(str => str.Trim() != "").ToList();
                outputOption.spliter = outputOptionSpliterValue;
                outputOption.hasQuotes = outputOptionHasQuotes;
                outputOption.showHeader = outputOptionShowHeader;
                outputOption.outputPath = outputOptionOutputPath; outputOption.outputFileName = outputOptionOutputFileName;
            }

            this.window.DialogResult = true;
        }
    }
}
