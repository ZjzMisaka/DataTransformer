﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransformer
{
    public class RunningRule
    {
        public string analyzers;
        public string csvExplainers;
        public bool executeInSequence;
        public string param;
        public string basePath;
        public string outputPath;
        public string outputName;

        public string watchPath;
        public string filter;
    }
}
