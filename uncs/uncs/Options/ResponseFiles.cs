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
// CSC CSConsoleArgs.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
#define CLIENT_IS_CSC

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // class COptionManager (partial)
    //======================================================================
    internal partial class COptionManager
    {
#if false
        //------------------------------------------------------------
        // CSConsoleArgs Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// string array of arguments to the command line compiler.
        /// </summary>
        private List<string> optionList = null;    // WCBuffer* m_rgArgs; WStrList* m_listArgs;

        internal List<string> OptionList
        {
            get { return optionList; }
        }

        private ConsoleOutput consoleOutput = null; // * m_output

        internal bool HadError
        {
            get
            {
                if (consoleOutput != null)
                {
                    return consoleOutput.HadError;
                }
                return false;
            }
        }

        internal bool HadFatalError
        {
            get
            {
                if (consoleOutput != null)
                {
                    return consoleOutput.HadFatalError;
                }
                return false;
            }
        }

        //------------------------------------------------------------
        // CSConsoleArgs Constructor
        //
        /// <summary></summary>
        /// <param name="output"></param>
        //------------------------------------------------------------
        internal CSConsoleArgs(ConsoleOutput output)
        {
            consoleOutput = output;
        }
#endif
        //------------------------------------------------------------
        // CSConsoleArgs.GetFullFileName    (1)
        //
        /// <summary>
        /// Get the full path of the specified file, in lower case.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="preserveCase"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetFullFileName(string fileName, bool preserveCase)
        {
            bool fileExists;
            Exception excp = null;
            string fullName = null;

            fullName = IOUtil.GetCanonFilePath(fileName, true, out fileExists, out excp);

            if (!preserveCase)
            {
                fullName.ToLower();
            }
            return fullName;
        }

        //------------------------------------------------------------
        // CSConsoleArgs.GetFullFileName    (2)
        //
        /// <summary>
        /// Get the full path of the specified file.
        /// </summary>
        //------------------------------------------------------------
        internal string GetFullFileName(string fileName)
        {
            return GetFullFileName(fileName, true);
        }

        //------------------------------------------------------------
        // CSConsoleArgs.ProcessResponseFiles
        //
        /// <summary>
        /// <para>(ConsoleArgs::GetArgs in sscli)</para>
        /// <para>(sscli)
        /// returns false and returns 0/NULL if there are previous errors.</para>
        /// <para>(sscli)
        /// Copy an argument list into an argv array.
        /// The argv array is allocated, the arguments themselves are not allocated
        /// -- just pointer copied.</para>
        /// <para>
        /// <list type="bullet">
        /// <item>If /noconfig is not specified, add each line of csc.rsp to argument list.</item>
        /// <item>If response files are specified, add each line of response files to argument list.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="args">argument list</param>    
        /// <remarks>
        /// No exception will be caught in this method.
        /// </remarks>
        //------------------------------------------------------------
        internal bool ProcessResponseFiles()
        {
            if (this.Controller.HadFatalError)
            {
                return false;
            }

            // Process '/noconfig', and csc.rsp, modifying the argument list
            ExamineNoConfigOption();

            // Process Response Files
            ExpandResponseFiles();

            return !this.Controller.HadError;
        }

        //------------------------------------------------------------
        // CSConsoleArgs.wcstoul64
        //------------------------------------------------------------
        // to \Utilities

        //------------------------------------------------------------
        // CSConsoleArgs.ExamineNoConfigOption
        //
        /// <summary>
        /// <para>(ConsoleArgs::ProcessAutoConfig in sscli)</para>
        /// <para>Process '/noconfig', and CSC.CFG, modifying the argument list</para>
        /// <para>Process Auto Config options:
        /// #1 search for '/noconfig'
        /// if not present and csc.cfg exists in EXE dir, inject after env var stuff</para>
        /// <para>/noconfig が指定されていなければ、
        /// csc.rsp ファイルの内容をオプションリストに追加する。</para>
        /// </summary>
        //------------------------------------------------------------
        private void ExamineNoConfigOption()
        {
            this.NoConfig = false;

            //--------------------------------------------------
            // Scan the argument list for the "/noconfig" options. If present, just kill it and bail.
            // /noconfig または -noconfig が指定されていれば foundNoConfig に true をセットする。
            //--------------------------------------------------
            for (int i = 0; i < this.OptionList.Count; ++i)
            {
                string[] opt= this.OptionList[i];

                // Skip everything except options
                if ((opt.Length < 3 || opt[1] != "/"))
                {
                    continue;
                }

                if (String.Compare(opt[2], "noconfig", true) == 0)
                {
                    this.NoConfig = true;
                    // /noconfig オプションは引数リストから削除する。
                    // 複数回指定されている可能性もあるので break しない。
                    this.OptionList[i] = null;
                }
            }

            // /noconfig が指定されている場合は終了する。
            if (this.NoConfig)
            {
                return;
            }

            //--------------------------------------------------
            // 指定されていない場合は csc.rsp をレスポンスファイルとしてオプションに追加する。
            // まずこのアセンブリと同じディレクトリに csc.rsp があるか調べる。
            //--------------------------------------------------
            string rspFullName = null;
            System.IO.FileInfo finfo = null;
            Exception excp = null;

            try
            {
                rspFullName = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetAssembly(typeof(Util)).Location),
                    @"csc.rsp");
            }
            catch (ArgumentException ex)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, ex);
                return;
            }
            catch (PathTooLongException ex)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, ex);
                return;
            }

            if (!IOUtil.CreateFileInfo(rspFullName, out finfo, out excp) || finfo == null)
            {
                if (excp != null)
                {
                    this.Controller.ReportError(ERRORKIND.ERROR, excp);
                }
                else
                {
                    this.Controller.ReportInternalCompilerError("ExamineNoConfigOption");
                }
                return;
            }

            if (!finfo.Exists)
            {
                // In this project, mscorlib.dll and system.dll are always imported.
                // So, it is not an error that csc.rsp is not found.
                this.Controller.WriteLine("no csc.rsp");
                return;
            }
            // If cannot open csc.rsp, show warning.
            if ((finfo.Attributes & (
                System.IO.FileAttributes.Compressed |
                System.IO.FileAttributes.Directory |
                System.IO.FileAttributes.Encrypted)) != 0)
            {
                this.Controller.ReportError(ERRORKIND.WARNING, "csc.rsp could not be opened.");
                return;
            }

            //--------------------------------------------------
            // この時点では実際に csc.rsp が存在し読み取り可能である。
            // 先頭に @ をつけてオプションリストの先頭に追加する。
            //--------------------------------------------------
            this.OptionList.Insert(0, new string[3] { "@" + rspFullName, "@", rspFullName });
        }

        //------------------------------------------------------------
        // CSConsoleArgs.TextToArgs
        //
        /// <summary>
        /// <para>Parse the text into a list of argument return the total count
        /// and set 'args' to point to the last list element's 'next'
        /// This function assumes the text is NULL terminated</para>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="optionList"></param>
        //------------------------------------------------------------
        private void TextToArgs(string text, List<string[]> optionList)
        {
            if (String.IsNullOrEmpty(text) || optionList == null)
            {
                return;
            }

            int idx = 0;    // text 内の位置を示すインデックス。
            StringBuilder bufTemp = new StringBuilder();
            int countBSlash = 0;
            int countQuotes = 0;
            char illegalChar = '\0';
            List<string> optStrList = new List<string>();

            while (idx < text.Length && text[idx] != '\0')
            {
            LEADINGWHITE:
                // 空白文字とコメント（# から行末まで）を読み飛ばす。
                while (idx < text.Length && System.Char.IsWhiteSpace(text[idx]))
                {
                    ++idx;
                }
                if (idx >= text.Length)
                {
                    break;
                }
                if (text[idx] == '#')
                {
                    ++idx;
                    while (idx < text.Length && text[idx] != '\r' && text[idx] != '\n')
                    {
                        ++idx;
                    }
                    goto LEADINGWHITE;
                }

                bufTemp.Length = 0;
                countQuotes = 0;
                illegalChar = '\0';

                // 空白文字でない、または引用符内の文字をまとめて１つのオプションとする。
                while (idx < text.Length &&
                    (!System.Char.IsWhiteSpace(text[idx]) || (countQuotes & 1) != 0))
                {
                    char ch = text[idx];
                    if (ch == '\\')
                    {
                        // All this weird slash stuff follows the standard argument processing routines
                        // 連続する \ を数える。
                        countBSlash = 0;
                        // Copy and advance while counting slashes
                        while (idx < text.Length && text[idx] == '\\')
                        {
                            bufTemp.Append(text[idx]);
                            ++idx;
                            ++countBSlash;
                        }
                        if (idx >= text.Length)
                        {
                            break;
                        }

                        // Slashes not followed by a quote character don't matter now
                        // \ の次が " でないなら通常の文字として処理する。

                        // If there's an odd count of slashes, it's escaping the quote
                        // Otherwise the quote is a quote
                        // \ が奇数個なら " はエスケープされている。
                        // \ が偶数個なら " は引用符として処理する。

                        if (text[idx] == '\"' && (countBSlash & 1) == 0)
                        {
                            ++countQuotes;
                        }

                        bufTemp.Append(text[idx]);
                        ++idx;
                    }
                    else if (ch == '\"')
                    {
                        ++countQuotes;
                        bufTemp.Append(text[idx]);
                        ++idx;
                    }
                    else if ((ch >= '\x1' && ch <= '\x1F') || ch == '|')
                    {
                        // Save the first legal character and skip over them
                        if (illegalChar == '\0')
                        {
                            illegalChar = ch;
                        }
                        ++idx;
                    }
                    if (ch == '\0')
                    {
                        break;
                    }
                    else
                    {
                        // Copy the char and advance
                        bufTemp.Append(text[idx]);
                        ++idx;
                    }
                }

                // If the string is surrounded by quotes, with no interior quotes, remove them.
                // 両端が引用符で、内部に引用符がないなら、それらを取り除く。
                if (countQuotes == 2)
                {
                    if (bufTemp.Length <= 2)
                    {
                        continue;
                    }
                    if ((bufTemp[0] == '\"') &&
                        (bufTemp[bufTemp.Length - 1] == '\"'))
                    {
                        bufTemp.Remove(0, 1);
                        bufTemp.Remove(bufTemp.Length - 1, 1);
                    }
                }

                // 使用できない文字が見つかった場合は、エラーメッセージを表示する。
                // そうでない場合はオプションリストに追加する。
                if (illegalChar == '\0')
                {
                    optStrList.Add(bufTemp.ToString().Trim());
                }
                else
                {
                    // エラーメッセージを表示する。
                    this.Controller.ReportError(
                        CSCERRID.ERR_IllegalOptionChar,
                        ERRORKIND.ERROR,
                        String.Format("0x{0,4:X4}", illegalChar));
                }
            }

            OptionUtil.ArgumentsToOptionList(optStrList, optStrList.Count, optionList);
        }

        //------------------------------------------------------------
        // CSConsoleArgs.ExpandResponseFiles
        //
        /// <summary>
        /// <para>(ConsoleArgs::ProcessResponseArgs in sscli.)</para>
        /// <para>Process Response files on the command line
        /// Returns true if it allocated a new argv array that must be freed later</para>
        /// <para>コマンドラインでレスポンスファイルが指定されていたら、
        /// その内容をオプションリストに追加する。
        /// （レスポンスファイルは @ファイル名 の形式で指定されている。）</para>
        /// </summary>
        //------------------------------------------------------------
        private void ExpandResponseFiles()
        {
            Dictionary<string, bool> processedResponseFiles
                = new Dictionary<string, bool>(new StringEqualityComparerIgnoreCase());
            List<string[]> listTemp = null;
            string fileName = null;
            string fullName = null;
            string fileContents = null;
            Exception excp = null;
            bool foundResponseFile;
            int loopCount = 0;
            const int maxLoopCount = 32;

            do
            {
                if (loopCount >= maxLoopCount)
                {
                    throw new Exception("Too many nested response file.");
                }

                listTemp = new List<string[]>();
                foundResponseFile = false;
                foreach (string[] opt in this.OptionList)
                {
                    // オプションが null や空の場合は破棄する。
                    if (opt == null || opt.Length == 0)
                    {
                        continue;
                    }
                    // 1 文字目が @ でないものはそのままリストに追加する。
                    if (opt[1] != "@")
                    {
                        listTemp.Add(opt);
                        continue;
                    }

                    if (opt.Length <= 2 || String.IsNullOrEmpty(opt[2]))
                    {
                        this.Controller.ReportError(
                            CSCERRID.ERR_NoFileSpec,
                            ERRORKIND.ERROR,
                            opt[0]);
                        continue;
                    }

                    // Check for duplicates
                    // @ と引用符を外す。
                    fileName = opt[2];

                    IOUtil.RemoveQuotes(ref fileName);

                    if (String.IsNullOrEmpty(fileName))
                    {
                        this.Controller.ReportError(
                            CSCERRID.ERR_NoFileSpec,
                            ERRORKIND.ERROR,
                            opt[0]);
                        continue;
                    }

                    // ファイルのフルパスを求める。
                    fullName = this.GetFullFileName(fileName, false);
                    if (String.IsNullOrEmpty(fullName))
                    {
                        this.Controller.ReportError(
                            CSCERRID.ERR_NoFileSpec,
                            ERRORKIND.ERROR,
                            opt[0]);
                        continue;
                    }

                    // 得られたファイルがすでに処理されていないか調べる。
                    try
                    {
                        if (processedResponseFiles.ContainsKey(fullName))
                        {
                            continue;
                        }
                    }
                    catch (ArgumentException)
                    {
                        DebugUtil.Assert(false);
                        continue;
                    }

                    // ファイルの内容を読み込む。エラーの場合はメッセージを表示して次へ。
                    if (!IOUtil.ReadTextFile(fullName, null, out fileContents, out excp))
                    {
                        this.Controller.ReportError(ERRORKIND.ERROR, excp.Message);
                        continue;
                    }

                    // レスポンスファイル内で指定されているオプションを取得する。
                    TextToArgs(fileContents, listTemp);
                    foundResponseFile = true;

                    // 処理済みファイルのリストに追加する。
                    processedResponseFiles.Add(fullName, true);
                }
                this.OptionList = listTemp;
                ++loopCount;
            } while (foundResponseFile);
        }
    }
}
