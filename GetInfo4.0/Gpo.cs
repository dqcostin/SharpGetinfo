using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using 

namespace GetInfo4._0
{
    class Gpo
    {
        public static void Gpo_pass()
        {
            GetInfo.Program.RunCMDCommand("cmd", "whoami");
        }
    }
}
