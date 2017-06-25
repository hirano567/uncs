//============================================================================
// Resources.cs
//
// 2015/10/18 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Resources;

namespace Uncs
{
    //======================================================================
    // class CResources (static)
    //======================================================================
    internal static class CResources
    {
        //============================================================
        // class CResources.Info
        //============================================================
        internal class Info
        {
            //--------------------------------------------------------
            // CResources.Info Fields and Properties
            //--------------------------------------------------------
            internal ResNo No = ResNo.Invalid;
            internal string ID = null;
            internal int ArgCount = 0;

            //--------------------------------------------------------
            // CResources.Info Constructor
            //
            /// <summary></summary>
            /// <param name="no"></param>
            /// <param name="id"></param>
            /// <param name="count"></param>
            //--------------------------------------------------------
            internal Info(ResNo no, string id, int count)
            {
                this.No = no;
                this.ID = id;
                this.ArgCount = count;
            }
        }

        //------------------------------------------------------------
        // CResources Fields and Properties
        //------------------------------------------------------------
        internal static ResourceManager Manager =
            new ResourceManager("uncs.Properties.Resources", typeof(CResources).Assembly);

        internal static Dictionary<ResNo, Info> InfoDic
            = new Dictionary<ResNo, Info>();

        //------------------------------------------------------------
        // CResources Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        static CResources()
        {
            Init();
        }

        //------------------------------------------------------------
        // CResources.Init
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal static void Init()
        {
            InitInfoDic();
        }

        //------------------------------------------------------------
        // CResources.GetInfo
        //
        /// <summary></summary>
        /// <param name="resNo"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool GetInfo(ResNo resNo, out Info info)
        {
            if (InfoDic.TryGetValue(resNo, out info))
            {
                return true;
            }
            info = null;
            return false;
        }

        //------------------------------------------------------------
        // CResources.GetString (1)
        //
        /// <summary></summary>
        /// <param name="resID"></param>
        /// <param name="resStr"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool GetString(ResNo resNo, out string resStr, out Exception excp)
        {
            Info info = null;
            excp = null;

            try
            {
                if (InfoDic.TryGetValue(resNo, out info))
                {
                    resStr = Manager.GetString(info.ID, Config.Culture);
                    return true;
                }
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (MissingManifestResourceException ex)
            {
                excp = ex;
            }
            catch (MissingSatelliteAssemblyException ex)
            {
                excp = ex;
            }
            resStr = null;
            return false;
        }

        //------------------------------------------------------------
        // CResources.GetString (2)
        //
        /// <summary></summary>
        /// <param name="resID"></param>
        /// <param name="resStr"></param>
        /// <param name="argCount"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool GetString(
            ResNo resNo,
            out string resStr,
            out int argCount,
            out Exception excp)
        {
            excp = null;
            Info info = null;

            try
            {
                if (InfoDic.TryGetValue(resNo, out info))
                {
                    resStr = Manager.GetString(info.ID, Config.Culture);
                    argCount = info.ArgCount;
                    return true;
                }
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (MissingManifestResourceException ex)
            {
                excp = ex;
            }
            catch (MissingSatelliteAssemblyException ex)
            {
                excp = ex;
            }
            resStr = null;
            argCount = 0;
            return false;
        }

        //------------------------------------------------------------
        // CResources.AddInfo
        //
        /// <summary></summary>
        /// <param name="resNo"></param>
        /// <param name="resID"></param>
        /// <param name="argCount"></param>
        //------------------------------------------------------------
        internal static void AddInfo(ResNo resNo, string resID, int argCount)
        {
            try
            {
                InfoDic.Add(resNo, new Info(resNo, resID, argCount));
            }
            catch (ArgumentException ex)
            {
            }
        }

        //------------------------------------------------------------
        // CResources.InitInfoDic
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal static void InitInfoDic()
        {
            AddInfo(ResNo.ALSTR_AgnosticToMachineModule, "ALSTR_AgnosticToMachineModule", 1);
            AddInfo(ResNo.ALSTR_AppNeedsMain, "ALSTR_AppNeedsMain", 0);
            AddInfo(ResNo.ALSTR_AssemblyModuleImportError, "ALSTR_AssemblyModuleImportError", 3);
            AddInfo(ResNo.ALSTR_AutoResGen, "ALSTR_AutoResGen", 2);
            AddInfo(ResNo.ALSTR_BadMainFound, "ALSTR_BadMainFound", 1);
            AddInfo(ResNo.ALSTR_BadOptionValue, "ALSTR_BadOptionValue", 2);
            AddInfo(ResNo.ALSTR_BadOptionValueHR, "ALSTR_BadOptionValueHR", 2);
            AddInfo(ResNo.ALSTR_BadSwitch, "ALSTR_BadSwitch", 1);
            AddInfo(ResNo.ALSTR_BANNER1, "ALSTR_BANNER1", 0);
            AddInfo(ResNo.ALSTR_BANNER1PART2, "ALSTR_BANNER1PART2", 0);
            AddInfo(ResNo.ALSTR_BANNER2, "ALSTR_BANNER2", 0);
            AddInfo(ResNo.ALSTR_BinaryFile, "ALSTR_BinaryFile", 1);
            AddInfo(ResNo.ALSTR_BUGREPORTWARN, "ALSTR_BUGREPORTWARN", 0);
            AddInfo(ResNo.ALSTR_CantAddAssembly, "ALSTR_CantAddAssembly", 1);
            AddInfo(ResNo.ALSTR_CantAddExes, "ALSTR_CantAddExes", 1);
            AddInfo(ResNo.ALSTR_CantEmbedResource, "ALSTR_CantEmbedResource", 2);
            AddInfo(ResNo.ALSTR_CantOpenBinaryAsText, "ALSTR_CantOpenBinaryAsText", 1);
            AddInfo(ResNo.ALSTR_CantOpenFileWrite, "ALSTR_CantOpenFileWrite", 1);
            AddInfo(ResNo.ALSTR_CantReadIcon, "ALSTR_CantReadIcon", 2);
            AddInfo(ResNo.ALSTR_CantReadResource, "ALSTR_CantReadResource", 2);
            AddInfo(ResNo.ALSTR_CantRenameAssembly, "ALSTR_CantRenameAssembly", 1);
            AddInfo(ResNo.ALSTR_ComPlusInit, "ALSTR_ComPlusInit", 1);
            AddInfo(ResNo.ALSTR_ConflictingMachineAssembly, "ALSTR_ConflictingMachineAssembly", 1);
            AddInfo(ResNo.ALSTR_ConflictingMachineModule, "ALSTR_ConflictingMachineModule", 1);
            AddInfo(ResNo.ALSTR_CryptoFailed, "ALSTR_CryptoFailed", 0);
            AddInfo(ResNo.ALSTR_CryptoFileFailed, "ALSTR_CryptoFileFailed", 2);
            AddInfo(ResNo.ALSTR_CryptoHashFailed, "ALSTR_CryptoHashFailed", 1);
            AddInfo(ResNo.ALSTR_CryptoNoKeyContainer, "ALSTR_CryptoNoKeyContainer", 1);
            AddInfo(ResNo.ALSTR_DelaySignWithNoKey, "ALSTR_DelaySignWithNoKey", 0);
            AddInfo(ResNo.ALSTR_DuplicateCA, "ALSTR_DuplicateCA", 1);
            AddInfo(ResNo.ALSTR_DuplicateExportedType, "ALSTR_DuplicateExportedType", 3);
            AddInfo(ResNo.ALSTR_DuplicateModule, "ALSTR_DuplicateModule", 1);
            AddInfo(ResNo.ALSTR_DuplicateResponseFile, "ALSTR_DuplicateResponseFile", 1);
            AddInfo(ResNo.ALSTR_DuplicateTypeForwarders, "ALSTR_DuplicateTypeForwarders", 3);
            AddInfo(ResNo.ALSTR_DupResourceIdent, "ALSTR_DupResourceIdent", 1);
            AddInfo(ResNo.ALSTR_EmitCAFailed, "ALSTR_EmitCAFailed", 2);
            AddInfo(ResNo.ALSTR_ENTERCORRECT, "ALSTR_ENTERCORRECT", 0);
            AddInfo(ResNo.ALSTR_ENTERDESC, "ALSTR_ENTERDESC", 0);
            AddInfo(ResNo.ALSTR_ExeHasCulture, "ALSTR_ExeHasCulture", 0);
            AddInfo(ResNo.ALSTR_FeatureDeprecated, "ALSTR_FeatureDeprecated", 2);
            AddInfo(ResNo.ALSTR_FileNameTooLong, "ALSTR_FileNameTooLong", 1);
            AddInfo(ResNo.ALSTR_FileTooBig, "ALSTR_FileTooBig", 1);
            AddInfo(ResNo.ALSTR_FusionInit, "ALSTR_FusionInit", 1);
            AddInfo(ResNo.ALSTR_FusionInstallFailed, "ALSTR_FusionInstallFailed", 1);
            AddInfo(ResNo.ALSTR_H_ALGID, "ALSTR_H_ALGID", 0);
            AddInfo(ResNo.ALSTR_H_BASEADDRESS, "ALSTR_H_BASEADDRESS", 0);
            AddInfo(ResNo.ALSTR_H_BUGREPORT, "ALSTR_H_BUGREPORT", 0);
            AddInfo(ResNo.ALSTR_H_COMPANY, "ALSTR_H_COMPANY", 0);
            AddInfo(ResNo.ALSTR_H_CONFIG, "ALSTR_H_CONFIG", 0);
            AddInfo(ResNo.ALSTR_H_COPYRIGHT, "ALSTR_H_COPYRIGHT", 0);
            AddInfo(ResNo.ALSTR_H_DELAYSIGN, "ALSTR_H_DELAYSIGN", 0);
            AddInfo(ResNo.ALSTR_H_DESCR, "ALSTR_H_DESCR", 0);
            AddInfo(ResNo.ALSTR_H_EMBED1, "ALSTR_H_EMBED1", 0);
            AddInfo(ResNo.ALSTR_H_EMBED2, "ALSTR_H_EMBED2", 0);
            AddInfo(ResNo.ALSTR_H_EVIDENCE, "ALSTR_H_EVIDENCE", 0);
            AddInfo(ResNo.ALSTR_H_FILEVER, "ALSTR_H_FILEVER", 0);
            AddInfo(ResNo.ALSTR_H_FLAGS, "ALSTR_H_FLAGS", 0);
            AddInfo(ResNo.ALSTR_H_FULLPATHS, "ALSTR_H_FULLPATHS", 0);
            AddInfo(ResNo.ALSTR_H_HELP, "ALSTR_H_HELP", 0);
            AddInfo(ResNo.ALSTR_H_KEYFILE, "ALSTR_H_KEYFILE", 0);
            AddInfo(ResNo.ALSTR_H_KEYNAME, "ALSTR_H_KEYNAME", 0);
            AddInfo(ResNo.ALSTR_H_LINK1, "ALSTR_H_LINK1", 0);
            AddInfo(ResNo.ALSTR_H_LINK2, "ALSTR_H_LINK2", 0);
            AddInfo(ResNo.ALSTR_H_LOCALE, "ALSTR_H_LOCALE", 0);
            AddInfo(ResNo.ALSTR_H_MAIN, "ALSTR_H_MAIN", 0);
            AddInfo(ResNo.ALSTR_H_NOLOGO, "ALSTR_H_NOLOGO", 0);
            AddInfo(ResNo.ALSTR_H_OUT, "ALSTR_H_OUT", 0);
            AddInfo(ResNo.ALSTR_H_PLATFORM, "ALSTR_H_PLATFORM", 0);
            AddInfo(ResNo.ALSTR_H_PLATFORM2, "ALSTR_H_PLATFORM2", 0);
            AddInfo(ResNo.ALSTR_H_PRODUCT, "ALSTR_H_PRODUCT", 0);
            AddInfo(ResNo.ALSTR_H_PRODVER, "ALSTR_H_PRODVER", 0);
            AddInfo(ResNo.ALSTR_H_RESPONSE, "ALSTR_H_RESPONSE", 0);
            AddInfo(ResNo.ALSTR_H_SOURCEFILE, "ALSTR_H_SOURCEFILE", 0);
            AddInfo(ResNo.ALSTR_H_TARGET, "ALSTR_H_TARGET", 0);
            AddInfo(ResNo.ALSTR_H_TARGET2, "ALSTR_H_TARGET2", 0);
            AddInfo(ResNo.ALSTR_H_TARGET3, "ALSTR_H_TARGET3", 0);
            AddInfo(ResNo.ALSTR_H_TEMPLATE, "ALSTR_H_TEMPLATE", 0);
            AddInfo(ResNo.ALSTR_H_TITLE, "ALSTR_H_TITLE", 0);
            AddInfo(ResNo.ALSTR_H_TRADEMARK, "ALSTR_H_TRADEMARK", 0);
            AddInfo(ResNo.ALSTR_H_VERSION, "ALSTR_H_VERSION", 0);
            AddInfo(ResNo.ALSTR_H_WIN32ICON, "ALSTR_H_WIN32ICON", 0);
            AddInfo(ResNo.ALSTR_H_WIN32RES, "ALSTR_H_WIN32RES", 0);
            AddInfo(ResNo.ALSTR_HELP10, "ALSTR_HELP10", 0);
            AddInfo(ResNo.ALSTR_HELP20, "ALSTR_HELP20", 0);
            AddInfo(ResNo.ALSTR_HELP30, "ALSTR_HELP30", 0);
            AddInfo(ResNo.ALSTR_IgnoringAssembly, "ALSTR_IgnoringAssembly", 1);
            AddInfo(ResNo.ALSTR_IgnoringDuplicateSource, "ALSTR_IgnoringDuplicateSource", 1);
            AddInfo(ResNo.ALSTR_IllegalOptionChar, "ALSTR_IllegalOptionChar", 1);
            AddInfo(ResNo.ALSTR_InitError, "ALSTR_InitError", 1);
            AddInfo(ResNo.ALSTR_InputFileNameTooLong, "ALSTR_InputFileNameTooLong", 1);
            AddInfo(ResNo.ALSTR_InternalError, "ALSTR_InternalError", 0);
            AddInfo(ResNo.ALSTR_InvalidFileDefInComType, "ALSTR_InvalidFileDefInComType", 2);
            AddInfo(ResNo.ALSTR_InvalidOSString, "ALSTR_InvalidOSString", 1);
            AddInfo(ResNo.ALSTR_InvalidTime, "ALSTR_InvalidTime", 0);
            AddInfo(ResNo.ALSTR_InvalidVersionFormat, "ALSTR_InvalidVersionFormat", 2);
            AddInfo(ResNo.ALSTR_InvalidVersionString, "ALSTR_InvalidVersionString", 1);
            AddInfo(ResNo.ALSTR_MetaDataError, "ALSTR_MetaDataError", 1);
            AddInfo(ResNo.ALSTR_MissingOptionArg, "ALSTR_MissingOptionArg", 1);
            AddInfo(ResNo.ALSTR_ModuleImportError, "ALSTR_ModuleImportError", 2);
            AddInfo(ResNo.ALSTR_ModuleNameDifferent, "ALSTR_ModuleNameDifferent", 2);
            AddInfo(ResNo.ALSTR_N_ALGID, "ALSTR_N_ALGID", 0);
            AddInfo(ResNo.ALSTR_N_COMPANY, "ALSTR_N_COMPANY", 0);
            AddInfo(ResNo.ALSTR_N_CONFIG, "ALSTR_N_CONFIG", 0);
            AddInfo(ResNo.ALSTR_N_COPYRIGHT, "ALSTR_N_COPYRIGHT", 0);
            AddInfo(ResNo.ALSTR_N_DELAYSIGN, "ALSTR_N_DELAYSIGN", 0);
            AddInfo(ResNo.ALSTR_N_DESCR, "ALSTR_N_DESCR", 0);
            AddInfo(ResNo.ALSTR_N_FILEVER, "ALSTR_N_FILEVER", 0);
            AddInfo(ResNo.ALSTR_N_FLAGS, "ALSTR_N_FLAGS", 0);
            AddInfo(ResNo.ALSTR_N_FRIENDASSEM, "ALSTR_N_FRIENDASSEM", 0);
            AddInfo(ResNo.ALSTR_N_KEYFILE, "ALSTR_N_KEYFILE", 0);
            AddInfo(ResNo.ALSTR_N_KEYNAME, "ALSTR_N_KEYNAME", 0);
            AddInfo(ResNo.ALSTR_N_LOCALE, "ALSTR_N_LOCALE", 0);
            AddInfo(ResNo.ALSTR_N_OS, "ALSTR_N_OS", 0);
            AddInfo(ResNo.ALSTR_N_PROC, "ALSTR_N_PROC", 0);
            AddInfo(ResNo.ALSTR_N_PRODUCT, "ALSTR_N_PRODUCT", 0);
            AddInfo(ResNo.ALSTR_N_PRODVER, "ALSTR_N_PRODVER", 0);
            AddInfo(ResNo.ALSTR_N_SATELLITEVER, "ALSTR_N_SATELLITEVER", 0);
            AddInfo(ResNo.ALSTR_N_TITLE, "ALSTR_N_TITLE", 0);
            AddInfo(ResNo.ALSTR_N_TRADEMARK, "ALSTR_N_TRADEMARK", 0);
            AddInfo(ResNo.ALSTR_N_VERSION, "ALSTR_N_VERSION", 0);
            AddInfo(ResNo.ALSTR_NeedPrivateKey, "ALSTR_NeedPrivateKey", 1);
            AddInfo(ResNo.ALSTR_NoFileSpec, "ALSTR_NoFileSpec", 1);
            AddInfo(ResNo.ALSTR_NoInputs, "ALSTR_NoInputs", 0);
            AddInfo(ResNo.ALSTR_NoMainFound, "ALSTR_NoMainFound", 1);
            AddInfo(ResNo.ALSTR_NoMainOnDLLs, "ALSTR_NoMainOnDLLs", 0);
            AddInfo(ResNo.ALSTR_NoMemory, "ALSTR_NoMemory", 0);
            AddInfo(ResNo.ALSTR_NoOutput, "ALSTR_NoOutput", 0);
            AddInfo(ResNo.ALSTR_OpenResponseFile, "ALSTR_OpenResponseFile", 2);
            AddInfo(ResNo.ALSTR_OPTDSC_ARGORITHMID, "ALSTR_OPTDSC_ARGORITHMID", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_COMPANY, "ALSTR_OPTDSC_COMPANY", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_CONFIGURATION, "ALSTR_OPTDSC_CONFIGURATION", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_COPYRIGHT, "ALSTR_OPTDSC_COPYRIGHT", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_CULTURE, "ALSTR_OPTDSC_CULTURE", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_DESCRIPTION, "ALSTR_OPTDSC_DESCRIPTION", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_EVIDENCE, "ALSTR_OPTDSC_EVIDENCE", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_FILEVERSION, "ALSTR_OPTDSC_FILEVERSION", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_FLAGS, "ALSTR_OPTDSC_FLAGS", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_PRODUCT, "ALSTR_OPTDSC_PRODUCT", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_PRODUCTVERSION, "ALSTR_OPTDSC_PRODUCTVERSION", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_TEMPLATE, "ALSTR_OPTDSC_TEMPLATE", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_TITLE, "ALSTR_OPTDSC_TITLE", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_TRADEMARK, "ALSTR_OPTDSC_TRADEMARK", 0);
            AddInfo(ResNo.ALSTR_OPTDSC_VERSION, "ALSTR_OPTDSC_VERSION", 0);
            AddInfo(ResNo.ALSTR_OPTGRP_OPTIONS, "ALSTR_OPTGRP_OPTIONS", 0);
            AddInfo(ResNo.ALSTR_OPTGRP_SOURCES, "ALSTR_OPTGRP_SOURCES", 0);
            AddInfo(ResNo.ALSTR_OptionConflicts, "ALSTR_OptionConflicts", 1);
            AddInfo(ResNo.ALSTR_OPTSYN_ARGORITHMID, "ALSTR_OPTSYN_ARGORITHMID", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_COMPANY, "ALSTR_OPTSYN_COMPANY", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_CONFIGURATION, "ALSTR_OPTSYN_CONFIGURATION", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_COPYRIGHT, "ALSTR_OPTSYN_COPYRIGHT", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_CULTURE, "ALSTR_OPTSYN_CULTURE", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_DESCRIPTION, "ALSTR_OPTSYN_DESCRIPTION", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_EVIDENCE, "ALSTR_OPTSYN_EVIDENCE", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_FILEVERSION, "ALSTR_OPTSYN_FILEVERSION", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_FLAGS, "ALSTR_OPTSYN_FLAGS", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_PRODUCT, "ALSTR_OPTSYN_PRODUCT", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_PRODUCTVERSION, "ALSTR_OPTSYN_PRODUCTVERSION", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_TEMPLATE, "ALSTR_OPTSYN_TEMPLATE", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_TITLE, "ALSTR_OPTSYN_TITLE", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_TRADEMARK, "ALSTR_OPTSYN_TRADEMARK", 0);
            AddInfo(ResNo.ALSTR_OPTSYN_VERSION, "ALSTR_OPTSYN_VERSION", 0);
            AddInfo(ResNo.ALSTR_OutputFileExists, "ALSTR_OutputFileExists", 1);
            AddInfo(ResNo.ALSTR_ParentNotAnAssembly, "ALSTR_ParentNotAnAssembly", 1);
            AddInfo(ResNo.ALSTR_RefHasCulture, "ALSTR_RefHasCulture", 1);
            AddInfo(ResNo.ALSTR_RefNotStrong, "ALSTR_RefNotStrong", 1);
            AddInfo(ResNo.ALSTR_REPROBINFILE, "ALSTR_REPROBINFILE", 1);
            AddInfo(ResNo.ALSTR_REPROCOMMANDLINE, "ALSTR_REPROCOMMANDLINE", 0);
            AddInfo(ResNo.ALSTR_REPROCORRECTBEHAVIOR, "ALSTR_REPROCORRECTBEHAVIOR", 0);
            AddInfo(ResNo.ALSTR_REPRODESCRIPTION, "ALSTR_REPRODESCRIPTION", 0);
            AddInfo(ResNo.ALSTR_REPRODIAGS, "ALSTR_REPRODIAGS", 0);
            AddInfo(ResNo.ALSTR_REPROLCID, "ALSTR_REPROLCID", 1);
            AddInfo(ResNo.ALSTR_REPROOS, "ALSTR_REPROOS", 5);
            AddInfo(ResNo.ALSTR_REPROSOURCEFILE, "ALSTR_REPROSOURCEFILE", 1);
            AddInfo(ResNo.ALSTR_REPROTITLE, "ALSTR_REPROTITLE", 1);
            AddInfo(ResNo.ALSTR_REPROURTVER, "ALSTR_REPROURTVER", 1);
            AddInfo(ResNo.ALSTR_REPROVER, "ALSTR_REPROVER", 1);
            AddInfo(ResNo.ALSTR_RequiredFileNotFound, "ALSTR_RequiredFileNotFound", 1);
            AddInfo(ResNo.ALSTR_SameOutAndSource, "ALSTR_SameOutAndSource", 1);
            AddInfo(ResNo.ALSTR_SwitchNeedsString, "ALSTR_SwitchNeedsString", 1);
            AddInfo(ResNo.ALSTR_TypeFwderMatchesDeclared, "ALSTR_TypeFwderMatchesDeclared", 3);
            AddInfo(ResNo.ALSTR_UnknownError, "ALSTR_UnknownError", 1);
            AddInfo(ResNo.CSCSTR_AbstractAndExtern, "CSCSTR_AbstractAndExtern", 1);
            AddInfo(ResNo.CSCSTR_AbstractAndSealed, "CSCSTR_AbstractAndSealed", 1);
            AddInfo(ResNo.CSCSTR_AbstractAttributeClass, "CSCSTR_AbstractAttributeClass", 1);
            AddInfo(ResNo.CSCSTR_AbstractBaseCall, "CSCSTR_AbstractBaseCall", 1);
            AddInfo(ResNo.CSCSTR_AbstractEventInitializer, "CSCSTR_AbstractEventInitializer", 1);
            AddInfo(ResNo.CSCSTR_AbstractField, "CSCSTR_AbstractField", 0);
            AddInfo(ResNo.CSCSTR_AbstractHasBody, "CSCSTR_AbstractHasBody", 1);
            AddInfo(ResNo.CSCSTR_AbstractInConcreteClass, "CSCSTR_AbstractInConcreteClass", 2);
            AddInfo(ResNo.CSCSTR_AbstractNotVirtual, "CSCSTR_AbstractNotVirtual", 1);
            AddInfo(ResNo.CSCSTR_AbstractSealedStatic, "CSCSTR_AbstractSealedStatic", 1);
            AddInfo(ResNo.CSCSTR_AccessModMissingAccessor, "CSCSTR_AccessModMissingAccessor", 1);
            AddInfo(ResNo.CSCSTR_AccessorImplementingMethod, "CSCSTR_AccessorImplementingMethod", 3);
            AddInfo(ResNo.CSCSTR_AddModuleAssembly, "CSCSTR_AddModuleAssembly", 1);
            AddInfo(ResNo.CSCSTR_AddOrRemoveExpected, "CSCSTR_AddOrRemoveExpected", 0);
            AddInfo(ResNo.CSCSTR_AddRemoveMustHaveBody, "CSCSTR_AddRemoveMustHaveBody", 0);
            AddInfo(ResNo.CSCSTR_AddrOnReadOnlyLocal, "CSCSTR_AddrOnReadOnlyLocal", 0);
            AddInfo(ResNo.CSCSTR_AK_CLASS, "CSCSTR_AK_CLASS", 0);
            AddInfo(ResNo.CSCSTR_AK_DELEGATE, "CSCSTR_AK_DELEGATE", 0);
            AddInfo(ResNo.CSCSTR_AK_ENUM, "CSCSTR_AK_ENUM", 0);
            AddInfo(ResNo.CSCSTR_AK_INTERFACE, "CSCSTR_AK_INTERFACE", 0);
            AddInfo(ResNo.CSCSTR_AK_STRUCT, "CSCSTR_AK_STRUCT", 0);
            AddInfo(ResNo.CSCSTR_AK_UNKNOWN, "CSCSTR_AK_UNKNOWN", 0);
            AddInfo(ResNo.CSCSTR_AliasMissingFile, "CSCSTR_AliasMissingFile", 1);
            AddInfo(ResNo.CSCSTR_AliasNotFound, "CSCSTR_AliasNotFound", 1);
            AddInfo(ResNo.CSCSTR_AliasQualAsExpression, "CSCSTR_AliasQualAsExpression", 0);
            AddInfo(ResNo.CSCSTR_ALinkCloseFailed, "CSCSTR_ALinkCloseFailed", 1);
            AddInfo(ResNo.CSCSTR_ALinkFailed, "CSCSTR_ALinkFailed", 1);
            AddInfo(ResNo.CSCSTR_ALinkWarn, "CSCSTR_ALinkWarn", 1);
            AddInfo(ResNo.CSCSTR_AlwaysNull, "CSCSTR_AlwaysNull", 1);
            AddInfo(ResNo.CSCSTR_AmbigBinaryOps, "CSCSTR_AmbigBinaryOps", 3);
            AddInfo(ResNo.CSCSTR_AmbigCall, "CSCSTR_AmbigCall", 2);
            AddInfo(ResNo.CSCSTR_AmbigContext, "CSCSTR_AmbigContext", 3);
            AddInfo(ResNo.CSCSTR_AmbigLookupMeth, "CSCSTR_AmbigLookupMeth", 2);
            AddInfo(ResNo.CSCSTR_AmbigMember, "CSCSTR_AmbigMember", 2);
            AddInfo(ResNo.CSCSTR_AmbigousAttribute, "CSCSTR_AmbigousAttribute", 3);
            AddInfo(ResNo.CSCSTR_AmbigOverride, "CSCSTR_AmbigOverride", 3);
            AddInfo(ResNo.CSCSTR_AmbigQM, "CSCSTR_AmbigQM", 2);
            AddInfo(ResNo.CSCSTR_AmbigUDConv, "CSCSTR_AmbigUDConv", 4);
            AddInfo(ResNo.CSCSTR_AmbigUnaryOp, "CSCSTR_AmbigUnaryOp", 2);
            AddInfo(ResNo.CSCSTR_AmbiguousXMLReference, "CSCSTR_AmbiguousXMLReference", 3);
            AddInfo(ResNo.CSCSTR_AnonDelegateCantUse, "CSCSTR_AnonDelegateCantUse", 1);
            AddInfo(ResNo.CSCSTR_AnonMethGrpInForEach, "CSCSTR_AnonMethGrpInForEach", 1);
            AddInfo(ResNo.CSCSTR_AnonMethNotAllowed, "CSCSTR_AnonMethNotAllowed", 0);
            AddInfo(ResNo.CSCSTR_AnonMethod, "CSCSTR_AnonMethod", 0);
            AddInfo(ResNo.CSCSTR_AnonMethToNonDel, "CSCSTR_AnonMethToNonDel", 2);
            AddInfo(ResNo.CSCSTR_AnonymousReturnExpected, "CSCSTR_AnonymousReturnExpected", 1);
            AddInfo(ResNo.CSCSTR_ArgsInvalid, "CSCSTR_ArgsInvalid", 0);
            AddInfo(ResNo.CSCSTR_ArrayElementCantBeRefAny, "CSCSTR_ArrayElementCantBeRefAny", 1);
            AddInfo(ResNo.CSCSTR_ArrayInitInBadPlace, "CSCSTR_ArrayInitInBadPlace", 0);
            AddInfo(ResNo.CSCSTR_ArrayInitToNonArrayType, "CSCSTR_ArrayInitToNonArrayType", 0);
            AddInfo(ResNo.CSCSTR_ArrayOfStaticClass, "CSCSTR_ArrayOfStaticClass", 1);
            AddInfo(ResNo.CSCSTR_ArraySizeInDeclaration, "CSCSTR_ArraySizeInDeclaration", 0);
            AddInfo(ResNo.CSCSTR_AsMustHaveReferenceType, "CSCSTR_AsMustHaveReferenceType", 1);
            AddInfo(ResNo.CSCSTR_AssemblyMatchBadVersion, "CSCSTR_AssemblyMatchBadVersion", 3);
            AddInfo(ResNo.CSCSTR_AssemblyNameOnNonModule, "CSCSTR_AssemblyNameOnNonModule", 0);
            AddInfo(ResNo.CSCSTR_AssgLvalueExpected, "CSCSTR_AssgLvalueExpected", 0);
            AddInfo(ResNo.CSCSTR_AssgReadonly, "CSCSTR_AssgReadonly", 0);
            AddInfo(ResNo.CSCSTR_AssgReadonly2, "CSCSTR_AssgReadonly2", 1);
            AddInfo(ResNo.CSCSTR_AssgReadonlyLocal, "CSCSTR_AssgReadonlyLocal", 1);
            AddInfo(ResNo.CSCSTR_AssgReadonlyLocal2, "CSCSTR_AssgReadonlyLocal2", 1);
            AddInfo(ResNo.CSCSTR_AssgReadonlyLocal2Cause, "CSCSTR_AssgReadonlyLocal2Cause", 2);
            AddInfo(ResNo.CSCSTR_AssgReadonlyLocalCause, "CSCSTR_AssgReadonlyLocalCause", 2);
            AddInfo(ResNo.CSCSTR_AssgReadonlyProp, "CSCSTR_AssgReadonlyProp", 1);
            AddInfo(ResNo.CSCSTR_AssgReadonlyStatic, "CSCSTR_AssgReadonlyStatic", 0);
            AddInfo(ResNo.CSCSTR_AssgReadonlyStatic2, "CSCSTR_AssgReadonlyStatic2", 1);
            AddInfo(ResNo.CSCSTR_AssignmentToLockOrDispose, "CSCSTR_AssignmentToLockOrDispose", 1);
            AddInfo(ResNo.CSCSTR_AssignmentToSelf, "CSCSTR_AssignmentToSelf", 0);
            AddInfo(ResNo.CSCSTR_AssumedMatchThis, "CSCSTR_AssumedMatchThis", 2);
            AddInfo(ResNo.CSCSTR_AsWithTypeVar, "CSCSTR_AsWithTypeVar", 1);
            AddInfo(ResNo.CSCSTR_AttrArgWithTypeVars, "CSCSTR_AttrArgWithTypeVars", 1);
            AddInfo(ResNo.CSCSTR_AttributeCantBeGeneric, "CSCSTR_AttributeCantBeGeneric", 0);
            AddInfo(ResNo.CSCSTR_AttributeLocationOnBadDeclaration, "CSCSTR_AttributeLocationOnBadDeclaration", 2);
            AddInfo(ResNo.CSCSTR_AttributeNotOnAccessor, "CSCSTR_AttributeNotOnAccessor", 2);
            AddInfo(ResNo.CSCSTR_AttributeOnBadSymbolType, "CSCSTR_AttributeOnBadSymbolType", 2);
            AddInfo(ResNo.CSCSTR_AttributeUsageOnNonAttributeClass, "CSCSTR_AttributeUsageOnNonAttributeClass", 1);
            AddInfo(ResNo.CSCSTR_AttrOnTypeArg, "CSCSTR_AttrOnTypeArg", 0);
            AddInfo(ResNo.CSCSTR_AutoResGen, "CSCSTR_AutoResGen", 1);
            AddInfo(ResNo.CSCSTR_BadAccess, "CSCSTR_BadAccess", 1);
            AddInfo(ResNo.CSCSTR_BadArgCount, "CSCSTR_BadArgCount", 2);
            AddInfo(ResNo.CSCSTR_BadArgExtraRef, "CSCSTR_BadArgExtraRef", 2);
            AddInfo(ResNo.CSCSTR_BadArgRef, "CSCSTR_BadArgRef", 2);
            AddInfo(ResNo.CSCSTR_BadArgType, "CSCSTR_BadArgType", 3);
            AddInfo(ResNo.CSCSTR_BadArgTypes, "CSCSTR_BadArgTypes", 1);
            AddInfo(ResNo.CSCSTR_BadArgumentToAttribute, "CSCSTR_BadArgumentToAttribute", 1);
            AddInfo(ResNo.CSCSTR_BadArity, "CSCSTR_BadArity", 3);
            AddInfo(ResNo.CSCSTR_BadArraySyntax, "CSCSTR_BadArraySyntax", 0);
            AddInfo(ResNo.CSCSTR_BadAttributeParam, "CSCSTR_BadAttributeParam", 0);
            AddInfo(ResNo.CSCSTR_BADBASENUMBER, "CSCSTR_BADBASENUMBER", 1);
            AddInfo(ResNo.CSCSTR_BadBaseType, "CSCSTR_BadBaseType", 0);
            AddInfo(ResNo.CSCSTR_BadBinaryOperatorSignature, "CSCSTR_BadBinaryOperatorSignature", 0);
            AddInfo(ResNo.CSCSTR_BadBinaryOps, "CSCSTR_BadBinaryOps", 3);
            AddInfo(ResNo.CSCSTR_BadBinOpArgs, "CSCSTR_BadBinOpArgs", 1);
            AddInfo(ResNo.CSCSTR_BadBoolOp, "CSCSTR_BadBoolOp", 1);
            AddInfo(ResNo.CSCSTR_BadBoundType, "CSCSTR_BadBoundType", 1);
            AddInfo(ResNo.CSCSTR_BadCastInFixed, "CSCSTR_BadCastInFixed", 0);
            AddInfo(ResNo.CSCSTR_BadCoClassSig, "CSCSTR_BadCoClassSig", 2);
            AddInfo(ResNo.CSCSTR_BADCODEPAGE, "CSCSTR_BADCODEPAGE", 1);
            AddInfo(ResNo.CSCSTR_BadCompatMode, "CSCSTR_BadCompatMode", 1);
            AddInfo(ResNo.CSCSTR_BadConstraintType, "CSCSTR_BadConstraintType", 0);
            AddInfo(ResNo.CSCSTR_BadConstType, "CSCSTR_BadConstType", 1);
            AddInfo(ResNo.CSCSTR_BadDebugType, "CSCSTR_BadDebugType", 1);
            AddInfo(ResNo.CSCSTR_BadDelArgCount, "CSCSTR_BadDelArgCount", 2);
            AddInfo(ResNo.CSCSTR_BadDelArgTypes, "CSCSTR_BadDelArgTypes", 1);
            AddInfo(ResNo.CSCSTR_BadDelegateConstructor, "CSCSTR_BadDelegateConstructor", 1);
            AddInfo(ResNo.CSCSTR_BadDelegateLeave, "CSCSTR_BadDelegateLeave", 0);
            AddInfo(ResNo.CSCSTR_BadDestructorName, "CSCSTR_BadDestructorName", 0);
            AddInfo(ResNo.CSCSTR_BadDirectivePlacement, "CSCSTR_BadDirectivePlacement", 0);
            AddInfo(ResNo.CSCSTR_BadEmbeddedStmt, "CSCSTR_BadEmbeddedStmt", 0);
            AddInfo(ResNo.CSCSTR_BadEmptyThrow, "CSCSTR_BadEmptyThrow", 0);
            AddInfo(ResNo.CSCSTR_BadEmptyThrowInFinally, "CSCSTR_BadEmptyThrowInFinally", 0);
            AddInfo(ResNo.CSCSTR_BadEventUsage, "CSCSTR_BadEventUsage", 2);
            AddInfo(ResNo.CSCSTR_BadEventUsageNoField, "CSCSTR_BadEventUsageNoField", 1);
            AddInfo(ResNo.CSCSTR_BadExceptionType, "CSCSTR_BadExceptionType", 0);
            AddInfo(ResNo.CSCSTR_BadExternAlias, "CSCSTR_BadExternAlias", 1);
            AddInfo(ResNo.CSCSTR_BadExternIdentifier, "CSCSTR_BadExternIdentifier", 1);
            AddInfo(ResNo.CSCSTR_BadFileAlignment, "CSCSTR_BadFileAlignment", 1);
            AddInfo(ResNo.CSCSTR_BadFinallyLeave, "CSCSTR_BadFinallyLeave", 0);
            AddInfo(ResNo.CSCSTR_BadFixedInitType, "CSCSTR_BadFixedInitType", 0);
            AddInfo(ResNo.CSCSTR_BadForeachDecl, "CSCSTR_BadForeachDecl", 0);
            AddInfo(ResNo.CSCSTR_BadGetEnumerator, "CSCSTR_BadGetEnumerator", 2);
            AddInfo(ResNo.CSCSTR_BadIncDecRetType, "CSCSTR_BadIncDecRetType", 0);
            AddInfo(ResNo.CSCSTR_BadIncDecsignature, "CSCSTR_BadIncDecsignature", 0);
            AddInfo(ResNo.CSCSTR_BadIndexCount, "CSCSTR_BadIndexCount", 1);
            AddInfo(ResNo.CSCSTR_BadIndexerNameAttr, "CSCSTR_BadIndexerNameAttr", 1);
            AddInfo(ResNo.CSCSTR_BadIndexLHS, "CSCSTR_BadIndexLHS", 1);
            AddInfo(ResNo.CSCSTR_BadIteratorArgType, "CSCSTR_BadIteratorArgType", 0);
            AddInfo(ResNo.CSCSTR_BadIteratorReturn, "CSCSTR_BadIteratorReturn", 2);
            AddInfo(ResNo.CSCSTR_BadMemberFlag, "CSCSTR_BadMemberFlag", 1);
            AddInfo(ResNo.CSCSTR_BadMemberProtection, "CSCSTR_BadMemberProtection", 0);
            AddInfo(ResNo.CSCSTR_BadModifierLocation, "CSCSTR_BadModifierLocation", 1);
            AddInfo(ResNo.CSCSTR_BadModifiersOnNamespace, "CSCSTR_BadModifiersOnNamespace", 0);
            AddInfo(ResNo.CSCSTR_BadNamedAttributeArgument, "CSCSTR_BadNamedAttributeArgument", 1);
            AddInfo(ResNo.CSCSTR_BadNamedAttributeArgumentType, "CSCSTR_BadNamedAttributeArgumentType", 1);
            AddInfo(ResNo.CSCSTR_BadNewExpr, "CSCSTR_BadNewExpr", 0);
            AddInfo(ResNo.CSCSTR_BadOperatorSyntax, "CSCSTR_BadOperatorSyntax", 1);
            AddInfo(ResNo.CSCSTR_BadOperatorSyntax2, "CSCSTR_BadOperatorSyntax2", 1);
            AddInfo(ResNo.CSCSTR_BadParamExtraRef, "CSCSTR_BadParamExtraRef", 2);
            AddInfo(ResNo.CSCSTR_BadParamRef, "CSCSTR_BadParamRef", 2);
            AddInfo(ResNo.CSCSTR_BadParamType, "CSCSTR_BadParamType", 3);
            AddInfo(ResNo.CSCSTR_BadPDBFormat, "CSCSTR_BadPDBFormat", 1);
            AddInfo(ResNo.CSCSTR_BadPlatformType, "CSCSTR_BadPlatformType", 1);
            AddInfo(ResNo.CSCSTR_BadProtectedAccess, "CSCSTR_BadProtectedAccess", 3);
            AddInfo(ResNo.CSCSTR_BadRefCompareLeft, "CSCSTR_BadRefCompareLeft", 1);
            AddInfo(ResNo.CSCSTR_BadRefCompareRight, "CSCSTR_BadRefCompareRight", 1);
            AddInfo(ResNo.CSCSTR_BadResourceVis, "CSCSTR_BadResourceVis", 1);
            AddInfo(ResNo.CSCSTR_BadRestoreNumber, "CSCSTR_BadRestoreNumber", 1);
            AddInfo(ResNo.CSCSTR_BadRetType, "CSCSTR_BadRetType", 2);
            AddInfo(ResNo.CSCSTR_BADSECONDTARGET, "CSCSTR_BADSECONDTARGET", 0);
            AddInfo(ResNo.CSCSTR_BadShiftOperatorSignature, "CSCSTR_BadShiftOperatorSignature", 0);
            AddInfo(ResNo.CSCSTR_BadSKknown, "CSCSTR_BadSKknown", 3);
            AddInfo(ResNo.CSCSTR_BadSKunknown, "CSCSTR_BadSKunknown", 2);
            AddInfo(ResNo.CSCSTR_BadStackAllocExpr, "CSCSTR_BadStackAllocExpr", 0);
            AddInfo(ResNo.CSCSTR_BADSWITCH, "CSCSTR_BADSWITCH", 1);
            AddInfo(ResNo.CSCSTR_BadTokenInType, "CSCSTR_BadTokenInType", 0);
            AddInfo(ResNo.CSCSTR_BadTypeArgument, "CSCSTR_BadTypeArgument", 1);
            AddInfo(ResNo.CSCSTR_BadTypeReference, "CSCSTR_BadTypeReference", 2);
            AddInfo(ResNo.CSCSTR_BadUnaryOp, "CSCSTR_BadUnaryOp", 2);
            AddInfo(ResNo.CSCSTR_BadUnaryOperatorSignature, "CSCSTR_BadUnaryOperatorSignature", 0);
            AddInfo(ResNo.CSCSTR_BadUnOpArgs, "CSCSTR_BadUnOpArgs", 1);
            AddInfo(ResNo.CSCSTR_BadUsingNamespace, "CSCSTR_BadUsingNamespace", 1);
            AddInfo(ResNo.CSCSTR_BadVarargs, "CSCSTR_BadVarargs", 0);
            AddInfo(ResNo.CSCSTR_BadVarDecl, "CSCSTR_BadVarDecl", 0);
            AddInfo(ResNo.CSCSTR_BadVisBaseClass, "CSCSTR_BadVisBaseClass", 2);
            AddInfo(ResNo.CSCSTR_BadVisBaseInterface, "CSCSTR_BadVisBaseInterface", 2);
            AddInfo(ResNo.CSCSTR_BadVisBound, "CSCSTR_BadVisBound", 2);
            AddInfo(ResNo.CSCSTR_BadVisDelegateParam, "CSCSTR_BadVisDelegateParam", 2);
            AddInfo(ResNo.CSCSTR_BadVisDelegateReturn, "CSCSTR_BadVisDelegateReturn", 2);
            AddInfo(ResNo.CSCSTR_BadVisFieldType, "CSCSTR_BadVisFieldType", 2);
            AddInfo(ResNo.CSCSTR_BadVisIndexerParam, "CSCSTR_BadVisIndexerParam", 2);
            AddInfo(ResNo.CSCSTR_BadVisIndexerReturn, "CSCSTR_BadVisIndexerReturn", 2);
            AddInfo(ResNo.CSCSTR_BadVisOpParam, "CSCSTR_BadVisOpParam", 2);
            AddInfo(ResNo.CSCSTR_BadVisOpReturn, "CSCSTR_BadVisOpReturn", 2);
            AddInfo(ResNo.CSCSTR_BadVisParamType, "CSCSTR_BadVisParamType", 2);
            AddInfo(ResNo.CSCSTR_BadVisPropertyType, "CSCSTR_BadVisPropertyType", 2);
            AddInfo(ResNo.CSCSTR_BadVisReturnType, "CSCSTR_BadVisReturnType", 2);
            AddInfo(ResNo.CSCSTR_BadWarningLevel, "CSCSTR_BadWarningLevel", 0);
            AddInfo(ResNo.CSCSTR_BadWarningNumber, "CSCSTR_BadWarningNumber", 1);
            AddInfo(ResNo.CSCSTR_BadWatsonMode, "CSCSTR_BadWatsonMode", 1);
            AddInfo(ResNo.CSCSTR_BadWin32Res, "CSCSTR_BadWin32Res", 1);
            AddInfo(ResNo.CSCSTR_BadXMLRef, "CSCSTR_BadXMLRef", 2);
            AddInfo(ResNo.CSCSTR_BadXMLRefParamType, "CSCSTR_BadXMLRefParamType", 2);
            AddInfo(ResNo.CSCSTR_BadXMLRefReturnType, "CSCSTR_BadXMLRefReturnType", 0);
            AddInfo(ResNo.CSCSTR_BadXMLRefSyntax, "CSCSTR_BadXMLRefSyntax", 2);
            AddInfo(ResNo.CSCSTR_BadXMLRefTypeVar, "CSCSTR_BadXMLRefTypeVar", 2);
            AddInfo(ResNo.CSCSTR_BadYieldInCatch, "CSCSTR_BadYieldInCatch", 0);
            AddInfo(ResNo.CSCSTR_BadYieldInFinally, "CSCSTR_BadYieldInFinally", 0);
            AddInfo(ResNo.CSCSTR_BadYieldInTryOfCatch, "CSCSTR_BadYieldInTryOfCatch", 0);
            AddInfo(ResNo.CSCSTR_BANNER1, "CSCSTR_BANNER1", 0);
            AddInfo(ResNo.CSCSTR_BANNER1PART2, "CSCSTR_BANNER1PART2", 0);
            AddInfo(ResNo.CSCSTR_BANNER2, "CSCSTR_BANNER2", 0);
            AddInfo(ResNo.CSCSTR_BaseClassMustBeFirst, "CSCSTR_BaseClassMustBeFirst", 1);
            AddInfo(ResNo.CSCSTR_BaseConstraintConflict, "CSCSTR_BaseConstraintConflict", 3);
            AddInfo(ResNo.CSCSTR_BaseIllegal, "CSCSTR_BaseIllegal", 0);
            AddInfo(ResNo.CSCSTR_BaseInBadContext, "CSCSTR_BaseInBadContext", 0);
            AddInfo(ResNo.CSCSTR_BaseInStaticMeth, "CSCSTR_BaseInStaticMeth", 0);
            AddInfo(ResNo.CSCSTR_BINARYFILE, "CSCSTR_BINARYFILE", 1);
            AddInfo(ResNo.CSCSTR_BindToBogus, "CSCSTR_BindToBogus", 1);
            AddInfo(ResNo.CSCSTR_BindToBogusProp1, "CSCSTR_BindToBogusProp1", 2);
            AddInfo(ResNo.CSCSTR_BindToBogusProp2, "CSCSTR_BindToBogusProp2", 3);
            AddInfo(ResNo.CSCSTR_BitwiseOrSignExtend, "CSCSTR_BitwiseOrSignExtend", 0);
            AddInfo(ResNo.CSCSTR_BogusExplicitImpl, "CSCSTR_BogusExplicitImpl", 2);
            AddInfo(ResNo.CSCSTR_BogusType, "CSCSTR_BogusType", 1);
            AddInfo(ResNo.CSCSTR_BUGREPORTWARN, "CSCSTR_BUGREPORTWARN", 0);
            AddInfo(ResNo.CSCSTR_ByRefNonAgileField, "CSCSTR_ByRefNonAgileField", 1);
            AddInfo(ResNo.CSCSTR_CallingBaseFinalizeDeprecated, "CSCSTR_CallingBaseFinalizeDeprecated", 0);
            AddInfo(ResNo.CSCSTR_CallingFinalizeDepracated, "CSCSTR_CallingFinalizeDepracated", 0);
            AddInfo(ResNo.CSCSTR_CallOnNonAgileField, "CSCSTR_CallOnNonAgileField", 1);
            AddInfo(ResNo.CSCSTR_CantCallSpecialMethod, "CSCSTR_CantCallSpecialMethod", 1);
            AddInfo(ResNo.CSCSTR_CantChangeAccessOnOverride, "CSCSTR_CantChangeAccessOnOverride", 3);
            AddInfo(ResNo.CSCSTR_CantChangeReturnTypeOnOverride, "CSCSTR_CantChangeReturnTypeOnOverride", 3);
            AddInfo(ResNo.CSCSTR_CantChangeTypeOnOverride, "CSCSTR_CantChangeTypeOnOverride", 3);
            AddInfo(ResNo.CSCSTR_CantConvAnonMethNoParams, "CSCSTR_CantConvAnonMethNoParams", 1);
            AddInfo(ResNo.CSCSTR_CantConvAnonMethParams, "CSCSTR_CantConvAnonMethParams", 1);
            AddInfo(ResNo.CSCSTR_CantConvAnonMethReturns, "CSCSTR_CantConvAnonMethReturns", 2);
            AddInfo(ResNo.CSCSTR_CantDeriveFromSealedType, "CSCSTR_CantDeriveFromSealedType", 2);
            AddInfo(ResNo.CSCSTR_CantGetCORSystemDir, "CSCSTR_CantGetCORSystemDir", 1);
            AddInfo(ResNo.CSCSTR_CantHaveWin32ResAndIcon, "CSCSTR_CantHaveWin32ResAndIcon", 0);
            AddInfo(ResNo.CSCSTR_CantImportBase, "CSCSTR_CantImportBase", 3);
            AddInfo(ResNo.CSCSTR_CantIncludeDirectory, "CSCSTR_CantIncludeDirectory", 1);
            AddInfo(ResNo.CSCSTR_CantInferMethTypeArgs, "CSCSTR_CantInferMethTypeArgs", 1);
            AddInfo(ResNo.CSCSTR_CantMakeTempFile, "CSCSTR_CantMakeTempFile", 2);
            AddInfo(ResNo.CSCSTR_CANTOPENFILEWRITE, "CSCSTR_CANTOPENFILEWRITE", 1);
            AddInfo(ResNo.CSCSTR_CantOpenWin32Res, "CSCSTR_CantOpenWin32Res", 2);
            AddInfo(ResNo.CSCSTR_CantOverrideBogusMethod, "CSCSTR_CantOverrideBogusMethod", 2);
            AddInfo(ResNo.CSCSTR_CantOverrideNonEvent, "CSCSTR_CantOverrideNonEvent", 2);
            AddInfo(ResNo.CSCSTR_CantOverrideNonFunction, "CSCSTR_CantOverrideNonFunction", 2);
            AddInfo(ResNo.CSCSTR_CantOverrideNonProperty, "CSCSTR_CantOverrideNonProperty", 2);
            AddInfo(ResNo.CSCSTR_CantOverrideNonVirtual, "CSCSTR_CantOverrideNonVirtual", 2);
            AddInfo(ResNo.CSCSTR_CantOverrideSealed, "CSCSTR_CantOverrideSealed", 2);
            AddInfo(ResNo.CSCSTR_CantReadResource, "CSCSTR_CantReadResource", 2);
            AddInfo(ResNo.CSCSTR_CantRefResource, "CSCSTR_CantRefResource", 1);
            AddInfo(ResNo.CSCSTR_CantUseRequiredAttribute, "CSCSTR_CantUseRequiredAttribute", 0);
            AddInfo(ResNo.CSCSTR_CheckedOverflow, "CSCSTR_CheckedOverflow", 0);
            AddInfo(ResNo.CSCSTR_CHILD, "CSCSTR_CHILD", 0);
            AddInfo(ResNo.CSCSTR_CircConstValue, "CSCSTR_CircConstValue", 1);
            AddInfo(ResNo.CSCSTR_CircularBase, "CSCSTR_CircularBase", 2);
            AddInfo(ResNo.CSCSTR_CircularConstraint, "CSCSTR_CircularConstraint", 2);
            AddInfo(ResNo.CSCSTR_ClassBoundNotFirst, "CSCSTR_ClassBoundNotFirst", 1);
            AddInfo(ResNo.CSCSTR_ClassDoesntImplementInterface, "CSCSTR_ClassDoesntImplementInterface", 2);
            AddInfo(ResNo.CSCSTR_ClassNameTooLong, "CSCSTR_ClassNameTooLong", 1);
            AddInfo(ResNo.CSCSTR_ClassTypeExpected, "CSCSTR_ClassTypeExpected", 0);
            AddInfo(ResNo.CSCSTR_CLB_ERROR_FIRST, "CSCSTR_CLB_ERROR_FIRST", 0);
            AddInfo(ResNo.CSCSTR_CloseParenExpected, "CSCSTR_CloseParenExpected", 0);
            AddInfo(ResNo.CSCSTR_CloseUnimplementedInterfaceMember, "CSCSTR_CloseUnimplementedInterfaceMember", 3);
            AddInfo(ResNo.CSCSTR_CLS_ArrayArgumentToAttribute, "CSCSTR_CLS_ArrayArgumentToAttribute", 0);
            AddInfo(ResNo.CSCSTR_CLS_AssemblyNotCLS, "CSCSTR_CLS_AssemblyNotCLS", 1);
            AddInfo(ResNo.CSCSTR_CLS_AssemblyNotCLS2, "CSCSTR_CLS_AssemblyNotCLS2", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadArgType, "CSCSTR_CLS_BadArgType", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadAttributeType, "CSCSTR_CLS_BadAttributeType", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadBase, "CSCSTR_CLS_BadBase", 2);
            AddInfo(ResNo.CSCSTR_CLS_BadFieldPropType, "CSCSTR_CLS_BadFieldPropType", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadIdentifier, "CSCSTR_CLS_BadIdentifier", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadIdentifierCase, "CSCSTR_CLS_BadIdentifierCase", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadInterface, "CSCSTR_CLS_BadInterface", 2);
            AddInfo(ResNo.CSCSTR_CLS_BadInterfacemember, "CSCSTR_CLS_BadInterfacemember", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadReturnType, "CSCSTR_CLS_BadReturnType", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadTypeVar, "CSCSTR_CLS_BadTypeVar", 1);
            AddInfo(ResNo.CSCSTR_CLS_BadUnicode, "CSCSTR_CLS_BadUnicode", 0);
            AddInfo(ResNo.CSCSTR_CLS_IllegalTrueInFalse, "CSCSTR_CLS_IllegalTrueInFalse", 2);
            AddInfo(ResNo.CSCSTR_CLS_MeaninglessOnParam, "CSCSTR_CLS_MeaninglessOnParam", 0);
            AddInfo(ResNo.CSCSTR_CLS_MeaninglessOnPrivateType, "CSCSTR_CLS_MeaninglessOnPrivateType", 1);
            AddInfo(ResNo.CSCSTR_CLS_MeaninglessOnReturn, "CSCSTR_CLS_MeaninglessOnReturn", 0);
            AddInfo(ResNo.CSCSTR_CLS_ModuleMissingCLS, "CSCSTR_CLS_ModuleMissingCLS", 0);
            AddInfo(ResNo.CSCSTR_CLS_NoAbstractMembers, "CSCSTR_CLS_NoAbstractMembers", 1);
            AddInfo(ResNo.CSCSTR_CLS_NotOnModules, "CSCSTR_CLS_NotOnModules", 0);
            AddInfo(ResNo.CSCSTR_CLS_NotOnModules2, "CSCSTR_CLS_NotOnModules2", 0);
            AddInfo(ResNo.CSCSTR_CLS_NoVarArgs, "CSCSTR_CLS_NoVarArgs", 0);
            AddInfo(ResNo.CSCSTR_CLS_OverloadRefOut, "CSCSTR_CLS_OverloadRefOut", 1);
            AddInfo(ResNo.CSCSTR_CLS_OverloadUnnamed, "CSCSTR_CLS_OverloadUnnamed", 1);
            AddInfo(ResNo.CSCSTR_CLS_VolatileField, "CSCSTR_CLS_VolatileField", 1);
            AddInfo(ResNo.CSCSTR_CmdOptionConflictsSource, "CSCSTR_CmdOptionConflictsSource", 2);
            AddInfo(ResNo.CSCSTR_CmpAlwaysFalse, "CSCSTR_CmpAlwaysFalse", 1);
            AddInfo(ResNo.CSCSTR_CoClassWithoutComImport, "CSCSTR_CoClassWithoutComImport", 1);
            AddInfo(ResNo.CSCSTR_ColColWithTypeAlias, "CSCSTR_ColColWithTypeAlias", 1);
            AddInfo(ResNo.CSCSTR_Collection, "CSCSTR_Collection", 0);
            AddInfo(ResNo.CSCSTR_ComImportWithBase, "CSCSTR_ComImportWithBase", 1);
            AddInfo(ResNo.CSCSTR_ComImportWithImpl, "CSCSTR_ComImportWithImpl", 2);
            AddInfo(ResNo.CSCSTR_ComImportWithoutUuidAttribute, "CSCSTR_ComImportWithoutUuidAttribute", 0);
            AddInfo(ResNo.CSCSTR_ComImportWithUserCtor, "CSCSTR_ComImportWithUserCtor", 0);
            AddInfo(ResNo.CSCSTR_ComparisonToSelf, "CSCSTR_ComparisonToSelf", 0);
            AddInfo(ResNo.CSCSTR_CompileCancelled, "CSCSTR_CompileCancelled", 0);
            AddInfo(ResNo.CSCSTR_ComPlusInit, "CSCSTR_ComPlusInit", 1);
            AddInfo(ResNo.CSCSTR_ConcreteMissingBody, "CSCSTR_ConcreteMissingBody", 1);
            AddInfo(ResNo.CSCSTR_ConditionalMustReturnVoid, "CSCSTR_ConditionalMustReturnVoid", 1);
            AddInfo(ResNo.CSCSTR_ConditionalOnInterfaceMethod, "CSCSTR_ConditionalOnInterfaceMethod", 0);
            AddInfo(ResNo.CSCSTR_ConditionalOnNonAttributeClass, "CSCSTR_ConditionalOnNonAttributeClass", 1);
            AddInfo(ResNo.CSCSTR_ConditionalOnOverride, "CSCSTR_ConditionalOnOverride", 1);
            AddInfo(ResNo.CSCSTR_ConditionalOnSpecialMethod, "CSCSTR_ConditionalOnSpecialMethod", 1);
            AddInfo(ResNo.CSCSTR_ConditionalWithOutParam, "CSCSTR_ConditionalWithOutParam", 1);
            AddInfo(ResNo.CSCSTR_ConflictAliasAndMember, "CSCSTR_ConflictAliasAndMember", 2);
            AddInfo(ResNo.CSCSTR_ConflictingChecksum, "CSCSTR_ConflictingChecksum", 1);
            AddInfo(ResNo.CSCSTR_ConstantExpected, "CSCSTR_ConstantExpected", 0);
            AddInfo(ResNo.CSCSTR_ConstOutOfRange, "CSCSTR_ConstOutOfRange", 2);
            AddInfo(ResNo.CSCSTR_ConstOutOfRangeChecked, "CSCSTR_ConstOutOfRangeChecked", 2);
            AddInfo(ResNo.CSCSTR_ConstraintIsStaticClass, "CSCSTR_ConstraintIsStaticClass", 1);
            AddInfo(ResNo.CSCSTR_ConstraintOnlyAllowedOnGenericDecl, "CSCSTR_ConstraintOnlyAllowedOnGenericDecl", 0);
            AddInfo(ResNo.CSCSTR_ConstructorInStaticClass, "CSCSTR_ConstructorInStaticClass", 0);
            AddInfo(ResNo.CSCSTR_ConstValueRequired, "CSCSTR_ConstValueRequired", 0);
            AddInfo(ResNo.CSCSTR_ConversionNotInvolvingContainedType, "CSCSTR_ConversionNotInvolvingContainedType", 0);
            AddInfo(ResNo.CSCSTR_ConversionWithBase, "CSCSTR_ConversionWithBase", 1);
            AddInfo(ResNo.CSCSTR_ConversionWithDerived, "CSCSTR_ConversionWithDerived", 1);
            AddInfo(ResNo.CSCSTR_ConversionWithInterface, "CSCSTR_ConversionWithInterface", 1);
            AddInfo(ResNo.CSCSTR_ConvertToStaticClass, "CSCSTR_ConvertToStaticClass", 1);
            AddInfo(ResNo.CSCSTR_ConWithValCon, "CSCSTR_ConWithValCon", 2);
            AddInfo(ResNo.CSCSTR_CryptoFailed, "CSCSTR_CryptoFailed", 2);
            AddInfo(ResNo.CSCSTR_CryptoNotFound, "CSCSTR_CryptoNotFound", 0);
            AddInfo(ResNo.CSCSTR_CStyleArray, "CSCSTR_CStyleArray", 0);
            AddInfo(ResNo.CSCSTR_CustomAttributeError, "CSCSTR_CustomAttributeError", 2);
            AddInfo(ResNo.CSCSTR_CycleInInterfaceInheritance, "CSCSTR_CycleInInterfaceInheritance", 2);
            AddInfo(ResNo.CSCSTR_CycleInTypeForwarder, "CSCSTR_CycleInTypeForwarder", 2);
            AddInfo(ResNo.CSCSTR_DEBUG_CONFLICT, "CSCSTR_DEBUG_CONFLICT", 2);
            AddInfo(ResNo.CSCSTR_DebugEmitFailure, "CSCSTR_DebugEmitFailure", 2);
            AddInfo(ResNo.CSCSTR_DebugInit, "CSCSTR_DebugInit", 1);
            AddInfo(ResNo.CSCSTR_DebugInitFile, "CSCSTR_DebugInitFile", 2);
            AddInfo(ResNo.CSCSTR_DecConstError, "CSCSTR_DecConstError", 1);
            AddInfo(ResNo.CSCSTR_DefaultMemberOnIndexedType, "CSCSTR_DefaultMemberOnIndexedType", 0);
            AddInfo(ResNo.CSCSTR_DefaultValueBadParamType, "CSCSTR_DefaultValueBadParamType", 1);
            AddInfo(ResNo.CSCSTR_DefaultValueBadValueType, "CSCSTR_DefaultValueBadValueType", 1);
            AddInfo(ResNo.CSCSTR_DefaultValueTypeMustMatch, "CSCSTR_DefaultValueTypeMustMatch", 0);
            AddInfo(ResNo.CSCSTR_DefineIdentifierRequired, "CSCSTR_DefineIdentifierRequired", 1);
            AddInfo(ResNo.CSCSTR_DelegateNewMethBind, "CSCSTR_DelegateNewMethBind", 3);
            AddInfo(ResNo.CSCSTR_DelegateOnConditional, "CSCSTR_DelegateOnConditional", 1);
            AddInfo(ResNo.CSCSTR_DelegateOnNullable, "CSCSTR_DelegateOnNullable", 1);
            AddInfo(ResNo.CSCSTR_DeleteAutoResFailed, "CSCSTR_DeleteAutoResFailed", 2);
            AddInfo(ResNo.CSCSTR_DeprecatedSymbol, "CSCSTR_DeprecatedSymbol", 1);
            AddInfo(ResNo.CSCSTR_DeprecatedSymbolStr, "CSCSTR_DeprecatedSymbolStr", 2);
            AddInfo(ResNo.CSCSTR_DeriveFromEnumOrValueType, "CSCSTR_DeriveFromEnumOrValueType", 2);
            AddInfo(ResNo.CSCSTR_DerivingFromATyVar, "CSCSTR_DerivingFromATyVar", 1);
            AddInfo(ResNo.CSCSTR_DestructorInStaticClass, "CSCSTR_DestructorInStaticClass", 0);
            AddInfo(ResNo.CSCSTR_DllImportOnInvalidMethod, "CSCSTR_DllImportOnInvalidMethod", 0);
            AddInfo(ResNo.CSCSTR_DocFileGen, "CSCSTR_DocFileGen", 2);
            AddInfo(ResNo.CSCSTR_DoNotUseFixedBufferAttr, "CSCSTR_DoNotUseFixedBufferAttr", 0);
            AddInfo(ResNo.CSCSTR_DontUseInvoke, "CSCSTR_DontUseInvoke", 0);
            AddInfo(ResNo.CSCSTR_DotOnDefault, "CSCSTR_DotOnDefault", 1);
            AddInfo(ResNo.CSCSTR_DottedTypeNameNotFoundInAgg, "CSCSTR_DottedTypeNameNotFoundInAgg", 2);
            AddInfo(ResNo.CSCSTR_DottedTypeNameNotFoundInNS, "CSCSTR_DottedTypeNameNotFoundInNS", 2);
            AddInfo(ResNo.CSCSTR_DuplicateAccessor, "CSCSTR_DuplicateAccessor", 0);
            AddInfo(ResNo.CSCSTR_DuplicateAlias, "CSCSTR_DuplicateAlias", 1);
            AddInfo(ResNo.CSCSTR_DuplicateAttribute, "CSCSTR_DuplicateAttribute", 1);
            AddInfo(ResNo.CSCSTR_DuplicateBound, "CSCSTR_DuplicateBound", 2);
            AddInfo(ResNo.CSCSTR_DuplicateCaseLabel, "CSCSTR_DuplicateCaseLabel", 1);
            AddInfo(ResNo.CSCSTR_DuplicateConstraintClause, "CSCSTR_DuplicateConstraintClause", 1);
            AddInfo(ResNo.CSCSTR_DuplicateConversionInClass, "CSCSTR_DuplicateConversionInClass", 1);
            AddInfo(ResNo.CSCSTR_DuplicateImport, "CSCSTR_DuplicateImport", 1);
            AddInfo(ResNo.CSCSTR_DuplicateImportSimple, "CSCSTR_DuplicateImportSimple", 1);
            AddInfo(ResNo.CSCSTR_DuplicateInterfaceInBaseList, "CSCSTR_DuplicateInterfaceInBaseList", 1);
            AddInfo(ResNo.CSCSTR_DuplicateLabel, "CSCSTR_DuplicateLabel", 1);
            AddInfo(ResNo.CSCSTR_DuplicateModifier, "CSCSTR_DuplicateModifier", 1);
            AddInfo(ResNo.CSCSTR_DuplicateNamedAttributeArgument, "CSCSTR_DuplicateNamedAttributeArgument", 1);
            AddInfo(ResNo.CSCSTR_DuplicateNameInClass, "CSCSTR_DuplicateNameInClass", 2);
            AddInfo(ResNo.CSCSTR_DuplicateNameInNS, "CSCSTR_DuplicateNameInNS", 2);
            AddInfo(ResNo.CSCSTR_DuplicateParamName, "CSCSTR_DuplicateParamName", 1);
            AddInfo(ResNo.CSCSTR_DuplicateParamTag, "CSCSTR_DuplicateParamTag", 2);
            AddInfo(ResNo.CSCSTR_DuplicatePropertyAccessMods, "CSCSTR_DuplicatePropertyAccessMods", 1);
            AddInfo(ResNo.CSCSTR_DUPLICATERESPONSEFILE, "CSCSTR_DUPLICATERESPONSEFILE", 1);
            AddInfo(ResNo.CSCSTR_DuplicateTypeParameter, "CSCSTR_DuplicateTypeParameter", 1);
            AddInfo(ResNo.CSCSTR_DuplicateTypeParamTag, "CSCSTR_DuplicateTypeParamTag", 2);
            AddInfo(ResNo.CSCSTR_DuplicateUsing, "CSCSTR_DuplicateUsing", 1);
            AddInfo(ResNo.CSCSTR_EmptyCharConst, "CSCSTR_EmptyCharConst", 0);
            AddInfo(ResNo.CSCSTR_EmptyFileName, "CSCSTR_EmptyFileName", 0);
            AddInfo(ResNo.CSCSTR_EmptySwitch, "CSCSTR_EmptySwitch", 0);
            AddInfo(ResNo.CSCSTR_EmptyYield, "CSCSTR_EmptyYield", 0);
            AddInfo(ResNo.CSCSTR_EndifDirectiveExpected, "CSCSTR_EndifDirectiveExpected", 0);
            AddInfo(ResNo.CSCSTR_EndOfPPLineExpected, "CSCSTR_EndOfPPLineExpected", 0);
            AddInfo(ResNo.CSCSTR_EndRegionDirectiveExpected, "CSCSTR_EndRegionDirectiveExpected", 0);
            AddInfo(ResNo.CSCSTR_ENTERCORRECT, "CSCSTR_ENTERCORRECT", 0);
            AddInfo(ResNo.CSCSTR_ENTERDESC, "CSCSTR_ENTERDESC", 0);
            AddInfo(ResNo.CSCSTR_EnumeratorOverflow, "CSCSTR_EnumeratorOverflow", 1);
            AddInfo(ResNo.CSCSTR_EOFExpected, "CSCSTR_EOFExpected", 0);
            AddInfo(ResNo.CSCSTR_EqualityOpWithoutEquals, "CSCSTR_EqualityOpWithoutEquals", 1);
            AddInfo(ResNo.CSCSTR_EqualityOpWithoutGetHashCode, "CSCSTR_EqualityOpWithoutGetHashCode", 1);
            AddInfo(ResNo.CSCSTR_EqualsWithoutGetHashCode, "CSCSTR_EqualsWithoutGetHashCode", 1);
            AddInfo(ResNo.CSCSTR_ErrorDirective, "CSCSTR_ErrorDirective", 1);
            AddInfo(ResNo.CSCSTR_ErrorOverride, "CSCSTR_ErrorOverride", 2);
            AddInfo(ResNo.CSCSTR_ERRORSYM, "CSCSTR_ERRORSYM", 0);
            AddInfo(ResNo.CSCSTR_EventNeedsBothAccessors, "CSCSTR_EventNeedsBothAccessors", 1);
            AddInfo(ResNo.CSCSTR_EventNotDelegate, "CSCSTR_EventNotDelegate", 1);
            AddInfo(ResNo.CSCSTR_EventPropertyInInterface, "CSCSTR_EventPropertyInInterface", 0);
            AddInfo(ResNo.CSCSTR_ExpectedDotOrParen, "CSCSTR_ExpectedDotOrParen", 0);
            AddInfo(ResNo.CSCSTR_ExpectedEndTry, "CSCSTR_ExpectedEndTry", 0);
            AddInfo(ResNo.CSCSTR_ExpectedVerbatimLiteral, "CSCSTR_ExpectedVerbatimLiteral", 0);
            AddInfo(ResNo.CSCSTR_ExplicitEventFieldImpl, "CSCSTR_ExplicitEventFieldImpl", 0);
            AddInfo(ResNo.CSCSTR_ExplicitImplParams, "CSCSTR_ExplicitImplParams", 2);
            AddInfo(ResNo.CSCSTR_ExplicitInterfaceImplementationInNonClassOrStruct, "CSCSTR_ExplicitInterfaceImplementationInNonClassOrStruct", 1);
            AddInfo(ResNo.CSCSTR_ExplicitInterfaceImplementationNotInterface, "CSCSTR_ExplicitInterfaceImplementationNotInterface", 1);
            AddInfo(ResNo.CSCSTR_ExplicitMethodImplAccessor, "CSCSTR_ExplicitMethodImplAccessor", 2);
            AddInfo(ResNo.CSCSTR_ExplicitParamArray, "CSCSTR_ExplicitParamArray", 0);
            AddInfo(ResNo.CSCSTR_ExplicitPropertyAddingAccessor, "CSCSTR_ExplicitPropertyAddingAccessor", 2);
            AddInfo(ResNo.CSCSTR_ExplicitPropertyMissingAccessor, "CSCSTR_ExplicitPropertyMissingAccessor", 2);
            AddInfo(ResNo.CSCSTR_ExternAfterElements, "CSCSTR_ExternAfterElements", 0);
            AddInfo(ResNo.CSCSTR_ExternHasBody, "CSCSTR_ExternHasBody", 1);
            AddInfo(ResNo.CSCSTR_ExternMethodNoImplementation, "CSCSTR_ExternMethodNoImplementation", 1);
            AddInfo(ResNo.CSCSTR_FailedInclude, "CSCSTR_FailedInclude", 3);
            AddInfo(ResNo.CSCSTR_FeatureAnonDelegates, "CSCSTR_FeatureAnonDelegates", 0);
            AddInfo(ResNo.CSCSTR_FeatureDefault, "CSCSTR_FeatureDefault", 0);
            AddInfo(ResNo.CSCSTR_FeatureDeprecated, "CSCSTR_FeatureDeprecated", 2);
            AddInfo(ResNo.CSCSTR_FeatureExternAlias, "CSCSTR_FeatureExternAlias", 0);
            AddInfo(ResNo.CSCSTR_FeatureFixedBuffer, "CSCSTR_FeatureFixedBuffer", 0);
            AddInfo(ResNo.CSCSTR_FeatureGenerics, "CSCSTR_FeatureGenerics", 0);
            AddInfo(ResNo.CSCSTR_FeatureGlobalNamespace, "CSCSTR_FeatureGlobalNamespace", 0);
            AddInfo(ResNo.CSCSTR_FeatureIterators, "CSCSTR_FeatureIterators", 0);
            AddInfo(ResNo.CSCSTR_FeatureModuleAttrLoc, "CSCSTR_FeatureModuleAttrLoc", 0);
            AddInfo(ResNo.CSCSTR_FeatureNullable, "CSCSTR_FeatureNullable", 0);
            AddInfo(ResNo.CSCSTR_FeatureNYI, "CSCSTR_FeatureNYI", 1);
            AddInfo(ResNo.CSCSTR_FeatureNYI2, "CSCSTR_FeatureNYI2", 1);
            AddInfo(ResNo.CSCSTR_FeaturePartialTypes, "CSCSTR_FeaturePartialTypes", 0);
            AddInfo(ResNo.CSCSTR_FeaturePragma, "CSCSTR_FeaturePragma", 0);
            AddInfo(ResNo.CSCSTR_FeaturePropertyAccessorMods, "CSCSTR_FeaturePropertyAccessorMods", 0);
            AddInfo(ResNo.CSCSTR_FeatureStaticClasses, "CSCSTR_FeatureStaticClasses", 0);
            AddInfo(ResNo.CSCSTR_FeatureSwitchOnBool, "CSCSTR_FeatureSwitchOnBool", 0);
            AddInfo(ResNo.CSCSTR_FieldCantBeRefAny, "CSCSTR_FieldCantBeRefAny", 1);
            AddInfo(ResNo.CSCSTR_FieldCantHaveVoidType, "CSCSTR_FieldCantHaveVoidType", 0);
            AddInfo(ResNo.CSCSTR_FieldInitializerInStruct, "CSCSTR_FieldInitializerInStruct", 1);
            AddInfo(ResNo.CSCSTR_FieldInitRefNonstatic, "CSCSTR_FieldInitRefNonstatic", 1);
            AddInfo(ResNo.CSCSTR_FILEALREADYINCLUDED, "CSCSTR_FILEALREADYINCLUDED", 1);
            AddInfo(ResNo.CSCSTR_FileNameTooLong, "CSCSTR_FileNameTooLong", 0);
            AddInfo(ResNo.CSCSTR_FILENOTFOUND, "CSCSTR_FILENOTFOUND", 1);
            AddInfo(ResNo.CSCSTR_FinalizeMethod, "CSCSTR_FinalizeMethod", 0);
            AddInfo(ResNo.CSCSTR_FixedBufferNotFixed, "CSCSTR_FixedBufferNotFixed", 0);
            AddInfo(ResNo.CSCSTR_FixedDimsRequired, "CSCSTR_FixedDimsRequired", 0);
            AddInfo(ResNo.CSCSTR_FIXEDLOCAL, "CSCSTR_FIXEDLOCAL", 0);
            AddInfo(ResNo.CSCSTR_FixedMustInit, "CSCSTR_FixedMustInit", 0);
            AddInfo(ResNo.CSCSTR_FixedNeeded, "CSCSTR_FixedNeeded", 0);
            AddInfo(ResNo.CSCSTR_FixedNeedsLvalue, "CSCSTR_FixedNeedsLvalue", 0);
            AddInfo(ResNo.CSCSTR_FixedNotInStruct, "CSCSTR_FixedNotInStruct", 0);
            AddInfo(ResNo.CSCSTR_FixedNotNeeded, "CSCSTR_FixedNotNeeded", 0);
            AddInfo(ResNo.CSCSTR_FixedOverflow, "CSCSTR_FixedOverflow", 2);
            AddInfo(ResNo.CSCSTR_FloatOverflow, "CSCSTR_FloatOverflow", 1);
            AddInfo(ResNo.CSCSTR_FOREACHLOCAL, "CSCSTR_FOREACHLOCAL", 0);
            AddInfo(ResNo.CSCSTR_ForEachMissingMember, "CSCSTR_ForEachMissingMember", 3);
            AddInfo(ResNo.CSCSTR_ForwardedTypeInThisAssembly, "CSCSTR_ForwardedTypeInThisAssembly", 1);
            AddInfo(ResNo.CSCSTR_ForwardedTypeIsNested, "CSCSTR_ForwardedTypeIsNested", 2);
            AddInfo(ResNo.CSCSTR_FriendAssemblyBadArgs, "CSCSTR_FriendAssemblyBadArgs", 1);
            AddInfo(ResNo.CSCSTR_FriendAssemblySNReq, "CSCSTR_FriendAssemblySNReq", 1);
            AddInfo(ResNo.CSCSTR_FriendRefNotEqualToThis, "CSCSTR_FriendRefNotEqualToThis", 2);
            AddInfo(ResNo.CSCSTR_FwdedGeneric, "CSCSTR_FwdedGeneric", 1);
            AddInfo(ResNo.CSCSTR_GenericArgIsStaticClass, "CSCSTR_GenericArgIsStaticClass", 1);
            AddInfo(ResNo.CSCSTR_GenericConstraintNotSatisfied, "CSCSTR_GenericConstraintNotSatisfied", 4);
            AddInfo(ResNo.CSCSTR_GenericDerivingFromAttribute, "CSCSTR_GenericDerivingFromAttribute", 1);
            AddInfo(ResNo.CSCSTR_GetOrSetExpected, "CSCSTR_GetOrSetExpected", 0);
            AddInfo(ResNo.CSCSTR_GlobalAliasDefn, "CSCSTR_GlobalAliasDefn", 0);
            AddInfo(ResNo.CSCSTR_GlobalExternAlias, "CSCSTR_GlobalExternAlias", 0);
            AddInfo(ResNo.CSCSTR_GlobalNamespace, "CSCSTR_GlobalNamespace", 0);
            AddInfo(ResNo.CSCSTR_GlobalSingleTypeNameNotFound, "CSCSTR_GlobalSingleTypeNameNotFound", 1);
            AddInfo(ResNo.CSCSTR_GotoCaseShouldConvert, "CSCSTR_GotoCaseShouldConvert", 1);
            AddInfo(ResNo.CSCSTR_HasNoTypeVars, "CSCSTR_HasNoTypeVars", 2);
            AddInfo(ResNo.CSCSTR_HELP10, "CSCSTR_HELP10", 0);
            AddInfo(ResNo.CSCSTR_HidingAbstractMethod, "CSCSTR_HidingAbstractMethod", 2);
            AddInfo(ResNo.CSCSTR_ICE_Culprit, "CSCSTR_ICE_Culprit", 3);
            AddInfo(ResNo.CSCSTR_ICE_File, "CSCSTR_ICE_File", 1);
            AddInfo(ResNo.CSCSTR_ICE_Lexer, "CSCSTR_ICE_Lexer", 0);
            AddInfo(ResNo.CSCSTR_ICE_Node, "CSCSTR_ICE_Node", 1);
            AddInfo(ResNo.CSCSTR_ICE_Parser, "CSCSTR_ICE_Parser", 0);
            AddInfo(ResNo.CSCSTR_ICE_Stage, "CSCSTR_ICE_Stage", 1);
            AddInfo(ResNo.CSCSTR_ICE_Symbol, "CSCSTR_ICE_Symbol", 2);
            AddInfo(ResNo.CSCSTR_IdentifierExpected, "CSCSTR_IdentifierExpected", 0);
            AddInfo(ResNo.CSCSTR_IdentifierExpectedKW, "CSCSTR_IdentifierExpectedKW", 2);
            AddInfo(ResNo.CSCSTR_IdentifierTooLong, "CSCSTR_IdentifierTooLong", 0);
            AddInfo(ResNo.CSCSTR_IdentityConversion, "CSCSTR_IdentityConversion", 0);
            AddInfo(ResNo.CSCSTR_IllegalArglist, "CSCSTR_IllegalArglist", 0);
            AddInfo(ResNo.CSCSTR_IllegalEscape, "CSCSTR_IllegalEscape", 0);
            AddInfo(ResNo.CSCSTR_IllegalFixedType, "CSCSTR_IllegalFixedType", 0);
            AddInfo(ResNo.CSCSTR_IllegalInnerUnsafe, "CSCSTR_IllegalInnerUnsafe", 0);
            AddInfo(ResNo.CSCSTR_IllegalOptionChar, "CSCSTR_IllegalOptionChar", 1);
            AddInfo(ResNo.CSCSTR_IllegalParams, "CSCSTR_IllegalParams", 0);
            AddInfo(ResNo.CSCSTR_IllegalPPChecksum, "CSCSTR_IllegalPPChecksum", 0);
            AddInfo(ResNo.CSCSTR_IllegalPPWarning, "CSCSTR_IllegalPPWarning", 0);
            AddInfo(ResNo.CSCSTR_IllegalPragma, "CSCSTR_IllegalPragma", 0);
            AddInfo(ResNo.CSCSTR_IllegalRefParam, "CSCSTR_IllegalRefParam", 0);
            AddInfo(ResNo.CSCSTR_IllegalStatement, "CSCSTR_IllegalStatement", 0);
            AddInfo(ResNo.CSCSTR_IllegalUnsafe, "CSCSTR_IllegalUnsafe", 0);
            AddInfo(ResNo.CSCSTR_IllegalVarArgs, "CSCSTR_IllegalVarArgs", 0);
            AddInfo(ResNo.CSCSTR_ImplBadConstraints, "CSCSTR_ImplBadConstraints", 4);
            AddInfo(ResNo.CSCSTR_ImportBadBase, "CSCSTR_ImportBadBase", 1);
            AddInfo(ResNo.CSCSTR_ImportedCircularBase, "CSCSTR_ImportedCircularBase", 1);
            AddInfo(ResNo.CSCSTR_ImportNonAssembly, "CSCSTR_ImportNonAssembly", 1);
            AddInfo(ResNo.CSCSTR_InaccessibleGetter, "CSCSTR_InaccessibleGetter", 1);
            AddInfo(ResNo.CSCSTR_InaccessibleSetter, "CSCSTR_InaccessibleSetter", 1);
            AddInfo(ResNo.CSCSTR_InAttrOnOutParam, "CSCSTR_InAttrOnOutParam", 0);
            AddInfo(ResNo.CSCSTR_InconsistantIndexerNames, "CSCSTR_InconsistantIndexerNames", 0);
            AddInfo(ResNo.CSCSTR_IncorrectBooleanAssg, "CSCSTR_IncorrectBooleanAssg", 0);
            AddInfo(ResNo.CSCSTR_IncrSwitchObsolete, "CSCSTR_IncrSwitchObsolete", 0);
            AddInfo(ResNo.CSCSTR_IndexerCantHaveVoidType, "CSCSTR_IndexerCantHaveVoidType", 0);
            AddInfo(ResNo.CSCSTR_IndexerInStaticClass, "CSCSTR_IndexerInStaticClass", 1);
            AddInfo(ResNo.CSCSTR_IndexerNeedsParam, "CSCSTR_IndexerNeedsParam", 0);
            AddInfo(ResNo.CSCSTR_InExpected, "CSCSTR_InExpected", 0);
            AddInfo(ResNo.CSCSTR_INITERROR, "CSCSTR_INITERROR", 1);
            AddInfo(ResNo.CSCSTR_InputFileNameTooLong, "CSCSTR_InputFileNameTooLong", 1);
            AddInfo(ResNo.CSCSTR_InstanceMemberInStaticClass, "CSCSTR_InstanceMemberInStaticClass", 1);
            AddInfo(ResNo.CSCSTR_InstantiatingStaticClass, "CSCSTR_InstantiatingStaticClass", 1);
            AddInfo(ResNo.CSCSTR_IntDivByZero, "CSCSTR_IntDivByZero", 0);
            AddInfo(ResNo.CSCSTR_IntegralTypeExpected, "CSCSTR_IntegralTypeExpected", 0);
            AddInfo(ResNo.CSCSTR_IntegralTypeValueExpected, "CSCSTR_IntegralTypeValueExpected", 0);
            AddInfo(ResNo.CSCSTR_InterfaceEventInitializer, "CSCSTR_InterfaceEventInitializer", 1);
            AddInfo(ResNo.CSCSTR_InterfaceImplementedByConditional, "CSCSTR_InterfaceImplementedByConditional", 3);
            AddInfo(ResNo.CSCSTR_InterfaceMemberHasBody, "CSCSTR_InterfaceMemberHasBody", 1);
            AddInfo(ResNo.CSCSTR_InterfaceMemberNotFound, "CSCSTR_InterfaceMemberNotFound", 1);
            AddInfo(ResNo.CSCSTR_InterfacesCannotContainTypes, "CSCSTR_InterfacesCannotContainTypes", 1);
            AddInfo(ResNo.CSCSTR_InterfacesCantContainConstructors, "CSCSTR_InterfacesCantContainConstructors", 0);
            AddInfo(ResNo.CSCSTR_InterfacesCantContainFields, "CSCSTR_InterfacesCantContainFields", 0);
            AddInfo(ResNo.CSCSTR_InterfacesCantContainOperators, "CSCSTR_InterfacesCantContainOperators", 0);
            AddInfo(ResNo.CSCSTR_InternalError, "CSCSTR_InternalError", 1);
            AddInfo(ResNo.CSCSTR_InternalVirtual, "CSCSTR_InternalVirtual", 1);
            AddInfo(ResNo.CSCSTR_IntOverflow, "CSCSTR_IntOverflow", 0);
            AddInfo(ResNo.CSCSTR_InvalidAddrOp, "CSCSTR_InvalidAddrOp", 0);
            AddInfo(ResNo.CSCSTR_InvalidArray, "CSCSTR_InvalidArray", 0);
            AddInfo(ResNo.CSCSTR_InvalidAssemblyName, "CSCSTR_InvalidAssemblyName", 1);
            AddInfo(ResNo.CSCSTR_InvalidAttributeArgument, "CSCSTR_InvalidAttributeArgument", 1);
            AddInfo(ResNo.CSCSTR_InvalidAttributeLocation, "CSCSTR_InvalidAttributeLocation", 1);
            AddInfo(ResNo.CSCSTR_InvalidDefaultCharSetValue, "CSCSTR_InvalidDefaultCharSetValue", 0);
            AddInfo(ResNo.CSCSTR_InvalidExprTerm, "CSCSTR_InvalidExprTerm", 1);
            AddInfo(ResNo.CSCSTR_InvalidFixedArraySize, "CSCSTR_InvalidFixedArraySize", 0);
            AddInfo(ResNo.CSCSTR_InvalidFwdType, "CSCSTR_InvalidFwdType", 0);
            AddInfo(ResNo.CSCSTR_InvalidGenericEnum, "CSCSTR_InvalidGenericEnum", 0);
            AddInfo(ResNo.CSCSTR_InvalidGotoCase, "CSCSTR_InvalidGotoCase", 0);
            AddInfo(ResNo.CSCSTR_InvalidInclude, "CSCSTR_InvalidInclude", 1);
            AddInfo(ResNo.CSCSTR_InvalidLineNumber, "CSCSTR_InvalidLineNumber", 0);
            AddInfo(ResNo.CSCSTR_InvalidMainSig, "CSCSTR_InvalidMainSig", 1);
            AddInfo(ResNo.CSCSTR_InvalidMemberDecl, "CSCSTR_InvalidMemberDecl", 1);
            AddInfo(ResNo.CSCSTR_InvalidNamedArgument, "CSCSTR_InvalidNamedArgument", 1);
            AddInfo(ResNo.CSCSTR_InvalidNumber, "CSCSTR_InvalidNumber", 0);
            AddInfo(ResNo.CSCSTR_InvalidPreprocExpr, "CSCSTR_InvalidPreprocExpr", 0);
            AddInfo(ResNo.CSCSTR_InvalidPropertyAccessMod, "CSCSTR_InvalidPropertyAccessMod", 2);
            AddInfo(ResNo.CSCSTR_InvalidQM, "CSCSTR_InvalidQM", 2);
            AddInfo(ResNo.CSCSTR_InvalidSearchPathDir, "CSCSTR_InvalidSearchPathDir", 3);
            AddInfo(ResNo.CSCSTR_InvalidSourceMap, "CSCSTR_InvalidSourceMap", 1);
            AddInfo(ResNo.CSCSTR_INVALIDTARGET, "CSCSTR_INVALIDTARGET", 0);
            AddInfo(ResNo.CSCSTR_IsAlwaysFalse, "CSCSTR_IsAlwaysFalse", 1);
            AddInfo(ResNo.CSCSTR_IsAlwaysTrue, "CSCSTR_IsAlwaysTrue", 1);
            AddInfo(ResNo.CSCSTR_LabelNotFound, "CSCSTR_LabelNotFound", 1);
            AddInfo(ResNo.CSCSTR_LabelShadow, "CSCSTR_LabelShadow", 1);
            AddInfo(ResNo.CSCSTR_LbraceExpected, "CSCSTR_LbraceExpected", 0);
            AddInfo(ResNo.CSCSTR_LIB_ENV, "CSCSTR_LIB_ENV", 0);
            AddInfo(ResNo.CSCSTR_LIB_OPTION, "CSCSTR_LIB_OPTION", 0);
            AddInfo(ResNo.CSCSTR_LineTooLong, "CSCSTR_LineTooLong", 1);
            AddInfo(ResNo.CSCSTR_LinkDemandOnOverride, "CSCSTR_LinkDemandOnOverride", 2);
            AddInfo(ResNo.CSCSTR_LiteralDoubleCast, "CSCSTR_LiteralDoubleCast", 2);
            AddInfo(ResNo.CSCSTR_LocalCantBeFixedAndHoisted, "CSCSTR_LocalCantBeFixedAndHoisted", 1);
            AddInfo(ResNo.CSCSTR_LocalDuplicate, "CSCSTR_LocalDuplicate", 1);
            AddInfo(ResNo.CSCSTR_LocalIllegallyOverrides, "CSCSTR_LocalIllegallyOverrides", 2);
            AddInfo(ResNo.CSCSTR_LocalSameNameAsTypeParam, "CSCSTR_LocalSameNameAsTypeParam", 1);
            AddInfo(ResNo.CSCSTR_LockNeedsReference, "CSCSTR_LockNeedsReference", 1);
            AddInfo(ResNo.CSCSTR_LookupInTypeVariable, "CSCSTR_LookupInTypeVariable", 1);
            AddInfo(ResNo.CSCSTR_LowercaseEllSuffix, "CSCSTR_LowercaseEllSuffix", 0);
            AddInfo(ResNo.CSCSTR_MainCantBeGeneric, "CSCSTR_MainCantBeGeneric", 1);
            AddInfo(ResNo.CSCSTR_MainClassIsImport, "CSCSTR_MainClassIsImport", 1);
            AddInfo(ResNo.CSCSTR_MainClassNotClass, "CSCSTR_MainClassNotClass", 1);
            AddInfo(ResNo.CSCSTR_MainClassNotFound, "CSCSTR_MainClassNotFound", 1);
            AddInfo(ResNo.CSCSTR_MainClassWrongFile, "CSCSTR_MainClassWrongFile", 1);
            AddInfo(ResNo.CSCSTR_MalformedMetadata, "CSCSTR_MalformedMetadata", 1);
            AddInfo(ResNo.CSCSTR_ManagedAddr, "CSCSTR_ManagedAddr", 1);
            AddInfo(ResNo.CSCSTR_MemberAbstractSealed, "CSCSTR_MemberAbstractSealed", 1);
            AddInfo(ResNo.CSCSTR_MemberAlreadyExists, "CSCSTR_MemberAlreadyExists", 2);
            AddInfo(ResNo.CSCSTR_MemberNameSameAsType, "CSCSTR_MemberNameSameAsType", 1);
            AddInfo(ResNo.CSCSTR_MemberNeedsType, "CSCSTR_MemberNeedsType", 0);
            AddInfo(ResNo.CSCSTR_MetadataCantOpenFile, "CSCSTR_MetadataCantOpenFile", 2);
            AddInfo(ResNo.CSCSTR_MetadataEmitFailure, "CSCSTR_MetadataEmitFailure", 2);
            AddInfo(ResNo.CSCSTR_MetadataImportFailure, "CSCSTR_MetadataImportFailure", 2);
            AddInfo(ResNo.CSCSTR_MethDelegateMismatch, "CSCSTR_MethDelegateMismatch", 2);
            AddInfo(ResNo.CSCSTR_MethGrpToNonDel, "CSCSTR_MethGrpToNonDel", 2);
            AddInfo(ResNo.CSCSTR_MethodArgCantBeRefAny, "CSCSTR_MethodArgCantBeRefAny", 1);
            AddInfo(ResNo.CSCSTR_MethodGroup, "CSCSTR_MethodGroup", 0);
            AddInfo(ResNo.CSCSTR_MethodImplementingAccessor, "CSCSTR_MethodImplementingAccessor", 3);
            AddInfo(ResNo.CSCSTR_MethodNameExpected, "CSCSTR_MethodNameExpected", 0);
            AddInfo(ResNo.CSCSTR_MethodReturnCantBeRefAny, "CSCSTR_MethodReturnCantBeRefAny", 1);
            AddInfo(ResNo.CSCSTR_MissingArraySize, "CSCSTR_MissingArraySize", 0);
            AddInfo(ResNo.CSCSTR_MissingCoClass, "CSCSTR_MissingCoClass", 2);
            AddInfo(ResNo.CSCSTR_MissingComTypeOrMarshaller, "CSCSTR_MissingComTypeOrMarshaller", 1);
            AddInfo(ResNo.CSCSTR_MissingOptionArg, "CSCSTR_MissingOptionArg", 1);
            AddInfo(ResNo.CSCSTR_MissingParamTag, "CSCSTR_MissingParamTag", 2);
            AddInfo(ResNo.CSCSTR_MissingPartial, "CSCSTR_MissingPartial", 1);
            AddInfo(ResNo.CSCSTR_MissingPPFile, "CSCSTR_MissingPPFile", 0);
            AddInfo(ResNo.CSCSTR_MissingPredefinedMember, "CSCSTR_MissingPredefinedMember", 2);
            AddInfo(ResNo.CSCSTR_MissingStructOffset, "CSCSTR_MissingStructOffset", 1);
            AddInfo(ResNo.CSCSTR_MissingTypeInAssembly, "CSCSTR_MissingTypeInAssembly", 2);
            AddInfo(ResNo.CSCSTR_MissingTypeInSource, "CSCSTR_MissingTypeInSource", 1);
            AddInfo(ResNo.CSCSTR_MissingTypeNested, "CSCSTR_MissingTypeNested", 2);
            AddInfo(ResNo.CSCSTR_MissingTypeParamTag, "CSCSTR_MissingTypeParamTag", 2);
            AddInfo(ResNo.CSCSTR_MissingXMLComment, "CSCSTR_MissingXMLComment", 1);
            AddInfo(ResNo.CSCSTR_ModuleNotAdded, "CSCSTR_ModuleNotAdded", 2);
            AddInfo(ResNo.CSCSTR_ModuleNotScoped, "CSCSTR_ModuleNotScoped", 2);
            AddInfo(ResNo.CSCSTR_MultipleEntryPoints, "CSCSTR_MultipleEntryPoints", 2);
            AddInfo(ResNo.CSCSTR_MultipleIEnumOfT, "CSCSTR_MultipleIEnumOfT", 2);
            AddInfo(ResNo.CSCSTR_MultiplePredefTypes, "CSCSTR_MultiplePredefTypes", 2);
            AddInfo(ResNo.CSCSTR_MultiTypeInDeclaration, "CSCSTR_MultiTypeInDeclaration", 0);
            AddInfo(ResNo.CSCSTR_MustHaveOpTF, "CSCSTR_MustHaveOpTF", 1);
            AddInfo(ResNo.CSCSTR_NameAttributeOnOverride, "CSCSTR_NameAttributeOnOverride", 0);
            AddInfo(ResNo.CSCSTR_NamedArgumentExpected, "CSCSTR_NamedArgumentExpected", 0);
            AddInfo(ResNo.CSCSTR_NameIllegallyOverrides, "CSCSTR_NameIllegallyOverrides", 2);
            AddInfo(ResNo.CSCSTR_NameNotInContext, "CSCSTR_NameNotInContext", 1);
            AddInfo(ResNo.CSCSTR_NamespaceUnexpected, "CSCSTR_NamespaceUnexpected", 0);
            AddInfo(ResNo.CSCSTR_NegativeArrayIndex, "CSCSTR_NegativeArrayIndex", 0);
            AddInfo(ResNo.CSCSTR_NegativeArraySize, "CSCSTR_NegativeArraySize", 0);
            AddInfo(ResNo.CSCSTR_NegativeStackAllocSize, "CSCSTR_NegativeStackAllocSize", 0);
            AddInfo(ResNo.CSCSTR_NewBoundMustBeLast, "CSCSTR_NewBoundMustBeLast", 0);
            AddInfo(ResNo.CSCSTR_NewBoundWithVal, "CSCSTR_NewBoundWithVal", 0);
            AddInfo(ResNo.CSCSTR_NewConstraintNotSatisfied, "CSCSTR_NewConstraintNotSatisfied", 3);
            AddInfo(ResNo.CSCSTR_NewlineInConst, "CSCSTR_NewlineInConst", 0);
            AddInfo(ResNo.CSCSTR_NewNotRequired, "CSCSTR_NewNotRequired", 1);
            AddInfo(ResNo.CSCSTR_NewOrOverrideExpected, "CSCSTR_NewOrOverrideExpected", 2);
            AddInfo(ResNo.CSCSTR_NewRequired, "CSCSTR_NewRequired", 2);
            AddInfo(ResNo.CSCSTR_NewTyvarWithArgs, "CSCSTR_NewTyvarWithArgs", 1);
            AddInfo(ResNo.CSCSTR_NewVirtualInSealed, "CSCSTR_NewVirtualInSealed", 2);
            AddInfo(ResNo.CSCSTR_NoAccessibleMember, "CSCSTR_NoAccessibleMember", 2);
            AddInfo(ResNo.CSCSTR_NoBaseClass, "CSCSTR_NoBaseClass", 0);
            AddInfo(ResNo.CSCSTR_NoBreakOrCont, "CSCSTR_NoBreakOrCont", 0);
            AddInfo(ResNo.CSCSTR_NoConfigNotOnCommandLine, "CSCSTR_NoConfigNotOnCommandLine", 0);
            AddInfo(ResNo.CSCSTR_NoConstructors, "CSCSTR_NoConstructors", 1);
            AddInfo(ResNo.CSCSTR_NoConvToIDisp, "CSCSTR_NoConvToIDisp", 1);
            AddInfo(ResNo.CSCSTR_NoDebugSwitchSourceMap, "CSCSTR_NoDebugSwitchSourceMap", 0);
            AddInfo(ResNo.CSCSTR_NoDefaultArgs, "CSCSTR_NoDefaultArgs", 0);
            AddInfo(ResNo.CSCSTR_NoEntryPoint, "CSCSTR_NoEntryPoint", 1);
            AddInfo(ResNo.CSCSTR_NoExplicitBuiltinConv, "CSCSTR_NoExplicitBuiltinConv", 2);
            AddInfo(ResNo.CSCSTR_NoExplicitConv, "CSCSTR_NoExplicitConv", 2);
            AddInfo(ResNo.CSCSTR_NOFILESPEC, "CSCSTR_NOFILESPEC", 1);
            AddInfo(ResNo.CSCSTR_NoGetToOverride, "CSCSTR_NoGetToOverride", 2);
            AddInfo(ResNo.CSCSTR_NoImplicitConv, "CSCSTR_NoImplicitConv", 2);
            AddInfo(ResNo.CSCSTR_NoImplicitConvCast, "CSCSTR_NoImplicitConvCast", 2);
            AddInfo(ResNo.CSCSTR_NoInvoke, "CSCSTR_NoInvoke", 1);
            AddInfo(ResNo.CSCSTR_NoMainInClass, "CSCSTR_NoMainInClass", 1);
            AddInfo(ResNo.CSCSTR_NOMAINONDLL, "CSCSTR_NOMAINONDLL", 0);
            AddInfo(ResNo.CSCSTR_NoMemory, "CSCSTR_NoMemory", 0);
            AddInfo(ResNo.CSCSTR_NoMetadataFile, "CSCSTR_NoMetadataFile", 1);
            AddInfo(ResNo.CSCSTR_NoModifiersOnAccessor, "CSCSTR_NoModifiersOnAccessor", 0);
            AddInfo(ResNo.CSCSTR_NoMultipleInheritance, "CSCSTR_NoMultipleInheritance", 3);
            AddInfo(ResNo.CSCSTR_NoNamespacePrivate, "CSCSTR_NoNamespacePrivate", 0);
            AddInfo(ResNo.CSCSTR_NonECMAFeature, "CSCSTR_NonECMAFeature", 1);
            AddInfo(ResNo.CSCSTR_NonECMAFeatureOK, "CSCSTR_NonECMAFeatureOK", 1);
            AddInfo(ResNo.CSCSTR_NoNewAbstract, "CSCSTR_NoNewAbstract", 1);
            AddInfo(ResNo.CSCSTR_NoNewOnNamespaceElement, "CSCSTR_NoNewOnNamespaceElement", 0);
            AddInfo(ResNo.CSCSTR_NoNewTyvar, "CSCSTR_NoNewTyvar", 1);
            AddInfo(ResNo.CSCSTR_NonInterfaceInInterfaceList, "CSCSTR_NonInterfaceInInterfaceList", 1);
            AddInfo(ResNo.CSCSTR_NonObsoleteOverridingObsolete, "CSCSTR_NonObsoleteOverridingObsolete", 2);
            AddInfo(ResNo.CSCSTR_NonVirtualCallFromClosure, "CSCSTR_NonVirtualCallFromClosure", 1);
            AddInfo(ResNo.CSCSTR_NORESPONSEFILE, "CSCSTR_NORESPONSEFILE", 2);
            AddInfo(ResNo.CSCSTR_NoSetToOverride, "CSCSTR_NoSetToOverride", 2);
            AddInfo(ResNo.CSCSTR_NoSourceFile, "CSCSTR_NoSourceFile", 2);
            AddInfo(ResNo.CSCSTR_NoSourceMapFile, "CSCSTR_NoSourceMapFile", 1);
            AddInfo(ResNo.CSCSTR_NOSOURCES, "CSCSTR_NOSOURCES", 0);
            AddInfo(ResNo.CSCSTR_NoSourcesInLastInputSet, "CSCSTR_NoSourcesInLastInputSet", 0);
            AddInfo(ResNo.CSCSTR_NoStdLib, "CSCSTR_NoStdLib", 1);
            AddInfo(ResNo.CSCSTR_NoSuchMember, "CSCSTR_NoSuchMember", 2);
            AddInfo(ResNo.CSCSTR_NotAnAttributeClass, "CSCSTR_NotAnAttributeClass", 1);
            AddInfo(ResNo.CSCSTR_NotConstantExpression, "CSCSTR_NotConstantExpression", 1);
            AddInfo(ResNo.CSCSTR_NotNullConstRefField, "CSCSTR_NotNullConstRefField", 2);
            AddInfo(ResNo.CSCSTR_NoTypeDef, "CSCSTR_NoTypeDef", 2);
            AddInfo(ResNo.CSCSTR_NoVoidHere, "CSCSTR_NoVoidHere", 0);
            AddInfo(ResNo.CSCSTR_NoVoidParameter, "CSCSTR_NoVoidParameter", 0);
            AddInfo(ResNo.CSCSTR_NubExprIsConstBool, "CSCSTR_NubExprIsConstBool", 3);
            AddInfo(ResNo.CSCSTR_NULL, "CSCSTR_NULL", 0);
            AddInfo(ResNo.CSCSTR_NullNotValid, "CSCSTR_NullNotValid", 0);
            AddInfo(ResNo.CSCSTR_ObjectCallingBaseConstructor, "CSCSTR_ObjectCallingBaseConstructor", 1);
            AddInfo(ResNo.CSCSTR_ObjectCantHaveBases, "CSCSTR_ObjectCantHaveBases", 0);
            AddInfo(ResNo.CSCSTR_ObjectProhibited, "CSCSTR_ObjectProhibited", 1);
            AddInfo(ResNo.CSCSTR_ObjectRequired, "CSCSTR_ObjectRequired", 1);
            AddInfo(ResNo.CSCSTR_OD_ALIAS, "CSCSTR_OD_ALIAS", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_ADDR, "CSCSTR_OD_ARG_ADDR", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_ALIAS, "CSCSTR_OD_ARG_ALIAS", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_DEBUGTYPE, "CSCSTR_OD_ARG_DEBUGTYPE", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_FILE, "CSCSTR_OD_ARG_FILE", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_FILELIST, "CSCSTR_OD_ARG_FILELIST", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_NUMBER, "CSCSTR_OD_ARG_NUMBER", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_RESINFO, "CSCSTR_OD_ARG_RESINFO", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_STRING, "CSCSTR_OD_ARG_STRING", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_SYMLIST, "CSCSTR_OD_ARG_SYMLIST", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_TYPE, "CSCSTR_OD_ARG_TYPE", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_WARNLIST, "CSCSTR_OD_ARG_WARNLIST", 0);
            AddInfo(ResNo.CSCSTR_OD_ARG_WILDCARD, "CSCSTR_OD_ARG_WILDCARD", 0);
            AddInfo(ResNo.CSCSTR_OD_BUG, "CSCSTR_OD_BUG", 0);
            AddInfo(ResNo.CSCSTR_OD_EMITDEBUGINFO, "CSCSTR_OD_EMITDEBUGINFO", 0);
            AddInfo(ResNo.CSCSTR_OD_INCBUILD, "CSCSTR_OD_INCBUILD", 0);
            AddInfo(ResNo.CSCSTR_OD_MODULEASSEMBLY, "CSCSTR_OD_MODULEASSEMBLY", 0);
            AddInfo(ResNo.CSCSTR_OD_NOCODEGEN, "CSCSTR_OD_NOCODEGEN", 0);
            AddInfo(ResNo.CSCSTR_OD_TIMING, "CSCSTR_OD_TIMING", 0);
            AddInfo(ResNo.CSCSTR_OD_WATSONMODE, "CSCSTR_OD_WATSONMODE", 0);
            AddInfo(ResNo.CSCSTR_OneAliasPerRefernce, "CSCSTR_OneAliasPerRefernce", 0);
            AddInfo(ResNo.CSCSTR_OnlyClassesCanContainDestructors, "CSCSTR_OnlyClassesCanContainDestructors", 0);
            AddInfo(ResNo.CSCSTR_OnlyValidOnCustomMarshaller, "CSCSTR_OnlyValidOnCustomMarshaller", 1);
            AddInfo(ResNo.CSCSTR_OpenEndedComment, "CSCSTR_OpenEndedComment", 0);
            AddInfo(ResNo.CSCSTR_OperatorCantReturnVoid, "CSCSTR_OperatorCantReturnVoid", 0);
            AddInfo(ResNo.CSCSTR_OperatorInStaticClass, "CSCSTR_OperatorInStaticClass", 1);
            AddInfo(ResNo.CSCSTR_OperatorNeedsMatch, "CSCSTR_OperatorNeedsMatch", 2);
            AddInfo(ResNo.CSCSTR_OperatorsMustBeStatic, "CSCSTR_OperatorsMustBeStatic", 1);
            AddInfo(ResNo.CSCSTR_OPT_NOTIMPLEMENTED, "CSCSTR_OPT_NOTIMPLEMENTED", 1);
            AddInfo(ResNo.CSCSTR_OPTDSC_ADDMODULE, "CSCSTR_OPTDSC_ADDMODULE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_ALIGN, "CSCSTR_OPTDSC_ALIGN", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_ANALYZER, "CSCSTR_OPTDSC_ANALYZER", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_APPCONFIGR, "CSCSTR_OPTDSC_APPCONFIGR", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_BASEADDRESS, "CSCSTR_OPTDSC_BASEADDRESS", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_BUGREPORT, "CSCSTR_OPTDSC_BUGREPORT", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_CHECKED, "CSCSTR_OPTDSC_CHECKED", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_CHECKSUMALGORITHM, "CSCSTR_OPTDSC_CHECKSUMALGORITHM", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_CODEPAGE, "CSCSTR_OPTDSC_CODEPAGE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_COMPATIBILITY, "CSCSTR_OPTDSC_COMPATIBILITY", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_DEBUG, "CSCSTR_OPTDSC_DEBUG", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_DEBUGTYPE, "CSCSTR_OPTDSC_DEBUGTYPE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_DEFINECCSYMBOLS, "CSCSTR_OPTDSC_DEFINECCSYMBOLS", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_DELAYSIGN, "CSCSTR_OPTDSC_DELAYSIGN", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_EMBEDRESOURCE, "CSCSTR_OPTDSC_EMBEDRESOURCE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_ERRORREPORT, "CSCSTR_OPTDSC_ERRORREPORT", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_FULLPATH, "CSCSTR_OPTDSC_FULLPATH", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_HELP, "CSCSTR_OPTDSC_HELP", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_HIGHENTROPYVA, "CSCSTR_OPTDSC_HIGHENTROPYVA", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_KEYCONTAINER, "CSCSTR_OPTDSC_KEYCONTAINER", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_KEYFILE, "CSCSTR_OPTDSC_KEYFILE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_LIBPATH, "CSCSTR_OPTDSC_LIBPATH", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_LINK, "CSCSTR_OPTDSC_LINK", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_LINKRESOURCE, "CSCSTR_OPTDSC_LINKRESOURCE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_MAIN, "CSCSTR_OPTDSC_MAIN", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_MODULEASSEMBLYNAME, "CSCSTR_OPTDSC_MODULEASSEMBLYNAME", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_MODULENAME, "CSCSTR_OPTDSC_MODULENAME", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_NOCONFIG, "CSCSTR_OPTDSC_NOCONFIG", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_NOLOGO, "CSCSTR_OPTDSC_NOLOGO", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_NOSTDLIB, "CSCSTR_OPTDSC_NOSTDLIB", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_NOWARNLIST, "CSCSTR_OPTDSC_NOWARNLIST", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_NOWIN32MANIFEST, "CSCSTR_OPTDSC_NOWIN32MANIFEST", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_OPTIMIZATIONS, "CSCSTR_OPTDSC_OPTIMIZATIONS", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_OUTPUTFILENAME, "CSCSTR_OPTDSC_OUTPUTFILENAME", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_PARALLEL, "CSCSTR_OPTDSC_PARALLEL", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_PDBFILENAME, "CSCSTR_OPTDSC_PDBFILENAME", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_PLATFORM, "CSCSTR_OPTDSC_PLATFORM", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_PREFERREDUILANG, "CSCSTR_OPTDSC_PREFERREDUILANG", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_RECURSE, "CSCSTR_OPTDSC_RECURSE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_REFERENCE, "CSCSTR_OPTDSC_REFERENCE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_RESPONSEFILE, "CSCSTR_OPTDSC_RESPONSEFILE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_RULESET, "CSCSTR_OPTDSC_RULESET", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_SUBSYSTEMVERSION, "CSCSTR_OPTDSC_SUBSYSTEMVERSION", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_TARGET, "CSCSTR_OPTDSC_TARGET", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_TARGET_DLL, "CSCSTR_OPTDSC_TARGET_DLL", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_TARGET_EXE, "CSCSTR_OPTDSC_TARGET_EXE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_TARGET_MODULE, "CSCSTR_OPTDSC_TARGET_MODULE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_TARGET_WINEXE, "CSCSTR_OPTDSC_TARGET_WINEXE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_UNSAFE, "CSCSTR_OPTDSC_UNSAFE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_UTF8OUTPUT, "CSCSTR_OPTDSC_UTF8OUTPUT", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_WARNASERRORLIST, "CSCSTR_OPTDSC_WARNASERRORLIST", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_WARNINGLEVEL, "CSCSTR_OPTDSC_WARNINGLEVEL", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_WARNINGSAREERRORS, "CSCSTR_OPTDSC_WARNINGSAREERRORS", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_WIN32ICON, "CSCSTR_OPTDSC_WIN32ICON", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_WIN32MANIFEST, "CSCSTR_OPTDSC_WIN32MANIFEST", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_WIN32RESOURCE, "CSCSTR_OPTDSC_WIN32RESOURCE", 0);
            AddInfo(ResNo.CSCSTR_OPTDSC_XML_DOCFILE, "CSCSTR_OPTDSC_XML_DOCFILE", 0);
            AddInfo(ResNo.CSCSTR_OpTFRetType, "CSCSTR_OpTFRetType", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_ADVANCED, "CSCSTR_OPTGRP_ADVANCED", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_ASSEMBLIES, "CSCSTR_OPTGRP_ASSEMBLIES", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_CODEGENERATION, "CSCSTR_OPTGRP_CODEGENERATION", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_DEBUGGING, "CSCSTR_OPTGRP_DEBUGGING", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_ERRORS, "CSCSTR_OPTGRP_ERRORS", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_HELP, "CSCSTR_OPTGRP_HELP", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_INPUT, "CSCSTR_OPTGRP_INPUT", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_LANGUAGE, "CSCSTR_OPTGRP_LANGUAGE", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_MISC, "CSCSTR_OPTGRP_MISC", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_OBSOLETE, "CSCSTR_OPTGRP_OBSOLETE", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_OPTIMIZATION, "CSCSTR_OPTGRP_OPTIMIZATION", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_OUTPUT, "CSCSTR_OPTGRP_OUTPUT", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_PROPRCESSOR, "CSCSTR_OPTGRP_PROPRCESSOR", 0);
            AddInfo(ResNo.CSCSTR_OPTGRP_RESOURCES, "CSCSTR_OPTGRP_RESOURCES", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_ADDITIONALFILE, "CSCSTR_OPTSYN_ADDITIONALFILE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_ADDMODULE, "CSCSTR_OPTSYN_ADDMODULE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_ANALYZER, "CSCSTR_OPTSYN_ANALYZER", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_APPCONFIG, "CSCSTR_OPTSYN_APPCONFIG", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_BASEADDRESS, "CSCSTR_OPTSYN_BASEADDRESS", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_BUGREPORT, "CSCSTR_OPTSYN_BUGREPORT", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_CHECKED, "CSCSTR_OPTSYN_CHECKED", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_CHECKSUMALGORITHM, "CSCSTR_OPTSYN_CHECKSUMALGORITHM", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_CODEPAGE, "CSCSTR_OPTSYN_CODEPAGE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_DEBUG, "CSCSTR_OPTSYN_DEBUG", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_DEBUGTYPE, "CSCSTR_OPTSYN_DEBUGTYPE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_DEFINECCSYMBOLS, "CSCSTR_OPTSYN_DEFINECCSYMBOLS", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_DELAYSIGN, "CSCSTR_OPTSYN_DELAYSIGN", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_EMBEDRESOURCE, "CSCSTR_OPTSYN_EMBEDRESOURCE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_ERRORREPORT, "CSCSTR_OPTSYN_ERRORREPORT", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_FILEALIGN, "CSCSTR_OPTSYN_FILEALIGN", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_FULLPATHS, "CSCSTR_OPTSYN_FULLPATHS", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_HELP, "CSCSTR_OPTSYN_HELP", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_HIGHENTROPYVA, "CSCSTR_OPTSYN_HIGHENTROPYVA", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_KEYCONTAINER, "CSCSTR_OPTSYN_KEYCONTAINER", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_KEYFILE, "CSCSTR_OPTSYN_KEYFILE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_LANGVERSION, "CSCSTR_OPTSYN_LANGVERSION", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_LIBPATH, "CSCSTR_OPTSYN_LIBPATH", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_LINK, "CSCSTR_OPTSYN_LINK", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_LINKRESOURCE, "CSCSTR_OPTSYN_LINKRESOURCE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_MAIN, "CSCSTR_OPTSYN_MAIN", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_MODULEASSEMBLYNAME, "CSCSTR_OPTSYN_MODULEASSEMBLYNAME", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_MODULENAME, "CSCSTR_OPTSYN_MODULENAME", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_NOCONFIG, "CSCSTR_OPTSYN_NOCONFIG", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_NOLOGO, "CSCSTR_OPTSYN_NOLOGO", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_NOSTDLIB, "CSCSTR_OPTSYN_NOSTDLIB", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_NOWARNLIST, "CSCSTR_OPTSYN_NOWARNLIST", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_NOWIN32MANIFEST, "CSCSTR_OPTSYN_NOWIN32MANIFEST", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_OPTIMIZE, "CSCSTR_OPTSYN_OPTIMIZE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_OUTPUTFILENAME, "CSCSTR_OPTSYN_OUTPUTFILENAME", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_PARALLEL, "CSCSTR_OPTSYN_PARALLEL", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_PDBFILENAME, "CSCSTR_OPTSYN_PDBFILENAME", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_PLATFORM, "CSCSTR_OPTSYN_PLATFORM", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_PREFERREDUILANG, "CSCSTR_OPTSYN_PREFERREDUILANG", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_RECURSE, "CSCSTR_OPTSYN_RECURSE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_REFERENCE, "CSCSTR_OPTSYN_REFERENCE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_RESPONSEFILE, "CSCSTR_OPTSYN_RESPONSEFILE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_RULESET, "CSCSTR_OPTSYN_RULESET", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_SUBSYSTEMVERSION, "CSCSTR_OPTSYN_SUBSYSTEMVERSION", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_TARGET, "CSCSTR_OPTSYN_TARGET", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_UNSAFE, "CSCSTR_OPTSYN_UNSAFE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_UTF8OUTPUT, "CSCSTR_OPTSYN_UTF8OUTPUT", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_WARNASERRORLIST, "CSCSTR_OPTSYN_WARNASERRORLIST", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_WARNINGLEVEL, "CSCSTR_OPTSYN_WARNINGLEVEL", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_WARNINGSASERRORS, "CSCSTR_OPTSYN_WARNINGSASERRORS", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_WIN32ICON, "CSCSTR_OPTSYN_WIN32ICON", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_WIN32MANIFEST, "CSCSTR_OPTSYN_WIN32MANIFEST", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_WIN32RESOURCE, "CSCSTR_OPTSYN_WIN32RESOURCE", 0);
            AddInfo(ResNo.CSCSTR_OPTSYN_XMLDOCFILE, "CSCSTR_OPTSYN_XMLDOCFILE", 0);
            AddInfo(ResNo.CSCSTR_OutAttrOnRefParam, "CSCSTR_OutAttrOnRefParam", 1);
            AddInfo(ResNo.CSCSTR_OutputFileExists, "CSCSTR_OutputFileExists", 1);
            AddInfo(ResNo.CSCSTR_OutputFileNameTooLong, "CSCSTR_OutputFileNameTooLong", 0);
            AddInfo(ResNo.CSCSTR_OutputNeedsInput, "CSCSTR_OutputNeedsInput", 1);
            AddInfo(ResNo.CSCSTR_OutputNeedsName, "CSCSTR_OutputNeedsName", 0);
            AddInfo(ResNo.CSCSTR_OutputWriteFailed, "CSCSTR_OutputWriteFailed", 2);
            AddInfo(ResNo.CSCSTR_OverloadRefOut, "CSCSTR_OverloadRefOut", 1);
            AddInfo(ResNo.CSCSTR_OverrideFinalizeDeprecated, "CSCSTR_OverrideFinalizeDeprecated", 0);
            AddInfo(ResNo.CSCSTR_OverrideNotExpected, "CSCSTR_OverrideNotExpected", 1);
            AddInfo(ResNo.CSCSTR_OverrideNotNew, "CSCSTR_OverrideNotNew", 1);
            AddInfo(ResNo.CSCSTR_OverrideWithConstraints, "CSCSTR_OverrideWithConstraints", 0);
            AddInfo(ResNo.CSCSTR_OvlBinaryOperatorExpected, "CSCSTR_OvlBinaryOperatorExpected", 0);
            AddInfo(ResNo.CSCSTR_OvlOperatorExpected, "CSCSTR_OvlOperatorExpected", 0);
            AddInfo(ResNo.CSCSTR_OvlUnaryOperatorExpected, "CSCSTR_OvlUnaryOperatorExpected", 0);
            AddInfo(ResNo.CSCSTR_ParameterIsStaticClass, "CSCSTR_ParameterIsStaticClass", 1);
            AddInfo(ResNo.CSCSTR_ParamsCantBeRefOut, "CSCSTR_ParamsCantBeRefOut", 0);
            AddInfo(ResNo.CSCSTR_ParamsLast, "CSCSTR_ParamsLast", 0);
            AddInfo(ResNo.CSCSTR_ParamsMustBeArray, "CSCSTR_ParamsMustBeArray", 0);
            AddInfo(ResNo.CSCSTR_ParamUnassigned, "CSCSTR_ParamUnassigned", 1);
            AddInfo(ResNo.CSCSTR_PARENT, "CSCSTR_PARENT", 0);
            AddInfo(ResNo.CSCSTR_PartialMisplaced, "CSCSTR_PartialMisplaced", 0);
            AddInfo(ResNo.CSCSTR_PartialModifierConflict, "CSCSTR_PartialModifierConflict", 1);
            AddInfo(ResNo.CSCSTR_PartialMultipleBases, "CSCSTR_PartialMultipleBases", 1);
            AddInfo(ResNo.CSCSTR_PartialTypeKindConflict, "CSCSTR_PartialTypeKindConflict", 1);
            AddInfo(ResNo.CSCSTR_PartialWrongConstraints, "CSCSTR_PartialWrongConstraints", 2);
            AddInfo(ResNo.CSCSTR_PartialWrongTypeParams, "CSCSTR_PartialWrongTypeParams", 1);
            AddInfo(ResNo.CSCSTR_PatternBadSignature, "CSCSTR_PatternBadSignature", 3);
            AddInfo(ResNo.CSCSTR_PatternIsAmbiguous, "CSCSTR_PatternIsAmbiguous", 4);
            AddInfo(ResNo.CSCSTR_PatternStaticOrInaccessible, "CSCSTR_PatternStaticOrInaccessible", 3);
            AddInfo(ResNo.CSCSTR_PointerInAsOrIs, "CSCSTR_PointerInAsOrIs", 0);
            AddInfo(ResNo.CSCSTR_PossibleBadNegCast, "CSCSTR_PossibleBadNegCast", 0);
            AddInfo(ResNo.CSCSTR_PossibleMistakenNullStatement, "CSCSTR_PossibleMistakenNullStatement", 0);
            AddInfo(ResNo.CSCSTR_PPDefFollowsToken, "CSCSTR_PPDefFollowsToken", 0);
            AddInfo(ResNo.CSCSTR_PPDirectiveExpected, "CSCSTR_PPDirectiveExpected", 0);
            AddInfo(ResNo.CSCSTR_PredefinedTypeBadType, "CSCSTR_PredefinedTypeBadType", 1);
            AddInfo(ResNo.CSCSTR_PredefinedTypeNotFound, "CSCSTR_PredefinedTypeNotFound", 1);
            AddInfo(ResNo.CSCSTR_PrivateAbstractAccessor, "CSCSTR_PrivateAbstractAccessor", 1);
            AddInfo(ResNo.CSCSTR_PropertyAccessModInInterface, "CSCSTR_PropertyAccessModInInterface", 1);
            AddInfo(ResNo.CSCSTR_PropertyCantHaveVoidType, "CSCSTR_PropertyCantHaveVoidType", 1);
            AddInfo(ResNo.CSCSTR_PropertyLacksGet, "CSCSTR_PropertyLacksGet", 1);
            AddInfo(ResNo.CSCSTR_PropertyWithNoAccessors, "CSCSTR_PropertyWithNoAccessors", 1);
            AddInfo(ResNo.CSCSTR_ProtectedInSealed, "CSCSTR_ProtectedInSealed", 1);
            AddInfo(ResNo.CSCSTR_ProtectedInStatic, "CSCSTR_ProtectedInStatic", 1);
            AddInfo(ResNo.CSCSTR_ProtectedInStruct, "CSCSTR_ProtectedInStruct", 1);
            AddInfo(ResNo.CSCSTR_PtrExpected, "CSCSTR_PtrExpected", 0);
            AddInfo(ResNo.CSCSTR_PtrIndexSingle, "CSCSTR_PtrIndexSingle", 0);
            AddInfo(ResNo.CSCSTR_RbraceExpected, "CSCSTR_RbraceExpected", 0);
            AddInfo(ResNo.CSCSTR_RecursiveConstructorCall, "CSCSTR_RecursiveConstructorCall", 1);
            AddInfo(ResNo.CSCSTR_RefConstraintNotSatisfied, "CSCSTR_RefConstraintNotSatisfied", 3);
            AddInfo(ResNo.CSCSTR_RefLvalueExpected, "CSCSTR_RefLvalueExpected", 0);
            AddInfo(ResNo.CSCSTR_RefProperty, "CSCSTR_RefProperty", 0);
            AddInfo(ResNo.CSCSTR_RefReadonly, "CSCSTR_RefReadonly", 0);
            AddInfo(ResNo.CSCSTR_RefReadonly2, "CSCSTR_RefReadonly2", 1);
            AddInfo(ResNo.CSCSTR_RefReadonlyLocal, "CSCSTR_RefReadonlyLocal", 1);
            AddInfo(ResNo.CSCSTR_RefReadonlyLocal2, "CSCSTR_RefReadonlyLocal2", 1);
            AddInfo(ResNo.CSCSTR_RefReadonlyLocal2Cause, "CSCSTR_RefReadonlyLocal2Cause", 2);
            AddInfo(ResNo.CSCSTR_RefReadonlyLocalCause, "CSCSTR_RefReadonlyLocalCause", 2);
            AddInfo(ResNo.CSCSTR_RefReadonlyStatic, "CSCSTR_RefReadonlyStatic", 0);
            AddInfo(ResNo.CSCSTR_RefReadonlyStatic2, "CSCSTR_RefReadonlyStatic2", 1);
            AddInfo(ResNo.CSCSTR_RefValBoundMustBeFirst, "CSCSTR_RefValBoundMustBeFirst", 0);
            AddInfo(ResNo.CSCSTR_RefValBoundWithClass, "CSCSTR_RefValBoundWithClass", 1);
            AddInfo(ResNo.CSCSTR_RELATEDERROR, "CSCSTR_RELATEDERROR", 0);
            AddInfo(ResNo.CSCSTR_RELATEDWARNING, "CSCSTR_RELATEDWARNING", 0);
            AddInfo(ResNo.CSCSTR_REPROBINFILE, "CSCSTR_REPROBINFILE", 1);
            AddInfo(ResNo.CSCSTR_REPROCOMMANDLINE, "CSCSTR_REPROCOMMANDLINE", 0);
            AddInfo(ResNo.CSCSTR_REPROCORRECTBEHAVIOR, "CSCSTR_REPROCORRECTBEHAVIOR", 0);
            AddInfo(ResNo.CSCSTR_REPRODESCRIPTION, "CSCSTR_REPRODESCRIPTION", 0);
            AddInfo(ResNo.CSCSTR_REPRODIAGS, "CSCSTR_REPRODIAGS", 0);
            AddInfo(ResNo.CSCSTR_REPROLCID, "CSCSTR_REPROLCID", 1);
            AddInfo(ResNo.CSCSTR_REPROOS, "CSCSTR_REPROOS", 5);
            AddInfo(ResNo.CSCSTR_REPROSOURCEFILE, "CSCSTR_REPROSOURCEFILE", 1);
            AddInfo(ResNo.CSCSTR_REPROTITLE, "CSCSTR_REPROTITLE", 1);
            AddInfo(ResNo.CSCSTR_REPROURTVER, "CSCSTR_REPROURTVER", 1);
            AddInfo(ResNo.CSCSTR_REPROVER, "CSCSTR_REPROVER", 1);
            AddInfo(ResNo.CSCSTR_RequiredFileNotFound, "CSCSTR_RequiredFileNotFound", 1);
            AddInfo(ResNo.CSCSTR_ReservedEnumerator, "CSCSTR_ReservedEnumerator", 1);
            AddInfo(ResNo.CSCSTR_ReservedIdentifier2, "CSCSTR_ReservedIdentifier2", 1);
            AddInfo(ResNo.CSCSTR_RESINFO_DESCRIPTION, "CSCSTR_RESINFO_DESCRIPTION", 0);
            AddInfo(ResNo.CSCSTR_ResourceNotUnique, "CSCSTR_ResourceNotUnique", 1);
            AddInfo(ResNo.CSCSTR_RetNoObjectRequired, "CSCSTR_RetNoObjectRequired", 1);
            AddInfo(ResNo.CSCSTR_RetObjectRequired, "CSCSTR_RetObjectRequired", 1);
            AddInfo(ResNo.CSCSTR_ReturnExpected, "CSCSTR_ReturnExpected", 1);
            AddInfo(ResNo.CSCSTR_ReturnInIterator, "CSCSTR_ReturnInIterator", 0);
            AddInfo(ResNo.CSCSTR_ReturnNotLValue, "CSCSTR_ReturnNotLValue", 1);
            AddInfo(ResNo.CSCSTR_ReturnTypeIsStaticClass, "CSCSTR_ReturnTypeIsStaticClass", 1);
            AddInfo(ResNo.CSCSTR_SameFullNameAggAgg, "CSCSTR_SameFullNameAggAgg", 3);
            AddInfo(ResNo.CSCSTR_SameFullNameNsAgg, "CSCSTR_SameFullNameNsAgg", 4);
            AddInfo(ResNo.CSCSTR_SameFullNameThisAggAgg, "CSCSTR_SameFullNameThisAggAgg", 4);
            AddInfo(ResNo.CSCSTR_SameFullNameThisAggNs, "CSCSTR_SameFullNameThisAggNs", 4);
            AddInfo(ResNo.CSCSTR_SameFullNameThisAggThisAgg, "CSCSTR_SameFullNameThisAggThisAgg", 4);
            AddInfo(ResNo.CSCSTR_SameFullNameThisAggThisNs, "CSCSTR_SameFullNameThisAggThisNs", 4);
            AddInfo(ResNo.CSCSTR_SameFullNameThisNsAgg, "CSCSTR_SameFullNameThisNsAgg", 4);
            AddInfo(ResNo.CSCSTR_SealedNonOverride, "CSCSTR_SealedNonOverride", 1);
            AddInfo(ResNo.CSCSTR_SealedStaticClass, "CSCSTR_SealedStaticClass", 1);
            AddInfo(ResNo.CSCSTR_SemicolonExpected, "CSCSTR_SemicolonExpected", 0);
            AddInfo(ResNo.CSCSTR_SemiOrLBraceExpected, "CSCSTR_SemiOrLBraceExpected", 0);
            AddInfo(ResNo.CSCSTR_SequentialOnPartialClass, "CSCSTR_SequentialOnPartialClass", 1);
            AddInfo(ResNo.CSCSTR_SHORTFORM, "CSCSTR_SHORTFORM", 0);
            AddInfo(ResNo.CSCSTR_SingleTypeNameNotFound, "CSCSTR_SingleTypeNameNotFound", 1);
            AddInfo(ResNo.CSCSTR_SizeofUnsafe, "CSCSTR_SizeofUnsafe", 1);
            AddInfo(ResNo.CSCSTR_SK_ALIAS, "CSCSTR_SK_ALIAS", 0);
            AddInfo(ResNo.CSCSTR_SK_CLASS, "CSCSTR_SK_CLASS", 0);
            AddInfo(ResNo.CSCSTR_SK_EVENT, "CSCSTR_SK_EVENT", 0);
            AddInfo(ResNo.CSCSTR_SK_EXTERNALIAS, "CSCSTR_SK_EXTERNALIAS", 0);
            AddInfo(ResNo.CSCSTR_SK_FIELD, "CSCSTR_SK_FIELD", 0);
            AddInfo(ResNo.CSCSTR_SK_METHOD, "CSCSTR_SK_METHOD", 0);
            AddInfo(ResNo.CSCSTR_SK_NAMESPACE, "CSCSTR_SK_NAMESPACE", 0);
            AddInfo(ResNo.CSCSTR_SK_PROPERTY, "CSCSTR_SK_PROPERTY", 0);
            AddInfo(ResNo.CSCSTR_SK_TYVAR, "CSCSTR_SK_TYVAR", 0);
            AddInfo(ResNo.CSCSTR_SK_UNKNOWN, "CSCSTR_SK_UNKNOWN", 0);
            AddInfo(ResNo.CSCSTR_SK_VARIABLE, "CSCSTR_SK_VARIABLE", 0);
            AddInfo(ResNo.CSCSTR_SourceMapFileBinary, "CSCSTR_SourceMapFileBinary", 1);
            AddInfo(ResNo.CSCSTR_SpecialTypeAsBound, "CSCSTR_SpecialTypeAsBound", 1);
            AddInfo(ResNo.CSCSTR_StackallocInCatchFinally, "CSCSTR_StackallocInCatchFinally", 0);
            AddInfo(ResNo.CSCSTR_StackOverflow, "CSCSTR_StackOverflow", 1);
            AddInfo(ResNo.CSCSTR_StaticBaseClass, "CSCSTR_StaticBaseClass", 2);
            AddInfo(ResNo.CSCSTR_StaticClassInterfaceImpl, "CSCSTR_StaticClassInterfaceImpl", 1);
            AddInfo(ResNo.CSCSTR_StaticConstant, "CSCSTR_StaticConstant", 1);
            AddInfo(ResNo.CSCSTR_StaticConstParam, "CSCSTR_StaticConstParam", 1);
            AddInfo(ResNo.CSCSTR_StaticConstructorWithAccessModifiers, "CSCSTR_StaticConstructorWithAccessModifiers", 1);
            AddInfo(ResNo.CSCSTR_StaticConstructorWithExplicitConstructorCall, "CSCSTR_StaticConstructorWithExplicitConstructorCall", 1);
            AddInfo(ResNo.CSCSTR_StaticDerivedFromNonObject, "CSCSTR_StaticDerivedFromNonObject", 2);
            AddInfo(ResNo.CSCSTR_StaticNotVirtual, "CSCSTR_StaticNotVirtual", 1);
            AddInfo(ResNo.CSCSTR_STRING4001, "CSCSTR_STRING4001", 0);
            AddInfo(ResNo.CSCSTR_STRING4002, "CSCSTR_STRING4002", 0);
            AddInfo(ResNo.CSCSTR_STRING4003, "CSCSTR_STRING4003", 0);
            AddInfo(ResNo.CSCSTR_STRING4004, "CSCSTR_STRING4004", 0);
            AddInfo(ResNo.CSCSTR_STRING4005, "CSCSTR_STRING4005", 0);
            AddInfo(ResNo.CSCSTR_STRING4006, "CSCSTR_STRING4006", 0);
            AddInfo(ResNo.CSCSTR_STRING4007, "CSCSTR_STRING4007", 0);
            AddInfo(ResNo.CSCSTR_STRING4011, "CSCSTR_STRING4011", 0);
            AddInfo(ResNo.CSCSTR_STRING4012, "CSCSTR_STRING4012", 0);
            AddInfo(ResNo.CSCSTR_STRING4013, "CSCSTR_STRING4013", 0);
            AddInfo(ResNo.CSCSTR_STRING4014, "CSCSTR_STRING4014", 0);
            AddInfo(ResNo.CSCSTR_STRING4015, "CSCSTR_STRING4015", 0);
            AddInfo(ResNo.CSCSTR_STRING4016, "CSCSTR_STRING4016", 0);
            AddInfo(ResNo.CSCSTR_StructLayoutCycle, "CSCSTR_StructLayoutCycle", 2);
            AddInfo(ResNo.CSCSTR_StructOffsetOnBadField, "CSCSTR_StructOffsetOnBadField", 0);
            AddInfo(ResNo.CSCSTR_StructOffsetOnBadStruct, "CSCSTR_StructOffsetOnBadStruct", 0);
            AddInfo(ResNo.CSCSTR_StructsCantContainDefaultContructor, "CSCSTR_StructsCantContainDefaultContructor", 0);
            AddInfo(ResNo.CSCSTR_StructWithBaseConstructorCall, "CSCSTR_StructWithBaseConstructorCall", 1);
            AddInfo(ResNo.CSCSTR_SwitchFallInto, "CSCSTR_SwitchFallInto", 1);
            AddInfo(ResNo.CSCSTR_SwitchFallThrough, "CSCSTR_SwitchFallThrough", 1);
            AddInfo(ResNo.CSCSTR_SwitchNeedsNumber, "CSCSTR_SwitchNeedsNumber", 1);
            AddInfo(ResNo.CSCSTR_SWITCHNEEDSSTRING, "CSCSTR_SWITCHNEEDSSTRING", 1);
            AddInfo(ResNo.CSCSTR_SyntaxError, "CSCSTR_SyntaxError", 1);
            AddInfo(ResNo.CSCSTR_SystemVoid, "CSCSTR_SystemVoid", 0);
            AddInfo(ResNo.CSCSTR_ThisAssembly, "CSCSTR_ThisAssembly", 0);
            AddInfo(ResNo.CSCSTR_ThisInBadContext, "CSCSTR_ThisInBadContext", 0);
            AddInfo(ResNo.CSCSTR_ThisInStaticMeth, "CSCSTR_ThisInStaticMeth", 0);
            AddInfo(ResNo.CSCSTR_ThisOrBaseExpected, "CSCSTR_ThisOrBaseExpected", 0);
            AddInfo(ResNo.CSCSTR_ThisStructNotInAnonMeth, "CSCSTR_ThisStructNotInAnonMeth", 0);
            AddInfo(ResNo.CSCSTR_TooManyCatches, "CSCSTR_TooManyCatches", 0);
            AddInfo(ResNo.CSCSTR_TooManyCharsInConst, "CSCSTR_TooManyCharsInConst", 0);
            AddInfo(ResNo.CSCSTR_TooManyLines, "CSCSTR_TooManyLines", 1);
            AddInfo(ResNo.CSCSTR_TooManyLinesForDebugger, "CSCSTR_TooManyLinesForDebugger", 0);
            AddInfo(ResNo.CSCSTR_TooManyLocals, "CSCSTR_TooManyLocals", 0);
            AddInfo(ResNo.CSCSTR_TypeArgsNotAllowed, "CSCSTR_TypeArgsNotAllowed", 2);
            AddInfo(ResNo.CSCSTR_TypeArgsNotAllowedAmbig, "CSCSTR_TypeArgsNotAllowedAmbig", 2);
            AddInfo(ResNo.CSCSTR_TypeExpected, "CSCSTR_TypeExpected", 0);
            AddInfo(ResNo.CSCSTR_TypeNameBuilderError, "CSCSTR_TypeNameBuilderError", 2);
            AddInfo(ResNo.CSCSTR_TypeParameterSameAsOuterTypeParameter, "CSCSTR_TypeParameterSameAsOuterTypeParameter", 2);
            AddInfo(ResNo.CSCSTR_TypeParamMustBeIdentifier, "CSCSTR_TypeParamMustBeIdentifier", 0);
            AddInfo(ResNo.CSCSTR_TypeVarCantBeNull, "CSCSTR_TypeVarCantBeNull", 1);
            AddInfo(ResNo.CSCSTR_TypeVariableSameAsParent, "CSCSTR_TypeVariableSameAsParent", 1);
            AddInfo(ResNo.CSCSTR_TyVarNotFoundInConstraint, "CSCSTR_TyVarNotFoundInConstraint", 2);
            AddInfo(ResNo.CSCSTR_UnassignedInternalField, "CSCSTR_UnassignedInternalField", 2);
            AddInfo(ResNo.CSCSTR_UnassignedThis, "CSCSTR_UnassignedThis", 1);
            AddInfo(ResNo.CSCSTR_UnboxNotLValue, "CSCSTR_UnboxNotLValue", 0);
            AddInfo(ResNo.CSCSTR_UnexpectedCharacter, "CSCSTR_UnexpectedCharacter", 1);
            AddInfo(ResNo.CSCSTR_UnexpectedDirective, "CSCSTR_UnexpectedDirective", 0);
            AddInfo(ResNo.CSCSTR_UnexpectedPredefTypeLoc, "CSCSTR_UnexpectedPredefTypeLoc", 3);
            AddInfo(ResNo.CSCSTR_UnexpectedSemicolon, "CSCSTR_UnexpectedSemicolon", 0);
            AddInfo(ResNo.CSCSTR_UnifyingInterfaceInstantiations, "CSCSTR_UnifyingInterfaceInstantiations", 3);
            AddInfo(ResNo.CSCSTR_UnifyReferenceBldRev, "CSCSTR_UnifyReferenceBldRev", 2);
            AddInfo(ResNo.CSCSTR_UnifyReferenceMajMin, "CSCSTR_UnifyReferenceMajMin", 2);
            AddInfo(ResNo.CSCSTR_UnimplementedAbstractMethod, "CSCSTR_UnimplementedAbstractMethod", 2);
            AddInfo(ResNo.CSCSTR_UnimplementedInterfaceAccessor, "CSCSTR_UnimplementedInterfaceAccessor", 3);
            AddInfo(ResNo.CSCSTR_UnimplementedInterfaceMember, "CSCSTR_UnimplementedInterfaceMember", 2);
            AddInfo(ResNo.CSCSTR_UnimplementedOp, "CSCSTR_UnimplementedOp", 1);
            AddInfo(ResNo.CSCSTR_UnknownOption, "CSCSTR_UnknownOption", 1);
            AddInfo(ResNo.CSCSTR_UnknownTestSwitch, "CSCSTR_UnknownTestSwitch", 1);
            AddInfo(ResNo.CSCSTR_UnmatchedParamTag, "CSCSTR_UnmatchedParamTag", 2);
            AddInfo(ResNo.CSCSTR_UnmatchedTypeParamTag, "CSCSTR_UnmatchedTypeParamTag", 2);
            AddInfo(ResNo.CSCSTR_UnprocessedXMLComment, "CSCSTR_UnprocessedXMLComment", 0);
            AddInfo(ResNo.CSCSTR_UnreachableCatch, "CSCSTR_UnreachableCatch", 1);
            AddInfo(ResNo.CSCSTR_UnreachableCode, "CSCSTR_UnreachableCode", 0);
            AddInfo(ResNo.CSCSTR_UnreachableExpr, "CSCSTR_UnreachableExpr", 0);
            AddInfo(ResNo.CSCSTR_UnreachableGeneralCatch, "CSCSTR_UnreachableGeneralCatch", 0);
            AddInfo(ResNo.CSCSTR_UnreferencedEvent, "CSCSTR_UnreferencedEvent", 1);
            AddInfo(ResNo.CSCSTR_UnreferencedField, "CSCSTR_UnreferencedField", 1);
            AddInfo(ResNo.CSCSTR_UnreferencedFieldAssg, "CSCSTR_UnreferencedFieldAssg", 1);
            AddInfo(ResNo.CSCSTR_UnreferencedLabel, "CSCSTR_UnreferencedLabel", 0);
            AddInfo(ResNo.CSCSTR_UnreferencedVar, "CSCSTR_UnreferencedVar", 1);
            AddInfo(ResNo.CSCSTR_UnreferencedVarAssg, "CSCSTR_UnreferencedVarAssg", 1);
            AddInfo(ResNo.CSCSTR_UnsafeIteratorArgType, "CSCSTR_UnsafeIteratorArgType", 0);
            AddInfo(ResNo.CSCSTR_UnsafeNeeded, "CSCSTR_UnsafeNeeded", 0);
            AddInfo(ResNo.CSCSTR_UnterminatedStringLit, "CSCSTR_UnterminatedStringLit", 0);
            AddInfo(ResNo.CSCSTR_UseDefViolation, "CSCSTR_UseDefViolation", 1);
            AddInfo(ResNo.CSCSTR_UseDefViolationField, "CSCSTR_UseDefViolationField", 1);
            AddInfo(ResNo.CSCSTR_UseDefViolationOut, "CSCSTR_UseDefViolationOut", 1);
            AddInfo(ResNo.CSCSTR_UseDefViolationThis, "CSCSTR_UseDefViolationThis", 0);
            AddInfo(ResNo.CSCSTR_USENEWSWITCH, "CSCSTR_USENEWSWITCH", 2);
            AddInfo(ResNo.CSCSTR_UseSwitchInsteadOfAttribute, "CSCSTR_UseSwitchInsteadOfAttribute", 2);
            AddInfo(ResNo.CSCSTR_UsingAfterElements, "CSCSTR_UsingAfterElements", 0);
            AddInfo(ResNo.CSCSTR_USINGLOCAL, "CSCSTR_USINGLOCAL", 0);
            AddInfo(ResNo.CSCSTR_VacuousIntegralComp, "CSCSTR_VacuousIntegralComp", 1);
            AddInfo(ResNo.CSCSTR_ValConstraintNotSatisfied, "CSCSTR_ValConstraintNotSatisfied", 3);
            AddInfo(ResNo.CSCSTR_ValueCantBeNull, "CSCSTR_ValueCantBeNull", 1);
            AddInfo(ResNo.CSCSTR_ValueExpected, "CSCSTR_ValueExpected", 0);
            AddInfo(ResNo.CSCSTR_VarargsIterator, "CSCSTR_VarargsIterator", 0);
            AddInfo(ResNo.CSCSTR_VarargsLast, "CSCSTR_VarargsLast", 0);
            AddInfo(ResNo.CSCSTR_VarDeclIsStaticClass, "CSCSTR_VarDeclIsStaticClass", 1);
            AddInfo(ResNo.CSCSTR_VERSION_CONFLICT, "CSCSTR_VERSION_CONFLICT", 4);
            AddInfo(ResNo.CSCSTR_VirtualPrivate, "CSCSTR_VirtualPrivate", 1);
            AddInfo(ResNo.CSCSTR_VoidError, "CSCSTR_VoidError", 0);
            AddInfo(ResNo.CSCSTR_VolatileAndReadonly, "CSCSTR_VolatileAndReadonly", 1);
            AddInfo(ResNo.CSCSTR_VolatileByRef, "CSCSTR_VolatileByRef", 1);
            AddInfo(ResNo.CSCSTR_VolatileStruct, "CSCSTR_VolatileStruct", 2);
            AddInfo(ResNo.CSCSTR_WarnAsError, "CSCSTR_WarnAsError", 1);
            AddInfo(ResNo.CSCSTR_WarningAsError, "CSCSTR_WarningAsError", 0);
            AddInfo(ResNo.CSCSTR_WarningDirective, "CSCSTR_WarningDirective", 1);
            AddInfo(ResNo.CSCSTR_WATSON_APPNAME, "CSCSTR_WATSON_APPNAME", 0);
            AddInfo(ResNo.CSCSTR_WATSON_ERROR_HEADER, "CSCSTR_WATSON_ERROR_HEADER", 0);
            AddInfo(ResNo.CSCSTR_WATSON_ERROR_MESSAGE, "CSCSTR_WATSON_ERROR_MESSAGE", 0);
            AddInfo(ResNo.CSCSTR_WatsonSendNotOptedIn, "CSCSTR_WatsonSendNotOptedIn", 1);
            AddInfo(ResNo.CSCSTR_WrongNestedThis, "CSCSTR_WrongNestedThis", 2);
            AddInfo(ResNo.CSCSTR_WrongSignature, "CSCSTR_WrongSignature", 1);
            AddInfo(ResNo.CSCSTR_XMLBADINCLUDE, "CSCSTR_XMLBADINCLUDE", 0);
            AddInfo(ResNo.CSCSTR_XMLFAILEDINCLUDE, "CSCSTR_XMLFAILEDINCLUDE", 0);
            AddInfo(ResNo.CSCSTR_XMLIGNORED, "CSCSTR_XMLIGNORED", 1);
            AddInfo(ResNo.CSCSTR_XMLIGNORED2, "CSCSTR_XMLIGNORED2", 1);
            AddInfo(ResNo.CSCSTR_XMLMISSINGINCLUDEFILE, "CSCSTR_XMLMISSINGINCLUDEFILE", 0);
            AddInfo(ResNo.CSCSTR_XMLMISSINGINCLUDEPATH, "CSCSTR_XMLMISSINGINCLUDEPATH", 0);
            AddInfo(ResNo.CSCSTR_XMLNOINCLUDE, "CSCSTR_XMLNOINCLUDE", 0);
            AddInfo(ResNo.CSCSTR_XMLParseError, "CSCSTR_XMLParseError", 2);
            AddInfo(ResNo.CSCSTR_XMLParseIncludeError, "CSCSTR_XMLParseIncludeError", 1);
            AddInfo(ResNo.CSCSTR_XMLParserNotFound, "CSCSTR_XMLParserNotFound", 2);
            AddInfo(ResNo.CSCSTR_YieldInAnonMeth, "CSCSTR_YieldInAnonMeth", 0);

            // CS3

            AddInfo(ResNo.CSCSTR_LambdaExpression, "CSCSTR_LambdaExpression", 1);

            AddInfo(ResNo.CSCSTR_VariableNotDeclared, "CSCSTR_VariableNotDeclared", 1);
            AddInfo(ResNo.CSCSTR_CannotAssignToImplicitType, "CSCSTR_CannotAssignToImplicitType", 1);
            AddInfo(ResNo.CSCSTR_ImplicitTypeNotInitialized, "CSCSTR_ImplicitTypeNotInitialized", 0);
            AddInfo(ResNo.CSCSTR_ImplicitTypeMultipleDeclarators, "CSCSTR_ImplicitTypeMultipleDeclarators", 0);
            AddInfo(ResNo.CSCSTR_PropertyAccessorHasNoBody, "CSCSTR_PropertyAccessorHasNoBody", 1);
            AddInfo(ResNo.CSCSTR_NoBestTypeForArray, "CSCSTR_NoBestTypeForArray", 0);
            AddInfo(ResNo.CSCSTR_InvalidAnonTypeMemberDeclarator, "CSCSTR_InvalidAnonTypeMemberDeclarator", 0);
            AddInfo(ResNo.CSCSTR_InvalidInitializerDeclarator, "CSCSTR_InvalidInitializerDeclarator", 0);
            AddInfo(ResNo.CSCSTR_CollectInitRequiresIEnumerable, "CSCSTR_CollectInitRequiresIEnumerable", 1);
            AddInfo(ResNo.CSCSTR_ThisModifierNotOnFirstParam, "CSCSTR_ThisModifierNotOnFirstParam", 1);
            AddInfo(ResNo.CSCSTR_NonStaticExtensionMethod, "CSCSTR_NonStaticExtensionMethod", 0);
            AddInfo(ResNo.CSCSTR_ExtensionMethodInImproperClass, "CSCSTR_ExtensionMethodInImproperClass", 0);
            AddInfo(ResNo.CSCSTR_ExtensionMethodInNestedClass, "CSCSTR_ExtensionMethodInNestedClass", 1);
            AddInfo(ResNo.CSCSTR_InconsistentLambdaParameters, "CSCSTR_InconsistentLambdaParameters", 0);
            AddInfo(ResNo.CSCSTR_QueryPatternNotImplemented, "CSCSTR_QueryPatternNotImplemented", 2);
            AddInfo(ResNo.CSCSTR_ExpectContextualKeyword, "CSCSTR_ExpectContextualKeyword", 1);
            AddInfo(ResNo.CSCSTR_QueryTypeInferenceFailed, "CSCSTR_QueryTypeInferenceFailed", 1);
            AddInfo(ResNo.CSCSTR_QueryBodyHasNoSelectOrGroup, "CSCSTR_QueryBodyHasNoSelectOrGroup", 0);
            AddInfo(ResNo.CSCSTR_ExprTreeContainsAssignment, "CSCSTR_ExprTreeContainsAssignment", 0);
            AddInfo(ResNo.CSCSTR_CannotConvertToExprTree, "CSCSTR_CannotConvertToExprTree", 0);
            AddInfo(ResNo.CSCSTR_MultiplePartialMethodImplementation, "CSCSTR_MultiplePartialMethodImplementation", 0);
            AddInfo(ResNo.CSCSTR_BadModifierForPartialMethod, "CSCSTR_BadModifierForPartialMethod", 0);
            AddInfo(ResNo.CSCSTR_PartialMethodHasOutParameter, "CSCSTR_PartialMethodHasOutParameter", 0);

            // CS4

            AddInfo(ResNo.CSCSTR_DynamicOnTypeof, "CSCSTR_DynamicOnTypeof", 0);

#if false
            ArgCountDic.Add("CSCSTR_InternalError", 1); // "Internal compiler error ({0})"
            ArgCountDic.Add("CSCSTR_NoMemory", 0); // "Out of memory"
            ArgCountDic.Add("CSCSTR_WarningAsError", 0); // "Warning treated as error"
            ArgCountDic.Add("CSCSTR_MissingOptionArg", 1); // "Compiler option '{0}' must be followed by an argument"
            ArgCountDic.Add("CSCSTR_NoMetadataFile", 1); // "Metadata file '{0}' could not be found"
            ArgCountDic.Add("CSCSTR_ComPlusInit", 1); // "Unexpected common language runtime initialization error -- '{0}'"
            ArgCountDic.Add("CSCSTR_MetadataImportFailure", 2); // "Unexpected error reading metadata from file '{1}' -- '{0}'"
            ArgCountDic.Add("CSCSTR_MetadataCantOpenFile", 2); // "Metadata file '{1}' could not be opened -- '{0}'"
            ArgCountDic.Add("CSCSTR_CantImportBase", 3); // "The base class or interface '{1}' in assembly '{2}' referenced by type '{0}' could not be resolved"
            ArgCountDic.Add("CSCSTR_ImportBadBase", 1); // "The base class or interface of '{0}' could not be resolved or is invalid"
            ArgCountDic.Add("CSCSTR_NoTypeDef", 2); // "The type '{0}' is defined in an assembly that is not referenced. You must add a reference to assembly '{1}'."
            ArgCountDic.Add("CSCSTR_MetadataEmitFailure", 2); // "Unexpected error writing metadata to file '{1}' -- '{0}'"
            ArgCountDic.Add("CSCSTR_RequiredFileNotFound", 1); // "Required file '{0}' could not be found"
            ArgCountDic.Add("CSCSTR_ClassNameTooLong", 1); // "The name of type '{0}' is too long"
            ArgCountDic.Add("CSCSTR_TypeNameBuilderError", 2); // "Unexpected error building metadata name for type '{0}' -- '{1}'"
            ArgCountDic.Add("CSCSTR_AbstractHasBody", 1); // "'{0}' cannot declare a body because it is marked abstract"
            ArgCountDic.Add("CSCSTR_ConcreteMissingBody", 1); // "'{0}' must declare a body because it is not marked abstract or extern"
            ArgCountDic.Add("CSCSTR_AbstractAndSealed", 1); // "'{0}' cannot be both abstract and sealed"
            ArgCountDic.Add("CSCSTR_AbstractNotVirtual", 1); // "The abstract method '{0}' cannot be marked virtual"
            ArgCountDic.Add("CSCSTR_StaticConstant", 1); // "The constant '{0}' cannot be marked static"
            ArgCountDic.Add("CSCSTR_CantOverrideNonFunction", 2); // "'{0}': cannot override because '{1}' is not a function"
            ArgCountDic.Add("CSCSTR_CantOverrideNonVirtual", 2); // "'{0}': cannot override inherited member '{1}' because it is not marked virtual, abstract, or override"
            ArgCountDic.Add("CSCSTR_CantChangeAccessOnOverride", 3); // "'{0}': cannot change access modifiers when overriding '{1}' inherited member '{2}'"
            ArgCountDic.Add("CSCSTR_CantChangeReturnTypeOnOverride", 3); // "'{0}': return type must be '{2}' to match overridden member '{1}'"
            ArgCountDic.Add("CSCSTR_CantChangeTypeOnOverride", 3); // "'{0}': type must be '{2}' to match overridden member '{1}'"
            ArgCountDic.Add("CSCSTR_CantDeriveFromSealedType", 2); // "'{0}': cannot derive from sealed type '{1}'"
            ArgCountDic.Add("CSCSTR_AbstractInConcreteClass", 2); // "'{0}' is abstract but it is contained in nonabstract class '{1}'"
            ArgCountDic.Add("CSCSTR_StaticConstructorWithExplicitConstructorCall", 1); // "'{0}': static constructor cannot have an explicit 'this' or 'base' constructor call"
            ArgCountDic.Add("CSCSTR_StaticConstructorWithAccessModifiers", 1); // "'{0}': access modifiers are not allowed on static constructors"
            ArgCountDic.Add("CSCSTR_RecursiveConstructorCall", 1); // "Constructor '{0}' cannot call itself"
            ArgCountDic.Add("CSCSTR_ObjectCallingBaseConstructor", 1); // "'{0}' has no base class and cannot call a base constructor"
            ArgCountDic.Add("CSCSTR_PredefinedTypeNotFound", 1); // "Predefined type '{0}' is not defined or imported"
            ArgCountDic.Add("CSCSTR_PredefinedTypeBadType", 1); // "Predefined type '{0}' is declared incorrectly"
            ArgCountDic.Add("CSCSTR_StructWithBaseConstructorCall", 1); // "'{0}': structs cannot call base class constructors"
            ArgCountDic.Add("CSCSTR_StructLayoutCycle", 2); // "Struct member '{0}' of type '{1}' causes a cycle in the struct layout"
            ArgCountDic.Add("CSCSTR_InterfacesCannotContainTypes", 1); // "'{0}': interfaces cannot declare types"
            ArgCountDic.Add("CSCSTR_InterfacesCantContainFields", 0); // "Interfaces cannot contain fields"
            ArgCountDic.Add("CSCSTR_InterfacesCantContainConstructors", 0); // "Interfaces cannot contain constructors"
            ArgCountDic.Add("CSCSTR_NonInterfaceInInterfaceList", 1); // "Type '{0}' in interface list is not an interface"
            ArgCountDic.Add("CSCSTR_NoMultipleInheritance", 3); // "Class '{0}' cannot have multiple base classes: '{1}' and '{2}'"
            ArgCountDic.Add("CSCSTR_BaseClassMustBeFirst", 1); // "Base class '{0}' must come before any interfaces"
            ArgCountDic.Add("CSCSTR_DuplicateInterfaceInBaseList", 1); // "'{0}' is already listed in interface list"
            ArgCountDic.Add("CSCSTR_CycleInInterfaceInheritance", 2); // "Inherited interface '{1}' causes a cycle in the interface hierarchy of '{0}'"
            ArgCountDic.Add("CSCSTR_InterfaceMemberHasBody", 1); // "'{0}': interface members cannot have a definition"
            ArgCountDic.Add("CSCSTR_HidingAbstractMethod", 2); // "'{0}' hides inherited abstract member '{1}'"
            ArgCountDic.Add("CSCSTR_UnimplementedAbstractMethod", 2); // "'{0}' does not implement inherited abstract member '{1}'"
            ArgCountDic.Add("CSCSTR_UnimplementedInterfaceMember", 2); // "'{0}' does not implement interface member '{1}'"
            ArgCountDic.Add("CSCSTR_CloseUnimplementedInterfaceMember", 3); // "'{0}' does not implement interface member '{1}'. '{2}' is either static, not public, or has the wrong return type."
            ArgCountDic.Add("CSCSTR_ObjectCantHaveBases", 0); // "The class System.Object cannot have a base class or implement an interface"
            ArgCountDic.Add("CSCSTR_ExplicitInterfaceImplementationNotInterface", 1); // "'{0}' in explicit interface declaration is not an interface"
            ArgCountDic.Add("CSCSTR_InterfaceMemberNotFound", 1); // "'{0}' in explicit interface declaration is not a member of interface"
            ArgCountDic.Add("CSCSTR_ClassDoesntImplementInterface", 2); // "'{0}': containing type does not implement interface '{1}'"
            ArgCountDic.Add("CSCSTR_ExplicitInterfaceImplementationInNonClassOrStruct", 1); // "'{0}': explicit interface declaration can only be declared in a class or struct"
            ArgCountDic.Add("CSCSTR_MemberNameSameAsType", 1); // "'{0}': member names cannot be the same as their enclosing type"
            ArgCountDic.Add("CSCSTR_EnumeratorOverflow", 1); // "'{0}': the enumerator value is too large to fit in its type"
            ArgCountDic.Add("CSCSTR_InstanceMemberInStaticClass", 1); // "'{0}': cannot declare instance members in a static class"
            ArgCountDic.Add("CSCSTR_ConstructorInStaticClass", 0); // "Static classes cannot have instance constructors"
            ArgCountDic.Add("CSCSTR_DestructorInStaticClass", 0); // "Static classes cannot contain destructors"
            ArgCountDic.Add("CSCSTR_StaticBaseClass", 2); // "'{1}': cannot derive from static class '{0}'"
            ArgCountDic.Add("CSCSTR_InstantiatingStaticClass", 1); // "Cannot create an instance of the static class '{0}'"
            ArgCountDic.Add("CSCSTR_StaticDerivedFromNonObject", 2); // "Static class '{0}' cannot derive from type '{1}'. Static classes must derive from object."
            ArgCountDic.Add("CSCSTR_StaticClassInterfaceImpl", 1); // "'{0}': static classes cannot implement interfaces"
            ArgCountDic.Add("CSCSTR_OperatorInStaticClass", 1); // "'{0}': static classes cannot contain user-defined operators"
            ArgCountDic.Add("CSCSTR_ConvertToStaticClass", 1); // "Cannot convert to static type '{0}'"
            ArgCountDic.Add("CSCSTR_ConstraintIsStaticClass", 1); // "'{0}': static classes cannot be used as constraints"
            ArgCountDic.Add("CSCSTR_GenericArgIsStaticClass", 1); // "'{0}': static types cannot be used as generic arguments"
            ArgCountDic.Add("CSCSTR_ArrayOfStaticClass", 1); // "'{0}': array elements cannot be of static type"
            ArgCountDic.Add("CSCSTR_IndexerInStaticClass", 1); // "'{0}': cannot declare indexers in a static class"
            ArgCountDic.Add("CSCSTR_ParameterIsStaticClass", 1); // "'{0}': static types cannot be used as parameters"
            ArgCountDic.Add("CSCSTR_ReturnTypeIsStaticClass", 1); // "'{0}': static types cannot be used as return types"
            ArgCountDic.Add("CSCSTR_VarDeclIsStaticClass", 1); // "Cannot declare variable of static type '{0}'"
            ArgCountDic.Add("CSCSTR_PatternIsAmbiguous", 4); // "'{0}' does not implement the '{1}' pattern. '{2}' is ambiguous with '{3}'."
            ArgCountDic.Add("CSCSTR_PatternStaticOrInaccessible", 3); // "'{0}' does not implement the '{1}' pattern. '{2}' is either static or not public."
            ArgCountDic.Add("CSCSTR_PatternBadSignature", 3); // "'{0}' does not implement the '{1}' pattern. '{2}' has the wrong signature."
            ArgCountDic.Add("CSCSTR_FriendRefNotEqualToThis", 2); // "Friend access was granted to '{0}', but the output assembly is named '{1}'. Try adding a reference to '{0}' or changing the output assembly name to match."
            ArgCountDic.Add("CSCSTR_SequentialOnPartialClass", 1); // "There is no defined ordering between fields in multiple declarations of partial class or struct '{0}'. To specify an ordering, all instance fields must be in the same declaration."
            ArgCountDic.Add("CSCSTR_CantOverrideNonProperty", 2); // "'{0}': cannot override because '{1}' is not a property"
            ArgCountDic.Add("CSCSTR_NoGetToOverride", 2); // "'{0}': cannot override because '{1}' does not have an overridable get accessor"
            ArgCountDic.Add("CSCSTR_NoSetToOverride", 2); // "'{0}': cannot override because '{1}' does not have an overridable set accessor"
            ArgCountDic.Add("CSCSTR_PropertyCantHaveVoidType", 1); // "'{0}': property or indexer cannot have void type"
            ArgCountDic.Add("CSCSTR_PropertyWithNoAccessors", 1); // "'{0}': property or indexer must have at least one accessor"
            ArgCountDic.Add("CSCSTR_NewVirtualInSealed", 2); // "'{0}' is a new virtual member in sealed class '{1}'"
            ArgCountDic.Add("CSCSTR_ExplicitPropertyAddingAccessor", 2); // "'{0}' adds an accessor not found in interface member '{1}'"
            ArgCountDic.Add("CSCSTR_ExplicitPropertyMissingAccessor", 2); // "Explicit interface implementation '{0}' is missing accessor '{1}'"
            ArgCountDic.Add("CSCSTR_ConversionWithInterface", 1); // "'{0}': user-defined conversion to/from interface"
            ArgCountDic.Add("CSCSTR_ConversionWithBase", 1); // "'{0}': user-defined conversion to/from base class"
            ArgCountDic.Add("CSCSTR_ConversionWithDerived", 1); // "'{0}': user-defined conversion to/from derived class"
            ArgCountDic.Add("CSCSTR_IdentityConversion", 0); // "User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type"
            ArgCountDic.Add("CSCSTR_ConversionNotInvolvingContainedType", 0); // "User-defined conversion must convert to or from the enclosing type"
            ArgCountDic.Add("CSCSTR_DuplicateConversionInClass", 1); // "Duplicate user-defined conversion in type '{0}'"
            ArgCountDic.Add("CSCSTR_OperatorsMustBeStatic", 1); // "User-defined operator '{0}' must be declared static and public"
            ArgCountDic.Add("CSCSTR_BadIncDecsignature", 0); // "The parameter type for ++ or -- operator must be the containing type"
            ArgCountDic.Add("CSCSTR_MissingPredefinedMember", 2); // "Missing compiler required member '{0}.{1}'"
            ArgCountDic.Add("CSCSTR_AttributeLocationOnBadDeclaration", 2); // "'{0}' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are '{1}'. All attributes in this block will be ignored."
            ArgCountDic.Add("CSCSTR_InvalidAttributeLocation", 1); // "'{0}' is not a recognized attribute location. All attributes in this block will be ignored."
            ArgCountDic.Add("CSCSTR_EqualsWithoutGetHashCode", 1); // "'{0}' overrides Object.Equals(object o) but does not override Object.GetHashCode()"
            ArgCountDic.Add("CSCSTR_EqualityOpWithoutEquals", 1); // "'{0}' defines operator == or operator != but does not override Object.Equals(object o)"
            ArgCountDic.Add("CSCSTR_EqualityOpWithoutGetHashCode", 1); // "'{0}' defines operator == or operator != but does not override Object.GetHashCode()"
            ArgCountDic.Add("CSCSTR_OutAttrOnRefParam", 1); // "'{0}' cannot specify only Out attribute on a ref parameter. Use both In and Out attributes, or neither."
            ArgCountDic.Add("CSCSTR_OverloadRefOut", 1); // "'{0}' cannot define overloaded methods that differ only on ref and out"
            ArgCountDic.Add("CSCSTR_LiteralDoubleCast", 2); // "Literal of type double cannot be implicitly converted to type '{1}'; use an '{0}' suffix to create a literal of this type"
            ArgCountDic.Add("CSCSTR_IncorrectBooleanAssg", 0); // "Assignment in conditional expression is always constant; did you mean to use == instead of = ?"
            ArgCountDic.Add("CSCSTR_ProtectedInStruct", 1); // "'{0}': new protected member declared in struct"
            ArgCountDic.Add("CSCSTR_ProtectedInStatic", 1); // "'{0}': static classes cannot contain protected members"
            ArgCountDic.Add("CSCSTR_InconsistantIndexerNames", 0); // "Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type"
            ArgCountDic.Add("CSCSTR_ComImportWithUserCtor", 0); // "A class with the ComImport attribute cannot have a user-defined constructor"
            ArgCountDic.Add("CSCSTR_FieldCantHaveVoidType", 0); // "Field cannot have void type"
            ArgCountDic.Add("CSCSTR_AssignmentToSelf", 0); // "Assignment made to same variable; did you mean to assign something else?"
            ArgCountDic.Add("CSCSTR_ComparisonToSelf", 0); // "Comparison made to same variable; did you mean to compare something else?"
            ArgCountDic.Add("CSCSTR_BadUnaryOperatorSignature", 0); // "The parameter of a unary operator must be the containing type"
            ArgCountDic.Add("CSCSTR_BadBinaryOperatorSignature", 0); // "One of the parameters of a binary operator must be the containing type"
            ArgCountDic.Add("CSCSTR_BadShiftOperatorSignature", 0); // "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int"
            ArgCountDic.Add("CSCSTR_InterfacesCantContainOperators", 0); // "Interfaces cannot contain operators"
            ArgCountDic.Add("CSCSTR_StructsCantContainDefaultContructor", 0); // "Structs cannot contain explicit parameterless constructors"
            ArgCountDic.Add("CSCSTR_CantOverrideBogusMethod", 2); // "'{0}': cannot override '{1}' because it is not supported by the language"
            ArgCountDic.Add("CSCSTR_BindToBogus", 1); // "'{0}' is not supported by the language"
            ArgCountDic.Add("CSCSTR_CantCallSpecialMethod", 1); // "'{0}': cannot explicitly call operator or accessor"
            ArgCountDic.Add("CSCSTR_BadTypeReference", 2); // "'{0}': cannot reference a type through an expression; try '{1}' instead"
            ArgCountDic.Add("CSCSTR_FieldInitializerInStruct", 1); // "'{0}': cannot have instance field initializers in structs"
            ArgCountDic.Add("CSCSTR_BadDestructorName", 0); // "Name of destructor must match name of class"
            ArgCountDic.Add("CSCSTR_OnlyClassesCanContainDestructors", 0); // "Only class types can contain destructors"
            ArgCountDic.Add("CSCSTR_NoVoidParameter", 0); // "Invalid parameter type 'void'"
            ArgCountDic.Add("CSCSTR_DuplicateAlias", 1); // "The using alias '{0}' appeared previously in this namespace"
            ArgCountDic.Add("CSCSTR_BadProtectedAccess", 3); // "Cannot access protected member '{0}' via a qualifier of type '{1}'; the qualifier must be of type '{2}' (or derived from it)"
            ArgCountDic.Add("CSCSTR_CantIncludeDirectory", 1); // "Invalid reference option: '{0}' -- cannot reference directories"
            ArgCountDic.Add("CSCSTR_AliasMissingFile", 1); // "Invalid reference alias option: '{0}=' -- missing filename"
            ArgCountDic.Add("CSCSTR_AddModuleAssembly", 1); // "'{0}' cannot be added to this assembly because it already is an assembly; use '/R' option instead"
            ArgCountDic.Add("CSCSTR_ModuleNotScoped", 2); // "Added module '{0}' has unresolved reference to type '{1}'"
            ArgCountDic.Add("CSCSTR_ModuleNotAdded", 2); // "Added module '{1}' references module '{0}' which was not added"
            ArgCountDic.Add("CSCSTR_BindToBogusProp2", 3); // "Property, indexer, or event '{0}' is not supported by the language; try directly calling accessor methods '{1}' or '{2}'"
            ArgCountDic.Add("CSCSTR_BindToBogusProp1", 2); // "Property, indexer, or event '{0}' is not supported by the language; try directly calling accessor method '{1}'"
            ArgCountDic.Add("CSCSTR_NoVoidHere", 0); // "Keyword 'void' cannot be used in this context"
            ArgCountDic.Add("CSCSTR_CryptoFailed", 2); // "Cryptographic failure while signing assembly '{1}' -- '{0}'"
            ArgCountDic.Add("CSCSTR_CryptoNotFound", 0); // "Appropriate cryptographic service not found"
            ArgCountDic.Add("CSCSTR_IndexerNeedsParam", 0); // "Indexers must have at least one parameter"
            ArgCountDic.Add("CSCSTR_BadWarningLevel", 0); // "Warning level must be in the range 0-4"
            ArgCountDic.Add("CSCSTR_BadDebugType", 1); // "Invalid option '{0}' for /debug; must be full or pdbonly"
            ArgCountDic.Add("CSCSTR_BadPlatformType", 1); // "Invalid option '{0}' for /platform; must be anycpu, x86, Itanium or x64"
            ArgCountDic.Add("CSCSTR_UnknownTestSwitch", 1); // "Unrecognized value '{0}' provided for '/test' option"
            ArgCountDic.Add("CSCSTR_FixedNotInStruct", 0); // "Fixed size buffer fields may only be members of structs"
            ArgCountDic.Add("CSCSTR_StackOverflow", 1); // "An expression is too long or complex to compile near '{0}'"
            ArgCountDic.Add("CSCSTR_InvalidSearchPathDir", 3); // "Invalid search path '{0}' specified in '{1}' -- '{2}'"
            ArgCountDic.Add("CSCSTR_BadResourceVis", 1); // "Invalid option '{0}'; Resource visibility must be either 'public' or 'private'"
            ArgCountDic.Add("CSCSTR_DefaultValueTypeMustMatch", 0); // "The type of the argument to the DefaultValue attribute must match the parameter type"
            ArgCountDic.Add("CSCSTR_DefaultValueBadParamType", 1); // "The DefaultValue attribute is not applicable on parameters of type '{0}'"
            ArgCountDic.Add("CSCSTR_DefaultValueBadValueType", 1); // "Argument of type '{0}' is not applicable for the DefaultValue attribute"
            ArgCountDic.Add("CSCSTR_NonVirtualCallFromClosure", 1); // "Access to member '{0}' through a 'base' keyword from an anonymous method or iterator results in unverifiable code. Consider moving the access into a helper method on the containing type."
            ArgCountDic.Add("CSCSTR_CmdOptionConflictsSource", 2); // "Option '{0}' overrides attribute '{1}' given in a source file or added module"
            ArgCountDic.Add("CSCSTR_BadCompatMode", 1); // "Invalid option '{0}' for /langversion; must be ISO-1 or Default"
            ArgCountDic.Add("CSCSTR_BadWatsonMode", 1); // "Invalid option '{0}' for /errorreport; must be prompt, send, queue, or none"
            ArgCountDic.Add("CSCSTR_WatsonSendNotOptedIn", 1); // "Cannot send error report automatically without authorization. Please visit '{0}' to authorize sending error report."
            ArgCountDic.Add("CSCSTR_DelegateOnConditional", 1); // "Cannot create delegate with '{0}' because it has a Conditional attribute"
            ArgCountDic.Add("CSCSTR_CantMakeTempFile", 2); // "Cannot create temporary file '{1}' -- {0}"
            ArgCountDic.Add("CSCSTR_BadArgRef", 2); // "Argument '{0}' must be passed with the '{1}' keyword"
            ArgCountDic.Add("CSCSTR_YieldInAnonMeth", 0); // "The yield statement cannot be used inside anonymous method blocks"
            ArgCountDic.Add("CSCSTR_ReturnInIterator", 0); // "Cannot return a value from an iterator. Use the yield return statement to return a value, or yield break to end the iteration."
            ArgCountDic.Add("CSCSTR_BadIteratorArgType", 0); // "Iterators cannot have ref or out parameters"
            ArgCountDic.Add("CSCSTR_UnsafeIteratorArgType", 0); // "Iterators cannot have unsafe parameters or yield types"
            ArgCountDic.Add("CSCSTR_BadIteratorReturn", 2); // "The body of '{0}' cannot be an iterator block because '{1}' is not an iterator interface type"
            ArgCountDic.Add("CSCSTR_BadYieldInFinally", 0); // "Cannot yield in the body of a finally clause"
            ArgCountDic.Add("CSCSTR_BadYieldInCatch", 0); // "Cannot yield a value in the body of a catch clause"
            ArgCountDic.Add("CSCSTR_BadYieldInTryOfCatch", 0); // "Cannot yield a value in the body of a try block with a catch clause"
            ArgCountDic.Add("CSCSTR_EmptyYield", 0); // "Expression expected after yield return"
            ArgCountDic.Add("CSCSTR_VarargsIterator", 0); // "__arglist is not allowed in parameter list of iterators"
            ArgCountDic.Add("CSCSTR_IllegalVarArgs", 0); // "__arglist is not valid in this context"
            ArgCountDic.Add("CSCSTR_IllegalParams", 0); // "params is not valid in this context"
            ArgCountDic.Add("CSCSTR_DelegateNewMethBind", 3); // "Delegate '{0}' bound to '{1}' instead of '{2}' because of new language rules"
            ArgCountDic.Add("CSCSTR_DelegateOnNullable", 1); // "Cannot bind delegate to '{0}' because it is a member of 'System.Nullable<T>'"
            ArgCountDic.Add("CSCSTR_DocFileGen", 2); // "Error generating XML documentation file '{0}' ('{1}')"
            ArgCountDic.Add("CSCSTR_XMLParseError", 2); // "XML comment on '{1}' has badly formed XML -- '{0}'"
            ArgCountDic.Add("CSCSTR_DuplicateParamTag", 2); // "XML comment on '{1}' has a duplicate param tag for '{0}'"
            ArgCountDic.Add("CSCSTR_UnmatchedParamTag", 2); // "XML comment on '{1}' has a param tag for '{0}', but there is no parameter by that name"
            ArgCountDic.Add("CSCSTR_MissingParamTag", 2); // "Parameter '{0}' has no matching param tag in the XML comment for '{1}' (but other parameters do)"
            ArgCountDic.Add("CSCSTR_BadXMLRef", 2); // "XML comment on '{1}' has cref attribute '{0}' that could not be resolved"
            ArgCountDic.Add("CSCSTR_BadXMLRefTypeVar", 2); // "XML comment on '{1}' has cref attribute '{0}' that refers to a type parameter"
            ArgCountDic.Add("CSCSTR_BadStackAllocExpr", 0); // "A stackalloc expression requires [] after type"
            ArgCountDic.Add("CSCSTR_InvalidLineNumber", 0); // "The line number specified for #line directive is missing or invalid"
            ArgCountDic.Add("CSCSTR_ALinkFailed", 1); // "Assembly generation failed -- {0}"
            ArgCountDic.Add("CSCSTR_MissingPPFile", 0); // "Filename, single-line comment or end-of-line expected"
            ArgCountDic.Add("CSCSTR_ForEachMissingMember", 3); // "foreach statement cannot operate on variables of type '{0}' because '{1}' does not contain a public definition for '{2}'"
            ArgCountDic.Add("CSCSTR_MultipleIEnumOfT", 2); // "foreach statement cannot operate on variables of type '{0}' because it implements multiple instantiations of '{1}', try casting to a specific interface instantiation"
            ArgCountDic.Add("CSCSTR_BadXMLRefParamType", 2); // "Invalid type for parameter '{0}' in XML comment cref attribute: '{1}'"
            ArgCountDic.Add("CSCSTR_BadXMLRefReturnType", 0); // "Invalid return type in XML comment cref attribute"
            ArgCountDic.Add("CSCSTR_BadWin32Res", 1); // "'{0}' is not a valid Win32 resource file"
            ArgCountDic.Add("CSCSTR_DuplicateTypeParamTag", 2); // "XML comment on '{1}' has a duplicate typeparam tag for '{0}'"
            ArgCountDic.Add("CSCSTR_UnmatchedTypeParamTag", 2); // "XML comment on '{1}' has a typeparam tag for '{0}', but there is no type parameter by that name"
            ArgCountDic.Add("CSCSTR_MissingTypeParamTag", 2); // "Type parameter '{0}' has no matching typeparam tag in the XML comment on '{1}' (but other type parameters do)"
            ArgCountDic.Add("CSCSTR_InvalidDefaultCharSetValue", 0); // "Value specified for the argument to 'System.Runtime.InteropServices.DefaultCharSetAttribute' is not valid"
            ArgCountDic.Add("CSCSTR_CompileCancelled", 0); // "Compilation cancelled by user"
            ArgCountDic.Add("CSCSTR_MethodArgCantBeRefAny", 1); // "Method or delegate parameter cannot be of type '{0}'"
            ArgCountDic.Add("CSCSTR_AssgReadonlyLocal", 1); // "Cannot assign to '{0}' because it is read-only"
            ArgCountDic.Add("CSCSTR_RefReadonlyLocal", 1); // "Cannot pass '{0}' as a ref or out argument because it is read-only"
            ArgCountDic.Add("CSCSTR_AssgReadonlyLocal2", 1); // "Cannot modify members of '{0}' because it is read-only"
            ArgCountDic.Add("CSCSTR_RefReadonlyLocal2", 1); // "Cannot pass fields of '{0}' as a ref or out argument because it is read-only"
            ArgCountDic.Add("CSCSTR_AssgReadonlyLocal2Cause", 2); // "Cannot modify members of '{0}' because it is a '{1}'"
            ArgCountDic.Add("CSCSTR_RefReadonlyLocal2Cause", 2); // "Cannot pass fields of '{0}' as a ref or out argument because it is a '{1}'"
            ArgCountDic.Add("CSCSTR_AssgReadonlyLocalCause", 2); // "Cannot assign to '{0}' because it is a '{1}'"
            ArgCountDic.Add("CSCSTR_RefReadonlyLocalCause", 2); // "Cannot pass '{0}' as a ref or out argument because it is a '{1}'"
            ArgCountDic.Add("CSCSTR_ALinkCloseFailed", 1); // "Assembly signing failed; output may not be signed -- {0}"
            ArgCountDic.Add("CSCSTR_ALinkWarn", 1); // "Assembly generation -- {0}"
            ArgCountDic.Add("CSCSTR_CantUseRequiredAttribute", 0); // "The Required attribute is not permitted on C# types"
            ArgCountDic.Add("CSCSTR_NoModifiersOnAccessor", 0); // "Modifiers cannot be placed on event accessor declarations"
            ArgCountDic.Add("CSCSTR_DeleteAutoResFailed", 2); // "Unable to delete temporary file '{0}' used for default Win32 resource -- {1}"
            ArgCountDic.Add("CSCSTR_ParamsCantBeRefOut", 0); // "The params parameter cannot be declared as ref or out"
            ArgCountDic.Add("CSCSTR_ReturnNotLValue", 1); // "Cannot modify the return value of '{0}' because it is not a variable"
            ArgCountDic.Add("CSCSTR_MissingCoClass", 2); // "The managed coclass wrapper class '{0}' for interface '{1}' cannot be found (are you missing an assembly reference?)"
            ArgCountDic.Add("CSCSTR_BadCoClassSig", 2); // "The managed coclass wrapper class signature '{0}' for interface '{1}' is not a valid class name signature"
            ArgCountDic.Add("CSCSTR_BadArgExtraRef", 2); // "Argument '{0}' should not be passed with the '{1}' keyword"
            ArgCountDic.Add("CSCSTR_BadXMLRefSyntax", 2); // "XML comment on '{1}' has syntactically incorrect cref attribute '{0}'"
            ArgCountDic.Add("CSCSTR_BadModifierLocation", 1); // "Member modifier '{0}' must precede the member type and name"
            ArgCountDic.Add("CSCSTR_MissingArraySize", 0); // "Array creation must have array size or array initializer"
            ArgCountDic.Add("CSCSTR_UnprocessedXMLComment", 0); // "XML comment is not placed on a valid language element"
            ArgCountDic.Add("CSCSTR_CantGetCORSystemDir", 1); // "Cannot determine common language runtime directory -- '{0}'"
            ArgCountDic.Add("CSCSTR_FailedInclude", 3); // "Unable to include XML fragment '{1}' of file '{0}' -- {2}"
            ArgCountDic.Add("CSCSTR_InvalidInclude", 1); // "Invalid XML include element -- {0}"
            ArgCountDic.Add("CSCSTR_MissingXMLComment", 1); // "Missing XML comment for publicly visible type or member '{0}'"
            ArgCountDic.Add("CSCSTR_XMLParseIncludeError", 1); // "Badly formed XML in included comments file -- '{0}'"
            ArgCountDic.Add("CSCSTR_BadDelArgCount", 2); // "Delegate '{0}' does not take '{1}' arguments"
            ArgCountDic.Add("CSCSTR_BadDelArgTypes", 1); // "Delegate '{0}' has some invalid arguments"
            ArgCountDic.Add("CSCSTR_MultiplePredefTypes", 2); // "The predefined type '{0}' is defined in multiple assemblies in the global alias; using definition from '{1}'"
            ArgCountDic.Add("CSCSTR_XMLParserNotFound", 2); // "XML parser could not be loaded for the following reason: '{1}'. The XML documentation file '{0}' will not be generated."
            ArgCountDic.Add("CSCSTR_UnexpectedSemicolon", 0); // "Semicolon after method or accessor block is not valid"
            ArgCountDic.Add("CSCSTR_ERRORSYM", 0); // "<error>"
            ArgCountDic.Add("CSCSTR_NULL", 0); // "<null>"
            ArgCountDic.Add("CSCSTR_RELATEDERROR", 0); // "(Location of symbol related to previous error)"
            ArgCountDic.Add("CSCSTR_RELATEDWARNING", 0); // "(Location of symbol related to previous warning)"
            ArgCountDic.Add("CSCSTR_XMLIGNORED", 1); // "<!-- Badly formed XML comment ignored for member "{0}" -->"
            ArgCountDic.Add("CSCSTR_XMLIGNORED2", 1); // "<error><!-- Badly formed XML file "{0}" cannot be included --></error>"
            ArgCountDic.Add("CSCSTR_XMLFAILEDINCLUDE", 0); // " Failed to insert some or all of included XML "
            ArgCountDic.Add("CSCSTR_XMLBADINCLUDE", 0); // " Include tag is invalid "
            ArgCountDic.Add("CSCSTR_XMLNOINCLUDE", 0); // " No matching elements were found for the following include tag "
            ArgCountDic.Add("CSCSTR_XMLMISSINGINCLUDEFILE", 0); // "Missing file attribute"
            ArgCountDic.Add("CSCSTR_XMLMISSINGINCLUDEPATH", 0); // "Missing path attribute"
            ArgCountDic.Add("CSCSTR_GlobalNamespace", 0); // "<global namespace>"
            ArgCountDic.Add("CSCSTR_ThisAssembly", 0); // "<this assembly>"
            ArgCountDic.Add("CSCSTR_CLS_BadIdentifier", 1); // "Identifier '{0}' is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_BadBase", 2); // "'{0}': base type '{1}' is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_BadInterfacemember", 1); // "'{0}': CLS-compliant interfaces must have only CLS-compliant members"
            ArgCountDic.Add("CSCSTR_CLS_NoAbstractMembers", 1); // "'{0}': only CLS-compliant members can be abstract"
            ArgCountDic.Add("CSCSTR_CLS_NotOnModules", 0); // "You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking"
            ArgCountDic.Add("CSCSTR_CLS_ModuleMissingCLS", 0); // "Added modules must be marked with the CLSCompliant attribute to match the assembly"
            ArgCountDic.Add("CSCSTR_CLS_AssemblyNotCLS", 1); // "'{0}' cannot be marked as CLS-compliant because the assembly does not have a CLSCompliant attribute"
            ArgCountDic.Add("CSCSTR_CLS_AssemblyNotCLS2", 1); // "'{0}' does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute"
            ArgCountDic.Add("CSCSTR_CLS_BadAttributeType", 1); // "'{0}' has no accessible constructors which use only CLS-compliant types"
            ArgCountDic.Add("CSCSTR_CLS_ArrayArgumentToAttribute", 0); // "Arrays as attribute arguments is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_NotOnModules2", 0); // "You cannot specify the CLSCompliant attribute on a module that differs from the CLSCompliant attribute on the assembly"
            ArgCountDic.Add("CSCSTR_CLS_IllegalTrueInFalse", 2); // "'{0}' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type '{1}'"
            ArgCountDic.Add("CSCSTR_CLS_MeaninglessOnPrivateType", 1); // "CLS compliance checking will not be performed on '{0}' because it is not visible from outside this assembly."
            ArgCountDic.Add("CSCSTR_CLS_NoVarArgs", 0); // "Methods with variable arguments are not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_BadArgType", 1); // "Argument type '{0}' is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_BadReturnType", 1); // "Return type of '{0}' is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_BadFieldPropType", 1); // "Type of '{0}' is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_BadUnicode", 0); // "Mixed and decomposed Unicode characters are not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_BadIdentifierCase", 1); // "Identifier '{0}' differing only in case is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_OverloadRefOut", 1); // "Overloaded method '{0}' differing only in ref or out, or in array rank, is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_OverloadUnnamed", 1); // "Overloaded method '{0}' differing only by unnamed array types is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_MeaninglessOnParam", 0); // "CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead."
            ArgCountDic.Add("CSCSTR_CLS_MeaninglessOnReturn", 0); // "CLSCompliant attribute has no meaning when applied to return types. Try putting it on the method instead."
            ArgCountDic.Add("CSCSTR_CLS_BadTypeVar", 1); // "Constraint type '{0}' is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_CLS_VolatileField", 1); // "CLS-compliant field '{0}' cannot be volatile"
            ArgCountDic.Add("CSCSTR_CLS_BadInterface", 2); // "'{0}' is not CLS-compliant because base interface '{1}' is not CLS-compliant"
            ArgCountDic.Add("CSCSTR_BadArraySyntax", 0); // "Array type specifier, [], must appear before parameter name"
            ArgCountDic.Add("CSCSTR_BadOperatorSyntax", 1); // "Declaration is not valid; use '{0} operator <dest-type> (...' instead"
            ArgCountDic.Add("CSCSTR_BadOperatorSyntax2", 1); // "Declaration is not valid; use '<type> operator {0} (...' instead"
            ArgCountDic.Add("CSCSTR_MainClassNotFound", 1); // "Could not find '{0}' specified for Main method"
            ArgCountDic.Add("CSCSTR_MainClassNotClass", 1); // "'{0}' specified for Main method must be a valid class or struct"
            ArgCountDic.Add("CSCSTR_MainClassWrongFile", 1); // "Cannot use '{0}' for Main method because it is in a different output file"
            ArgCountDic.Add("CSCSTR_NoMainInClass", 1); // "'{0}' does not have a suitable static Main method"
            ArgCountDic.Add("CSCSTR_MainClassIsImport", 1); // "Cannot use '{0}' for Main method because it is imported"
            ArgCountDic.Add("CSCSTR_FileNameTooLong", 0); // "Invalid filename specified for preprocessor directive. Filename is too long or not a valid filename."
            ArgCountDic.Add("CSCSTR_EmptyFileName", 0); // "Filename specified for preprocessor directive is empty"
            ArgCountDic.Add("CSCSTR_OutputFileNameTooLong", 0); // "Output filename is too long or invalid"
            ArgCountDic.Add("CSCSTR_OutputNeedsName", 0); // "Outputs without source must have the /out option specified"
            ArgCountDic.Add("CSCSTR_OutputNeedsInput", 1); // "Output '{0}' does not have any source files"
            ArgCountDic.Add("CSCSTR_MethodReturnCantBeRefAny", 1); // "Method or delegate cannot return type '{0}'"
            ArgCountDic.Add("CSCSTR_CantHaveWin32ResAndIcon", 0); // "Conflicting options specified: Win32 resource file; Win32 icon"
            ArgCountDic.Add("CSCSTR_CantReadResource", 2); // "Error reading resource file '{0}' -- '{1}'"
            ArgCountDic.Add("CSCSTR_AutoResGen", 1); // "Error generating Win32 resource: {0}"
            ArgCountDic.Add("CSCSTR_CantOpenWin32Res", 2); // "Error reading Win32 resource file '{0}' -- '{1}'"
            ArgCountDic.Add("CSCSTR_ConflictAliasAndMember", 2); // "Namespace '{1}' contains a definition conflicting with alias '{0}'"
            ArgCountDic.Add("CSCSTR_ConditionalOnSpecialMethod", 1); // "The Conditional attribute is not valid on '{0}' because it is a constructor, destructor, operator, or explicit interface implementation"
            ArgCountDic.Add("CSCSTR_ConditionalMustReturnVoid", 1); // "The Conditional attribute is not valid on '{0}' because its return type is not void"
            ArgCountDic.Add("CSCSTR_DuplicateAttribute", 1); // "Duplicate '{0}' attribute"
            ArgCountDic.Add("CSCSTR_ConditionalOnInterfaceMethod", 0); // "The Conditional attribute is not valid on interface members"
            ArgCountDic.Add("CSCSTR_ICE_Culprit", 3); // "Internal Compiler Error (0x{0} at address {2}): likely culprit is '{1}'.\n\nAn internal error has occurred in the compiler. To work around this problem, try simplifying or changing the program near the locations listed below. Locations at the top of the list are closer to the point at which the internal error occurred. Errors such as this can be reported to Microsoft by using the /errorreport option.\n "
            ArgCountDic.Add("CSCSTR_ICE_Symbol", 2); // "Internal Compiler Error: stage '{1}' symbol '{0}'"
            ArgCountDic.Add("CSCSTR_ICE_Node", 1); // "Internal Compiler Error: stage '{0}'"
            ArgCountDic.Add("CSCSTR_ICE_File", 1); // "Internal Compiler Error: stage '{0}'"
            ArgCountDic.Add("CSCSTR_ICE_Stage", 1); // "Internal Compiler Error: stage '{0}'"
            ArgCountDic.Add("CSCSTR_ICE_Lexer", 0); // "Internal Compiler Error: stage 'LEX'"
            ArgCountDic.Add("CSCSTR_ICE_Parser", 0); // "Internal Compiler Error: stage 'PARSE'"
            ArgCountDic.Add("CSCSTR_OperatorCantReturnVoid", 0); // "User-defined operators cannot return void"
            ArgCountDic.Add("CSCSTR_InvalidAttributeArgument", 1); // "Invalid value for argument to '{0}' attribute"
            ArgCountDic.Add("CSCSTR_AttributeOnBadSymbolType", 2); // "Attribute '{0}' is not valid on this declaration type. It is valid on '{1}' declarations only."
            ArgCountDic.Add("CSCSTR_AttributeNotOnAccessor", 2); // "Attribute '{0}' is not valid on property or event accessors. It is valid on '{1}' declarations only."
            ArgCountDic.Add("CSCSTR_FloatOverflow", 1); // "Floating-point constant is outside the range of type '{0}'"
            ArgCountDic.Add("CSCSTR_ComImportWithoutUuidAttribute", 0); // "The Guid attribute must be specified with the ComImport attribute"
            ArgCountDic.Add("CSCSTR_InvalidNamedArgument", 1); // "Invalid value for named attribute argument '{0}'"
            ArgCountDic.Add("CSCSTR_DllImportOnInvalidMethod", 0); // "The DllImport attribute must be specified on a method marked 'static' and 'extern'"
            ArgCountDic.Add("CSCSTR_FeatureDeprecated", 2); // "The feature '{0}' is deprecated. Please use '{1}' instead'."
            ArgCountDic.Add("CSCSTR_NameAttributeOnOverride", 0); // "Cannot set the IndexerName attribute on an indexer marked override"
            ArgCountDic.Add("CSCSTR_FieldCantBeRefAny", 1); // "Field or property cannot be of type '{0}'"
            ArgCountDic.Add("CSCSTR_ArrayElementCantBeRefAny", 1); // "Array elements cannot be of type '{0}'"
            ArgCountDic.Add("CSCSTR_DeprecatedSymbol", 1); // "'{0}' is obsolete"
            ArgCountDic.Add("CSCSTR_NotAnAttributeClass", 1); // "'{0}' is not an attribute class"
            ArgCountDic.Add("CSCSTR_BadNamedAttributeArgument", 1); // "'{0}' is not a valid named attribute argument. Named attribute arguments must be fields which are not readonly, static, or const, or read-write properties which are public and not static."
            ArgCountDic.Add("CSCSTR_DeprecatedSymbolStr", 2); // "'{0}' is obsolete: '{1}'"
            ArgCountDic.Add("CSCSTR_IndexerCantHaveVoidType", 0); // "Indexers cannot have void type"
            ArgCountDic.Add("CSCSTR_VirtualPrivate", 1); // "'{0}': virtual or abstract members cannot be private"
            ArgCountDic.Add("CSCSTR_ArrayInitToNonArrayType", 0); // "Can only use array initializer expressions to assign to array types. Try using a new expression instead."
            ArgCountDic.Add("CSCSTR_ArrayInitInBadPlace", 0); // "Array initializers can only be used in a variable or field initializer. Try using a new expression instead."
            ArgCountDic.Add("CSCSTR_AttributeUsageOnNonAttributeClass", 1); // "Attribute '{0}' is only valid on classes derived from System.Attribute"
            ArgCountDic.Add("CSCSTR_ConditionalOnNonAttributeClass", 1); // "Attribute '{0}' is only valid on methods or attribute classes"
            ArgCountDic.Add("CSCSTR_PossibleMistakenNullStatement", 0); // "Possible mistaken empty statement"
            ArgCountDic.Add("CSCSTR_DuplicateNamedAttributeArgument", 1); // "'{0}' duplicate named attribute argument"
            ArgCountDic.Add("CSCSTR_DeriveFromEnumOrValueType", 2); // "'{0}' cannot derive from special class '{1}'"
            ArgCountDic.Add("CSCSTR_IdentifierTooLong", 0); // "Identifier too long"
            ArgCountDic.Add("CSCSTR_DefaultMemberOnIndexedType", 0); // "Cannot specify the DefaultMember attribute on a type containing an indexer"
            ArgCountDic.Add("CSCSTR_CustomAttributeError", 2); // "Error emitting '{1}' attribute -- '{0}'"
            ArgCountDic.Add("CSCSTR_BogusType", 1); // "'{0}' is a type not supported by the language"
            ArgCountDic.Add("CSCSTR_UnassignedInternalField", 2); // "Field '{0}' is never assigned to, and will always have its default value {1}"
            ArgCountDic.Add("CSCSTR_CStyleArray", 0); // "Syntax error, bad array declarator. To declare a managed array the rank specifier precedes the variable's identifier. To declare a fixed size buffer field, use the fixed keyword before the field type."
            ArgCountDic.Add("CSCSTR_VacuousIntegralComp", 1); // "Comparison to integral constant is useless; the constant is outside the range of type '{0}'"
            ArgCountDic.Add("CSCSTR_AbstractAttributeClass", 1); // "Cannot apply attribute class '{0}' because it is abstract"
            ArgCountDic.Add("CSCSTR_BadNamedAttributeArgumentType", 1); // "'{0}' is not a valid named attribute argument because it is not a valid attribute parameter type"
            ArgCountDic.Add("CSCSTR_MissingStructOffset", 1); // "'{0}': instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute"
            ArgCountDic.Add("CSCSTR_ExternMethodNoImplementation", 1); // "Method, operator, or accessor '{0}' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation."
            ArgCountDic.Add("CSCSTR_ProtectedInSealed", 1); // "'{0}': new protected member declared in sealed class"
            ArgCountDic.Add("CSCSTR_InterfaceImplementedByConditional", 3); // "Conditional member '{0}' cannot implement interface member '{1}' in type '{2}'"
            ArgCountDic.Add("CSCSTR_IllegalRefParam", 0); // "ref and out are not valid in this context"
            ArgCountDic.Add("CSCSTR_BadArgumentToAttribute", 1); // "The argument to the '{0}' attribute must be a valid identifier"
            ArgCountDic.Add("CSCSTR_OnlyValidOnCustomMarshaller", 1); // "'{0}': argument only valid for marshal of type System.Interop.UnmanagedType.CustomMarshaller"
            ArgCountDic.Add("CSCSTR_MissingComTypeOrMarshaller", 1); // "'{0}': System.Interop.UnmanagedType.CustomMarshaller requires named arguments ComType and Marshal"
            ArgCountDic.Add("CSCSTR_StructOffsetOnBadStruct", 0); // "The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)"
            ArgCountDic.Add("CSCSTR_StructOffsetOnBadField", 0); // "The FieldOffset attribute is not allowed on static or const fields"
            ArgCountDic.Add("CSCSTR_DoNotUseFixedBufferAttr", 0); // "Do not use 'System.Runtime.CompilerServices.FixedBuffer' attribute. Use the 'fixed' field modifier instead."
            ArgCountDic.Add("CSCSTR_IdentifierExpected", 0); // "Identifier expected"
            ArgCountDic.Add("CSCSTR_ExpectedVerbatimLiteral", 0); // "Keyword, identifier, or string expected after verbatim specifier: @"
            ArgCountDic.Add("CSCSTR_SemicolonExpected", 0); // "; expected"
            ArgCountDic.Add("CSCSTR_SyntaxError", 1); // "Syntax error, '{0}' expected"
            ArgCountDic.Add("CSCSTR_DuplicateModifier", 1); // "Duplicate '{0}' modifier"
            ArgCountDic.Add("CSCSTR_DuplicateAccessor", 0); // "Property accessor already defined"
            ArgCountDic.Add("CSCSTR_MissingTypeNested", 2); // "Reference to type '{0}' claims it is nested within '{1}', but it could not be found"
            ArgCountDic.Add("CSCSTR_MissingTypeInSource", 1); // "Reference to type '{0}' claims it is defined in this assembly, but it is not defined in source or any added modules"
            ArgCountDic.Add("CSCSTR_MissingTypeInAssembly", 2); // "Reference to type '{0}' claims it is defined in '{1}', but it could not be found"
            ArgCountDic.Add("CSCSTR_MemberNeedsType", 0); // "Class, struct, or interface method must have a return type"
            ArgCountDic.Add("CSCSTR_BadBaseType", 0); // "Invalid base type"
            ArgCountDic.Add("CSCSTR_BadConstraintType", 0); // "Invalid constraint type. A type used as a constraint must be an interface, a non-sealed class or a type parameter."
            ArgCountDic.Add("CSCSTR_EmptySwitch", 0); // "Empty switch block"
            ArgCountDic.Add("CSCSTR_ExpectedEndTry", 0); // "Expected catch or finally"
            ArgCountDic.Add("CSCSTR_InvalidExprTerm", 1); // "Invalid expression term '{0}'"
            ArgCountDic.Add("CSCSTR_BadNewExpr", 0); // "A new expression requires () or [] after type"
            ArgCountDic.Add("CSCSTR_NoNamespacePrivate", 0); // "Namespace elements cannot be explicitly declared as private, protected, or protected internal"
            ArgCountDic.Add("CSCSTR_BadVarDecl", 0); // "Expected ; or = (cannot specify constructor arguments in declaration)"
            ArgCountDic.Add("CSCSTR_UsingAfterElements", 0); // "A using clause must precede all other namespace elements except extern alias declarations"
            ArgCountDic.Add("CSCSTR_NoNewOnNamespaceElement", 0); // "Keyword new not allowed on namespace elements"
            ArgCountDic.Add("CSCSTR_DontUseInvoke", 0); // "Invoke cannot be called directly on a delegate"
            ArgCountDic.Add("CSCSTR_BadBinOpArgs", 1); // "Overloaded binary operator '{0}' takes two parameters"
            ArgCountDic.Add("CSCSTR_BadUnOpArgs", 1); // "Overloaded unary operator '{0}' takes one parameter"
            ArgCountDic.Add("CSCSTR_NoSourceFile", 2); // "Source file '{0}' could not be opened ('{1}')"
            ArgCountDic.Add("CSCSTR_CantRefResource", 1); // "Cannot link resource file '{0}' when building a module"
            ArgCountDic.Add("CSCSTR_ResourceNotUnique", 1); // "Resource identifier '{0}' has already been used in this assembly"
            ArgCountDic.Add("CSCSTR_ImportNonAssembly", 1); // "Referenced file '{0}' is not an assembly; use '/addmodule' option instead"
            ArgCountDic.Add("CSCSTR_RefLvalueExpected", 0); // "A ref or out argument must be an assignable variable"
            ArgCountDic.Add("CSCSTR_BaseInStaticMeth", 0); // "Keyword 'base' is not available in a static method"
            ArgCountDic.Add("CSCSTR_BaseInBadContext", 0); // "Keyword 'base' is not available in the current context"
            ArgCountDic.Add("CSCSTR_RbraceExpected", 0); // "} expected"
            ArgCountDic.Add("CSCSTR_LbraceExpected", 0); // "{ expected"
            ArgCountDic.Add("CSCSTR_InExpected", 0); // "'in' expected"
            ArgCountDic.Add("CSCSTR_InvalidPreprocExpr", 0); // "Invalid preprocessor expression"
            ArgCountDic.Add("CSCSTR_BadTokenInType", 0); // "Expected class, delegate, enum, interface, or struct"
            ArgCountDic.Add("CSCSTR_BadModifiersOnNamespace", 0); // "A namespace declaration cannot have modifiers or attributes"
            ArgCountDic.Add("CSCSTR_InvalidMemberDecl", 1); // "Invalid token '{0}' in class, struct, or interface member declaration"
            ArgCountDic.Add("CSCSTR_DuplicateParamName", 1); // "The parameter name '{0}' is a duplicate"
            ArgCountDic.Add("CSCSTR_DuplicateNameInNS", 2); // "The namespace '{1}' already contains a definition for '{0}'"
            ArgCountDic.Add("CSCSTR_DuplicateNameInClass", 2); // "The type '{1}' already contains a definition for '{0}'"
            ArgCountDic.Add("CSCSTR_NameNotInContext", 1); // "The name '{0}' does not exist in the current context"
            ArgCountDic.Add("CSCSTR_AmbigContext", 3); // "'{0}' is an ambiguous reference between '{1}' and '{2}'"
            ArgCountDic.Add("CSCSTR_DuplicateUsing", 1); // "The using directive for '{0}' appeared previously in this namespace"
            ArgCountDic.Add("CSCSTR_BadMemberFlag", 1); // "The modifier '{0}' is not valid for this item"
            ArgCountDic.Add("CSCSTR_BadMemberProtection", 0); // "More than one protection modifier"
            ArgCountDic.Add("CSCSTR_NewRequired", 2); // "'{0}' hides inherited member '{1}'. Use the new keyword if hiding was intended."
            ArgCountDic.Add("CSCSTR_NewNotRequired", 1); // "The member '{0}' does not hide an inherited member. The new keyword is not required."
            ArgCountDic.Add("CSCSTR_CircConstValue", 1); // "The evaluation of the constant value for '{0}' involves a circular definition"
            ArgCountDic.Add("CSCSTR_MemberAlreadyExists", 2); // "Type '{1}' already defines a member called '{0}' with the same parameter types"
            ArgCountDic.Add("CSCSTR_IntegralTypeExpected", 0); // "Type byte, sbyte, short, ushort, int, uint, long, or ulong expected"
            ArgCountDic.Add("CSCSTR_IllegalEscape", 0); // "Unrecognized escape sequence"
            ArgCountDic.Add("CSCSTR_NewlineInConst", 0); // "Newline in constant"
            ArgCountDic.Add("CSCSTR_EmptyCharConst", 0); // "Empty character literal"
            ArgCountDic.Add("CSCSTR_TooManyCharsInConst", 0); // "Too many characters in character literal"
            ArgCountDic.Add("CSCSTR_InvalidNumber", 0); // "Invalid number"
            ArgCountDic.Add("CSCSTR_GetOrSetExpected", 0); // "A get or set accessor expected"
            ArgCountDic.Add("CSCSTR_ClassTypeExpected", 0); // "An object, string, or class type expected"
            ArgCountDic.Add("CSCSTR_NamedArgumentExpected", 0); // "Named attribute argument expected"
            ArgCountDic.Add("CSCSTR_TooManyCatches", 0); // "Try statement already has an empty catch block"
            ArgCountDic.Add("CSCSTR_ThisOrBaseExpected", 0); // "Keyword 'this' or 'base' expected"
            ArgCountDic.Add("CSCSTR_OvlUnaryOperatorExpected", 0); // "Overloadable unary operator expected"
            ArgCountDic.Add("CSCSTR_OvlBinaryOperatorExpected", 0); // "Overloadable binary operator expected"
            ArgCountDic.Add("CSCSTR_IntOverflow", 0); // "Integral constant is too large"
            ArgCountDic.Add("CSCSTR_EOFExpected", 0); // "Type or namespace definition, or end-of-file expected"
            ArgCountDic.Add("CSCSTR_BadEmbeddedStmt", 0); // "Embedded statement cannot be a declaration or labeled statement"
            ArgCountDic.Add("CSCSTR_PPDirectiveExpected", 0); // "Preprocessor directive expected"
            ArgCountDic.Add("CSCSTR_EndOfPPLineExpected", 0); // "Single-line comment or end-of-line expected"
            ArgCountDic.Add("CSCSTR_CloseParenExpected", 0); // ") expected"
            ArgCountDic.Add("CSCSTR_EndifDirectiveExpected", 0); // "#endif directive expected"
            ArgCountDic.Add("CSCSTR_UnexpectedDirective", 0); // "Unexpected preprocessor directive"
            ArgCountDic.Add("CSCSTR_ErrorDirective", 1); // "#error: '{0}'"
            ArgCountDic.Add("CSCSTR_WarningDirective", 1); // "#warning: '{0}'"
            ArgCountDic.Add("CSCSTR_TypeExpected", 0); // "Type expected"
            ArgCountDic.Add("CSCSTR_PPDefFollowsToken", 0); // "Cannot define/undefine preprocessor symbols after first token in file"
            ArgCountDic.Add("CSCSTR_TooManyLines", 1); // "Compiler limit exceeded: File cannot exceed {0} lines"
            ArgCountDic.Add("CSCSTR_LineTooLong", 1); // "Compiler limit exceeded: Line cannot exceed {0} characters"
            ArgCountDic.Add("CSCSTR_OpenEndedComment", 0); // "End-of-file found, '*/' expected"
            ArgCountDic.Add("CSCSTR_ExpectedDotOrParen", 0); // "( or . expected"
            ArgCountDic.Add("CSCSTR_OvlOperatorExpected", 0); // "Overloadable operator expected"
            ArgCountDic.Add("CSCSTR_EndRegionDirectiveExpected", 0); // "#endregion directive expected"
            ArgCountDic.Add("CSCSTR_UnterminatedStringLit", 0); // "Unterminated string literal"
            ArgCountDic.Add("CSCSTR_BadDirectivePlacement", 0); // "Preprocessor directives must appear as the first non-whitespace character on a line"
            ArgCountDic.Add("CSCSTR_IdentifierExpectedKW", 2); // "Identifier expected, '{1}' is a keyword"
            ArgCountDic.Add("CSCSTR_SemiOrLBraceExpected", 0); // "{ or ; expected"
            ArgCountDic.Add("CSCSTR_MultiTypeInDeclaration", 0); // "Cannot use more than one type in a for, using, fixed, or declaration statement"
            ArgCountDic.Add("CSCSTR_AddOrRemoveExpected", 0); // "An add or remove accessor expected"
            ArgCountDic.Add("CSCSTR_UnexpectedCharacter", 1); // "Unexpected character '{0}'"
            ArgCountDic.Add("CSCSTR_BadDelegateLeave", 0); // "Control cannot leave the body of an anonymous method"
            ArgCountDic.Add("CSCSTR_IllegalPPWarning", 0); // "Expected disable or restore"
            ArgCountDic.Add("CSCSTR_IllegalPragma", 0); // "Unrecognized #pragma directive"
            ArgCountDic.Add("CSCSTR_BadRestoreNumber", 1); // "Cannot restore warning 'CS{0}' because it was disabled globally"
            ArgCountDic.Add("CSCSTR_IllegalPPChecksum", 0); // "Invalid #pragma checksum syntax; should be #pragma checksum "filename" "{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}" "XXXX...""
            ArgCountDic.Add("CSCSTR_ConflictingChecksum", 1); // "Different checksum values given for '{0}'"
            ArgCountDic.Add("CSCSTR_UnknownOption", 1); // "Unknown compiler option '{0}'"
            ArgCountDic.Add("CSCSTR_NoEntryPoint", 1); // "Program '{0}' does not contain a static 'Main' method suitable for an entry point"
            ArgCountDic.Add("CSCSTR_AnonDelegateCantUse", 1); // "Cannot use ref or out parameter '{0}' inside an anonymous method block"
            ArgCountDic.Add("CSCSTR_OPTDSC_WARNINGLEVEL", 0); // "Set warning level (0-4)"
            ArgCountDic.Add("CSCSTR_OPTDSC_WARNINGSAREERRORS", 0); // "Report all warnings as errors"
            ArgCountDic.Add("CSCSTR_OPTDSC_WARNASERRORLIST", 0); // "Report specific warnings as errors"
            ArgCountDic.Add("CSCSTR_OPTDSC_DEFINECCSYMBOLS", 0); // "Define conditional compilation symbol(s)"
            ArgCountDic.Add("CSCSTR_OPTDSC_NOSTDLIB", 0); // "Do not reference standard library (mscorlib.dll)"
            ArgCountDic.Add("CSCSTR_OD_EMITDEBUGINFO", 0); // "Emit debugging information"
            ArgCountDic.Add("CSCSTR_OPTDSC_OPTIMIZATIONS", 0); // "Enable optimizations"
            ArgCountDic.Add("CSCSTR_OPTDSC_REFERENCE", 0); // "Reference metadata from the specified assembly files"
            ArgCountDic.Add("CSCSTR_OD_ALIAS", 0); // "Reference metadata from the specified assembly file using the given alias"
            ArgCountDic.Add("CSCSTR_OD_NOCODEGEN", 0); // "Only check code for errors; do not emit executable"
            ArgCountDic.Add("CSCSTR_OD_TIMING", 0); // "Output mini-profile (timings of important code sections)"
            ArgCountDic.Add("CSCSTR_OD_INCBUILD", 0); // "Enable incremental compilation"
            ArgCountDic.Add("CSCSTR_OPTDSC_PLATFORM", 0); // "Limit which platforms this code can run on: x86, Itanium, x64, or anycpu. The default is anycpu."
            ArgCountDic.Add("CSCSTR_OD_MODULEASSEMBLY", 0); // "Name of the assembly which this module will be a part of."
            ArgCountDic.Add("CSCSTR_OPTDSC_ADDMODULE", 0); // "Link the specified modules into this assembly"
            ArgCountDic.Add("CSCSTR_OPTDSC_NOWARNLIST", 0); // "Disable specific warning messages"
            ArgCountDic.Add("CSCSTR_OPTDSC_XML_DOCFILE", 0); // "XML Documentation file to generate"
            ArgCountDic.Add("CSCSTR_OPTDSC_CHECKED", 0); // "Generate overflow checks"
            ArgCountDic.Add("CSCSTR_OPTDSC_UNSAFE", 0); // "Allow 'unsafe' code"
            ArgCountDic.Add("CSCSTR_FeatureGenerics", 0); // "generics"
            ArgCountDic.Add("CSCSTR_FeatureAnonDelegates", 0); // "anonymous methods"
            ArgCountDic.Add("CSCSTR_FeatureModuleAttrLoc", 0); // "module as an attribute target specifier"
            ArgCountDic.Add("CSCSTR_FeatureGlobalNamespace", 0); // "namespace alias qualifier"
            ArgCountDic.Add("CSCSTR_FeatureFixedBuffer", 0); // "fixed size buffers"
            ArgCountDic.Add("CSCSTR_FeaturePragma", 0); // "#pragma"
            ArgCountDic.Add("CSCSTR_FeatureStaticClasses", 0); // "static classes"
            ArgCountDic.Add("CSCSTR_FeaturePartialTypes", 0); // "partial types"
            ArgCountDic.Add("CSCSTR_FeatureSwitchOnBool", 0); // "switch on boolean type"
            ArgCountDic.Add("CSCSTR_NonECMAFeature", 1); // "Feature '{0}' cannot be used because it is not part of the standardized ISO C# language specification"
            ArgCountDic.Add("CSCSTR_NonECMAFeatureOK", 1); // "Feature '{0}' is not part of the standardized ISO C# language specification, and may not be accepted by other compilers"
            ArgCountDic.Add("CSCSTR_MethodGroup", 0); // "method group"
            ArgCountDic.Add("CSCSTR_AnonMethod", 0); // "anonymous method"
            ArgCountDic.Add("CSCSTR_Collection", 0); // "collection"
            ArgCountDic.Add("CSCSTR_FeaturePropertyAccessorMods", 0); // "access modifiers on properties"
            ArgCountDic.Add("CSCSTR_FeatureExternAlias", 0); // "extern alias"
            ArgCountDic.Add("CSCSTR_FeatureIterators", 0); // "iterators"
            ArgCountDic.Add("CSCSTR_FeatureDefault", 0); // "default operator"
            ArgCountDic.Add("CSCSTR_FeatureNullable", 0); // "nullable types"
            ArgCountDic.Add("CSCSTR_OPTDSC_NOLOGO", 0); // "Suppress compiler copyright message"
            ArgCountDic.Add("CSCSTR_OD_BUG", 0); // "Create a 'Bug Report' file."
            ArgCountDic.Add("CSCSTR_OPTDSC_CODEPAGE", 0); // "Specify the codepage to use when opening source files"
            ArgCountDic.Add("CSCSTR_OPTDSC_MAIN", 0); // "Specify the type that contains the entry point (ignore all other possible entry points)"
            ArgCountDic.Add("CSCSTR_OPTDSC_BASEADDRESS", 0); // "Base address for the library to be built"
            ArgCountDic.Add("CSCSTR_OPTDSC_FULLPATH", 0); // "Compiler generates fully qualified paths"
            ArgCountDic.Add("CSCSTR_OPTDSC_DEBUGTYPE", 0); // "Specify debugging type ('full' is default, and enables attaching a debugger to a running program)"
            ArgCountDic.Add("CSCSTR_OPTDSC_NOCONFIG", 0); // "Do not auto include CSC.RSP file"
            ArgCountDic.Add("CSCSTR_OPTDSC_LIBPATH", 0); // "Specify additional directories to search in for references"
            ArgCountDic.Add("CSCSTR_OPTDSC_ALIGN", 0); // "Specify the alignment used for output file sections"
            ArgCountDic.Add("CSCSTR_OPTDSC_UTF8OUTPUT", 0); // "Output compiler messages in UTF-8 encoding"
            ArgCountDic.Add("CSCSTR_OPTDSC_DELAYSIGN", 0); // "Delay-sign the assembly using only the public portion of the strong name key"
            ArgCountDic.Add("CSCSTR_OPTDSC_KEYFILE", 0); // "Specify a strong name key file"
            ArgCountDic.Add("CSCSTR_OPTDSC_KEYCONTAINER", 0); // "Specify a strong name key container"
            ArgCountDic.Add("CSCSTR_OPTDSC_COMPATIBILITY", 0); // "Specify language version mode: ISO-1 or Default"
            ArgCountDic.Add("CSCSTR_OD_WATSONMODE", 0); // "Specify how to handle internal compiler errors: prompt, send, queue, or none. The default is queue."
            ArgCountDic.Add("CSCSTR_OutputWriteFailed", 2); // "Could not write to output file '{1}' -- '{0}'"
            ArgCountDic.Add("CSCSTR_MultipleEntryPoints", 2); // "Program '{1}' has more than one entry point defined: '{0}'"
            ArgCountDic.Add("CSCSTR_UnimplementedOp", 1); // "Operator '{0}' is not yet implemented"
            ArgCountDic.Add("CSCSTR_BadBinaryOps", 3); // "Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'"
            ArgCountDic.Add("CSCSTR_IntDivByZero", 0); // "Division by constant zero"
            ArgCountDic.Add("CSCSTR_BadIndexLHS", 1); // "Cannot apply indexing with [] to an expression of type '{0}'"
            ArgCountDic.Add("CSCSTR_BadIndexCount", 1); // "Wrong number of indices inside [], expected '{0}'"
            ArgCountDic.Add("CSCSTR_BadUnaryOp", 2); // "Operator '{0}' cannot be applied to operand of type '{1}'"
            ArgCountDic.Add("CSCSTR_NoStdLib", 1); // "Standard library file '{0}' could not be found"
            ArgCountDic.Add("CSCSTR_ThisInStaticMeth", 0); // "Keyword 'this' is not valid in a static property, static method, or static field initializer"
            ArgCountDic.Add("CSCSTR_ThisInBadContext", 0); // "Keyword 'this' is not available in the current context"
            ArgCountDic.Add("CSCSTR_ThisStructNotInAnonMeth", 0); // "Anonymous methods inside structs cannot access instance members of 'this'. Consider copying 'this' to a local variable outside the anonymous method and using the local instead."
            ArgCountDic.Add("CSCSTR_InvalidMainSig", 1); // "'{0}' has the wrong signature to be an entry point"
            ArgCountDic.Add("CSCSTR_NoImplicitConv", 2); // "Cannot implicitly convert type '{0}' to '{1}'"
            ArgCountDic.Add("CSCSTR_NoImplicitConvCast", 2); // "Cannot implicitly convert type '{0}' to '{1}'. An explicit conversion exists (are you missing a cast?)"
            ArgCountDic.Add("CSCSTR_NoExplicitConv", 2); // "Cannot convert type '{0}' to '{1}'"
            ArgCountDic.Add("CSCSTR_ConstOutOfRange", 2); // "Constant value '{0}' cannot be converted to a '{1}'"
            ArgCountDic.Add("CSCSTR_AnonMethToNonDel", 1); // "Cannot convert anonymous method block to type '{0}' because it is not a delegate type"
            ArgCountDic.Add("CSCSTR_CantConvAnonMethParams", 1); // "Cannot convert anonymous method block to delegate type '{0}' because the specified block's parameter types do not match the delegate parameter types"
            ArgCountDic.Add("CSCSTR_CantConvAnonMethNoParams", 1); // "Cannot convert anonymous method block without a parameter list to delegate type '{0}' because it has one or more out parameters"
            ArgCountDic.Add("CSCSTR_CantConvAnonMethReturns", 1); // "Cannot convert anonymous method block to delegate type '{0}' because some of the return types in the block are not implicitly convertible to the delegate return type"
            ArgCountDic.Add("CSCSTR_NoConvToIDisp", 1); // "'{0}': type used in a using statement must be implicitly convertible to 'System.IDisposable'"
            ArgCountDic.Add("CSCSTR_InvalidGenericEnum", 0); // "Enums cannot have type parameters"
            ArgCountDic.Add("CSCSTR_StaticNotVirtual", 1); // "A static member '{0}' cannot be marked as override, virtual, or abstract"
            ArgCountDic.Add("CSCSTR_OverrideNotNew", 1); // "A member '{0}' marked as override cannot be marked as new or virtual"
            ArgCountDic.Add("CSCSTR_NewOrOverrideExpected", 2); // "'{0}' hides inherited member '{1}'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
            ArgCountDic.Add("CSCSTR_OverrideNotExpected", 1); // "'{0}': no suitable method found to override"
            ArgCountDic.Add("CSCSTR_NamespaceUnexpected", 0); // "A namespace does not directly contain members such as fields or methods"
            ArgCountDic.Add("CSCSTR_NoSuchMember", 2); // "'{0}' does not contain a definition for '{1}'"
            ArgCountDic.Add("CSCSTR_BadSKknown", 3); // "'{0}' is a '{1}' but is used like a '{2}'"
            ArgCountDic.Add("CSCSTR_BadSKunknown", 2); // "'{0}' is a '{1}', which is not valid in the given context"
            ArgCountDic.Add("CSCSTR_ObjectRequired", 1); // "An object reference is required for the nonstatic field, method, or property '{0}'"
            ArgCountDic.Add("CSCSTR_AmbigCall", 2); // "The call is ambiguous between the following methods or properties: '{0}' and '{1}'"
            ArgCountDic.Add("CSCSTR_BadAccess", 1); // "'{0}' is inaccessible due to its protection level"
            ArgCountDic.Add("CSCSTR_MethDelegateMismatch", 2); // "No overload for '{0}' matches delegate '{1}'"
            ArgCountDic.Add("CSCSTR_RetObjectRequired", 1); // "An object of a type convertible to '{0}' is required"
            ArgCountDic.Add("CSCSTR_RetNoObjectRequired", 1); // "Since '{0}' returns void, a return keyword must not be followed by an object expression"
            ArgCountDic.Add("CSCSTR_SK_METHOD", 0); // "method"
            ArgCountDic.Add("CSCSTR_SK_CLASS", 0); // "type"
            ArgCountDic.Add("CSCSTR_SK_NAMESPACE", 0); // "namespace"
            ArgCountDic.Add("CSCSTR_SK_FIELD", 0); // "field"
            ArgCountDic.Add("CSCSTR_SK_PROPERTY", 0); // "property"
            ArgCountDic.Add("CSCSTR_SK_UNKNOWN", 0); // "element"
            ArgCountDic.Add("CSCSTR_SK_VARIABLE", 0); // "variable"
            ArgCountDic.Add("CSCSTR_SK_EVENT", 0); // "event"
            ArgCountDic.Add("CSCSTR_SK_TYVAR", 0); // "type parameter"
            ArgCountDic.Add("CSCSTR_SK_ALIAS", 0); // "using alias"
            ArgCountDic.Add("CSCSTR_SK_EXTERNALIAS", 0); // "extern alias"
            ArgCountDic.Add("CSCSTR_AK_CLASS", 0); // "class"
            ArgCountDic.Add("CSCSTR_AK_DELEGATE", 0); // "delegate"
            ArgCountDic.Add("CSCSTR_AK_INTERFACE", 0); // "interface"
            ArgCountDic.Add("CSCSTR_AK_STRUCT", 0); // "struct"
            ArgCountDic.Add("CSCSTR_AK_ENUM", 0); // "enum"
            ArgCountDic.Add("CSCSTR_AK_UNKNOWN", 0); // "type"
            ArgCountDic.Add("CSCSTR_CHILD", 0); // "child"
            ArgCountDic.Add("CSCSTR_PARENT", 0); // "parent or current"
            ArgCountDic.Add("CSCSTR_LocalDuplicate", 1); // "A local variable named '{0}' is already defined in this scope"
            ArgCountDic.Add("CSCSTR_AssgLvalueExpected", 0); // "The left-hand side of an assignment must be a variable, property or indexer"
            ArgCountDic.Add("CSCSTR_StaticConstParam", 1); // "'{0}': a static constructor must be parameterless"
            ArgCountDic.Add("CSCSTR_NotConstantExpression", 1); // "The expression being assigned to '{0}' must be constant"
            ArgCountDic.Add("CSCSTR_NotNullConstRefField", 2); // "'{0}' is of type '{1}.' A const of reference type other than string can only be initialized with null"
            ArgCountDic.Add("CSCSTR_BadConstType", 1); // "The type '{0}' cannot be declared const"
            ArgCountDic.Add("CSCSTR_NameIllegallyOverrides", 2); // "'{1}' conflicts with the declaration '{0}'"
            ArgCountDic.Add("CSCSTR_LocalIllegallyOverrides", 2); // "A local variable named '{0}' cannot be declared in this scope because it would give a different meaning to '{0}', which is already used in a '{1}' scope to denote something else"
            ArgCountDic.Add("CSCSTR_BadUsingNamespace", 1); // "A using namespace directive can only be applied to namespaces; '{0}' is a type not a namespace"
            ArgCountDic.Add("CSCSTR_NoBreakOrCont", 0); // "No enclosing loop out of which to break or continue"
            ArgCountDic.Add("CSCSTR_DuplicateLabel", 1); // "The label '{0}' is a duplicate"
            ArgCountDic.Add("CSCSTR_NoConstructors", 1); // "The type '{0}' has no constructors defined"
            ArgCountDic.Add("CSCSTR_NoNewAbstract", 1); // "Cannot create an instance of the abstract class or interface '{0}'"
            ArgCountDic.Add("CSCSTR_ConstValueRequired", 0); // "A const field requires a value to be provided"
            ArgCountDic.Add("CSCSTR_FixedDimsRequired", 0); // "A fixed size buffer field must have the array size specifier after the field name"
            ArgCountDic.Add("CSCSTR_CircularBase", 2); // "Circular base class dependency involving '{0}' and '{1}'"
            ArgCountDic.Add("CSCSTR_BadDelegateConstructor", 1); // "The delegate '{0}' does not have a valid constructor"
            ArgCountDic.Add("CSCSTR_MethodNameExpected", 0); // "Method name expected"
            ArgCountDic.Add("CSCSTR_ConstantExpected", 0); // "A constant value is expected"
            ArgCountDic.Add("CSCSTR_IntegralTypeValueExpected", 0); // "A value of an integral type expected"
            ArgCountDic.Add("CSCSTR_DuplicateCaseLabel", 1); // "The label '{0}' already occurs in this switch statement"
            ArgCountDic.Add("CSCSTR_InvalidGotoCase", 0); // "A goto case is only valid inside a switch statement"
            ArgCountDic.Add("CSCSTR_PropertyLacksGet", 1); // "The property or indexer '{0}' cannot be used in this context because it lacks the get accessor"
            ArgCountDic.Add("CSCSTR_BadExceptionType", 0); // "The type caught or thrown must be derived from System.Exception"
            ArgCountDic.Add("CSCSTR_BadEmptyThrow", 0); // "A throw statement with no arguments is not allowed outside of a catch clause"
            ArgCountDic.Add("CSCSTR_BadEmptyThrowInFinally", 0); // "A throw statement with no arguments is not allowed inside of a finally clause nested inside of the innermost catch clause"
            ArgCountDic.Add("CSCSTR_BadFinallyLeave", 0); // "Control cannot leave the body of a finally clause"
            ArgCountDic.Add("CSCSTR_LabelShadow", 1); // "The label '{0}' shadows another label by the same name in a contained scope"
            ArgCountDic.Add("CSCSTR_LabelNotFound", 1); // "No such label '{0}' within the scope of the goto statement"
            ArgCountDic.Add("CSCSTR_AssignmentToLockOrDispose", 1); // "Possibly incorrect assignment to local '{0}' which is the argument to a using or lock statement. The Dispose call or unlocking will happen on the original value of the local."
            ArgCountDic.Add("CSCSTR_ForwardedTypeInThisAssembly", 1); // "Type '{0}' is defined in this assembly, but a type forwarder is specified for it"
            ArgCountDic.Add("CSCSTR_ForwardedTypeIsNested", 2); // "Cannot forward type '{0}' because it is a nested type of '{1}'"
            ArgCountDic.Add("CSCSTR_CycleInTypeForwarder", 2); // "The type forwarder for type '{0}' in assembly '{1}' causes a cycle"
            ArgCountDic.Add("CSCSTR_FwdedGeneric", 1); // "Cannot forward generic type, '{0}'"
            ArgCountDic.Add("CSCSTR_InvalidFwdType", 0); // "Invalid type specified as an argument for TypeForwardedTo attribute"
            ArgCountDic.Add("CSCSTR_NoNewTyvar", 1); // "Cannot create an instance of the variable type '{0}' because it does not have the new() constraint"
            ArgCountDic.Add("CSCSTR_BadArity", 3); // "Using the generic {1} '{0}' requires '{2}' type arguments"
            ArgCountDic.Add("CSCSTR_BadTypeArgument", 1); // "The type '{0}' may not be used as a type argument"
            ArgCountDic.Add("CSCSTR_TypeArgsNotAllowed", 2); // "The {1} '{0}' cannot be used with type arguments"
            ArgCountDic.Add("CSCSTR_HasNoTypeVars", 2); // "The non-generic {1} '{0}' cannot be used with type arguments"
            ArgCountDic.Add("CSCSTR_GenericConstraintNotSatisfied", 4); // "The type '{3}' must be convertible to '{1}' in order to use it as parameter '{2}' in the generic type or method '{0}'"
            ArgCountDic.Add("CSCSTR_NewConstraintNotSatisfied", 3); // "The type '{2}' must have a public parameterless constructor in order to use it as parameter '{1}' in the generic type or method '{0}'"
            ArgCountDic.Add("CSCSTR_GlobalSingleTypeNameNotFound", 1); // "The type or namespace name '{0}' could not be found in the global namespace (are you missing an assembly reference?)"
            ArgCountDic.Add("CSCSTR_NewBoundMustBeLast", 0); // "The new() constraint must be the last constraint specified"
            ArgCountDic.Add("CSCSTR_MainCantBeGeneric", 1); // "'{0}': an entry point cannot be generic or in a generic type"
            ArgCountDic.Add("CSCSTR_TypeVarCantBeNull", 1); // "Cannot convert null to type parameter '{0}' because it could be a value type. Consider using 'default({0})' instead."
            ArgCountDic.Add("CSCSTR_AttributeCantBeGeneric", 0); // "'<' unexpected : attributes cannot be generic"
            ArgCountDic.Add("CSCSTR_DuplicateBound", 2); // "Duplicate constraint '{0}' for type parameter '{1}'"
            ArgCountDic.Add("CSCSTR_ClassBoundNotFirst", 1); // "The class type constraint '{0}' must come before any other constraints"
            ArgCountDic.Add("CSCSTR_BadRetType", 2); // "'{1} {0}' has the wrong return type"
            ArgCountDic.Add("CSCSTR_DuplicateConstraintClause", 1); // "A constraint clause has already been specified for type parameter '{0}'. All of the constraints for a type parameter must be specified in a single where clause."
            ArgCountDic.Add("CSCSTR_WrongSignature", 1); // "No overload for '{0}' has the correct parameter and return types"
            ArgCountDic.Add("CSCSTR_CantInferMethTypeArgs", 1); // "The type arguments for method '{0}' cannot be inferred from the usage. Try specifying the type arguments explicitly."
            ArgCountDic.Add("CSCSTR_LocalSameNameAsTypeParam", 1); // "'{0}': a parameter or local variable cannot have the same name as a method type parameter"
            ArgCountDic.Add("CSCSTR_AsWithTypeVar", 1); // "The type parameter '{0}' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint"
            ArgCountDic.Add("CSCSTR_UnreferencedFieldAssg", 1); // "The private field '{0}' is assigned but its value is never used"
            ArgCountDic.Add("CSCSTR_BadIndexerNameAttr", 1); // "The '{0}' attribute is valid only on an indexer that is not an explicit interface member declaration"
            ArgCountDic.Add("CSCSTR_AttrArgWithTypeVars", 1); // "'{0}': an attribute argument cannot use type parameters"
            ArgCountDic.Add("CSCSTR_NewTyvarWithArgs", 1); // "'{0}': cannot provide arguments when creating an instance of a variable type"
            ArgCountDic.Add("CSCSTR_AbstractSealedStatic", 1); // "'{0}': an abstract class cannot be sealed or static"
            ArgCountDic.Add("CSCSTR_AmbiguousXMLReference", 3); // "Ambiguous reference in cref attribute: '{0}'. Assuming '{1}', but could have also matched other overloads including '{2}'."
            ArgCountDic.Add("CSCSTR_VolatileByRef", 1); // "'{0}': a reference to a volatile field will not be treated as volatile"
            ArgCountDic.Add("CSCSTR_IncrSwitchObsolete", 0); // "The /incremental option is no longer supported"
            ArgCountDic.Add("CSCSTR_ComImportWithImpl", 2); // "Since '{1}' has the ComImport attribute, '{0}' must be extern or abstract"
            ArgCountDic.Add("CSCSTR_ComImportWithBase", 1); // "'{0}': a class with the ComImport attribute cannot specify a base class"
            ArgCountDic.Add("CSCSTR_ImplBadConstraints", 4); // "The constraints for type parameter '{0}' of method '{1}' must match the constraints for type parameter '{2}' of interface method '{3}'. Consider using an explicit interface implementation instead."
            ArgCountDic.Add("CSCSTR_DottedTypeNameNotFoundInAgg", 2); // "The type name '{0}' does not exist in the type '{1}'"
            ArgCountDic.Add("CSCSTR_MethGrpToNonDel", 2); // "Cannot convert method group '{0}' to non-delegate type '{1}'. Did you intend to invoke the method?"
            ArgCountDic.Add("CSCSTR_UnreachableExpr", 0); // "Unreachable expression code detected"
            ArgCountDic.Add("CSCSTR_BadExternAlias", 1); // "The extern alias '{0}' was not specified in a /reference option"
            ArgCountDic.Add("CSCSTR_ColColWithTypeAlias", 1); // "Cannot use alias '{0}' with '::' since the alias references a type. Use '.' instead."
            ArgCountDic.Add("CSCSTR_AliasNotFound", 1); // "Alias '{0}' not found"
            ArgCountDic.Add("CSCSTR_SameFullNameAggAgg", 3); // "The type '{1}' exists in both '{0}' and '{2}'"
            ArgCountDic.Add("CSCSTR_SameFullNameNsAgg", 4); // "The namespace '{1}' in '{0}' conflicts with the type '{3}' in '{2}'"
            ArgCountDic.Add("CSCSTR_SameFullNameThisNsAgg", 4); // "The namespace '{1}' in '{0}' conflicts with the imported type '{3}' in '{2}'. Using the namespace."
            ArgCountDic.Add("CSCSTR_SameFullNameThisAggAgg", 4); // "The type '{1}' in '{0}' conflicts with the imported type '{3}' in '{2}'. Using the one in '{0}'."
            ArgCountDic.Add("CSCSTR_SameFullNameThisAggNs", 4); // "The type '{1}' in '{0}' conflicts with the imported namespace '{3}' in '{2}'. Using the type."
            ArgCountDic.Add("CSCSTR_SameFullNameThisAggThisNs", 4); // "The type '{1}' in '{0}' conflicts with the namespace '{3}' in '{2}'."
            ArgCountDic.Add("CSCSTR_ExternAfterElements", 0); // "An extern alias declaration must precede all other namespace elements"
            ArgCountDic.Add("CSCSTR_GlobalAliasDefn", 0); // "Defining an alias named 'global' is ill-advised since 'global::' always references the global namespace and not an alias"
            ArgCountDic.Add("CSCSTR_SealedStaticClass", 1); // "'{0}': a class cannot be both static and sealed"
            ArgCountDic.Add("CSCSTR_PrivateAbstractAccessor", 1); // "'{0}': abstract properties cannot have private accessors"
            ArgCountDic.Add("CSCSTR_ValueExpected", 0); // "Syntax error, value expected"
            ArgCountDic.Add("CSCSTR_UnexpectedPredefTypeLoc", 3); // "Predefined type '{0}' was not found in '{1}' but was found in '{2}'"
            ArgCountDic.Add("CSCSTR_UnboxNotLValue", 0); // "Cannot modify the result of an unboxing conversion"
            ArgCountDic.Add("CSCSTR_AnonMethGrpInForEach", 1); // "Foreach cannot operate on a '{0}'. Did you intend to invoke the '{0}'?"
            ArgCountDic.Add("CSCSTR_AttrOnTypeArg", 0); // "Attributes cannot be used on type arguments, only on type parameters"
            ArgCountDic.Add("CSCSTR_BadIncDecRetType", 0); // "The return type for ++ or -- operator must be the containing type or derived from the containing type"
            ArgCountDic.Add("CSCSTR_RefValBoundMustBeFirst", 0); // "The 'class' or 'struct' constraint must come before any other constraints"
            ArgCountDic.Add("CSCSTR_RefValBoundWithClass", 1); // "'{0}': cannot specify both a constraint class and the 'class' or 'struct' constraint"
            ArgCountDic.Add("CSCSTR_NewBoundWithVal", 0); // "The 'new()' constraint cannot be used with the 'struct' constraint"
            ArgCountDic.Add("CSCSTR_RefConstraintNotSatisfied", 3); // "The type '{2}' must be a reference type in order to use it as parameter '{1}' in the generic type or method '{0}'"
            ArgCountDic.Add("CSCSTR_ValConstraintNotSatisfied", 3); // "The type '{2}' must be a non-nullable value type in order to use it as parameter '{1}' in the generic type or method '{0}'"
            ArgCountDic.Add("CSCSTR_CircularConstraint", 2); // "Circular constraint dependency involving '{0}' and '{1}'"
            ArgCountDic.Add("CSCSTR_BaseConstraintConflict", 3); // "Type parameter '{0}' inherits conflicting constraints '{1}' and '{2}'"
            ArgCountDic.Add("CSCSTR_ConWithValCon", 2); // "Type parameter '{1}' has the 'struct' constraint so '{1}' cannot be used as a constraint for '{0}'"
            ArgCountDic.Add("CSCSTR_AmbigUDConv", 4); // "Ambiguous user defined conversions '{0}' and '{1}' when converting from '{2}' to '{3}'"
            ArgCountDic.Add("CSCSTR_AlwaysNull", 1); // "The result of the expression is always 'null' of type '{0}'"
            ArgCountDic.Add("CSCSTR_OverrideWithConstraints", 0); // "Constraints for override and explicit interface implementation methods are inherited from the base method, so they cannot be specified directly"
            ArgCountDic.Add("CSCSTR_AmbigOverride", 3); // "The inherited members '{0}' and '{1}' have the same signature in type '{2}', so they cannot be overridden"
            ArgCountDic.Add("CSCSTR_DecConstError", 1); // "Evaluation of the decimal constant expression failed with error: '{0}'"
            ArgCountDic.Add("CSCSTR_CmpAlwaysFalse", 1); // "Comparing with null of type '{0}' always produces 'false'"
            ArgCountDic.Add("CSCSTR_FinalizeMethod", 0); // "Introducing a 'Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?"
            ArgCountDic.Add("CSCSTR_ExplicitImplParams", 2); // "'{0}' should not have a params parameter since '{1}' does not"
            ArgCountDic.Add("CSCSTR_AmbigLookupMeth", 2); // "Ambiguity between method '{0}' and non-method '{1}'. Using method group."
            ArgCountDic.Add("CSCSTR_SameFullNameThisAggThisAgg", 4); // "Ambiguity between {0} '{1}' and {2} '{3}'"
            ArgCountDic.Add("CSCSTR_GotoCaseShouldConvert", 1); // "The 'goto case' value is not implicitly convertible to type '{0}'"
            ArgCountDic.Add("CSCSTR_MethodImplementingAccessor", 3); // "Method '{0}' cannot implement interface accessor '{1}' for type '{2}'. Use an explicit interface implementation."
            ArgCountDic.Add("CSCSTR_TypeArgsNotAllowedAmbig", 2); // "The {1} '{0}' is not a generic method. If you intended an expression list, use parentheses around the < expression."
            ArgCountDic.Add("CSCSTR_NubExprIsConstBool", 3); // "The result of the expression is always '{0}' since a value of type '{1}' is never equal to 'null' of type '{2}'"
            ArgCountDic.Add("CSCSTR_UnreachableCatch", 1); // "A previous catch clause already catches all exceptions of this or of a super type ('{0}')"
            ArgCountDic.Add("CSCSTR_UnreachableGeneralCatch", 0); // "A previous catch clause already catches all exceptions. All non-exceptions thrown will be wrapped in a System.Runtime.CompilerServices.RuntimeWrappedException"
            ArgCountDic.Add("CSCSTR_ReturnExpected", 1); // "'{0}': not all code paths return a value"
            ArgCountDic.Add("CSCSTR_AnonymousReturnExpected", 1); // "Anonymous method of type '{0}': not all code paths return a value"
            ArgCountDic.Add("CSCSTR_UnreachableCode", 0); // "Unreachable code detected"
            ArgCountDic.Add("CSCSTR_SwitchFallThrough", 1); // "Control cannot fall through from one case label ('{0}') to another"
            ArgCountDic.Add("CSCSTR_UnreferencedLabel", 0); // "This label has not been referenced"
            ArgCountDic.Add("CSCSTR_UseDefViolation", 1); // "Use of unassigned local variable '{0}'"
            ArgCountDic.Add("CSCSTR_SwitchFallInto", 1); // "Control cannot fall through from one case label to another ('{0}')"
            ArgCountDic.Add("CSCSTR_NoInvoke", 1); // "The delegate '{0}' is missing the Invoke method"
            ArgCountDic.Add("CSCSTR_UnreferencedVar", 1); // "The variable '{0}' is declared but never used"
            ArgCountDic.Add("CSCSTR_UnreferencedField", 1); // "The private field '{0}' is never used"
            ArgCountDic.Add("CSCSTR_UseDefViolationField", 1); // "Use of possibly unassigned field '{0}'"
            ArgCountDic.Add("CSCSTR_UnassignedThis", 1); // "Field '{0}' must be fully assigned before control leaves the constructor"
            ArgCountDic.Add("CSCSTR_AmbigQM", 2); // "Type of conditional expression cannot be determined because '{0}' and '{1}' implicitly convert to one another"
            ArgCountDic.Add("CSCSTR_InvalidQM", 2); // "Type of conditional expression cannot be determined because there is no implicit conversion between '{0}' and '{1}'"
            ArgCountDic.Add("CSCSTR_NoBaseClass", 0); // "A base class is required for a 'base' reference"
            ArgCountDic.Add("CSCSTR_BaseIllegal", 0); // "Use of keyword 'base' is not valid in this context"
            ArgCountDic.Add("CSCSTR_UseDefViolationOut", 1); // "Use of unassigned out parameter '{0}'"
            ArgCountDic.Add("CSCSTR_ArraySizeInDeclaration", 0); // "Array size cannot be specified in a variable declaration (try initializing with a 'new' expression)"
            ArgCountDic.Add("CSCSTR_InaccessibleGetter", 1); // "The property or indexer '{0}' cannot be used in this context because the get accessor is inaccessible"
            ArgCountDic.Add("CSCSTR_InaccessibleSetter", 1); // "The property or indexer '{0}' cannot be used in this context because the set accessor is inaccessible"
            ArgCountDic.Add("CSCSTR_InvalidPropertyAccessMod", 2); // "The accessibility modifier of the '{0}' accessor must be more restrictive than the property or indexer '{1}'"
            ArgCountDic.Add("CSCSTR_DuplicatePropertyAccessMods", 1); // "Cannot specify accessibility modifiers for both accessors of the property or indexer '{0}'"
            ArgCountDic.Add("CSCSTR_PropertyAccessModInInterface", 1); // "'{0}': accessibility modifiers may not be used on accessors in an interface"
            ArgCountDic.Add("CSCSTR_AccessModMissingAccessor", 1); // "'{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor"
            ArgCountDic.Add("CSCSTR_UnimplementedInterfaceAccessor", 3); // "'{0}' does not implement interface member '{1}'. '{2}' is not public."
            ArgCountDic.Add("CSCSTR_ObjectProhibited", 1); // "Static member '{0}' cannot be accessed with an instance reference; qualify it with a type name instead"
            ArgCountDic.Add("CSCSTR_ParamUnassigned", 1); // "The out parameter '{0}' must be assigned to before control leaves the current method"
            ArgCountDic.Add("CSCSTR_InvalidArray", 0); // "Invalid rank specifier: expected ',' or ']'"
            ArgCountDic.Add("CSCSTR_ExternHasBody", 1); // "'{0}' cannot be extern and declare a body"
            ArgCountDic.Add("CSCSTR_AbstractAndExtern", 1); // "'{0}' cannot be both extern and abstract"
            ArgCountDic.Add("CSCSTR_BadAttributeParam", 0); // "An attribute argument must be a constant expression, typeof expression or array creation expression"
            ArgCountDic.Add("CSCSTR_IsAlwaysTrue", 1); // "The given expression is always of the provided ('{0}') type"
            ArgCountDic.Add("CSCSTR_IsAlwaysFalse", 1); // "The given expression is never of the provided ('{0}') type"
            ArgCountDic.Add("CSCSTR_LockNeedsReference", 1); // "'{0}' is not a reference type as required by the lock statement"
            ArgCountDic.Add("CSCSTR_NullNotValid", 0); // "Use of null is not valid in this context"
            ArgCountDic.Add("CSCSTR_UseDefViolationThis", 0); // "The 'this' object cannot be used before all of its fields are assigned to"
            ArgCountDic.Add("CSCSTR_FeatureNYI", 1); // "The feature you are attempting to use, '{0}', has not been implemented. Please refrain from using it until a later time."
            ArgCountDic.Add("CSCSTR_FeatureNYI2", 1); // "The feature you are attempting to use, '{0}', may not be fully implemented by the compiler and/or runtime. Proceed at your own risk."
            ArgCountDic.Add("CSCSTR_ArgsInvalid", 0); // "The __arglist construct is valid only within a variable argument method"
            ArgCountDic.Add("CSCSTR_AssgReadonly", 0); // "A readonly field cannot be assigned to (except in a constructor or a variable initializer)"
            ArgCountDic.Add("CSCSTR_AssgReadonly2", 1); // "Members of readonly field '{0}' cannot be modified (except in a constructor or a variable initializer)"
            ArgCountDic.Add("CSCSTR_RefReadonly", 0); // "A readonly field cannot be passed ref or out (except in a constructor)"
            ArgCountDic.Add("CSCSTR_RefReadonly2", 1); // "Members of readonly field '{0}' cannot be passed ref or out (except in a constructor)"
            ArgCountDic.Add("CSCSTR_PtrExpected", 0); // "The * or -> operator must be applied to a pointer"
            ArgCountDic.Add("CSCSTR_PtrIndexSingle", 0); // "A pointer must be indexed by only one value"
            ArgCountDic.Add("CSCSTR_ByRefNonAgileField", 1); // "Passing '{0}' as ref or out or taking its address may cause a runtime exception because it is a field of a marshal-by-reference class"
            ArgCountDic.Add("CSCSTR_CallOnNonAgileField", 1); // "Accessing a member on '{0}' may cause a runtime exception because it is a field of a marshal-by-reference class"
            ArgCountDic.Add("CSCSTR_AssgReadonlyStatic", 0); // "A static readonly field cannot be assigned to (except in a static constructor or a variable initializer)"
            ArgCountDic.Add("CSCSTR_RefReadonlyStatic", 0); // "A static readonly field cannot be passed ref or out (except in a static constructor)"
            ArgCountDic.Add("CSCSTR_AssgReadonlyStatic2", 1); // "Fields of static readonly field '{0}' cannot be assigned to (except in a static constructor or a variable initializer)"
            ArgCountDic.Add("CSCSTR_RefReadonlyStatic2", 1); // "Fields of static readonly field '{0}' cannot be passed ref or out (except in a static constructor)"
            ArgCountDic.Add("CSCSTR_AssgReadonlyProp", 1); // "Property or indexer '{0}' cannot be assigned to -- it is read only"
            ArgCountDic.Add("CSCSTR_IllegalStatement", 0); // "Only assignment, call, increment, decrement, and new object expressions can be used as a statement"
            ArgCountDic.Add("CSCSTR_BadGetEnumerator", 2); // "foreach requires that the return type '{0}' of '{1}' must have a suitable public MoveNext method and public Current property"
            ArgCountDic.Add("CSCSTR_TooManyLocals", 0); // "Only 65535 locals are allowed"
            ArgCountDic.Add("CSCSTR_AbstractBaseCall", 1); // "Cannot call an abstract base member: '{0}'"
            ArgCountDic.Add("CSCSTR_RefProperty", 0); // "A property or indexer may not be passed as an out or ref parameter"
            ArgCountDic.Add("CSCSTR_AmbigBinaryOps", 3); // "Operator '{0}' is ambiguous on operands of type '{1}' and '{2}'"
            ArgCountDic.Add("CSCSTR_AmbigUnaryOp", 2); // "Operator '{0}' is ambiguous on an operand of type '{1}'"
            ArgCountDic.Add("CSCSTR_InAttrOnOutParam", 0); // "An out parameter cannot have the In attribute"
            ArgCountDic.Add("CSCSTR_ValueCantBeNull", 1); // "Cannot convert null to '{0}' because it is a value type"
            ArgCountDic.Add("CSCSTR_WrongNestedThis", 2); // "Cannot access a nonstatic member of outer type '{0}' via nested type '{1}'"
            ArgCountDic.Add("CSCSTR_NoExplicitBuiltinConv", 2); // "Cannot convert type '{0}' to '{1}' via a built-in conversion"
            ArgCountDic.Add("CSCSTR_DebugInit", 1); // "Unexpected debug information initialization error -- '{0}'"
            ArgCountDic.Add("CSCSTR_DebugEmitFailure", 2); // "Unexpected error writing debug information to file '{1}' -- '{0}'"
            ArgCountDic.Add("CSCSTR_DebugInitFile", 2); // "Unexpected error creating debug information file '{0}' -- '{1}'"
            ArgCountDic.Add("CSCSTR_BadPDBFormat", 1); // "PDB file '{0}' has an incorrect or out-of-date format. Delete it and rebuild."
            ArgCountDic.Add("CSCSTR_BadArgCount", 2); // "No overload for method '{0}' takes '{1}' arguments"
            ArgCountDic.Add("CSCSTR_BadArgTypes", 1); // "The best overloaded method match for '{0}' has some invalid arguments"
            ArgCountDic.Add("CSCSTR_BadArgType", 3); // "Argument '{0}': cannot convert from '{1}' to '{2}'"
            ArgCountDic.Add("CSCSTR_BadParamType", 3); // "Parameter '{0}' is declared as type '{1}' but should be '{2}'"
            ArgCountDic.Add("CSCSTR_BadParamRef", 2); // "Parameter '{0}' must be declared with the '{1}' keyword"
            ArgCountDic.Add("CSCSTR_BadParamExtraRef", 2); // "Parameter '{0}' should not be declared with the '{1}' keyword"
            ArgCountDic.Add("CSCSTR_BadVisReturnType", 2); // "Inconsistent accessibility: return type '{1}' is less accessible than method '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisParamType", 2); // "Inconsistent accessibility: parameter type '{1}' is less accessible than method '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisFieldType", 2); // "Inconsistent accessibility: field type '{1}' is less accessible than field '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisPropertyType", 2); // "Inconsistent accessibility: property type '{1}' is less accessible than property '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisIndexerReturn", 2); // "Inconsistent accessibility: indexer return type '{1}' is less accessible than indexer '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisIndexerParam", 2); // "Inconsistent accessibility: parameter type '{1}' is less accessible than indexer '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisOpReturn", 2); // "Inconsistent accessibility: return type '{1}' is less accessible than operator '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisOpParam", 2); // "Inconsistent accessibility: parameter type '{1}' is less accessible than operator '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisDelegateReturn", 2); // "Inconsistent accessibility: return type '{1}' is less accessible than delegate '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisDelegateParam", 2); // "Inconsistent accessibility: parameter type '{1}' is less accessible than delegate '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisBaseClass", 2); // "Inconsistent accessibility: base class '{1}' is less accessible than class '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisBaseInterface", 2); // "Inconsistent accessibility: base interface '{1}' is less accessible than interface '{0}'"
            ArgCountDic.Add("CSCSTR_ManagedAddr", 1); // "Cannot take the address of, get the size of, or declare a pointer to a managed type ('{0}')"
            ArgCountDic.Add("CSCSTR_BadFixedInitType", 0); // "The type of locals declared in a fixed statement must be a pointer type"
            ArgCountDic.Add("CSCSTR_FixedMustInit", 0); // "You must provide an initializer in a fixed or using statement declaration"
            ArgCountDic.Add("CSCSTR_InvalidAddrOp", 0); // "Cannot take the address of the given expression"
            ArgCountDic.Add("CSCSTR_AddrOnReadOnlyLocal", 0); // "Cannot take the address of a read-only local variable"
            ArgCountDic.Add("CSCSTR_FixedNeeded", 0); // "You can only take the address of an unfixed expression inside of a fixed statement initializer"
            ArgCountDic.Add("CSCSTR_FixedBufferNotFixed", 0); // "You cannot use fixed size buffers contained in unfixed expressions. Try using the fixed statement."
            ArgCountDic.Add("CSCSTR_FixedNeedsLvalue", 0); // "Fixed size buffers can only be accessed through locals or fields"
            ArgCountDic.Add("CSCSTR_FixedNotNeeded", 0); // "You cannot use the fixed statement to take the address of an already fixed expression"
            ArgCountDic.Add("CSCSTR_UnsafeNeeded", 0); // "Pointers and fixed size buffers may only be used in an unsafe context"
            ArgCountDic.Add("CSCSTR_OpTFRetType", 0); // "The return type of operator True or False must be bool"
            ArgCountDic.Add("CSCSTR_OperatorNeedsMatch", 2); // "The operator '{0}' requires a matching operator '{1}' to also be defined"
            ArgCountDic.Add("CSCSTR_BadBoolOp", 1); // "In order to be applicable as a short circuit operator a user-defined logical operator ('{0}') must have the same return type as the type of its 2 parameters"
            ArgCountDic.Add("CSCSTR_MustHaveOpTF", 1); // "The type ('{0}') must contain declarations of operator true and operator false"
            ArgCountDic.Add("CSCSTR_UnreferencedVarAssg", 1); // "The variable '{0}' is assigned but its value is never used"
            ArgCountDic.Add("CSCSTR_CheckedOverflow", 0); // "The operation overflows at compile time in checked mode"
            ArgCountDic.Add("CSCSTR_ConstOutOfRangeChecked", 2); // "Constant value '{0}' cannot be converted to a '{1}' (use 'unchecked' syntax to override)"
            ArgCountDic.Add("CSCSTR_EventNeedsBothAccessors", 1); // "'{0}': event property must have both add and remove accessors"
            ArgCountDic.Add("CSCSTR_EventNotDelegate", 1); // "'{0}': event must be of a delegate type"
            ArgCountDic.Add("CSCSTR_UnreferencedEvent", 1); // "The event '{0}' is never used"
            ArgCountDic.Add("CSCSTR_InterfaceEventInitializer", 1); // "'{0}': event in interface cannot have initializer"
            ArgCountDic.Add("CSCSTR_EventPropertyInInterface", 0); // "An event in an interface cannot have add or remove accessors"
            ArgCountDic.Add("CSCSTR_BadEventUsage", 2); // "The event '{0}' can only appear on the left hand side of += or -= (except when used from within the type '{1}')"
            ArgCountDic.Add("CSCSTR_ExplicitEventFieldImpl", 0); // "An explicit interface implementation of an event must use property syntax"
            ArgCountDic.Add("CSCSTR_CantOverrideNonEvent", 2); // "'{0}': cannot override; '{1}' is not an event"
            ArgCountDic.Add("CSCSTR_AddRemoveMustHaveBody", 0); // "An add or remove accessor must have a body"
            ArgCountDic.Add("CSCSTR_AbstractEventInitializer", 1); // "'{0}': abstract event cannot have initializer"
            ArgCountDic.Add("CSCSTR_PossibleBadNegCast", 0); // "To cast a negative value, you must enclose the value in parentheses"
            ArgCountDic.Add("CSCSTR_ReservedEnumerator", 1); // "The enumerator name '{0}' is reserved and cannot be used"
            ArgCountDic.Add("CSCSTR_AsMustHaveReferenceType", 1); // "The as operator must be used with a reference type ('{0}' is a value type)"
            ArgCountDic.Add("CSCSTR_LowercaseEllSuffix", 0); // "The 'l' suffix is easily confused with the digit '1' -- use 'L' for clarity"
            ArgCountDic.Add("CSCSTR_BadEventUsageNoField", 1); // "The event '{0}' can only appear on the left hand side of += or -="
            ArgCountDic.Add("CSCSTR_ConstraintOnlyAllowedOnGenericDecl", 0); // "Constraints are not allowed on non-generic declarations"
            ArgCountDic.Add("CSCSTR_TypeParamMustBeIdentifier", 0); // "Type parameter declaration must be an identifier not a type"
            ArgCountDic.Add("CSCSTR_BadWarningNumber", 1); // "'{0}' is not a valid warning number"
            ArgCountDic.Add("CSCSTR_AmbigousAttribute", 3); // "'{0}' is ambiguous between '{1}' and '{2}'; use either '@{0}' or '{0}Attribute'"
            ArgCountDic.Add("CSCSTR_ErrorOverride", 2); // "{0}. See also error CS{1}."
            ArgCountDic.Add("CSCSTR_ReservedIdentifier2", 1); // "'{0}' is a reserved identifier and cannot be used when ISO language version mode is used"
            ArgCountDic.Add("CSCSTR_IllegalFixedType", 0); // "Fixed size buffer type must be one of the following: bool, byte, short, int, long, char, sbyte, ushort, uint, ulong, float or double"
            ArgCountDic.Add("CSCSTR_FixedOverflow", 2); // "Fixed size buffer of length '{0}' and type '{1}' is too big"
            ArgCountDic.Add("CSCSTR_InvalidFixedArraySize", 0); // "Fixed size buffers must have a length greater than zero"
            ArgCountDic.Add("CSCSTR_BANNER1", 0); // "Microsoft (R) Shared Source CLI C# Compiler version "
            ArgCountDic.Add("CSCSTR_BANNER1PART2", 0); // "for Microsoft (R) Shared Source CLI version "
            ArgCountDic.Add("CSCSTR_BANNER2", 0); // "Copyright (C) Microsoft Corporation. All rights reserved."
            ArgCountDic.Add("CSCSTR_VERSION_CONFLICT", 4); // "The following two files are marked as incompatible:\n\n\t{0}\n\t(Version {1})\n\n\t{2}\n\t(Version {3})\n\nThe compiler cannot be used in this configuration. This is usually caused by installing different flavors of VS and NDP, and not specifying the VS version for the /CompilerSourcePath and /NonNdpSourcePath options when running NdpSetup"
            ArgCountDic.Add("CSCSTR_DEBUG_CONFLICT", 2); // "The {0} compiler cannot be used with a {1} IDE. This is usually caused by installing different flavors of VS and NDP, and not specifying the VS flavor for the /CompilerSourcePath and /NonNdpSourcePath options when running NdpSetup."
            ArgCountDic.Add("CSCSTR_INITERROR", 1); // "Compiler initialization failed unexpectedly -- '{0}'"
            ArgCountDic.Add("CSCSTR_FILENOTFOUND", 1); // "Source file '{0}' could not be found"
            ArgCountDic.Add("CSCSTR_NORESPONSEFILE", 2); // "Error opening response file '{0}' -- '{1}'"
            ArgCountDic.Add("CSCSTR_NOFILESPEC", 1); // "Missing file specification for '{0}' option"
            ArgCountDic.Add("CSCSTR_BADSWITCH", 1); // "Unrecognized option: '{0}'"
            ArgCountDic.Add("CSCSTR_NOSOURCES", 0); // "No inputs specified"
            ArgCountDic.Add("CSCSTR_SWITCHNEEDSSTRING", 1); // "Command-line syntax error: Missing ':<text>' for '{0}' option"
            ArgCountDic.Add("CSCSTR_SwitchNeedsNumber", 1); // "Command-line syntax error: Missing ':<number>' for '{0}' option"
            ArgCountDic.Add("CSCSTR_DUPLICATERESPONSEFILE", 1); // "Response file '{0}' included multiple times"
            ArgCountDic.Add("CSCSTR_FILEALREADYINCLUDED", 1); // "Source file '{0}' specified multiple times"
            ArgCountDic.Add("CSCSTR_IllegalOptionChar", 1); // "Character '{0}' is not allowed on the command-line or in response files"
            ArgCountDic.Add("CSCSTR_CANTOPENFILEWRITE", 1); // "Cannot open '{0}' for writing"
            ArgCountDic.Add("CSCSTR_BADBASENUMBER", 1); // "Invalid image base number '{0}'"
            ArgCountDic.Add("CSCSTR_USENEWSWITCH", 2); // "Compiler option '{0}' is obsolete, please use '{1}' instead"
            ArgCountDic.Add("CSCSTR_BINARYFILE", 1); // "'{0}' is a binary file instead of a text file"
            ArgCountDic.Add("CSCSTR_BADCODEPAGE", 1); // "Code page '{0}' is invalid or not installed"
            ArgCountDic.Add("CSCSTR_NOMAINONDLL", 0); // "Cannot specify /main if building a module or library"
            ArgCountDic.Add("CSCSTR_INVALIDTARGET", 0); // "Invalid target type for /target: must specify 'exe', 'winexe', 'library', or 'module'"
            ArgCountDic.Add("CSCSTR_BADSECONDTARGET", 0); // "Only the first set of input files can build a target other than 'module'"
            ArgCountDic.Add("CSCSTR_InputFileNameTooLong", 1); // "File name '{0}' is too long or invalid"
            ArgCountDic.Add("CSCSTR_OutputFileExists", 1); // "Cannot create short filename '{0}' when a long filename with the same short filename already exists"
            ArgCountDic.Add("CSCSTR_NoSourcesInLastInputSet", 0); // "Options '/out' and '/target' must appear before source file names"
            ArgCountDic.Add("CSCSTR_NoConfigNotOnCommandLine", 0); // "Ignoring /noconfig option because it was specified in a response file"
            ArgCountDic.Add("CSCSTR_BadFileAlignment", 1); // "Invalid file section alignment number '{0}'"
            ArgCountDic.Add("CSCSTR_NoDebugSwitchSourceMap", 0); // "Must emit debug info with /sourcemap. Are you missing '/debug'?"
            ArgCountDic.Add("CSCSTR_SourceMapFileBinary", 1); // "Sourcemap file '{0}' is a binary file instead of a text file"
            ArgCountDic.Add("CSCSTR_DefineIdentifierRequired", 1); // "Invalid value for '/define'; '{0}' is not a valid identifier"
            ArgCountDic.Add("CSCSTR_BadExternIdentifier", 1); // "Invalid extern alias for '/reference'; '{0}' is not a valid identifier"
            ArgCountDic.Add("CSCSTR_GlobalExternAlias", 0); // "You cannot redefine the global extern alias"
            ArgCountDic.Add("CSCSTR_InvalidSourceMap", 1); // "Sourcemap file is invalid; there was an error on line {0}"
            ArgCountDic.Add("CSCSTR_NoSourceMapFile", 1); // "Unable to open source map file '{0}'"
            ArgCountDic.Add("CSCSTR_OneAliasPerRefernce", 0); // "A /reference option that declares an extern alias can only have one filename. To specify multiple aliases or filenames, use multiple /reference options."
            ArgCountDic.Add("CSCSTR_AssemblyNameOnNonModule", 0); // "The /moduleassemblyname option may only be specified when building a target type of 'module'"
            ArgCountDic.Add("CSCSTR_HELP10", 0); // "Visual C# 2005 Compiler Options"
            ArgCountDic.Add("CSCSTR_SHORTFORM", 0); // "Short form"
            ArgCountDic.Add("CSCSTR_OPTDSC_TARGET_MODULE", 0); // "Build a module that can be added to another assembly"
            ArgCountDic.Add("CSCSTR_OPTDSC_TARGET_EXE", 0); // "Build a console executable (default)"
            ArgCountDic.Add("CSCSTR_OPTDSC_TARGET_WINEXE", 0); // "Build a Windows executable"
            ArgCountDic.Add("CSCSTR_OPTDSC_TARGET_DLL", 0); // "Build a library"
            ArgCountDic.Add("CSCSTR_OPTDSC_OUTPUTFILENAME", 0); // "Specify output file name (default: base name of file with main class or first file)"
            ArgCountDic.Add("CSCSTR_OPTDSC_PDBFILENAME", 0); // "Specify debug information file name (default: output file name with .pdb extension)"
            ArgCountDic.Add("CSCSTR_OPTDSC_WIN32RESOURCE", 0); // "Specify a Win32 resource file (.res)"
            ArgCountDic.Add("CSCSTR_OPTDSC_WIN32ICON", 0); // "Use this icon for the output"
            ArgCountDic.Add("CSCSTR_OPTDSC_EMBEDRESOURCE", 0); // "Embed the specified resource"
            ArgCountDic.Add("CSCSTR_OPTDSC_LINKRESOURCE", 0); // "Link the specified resource to this assembly"
            ArgCountDic.Add("CSCSTR_OPTDSC_RESPONSEFILE", 0); // "Read response file for more options"
            ArgCountDic.Add("CSCSTR_OPTDSC_RECURSE", 0); // "Include all files in the current directory and subdirectories according to the wildcard specifications"
            ArgCountDic.Add("CSCSTR_OPTDSC_HELP", 0); // "Display this usage message"
            ArgCountDic.Add("CSCSTR_REPROTITLE", 1); // "### Visual C# 2005 Compiler Defect Report, created {0}"
            ArgCountDic.Add("CSCSTR_REPROVER", 1); // "### Compiler version: {0}"
            ArgCountDic.Add("CSCSTR_REPROOS", 5); // "### Operating System: {0} {1}.{2}.{3}   {4}"
            ArgCountDic.Add("CSCSTR_REPROLCID", 1); // "### Console and Defect Report Code Page: {0}"
            ArgCountDic.Add("CSCSTR_REPROCOMMANDLINE", 0); // "### Compiler command line"
            ArgCountDic.Add("CSCSTR_REPROSOURCEFILE", 1); // "### Source file: '{0}'"
            ArgCountDic.Add("CSCSTR_REPRODIAGS", 0); // "### Compiler output"
            ArgCountDic.Add("CSCSTR_REPRODESCRIPTION", 0); // "### User description"
            ArgCountDic.Add("CSCSTR_REPROCORRECTBEHAVIOR", 0); // "### User suggested correct behavior"
            ArgCountDic.Add("CSCSTR_REPROURTVER", 1); // "### .NET common language runtime version: {0}"
            ArgCountDic.Add("CSCSTR_REPROBINFILE", 1); // "### Binary file: '{0}'"
            ArgCountDic.Add("CSCSTR_BUGREPORTWARN", 0); // "A file is being created with information needed to reproduce your compiler problem. This information includes software versions, the pathnames and contents of source code files, referenced assemblies and modules, compiler options, compiler output, and any additional information you provide in the following prompts. This file will not include the contents of any keyfiles."
            ArgCountDic.Add("CSCSTR_ENTERDESC", 0); // "Please describe the compiler problem (press Enter twice to finish):"
            ArgCountDic.Add("CSCSTR_ENTERCORRECT", 0); // "Describe what you think should have happened (press Enter twice to finish):"
            ArgCountDic.Add("CSCSTR_BadVarargs", 0); // "A method with vararg cannot be generic, be in a generic type, or have a params parameter"
            ArgCountDic.Add("CSCSTR_ParamsMustBeArray", 0); // "The params parameter must be a single dimensional array"
            ArgCountDic.Add("CSCSTR_IllegalArglist", 0); // "An __arglist expression may only appear inside of a call or new expression"
            ArgCountDic.Add("CSCSTR_IllegalUnsafe", 0); // "Unsafe code may only appear if compiling with /unsafe"
            ArgCountDic.Add("CSCSTR_IllegalInnerUnsafe", 0); // "Unsafe code may not appear in iterators"
            ArgCountDic.Add("CSCSTR_LocalCantBeFixedAndHoisted", 1); // "Local '{0}' or its members cannot have their address taken and be used inside an anonymous method block"
            ArgCountDic.Add("CSCSTR_NoAccessibleMember", 2); // "'{0}' does not contain a definition for '{1}', or it is not accessible"
            ArgCountDic.Add("CSCSTR_AmbigMember", 2); // "Ambiguity between '{0}' and '{1}'"
            ArgCountDic.Add("CSCSTR_BadForeachDecl", 0); // "Type and identifier are both required in a foreach statement"
            ArgCountDic.Add("CSCSTR_ParamsLast", 0); // "A params parameter must be the last parameter in a formal parameter list"
            ArgCountDic.Add("CSCSTR_SizeofUnsafe", 1); // "'{0}' does not have a predefined size, therefore sizeof can only be used in an unsafe context (consider using System.Runtime.InteropServices.Marshal.SizeOf)"
            ArgCountDic.Add("CSCSTR_DottedTypeNameNotFoundInNS", 2); // "The type or namespace name '{0}' does not exist in the namespace '{1}' (are you missing an assembly reference?)"
            ArgCountDic.Add("CSCSTR_FieldInitRefNonstatic", 1); // "A field initializer cannot reference the nonstatic field, method, or property '{0}'"
            ArgCountDic.Add("CSCSTR_SealedNonOverride", 1); // "'{0}' cannot be sealed because it is not an override"
            ArgCountDic.Add("CSCSTR_CantOverrideSealed", 2); // "'{0}': cannot override inherited member '{1}' because it is sealed"
            ArgCountDic.Add("CSCSTR_VarargsLast", 0); // "An __arglist parameter must be the last parameter in a formal parameter list"
            ArgCountDic.Add("CSCSTR_DotOnDefault", 1); // "Expression will always cause a System.NullReferenceException because the default value of '{0}' is null"
            ArgCountDic.Add("CSCSTR_OPTGRP_OUTPUT", 0); // "- OUTPUT FILES -"
            ArgCountDic.Add("CSCSTR_OPTGRP_INPUT", 0); // "- INPUT FILES -"
            ArgCountDic.Add("CSCSTR_OPTGRP_RESOURCES", 0); // "- RESOURCES -"
            ArgCountDic.Add("CSCSTR_OPTGRP_CODEGENERATION", 0); // "- CODE GENERATION -"
            ArgCountDic.Add("CSCSTR_OPTGRP_ERRORS", 0); // "- ERRORS AND WARNINGS -"
            ArgCountDic.Add("CSCSTR_OPTGRP_LANGUAGE", 0); // "- LANGUAGE -"
            ArgCountDic.Add("CSCSTR_OPTGRP_MISC", 0); // "- MISCELLANEOUS -"
            ArgCountDic.Add("CSCSTR_OPTGRP_ADVANCED", 0); // "- ADVANCED -"
            ArgCountDic.Add("CSCSTR_OD_ARG_FILELIST", 0); // "<file list>"
            ArgCountDic.Add("CSCSTR_OD_ARG_FILE", 0); // "<file>"
            ArgCountDic.Add("CSCSTR_OD_ARG_SYMLIST", 0); // "<symbol list>"
            ArgCountDic.Add("CSCSTR_OD_ARG_WILDCARD", 0); // "<wildcard>"
            ArgCountDic.Add("CSCSTR_OD_ARG_TYPE", 0); // "<type>"
            ArgCountDic.Add("CSCSTR_OD_ARG_RESINFO", 0); // "<resinfo>"
            ArgCountDic.Add("CSCSTR_OD_ARG_WARNLIST", 0); // "<warn list>"
            ArgCountDic.Add("CSCSTR_OD_ARG_ADDR", 0); // "<address>"
            ArgCountDic.Add("CSCSTR_OD_ARG_NUMBER", 0); // "<n>"
            ArgCountDic.Add("CSCSTR_OD_ARG_DEBUGTYPE", 0); // "{full|pdbonly}"
            ArgCountDic.Add("CSCSTR_OD_ARG_STRING", 0); // "<string>"
            ArgCountDic.Add("CSCSTR_OD_ARG_ALIAS", 0); // "<alias>=<file>"
            ArgCountDic.Add("CSCSTR_RESINFO_DESCRIPTION", 0); // "Where the resinfo format is <file>[,<string name>[,public|private]]"
            ArgCountDic.Add("CSCSTR_CLB_ERROR_FIRST", 0); // "Error occurred during a read"
            ArgCountDic.Add("CSCSTR_STRING4001", 0); // "Error occurred during a write"
            ArgCountDic.Add("CSCSTR_STRING4002", 0); // "File is read only"
            ArgCountDic.Add("CSCSTR_STRING4003", 0); // "An ill-formed name was given"
            ArgCountDic.Add("CSCSTR_STRING4004", 0); // "Data value was truncated"
            ArgCountDic.Add("CSCSTR_STRING4005", 0); // "Old version error"
            ArgCountDic.Add("CSCSTR_STRING4006", 0); // "A shared memory open failed to open at the originally assigned memory address"
            ArgCountDic.Add("CSCSTR_STRING4007", 0); // "Create of shared memory failed. A memory mapping of the same name already exists."
            ArgCountDic.Add("CSCSTR_STRING4011", 0); // "There isn't metadata in the memory or stream"
            ArgCountDic.Add("CSCSTR_STRING4012", 0); // "Database is read only"
            ArgCountDic.Add("CSCSTR_STRING4013", 0); // "The importing scope is not compatible with the emitting scope"
            ArgCountDic.Add("CSCSTR_STRING4014", 0); // "File is corrupt"
            ArgCountDic.Add("CSCSTR_STRING4015", 0); // "Version of schema not found"
            ArgCountDic.Add("CSCSTR_STRING4016", 0); // "Cannot open a incrementally built scope for full update"
            ArgCountDic.Add("CSCSTR_FOREACHLOCAL", 0); // "foreach iteration variable"
            ArgCountDic.Add("CSCSTR_FIXEDLOCAL", 0); // "fixed variable"
            ArgCountDic.Add("CSCSTR_USINGLOCAL", 0); // "using variable"
            ArgCountDic.Add("CSCSTR_WATSON_APPNAME", 0); // "Microsoft (R) Visual C# 2005 Compiler"
            ArgCountDic.Add("CSCSTR_WATSON_ERROR_HEADER", 0); // "The Microsoft (R) Visual C# 2005 Compiler has encountered an internal error. We are sorry for the inconvenience."
            ArgCountDic.Add("CSCSTR_WATSON_ERROR_MESSAGE", 0); // "No information has been lost. Please check the compiler output for possible ways to avoid this error."
            ArgCountDic.Add("CSCSTR_LIB_ENV", 0); // "LIB environment variable"
            ArgCountDic.Add("CSCSTR_LIB_OPTION", 0); // "/LIB option"
            ArgCountDic.Add("CSCSTR_WarnAsError", 1); // "Warning as Error: {0}"
            ArgCountDic.Add("CSCSTR_MemberAbstractSealed", 1); // "'{0}' cannot be both abstract and sealed"
            ArgCountDic.Add("CSCSTR_NoDefaultArgs", 0); // "Default parameter specifiers are not permitted"
            ArgCountDic.Add("CSCSTR_VoidError", 0); // "The operation in question is undefined on void pointers"
            ArgCountDic.Add("CSCSTR_ConditionalOnOverride", 1); // "The Conditional attribute is not valid on '{0}' because it is an override method"
            ArgCountDic.Add("CSCSTR_PointerInAsOrIs", 0); // "Neither "is" nor "as" are valid on pointer types"
            ArgCountDic.Add("CSCSTR_CallingFinalizeDepracated", 0); // "Destructors and object.Finalize cannot be called directly. Consider calling IDisposable.Dispose if available."
            ArgCountDic.Add("CSCSTR_SingleTypeNameNotFound", 1); // "The type or namespace name '{0}' could not be found (are you missing a using directive or an assembly reference?)"
            ArgCountDic.Add("CSCSTR_NegativeStackAllocSize", 0); // "Cannot use a negative size with stackalloc"
            ArgCountDic.Add("CSCSTR_NegativeArraySize", 0); // "Cannot create an array with a negative size"
            ArgCountDic.Add("CSCSTR_OverrideFinalizeDeprecated", 0); // "Do not override object.Finalize. Instead, provide a destructor."
            ArgCountDic.Add("CSCSTR_CallingBaseFinalizeDeprecated", 0); // "Do not directly call your base class Finalize method. It is called automatically from your destructor."
            ArgCountDic.Add("CSCSTR_NegativeArrayIndex", 0); // "Indexing an array with a negative index (array indices always start at zero)"
            ArgCountDic.Add("CSCSTR_BadRefCompareLeft", 1); // "Possible unintended reference comparison; to get a value comparison, cast the left hand side to type '{0}'"
            ArgCountDic.Add("CSCSTR_BadRefCompareRight", 1); // "Possible unintended reference comparison; to get a value comparison, cast the right hand side to type '{0}'"
            ArgCountDic.Add("CSCSTR_BadCastInFixed", 0); // "The right hand side of a fixed statement assignment may not be a cast expression"
            ArgCountDic.Add("CSCSTR_StackallocInCatchFinally", 0); // "stackalloc may not be used in a catch or finally block"
            ArgCountDic.Add("CSCSTR_TooManyLinesForDebugger", 0); // "Source file has exceeded the limit of 16,707,565 lines representable in the PDB, debug information will be incorrect"
            ArgCountDic.Add("CSCSTR_UnifyReferenceMajMin", 2); // "Assuming assembly reference '{0}' matches '{1}', you may need to supply runtime policy"
            ArgCountDic.Add("CSCSTR_UnifyReferenceBldRev", 2); // "Assuming assembly reference '{0}' matches '{1}', you may need to supply runtime policy"
            ArgCountDic.Add("CSCSTR_DuplicateImport", 1); // "An assembly with the same identity '{0}' has already been imported. Try removing one of the duplicate references."
            ArgCountDic.Add("CSCSTR_DuplicateImportSimple", 1); // "An assembly with the same simple name '{0} has already been imported. Try removing one of the references or sign them to enable side-by-side."
            ArgCountDic.Add("CSCSTR_AssemblyMatchBadVersion", 3); // "Assembly '{0}' uses '{1}' which has a higher version than referenced assembly '{2}'"
            ArgCountDic.Add("CSCSTR_NonObsoleteOverridingObsolete", 2); // "Member '{0}' overrides obsolete member '{1}'. Add the Obsolete attribute to '{0}'."
            ArgCountDic.Add("CSCSTR_SystemVoid", 0); // "System.Void cannot be used from C# -- use typeof(void) to get the void type object."
            ArgCountDic.Add("CSCSTR_ExplicitParamArray", 0); // "Do not use 'System.ParamArrayAttribute'. Use the 'params' keyword instead."
            ArgCountDic.Add("CSCSTR_BitwiseOrSignExtend", 0); // "Bitwise-or operator used on a sign-extended operand; consider casting to a smaller unsigned type first"
            ArgCountDic.Add("CSCSTR_VolatileStruct", 2); // "'{0}': a volatile field cannot be of the type '{1}'"
            ArgCountDic.Add("CSCSTR_VolatileAndReadonly", 1); // "'{0}': a field cannot be both volatile and readonly"
            ArgCountDic.Add("CSCSTR_InternalVirtual", 1); // "Other languages may permit the internal virtual member '{0}' to be overridden"
            ArgCountDic.Add("CSCSTR_AbstractField", 0); // "The modifier 'abstract' is not valid on fields. Try using a property instead."
            ArgCountDic.Add("CSCSTR_BogusExplicitImpl", 2); // "'{0}' cannot implement '{1}' because it is not supported by the language"
            ArgCountDic.Add("CSCSTR_ExplicitMethodImplAccessor", 2); // "'{0}' explicit method implementation cannot implement '{1}' because it is an accessor"
            ArgCountDic.Add("CSCSTR_CoClassWithoutComImport", 1); // "'{0}' interface marked with 'CoClassAttribute' not marked with 'ComImportAttribute'"
            ArgCountDic.Add("CSCSTR_LinkDemandOnOverride", 2); // "'{0}' has a link demand, but overrides or implements '{1}' which does not have a link demand. A security hole may exist."
            ArgCountDic.Add("CSCSTR_MalformedMetadata", 1); // "Input file '{0}' contains invalid metadata."
            ArgCountDic.Add("CSCSTR_DuplicateTypeParameter", 1); // "Duplicate type parameter '{0}'"
            ArgCountDic.Add("CSCSTR_TypeParameterSameAsOuterTypeParameter", 2); // "Type parameter '{0}' has the same name as the type parameter from outer type '{1}'"
            ArgCountDic.Add("CSCSTR_TypeVariableSameAsParent", 1); // "Type parameter '{0}' has the same name as the containing type, or method"
            ArgCountDic.Add("CSCSTR_UnifyingInterfaceInstantiations", 3); // "'{0}' cannot implement both '{1}' and '{2}' because they may unify for some type parameter substitutions"
            ArgCountDic.Add("CSCSTR_ConditionalWithOutParam", 1); // "Conditional member '{0}' cannot have an out parameter"
            ArgCountDic.Add("CSCSTR_AccessorImplementingMethod", 3); // "Accessor '{0}' cannot implement interface member '{1}' for type '{2}'. Use an explicit interface implementation."
            ArgCountDic.Add("CSCSTR_AliasQualAsExpression", 0); // "The namespace alias qualifier '::' always resolves to a type or namespace so is illegal here. Consider using '.' instead."
            ArgCountDic.Add("CSCSTR_DerivingFromATyVar", 1); // "Cannot derive from '{0}' because it is a type parameter"
            ArgCountDic.Add("CSCSTR_GenericDerivingFromAttribute", 1); // "A generic type cannot derive from '{0}' because it is an attribute class"
            ArgCountDic.Add("CSCSTR_TyVarNotFoundInConstraint", 2); // "'{1}' does not define type parameter '{0}'"
            ArgCountDic.Add("CSCSTR_BadBoundType", 1); // "'{0}' is not a valid constraint. A type used as a constraint must be an interface, a non-sealed class or a type parameter."
            ArgCountDic.Add("CSCSTR_SpecialTypeAsBound", 1); // "Constraint cannot be special class '{0}'"
            ArgCountDic.Add("CSCSTR_BadVisBound", 2); // "Inconsistent accessibility: constraint type '{1}' is less accessible than '{0}'"
            ArgCountDic.Add("CSCSTR_LookupInTypeVariable", 1); // "Cannot do member lookup in '{0}' because it is a type parameter"
            ArgCountDic.Add("CSCSTR_AnonMethNotAllowed", 0); // "Expression cannot contain anonymous methods"
            ArgCountDic.Add("CSCSTR_MissingPartial", 1); // "Missing partial modifier on declaration of type '{0}'; another partial declaration of this type exists"
            ArgCountDic.Add("CSCSTR_PartialTypeKindConflict", 1); // "Partial declarations of '{0}' must be all classes, all structs, or all interfaces"
            ArgCountDic.Add("CSCSTR_PartialModifierConflict", 1); // "Partial declarations of '{0}' have conflicting accessibility modifiers"
            ArgCountDic.Add("CSCSTR_PartialMultipleBases", 1); // "Partial declarations of '{0}' must not specify different base classes"
            ArgCountDic.Add("CSCSTR_PartialWrongTypeParams", 1); // "Partial declarations of '{0}' must have the same type parameter names in the same order"
            ArgCountDic.Add("CSCSTR_PartialWrongConstraints", 2); // "Partial declarations of '{0}' have inconsistent constraints for type parameter '{1}'"
            ArgCountDic.Add("CSCSTR_PartialMisplaced", 0); // "The partial modifier can only appear immediately before 'class', 'struct', or 'interface'"
            ArgCountDic.Add("CSCSTR_ImportedCircularBase", 1); // "Imported type '{0}' is invalid. It contains a circular base class dependency."
            ArgCountDic.Add("CSCSTR_AssumedMatchThis", 2); // "Circular assembly reference '{0}' does not match the output assembly name '{1}'. Try adding a reference to '{0}' or changing the output assembly name to match."
            ArgCountDic.Add("CSCSTR_InvalidAssemblyName", 1); // "Assembly reference '{0}' is invalid and cannot be resolved"
            ArgCountDic.Add("CSCSTR_UseSwitchInsteadOfAttribute", 2); // "Use command line option '/{0}' or appropriate project settings instead of '{1}'"
            ArgCountDic.Add("CSCSTR_FriendAssemblyBadArgs", 1); // "Friend assembly reference '{0}' is invalid. InternalsVisibleTo declarations cannot have a version, culture, public key token, or processor architecture specified."
            ArgCountDic.Add("CSCSTR_FriendAssemblySNReq", 1); // "Friend assembly reference '{0}' is invalid. Strong-name signed assemblies must specify a public key in their InternalsVisibleTo declarations."
            ArgCountDic.Add("ALSTR_InternalError", 0); // "Internal compiler error"
            ArgCountDic.Add("ALSTR_NoMemory", 0); // "Out of memory"
            ArgCountDic.Add("ALSTR_MissingOptionArg", 1); // "Compiler option '{0}' must be followed by an argument"
            ArgCountDic.Add("ALSTR_ComPlusInit", 1); // "Unexpected common language runtime initialization error -- '{0}'"
            ArgCountDic.Add("ALSTR_FileTooBig", 1); // "File '{0}' too big to open"
            ArgCountDic.Add("ALSTR_DuplicateResponseFile", 1); // "Response file '{0}' was already included"
            ArgCountDic.Add("ALSTR_OpenResponseFile", 2); // "Error opening response file '{0}' -- '{1}'"
            ArgCountDic.Add("ALSTR_NoFileSpec", 1); // "Missing file specification for '{0}' command-line option"
            ArgCountDic.Add("ALSTR_CantOpenFileWrite", 1); // "Can't open '{0}' for writing"
            ArgCountDic.Add("ALSTR_SwitchNeedsString", 1); // "Command-line syntax error: Missing ':<text>' for '{0}' option"
            ArgCountDic.Add("ALSTR_CantOpenBinaryAsText", 1); // "File '{0}' is an executable file and cannot be opened as a text file"
            ArgCountDic.Add("ALSTR_BadOptionValue", 2); // "'{1}' is not a valid setting for option '{0}'"
            ArgCountDic.Add("ALSTR_BadSwitch", 1); // "Unrecognized command-line option: '{0}'"
            ArgCountDic.Add("ALSTR_InitError", 1); // "Unexpected initialization error -- '{0}'"
            ArgCountDic.Add("ALSTR_IllegalOptionChar", 1); // "Character '{0}' is not allowed on the command-line or in response files"
            ArgCountDic.Add("ALSTR_AssemblyModuleImportError", 3); // "Error importing module '{1}' of assembly '{0}' -- {2}"
            ArgCountDic.Add("ALSTR_BinaryFile", 1); // "'{0}' is a binary file instead of a text file"
            ArgCountDic.Add("ALSTR_InvalidTime", 0); // "Cannot auto-generate build and revision version numbers for dates previous to January 1, 2000"
            ArgCountDic.Add("ALSTR_FeatureDeprecated", 2); // "The feature you are using '{0}' is no longer supported; please use '{1}' instead"
            ArgCountDic.Add("ALSTR_EmitCAFailed", 2); // "Error emitting '{0}' attribute --'{1}'"
            ArgCountDic.Add("ALSTR_ParentNotAnAssembly", 1); // "File '{0}' is not an assembly"
            ArgCountDic.Add("ALSTR_InvalidVersionFormat", 2); // "The version '{1}' specified for the '{0}' is not in the normal 'major.minor.build.revision' format"
            ArgCountDic.Add("ALSTR_RefNotStrong", 1); // "Referenced assembly '{0}' does not have a strong name"
            ArgCountDic.Add("ALSTR_RefHasCulture", 1); // "Referenced assembly '{0}' is a localized satellite assembly"
            ArgCountDic.Add("ALSTR_ExeHasCulture", 0); // "Executables cannot be satellite assemblies, Culture should always be empty"
            ArgCountDic.Add("ALSTR_CantAddAssembly", 1); // "'{0}' is an assembly and cannot be added as a module"
            ArgCountDic.Add("ALSTR_UnknownError", 1); // "Unknown error ({0})"
            ArgCountDic.Add("ALSTR_CryptoHashFailed", 1); // "Cryptographic failure while creating hashes -- {0}"
            ArgCountDic.Add("ALSTR_BadOptionValueHR", 2); // "Cannot set option '{0}' because '{1}'"
            ArgCountDic.Add("ALSTR_AutoResGen", 2); // "Error generating resources for '{0}' -- {1}"
            ArgCountDic.Add("ALSTR_DuplicateCA", 1); // "Assembly custom attribute '{0}' was specified multiple times with different values"
            ArgCountDic.Add("ALSTR_CantRenameAssembly", 1); // "Assembly '{0}' cannot be copied or renamed."
            ArgCountDic.Add("ALSTR_NoMainOnDLLs", 0); // "Libraries cannot have an entry point"
            ArgCountDic.Add("ALSTR_AppNeedsMain", 0); // "Entry point required for executable applications"
            ArgCountDic.Add("ALSTR_NoMainFound", 1); // "Unable to find the entry point method '{0}'"
            ArgCountDic.Add("ALSTR_FusionInit", 1); // "Initialization of global assembly cache manager failed -- {0}"
            ArgCountDic.Add("ALSTR_FusionInstallFailed", 1); // "Failed to install assembly into cache -- {0}"
            ArgCountDic.Add("ALSTR_BadMainFound", 1); // "'{0}' cannot be the entry point because the signature or visibility is incorrect, or it is generic"
            ArgCountDic.Add("ALSTR_CantAddExes", 1); // "'{0}': EXEs cannot be added modules"
            ArgCountDic.Add("ALSTR_SameOutAndSource", 1); // "Manifest filename '{0}' cannot be the same as any modules"
            ArgCountDic.Add("ALSTR_CryptoFileFailed", 2); // "Error reading key file '{0}' -- {1}"
            ArgCountDic.Add("ALSTR_FileNameTooLong", 1); // "Filename '{0}' is too long or invalid"
            ArgCountDic.Add("ALSTR_DupResourceIdent", 1); // "Resource identifier '{0}' has already been used in this assembly"
            ArgCountDic.Add("ALSTR_ModuleImportError", 2); // "Error importing file '{0}' -- {1}"
            ArgCountDic.Add("ALSTR_DuplicateExportedType", 3); // "Public type '{0}' is defined in multiple places in this assembly: '{1}' and '{2}'"
            ArgCountDic.Add("ALSTR_DuplicateTypeForwarders", 3); // "Type '{0}' is forwarded to multiple assemblies: '{1}' and '{2}'"
            ArgCountDic.Add("ALSTR_TypeFwderMatchesDeclared", 3); // "Public type '{0}' is defined in '{1}' and forwarded to '{2}'"
            ArgCountDic.Add("ALSTR_NoInputs", 0); // "No valid input files were specified"
            ArgCountDic.Add("ALSTR_NoOutput", 0); // "No target filename was specified"
            ArgCountDic.Add("ALSTR_RequiredFileNotFound", 1); // "Required file '{0}' could not be loaded"
            ArgCountDic.Add("ALSTR_MetaDataError", 1); // "Metadata failure while creating assembly -- {0}"
            ArgCountDic.Add("ALSTR_IgnoringAssembly", 1); // "Ignoring included assembly '{0}'"
            ArgCountDic.Add("ALSTR_OptionConflicts", 1); // "'{0}' : overriding previous setting"
            ArgCountDic.Add("ALSTR_CantReadResource", 2); // "Error reading embedded resource '{0}' -- {1}"
            ArgCountDic.Add("ALSTR_CantEmbedResource", 2); // "Error embedding resource '{0}' -- {1}"
            ArgCountDic.Add("ALSTR_InvalidFileDefInComType", 2); // "ComType record '{0}' points to an invalid file record '{1}'"
            ArgCountDic.Add("ALSTR_InvalidVersionString", 1); // "The version specified '{0}' is invalid"
            ArgCountDic.Add("ALSTR_InvalidOSString", 1); // "The operating system specified '{0}' is invalid"
            ArgCountDic.Add("ALSTR_NeedPrivateKey", 1); // "Key file '{0}' is missing the private key needed for signing"
            ArgCountDic.Add("ALSTR_CryptoNoKeyContainer", 1); // "The key container name '{0}' does not exist"
            ArgCountDic.Add("ALSTR_CryptoFailed", 0); // "The cryptographic service is not installed properly or does not have a suitable key provider"
            ArgCountDic.Add("ALSTR_CantReadIcon", 2); // "Error reading icon '{0}' -- {1}"
            ArgCountDic.Add("ALSTR_IgnoringDuplicateSource", 1); // "Module '{0}' was specified multiple times; it will only be included once"
            ArgCountDic.Add("ALSTR_DuplicateModule", 1); // "Module '{0}' is already defined in this assembly. Each linked resource and module must have a unique filename."
            ArgCountDic.Add("ALSTR_InputFileNameTooLong", 1); // "File name '{0}' is too long or invalid"
            ArgCountDic.Add("ALSTR_OutputFileExists", 1); // "Cannot create short filename '{0}' when a long filename with the same short filename already exists"
            ArgCountDic.Add("ALSTR_AgnosticToMachineModule", 1); // "Agnostic assembly cannot have a processor specific module '{0}'"
            ArgCountDic.Add("ALSTR_ConflictingMachineModule", 1); // "Assembly and module '{0}' cannot target different processors"
            ArgCountDic.Add("ALSTR_ConflictingMachineAssembly", 1); // "Referenced assembly '{0}' targets a different processor"
            ArgCountDic.Add("ALSTR_ModuleNameDifferent", 2); // "Module name '{1}' stored in '{0}' must match its filename"
            ArgCountDic.Add("ALSTR_DelaySignWithNoKey", 0); // "Delay signing was requested, but no key was given"
            ArgCountDic.Add("ALSTR_H_SOURCEFILE", 0); // "  <filename>[,<targetfile>] add file to assembly"
            ArgCountDic.Add("ALSTR_H_EMBED1", 0); // "  /embed[resource]:<filename>[,<name>[,Private]]"
            ArgCountDic.Add("ALSTR_H_EMBED2", 0); // "                            embed the file as a resource in the assembly"
            ArgCountDic.Add("ALSTR_H_LINK1", 0); // "  /link[resource]:<filename>[,<name>[,<targetfile>[,Private]]]"
            ArgCountDic.Add("ALSTR_H_LINK2", 0); // "                            link the file as a resource to the assembly"
            ArgCountDic.Add("ALSTR_HELP10", 0); // "Usage: al [options] [sources]"
            ArgCountDic.Add("ALSTR_HELP20", 0); // "Options: ('/out' must be specified)"
            ArgCountDic.Add("ALSTR_HELP30", 0); // "Sources: (at least one source input is required)"
            ArgCountDic.Add("ALSTR_H_HELP", 0); // "  /? or /help               Display this usage message"
            ArgCountDic.Add("ALSTR_H_RESPONSE", 0); // "  @<filename>               Read response file for more options"
            ArgCountDic.Add("ALSTR_H_ALGID", 0); // "  /algid:<id>               Algorithm used to hash files (in hexadecimal)"
            ArgCountDic.Add("ALSTR_BANNER1", 0); // "Microsoft (R) Shared Source CLI Assembly Linker version "
            ArgCountDic.Add("ALSTR_BANNER1PART2", 0); // "for Microsoft (R) Shared Source CLI version "
            ArgCountDic.Add("ALSTR_BANNER2", 0); // "Copyright (C) Microsoft Corporation. All rights reserved."
            ArgCountDic.Add("ALSTR_H_TARGET", 0); // "  /t[arget]:lib[rary]       Create a library"
            ArgCountDic.Add("ALSTR_H_TARGET2", 0); // "  /t[arget]:exe             Create a console executable"
            ArgCountDic.Add("ALSTR_H_TARGET3", 0); // "  /t[arget]:win[exe]        Create a Windows executable"
            ArgCountDic.Add("ALSTR_H_TEMPLATE", 0); // "  /template:<filename>      Specifies an assembly to get default options from"
            ArgCountDic.Add("ALSTR_H_TITLE", 0); // "  /title:<text>             Title"
            ArgCountDic.Add("ALSTR_H_TRADEMARK", 0); // "  /trade[mark]:<text>       Trademark message"
            ArgCountDic.Add("ALSTR_H_DESCR", 0); // "  /descr[iption]:<text>     Description"
            ArgCountDic.Add("ALSTR_H_EVIDENCE", 0); // "  /e[vidence]:<filename>    Security evidence file to embed"
            ArgCountDic.Add("ALSTR_H_FILEVER", 0); // "  /fileversion:<version>    Optional Win32 version (overrides assembly version)"
            ArgCountDic.Add("ALSTR_H_FLAGS", 0); // "  /flags:<flags>            Assembly flags  (in hexadecimal)"
            ArgCountDic.Add("ALSTR_H_FULLPATHS", 0); // "  /fullpaths                Display files using fully-qualified filenames"
            ArgCountDic.Add("ALSTR_H_BASEADDRESS", 0); // "  /base[address]:<addr>     Base address for the library"
            ArgCountDic.Add("ALSTR_H_BUGREPORT", 0); // "  /bugreport:<filename>     Create a 'Bug Report' file"
            ArgCountDic.Add("ALSTR_H_COMPANY", 0); // "  /comp[any]:<text>         Company name"
            ArgCountDic.Add("ALSTR_H_CONFIG", 0); // "  /config[uration]:<text>   Configuration string"
            ArgCountDic.Add("ALSTR_H_COPYRIGHT", 0); // "  /copy[right]:<text>       Copyright message"
            ArgCountDic.Add("ALSTR_H_LOCALE", 0); // "  /c[ulture]:<text>         Supported culture"
            ArgCountDic.Add("ALSTR_H_DELAYSIGN", 0); // "  /delay[sign][+|-]         Delay sign this assembly"
            ArgCountDic.Add("ALSTR_H_VERSION", 0); // "  /v[ersion]:<version>      Version (use * to auto-generate remaining numbers)"
            ArgCountDic.Add("ALSTR_H_WIN32ICON", 0); // "  /win32icon:<filename>     Use this icon for the output"
            ArgCountDic.Add("ALSTR_H_WIN32RES", 0); // "  /win32res:<filename>      Specifies the Win32 resource file"
            ArgCountDic.Add("ALSTR_H_PLATFORM", 0); // "  /platform:<text>          Limit which platforms this code can run on; must be"
            ArgCountDic.Add("ALSTR_H_PLATFORM2", 0); // "                            one of x86, Itanium, x64, or anycpu (the default)"
            ArgCountDic.Add("ALSTR_H_PRODUCT", 0); // "  /prod[uct]:<text>         Product name"
            ArgCountDic.Add("ALSTR_H_PRODVER", 0); // "  /productv[ersion]:<text>  Product version"
            ArgCountDic.Add("ALSTR_H_KEYFILE", 0); // "  /keyf[ile]:<filename>     File containing key to sign the assembly"
            ArgCountDic.Add("ALSTR_H_KEYNAME", 0); // "  /keyn[ame]:<text>         Key container name of key to sign assembly"
            ArgCountDic.Add("ALSTR_H_MAIN", 0); // "  /main:<method>            Specifies the method name of the entry point"
            ArgCountDic.Add("ALSTR_H_NOLOGO", 0); // "  /nologo                   Suppress the startup banner and copyright message"
            ArgCountDic.Add("ALSTR_H_OUT", 0); // "  /out:<filename>           Output file name for the assembly manifest"
            ArgCountDic.Add("ALSTR_REPROTITLE", 1); // "### Assembly Linker Defect Report, created {0}"
            ArgCountDic.Add("ALSTR_REPROVER", 1); // "### ALink version: {0}"
            ArgCountDic.Add("ALSTR_REPROOS", 5); // "### Operating System: {0} {1}.{2}.{3}   {4}"
            ArgCountDic.Add("ALSTR_REPROCOMMANDLINE", 0); // "### ALink command line"
            ArgCountDic.Add("ALSTR_REPROSOURCEFILE", 1); // "### Source file: '{0}'"
            ArgCountDic.Add("ALSTR_REPROBINFILE", 1); // "### Binary file: '{0}'"
            ArgCountDic.Add("ALSTR_REPRODIAGS", 0); // "### ALink output"
            ArgCountDic.Add("ALSTR_REPRODESCRIPTION", 0); // "### User description"
            ArgCountDic.Add("ALSTR_REPROCORRECTBEHAVIOR", 0); // "### User suggested correct behavior"
            ArgCountDic.Add("ALSTR_REPROURTVER", 1); // "### .NET Common Language Runtime version: {0}"
            ArgCountDic.Add("ALSTR_REPROLCID", 1); // "### Console and Defect Report Code Page: {0}"
            ArgCountDic.Add("ALSTR_BUGREPORTWARN", 0); // "A file is being created with information needed to reproduce your compiler problem. This information includes: software versions, the pathnames and contents of source code files, referenced assemblies, and modules, compiler options, compiler output, and any additional information you provide in the following prompts."
            ArgCountDic.Add("ALSTR_ENTERDESC", 0); // "Please describe the compiler problem (press Enter twice to finish):"
            ArgCountDic.Add("ALSTR_ENTERCORRECT", 0); // "Describe what you think should have happened (press Enter twice to finish):"
            ArgCountDic.Add("ALSTR_N_TITLE", 0); // "title"
            ArgCountDic.Add("ALSTR_N_DESCR", 0); // "description"
            ArgCountDic.Add("ALSTR_N_CONFIG", 0); // "configuration"
            ArgCountDic.Add("ALSTR_N_OS", 0); // "operating system"
            ArgCountDic.Add("ALSTR_N_PROC", 0); // "processor"
            ArgCountDic.Add("ALSTR_N_LOCALE", 0); // "culture"
            ArgCountDic.Add("ALSTR_N_VERSION", 0); // "version"
            ArgCountDic.Add("ALSTR_N_COMPANY", 0); // "company"
            ArgCountDic.Add("ALSTR_N_PRODUCT", 0); // "product name"
            ArgCountDic.Add("ALSTR_N_PRODVER", 0); // "product version"
            ArgCountDic.Add("ALSTR_N_COPYRIGHT", 0); // "copyright"
            ArgCountDic.Add("ALSTR_N_TRADEMARK", 0); // "trademark"
            ArgCountDic.Add("ALSTR_N_KEYFILE", 0); // "key file"
            ArgCountDic.Add("ALSTR_N_KEYNAME", 0); // "key container name"
            ArgCountDic.Add("ALSTR_N_ALGID", 0); // "hash algorithm"
            ArgCountDic.Add("ALSTR_N_FLAGS", 0); // "assembly flags"
            ArgCountDic.Add("ALSTR_N_DELAYSIGN", 0); // "delay sign"
            ArgCountDic.Add("ALSTR_N_FILEVER", 0); // "file version"
            ArgCountDic.Add("ALSTR_N_SATELLITEVER", 0); // "System.Resources.SatelliteContractVersionAttribute"
            ArgCountDic.Add("ALSTR_N_FRIENDASSEM", 0); // "System.Runtime.CompilerServices.InternalIsVisibleToAttribute"
#endif
        }
    }
}
