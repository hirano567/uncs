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
// File: constval.h
//
// ===========================================================================

//============================================================================
// Constval.cs
//
// 2015/05/06
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{

    //======================================================================
    // class ConstValUtility (static)
    //======================================================================
    static internal class ConstValUtility
    {
        static internal Dictionary<System.Type, PREDEFTYPE> TypeToPtDic
            = new Dictionary<Type, PREDEFTYPE>();

        //------------------------------------------------------------
        // ConstValUtility Constructor
        //------------------------------------------------------------
        static ConstValUtility()
        {
            InitTypeToPtDic();
        }

        //------------------------------------------------------------
        // ConstValUtility.InitTypeToPtDic
        //------------------------------------------------------------
        static private void InitTypeToPtDic()
        {
            TypeToPtDic.Add(SystemType.ByteType, PREDEFTYPE.BYTE);
            TypeToPtDic.Add(SystemType.ShortType, PREDEFTYPE.SHORT);
            TypeToPtDic.Add(SystemType.IntType, PREDEFTYPE.INT);
            TypeToPtDic.Add(SystemType.LongType, PREDEFTYPE.LONG);
            TypeToPtDic.Add(SystemType.FloatType, PREDEFTYPE.FLOAT);
            TypeToPtDic.Add(SystemType.DoubleType, PREDEFTYPE.DOUBLE);
            TypeToPtDic.Add(SystemType.DecimalType, PREDEFTYPE.DECIMAL);
            TypeToPtDic.Add(SystemType.CharType, PREDEFTYPE.CHAR);
            TypeToPtDic.Add(SystemType.BoolType, PREDEFTYPE.BOOL);
            TypeToPtDic.Add(SystemType.SByteType, PREDEFTYPE.SBYTE);
            TypeToPtDic.Add(SystemType.UShortType, PREDEFTYPE.USHORT);
            TypeToPtDic.Add(SystemType.UIntType, PREDEFTYPE.UINT);
            TypeToPtDic.Add(SystemType.ULongType, PREDEFTYPE.ULONG);
            TypeToPtDic.Add(SystemType.ObjectType, PREDEFTYPE.OBJECT);
            TypeToPtDic.Add(SystemType.StringType, PREDEFTYPE.STRING);
        }

        //------------------------------------------------------------
        // ConstValUtility.GetPredefType
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal PREDEFTYPE GetPredefType(Type type)
        {
            PREDEFTYPE pt;
            if (type != null)
            {
                try
                {
                    if (TypeToPtDic.TryGetValue(type, out pt))
                    {
                        return pt;
                    }
                }
                catch (ArgumentException)
                {
                    DebugUtil.Assert(false);
                }
            }
            return PREDEFTYPE.UNDEFINED;
        }
    }

    
    // struct DecimalConstantBuffer の代わりに decimal を使う。

    // class CONSTVAL は System.Object を使う。

    //----------------------------------------------------------------------
    // A string constant. These are NOT nul terminated, and can contain
    // internal nul characters.  
    //----------------------------------------------------------------------
    //  struct STRCONST
    //  null で分割されている場合も想定しているので、
    //      System.Collections.Generic.List<System.String>
    //  で対応することにする。

    //======================================================================
    // class CONSTVAL
    //
    /// <summary>
    /// <para>A constant value. We want this to use only 4 bytes, so larger
    /// values are represented by pointers.</para>
    /// <para>In this project, not define CONSTVALNS.</para>
    /// </summary>
    //======================================================================
    internal class CONSTVAL
    {
        static protected Type typeByte = typeof(byte);
        static protected Type typeShort = typeof(short);
        static protected Type typeInt = typeof(int);
        static protected Type typeLong = typeof(long);
        static protected Type typeFloat = typeof(float);
        static protected Type typeDouble = typeof(double);
        static protected Type typeDecimal = typeof(decimal);
        static protected Type typeChar = typeof(char);
        static protected Type typeBool = typeof(bool);
        static protected Type typeSByte = typeof(sbyte);
        static protected Type typeUShort = typeof(ushort);
        static protected Type typeUInt = typeof(uint);
        static protected Type typeULong = typeof(ulong);
        static protected Type typeObject = typeof(Object);
        static protected Type typeString = typeof(string);

        //------------------------------------------------------------
        // CONSTVAL Fields and Properties (1) value
        //------------------------------------------------------------
        protected object val = null;
        protected PREDEFTYPE constType = PREDEFTYPE.UNDEFINED;

        internal bool IsNull
        {
            get { return (this.val == null); }
        }

        //------------------------------------------------------------
        // CONSTVAL Fields and Properties (2) Is*
        //------------------------------------------------------------
        internal bool IsByte
        {
            get { return constType == PREDEFTYPE.BYTE; }
        }
        internal bool IsShort
        {
            get { return constType == PREDEFTYPE.SHORT; }
        }
        internal bool IsInt
        {
            get { return constType == PREDEFTYPE.INT; }
        }
        internal bool IsLong
        {
            get { return constType == PREDEFTYPE.LONG; }
        }
        internal bool IsFloat
        {
            get { return constType == PREDEFTYPE.FLOAT; }
        }
        internal bool IsDouble
        {
            get { return constType == PREDEFTYPE.DOUBLE; }
        }
        internal bool IsDecimal
        {
            get { return constType == PREDEFTYPE.DECIMAL; }
        }
        internal bool IsChar
        {
            get { return constType == PREDEFTYPE.CHAR; }
        }
        internal bool IsBool
        {
            get { return constType == PREDEFTYPE.BOOL; }
        }
        internal bool IsSByte
        {
            get { return constType == PREDEFTYPE.SBYTE; }
        }
        internal bool IsUShort
        {
            get { return constType == PREDEFTYPE.USHORT; }
        }
        internal bool IsUInt
        {
            get { return constType == PREDEFTYPE.UINT; }
        }
        internal bool IsULong
        {
            get { return constType == PREDEFTYPE.ULONG; }
        }
        internal bool IsObject
        {
            get { return constType == PREDEFTYPE.OBJECT; }
        }
        internal bool IsString
        {
            get { return constType == PREDEFTYPE.STRING; }
        }

        //------------------------------------------------------------
        // CONSTVAL Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CONSTVAL() { }

        //------------------------------------------------------------
        // CONSTVAL Constructor (2)
        //
        /// <summary></summary>
        /// <param name="v"></param>
        //------------------------------------------------------------
        internal CONSTVAL(object v)
        {
            val = v;
            if (val == null)
            {
                this.constType = PREDEFTYPE.UNDEFINED;
                return;
            }

            Type objType = val.GetType();
            if (objType == typeByte)
            {
                this.constType = PREDEFTYPE.BYTE;
            }
            else if (objType == typeShort)
            {
                this.constType = PREDEFTYPE.SHORT;
            }
            else if (objType == typeInt)
            {
                this.constType = PREDEFTYPE.INT;
            }
            else if (objType == typeLong)
            {
                this.constType = PREDEFTYPE.LONG;
            }
            else if (objType == typeFloat)
            {
                this.constType = PREDEFTYPE.FLOAT;
            }
            else if (objType == typeDouble)
            {
                this.constType = PREDEFTYPE.DOUBLE;
            }
            else if (objType == typeDecimal)
            {
                this.constType = PREDEFTYPE.DECIMAL;
            }
            else if (objType == typeChar)
            {
                this.constType = PREDEFTYPE.CHAR;
            }
            else if (objType == typeBool)
            {
                this.constType = PREDEFTYPE.BOOL;
            }
            else if (objType == typeSByte)
            {
                this.constType = PREDEFTYPE.SBYTE;
            }
            else if (objType == typeUShort)
            {
                this.constType = PREDEFTYPE.USHORT;
            }
            else if (objType == typeUInt)
            {
                this.constType = PREDEFTYPE.UINT;
            }
            else if (objType == typeULong)
            {
                this.constType = PREDEFTYPE.ULONG;
            }
            else if (objType == typeString)
            {
                this.constType = PREDEFTYPE.STRING;
            }
            else
            {
                this.constType = PREDEFTYPE.UNDEFINED;
            }
        }

        //------------------------------------------------------------
        // CONSTVAL Constructor (3)
        //
        /// <summary></summary>
        /// <param name="cv"></param>
        //------------------------------------------------------------
        internal CONSTVAL(CONSTVAL cv)
        {
            Set(cv);
        }

        //------------------------------------------------------------
        // CONSTVAL.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        virtual internal void Clear()
        {
            this.val = null;
            this.constType = PREDEFTYPE.UNDEFINED;
        }

        //------------------------------------------------------------
        // CONSTVAL.IsValidPredefType
        //
        /// <summary>
        /// Return true if the specified predefined type is valid in CONSTVAL.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsValidPredefType(PREDEFTYPE pt)
        {
            switch (pt)
            {
                case PREDEFTYPE.BYTE:
                case PREDEFTYPE.SHORT:
                case PREDEFTYPE.INT:
                case PREDEFTYPE.LONG:
                case PREDEFTYPE.FLOAT:
                case PREDEFTYPE.DOUBLE:
                case PREDEFTYPE.DECIMAL:
                case PREDEFTYPE.CHAR:
                case PREDEFTYPE.BOOL:
                case PREDEFTYPE.SBYTE:
                case PREDEFTYPE.USHORT:
                case PREDEFTYPE.UINT:
                case PREDEFTYPE.ULONG:
                case PREDEFTYPE.OBJECT:
                case PREDEFTYPE.STRING:
                    return true;

                default:
                    break;
            }
            return false;
        }

        //------------------------------------------------------------
        // CONSTVAL.ValidateConstPredefType
        //
        /// <summary></summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PREDEFTYPE ValidateConstPredefType()
        {
            if (this.val == null)
            {
                return PREDEFTYPE.UNDEFINED;
            }
            if (IsValidPredefType(this.constType))
            {
                return this.constType;
            }

            Type type = this.val.GetType();
            if (type.IsEnum)
            {
                Type utype = Enum.GetUnderlyingType(type);
                if (utype != null)
                {
                    return ConstValUtility.GetPredefType(utype);
                }
            }
            return PREDEFTYPE.UNDEFINED;
        }

        //------------------------------------------------------------
        // CONSTVAL.Set (1)
        //------------------------------------------------------------
        virtual internal void Set(CONSTVAL cv)
        {
            if (cv == null)
            {
                return;
            }

            this.val = cv.val;
            this.constType = cv.constType;
        }

        //------------------------------------------------------------
        // CONSTVAL.Set (2)
        //------------------------------------------------------------
        virtual internal void Set(object obj, PREDEFTYPE pt)
        {
            this.val = obj;
            this.constType = pt;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetByte
        //------------------------------------------------------------
        internal void SetByte(byte bval)
        {
            val = bval;
            constType = PREDEFTYPE.BYTE;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetByte (1)
        //------------------------------------------------------------
        internal byte GetByte(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (byte)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (byte)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (byte)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (byte)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (byte)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (byte)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (byte)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (byte)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (byte)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (byte)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (byte)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (byte)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetByte();
        }


        //------------------------------------------------------------
        // CONSTVAL.GetByte (2)
        //------------------------------------------------------------
        internal byte GetByte()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (byte)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (byte)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (byte)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (byte)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (byte)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (byte)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (byte)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (byte)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (byte)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (byte)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (byte)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (byte)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetShort
        //------------------------------------------------------------
        internal void SetShort(short sval)
        {
            val = sval;
            constType = PREDEFTYPE.SHORT;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetShort (1)
        //------------------------------------------------------------
        internal short GetShort(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (short)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (short)this.val;
                        case PREDEFTYPE.INT:
                            return (short)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (short)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (short)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (short)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (short)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (short)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (short)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (short)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (short)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (short)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (short)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetShort();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetShort (2)
        //------------------------------------------------------------
        internal short GetShort()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (short)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (short)this.val;
                        case PREDEFTYPE.INT:
                            return (short)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (short)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (short)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (short)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (short)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (short)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (short)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (short)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (short)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (short)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (short)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetInt
        //------------------------------------------------------------
        internal void SetInt(int ival)
        {
            val = ival;
            constType = PREDEFTYPE.INT;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetInt (1)
        //------------------------------------------------------------
        internal int GetInt(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (int)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (int)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (int)this.val;
                        case PREDEFTYPE.LONG:
                            return (int)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (int)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (int)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (int)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (int)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (int)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (int)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (int)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (int)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (int)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetInt();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetInt (2)
        //------------------------------------------------------------
        internal int GetInt()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (int)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (int)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (int)this.val;
                        case PREDEFTYPE.LONG:
                            return (int)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (int)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (int)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (int)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (int)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (int)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (int)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (int)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (int)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (int)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                            return 0;
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetLong
        //------------------------------------------------------------
        internal void SetLong(long lval)
        {
            val = lval;
            constType = PREDEFTYPE.LONG;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetLong (1)
        //------------------------------------------------------------
        internal long GetLong(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (long)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (long)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (long)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (long)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (long)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (long)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (long)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (long)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (long)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (long)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (long)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (long)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetLong();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetLong (2)
        //------------------------------------------------------------
        internal long GetLong()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (long)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (long)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (long)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (long)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (long)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (long)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (long)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (long)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (long)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (long)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (long)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (long)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetFloat
        //------------------------------------------------------------
        internal void SetFloat(float fval)
        {
            val = fval;
            constType = PREDEFTYPE.FLOAT;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetFloat (1)
        //------------------------------------------------------------
        internal float GetFloat(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (float)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (float)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (float)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (float)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (float)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (float)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (float)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (float)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (float)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (float)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (float)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (float)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetFloat();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetFloat (2)
        //------------------------------------------------------------
        internal float GetFloat()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (float)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (float)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (float)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (float)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (float)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (float)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (float)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (float)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (float)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (float)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (float)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (float)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetDouble
        //------------------------------------------------------------
        internal void SetDouble(double dval)
        {
            val = dval;
            constType = PREDEFTYPE.DOUBLE;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetDouble (1)
        //------------------------------------------------------------
        internal double GetDouble(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (double)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (double)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (double)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (double)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (double)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (double)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (double)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (double)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (double)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (double)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (double)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (double)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetDouble();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetDouble (2)
        //------------------------------------------------------------
        internal double GetDouble()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (double)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (double)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (double)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (double)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (double)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (double)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (double)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (double)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (double)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (double)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (double)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (double)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetDecimal
        //------------------------------------------------------------
        internal void SetDecimal(decimal dval)
        {
            val = dval;
            constType = PREDEFTYPE.DECIMAL;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetDecimal (1)
        //------------------------------------------------------------
        internal decimal GetDecimal(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (decimal)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (decimal)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (decimal)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (decimal)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (decimal)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (decimal)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (decimal)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (decimal)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (decimal)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (decimal)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (decimal)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (decimal)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetDecimal();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetDecimal (2)
        //------------------------------------------------------------
        internal decimal GetDecimal()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (decimal)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (decimal)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (decimal)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (decimal)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (decimal)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (decimal)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (decimal)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (decimal)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (decimal)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (decimal)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (decimal)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (decimal)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetChar
        //------------------------------------------------------------
        internal void SetChar(char cval)
        {
            val = cval;
            constType = PREDEFTYPE.CHAR;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetChar (1)
        //------------------------------------------------------------
        internal char GetChar(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (char)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (char)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (char)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (char)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (char)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (char)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (char)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (char)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (char)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (char)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (char)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (char)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return (char)0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetChar();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetChar (2)
        //------------------------------------------------------------
        internal char GetChar()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (char)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (char)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (char)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (char)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (char)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (char)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (char)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (char)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (char)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (char)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (char)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (char)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return (char)0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return (char)0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetBool
        //------------------------------------------------------------
        internal void SetBool(bool bval)
        {
            val = bval;
            constType = PREDEFTYPE.BOOL;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetBool (1)
        //------------------------------------------------------------
        internal bool GetBool(out Exception excp)
        {
            excp = null;

            try
            {
                return (bool)val;
            }
            catch (InvalidCastException ex)
            {
                excp = ex;
            }
            return GetBool();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetBool (2)
        //------------------------------------------------------------
        internal bool GetBool()
        {
            try
            {
                unchecked
                {
                    switch (this.constType)
                    {
                        case PREDEFTYPE.BYTE:
                            return ((byte)this.val != 0);
                        case PREDEFTYPE.SHORT:
                            return ((short)this.val != 0);
                        case PREDEFTYPE.INT:
                            return ((int)this.val != 0);
                        case PREDEFTYPE.LONG:
                            return ((long)this.val != 0);
                        case PREDEFTYPE.FLOAT:
                            return ((float)this.val != 0);
                        case PREDEFTYPE.DOUBLE:
                            return ((double)this.val != 0);
                        case PREDEFTYPE.DECIMAL:
                            return ((decimal)this.val != 0);
                        case PREDEFTYPE.CHAR:
                            return ((char)this.val != 0);
                        case PREDEFTYPE.BOOL:
                            return (bool)this.val;
                        case PREDEFTYPE.SBYTE:
                            return ((sbyte)this.val != 0);
                        case PREDEFTYPE.USHORT:
                            return ((ushort)this.val != 0);
                        case PREDEFTYPE.UINT:
                            return ((uint)this.val != 0);
                        case PREDEFTYPE.ULONG:
                            return ((ulong)this.val != 0);
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return this.val != null;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return false;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetSByte
        //------------------------------------------------------------
        internal void SetSByte(sbyte sbval)
        {
            val = sbval;
            constType = PREDEFTYPE.SBYTE;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetSByte (1)
        //------------------------------------------------------------
        internal sbyte GetSByte(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (sbyte)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (sbyte)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (sbyte)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (sbyte)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (sbyte)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (sbyte)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (sbyte)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (sbyte)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (sbyte)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (sbyte)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (sbyte)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (sbyte)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetSByte();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetSByte (2)
        //------------------------------------------------------------
        internal sbyte GetSByte()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (sbyte)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (sbyte)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (sbyte)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (sbyte)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (sbyte)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (sbyte)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (sbyte)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (sbyte)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (sbyte)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (sbyte)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (sbyte)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (sbyte)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetUShort
        //------------------------------------------------------------
        internal void SetUShort(ushort usval)
        {
            val = usval;
            constType = PREDEFTYPE.USHORT;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetUShort (1)
        //------------------------------------------------------------
        internal ushort GetUShort(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (ushort)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (ushort)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (ushort)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (ushort)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (ushort)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (ushort)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (ushort)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (ushort)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (ushort)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (ushort)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (ushort)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (ushort)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetUShort();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetUShort (2)
        //------------------------------------------------------------
        internal ushort GetUShort()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (ushort)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (ushort)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (ushort)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (ushort)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (ushort)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (ushort)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (ushort)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (ushort)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (ushort)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (ushort)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (ushort)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (ushort)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetUInt
        //------------------------------------------------------------
        internal void SetUInt(uint uival)
        {
            val = uival;
            constType = PREDEFTYPE.UINT;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetUInt (1)
        //------------------------------------------------------------
        internal uint GetUInt(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (uint)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (uint)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (uint)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (uint)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (uint)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (uint)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (uint)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (uint)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (uint)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (uint)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (uint)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (uint)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetUInt();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetUInt (2)
        //------------------------------------------------------------
        internal uint GetUInt()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (uint)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (uint)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (uint)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (uint)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (uint)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (uint)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (uint)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (uint)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (uint)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (uint)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (uint)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (uint)(ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetULong
        //------------------------------------------------------------
        internal void SetULong(ulong ulval)
        {
            val = ulval;
            constType = PREDEFTYPE.ULONG;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetULong (1)
        //------------------------------------------------------------
        internal ulong GetULong(out Exception excp)
        {
            PREDEFTYPE pt = ValidateConstPredefType();
            excp = null;

            try
            {
                checked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (ulong)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (ulong)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (ulong)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (ulong)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (ulong)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (ulong)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (ulong)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (ulong)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (ulong)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (ulong)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (ulong)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (ulong)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return GetULong();
        }

        //------------------------------------------------------------
        // CONSTVAL.GetULong (2)
        //------------------------------------------------------------
        internal ulong GetULong()
        {
            PREDEFTYPE pt = ValidateConstPredefType();

            try
            {
                unchecked
                {
                    switch (pt)
                    {
                        case PREDEFTYPE.BYTE:
                            return (ulong)(byte)this.val;
                        case PREDEFTYPE.SHORT:
                            return (ulong)(short)this.val;
                        case PREDEFTYPE.INT:
                            return (ulong)(int)this.val;
                        case PREDEFTYPE.LONG:
                            return (ulong)(long)this.val;
                        case PREDEFTYPE.FLOAT:
                            return (ulong)(float)this.val;
                        case PREDEFTYPE.DOUBLE:
                            return (ulong)(double)this.val;
                        case PREDEFTYPE.DECIMAL:
                            return (ulong)(decimal)this.val;
                        case PREDEFTYPE.CHAR:
                            return (ulong)(char)this.val;
                        case PREDEFTYPE.BOOL:
                            return (ulong)((bool)val == true ? 1 : 0);
                        case PREDEFTYPE.SBYTE:
                            return (ulong)(sbyte)this.val;
                        case PREDEFTYPE.USHORT:
                            return (ulong)(ushort)this.val;
                        case PREDEFTYPE.UINT:
                            return (ulong)(uint)this.val;
                        case PREDEFTYPE.ULONG:
                            return (ulong)this.val;
                        case PREDEFTYPE.OBJECT:
                        case PREDEFTYPE.STRING:
                        default:
                            return 0;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetObject
        //
        /// <summary>
        /// <para>In sscli, use init member of union to initialize.
        /// In this program, use ObjectValue property instead.</para>
        /// <para>no set accessor. use Set method.</para>
        /// </summary>
        //------------------------------------------------------------
        internal object GetObject()
        {
            return val;
        }

        //------------------------------------------------------------
        // CONSTVAL.SetObject
        //------------------------------------------------------------
        virtual internal void SetObject(object obj)
        {
            if (obj == null)
            {
                return;
            }

            Type type = obj.GetType();
            string str = obj as string;
            if (str != null)
            {
                this.SetString(str);
            }
            else if (type == typeBool)
            {
                this.SetBool((bool)obj);
            }
            else if (type == typeInt)
            {
                this.SetInt((int)obj);
            }
            else if (type == typeChar)
            {
                this.SetChar((char)obj);
            }
            else if (type == typeLong)
            {
                this.SetLong((long)obj);
            }
            else if (type == typeDouble)
            {
                this.SetDouble((double)obj);
            }
            else if (type == typeUInt)
            {
                this.SetUInt((uint)obj);
            }
            else if (type == typeULong)
            {
                this.SetULong((ulong)obj);
            }
            else if (type == typeByte)
            {
                this.SetByte((byte)obj);
            }
            else if (type == typeShort)
            {
                this.SetShort((short)obj);
            }
            else if (type == typeSByte)
            {
                this.SetSByte((sbyte)obj);
            }
            else if (type == typeUShort)
            {
                this.SetUShort((ushort)obj);
            }
            else if (type == typeFloat)
            {
                this.SetFloat((float)obj);
            }
            else if (type == typeDecimal)
            {
                this.SetDecimal((decimal)obj);
            }
            else
            {
                this.val = obj;
                this.constType = PREDEFTYPE.OBJECT;
            }
        }

        //------------------------------------------------------------
        // CONSTVAL.SetString
        //------------------------------------------------------------
        internal void SetString(string sval)
        {
            val = sval;
            constType = PREDEFTYPE.STRING;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetString (1)
        //------------------------------------------------------------
        internal string GetString(out Exception excp)
        {
            excp = null;

            try
            {
                if (constType == PREDEFTYPE.STRING)
                {
                    return (string)this.val;
                }
            }
            catch (InvalidCastException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // CONSTVAL.GetString (2)
        //------------------------------------------------------------
        internal string GetString()
        {
            if (constType == PREDEFTYPE.STRING)
            {
                return this.val as string;
            }
            return null;
        }
    }

    //======================================================================
    //  class ConstValInit
    //======================================================================
    internal class ConstValInit : CONSTVAL
    {
        internal ConstValInit() { }
        internal ConstValInit(bool val) { this.SetBool(val); }
        internal ConstValInit(int val) { this.SetInt(val); }
        internal ConstValInit(uint val) { this.SetUInt(val); }
        internal ConstValInit(string val) { this.SetString(val); }
    }
}
