//============================================================================
// Program.cs
//    The entry point of "test" project.
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

//================================================================================
// DEBUG
//================================================================================
#if DEBUG
namespace Test
{
    //======================================================================
    // clas Program
    //======================================================================
    internal static partial class Program
    {
        internal static void Temp01()
        {
            //Temp01a();
            Temp01b();
        }

        //------------------------------------------------------------
        // Temp01a
        //------------------------------------------------------------
        internal static void Temp01a()
        {
            Uncs.CNameManager nameManager = new Uncs.CNameManager();
            Uncs.ConsoleOutput cout = new Uncs.ConsoleOutput(new Uncs.SystemConsole());
            Uncs.CController controller = new Uncs.CController();
            controller.Initialize(0, cout, nameManager);
            Uncs.COptionManager optManger = controller.OptionManager;

            string[] args =
            {
                @"/warnaserror",
                @"/warnaserror-",
                @"/warnaserror:28,67,5000",
                @"D:\Develop\Sample\program1.cs",
            };

            optManger.SetCommandArguments(args);
            optManger.ProcessResponseFiles();
            optManger.ProcessPreSwitches();
            optManger.ProcessOptions(Uncs.CommandID.CSC);

            StringBuilder sb = new StringBuilder();
            List<Uncs.CInputSet> inpList = controller.InputSetList;
            foreach (Uncs.CInputSet inp in inpList)
            {
                sb.Append("--------------------\n");
                inp.Debug(sb);
            }
            sb.Append("--------------------\n");
            string dbstr = sb.ToString();
        }

        //------------------------------------------------------------
        // Temp01b
        //------------------------------------------------------------
        internal static void Temp01b()
        {
            Assembly syscore = Assembly.GetAssembly(typeof(System.Linq.Enumerable));
        }
    }
}
#endif
