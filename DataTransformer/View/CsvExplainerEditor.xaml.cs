using CustomizableMessageBox;
using DataTransformer.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace DataTransformer
{
    /// <summary>
    /// CsvExplainerEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class CsvExplainerEditor : Window
    {
        public CsvExplainerEditor()
        {
            InitializeComponent();
            this.DataContext = new CsvExplainerViewModel();

            tb_paths.TextChanged += new TextChangedEventHandler((s, e) =>
            {
                BindingExpression be = tb_paths.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            });
            tb_filenames.TextChanged += new TextChangedEventHandler((s, e) =>
            {
                BindingExpression be = tb_filenames.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            });
        }
    }
}
