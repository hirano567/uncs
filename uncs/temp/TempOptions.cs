#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace temp
{
    public static class CTempOptions
    {
        //------------------------------------------------------------
        // CTempOptions.Run
        //------------------------------------------------------------
        internal static void Run()
        {
            Uncs.SystemConsole sysConsole = new Uncs.SystemConsole();
            Uncs.ConsoleOutput cout = new Uncs.ConsoleOutput(sysConsole);

            Uncs.OptionInfoManager.PrintCSCHelp(cout);
            Console.WriteLine();
            Console.WriteLine("//////////////////////////////////////////////////");
            Console.WriteLine();
            Uncs.OptionInfoManager.PrintALHelp(cout);
        }
    }
}
#endif
