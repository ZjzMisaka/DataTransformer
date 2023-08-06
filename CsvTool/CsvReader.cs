using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvTool
{
    public class CsvReader
    {
        private StreamReader _reader;
        private CsvOption _csvOption;

        public CsvReader(CsvOption csvOption, string filePath)
        {
            _reader = new StreamReader(filePath);
            _csvOption = csvOption;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public Dictionary<string, string> ReadNext()
        {
            string line = _reader.ReadLine();
            if (line == null)
            {
                return null;
            }

            var data = ParseLine(line, Constant.splitorDic[_csvOption.spliter], _csvOption.hasQuotes);
            return GetDataHeaderDictionary(data);
        }

        public IEnumerable<IEnumerable<string>> GetLists()
        {
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                var data = ParseLine(line, Constant.splitorDic[_csvOption.spliter], _csvOption.hasQuotes);
                yield return data;
            }
        }

        public Dictionary<string, string> GetDataHeaderDictionary(IEnumerable<string> data)
        {
            var dict = new Dictionary<string, string>();
            var headers = _csvOption.headerList;
            var dataList = data.ToList();

            for (int i = 0; i < headers.Count; i++)
            {
                dict[headers[i]] = dataList[i];
            }

            return dict;
        }

        private IEnumerable<string> ParseLine(string line, string separator, bool hasQuotes)
        {
            var fields = line.Split(new string[] { separator }, StringSplitOptions.None);

            if (hasQuotes)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = fields[i].Trim('\"');
                }
            }

            return fields;
        }
    }
}
