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
// File: tokinfo.h
//
// ===========================================================================

//============================================================================
// TokInfo.cs
//
// 2013/10/20
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // TOKINFO
    //======================================================================
    internal class TOKINFO
    {
        /// <summary>
        /// token text
        /// </summary>
        internal string Text = null; // PCWSTR pszText;

        // Keep the pointers first for better packing on 64-bits

        /// <summary>
        /// token flags
        /// </summary>
        internal TOKFLAGS Flags;    // DWORD dwFlags;

        /// <summary>
        /// token length
        /// </summary>
        internal int Length = 0;    // BYTE iLen;
 

        /// <summary>
        /// Parser fn
        /// </summary>
        internal CParser.PARSERTYPE StatementParser;    // BYTE iStmtParser;

        /// <summary>
        /// predefined type represented by token
        /// </summary>
        internal PREDEFTYPE PredefinedType;
        
        /// <summary>
        /// Binary operator
        /// </summary>
        internal OPERATOR BinaryOperator;

        /// <summary>
        /// Unary operator
        /// </summary>
        internal OPERATOR UnaryOperator;

        /// <summary>
        /// Self operator (like true, false, this...)
        /// </summary>
        internal OPERATOR SelfOperator;

        internal TOKINFO() { }
        internal TOKINFO(
            string text,
            TOKFLAGS flags,
            int len,
            CParser.PARSERTYPE parser,
            PREDEFTYPE predef,
            OPERATOR binary,
            OPERATOR unary,
            OPERATOR self
            )
        {
            this.Text = text;
            this.Flags = flags;
            this.Length = len;
            this.StatementParser = parser;
            this.PredefinedType = predef;
            this.BinaryOperator = binary;
            this.UnaryOperator = unary;
            this.SelfOperator = self;
        }

    }

    //======================================================================
    // OPINFO
    //======================================================================
    internal class OPINFO
    {
        internal TOKENID TokenID;           // Token ID
        internal int Precedence;            // Operator precedence
        internal bool RightAssociativity;   // Associativity

        //------------------------------------------------------------
        // OPINFO Constructor
        //------------------------------------------------------------
        internal OPINFO(TOKENID id, int prec, bool rassoc)
        {
            this.TokenID = id;
            this.Precedence = prec;
            this.RightAssociativity = rassoc;
        }
    }

    //struct NAME;
    //class COMPILER;
}
