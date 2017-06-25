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
// File: tokdata.h
//
// ===========================================================================

//============================================================================
// TokData.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // TOKFLAGS
    //
    /// <summary>
    /// <list type="bullet">
    /// <item>Rename (TFF_) to TOKENFLAGS.F_～ .</item>
    /// <item>Rename (TF_) to TOKENFLAGS.～ .</item>
    /// </list>
    /// </summary>
    //======================================================================
    [Flags]
    internal enum TOKFLAGS : uint
    {
        NONE = 0,

        /// <summary>
        /// Token belongs to first set for namespace elements
        /// </summary>
        F_NSELEMENT = 0x00000001,

        /// <summary>
        /// Token belongs to first set for type members
        /// </summary>
        F_MEMBER = 0x00000002,

        /// <summary>
        /// Token belongs to first set for type declarations (sans modifiers)
        /// </summary>
        F_TYPEDECL = 0x00000004,

        /// <summary>
        /// Token is a predefined type
        /// </summary>
        F_PREDEFINED = 0x00000008,

        /// <summary>
        /// Token belongs to first set for term-or-unary-operator
        /// (follows casts), but is not a predefined type.
        /// </summary>
        F_TERM = 0x00000010,

        /// <summary>
        /// Token is an overloadable unary operator
        /// </summary>
        F_OVLUNOP = 0x00000020,

        /// <summary>
        /// Token is an overloadable binary operator
        /// </summary>
        F_OVLBINOP = 0x00000040,

        /// <summary>
        /// Token is NOT an operator even though op != OP_NONE
        /// </summary>
        F_OVLOPKWD = 0x00000080,

        /// <summary>
        /// Token after an ambiguous type-or-expr cast forces a cast
        /// </summary>
        F_CASTEXPR = 0x00000100,

        /// <summary>
        /// Token is a modifier keyword
        /// </summary>
        F_MODIFIER = 0x00000200,

        /// <summary>
        /// Token is "noise"; should be ignored by parser
        /// </summary>
        F_NOISE = 0x00000400,

        /// <summary>
        /// Token is a Microsoft Extension keyword (i.e not part of the ECMA or ISO standard)
        /// </summary>
        F_MSKEYWORD = 0x00000800,

        /// <summary>
        /// Token is not valid to start a sub expression with (like "if").
        /// </summary>
        F_INVALIDSUBEXPRESSION = 0x00001000,

        //
        // These flags are for instances of tokens -- they go in the user bits, of which there are 4.
        //

        /// <summary>
        /// Token has "overhead"
        /// </summary>
        OVERHEAD = 0x1,

        /// <summary>
        /// Unterminated token such as TID_STRINGLIT or TID_CHARLIT
        /// </summary>
        UNTERMINATED = 0x2,

        /// <summary>
        /// Token's overhead was allocated on the heap (only set/checked by CSourceLexer)
        /// </summary>
        HEAPALLOCATED = 0x4,

        /// <summary>
        /// TID_STRINGLIT is a @"..." form
        /// </summary>
        VERBATIMSTRING = 0x8,

        /// <summary>
        /// TID_DOCCOMMENT or TID_MLDOCCOMMENT has been used!
        /// </summary>
        USEDCOMMENT = 0x8,

        /// <summary>
        /// a hexadecimal constant 0xDDDD. Used for enum conversions.
        /// </summary>
        HEXLITERAL = 0x8,
    }

    //======================================================================
    // ESCAPEDNAME
    //
    /// <summary>
    /// <para>Used to represent an identifier that contained unicode escape(s) in source.
    ///	A special structure so we know how int the token was in the source code.</para>
    /// </summary>
    //======================================================================
    internal class ESCAPEDNAME
    {
        /// <summary>
        /// Name (escapes converted)
        /// </summary>
        internal string Name;

        /// <summary>
        /// In-source length
        /// </summary>
        internal int SourceLength;

        //------------------------------------------------------------
        // ESCAPEDNAME Constructor
        //
        /// <summary></summary>
        /// <param name="nm"></param>
        /// <param name="len"></param>
        //------------------------------------------------------------
        internal ESCAPEDNAME(string nm, int len)
        {
            this.Name = nm;
            this.SourceLength = len;
        }

        //------------------------------------------------------------
        // ESCAPEDNAME.Clone
        //
        /// <summary>Duplicate this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ESCAPEDNAME Clone()
        {
            return new ESCAPEDNAME(this.Name, this.SourceLength);
        }

        //------------------------------------------------------------
        // ESCAPEDNAME.ToString
        //
        /// <summary>Return the string representing the fields of this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("ESCAPEDNAME:{0} ({1})",this.Name, this.SourceLength);
        }
    }

    //======================================================================
    // STRLITERAL
    //
    /// <summary>
    /// <para>Holds a string literal, including the converted text and length, as well as
    ///	the in-source length (including delimiters, of course)</para>
    /// </summary>
    //======================================================================
    internal class STRLITERAL
    {
        /// <summary>
        /// String literal.
        /// </summary>
        internal string Str;

        /// <summary>
        /// Length (in-source, including delimiters)
        /// </summary>
        internal int SourceLength;

        //------------------------------------------------------------
        // STRLITERAL Constructor
        //
        /// <summary></summary>
        /// <param name="st"></param>
        /// <param name="ln"></param>
        //------------------------------------------------------------
        internal STRLITERAL(string st, int ln)
        {
            this.Str = st;
            this.SourceLength = ln;
        }

        //------------------------------------------------------------
        // STRLITERAL.Clone
        //
        /// <summary>Duplicate this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STRLITERAL Clone()
        {
            return new STRLITERAL(this.Str, this.SourceLength);
        }

        //------------------------------------------------------------
        // STRLITERAL.ToString
        //
        /// <summary>Return the string representing the fields of this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("STRLITERAL:{0} ({1})", this.Str, this.SourceLength);
        }
    }

    //======================================================================
    //	VSLITERAL
    //
    /// <summary>
    /// <para>Holds a verbatim string literal,
    /// including the converted text and length, as well as the end-position </para>
    /// </summary>
    //======================================================================
    internal class VSLITERAL
    {
        /// <summary>
        /// String literal.
        /// </summary>
        internal string Str = null;

        /// <summary>
        /// End-position of string
        /// </summary>
        internal POSDATA PosEnd;

        //------------------------------------------------------------
        // VSLITERAL Constructor
        //
        /// <summary></summary>
        /// <param name="st"></param>
        /// <param name="ep"></param>
        //------------------------------------------------------------
        internal VSLITERAL(string st, POSDATA ep)
        {
            this.Str = st;
            this.PosEnd = ep;
        }

        //------------------------------------------------------------
        // VSLITERAL
        //
        /// <summary>Duplicate this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal VSLITERAL Clone()
        {
            return new VSLITERAL(this.Str, new POSDATA(this.PosEnd));
        }

        //------------------------------------------------------------
        // VSLITERAL.ToString
        //
        /// <summary>Return the string representing the fields of this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("VSLITERAL:{0} ({1},{2})",
                this.Str, this.PosEnd.LineIndex, this.PosEnd.CharIndex);
        }
    }

    //======================================================================
    // LITERAL
    //
    /// <summary>
    /// <para>Used to represent any token whose text is interesting/essential post-lex,
    /// such as strings, numbers, even comments.  The text pointer</para>
    /// </summary>
    //======================================================================
    internal class LITERAL
    {
        /// <summary>
        /// Text of token (extent of which depends on token;
        /// i.e. comments to not include delimiters, etc.)
        /// </summary>
        internal string Text = null;

        /// <summary>
        /// In-source length
        /// </summary>
        internal int SourceLength = 0;

        //------------------------------------------------------------
        // LITERAL Constructor (1)
        //
        /// <summary></summary>
        /// <param name="tx"></param>
        /// <param name="ln"></param>
        //------------------------------------------------------------
        internal LITERAL(string tx, int ln)
        {
            this.Text = tx;
            this.SourceLength = ln;
        }

        //------------------------------------------------------------
        // LITERAL Constructor (2)
        //
        /// <summary></summary>
        /// <param name="other"></param>
        //------------------------------------------------------------
        internal LITERAL(LITERAL other)
        {
            this.Text = other.Text;
            this.SourceLength = other.SourceLength;
        }

        //------------------------------------------------------------
        // LITERAL.Clone
        //
        /// <summary>Duplicate this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal LITERAL Clone()
        {
            return new LITERAL(this);
        }

        //------------------------------------------------------------
        // LITERAL.ToString
        //
        /// <summary>Return the string representing the fields of this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("LITERAL:{0} ({1})", this.Text, this.SourceLength);
        }
    }

    //======================================================================
    // DOCLITERAL
    //
    /// <summary>
    /// <para>Used to represent a doc comment token
    /// whose text is interesting/essential post-lex,</para>
    /// </summary>
    //======================================================================
    internal class DOCLITERAL
    {
        /// <summary>
        /// Text of token (includes "///", EOL, "/**", and "*/")
        /// </summary>
        internal string Text = null;

        /// <summary>
        /// In-source end-pos (either EOL char or '/' char in terminating "*/")
        /// </summary>
        internal POSDATA PosEnd;

        //------------------------------------------------------------
        // DOCLITERAL Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal DOCLITERAL(string tx, POSDATA pe)
        {
            this.Text = tx;
            this.PosEnd = pe;
        }

        //------------------------------------------------------------
        // DOCLITERAL.Clone
        //
        /// <summary>Duplicate this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal DOCLITERAL Clone()
        {
            return new DOCLITERAL(this.Text, new POSDATA(this.PosEnd));
        }

        //------------------------------------------------------------
        // DOCLITERAL.ToString
        //
        /// <summary>Return the string representing the fields of this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            return String.Format(
                "DOCLITERAL:{0} ({1},{2})",
                this.Text,
                this.PosEnd.LineIndex,
                this.PosEnd.CharIndex);
        }
    }

    //======================================================================
    // CHARLITERAL
    //
    /// <summary>
    /// String literal representing one character. (TOKENID.CHARLIT)
    /// </summary>
    //======================================================================
    internal class CHARLITERAL
    {
        /// <summary>
        /// Character value
        /// </summary>
        internal char Value;

        /// <summary>
        /// Length (in-source, including delimiters)
        /// </summary>
        internal int SourceLength;

        //------------------------------------------------------------
        // CHARLITERAL Constructor
        //
        /// <summary></summary>
        /// <param name="val"></param>
        /// <param name="len"></param>
        //------------------------------------------------------------
        internal CHARLITERAL(char val, int len)
        {
            this.Value = val;
            this.SourceLength = len;
        }

        //------------------------------------------------------------
        // CHARLITERAL.Clone
        //
        /// <summary>Duplicate this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CHARLITERAL Clone()
        {
            return new CHARLITERAL(this.Value, this.SourceLength);
        }

        //------------------------------------------------------------
        // CHARLITERAL.ToString
        //
        /// <summary>Return the string representing the fields of this.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("CHARLITERAL:{0} ({1})", this.Value, this.SourceLength);
        }
    }

    //======================================================================
    // CSTOKEN
    //
    /// <summary>
    /// <para>Here's the new token structure.  Since TOKENDATA will become obsolete, we'll
    /// rename this to TOKENDATA once the compiler is producing the new stream form.</para>
    /// <para>Derives from POSDATA. Store data of token.
    /// The kind of token is stored in POSDATA.UserByte.</para>
    /// </summary>
    //======================================================================
    internal partial class CSTOKEN : POSDATA
    {
        //------------------------------------------------------------
        // CSTOKEN.TokenID
        //
        /// <summary>
        /// (RW) Use UserByte as type of TOKENID.
        /// </summary>
        //------------------------------------------------------------
        internal TOKENID TokenID
        {
            get { return  (TOKENID)base.UserByte; }
            set { base.UserByte = (int)value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.TokenFlags
        //
        /// <summary>
        /// (RW) Use UserBits as type of TOKFLAGS.
        /// </summary>
        //------------------------------------------------------------
        internal TOKFLAGS TokenFlags
        {
            get { return (TOKFLAGS)UserBits; }
            set { base.UserBits = (int)value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.tokenData
        //
        /// <summary>
        /// <para>Based on what the token is, the extra data is kept here</para>
        /// </summary>
        //------------------------------------------------------------
        protected object tokenData = null;

        internal object TokenData
        {
            get { return tokenData; }
        }

        //------------------------------------------------------------
        // CSTOKEN.Name
        //
        /// <summary>
        /// <para>TID_IDENTIFIER (HasOverhead = FALSE), or
        /// TID_UNKNOWN (which is treated as an identifier):</para>
        /// <para>(RW) Use tokenData as type of string.</para>
        /// </summary>
        //------------------------------------------------------------
        internal string Name
        {
            get
            {
                //NAME* Name() const
                //{
                //    ASSERT (Token() == TID_IDENTIFIER || Token() == TID_UNKNOWN);
                //    return (Token() == TID_IDENTIFIER && HasOverhead()) ? pEscName->pName : pName;
                //}

                if (this.IsEscapedName)
                {
                    return this.EscapedName.Name;
                }
                return tokenData as string;
            }
            set
            {
                tokenData = value;
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.EscName
        //
        /// <summary>
        /// <para>TID_IDENTIFIER (HasOverhead = TRUE):</para>
        /// <para>(RW) Use tokenData as an ESCAPEDNAME instance.</para>
        /// </summary>
        //------------------------------------------------------------
        internal ESCAPEDNAME EscapedName
        {
            get { return tokenData as ESCAPEDNAME; }
            set { tokenData = value; }
        }

        /// <summary>
        /// (R) Determine that tokenData is a non-null ESCAPEDNAME.
        /// </summary>
        internal bool IsEscapedName
        {
            get { return (tokenData is ESCAPEDNAME); }
        }

        //------------------------------------------------------------
        // CSTOKEN.StringLiteral
        //
        /// <summary>
        /// <para>TID_STRINGLIT:
        /// STRLITERAL structure containing converted text,
        /// converted length, and in-source length</para>
        /// <para>(RW) Use tokenData as a StringLiteral instance.</para>
        /// </summary>
        //------------------------------------------------------------
        internal STRLITERAL StringLiteral
        {
            get { return tokenData as STRLITERAL; }
            set { tokenData = value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.VSLiteral
        //
        /// <summary>
        /// <para>TID_VSLITERAL:</para>
        /// <para>(RW) Use tokenData as a VSLiteral instance.</para>
        /// </summary>
        //------------------------------------------------------------
        internal VSLITERAL VSLiteral
        {
            get { return tokenData as VSLITERAL; }
            set { tokenData = value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.Literal
        //
        /// <summary>
        /// <para>TID_NUMBER:</para>
        /// <para>(RW) Use tokenData as a Literal instance.</para>
        /// </summary>
        //------------------------------------------------------------
        internal LITERAL Literal
        {
            get { return tokenData as LITERAL; }
            set { tokenData = value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.PosEnd
        //
        /// <summary>
        /// <para>TID_MLCOMMENT: POSDATA containing the position of the end of the comment</para>
        /// <para>(RW) Use tokenData as POSDATA instance.</para>
        /// </summary>
        //------------------------------------------------------------
        internal POSDATA PosEnd
        {
            get { return tokenData as POSDATA; }
            set { tokenData = value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.DocLiteral
        //
        /// <summary>
        /// <para>TID_DOCCOMMENT, TID_MLDOCCOMMENT:
        /// DOCCOMMENT containing text of comment and ending position</para>
        /// <para>(RW) Use tokenData as a DocLiteral instance.</para>
        /// </summary>
        //------------------------------------------------------------
        internal DOCLITERAL DocLiteral
        {
            get { return tokenData as DOCLITERAL; }
            set { tokenData = value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.Length
        //
        /// <summary>
        /// <para>All "fixed" tokens, plus TID_SLCOMMENT:
        /// Length of token (including delimiter(s))</para>
        /// <para>(RW) Use tokenData as type of int.</para>
        /// </summary>
        //------------------------------------------------------------
        internal int Length
        {
            get { return (int)tokenData; }
            set { tokenData = value; }
        }

        //------------------------------------------------------------
        // CSTOKEN.CharLiteral
        //
        /// <summary>
        /// <para>TID_CHARLIT:
        /// Character value and Length (in-source, including delimiters)</para>
        /// <para>(RW) Use tokenData as CharLiteral instance.</para>
        /// </summary>
        //------------------------------------------------------------
        internal CHARLITERAL CharLiteral
        {
            get { return tokenData as CHARLITERAL; }
            set { tokenData = value; }
        }

        //Turns out inline is important for this function.  Dont' remove it without reason

        //------------------------------------------------------------
        // CSTOKEN.IsKeyword
        //
        /// <summary>
        /// Return true if this instance represents a token of a keyword.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsKeyword
        {
            get { return this.TokenID < TOKENID.IDENTIFIER; }
        }

        //------------------------------------------------------------
        // CSTOKEN.IsModifier
        //
        /// <summary>
        /// Return true if this instance represents a token of a modifier.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsModifier
        {
            get
            {
                switch (this.TokenID)
                {
                    case TOKENID.EXTERN:
                    case TOKENID.OVERRIDE:
                    case TOKENID.READONLY:
                    case TOKENID.VIRTUAL:
                    case TOKENID.VOLATILE:
                    case TOKENID.ABSTRACT:
                    case TOKENID.INTERNAL:
                    case TOKENID.PRIVATE:
                    case TOKENID.PROTECTED:
                    case TOKENID.PUBLIC:
                    case TOKENID.SEALED:
                    case TOKENID.UNSAFE:
                    case TOKENID.NEW:
                        return true;

                    default:
                        return false;
                }
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.IsAccessibilityModifier
        //
        /// <summary>
        /// Return true if this instance represents a token of a accessibility modifier.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsAccessibilityModifier
        {
            get
            {
                switch (this.TokenID)
                {
                    case TOKENID.INTERNAL:
                    case TOKENID.PRIVATE:
                    case TOKENID.PROTECTED:
                    case TOKENID.PUBLIC:
                        return true;

                    default:
                        return false;
                }
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.HasOverhead
        //------------------------------------------------------------
        //internal bool HasOverhead()
        //{
        //    return ((base.UserBits & (int)TOKFLAGS.OVERHEAD) != 0);
        //}

        //------------------------------------------------------------
        // CSTOKEN.IsUnterminated
        //
        /// <summary>
        /// Return true if this instance represents a non-terminal symbol.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsUnterminated
        {
            get { return ((base.UserBits & (int)TOKFLAGS.UNTERMINATED) != 0); }
        }

        //------------------------------------------------------------
        // CSTOKEN.IsComment
        //
        /// <summary>
        /// Return true if this instance represents a comment.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsComment
        {
            get
            {
                return (
                    base.UserByte == (int)TOKENID.SLCOMMENT ||
                    base.UserByte == (int)TOKENID.MLCOMMENT ||
                    base.UserByte == (int)TOKENID.DOCCOMMENT ||
                    base.UserByte == (int)TOKENID.MLDOCCOMMENT);
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.IsDocComment
        //
        /// <summary>
        /// Return true if this instance represents a DOCCOMMENT.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsDocComment
        {
            get
            {
                return (
                    base.UserByte == (int)TOKENID.DOCCOMMENT ||
                    base.UserByte == (int)TOKENID.MLDOCCOMMENT);
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.IsNoisy
        //
        /// <summary>
        /// Return true if this instance represents a comment or unknown token.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsNoisy
        {
            get
            {
                return (this.IsComment || this.TokenID == TOKENID.UNKNOWN);
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.CharValue
        //
        /// <summary>
        /// Return true if this instance represents a CHARLIT.
        /// </summary>
        //------------------------------------------------------------
        internal char CharValue()
        {
            DebugUtil.Assert(this.TokenID == TOKENID.CHARLIT);
            return this.CharLiteral.Value;
        }

        //------------------------------------------------------------
        // CSTOKEN.SourceLength
        //
        /// <summary>
        /// <para>ALWAYS returns IN-SOURCE length;
        ///  -1 if token is a multi-line token (TID_MLCOMMENT or TID_VSLITERAL spanning > 1 line)</para>
        /// <para>In sscli, Length</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int SourceLength()
        {
            switch (this.TokenID)
            {
                case TOKENID.MLCOMMENT:
                case TOKENID.VSLITERAL:
                    POSDATA pos;
                    if (this.TokenID == TOKENID.MLCOMMENT)
                    {
                        pos = this.PosEnd;
                    }
                    else
                    {
                        pos = this.VSLiteral.PosEnd;
                    }
                    DebugUtil.Assert(pos != null);

                    // If the token ends on the same line we can return a length
                    if (pos.LineIndex == this.LineIndex)
                    {
                        return pos.CharIndex - base.CharIndex;
                    }
                    // Otherwise, the caller has to deal with this differently
                    return -1;

                case TOKENID.UNKNOWN:
                case TOKENID.IDENTIFIER:
                    {
                        //if (HasOverhead()) return this.EscName.Length;
                        if (this.IsEscapedName)
                        {
                            return this.EscapedName.SourceLength;
                        }
                        DebugUtil.Assert(this.Name != null);
                        return this.Name.Length;
                    }

                case TOKENID.STRINGLIT:
                    return this.StringLiteral.SourceLength;

                case TOKENID.DOCCOMMENT:
                case TOKENID.MLDOCCOMMENT:
                    // If the token ends on the same line we can return a length
                    if (this.PosEnd.LineIndex == base.LineIndex)
                    {
                        return this.PosEnd.CharIndex - base.CharIndex;
                    }
                    DebugUtil.Assert(this.TokenID == TOKENID.MLDOCCOMMENT);

                    // Otherwise, the caller has to deal with this differently
                    return -1;

                case TOKENID.NUMBER:
                    return this.Literal.SourceLength;

                case TOKENID.CHARLIT:
                    return this.CharLiteral.SourceLength;

                case TOKENID.INVALID:
                    return 1;

                default:
                    return this.Length;
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.StartPosition
        //
        /// <summary>
        /// Return the start position of the range corresponding to the token of this.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal POSDATA StartPosition()
        {
            return (this as POSDATA);
        }

        //------------------------------------------------------------
        // CSTOKEN.StopPosition
        //
        /// <summary>
        /// Return the end position of the range corresponding to the token of this.
        /// this position is not in the range, 
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal POSDATA StopPosition()
        {
            if (TokenID == TOKENID.MLCOMMENT)
            {
                return this.PosEnd;
            }

            if (TokenID == TOKENID.VSLITERAL)
            {
                return this.VSLiteral.PosEnd;
            }

            if (TokenID == TOKENID.DOCCOMMENT || TokenID == TOKENID.MLDOCCOMMENT)
            {
                return this.DocLiteral.PosEnd;
            }

            POSDATA p = new POSDATA((POSDATA)this);
            p.CharIndex += this.SourceLength();
            return p;
        }

        //------------------------------------------------------------
        // CSTOKEN.CopyTokenOverhead
        //------------------------------------------------------------
        //static internal void CopyTokenOverHead(CSTOKEN src, CSTOKEN dst)
        //{
        //    src.CloneOverhead(dst);
        //}

        //------------------------------------------------------------
        // CSTOKEN.CloneOverhead
        //
        /// <summary>
        /// Copy tokenData.
        /// </summary>
        /// <param name="tok"></param>
        //------------------------------------------------------------
        internal void CloneOverhead(CSTOKEN tok)
        {
            // see CLexer::ScanToken to understand what we do here (look for calls to TokenMemAlloc)
            switch (TokenID)
            {
                case TOKENID.STRINGLIT:
                    tok.StringLiteral = this.StringLiteral.Clone();
                    break;

                case TOKENID.VSLITERAL:
                    tok.VSLiteral = this.VSLiteral.Clone();
                    break;

                case TOKENID.MLDOCCOMMENT:
                case TOKENID.DOCCOMMENT:
                    tok.DocLiteral = this.DocLiteral.Clone();
                    break;

                case TOKENID.NUMBER:
                    tok.Literal = this.Literal.Clone();
                    break;

                case TOKENID.MLCOMMENT:
                    tok.PosEnd = new POSDATA(this.PosEnd);
                    break;

                case TOKENID.IDENTIFIER:
                default:
                    tok.tokenData = this.tokenData;
                    break;
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.IsEqual
        //
        /// <summary>
        /// <para>Determine if this is equal to other instance.</para>
        /// <para>Two instances are equal when they have same TokenID,
        /// and same tokenData in some kinds.</para>
        /// </summary>
        /// <param name="comparePositions">Furthermore consider positions if true.</param>
        /// <param name="other">CSTOKEN instance to compare.</param>
        /// <returns>Return true if equal.</returns>
        //------------------------------------------------------------
        internal bool IsEqual(CSTOKEN other, bool comparePositions)
        {
            if (TokenID != other.TokenID)
            {
                return false;
            }

            if (comparePositions)
            {
                if (!this.Equals(other))
                {
                    return false;
                }
                if (this.StopPosition().Equals(other.StopPosition()))
                {
                    return false;
                }
            }

            switch (TokenID)
            {
                case TOKENID.IDENTIFIER:
                    return (this.Name == other.Name);

                case TOKENID.STRINGLIT:
                    if (this.StringLiteral.Str != other.StringLiteral.Str)
                    {
                        return false;
                    }
                    return true;

                case TOKENID.VSLITERAL:
                    return this.VSLiteral.Str == other.VSLiteral.Str;


                case TOKENID.NUMBER:
                    return this.Literal.Text == other.Literal.Text;

                case TOKENID.DOCCOMMENT:
                case TOKENID.MLDOCCOMMENT:
                    return this.DocLiteral.Text == other.DocLiteral.Text;

                case TOKENID.CHARLIT:
                    return this.CharLiteral.Value == other.CharLiteral.Value;

                default:
                    return true;
            }
        }

        //------------------------------------------------------------
        // CSTOKEN.UpdateStartAndEndPositions
        //
        /// <summary>Update the POSDATAs of start and end positions.</summary>
        /// <param name="charDelta">Additional value of column.</param>
        /// <param name="lineDelta">Additional value of row.</param>
        //------------------------------------------------------------
        internal void UpdateStartAndEndPositions(int lineDelta, int charDelta)
        {
            // Update the "end" position if there is one
            POSDATA pEnd = null;
            switch (TokenID)
            {
                case TOKENID.DOCCOMMENT:
                case TOKENID.MLDOCCOMMENT:
                    pEnd = this.DocLiteral.PosEnd;
                    break;

                case TOKENID.MLCOMMENT:
                    pEnd = this.PosEnd;
                    break;

                case TOKENID.VSLITERAL:
                    pEnd = this.VSLiteral.PosEnd;
                    break;

                default:
                    break;
            }

            // Ok, the token actually has an "end" position, update it.
            if (pEnd != null)
            {
                // We only update "iChar"
                // if posEnd is exactly on the same line as the start position
                if (this.LineIndex == pEnd.LineIndex)
                {
                    pEnd.CharIndex += charDelta;
                }
                pEnd.LineIndex += lineDelta;
            }

            // Update the token position. We only do it now because we needed to check
            // the old "iLine" value when updating pPosEnd.
            this.LineIndex += lineDelta;
            this.CharIndex += charDelta;
        }

        //------------------------------------------------------------
        // CSTOKEN.Assign
        //
        /// <summary>
        /// Assignment.
        /// In C#, operator= cannot be overrided.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CSTOKEN Assign(POSDATA pos)
        {
            this.CharIndex = pos.CharIndex;
            this.UserByte = pos.UserByte;
            this.LineIndex = pos.LineIndex;
            this.UserBits = pos.UserBits;
            this.tokenData = null;
            return this;
        }
    }

    //======================================================================
    // TOKENSTREAM
    //======================================================================
    //internal class TOKENSTREAM
    //{
    //    //TOKENSTREAM() { }
    //
    //    internal List<CSTOKEN> TokenList = new List<CSTOKEN>();
    //}

    //======================================================================
    //	LEXDATA
    //
    /// <summary>
    /// <para>
    ///	This structure contains all the data that is accumlated/calculated/produced
    ///	by bringing a source module's token stream up to date.  This includes
    ///	<list type="bullet">
    ///	<item>the token stream (tokens, positions, count),</item>
    ///	<item>the line table (line offsets and count),</item>
    ///	<item>the raw source text,</item>
    ///	<item>the comment table (positions and count),</item>
    ///	<item>the conditional compilation state transitions (line numbers and count),</item>
    ///	<item>etc., etc.</item>
    ///	</list>
    ///	</para>
    ///	<para>Note that some of the fields in this struct will not be filled with data
    ///	unless the appropriate flags were passed to the construction of the
    ///	compiler (controller) object -- for example, the identifier table will not
    ///	be present unless CCF_KEEPIDENTTABLES was given, etc.</para>
    /// <para>All the data pointed to by this structure is owned by the originating
    ///	source module, and is only valid while the calling thread holds a reference
    ///	to the source data object through which it obtained this data!</para>
    /// </summary>
    //======================================================================
    internal class LEXDATA
    {
        //------------------------------------------------------------
        // LEXDATA  Fields and Properties (Source text)
        //------------------------------------------------------------

        /// <summary>
        /// <para>Raw source text pointer
        /// Pointer to the entire provided source text, null-terminated</para>
        /// </summary>
        private string sourceText = null;   // pszSource

        /// <summary>
        /// (R) Get the source text.
        /// </summary>
        internal string SourceText
        {
            get { return sourceText; }
        }

        /// <summary>
        /// (R) The count of the characters in the source text.
        /// if the text is null or empty, return 0.
        /// </summary>
        internal int SourceTextLength
        {
            get { return sourceText != null ? sourceText.Length : 0; }
        }

        /// <summary>
        /// (R) return true if a valid source text is set.
        /// </summary>
        internal bool HasSource
        {
            get { return (this.SourceText != null); }   // bool HasSource() const;
        }

        /// <summary>
        /// Identifier table (CCF_KEEPIDENTTABLES flag required)
        /// </summary>
        internal CIdentDictionary IdentDictionary = new CIdentDictionary(); // CIdentTable *pIdentTable;

        //------------------------------------------------------------
        // LEXDATA  Fields and Properties (Token stream)
        //------------------------------------------------------------

        /// <summary>
        /// Token stream data
        /// </summary>
        protected CSTOKEN[] tokenArray = null; // CSTOKEN *pTokens; // Array of CSTOKEN's

        /// <summary>
        /// Count of tokens in TokenList.
        /// </summary>
        protected int tokenCount = 0;   // long iTokens; // Token count

        /// <summary>
        /// (R) Token stream data
        /// </summary>
        internal CSTOKEN[] TokenArray
        {
            get { return this.tokenArray; }
        }

        /// <summary>
        /// (R) Count of tokens in TokenList.
        /// </summary>
        internal int TokenCount
        {
            get { return tokenCount; }
        }

        //------------------------------------------------------------
        // LEXDATA  Fields and Properties (Lines)
        //------------------------------------------------------------
#if true
        /// <summary>
        /// <para>Line table data
        /// Array of offsets from SourceText of beginning of each line (LineList[0] == 0 always)</para>
        /// </summary>
        private List<int> lineList = new List<int>();   // long *piLines;

        /// <summary>
        /// (R) List of indice of the first characters of each lines.
        /// </summary>
        internal List<int> LineList
        {
            get { return lineList; }
        }

        /// <summary>
        /// (R) Count of lines.
        /// </summary>
        internal int LineCount
        {
            get { return lineList.Count; }  // long iLines; // Line count
        }
#else
        protected int[] lineArray = null;

        protected int lineCount = 0;

        internal int[] LineArray
        {
            get { return this.lineArray; }
        }

        internal int LineCount
        {
            get { return this.lineCount; }
        }
#endif

        //------------------------------------------------------------
        // LEXDATA  Fields and Properties (Conditional compilation)
        //------------------------------------------------------------

        /// <summary>
        /// Conditional compilation data
        /// Array of line numbers indicating conditional inclusion/exclusion transitions
        /// </summary>
        private List<int> transitionLineList = new List<int>(); // long *piTransitionLines;

        internal List<int> TransitionLineList
        {
            get { return transitionLineList; }
        }

        internal int TransitionLineCount
        {
            get { return this.TransitionLineList.Count; }   // long iTransitionLines; // Transition count
        }

        //------------------------------------------------------------
        // LEXDATA  Fields and Properties (Preprocessor directive)
        //------------------------------------------------------------

        /// <summary>
        /// Region table data
        /// Array of line numbers indicating #region directives
        /// </summary>
        private List<int> regionStartList = new List<int>();    // long *piRegionStart;

        /// <summary>
        /// (R) List of #region
        /// </summary>
        internal List<int> RegionStartList
        {
            get { return regionStartList; }
        }

        /// <summary>
        /// Array of line numbers indicating #endregion directives
        /// (CORRESPONDING
        ///  -- these could be nested but are represented here in order of appearance of #region)
        /// </summary>
        private List<int> regionEndList = new List<int>();  // long *piRegionEnd;

        /// <summary>
        /// (R) #endregion 行のリスト
        /// </summary>
        internal List<int> RegionEndList
        {
            get { return regionEndList; }
        }

        /// <summary>
        /// Region count. Count of #region directives.
        /// </summary>
        internal int RegionCount
        {
            get { return regionStartList != null ? regionStartList.Count : 0; } // long iRegions;
        }

        /// <summary>
        /// #pragma warning map
        /// </summary>
        internal CWarningMap WarningMap = new CWarningMap();    // CWarningMap *pWarningMap;

        //------------------------------------------------------------
        // LEXDATA  Constructor
        //
        /// <summary>Do nothing.</summary>
        //------------------------------------------------------------
        internal LEXDATA() { }

        //------------------------------------------------------------
        // LEXDATA.InitTokens (1)
        //
        /// <summary>Set a token list.</summary>
        //------------------------------------------------------------
        internal void InitTokens(CSTOKEN[] tokArray)
        {
            this.tokenArray = tokArray;
            this.tokenCount = this.tokenArray.Length;
        }

        //------------------------------------------------------------
        // LEXDATA.InitTokens (2)
        //
        /// <summary>Set a token list.</summary>
        //------------------------------------------------------------
        internal void InitTokens(List<CSTOKEN> tokList)
        {
            if (tokList == null)
            {
                return;
            }

            int count = tokList.Count;
            this.tokenArray = new CSTOKEN[count];
            tokList.CopyTo(this.tokenArray);
            this.tokenCount = this.tokenArray.Length;
        }

        //------------------------------------------------------------
        // LEXDATA.ClearTokens
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearTokens()
        {
            this.tokenArray = null;
            this.tokenCount = 0;
        }

        //------------------------------------------------------------
        // LEXDATA.ClearLineOffsets
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearLineOffsets()
        {
            this.lineList.Clear();
        }

        //------------------------------------------------------------
        // LEXDATA.UnsafeExposeTokens
        //
        /// <summary>
        /// Return tokenList.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal List<CSTOKEN> UnsafeExposeTokens()
        //{
        //    return this.TokenList;
        //}

        //------------------------------------------------------------
        // LEXDATA.TokenAt
        //
        /// <summary>
        /// Return the CSTOKEN instance at tokenIndex.
        /// if tokenIndex is invalid, return null.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CSTOKEN TokenAt(int tokenIndex)
        {
            if (this.tokenArray == null)
            {
                return null;
            }

            try
            {
                return this.tokenArray[tokenIndex];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // LEXDATA.InitLineOffsets (1)
        //
        /// <summary>
        /// Set lineList.
        /// </summary>
        /// <param name="list"></param>
        //------------------------------------------------------------
        //internal void InitLineOffsets(int[] arr)
        //{
        //    this.lineArray = arr;
        //    this.lineCount = this.lineArray.Length;
        //}

        //------------------------------------------------------------
        // LEXDATA.InitLineOffsets (2)
        //
        /// <summary>
        /// Set lineList.
        /// </summary>
        /// <param name="list"></param>
        //------------------------------------------------------------
        internal void InitLineOffsets(List<int> list)
        {
            if (list == null)
            {
                return;
            }
            this.lineList = list;
        }

        //------------------------------------------------------------
        // LEXDATA.UnsafeExposeLineOffsets
        //
        /// <summary>
        /// Return lineList.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal List<int> UnsafeExposeLineOffsets()
        //{
        //    return this.LineList;
        //}

        //------------------------------------------------------------
        // LEXDATA.LineOffsetAt
        //
        /// <summary>
        /// Return the first character index of the specified line.
        /// </summary>
        /// <param name="lineIndex">Index of line.</param>
        /// <returns>The first character index.</returns>
        //------------------------------------------------------------
        internal int LineOffsetAt(int lineIndex)
        {
            if (lineList == null)
            {
                return -1;
            }

            try
            {
                return this.lineList[lineIndex];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return -1;
        }

        //------------------------------------------------------------
        // LEXDATA.TransitionLineAt
        //
        /// <summary></summary>
        /// <param name="trIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int TransitionLineAt(int trIndex)
        {
            try
            {
                return this.transitionLineList[trIndex];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return -1;
        }

        //------------------------------------------------------------
        // LEXDATA.InitTransition
        //
        /// <summary></summary>
        /// <param name="trList"></param>
        //------------------------------------------------------------
        internal void InitTransition(List<int> trList)
        {
            this.transitionLineList = trList;
        }

        //------------------------------------------------------------
        // LEXDATA.RegionStartAt
        //
        /// <summary>
        /// Return the line index of the specified #Region.
        /// </summary>
        //------------------------------------------------------------
        internal int RegionStartAt(int regionIndex)
        {
            try
            {
                return this.regionStartList[regionIndex];
            }
            catch (System.IndexOutOfRangeException)
            {
            }
            return -1;
        }

        //------------------------------------------------------------
        // LEXDATA.RegionEndAt
        //
        /// <summary>
        /// Return the line index of the specified #endregion.
        /// </summary>
        /// <param name="regionIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int RegionEndAt(int regionIndex)
        {
            try
            {
                return this.regionEndList[regionIndex];
            }
            catch (System.IndexOutOfRangeException)
            {
            }
            return -1;
        }

        //------------------------------------------------------------
        // LEXDATA.ClearRegions
        //
        /// <summary>
        /// Clear all #Region directives.
        /// </summary>
        //------------------------------------------------------------
        internal void ClearRegions()
        {
            this.regionStartList.Clear();
            this.regionEndList.Clear();
        }

        //------------------------------------------------------------
        // LEXDATA.InitRegion
        //
        /// <summary>
        /// Set sr to #region list and er to #endregion list.
        /// </summary>
        /// <param name="startList"></param>
        /// <param name="endList"></param>
        //------------------------------------------------------------
        internal void InitRegion(List<int> startList, List<int> endList)
        {
            this.regionStartList = startList;
            this.regionEndList = endList;
        }

        //------------------------------------------------------------
        // LEXDATA.InitSource
        //
        /// <summary>
        /// Set a source text.
        /// </summary>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal void InitSource(string src)
        {
            this.sourceText = src;
        }

        //------------------------------------------------------------
        // LEXDATA.UnsafeExposeSource
        //
        /// <summary>
        /// Return a source text.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string UnsafeExposeSource()
        {
            return this.sourceText;
        }

        //------------------------------------------------------------
        // LEXDATA.TextAt
        //
        /// <summary>
        /// Return the index in the source text of the character
        /// whose row and column are lineIndex and charIndex respectively.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int TextAt(int lineIndex, int charIndex)
        {
            if (this.sourceText == null)
            {
                return -1;
            }

            int offset = LineOffsetAt(lineIndex);
            if (offset < 0)
            {
                return -1;
            }
            return offset + charIndex;
        }

        //------------------------------------------------------------
        // LEXDATA.TextAt
        //
        /// <summary>
        /// Return the index in the source text of the character
        /// whose position is specified by POSTDATA pos.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int TextAt(POSDATA pos)
        {
            return TextAt(pos.LineIndex, pos.CharIndex);
        }

        //------------------------------------------------------------
        // LEXDATA.ExtractText
        //
        /// <summary>
        /// Return the string of line at iLine.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ExtractText(int lineIndex)
        {
            return ExtractText(new POSDATA(lineIndex, 0));
        }

        //------------------------------------------------------------
        // LEXDATA.ExtractText
        //
        /// <summary>
        /// Return the string of line where posStart is in.
        /// </summary>
        /// <param name="posStart"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ExtractText(POSDATA posStart)
        {
            POSDATA posEnd = new POSDATA(
                posStart.LineIndex,
                this.GetLineLength(posStart.LineIndex));
            return ExtractText(posStart, posEnd);
        }

        //------------------------------------------------------------
        // LEXDATA.ExtractText
        //
        /// <summary>
        /// Return the substring from posStart to posEnd. (posEnd is not included.)
        /// </summary>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ExtractText(POSDATA posStart, POSDATA posEnd)
        {
            if (posStart > posEnd)
            {
                //VSFAIL("Bad positions");
                return null;
            }

            if (TokenCount == 0)
            {
                return null;
            }

            if (posStart < new POSDATA(0, 0))
            {
                //VSFAIL("Bad start position");
                return null;
            }

            if (posEnd > TokenAt(TokenCount - 1).StopPosition())
            {
                //VSFAIL("Bad end position");
                return null;
            }

            int pos1 = TextAt(posStart);
            int pos2 = TextAt(posEnd);
            if (pos1 > pos2)
            {
                int pos3 = pos1;
                pos1 = pos2;
                pos2 = pos3;
            }
            string str;
            try
            {
                str = sourceText.Substring(pos1, pos2 - pos1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
            return str;
        }

        //------------------------------------------------------------
        // LEXDATA.CharAt
        //
        /// <summary>
        /// Return the character at (lineIndex, charIndex).
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char CharAt(int lineIndex, int charIndex)
        {
            if (this.sourceText == null)
            {
                return '\0';
            }

            char c;
            try
            {
                c = sourceText[TextAt(lineIndex, charIndex)];
            }
            catch (IndexOutOfRangeException)
            {
                return '\0';
            }
            return c;
        }

        //------------------------------------------------------------
        // LEXDATA.CharAt
        //
        /// <summary>
        /// Return the character at pos.
        /// </summary>
        //------------------------------------------------------------
        internal char CharAt(POSDATA pos)
        {
            return CharAt(pos.LineIndex, pos.CharIndex);
        }

        //------------------------------------------------------------
        // LEXDATA.FindFirstTokenOnLine
        //
        /// <summary>
        /// <para>if lineIndex is valid and the specified line has tokens,
        /// set the index of the first token in the line to firstIndex and return true.</para>
        /// <para>Otherwise, return false.</para>
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="firstIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindFirstTokenOnLine(int lineIndex, out int firstIndex)
        {
            firstIndex = -1;
            int tokCount = this.TokenCount;
            if (tokCount == 0)
            {
                return false;
            }
            if (lineIndex < 0 || lineIndex >= LineCount)
            {
                return false;
            }

            POSDATA pos = new POSDATA(lineIndex, 0);
            int nearestIndex = this.FindNearestPosition(pos);
            int iIndex = -1;

            // nearestIndex will either be/start on lineIndex, or a line < lineIndex.
            // If it is on lineIndex then we're done
            // (there's a token a column zero, which is actually very rare...)

            if (nearestIndex >= 0 && TokenAt(nearestIndex).LineIndex == lineIndex)
            {
                // Boom -- we're done.
                firstIndex = nearestIndex;
                return true;
            }
            else
            {
                // Look at the next token -- if it starts on lineIndex, we're golden.
                // tokenList is not emply.
                if (nearestIndex + 1 < tokCount &&
                    TokenAt(nearestIndex + 1).LineIndex == lineIndex)
                {
                    firstIndex = nearestIndex + 1;
                    return true;
                }
                else
                {
                    // No dice -- there are no tokens on this line.
                    iIndex = nearestIndex;
                    return false;
                }
            }
        }

        //------------------------------------------------------------
        // LEXDATA.FindLastTokenOnLine
        //
        /// <summary>
        /// <para>if lineIndex is valid and the specified line has tokens,
        /// set the index of the last token in the line to lastIndex and return true.</para>
        /// <para>Otherwise, return false.</para>
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="lastIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindLastTokenOnLine(int lineIndex, out int lastIndex)
        {
            lastIndex = -1;
            int tokCount = this.TokenCount;
            if (tokCount == 0)
            {
                return false;
            }
            if (lineIndex < 0 || lineIndex >= LineCount)
            {
                return false;
            }

            if (lineIndex < 0 || lineIndex >= this.LineCount)
            {
                return false;
            }

            int tok = -1;
            int tokFirst = -1;

            if (!FindFirstTokenOnLine(lineIndex, out tokFirst))
            {
                return false;
            }

            for (tok = tokFirst + 1; tok < tokCount; tok++)
            {
                if (TokenAt(tok).LineIndex > lineIndex) break;
            }
            lastIndex = tok - 1;
            return true;
        }

        //------------------------------------------------------------
        // LEXDATA.IsFirstTokenOnLine
        //
        /// <summary>
        /// Determine that the specified token is the first token in the line where the token is in.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <param name="skipNoisyTokens">If true, ignore comments and undefined tokens and determine.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsFirstTokenOnLine(int tokenIndex, bool skipNoisyTokens)
        {
            if (this.tokenCount == 0)
            {
                return false;
            }

            int prevIndex = tokenIndex;
            do
            {
                prevIndex--;
            }
            while (skipNoisyTokens && (prevIndex >= 0) && IsNoisyToken(prevIndex));

            return (
                prevIndex < 0) ||
                (TokenAt(tokenIndex).LineIndex > TokenAt(prevIndex).LineIndex);
        }

        //------------------------------------------------------------
        // LEXDATA.IsFirstTokenOnLine
        //
        /// <summary>
        /// <para>Determine that the specified token is the first token in the line where the token is in,
        /// including comments and undefined tokens.</para>
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsFirstTokenOnLine(int tokenIndex)
        {
            return IsFirstTokenOnLine(tokenIndex, false);	// skipNosiyTokens
        }

        //------------------------------------------------------------
        // LEXDATA.IsLastTokenOnLine
        //
        /// <summary>
        /// Determine that the specified token is the last token in the line where the token is in.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <param name="skipNoisyTokens">If true, ignore comments and undefined tokens and determine.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsLastTokenOnLine(int tokenIndex, bool skipNoisyTokens)
        {
            if (this.tokenCount == 0)
            {
                return false;
            }

            int nextIndex = tokenIndex;
            do
            {
                nextIndex++;
            }
            while (skipNoisyTokens && (nextIndex < this.tokenCount) && IsNoisyToken(nextIndex));

            // The token just before the ENDFILE token should be considered the last one on the line.
            return (
                (nextIndex >= this.tokenCount - 1) ||
                (TokenAt(nextIndex).StopPosition().LineIndex > TokenAt(tokenIndex).LineIndex));
        }

        //------------------------------------------------------------
        // LEXDATA.IsLastTokenOnLine
        //
        /// <summary>
        /// <para>Determine that the specified token is the last token in the line where the token is in,
        /// including comments and undefined tokens.</para>
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsLastTokenOnLine(int tokenIndex)
        {
            return IsLastTokenOnLine(tokenIndex, false);	// skipNosiyTokens
        }

        //------------------------------------------------------------
        // LEXDATA.FindFirstNonWhiteChar
        //
        /// <summary>
        /// Return the index of the first non white space character in the specified line.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="nwCharIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindFirstNonWhiteChar(int lineIndex, ref int nwCharIndex)
        {
            if (this.tokenCount == 0)
            {
                return false;
            }

            if (lineIndex < 0 || lineIndex >= this.LineCount)
            {
                return false;   // (int)COM.HRESULT.E_INVALIDARG;
            }
            int iChar = 0;

            int pos;
            for (pos = this.TextAt(lineIndex, 0);
                CharUtil.IsWhitespaceChar(sourceText[pos]);
                pos++)
            {
                iChar++;
            }

            nwCharIndex = iChar;
            //return (int)((Util.IsEndOfLineChar(sourceText[pos]) || sourceText[pos] == 0) ?
            // COM.HRESULT.S_FALSE : COM.HRESULT.S_OK);
            return (!CharUtil.IsEndOfLineChar(sourceText[pos]) && pos < sourceText.Length);
        }

        //------------------------------------------------------------
        // LEXDATA.FindEndOfComment
        //
        /// <summary>
        /// Find the end position of the comment whose token index is commentTokenIndex.
        /// If found, set it to posEnd and return true. Otherwise, return false.
        /// </summary>
        /// <param name="commentTokenIndex"></param>
        /// <param name="posEnd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindEndOfComment(int commentTokenIndex, out POSDATA posEnd)
        {
            posEnd = null;
            if (this.tokenCount == 0)
            {
                return false;
            }

            if (commentTokenIndex < 0 || commentTokenIndex >= this.tokenCount)
            {
                return false;
            }

            posEnd = TokenAt(commentTokenIndex).StopPosition();
            return ((TokenAt(commentTokenIndex).UserBits & (int)TOKFLAGS.UNTERMINATED) == 0);
        }

        //------------------------------------------------------------
        // LEXDATA.FindFirstPrecedingNonWhiteChar
        //
        /// <summary>
        /// Find the last position of non white space characters before the position specified by pos.
        /// If found, set it to posNonWhite and return true. Otherwise, return false.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="posNonWhite"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindFirstPrecedingNonWhiteChar(POSDATA pos, out POSDATA posNonWhite)
        {
            posNonWhite = new POSDATA();
            if (sourceText == null || sourceText.Length == 0)
            {
                return false;
            }

            if (pos > new POSDATA(LineCount - 1, sourceText.Length - TextAt(LineCount - 1, 0)))
            {
                return false;
            }

            int lineIndex = pos.LineIndex;
            int charIndex = pos.CharIndex;
            int lineStartIndex = this.TextAt(lineIndex, 0);

            while (lineIndex > 0 || charIndex > 0)
            {
                charIndex--;
                while (charIndex < 0)
                {
                    if (lineIndex == 0)
                    {
                        posNonWhite.LineIndex = 0;
                        posNonWhite.CharIndex = 0;
                        return false;
                    }
                    lineIndex--;
                    lineStartIndex = this.TextAt(lineIndex, 0);
                    charIndex = this.GetLineLength(lineIndex) - 1;
                }

                if (!CharUtil.IsWhitespaceChar(sourceText[lineStartIndex + charIndex]))
                {
                    posNonWhite.LineIndex = lineIndex;
                    posNonWhite.CharIndex = charIndex;
                    return true;
                }
            }
            // Only way to get here is to call w/ POSDATA(0,0)...
            posNonWhite.LineIndex = posNonWhite.CharIndex = 0;
            return false;
        }

        //------------------------------------------------------------
        // LEXDATA.GetPreprocessorDirective
        //
        /// <summary>
        /// If the given line is a preprocessor line, this function returns the PPTOKENID
        /// of the directive (i.e. PPT_DEFINE, etc.).  If not, this will return E_FAIL.
        /// </summary>
        /// <param name="lineIndex">Index of line.</param>
        /// <param name="nameManager">CNameManager instance.</param>
        /// <param name="preprocTokenId">Set a preprocessor token ID.</param>
        /// <param name="startIndex">The character (or column) index of '#'.</param>
        /// <param name="endIndex">The character (or column) index of the end of directive.</param>
        /// <returns>If succeed, return true.</returns>
        //------------------------------------------------------------
        internal bool GetPreprocessorDirective(
            int lineIndex,
            CNameManager nameManager,
            out PPTOKENID preprocTokenId,
            out int startIndex,
            out int endIndex)
        {
            preprocTokenId = PPTOKENID.UNDEFINED;
            startIndex = endIndex = -1;

            if (lineIndex < 0 || lineIndex >= this.LineCount)
            {
                return false;
            }

            // Store the head of the currrent line to posLine.
            int pos = this.TextAt(lineIndex, 0);
            int posLine = pos;

            // Skip whitespace...
            while (CharUtil.IsWhitespaceChar(SourceText[pos]))
            {
                pos++;
            }
            startIndex = pos - posLine;

            // Check for '#' -- if not there, fail
            if (SourceText[pos++] != '#')
            {
                return false;
            }

            // Skip more whitespace...
            while (CharUtil.IsWhitespaceChar(SourceText[pos]))
            {
                pos++;
            }

            // Scan an identifier
            if (!CharUtil.IsIdentifierChar(SourceText[pos])) return false;
            int posStart = pos; // start of idntifier
            while (CharUtil.IsIdentifierCharOrDigit(SourceText[pos]))
            {
                pos++;
            }
            endIndex = pos - posLine;   // end of identifier

            string ppName = null;
            bool br = false;
            int iToken = 0;

            // Make a name out of it and see if it's a preprocessor keyword
            if (nameManager.AddLen(ppName, posStart, pos - posStart) &&
                nameManager.GetPreprocessorDirective(ppName, out iToken))
            {
                preprocTokenId = (PPTOKENID)iToken;
                br = true;
            }

            return br;
        }

        //------------------------------------------------------------
        // LEXDATA.IsPreprocessorLine
        //
        /// <summary>
        /// Determine if the specified line is a preprocessor line.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsPreprocessorLine(int lineIndex, CNameManager nameManager)
        {
            PPTOKENID tokenId;
            int start, end;
            return GetPreprocessorDirective(lineIndex, nameManager, out tokenId, out start, out end);
        }

        //------------------------------------------------------------
        // LEXDATA.SpanContainsPreprocessorDirective
        //
        /// <summary>
        /// Determin if a preprocessor line is in a specified range of lines.
        /// </summary>
        /// <param name="startLineIndex">The index of the first line of a line range.</param>
        /// <param name="endLineIndex">The index of the next line following a line range.</param>
        /// <param name="nameManager">CNameManager instance.</param>
        /// <returns>If contains, return true.</returns>
        //------------------------------------------------------------
        internal bool SpanContainsPreprocessorDirective(
            int startLineIndex,
            int endLineIndex,
            CNameManager nameManager)
        {
            if (startLineIndex < 0 || endLineIndex >= LineCount)
            {
                DebugUtil.Assert(false);
                return false;
            }

            if (startLineIndex > endLineIndex)
            {
                DebugUtil.Assert(false);
                return false;
            }

            for (int iLine = startLineIndex; iLine <= endLineIndex; ++iLine)
            {
                PPTOKENID preprocId;
                int start, end;
                if (GetPreprocessorDirective(iLine, nameManager, out preprocId, out start, out end))
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // LEXDATA.GetLineLength
        //
        /// <summary>Return the length of a line.</summary>
        //------------------------------------------------------------
        internal int GetLineLength(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= this.LineCount)
            {
                return 0;
            }

            // the last line.
            if (lineIndex == this.LineCount - 1)
            {
                return (sourceText.Length - this.TextAt(lineIndex, 0));
            }

            // Otherwise.
            int pos = this.TextAt(lineIndex + 1, 0) - 1;
            int posThis = this.TextAt(lineIndex, 0);
            while (pos > posThis && CharUtil.IsEndOfLineChar(sourceText[pos - 1]))
            {
                pos--;
            }
            return (pos - posThis);
        }

        //------------------------------------------------------------
        // LEXDATA.IsWhitespace
        //
        /// <summary>
        /// Return true if all characters in the specified range are white spaces.
        /// </summary>
        /// <param name="startLineIndex">The line index of the start position of a range.</param>
        /// <param name="startCharIndex">The character (or column) index of the start position of a range.</param>
        /// <param name="endLineIndex">The line index of the end position of a range.</param>
        /// <param name="endCharIndex">The character (or column) index of the start position of a range.
        /// This character is not included in the range.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsWhitespace(
            int startLineIndex,
            int startCharIndex,
            int endLineIndex,
            int endCharIndex)
        {
            if (startLineIndex < 0 ||
                endLineIndex >= this.LineCount ||
                startLineIndex > endLineIndex)
            {
                return false;
            }

            int pos = this.TextAt(startLineIndex, startCharIndex);
            int posStop = this.TextAt(endLineIndex, endCharIndex);
            char c;

            while (pos < posStop)
            {
                c = sourceText[pos];
                if (!CharUtil.IsWhitespaceChar(c) && !CharUtil.IsEndOfLineChar(c))
                    return false;
                pos++;
            }
            return true;
        }

        //------------------------------------------------------------
        // LEXDATA.IsLineWhitespaceBefore
        //
        /// <summary>
        /// Return true if the specified character is the first non white spece character in the line.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsLineWhitespaceBefore(int lineIndex, int charIndex)
        {
            return IsWhitespace(lineIndex, 0, lineIndex, charIndex);
        }

        //------------------------------------------------------------
        // LEXDATA.IsLineWhitespaceBefore
        //
        /// <summary>
        /// Return true if the specified character is the first non white spece character in the line.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsLineWhitespaceBefore(POSDATA pos)
        {
            return IsWhitespace(pos.LineIndex, 0, pos.LineIndex, pos.CharIndex);
        }

        //------------------------------------------------------------
        // LEXDATA.IsLineWhitespaceAfter
        //
        /// <summary>
        /// Return true if the specified character is the last non white spece character in the line.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsLineWhitespaceAfter(int lineIndex, int charIndex)
        {
            return IsWhitespace(lineIndex, charIndex, lineIndex, GetLineLength(lineIndex));
        }

        //------------------------------------------------------------
        // LEXDATA.IsLineWhitespaceAfter
        //
        /// <summary>
        /// Return true if the specified character is the last non white spece character in the line.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsLineWhitespaceAfter(POSDATA pos)
        {
            return IsWhitespace(
                pos.LineIndex,
                pos.CharIndex,
                pos.LineIndex,
                GetLineLength(pos.LineIndex));
        }

        //------------------------------------------------------------
        // LEXDATA.IsInsideString
        //
        /// <summary>
        /// Return true if the specifiec position is in a literal.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsInsideString(POSDATA pos)
        {
            // Find the nearest token
            int i = this.FindNearestPosition(pos);
            if (i < 0)
            {
                return false;
            }

            CSTOKEN token = this.TokenAt(i);
            // The first character of a literal is quotation.
            // Therefore, if pos is at the first character of literal,
            // pos is not in literal.
            if (token.Equals(pos) ||
               (token.TokenID != TOKENID.STRINGLIT &&
                token.TokenID != TOKENID.VSLITERAL))
            {
                // We're either not in the string token, or it's not a string token
                return false;
            }
            else
            {
                if (token.IsUnterminated)
                {
                    return (token.StopPosition() >= pos);
                }
                else
                {
                    return (token.StopPosition() > pos);
                }
            }
        }

        //------------------------------------------------------------
        // LEXDATA.IsInsideSkippedPreProcessorRegion
        //
        /// <summary>
        /// Return true if the specified position is between #region and #endregion.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsInsideSkippedPreProcessorRegion(POSDATA pos)
        {
            bool inside = false;
            // The array is sorted.  We could do a binary search, but the count should be small!
            int lineIndex = pos.LineIndex;
            for (int i = 0; i < this.TransitionLineCount; i++)
            {
                if (this.TransitionLineAt(i) > lineIndex) break;
                inside = !inside;
            }
            return inside;
        }

        //------------------------------------------------------------
        // LEXDATA.GetAbsolutePosition
        //
        /// <summary>
        /// Return the index of the first character of specified token in the source text
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetAbsolutePosition(int tokenIndex)
        {
            return TextAt(TokenAt(tokenIndex));
        }

        //------------------------------------------------------------
        // LEXDATA.FindNearestPosition
        //
        /// <summary>
        /// <para>Given a single position,
        /// this function finds that position in the array using a binary search and returns its index.
        /// If the position doesn't exist in the token stream,</para>
        /// <para>this function returns the index of last entry PRIOR to the given position
        /// (obviously assuming that the array is sorted).</para>
        /// <para>The return value can thus be -1
        /// (meaning all entries in the array are beyond the given position).</para>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>Token index.</returns>
        //------------------------------------------------------------
        internal int FindNearestPosition(POSDATA pos)
        {
            if (this.tokenCount == 0)
            {
                return -1;
            }

            int minIndex = 0;
            int maxIndex = this.TokenCount - 1;
            int midIndex = maxIndex >> 1;
            int c;

            // Zero in on a token near pos
            while (maxIndex - minIndex >= 3)
            {
                c = this.TokenAt(midIndex).Compare(pos);

                if (c == 0)
                {
                    return midIndex;    // Wham!  exact match
                }
                if (c > 0)  // pos < mid
                {
                    maxIndex = midIndex - 1;
                }
                else // mid < pos
                {
                    minIndex = midIndex + 1;
                }
                midIndex = minIndex + ((maxIndex - minIndex) >> 1);
            }

            // Last-ditch -- check from minIndex to maxIndex for a match, or closest to.
            for (midIndex = minIndex; midIndex <= maxIndex; ++midIndex)
            {
                c = this.TokenAt(midIndex).Compare(pos);
                if (c == 0)
                {
                    if (this.TokenAt(midIndex).TokenID == TOKENID.ENDFILE)
                    {
                        return midIndex - 1;
                    }
                    return midIndex;
                }
                else if (c > 0)
                {
                    return midIndex - 1;
                }
            }

            // This is it!
            return midIndex - 1;
        }

        //------------------------------------------------------------
        // LEXDATA.CloneTokenStream
        //
        /// <summary>
        /// Copy the tokens in tokenList to destination.
        /// </summary>
        //------------------------------------------------------------
        internal bool CloneTokenStream(int first, int count, out CSTOKEN[] dest)
        {
            dest = null;
            DebugUtil.Assert(first >= 0 && count >= 0);
            if (count == 0)
            {
                dest = new CSTOKEN[0];
                return true;
            }
            if (first >= this.tokenCount)
            {
                return false;
            }

            if (first + count > this.tokenCount)
            {
                count = this.tokenCount - first;
            }
            dest = new CSTOKEN[count];
            Array.Copy(this.tokenArray, first, dest, 0, count);
            return true;
        }

        //------------------------------------------------------------
        // LEXDATA.FreeClonedTokenStream
        //
        /// <summary>
        /// Clear all tokens of tokenStream.TokenList.
        /// </summary>
        /// <param name="tokenStream"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal bool FreeClonedTokenStream(TOKENSTREAM tokenStream)
        //{
        //    tokenStream.TokenList.Clear();
        //    return true;
        //}

        //------------------------------------------------------------
        // LEXDATA.FindPositionOfText
        //
        /// <summary>
        /// Create POSDATA instance corresponding to textIndex of source text.
        /// </summary>
        /// <param name="posData">Set the created POSDATA instance.</param>
        /// <param name="textIndex">Index in source text.</param>
        /// <returns>Return true if POSDATA is normally created.</returns>
        //------------------------------------------------------------
        internal bool FindPositionOfText(int textIndex, out POSDATA posData)
        {
            posData = null;
            if (sourceText == null)
            {
                DebugUtil.Assert(false);
                return false;
            }

            for (int line = this.LineCount - 1; line >= 0; line--)
            {
                //Skip this line if it comes after the text we're looking for
                if (TextAt(line, 0) > textIndex) continue;
                posData = new POSDATA(line, (textIndex - TextAt(line, 0)));
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // LEXDATA.TokenTouchesLine
        //
        /// <summary>
        /// Determine if the specified token has a intersection with the specified line.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool TokenTouchesLine(int tokenIndex, int lineIndex)
        {
            POSDATA posStart = this.TokenAt(tokenIndex);
            POSDATA posEnd = this.TokenAt(tokenIndex).StopPosition();
            return (posStart.LineIndex <= lineIndex && lineIndex <= posEnd.LineIndex);
        }

        //------------------------------------------------------------
        // LEXDATA.IsNoisyToken
        //
        /// <summary>
        /// Return true if the specified token is comment or unknown token.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsNoisyToken(int tokenIndex)
        {
            return TokenAt(tokenIndex).IsNoisy;
        }

        //------------------------------------------------------------
        // LEXDATA.NextNonNoisyToken
        //
        /// <summary>
        /// Find the next token which is not noisy after specified token.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int NextNonNoisyToken(int tokenIndex)
        {
            DebugUtil.Assert(tokenIndex >= 0 && tokenIndex < this.tokenCount);

            while (tokenIndex < this.tokenCount && TokenAt(tokenIndex).IsComment)
            {
                ++tokenIndex;
            }
            return (tokenIndex < this.tokenCount - 1 ? tokenIndex : this.tokenCount - 1);
        }

        //------------------------------------------------------------
        // LEXDATA.SkipNoisyTokens
        //
        /// <summary>
        /// Skip comment tokens and unknown tokens.
        /// </summary>
        /// <param name="increment">if 1, advance, or if -1, retreat.</param>
        /// <param name="tokenIndex">Index of starting token.</param>
        /// <returns>
        /// Return token index if found.
        /// Otherwise, return the count of token or -1.
        /// </returns>
        //------------------------------------------------------------
        internal int SkipNoisyTokens(int tokenIndex, int increment)
        {
            DebugUtil.Assert(increment == 1 || increment == -1);
            DebugUtil.Assert(tokenIndex >= 0);
            DebugUtil.Assert(tokenIndex < this.tokenCount);

            if (increment == 1)
            {
                int i;
                for (i = tokenIndex; i < this.tokenCount - 1; i++)
                {
                    if (!TokenAt(i).IsComment) break;
                }
                return i;
            }
            else if (increment == -1)
            {
                int i;
                for (i = tokenIndex; i >= 0; i--)
                {
                    if (!TokenAt(i).IsComment) break;
                }
                return i;
            }
            else
            {
                return tokenIndex;
            }
        }

        //------------------------------------------------------------
        // LEXDATA.PeekTokenIndexFrom
        //
        /// <summary></summary>
        /// <param name="iCur"></param>
        /// <param name="iPeek"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int PeekTokenIndexFrom(int iCur, int iPeek)
        {
            return PeekTokenIndexFromInternal(this.tokenArray, iCur, iPeek);
        }

        //------------------------------------------------------------
        // LEXDATA.PeekTokenIndexFromInternal
        //
        /// <summary></summary>
        /// <param name="tokenList"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="peekCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static int PeekTokenIndexFromInternal(
            CSTOKEN[] tokArray,
            int tokenIndex,
            int peekCount)
        {
            if (tokArray == null)
            {
                return -1;
            }
            int iTokens = tokArray.Length;
            int ie = iTokens - 1;

            if (peekCount > 0)
            {
                int i;
                for (i = tokenIndex + 1; i < ie; i++)
                {
                    if (!tokArray[i].IsComment)
                    {
                        if (--peekCount == 0) break;
                    }
                }
                return (i < ie ? i : ie);
            }

            if (peekCount < 0)
            {
                int i;
                for (i = tokenIndex - 1; i >= 0; i--)
                {
                    if (!tokArray[i].IsComment)
                    {
                        if (++peekCount == 0) break;
                    }
                }
                return (i > 0 ? i : 0);
            }
            else
            {
                return tokenIndex;
            }
        }

        //------------------------------------------------------------
        // LEXDATA.CloneTo
        //
        /// <summary>Copy data to lexClone.</summary>
        /// <param name="lexClone"></param>
        //------------------------------------------------------------
        internal void CloneTo(LEXDATA lexClone)
        {
            if (lexClone == null) return;

            if (this.tokenArray == null)
            {
                lexClone.tokenArray = null;
            }
            else
            {
                lexClone.tokenArray = new CSTOKEN[this.TokenCount];
                this.tokenArray.CopyTo(lexClone.tokenArray, this.TokenCount);
            }

            if (this.lineList == null)
            {
                lexClone.lineList = null;
            }
            else
            {
                lexClone.lineList = new List<int>(this.LineList);
            }

            lexClone.transitionLineList = new List<int>(this.TransitionLineList);
            lexClone.regionStartList = new List<int>(this.RegionStartList);
            lexClone.regionEndList = new List<int>(this.RegionEndList);

            lexClone.sourceText = this.SourceText;

            Dictionary<string, string> idClone;
            this.IdentDictionary.DuplicateDictionary(out idClone);
            lexClone.IdentDictionary.SetDictionary(idClone);

            lexClone.WarningMap.Copy(this.WarningMap);
        }

        // Member for debug
#if DEBUG

        //------------------------------------------------------------
        // LEXDATA.DebugTokenList
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal string DebugTokenList()
        internal void DebugTokenList(StringBuilder sb)
        {
            //StringBuilder sb = new StringBuilder();

            for (int i = 0; i < this.tokenCount; ++i)
            {
                //sb.AppendFormat("({0})\r\n{1}\r\n", i, tokenArray[i].DebugString());
                sb.AppendFormat("({0})\r\n", i);
                tokenArray[i].DebugString(sb);
                sb.Append("\r\n");
            }
            //return sb.ToString();
        }
#endif
    }

    //	struct LEXDATA
    //	{
    //	    // The incremental tokenizer is really partying on with the token stream array
    //	    friend class CSourceModuleBase;
    //	    friend class CSourceModule;
    //	    friend class CSourceModuleClone;
    //	
    //	protected:
    //	    // Token stream data
    //	    CSTOKEN         *pTokens;               // Array of CSTOKEN's 
    //	    long            iTokens;                // Token count
    //	
    //	    // Line table data
    //	    long            *piLines;               // Array of offsets from pszSource of beginning of each line (piLines[0] == 0 always)
    //	    long            iLines;                 // Line count
    //	
    //	    // Conditional compilation data
    //	    long            *piTransitionLines;     // Array of line numbers indicating conditional inclusion/exclusion transitions
    //	    long            iTransitionLines;       // Transition count
    //	
    //	    // Region table data
    //	    long            *piRegionStart;         // Array of line numbers indicating #region directives
    //	    long            *piRegionEnd;           // Array of line numbers indicating #endregion directives (CORRESPONDING -- these could be nested but are represented here in order of appearance of #region)
    //	    long            iRegions;               // Region count
    //	
    //	    // Raw source text pointer
    //	    PCWSTR           pszSource;             // Pointer to the entire provided source text, null-terminated
    //	
    //	internal:
    //	
    //	    // Identifier table                     (CCF_KEEPIDENTTABLES flag required)
    //	    CIdentTable     *pIdentTable;           // Identifier table
    //	
    //	    // #pragma warning map
    //	    CWarningMap     *pWarningMap;
    //	
    //	internal:
    //	    LEXDATA();
    //	
    //	    void            InitTokens (CSTOKEN *pTok, long iTok);
    //	    void            UnsafeExposeTokens(CSTOKEN **ppTokens, long *piTokens) const;
    //	    long            TokenCount() const;
    //	    CSTOKEN &       TokenAt (int iTok) const;
    //	
    //	    void            InitLineOffsets(long *piLines, long iLines);
    //	    void            UnsafeExposeLineOffsets(long **ppLines, long *piLines) const;
    //	    long            LineCount() const;
    //	    long            LineOffsetAt(long iLineOffset) const;
    //	
    //	    long            TransitionLineCount() const;
    //	    long            TransitionLineAt(long iTransitionLine) const;
    //	
    //	    long            RegionCount() const;
    //	    long            RegionStartAt(long iRegion) const;
    //	    long            RegionEndAt(long iRegion) const;
    //	
    //	    void            InitSource(PCWSTR pszSource);
    //	    bool            HasSource() const;
    //	    void            UnsafeExposeSource(PCWSTR *ppszSource) const;
    //	
    //	
    //	    PCWSTR          TextAt (long iLine, long iChar)  const;
    //	    PCWSTR          TextAt (const POSDATA& pos)  const;
    //	    HRESULT         ExtractText(long iLine, BSTR *pbstrText) const;
    //	    HRESULT         ExtractText(const POSDATA& posStart, BSTR *pbstrText) const;
    //	    HRESULT         ExtractText(const POSDATA &posStart, const POSDATA &posEnd, BSTR *pbstrText) const;
    //	    WCHAR           CharAt (long iLine, long iChar) const;
    //	    WCHAR           CharAt (const POSDATA &pos) const;
    //	    HRESULT         FindFirstTokenOnLine (long iLine, long *piToken) const;
    //	    HRESULT         FindLastTokenOnLine(long iLine, long *piToken) const;
    //	    BOOL            IsFirstTokenOnLine (long iToken) const;
    //	    BOOL            IsLastTokenOnLine (long iToken) const;
    //	    HRESULT         FindFirstNonWhiteChar (long iLine, long *piChar) const;
    //	    HRESULT         FindEndOfComment (long iComment, POSDATA *pposEnd) const;
    //	    HRESULT         FindFirstPrecedingNonWhiteChar (const POSDATA &pos, POSDATA *pposNonWhite) const;
    //	    HRESULT         GetPreprocessorDirective (long iLine, ICSNameTable *pNameTable, PPTOKENID *piToken, long *piStart, long *piEnd) const;
    //	    bool            IsPreprocessorLine(long iLine, ICSNameTable* pNameTable) const;
    //	    HRESULT         SpanContainsPreprocessorDirective(long iStartLine, long iEndLine, ICSNameTable *pNameTable, bool *pfContains) const;
    //	    long            GetLineLength (long iLine) const;
    //	    BOOL            IsWhitespace (long iStartLine, long iStartChar, long iEndLine, long iEndChar) const;
    //	    BOOL            IsLineWhitespaceBefore (long iLine, long iChar) const;
    //	    BOOL            IsLineWhitespaceBefore (const POSDATA &pos) const;
    //	    BOOL            IsLineWhitespaceAfter (long iLine, long iChar) const;
    //	    BOOL            IsLineWhitespaceAfter (const POSDATA &pos) const;
    //	    bool            IsInsideString(const POSDATA& pos) const;
    //	    bool            IsInsideSkippedPreProcessorRegion(const POSDATA& pos) const;
    //	    long            GetAbsolutePosition (long iToken) const;
    //	    long            FindNearestPosition (const POSDATA& pos) const;
    //	    HRESULT         CloneTokenStream(ITokenAllocator *pAllocator, long iFirstToken, long iTokenCount, long iAllocTokens, TOKENSTREAM *pTokenstream) const;
    //	    HRESULT         FreeClonedTokenStream(ITokenAllocator *pAllocator, TOKENSTREAM *pTokenstream) const;
    //	    HRESULT         FindPositionOfText(POSDATA startPos, PCWSTR pszText, POSDATA *ppos) const;
    //	    HRESULT         FindPositionOfText(PCWSTR pszText, POSDATA *ppos) const;
    //	
    //	    bool            TokenTouchesLine(long iTok, unsigned long iLine) const;
    //	
    //	    BOOL            IsNoisyToken(long iToken) const;
    //	    BOOL            IsFirstTokenOnLine (long iToken, bool bSkipNoisyTokens) const;
    //	    BOOL            IsLastTokenOnLine (long iToken, bool bSkipNoisyTokens) const;
    //	    long            PeekTokenIndexFrom (long iCur, long iPeek = 1) const;
    //	    long            NextNonNoisyToken (long iCur) const;
    //	    long            SkipNoisyTokens(long iCur, long iInc) const;
    //	
    //	    static long     PeekTokenIndexFromInternal (CSTOKEN * pTokens, long iTokens, const long iCur, long iPeek);
    //	};

    //======================================================================	
    // Useful functions...
    //======================================================================
    // Moved to CharUtil.cs

    //======================================================================
    // CSTOKEN (for debug)
    //======================================================================
    internal partial class CSTOKEN : POSDATA
    {
#if DEBUG
        internal string GetTokenString(TOKENID id)
        {
            string strToken = null;

            switch (id)
            {
                case TOKENID.UNDEFINED: strToken = "undefined"; break;
                case TOKENID.ARGS: strToken = "__arglist"; break;
                case TOKENID.MAKEREFANY: strToken = "__makeref"; break;
                case TOKENID.REFTYPE: strToken = "__reftype"; break;
                case TOKENID.REFVALUE: strToken = "__refvalue"; break;
                case TOKENID.ABSTRACT: strToken = "abstract"; break;
                case TOKENID.AS: strToken = "as"; break;
                case TOKENID.BASE: strToken = "base"; break;
                case TOKENID.BOOL: strToken = "bool"; break;
                case TOKENID.BREAK: strToken = "break"; break;
                case TOKENID.BYTE: strToken = "byte"; break;
                case TOKENID.CASE: strToken = "case"; break;
                case TOKENID.CATCH: strToken = "catch"; break;
                case TOKENID.CHAR: strToken = "char"; break;
                case TOKENID.CHECKED: strToken = "checked"; break;
                case TOKENID.CLASS: strToken = "class"; break;
                case TOKENID.CONST: strToken = "const"; break;
                case TOKENID.CONTINUE: strToken = "continue"; break;
                case TOKENID.DECIMAL: strToken = "decimal"; break;
                case TOKENID.DEFAULT: strToken = "default"; break;
                case TOKENID.DELEGATE: strToken = "delegate"; break;
                case TOKENID.DO: strToken = "do"; break;
                case TOKENID.DOUBLE: strToken = "double"; break;
                case TOKENID.ELSE: strToken = "else"; break;
                case TOKENID.ENUM: strToken = "enum"; break;
                case TOKENID.EVENT: strToken = "event"; break;
                case TOKENID.EXPLICIT: strToken = "explicit"; break;
                case TOKENID.EXTERN: strToken = "extern"; break;
                case TOKENID.FALSE: strToken = "false"; break;
                case TOKENID.FINALLY: strToken = "finally"; break;
                case TOKENID.FIXED: strToken = "fixed"; break;
                case TOKENID.FLOAT: strToken = "float"; break;
                case TOKENID.FOR: strToken = "for"; break;
                case TOKENID.FOREACH: strToken = "foreach"; break;
                case TOKENID.GOTO: strToken = "goto"; break;
                case TOKENID.IF: strToken = "if"; break;
                case TOKENID.IN: strToken = "in"; break;
                case TOKENID.IMPLICIT: strToken = "implicit"; break;
                case TOKENID.INT: strToken = "int"; break;
                case TOKENID.INTERFACE: strToken = "interface"; break;
                case TOKENID.INTERNAL: strToken = "internal"; break;
                case TOKENID.IS: strToken = "is"; break;
                case TOKENID.LOCK: strToken = "lock"; break;
                case TOKENID.LONG: strToken = "long"; break;
                case TOKENID.NAMESPACE: strToken = "namespace"; break;
                case TOKENID.NEW: strToken = "new"; break;
                case TOKENID.NULL: strToken = "null"; break;
                case TOKENID.OBJECT: strToken = "object"; break;
                case TOKENID.OPERATOR: strToken = "operator"; break;
                case TOKENID.OUT: strToken = "out"; break;
                case TOKENID.OVERRIDE: strToken = "override"; break;
                case TOKENID.PARAMS: strToken = "params"; break;
                case TOKENID.PRIVATE: strToken = "private"; break;
                case TOKENID.PROTECTED: strToken = "protected"; break;
                case TOKENID.PUBLIC: strToken = "public"; break;
                case TOKENID.READONLY: strToken = "readonly"; break;
                case TOKENID.REF: strToken = "ref"; break;
                case TOKENID.RETURN: strToken = "return"; break;
                case TOKENID.SBYTE: strToken = "sbyte"; break;
                case TOKENID.SEALED: strToken = "sealed"; break;
                case TOKENID.SHORT: strToken = "short"; break;
                case TOKENID.SIZEOF: strToken = "sizeof"; break;
                case TOKENID.STACKALLOC: strToken = "stackalloc"; break;
                case TOKENID.STATIC: strToken = "static"; break;
                case TOKENID.STRING: strToken = "string"; break;
                case TOKENID.STRUCT: strToken = "struct"; break;
                case TOKENID.SWITCH: strToken = "switch"; break;
                case TOKENID.THIS: strToken = "this"; break;
                case TOKENID.THROW: strToken = "throw"; break;
                case TOKENID.TRUE: strToken = "true"; break;
                case TOKENID.TRY: strToken = "try"; break;
                case TOKENID.TYPEOF: strToken = "typeof"; break;
                case TOKENID.UINT: strToken = "uint"; break;
                case TOKENID.ULONG: strToken = "ulong"; break;
                case TOKENID.UNCHECKED: strToken = "unchecked"; break;
                case TOKENID.UNSAFE: strToken = "unsafe"; break;
                case TOKENID.USHORT: strToken = "ushort"; break;
                case TOKENID.USING: strToken = "using"; break;
                case TOKENID.VIRTUAL: strToken = "virtual"; break;
                case TOKENID.VOID: strToken = "void"; break;
                case TOKENID.VOLATILE: strToken = "volatile"; break;
                case TOKENID.WHILE: strToken = "while"; break;
                case TOKENID.IDENTIFIER: strToken = "identifier"; break;
                case TOKENID.NUMBER: strToken = "number"; break;
                case TOKENID.STRINGLIT: strToken = "stringlit"; break;
                case TOKENID.VSLITERAL: strToken = "vsliteral"; break;
                case TOKENID.CHARLIT: strToken = "charlit"; break;
                case TOKENID.SLCOMMENT: strToken = "slcomment"; break;
                case TOKENID.DOCCOMMENT: strToken = "doccomment"; break;
                case TOKENID.MLCOMMENT: strToken = "mlcomment"; break;
                case TOKENID.MLDOCCOMMENT: strToken = "mldoccomment"; break;
                case TOKENID.SEMICOLON: strToken = "semicolon"; break;
                case TOKENID.CLOSEPAREN: strToken = "closeparen"; break;
                case TOKENID.CLOSESQUARE: strToken = "closesquare"; break;
                case TOKENID.OPENCURLY: strToken = "opencurly"; break;
                case TOKENID.CLOSECURLY: strToken = "closecurly"; break;
                case TOKENID.COMMA: strToken = "comma"; break;
                case TOKENID.EQUAL: strToken = "equal"; break;
                case TOKENID.PLUSEQUAL: strToken = "plusequal"; break;
                case TOKENID.MINUSEQUAL: strToken = "minusequal"; break;
                case TOKENID.SPLATEQUAL: strToken = "splatequal"; break;
                case TOKENID.SLASHEQUAL: strToken = "slashequal"; break;
                case TOKENID.MODEQUAL: strToken = "modequal"; break;
                case TOKENID.ANDEQUAL: strToken = "andequal"; break;
                case TOKENID.HATEQUAL: strToken = "hatequal"; break;
                case TOKENID.BAREQUAL: strToken = "barequal"; break;
                case TOKENID.SHIFTLEFTEQ: strToken = "shiftlefteq"; break;
                case TOKENID.SHIFTRIGHTEQ: strToken = "shiftrighteq"; break;
                case TOKENID.QUESTION: strToken = "question"; break;
                case TOKENID.COLON: strToken = "colon"; break;
                case TOKENID.COLONCOLON: strToken = "coloncolon"; break;
                case TOKENID.LOG_OR: strToken = "log_or"; break;
                case TOKENID.LOG_AND: strToken = "log_and"; break;
                case TOKENID.BAR: strToken = "bar"; break;
                case TOKENID.HAT: strToken = "hat"; break;
                case TOKENID.AMPERSAND: strToken = "ampersand"; break;
                case TOKENID.EQUALEQUAL: strToken = "equalequal"; break;
                case TOKENID.NOTEQUAL: strToken = "notequal"; break;
                case TOKENID.LESS: strToken = "less"; break;
                case TOKENID.LESSEQUAL: strToken = "lessequal"; break;
                case TOKENID.GREATER: strToken = "greater"; break;
                case TOKENID.GREATEREQUAL: strToken = "greaterequal"; break;
                case TOKENID.SHIFTLEFT: strToken = "shiftleft"; break;
                case TOKENID.SHIFTRIGHT: strToken = "shiftright"; break;
                case TOKENID.PLUS: strToken = "plus"; break;
                case TOKENID.MINUS: strToken = "minus"; break;
                case TOKENID.STAR: strToken = "star"; break;
                case TOKENID.SLASH: strToken = "slash"; break;
                case TOKENID.PERCENT: strToken = "percent"; break;
                case TOKENID.TILDE: strToken = "tilde"; break;
                case TOKENID.BANG: strToken = "bang"; break;
                case TOKENID.PLUSPLUS: strToken = "plusplus"; break;
                case TOKENID.MINUSMINUS: strToken = "minusminus"; break;
                case TOKENID.OPENPAREN: strToken = "openparen"; break;
                case TOKENID.OPENSQUARE: strToken = "opensquare"; break;
                case TOKENID.DOT: strToken = "dot"; break;
                case TOKENID.ARROW: strToken = "arrow"; break;
                case TOKENID.QUESTQUEST: strToken = "questquest"; break;
                case TOKENID.EQUALGREATER: strToken = "equalgreater"; break;
                case TOKENID.ENDFILE: strToken = "endfile"; break;
                case TOKENID.UNKNOWN: strToken = "unknown"; break;
                case TOKENID.INVALID: strToken = "invalid"; break;
                case TOKENID.NUMTOKENS: strToken = "(numtokens|openangle|closeangle)"; break;
                //case TOKENID.OPENANGLE: strToken = "openangle"; break;
                //case TOKENID.CLOSEANGLE: strToken = "closeangle"; break;
                default: strToken = "(Error: invalid token)"; break;
            }
            return strToken;
        }

        internal void DebugString(StringBuilder sb)
        {
            string strTemp = null;

            sb.AppendFormat("TokenID   : {0}\r\n", GetTokenString(TokenID));
            sb.AppendFormat("TokenData : {0}\r\n", this.tokenData.ToString());

            sb.Append("TokenFlags:");
            strTemp = ((TOKFLAGS.F_NSELEMENT & TokenFlags) != 0 ? "F_NSELEMENT" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_MEMBER & TokenFlags) != 0 ? "F_MEMBER" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_TYPEDECL & TokenFlags) != 0 ? "F_TYPEDECL" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_PREDEFINED & TokenFlags) != 0 ? "F_PREDEFINED" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_TERM & TokenFlags) != 0 ? "F_TERM" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_OVLUNOP & TokenFlags) != 0 ? "F_OVLUNOP" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_OVLBINOP & TokenFlags) != 0 ? "F_OVLBINOP" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_OVLOPKWD & TokenFlags) != 0 ? "F_OVLOPKWD" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_CASTEXPR & TokenFlags) != 0 ? "F_CASTEXPR" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_MODIFIER & TokenFlags) != 0 ? "F_MODIFIER" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_NOISE & TokenFlags) != 0 ? "F_NOISE" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_MSKEYWORD & TokenFlags) != 0 ? "F_MSKEYWORD" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.F_INVALIDSUBEXPRESSION & TokenFlags) != 0 ? "F_INVALIDSUBEXPRESSION" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.OVERHEAD & TokenFlags) != 0 ? "OVERHEAD" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.UNTERMINATED & TokenFlags) != 0 ? "UNTERMINATED" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.HEAPALLOCATED & TokenFlags) != 0 ? "HEAPALLOCATED" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.VERBATIMSTRING & TokenFlags) != 0 ? "VERBATIMSTRING" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.USEDCOMMENT & TokenFlags) != 0 ? "USEDCOMMENT" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            strTemp = ((TOKFLAGS.HEXLITERAL & TokenFlags) != 0 ? "HEXLITERAL" : null);
            if (strTemp != null) sb.AppendFormat(" {0}", strTemp);
            sb.Append("\r\n");

            sb.AppendFormat("Position  : ({0}, {1})\r\n", this.LineIndex, this.CharIndex);
        }

#endif
    }
}
