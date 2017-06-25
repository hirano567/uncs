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
// ===========================================================================
// File: lexer.h
//
// ===========================================================================

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
// File: lexer.cpp
//
// ===========================================================================

//============================================================================
// Lexer.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Uncs
{
    //======================================================================
    // class CLexer
    //
    /// <summary>
    /// <para>This is the base lexer class. It's abstract --
    /// you must create a derivation and implement the pure virtuals.</para>
    /// </summary>
    //======================================================================
    abstract internal class CLexer
    {
        //------------------------------------------------------------
        // CLexer Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// Name manager.
        /// </summary>
        protected CNameManager nameManager = null;

        /// <summary>
        /// (R) NameManager.
        /// </summary>
        internal CNameManager NameManager
        {
            get { return nameManager; }
        }

        /// <summary>
        /// Source text and the position to read.
        /// </summary>
        internal CTextReader TextReader = null;

        /// <summary>
        /// The index of the first character of the current line.
        /// </summary>
        internal int CurrentLineStartIndex = 0;

        /// <summary>
        /// Index of current line
        /// </summary>
        internal int CurrentLineIndex = 0;

        /// <summary>
        /// Which keywords this lexer recognizes
        /// </summary>
        internal LangVersionEnum LangVersion = LangVersionEnum.Default;

        /// <summary>
        /// true if token had escapes
        /// </summary>
        internal bool IsEscapedToken = false;

        /// <summary>
        /// true if no tokens have been scanned from this line yet
        /// </summary>
        internal bool NotScannedLine = false;

        /// <summary>
        /// true if preprocessor tokens are valid
        /// </summary>
        internal bool IsValidPreprocessorToken = false;

        /// <summary>
        /// true if tokens have been seen (and thus #define/#undef are invalid)
        /// </summary>
        internal bool TokensHaveBeenScanned = false;

        /// <summary>
        /// true if too many lines or line too long
        /// </summary>
        internal bool IsLineLimitExceeded = false;

        /// <summary>
        /// true if this line is too long and an error has already been given
        /// </summary>
        internal bool IsTooLongLine = false;

        internal bool Result = true;

        //------------------------------------------------------------
        // CLexer   Constructor
        //
        /// <summary></summary>
        /// <param name="mgr"></param>
        /// <param name="mode"></param>
        //------------------------------------------------------------
        internal CLexer(CNameManager nameMgr, LangVersionEnum langVer)
        {
            this.nameManager = nameMgr;
            this.LangVersion = langVer;
        }

        //------------------------------------------------------------
        // CLexer.NextLine
        //
        /// <summary>
        /// Increment CurrentLineIndex.
        /// </summary>
        //------------------------------------------------------------
        internal void NextLine()
        {
            ++CurrentLineIndex;
        }

        //------------------------------------------------------------
        // CLexer.PositionOf
        //
        /// <summary>
        /// If argument charIndex is valid and is after the current line,
        /// Set the position to posData
        /// </summary>
        /// <param name="charIndex"></param>
        /// <param name="posData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool PositionOf(int charIndex, POSDATA posData)
        {
            if (charIndex < CurrentLineStartIndex ||
                charIndex - CurrentLineStartIndex >= POSDATA.MAX_POS_LINE_LEN)
            {
                return false;
            }
            posData.LineIndex = CurrentLineIndex;
            posData.CharIndex = charIndex - CurrentLineStartIndex;
            return true;
        }

        //------------------------------------------------------------
        // CLexer.ScanEscapeSequence
        //
        /// <summary>
        /// Convert a escape sequence to a character.
        /// </summary>
        /// <param name="textReader"></param>
        /// <param name="surrogate">A low surrogate is set to this if need.</param>
        /// <returns>a unicode character or a high surrogate.</returns>
        //------------------------------------------------------------
        internal char ScanEscapeSequence(CTextReader textReader, out char surrogate)
        {
            surrogate = (char)0;
            CTextReader oldReader = textReader.Clone();
            char ch = textReader.Char;
            textReader.Next();

            switch (ch)
            {
                case '\'':
                case '\"':
                case '\\':
                    break;

                case 'a':
                    ch = '\a';
                    break;

                case 'b':
                    ch = '\b';
                    break;

                case 'f':
                    ch = '\f';
                    break;

                case 'n':
                    ch = '\n';
                    break;

                case 'r':
                    ch = '\r';
                    break;

                case 't':
                    ch = '\t';
                    break;

                case 'v':
                    ch = '\v';
                    break;

                case 'x':
                case 'u':
                case 'U':
                    // Backup so ScanUnicodeEscape can re-read the character
                    textReader.Back();
                    ch = ScanUnicodeEscape(textReader, out surrogate, false);
                    break;

                case '0':
                    ch = '\0';
                    break;

                default:
                    ErrorAtPosition(
                        CurrentLineIndex,
                        oldReader.Index - CurrentLineStartIndex,
                        textReader.Index - oldReader.Index,
                        CSCERRID.ERR_IllegalEscape);
                    break;
            }
            return ch;
        }

        //------------------------------------------------------------
        // CLexer.TrackLine
        //
        /// <summary>
        /// Overridables.
        /// Derived classes must call CLexer::TrackLine so that we can reset IsTooLongLine.
        /// Otherwise we can't properly report exactly 1 error message when a line is too long
        /// </summary>
        /// <param name="newLineIndex"></param>
        //------------------------------------------------------------
        virtual internal void TrackLine(int newLineIndex)
        {
            IsTooLongLine = false;
        }

        //------------------------------------------------------------
        // CLexer.ScanPreprocessorLine
        //
        /// <summary>
        /// return false.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// (sscli) BOOL ScanPreprocessorLine (PCWSTR &p) { return FALSE; }
        /// </remarks>
        //------------------------------------------------------------
        virtual internal bool ScanPreprocessorLine()
        {
            IsValidPreprocessorToken = false;
            return false;
        }

        //------------------------------------------------------------
        // CLexer.RepresentNoiseTokens (abstract)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        abstract internal bool RepresentNoiseTokens();

        //------------------------------------------------------------
        // CLexer.ErrorAtPosition (1)
        //
        /// <summary></summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="extent"></param>
        /// <param name="errorId"></param>
        //------------------------------------------------------------
        internal void ErrorAtPosition(int line, int col, int extent, CSCERRID errorId)
        {
            ErrorPosArgs(line, col, extent, errorId);
        }

        //------------------------------------------------------------
        // CLexer.ErrorAtPosition (2)
        //
        /// <summary></summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="extent"></param>
        /// <param name="errorId"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        internal void ErrorAtPosition(
            int line,
            int col,
            int extent,
            CSCERRID errorId,
            ErrArg arg)
        {
            ErrorPosArgs(line, col, extent, errorId, arg);
        }

        //protected:
        //    friend class CSourceModule;

        //------------------------------------------------------------
        // CLexer.ScanUnicodeEscape
        //
        // hexadecimal-escape-sequence:
        //     "\x"  hex-digit  [hex-digit]  [hex-digit]  [hex-digit]
        // unicode-escape-sequence:
        //     "\u"  hex-digit  hex-digit  hex-digit  hex-digit
        //     "\U"  hex-digit  hex-digit  hex-digit  hex-digit  hex-digit  hex-digit  hex-digit  hex-digit
        //
        /// <summary>
        /// <para>Decode an Unicode character escape sequence to an Unicode character of type char.</para>
        /// <para>Assume that the character at text.Index is 'x' or 'U' or 'u' following '\'.</para>
        /// <para>If no hex digit or invalid code, return 'x' or 'U' or 'u' and increment index.</para>
        /// <para>Even if hex digits are not enough, decode them.</para>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="surrogate">a low surrogate is set to if necessary.</param>
        /// <param name="peek">If false, show a error message.</param>
        /// <returns>return a Unicode character or hight surrogate.
        /// If invalid sequence, return U+0000 and restore text.Index.</returns>
        /// <remarks>
        /// If succeeded, text.Index is of the next character following the escape sequence.
        /// </remarks>
        //------------------------------------------------------------
        internal char ScanUnicodeEscape(CTextReader text, out char surrogate, bool peek)
        {
            surrogate = '\0';

            //PCWSTR  pszStart = p - 1;   // Back-up to the '\'
            //ASSERT(*pszStart == '\\');
            int startIndex = text.Index;

            //------------------------------------------------------------
            // Get a current character and proceed to the next character.
            //------------------------------------------------------------
            char ch = text.Char;
            text.Next();

            //------------------------------------------------------------
            // If "\U", eight hex digits follow.
            //------------------------------------------------------------
            if (ch == 'U')
            {
                uint uChar = 0;

                if (!CharUtil.IsHexDigit(text.Char))
                {
                    if (!peek)
                    {
                        ErrorAtPosition(
                            CurrentLineIndex,
                            startIndex - CurrentLineStartIndex,
                            text.Index - startIndex,
                            CSCERRID.ERR_IllegalEscape);
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (!CharUtil.IsHexDigit(text.Char))
                        {
                            if (!peek)
                            {
                                ErrorAtPosition(
                                    CurrentLineIndex,
                                    startIndex - CurrentLineStartIndex,
                                    text.Index - startIndex,
                                    CSCERRID.ERR_IllegalEscape);
                            }
                            break;
                        }
                        uChar = (uChar << 4) + (uint)CharUtil.HexValue(text.Char);
                        text.Next();
                    }

                    // If in Basic Multilingual Plane,
                    // convert it to type of char and return it.
                    if (uChar < 0x00010000)
                    {
                        ch = (char)uChar;
                        surrogate = '\0';
                    }
                    // Invalid code.
                    else if (uChar > 0x0010FFFF)
                    {
                        if (!peek)
                        {
                            ErrorAtPosition(
                                    CurrentLineIndex,
                                   startIndex - CurrentLineStartIndex,
                                   text.Index - startIndex,
                                   CSCERRID.ERR_IllegalEscape);
                        }
                    }
                    // If in Supplementary Multilingual Plane,
                    // return high surrogate and set low surrogate to argument surrogate.
                    else
                    {
                        //ASSERT(uChar > 0x0000FFFF && uChar <= 0x0010FFFF);
                        ch = (char)((uChar - 0x00010000) / 0x0400 + 0xD800);
                        surrogate = (char)((uChar - 0x00010000) % 0x0400 + 0xDC00);
                    }
                }
            }
            //------------------------------------------------------------
            // If "\x", four hex digits follow at most.
            // If "\u", four hex digits follow.
            //------------------------------------------------------------
            else
            {
                DebugUtil.Assert(ch == 'u' || ch == 'x');
                int iChar = 0;

                if (!CharUtil.IsHexDigit(text.Char))
                {
                    if (!peek)
                    {
                        ErrorAtPosition(
                            CurrentLineIndex,
                            startIndex - CurrentLineStartIndex,
                            text.Index - startIndex,
                            CSCERRID.ERR_IllegalEscape);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (!CharUtil.IsHexDigit(text.Char))
                        {
                            if (ch == 'u' && !peek)
                            {
                                ErrorAtPosition(
                                    CurrentLineIndex,
                                    startIndex - CurrentLineStartIndex,
                                    text.Index - startIndex,
                                    CSCERRID.ERR_IllegalEscape);
                            }
                            break;
                        }
                        iChar = (iChar << 4) + CharUtil.HexValue(text.Char);
                        text.Next();
                    }
                    ch = (char)iChar;
                }
            }
            return ch;
        }

        //------------------------------------------------------------
        // CLexer.PeekUnicodeEscape
        //
        /// <summary>
        /// <para>Decode an Unicode character escape sequence as ScanUnicodeEscape method.</para>
        /// <para>The difference is that this method does not advance index.</para>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="surrogate"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char PeekUnicodeEscape(CTextReader text, out char surrogate)
        {
            // if we're peeking, then we don't want to change the position
            CTextReader temp = text.Clone();
            temp.Next();
            return ScanUnicodeEscape(temp, out surrogate, true);
        }

        //------------------------------------------------------------
        // CLexer.PeekChar
        //
        /// <summary>
        /// <para>Return a current character and leave the current character index.</para>
        /// <para>Unicode character escape sequence is decoded.</para>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="surrogate"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char PeekChar(CTextReader text, out char surrogate)
        {
            char ch = text.Char;
            char ch1;
            surrogate = '\0';
            if (ch == '\\' && ((ch1 = text.NextChar()) == 'U' || ch1 == 'u'))
            {
                return PeekUnicodeEscape(text, out surrogate);
            }
            else
            {
                return ch;
            }
        }

        //------------------------------------------------------------
        // CLexer.NextChar
        //
        /// <summary>
        /// <para>Return a current character or a high surrogate
        /// and advance the character index to the next character.</para>
        /// <para>Unicode character escape sequence is decoded.</para>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="surrogate">Low surrogate will be set if necessary.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char NextChar(CTextReader text, out char surrogate)
        {
            surrogate = '\0';
            char ch = text.Char;
            char ch1;
            text.Next();

            if (ch == '\\' && ((ch1 = text.Char) == 'U' || ch1 == 'u'))
            {
                return ScanUnicodeEscape(text, out surrogate, false);
            }
            else
            {
                return ch;
            }
        }

        //------------------------------------------------------------
        // CLexer.CreateInvalidToken
        //
        /// <summary>
        /// <para>Set the specified text range to the name of token and
        /// set TOKENID.UNKNOWN to token.</para>
        /// </summary>
        /// <param name="token"></param>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        //------------------------------------------------------------
        internal void CreateInvalidToken(CSTOKEN token, string text, int start, int end)
        {
            DebugUtil.Assert(token != null && text != null);
            string name;

            try
            {
                name = text.Substring(start, end - start);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            nameManager.AddString(name);
            token.TokenID = TOKENID.UNKNOWN;
            token.Name = name;
        }

        //------------------------------------------------------------
        // CLexer.ReportInvalidToken
        //
        /// <summary>
        /// Report that a invalid token has been found.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        //------------------------------------------------------------
        internal void ReportInvalidToken(CSTOKEN token, string text, int start, int end)
        {
            CreateInvalidToken(token, text, start, end);
            ErrorAtPosition(
                CurrentLineIndex,
                start - CurrentLineStartIndex,
                end - start,
               CSCERRID.ERR_UnexpectedCharacter,
               new ErrArg(token.Name));
        }

        //------------------------------------------------------------
        // CLexer.ErrorPosArgs (abstract)
        //
        /// <summary></summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="extent"></param>
        /// <param name="id"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        abstract internal void ErrorPosArgs(
            int line,
            int col,
            int extent,
            CSCERRID id,
            params ErrArg[] arg);

        //------------------------------------------------------------
        // CLexer.ParseIdentifier
        //
        /// <summary>
        /// <para>Get an idetifier and set it to a CTOKEN instance.</para>
        /// <para>Argument firstChar must be valid character or high surrogate for an identifier.
        /// (Unicode character escape sequence has already been decoded.)</para>
        /// <para>textReader.Index must be of the second character.</para>
        /// <para>Must confirm that firstChar (and firstLowSurrogate) is invalid identifier character
        /// before call this method. Therefore nextToken should not be set invalid.</para>
        /// </summary>
        /// <param name="firstChar"></param>
        /// <param name="firstLowSurrogate"></param>
        /// <param name="textReader"></param>
        /// <param name="tokenStartIndex"></param>
        /// <param name="nextToken"></param>
        /// <param name="atPrefix">true if with '@' prefix.</param>
        /// <param name="isEscaped"></param>
        /// <returns></returns>
        /// <remarks>
        /// Separated from ScanToken method.
        /// </remarks>
        //------------------------------------------------------------
        private bool ParseIdentifier(
            char firstChar,
            char firstLowSurrogate,
            CTextReader textReader,
            int tokenStartIndex,
            CSTOKEN nextToken,
            bool atPrefix,
            out bool isEscaped)
        {
            StringBuilder sb = new StringBuilder(32);   // experimentally
            char ch1 = '\0';
            char lowSurrogate = '\0';
            bool doubleUnderscore = false;
            isEscaped = false;

            // Remember, because we're processing identifiers here,
            // unicode escape sequences are allowed and must be handled
            sb.Append(firstChar);
            if (firstLowSurrogate != '\0')
            {
                sb.Append(firstLowSurrogate);
            }

            do
            {
                ch1 = PeekChar(textReader, out lowSurrogate);
                switch (ch1)
                {
                    case '_':
                        // Common identifier character,
                        // but we need check for double consecutive underscores
                        if (!doubleUnderscore && (firstChar == '_'))
                        {
                            doubleUnderscore = true;
                        }
                        break;

                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        // Again, these are the 'common' identifier characters...
                        break;

                    case ' ':
                    case '\t':
                    case '.':
                    case ';':
                    case '(':
                    case ')':
                    case ',':
                        // ...and these are the 'common' stop characters.
                        goto ExitLoop;

                    default:
                        // This is the 'expensive' call
                        // BUG 424819 : Handle identifier chars > 0xFFFF via surrogate pairs
                        if (CharUtil.IsIdentifierCharOrDigit(ch1))
                        {
                            // Ignore formatting characters
                            if (Char.GetUnicodeCategory(ch1) == UnicodeCategory.Format)
                            {
                                goto SkipChar;
                            }
                        }
                        else
                        {
                            // Not a valid identifier character, so bail.
                            goto ExitLoop;
                        }
                        break;
                }

                sb.Append(ch1);
                if (firstLowSurrogate != '\0')
                {
                    sb.Append(firstLowSurrogate);
                }

            SkipChar:
                ch1 = NextChar(textReader, out firstLowSurrogate);
            }
            while (ch1 != '\0');

        ExitLoop:
            DebugUtil.Assert(sb.Length > 0);
            string name = sb.ToString();
            int nameLength = sb.Length;

            // "escaped" means there was an @ prefix, or there was a unicode escape -- both of which
            // indicate overhead, since the identifier length will not be equal to the token length
            isEscaped = (atPrefix || (textReader.Index - tokenStartIndex > nameLength));

            if (sb.Length >= ImportUtil.MAX_IDENT_SIZE)
            {
                ErrorAtPosition(
                    CurrentLineIndex,
                    tokenStartIndex - CurrentLineStartIndex,
                    textReader.Index - tokenStartIndex ,
                    CSCERRID.ERR_IdentifierTooLong);
                nameLength = ImportUtil.MAX_IDENT_SIZE - 1;
                //name = name.Substring(0, nameLength);
            }

            int keywordIndex = 0;

            // Add the identifier to the name table
            nameManager.AddString(name);
            nextToken.Name = name;

            // ...and check to see if it is a keyword, if appropriate
            if (isEscaped ||
                !nameManager.IsKeyword(nextToken.Name, LangVersion, out keywordIndex))
            {
                nextToken.TokenID = TOKENID.IDENTIFIER;

                if (doubleUnderscore &&
                    !atPrefix &&
                    //KeywordMode == CompatibilityMode.ECMA1)
                    LangVersion==LangVersionEnum.ECMA1)
                {
                    ErrorAtPosition(
                        CurrentLineIndex,
                        tokenStartIndex - CurrentLineStartIndex,
                        textReader.Index - tokenStartIndex,
                        CSCERRID.ERR_ReservedIdentifier,
                        new ErrArg(nextToken.Name));
                }

                if (isEscaped)
                {
                    // Hold this so assignment to pEscName doesn't whack it
                    name = nextToken.Name;

                    nextToken.TokenFlags |= TOKFLAGS.OVERHEAD;
                    nextToken.EscapedName =
                        new ESCAPEDNAME(nextToken.Name, textReader.Index - tokenStartIndex);
                }
            }
            else
            {
                nextToken.UserByte = keywordIndex;
                nextToken.Length = nameLength;
            }

            if (atPrefix)
            {
                nextToken.TokenFlags |= TOKFLAGS.VERBATIMSTRING;
                // We need to know this later
            }
            return true;
        }

        //------------------------------------------------------------
        // CLexer.ParseNumber
        //
        // integer-literal::
        // 	decimal-integer-literal
        // 	hexadecimal-integer-literal
        //
        // decimal-integer-literal::
        // 	decimal-digits [integer-type-suffix]
        //
        // decimal-digits::
        // 	decimal-digit
        // 	decimal-digits decimal-digit
        //
        // decimal-digit:: one of
        // "0" "1" "2" "3" "4" "5" "6" "7" "8" "9"
        //
        // integer-type-suffix:: one of
        // 	"U" "u" "L" "l" "UL" "Ul" "uL" "ul" "LU" "Lu" "lU" "lu"
        //
        // hexadecimal-integer-literal::
        // 	"0x" hex-digits [integer-type-suffix]
        // 	"0X" hex-digits [integer-type-suffix]
        //
        // hex-digits::
        // 	hex-digit
        // 	hex-digits hex-digit
        //
        // hex-digit:: one of
        // 	"0" "1" "2" "3" "4" "5" "6" "7" "8" "9" "A" "B" "C" "D" "E" "F" "a" "b" "c" "d" "e" "f"
        //
        // real-literal::
        // 	decimal-digits "." decimal-digits [exponent-part] [real-type-suffix]
        // 	"." decimal-digits [exponent-part] [real-type-suffix]
        // 	decimal-digits exponent-part [real-type-suffix]
        // 	decimal-digits real-type-suffix
        //
        // exponent-part::
        // 	"e" [sign] decimal-digits
        // 	"E" [sign] decimal-digits
        //
        // sign:: one of
        // 	"+" "-"
        //
        // real-type-suffix:: one of
        // 	"F" "f" "D" "d" "M" "m"
        //
        /// <summary>
        /// <para>Get a numerical literal and set it to CTOKEN instance.</para>
        /// <para>Not convert to a number.</para>
        /// </summary>
        /// <param name="textReader"></param>
        /// <param name="ch"></param>
        /// <param name="tokenStartIndex"></param>
        /// <param name="decimalPoint"></param>
        /// <param name="nextToken"></param>
        /// <param name="isReal"></param>
        /// <param name="hexNumber"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseNumber(
            CTextReader textReader,
            char ch,
            int tokenStartIndex,
            bool decimalPoint,  // If true, the current character is decimal point.
            CSTOKEN nextToken,
            out bool isReal,
            out bool hexNumber)
        {
            isReal = false;
            hexNumber = (
                !decimalPoint &&
                (ch == '0' && (textReader.Char == 'x' || textReader.Char == 'X')));

            int indexHolder = -1;
            char currentChar;

            //--------------------------------------------------
            // Hex number
            //--------------------------------------------------
            if (hexNumber)
            {
                // it's a hex constant
                textReader.Next();

                // It's OK if it has no digits after the '0x'
                // -- we'll catch it in ScanNumericLiteral and give a proper error then.
                while (CharUtil.IsHexDigit(textReader.Char))
                {
                    textReader.Next();
                }
                currentChar = textReader.Char;

                if (currentChar == 'L' || currentChar == 'l')
                {
                    textReader.Next();
                    if (textReader.Char == 'u' || textReader.Char == 'U')
                    {
                        textReader.Next();
                    }
                }
                else if (currentChar == 'u' || currentChar == 'U')
                {
                    textReader.Next();
                    if (textReader.Char == 'L' || textReader.Char == 'l')
                    {
                        textReader.Next();
                    }
                }
            }
            //--------------------------------------------------
            // Decimal number
            //--------------------------------------------------
            else
            {
                // skip digits
                //while (textReader.Char >= '0' && textReader.Char <= '9')
                while ((currentChar = textReader.Char) >= '0' && currentChar <= '9')
                {
                    textReader.Next();
                }

                if (currentChar == '.')
                {
                    //holdItr = textReader.Clone();
                    indexHolder = textReader.Index;
                    textReader.Next();

                    if ((currentChar = textReader.Char) >= '0' && currentChar <= '9')
                    {
                        // skip digits after decimal point
                        textReader.Next();
                        isReal = true;
                        while ((currentChar = textReader.Char) >= '0' && currentChar <= '9')
                        {
                            textReader.Next();
                        }
                    }
                    else
                    {
                        // Number + dot + non-digit -- these are separate tokens, so don't absorb the
                        // dot token into the number.
                        //textReader.Assign(holdItr);
                        textReader.Index = indexHolder;
                        goto SET_TOKEN;
                    }
                }

                //if (textReader.Char == 'E' || textReader.Char == 'e')
                if (currentChar == 'E' || currentChar == 'e')
                {
                    isReal = true;

                    // skip exponent
                    textReader.Next();
                    if ((currentChar = textReader.Char) == '+' || currentChar == '-')
                    {
                        textReader.Next();
                    }

                    while ((currentChar = textReader.Char) >= '0' && currentChar <= '9')
                    {
                        textReader.Next();
                    }
                }

                //currentChar = textReader.Char;
                if (isReal)
                {
                    if (currentChar == 'f' ||
                        currentChar == 'F' ||
                        currentChar == 'D' ||
                        currentChar == 'd' ||
                        currentChar == 'm' ||
                        currentChar == 'M')
                    {
                        textReader.Next();
                    }
                }
                else if (
                    currentChar == 'F' ||
                    currentChar == 'f' ||
                    currentChar == 'D' ||
                    currentChar == 'd' ||
                    currentChar == 'm' ||
                    currentChar == 'M')
                {
                    textReader.Next();
                }
                else if (currentChar == 'L' || currentChar == 'l')
                {
                    textReader.Next();
                    if (textReader.Char == 'u' || textReader.Char == 'U')
                    {
                        textReader.Next();
                    }
                }
                //else if (textReader.Char == 'u' || textReader.Char == 'U')
                else if (currentChar == 'u' || currentChar == 'U')
                {
                    textReader.Next();
                    if (textReader.Char == 'L' || textReader.Char == 'l')
                    {
                        textReader.Next();
                    }
                }
            }

            //--------------------------------------------------
            // Set to nextToken
            //--------------------------------------------------
        SET_TOKEN:
            nextToken.TokenID = TOKENID.NUMBER;
            nextToken.TokenFlags |= TOKFLAGS.OVERHEAD;
            if (hexNumber)
            {
                nextToken.TokenFlags |= TOKFLAGS.HEXLITERAL;
            }
            int length = textReader.Index - tokenStartIndex;
            nextToken.Literal = new LITERAL(
                textReader.Text.Substring(tokenStartIndex, length),
                length);
            return true;
        }

        //------------------------------------------------------------
        // CLexer.CheckIdentifier
        //
        /// <summary>
        /// <para>Check if a character is valid for an identifier.</para>
        /// </summary>
        /// <remarks>
        /// Separate from ScanToken method.
        /// </remarks>
        //------------------------------------------------------------
        private bool CheckIdentifier(
            char ch,
            CTextReader textReader,
            int tokenStartIndex,
            CSTOKEN nextToken)
        {
            // BUG 424819 : Handle identifier chars > 0xFFFF via surrogate pairs
            if (!CharUtil.IsIdentifierChar(ch))
            {
                ReportInvalidToken(
                    nextToken,
                    textReader.Text,
                    tokenStartIndex,
                    textReader.Index);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // CLexer.ScanToken
        //
        /// <summary>
        /// <para>This function scans the next token present in the current input stream,
        /// and puts the result in the given CSTOKEN.</para>
        /// <para>Return a TOKENID value which tells a kind of token.</para>
        /// </summary>
        /// <param name="nextToken"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TOKENID ScanToken(CSTOKEN nextToken)
        {
            char currentChar, quoteChar, surrogateChar = '\0';
            int startIndex = TextReader.Index;
            int tokenStartIndex = -1;
            bool isReal = false;
            bool isEscaped = false;
            bool hexNumber = false;
            bool br = true;
            char tempChar;

            // Initialize for new token scan
            nextToken.CharIndex = 0;
            nextToken.LineIndex = 0;
            nextToken.UserByte = (int)TOKENID.INVALID;
            nextToken.UserBits = 0;

            //--------------------------------------------------
            // Start scanning the token
            //--------------------------------------------------
            while (nextToken.TokenID == TOKENID.INVALID)
            {
                // Set a current position to nextToken.
                if (!PositionOf(TextReader.Index, nextToken) && !IsTooLongLine)
                {
                    ErrorAtPosition(
                        CurrentLineIndex,
                        POSDATA.MAX_POS_LINE_LEN - 1,
                        1,
                        CSCERRID.ERR_LineTooLong,
                        new ErrArg(POSDATA.MAX_POS_LINE_LEN));

                    IsLineLimitExceeded = true;
                    IsTooLongLine = true;
                }

                //tokenReader.Assign(TextReader);
                tokenStartIndex = TextReader.Index;
                currentChar = TextReader.Char;

                // Advance at least one character each loop.
                // Call TextReader.Back() when currentChar is one of the following characters.
                //  \0  : Set TOKENID.ENDFILE to TokenID and exit loop.
                //  #   : Advance to the next line or If failed, advance one character.
                //  . (digit point) : Advance to the character next to a numerical literal in ParseNumber.
                //  \   : Call NextChar to decode a Unicode character escape sequence.
                TextReader.Next();

                switch (currentChar)
                {
                    //------------------------------------------------
                    // 01 null
                    //
                    // Back up to point to the 0 again...
                    //------------------------------------------------
                    case (char)0:
                        TextReader.Back();
                        nextToken.TokenID = TOKENID.ENDFILE;
                        nextToken.Length = 0;
                        break;

                    //------------------------------------------------
                    // 02 space, tab
                    //
                    // Tabs and spaces tend to roam in groups... scan them together
                    //------------------------------------------------
                    case '\t':
                    case ' ':
                        while ((tempChar = TextReader.Char) == ' ' || tempChar == '\t')
                        {
                            TextReader.Next();
                        }
                        break;

                    //------------------------------------------------
                    // 03 new line
                    //------------------------------------------------
                    case UCH.PS:    // \u2029
                    case UCH.LS:    // \u2028
                    case (char)0x0085:
                    case '\n':
                        // This is a new line
                        TrackLine(TextReader.Index);
                        break;

                    //------------------------------------------------
                    // 04 carriage return
                    //
                    // Bare CR's are lines,
                    // but CRLF pairs are considered a single line.
                    //------------------------------------------------
                    case '\r':
                        if (TextReader.Char == '\n')
                        {
                            TextReader.Next();
                        }
                        TrackLine(TextReader.Index);
                        break;

                    //------------------------------------------------
                    // 05 other space characters
                    //------------------------------------------------
                    case UCH.BOM:       // Unicode Byte-order marker (\uFEFF)
                    case (char)0x001A:  // Ctrl+Z
                    case '\v':          // Vertical Tab
                    case '\f':          // Form-feed
                        break;

                    //------------------------------------------------
                    // 06 #
                    //------------------------------------------------
                    case '#':
                        TextReader.Back();
                        if (!ScanPreprocessorLine())
                        {
                            DebugUtil.Assert(!IsValidPreprocessorToken);
                            TextReader.Next();
                            ReportInvalidToken(
                                nextToken,
                                TextReader.Text,
                                TextReader.Index,
                                tokenStartIndex);
                        }
                        break;

                    //------------------------------
                    // 07 quote
                    //
                    // "Normal" strings (double-quoted and single-quoted (char) literals).
                    // We translate escape sequences here,
                    // and construct the STRCONST (for strings) directly
                    // (char literals are encoded w/o overhead)
                    //------------------------------
                    case '\"':
                    case '\'':
                        System.Text.StringBuilder sb = new StringBuilder(32); // experimentally

                        quoteChar = currentChar;
                        char ch1, ch2;

                        //--------------------------------------------
                        // 0700 Find the next quote.
                        //--------------------------------------------
                        while (TextReader.Char != quoteChar)
                        {
                            ch1 = TextReader.Char;
                            TextReader.Next();

                            if (ch1 == '\\')
                            {
                                ch1 = ScanEscapeSequence(TextReader, out ch2);

                                // We use a string building to construct the string constant's value.
                                // Yes, CStringBuilder is equipped
                                // to deal with embedded nul characters.

                                sb.Append(ch1);
                                if (ch2 != 0)
                                {
                                    sb.Append(ch2);
                                }
                            }
                            else if (CharUtil.IsEndOfLineChar(ch1) || TextReader.End())
                            {
                                DebugUtil.Assert(TextReader.Index > tokenStartIndex);
                                //TextReader.Back();
                                ErrorAtPosition(
                                    CurrentLineIndex,
                                    tokenStartIndex - CurrentLineStartIndex,
                                    (TextReader.Index - 1) - tokenStartIndex,
                                    CSCERRID.ERR_NewlineInConst);

                                nextToken.TokenFlags |= TOKFLAGS.UNTERMINATED;
                                break;
                            }
                            else
                            {
                                // We use a string building to construct the string constant's value.
                                // Yes, CStringBuilder is equipped
                                // to deal with embedded nul characters.

                                sb.Append(ch1);
                            }
                        }

                        // Skip the terminating quote (if present)
                        if ((nextToken.UserBits & (int)TOKFLAGS.UNTERMINATED) == 0)
                        {
                            TextReader.Next();
                        }

                        //--------------------------------------------
                        // 0701 '
                        //
                        // This was a char literal -- no need to allocate overhead...
                        //--------------------------------------------
                        if (quoteChar == '\'')
                        {
                            if (sb.Length != 1)
                            {
                                ErrorAtPosition(
                                    CurrentLineIndex,
                                    tokenStartIndex - CurrentLineStartIndex,
                                    TextReader.Index - tokenStartIndex,
                                    (sb.Length != 0) ?
                                    CSCERRID.ERR_TooManyCharsInConst :
                                    CSCERRID.ERR_EmptyCharConst);
                            }

                            nextToken.TokenID = TOKENID.CHARLIT;
                            nextToken.CharLiteral =
                                new CHARLITERAL(sb[0], TextReader.Index - tokenStartIndex);
                        }
                        //--------------------------------------------
                        // 0702 "
                        //
                        // This one requires special allocation.
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.STRINGLIT;
                            nextToken.TokenFlags |= TOKFLAGS.OVERHEAD;
                            nextToken.StringLiteral =
                                new STRLITERAL(sb.ToString(), TextReader.Index - tokenStartIndex);
                        }
                        break;

                    //------------------------------------------------
                    // 08 /
                    //
                    // Lotsa things start with slash...
                    //------------------------------------------------
                    case '/':
                        switch (TextReader.Char)
                        {
                            //----------------------------------------
                            // 0801 //
                            //
                            // Single-line comments...
                            //----------------------------------------
                            case '/':
                                bool isDocComment = (
                                    TextReader.NextChar(1) == '/' &&
                                    TextReader.NextChar(2) != '/');

                                // Find the end of the line,
                                // and make sure it's not too long (even for non-doc comments...)
                                while (
                                    TextReader.Char != (char)0 &&
                                    !CharUtil.IsEndOfLineChar(TextReader.Char))
                                {
                                    if (TextReader.Index - CurrentLineStartIndex
                                            >= POSDATA.MAX_POS_LINE_LEN &&
                                        !IsTooLongLine)
                                    {
                                        ErrorAtPosition(
                                            CurrentLineIndex,
                                            POSDATA.MAX_POS_LINE - 1,
                                            1,
                                            CSCERRID.ERR_LineTooLong,
                                            new ErrArg(POSDATA.MAX_POS_LINE));
                                        IsLineLimitExceeded = true;
                                        IsTooLongLine = true;
                                    }
                                    TextReader.Next();
                                }

                                // Only put comments in the token stream if asked
                                if (RepresentNoiseTokens())
                                {
                                    if (isDocComment)
                                    {
                                        // Doc comments require, ironically enough,
                                        // overhead in the token stream.
                                        nextToken.TokenID = TOKENID.DOCCOMMENT;
                                        nextToken.TokenFlags |= TOKFLAGS.OVERHEAD;
                                        nextToken.DocLiteral = new DOCLITERAL(
                                            TextReader.Text.Substring(tokenStartIndex, TextReader.Index - tokenStartIndex),
                                            new POSDATA(CurrentLineIndex, TextReader.Index - CurrentLineStartIndex));
                                    }
                                    else
                                    {
                                        // No overhead incurred for single-line non-doc comments,
                                        // but we do need the length.
                                        nextToken.TokenID = TOKENID.SLCOMMENT;
                                        nextToken.Length = TextReader.Index - tokenStartIndex;
                                    }
                                }
                                break;

                            //----------------------------------------
                            // 0802 /*
                            // 
                            // Multi-line comments... (Delimited comment)
                            //----------------------------------------
                            case '*':
                                isDocComment = (
                                   TextReader.NextChar(1) == '*' &&
                                   TextReader.NextChar(2) != '*');
                                bool fDone = false;

                                TextReader.Next();
                                while (!fDone)
                                {
                                    if (TextReader.Char == (char)0)
                                    {
                                        // The comment didn't end.
                                        // Report an error at the start point.
                                        ErrorAtPosition(
                                            nextToken.LineIndex,
                                            nextToken.CharIndex,
                                            2,
                                            CSCERRID.ERR_OpenEndedComment);

                                        if (RepresentNoiseTokens())
                                        {
                                            nextToken.TokenFlags |= TOKFLAGS.UNTERMINATED;
                                        }
                                        fDone = true;
                                        break;
                                    }

                                    if (TextReader.Char == '*' &&
                                        TextReader.NextChar(1) == '/')
                                    {
                                        TextReader.Next();
                                        TextReader.Next();
                                        break;
                                    }

                                    if (CharUtil.IsEndOfLineChar(TextReader.Char))
                                    {
                                        if (TextReader.Char == '\r' &&
                                            TextReader.NextChar(1) == '\n')
                                        {
                                            TextReader.Next();
                                        }
                                        TextReader.Next();
                                        TrackLine(TextReader.Index);
                                    }
                                    else
                                    {
                                        TextReader.Next();
                                    }
                                }

                                NotScannedLine = false;

                                if (RepresentNoiseTokens())
                                {
                                    nextToken.TokenFlags |= TOKFLAGS.OVERHEAD;
                                    if (isDocComment)
                                    {
                                        // Doc comments require, ironically enough,
                                        // overhead in the token stream.

                                        nextToken.TokenID = TOKENID.MLDOCCOMMENT;
                                        nextToken.DocLiteral = new DOCLITERAL(
                                            TextReader.Text.Substring(tokenStartIndex, TextReader.Index - tokenStartIndex),
                                            new POSDATA(CurrentLineIndex, TextReader.Index - CurrentLineStartIndex));

                                        if (TextReader.Index - CurrentLineStartIndex
                                                >= POSDATA.MAX_POS_LINE_LEN &&
                                            !IsTooLongLine)
                                        {
                                            ErrorAtPosition(
                                                CurrentLineIndex,
                                                POSDATA.MAX_POS_LINE_LEN - 1,
                                                1,
                                                CSCERRID.ERR_LineTooLong,
                                                new ErrArg(POSDATA.MAX_POS_LINE_LEN));
                                            IsLineLimitExceeded = true;
                                            IsTooLongLine = true;
                                        }
                                    }
                                    else
                                    {
                                        // For multi-line comments,
                                        // we don't put the text in but we do need the
                                        // end position
                                        // -- which means ML comments incur overhead...  :-(
                                        nextToken.TokenID = TOKENID.MLCOMMENT;
                                        nextToken.PosEnd = new POSDATA();
                                        if (!PositionOf(TextReader.Index, nextToken.PosEnd) &&
                                            !IsTooLongLine)
                                        {
                                            ErrorAtPosition(
                                                CurrentLineIndex,
                                                POSDATA.MAX_POS_LINE_LEN - 1,
                                                1,
                                                CSCERRID.ERR_LineTooLong,
                                                new ErrArg(POSDATA.MAX_POS_LINE_LEN));
                                            IsLineLimitExceeded = true;
                                            IsTooLongLine = true;
                                        }
                                    }
                                }
                                break;

                            //----------------------------------------
                            // 0803 /= (Compound assignment)
                            //----------------------------------------
                            case '=':
                                TextReader.Next();
                                nextToken.TokenID = TOKENID.SLASHEQUAL;
                                nextToken.Length = 2;
                                break;

                            //----------------------------------------
                            // 0804 / (Division)
                            //----------------------------------------
                            default:
                                nextToken.TokenID = TOKENID.SLASH;
                                nextToken.Length = 1;
                                break;
                        }   // '/' 内の switch
                        break;

                    //------------------------------------------------
                    // 09 .
                    //------------------------------------------------
                    case '.':
                        //--------------------------------------------
                        // 0901 Digit
                        //--------------------------------------------
                        if ((tempChar = TextReader.Char) >= '0' &&
                            tempChar <= '9')
                        {
                            // Back to a decimal point.
                            TextReader.Back();
                            ch1 = (char)0;
                            ParseNumber(
                                TextReader,
                                ch1,
                                tokenStartIndex,
                                true,
                                nextToken,
                                out isReal,
                                out hexNumber);
                            break;
                        }
                        //--------------------------------------------
                        // 0902 Otherwise
                        //--------------------------------------------
                        nextToken.TokenID = TOKENID.DOT;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 10 ,
                    //------------------------------------------------
                    case ',':
                        nextToken.TokenID = TOKENID.COMMA;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 11 :
                    //------------------------------------------------
                    case ':':
                        //--------------------------------------------
                        // 1101 ::
                        //--------------------------------------------
                        if (TextReader.Char == ':')
                        {
                            nextToken.TokenID = TOKENID.COLONCOLON;
                            nextToken.Length = 2;
                            TextReader.Next();
                        }
                        //--------------------------------------------
                        // 1102 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.COLON;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 12 ;
                    //------------------------------------------------
                    case ';':
                        nextToken.TokenID = TOKENID.SEMICOLON;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 13 ~
                    //------------------------------------------------
                    case '~':
                        nextToken.TokenID = TOKENID.TILDE;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 14 !
                    //------------------------------------------------
                    case '!':
                        //--------------------------------------------
                        // 1401 !=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            nextToken.TokenID = TOKENID.NOTEQUAL;
                            nextToken.Length = 2;
                            TextReader.Next();
                        }
                        //--------------------------------------------
                        // 1402 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.BANG;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 15 =
                    //------------------------------------------------
                    case '=':
                        //--------------------------------------------
                        // 1501 ==
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            nextToken.TokenID = TOKENID.EQUALEQUAL;
                            nextToken.Length = 2;
                            TextReader.Next();
                        }
                        //--------------------------------------------
                        // 1502 =>
                        //--------------------------------------------
                        else if (TextReader.Char == '>')
                        {
                            nextToken.TokenID = TOKENID.EQUALGREATER;
                            nextToken.Length = 2;
                            TextReader.Next();
                        }
                        //--------------------------------------------
                        // 1503 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.EQUAL;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 16 *
                    //------------------------------------------------
                    case '*':
                        //--------------------------------------------
                        // 1601 *=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            nextToken.TokenID = TOKENID.SPLATEQUAL;
                            nextToken.Length = 2;
                            TextReader.Next();
                        }
                        //--------------------------------------------
                        // 1602 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.STAR;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 17 (
                    //------------------------------------------------
                    case '(':
                        nextToken.TokenID = TOKENID.OPENPAREN;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 18 )
                    //------------------------------------------------
                    case ')':
                        nextToken.TokenID = TOKENID.CLOSEPAREN;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 19 {
                    //------------------------------------------------
                    case '{':
                        nextToken.TokenID = TOKENID.OPENCURLY;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 20 }
                    //------------------------------------------------
                    case '}':
                        nextToken.TokenID = TOKENID.CLOSECURLY;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 21 [
                    //------------------------------------------------
                    case '[':
                        nextToken.TokenID = TOKENID.OPENSQUARE;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 22 ]
                    //------------------------------------------------
                    case ']':
                        nextToken.TokenID = TOKENID.CLOSESQUARE;
                        nextToken.Length = 1;
                        break;

                    //------------------------------------------------
                    // 23 ?
                    //------------------------------------------------
                    case '?':
                        //--------------------------------------------
                        // 2301 ??
                        //--------------------------------------------
                        if (TextReader.Char == '?')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.QUESTQUEST;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2302 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.QUESTION;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 24 +
                    //------------------------------------------------
                    case '+':
                        //--------------------------------------------
                        // 2401 +=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.PLUSEQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2402 ++
                        //--------------------------------------------
                        else if (TextReader.Char == '+')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.PLUSPLUS;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2403 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.PLUS;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 25 -
                    //------------------------------------------------
                    case '-':
                        //--------------------------------------------
                        // 2501 -=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.MINUSEQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2502 --
                        //--------------------------------------------
                        else if (TextReader.Char == '-')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.MINUSMINUS;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2503 ->
                        //--------------------------------------------
                        else if (TextReader.Char == '>')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.ARROW;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2504 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.MINUS;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 26 %
                    //------------------------------------------------
                    case '%':
                        //--------------------------------------------
                        // 2601 %=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.MODEQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2602 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.PERCENT;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 27 &
                    //------------------------------------------------
                    case '&':
                        //--------------------------------------------
                        // 2701 &=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.ANDEQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2702 &&
                        //--------------------------------------------
                        else if (TextReader.Char == '&')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.LOG_AND;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2703 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.AMPERSAND;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 28 ^
                    //------------------------------------------------
                    case '^':
                        //--------------------------------------------
                        // 2801 ^=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.HATEQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2802 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.HAT;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 29 |
                    //------------------------------------------------
                    case '|':
                        //--------------------------------------------
                        // 2901 |=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.BAREQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2902 ||
                        //--------------------------------------------
                        else if (TextReader.Char == '|')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.LOG_OR;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 2903 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.BAR;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 30 <
                    //------------------------------------------------
                    case '<':
                        //--------------------------------------------
                        // 3001 <=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.LESSEQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 3002 <<
                        //--------------------------------------------
                        else if (TextReader.Char == '<')
                        {
                            TextReader.Next();
                            if (TextReader.Char == '=')
                            {
                                TextReader.Next();
                                nextToken.TokenID = TOKENID.SHIFTLEFTEQ;
                                nextToken.Length = 3;
                            }
                            else
                            {
                                nextToken.TokenID = TOKENID.SHIFTLEFT;
                                nextToken.Length = 2;
                            }
                        }
                        //--------------------------------------------
                        // 3003 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.LESS;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 31 >
                    //------------------------------------------------
                    case '>':
                        //--------------------------------------------
                        // 3101 >=
                        //--------------------------------------------
                        if (TextReader.Char == '=')
                        {
                            TextReader.Next();
                            nextToken.TokenID = TOKENID.GREATEREQUAL;
                            nextToken.Length = 2;
                        }
                        //--------------------------------------------
                        // 3102 Otherwise
                        //--------------------------------------------
                        else
                        {
                            nextToken.TokenID = TOKENID.GREATER;
                            nextToken.Length = 1;
                        }
                        break;

                    //------------------------------------------------
                    // 32 @
                    //------------------------------------------------
                    case '@':
                        {
                            //----------------------------------------
                            // 3201 @"
                            //
                            // Verbatim string literal.
                            // While scanning/accumulating its value into the string builder,
                            // track lines and ignore escape characters
                            // (they don't apply in VSL's) -- watch for double-quotes as well.
                            //----------------------------------------
                            if (TextReader.Char == '"')
                            {
                                sb = new StringBuilder(32);
                                bool fDone = false;
                                char c;

                                TextReader.Next();
                                while (!fDone)
                                {
                                    c = TextReader.Char;
                                    TextReader.Next();
                                    switch (c)
                                    {
                                        case UCH.PS:
                                        case UCH.LS:
                                        case (char)0x0085:
                                        case '\n':
                                            {
                                                TrackLine(TextReader.Index);
                                                break;
                                            }

                                        case '\r':
                                            {
                                                if (TextReader.Char == '\n')
                                                {
                                                    sb.Append(c);
                                                    c = TextReader.Char;
                                                    TextReader.Next();
                                                }
                                                TrackLine(TextReader.Index);
                                                break;
                                            }

                                        case '\"':
                                            {
                                                if (TextReader.Char == '\"')
                                                    TextReader.Next();
                                                // Doubled quote
                                                // -- skip & put the single quote in the string
                                                else
                                                    fDone = true;
                                                break;
                                            }

                                        case (char)0:
                                            {
                                                // Reached the end of the source
                                                // without finding the end-quote.
                                                // Give an error back at the starting point.
                                                ErrorAtPosition(
                                                    nextToken.LineIndex,
                                                    nextToken.CharIndex,
                                                    2,
                                                    CSCERRID.ERR_UnterminatedStringLit);
                                                nextToken.TokenFlags |= TOKFLAGS.UNTERMINATED;
                                                fDone = true;
                                                //TextReader.Back();
                                                break;
                                            }
                                        default:
                                            DebugUtil.Assert(!CharUtil.IsEndOfLineChar(c));
                                            break;
                                    }

                                    if (!fDone)
                                    {
                                        sb.Append(c);
                                    }
                                }

                                nextToken.TokenID = TOKENID.VSLITERAL;
                                nextToken.TokenFlags |= TOKFLAGS.OVERHEAD;
                                POSDATA pos = new POSDATA();
                                PositionOf(TextReader.Index, pos);
                                nextToken.VSLiteral
                                    = new VSLITERAL(sb.ToString(), pos);
                                break;
                            }

                            //----------------------------------------
                            // 3202 Otherwise
                            //
                            // Check for identifiers.
                            // NOTE: unicode escapes are allowed here!
                            //----------------------------------------
                            ch1 = PeekChar(TextReader, out surrogateChar);

                            // BUG 424819 : Handle identifier chars > 0xFFFF via surrogate pairs
                            if (!CharUtil.IsIdentifierChar(ch1))
                            {
                                // After the '@' we have neither an identifier nor and string quote,
                                // so assume it is an identifier.
                                CreateInvalidToken(nextToken, TextReader.Text, tokenStartIndex, TextReader.Index);
                                ErrorAtPosition(
                                    CurrentLineIndex,
                                    tokenStartIndex - CurrentLineStartIndex,
                                    TextReader.Index - tokenStartIndex,
                                   CSCERRID.ERR_ExpectedVerbatimLiteral);
                                break;
                            }

                            ch1 = NextChar(TextReader, out surrogateChar);
                            br = ParseIdentifier(
                                ch1,
                                surrogateChar,
                                TextReader,
                                tokenStartIndex,
                                nextToken,
                                true,   // atPrefix
                                out isEscaped);
                            // (Goto avoids the IsSpaceSeparator() check
                            // and the redundant IsIdentifierChar() check below...)
                        }
                        break;

                    //------------------------------------------------
                    // 33 \
                    //------------------------------------------------
                    case '\\':
                        // Could be unicode escape. Try that.
                        TextReader.Back();
                        ch1 = NextChar(TextReader, out surrogateChar);

                        // If we had a unicode escape, ch is it.
                        // If we didn't, ch is still a backslash. Unicode escape
                        // must start an identifers, so check only for identifiers now.

                        //goto _CheckIdentifier;
                        if (CheckIdentifier(ch1, TextReader, tokenStartIndex, nextToken))
                        {
                            br = ParseIdentifier(
                                ch1,
                                surrogateChar,
                                TextReader,
                                tokenStartIndex,
                                nextToken,
                                false,
                                out isEscaped);
                        }
                        break;

                    //------------------------------------------------
                    // 34 A-Z, a-z, _
                    //------------------------------------------------
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '_':
                        //ch1 = NextChar(TextReader, out surrogateChar);
                        br = ParseIdentifier(
                            currentChar,
                            (char)0,
                            TextReader,
                            tokenStartIndex,
                            nextToken,
                            false,
                            out isEscaped);
                        break;

                    //------------------------------------------------
                    // 35 0-9
                    //------------------------------------------------
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        ParseNumber(
                            TextReader,
                            currentChar,
                            tokenStartIndex,
                            false,
                            nextToken,
                            out isReal,
                            out hexNumber);
                        break;

                    //------------------------------------------------
                    // 36 Otherwise
                    //------------------------------------------------
                    default:
                        if (Char.GetUnicodeCategory(currentChar)==UnicodeCategory.SpaceSeparator)
                        {
                            while (Char.GetUnicodeCategory(currentChar) == UnicodeCategory.SpaceSeparator)
                            {
                                TextReader.Next();
                            }
                            break;
                        }

                        if (!CheckIdentifier(currentChar, TextReader, tokenStartIndex, nextToken))
                        {
                            ReportInvalidToken(nextToken, TextReader.Text, tokenStartIndex, TextReader.Index);
                            break;
                        }
                        TextReader.Back();
                        ch1 = NextChar(TextReader, out surrogateChar);
                        br = ParseIdentifier(
                            ch1,
                            surrogateChar,
                            TextReader,
                            tokenStartIndex,
                            nextToken,
                            false,
                            out isEscaped);
                        break;

                } // switch (currentChar)
            } // while (nextToken.TokenID == TOKENID.INVALID)

            NotScannedLine = false;
            if (!TokensHaveBeenScanned)
            {
                TokensHaveBeenScanned =
                    ((CParser.TokenInfoArray[(int)nextToken.TokenID].Flags & TOKFLAGS.F_NOISE) == 0);
            }
            return nextToken.TokenID;
        }
    }

    //======================================================================
    // CTextLexer
    //
    /// <summary>
    /// <para>This is the implementation of a CLexer that also exposes ICSLexer for external clients.
    /// It duplicates the given input text (and thus isn't intended for large-scale lexing tasks),
    /// creates a line table and token stream, and has rescan capability.</para>
    /// <para>This class has a list of tokens and a list of staring indice of each line.</para>
    /// </summary>
    //======================================================================
    internal class CTextLexer : CLexer
    {
        //------------------------------------------------------------
        // Fields
        //------------------------------------------------------------
        private List<CSTOKEN> tokenList = new List<CSTOKEN>();
        private List<int> lineList = new List<int>();
        private bool tokenized = false;
        private bool representNoise = true;

        //------------------------------------------------------------
        // CTextLexer   Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CTextLexer()
            : base(null, LangVersionEnum.Default)
        {
        }

        //------------------------------------------------------------
        // CTextLexer.CreateInstance
        //
        /// <summary>
        /// Create a CTextLexer instance.
        /// </summary>
        /// <param name="nameMgr"></param>
        /// <param name="langVer"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal CTextLexer CreateInstance(
            CNameManager nameMgr,
            LangVersionEnum langVer)
        {
            CTextLexer tl = new CTextLexer();
            tl.Initialize(nameMgr, langVer);
            return tl;
        }

        //------------------------------------------------------------
        // CTextLexer.Initialize
        //
        /// <summary>
        /// Initialize a CTextLexer instance.
        /// </summary>
        /// <param name="nameMgr"></param>
        /// <param name="langVer">Specify whether according to ECMA standard.</param>
        //------------------------------------------------------------
        internal void Initialize(CNameManager nameMgr, LangVersionEnum langVer)
        {
            this.nameManager = nameMgr;
            this.LangVersion = langVer;
        }

        // ICSLexer
        //------------------------------------------------------------
        // CTextLexer.SetInput
        //
        /// <summary>
        /// Create token sequence from text.
        /// </summary>
        /// <param name="text"></param>
        //------------------------------------------------------------
        virtual internal void SetInput(string text)
        {
            SetInput(text, 0, -1);
        }

        //------------------------------------------------------------
        // CTextLexer.SetInput
        //
        /// <summary>
        /// Create token sequence from text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        //------------------------------------------------------------
        virtual internal void SetInput(string text, int length)
        {
            SetInput(text, 0, length);
        }

        //------------------------------------------------------------
        // CTextLexer.SetInput
        //
        /// <summary>
        /// Create token sequence from text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <remarks>
        /// Catch no exception in this method.
        /// </remarks>
        //------------------------------------------------------------
        virtual internal void SetInput(string text, int start, int length)
        {
            // Release anything we already have
            tokenList.Clear();
            lineList.Clear();
            tokenized = false;
            Result = true;

            // If length is -1, use the null-terminated length
            if (length < 0)
            {
                TextReader = new CTextReader(text, 0);
            }
            else
            {
                TextReader = new CTextReader(text.Substring(start, length), 0);
            }

            // Establish the first entry in the line table
            lineList.Add(0);

            // Start the input stream for tokenization

            TOKENID tokId = TOKENID.INVALID;
            CSTOKEN token = new CSTOKEN();

            // Scan the text
            while (tokId != TOKENID.ENDFILE && this.Result)
            {
                tokenList.Add(token);
                tokId = ScanToken(token);
            }
            // That's it!  If we didn't fail, signal ourselves as being tokenized
            if (this.Result) this.tokenized = true;
        }

        //------------------------------------------------------------
        //  CTextLexer.GetLexResults
        //
        /// <summary>LEXDATA インスタンスに保持している結果を複製する。</summary>
        /// <param name="lexData">複製先となる LEXDATA インスタンス。</param>
        //------------------------------------------------------------
        virtual internal void GetLexResults(LEXDATA lexData)
        {
            // Must be tokenized
            if (!tokenized || lexData == null)
            {
                return;
            }

            // Just refer to the arrays accumulated in SetInput
            lexData.InitTokens(tokenList);
            lexData.InitSource(TextReader.Text);
            lexData.InitLineOffsets(lineList);
        }

        //------------------------------------------------------------
        // CTextLexer.TrackLine
        //
        /// <summary>
        /// <para>Register a new line.</para>
        /// <para>Assume that the current character is the first character of a new line.</para>
        /// </summary>
        /// <param name="newLineIndex"></param>
        //------------------------------------------------------------
        override internal void TrackLine(int newLineIndex)
        {
            // We only update the line table here if !tokenized
            // (which means we're currently doing the initial tokenization of new input).

            if (!tokenized)
            {
                lineList.Add(newLineIndex);
                this.Result = true;
            }

            CurrentLineStartIndex = newLineIndex;
            ++CurrentLineIndex;
            base.TrackLine(newLineIndex);
        }

        //------------------------------------------------------------
        // CTextLexer.ScanPreprocessorLine
        //
        /// <summary>
        /// Return false.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool ScanPreprocessorLine()
        {
            return false;
        }

        //------------------------------------------------------------
        // CTextLexer.RepresentNoiseTokens
        //
        /// <summary>
        /// Return representNoise.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool RepresentNoiseTokens()
        {
            return representNoise;
        }

        //------------------------------------------------------------
        // CTextLexer.ErrorPosArgs
        //
        /// <summary></summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="extent"></param>
        /// <param name="id"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        internal override void ErrorPosArgs(
            int line,
            int col,
            int extent,
            CSCERRID id,
            params ErrArg[] arg)
        {
        }
    }

    //======================================================================
    // CScanLexer
    //
    /// <summary></summary>
    //======================================================================
    internal class CScanLexer : CLexer
    {
        //------------------------------------------------------------
        // CScanLexer Fields and Properties
        //------------------------------------------------------------
        private bool representNoise;

        //------------------------------------------------------------
        // CScanLexer Constructor
        //
        /// <summary></summary>
        /// <param name="nameMgr"></param>
        /// <param name="noise"></param>
        /// <param name="langVer"></param>
        //------------------------------------------------------------
        internal CScanLexer(
            CNameManager nameMgr,
            bool noise,
            LangVersionEnum langVer)
            : base(nameMgr, langVer)
        {
            representNoise = noise;
        }

        //------------------------------------------------------------
        // CScanLexer.SetInput
        //
        /// <summary></summary>
        /// <param name="text"></param>
        //------------------------------------------------------------
        internal void SetInput(string text)
        {
            TextReader = new CTextReader(text, 0);
            CurrentLineStartIndex = 0;
        }

        // CLexer overrides.  Note, nothing happens for ALL of them (except TrackLine)...

        //------------------------------------------------------------
        // CScanLexer.TrackLine
        //
        /// <summary></summary>
        /// <param name="newLineIndex"></param>
        //------------------------------------------------------------
        override internal void TrackLine(int newLineIndex)
        {
            CurrentLineStartIndex = newLineIndex;
            ++CurrentLineIndex;
            base.TrackLine(newLineIndex);
        }

        //------------------------------------------------------------
        // CScanLexer.ScanPreprocessorLine
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool ScanPreprocessorLine()
        {
            return false;
        }

        //------------------------------------------------------------
        // CScanLexer.RepresentNoiseTokens
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool RepresentNoiseTokens()
        {
            return true;
        }

        //------------------------------------------------------------
        // CScanLexer.ErrorPosArgs
        //
        /// <summary></summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="extent"></param>
        /// <param name="id"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        override internal void ErrorPosArgs(
            int line,
            int col,
            int extent,
            CSCERRID id,
            ErrArg[] arg)
        {
        }
    }
}
