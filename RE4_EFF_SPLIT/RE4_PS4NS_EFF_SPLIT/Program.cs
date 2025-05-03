using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_PS4NS_EFF_SPLIT
{
    class Program
    {
        static void Main(string[] args)
        {
            EFF_SPLIT.MainProgram.Continue(args, EFF_SPLIT.IsVersion.IsPS4NS, "IDX_PS4NS_EFF_SPLIT", "RE4 PS4NS EFF SPLIT");
        }
    }
}
