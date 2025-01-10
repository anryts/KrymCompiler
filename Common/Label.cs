using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

public struct Label(string name, int index)
{
    public string Name { get; set; } = name;
    public int Index { get; set; } = index;
    public string Value { get; set; }

    public List<string> Operations { get; set; } = new List<string>();

    public override string ToString()
    {
        return $"{Name,-10} {Index,-10} {Value,-10}";
    }
}
