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

////////////////////////////////////////////////////////////////////////////////
// FILE_CAN.H

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
////////////////////////////////////////////////////////////////////////////////
// FILE.CPP
//
// Contains common code for file manipulations (i.e. concatinating filenames, opening text files, manipulating paths)
// this file is 'shared' with ALink and C# so make sure to integrate
// any changes into both files

//============================================================================
// IO.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Uncs
{
    //======================================================================
    // enum FileType	(ft)
    //======================================================================
    internal enum FileType : int
    {
        Unknown = 0,
        Unicode,
        SwappedUnicode,
        UTF8,
        ASCII,
        Binary
    }

    //======================================================================
    // class IOUtil	(static)
    //======================================================================
    static internal class IOUtil
    {
        //------------------------------------------------------------
        // IOUtil.IsPathRooted
        //
        /// <summary>
        /// <para>>Path.IsPathRooted determines
        /// that a relative path with a drive name is an absolute path. 
        /// For example, it returns true for @"C:User\temp.txt". (CLS 2.0)</para>
        /// <para>This method</para>
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsPathRooted(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                return false;
            }

            int i = 0;
            while (Char.IsWhiteSpace(path[i])) ++i;
            if (path.Length - i >= 3 &&
                CharUtil.IsAsciiAlphabet(path[i]) &&
                path[i + 1] == Path.VolumeSeparatorChar &&
                path[i + 2] != Path.DirectorySeparatorChar)
            {
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // IOUtil.CreateDirectoryInfo
        //
        /// <summary>
        /// <para>Create DirectoryInfo instance by path string.</para>
        /// <para>Not catch UnauthorizedAccessException and
        /// NotSupportedException in this method.</para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dirInfo"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool CreateDirectoryInfo(
            string path,
            out DirectoryInfo dirInfo,
            out Exception excp)
        {
            dirInfo = null;
            excp = null;

            try
            {
                dirInfo = new DirectoryInfo(path);
                return (dirInfo != null);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (PathTooLongException ex)
            {
                excp = ex;
            }
            catch (System.Security.SecurityException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // IOUtil.CreateDirectoryInfoFromFilePath
        //
        /// <summary>
        /// <para>Create DirectoryInfo instance by path string.
        /// If the string is a simple file name, assume that the file is in the current directory.</para>
        /// <para>Not determine if the directory is exist.</para>
        /// <para>Not catch UnauthorizedAccessException、NotSupportedException in this method.</para>
        /// </summary>
        /// <param name="dirName">path string.</param>
        //------------------------------------------------------------
        static internal bool CreateDirectoryInfoFromFilePath(
            string path,
            out DirectoryInfo dirInfo,
            out Exception excp)
        {
            dirInfo = null;
            excp = null;
            string dir = null;

            // Get the directory part from path.
            try
            {
                dir = Path.GetDirectoryName(path);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (PathTooLongException ex)
            {
                excp = ex;
            }

            if (excp != null)
            {
                return false;
            }
            if (dir == null)
            {
                // dir is null if path is of a root directory, not of a file.
                excp = new ArgumentException(String.Format("{0} is not a file path.", path));
                return false;
            }

            // Create a DirectoryInfo instance.
            try
            {
                if (dir.Length == 0)
                {
                    dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
                }
                else
                {
                    dirInfo = new DirectoryInfo(dir);
                }
                return (dirInfo != null);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (PathTooLongException ex)
            {
                excp = ex;
            }
            catch (System.Security.SecurityException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // IOUtil.CreateFileInfo
        //
        /// <summary>
        /// <para>Create a System.IO.FileInfo instance.
        /// Catch some exception in this method.</para>
        /// <para>Not determine if the file exits.</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileInfo"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool CreateFileInfo(
            string fileName,
            out FileInfo fileInfo,
            out Exception excp)
        {
            fileInfo = null;
            excp = null;

            try
            {
                fileInfo = new FileInfo(fileName);
                return true;
            }
            catch (ArgumentException ex)
            {
                // ArgumentNullException is included.
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (System.IO.PathTooLongException ex)
            {
                excp = ex;
            }
            catch (System.Security.SecurityException ex)
            {
                excp = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // IOUtil.FileExists
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <param name="fileInfo"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool FileExists(
            string fileName,
            out FileInfo fileInfo,
            out Exception excp)
        {
            excp = null;
            fileInfo = null;

            if (!CreateFileInfo(fileName, out fileInfo, out excp))
            {
                return false;
            }

            if (fileInfo.Exists)
            {
                return true;
            }
            excp = new FileNotFoundException(String.Format("'{0}' could not be found", fileName));
            return false;
        }

        //------------------------------------------------------------
        // IOUtil.EnumFileInfo
        //
        /// <summary>Enumerate the files matching conditions in the specified directory.</summary>
        /// <param name="dirInfo">DirectoryInfo instance of the directory where files are enumerated.</param>
        /// <param name="fileInfoList">List instance storing FileInfos of the matching files.</param>
        /// <param name="recursiveSearch">If true, enumerate subdirectories.</param>
        /// <param name="searchPattern">file name or pattern for matching. Can use wild cards.</param>
        /// <param name="withAttribute">File attributes which files should have.</param>
        /// <param name="withoutAttribute">File attributes which files should not have.</param>
        /// <remarks>
        /// <para>
        /// In the specifiled directory, add all the FileInfos of the files which
        /// <list type="bullet">
        /// <item><description>match to searchPattern, </description></item>
        /// <item><description>have all the attributes in withAttribute, </description></item>
        /// <item><description>have none of withoutAttribute</description></item>
        /// </list>to fileInfoList.
        /// </para>
        /// <para>Not catch any exceptions.</para>
        /// </remarks>
        //------------------------------------------------------------
        static internal void EnumFileInfo(
            System.IO.DirectoryInfo dirInfo,
            string searchPattern,
            System.IO.FileAttributes withAttribute,
            System.IO.FileAttributes withoutAttribute,
            List<System.IO.FileInfo> fileInfoList,
            bool recursiveSearch)
        {
            if (dirInfo == null || String.IsNullOrEmpty(searchPattern) || fileInfoList == null)
            {
                return;
            }
            SearchOption searchOption =
                (recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            // Find the matching files in this directory.
            FileInfo[] fileInfos = dirInfo.GetFiles(searchPattern, searchOption);

            // 
            foreach (System.IO.FileInfo fi in fileInfos)
            {
                if (withAttribute != 0)
                {
                    if ((fi.Attributes & (withAttribute)) == 0)
                    {
                        continue;
                    }
                }
                if (withoutAttribute != 0)
                {
                    if ((fi.Attributes & (withoutAttribute)) != 0)
                    {
                        continue;
                    }
                }
                fileInfoList.Add(fi);
            }
        }

        //------------------------------------------------------------
        // IOUtil.GetFullPath
        //
        /// <summary>
        /// Get the FullName of the specified file name.
        /// If not exist. throw exception.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetFullPath(string fileName, out Exception excp)
        {
            FileInfo finfo = null;
            string fullPah = null;

            if (!CreateFileInfo(fileName, out finfo, out excp))
            {
                return null;
            }
            if (!finfo.Exists)
            {
                excp = new FileNotFoundException(finfo.FullName);
            }
            fullPah = finfo.FullName;
            return fullPah;
        }

        //------------------------------------------------------------
        // IOUtil.GetFileName
        //
        /// <summary></summary>
        /// <param name="pathStr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetFileName(string pathStr)
        {
            try
            {
                return Path.GetFileName(pathStr);
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // IOUtil.GetFileNameWithoutExtension
        //
        /// <summary></summary>
        /// <param name="pathStr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetFileNameWithoutExtension(string pathStr)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(pathStr);
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        static private char[] wildcards = { '*', '?' };

        //------------------------------------------------------------
        // IOUtil.HasWildcard
        //
        /// <summary>
        /// If a file name string has wild card characters, return true.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool HasWildcard(string fileName)
        {
            string fname = null;
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }

            try
            {
                fname = Path.GetFileName(fileName);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (String.IsNullOrEmpty(fname))
            {
                return false;
            }
            return (fname.IndexOfAny(wildcards) >= 0);
        }

        //------------------------------------------------------------
        // IOUtil.ReadTextFile
        //
        /// <summary>
        /// Read a text file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="fileEncode">Encoding instace of a text file.</param>
        /// <param name="fileContents">Set a text.</param>
        /// <param name="excp"></param>
        /// <returns>If failed to read, return false.</returns>
        //------------------------------------------------------------
        static internal bool ReadTextFile(
            string fileName,
            Encoding fileEncode,
            out string fileContents,
            out Exception excp)
        {
            excp = null;
            fileContents = null;
            bool br = true;

            StreamReader reader = null;
            if (fileEncode == null)
            {
                fileEncode = System.Text.Encoding.Default;
            }

            try
            {
                reader = new StreamReader(fileName, fileEncode);
                fileContents = reader.ReadToEnd();
            }
            catch (ArgumentNullException ex)
            {
                excp = ex;
                br = false;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
                br = false;
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                excp = ex;
                br = false;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                excp = ex;
                br = false;
            }
            catch (UnauthorizedAccessException ex)
            {
                excp = ex;
                br = false;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
                br = false;
            }
            catch (System.IO.IOException ex)
            {
                excp = ex;
                br = false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return br;
        }

        //------------------------------------------------------------
        // IOUtil.ReadBinaryFile
        //
        /// <summary>
        /// Read a binary file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool ReadBinaryFile(
            string filePath,
            out byte[] data,
            out Exception excp)
        {
            data = null;
            excp = null;

            try
            {
                data = File.ReadAllBytes(filePath);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (PathTooLongException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (DirectoryNotFoundException ex)
            {
                excp = ex;
            }
            catch (FileNotFoundException ex)
            {
                excp = ex;
            }
            catch (IOException ex)
            {
                excp = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                excp = ex;
            }
            catch (System.Security.SecurityException e)
            {
                excp = e;
            }
            return false;
        }

        //------------------------------------------------------------
        // IOUtil.WriteBinaryFile
        //
        /// <summary>
        /// Write binary data.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool WriteBinaryFile(
            string filePath,
            byte[] data,
            out Exception excp)
        {
            excp = null;

            try
            {
                File.WriteAllBytes(filePath, data);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (PathTooLongException ex)
            {
                excp = ex;
            }
            catch (DirectoryNotFoundException ex)
            {
                excp = ex;
            }
            catch (IOException ex)
            {
                excp = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (System.Security.SecurityException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // IOUtil.DeleteFile
        //
        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool DeleteFile(string filePath, out Exception excp)
        {
            excp = null;

            try
            {
                System.IO.File.Delete(filePath);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (DirectoryNotFoundException ex)
            {
                excp = ex;
            }
            catch (PathTooLongException ex)
            {
                excp = ex;
            }
            catch (IOException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // IOUtil.SelectFileName
        //
        /// <summary>
        /// Return FileInfo.Name or FileInfo.FullName.
        /// </summary>
        /// <param name="fInfo"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string SelectFileName(FileInfo fInfo, bool fullPath)
        {
            if (fInfo != null)
            {
                return fullPath ? fInfo.FullName : fInfo.Name;
            }
            return null;
        }

        //============================================================
        // move from CSharp\Inc\File_Can.cs
        //============================================================

        //------------------------------------------------------------
        // IOUtil.NormalizeFilePath
        //
        /// <summary>
        /// <para>パス文字列に以下の処理を施す。
        /// <list type="bullet">
        /// <item>両端が引用符ならそれらをはずす。</item>
        /// <item>パス中で \ が連続していれば \ 1 文字に置き換える。
        /// （先頭の \\ はそのまま）</item>
        /// </list></para>
        /// </summary>
        /// <param name="fileName">対象の文字列。</param>
        //------------------------------------------------------------
        static internal void NormalizeFilePath(ref string fileName)
        {
            if (fileName == null || fileName.Length <= 0)
            {
                return;
            }

            StringBuilder buffer = new StringBuilder();
            int start, end, length;

            // 両端が引用符の場合はそれらをはずす。
            // なお、\ はエスケープ文字ではないので、\" も \ と " として扱う。
            if (fileName[0] == '"' && fileName.Length >= 2 && fileName[fileName.Length - 1] == '"')
            {
                start = 1;
                end = fileName.Length - 1;
                length = end - start;
            }
            else
            {
                start = 0;
                end = fileName.Length;
                length = end - start;
            }
            if (start >= end)
            {
                fileName = "";
                return;
            }

            // Replace '\\' with single backslashes in paths,
            // because W_GetFullPathName fails to do this on win9x.
            // 最後から１文字前まで。
            --end;
            int j = start;

            // UNC paths start with '\\' so skip the first character if it is a backslash.
            // 先頭で \ が連続している場合、\\、\\\、\\\\ はいずれも \\ とみなす。
            int count = 0;
            int se = (length > 4 ? 4 : length);
            while (count < se && fileName[j] == '\\')
            {
                ++count;
                ++j;
            }
            if (count == 1)
            {
                buffer.Append('\\');
            }
            else if (count > 1)
            {
                buffer.Append("\\\\");
            }

            // 2 文字が \\ なら 1 文字目を飛ばす。
            while (j < end)
            {
                if (fileName[j] == '\\' && fileName[j + 1] == '\\')
                {
                    ++j;
                }
                else
                {
                    buffer.Append(fileName[j]);
                    ++j;
                }
            }
            // 最後の文字はそのまま追加すればよい。
            if (j <= end) buffer.Append(fileName[j]);

            fileName = buffer.ToString();
        }

        //------------------------------------------------------------
        // IOUtil.GetCanonFileInfo
        //
        /// <summary>
        /// <para>Src and Dest may be the same buffer
        /// Returns 0 for error (check via GetLastError()) or count of characters
        /// (not including NULL) copied to Dest.
        /// if fPreserveSrcCasing is set, ignores on-disk casing of filename
        /// (but still gets on-disk casing of directories) 
        /// if fPreserveSrcCasing is set and and existing file matches with different short/longness
        /// it will fail and set the error code to ERROR_FILE_EXISTS</para>
        /// </summary>
        /// <param name="srcFileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal FileInfo GetCanonFileInfo(string srcFileName, out Exception excp)
        {
            FileInfo finfo = null;
            excp = null;

            // Remove quotes. replace \\ with \ unless \\ is not at the top of the string.
            NormalizeFilePath(ref srcFileName);

            // Create a FileInfo by srcFileName.
            if (!IOUtil.CreateFileInfo(srcFileName, out finfo, out excp))
            {
                //throw new Exception(errorMessage);
                return null;
            }

            return finfo;
        }

        //------------------------------------------------------------
        // IOUtil.GetCanonFilePath
        //
        /// <summary>
        /// <para>(sscli) GetCanonFilePath (csharp\inc\file_can.h)</para>
        /// </summary>
        /// <param name="srcFileName"></param>
        /// <param name="preserveCase"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetCanonFilePath(
            string srcFileName,
            bool preserveCase,
            out bool exists,
            out Exception excp)
        {
            exists = false;

            FileInfo finfo = GetCanonFileInfo(srcFileName, out excp);

            if (finfo != null)
            {
                exists = finfo.Exists;
                if (preserveCase)
                {
                    return finfo.FullName;
                }
                else
                {
                    return finfo.Name.ToLower();
                }
            }
            exists = false;
            return null;
        }

        //------------------------------------------------------------
        // IOUtil.ProcessSlashes
        //
        /// <summary>
        /// <para>(sscli) ProcessSlashes (csharp\share\file.cpp)</para>
        /// <para>All this weird slash stuff follows the standard argument processing routines</para>
        /// <para>\\ を \ に変換する。また \ が " をエスケープしていないか調べる。</para>
        /// </summary>
        /// <param name="idx">次の処理位置のインデックスがセットされる。</param>
        /// <param name="outBuf">変換後の文字列がセットされる。</param>
        /// <param name="srcText">対象のテキスト。</param>
        /// <returns>処理が終了した時点で、引用符内にあれば true を返す。</returns>
        //------------------------------------------------------------
        static internal bool ProcessSlashes(
            System.Text.StringBuilder outBuf,
            string srcText,
            ref int idx)
        {
            //size_t iSlash = 0;
            //LPCWSTR pCur = *pszCur;
            //bool fIsQuoted = false;

            int iSlash = 0;
            bool isQuoted = false;

            //while (*pCur == L'\\')
            //    iSlash++, pCur++;
            while (srcText[idx] == '\\')
            {
                ++iSlash;
                ++idx;
            }

            if (srcText[idx] == '\"')
            {
                // Slashes followed by a quote character
                // put one slash in the output for every 2 slashes in the input
                for (; iSlash >= 2; iSlash -= 2)
                {
                    outBuf.Append('\\');
                }

                // If there's 1 remaining slash, it's escaping the quote
                // so ignore the slash and keep the quote (as a normal character)
                if ((iSlash & 1) != 0)
                { // Is it odd?
                    outBuf.Append(srcText[idx]);
                    ++idx;
                }
                else
                {
                    // A regular quote, so eat it and change the bQuoted
                    ++idx;
                    isQuoted = true;
                }
            }
            else
            {
                // Slashs not followed by a quote are just slashes
                for (; iSlash > 0; iSlash--)
                {
                    outBuf.Append('\\');
                }
            }
            return isQuoted;
        }

        //------------------------------------------------------------
        // IOUtil.RemoveQuotes
        //
        /// <summary>
        /// <para>(sscli) RemoveQuotes (csharp\share\file.cpp)</para>
        /// <para>Remove quote marks from a string
        /// The translation is done in-place, and the argument is returned.</para>
        /// </summary>
        /// <param name="quotedStr">対象の文字列。</param>
        //------------------------------------------------------------
        static internal void RemoveQuotes(ref string quotedStr)
        {
            if (quotedStr == null || quotedStr.Length <= 1)
            {
                return;
            }

            System.Text.StringBuilder sbTemp = new StringBuilder(quotedStr.Length);
            int idx = 0;
            char ch;

            for (; idx < quotedStr.Length; )
            {
                ch = quotedStr[idx];
                switch (ch)
                {
                    case '\\':
                        ProcessSlashes(sbTemp, quotedStr, ref idx);
                        // Not break because ProcessSlashes has already advanced pIn
                        continue;

                    case '\"':
                        break;

                    default:
                        sbTemp.Append(ch);
                        break;
                }
                ++idx;
            }

            quotedStr = sbTemp.ToString();
        }
    }

    //============================================================
    // move from CSharp\Shared\File.cs
    //============================================================

#if false
    static internal class CShared
    {
        //------------------------------------------------------------
        // ProcessSlashes
        //
        /// <summary>
        /// <para>(sscli) ProcessSlashes (csharp\shared\fielcpp)</para>
        /// <para>（連続する）\ とその直後の文字を解釈し、バッファに追加する。
        /// さらに " が続いているならそれを読み飛ばし、
        /// 以降が引用符内であることを示すために true を返す。
        /// それ以外なら引用符内ではないことを示すために false を返す。</para>
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="index"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool ProcessSlashes(
            string sourceText,
            ref int index,
            System.Text.StringBuilder buffer)
        {
            if (sourceText == null || index < 0)
            {
                return false;
            }

            // All this weird slash stuff follows the standard argument processing routines
            int slashCount = 0;
            int idx = index;
            bool isQuoted = false;

            while (idx < sourceText.Length && sourceText[idx] == '\\')
            {
                slashCount++;
                idx++;
            }
            if (slashCount == 0)
            {
                return false;
            }

            if (idx < sourceText.Length && sourceText[idx] == '\"')
            {
                // Slashes followed by a quote character
                // put one slash in the output for every 2 slashes in the input
                for (; slashCount >= 2; slashCount -= 2)
                {
                    buffer.Append('\\');
                }

                // If there's 1 remaining slash, it's escaping the quote
                // so ignore the slash and keep the quote (as a normal character)
                if ((slashCount & 1) != 0)
                {
                    // Is it odd?
                    buffer.Append('\"');
                    idx++;
                }
                else
                {
                    // A regular quote, so eat it and change the bQuoted
                    idx++;
                    isQuoted = true;
                }
            }
            else
            {
                // Slashs not followed by a quote are just slashes
                for (; slashCount > 0; slashCount--) buffer.Append('\\');
            }

            index = idx;
            return isQuoted;
        }

        //------------------------------------------------------------
        // RemoveQuotesAndReplaceComma
        //
        /// <summary>
        /// <para>(sscli) RemoveQuotesAndReplaceComma (csharp\shared\fielcpp)</para>
        /// <para>Remove quote marks from a string. Also, commas (,) that are not quoted
        /// are converted to the pipe (|) character.
        /// The translation is done in-place, and the argument is returned.</para>
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string RemoveQuotesAndReplaceComma(string sourceText)
        {
            if (sourceText == null)
            {
                return null;
            }

            string input = sourceText;
            StringBuilder buffer = new StringBuilder();

            char ch;
            bool inQuote;

            int index = 0;
            inQuote = false;
            while (index < sourceText.Length)
            {
                switch (ch = sourceText[index])
                {
                    case '\0':
                        // End of string. We're done.
                        return buffer.ToString();

                    case '\"':
                        inQuote = !inQuote;
                        break;

                    case '|':
                        DebugUtil.VsFail("How did we get this here!");
                        //__assume(0);
                        goto default;

                    case '\\':
                        if (ProcessSlashes(sourceText, ref index, buffer))
                        {
                            inQuote = !inQuote;
                        }
                        // Not break because ProcessSlashes has already advanced index
                        continue;

                    case ',':
                        if (inQuote)
                        {
                            goto default;
                        }
                        buffer.Append('|');
                        break;

                    default:
                        buffer.Append(ch);
                        break;
                }
                ++index;
            }
            return buffer.ToString();
        }

        //------------------------------------------------------------
        // RemoveQuotesAndReplacePathDelim
        //
        /// <summary>
        /// <para>(sscli) RemoveQuotesAndReplacePathDelim (csharp\shared\fielcpp)</para>
        /// <para>Remove quote marks from a string. Also, commas (,) and semicolons (;)
        /// that are not quoted are converted to the pipe (|) character.
        /// The translation is done in-place, and the argument is returned.</para>
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string RemoveQuotesAndReplacePathDelim(string sourceText)
        {
            if (sourceText == null || sourceText.Length <= 0)
            {
                return null;
            }

            StringBuilder buffer = new StringBuilder();
            int index = 0;

            char ch;
            bool inQuote = false;

            while (index < sourceText.Length)
            {
                ch = sourceText[index];
                switch (ch)
                {
                    case '\0':
                        // End of string. We're done.
                        return buffer.ToString();

                    case '\\':
                        if (ProcessSlashes(sourceText, ref index, buffer))
                        {
                            inQuote = !inQuote;
                        }
                        // Not break because ProcessSlashes has already advanced index
                        continue;

                    case '\"':
                        inQuote = !inQuote;
                        break;

                    case '|':
                        DebugUtil.VsFail("How did we get this here!");
                        goto default;

                    case ',':
                    case ';':
                        if (inQuote)
                        {
                            goto default;
                        }
                        buffer.Append('|');
                        break;

                    default:
                        buffer.Append(ch);
                        break;
                }
                ++index;
            }
            return buffer.ToString();
        }

        //------------------------------------------------------------
        // RemoveQuotesAndSplit
        //
        /// <summary>
        /// 引数の文字列に対して、
        /// <list type="bullet">
        /// <item>引用符で囲まれていればそれを外す、</item>
        /// <item>指定された区切り文字で分割する、</item>
        /// </list>
        /// という処理をする。
        /// ただし、引用符内の区切り文字、エスケープされた区切り文字は通常の文字として扱う。
        /// List&lt;string&gt; を返す。（エラー時にも null ではなく空のリストを返す。）
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="delims"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal List<string> RemoveQuotesAndSplit(
            string sourceText,
            params char[] delims)
        {
            List<string> strList = new List<string>();
            if (sourceText == null || sourceText.Length <= 0)
            {
                return strList;
            }

            StringBuilder buffer = new StringBuilder();
            int index = 0;

            char ch;
            bool inQuote = false;
            bool isDelim = false;

            while (index < sourceText.Length)
            {
                ch = sourceText[index];
                switch (ch)
                {
                    case '\0':
                        // End of string. We're done.
                        if (buffer.Length > 0)
                        {
                            strList.Add(buffer.ToString());
                        }
                        return strList;

                    case '\\':
                        if (ProcessSlashes(sourceText, ref index, buffer))
                        {
                            inQuote = !inQuote;
                        }
                        // Not break because ProcessSlashes has already advanced index
                        continue;

                    case '\"':
                        inQuote = !inQuote;
                        break;

                    default:
                        isDelim = false;
                        if (!inQuote)
                        {
                            foreach (char c in delims)
                            {
                                if (c == ch)
                                {
                                    if (buffer.Length > 0)
                                    {
                                        strList.Add(buffer.ToString());
                                        buffer.Length = 0;
                                    }
                                    isDelim = true;
                                    break;
                                }
                            }
                        }
                        if (isDelim == false)
                        {
                            buffer.Append(ch);
                        }
                        break;
                }
                ++index;
            }
            if (buffer.Length > 0)
            {
                strList.Add(buffer.ToString());
            }
            return strList;
        }

        //------------------------------------------------------------
        // RemoveQuotesAndReplaceAlias
        //
        /// <summary>
        /// <para>(sscli) RemoveQuotesAndReplaceAlias (csharp\shared\fielcpp)</para>
        /// <para>Remove quote marks from a string. Also, commas (,), semicolons (;), and equals (=)
        /// that are not quoted are converted to the pipe (|) or angle ('\x01') character.
        /// The translation is done in-place, and the argument is returned.</para>
        /// <para>Parse compiler option "/reference:[alias=]filename" and create string[2].
        /// If no alias, string[0] is null.</para>
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string[] RemoveQuotesAndReplaceAlias(string sourceText)
        {
            string[] rs = { null, null };
            if (String.IsNullOrEmpty(sourceText))
            {
                return rs;
            }

            StringBuilder buffer1 = new StringBuilder();
            StringBuilder buffer2 = null;
            StringBuilder buffer = buffer1;
            int index = 0;

            char ch;
            bool inQuote = false;
            //bool seenEquals = false;

            while (index < sourceText.Length)
            {
                ch = sourceText[index];
                switch (ch)
                {
                    case '\0':
                        // End of string. We're done.
                        return rs;

                    case '\"':
                        inQuote = !inQuote;
                        break;

                    case '\\':
                        if (ProcessSlashes(sourceText, ref index, buffer))
                        {
                            inQuote = !inQuote;
                        }
                        // Not break because ProcessSlashes has already advanced index
                        continue;

                    case '=':
                        if (inQuote || buffer2 != null)
                        {
                            goto default;
                        }
                        buffer2 = new StringBuilder();
                        buffer = buffer2;
                        break;

                    default:
                        buffer.Append(ch);
                        break;
                }
                ++index;
            }

            if (buffer2 == null)
            {
                rs[1] = buffer1.ToString();
            }
            else
            {
                rs[0] = buffer1.ToString();
                rs[1] = buffer2.ToString();
            }
            return rs;
        }
    }
#endif
}
