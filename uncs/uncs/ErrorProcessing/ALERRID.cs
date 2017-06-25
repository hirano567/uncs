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
// ALERRID.cs
//   (separated from CSharp\ALink\Dll\Assembly.cs)
//
// 2015/10/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;

namespace Uncs
{
    //======================================================================
    // enum ALERRID
    //
    /// <remarks>
    /// enum "ERRORS" in sscli.
    /// </remarks>
    //
    //#define ERRORDEF(num, sev, name, id) name,
    //enum ERRORS {
    //#include "errors.h"
    //};
    //======================================================================
    enum ALERRID : int
    {
        Invalid = -1,

        FTL_InternalError = 0,
        FTL_NoMemory,
        ERR_MissingOptionArg,
        FTL_ComPlusInit,
        FTL_FileTooBig,
        ERR_DuplicateResponseFile,
        ERR_OpenResponseFile,
        ERR_NoFileSpec,
        ERR_CantOpenFileWrite,
        ERR_SwitchNeedsString,
        ERR_CantOpenBinaryAsText,
        ERR_BadOptionValue,
        ERR_BadSwitch,
        FTL_InitError,
        FTL_NoMessagesDLL,
        ERR_NoInputs,
        ERR_NoOutput,
        FTL_RequiredFileNotFound,
        ERR_MetaDataError,
        WRN_IgnoringAssembly,
        WRN_OptionConflicts,
        ERR_CantReadResource,
        ERR_CantEmbedResource,
        // ERRORDEF( 1024, 0, ERR_FileNotInDir, IDS_FileNotInDir)  // Cut as part of multi-dir assemblies
        ERR_InvalidFileDefInComType,
        ERR_InvalidVersionFormat,
        ERR_InvalidOSString,
        ERR_NeedPrivateKey,
        ERR_CryptoNoKeyContainer,
        ERR_CryptoFailed,
        ERR_CantReadIcon,
        ERR_AutoResGen,
        ERR_DuplicateCA,
        ERR_CantRenameAssembly,
        ERR_NoMainOnDlls,
        ERR_AppNeedsMain,
        ERR_NoMainFound,
        //ERRORDEF( 1038, 0, ERR_OutAndInstall, IDS_OutAndInstall)
        FTL_FusionInit,
        ERR_FusionInstallFailed,
        ERR_BadMainFound,
        ERR_CantAddExes,
        ERR_SameOutAndSource,
        ERR_CryptoFileFailed,
        ERR_FileNameTooLong,
        ERR_DupResourceIdent,
        ERR_ModuleImportError,
        ERR_AssemblyModuleImportError,
        WRN_InvalidTime,
        WRN_FeatureDeprecated,
        ERR_EmitCAFailed,
        ERR_ParentNotAnAssembly,
        WRN_InvalidVersionString,
        ERR_InvalidVersionString,
        ERR_RefNotStrong,
        WRN_RefHasCulture,
        ERR_ExeHasCulture,
        ERR_CantAddAssembly,
        ERR_UnknownError,
        ERR_CryptoHashFailed,
        ERR_BadOptionValueHR,
        WRN_IgnoringDuplicateSource,
        ERR_DuplicateExportedType,
        // 1064
        FTL_InputFileNameTooLong,
        ERR_IllegalOptionChar,
        ERR_BinaryFile,
        ERR_DuplicateModule,
        FTL_OutputFileExists,
        ERR_AgnosticToMachine,
        // 1071
        ERR_ConflictingMachine,
        WRN_ConflictingMachine,
        ERR_ModuleNameDifferent,
        WRN_DelaySignWithNoKey,
        ERR_DuplicateTypeForwarders,
        ERR_TypeFwderMatchesDeclared,
    }
}
