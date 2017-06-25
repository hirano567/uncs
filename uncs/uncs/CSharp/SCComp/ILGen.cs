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
// File: ilgen.h
//
// Defines record used to generate il for a method
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
// File: ilgen.cpp
//
// Routines for generating il for a method
// ===========================================================================

//============================================================================
// ILGen.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //======================================================================
    // ILCODE   (CEE_)
    //======================================================================
    internal enum ILCODE : int
    {
        //#define OPDEF(id, name, pop, push, operand, type, len, b1, b2, cf) id,
        //#include "opcode.def"

        NOP,
        BREAK,
        LDARG_0,
        LDARG_1,
        LDARG_2,
        LDARG_3,
        LDLOC_0,
        LDLOC_1,
        LDLOC_2,
        LDLOC_3,
        STLOC_0,
        STLOC_1,
        STLOC_2,
        STLOC_3,
        LDARG_S,
        LDARGA_S,
        STARG_S,
        LDLOC_S,
        LDLOCA_S,
        STLOC_S,
        LDNULL,
        LDC_I4_M1,
        LDC_I4_0,
        LDC_I4_1,
        LDC_I4_2,
        LDC_I4_3,
        LDC_I4_4,
        LDC_I4_5,
        LDC_I4_6,
        LDC_I4_7,
        LDC_I4_8,
        LDC_I4_S,
        LDC_I4,
        LDC_I8,
        LDC_R4,
        LDC_R8,
        UNUSED49,
        DUP,
        POP,
        JMP,
        CALL,
        CALLI,
        RET,
        BR_S,
        BRFALSE_S,
        BRTRUE_S,
        BEQ_S,
        BGE_S,
        BGT_S,
        BLE_S,
        BLT_S,
        BNE_UN_S,
        BGE_UN_S,
        BGT_UN_S,
        BLE_UN_S,
        BLT_UN_S,
        BR,
        BRFALSE,
        BRTRUE,
        BEQ,
        BGE,
        BGT,
        BLE,
        BLT,
        BNE_UN,
        BGE_UN,
        BGT_UN,
        BLE_UN,
        BLT_UN,
        SWITCH,
        LDIND_I1,
        LDIND_U1,
        LDIND_I2,
        LDIND_U2,
        LDIND_I4,
        LDIND_U4,
        LDIND_I8,
        LDIND_I,
        LDIND_R4,
        LDIND_R8,
        LDIND_REF,
        STIND_REF,
        STIND_I1,
        STIND_I2,
        STIND_I4,
        STIND_I8,
        STIND_R4,
        STIND_R8,
        ADD,
        SUB,
        MUL,
        DIV,
        DIV_UN,
        REM,
        REM_UN,
        AND,
        OR,
        XOR,
        SHL,
        SHR,
        SHR_UN,
        NEG,
        NOT,
        CONV_I1,
        CONV_I2,
        CONV_I4,
        CONV_I8,
        CONV_R4,
        CONV_R8,
        CONV_U4,
        CONV_U8,
        CALLVIRT,
        CPOBJ,
        LDOBJ,
        LDSTR,
        NEWOBJ,
        CASTCLASS,
        ISINST,
        CONV_R_UN,
        UNUSED58,
        UNUSED1,
        UNBOX,
        THROW,
        LDFLD,
        LDFLDA,
        STFLD,
        LDSFLD,
        LDSFLDA,
        STSFLD,
        STOBJ,
        CONV_OVF_I1_UN,
        CONV_OVF_I2_UN,
        CONV_OVF_I4_UN,
        CONV_OVF_I8_UN,
        CONV_OVF_U1_UN,
        CONV_OVF_U2_UN,
        CONV_OVF_U4_UN,
        CONV_OVF_U8_UN,
        CONV_OVF_I_UN,
        CONV_OVF_U_UN,
        BOX,
        NEWARR,
        LDLEN,
        LDELEMA,
        LDELEM_I1,
        LDELEM_U1,
        LDELEM_I2,
        LDELEM_U2,
        LDELEM_I4,
        LDELEM_U4,
        LDELEM_I8,
        LDELEM_I,
        LDELEM_R4,
        LDELEM_R8,
        LDELEM_REF,
        STELEM_I,
        STELEM_I1,
        STELEM_I2,
        STELEM_I4,
        STELEM_I8,
        STELEM_R4,
        STELEM_R8,
        STELEM_REF,
        LDELEM,
        STELEM,
        UNBOX_ANY,
        UNUSED5,
        UNUSED6,
        UNUSED7,
        UNUSED8,
        UNUSED9,
        UNUSED10,
        UNUSED11,
        UNUSED12,
        UNUSED13,
        UNUSED14,
        UNUSED15,
        UNUSED16,
        UNUSED17,
        CONV_OVF_I1,
        CONV_OVF_U1,
        CONV_OVF_I2,
        CONV_OVF_U2,
        CONV_OVF_I4,
        CONV_OVF_U4,
        CONV_OVF_I8,
        CONV_OVF_U8,
        UNUSED50,
        UNUSED18,
        UNUSED19,
        UNUSED20,
        UNUSED21,
        UNUSED22,
        UNUSED23,
        REFANYVAL,
        CKFINITE,
        UNUSED24,
        UNUSED25,
        MKREFANY,
        UNUSED59,
        UNUSED60,
        UNUSED61,
        UNUSED62,
        UNUSED63,
        UNUSED64,
        UNUSED65,
        UNUSED66,
        UNUSED67,
        LDTOKEN,
        CONV_U2,
        CONV_U1,
        CONV_I,
        CONV_OVF_I,
        CONV_OVF_U,
        ADD_OVF,
        ADD_OVF_UN,
        MUL_OVF,
        MUL_OVF_UN,
        SUB_OVF,
        SUB_OVF_UN,
        ENDFINALLY,
        LEAVE,
        LEAVE_S,
        STIND_I,
        CONV_U,
        UNUSED26,
        UNUSED27,
        UNUSED28,
        UNUSED29,
        UNUSED30,
        UNUSED31,
        UNUSED32,
        UNUSED33,
        UNUSED34,
        UNUSED35,
        UNUSED36,
        UNUSED37,
        UNUSED38,
        UNUSED39,
        UNUSED40,
        UNUSED41,
        UNUSED42,
        UNUSED43,
        UNUSED44,
        UNUSED45,
        UNUSED46,
        UNUSED47,
        UNUSED48,
        PREFIX7,
        PREFIX6,
        PREFIX5,
        PREFIX4,
        PREFIX3,
        PREFIX2,
        PREFIX1,
        PREFIXREF,

        // 2 bytes
        ARGLIST,
        CEQ,
        CGT,
        CGT_UN,
        CLT,
        CLT_UN,
        LDFTN,
        LDVIRTFTN,
        UNUSED56,
        LDARG,
        LDARGA,
        STARG,
        LDLOC,
        LDLOCA,
        STLOC,
        LOCALLOC,
        UNUSED57,
        ENDFILTER,
        UNALIGNED,
        VOLATILE,
        TAILCALL,
        INITOBJ,
        CONSTRAINED,
        CPBLK,
        INITBLK,
        UNUSED69,
        RETHROW,
        UNUSED51,
        SIZEOF,
        REFANYTYPE,
        READONLY,
        UNUSED53,
        UNUSED54,
        UNUSED55,
        UNUSED70,

        ILLEGAL,
        MACRO_END,
        CODE_LABEL,

        //#undef OPDEF

        // 0 byte
        last,
        next,
        stop,
    }

    // class ILInstruction and its derived classes
    // These classes store arguments of ILGenerator.Emit method

    //======================================================================
    // class ILInstruction
    //
    /// <summary>
    /// The base class of ILInstruction_*.
    /// </summary>
    //======================================================================
    internal class ILInstruction
    {
        internal ILCODE Code;

        //------------------------------------------------------------
        // ILInstruction Constructor
        //------------------------------------------------------------
        internal ILInstruction() { }

        internal ILInstruction(ILCODE code)
        {
            this.Code = code;
        }

        //------------------------------------------------------------
        // ILInstruction.Emit
        //------------------------------------------------------------
        virtual internal void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction.Debug
        //------------------------------------------------------------
        virtual internal void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0}\n", this.Code);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Byte
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Byte : ILInstruction
    {
        internal Byte Arg;

        //------------------------------------------------------------
        // ILInstruction_Byte Constructor
        //------------------------------------------------------------
        internal ILInstruction_Byte() { }

        internal ILInstruction_Byte(ILCODE code, Byte arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Byte.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Byte.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} byte:{1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_SByte
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_SByte : ILInstruction
    {
        internal SByte Arg;

        //------------------------------------------------------------
        // ILInstruction_SByte Constructor
        //------------------------------------------------------------
        internal ILInstruction_SByte() { }

        internal ILInstruction_SByte(ILCODE code, SByte arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_SByte.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_SByte.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} sbyte:{1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Int16
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Int16 : ILInstruction
    {
        internal Int16 Arg;

        //------------------------------------------------------------
        // ILInstruction_Int16 Constructor
        //------------------------------------------------------------
        internal ILInstruction_Int16() { }

        internal ILInstruction_Int16(ILCODE code, Int16 arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Int16.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Int16.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} int16:{1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Int32
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Int32 : ILInstruction
    {
        internal Int32 Arg;

        //------------------------------------------------------------
        // ILInstruction_Int32 Constructor
        //------------------------------------------------------------
        internal ILInstruction_Int32() { }

        internal ILInstruction_Int32(ILCODE code, Int32 arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Int32.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Int32.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} int32:{1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_MethodInfo
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_MethodInfo : ILInstruction
    {
        internal MethodInfo Arg = null;
        internal Type[] VarArgTypes = null;

        //------------------------------------------------------------
        // ILInstruction_MethodInfo Constructor
        //------------------------------------------------------------
        internal ILInstruction_MethodInfo() { }

        internal ILInstruction_MethodInfo(ILCODE code, MethodInfo arg)
            : base(code)
        {
            this.Arg = arg;
        }

        internal ILInstruction_MethodInfo(ILCODE code, MethodInfo arg,Type[] types)
            : base(code)
        {
            this.Arg = arg;
            this.VarArgTypes = types;
        }

        //------------------------------------------------------------
        // ILInstruction_MethodInfo.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                if (this.VarArgTypes == null || this.VarArgTypes.Length == 0)
                {
                    gen.Emit(opCode, this.Arg);
                }
                else if (opCode == OpCodes.Calli)
                {
                    //gen.EmitCalli(opCode, this.Arg, this.VarArgTypes);
                    throw new NotImplementedException("ILInstruction_MethodInfo.Emit");
                }
                else
                {
                    gen.EmitCall(opCode, this.Arg, this.VarArgTypes);
                }
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_MethodInfo.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat(
                "{0} {1}\n",
                this.Code,
                this.Arg != null ? this.Arg.ToString() : "???");
        }
#endif
    }

    //======================================================================
    // class ILInstruction_SignatureHelper
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_SignatureHelper : ILInstruction
    {
        internal SignatureHelper Arg;

        //------------------------------------------------------------
        // ILInstruction_SignatureHelper Constructor
        //------------------------------------------------------------
        internal ILInstruction_SignatureHelper() { }

        internal ILInstruction_SignatureHelper(ILCODE code, SignatureHelper arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_SignatureHelper.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_SignatureHelper.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_ConstructorInfo
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_ConstructorInfo : ILInstruction
    {
        internal ConstructorInfo Arg;

        //------------------------------------------------------------
        // ILInstruction_ConstructorInfo Constructor
        //------------------------------------------------------------
        internal ILInstruction_ConstructorInfo() { }

        internal ILInstruction_ConstructorInfo(ILCODE code, ConstructorInfo arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_ConstructorInfo.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_ConstructorInfo.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Type
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Type : ILInstruction
    {
        internal Type Arg;

        //------------------------------------------------------------
        // ILInstruction_Type Constructor
        //------------------------------------------------------------
        internal ILInstruction_Type() { }

        internal ILInstruction_Type(ILCODE code, Type arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Type.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Type.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Int64
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Int64 : ILInstruction
    {
        internal Int64 Arg;

        //------------------------------------------------------------
        // ILInstruction_Int64 Constructor
        //------------------------------------------------------------
        internal ILInstruction_Int64() { }

        internal ILInstruction_Int64(ILCODE code, Int64 arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Int64.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Int64.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} int16:{1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Single
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Single : ILInstruction
    {
        internal Single Arg;

        //------------------------------------------------------------
        // ILInstruction_Single Constructor
        //------------------------------------------------------------
        internal ILInstruction_Single() { }

        internal ILInstruction_Single(ILCODE code, Single arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Single.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Single.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} single:{1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Double
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Double : ILInstruction
    {
        internal Double Arg;

        //------------------------------------------------------------
        // ILInstruction_Double Constructor
        //------------------------------------------------------------
        internal ILInstruction_Double() { }

        internal ILInstruction_Double(ILCODE code, Double arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Double.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Double.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} double:{1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_Label
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_Label : ILInstruction
    {
        internal Label Arg;

        //------------------------------------------------------------
        // ILInstruction_Label Constructor
        //------------------------------------------------------------
        internal ILInstruction_Label() { }

        internal ILInstruction_Label(ILCODE code, Label arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_Label.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_Label.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_LabelArray
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_LabelArray : ILInstruction
    {
        internal Label[] Arg;

        //------------------------------------------------------------
        // ILInstruction_LabelArray Constructor
        //------------------------------------------------------------
        internal ILInstruction_LabelArray() { }

        internal ILInstruction_LabelArray(ILCODE code, Label[] arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_LabelArray.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_LabelArray.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_FieldInfo
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_FieldInfo : ILInstruction
    {
        internal FieldInfo Arg;

        //------------------------------------------------------------
        // ILInstruction_FieldInfo Constructor
        //------------------------------------------------------------
        internal ILInstruction_FieldInfo() { }

        internal ILInstruction_FieldInfo(ILCODE code, FieldInfo arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_FieldInfo.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_FieldInfo.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            //sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
            sb.AppendFormat(
                "{0} FieldInfo: {1}\n", this.Code, this.Arg.Name);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_String
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_String : ILInstruction
    {
        internal String Arg;

        //------------------------------------------------------------
        // ILInstruction_String Constructor
        //------------------------------------------------------------
        internal ILInstruction_String() { }

        internal ILInstruction_String(ILCODE code, String arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_String.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_String.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class ILInstruction_LocalBuilder
    //
    /// <summary></summary>
    //======================================================================
    internal class ILInstruction_LocalBuilder : ILInstruction
    {
        internal LocalBuilder Arg;

        //------------------------------------------------------------
        // ILInstruction_LocalBuilder Constructor
        //------------------------------------------------------------
        internal ILInstruction_LocalBuilder() { }

        internal ILInstruction_LocalBuilder(ILCODE code, LocalBuilder arg)
            : base(code)
        {
            this.Arg = arg;
        }

        //------------------------------------------------------------
        // ILInstruction_LocalBuilder.Emit
        //
        /// <summary></summary>
        /// <param name="gen"></param>
        //------------------------------------------------------------
        internal override void Emit(ILGenerator gen)
        {
            OpCode opCode;
            if (ILGENREC.ILCodeToOpCode(this.Code, out opCode))
            {
                gen.Emit(opCode, this.Arg);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ILInstruction_LocalBuilder.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            sb.AppendFormat("{0} {1}\n", this.Code, this.Arg);
        }
#endif
    }

    //======================================================================
    // class REFENCODING
    //
    /// <summary></summary>
    //======================================================================
    internal class REFENCODING
    {
        internal byte b1;
        internal byte b2;

        internal REFENCODING(byte x1, byte x2)
        {
            this.b1 = x1;
            this.b2 = x2;
        }
    }

    //======================================================================
    // class SourceExtent
    //
    /// <summary></summary>
    //======================================================================
    internal class SourceExtent
    {
        //------------------------------------------------------------
        // SourceExtent Fields and Properties
        //------------------------------------------------------------
        internal INFILESYM InFileSym;               // * infile;
        internal POSDATA BeginPos = new POSDATA();  // begin;
        internal POSDATA EndPos = new POSDATA();    // end;
        internal bool NoDebugInfo;                  // fNoDebugInfo;
        internal bool ProhibitMerge;                // fProhibitMerge;

        internal bool IsValid
        {
            get { return NoDebugInfo || (InFileSym != null); }  // IsValid()
        }

        internal bool IsValidSource
        {
            get { return InFileSym != null; }   // IsValidSource()
        }

        internal bool IsMergeAllowed
        {
            get { return !ProhibitMerge; }  // IsMergeAllowed()
        }

        //------------------------------------------------------------
        // SourceExtent Constructor
        //------------------------------------------------------------
        internal SourceExtent()
        {
            SetInvalid();
        }

        //------------------------------------------------------------
        // SourceExtent.SetInvalid
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void SetInvalid()
        {
            this.InFileSym = null;
            this.BeginPos.SetUninitialized();
            this.EndPos.SetUninitialized();
            this.NoDebugInfo = false;
            this.ProhibitMerge = false;
        }

        //------------------------------------------------------------
        // SourceExtent.SetHiddenInvalidSource
        //
        /// <summary>
        /// Initialize fields.
        /// </summary>
        //------------------------------------------------------------
        internal void SetHiddenInvalidSource()
        {
            this.InFileSym = null;
            this.BeginPos.SetUninitialized();
            this.EndPos.SetUninitialized();
            this.NoDebugInfo = true;
        }

        //------------------------------------------------------------
        // SourceExtent.SetProhibitMerge
        //
        /// <summary>
        /// <para>same to ProhibitMerge() of sscli.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void SetProhibitMerge()
        {
            this.ProhibitMerge = true;
        }

        //------------------------------------------------------------
        // SourceExtent.Equal (static)
        //
        /// <summary></summary>
        /// <param name="extent1"></param>
        /// <param name="extent2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool Equal(SourceExtent extent1, SourceExtent extent2)
        {
            if (extent1 == null || extent2 == null) return false;
            if (extent1.NoDebugInfo && extent2.NoDebugInfo)   return true;
            return (
                extent1.InFileSym == extent2.InFileSym &&
                extent1.BeginPos == extent2.BeginPos &&
                extent1.EndPos == extent2.EndPos &&
                extent1.NoDebugInfo == extent2.NoDebugInfo
                );
        }

        //------------------------------------------------------------
        // SourceExtent.Equals (override)
        //
        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override public bool Equals(object obj)
        {
            SourceExtent other = obj as SourceExtent;
            if (obj != null)
            {
                return SourceExtent.Equal(this, other);
            }
            return false;
        }

        //------------------------------------------------------------
        // SourceExtent.GetHashCode (override)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override public int GetHashCode()
        {
            return (
                (this.InFileSym != null ? this.InFileSym.GetHashCode() : 0) +
                this.BeginPos.GetHashCode() +
                this.EndPos.GetHashCode() +
                this.NoDebugInfo.GetHashCode()) & 0x7FFFFFFF;
        }
    }

    //======================================================================
    // class DEBUGINFO
    //
    /// <summary></summary>
    //======================================================================
    internal class DEBUGINFO
    {
        internal SourceExtent Extent = new SourceExtent();  // extent;
        internal BBLOCK EndBBlock = null;                   // *endBlock;    
        internal BBLOCK BeginBBlock = null;                 // *beginBlock;
        internal int BeginOffset = 0;                       // unsigned short beginOffset;
        internal int EndOffset = 0;                         // unsigned short endOffset;
        internal DEBUGINFO Next = null;                     // *next;
        internal DEBUGINFO Prev = null;                     // *prev;
        internal bool AlreadyAdjusted = false;
    }

    //======================================================================
    // class SWITCHDESTGOTO
    //
    /// <summary></summary>
    //======================================================================
    internal class SWITCHDESTGOTO
    {
        internal BBLOCK DestBBlock = null;  // * dest;
        internal bool JumpIntoTry = false;  // jumpIntoTry:1;
    }

    //======================================================================
    // class SWITCHDEST
    //
    /// <summary>
    /// <para>Has an Array of SWITCHDESTGOTO instances.</para>
    /// <para>SWITCHDESTGOTO has a BBlock field.</para>
    /// </summary>
    //======================================================================
    internal class SWITCHDEST
    {
        //internal SWITCHDESTGOTO[] BBlockArray = null;
        // SWITCHDESTGOTO blocks[0]; // this is actually of size "count"
        internal List<SWITCHDESTGOTO> BBlockList = new List<SWITCHDESTGOTO>();

        internal int Count
        {
            get { return this.BBlockList.Count; }
        }
    }

    //======================================================================
    // class BBLOCK
    //
    /// <summary></summary>
    //======================================================================
    internal class BBLOCK
    {
        //------------------------------------------------------------
        // BBLOCK Fields and Properties
        //------------------------------------------------------------
        internal const int SIZE = 1024;
        internal const int TOPOFF = 20;
        internal const int PREBUFFER = 10;

        internal readonly int BBlockID = CObjectID.GenerateID();

        //union {
        //    BBLOCK * jumpDest;
        //    SWITCHDEST * switchDest;
        //    SYM * sym;
        //};

        internal BBLOCK JumpDestBBlock = null;  // * jumpDest;
        internal SWITCHDEST SwitchDest = null;  // * switchDest;
        internal SYM Sym = null;                // * sym;

        internal BBLOCK Next = null;    // * next;
        internal int Order = 0;         // order;

        //union {
        //    unsigned startOffset;
        //    BBLOCK * markedWith;
        //    SWITCHDESTGOTO * markedWithSwitch;
        //};

        internal int StartOffset = 0;                   // unsigned startOffset;
        internal BBLOCK MarkedWithBBlock = null;        // * markedWith;
        internal SWITCHDESTGOTO MarkedWithSwith = null; // * markedWithSwitch;

        internal List<ILInstruction> CodeList = null;   // BYTE * code;
        internal DEBUGINFO DebugInfo = null;            // * debugInfo;

        internal int CurrentLength = 0;             // size_t curLen;
        internal ILCODE ExitIL = ILCODE.ILLEGAL;    // exitIL : 16;
        internal ILCODE ExitILRev=ILCODE.ILLEGAL;   // exitILRev : 16;

        // post op && op && pre op
        internal bool Reachable = false;    // reachable:1; 

        // post op
        internal bool LargeJump = false;    // largeJump:1;

        // pre op && op
        internal bool GotoBlocked = false;          // gotoBlocked:1;
        internal bool JumpIntoTry = false;          // jumpIntoTry:1;
        //internal bool StartsCatchOrFinally = false; //startsCatchOrFinally:1;
        internal bool StartsException = false;      // startsTry:1;
        internal bool StartsCatch = false;          // (hirano567@hotmail.co.jp)
        internal bool StartsFinally = false;        // (hirano567@hotmail.co.jp)
        internal bool EndsException = false;        // endsFinally:1;
        internal bool HasLeaveTarget = false;       // leaveTarget:1;

        internal bool IsDestination = false;        // (hirano567@hotmail.co.jp)

        internal int TryNestingCount = 0;    // tryNesting;
        internal int LeaveNestingCount = 0;  // leaveNesting;
        internal TYPESYM catchTypeSym = null;

        internal bool IsNOP
        {
            get // isNOP()
            {
                return (
                    this.ExitIL == ILCODE.next &&
                    this.CurrentLength == 0 &&
                    !this.StartsException);
            }
        }

        internal bool IsEmpty
        {
            get { return this.ExitIL == ILCODE.next && this.CurrentLength == 0; }	// isEmpty()
        }

        //------------------------------------------------------------
        // BBLOCK Constructor
        //------------------------------------------------------------
#if DEBUG
        internal BBLOCK()
        {
            if (this.BBlockID == 6176)
            {
                ;
            }
        }
#endif

        //------------------------------------------------------------
        // BBLOCK.MakeEmpty
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void MakeEmpty()
        {
            this.CodeList.Clear();
            this.CurrentLength = 0;

            this.ExitIL = ILCODE.next;
            this.JumpDestBBlock = null;

            this.DebugInfo = null;
        }

        //------------------------------------------------------------
        // BBLOCK.FlipJump
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void FlipJump()
        {
            if (this.ExitILRev == ILCODE.ILLEGAL)
            {
                // Calculate it from exitIL.
                switch (this.ExitIL)
                {
                    case ILCODE.BRFALSE:
                        this.ExitILRev = ILCODE.BRTRUE;
                        break;

                    case ILCODE.BRTRUE:
                        this.ExitILRev = ILCODE.BRFALSE;
                        break;

                    case ILCODE.BEQ:
                        this.ExitILRev = ILCODE.BNE_UN;
                        break;

                    case ILCODE.BNE_UN:
                        this.ExitILRev = ILCODE.BEQ;
                        break;

                    case ILCODE.BLT:
                        this.ExitILRev = ILCODE.BGE;
                        break;

                    case ILCODE.BLE:
                        this.ExitILRev = ILCODE.BGT;
                        break;

                    case ILCODE.BGT:
                        this.ExitILRev = ILCODE.BLE;
                        break;

                    case ILCODE.BGE:
                        this.ExitILRev = ILCODE.BLT;
                        break;

                    case ILCODE.BLT_UN:
                        this.ExitILRev = ILCODE.BGE_UN;
                        break;

                    case ILCODE.BLE_UN:
                        this.ExitILRev = ILCODE.BGT_UN;
                        break;

                    case ILCODE.BGT_UN:
                        this.ExitILRev = ILCODE.BLE_UN;
                        break;

                    case ILCODE.BGE_UN:
                        this.ExitILRev = ILCODE.BLT_UN;
                        break;

                    default:
                        DebugUtil.Assert(false, "bad jump");
                        break;
                }
            }

            // Swap them.
            ILCODE code = this.ExitIL;
            this.ExitIL = this.ExitILRev;
            this.ExitILRev = code;
        }

        // Size = 10 x 4 == 40 bytes... (on 32 bit machines, on 64 bit its more...)

#if DEBUG
        //------------------------------------------------------------
        // BBLOCK.Debug
        //------------------------------------------------------------
        internal void Debug(StringBuilder sb)
        {
            sb.AppendFormat("BBlock ID               : No.{0}\n", BBlockID);
            sb.AppendFormat("Order                   : {0}\n", Order);
            sb.AppendFormat("StartOffset             : {0}\n", StartOffset);
            sb.AppendFormat("CurrentLength           : {0}\n", CurrentLength);
            sb.AppendFormat("IsNOP                   : {0}\n", IsNOP);
            if (this.GotoBlocked)
            {
                sb.Append("GotoBlocked             : True\n");
            }
            if (this.JumpIntoTry)
            {
                sb.Append("JumpIntoTry             : True\n");
            }
            //if (this.StartsCatchOrFinally)
            //{
            //    sb.Append("StartsCatchOrFinally    : True\n");
            //}
            if (this.StartsException)
            {
                sb.Append("StartsException         : True\n");
            }
            if (this.StartsCatch)
            {
                sb.Append("StartsCatch             : True\n");
            }
            if (this.StartsFinally)
            {
                sb.Append("StartsFinally           : True\n");
            }
            if (this.IsDestination)
            {
                sb.Append("IsDestination           : True\n");
            }
            if (this.TryNestingCount>0)
            {
                sb.AppendFormat("TryNestingCount         : {0}\n", TryNestingCount);
            }
            if (this.LeaveNestingCount > 0)
            {
                sb.AppendFormat("LeaveNestingCount       : {0}\n", LeaveNestingCount);
            }
            sb.Append("\n");

            for (int i = 0; i < CodeList.Count; ++i)
            {
                CodeList[i].Debug(sb);
            }

            sb.Append("\n");
            if (JumpDestBBlock != null)
            {
                sb.AppendFormat("JumpDestBBlock  : No.{0}\n", JumpDestBBlock.BBlockID);
            }
            else
            {
                sb.Append("JumpDestBBlock  :\n");
            }
            if (SwitchDest != null)
            {
                sb.Append("SwitchDest      :\n");
            }
            else
            {
                sb.Append("SwitchDest      :\n");
            }
            if (Sym != null)
            {
                sb.AppendFormat("Sym             : No.{0}\n", Sym.SymID);
            }
            else
            {
                sb.Append("Sym             :\n");
            }

            if (Next != null)
            {
                sb.AppendFormat("Next            : No.{0}\n", Next.BBlockID);
            }
            else
            {
                sb.Append("Next            :\n");
            }
            if (this.EndsException)
            {
                sb.Append("EndsException           : True\n");
            }
            if (this.HasLeaveTarget)
            {
                sb.Append("HasLeaveTarget          : True\n");
            }
            sb.AppendFormat("ExitIL          : {0}\n", ExitIL);
            sb.AppendFormat("ExitILRev       : {0}\n", ExitILRev);
        }
#endif
    }

    // These are defined as const fields of BBLOCK
    //#define BB_SIZE 1024
    //#define BB_TOPOFF 20
    //#define BB_PREBUFFER 10

    //======================================================================
    // class HANDLERINFO
    //======================================================================
    internal class HANDLERINFO
    {
        /// <summary>
        /// beginning of try
        /// </summary>
        internal BBLOCK TryBeginBBlock = null;      // * tryBegin;

        /// <summary>
        /// end of try
        /// </summary>
        internal BBLOCK TryEndBBlock = null;        // * tryEnd;

        /// <summary>
        /// beginning of handler
        /// </summary>
        internal BBLOCK HandlerBeginBBlock = null;  // * handBegin;

        /// <summary>
        /// end of handler
        /// </summary>
        internal BBLOCK HandlerEndBBlock = null;    // * handEnd;

        /// <summary>
        /// <para>(ssclil) value of 1 indicates fault (instead of finally)</para>
        /// <para>Use invalidTypeSym to indicate fault.</para>
        /// </summary>
        internal TYPESYM TypeSym = null;    // * type;

        internal HANDLERINFO Next = null;   // * next;

        internal bool HandlerShouldIncludeNOP = false;  // handlerShouldIncludeNOP;

        static readonly internal TYPESYM faultTypeSym = new TYPESYM();  // (void*)1

        internal bool IsTryCatch
        {
            get
            {
                return (
                    TypeSym != null &&
                    TypeSym != faultTypeSym // TypeSym != (void*)1
                    );
            }
        }

        internal bool IsTryFinally
        {
            get { return TypeSym == null; }
        }

        internal bool IsTryFault
        {
            get { return TypeSym == faultTypeSym; } // TypeSym == (void*)1;
        }
    }

    //typedef LOCSLOTINFO * PSLOT;

    //static const int TEMPBUCKETSIZE = 16; 

    //======================================================================
    // class TEMPBUCKET
    //
    /// <summary>
    /// Has temporary LOCSLOTINFO instances.
    /// </summary>
    //======================================================================
    internal class TEMPBUCKET
    {
        internal const int SIZE = 16; // TEMPBUCKETSIZE

        internal LOCSLOTINFO[] Slots = new LOCSLOTINFO[SIZE];
        internal TEMPBUCKET Next = null; // * next;
    }

    //======================================================================
    // class SWITCHBUCKET
    //======================================================================
    internal class SWITCHBUCKET
    {
        internal ulong FirstMember = 0; // unsigned __int64 firstMember;
        internal ulong LastMember = 0;  // unsigned __int64 lastMember;
        // ie, member after last, so that (last-first)+1 == slots

        /// <summary>
        /// actual members present in slots
        /// </summary>
        internal int MemberCount = 0;

        //EXPRSWITCHLABEL** labels; // starts w/ firstMember
        internal int FirstIndex;

        internal SWITCHBUCKET PrevBucket = null; // * prevBucket;
    }

    //typedef SWITCHBUCKET * PSWITCHBUCKET;

    //======================================================================
    // class MARKREACHABLEINFO
    //======================================================================
    internal class MARKREACHABLEINFO
    {
        //struct BBSTACK {
        //    BBLOCK *  bbItem[8];
        //    BBSTACK * next;
        //};

        //------------------------------------------------------------
        // MARKREACHABLEINFO Fields and Properties
        //------------------------------------------------------------
        //NRHEAP *  allocator;
        //NRMARK    markBase;
        private Stack<BBLOCK> stack = new Stack<BBLOCK>();// BBSTACK * stack;
        //private Stack<BBLOCK> empty = new Stack<BBLOCK>();// BBSTACK * empty;
        //private int stackIndex;

        //------------------------------------------------------------
        // MARKREACHABLEINFO Constructor
        //------------------------------------------------------------
        internal MARKREACHABLEINFO() { }

        //~MARKREACHABLEINFO();

        //------------------------------------------------------------
        // MARKREACHABLEINFO.MarkAllReachableBB
        //
        /// <summary>
        /// mark all reachable blocks starting from start using the provided marking function
        /// </summary>
        /// <param name="start"></param>
        //------------------------------------------------------------
        internal void MarkAllReachableBB(BBLOCK bblock)
        {
        AGAIN:
            if (!ILGENREC.MarkAsVisited(bblock))
            {
                return;
            }

            switch (bblock.ExitIL)
            {
                case ILCODE.stop:
                case ILCODE.ENDFINALLY:
                case ILCODE.RET:
                    return;

                case ILCODE.next:
                    DebugUtil.Assert(bblock.Next != null);
                    bblock = bblock.Next;
                    goto AGAIN;

                case ILCODE.LEAVE:
                    if (bblock.GotoBlocked)
                    {
                        break;
                    }
                    goto case ILCODE.BR;

                case ILCODE.BR:
                    DebugUtil.Assert(bblock.JumpDestBBlock != null);
                    bblock = bblock.JumpDestBBlock;
                    goto AGAIN;

                case ILCODE.SWITCH:
                    for (int i = 0; i < bblock.SwitchDest.Count; i++)
                    {
                        Push(bblock.SwitchDest.BBlockList[i].DestBBlock);
                    }
                    DebugUtil.Assert(bblock.Next != null);
                    bblock = bblock.Next;
                    goto AGAIN;

                default:
                    Push(bblock.JumpDestBBlock);
                    DebugUtil.Assert(bblock.Next != null);
                    bblock = bblock.Next;
                    goto AGAIN;
            }
        }

        //------------------------------------------------------------
        // MARKREACHABLEINFO.Push
        //------------------------------------------------------------
        internal void Push(BBLOCK block)
        {
            this.stack.Push(block);
        }

        //------------------------------------------------------------
        // MARKREACHABLEINFO.Pop
        //------------------------------------------------------------
        internal BBLOCK Pop()
        {
            try
            {
                return this.stack.Pop();
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }
    }

    //======================================================================
    // class IlSlotInfo
    //
    /// <summary>
    /// <para>(CSharp\SCComp\ILGen.cs)</para>
    /// </summary>
    //======================================================================
    internal class IlSlotInfo
    {
        internal LocalBuilder LocalBuilder = null;

        internal string Name = null;        // NAME * name;
        internal TYPESYM TypeSym;           // * type;
        //internal int IlSlotNum = 0;       // uint ilSlotNum;
        internal TEMP_KIND TempKind = 0;    // tempKind;
        internal bool IsUsed = false;       // fIsUsed;

        internal int IlSlotNum
        {
            get
            {
                return LocalBuilder != null ? LocalBuilder.LocalIndex : -1;
            }
        }
    }

    //// For 64-bit we need to make sure that the struct will stay pointer-aligned
    //// even when allocated sequentially in an array
    //C_ASSERT((sizeof(IlSlotInfo) % sizeof(void*)) == 0);

    //======================================================================
    // enum SpecialDebugPointEnum
    //======================================================================
    internal enum SpecialDebugPointEnum : int
    {
        HiddenCode,
        OpenCurly,
        CloseCurly,
    }
    //typedef SpecialDebugPoint::_Type SpecialDebugPointEnum;

    //#define NO_DEBUG_LINE   (0x00FEEFEE)

    //======================================================================
    // class AccessTask
    //
    /// <summary>
    /// <para>(CSharp\SCComp\IlGen.cs)</para>
    /// </summary>
    //======================================================================
    internal class AccessTask
    {
        //enum _Enum {
        //    Addr = 0x01, // Leave the "address" on the stack.
        //    Load = 0x02, // Load the value. Can combine with Addr.

        //    // Store the value, assuming the "address" is on the stack (below the value to store).
        //    // Not valid with Load or Addr.
        //    Store = 0x04
        //};

        //------------------------------------------------------------
        // enum AccessTask.Enum
        //------------------------------------------------------------
        internal enum Enum : int
        {
            /// <summary>
            /// Leave the "address" on the stack.
            /// </summary>
            Addr = 0x01,

            /// <summary>
            /// Load the value. Can combine with Addr.
            /// </summary>
            Load = 0x02,

            /// <summary>
            /// Store the value, assuming the "address" is on the stack (below the value to store).
            /// Not valid with Load or Addr.
            /// </summary>
            Store = 0x04,
        }

        //typedef _Enum _EnumType;

        //------------------------------------------------------------
        // AccessTask.FValid (static)
        //------------------------------------------------------------
        //static bool FValid(_EnumType flags) { return flags && ((flags == Store) || !(flags & Store)); }
        static internal bool FValid(AccessTask.Enum flags)
        {
#if DEBUG
            return flags != 0 && ((flags == Enum.Store) || (flags & Enum.Store) == 0);
#else
            return true;
#endif // DEBUG
        }

        //------------------------------------------------------------
        // AccessTask.FAddr (static)
        //------------------------------------------------------------
        static internal bool FAddr(AccessTask.Enum flags)
        {
            return (flags & Enum.Addr) != 0;
        }

        //------------------------------------------------------------
        // AccessTask.FLoad (static)
        //------------------------------------------------------------
        static internal bool FLoad(AccessTask.Enum flags)
        {
            return (flags & Enum.Load) != 0;
        }

        //------------------------------------------------------------
        // AccessTask.FStore (static)
        //------------------------------------------------------------
        static internal bool FStore(AccessTask.Enum flags)
        {
            return (flags & Enum.Store) != 0;
        }

        //------------------------------------------------------------
        // AccessTask.FAddrOnly (static)
        //------------------------------------------------------------
        static internal bool FAddrOnly(AccessTask.Enum flags)
        {
            return (flags == Enum.Addr);
        }

        //------------------------------------------------------------
        // AccessTask.FAddrOrLoad (static)
        //------------------------------------------------------------
        static internal bool FAddrOrLoad(AccessTask.Enum flags)
        {
            return (flags & (Enum.Addr | Enum.Load)) != 0;
        }

        //------------------------------------------------------------
        // AccessTask.FLoadOrStore (static)
        //------------------------------------------------------------
        static internal bool FLoadOrStore(AccessTask.Enum flags)
        {
            return (flags & (Enum.Load | Enum.Store)) != 0;
        }

        //------------------------------------------------------------
        // AccessTask.FDup (static)
        //------------------------------------------------------------
        static internal bool FDup(AccessTask.Enum flags)
        {
            return (flags == (Enum.Load | Enum.Addr));
        }
    }

    //DECLARE_FLAGS_TYPE(AccessTask);

    //======================================================================
    // class AddrInfo
    //
    /// <summary>
    /// Has a LOCSLOTINFO field.
    /// </summary>
    //======================================================================
    internal class AddrInfo
    {
        internal LOCSLOTINFO SlotStore = null;  // PSLOT slotStore;
        internal bool IndirectArray = false;    // fIndirectArray;

        internal void Init()
        {
            SlotStore = null;
            IndirectArray = false;
        }
    }

    //======================================================================
    // class MultiOpInfo
    //======================================================================
    internal class MultiOpInfo
    {
        internal AddrInfo Addr = new AddrInfo();    // addr;
        internal bool NeedOld = false;              // fNeedOld;
        internal int GetCount = 0;                  // byte cget;
        internal int ValStackCount = 0;             // cvalStack;
        internal LOCSLOTINFO Slot = null;           // PSLOT slot;
    }

    //======================================================================
    // enum NumericOrderSchemeEnum (knos)
    //
    /// <summary>
    /// <para>In sscli, defined in ilgen.cpp with prefix "knos".</para>
    /// <para>(CSharp\SCComp\ILGen.cs)</para>
    /// </summary>
    //======================================================================
    internal enum NumericOrderSchemeEnum : int
    {
        Signed,
        Unsigned,
        Float
    }

    //======================================================================
    // class ILGENREC
    //
    /// <summary>
    /// Class to generates il codes of methods.
    /// </summary>
    //======================================================================
    internal class ILGENREC
    {
        //friend class MARKREACHABLEINFO;

        //------------------------------------------------------------
        // ILGENREC Fields and Properties (static)
        //
        // These arrays are defined in the end of this class definition.
        //------------------------------------------------------------
        //static private const REFENCODING ILcodes[ILCODE.last];
        //static private const int ILStackOps[ILCODE.last];
        //static private const BYTE ILcodesSize[ILCODE.last];
        //static private const ILCODE ILlsTiny[4][6];
        //static private const ILCODE ILarithInstr[EXPRKIND.ARRLEN - EXPRKIND.ADD + 1];
        //static private const ILCODE ILarithInstrUN[EXPRKIND.ARRLEN - EXPRKIND.ADD + 1];
        //static private const ILCODE ILarithInstrOvf[EXPRKIND.ARRLEN - EXPRKIND.ADD + 1];
        //static private const ILCODE ILarithInstrUNOvf[EXPRKIND.ARRLEN - EXPRKIND.ADD + 1];
        //static private const ILCODE ILstackLoad[FUNDTYPE.COUNT];
        //static private const ILCODE ILstackStore[FUNDTYPE.COUNT];
        //static private const ILCODE ILarrayLoad[FUNDTYPE.COUNT];
        //static private const ILCODE ILarrayStore[FUNDTYPE.COUNT];
        //static private const ILCODE ILaddrLoad[2][2];
        //static private const ILCODE simpleTypeConversions[Util.NUM_EXT_TYPES][Util.NUM_EXT_TYPES];
        //static private const ILCODE simpleTypeConversionsOvf[Util.NUM_EXT_TYPES][Util.NUM_EXT_TYPES];
        //static private const ILCODE simpleTypeConversionsEx[Util.NUM_EXT_TYPES][Util.NUM_EXT_TYPES];

#if DEBUG
        //static private const PWSTR ILnames[ILCODE.last];
#endif

        //------------------------------------------------------------
        // ILGENREC Fields and Properties
        //------------------------------------------------------------
        private METHSYM methodSym = null;       // * method;
        private METHINFO methodInfo = null;     // * info;
        private SCOPESYM localScopeSym = null;  // * localScope;
        private AGGSYM aggSym = null;           // * cls;

        // typedef void *HCEEFILE; // (clr\src\inc\iceefilegen.h)
        private Object hceeFile = null;         // HCEEFILE pFile;

#if DEBUG
        private bool privShouldDumpAllBlocks;
#endif

        //private NRHEAP * allocator;

        private BBLOCK firstBBlock = null;              // * firstBB;
        private BBLOCK currentBBlock = null;            // * currentBB;
        //private BBLOCK inlineBBlock = new BBLOCK();   // inlineBB;  // use currentBBlock.

        private HANDLERINFO handlerInfos = null;        // * handlers;
        private HANDLERINFO lastHandlerInfo = null;     // * lastHandler;
        private LOCSLOTINFO originalException = null;   // PSLOT origException;
        private BBLOCK returnLocationBBlock = null;     // * returnLocation;
        private bool returnHandled = false;
        private int blockedLeave = 0;                   // unsigned blockedLeave;
        private int ehCount = 0;                        // unsigned ehCount;
        private bool closeIndexUsed = false;
        private bool compileForEnc = false;

        private DEBUGINFO currentDebugInfo = null;                  // * curDebugInfo;
        private SourceExtent currentExtent = new SourceExtent();    // extentCurrent;
        private BASENODE currentNode = null;                        // * nodeCurrent;

        private List<LOCSLOTINFO> temporarySlots = new List<LOCSLOTINFO>(); // * temporaries;
        //private TEMPBUCKET temporaryBuckets = null;
        private LOCSLOTINFO temporaryReturnSlotInfo = null; // PSLOT retTemp;
        private int globalFieldCount = 0;

        private int currentStackCount = 0;      // curStack;
        private int maxStackCount = 0;          // maxStack;

        private int finallyNestingCount = 0;    // finallyNesting;

        //private BlobBldrNrHeap * m_pbldrLocalSlotArray;
        private List<IlSlotInfo> localSlotList = null;

        //private   BYTE reusableBuffer[BB_PREBUFFER + BB_SIZE + BB_TOPOFF];

        private COMPILER compiler = null;   // private COMPILER * compiler();

#if DEBUG
        private bool smallBlock = false;
#endif

        //------------------------------------------------------------
        // ILGENREC.CallAsVirtual (static)
        //
        /// <summary>
        /// Should we use the "call" or "callvirt" opcode? We use "callvirt" even on non-virtual
        /// instance methods (exception base calls or structs) to get a null check.
        /// </summary>
        /// <param name="meth"></param>
        /// <param name="expr"></param>
        /// <param name="isBaseCall"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool CallAsVirtual(METHSYM meth, EXPR expr, bool isBaseCall)
        {
            DebugUtil.Assert((!meth.IsStatic) == (expr != null));

            if (meth.IsStatic || isBaseCall || (expr.Flags & EXPRFLAG.CANTBENULL) != 0 && !meth.IsVirtual)
            {
                return false;
            }

            FUNDTYPE ft = expr.TypeSym.FundamentalType();
            return (ft == FUNDTYPE.REF || ft == FUNDTYPE.VAR);
        }

        //------------------------------------------------------------
        // ILGENREC.MarkAsVisited (static)
        //
        /// <summary>
        /// mark a given block as visited,
        /// and reuturn true if this is the first time for that block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool MarkAsVisited(BBLOCK bblock)
        {
            if (bblock.Reachable)
            {
                return false;
            }
            bblock.Reachable = true;
            return true;
        }

        //------------------------------------------------------------
        // ILGENREC.CodeForCompare (static)
        //
        /// <summary></summary>
        /// <param name="exprKind"></param>
        /// <param name="typeSym"></param>
        /// <param name="sense"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static private ILCODE CodeForCompare(EXPRKIND exprKind, TYPESYM typeSym, ref bool sense)
        {
            int senseVal = (sense ? 1 : 0);
            DebugUtil.Assert((senseVal & ~1) == 0);

            if (exprKind <= EXPRKIND.NE)
            {
                // EQ and NE don't depend on the nos.
                DebugUtil.Assert(exprKind == EXPRKIND.EQ || exprKind == EXPRKIND.NE);
                if (exprKind == EXPRKIND.NE)
                {
                    //*psense ^= true;
                    senseVal ^= 1;
                    sense = (senseVal != 0);
                }
                return ILCODE.CEQ;
            }

            int dek = (int)(exprKind - EXPRKIND.LT);
            DebugUtil.Assert(0 <= dek && dek < 4);

            // The sense is inverted for odd values of dek.
            senseVal ^= (dek & 1);
            sense = (senseVal != 0);

            NumericOrderSchemeEnum nos;
            FUNDTYPE ft = typeSym.FundamentalType();

            if (ft == FUNDTYPE.R4 || ft == FUNDTYPE.R8)
            {
                nos = NumericOrderSchemeEnum.Float;
            }
            else if (typeSym.IsUnsigned())
            {
                nos = NumericOrderSchemeEnum.Unsigned;
            }
            else
            {
                nos = NumericOrderSchemeEnum.Signed;
            }

            //return FetchAtIndex(rgcodeCmp, dek + 4 * (int)nos);
            DebugUtil.Assert(dek + 4 * (int)nos < rgcodeCmp.Length);
            return rgcodeCmp[dek + 4 * (int)nos];
        }

        //------------------------------------------------------------
        // ILGENREC Constructor
        //
        /// <summary>
        /// constructor, just use the local allocator...
        /// </summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        internal ILGENREC(COMPILER comp)
        {
            //allocator = &(compiler()->localSymAlloc);
            DebugUtil.Assert(comp != null);
            this.compiler = comp;
        }

        //------------------------------------------------------------
        // ILGENREC.Compile
        //
        /// <summary>
        /// the method visible to the world...
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="methodInfo"></param>
        /// <param name="treeExpr"></param>
        //------------------------------------------------------------
        internal void Compile(METHSYM methodSym, METHINFO methodInfo, EXPR treeExpr)
        {
#if DEBUG
            InitDebugPrintf();

            if (COMPILER.GetRegDWORD("SmallBlock") != 0)
            {
                smallBlock = true;
            }
            else
            {
                smallBlock = false;
            }

            StringBuilder sb = new StringBuilder();
#endif

            //--------------------------------------------------------
            // save our context...
            //--------------------------------------------------------
            this.methodSym = methodSym;
            this.aggSym = methodSym.ClassSym;
            this.methodInfo = methodInfo;

            InitDumpingAllBlocks();
#if DEBUG
            privShouldDumpAllBlocks = true;
#endif

            this.compileForEnc = !compiler.OptionManager.Optimize;

            InitFirstBB();
            InitBBlock(this.currentBBlock); //InitInlineBB();

            //BlobBldrNrHeap bldrLocalSlotArray(&compiler.localSymAlloc);
            //m_pbldrLocalSlotArray = &bldrLocalSlotArray;
            this.localSlotList = new List<IlSlotInfo>();
            InitLocalsFromEnc();

            //--------------------------------------------------------
            // initialize temporaries and locals and params
            //--------------------------------------------------------
            //this.temporaryBuckets = null;
            this.temporarySlots.Clear();
            this.localScopeSym = GetLocalScope();
            if (this.localScopeSym != null)
            {
                AssignLocals(this.localScopeSym);
            }
            AssignParams();

            this.globalFieldCount = 0;
            this.currentStackCount = 0;
            this.maxStackCount = 0;
            this.handlerInfos = null;
            this.finallyNestingCount = 0;
            this.lastHandlerInfo = null;
            this.ehCount = 0;
            this.returnLocationBBlock = null;
            this.closeIndexUsed = false;
            this.blockedLeave = 0;
            this.originalException = null;
            this.currentDebugInfo = null;
            this.returnHandled = false;

            if (!methodSym.ReturnTypeSym.IsVoidType &&  // != compiler.MainSymbolManager.VoidSym &&
                (methodInfo.HasReturnAsLeave || !compiler.OptionManager.Optimize))
            {
                this.temporaryReturnSlotInfo = AllocTemporary(methodSym.ReturnTypeSym, TEMP_KIND.RETURN);
            }
            else
            {
                this.temporaryReturnSlotInfo = null;
            }

            this.currentExtent.SetInvalid();
            this.currentNode = null;

            //--------------------------------------------------------
            // generate the prologue
            //--------------------------------------------------------
            GenPrologue(treeExpr as EXPRBLOCK);

            //--------------------------------------------------------
            // generate the code
            //--------------------------------------------------------
#if DEBUG
            sb.Length = 0;
            DebugUtil.DebugSymsOutput(sb);
            sb.Length = 0;
            DebugUtil.DebugExprsOutput(sb);
#endif

            GenBlock(treeExpr as EXPRBLOCK);

#if DEBUG
            sb.Length = 0;
            ReflectionUtil.DebugCodeList(sb, this.firstBBlock);   // inlineBBlock
#endif
            DebugUtil.Assert(this.finallyNestingCount == 0);
            DebugUtil.Assert(this.finallyNestingCount == 0);

            // do the COM+ magic to emit the method:
            compiler.SetLocation(COMPILER.STAGE.EMITIL);

            //--------------------------------------------------------
            // Calculate the code size.
            //--------------------------------------------------------
            int codeSize = GetFinalCodeSize();
#if DEBUG
            sb.Length = 0;
            ReflectionUtil.DebugCodeList(sb, this.firstBBlock);   // inlineBBlock
#endif
            EmitMethodBody(methodSym, this.firstBBlock);    // inlineBBlock

            //--------------------------------------------------------
            // make sure no temps leaked
            //--------------------------------------------------------
#if DEBUG
            //VerifyAllTempsFree();
#endif

            ////DWORD dwFlags = 0;
            //int dwFlags = 0;
            //COR_ILMETHOD_FAT fatHeader;
            //fatHeader.SetMaxStack(maxStack);
            //fatHeader.SetCodeSize(codeSize);
            //if (bldrLocalSlotArray.Length() > 0)
            //{
            //    fatHeader.SetLocalVarSigTok(computeLocalSignature());
            //    dwFlags |= CorILMethod_InitLocals;
            //}
            //else
            //{
            //    fatHeader.SetLocalVarSigTok(mdTokenNil);
            //}
            //fatHeader.SetFlags(dwFlags);

            //COR_ILMETHOD_SECT_EH_CLAUSE_FAT * clauses = STACK_ALLOC(COR_ILMETHOD_SECT_EH_CLAUSE_FAT, ehCount);
            //copyHandlers(clauses);

            //bool moreSections = ehCount != 0;
            //unsigned alignmentJunk;
            //if (moreSections) {
            //    alignmentJunk = RoundUp4((int)codeSize) - codeSize;
            //    codeSize += alignmentJunk;
            //} else {
            //    alignmentJunk = 0;
            //}

            //unsigned headerSize = COR_ILMETHOD::Size(&fatHeader, moreSections);
            //unsigned ehSize = COR_ILMETHOD_SECT_EH::Size(ehCount, clauses);
            //unsigned totalSize = headerSize + codeSize + ehSize;
            //bool align = headerSize != 1;

            //BYTE * buffer = (BYTE*) compiler.emitter.EmitMethodRVA(methodSym, totalSize, align ? 4 : 1);

            //emitDebugInfo(codeSize - alignmentJunk, fatHeader.GetLocalVarSigTok());

#if DEBUG
            //BYTE * bufferBeg, * endBuffer;

            //bufferBeg = buffer;
            //endBuffer = &buffer[totalSize];
#endif

            //buffer += COR_ILMETHOD::Emit(headerSize, &fatHeader, moreSections, buffer);

            //buffer = copyCode(buffer);

            //memset (buffer, 0, alignmentJunk);
            //buffer += alignmentJunk;

            //buffer += COR_ILMETHOD_SECT_EH::Emit(ehSize, ehCount, clauses, false, buffer);

            //DebugUtil.Assert(buffer == endBuffer);
            //m_pbldrLocalSlotArray = null;
            this.localSlotList = null;
        }

        //------------------------------------------------------------
        // ILGENREC.CodeForJump (static)
        //
        /// <summary>
        /// The opcodes to emit based on ek, nos, and sense.
        /// </summary>
        //------------------------------------------------------------
        static private ILCODE[] rgcodeJmp =
        {	//	<          		<=          	>				>=
            ILCODE.BLT,		ILCODE.BLE,		ILCODE.BGT,		ILCODE.BGE,		// Signed
            ILCODE.BGE,		ILCODE.BGT,		ILCODE.BLE,		ILCODE.BLT,		// Signed Invert
            ILCODE.BLT_UN,	ILCODE.BLE_UN,	ILCODE.BGT_UN,	ILCODE.BGE_UN,	// Unsigned
            ILCODE.BGE_UN,	ILCODE.BGT_UN,	ILCODE.BLE_UN,	ILCODE.BLT_UN,	// Unsigned Invert
            ILCODE.BLT,		ILCODE.BLE,		ILCODE.BGT,		ILCODE.BGE,		// Float
            ILCODE.BGE_UN,	ILCODE.BGT_UN,	ILCODE.BLE_UN,	ILCODE.BLT_UN,	// Float Invert
        };

        //------------------------------------------------------------
        // ILGENREC.CodeForJump (static)
        //
        /// <summary></summary>
        /// <param name="ek"></param>
        /// <param name="typeSym"></param>
        /// <param name="sense"></param>
        /// <param name="codeRev"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static private ILCODE CodeForJump(EXPRKIND ek, TYPESYM typeSym, bool sense, ref ILCODE codeRev)
        {

            // In CSharp\SCComp\ExprList.cs	
            //   EQ,
            //   RELATIONAL_MIN = EQ,
            //   NE,
            //   LT,
            //   LE,
            //   GT,
            //   GE,
            //   RELATIONAL_MAX = GE,

            DebugUtil.Assert(EXPRKIND.EQ <= ek && ek <= EXPRKIND.GE);
            //DebugUtil.Assert(((int)sense & ~1) == 0);
            int senseVal = (sense ? 1 : 0);

            if (ek <= EXPRKIND.NE)
            {
                // EQ and NE don't depend on the nos.
                DebugUtil.Assert(ek == EXPRKIND.EQ || ek == EXPRKIND.NE);
                codeRev = ILCODE.ILLEGAL; // Not needed.
                return ((int)(ek - EXPRKIND.EQ) ^ senseVal) != 0 ? ILCODE.BEQ : ILCODE.BNE_UN;
            }

            int dek = ek - EXPRKIND.LT;
            DebugUtil.Assert(0 <= dek && dek < 4);

            NumericOrderSchemeEnum nos;
            FUNDTYPE ft = typeSym.FundamentalType();

            if (ft == FUNDTYPE.R4 || ft == FUNDTYPE.R8)
            {
                nos = NumericOrderSchemeEnum.Float;
            }
            else if (typeSym.IsUnsigned())
            {
                nos = NumericOrderSchemeEnum.Unsigned;
            }
            else
            {
                nos = NumericOrderSchemeEnum.Signed;
            }

            codeRev = rgcodeJmp[dek + 8 * (int)nos + 4 * senseVal];
            return rgcodeJmp[dek + 8 * (int)nos + 4 * (1 - senseVal)];
        }

        //------------------------------------------------------------
        // ILGENREC.TrackDebugInfo
        //
        /// <summary>
        /// Return true if we should gererate debug informations.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool TrackDebugInfo()
        {
            return (
                compiler.OptionManager.GenerateDebugInfo &&
                (this.methodInfo == null || !this.methodInfo.NoDebugInfo));
        }

        //------------------------------------------------------------
        // ILGENREC.MarkStackMax
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void MarkStackMax()
        {
            if (this.maxStackCount < this.currentStackCount)
            {
                this.maxStackCount = this.currentStackCount;
            }
            DebugUtil.Assert(this.maxStackCount >= 0 && this.currentStackCount >= 0);
        }

        //------------------------------------------------------------
        // ILGENREC.GetOpenIndex
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetOpenIndex()
        {
            if (this.methodSym == null)
            {
                return -1;
            }

            BASENODE treeNode = this.methodSym.GetParseTree();
            if (treeNode == null)
            {
                return -1;
            }

            switch (treeNode.Kind)
            {
                case NODEKIND.ACCESSOR:
                    return (treeNode as ACCESSORNODE).OpenCurlyIndex;

                case NODEKIND.METHOD:
                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                case NODEKIND.OPERATOR:
                    return treeNode.AsANYMETHOD.OpenIndex;

                default:
                    return -1;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GetCloseIndex
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetCloseIndex()
        {
            BLOCKNODE block;

            if (this.methodSym == null)
            {
                return -1;
            }

            BASENODE tree = this.methodSym.GetParseTree();
            if (tree == null)
            {
                return -1;
            }

            switch (tree.Kind)
            {
                case NODEKIND.ACCESSOR:
                    return (tree as ACCESSORNODE).CloseCurlyIndex;

                case NODEKIND.METHOD:
                case NODEKIND.CTOR:
                case NODEKIND.OPERATOR:
                case NODEKIND.DTOR:
                    block = tree.AsANYMETHOD.BodyNode;
                    if (block == null)
                    {
                        return -1;
                    }
                    return block.CloseCurlyIndex;

                default:
                    return -1;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GetLocalScope
        //
        /// <summary>
        /// return the scope just below the outermost param scope, if any
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private SCOPESYM GetLocalScope()
        {
            SYM current = this.methodInfo.OuterScopeSym.FirstChildSym;
            while (current != null)
            {
                if (current.IsSCOPESYM && ((current as SCOPESYM).ScopeFlags & SCOPEFLAGS.ARGSCOPE) == 0)
                {
                    return current as SCOPESYM;
                }
                current = current.NextSym;
            }
            return null;
        }

        //------------------------------------------------------------
        // ILGENREC.GetVarArgMethod
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <param name="argsExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM GetVarArgMethod(METHSYM methodSym, EXPR argsExpr)
        {
            DebugUtil.Assert(methodSym.IsVarargs);
            DebugUtil.Assert(!methodSym.IsFAKEMETHSYM);

            int realCount = GetArgCount(argsExpr);
            int fixedCount = methodSym.ParameterTypes.Count - 1;
            DebugUtil.Assert(realCount >= fixedCount);

            //TYPESYM ** sig;

            if (realCount > fixedCount)
            {
                ++realCount; // for the sentinel
            }

            //sig = STACK_ALLOC(TYPESYM*, realCount);
            List<TYPESYM> sig = new List<TYPESYM>();
            methodSym.ParameterTypes.CopyItems(0, fixedCount, sig);

            if (realCount > fixedCount)
            {
                //TYPESYM ** sigNew = sig + fixedCount;
                int iSigNew = fixedCount;

                //*sigNew = compiler.MainSymbolManager.ArgListSym;
                //sigNew++;
                sig.Add(compiler.MainSymbolManager.ArgListSym);
                ++iSigNew;
                int originalCount = fixedCount;

                EXPR expr = argsExpr;
                while (expr != null)
                {
                    EXPR arg;
                    if (expr.Kind == EXPRKIND.LIST)
                    {
                        arg = expr.AsBIN.Operand1;
                        expr = expr.AsBIN.Operand2;
                    }
                    else
                    {
                        arg = expr;
                        expr = null;
                    }

                    --originalCount;
                    if (originalCount < 0)
                    {
                        if (arg.TypeSym == compiler.MainSymbolManager.NullSym)
                        {
                            sig.Add(compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true));
                        }
                        else
                        {
                            sig.Add(arg.TypeSym);
                        }
                        //++sigNew;
                    }
                }
            }

            TypeArray ptaSig = compiler.MainSymbolManager.AllocParams(sig);
            DebugUtil.Assert(methodSym.Kind == SYMKIND.METHSYM);
            FAKEMETHSYM previous = compiler.MainSymbolManager.LookupAggMember(
                methodSym.Name,
                methodSym.ClassSym,
                SYMBMASK.FAKEMETHSYM) as FAKEMETHSYM;

            while (
                previous != null &&
                (previous.ParentMethodSym != methodSym ||
                previous.ReturnTypeSym != methodSym.ReturnTypeSym ||
                previous.ParameterTypes != ptaSig))
            {
                previous = BSYMMGR.LookupNextSym(
                    previous,
                    methodSym.ParentSym,
                    SYMBMASK.FAKEMETHSYM) as FAKEMETHSYM;
            }

            if (previous != null)
            {
                return previous;
            }

            previous = compiler.MainSymbolManager.CreateGlobalSym(
                SYMKIND.FAKEMETHSYM,
                methodSym.Name,
                methodSym.ParentSym) as FAKEMETHSYM;

            methodSym.CopyInto(previous, null, compiler);
            previous.ParentMethodSym = methodSym;
            previous.ParameterTypes = ptaSig;

            return previous;
        }

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        //private TYPESYM ** getLocalTypes(TYPESYM ** array, SCOPESYM * scope);

        //------------------------------------------------------------
        // ILGENREC.AssignLocals
        //
        /// <summary>
        /// assign slots to all locals for a given scope and starting with
        /// a provided index.  returns the next available slot
        /// </summary>
        /// <param name="scopeSym"></param>
        //------------------------------------------------------------
        private void AssignLocals(SCOPESYM scopeSym)
        {
            LOCVARSYM localSym;

            SYM currentSym = scopeSym.FirstChildSym;
            while (currentSym != null)
            {
                switch (currentSym.Kind)
                {
                    case SYMKIND.LOCVARSYM:
                        localSym = currentSym as LOCVARSYM;
                        if (!localSym.IsConst &&
                            (!compiler.OptionManager.Optimize || localSym.LocSlotInfo.IsUsed))
                        {
                            if (localSym.IsIteratorLocal)
                            {
                                localSym.LocSlotInfo.Index = -1;
                                localSym.LocSlotInfo.TypeSym = null;
                            }
                            else
                            {
                                TYPESYM typeSym = localSym.TypeSym;
                                if (localSym.LocSlotInfo.IsPinned)
                                {
                                    typeSym = compiler.MainSymbolManager.GetPinnedType(typeSym);
                                }

                                localSym.LocSlotInfo.LocalBuilder = GetLocalBuilder(
                                    localSym.Name,
                                    typeSym,
                                    localSym.LocSlotInfo.IsPinned);

                                localSym.LocSlotInfo.TypeSym = localSym.TypeSym;
                                localSym.LocSlotInfo.Index = localSym.LocSlotInfo.LocalBuilder.LocalIndex;
                            }
                            localSym.LocSlotInfo.IsParameter = false;
                            localSym.FirstUsedPos.SetUninitialized();
                        }
                        else
                        {
                            localSym.LocSlotInfo.TypeSym = null;
                            localSym.LocSlotInfo.Index = 0;
                        }
                        break;

                    case SYMKIND.SCOPESYM:
                        AssignLocals(currentSym as SCOPESYM);
                        break;

                    case SYMKIND.ANONSCOPESYM:
                    case SYMKIND.CACHESYM:
                    case SYMKIND.LABELSYM:
                        break;

                    default:
                        DebugUtil.VsFail("Unexpected sym kind");
                        break;
                }
                currentSym = currentSym.NextSym;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.InitLocal
        //
        /// <summary></summary>
        /// <param name="slot"></param>
        /// <param name="type"></param>
        //------------------------------------------------------------
        private void InitLocal(LOCSLOTINFO slot, TYPESYM type)
        {
            if (type == null)
            {
                type = slot.TypeSym;
                DebugUtil.Assert(type != null);
            }

            if (type.FundamentalType() != FUNDTYPE.STRUCT)
            // || type.getPredefType() == PREDEFTYPE.INTPTR) {
            {
                GenZero(null, type);
                DumpLocal(slot, true);
            }
            else
            {
                GenSlotAddress(slot, false);
                //PutOpcode(ILCODE.INITOBJ);
                //EmitTypeToken(type);
                PutILInstruction(ILCODE.INITOBJ, type);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.AssignParams
        //
        /// <summary>
        /// assign slots to all parameters of the method being compiled...
        /// </summary>
        //------------------------------------------------------------
        private void AssignParams()
        {
            int currentSlot = 0;    // unsigned curSlot = 0;

            SYM currentSym = this.methodInfo.OuterScopeSym.FirstChildSym;

            while (currentSym != null)
            {
                if (currentSym.IsLOCVARSYM)
                {
                    LOCVARSYM localSym = currentSym as LOCVARSYM;
                    localSym.LocSlotInfo.TypeSym = localSym.TypeSym;
                    localSym.LocSlotInfo.IsParameter = true;
                    localSym.LocSlotInfo.Index = currentSlot++;
                    DebugUtil.Assert(!localSym.IsThis || localSym.LocSlotInfo.Index == 0);
                }
                currentSym = currentSym.NextSym;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        //private mdToken computeLocalSignature();

        //------------------------------------------------------------
        // ILGENREC.GetInfileFromTree
        //
        /// <summary></summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private INFILESYM GetInfileFromTree(BASENODE tree)
        {
            string inputFileName = tree.GetContainingFileName(true);
            INFILESYM inputFile = compiler.MainSymbolManager.FindInfileSym(inputFileName);
            return inputFile;
        }

        //------------------------------------------------------------
        // ILGENREC.getPosFromTree
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private SourceExtent GetPosFromTree(BASENODE treeNode, EXPRFLAG flags)
        {
            INFILESYM infile = GetInfileFromTree(treeNode);

            if (treeNode.Kind == NODEKIND.FOR &&
                (treeNode.Flags & NODEFLAGS.FOR_FOREACH) != 0 &&
                (flags & EXPRFLAG.USEORIGDEBUGINFO) == 0)
            {
                return GetSpecialPos((treeNode as FORSTMTNODE).InKeyword, infile);
            }

            SourceExtent extent = new SourceExtent();
            extent.InFileSym = infile;
            DebugUtil.Assert(extent.InFileSym != null);
            if (extent.InFileSym != null)
            {
                bool br = extent.InFileSym.SourceData.GetExtentEx(
                    treeNode,
                    extent.BeginPos,
                    extent.EndPos,
                    ExtentFlags.SINGLESTMT);
                DebugUtil.Assert(br);
                if (!br)
                {
                    extent.SetInvalid();
                }
            }
            return extent;
        }

        //------------------------------------------------------------
        // ILGENREC.GetSpecialPos (1)
        //
        /// <summary></summary>
        /// <param name="e"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private SourceExtent GetSpecialPos(SpecialDebugPointEnum e)
        {
            int index;

            // Only called in debug path
            DebugUtil.Assert(TrackDebugInfo());

            switch (e)
            {
                default:
                    DebugUtil.VsFail("Bad debug point!");
                    //__assume(0);
                    // fall through
                    goto case SpecialDebugPointEnum.HiddenCode;

                case SpecialDebugPointEnum.HiddenCode:
                    SourceExtent extent = new SourceExtent();
                    extent.SetHiddenInvalidSource();
                    return extent;

                case SpecialDebugPointEnum.OpenCurly:
                    index = GetOpenIndex();
                    break;

                case SpecialDebugPointEnum.CloseCurly:
                    index = GetCloseIndex();
                    break;
            }

            return GetSpecialPos(index, this.methodSym.GetInputFile());
        }

        //------------------------------------------------------------
        // ILGENREC.GetSpecialPos (2)
        //
        /// <summary></summary>
        /// <param name="index"></param>
        /// <param name="infileSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private SourceExtent GetSpecialPos(int index, INFILESYM infileSym)
        {
            LEXDATA ld = new LEXDATA();
            SourceExtent extent = new SourceExtent();

            // Only called in debug path
            DebugUtil.Assert(TrackDebugInfo());

            bool br = infileSym.SourceData.GetLexResults(ld);
            DebugUtil.Assert(br);
            if (!br || index == -1)
            {
                extent.SetHiddenInvalidSource();
            }
            else
            {
                extent.NoDebugInfo = false;
                extent.InFileSym = infileSym;
                extent.BeginPos = ld.TokenAt(index);
                extent.EndPos = ld.TokenAt(index).StopPosition();
            }
            return extent;
        }

        //------------------------------------------------------------
        // ILGENREC.InitLocalsFromEnc
        //
        /// <summary>
        /// <para>(sscli)
        /// get the list of locals from the EnC manager
        /// and pre-poluate the local slots</para>
        /// <para>Does nothing.</para>
        /// </summary>
        //------------------------------------------------------------
        private void InitLocalsFromEnc()
        {
            // Does nothing.
        }

#if false
        //------------------------------------------------------------
        // ILGENREC.GetLocalSlot (1)
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetLocalSlot(string name, TYPESYM typeSym)
        {
            DebugUtil.Assert(typeSym != null && name != null);
            //DebugUtil.Assert(m_pbldrLocalSlotArray.Length() % sizeof(IlSlotInfo) == 0);

            // Normalize everything
            typeSym = compiler.MainSymbolManager.SubstType(typeSym, SubstTypeFlagsEnum.NormAll);
            if (!compiler.Options.Optimizations)
            {
                // Look for an existing slot
                //for (IlSlotInfo * slotInfo = (IlSlotInfo*)m_pbldrLocalSlotArray.Buffer(),
                //        * pisiLim = (IlSlotInfo*)(m_pbldrLocalSlotArray.Buffer() + m_pbldrLocalSlotArray.Length());
                //        slotInfo < pisiLim; slotInfo++)
                foreach (IlSlotInfo si in this.localSlotList)
                {
                    if (si.Name == name && si.TypeSym == typeSym)
                    {
                        si.IsUsed = true;
                        return si.IlSlotNum;
                    }
                }
            }
            // If none are found (or we're not reusing slots as is the case for optimized builds)
            // Just add a new one at the end of the list
            //IlSlotInfo * slotInfo = (IlSlotInfo*)m_pbldrLocalSlotArray.AddBuf(sizeof(IlSlotInfo));
            IlSlotInfo slotInfo = new IlSlotInfo();
            slotInfo.Name = name;
            slotInfo.TypeSym = typeSym;
            slotInfo.IsUsed = true;
            //slotInfo.IlSlotNum = m_pbldrLocalSlotArray.Length() / sizeof(IlSlotInfo) - 1;
            slotInfo.IlSlotNum = this.localSlotList.Count;
            this.localSlotList.Add(slotInfo);
            return slotInfo.IlSlotNum;
        }

        //------------------------------------------------------------
        // ILGENREC.GetLocalSlot (2)
        //
        /// <summary>
        /// <para>Search a IlSlotInfo instance with the specified TYPESYM and TEMP_KIND fields
        /// in this.localSlotList</para>
        /// </summary>
        /// <param name="tempKind"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetLocalSlot(TEMP_KIND tempKind, TYPESYM typeSym)
        {
            DebugUtil.Assert(typeSym != null);
            //DebugUtil.Assert(m_pbldrLocalSlotArray.Length() % sizeof(IlSlotInfo) == 0);

            // Normalize everything
            typeSym = compiler.MainSymbolManager.SubstType(typeSym, SubstTypeFlagsEnum.NormAll);

            // In non-EnC builds this array only contains used items, so don't bother to search it
            if (compiler.FEncBuild())
            {
                // Look for an existing unused slot
                //for (IlSlotInfo * pisi = (IlSlotInfo*)m_pbldrLocalSlotArray.Buffer(),
                //        * pisiLim = (IlSlotInfo*)(m_pbldrLocalSlotArray.Buffer() + m_pbldrLocalSlotArray.Length());
                //        pisi < pisiLim; pisi++)
                foreach (IlSlotInfo si in this.localSlotList)
                {
                    if (si.Name == null && si.TempKind == tempKind && si.TypeSym == typeSym && !si.IsUsed)
                    {
                        si.IsUsed = true;
                        return si.IlSlotNum;
                    }
                }
            }

            // If none are found just add a new one at the end of the list
            //IlSlotInfo * slotInfo = (IlSlotInfo*)m_pbldrLocalSlotArray.AddBuf(sizeof(IlSlotInfo));
            IlSlotInfo slotInfo = new IlSlotInfo();
            slotInfo.Name = null;
            slotInfo.TempKind = tempKind;
            slotInfo.TypeSym = typeSym;
            slotInfo.IsUsed = true;
            //slotInfo.IlSlotNum = m_pbldrLocalSlotArray.Length() / sizeof(IlSlotInfo) - 1;
            //slotInfo.IlSlotNum = this.localSlotList.Count;
            this.localSlotList.Add(slotInfo);
            return slotInfo.IlSlotNum;
        }
#endif

        //------------------------------------------------------------
        // ILGENREC.GetLocalBuilder (1)
        //
        /// <summary>
        /// <para>Create/Find the LiSlotInfo instance and
        /// Set the slot index to LOCVARSYM.LocalSlotInfo.</para>
        /// <para>Use in place of GetLocalSlot in sscli.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private LocalBuilder GetLocalBuilder(
            string name,
            TYPESYM typeSym,
            bool pinned)
        {
            DebugUtil.Assert(typeSym != null && name != null);

            // Normalize everything
            //typeSym = compiler.MainSymbolManager.SubstType(typeSym, SubstTypeFlagsEnum.NormAll);

            if (!compiler.OptionManager.Optimize)
            {
                foreach (IlSlotInfo si in this.localSlotList)
                {
                    if (si.Name == name && si.TypeSym == typeSym)
                    {
                        si.IsUsed = true;
                        return si.LocalBuilder;
                    }
                }
            }

            // If none are found (or we're not reusing slots as is the case for optimized builds)
            // Just add a new one at the end of the list

            IlSlotInfo slotInfo = new IlSlotInfo();
            slotInfo.Name = name;
            slotInfo.TypeSym = typeSym;
            slotInfo.IsUsed = true;
            slotInfo.LocalBuilder = compiler.Emitter.EmitLocalVariable(this.methodSym, typeSym, pinned);
            this.localSlotList.Add(slotInfo);

            return slotInfo.LocalBuilder;
        }

        //------------------------------------------------------------
        // ILGENREC.GetLocalSlot (2)
        //
        /// <summary>
        /// <para>Search a IlSlotInfo instance with the specified TYPESYM and TEMP_KIND fields
        /// in this.localSlotList</para>
        /// </summary>
        /// <param name="tempKind"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private LocalBuilder GetLocalBuilder(
            TEMP_KIND tempKind,
            TYPESYM typeSym,
            bool isPinned)
        {
            DebugUtil.Assert(typeSym != null);

            // Normalize everything
            //typeSym = compiler.MainSymbolManager.SubstType(typeSym, SubstTypeFlagsEnum.NormAll);

            // In non-EnC builds this array only contains used items, so don't bother to search it
            if (compiler.FEncBuild())
            {
                // Look for an existing unused slot
                foreach (IlSlotInfo si in this.localSlotList)
                {
                    if (si.Name == null && si.TempKind == tempKind && si.TypeSym == typeSym && !si.IsUsed)
                    {
                        si.IsUsed = true;
                        return si.LocalBuilder;
                    }
                }
            }

            // If none are found just add a new one at the end of the list
            IlSlotInfo slotInfo = new IlSlotInfo();
            slotInfo.Name = null;
            slotInfo.TempKind = tempKind;
            slotInfo.TypeSym = typeSym;
            slotInfo.IsUsed = true;
            slotInfo.LocalBuilder = compiler.Emitter.EmitLocalVariable(this.methodSym, typeSym, isPinned);
            this.localSlotList.Add(slotInfo);
            return slotInfo.LocalBuilder;
        }

        //------------------------------------------------------------
        // ILGENREC.CreateNewBB
        //
        /// <summary>
        /// <para>create a new basic block and maybe make it the current one so that
        /// we can emit to it.</para>
        /// <para>Create a BBLOCK instance and set it to this.currentBBlock.</para>
        /// <para>argument makeCurrent has the default value false.</para>
        /// </summary>
        /// <param name="makeCurrent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private BBLOCK CreateNewBB(bool makeCurrent)	// = false
        {
            //BBLOCK * rval = (BBLOCK*) allocator.Alloc(sizeof(BBLOCK));
            //rval.LeaveTarget = false;	// assigned in the constructor.
            BBLOCK block = new BBLOCK();
            if (makeCurrent)
            {
                //DebugUtil.Assert(this.inlineBBlock.ExitIL != ILCODE.ILLEGAL);

                InitBBlock(block);  //InitInlineBB();
                this.currentBBlock = block;
            }
            return block;
        }

#if false
        //------------------------------------------------------------
        // ILGENREC.StartNewBB (old)
        //
        /// <summary>
        /// <para>close the previuos bb and start a new one,
        /// either the one provided by next, or a brand new one...
        /// Return the now current BB</para>
        /// <para>Specify the end processing of this.currentBBlock and
        /// set new BBlock to this.currentBBlock.</para>
        /// <para>Default values: ILCODE.next, null, ILCODE.ILLEGAL</para>
        /// </summary>
        /// <param name="nextBBlock"></param>
        /// <param name="exitIL"></param>
        /// <param name="jumpDest"></param>
        /// <param name="exitILRev"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private BBLOCK StartNewBB(
            BBLOCK nextBBlock,
            ILCODE exitIL,		//  = ILCODE.next,
            BBLOCK jumpDest,	//  = null,
            ILCODE exitILRev)	//  = ILCODE.ILLEGAL
        {
            EndBB(exitIL, jumpDest, exitILRev);
            if (nextBBlock != null)
            {
                bool hasTarget = nextBBlock.HasLeaveTarget;
                this.currentBBlock.Next = nextBBlock;
                //InitInlineBB();
                this.currentBBlock = nextBBlock;
                InitBBlock(this.currentBBlock);
                //this.currentBBlock.HasLeaveTarget = nextBBlock.HasLeaveTarget;  // inlineBBlock
                this.currentBBlock.HasLeaveTarget = hasTarget;
            }
            else
            {
                BBLOCK prev = this.currentBBlock;
                CreateNewBB(true);
                prev.Next = this.currentBBlock;
            }
            return this.currentBBlock;
        }
#endif

        //------------------------------------------------------------
        // ILGENREC.StartNewBBcore
        //
        /// <summary>
        /// <para>close the previuos bb and start a new one,
        /// either the one provided by next, or a brand new one...
        /// Return the now current BB</para>
        /// <para>Specify the end processing of this.currentBBlock and
        /// set new BBlock to this.currentBBlock.</para>
        /// <para>Default values: ILCODE.next, null, ILCODE.ILLEGAL</para>
        /// </summary>
        /// <param name="nextBBlock"></param>
        /// <param name="exitIL"></param>
        /// <param name="jumpDest"></param>
        /// <param name="exitILRev"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private BBLOCK StartNewBBcore(BBLOCK nextBBlock)
        {
            if (nextBBlock != null)
            {
                bool hasTarget = nextBBlock.HasLeaveTarget;
                this.currentBBlock.Next = nextBBlock;
                //InitInlineBB();
                this.currentBBlock = nextBBlock;
                InitBBlock(this.currentBBlock);
                //this.currentBBlock.HasLeaveTarget = nextBBlock.HasLeaveTarget;  // inlineBBlock
                this.currentBBlock.HasLeaveTarget = hasTarget;
            }
            else
            {
                BBLOCK prev = this.currentBBlock;
                CreateNewBB(true);
                prev.Next = this.currentBBlock;
            }
            return this.currentBBlock;
        }

        //------------------------------------------------------------
        // ILGENREC.StartNewBB (1)
        //
        /// <summary>
        /// <para>close the previuos bb and start a new one,
        /// either the one provided by next, or a brand new one...
        /// Return the now current BB</para>
        /// <para>Specify the end processing of this.currentBBlock and
        /// set new BBlock to this.currentBBlock.</para>
        /// <para>Default values: ILCODE.next, null, ILCODE.ILLEGAL</para>
        /// </summary>
        /// <param name="nextBBlock"></param>
        /// <param name="exitIL"></param>
        /// <param name="jumpDest"></param>
        /// <param name="exitILRev"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private BBLOCK StartNewBB(
            BBLOCK nextBBlock,
            ILCODE exitIL,		//  = ILCODE.next,
            BBLOCK jumpDest,	//  = null,
            ILCODE exitILRev)	//  = ILCODE.ILLEGAL
        {
            EndBB(exitIL, jumpDest, exitILRev);
            return StartNewBBcore(nextBBlock);
        }

        //------------------------------------------------------------
        // ILGENREC.StartNewSwitchBB (2)
        //
        /// <summary>
        /// <para>close the previuos bb and start a new one,
        /// either the one provided by next, or a brand new one...
        /// Return the now current BB</para>
        /// <para>Specify the end processing of this.currentBBlock and
        /// set new BBlock to this.currentBBlock.</para>
        /// <para>Default values: ILCODE.next, null, ILCODE.ILLEGAL</para>
        /// </summary>
        /// <param name="nextBBlock"></param>
        /// <param name="exitIL"></param>
        /// <param name="jumpDest"></param>
        /// <param name="exitILRev"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private BBLOCK StartNewSwitchBB(
            BBLOCK nextBBlock,
            SWITCHDEST switchDest,  //  = null,
            ILCODE exitILRev)	    //  = ILCODE.ILLEGAL
        {
            EndBBtoSwitch(ILCODE.SWITCH, switchDest, exitILRev);
            return StartNewBBcore(nextBBlock);
        }

        //------------------------------------------------------------
        // ILGENREC.EndBB
        //
        /// <summary>
        /// <para>terminate the current bb with the given exit code & jump destination</para>
        /// <para>Set the exit code and jump destination to this.currentBBlock.</para>
        /// </summary>
        /// <param name="exitIL"></param>
        /// <param name="jumpDest"></param>
        /// <param name="exitILRev"></param>
        //------------------------------------------------------------
        private void EndBB(
            ILCODE exitIL,
            BBLOCK jumpDest,
            ILCODE exitILRev)	//  = ILCODE.ILLEGAL
        {
            // remember, if currently being emitted to, the bb has its exit set to ILCODE.ILLEGAL
            DebugUtil.Assert(this.currentBBlock.ExitIL == ILCODE.ILLEGAL);  // inlineBBlock
            this.currentBBlock.ExitIL = exitIL; // inlineBBlock
            if (exitIL == ILCODE.LEAVE)
            {
                DebugUtil.Assert(jumpDest != null);
                if (jumpDest != null)
                {
                    jumpDest.HasLeaveTarget = true;
                }
            }
            this.currentBBlock.ExitILRev = exitILRev;   // inlineBBlock
            if (exitIL == ILCODE.LEAVE)
            {
                this.currentBBlock.LeaveNestingCount = this.finallyNestingCount;    // inlineBBlock
            }

#if DEBUG
            if (jumpDest != null)
            {
                this.currentBBlock.JumpDestBBlock = jumpDest;   // inlineBBlock
            }
            else
            {
                //memset( &this.inlineBBlock.jumpDest, 0xCCCCCCCC, sizeof(BBLOCK*));
            }
#else
            this.currentBBlock.JumpDestBBlock = jumpDest;   // inlineBBlock
#endif
            if (exitIL < ILCODE.last)
            {
                this.currentBBlock.CurrentLength += ILcodesSize[(int)exitIL];
                if (this.currentBBlock.JumpDestBBlock != null)
                {
                    this.currentBBlock.CurrentLength += 4;
                }
            }

            //--------------------------------------------------
            // copy the code into a more permanent place...
            //--------------------------------------------------
            //this.inlineBBlock.curLen = this.inlineBBlock.code - reusableBuffer;

            //BYTE * newBuffer = ((BYTE*) allocator.Alloc(
            //    BB_PREBUFFER + this.inlineBBlock.curLen + BB_TOPOFF)) + BB_PREBUFFER;
            //memcpy(newBuffer, reusableBuffer, this.inlineBBlock.curLen);
            //this.inlineBBlock.code = newBuffer;
            //
            // and finally code the bb info to its permanent mapping
            //memcpy(currentBB, &this.inlineBBlock, sizeof(BBLOCK));
        }

        //------------------------------------------------------------
        // ILGENREC.EndSwitchBB
        //
        /// <summary>
        /// <para>terminate the current bb with the given exit code & jump destination</para>
        /// <para>Set the exit code and jump destination to this.currentBBlock.</para>
        /// </summary>
        /// <param name="exitIL"></param>
        /// <param name="switchDest"></param>
        /// <param name="exitILRev"></param>
        //------------------------------------------------------------
        private void EndBBtoSwitch(
            ILCODE exitIL,
            SWITCHDEST switchDest,
            ILCODE exitILRev)	//  = ILCODE.ILLEGAL
        {
            // remember, if currently being emitted to, the bb has its exit set to ILCODE.ILLEGAL
            DebugUtil.Assert(this.currentBBlock.ExitIL == ILCODE.ILLEGAL);  // inlineBBlock
            this.currentBBlock.ExitIL = exitIL; // inlineBBlock
            //if (exitIL == ILCODE.LEAVE)
            //{
            //    DebugUtil.Assert(jumpDest != null);
            //    if (jumpDest != null)
            //    {
            //        jumpDest.HasLeaveTarget = true;
            //    }
            //}
            this.currentBBlock.ExitILRev = exitILRev;   // inlineBBlock
            if (exitIL == ILCODE.LEAVE)
            {
                this.currentBBlock.LeaveNestingCount = this.finallyNestingCount;    // inlineBBlock
            }

#if DEBUG
            if (switchDest != null)
            {
                this.currentBBlock.SwitchDest= switchDest;   // inlineBBlock
            }
            else
            {
                //memset( &this.inlineBBlock.jumpDest, 0xCCCCCCCC, sizeof(BBLOCK*));
            }
#else
            this.currentBBlock.SwitchDest= switchDest;   // inlineBBlock
#endif
            if (exitIL < ILCODE.last)
            {
                this.currentBBlock.CurrentLength += ILcodesSize[(int)exitIL];
                if (this.currentBBlock.JumpDestBBlock != null)
                {
                    this.currentBBlock.CurrentLength += 4;
                }
            }

            //--------------------------------------------------
            // copy the code into a more permanent place...
            //--------------------------------------------------
            //this.inlineBBlock.curLen = this.inlineBBlock.code - reusableBuffer;

            //BYTE * newBuffer = ((BYTE*) allocator.Alloc(
            //    BB_PREBUFFER + this.inlineBBlock.curLen + BB_TOPOFF)) + BB_PREBUFFER;
            //memcpy(newBuffer, reusableBuffer, this.inlineBBlock.curLen);
            //this.inlineBBlock.code = newBuffer;
            //
            // and finally code the bb info to its permanent mapping
            //memcpy(currentBB, &this.inlineBBlock, sizeof(BBLOCK));
        }

#if false
        //------------------------------------------------------------
        // ILGENREC.InitInlineBB
        //
        /// <summary>
        /// <para>initialzie the inline bb.
        /// we only initialize the fields
        /// we know we are going to read before ending the bb.</para>
        /// </summary>
        //------------------------------------------------------------
        private void InitInlineBBB()
        {
            //this.inlineBBBlock.code = reusableBuffer;
            this.inlineBBBlock.CodeList = new List<ILInstruction>();
            this.inlineBBBlock.DebugInfo = null;
            this.inlineBBBlock.StartsTry = false;
            this.inlineBBBlock.EndsFinally = false;
            this.inlineBBBlock.StartsCatchOrFinally = false;
            this.inlineBBBlock.JumpIntoTry = false;
            this.inlineBBBlock.GotoBlocked = false;
            this.inlineBBBlock.TryNestingCount = 0;
            this.inlineBBBlock.LeaveNestingCount = 0;
            this.inlineBBBlock.LeaveTarget = false;
#if DEBUG
            this.inlineBBBlock.ExitIL = ILCODE.ILLEGAL;
            this.inlineBBBlock.StartOffset =  -1;
            this.inlineBBBlock.Sym = null;
            this.inlineBBBlock.CurrentLength = 0;   // 0xffffffff;
#endif
        }
#endif
        //------------------------------------------------------------
        // ILGENREC.InitBBlock
        //
        /// <summary></summary>
        /// <param name="block"></param>
        //------------------------------------------------------------
        private void InitBBlock(BBLOCK block)
        {
            block.CodeList = new List<ILInstruction>();
            block.DebugInfo = null;
            block.StartsException = false;
            block.EndsException= false;
            block.StartsCatch = false;
            block.StartsFinally = false;
            block.JumpIntoTry = false;
            block.GotoBlocked = false;
            block.TryNestingCount = 0;
            block.HasLeaveTarget = false;

#if DEBUG
            block.ExitIL = ILCODE.ILLEGAL;
            block.StartOffset = -1;
            block.Sym = null;
            block.CurrentLength = 0;
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.InitFirstBB
        //
        /// initialize the first bb before generating code
        //------------------------------------------------------------
        private void InitFirstBB()
        {
            this.firstBBlock = CreateNewBB(true);
#if DEBUG
            this.currentBBlock.ExitIL = ILCODE.next;    // inlineBBlock
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        //private void flushBB();

        //------------------------------------------------------------
        // ILGENREC.GetCOffset
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetCOffset()
        {
            //return (inlineBB.code - (reusableBuffer));
            return this.currentBBlock.CodeList.Count;   // inlineBBlock
        }

        //------------------------------------------------------------
        // ILGENREC.CloseDebugInfo
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void CloseDebugInfo()
        {
            if (this.currentDebugInfo == null)
            {
                return;
            }

            DebugUtil.Assert(TrackDebugInfo());
#if DEBUG
            if (this.currentDebugInfo.Prev != null)
            {
                DebugUtil.Assert(this.currentDebugInfo.Prev.Next == this.currentDebugInfo);
            }
#endif

            if (this.currentDebugInfo.BeginBBlock == this.currentBBlock &&
                this.currentDebugInfo.BeginOffset == GetCOffset())
            {
                if (this.currentDebugInfo.Prev != null)
                {
                    this.currentDebugInfo.Prev.Next = null;
                }
                if (this.currentDebugInfo.BeginBBlock == this.currentBBlock)
                {
                    DebugUtil.Assert(this.currentBBlock.DebugInfo == this.currentDebugInfo);    // inlineBBlock
                    this.currentBBlock.DebugInfo = this.currentDebugInfo.Prev;  // inlineBBlock
                }
                else
                {
                    DebugUtil.Assert(this.currentDebugInfo.BeginBBlock.DebugInfo== this.currentDebugInfo);
                    this.currentDebugInfo.BeginBBlock.DebugInfo = this.currentDebugInfo.Prev;
                }
                this.currentDebugInfo.BeginBBlock.DebugInfo = this.currentDebugInfo.Prev;
                this.currentDebugInfo = null;
                return;
            }

            this.currentDebugInfo.EndBBlock = this.currentBBlock;
            this.currentDebugInfo.EndOffset = GetCOffset();
            this.currentDebugInfo = null;
        }

        //------------------------------------------------------------
        // ILGENREC.EmitDebugDataPoint (1)
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void EmitDebugDataPoint(BASENODE treeNode, EXPRFLAG flags)
        {
            // Only called in debug path
            DebugUtil.Assert(TrackDebugInfo());
            DebugUtil.Assert(this.currentDebugInfo != null);

            if (treeNode == null ||
                (treeNode == this.currentNode && (flags & EXPRFLAG.NODEBUGINFO) == 0))
            {
                return;
            }

            // Only set this if we don't have a statement already
            if (!this.currentDebugInfo.Extent.IsValidSource)
            {
                if ((flags & EXPRFLAG.LASTBRACEDEBUGINFO) != 0)
                {
                    this.currentNode = null;
                    this.currentExtent = GetSpecialPos(SpecialDebugPointEnum.CloseCurly);
                    if (!this.currentExtent.IsValid)
                    {
                        this.currentExtent.SetHiddenInvalidSource();
                    }
                }
                else
                {
                    this.currentNode = treeNode;
                    this.currentExtent = GetPosFromTree(treeNode, flags);
                    if ((flags & EXPRFLAG.NODEBUGINFO) != 0)
                    {
                        this.currentExtent.NoDebugInfo = true;
                        this.currentNode = null;
                    }
                }
                SetDebugDataPoint();
            }
        }

        //------------------------------------------------------------
        // ILGENREC.EmitDebugDataPoint (2)
        //
        /// <summary></summary>
        /// <param name="debugPoint"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void EmitDebugDataPoint(SpecialDebugPointEnum debugPoint, EXPRFLAG flags)
        {
            // Only called in debug path
            DebugUtil.Assert(TrackDebugInfo());
            DebugUtil.Assert(this.currentDebugInfo != null);

            SourceExtent extent = new SourceExtent();

            switch (debugPoint)
            {
                case SpecialDebugPointEnum.HiddenCode:
                    DebugUtil.Assert((flags & EXPRFLAG.NODEBUGINFO) != 0);
                    this.currentExtent.SetHiddenInvalidSource();
                    this.currentNode = null;
                    SetDebugDataPoint();
                    return;

                case SpecialDebugPointEnum.OpenCurly:
                case SpecialDebugPointEnum.CloseCurly:
                    extent = GetSpecialPos(debugPoint);
                    break;

                default:
                    DebugUtil.VsFail("Bad debug point!");
                    //__assume(0);
                    return;
            }

            if (!this.currentExtent.IsValidSource ||
                extent.EndPos.IsUninitialized ||
                this.currentExtent.EndPos.IsUninitialized ||
                extent.EndPos > this.currentExtent.EndPos)
            {
                this.currentExtent = extent;
                this.currentNode = null;
                if ((flags & EXPRFLAG.NODEBUGINFO) != 0)
                {
                    this.currentExtent.NoDebugInfo = true;
                }
                else if (debugPoint == SpecialDebugPointEnum.CloseCurly)
                {
                    this.closeIndexUsed = true;
                }
                SetDebugDataPoint();
            }
        }

        //------------------------------------------------------------
        // ILGENREC.EmitDebugDataPoint (3)
        //
        /// <summary></summary>
        /// <param name="blockNode"></param>
        /// <param name="openCurly"></param>
        //------------------------------------------------------------
        private void EmitDebugDataPoint(BLOCKNODE blockNode, bool openCurly)
        {
            // Only called in debug path
            DebugUtil.Assert(TrackDebugInfo());
            DebugUtil.Assert(this.currentDebugInfo != null);

            SourceExtent extent = new SourceExtent();
            if (openCurly)
            {
                extent = GetSpecialPos(blockNode.TokenIndex, GetInfileFromTree(blockNode));
            }
            else
            {
                extent = GetSpecialPos(blockNode.CloseCurlyIndex, GetInfileFromTree(blockNode));
            }

            if (!this.currentExtent.IsValidSource ||
                this.currentExtent.EndPos.IsUninitialized ||
                extent.EndPos > this.currentExtent.EndPos)
            {
                this.currentExtent = extent;
                this.currentNode = null;
                SetDebugDataPoint();
            }
        }

        //------------------------------------------------------------
        // ILGENREC.SetDebugDataPoint
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void SetDebugDataPoint()
        {
            // Only called in debug path
            DebugUtil.Assert(TrackDebugInfo());

            if (!this.currentDebugInfo.Extent.IsValidSource)
            {
                this.currentDebugInfo.Extent = this.currentExtent;
            }
            else if (this.currentExtent.IsValidSource)
            {
                DebugUtil.Assert(!this.currentExtent.EndPos.IsUninitialized);
                DebugUtil.Assert(!this.currentExtent.BeginPos.IsUninitialized);
                DebugUtil.Assert(this.currentExtent.InFileSym == this.currentDebugInfo.Extent.InFileSym);
                DebugUtil.Assert(this.currentExtent.NoDebugInfo == this.currentDebugInfo.Extent.NoDebugInfo);

                if (this.currentExtent.EndPos > this.currentDebugInfo.Extent.EndPos ||
                    this.currentDebugInfo.Extent.EndPos.IsUninitialized)
                {
                    this.currentDebugInfo.Extent.EndPos = this.currentExtent.EndPos;
                }
                if (this.currentExtent.BeginPos < this.currentDebugInfo.Extent.BeginPos ||
                    this.currentDebugInfo.Extent.BeginPos.IsUninitialized)
                {
                    this.currentDebugInfo.Extent.BeginPos = this.currentExtent.BeginPos;
                }
            }
        }

        //------------------------------------------------------------
        // ILGENREC.EmitDebugLocalUsage
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="localSym"></param>
        //------------------------------------------------------------
        private void EmitDebugLocalUsage(BASENODE treeNode, LOCVARSYM localSym)
        {
            // Only called in debug path
            DebugUtil.Assert(TrackDebugInfo());

            if (treeNode != this.currentNode)
            {
                this.currentNode = treeNode;
                this.currentExtent = GetPosFromTree(treeNode, 0);
            }

            DebugUtil.Assert(
                !this.currentExtent.IsValidSource ||
                this.currentExtent.InFileSym == GetInfileFromTree(localSym.GetParseTree()));

            if (this.currentExtent.IsValidSource ||
                this.currentExtent.BeginPos < localSym.FirstUsedPos)
            {
                localSym.FirstUsedPos = this.currentExtent.BeginPos;
                localSym.DegubBlockFirstUsed = this.currentBBlock;
                localSym.DebugOffsetFirstUsed = GetCOffset();
            }
        }

        //------------------------------------------------------------
        // ILGENREC.maybeEmitDebugLocalUsage
        //------------------------------------------------------------
        //private void maybeEmitDebugLocalUsage(BASENODE * tree, LOCVARSYM * sym);
        private void MaybeEmitDebugLocalUsage(BASENODE tree, LOCVARSYM sym)
        {
            if (TrackDebugInfo() &&
                (sym.LocSlotInfo.TypeSym != null || sym.IsIteratorLocal))
            {
                EmitDebugLocalUsage(tree, sym);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.OpenDebugInfo (1)
        //
        /// <summary></summary>
        /// <param name="tree"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void OpenDebugInfo(BASENODE tree, EXPRFLAG flags)
        {
            if (!TrackDebugInfo())
            {
                return;
            }

            CreateNewDebugInfo();
            EmitDebugDataPoint(tree, flags);
        }

        //------------------------------------------------------------
        // ILGENREC.OpenDebugInfo (2)
        //
        /// <summary></summary>
        /// <param name="e"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void OpenDebugInfo(SpecialDebugPointEnum e, EXPRFLAG flags)
        {
            if (!TrackDebugInfo() ||
                (e == SpecialDebugPointEnum.HiddenCode && (flags & EXPRFLAG.NODEBUGINFO) == 0))
            {
                return;
            }

            CreateNewDebugInfo();
            EmitDebugDataPoint(e, flags);
        }

        //------------------------------------------------------------
        // ILGENREC.OpenDebugInfo (3)
        //
        /// <summary></summary>
        /// <param name="block"></param>
        /// <param name="openCurly"></param>
        //------------------------------------------------------------
        private void OpenDebugInfo(BLOCKNODE block, bool openCurly)
        {
            if (!TrackDebugInfo()) return;

            CreateNewDebugInfo();
            EmitDebugDataPoint(block, openCurly);
        }

        //------------------------------------------------------------
        // ILGENREC.EmitNopForCurly
        //------------------------------------------------------------
        private void EmitNopForCurly(EXPRBLOCK block, bool openCurly)
        {
            OpenDebugInfo(block.TreeNode as BLOCKNODE, openCurly);
            PutILInstruction(ILCODE.NOP);
            CloseDebugInfo();
            this.currentExtent.SetInvalid();
        }

        //------------------------------------------------------------
        // ILGENREC.CreateNewDebugInfo
        //
        /// <summary>
        /// Create this.currentDebugInfo.
        /// </summary>
        //------------------------------------------------------------
        private void CreateNewDebugInfo()
        {
            DebugUtil.Assert(TrackDebugInfo());
            DebugUtil.Assert(this.currentDebugInfo == null);

            //this.currentDebugInfo = (DEBUGINFO*) allocator.AllocZero(sizeof(DEBUGINFO));
            this.currentDebugInfo = new DEBUGINFO();
            this.currentDebugInfo.BeginBBlock = this.currentBBlock;
            this.currentDebugInfo.Extent.SetHiddenInvalidSource();
            this.currentDebugInfo.Prev = this.currentBBlock.DebugInfo;  // inlineBBlock

            if (this.currentBBlock.DebugInfo != null)   // inlineBBlock
            {
                this.currentBBlock.DebugInfo.Next = this.currentDebugInfo;  // inlineBBlock
            }
            this.currentBBlock.DebugInfo = this.currentDebugInfo;   // inlineBBlock
            this.currentDebugInfo.BeginOffset = GetCOffset();
        }

        //------------------------------------------------------------
        // ILGENREC.FitsInBucket
        //
        /// <summary>
        /// <para>Determine if key is close to bucket.</para>
        /// <para>Return false if we should create a new bucket.</para>
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="newMembers"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool FitsInBucket(SWITCHBUCKET bucket, ulong key, int newMembers)
        {
            if (bucket == null) return false;

            //ASSERT((__int64)key > (__int64)bucket->lastMember);
            DebugUtil.Assert(unchecked((long)key > (long)bucket.LastMember));

            ulong slots = key - bucket.FirstMember;
            if (slots >= System.Int32.MaxValue)
            {
                return false;
            }
            ++slots;

            // Ensure > 50% table density
            return (ulong)((bucket.MemberCount + newMembers) * 2) > slots;
        }

        //------------------------------------------------------------
        // ILGENREC.MergeLastBucket
        //
        /// <summary>
        /// Examine how many preceding buckets can be united with the current bucket.
        /// </summary>
        /// <param name="lastBucket"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int MergeLastBucket(ref SWITCHBUCKET lastBucket)
        {
            int merged = 0;
            SWITCHBUCKET currentBucket = lastBucket;

        AGAIN:
            SWITCHBUCKET prevBucket = currentBucket.PrevBucket;

            if (FitsInBucket(prevBucket, currentBucket.LastMember, currentBucket.MemberCount))
            {
                lastBucket = prevBucket;
                prevBucket.LastMember = currentBucket.LastMember;
                prevBucket.MemberCount += currentBucket.MemberCount;
                currentBucket = prevBucket;
                ++merged;

                goto AGAIN;
            }

            return merged;
        }

        //------------------------------------------------------------
        // ILGENREC.emitSwitchBuckets
        //
        /// <summary></summary>
        /// <param name="switchExpr"></param>
        /// <param name="array"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <param name="slot"></param>
        /// <param name="defBlock"></param>
        //------------------------------------------------------------
        private void EmitSwitchBuckets(
            EXPRSWITCH switchExpr,
            SWITCHBUCKET[] array,
            int first,
            int last,
            LOCSLOTINFO slot,
            BBLOCK defaultBlock)
        {
            if (first == last)
            {
                EmitSwitchBucket(switchExpr, array[first], slot, defaultBlock);
                return;
            }

            int mid = (last + first + 1) / 2;

            // This way (0 1 2 3) will produce a mid of 2 while
            // (0 1 2) will produce a mid of 1

            // Now, the first half is first to mid-1
            // and the second half is mid to last.

            // If the first half consists of only one bucket, then we will automatically fall into
            // the second half if we fail that switch.  Otherwise, however, we need to check 
            // ourselves which half we belong to:

            if (first != mid - 1)
            {
                SWITCHBUCKET currentBucket = array[mid - 1];
                int lastIndex = currentBucket.FirstIndex + currentBucket.MemberCount - 1;
                //EXPRSWITCHLABEL lastLabel = array[mid - 1].labels[array[mid - 1].members - 1];
                EXPRSWITCHLABEL lastLabel = switchExpr.LabelArray[lastIndex];
                EXPR lastKey = lastLabel.KeyExpr;

                BBLOCK secondHalf = EmitSwitchBucketGuard(lastKey, slot, true);

                EmitSwitchBuckets(switchExpr, array, first, mid - 1, slot, defaultBlock);

                StartNewBB(secondHalf, ILCODE.BR, defaultBlock, ILCODE.ILLEGAL);

            }
            else
            {
                EmitSwitchBucket(switchExpr, array[first], slot, defaultBlock);
            }

            EmitSwitchBuckets(switchExpr, array, mid, last, slot, defaultBlock);

            StartNewBB(null, ILCODE.BR, defaultBlock, ILCODE.ILLEGAL);
        }

        //------------------------------------------------------------
        // ILGENREC.EmitSwitchBucket
        //
        /// <summary></summary>
        /// <param name="switchExpr"></param>
        /// <param name="bucket"></param>
        /// <param name="slot"></param>
        /// <param name="defBlock"></param>
        //------------------------------------------------------------
        private void EmitSwitchBucket(
            EXPRSWITCH switchExpr,
            SWITCHBUCKET bucket,
            LOCSLOTINFO slot,
            BBLOCK defaultBlock)
        {
            EXPRSWITCHLABEL firstLabelExpr = switchExpr.LabelArray[bucket.FirstIndex];

            // If this bucket holds 1 member only we dispense w/ the switch statement...
            if (bucket.MemberCount == 1)
            {
                // Use a simple compare...
                DumpLocal(slot, false);
                //GenExpr(bucket.labels[0].key);
                GenExpr(firstLabelExpr.KeyExpr, true);
                this.currentStackCount -= 2;
                StartNewBB(null, ILCODE.BEQ, firstLabelExpr.BBlock = CreateNewBB(false), ILCODE.ILLEGAL);
                return;
            }

            int lastIndex = bucket.FirstIndex + bucket.MemberCount - 1;
            EXPRSWITCHLABEL lastLabelExpr = switchExpr.LabelArray[lastIndex];

            BBLOCK guardBlock = EmitSwitchBucketGuard(lastLabelExpr.KeyExpr, slot, false);
            if (guardBlock != null)
            {
                DumpLocal(slot, false);
                GenExpr(firstLabelExpr.KeyExpr, true);
                this.currentStackCount -= 2;
                DebugUtil.Assert(firstLabelExpr.KeyExpr.TypeSym.FundamentalType() != FUNDTYPE.U8);

                StartNewBB(
                    null,
                    firstLabelExpr.KeyExpr.TypeSym.IsUnsigned() ? ILCODE.BLT_UN : ILCODE.BLT,
                    defaultBlock,
                    ILCODE.ILLEGAL);
            }

            ulong expectedKey = (firstLabelExpr.KeyExpr as EXPRCONSTANT).ConstVal.GetULong();

            DumpLocal(slot, false);
            // Ok, we now need to normalize the key to 0
            if (expectedKey != 0)
            {
                GenExpr(firstLabelExpr.KeyExpr, true);
                PutILInstruction(ILCODE.SUB);
            }
            if (guardBlock != null)
            {
                PutILInstruction(ILCODE.CONV_I4);
            }
            --this.currentStackCount;

            // Now, lets construct the target blocks...

            SWITCHDEST switchDest = new SWITCHDEST();
            ulong destCount = bucket.LastMember - bucket.FirstMember + 1;
            //switchDest.BlockArray = new SWITCHDESTGOTO[destCount];
            for (ulong ul = 0; ul < destCount; ++ul)
            {
                //switchDest.BlockArray[ul] = new SWITCHDESTGOTO();
                switchDest.BBlockList.Add(new SWITCHDESTGOTO());
            }

            int slotNum = 0;
            for (int i = 0; i < bucket.MemberCount; ++i)
            {
            AGAIN:
                EXPRSWITCHLABEL labelExpr = switchExpr.LabelArray[bucket.FirstIndex + i];
                switchDest.BBlockList[slotNum].JumpIntoTry = false;
                if (expectedKey == (labelExpr.KeyExpr as EXPRCONSTANT).ConstVal.GetULong())
                {
                    switchDest.BBlockList[slotNum++].DestBBlock = labelExpr.BBlock = CreateNewBB(false);
                    expectedKey++;
                }
                else
                {
                    switchDest.BBlockList[slotNum++].DestBBlock = defaultBlock;
                    expectedKey++;
                    goto AGAIN;
                }
            }

            DebugUtil.Assert(expectedKey == bucket.LastMember + 1);
            DebugUtil.Assert((ulong)slotNum == 1 + (bucket.LastMember - bucket.FirstMember));

            StartNewSwitchBB(
                guardBlock,
                switchDest,
                ILCODE.ILLEGAL);
        }

        //------------------------------------------------------------
        // ILGENREC.EmitSwitchBucketGuard
        //
        /// <summary></summary>
        /// <param name="key"></param>
        /// <param name="slot"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private BBLOCK EmitSwitchBucketGuard(EXPR key, LOCSLOTINFO slot, bool force)
        {
            FUNDTYPE ft;

            if (!force &&
                (ft = key.TypeSym.UnderlyingType().FundamentalType()) != FUNDTYPE.I8 &&
                ft != FUNDTYPE.U8)
            {
                return null;
            }

            DumpLocal(slot, false);
            GenExpr(key,true);
            this.currentStackCount -= 2;

            BBLOCK rval;
            DebugUtil.Assert(key.TypeSym.FundamentalType() != FUNDTYPE.U8);

            StartNewBB(
                null,
                key.TypeSym.IsUnsigned() ? ILCODE.BGT_UN : ILCODE.BGT,
                rval = CreateNewBB(false),
                ILCODE.ILLEGAL);
            return rval;
        }

        //------------------------------------------------------------
        // ILGENREC.emitDebugInfo
        //------------------------------------------------------------
        //private void emitDebugInfo(unsigned codeSize, mdToken tkLocalVarSig);

        //------------------------------------------------------------
        // ILGENREC.emitDebugScopesAndVars
        //------------------------------------------------------------
        //private void emitDebugScopesAndVars(SCOPESYM * scope, unsigned codeSize, mdToken tkLocalVarSig);

        //------------------------------------------------------------
        // ILGENREC::putOpcode (sscli)
        //
        // emit a single opcode to e given buffer.  advance the buffer by the size of the opcode
        //------------------------------------------------------------
        //__forceinline void ILGENREC::putOpcode(BYTE ** buffer, ILCODE opcode)
        //{
        //    ASSERT(opcode != CEE_ILLEGAL && opcode != CEE_UNUSED1 && opcode < cee_last);
        //    REFENCODING ref = FetchAtIndex(ILcodes, opcode);
        //    if (ref.b1 != 0xFF) {
        //        ASSERT(FetchAtIndex(ILcodesSize, opcode) == 2);
        //        *(REFENCODING*)(*buffer) = ref;
        //        (*buffer) += 2;
        //    } else {
        //        ASSERT(FetchAtIndex(ILcodesSize, opcode) == 1);
        //        (**buffer) = ref.b2;
        //        (*buffer)++;
        //    }
        //}

        //------------------------------------------------------------
        // ILGENREC::putOpcode (sscli)
        //
        // emit, but to the default buffer from the inline bb
        //------------------------------------------------------------
        //__forceinline void ILGENREC::putOpcode(ILCODE opcode)
        //{
        //    if (inlineBB.code > (reusableBuffer + BB_SIZE)) {
        //        flushBB();
        //    }
        //#if DEBUG
        //    if (smallBlock) {
        //        startNewBB(NULL);
        //    }
        //#endif
        //    putOpcode(&(inlineBB.code), opcode);
        //    curStack += ILStackOps[opcode];
        //    markStackMax();
        //}

        //------------------------------------------------------------
        // ILGENREC::putWORD (sscli)
        //
        // write a given value to the ilcode stream
        //------------------------------------------------------------
        //void ILGENREC::putWORD(WORD w)
        //{
        //    SET_UNALIGNED_VAL16(inlineBB.code, w);
        //    inlineBB.code += sizeof(WORD);
        //}

        //------------------------------------------------------------
        // ILGENREC::putDWORD (sscli)
        //
        //------------------------------------------------------------
        //void ILGENREC::putDWORD(DWORD dw)
        //{
        //    SET_UNALIGNED_VAL32(inlineBB.code, dw);
        //    inlineBB.code += sizeof(DWORD);
        //}

        //------------------------------------------------------------
        // ILGENREC::putCHAR (sscli)
        //
        //------------------------------------------------------------
        //void ILGENREC::putCHAR(char c)
        //{
        //    (*(char*)(inlineBB.code)) = c;
        //    inlineBB.code += sizeof(char);
        //}

        //------------------------------------------------------------
        // ILGENREC::putQWORD (sscli)
        //
        //------------------------------------------------------------
        //void ILGENREC::putQWORD(__int64 * qv)
        //{
        //    SET_UNALIGNED_VAL64(inlineBB.code, *qv);
        //    inlineBB.code += sizeof(__int64);
        //}

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (0-1)
        //
        /// <summary></summary>
        /// <param name="unit"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILInstruction inst)
        {
            this.currentBBlock.CodeList.Add(inst);  // inlineBBlock
            this.currentStackCount += ILStackOps[(int)inst.Code];
            this.currentBBlock.CurrentLength += ILcodesSize[(int)inst.Code];
            MarkStackMax();
            return true;
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (0-2)
        //
        /// <summary></summary>
        /// <param name="unit"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILInstruction inst, BBLOCK bblock)
        {
            bblock.CodeList.Add(inst);  // inlineBBlock
            //this.currentStackCount += ILStackOps[(int)inst.Code];
            bblock.CurrentLength += ILcodesSize[(int)inst.Code];
            //MarkStackMax();
            return true;
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (1)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null,ILCODE.next,null,ILCODE.ILLEGAL);
            }
#endif
            //this.currentBBlock.CurrentLength += 0;
            return PutILInstruction(new ILInstruction(code));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (2)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Byte arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 1;
            return PutILInstruction(new ILInstruction_Byte(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (3)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, SByte arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 1;
            return PutILInstruction(new ILInstruction_SByte(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (4)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Int16 arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 2;
            return PutILInstruction(new ILInstruction_Int16(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (5)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Int32 arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_Int32(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (6-1)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="methInfo"></param>
        /// <param name="parentSym"></param>
        /// <param name="typeArguments"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(
            ILCODE code,
            MethodInfo methInfo,
            AGGTYPESYM parentSym,
            TypeArray paramTypeArray,
            TypeArray methodTypeArguments,
            TypeArray varargTypeArray)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            this.currentBBlock.Sym = methodSym; // inlineBBlock
#endif
            MethodInfo matchingMethodInfo = null;
            Exception excp = null;

            Type declaringTypeDef1 = methInfo.DeclaringType;
            if (declaringTypeDef1.IsGenericType &&
                !declaringTypeDef1.IsGenericTypeDefinition)
            {
                declaringTypeDef1 = declaringTypeDef1.GetGenericTypeDefinition();
            }

            bool isGenericClass = declaringTypeDef1.IsGenericType;
                //classTypeArguments != null &&
                //classTypeArguments.Count > 0;

            bool isGenericMethod = methInfo.IsGenericMethod;
                //methodTypeArguments != null &&
                //methodTypeArguments.Count > 0;

            Type[] varargTypes = null;
            if (varargTypeArray != null)
            {
                int start = 0;
                int tcount = varargTypeArray.Count;
                int vcount = 0;
                for (int i = 0; i < tcount; ++i)
                {
                    if (varargTypeArray[i] == compiler.MainSymbolManager.ArgListSym)
                    {
                        start = i + 1;
                        break;
                    }
                }
                vcount = tcount - start;
                if (vcount > 0)
                {
                    varargTypes = new Type[vcount];
                    for (int i = start, j = 0; i < tcount; ++i, ++j)
                    {
                        varargTypes[j] = varargTypeArray[i].Type;
                    }
                }
            }

            //--------------------------------------------------------
            // (1-1) generic class
            //--------------------------------------------------------
            if (isGenericClass)
            {
                TypeArray classTypeArguments = parentSym != null ? parentSym.AllTypeArguments : null;
                AGGSYM tvAggSym = this.methodSym.ClassSym;

                TypeArray paramTypeArray2 = null;
                Type[] paramTypes = null;

                Type declaringType = parentSym.GetConstructedType(
                    this.methodSym.ClassSym,
                    this.methodSym,
                    true);

                while (declaringType != SystemType.ObjectType)
                {
                    Type declaringTypedef2 =
                    declaringType.IsGenericType ?
                    declaringType.GetGenericTypeDefinition() : declaringType;

                    if (declaringTypeDef1 == declaringTypedef2)
                    {
                        break;
                    }
                    declaringType = declaringType.BaseType;
                }

                // parameters

                if (paramTypeArray != null && paramTypeArray.Count > 0)
                {
                    // At this point, do not subsutitute the method type arguments.
                    paramTypeArray2 = compiler.MainSymbolManager.SubstTypeArray(
                        paramTypeArray,
                        parentSym.AllTypeArguments,
                        null,
                        SubstTypeFlagsEnum.NormNone);

                    paramTypes = SymUtil.GetSystemTypesFromTypeArray(
                        paramTypeArray2,
                        this.methodSym.ClassSym,
                        this.methodSym);
                }
                else
                {
                    paramTypes = Type.EmptyTypes;
                }

                //----------------------------------------------------
                // (1-1-1) the method is not generic
                //----------------------------------------------------
                if (!isGenericMethod)
                {
                    matchingMethodInfo = ReflectionUtil.GetMethodInfo(
                        declaringType,
                        methInfo,
                        out excp);

                    if (matchingMethodInfo == null)
                    {
                        matchingMethodInfo = ReflectionUtil.GetMethodInfo(
                            declaringType,
                            methInfo.Name,
                            paramTypes,
                            out excp);
                    }
                }
                //----------------------------------------------------
                // (1-1-2) the method is generic
                //----------------------------------------------------
                else // if (isGenericMethod)
                {
                    MethodInfo mi1 = ReflectionUtil.GetMethodInfo(
                        declaringType,
                        methInfo,
                        out excp);

                    if (mi1 != null)
                    {
                        Type[] typeArgs = SymUtil.GetSystemTypesFromTypeArray(
                            methodTypeArguments,
                            this.methodSym.ClassSym,
                            this.methodSym);
                        matchingMethodInfo = mi1.MakeGenericMethod(typeArgs);
                    }
                    else
                    {
                        try
                        {
                            MethodInfo[] mis = declaringType.GetMethods();

                            try
                            {
                                foreach (MethodInfo mi2 in mis)
                                {
                                    if (!mi2.IsGenericMethodDefinition)
                                    {
                                        continue;
                                    }
                                    if (mi2.Name != methInfo.Name)
                                    {
                                        continue;
                                    }

                                    if (mi2.GetGenericArguments().Length != methodTypeArguments.Count)
                                    {
                                        continue;
                                    }
                                    if (mi2.GetParameters().Length != paramTypes.Length)
                                    {
                                        continue;
                                    }

                                    Type[] typeArgs = SymUtil.GetSystemTypesFromTypeArray(
                                        methodTypeArguments,
                                        this.methodSym.ClassSym,
                                        this.methodSym);
                                    MethodInfo cnstMethInfo = mi2.MakeGenericMethod(typeArgs);
                                    ParameterInfo[] pInfos = cnstMethInfo.GetParameters();

                                    if (pInfos.Length == 0)
                                    {
                                        matchingMethodInfo = mi2;
                                        break;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < pInfos.Length; ++i)
                                        {
                                            if (pInfos[i].ParameterType != paramTypes[i])
                                            {
                                                continue;
                                            }
                                        }
                                        matchingMethodInfo = cnstMethInfo;
                                        break;
                                    }
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                matchingMethodInfo = null;
                                excp = ex;
                            }
                            catch (InvalidOperationException ex)
                            {
                                matchingMethodInfo = null;
                                excp = ex;
                            }
                        }
                        catch (NotSupportedException ex)
                        {
                            matchingMethodInfo = null;
                            excp = ex;
                        }
                    }
                }
            }
            //--------------------------------------------------------
            // (1-2) the class is not generic
            //--------------------------------------------------------
            else // if (isGenericClass)
            {
                //----------------------------------------------------
                // (1-2-1) the method is generic
                //----------------------------------------------------
                if (isGenericMethod)
                {
                    matchingMethodInfo = ReflectionUtil.GetGenericMethod(
                        methInfo,
                        SymUtil.GetSystemTypesFromTypeArray(
                            methodTypeArguments,
                            this.methodSym.ClassSym,
                            this.methodSym),
                        out excp);
                }
                //----------------------------------------------------
                // (1-2-2) the method is not generic
                //----------------------------------------------------
                else
                {
                    matchingMethodInfo = methInfo;
                }
            }

            //--------------------------------------------------------
            // (2) error handling
            //--------------------------------------------------------
            if (matchingMethodInfo == null)
            {
                if (excp != null)
                {
                    this.compiler.Error(ERRORKIND.ERROR, excp);
                }
                else
                {
                    this.compiler.Error(ERRORKIND.ERROR, "Failed to generate IL codes: invalid MethodInfo.");
                }
                return false;
            }

            //--------------------------------------------------------
            // (3) Generate IL codes.
            //--------------------------------------------------------
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_MethodInfo(code, matchingMethodInfo, varargTypes));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (6-2)
        //
        /// <summary>
        /// For non-generic methods of non-generic types.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="methInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool PutILInstruction(
            ILCODE code,
            MethodInfo methInfo)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            this.currentBBlock.Sym = methodSym; // inlineBBlock
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_MethodInfo(code, methInfo));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (6-3)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="constrInfo"></param>
        /// <param name="parentSym"></param>
        /// <param name="typeArguments"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(
            ILCODE code,
            METHSYM methSym,
            AGGTYPESYM parentSym,
            TypeArray typeArguments)
        {
            if (methSym.MethodInfo != null)
            {
                TypeArray vaTypeArray = null;
                if (methSym.IsVarargs)
                {
                    vaTypeArray = methSym.ParameterTypes;
                }

                return PutILInstruction(
                    code,
                    methSym.MethodInfo,
                    parentSym,
                    methSym.ParameterTypes,
                    typeArguments,
                    vaTypeArray);
            }
            else if (methSym.ConstructorInfo != null)
            {
                return PutILInstruction(
                    code,
                    methSym.ConstructorInfo,
                    parentSym,
                    methSym.ParameterTypes);
            }
            return false;
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (6-4)
        //
        //------------------------------------------------------------
        private bool PutILInstruction(
            ILCODE code,
            MethWithInst mwi,
            TypeArray vaTypeArray)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            this.currentBBlock.Sym = methodSym; // inlineBBlock
#endif
            if (mwi.IsNull)
            {
                return false;
            }
            METHSYM methSym = mwi.MethSym;

            if (methSym.MethodInfo != null)
            {
                return PutILInstruction(
                    code,
                    mwi.MethSym.MethodInfo,
                    mwi.AggTypeSym,
                    mwi.MethSym.ParameterTypes,
                    mwi.TypeArguments,
                    vaTypeArray);
            }
            else if (methSym.ConstructorInfo != null)
            {
                return PutILInstruction(
                    code,
                    mwi.MethSym.ConstructorInfo,
                    mwi.AggTypeSym,
                    mwi.MethSym.ParameterTypes);
            }
            return false;
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (7)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, SignatureHelper arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_SignatureHelper(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (8-1)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="constrInfo"></param>
        /// <param name="parentSym"></param>
        /// <param name="typeArguments"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(
            ILCODE code,
            ConstructorInfo constrInfo,
            AGGTYPESYM parentSym,
            TypeArray paramTypes)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            this.currentBBlock.Sym = methodSym; // inlineBBlock
#endif
            ConstructorInfo cInfo = null;
            Exception excp = null;

            //--------------------------------------------------------
            // (1-1) parentSym is of a generic type.
            //--------------------------------------------------------
            if (parentSym != null && parentSym.AllTypeArguments.Count > 0)
            {
                Type declaringType = parentSym.GetConstructedType(
                    this.methodSym.ClassSym,
                    this.methodSym,
                    true);

                Type[] types;
                if (paramTypes != null && paramTypes.Count > 0)
                {
                    TypeArray paramTypes2 = this.compiler.MainSymbolManager.SubstTypeArray(
                        paramTypes,
                        parentSym.AllTypeArguments,
                        null,
                        SubstTypeFlagsEnum.NormNone);
                    types = SymUtil.GetSystemTypesFromTypeArray(
                        paramTypes2,
                        this.methodSym.ClassSym,
                        this.methodSym);
                }
                else
                {
                    types = Type.EmptyTypes;
                }

                cInfo = ReflectionUtil.GetConstructorInfo(
                    declaringType,
                    constrInfo,
                    out excp);

                if (cInfo == null)
                {
                    cInfo = ReflectionUtil.GetConstructorInfo(
                        declaringType,
                        types,
                        out excp);
                }
            }
            //--------------------------------------------------------
            // (1-2) parentSym is not of a generic type.
            //--------------------------------------------------------
            else
            {
                cInfo = constrInfo;
            }

            //--------------------------------------------------------
            // (2) Generate IL codes.
            //--------------------------------------------------------
            if (cInfo == null)
            {
                if (excp != null)
                {
                    this.compiler.Error(ERRORKIND.ERROR, excp);
                }
                else
                {
                    this.compiler.Error(ERRORKIND.ERROR, "Failed to generate IL codes: Invalid ConstructorInfo.");
                }
                return false;
            }

            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_ConstructorInfo(code, cInfo));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (8-2)
        //
        /// <summary>
        /// For the constructors of non-generic types.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, ConstructorInfo arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_ConstructorInfo(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (9a)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Type type)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_Type(code, type));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (9b)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, TYPESYM typeSym)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            this.currentBBlock.Sym = typeSym;   // inlineBBlock
#endif
            this.currentBBlock.CurrentLength += 4;

            Type type = SymUtil.GetSystemTypeFromSym(
                typeSym,
                this.methodSym.ClassSym,
                this.methodSym);
            DebugUtil.Assert(type != null);
            return PutILInstruction(new ILInstruction_Type(code, type));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (10)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Int64 arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 8;
            return PutILInstruction(new ILInstruction_Int64(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (11)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Single arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_Single(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (12)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Double arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 8;
            return PutILInstruction(new ILInstruction_Double(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (13)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Label arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_Label(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (14)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, Label[] arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_LabelArray(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (15-1)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(
            ILCODE code,
            FieldInfo fieldInfo,
            AGGTYPESYM parentSym)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            FieldInfo fInfo = null;
            Exception excp = null;

            //--------------------------------------------------------
            // (1-1) parentSym is of a generic type.
            //--------------------------------------------------------
            if (parentSym != null && parentSym.AllTypeArguments.Count > 0)
            {
                Type declaringType = parentSym.GetConstructedType(
                    this.methodSym.ClassSym,
                    this.methodSym,
                    false);

                fInfo = ReflectionUtil.GetFieldInfo(
                    declaringType,
                    fieldInfo,
                    out excp);

                if (fInfo == null)
                {
                    fInfo = ReflectionUtil.GetFieldInfo(
                        declaringType,
                        fieldInfo.Name,
                        out excp);
                }
            }
            //--------------------------------------------------------
            // (1-2) parentSym is not of a generic type.
            //--------------------------------------------------------
            else
            {
                fInfo = fieldInfo;
            }

            //--------------------------------------------------------
            // Generate the IL instruction or display the error message.
            //--------------------------------------------------------
            if (fInfo == null)
            {
                if (excp != null)
                {
                    this.compiler.Error(ERRORKIND.ERROR, excp);
                }
                else
                {
                    this.compiler.Error(ERRORKIND.ERROR, "Failed to generate IL codes: FieldInfo.");
                }
                return false;
            }

            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_FieldInfo(code, fInfo));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (15-2)
        //
        /// <summary>
        /// For fields of non-generic types.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, FieldInfo info)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_FieldInfo(code, info));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (15-3)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(
            ILCODE code,
            MEMBVARSYM fieldSym,
            AGGTYPESYM parentSym)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            this.currentBBlock.Sym = fieldSym;  // inlineBBlock
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(code, fieldSym.FieldInfo, parentSym);
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (16)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, String arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_String(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.PutILInstruction (17)
        //
        /// <summary></summary>
        /// <param name="code"></param>
        /// <param name="arg"></param>
        //------------------------------------------------------------
        private bool PutILInstruction(ILCODE code, LocalBuilder arg)
        {
#if DEBUG
            if (this.smallBlock)
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            }
#endif
            this.currentBBlock.CurrentLength += 4;
            return PutILInstruction(new ILInstruction_LocalBuilder(code, arg));
        }

        //------------------------------------------------------------
        // ILGENREC.GenPrologue
        //
        /// <summary></summary>
        /// <param name="block"></param>
        //------------------------------------------------------------
        private void GenPrologue(EXPRBLOCK block)
        {
            if (block.StatementsExpr == null &&
                !compiler.OptionManager.Optimize &&
                !ShouldEmitNopForBlock(block))
            {
                PutILInstruction(ILCODE.NOP);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenBlock
        //
        /// <summary>
        /// generate code for a block
        /// </summary>
        /// <param name="blockExpr"></param>
        //------------------------------------------------------------
        private void GenBlock(EXPRBLOCK blockExpr)
        {
            // record debug info for scopes, so we can emit that later.
            if (TrackDebugInfo() && blockExpr.ScopeSym != null)
            {
                blockExpr.ScopeSym.DebugStartBBlock = this.currentBBlock;
                blockExpr.ScopeSym.DebugStartOffset = GetCOffset();
            }
            bool needNopsForCurlies = ShouldEmitNopForBlock(blockExpr);

            if (needNopsForCurlies)
            {
                EmitNopForCurly(blockExpr, true);
            }

            GenStmtChain(blockExpr.StatementsExpr);

            if ((blockExpr.Flags & EXPRFLAG.NEEDSRET) != 0)
            {
                HandleReturn(true);
                StartNewBB(null, ILCODE.RET, null, ILCODE.ILLEGAL);
                CloseDebugInfo();
            }
            else if (needNopsForCurlies)
            {
                EmitNopForCurly(blockExpr, false);
            }

            if (TrackDebugInfo() && blockExpr.ScopeSym != null)
            {
                blockExpr.ScopeSym.DebugEndBBlock = this.currentBBlock;
                blockExpr.ScopeSym.DebugEndOffset = GetCOffset();
            }
        }

        //------------------------------------------------------------
        // ILGENREC.shouldEmitNopForBlock
        //
        /// <summary></summary>
        /// <param name="blockExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ShouldEmitNopForBlock(EXPRBLOCK blockExpr)
        {
            if (blockExpr.TreeNode != null &&
                blockExpr.TreeNode.Kind == NODEKIND.BLOCK &&
                (blockExpr.TreeNode as BLOCKNODE).CloseCurlyIndex != -1 &&
                (blockExpr.Flags & EXPRFLAG.NODEBUGINFO) == 0 &&
                !compiler.OptionManager.Optimize &&
                (this.methodInfo == null || !this.methodInfo.NoDebugInfo))
            {
                BLOCKNODE blockNode = blockExpr.TreeNode as BLOCKNODE;
                if (blockNode.ParentNode != null &&
                    blockNode.ParentNode.Kind == NODEKIND.TRY &&
                    (blockNode.ParentNode.Flags & NODEFLAGS.TRY_FINALLY) != 0 &&
                    blockNode.StatementsNode != null &&
                    blockNode.StatementsNode.Kind == NODEKIND.TRY &&
                    (blockNode.StatementsNode.Flags & NODEFLAGS.TRY_CATCH) != 0)
                {
                    // If my iClose == the last catch's iClose, then this is really a try/catch/finally
                    // and so we shouldn't emit the NOP
                    BASENODE catchesNode = (blockNode.StatementsNode as TRYSTMTNODE).CatchNode;
                    while (catchesNode.Kind == NODEKIND.LIST)
                    {
                        catchesNode = catchesNode.AsLIST.Operand2;
                    }
                    if (blockNode.CloseCurlyIndex == (catchesNode as CATCHNODE).BlockNode.CloseCurlyIndex)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // ILGENREC.GenStmtChain
        //
        /// <summary>
        /// Call GenStatement method for each EXPRSTMT instances.
        /// </summary>
        /// <param name="tree"></param>
        //------------------------------------------------------------
        private void GenStmtChain(EXPRSTMT tree)
        {
            for (EXPRSTMT stmt = tree; stmt != null; stmt = stmt.NextStatement)
            {
#if DEBUG
                int id = stmt.ExprID;
                if (id == 9758)
                {
                    ;
                }
#endif
                GenStatement(stmt);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenStatement
        //
        /// <summary></summary>
        /// <param name="statementExpr"></param>
        //------------------------------------------------------------
        private void GenStatement(EXPRSTMT statementExpr)
        {
#if DEBUG
            if (statementExpr.ExprID == 9926)
            {
                ;
            }
#endif
            if (statementExpr == null)
            {
                return;
            }

            if (!statementExpr.Reachable())
            {
                // Filter out unreachable statements. We need to process EXPRKIND.SWITCH
                // and EXPRKIND.SWITCHLABEL because they may contain reachable code, even
                // when they themselves are unreachable (because of constant switch
                // values and gotos).
                switch (statementExpr.Kind)
                {
                    default:
                        return;
                    case EXPRKIND.SWITCH:
                    case EXPRKIND.SWITCHLABEL:
                    case EXPRKIND.LABEL:
                        break;
                }
            }

            compiler.SetLocation(statementExpr.TreeNode);

            if (TrackDebugInfo())
            {
                switch (statementExpr.Kind)
                {
                    case EXPRKIND.BLOCK:
                    case EXPRKIND.SWITCHLABEL:
                    case EXPRKIND.TRY:
                        break;
                    default:
                        OpenDebugInfo(statementExpr.TreeNode, statementExpr.Flags);
                        break;
                }
            }

            switch (statementExpr.Kind)
            {
                case EXPRKIND.STMTAS:
                    GenExpr((statementExpr as EXPRSTMTAS).Expr, false);
                    break;

                case EXPRKIND.RETURN:
                    GenReturn(statementExpr as EXPRRETURN);
                    break;

                case EXPRKIND.DECL:
                    MaybeEmitDebugLocalUsage(statementExpr.TreeNode, (statementExpr as EXPRDECL).LocVarSym);
                    GenExpr((statementExpr as EXPRDECL).InitialExpr, false);
                    break;

                case EXPRKIND.BLOCK:
                    GenBlock(statementExpr as EXPRBLOCK);
                    break;

                case EXPRKIND.GOTO:
                    GenGoto(statementExpr as EXPRGOTO);
                    break;

                case EXPRKIND.GOTOIF:
                    GenGotoIf(statementExpr as EXPRGOTOIF);
                    break;

                case EXPRKIND.SWITCHLABEL:
                    GenLabel(statementExpr as EXPRSWITCHLABEL);
                    GenStmtChain((statementExpr as EXPRSWITCHLABEL).StatementsExpr);
                    break;

                case EXPRKIND.LABEL:
                    GenLabel(statementExpr as EXPRLABEL);
                    break;

                case EXPRKIND.SWITCH:
                    GenSwitch(statementExpr as EXPRSWITCH);
                    break;

                case EXPRKIND.THROW:
                    GenThrow(statementExpr as EXPRTHROW);
                    break;

                case EXPRKIND.TRY:
                    GenTry(statementExpr as EXPRTRY);
                    break;

                case EXPRKIND.NOOP:
                    break;

                case EXPRKIND.DEBUGNOOP:
                    if (!compiler.OptionManager.Optimize)
                    {
                        PutILInstruction(ILCODE.NOP);
                    }
                    break;

                default:
                    DebugUtil.VsFail("Bad stmt expr kind");
                    break;
            }
            CloseDebugInfo();
#if DEBUG
            if (!(this.currentStackCount == 0))
            {
                ;
            }
#endif
            DebugUtil.Assert(this.currentStackCount == 0);
        }

        //------------------------------------------------------------
        // ILGENREC.GenExpr
        //
        /// <summary>
        /// <para>generate a generic expression</para>
        /// <para>(sslic) valUsed has the default value true.</para>
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenExpr(EXPR treeExpr, bool valUsed)
        {
            AddrInfo addr;
            int cvalStack;
            LOCSLOTINFO slot;

        AGAIN:
            if (treeExpr == null)
            {
                return;
            }

            switch (treeExpr.Kind)
            {
                case EXPRKIND.CONCAT:
                    if ((treeExpr.Flags & EXPRFLAG.UNREALIZEDCONCAT) != 0)
                    {
                        if (!valUsed)
                        {
                            return;
                        }
                        // This occurs in unreachable code, so just gen the first operand.
                        treeExpr = (treeExpr as EXPRCONCAT).List;
                        if (treeExpr.Kind == EXPRKIND.LIST)
                        {
                            treeExpr = treeExpr.AsBIN.Operand1;
                        }
                    }
                    else
                    {
                        treeExpr = (treeExpr as EXPRCONCAT).List;
                    }
                    goto AGAIN;

                case EXPRKIND.WRAP:
                    if ((treeExpr as EXPRWRAP).LocSlotInfo != null)
                    {
                        DumpLocal((treeExpr as EXPRWRAP).LocSlotInfo, false);
                        if (!(treeExpr as EXPRWRAP).DoNotFree)
                        {
                            FreeTemporary((treeExpr as EXPRWRAP).LocSlotInfo);
                        }
                    }
                    else if ((treeExpr as EXPRWRAP).Expr != null && (treeExpr as EXPRWRAP).Expr.Kind == EXPRKIND.WRAP)
                    {
                        EXPRWRAP tempWrap = (treeExpr as EXPRWRAP).Expr as EXPRWRAP;
                        if (tempWrap.LocSlotInfo != null && tempWrap.DoNotFree)
                        {
                            FREETEMP(ref tempWrap.LocSlotInfo);
                        }
                    }
                    return;

                case EXPRKIND.USERLOGOP:
                    GenExpr((treeExpr as EXPRUSERLOGOP).OpX, true);
                    PutILInstruction(ILCODE.DUP);
                    GenExpr((treeExpr as EXPRUSERLOGOP).CallTF, true);
                    BBLOCK target = CreateNewBB(false);
                    StartNewBB(null, ILCODE.BRTRUE, target, ILCODE.ILLEGAL);
                    this.currentStackCount--;
                    GenExpr((treeExpr as EXPRUSERLOGOP).CallOp, true);
                    StartNewBB(target, ILCODE.next, null, ILCODE.ILLEGAL);
                    break;

                case EXPRKIND.DBLQMARK:
                    EXPR testExpr = (treeExpr as EXPRDBLQMARK).TestExpr;

                    GenExpr(testExpr, true);
                    PutILInstruction(ILCODE.DUP);
                    if (testExpr.TypeSym.IsTYVARSYM)
                    {
                        PutILInstruction(ILCODE.BOX, testExpr.TypeSym);
                    }

                    BBLOCK convBBlock = CreateNewBB(false);
                    BBLOCK doneBBlock = CreateNewBB(false);

                    StartNewBB(null, ILCODE.BRTRUE, convBBlock, ILCODE.ILLEGAL);
                    this.currentStackCount--;
                    PutILInstruction(ILCODE.POP);
                    GenExpr((treeExpr as EXPRDBLQMARK).ElseExpr, true);

                    BBLOCK elseBBlock = this.currentBBlock;
                    StartNewBB(convBBlock, ILCODE.BR, doneBBlock, ILCODE.ILLEGAL);
                    GenExpr((treeExpr as EXPRDBLQMARK).ConvertExpr, true);

                    if (this.currentBBlock.CodeList.Count == 0 &&   // inlineBBlock
                        this.currentBBlock == convBBlock)
                    {
                        // No code emitted for the conversion so we don't need doneBBlock.
                        DebugUtil.Assert(elseBBlock.ExitIL == ILCODE.BR);
                        DebugUtil.Assert(elseBBlock.JumpDestBBlock == doneBBlock);

                        elseBBlock.ExitIL = ILCODE.next;
                        elseBBlock.JumpDestBBlock = null;
                    }
                    else
                    {
                        StartNewBB(doneBBlock, ILCODE.next, null, ILCODE.ILLEGAL);
                    }
                    break;

                case EXPRKIND.SAVE:
                    DebugUtil.Assert(valUsed);
                    DebugUtil.Assert(
                        treeExpr.AsBIN.Operand2.Kind == EXPRKIND.WRAP ||
                        treeExpr.AsBIN.Operand2.Kind == EXPRKIND.FIELD);

                    if (treeExpr.AsBIN.Operand2.Kind == EXPRKIND.FIELD)
                    {
                        addr = new AddrInfo();
                        cvalStack = GenAddr(treeExpr.AsBIN.Operand2, ref addr);
                        DebugUtil.Assert(cvalStack >= 0);

                        GenExpr(treeExpr.AsBIN.Operand1, true);
                        slot = null;
                        PutILInstruction(ILCODE.DUP);
                        if (cvalStack != 0)
                        {
                            slot = STOREINTEMP(treeExpr.AsBIN.Operand1.TypeSym, TEMP_KIND.SHORTLIVED);
                        }
                        GenStore(treeExpr.AsBIN.Operand2, ref addr);
                        if (slot != null)
                        {
                            DumpLocal(slot, false);
                            FreeTemporary(slot);
                        }
                    }
                    else
                    {
                        GenExpr(treeExpr.AsBIN.Operand1, true);
                        PutILInstruction(ILCODE.DUP);
                        (treeExpr.AsBIN.Operand2 as EXPRWRAP).LocSlotInfo
                            = STOREINTEMP(treeExpr.AsBIN.Operand1.TypeSym, (treeExpr.AsBIN.Operand2 as EXPRWRAP).TempKind);
                    }
                    return;

                case EXPRKIND.LOCALLOC:
                    GenExpr(treeExpr.AsBIN.Operand1, true);
                    PutILInstruction(ILCODE.LOCALLOC);
                    break;

                case EXPRKIND.QMARK:
                    GenQMark(treeExpr.AsBIN, valUsed);
                    return;

                case EXPRKIND.SWAP:
                    GenSwap(treeExpr.AsBIN, valUsed);
                    return;

                case EXPRKIND.IS:
                    GenIs(treeExpr.AsBIN, valUsed, false);
                    return;

                case EXPRKIND.AS:
                    GenIs(treeExpr.AsBIN, valUsed, true);
                    return;

                case EXPRKIND.CAST:
                    GenCast(treeExpr as EXPRCAST, valUsed);
                    return;

                case EXPRKIND.MAKERA:
                    GenMakeRefAny(treeExpr.AsBIN, valUsed);
                    return;

                case EXPRKIND.TYPERA:
                    GenTypeRefAny(treeExpr.AsBIN, valUsed);
                    return;

                case EXPRKIND.FUNCPTR:
                    if ((treeExpr as EXPRFUNCPTR).MethWithInst.MethSym.IsVirtual &&
                        !(treeExpr as EXPRFUNCPTR).MethWithInst.MethSym.ClassSym.IsSealed &&
                        (treeExpr as EXPRFUNCPTR).ObjectExpr.TypeSym.FundamentalType() == FUNDTYPE.REF &&
                        (treeExpr.Flags & EXPRFLAG.BASECALL) == 0)
                    {
                        PutILInstruction(ILCODE.DUP);
                        PutILInstruction(ILCODE.LDVIRTFTN, (treeExpr as EXPRFUNCPTR).MethWithInst, null);
                    }
                    else
                    {
                        PutILInstruction(ILCODE.LDFTN, (treeExpr as EXPRFUNCPTR).MethWithInst, null);
                    }
#if USAGEHACK
                    treeExpr.AsFUNCPTR.MethWithInst.MethSym.IsUsed = true;
#endif
                    // PutILInstruction methos include the operations of EmitMethodToken
                    //EmitMethodToken(
                    //    (treeExpr as EXPRFUNCPTR).MethWithInst.MethSym,
                    //    (treeExpr as EXPRFUNCPTR).MethWithInst.AggTypeSym,
                    //    (treeExpr as EXPRFUNCPTR).MethWithInst.TypeArguments);
                    break;

                case EXPRKIND.TYPEOF:
                    if (valUsed)
                    {
                        EXPRTYPEOF typeofExpr = treeExpr as EXPRTYPEOF;

                        if ((treeExpr.Flags & EXPRFLAG.OPENTYPE) != 0)
                        {
                            TYPESYM typeSym = typeofExpr.SourceTypeSym;
                            DebugUtil.Assert(
                                typeSym.IsAGGTYPESYM &&
                                (typeSym as AGGTYPESYM).AllTypeArguments.Count > 0 &&
                                (typeSym as AGGTYPESYM).AllTypeArguments[(typeSym as AGGTYPESYM).AllTypeArguments.Count - 1].IsUNITSYM);
                            this.currentBBlock.Sym = typeSym.GetAggregate();    // inlineBBlock

                            PutILInstruction(ILCODE.LDTOKEN, typeSym.GetAggregate().Type);
                        }
                        else
                        {
                            PutILInstruction(ILCODE.LDTOKEN, typeofExpr.SourceTypeSym);
                        }
                        PutILInstruction(
                            ILCODE.CALL,
                            typeofExpr.MethodSym.MethodInfo,
                            typeofExpr.SourceTypeSym as AGGTYPESYM,
                            typeofExpr.MethodSym.ParameterTypes,
                            null,
                            null);
                    }
                    return;

                case EXPRKIND.SIZEOF:
                    if (valUsed)
                    {
                        GenSizeOf((treeExpr as EXPRSIZEOF).SourceTypeSym);
                    }
                    return;

                case EXPRKIND.ARRINIT:
                    GenArrayInit(treeExpr as EXPRARRINIT, valUsed);
                    return;

                case EXPRKIND.CONSTANT:
                    if (!valUsed)
                    {
                        return;
                    }
                    switch (treeExpr.TypeSym.FundamentalType())
                    {
                        case FUNDTYPE.I1:
                        case FUNDTYPE.U1:
                        case FUNDTYPE.I2:
                        case FUNDTYPE.U2:
                        case FUNDTYPE.I4:
                        case FUNDTYPE.U4:
                            GenIntConstant((treeExpr as EXPRCONSTANT).ConstVal.GetInt());
                            // OK for U4, since IL treats them the same.
                            break;

                        case FUNDTYPE.R4:
                            // REVIER_CONSIDER: should we use genDoubleConstant ever for R4s if the constant
                            // has greater precision than an R4. We do constant fold to greater precision
                            // so it might be available.
                            GenFloatConstant((treeExpr as EXPRCONSTANT).ConstVal.GetFloat());
                            break;

                        case FUNDTYPE.R8:
                            GenDoubleConstant((treeExpr as EXPRCONSTANT).ConstVal.GetDouble());
                            break;

                        case FUNDTYPE.I8:
                        case FUNDTYPE.U8:
                            GenLongConstant((treeExpr as EXPRCONSTANT).ConstVal.GetLong());
                            break;

                        case FUNDTYPE.REF:
                            //string str = (treeExpr as EXPRCONSTANT).ConstVal.GetString();
                            string str = (treeExpr as EXPRCONSTANT).GetStringValue();
                            if (str != null)
                            {
                                // this must be a string...
                                GenString(str);
                            }
                            else
                            {
                                PutILInstruction(ILCODE.LDNULL);
                            }
                            break;

                        case FUNDTYPE.STRUCT:
                            DebugUtil.Assert(treeExpr.TypeSym.IsPredefType(PREDEFTYPE.DECIMAL));
                            GenDecimalConstant((treeExpr as EXPRCONSTANT).ConstVal.GetDecimal());
                            break;

                        default:
                            DebugUtil.Assert(false, "bad constant type");
                            break;
                    }
                    break;

                case EXPRKIND.CALL:
                    GenCall(treeExpr as EXPRCALL, valUsed);
                    return;

                case EXPRKIND.NOOP:
                    return;

                case EXPRKIND.DELIM:
                    return;

                case EXPRKIND.MULTI:
                    GenMultiOp(treeExpr as EXPRMULTI, valUsed);
                    return;

                case EXPRKIND.MULTIGET:
                    GenMultiGet((treeExpr as EXPRMULTIGET).MultiExpr, valUsed);
                    return;

                // Temp management.
                case EXPRKIND.STTMP:
                    DebugUtil.Assert((treeExpr as EXPRSTTMP).LocSlotInfo == null);
                    GenExpr((treeExpr as EXPRSTTMP).SourceExpr, true);
                    if (valUsed)
                    {
                        PutILInstruction(ILCODE.DUP);
                    }
                    (treeExpr as EXPRSTTMP).LocSlotInfo =
                        STOREINTEMP((treeExpr as EXPRSTTMP).SourceExpr.TypeSym, TEMP_KIND.SHORTLIVED);
                    return;

                case EXPRKIND.LDTMP:
                    DebugUtil.Assert((treeExpr as EXPRLDTMP).TmpExpr.LocSlotInfo != null);
                    if (valUsed)
                    {
                        DumpLocal((treeExpr as EXPRLDTMP).TmpExpr.LocSlotInfo, false);
                    }
                    return;

                case EXPRKIND.FREETMP:
                    DebugUtil.Assert(!valUsed);
                    DebugUtil.Assert((treeExpr as EXPRFREETMP).TmpExpr.LocSlotInfo != null);
                    FREETEMP(ref (treeExpr as EXPRFREETMP).TmpExpr.LocSlotInfo);
                    return;

                case EXPRKIND.LIST:
                    // this kind of loop takes less code if we know that the elements
                    // are also expressions
                    GenExpr(treeExpr.AsBIN.Operand1, valUsed);
                    treeExpr = treeExpr.AsBIN.Operand2;
                    goto AGAIN;

                case EXPRKIND.NEWARRAY:
                    GenNewArray(treeExpr.AsBIN);
                    break;

                case EXPRKIND.SEQUENCE:
                    GenSideEffects(treeExpr.AsBIN.Operand1);
                    treeExpr = treeExpr.AsBIN.Operand2;
                    goto AGAIN;

                case EXPRKIND.SEQREV:
                    if (valUsed)
                    {
                        GenExpr(treeExpr.AsBIN.Operand1, true);
                    }
                    else
                    {
                        GenSideEffects(treeExpr.AsBIN.Operand1);
                    }
                    GenSideEffects(treeExpr.AsBIN.Operand2);
                    return;

                case EXPRKIND.ARGS:
                    PutILInstruction(ILCODE.ARGLIST);
                    break;

                case EXPRKIND.LOCAL:
                    if (!(treeExpr as EXPRLOCAL).LocVarSym.LocSlotInfo.IsReferenceParameter)
                    {
#if DEBUG
                        if (!((treeExpr as EXPRLOCAL).LocVarSym.LocSlotInfo.HasIndex))
                        {
                            ;
                        }
#endif
                        //DumpLocal(&(treeExpr.asLOCAL().LocVarSym.slot), false);
                        DumpLocal((treeExpr as EXPRLOCAL).LocVarSym.LocSlotInfo, false);
                        break;
                    }
                    goto case EXPRKIND.FIELD;

                case EXPRKIND.INDIR:
                case EXPRKIND.VALUERA:
                case EXPRKIND.PROP:
                case EXPRKIND.ARRINDEX:
                case EXPRKIND.FIELD:
                    GenLoad(treeExpr);
                    break;

                case EXPRKIND.ZEROINIT:
                    GenZeroInit(treeExpr as EXPRZEROINIT, valUsed);
                    return;

                case EXPRKIND.ADDR:
                    GenPtrAddr(treeExpr.AsBIN.Operand1, (treeExpr.Flags & EXPRFLAG.ADDRNOCONV) != 0, valUsed);
                    return;

                case EXPRKIND.ASSG:
                    EXPR op1 = treeExpr.AsBIN.Operand1;
                    EXPR op2 = treeExpr.AsBIN.Operand2;

                    if (IsExprOptimizedAway(op1))
                    {
                        if (valUsed)
                        {
                            GenExpr(op2, true);
                        }
                        else
                        {
                            GenSideEffects(op2);
                        }
                        return;
                    }

                    addr = new AddrInfo();

                    cvalStack = GenAddr(op1, ref addr);
                    DebugUtil.Assert(cvalStack >= 0);
                    GenExpr(op2, true);
                    slot = null;
                    if (valUsed)
                    {
                        PutILInstruction(ILCODE.DUP);
                        if (cvalStack != 0)
                        {
                            slot = STOREINTEMP(op2.TypeSym, TEMP_KIND.SHORTLIVED);
                        }
                    }

                    GenStore(op1, ref addr);

                    if (slot != null)
                    {
                        DumpLocal(slot, false);
                        FreeTemporary(slot);
                    }
                    return;

                case EXPRKIND.ANONMETH:
                case EXPRKIND.MEMGRP:
                case EXPRKIND.LAMBDAEXPR:   // CS3
                    // We should have given an error
                    DebugUtil.Assert(compiler.FAbortCodeGen(0));
                    PutILInstruction(ILCODE.LDNULL);
                    // Just to keep the stack happy
                    return;

                case EXPRKIND.SYSTEMTYPE:   // CS3
                    Type typeType = typeof(System.Type);
                    MethodInfo gtfhInfo = typeType.GetMethod(
                        "GetTypeFromHandle",
                        new Type[] { typeof(System.RuntimeTypeHandle) });
                    PutILInstruction(ILCODE.LDTOKEN, (treeExpr as EXPRSYSTEMTYPE).Type);
                    PutILInstruction(ILCODE.CALL, gtfhInfo);
                    return;

                case EXPRKIND.FIELDINFO:    // CS3
                    Type fieldInfoType = typeof(System.Reflection.FieldInfo);
                    MethodInfo gffhInfo = fieldInfoType.GetMethod(
                        "GetFieldFromHandle",
                        new Type[] { typeof(System.RuntimeFieldHandle) });
                    PutILInstruction(ILCODE.LDTOKEN, (treeExpr as EXPRFIELDINFO).FieldInfo);
                    PutILInstruction(ILCODE.CALL, gffhInfo);
                    return;

                case EXPRKIND.METHODINFO:   // CS3
                    {
                        EXPRMETHODINFO miExpr = treeExpr as EXPRMETHODINFO;

                        Type methBaseType = typeof(System.Reflection.MethodBase);

                        MethodInfo gmfhInfo = null;

                        if (miExpr.DeclaringType != null)
                        {
                            gmfhInfo = methBaseType.GetMethod(
                                "GetMethodFromHandle",
                                new Type[]
                                {
                                    typeof(System.RuntimeMethodHandle),
                                    typeof(System.RuntimeTypeHandle)
                                });
                            PutILInstruction(ILCODE.LDTOKEN, miExpr.MethodInfo);
                            PutILInstruction(ILCODE.LDTOKEN, miExpr.DeclaringType);
                            this.currentStackCount--;
                        }
                        else
                        {
                            gmfhInfo = methBaseType.GetMethod(
                                "GetMethodFromHandle",
                                new Type[] { typeof(System.RuntimeMethodHandle) });
                            PutILInstruction(ILCODE.LDTOKEN, miExpr.MethodInfo);
                        }

                        PutILInstruction(ILCODE.CALL, gmfhInfo);
                        PutILInstruction(ILCODE.CASTCLASS, typeof(System.Reflection.MethodInfo));
                    }
                    return;

                case EXPRKIND.CONSTRUCTORINFO:  // CS3
                    {
                        Type methBaseType = typeof(System.Reflection.MethodBase);
                        MethodInfo gmfhInfo = methBaseType.GetMethod(
                            "GetMethodFromHandle",
                            new Type[] { typeof(System.RuntimeMethodHandle) });
                        PutILInstruction(
                            ILCODE.LDTOKEN,
                            (treeExpr as EXPRCONSTRUCTORINFO).ConstructorInfo);
                        PutILInstruction(ILCODE.CALL, gmfhInfo);
                        PutILInstruction(ILCODE.CASTCLASS, typeof(System.Reflection.ConstructorInfo));
                    }
                    return;

                default:
                    if ((treeExpr.Flags & EXPRFLAG.BINOP) != 0)
                    {
                        GenBinopExpr(treeExpr.AsBIN, valUsed);
                    }
                    else
                    {
                        DebugUtil.Assert(false, "bad expr type");
                    }
                    break;
            }

            if (!valUsed)
            {
                PutILInstruction(ILCODE.POP);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenBinopExpr
        //
        /// <summary>
        /// <para>generate a standard binary operation...</para>
        /// <para>(sscli) valUsed has the default value true.</para>
        /// </summary>
        /// <param name="binopExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenBinopExpr(
            EXPRBINOP binopExpr,
            bool valUsed)   // = true
        {
            ILCODE ilcode;
            EXPR.CONSTRESKIND dummyCK = 0;

            switch (binopExpr.Kind)
            {
                case EXPRKIND.LOGAND:
                case EXPRKIND.LOGOR:
                case EXPRKIND.LOGNOT:
                case EXPRKIND.LT:
                case EXPRKIND.LE:
                case EXPRKIND.GT:
                case EXPRKIND.GE:
                case EXPRKIND.EQ:
                case EXPRKIND.NE:
                    GenCondExpr(binopExpr, true,ref dummyCK);
                    return;

                case EXPRKIND.ADD:
                case EXPRKIND.SUB:
                case EXPRKIND.MUL:
                case EXPRKIND.DIV:
                case EXPRKIND.MOD:
                case EXPRKIND.NEG:
                case EXPRKIND.BITAND:
                case EXPRKIND.BITOR:
                case EXPRKIND.BITXOR:
                case EXPRKIND.BITNOT:
                case EXPRKIND.LSHIFT:
                case EXPRKIND.RSHIFT:
                case EXPRKIND.ARRLEN:
                    if ((binopExpr.Flags & EXPRFLAG.CHECKOVERFLOW) != 0)
                    {
                        if (binopExpr.Operand1.TypeSym.IsUnsigned())
                        {
                            ilcode = ILarithInstrUNOvf[(int)(binopExpr.Kind - EXPRKIND.ADD)];
                        }
                        else
                        {
                            ilcode = ILarithInstrOvf[(int)(binopExpr.Kind - EXPRKIND.ADD)];
                        }
                    }
                    else
                    {
                        if (binopExpr.Operand1.TypeSym.IsUnsigned())
                        {
                            ilcode = ILarithInstrUN[(int)(binopExpr.Kind - EXPRKIND.ADD)];
                        }
                        else
                        {
                            ilcode = ILarithInstr[(int)(binopExpr.Kind - EXPRKIND.ADD)];
                        }
                    }
                    break;

                case EXPRKIND.UPLUS:
                    // This is a non-op (we just need to emit the numeric promotion)
                    GenExpr(binopExpr.Operand1, true);
                    DebugUtil.Assert(binopExpr.Operand2 == null);
                    return;

                default:
                    DebugUtil.Assert(false, "bad binop expr");
                    ilcode = ILCODE.ILLEGAL;
                    break;
            }

            GenExpr(binopExpr.Operand1, true);
            GenExpr(binopExpr.Operand2, true);

            //PutOpcode(ilcode);
            PutILInstruction(ilcode);

            if (ilcode == ILCODE.LDLEN)
            {
                //PutOpcode(ILCODE.CONV_I4);
                PutILInstruction(ILCODE.CONV_I4);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenString
        //
        /// <summary>
        /// generate a string constant
        /// </summary>
        /// <remarks>
        /// private void GenString(CONSTVAL constVal)
        /// </remarks>
        /// <param name="constVal"></param>
        //------------------------------------------------------------
        private void GenString(string str)
        {
            //ulong rva = compiler.Emitter.GetStringRef(constVal.strVal);
            //putDWORD(rva);
            //PutOpcode(ILCODE.LDSTR);
            //PutIlArgument(str);
            PutILInstruction(ILCODE.LDSTR, str);
        }

        //------------------------------------------------------------
        // ILGENREC.GenCall
        //
        /// <summary>
        /// generate code for a function call
        /// </summary>
        /// <param name="callExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenCall(EXPRCALL callExpr, bool valUsed)
        {
            METHSYM methodSym = callExpr.MethodWithInst.MethSym;
            AGGTYPESYM aggTypeSym = callExpr.MethodWithInst.AggTypeSym;
            DebugUtil.Assert(aggTypeSym != null && aggTypeSym.GetAggregate() == methodSym.ClassSym);

#if USAGEHACK
    methodSym.IsUsed = true;
#endif

            EXPR expr = callExpr.ObjectExpr;

            bool isNewObjCall = (callExpr.Flags & EXPRFLAG.NEWOBJCALL) != 0;
            bool isBaseCall = (callExpr.Flags & EXPRFLAG.BASECALL) != 0;
            bool isStructAssgCall = (callExpr.Flags & EXPRFLAG.NEWSTRUCTASSG) != 0;
            bool isImplicitStructAssgCall = (callExpr.Flags & EXPRFLAG.IMPLICITSTRUCTASSG) != 0;
            bool hasRefParams = (callExpr.Flags & EXPRFLAG.HASREFPARAM) != 0;
            bool needsCopy = false;
            EXPRLOCAL exprDumpAddr = null;
            bool wasStructAssgCall = false;
            bool isConstrained = false;

            if (expr != null &&
                expr.Kind == EXPRKIND.FIELD &&
                (expr as EXPRFIELD).FieldWithType.FieldSym.IsReadOnly &&
                (expr.Flags & EXPRFLAG.LVALUE) == 0)
            {
                needsCopy = true;
            }

            if (methodSym.IsVarargs)
            {
                methodSym = GetVarArgMethod(methodSym, callExpr.ArgumentsExpr).AsFMETHSYM;
                DebugUtil.Assert(aggTypeSym != null && aggTypeSym.GetAggregate() == methodSym.ClassSym);
            }

            LOCSLOTINFO slotObject = null;

            bool retIsVoid = callExpr.TypeSym.IsVoidType;   // == compiler.MainSymbolManager.VoidSym;
            if (methodSym.IsStatic || isNewObjCall)
            {
                DebugUtil.Assert(expr == null);
            }
            else if (isStructAssgCall || isImplicitStructAssgCall)
            {
                if (!IsExprOptimizedAway(expr))
                {
                    if (isImplicitStructAssgCall)
                    {
                        if (expr.Kind != EXPRKIND.LOCAL || (expr as EXPRLOCAL).LocVarSym.LocSlotInfo.AliasPossible)
                        {
                            wasStructAssgCall = true;// = 1;
                            isStructAssgCall = false;// = 0;
                            isNewObjCall = true;// = 1;
                        }
                        else
                        {
                            isStructAssgCall = true;// = 1;
                        }
                    }
                    GenObjectPtr(expr, methodSym, ref slotObject);
                    if (valUsed)
                    {
                        DebugUtil.Assert(expr != null);
                        if (expr.GetSeqVal().Kind == EXPRKIND.LOCAL)
                        {
                            // this is a problem... the verifier might complain that we are duping an
                            // uninitialized local's address
                            exprDumpAddr = expr.GetSeqVal() as EXPRLOCAL;
                        }
                        else
                        {
                            PutILInstruction(ILCODE.DUP);
                        }
                    }

                    retIsVoid = true;

                }
                else
                {
                    // if this is an assignment to a local which is not used, then don't assign...
                    expr = null;
                    isNewObjCall = true;// = 1;
                    isStructAssgCall = false;// = 0;
                }
            }
            else if ((callExpr.Flags & EXPRFLAG.CONSTRAINED) != 0)
            {
                // Use the constrained prefix
                DebugUtil.Assert(expr.Kind == EXPRKIND.CAST &&
                    ((expr as EXPRCAST).Operand.TypeSym.IsTYVARSYM ||
                        (expr as EXPRCAST).Operand.TypeSym.IsValueType()));
                DebugUtil.Assert(!isBaseCall);

                expr = (expr as EXPRCAST).Operand;
                GenMemoryAddress(expr, ref slotObject, false, true); // Readonly for array element access
                isConstrained = true;
            }
            else
            {
                GenObjectPtr(expr, methodSym, ref slotObject);
            }

            int stackPrev = this.currentStackCount;

            //LOCSLOTINFO * tempSlots = null;
            LOCSLOTINFO[] tempSlots = null;
            int slotCount = 0;

            if (hasRefParams)
            {
                // If the arguments being passed by ref are optimized away, we need temporary slots for them...
                //LOCSLOTINFO * curSlot = tempSlots = STACK_ALLOC(LOCSLOTINFO, methodSym.ParameterTypes.Count);
                tempSlots = new LOCSLOTINFO[methodSym.ParameterTypes.Count];
                LOCSLOTINFO curSlot = null;
                int index = 0;

                EXPR ex1 = callExpr.ArgumentsExpr;
                while (ex1 != null)
                {
                    EXPR arg;
                    if (ex1.Kind == EXPRKIND.LIST)
                    {
                        arg = ex1.AsBIN.Operand1;
                        ex1 = ex1.AsBIN.Operand2;
                    }
                    else
                    {
                        arg = ex1;
                        ex1 = null;
                    }

                    if (arg.TypeSym.IsPARAMMODSYM)
                    {
                        tempSlots[index] = curSlot = new LOCSLOTINFO();
                        EmitRefParam(arg, ref curSlot);
                    }
                    else
                    {
                        GenExpr(arg, true);
                        tempSlots[index] = null;
                    }
                    ++index;
                }

                slotCount = index; // (unsigned)(curSlot - tempSlots);

            }
            else
            {
                GenExpr(callExpr.ArgumentsExpr, true);
            }

            int stackDiff;

            if (!methodSym.IsVarargs)
            {
                stackDiff = methodSym.ParameterTypes.Count;
            }
            else
            {
                stackDiff = this.currentStackCount - stackPrev;
            }

            if (!methodSym.IsStatic && !isNewObjCall)
            {
                // adjust for this pointer
                stackDiff++;
            }

            ILCODE ilcode;
            if (isConstrained)
            {
                DebugUtil.Assert(expr.TypeSym.IsTYVARSYM || expr.TypeSym.IsValueType());

                //PutOpcode(ILCODE.CONSTRAINED);
                //EmitTypeToken(expr.TypeSym);
                PutILInstruction(ILCODE.CONSTRAINED, expr.TypeSym);
                ilcode = ILCODE.CALLVIRT;
            }
            else if (isNewObjCall)
            {
                ilcode = ILCODE.NEWOBJ;
            }
            else if (CallAsVirtual(methodSym, expr, isBaseCall))
            {
                ilcode = ILCODE.CALLVIRT;
            }
            else
            {
                ilcode = ILCODE.CALL;
            }

            //PutOpcode(ilcode);
            //EmitMethodToken(methodSym, aggTypeSym, callExpr.MethodWithInst.TypeArguments);
            if (methodSym.IsVarargs)
            {
                PutILInstruction(
                    ilcode,
                    callExpr.MethodWithInst,
                    methodSym.ParameterTypes);
            }
            else
            {
                PutILInstruction(
                    ilcode,
                    callExpr.MethodWithInst,
                    null);
            }

            // eat the arguments off the stack
            this.currentStackCount -= stackDiff;
            DebugUtil.Assert(this.currentStackCount >= 0);
            if (!methodSym.ReturnTypeSym.IsVoidType)    // != compiler.MainSymbolManager.VoidSym)
            {
                ++this.currentStackCount;
                MarkStackMax();
            }

            if (wasStructAssgCall)
            {
                PutILInstruction(ILCODE.STOBJ, expr.TypeSym);
            }

            // The difference between the above check (of funcrettype) and here, 
            // (of tree type) is that call exprs have a type of non-void for new_obj calls
            // since they denote a value of a certain type, while the constructor itself has
            // a return type of void...

            if ((isStructAssgCall || wasStructAssgCall) && valUsed)
            {
                if (exprDumpAddr != null)
                {
                    GenSlotAddress(exprDumpAddr.LocVarSym.LocSlotInfo, false);
                }
                PutILInstruction(ILCODE.LDOBJ, expr.TypeSym);
            }
            else if (!valUsed)
            {
                // so, if val is not used, and we had something, then we need to pop it...
                if (!retIsVoid)
                {
                    PutILInstruction(ILCODE.POP);
                }
                else if (
                    !wasStructAssgCall &&
                    !compiler.OptionManager.Optimize &&
                    (this.methodInfo == null || !this.methodInfo.NoDebugInfo) &&
                    (callExpr.Flags & EXPRFLAG.NODEBUGINFO) == 0)
                {
                    PutILInstruction(ILCODE.NOP);
                }
            }

            if (slotObject != null)
            {
                FreeTemporary(slotObject);
            }

            if (tempSlots != null && tempSlots.Length > 0)
            {
                for (int i = 0; i < slotCount; i++)
                {
                    if (tempSlots[i] != null)
                    {
                        FreeTemporary(tempSlots[i]);
                    }
                }
            }
        }

        //------------------------------------------------------------
        // ILGENREC.EmitRefParam
        //
        /// <summary></summary>
        /// <param name="arg"></param>
        /// <param name="curSlot"></param>
        //------------------------------------------------------------
        private void EmitRefParam(EXPR arg, ref LOCSLOTINFO curSlot)
        {
            if (arg.Kind == EXPRKIND.LOCAL && (arg as EXPRLOCAL).LocVarSym.LocSlotInfo.TypeSym == null)
            {
                curSlot = AllocTemporary(arg.TypeSym.ParentSym as TYPESYM, TEMP_KIND.SHORTLIVED);
                InitLocal(curSlot, arg.TypeSym.ParentSym as TYPESYM);
                GenSlotAddress(curSlot, false);
            }
            else
            {
                LOCSLOTINFO slot = null;
                curSlot = null;
                GenMemoryAddress(arg, ref slot, true, false);
                DebugUtil.Assert(slot == null);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenSideEffects
        //
        /// <summary>
        /// generate the sideeffects of an expression
        /// </summary>
        /// <param name="treeExpr"></param>
        //------------------------------------------------------------
        private void GenSideEffects(EXPR treeExpr)
        {
            EXPRBINOP colonExpr;
            BBLOCK endLabelBBlock;

        AGAIN:

            if (treeExpr == null)
            {
                return;
            }

            if ((treeExpr.Flags & (EXPRFLAG.ASSGOP | EXPRFLAG.CHECKOVERFLOW)) != 0)
            {
                GenExpr(treeExpr, false);
                return;
            }
            if ((treeExpr.Flags & EXPRFLAG.BINOP) != 0)
            {
                bool sense;
                switch (treeExpr.Kind)
                {
                    case EXPRKIND.IS:
                    case EXPRKIND.AS:
                    case EXPRKIND.SWAP:
                        GenSideEffects(treeExpr.AsBIN.Operand1);
                        treeExpr = treeExpr.AsBIN.Operand2;
                        goto AGAIN;

                    case EXPRKIND.LOGAND:
                        sense = false;
                    // for (a && b) and (a || b) we generate a and then b depending on the 
                    // result of a.

                    // if b has no sideefects, then we generate se(a)
                    DOCONDITIONAL:
                        if (treeExpr.AsBIN.Operand2.HasSideEffects(compiler))
                        {
                            endLabelBBlock = GenCondBranch(treeExpr.AsBIN.Operand1, CreateNewBB(false), sense);
                            GenSideEffects(treeExpr.AsBIN.Operand2);
                            StartNewBB(endLabelBBlock, ILCODE.next, null, ILCODE.ILLEGAL);
                            return;
                        }
                        else
                        {
                            treeExpr = treeExpr.AsBIN.Operand1;
                            goto AGAIN;
                        }

                    case EXPRKIND.LOGOR:
                    sense = true;
                    goto DOCONDITIONAL;

                    case EXPRKIND.QMARK:
                    colonExpr = treeExpr.AsBIN.Operand2 as EXPRBINOP;
                    if ((colonExpr.Operand1 != null && colonExpr.Operand1.HasSideEffects(compiler)) ||
                        (colonExpr.Operand2 != null && colonExpr.Operand2.HasSideEffects(compiler)))
                    {
                        BBLOCK trueLabelBBlock = GenCondBranch(treeExpr.AsBIN.Operand1, CreateNewBB(false), true);
                        BBLOCK fallThroughLabelBBlock = CreateNewBB(false);
                        GenSideEffects(colonExpr.Operand2);
                        StartNewBB(trueLabelBBlock, ILCODE.BR, fallThroughLabelBBlock, ILCODE.ILLEGAL);
                        GenSideEffects(colonExpr.Operand1);
                        StartNewBB(fallThroughLabelBBlock, ILCODE.next, null, ILCODE.ILLEGAL);
                        return;
                    }
                    else
                    {
                        treeExpr = treeExpr.AsBIN.Operand1;
                        goto AGAIN;
                    }

                    default:
                    break;
                }

                GenSideEffects(treeExpr.AsBIN.Operand1);
                treeExpr = treeExpr.AsBIN.Operand2;
                goto AGAIN;
            }

            switch (treeExpr.Kind)
            {
                case EXPRKIND.ZEROINIT:
                    if (treeExpr.HasSideEffects(compiler))
                    {
                        GenZeroInit(treeExpr as EXPRZEROINIT, false);
                    }
                    break;

                case EXPRKIND.CONCAT:
                case EXPRKIND.PROP:
                    GenExpr(treeExpr, false);
                    break;

                case EXPRKIND.CALL:
                    GenCall(treeExpr as EXPRCALL, false);
                    break;

                case EXPRKIND.NOOP:
                    break;

                case EXPRKIND.DELIM:
                    break;

                case EXPRKIND.FIELD:
                    if (treeExpr.HasSideEffects(compiler))
                    {
                        GenExpr(treeExpr, false);
                        return;
                    }
                    else
                    {
                        treeExpr = (treeExpr as EXPRFIELD).ObjectExpr;
                        goto AGAIN;
                    }

                case EXPRKIND.ARRINIT:
                    treeExpr = (treeExpr as EXPRARRINIT).ArgumentsExpr;
                    goto AGAIN;

                case EXPRKIND.CAST:
                    if ((treeExpr.Flags & (
                        EXPRFLAG.BOX |
                        EXPRFLAG.UNBOX |
                        EXPRFLAG.FORCE_UNBOX |
                        EXPRFLAG.CHECKOVERFLOW |
                        EXPRFLAG.REFCHECK)) != 0)
                    {
                        GenExpr(treeExpr, false);
                    }
                    else
                    {
                        treeExpr = (treeExpr as EXPRCAST).Operand;
                        goto AGAIN;
                    }
                    // fall through otherwise...
                    goto case EXPRKIND.LDTMP;

                case EXPRKIND.LOCAL:
                case EXPRKIND.CONSTANT:
                case EXPRKIND.FUNCPTR:
                case EXPRKIND.TYPEOF:
                case EXPRKIND.SIZEOF:
                case EXPRKIND.LDTMP:
                    return;

                case EXPRKIND.MULTIGET:
                    GenMultiGet((treeExpr as EXPRMULTIGET).MultiExpr, false);
                    return;

                case EXPRKIND.WRAP:
                    EXPRWRAP tempWrap1 = treeExpr as EXPRWRAP;
                    EXPRWRAP tempWrap2 = tempWrap1.Expr as EXPRWRAP;
                    if (tempWrap1.Expr != null &&
                        tempWrap1.Expr.Kind == EXPRKIND.WRAP &&
                        tempWrap2.LocSlotInfo != null &&
                        tempWrap2.DoNotFree)
                    {
                        FREETEMP(ref tempWrap2.LocSlotInfo);
                    }
                    return;

                case EXPRKIND.DBLQMARK:
                    EXPR testExpr = (treeExpr as EXPRDBLQMARK).TestExpr;
                    EXPR convExpr = (treeExpr as EXPRDBLQMARK).ConvertExpr;
                    bool isConv =
                        convExpr.Kind != EXPRKIND.WRAP &&
                        (convExpr.Kind != EXPRKIND.CAST || (convExpr as EXPRCAST).Operand.Kind != EXPRKIND.WRAP) &&
                        convExpr.HasSideEffects(compiler);
                    bool fElse = (treeExpr as EXPRDBLQMARK).ElseExpr.HasSideEffects(compiler);

                    if (isConv == fElse)
                    {
                        if (isConv)
                        {
                            GenExpr(treeExpr, false);
                        }
                        else
                        {
                            GenSideEffects(testExpr);
                        }
                        return;
                    }

                    GenExpr(testExpr, true);
                    if (isConv)
                    {
                        PutILInstruction(ILCODE.DUP);
                    }
                    if (testExpr.TypeSym.IsTYVARSYM)
                    {
                        PutILInstruction(ILCODE.BOX, testExpr.TypeSym);
                    }

                    if (fElse)
                    {
                        BBLOCK doneBBlock = CreateNewBB(false);
                        StartNewBB(null, ILCODE.BRTRUE, doneBBlock, ILCODE.ILLEGAL);
                        --this.currentStackCount;
                        GenSideEffects((treeExpr as EXPRDBLQMARK).ElseExpr);
                        StartNewBB(doneBBlock, ILCODE.next, null, ILCODE.ILLEGAL);
                    }
                    else
                    {
                        DebugUtil.Assert(isConv);

                        BBLOCK doneBBlock = CreateNewBB(false);
                        StartNewBB(null, ILCODE.BRFALSE, doneBBlock, ILCODE.ILLEGAL);
                        --this.currentStackCount;
                        GenExpr((treeExpr as EXPRDBLQMARK).ConvertExpr, true);
                        StartNewBB(doneBBlock, ILCODE.next, null, ILCODE.ILLEGAL);
                        PutILInstruction(ILCODE.POP);
                    }
                    break;

                default:
                    DebugUtil.VsFail("bad expr");
                    break;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenAccess
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <param name="flags"></param>
        /// <param name="addrInfo"></param>
        //------------------------------------------------------------
        private void GenAccess(EXPR treeExpr, AccessTask.Enum flags, ref AddrInfo addrInfo)
        {
            DebugUtil.Assert(AccessTask.FValid(flags));

            if (!AccessTask.FStore(flags))
            {
                addrInfo.Init();
            }

            switch (treeExpr.Kind)
            {
                default:
                    DebugUtil.Assert(false, "bad addr expr");
                    return;

                case EXPRKIND.FIELD:
                    GenFieldAccess(treeExpr as EXPRFIELD, flags, ref addrInfo);
                    return;

                case EXPRKIND.PROP:
                    GenPropAccess(treeExpr as EXPRPROP, flags, ref addrInfo);
                    return;

                case EXPRKIND.ARRINDEX:
                    GenArrayAccess(treeExpr.AsBIN, flags,ref addrInfo);
                    return;

                case EXPRKIND.WRAP:
                    if (!AccessTask.FLoadOrStore(flags))
                    {
                        DebugUtil.Assert(AccessTask.FAddrOnly(flags));
                        return;
                    }
                    EXPRWRAP tempWrap = treeExpr as EXPRWRAP;
                    if (tempWrap.LocSlotInfo == null)
                    {
                        tempWrap.LocSlotInfo = AllocTemporary(tempWrap.TypeSym, tempWrap.TempKind);
                    }
                    DumpLocal(tempWrap.LocSlotInfo, AccessTask.FStore(flags));
                    return;

                case EXPRKIND.LDTMP:
                    DebugUtil.Assert((treeExpr as EXPRLDTMP).TmpExpr.LocSlotInfo != null);
                    if (!AccessTask.FLoadOrStore(flags))
                    {
                        DebugUtil.Assert(AccessTask.FAddrOnly(flags));
                        return;
                    }
                    DumpLocal((treeExpr as EXPRLDTMP).TmpExpr.LocSlotInfo, AccessTask.FStore(flags));
                    return;

                case EXPRKIND.LOCAL:
                    DebugUtil.Assert(!(treeExpr as EXPRLOCAL).LocVarSym.IsConst);
                    LOCSLOTINFO slot = (treeExpr as EXPRLOCAL).LocVarSym.LocSlotInfo;
                    if (!slot.IsReferenceParameter)
                    {
                        if (AccessTask.FLoadOrStore(flags))
                        {
                            DumpLocal(slot, AccessTask.FStore(flags));
                        }
                        return;
                    }

                    if (AccessTask.FAddrOrLoad(flags))
                    {
                        DumpLocal(slot, false);
                        if (AccessTask.FDup(flags))
                        {
                            PutILInstruction(ILCODE.DUP);
                        }
                    }
                    // Indirect.
                    break;

                case EXPRKIND.INDIR:
                    if (AccessTask.FAddrOrLoad(flags))
                    {
                        GenExpr(treeExpr.AsBIN.Operand1, true);
                        if (AccessTask.FDup(flags))
                        {
                            PutILInstruction(ILCODE.DUP);
                        }
                    }
                    // Indirect.
                    break;

                case EXPRKIND.VALUERA:
                    if (AccessTask.FAddrOrLoad(flags))
                    {
                        EmitRefValue(treeExpr.AsBIN);
                        if (AccessTask.FDup(flags))
                        {
                            PutILInstruction(ILCODE.DUP);
                        }
                    }
                    // Indirect.
                    break;
            }
            DebugUtil.Assert(addrInfo.SlotStore == null);
            if (!AccessTask.FLoadOrStore(flags))
            {
                DebugUtil.Assert(AccessTask.FAddrOnly(flags));
                return;
            }

            // This assumes the address is on the stack.
            FUNDTYPE ft = treeExpr.TypeSym.FundamentalType();
            ILCODE code = (AccessTask.FStore(flags) ? ILstackStore[(int)ft] : ILstackLoad[(int)ft]);
            //PutOpcode(code);
            if (code == ILCODE.LDOBJ || code == ILCODE.STOBJ)
            {
                //EmitTypeToken(treeExpr.TypeSym);
                PutILInstruction(code, treeExpr.TypeSym);
            }
            else
            {
                PutILInstruction(code);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenLoad
        //------------------------------------------------------------
        private void GenLoad(EXPR tree)
        {
            AddrInfo addr = new AddrInfo();
            GenAccess(tree, AccessTask.Enum.Load, ref addr);
        }

        // Returns the stack delta (which will always be >= 0).

        //------------------------------------------------------------
        // ILGENREC.GenAddr
        //
        /// <summary></summary>
        /// <param name="tree"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GenAddr(EXPR tree, ref AddrInfo addr)
        {
            int cvalStack = this.currentStackCount;
            GenAccess(tree, AccessTask.Enum.Addr, ref addr);
            DebugUtil.Assert(this.currentStackCount >= cvalStack);
            return this.currentStackCount - cvalStack;
        }

        //------------------------------------------------------------
        // ILGENREC.GenStore
        //------------------------------------------------------------
        private void GenStore(EXPR tree, ref AddrInfo addr)
        {
            GenAccess(tree, AccessTask.Enum.Store, ref addr);
#if DEBUG
            addr.SlotStore = LOCSLOTINFO.INVALID;   // (LOCSLOTINFO)(UINT_PTR)0xC3C3C3C3;
#endif // DEBUG
        }

        //------------------------------------------------------------
        // ILGENREC.GenFieldAccess
        //
        /// <summary></summary>
        /// <param name="fieldExpr"></param>
        /// <param name="flags"></param>
        /// <param name="addrInfo"></param>
        //------------------------------------------------------------
        private void GenFieldAccess(EXPRFIELD fieldExpr, AccessTask.Enum flags, ref AddrInfo addrInfo)
        {
            DebugUtil.Assert(AccessTask.FValid(flags));
            DebugUtil.Assert(addrInfo.SlotStore == null || AccessTask.FStore(flags));

            MEMBVARSYM fieldSym = fieldExpr.FieldWithType.FieldSym;
            AGGTYPESYM aggTypeSym = fieldExpr.FieldWithType.AggTypeSym;
            DebugUtil.Assert((fieldExpr.ObjectExpr != null) == !fieldSym.IsStatic);

            if (!fieldSym.IsStatic && AccessTask.FAddrOrLoad(flags))
            {
                if (flags == AccessTask.Enum.Load && fieldSym.ClassSym.IsValueType)
                {
                    if (aggTypeSym == fieldSym.TypeSym)
                    {
                        // This handles int32 loading its m_value field (also of type int32!).
                        GenExpr(fieldExpr.ObjectExpr, true);
                        return;
                    }
                    if ((fieldExpr.Flags & EXPRFLAG.LVALUE) == 0 &&
                        fieldExpr.ObjectExpr.GetSeqVal().Kind != EXPRKIND.LOCAL &&
                        !fieldSym.ClassSym.IsCLRAmbigStruct())
                    {
                        GenExpr(fieldExpr.ObjectExpr, true);
                    }
                    else
                    {
                        GenObjectPtr(fieldExpr.ObjectExpr, fieldSym, ref addrInfo.SlotStore);
                    }
                }
                else
                {
                    GenObjectPtr(fieldExpr.ObjectExpr, fieldSym, ref addrInfo.SlotStore);
                    if (AccessTask.FDup(flags))
                    {
                        PutILInstruction(ILCODE.DUP);
                    }
                }
            }

            if (!AccessTask.FLoadOrStore(flags))
            {
                DebugUtil.Assert(AccessTask.FAddrOnly(flags));
                return;
            }

            if (fieldSym.IsVolatile)
            {
                PutILInstruction(ILCODE.VOLATILE);
            }

            //if (fieldSym.IsStatic)
            //{
            //    PutOpcode(AccessTask.FStore(flags) ? ILCODE.STSFLD : ILCODE.LDSFLD);
            //}
            //else
            //{
            //    PutOpcode(AccessTask.FStore(flags) ? ILCODE.STFLD : ILCODE.LDFLD);
            //}
            //EmitFieldToken(fieldSym, fieldExpr.FieldWithType.AggTypeSym);

            if (fieldSym.IsStatic)
            {
                if (AccessTask.FStore(flags))
                {
                    PutILInstruction(ILCODE.STSFLD, fieldSym, aggTypeSym);
                }
                else
                {
                    PutILInstruction(ILCODE.LDSFLD, fieldSym, aggTypeSym);
                }
            }
            else
            {
                if (AccessTask.FStore(flags))
                {
                    PutILInstruction(ILCODE.STFLD, fieldSym, aggTypeSym);
                }
                else
                {
                    PutILInstruction(ILCODE.LDFLD, fieldSym, aggTypeSym);
                }
            }

            if (!AccessTask.FAddr(flags) && addrInfo.SlotStore != null)
            {
                FREETEMP(ref addrInfo.SlotStore);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenPropAccess
        //
        /// <summary></summary>
        /// <param name="propExpr"></param>
        /// <param name="flags"></param>
        /// <param name="addrInfo"></param>
        //------------------------------------------------------------
        private void GenPropAccess(EXPRPROP propExpr, AccessTask.Enum flags, ref AddrInfo addrInfo)
        {
            DebugUtil.Assert(AccessTask.FValid(flags));
            DebugUtil.Assert(addrInfo.SlotStore == null || AccessTask.FStore(flags));

            PROPSYM propSym = propExpr.SlotPropWithType.PropSym;
            EXPR expr = propExpr.ObjectExpr;
            bool isConstrained = false;
            EXPR argsExpr = propExpr.ArgumentsExpr;

            // DebugUtil.Assert(type && type.getAggregate() == prop.getClass());
            DebugUtil.Assert((expr != null) == !propSym.IsStatic);

            if ((propExpr.Flags & EXPRFLAG.CONSTRAINED) != 0)
            {
                DebugUtil.Assert((propExpr.Flags & EXPRFLAG.BASECALL) == 0);
                DebugUtil.Assert(!propSym.IsStatic);
                DebugUtil.Assert(
                    expr.Kind == EXPRKIND.CAST &&
                    ((expr as EXPRCAST).Operand.TypeSym.IsTYVARSYM ||
                    (expr as EXPRCAST).Operand.TypeSym.IsValueType()));

                expr = (expr as EXPRCAST).Operand;
                isConstrained = true;
            }

            if (AccessTask.FAddrOrLoad(flags))
            {
                // Generate the "address" on the stack. The address consists of:
                //
                // 1) The object if there is one. If (isConstrained), we need a memory address,
                //    not the value.
                // 2) The argsExpr (for indexers).
                //
                // If FDup(flags), we need to duplicate the entire "address" on the stack.
                if (isConstrained)
                {
                    GenMemoryAddress(expr, ref addrInfo.SlotStore, false, true);
                }
                else if (!propSym.IsStatic)
                {
                    GenObjectPtr(expr, propSym, ref addrInfo.SlotStore);
                }

                if (AccessTask.FDup(flags))
                {
                    GenArgsDup(expr, isConstrained, argsExpr, false);
                }
                else
                {
                    GenExpr(argsExpr,true);
                }
            }

            if (!AccessTask.FLoadOrStore(flags))
            {
                DebugUtil.Assert(AccessTask.FAddrOnly(flags));
                return;
            }

            MethWithType mwt = AccessTask.FStore(flags) ? propExpr.SetMethodWithType : propExpr.GetMethodWithType;
            DebugUtil.Assert(mwt.Sym != null && mwt.Sym.IsMETHSYM && !mwt.MethSym.IsStatic == !propSym.IsStatic);

#if USAGEHACK
    mwt.MethSym.IsUsed = true;
#endif

            if (isConstrained)
            {
                DebugUtil.Assert(expr.TypeSym.IsTYVARSYM || expr.TypeSym.IsValueType());

                PutILInstruction(ILCODE.CONSTRAINED, expr.TypeSym);
                PutILInstruction(ILCODE.CALLVIRT, mwt.MethSym, mwt.AggTypeSym, null);
            }
            else if (CallAsVirtual(mwt.MethSym, expr, (propExpr.Flags & EXPRFLAG.BASECALL) != 0))
            {
                PutILInstruction(ILCODE.CALLVIRT, mwt.MethSym, mwt.AggTypeSym, null);
            }
            else
            {
                PutILInstruction(ILCODE.CALL, mwt.MethSym, mwt.AggTypeSym, null);
            }
            // PutILInstruction methos include the operations of EmitMethodToken
            //EmitMethodToken(mwt.MethSym, mwt.AggTypeSym, null);

            if (this.compileForEnc && AccessTask.FStore(flags))
            {
                PutILInstruction(ILCODE.NOP);
            }

            this.currentStackCount -= mwt.MethSym.ParameterTypes.Count;
            if (!mwt.MethSym.IsStatic)
            {
                --this.currentStackCount;
            }
            DebugUtil.Assert(this.currentStackCount >= 0);

            if (AccessTask.FLoad(flags))
            {
                ++this.currentStackCount;
                MarkStackMax();
            }

            if (!AccessTask.FAddr(flags) && addrInfo.SlotStore != null)
            {
                FREETEMP(ref addrInfo.SlotStore);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenArrayAccess
        //
        /// <summary></summary>
        /// <param name="binopExpr"></param>
        /// <param name="flags"></param>
        /// <param name="addrInfo"></param>
        //------------------------------------------------------------
        private void GenArrayAccess(EXPRBINOP binopExpr, AccessTask.Enum flags, ref AddrInfo addrInfo)
        {
            DebugUtil.Assert(AccessTask.FValid(flags));
            DebugUtil.Assert(addrInfo.SlotStore == null);
            DebugUtil.Assert(binopExpr.Kind == EXPRKIND.ARRINDEX);
            DebugUtil.Assert(binopExpr.TypeSym == (binopExpr.Operand1.TypeSym as ARRAYSYM).ElementTypeSym);

            ARRAYSYM arraySym = binopExpr.Operand1.TypeSym as ARRAYSYM;
            bool isValueType = binopExpr.TypeSym.IsStructOrEnum();
            FUNDTYPE ft = binopExpr.TypeSym.FundamentalType();

            DebugUtil.Assert(GetArgCount(binopExpr.Operand2) == arraySym.Rank);

            if (AccessTask.FAddrOrLoad(flags))
            {
                DebugUtil.Assert(!addrInfo.IndirectArray);

                // Generate the "address" on the stack. The address is either:
                //
                // 1) A memory address when addrInfo.fIndirectArray is set to true. This is when
                //    the element type is a value type and FDup is true or when the element type
                //    is a non-trivial struct. Duplicating a single memory address is much more
                //    efficient than duplicating the other address form.
                //
                // 2) The object followed by the indices. We always use this form for reference
                //    types, since ldelema causes a type check which may fail when our assignment
                //    may very well succeed.

                // Use indirection only on value types since ldelema on a reference type array
                // causes a type check (because of array covariance).

                addrInfo.IndirectArray = isValueType && (AccessTask.FDup(flags) || ft == FUNDTYPE.STRUCT);

                // Load the array
                GenExpr(binopExpr.Operand1,true);

                if (addrInfo.IndirectArray)
                {
                    GenExpr(binopExpr.Operand2,true);
                    if (arraySym.Rank == 1)
                    {
                        PutILInstruction(ILCODE.LDELEMA, binopExpr.TypeSym);
                    }
                    else
                    {
                        GenArrayCall(arraySym, arraySym.Rank, ARRAYMETHOD.LOADADDR);
                    }
                    if (AccessTask.FDup(flags))
                    {
                        PutILInstruction(ILCODE.DUP);
                    }
                }
                else if (AccessTask.FDup(flags))
                {
                    GenArgsDup(binopExpr.Operand1, false, binopExpr.Operand2, true);
                }
                else
                {
                    GenExpr(binopExpr.Operand2,true);
                }
            }

            if (!AccessTask.FLoadOrStore(flags))
            {
                DebugUtil.Assert(AccessTask.FAddrOnly(flags));
                return;
            }

            if (addrInfo.IndirectArray)
            {
                //PutOpcode(AccessTask.FStore(flags) ? ILCODE.STOBJ : ILCODE.LDOBJ);
                //EmitTypeToken(binopExpr.TypeSym);
                if (AccessTask.FStore(flags))
                {
                    PutILInstruction(ILCODE.STOBJ, binopExpr.TypeSym);
                }
                else
                {
                    PutILInstruction(ILCODE.LDOBJ, binopExpr.TypeSym);
                }
            }
            else if (arraySym.Rank == 1)
            {
                ILCODE code;

                if (AccessTask.FStore(flags))
                {
                    code = ILarrayStore[(int)ft];
                    //PutOpcode(code);
                    if (code == ILCODE.STELEM)
                    {
                        PutILInstruction(code, binopExpr.TypeSym);
                    }
                    else
                    {
                        PutILInstruction(code);
                    }
                }
                else
                {
                    code = ILarrayLoad[(int)ft];
                    //PutOpcode(code);
                    if (code == ILCODE.LDELEM)
                    {
                        PutILInstruction(code, binopExpr.TypeSym);
                    }
                    else
                    {
                        PutILInstruction(code);
                    }
                }
            }
            else
            {
                GenArrayCall(
                    arraySym,
                    arraySym.Rank,
                    AccessTask.FStore(flags) ? ARRAYMETHOD.STORE : ARRAYMETHOD.LOAD);
            }
            DebugUtil.Assert(addrInfo.SlotStore == null);
        }

        //------------------------------------------------------------
        // ILGENREC.GenArgsDup
        //
        /// <summary></summary>
        /// <param name="expr"></param>
        /// <param name="isConstrained"></param>
        /// <param name="argsExpr"></param>
        /// <param name="isArray"></param>
        //------------------------------------------------------------
        private void GenArgsDup(EXPR expr, bool isConstrained, EXPR argsExpr, bool isArray)
        {
            // The object should be on the stack coming into this (if there is one).
            // The stack value is assumed to be indirect iff isConstrained or expr.TypeSym
            // is a value type.

            if (expr != null)
            {
                PutILInstruction(ILCODE.DUP);
            }

            if (argsExpr == null)
            {
                return;
            }

            LOCSLOTINFO slotObj = null;

            // Duplicate and store the object if the object addr is not constant.
            if (expr != null)
            {
                TYPESYM typeSym = expr.TypeSym;
                if (isConstrained || typeSym.IsStructOrEnum())
                {
                    DebugUtil.Assert(!isArray);
                    typeSym = compiler.MainSymbolManager.GetParamModifier(typeSym, false);
                }
                slotObj = AllocTemporary(typeSym, TEMP_KIND.SHORTLIVED);
                DumpLocal(slotObj, true);
            }

            // Count the number of non-constant ar^gs
            int cslot = 0;
            EXPR ex;

            ex = argsExpr;
            while (ex != null)
            {
                EXPR arg;
                if (ex.Kind == EXPRKIND.LIST)
                {
                    arg = ex.AsBIN.Operand1;
                    ex = ex.AsBIN.Operand2;
                }
                else
                {
                    arg = ex;
                    ex = null;
                }

                if (arg.GetConst() == null)
                {
                    ++cslot;
                }
            }

            //LOCSLOTINFO  prgslot = (cslot > 0) ? (PSLOT *)STACK_ALLOC(PSLOT, cslot) : null;
            LOCSLOTINFO[] prgslot = null;
            if (cslot > 0)
            {
                prgslot = new LOCSLOTINFO[cslot];
            }

            int islot = 0;
            ex = argsExpr;
            while (ex != null)
            {
                EXPR arg;
                if (ex.Kind == EXPRKIND.LIST)
                {
                    arg = ex.AsBIN.Operand1;
                    ex = ex.AsBIN.Operand2;
                }
                else
                {
                    arg = ex;
                    ex = null;
                }

                GenExpr(arg, true);
                if (arg.GetConst() != null)
                {
                    continue;
                }

                PutILInstruction(ILCODE.DUP);
                TYPESYM typeSym = isArray ? compiler.MainSymbolManager.NaturalIntTypeSym : arg.TypeSym;
                prgslot[islot++] = STOREINTEMP(typeSym, TEMP_KIND.SHORTLIVED);
            }
            DebugUtil.Assert(islot == cslot);

            if (expr != null)
            {
                DumpLocal(slotObj, false);
            }

            islot = 0;
            ex = argsExpr;
            while (ex != null)
            {
                EXPR arg;
                if (ex.Kind == EXPRKIND.LIST)
                {
                    arg = ex.AsBIN.Operand1;
                    ex = ex.AsBIN.Operand2;
                }
                else
                {
                    arg = ex;
                    ex = null;
                }

                if (arg.GetConst() != null)
                {
                    // Don't gen the side effects again - just the value.
                    GenExpr(arg.GetConst(), true);
                }
                else
                {
                    DumpLocal(prgslot[islot++], false);
                }
            }
            DebugUtil.Assert(islot == cslot);

            if (slotObj != null)
            {
                FreeTemporary(slotObj);
            }

            for (islot = 0; islot < cslot; islot++)
            {
                FreeTemporary(prgslot[islot]);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GetArgCount
        //
        /// <summary></summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetArgCount(EXPR args)
        {
            int rval = 0;
            EXPR expr = args;
            while (expr != null)
            {
                EXPR arg;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg = expr;
                    expr = null;
                }
                ++rval;
            }
            return rval;
        }

        //------------------------------------------------------------
        // ILGENREC.GenMemoryAddress
        //
        /// <summary>
        /// <para>(sscli) ptrAddr and readOnly have the default value false.</para>
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <param name="locSlotInfo"></param>
        /// <param name="ptrAddr"></param>
        /// <param name="readOnly"></param>
        //------------------------------------------------------------
        private void GenMemoryAddress(
            EXPR treeExpr,
            ref LOCSLOTINFO locSlotInfo,
            bool ptrAddr,   // = false
            bool readOnly)  // = false
        {
            TYPESYM typeSym;

        LRestart:
            switch (treeExpr.Kind)
            {
                case EXPRKIND.WRAP:
                    EXPRWRAP tempWrap = treeExpr as EXPRWRAP;
                    if (tempWrap.LocSlotInfo == null)
                    {
                        if (tempWrap.NeedEmptySlot)
                        {
                            tempWrap.LocSlotInfo =
                                AllocTemporary(treeExpr.TypeSym, tempWrap.TempKind);
                        }
                        else
                        {
                            // we need to get the thing from the stack and load its address
                            tempWrap.LocSlotInfo =
                                STOREINTEMP(treeExpr.TypeSym, tempWrap.TempKind);
                            locSlotInfo = tempWrap.LocSlotInfo;
                        }
                    }
                    GenSlotAddress(tempWrap.LocSlotInfo, false);
                    return;

                case EXPRKIND.LDTMP:
                    DebugUtil.Assert((treeExpr as EXPRLDTMP).TmpExpr.LocSlotInfo != null);
                    GenSlotAddress((treeExpr as EXPRLDTMP).TmpExpr.LocSlotInfo, false);
                    return;

                case EXPRKIND.STTMP:
                    DebugUtil.Assert((treeExpr as EXPRSTTMP).LocSlotInfo == null);
                    GenExpr(treeExpr, false);
                    DebugUtil.Assert((treeExpr as EXPRSTTMP).LocSlotInfo != null);
                    GenSlotAddress((treeExpr as EXPRSTTMP).LocSlotInfo, false);
                    return;

                case EXPRKIND.LOCAL:
                    DebugUtil.Assert(!(treeExpr as EXPRLOCAL).LocVarSym.IsConst);
                    GenSlotAddress((treeExpr as EXPRLOCAL).LocVarSym.LocSlotInfo, ptrAddr);
                    return;

                case EXPRKIND.VALUERA:
                    EmitRefValue(treeExpr.AsBIN);
                    return;

                case EXPRKIND.INDIR:
                    GenExpr(treeExpr.AsBIN.Operand1, true);
                    return;

                case EXPRKIND.FIELD:
                    if ((treeExpr.Flags & EXPRFLAG.LVALUE) == 0)
                    {
                        DebugUtil.Assert(!ptrAddr);
                        GenExpr(treeExpr, true);
                        locSlotInfo = STOREINTEMP(treeExpr.TypeSym, TEMP_KIND.SHORTLIVED);
                        GenSlotAddress(locSlotInfo, false);
                        return;
                    }

                    if ((treeExpr as EXPRFIELD).FieldWithType.FieldSym.IsStatic)
                    {
                        DebugUtil.Assert((treeExpr as EXPRFIELD).ObjectExpr == null);
                        FieldWithType fwt = (treeExpr as EXPRFIELD).FieldWithType;
                        PutILInstruction(ILCODE.LDSFLDA, fwt.FieldSym, fwt.AggTypeSym);
                    }
                    else
                    {
                        GenObjectPtr(
                            (treeExpr as EXPRFIELD).ObjectExpr,
                            (treeExpr as EXPRFIELD).FieldWithType.FieldSym,
                            ref locSlotInfo);
                        if (!ptrAddr && (treeExpr as EXPRFIELD).ObjectExpr.TypeSym.IsPTRSYM)
                        {
                            FieldWithType fwt = (treeExpr as EXPRFIELD).FieldWithType;
                            if (fwt.FieldSym.IsVolatile)
                            {
                                PutILInstruction(ILCODE.VOLATILE);
                            }
                            PutILInstruction(ILCODE.LDFLD, fwt.FieldSym, fwt.AggTypeSym);
                        }
                        else
                        {
#if DEBUG
                            DebugUtil.Assert((treeExpr as EXPRFIELD).CheckedMarshalByRef);
#endif
                            FieldWithType fwt = (treeExpr as EXPRFIELD).FieldWithType;
                            PutILInstruction(ILCODE.LDFLDA, fwt.FieldSym, fwt.AggTypeSym);
                        }
                    }
                    //EmitFieldToken(
                    //    (treeExpr as EXPRFIELD).FieldWithType.FieldSym,
                    //    (treeExpr as EXPRFIELD).FieldWithType.AggTypeSym);
                    return;

                case EXPRKIND.ARRINDEX:
                    GenExpr(treeExpr.AsBIN.Operand1, true);
                    GenExpr(treeExpr.AsBIN.Operand2, true);
                    if (((typeSym = treeExpr.AsBIN.Operand1.TypeSym) as ARRAYSYM).Rank == 1)
                    {
                        if (readOnly && ((typeSym as ARRAYSYM).ElementTypeSym.IsTYVARSYM ||
                            (typeSym as ARRAYSYM).ElementTypeSym.IsReferenceType()))
                        {
                            PutILInstruction(ILCODE.READONLY);
                        }
                        PutILInstruction(ILCODE.LDELEMA, (typeSym as ARRAYSYM).ElementTypeSym);
                    }
                    else
                    {
                        GenArrayCall(
                            typeSym as ARRAYSYM,
                            (typeSym as ARRAYSYM).Rank > 0 ? (typeSym as ARRAYSYM).Rank : GetArgCount(treeExpr.AsBIN.Operand2),
                            ARRAYMETHOD.LOADADDR);
                    }
                    return;

                case EXPRKIND.SEQUENCE:
                    GenSideEffects(treeExpr.AsBIN.Operand1);
                    treeExpr = treeExpr.AsBIN.Operand2;
                    goto LRestart;

                case EXPRKIND.SEQREV:
                    GenMemoryAddress(treeExpr.AsBIN.Operand1, ref locSlotInfo, ptrAddr, readOnly);
                    GenSideEffects(treeExpr.AsBIN.Operand2);
                    return;

                default:
                    typeSym = treeExpr.TypeSym;
                    if (typeSym.IsPARAMMODSYM)
                    {
                        typeSym = (typeSym as PARAMMODSYM).ParamTypeSym;
                        DebugUtil.Assert(treeExpr.Kind == EXPRKIND.PROP);
                    }
                    GenExpr(treeExpr, true);
                    locSlotInfo = STOREINTEMP(typeSym, TEMP_KIND.SHORTLIVED);
                    GenSlotAddress(locSlotInfo, false);
                    return;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenDupSafeMemAddr
        //
        /// <summary>
        /// Generate a memory address that is safe to duplicate
        /// on the stack without concern for concurrency issues.
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <param name="locSlotInfo"></param>
        //------------------------------------------------------------
        private void GenDupSafeMemAddr(EXPR treeExpr, ref LOCSLOTINFO locSlotInfo)
        {
            TYPESYM typeSym;

        LRestart:
            switch (treeExpr.Kind)
            {
                case EXPRKIND.WRAP:
                case EXPRKIND.LDTMP:
                case EXPRKIND.STTMP:
                case EXPRKIND.LOCAL:
                    GenMemoryAddress(treeExpr, ref locSlotInfo, false, false);
                    return;

                case EXPRKIND.SEQUENCE:
                    GenSideEffects(treeExpr.AsBIN.Operand1);
                    treeExpr = treeExpr.AsBIN.Operand2;
                    goto LRestart;

                case EXPRKIND.SEQREV:
                    GenDupSafeMemAddr(treeExpr.AsBIN.Operand1, ref locSlotInfo);
                    GenSideEffects(treeExpr.AsBIN.Operand2);
                    return;

                default:
                    typeSym = treeExpr.TypeSym;
                    if (typeSym.IsPARAMMODSYM)
                    {
                        typeSym = (typeSym as PARAMMODSYM).ParamTypeSym;
                        DebugUtil.Assert(treeExpr.Kind == EXPRKIND.PROP);
                    }
                    GenExpr(treeExpr, true);
                    locSlotInfo = STOREINTEMP(typeSym, TEMP_KIND.SHORTLIVED);
                    GenSlotAddress(locSlotInfo, false);
                    return;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenSlotAddress
        //
        /// <summary>
        /// <para>(sscli)ptrAddr has the default value false.</para>
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="ptrAddr"></param>
        //------------------------------------------------------------
        private void GenSlotAddress(
            LOCSLOTINFO slot,
            bool ptrAddr)	// = false
        {
            DebugUtil.Assert(slot.HasIndex);

            if (slot.IsReferenceParameter ||
                (!ptrAddr && (slot.TypeSym!=null && slot.TypeSym.IsPTRSYM)))
            {
                DumpLocal(slot, false);
                return;
            }

            DebugUtil.Assert((slot.Index & ~0xFFFF) == 0);
            int size = 1;
            if ((slot.Index & ~0xFF) == 0)
            {
                size = 0; // so its small
            }
            //PutOpcode(ILaddrLoad[(slot.IsParameter ? 1 : 0), size]);
            if (size != 0)
            {
                PutILInstruction(
                    ILaddrLoad[(slot.IsParameter ? 1 : 0), size],
                    (ushort)slot.Index);
            }
            else
            {
                PutILInstruction(
                    ILaddrLoad[(slot.IsParameter ? 1 : 0), size],
                    (char)slot.Index);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenReturn
        //
        /// <summary>
        /// generate code for a return statement
        /// </summary>
        /// <param name="returnExpr"></param>
        //------------------------------------------------------------
        private void GenReturn(EXPRRETURN returnExpr)
        {
            //--------------------------------------------------------
            // load the return value to the stack.
            //--------------------------------------------------------
            if (returnExpr.ObjectExpr != null)
            {
                GenExpr(returnExpr.ObjectExpr, true);
            }

            //--------------------------------------------------------
            // If leave instruction is required,
            //--------------------------------------------------------
            if ((returnExpr.Flags & EXPRFLAG.ASLEAVE) != 0 ||
                !compiler.OptionManager.Optimize)
            {
                if (this.returnLocationBBlock == null)
                {
                    this.returnLocationBBlock = CreateNewBB(false);
                    this.returnHandled = false;
                }

                if (this.temporaryReturnSlotInfo != null)
                {
                    DumpLocal(this.temporaryReturnSlotInfo, true);
                }
                else if (returnExpr.ObjectExpr != null)
                {
                    // this could only happen if this is unreachable code...
                    this.currentStackCount--;
                }

                if ((returnExpr.Flags & EXPRFLAG.FINALLYBLOCKED) != 0)
                {
                    this.currentBBlock.GotoBlocked = true;  // inlineBBlock
                    this.blockedLeave++;
                }

                if ((returnExpr.Flags & EXPRFLAG.ASLEAVE) != 0)
                {
                    StartNewBB(
                        null,
                        ILCODE.LEAVE,
                        this.returnLocationBBlock,
                        ILCODE.ILLEGAL);
                }
                else
                {
                    StartNewBB(
                        null,
                        ILCODE.BR,
                        this.returnLocationBBlock,
                        ILCODE.ILLEGAL);
                }
            }
            //--------------------------------------------------------
            // If use br instruction,
            //--------------------------------------------------------
            else
            {
                HandleReturn(false);
                StartNewBB(
                    null,
                    ILCODE.RET,
                    null,
                    ILCODE.ILLEGAL);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenGoto
        //
        /// <summary>
        /// <para>generate code for a goto...</para>
        /// </summary>
        /// <param name="gotoExpr"></param>
        //------------------------------------------------------------
        private void GenGoto(EXPRGOTO gotoExpr)
        {
            DebugUtil.Assert((gotoExpr.Flags & EXPRFLAG.UNREALIZEDGOTO) == 0);

            BBLOCK destBBlock = gotoExpr.LabelExpr.BBlock;
            if (destBBlock == null)
            {
                destBBlock = gotoExpr.LabelExpr.BBlock = CreateNewBB(false);
            }
            //destBBlock.IsDestination = true;

            if ((gotoExpr.Flags & EXPRFLAG.ASLEAVE) != 0)
            {
                if ((gotoExpr.Flags & EXPRFLAG.FINALLYBLOCKED) != 0)
                {
                    this.currentBBlock.GotoBlocked = true;  // inlineBBlock
                    this.blockedLeave++;
                }
                StartNewBB(null, ILCODE.LEAVE, destBBlock, ILCODE.ILLEGAL);
            }
            else
            {
                StartNewBB(null, ILCODE.BR, destBBlock, ILCODE.ILLEGAL);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.genGotoIf
        //
        /// <summary>
        /// <para>generate code for a gotoif...</para>
        /// <para>Generate codes to calculate the condition and
        /// set destination BBlock to this.currentBBlock.</para>
        /// </summary>
        /// <param name="gotoifExpr"></param>
        //------------------------------------------------------------
        private void GenGotoIf(EXPRGOTOIF gotoifExpr)
        {
            // this had to be taken care of by the def-use pass...
            DebugUtil.Assert((gotoifExpr.Flags & EXPRFLAG.UNREALIZEDGOTO) == 0);

            BBLOCK destBBlock = gotoifExpr.LabelExpr.BBlock;
            if (destBBlock == null)
            {
                destBBlock = gotoifExpr.LabelExpr.BBlock = CreateNewBB(false);
            }

            if (this.compileForEnc)
            {
                EXPR.CONSTRESKIND crk = 0;
                GenCondExpr(gotoifExpr.ConditionExpr, gotoifExpr.HasSense, ref crk);
                bool dumpToStack = crk == EXPR.CONSTRESKIND.NotConst;
                DumpToDurable(compiler.GetReqPredefType(PREDEFTYPE.BOOL,true), true, dumpToStack);
                ILCODE il;

                switch (crk)
                {
                    case EXPR.CONSTRESKIND.False:
                        destBBlock = null;
                        il = ILCODE.next;
                        break;

                    case EXPR.CONSTRESKIND.True:
                        il = ILCODE.BR;
                        break;

                    default:
                        DebugUtil.Assert(false);
                        goto case EXPR.CONSTRESKIND.NotConst;

                    case EXPR.CONSTRESKIND.NotConst:
                        il = ILCODE.BRTRUE;
                        break;
                }

                StartNewBB(null, il, destBBlock,ILCODE.ILLEGAL);
                if (il != ILCODE.next)
                {
                    this.currentStackCount += ILStackOps[(int)il];
                    MarkStackMax();
                }
            }
            else
            {
                GenCondBranch(gotoifExpr.ConditionExpr, destBBlock, gotoifExpr.HasSense);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenLabel
        //
        /// <summary>
        /// generate code for a label, this merely involves starting the right bblock.
        /// </summary>
        /// <param name="labelExpr"></param>
        //------------------------------------------------------------
        private void GenLabel(EXPRLABEL labelExpr)
        {
            if (labelExpr.BBlock != null)
            {
                // if we got a block for this, then just make this the current block...
                StartNewBB(labelExpr.BBlock, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            else
            {
                StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
                labelExpr.BBlock = this.currentBBlock;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenCondBranch
        //
        /// <summary>
        /// generate a jump to dest if condition == sense is true
        /// </summary>
        /// <param name="conditionExpr"></param>
        /// <param name="destBBlock"></param>
        /// <param name="sense"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private BBLOCK GenCondBranch(EXPR conditionExpr, BBLOCK destBBlock, bool sense)
        {
            DebugUtil.Assert(destBBlock != null);
            //destBBlock.IsDestination = true;

            ILCODE ilcode = ILCODE.NOP;
            ILCODE ilcodeRev = ILCODE.ILLEGAL;
            EXPR.CONSTRESKIND crk;

        AGAIN:

            ilcodeRev = ILCODE.ILLEGAL; // Default.

            switch (conditionExpr.Kind)
            {
                case EXPRKIND.LOGAND:
                    // see comment in genCondExpr
                    if (!sense)
                    {
                        goto LOGOR;
                    }
                LOGAND:
                    // we generate:
                    // gotoif(a != sense) labFT
                    // gotoif(b == sense) labDest
                    // labFT
                    BBLOCK fallThrough = GenCondBranch(conditionExpr.AsBIN.Operand1, CreateNewBB(false), !sense);
                    GenCondBranch(conditionExpr.AsBIN.Operand2, destBBlock, sense);
                    StartNewBB(fallThrough, ILCODE.next, null, ILCODE.ILLEGAL);
                    return destBBlock;

                case EXPRKIND.LOGOR:
                    if (!sense)
                    {
                        goto LOGAND;
                    }
                LOGOR:
                    // we generate:
                    // gotoif(a == sense) labDest
                    // gotoif(b == sense) labDest
                    GenCondBranch(conditionExpr.AsBIN.Operand1, destBBlock, sense);
                    conditionExpr = conditionExpr.AsBIN.Operand2;
                    goto AGAIN;

                case EXPRKIND.CONSTANT:
                    // make sure that only the bool bits are set:
                    DebugUtil.Assert(
                        (conditionExpr as EXPRCONSTANT).ConstVal.GetInt() == 0 ||
                        (conditionExpr as EXPRCONSTANT).ConstVal.GetInt() == 1);

                    if ((conditionExpr as EXPRCONSTANT).ConstVal.GetInt() == (sense ? 1 : 0))
                    {
                        ilcode = ILCODE.BR;
                        break;
                    }
                    // otherwise this branch will never be taken, so just fall through...
                    return destBBlock;

                case EXPRKIND.EQ:
                case EXPRKIND.NE:
                    if (IsSimpleExpr(conditionExpr, ref sense))
                    {
                        goto SIMPLEBR;
                    }
                    // Fall through
                    goto case EXPRKIND.GE;

                case EXPRKIND.LT:
                case EXPRKIND.LE:
                case EXPRKIND.GT:
                case EXPRKIND.GE:
                    GenExpr(conditionExpr.AsBIN.Operand1, true);
                    GenExpr(conditionExpr.AsBIN.Operand2, true);

                    ilcode = CodeForJump(conditionExpr.Kind, conditionExpr.AsBIN.Operand1.TypeSym, sense, ref ilcodeRev);
                    break;

                case EXPRKIND.LOGNOT:
                    sense = !sense;
                    conditionExpr = conditionExpr.AsBIN.Operand1;
                    goto AGAIN;

                case EXPRKIND.IS:
                case EXPRKIND.AS:
                    if (conditionExpr.AsBIN.Operand2.Kind == EXPRKIND.TYPEOF)
                    {
                        GenExpr(conditionExpr.AsBIN.Operand1, true);
                        PutILInstruction(
                            ILCODE.ISINST,
                            (conditionExpr.AsBIN.Operand2 as EXPRTYPEOF).SourceTypeSym);
                        goto SIMPLEBR;
                    }
                    break;

                case EXPRKIND.SEQUENCE:
                    GenSideEffects(conditionExpr.AsBIN.Operand1);
                    conditionExpr = conditionExpr.AsBIN.Operand2;
                    goto AGAIN;

                case EXPRKIND.SEQREV:
                    if (!conditionExpr.AsBIN.Operand2.HasSideEffects(compiler))
                    {
                        conditionExpr = conditionExpr.AsBIN.Operand1;
                        goto AGAIN;
                    }
                    goto default;

                default:
                    if ((crk = conditionExpr.GetConstantResult()) != EXPR.CONSTRESKIND.NotConst)
                    {
                        GenExpr(conditionExpr, false);
                        if (EXPR.ConstantMatchesSense(crk, sense))
                        {
                            ilcode = ILCODE.BR;
                            break;
                        }
                        else
                        {
                            return destBBlock;
                        }
                    }
                    GenExpr(conditionExpr, true);
                SIMPLEBR:
                    ilcode = sense ? ILCODE.BRTRUE : ILCODE.BRFALSE;
                    break;
            }
            DebugUtil.Assert(ilcodeRev == ILCODE.ILLEGAL || ILStackOps[(int)ilcodeRev] == ILStackOps[(int)ilcode]);
            StartNewBB(null, ilcode, destBBlock, ilcodeRev);

            // since we are not emitting the instruction, but rather are saving it in the bblock,
            // we need to manipulate the stack ourselves...
            this.currentStackCount += ILStackOps[(int)ilcode];
            MarkStackMax();

            return destBBlock;
        }

        //------------------------------------------------------------
        // ILGENREC.IsSimpleExpr
        //
        /// <summary></summary>
        /// <param name="conditionExpr"></param>
        /// <param name="sense"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsSimpleExpr(EXPR conditionExpr, ref bool sense)
        {
            EXPR op1 = conditionExpr.AsBIN.Operand1;
            EXPR op2 = conditionExpr.AsBIN.Operand2;

            bool c1 = (op1.Kind == EXPRKIND.CONSTANT);
            bool c2 = (op2.Kind == EXPRKIND.CONSTANT);

            if (!c1 && !c2)
            {
                return false;
            }

            EXPR constOp;
            EXPR nonConstOp;

            if (c1)
            {
                constOp = op1;
                nonConstOp = op2;
            }
            else
            {
                constOp = op2;
                nonConstOp = op1;
            }

            FUNDTYPE ft = nonConstOp.TypeSym.FundamentalType();
            if (ft == FUNDTYPE.NONE || (ft >= FUNDTYPE.I8 && ft <= FUNDTYPE.R8))
            {
                return false;
            }

            bool isBool = nonConstOp.TypeSym.IsPredefType(PREDEFTYPE.BOOL);
            bool isZero = (ft == FUNDTYPE.I8 || ft == FUNDTYPE.U8) ?
                (constOp as EXPRCONSTANT).ConstVal.GetLong() == 0 :
                (constOp as EXPRCONSTANT).ConstVal.GetInt() == 0;

            // bool is special, only it can be compared to true and false...
            if (!isBool && !isZero)
            {
                return false;
            }

            // if comparing to zero, flip the sense
            if (isZero)
            {
                sense = !sense;
            }

            // if comparing != flip the sense
            if (conditionExpr.Kind == EXPRKIND.NE)
            {
                sense = !sense;
            }
            GenExpr(nonConstOp, true);
            return true;
        }

        //------------------------------------------------------------
        // ILGENREC.GenCondExpr
        //
        /// <summary>
        /// generate a conditional (ie, boolean) expression...
        /// this will leave a value on the stack which conforms to sense, ie:
        /// (condition == sense)
        /// <para>(sscli) constresKind has the default value null,
        /// but in C#, null cannot be specified for ref parameter.</para>
        /// </summary>
        /// <param name="conditionExpr"></param>
        /// <param name="sense"></param>
        /// <param name="constresKind"></param>
        //------------------------------------------------------------
        private void GenCondExpr(EXPR conditionExpr, bool sense, ref EXPR.CONSTRESKIND constresKind)
        {
            constresKind = EXPR.CONSTRESKIND.NotConst;
            BBLOCK fallThrough = null;
            EXPR.CONSTRESKIND dummyCK = 0;

        LRecurse:

#if DEBUG
            int prevStack = this.currentStackCount;
            if ((conditionExpr.Flags & EXPRFLAG.BINOP) != 0 &&
                conditionExpr.AsBIN.Operand1.Kind == EXPRKIND.WRAP)
            {
                --prevStack;
            }
#endif

            switch (conditionExpr.Kind)
            {
                case EXPRKIND.LOGAND:
                    // remember that a == false is ~a, and so ~(a && b) is done as (~a || ~b)...

                    // E1 = (a == true && b == true)
                    // E1 == false  <=>  a == false || b == false
                    // E2 = (a == true || b == true)
                    // E2 == false  <=>  a == false && b == false

                    if (!sense)
                    {
                        goto LLogOr;
                    }

                LLogAnd:
                    // we generate:
                    // gotoif (a != sense) lab0
                    // b == sense
                    // goto labEnd
                    // lab0
                    // 0
                    // labEnd

                    // Generate code to check
                    //     E = (a == ture && b == true) holds or
                    //     E = (a == false && b == false) holds (that is a == true || b == true does not hold)
                    //
                    // sense is a boolean value)
                    //     If (a == sense) does not holds, the value of E is false (or 0).
                    //     If (a == sense) holds, the value of E is (b == sense).

                    //     gotoif (a != sense) Label_1
                    //     E = (b == sense)
                    //     goto Label_2
                    // Label_1:
                    //     E = 0
                    // Label_2:
                    //     check E

                    fallThrough = GenCondBranch(conditionExpr.AsBIN.Operand1, CreateNewBB(false), !sense);
                    GenCondExpr(conditionExpr.AsBIN.Operand2, sense, ref dummyCK);
#if DEBUG
                    DebugUtil.Assert(prevStack + 1 == this.currentStackCount);
#endif
                    --this.currentStackCount;
                    BBLOCK labEnd = CreateNewBB(false);
                    StartNewBB(fallThrough, ILCODE.BR, labEnd, ILCODE.ILLEGAL);
                    GenIntConstant(0);
                    StartNewBB(labEnd, ILCODE.next, null, ILCODE.ILLEGAL);
                    break;

                case EXPRKIND.LOGOR:
                    // as above, ~(a || b) is (~a && ~b)
                    if (!sense)
                    {
                        goto LLogAnd;
                    }

                LLogOr:
                    // we generate:
                    // gotoif (a == sense) lab1
                    // b == sense
                    // goto labEnd
                    // lab1
                    // 1
                    // labEnd

                    // Generate code to check
                    //     E = (a == ture || b == true) holds or
                    //     E = (a == false || b == false) holds (that is a == true && b == true does not hold)
                    //
                    // sense is a boolean value)
                    //     If (a == sense) holds, the value of E is true (or 1).
                    //     If (a == sense) does not hold, the value of E is (b == sense).

                    //     gotoif (a == sense) Label_1
                    //     E = (b == sense)
                    //     goto Label_2
                    // Label_1:
                    //     E = true
                    // Label_2:
                    //     check E

                BBLOCK ldOne = GenCondBranch(conditionExpr.AsBIN.Operand1, CreateNewBB(false), sense);
                    GenCondExpr(conditionExpr.AsBIN.Operand2, sense, ref dummyCK);
#if DEBUG
                    DebugUtil.Assert(prevStack + 1 == this.currentStackCount);
#endif
                    --this.currentStackCount;
                    fallThrough = CreateNewBB(false);
                    StartNewBB(ldOne, ILCODE.BR, fallThrough, ILCODE.ILLEGAL);
                    GenIntConstant(1);
                    StartNewBB(fallThrough, ILCODE.next, null, ILCODE.ILLEGAL);
                    break;

                case EXPRKIND.CONSTANT:
                    // Make sure that at most the low bit is set:
                    DebugUtil.Assert(((conditionExpr as EXPRCONSTANT).ConstVal.GetInt() & ~1) == 0);
                    //DebugUtil.Assert(((int)sense & ~1) == 0);
                    int constant;
                    int senseVal = (sense ? 1 : 0);
                    constant = (((((conditionExpr as EXPRCONSTANT).ConstVal.GetInt() & 1) == senseVal) ? 1 : 0) & 1);
                    if (constresKind != 0)
                    {
                        constresKind = constant != 0 ? EXPR.CONSTRESKIND.True : EXPR.CONSTRESKIND.False;
                    }
                    GenIntConstant(constant);
                    break;

                case EXPRKIND.LT:
                case EXPRKIND.LE:
                case EXPRKIND.GT:
                case EXPRKIND.GE:
                case EXPRKIND.EQ:
                case EXPRKIND.NE:
                    GenExpr(conditionExpr.AsBIN.Operand1, true);
                    GenExpr(conditionExpr.AsBIN.Operand2, true);

                    PutILInstruction(CodeForCompare(conditionExpr.Kind, conditionExpr.AsBIN.Operand1.TypeSym, ref sense));

                    if (!sense)
                    {
                        goto LNegate;
                    }
                    break;

                case EXPRKIND.LOGNOT:
                    conditionExpr = conditionExpr.AsBIN.Operand1;
                    sense = !sense;
                    goto LRecurse;

                default:
                    GenExpr(conditionExpr, true);
                    if (constresKind != 0)
                    {
                        constresKind = conditionExpr.GetConstantResult();
                        if (!sense && constresKind != EXPR.CONSTRESKIND.NotConst)
                        {
                            constresKind = (constresKind == EXPR.CONSTRESKIND.True) ?
                                EXPR.CONSTRESKIND.False : EXPR.CONSTRESKIND.True;
                        }
                    }
                    if (!sense)
                    {
                        goto LNegate;
                    }
                    break;
            }
            return;

        LNegate:
            PutILInstruction(ILCODE.LDC_I4_0);
            PutILInstruction(ILCODE.CEQ);
        }

        //------------------------------------------------------------
        // ILGENREC.GenNewArray
        //
        /// <summary>
        /// generate code to create an array...
        /// </summary>
        /// <param name="binopExpr"></param>
        //------------------------------------------------------------
        private void GenNewArray(EXPRBINOP binopExpr)
        {
            ARRAYSYM arraySym = binopExpr.TypeSym as ARRAYSYM;
            int rank = arraySym.Rank;
            TYPESYM elemTypeSym = arraySym.ElementTypeSym;

            int oldStack = this.currentStackCount;

            GenExpr(binopExpr.Operand1, true);
            if (rank == 1)
            {
                PutILInstruction(ILCODE.NEWARR, elemTypeSym);
                return;
            }
            DebugUtil.Assert(this.currentStackCount > oldStack);
            GenArrayCall(arraySym, this.currentStackCount - oldStack, ARRAYMETHOD.CTOR);
        }

        //------------------------------------------------------------
        // ILGENREC.GenSwitch
        //
        /// <summary></summary>
        /// <param name="switchExpr"></param>
        //------------------------------------------------------------
        private void GenSwitch(EXPRSWITCH switchExpr)
        {
            DebugUtil.Assert(!switchExpr.Reachable() == (switchExpr.ArgumentExpr.Kind == EXPRKIND.CONSTANT));
            TYPESYM tempTypeSym;

            if (switchExpr.ArgumentExpr.TypeSym.IsPredefType(PREDEFTYPE.STRING))
            {
                GenStringSwitch(switchExpr);
                return;
            }

            SWITCHBUCKET lastBucket = null;
            int bucketCount = 0;

            int labelCount = switchExpr.LabelCount;
            EXPRSWITCHLABEL defaultLabelExpr = null;

            if ((switchExpr.Flags & EXPRFLAG.HASDEFAULT) != 0)
            {
                DebugUtil.Assert(labelCount > 0);
                defaultLabelExpr = switchExpr.LabelArray[--labelCount];
            }
            if (switchExpr.NullLabelExpr != null)
            {
                DebugUtil.Assert(labelCount > 0);
                DebugUtil.Assert(
                    switchExpr.ArgumentExpr.TypeSym.IsNUBSYM &&
                    switchExpr.NullLabelExpr == switchExpr.LabelArray[labelCount - 1]);

                --labelCount;
            }

            SourceExtent extentSwitchOnly = new SourceExtent();
            extentSwitchOnly.SetInvalid();
            BBLOCK defaultBlock = CreateNewBB(false);

            if (switchExpr.ArgumentExpr.Kind != EXPRKIND.CONSTANT)
            {
                //----------------------------------------
                // Create the SWITCHBUCKET instances which represent the dense range of keys,
                // and link them in list.
                //----------------------------------------
                //EXPRSWITCHLABEL ** pexprLim = switchExpr.LabelArray + labelCount;
                //for (EXPRSWITCHLABEL ** pexprCur = switchExpr.LabelArray; pexprCur < pexprLim; pexprCur++)
                for (int i = 0; i < labelCount; ++i)
                {
                    //DebugUtil.Assert(*pexprCur && *pexprCur != switchExpr.nullLabel && *pexprCur != defaultLabelExpr);
                    EXPRSWITCHLABEL currentLabelExpr = switchExpr.LabelArray[i];

                    DebugUtil.Assert(
                        currentLabelExpr != null &&
                        currentLabelExpr != switchExpr.NullLabelExpr &&
                        currentLabelExpr != defaultLabelExpr);

                    // First, see if we fit in the last bucket, or if we need to start a new one...
                    //unsigned __int64 key = (*pexprCur).key.asCONSTANT().getI64Value();
                    ulong key = (currentLabelExpr.KeyExpr as EXPRCONSTANT).ConstVal.GetULong();
                    if (FitsInBucket(lastBucket, key, 1))
                    {
                        lastBucket.LastMember = key;
                        ++lastBucket.MemberCount;
                        bucketCount -= MergeLastBucket(ref lastBucket);
                    }
                    else
                    {
                        // create a new bucket...
                        //PSWITCHBUCKET newBucket = STACK_ALLOC(SWITCHBUCKET, 1);
                        SWITCHBUCKET newBucket = new SWITCHBUCKET();
                        newBucket.FirstMember = key;
                        newBucket.LastMember = key;
                        //newBucket.labels = pexprCur;
                        newBucket.FirstIndex = i;
                        newBucket.MemberCount = 1;
                        newBucket.PrevBucket = lastBucket;
                        lastBucket = newBucket;
                        ++bucketCount;
                    }
                }   // for (int i = 0; i < labelCount; ++i)

                //----------------------------------------
                // Ok, now to copy all this into an array so that we can do a binary traversal on it:
                //----------------------------------------

                //PSWITCHBUCKET * bucketArray = STACK_ALLOC(PSWITCHBUCKET, bucketCount);
                SWITCHBUCKET[] bucketArray = new SWITCHBUCKET[bucketCount];

                for (int i = bucketCount - 1; i >= 0; i--)
                {
                    DebugUtil.Assert(lastBucket != null);
                    bucketArray[i] = lastBucket;
                    lastBucket = lastBucket.PrevBucket;
                }
                DebugUtil.Assert(lastBucket == null);

                if (bucketCount > 0 || switchExpr.NullLabelExpr != null)
                {
                    LOCSLOTINFO slot;

                    // Deal with null.
                    if (switchExpr.ArgumentExpr.TypeSym.IsNUBSYM)
                    {
                        tempTypeSym = switchExpr.ArgumentExpr.TypeSym.StripNubs();
                        AGGTYPESYM ats = (switchExpr.ArgumentExpr.TypeSym as NUBSYM).GetAggTypeSym();
                        LOCSLOTINFO nubSlot = null;

                        GenDupSafeMemAddr(switchExpr.ArgumentExpr, ref nubSlot);
                        PutILInstruction(ILCODE.DUP);
                        PutILInstruction(
                            ILCODE.CALL,
                            compiler.MainSymbolManager.NullableGetValOrDefMethodSym,
                            null,
                            null);
                        //EmitMethodToken(
                        //    compiler.MainSymbolManager.NubGetValOrDefMethSym,
                        //    ats,
                        //    null);

                        if (compileForEnc)
                        {
                            slot = DumpToDurable(tempTypeSym, false, false);
                        }
                        else
                        {
                            slot = STOREINTEMP(tempTypeSym, TEMP_KIND.SHORTLIVED);
                        }

                        PutILInstruction(
                            ILCODE.CALL,
                            compiler.MainSymbolManager.NullableHasValuePropertySym.GetMethodSym,
                            null,
                            null);
                        // PutILInstruction methos include the operations of EmitMethodToken
                        //EmitMethodToken(
                        //    compiler.MainSymbolManager.NullableHasValuePropertySym.GetMethodSym,
                        //    ats,
                        //    null);
                        if (nubSlot != null)
                        {
                            FreeTemporary(nubSlot);
                        }

                        BBLOCK nullBlock;
                        if (switchExpr.NullLabelExpr != null)
                        {
                            switchExpr.NullLabelExpr.BBlock = nullBlock = CreateNewBB(false);
                        }
                        else
                        {
                            nullBlock = defaultBlock;
                        }

                        StartNewBB(null, ILCODE.BRFALSE, nullBlock, ILCODE.ILLEGAL);
                        this.currentStackCount--;
                    }
                    else // if (switchExpr.ArgumentExpr.TypeSym.IsNUBSYM)
                    {
                        tempTypeSym = switchExpr.ArgumentExpr.TypeSym;

                        GenExpr(switchExpr.ArgumentExpr, true);
                        DebugUtil.Assert(switchExpr.NullLabelExpr == null);

                        if (compileForEnc)
                        {
                            slot = DumpToDurable(tempTypeSym, false, false);
                        }
                        else
                        {
                            slot = STOREINTEMP(tempTypeSym, TEMP_KIND.SHORTLIVED);
                        }
                    }

                    // Save the ending line/col so we don't include the case labels
                    // when we emit the buckets
                    if (this.currentDebugInfo != null)
                    {
                        extentSwitchOnly = this.currentDebugInfo.Extent;
                    }

                    if (bucketCount > 0)
                    {
                        EmitSwitchBuckets(switchExpr, bucketArray, 0, bucketCount - 1, slot, defaultBlock);
                    }
                    FreeTemporary(slot);
                }
                else // if (bucketCount > 0 || switchExpr.NullLabelExpr != null)
                {
                    GenSideEffects(switchExpr.ArgumentExpr);
                }

                if ((switchExpr.Flags & EXPRFLAG.HASDEFAULT) != 0)
                {
                    DebugUtil.Assert(defaultLabelExpr != null);
                    StartNewBB(null, ILCODE.BR, defaultLabelExpr.BBlock = defaultBlock, ILCODE.ILLEGAL);
                }
                else
                {
                    StartNewBB(null, ILCODE.BR, switchExpr.BreakLabelExpr.BBlock = defaultBlock, ILCODE.ILLEGAL);
                }
            } // if (switchExpr.ArgumentExpr.Kind != EXPRKIND.CONSTANT)

            if (this.currentDebugInfo != null)
            {
                if (extentSwitchOnly.IsValid)
                {
                    // Restore the extent info (overwritting whatever extra extents were set by emitSwitchBuckets)
                    this.currentDebugInfo.Extent = extentSwitchOnly;
                }
                CloseDebugInfo();
            }

            if (switchExpr.BodiesExpr != null)
            {
                GenStmtChain(switchExpr.BodiesExpr);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenHashtableStringSwitchInit
        //
        /// <summary></summary>
        /// <param name="switchExpr"></param>
        //------------------------------------------------------------
        private void GenHashtableStringSwitchInit(EXPRSWITCH switchExpr)
        {
            switchExpr.HashTableInfo = compiler.Emitter.GetGlobalFieldDef(
                this.methodSym,
                ++(this.globalFieldCount),
                compiler.MainSymbolManager.DictionaryAggTypeSym,
                0);

            BBLOCK contBlock = CreateNewBB(false);

            PutILInstruction(ILCODE.VOLATILE);
            PutILInstruction(ILCODE.LDSFLD, switchExpr.HashTableInfo);
            StartNewBB(null, ILCODE.BRTRUE, contBlock, ILCODE.ILLEGAL);
            this.currentStackCount -= 1;

            int labelCount = switchExpr.LabelArray.Count;
            if ((switchExpr.Flags & EXPRFLAG.HASDEFAULT) != 0)
            {
                labelCount--;
            }
            DebugUtil.Assert(labelCount >= 0);

            GenIntConstant(labelCount);
            PutILInstruction(
                ILCODE.NEWOBJ,
                compiler.MainSymbolManager.DictionaryCtorMethSym.ConstructorInfo,
                compiler.MainSymbolManager.DictionaryAggTypeSym,
                compiler.MainSymbolManager.DictionaryCtorMethSym.ParameterTypes);
            //EmitMethodToken(
            //    compiler.MainSymbolManager.DictionaryCtorMethSym,
            //    compiler.MainSymbolManager.DictionaryAggTypeSym);
            this.currentStackCount -= 1;

            //EXPRSWITCHLABEL ** start = switchExpr.labels;
            //EXPRSWITCHLABEL ** end = start + labelCount;

            int caseNumber = 0;

            //while (start != end)
            for (int i = 0; i < labelCount; ++i)
            {
                EXPRSWITCHLABEL labExpr = switchExpr.LabelArray[i];
                if (!(labExpr.KeyExpr as EXPRCONSTANT).IsNull())
                {
                    PutILInstruction(ILCODE.DUP); // dup the hashtable...
                    GenString((labExpr.KeyExpr as EXPRCONSTANT).ConstVal.GetString());
                    GenIntConstant(caseNumber++);
                    PutILInstruction(
                        ILCODE.CALL,
                        compiler.MainSymbolManager.DictionaryAddMethSym.MethodInfo,
                        compiler.MainSymbolManager.DictionaryAggTypeSym,
                        compiler.MainSymbolManager.DictionaryAddMethSym.ParameterTypes,
                        null,
                        null);
                    //EmitMethodToken(
                    //    compiler.MainSymbolManager.DictionaryAddMethSym,
                    //    compiler.MainSymbolManager.DictionaryAggTypeSym);
                    this.currentStackCount -= 3;
                }
                //start ++;
            }

            PutILInstruction(ILCODE.VOLATILE);
            PutILInstruction(ILCODE.STSFLD, switchExpr.HashTableInfo);

            StartNewBB(contBlock, ILCODE.next, null, ILCODE.ILLEGAL);
        }

        //------------------------------------------------------------
        // ILGENREC.GenStringSwitch
        //
        /// <summary></summary>
        /// <param name="switchExpr"></param>
        //------------------------------------------------------------
        private void GenStringSwitch(EXPRSWITCH switchExpr)
        {
            DebugUtil.Assert(switchExpr.ArgumentExpr.TypeSym.IsPredefType(PREDEFTYPE.STRING));
            int labelCount = switchExpr.LabelCount;
            if ((switchExpr.Flags & EXPRFLAG.HASDEFAULT) != 0)
            {
                labelCount--;
            }
            DebugUtil.Assert(labelCount >= 0);

            List<EXPRSWITCHLABEL> labelArray = switchExpr.LabelArray;
            BBLOCK defBlock = CreateNewBB(false);
            BBLOCK nullBlock = null;
            if (switchExpr.NullLabelExpr != null)
            {
                switchExpr.NullLabelExpr.BBlock = nullBlock = CreateNewBB(false);
            }
            else
            {
                nullBlock = defBlock;
            }

            if (switchExpr.ArgumentExpr.Kind != EXPRKIND.CONSTANT)
            {
                if (labelCount > 0)
                {
                    if ((switchExpr.Flags & EXPRFLAG.HASHTABLESWITCH) != 0)
                    {
                        DebugUtil.Assert(!compiler.FEncBuild());
                        SWITCHDEST switchDest = new SWITCHDEST();

                        GenExpr(switchExpr.ArgumentExpr, true);

                        LOCSLOTINFO slot = null;
                        if (this.compileForEnc)
                        {
                            slot = DumpToDurable(compiler.GetReqPredefType(PREDEFTYPE.STRING, true), false, true);
                        }
                        else
                        {
                            PutILInstruction(ILCODE.DUP);
                            slot = STOREINTEMP(compiler.GetReqPredefType(PREDEFTYPE.STRING, true), TEMP_KIND.SHORTLIVED);
                        }
                        LOCSLOTINFO intSlot = AllocTemporary(
                            compiler.GetReqPredefType(PREDEFTYPE.INT, true),
                            TEMP_KIND.SHORTLIVED);

                        StartNewBB(null, ILCODE.BRFALSE, nullBlock, ILCODE.ILLEGAL);
                        this.currentStackCount--;

                        GenHashtableStringSwitchInit(switchExpr);
                        PutILInstruction(ILCODE.VOLATILE);
                        PutILInstruction(ILCODE.LDSFLD, switchExpr.HashTableInfo);

                        DumpLocal(slot, false);
                        GenSlotAddress(intSlot, false);
                        PutILInstruction(
                            ILCODE.CALL,
                            compiler.MainSymbolManager.DictionaryTryGetValueMethSym.MethodInfo,
                            compiler.MainSymbolManager.DictionaryAggTypeSym,
                            compiler.MainSymbolManager.DictionaryTryGetValueMethSym.ParameterTypes,
                            null,
                            null);
                        this.currentStackCount -= 2;
                        StartNewBB(null, ILCODE.BRFALSE, defBlock, ILCODE.ILLEGAL);
                        this.currentStackCount--;
                        DumpLocal(intSlot, false);
                        FreeTemporary(slot);
                        FreeTemporary(intSlot);
                        this.currentStackCount--;
                        for (int i = 0; i < labelCount; ++i)
                        {
                            EXPRSWITCHLABEL labExpr = labelArray[i];
                            if (!labExpr.KeyExpr.IsNull())
                            {
                                SWITCHDESTGOTO sdg = new SWITCHDESTGOTO();
                                sdg.JumpIntoTry = false;
                                DebugUtil.Assert(labExpr.BBlock == null);
                                sdg.DestBBlock
                                    = labExpr.BBlock
                                    = CreateNewBB(false);
                                switchDest.BBlockList.Add(sdg);
                            }
                        }
                        StartNewSwitchBB(null, switchDest, ILCODE.ILLEGAL);
                    }
                    else // if ((switchExpr.Flags & EXPRFLAG.HASHTABLESWITCH) != 0) 
                    {
                        GenExpr(switchExpr.ArgumentExpr, true);
                        LOCSLOTINFO slot = null;
                        if (this.compileForEnc)
                        {
                            slot = DumpToDurable(compiler.GetReqPredefType(PREDEFTYPE.STRING, true), false, true);
                        }
                        else
                        {
                            PutILInstruction(ILCODE.DUP);
                            slot = STOREINTEMP(compiler.GetReqPredefType(PREDEFTYPE.STRING, true), TEMP_KIND.SHORTLIVED);
                        }
                        StartNewBB(null, ILCODE.BRFALSE, nullBlock, ILCODE.ILLEGAL);
                        this.currentStackCount--;

                        for (int i = 0; i < labelCount; ++i)
                        {
                            EXPRSWITCHLABEL labExpr = labelArray[i];
                            if (!(labExpr.KeyExpr as EXPRCONSTANT).IsNull())
                            {
                                DumpLocal(slot, false);
                                GenString((labExpr.KeyExpr as EXPRCONSTANT).ConstVal.GetString());
                                PutILInstruction(
                                    ILCODE.CALL,
                                    compiler.MainSymbolManager.StringEqualsMethSym.MethodInfo);
                                labExpr.BBlock = CreateNewBB(false);
                                StartNewBB(null, ILCODE.BRTRUE, labExpr.BBlock, ILCODE.ILLEGAL);
                                this.currentStackCount -= 2;
                            }
                        }
                        FreeTemporary(slot);
                    }
                }
                else
                {
                    GenSideEffects(switchExpr.ArgumentExpr);
                }

                if ((switchExpr.Flags & EXPRFLAG.HASDEFAULT) != 0)
                {
                    labelArray[labelArray.Count - 1].BBlock = defBlock;
                    StartNewBB(null, ILCODE.BR, defBlock, ILCODE.ILLEGAL);
                }
                else
                {
                    switchExpr.BreakLabelExpr.BBlock = defBlock;
                    StartNewBB(null, ILCODE.BR, defBlock, ILCODE.ILLEGAL);
                }
            }

            CloseDebugInfo();

            if (switchExpr.BodiesExpr != null)
            {
                GenStmtChain(switchExpr.BodiesExpr);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenTry
        //
        /// <summary></summary>
        /// <param name="tryExpr"></param>
        //------------------------------------------------------------
        private void GenTry(EXPRTRY tryExpr)
        {
            if ((tryExpr.Flags & EXPRFLAG.REMOVEFINALLY) != 0)
            {
                DebugUtil.Assert((tryExpr.Flags & EXPRFLAG.ISFINALLY) != 0);
                GenBlock(tryExpr.TryBlockExpr);
                GenBlock(tryExpr.HandlersExpr as EXPRBLOCK);
                return;
            }

            //--------------------------------------------------
            // start : is top of the try block
            //--------------------------------------------------
            BBLOCK tryBeginBBlock = StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
            if ((tryExpr.Flags & EXPRFLAG.ISFINALLY) != 0)
            {
                ++this.finallyNestingCount;
                this.currentBBlock.TryNestingCount = this.finallyNestingCount;  // inlineBBlock
            }
            tryBeginBBlock.StartsException = true;  // inlineBBlock
            GenBlock(tryExpr.TryBlockExpr);

            //--------------------------------------------------
            // end is after the end of the protected block
            //--------------------------------------------------
            BBLOCK tryEndBBlock;
            if ((tryExpr.Flags & EXPRFLAG.ISFINALLY) != 0)
            {
                BBLOCK afterFinallyBBlock = CreateNewBB(false);

                tryEndBBlock = StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
                // this will point to the ILCODE.LEAVE...

                //this.inlineBBlock.mayRemove = true; 	// this is a synthetic goto,
                // which may point beyond the end of code... 

                if (tryExpr.IsFinallyBlocked())
                {
                    this.currentBBlock.GotoBlocked = true; // this refers to the following leave    // inlineBBlock
                    ++this.blockedLeave;
                }

                CloseDebugInfo();  // close the info for the try block

                OpenDebugInfo(SpecialDebugPointEnum.HiddenCode, EXPRFLAG.NODEBUGINFO);
                BBLOCK handBeginBBlock = StartNewBB(null, ILCODE.LEAVE, afterFinallyBBlock, ILCODE.ILLEGAL);
                CloseDebugInfo();
                // and this will point to the finally block following the ILCODE.LEAVE...

                this.currentBBlock.StartsFinally = true;    // inlineBBlock
                GenBlock(tryExpr.HandlersExpr as EXPRBLOCK);

                HANDLERINFO handlerInfo = CreateHandler(
                    tryBeginBBlock,
                    tryEndBBlock,
                    handBeginBBlock,
                    (tryExpr.Flags & EXPRFLAG.ISFAULT) != 0 ? HANDLERINFO.faultTypeSym : null);
                // Use HANDLERINFO.faultTypeSym in place of (TYPESYM*)1.

                handlerInfo.HandlerEndBBlock = StartNewBB(null,ILCODE.next,null,ILCODE.ILLEGAL);
                // this points to the ILCODE.ENDFINALLY...

                OpenDebugInfo(SpecialDebugPointEnum.HiddenCode, EXPRFLAG.NODEBUGINFO);
                this.currentBBlock.EndsException = true;  // inlineBBlock
                //PutILInstruction(ILCODE.ENDFINALLY);
                // ENDFINARLLY instruction is inserted by ILGenerator.EndExceptionBlock().
                CloseDebugInfo();
                StartNewBB(afterFinallyBBlock, ILCODE.ENDFINALLY, null, ILCODE.ILLEGAL);

                --this.finallyNestingCount;
            }
            else
            {
                // fallthrough is after the handler
                BBLOCK fallThroughBBlock = CreateNewBB(false);

                tryEndBBlock = StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
                // this points to the ILCODE.LEAVE

                //this.inlineBBlock.mayRemove = true; 

                // close the info for the try block
                CloseDebugInfo();

                OpenDebugInfo(SpecialDebugPointEnum.HiddenCode, EXPRFLAG.NODEBUGINFO);
                StartNewBB(null, ILCODE.LEAVE, fallThroughBBlock, ILCODE.ILLEGAL);
                CloseDebugInfo();
                // Current is now after the ILCODE.LEAVE...

                EXPRSTMT stmtExpr = tryExpr.HandlersExpr;
                while (stmtExpr != null)
                {
                    EXPRSTMT hand = stmtExpr;
                    stmtExpr = stmtExpr.NextStatement;

                    EXPRHANDLER catchHandler = hand as EXPRHANDLER;
                    BBLOCK handBeginBBlock = this.currentBBlock;
                    handBeginBBlock.StartsCatch = true; // inlineBBlock
                    handBeginBBlock.catchTypeSym = catchHandler.TypeSym;

                    ++this.currentStackCount;
                    MarkStackMax();

                    // emit debug info for the catch.
                    OpenDebugInfo(catchHandler.TreeNode, 0);

                    bool usedParam =
                        catchHandler.ParameterSym != null && catchHandler.ParameterSym.LocSlotInfo.TypeSym != null;

                    DebugUtil.Assert(
                        catchHandler.ParameterSym == null ||
                        catchHandler.ParameterSym.MovedToFieldSym == null ||
                        usedParam);

                    if (usedParam)
                    {
                        MaybeEmitDebugLocalUsage(catchHandler.TreeNode, catchHandler.ParameterSym);
                        if (catchHandler.ParameterSym.TypeSym.IsTYVARSYM)
                        {
                            PutILInstruction(ILCODE.UNBOX_ANY, catchHandler.ParameterSym.TypeSym);
                        }
                        DumpLocal(catchHandler.ParameterSym.LocSlotInfo, true);
                    }
                    else
                    {
                        PutILInstruction(ILCODE.POP);
                    }

                    CloseDebugInfo();  // finish debug info for the catch.

                    GenBlock(catchHandler.HandlerBlock);

                    HANDLERINFO handlerInfo = CreateHandler(
                        tryBeginBBlock,
                        tryEndBBlock,
                        handBeginBBlock,
                        catchHandler.TypeSym);
                    handlerInfo.HandlerEndBBlock = StartNewBB(null, ILCODE.next, null, ILCODE.ILLEGAL);
                    // this will now point to the ILCODE.LEAVE...

                    //this.inlineBBlock.mayRemove = true;  

                    CloseDebugInfo();  // finish debug info for the catch.

                    OpenDebugInfo(SpecialDebugPointEnum.HiddenCode, EXPRFLAG.NODEBUGINFO);
                    StartNewBB(null, ILCODE.LEAVE, fallThroughBBlock, ILCODE.ILLEGAL);
                    CloseDebugInfo();
                }

                this.currentBBlock.EndsException = true;
                StartNewBB(fallThroughBBlock, ILCODE.next, null, ILCODE.ILLEGAL);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenThrow
        //
        /// <summary></summary>
        /// <param name="tree"></param>
        //------------------------------------------------------------
        private void GenThrow(EXPRTHROW tree)
        {
            if (tree.ObjectExpr != null)
            {
                GenExpr(tree.ObjectExpr, true);
                PutILInstruction(ILCODE.THROW);
            }
            else
            {
                PutILInstruction(ILCODE.RETHROW);
            }
            StartNewBB(null, ILCODE.stop, null, ILCODE.ILLEGAL);
        }

        //------------------------------------------------------------
        // ILGENREC.CreateHandler
        //
        /// <summary></summary>
        /// <param name="tryBegin"></param>
        /// <param name="tryEnd"></param>
        /// <param name="handBegin"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private HANDLERINFO CreateHandler(
            BBLOCK tryBegin,
            BBLOCK tryEnd,
            BBLOCK handBegin,
            TYPESYM type)
        {
            ++this.ehCount;
            //HANDLERINFO * handler = (HANDLERINFO*)allocator.Alloc(sizeof(HANDLERINFO));
            HANDLERINFO handler = new HANDLERINFO();
            handler.TryBeginBBlock = tryBegin;
            handler.TryEndBBlock = tryEnd;
            handler.HandlerBeginBBlock = handBegin;
            handler.TypeSym = type;
            handler.Next = null;
            handler.HandlerShouldIncludeNOP = false;

            if (this.lastHandlerInfo != null)
            {
                this.lastHandlerInfo.Next = handler;
                this.lastHandlerInfo = handler;
            }
            else
            {
                this.lastHandlerInfo = this.handlerInfos = handler;
            }

            return handler;
        }

        //------------------------------------------------------------
        // ILGENREC.HandleReturn
        //
        /// <summary>
        /// <para>(sscli) addDebugInfo has the default value false.</para>
        /// </summary>
        /// <param name="addDebugInfo"></param>
        //------------------------------------------------------------
        private void HandleReturn(bool addDebugInfo)    // = false
        {
            if (this.returnLocationBBlock != null &&
                !this.returnHandled &&
                this.temporaryReturnSlotInfo == null)
            {
                StartNewBB(
                    this.returnLocationBBlock,
                    ILCODE.next,
                    null,
                    ILCODE.ILLEGAL);
                this.returnHandled = true;
            }

            if (!this.methodSym.ReturnTypeSym.IsVoidType)   // != compiler.MainSymbolManager.VoidSym)
            {
                this.currentStackCount--;
            }

            if (addDebugInfo)
            {
                if (this.closeIndexUsed)
                {
                    OpenDebugInfo(SpecialDebugPointEnum.HiddenCode, EXPRFLAG.NODEBUGINFO);
                }
                else
                {
                    OpenDebugInfo(SpecialDebugPointEnum.CloseCurly, 0);

                }
            }

            PutILInstruction(ILCODE.RET);
        }

        //------------------------------------------------------------
        // ILGENREC.GenMultiOp
        //
        /// <summary>
        /// <para>(sscli) valUsed has the default value true.</para>
        /// </summary>
        /// <param name="multiExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenMultiOp(
            EXPRMULTI multiExpr,
            bool valUsed)   // = true
        {
            MultiOpInfo info = new MultiOpInfo();

            multiExpr.MultiOpInfo = info;

            info.Addr.Init();
            info.NeedOld = valUsed && (multiExpr.Flags & EXPRFLAG.ISPOSTOP) != 0;
            info.GetCount = 0;
            info.ValStackCount = this.currentStackCount;
            info.Slot = null;

            GenExpr(multiExpr.OperandExpr, true);

            // GenMultiGet should have been called exactly once.
            DebugUtil.Assert(info.GetCount == 1);

            // There should at least be a value on the stack (for assignment) and
            // possibly an "address"
            DebugUtil.Assert(this.currentStackCount > info.ValStackCount);

            if (valUsed && !info.NeedOld)
            {
                // Need the new value.
                DebugUtil.Assert(info.Slot == null);

                PutILInstruction(ILCODE.DUP);
                if (this.currentStackCount - info.ValStackCount > 2)
                {
                    // There's an "address" on the stack so store the new value in a temp.
                    info.Slot = AllocTemporary(multiExpr.LeftExpr.TypeSym, TEMP_KIND.SHORTLIVED);
                    DumpLocal(info.Slot, true);
                }
            }

            GenStore(multiExpr.LeftExpr,ref info.Addr);

            if (info.Slot != null)
            {
                DumpLocal(info.Slot, false);
                FreeTemporary(info.Slot);
            }

            multiExpr.MultiOpInfo = null;
        }

        //------------------------------------------------------------
        // ILGENREC.GenMultiGet
        //
        /// <summary>
        /// <para>(sscli) valUsed has the default value true.</para>
        /// </summary>
        /// <param name="multiExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenMultiGet(
            EXPRMULTI multiExpr,
            bool valUsed)   // = true
        {
            DebugUtil.Assert(multiExpr.MultiOpInfo != null);
            MultiOpInfo info = multiExpr.MultiOpInfo;
            DebugUtil.Assert(info.Slot == null);
            DebugUtil.Assert(info.GetCount == 0);
            ++info.GetCount;

            // Stack level shouldn't have changed since GenMultiOp called genExpr.
            DebugUtil.Assert(info.ValStackCount == this.currentStackCount);
            if (!info.NeedOld && !valUsed)
            {
                GenAccess(multiExpr.LeftExpr, AccessTask.Enum.Addr | AccessTask.Enum.Load,ref info.Addr);
                PutILInstruction(ILCODE.POP);
                return;
            }

            GenAccess(multiExpr.LeftExpr, AccessTask.Enum.Addr | AccessTask.Enum.Load, ref info.Addr);

            // There should be at least a value on the stack - and possibly an "address"
            DebugUtil.Assert(this.currentStackCount > info.ValStackCount);
            if (info.NeedOld)
            {
                int cval = 1;
                if (valUsed)
                {
                    PutILInstruction(ILCODE.DUP);
                    ++cval;
                }
                if (this.currentStackCount - info.ValStackCount > cval)
                {
                    // There's an "address" on the stack so store the old value in a temp.
                    info.Slot = AllocTemporary(multiExpr.LeftExpr.TypeSym, TEMP_KIND.SHORTLIVED);
                    DumpLocal(info.Slot, true);
                }
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenQMark
        //------------------------------------------------------------
        private void GenQMark(EXPRBINOP binopExpr, bool valUsed)
        {
            if (!valUsed)
            {
                GenSideEffects(binopExpr);
                return;
            }

            if (binopExpr.TypeSym.IsInterfaceType())
            {
                EXPR p1 = binopExpr.Operand2.AsBIN.Operand1;
                EXPR p2 = binopExpr.Operand2.AsBIN.Operand2;
                DebugUtil.Assert(p1.TypeSym == binopExpr.TypeSym && p2.TypeSym == binopExpr.TypeSym);

                if (p1.Kind == EXPRKIND.CAST &&
                    p2.Kind == EXPRKIND.CAST &&
                    (p1.Flags & EXPRFLAG.CAST_ALL) == 0 &&
                    (p2.Flags & EXPRFLAG.CAST_ALL) == 0)
                {
                    // We only need to static cast one of the two.
                    p2.Flags |= EXPRFLAG.STATIC_CAST;
                }
            }

            BBLOCK trueBranch = GenCondBranch(binopExpr.Operand1, CreateNewBB(false), true);
            binopExpr = binopExpr.Operand2 as EXPRBINOP;

            BBLOCK fallThrough = CreateNewBB(false);

            GenExpr(binopExpr.Operand2, true);
            StartNewBB(trueBranch, ILCODE.BR, fallThrough, ILCODE.ILLEGAL);
            GenExpr(binopExpr.Operand1, true);
            --this.currentStackCount;
            StartNewBB(fallThrough, ILCODE.next, null, ILCODE.ILLEGAL);
        }

        //------------------------------------------------------------
        // ILGENREC.GenArrayInit
        //
        /// <summary></summary>
        /// <param name="arrInitExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenArrayInit(EXPRARRINIT arrInitExpr, bool valUsed)
        {
            ARRAYSYM arraySym = arrInitExpr.TypeSym as ARRAYSYM;
            int rank = arraySym.Rank;
            TYPESYM elemTypeSym = arraySym.ElementTypeSym;

            LOCSLOTINFO wrappedValue = null;

            if ((arrInitExpr.Flags & EXPRFLAG.PUSH_OP_FIRST) != 0)
            {
                // This only occurs for string/object concatenation.
                DebugUtil.Assert(rank == 1 && arrInitExpr.ArgumentsExpr != null);
                DebugUtil.Assert(
                    elemTypeSym.IsPredefType(PREDEFTYPE.STRING) ||
                    elemTypeSym.IsPredefType(PREDEFTYPE.OBJECT));

                EXPR firstExpr = (arrInitExpr.ArgumentsExpr.Kind == EXPRKIND.LIST ?
                    arrInitExpr.ArgumentsExpr.AsBIN.Operand1 : arrInitExpr.ArgumentsExpr);
                GenExpr(firstExpr, true);
                wrappedValue = STOREINTEMP(elemTypeSym, TEMP_KIND.SHORTLIVED);
            }

            for (int i = 0; i < rank; ++i)
            {
                GenIntConstant(arrInitExpr.DimSizes[i]);
            }
            if (rank == 1)
            {
                PutILInstruction(ILCODE.NEWARR, elemTypeSym);
            }
            else
            {
                GenArrayCall(arraySym, rank, ARRAYMETHOD.CTOR);
            }

            if (arrInitExpr.ArgumentsExpr == null)
            {
                // If there are no args, our code gen should be just like
                // EXPRKIND.NEWARRAY.
                return;
            }

            if ((arrInitExpr.Flags & (EXPRFLAG.ARRAYCONST | EXPRFLAG.ARRAYALLCONST)) != 0)
            {
                DebugUtil.Assert(!compiler.FEncBuild());

                GenArrayInitConstant(arrInitExpr, elemTypeSym, valUsed);
                if ((arrInitExpr.Flags & EXPRFLAG.ARRAYALLCONST) != 0)
                {
                    return;
                }
            }

            LOCSLOTINFO slot = AllocTemporary(arrInitExpr.TypeSym, TEMP_KIND.SHORTLIVED);
            DumpLocal(slot, true);

            if (arrInitExpr.ArgumentsExpr != null)
            {
                ILCODE il = ILarrayStore[(int)elemTypeSym.FundamentalType()];

                //int *rows;
                //rows = STACK_ALLOC_ZERO(int, rank);
                int[] rows = new int[rank];

                bool isStruct = (elemTypeSym.FundamentalType() == FUNDTYPE.STRUCT);

                EXPR arg = arrInitExpr.ArgumentsExpr;
                while (arg != null)
                {
                    EXPR elem;
                    if (arg.Kind == EXPRKIND.LIST)
                    {
                        elem = arg.AsBIN.Operand1;
                        arg = arg.AsBIN.Operand2;
                    }
                    else
                    {
                        elem = arg;
                        arg = null;
                    }

                    EXPR constExpr = elem.GetConst();
                    if (wrappedValue != null ||
                        constExpr == null ||
                        !constExpr.IsZero(true) && (arrInitExpr.Flags & EXPRFLAG.ARRAYCONST) == 0)
                    {
                        DumpLocal(slot, false);
                        for (int i = 0; i < rank; i++)
                        {
                            GenIntConstant(rows[i]);
                        }

                        if (isStruct)
                        {
                            if (rank == 1)
                            {
                                PutILInstruction(ILCODE.LDELEMA, elemTypeSym);
                            }
                            else
                            {
                                GenArrayCall(arraySym, rank, ARRAYMETHOD.LOADADDR);
                            }
                        }

                        if (wrappedValue != null)
                        {
                            DumpLocal(wrappedValue, false);
                            FREETEMP(ref wrappedValue);
                        }
                        else
                        {
                            GenExpr(elem, true);
                        }

                        if (isStruct)
                        {
                            PutILInstruction(ILCODE.STOBJ, elemTypeSym);
                        }
                        else if (rank == 1 && il != ILCODE.ILLEGAL)
                        {
                            //PutOpcode(il);
                            if (il == ILCODE.STELEM)
                            {
                                PutILInstruction(il, elemTypeSym); ;
                            }
                            else
                            {
                                PutILInstruction(il);
                            }
                        }
                        else
                        {
                            GenArrayCall(arraySym, rank, ARRAYMETHOD.STORE);
                        }
                    }
                    else
                    {
                        GenSideEffects(elem);
                    }

                    int row = rank - 1;
                    while (true)
                    {
                        rows[row]++;
                        if (rows[row] == arrInitExpr.DimSizes[row])
                        {
                            rows[row] = 0;
                            if (row == 0)
                            {
                                DebugUtil.Assert(arg == null);
                                goto DONE;
                            }
                            row--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        DONE:

            if (valUsed)
            {
                DumpLocal(slot, false);
            }
            FreeTemporary(slot);
        }

        //------------------------------------------------------------
        // ILGENREC.GenArrayInitConstant
        //
        /// <summary></summary>
        /// <param name="arrInitExpr"></param>
        /// <param name="elemType"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenArrayInitConstant(EXPRARRINIT arrInitExpr, TYPESYM elemType, bool valUsed)
        {
            int rank = (arrInitExpr.TypeSym as ARRAYSYM).Rank;

            int size, initSize;
            initSize = size = BSYMMGR.GetAttrArgSize(elemType.GetPredefType());
            DebugUtil.Assert(initSize > 0);
            for (int i = 0; i < rank; ++i)
            {
                size = size * arrInitExpr.DimSizes[i];
            }

            //BYTE * buffer;
            byte[] buffer = null;
            //mdToken token = compiler().emitter.GetGlobalFieldDef(method, ++globalFieldCount, (unsigned) size, &buffer);

            switch (initSize)
            {
                case 1:
                    //buffer = WriteArrayValues1(arrInitExpr.ArgumentsExpr);
                    break;

                case 2:
                    //buffer = WriteArrayValues2(arrInitExpr.ArgumentsExpr);
                    break;

                case 4:
                    if ((elemType as AGGTYPESYM).FundamentalType() == FUNDTYPE.R4)
                    {
                        //buffer = WriteArrayValuesF(arrInitExpr.ArgumentsExpr);
                    }
                    else
                    {
                        buffer = WriteArrayValues4(arrInitExpr.ArgumentsExpr);
                    }
                    break;

                case 8:
                    if ((elemType as AGGTYPESYM).FundamentalType() == FUNDTYPE.R8)
                    {
                        //buffer = WriteArrayValuesD(arrInitExpr.ArgumentsExpr);
                    }
                    else
                    {
                        //buffer = WriteArrayValues8(arrInitExpr.ArgumentsExpr);
                    }
                    break;

                default:
                    DebugUtil.Assert(false);
                    break;
            }

            FieldBuilder fieldBuilder = compiler.Emitter.GetGlobalFieldDef(
               this.methodSym, ++this.globalFieldCount, buffer);
            if (fieldBuilder == null)
            {
                return;
            }
            //fieldBuilder.SetConstant(buffer);

#if false
            if (valUsed || (arrInitExpr.Flags & EXPRFLAG.ARRAYALLCONST) == 0)
            {
                PutILInstruction(ILCODE.DUP);
            }

            PutILInstruction(ILCODE.LDTOKEN, fieldBuilder);
            PutILInstruction(
                ILCODE.CALL,
                compiler.MainSymbolManager.InitArrayMethSym,
                null,
                null);
            this.currentStackCount -= 2;
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.WriteArrayValues1
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private static Object WriteArrayValues1(EXPR treeExpr)
        {
            throw new NotImplementedException("ILGENREC.WriteArrayValues1");
        }
        //private void WriteArrayValues1(EXPR tree)
        //{
        //    EXPR  expr = tree;
        //    while (expr!=null)
        //    {
        //		EXPR  elem;
        //		if (expr.Kind == EXPRKIND.LIST)
        //		{
        //			elem = expr.AsBIN.Operand1;
        //			expr = expr.AsBIN.Operand2;
        //		}
        //		else
        //		{
        //			elem = expr;
        //			expr = null;
        //		}
        //
        //        EXPRCONSTANT  constExpr = elem.GetConst().AsCONSTANT;
        //        if (constExpr!=null)
        //        {
        //            *buffer = (BYTE) (constExpr.getVal().uiVal & 0xff);
        //        }
        //        else
        //        {
        //            *buffer = 0;
        //        }
        //        buffer += 1;
        //    }
        //}

        //------------------------------------------------------------
        // ILGENREC.WriteArrayValues2
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private static Object WriteArrayValues2(EXPR treeExpr)
        {
            throw new NotImplementedException("ILGENREC.WriteArrayValues2");
        }

        //------------------------------------------------------------
        // ILGENREC.WriteArrayValues4
        //
        /// <summary></summary>
        /// <param name="treeExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private byte[] WriteArrayValues4(EXPR treeExpr)
        {
            int size = ExprUtil.CountArguments(treeExpr);
            if (size == 0)
            {
                return new byte[0];
            }
            byte[] buffer = new byte[size * 4];
            byte[] bytes = null;
            byte[] bytes0 = { 0, 0, 0, 0 };

            EXPR expr = treeExpr;
            int index = 0;
            while (expr != null)
            {
                EXPR elem;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    elem = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    elem = expr;
                    expr = null;
                }

                EXPRCONSTANT constExpr = elem.GetConst() as EXPRCONSTANT;
                if (constExpr != null)
                {
                    //*((DWORD*)buffer) = VAL32((DWORD) (constExpr.getVal().uiVal));
                    bytes = BitConverter.GetBytes(constExpr.ConstVal.GetInt());
                }
                else
                {
                    //*((DWORD*)buffer) = (DWORD) 0;
                    bytes = bytes0;
                }
                DebugUtil.Assert(bytes.Length == 4);

                buffer[index++] = bytes[0];
                buffer[index++] = bytes[1];
                buffer[index++] = bytes[2];
                buffer[index++] = bytes[3];
            }
            return buffer;
        }

        //------------------------------------------------------------
        // ILGENREC.WriteArrayValues8
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private static Object WriteArrayValues8(EXPR treeExpr)
        {
            throw new NotImplementedException("ILGENREC.WriteArrayValues8");
        }

        //------------------------------------------------------------
        // ILGENREC.WriteArrayValuesD
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private static Object WriteArrayValuesD(EXPR treeExpr)
        {
            throw new NotImplementedException("ILGENREC.WriteArrayValuesD");
        }

        //------------------------------------------------------------
        // ILGENREC.WriteArrayValuesF
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private static Object WriteArrayValuesF(EXPR treeExpr)
        {
            throw new NotImplementedException("ILGENREC.WriteArrayValuesF");
        }

        //------------------------------------------------------------
        // ILGENREC.GenCast
        //
        /// <summary></summary>
        /// <param name="castExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenCast(EXPRCAST castExpr, bool valUsed)
        {
            EXPRFLAG key = (castExpr.Flags & EXPRFLAG.CAST_ALL);

            // The only valid combination with more than one bit set is (EXPRFLAG.FORCE_BOX | EXPRFLAG.REFCHECK).
            DebugUtil.Assert((key & (key - 1)) == 0 || key == (EXPRFLAG.FORCE_BOX | EXPRFLAG.REFCHECK));

            // Short-cut for unused expressions - simply generate the side effects
            // of the expression instead.
            if (!valUsed &&
                (castExpr.Flags & (
                    EXPRFLAG.CHECKOVERFLOW |
                    EXPRFLAG.UNBOX |
                    EXPRFLAG.REFCHECK |
                    EXPRFLAG.FORCE_UNBOX)) == 0)
            {
                GenSideEffects(castExpr.Operand);
                return;
            }

            ILCODE il;

            TYPESYM fromTypeSym = castExpr.Operand.TypeSym.UnderlyingType();
            TYPESYM toTypeSym = castExpr.TypeSym.UnderlyingType();

            // FORCE_BOX may be used with a REFCHECK (for type variable to interface cast).
            bool refCheck = false;

            if (key == EXPRFLAG.BOX || (key & EXPRFLAG.FORCE_BOX) != 0)
            {
                GenExpr(castExpr.Operand, true);
                PutILInstruction(ILCODE.BOX, castExpr.Operand.TypeSym);
                key &= ~(EXPRFLAG.BOX | EXPRFLAG.FORCE_BOX);
                DebugUtil.Assert(key == 0 || key == EXPRFLAG.REFCHECK);
                if (key == EXPRFLAG.REFCHECK)
                {
                    refCheck = true;
                    goto LRefCheck;
                }
            }
            else if (key == EXPRFLAG.INDEXEXPR)
            {
                GenExpr(castExpr.Operand, true);
                if (toTypeSym.FundamentalType() == FUNDTYPE.U4)
                {
                    PutILInstruction(ILCODE.CONV_U);
                }
                else if (toTypeSym.FundamentalType() == FUNDTYPE.I8)
                {
                    PutILInstruction(ILCODE.CONV_OVF_I);
                }
                else
                {
                    DebugUtil.Assert(toTypeSym.FundamentalType() == FUNDTYPE.U8);
                    PutILInstruction(ILCODE.CONV_OVF_I_UN);
                }
            }
            else if (key == EXPRFLAG.FORCE_UNBOX)
            {
                GenExpr(castExpr.Operand, true);
                PutILInstruction(ILCODE.UNBOX_ANY, castExpr.TypeSym);
            }
            else if (key == EXPRFLAG.STATIC_CAST)
            {
                LOCSLOTINFO slot = AllocTemporary(toTypeSym, TEMP_KIND.SHORTLIVED);
                GenExpr(castExpr.Operand, true);
                DumpLocal(slot, true);
                DumpLocal(slot, false);
                FreeTemporary(slot);
            }
            else
            {
                // This is null being cast to a pointer, change this into a zero instead...
                if (toTypeSym.IsPTRSYM &&
                    !fromTypeSym.IsPTRSYM &&
                    castExpr.Operand.TypeSym.FundamentalType() == FUNDTYPE.REF &&
                    castExpr.Operand.GetConst() != null)
                {
                    GenSideEffects(castExpr.Operand);
                    GenIntConstant(0);
                    fromTypeSym = compiler.GetReqPredefType(PREDEFTYPE.UINT, false);
                }
                else
                {
                    GenExpr(castExpr.Operand, true);
                }

                switch (key)
                {
                    case EXPRFLAG.UNBOX:
                        PutILInstruction(ILCODE.UNBOX_ANY, castExpr.TypeSym);
                        break;

                    case EXPRFLAG.REFCHECK:
                        //LRefCheck:
                        //PutOpcode(ILCODE.CASTCLASS);
                        //EmitTypeToken(toTypeSym);
                        //break;
                        refCheck = true;
                        goto LRefCheck;

                    default:
                        PREDEFTYPE toPredef = COMPILER.GetPredefIndex(toTypeSym);
                        PREDEFTYPE fromPredef = COMPILER.GetPredefIndex(fromTypeSym);
                        if (fromPredef == PREDEFTYPE.UNDEFINED || toPredef == PREDEFTYPE.UNDEFINED)
                        {
                            break;
                        }
                        if ((castExpr.Flags & EXPRFLAG.CHECKOVERFLOW) != 0)
                        {
                            il = simpleTypeConversionsOvf[(int)fromPredef, (int)toPredef];
                        }
                        else
                        {
                            il = simpleTypeConversions[(int)fromPredef, (int)toPredef];
                        }

                        DebugUtil.Assert(il != ILCODE.ILLEGAL);
                        if (il != ILCODE.next)
                        {
                            PutILInstruction(il);
                            if (il == ILCODE.CONV_R_UN)
                            {
                                il = simpleTypeConversionsEx[(int)fromPredef, (int)toPredef];
                                DebugUtil.Assert(il != ILCODE.ILLEGAL && il != ILCODE.next);
                                PutILInstruction(il);
                            }
                        }
                        break;
                }
            }

        LRefCheck:
            if (refCheck)
            {
                PutILInstruction(ILCODE.CASTCLASS, toTypeSym);
            }
            
            if (!valUsed)
            {
                PutILInstruction(ILCODE.POP);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenObjectPtr
        //
        /// <summary></summary>
        /// <param name="expr"></param>
        /// <param name="memberSym"></param>
        /// <param name="slotInfo"></param>
        //------------------------------------------------------------
        private void GenObjectPtr(EXPR objectExpr, SYM memberSym, ref LOCSLOTINFO slotInfo)
        {
#if DEBUG
            if (!(objectExpr != null))
            {
                ;
            }
#endif
            DebugUtil.Assert(objectExpr != null);
            DebugUtil.Assert(!objectExpr.TypeSym.IsTYVARSYM);
            //DebugUtil.Assert(!slotInfo || !*slotInfo);

            if (!objectExpr.TypeSym.IsStructOrEnum())
            {
                GenExpr(objectExpr, true);
            }
            else if (NeedsBoxing(memberSym.ParentSym, objectExpr.TypeSym))
            {
                GenExpr(objectExpr, true);
                PutILInstruction(ILCODE.BOX, objectExpr.TypeSym);
            }
            else
            {
                GenMemoryAddress(objectExpr, ref slotInfo, false, false);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenIs
        //
        /// <summary>
        /// generate "is" or "as", depending on isAs.
        /// </summary>
        /// <param name="binopExpr"></param>
        /// <param name="valUsed"></param>
        /// <param name="isAs"></param>
        //------------------------------------------------------------
        private void GenIs(EXPRBINOP binopExpr, bool valUsed, bool isAs)
        {
            if (!valUsed)
            {
                GenSideEffects(binopExpr);
                return;
            }

            EXPR e1 = binopExpr.Operand1;
            EXPR e2 = binopExpr.Operand2;
            DebugUtil.Assert(e2.Kind == EXPRKIND.TYPEOF);

            GenExpr(e1, true);
            PutILInstruction(ILCODE.ISINST, (e2 as EXPRTYPEOF).SourceTypeSym);
            if (!isAs)
            {
                PutILInstruction(ILCODE.LDNULL);
                PutILInstruction(ILCODE.CGT_UN);
            }
            else if (binopExpr.TypeSym.IsNUBSYM)
            {
                PutILInstruction(ILCODE.UNBOX_ANY, binopExpr.TypeSym);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenZeroInit
        //
        /// <summary></summary>
        /// <param name="zeroInitExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenZeroInit(EXPRZEROINIT zeroInitExpr, bool valUsed)
        {
            EXPR operandExpr = zeroInitExpr.Operand;

            if (operandExpr != null && IsExprOptimizedAway(operandExpr))
            {
                operandExpr = null;
            }

            if (operandExpr == null && !valUsed)
            {
                return;
            }

            TYPESYM typeSym = zeroInitExpr.TypeSym;

            switch (typeSym.FundamentalType())
            {
                case FUNDTYPE.VAR:
                    // Type variables always follow FUNDTYPE.STRUCT - even with a class bound.
                    // Fall through....
                    goto case FUNDTYPE.STRUCT;

                case FUNDTYPE.STRUCT:
                    if (valUsed)
                    {
                        AddrInfo addr = new AddrInfo();
                        int cvalStack = 0;

                        if (operandExpr != null)
                        {
                            cvalStack = GenAddr(operandExpr, ref addr);
                            DebugUtil.Assert(cvalStack >= 0);
                        }

                        LOCSLOTINFO slot = AllocTemporary(typeSym, TEMP_KIND.SHORTLIVED);
                        GenSlotAddress(slot, false);
                        PutILInstruction(ILCODE.INITOBJ, typeSym);
                        DumpLocal(slot, false);

                        if (operandExpr != null)
                        {
                            if (cvalStack == 0)
                            {
                                PutILInstruction(ILCODE.DUP);
                            }
                            GenStore(operandExpr, ref addr);
                            if (cvalStack > 0)
                            {
                                DumpLocal(slot, false);
                            }
                        }
                        FreeTemporary(slot);
                    }
                    else
                    {
                        DebugUtil.Assert(operandExpr != null);

                        LOCSLOTINFO slot = null;
                        GenMemoryAddress(operandExpr, ref slot, false, false);

                        DebugUtil.Assert(slot == null);
                        PutILInstruction(ILCODE.INITOBJ, typeSym);
                    }
                    break;

                default:
                    if (operandExpr != null)
                    {
                        AddrInfo addr = new AddrInfo();

                        int cvalStack = GenAddr(operandExpr, ref addr);
                        DebugUtil.Assert(cvalStack >= 0);

                        GenZero(zeroInitExpr.TreeNode, typeSym);
                        if (valUsed && cvalStack == 0)
                        {
                            PutILInstruction(ILCODE.DUP);
                        }

                        GenStore(operandExpr, ref addr);

                        if (valUsed && cvalStack > 0)
                        {
                            GenZero(zeroInitExpr.TreeNode, typeSym);
                        }
                    }
                    else
                    {
                        DebugUtil.Assert(valUsed);
                        GenZero(zeroInitExpr.TreeNode, typeSym);
                    }
                    break;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenArrayCall
        //
        /// <summary></summary>
        /// <param name="array"></param>
        /// <param name="args"></param>
        /// <param name="meth"></param>
        //------------------------------------------------------------
        private void GenArrayCall(ARRAYSYM arraySym, int argCount, ARRAYMETHOD methodId)
        {
#if DEBUG
            this.currentBBlock.Sym = arraySym;
#endif
            this.currentStackCount -= argCount; // (1 for obj pointer)

            ILCODE il = ILCODE.CALL;

            if (methodId == ARRAYMETHOD.CTOR)
            {
                MethodInfo mi = compiler.Emitter.GetArrayConstructMethod(
                    arraySym,
                    this.methodSym.ClassSym,
                    this.methodSym);

                DebugUtil.Assert(mi != null);
                PutILInstruction(ILCODE.NEWOBJ, mi);
            }
            else // if (methodId == ARRAYMETHOD.CTOR)
            {
                switch (methodId)
                {
                    case ARRAYMETHOD.LOAD:
                    case ARRAYMETHOD.LOADADDR:
                    case ARRAYMETHOD.GETAT:
                        break;

                    case ARRAYMETHOD.STORE:
                        this.currentStackCount -= 2;
                        break;

                    default:
                        DebugUtil.Assert(false);
                        return;
                }
                MarkStackMax();

                MethodInfo mi = compiler.Emitter.GetArrayMethodRef(
                    arraySym,
                    methodId,
                    this.methodSym.ClassSym,
                    this.methodSym);

                DebugUtil.Assert(mi != null);
                PutILInstruction(il, mi);
            } // if (methodId == ARRAYMETHOD.CTOR)
        }

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        //private void genFlipParams(TYPESYM * elem, unsigned count);

        //------------------------------------------------------------
        // ILGENREC.GenPtrAddr
        //
        /// <summary></summary>
        /// <param name="operandExpr"></param>
        /// <param name="noConv"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenPtrAddr(EXPR operandExpr, bool noConv, bool valUsed)
        {
            // strings are special...
            if (operandExpr.TypeSym.IsPINNEDSYM &&
                (operandExpr.TypeSym as PINNEDSYM).BaseTypeSym.IsPredefType(PREDEFTYPE.STRING))
            {
                GenExpr(operandExpr, true);
                if (!valUsed)
                {
                    PutILInstruction(ILCODE.POP);
                    return;
                }
                PutILInstruction(ILCODE.CONV_I);
                PutILInstruction(ILCODE.DUP);
                BBLOCK block = CreateNewBB(false);
                StartNewBB(null, ILCODE.BRFALSE, block, ILCODE.ILLEGAL);
                --this.currentStackCount;
                PutILInstruction(
                    ILCODE.CALL,
                    compiler.MainSymbolManager.StringOffsetMethSym,
                    null,
                    null);
                ++this.currentStackCount;
                MarkStackMax();
                PutILInstruction(ILCODE.ADD);
                StartNewBB(block, ILCODE.next, null, ILCODE.ILLEGAL);
            }
            else
            {
                LOCSLOTINFO slot = new LOCSLOTINFO();
                GenMemoryAddress(operandExpr, ref slot, true, false);
                if (!valUsed)
                {
                    PutILInstruction(ILCODE.POP);
                }
                else if (!noConv)
                {
                    PutILInstruction(ILCODE.CONV_U);
                }
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenSizeOf
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        private void GenSizeOf(TYPESYM sym)
        {
            PutILInstruction(ILCODE.SIZEOF, sym);
        }

        //------------------------------------------------------------
        // ILGENREC.GenIntConstant
        //
        /// <summary>
        /// generate an integer constant by using the smallest possible ilcode
        /// </summary>
        /// <param name="val"></param>
        //------------------------------------------------------------
        private void GenIntConstant(int val)
        {
            DebugUtil.Assert(ILCODE.LDC_I4_M1 + 9 == ILCODE.LDC_I4_8);

            if (val >= -1 && val <= 8)
            {
                PutILInstruction((ILCODE)(ILCODE.LDC_I4_0 + val));
                return;
            }

            //if (val == (int)(byte)(0xFF & val))
            if (val == (int)(byte)(0x7F & val))
            {
                PutILInstruction(ILCODE.LDC_I4_S, (byte)val);
                return;
            }

            PutILInstruction(ILCODE.LDC_I4, val);
        }

        //------------------------------------------------------------
        // ILGENREC.GenFloatConstant
        //
        /// <summary></summary>
        /// <param name="val"></param>
        //------------------------------------------------------------
        private void GenFloatConstant(float val)
        {
            PutILInstruction(ILCODE.LDC_R4, val);
            //putDWORD(*(DWORD*)(&val));
        }

        //------------------------------------------------------------
        // ILGENREC.GenDoubleConstant
        //
        /// <summary></summary>
        /// <param name="val"></param>
        //------------------------------------------------------------
        private void GenDoubleConstant(double val)
        {
            PutILInstruction(ILCODE.LDC_R8, val);
            //putQWORD((__int64 *)val);
        }

        //------------------------------------------------------------
        // ILGENREC.GenDecimalConstant
        //------------------------------------------------------------
        private void GenDecimalConstant(Decimal decVal)
        {
            AGGSYM decimalAggSym = compiler.GetOptPredefAgg(PREDEFTYPE.DECIMAL, true);
            DebugUtil.Assert(decimalAggSym != null);

            METHSYM ctorSym = compiler.MainSymbolManager.LookupAggMember(
                compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR), decimalAggSym,
                SYMBMASK.ALL) as METHSYM;
            AGGTYPESYM int32Sym = compiler.GetReqPredefType(PREDEFTYPE.INT, true);
            int countArgs = 0;

            int decLow, decMid, decHigh;
            bool decSign;
            byte decScale;

            Util.GetDecimalBits(decVal, out decLow, out decMid, out decHigh, out decSign, out decScale);

            // check if we can call a simple constructor
            if (decScale == 0 &&
                decHigh == 0 &&
                ((decMid & 0x80000000) == 0 || (decSign && (uint)decMid == 0x80000000 && decLow == 0)))
            {
                if (decMid == 0 &&
                    ((decLow & 0x80000000)) == 0 || decSign && (uint)decLow == 0x80000000)
                {
                    int val32 = decLow;
                    if (decSign)
                    {
                        val32 = -val32;
                    }
                    GenIntConstant(val32);

                    while (ctorSym.ParameterTypes.Count != 1 || ctorSym.ParameterTypes[0] != int32Sym)
                    {
                        ctorSym = ctorSym.NextSameNameSym as METHSYM;
                    }
                }
                else
                {
                    long val64;
                    val64 = (long)(((ulong)(uint)decMid << 32) | (ulong)(uint)decLow);

                    if (decSign)
                    {
                        val64 = -val64;
                    }
                    GenLongConstant(val64);

                    AGGTYPESYM int64Sym = compiler.GetReqPredefType(PREDEFTYPE.LONG, true);
                    while (ctorSym.ParameterTypes.Count != 1 || ctorSym.ParameterTypes[0] != int64Sym)
                    {
                        ctorSym = ctorSym.NextSameNameSym as METHSYM;
                    }
                }
                countArgs = 1;
            }
            else
            {
                // new Decimal(lo, mid, hi, sign, scale)
                GenIntConstant(decLow);
                GenIntConstant(decMid);
                GenIntConstant(decHigh);
                GenIntConstant(decSign ? 1 : 0);
                GenIntConstant((int)decScale);

                AGGTYPESYM bool32Sym = compiler.GetReqPredefType(PREDEFTYPE.BOOL, true);
                AGGTYPESYM byte32Sym = compiler.GetReqPredefType(PREDEFTYPE.BYTE, true);
                while (
                    ctorSym.ParameterTypes.Count != 5 ||
                    ctorSym.ParameterTypes[0] != int32Sym ||
                    ctorSym.ParameterTypes[1] != int32Sym ||
                    ctorSym.ParameterTypes[2] != int32Sym ||
                    ctorSym.ParameterTypes[3] != bool32Sym ||
                    ctorSym.ParameterTypes[4] != byte32Sym)
                {
                    ctorSym = ctorSym.NextSameNameSym as METHSYM;
                    DebugUtil.Assert(ctorSym != null);
                }
                countArgs = 5;
            }

            PutILInstruction(ILCODE.NEWOBJ, ctorSym.ConstructorInfo);
            this.currentStackCount -= countArgs;
        }

        //------------------------------------------------------------
        // ILGENREC.GenLongConstant
        //
        /// <summary>
        /// generate a long, but try to do it as an int if it fits...
        /// </summary>
        /// <remarks>
        /// private void GenLongConstant(__int64 * val)
        /// </remarks>
        /// <param name="val"></param>
        //------------------------------------------------------------
        private void GenLongConstant(long val)
        {
            if ((long)((int)(val & 0xFFFFFFFF)) == val)
            {
                GenIntConstant((int)val);
                PutILInstruction(ILCODE.CONV_I8);
            }
            else if ((ulong)((uint)((val) & 0xFFFFFFFF)) == (ulong)val)
            {
                GenIntConstant((int)val);
                PutILInstruction(ILCODE.CONV_U8);
            }
            else
            {
                PutILInstruction(ILCODE.LDC_I8, val);
                //putQWORD(val);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.genZero
        //
        /// <summary></summary>
        /// <param name="tree"></param>
        /// <param name="type"></param>
        //------------------------------------------------------------
        private void GenZero(BASENODE tree, TYPESYM type)
        {
            switch (type.FundamentalType())
            {
                case FUNDTYPE.REF:
                    PutILInstruction(ILCODE.LDNULL);
                    break;

                case FUNDTYPE.PTR:
                    GenIntConstant(0);
                    PutILInstruction(ILCODE.CONV_U);
                    break;

                case FUNDTYPE.STRUCT:
                    DebugUtil.Assert(
                        type.IsPredefType(PREDEFTYPE.INTPTR) ||
                        type.IsPredefType(PREDEFTYPE.UINTPTR));
                    GenIntConstant(0);
                    PutILInstruction(ILCODE.CONV_I);
                    break;

                default:
                    type = type.UnderlyingType();
                    DebugUtil.Assert(type.IsPredefined());
                    EXPRCONSTANT expr = new EXPRCONSTANT();
                    expr.Kind = EXPRKIND.CONSTANT;

                    expr.ConstVal.SetObject(
                        compiler.MainSymbolManager.GetPredefZero((PREDEFTYPE)(type.GetPredefType())));

                    expr.TypeSym = type;
                    expr.TreeNode = tree;
                    GenExpr(expr, true);
                    break;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GenSwap
        //
        /// <summary></summary>
        /// <param name="binopExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenSwap(EXPRBINOP binopExpr, bool valUsed)
        {
            if (!valUsed)
            {
                GenSideEffects(binopExpr);
                return;
            }

            GenExpr(binopExpr.Operand1, true);
            LOCSLOTINFO slot = STOREINTEMP(binopExpr.Operand2.TypeSym, TEMP_KIND.SHORTLIVED);
            GenExpr(binopExpr.Operand2, true);
            DumpLocal(slot, false);
            FreeTemporary(slot);
        }

        //------------------------------------------------------------
        // ILGENREC.EmitRefValue
        //
        /// <summary></summary>
        /// <param name="binopExpr"></param>
        //------------------------------------------------------------
        private void EmitRefValue(EXPRBINOP binopExpr)
        {
            GenExpr(binopExpr.Operand1, true);
            PutILInstruction(ILCODE.REFANYVAL, (binopExpr.Operand2 as EXPRTYPEOF).SourceTypeSym);
        }

        //------------------------------------------------------------
        // ILGENREC.GenMakeRefAny
        //
        /// <summary></summary>
        /// <param name="binopExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenMakeRefAny(EXPRBINOP binopExpr, bool valUsed)
        {
            if (!valUsed)
            {
                GenSideEffects(binopExpr.Operand1);
                return;
            }

            LOCSLOTINFO slot = null;
            GenMemoryAddress(binopExpr.Operand1, ref slot, false, false);
            PutILInstruction(ILCODE.MKREFANY, binopExpr.Operand1.TypeSym);
        }

        //------------------------------------------------------------
        // ILGENREC.GenTypeRefAny
        //
        /// <summary></summary>
        /// <param name="binopExpr"></param>
        /// <param name="valUsed"></param>
        //------------------------------------------------------------
        private void GenTypeRefAny(EXPRBINOP binopExpr, bool valUsed)
        {
            if (!valUsed)
            {
                GenSideEffects(binopExpr.Operand1);
                return;
            }

            GenExpr(binopExpr.Operand1, true);
            PutILInstruction(ILCODE.REFANYTYPE);
            PutILInstruction(ILCODE.CALL, (binopExpr.Operand2 as EXPRTYPEOF).MethodSym, null, null);
        }

        //------------------------------------------------------------
        // ILGENREC.DumpLocal (1)
        //
        /// <summary>
        /// <para>store or load a local or param.
        /// we try to use the smallest opcode available...</para>
        /// <para>Generates codes to load a local variable on the stack.</para>
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="store"></param>
        //------------------------------------------------------------
        private void DumpLocal(LOCSLOTINFO slot, bool store)
        {
#if DEBUG
            if (!(slot.HasIndex))
            {
                ;
            }
#endif
            DebugUtil.Assert(slot.HasIndex);
            DumpLocal(slot.Index, slot.IsParameter, store);

            if (!store && slot.IsPinned)
            {
                PutILInstruction(ILCODE.CONV_I);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.DumpLocal (2)
        //
        /// <summary>
        /// <para>Generates codes to load a local variable on the stack.</para>
        /// </summary>
        /// <param name="islot"></param>
        /// <param name="isParam"></param>
        /// <param name="store"></param>
        //------------------------------------------------------------
        private void DumpLocal(int islot, bool isParam, bool store)
        {
            //DebugUtil.Assert(store == false || store == true);

            ILCODE ilcode;

            int idx1 = (isParam ? 2 : 0) + (store ? 1 : 0); //(int) store;
            if (islot < 4)
            {
                ilcode = ILGENREC.ILlsTiny[idx1, islot];
                if (ilcode == ILCODE.ILLEGAL)
                {
                    goto USE_S;
                }
                PutILInstruction(ilcode);
            }
            else if (islot <= (int)0xFF)
            {
                goto USE_S;
            }
            else
            {
                DebugUtil.Assert(islot <= 0xffff);
                PutILInstruction(ILGENREC.ILlsTiny[idx1, 5], (ushort)islot);
                //PutWORD((ushort)islot);
            }
            return;

        USE_S:
            PutILInstruction(ILGENREC.ILlsTiny[idx1, 4], (char)islot);
            //PutCHAR((char)islot);
        }

        //------------------------------------------------------------
        // ILGENREC.StoreLocal
        //
        /// <summary></summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private LOCSLOTINFO StoreLocal(LOCSLOTINFO slot)
        {
            DumpLocal(slot, true);
            return slot;
        }

        //------------------------------------------------------------
        // ILGENREC.isExprOptimizedAway
        //
        /// <summary></summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsExprOptimizedAway(EXPR tree)
        {
        AGAIN:
            switch (tree.Kind)
            {
                case EXPRKIND.LOCAL:
                    return ((tree as EXPRLOCAL).LocVarSym.LocSlotInfo.TypeSym == null);

                case EXPRKIND.FIELD:
                    tree = (tree as EXPRFIELD).ObjectExpr;
                    if (tree != null) goto AGAIN;
                    return false;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.InitDumpingAllBlocks (DEBUG)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void InitDumpingAllBlocks()
        {
#if DEBUG
            this.privShouldDumpAllBlocks = false;

            if (this.aggSym.Name == null || this.methodSym.Name == null)
            {
                return;
            }

            if (!COMPILER.IsRegString("*", "BlockClass") &&
                !COMPILER.IsRegString(this.aggSym.Name, "BlockClass"))
            {
                return;
            }

            if (!COMPILER.IsRegString("*", "BlockMethod") &&
                !COMPILER.IsRegString(this.methodSym.Name, "BlockMethod"))
            {
                return;
            }

            this.privShouldDumpAllBlocks = true;
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.ShouldDumpAllBlocks
        //------------------------------------------------------------
        private bool ShouldDumpAllBlocks()
        {
#if DEBUG
            return privShouldDumpAllBlocks;
#else
            return false;
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.DumpAllBlocks
        //------------------------------------------------------------
        private void DumpAllBlocks(string label)
        {
#if DEBUG
            if (!ShouldDumpAllBlocks())
            {
                return;
            }

            DebugPrintf("******** {0}\n", label);

            DebugPrintf("{0} : {1}\n", this.aggSym.Name, this.methodSym.Name);


            int count = 0;
            BBLOCK bblock;
            for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                ++count;
            }

            //BBLOCK ** list = STACK_ALLOC(BBLOCK*, count);
            BBLOCK[] list = new BBLOCK[count];

            int i;
            for (i = 0, bblock = this.firstBBlock; bblock != null; bblock = bblock.Next, ++i)
            {
                list[i] = bblock;
            }

            for (i = 0, bblock = this.firstBBlock; bblock != null; bblock = bblock.Next, ++i)
            {
                bool tryEnd = false;
                bool handlerEnd = false;
                for (HANDLERINFO hi = this.handlerInfos; hi != null; hi = hi.Next)
                {
                    if (hi.TryBeginBBlock == null)
                    {
                        break;
                    }
                    if (hi.TryBeginBBlock == bblock)
                    {
                        DebugPrintf("TRY: ");
                    }
                    if (hi.TryEndBBlock == bblock)
                    {
                        tryEnd = true;
                    }
                    if (hi.HandlerBeginBBlock == bblock)
                    {
                        DebugPrintf("HAND: ");
                    }
                    if (hi.HandlerEndBBlock == bblock)
                    {
                        handlerEnd = true;
                    }
                }
                DebugPrintf("Block {0} : {1} : ", FindBlockInList(list, bblock), bblock.BBlockID);
                if (bblock.ExitIL == ILCODE.next && bblock.CurrentLength == 0)
                {
                    DebugPrintf("EMPTY \n");
                }
                else
                {
                    DebugPrintf("Size({0}) : ", bblock.CurrentLength);
                    bool doJump = true;
                    switch (bblock.ExitIL)
                    {
                        case ILCODE.next:
                            DebugPrintf("ILCODE.next");
                            doJump = false;
                            break;

                        case ILCODE.stop:
                            DebugPrintf("ILCODE.stop");
                            doJump = false;
                            break;

                        case ILCODE.SWITCH:
                        case ILCODE.RET:
                            doJump = false;
                            goto default;

                        default:
                            //DebugPrintf("%s ", FetchAtIndex(ILnames, bblock.ExitIL));
                            DebugPrintf("{0} ", ILnames[(int)bblock.ExitIL]);
                            break;
                    }

                    if (doJump)
                    {
                        DebugPrintf(" -. {0}", FindBlockInList(list, bblock.JumpDestBBlock));
                    }
                    if (tryEnd)
                    {
                        DebugPrintf(" TRYEND");
                    }
                    if (handlerEnd)
                    {
                        DebugPrintf(" HANDEND");
                    }
                    DebugPrintf("\n");
                }
            }
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.dumpAllBlocksContent
        //------------------------------------------------------------
        //private void dumpAllBlocksContent(PCWSTR label);
        private void DumpAllBlocksContent(string label)
        {
#if DEBUG
            if (!ShouldDumpAllBlocks())
            {
                return;
            }

            DebugPrintf("******** {0}\n", label);
            DebugPrintf("\n\n{0} : {1}\n", this.aggSym.Name, this.methodSym.Name);

            int count = 0;
            BBLOCK bblock;
            for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                ++count;
            }

            //BBLOCK ** list = STACK_ALLOC(BBLOCK*, count);
            BBLOCK[] list = new BBLOCK[count];

            int i;
            for (i = 0, bblock = this.firstBBlock; bblock != null; bblock = bblock.Next, ++i)
            {
                list[i] = bblock;
            }

            DEBUGINFO debugInfo = null;
            int lastOff = -1;

            for (i = 0, bblock = this.firstBBlock; bblock != null; bblock = bblock.Next, ++i)
            {
                if (bblock.StartOffset != -1 && bblock.StartOffset != lastOff)
                {
                    DebugPrintf("[{0}] ", bblock.StartOffset);
                    lastOff = bblock.StartOffset;
                }

            GETCDI:
                if (debugInfo == null)
                {
                    debugInfo = bblock.DebugInfo;
                    if (debugInfo != null)
                    {
                        while (debugInfo.Prev != null)
                        {
                            debugInfo = debugInfo.Prev;
                        }
                    }
                }
                else
                {
                    if (debugInfo.EndBBlock == bblock)
                    {
                        if (debugInfo.EndOffset == 0)
                        {
                            debugInfo = null;
                            goto GETCDI;
                        }
                    }
                }

                DebugPrintf("{0} : ", i);
                //BYTE *pb = bblock.code;
                List<ILInstruction> codeList = bblock.CodeList;
                int length = bblock.CurrentLength;

                // If necessary for debugging in the future,
                // we could dump entire instruction stream including typed instruction arguments
                if (length > 0)
                {
                    //BYTE b = pb[0];
                    //BYTE b2;
                    //if (b == 0xfe) {
                    //    b2 = pb[1];
                    //} else {
                    //    b2 = b;
                    //    b = 0xff;
                    //}
                    //ILCODE ilcode = findInstruction(b, b2);

                    ILCODE ilcode = codeList[0].Code;
                    //pb += ILcodesSize[ilcode];
                    //length -= ILcodesSize[ilcode];
                    DebugPrintf("{0}\n", ILnames[(int)ilcode]);
                }
                if (bblock.CurrentLength > 0)
                {
                    if (debugInfo != null)
                    {
                        if (debugInfo.Extent.IsValid)
                        {
                            if (debugInfo.Extent.NoDebugInfo)
                            {
                                DebugPrintf("[ no debug info ]\n");
                            }
                            else
                            {
                                DebugPrintf("[{0} ({1}, {2}) - ({3}, {4})]\n",
                                    debugInfo.Extent.InFileSym.Name,
                                    debugInfo.Extent.BeginPos.LineIndex + 1,
                                    debugInfo.Extent.BeginPos.CharIndex + 1,
                                    debugInfo.Extent.EndPos.LineIndex + 1,
                                    debugInfo.Extent.EndPos.CharIndex + 1);
                            }
                        }
                        else
                        {
                            DebugPrintf("[ invalid debug info ]\n");
                        }

                        if (debugInfo.EndBBlock == bblock && debugInfo.EndOffset == bblock.CurrentLength)
                        {
                            debugInfo = debugInfo.Next;
                            if (debugInfo == null)
                            {
                                debugInfo = bblock.DebugInfo;
                            }
                        }
                    }
                }

                if (bblock.ExitIL == ILCODE.next && bblock.CurrentLength == 0)
                {
                    if (bblock.Sym != null)   // && bblock.Sym != (SYM *)I64(0xCCCCCCCCCCCCCCCC))
                    {
                        DebugPrintf("{0}", compiler.ErrSym(bblock.Sym, null, true));
                    }
                }
                else
                {
                    bool doJump = true;
                    bool noToken = false;
                    switch (bblock.ExitIL)
                    {
                        case ILCODE.next:
                        case ILCODE.stop:
                        case ILCODE.RET:
                            doJump = false;
                            break;

                        case ILCODE.SWITCH:
                            doJump = false;
                            noToken = true;
                            goto default;

                        default:
                            DebugPrintf("{0} ", ILnames[(int)bblock.ExitIL]);
                            break;
                    }

                    if (doJump)
                    {
                        DebugPrintf(" -. {0} ", FindBlockInList(list, bblock.JumpDestBBlock));
                        if (debugInfo != null)
                        {
                            if (debugInfo.Extent.IsValid)
                            {
                                if (debugInfo.Extent.NoDebugInfo)
                                {
                                    DebugPrintf("[ no debug info ]\n");
                                }
                                else
                                {
                                    DebugPrintf("[{0} ({1}, {2}) - ({3}, {4})]\n",
                                        debugInfo.Extent.InFileSym.Name,
                                        debugInfo.Extent.BeginPos.LineIndex + 1,
                                        debugInfo.Extent.BeginPos.CharIndex + 1,
                                        debugInfo.Extent.EndPos.LineIndex + 1,
                                        debugInfo.Extent.EndPos.CharIndex + 1);
                                }
                            }
                            else
                            {
                                DebugPrintf("[ invalid debug info ]\n");
                            }
                        }
                        else
                        {
                            DebugPrintf("\n");
                        }
                    }
                    else
                    {
                        if (!noToken && !doJump && bblock.Sym != null)    // && bblock.Sym != (SYM *)I64(0xCCCCCCCCCCCCCCCC))
                        {
                            DebugPrintf("{0}\n", compiler.ErrSym(bblock.Sym, null, true));
                        }
                        else
                        {
                            DebugPrintf("\n");
                        }
                    }
                }

                if (debugInfo != null && debugInfo.EndBBlock == bblock)
                {
                    debugInfo = null;
                }
            }

            // dump handlers
            for (HANDLERINFO hi = this.handlerInfos; hi != null; hi = hi.Next)
            {
                DebugPrintf("handler: start {0} end {1} hand {2} last {3}\n",
                    FindBlockInList(list, hi.TryBeginBBlock),
                    FindBlockInList(list, hi.TryEndBBlock),
                    FindBlockInList(list, hi.HandlerBeginBBlock),
                    FindBlockInList(list, hi.HandlerEndBBlock));
            }
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.FindBlockInList
        //------------------------------------------------------------
#if DEBUG
        private int FindBlockInList(BBLOCK[] list, BBLOCK block)
        {
            for (int i = 0; i < list.Length; ++i)
            {
                if (list[i] == block)
                {
                    return i;
                }
            }
            return -1;
        }

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        //private ILCODE findInstruction(BYTE b1, BYTE b2);

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        //private void  verifyAllTempsFree();

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        private StringBuilder debugSb = null;

        private void InitDebugPrintf()
        {
            debugSb = new StringBuilder();
        }

        private void DebugPrintf(string format, params object[] args)
        {
            if (debugSb != null)
            {
                debugSb.AppendFormat(format, args);
            }
        }
#endif

        //------------------------------------------------------------
        // ILGENREC.AllocTemporary (1)
        //
        /// <summary>
        /// <para>allocate a new temporary, and record where from when in debug</para>
        /// <para>Return a temporary LOCSLOTINFO instance.
        /// If an instance with the same TYPESYM and TEMP_KIND fields exits, return it.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="tempKind"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private LOCSLOTINFO AllocTemporary(
            TYPESYM typeSym,
            TEMP_KIND tempKind,
            string file,
            int line)
        {
            if (typeSym.IsNULLSYM)
            {
                typeSym = compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true);
            }

            LOCSLOTINFO tempSlot = null;

            foreach (LOCSLOTINFO slot in this.temporarySlots)
            {
                if (slot.TypeSym == null ||
                    (!slot.IsTaken &&
                    slot.TypeSym == typeSym &&
                    slot.TempKind == tempKind &&
                    (!compiler.OptionManager.Optimize ||
                    //(m_pbldrLocalSlotArray.Length() / sizeof(IlSlotInfo)) > 60)))
                    this.localSlotList.Count > 60)))
                {
                    tempSlot = slot;
                    break;
                }
            }

            if (tempSlot == null)
            {
                tempSlot = new LOCSLOTINFO();
                this.temporarySlots.Add(tempSlot);
            }
            DebugUtil.Assert(tempSlot != null);
            tempSlot.IsTaken = true;
            if (tempSlot.LocalBuilder == null)
            {
                tempSlot.LocalBuilder = GetLocalBuilder(
                    tempKind,
                    typeSym,
                    false);
                tempSlot.Index = tempSlot.LocalBuilder.LocalIndex;
            }
            else if (!tempSlot.HasIndex)
            {
                tempSlot.Index = tempSlot.LocalBuilder.LocalIndex;
            }
            tempSlot.IsTemporary = true;
            tempSlot.TypeSym = typeSym;
            tempSlot.TempKind = tempKind;

#if DEBUG
            if (!String.IsNullOrEmpty(file))
            {
                tempSlot.LastFile=file;
            }
            if (line >= 0)
            {
                tempSlot.LastLine = line;
            }
#endif
            return tempSlot;
        }

#if false
        private LOCSLOTINFO AllocTemporary(
            TYPESYM typeSym,
            TEMP_KIND tempKind,
            string file,
            int line)
        {
            if (typeSym.IsNULLSYM)
            {
                typeSym = compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true);
            }

            for (TEMPBUCKET bucket = this.temporaryBuckets; true; bucket = bucket.Next)
            {
                bool mustFindInThisBucket = false;
                if (bucket == null)
                {
                    mustFindInThisBucket = true;
                    bucket = AllocBucket();
                }
                for (int i = 0; i < TEMPBUCKET.SIZE; ++i)
                {
                    // Only use this slot if it's new (typeSym == null) OR
                    // we're not optimizing and the slot 'matches'
                    // Or we're getting close to the 64-locals limit that the JIT tracks

                    if (bucket.Slots[i].TypeSym == null ||
                        (!bucket.Slots[i].IsTaken &&
                        bucket.Slots[i].TypeSym == typeSym &&
                        bucket.Slots[i].TempKind == tempKind &&
                        (!compiler.Options.Optimizations ||
                        //(m_pbldrLocalSlotArray.Length() / sizeof(IlSlotInfo)) > 60)))
                        this.localSlotList.Count > 60)))
                    {
                        bucket.Slots[i].IsTaken = true;

                        if (!bucket.Slots[i].HasIndex)
                        {
                            bucket.Slots[i].Index = GetLocalSlot(tempKind, typeSym);
                        }

                        bucket.Slots[i].IsTemporary = true;
                        bucket.Slots[i].TypeSym = typeSym;
                        bucket.Slots[i].TempKind = tempKind;
#if DEBUG
                        if (!String.IsNullOrEmpty(file))
                        {
                            bucket.Slots[i].LastFile = file;
                        }
                        if (line >= 0)
                        {
                            bucket.Slots[i].LastLine = line;
                        }
#endif
                        return bucket.Slots[i];
                    }
                }
                DebugUtil.Assert(!mustFindInThisBucket);
            }
        }
#endif

        //------------------------------------------------------------
        // ILGENREC.AllocTemporary (2)
        //
        /// <summary>
        /// <para>Call AllocTemporary method with no debug infomations.</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tempKind"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private LOCSLOTINFO AllocTemporary(TYPESYM type, TEMP_KIND tempKind)
        {
            return AllocTemporary(type, tempKind, null, -1);
        }

        //------------------------------------------------------------
        // ILGENREC.AllocBucket
        //
        /// <summary>
        /// Create a TEMPBUCKET instance and return it.
        /// And add it to the list starting at this.temporaryBuckets.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
#if false
        private TEMPBUCKET AllocBucket()
        {
            //TEMPBUCKET * bucket = (TEMPBUCKET *) allocator->AllocZero(sizeof(TEMPBUCKET));
            TEMPBUCKET bucket = new TEMPBUCKET();

            if (this.temporaryBuckets == null)
            {
                this.temporaryBuckets = bucket;
            }
            else
            {
                TEMPBUCKET temp = this.temporaryBuckets;
                while (temp.Next != null) temp = temp.Next;
                temp.Next = bucket;
            }
            return bucket;
        }
#endif

        //------------------------------------------------------------
        // ILGENREC.FreeTemporary
        //
        /// <summary>
        /// <para>free the given temporary</para>
        /// <para>Set IsTaken field false.</para>
        /// </summary>
        /// <param name="slot"></param>
        //------------------------------------------------------------
        private void FreeTemporary(LOCSLOTINFO slot)
        {
            //----------------------------------------
            // temporary cure
            //----------------------------------------
            if (!slot.IsTaken)
            {
                return;
            }

            DebugUtil.Assert(
                slot.IsTaken &&
                !slot.IsParameter &&
                slot.TypeSym != null &&
                slot.IsTemporary);

            slot.IsTaken = false;
        }

        //------------------------------------------------------------
        // ILGENREC.DumpToDurable
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <param name="isFree"></param>
        /// <param name="dumpToStack"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private LOCSLOTINFO DumpToDurable(
            TYPESYM type,
            bool isFree,
            bool dumpToStack)	// = true
        {
            LOCSLOTINFO temp = STOREINTEMP(type, TEMP_KIND.DURABLE);
            CloseDebugInfo();
            OpenDebugInfo(SpecialDebugPointEnum.HiddenCode, EXPRFLAG.NODEBUGINFO);
            if (dumpToStack)
            {
                DumpLocal(temp, false);
            }
            if (!isFree)
            {
                return temp;
            }

            FreeTemporary(temp);
            return null;
        }

        //------------------------------------------------------------
        // ILGENREC.EmitFieldToken
        //
        /// <summary>
        /// <para>emit MD tokens of various types</para>
        /// <para>(sscli) mathodInType has the default value null.</para>
        /// </summary>
        /// <param name="fieldSym"></param>
        /// <param name="methodInType"></param>
        //------------------------------------------------------------
        //private void EmitFieldToken(
        //    MEMBVARSYM fieldSym,
        //    AGGTYPESYM methodInType)    // = null
        //{
        //    //mdToken tok;
        //
        //    DebugUtil.Assert(methodInType != null && methodInType.GetAggregate() == fieldSym.ClassSym);
        //
        //    // GENERICS: emit a field ref with a typeSpec parent whenever we use a field in a generic class.
        //    //if (methodInType.AllTypeArguments.Count >0)
        //    //{
        //    //    tok = compiler().emitter.GetMembVarRef(fieldSym, methodInType);
        //    //}
        //    //else
        //    //{
        //    //    tok = compiler().emitter.GetMembVarRef(fieldSym);
        //    //}
        //#if DEBUG
        //    //DebugUtil.Assert(!inlineBB.Sym);
        //    this.inlineBBlock.Sym = fieldSym;
        //#endif
        //    //putDWORD(tok);
        //    if (fieldSym.FieldInfo != null)
        //    {
        //        PutIlArgument(fieldSym.FieldInfo);
        //    }
        //}

        //------------------------------------------------------------
        // ILGENREC::emitFieldToken (sscli)
        //
        // emit MD tokens of various types
        //------------------------------------------------------------
        //void ILGENREC::emitFieldToken(MEMBVARSYM * sym, AGGTYPESYM *methodInType)
        //{
        //    mdToken tok;
        //
        //    ASSERT(methodInType && methodInType->getAggregate() == sym->getClass());
        //
        //    // GENERICS: emit a field ref with a typeSpec parent whenever we use a field in a generic class.
        //    if (methodInType->typeArgsAll->size) {
        //        tok = compiler()->emitter.GetMembVarRef(sym, methodInType);
        //    }
        //    else {
        //        tok = compiler()->emitter.GetMembVarRef(sym);
        //    }
        //#if DEBUG
        ////    ASSERT(!inlineBB.sym);
        //    inlineBB.sym = sym;
        //#endif
        //    putDWORD(tok);
        //}

        //------------------------------------------------------------
        // ILGENREC.EmitMethodToken
        //
        // This method puts the metadata token of the specified method.
        // In this project, PutILInstruction methods do the operation.
        //------------------------------------------------------------
        //------------------------------------------------------------
        // ILGENREC::emitMethodToken (sscli)
        //------------------------------------------------------------
        //void ILGENREC::emitMethodToken(METHSYM * sym, AGGTYPESYM *methodInType, TypeArray *pMethArgs)
        //{
        //    if (!methodInType) {
        //        methodInType = sym->getClass()->getThisType();
        //        ASSERT(!methodInType->typeArgsAll->size);
        //    }
        //    ASSERT(sym->typeVars->size == (pMethArgs ? pMethArgs->size : 0));
        //
        //    mdToken tok = compiler()->emitter.GetMethodRef(sym, methodInType, pMethArgs);
        //
        //#if DEBUG
        //    inlineBB.sym = sym;
        //#endif
        //    putDWORD(tok);
        //}

        //------------------------------------------------------------
        // ILGENREC.emitTypeToken
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        //private void EmitTypeToken(TYPESYM sym)
        //{
        //    //mdToken tok = compiler().emitter.GetTypeRef(sym, false);
        //#if DEBUG
        //    this.inlineBBlock.Sym = sym;
        //#endif
        //    if (sym.Type != null)
        //    {
        //        PutIlArgument(sym.Type);
        //    }
        //}

        //------------------------------------------------------------
        // ILGENREC::emitTypeToken (sscli)
        //------------------------------------------------------------
        //void ILGENREC::emitTypeToken(TYPESYM * sym)
        //{
        //    mdToken tok = compiler()->emitter.GetTypeRef(sym, false);
        //#if DEBUG
        //    inlineBB.sym = sym;
        //#endif
        //    putDWORD(tok);
        //}

        //------------------------------------------------------------
        // ILGENREC.EmitArrayMethodToken
        //
        /// <summary></summary>
        /// <param name="arraySym"></param>
        /// <param name="methodId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private MethodInfo EmitArrayMethodToken(ARRAYSYM arraySym, ARRAYMETHOD methodId)
        {
            //MethodInfo mi = compiler.Emitter.GetArrayMethodRef(arraySym, methodId);
#if DEBUG
            //this.currentBBlock.Sym = arraySym;
#endif
            //return mi;
            DebugUtil.VsFail("Do not call this method.");
            return null;
        }

        //------------------------------------------------------------
        // ILGENREC::emitArrayMethodToken (sscli)
        //------------------------------------------------------------
        //void ILGENREC::emitArrayMethodToken(ARRAYSYM * sym, ARRAYMETHOD methodId)
        //{
        //    mdToken tok = compiler()->emitter.GetArrayMethodRef(sym, methodId);
        //#if DEBUG
        //    inlineBB.sym = sym;
        //#endif
        //    putDWORD(tok);
        //}

        //------------------------------------------------------------
        // ILGENREC.NeedsBoxing
        //
        /// <summary>
        /// see notes at places where this is called as to why I've made it more lenient.
        /// </summary>
        /// <param name="parentSym"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool NeedsBoxing(PARENTSYM parentSym, TYPESYM typeSym)
        {
            DebugUtil.Assert(typeSym.IsStructOrEnum());
            if (typeSym.IsNUBSYM)
            {
                return !parentSym.IsAGGSYM || !(parentSym as AGGSYM).IsPredefAgg(PREDEFTYPE.G_OPTIONAL);
            }
            return (parentSym != typeSym.GetAggregate());
        }

        //------------------------------------------------------------
        // ILGENREC.MarkAllReachableBB
        //
        /// <summary>
        /// <para>mark all reachable blocks starting from bblock
        /// using the provided marking function</para>
        /// </summary>
        /// <param name="bblock"></param>
        //------------------------------------------------------------
        private void MarkAllReachableBB(BBLOCK bblock)
        {
            MARKREACHABLEINFO marker = new MARKREACHABLEINFO();
            do
            {
                marker.MarkAllReachableBB(bblock);
                bblock = marker.Pop();
            } while (bblock != null);
        }

        //------------------------------------------------------------
        // ILGENREC.AdvanceToNonNOP
        //
        /// <summary>
        /// <para>Move bblock to the BBLOCK instance which is not NOP.</para>
        /// <para>If the new target is a tyr block, return a non-zero value.</para>
        /// </summary>
        /// <param name="bblock"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int AdvanceToNonNOP(ref BBLOCK bblock)
        {
            int rval = 0;
            while (bblock != null && bblock.IsNOP)
            {
                if (bblock.Next.StartsException)
                {
                    rval = -1;
                }
                bblock = bblock.Next;
            }
            return rval;
        }

        //------------------------------------------------------------
        // ILGENREC.OptimizeBranchesToNOPs
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void OptimizeBranchesToNOPs()
        {
            BBLOCK bblock;
            int i;

            for (bblock = this.firstBBlock; bblock.Next != null; bblock = bblock.Next)
            {
                switch (bblock.ExitIL)
                {
                    case ILCODE.RET:
                    case ILCODE.next:
                    case ILCODE.stop:
                    case ILCODE.ENDFINALLY:
                        break;

                    case ILCODE.SWITCH:
                        for (i = 0; i < bblock.SwitchDest.Count; ++i)
                        {
                            if (bblock.SwitchDest.BBlockList[i] == null)
                            {
                                continue;
                            }
                            if (AdvanceToNonNOP(ref bblock.SwitchDest.BBlockList[i].DestBBlock) != 0)
                            {
                                bblock.SwitchDest.BBlockList[i].JumpIntoTry = true;
                            }
                        }
                        break;

                    default:
                        if (AdvanceToNonNOP(ref bblock.JumpDestBBlock) != 0)
                        {
                            bblock.JumpIntoTry = true;
                        }
                        break;
                }
            }

            // must advance handlers past NOPs as well
            // so that handlers don't point to bogusly unreachable BlockArray
            for (HANDLERINFO hi = this.handlerInfos; hi != null; hi = hi.Next)
            {
                AdvanceToNonNOP(ref hi.TryBeginBBlock);
                AdvanceToNonNOP(ref hi.TryEndBBlock);
                AdvanceToNonNOP(ref hi.HandlerBeginBBlock);
                AdvanceToNonNOP(ref hi.HandlerEndBBlock);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.OptimizeBranchesToNext
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void OptimizeBranchesToNext()
        {
            // branches to next block:
            for (BBLOCK bblock = this.firstBBlock; bblock.Next != null; bblock = bblock.Next)
            {
                BBLOCK targetBBlock;
                switch (bblock.ExitIL)
                {
                    case ILCODE.RET:
                    case ILCODE.next:
                    case ILCODE.ENDFINALLY:
                    case ILCODE.SWITCH:
                    case ILCODE.stop:
                    case ILCODE.LEAVE:
                        break;

                    default:
                        targetBBlock = bblock.Next;
                        do
                        {
                            if (bblock.JumpDestBBlock == targetBBlock)
                            {
                                if (bblock.ExitIL != ILCODE.BR)
                                {
                                    // here we have a conditional branch with equal
                                    // destination addresses

                                    //BYTE * offset = bblock.curLen + bblock.code;
                                    // it's too late now to do any better :-( ...
                                    //putOpcode (&offset, ILCODE.POP);
                                    bblock.CodeList.Add(new ILInstruction(ILCODE.POP));
                                    if (bblock.ExitIL != ILCODE.BRTRUE &&
                                        bblock.ExitIL != ILCODE.BRFALSE)
                                    {
                                        //putOpcode (&offset, ILCODE.POP);
                                        bblock.CodeList.Add(new ILInstruction(ILCODE.POP));
                                        bblock.CurrentLength += ILcodesSize[(int)ILCODE.POP];
                                    }
                                    bblock.CurrentLength += ILcodesSize[(int)ILCODE.POP];
                                }
                                bblock.ExitIL = ILCODE.next;
                                break;
                            }
                            if (targetBBlock.IsNOP)
                            {
                                targetBBlock = targetBBlock.Next;
                            }
                            else
                            {
                                break;
                            }
                        } while (true);
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // ILGENREC.OptimizeBranchesOverBranches
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void OptimizeBranchesOverBranches()
        {
            BBLOCK bblock;

            // cond branches over branches:
            // gotoif A, lab1             gotoif !a, lab2
            // goto lab2        --.      nop
            // lab1:                      lab1:

            for (bblock = this.firstBBlock; bblock.Next != null; bblock = bblock.Next)
            {
                BBLOCK targetBBlock;
                BBLOCK nextBBlock;
                switch (bblock.ExitIL)
                {
                    case ILCODE.RET:
                    case ILCODE.next:
                    case ILCODE.stop:
                    case ILCODE.BR:
                    case ILCODE.ENDFINALLY:
                    case ILCODE.SWITCH:
                    case ILCODE.LEAVE:
                        break;

                    default:
                        nextBBlock = bblock.Next;
                        while (nextBBlock.IsNOP)
                        {
                            nextBBlock = nextBBlock.Next;
                        }
                        if (nextBBlock.ExitIL == ILCODE.BR && nextBBlock.CurrentLength == 0)
                        {
                            targetBBlock = nextBBlock.Next;
                            if (targetBBlock != null)
                            {
                                while (targetBBlock.IsNOP)
                                {
                                    targetBBlock = targetBBlock.Next;
                                }
                                if (bblock.JumpDestBBlock == targetBBlock)
                                {
                                    bblock.JumpDestBBlock = nextBBlock.JumpDestBBlock;
                                    // we wipe the branch:
                                    nextBBlock.ExitIL = ILCODE.next;
                                    bblock.FlipJump();
                                }
                            }
                        }
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // ILGENREC.OptimizeBranchesToBranches
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void OptimizeBranchesToBranches()
        {
            BBLOCK bblock;
            int i;

            // branches to branches and branches to ret
            for (bblock = this.firstBBlock; bblock.Next != null; bblock = bblock.Next)
            {
                switch (bblock.ExitIL)
                {
                    case ILCODE.RET:
                    case ILCODE.next:
                    case ILCODE.stop:
                    case ILCODE.ENDFINALLY:
                        break;

                    case ILCODE.SWITCH:
                        // Need to examine if any of the cases go to a br, in which case they canbe
                        // redirected furher...
                        for (i = 0; i < bblock.SwitchDest.Count; ++i)
                        {
                        AGAINSW:
                            BBLOCK targetBlock = bblock.SwitchDest.BBlockList[i].DestBBlock;
                            if (targetBlock.CurrentLength == 0)
                            {
                                if (bblock.SwitchDest.BBlockList[i] == targetBlock.MarkedWithSwith)
                                {
                                    // protect against cycles > 1 in size
                                    targetBlock.JumpDestBBlock = targetBlock;
                                    continue;
                                }
                                if (targetBlock.ExitIL == ILCODE.BR &&
                                    targetBlock.JumpDestBBlock != targetBlock)
                                {
                                    if (!bblock.SwitchDest.BBlockList[i].JumpIntoTry)
                                    {
                                        targetBlock.MarkedWithSwith = bblock.SwitchDest.BBlockList[i];
                                        bblock.SwitchDest.BBlockList[i].JumpIntoTry = targetBlock.JumpIntoTry;
                                        bblock.SwitchDest.BBlockList[i].DestBBlock = targetBlock.JumpDestBBlock;
                                        goto AGAINSW;
                                    }
                                }
                            }
                        }
                        break;

                    default:
                    // We need to catch extended cycles.  we do this by writing current into the code field of the
                    // destination if we suck it in.  since we only suck in empty blocks we don't overwrite any code
                    AGAINBR:
                        if (bblock.JumpDestBBlock.CurrentLength == 0)
                        {
                            if (bblock == bblock.JumpDestBBlock.MarkedWithBBlock)
                            {
                                // we sucked this in already... which means that we hit a cycle, so we might as well
                                // emit a jump to ourselves...
                                bblock.JumpDestBBlock.JumpDestBBlock = bblock.JumpDestBBlock;
                                break;
                            }
                            if (bblock.JumpDestBBlock.ExitIL == ILCODE.BR &&
                                bblock.JumpDestBBlock.JumpDestBBlock != bblock.JumpDestBBlock)
                            {
                                if (!bblock.JumpIntoTry)
                                {
                                    if (bblock.JumpDestBBlock.JumpIntoTry)
                                    {
                                        bblock.JumpIntoTry = true;
                                    }
                                    //  we suck in the destination, and mark it as sucked in
                                    bblock.JumpDestBBlock.MarkedWithBBlock = bblock;
                                    bblock.JumpDestBBlock = bblock.JumpDestBBlock.JumpDestBBlock;
                                    goto AGAINBR;
                                }
                            }
                        }
                        if (bblock.ExitIL == ILCODE.BR &&
                            bblock.JumpDestBBlock.ExitIL == ILCODE.RET &&
                            bblock.JumpDestBBlock.CurrentLength == ILcodesSize[(int)ILCODE.RET])
                        {
                            bblock.ExitIL = ILCODE.RET;
                            //BYTE * offset = bblock.CurrentLength + bblock.code;
                            //putOpcode (&offset, ILCODE.RET);
                            bblock.CodeList.Add(new ILInstruction(ILCODE.RET));
                            bblock.CurrentLength += ILcodesSize[(int)ILCODE.RET];
                        }
                        break;
                }
            }

            // INVARIANT: No br instruction has br as its immediate target
            DumpAllBlocks("Before opt br to next");

            OptimizeBranchesToNext();
            DumpAllBlocks("After opt br to next");
        }

        //------------------------------------------------------------
        // ILGENREC.OptimizeGotos
        //
        /// <summary>
        /// optimize branches
        /// </summary>
        //------------------------------------------------------------
        private void OptimizeGotos()
        {
#if DEBUG
            if (COMPILER.GetRegDWORD("Before") != 0)
            {
                DumpAllBlocksContent("Before");
            }
#endif

            BBLOCK bblock;

            //--------------------------------------------------------
            // Assign the orders to BBlocks.
            //--------------------------------------------------------
            int order = 0;
            for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                bblock.Order = ++order;
            }

            DumpAllBlocks("after order assigned");

            //--------------------------------------------------------
            //
            //--------------------------------------------------------
            if (compileForEnc)
            {
                for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
                {
                    if (bblock.HasLeaveTarget)
                    {
                        // insert NOP to the top of the code list.
                        bblock.CurrentLength += ILcodesSize[(int)ILCODE.NOP];
                        //bblock.code -= ILcodesSize[(int)ILCODE.NOP];
                        //BYTE * offset = bblock.code;
                        //putOpcode(&offset, ILCODE.NOP);
                        bblock.CodeList.Insert(0, new ILInstruction(ILCODE.NOP));

                        if (TrackDebugInfo())
                        {
                            DEBUGINFO nopDebugInfo = new DEBUGINFO();
                            //nopDebugInfo = (DEBUGINFO*) allocator.AllocZero(sizeof(DEBUGINFO));
                            nopDebugInfo.BeginBBlock = bblock;
                            nopDebugInfo.Extent.SetHiddenInvalidSource();
                            nopDebugInfo.Extent.SetProhibitMerge();
                            nopDebugInfo.BeginOffset = 0;
                            nopDebugInfo.EndBBlock = bblock;
                            nopDebugInfo.EndOffset = ILcodesSize[(int)ILCODE.NOP];
                            nopDebugInfo.AlreadyAdjusted = true;
                            DEBUGINFO dinfo = bblock.DebugInfo;
                            while (dinfo != null && dinfo.Prev != null)
                            {
                                dinfo = dinfo.Prev;
                            }
                            if (dinfo != null)
                            {
                                nopDebugInfo.Next = dinfo;
                                dinfo.Prev = nopDebugInfo;
                            }
                            else
                            {
                                bblock.DebugInfo = nopDebugInfo;
                            }
                        }
                    }
                }
            } // if (compileForEnc)

            DumpAllBlocks("before br to nop");
            OptimizeBranchesToNOPs();
            DumpAllBlocks("after br to nop");

            //--------------------------------------------------------
            // INVARIANT:  all br instructions have actual targets that cannot be skipped.
            //--------------------------------------------------------
            bool redoBranchesToBranches = false;

        REDOBRANCHESTOBRANCHES:
            if (compiler.OptionManager.Optimize || redoBranchesToBranches)
            {
                redoBranchesToBranches = false;
                DebugUtil.Assert(compiler.OptionManager.Optimize);
                OptimizeBranchesToBranches();
                // INVARIANT: No br instruction has an offset of 0.

            }

            for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                bblock.Reachable = false;
            }

            // mark all normally reachable blocks:
            MarkAllReachableBB(this.firstBBlock);

            // now, mark all blocks reachable from reachable exception handlers...

            DumpAllBlocks("after mark all reachable");

            int unreachCountOld = ehCount;

        EXCEPAGAIN:

            int unreachable = 0;

            HANDLERINFO hi;
            for (hi = this.handlerInfos; hi != null; hi = hi.Next)
            {
                bool reached = false;
                for (bblock = hi.TryBeginBBlock;
                    bblock != null && bblock != hi.TryEndBBlock.Next;
                    bblock = bblock.Next)
                {
                    if (bblock.Reachable)
                    {
                        reached = true;
                        if (!hi.HandlerBeginBBlock.Reachable)
                        {
                            MarkAllReachableBB(hi.HandlerBeginBBlock);
                        }
                        break;
                    }
                }
                if (!reached)
                {
                    ++unreachable;
                }
            }

            if (unreachable > 0 && (unreachable != unreachCountOld))
            {
                unreachCountOld = unreachable;
                goto EXCEPAGAIN;
            }
            DebugUtil.Assert(unreachable <= ehCount);
            bool redoTryFinally;

        REDOTRYFINALLY:
            redoTryFinally = false;

            // now, remove handlers for empty bodies if optimizing
            if (compiler.OptionManager.Optimize)
            {
                bool changed = false;

            DOHANDLERS:

                if (changed)
                {
                    redoTryFinally = true;
                    changed = false;
                }

                for (hi = this.handlerInfos; hi != null; hi = hi.Next)
                {

                    if (hi.TryBeginBBlock == null)
                    {
                        continue;
                    }

                    BBLOCK start;
                    start = hi.TryBeginBBlock;
                    bool needToTryHandle = false;

                    while (start != hi.TryEndBBlock && (start.IsEmpty || !start.Reachable))
                    {
                        start = start.Next;
                    }

                    if (start == hi.TryEndBBlock)
                    {
                        // ok, this try block is empty, so we have one of 2 cases, if we optimize:

                        // case one: its a catch, so we transform the catch into a nop...
                        // case 3: its a fault, so we also transform the handler to a nop...

                        if (!hi.IsTryFinally)
                        {
                            changed = true;
                            // if it's a catch, then it will never be executed, so
                            // it's sufficient if we skip it entirely...
                            start = hi.HandlerBeginBBlock;
                            while (start != hi.HandlerEndBBlock.Next)
                            {
                                start.MakeEmpty();
                                start = start.Next;
                            }

                            // also, eliminate the ILCODE.LEAVE at the end of the try block, as
                            // it is the same as a fallthrough now (since there is no catch for it to
                            // jump over...
                            DebugUtil.Assert(
                                hi.TryEndBBlock.ExitIL == ILCODE.LEAVE ||
                                hi.TryEndBBlock.ExitIL == ILCODE.BR ||
                                hi.TryEndBBlock.ExitIL == ILCODE.next);

                            if (hi.TryEndBBlock.ExitIL == ILCODE.LEAVE)
                            {
                                hi.TryEndBBlock.ExitIL = ILCODE.BR;
                                // since this enables more BR to BR:
                                redoBranchesToBranches = true;
                            }

                            hi.TryBeginBBlock = null;

                        }
                        // case 2: its a finally:
                        else
                        {
                            // if it's a finally, then we convert it to a normal block, by
                            // removing the END_FINALLY instruction...
                            //DebugUtil.Assert(start.ExitIL == ILCODE.LEAVE);
                            //start.MakeEmpty();

                            // remove the ILCODE.ENDFINALLY instruction:
                            //hi.HandlerEndBBlock.MakeEmpty();


                            // in both cases, we need to make sure that we don't emit EIT information
                            // for this try:
                            //hi.TryBeginBBlock = null;

                            // Saddly, we can do nothing here... the problem is that the CLR has this rule that an async
                            // thread abort exception is not allowed to abort a thread while its in a finally,
                            // so finalies are significant even if the try had no code...
                            // so, there is only one thing to do:

                            needToTryHandle = true;
                            goto TRYHANDLER;
                        }
                    }
                    else
                    {
                        // non empty try:

                        // the trybody had code in it, buf if the finally or fault body has no code
                        // we can optimize it away...
                        if (!hi.IsTryCatch)
                        {
                            needToTryHandle = true;
                            goto TRYHANDLER;
                        }
                    }

                TRYHANDLER:
                    if (needToTryHandle)
                    {
                        needToTryHandle = false;
                        start = hi.HandlerBeginBBlock;
                        while (start != hi.HandlerEndBBlock && (start.IsEmpty || !start.Reachable))
                        {
                            start = start.Next;
                        }
                        if (start == hi.HandlerEndBBlock)
                        {
                            // also, remove the ILCODE.LEAVE as well as the ILCODE.ENDFINALLY instrs
                            hi.HandlerEndBBlock.MakeEmpty();

                            // BUT, before removing the leave, check that the leave doesn't jump over
                            // code, in which case it cannot be removed  (this is possible since leaves
                            // suck up branches.)  So, if there is code we are jumping over that we
                            // would now fall into, make the leave into a branch instead.

                            if (hi.TryEndBBlock.Reachable)
                            {
                                if (hi.TryEndBBlock.ExitIL == ILCODE.LEAVE)
                                {
                                    start = hi.TryEndBBlock.Next;
                                    while (
                                        start != null &&
                                        (!start.Reachable || start.IsEmpty) &&
                                        start != hi.TryEndBBlock.JumpDestBBlock)
                                    {
                                        start = start.Next;
                                    }
                                    if (start == hi.TryEndBBlock.JumpDestBBlock)
                                    {
                                        hi.TryEndBBlock.MakeEmpty();
                                    }
                                    else
                                    {
                                        hi.TryEndBBlock.ExitIL = ILCODE.BR;
                                        // since this enables more BR to BR:
                                        redoBranchesToBranches = true;
                                    }
                                }
                                else
                                {
                                    DebugUtil.Assert(hi.TryEndBBlock.ExitIL == ILCODE.next);
                                }
                            }

                            // no code, in finally block, so just mark it as unreachable and we
                            // will not emit EIT info for it
                            hi.TryBeginBBlock = null;
                            changed = true;
                        }
                    } // if (needToTryHandle)
                }

                if (changed)
                {
                    goto DOHANDLERS;
                }
                if (redoBranchesToBranches)
                {
                    goto REDOBRANCHESTOBRANCHES;
                }

            }
            else
            {
                // if not optimizing, we will merely remove unreachable trys

                for (hi = this.handlerInfos; hi != null; hi = hi.Next)
                {
                    if (!hi.HandlerBeginBBlock.Reachable)
                    {
                        hi.TryBeginBBlock = null; // this effectively removes the try...
                    }
                }
            }

            DumpAllBlocks("before leave adjustment");

            // we need to insure that those leave's don't go into outer space...
            for (bblock = this.firstBBlock; bblock != null && blockedLeave > 0; bblock = bblock.Next)
            {
                if (bblock.GotoBlocked)
                {
                    --blockedLeave;
                    if (bblock.Reachable)
                    {
                        BBLOCK temp = bblock.JumpDestBBlock;
                        while (temp != null &&
                            !temp.StartsCatch &&
                            !temp.StartsFinally &&
                            !temp.EndsException &&
                            (!temp.Reachable ||
                                (temp.CurrentLength == 0 && (temp.ExitIL == ILCODE.next || temp.ExitIL == ILCODE.stop))))
                        {
                            temp = temp.Next;
                        }
                        if (temp == null ||
                            temp.StartsCatch ||
                            temp.StartsFinally ||
                            temp.EndsException)
                        {
                            // ok, this leave either points to lala land, or into a catch or finally (also illegal),
                            // or, crosses a finally boundary (some false positives here, buts that's ok)
                            // so we need to make it point to some sane piece of code, namely an infinite loop...

                            bblock.JumpDestBBlock.ExitIL = ILCODE.BR;
                            bblock.JumpDestBBlock.CurrentLength = 0;
                            bblock.JumpDestBBlock.JumpDestBBlock = bblock.JumpDestBBlock;
                            bblock.JumpDestBBlock.Reachable = true;
                            bblock.JumpDestBBlock.DebugInfo = null;
                        }
                    }
                }
            }

            // System.Reflection.Emit.ILGenerator automatically inserts OpCodes.Leave codes
            // at the last of try blocks.
            // So, some codes must be after the handler blocks.
            for (hi = this.handlerInfos; hi != null; hi = hi.Next)
            {
                BBLOCK next = null;
                if (hi.HandlerEndBBlock != null &&
                    (next = hi.HandlerEndBBlock.Next) != null)
                {
                    if (next.Next == null)
                    {
                        next.Reachable = true;
                        List<ILInstruction> codeList;
                        if (next.CodeList == null)
                        {
                            next.CodeList = new List<ILInstruction>();
                            PutILInstruction(new ILInstruction(ILCODE.RET), next);
                        }
                        else if (next.CodeList.Count == 0)
                        {
                            PutILInstruction(new ILInstruction(ILCODE.RET), next);
                        }
                        else
                        {
                            codeList = next.CodeList;
                            ILInstruction last = codeList[codeList.Count - 1];
                            if (last.Code != ILCODE.RET)
                            {
                                PutILInstruction(new ILInstruction(ILCODE.RET), next);
                            }
                        }

                    }
                }
            }

            // now, suck up all unreachable blocks from this list...
            // [Well, actually, all we do is wipe the code and the exit instruction...]
            for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                if (!bblock.Reachable)
                {
                    bblock.MakeEmpty();
                }
            }

            // if this were unreachable, it could have been wiped, so let's reset it just in case
            if (this.currentBBlock.ExitIL == ILCODE.next)
            {
                this.currentBBlock.ExitIL = ILCODE.stop;
            }
            DumpAllBlocks("after unreach opts");

            if (compiler.OptionManager.Optimize)
            {
                // now that we no longer have unreachable code, optimize branches over branches...

                OptimizeBranchesOverBranches();
                DumpAllBlocks("after br over br");

                // Now, we might have enabled more branches to next which used to be
                // branches over dead code... so let's do that

                OptimizeBranchesToNext();
                DumpAllBlocks("after br to next");

                // at this point there are no other opts to perform...
                // since branchesToNext removes branches, there are no new cases of
                // branchesToBranches or branchesToRet which can be optimized...

                if (redoTryFinally)
                {
                    goto REDOTRYFINALLY;
                }
            }

            // Now, scan for leaves which needs NOPS instrted to properle leave the try
            // (leaves to first instruction of try)

            for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                if (bblock.Reachable && bblock.ExitIL == ILCODE.LEAVE && bblock.LeaveNestingCount > 0)
                {
                    if (bblock.JumpDestBBlock.TryNestingCount > 0 &&
                        bblock.LeaveNestingCount >= bblock.JumpDestBBlock.LeaveNestingCount)
                    {
                        if (bblock.JumpDestBBlock.TryNestingCount == bblock.LeaveNestingCount)
                        {
                            // if they are equal we want to eliminate the case where the leave just
                            // goes to a following block, in which case the nop is not necessary
                            if (bblock.JumpDestBBlock.Order > bblock.Order)
                            {
                                goto NEXTLEAVE;
                            }
                        }
                        BBLOCK target = bblock.JumpDestBBlock;
                        while (target.Next != null && target.IsNOP)
                        {
                            target = target.Next;
                        }

                        // This might leave the nop in no-man's land (after the try, but before the handler)
                        // That situation can occur if we advance the handlers to the same BB and 
                        // set the nop before that point.  Let's detect this situation and fix it.

                        for (hi = this.handlerInfos; hi != null; hi = hi.Next)
                        {
                            if (hi.TryBeginBBlock != null &&
                                hi.TryEndBBlock != null &&
                                hi.HandlerBeginBBlock != null &&
                                hi.HandlerEndBBlock != null)
                            {
                                if (target.Order > hi.TryEndBBlock.Order &&
                                    target.Order <= hi.HandlerBeginBBlock.Order)
                                {
                                    // This is the rare but bad situation...
                                    hi.HandlerShouldIncludeNOP = true;
                                }
                            }
                        }
                        target.TryNestingCount = -1;
                    }
                }
            NEXTLEAVE: ;
            }

            // useful peephole opt:  sequences such as ILCODE.RET ILCODE.RET are equivalent to ILCODE.RET
            if (compiler.OptionManager.Optimize)
            {
                for (bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
                {
                    if (bblock.ExitIL == ILCODE.RET)
                    {
                        BBLOCK next = bblock.Next;
                        while (next!=null && next.IsNOP)
                        {
                            next = next.Next;
                        }
                        if (next != null &&
                            next.ExitIL == ILCODE.RET &&
                            next.CurrentLength == ILcodesSize[(int)ILCODE.RET])
                        {
                            bblock.ExitIL = ILCODE.next;
                            bblock.CurrentLength -= ILcodesSize[(int)ILCODE.RET];
                        }
                    }
                }
            }
        }

        //------------------------------------------------------------
        // ILGENREC.GetFinalCodeSize
        //
        /// <summary>
        /// calculate the size of our code and set the correct bb offsets.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetFinalCodeSize()
        {
#if DEBUG
            StringBuilder sb = new StringBuilder();
#endif

            if (this.returnLocationBBlock != null && !returnHandled)
            {
                StartNewBB(this.returnLocationBBlock, ILCODE.next, null, ILCODE.ILLEGAL);
                if (closeIndexUsed)
                {
                    OpenDebugInfo(SpecialDebugPointEnum.HiddenCode, EXPRFLAG.NODEBUGINFO);
                }
                else
                {
                    OpenDebugInfo(SpecialDebugPointEnum.CloseCurly, (EXPRFLAG)0);
                }
                if (this.temporaryReturnSlotInfo != null)
                {
                    DumpLocal(this.temporaryReturnSlotInfo, false);
                    FreeTemporary(this.temporaryReturnSlotInfo);
                }
                PutILInstruction(ILCODE.RET);
                CloseDebugInfo();
            }
            else if (this.temporaryReturnSlotInfo != null)
            {
                FreeTemporary(this.temporaryReturnSlotInfo);
            }

            EndBB(ILCODE.stop, null, ILCODE.ILLEGAL);
            this.currentBBlock.Next = null;

#if DEBUG
            sb.Length = 0;
            ReflectionUtil.DebugCodeList(sb, this.firstBBlock);   // inlineBBlock
#endif

            //--------------------------------------------------------
            // ILGenerator.BeingCatchBlock and similar methods append LEAVE instructions.
            //--------------------------------------------------------
            for (BBLOCK bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
#if false
                BBLOCK nextbblock = bblock.Next;
                if (nextbblock != null &&
                    (nextbblock.StartsCatch || nextbblock.StartsFinally))
                {
                    if (bblock.ExitIL == ILCODE.LEAVE)
                    {
                        bblock.ExitIL = ILCODE.next;
                        //bblock.JumpDestBBlock = null;
                        bblock.CurrentLength -= 5;
                    }
                }
#endif
                if (bblock.StartsCatch)
                {
                    // LEAVE <4 bytes address>
                    bblock.CurrentLength += 5;
                }
                else if (bblock.StartsFinally)
                {
                    // LEAVE <4 bytes address>
                    bblock.CurrentLength += 5;
                }

                if (bblock.EndsException)
                {
                    if (bblock.ExitIL == ILCODE.ENDFINALLY)
                    {
                        // ENDFINALLY
                        //bblock.CurrentLength += 1;    // One ENDFINALLY will be removed.
                    }
                    else
                    {
#if false
                        if (bblock.ExitIL == ILCODE.LEAVE)
                        {
                            bblock.ExitIL = ILCODE.next;
                            //bblock.JumpDestBBlock = null;
                        }
                        else
                        {
                            // LEAVE <4 bytes address>
                            bblock.CurrentLength += 5;
                        }
#else
                        bblock.CurrentLength += 5;
#endif
                    }
                }
            }

#if DEBUG
            sb.Length = 0;
            ReflectionUtil.DebugCodeList(sb, this.firstBBlock);   // inlineBBlock
#endif
            OptimizeGotos();

#if DEBUG
            sb.Length = 0;
            ReflectionUtil.DebugCodeList(sb, this.firstBBlock);   // inlineBBlock
#endif

            int currentOffset = 0;
            BBLOCK curBB = this.firstBBlock;

            do
            {
                if (curBB.TryNestingCount == -1)
                {
                    currentOffset += ILcodesSize[(int)ILCODE.NOP];
                }
                curBB.StartOffset = currentOffset;
                curBB.LargeJump = false;
                currentOffset += curBB.CurrentLength;

                switch (curBB.ExitIL)
                {
                    case ILCODE.RET:
                    case ILCODE.stop:
                    case ILCODE.ENDFINALLY:
                    case ILCODE.next:
                        break;

                    case ILCODE.SWITCH:
                        currentOffset += ComputeSwitchSize(curBB);
                        break;

                    case ILCODE.LEAVE:
                        if (curBB.JumpDestBBlock.TryNestingCount == -1)
                        {
                            curBB.LeaveNestingCount = -1;
                        }
                        // fallthrough
                        goto default;

                    default:
                        DebugUtil.Assert(curBB.ExitIL < ILCODE.last);
                        curBB.LargeJump = true; // 1;
                        // assume a large offset...
                        //currentOffset += FetchAtIndex(ILcodesSize, curBB.ExitIL) + 4;
                        currentOffset += ILcodesSize[(int)curBB.ExitIL] + 4;
                        break;
                }
                curBB = curBB.Next;
            } while (curBB != null);

            int delta;
            do
            {
                curBB = this.firstBBlock;
                delta = 0;
                do
                {
                    curBB.StartOffset -= delta;
                    if (curBB.LargeJump)
                    {
                        int noopSpace = (curBB.LeaveNestingCount == -1) ? ILcodesSize[(int)ILCODE.NOP] : 0;
                        int offset;
                        ILCODE newOpcode = GetShortOpcode(curBB.ExitIL);
                        if (curBB.JumpDestBBlock.StartOffset > curBB.StartOffset)
                        {
                            // forward jump
                            offset = (int)(
                                curBB.JumpDestBBlock.StartOffset
                                - curBB.StartOffset
                                - delta
                                - ILcodesSize[(int)curBB.ExitIL]
                                - 4
                                - curBB.CurrentLength - noopSpace);
                        }
                        else
                        {
                            // backward jump
                            offset = (int)(
                                curBB.JumpDestBBlock.StartOffset
                                - (curBB.StartOffset + curBB.CurrentLength + ILcodesSize[(int)newOpcode] + 1 + noopSpace));

                        }
                        int len = (offset > 0 ? offset : -offset);
                        if (len == (int)((uint)len & 0x07F))
                        {
                            // this fits!!! 
                            delta += (ILcodesSize[(int)curBB.ExitIL] - ILcodesSize[(int)newOpcode]) + 3;
                            curBB.ExitIL = newOpcode;
                            curBB.LargeJump = false;
                        }
                    }
                    curBB = curBB.Next;
                } while (curBB != null);
                currentOffset -= delta;
            } while (delta != 0);

#if DEBUG
            if (COMPILER.GetRegDWORD("After") != 0)
            {
                DumpAllBlocksContent("all done in get final code size");
            }

            sb.Length = 0;
            ReflectionUtil.DebugCodeList(sb, this.firstBBlock);   // inlineBBlock
#endif
            DumpAllBlocks("Final");

            return currentOffset;
        }

        //------------------------------------------------------------
        // ILGENREC.ComputeSwitchSize
        //
        /// <summary>
        /// <para>Not implemented.</para>
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int ComputeSwitchSize(BBLOCK block)
        {
            DebugUtil.Assert(block.ExitIL == ILCODE.SWITCH);
            return ILcodesSize[(int)ILCODE.SWITCH] + sizeof(uint) + (sizeof(int) * block.SwitchDest.Count);
        }

        //------------------------------------------------------------
        // ILGENREC.
        //------------------------------------------------------------
        //private BYTE * copySwitchInstruction(BYTE * outBuffer, BBLOCK * block);
        //private BYTE * copyCode(BYTE * outBuffer);
        //private int    computeJumpOffset(BBLOCK * from, BBLOCK * to, unsigned instrSize);
        //private void copyHandlers(COR_ILMETHOD_SECT_EH_CLAUSE_FAT* clauses);

        //------------------------------------------------------------
        // ILGENREC.GetShortOpcode
        //
        /// <summary>
        /// return the short ilcode form for a given jump instruction.
        /// </summary>
        /// <param name="longOpcode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private ILCODE GetShortOpcode(ILCODE longOpcode)
        {
            switch (longOpcode)
            {
                case ILCODE.BEQ:
                    longOpcode = ILCODE.BEQ_S;
                    break;

                case ILCODE.BGE:
                    longOpcode = ILCODE.BGE_S;
                    break;

                case ILCODE.BGT:
                    longOpcode = ILCODE.BGT_S;
                    break;

                case ILCODE.BLE:
                    longOpcode = ILCODE.BLE_S;
                    break;

                case ILCODE.BLT:
                    longOpcode = ILCODE.BLT_S;
                    break;

                case ILCODE.BGE_UN:
                    longOpcode = ILCODE.BGE_UN_S;
                    break;

                case ILCODE.BGT_UN:
                    longOpcode = ILCODE.BGT_UN_S;
                    break;

                case ILCODE.BLE_UN:
                    longOpcode = ILCODE.BLE_UN_S;
                    break;

                case ILCODE.BLT_UN:
                    longOpcode = ILCODE.BLT_UN_S;
                    break;

                case ILCODE.BNE_UN:
                    longOpcode = ILCODE.BNE_UN_S;
                    break;

                case ILCODE.BRTRUE:
                    longOpcode = ILCODE.BRTRUE_S;
                    break;

                case ILCODE.BRFALSE:
                    longOpcode = ILCODE.BRFALSE_S;
                    break;

                case ILCODE.BR:
                    longOpcode = ILCODE.BR_S;
                    break;

                case ILCODE.LEAVE:
                    longOpcode = ILCODE.LEAVE_S;
                    break;

                default:
                    DebugUtil.Assert(false, "bad jump opcode");
                    return ILCODE.ILLEGAL;
            }

            return longOpcode;
        }

        //------------------------------------------------------------
        // ILGENREC.FREETEMP
        //
        //
        /// <summary></summary>
        /// <remarks>
        /// <code>
        /// #define FREETEMP(temp) \
        /// if (temp) { \
        ///     freeTemporary(temp); \
        ///     (temp) = NULL; \
        /// }
        /// </code>
        /// </remarks>
        /// <param name="temp"></param>
        //------------------------------------------------------------
        private void FREETEMP(ref LOCSLOTINFO temp)
        {
            if (temp != null)
            {
                FreeTemporary(temp);
                temp = null;
            }
        }

        //------------------------------------------------------------
        // ILGENREC.STOREINTEMP
        //
        /// <summary>
        /// This is a macro instead of a function so that we can capture at what line it
        /// </summary>
        /// <remarks>
        /// <code>
        /// #define STOREINTEMP(type, kind) \
        ///     (storeLocal(allocTemporary(type, kind)))
        /// </code>
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="kind"></param>
        //------------------------------------------------------------
        private LOCSLOTINFO STOREINTEMP(TYPESYM type, TEMP_KIND kind)
        {
            return StoreLocal(AllocTemporary(type, kind));
        }

        //------------------------------------------------------------
        // ILGENREC.EmitMethodBody (0)
        //
        /// <summary></summary>
        /// <param name="?"></param>
        /// <param name="codeList"></param>
        //------------------------------------------------------------
        internal void EmitMethodBody(METHSYM sym, BBLOCK firstBBlock)
        {
            if (sym.IsCtor)
            {
                EmitMethodBody(sym.ConstructorBuilder, firstBBlock);
            }
            else
            {
                EmitMethodBody(sym.MethodBuilder, firstBBlock);
            }
        }

        //------------------------------------------------------------
        // ILGENREC.EmitMethodBody (1)
        //
        /// <summary></summary>
        /// <param name="?"></param>
        /// <param name="codeList"></param>
        //------------------------------------------------------------
        internal void EmitMethodBody(MethodBuilder builder, BBLOCK firstBBlock)
        {
            DebugUtil.Assert(builder != null);
            ILGenerator gen = builder.GetILGenerator();
            EmitMethodBody(gen, firstBBlock);
        }

        //------------------------------------------------------------
        // ILGENREC.EmitMethodBody (2)
        //
        /// <summary></summary>
        /// <param name="?"></param>
        /// <param name="codeList"></param>
        //------------------------------------------------------------
        internal void EmitMethodBody(ConstructorBuilder builder, BBLOCK firstBBlock)
        {
            DebugUtil.Assert(builder != null);
            ILGenerator gen = builder.GetILGenerator();
            EmitMethodBody(gen, firstBBlock);
        }

        //------------------------------------------------------------
        // ILGENREC.EmitMethodBody (3)
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="firstBBlock"></param>
        //------------------------------------------------------------
        internal void EmitMethodBody(ILGenerator gen, BBLOCK firstBBlock)
        {
            Dictionary<int, BBLOCK> bblockDic = new Dictionary<int, BBLOCK>();
            Dictionary<int, Label> labelDic = new Dictionary<int, Label>();
            List<int> destList = new List<int>();
#if DEBUG
            StringBuilder sb1 = new StringBuilder();
#endif

            //--------------------------------------------------------
            // Enumerate all destinations.
            //--------------------------------------------------------
            for (BBLOCK bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                bblockDic.Add(bblock.BBlockID, bblock);
                if (bblock.JumpDestBBlock != null)
                {
                    destList.Add(bblock.JumpDestBBlock.BBlockID);
                }
                if (bblock.SwitchDest != null)
                {
                    foreach (SWITCHDESTGOTO sdg in bblock.SwitchDest.BBlockList)
                    {
                        destList.Add(sdg.DestBBlock.BBlockID);
                    }
                }
            }

            foreach (int i in destList)
            {
                BBLOCK block;
                if (bblockDic.TryGetValue(i, out block))
                {
                    block.IsDestination = true;
                }
                else
                {
                    DebugUtil.Assert(false);
                }
            }

            //--------------------------------------------------------
            // Emit codes.
            //--------------------------------------------------------
            for (BBLOCK bblock = this.firstBBlock; bblock != null; bblock = bblock.Next)
            {
                //----------------------------------------------------
                // If this bblock is a br destination, define label and mark here.
                //----------------------------------------------------
                if (bblock.IsDestination)
                {
                    Label lab;
                    if (!labelDic.TryGetValue(bblock.BBlockID, out lab))
                    {
                        lab = gen.DefineLabel();
                        labelDic.Add(bblock.BBlockID, lab);
                    }

                    try
                    {
                        gen.MarkLabel(lab);
                    }
                    catch (ArgumentException)
                    {
                        DebugUtil.Assert(false);
                    }
#if DEBUG
                    sb1.AppendFormat("<MarkLabel> {0}:{1}, {2}\n", lab, lab.GetHashCode(), bblock.BBlockID);
#endif
                }

                //----------------------------------------------------
                // Strat try, catch or finally.
                //----------------------------------------------------
                if (bblock.StartsException)
                {
                    gen.BeginExceptionBlock();
#if DEBUG
                    sb1.AppendFormat("<BeginException>\n");
#endif
                }
                else if (bblock.StartsCatch)
                {
                    Type catchType = null;
                    if (bblock.catchTypeSym != null)
                    {
                        catchType = SymUtil.GetSystemTypeFromSym(
                            bblock.catchTypeSym,
                            this.methodSym.ClassSym,
                            this.methodSym);
                    }
                    DebugUtil.Assert(catchType != null);
                    gen.BeginCatchBlock(catchType);
#if DEBUG
                    sb1.AppendFormat("<BeginCatch> {0}\n",
                        catchType != null ? catchType.Name : "(unknown type)");
#endif
                }
                else if (bblock.StartsFinally)
                {
                    gen.BeginFinallyBlock();
#if DEBUG
                    sb1.AppendFormat("<BeginFinally>\n");
#endif
                }

                //----------------------------------------------------
                // Emit codes.
                //----------------------------------------------------
                if (!bblock.IsNOP)
                {
                    List<ILInstruction> codeList = bblock.CodeList;
                    for (int i = 0; i < codeList.Count; ++i)
                    {
                        codeList[i].Emit(gen);
#if DEBUG
                        codeList[i].Debug(sb1);
#endif
                    }
                }

                //----------------------------------------------------
                // if the jump destination is specified, generate codes to jump.
                //----------------------------------------------------
                if (bblock.JumpDestBBlock != null)
                {
                    Label lab;
                    if (!labelDic.TryGetValue(bblock.JumpDestBBlock.BBlockID, out lab))
                    {
                        lab = gen.DefineLabel();
                        labelDic.Add(bblock.JumpDestBBlock.BBlockID, lab);
                    }

                    OpCode opCode;
                    if (ILGENREC.ILCodeToOpCode(bblock.ExitIL, out opCode))
                    {
                        gen.Emit(opCode, lab);
#if DEBUG
                        sb1.AppendFormat("{0} {1}:{2}, {3}\n",
                            opCode, lab, lab.GetHashCode(), bblock.JumpDestBBlock.BBlockID);
#endif
                    }
                }

                if (bblock.ExitIL == ILCODE.SWITCH)
                {
                    DebugUtil.Assert(bblock.SwitchDest != null);
                    List<SWITCHDESTGOTO> gotoList = bblock.SwitchDest.BBlockList;
                    DebugUtil.Assert(gotoList != null && gotoList.Count > 0);
                    Label[] destLabelArray = new Label[gotoList.Count];

                    for (int i = 0; i < gotoList.Count; ++i)
                    {
                        SWITCHDESTGOTO gt = gotoList[i];
                        int blockID = gt.DestBBlock.BBlockID;
                        Label lab;
                        if (!labelDic.TryGetValue(blockID, out lab))
                        {
                            lab = gen.DefineLabel();
                            labelDic.Add(blockID, lab);
                        }
                        destLabelArray[i] = lab;
                    }


                    OpCode opCode;
                    if (ILGENREC.ILCodeToOpCode(bblock.ExitIL, out opCode))
                    {
                        gen.Emit(OpCodes.Switch, destLabelArray);
#if DEBUG
                        StringBuilder swtSb = new StringBuilder();
                        foreach (SWITCHDESTGOTO gt in bblock.SwitchDest.BBlockList)
                        {
                            if (swtSb.Length > 0)
                            {
                                swtSb.Append(", ");
                            }
                            swtSb.AppendFormat("No.{0}", gt.DestBBlock.BBlockID);
                        }
                        sb1.AppendFormat("{0} :{1}\n", OpCodes.Switch, swtSb.ToString());
#endif
                    }
                }

                if (bblock.EndsException)
                {
                    gen.EndExceptionBlock();
#if DEBUG
                    sb1.AppendFormat("<EndException>\n");
#endif
                }
            }
#if DEBUG
            string dbstr = sb1.ToString();
#endif
        }

        //------------------------------------------------------------
        // ILGENREC.EnsureRet
        //
        /// <summary>
        /// <para>Ensure that the last code is ILCODE.RET.</para>
        /// </summary>
        /// <param name="codeList"></param>
        //------------------------------------------------------------
        internal void EnsureRet(List<ILInstruction> codeList)
        {
            int count = codeList.Count;
            if (count > 0)
            {
                if (codeList[count - 1].Code == ILCODE.RET)
                {
                    return;
                }
            }
            codeList.Add(new ILInstruction(ILCODE.RET));
        }

        //------------------------------------------------------------
        // ILGENREC.ILCodeToOpCode
        //
        /// <summary>
        /// Return the OpCode instance corresponding to a given ILCODE.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="emitOpCode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool ILCodeToOpCode(
            ILCODE code,
            out OpCode emitOpCode)
        {
            switch (code)
            {
                case ILCODE.NOP:
                    emitOpCode = OpCodes.Nop;
                    return true;
                case ILCODE.BREAK:
                    emitOpCode = OpCodes.Break;
                    return true;
                case ILCODE.LDARG_0:
                    emitOpCode = OpCodes.Ldarg_0;
                    return true;
                case ILCODE.LDARG_1:
                    emitOpCode = OpCodes.Ldarg_1;
                    return true;
                case ILCODE.LDARG_2:
                    emitOpCode = OpCodes.Ldarg_2;
                    return true;
                case ILCODE.LDARG_3:
                    emitOpCode = OpCodes.Ldarg_3;
                    return true;
                case ILCODE.LDLOC_0:
                    emitOpCode = OpCodes.Ldloc_0;
                    return true;
                case ILCODE.LDLOC_1:
                    emitOpCode = OpCodes.Ldloc_1;
                    return true;
                case ILCODE.LDLOC_2:
                    emitOpCode = OpCodes.Ldloc_2;
                    return true;
                case ILCODE.LDLOC_3:
                    emitOpCode = OpCodes.Ldloc_3;
                    return true;
                case ILCODE.STLOC_0:
                    emitOpCode = OpCodes.Stloc_0;
                    return true;
                case ILCODE.STLOC_1:
                    emitOpCode = OpCodes.Stloc_1;
                    return true;
                case ILCODE.STLOC_2:
                    emitOpCode = OpCodes.Stloc_2;
                    return true;
                case ILCODE.STLOC_3:
                    emitOpCode = OpCodes.Stloc_3;
                    return true;
                case ILCODE.LDARG_S:
                    emitOpCode = OpCodes.Ldarg_S;
                    return true;
                case ILCODE.LDARGA_S:
                    emitOpCode = OpCodes.Ldarga_S;
                    return true;
                case ILCODE.STARG_S:
                    emitOpCode = OpCodes.Starg_S;
                    return true;
                case ILCODE.LDLOC_S:
                    emitOpCode = OpCodes.Ldloc_S;
                    return true;
                case ILCODE.LDLOCA_S:
                    emitOpCode = OpCodes.Ldloca_S;
                    return true;
                case ILCODE.STLOC_S:
                    emitOpCode = OpCodes.Stloc_S;
                    return true;
                case ILCODE.LDNULL:
                    emitOpCode = OpCodes.Ldnull;
                    return true;
                case ILCODE.LDC_I4_M1:
                    emitOpCode = OpCodes.Ldc_I4_M1;
                    return true;
                case ILCODE.LDC_I4_0:
                    emitOpCode = OpCodes.Ldc_I4_0;
                    return true;
                case ILCODE.LDC_I4_1:
                    emitOpCode = OpCodes.Ldc_I4_1;
                    return true;
                case ILCODE.LDC_I4_2:
                    emitOpCode = OpCodes.Ldc_I4_2;
                    return true;
                case ILCODE.LDC_I4_3:
                    emitOpCode = OpCodes.Ldc_I4_3;
                    return true;
                case ILCODE.LDC_I4_4:
                    emitOpCode = OpCodes.Ldc_I4_4;
                    return true;
                case ILCODE.LDC_I4_5:
                    emitOpCode = OpCodes.Ldc_I4_5;
                    return true;
                case ILCODE.LDC_I4_6:
                    emitOpCode = OpCodes.Ldc_I4_6;
                    return true;
                case ILCODE.LDC_I4_7:
                    emitOpCode = OpCodes.Ldc_I4_7;
                    return true;
                case ILCODE.LDC_I4_8:
                    emitOpCode = OpCodes.Ldc_I4_8;
                    return true;
                case ILCODE.LDC_I4_S:
                    emitOpCode = OpCodes.Ldc_I4_S;
                    return true;
                case ILCODE.LDC_I4:
                    emitOpCode = OpCodes.Ldc_I4;
                    return true;
                case ILCODE.LDC_I8:
                    emitOpCode = OpCodes.Ldc_I8;
                    return true;
                case ILCODE.LDC_R4:
                    emitOpCode = OpCodes.Ldc_R4;
                    return true;
                case ILCODE.LDC_R8:
                    emitOpCode = OpCodes.Ldc_R8;
                    return true;
                case ILCODE.UNUSED49:
                    break;
                case ILCODE.DUP:
                    emitOpCode = OpCodes.Dup;
                    return true;
                case ILCODE.POP:
                    emitOpCode = OpCodes.Pop;
                    return true;
                case ILCODE.JMP:
                    emitOpCode = OpCodes.Jmp;
                    return true;
                case ILCODE.CALL:
                    emitOpCode = OpCodes.Call;
                    return true;
                case ILCODE.CALLI:
                    emitOpCode = OpCodes.Calli;
                    return true;
                case ILCODE.RET:
                    emitOpCode = OpCodes.Ret;
                    return true;
                case ILCODE.BR_S:
                    emitOpCode = OpCodes.Br_S;
                    return true;
                case ILCODE.BRFALSE_S:
                    emitOpCode = OpCodes.Brfalse_S;
                    return true;
                case ILCODE.BRTRUE_S:
                    emitOpCode = OpCodes.Brtrue_S;
                    return true;
                case ILCODE.BEQ_S:
                    emitOpCode = OpCodes.Beq_S;
                    return true;
                case ILCODE.BGE_S:
                    emitOpCode = OpCodes.Bge_S;
                    return true;
                case ILCODE.BGT_S:
                    emitOpCode = OpCodes.Bgt_S;
                    return true;
                case ILCODE.BLE_S:
                    emitOpCode = OpCodes.Ble_S;
                    return true;
                case ILCODE.BLT_S:
                    emitOpCode = OpCodes.Blt_S;
                    return true;
                case ILCODE.BNE_UN_S:
                    emitOpCode = OpCodes.Bne_Un_S;
                    return true;
                case ILCODE.BGE_UN_S:
                    emitOpCode = OpCodes.Bge_Un_S;
                    return true;
                case ILCODE.BGT_UN_S:
                    emitOpCode = OpCodes.Bgt_Un_S;
                    return true;
                case ILCODE.BLE_UN_S:
                    emitOpCode = OpCodes.Ble_Un_S;
                    return true;
                case ILCODE.BLT_UN_S:
                    emitOpCode = OpCodes.Blt_Un_S;
                    return true;
                case ILCODE.BR:
                    emitOpCode = OpCodes.Br;
                    return true;
                case ILCODE.BRFALSE:
                    emitOpCode = OpCodes.Brfalse;
                    return true;
                case ILCODE.BRTRUE:
                    emitOpCode = OpCodes.Brtrue;
                    return true;
                case ILCODE.BEQ:
                    emitOpCode = OpCodes.Beq;
                    return true;
                case ILCODE.BGE:
                    emitOpCode = OpCodes.Bge;
                    return true;
                case ILCODE.BGT:
                    emitOpCode = OpCodes.Bgt;
                    return true;
                case ILCODE.BLE:
                    emitOpCode = OpCodes.Ble;
                    return true;
                case ILCODE.BLT:
                    emitOpCode = OpCodes.Blt;
                    return true;
                case ILCODE.BNE_UN:
                    emitOpCode = OpCodes.Bne_Un;
                    return true;
                case ILCODE.BGE_UN:
                    emitOpCode = OpCodes.Bge_Un;
                    return true;
                case ILCODE.BGT_UN:
                    emitOpCode = OpCodes.Bgt_Un;
                    return true;
                case ILCODE.BLE_UN:
                    emitOpCode = OpCodes.Ble_Un;
                    return true;
                case ILCODE.BLT_UN:
                    emitOpCode = OpCodes.Blt_Un;
                    return true;
                case ILCODE.SWITCH:
                    emitOpCode = OpCodes.Switch;
                    return true;
                case ILCODE.LDIND_I1:
                    emitOpCode = OpCodes.Ldind_I1;
                    return true;
                case ILCODE.LDIND_U1:
                    emitOpCode = OpCodes.Ldind_U1;
                    return true;
                case ILCODE.LDIND_I2:
                    emitOpCode = OpCodes.Ldind_I2;
                    return true;
                case ILCODE.LDIND_U2:
                    emitOpCode = OpCodes.Ldind_U2;
                    return true;
                case ILCODE.LDIND_I4:
                    emitOpCode = OpCodes.Ldind_I4;
                    return true;
                case ILCODE.LDIND_U4:
                    emitOpCode = OpCodes.Ldind_U4;
                    return true;
                case ILCODE.LDIND_I8:
                    emitOpCode = OpCodes.Ldind_I8;
                    return true;
                case ILCODE.LDIND_I:
                    emitOpCode = OpCodes.Ldind_I;
                    return true;
                case ILCODE.LDIND_R4:
                    emitOpCode = OpCodes.Ldind_R4;
                    return true;
                case ILCODE.LDIND_R8:
                    emitOpCode = OpCodes.Ldind_R8;
                    return true;
                case ILCODE.LDIND_REF:
                    emitOpCode = OpCodes.Ldind_Ref;
                    return true;
                case ILCODE.STIND_REF:
                    emitOpCode = OpCodes.Stind_Ref;
                    return true;
                case ILCODE.STIND_I1:
                    emitOpCode = OpCodes.Stind_I1;
                    return true;
                case ILCODE.STIND_I2:
                    emitOpCode = OpCodes.Stind_I2;
                    return true;
                case ILCODE.STIND_I4:
                    emitOpCode = OpCodes.Stind_I4;
                    return true;
                case ILCODE.STIND_I8:
                    emitOpCode = OpCodes.Stind_I8;
                    return true;
                case ILCODE.STIND_R4:
                    emitOpCode = OpCodes.Stind_R4;
                    return true;
                case ILCODE.STIND_R8:
                    emitOpCode = OpCodes.Stind_R8;
                    return true;
                case ILCODE.ADD:
                    emitOpCode = OpCodes.Add;
                    return true;
                case ILCODE.SUB:
                    emitOpCode = OpCodes.Sub;
                    return true;
                case ILCODE.MUL:
                    emitOpCode = OpCodes.Mul;
                    return true;
                case ILCODE.DIV:
                    emitOpCode = OpCodes.Div;
                    return true;
                case ILCODE.DIV_UN:
                    emitOpCode = OpCodes.Div_Un;
                    return true;
                case ILCODE.REM:
                    emitOpCode = OpCodes.Rem;
                    return true;
                case ILCODE.REM_UN:
                    emitOpCode = OpCodes.Rem_Un;
                    return true;
                case ILCODE.AND:
                    emitOpCode = OpCodes.And;
                    return true;
                case ILCODE.OR:
                    emitOpCode = OpCodes.Or;
                    return true;
                case ILCODE.XOR:
                    emitOpCode = OpCodes.Xor;
                    return true;
                case ILCODE.SHL:
                    emitOpCode = OpCodes.Shl;
                    return true;
                case ILCODE.SHR:
                    emitOpCode = OpCodes.Shr;
                    return true;
                case ILCODE.SHR_UN:
                    emitOpCode = OpCodes.Shr_Un;
                    return true;
                case ILCODE.NEG:
                    emitOpCode = OpCodes.Neg;
                    return true;
                case ILCODE.NOT:
                    emitOpCode = OpCodes.Not;
                    return true;
                case ILCODE.CONV_I1:
                    emitOpCode = OpCodes.Conv_I1;
                    return true;
                case ILCODE.CONV_I2:
                    emitOpCode = OpCodes.Conv_I2;
                    return true;
                case ILCODE.CONV_I4:
                    emitOpCode = OpCodes.Conv_I4;
                    return true;
                case ILCODE.CONV_I8:
                    emitOpCode = OpCodes.Conv_I8;
                    return true;
                case ILCODE.CONV_R4:
                    emitOpCode = OpCodes.Conv_R4;
                    return true;
                case ILCODE.CONV_R8:
                    emitOpCode = OpCodes.Conv_R8;
                    return true;
                case ILCODE.CONV_U4:
                    emitOpCode = OpCodes.Conv_U4;
                    return true;
                case ILCODE.CONV_U8:
                    emitOpCode = OpCodes.Conv_U8;
                    return true;
                case ILCODE.CALLVIRT:
                    emitOpCode = OpCodes.Callvirt;
                    return true;
                case ILCODE.CPOBJ:
                    emitOpCode = OpCodes.Cpobj;
                    return true;
                case ILCODE.LDOBJ:
                    emitOpCode = OpCodes.Ldobj;
                    return true;
                case ILCODE.LDSTR:
                    emitOpCode = OpCodes.Ldstr;
                    return true;
                case ILCODE.NEWOBJ:
                    emitOpCode = OpCodes.Newobj;
                    return true;
                case ILCODE.CASTCLASS:
                    emitOpCode = OpCodes.Castclass;
                    return true;
                case ILCODE.ISINST:
                    emitOpCode = OpCodes.Isinst;
                    return true;
                case ILCODE.CONV_R_UN:
                    emitOpCode = OpCodes.Conv_R_Un;
                    return true;
                case ILCODE.UNUSED58:
                    break;
                case ILCODE.UNUSED1:
                    break;
                case ILCODE.UNBOX:
                    emitOpCode = OpCodes.Unbox;
                    return true;
                case ILCODE.THROW:
                    emitOpCode = OpCodes.Throw;
                    return true;
                case ILCODE.LDFLD:
                    emitOpCode = OpCodes.Ldfld;
                    return true;
                case ILCODE.LDFLDA:
                    emitOpCode = OpCodes.Ldflda;
                    return true;
                case ILCODE.STFLD:
                    emitOpCode = OpCodes.Stfld;
                    return true;
                case ILCODE.LDSFLD:
                    emitOpCode = OpCodes.Ldsfld;
                    return true;
                case ILCODE.LDSFLDA:
                    emitOpCode = OpCodes.Ldsflda;
                    return true;
                case ILCODE.STSFLD:
                    emitOpCode = OpCodes.Stsfld;
                    return true;
                case ILCODE.STOBJ:
                    emitOpCode = OpCodes.Stobj;
                    return true;
                case ILCODE.CONV_OVF_I1_UN:
                    emitOpCode = OpCodes.Conv_Ovf_I1_Un;
                    return true;
                case ILCODE.CONV_OVF_I2_UN:
                    emitOpCode = OpCodes.Conv_Ovf_I2_Un;
                    return true;
                case ILCODE.CONV_OVF_I4_UN:
                    emitOpCode = OpCodes.Conv_Ovf_I4_Un;
                    return true;
                case ILCODE.CONV_OVF_I8_UN:
                    emitOpCode = OpCodes.Conv_Ovf_I8_Un;
                    return true;
                case ILCODE.CONV_OVF_U1_UN:
                    emitOpCode = OpCodes.Conv_Ovf_U1_Un;
                    return true;
                case ILCODE.CONV_OVF_U2_UN:
                    emitOpCode = OpCodes.Conv_Ovf_U2_Un;
                    return true;
                case ILCODE.CONV_OVF_U4_UN:
                    emitOpCode = OpCodes.Conv_Ovf_U4_Un;
                    return true;
                case ILCODE.CONV_OVF_U8_UN:
                    emitOpCode = OpCodes.Conv_Ovf_U8_Un;
                    return true;
                case ILCODE.CONV_OVF_I_UN:
                    emitOpCode = OpCodes.Conv_Ovf_I_Un;
                    return true;
                case ILCODE.CONV_OVF_U_UN:
                    emitOpCode = OpCodes.Conv_Ovf_U_Un;
                    return true;
                case ILCODE.BOX:
                    emitOpCode = OpCodes.Box;
                    return true;
                case ILCODE.NEWARR:
                    emitOpCode = OpCodes.Newarr;
                    return true;
                case ILCODE.LDLEN:
                    emitOpCode = OpCodes.Ldlen;
                    return true;
                case ILCODE.LDELEMA:
                    emitOpCode = OpCodes.Ldelema;
                    return true;
                case ILCODE.LDELEM_I1:
                    emitOpCode = OpCodes.Ldelem_I1;
                    return true;
                case ILCODE.LDELEM_U1:
                    emitOpCode = OpCodes.Ldelem_U1;
                    return true;
                case ILCODE.LDELEM_I2:
                    emitOpCode = OpCodes.Ldelem_I2;
                    return true;
                case ILCODE.LDELEM_U2:
                    emitOpCode = OpCodes.Ldelem_U2;
                    return true;
                case ILCODE.LDELEM_I4:
                    emitOpCode = OpCodes.Ldelem_I4;
                    return true;
                case ILCODE.LDELEM_U4:
                    emitOpCode = OpCodes.Ldelem_U4;
                    return true;
                case ILCODE.LDELEM_I8:
                    emitOpCode = OpCodes.Ldelem_I8;
                    return true;
                case ILCODE.LDELEM_I:
                    emitOpCode = OpCodes.Ldelem_I;
                    return true;
                case ILCODE.LDELEM_R4:
                    emitOpCode = OpCodes.Ldelem_R4;
                    return true;
                case ILCODE.LDELEM_R8:
                    emitOpCode = OpCodes.Ldelem_R8;
                    return true;
                case ILCODE.LDELEM_REF:
                    emitOpCode = OpCodes.Ldelem_Ref;
                    return true;
                case ILCODE.STELEM_I:
                    emitOpCode = OpCodes.Stelem_I;
                    return true;
                case ILCODE.STELEM_I1:
                    emitOpCode = OpCodes.Stelem_I1;
                    return true;
                case ILCODE.STELEM_I2:
                    emitOpCode = OpCodes.Stelem_I2;
                    return true;
                case ILCODE.STELEM_I4:
                    emitOpCode = OpCodes.Stelem_I4;
                    return true;
                case ILCODE.STELEM_I8:
                    emitOpCode = OpCodes.Stelem_I8;
                    return true;
                case ILCODE.STELEM_R4:
                    emitOpCode = OpCodes.Stelem_R4;
                    return true;
                case ILCODE.STELEM_R8:
                    emitOpCode = OpCodes.Stelem_R8;
                    return true;
                case ILCODE.STELEM_REF:
                    emitOpCode = OpCodes.Stelem_Ref;
                    return true;
                case ILCODE.LDELEM:
                    emitOpCode = OpCodes.Ldelem;
                    return true;
                case ILCODE.STELEM:
                    emitOpCode = OpCodes.Stelem;
                    return true;
                case ILCODE.UNBOX_ANY:
                    emitOpCode = OpCodes.Unbox_Any;
                    return true;
                case ILCODE.UNUSED5:
                    break;
                case ILCODE.UNUSED6:
                    break;
                case ILCODE.UNUSED7:
                    break;
                case ILCODE.UNUSED8:
                    break;
                case ILCODE.UNUSED9:
                    break;
                case ILCODE.UNUSED10:
                    break;
                case ILCODE.UNUSED11:
                    break;
                case ILCODE.UNUSED12:
                    break;
                case ILCODE.UNUSED13:
                    break;
                case ILCODE.UNUSED14:
                    break;
                case ILCODE.UNUSED15:
                    break;
                case ILCODE.UNUSED16:
                    break;
                case ILCODE.UNUSED17:
                    break;
                case ILCODE.CONV_OVF_I1:
                    emitOpCode = OpCodes.Conv_Ovf_I1;
                    return true;
                case ILCODE.CONV_OVF_U1:
                    emitOpCode = OpCodes.Conv_Ovf_U1;
                    return true;
                case ILCODE.CONV_OVF_I2:
                    emitOpCode = OpCodes.Conv_Ovf_I2;
                    return true;
                case ILCODE.CONV_OVF_U2:
                    emitOpCode = OpCodes.Conv_Ovf_U2;
                    return true;
                case ILCODE.CONV_OVF_I4:
                    emitOpCode = OpCodes.Conv_Ovf_I4;
                    return true;
                case ILCODE.CONV_OVF_U4:
                    emitOpCode = OpCodes.Conv_Ovf_U4;
                    return true;
                case ILCODE.CONV_OVF_I8:
                    emitOpCode = OpCodes.Conv_Ovf_I8;
                    return true;
                case ILCODE.CONV_OVF_U8:
                    emitOpCode = OpCodes.Conv_Ovf_U8;
                    return true;
                case ILCODE.UNUSED50:
                    break;
                case ILCODE.UNUSED18:
                    break;
                case ILCODE.UNUSED19:
                    break;
                case ILCODE.UNUSED20:
                    break;
                case ILCODE.UNUSED21:
                    break;
                case ILCODE.UNUSED22:
                    break;
                case ILCODE.UNUSED23:
                    break;
                case ILCODE.REFANYVAL:
                    emitOpCode = OpCodes.Refanyval;
                    return true;
                case ILCODE.CKFINITE:
                    emitOpCode = OpCodes.Ckfinite;
                    return true;
                case ILCODE.UNUSED24:
                    break;
                case ILCODE.UNUSED25:
                    break;
                case ILCODE.MKREFANY:
                    emitOpCode = OpCodes.Mkrefany;
                    return true;
                case ILCODE.UNUSED59:
                    break;
                case ILCODE.UNUSED60:
                    break;
                case ILCODE.UNUSED61:
                    break;
                case ILCODE.UNUSED62:
                    break;
                case ILCODE.UNUSED63:
                    break;
                case ILCODE.UNUSED64:
                    break;
                case ILCODE.UNUSED65:
                    break;
                case ILCODE.UNUSED66:
                    break;
                case ILCODE.UNUSED67:
                    break;
                case ILCODE.LDTOKEN:
                    emitOpCode = OpCodes.Ldtoken;
                    return true;
                case ILCODE.CONV_U2:
                    emitOpCode = OpCodes.Conv_U2;
                    return true;
                case ILCODE.CONV_U1:
                    emitOpCode = OpCodes.Conv_U1;
                    return true;
                case ILCODE.CONV_I:
                    emitOpCode = OpCodes.Conv_I;
                    return true;
                case ILCODE.CONV_OVF_I:
                    emitOpCode = OpCodes.Conv_Ovf_I;
                    return true;
                case ILCODE.CONV_OVF_U:
                    emitOpCode = OpCodes.Conv_Ovf_U;
                    return true;
                case ILCODE.ADD_OVF:
                    emitOpCode = OpCodes.Add_Ovf;
                    return true;
                case ILCODE.ADD_OVF_UN:
                    emitOpCode = OpCodes.Add_Ovf_Un;
                    return true;
                case ILCODE.MUL_OVF:
                    emitOpCode = OpCodes.Mul_Ovf;
                    return true;
                case ILCODE.MUL_OVF_UN:
                    emitOpCode = OpCodes.Mul_Ovf_Un;
                    return true;
                case ILCODE.SUB_OVF:
                    emitOpCode = OpCodes.Sub_Ovf;
                    return true;
                case ILCODE.SUB_OVF_UN:
                    emitOpCode = OpCodes.Sub_Ovf_Un;
                    return true;
                case ILCODE.ENDFINALLY:
                    emitOpCode = OpCodes.Endfinally;
                    return true;
                case ILCODE.LEAVE:
                    emitOpCode = OpCodes.Leave;
                    return true;
                case ILCODE.LEAVE_S:
                    emitOpCode = OpCodes.Leave_S;
                    return true;
                case ILCODE.STIND_I:
                    emitOpCode = OpCodes.Stind_I;
                    return true;
                case ILCODE.CONV_U:
                    emitOpCode = OpCodes.Conv_U;
                    return true;
                case ILCODE.UNUSED26:
                    break;
                case ILCODE.UNUSED27:
                    break;
                case ILCODE.UNUSED28:
                    break;
                case ILCODE.UNUSED29:
                    break;
                case ILCODE.UNUSED30:
                    break;
                case ILCODE.UNUSED31:
                    break;
                case ILCODE.UNUSED32:
                    break;
                case ILCODE.UNUSED33:
                    break;
                case ILCODE.UNUSED34:
                    break;
                case ILCODE.UNUSED35:
                    break;
                case ILCODE.UNUSED36:
                    break;
                case ILCODE.UNUSED37:
                    break;
                case ILCODE.UNUSED38:
                    break;
                case ILCODE.UNUSED39:
                    break;
                case ILCODE.UNUSED40:
                    break;
                case ILCODE.UNUSED41:
                    break;
                case ILCODE.UNUSED42:
                    break;
                case ILCODE.UNUSED43:
                    break;
                case ILCODE.UNUSED44:
                    break;
                case ILCODE.UNUSED45:
                    break;
                case ILCODE.UNUSED46:
                    break;
                case ILCODE.UNUSED47:
                    break;
                case ILCODE.UNUSED48:
                    break;
                case ILCODE.PREFIX7:
                    emitOpCode = OpCodes.Prefix7;
                    return true;
                case ILCODE.PREFIX6:
                    emitOpCode = OpCodes.Prefix6;
                    return true;
                case ILCODE.PREFIX5:
                    emitOpCode = OpCodes.Prefix5;
                    return true;
                case ILCODE.PREFIX4:
                    emitOpCode = OpCodes.Prefix4;
                    return true;
                case ILCODE.PREFIX3:
                    emitOpCode = OpCodes.Prefix3;
                    return true;
                case ILCODE.PREFIX2:
                    emitOpCode = OpCodes.Prefix2;
                    return true;
                case ILCODE.PREFIX1:
                    emitOpCode = OpCodes.Prefix1;
                    return true;
                case ILCODE.PREFIXREF:
                    emitOpCode = OpCodes.Prefixref;
                    return true;
                case ILCODE.ARGLIST:
                    emitOpCode = OpCodes.Arglist;
                    return true;
                case ILCODE.CEQ:
                    emitOpCode = OpCodes.Ceq;
                    return true;
                case ILCODE.CGT:
                    emitOpCode = OpCodes.Cgt;
                    return true;
                case ILCODE.CGT_UN:
                    emitOpCode = OpCodes.Cgt_Un;
                    return true;
                case ILCODE.CLT:
                    emitOpCode = OpCodes.Clt;
                    return true;
                case ILCODE.CLT_UN:
                    emitOpCode = OpCodes.Clt_Un;
                    return true;
                case ILCODE.LDFTN:
                    emitOpCode = OpCodes.Ldftn;
                    return true;
                case ILCODE.LDVIRTFTN:
                    emitOpCode = OpCodes.Ldvirtftn;
                    return true;
                case ILCODE.UNUSED56:
                    break;
                case ILCODE.LDARG:
                    emitOpCode = OpCodes.Ldarg;
                    return true;
                case ILCODE.LDARGA:
                    emitOpCode = OpCodes.Ldarga;
                    return true;
                case ILCODE.STARG:
                    emitOpCode = OpCodes.Starg;
                    return true;
                case ILCODE.LDLOC:
                    emitOpCode = OpCodes.Ldloc;
                    return true;
                case ILCODE.LDLOCA:
                    emitOpCode = OpCodes.Ldloca;
                    return true;
                case ILCODE.STLOC:
                    emitOpCode = OpCodes.Stloc;
                    return true;
                case ILCODE.LOCALLOC:
                    emitOpCode = OpCodes.Localloc;
                    return true;
                case ILCODE.UNUSED57:
                    break;
                case ILCODE.ENDFILTER:
                    emitOpCode = OpCodes.Endfilter;
                    return true;
                case ILCODE.UNALIGNED:
                    emitOpCode = OpCodes.Unaligned;
                    return true;
                case ILCODE.VOLATILE:
                    emitOpCode = OpCodes.Volatile;
                    return true;
                case ILCODE.TAILCALL:
                    emitOpCode = OpCodes.Tailcall;
                    return true;
                case ILCODE.INITOBJ:
                    emitOpCode = OpCodes.Initobj;
                    return true;
                case ILCODE.CONSTRAINED:
                    emitOpCode = OpCodes.Constrained;
                    return true;
                case ILCODE.CPBLK:
                    emitOpCode = OpCodes.Cpblk;
                    return true;
                case ILCODE.INITBLK:
                    emitOpCode = OpCodes.Initblk;
                    return true;
                case ILCODE.UNUSED69:
                    break;
                case ILCODE.RETHROW:
                    emitOpCode = OpCodes.Rethrow;
                    return true;
                case ILCODE.UNUSED51:
                    break;
                case ILCODE.SIZEOF:
                    emitOpCode = OpCodes.Sizeof;
                    return true;
                case ILCODE.REFANYTYPE:
                    emitOpCode = OpCodes.Refanytype;
                    return true;
                case ILCODE.READONLY:
                    emitOpCode = OpCodes.Readonly;
                    return true;
                case ILCODE.UNUSED53:
                    break;
                case ILCODE.UNUSED54:
                    break;
                case ILCODE.UNUSED55:
                    break;
                case ILCODE.UNUSED70:
                    break;
                case ILCODE.ILLEGAL:
                    break;
                case ILCODE.MACRO_END:
                    break;
                case ILCODE.CODE_LABEL:
                    break;
                case ILCODE.last:
                    break;

                case ILCODE.next:
                    emitOpCode = OpCodes.Nop;
                    return false;

                default:
                    break;
            }
            // show error message.
            emitOpCode = OpCodes.Nop;
            return false;
        }

        //------------------------------------------------------------
        // ILGENREC.ILcodes
        //------------------------------------------------------------
        static private REFENCODING[] ILcodes =
		{
			new REFENCODING(0xFF, 0x00),
			new REFENCODING(0xFF, 0x01),
			new REFENCODING(0xFF, 0x02),
			new REFENCODING(0xFF, 0x03),
			new REFENCODING(0xFF, 0x04),
			new REFENCODING(0xFF, 0x05),
			new REFENCODING(0xFF, 0x06),
			new REFENCODING(0xFF, 0x07),
			new REFENCODING(0xFF, 0x08),
			new REFENCODING(0xFF, 0x09),
			new REFENCODING(0xFF, 0x0A),
			new REFENCODING(0xFF, 0x0B),
			new REFENCODING(0xFF, 0x0C),
			new REFENCODING(0xFF, 0x0D),
			new REFENCODING(0xFF, 0x0E),
			new REFENCODING(0xFF, 0x0F),
			new REFENCODING(0xFF, 0x10),
			new REFENCODING(0xFF, 0x11),
			new REFENCODING(0xFF, 0x12),
			new REFENCODING(0xFF, 0x13),
			new REFENCODING(0xFF, 0x14),
			new REFENCODING(0xFF, 0x15),
			new REFENCODING(0xFF, 0x16),
			new REFENCODING(0xFF, 0x17),
			new REFENCODING(0xFF, 0x18),
			new REFENCODING(0xFF, 0x19),
			new REFENCODING(0xFF, 0x1A),
			new REFENCODING(0xFF, 0x1B),
			new REFENCODING(0xFF, 0x1C),
			new REFENCODING(0xFF, 0x1D),
			new REFENCODING(0xFF, 0x1E),
			new REFENCODING(0xFF, 0x1F),
			new REFENCODING(0xFF, 0x20),
			new REFENCODING(0xFF, 0x21),
			new REFENCODING(0xFF, 0x22),
			new REFENCODING(0xFF, 0x23),
			new REFENCODING(0xFF, 0x24),
			new REFENCODING(0xFF, 0x25),
			new REFENCODING(0xFF, 0x26),
			new REFENCODING(0xFF, 0x27),
			new REFENCODING(0xFF, 0x28),
			new REFENCODING(0xFF, 0x29),
			new REFENCODING(0xFF, 0x2A),
			new REFENCODING(0xFF, 0x2B),
			new REFENCODING(0xFF, 0x2C),
			new REFENCODING(0xFF, 0x2D),
			new REFENCODING(0xFF, 0x2E),
			new REFENCODING(0xFF, 0x2F),
			new REFENCODING(0xFF, 0x30),
			new REFENCODING(0xFF, 0x31),
			new REFENCODING(0xFF, 0x32),
			new REFENCODING(0xFF, 0x33),
			new REFENCODING(0xFF, 0x34),
			new REFENCODING(0xFF, 0x35),
			new REFENCODING(0xFF, 0x36),
			new REFENCODING(0xFF, 0x37),
			new REFENCODING(0xFF, 0x38),
			new REFENCODING(0xFF, 0x39),
			new REFENCODING(0xFF, 0x3A),
			new REFENCODING(0xFF, 0x3B),
			new REFENCODING(0xFF, 0x3C),
			new REFENCODING(0xFF, 0x3D),
			new REFENCODING(0xFF, 0x3E),
			new REFENCODING(0xFF, 0x3F),
			new REFENCODING(0xFF, 0x40),
			new REFENCODING(0xFF, 0x41),
			new REFENCODING(0xFF, 0x42),
			new REFENCODING(0xFF, 0x43),
			new REFENCODING(0xFF, 0x44),
			new REFENCODING(0xFF, 0x45),
			new REFENCODING(0xFF, 0x46),
			new REFENCODING(0xFF, 0x47),
			new REFENCODING(0xFF, 0x48),
			new REFENCODING(0xFF, 0x49),
			new REFENCODING(0xFF, 0x4A),
			new REFENCODING(0xFF, 0x4B),
			new REFENCODING(0xFF, 0x4C),
			new REFENCODING(0xFF, 0x4D),
			new REFENCODING(0xFF, 0x4E),
			new REFENCODING(0xFF, 0x4F),
			new REFENCODING(0xFF, 0x50),
			new REFENCODING(0xFF, 0x51),
			new REFENCODING(0xFF, 0x52),
			new REFENCODING(0xFF, 0x53),
			new REFENCODING(0xFF, 0x54),
			new REFENCODING(0xFF, 0x55),
			new REFENCODING(0xFF, 0x56),
			new REFENCODING(0xFF, 0x57),
			new REFENCODING(0xFF, 0x58),
			new REFENCODING(0xFF, 0x59),
			new REFENCODING(0xFF, 0x5A),
			new REFENCODING(0xFF, 0x5B),
			new REFENCODING(0xFF, 0x5C),
			new REFENCODING(0xFF, 0x5D),
			new REFENCODING(0xFF, 0x5E),
			new REFENCODING(0xFF, 0x5F),
			new REFENCODING(0xFF, 0x60),
			new REFENCODING(0xFF, 0x61),
			new REFENCODING(0xFF, 0x62),
			new REFENCODING(0xFF, 0x63),
			new REFENCODING(0xFF, 0x64),
			new REFENCODING(0xFF, 0x65),
			new REFENCODING(0xFF, 0x66),
			new REFENCODING(0xFF, 0x67),
			new REFENCODING(0xFF, 0x68),
			new REFENCODING(0xFF, 0x69),
			new REFENCODING(0xFF, 0x6A),
			new REFENCODING(0xFF, 0x6B),
			new REFENCODING(0xFF, 0x6C),
			new REFENCODING(0xFF, 0x6D),
			new REFENCODING(0xFF, 0x6E),
			new REFENCODING(0xFF, 0x6F),
			new REFENCODING(0xFF, 0x70),
			new REFENCODING(0xFF, 0x71),
			new REFENCODING(0xFF, 0x72),
			new REFENCODING(0xFF, 0x73),
			new REFENCODING(0xFF, 0x74),
			new REFENCODING(0xFF, 0x75),
			new REFENCODING(0xFF, 0x76),
			new REFENCODING(0xFF, 0x77),
			new REFENCODING(0xFF, 0x78),
			new REFENCODING(0xFF, 0x79),
			new REFENCODING(0xFF, 0x7A),
			new REFENCODING(0xFF, 0x7B),
			new REFENCODING(0xFF, 0x7C),
			new REFENCODING(0xFF, 0x7D),
			new REFENCODING(0xFF, 0x7E),
			new REFENCODING(0xFF, 0x7F),
			new REFENCODING(0xFF, 0x80),
			new REFENCODING(0xFF, 0x81),
			new REFENCODING(0xFF, 0x82),
			new REFENCODING(0xFF, 0x83),
			new REFENCODING(0xFF, 0x84),
			new REFENCODING(0xFF, 0x85),
			new REFENCODING(0xFF, 0x86),
			new REFENCODING(0xFF, 0x87),
			new REFENCODING(0xFF, 0x88),
			new REFENCODING(0xFF, 0x89),
			new REFENCODING(0xFF, 0x8A),
			new REFENCODING(0xFF, 0x8B),
			new REFENCODING(0xFF, 0x8C),
			new REFENCODING(0xFF, 0x8D),
			new REFENCODING(0xFF, 0x8E),
			new REFENCODING(0xFF, 0x8F),
			new REFENCODING(0xFF, 0x90),
			new REFENCODING(0xFF, 0x91),
			new REFENCODING(0xFF, 0x92),
			new REFENCODING(0xFF, 0x93),
			new REFENCODING(0xFF, 0x94),
			new REFENCODING(0xFF, 0x95),
			new REFENCODING(0xFF, 0x96),
			new REFENCODING(0xFF, 0x97),
			new REFENCODING(0xFF, 0x98),
			new REFENCODING(0xFF, 0x99),
			new REFENCODING(0xFF, 0x9A),
			new REFENCODING(0xFF, 0x9B),
			new REFENCODING(0xFF, 0x9C),
			new REFENCODING(0xFF, 0x9D),
			new REFENCODING(0xFF, 0x9E),
			new REFENCODING(0xFF, 0x9F),
			new REFENCODING(0xFF, 0xA0),
			new REFENCODING(0xFF, 0xA1),
			new REFENCODING(0xFF, 0xA2),
			new REFENCODING(0xFF, 0xA3),
			new REFENCODING(0xFF, 0xA4),
			new REFENCODING(0xFF, 0xA5),
			new REFENCODING(0xFF, 0xA6),
			new REFENCODING(0xFF, 0xA7),
			new REFENCODING(0xFF, 0xA8),
			new REFENCODING(0xFF, 0xA9),
			new REFENCODING(0xFF, 0xAA),
			new REFENCODING(0xFF, 0xAB),
			new REFENCODING(0xFF, 0xAC),
			new REFENCODING(0xFF, 0xAD),
			new REFENCODING(0xFF, 0xAE),
			new REFENCODING(0xFF, 0xAF),
			new REFENCODING(0xFF, 0xB0),
			new REFENCODING(0xFF, 0xB1),
			new REFENCODING(0xFF, 0xB2),
			new REFENCODING(0xFF, 0xB3),
			new REFENCODING(0xFF, 0xB4),
			new REFENCODING(0xFF, 0xB5),
			new REFENCODING(0xFF, 0xB6),
			new REFENCODING(0xFF, 0xB7),
			new REFENCODING(0xFF, 0xB8),
			new REFENCODING(0xFF, 0xB9),
			new REFENCODING(0xFF, 0xBA),
			new REFENCODING(0xFF, 0xBB),
			new REFENCODING(0xFF, 0xBC),
			new REFENCODING(0xFF, 0xBD),
			new REFENCODING(0xFF, 0xBE),
			new REFENCODING(0xFF, 0xBF),
			new REFENCODING(0xFF, 0xC0),
			new REFENCODING(0xFF, 0xC1),
			new REFENCODING(0xFF, 0xC2),
			new REFENCODING(0xFF, 0xC3),
			new REFENCODING(0xFF, 0xC4),
			new REFENCODING(0xFF, 0xC5),
			new REFENCODING(0xFF, 0xC6),
			new REFENCODING(0xFF, 0xC7),
			new REFENCODING(0xFF, 0xC8),
			new REFENCODING(0xFF, 0xC9),
			new REFENCODING(0xFF, 0xCA),
			new REFENCODING(0xFF, 0xCB),
			new REFENCODING(0xFF, 0xCC),
			new REFENCODING(0xFF, 0xCD),
			new REFENCODING(0xFF, 0xCE),
			new REFENCODING(0xFF, 0xCF),
			new REFENCODING(0xFF, 0xD0),
			new REFENCODING(0xFF, 0xD1),
			new REFENCODING(0xFF, 0xD2),
			new REFENCODING(0xFF, 0xD3),
			new REFENCODING(0xFF, 0xD4),
			new REFENCODING(0xFF, 0xD5),
			new REFENCODING(0xFF, 0xD6),
			new REFENCODING(0xFF, 0xD7),
			new REFENCODING(0xFF, 0xD8),
			new REFENCODING(0xFF, 0xD9),
			new REFENCODING(0xFF, 0xDA),
			new REFENCODING(0xFF, 0xDB),
			new REFENCODING(0xFF, 0xDC),
			new REFENCODING(0xFF, 0xDD),
			new REFENCODING(0xFF, 0xDE),
			new REFENCODING(0xFF, 0xDF),
			new REFENCODING(0xFF, 0xE0),
			new REFENCODING(0xFF, 0xE1),
			new REFENCODING(0xFF, 0xE2),
			new REFENCODING(0xFF, 0xE3),
			new REFENCODING(0xFF, 0xE4),
			new REFENCODING(0xFF, 0xE5),
			new REFENCODING(0xFF, 0xE6),
			new REFENCODING(0xFF, 0xE7),
			new REFENCODING(0xFF, 0xE8),
			new REFENCODING(0xFF, 0xE9),
			new REFENCODING(0xFF, 0xEA),
			new REFENCODING(0xFF, 0xEB),
			new REFENCODING(0xFF, 0xEC),
			new REFENCODING(0xFF, 0xED),
			new REFENCODING(0xFF, 0xEE),
			new REFENCODING(0xFF, 0xEF),
			new REFENCODING(0xFF, 0xF0),
			new REFENCODING(0xFF, 0xF1),
			new REFENCODING(0xFF, 0xF2),
			new REFENCODING(0xFF, 0xF3),
			new REFENCODING(0xFF, 0xF4),
			new REFENCODING(0xFF, 0xF5),
			new REFENCODING(0xFF, 0xF6),
			new REFENCODING(0xFF, 0xF7),
			new REFENCODING(0xFF, 0xF8),
			new REFENCODING(0xFF, 0xF9),
			new REFENCODING(0xFF, 0xFA),
			new REFENCODING(0xFF, 0xFB),
			new REFENCODING(0xFF, 0xFC),
			new REFENCODING(0xFF, 0xFD),
			new REFENCODING(0xFF, 0xFE),
			new REFENCODING(0xFF, 0xFF),
			new REFENCODING(0xFE, 0x00),
			new REFENCODING(0xFE, 0x01),
			new REFENCODING(0xFE, 0x02),
			new REFENCODING(0xFE, 0x03),
			new REFENCODING(0xFE, 0x04),
			new REFENCODING(0xFE, 0x05),
			new REFENCODING(0xFE, 0x06),
			new REFENCODING(0xFE, 0x07),
			new REFENCODING(0xFE, 0x08),
			new REFENCODING(0xFE, 0x09),
			new REFENCODING(0xFE, 0x0A),
			new REFENCODING(0xFE, 0x0B),
			new REFENCODING(0xFE, 0x0C),
			new REFENCODING(0xFE, 0x0D),
			new REFENCODING(0xFE, 0x0E),
			new REFENCODING(0xFE, 0x0F),
			new REFENCODING(0xFE, 0x10),
			new REFENCODING(0xFE, 0x11),
			new REFENCODING(0xFE, 0x12),
			new REFENCODING(0xFE, 0x13),
			new REFENCODING(0xFE, 0x14),
			new REFENCODING(0xFE, 0x15),
			new REFENCODING(0xFE, 0x16),
			new REFENCODING(0xFE, 0x17),
			new REFENCODING(0xFE, 0x18),
			new REFENCODING(0xFE, 0x19),
			new REFENCODING(0xFE, 0x1A),
			new REFENCODING(0xFE, 0x1B),
			new REFENCODING(0xFE, 0x1C),
			new REFENCODING(0xFE, 0x1D),
			new REFENCODING(0xFE, 0x1E),
			new REFENCODING(0xFE, 0x1F),
			new REFENCODING(0xFE, 0x20),
			new REFENCODING(0xFE, 0x21),
			new REFENCODING(0xFE, 0x22),
			new REFENCODING(0x00, 0x00),
			new REFENCODING(0x00, 0x00),
			new REFENCODING(0x00, 0x00),
		};

        //------------------------------------------------------------
        // ILGENREC.ILStackOps
        //------------------------------------------------------------
        static private int[] ILStackOps =
        {
			(0) - (0) ,
			(0) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(0) - (1) ,
			(0) - (1) ,
			(0) - (1) ,
			(0) - (1) ,
			(1) - (0) ,
			(1) - (0) ,
			(0) - (1) ,
			(1) - (0) ,
			(1) - (0) ,
			(0) - (1) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(0) - (0) ,
			(1+1) - (1) ,
			(0) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (1) ,
			(0) - (1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (0) ,
			(0) - (1) ,
			(0) - (1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(0) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (1+1) ,
			(1) - (1) ,
			(1) - (0) ,
			(1) - (0) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(1) - (1) ,
			(0) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(0) - (1+1) ,
			(1) - (0) ,
			(1) - (0) ,
			(0) - (1) ,
			(0) - (1+1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(1) - (1+1) ,
			(0) - (1+1+1) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(1) - (1) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(1) - (0) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (1+1) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(1) - (0) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (1+1) ,
			(1) - (0) ,
			(1) - (1) ,
			(0) - (0) ,
			(1) - (0) ,
			(1) - (0) ,
			(0) - (1) ,
			(1) - (0) ,
			(1) - (0) ,
			(0) - (1) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (1) ,
			(0) - (0) ,
			(0) - (1+1+1) ,
			(0) - (1+1+1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(1) - (0) ,
			(1) - (1) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
			(0) - (0) ,
		};

        //------------------------------------------------------------
        // ILGENREC.ILcodesSize
        //------------------------------------------------------------
        static private int[] ILcodesSize =
		{
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			1 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			2 ,
			0 ,
			0 ,
			0 ,
		};

        //------------------------------------------------------------
        // ILGENREC.ILlsTiny
        //
        /// <summary>
        /// array of load store instructions. if an inline version is not present
        /// we indicate this with CEE_ILLEGAL which means that we have to use the _S version
        /// </summary>
        //------------------------------------------------------------
        private static ILCODE[,] ILlsTiny =
        {
        { ILCODE.LDLOC_0, ILCODE.LDLOC_1, ILCODE.LDLOC_2, ILCODE.LDLOC_3, ILCODE.LDLOC_S, ILCODE.LDLOC, },
        { ILCODE.STLOC_0, ILCODE.STLOC_1, ILCODE.STLOC_2, ILCODE.STLOC_3, ILCODE.STLOC_S, ILCODE.STLOC, },
        { ILCODE.LDARG_0, ILCODE.LDARG_1, ILCODE.LDARG_2, ILCODE.LDARG_3, ILCODE.LDARG_S, ILCODE.LDARG, },
        { ILCODE.ILLEGAL, ILCODE.ILLEGAL, ILCODE.ILLEGAL, ILCODE.ILLEGAL, ILCODE.STARG_S, ILCODE.STARG, },
        };

#if false
        //------------------------------------------------------------s
        // ILGENREC.OpCodeLSTiny
        //------------------------------------------------------------
        private static OpCode[,] OpCodeLSTiny =
        {
            {OpCodes.Ldloc_0,OpCodes.Ldloc_1,OpCodes.Ldloc_2,OpCodes.Ldloc_3,OpCodes.Ldloc_S,OpCodes.Ldloc,},
            {OpCodes.Stloc_0,OpCodes.Stloc_1,OpCodes.Stloc_2,OpCodes.Stloc_3,OpCodes.Stloc_S,OpCodes.Stloc,},
            {OpCodes.Ldarg_0,OpCodes.Ldarg_1,OpCodes.Ldarg_2,OpCodes.Ldarg_3,OpCodes.Ldarg_S,OpCodes.Ldarg},
        };
#endif

        //------------------------------------------------------------
        // ILGENREC.ILarithInstr
        //------------------------------------------------------------
        private static ILCODE[] ILarithInstr =
        {
			ILCODE.ADD,    	// EXPRKIND.ADD
			ILCODE.SUB,    	// EXPRKIND.SUB
			ILCODE.MUL,    	// EXPRKIND.MUL
			ILCODE.DIV,    	// EXPRKIND.DIV
			ILCODE.REM,    	// EXPRKIND.MOD
			ILCODE.NEG,    	// EXPRKIND.NEG
			ILCODE.ILLEGAL,	// EXPRKIND.UPLUS

			ILCODE.AND,    	// EXPRKIND.BITAND
			ILCODE.OR,     	// EXPRKIND.BITOR
			ILCODE.XOR,    	// EXPRKIND.BITXOR
			ILCODE.NOT,    	// EXPRKIND.BITNOT

			ILCODE.SHL,    	// EXPRKIND.LSHIFT
			ILCODE.SHR,    	// EXPRKIND.RSHIFT
			ILCODE.LDLEN,  	// EXPRKIND.ARRLEN
        };

        //------------------------------------------------------------
        // ILGENREC.ILarithInstrUN
        //------------------------------------------------------------
        private static ILCODE[] ILarithInstrUN =
        {
			ILCODE.ADD,    	// EXPRKIND.ADD
			ILCODE.SUB,    	// EXPRKIND.SUB
			ILCODE.MUL,    	// EXPRKIND.MUL
			ILCODE.DIV_UN, 	// EXPRKIND.DIV
			ILCODE.REM_UN, 	// EXPRKIND.MOD
			ILCODE.NEG,    	// EXPRKIND.NEG
			ILCODE.ILLEGAL,	// EXPRKIND.UPLUS

			ILCODE.AND,    	// EXPRKIND.BITAND
			ILCODE.OR,     	// EXPRKIND.BITOR
			ILCODE.XOR,    	// EXPRKIND.BITXOR
			ILCODE.NOT,    	// EXPRKIND.BITNOT

			ILCODE.SHL,    	// EXPRKIND.LSHIFT
			ILCODE.SHR_UN, 	// EXPRKIND.RSHIFT
			ILCODE.LDLEN,  	// EXPRKIND.ARRLEN
        };

        //------------------------------------------------------------
        // ILGENREC.ILarithInstrOvf
        //------------------------------------------------------------
        private static ILCODE[] ILarithInstrOvf =
        {
			ILCODE.ADD_OVF,	// EXPRKIND.ADD
			ILCODE.SUB_OVF,	// EXPRKIND.SUB
			ILCODE.MUL_OVF,	// EXPRKIND.MUL
			ILCODE.DIV,    	// EXPRKIND.DIV
			ILCODE.REM,    	// EXPRKIND.MOD
			ILCODE.NEG,    	// EXPRKIND.NEG
			ILCODE.ILLEGAL,	// EXPRKIND.UPLUS

			ILCODE.AND,    	// EXPRKIND.BITAND
			ILCODE.OR,     	// EXPRKIND.BITOR
			ILCODE.XOR,    	// EXPRKIND.BITXOR
			ILCODE.NOT,    	// EXPRKIND.BITNOT

			ILCODE.SHL,    	// EXPRKIND.LSHIFT
			ILCODE.SHR,    	// EXPRKIND.RSHIFT
			ILCODE.LDLEN,  	// EXPRKIND.ARRLEN
        };

        //------------------------------------------------------------
        // ILGENREC.ILarithInstrUNOvf
        //------------------------------------------------------------
        private static ILCODE[] ILarithInstrUNOvf =
        {
			ILCODE.ADD_OVF_UN,	// EXPRKIND.ADD
			ILCODE.SUB_OVF_UN,	// EXPRKIND.SUB
			ILCODE.MUL_OVF_UN,	// EXPRKIND.MUL
			ILCODE.DIV_UN,		// EXPRKIND.DIV
			ILCODE.REM_UN,		// EXPRKIND.MOD
			ILCODE.NEG,			// EXPRKIND.NEG
			ILCODE.ILLEGAL,		// EXPRKIND.UPLUS

			ILCODE.AND,			// EXPRKIND.BITAND
			ILCODE.OR,			// EXPRKIND.BITOR
			ILCODE.XOR,			// EXPRKIND.BITXOR
			ILCODE.NOT,			// EXPRKIND.BITNOT

			ILCODE.SHL,			// EXPRKIND.LSHIFT
			ILCODE.SHR_UN,		// EXPRKIND.RSHIFT
			ILCODE.LDLEN,		// EXPRKIND.ARRLEN
        };

        //------------------------------------------------------------
        // ILGENREC.ILstackLoad
        //------------------------------------------------------------
        private static ILCODE[] ILstackLoad =
        {
            ILCODE.ILLEGAL,     // FUNDTYPE.NONE,   // No fundemental type
            ILCODE.LDIND_I1,    // FUNDTYPE.I1,
            ILCODE.LDIND_I2,    // FUNDTYPE.I2,
            ILCODE.LDIND_I4,    // FUNDTYPE.I4,
            ILCODE.LDIND_U1,    // FUNDTYPE.U1,
            ILCODE.LDIND_U2,    // FUNDTYPE.U2,
            ILCODE.LDIND_U4,    // FUNDTYPE.U4,
            ILCODE.LDIND_I8,    // FUNDTYPE.I8,
            ILCODE.LDIND_I8,    // FUNDTYPE.U8,     // integral types
            ILCODE.LDIND_R4,    // FUNDTYPE.R4,
            ILCODE.LDIND_R8,    // FUNDTYPE.R8,     // floating types
            ILCODE.LDIND_REF,   // FUNDTYPE.REF,    // reference type
            ILCODE.LDOBJ,       // FUNDTYPE.STRUCT,	// structure type
            ILCODE.LDIND_I,     // FUNDTYPE.PTR
            ILCODE.LDOBJ,       // FUNDTYPE.VAR     // type variable
        };

        //------------------------------------------------------------
        // ILGENREC.ILstackStore
        //------------------------------------------------------------
        private static ILCODE[] ILstackStore =
        {
            ILCODE.ILLEGAL,		// FUNDTYPE.NONE,   // No fundemental type
            ILCODE.STIND_I1,	// FUNDTYPE.I1,
            ILCODE.STIND_I2,	// FUNDTYPE.I2,
            ILCODE.STIND_I4,	// FUNDTYPE.I4,
            ILCODE.STIND_I1,	// FUNDTYPE.U1,
            ILCODE.STIND_I2,	// FUNDTYPE.U2,
            ILCODE.STIND_I4,	// FUNDTYPE.U4,
            ILCODE.STIND_I8,	// FUNDTYPE.I8,
            ILCODE.STIND_I8,	// FUNDTYPE.U8,     // integral types
            ILCODE.STIND_R4,	// FUNDTYPE.R4,
            ILCODE.STIND_R8,	// FUNDTYPE.R8,     // floating types
            ILCODE.STIND_REF,	// FUNDTYPE.REF,    // reference type
            ILCODE.STOBJ,		// FUNDTYPE.STRUCT,	// structure type
            ILCODE.STIND_I,
            ILCODE.STOBJ,		// FUNDTYPE.VAR,    // type variable
        };

        //------------------------------------------------------------
        // ILGENREC.ILarrayLoad
        //------------------------------------------------------------
        private static ILCODE[] ILarrayLoad =
        {
            ILCODE.ILLEGAL,		// FUNDTYPE.NONE,	// No fundemental type
            ILCODE.LDELEM_I1,	// FUNDTYPE.I1,
            ILCODE.LDELEM_I2,	// FUNDTYPE.I2,
            ILCODE.LDELEM_I4,	// FUNDTYPE.I4,
            ILCODE.LDELEM_U1,	// FUNDTYPE.U1,
            ILCODE.LDELEM_U2,	// FUNDTYPE.U2,
            ILCODE.LDELEM_U4,	// FUNDTYPE.U4,
            ILCODE.LDELEM_I8,	// FUNDTYPE.I8,
            ILCODE.LDELEM_I8,	// FUNDTYPE.U8,     // integral types
            ILCODE.LDELEM_R4,	// FUNDTYPE.R4,
            ILCODE.LDELEM_R8,	// FUNDTYPE.R8,     // floating types
            ILCODE.LDELEM_REF,	// FUNDTYPE.REF,    // reference type
            ILCODE.ILLEGAL,		// FUNDTYPE.STRUCT,	// structure type
            ILCODE.LDELEM_I,	// FUNDTYPE.PTR
            ILCODE.LDELEM		// FUNDTYPE.VAR,    // generics
        };

        //------------------------------------------------------------
        // ILGENREC.ILarrayStore
        //------------------------------------------------------------
        private static ILCODE[] ILarrayStore =
        {
            ILCODE.ILLEGAL,		// FUNDTYPE.NONE,	// No fundemental type
            ILCODE.STELEM_I1,	// FUNDTYPE.I1,
            ILCODE.STELEM_I2,	// FUNDTYPE.I2,
            ILCODE.STELEM_I4,	// FUNDTYPE.I4,
            ILCODE.STELEM_I1,	// FUNDTYPE.U1,
            ILCODE.STELEM_I2,	// FUNDTYPE.U2,
            ILCODE.STELEM_I4,	// FUNDTYPE.U4,
            ILCODE.STELEM_I8,	// FUNDTYPE.I8,
            ILCODE.STELEM_I8,	// FUNDTYPE.U8,     // integral types
            ILCODE.STELEM_R4,	// FUNDTYPE.R4,
            ILCODE.STELEM_R8,	// FUNDTYPE.R8,     // floating types
            ILCODE.STELEM_REF,	// FUNDTYPE.REF,    // reference type
            ILCODE.ILLEGAL,		// FUNDTYPE.STRUCT,	// structure type
            ILCODE.STELEM_I,	// FUNDTYPE.PTR		// FIXED BUG!!!!
            ILCODE.STELEM		// FUNDTYPE.VAR,    // generics
        };

        //------------------------------------------------------------
        // ILGENREC.ILaddrLoad
        //------------------------------------------------------------
        private static ILCODE[,] ILaddrLoad =
        {
        { ILCODE.LDLOCA_S, ILCODE.LDLOCA, },
        { ILCODE.LDARGA_S, ILCODE.LDARGA, },
        };

        //------------------------------------------------------------
        // ILGENREC.simpleTypeConversions
        //------------------------------------------------------------
        private static ILCODE[,] simpleTypeConversions =
        {
			{
				ILCODE.next,			// BYTE -> BYTE
				ILCODE.next,			// BYTE -> I2
				ILCODE.next,			// BYTE -> I4
				ILCODE.CONV_U8,			// BYTE -> I8
				ILCODE.CONV_R4,			// BYTE -> FLT
				ILCODE.CONV_R8,			// BYTE -> DBL
				ILCODE.ILLEGAL,			// BYTE -> DEC
				ILCODE.next,			// BYTE -> CHAR
				ILCODE.ILLEGAL,			// BYTE -> BOOL
				ILCODE.CONV_I1,			// BYTE -> SBYTE
				ILCODE.next,			// BYTE -> U2
				ILCODE.next,			// BYTE -> U4
				ILCODE.CONV_U8,			// BYTE -> U8
				ILCODE.CONV_U,			// BYTE -> I
				ILCODE.CONV_U,			// BYTE -> U
			},

			{
				ILCODE.CONV_U1,			// I2 -> BYTE
				ILCODE.next,			// I2 -> I2
				ILCODE.next,			// I2 -> I4
				ILCODE.CONV_I8,			// I2 -> I8
				ILCODE.CONV_R4,			// I2 -> FLT
				ILCODE.CONV_R8,			// I2 -> DBL
				ILCODE.ILLEGAL,			// I2 -> DEC
				ILCODE.CONV_U2,			// I2 -> CHAR
				ILCODE.ILLEGAL,			// I2 -> BOOL
				ILCODE.CONV_I1,			// I2 -> SBYTE
				ILCODE.CONV_U2,			// I2 -> U2
				ILCODE.next,			// I2 -> U4
				ILCODE.CONV_I8,			// I2 -> U8
				ILCODE.CONV_I,			// I2 -> I
				ILCODE.CONV_I,			// I2 -> U
			},

			{
				ILCODE.CONV_U1,			// I4 -> BYTE
				ILCODE.CONV_I2,			// I4 -> I2
				ILCODE.next,			// I4 -> I4
				ILCODE.CONV_I8,			// I4 -> I8
				ILCODE.CONV_R4,			// I4 -> FLT
				ILCODE.CONV_R8,			// I4 -> DBL
				ILCODE.ILLEGAL,			// I4 -> DEC
				ILCODE.CONV_U2,			// I4 -> CHAR
				ILCODE.ILLEGAL,			// I4 -> BOOL
				ILCODE.CONV_I1,			// I4 -> SBYTE
				ILCODE.CONV_U2,			// I4 -> U2
				ILCODE.next,			// I4 -> U4
				ILCODE.CONV_I8,			// I4 -> U8
				ILCODE.CONV_I,			// I4 -> I
				ILCODE.CONV_I,			// I4 -> U
			},

			{
				ILCODE.CONV_U1,			// I8 -> BYTE
				ILCODE.CONV_I2,			// I8 -> I2
				ILCODE.CONV_I4,			// I8 -> I4
				ILCODE.next,			// I8 -> I8
				ILCODE.CONV_R4,			// I8 -> FLT
				ILCODE.CONV_R8,			// I8 -> DBL
				ILCODE.ILLEGAL,			// I8 -> DEC
				ILCODE.CONV_U2,			// I8 -> CHAR
				ILCODE.ILLEGAL,			// I8 -> BOOL
				ILCODE.CONV_I1,			// I8 -> SBYTE
				ILCODE.CONV_U2,			// I8 -> U2
				ILCODE.CONV_U4,			// I8 -> U4
				ILCODE.next,			// I8 -> U8
				ILCODE.CONV_I,			// I8 -> I
				ILCODE.CONV_U,			// I8 -> U
			},

			{
				ILCODE.CONV_U1,			// FLT -> BYTE
				ILCODE.CONV_I2,			// FLT -> I2
				ILCODE.CONV_I4,			// FLT -> I4
				ILCODE.CONV_I8,			// FLT -> I8
				ILCODE.CONV_R4,			// FLT -> FLT
				ILCODE.CONV_R8,			// FLT -> DBL
				ILCODE.ILLEGAL,			// FLT -> DEC
				ILCODE.CONV_U2,			// FLT -> CHAR
				ILCODE.ILLEGAL,			// FLT -> BOOL
				ILCODE.CONV_I1,			// FLT -> SBYTE
				ILCODE.CONV_U2,			// FLT -> U2
				ILCODE.CONV_U4,			// FLT -> U4
				ILCODE.CONV_U8,			// FLT -> U8
				ILCODE.CONV_I,			// FLT -> I
				ILCODE.CONV_U,			// FLT -> U
			},

			{
				ILCODE.CONV_U1,			// DBL -> BYTE
				ILCODE.CONV_I2,			// DBL -> I2
				ILCODE.CONV_I4,			// DBL -> I4
				ILCODE.CONV_I8,			// DBL -> I8
				ILCODE.CONV_R4,			// DBL -> FLT
				ILCODE.CONV_R8,			// DBL -> DBL
				ILCODE.ILLEGAL,			// DBL -> DEC
				ILCODE.CONV_U2,			// DBL -> CHAR
				ILCODE.ILLEGAL,			// DBL -> BOOL
				ILCODE.CONV_I1,			// DBL -> SBYTE
				ILCODE.CONV_U2,			// DBL -> U2
				ILCODE.CONV_U4,			// DBL -> U4
				ILCODE.CONV_U8,			// DBL -> U8
				ILCODE.CONV_I,			// DBL -> I
				ILCODE.CONV_U,			// DBL -> U
			},

			{
				ILCODE.ILLEGAL,			// DEC -> BYTE
				ILCODE.ILLEGAL,			// DEC -> I2
				ILCODE.ILLEGAL,			// DEC -> I4
				ILCODE.ILLEGAL,			// DEC -> I8
				ILCODE.ILLEGAL,			// DEC -> FLT
				ILCODE.ILLEGAL,			// DEC -> DBL
				ILCODE.ILLEGAL,			// DEC -> DEC
				ILCODE.ILLEGAL,			// DEC -> CHAR
				ILCODE.ILLEGAL,			// DEC -> BOOL
				ILCODE.ILLEGAL,			// DEC -> SBYTE
				ILCODE.ILLEGAL,			// DEC -> U2
				ILCODE.ILLEGAL,			// DEC -> U4
				ILCODE.ILLEGAL,			// DEC -> U8
				ILCODE.ILLEGAL,			// DEC -> I
				ILCODE.ILLEGAL,			// DEC -> U
			},

			{
				ILCODE.CONV_U1,			// CHAR -> BYTE
				ILCODE.CONV_I2,			// CHAR -> I2
				ILCODE.next,			// CHAR -> I4
				ILCODE.CONV_U8,			// CHAR -> I8
				ILCODE.CONV_R4,			// CHAR -> FLT
				ILCODE.CONV_R8,			// CHAR -> DBL
				ILCODE.ILLEGAL,			// CHAR -> DEC
				ILCODE.next,			// CHAR -> CHAR
				ILCODE.ILLEGAL,			// CHAR -> BOOL
				ILCODE.CONV_I1,			// CHAR -> SBYTE
				ILCODE.next,			// CHAR -> U2
				ILCODE.next,			// CHAR -> U4
				ILCODE.CONV_U8,			// CHAR -> U8
				ILCODE.CONV_U,			// CHAR -> I
				ILCODE.CONV_U,			// CHAR -> U
			},

			{
				ILCODE.ILLEGAL,			// BOOL -> BYTE
				ILCODE.ILLEGAL,			// BOOL -> I2
				ILCODE.ILLEGAL,			// BOOL -> I4
				ILCODE.ILLEGAL,			// BOOL -> I8
				ILCODE.ILLEGAL,			// BOOL -> FLT
				ILCODE.ILLEGAL,			// BOOL -> DBL
				ILCODE.ILLEGAL,			// BOOL -> DEC
				ILCODE.ILLEGAL,			// BOOL -> CHAR
				ILCODE.ILLEGAL,			// BOOL -> BOOL
				ILCODE.ILLEGAL,			// BOOL -> SBYTE
				ILCODE.ILLEGAL,			// BOOL -> U2
				ILCODE.ILLEGAL,			// BOOL -> U4
				ILCODE.ILLEGAL,			// BOOL -> U8
				ILCODE.ILLEGAL,			// BOOL -> I
				ILCODE.ILLEGAL,			// BOOL -> U
			},

			{
				ILCODE.CONV_U1,			// SBYTE -> BYTE
				ILCODE.next,			// SBYTE -> I2
				ILCODE.next,			// SBYTE -> I4
				ILCODE.CONV_I8,			// SBYTE -> I8
				ILCODE.CONV_R4,			// SBYTE -> FLT
				ILCODE.CONV_R8,			// SBYTE -> DBL
				ILCODE.ILLEGAL,			// SBYTE -> DEC
				ILCODE.CONV_U2,			// SBYTE -> CHAR
				ILCODE.ILLEGAL,			// SBYTE -> BOOL
				ILCODE.next,			// SBYTE -> SBYTE
				ILCODE.CONV_U2,			// SBYTE -> U2
				ILCODE.next,			// SBYTE -> U4
				ILCODE.CONV_I8,			// SBYTE -> U8
				ILCODE.CONV_I,			// SBYTE -> I
				ILCODE.CONV_I,			// SBYTE -> U
			},

			{
				ILCODE.CONV_U1,			// U2 -> BYTE
				ILCODE.CONV_I2,			// U2 -> I2
				ILCODE.next,			// U2 -> I4
				ILCODE.CONV_U8,			// U2 -> I8
				ILCODE.CONV_R4,			// U2 -> FLT
				ILCODE.CONV_R8,			// U2 -> DBL
				ILCODE.ILLEGAL,			// U2 -> DEC
				ILCODE.next,			// U2 -> CHAR
				ILCODE.ILLEGAL,			// U2 -> BOOL
				ILCODE.CONV_I1,			// U2 -> SBYTE
				ILCODE.next,			// U2 -> U2
				ILCODE.next,			// U2 -> U4
				ILCODE.CONV_U8,			// U2 -> U8
				ILCODE.CONV_U,			// U2 -> I
				ILCODE.CONV_U,			// U2 -> U
			},

			{
				ILCODE.CONV_U1,			// U4 -> BYTE
				ILCODE.CONV_I2,			// U4 -> I2
				ILCODE.next,			// U4 -> I4
				ILCODE.CONV_U8,			// U4 -> I8
				ILCODE.CONV_R_UN,		// U4 -> FLT
				ILCODE.CONV_R_UN,		// U4 -> DBL
				ILCODE.ILLEGAL,			// U4 -> DEC
				ILCODE.CONV_U2,			// U4 -> CHAR
				ILCODE.ILLEGAL,			// U4 -> BOOL
				ILCODE.CONV_I1,			// U4 -> SBYTE
				ILCODE.CONV_U2,			// U4 -> U2
				ILCODE.next,			// U4 -> U4
				ILCODE.CONV_U8,			// U4 -> U8
				ILCODE.CONV_U,			// U4 -> I
				ILCODE.CONV_U,			// U4 -> U
			},

			{
				ILCODE.CONV_U1,			// U8 -> BYTE
				ILCODE.CONV_I2,			// U8 -> I2
				ILCODE.CONV_I4,			// U8 -> I4
				ILCODE.next,			// U8 -> I8
				ILCODE.CONV_R_UN,		// U8 -> FLT
				ILCODE.CONV_R_UN,		// U8 -> DBL
				ILCODE.ILLEGAL,			// U8 -> DEC
				ILCODE.CONV_U2,			// U8 -> CHAR
				ILCODE.ILLEGAL,			// U8 -> BOOL
				ILCODE.CONV_I1,			// U8 -> SBYTE
				ILCODE.CONV_U2,			// U8 -> U2
				ILCODE.CONV_U4,			// U8 -> U4
				ILCODE.next,			// U8 -> U8
				ILCODE.CONV_I,			// U8 -> I
				ILCODE.CONV_U,			// U8 -> U
			},

			{
				ILCODE.CONV_U1,			// I -> BYTE
				ILCODE.CONV_I2,			// I -> I2
				ILCODE.CONV_I4,			// I -> I4
				ILCODE.CONV_I8,			// I -> I8
				ILCODE.CONV_R4,			// I -> FLT
				ILCODE.CONV_R8,			// I -> DBL
				ILCODE.ILLEGAL,			// I -> DEC
				ILCODE.CONV_U2,			// I -> CHAR
				ILCODE.ILLEGAL,			// I -> BOOL
				ILCODE.CONV_I1,			// I -> SBYTE
				ILCODE.CONV_U2,			// I -> U2
				ILCODE.CONV_U4,			// I -> U4
				ILCODE.CONV_I8,			// I -> U8
				ILCODE.next,			// I -> I
				ILCODE.next,			// I -> U
			},

			{
				ILCODE.CONV_U1,			// U -> BYTE
				ILCODE.CONV_I2,			// U -> I2
				ILCODE.CONV_I4,			// U -> I4
				ILCODE.CONV_U8,			// U -> I8
				ILCODE.CONV_R_UN,		// U -> FLT
				ILCODE.CONV_R_UN,		// U -> DBL
				ILCODE.ILLEGAL,			// U -> DEC
				ILCODE.CONV_U2,			// U -> CHAR
				ILCODE.ILLEGAL,			// U -> BOOL
				ILCODE.CONV_I1,			// U -> SBYTE
				ILCODE.CONV_U2,			// U -> U2
				ILCODE.CONV_U4,			// U -> U4
				ILCODE.CONV_U8,			// U -> U8
				ILCODE.next,			// U -> I
				ILCODE.next,			// U -> U
			},
        };

        //------------------------------------------------------------
        // ILGENREC.simpleTypeConversionsOvf
        //------------------------------------------------------------
        private static ILCODE[,] simpleTypeConversionsOvf =
        {
			{
				ILCODE.next,			// BYTE -> BYTE
				ILCODE.next,			// BYTE -> I2
				ILCODE.next,			// BYTE -> I4
				ILCODE.CONV_U8,			// BYTE -> I8
				ILCODE.CONV_R4,			// BYTE -> FLT
				ILCODE.CONV_R8,			// BYTE -> DBL
				ILCODE.ILLEGAL,			// BYTE -> DEC
				ILCODE.next,			// BYTE -> CHAR
				ILCODE.ILLEGAL,			// BYTE -> BOOL
				ILCODE.CONV_OVF_I1_UN,	// BYTE -> SBYTE
				ILCODE.next,			// BYTE -> U2
				ILCODE.next,			// BYTE -> U4
				ILCODE.CONV_U8,			// BYTE -> U8
				ILCODE.CONV_U,			// BYTE -> I
				ILCODE.CONV_U,			// BYTE -> U
			},

			{
				ILCODE.CONV_OVF_U1,		// I2 -> BYTE
				ILCODE.next,			// I2 -> I2
				ILCODE.next,			// I2 -> I4
				ILCODE.CONV_I8,			// I2 -> I8
				ILCODE.CONV_R4,			// I2 -> FLT
				ILCODE.CONV_R8,			// I2 -> DBL
				ILCODE.ILLEGAL,			// I2 -> DEC
				ILCODE.CONV_OVF_U2,		// I2 -> CHAR
				ILCODE.ILLEGAL,			// I2 -> BOOL
				ILCODE.CONV_OVF_I1,		// I2 -> SBYTE
				ILCODE.CONV_OVF_U2,		// I2 -> U2
				ILCODE.CONV_OVF_U4,		// I2 -> U4
				ILCODE.CONV_OVF_U8,		// I2 -> U8
				ILCODE.CONV_I,			// I2 -> I
				ILCODE.CONV_OVF_U,		// I2 -> U
			},

			{
				ILCODE.CONV_OVF_U1,		// I4 -> BYTE
				ILCODE.CONV_OVF_I2,		// I4 -> I2
				ILCODE.next,			// I4 -> I4
				ILCODE.CONV_I8,			// I4 -> I8
				ILCODE.CONV_R4,			// I4 -> FLT
				ILCODE.CONV_R8,			// I4 -> DBL
				ILCODE.ILLEGAL,			// I4 -> DEC
				ILCODE.CONV_OVF_U2,		// I4 -> CHAR
				ILCODE.ILLEGAL,			// I4 -> BOOL
				ILCODE.CONV_OVF_I1,		// I4 -> SBYTE
				ILCODE.CONV_OVF_U2,		// I4 -> U2
				ILCODE.CONV_OVF_U4,		// I4 -> U4
				ILCODE.CONV_OVF_U8,		// I4 -> U8
				ILCODE.CONV_I,			// I4 -> I
				ILCODE.CONV_OVF_U,		// I4 -> U
			},

			{
				ILCODE.CONV_OVF_U1,		// I8 -> BYTE
				ILCODE.CONV_OVF_I2,		// I8 -> I2
				ILCODE.CONV_OVF_I4,		// I8 -> I4
				ILCODE.next,			// I8 -> I8
				ILCODE.CONV_R4,			// I8 -> FLT
				ILCODE.CONV_R8,			// I8 -> DBL
				ILCODE.ILLEGAL,			// I8 -> DEC
				ILCODE.CONV_OVF_U2,		// I8 -> CHAR
				ILCODE.ILLEGAL,			// I8 -> BOOL
				ILCODE.CONV_OVF_I1,		// I8 -> SBYTE
				ILCODE.CONV_OVF_U2,		// I8 -> U2
				ILCODE.CONV_OVF_U4,		// I8 -> U4
				ILCODE.CONV_OVF_U8,		// I8 -> U8
				ILCODE.CONV_OVF_I,		// I8 -> I
				ILCODE.CONV_OVF_U,		// I8 -> U
			},

			{
				ILCODE.CONV_OVF_U1,		// FLT -> BYTE
				ILCODE.CONV_OVF_I2,		// FLT -> I2
				ILCODE.CONV_OVF_I4,		// FLT -> I4
				ILCODE.CONV_OVF_I8,		// FLT -> I8
				ILCODE.CONV_R4,			// FLT -> FLT
				ILCODE.CONV_R8,			// FLT -> DBL
				ILCODE.ILLEGAL,			// FLT -> DEC
				ILCODE.CONV_OVF_U2,		// FLT -> CHAR
				ILCODE.ILLEGAL,			// FLT -> BOOL
				ILCODE.CONV_OVF_I1,		// FLT -> SBYTE
				ILCODE.CONV_OVF_U2,		// FLT -> U2
				ILCODE.CONV_OVF_U4,		// FLT -> U4
				ILCODE.CONV_OVF_U8,		// FLT -> U8
				ILCODE.CONV_OVF_I,		// FLT -> I
				ILCODE.CONV_OVF_U,		// FLT -> U
			},

			{
				ILCODE.CONV_OVF_U1,		// DBL -> BYTE
				ILCODE.CONV_OVF_I2,		// DBL -> I2
				ILCODE.CONV_OVF_I4,		// DBL -> I4
				ILCODE.CONV_OVF_I8,		// DBL -> I8
				ILCODE.CONV_R4,			// DBL -> FLT
				ILCODE.CONV_R8,			// DBL -> DBL
				ILCODE.ILLEGAL,			// DBL -> DEC
				ILCODE.CONV_OVF_U2,		// DBL -> CHAR
				ILCODE.ILLEGAL,			// DBL -> BOOL
				ILCODE.CONV_OVF_I1,		// DBL -> SBYTE
				ILCODE.CONV_OVF_U2,		// DBL -> U2
				ILCODE.CONV_OVF_U4,		// DBL -> U4
				ILCODE.CONV_OVF_U8,		// DBL -> U8
				ILCODE.CONV_OVF_I,		// DBL -> I
				ILCODE.CONV_OVF_U,		// DBL -> U
			},

			{
				ILCODE.ILLEGAL,			// DEC -> BYTE
				ILCODE.ILLEGAL,			// DEC -> I2
				ILCODE.ILLEGAL,			// DEC -> I4
				ILCODE.ILLEGAL,			// DEC -> I8
				ILCODE.ILLEGAL,			// DEC -> FLT
				ILCODE.ILLEGAL,			// DEC -> DBL
				ILCODE.ILLEGAL,			// DEC -> DEC
				ILCODE.ILLEGAL,			// DEC -> CHAR
				ILCODE.ILLEGAL,			// DEC -> BOOL
				ILCODE.ILLEGAL,			// DEC -> SBYTE
				ILCODE.ILLEGAL,			// DEC -> U2
				ILCODE.ILLEGAL,			// DEC -> U4
				ILCODE.ILLEGAL,			// DEC -> U8
				ILCODE.ILLEGAL,			// DEC -> I
				ILCODE.ILLEGAL,			// DEC -> U
			},

			{
				ILCODE.CONV_OVF_U1_UN,	// CHAR -> BYTE
				ILCODE.CONV_OVF_I2_UN,	// CHAR -> I2
				ILCODE.next,			// CHAR -> I4
				ILCODE.CONV_I8,			// CHAR -> I8
				ILCODE.CONV_R4,			// CHAR -> FLT
				ILCODE.CONV_R8,			// CHAR -> DBL
				ILCODE.ILLEGAL,			// CHAR -> DEC
				ILCODE.next,			// CHAR -> CHAR
				ILCODE.ILLEGAL,			// CHAR -> BOOL
				ILCODE.CONV_OVF_I1_UN,	// CHAR -> SBYTE
				ILCODE.next,			// CHAR -> U2
				ILCODE.next,			// CHAR -> U4
				ILCODE.CONV_U8,			// CHAR -> U8
				ILCODE.CONV_U,			// CHAR -> I
				ILCODE.CONV_U,			// CHAR -> U
			},

			{
				ILCODE.ILLEGAL,			// BOOL -> BYTE
				ILCODE.ILLEGAL,			// BOOL -> I2
				ILCODE.ILLEGAL,			// BOOL -> I4
				ILCODE.ILLEGAL,			// BOOL -> I8
				ILCODE.ILLEGAL,			// BOOL -> FLT
				ILCODE.ILLEGAL,			// BOOL -> DBL
				ILCODE.ILLEGAL,			// BOOL -> DEC
				ILCODE.ILLEGAL,			// BOOL -> CHAR
				ILCODE.ILLEGAL,			// BOOL -> BOOL
				ILCODE.ILLEGAL,			// BOOL -> SBYTE
				ILCODE.ILLEGAL,			// BOOL -> U2
				ILCODE.ILLEGAL,			// BOOL -> U4
				ILCODE.ILLEGAL,			// BOOL -> U8
				ILCODE.ILLEGAL,			// BOOL -> I
				ILCODE.ILLEGAL,			// BOOL -> U
			},

			{
				ILCODE.CONV_OVF_U1,		// SBYTE -> BYTE
				ILCODE.next,			// SBYTE -> I2
				ILCODE.next,			// SBYTE -> I4
				ILCODE.CONV_I8,			// SBYTE -> I8
				ILCODE.CONV_R4,			// SBYTE -> FLT
				ILCODE.CONV_R8,			// SBYTE -> DBL
				ILCODE.ILLEGAL,			// SBYTE -> DEC
				ILCODE.CONV_OVF_U2,		// SBYTE -> CHAR
				ILCODE.ILLEGAL,			// SBYTE -> BOOL
				ILCODE.next,			// SBYTE -> SBYTE
				ILCODE.CONV_OVF_U2,		// SBYTE -> U2
				ILCODE.CONV_OVF_U4,		// SBYTE -> U4
				ILCODE.CONV_OVF_U8,		// SBYTE -> U8
				ILCODE.CONV_I,			// SBYTE -> I
				ILCODE.CONV_OVF_U,		// SBYTE -> U
			},

			{
				ILCODE.CONV_OVF_U1_UN,	// U2 -> BYTE
				ILCODE.CONV_OVF_I2_UN,	// U2 -> I2
				ILCODE.next,			// U2 -> I4
				ILCODE.CONV_I8,			// U2 -> I8
				ILCODE.CONV_R4,			// U2 -> FLT
				ILCODE.CONV_R8,			// U2 -> DBL
				ILCODE.ILLEGAL,			// U2 -> DEC
				ILCODE.next,			// U2 -> CHAR
				ILCODE.ILLEGAL,			// U2 -> BOOL
				ILCODE.CONV_OVF_I1_UN,	// U2 -> SBYTE
				ILCODE.next,			// U2 -> U2
				ILCODE.next,			// U2 -> U4
				ILCODE.CONV_U8,			// U2 -> U8
				ILCODE.CONV_U,			// U2 -> I
				ILCODE.CONV_U,			// U2 -> U
			},

			{
				ILCODE.CONV_OVF_U1_UN,	// U4 -> BYTE
				ILCODE.CONV_OVF_I2_UN,	// U4 -> I2
				ILCODE.CONV_OVF_I4_UN,	// U4 -> I4
				ILCODE.CONV_OVF_I8_UN,	// U4 -> I8
				ILCODE.CONV_R_UN,		// U4 -> FLT
				ILCODE.CONV_R_UN,		// U4 -> DBL
				ILCODE.ILLEGAL,			// U4 -> DEC
				ILCODE.CONV_OVF_U2_UN,	// U4 -> CHAR
				ILCODE.ILLEGAL,			// U4 -> BOOL
				ILCODE.CONV_OVF_I1_UN,	// U4 -> SBYTE
				ILCODE.CONV_OVF_U2_UN,	// U4 -> U2
				ILCODE.next,			// U4 -> U4
				ILCODE.CONV_U8,			// U4 -> U8
				ILCODE.CONV_OVF_I_UN,	// U4 -> I
				ILCODE.CONV_U,			// U4 -> U
			},

			{
				ILCODE.CONV_OVF_U1_UN,	// U8 -> BYTE
				ILCODE.CONV_OVF_I2_UN,	// U8 -> I2
				ILCODE.CONV_OVF_I4_UN,	// U8 -> I4
				ILCODE.CONV_OVF_I8_UN,	// U8 -> I8
				ILCODE.CONV_R_UN,		// U8 -> FLT
				ILCODE.CONV_R_UN,		// U8 -> DBL
				ILCODE.ILLEGAL,			// U8 -> DEC
				ILCODE.CONV_OVF_U2_UN,	// U8 -> CHAR
				ILCODE.ILLEGAL,			// U8 -> BOOL
				ILCODE.CONV_OVF_I1_UN,	// U8 -> SBYTE
				ILCODE.CONV_OVF_U2_UN,	// U8 -> U2
				ILCODE.CONV_OVF_U4_UN,	// U8 -> U4
				ILCODE.next,			// U8 -> U8
				ILCODE.CONV_OVF_I_UN,	// U8 -> I
				ILCODE.CONV_OVF_U_UN,	// U8 -> U
			},

			{
				ILCODE.CONV_OVF_U1,		// I -> BYTE
				ILCODE.CONV_OVF_I2,		// I -> I2
				ILCODE.CONV_OVF_I4,		// I -> I4
				ILCODE.CONV_I8,			// I -> I8
				ILCODE.CONV_R4,			// I -> FLT
				ILCODE.CONV_R8,			// I -> DBL
				ILCODE.ILLEGAL,			// I -> DEC
				ILCODE.CONV_OVF_U2,		// I -> CHAR
				ILCODE.ILLEGAL,			// I -> BOOL
				ILCODE.CONV_OVF_I1,		// I -> SBYTE
				ILCODE.CONV_OVF_U2,		// I -> U2
				ILCODE.CONV_OVF_U4,		// I -> U4
				ILCODE.CONV_OVF_U8,		// I -> U8
				ILCODE.next,			// I -> I
				ILCODE.CONV_OVF_U,		// I -> U
			},

			{
				ILCODE.CONV_OVF_U1_UN,	// U -> BYTE
				ILCODE.CONV_OVF_I2_UN,	// U -> I2
				ILCODE.CONV_OVF_I4_UN,	// U -> I4
				ILCODE.CONV_OVF_I8_UN,	// U -> I8
				ILCODE.CONV_R_UN,		// U -> FLT
				ILCODE.CONV_R_UN,		// U -> DBL
				ILCODE.ILLEGAL,			// U -> DEC
				ILCODE.CONV_OVF_U2_UN,	// U -> CHAR
				ILCODE.ILLEGAL,			// U -> BOOL
				ILCODE.CONV_OVF_I1_UN,	// U -> SBYTE
				ILCODE.CONV_OVF_U2_UN,	// U -> U2
				ILCODE.CONV_OVF_U4_UN,	// U -> U4
				ILCODE.CONV_U8,			// U -> U8
				ILCODE.CONV_OVF_I_UN,	// U -> I
				ILCODE.next,			// U -> U
			},
        };

        //------------------------------------------------------------
        // ILGENREC.simpleTypeConversionsEx
        //------------------------------------------------------------
        private static ILCODE[,] simpleTypeConversionsEx =
        {
			{
				ILCODE.ILLEGAL,			// BYTE -> BYTE
				ILCODE.ILLEGAL,			// BYTE -> I2
				ILCODE.ILLEGAL,			// BYTE -> I4
				ILCODE.ILLEGAL,			// BYTE -> I8
				ILCODE.ILLEGAL,			// BYTE -> FLT
				ILCODE.ILLEGAL,			// BYTE -> DBL
				ILCODE.ILLEGAL,			// BYTE -> DEC
				ILCODE.ILLEGAL,			// BYTE -> CHAR
				ILCODE.ILLEGAL,			// BYTE -> BOOL
				ILCODE.ILLEGAL,			// BYTE -> SBYTE
				ILCODE.ILLEGAL,			// BYTE -> U2
				ILCODE.ILLEGAL,			// BYTE -> U4
				ILCODE.ILLEGAL,			// BYTE -> U8
				ILCODE.ILLEGAL,			// BYTE -> I
				ILCODE.ILLEGAL,			// BYTE -> U
			},

			{
				ILCODE.ILLEGAL,			// I2 -> BYTE
				ILCODE.ILLEGAL,			// I2 -> I2
				ILCODE.ILLEGAL,			// I2 -> I4
				ILCODE.ILLEGAL,			// I2 -> I8
				ILCODE.ILLEGAL,			// I2 -> FLT
				ILCODE.ILLEGAL,			// I2 -> DBL
				ILCODE.ILLEGAL,			// I2 -> DEC
				ILCODE.ILLEGAL,			// I2 -> CHAR
				ILCODE.ILLEGAL,			// I2 -> BOOL
				ILCODE.ILLEGAL,			// I2 -> SBYTE
				ILCODE.ILLEGAL,			// I2 -> U2
				ILCODE.ILLEGAL,			// I2 -> U4
				ILCODE.ILLEGAL,			// I2 -> U8
				ILCODE.ILLEGAL,			// I2 -> I
				ILCODE.ILLEGAL,			// I2 -> U
			},

			{
				ILCODE.ILLEGAL,			// I4 -> BYTE
				ILCODE.ILLEGAL,			// I4 -> I2
				ILCODE.ILLEGAL,			// I4 -> I4
				ILCODE.ILLEGAL,			// I4 -> I8
				ILCODE.ILLEGAL,			// I4 -> FLT
				ILCODE.ILLEGAL,			// I4 -> DBL
				ILCODE.ILLEGAL,			// I4 -> DEC
				ILCODE.ILLEGAL,			// I4 -> CHAR
				ILCODE.ILLEGAL,			// I4 -> BOOL
				ILCODE.ILLEGAL,			// I4 -> SBYTE
				ILCODE.ILLEGAL,			// I4 -> U2
				ILCODE.ILLEGAL,			// I4 -> U4
				ILCODE.ILLEGAL,			// I4 -> U8
				ILCODE.ILLEGAL,			// I4 -> I
				ILCODE.ILLEGAL,			// I4 -> U
			},

			{
				ILCODE.ILLEGAL,			// I8 -> BYTE
				ILCODE.ILLEGAL,			// I8 -> I2
				ILCODE.ILLEGAL,			// I8 -> I4
				ILCODE.ILLEGAL,			// I8 -> I8
				ILCODE.ILLEGAL,			// I8 -> FLT
				ILCODE.ILLEGAL,			// I8 -> DBL
				ILCODE.ILLEGAL,			// I8 -> DEC
				ILCODE.ILLEGAL,			// I8 -> CHAR
				ILCODE.ILLEGAL,			// I8 -> BOOL
				ILCODE.ILLEGAL,			// I8 -> SBYTE
				ILCODE.ILLEGAL,			// I8 -> U2
				ILCODE.ILLEGAL,			// I8 -> U4
				ILCODE.ILLEGAL,			// I8 -> U8
				ILCODE.ILLEGAL,			// I8 -> I
				ILCODE.ILLEGAL,			// I8 -> U
			},

			{
				ILCODE.ILLEGAL,			// FLT -> BYTE
				ILCODE.ILLEGAL,			// FLT -> I2
				ILCODE.ILLEGAL,			// FLT -> I4
				ILCODE.ILLEGAL,			// FLT -> I8
				ILCODE.ILLEGAL,			// FLT -> FLT
				ILCODE.ILLEGAL,			// FLT -> DBL
				ILCODE.ILLEGAL,			// FLT -> DEC
				ILCODE.ILLEGAL,			// FLT -> CHAR
				ILCODE.ILLEGAL,			// FLT -> BOOL
				ILCODE.ILLEGAL,			// FLT -> SBYTE
				ILCODE.ILLEGAL,			// FLT -> U2
				ILCODE.ILLEGAL,			// FLT -> U4
				ILCODE.ILLEGAL,			// FLT -> U8
				ILCODE.ILLEGAL,			// FLT -> I
				ILCODE.ILLEGAL,			// FLT -> U
			},

			{
				ILCODE.ILLEGAL,			// DBL -> BYTE
				ILCODE.ILLEGAL,			// DBL -> I2
				ILCODE.ILLEGAL,			// DBL -> I4
				ILCODE.ILLEGAL,			// DBL -> I8
				ILCODE.ILLEGAL,			// DBL -> FLT
				ILCODE.ILLEGAL,			// DBL -> DBL
				ILCODE.ILLEGAL,			// DBL -> DEC
				ILCODE.ILLEGAL,			// DBL -> CHAR
				ILCODE.ILLEGAL,			// DBL -> BOOL
				ILCODE.ILLEGAL,			// DBL -> SBYTE
				ILCODE.ILLEGAL,			// DBL -> U2
				ILCODE.ILLEGAL,			// DBL -> U4
				ILCODE.ILLEGAL,			// DBL -> U8
				ILCODE.ILLEGAL,			// DBL -> I
				ILCODE.ILLEGAL,			// DBL -> U
			},

			{
				ILCODE.ILLEGAL,			// DEC -> BYTE
				ILCODE.ILLEGAL,			// DEC -> I2
				ILCODE.ILLEGAL,			// DEC -> I4
				ILCODE.ILLEGAL,			// DEC -> I8
				ILCODE.ILLEGAL,			// DEC -> FLT
				ILCODE.ILLEGAL,			// DEC -> DBL
				ILCODE.ILLEGAL,			// DEC -> DEC
				ILCODE.ILLEGAL,			// DEC -> CHAR
				ILCODE.ILLEGAL,			// DEC -> BOOL
				ILCODE.ILLEGAL,			// DEC -> SBYTE
				ILCODE.ILLEGAL,			// DEC -> U2
				ILCODE.ILLEGAL,			// DEC -> U4
				ILCODE.ILLEGAL,			// DEC -> U8
				ILCODE.ILLEGAL,			// DEC -> I
				ILCODE.ILLEGAL,			// DEC -> U
			},

			{
				ILCODE.ILLEGAL,			// CHAR -> BYTE
				ILCODE.ILLEGAL,			// CHAR -> I2
				ILCODE.ILLEGAL,			// CHAR -> I4
				ILCODE.ILLEGAL,			// CHAR -> I8
				ILCODE.ILLEGAL,			// CHAR -> FLT
				ILCODE.ILLEGAL,			// CHAR -> DBL
				ILCODE.ILLEGAL,			// CHAR -> DEC
				ILCODE.ILLEGAL,			// CHAR -> CHAR
				ILCODE.ILLEGAL,			// CHAR -> BOOL
				ILCODE.ILLEGAL,			// CHAR -> SBYTE
				ILCODE.ILLEGAL,			// CHAR -> U2
				ILCODE.ILLEGAL,			// CHAR -> U4
				ILCODE.ILLEGAL,			// CHAR -> U8
				ILCODE.ILLEGAL,			// CHAR -> I
				ILCODE.ILLEGAL,			// CHAR -> U
			},

			{
				ILCODE.ILLEGAL,			// BOOL -> BYTE
				ILCODE.ILLEGAL,			// BOOL -> I2
				ILCODE.ILLEGAL,			// BOOL -> I4
				ILCODE.ILLEGAL,			// BOOL -> I8
				ILCODE.ILLEGAL,			// BOOL -> FLT
				ILCODE.ILLEGAL,			// BOOL -> DBL
				ILCODE.ILLEGAL,			// BOOL -> DEC
				ILCODE.ILLEGAL,			// BOOL -> CHAR
				ILCODE.ILLEGAL,			// BOOL -> BOOL
				ILCODE.ILLEGAL,			// BOOL -> SBYTE
				ILCODE.ILLEGAL,			// BOOL -> U2
				ILCODE.ILLEGAL,			// BOOL -> U4
				ILCODE.ILLEGAL,			// BOOL -> U8
				ILCODE.ILLEGAL,			// BOOL -> I
				ILCODE.ILLEGAL,			// BOOL -> U
			},

			{
				ILCODE.ILLEGAL,			// SBYTE -> BYTE
				ILCODE.ILLEGAL,			// SBYTE -> I2
				ILCODE.ILLEGAL,			// SBYTE -> I4
				ILCODE.ILLEGAL,			// SBYTE -> I8
				ILCODE.ILLEGAL,			// SBYTE -> FLT
				ILCODE.ILLEGAL,			// SBYTE -> DBL
				ILCODE.ILLEGAL,			// SBYTE -> DEC
				ILCODE.ILLEGAL,			// SBYTE -> CHAR
				ILCODE.ILLEGAL,			// SBYTE -> BOOL
				ILCODE.ILLEGAL,			// SBYTE -> SBYTE
				ILCODE.ILLEGAL,			// SBYTE -> U2
				ILCODE.ILLEGAL,			// SBYTE -> U4
				ILCODE.ILLEGAL,			// SBYTE -> U8
				ILCODE.ILLEGAL,			// SBYTE -> I
				ILCODE.ILLEGAL,			// SBYTE -> U
			},

			{
				ILCODE.ILLEGAL,			// U2 -> BYTE
				ILCODE.ILLEGAL,			// U2 -> I2
				ILCODE.ILLEGAL,			// U2 -> I4
				ILCODE.ILLEGAL,			// U2 -> I8
				ILCODE.ILLEGAL,			// U2 -> FLT
				ILCODE.ILLEGAL,			// U2 -> DBL
				ILCODE.ILLEGAL,			// U2 -> DEC
				ILCODE.ILLEGAL,			// U2 -> CHAR
				ILCODE.ILLEGAL,			// U2 -> BOOL
				ILCODE.ILLEGAL,			// U2 -> SBYTE
				ILCODE.ILLEGAL,			// U2 -> U2
				ILCODE.ILLEGAL,			// U2 -> U4
				ILCODE.ILLEGAL,			// U2 -> U8
				ILCODE.ILLEGAL,			// U2 -> I
				ILCODE.ILLEGAL,			// U2 -> U
			},

			{
				ILCODE.ILLEGAL,			// U4 -> BYTE
				ILCODE.ILLEGAL,			// U4 -> I2
				ILCODE.ILLEGAL,			// U4 -> I4
				ILCODE.ILLEGAL,			// U4 -> I8
				ILCODE.CONV_R4,			// U4 -> FLT
				ILCODE.CONV_R8,			// U4 -> DBL
				ILCODE.ILLEGAL,			// U4 -> DEC
				ILCODE.ILLEGAL,			// U4 -> CHAR
				ILCODE.ILLEGAL,			// U4 -> BOOL
				ILCODE.ILLEGAL,			// U4 -> SBYTE
				ILCODE.ILLEGAL,			// U4 -> U2
				ILCODE.ILLEGAL,			// U4 -> U4
				ILCODE.ILLEGAL,			// U4 -> U8
				ILCODE.ILLEGAL,			// U4 -> I
				ILCODE.ILLEGAL,			// U4 -> U
			},

			{
				ILCODE.ILLEGAL,			// U8 -> BYTE
				ILCODE.ILLEGAL,			// U8 -> I2
				ILCODE.ILLEGAL,			// U8 -> I4
				ILCODE.ILLEGAL,			// U8 -> I8
				ILCODE.CONV_R4,			// U8 -> FLT
				ILCODE.CONV_R8,			// U8 -> DBL
				ILCODE.ILLEGAL,			// U8 -> DEC
				ILCODE.ILLEGAL,			// U8 -> CHAR
				ILCODE.ILLEGAL,			// U8 -> BOOL
				ILCODE.ILLEGAL,			// U8 -> SBYTE
				ILCODE.ILLEGAL,			// U8 -> U2
				ILCODE.ILLEGAL,			// U8 -> U4
				ILCODE.ILLEGAL,			// U8 -> U8
				ILCODE.ILLEGAL,			// U8 -> I
				ILCODE.ILLEGAL,			// U8 -> U
			},

			{
				ILCODE.ILLEGAL,			// I -> BYTE
				ILCODE.ILLEGAL,			// I -> I2
				ILCODE.ILLEGAL,			// I -> I4
				ILCODE.ILLEGAL,			// I -> I8
				ILCODE.ILLEGAL,			// I -> FLT
				ILCODE.ILLEGAL,			// I -> DBL
				ILCODE.ILLEGAL,			// I -> DEC
				ILCODE.ILLEGAL,			// I -> CHAR
				ILCODE.ILLEGAL,			// I -> BOOL
				ILCODE.ILLEGAL,			// I -> SBYTE
				ILCODE.ILLEGAL,			// I -> U2
				ILCODE.ILLEGAL,			// I -> U4
				ILCODE.ILLEGAL,			// I -> U8
				ILCODE.ILLEGAL,			// I -> I
				ILCODE.ILLEGAL,			// I -> U
			},

			{
				ILCODE.ILLEGAL,			// U -> BYTE
				ILCODE.ILLEGAL,			// U -> I2
				ILCODE.ILLEGAL,			// U -> I4
				ILCODE.ILLEGAL,			// U -> I8
				ILCODE.CONV_R4,			// U -> FLT
				ILCODE.CONV_R8,			// U -> DBL
				ILCODE.ILLEGAL,			// U -> DEC
				ILCODE.ILLEGAL,			// U -> CHAR
				ILCODE.ILLEGAL,			// U -> BOOL
				ILCODE.ILLEGAL,			// U -> SBYTE
				ILCODE.ILLEGAL,			// U -> U2
				ILCODE.ILLEGAL,			// U -> U4
				ILCODE.ILLEGAL,			// U -> U8
				ILCODE.ILLEGAL,			// U -> I
				ILCODE.ILLEGAL,			// U -> U
			},
        };

        //------------------------------------------------------------
        // ILGENREC.rgcodeCmp
        //
        /// The opcodes to emit based on ek and nos. Note that *psense is inverted iff
        /// (ek - EK_LT) is odd. If this needs to change to some non-trivial calculation,
        /// use a parallel table of bools to indicate whether *psense should be inverted.
        //------------------------------------------------------------
        static private ILCODE[] rgcodeCmp =
        {
			//	<				<=				>				>=
			ILCODE.CLT,		ILCODE.CGT,		ILCODE.CGT,		ILCODE.CLT,		// Signed
			ILCODE.CLT_UN,	ILCODE.CGT_UN,	ILCODE.CGT_UN,	ILCODE.CLT_UN,	// Unsigned
			ILCODE.CLT,		ILCODE.CGT_UN,	ILCODE.CGT,		ILCODE.CLT_UN,	// Float
        };

#if DEBUG
        //------------------------------------------------------------
        // ILGENREC.ILnames
        //------------------------------------------------------------
        static private string[] ILnames =
		{
			"nop",
			"break",
			"ldarg.0",
			"ldarg.1",
			"ldarg.2",
			"ldarg.3",
			"ldloc.0",
			"ldloc.1",
			"ldloc.2",
			"ldloc.3",
			"stloc.0",
			"stloc.1",
			"stloc.2",
			"stloc.3",
			"ldarg.s",
			"ldarga.s",
			"starg.s",
			"ldloc.s",
			"ldloca.s",
			"stloc.s",
			"ldnull",
			"ldc.i4.m1",
			"ldc.i4.0",
			"ldc.i4.1",
			"ldc.i4.2",
			"ldc.i4.3",
			"ldc.i4.4",
			"ldc.i4.5",
			"ldc.i4.6",
			"ldc.i4.7",
			"ldc.i4.8",
			"ldc.i4.s",
			"ldc.i4",
			"ldc.i8",
			"ldc.r4",
			"ldc.r8",
			"unused",
			"dup",
			"pop",
			"jmp",
			"call",
			"calli",
			"ret",
			"br.s",
			"brfalse.s",
			"brtrue.s",
			"beq.s",
			"bge.s",
			"bgt.s",
			"ble.s",
			"blt.s",
			"bne.un.s",
			"bge.un.s",
			"bgt.un.s",
			"ble.un.s",
			"blt.un.s",
			"br",
			"brfalse",
			"brtrue",
			"beq",
			"bge",
			"bgt",
			"ble",
			"blt",
			"bne.un",
			"bge.un",
			"bgt.un",
			"ble.un",
			"blt.un",
			"switch",
			"ldind.i1",
			"ldind.u1",
			"ldind.i2",
			"ldind.u2",
			"ldind.i4",
			"ldind.u4",
			"ldind.i8",
			"ldind.i",
			"ldind.r4",
			"ldind.r8",
			"ldind.ref",
			"stind.ref",
			"stind.i1",
			"stind.i2",
			"stind.i4",
			"stind.i8",
			"stind.r4",
			"stind.r8",
			"add",
			"sub",
			"mul",
			"div",
			"div.un",
			"rem",
			"rem.un",
			"and",
			"or",
			"xor",
			"shl",
			"shr",
			"shr.un",
			"neg",
			"not",
			"conv.i1",
			"conv.i2",
			"conv.i4",
			"conv.i8",
			"conv.r4",
			"conv.r8",
			"conv.u4",
			"conv.u8",
			"callvirt",
			"cpobj",
			"ldobj",
			"ldstr",
			"newobj",
			"castclass",
			"isinst",
			"conv.r.un",
			"unused",
			"unused",
			"unbox",
			"throw",
			"ldfld",
			"ldflda",
			"stfld",
			"ldsfld",
			"ldsflda",
			"stsfld",
			"stobj",
			"conv.ovf.i1.un",
			"conv.ovf.i2.un",
			"conv.ovf.i4.un",
			"conv.ovf.i8.un",
			"conv.ovf.u1.un",
			"conv.ovf.u2.un",
			"conv.ovf.u4.un",
			"conv.ovf.u8.un",
			"conv.ovf.i.un",
			"conv.ovf.u.un",
			"box",
			"newarr",
			"ldlen",
			"ldelema",
			"ldelem.i1",
			"ldelem.u1",
			"ldelem.i2",
			"ldelem.u2",
			"ldelem.i4",
			"ldelem.u4",
			"ldelem.i8",
			"ldelem.i",
			"ldelem.r4",
			"ldelem.r8",
			"ldelem.ref",
			"stelem.i",
			"stelem.i1",
			"stelem.i2",
			"stelem.i4",
			"stelem.i8",
			"stelem.r4",
			"stelem.r8",
			"stelem.ref",
			"ldelem",
			"stelem",
			"unbox.any",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"conv.ovf.i1",
			"conv.ovf.u1",
			"conv.ovf.i2",
			"conv.ovf.u2",
			"conv.ovf.i4",
			"conv.ovf.u4",
			"conv.ovf.i8",
			"conv.ovf.u8",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"refanyval",
			"ckfinite",
			"unused",
			"unused",
			"mkrefany",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"ldtoken",
			"conv.u2",
			"conv.u1",
			"conv.i",
			"conv.ovf.i",
			"conv.ovf.u",
			"add.ovf",
			"add.ovf.un",
			"mul.ovf",
			"mul.ovf.un",
			"sub.ovf",
			"sub.ovf.un",
			"endfinally",
			"leave",
			"leave.s",
			"stind.i",
			"conv.u",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"unused",
			"prefix7",
			"prefix6",
			"prefix5",
			"prefix4",
			"prefix3",
			"prefix2",
			"prefix1",
			"prefixref",
			"arglist",
			"ceq",
			"cgt",
			"cgt.un",
			"clt",
			"clt.un",
			"ldftn",
			"ldvirtftn",
			"unused",
			"ldarg",
			"ldarga",
			"starg",
			"ldloc",
			"ldloca",
			"stloc",
			"localloc",
			"unused",
			"endfilter",
			"unaligned.",
			"volatile.",
			"tail.",
			"initobj",
			"constrained.",
			"cpblk",
			"initblk",
			"unused",
			"rethrow",
			"unused",
			"sizeof",
			"refanytype",
			"readonly.",
			"unused",
			"unused",
			"unused",
			"unused",
			"illegal",
			"endmac",
			"codelabel",
		};
#endif
    }
}
