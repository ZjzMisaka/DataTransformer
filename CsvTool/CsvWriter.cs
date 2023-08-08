using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CsvTool
{
    public class CsvWriter
    {
        private CsvOption csvOption;
        private string separator;

        public CsvWriter(CsvOption csvOption)
        {
            this.csvOption = csvOption;
            this.separator = Constant.splitorDic[csvOption.spliter];
        }

        public void Write(IEnumerable<IEnumerable<string>> data, string filePath)
        {
            StreamWriter writer = new StreamWriter(filePath);

            if (csvOption.showHeader && csvOption.headerList != null)
            {
                WriteRow(csvOption.headerList, writer);
            }

            foreach (var row in data)
            {
                WriteRow(row, writer);
            }

            writer.Dispose();
        }

        private void WriteRow(IEnumerable<string> row, StreamWriter writer)
        {
            StringBuilder finalRow = new StringBuilder();
            foreach (string field in row)
            {
                if (csvOption.hasQuotes || needQuotes(field))
                {
                    finalRow.Append("\"" + field.Replace("\"", "\"\"") + "\"" + separator);
                }
                else
                {
                    finalRow.Append(field + separator);
                }
            }

            writer.WriteLine(finalRow.ToString().TrimEnd(separator.ToCharArray()));
        }

        private bool needQuotes(string field)
        {
            if (separator == "," && field.Contains(","))
            {
                return true;
            }

            return false;
        }
    }
}
