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
// utf.h
//-----------------------------------------------------------------
//
// Routines are documented in more detail below the declarations in 
// the "-- DOCUMENTATION --" section.
//
//-----------------------------------------------------------------

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

// utf.cpp

//============================================================================
// CharUtil.cs
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
    // class UCH
    //======================================================================
    static internal class UCH
    {
        internal const char NULL = '\x0000';	// NULL
        internal const char TAB = '\x0009';	    // HORIZONTAL TABULATION
        internal const char LF = '\x000A';	    // LINE FEED
        internal const char CR = '\x000D';	    // CARRIAGE RETURN
        internal const char SPACE = '\x0020';	// SPACE
        internal const char SP = '\x0020';	    // SPACE
        internal const char NBSP = '\x00A0';	// NO-BREAK SPACE
        internal const char IDEOSP = '\x3000';	// IDEOGRAPHIC SPACE
        internal const char LS = '\x2028';	    // LINE SEPARATOR
        internal const char PS = '\x2029';	    // PARAGRAPH SEPARATOR
        internal const char NEL = '\x0085';	    // NEXT LINE
        internal const char ZWNBSP = '\xFEFF';	// ZERO WIDTH NO-BREAK SPACE (byte order mark)
        internal const char BOM = '\xFEFF';	    // ZERO WIDTH NO-BREAK SPACE (byte order mark)
        internal const char BOMSWAP = '\xFFFE';	// byte-swapped byte order mark
        internal const char NONCHAR = '\xFFFF';	// Not a Character
        internal const char OBJECT = '\xFFFC';	// OBJECT REPLACEMENT CHARACTER
        internal const char REPLACE = '\xFFFD';	// REPLACEMENT CHARACTER

        internal const char ENQUAD = '\x2000';	// EN QUAD
        internal const char EMQUAD = '\x2001';	// EM QUAD
        internal const char ENSP = '\x2002';	// EN SPACE
        internal const char EMSP = '\x2003';	// EM SPACE
        internal const char EMSP3 = '\x2004';	// THREE-PER-EM SPACE
        internal const char EMSP4 = '\x2005';	// FOUR-PER-EM SPACE
        internal const char EMSP6 = '\x2006';	// SIX-PER-EM SPACE
        internal const char FIGSP = '\x2007';	// FIGURE SPACE
        internal const char PUNSP = '\x2008';	// PUNCTUATION SPACE
        internal const char THINSP = '\x2009';	// THIN SPACE
        internal const char HAIRSP = '\x200A';	// HAIR SPACE
        internal const char ZWSP = '\x200B';	// ZERO WIDTH SPACE
        internal const char ZWNJ = '\x200C';	// ZERO WIDTH NON-JOINER
        internal const char ZWJ = '\x200D';	    // ZERO WIDTH JOINER
        internal const char LTR = '\x200E';	    // LEFT-TO-RIGHT MARK
        internal const char RTL = '\x200F';	    // RIGHT-TO-LEFT MARK
        internal const char HYPHEN = '\x2010';	// HYPHEN
        internal const char NBHYPHEN = '\x2011';	// NON-BREAKING HYPHEN
        internal const char FIGDASH = '\x2012';	    // FIGURE DASH
        internal const char ENDASH = '\x2013';	    // EN DASH
        internal const char EMDASH = '\x2014';	    // EM DASH
        //internal const char IDEOSP = '\x3000';	// IDEOGRAPHIC SPACE    定義済み

        internal const char EURO = '\x20AC';	// EURO SIGN

        internal const char SURROGATE_FIRST = '\xD800';	// First surrogate
        internal const char HI_SURROGATE_FIRST = '\xD800';	// First High Surrogate
        internal const char PV_HI_SURROGATE_FIRST = '\xDB80';	// <Private Use High Surrogate, First>
        internal const char PV_HI_SURROGATE_LAST = '\xDBFF';	// <Private Use High Surrogate, Last>
        internal const char HI_SURROGATE_LAST = '\xDBFF';	// Last High Surrogate
        internal const char LO_SURROGATE_FIRST = '\xDC00';	// <Low Surrogate, First>
        internal const char LO_SURROGATE_LAST = '\xDFFF';	// <Low Surrogate, Last>
        internal const char SURROGATE_LAST = '\xDFFF';	// Last surrogate

        internal const char HANGUL_JAMO_FIRST = '\x1100';
        internal const char HANGUL_JAMO_LEAD_FIRST = '\x1100';
        internal const char HANGUL_JAMO_LEAD_LAST = '\x115F';
        internal const char HANGUL_JAMO_VOWEL_FIRST = '\x1160';
        internal const char HANGUL_JAMO_VOWEL_LAST = '\x11A2';
        internal const char HANGUL_JAMO_TRAIL_FIRST = '\x11A8';
        internal const char HANGUL_JAMO_TRAIL_LAST = '\x11F9';
        internal const char HANGUL_JAMO_LAST = '\x11FF';
    }

    //======================================================================
    // class USZ
    //======================================================================
    static internal class USZ
    {
        // The set of Unicode line-break chars
        internal const string EOLCHARSET = "\x000D\x000A\x2028\x2029\x0085";
        internal const string CRLF = "\x000D\x000A";
        internal const string LF = "\x000A";
        internal const string CR = "\x000D";
        internal const string LS = "\x2028";
        internal const string PS = "\x2029";
        internal const string NEL = "\x0085";
    }

    //======================================================================
    // CharUtil
    //======================================================================
    static internal class CharUtil
    {
        //------------------------------------------------------------
        // CharUtil.UTF8LengthOfUnicode
        //
        /// <summary>
        /// <para>(sscli, csharp\inc\utf.h)</para>
        /// <para>Count the UTF8 characters
        /// which are converted from a given UTF16 string.</para>
        /// </summary>
        /// <param name="unicodeStr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal int UTF8LengthOfUnicode(string unicodeStr)
        {
            return UTF8LengthOfUTF16(unicodeStr);
        }

        //------------------------------------------------------------
        // CharUtil.UTF8LengthOfUTF16
        //
        /// <summary>
        /// <para>(sscli, csharp\inc\utf.h)</para>
        /// <para>Count the UTF8 characters
        /// which are converted from a given UTF16 string.</para>
        /// </summary>
        /// <param name="utf16Str"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal int UTF8LengthOfUTF16(string utf16Str)
        {
            if (String.IsNullOrEmpty(utf16Str)) return 0;

            byte[] srcBytes = Encoding.Unicode.GetBytes(utf16Str);
            byte[] dstBytes = ConvertBytes(Encoding.Unicode, Encoding.UTF8, srcBytes);
            return dstBytes.Length;
        }

        //------------------------------------------------------------
        // CharUtil.ConvertBytes
        //
        /// <summary></summary>
        /// <param name="srcEnc"></param>
        /// <param name="dstEnc"></param>
        /// <param name="srcBytes"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal byte[] ConvertBytes(Encoding srcEnc, Encoding dstEnc, byte[] srcBytes)
        {
            if (srcBytes == null)
            {
                return null;
            }
            if (srcBytes.Length == 0)
            {
                return new byte[0];
            }

            try
            {
                return Encoding.Convert(srcEnc, dstEnc, srcBytes);
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return null;
        }

        //------------------------------------------------------------
        // CreateEncoding   (1)
        //
        /// <summary>Createing a System.Text.Encoding instance by a codePage.</summary>
        /// <param name="codePage">a codepage value of type int.</param>
        /// <param name="encoding">The created Encoding instance.</param>
        /// <returns>If failed to create, return false.</returns>
        //------------------------------------------------------------
        static internal bool CreateEncoding(int codePage, ref System.Text.Encoding encoding)
        {
            System.Text.Encoding enc = System.Text.Encoding.Default;

            try
            {
                enc = System.Text.Encoding.GetEncoding(codePage);
            }
            catch (ArgumentException)
            {
                // ArgumentOutOfRangeException is included.
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            encoding = enc;
            return true;
        }

        //------------------------------------------------------------
        // CreateEncoding   (2)
        //
        /// <summary>Createing a System.Text.Encoding instance by an encoding name.</summary>
        /// <param name="name">An encoding name.</param>
        /// <param name="encoding">The created Encoding instance.</param>
        /// <returns>If failed to create, return false.</returns>
        //------------------------------------------------------------
        static internal bool CreateEncoding(string name, ref System.Text.Encoding encoding)
        {
            System.Text.Encoding enc = System.Text.Encoding.Default;

            try
            {
                enc = System.Text.Encoding.GetEncoding(name);
            }
            catch (ArgumentException)
            {
                // ArgumentOutOfRangeException is included.
                return false;
            }
            encoding = enc;
            return true;
        }

        //------------------------------------------------------------
        // SplitToLines
        //
        /// <summary>
        /// Create a array of lines from a text.
        /// </summary>
        //------------------------------------------------------------
        static internal string[] SplitToLines(string text)
        {
            if (text == null) return null;
            string[] separators = { "\r\n", "\n", "\r", "\x0085", "\x2028", "\x2029", };
            return text.Split(separators, StringSplitOptions.None);
        }

        //------------------------------------------------------------
        // GetLineByIndex
        //
        /// <summary>
        /// <para>Return the line at the line index in a text. If failed, return null.</para>
        /// <para>The index starts with 0.</para>
        /// </summary>
        //------------------------------------------------------------
        static internal string GetLineByIndex(string text, int index)
        {
            if (text == null)
            {
                return null;
            }

            string[] lines = SplitToLines(text);

            if (index >= 0 && index < lines.Length)
            {
                return lines[index];
            }
            return null;
        }

        //------------------------------------------------------------
        // HasBadChar
        //
        /// <summary>If a string includes 0xFFFF, return true.</summary>
        //------------------------------------------------------------
        static internal bool HasBadChars(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == '\xFFFF') return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // ClobberBadChars
        //
        /// <summary>If a string includes 0xFFFF, replaces with '?'.</summary>
        //------------------------------------------------------------
        static internal void ClobberBadChars(ref string str)
        {
            if (str == null) return;
            System.Text.StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; ++i)
            {
                sb.Append((str[i] == '\xFFFF' ? '.' : str[i]));
            }
            str = sb.ToString();
        }

#if false
        //============================================================
        // Swap<T>
        //
        /// <summary>Swap two values of type T.</summary>
        //============================================================
        static internal void Swap<T>(ref T x, ref T y)
        {
            T t = x;
            x = y;
            y = t;
        }
#endif

        //------------------------------------------------------------
        // BinarySearch<T>
        //
        /// <summary>Binary search in an array of type T.</summary>
        /// <param name="arr">array of type T.</param>
        /// <param name="elem">The value to find.</param>
        /// <param name="idx">if found, the index is set, or negative value is set.</param>
        /// <returns>If found, return true.</returns>
        //------------------------------------------------------------
        static internal bool BinarySearch<T>(T[] arr, T elem, out int idx)
        {
            try
            {
                idx = Array.BinarySearch(arr, elem);
            }
            catch (ArgumentException)
            {
                idx = -arr.Length;
                return false;
            }
            catch (RankException)
            {
                idx = -arr.Length;
                return false;
            }
            catch (InvalidOperationException)
            {
                idx = -arr.Length;
                return false;
            }
            return (idx >= 0);
        }

        //------------------------------------------------------------
        // AdjustListCount  (1)
        //
        /// <summary>Add or remove list elements to the specifiled count.</summary>
        /// <param name="cnt">elements count.</param>
        /// <param name="lst">List.</param>
        /// <param name="val">If elements are not enough, add this value.</param>
        //------------------------------------------------------------
        static internal void AdjustListCount<T>(List<T> lst, int cnt, T val)
        {
            if (lst.Count == cnt) return;
            else if (lst.Count > cnt)
            {
                lst.RemoveRange(cnt, lst.Count - cnt);
            }
            else
            {
                while (lst.Count < cnt) lst.Add(val);
            }
        }

        //------------------------------------------------------------
        // AdjustListCount  (2)
        //
        /// <summary>Add or remove list elements to the specifiled count.</summary>
        /// <param name="cnt">elements count.</param>
        /// <param name="lst">List.</param>
        /// <param name="val">If elements are not enough, add the default value of type T.</param>
        //------------------------------------------------------------
        static internal void AdjustListCount<T>(List<T> lst, int cnt)
        {
            AdjustListCount<T>(lst, cnt, default(T));
        }

        //------------------------------------------------------------
        // RemoveElementsFromList
        //
        /// <summary>
        /// Creates a new list which has no matching elements from list.
        /// </summary>
        /// <param name="list">List.</param>
        /// <param name="match">Matching pattern.</param>
        /// <returns>New list.</returns>
        //------------------------------------------------------------
        static internal List<T> RemoveElementsFromList<T>(List<T> list, T match) where T : class
        {
            if (list == null) return null;

            List<T> newList = new List<T>();
            for (int i = 0; i < list.Count; ++i)
            {
                if (match.Equals(list[i])) continue;
                newList.Add(list[i]);
            }
            return newList;
        }

        //
        // Useful functions...  (uncs\CSharp\Inc\TokData.cs から)
        //

        //------------------------------------------------------------
        // IsIdentifierProp
        //------------------------------------------------------------
        static internal bool IsIdentifierProp(char c)
        {
            if (Char.IsLetter(c))
            {
                return true;
            }
            return (Char.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber);
        }
        //inline BOOL IsIdentifierProp (BYTE prop) 
        //{ 
        //    return IsPropAlpha(prop) || 
        //           IsPropLetterDigit(prop); 
        //}

        //------------------------------------------------------------
        // IsIdentifierChar
        //
        /// <summary>
        /// Return true if a character can be in identifier, that is,
        /// alphabetic or Letter_Numeric or '_'.
        /// </summary>
        /// <param name="c">character.</param>
        /// <returns>If can, return true.</returns>
        //------------------------------------------------------------
        static internal bool IsIdentifierChar(char c)
        {
            if (Char.IsLetter(c))
            {
                return true;
            }
            if (c == '_')
            {
                return true;
            }
            return (Char.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber);
        }

        //------------------------------------------------------------
        // IsIdentifierPropOrDigit
        //------------------------------------------------------------
        //static internal bool IsIdentifierPropOrDigit(BYTE prop)
        //{
        //    return IsIdentifierProp(prop) ||
        //    IsPropDecimalDigit(prop) ||
        //    IsPropNonSpacingMark(prop) ||
        //    IsPropCombiningMark(prop) ||
        //    IsPropConnectorPunctuation(prop) ||
        //    IsPropOtherFormat(prop);
        //}

        //------------------------------------------------------------
        // IsIdentifierCharOrDigit
        //
        /// <summary>
        /// Return true if a unicode character is one of
        /// <list type="bullet">
        /// <item><description>Alphabetic</description></item>
        /// <item><description>Letter_Number (NL)</description></item>
        /// <item><description>Decimal_Number (Nd)</description></item>
        /// <item><description>Nonspacing_Mark (Mn)</description></item>
        /// <item><description>Spacing_Mark (Mc)</description></item>
        /// <item><description>Connector_Punctuation (Pc)</description></item>
        /// <item><description>Format (Cf)</description></item>
        /// </list>
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsIdentifierCharOrDigit(char c)
        {
            System.Globalization.UnicodeCategory uc;

            //if (IsIdentifierProp(c))
            //{
            //    return true;
            //}
            if (Char.IsLetter(c))
            {
                return true;
            }

            uc = Char.GetUnicodeCategory(c);

            if (
                uc == System.Globalization.UnicodeCategory.DecimalDigitNumber ||
                uc == System.Globalization.UnicodeCategory.NonSpacingMark ||
                uc == System.Globalization.UnicodeCategory.SpacingCombiningMark ||
                uc == System.Globalization.UnicodeCategory.ConnectorPunctuation ||
                uc == System.Globalization.UnicodeCategory.Format)
            {
                return true;
            }

            return (uc == UnicodeCategory.LetterNumber);
        }

        //------------------------------------------------------------
        //
        //------------------------------------------------------------
        //	inline BOOL IsIdentifierCharOrDigit (WCHAR c) 
        //	{ 
        //	    return IsIdentifierPropOrDigit(UProp(c)); 
        //	}

        //------------------------------------------------------------
        // ASCII characters
        //------------------------------------------------------------
        private const uint bAlp = 1 << 0;  // Alphabet
        private const uint bDig = 1 << 1;  // Digit
        private const uint bId1 = 1 << 2;  // Identifier First Character
        private const uint bId2 = 1 << 3;  // Identifier Following Character

        static internal uint[] ASCIICharInfo =
        {
			// NULL	0	0x00
			0,
			// SOH	1	0x01
			0,
			// STX	2	0x02
			0,
			// ETX	3	0x03
			0,
			// EOT	4	0x04
			0,
			// ENQ	5	0x05
			0,
			// ACK	6	0x06
			0,
			// BEL	7	0x07
			0,
			// BS	8	0x08
			0,
			// HT	9	0x09
			0,
			// LF	10	0x0A
			0,
			// VT	11	0x0B
			0,
			// FF	12	0x0C
			0,
			// CR	13	0x0D
			0,
			// SO	14	0x0E
			0,
			// SI	15	0x0F
			0,
			// DLE	16	0x10
			0,
			// DC1	17	0x11
			0,
			// DC2	18	0x12
			0,
			// DC3	19	0x13
			0,
			// DC4	20	0x14
			0,
			// NAK	21	0x15
			0,
			// SYN	22	0x16
			0,
			// ETB	23	0x17
			0,
			// CAN	24	0x18
			0,
			// EM	25	0x19
			0,
			// SUB	26	0x1A
			0,
			// ESC	27	0x1B
			0,
			// FS	28	0x1C
			0,
			// GS	29	0x1D
			0,
			// RS	30	0x1E
			0,
			// US	31	0x1F
			0,
			// SP	32	0x20
			0,
			// !	33	0x21
			0,
			// "	34	0x22
			0,
			// #	35	0x23
			0,
			// $	36	0x24
			0,
			// %	37	0x25
			0,
			// &	38	0x26
			0,
			// '	39	0x27
			0,
			// (	40	0x28
			0,
			// )	41	0x29
			0,
			// *	42	0x2A
			0,
			// +	43	0x2B
			0,
			// ,	44	0x2C
			0,
			// -	45	0x2D
			0,
			// .	46	0x2E
			0,
			// /	47	0x2F
			0,
			// 0	48	0x30
			0 | bDig | bId2,
			// 1	49	0x31
			0 | bDig | bId2,
			// 2	50	0x32
			0 | bDig | bId2,
			// 3	51	0x33
			0 | bDig | bId2,
			// 4	52	0x34
			0 | bDig | bId2,
			// 5	53	0x35
			0 | bDig | bId2,
			// 6	54	0x36
			0 | bDig | bId2,
			// 7	55	0x37
			0 | bDig | bId2,
			// 8	56	0x38
			0 | bDig | bId2,
			// 9	57	0x39
			0 | bDig | bId2,
			// :	58	0x3A
			0,
			// ;	59	0x3B
			0,
			// <	60	0x3C
			0,
			// =	61	0x3D
			0,
			// >	62	0x3E
			0,
			// ?	63	0x3F
			0,
			// @	64	0x40
			0,
			// A	65	0x41
			0 | bAlp | bId1 | bId2,
			// B	66	0x42
			0 | bAlp | bId1 | bId2,
			// C	67	0x43
			0 | bAlp | bId1 | bId2,
			// D	68	0x44
			0 | bAlp | bId1 | bId2,
			// E	69	0x45
			0 | bAlp | bId1 | bId2,
			// F	70	0x46
			0 | bAlp | bId1 | bId2,
			// G	71	0x47
			0 | bAlp | bId1 | bId2,
			// H	72	0x48
			0 | bAlp | bId1 | bId2,
			// I	73	0x49
			0 | bAlp | bId1 | bId2,
			// J	74	0x4A
			0 | bAlp | bId1 | bId2,
			// K	75	0x4B
			0 | bAlp | bId1 | bId2,
			// L	76	0x4C
			0 | bAlp | bId1 | bId2,
			// M	77	0x4D
			0 | bAlp | bId1 | bId2,
			// N	78	0x4E
			0 | bAlp | bId1 | bId2,
			// O	79	0x4F
			0 | bAlp | bId1 | bId2,
			// P	80	0x50
			0 | bAlp | bId1 | bId2,
			// Q	81	0x51
			0 | bAlp | bId1 | bId2,
			// R	82	0x52
			0 | bAlp | bId1 | bId2,
			// S	83	0x53
			0 | bAlp | bId1 | bId2,
			// T	84	0x54
			0 | bAlp | bId1 | bId2,
			// U	85	0x55
			0 | bAlp | bId1 | bId2,
			// V	86	0x56
			0 | bAlp | bId1 | bId2,
			// W	87	0x57
			0 | bAlp | bId1 | bId2,
			// X	88	0x58
			0 | bAlp | bId1 | bId2,
			// Y	89	0x59
			0 | bAlp | bId1 | bId2,
			// Z	90	0x5A
			0 | bAlp | bId1 | bId2,
			// [	91	0x5B
			0,
			// \	92	0x5C
			0,
			// ]	93	0x5D
			0,
			// ^	94	0x5E
			0,
			// _	95	0x5F
			0 | bId1 | bId2,
			// `	96	0x60
			0,
			// a	97	0x61
			0 | bAlp | bId1 | bId2,
			// b	98	0x62
			0 | bAlp | bId1 | bId2,
			// c	99	0x63
			0 | bAlp | bId1 | bId2,
			// d	100	0x64
			0 | bAlp | bId1 | bId2,
			// e	101	0x65
			0 | bAlp | bId1 | bId2,
			// f	102	0x66
			0 | bAlp | bId1 | bId2,
			// g	103	0x67
			0 | bAlp | bId1 | bId2,
			// h	104	0x68
			0 | bAlp | bId1 | bId2,
			// i	105	0x69
			0 | bAlp | bId1 | bId2,
			// j	106	0x6A
			0 | bAlp | bId1 | bId2,
			// k	107	0x6B
			0 | bAlp | bId1 | bId2,
			// l	108	0x6C
			0 | bAlp | bId1 | bId2,
			// m	109	0x6D
			0 | bAlp | bId1 | bId2,
			// n	110	0x6E
			0 | bAlp | bId1 | bId2,
			// o	111	0x6F
			0 | bAlp | bId1 | bId2,
			// p	112	0x70
			0 | bAlp | bId1 | bId2,
			// q	113	0x71
			0 | bAlp | bId1 | bId2,
			// r	114	0x72
			0 | bAlp | bId1 | bId2,
			// s	115	0x73
			0 | bAlp | bId1 | bId2,
			// t	116	0x74
			0 | bAlp | bId1 | bId2,
			// u	117	0x75
			0 | bAlp | bId1 | bId2,
			// v	118	0x76
			0 | bAlp | bId1 | bId2,
			// w	119	0x77
			0 | bAlp | bId1 | bId2,
			// x	120	0x78
			0 | bAlp | bId1 | bId2,
			// y	121	0x79
			0 | bAlp | bId1 | bId2,
			// z	122	0x7A
			0 | bAlp | bId1 | bId2,
			// {	123	0x7B
			0,
			// |	124	0x7C
			0,
			// }	125	0x7D
			0,
			// ~	126	0x7E
			0,
			// DEL	127	0x7F
			0,
        };

        //------------------------------------------------------------
        // IsAsciiChar
        //
        /// <summary>指定された文字が 7 bit の ASCII 文字化を調べる。</summary>
        /// <param name="ch">対象の文字。</param>
        /// <returns>当てはまるなら true を返す。</returns>
        //------------------------------------------------------------
        static internal bool IsAsciiChar(char ch)
        {
            return ((ch | 0x7F) == 0x7F);
        }

        //------------------------------------------------------------
        // IsAsciiAlphabet
        //
        /// <summary>
        /// Determine if a character is an ASCII character. (0x00 - 0x7F)
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsAsciiAlphabet(char ch)
        {
            if (!IsAsciiChar(ch)) return false;
            return (ASCIICharInfo[ch] & bAlp) != 0;
        }

        //------------------------------------------------------------
        // IsAsciiDigit
        //
        /// <summary>指定された文字が ASCII 文字の 10 進数を表す文字であるかを調べる。</summary>
        /// <param name="ch">対象の文字。</param>
        /// <returns>当てはまるなら true を返す。</returns>
        //------------------------------------------------------------
        static internal bool IsAsciiDigit(char ch)
        {
            if (!IsAsciiChar(ch)) return false;
            return (ASCIICharInfo[ch] & bDig) != 0;
        }

        //------------------------------------------------------------
        // IsAsciiIdentifierChar1
        //
        /// <summary>指定された文字が識別子の先頭に使える文字であるかを調べる。</summary>
        /// <param name="ch">対象の文字。</param>
        /// <returns>当てはまるなら true を返す。</returns>
        //------------------------------------------------------------
        static internal bool IsAsciiIdentifierChar1(char ch)
        {
            if (!IsAsciiChar(ch)) return true;
            return (ASCIICharInfo[ch] & bId1) != 0;
        }

        //------------------------------------------------------------
        // IsAsciiIdentifierChar2
        //
        /// <summary>指定された文字が識別子の 2 文字名以降に使える文字であるかを調べる。</summary>
        /// <param name="ch">対象の文字。</param>
        /// <returns>当てはまるなら true を返す。</returns>
        //------------------------------------------------------------
        static internal bool IsAsciiIdentifierChar2(char ch)
        {
            if (!IsAsciiChar(ch)) return true;
            return (ASCIICharInfo[ch] & bId2) != 0;
        }

        //============================================================
        // Sting to Number
        //============================================================
        //------------------------------------------------------------
        // CharDecimalValue
        //------------------------------------------------------------
        static internal int[] CharDecimalValue =
        {
            //NULL	SOH		STX		ETX		EOT		ENQ		ACK		BEL
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //BS	HT		LF		VT		FF		CR		SO		SI
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //DLE	DC1		DC2		DC3		DC4		NAK		SYN		ETB
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //CAN	EM		SUB		ESC		FS		GS		RS		US
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //SP	!		"		#		$		%		&		'
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //(		)		*		+		,		-		.		/
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //0		1		2		3		4		5		6		7
            0,		1,		2,		3,		4,		5,		6,		7,

            //8		9		:		;		<		=		>		?
            8,		9,		-1,		-1,		-1,		-1,		-1,		-1,
        };


        //------------------------------------------------------------
        // CharHexValue
        //------------------------------------------------------------
        static internal int[] CharHexValue =
        {
            //NULL	SOH		STX		ETX		EOT		ENQ		ACK		BEL
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //BS		HT		LF		VT		FF		CR		SO		SI
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //DLE	DC1		DC2		DC3		DC4		NAK		SYN		ETB
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //CAN	EM		SUB		ESC		FS		GS		RS		US
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //SP	!		"		#		$		%		&		'
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //(		)		*		+		,		-		.		/
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //0		1		2		3		4		5		6		7
            0,		1,		2,		3,		4,		5,		6,		7,

            //8		9		:		;		<		=		>		?
            8,		9,		-1,		-1,		-1,		-1,		-1,		-1,

            //@		A		B		C		D		E		F		G
            -1,		10,		11,		12,		13,		14,		15,		-1,

            //H		I		J		K		L		M		N		O
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //P		Q		R		S		T		U		V		W
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //X		Y		Z		[		\		]		^		_
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //`		a		b		c		d		e		f		g
            -1,		10,		11,		12,		13,		14,		15,		-1,

            //h		i		j		k		l		m		n		o
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //p		q		r		s		t		u		v		w
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,

            //x		y		z		{		|		}		~		DEL
            -1,		-1,		-1,		-1,		-1,		-1,		-1,		-1,
        };

        //------------------------------------------------------------
        // IsOctalDigit
        //
        /// <summary>
        /// Determine if a character is an octal digit.
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsOctalDigit(char c)
        {
            if ((0x3F | c) == 0x3F)
            {
                int val = CharDecimalValue[(int)c];
                return ((val < 8) ? true : false);
            }
            return false;
        }

        //------------------------------------------------------------
        // OctalValue
        //
        /// <summary>
        /// <para>If a character is an octal digit character, convert it to its numerical value.</para>
        /// <para>Otherwise, return a negative value.</para>
        /// </summary>
        //------------------------------------------------------------
        static internal int OctalValue(char c)
        {
            if ((0x3F | c) == 0x3F)
            {
                int val = CharDecimalValue[(int)c];
                return ((val < 8) ? val : -1);
            }
            return -1;
        }

        //------------------------------------------------------------
        // OctalStringToInteger
        //
        /// <summary>
        /// <para>If a string is an octal numerical literal, convert it to its numerical value.</para>
        /// <para>If failed, throw an exception.</para>
        /// <para>The first character must be an octal digit character.</para>
        /// </summary>
        /// <param name="index">The index of the first character of a numerical literal,
        /// and will be set the index of the character next to the converted numerical literl.</param>
        //------------------------------------------------------------
        static internal ulong OctalStringToInteger(string source, ref int index)
        {
            const ulong baseNumber = 8;
            const ulong maxValue = (ulong)0xFFFFFFFFFFFFFFFF;
            const ulong maxQuotient = (ulong)(maxValue / baseNumber);
            const ulong maxReminder = (ulong)(maxValue % baseNumber);

            if (source == null || source.Length <= 0 || index >= source.Length)
            {
                throw new ArgumentNullException();
            }

            int idx = index;

            // If the first character is not a octal digit, throw an exception.
            int hv = OctalValue(source[idx]);
            if (hv < 0)
            {
                throw new ArgumentException();
            }
            ulong val = (ulong)hv;

            // Then, if overflow, throw an exception.
            ++idx;
            while (idx < source.Length)
            {
                hv = OctalValue(source[idx]);
                if (hv < 0) break;

                if (val > maxQuotient || (val == maxQuotient && (ulong)hv > maxReminder))
                {
                    throw new OverflowException();
                }
                val = baseNumber * val + (ulong)hv;
                ++idx;
            }
            index = idx;
            return val;
        }

        //------------------------------------------------------------
        // IsDecDigit
        //
        /// <summary>
        /// Determine if a character is a decimal digit.
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsDecimalDigit(char c)
        {
            //return (
            //    (c >= '0' && c <= '9') ||
            //    (c >= 'A' && c <= 'F') ||
            //    (c >= 'a' && c <= 'f')
            //    );

            return (((0x3F | c) == 0x3F) && (CharDecimalValue[(int)c] >= 0));
        }

        //------------------------------------------------------------
        // DecValue
        //
        /// <summary>
        /// <para>If a character is a decimal digit character, convert it to its numerical value.</para>
        /// <para>Otherwise, return a negative value.</para>
        /// </summary>
        //------------------------------------------------------------
        static internal int DecimalValue(char c)
        {
            //if (IsHexDigit(c) == false) return -1;
            //return (int)((c >= '0' && c <= '9') ? c - '0' : (c & 0xdf) - 'A' + 10);

            return (((0x3F | c) == 0x3F) ? CharDecimalValue[(int)c] : -1);
        }

        //------------------------------------------------------------
        // DecimalStringToInteger
        //
        /// <summary>
        /// <para>If a string is a decimal numerical literal, convert it to its numerical value.</para>
        /// <para>If failed, throw an exception.</para>
        /// <para>The first character must be an decimal digit character.</para>
        /// </summary>
        /// <param name="index">The index of the first character of a numerical literal,
        /// and will be set the index of the character next to the converted numerical literl.</param>
        //------------------------------------------------------------
        static internal ulong DecimalStringToInteger(string source, ref int index)
        {
            const ulong baseNumber = 10;
            const ulong maxValue = (ulong)0xFFFFFFFFFFFFFFFF;
            const ulong maxQuotient = (ulong)(maxValue / baseNumber);
            const ulong maxReminder = (ulong)(maxValue % baseNumber);

            if (source == null || source.Length <= 0 || index >= source.Length)
            {
                throw new ArgumentNullException();
            }

            int idx = index;

            // If the first character is not a decimal digit, throw an exception.
            int hv = DecimalValue(source[idx]);
            if (hv < 0)
            {
                throw new ArgumentException();
            }
            ulong val = (ulong)hv;

            // Then, if overflow, throw an exception.
            ++idx;
            while (idx < source.Length)
            {
                hv = DecimalValue(source[idx]);
                if (hv < 0) break;

                if (val > maxQuotient || (val == maxQuotient && (ulong)hv > maxReminder))
                {
                    throw new OverflowException();
                }
                val = baseNumber * val + (ulong)hv;
                ++idx;
            }
            index = idx;
            return val;
        }

        //------------------------------------------------------------
        // IsHexDigit
        //
        /// <summary>
        /// Determine if a character is a hex digit.
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsHexDigit(char c)
        {
            //return (
            //    (c >= '0' && c <= '9') ||
            //    (c >= 'A' && c <= 'F') ||
            //    (c >= 'a' && c <= 'f')
            //    );

            return (((0x7F | c) == 0x7F) && (CharHexValue[(int)c] >= 0));
        }

        //------------------------------------------------------------
        // HexValue
        //
        /// <summary>
        /// <para>If a character is a hex digit character, convert it to its numerical value.</para>
        /// <para>Otherwise, return a negative value.</para>
        /// </summary>
        //------------------------------------------------------------
        static internal int HexValue(char c)
        {
            //if (IsHexDigit(c) == false) return -1;
            //return (int)((c >= '0' && c <= '9') ? c - '0' : (c & 0xdf) - 'A' + 10);

            return (((0x7F | c) == 0x7F) ? CharHexValue[(int)c] : -1);
        }

        //------------------------------------------------------------
        // HexStringToValue
        //
        /// <summary>
        /// <para>If a string is a hex numerical literal, convert it to its numerical value.</para>
        /// <para>If failed, throw an exception.</para>
        /// <para>The first character must be an decimal digit character.</para>
        /// </summary>
        /// <param name="index">The index of the first character of a numerical literal,
        /// and will be set the index of the character next to the converted numerical literl.</param>
        //------------------------------------------------------------
        static internal ulong HexStringToInteger(string source, ref int index)
        {
            const ulong baseNumber = 16;
            const ulong maxValue = (ulong)0xFFFFFFFFFFFFFFFF;
            const ulong maxQuotient = (ulong)(maxValue / baseNumber);
            const ulong maxReminder = (ulong)(maxValue % baseNumber);

            if (source == null || source.Length <= 0 || index >= source.Length)
            {
                throw new ArgumentNullException();
            }

            int idx = index;

            // If the first character is not a hex digit, throw an exception.
            int hv = HexValue(source[idx]);
            if (hv < 0)
            {
                throw new ArgumentException();
            }
            ulong val = (ulong)hv;

            // Then, if overflow, throw an exception.
            ++idx;
            while (idx < source.Length)
            {
                hv = HexValue(source[idx]);
                if (hv < 0) break;

                if (val > maxQuotient || (val == maxQuotient && (ulong)hv > maxReminder))
                {
                    throw new OverflowException();
                }
                val = baseNumber * val + (ulong)hv;
                ++idx;
            }
            index = idx;
            return val;
        }

        //------------------------------------------------------------
        // wcstoul64    (separated from class ConsoleArgs)
        //
        /// <summary>
        /// <para>Similar to wcstoul, but returns a 64-bit number, and always assumes base is 0</para>
        /// <para>Convert a octal or decimal or hex numerical literal to a value of type long.</para>
        /// <para>If error, throw a exception.</para>
        /// </summary>
        /// <param name="index">The index of the first character of a numerical literal,
        /// and will be set the index of the character next to the converted numerical literl.</param>
        //
        // integer-literal::
        //     decimal-integer-literal
        //     hexadecimal-integer-literal
        // decimal-integer-literal::
        //     decimal-digits integer-type-suffixopt
        // decimal-digits::
        //     decimal-digit
        //     decimal-digits decimal-digit
        // decimal-digit:: one of
        //     0 1 2 3 4 5 6 7 8 9
        // integer-type-suffix:: one of
        //     U u L l UL Ul uL ul LU Lu lU lu
        // hexadecimal-integer-literal::
        //     0x hex-digits integer-type-suffixopt
        //     0X hex-digits integer-type-suffixopt
        // hex-digits::
        //     hex-digit
        //     hex-digits hex-digit
        // hex-digit:: one of
        //     0 1 2 3 4 5 6 7 8 9 A B C D E F a b c d e f
        //------------------------------------------------------------
        static internal ulong wcstoul64(string source, ref int index)
        {
            ulong val = 0; // accumulator
            int idx = index;
            char chr;                // current char
            bool negated = false;

            if (source == null || index < 0 || index >= source.Length)
            {
                throw new ArgumentException();
            }

            chr = source[idx];
            while (Char.IsWhiteSpace(chr))
            {
                ++idx;
                if (idx >= source.Length)
                {
                    throw new ArgumentException();
                }
                chr = source[idx];
            }

            if (chr == '+' || chr == '-')
            {
                if (chr == '-') negated = true;
                ++idx;
                if (idx >= source.Length)
                {
                    throw new ArgumentException();
                }
                chr = source[idx];
            }

            // If the first character is '0', treat the numerical literal as being octal or hex.
            if (chr == '0')
            {
                ++idx;
                if (idx >= source.Length)
                {
                    val = 0;
                }
                else
                {
                    chr = source[idx];
                    if (chr == 'x' || chr == 'X')
                    {
                        // Hex
                        ++idx;
                        if (idx >= source.Length)
                        {
                            throw new ArgumentException();
                        }
                        val = CharUtil.HexStringToInteger(source, ref idx);
                    }
                    else
                    {
                        // Octal
                        // Back to '0'.
                        --idx;
                        val = CharUtil.OctalStringToInteger(source, ref idx);
                    }
                }
            }
            else
            {
                // Decimal
                val = CharUtil.DecimalStringToInteger(source, ref idx);
            }

            // store pointer to char that stopped the scan
            index = idx;

            if (negated)
                // negate result if there was a neg sign
                val = (ulong)(-(long)val);

            return val;
        }

        //------------------------------------------------------------
        // wcstoui32
        //
        /// <summary>
        /// Convert a octal or decimal or hex numerical literal to a value of type int.
        /// </summary>
        /// <param name="index">The index of the first character of a numerical literal,
        /// and will be set the index of the character next to the converted numerical literl.</param>
        //------------------------------------------------------------
        static internal uint wcstoui32(string source, ref int index)
        {
            int idx = index;
            ulong value = wcstoul64(source, ref idx);
            if (value <= 0xFFFFFFFF)
            {
                index = idx;
                return (uint)value;
            }

            throw new CSException(CSCERRID.ERR_IntOverflow);
        }

        //------------------------------------------------------------
        // IsWhitespaceChar
        //
        /// <summary>
        /// Determine if a character is a white space character.
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsWhitespaceChar(char ch)
        {
            return Char.IsWhiteSpace(ch);
        }

        //------------------------------------------------------------
        // IsEndOfLineChar
        //
        /// <summary>
        /// \r, \n, 0x2029 (PARAGRAPH SEPARATOR), 0x2028 (LINE SEPARATOR), 0x0085.
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsEndOfLineChar(char ch)
        {
            return
                ch == '\r' ||
                ch == '\n' ||
                ch == UCH.PS ||
                ch == UCH.LS ||
                ch == 0x0085;
        }

        //------------------------------------------------------------
        // IsWhitespaceCharNotEndOfLine
        //
        /// <summary>
        /// Determine if a character is a white space character except
        /// \r, \n, 0x2029 (PARAGRAPH SEPARATOR), 0x2028 (LINE SEPARATOR), 0x0085.
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsWhitespaceCharNotEndOfLine(char ch)
        {
            if (IsEndOfLineChar(ch)) return false;
            return Char.IsWhiteSpace(ch);
        }

        //------------------------------------------------------------
        // GetUnicodeCategory
        //
        /// <summary>Get Unicode General_Categry of a character.</summary>
        /// <returns>a value of type System.Globalization.UnicodeCategory.</returns>
        //------------------------------------------------------------
        static internal System.Globalization.UnicodeCategory GetUnicodeCategory(char c)
        {
            return System.Char.GetUnicodeCategory(c);
        }

        //------------------------------------------------------------
        // InUicodeCategory
        //
        /// <summary>
        /// Determine if a character is of the specified Unicode General_Category.
        /// </summary>
        /// <param name="uc">General_Category value of type System.Globalization.UnicodeCategory.</param>
        //------------------------------------------------------------
        static internal bool InUicodeCategory(char ch, System.Globalization.UnicodeCategory uc)
        {
            return (System.Char.GetUnicodeCategory(ch) == uc);
        }

        //------------------------------------------------------------
        // IsSpaceSeparator
        //
        /// <summary>
        /// Determine if a character is a Space_Separator (Zs) character of Unicode General_Category.
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsSpaceSeparator(char c)
        {
            return InUicodeCategory(c, UnicodeCategory.SpaceSeparator);
        }

        //------------------------------------------------------------
        // ReplaceControlCharacters
        //
        /// <summary>
        /// Replace control characters in a given string to the specified string.
        /// If the replacement is null, replace by character codes.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string ReplaceControlCharacters(
            string source,
            string replacement)
        {
            if (String.IsNullOrEmpty(source))
            {
                return source;
            }

            bool hasCtrl = false;
            for (int i = 0; i < source.Length; ++i)
            {
                if (Char.IsControl(source[i]))
                {
                    hasCtrl = true;
                    break;
                }
            }
            if (!hasCtrl)
            {
                return source;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < source.Length; ++i)
            {
                char c = source[i];
                if (!Char.IsControl(c))
                {
                    sb.Append(c);
                    continue;
                }
                else if (replacement != null)
                {
                    sb.Append(replacement);
                    continue;
                }
                else
                {
                    if ((int)c < 256)
                    {
                        sb.AppendFormat("(0x{0,0:X2})", (int)c);
                    }
                    else
                    {
                        sb.AppendFormat("(0x{0,0:X4})", (int)c);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
