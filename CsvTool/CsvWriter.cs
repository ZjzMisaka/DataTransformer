using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTool
{
    public class CsvWriter
    {
        private CsvOption _csvOption;
        private string _separator;

        public CsvWriter(CsvOption csvOption)
        {
            _csvOption = csvOption;
            _separator = Constant.splitorDic[csvOption.spliter];
        }

        public void Write(IEnumerable<IEnumerable<string>> data, string filePath)
        {
            StreamWriter writer = new StreamWriter(filePath);

            if (_csvOption.showHeader && _csvOption.headerList != null)
            {
                WriteRow(_csvOption.headerList, filePath, writer);
            }

            foreach (var row in data)
            {
                WriteRow(row, filePath, writer);
            }

            writer.Dispose();
        }

        private void WriteRow(IEnumerable<string> row, string filePath, StreamWriter writer)
        {
            var finalRow = new StringBuilder();
            foreach (var field in row)
            {
                if (_csvOption.hasQuotes || needQuotes(field))
                {
                    finalRow.Append("\"" + field.Replace("\"", "\"\"") + "\"" + _separator);
                }
                else
                {
                    finalRow.Append(field + _separator);
                }
            }

            writer.WriteLine(finalRow.ToString().TrimEnd(_separator.ToCharArray()));
        }

        private bool needQuotes(string field)
        {
            if (_separator == "," && field.Contains(","))
            {
                return true;
            }

            return false;
        }
    }
}
