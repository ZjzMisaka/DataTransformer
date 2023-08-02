using CsvTool;
using DataTransformer.ViewModel;
using DataTransformer;
using DataTransformer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DataTransformer.View
{
    [SupportedOSPlatform("windows7.0")]
    /// <summary>
    /// CsvOptionEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class CsvOptionEditor : Window
    {
        public CsvOptionEditor(CsvOption csvOption, bool isInputOption)
        {
            InitializeComponent();
            this.DataContext = new CsvOptionViewModel(csvOption, isInputOption);
        }
    }
}
