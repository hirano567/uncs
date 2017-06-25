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
// File: options.h
//
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
// File: options.cpp
//
// Code to parse and set the compiler options.
// ===========================================================================

//============================================================================
// OptionInfo.cs
//
// 2015/10/26 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // enum OptionInfoID
    //======================================================================
    internal enum OptionInfoID : int
    {
        WrongUse = -1,

        Invalid = 0,
        FirstOption,

        ResponseFile = FirstOption,
        Help,
        AdditionalFile,
        AddModule,
        Analyzer,
        AppConfig,
        BaseAddress,
        BugReport,
        Checked,
        CheckSumAlgorithm,
        Codepage,
        Debug,
        DebugType,
        DefineCCSymbols,
        DelaySign,
        XMLDocFile,
        ErrorReport,
        FileAlign,
        FullPaths,
        HighEntropyVa,
        KeyContainer,
        KeyFile,
        LangVersion,
        LibPath,
        Link,
        LinkResource,
        Main,
        ModuleAssemblyName,
        ModuleName,
        NoConfig,
        NoLogo,
        NoStdLib,
        NoWarnList,
        NoWin32Manifest,
        Optimize,
        OutputFile,
        Parallel,
        PDBFile,
        Platform,
        PreferredUILang,
        Recurse,
        Reference,
        Resource,
        RuleSet,
        SubSystemVersion,
        Target,
        Unsafe,
        UTF8Output,
        WarningLevel,
        WarningsAsErrors,
        WarnAsErrorList,
        Win32Icon,
        Win32Manifest,
        Win32Resource,

        ArgorithmID,
        Company,
        Configuration,
        Copyright,
        Culture,
        Description,
        Evidence,
        FileVersion,
        Flags,
        Product,
        ProductVersion,
        Template,
        Title,
        Trademark,
        Version,

        LastOption = Version,
        OptionCount = LastOption,

        Source,
    }

    //======================================================================
    // enum CommandOptionFlag
    //======================================================================
    [Flags]
    internal enum CommandOptionFlag : int
    {
        // can be specifined by csc.exe or al.exe.
        CMD_CSC = 1 << 0,
        CMD_AL = CMD_CSC << 1,

        // Group CSC.EXE
        GRP_CSC_Help = CMD_AL << 1,
        GRP_CSC_Optimization = GRP_CSC_Help << 1,
        GRP_CSC_OutputFiles = GRP_CSC_Optimization << 1,
        GRP_CSC_Assemblies = GRP_CSC_OutputFiles << 1,
        GRP_CSC_Debugging = GRP_CSC_Assemblies << 1,
        GRP_CSC_Preprocessor = GRP_CSC_Debugging << 1,
        GRP_CSC_Resources = GRP_CSC_Preprocessor << 1,
        GRP_CSC_Miscellaneous = GRP_CSC_Resources << 1,
        GRP_CSC_Obsolete = GRP_CSC_Miscellaneous << 1,

        // Group AL.EXE
        GRP_AL_Help = GRP_CSC_Obsolete << 1,
        GRP_AL_Source = GRP_AL_Help << 1,
        GRP_AL_Option = GRP_AL_Source << 1,
        GRP_AL_Obsolete = GRP_AL_Option << 1,
    }

    //======================================================================
    // class CommandOptionInfo
    //
    /// <summary>
    /// <para>(sscli) OPTIONDEF.
    /// Struct used for the static options table</para>
    /// </summary>
    //======================================================================
    internal class CommandOptionInfo
    {
        //------------------------------------------------------------
        // OptionInfo Fields and Properties
        //------------------------------------------------------------
        internal OptionInfoID InfoID;
        internal bool Disabled = false;
        internal CommandOptionID[] OptionID = null;
        internal string[] Switch = null;
        internal bool IsBooleanOption = false;
        internal ResNo[] Syntax = null;
        internal ResNo[] Descriptions = null;
        internal CommandOptionFlag Flags = 0;
        internal string Attribute = null;

        internal CommandOptionInfo NextSameSwitchOption = null;

        //------------------------------------------------------------
        // CommandOptionInfo Constructor
        //
        /// <summary></summary>
        /// <param name="disabled"></param>
        /// <param name="ids"></param>
        /// <param name="switches"></param>
        /// <param name="resId"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal CommandOptionInfo(
            OptionInfoID iid,
            bool disabled,
            CommandOptionID[] oids,
            string[] switches,
            bool isBoolean,
            ResNo[] syn,
            ResNo[] desc,
            CommandOptionFlag flags,
            string attr)
        {
            this.InfoID = iid;
            this.Disabled = disabled;
            this.OptionID = oids;
            this.Switch = switches;
            this.IsBooleanOption = isBoolean;
            this.Syntax = syn;
            this.Descriptions = desc;
            this.Flags = flags;
            this.Attribute = attr;
        }
    }

    //======================================================================
    // class OptionInfoManager
    //
    /// <summary></summary>
    //======================================================================
    internal static class OptionInfoManager
    {
        //============================================================
        // class OptionInfoManager.CompareInfoAlphabetically
        //
        /// <summary>
        /// for sorting InfoList alphabetically.
        /// </summary>
        //============================================================
        private class CompareInfoAlphabetically : IComparer<CommandOptionInfo>
        {
            public int Compare(CommandOptionInfo info1, CommandOptionInfo info2)
            {
                return String.Compare(info1.Switch[0], info2.Switch[0], true);
            }
        }

        //------------------------------------------------------------
        // OptionInfoManager Fields and Properties
        //------------------------------------------------------------
        internal static Dictionary<CommandOptionID, CommandOptionInfo> IDtoInfoDic
            = new Dictionary<CommandOptionID, CommandOptionInfo>();

        internal static Dictionary<string, CommandOptionInfo> SwitchToInfoDic
            = new Dictionary<string, CommandOptionInfo>();

        internal static List<CommandOptionInfo> InfoList
            = new List<CommandOptionInfo>();

        //------------------------------------------------------------
        // OptionInfoManager Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        static OptionInfoManager()
        {
            AddAllOptionInfos();

            InfoList.Sort(new CompareInfoAlphabetically());
        }

        //------------------------------------------------------------
        // OptionInfoManager.GetOptionInfo
        //
        /// <summary></summary>
        /// <param name="switchStr"></param>
        /// <param name="isBoolean"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static OptionInfoID GetOptionInfo(
            string switchStr,
            bool isBoolean,
            out CommandOptionInfo info)
        {
            info = null;

            if (SwitchToInfoDic.TryGetValue(switchStr.ToLower(), out info))
            {
                do
                {
                    if (info.IsBooleanOption == isBoolean)
                    {
                        return info.InfoID;
                    }
                    info = info.NextSameSwitchOption;
                } while (info != null);

                return OptionInfoID.WrongUse;
            }
            return OptionInfoID.Invalid;
        }

        //------------------------------------------------------------
        // OptionInfoManager.AddOptionInfo
        //
        /// <summary></summary>
        /// <param name="disabled"></param>
        /// <param name="optIDs"></param>
        /// <param name="switches"></param>
        /// <param name="strID"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal static void AddOptionInfo(
            OptionInfoID infoID,
            bool disabled,
            CommandOptionID[] optIDs,
            string[] switches,
            bool isBoolean,
            ResNo[] syn,
            ResNo[] desc,
            CommandOptionFlag flags,
            string attr)
        {
            CommandOptionInfo info = new CommandOptionInfo(
                infoID,
                disabled,
                optIDs,
                switches,
                isBoolean,
                syn,
                desc,
                flags,
                attr);

            foreach (CommandOptionID id in optIDs)
            {
                IDtoInfoDic.Add(id, info);
            }

            foreach (string sw1 in switches)
            {
                string sw2 = sw1.ToLower();
                CommandOptionInfo info2 = null;
                if (!SwitchToInfoDic.TryGetValue(sw2, out info2))
                {
                    SwitchToInfoDic.Add(sw2, info);
                }
                else if (info2 != null)
                {
                    while (info2.NextSameSwitchOption != null)
                    {
                        info2 = info2.NextSameSwitchOption;
                    }
                    info2.NextSameSwitchOption = info;
                }
                else // found sw, but info2 == null
                {
                    SwitchToInfoDic[sw2] = info;
                }
            }

            InfoList.Add(info);
        }

        //------------------------------------------------------------
        // OptionInfoManager.AddAllOptionInfos
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal static void AddAllOptionInfos()
        {
            OptionInfoID infoID = OptionInfoID.Invalid;
            CommandOptionID[] optIDs = null;
            string[] switches = null;
            ResNo[] syntax = null;
            ResNo[] descs = null;
            CommandOptionFlag flags = 0;
            string attr = null;

            //--------------------------------------------------------
            // CSC_ResponseFile, AL_ResponseFile
            //--------------------------------------------------------
            infoID = OptionInfoID.Resource;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_ResponseFile,
                CommandOptionID.AL_ResponseFile
            };
            switches = new string[1] { "@" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_RESPONSEFILE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_RESPONSEFILE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Miscellaneous |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Help, AL_Help
            //--------------------------------------------------------
            infoID = OptionInfoID.Help;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_Help,
                CommandOptionID.AL_Help
            };
            switches = new string[2] { "help", "?" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_HELP };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_HELP };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Miscellaneous |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_AdditionalFile
            //--------------------------------------------------------
            infoID = OptionInfoID.AdditionalFile;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_AdditionalFile,
            };
            switches = new string[1] { "additionalfile" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_ADDITIONALFILE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_HELP };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, true, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_AddModule
            //--------------------------------------------------------
            infoID = OptionInfoID.AddModule;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_AddModule,
            };
            switches = new string[1] { "addmodule" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_ADDMODULE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_ADDMODULE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Analyzer
            //--------------------------------------------------------
            infoID = OptionInfoID.Analyzer;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Analyzer,
            };
            switches = new string[2] { "analyzer", "a" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_ANALYZER };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_ANALYZER };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, true, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_AppConfig
            //--------------------------------------------------------
            infoID = OptionInfoID.AppConfig;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_AppConfig,
            };
            switches = new string[1] { "appconfig" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_APPCONFIG };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_APPCONFIGR };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_BaseAddress, AL_BaseAddress
            //--------------------------------------------------------
            infoID = OptionInfoID.BaseAddress;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_BaseAddress,
                CommandOptionID.AL_BaseAddress,
            };
            switches = new string[1] { "baseaddress" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_BASEADDRESS };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_BASEADDRESS };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Miscellaneous |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_BugReport, AL_BugReport
            //--------------------------------------------------------
            infoID = OptionInfoID.BugReport;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_BugReport,
                CommandOptionID.AL_BugReport,
            };
            switches = new string[1] { "bugreport" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_BUGREPORT };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_BUGREPORT };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Debugging |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Checked
            //--------------------------------------------------------
            infoID = OptionInfoID.Checked;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Checked,
            };
            switches = new string[1] { "checked" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_CHECKED };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_CHECKED };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_CheckSumAlgorithm
            //--------------------------------------------------------
            infoID = OptionInfoID.CheckSumAlgorithm;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_CheckSumAlgorithm,
            };
            switches = new string[1] { "checksumalgorithm" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_CHECKSUMALGORITHM };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_CHECKSUMALGORITHM };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, true, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Codepage
            //--------------------------------------------------------
            infoID = OptionInfoID.Codepage;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Codepage,
            };
            switches = new string[1] { "codepage" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_CODEPAGE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_CODEPAGE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Debug
            //--------------------------------------------------------
            infoID = OptionInfoID.Debug;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Debug,
            };
            switches = new string[1] { "debug" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_DEBUG };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_DEBUG };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_DebugType
            //--------------------------------------------------------
            infoID = OptionInfoID.DebugType;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_DebugType,
            };
            switches = new string[1] { "debug" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_DEBUGTYPE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_DEBUGTYPE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_DefineCCSymbols
            //--------------------------------------------------------
            infoID = OptionInfoID.DefineCCSymbols;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_DefineCCSymbols,
            };
            switches = new string[2] { "define", "d" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_DEFINECCSYMBOLS };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_DEFINECCSYMBOLS };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Preprocessor;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_DelaySign, AL_DelaySign
            //--------------------------------------------------------
            infoID = OptionInfoID.DelaySign;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_DelaySign,
                CommandOptionID.AL_DelaySign,
            };
            switches = new string[1] { "delaysign" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_DELAYSIGN };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_DELAYSIGN };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Assemblies |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyDelaySignAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_XMLDocFile
            //--------------------------------------------------------
            infoID = OptionInfoID.XMLDocFile;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_XMLDocFile,
            };
            switches = new string[1] { "doc" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_XMLDOCFILE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_XML_DOCFILE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_OutputFiles;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_ErrorReport
            //--------------------------------------------------------
            infoID = OptionInfoID.ErrorReport;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_ErrorReport,
            };
            switches = new string[1] { "errorreport" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_ERRORREPORT };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_ERRORREPORT };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_FileAlign
            //--------------------------------------------------------
            infoID = OptionInfoID.FileAlign;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_FileAlign,
            };
            switches = new string[1] { "filealign" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_FILEALIGN };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_ALIGN };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Optimization;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_FullPaths, AL_FullPaths
            //--------------------------------------------------------
            infoID = OptionInfoID.FullPaths;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_FullPaths,
                CommandOptionID.AL_FullPaths,
            };
            switches = new string[1] { "fullpaths" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_FULLPATHS };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_FULLPATH };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Debugging |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_HighEntropyVa
            //--------------------------------------------------------
            infoID = OptionInfoID.HighEntropyVa;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_HighEntropyVa,
            };
            switches = new string[1] { "highentropyva" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_HIGHENTROPYVA };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_HIGHENTROPYVA };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_KeyContainer, AL_KeyContainer
            //--------------------------------------------------------
            infoID = OptionInfoID.KeyContainer;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_KeyContainer,
                CommandOptionID.AL_KeyContainer,
            };
            switches = new string[3] { "keycontainer", "keyname", "keyn" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_KEYCONTAINER };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_KEYCONTAINER };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Assemblies |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyKeyNameAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_KeyFile, AL_KeyFile
            //--------------------------------------------------------
            infoID = OptionInfoID.KeyFile;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_KeyFile,
                CommandOptionID.AL_KeyFile,
            };
            switches = new string[2] { "keyfile", "keyf" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_KEYFILE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_KEYFILE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Assemblies |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyKeyFileAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_LangVersion
            //--------------------------------------------------------
            infoID = OptionInfoID.LangVersion;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_LangVersion,
            };
            switches = new string[1] { "langversion", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_LANGVERSION };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_COMPATIBILITY };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_LibPath
            //--------------------------------------------------------
            infoID = OptionInfoID.LibPath;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_LibPath,
            };
            switches = new string[1] { "lib", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_LIBPATH };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_LIBPATH };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Link
            //--------------------------------------------------------
            infoID = OptionInfoID.Link;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Link,
            };
            switches = new string[1] { "link", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_LINK };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_LINK };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Resources;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_LinkResource, AL_LinkResource
            //--------------------------------------------------------
            infoID = OptionInfoID.LinkResource;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_LinkResource,
                CommandOptionID.AL_LinkResource,
            };
            switches = new string[2] { "linkresource", "linkres", };    // "link" in al.exe
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_LINKRESOURCE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_LINKRESOURCE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Resources |
                CommandOptionFlag.GRP_AL_Source;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Main, AL_Main
            //--------------------------------------------------------
            infoID = OptionInfoID.Main;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_Main,
                CommandOptionID.AL_Main,
            };
            switches = new string[1] { "main",};
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_MAIN };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_MAIN };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Miscellaneous |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_ModuleAssemblyName
            //--------------------------------------------------------
            infoID = OptionInfoID.ModuleAssemblyName;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_ModuleAssemblyName,
            };
            switches = new string[1] { "moduleassemblyname", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_MODULEASSEMBLYNAME };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_MODULEASSEMBLYNAME };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_ModuleName
            //--------------------------------------------------------
            infoID = OptionInfoID.ModuleName;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_ModuleName,
            };
            switches = new string[1] { "modulename", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_MODULENAME };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_MODULENAME };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_NoConfig
            //--------------------------------------------------------
            infoID = OptionInfoID.NoConfig;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_NoConfig,
            };
            switches = new string[1] { "noconfig", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_NOCONFIG };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_NOCONFIG };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_NoLogo, AL_NoLogo
            //--------------------------------------------------------
            infoID = OptionInfoID.NoLogo;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_NoLogo,
                CommandOptionID.AL_NoLogo,
            };
            switches = new string[1] { "nologo", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_NOLOGO };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_NOLOGO };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Miscellaneous |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_NoStdLib
            //--------------------------------------------------------
            infoID = OptionInfoID.NoStdLib;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_NoStdLib,
            };
            switches = new string[1] { "nostdlib", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_NOSTDLIB };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_NOSTDLIB };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_NoWarnList
            //--------------------------------------------------------
            infoID = OptionInfoID.NoWarnList;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_NoWarnList,
            };
            switches = new string[1] { "nowarn", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_NOWARNLIST };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_NOWARNLIST };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_NoWin32Manifest
            //--------------------------------------------------------
            infoID = OptionInfoID.NoWin32Manifest;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_NoWin32Manifest,
            };
            switches = new string[1] { "nowin32manifest", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_NOWIN32MANIFEST };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_NOWIN32MANIFEST };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Optimize
            //--------------------------------------------------------
            infoID = OptionInfoID.Optimize;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Optimize,
            };
            switches = new string[2] { "optimize", "o", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_OPTIMIZE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_OPTIMIZATIONS };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Optimization;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_OutputFile, AL_OutputFile
            //--------------------------------------------------------
            infoID = OptionInfoID.OutputFile;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_OutputFile,
                CommandOptionID.AL_OutputFile,
            };
            switches = new string[1] { "out", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_OUTPUTFILENAME };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_OUTPUTFILENAME };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_OutputFiles |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Parallel
            //--------------------------------------------------------
            infoID = OptionInfoID.Parallel;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Parallel,
            };
            switches = new string[1] { "parallel", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_PARALLEL };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_PARALLEL };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_PDBFile
            //--------------------------------------------------------
            infoID = OptionInfoID.PDBFile;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_PDBFile,
            };
            switches = new string[1] { "pdb", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_PDBFILENAME };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_PDBFILENAME };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_OutputFiles;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Platform, AL_Platform
            //--------------------------------------------------------
            infoID = OptionInfoID.Platform;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_Platform,
                CommandOptionID.AL_Platform,
            };
            switches = new string[1] { "platform", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_PLATFORM };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_PLATFORM };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_OutputFiles |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_PreferredUILang
            //--------------------------------------------------------
            infoID = OptionInfoID.PreferredUILang;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_PreferredUILang,
            };
            switches = new string[1] { "preferreduilang", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_PREFERREDUILANG };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_PREFERREDUILANG };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_OutputFiles;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Recurse
            //--------------------------------------------------------
            infoID = OptionInfoID.Recurse;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Recurse,
            };
            switches = new string[1] { "recurse", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_RECURSE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_RECURSE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Reference
            //--------------------------------------------------------
            infoID = OptionInfoID.Reference;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Reference,
            };
            switches = new string[2] { "reference", "r", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_REFERENCE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_REFERENCE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Resource, AL_Resource
            //--------------------------------------------------------
            infoID = OptionInfoID.Resource;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_Resource,
                CommandOptionID.AL_Resource,
            };
            switches = new string[4] { "resource", "res", "embed", "embedresource" };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_EMBEDRESOURCE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_EMBEDRESOURCE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Resources |
                CommandOptionFlag.GRP_AL_Source;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_RuleSet
            //--------------------------------------------------------
            infoID = OptionInfoID.RuleSet;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_RuleSet,
            };
            switches = new string[1] { "ruleset", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_RULESET };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_RULESET };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_SubSystemVersion
            //--------------------------------------------------------
            infoID = OptionInfoID.SubSystemVersion;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_SubSystemVersion,
            };
            switches = new string[1] { "subsystemversion", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_SUBSYSTEMVERSION };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_SUBSYSTEMVERSION };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Target, AL_Target
            //--------------------------------------------------------
            infoID = OptionInfoID.Target;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_Target,
                CommandOptionID.AL_Target,
            };
            switches = new string[2] { "target", "t", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_TARGET };
            descs = new ResNo[5]
            {
                ResNo.CSCSTR_OPTDSC_TARGET,
                ResNo.CSCSTR_OPTDSC_TARGET_EXE,
                ResNo.CSCSTR_OPTDSC_TARGET_WINEXE,
                ResNo.CSCSTR_OPTDSC_TARGET_DLL,
                ResNo.CSCSTR_OPTDSC_TARGET_MODULE,
            };
            //CSCSTR_OPTDSC_TARGET_MODULE, CSCSTR_OPTDSC_TARGET_EXE, CSCSTR_OPTDSC_TARGET_WINEXE, CSCSTR_OPTDSC_TARGET_DLL
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_OutputFiles |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Unsafe
            //--------------------------------------------------------
            infoID = OptionInfoID.Unsafe;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Unsafe,
            };
            switches = new string[1] { "unsafe", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_UNSAFE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_UNSAFE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_UTF8Output
            //--------------------------------------------------------
            infoID = OptionInfoID.UTF8Output;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_UTF8Output,
            };
            switches = new string[1] { "utf8output", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_UTF8OUTPUT };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_UTF8OUTPUT };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Miscellaneous;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_WarningLevel
            //--------------------------------------------------------
            infoID = OptionInfoID.WarningLevel;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_WarningLevel,
            };
            switches = new string[1] { "warn", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_WARNINGLEVEL };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_WARNINGLEVEL };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_WarningsAsErrors
            //--------------------------------------------------------
            infoID = OptionInfoID.WarningsAsErrors;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_WarningsAsErrors,
            };
            switches = new string[1] { "warnaserror", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_WARNINGSASERRORS };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_WARNINGSAREERRORS };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, true, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_WarnAsErrorList
            //--------------------------------------------------------
            infoID = OptionInfoID.WarnAsErrorList;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_WarnAsErrorList,
            };
            switches = new string[1] { "warnaserror", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_WARNASERRORLIST };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_WARNASERRORLIST };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Debugging;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Win32Icon, AL_Win32Icon
            //--------------------------------------------------------
            infoID = OptionInfoID.Win32Icon;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_Win32Icon,
                CommandOptionID.AL_Win32Icon,
            };
            switches = new string[1] { "win32icon", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_WIN32ICON };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_WIN32ICON };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Resources |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Win32Manifest
            //--------------------------------------------------------
            infoID = OptionInfoID.Win32Manifest;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.CSC_Win32Manifest,
            };
            switches = new string[1] { "win32manifest", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_WIN32MANIFEST };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_WIN32MANIFEST };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.GRP_CSC_Assemblies;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // CSC_Win32Resource, AL_Win32Resource
            //--------------------------------------------------------
            infoID = OptionInfoID.Win32Resource;
            optIDs = new CommandOptionID[2]
            {
                CommandOptionID.CSC_Win32Resource,
                CommandOptionID.AL_Win32Resource,
            };
            switches = new string[1] { "win32res", };
            syntax = new ResNo[1] { ResNo.CSCSTR_OPTSYN_WIN32RESOURCE };
            descs = new ResNo[1] { ResNo.CSCSTR_OPTDSC_WIN32RESOURCE };
            flags =
                CommandOptionFlag.CMD_CSC |
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_CSC_Resources |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_ArgorithmID
            //--------------------------------------------------------
            infoID = OptionInfoID.ArgorithmID;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_ArgorithmID,
            };
            switches = new string[1] { "algid", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_ARGORITHMID };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_ARGORITHMID };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyAlgorithmIdAttribute";

            AddOptionInfo(infoID, true, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Company
            //--------------------------------------------------------
            infoID = OptionInfoID.Company;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Company,
            };
            switches = new string[2] { "company", "comp", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_COMPANY};
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_COMPANY };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyCompanyAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Configuration
            //--------------------------------------------------------
            infoID = OptionInfoID.Configuration;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Configuration,
            };
            switches = new string[2] { "configuration", "config", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_CONFIGURATION };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_CONFIGURATION };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyCompanyAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Copyright
            //--------------------------------------------------------
            infoID = OptionInfoID.Copyright;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Copyright,
            };
            switches = new string[2] { "copyright", "copy", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_COPYRIGHT };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_COPYRIGHT };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyCopyrightAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Culture
            //--------------------------------------------------------
            infoID = OptionInfoID.Culture;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Culture,
            };
            switches = new string[2] { "culture", "c", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_CULTURE };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_CULTURE };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyCultureAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Description
            //--------------------------------------------------------
            infoID = OptionInfoID.Description;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Description,
            };
            switches = new string[2] { "description", "descr", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_DESCRIPTION };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_DESCRIPTION };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyDescriptionAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Evidence
            //--------------------------------------------------------
            infoID = OptionInfoID.Evidence;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Evidence,
            };
            switches = new string[2] { "evidence", "e", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_EVIDENCE };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_EVIDENCE };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_FileVersion
            //--------------------------------------------------------
            infoID = OptionInfoID.FileVersion;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_FileVersion,
            };
            switches = new string[1] { "fileversion", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_FILEVERSION };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_FILEVERSION };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyFileVersionAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Flags
            //--------------------------------------------------------
            infoID = OptionInfoID.Flags;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Flags,
            };
            switches = new string[1] { "flags", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_FLAGS };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_FLAGS };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyFlagsAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Product
            //--------------------------------------------------------
            infoID = OptionInfoID.Product;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Product,
            };
            switches = new string[2] { "product", "prod", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_PRODUCT };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_PRODUCT };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyProductAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_ProductVersion
            //--------------------------------------------------------
            infoID = OptionInfoID.ProductVersion;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_ProductVersion,
            };
            switches = new string[2] { "productversion", "productv", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_PRODUCTVERSION };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_PRODUCTVERSION };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyInformationalVersionAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Template
            //--------------------------------------------------------
            infoID = OptionInfoID.Template;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Template,
            };
            switches = new string[1] { "template", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_TEMPLATE };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_TEMPLATE };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = null;

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Title
            //--------------------------------------------------------
            infoID = OptionInfoID.Title;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Title,
            };
            switches = new string[1] { "title", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_TITLE };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_TITLE };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyTitleAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Trademark
            //--------------------------------------------------------
            infoID = OptionInfoID.Trademark;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Trademark,
            };
            switches = new string[2] { "trademark", "trade", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_TRADEMARK };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_TRADEMARK };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyTrademarkAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);

            //--------------------------------------------------------
            // AL_Version
            //--------------------------------------------------------
            infoID = OptionInfoID.Version;
            optIDs = new CommandOptionID[1]
            {
                CommandOptionID.AL_Version,
            };
            switches = new string[2] { "version", "v", };
            syntax = new ResNo[1] { ResNo.ALSTR_OPTSYN_VERSION };
            descs = new ResNo[1] { ResNo.ALSTR_OPTDSC_VERSION };
            flags =
                CommandOptionFlag.CMD_AL |
                CommandOptionFlag.GRP_AL_Option;
            attr = "System.Reflection.AssemblyVersionAttribute";

            AddOptionInfo(infoID, false, optIDs, switches, false, syntax, descs, flags, attr);
        }

        //------------------------------------------------------------
        // CSCHelpGroups
        //------------------------------------------------------------
        private static CommandOptionFlag[] CSCHelpGroupFlags =
        {
            //CommandOptionFlag.GRP_CSC_Help,
            CommandOptionFlag.GRP_CSC_Optimization,
            CommandOptionFlag.GRP_CSC_OutputFiles,
            CommandOptionFlag.GRP_CSC_Assemblies,
            CommandOptionFlag.GRP_CSC_Debugging,
            CommandOptionFlag.GRP_CSC_Preprocessor,
            CommandOptionFlag.GRP_CSC_Resources,
            CommandOptionFlag.GRP_CSC_Miscellaneous,
            //CommandOptionFlag.GRP_CSC_Obsolete,
        };

        private static ResNo[] CSCHelpGroupIDs =
        {
            ResNo.CSCSTR_OPTGRP_OPTIMIZATION,
            ResNo.CSCSTR_OPTGRP_OUTPUT,
            ResNo.CSCSTR_OPTGRP_ASSEMBLIES,
            ResNo.CSCSTR_OPTGRP_DEBUGGING,
            ResNo.CSCSTR_OPTGRP_PROPRCESSOR,
            ResNo.CSCSTR_OPTGRP_RESOURCES,
            ResNo.CSCSTR_OPTGRP_MISC,
        };

        private static CommandOptionFlag[] ALHelpGroupFlags =
        {
            //CommandOptionFlag.GRP_AL_Help,
            CommandOptionFlag.GRP_AL_Source,
            CommandOptionFlag.GRP_AL_Option,
            //CommandOptionFlag.GRP_AL_Obsolete,
        };

        private static ResNo[] ALHelpGroupIDs =
        {
            ResNo.ALSTR_OPTGRP_SOURCES,
            ResNo.ALSTR_OPTGRP_OPTIONS,
        };

        //------------------------------------------------------------
        // PrintCSCHelp
        //
        /// <summary></summary>
        /// <param name="cout"></param>
        //------------------------------------------------------------
        internal static void PrintCSCHelp(ConsoleOutput cout)
        {
            Exception excp = null;

            for (int i = 0; i < CSCHelpGroupFlags.Length; ++i)
            {
                string grpName;
                if (CResources.GetString(CSCHelpGroupIDs[i], out grpName, out excp))
                {
                    cout.WriteLine(grpName);
                }
                else
                {
                    cout.WriteLine("- Option Group {0} -", i + 1);
                }
                cout.WriteLine();

                ResNo[] resNos = null;
                string resStr = null;

                foreach (CommandOptionInfo info in InfoList)
                {
                    if (info.Disabled || (info.Flags & CSCHelpGroupFlags[i]) == 0)
                    {
                        continue;
                    }

                    resNos = info.Syntax;
                    if (resNos != null && resNos.Length > 0)
                    {
                        for (int j = 0; j < resNos.Length; ++j)
                        {
                            if (CResources.GetString(resNos[j], out resStr, out excp))
                            {
                                cout.WriteLine(resStr);
                            }
                        }
                    }

                    resNos = info.Descriptions;
                    if (resNos != null && resNos.Length > 0)
                    {
                        for (int j = 0; j < resNos.Length; ++j)
                        {
                            if (CResources.GetString(resNos[j], out resStr, out excp))
                            {
                                cout.WriteLine(resStr);
                            }
                        }
                    }
#if false
                    if (!CResources.GetString(info.SyntaxID, out syntaxStr, out excp))
                    {
                        if (excp != null)
                        {
                            cout.ShowMessage(excp.Message);
                        }
                        syntaxStr = "";
                    }

                    if (info.DescriptonID == null ||
                        !CResources.GetString(info.DescriptonID, out descStr, out excp))
                    {
                        //if (excp != null)
                        //{
                        //    cout.ShowMessage(excp.Message);
                        //}
                        descStr = null;
                    }

                    cout.WriteLine(syntaxStr);
                    if (descStr != null)
                    {
                        cout.WriteLine(descStr);
                    }
#endif
                    cout.WriteLine();
                }
            }
        }

        //------------------------------------------------------------
        // PrintALHelp
        //
        /// <summary></summary>
        /// <param name="cout"></param>
        //------------------------------------------------------------
        internal static void PrintALHelp(ConsoleOutput cout)
        {
            Exception excp = null;

            for (int i = 0; i < ALHelpGroupFlags.Length; ++i)
            {
                string grpName;
                if (CResources.GetString(ALHelpGroupIDs[i], out grpName, out excp))
                {
                    cout.WriteLine(grpName);
                }
                else
                {
                    cout.WriteLine("- Option Group {0} -", i + 1);
                }
                cout.WriteLine();

                ResNo[] resNos = null;
                string resStr = null;

                foreach (CommandOptionInfo info in InfoList)
                {
                    if (info.Disabled || (info.Flags & ALHelpGroupFlags[i]) == 0)
                    {
                        continue;
                    }

                    resNos = info.Syntax;
                    if (resNos != null && resNos.Length > 0)
                    {
                        for (int j = 0; j < resNos.Length; ++j)
                        {
                            if (CResources.GetString(resNos[j], out resStr, out excp))
                            {
                                cout.WriteLine(resStr);
                            }
                        }
                    }

                    resNos = info.Descriptions;
                    if (resNos != null && resNos.Length > 0)
                    {
                        for (int j = 0; j < resNos.Length; ++j)
                        {
                            if (CResources.GetString(resNos[j], out resStr, out excp))
                            {
                                cout.WriteLine(resStr);
                            }
                        }
                    }
#if false
                    if (!CResources.GetString(info.SyntaxID, out syntaxStr, out excp))
                    {
                        if (excp != null)
                        {
                            cout.ShowMessage(excp.Message);
                        }
                        syntaxStr = "";
                    }
                    if (info.DescriptonID == null ||
                        !CResources.GetString(info.DescriptonID, out descStr, out excp))
                    {
                        //if (excp != null)
                        //{
                        //    cout.ShowMessage(excp.Message);
                        //}
                        descStr = null;
                    }

                    cout.WriteLine(syntaxStr);
                    if (descStr != null)
                    {
                        cout.WriteLine(descStr);
                    }
#endif
                    cout.WriteLine();
                }
            }
        }
    }
}
