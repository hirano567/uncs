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
// File: assembly.h
//
// This is the real work horse
// ===========================================================================

//============================================================================
// ALErrorInfo.cs
//   (separated from CSharp\ALink\Dll\Assembly.cs)
//
// 2015/10/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Uncs
{
    //======================================================================
    // class ALErrorInfo
    //
    /// <summary></summary>
    //======================================================================
    internal static class ALErrorInfo
    {
        internal static ALErrorInfoManager Manager = new ALErrorInfoManager();
    }
    
    //======================================================================
    // class ALErrorInfoManager
    //
    /// <summary></summary>
    //======================================================================
    internal class ALErrorInfoManager : BCErrorInfoManager<ALERRID>
    {
        //------------------------------------------------------------
        // ALErrorInfoManager Constructor
        //------------------------------------------------------------
        internal ALErrorInfoManager()
            : base()
        {
            this.Prefix = "AL";
            this.InvalidErrorID = ALERRID.Invalid;
        }

        //------------------------------------------------------------
        // ALErrorInfoManager.InitErrorInfoDic
        //------------------------------------------------------------
        internal override void InitErrorInfoDic()
        {
            ErrorInfoDic.Add(
                ALERRID.FTL_InternalError,
                new ERRORINFO(
                    ALERRID.FTL_InternalError,
                    1001,
                    -1,
                    ResNo.ALSTR_InternalError));
            ErrorInfoDic.Add(
                ALERRID.FTL_NoMemory,
                new ERRORINFO(
                    ALERRID.FTL_NoMemory,
                    1002,
                    -1,
                    ResNo.ALSTR_NoMemory));
            ErrorInfoDic.Add(
                ALERRID.ERR_MissingOptionArg,
                new ERRORINFO(
                    ALERRID.ERR_MissingOptionArg,
                    1003,
                    0,
                    ResNo.ALSTR_MissingOptionArg));
            ErrorInfoDic.Add(
                ALERRID.FTL_ComPlusInit,
                new ERRORINFO(
                    ALERRID.FTL_ComPlusInit,
                    1004,
                    -1,
                    ResNo.ALSTR_ComPlusInit));
            ErrorInfoDic.Add(
                ALERRID.FTL_FileTooBig,
                new ERRORINFO(
                    ALERRID.FTL_FileTooBig,
                    1005,
                    -1,
                    ResNo.ALSTR_FileTooBig));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateResponseFile,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateResponseFile,
                    1006,
                    0,
                    ResNo.ALSTR_DuplicateResponseFile));
            ErrorInfoDic.Add(
                ALERRID.ERR_OpenResponseFile,
                new ERRORINFO(
                    ALERRID.ERR_OpenResponseFile,
                    1007,
                    0,
                    ResNo.ALSTR_OpenResponseFile));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoFileSpec,
                new ERRORINFO(
                    ALERRID.ERR_NoFileSpec,
                    1008,
                    0,
                    ResNo.ALSTR_NoFileSpec));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantOpenFileWrite,
                new ERRORINFO(
                    ALERRID.ERR_CantOpenFileWrite,
                    1009,
                    0,
                    ResNo.ALSTR_CantOpenFileWrite));
            ErrorInfoDic.Add(
                ALERRID.ERR_SwitchNeedsString,
                new ERRORINFO(
                    ALERRID.ERR_SwitchNeedsString,
                    1010,
                    0,
                    ResNo.ALSTR_SwitchNeedsString));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantOpenBinaryAsText,
                new ERRORINFO(
                    ALERRID.ERR_CantOpenBinaryAsText,
                    1011,
                    0,
                    ResNo.ALSTR_CantOpenBinaryAsText));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadOptionValue,
                new ERRORINFO(
                    ALERRID.ERR_BadOptionValue,
                    1012,
                    0,
                    ResNo.ALSTR_BadOptionValue));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadSwitch,
                new ERRORINFO(
                    ALERRID.ERR_BadSwitch,
                    1013,
                    0,
                    ResNo.ALSTR_BadSwitch));
            ErrorInfoDic.Add(
                ALERRID.FTL_InitError,
                new ERRORINFO(
                    ALERRID.FTL_InitError,
                    1014,
                    -1,
                    ResNo.ALSTR_InitError));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoInputs,
                new ERRORINFO(
                    ALERRID.ERR_NoInputs,
                    1016,
                    0,
                    ResNo.ALSTR_NoInputs));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoOutput,
                new ERRORINFO(
                    ALERRID.ERR_NoOutput,
                    1017,
                    0,
                    ResNo.ALSTR_NoOutput));
            ErrorInfoDic.Add(
                ALERRID.FTL_RequiredFileNotFound,
                new ERRORINFO(
                    ALERRID.FTL_RequiredFileNotFound,
                    1018,
                    -1,
                    ResNo.ALSTR_RequiredFileNotFound));
            ErrorInfoDic.Add(
                ALERRID.ERR_MetaDataError,
                new ERRORINFO(
                    ALERRID.ERR_MetaDataError,
                    1019,
                    0,
                    ResNo.ALSTR_MetaDataError));
            ErrorInfoDic.Add(
                ALERRID.WRN_IgnoringAssembly,
                new ERRORINFO(
                    ALERRID.WRN_IgnoringAssembly,
                    1020,
                    1,
                    ResNo.ALSTR_IgnoringAssembly));
            ErrorInfoDic.Add(
                ALERRID.WRN_OptionConflicts,
                new ERRORINFO(
                    ALERRID.WRN_OptionConflicts,
                    1021,
                    1,
                    ResNo.ALSTR_OptionConflicts));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantReadResource,
                new ERRORINFO(
                    ALERRID.ERR_CantReadResource,
                    1022,
                    0,
                    ResNo.ALSTR_CantReadResource));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantEmbedResource,
                new ERRORINFO(
                    ALERRID.ERR_CantEmbedResource,
                    1023,
                    0,
                    ResNo.ALSTR_CantEmbedResource));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidFileDefInComType,
                new ERRORINFO(
                    ALERRID.ERR_InvalidFileDefInComType,
                    1025,
                    0,
                    ResNo.ALSTR_InvalidFileDefInComType));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidVersionFormat,
                new ERRORINFO(
                    ALERRID.ERR_InvalidVersionFormat,
                    1026,
                    0,
                    ResNo.ALSTR_InvalidVersionString));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidOSString,
                new ERRORINFO(
                    ALERRID.ERR_InvalidOSString,
                    1027,
                    0,
                    ResNo.ALSTR_InvalidOSString));
            ErrorInfoDic.Add(
                ALERRID.ERR_NeedPrivateKey,
                new ERRORINFO(
                    ALERRID.ERR_NeedPrivateKey,
                    1028,
                    0,
                    ResNo.ALSTR_NeedPrivateKey));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoNoKeyContainer,
                new ERRORINFO(
                    ALERRID.ERR_CryptoNoKeyContainer,
                    1029,
                    0,
                    ResNo.ALSTR_CryptoNoKeyContainer));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoFailed,
                new ERRORINFO(
                    ALERRID.ERR_CryptoFailed,
                    1030,
                    0,
                    ResNo.ALSTR_CryptoFailed));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantReadIcon,
                new ERRORINFO(
                    ALERRID.ERR_CantReadIcon,
                    1031,
                    0,
                    ResNo.ALSTR_CantReadIcon));
            ErrorInfoDic.Add(
                ALERRID.ERR_AutoResGen,
                new ERRORINFO(
                    ALERRID.ERR_AutoResGen,
                    1032,
                    0,
                    ResNo.ALSTR_AutoResGen));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateCA,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateCA,
                    1033,
                    0,
                    ResNo.ALSTR_DuplicateCA));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantRenameAssembly,
                new ERRORINFO(
                    ALERRID.ERR_CantRenameAssembly,
                    1034,
                    0,
                    ResNo.ALSTR_CantRenameAssembly));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoMainOnDlls,
                new ERRORINFO(
                    ALERRID.ERR_NoMainOnDlls,
                    1035,
                    0,
                    ResNo.ALSTR_NoMainOnDLLs));
            ErrorInfoDic.Add(
                ALERRID.ERR_AppNeedsMain,
                new ERRORINFO(
                    ALERRID.ERR_AppNeedsMain,
                    1036,
                    0,
                    ResNo.ALSTR_AppNeedsMain));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoMainFound,
                new ERRORINFO(
                    ALERRID.ERR_NoMainFound,
                    1037,
                    0,
                    ResNo.ALSTR_NoMainFound));
            ErrorInfoDic.Add(
                ALERRID.FTL_FusionInit,
                new ERRORINFO(
                    ALERRID.FTL_FusionInit,
                    1039,
                    -1,
                    ResNo.ALSTR_FusionInit));
            ErrorInfoDic.Add(
                ALERRID.ERR_FusionInstallFailed,
                new ERRORINFO(
                    ALERRID.ERR_FusionInstallFailed,
                    1040,
                    0,
                    ResNo.ALSTR_FusionInstallFailed));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadMainFound,
                new ERRORINFO(
                    ALERRID.ERR_BadMainFound,
                    1041,
                    0,
                    ResNo.ALSTR_BadMainFound));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantAddExes,
                new ERRORINFO(
                    ALERRID.ERR_CantAddExes,
                    1042,
                    0,
                    ResNo.ALSTR_CantAddExes));
            ErrorInfoDic.Add(
                ALERRID.ERR_SameOutAndSource,
                new ERRORINFO(
                    ALERRID.ERR_SameOutAndSource,
                    1043,
                    0,
                    ResNo.ALSTR_SameOutAndSource));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoFileFailed,
                new ERRORINFO(
                    ALERRID.ERR_CryptoFileFailed,
                    1044,
                    0,
                    ResNo.ALSTR_CryptoFileFailed));
            ErrorInfoDic.Add(
                ALERRID.ERR_FileNameTooLong,
                new ERRORINFO(
                    ALERRID.ERR_FileNameTooLong,
                    1045,
                    0,
                    ResNo.ALSTR_FileNameTooLong));
            ErrorInfoDic.Add(
                ALERRID.ERR_DupResourceIdent,
                new ERRORINFO(
                    ALERRID.ERR_DupResourceIdent,
                    1046,
                    0,
                    ResNo.ALSTR_DupResourceIdent));
            ErrorInfoDic.Add(
                ALERRID.ERR_ModuleImportError,
                new ERRORINFO(
                    ALERRID.ERR_ModuleImportError,
                    1047,
                    0,
                    ResNo.ALSTR_ModuleImportError));
            ErrorInfoDic.Add(
                ALERRID.ERR_AssemblyModuleImportError,
                new ERRORINFO(
                    ALERRID.ERR_AssemblyModuleImportError,
                    1048,
                    0,
                    ResNo.ALSTR_AssemblyModuleImportError));
            ErrorInfoDic.Add(
                ALERRID.WRN_InvalidTime,
                new ERRORINFO(
                    ALERRID.WRN_InvalidTime,
                    1049,
                    1,
                    ResNo.ALSTR_InvalidTime));
            ErrorInfoDic.Add(
                ALERRID.WRN_FeatureDeprecated,
                new ERRORINFO(
                    ALERRID.WRN_FeatureDeprecated,
                    1050,
                    1,
                    ResNo.ALSTR_FeatureDeprecated));
            ErrorInfoDic.Add(
                ALERRID.ERR_EmitCAFailed,
                new ERRORINFO(
                    ALERRID.ERR_EmitCAFailed,
                    1051,
                    0,
                    ResNo.ALSTR_EmitCAFailed));
            ErrorInfoDic.Add(
                ALERRID.ERR_ParentNotAnAssembly,
                new ERRORINFO(
                    ALERRID.ERR_ParentNotAnAssembly,
                    1052,
                    0,
                    ResNo.ALSTR_ParentNotAnAssembly));
            ErrorInfoDic.Add(
                ALERRID.WRN_InvalidVersionString,
                new ERRORINFO(
                    ALERRID.WRN_InvalidVersionString,
                    1053,
                    1,
                    ResNo.ALSTR_InvalidVersionFormat));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidVersionString,
                new ERRORINFO(
                    ALERRID.ERR_InvalidVersionString,
                    1054,
                    0,
                    ResNo.ALSTR_InvalidVersionFormat));
            ErrorInfoDic.Add(
                ALERRID.ERR_RefNotStrong,
                new ERRORINFO(
                    ALERRID.ERR_RefNotStrong,
                    1055,
                    0,
                    ResNo.ALSTR_RefNotStrong));
            ErrorInfoDic.Add(
                ALERRID.WRN_RefHasCulture,
                new ERRORINFO(
                    ALERRID.WRN_RefHasCulture,
                    1056,
                    1,
                    ResNo.ALSTR_RefHasCulture));
            ErrorInfoDic.Add(
                ALERRID.ERR_ExeHasCulture,
                new ERRORINFO(
                    ALERRID.ERR_ExeHasCulture,
                    1057,
                    0,
                    ResNo.ALSTR_ExeHasCulture));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantAddAssembly,
                new ERRORINFO(
                    ALERRID.ERR_CantAddAssembly,
                    1058,
                    0,
                    ResNo.ALSTR_CantAddAssembly));
            ErrorInfoDic.Add(
                ALERRID.ERR_UnknownError,
                new ERRORINFO(
                    ALERRID.ERR_UnknownError,
                    1059,
                    0,
                    ResNo.ALSTR_UnknownError));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoHashFailed,
                new ERRORINFO(
                    ALERRID.ERR_CryptoHashFailed,
                    1060,
                    0,
                    ResNo.ALSTR_CryptoHashFailed));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadOptionValueHR,
                new ERRORINFO(
                    ALERRID.ERR_BadOptionValueHR,
                    1061,
                    0,
                    ResNo.ALSTR_BadOptionValueHR));
            ErrorInfoDic.Add(
                ALERRID.WRN_IgnoringDuplicateSource,
                new ERRORINFO(
                    ALERRID.WRN_IgnoringDuplicateSource,
                    1062,
                    1,
                    ResNo.ALSTR_IgnoringDuplicateSource));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateExportedType,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateExportedType,
                    1063,
                    0,
                    ResNo.ALSTR_DuplicateExportedType));
            ErrorInfoDic.Add(
                ALERRID.FTL_InputFileNameTooLong,
                new ERRORINFO(
                    ALERRID.FTL_InputFileNameTooLong,
                    1065,
                    0,
                    ResNo.ALSTR_InputFileNameTooLong));
            ErrorInfoDic.Add(
                ALERRID.ERR_IllegalOptionChar,
                new ERRORINFO(
                    ALERRID.ERR_IllegalOptionChar,
                    1066,
                    0,
                    ResNo.ALSTR_IllegalOptionChar));
            ErrorInfoDic.Add(
                ALERRID.ERR_BinaryFile,
                new ERRORINFO(
                    ALERRID.ERR_BinaryFile,
                    1067,
                    0,
                    ResNo.ALSTR_BinaryFile));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateModule,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateModule,
                    1068,
                    0,
                    ResNo.ALSTR_DuplicateModule));
            ErrorInfoDic.Add(
                ALERRID.FTL_OutputFileExists,
                new ERRORINFO(
                    ALERRID.FTL_OutputFileExists,
                    1069,
                    -1,
                    ResNo.ALSTR_OutputFileExists));
            ErrorInfoDic.Add(
                ALERRID.ERR_AgnosticToMachine,
                new ERRORINFO(
                    ALERRID.ERR_AgnosticToMachine,
                    1070,
                    0,
                    ResNo.ALSTR_AgnosticToMachineModule));
            ErrorInfoDic.Add(
                ALERRID.ERR_ConflictingMachine,
                new ERRORINFO(
                    ALERRID.ERR_ConflictingMachine,
                    1072,
                    0,
                    ResNo.ALSTR_ConflictingMachineModule));
            ErrorInfoDic.Add(
                ALERRID.WRN_ConflictingMachine,
                new ERRORINFO(
                    ALERRID.WRN_ConflictingMachine,
                    1073,
                    1,
                    ResNo.ALSTR_ConflictingMachineAssembly));
            ErrorInfoDic.Add(
                ALERRID.ERR_ModuleNameDifferent,
                new ERRORINFO(
                    ALERRID.ERR_ModuleNameDifferent,
                    1074,
                    0,
                    ResNo.ALSTR_ModuleNameDifferent));
            ErrorInfoDic.Add(
                ALERRID.WRN_DelaySignWithNoKey,
                new ERRORINFO(
                    ALERRID.WRN_DelaySignWithNoKey,
                    1075,
                    1,
                    ResNo.ALSTR_DelaySignWithNoKey));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateTypeForwarders,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateTypeForwarders,
                    1076,
                    0,
                    ResNo.ALSTR_DuplicateTypeForwarders));
            ErrorInfoDic.Add(
                ALERRID.ERR_TypeFwderMatchesDeclared,
                new ERRORINFO(
                    ALERRID.ERR_TypeFwderMatchesDeclared,
                    1077,
                    0,
                    ResNo.ALSTR_TypeFwderMatchesDeclared));

#if false
            ErrorInfoDic.Add(
                ALERRID.FTL_InternalError,
                new ERRORINFO(
                    ALERRID.FTL_InternalError,
                    1001,
                    -1,
                    "ALSTR_InternalError"));
            ErrorInfoDic.Add(
                ALERRID.FTL_NoMemory,
                new ERRORINFO(
                    ALERRID.FTL_NoMemory,
                    1002,
                    -1,
                    "ALSTR_NoMemory"));
            ErrorInfoDic.Add(
                ALERRID.ERR_MissingOptionArg,
                new ERRORINFO(
                    ALERRID.ERR_MissingOptionArg,
                    1003,
                    0,
                    "ALSTR_MissingOptionArg"));
            ErrorInfoDic.Add(
                ALERRID.FTL_ComPlusInit,
                new ERRORINFO(
                    ALERRID.FTL_ComPlusInit,
                    1004,
                    -1,
                    "ALSTR_ComPlusInit"));
            ErrorInfoDic.Add(
                ALERRID.FTL_FileTooBig,
                new ERRORINFO(
                    ALERRID.FTL_FileTooBig,
                    1005,
                    -1,
                    "ALSTR_FileTooBig"));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateResponseFile,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateResponseFile,
                    1006,
                    0,
                    "ALSTR_DuplicateResponseFile"));
            ErrorInfoDic.Add(
                ALERRID.ERR_OpenResponseFile,
                new ERRORINFO(
                    ALERRID.ERR_OpenResponseFile,
                    1007,
                    0,
                    "ALSTR_OpenResponseFile"));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoFileSpec,
                new ERRORINFO(
                    ALERRID.ERR_NoFileSpec,
                    1008,
                    0,
                    "ALSTR_NoFileSpec"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantOpenFileWrite,
                new ERRORINFO(
                    ALERRID.ERR_CantOpenFileWrite,
                    1009,
                    0,
                    "ALSTR_CantOpenFileWrite"));
            ErrorInfoDic.Add(
                ALERRID.ERR_SwitchNeedsString,
                new ERRORINFO(
                    ALERRID.ERR_SwitchNeedsString,
                    1010,
                    0,
                    "ALSTR_SwitchNeedsString"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantOpenBinaryAsText,
                new ERRORINFO(
                    ALERRID.ERR_CantOpenBinaryAsText,
                    1011,
                    0,
                    "ALSTR_CantOpenBinaryAsText"));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadOptionValue,
                new ERRORINFO(
                    ALERRID.ERR_BadOptionValue,
                    1012,
                    0,
                    "ALSTR_BadOptionValue"));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadSwitch,
                new ERRORINFO(
                    ALERRID.ERR_BadSwitch,
                    1013,
                    0,
                    "ALSTR_BadSwitch"));
            ErrorInfoDic.Add(
                ALERRID.FTL_InitError,
                new ERRORINFO(
                    ALERRID.FTL_InitError,
                    1014,
                    -1,
                    "ALSTR_InitError"));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoInputs,
                new ERRORINFO(
                    ALERRID.ERR_NoInputs,
                    1016,
                    0,
                    "ALSTR_NoInputs"));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoOutput,
                new ERRORINFO(
                    ALERRID.ERR_NoOutput,
                    1017,
                    0,
                    "ALSTR_NoOutput"));
            ErrorInfoDic.Add(
                ALERRID.FTL_RequiredFileNotFound,
                new ERRORINFO(
                    ALERRID.FTL_RequiredFileNotFound,
                    1018,
                    -1,
                    "ALSTR_RequiredFileNotFound"));
            ErrorInfoDic.Add(
                ALERRID.ERR_MetaDataError,
                new ERRORINFO(
                    ALERRID.ERR_MetaDataError,
                    1019,
                    0,
                    "ALSTR_MetaDataError"));
            ErrorInfoDic.Add(
                ALERRID.WRN_IgnoringAssembly,
                new ERRORINFO(
                    ALERRID.WRN_IgnoringAssembly,
                    1020,
                    1,
                    "ALSTR_IgnoringAssembly"));
            ErrorInfoDic.Add(
                ALERRID.WRN_OptionConflicts,
                new ERRORINFO(
                    ALERRID.WRN_OptionConflicts,
                    1021,
                    1,
                    "ALSTR_OptionConflicts"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantReadResource,
                new ERRORINFO(
                    ALERRID.ERR_CantReadResource,
                    1022,
                    0,
                    "ALSTR_CantReadResource"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantEmbedResource,
                new ERRORINFO(
                    ALERRID.ERR_CantEmbedResource,
                    1023,
                    0,
                    "ALSTR_CantEmbedResource"));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidFileDefInComType,
                new ERRORINFO(
                    ALERRID.ERR_InvalidFileDefInComType,
                    1025,
                    0,
                    "ALSTR_InvalidFileDefInComType"));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidVersionFormat,
                new ERRORINFO(
                    ALERRID.ERR_InvalidVersionFormat,
                    1026,
                    0,
                    "ALSTR_InvalidVersionString"));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidOSString,
                new ERRORINFO(
                    ALERRID.ERR_InvalidOSString,
                    1027,
                    0,
                    "ALSTR_InvalidOSString"));
            ErrorInfoDic.Add(
                ALERRID.ERR_NeedPrivateKey,
                new ERRORINFO(
                    ALERRID.ERR_NeedPrivateKey,
                    1028,
                    0,
                    "ALSTR_NeedPrivateKey"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoNoKeyContainer,
                new ERRORINFO(
                    ALERRID.ERR_CryptoNoKeyContainer,
                    1029,
                    0,
                    "ALSTR_CryptoNoKeyContainer"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoFailed,
                new ERRORINFO(
                    ALERRID.ERR_CryptoFailed,
                    1030,
                    0,
                    "ALSTR_CryptoFailed"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantReadIcon,
                new ERRORINFO(
                    ALERRID.ERR_CantReadIcon,
                    1031,
                    0,
                    "ALSTR_CantReadIcon"));
            ErrorInfoDic.Add(
                ALERRID.ERR_AutoResGen,
                new ERRORINFO(
                    ALERRID.ERR_AutoResGen,
                    1032,
                    0,
                    "ALSTR_AutoResGen"));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateCA,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateCA,
                    1033,
                    0,
                    "ALSTR_DuplicateCA"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantRenameAssembly,
                new ERRORINFO(
                    ALERRID.ERR_CantRenameAssembly,
                    1034,
                    0,
                    "ALSTR_CantRenameAssembly"));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoMainOnDlls,
                new ERRORINFO(
                    ALERRID.ERR_NoMainOnDlls,
                    1035,
                    0,
                    "ALSTR_NoMainOnDLLs"));
            ErrorInfoDic.Add(
                ALERRID.ERR_AppNeedsMain,
                new ERRORINFO(
                    ALERRID.ERR_AppNeedsMain,
                    1036,
                    0,
                    "ALSTR_AppNeedsMain"));
            ErrorInfoDic.Add(
                ALERRID.ERR_NoMainFound,
                new ERRORINFO(
                    ALERRID.ERR_NoMainFound,
                    1037,
                    0,
                    "ALSTR_NoMainFound"));
            ErrorInfoDic.Add(
                ALERRID.FTL_FusionInit,
                new ERRORINFO(
                    ALERRID.FTL_FusionInit,
                    1039,
                    -1,
                    "ALSTR_FusionInit"));
            ErrorInfoDic.Add(
                ALERRID.ERR_FusionInstallFailed,
                new ERRORINFO(
                    ALERRID.ERR_FusionInstallFailed,
                    1040,
                    0,
                    "ALSTR_FusionInstallFailed"));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadMainFound,
                new ERRORINFO(
                    ALERRID.ERR_BadMainFound,
                    1041,
                    0,
                    "ALSTR_BadMainFound"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantAddExes,
                new ERRORINFO(
                    ALERRID.ERR_CantAddExes,
                    1042,
                    0,
                    "ALSTR_CantAddExes"));
            ErrorInfoDic.Add(
                ALERRID.ERR_SameOutAndSource,
                new ERRORINFO(
                    ALERRID.ERR_SameOutAndSource,
                    1043,
                    0,
                    "ALSTR_SameOutAndSource"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoFileFailed,
                new ERRORINFO(
                    ALERRID.ERR_CryptoFileFailed,
                    1044,
                    0,
                    "ALSTR_CryptoFileFailed"));
            ErrorInfoDic.Add(
                ALERRID.ERR_FileNameTooLong,
                new ERRORINFO(
                    ALERRID.ERR_FileNameTooLong,
                    1045,
                    0,
                    "ALSTR_FileNameTooLong"));
            ErrorInfoDic.Add(
                ALERRID.ERR_DupResourceIdent,
                new ERRORINFO(
                    ALERRID.ERR_DupResourceIdent,
                    1046,
                    0,
                    "ALSTR_DupResourceIdent"));
            ErrorInfoDic.Add(
                ALERRID.ERR_ModuleImportError,
                new ERRORINFO(
                    ALERRID.ERR_ModuleImportError,
                    1047,
                    0,
                    "ALSTR_ModuleImportError"));
            ErrorInfoDic.Add(
                ALERRID.ERR_AssemblyModuleImportError,
                new ERRORINFO(
                    ALERRID.ERR_AssemblyModuleImportError,
                    1048,
                    0,
                    "ALSTR_AssemblyModuleImportError"));
            ErrorInfoDic.Add(
                ALERRID.WRN_InvalidTime,
                new ERRORINFO(
                    ALERRID.WRN_InvalidTime,
                    1049,
                    1,
                    "ALSTR_InvalidTime"));
            ErrorInfoDic.Add(
                ALERRID.WRN_FeatureDeprecated,
                new ERRORINFO(
                    ALERRID.WRN_FeatureDeprecated,
                    1050,
                    1,
                    "ALSTR_FeatureDeprecated"));
            ErrorInfoDic.Add(
                ALERRID.ERR_EmitCAFailed,
                new ERRORINFO(
                    ALERRID.ERR_EmitCAFailed,
                    1051,
                    0,
                    "ALSTR_EmitCAFailed"));
            ErrorInfoDic.Add(
                ALERRID.ERR_ParentNotAnAssembly,
                new ERRORINFO(
                    ALERRID.ERR_ParentNotAnAssembly,
                    1052,
                    0,
                    "ALSTR_ParentNotAnAssembly"));
            ErrorInfoDic.Add(
                ALERRID.WRN_InvalidVersionString,
                new ERRORINFO(
                    ALERRID.WRN_InvalidVersionString,
                    1053,
                    1,
                    "ALSTR_InvalidVersionFormat"));
            ErrorInfoDic.Add(
                ALERRID.ERR_InvalidVersionString,
                new ERRORINFO(
                    ALERRID.ERR_InvalidVersionString,
                    1054,
                    0,
                    "ALSTR_InvalidVersionFormat"));
            ErrorInfoDic.Add(
                ALERRID.ERR_RefNotStrong,
                new ERRORINFO(
                    ALERRID.ERR_RefNotStrong,
                    1055,
                    0,
                    "ALSTR_RefNotStrong"));
            ErrorInfoDic.Add(
                ALERRID.WRN_RefHasCulture,
                new ERRORINFO(
                    ALERRID.WRN_RefHasCulture,
                    1056,
                    1,
                    "ALSTR_RefHasCulture"));
            ErrorInfoDic.Add(
                ALERRID.ERR_ExeHasCulture,
                new ERRORINFO(
                    ALERRID.ERR_ExeHasCulture,
                    1057,
                    0,
                    "ALSTR_ExeHasCulture"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CantAddAssembly,
                new ERRORINFO(
                    ALERRID.ERR_CantAddAssembly,
                    1058,
                    0,
                    "ALSTR_CantAddAssembly"));
            ErrorInfoDic.Add(
                ALERRID.ERR_UnknownError,
                new ERRORINFO(
                    ALERRID.ERR_UnknownError,
                    1059,
                    0,
                    "ALSTR_UnknownError"));
            ErrorInfoDic.Add(
                ALERRID.ERR_CryptoHashFailed,
                new ERRORINFO(
                    ALERRID.ERR_CryptoHashFailed,
                    1060,
                    0,
                    "ALSTR_CryptoHashFailed"));
            ErrorInfoDic.Add(
                ALERRID.ERR_BadOptionValueHR,
                new ERRORINFO(
                    ALERRID.ERR_BadOptionValueHR,
                    1061,
                    0,
                    "ALSTR_BadOptionValueHR"));
            ErrorInfoDic.Add(
                ALERRID.WRN_IgnoringDuplicateSource,
                new ERRORINFO(
                    ALERRID.WRN_IgnoringDuplicateSource,
                    1062,
                    1,
                    "ALSTR_IgnoringDuplicateSource"));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateExportedType,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateExportedType,
                    1063,
                    0,
                    "ALSTR_DuplicateExportedType"));
            ErrorInfoDic.Add(
                ALERRID.FTL_InputFileNameTooLong,
                new ERRORINFO(
                    ALERRID.FTL_InputFileNameTooLong,
                    1065,
                    0,
                    "ALSTR_InputFileNameTooLong"));
            ErrorInfoDic.Add(
                ALERRID.ERR_IllegalOptionChar,
                new ERRORINFO(
                    ALERRID.ERR_IllegalOptionChar,
                    1066,
                    0,
                    "ALSTR_IllegalOptionChar"));
            ErrorInfoDic.Add(
                ALERRID.ERR_BinaryFile,
                new ERRORINFO(
                    ALERRID.ERR_BinaryFile,
                    1067,
                    0,
                    "ALSTR_BinaryFile"));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateModule,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateModule,
                    1068,
                    0,
                    "ALSTR_DuplicateModule"));
            ErrorInfoDic.Add(
                ALERRID.FTL_OutputFileExists,
                new ERRORINFO(
                    ALERRID.FTL_OutputFileExists,
                    1069,
                    -1,
                    "ALSTR_OutputFileExists"));
            ErrorInfoDic.Add(
                ALERRID.ERR_AgnosticToMachine,
                new ERRORINFO(
                    ALERRID.ERR_AgnosticToMachine,
                    1070,
                    0,
                    "ALSTR_AgnosticToMachineModule"));
            ErrorInfoDic.Add(
                ALERRID.ERR_ConflictingMachine,
                new ERRORINFO(
                    ALERRID.ERR_ConflictingMachine,
                    1072,
                    0,
                    "ALSTR_ConflictingMachineModule"));
            ErrorInfoDic.Add(
                ALERRID.WRN_ConflictingMachine,
                new ERRORINFO(
                    ALERRID.WRN_ConflictingMachine,
                    1073,
                    1,
                    "ALSTR_ConflictingMachineAssembly"));
            ErrorInfoDic.Add(
                ALERRID.ERR_ModuleNameDifferent,
                new ERRORINFO(
                    ALERRID.ERR_ModuleNameDifferent,
                    1074,
                    0,
                    "ALSTR_ModuleNameDifferent"));
            ErrorInfoDic.Add(
                ALERRID.WRN_DelaySignWithNoKey,
                new ERRORINFO(
                    ALERRID.WRN_DelaySignWithNoKey,
                    1075,
                    1,
                    "ALSTR_DelaySignWithNoKey"));
            ErrorInfoDic.Add(
                ALERRID.ERR_DuplicateTypeForwarders,
                new ERRORINFO(
                    ALERRID.ERR_DuplicateTypeForwarders,
                    1076,
                    0,
                    "ALSTR_DuplicateTypeForwarders"));
            ErrorInfoDic.Add(
                ALERRID.ERR_TypeFwderMatchesDeclared,
                new ERRORINFO(
                    ALERRID.ERR_TypeFwderMatchesDeclared,
                    1077,
                    0,
                    "ALSTR_TypeFwderMatchesDeclared"));
#endif
        }
    }
}
