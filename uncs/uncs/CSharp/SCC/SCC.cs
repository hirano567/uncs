// sscli20_20060311

// ==++==
//
//   
//    Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//   
//    The use and distribution terms for this software are contained in the file
//    named license.txt, which can be found in the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by the
//    terms of this license.
//   
//    You must not remove this notice, or any other, from this software.
//   
//
// ==--==

// ==++==
//
//   
//    Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//   
//    The use and distribution terms for this software are contained in the file
//    named license.txt, which can be found in the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by the
//    terms of this license.
//   
//    You must not remove this notice, or any other, from this software.
//   
//
// ==--==
// ===========================================================================
// File: scc.cpp
//
// The command line driver for the C# compiler.
// ===========================================================================

//============================================================================
// SCC.cs
//
// 2015/11/12 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Uncs
{
    // The definition of ERROR_INFO[] errorInfo is moved to ErrorProcessing\CSCErrorInfo.cs

    //======================================================================
    // class CommandLineCompiler
    //
    /// <summary>
    /// Command line compiler.
    /// </summary>
    //======================================================================
    internal class CommandLineCompiler
    {
        //------------------------------------------------------------
        // CommandLineCompiler Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CommandLineCompiler()
        {
        }

        //------------------------------------------------------------
        // CommandLineCompiler.Compile
        //
        /// <summary>
        /// <para>Compile by arguments.</para>
        /// <para>In sscli, this method is the main entry point.</para>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int Compile(string[] args)
        {
            CNameManager nameManager = CNameManager.CreateInstance();
            CscConsoleOutput cout = new CscConsoleOutput(new Uncs.SystemConsole());

            CompilerCreationFlags flags = CompilerCreationFlags.TRACKCOMMENTS;
            CController controller = CController.CreateInstance(flags, cout, nameManager);
            cout.SetController(controller);

            COptionManager optManger = controller.OptionManager;
            optManger.DefaultTarget = TargetType.Exe;

            optManger.SetCommandArguments(args);
            optManger.ProcessResponseFiles();
            optManger.ProcessPreSwitches();
            if (controller.HadError)
            {
                return 1;
            }

            if (!optManger.NoLogo)
            {
                cout.PrintBanner();
            }

            if (optManger.ShowHelp)
            {
                OptionInfoManager.PrintCSCHelp(cout);
            }
            else
            {
                optManger.ProcessOptions(Uncs.CommandID.CSC);
                if (!controller.HadError)
                {
                    controller.Compile(null);
                }
            }

            return (controller.HadError?1:0);
        }
    }

    //======================================================================
    // class Compiler
    //
    /// <summary>
    /// Public
    /// </summary>
    //======================================================================
    static public partial class Compiler
    {
        //------------------------------------------------------------
        // Compiler.Compile
        //------------------------------------------------------------
        static public void Compile(params string[] args)
        {
            DebugUtil.CreateStopwatchs(2);
            DebugUtil.StartStopwatch(true, "Parse args");
            DebugUtil.StartStopwatch(1, true, "total");

            CommandLineCompiler compiler = new CommandLineCompiler();
            compiler.Compile(args);

            DebugUtil.DisplayAllStopwatchTimes();
        }
    }
}
