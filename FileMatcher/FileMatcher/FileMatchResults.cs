using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileMatcher
{
    class FileMatchResults
    {
        public Dictionary<string, string> hashed { get; set; }
        public Dictionary<string, List<string>> grouped { get; set; }
        public List<string> failed { get; set; }
    }
}
