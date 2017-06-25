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

//============================================================================
// ConsoleOutput.cs (uncs\CSharp\Inc\)
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Uncs
{
    //======================================================================
    // interface IConsole
    //
    /// <summary>
    /// 
    /// </summary>
    //======================================================================
    public interface IConsole
    {
        int Width { get; }

        void Write(string str);
        void WriteLine();
        void WriteLine(string str);
    }

    //======================================================================
    // class SystemConsole
    //
    /// <summary>
    /// Use System.Console.
    /// </summary>
    //======================================================================
    public class SystemConsole : IConsole
    {
        //------------------------------------------------------------
        // SystemConsole Fields and Properties
        //------------------------------------------------------------
        private static Object lockObject = new Object();

        public int Width // (IConsole)
        {
            get { return System.Console.WindowWidth; }
        }

        //------------------------------------------------------------
        // SystemConsole.Write (IConsole)
        //
        /// <summary></summary>
        /// <param name="str"></param>
        //------------------------------------------------------------
        public void Write(string str)
        {
            lock (SystemConsole.lockObject)
            {
                System.Console.Write(str);
            }
        }

        //------------------------------------------------------------
        // SystemConsole.WriteLine (1) (IConsole)
        //
        /// <summary>
        /// <para>This method catch no exception.</para>
        /// </summary>
        //------------------------------------------------------------
        public void WriteLine()
        {
            lock (SystemConsole.lockObject)
            {
                System.Console.WriteLine();
            }
        }

        //------------------------------------------------------------
        // SystemConsole.WriteLine (2) (IConsole)
        //
        /// <summary>
        /// <para>This method catch no exception.</para>
        /// </summary>
        //------------------------------------------------------------
        public void WriteLine(string str)
        {
            lock (SystemConsole.lockObject)
            {
                System.Console.WriteLine(str);
            }
        }
    }

    // class ERROR_INFO (Moved to ErrorProcessing\ErrorInfo.cs)

    //======================================================================
    // class ConsoleOutput
    //
    /// <summary>
    /// <para>ConsoleOutput contains all shared code relative to console output for both CSC and Alink.
    /// Be sure to integrate any changes into both both.</para>
    /// <para>An abstract class to show messages.</para>
    /// </summary>
    //======================================================================
    internal class ConsoleOutput
    {
        //------------------------------------------------------------
        // ConsoleOutput    Fields and Properties
        //------------------------------------------------------------
        protected CController controller = null;
        protected IConsole consoleObject = null;

        protected Encoding defaultEncoding = Encoding.Default;

        internal Encoding DefaultEncoding
        {
            get { return this.defaultEncoding; }
        }

        /// <summary>
        /// <para>If null, no conversion.</para>
        /// </summary>
        protected Encoding outputEncoding = null;     // UINT m_ConsoleCodepage;

        internal Encoding OutputEncoding            // UINT GetConsoleOutputCP();
        {
            get { return this.outputEncoding; }
        }

        protected bool prettyPrint = false;         // m_fPrettyPrint

        //protected bool redirectingToFile = true;  // m_fRedirectingToFile

        internal virtual int ConsoleWidth           // size_t m_width;
        {
            get
            {
                DebugUtil.Assert(this.consoleObject != null);
                return this.consoleObject.Width;
            }
        }

        protected int currentColumn = 0;            // size_t m_currentColumn;

        static protected string errorPrefix = null; // static const LPWSTR m_errorPrefix;

        //const ERROR_INFO* m_pErrorInfo;
        // Use static class CSCErrorInfo (defined in CSharp\SCC\CSCErrorInfo.cs).

        //------------------------------------------------------------
        // ConsoleOutput Constructor
        //
        /// <summary></summary>
        /// <param name="con"></param>
        //------------------------------------------------------------
        internal ConsoleOutput(IConsole con)
        {
            this.consoleObject = con;
        }

        //------------------------------------------------------------
        // ConsoleOutput.SetController
        //
        /// <summary></summary>
        /// <param name="contr"></param>
        //------------------------------------------------------------
        virtual internal void SetController(CController contr)
        {
            this.controller = contr;
        }

        //------------------------------------------------------------
        // ConsoleOutput.FormatString
        //
        /// <summary></summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected string FormatString(string fmt, params Object[] args)
        {
            string buffer;
            Exception excp = null;

            if (FormatStringUtil.FormatString(out buffer, out excp, fmt, args))
            {
                return buffer;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}: ",
                (excp != null ? excp.Message : "Failed to Format"));
            foreach (Object obj in args)
            {
                sb.AppendFormat(", {0}", obj);
            }
            return sb.ToString();
        }

        //------------------------------------------------------------
        // ConsoleOutput.Write (1)
        //
        /// <summary></summary>
        /// <param name="str1"></param>
        //------------------------------------------------------------
        internal void Write(string str1)
        {
            DebugUtil.Assert(this.consoleObject != null);

            if (this.outputEncoding == null)
            {
                this.consoleObject.Write(str1);
            }
            else
            {
                this.consoleObject.Write(this.ConvertString(str1));
            }
        }

        //------------------------------------------------------------
        // ConsoleOutput.Write (2)
        //
        /// <summary></summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Write(string fmt, params Object[] args)
        {
            DebugUtil.Assert(this.consoleObject != null);
            this.Write(this.FormatString(fmt, args));
        }

        //------------------------------------------------------------
        // ConsoleOutput.WriteLine (1)
        //
        /// <summary>
        /// <para>Output an empty line.</para>
        /// <para>(PrintBlankLine in sscli)</para>
        /// </summary>
        //------------------------------------------------------------
        internal void WriteLine()
        {
            DebugUtil.Assert(this.consoleObject != null);
            this.consoleObject.WriteLine();
        }

        //------------------------------------------------------------
        // ConsoleOutput.WriteLine (2)
        //
        /// <summary></summary>
        /// <param name="str1"></param>
        //------------------------------------------------------------
        internal void WriteLine(string str1)
        {
            DebugUtil.Assert(this.consoleObject != null);

            if (this.outputEncoding == null)
            {
                this.consoleObject.WriteLine(str1);
            }
            else
            {
                this.consoleObject.WriteLine(this.ConvertString(str1));
            }
        }

        //------------------------------------------------------------
        // ConsoleOutput.WriteLine (3)
        //
        /// <summary></summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void WriteLine(string fmt, params Object[] args)
        {
            DebugUtil.Assert(this.consoleObject != null);
            this.consoleObject.WriteLine(this.FormatString(fmt, args));
        }

        //------------------------------------------------------------
        // ConsoleOutput.Initialize
        //
        /// <summary>
        /// Do nothing for now.
        /// </summary>
        /// <remarks>Return true.</remarks>
        //------------------------------------------------------------
        internal bool Initialize()
        {
            return true;
        }

        //------------------------------------------------------------
        // ConsoleOutput.PrintString (1)
        //
        /// <summary>
        /// Load a string from a resource and print it, followed by an optional newline.
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="newline"></param>
        /// <param name="prettyprint"></param>
        /// <remarks>
        /// ALink's PrintHelp() function will just iterate through a range of ids,
        /// and only print the ones which exist.
        /// Thus, the string may not exist for ALink, but it should for CSC.
        /// </remarks>
        //------------------------------------------------------------
        internal void PrintString(
            ResNo resNo,
            bool newLine,       // = true,
            bool prettyPrint)   // = false
        {
            string buffer;
            Exception excp = null;

            if (!CResources.GetString(resNo, out buffer, out excp))
            {
                return;
            }

            if (prettyPrint)
            {
                //DebugUtil.Assert(newline);
                PrettyPrint(buffer, 0);
            }
            else
            {
                PrintInternal(buffer, newLine);
            }
        }

        //------------------------------------------------------------
        // ConsoleOutput.PrintString (2)
        //
        /// <summary>
        /// Load a string from a resource and print it, followed by an optional newline.
        /// </summary>
        /// <param name="eid"></param>
        /// <param name="newline"></param>
        /// <param name="prettyprint"></param>
        /// <remarks>
        /// ALink's PrintHelp() function will just iterate through a range of ids,
        /// and only print the ones which exist.
        /// Thus, the string may not exist for ALink, but it should for CSC.
        /// </remarks>
        //------------------------------------------------------------
        internal void PrintString(
            CSCERRID errorId,
            bool newLine,       // = true,
            bool prettyPrint    // = false
            )
        {
            ResNo resNo;
            if (!CSCErrorInfo.Manager.GetResourceNumber(errorId, out resNo))
            {
                return;
            }
            PrintString(resNo, newLine, prettyPrint);
        }

        //------------------------------------------------------------
        // ConsoleOutput.PrintBanner
        //
        /// <summary></summary>
        //------------------------------------------------------------
        virtual internal void PrintBanner()
        {
            //string banner1a, banner1b, banner2;
            //Util.CSResources.GetString(CSCSTRID.BANNER1, out banner1a);
            //Util.CSResources.GetString(CSCSTRID.BANNER1PART2, out banner1b);
            //Util.CSResources.GetString(CSCSTRID.BANNER2, out banner2);
            WriteLine("sscli20_20060311 C# Compiler");
        }

        //------------------------------------------------------------
        // ConsoleOutput.EnableUTF8Output
        //
        /// <summary></summary>
        /// <param name="enable"></param>
        //------------------------------------------------------------
        internal void EnableUTF8Output(bool enable)
        {
            // can't output utf 8 to console, only to file
            if (enable)
            {
                this.outputEncoding = Encoding.UTF8;
            }
            else
            {
                this.outputEncoding = Encoding.Default;
            }
        }

        //protected:

        //------------------------------------------------------------
        // ConsoleOutput.PrintInternal
        //
        /// <summary>
        /// Output a text.
        /// Then, if argument newLine is true, output '\n'.
        /// </summary>
        //------------------------------------------------------------
        virtual protected void PrintInternal(string text, bool newLine)
        {
            if (this.consoleObject == null)
            {
                return;
            }

            if (newLine)
            {
                consoleObject.WriteLine(text);
                this.currentColumn = 0;
            }
            else
            {
                consoleObject.Write(text);

                int idx = text.LastIndexOf('\n');
                if (idx < 0)
                {
                    this.currentColumn += text.Length;
                }
                else
                {
                    this.currentColumn += (text.Length - idx - 1);
                }
            }
        }

        //------------------------------------------------------------
        // ConsoleOutput.PrettyPrint (1)
        //
        /// <summary>
        /// Show a text with indent.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="indent"></param>
        /// <remarks>
        /// Print out the given text, with word-wraping based on screen size
        /// and indenting the wrapped lines.
        /// Word wraps on whitespace chars if possible.
        /// Indents all lines after the first.
        /// There may be newlines in pszText.
        /// Always finishes with the console at the start of a new line.
        /// </remarks>
        //------------------------------------------------------------
        protected void PrettyPrint(string text, int indent)
        {
            // 100 characters should be more than large enough for any indent
            DebugUtil.Assert(indent >= 0 && indent <= 100);

            StringBuilder sb = new StringBuilder();
            PrettyPrint(sb, text, indent);
            PrintInternal(sb.ToString(), true);
        }

        //------------------------------------------------------------
        // ConsoleOutput.PrettyPrint (2)
        //
        /// <summary></summary>
        /// <param name="builder"></param>
        /// <param name="text"></param>
        /// <param name="indent"></param>
        //------------------------------------------------------------
        protected void PrettyPrint(
            StringBuilder builder,
            string text,
            int indent)
        {
            // 100 characters should be more than large enough for any indent
            DebugUtil.Assert(indent >= 0 && indent <= 100);

            if (String.IsNullOrEmpty(text))
            {
                return;
            }

            int width = this.ConsoleWidth - indent - 1;
            if (width <= 0)
            {
                builder.Append(text);
                return;
            }

            // the first line

            int count = indent;
            if (builder.Length >= indent)
            {
                builder.Append(System.Environment.NewLine);
                builder.Append(' ', count);
            }
            else
            {
                count -= builder.Length;
                builder.Append(' ', count);
            }

            if (text.Length <= width)
            {
                builder.Append(text);
                builder.Append(System.Environment.NewLine);
                return;
            }

            builder.Append(text.Substring(0, width));
            builder.Append(System.Environment.NewLine);
            text = text.Substring(width);

            while (true)
            {
                builder.Append(' ', indent);

                if (text.Length <= width)
                {
                    builder.Append(text);
                    builder.Append(System.Environment.NewLine);
                    return;
                }

                builder.Append(text.Substring(0, width));
                builder.Append(System.Environment.NewLine);
                text = text.Substring(width);
            }
        }

        //------------------------------------------------------------
        // ConsoleOutput.Indent
        //
        /// <summary>
        /// Output ' ' up to the specified position.
        /// </summary>
        /// <param name="indent"></param>
        //------------------------------------------------------------
        protected void Indent(int indent)
        {
            if (ConsoleWidth > 0 && indent >= ConsoleWidth)
            {
                indent = 0;
            }
            if (currentColumn > indent)
            {
                WriteLine();
            }

            DebugUtil.Assert(indent >= currentColumn);
            PrintInternal(new String(' ', indent - currentColumn), false);
            DebugUtil.Assert(currentColumn == indent);
        }

        //------------------------------------------------------------
        // ConsoleOutput.ConvertString
        //
        /// <summary>
        /// Return System.Text.Encoding.Default.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual protected string ConvertString(string srcStr)
        {
            if (String.IsNullOrEmpty(srcStr))
            {
                return srcStr;
            }
            Exception excp = null;

            if (this.outputEncoding != null &&
                this.outputEncoding != this.defaultEncoding)
            {
                try
                {
                    byte[] srcBytes = this.defaultEncoding.GetBytes(srcStr);
                    byte[] dstBytes = Encoding.Convert(
                        this.defaultEncoding,
                        this.outputEncoding,
                        srcBytes);
                    return this.outputEncoding.GetString(dstBytes);
                }
                catch (ArgumentException ex)
                {
                    excp = ex;
                }
                if (excp != null)
                {
                }
            }
            return srcStr;
        }
    }

    //======================================================================
    // class CscConsoleOutput
    //
    // ConsoleOutput contains all shared code relative to console output for both CSC and Alink.
    // Be sure to integrate any changes into both both.
    //
    // class CscConsoleOutput    コマンドラインコンパイラ用コンソール出力クラス
    // 文字列リソースの ID の型は CSCERRID。
    //======================================================================
    internal class CscConsoleOutput : ConsoleOutput
    {
        //------------------------------------------------------------
        // CscConsoleOutput  Fields and Properties
        //------------------------------------------------------------

        internal override int ConsoleWidth
        {
            get { return System.Console.WindowWidth; }
        }

        //------------------------------------------------------------
        // CscConsoleOutput  Constructor
        //------------------------------------------------------------
        internal CscConsoleOutput(IConsole cons)
            : base(cons)
        {
            //warningMessageForRelatedLocation = "(Location of symbol related to previous warning)";
            //errorMessageForRelatedLocation = "(Location of symbol related to previous error)";
            errorPrefix = "CS";
        }

        //------------------------------------------------------------
        // CscConsoleOutput.PrintBanner
        //------------------------------------------------------------
        override internal void PrintBanner()
        {
            string fileVersion = "2.0.0001";
            Version ver = System.Environment.Version;

#if false
            this.PrintString(CSCSTRID.BANNER1, false, false);
            this.PrintInternal(fileVersion, true);
            //this.PrintString(CSCSTRID.BANNER1PART2, true, false);
            this.Print("for Microsoft (R) .NET Framework version {0}.{1}", ver.Major, ver.Minor);
            this.PrintString(CSCSTRID.BANNER2, true, false);
#else
            this.PrintInternal("Microsoft (R) Shared Source CLI C# Compiler version ", false);
            this.PrintInternal(fileVersion, true);
            this.PrintInternal(
                String.Format("for Microsoft (R) .NET Framework version {0}.{1}", ver.Major, ver.Minor),
                true);
            this.PrintInternal("Copyright (C) Microsoft Corporation. All rights reserved.", true);
#endif
            this.WriteLine();
        }
    }

    //======================================================================
    // class ALinkConsoleOutput
    //
    // ConsoleOutput contains all shared code relative to console output for both CSC and Alink.
    // Be sure to integrate any changes into both both.
    //
    // class CscConsoleOutput    文字列リソースの ID の型は ALSTRID。
    //======================================================================
    internal class ALinkConsoleOutput : ConsoleOutput
    {
        //------------------------------------------------------------
        // ALinkConsoleOutput  Constructor
        //------------------------------------------------------------
        internal ALinkConsoleOutput(IConsole cons)
            : base(cons)
        {
            errorPrefix = "ALINK";
        }
    }
}
