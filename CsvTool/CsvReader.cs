using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvTool
{
    public class CsvReader
    {
        private StreamReader reader;
        private CsvOption csvOption;

        public CsvReader(CsvOption csvOption, string filePath)
        {
            this.reader = new StreamReader(filePath);
            this.csvOption = csvOption;
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public Dictionary<string, string> ReadNext()
        {
            string line = reader.ReadLine();
            if (line == null)
            {
                return null;
            }

            IEnumerable<string> data = ParseLine(line, Constant.splitorDic[csvOption.spliter], csvOption.hasQuotes);
            return GetDataHeaderDictionary(data);
        }

        public IEnumerable<IEnumerable<string>> GetLists()
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                IEnumerable<string> data = ParseLine(line, Constant.splitorDic[csvOption.spliter], csvOption.hasQuotes);
                yield return data;
            }
        }

        public Dictionary<string, string> GetDataHeaderDictionary(IEnumerable<string> data)
        {
            var dict = new Dictionary<string, string>();
            var headers = csvOption.headerList;
            var dataList = data.ToList();

            if (headers.Count != dataList.Count)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    dict[i.ToString()] = dataList[i];
                }
            }
            else
            {
                
                for (int i = 0; i < headers.Count; i++)
                {
                    if (headers[i].Trim() == "")
                    {
                        dict[i.ToString()] = dataList[i];
                    }
                    else
                    {
                        dict[headers[i]] = dataList[i];
                    }
                }
                
            }

            return dict;
        }

        private IEnumerable<string> ParseLine(string line, string separator, bool hasQuotes)
        {
            string[] fields = line.Split(new string[] { separator }, StringSplitOptions.None);

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
