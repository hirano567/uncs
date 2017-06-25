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
        //------------------------------------------------------------
        // Main
        //------------------------------------------------------------
        static void Main(string[] args)
        {
            //TestCsc.Do();
            Test01();
            //Temp01();

            Console.Write("\nPress Enter key.");
            Console.Read();
        }

        //------------------------------------------------------------
        // Test01
        //------------------------------------------------------------
        static void Test01()
        {
            string[] args0 =
            {
                @"/warn:0",
                @"/target:library",
                @"/out:uncs.dll",
                @"/recurse:D:\Develop\TestCsc\elcs_src\elcs3\*.cs",
            };

            string srcDir1 = @"D:\Develop\TestCsc\Temp";
            string[] args1 =
            {
                Path.Combine(srcDir1, @"Temp01.cs"),
            };

            string srcDir2 = @"D:\Develop\TestCsc\CS4\Dynamic";
            string[] args2 =
            {
                //Path.Combine(srcDir2, @"Dynamic01.cs"),
                Path.Combine(srcDir2, @"Dynamic07.cs"),
            };

            string srcDir3 = @"D:\Develop\TestCsc\CS3\Partial";
            string[] args3 =
            {
                Path.Combine(srcDir3, @"Partial01.cs"),
            };

            string srcDir4 = @"D:\Develop\TestCsc\Unofficial\ExpressionTrees";
            string[] args4 =
            {
                Path.Combine(srcDir4, @"unof_ExTrees20.cs"),
            };

            string srcDir5 = @"D:\Develop\Temp";
            string[] args5 =
            {
                Path.Combine(srcDir5, @"Temp01.cs"),
            };

            Uncs.Compiler.Compile(args2);
        }
    }

    //======================================================================
    // class CompileSet
    //======================================================================
    internal class CompileSet
    {
        public Uncs.CNameManager NameManager = null;
        public Uncs.CController Controller = null;
        public Uncs.COptionManager OptionManager = null;
        public Uncs.COMPILER Compiler = null;

        public Uncs.CompilerCreationFlags CreationFlags = Uncs.CompilerCreationFlags.TRACKCOMMENTS;
        public Uncs.CscConsoleOutput ConsoleOutput = null;

        //------------------------------------------------------------
        // CompileSet.Create
        //------------------------------------------------------------
        static internal CompileSet Create()
        {
            CompileSet set = new CompileSet();
            set.ConsoleOutput = new Uncs.CscConsoleOutput(new Uncs.SystemConsole());
            set.NameManager = Uncs.CNameManager.CreateInstance();
            set.Controller = Uncs.CController.CreateInstance(
                set.CreationFlags,
                set.ConsoleOutput,
                set.NameManager);
            set.OptionManager = set.Controller.OptionManager;

            set.Compiler = new Uncs.COMPILER(set.Controller, set.NameManager);
            set.Compiler.Init();

            return set;
        }
    }

    //======================================================================
    // class Util
    //======================================================================
    internal static class Util
    {
    }
}

#else
//================================================================================
// Not DEBUG
//================================================================================
public static class Program
{
    public static void Main()
    {
        Console.Write("Run in DEBUG mode.");
        Console.Read();
    }
}
#endif