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
// File: predeftyp.h
//
// Contains a list of the various predefined types.
// ===========================================================================

//============================================================================
// PredefType.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Uncs
{
    //======================================================================
    //  PREDEFTYPE  (PT_)
    //
    /// <summary>
    /// <para>In sscli, defined in csharp\inc\enum.h.</para>
    /// </summary>
    //======================================================================
    internal enum PREDEFTYPE : int
    {
        UNDEFINED = -1,  //  #define UNDEFINEDINDEX 0xffffffff (compiler.h line 51)

        BYTE = 0,                   // BYTE + 0
        SHORT,	                    // BYTE + 1
        INT,	                    // BYTE + 2
        LONG,	                    // BYTE + 3
        FLOAT,	                    // BYTE + 4
        DOUBLE,                     // BYTE + 5
        DECIMAL,	                // BYTE + 6
        CHAR,	                    // BYTE + 7
        BOOL,	                    // BYTE + 8
        SBYTE,	                    // BYTE + 9
        USHORT,	                    // BYTE + 10
        UINT,	                    // BYTE + 11
        ULONG,	                    // BYTE + 12
        INTPTR,	                    // BYTE + 13
        UINTPTR,	                // BYTE + 14
        OBJECT,	                    // BYTE + 15
        STRING,	                    // BYTE + 16
        DELEGATE,	                // BYTE + 17
        MULTIDEL,	                // BYTE + 18
        ARRAY,	                    // BYTE + 19
        EXCEPTION,	                // BYTE + 20
        TYPE,	                    // BYTE + 21
        CRITICAL,	                // BYTE + 22
        VALUE,	                    // BYTE + 23
        ENUM,	                    // BYTE + 24
        SECURITYATTRIBUTE,	        // BYTE + 25
        SECURITYPERMATTRIBUTE,	    // BYTE + 26
        UNVERIFCODEATTRIBUTE,	    // BYTE + 27
        DEBUGGABLEATTRIBUTE,	    // BYTE + 28
        MARSHALBYREF,	            // BYTE + 29
        CONTEXTBOUND,	            // BYTE + 30
        IN,	                        // BYTE + 31
        OUT,	                    // BYTE + 32
        ATTRIBUTE,	                // BYTE + 33
        ATTRIBUTEUSAGE,	            // BYTE + 34
        ATTRIBUTETARGETS,	        // BYTE + 35
        OBSOLETE,	                // BYTE + 36
        CONDITIONAL,	            // BYTE + 37
        CLSCOMPLIANT,	            // BYTE + 38
        GUID,	                    // BYTE + 39
        DEFAULTMEMBER,	            // BYTE + 40
        PARAMS,	                    // BYTE + 41
        COMIMPORT,	                // BYTE + 42
        FIELDOFFSET,	            // BYTE + 43
        STRUCTLAYOUT,	            // BYTE + 44
        LAYOUTKIND,	                // BYTE + 45
        MARSHALAS,	                // BYTE + 46
        DLLIMPORT,	                // BYTE + 47
        INDEXERNAME,	            // BYTE + 48
        DECIMALCONSTANT,	        // BYTE + 49
        REQUIRED,	                // BYTE + 50
        DEFAULTVALUE,	            // BYTE + 51
        UNMANAGEDFUNCTIONPOINTER,   // BYTE + 52
        REFANY,	                    // BYTE + 53
        ARGITERATOR,	            // BYTE + 54
        TYPEHANDLE,	                // BYTE + 55
        FIELDHANDLE,	            // BYTE + 56
        METHODHANDLE,	            // BYTE + 57
        ARGUMENTHANDLE,	            // BYTE + 58
        HASHTABLE,	                // BYTE + 59
        G_DICTIONARY,	            // BYTE + 60
        IASYNCRESULT,	            // BYTE + 61
        ASYNCCBDEL,	                // BYTE + 62
        SECURITYACTION,	            // BYTE + 63
        IDISPOSABLE,	            // BYTE + 64
        IENUMERABLE,	            // BYTE + 65
        IENUMERATOR,	            // BYTE + 66
        SYSTEMVOID,	                // BYTE + 67
        RUNTIMEHELPERS,	            // BYTE + 68
        VOLATILEMOD,	            // BYTE + 69
        COCLASS,	                // BYTE + 70
        ACTIVATOR,	                // BYTE + 71
        G_IENUMERABLE,	            // BYTE + 72
        G_IENUMERATOR,	            // BYTE + 73
        G_OPTIONAL,	                // BYTE + 74
        FIXEDBUFFER,	            // BYTE + 75
        DEFAULTCHARSET,	            // BYTE + 76
        COMPILATIONRELAXATIONS,	    // BYTE + 77
        RUNTIMECOMPATIBILITY,	    // BYTE + 78
        FRIENDASSEMBLY,	            // BYTE + 79
        DEBUGGERHIDDEN,	            // BYTE + 80
        TYPEFORWARDER,	            // BYTE + 81
        KEYFILE,	                // BYTE + 82
        KEYNAME,	                // BYTE + 83
        DELAYSIGN,	                // BYTE + 84
        NOTSUPPORTEDEXCEPTION,	    // BYTE + 85
        INTERLOCKED,	            // BYTE + 86
        COMPILERGENERATED,	        // BYTE + 87
        UNSAFEVALUETYPE,	        // BYTE + 88
        G_ICOLLECTION,	            // BYTE + 89
        G_ILIST,	                // BYTE + 90

        // CS3
        G1_FUNC,                    // BYTE + 91
        G2_FUNC,                    // BYTE + 92
        G3_FUNC,                    // BYTE + 93
        G4_FUNC,                    // BYTE + 94
        G5_FUNC,                    // BYTE + 95
        G_EQUALITYCOMPARER,         // BYTE + 96
        LINQ_ENUMERABLE,            // BYTE + 97
        LINQ_IGROUPING,                 // BYTE + 98
        LINQ_EXPRESSIONS_EXPRESSION,    // BYTE + 99
        LINQ_EXPRESSIONS_G_EXPRESSION,  // BYTE + 100
        G_ICOMPARER,                    // BYTE + 101
        G_IEQUALITYCOMPARER,            // BYTE + 102
        QUERYABLE,                      // BYTE + 103
        IQUERYABLE,                     // BYTE + 104
        G_IQUERYABLE,                   // BYTE + 105

        // CS4
        ACTION,                         // BYTE + 106
        G1_ACTION,                      // BYTE + 107
        G2_ACTION,                      // BYTE + 108
        G3_ACTION,                      // BYTE + 109
        G4_ACTION,                      // BYTE + 110
        G5_ACTION,                      // BYTE + 111
        G6_ACTION,                      // BYTE + 112
        G7_ACTION,                      // BYTE + 113
        G8_ACTION,                      // BYTE + 114
        G9_ACTION,                      // BYTE + 115
        G10_ACTION,                     // BYTE + 116
        G11_ACTION,                     // BYTE + 117
        G12_ACTION,                     // BYTE + 118
        G13_ACTION,                     // BYTE + 119
        G14_ACTION,                     // BYTE + 120
        G15_ACTION,                     // BYTE + 121
        G16_ACTION,                     // BYTE + 122
        CALLSITE,                       // BYTE + 123
        G_CALLSITE,                     // BYTE + 124
        CALLSITEBINDER,                 // BYTE + 125
        CSHARPARGUMENTINFO,             // BYTE + 126
        RUNTIMEBINDER_BINDER,           // BYTE + 127
        CSHARPARGUMENTINFOFLAGS,        // BYTE + 128
        CSHARPBINDERFLAGS,              // BYTE + 129
        LINQ_EXPRESSIONTYPE,            // BYTE + 130

        COUNT,	                    // BYTE + 131
        VOID,	                    // BYTE + 132
    }

    //======================================================================
    // class PredefTypeInfo
    //
    /// <summary>
    /// Predefined type information.
    /// </summary>
    //======================================================================
    internal class PredefTypeInfo
    {
        internal string fullName;
        internal bool isRequired;
        internal bool isSimple;
        internal bool isNumeric;
        internal bool isQSimple;
        internal FUNDTYPE ft;
        internal CorElementType et;
        internal AggKindEnum aggKind;
        internal string niceName;
        internal Object zero;
        internal PREDEFATTR attr;

        /// <summary>
        /// <para>char, number (except decimal): number of bytes.</para>
        /// <para>System.Object, System.String, System.Type: -1</para>
        /// </summary>
        internal int asize;

        internal int arity;
        internal int inmscorlib;

        //------------------------------------------------------------
        // PredefTypeInfo Constructor
        //------------------------------------------------------------
        internal PredefTypeInfo(
            string fullName,
            bool isRequired,
            bool isSimple,
            bool isNumeric,
            bool isQSimple,
            FUNDTYPE ft,
            CorElementType et,
            AggKindEnum aggKind,
            string niceName,
            Object zero,
            PREDEFATTR attr,
            int asize,
            int arity,
            int inmscorlib)
        {
            this.fullName = fullName;
            this.isRequired = isRequired;
            this.isSimple = isSimple;
            this.isNumeric = isNumeric;
            this.isQSimple = isQSimple;
            this.ft = ft;
            this.et = et;
            this.aggKind = aggKind;
            this.niceName = niceName;
            this.zero = zero;
            this.attr = attr;
            this.asize = asize;
            this.arity = arity;
            this.inmscorlib = inmscorlib;
        }
    }

    //======================================================================
    // class PredefType
    //
    /// <summary></summary>
    //======================================================================
    static internal partial class PredefType
    {
        //------------------------------------------------------------
        // PredefType.NameTable
        //
        // csharp\shared\node.cpp (30 行から)
        //
        // struct PTNAME
        // {
        //     PCWSTR  pszFull;
        //     PCWSTR  pszNice;
        // };
        //
        // #define PREDEFTYPEDEF( \
        //     id, name, required, simple, numeric, kind, ft, et, nicename, zero, qspec, asize, st, attr, arity, inmscorlib \
        // ) \
        // { L##name, nicename },
        // static  const   PTNAME  rgPredefNames[] = {
        // #include "predeftype.h"
        //     { L"", NULL},
        //     { L"System.Void", L"void"}
        // };
        //
        // これは既定の型名のデータだが、元のソースでは rgPredefNames という名前が付けられている。
        // だが既定の名前のデータにも predefname が使われている。
        // ここでは混乱を避けるため name ではなく type を使うことにする。
        //
        /// <summary>
        /// <para>既定の型名を格納した string[ ,2] 型の配列。
        /// それぞれの name と nicename が入っている。
        /// (CSharp\inc\PredefType.cs)</para>
        /// <para>元のソースでは rgPredefNames という名前の PTNAME 型配列。</para>
        /// </summary>
        //------------------------------------------------------------
        static internal string[,] NameTable =
        {
            //{null,null},    // UNDEFINED

            { "System.Byte", "byte" },
            { "System.Int16", "short" },
            { "System.Int32", "int" },
            { "System.Int64", "long" },
            { "System.Single", "float" },
            { "System.Double", "double" },
            { "System.Decimal", "decimal" },
            { "System.Char", "char" },
            { "System.Boolean", "bool" },
            { "System.SByte", "sbyte" },
            { "System.UInt16", "ushort" },
            { "System.UInt32", "uint" },
            { "System.UInt64", "ulong" },
            { "System.IntPtr", null },
            { "System.UIntPtr", null },
            { "System.Object", "object" },
            { "System.String", "string" },
            { "System.Delegate", null },
            { "System.MulticastDelegate", null },
            { "System.Array", null },
            { "System.Exception", null },
            { "System.Type", null },
            { "System.Threading.Monitor", null },
            { "System.ValueType", null },
            { "System.Enum", null },
            { "System.Security.Permissions.CodeAccessSecurityAttribute", null },
            { "System.Security.Permissions.SecurityPermissionAttribute", null },
            { "System.Security.UnverifiableCodeAttribute", null },
            { "System.Diagnostics.DebuggableAttribute", null },
            { "System.MarshalByRefObject", null },
            { "System.ContextBoundObject", null },
            { "System.Runtime.InteropServices.InAttribute", null },
            { "System.Runtime.InteropServices.OutAttribute", null },
            { "System.Attribute", null },
            { "System.AttributeUsageAttribute", null },
            { "System.AttributeTargets", null },
            { "System.ObsoleteAttribute", null },
            { "System.Diagnostics.ConditionalAttribute", null },
            { "System.CLSCompliantAttribute", null },
            { "System.Runtime.InteropServices.GuidAttribute", null },
            { "System.Reflection.DefaultMemberAttribute", null },
            { "System.ParamArrayAttribute", null },
            { "System.Runtime.InteropServices.ComImportAttribute", null },
            { "System.Runtime.InteropServices.FieldOffsetAttribute", null },
            { "System.Runtime.InteropServices.StructLayoutAttribute", null },
            { "System.Runtime.InteropServices.LayoutKind", null },
            { "System.Runtime.InteropServices.MarshalAsAttribute", null },
            { "System.Runtime.InteropServices.DllImportAttribute", null },
            { "System.Runtime.CompilerServices.IndexerNameAttribute", null },
            { "System.Runtime.CompilerServices.DecimalConstantAttribute", null },
            { "System.Runtime.CompilerServices.RequiredAttributeAttribute", null },
            { "System.Runtime.InteropServices.DefaultParameterValueAttribute", null },
            { "System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute", null },
            { "System.TypedReference", null },
            { "System.ArgIterator", null },
            { "System.RuntimeTypeHandle", null },
            { "System.RuntimeFieldHandle", null },
            { "System.RuntimeMethodHandle", null },
            { "System.RuntimeArgumentHandle", null },
            { "System.Collections.Hashtable", null },
            { "System.Collections.Generic.Dictionary", null },
            { "System.IAsyncResult", null },
            { "System.AsyncCallback", null },
            { "System.Security.Permissions.SecurityAction", null },
            { "System.IDisposable", null },
            { "System.Collections.IEnumerable", null },
            { "System.Collections.IEnumerator", null },
            { "System.Void", null },
            { "System.Runtime.CompilerServices.RuntimeHelpers", null },
            { "System.Runtime.CompilerServices.IsVolatile", null },
            { "System.Runtime.InteropServices.CoClassAttribute", null },
            { "System.Activator", null },
            { "System.Collections.Generic.IEnumerable", null },
            { "System.Collections.Generic.IEnumerator", null },
            { "System.Nullable", null },
            { "System.Runtime.CompilerServices.FixedBufferAttribute", null },
            { "System.Runtime.InteropServices.DefaultCharSetAttribute", null },
            { "System.Runtime.CompilerServices.CompilationRelaxationsAttribute", null },
            { "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute", null },
            { "System.Runtime.CompilerServices.InternalsVisibleToAttribute", null },
            { "System.Diagnostics.DebuggerHiddenAttribute", null },
            { "System.Runtime.CompilerServices.TypeForwardedToAttribute", null },
            { "System.Reflection.AssemblyKeyFileAttribute", null },
            { "System.Reflection.AssemblyKeyNameAttribute", null },
            { "System.Reflection.AssemblyDelaySignAttribute", null },
            { "System.NotSupportedException", null },
            { "System.Threading.Interlocked", null },
            { "System.Runtime.CompilerServices.CompilerGeneratedAttribute", null },
            { "System.Runtime.CompilerServices.UnsafeValueTypeAttribute", null },
            { "System.Collections.Generic.ICollection", null },
            { "System.Collections.Generic.IList", null },   // 90

            // CS3
            { "System.Func", null },            // 91
            { "System.Func", null },            // 92
            { "System.Func", null },            // 93
            { "System.Func", null },            // 94
            { "System.Func", null },            // 95
            { "System.Collections.Generic.EqualityComparer", null },    // 96
            { "System.Linq.Enumerable", null },                         // 97
            { "System.Linq.IGrouping", null },                          // 98
            { "System.Linq.Expressions.Expression", null },             // 99
            { "System.Linq.Expressions.Expression", null },             // 100
            { "System.Collections.Generic.IComparer", null },           // 101
            { "System.Collections.Generic.IEqualityComparer", null },   // 102
            { "System.Linq.Queryable", null },                          // 103
            { "System.Linq.IQueryable", null },                         // 104
            { "System.Linq.IQueryable", null },                         // 105

            // CS4
            { "System.Action", null },            // 106
            { "System.Action", null },            // 107
            { "System.Action", null },            // 108
            { "System.Action", null },            // 109
            { "System.Action", null },            // 110
            { "System.Action", null },            // 111
            { "System.Action", null },            // 112
            { "System.Action", null },            // 113
            { "System.Action", null },            // 114
            { "System.Action", null },            // 115
            { "System.Action", null },            // 116
            { "System.Action", null },            // 117
            { "System.Action", null },            // 118
            { "System.Action", null },            // 119
            { "System.Action", null },            // 120
            { "System.Action", null },            // 121
            { "System.Action", null },            // 122
            { "System.Runtime.CompilerServices.CallSite", null },               // 123
            { "System.Runtime.CompilerServices.CallSite", null },               // 124
            { "System.Runtime.CompilerServices.CallSiteBinder", null },         // 125
            { "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo", null },      // 126
            { "Microsoft.CSharp.RuntimeBinder.Binder" , null},                  // 127
            { "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags" , null}, // 128
            { "Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags" , null},       // 129
            { "System.Linq.Expressions.ExpressionType" , null},                 // 130

            { "", null},
            { "System.Void", "void"},
        };

        //------------------------------------------------------------
        // PredefType.SerializationTypeTable
        //
        // sccomp\metaattr.cpp（851 行から）
        //
        // #define PREDEFTYPEDEF( \
        //     id, name, isRequired, isSimple, isNumeric, kind, ft, et, nicename, zero, qspec, asize, st, attr, arity, inmscorlib \
        // ) (CorSerializationType)st,
        // CorSerializationType serializationTypes[] =
        // {
        // #include "predeftype.h"
        // };
        //
        /// <summary>
        /// <para>serializationTypes in sscli (sccomp\metaattr.cpp)</para>
        /// <para>(CSharp\SCComp\PredefType.cs)</para>
        /// </summary>
        //------------------------------------------------------------
        static internal CorSerializationType[] SerializationTypeTable =
        {
            //(CorSerializationType)0,  // UNDEFINED

            CorSerializationType.U1,    // 0
            CorSerializationType.I2,
            CorSerializationType.I4,
            CorSerializationType.I8,
            CorSerializationType.R4,
            CorSerializationType.R8,
            (CorSerializationType)0,
            CorSerializationType.CHAR,
            CorSerializationType.BOOLEAN,
            CorSerializationType.I1,
            CorSerializationType.U2,
            CorSerializationType.U4,
            CorSerializationType.U8,
            (CorSerializationType)0,
            (CorSerializationType)0,
            CorSerializationType.TAGGED_OBJECT,
            CorSerializationType.STRING,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            CorSerializationType.TYPE,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,
            (CorSerializationType)0,    // 90

            // CS3
            (CorSerializationType)0,    // 91
            (CorSerializationType)0,    // 92
            (CorSerializationType)0,    // 93
            (CorSerializationType)0,    // 94
            (CorSerializationType)0,    // 95
            (CorSerializationType)0,    // 96
            (CorSerializationType)0,    // 97
            (CorSerializationType)0,    // 98
            (CorSerializationType)0,    // 99
            (CorSerializationType)0,    // 100
            (CorSerializationType)0,    // 101
            (CorSerializationType)0,    // 102
            (CorSerializationType)0,    // 103
            (CorSerializationType)0,    // 104
            (CorSerializationType)0,    // 105

            // CS4
            (CorSerializationType)0,    // 106
            (CorSerializationType)0,    // 107
            (CorSerializationType)0,    // 108
            (CorSerializationType)0,    // 109
            (CorSerializationType)0,    // 110
            (CorSerializationType)0,    // 111
            (CorSerializationType)0,    // 112
            (CorSerializationType)0,    // 113
            (CorSerializationType)0,    // 114
            (CorSerializationType)0,    // 115
            (CorSerializationType)0,    // 116
            (CorSerializationType)0,    // 117
            (CorSerializationType)0,    // 118
            (CorSerializationType)0,    // 119
            (CorSerializationType)0,    // 120
            (CorSerializationType)0,    // 121
            (CorSerializationType)0,    // 122
            (CorSerializationType)0,    // 123
            (CorSerializationType)0,    // 124
            (CorSerializationType)0,    // 125
            (CorSerializationType)0,    // 126
            (CorSerializationType)0,    // 127
            (CorSerializationType)0,    // 128
            (CorSerializationType)0,    // 129
            (CorSerializationType)0,    // 130
        };
        
        internal const double doubleZero = 0;
        internal const long longZero = 0;
        internal const decimal decimalZero = 0;

        //------------------------------------------------------------
        // PredefType.InfoTable
        //
        /// <summary>
        /// <para>Static array of info about the predefined types.</para>
        /// <para>(CSharp\Inc\PredefType.cs, in sscli, defined in symmgr.cpp)</para>
        /// </summary>
        //------------------------------------------------------------
        internal static PredefTypeInfo[] InfoTable =
        {
            // 0
            new PredefTypeInfo(
                "System.Byte",	    // fullName
                true,	            // isRequired
                true,	            // isSimple
                true,	            // isNumeric
                false,	            // isQSimple
                FUNDTYPE.U1,	    // ft
                CorElementType.U1,	// et
                AggKindEnum.Struct,	// aggKind
                "byte",	            // niceName
                (Byte)0,	        // zero
                PREDEFATTR.COUNT,	// attr
                1,	                // asize
                0,	                // arity
                1	                // inmscorlib
            ),

            // 1
			new PredefTypeInfo(
				"System.Int16",	    // fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.I2,	    // ft
				CorElementType.I2,	// et
				AggKindEnum.Struct,	// aggKind
				"short",	        // niceName
				(Int16)0,	        // zero
				PREDEFATTR.COUNT,	// attr
				2,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 2
			new PredefTypeInfo(
				"System.Int32",	    // fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.I4,	    // ft
				CorElementType.I4,	// et
				AggKindEnum.Struct,	// aggKind
				"int",	            // niceName
				(Int32)0,	        // zero
				PREDEFATTR.COUNT,	// attr
				4,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 3
			new PredefTypeInfo(
				"System.Int64",	    // fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.I8,        // ft
				CorElementType.I8,	// et
				AggKindEnum.Struct,	// aggKind
				"long",	            // niceName
				longZero,	        // zero
				PREDEFATTR.COUNT,	// attr
				8,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 4
			new PredefTypeInfo(
				"System.Single",	// fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.R4,	    // ft
				CorElementType.R4,	// et
				AggKindEnum.Struct,	// aggKind
				"float",            // niceName
				(Single)0,	        // zero // doubleZero in sscli
				PREDEFATTR.COUNT,	// attr
				4,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 5
			new PredefTypeInfo(
				"System.Double",	// fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.R8,	    // ft
				CorElementType.R8,	// et
				AggKindEnum.Struct,	// aggKind
				"double",	        // niceName
				(Double)0,	        // zero // doubleZero in sscli.
				PREDEFATTR.COUNT,	// attr
				8,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 6
			new PredefTypeInfo(
				"System.Decimal",	// fullName
				false,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				"decimal",	        // niceName
				decimalZero,	    // zero
				PREDEFATTR.COUNT,	// attr
				0,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 7
			new PredefTypeInfo(
				"System.Char",	        // fullName
				true,	                // isRequired
				true,	                // isSimple
				false,	                // isNumeric
				false,	                // isQSimple
				FUNDTYPE.U2,	        // ft
				CorElementType.CHAR,    // et
				AggKindEnum.Struct,	    // aggKind
				"char",	                // niceName
				(Char)0,	            // zero
				PREDEFATTR.COUNT,	    // attr
				2,	                    // asize
				0,	                    // arity
				1	                    // inmscorlib
			),

            // 8
			new PredefTypeInfo(
				"System.Boolean",	    // fullName
				true,	                // isRequired
				true,	                // isSimple
				false,	                // isNumeric
				false,	                // isQSimple
				FUNDTYPE.I1,	        // ft
				CorElementType.BOOLEAN,	// et
				AggKindEnum.Struct,	    // aggKind
				"bool",	                // niceName
				0,                      // zero
				PREDEFATTR.COUNT,	    // attr
				1,	                    // asize
				0,	                    // arity
				1	                    // inmscorlib
			),

            // 9
			new PredefTypeInfo(
				"System.SByte",	    // fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				true,	            // isQSimple
				FUNDTYPE.I1,	    // ft
				CorElementType.I1,	// et
				AggKindEnum.Struct,	// aggKind
				"sbyte",	        // niceName
				(SByte)0,	        // zero
				PREDEFATTR.COUNT,	// attr
				1,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 10
			new PredefTypeInfo(
				"System.UInt16",	// fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				true,	            // isQSimple
				FUNDTYPE.U2,	    // ft
				CorElementType.U2,	// et
				AggKindEnum.Struct,	// aggKind
				"ushort",	        // niceName
				(UInt16)0,	        // zero
				PREDEFATTR.COUNT,	// attr
				2,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 11
			new PredefTypeInfo(
				"System.UInt32",	// fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				true,	            // isQSimple
				FUNDTYPE.U4,	    // ft
				CorElementType.U4,	// et
				AggKindEnum.Struct,	// aggKind
				"uint",	            // niceName
				(UInt32)0,          // zero
				PREDEFATTR.COUNT,	// attr
				4,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 12
			new PredefTypeInfo(
				"System.UInt64",	// fullName
				true,	            // isRequired
				true,	            // isSimple
				true,	            // isNumeric
				true,	            // isQSimple
				FUNDTYPE.U8,	    // ft
				CorElementType.U8,	// et
				AggKindEnum.Struct,	// aggKind
				"ulong",	        // niceName
				(UInt64)0,	        // zero // longZero (In sscli)
				PREDEFATTR.COUNT,	// attr
				8,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 13
			new PredefTypeInfo(
				"System.IntPtr",	// fullName
				true,               // isRequired
				false,	            // isSimple
				false,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.I,	// et
				AggKindEnum.Struct,	// aggKind
				null,	            // niceName
				(IntPtr)0,          // zero
				PREDEFATTR.COUNT,	// attr
				0,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 14
			new PredefTypeInfo(
				"System.UIntPtr",	// fullName
				true,	            // isRequired
				false,	            // isSimple
				false,	            // isNumeric
				false,	            // isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.U,	// et
				AggKindEnum.Struct,	// aggKind
				null,	            // niceName
				(UIntPtr)0,	        // zero
				PREDEFATTR.COUNT,	// attr
				0,	                // asize
				0,	                // arity
				1	                // inmscorlib
			),

            // 15
			new PredefTypeInfo(
				"System.Object",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.OBJECT,	// et
				AggKindEnum.Class,	// aggKind
				"object",	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				-1,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 16
			new PredefTypeInfo(
				"System.String",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.STRING,	// et
				AggKindEnum.Class,	// aggKind
				"string",	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				-1,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 17
			new PredefTypeInfo(
				"System.Delegate",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 18
			new PredefTypeInfo(
				"System.MulticastDelegate",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 19
			new PredefTypeInfo(
				"System.Array",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 20
			new PredefTypeInfo(
				"System.Exception",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 21
			new PredefTypeInfo(
				"System.Type",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				-1,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 22
			new PredefTypeInfo(
				"System.Threading.Monitor",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 23
			new PredefTypeInfo(
				"System.ValueType",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 24
			new PredefTypeInfo(
				"System.Enum",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 25
			new PredefTypeInfo(
				"System.Security.Permissions.CodeAccessSecurityAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 26
			new PredefTypeInfo(
				"System.Security.Permissions.SecurityPermissionAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 27
			new PredefTypeInfo(
				"System.Security.UnverifiableCodeAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 28
			new PredefTypeInfo(
				"System.Diagnostics.DebuggableAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.DEBUGGABLE,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 29
			new PredefTypeInfo(
				"System.MarshalByRefObject",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 30
			new PredefTypeInfo(
				"System.ContextBoundObject",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 31
			new PredefTypeInfo(
				"System.Runtime.InteropServices.InAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.IN,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 32
			new PredefTypeInfo(
				"System.Runtime.InteropServices.OutAttribute",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.OUT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 33
			new PredefTypeInfo(
				"System.Attribute",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 34
			new PredefTypeInfo(
				"System.AttributeUsageAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.ATTRIBUTEUSAGE,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 35
			new PredefTypeInfo(
				"System.AttributeTargets",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Enum,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 36
			new PredefTypeInfo(
				"System.ObsoleteAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.OBSOLETE,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 37
			new PredefTypeInfo(
				"System.Diagnostics.ConditionalAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.CONDITIONAL,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 38
			new PredefTypeInfo(
				"System.CLSCompliantAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.CLSCOMPLIANT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 39
			new PredefTypeInfo(
				"System.Runtime.InteropServices.GuidAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.GUID,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 40
			new PredefTypeInfo(
				"System.Reflection.DefaultMemberAttribute",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.DEFAULTMEMBER,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 41
			new PredefTypeInfo(
				"System.ParamArrayAttribute",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.PARAMARRAY,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 42
			new PredefTypeInfo(
				"System.Runtime.InteropServices.ComImportAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COMIMPORT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 43
			new PredefTypeInfo(
				"System.Runtime.InteropServices.FieldOffsetAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.STRUCTOFFSET,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 44
			new PredefTypeInfo(
				"System.Runtime.InteropServices.StructLayoutAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.STRUCTLAYOUT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 45
			new PredefTypeInfo(
				"System.Runtime.InteropServices.LayoutKind",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Enum,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 46
			new PredefTypeInfo(
				"System.Runtime.InteropServices.MarshalAsAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 47
			new PredefTypeInfo(
				"System.Runtime.InteropServices.DllImportAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.DLLIMPORT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 48
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.IndexerNameAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.NAME,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 49
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.DecimalConstantAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 50
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.RequiredAttributeAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.REQUIRED,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 51
			new PredefTypeInfo(
				"System.Runtime.InteropServices.DefaultParameterValueAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.DEFAULTVALUE,	// attr
				0,	// asize
				0,	// arity
				0	// inmscorlib
			),

            // 52
			new PredefTypeInfo(
				"System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.UNMANAGEDFUNCTIONPOINTER,	// attr
				0,	// asize
				0,	// arity
				0	// inmscorlib
			),

            // 53
			new PredefTypeInfo(
				"System.TypedReference",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.TYPEDBYREF,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 54
			new PredefTypeInfo(
				"System.ArgIterator",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 55
			new PredefTypeInfo(
				"System.RuntimeTypeHandle",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 56
			new PredefTypeInfo(
				"System.RuntimeFieldHandle",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 57
			new PredefTypeInfo(
				"System.RuntimeMethodHandle",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 58
			new PredefTypeInfo(
				"System.RuntimeArgumentHandle",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 59
			new PredefTypeInfo(
				"System.Collections.Hashtable",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 60
			new PredefTypeInfo(
				"System.Collections.Generic.Dictionary",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				2,	// arity
				1	// inmscorlib
			),

            // 61
			new PredefTypeInfo(
				"System.IAsyncResult",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 62
			new PredefTypeInfo(
				"System.AsyncCallback",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Delegate,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 63
			new PredefTypeInfo(
				"System.Security.Permissions.SecurityAction",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.I4,	// ft
				CorElementType.END,	// et
				AggKindEnum.Enum,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 64
			new PredefTypeInfo(
				"System.IDisposable",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 65
			new PredefTypeInfo(
				"System.Collections.IEnumerable",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 66
			new PredefTypeInfo(
				"System.Collections.IEnumerator",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 67
			new PredefTypeInfo(
				"System.Void",	// fullName
				true,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 68
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.RuntimeHelpers",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 69
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.IsVolatile",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 70
			new PredefTypeInfo(
				"System.Runtime.InteropServices.CoClassAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COCLASS,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 71
			new PredefTypeInfo(
				"System.Activator",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 72
			new PredefTypeInfo(
				"System.Collections.Generic.IEnumerable",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				1	// inmscorlib
			),

            // 73
			new PredefTypeInfo(
				"System.Collections.Generic.IEnumerator",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				1	// inmscorlib
			),

            // 74
			new PredefTypeInfo(
				"System.Nullable",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.STRUCT,	// ft
				CorElementType.END,	// et
				AggKindEnum.Struct,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				1	// inmscorlib
			),

            // 75
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.FixedBufferAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.FIXED,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 76
			new PredefTypeInfo(
				"System.Runtime.InteropServices.DefaultCharSetAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.DEFAULTCHARSET,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 77
            new PredefTypeInfo(
				"System.Runtime.CompilerServices.CompilationRelaxationsAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COMPILATIONRELAXATIONS,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 78
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.RUNTIMECOMPATIBILITY,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 79
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.InternalsVisibleToAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.FRIENDASSEMBLY,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 80
			new PredefTypeInfo(
				"System.Diagnostics.DebuggerHiddenAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 81
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.TypeForwardedToAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.TYPEFORWARDER,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 82
			new PredefTypeInfo(
				"System.Reflection.AssemblyKeyFileAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.KEYFILE,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 83
			new PredefTypeInfo(
				"System.Reflection.AssemblyKeyNameAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.KEYNAME,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 84
			new PredefTypeInfo(
				"System.Reflection.AssemblyDelaySignAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.DELAYSIGN,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 85
			new PredefTypeInfo(
				"System.NotSupportedException",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 86
			new PredefTypeInfo(
				"System.Threading.Interlocked",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 87
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.CompilerGeneratedAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 88
			new PredefTypeInfo(
				"System.Runtime.CompilerServices.UnsafeValueTypeAttribute",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				1	// inmscorlib
			),

            // 89
			new PredefTypeInfo(
				"System.Collections.Generic.ICollection",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				1	// inmscorlib
			),

            // 90
			new PredefTypeInfo(
				"System.Collections.Generic.IList",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				1	// inmscorlib
			),

            // CS3

            // 91
			new PredefTypeInfo(
				"System.Func",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Delegate,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				0	// inmscorlib
			),

            // 92
			new PredefTypeInfo(
				"System.Func",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Delegate,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				2,	// arity
				0	// inmscorlib
			),

            // 93
			new PredefTypeInfo(
				"System.Func",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Delegate,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				3,	// arity
				0	// inmscorlib
			),

            // 94
			new PredefTypeInfo(
				"System.Func",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Delegate,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				4,	// arity
				0	// inmscorlib
			),

            // 95
			new PredefTypeInfo(
				"System.Func",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Delegate,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				5,	// arity
				0	// inmscorlib
			),

            // 96
			new PredefTypeInfo(
				"System.Collections.Generic.EqualityComparer",  // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				0	// inmscorlib
			),

            // 97
			new PredefTypeInfo(
				"System.Linq.Enumerable",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				0	// inmscorlib
			),

            // 98
			new PredefTypeInfo(
				"System.Linq.IGrouping",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				2,	// arity
				0	// inmscorlib
			),

            // 99
			new PredefTypeInfo(
				"System.Linq.Expressions.Expression",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				0	// inmscorlib
			),

            // 100
			new PredefTypeInfo(
				"System.Linq.Expressions.Expression",	// fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				0	// inmscorlib
			),

            // 101
			new PredefTypeInfo(
				"System.Collections.Generic.IComparer",  // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				0	// inmscorlib
			),

            // 102
			new PredefTypeInfo(
				"System.Collections.Generic.IEqualityComparer",  // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Interface,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				0	// inmscorlib
			),

            // 103
			new PredefTypeInfo(
				"System.Linq.Queryable",    // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				0	// inmscorlib
			),

            // 104
			new PredefTypeInfo(
				"System.Linq.IQueryable",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				0	// inmscorlib
			),

            // 105
			new PredefTypeInfo(
				"System.Linq.IQueryable",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				0	// inmscorlib
			),

            // CS4

            // 106
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				0,	// arity
				0	// inmscorlib
			),

            // 107
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				1,	// arity
				0	// inmscorlib
			),

            // 108
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				2,	// arity
				0	// inmscorlib
			),

            // 109
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				3,	// arity
				0	// inmscorlib
			),

            // 110
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
				4,	// arity
				0	// inmscorlib
			),

            // 111
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                5,	// arity
				0	// inmscorlib
			),

            // 112
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                6,	// arity
				0	// inmscorlib
			),

            // 113
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                7,	// arity
				0	// inmscorlib
			),

            // 114
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                8,	// arity
				0	// inmscorlib
			),

            // 115
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                9,	// arity
				0	// inmscorlib
			),

            // 116
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                10,	// arity
				0	// inmscorlib
			),

            // 117
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                11,	// arity
				0	// inmscorlib
			),

            // 118
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                12,	// arity
				0	// inmscorlib
			),

            // 119
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                13,	// arity
				0	// inmscorlib
			),

            // 120
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                14,	// arity
				0	// inmscorlib
			),

            // 121
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                15,	// arity
				0	// inmscorlib
			),

            // 122
			new PredefTypeInfo(
                "System.Action",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                16,	// arity
				0	// inmscorlib
			),

            // 123
			new PredefTypeInfo(
                "System.Runtime.CompilerServices.CallSite",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                0,	// arity
				0	// inmscorlib
			),

            // 124
			new PredefTypeInfo(
                "System.Runtime.CompilerServices.CallSite",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                1,	// arity
				0	// inmscorlib
			),

            // 125
			new PredefTypeInfo(
                "System.Runtime.CompilerServices.CallSiteBinder",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                0,	// arity
				0	// inmscorlib
			),

            // 126
			new PredefTypeInfo(
                "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                0,	// arity
				0	// inmscorlib
			),

            // 127
			new PredefTypeInfo(
                "Microsoft.CSharp.RuntimeBinder.Binder",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Class,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                0,	// arity
				0	// inmscorlib
			),

            // 128
			new PredefTypeInfo(
                "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Enum,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                0,	// arity
				0	// inmscorlib
			),

            // 129
			new PredefTypeInfo(
                "Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Enum,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                0,	// arity
				0	// inmscorlib
			),

            // 130
			new PredefTypeInfo(
                "System.Linq.Expressions.ExpressionType",   // fullName
				false,	// isRequired
				false,	// isSimple
				false,	// isNumeric
				false,	// isQSimple
				FUNDTYPE.REF,	// ft
				CorElementType.END,	// et
				AggKindEnum.Enum,	// aggKind
				null,	// niceName
				0,	// zero
				PREDEFATTR.COUNT,	// attr
				0,	// asize
                0,	// arity
				0	// inmscorlib
			),

            // 131 COUNT // Count of the predefined types.

        };
    }

    //======================================================================
    // class CTypeOf
    //
    /// <summary>
    /// (CSharp\Inc\PredefType.cs)
    /// </summary>
    //======================================================================
    internal static class CTypeOf
    {
        //------------------------------------------------------------
        // System
        //------------------------------------------------------------
        internal static partial class System
        {
            internal static Type Void = typeof(void);
        }

        //------------------------------------------------------------
        // System.Runtime.CompilerServices
        //------------------------------------------------------------
        internal static partial class System
        {
            internal static partial class Runtime
            {
                internal static class CompilerServices
                {
                    internal static Type IsVolatile
                        = typeof(global::System.Runtime.CompilerServices.IsVolatile);
                }
            }
        }
    }
}