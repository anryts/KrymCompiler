using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaltaLangParser
{
    static class GlobalVars
    {
        public static int CurrentTokenIndex { get; set; }

        public static List<Variable> VariableTable { get; set; } = new List<Variable>();
    }
}
