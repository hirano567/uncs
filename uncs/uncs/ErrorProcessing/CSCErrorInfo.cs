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
// File: scc.cpp
//
// The command line driver for the C# compiler.
// ===========================================================================

//============================================================================
// CSCErrorInfo.cs
//
// Definition of CSCErrorInfo.
// In sscli, this definition is in CSharp\SCC\SCC.cpp.
//
// 2015/10/18 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // class CSCErrorInfo
    //
    /// <summary></summary>
    //======================================================================
    internal static class CSCErrorInfo
    {
        internal static CSCErrorInfoManager Manager = new CSCErrorInfoManager();
    }

    //======================================================================
    // class CSCErrorInfoManager
    //
    /// <summary></summary>
    //======================================================================
    internal class CSCErrorInfoManager : BCErrorInfoManager<CSCERRID>
    {
        //------------------------------------------------------------
        // CSCErrorInfoManager Constructor
        //------------------------------------------------------------
        internal CSCErrorInfoManager()
            : base()
        {
            this.Prefix = "CS";
            this.InvalidErrorID = CSCERRID.Invalid;
        }

        //------------------------------------------------------------
        // CSCErrorInfoManager.InitErrorInfoDic (override)
        //------------------------------------------------------------
        internal override void InitErrorInfoDic()
        {
            ErrorInfoDic.Add(
                CSCERRID.FTL_InternalError,
                new ERRORINFO(
                    CSCERRID.FTL_InternalError,
                    1,
                    -1,
                    ResNo.CSCSTR_InternalError));
            ErrorInfoDic.Add(
                CSCERRID.FTL_NoMemory,
                new ERRORINFO(
                    CSCERRID.FTL_NoMemory,
                    3,
                    -1,
                    ResNo.CSCSTR_NoMemory));
            ErrorInfoDic.Add(
                CSCERRID.ERR_WarningAsError,
                new ERRORINFO(
                    CSCERRID.ERR_WarningAsError,
                    4,
                    0,
                    ResNo.CSCSTR_WarningAsError));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingOptionArg,
                new ERRORINFO(
                    CSCERRID.ERR_MissingOptionArg,
                    5,
                    0,
                    ResNo.CSCSTR_MissingOptionArg));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoMetadataFile,
                new ERRORINFO(
                    CSCERRID.ERR_NoMetadataFile,
                    6,
                    0,
                    ResNo.CSCSTR_NoMetadataFile));
            ErrorInfoDic.Add(
                CSCERRID.FTL_ComPlusInit,
                new ERRORINFO(
                    CSCERRID.FTL_ComPlusInit,
                    7,
                    -1,
                    ResNo.CSCSTR_ComPlusInit));
            ErrorInfoDic.Add(
                CSCERRID.FTL_MetadataImportFailure,
                new ERRORINFO(
                    CSCERRID.FTL_MetadataImportFailure,
                    8,
                    -1,
                    ResNo.CSCSTR_MetadataImportFailure));
            ErrorInfoDic.Add(
                CSCERRID.FTL_MetadataCantOpenFile,
                new ERRORINFO(
                    CSCERRID.FTL_MetadataCantOpenFile,
                    9,
                    -1,
                    ResNo.CSCSTR_MetadataCantOpenFile));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantImportBase,
                new ERRORINFO(
                    CSCERRID.ERR_CantImportBase,
                    11,
                    0,
                    ResNo.CSCSTR_CantImportBase));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoTypeDef,
                new ERRORINFO(
                    CSCERRID.ERR_NoTypeDef,
                    12,
                    0,
                    ResNo.CSCSTR_NoTypeDef));
            ErrorInfoDic.Add(
                CSCERRID.FTL_MetadataEmitFailure,
                new ERRORINFO(
                    CSCERRID.FTL_MetadataEmitFailure,
                    13,
                    -1,
                    ResNo.CSCSTR_MetadataEmitFailure));
            ErrorInfoDic.Add(
                CSCERRID.FTL_RequiredFileNotFound,
                new ERRORINFO(
                    CSCERRID.FTL_RequiredFileNotFound,
                    14,
                    -1,
                    ResNo.CSCSTR_RequiredFileNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ClassNameTooLong,
                new ERRORINFO(
                    CSCERRID.ERR_ClassNameTooLong,
                    15,
                    0,
                    ResNo.CSCSTR_ClassNameTooLong));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OutputWriteFailed,
                new ERRORINFO(
                    CSCERRID.ERR_OutputWriteFailed,
                    16,
                    0,
                    ResNo.CSCSTR_OutputWriteFailed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MultipleEntryPoints,
                new ERRORINFO(
                    CSCERRID.ERR_MultipleEntryPoints,
                    17,
                    0,
                    ResNo.CSCSTR_MultipleEntryPoints));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnimplementedOp,
                new ERRORINFO(
                    CSCERRID.ERR_UnimplementedOp,
                    18,
                    0,
                    ResNo.CSCSTR_UnimplementedOp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadBinaryOps,
                new ERRORINFO(
                    CSCERRID.ERR_BadBinaryOps,
                    19,
                    0,
                    ResNo.CSCSTR_BadBinaryOps));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IntDivByZero,
                new ERRORINFO(
                    CSCERRID.ERR_IntDivByZero,
                    20,
                    0,
                    ResNo.CSCSTR_IntDivByZero));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadIndexLHS,
                new ERRORINFO(
                    CSCERRID.ERR_BadIndexLHS,
                    21,
                    0,
                    ResNo.CSCSTR_BadIndexLHS));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadIndexCount,
                new ERRORINFO(
                    CSCERRID.ERR_BadIndexCount,
                    22,
                    0,
                    ResNo.CSCSTR_BadIndexCount));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadUnaryOp,
                new ERRORINFO(
                    CSCERRID.ERR_BadUnaryOp,
                    23,
                    0,
                    ResNo.CSCSTR_BadUnaryOp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoStdLib,
                new ERRORINFO(
                    CSCERRID.ERR_NoStdLib,
                    25,
                    0,
                    ResNo.CSCSTR_NoStdLib));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ThisInStaticMeth,
                new ERRORINFO(
                    CSCERRID.ERR_ThisInStaticMeth,
                    26,
                    0,
                    ResNo.CSCSTR_ThisInStaticMeth));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ThisInBadContext,
                new ERRORINFO(
                    CSCERRID.ERR_ThisInBadContext,
                    27,
                    0,
                    ResNo.CSCSTR_ThisInBadContext));
            ErrorInfoDic.Add(
                CSCERRID.WRN_InvalidMainSig,
                new ERRORINFO(
                    CSCERRID.WRN_InvalidMainSig,
                    28,
                    4,
                    ResNo.CSCSTR_InvalidMainSig));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoImplicitConv,
                new ERRORINFO(
                    CSCERRID.ERR_NoImplicitConv,
                    29,
                    0,
                    ResNo.CSCSTR_NoImplicitConv));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoExplicitConv,
                new ERRORINFO(
                    CSCERRID.ERR_NoExplicitConv,
                    30,
                    0,
                    ResNo.CSCSTR_NoExplicitConv));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConstOutOfRange,
                new ERRORINFO(
                    CSCERRID.ERR_ConstOutOfRange,
                    31,
                    0,
                    ResNo.CSCSTR_ConstOutOfRange));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigBinaryOps,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigBinaryOps,
                    34,
                    0,
                    ResNo.CSCSTR_AmbigBinaryOps));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigUnaryOp,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigUnaryOp,
                    35,
                    0,
                    ResNo.CSCSTR_AmbigUnaryOp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InAttrOnOutParam,
                new ERRORINFO(
                    CSCERRID.ERR_InAttrOnOutParam,
                    36,
                    0,
                    ResNo.CSCSTR_InAttrOnOutParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ValueCantBeNull,
                new ERRORINFO(
                    CSCERRID.ERR_ValueCantBeNull,
                    37,
                    0,
                    ResNo.CSCSTR_ValueCantBeNull));
            ErrorInfoDic.Add(
                CSCERRID.ERR_WrongNestedThis,
                new ERRORINFO(
                    CSCERRID.ERR_WrongNestedThis,
                    38,
                    0,
                    ResNo.CSCSTR_WrongNestedThis));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoExplicitBuiltinConv,
                new ERRORINFO(
                    CSCERRID.ERR_NoExplicitBuiltinConv,
                    39,
                    0,
                    ResNo.CSCSTR_NoExplicitBuiltinConv));
            ErrorInfoDic.Add(
                CSCERRID.FTL_DebugInit,
                new ERRORINFO(
                    CSCERRID.FTL_DebugInit,
                    40,
                    -1,
                    ResNo.CSCSTR_DebugInit));
            ErrorInfoDic.Add(
                CSCERRID.FTL_DebugEmitFailure,
                new ERRORINFO(
                    CSCERRID.FTL_DebugEmitFailure,
                    41,
                    -1,
                    ResNo.CSCSTR_DebugEmitFailure));
            ErrorInfoDic.Add(
                CSCERRID.FTL_DebugInitFile,
                new ERRORINFO(
                    CSCERRID.FTL_DebugInitFile,
                    42,
                    -1,
                    ResNo.CSCSTR_DebugInitFile));
            ErrorInfoDic.Add(
                CSCERRID.FTL_BadPDBFormat,
                new ERRORINFO(
                    CSCERRID.FTL_BadPDBFormat,
                    43,
                    -1,
                    ResNo.CSCSTR_BadPDBFormat));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisReturnType,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisReturnType,
                    50,
                    0,
                    ResNo.CSCSTR_BadVisReturnType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisParamType,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisParamType,
                    51,
                    0,
                    ResNo.CSCSTR_BadVisParamType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisFieldType,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisFieldType,
                    52,
                    0,
                    ResNo.CSCSTR_BadVisFieldType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisPropertyType,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisPropertyType,
                    53,
                    0,
                    ResNo.CSCSTR_BadVisPropertyType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisIndexerReturn,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisIndexerReturn,
                    54,
                    0,
                    ResNo.CSCSTR_BadVisIndexerReturn));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisIndexerParam,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisIndexerParam,
                    55,
                    0,
                    ResNo.CSCSTR_BadVisIndexerParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisOpReturn,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisOpReturn,
                    56,
                    0,
                    ResNo.CSCSTR_BadVisOpReturn));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisOpParam,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisOpParam,
                    57,
                    0,
                    ResNo.CSCSTR_BadVisOpParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisDelegateReturn,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisDelegateReturn,
                    58,
                    0,
                    ResNo.CSCSTR_BadVisDelegateReturn));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisDelegateParam,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisDelegateParam,
                    59,
                    0,
                    ResNo.CSCSTR_BadVisDelegateParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisBaseClass,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisBaseClass,
                    60,
                    0,
                    ResNo.CSCSTR_BadVisBaseClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisBaseInterface,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisBaseInterface,
                    61,
                    0,
                    ResNo.CSCSTR_BadVisBaseInterface));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EventNeedsBothAccessors,
                new ERRORINFO(
                    CSCERRID.ERR_EventNeedsBothAccessors,
                    65,
                    0,
                    ResNo.CSCSTR_EventNeedsBothAccessors));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EventNotDelegate,
                new ERRORINFO(
                    CSCERRID.ERR_EventNotDelegate,
                    66,
                    0,
                    ResNo.CSCSTR_EventNotDelegate));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreferencedEvent,
                new ERRORINFO(
                    CSCERRID.WRN_UnreferencedEvent,
                    67,
                    3,
                    ResNo.CSCSTR_UnreferencedEvent));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfaceEventInitializer,
                new ERRORINFO(
                    CSCERRID.ERR_InterfaceEventInitializer,
                    68,
                    0,
                    ResNo.CSCSTR_InterfaceEventInitializer));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EventPropertyInInterface,
                new ERRORINFO(
                    CSCERRID.ERR_EventPropertyInInterface,
                    69,
                    0,
                    ResNo.CSCSTR_EventPropertyInInterface));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadEventUsage,
                new ERRORINFO(
                    CSCERRID.ERR_BadEventUsage,
                    70,
                    0,
                    ResNo.CSCSTR_BadEventUsage));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitEventFieldImpl,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitEventFieldImpl,
                    71,
                    0,
                    ResNo.CSCSTR_ExplicitEventFieldImpl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOverrideNonEvent,
                new ERRORINFO(
                    CSCERRID.ERR_CantOverrideNonEvent,
                    72,
                    0,
                    ResNo.CSCSTR_CantOverrideNonEvent));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AddRemoveMustHaveBody,
                new ERRORINFO(
                    CSCERRID.ERR_AddRemoveMustHaveBody,
                    73,
                    0,
                    ResNo.CSCSTR_AddRemoveMustHaveBody));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractEventInitializer,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractEventInitializer,
                    74,
                    0,
                    ResNo.CSCSTR_AbstractEventInitializer));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PossibleBadNegCast,
                new ERRORINFO(
                    CSCERRID.ERR_PossibleBadNegCast,
                    75,
                    0,
                    ResNo.CSCSTR_PossibleBadNegCast));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ReservedEnumerator,
                new ERRORINFO(
                    CSCERRID.ERR_ReservedEnumerator,
                    76,
                    0,
                    ResNo.CSCSTR_ReservedEnumerator));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AsMustHaveReferenceType,
                new ERRORINFO(
                    CSCERRID.ERR_AsMustHaveReferenceType,
                    77,
                    0,
                    ResNo.CSCSTR_AsMustHaveReferenceType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_LowercaseEllSuffix,
                new ERRORINFO(
                    CSCERRID.WRN_LowercaseEllSuffix,
                    78,
                    4,
                    ResNo.CSCSTR_LowercaseEllSuffix));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadEventUsageNoField,
                new ERRORINFO(
                    CSCERRID.ERR_BadEventUsageNoField,
                    79,
                    0,
                    ResNo.CSCSTR_BadEventUsageNoField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConstraintOnlyAllowedOnGenericDecl,
                new ERRORINFO(
                    CSCERRID.ERR_ConstraintOnlyAllowedOnGenericDecl,
                    80,
                    0,
                    ResNo.CSCSTR_ConstraintOnlyAllowedOnGenericDecl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TypeParamMustBeIdentifier,
                new ERRORINFO(
                    CSCERRID.ERR_TypeParamMustBeIdentifier,
                    81,
                    0,
                    ResNo.CSCSTR_TypeParamMustBeIdentifier));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateParamName,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateParamName,
                    100,
                    0,
                    ResNo.CSCSTR_DuplicateParamName));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateNameInNS,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateNameInNS,
                    101,
                    0,
                    ResNo.CSCSTR_DuplicateNameInNS));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateNameInClass,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateNameInClass,
                    102,
                    0,
                    ResNo.CSCSTR_DuplicateNameInClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NameNotInContext,
                new ERRORINFO(
                    CSCERRID.ERR_NameNotInContext,
                    103,
                    0,
                    ResNo.CSCSTR_NameNotInContext));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigContext,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigContext,
                    104,
                    0,
                    ResNo.CSCSTR_AmbigContext));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DuplicateUsing,
                new ERRORINFO(
                    CSCERRID.WRN_DuplicateUsing,
                    105,
                    3,
                    ResNo.CSCSTR_DuplicateUsing));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadMemberFlag,
                new ERRORINFO(
                    CSCERRID.ERR_BadMemberFlag,
                    106,
                    0,
                    ResNo.CSCSTR_BadMemberFlag));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadMemberProtection,
                new ERRORINFO(
                    CSCERRID.ERR_BadMemberProtection,
                    107,
                    0,
                    ResNo.CSCSTR_BadMemberProtection));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NewRequired,
                new ERRORINFO(
                    CSCERRID.WRN_NewRequired,
                    108,
                    2,
                    ResNo.CSCSTR_NewRequired));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NewNotRequired,
                new ERRORINFO(
                    CSCERRID.WRN_NewNotRequired,
                    109,
                    4,
                    ResNo.CSCSTR_NewNotRequired));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CircConstValue,
                new ERRORINFO(
                    CSCERRID.ERR_CircConstValue,
                    110,
                    0,
                    ResNo.CSCSTR_CircConstValue));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MemberAlreadyExists,
                new ERRORINFO(
                    CSCERRID.ERR_MemberAlreadyExists,
                    111,
                    0,
                    ResNo.CSCSTR_MemberAlreadyExists));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticNotVirtual,
                new ERRORINFO(
                    CSCERRID.ERR_StaticNotVirtual,
                    112,
                    0,
                    ResNo.CSCSTR_StaticNotVirtual));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OverrideNotNew,
                new ERRORINFO(
                    CSCERRID.ERR_OverrideNotNew,
                    113,
                    0,
                    ResNo.CSCSTR_OverrideNotNew));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NewOrOverrideExpected,
                new ERRORINFO(
                    CSCERRID.WRN_NewOrOverrideExpected,
                    114,
                    2,
                    ResNo.CSCSTR_NewOrOverrideExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OverrideNotExpected,
                new ERRORINFO(
                    CSCERRID.ERR_OverrideNotExpected,
                    115,
                    0,
                    ResNo.CSCSTR_OverrideNotExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NamespaceUnexpected,
                new ERRORINFO(
                    CSCERRID.ERR_NamespaceUnexpected,
                    116,
                    0,
                    ResNo.CSCSTR_NamespaceUnexpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoSuchMember,
                new ERRORINFO(
                    CSCERRID.ERR_NoSuchMember,
                    117,
                    0,
                    ResNo.CSCSTR_NoSuchMember));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadSKknown,
                new ERRORINFO(
                    CSCERRID.ERR_BadSKknown,
                    118,
                    0,
                    ResNo.CSCSTR_BadSKknown));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadSKunknown,
                new ERRORINFO(
                    CSCERRID.ERR_BadSKunknown,
                    119,
                    0,
                    ResNo.CSCSTR_BadSKunknown));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ObjectRequired,
                new ERRORINFO(
                    CSCERRID.ERR_ObjectRequired,
                    120,
                    0,
                    ResNo.CSCSTR_ObjectRequired));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigCall,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigCall,
                    121,
                    0,
                    ResNo.CSCSTR_AmbigCall));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadAccess,
                new ERRORINFO(
                    CSCERRID.ERR_BadAccess,
                    122,
                    0,
                    ResNo.CSCSTR_BadAccess));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MethDelegateMismatch,
                new ERRORINFO(
                    CSCERRID.ERR_MethDelegateMismatch,
                    123,
                    0,
                    ResNo.CSCSTR_MethDelegateMismatch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RetObjectRequired,
                new ERRORINFO(
                    CSCERRID.ERR_RetObjectRequired,
                    126,
                    0,
                    ResNo.CSCSTR_RetObjectRequired));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RetNoObjectRequired,
                new ERRORINFO(
                    CSCERRID.ERR_RetNoObjectRequired,
                    127,
                    0,
                    ResNo.CSCSTR_RetNoObjectRequired));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LocalDuplicate,
                new ERRORINFO(
                    CSCERRID.ERR_LocalDuplicate,
                    128,
                    0,
                    ResNo.CSCSTR_LocalDuplicate));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgLvalueExpected,
                new ERRORINFO(
                    CSCERRID.ERR_AssgLvalueExpected,
                    131,
                    0,
                    ResNo.CSCSTR_AssgLvalueExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticConstParam,
                new ERRORINFO(
                    CSCERRID.ERR_StaticConstParam,
                    132,
                    0,
                    ResNo.CSCSTR_StaticConstParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NotConstantExpression,
                new ERRORINFO(
                    CSCERRID.ERR_NotConstantExpression,
                    133,
                    0,
                    ResNo.CSCSTR_NotConstantExpression));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NotNullConstRefField,
                new ERRORINFO(
                    CSCERRID.ERR_NotNullConstRefField,
                    134,
                    0,
                    ResNo.CSCSTR_NotNullConstRefField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NameIllegallyOverrides,
                new ERRORINFO(
                    CSCERRID.ERR_NameIllegallyOverrides,
                    135,
                    0,
                    ResNo.CSCSTR_NameIllegallyOverrides));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LocalIllegallyOverrides,
                new ERRORINFO(
                    CSCERRID.ERR_LocalIllegallyOverrides,
                    136,
                    0,
                    ResNo.CSCSTR_LocalIllegallyOverrides));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadUsingNamespace,
                new ERRORINFO(
                    CSCERRID.ERR_BadUsingNamespace,
                    138,
                    0,
                    ResNo.CSCSTR_BadUsingNamespace));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoBreakOrCont,
                new ERRORINFO(
                    CSCERRID.ERR_NoBreakOrCont,
                    139,
                    0,
                    ResNo.CSCSTR_NoBreakOrCont));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateLabel,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateLabel,
                    140,
                    0,
                    ResNo.CSCSTR_DuplicateLabel));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FeatureNYI,
                new ERRORINFO(
                    CSCERRID.ERR_FeatureNYI,
                    141,
                    0,
                    ResNo.CSCSTR_FeatureNYI));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoConstructors,
                new ERRORINFO(
                    CSCERRID.ERR_NoConstructors,
                    143,
                    0,
                    ResNo.CSCSTR_NoConstructors));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoNewAbstract,
                new ERRORINFO(
                    CSCERRID.ERR_NoNewAbstract,
                    144,
                    0,
                    ResNo.CSCSTR_NoNewAbstract));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConstValueRequired,
                new ERRORINFO(
                    CSCERRID.ERR_ConstValueRequired,
                    145,
                    0,
                    ResNo.CSCSTR_ConstValueRequired));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CircularBase,
                new ERRORINFO(
                    CSCERRID.ERR_CircularBase,
                    146,
                    0,
                    ResNo.CSCSTR_CircularBase));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadDelegateConstructor,
                new ERRORINFO(
                    CSCERRID.ERR_BadDelegateConstructor,
                    148,
                    0,
                    ResNo.CSCSTR_BadDelegateConstructor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MethodNameExpected,
                new ERRORINFO(
                    CSCERRID.ERR_MethodNameExpected,
                    149,
                    0,
                    ResNo.CSCSTR_MethodNameExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConstantExpected,
                new ERRORINFO(
                    CSCERRID.ERR_ConstantExpected,
                    150,
                    0,
                    ResNo.CSCSTR_ConstantExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IntegralTypeValueExpected,
                new ERRORINFO(
                    CSCERRID.ERR_IntegralTypeValueExpected,
                    151,
                    0,
                    ResNo.CSCSTR_IntegralTypeValueExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateCaseLabel,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateCaseLabel,
                    152,
                    0,
                    ResNo.CSCSTR_DuplicateCaseLabel));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidGotoCase,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidGotoCase,
                    153,
                    0,
                    ResNo.CSCSTR_InvalidGotoCase));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PropertyLacksGet,
                new ERRORINFO(
                    CSCERRID.ERR_PropertyLacksGet,
                    154,
                    0,
                    ResNo.CSCSTR_PropertyLacksGet));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadExceptionType,
                new ERRORINFO(
                    CSCERRID.ERR_BadExceptionType,
                    155,
                    0,
                    ResNo.CSCSTR_BadExceptionType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadEmptyThrow,
                new ERRORINFO(
                    CSCERRID.ERR_BadEmptyThrow,
                    156,
                    0,
                    ResNo.CSCSTR_BadEmptyThrow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadFinallyLeave,
                new ERRORINFO(
                    CSCERRID.ERR_BadFinallyLeave,
                    157,
                    0,
                    ResNo.CSCSTR_BadFinallyLeave));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LabelShadow,
                new ERRORINFO(
                    CSCERRID.ERR_LabelShadow,
                    158,
                    0,
                    ResNo.CSCSTR_LabelShadow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LabelNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_LabelNotFound,
                    159,
                    0,
                    ResNo.CSCSTR_LabelNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnreachableCatch,
                new ERRORINFO(
                    CSCERRID.ERR_UnreachableCatch,
                    160,
                    0,
                    ResNo.CSCSTR_UnreachableCatch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ReturnExpected,
                new ERRORINFO(
                    CSCERRID.ERR_ReturnExpected,
                    161,
                    0,
                    ResNo.CSCSTR_ReturnExpected));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreachableCode,
                new ERRORINFO(
                    CSCERRID.WRN_UnreachableCode,
                    162,
                    2,
                    ResNo.CSCSTR_UnreachableCode));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SwitchFallThrough,
                new ERRORINFO(
                    CSCERRID.ERR_SwitchFallThrough,
                    163,
                    0,
                    ResNo.CSCSTR_SwitchFallThrough));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreferencedLabel,
                new ERRORINFO(
                    CSCERRID.WRN_UnreferencedLabel,
                    164,
                    2,
                    ResNo.CSCSTR_UnreferencedLabel));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UseDefViolation,
                new ERRORINFO(
                    CSCERRID.ERR_UseDefViolation,
                    165,
                    0,
                    ResNo.CSCSTR_UseDefViolation));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoInvoke,
                new ERRORINFO(
                    CSCERRID.ERR_NoInvoke,
                    167,
                    0,
                    ResNo.CSCSTR_NoInvoke));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreferencedVar,
                new ERRORINFO(
                    CSCERRID.WRN_UnreferencedVar,
                    168,
                    3,
                    ResNo.CSCSTR_UnreferencedVar));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreferencedField,
                new ERRORINFO(
                    CSCERRID.WRN_UnreferencedField,
                    169,
                    3,
                    ResNo.CSCSTR_UnreferencedField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UseDefViolationField,
                new ERRORINFO(
                    CSCERRID.ERR_UseDefViolationField,
                    170,
                    0,
                    ResNo.CSCSTR_UseDefViolationField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnassignedThis,
                new ERRORINFO(
                    CSCERRID.ERR_UnassignedThis,
                    171,
                    0,
                    ResNo.CSCSTR_UnassignedThis));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigQM,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigQM,
                    172,
                    0,
                    ResNo.CSCSTR_AmbigQM));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidQM,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidQM,
                    173,
                    0,
                    ResNo.CSCSTR_InvalidQM));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoBaseClass,
                new ERRORINFO(
                    CSCERRID.ERR_NoBaseClass,
                    174,
                    0,
                    ResNo.CSCSTR_NoBaseClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BaseIllegal,
                new ERRORINFO(
                    CSCERRID.ERR_BaseIllegal,
                    175,
                    0,
                    ResNo.CSCSTR_BaseIllegal));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ObjectProhibited,
                new ERRORINFO(
                    CSCERRID.ERR_ObjectProhibited,
                    176,
                    0,
                    ResNo.CSCSTR_ObjectProhibited));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ParamUnassigned,
                new ERRORINFO(
                    CSCERRID.ERR_ParamUnassigned,
                    177,
                    0,
                    ResNo.CSCSTR_ParamUnassigned));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidArray,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidArray,
                    178,
                    0,
                    ResNo.CSCSTR_InvalidArray));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExternHasBody,
                new ERRORINFO(
                    CSCERRID.ERR_ExternHasBody,
                    179,
                    0,
                    ResNo.CSCSTR_ExternHasBody));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractAndExtern,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractAndExtern,
                    180,
                    0,
                    ResNo.CSCSTR_AbstractAndExtern));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadAttributeParam,
                new ERRORINFO(
                    CSCERRID.ERR_BadAttributeParam,
                    182,
                    0,
                    ResNo.CSCSTR_BadAttributeParam));
            ErrorInfoDic.Add(
                CSCERRID.WRN_IsAlwaysTrue,
                new ERRORINFO(
                    CSCERRID.WRN_IsAlwaysTrue,
                    183,
                    1,
                    ResNo.CSCSTR_IsAlwaysTrue));
            ErrorInfoDic.Add(
                CSCERRID.WRN_IsAlwaysFalse,
                new ERRORINFO(
                    CSCERRID.WRN_IsAlwaysFalse,
                    184,
                    1,
                    ResNo.CSCSTR_IsAlwaysFalse));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LockNeedsReference,
                new ERRORINFO(
                    CSCERRID.ERR_LockNeedsReference,
                    185,
                    0,
                    ResNo.CSCSTR_LockNeedsReference));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NullNotValid,
                new ERRORINFO(
                    CSCERRID.ERR_NullNotValid,
                    186,
                    0,
                    ResNo.CSCSTR_NullNotValid));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UseDefViolationThis,
                new ERRORINFO(
                    CSCERRID.ERR_UseDefViolationThis,
                    188,
                    0,
                    ResNo.CSCSTR_UseDefViolationThis));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FeatureNYI,
                new ERRORINFO(
                    CSCERRID.WRN_FeatureNYI,
                    189,
                    2,
                    ResNo.CSCSTR_FeatureNYI2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ArgsInvalid,
                new ERRORINFO(
                    CSCERRID.ERR_ArgsInvalid,
                    190,
                    0,
                    ResNo.CSCSTR_ArgsInvalid));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonly,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonly,
                    191,
                    0,
                    ResNo.CSCSTR_AssgReadonly));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonly,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonly,
                    192,
                    0,
                    ResNo.CSCSTR_RefReadonly));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PtrExpected,
                new ERRORINFO(
                    CSCERRID.ERR_PtrExpected,
                    193,
                    0,
                    ResNo.CSCSTR_PtrExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PtrIndexSingle,
                new ERRORINFO(
                    CSCERRID.ERR_PtrIndexSingle,
                    196,
                    0,
                    ResNo.CSCSTR_PtrIndexSingle));
            ErrorInfoDic.Add(
                CSCERRID.WRN_ByRefNonAgileField,
                new ERRORINFO(
                    CSCERRID.WRN_ByRefNonAgileField,
                    197,
                    1,
                    ResNo.CSCSTR_ByRefNonAgileField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonlyStatic,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonlyStatic,
                    198,
                    0,
                    ResNo.CSCSTR_AssgReadonlyStatic));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonlyStatic,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonlyStatic,
                    199,
                    0,
                    ResNo.CSCSTR_RefReadonlyStatic));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonlyProp,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonlyProp,
                    200,
                    0,
                    ResNo.CSCSTR_AssgReadonlyProp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalStatement,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalStatement,
                    201,
                    0,
                    ResNo.CSCSTR_IllegalStatement));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadGetEnumerator,
                new ERRORINFO(
                    CSCERRID.ERR_BadGetEnumerator,
                    202,
                    0,
                    ResNo.CSCSTR_BadGetEnumerator));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TooManyLocals,
                new ERRORINFO(
                    CSCERRID.ERR_TooManyLocals,
                    204,
                    0,
                    ResNo.CSCSTR_TooManyLocals));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractBaseCall,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractBaseCall,
                    205,
                    0,
                    ResNo.CSCSTR_AbstractBaseCall));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefProperty,
                new ERRORINFO(
                    CSCERRID.ERR_RefProperty,
                    206,
                    0,
                    ResNo.CSCSTR_RefProperty));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ManagedAddr,
                new ERRORINFO(
                    CSCERRID.ERR_ManagedAddr,
                    208,
                    0,
                    ResNo.CSCSTR_ManagedAddr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadFixedInitType,
                new ERRORINFO(
                    CSCERRID.ERR_BadFixedInitType,
                    209,
                    0,
                    ResNo.CSCSTR_BadFixedInitType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedMustInit,
                new ERRORINFO(
                    CSCERRID.ERR_FixedMustInit,
                    210,
                    0,
                    ResNo.CSCSTR_FixedMustInit));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidAddrOp,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidAddrOp,
                    211,
                    0,
                    ResNo.CSCSTR_InvalidAddrOp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedNeeded,
                new ERRORINFO(
                    CSCERRID.ERR_FixedNeeded,
                    212,
                    0,
                    ResNo.CSCSTR_FixedNeeded));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedNotNeeded,
                new ERRORINFO(
                    CSCERRID.ERR_FixedNotNeeded,
                    213,
                    0,
                    ResNo.CSCSTR_FixedNotNeeded));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnsafeNeeded,
                new ERRORINFO(
                    CSCERRID.ERR_UnsafeNeeded,
                    214,
                    0,
                    ResNo.CSCSTR_UnsafeNeeded));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OpTFRetType,
                new ERRORINFO(
                    CSCERRID.ERR_OpTFRetType,
                    215,
                    0,
                    ResNo.CSCSTR_OpTFRetType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OperatorNeedsMatch,
                new ERRORINFO(
                    CSCERRID.ERR_OperatorNeedsMatch,
                    216,
                    0,
                    ResNo.CSCSTR_OperatorNeedsMatch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadBoolOp,
                new ERRORINFO(
                    CSCERRID.ERR_BadBoolOp,
                    217,
                    0,
                    ResNo.CSCSTR_BadBoolOp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MustHaveOpTF,
                new ERRORINFO(
                    CSCERRID.ERR_MustHaveOpTF,
                    218,
                    0,
                    ResNo.CSCSTR_MustHaveOpTF));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreferencedVarAssg,
                new ERRORINFO(
                    CSCERRID.WRN_UnreferencedVarAssg,
                    219,
                    3,
                    ResNo.CSCSTR_UnreferencedVarAssg));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CheckedOverflow,
                new ERRORINFO(
                    CSCERRID.ERR_CheckedOverflow,
                    220,
                    0,
                    ResNo.CSCSTR_CheckedOverflow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConstOutOfRangeChecked,
                new ERRORINFO(
                    CSCERRID.ERR_ConstOutOfRangeChecked,
                    221,
                    0,
                    ResNo.CSCSTR_ConstOutOfRangeChecked));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVarargs,
                new ERRORINFO(
                    CSCERRID.ERR_BadVarargs,
                    224,
                    0,
                    ResNo.CSCSTR_BadVarargs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ParamsMustBeArray,
                new ERRORINFO(
                    CSCERRID.ERR_ParamsMustBeArray,
                    225,
                    0,
                    ResNo.CSCSTR_ParamsMustBeArray));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalArglist,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalArglist,
                    226,
                    0,
                    ResNo.CSCSTR_IllegalArglist));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalUnsafe,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalUnsafe,
                    227,
                    0,
                    ResNo.CSCSTR_IllegalUnsafe));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoAccessibleMember,
                new ERRORINFO(
                    CSCERRID.ERR_NoAccessibleMember,
                    228,
                    0,
                    ResNo.CSCSTR_NoAccessibleMember));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigMember,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigMember,
                    229,
                    0,
                    ResNo.CSCSTR_AmbigMember));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadForeachDecl,
                new ERRORINFO(
                    CSCERRID.ERR_BadForeachDecl,
                    230,
                    0,
                    ResNo.CSCSTR_BadForeachDecl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ParamsLast,
                new ERRORINFO(
                    CSCERRID.ERR_ParamsLast,
                    231,
                    0,
                    ResNo.CSCSTR_ParamsLast));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SizeofUnsafe,
                new ERRORINFO(
                    CSCERRID.ERR_SizeofUnsafe,
                    233,
                    0,
                    ResNo.CSCSTR_SizeofUnsafe));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DottedTypeNameNotFoundInNS,
                new ERRORINFO(
                    CSCERRID.ERR_DottedTypeNameNotFoundInNS,
                    234,
                    0,
                    ResNo.CSCSTR_DottedTypeNameNotFoundInNS));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FieldInitRefNonstatic,
                new ERRORINFO(
                    CSCERRID.ERR_FieldInitRefNonstatic,
                    236,
                    0,
                    ResNo.CSCSTR_FieldInitRefNonstatic));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SealedNonOverride,
                new ERRORINFO(
                    CSCERRID.ERR_SealedNonOverride,
                    238,
                    0,
                    ResNo.CSCSTR_SealedNonOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOverrideSealed,
                new ERRORINFO(
                    CSCERRID.ERR_CantOverrideSealed,
                    239,
                    0,
                    ResNo.CSCSTR_CantOverrideSealed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoDefaultArgs,
                new ERRORINFO(
                    CSCERRID.ERR_NoDefaultArgs,
                    241,
                    0,
                    ResNo.CSCSTR_NoDefaultArgs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_VoidError,
                new ERRORINFO(
                    CSCERRID.ERR_VoidError,
                    242,
                    0,
                    ResNo.CSCSTR_VoidError));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConditionalOnOverride,
                new ERRORINFO(
                    CSCERRID.ERR_ConditionalOnOverride,
                    243,
                    0,
                    ResNo.CSCSTR_ConditionalOnOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PointerInAsOrIs,
                new ERRORINFO(
                    CSCERRID.ERR_PointerInAsOrIs,
                    244,
                    0,
                    ResNo.CSCSTR_PointerInAsOrIs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CallingFinalizeDepracated,
                new ERRORINFO(
                    CSCERRID.ERR_CallingFinalizeDepracated,
                    245,
                    0,
                    ResNo.CSCSTR_CallingFinalizeDepracated));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SingleTypeNameNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    246,
                    0,
                    ResNo.CSCSTR_SingleTypeNameNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NegativeStackAllocSize,
                new ERRORINFO(
                    CSCERRID.ERR_NegativeStackAllocSize,
                    247,
                    0,
                    ResNo.CSCSTR_NegativeStackAllocSize));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NegativeArraySize,
                new ERRORINFO(
                    CSCERRID.ERR_NegativeArraySize,
                    248,
                    0,
                    ResNo.CSCSTR_NegativeArraySize));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OverrideFinalizeDeprecated,
                new ERRORINFO(
                    CSCERRID.ERR_OverrideFinalizeDeprecated,
                    249,
                    0,
                    ResNo.CSCSTR_OverrideFinalizeDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CallingBaseFinalizeDeprecated,
                new ERRORINFO(
                    CSCERRID.ERR_CallingBaseFinalizeDeprecated,
                    250,
                    0,
                    ResNo.CSCSTR_CallingBaseFinalizeDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NegativeArrayIndex,
                new ERRORINFO(
                    CSCERRID.WRN_NegativeArrayIndex,
                    251,
                    2,
                    ResNo.CSCSTR_NegativeArrayIndex));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadRefCompareLeft,
                new ERRORINFO(
                    CSCERRID.WRN_BadRefCompareLeft,
                    252,
                    2,
                    ResNo.CSCSTR_BadRefCompareLeft));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadRefCompareRight,
                new ERRORINFO(
                    CSCERRID.WRN_BadRefCompareRight,
                    253,
                    2,
                    ResNo.CSCSTR_BadRefCompareRight));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadCastInFixed,
                new ERRORINFO(
                    CSCERRID.ERR_BadCastInFixed,
                    254,
                    0,
                    ResNo.CSCSTR_BadCastInFixed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StackallocInCatchFinally,
                new ERRORINFO(
                    CSCERRID.ERR_StackallocInCatchFinally,
                    255,
                    0,
                    ResNo.CSCSTR_StackallocInCatchFinally));
            ErrorInfoDic.Add(
                CSCERRID.ERR_VarargsLast,
                new ERRORINFO(
                    CSCERRID.ERR_VarargsLast,
                    257,
                    0,
                    ResNo.CSCSTR_VarargsLast));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingPartial,
                new ERRORINFO(
                    CSCERRID.ERR_MissingPartial,
                    260,
                    0,
                    ResNo.CSCSTR_MissingPartial));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PartialTypeKindConflict,
                new ERRORINFO(
                    CSCERRID.ERR_PartialTypeKindConflict,
                    261,
                    0,
                    ResNo.CSCSTR_PartialTypeKindConflict));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PartialModifierConflict,
                new ERRORINFO(
                    CSCERRID.ERR_PartialModifierConflict,
                    262,
                    0,
                    ResNo.CSCSTR_PartialModifierConflict));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PartialMultipleBases,
                new ERRORINFO(
                    CSCERRID.ERR_PartialMultipleBases,
                    263,
                    0,
                    ResNo.CSCSTR_PartialMultipleBases));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PartialWrongTypeParams,
                new ERRORINFO(
                    CSCERRID.ERR_PartialWrongTypeParams,
                    264,
                    0,
                    ResNo.CSCSTR_PartialWrongTypeParams));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PartialWrongConstraints,
                new ERRORINFO(
                    CSCERRID.ERR_PartialWrongConstraints,
                    265,
                    0,
                    ResNo.CSCSTR_PartialWrongConstraints));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoImplicitConvCast,
                new ERRORINFO(
                    CSCERRID.ERR_NoImplicitConvCast,
                    266,
                    0,
                    ResNo.CSCSTR_NoImplicitConvCast));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PartialMisplaced,
                new ERRORINFO(
                    CSCERRID.ERR_PartialMisplaced,
                    267,
                    0,
                    ResNo.CSCSTR_PartialMisplaced));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ImportedCircularBase,
                new ERRORINFO(
                    CSCERRID.ERR_ImportedCircularBase,
                    268,
                    0,
                    ResNo.CSCSTR_ImportedCircularBase));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UseDefViolationOut,
                new ERRORINFO(
                    CSCERRID.ERR_UseDefViolationOut,
                    269,
                    0,
                    ResNo.CSCSTR_UseDefViolationOut));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ArraySizeInDeclaration,
                new ERRORINFO(
                    CSCERRID.ERR_ArraySizeInDeclaration,
                    270,
                    0,
                    ResNo.CSCSTR_ArraySizeInDeclaration));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InaccessibleGetter,
                new ERRORINFO(
                    CSCERRID.ERR_InaccessibleGetter,
                    271,
                    0,
                    ResNo.CSCSTR_InaccessibleGetter));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InaccessibleSetter,
                new ERRORINFO(
                    CSCERRID.ERR_InaccessibleSetter,
                    272,
                    0,
                    ResNo.CSCSTR_InaccessibleSetter));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidPropertyAccessMod,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidPropertyAccessMod,
                    273,
                    0,
                    ResNo.CSCSTR_InvalidPropertyAccessMod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicatePropertyAccessMods,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicatePropertyAccessMods,
                    274,
                    0,
                    ResNo.CSCSTR_DuplicatePropertyAccessMods));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PropertyAccessModInInterface,
                new ERRORINFO(
                    CSCERRID.ERR_PropertyAccessModInInterface,
                    275,
                    0,
                    ResNo.CSCSTR_PropertyAccessModInInterface));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AccessModMissingAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_AccessModMissingAccessor,
                    276,
                    0,
                    ResNo.CSCSTR_AccessModMissingAccessor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnimplementedInterfaceAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_UnimplementedInterfaceAccessor,
                    277,
                    0,
                    ResNo.CSCSTR_UnimplementedInterfaceAccessor));
            ErrorInfoDic.Add(
                CSCERRID.WRN_PatternIsAmbiguous,
                new ERRORINFO(
                    CSCERRID.WRN_PatternIsAmbiguous,
                    278,
                    2,
                    ResNo.CSCSTR_PatternIsAmbiguous));
            ErrorInfoDic.Add(
                CSCERRID.WRN_PatternStaticOrInaccessible,
                new ERRORINFO(
                    CSCERRID.WRN_PatternStaticOrInaccessible,
                    279,
                    2,
                    ResNo.CSCSTR_PatternStaticOrInaccessible));
            ErrorInfoDic.Add(
                CSCERRID.WRN_PatternBadSignature,
                new ERRORINFO(
                    CSCERRID.WRN_PatternBadSignature,
                    280,
                    2,
                    ResNo.CSCSTR_PatternBadSignature));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FriendRefNotEqualToThis,
                new ERRORINFO(
                    CSCERRID.ERR_FriendRefNotEqualToThis,
                    281,
                    0,
                    ResNo.CSCSTR_FriendRefNotEqualToThis));
            ErrorInfoDic.Add(
                CSCERRID.WRN_SequentialOnPartialClass,
                new ERRORINFO(
                    CSCERRID.WRN_SequentialOnPartialClass,
                    282,
                    3,
                    ResNo.CSCSTR_SequentialOnPartialClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadConstType,
                new ERRORINFO(
                    CSCERRID.ERR_BadConstType,
                    283,
                    0,
                    ResNo.CSCSTR_BadConstType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoNewTyvar,
                new ERRORINFO(
                    CSCERRID.ERR_NoNewTyvar,
                    304,
                    0,
                    ResNo.CSCSTR_NoNewTyvar));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArity,
                new ERRORINFO(
                    CSCERRID.ERR_BadArity,
                    305,
                    0,
                    ResNo.CSCSTR_BadArity));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadTypeArgument,
                new ERRORINFO(
                    CSCERRID.ERR_BadTypeArgument,
                    306,
                    0,
                    ResNo.CSCSTR_BadTypeArgument));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TypeArgsNotAllowed,
                new ERRORINFO(
                    CSCERRID.ERR_TypeArgsNotAllowed,
                    307,
                    0,
                    ResNo.CSCSTR_TypeArgsNotAllowed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_HasNoTypeVars,
                new ERRORINFO(
                    CSCERRID.ERR_HasNoTypeVars,
                    308,
                    0,
                    ResNo.CSCSTR_HasNoTypeVars));
            ErrorInfoDic.Add(
                CSCERRID.ERR_GenericConstraintNotSatisfied,
                new ERRORINFO(
                    CSCERRID.ERR_GenericConstraintNotSatisfied,
                    309,
                    0,
                    ResNo.CSCSTR_GenericConstraintNotSatisfied));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NewConstraintNotSatisfied,
                new ERRORINFO(
                    CSCERRID.ERR_NewConstraintNotSatisfied,
                    310,
                    0,
                    ResNo.CSCSTR_NewConstraintNotSatisfied));
            ErrorInfoDic.Add(
                CSCERRID.ERR_GlobalSingleTypeNameNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_GlobalSingleTypeNameNotFound,
                    400,
                    0,
                    ResNo.CSCSTR_GlobalSingleTypeNameNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NewBoundMustBeLast,
                new ERRORINFO(
                    CSCERRID.ERR_NewBoundMustBeLast,
                    401,
                    0,
                    ResNo.CSCSTR_NewBoundMustBeLast));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MainCantBeGeneric,
                new ERRORINFO(
                    CSCERRID.WRN_MainCantBeGeneric,
                    402,
                    4,
                    ResNo.CSCSTR_MainCantBeGeneric));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TypeVarCantBeNull,
                new ERRORINFO(
                    CSCERRID.ERR_TypeVarCantBeNull,
                    403,
                    0,
                    ResNo.CSCSTR_TypeVarCantBeNull));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AttributeCantBeGeneric,
                new ERRORINFO(
                    CSCERRID.ERR_AttributeCantBeGeneric,
                    404,
                    0,
                    ResNo.CSCSTR_AttributeCantBeGeneric));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateBound,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateBound,
                    405,
                    0,
                    ResNo.CSCSTR_DuplicateBound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ClassBoundNotFirst,
                new ERRORINFO(
                    CSCERRID.ERR_ClassBoundNotFirst,
                    406,
                    0,
                    ResNo.CSCSTR_ClassBoundNotFirst));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadRetType,
                new ERRORINFO(
                    CSCERRID.ERR_BadRetType,
                    407,
                    0,
                    ResNo.CSCSTR_BadRetType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateConstraintClause,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateConstraintClause,
                    409,
                    0,
                    ResNo.CSCSTR_DuplicateConstraintClause));
            ErrorInfoDic.Add(
                CSCERRID.ERR_WrongSignature,
                new ERRORINFO(
                    CSCERRID.ERR_WrongSignature,
                    410,
                    0,
                    ResNo.CSCSTR_WrongSignature));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantInferMethTypeArgs,
                new ERRORINFO(
                    CSCERRID.ERR_CantInferMethTypeArgs,
                    411,
                    0,
                    ResNo.CSCSTR_CantInferMethTypeArgs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LocalSameNameAsTypeParam,
                new ERRORINFO(
                    CSCERRID.ERR_LocalSameNameAsTypeParam,
                    412,
                    0,
                    ResNo.CSCSTR_LocalSameNameAsTypeParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AsWithTypeVar,
                new ERRORINFO(
                    CSCERRID.ERR_AsWithTypeVar,
                    413,
                    0,
                    ResNo.CSCSTR_AsWithTypeVar));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreferencedFieldAssg,
                new ERRORINFO(
                    CSCERRID.WRN_UnreferencedFieldAssg,
                    414,
                    3,
                    ResNo.CSCSTR_UnreferencedFieldAssg));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadIndexerNameAttr,
                new ERRORINFO(
                    CSCERRID.ERR_BadIndexerNameAttr,
                    415,
                    0,
                    ResNo.CSCSTR_BadIndexerNameAttr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AttrArgWithTypeVars,
                new ERRORINFO(
                    CSCERRID.ERR_AttrArgWithTypeVars,
                    416,
                    0,
                    ResNo.CSCSTR_AttrArgWithTypeVars));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NewTyvarWithArgs,
                new ERRORINFO(
                    CSCERRID.ERR_NewTyvarWithArgs,
                    417,
                    0,
                    ResNo.CSCSTR_NewTyvarWithArgs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractSealedStatic,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractSealedStatic,
                    418,
                    0,
                    ResNo.CSCSTR_AbstractSealedStatic));
            ErrorInfoDic.Add(
                CSCERRID.WRN_AmbiguousXMLReference,
                new ERRORINFO(
                    CSCERRID.WRN_AmbiguousXMLReference,
                    419,
                    3,
                    ResNo.CSCSTR_AmbiguousXMLReference));
            ErrorInfoDic.Add(
                CSCERRID.WRN_VolatileByRef,
                new ERRORINFO(
                    CSCERRID.WRN_VolatileByRef,
                    420,
                    1,
                    ResNo.CSCSTR_VolatileByRef));
            ErrorInfoDic.Add(
                CSCERRID.WRN_IncrSwitchObsolete,
                new ERRORINFO(
                    CSCERRID.WRN_IncrSwitchObsolete,
                    422,
                    4,
                    ResNo.CSCSTR_IncrSwitchObsolete));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ComImportWithImpl,
                new ERRORINFO(
                    CSCERRID.ERR_ComImportWithImpl,
                    423,
                    0,
                    ResNo.CSCSTR_ComImportWithImpl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ComImportWithBase,
                new ERRORINFO(
                    CSCERRID.ERR_ComImportWithBase,
                    424,
                    0,
                    ResNo.CSCSTR_ComImportWithBase));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ImplBadConstraints,
                new ERRORINFO(
                    CSCERRID.ERR_ImplBadConstraints,
                    425,
                    0,
                    ResNo.CSCSTR_ImplBadConstraints));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DottedTypeNameNotFoundInAgg,
                new ERRORINFO(
                    CSCERRID.ERR_DottedTypeNameNotFoundInAgg,
                    426,
                    0,
                    ResNo.CSCSTR_DottedTypeNameNotFoundInAgg));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MethGrpToNonDel,
                new ERRORINFO(
                    CSCERRID.ERR_MethGrpToNonDel,
                    428,
                    0,
                    ResNo.CSCSTR_MethGrpToNonDel));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreachableExpr,
                new ERRORINFO(
                    CSCERRID.WRN_UnreachableExpr,
                    429,
                    4,
                    ResNo.CSCSTR_UnreachableExpr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadExternAlias,
                new ERRORINFO(
                    CSCERRID.ERR_BadExternAlias,
                    430,
                    0,
                    ResNo.CSCSTR_BadExternAlias));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ColColWithTypeAlias,
                new ERRORINFO(
                    CSCERRID.ERR_ColColWithTypeAlias,
                    431,
                    0,
                    ResNo.CSCSTR_ColColWithTypeAlias));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AliasNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_AliasNotFound,
                    432,
                    0,
                    ResNo.CSCSTR_AliasNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SameFullNameAggAgg,
                new ERRORINFO(
                    CSCERRID.ERR_SameFullNameAggAgg,
                    433,
                    0,
                    ResNo.CSCSTR_SameFullNameAggAgg));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SameFullNameNsAgg,
                new ERRORINFO(
                    CSCERRID.ERR_SameFullNameNsAgg,
                    434,
                    0,
                    ResNo.CSCSTR_SameFullNameNsAgg));
            ErrorInfoDic.Add(
                CSCERRID.WRN_SameFullNameThisNsAgg,
                new ERRORINFO(
                    CSCERRID.WRN_SameFullNameThisNsAgg,
                    435,
                    2,
                    ResNo.CSCSTR_SameFullNameThisNsAgg));
            ErrorInfoDic.Add(
                CSCERRID.WRN_SameFullNameThisAggAgg,
                new ERRORINFO(
                    CSCERRID.WRN_SameFullNameThisAggAgg,
                    436,
                    2,
                    ResNo.CSCSTR_SameFullNameThisAggAgg));
            ErrorInfoDic.Add(
                CSCERRID.WRN_SameFullNameThisAggNs,
                new ERRORINFO(
                    CSCERRID.WRN_SameFullNameThisAggNs,
                    437,
                    2,
                    ResNo.CSCSTR_SameFullNameThisAggNs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SameFullNameThisAggThisNs,
                new ERRORINFO(
                    CSCERRID.ERR_SameFullNameThisAggThisNs,
                    438,
                    0,
                    ResNo.CSCSTR_SameFullNameThisAggThisNs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExternAfterElements,
                new ERRORINFO(
                    CSCERRID.ERR_ExternAfterElements,
                    439,
                    0,
                    ResNo.CSCSTR_ExternAfterElements));
            ErrorInfoDic.Add(
                CSCERRID.WRN_GlobalAliasDefn,
                new ERRORINFO(
                    CSCERRID.WRN_GlobalAliasDefn,
                    440,
                    2,
                    ResNo.CSCSTR_GlobalAliasDefn));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SealedStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_SealedStaticClass,
                    441,
                    0,
                    ResNo.CSCSTR_SealedStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PrivateAbstractAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_PrivateAbstractAccessor,
                    442,
                    0,
                    ResNo.CSCSTR_PrivateAbstractAccessor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ValueExpected,
                new ERRORINFO(
                    CSCERRID.ERR_ValueExpected,
                    443,
                    0,
                    ResNo.CSCSTR_ValueExpected));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnexpectedPredefTypeLoc,
                new ERRORINFO(
                    CSCERRID.WRN_UnexpectedPredefTypeLoc,
                    444,
                    2,
                    ResNo.CSCSTR_UnexpectedPredefTypeLoc));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnboxNotLValue,
                new ERRORINFO(
                    CSCERRID.ERR_UnboxNotLValue,
                    445,
                    0,
                    ResNo.CSCSTR_UnboxNotLValue));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AnonMethGrpInForEach,
                new ERRORINFO(
                    CSCERRID.ERR_AnonMethGrpInForEach,
                    446,
                    0,
                    ResNo.CSCSTR_AnonMethGrpInForEach));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AttrOnTypeArg,
                new ERRORINFO(
                    CSCERRID.ERR_AttrOnTypeArg,
                    447,
                    0,
                    ResNo.CSCSTR_AttrOnTypeArg));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadIncDecRetType,
                new ERRORINFO(
                    CSCERRID.ERR_BadIncDecRetType,
                    448,
                    0,
                    ResNo.CSCSTR_BadIncDecRetType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefValBoundMustBeFirst,
                new ERRORINFO(
                    CSCERRID.ERR_RefValBoundMustBeFirst,
                    449,
                    0,
                    ResNo.CSCSTR_RefValBoundMustBeFirst));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefValBoundWithClass,
                new ERRORINFO(
                    CSCERRID.ERR_RefValBoundWithClass,
                    450,
                    0,
                    ResNo.CSCSTR_RefValBoundWithClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NewBoundWithVal,
                new ERRORINFO(
                    CSCERRID.ERR_NewBoundWithVal,
                    451,
                    0,
                    ResNo.CSCSTR_NewBoundWithVal));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefConstraintNotSatisfied,
                new ERRORINFO(
                    CSCERRID.ERR_RefConstraintNotSatisfied,
                    452,
                    0,
                    ResNo.CSCSTR_RefConstraintNotSatisfied));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ValConstraintNotSatisfied,
                new ERRORINFO(
                    CSCERRID.ERR_ValConstraintNotSatisfied,
                    453,
                    0,
                    ResNo.CSCSTR_ValConstraintNotSatisfied));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CircularConstraint,
                new ERRORINFO(
                    CSCERRID.ERR_CircularConstraint,
                    454,
                    0,
                    ResNo.CSCSTR_CircularConstraint));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BaseConstraintConflict,
                new ERRORINFO(
                    CSCERRID.ERR_BaseConstraintConflict,
                    455,
                    0,
                    ResNo.CSCSTR_BaseConstraintConflict));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConWithValCon,
                new ERRORINFO(
                    CSCERRID.ERR_ConWithValCon,
                    456,
                    0,
                    ResNo.CSCSTR_ConWithValCon));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigUDConv,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigUDConv,
                    457,
                    0,
                    ResNo.CSCSTR_AmbigUDConv));
            ErrorInfoDic.Add(
                CSCERRID.WRN_AlwaysNull,
                new ERRORINFO(
                    CSCERRID.WRN_AlwaysNull,
                    458,
                    2,
                    ResNo.CSCSTR_AlwaysNull));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AddrOnReadOnlyLocal,
                new ERRORINFO(
                    CSCERRID.ERR_AddrOnReadOnlyLocal,
                    459,
                    0,
                    ResNo.CSCSTR_AddrOnReadOnlyLocal));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OverrideWithConstraints,
                new ERRORINFO(
                    CSCERRID.ERR_OverrideWithConstraints,
                    460,
                    0,
                    ResNo.CSCSTR_OverrideWithConstraints));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigOverride,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigOverride,
                    462,
                    0,
                    ResNo.CSCSTR_AmbigOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DecConstError,
                new ERRORINFO(
                    CSCERRID.ERR_DecConstError,
                    463,
                    0,
                    ResNo.CSCSTR_DecConstError));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CmpAlwaysFalse,
                new ERRORINFO(
                    CSCERRID.WRN_CmpAlwaysFalse,
                    464,
                    2,
                    ResNo.CSCSTR_CmpAlwaysFalse));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FinalizeMethod,
                new ERRORINFO(
                    CSCERRID.WRN_FinalizeMethod,
                    465,
                    1,
                    ResNo.CSCSTR_FinalizeMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitImplParams,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitImplParams,
                    466,
                    0,
                    ResNo.CSCSTR_ExplicitImplParams));
            ErrorInfoDic.Add(
                CSCERRID.WRN_AmbigLookupMeth,
                new ERRORINFO(
                    CSCERRID.WRN_AmbigLookupMeth,
                    467,
                    2,
                    ResNo.CSCSTR_AmbigLookupMeth));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SameFullNameThisAggThisAgg,
                new ERRORINFO(
                    CSCERRID.ERR_SameFullNameThisAggThisAgg,
                    468,
                    0,
                    ResNo.CSCSTR_SameFullNameThisAggThisAgg));
            ErrorInfoDic.Add(
                CSCERRID.WRN_GotoCaseShouldConvert,
                new ERRORINFO(
                    CSCERRID.WRN_GotoCaseShouldConvert,
                    469,
                    2,
                    ResNo.CSCSTR_GotoCaseShouldConvert));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MethodImplementingAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_MethodImplementingAccessor,
                    470,
                    0,
                    ResNo.CSCSTR_MethodImplementingAccessor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TypeArgsNotAllowedAmbig,
                new ERRORINFO(
                    CSCERRID.ERR_TypeArgsNotAllowedAmbig,
                    471,
                    0,
                    ResNo.CSCSTR_TypeArgsNotAllowedAmbig));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NubExprIsConstBool,
                new ERRORINFO(
                    CSCERRID.WRN_NubExprIsConstBool,
                    472,
                    2,
                    ResNo.CSCSTR_NubExprIsConstBool));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractHasBody,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractHasBody,
                    500,
                    0,
                    ResNo.CSCSTR_AbstractHasBody));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConcreteMissingBody,
                new ERRORINFO(
                    CSCERRID.ERR_ConcreteMissingBody,
                    501,
                    0,
                    ResNo.CSCSTR_ConcreteMissingBody));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractAndSealed,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractAndSealed,
                    502,
                    0,
                    ResNo.CSCSTR_AbstractAndSealed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractNotVirtual,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractNotVirtual,
                    503,
                    0,
                    ResNo.CSCSTR_AbstractNotVirtual));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticConstant,
                new ERRORINFO(
                    CSCERRID.ERR_StaticConstant,
                    504,
                    0,
                    ResNo.CSCSTR_StaticConstant));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOverrideNonFunction,
                new ERRORINFO(
                    CSCERRID.ERR_CantOverrideNonFunction,
                    505,
                    0,
                    ResNo.CSCSTR_CantOverrideNonFunction));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOverrideNonVirtual,
                new ERRORINFO(
                    CSCERRID.ERR_CantOverrideNonVirtual,
                    506,
                    0,
                    ResNo.CSCSTR_CantOverrideNonVirtual));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantChangeAccessOnOverride,
                new ERRORINFO(
                    CSCERRID.ERR_CantChangeAccessOnOverride,
                    507,
                    0,
                    ResNo.CSCSTR_CantChangeAccessOnOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantChangeReturnTypeOnOverride,
                new ERRORINFO(
                    CSCERRID.ERR_CantChangeReturnTypeOnOverride,
                    508,
                    0,
                    ResNo.CSCSTR_CantChangeReturnTypeOnOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantDeriveFromSealedType,
                new ERRORINFO(
                    CSCERRID.ERR_CantDeriveFromSealedType,
                    509,
                    0,
                    ResNo.CSCSTR_CantDeriveFromSealedType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractInConcreteClass,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractInConcreteClass,
                    513,
                    0,
                    ResNo.CSCSTR_AbstractInConcreteClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticConstructorWithExplicitConstructorCall,
                new ERRORINFO(
                    CSCERRID.ERR_StaticConstructorWithExplicitConstructorCall,
                    514,
                    0,
                    ResNo.CSCSTR_StaticConstructorWithExplicitConstructorCall));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticConstructorWithAccessModifiers,
                new ERRORINFO(
                    CSCERRID.ERR_StaticConstructorWithAccessModifiers,
                    515,
                    0,
                    ResNo.CSCSTR_StaticConstructorWithAccessModifiers));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RecursiveConstructorCall,
                new ERRORINFO(
                    CSCERRID.ERR_RecursiveConstructorCall,
                    516,
                    0,
                    ResNo.CSCSTR_RecursiveConstructorCall));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ObjectCallingBaseConstructor,
                new ERRORINFO(
                    CSCERRID.ERR_ObjectCallingBaseConstructor,
                    517,
                    0,
                    ResNo.CSCSTR_ObjectCallingBaseConstructor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PredefinedTypeNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_PredefinedTypeNotFound,
                    518,
                    0,
                    ResNo.CSCSTR_PredefinedTypeNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PredefinedTypeBadType,
                new ERRORINFO(
                    CSCERRID.ERR_PredefinedTypeBadType,
                    520,
                    0,
                    ResNo.CSCSTR_PredefinedTypeBadType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StructWithBaseConstructorCall,
                new ERRORINFO(
                    CSCERRID.ERR_StructWithBaseConstructorCall,
                    522,
                    0,
                    ResNo.CSCSTR_StructWithBaseConstructorCall));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StructLayoutCycle,
                new ERRORINFO(
                    CSCERRID.ERR_StructLayoutCycle,
                    523,
                    0,
                    ResNo.CSCSTR_StructLayoutCycle));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfacesCannotContainTypes,
                new ERRORINFO(
                    CSCERRID.ERR_InterfacesCannotContainTypes,
                    524,
                    0,
                    ResNo.CSCSTR_InterfacesCannotContainTypes));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfacesCantContainFields,
                new ERRORINFO(
                    CSCERRID.ERR_InterfacesCantContainFields,
                    525,
                    0,
                    ResNo.CSCSTR_InterfacesCantContainFields));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfacesCantContainConstructors,
                new ERRORINFO(
                    CSCERRID.ERR_InterfacesCantContainConstructors,
                    526,
                    0,
                    ResNo.CSCSTR_InterfacesCantContainConstructors));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NonInterfaceInInterfaceList,
                new ERRORINFO(
                    CSCERRID.ERR_NonInterfaceInInterfaceList,
                    527,
                    0,
                    ResNo.CSCSTR_NonInterfaceInInterfaceList));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateInterfaceInBaseList,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateInterfaceInBaseList,
                    528,
                    0,
                    ResNo.CSCSTR_DuplicateInterfaceInBaseList));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CycleInInterfaceInheritance,
                new ERRORINFO(
                    CSCERRID.ERR_CycleInInterfaceInheritance,
                    529,
                    0,
                    ResNo.CSCSTR_CycleInInterfaceInheritance));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfaceMemberHasBody,
                new ERRORINFO(
                    CSCERRID.ERR_InterfaceMemberHasBody,
                    531,
                    0,
                    ResNo.CSCSTR_InterfaceMemberHasBody));
            ErrorInfoDic.Add(
                CSCERRID.ERR_HidingAbstractMethod,
                new ERRORINFO(
                    CSCERRID.ERR_HidingAbstractMethod,
                    533,
                    0,
                    ResNo.CSCSTR_HidingAbstractMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnimplementedAbstractMethod,
                new ERRORINFO(
                    CSCERRID.ERR_UnimplementedAbstractMethod,
                    534,
                    0,
                    ResNo.CSCSTR_UnimplementedAbstractMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnimplementedInterfaceMember,
                new ERRORINFO(
                    CSCERRID.ERR_UnimplementedInterfaceMember,
                    535,
                    0,
                    ResNo.CSCSTR_UnimplementedInterfaceMember));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CloseUnimplementedInterfaceMember,
                new ERRORINFO(
                    CSCERRID.ERR_CloseUnimplementedInterfaceMember,
                    536,
                    0,
                    ResNo.CSCSTR_CloseUnimplementedInterfaceMember));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ObjectCantHaveBases,
                new ERRORINFO(
                    CSCERRID.ERR_ObjectCantHaveBases,
                    537,
                    0,
                    ResNo.CSCSTR_ObjectCantHaveBases));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitInterfaceImplementationNotInterface,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitInterfaceImplementationNotInterface,
                    538,
                    0,
                    ResNo.CSCSTR_ExplicitInterfaceImplementationNotInterface));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfaceMemberNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_InterfaceMemberNotFound,
                    539,
                    0,
                    ResNo.CSCSTR_InterfaceMemberNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ClassDoesntImplementInterface,
                new ERRORINFO(
                    CSCERRID.ERR_ClassDoesntImplementInterface,
                    540,
                    0,
                    ResNo.CSCSTR_ClassDoesntImplementInterface));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitInterfaceImplementationInNonClassOrStruct,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitInterfaceImplementationInNonClassOrStruct,
                    541,
                    0,
                    ResNo.CSCSTR_ExplicitInterfaceImplementationInNonClassOrStruct));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MemberNameSameAsType,
                new ERRORINFO(
                    CSCERRID.ERR_MemberNameSameAsType,
                    542,
                    0,
                    ResNo.CSCSTR_MemberNameSameAsType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EnumeratorOverflow,
                new ERRORINFO(
                    CSCERRID.ERR_EnumeratorOverflow,
                    543,
                    0,
                    ResNo.CSCSTR_EnumeratorOverflow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOverrideNonProperty,
                new ERRORINFO(
                    CSCERRID.ERR_CantOverrideNonProperty,
                    544,
                    0,
                    ResNo.CSCSTR_CantOverrideNonProperty));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoGetToOverride,
                new ERRORINFO(
                    CSCERRID.ERR_NoGetToOverride,
                    545,
                    0,
                    ResNo.CSCSTR_NoGetToOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoSetToOverride,
                new ERRORINFO(
                    CSCERRID.ERR_NoSetToOverride,
                    546,
                    0,
                    ResNo.CSCSTR_NoSetToOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PropertyCantHaveVoidType,
                new ERRORINFO(
                    CSCERRID.ERR_PropertyCantHaveVoidType,
                    547,
                    0,
                    ResNo.CSCSTR_PropertyCantHaveVoidType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PropertyWithNoAccessors,
                new ERRORINFO(
                    CSCERRID.ERR_PropertyWithNoAccessors,
                    548,
                    0,
                    ResNo.CSCSTR_PropertyWithNoAccessors));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NewVirtualInSealed,
                new ERRORINFO(
                    CSCERRID.ERR_NewVirtualInSealed,
                    549,
                    0,
                    ResNo.CSCSTR_NewVirtualInSealed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitPropertyAddingAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitPropertyAddingAccessor,
                    550,
                    0,
                    ResNo.CSCSTR_ExplicitPropertyAddingAccessor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitPropertyMissingAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitPropertyMissingAccessor,
                    551,
                    0,
                    ResNo.CSCSTR_ExplicitPropertyMissingAccessor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConversionWithInterface,
                new ERRORINFO(
                    CSCERRID.ERR_ConversionWithInterface,
                    552,
                    0,
                    ResNo.CSCSTR_ConversionWithInterface));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConversionWithBase,
                new ERRORINFO(
                    CSCERRID.ERR_ConversionWithBase,
                    553,
                    0,
                    ResNo.CSCSTR_ConversionWithBase));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConversionWithDerived,
                new ERRORINFO(
                    CSCERRID.ERR_ConversionWithDerived,
                    554,
                    0,
                    ResNo.CSCSTR_ConversionWithDerived));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IdentityConversion,
                new ERRORINFO(
                    CSCERRID.ERR_IdentityConversion,
                    555,
                    0,
                    ResNo.CSCSTR_IdentityConversion));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConversionNotInvolvingContainedType,
                new ERRORINFO(
                    CSCERRID.ERR_ConversionNotInvolvingContainedType,
                    556,
                    0,
                    ResNo.CSCSTR_ConversionNotInvolvingContainedType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateConversionInClass,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateConversionInClass,
                    557,
                    0,
                    ResNo.CSCSTR_DuplicateConversionInClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OperatorsMustBeStatic,
                new ERRORINFO(
                    CSCERRID.ERR_OperatorsMustBeStatic,
                    558,
                    0,
                    ResNo.CSCSTR_OperatorsMustBeStatic));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadIncDecsignature,
                new ERRORINFO(
                    CSCERRID.ERR_BadIncDecsignature,
                    559,
                    0,
                    ResNo.CSCSTR_BadIncDecsignature));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadUnaryOperatorSignature,
                new ERRORINFO(
                    CSCERRID.ERR_BadUnaryOperatorSignature,
                    562,
                    0,
                    ResNo.CSCSTR_BadUnaryOperatorSignature));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadBinaryOperatorSignature,
                new ERRORINFO(
                    CSCERRID.ERR_BadBinaryOperatorSignature,
                    563,
                    0,
                    ResNo.CSCSTR_BadBinaryOperatorSignature));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadShiftOperatorSignature,
                new ERRORINFO(
                    CSCERRID.ERR_BadShiftOperatorSignature,
                    564,
                    0,
                    ResNo.CSCSTR_BadShiftOperatorSignature));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfacesCantContainOperators,
                new ERRORINFO(
                    CSCERRID.ERR_InterfacesCantContainOperators,
                    567,
                    0,
                    ResNo.CSCSTR_InterfacesCantContainOperators));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StructsCantContainDefaultContructor,
                new ERRORINFO(
                    CSCERRID.ERR_StructsCantContainDefaultContructor,
                    568,
                    0,
                    ResNo.CSCSTR_StructsCantContainDefaultContructor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOverrideBogusMethod,
                new ERRORINFO(
                    CSCERRID.ERR_CantOverrideBogusMethod,
                    569,
                    0,
                    ResNo.CSCSTR_CantOverrideBogusMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BindToBogus,
                new ERRORINFO(
                    CSCERRID.ERR_BindToBogus,
                    570,
                    0,
                    ResNo.CSCSTR_BindToBogus));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantCallSpecialMethod,
                new ERRORINFO(
                    CSCERRID.ERR_CantCallSpecialMethod,
                    571,
                    0,
                    ResNo.CSCSTR_CantCallSpecialMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadTypeReference,
                new ERRORINFO(
                    CSCERRID.ERR_BadTypeReference,
                    572,
                    0,
                    ResNo.CSCSTR_BadTypeReference));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FieldInitializerInStruct,
                new ERRORINFO(
                    CSCERRID.ERR_FieldInitializerInStruct,
                    573,
                    0,
                    ResNo.CSCSTR_FieldInitializerInStruct));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadDestructorName,
                new ERRORINFO(
                    CSCERRID.ERR_BadDestructorName,
                    574,
                    0,
                    ResNo.CSCSTR_BadDestructorName));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OnlyClassesCanContainDestructors,
                new ERRORINFO(
                    CSCERRID.ERR_OnlyClassesCanContainDestructors,
                    575,
                    0,
                    ResNo.CSCSTR_OnlyClassesCanContainDestructors));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConflictAliasAndMember,
                new ERRORINFO(
                    CSCERRID.ERR_ConflictAliasAndMember,
                    576,
                    0,
                    ResNo.CSCSTR_ConflictAliasAndMember));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConditionalOnSpecialMethod,
                new ERRORINFO(
                    CSCERRID.ERR_ConditionalOnSpecialMethod,
                    577,
                    0,
                    ResNo.CSCSTR_ConditionalOnSpecialMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConditionalMustReturnVoid,
                new ERRORINFO(
                    CSCERRID.ERR_ConditionalMustReturnVoid,
                    578,
                    0,
                    ResNo.CSCSTR_ConditionalMustReturnVoid));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateAttribute,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateAttribute,
                    579,
                    0,
                    ResNo.CSCSTR_DuplicateAttribute));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConditionalOnInterfaceMethod,
                new ERRORINFO(
                    CSCERRID.ERR_ConditionalOnInterfaceMethod,
                    582,
                    0,
                    ResNo.CSCSTR_ConditionalOnInterfaceMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ICE_Culprit,
                new ERRORINFO(
                    CSCERRID.ERR_ICE_Culprit,
                    583,
                    0,
                    ResNo.CSCSTR_ICE_Culprit));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ICE_Symbol,
                new ERRORINFO(
                    CSCERRID.ERR_ICE_Symbol,
                    584,
                    0,
                    ResNo.CSCSTR_ICE_Symbol));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ICE_Node,
                new ERRORINFO(
                    CSCERRID.ERR_ICE_Node,
                    585,
                    0,
                    ResNo.CSCSTR_ICE_Node));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ICE_File,
                new ERRORINFO(
                    CSCERRID.ERR_ICE_File,
                    586,
                    0,
                    ResNo.CSCSTR_ICE_File));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ICE_Stage,
                new ERRORINFO(
                    CSCERRID.ERR_ICE_Stage,
                    587,
                    0,
                    ResNo.CSCSTR_ICE_Stage));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ICE_Lexer,
                new ERRORINFO(
                    CSCERRID.ERR_ICE_Lexer,
                    588,
                    0,
                    ResNo.CSCSTR_ICE_Lexer));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ICE_Parser,
                new ERRORINFO(
                    CSCERRID.ERR_ICE_Parser,
                    589,
                    0,
                    ResNo.CSCSTR_ICE_Parser));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OperatorCantReturnVoid,
                new ERRORINFO(
                    CSCERRID.ERR_OperatorCantReturnVoid,
                    590,
                    0,
                    ResNo.CSCSTR_OperatorCantReturnVoid));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidAttributeArgument,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidAttributeArgument,
                    591,
                    0,
                    ResNo.CSCSTR_InvalidAttributeArgument));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AttributeOnBadSymbolType,
                new ERRORINFO(
                    CSCERRID.ERR_AttributeOnBadSymbolType,
                    592,
                    0,
                    ResNo.CSCSTR_AttributeOnBadSymbolType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FloatOverflow,
                new ERRORINFO(
                    CSCERRID.ERR_FloatOverflow,
                    594,
                    0,
                    ResNo.CSCSTR_FloatOverflow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ComImportWithoutUuidAttribute,
                new ERRORINFO(
                    CSCERRID.ERR_ComImportWithoutUuidAttribute,
                    596,
                    0,
                    ResNo.CSCSTR_ComImportWithoutUuidAttribute));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidNamedArgument,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidNamedArgument,
                    599,
                    0,
                    ResNo.CSCSTR_InvalidNamedArgument));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DllImportOnInvalidMethod,
                new ERRORINFO(
                    CSCERRID.ERR_DllImportOnInvalidMethod,
                    601,
                    0,
                    ResNo.CSCSTR_DllImportOnInvalidMethod));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FeatureDeprecated,
                new ERRORINFO(
                    CSCERRID.WRN_FeatureDeprecated,
                    602,
                    1,
                    ResNo.CSCSTR_FeatureDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NameAttributeOnOverride,
                new ERRORINFO(
                    CSCERRID.ERR_NameAttributeOnOverride,
                    609,
                    0,
                    ResNo.CSCSTR_NameAttributeOnOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FieldCantBeRefAny,
                new ERRORINFO(
                    CSCERRID.ERR_FieldCantBeRefAny,
                    610,
                    0,
                    ResNo.CSCSTR_FieldCantBeRefAny));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ArrayElementCantBeRefAny,
                new ERRORINFO(
                    CSCERRID.ERR_ArrayElementCantBeRefAny,
                    611,
                    0,
                    ResNo.CSCSTR_ArrayElementCantBeRefAny));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DeprecatedSymbol,
                new ERRORINFO(
                    CSCERRID.WRN_DeprecatedSymbol,
                    612,
                    1,
                    ResNo.CSCSTR_DeprecatedSymbol));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NotAnAttributeClass,
                new ERRORINFO(
                    CSCERRID.ERR_NotAnAttributeClass,
                    616,
                    0,
                    ResNo.CSCSTR_NotAnAttributeClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadNamedAttributeArgument,
                new ERRORINFO(
                    CSCERRID.ERR_BadNamedAttributeArgument,
                    617,
                    0,
                    ResNo.CSCSTR_BadNamedAttributeArgument));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DeprecatedSymbolStr,
                new ERRORINFO(
                    CSCERRID.WRN_DeprecatedSymbolStr,
                    618,
                    2,
                    ResNo.CSCSTR_DeprecatedSymbolStr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DeprecatedSymbolStr,
                new ERRORINFO(
                    CSCERRID.ERR_DeprecatedSymbolStr,
                    619,
                    0,
                    ResNo.CSCSTR_DeprecatedSymbolStr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IndexerCantHaveVoidType,
                new ERRORINFO(
                    CSCERRID.ERR_IndexerCantHaveVoidType,
                    620,
                    0,
                    ResNo.CSCSTR_IndexerCantHaveVoidType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_VirtualPrivate,
                new ERRORINFO(
                    CSCERRID.ERR_VirtualPrivate,
                    621,
                    0,
                    ResNo.CSCSTR_VirtualPrivate));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ArrayInitToNonArrayType,
                new ERRORINFO(
                    CSCERRID.ERR_ArrayInitToNonArrayType,
                    622,
                    0,
                    ResNo.CSCSTR_ArrayInitToNonArrayType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ArrayInitInBadPlace,
                new ERRORINFO(
                    CSCERRID.ERR_ArrayInitInBadPlace,
                    623,
                    0,
                    ResNo.CSCSTR_ArrayInitInBadPlace));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingStructOffset,
                new ERRORINFO(
                    CSCERRID.ERR_MissingStructOffset,
                    625,
                    0,
                    ResNo.CSCSTR_MissingStructOffset));
            ErrorInfoDic.Add(
                CSCERRID.WRN_ExternMethodNoImplementation,
                new ERRORINFO(
                    CSCERRID.WRN_ExternMethodNoImplementation,
                    626,
                    1,
                    ResNo.CSCSTR_ExternMethodNoImplementation));
            ErrorInfoDic.Add(
                CSCERRID.WRN_ProtectedInSealed,
                new ERRORINFO(
                    CSCERRID.WRN_ProtectedInSealed,
                    628,
                    4,
                    ResNo.CSCSTR_ProtectedInSealed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InterfaceImplementedByConditional,
                new ERRORINFO(
                    CSCERRID.ERR_InterfaceImplementedByConditional,
                    629,
                    0,
                    ResNo.CSCSTR_InterfaceImplementedByConditional));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalRefParam,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalRefParam,
                    631,
                    0,
                    ResNo.CSCSTR_IllegalRefParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArgumentToAttribute,
                new ERRORINFO(
                    CSCERRID.ERR_BadArgumentToAttribute,
                    633,
                    0,
                    ResNo.CSCSTR_BadArgumentToAttribute));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OnlyValidOnCustomMarshaller,
                new ERRORINFO(
                    CSCERRID.ERR_OnlyValidOnCustomMarshaller,
                    634,
                    0,
                    ResNo.CSCSTR_OnlyValidOnCustomMarshaller));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingComTypeOrMarshaller,
                new ERRORINFO(
                    CSCERRID.ERR_MissingComTypeOrMarshaller,
                    635,
                    0,
                    ResNo.CSCSTR_MissingComTypeOrMarshaller));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StructOffsetOnBadStruct,
                new ERRORINFO(
                    CSCERRID.ERR_StructOffsetOnBadStruct,
                    636,
                    0,
                    ResNo.CSCSTR_StructOffsetOnBadStruct));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StructOffsetOnBadField,
                new ERRORINFO(
                    CSCERRID.ERR_StructOffsetOnBadField,
                    637,
                    0,
                    ResNo.CSCSTR_StructOffsetOnBadField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AttributeUsageOnNonAttributeClass,
                new ERRORINFO(
                    CSCERRID.ERR_AttributeUsageOnNonAttributeClass,
                    641,
                    0,
                    ResNo.CSCSTR_AttributeUsageOnNonAttributeClass));
            ErrorInfoDic.Add(
                CSCERRID.WRN_PossibleMistakenNullStatement,
                new ERRORINFO(
                    CSCERRID.WRN_PossibleMistakenNullStatement,
                    642,
                    3,
                    ResNo.CSCSTR_PossibleMistakenNullStatement));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateNamedAttributeArgument,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateNamedAttributeArgument,
                    643,
                    0,
                    ResNo.CSCSTR_DuplicateNamedAttributeArgument));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DeriveFromEnumOrValueType,
                new ERRORINFO(
                    CSCERRID.ERR_DeriveFromEnumOrValueType,
                    644,
                    0,
                    ResNo.CSCSTR_DeriveFromEnumOrValueType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IdentifierTooLong,
                new ERRORINFO(
                    CSCERRID.ERR_IdentifierTooLong,
                    645,
                    0,
                    ResNo.CSCSTR_IdentifierTooLong));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DefaultMemberOnIndexedType,
                new ERRORINFO(
                    CSCERRID.ERR_DefaultMemberOnIndexedType,
                    646,
                    0,
                    ResNo.CSCSTR_DefaultMemberOnIndexedType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CustomAttributeError,
                new ERRORINFO(
                    CSCERRID.ERR_CustomAttributeError,
                    647,
                    0,
                    ResNo.CSCSTR_CustomAttributeError));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BogusType,
                new ERRORINFO(
                    CSCERRID.ERR_BogusType,
                    648,
                    0,
                    ResNo.CSCSTR_BogusType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnassignedInternalField,
                new ERRORINFO(
                    CSCERRID.WRN_UnassignedInternalField,
                    649,
                    4,
                    ResNo.CSCSTR_UnassignedInternalField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CStyleArray,
                new ERRORINFO(
                    CSCERRID.ERR_CStyleArray,
                    650,
                    0,
                    ResNo.CSCSTR_CStyleArray));
            ErrorInfoDic.Add(
                CSCERRID.WRN_VacuousIntegralComp,
                new ERRORINFO(
                    CSCERRID.WRN_VacuousIntegralComp,
                    652,
                    2,
                    ResNo.CSCSTR_VacuousIntegralComp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractAttributeClass,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractAttributeClass,
                    653,
                    0,
                    ResNo.CSCSTR_AbstractAttributeClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadNamedAttributeArgumentType,
                new ERRORINFO(
                    CSCERRID.ERR_BadNamedAttributeArgumentType,
                    655,
                    0,
                    ResNo.CSCSTR_BadNamedAttributeArgumentType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingPredefinedMember,
                new ERRORINFO(
                    CSCERRID.ERR_MissingPredefinedMember,
                    656,
                    0,
                    ResNo.CSCSTR_MissingPredefinedMember));
            ErrorInfoDic.Add(
                CSCERRID.WRN_AttributeLocationOnBadDeclaration,
                new ERRORINFO(
                    CSCERRID.WRN_AttributeLocationOnBadDeclaration,
                    657,
                    1,
                    ResNo.CSCSTR_AttributeLocationOnBadDeclaration));
            ErrorInfoDic.Add(
                CSCERRID.WRN_InvalidAttributeLocation,
                new ERRORINFO(
                    CSCERRID.WRN_InvalidAttributeLocation,
                    658,
                    1,
                    ResNo.CSCSTR_InvalidAttributeLocation));
            ErrorInfoDic.Add(
                CSCERRID.WRN_EqualsWithoutGetHashCode,
                new ERRORINFO(
                    CSCERRID.WRN_EqualsWithoutGetHashCode,
                    659,
                    3,
                    ResNo.CSCSTR_EqualsWithoutGetHashCode));
            ErrorInfoDic.Add(
                CSCERRID.WRN_EqualityOpWithoutEquals,
                new ERRORINFO(
                    CSCERRID.WRN_EqualityOpWithoutEquals,
                    660,
                    3,
                    ResNo.CSCSTR_EqualityOpWithoutEquals));
            ErrorInfoDic.Add(
                CSCERRID.WRN_EqualityOpWithoutGetHashCode,
                new ERRORINFO(
                    CSCERRID.WRN_EqualityOpWithoutGetHashCode,
                    661,
                    3,
                    ResNo.CSCSTR_EqualityOpWithoutGetHashCode));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OutAttrOnRefParam,
                new ERRORINFO(
                    CSCERRID.ERR_OutAttrOnRefParam,
                    662,
                    0,
                    ResNo.CSCSTR_OutAttrOnRefParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OverloadRefOut,
                new ERRORINFO(
                    CSCERRID.ERR_OverloadRefOut,
                    663,
                    0,
                    ResNo.CSCSTR_OverloadRefOut));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LiteralDoubleCast,
                new ERRORINFO(
                    CSCERRID.ERR_LiteralDoubleCast,
                    664,
                    0,
                    ResNo.CSCSTR_LiteralDoubleCast));
            ErrorInfoDic.Add(
                CSCERRID.WRN_IncorrectBooleanAssg,
                new ERRORINFO(
                    CSCERRID.WRN_IncorrectBooleanAssg,
                    665,
                    3,
                    ResNo.CSCSTR_IncorrectBooleanAssg));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ProtectedInStruct,
                new ERRORINFO(
                    CSCERRID.ERR_ProtectedInStruct,
                    666,
                    0,
                    ResNo.CSCSTR_ProtectedInStruct));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FeatureDeprecated,
                new ERRORINFO(
                    CSCERRID.ERR_FeatureDeprecated,
                    667,
                    0,
                    ResNo.CSCSTR_FeatureDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InconsistantIndexerNames,
                new ERRORINFO(
                    CSCERRID.ERR_InconsistantIndexerNames,
                    668,
                    0,
                    ResNo.CSCSTR_InconsistantIndexerNames));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ComImportWithUserCtor,
                new ERRORINFO(
                    CSCERRID.ERR_ComImportWithUserCtor,
                    669,
                    0,
                    ResNo.CSCSTR_ComImportWithUserCtor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FieldCantHaveVoidType,
                new ERRORINFO(
                    CSCERRID.ERR_FieldCantHaveVoidType,
                    670,
                    0,
                    ResNo.CSCSTR_FieldCantHaveVoidType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NonObsoleteOverridingObsolete,
                new ERRORINFO(
                    CSCERRID.WRN_NonObsoleteOverridingObsolete,
                    672,
                    1,
                    ResNo.CSCSTR_NonObsoleteOverridingObsolete));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SystemVoid,
                new ERRORINFO(
                    CSCERRID.ERR_SystemVoid,
                    673,
                    0,
                    ResNo.CSCSTR_SystemVoid));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitParamArray,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitParamArray,
                    674,
                    0,
                    ResNo.CSCSTR_ExplicitParamArray));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BitwiseOrSignExtend,
                new ERRORINFO(
                    CSCERRID.WRN_BitwiseOrSignExtend,
                    675,
                    3,
                    ResNo.CSCSTR_BitwiseOrSignExtend));
            ErrorInfoDic.Add(
                CSCERRID.ERR_VolatileStruct,
                new ERRORINFO(
                    CSCERRID.ERR_VolatileStruct,
                    677,
                    0,
                    ResNo.CSCSTR_VolatileStruct));
            ErrorInfoDic.Add(
                CSCERRID.ERR_VolatileAndReadonly,
                new ERRORINFO(
                    CSCERRID.ERR_VolatileAndReadonly,
                    678,
                    0,
                    ResNo.CSCSTR_VolatileAndReadonly));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AbstractField,
                new ERRORINFO(
                    CSCERRID.ERR_AbstractField,
                    681,
                    0,
                    ResNo.CSCSTR_AbstractField));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BogusExplicitImpl,
                new ERRORINFO(
                    CSCERRID.ERR_BogusExplicitImpl,
                    682,
                    0,
                    ResNo.CSCSTR_BogusExplicitImpl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExplicitMethodImplAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_ExplicitMethodImplAccessor,
                    683,
                    0,
                    ResNo.CSCSTR_ExplicitMethodImplAccessor));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CoClassWithoutComImport,
                new ERRORINFO(
                    CSCERRID.WRN_CoClassWithoutComImport,
                    684,
                    1,
                    ResNo.CSCSTR_CoClassWithoutComImport));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConditionalWithOutParam,
                new ERRORINFO(
                    CSCERRID.ERR_ConditionalWithOutParam,
                    685,
                    0,
                    ResNo.CSCSTR_ConditionalWithOutParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AccessorImplementingMethod,
                new ERRORINFO(
                    CSCERRID.ERR_AccessorImplementingMethod,
                    686,
                    0,
                    ResNo.CSCSTR_AccessorImplementingMethod));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AliasQualAsExpression,
                new ERRORINFO(
                    CSCERRID.ERR_AliasQualAsExpression,
                    687,
                    0,
                    ResNo.CSCSTR_AliasQualAsExpression));
            ErrorInfoDic.Add(
                CSCERRID.WRN_LinkDemandOnOverride,
                new ERRORINFO(
                    CSCERRID.WRN_LinkDemandOnOverride,
                    688,
                    1,
                    ResNo.CSCSTR_LinkDemandOnOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DerivingFromATyVar,
                new ERRORINFO(
                    CSCERRID.ERR_DerivingFromATyVar,
                    689,
                    0,
                    ResNo.CSCSTR_DerivingFromATyVar));
            ErrorInfoDic.Add(
                CSCERRID.FTL_MalformedMetadata,
                new ERRORINFO(
                    CSCERRID.FTL_MalformedMetadata,
                    690,
                    -1,
                    ResNo.CSCSTR_MalformedMetadata));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateTypeParameter,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateTypeParameter,
                    692,
                    0,
                    ResNo.CSCSTR_DuplicateTypeParameter));
            ErrorInfoDic.Add(
                CSCERRID.WRN_TypeParameterSameAsOuterTypeParameter,
                new ERRORINFO(
                    CSCERRID.WRN_TypeParameterSameAsOuterTypeParameter,
                    693,
                    3,
                    ResNo.CSCSTR_TypeParameterSameAsOuterTypeParameter));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TypeVariableSameAsParent,
                new ERRORINFO(
                    CSCERRID.ERR_TypeVariableSameAsParent,
                    694,
                    0,
                    ResNo.CSCSTR_TypeVariableSameAsParent));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnifyingInterfaceInstantiations,
                new ERRORINFO(
                    CSCERRID.ERR_UnifyingInterfaceInstantiations,
                    695,
                    0,
                    ResNo.CSCSTR_UnifyingInterfaceInstantiations));
            ErrorInfoDic.Add(
                CSCERRID.ERR_GenericDerivingFromAttribute,
                new ERRORINFO(
                    CSCERRID.ERR_GenericDerivingFromAttribute,
                    698,
                    0,
                    ResNo.CSCSTR_GenericDerivingFromAttribute));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TyVarNotFoundInConstraint,
                new ERRORINFO(
                    CSCERRID.ERR_TyVarNotFoundInConstraint,
                    699,
                    0,
                    ResNo.CSCSTR_TyVarNotFoundInConstraint));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadBoundType,
                new ERRORINFO(
                    CSCERRID.ERR_BadBoundType,
                    701,
                    0,
                    ResNo.CSCSTR_BadBoundType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SpecialTypeAsBound,
                new ERRORINFO(
                    CSCERRID.ERR_SpecialTypeAsBound,
                    702,
                    0,
                    ResNo.CSCSTR_SpecialTypeAsBound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVisBound,
                new ERRORINFO(
                    CSCERRID.ERR_BadVisBound,
                    703,
                    0,
                    ResNo.CSCSTR_BadVisBound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LookupInTypeVariable,
                new ERRORINFO(
                    CSCERRID.ERR_LookupInTypeVariable,
                    704,
                    0,
                    ResNo.CSCSTR_LookupInTypeVariable));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadConstraintType,
                new ERRORINFO(
                    CSCERRID.ERR_BadConstraintType,
                    706,
                    0,
                    ResNo.CSCSTR_BadConstraintType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InstanceMemberInStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_InstanceMemberInStaticClass,
                    708,
                    0,
                    ResNo.CSCSTR_InstanceMemberInStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticBaseClass,
                new ERRORINFO(
                    CSCERRID.ERR_StaticBaseClass,
                    709,
                    0,
                    ResNo.CSCSTR_StaticBaseClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConstructorInStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_ConstructorInStaticClass,
                    710,
                    0,
                    ResNo.CSCSTR_ConstructorInStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DestructorInStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_DestructorInStaticClass,
                    711,
                    0,
                    ResNo.CSCSTR_DestructorInStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InstantiatingStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_InstantiatingStaticClass,
                    712,
                    0,
                    ResNo.CSCSTR_InstantiatingStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticDerivedFromNonObject,
                new ERRORINFO(
                    CSCERRID.ERR_StaticDerivedFromNonObject,
                    713,
                    0,
                    ResNo.CSCSTR_StaticDerivedFromNonObject));
            ErrorInfoDic.Add(
                CSCERRID.ERR_StaticClassInterfaceImpl,
                new ERRORINFO(
                    CSCERRID.ERR_StaticClassInterfaceImpl,
                    714,
                    0,
                    ResNo.CSCSTR_StaticClassInterfaceImpl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OperatorInStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_OperatorInStaticClass,
                    715,
                    0,
                    ResNo.CSCSTR_OperatorInStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConvertToStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_ConvertToStaticClass,
                    716,
                    0,
                    ResNo.CSCSTR_ConvertToStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConstraintIsStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_ConstraintIsStaticClass,
                    717,
                    0,
                    ResNo.CSCSTR_ConstraintIsStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_GenericArgIsStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_GenericArgIsStaticClass,
                    718,
                    0,
                    ResNo.CSCSTR_GenericArgIsStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ArrayOfStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_ArrayOfStaticClass,
                    719,
                    0,
                    ResNo.CSCSTR_ArrayOfStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IndexerInStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_IndexerInStaticClass,
                    720,
                    0,
                    ResNo.CSCSTR_IndexerInStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ParameterIsStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_ParameterIsStaticClass,
                    721,
                    0,
                    ResNo.CSCSTR_ParameterIsStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ReturnTypeIsStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_ReturnTypeIsStaticClass,
                    722,
                    0,
                    ResNo.CSCSTR_ReturnTypeIsStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_VarDeclIsStaticClass,
                new ERRORINFO(
                    CSCERRID.ERR_VarDeclIsStaticClass,
                    723,
                    0,
                    ResNo.CSCSTR_VarDeclIsStaticClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadEmptyThrowInFinally,
                new ERRORINFO(
                    CSCERRID.ERR_BadEmptyThrowInFinally,
                    724,
                    0,
                    ResNo.CSCSTR_BadEmptyThrowInFinally));
            ErrorInfoDic.Add(
                CSCERRID.WRN_AssignmentToLockOrDispose,
                new ERRORINFO(
                    CSCERRID.WRN_AssignmentToLockOrDispose,
                    728,
                    2,
                    ResNo.CSCSTR_AssignmentToLockOrDispose));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ForwardedTypeInThisAssembly,
                new ERRORINFO(
                    CSCERRID.ERR_ForwardedTypeInThisAssembly,
                    729,
                    0,
                    ResNo.CSCSTR_ForwardedTypeInThisAssembly));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ForwardedTypeIsNested,
                new ERRORINFO(
                    CSCERRID.ERR_ForwardedTypeIsNested,
                    730,
                    0,
                    ResNo.CSCSTR_ForwardedTypeIsNested));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CycleInTypeForwarder,
                new ERRORINFO(
                    CSCERRID.ERR_CycleInTypeForwarder,
                    731,
                    0,
                    ResNo.CSCSTR_CycleInTypeForwarder));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FwdedGeneric,
                new ERRORINFO(
                    CSCERRID.ERR_FwdedGeneric,
                    733,
                    0,
                    ResNo.CSCSTR_FwdedGeneric));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssemblyNameOnNonModule,
                new ERRORINFO(
                    CSCERRID.ERR_AssemblyNameOnNonModule,
                    734,
                    0,
                    ResNo.CSCSTR_AssemblyNameOnNonModule));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidFwdType,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidFwdType,
                    735,
                    0,
                    ResNo.CSCSTR_InvalidFwdType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IdentifierExpected,
                new ERRORINFO(
                    CSCERRID.ERR_IdentifierExpected,
                    1001,
                    0,
                    ResNo.CSCSTR_IdentifierExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SemicolonExpected,
                new ERRORINFO(
                    CSCERRID.ERR_SemicolonExpected,
                    1002,
                    0,
                    ResNo.CSCSTR_SemicolonExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SyntaxError,
                new ERRORINFO(
                    CSCERRID.ERR_SyntaxError,
                    1003,
                    0,
                    ResNo.CSCSTR_SyntaxError));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateModifier,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateModifier,
                    1004,
                    0,
                    ResNo.CSCSTR_DuplicateModifier));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateAccessor,
                    1007,
                    0,
                    ResNo.CSCSTR_DuplicateAccessor));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IntegralTypeExpected,
                new ERRORINFO(
                    CSCERRID.ERR_IntegralTypeExpected,
                    1008,
                    0,
                    ResNo.CSCSTR_IntegralTypeExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalEscape,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalEscape,
                    1009,
                    0,
                    ResNo.CSCSTR_IllegalEscape));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NewlineInConst,
                new ERRORINFO(
                    CSCERRID.ERR_NewlineInConst,
                    1010,
                    0,
                    ResNo.CSCSTR_NewlineInConst));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EmptyCharConst,
                new ERRORINFO(
                    CSCERRID.ERR_EmptyCharConst,
                    1011,
                    0,
                    ResNo.CSCSTR_EmptyCharConst));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TooManyCharsInConst,
                new ERRORINFO(
                    CSCERRID.ERR_TooManyCharsInConst,
                    1012,
                    0,
                    ResNo.CSCSTR_TooManyCharsInConst));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidNumber,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidNumber,
                    1013,
                    0,
                    ResNo.CSCSTR_InvalidNumber));
            ErrorInfoDic.Add(
                CSCERRID.ERR_GetOrSetExpected,
                new ERRORINFO(
                    CSCERRID.ERR_GetOrSetExpected,
                    1014,
                    0,
                    ResNo.CSCSTR_GetOrSetExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ClassTypeExpected,
                new ERRORINFO(
                    CSCERRID.ERR_ClassTypeExpected,
                    1015,
                    0,
                    ResNo.CSCSTR_ClassTypeExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NamedArgumentExpected,
                new ERRORINFO(
                    CSCERRID.ERR_NamedArgumentExpected,
                    1016,
                    0,
                    ResNo.CSCSTR_NamedArgumentExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TooManyCatches,
                new ERRORINFO(
                    CSCERRID.ERR_TooManyCatches,
                    1017,
                    0,
                    ResNo.CSCSTR_TooManyCatches));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ThisOrBaseExpected,
                new ERRORINFO(
                    CSCERRID.ERR_ThisOrBaseExpected,
                    1018,
                    0,
                    ResNo.CSCSTR_ThisOrBaseExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OvlUnaryOperatorExpected,
                new ERRORINFO(
                    CSCERRID.ERR_OvlUnaryOperatorExpected,
                    1019,
                    0,
                    ResNo.CSCSTR_OvlUnaryOperatorExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OvlBinaryOperatorExpected,
                new ERRORINFO(
                    CSCERRID.ERR_OvlBinaryOperatorExpected,
                    1020,
                    0,
                    ResNo.CSCSTR_OvlBinaryOperatorExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IntOverflow,
                new ERRORINFO(
                    CSCERRID.ERR_IntOverflow,
                    1021,
                    0,
                    ResNo.CSCSTR_IntOverflow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EOFExpected,
                new ERRORINFO(
                    CSCERRID.ERR_EOFExpected,
                    1022,
                    0,
                    ResNo.CSCSTR_EOFExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadEmbeddedStmt,
                new ERRORINFO(
                    CSCERRID.ERR_BadEmbeddedStmt,
                    1023,
                    0,
                    ResNo.CSCSTR_BadEmbeddedStmt));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PPDirectiveExpected,
                new ERRORINFO(
                    CSCERRID.ERR_PPDirectiveExpected,
                    1024,
                    0,
                    ResNo.CSCSTR_PPDirectiveExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EndOfPPLineExpected,
                new ERRORINFO(
                    CSCERRID.ERR_EndOfPPLineExpected,
                    1025,
                    0,
                    ResNo.CSCSTR_EndOfPPLineExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CloseParenExpected,
                new ERRORINFO(
                    CSCERRID.ERR_CloseParenExpected,
                    1026,
                    0,
                    ResNo.CSCSTR_CloseParenExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EndifDirectiveExpected,
                new ERRORINFO(
                    CSCERRID.ERR_EndifDirectiveExpected,
                    1027,
                    0,
                    ResNo.CSCSTR_EndifDirectiveExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnexpectedDirective,
                new ERRORINFO(
                    CSCERRID.ERR_UnexpectedDirective,
                    1028,
                    0,
                    ResNo.CSCSTR_UnexpectedDirective));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ErrorDirective,
                new ERRORINFO(
                    CSCERRID.ERR_ErrorDirective,
                    1029,
                    0,
                    ResNo.CSCSTR_ErrorDirective));
            ErrorInfoDic.Add(
                CSCERRID.WRN_WarningDirective,
                new ERRORINFO(
                    CSCERRID.WRN_WarningDirective,
                    1030,
                    1,
                    ResNo.CSCSTR_WarningDirective));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TypeExpected,
                new ERRORINFO(
                    CSCERRID.ERR_TypeExpected,
                    1031,
                    0,
                    ResNo.CSCSTR_TypeExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_PPDefFollowsToken,
                new ERRORINFO(
                    CSCERRID.ERR_PPDefFollowsToken,
                    1032,
                    0,
                    ResNo.CSCSTR_PPDefFollowsToken));
            ErrorInfoDic.Add(
                CSCERRID.ERR_TooManyLines,
                new ERRORINFO(
                    CSCERRID.ERR_TooManyLines,
                    1033,
                    0,
                    ResNo.CSCSTR_TooManyLines));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LineTooLong,
                new ERRORINFO(
                    CSCERRID.ERR_LineTooLong,
                    1034,
                    0,
                    ResNo.CSCSTR_LineTooLong));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OpenEndedComment,
                new ERRORINFO(
                    CSCERRID.ERR_OpenEndedComment,
                    1035,
                    0,
                    ResNo.CSCSTR_OpenEndedComment));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExpectedDotOrParen,
                new ERRORINFO(
                    CSCERRID.ERR_ExpectedDotOrParen,
                    1036,
                    0,
                    ResNo.CSCSTR_ExpectedDotOrParen));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OvlOperatorExpected,
                new ERRORINFO(
                    CSCERRID.ERR_OvlOperatorExpected,
                    1037,
                    0,
                    ResNo.CSCSTR_OvlOperatorExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EndRegionDirectiveExpected,
                new ERRORINFO(
                    CSCERRID.ERR_EndRegionDirectiveExpected,
                    1038,
                    0,
                    ResNo.CSCSTR_EndRegionDirectiveExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnterminatedStringLit,
                new ERRORINFO(
                    CSCERRID.ERR_UnterminatedStringLit,
                    1039,
                    0,
                    ResNo.CSCSTR_UnterminatedStringLit));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadDirectivePlacement,
                new ERRORINFO(
                    CSCERRID.ERR_BadDirectivePlacement,
                    1040,
                    0,
                    ResNo.CSCSTR_BadDirectivePlacement));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IdentifierExpectedKW,
                new ERRORINFO(
                    CSCERRID.ERR_IdentifierExpectedKW,
                    1041,
                    0,
                    ResNo.CSCSTR_IdentifierExpectedKW));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SemiOrLBraceExpected,
                new ERRORINFO(
                    CSCERRID.ERR_SemiOrLBraceExpected,
                    1043,
                    0,
                    ResNo.CSCSTR_SemiOrLBraceExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MultiTypeInDeclaration,
                new ERRORINFO(
                    CSCERRID.ERR_MultiTypeInDeclaration,
                    1044,
                    0,
                    ResNo.CSCSTR_MultiTypeInDeclaration));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AddOrRemoveExpected,
                new ERRORINFO(
                    CSCERRID.ERR_AddOrRemoveExpected,
                    1055,
                    0,
                    ResNo.CSCSTR_AddOrRemoveExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnexpectedCharacter,
                new ERRORINFO(
                    CSCERRID.ERR_UnexpectedCharacter,
                    1056,
                    0,
                    ResNo.CSCSTR_UnexpectedCharacter));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ProtectedInStatic,
                new ERRORINFO(
                    CSCERRID.ERR_ProtectedInStatic,
                    1057,
                    0,
                    ResNo.CSCSTR_ProtectedInStatic));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnreachableGeneralCatch,
                new ERRORINFO(
                    CSCERRID.WRN_UnreachableGeneralCatch,
                    1058,
                    1,
                    ResNo.CSCSTR_UnreachableGeneralCatch));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FeatureDeprecated2,
                new ERRORINFO(
                    CSCERRID.WRN_FeatureDeprecated2,
                    1200,
                    1,
                    ResNo.CSCSTR_FeatureDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FeatureDeprecated3,
                new ERRORINFO(
                    CSCERRID.WRN_FeatureDeprecated3,
                    1201,
                    1,
                    ResNo.CSCSTR_FeatureDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FeatureDeprecated4,
                new ERRORINFO(
                    CSCERRID.WRN_FeatureDeprecated4,
                    1202,
                    1,
                    ResNo.CSCSTR_FeatureDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FeatureDeprecated5,
                new ERRORINFO(
                    CSCERRID.WRN_FeatureDeprecated5,
                    1203,
                    1,
                    ResNo.CSCSTR_FeatureDeprecated));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArgCount,
                new ERRORINFO(
                    CSCERRID.ERR_BadArgCount,
                    1501,
                    0,
                    ResNo.CSCSTR_BadArgCount));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArgTypes,
                new ERRORINFO(
                    CSCERRID.ERR_BadArgTypes,
                    1502,
                    0,
                    ResNo.CSCSTR_BadArgTypes));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArgType,
                new ERRORINFO(
                    CSCERRID.ERR_BadArgType,
                    1503,
                    0,
                    ResNo.CSCSTR_BadArgType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoSourceFile,
                new ERRORINFO(
                    CSCERRID.ERR_NoSourceFile,
                    1504,
                    0,
                    ResNo.CSCSTR_NoSourceFile));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantRefResource,
                new ERRORINFO(
                    CSCERRID.ERR_CantRefResource,
                    1507,
                    0,
                    ResNo.CSCSTR_CantRefResource));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ResourceNotUnique,
                new ERRORINFO(
                    CSCERRID.ERR_ResourceNotUnique,
                    1508,
                    0,
                    ResNo.CSCSTR_ResourceNotUnique));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ImportNonAssembly,
                new ERRORINFO(
                    CSCERRID.ERR_ImportNonAssembly,
                    1509,
                    0,
                    ResNo.CSCSTR_ImportNonAssembly));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefLvalueExpected,
                new ERRORINFO(
                    CSCERRID.ERR_RefLvalueExpected,
                    1510,
                    0,
                    ResNo.CSCSTR_RefLvalueExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BaseInStaticMeth,
                new ERRORINFO(
                    CSCERRID.ERR_BaseInStaticMeth,
                    1511,
                    0,
                    ResNo.CSCSTR_BaseInStaticMeth));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BaseInBadContext,
                new ERRORINFO(
                    CSCERRID.ERR_BaseInBadContext,
                    1512,
                    0,
                    ResNo.CSCSTR_BaseInBadContext));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RbraceExpected,
                new ERRORINFO(
                    CSCERRID.ERR_RbraceExpected,
                    1513,
                    0,
                    ResNo.CSCSTR_RbraceExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LbraceExpected,
                new ERRORINFO(
                    CSCERRID.ERR_LbraceExpected,
                    1514,
                    0,
                    ResNo.CSCSTR_LbraceExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InExpected,
                new ERRORINFO(
                    CSCERRID.ERR_InExpected,
                    1515,
                    0,
                    ResNo.CSCSTR_InExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidPreprocExpr,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidPreprocExpr,
                    1517,
                    0,
                    ResNo.CSCSTR_InvalidPreprocExpr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadTokenInType,
                new ERRORINFO(
                    CSCERRID.ERR_BadTokenInType,
                    1518,
                    0,
                    ResNo.CSCSTR_BadTokenInType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidMemberDecl,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidMemberDecl,
                    1519,
                    0,
                    ResNo.CSCSTR_InvalidMemberDecl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MemberNeedsType,
                new ERRORINFO(
                    CSCERRID.ERR_MemberNeedsType,
                    1520,
                    0,
                    ResNo.CSCSTR_MemberNeedsType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadBaseType,
                new ERRORINFO(
                    CSCERRID.ERR_BadBaseType,
                    1521,
                    0,
                    ResNo.CSCSTR_BadBaseType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_EmptySwitch,
                new ERRORINFO(
                    CSCERRID.WRN_EmptySwitch,
                    1522,
                    1,
                    ResNo.CSCSTR_EmptySwitch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExpectedEndTry,
                new ERRORINFO(
                    CSCERRID.ERR_ExpectedEndTry,
                    1524,
                    0,
                    ResNo.CSCSTR_ExpectedEndTry));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidExprTerm,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidExprTerm,
                    1525,
                    0,
                    ResNo.CSCSTR_InvalidExprTerm));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadNewExpr,
                new ERRORINFO(
                    CSCERRID.ERR_BadNewExpr,
                    1526,
                    0,
                    ResNo.CSCSTR_BadNewExpr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoNamespacePrivate,
                new ERRORINFO(
                    CSCERRID.ERR_NoNamespacePrivate,
                    1527,
                    0,
                    ResNo.CSCSTR_NoNamespacePrivate));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadVarDecl,
                new ERRORINFO(
                    CSCERRID.ERR_BadVarDecl,
                    1528,
                    0,
                    ResNo.CSCSTR_BadVarDecl));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UsingAfterElements,
                new ERRORINFO(
                    CSCERRID.ERR_UsingAfterElements,
                    1529,
                    0,
                    ResNo.CSCSTR_UsingAfterElements));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoNewOnNamespaceElement,
                new ERRORINFO(
                    CSCERRID.ERR_NoNewOnNamespaceElement,
                    1530,
                    0,
                    ResNo.CSCSTR_NoNewOnNamespaceElement));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DontUseInvoke,
                new ERRORINFO(
                    CSCERRID.ERR_DontUseInvoke,
                    1533,
                    0,
                    ResNo.CSCSTR_DontUseInvoke));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadBinOpArgs,
                new ERRORINFO(
                    CSCERRID.ERR_BadBinOpArgs,
                    1534,
                    0,
                    ResNo.CSCSTR_BadBinOpArgs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadUnOpArgs,
                new ERRORINFO(
                    CSCERRID.ERR_BadUnOpArgs,
                    1535,
                    0,
                    ResNo.CSCSTR_BadUnOpArgs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoVoidParameter,
                new ERRORINFO(
                    CSCERRID.ERR_NoVoidParameter,
                    1536,
                    0,
                    ResNo.CSCSTR_NoVoidParameter));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateAlias,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateAlias,
                    1537,
                    0,
                    ResNo.CSCSTR_DuplicateAlias));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadProtectedAccess,
                new ERRORINFO(
                    CSCERRID.ERR_BadProtectedAccess,
                    1540,
                    0,
                    ResNo.CSCSTR_BadProtectedAccess));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantIncludeDirectory,
                new ERRORINFO(
                    CSCERRID.ERR_CantIncludeDirectory,
                    1541,
                    0,
                    ResNo.CSCSTR_CantIncludeDirectory));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AddModuleAssembly,
                new ERRORINFO(
                    CSCERRID.ERR_AddModuleAssembly,
                    1542,
                    0,
                    ResNo.CSCSTR_AddModuleAssembly));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BindToBogusProp2,
                new ERRORINFO(
                    CSCERRID.ERR_BindToBogusProp2,
                    1545,
                    0,
                    ResNo.CSCSTR_BindToBogusProp2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BindToBogusProp1,
                new ERRORINFO(
                    CSCERRID.ERR_BindToBogusProp1,
                    1546,
                    0,
                    ResNo.CSCSTR_BindToBogusProp1));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoVoidHere,
                new ERRORINFO(
                    CSCERRID.ERR_NoVoidHere,
                    1547,
                    0,
                    ResNo.CSCSTR_NoVoidHere));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CryptoFailed,
                new ERRORINFO(
                    CSCERRID.ERR_CryptoFailed,
                    1548,
                    0,
                    ResNo.CSCSTR_CryptoFailed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CryptoNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_CryptoNotFound,
                    1549,
                    0,
                    ResNo.CSCSTR_CryptoNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IndexerNeedsParam,
                new ERRORINFO(
                    CSCERRID.ERR_IndexerNeedsParam,
                    1551,
                    0,
                    ResNo.CSCSTR_IndexerNeedsParam));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArraySyntax,
                new ERRORINFO(
                    CSCERRID.ERR_BadArraySyntax,
                    1552,
                    0,
                    ResNo.CSCSTR_BadArraySyntax));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadOperatorSyntax,
                new ERRORINFO(
                    CSCERRID.ERR_BadOperatorSyntax,
                    1553,
                    0,
                    ResNo.CSCSTR_BadOperatorSyntax));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadOperatorSyntax2,
                new ERRORINFO(
                    CSCERRID.ERR_BadOperatorSyntax2,
                    1554,
                    0,
                    ResNo.CSCSTR_BadOperatorSyntax2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MainClassNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_MainClassNotFound,
                    1555,
                    0,
                    ResNo.CSCSTR_MainClassNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MainClassNotClass,
                new ERRORINFO(
                    CSCERRID.ERR_MainClassNotClass,
                    1556,
                    0,
                    ResNo.CSCSTR_MainClassNotClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MainClassWrongFile,
                new ERRORINFO(
                    CSCERRID.ERR_MainClassWrongFile,
                    1557,
                    0,
                    ResNo.CSCSTR_MainClassWrongFile));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoMainInClass,
                new ERRORINFO(
                    CSCERRID.ERR_NoMainInClass,
                    1558,
                    0,
                    ResNo.CSCSTR_NoMainInClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MainClassIsImport,
                new ERRORINFO(
                    CSCERRID.ERR_MainClassIsImport,
                    1559,
                    0,
                    ResNo.CSCSTR_MainClassIsImport));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FileNameTooLong,
                new ERRORINFO(
                    CSCERRID.ERR_FileNameTooLong,
                    1560,
                    0,
                    ResNo.CSCSTR_FileNameTooLong));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OutputFileNameTooLong,
                new ERRORINFO(
                    CSCERRID.ERR_OutputFileNameTooLong,
                    1561,
                    0,
                    ResNo.CSCSTR_OutputFileNameTooLong));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OutputNeedsName,
                new ERRORINFO(
                    CSCERRID.ERR_OutputNeedsName,
                    1562,
                    0,
                    ResNo.CSCSTR_OutputNeedsName));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OutputNeedsInput,
                new ERRORINFO(
                    CSCERRID.ERR_OutputNeedsInput,
                    1563,
                    0,
                    ResNo.CSCSTR_OutputNeedsInput));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantHaveWin32ResAndIcon,
                new ERRORINFO(
                    CSCERRID.ERR_CantHaveWin32ResAndIcon,
                    1565,
                    0,
                    ResNo.CSCSTR_CantHaveWin32ResAndIcon));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantReadResource,
                new ERRORINFO(
                    CSCERRID.ERR_CantReadResource,
                    1566,
                    0,
                    ResNo.CSCSTR_CantReadResource));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AutoResGen,
                new ERRORINFO(
                    CSCERRID.ERR_AutoResGen,
                    1567,
                    0,
                    ResNo.CSCSTR_AutoResGen));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DocFileGen,
                new ERRORINFO(
                    CSCERRID.ERR_DocFileGen,
                    1569,
                    0,
                    ResNo.CSCSTR_DocFileGen));
            ErrorInfoDic.Add(
                CSCERRID.WRN_XMLParseError,
                new ERRORINFO(
                    CSCERRID.WRN_XMLParseError,
                    1570,
                    1,
                    ResNo.CSCSTR_XMLParseError));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DuplicateParamTag,
                new ERRORINFO(
                    CSCERRID.WRN_DuplicateParamTag,
                    1571,
                    2,
                    ResNo.CSCSTR_DuplicateParamTag));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnmatchedParamTag,
                new ERRORINFO(
                    CSCERRID.WRN_UnmatchedParamTag,
                    1572,
                    2,
                    ResNo.CSCSTR_UnmatchedParamTag));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MissingParamTag,
                new ERRORINFO(
                    CSCERRID.WRN_MissingParamTag,
                    1573,
                    4,
                    ResNo.CSCSTR_MissingParamTag));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadXMLRef,
                new ERRORINFO(
                    CSCERRID.WRN_BadXMLRef,
                    1574,
                    1,
                    ResNo.CSCSTR_BadXMLRef));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadStackAllocExpr,
                new ERRORINFO(
                    CSCERRID.ERR_BadStackAllocExpr,
                    1575,
                    0,
                    ResNo.CSCSTR_BadStackAllocExpr));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidLineNumber,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidLineNumber,
                    1576,
                    0,
                    ResNo.CSCSTR_InvalidLineNumber));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ALinkFailed,
                new ERRORINFO(
                    CSCERRID.ERR_ALinkFailed,
                    1577,
                    0,
                    ResNo.CSCSTR_ALinkFailed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingPPFile,
                new ERRORINFO(
                    CSCERRID.ERR_MissingPPFile,
                    1578,
                    0,
                    ResNo.CSCSTR_MissingPPFile));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ForEachMissingMember,
                new ERRORINFO(
                    CSCERRID.ERR_ForEachMissingMember,
                    1579,
                    0,
                    ResNo.CSCSTR_ForEachMissingMember));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadXMLRefParamType,
                new ERRORINFO(
                    CSCERRID.WRN_BadXMLRefParamType,
                    1580,
                    1,
                    ResNo.CSCSTR_BadXMLRefParamType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadXMLRefReturnType,
                new ERRORINFO(
                    CSCERRID.WRN_BadXMLRefReturnType,
                    1581,
                    1,
                    ResNo.CSCSTR_BadXMLRefReturnType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadWin32Res,
                new ERRORINFO(
                    CSCERRID.ERR_BadWin32Res,
                    1583,
                    0,
                    ResNo.CSCSTR_BadWin32Res));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadXMLRefSyntax,
                new ERRORINFO(
                    CSCERRID.WRN_BadXMLRefSyntax,
                    1584,
                    1,
                    ResNo.CSCSTR_BadXMLRefSyntax));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadModifierLocation,
                new ERRORINFO(
                    CSCERRID.ERR_BadModifierLocation,
                    1585,
                    0,
                    ResNo.CSCSTR_BadModifierLocation));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingArraySize,
                new ERRORINFO(
                    CSCERRID.ERR_MissingArraySize,
                    1586,
                    0,
                    ResNo.CSCSTR_MissingArraySize));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnprocessedXMLComment,
                new ERRORINFO(
                    CSCERRID.WRN_UnprocessedXMLComment,
                    1587,
                    2,
                    ResNo.CSCSTR_UnprocessedXMLComment));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantGetCORSystemDir,
                new ERRORINFO(
                    CSCERRID.ERR_CantGetCORSystemDir,
                    1588,
                    0,
                    ResNo.CSCSTR_CantGetCORSystemDir));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FailedInclude,
                new ERRORINFO(
                    CSCERRID.WRN_FailedInclude,
                    1589,
                    1,
                    ResNo.CSCSTR_FailedInclude));
            ErrorInfoDic.Add(
                CSCERRID.WRN_InvalidInclude,
                new ERRORINFO(
                    CSCERRID.WRN_InvalidInclude,
                    1590,
                    1,
                    ResNo.CSCSTR_InvalidInclude));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MissingXMLComment,
                new ERRORINFO(
                    CSCERRID.WRN_MissingXMLComment,
                    1591,
                    4,
                    ResNo.CSCSTR_MissingXMLComment));
            ErrorInfoDic.Add(
                CSCERRID.WRN_XMLParseIncludeError,
                new ERRORINFO(
                    CSCERRID.WRN_XMLParseIncludeError,
                    1592,
                    1,
                    ResNo.CSCSTR_XMLParseIncludeError));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadDelArgCount,
                new ERRORINFO(
                    CSCERRID.ERR_BadDelArgCount,
                    1593,
                    0,
                    ResNo.CSCSTR_BadDelArgCount));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadDelArgTypes,
                new ERRORINFO(
                    CSCERRID.ERR_BadDelArgTypes,
                    1594,
                    0,
                    ResNo.CSCSTR_BadDelArgTypes));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnexpectedSemicolon,
                new ERRORINFO(
                    CSCERRID.ERR_UnexpectedSemicolon,
                    1597,
                    0,
                    ResNo.CSCSTR_UnexpectedSemicolon));
            ErrorInfoDic.Add(
                CSCERRID.WRN_XMLParserNotFound,
                new ERRORINFO(
                    CSCERRID.WRN_XMLParserNotFound,
                    1598,
                    1,
                    ResNo.CSCSTR_XMLParserNotFound));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MethodReturnCantBeRefAny,
                new ERRORINFO(
                    CSCERRID.ERR_MethodReturnCantBeRefAny,
                    1599,
                    0,
                    ResNo.CSCSTR_MethodReturnCantBeRefAny));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CompileCancelled,
                new ERRORINFO(
                    CSCERRID.ERR_CompileCancelled,
                    1600,
                    0,
                    ResNo.CSCSTR_CompileCancelled));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MethodArgCantBeRefAny,
                new ERRORINFO(
                    CSCERRID.ERR_MethodArgCantBeRefAny,
                    1601,
                    0,
                    ResNo.CSCSTR_MethodArgCantBeRefAny));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonlyLocal,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonlyLocal,
                    1604,
                    0,
                    ResNo.CSCSTR_AssgReadonlyLocal));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonlyLocal,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonlyLocal,
                    1605,
                    0,
                    ResNo.CSCSTR_RefReadonlyLocal));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ALinkCloseFailed,
                new ERRORINFO(
                    CSCERRID.ERR_ALinkCloseFailed,
                    1606,
                    0,
                    ResNo.CSCSTR_ALinkCloseFailed));
            ErrorInfoDic.Add(
                CSCERRID.WRN_ALinkWarn,
                new ERRORINFO(
                    CSCERRID.WRN_ALinkWarn,
                    1607,
                    1,
                    ResNo.CSCSTR_ALinkWarn));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantUseRequiredAttribute,
                new ERRORINFO(
                    CSCERRID.ERR_CantUseRequiredAttribute,
                    1608,
                    0,
                    ResNo.CSCSTR_CantUseRequiredAttribute));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoModifiersOnAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_NoModifiersOnAccessor,
                    1609,
                    0,
                    ResNo.CSCSTR_NoModifiersOnAccessor));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DeleteAutoResFailed,
                new ERRORINFO(
                    CSCERRID.WRN_DeleteAutoResFailed,
                    1610,
                    4,
                    ResNo.CSCSTR_DeleteAutoResFailed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ParamsCantBeRefOut,
                new ERRORINFO(
                    CSCERRID.ERR_ParamsCantBeRefOut,
                    1611,
                    0,
                    ResNo.CSCSTR_ParamsCantBeRefOut));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ReturnNotLValue,
                new ERRORINFO(
                    CSCERRID.ERR_ReturnNotLValue,
                    1612,
                    0,
                    ResNo.CSCSTR_ReturnNotLValue));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MissingCoClass,
                new ERRORINFO(
                    CSCERRID.ERR_MissingCoClass,
                    1613,
                    0,
                    ResNo.CSCSTR_MissingCoClass));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AmbigousAttribute,
                new ERRORINFO(
                    CSCERRID.ERR_AmbigousAttribute,
                    1614,
                    0,
                    ResNo.CSCSTR_AmbigousAttribute));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArgExtraRef,
                new ERRORINFO(
                    CSCERRID.ERR_BadArgExtraRef,
                    1615,
                    0,
                    ResNo.CSCSTR_BadArgExtraRef));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CmdOptionConflictsSource,
                new ERRORINFO(
                    CSCERRID.WRN_CmdOptionConflictsSource,
                    1616,
                    1,
                    ResNo.CSCSTR_CmdOptionConflictsSource));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadCompatMode,
                new ERRORINFO(
                    CSCERRID.ERR_BadCompatMode,
                    1617,
                    0,
                    ResNo.CSCSTR_BadCompatMode));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DelegateOnConditional,
                new ERRORINFO(
                    CSCERRID.ERR_DelegateOnConditional,
                    1618,
                    0,
                    ResNo.CSCSTR_DelegateOnConditional));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantMakeTempFile,
                new ERRORINFO(
                    CSCERRID.ERR_CantMakeTempFile,
                    1619,
                    0,
                    ResNo.CSCSTR_CantMakeTempFile));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadArgRef,
                new ERRORINFO(
                    CSCERRID.ERR_BadArgRef,
                    1620,
                    0,
                    ResNo.CSCSTR_BadArgRef));
            ErrorInfoDic.Add(
                CSCERRID.ERR_YieldInAnonMeth,
                new ERRORINFO(
                    CSCERRID.ERR_YieldInAnonMeth,
                    1621,
                    0,
                    ResNo.CSCSTR_YieldInAnonMeth));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ReturnInIterator,
                new ERRORINFO(
                    CSCERRID.ERR_ReturnInIterator,
                    1622,
                    0,
                    ResNo.CSCSTR_ReturnInIterator));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadIteratorArgType,
                new ERRORINFO(
                    CSCERRID.ERR_BadIteratorArgType,
                    1623,
                    0,
                    ResNo.CSCSTR_BadIteratorArgType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadIteratorReturn,
                new ERRORINFO(
                    CSCERRID.ERR_BadIteratorReturn,
                    1624,
                    0,
                    ResNo.CSCSTR_BadIteratorReturn));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadYieldInFinally,
                new ERRORINFO(
                    CSCERRID.ERR_BadYieldInFinally,
                    1625,
                    0,
                    ResNo.CSCSTR_BadYieldInFinally));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadYieldInTryOfCatch,
                new ERRORINFO(
                    CSCERRID.ERR_BadYieldInTryOfCatch,
                    1626,
                    0,
                    ResNo.CSCSTR_BadYieldInTryOfCatch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_EmptyYield,
                new ERRORINFO(
                    CSCERRID.ERR_EmptyYield,
                    1627,
                    0,
                    ResNo.CSCSTR_EmptyYield));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AnonDelegateCantUse,
                new ERRORINFO(
                    CSCERRID.ERR_AnonDelegateCantUse,
                    1628,
                    0,
                    ResNo.CSCSTR_AnonDelegateCantUse));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalInnerUnsafe,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalInnerUnsafe,
                    1629,
                    0,
                    ResNo.CSCSTR_IllegalInnerUnsafe));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadWatsonMode,
                new ERRORINFO(
                    CSCERRID.ERR_BadWatsonMode,
                    1630,
                    0,
                    ResNo.CSCSTR_BadWatsonMode));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadYieldInCatch,
                new ERRORINFO(
                    CSCERRID.ERR_BadYieldInCatch,
                    1631,
                    0,
                    ResNo.CSCSTR_BadYieldInCatch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadDelegateLeave,
                new ERRORINFO(
                    CSCERRID.ERR_BadDelegateLeave,
                    1632,
                    0,
                    ResNo.CSCSTR_BadDelegateLeave));
            ErrorInfoDic.Add(
                CSCERRID.WRN_IllegalPragma,
                new ERRORINFO(
                    CSCERRID.WRN_IllegalPragma,
                    1633,
                    1,
                    ResNo.CSCSTR_IllegalPragma));
            ErrorInfoDic.Add(
                CSCERRID.WRN_IllegalPPWarning,
                new ERRORINFO(
                    CSCERRID.WRN_IllegalPPWarning,
                    1634,
                    1,
                    ResNo.CSCSTR_IllegalPPWarning));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadRestoreNumber,
                new ERRORINFO(
                    CSCERRID.WRN_BadRestoreNumber,
                    1635,
                    1,
                    ResNo.CSCSTR_BadRestoreNumber));
            ErrorInfoDic.Add(
                CSCERRID.ERR_VarargsIterator,
                new ERRORINFO(
                    CSCERRID.ERR_VarargsIterator,
                    1636,
                    0,
                    ResNo.CSCSTR_VarargsIterator));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnsafeIteratorArgType,
                new ERRORINFO(
                    CSCERRID.ERR_UnsafeIteratorArgType,
                    1637,
                    0,
                    ResNo.CSCSTR_UnsafeIteratorArgType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ReservedIdentifier,
                new ERRORINFO(
                    CSCERRID.ERR_ReservedIdentifier,
                    1638,
                    0,
                    ResNo.CSCSTR_ReservedIdentifier2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadCoClassSig,
                new ERRORINFO(
                    CSCERRID.ERR_BadCoClassSig,
                    1639,
                    0,
                    ResNo.CSCSTR_BadCoClassSig));
            ErrorInfoDic.Add(
                CSCERRID.ERR_MultipleIEnumOfT,
                new ERRORINFO(
                    CSCERRID.ERR_MultipleIEnumOfT,
                    1640,
                    0,
                    ResNo.CSCSTR_MultipleIEnumOfT));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedDimsRequired,
                new ERRORINFO(
                    CSCERRID.ERR_FixedDimsRequired,
                    1641,
                    0,
                    ResNo.CSCSTR_FixedDimsRequired));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedNotInStruct,
                new ERRORINFO(
                    CSCERRID.ERR_FixedNotInStruct,
                    1642,
                    0,
                    ResNo.CSCSTR_FixedNotInStruct));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AnonymousReturnExpected,
                new ERRORINFO(
                    CSCERRID.ERR_AnonymousReturnExpected,
                    1643,
                    0,
                    ResNo.CSCSTR_AnonymousReturnExpected));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NonECMAFeature,
                new ERRORINFO(
                    CSCERRID.ERR_NonECMAFeature,
                    1644,
                    0,
                    ResNo.CSCSTR_NonECMAFeature));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NonECMAFeature,
                new ERRORINFO(
                    CSCERRID.WRN_NonECMAFeature,
                    1645,
                    1,
                    ResNo.CSCSTR_NonECMAFeatureOK));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ExpectedVerbatimLiteral,
                new ERRORINFO(
                    CSCERRID.ERR_ExpectedVerbatimLiteral,
                    1646,
                    0,
                    ResNo.CSCSTR_ExpectedVerbatimLiteral));
            ErrorInfoDic.Add(
                CSCERRID.FTL_StackOverflow,
                new ERRORINFO(
                    CSCERRID.FTL_StackOverflow,
                    1647,
                    -1,
                    ResNo.CSCSTR_StackOverflow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonly2,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonly2,
                    1648,
                    0,
                    ResNo.CSCSTR_AssgReadonly2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonly2,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonly2,
                    1649,
                    0,
                    ResNo.CSCSTR_RefReadonly2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonlyStatic2,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonlyStatic2,
                    1650,
                    0,
                    ResNo.CSCSTR_AssgReadonlyStatic2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonlyStatic2,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonlyStatic2,
                    1651,
                    0,
                    ResNo.CSCSTR_RefReadonlyStatic2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonlyLocal2,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonlyLocal2,
                    1652,
                    0,
                    ResNo.CSCSTR_AssgReadonlyLocal2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonlyLocal2,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonlyLocal2,
                    1653,
                    0,
                    ResNo.CSCSTR_RefReadonlyLocal2));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonlyLocal2Cause,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonlyLocal2Cause,
                    1654,
                    0,
                    ResNo.CSCSTR_AssgReadonlyLocal2Cause));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonlyLocal2Cause,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonlyLocal2Cause,
                    1655,
                    0,
                    ResNo.CSCSTR_RefReadonlyLocal2Cause));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssgReadonlyLocalCause,
                new ERRORINFO(
                    CSCERRID.ERR_AssgReadonlyLocalCause,
                    1656,
                    0,
                    ResNo.CSCSTR_AssgReadonlyLocalCause));
            ErrorInfoDic.Add(
                CSCERRID.ERR_RefReadonlyLocalCause,
                new ERRORINFO(
                    CSCERRID.ERR_RefReadonlyLocalCause,
                    1657,
                    0,
                    ResNo.CSCSTR_RefReadonlyLocalCause));
            ErrorInfoDic.Add(
                CSCERRID.WRN_ErrorOverride,
                new ERRORINFO(
                    CSCERRID.WRN_ErrorOverride,
                    1658,
                    1,
                    ResNo.CSCSTR_ErrorOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AnonMethToNonDel,
                new ERRORINFO(
                    CSCERRID.ERR_AnonMethToNonDel,
                    1660,
                    0,
                    ResNo.CSCSTR_AnonMethToNonDel));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantConvAnonMethParams,
                new ERRORINFO(
                    CSCERRID.ERR_CantConvAnonMethParams,
                    1661,
                    0,
                    ResNo.CSCSTR_CantConvAnonMethParams));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantConvAnonMethReturns,
                new ERRORINFO(
                    CSCERRID.ERR_CantConvAnonMethReturns,
                    1662,
                    0,
                    ResNo.CSCSTR_CantConvAnonMethReturns));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalFixedType,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalFixedType,
                    1663,
                    0,
                    ResNo.CSCSTR_IllegalFixedType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedOverflow,
                new ERRORINFO(
                    CSCERRID.ERR_FixedOverflow,
                    1664,
                    0,
                    ResNo.CSCSTR_FixedOverflow));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidFixedArraySize,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidFixedArraySize,
                    1665,
                    0,
                    ResNo.CSCSTR_InvalidFixedArraySize));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedBufferNotFixed,
                new ERRORINFO(
                    CSCERRID.ERR_FixedBufferNotFixed,
                    1666,
                    0,
                    ResNo.CSCSTR_FixedBufferNotFixed));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AttributeNotOnAccessor,
                new ERRORINFO(
                    CSCERRID.ERR_AttributeNotOnAccessor,
                    1667,
                    0,
                    ResNo.CSCSTR_AttributeNotOnAccessor));
            ErrorInfoDic.Add(
                CSCERRID.WRN_InvalidSearchPathDir,
                new ERRORINFO(
                    CSCERRID.WRN_InvalidSearchPathDir,
                    1668,
                    2,
                    ResNo.CSCSTR_InvalidSearchPathDir));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalVarArgs,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalVarArgs,
                    1669,
                    0,
                    ResNo.CSCSTR_IllegalVarArgs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalParams,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalParams,
                    1670,
                    0,
                    ResNo.CSCSTR_IllegalParams));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadModifiersOnNamespace,
                new ERRORINFO(
                    CSCERRID.ERR_BadModifiersOnNamespace,
                    1671,
                    0,
                    ResNo.CSCSTR_BadModifiersOnNamespace));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadPlatformType,
                new ERRORINFO(
                    CSCERRID.ERR_BadPlatformType,
                    1672,
                    0,
                    ResNo.CSCSTR_BadPlatformType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ThisStructNotInAnonMeth,
                new ERRORINFO(
                    CSCERRID.ERR_ThisStructNotInAnonMeth,
                    1673,
                    0,
                    ResNo.CSCSTR_ThisStructNotInAnonMeth));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoConvToIDisp,
                new ERRORINFO(
                    CSCERRID.ERR_NoConvToIDisp,
                    1674,
                    0,
                    ResNo.CSCSTR_NoConvToIDisp));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidGenericEnum,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidGenericEnum,
                    1675,
                    0,
                    ResNo.CSCSTR_InvalidGenericEnum));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadParamRef,
                new ERRORINFO(
                    CSCERRID.ERR_BadParamRef,
                    1676,
                    0,
                    ResNo.CSCSTR_BadParamRef));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadParamExtraRef,
                new ERRORINFO(
                    CSCERRID.ERR_BadParamExtraRef,
                    1677,
                    0,
                    ResNo.CSCSTR_BadParamExtraRef));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadParamType,
                new ERRORINFO(
                    CSCERRID.ERR_BadParamType,
                    1678,
                    0,
                    ResNo.CSCSTR_BadParamType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadExternIdentifier,
                new ERRORINFO(
                    CSCERRID.ERR_BadExternIdentifier,
                    1679,
                    0,
                    ResNo.CSCSTR_BadExternIdentifier));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AliasMissingFile,
                new ERRORINFO(
                    CSCERRID.ERR_AliasMissingFile,
                    1680,
                    0,
                    ResNo.CSCSTR_AliasMissingFile));
            ErrorInfoDic.Add(
                CSCERRID.ERR_GlobalExternAlias,
                new ERRORINFO(
                    CSCERRID.ERR_GlobalExternAlias,
                    1681,
                    0,
                    ResNo.CSCSTR_GlobalExternAlias));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MissingTypeNested,
                new ERRORINFO(
                    CSCERRID.WRN_MissingTypeNested,
                    1682,
                    1,
                    ResNo.CSCSTR_MissingTypeNested));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MissingTypeInSource,
                new ERRORINFO(
                    CSCERRID.WRN_MissingTypeInSource,
                    1683,
                    1,
                    ResNo.CSCSTR_MissingTypeInSource));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MissingTypeInAssembly,
                new ERRORINFO(
                    CSCERRID.WRN_MissingTypeInAssembly,
                    1684,
                    1,
                    ResNo.CSCSTR_MissingTypeInAssembly));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MultiplePredefTypes,
                new ERRORINFO(
                    CSCERRID.WRN_MultiplePredefTypes,
                    1685,
                    1,
                    ResNo.CSCSTR_MultiplePredefTypes));
            ErrorInfoDic.Add(
                CSCERRID.ERR_LocalCantBeFixedAndHoisted,
                new ERRORINFO(
                    CSCERRID.ERR_LocalCantBeFixedAndHoisted,
                    1686,
                    0,
                    ResNo.CSCSTR_LocalCantBeFixedAndHoisted));
            ErrorInfoDic.Add(
                CSCERRID.WRN_TooManyLinesForDebugger,
                new ERRORINFO(
                    CSCERRID.WRN_TooManyLinesForDebugger,
                    1687,
                    1,
                    ResNo.CSCSTR_TooManyLinesForDebugger));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantConvAnonMethNoParams,
                new ERRORINFO(
                    CSCERRID.ERR_CantConvAnonMethNoParams,
                    1688,
                    0,
                    ResNo.CSCSTR_CantConvAnonMethNoParams));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ConditionalOnNonAttributeClass,
                new ERRORINFO(
                    CSCERRID.ERR_ConditionalOnNonAttributeClass,
                    1689,
                    0,
                    ResNo.CSCSTR_ConditionalOnNonAttributeClass));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CallOnNonAgileField,
                new ERRORINFO(
                    CSCERRID.WRN_CallOnNonAgileField,
                    1690,
                    1,
                    ResNo.CSCSTR_CallOnNonAgileField));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadWarningNumber,
                new ERRORINFO(
                    CSCERRID.WRN_BadWarningNumber,
                    1691,
                    1,
                    ResNo.CSCSTR_BadWarningNumber));
            ErrorInfoDic.Add(
                CSCERRID.WRN_InvalidNumber,
                new ERRORINFO(
                    CSCERRID.WRN_InvalidNumber,
                    1692,
                    1,
                    ResNo.CSCSTR_InvalidNumber));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FileNameTooLong,
                new ERRORINFO(
                    CSCERRID.WRN_FileNameTooLong,
                    1694,
                    1,
                    ResNo.CSCSTR_FileNameTooLong));
            ErrorInfoDic.Add(
                CSCERRID.WRN_IllegalPPChecksum,
                new ERRORINFO(
                    CSCERRID.WRN_IllegalPPChecksum,
                    1695,
                    1,
                    ResNo.CSCSTR_IllegalPPChecksum));
            ErrorInfoDic.Add(
                CSCERRID.WRN_EndOfPPLineExpected,
                new ERRORINFO(
                    CSCERRID.WRN_EndOfPPLineExpected,
                    1696,
                    1,
                    ResNo.CSCSTR_EndOfPPLineExpected));
            ErrorInfoDic.Add(
                CSCERRID.WRN_ConflictingChecksum,
                new ERRORINFO(
                    CSCERRID.WRN_ConflictingChecksum,
                    1697,
                    1,
                    ResNo.CSCSTR_ConflictingChecksum));
            ErrorInfoDic.Add(
                CSCERRID.WRN_AssumedMatchThis,
                new ERRORINFO(
                    CSCERRID.WRN_AssumedMatchThis,
                    1698,
                    2,
                    ResNo.CSCSTR_AssumedMatchThis));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UseSwitchInsteadOfAttribute,
                new ERRORINFO(
                    CSCERRID.WRN_UseSwitchInsteadOfAttribute,
                    1699,
                    1,
                    ResNo.CSCSTR_UseSwitchInsteadOfAttribute));
            ErrorInfoDic.Add(
                CSCERRID.WRN_InvalidAssemblyName,
                new ERRORINFO(
                    CSCERRID.WRN_InvalidAssemblyName,
                    1700,
                    3,
                    ResNo.CSCSTR_InvalidAssemblyName));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnifyReferenceMajMin,
                new ERRORINFO(
                    CSCERRID.WRN_UnifyReferenceMajMin,
                    1701,
                    2,
                    ResNo.CSCSTR_UnifyReferenceMajMin));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnifyReferenceBldRev,
                new ERRORINFO(
                    CSCERRID.WRN_UnifyReferenceBldRev,
                    1702,
                    3,
                    ResNo.CSCSTR_UnifyReferenceBldRev));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateImport,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateImport,
                    1703,
                    0,
                    ResNo.CSCSTR_DuplicateImport));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateImportSimple,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateImportSimple,
                    1704,
                    0,
                    ResNo.CSCSTR_DuplicateImportSimple));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AssemblyMatchBadVersion,
                new ERRORINFO(
                    CSCERRID.ERR_AssemblyMatchBadVersion,
                    1705,
                    0,
                    ResNo.CSCSTR_AssemblyMatchBadVersion));
            ErrorInfoDic.Add(
                CSCERRID.ERR_AnonMethNotAllowed,
                new ERRORINFO(
                    CSCERRID.ERR_AnonMethNotAllowed,
                    1706,
                    0,
                    ResNo.CSCSTR_AnonMethNotAllowed));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DelegateNewMethBind,
                new ERRORINFO(
                    CSCERRID.WRN_DelegateNewMethBind,
                    1707,
                    1,
                    ResNo.CSCSTR_DelegateNewMethBind));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FixedNeedsLvalue,
                new ERRORINFO(
                    CSCERRID.ERR_FixedNeedsLvalue,
                    1708,
                    0,
                    ResNo.CSCSTR_FixedNeedsLvalue));
            ErrorInfoDic.Add(
                CSCERRID.WRN_EmptyFileName,
                new ERRORINFO(
                    CSCERRID.WRN_EmptyFileName,
                    1709,
                    1,
                    ResNo.CSCSTR_EmptyFileName));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DuplicateTypeParamTag,
                new ERRORINFO(
                    CSCERRID.WRN_DuplicateTypeParamTag,
                    1710,
                    2,
                    ResNo.CSCSTR_DuplicateTypeParamTag));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnmatchedTypeParamTag,
                new ERRORINFO(
                    CSCERRID.WRN_UnmatchedTypeParamTag,
                    1711,
                    2,
                    ResNo.CSCSTR_UnmatchedTypeParamTag));
            ErrorInfoDic.Add(
                CSCERRID.WRN_MissingTypeParamTag,
                new ERRORINFO(
                    CSCERRID.WRN_MissingTypeParamTag,
                    1712,
                    4,
                    ResNo.CSCSTR_MissingTypeParamTag));
            ErrorInfoDic.Add(
                CSCERRID.FTL_TypeNameBuilderError,
                new ERRORINFO(
                    CSCERRID.FTL_TypeNameBuilderError,
                    1713,
                    -1,
                    ResNo.CSCSTR_TypeNameBuilderError));
            ErrorInfoDic.Add(
                CSCERRID.ERR_ImportBadBase,
                new ERRORINFO(
                    CSCERRID.ERR_ImportBadBase,
                    1714,
                    0,
                    ResNo.CSCSTR_ImportBadBase));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantChangeTypeOnOverride,
                new ERRORINFO(
                    CSCERRID.ERR_CantChangeTypeOnOverride,
                    1715,
                    0,
                    ResNo.CSCSTR_CantChangeTypeOnOverride));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DoNotUseFixedBufferAttr,
                new ERRORINFO(
                    CSCERRID.ERR_DoNotUseFixedBufferAttr,
                    1716,
                    0,
                    ResNo.CSCSTR_DoNotUseFixedBufferAttr));
            ErrorInfoDic.Add(
                CSCERRID.WRN_AssignmentToSelf,
                new ERRORINFO(
                    CSCERRID.WRN_AssignmentToSelf,
                    1717,
                    3,
                    ResNo.CSCSTR_AssignmentToSelf));
            ErrorInfoDic.Add(
                CSCERRID.WRN_ComparisonToSelf,
                new ERRORINFO(
                    CSCERRID.WRN_ComparisonToSelf,
                    1718,
                    3,
                    ResNo.CSCSTR_ComparisonToSelf));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOpenWin32Res,
                new ERRORINFO(
                    CSCERRID.ERR_CantOpenWin32Res,
                    1719,
                    0,
                    ResNo.CSCSTR_CantOpenWin32Res));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DotOnDefault,
                new ERRORINFO(
                    CSCERRID.WRN_DotOnDefault,
                    1720,
                    1,
                    ResNo.CSCSTR_DotOnDefault));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoMultipleInheritance,
                new ERRORINFO(
                    CSCERRID.ERR_NoMultipleInheritance,
                    1721,
                    0,
                    ResNo.CSCSTR_NoMultipleInheritance));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BaseClassMustBeFirst,
                new ERRORINFO(
                    CSCERRID.ERR_BaseClassMustBeFirst,
                    1722,
                    0,
                    ResNo.CSCSTR_BaseClassMustBeFirst));
            ErrorInfoDic.Add(
                CSCERRID.WRN_BadXMLRefTypeVar,
                new ERRORINFO(
                    CSCERRID.WRN_BadXMLRefTypeVar,
                    1723,
                    1,
                    ResNo.CSCSTR_BadXMLRefTypeVar));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidDefaultCharSetValue,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidDefaultCharSetValue,
                    1724,
                    0,
                    ResNo.CSCSTR_InvalidDefaultCharSetValue));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FriendAssemblyBadArgs,
                new ERRORINFO(
                    CSCERRID.ERR_FriendAssemblyBadArgs,
                    1725,
                    0,
                    ResNo.CSCSTR_FriendAssemblyBadArgs));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FriendAssemblySNReq,
                new ERRORINFO(
                    CSCERRID.ERR_FriendAssemblySNReq,
                    1726,
                    0,
                    ResNo.CSCSTR_FriendAssemblySNReq));
            ErrorInfoDic.Add(
                CSCERRID.ERR_WatsonSendNotOptedIn,
                new ERRORINFO(
                    CSCERRID.ERR_WatsonSendNotOptedIn,
                    1727,
                    0,
                    ResNo.CSCSTR_WatsonSendNotOptedIn));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DelegateOnNullable,
                new ERRORINFO(
                    CSCERRID.ERR_DelegateOnNullable,
                    1728,
                    0,
                    ResNo.CSCSTR_DelegateOnNullable));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadWarningLevel,
                new ERRORINFO(
                    CSCERRID.ERR_BadWarningLevel,
                    1900,
                    0,
                    ResNo.CSCSTR_BadWarningLevel));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadDebugType,
                new ERRORINFO(
                    CSCERRID.ERR_BadDebugType,
                    1902,
                    0,
                    ResNo.CSCSTR_BadDebugType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_UnknownTestSwitch,
                new ERRORINFO(
                    CSCERRID.ERR_UnknownTestSwitch,
                    1903,
                    0,
                    ResNo.CSCSTR_UnknownTestSwitch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadResourceVis,
                new ERRORINFO(
                    CSCERRID.ERR_BadResourceVis,
                    1906,
                    0,
                    ResNo.CSCSTR_BadResourceVis));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DefaultValueTypeMustMatch,
                new ERRORINFO(
                    CSCERRID.ERR_DefaultValueTypeMustMatch,
                    1908,
                    0,
                    ResNo.CSCSTR_DefaultValueTypeMustMatch));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DefaultValueBadParamType,
                new ERRORINFO(
                    CSCERRID.ERR_DefaultValueBadParamType,
                    1909,
                    0,
                    ResNo.CSCSTR_DefaultValueBadParamType));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DefaultValueBadValueType,
                new ERRORINFO(
                    CSCERRID.ERR_DefaultValueBadValueType,
                    1910,
                    0,
                    ResNo.CSCSTR_DefaultValueBadValueType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NonVirtualCallFromClosure,
                new ERRORINFO(
                    CSCERRID.WRN_NonVirtualCallFromClosure,
                    1911,
                    1,
                    ResNo.CSCSTR_NonVirtualCallFromClosure));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InitError,
                new ERRORINFO(
                    CSCERRID.ERR_InitError,
                    2000,
                    0,
                    ResNo.CSCSTR_INITERROR));
            ErrorInfoDic.Add(
                CSCERRID.ERR_FileNotFound,
                new ERRORINFO(
                    CSCERRID.ERR_FileNotFound,
                    2001,
                    0,
                    ResNo.CSCSTR_FILENOTFOUND));
            ErrorInfoDic.Add(
                CSCERRID.WRN_FileAlreadyIncluded,
                new ERRORINFO(
                    CSCERRID.WRN_FileAlreadyIncluded,
                    2002,
                    1,
                    ResNo.CSCSTR_FILEALREADYINCLUDED));
            ErrorInfoDic.Add(
                CSCERRID.ERR_DuplicateResponseFile,
                new ERRORINFO(
                    CSCERRID.ERR_DuplicateResponseFile,
                    2003,
                    0,
                    ResNo.CSCSTR_DUPLICATERESPONSEFILE));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoFileSpec,
                new ERRORINFO(
                    CSCERRID.ERR_NoFileSpec,
                    2005,
                    0,
                    ResNo.CSCSTR_NOFILESPEC));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SwitchNeedsString,
                new ERRORINFO(
                    CSCERRID.ERR_SwitchNeedsString,
                    2006,
                    0,
                    ResNo.CSCSTR_SWITCHNEEDSSTRING));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadSwitch,
                new ERRORINFO(
                    CSCERRID.ERR_BadSwitch,
                    2007,
                    0,
                    ResNo.CSCSTR_BADSWITCH));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoSources,
                new ERRORINFO(
                    CSCERRID.ERR_NoSources,
                    2008,
                    0,
                    ResNo.CSCSTR_NOSOURCES));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OpenResponseFile,
                new ERRORINFO(
                    CSCERRID.ERR_OpenResponseFile,
                    2011,
                    0,
                    ResNo.CSCSTR_NORESPONSEFILE));
            ErrorInfoDic.Add(
                CSCERRID.ERR_CantOpenFileWrite,
                new ERRORINFO(
                    CSCERRID.ERR_CantOpenFileWrite,
                    2012,
                    0,
                    ResNo.CSCSTR_CANTOPENFILEWRITE));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadBaseNumber,
                new ERRORINFO(
                    CSCERRID.ERR_BadBaseNumber,
                    2013,
                    0,
                    ResNo.CSCSTR_BADBASENUMBER));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UseNewSwitch,
                new ERRORINFO(
                    CSCERRID.WRN_UseNewSwitch,
                    2014,
                    1,
                    ResNo.CSCSTR_USENEWSWITCH));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BinaryFile,
                new ERRORINFO(
                    CSCERRID.ERR_BinaryFile,
                    2015,
                    0,
                    ResNo.CSCSTR_BINARYFILE));
            ErrorInfoDic.Add(
                CSCERRID.FTL_BadCodepage,
                new ERRORINFO(
                    CSCERRID.FTL_BadCodepage,
                    2016,
                    -1,
                    ResNo.CSCSTR_BADCODEPAGE));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoMainOnDLL,
                new ERRORINFO(
                    CSCERRID.ERR_NoMainOnDLL,
                    2017,
                    0,
                    ResNo.CSCSTR_NOMAINONDLL));
            ErrorInfoDic.Add(
                CSCERRID.FTL_InvalidTarget,
                new ERRORINFO(
                    CSCERRID.FTL_InvalidTarget,
                    2019,
                    -1,
                    ResNo.CSCSTR_INVALIDTARGET));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadTargetForSecondInputSet,
                new ERRORINFO(
                    CSCERRID.ERR_BadTargetForSecondInputSet,
                    2020,
                    0,
                    ResNo.CSCSTR_BADSECONDTARGET));
            ErrorInfoDic.Add(
                CSCERRID.FTL_InputFileNameTooLong,
                new ERRORINFO(
                    CSCERRID.FTL_InputFileNameTooLong,
                    2021,
                    -1,
                    ResNo.CSCSTR_InputFileNameTooLong));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoSourcesInLastInputSet,
                new ERRORINFO(
                    CSCERRID.ERR_NoSourcesInLastInputSet,
                    2022,
                    0,
                    ResNo.CSCSTR_NoSourcesInLastInputSet));
            ErrorInfoDic.Add(
                CSCERRID.WRN_NoConfigNotOnCommandLine,
                new ERRORINFO(
                    CSCERRID.WRN_NoConfigNotOnCommandLine,
                    2023,
                    1,
                    ResNo.CSCSTR_NoConfigNotOnCommandLine));
            ErrorInfoDic.Add(
                CSCERRID.ERR_BadFileAlignment,
                new ERRORINFO(
                    CSCERRID.ERR_BadFileAlignment,
                    2024,
                    0,
                    ResNo.CSCSTR_BadFileAlignment));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoDebugSwitchSourceMap,
                new ERRORINFO(
                    CSCERRID.ERR_NoDebugSwitchSourceMap,
                    2026,
                    0,
                    ResNo.CSCSTR_NoDebugSwitchSourceMap));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SourceMapFileBinary,
                new ERRORINFO(
                    CSCERRID.ERR_SourceMapFileBinary,
                    2027,
                    0,
                    ResNo.CSCSTR_SourceMapFileBinary));
            ErrorInfoDic.Add(
                CSCERRID.WRN_DefineIdentifierRequired,
                new ERRORINFO(
                    CSCERRID.WRN_DefineIdentifierRequired,
                    2029,
                    1,
                    ResNo.CSCSTR_DefineIdentifierRequired));
            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidSourceMap,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidSourceMap,
                    2030,
                    0,
                    ResNo.CSCSTR_InvalidSourceMap));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoSourceMapFile,
                new ERRORINFO(
                    CSCERRID.ERR_NoSourceMapFile,
                    2031,
                    0,
                    ResNo.CSCSTR_NoSourceMapFile));
            ErrorInfoDic.Add(
                CSCERRID.ERR_IllegalOptionChar,
                new ERRORINFO(
                    CSCERRID.ERR_IllegalOptionChar,
                    2032,
                    0,
                    ResNo.CSCSTR_IllegalOptionChar));
            ErrorInfoDic.Add(
                CSCERRID.FTL_OutputFileExists,
                new ERRORINFO(
                    CSCERRID.FTL_OutputFileExists,
                    2033,
                    -1,
                    ResNo.CSCSTR_OutputFileExists));
            ErrorInfoDic.Add(
                CSCERRID.ERR_OneAliasPerRefernce,
                new ERRORINFO(
                    CSCERRID.ERR_OneAliasPerRefernce,
                    2034,
                    0,
                    ResNo.CSCSTR_OneAliasPerRefernce));
            ErrorInfoDic.Add(
                CSCERRID.ERR_SwitchNeedsNumber,
                new ERRORINFO(
                    CSCERRID.ERR_SwitchNeedsNumber,
                    2035,
                    0,
                    ResNo.CSCSTR_SwitchNeedsNumber));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_NoVarArgs,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_NoVarArgs,
                    3000,
                    1,
                    ResNo.CSCSTR_CLS_NoVarArgs));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadArgType,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadArgType,
                    3001,
                    1,
                    ResNo.CSCSTR_CLS_BadArgType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadReturnType,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadReturnType,
                    3002,
                    1,
                    ResNo.CSCSTR_CLS_BadReturnType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadFieldPropType,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadFieldPropType,
                    3003,
                    1,
                    ResNo.CSCSTR_CLS_BadFieldPropType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadUnicode,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadUnicode,
                    3004,
                    1,
                    ResNo.CSCSTR_CLS_BadUnicode));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadIdentifierCase,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadIdentifierCase,
                    3005,
                    1,
                    ResNo.CSCSTR_CLS_BadIdentifierCase));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_OverloadRefOut,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_OverloadRefOut,
                    3006,
                    1,
                    ResNo.CSCSTR_CLS_OverloadRefOut));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_OverloadUnnamed,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_OverloadUnnamed,
                    3007,
                    1,
                    ResNo.CSCSTR_CLS_OverloadUnnamed));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadIdentifier,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadIdentifier,
                    3008,
                    1,
                    ResNo.CSCSTR_CLS_BadIdentifier));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadBase,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadBase,
                    3009,
                    1,
                    ResNo.CSCSTR_CLS_BadBase));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadInterfaceMember,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadInterfaceMember,
                    3010,
                    1,
                    ResNo.CSCSTR_CLS_BadInterfacemember));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_NoAbstractMembers,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_NoAbstractMembers,
                    3011,
                    1,
                    ResNo.CSCSTR_CLS_NoAbstractMembers));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_NotOnModules,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_NotOnModules,
                    3012,
                    1,
                    ResNo.CSCSTR_CLS_NotOnModules));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_ModuleMissingCLS,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_ModuleMissingCLS,
                    3013,
                    1,
                    ResNo.CSCSTR_CLS_ModuleMissingCLS));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_AssemblyNotCLS,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_AssemblyNotCLS,
                    3014,
                    1,
                    ResNo.CSCSTR_CLS_AssemblyNotCLS));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadAttributeType,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadAttributeType,
                    3015,
                    1,
                    ResNo.CSCSTR_CLS_BadAttributeType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_ArrayArgumentToAttribute,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_ArrayArgumentToAttribute,
                    3016,
                    1,
                    ResNo.CSCSTR_CLS_ArrayArgumentToAttribute));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_NotOnModules2,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_NotOnModules2,
                    3017,
                    1,
                    ResNo.CSCSTR_CLS_NotOnModules2));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_IllegalTrueInFalse,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_IllegalTrueInFalse,
                    3018,
                    1,
                    ResNo.CSCSTR_CLS_IllegalTrueInFalse));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_MeaninglessOnPrivateType,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_MeaninglessOnPrivateType,
                    3019,
                    2,
                    ResNo.CSCSTR_CLS_MeaninglessOnPrivateType));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_AssemblyNotCLS2,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_AssemblyNotCLS2,
                    3021,
                    2,
                    ResNo.CSCSTR_CLS_AssemblyNotCLS2));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_MeaninglessOnParam,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_MeaninglessOnParam,
                    3022,
                    1,
                    ResNo.CSCSTR_CLS_MeaninglessOnParam));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_MeaninglessOnReturn,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_MeaninglessOnReturn,
                    3023,
                    1,
                    ResNo.CSCSTR_CLS_MeaninglessOnReturn));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadTypeVar,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadTypeVar,
                    3024,
                    1,
                    ResNo.CSCSTR_CLS_BadTypeVar));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_VolatileField,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_VolatileField,
                    3026,
                    1,
                    ResNo.CSCSTR_CLS_VolatileField));
            ErrorInfoDic.Add(
                CSCERRID.WRN_CLS_BadInterface,
                new ERRORINFO(
                    CSCERRID.WRN_CLS_BadInterface,
                    3027,
                    1,
                    ResNo.CSCSTR_CLS_BadInterface));
            ErrorInfoDic.Add(
                CSCERRID.WRN_UnknownOption,
                new ERRORINFO(
                    CSCERRID.WRN_UnknownOption,
                    5000,
                    1,
                    ResNo.CSCSTR_UnknownOption));
            ErrorInfoDic.Add(
                CSCERRID.ERR_NoEntryPoint,
                new ERRORINFO(
                    CSCERRID.ERR_NoEntryPoint,
                    5001,
                    0,
                    ResNo.CSCSTR_NoEntryPoint));

            // CS3

            ErrorInfoDic.Add(
                CSCERRID.ERR_VariableNotDeclared,
                new ERRORINFO(
                    CSCERRID.ERR_VariableNotDeclared,
                    841,
                    0,
                    ResNo.CSCSTR_VariableNotDeclared));

            ErrorInfoDic.Add(
                CSCERRID.ERR_CannotAssignToImplicitType,
                new ERRORINFO(
                    CSCERRID.ERR_CannotAssignToImplicitType,
                    815,
                    0,
                    ResNo.CSCSTR_CannotAssignToImplicitType));

            ErrorInfoDic.Add(
                CSCERRID.ERR_ImplicitTypeNotInitialized,
                new ERRORINFO(
                    CSCERRID.ERR_ImplicitTypeNotInitialized,
                    818,
                    0,
                    ResNo.CSCSTR_ImplicitTypeNotInitialized));

            ErrorInfoDic.Add(
                CSCERRID.ERR_ImplicitTypeMultipleDeclarators,
                new ERRORINFO(
                    CSCERRID.ERR_ImplicitTypeMultipleDeclarators,
                    819,
                    0,
                    ResNo.CSCSTR_ImplicitTypeMultipleDeclarators));

            ErrorInfoDic.Add(
                CSCERRID.ERR_PropertyAccessorHasNoBody,
                new ERRORINFO(
                    CSCERRID.ERR_PropertyAccessorHasNoBody,
                    840,
                    0,
                    ResNo.CSCSTR_PropertyAccessorHasNoBody));

            ErrorInfoDic.Add(
                CSCERRID.ERR_NoBestTypeForArray,
                new ERRORINFO(
                    CSCERRID.ERR_NoBestTypeForArray,
                    826,
                    0,
                    ResNo.CSCSTR_NoBestTypeForArray));

            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidAnonTypeMemberDeclarator,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidAnonTypeMemberDeclarator,
                    746,
                    0,
                    ResNo.CSCSTR_InvalidAnonTypeMemberDeclarator));

            ErrorInfoDic.Add(
                CSCERRID.ERR_InvalidInitializerDeclarator,
                new ERRORINFO(
                    CSCERRID.ERR_InvalidInitializerDeclarator,
                    747,
                    0,
                    ResNo.CSCSTR_InvalidInitializerDeclarator));

            ErrorInfoDic.Add(
                CSCERRID.ERR_CollectInitRequiresIEnumerable,
                new ERRORINFO(
                    CSCERRID.ERR_CollectInitRequiresIEnumerable,
                    1922,
                    0,
                    ResNo.CSCSTR_CollectInitRequiresIEnumerable));

            ErrorInfoDic.Add(
                CSCERRID.ERR_ThisModifierNotOnFirstParam,
                new ERRORINFO(
                    CSCERRID.ERR_ThisModifierNotOnFirstParam,
                    1100,
                    0,
                    ResNo.CSCSTR_ThisModifierNotOnFirstParam));

            ErrorInfoDic.Add(
                CSCERRID.ERR_NonStaticExtensionMethod,
                new ERRORINFO(
                    CSCERRID.ERR_NonStaticExtensionMethod,
                    1105,
                    0,
                    ResNo.CSCSTR_NonStaticExtensionMethod));

            ErrorInfoDic.Add(
                CSCERRID.ERR_ExtensionMethodInImproperClass,
                new ERRORINFO(
                    CSCERRID.ERR_ExtensionMethodInImproperClass,
                    1106,
                    0,
                    ResNo.CSCSTR_ExtensionMethodInImproperClass));

            ErrorInfoDic.Add(
                CSCERRID.ERR_ExtensionMethodInNestedClass,
                new ERRORINFO(
                    CSCERRID.ERR_ExtensionMethodInNestedClass,
                    1109,
                    0,
                    ResNo.CSCSTR_ExtensionMethodInNestedClass));

            ErrorInfoDic.Add(
                CSCERRID.ERR_InconsistentLambdaParameters,
                new ERRORINFO(
                    CSCERRID.ERR_InconsistentLambdaParameters,
                    748,
                    0,
                    ResNo.CSCSTR_InconsistentLambdaParameters));

            ErrorInfoDic.Add(
                CSCERRID.ERR_QueryPatternNotImplemented,
                new ERRORINFO(
                    CSCERRID.ERR_QueryPatternNotImplemented,
                    1936,
                    0,
                    ResNo.CSCSTR_QueryPatternNotImplemented));

            ErrorInfoDic.Add(
                CSCERRID.ERR_ExpectContextualKeyword,
                new ERRORINFO(
                    CSCERRID.ERR_ExpectContextualKeyword,
                    743,
                    0,
                    ResNo.CSCSTR_ExpectContextualKeyword));

            ErrorInfoDic.Add(
                CSCERRID.ERR_QueryTypeInferenceFailed,
                new ERRORINFO(
                    CSCERRID.ERR_QueryTypeInferenceFailed,
                    1941,
                    0,
                    ResNo.CSCSTR_QueryTypeInferenceFailed));

            ErrorInfoDic.Add(
                CSCERRID.ERR_QueryBodyHasNoSelectOrGroup,
                new ERRORINFO(
                    CSCERRID.ERR_QueryBodyHasNoSelectOrGroup,
                    742,
                    0,
                    ResNo.CSCSTR_QueryBodyHasNoSelectOrGroup));

            ErrorInfoDic.Add(
                CSCERRID.ERR_ExprTreeContainsAssignment,
                new ERRORINFO(
                    CSCERRID.ERR_ExprTreeContainsAssignment,
                    832,
                    0,
                    ResNo.CSCSTR_ExprTreeContainsAssignment));

            ErrorInfoDic.Add(
                CSCERRID.ERR_CannotConvertToExprTree,
                new ERRORINFO(
                    CSCERRID.ERR_CannotConvertToExprTree,
                    834,
                    0,
                    ResNo.CSCSTR_CannotConvertToExprTree));

            ErrorInfoDic.Add(
                CSCERRID.ERR_MultiplePartialMethodImplementation,
                new ERRORINFO(
                    CSCERRID.ERR_MultiplePartialMethodImplementation,
                    757,
                    0,
                    ResNo.CSCSTR_MultiplePartialMethodImplementation));

            ErrorInfoDic.Add(
                CSCERRID.ERR_BadModifierForPartialMethod,
                new ERRORINFO(
                    CSCERRID.ERR_BadModifierForPartialMethod,
                    750,
                    0,
                    ResNo.CSCSTR_BadModifierForPartialMethod));

            ErrorInfoDic.Add(
                CSCERRID.ERR_PartialMethodHasOutParameter,
                new ERRORINFO(
                    CSCERRID.ERR_PartialMethodHasOutParameter,
                    752,
                    0,
                    ResNo.CSCSTR_PartialMethodHasOutParameter));

            // CS4

            ErrorInfoDic.Add(
                CSCERRID.ERR_DynamicOnTypeof,
                new ERRORINFO(
                    CSCERRID.ERR_DynamicOnTypeof,
                    1962,
                    0,
                    ResNo.CSCSTR_DynamicOnTypeof));
        }
    }
}
