﻿// sscli20_20060311

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
// File: predefname.h
//
// Contains a list of the various predefined names.
// ===========================================================================

//============================================================================
// PredefName.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // PREDEFNAME  (PN_)
    //
    /// <summary>
    /// <para>In sscli, defined in CSharp\inc\enum.h.</para>
    /// <para>(CSharp\Inc\PredefName.cs)</para>
    /// <para>Add "UNDEFINED = 0 at the top of member declarations.</para>
    /// </summary>
    //======================================================================
    internal enum PREDEFNAME : int
    {
        UNDEFINED = 0,  // hirano567@hotmail.co.jp

        CTOR,
        DTOR,
        STATCTOR,
        ENUMVALUE,
        PTR,
        NUB,
        PINNED,
        OUTPARAM,
        REFPARAM,
        ARRAY0,
        ARRAY1,
        ARRAY2,
        GARRAY0,
        GARRAY1,
        GARRAY2,
        THIS,
        MAIN,
        EMPTY,
        DEFAULT_CASE,
        INVOKE,
        BEGININVOKE,
        ENDINVOKE,
        VALUE,
        LENGTH,
        INDEXER,
        INDEXERINTERNAL,
        COMBINE,
        REMOVE,
        COMPAREEXCHANGE,
        ITERSTATE,
        ITERCURRENT,
        HOISTEDTHIS,
        TYVARCONTAINER,
        OPEXPLICITMN,
        OPIMPLICITMN,
        OPEXPLICIT,
        OPIMPLICIT,
        OPUNARYPLUS,
        OPUNARYMINUS,
        OPCOMPLEMENT,
        OPINCREMENT,
        OPDECREMENT,
        OPPLUS,
        OPMINUS,
        OPMULTIPLY,
        OPDIVISION,
        OPMODULUS,
        OPXOR,
        OPBITWISEAND,
        OPBITWISEOR,
        OPLEFTSHIFT,
        OPRIGHTSHIFT,
        OPEQUALS,
        OPCOMPARE,
        OPEQUALITY,
        OPINEQUALITY,
        OPGREATERTHAN,
        OPLESSTHAN,
        OPGREATERTHANOREQUAL,
        OPLESSTHANOREQUAL,
        OPTRUE,
        OPFALSE,
        OPNEGATION,
        GETTYPE,
        GETTYPEFROMHANDLE,
        ISINSTANCEOF,
        ENTER,
        EXIT,
        EQUALS,
        STRCONCAT,
        GETENUMERATOR,
        MOVENEXT,
        CURRENT,
        RESET,
        GETLOWERBOUND,
        GETUPPERBOUND,
        ADD,
        GETITEM,
        TRYGETVALUE,
        GETLENGTH,
        GETCHARS,
        GETHASHCODE,
        DISPOSE,
        INITIALIZEARRAY,
        OFFSETTOSTRINGDATA,
        ARGLIST,
        NATURALINT,
        VALIDON,
        ALLOWMULTIPLE,
        INHERITED,
        REQUESTMINIMUM,
        SKIPVERIFICATION,
        SubType,
        Size,
        ComType,
        Marshaller,
        Cookie,
        ENTRYPOINT,
        CharSet,
        SetLastError,
        EXACTSPELLING,
        CALLINGCONVENTION,
        SEQUENTIAL,
        EXPLICIT,
        WRAPNONEXCEPTIONTHROWS,
        Pack,
        TYPEPARAMS,
        OBSOLETE_CLASS,
        ATTRIBUTEUSAGE_CLASS,
        CONDITIONAL_CLASS,
        DECIMALLITERAL_CLASS,
        DEPRECATEDHACK_CLASS,
        PARAMS_CLASS,
        CLSCOMPLIANT,
        DEFAULTMEMBER_CLASS,
        DEFAULTMEMBER_CLASS2,
        PARAMARRAY_CLASS,
        OUT_CLASS,
        REQUIRED_CLASS,
        BROWSABLE_CLASS,
        COCLASS_CLASS,
        FIXEDBUFFER_CLASS,
        COMPILATIONRELAXATIONS_CLASS,
        RUNTIMECOMPATIBILTY_CLASS,
        DELEGATECTORPARAM0,
        DELEGATECTORPARAM1,
        DELEGATEBIPARAM0,
        DELEGATEBIPARAM1,
        DELEGATEEIPARAM0,
        DELEGATEEIPARAM0ALT,
        ASSEMBLY,
        MODULE,
        TYPE,
        METHOD,
        PROPERTY,
        FIELD,
        PARAM,
        EVENT,
        TRUE,
        FALSE,
        NULL,
        BASE,
        DEBUGGERDISPLAYATTR,
        DEBUGGERVISUALIZERPROXYATTR,
        DEBUGGERTYPEPROXYATTR,
        DEBUGGERBROWSABLEATTR,
        INTEROPGUIDATTR,
        SECONDARYSYM,
        PURETHIS,
        GETINITIALDATA,
        INPLACEUPDATE,
        CREATEREPLACEMENTOBJECT,
        CREATESOURCE,
        CREATENONVISUALPROXYCONSTRUCTOR,
        CREATENONVISUALPROXYOBJECT,
        TOSTRING,
        DEXCEPTION,
        EXCEPTION,
        INFINITY,
        NINFINITY,
        NAN,
        SOAPMETHOD,
        SOAPDOCUMENTMETHOD,
        SORTEDCHILDREN,
        CREATEDELEGATE,
        SIZEOF,
        FLAGS,
        LOAD,
        LOADFROM,
        GETDATA,
        GETSOURCE,
        ISINSTANCEOFTYPE,
        REPLACE,
        VISUALIZEROBJECTSOURCE,
        STRINGVALUEMEMBER,
        SQLSTRINGVALUEMEMBER,
        H,
        D,
        RAW,
        PRIVATE,
        NQ,
        T,
        AC,
        BUCKETS,
        KEY,
        VAL,
        UITEMS,
        USIZE,
        ANONYMOUSMETHOD,
        CREATESTATICDELEGATE,
        CREATEINSTANCEDELEGATE,
        CREATEINSTANCE,
        FIXEDELEMENT,
        EXTERNALIASCONTAINER,
        GLOBAL,
        FRIENDASSEMBLY_CLASS,
        ERROR_ASSEM,
        HASVALUE,
        CAP_VALUE,
        GET_VALUE_OR_DEF,
        NOCAP_HASVALUE,
        DEBUGGINGMODES_CLASS,
        DISABLEOPTIMIZATIONS,
        DEFAULT,
        IGNORESYMBOLSTORESEQUENCEPOINTS,
        ENABLEEDITANDCONTINUE,
        COUNT
    }

    static internal class CPredefinedName
    {
        //============================================================
        // CPredefinedName.InfoTable
        //
        /// <summary>
        /// <para>Static array of the predefined names.</para>
        /// <para>In sscli, defined in csharp\sccomp\namemgr.cpp (line 53).</para>
        /// </summary>
        //============================================================
        static internal string[] InfoTable =
        {
            null,   // UNDEFINED

            ".ctor",
            "Finalize",
            ".cctor",
            "value__",
            "*",
            "?*",
            "@",
            "#",
            "&",
            "[X\x0001",   // "[X\001"
            "[X\x0002",   // "[X\002"
            "[X\x0003",   // "[X\003"
            "[G\x0001",   // "[G\001"
            "[G\x0002",   // "[G\002"
            "[G\x0003",   // "[G\003"
            "<this>",
            "Main",
            "",
            "default:",
            "Invoke",
            "BeginInvoke",
            "EndInvoke",
            "value",
            "Length",
            "Item",
            "$Item$",
            "Combine",
            "Remove",
            "CompareExchange",
            "<>1__state",
            "<>2__current",
            "<>4__this",
            "<TypeVariableContainer>",
            "op_Explicit",
            "op_Implicit",
            "explicit",
            "implicit",
            "op_UnaryPlus",
            "op_UnaryNegation",
            "op_OnesComplement",
            "op_Increment",
            "op_Decrement",
            "op_Addition",
            "op_Subtraction",
            "op_Multiply",
            "op_Division",
            "op_Modulus",
            "op_ExclusiveOr",
            "op_BitwiseAnd",
            "op_BitwiseOr",
            "op_LeftShift",
            "op_RightShift",
            "op_Equals",
            "op_Compare",
            "op_Equality",
            "op_Inequality",
            "op_GreaterThan",
            "op_LessThan",
            "op_GreaterThanOrEqual",
            "op_LessThanOrEqual",
            "op_True",
            "op_False",
            "op_LogicalNot",
            "GetType",
            "GetTypeFromHandle",
            "IsInstanceOfType",
            "Enter",
            "Exit",
            "Equals",
            "Concat",
            "GetEnumerator",
            "MoveNext",
            "Current",
            "Reset",
            "GetLowerBound",
            "GetUpperBound",
            "Add",
            "get_Item",
            "TryGetValue",
            "get_Length",
            "get_Chars",
            "GetHashCode",
            "Dispose",
            "InitializeArray",
            "OffsetToStringData",
            "__arglist",
            "Natural Int",
            "ValidOn",
            "AllowMultiple",
            "Inherited",
            "RequestMinimum",
            "SkipVerification",
            "SubType",
            "Size",
            "ComType",
            "Marshaller",
            "Cookie",
            "EntryPoint",
            "CharSet",
            "SetLastError",
            "ExactSpelling",
            "CallingConvention",
            "Sequential",
            "Explicit",
            "WrapNonExceptionThrows",
            "Pack",
            "TypeParamNames",
            "System.ObsoleteAttribute",
            "System.AttributeUsageAttribute",
            "System.Diagnostics.ConditionalAttribute",
            "System.Runtime.CompilerServices.DecimalConstantAttribute",
            "Deprecated",
            "System.ParamArrayAttribute",
            "System.CLSCompliantAttribute",
            "System.Reflection.DefaultMemberAttribute",
            "System.Runtime.InteropServices.DefaultMemberAttribute",
            "System.ParamArrayAttribute",
            "System.Runtime.InteropServices.OutAttribute",
            "System.Runtime.CompilerServices.RequiredAttributeAttribute",
            "System.ComponentModel.EditorBrowsableAttribute",
            "System.Runtime.InteropServices.CoClassAttribute",
            "System.Runtime.CompilerServices.FixedBufferAttribute",
            "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
            "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
            "object",
            "method",
            "callback",
            "object",
            "result",
            "__result",
            "assembly",
            "module",
            "type",
            "method",
            "property",
            "field",
            "param",
            "event",
            "true",
            "false",
            "null",
            "base",
            "System.Diagnostics.DebuggerDisplayAttribute",
            "System.Diagnostics.DebuggerVisualizerAttribute",
            "System.Diagnostics.DebuggerTypeProxyAttribute",
            "System.Diagnostics.DebuggerBrowsableAttribute",
            "System.Runtime.InteropServices.GuidAttribute",
            "secondary sym",
            "this",
            "GetInitialData",
            "InPlaceUpdate",
            "CreateReplacementObject",
            "CreateSource",
            "CreateNonVisualProxyConstructor",
            "CreateNonVisualProxyObject",
            "ToString",
            "$exception",
            "exception",
            "Infinity",
            "-Infinity",
            "NaN",
            "System.Runtime.Remoting.Metadata.SoapMethodAttribute",
            "System.Web.Services.Protocols.SoapDocumentMethodAttribute",
            "sorted children",
            "CreateDelegate",
            "SizeOf",
            "System.FlagsAttribute",
            "Load",
            "LoadFrom",
            "GetData",
            "GetSource",
            "IsInstanceOfType",
            "Replace",
            "System.Diagnostics.VisualizerObjectSource",
            "m_StringValue",
            "m_value",
            "h",
            "d",
            "raw",
            "hidden",
            "nq",
            "t",
            "ac",
            "buckets",
            "key",
            "val",
            "_items",
            "_size",
            "AnonymousMethod",
            "CreateStaticDelegate",
            "CreateInstanceDelegate",
            "CreateInstance",
            "FixedElementField",
            "<ExternAliasContainer>",
            "global",
            "System.Runtime.CompilerServices.InternalsVisibleToAttribute",
            ":ErrAssem:",
            "HasValue",
            "Value",
            "GetValueOrDefault",
            "hasValue",
            "DebuggingModes",
            "DisableOptimizations",
            "Default",
            "IgnoreSymbolStoreSequencePoints",
            "EnableEditAndContinue",
        };
    }
}
