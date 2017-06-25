//============================================================================
// Misc.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // WINERROR
    //======================================================================
    //internal class WINERROR
    //{
    //    internal const int ERROR_SUCCESS = 0;
    //    internal const int NO_ERROR = 0;
    //}

    //======================================================================
    // COM 対応
    //======================================================================
    //internal class HRESULT
    //{
    //    internal const int S_OK = 0x0;	//	Success.
    //    internal const int S_FALSE = 0x1;	//	Success.
    //    internal const int E_PENDING = unchecked((int)0x8000000A);	//	The data necessary to complete this operation is not yet available.
    //    internal const int E_BOUNDS = unchecked((int)0x8000000B);	//	The operation attempted to access data outside the valid range
    //    internal const int E_CHANGED_STATE = unchecked((int)0x8000000C);	//	A concurrent or interleaved operation changed the state of the object
    //    internal const int E_ILLEGAL_STATE_CHANGE = unchecked((int)0x8000000D);	//	An illegal state change was requested.
    //    internal const int E_ILLEGAL_METHOD_CALL = unchecked((int)0x8000000E);	//	A method was called at an unexpected time.
    //    internal const int E_NOTIMPL = unchecked((int)0x80004001);	//	Not implemented
    //    internal const int E_NOINTERFACE = unchecked((int)0x80004002);	//	No such interface supported
    //    internal const int E_POINTER = unchecked((int)0x80004003);	//	Invalid pointer
    //    internal const int E_ABORT = unchecked((int)0x80004004);	//	Operation aborted
    //    internal const int E_FAIL = unchecked((int)0x80004005);	//	Unspecified error
    //    internal const int E_UNEXPECTED = unchecked((int)0x8000FFFF);	//	Catastrophic failure
    //    internal const int E_ACCESSDENIED = unchecked((int)0x80070005);	//	General access denied error
    //    internal const int E_HANDLE = unchecked((int)0x80070006);	//	Invalid handle
    //    internal const int E_OUTOFMEMORY = unchecked((int)0x8007000E);	//	Ran out of memory
    //    internal const int E_INVALIDARG = unchecked((int)0x80070057);	//	One or more arguments are invalid

    //    // palrt\inc\rotor_palrt.h(346...)
    //    internal const int FACILITY_WINDOWS = unchecked((int)8);
    //    internal const int FACILITY_URT = unchecked((int)19);
    //    internal const int FACILITY_UMI = unchecked((int)22);
    //    internal const int FACILITY_SXS = unchecked((int)23);
    //    internal const int FACILITY_STORAGE = unchecked((int)3);
    //    internal const int FACILITY_SSPI = unchecked((int)9);
    //    internal const int FACILITY_SCARD = unchecked((int)16);
    //    internal const int FACILITY_SETUPAPI = unchecked((int)15);
    //    internal const int FACILITY_SECURITY = unchecked((int)9);
    //    internal const int FACILITY_RPC = unchecked((int)1);
    //    internal const int FACILITY_WIN32 = unchecked((int)7);
    //    internal const int FACILITY_CONTROL = unchecked((int)10);
    //    internal const int FACILITY_NULL = unchecked((int)0);
    //    internal const int FACILITY_MSMQ = unchecked((int)14);
    //    internal const int FACILITY_MEDIASERVER = unchecked((int)13);
    //    internal const int FACILITY_INTERNET = unchecked((int)12);
    //    internal const int FACILITY_ITF = unchecked((int)4);
    //    internal const int FACILITY_DPLAY = unchecked((int)21);
    //    internal const int FACILITY_DISPATCH = unchecked((int)2);
    //    internal const int FACILITY_COMPLUS = unchecked((int)17);
    //    internal const int FACILITY_CERT = unchecked((int)11);
    //    internal const int FACILITY_ACS = unchecked((int)20);
    //    internal const int FACILITY_AAF = unchecked((int)18);

    //    internal const int NO_ERROR = unchecked((int)0);

    //    internal const int SEVERITY_SUCCESS = unchecked((int)0);
    //    internal const int SEVERITY_ERROR = unchecked((int)1);

    //    static internal bool SUCCEEDED(int hr) { return (hr >= 0); }
    //    static internal bool SUCCEEDED(uint hr) { return (((int)hr) >= 0); }

    //    static internal bool FAILED(int hr) { return (hr < 0); }
    //    static internal bool FAILED(uint hr) { return (((int)hr) < 0); }

    //    // palrt\inc\rotor_palrt.h(380): #define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)
    //    static internal int FACILITY(int i)
    //    {
    //        return (int)(((i) >> 16) & 0x1fff);
    //    }
    //}

    //======================================================================
    // class SystemType
    //
    /// <summary>
    /// A utility class for System.Type
    /// </summary>
    //======================================================================
    internal static class SystemType
    {
        internal static readonly Type VoidType = typeof(void);
        internal static readonly Type ByteType = typeof(byte);
        internal static readonly Type ShortType = typeof(short);
        internal static readonly Type IntType = typeof(int);
        internal static readonly Type LongType = typeof(long);
        internal static readonly Type FloatType = typeof(float);
        internal static readonly Type DoubleType = typeof(double);
        internal static readonly Type DecimalType = typeof(decimal);
        internal static readonly Type CharType = typeof(char);
        internal static readonly Type BoolType = typeof(bool);
        internal static readonly Type SByteType = typeof(sbyte);
        internal static readonly Type UShortType = typeof(ushort);
        internal static readonly Type UIntType = typeof(uint);
        internal static readonly Type ULongType = typeof(ulong);
        internal static readonly Type ObjectType = typeof(object);
        internal static readonly Type StringType = typeof(string);

        internal static readonly Assembly MsCorLib = Assembly.GetAssembly(typeof(object));
        private static Type nullableType = null;

        internal static Type GetNullable()
        {
            if (nullableType == null)
            {
                nullableType = MsCorLib.GetType("System.Nullable`1");
            }
            DebugUtil.Assert(nullableType != null);
            return nullableType;
        }
    }

    //======================================================================
    // partial class Util
    //======================================================================
    static internal partial class Util
    {
        // uncs\CSharp\sccomp\ から
        /// <summary>
        /// <para>Maximum identifier size we allow.  This is the max the compiler allows</para>
        /// <para>512 for now.</para>
        /// </summary>
        //internal const int MAX_IDENT_SIZE = 512;

        // Maximum namespace or fully qualified identifier size we allow. Comes from corhdr.h
        //internal const int MAX_FULLNAME_SIZE = (MAX_CLASS_NAME);

        //============================================================
        // For hash values
        //============================================================

        //------------------------------------------------------------
        // MakeHashCode
        //
        /// <summary>
        /// <para>Return hash code of type integer from multiple object.</para>
        /// <para>(PCSUtility.cs)</para>
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns>31 bits integer.</returns>
        //------------------------------------------------------------
        internal static int MakeHashCode(Object obj1, Object obj2)
        {
            uint h1 = (uint)(obj1 != null ? obj1.GetHashCode() : 0);
            uint h2 = (uint)(obj2 != null ? obj2.GetHashCode() : 0);
            return (int)((h1 + h2) & 0x7FFFFFFF);
        }

        //------------------------------------------------------------
        // UInt32x32To64
        //
        /// <summary>
        /// Create a value of type long from two values of type int.
        /// </summary>
        //------------------------------------------------------------
        static internal long UInt32x32To64(int a, int b)
        {
            return unchecked((long)a * (long)b);
        }

        //------------------------------------------------------------
        // HashInt
        //
        /// <summary>
        /// A generally useful hashing function. Uses no state.
        /// </summary>
        /// <remarks>
        /// Hash a 32-bit value to get a much more randomized 32-bit value.
        /// (Uses 64-bit values on Win64) Inlined so that
        /// </remarks>
        //------------------------------------------------------------
        static internal int HashInt(int i)
        {
            // Equivalent portable code.
            //long l = UInt32x32To64(u, 0x7ff19519U);  // this number is prime.
            long l = (long)i * (long)0x7ff19519;
            return (int)l + (int)(l >> 32);
        }

        //============================================================
        // For string
        //============================================================

        //------------------------------------------------------------
        // Util.SplitString
        //
        /// <summary></summary>
        /// <param name="str"></param>
        /// <param name="separators"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static List<string> SplitString(
            string str,
            string separators,
            bool eliminateEmptyString)
        {
            List<string> split1 = new List<string>();
            if (str == null)
            {
                return split1;
            }

            List<string> split2 = new List<string>();
            int pos = 0;
            int start = 0;
            bool inQuote = false;

            while (pos < str.Length)
            {
                char ch = str[pos];
                switch (ch)
                {
                    case '\\':
                        ++pos;
                        if (pos < str.Length)
                        {
                            ++pos;
                        }
                        break;

                    case '"':
                        inQuote = !inQuote;
                        ++pos;
                        break;

                    default:
                        if (separators.IndexOf(ch) >= 0 && !inQuote)
                        {
                            split2.Add(str.Substring(start, pos - start));
                            ++pos;
                            start = pos;
                        }
                        else
                        {
                            ++pos;
                        }
                        break;
                }
            }
            split2.Add(str.Substring(start, pos - start));

            for (int i = 0; i < split2.Count; ++i)
            {
                string seg = split2[i];
                seg = seg.Trim();
                IOUtil.RemoveQuotes(ref seg);
                if (!String.IsNullOrEmpty(seg) || !eliminateEmptyString)
                {
                    split1.Add(seg);
                }
            }
            return split1;
        }

        //------------------------------------------------------------
        // Util.SplitStrings
        //
        /// <summary></summary>
        /// <param name="strs"></param>
        /// <param name="separators"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static List<string> SplitStrings(
            string[] strs,
            string separators,
            bool eliminateEmptyString)
        {
            List<string> split = new List<string>();

            for (int i = 0; i < strs.Length; ++i)
            {
                split.AddRange(SplitString(strs[i], separators, eliminateEmptyString));
            }
            return split;
        }

        //------------------------------------------------------------
        // Util.StringToInt32
        //
        /// <summary></summary>
        /// <param name="src"></param>
        /// <param name="val"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool StringToInt32(string src, out int val, out Exception excp)
        {
            val = 0;
            excp = null;
            if (String.IsNullOrEmpty(src))
            {
                return false;
            }

            int basenum = 10;
            if (src.Length > 2 &&
                src[0] == '0' &&
                (src[1] == 'x' || src[1] == 'X'))
            {
                basenum = 16;
            }
            else if (src[0] == '0')
            {
                basenum = 8;
            }

            try
            {
                val = Convert.ToInt32(src, basenum);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (FormatException ex)
            {
                excp = ex;
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // Util.StringToUInt32
        //
        /// <summary></summary>
        /// <param name="src"></param>
        /// <param name="val"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool StringToUInt32(string src, out uint val, out Exception excp)
        {
            val = 0;
            excp = null;
            if (String.IsNullOrEmpty(src))
            {
                return false;
            }

            int basenum = 10;
            if (src.Length > 2 &&
                src[0] == '0' &&
                (src[1] == 'x' || src[1] == 'X'))
            {
                basenum = 16;
            }
            else if (src[0] == '0')
            {
                basenum = 8;
            }

            try
            {
                val = Convert.ToUInt32(src, basenum);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (FormatException ex)
            {
                excp = ex;
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // Util.StringToUInt64
        //
        /// <summary></summary>
        /// <param name="src"></param>
        /// <param name="val"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool StringToUInt64(string src, out ulong val, out Exception excp)
        {
            val = 0;
            excp = null;
            if (String.IsNullOrEmpty(src))
            {
                return false;
            }

            int basenum = 10;
            if (src.Length > 2 &&
                src[0] == '0' &&
                (src[1] == 'x' || src[1] == 'X'))
            {
                basenum = 16;
            }
            else if (src[0] == '0')
            {
                basenum = 8;
            }

            try
            {
                val = Convert.ToUInt64(src, basenum);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (FormatException ex)
            {
                excp = ex;
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return false;
        }

        //============================================================
        // For interger
        //============================================================

        //============================================================
        // For floating-point number
        //============================================================

        //------------------------------------------------------------
        // Util.IsFinite (float)
        //
        /// <summary></summary>
        /// <param name="n"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsFinite(float n)
        {
            return (!Single.IsInfinity(n) && !Single.IsNaN(n));
        }

        //------------------------------------------------------------
        // Util.IsFinite (double)
        //
        /// <summary></summary>
        /// <param name="n"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsFinite(double n)
        {
            return (!Double.IsInfinity(n) && !Double.IsNaN(n));
        }

        //------------------------------------------------------------
        // GetDecimalBits
        //
        /// <summary></summary>
        /// <param name="decValue"></param>
        /// <param name="high"></param>
        /// <param name="middle"></param>
        /// <param name="low"></param>
        /// <param name="negative"></param>
        /// <param name="exponent"></param>
        //------------------------------------------------------------
        static internal void GetDecimalBits(
            Decimal decValue,
            out int low,
            out int middle,
            out int high,
            out bool negative,
            out byte exponent)
        {
            int[] elements = Decimal.GetBits(decValue);
            low = elements[0];
            middle = elements[1];
            high = elements[2];
            negative = ((((uint)elements[3]) & 0x80000000) != 0);
            exponent = (byte)(((uint)elements[3] >> 16) & 0xFF);
        }

        //------------------------------------------------------------
        // GetDecimalConstantAttributeArguments
        //
        /// <summary></summary>
        /// <param name="decValue"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal object[] GetDecimalConstantAttributeArguments(
            ConstructorInfo cInfo,
            Decimal decValue)
        {
            int low, middle, high;
            bool negative;
            byte exponent;
            GetDecimalBits(decValue, out low, out middle, out high, out negative, out exponent);

            ParameterInfo[] paramType = cInfo.GetParameters();
            bool isUInt = false;
            if (paramType[2].ParameterType == typeof(uint))
            {
                isUInt = true;
            }

            object[] args = new object[5];
            args[0] = exponent;
            args[1] = (byte)(negative ? 1 : 0);
            if (isUInt)
            {
                args[2] = (uint)high;
                args[3] = (uint)middle;
                args[4] = (uint)low;
            }
            else
            {
                args[2] = high;
                args[3] = middle;
                args[4] = low;
            }

            return args;
        }

        //============================================================
        // For array or list
        //============================================================
        static internal List<T> RemoveDuplicateFromArray<T>(T[] tarray)
        {
            if (tarray == null) return null;
            List<T> tlist = new List<T>();
            for (int i = 0; i < tarray.Length; ++i)
            {
                if (tarray[i] == null) continue;
                bool dup = false;
                for (int j = 0; j < tlist.Count; ++j)
                {
                    if (tarray[i].Equals(tarray[j]))
                    {
                        dup = true;
                        break;
                    }
                }
                if (dup == false) tlist.Add(tarray[i]);
            }
            return tlist;
        }

        //============================================================
        // etc
        //============================================================
        //------------------------------------------------------------
        // Util.Swap
        //
        /// <summary>
        /// <para>Defined in Utilities\Misc.cs (csharp\inc\unimisc.h in sscli)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        //------------------------------------------------------------
        static internal void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        //------------------------------------------------------------
        // Util.CreateVersion
        //
        /// <summary></summary>
        /// <param name="str"></param>
        /// <param name="verObj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool CreateVersion(
            string str,
            out Version verObj,
            out Exception excp)
        {
            excp = null;

            try
            {
                verObj = new Version(str);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (FormatException ex)
            {
                excp = ex;
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            verObj = null;
            return false;
        }

        //------------------------------------------------------------
        // Util.CreateCultureInfo
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="cInfo"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool CreateCultureInfo(
            string name,
            out CultureInfo cInfo,
            out Exception excp)
        {
            excp = null;

            try
            {
                cInfo = new CultureInfo(name);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            cInfo = null;
            return false;
        }
    }
}
