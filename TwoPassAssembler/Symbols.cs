using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoPassAssembler
{
    public class Symbols
    {
        public string symName { set; get; }
        public int symAddress { set; get; }
        public bool isRelative { set; get; }
    }
}
