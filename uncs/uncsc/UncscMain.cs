//============================================================================
// UncscMain.cs
//    Command Line Compiler
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;

namespace Uncs
{
    //======================================================================
    // class CSC
    //
    /// <summary>
    /// Command Line Compiler
    /// </summary>
    //======================================================================
    static public class CSC
    {
        //============================================================
        // CSC.Main
        //
        /// <summary>
        /// <para>Call CommandLineCompiler.Compile method of uncs.dll.</para>
        /// </summary>
        /// <param name="args"></param>
        //============================================================
        static void Main(string[] args)
        {
            try
            {
                Compiler.Compile(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return;
        }
    }
}
