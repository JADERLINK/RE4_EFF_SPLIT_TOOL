using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_PS3X360_EFF_SPLIT
{
    class Program
    {
        static void Main(string[] args)
        {
            EFF_SPLIT.MainProgram.Continue(args, EFF_SPLIT.IsVersion.IsX360, "IDX_X360_EFF_SPLIT", "RE4 PS3X360 EFF SPLIT");
        }
    }
}
