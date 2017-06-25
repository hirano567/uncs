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
// CSourceText.cs
//
// In sscli, class CSourceText is defined in CSharp\SCC\SCC.h and CSharp\SCC\SCC.cpp.
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
    // class CSourceText
    //
    /// <summary>
    /// <para>-- implements ICSSourceText for a text file</para>
    /// <para>Store the text of a source file and output.</para>
    /// </summary>
    //======================================================================
    internal class CSourceText
    {
        //------------------------------------------------------------
        // CSourceText  Fields and Properties (1) Text
        //------------------------------------------------------------
        /// <summary>
        /// A source text.
        /// </summary>
        private string sourceText = null;

        /// <summary>
        /// (R) Get a whole text  (ICSSourceText)
        /// </summary>
        public string Text
        {
            get { return sourceText; }
        }

        /// <summary>
        /// <para>The index of the current character of sourceText.</para>
        /// </summary>
        private int charIndex = -1;

        //------------------------------------------------------------
        // CSourceText  Fields and Properties (2) File
        //------------------------------------------------------------
        private FileInfo sourceFileInfo = null;

        public FileInfo SourceFileInfo
        {
            get { return this.sourceFileInfo; }
        }

        //------------------------------------------------------------
        // CSourceText  Fields and Properties
        //------------------------------------------------------------
        private CHECKSUM sourceTextChecksum = new CHECKSUM();

        /// <summary>
        /// (R) CHECKSUM instance. (ICSSourceText)
        /// </summary>
        public CHECKSUM Checksum
        {
            get { return sourceTextChecksum; }
        }

        //------------------------------------------------------------
        // CSourceText  Constructor
        //
        /// <summary>
        /// Does nothing.
        /// </summary>
        //------------------------------------------------------------
        internal CSourceText() { }

        //------------------------------------------------------------
        // CSourceText.Initialize
        //
        /// <summary>
        /// Read the source file.
        /// </summary>
        /// <param name="fileName">source file name.</param>
        /// <param name="computeChecksum">If true, calculate the checksum.</param>
        /// <param name="encoding">Encoding instance of the source text</param>
        /// <param name="controller">To output error messages.</param>
        /// <returns>If failed to read the file, return false.</returns>
        //------------------------------------------------------------
        internal bool Initialize(
            FileInfo srcFileInfo,
            bool computeChecksum,
            System.Text.Encoding encoding,
            CController controller)
        {
            Exception excp = null;

            // Remember the file name as given to us
            this.sourceFileInfo= srcFileInfo;
            if (srcFileInfo == null)
            {
                sourceText = null;
                return false;
            }

            if (!IOUtil.ReadTextFile(srcFileInfo.FullName, encoding, out sourceText, out excp))
            {
                sourceText = null;
                controller.ReportError(ERRORKIND.ERROR, excp);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // CSourceText.CloneInMemory    (ICSSourceText)
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public CSourceText CloneInMemory()
        {
            throw new NotImplementedException("CSourceText.CloneInMemory");
        }

        //------------------------------------------------------------
        // CSourceText.GetText  (ICSSourceText)
        //
        /// <summary>
        /// Return the text and initialize the argument posEnd.
        /// </summary>
        /// <param name="posEnd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public string GetText(ref POSDATA posEnd)
        {
            if (posEnd != null)
            {
                posEnd.LineIndex = 0;
                posEnd.CharIndex = 0;
            }
            return sourceText;
        }

        //------------------------------------------------------------
        // CSourceText.GetLineCount (ICSSourceText)
        //
        /// <summary>
        /// <para>Let compiler figure out the line count...</para>
        /// <para>Not implemented. Throw an exception.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public int GetLineCount()
        {
            throw new NotImplementedException("CSourceText.GetLineCount");
        }

        //------------------------------------------------------------
        // CSourceText.ReleaseText (ICSSourceText)
        //
        /// <summary>
        /// Clear the text.
        /// </summary>
        //------------------------------------------------------------
        virtual public void ReleaseText()
        {
            sourceText = null;
        }

        //------------------------------------------------------------
        // CSourceText.AdviseChangeEvents (ICSSourceText)
        //
        /// <summary>
        /// <para>Not implemented. Throw an exception.</para>
        /// </summary>
        /// <param name="pSink"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public int AdviseChangeEvents(CSourceModule pSink, out uint cookie)
        {
            cookie = 0;
            throw new NotImplementedException("CSourceText.AdviseChangeEvents");
        }

        //------------------------------------------------------------
        // CSourceText.UnadviseChangeEvents (ICSSourceText)
        //
        /// <summary>
        /// <para>Not implemented. Throw an exception.</para>
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public int UnadviseChangeEvents(uint cookie)
        {
            throw new NotImplementedException("CSourceText.UnadviseChangeEvents");
        }

        //------------------------------------------------------------
        // CSourceText.GenerateChecksum (ICSSourceText)
        //
        /// <summary>
        /// <para>Not implemented. Throw an exception.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public CHECKSUM GenerateChecksum()
        {
            throw new NotImplementedException("CSourceText.GenerateChecksum");
        }

        //------------------------------------------------------------
        // CSourceText.ReportExceptionalOperation   (ICSSourceText)
        //
        /// <summary>
        /// Does nothing.
        /// </summary>
        //------------------------------------------------------------
        virtual public void ReportExceptionalOperation() { }

        //------------------------------------------------------------
        // CSourceText.OutputToReproFile
        //------------------------------------------------------------
        void OutputToReproFile() { }

        //------------------------------------------------------------
        // CSourceText  indexer
        //
        /// <summary>
        /// (R) indexer. Return the character at index of type int.
        /// if have no text or if the index is invalid, return '\0'.
        /// </summary>
        //------------------------------------------------------------
        public char this[int index]
        {
            get
            {
                if (sourceText != null)
                {
                    try
                    {
                        return sourceText[index];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return '\0';
                    }
                }
                return '\0';
            }
        }

        //------------------------------------------------------------
        // CSourceText.InitCharIndex
        //
        /// <summary>
        /// Set the current index to 0 (the first character). if have no text, return false.
        /// </summary>
        //------------------------------------------------------------
        public bool InitCharIndex()
        {
            if (String.IsNullOrEmpty(sourceText))
            {
                return false;
            }
            this.charIndex = 0;
            return true;
        }

        //------------------------------------------------------------
        // CSourceText.NextChar
        //
        /// <summary>
        /// Advance the current index to the next character. if the current index is not set, set it to 0.
        /// </summary>
        //------------------------------------------------------------
        public void NextChar()
        {
            if (String.IsNullOrEmpty(sourceText))
            {
                if (charIndex < 0)
                {
                    charIndex = 0;
                }
                else if (charIndex < sourceText.Length)
                {
                    ++charIndex;
                }
                else
                {
                    charIndex = sourceText.Length;
                }
            }
        }

        //------------------------------------------------------------
        // CSourceText operator ++
        //
        /// <summary>
        /// Advance the current index to the next character. if the current index is not set, set it to 0.
        /// </summary>
        //------------------------------------------------------------
        static public CSourceText operator ++(CSourceText opd)
        {
            if (opd != null)
            {
                opd.NextChar();
            }
            return opd;
        }

        //------------------------------------------------------------
        // CSourceText.PreviousChar
        //
        /// <summary>
        /// Retreat the current index to the previous character.
        /// if the current index is not set, set it to the last character.
        /// </summary>
        //------------------------------------------------------------
        public void PreviousChar()
        {
            if (String.IsNullOrEmpty(sourceText))
            {
                if (charIndex < 0)
                {
                    charIndex = -1;
                }
                else if (charIndex < sourceText.Length)
                {
                    --charIndex;
                }
                else
                {
                    charIndex = sourceText.Length - 1;
                }
            }
        }

        //------------------------------------------------------------
        // CSourceText operator --
        //
        /// <summary>
        /// Retreat the current index to the previous character.
        /// if the current index is not set, set it to the last character.
        /// </summary>
        //------------------------------------------------------------
        static public CSourceText operator --(CSourceText opd)
        {
            if (opd != null)
            {
                opd.PreviousChar();
            }
            return opd;
        }

        //------------------------------------------------------------
        // CSourceText.End
        //
        /// <summary>
        /// Return true if the current index is greater than or equal to the length of text.
        /// if have no text, also return true.
        /// </summary>
        //------------------------------------------------------------
        public bool End()
        {
            if (sourceText != null)
            {
                return (charIndex >= sourceText.Length);
            }
            return true;
        }

        //------------------------------------------------------------
        // CSourceText.Char (property)
        //
        /// <summary>
        /// (R) if the current index is valid, return the character at the current index.
        /// Otherwise, return '\0'.
        /// </summary>
        //------------------------------------------------------------
        public char Char
        {
            get { return this[charIndex]; }
        }
    }
}
