using CsvTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransformer
{
    public enum FindingMethod { SAME, CONTAIN, REGEX, ALL }
    public class CsvExplainer
    {
        public List<String> pathes = new List<String>();
        public KeyValuePair<FindingMethod, List<String>> fileNames = new KeyValuePair<FindingMethod, List<String>>();
        public CsvOption inputOption = new CsvOption();
    }
}
