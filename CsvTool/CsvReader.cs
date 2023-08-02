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

        public CsvReader(StreamReader reader, CsvOption csvOption)
        {
            _reader = reader;
            _csvOption = csvOption;
        }

        public IEnumerable<IEnumerable<string>> GetLists()
        {
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                var data = ParseLine(line, _csvOption.spliter, _csvOption.hasQuotes);
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
