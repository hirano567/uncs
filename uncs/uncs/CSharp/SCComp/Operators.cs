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
// File: operators.cpp
//
// Routines for operator overload resolution.
// ===========================================================================

// ===========================================================================
//  These are the predefined binary operator signatures
//
//      (object,    object)     :                   == !=
//      (string,    string)     :                   == !=
//      (string,    string)     :       +
//      (string,    object)     :       +
//      (object,    string)     :       +
//
//      (int,       int)        : * / % + - << >>   == != < > <= >= & | ^
//      (uint,      uint)       : * / % + -         == != < > <= >= & | ^
//      (long,      long)       : * / % + -         == != < > <= >= & | ^
//      (ulong,     ulong)      : * / % + -         == != < > <= >= & | ^
//      (uint,      int)        :           << >>
//      (long,      int)        :           << >>
//      (ulong,     int)        :           << >>
//
//      (float,     float)      : * / % + -         == != < > <= >=
//      (double,    double)     : * / % + -         == != < > <= >=
//      (decimal,   decimal)    : * / % + -         == != < > <= >=
//
//      (bool,      bool)       :                   == !=           & | ^ && ||
//
//      (Sys.Del,   Sys.Del)    :                   == !=
//
//      // Below here the types cannot be represented entirely by a PREDEFTYPE.
//      (delegate,  delegate)   :       + -         == !=
//
//      (enum,      enum)       :         -         == != < > <= >= & | ^
//      (enum,      under)      :       + -
//      (under,     enum)       :       +
//
//      (ptr,       ptr)        :         -
//      (ptr,       int)        :       + -
//      (ptr,       uint)       :       + -
//      (ptr,       long)       :       + -
//      (ptr,       ulong)      :       + -
//      (int,       ptr)        :       +
//      (uint,      ptr)        :       +
//      (long,      ptr)        :       +
//      (ulong,     ptr)        :       +
//
//      (void*,     void*)      :                   == != < > <= >=
//
//  There are the predefined unary operator signatures:
//
//      int     : + -   ~
//      uint    : +     ~
//      long    : + -   ~
//      ulong   : +     ~
//
//      float   : + -   
//      double  : + - 
//      decimal : + - 
//
//      bool    :     !
//
//      // Below here the types cannot be represented entirely by a PREDEFTYPE.
//      enum    :       ~
//      ptr     :         * 
//
//  Note that pointer operators cannot be lifted over nullable.
// ===========================================================================

//============================================================================
//  WithType.cs
//
//  2014/12/30
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // enum BindBinOpEnum
    //
    /// <summary></summary>
    /// <remarks>
    /// (2014/12/30 hirano567@hotmail.co.jp)
    /// </remarks>
    //======================================================================
    enum BindBinOpEnum : int
    {
        None = 0,
        Integer,
        Real,
        Decimal,
        String,
        Shift,
        Bool,
        BoolBitwise,
        Delegate,
        Enum,
        StringCompare,
        ReferenceCompare,
        Pointer,
        PointerCompare,
    }

    //======================================================================
    // enum BindUnaOpEnum
    //
    /// <summary></summary>
    /// <remarks>
    /// (2014/12/30 hirano567@hotmail.co.jp)
    /// </remarks>
    //======================================================================
    enum BindUnaOpEnum : int
    {
        None = 0,
        Integer,
        Real,
        Decimal,
        Bool,
        Enum,
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC.BinOpSigArray
        //------------------------------------------------------------
        static protected BinOpSig[] BinOpSigArray =  // static BinOpSig g_rgbos[];
        {
            new BinOpSig( PREDEFTYPE.INT, PREDEFTYPE.INT, BinOpMaskEnum.Integer, 8, BindBinOpEnum.Integer, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.UINT, PREDEFTYPE.UINT, BinOpMaskEnum.Integer, 7, BindBinOpEnum.Integer, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.LONG, PREDEFTYPE.LONG, BinOpMaskEnum.Integer, 6, BindBinOpEnum.Integer, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.ULONG, PREDEFTYPE.ULONG, BinOpMaskEnum.Integer, 5, BindBinOpEnum.Integer, OpSigFlagsEnum.Value ),

            // These two are errors.
            new BinOpSig( PREDEFTYPE.ULONG, PREDEFTYPE.LONG, BinOpMaskEnum.Integer, 4, BindBinOpEnum.None, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.LONG, PREDEFTYPE.ULONG, BinOpMaskEnum.Integer, 3, BindBinOpEnum.None, OpSigFlagsEnum.Value ),

            new BinOpSig( PREDEFTYPE.FLOAT, PREDEFTYPE.FLOAT, BinOpMaskEnum.Real, 1, BindBinOpEnum.Real, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.DOUBLE, PREDEFTYPE.DOUBLE, BinOpMaskEnum.Real, 0, BindBinOpEnum.Real, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.DECIMAL, PREDEFTYPE.DECIMAL, BinOpMaskEnum.Real, 0, BindBinOpEnum.Decimal, OpSigFlagsEnum.Value ),

            new BinOpSig( PREDEFTYPE.STRING, PREDEFTYPE.STRING, BinOpMaskEnum.Equal, 0, BindBinOpEnum.StringCompare, OpSigFlagsEnum.Reference ),

            new BinOpSig( PREDEFTYPE.STRING, PREDEFTYPE.STRING, BinOpMaskEnum.Add, 2, BindBinOpEnum.String, OpSigFlagsEnum.Reference ),
            new BinOpSig( PREDEFTYPE.STRING, PREDEFTYPE.OBJECT, BinOpMaskEnum.Add, 1, BindBinOpEnum.String, OpSigFlagsEnum.Reference ),
            new BinOpSig( PREDEFTYPE.OBJECT, PREDEFTYPE.STRING, BinOpMaskEnum.Add, 0, BindBinOpEnum.String, OpSigFlagsEnum.Reference ),

            new BinOpSig( PREDEFTYPE.INT, PREDEFTYPE.INT, BinOpMaskEnum.Shift, 3, BindBinOpEnum.Shift, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.UINT, PREDEFTYPE.INT, BinOpMaskEnum.Shift, 2, BindBinOpEnum.Shift, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.LONG, PREDEFTYPE.INT, BinOpMaskEnum.Shift, 1, BindBinOpEnum.Shift, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.ULONG, PREDEFTYPE.INT, BinOpMaskEnum.Shift, 0, BindBinOpEnum.Shift, OpSigFlagsEnum.Value ),

            new BinOpSig( PREDEFTYPE.BOOL, PREDEFTYPE.BOOL, BinOpMaskEnum.BoolNorm, 0, BindBinOpEnum.Bool, OpSigFlagsEnum.Value ),
            new BinOpSig( PREDEFTYPE.BOOL, PREDEFTYPE.BOOL, BinOpMaskEnum.Logical, 0, BindBinOpEnum.Bool, OpSigFlagsEnum.Convert ),
            new BinOpSig( PREDEFTYPE.BOOL, PREDEFTYPE.BOOL, BinOpMaskEnum.Bitwise, 0, BindBinOpEnum.BoolBitwise, OpSigFlagsEnum.BoolBit ),
        };

        //------------------------------------------------------------
        // FUNCBREC.UnaOpSigArray
        //------------------------------------------------------------
        static protected UnaOpSig[] UnaOpSigArray =  // static UnaOpSig g_rguos[];
        {
            new UnaOpSig( PREDEFTYPE.INT, UnaOpMaskEnum.Signed, 7, BindUnaOpEnum.Integer ),
            new UnaOpSig( PREDEFTYPE.UINT, UnaOpMaskEnum.Unsigned, 6, BindUnaOpEnum.Integer ),
            new UnaOpSig( PREDEFTYPE.LONG, UnaOpMaskEnum.Signed, 5, BindUnaOpEnum.Integer ),
            new UnaOpSig( PREDEFTYPE.ULONG, UnaOpMaskEnum.Unsigned, 4, BindUnaOpEnum.Integer ),

            // This is NOT a bug! We want unary minus to bind to "operator -(ulong)" and then we
            // produce an error (since there is no pfn). We can't let - bind to a floating point type,
            // since they lose precision. See the language spec. 
            new UnaOpSig( PREDEFTYPE.ULONG, UnaOpMaskEnum.Minus, 3, BindUnaOpEnum.None ),

            new UnaOpSig( PREDEFTYPE.FLOAT, UnaOpMaskEnum.Real, 1, BindUnaOpEnum.Real ),
            new UnaOpSig( PREDEFTYPE.DOUBLE, UnaOpMaskEnum.Real, 0, BindUnaOpEnum.Real ),
            new UnaOpSig( PREDEFTYPE.DECIMAL, UnaOpMaskEnum.Real, 0, BindUnaOpEnum.Decimal ),

            new UnaOpSig( PREDEFTYPE.BOOL, UnaOpMaskEnum.Bool, 0, BindUnaOpEnum.Bool ),

            // Increment and decrement operators. These are special cased.
            new UnaOpSig( PREDEFTYPE.SBYTE, UnaOpMaskEnum.IncDec, 10, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.BYTE, UnaOpMaskEnum.IncDec, 9, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.SHORT, UnaOpMaskEnum.IncDec, 8, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.USHORT, UnaOpMaskEnum.IncDec, 7, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.INT, UnaOpMaskEnum.IncDec, 6, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.UINT, UnaOpMaskEnum.IncDec, 5, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.LONG, UnaOpMaskEnum.IncDec, 4, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.ULONG, UnaOpMaskEnum.IncDec, 3, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.FLOAT, UnaOpMaskEnum.IncDec, 1, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.DOUBLE, UnaOpMaskEnum.IncDec, 0, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.DECIMAL, UnaOpMaskEnum.IncDec, 0, BindUnaOpEnum.None ),
            new UnaOpSig( PREDEFTYPE.CHAR, UnaOpMaskEnum.IncDec, 0, BindUnaOpEnum.None ),
        };

        //------------------------------------------------------------
        // FUNCBREC.BindBinaryOperator
        //
        // EXPR * BindIntBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindRealBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindDecBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindStrBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindShiftOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindBoolBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindBoolBitwiseOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindDelBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindEnumBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // 
        // EXPR * BindStrCmpOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindRefCmpOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindPtrBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        // EXPR * BindPtrCmpOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
        //
        /// <summary></summary>
        /// <param name="op"></param>
        /// <param name="tree"></param>
        /// <param name="ek"></param>
        /// <param name="flags"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindBinaryOperator(
            BindBinOpEnum op,
            BASENODE tree,
            EXPRKIND ek,
            EXPRFLAG flags,
            EXPR arg1,
            EXPR arg2)
        {
            switch (op)
            {
                case BindBinOpEnum.Integer:
                    // Handles standard binary integer based operators.
                    // EXPR * FUNCBREC::BindIntBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2)
                    DebugUtil.Assert(
                        arg1.TypeSym.IsPredefined() &&
                        arg2.TypeSym.IsPredefined() &&
                        arg1.TypeSym.GetPredefType() == arg2.TypeSym.GetPredefType());
                    return BindIntOp(tree, ek, flags, arg1, arg2, arg1.TypeSym.GetPredefType());

                case BindBinOpEnum.Real:
                    // Handles standard binary floating point (float, double) based operators.
                    // EXPR * FUNCBREC::BindRealBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2)
                    DebugUtil.Assert(
                        arg1.TypeSym.IsPredefined() &&
                        arg2.TypeSym.IsPredefined() &&
                        arg1.TypeSym.GetPredefType() == arg2.TypeSym.GetPredefType());
                    return BindFloatOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.Decimal:
                    return BindDecBinOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.String:
                    // Handles string concatenation.
                    // EXPR * BindStrBinOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg1, EXPR * arg2);
                    DebugUtil.Assert(ek == EXPRKIND.ADD);
                    DebugUtil.Assert(
                        arg1.TypeSym.IsPredefType(PREDEFTYPE.STRING) ||
                        arg2.TypeSym.IsPredefType(PREDEFTYPE.STRING));
                    return BindStringConcat(tree, arg1, arg2);

                case BindBinOpEnum.Shift:
                    return BindShiftOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.Bool:
                    return BindBoolBinOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.BoolBitwise:
                    return BindBoolBitwiseOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.Delegate:
                    return BindDelegateBinOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.Enum:
                    return BindEnumBinOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.StringCompare:
                    return BindStrCmpOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.ReferenceCompare:
                    return BindRefCmpOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.Pointer:
                    return BindPtrBinOp(tree, ek, flags, arg1, arg2);

                case BindBinOpEnum.PointerCompare:
                    return BindPtrCmpOp(tree, ek, flags, arg1, arg2);

                default:
                    DebugUtil.Assert(false, "FUNCBREC.BindBinaryOperator");
                    break;
            }
            return null;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindUnaryOperator
        //
        // EXPR * BindStdUnaOp(BASENODE * tree, OPERATOR op, EXPR * arg);
        // EXPR * BindIntUnaOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg);
        // EXPR * BindRealUnaOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg);
        // EXPR * BindDecUnaOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg);
        // EXPR * BindBoolUnaOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg);
        // EXPR * BindEnumUnaOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg);
        //------------------------------------------------------------
        internal EXPR BindUnaryOperator(
            BindUnaOpEnum op,
            BASENODE tree,
            EXPRKIND ek,
            EXPRFLAG flags,
            EXPR arg)
        {
            switch (op)
            {
                case BindUnaOpEnum.Integer:
                    // Handles standard unary integer based operators.
                    // EXPR * FUNCBREC::BindIntUnaOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg)
                    DebugUtil.Assert(arg.TypeSym.IsPredefined());
                    return BindIntOp(tree, ek, flags, arg, null, arg.TypeSym.GetPredefType());

                case BindUnaOpEnum.Real:
                    // Handles standard unary floating point (float, double) based operators.
                    // EXPR * FUNCBREC::BindRealUnaOp(BASENODE * tree, EXPRKIND ek, uint flags, EXPR * arg)
                    DebugUtil.Assert(arg.TypeSym.IsPredefined());
                    return BindFloatOp(tree, ek, flags, arg, null);

                case BindUnaOpEnum.Decimal:
                    return BindDecUnaOp(tree, ek, flags, arg);

                case BindUnaOpEnum.Bool:
                    return BindBoolUnaOp(tree, ek, flags, arg);
                    break;

                case BindUnaOpEnum.Enum:
                    return BindEnumUnaOp(tree, ek, flags, arg);

                default:
                    DebugUtil.Assert(false, "FUNCBREC.BindUnaryOperator");
                    break;
            }
            return null;
        }

        //------------------------------------------------------------
        // FUNCBREC.g_mpptptBetter
        //
        /// <summary>
        /// <para>This table indicates for predefined types
        /// through object which are better for the purposes of overload resolution.
        /// 0 means they're the same,
        /// 1 means the left index is better,
        /// 2 means the right,
        /// 3 means neither.
        /// These values MUST match the values of the BetterType enum.</para>
        /// <para>(CSharp\SCComp\Operators.cs)</para>
        /// </summary>
        //------------------------------------------------------------
        static protected byte[,] g_mpptptBetter =
        {
            //BYTE  SHORT   INT     LONG    FLOAT   DOUBLE  DECIMAL CHAR    BOOL    SBYTE   USHORT  UINT    ULONG   IPTR     UIPTR    OBJECT
            {0,     1,      1,      1,      1,      1,      1,      3,      3,      2,      1,      1,      1,      3,       3,       1}, // BYTE{
            {2,     0,      1,      1,      1,      1,      1,      3,      3,      2,      1,      1,      1,      3,       3,       1}, // SHORT
            {2,     2,      0,      1,      1,      1,      1,      2,      3,      2,      2,      1,      1,      3,       3,       1}, // INT
            {2,     2,      2,      0,      1,      1,      1,      2,      3,      2,      2,      2,      1,      3,       3,       1}, // LONG
            {2,     2,      2,      2,      0,      1,      3,      2,      3,      2,      2,      2,      2,      3,       3,       1}, // FLOAT
            {2,     2,      2,      2,      2,      0,      3,      2,      3,      2,      2,      2,      2,      3,       3,       1}, // DOUBLE
            {2,     2,      2,      2,      3,      3,      0,      2,      3,      2,      2,      2,      2,      3,       3,       1}, // DECIMAL
            {3,     3,      1,      1,      1,      1,      1,      0,      3,      3,      1,      1,      1,      3,       3,       1}, // CHAR
            {3,     3,      3,      3,      3,      3,      3,      3,      0,      3,      3,      3,      3,      3,       3,       1}, // BOOL
            {1,     1,      1,      1,      1,      1,      1,      3,      3,      0,      1,      1,      1,      3,       3,       1}, // SBYTE
            {2,     2,      1,      1,      1,      1,      1,      2,      3,      2,      0,      1,      1,      3,       3,       1}, // USHORT
            {2,     2,      2,      1,      1,      1,      1,      2,      3,      2,      2,      0,      1,      3,       3,       1}, // UINT
            {2,     2,      2,      2,      1,      1,      1,      2,      3,      2,      2,      2,      0,      3,       3,       1}, // ULONG
            {3,     3,      3,      3,      3,      3,      3,      3,      3,      3,      3,      3,      3,      0,       3,       1}, // IPTR
            {3,     3,      3,      3,      3,      3,      3,      3,      3,      3,      3,      3,      3,      3,       0,       1}, // UIPTR
            {2,     2,      2,      2,      2,      2,      2,      2,      2,      2,      2,      2,      2,      2,       2,       0} // OBJECT
        };
    }
}



