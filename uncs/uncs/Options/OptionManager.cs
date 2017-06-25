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
// Options.cs
//
// 2015/10/26 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // enum CommandID
    //
    /// <summary>
    /// <para>(uncs\Option\OptionManager.cs)</para>
    /// </summary>
    //======================================================================
    internal enum CommandID : int
    {
        CSC = 0,
        AL,
    }

    //======================================================================
    // class ResourceOptionInfo
    //
    /// <summary>
    /// <para>(uncs\Option\OptionManager.cs)</para>
    /// </summary>
    //======================================================================
    internal class ResourceOptionInfo
    {
        internal FileInfo FileInfo = null;
        internal string LogicalName = null;
        internal bool IsPrivate = false;

        //------------------------------------------------------------
        // ResourceOptionInfo Constructor
        //
        /// <summary></summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="isPrivate"></param>
        //------------------------------------------------------------
        internal ResourceOptionInfo(FileInfo info, string name, bool isPrivate)
        {
            this.FileInfo = info;
            this.LogicalName = name;
            this.IsPrivate = isPrivate;
        }
    }

    //======================================================================
    // class COptionManager
    //
    /// <summary>
    /// <para>Derives from CCoreOptionData.</para>
    /// </summary>
    //======================================================================
    internal partial class COptionManager
    {
        //------------------------------------------------------------
        // COptionManager Fields (1)
        //------------------------------------------------------------
        internal CController Controller = null;

        internal string[] SourceArguments = null;
        internal List<string[]> OptionList = null;

        //------------------------------------------------------------
        // COptionManager Fields (2)
        //------------------------------------------------------------
        // CSC_ResponseFile
        internal bool ShowHelp = false;                 // CSC_Help
        // CSC_AdditionalFile
        internal List<string> ModuleList = null;	    // CSC_AddModule
        // CSC_Analyzer
        internal FileInfo AppConfigFileInfo = null;     // CSC_AppConfig
        //internal ulong BaseAddress = 0;	                // CSC_BaseAddress, AL_BaseAddress
        internal FileInfo BugReportFileInfo = null;     // CSC_BugReport, AL_BugReport
        internal bool CheckOverflow = false;            // CSC_Checked
        // CSC_CheckSumAlgorithm
        internal Encoding Encoding = null;	            // CSC_Codepage
        internal bool GenerateDebugInfo = false;	    // CSC_Debug
        internal bool IsDebugInfoPDBOnly = false;	    // CSC_DebugType
        internal List<string> DefinedCCSymbolList = null;	// CSC_DefineCCSymbols
        internal bool? DelaySign = null;	            // CSC_DelaySign, AL_DelaySign
        internal FileInfo XMLDocFileInfo = null;        // CSC_XMLDocFile
        internal ErrorReportEnum ErrorReport = ErrorReportEnum.None;	// CSC_ErrorReport
        //internal uint SectionSize = 0;	                // CSC_FileAlign
        internal bool FullPaths = false;	            // CSC_FullPaths, AL_FullPaths
        internal bool HighEntropyVa = false;	        // CSC_HighEntropyVa
        //internal bool IncremantalBuild = false;	        // CSC_IncrementalBuild	obsolete
        internal string KeyContainer = null;	        // CSC_KeyContainer, AL_KeyContainer
        internal FileInfo KeyFileInfo = null;           // CSC_KeyFile, AL_KeyFile
        internal LangVersionEnum LangVersion = LangVersionEnum.Default;	// CSC_LangVersion
        internal List<string> LibPathList = null;	    // CSC_LibPath
        internal List<string> LinkList = null;	        // CSC_Link
        //internal List<ResourceOptionInfo> LinkResourceList = null;	// CSC_LinkResource, AL_LinkResource
        //internal string EntryPoint = null;              // CSC_Main, AL_Main
        internal string ModuleAssemblyName = null;	    // CSC_ModuleAssemblyName
        internal string SourceModuleName = null;	    // CSC_ModuleName
        internal bool NoConfig = false;	                // CSC_NoConfig
        internal bool NoLogo = false;	                // CSC_NoLogo, AL_NoLogo
        internal bool NoStdLib = false;	                // CSC_NoStdLib
        internal List<int> NoWarnList = null;	        // CSC_NoWarnList
        internal bool NoWin32Manifest = false;	        // CSC_NoWin32Manifest
        internal bool Optimize = false;	                // CSC_Optimize
        //internal FileInfo OutputFileInfo = null;        // CSC_OutputFile, AL_OutputFile
        internal bool ConcurrentBuild = false;	        // CSC_Parallel
        //internal FileInfo PDBFileInfo = null;           // CSC_PDBFile
        internal PlatformEnum Platform = PlatformEnum.AnyCPU;	// CSC_Platform, AL_Platform
        internal string PreferredUILangName = null;	    // CSC_PreferredUILang
        internal string Recurse = null;	                // CSC_Recurse
        internal List<string[]> ImportList = null;	    // CSC_Reference
        //internal List<ResourceOptionInfo> ResourceList = null;  // CSC_Resource, AL_Resource
        internal FileInfo RuleSetFileInfo = null;       // CSC_RuleSet
        internal Version SubSystemVersion = null;	    // CSC_SubSystemVersion
        //internal TargetType Target = TargetType.Exe;	// CSC_Target, AL_Target
        internal bool Unsafe = false;	                // CSC_Unsafe
        //internal bool UTF8Output = false;	            // CSC_UTF8Output
        internal int WarningLevel = 4;	                // CSC_WarningLevel
        //internal bool WarningsAsErrors = false;	        // CSC_WarningsAsErrors
        internal bool[] WarnAsErrorArray = null;	    // CSC_WarnAsErrorList
        //internal FileInfo Win32IconFileInfo = null;	    // CSC_Win32Icon, AL_Win32Icon
        internal FileInfo Win32ManifestFileInfo = null;   // CSC_Win32Manifest
        //internal FileInfo Win32ResourceFileInfo = null;   // CSC_Win32Resource

        // CSC_EmitDebugInfo
        // CSC_InternalTests
        // CSC_NoCodeGen
        // CSC_Timing
        // CSC_WatsonMode
        // CSC_PDBAltPath
        // CSC_SourceMap
        // CSC_CompilerSkeleton

        // AL_Resource, CSC_Resource
        // AL_LinkResource, CSC_LinkResource

        // AL_ArgorithmID
        // AL_BaseAddress -> CSC_BaseAddress
        // AL_BugReport -> CSC_BugReport
        internal string Company = null;	            // AL_Company
        internal string Configuration = null;	    // AL_Configuration
        internal string Copyright = null;	        // AL_Copyright
        internal CultureInfo Culture = null;	    // AL_Culture
        // AL_DelaySign -> CSC_DelaySign
        internal string Description = null;	        // AL_Description
        internal FileInfo EvidenceFileInfo = null;  // AL_Evidence
        internal string AssemblyFileVersion = null;	// AL_FileVersion
        internal AssemblyNameFlags? AssemblyNameFlags = null;	// AL_Flags
        // AL_FullPaths -> CSC_FullPaths
        // AL_KeyFile -> CSC_KeyFile
        // AL_KeyContainer -> CSC_KeyContainer
        // AL_Main -> CSC_Main
        // AL_NoLogo -> CSC_NoLogo
        // AL_OutputFile -> CSC_OutputFile
        // AL_Platform -> CSC_Platform
        internal string Product = null;	        // AL_Product
        internal string ProductVersion = null;	// AL_ProductVersion
        // AL_Target -> CSC_Target
        internal FileInfo TemplateFileInfo = null;  // AL_Template
        internal string Title = null;	        // AL_Title
        internal string Trademark = null;	    // AL_Trademark
        internal Version Version = null;        // AL_Version
        // AL_Win32Icon -> CSC_Win32Icon
        // AL_Win32Resource -> CSC_Win32Resource
        // AL_ResponseFile
        // AL_Help

        // AL_OS
        // AL_SatelliteVersion

        //------------------------------------------------------------
        // COptionManager Fields (3)
        //------------------------------------------------------------
        internal TargetType DefaultTarget = TargetType.Exe;

        //------------------------------------------------------------
        // COptionManager Properties
        //------------------------------------------------------------
        internal bool IsLangVersionDefault
        {
            get { return (this.LangVersion == LangVersionEnum.Default); }
        }

        internal bool IsLangVersionECMA1
        {
            get { return (this.LangVersion == LangVersionEnum.ECMA1); }
        }

        //------------------------------------------------------------
        // COptionManager Constructor
        //
        /// <summary></summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal COptionManager(CController cntr)
        {
            DebugUtil.Assert(cntr != null);
            this.Controller = cntr;

            this.WarnAsErrorArray = new bool[OptionUtil.WarningNumbers.Length];
            SetWarnAsErrorArray(false);
        }

        //------------------------------------------------------------
        // COptionManager.IsNoWarnNumber
        //
        /// <summary></summary>
        /// <param name="errNo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsNoWarnNumber(int errNo)
        {
            if (this.NoWarnList != null)
            {
                return this.NoWarnList.Contains(errNo);
            }
            return false;
        }

        //------------------------------------------------------------
        // COptionManager.SetWarnAsErrorArray
        //
        /// <summary></summary>
        /// <param name="bVal"></param>
        //------------------------------------------------------------
        internal void SetWarnAsErrorArray(bool bVal)
        {
            for (int i = 0; i < this.WarnAsErrorArray.Length; ++i)
            {
                this.WarnAsErrorArray[i] = bVal;
            }
        }

        //------------------------------------------------------------
        // COptionManager.SetWarnAsError
        //
        /// <summary></summary>
        /// <param name="errNo"></param>
        /// <param name="bVal"></param>
        //------------------------------------------------------------
        internal void SetWarnAsError(int errNo, bool bVal)
        {
            DebugUtil.Assert(
                OptionUtil.WarningNumbers != null &&
                this.WarnAsErrorArray != null);

            int idx = Array.BinarySearch(OptionUtil.WarningNumbers, errNo);
            if (idx >= 0 && idx < this.WarnAsErrorArray.Length)
            {
                this.WarnAsErrorArray[idx] = bVal;
            }
        }

        //------------------------------------------------------------
        // COptionManager.IsWarnAsError (1)
        //
        /// <summary></summary>
        /// <param name="errNo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsWarnAsError(int errNo)
        {
            DebugUtil.Assert(
                OptionUtil.WarningNumbers != null &&
                this.WarnAsErrorArray != null);

            int idx = Array.BinarySearch(OptionUtil.WarningNumbers, errNo);
            if (idx >= 0 && idx < this.WarnAsErrorArray.Length)
            {
                return this.WarnAsErrorArray[idx];
            }
            return false;
        }

        //------------------------------------------------------------
        // COptionManager.IsWarnAsError (2)
        //
        /// <summary></summary>
        /// <param name="errID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsWarnAsError(CSCERRID errID)
        {
            int errNo = CSCErrorInfo.Manager.GetErrorNumber(errID);
            if (errNo >= 0)
            {
                return IsWarnAsError(errNo);
            }
            return false;
        }

        //------------------------------------------------------------
        // COptionManager.SetCommandArguments
        //
        /// <summary></summary>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void SetCommandArguments(string[] args)
        {
            if (args == null)
            {
                this.SourceArguments = new string[0];
            }
            else
            {
                this.SourceArguments = args;
            }

            if (this.OptionList == null)
            {
                this.OptionList = new List<string[]>();
            }
            OptionUtil.ArgumentsToOptionList(
                this.SourceArguments,
                this.SourceArguments.Length,
                this.OptionList);
        }

        //------------------------------------------------------------
        // COptionManager.GetOptionInfoID
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal OptionInfoID GetOptionInfoID(string[] opt)
        {
            if (opt == null || opt.Length <= 1)
            {
                return OptionInfoID.Invalid;
            }
            if (opt.Length == 2)
            {
                return OptionInfoID.Source;
            }

            string switchStr = opt[2];
            if (String.IsNullOrEmpty(switchStr))
            {
                return OptionInfoID.Invalid;
            }

            CommandOptionInfo info = null;
            if (opt.Length == 3)
            {
                return OptionInfoManager.GetOptionInfo(switchStr, true, out info);
            }

            if (opt[3] == "+" || opt[3] == "-")
            {
                return OptionInfoManager.GetOptionInfo(switchStr, true, out info);
            }

            return OptionInfoManager.GetOptionInfo(switchStr, false, out info);
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToFile
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="fInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToFile(string[] opt, ref FileInfo fInfo)
        {
            fInfo = null;

            if (opt.Length <= 3 || String.IsNullOrEmpty(opt[3]))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            Exception excp = null;
            string fileName = opt[3];

            IOUtil.RemoveQuotes(ref fileName);
            if (String.IsNullOrEmpty(fileName))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            if (!IOUtil.CreateFileInfo(fileName, out fInfo, out excp))
            {
                if (excp != null)
                {
                    this.Controller.ReportError(ERRORKIND.ERROR, excp);
                }
                else
                {
                }
                return false;
            }

            if (opt.Length > 4)
            {
                // too many arguments
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ParseBooleanOption
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseBooleanOption(string[] opt, ref bool bValue)
        {
            if (opt.Length <= 2)
            {
                return false;
            }

            if (opt.Length == 3 || opt[3] == "+")
            {
                bValue = true;
                return true;
            }
            else if (opt[3] == "-")
            {
                bValue = false;
                return true;
            }
            else
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_BadSwitch,
                    ERRORKIND.ERROR,
                    opt[0]);
            }
            return false;
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToOneString
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToOneString(string[] opt, ref string strValue)
        {
            if (opt.Length <= 3 || String.IsNullOrEmpty(opt[3]))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            strValue = opt[3];
            IOUtil.RemoveQuotes(ref strValue);
            if (String.IsNullOrEmpty(strValue))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            if (opt.Length > 4)
            {
                // too many arguments, show an error message.
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToStringList
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="strList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToStringList(
            string[] opt,
            ref List<string> strList,
            string separator,
            bool eliminateEmptyString)
        {
            DebugUtil.Assert(opt != null);
            if (opt.Length <= 3 || String.IsNullOrEmpty(opt[3]))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }
            if (strList == null)
            {
                strList = new List<string>();
            }

            List<string> tempList = null;
            for (int i = 3; i < opt.Length; ++i)
            {
                tempList = Util.SplitString(
                    opt[i],
                    separator,
                    eliminateEmptyString);
                if (tempList != null && tempList.Count > 0)
                {
                    strList.AddRange(tempList);
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToInt32
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="iVal"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToInt32(string[] opt, ref int iVal)
        {
            if (opt.Length <= 3 || String.IsNullOrEmpty(opt[3]))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            string strValue = opt[3];
            IOUtil.RemoveQuotes(ref strValue);
            if (String.IsNullOrEmpty(strValue))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            Exception excp;
            if (!Util.StringToInt32(strValue, out iVal, out excp))
            {
                this.Controller.ReportError(
                    ALERRID.ERR_BadOptionValue,
                    ERRORKIND.ERROR,
                    opt[2], opt[0]);
                return false;
            }

            if (opt.Length > 4)
            {
                // too many arguments, show an error message.
                this.Controller.ReportError(
                    ALERRID.ERR_BadOptionValue,
                    ERRORKIND.ERROR,
                    opt[2], opt[0]);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToUInt32
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="uiVal"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToUInt32(string[] opt, ref uint uiVal)
        {
            if (opt.Length <= 3 || String.IsNullOrEmpty(opt[3]))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            string strValue = opt[3];
            IOUtil.RemoveQuotes(ref strValue);
            if (String.IsNullOrEmpty(strValue))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            Exception excp;
            if (!Util.StringToUInt32(strValue, out uiVal, out excp))
            {
                this.Controller.ReportError(
                    ALERRID.ERR_BadOptionValue,
                    ERRORKIND.ERROR,
                    opt[2], opt[0]);
                return false;
            }

            if (opt.Length > 4)
            {
                // too many arguments, show an error message.
                this.Controller.ReportError(
                    ALERRID.ERR_BadOptionValue,
                    ERRORKIND.ERROR,
                    opt[2], opt[0]);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToUInt64
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="ulVal"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToUInt64(string[] opt, ref ulong ulVal)
        {
            if (opt.Length <= 3 || String.IsNullOrEmpty(opt[3]))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            string strValue = opt[3];
            IOUtil.RemoveQuotes(ref strValue);
            if (String.IsNullOrEmpty(strValue))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }

            Exception excp;
            if (!Util.StringToUInt64(strValue, out ulVal, out excp))
            {
                this.Controller.ReportError(
                    ALERRID.ERR_BadOptionValue,
                    ERRORKIND.ERROR,
                    opt[2], opt[0]);
                return false;
            }

            if (opt.Length > 4)
            {
                // too many arguments, show an error message.
                this.Controller.ReportError(
                    ALERRID.ERR_BadOptionValue,
                    ERRORKIND.ERROR,
                    opt[2], opt[0]);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToIntergerList
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="strList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToIntergerList(
            string[] opt,
            ref List<int> intList,
            string separator)
        {
            DebugUtil.Assert(opt != null);
            if (opt.Length <= 3 || String.IsNullOrEmpty(opt[3]))
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[0]);
                return false;
            }
            if (intList == null)
            {
                intList = new List<int>();
            }

            List<string> tempList = null;
            bool invalidArg = false;

            for (int i = 3; i < opt.Length; ++i)
            {
                tempList = Util.SplitString(
                    opt[i],
                    separator,
                    true);

                if (tempList != null && tempList.Count > 0)
                {
                    foreach (string str in tempList)
                    {
                        int val;

                        if (Int32.TryParse(str, out val))
                        {
                            if (!intList.Contains(val))
                            {
                                intList.Add(val);
                            }
                        }
                        else
                        {
                            invalidArg = true;
                        }
                    }
                }
            }

            if (invalidArg)
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_SwitchNeedsNumber,
                    ERRORKIND.ERROR,
                    opt[2]);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ParseOptionToVersion
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="versionObj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseOptionToVersion(string[] opt, ref Version versionObj)
        {
            string strValue = null;
            Version ver = null;
            Exception excp;

            if (ParseOptionToOneString(opt, ref strValue))
            {
                if (Util.CreateVersion(
                        strValue,
                        out ver,
                        out excp))
                {
                    versionObj = ver;
                    return true;
                }
            }

            this.Controller.ReportError(
                ALERRID.ERR_BadOptionValue,
                ERRORKIND.ERROR,
                opt[2], opt[0]);
            return false;
        }

        //------------------------------------------------------------
        // COptionManager.ParseResourceOption
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        /// <param name="resOptInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ParseResourceOption(
            string[] opt,
            CInputSet inputSet,
            bool embed)
        {
            List<string> strList = new List<string>();
            ParseOptionToStringList(opt, ref strList, ",", false);

            if (strList.Count == 0)
            {
                this.Controller.ReportError(
                    CSCERRID.ERR_MissingOptionArg,
                    ERRORKIND.ERROR,
                    opt[2]);
                return false;
            }

            FileInfo fileInfo = null;
            Exception excp = null;

            if (!IOUtil.CreateFileInfo(
                    strList[0],
                    out fileInfo,
                    out excp))
            {
                this.Controller.ReportError(ERRORKIND.ERROR, excp);
                return false;
            }

            string logicalName = null;
            bool isPublic = true;

            if (strList.Count >= 2)
            {
                logicalName = strList[1];
                if (String.IsNullOrEmpty(logicalName))
                {
                    logicalName = null;
                }
            }
            if (String.IsNullOrEmpty(logicalName))
            {
                logicalName = strList[0];
            }

            if (strList.Count >= 3)
            {
                if (String.Compare(strList[2], "private", true) == 0)
                {
                    isPublic = false;
                }
                else if (String.Compare(strList[2], "public", true) == 0)
                {
                    isPublic = true;
                }
                else
                {
                    this.Controller.ReportError(
                        CSCERRID.ERR_BadResourceVis,
                        ERRORKIND.ERROR,
                        opt[0]);
                    return false;
                }
            }

            if (strList.Count >= 4)
            {
                // too many arguments.
                return false;
            }

            inputSet.AddResourceFile(fileInfo, logicalName, embed, isPublic);
            return true;
        }

        //------------------------------------------------------------
        // COptionManager.ProcessPreSwitches
        //
        /// <summary>
        /// <para>Process the "pre-switches":
        /// <list type="bullet">
        /// <item>/? or /help</item>
        /// <item>/nologo</item>
        /// <item>/codepage</item>
        /// <item>/bugreport</item>
        /// <item>/utf8output</item>
        /// <item>/fullpaths</item>
        /// </list>
        /// If they exist, they are processed and nulled out.</para>
        /// </summary>
        /// <remarks>
        /// In sscli, /time and /repro are also processed.
        /// </remarks>
        //------------------------------------------------------------
        internal void ProcessPreSwitches()
        {
            if (this.OptionList == null)
            {
                return;
            }

            this.NoLogo = false;
            this.ShowHelp = false;
            //timeCompile = false;
            this.Encoding = System.Text.Encoding.Default;

            for (int i = 0; i < this.OptionList.Count;++i )
            {
                string[] opt = this.OptionList[i];
                if (opt == null || opt.Length == 0)
                {
                    this.OptionList[i] = null;
                    continue;
                }
                OptionInfoID infoID = GetOptionInfoID(opt);

                switch (infoID)
                {
                    //------------------------------------------------
                    // (-1) WrongUse
                    //------------------------------------------------
                    case OptionInfoID.WrongUse:
                        this.Controller.ReportError(
                            ALERRID.ERR_BadOptionValue,
                            ERRORKIND.WARNING,
                            opt[2], opt[0]);
                        break;

                    //------------------------------------------------
                    // (0) Invalid
                    //------------------------------------------------
                    case OptionInfoID.Invalid:
                        this.Controller.ReportError(
                            CSCERRID.WRN_UnknownOption,
                            ERRORKIND.WARNING,
                            opt[0]);
                        break;

                    //------------------------------------------------
                    // (1) /?, /help
                    //------------------------------------------------
                    case OptionInfoID.Help:
                        if (opt.Length == 3)
                        {
                            this.ShowHelp = true;
                        }
                        else
                        {
                            this.Controller.ReportError(
                                CSCERRID.ERR_BadSwitch,
                                ERRORKIND.ERROR,
                                opt[0]);
                        }
                        break;

                    //------------------------------------------------
                    // (2) /nologo
                    //------------------------------------------------
                    case OptionInfoID.NoLogo:
                        if (opt.Length == 3)
                        {
                            this.NoLogo = true;
                        }
                        else
                        {
                            this.Controller.ReportError(
                                CSCERRID.ERR_BadSwitch,
                                ERRORKIND.ERROR,
                                opt[0]);
                        }
                        break;

                    //------------------------------------------------
                    // (3) /codepage:<id>
                    //------------------------------------------------
                    case OptionInfoID.Codepage:
                        string value;
                        if (opt.Length > 3)
                        {
                            value = opt[3];
                            if (!String.IsNullOrEmpty(value))
                            {
                                IOUtil.RemoveQuotes(ref value);
                            }
                        }
                        else
                        {
                            value = null;
                        }

                        // コードページが指定されていない場合
                        if (String.IsNullOrEmpty(value))
                        {
                            this.Controller.ReportError(
                                CSCERRID.ERR_SwitchNeedsString,
                                ERRORKIND.ERROR,
                                opt[0]);
                            continue;
                        }

                        int codePage;
                        try
                        {
                            if (Int32.TryParse(value, out codePage))
                            {
                                this.Encoding = System.Text.Encoding.GetEncoding(codePage);
                            }
                            else
                            {
                                this.Encoding = System.Text.Encoding.GetEncoding(value);
                            }
                        }
                        catch (ArgumentException)
                        {
                            this.Controller.ReportError(
                                CSCERRID.FTL_BadCodepage,
                                ERRORKIND.ERROR,
                                opt[0]);
                            this.Encoding = Encoding.Default;
                        }
                        catch (NotSupportedException)
                        {
                            this.Controller.ReportError(
                                CSCERRID.FTL_BadCodepage,
                                ERRORKIND.ERROR,
                                opt[0]);
                            this.Encoding = Encoding.Default;
                        }
                        break;

                    //------------------------------------------------
                    // (4) /bugreport:<file>
                    //------------------------------------------------
                    case OptionInfoID.BugReport:
                        ParseOptionToFile(opt, ref this.BugReportFileInfo);
                        break;

                    //------------------------------------------------
                    // (5) /utf8output
                    //------------------------------------------------
                    case OptionInfoID.UTF8Output:
                        bool isUTF8 = false;
                        if (ParseBooleanOption(opt, ref isUTF8))
                        {
                            DebugUtil.Assert(
                                this.Controller != null &&
                                this.Controller.ConsoleOutput != null);
                            this.Controller.ConsoleOutput.EnableUTF8Output(isUTF8);
                        }
                        break;

                    //------------------------------------------------
                    // (6) /fullpaths
                    //------------------------------------------------
                    case OptionInfoID.FullPaths:
                        ParseBooleanOption(opt, ref this.FullPaths);
                        break;

                    //------------------------------------------------
                    // default
                    //------------------------------------------------
                    default:
                        continue;
                }

                this.OptionList[i] = null;
            }
        }

        //------------------------------------------------------------
        // COptionManager.ProcessOptions
        //
        /// <summary></summary>
        /// <param name="command"></param>
        //------------------------------------------------------------
        internal void ProcessOptions(CommandID command)
        {
            CInputSet currentInputSet
                = this.Controller.CreateNewInputSet(this.DefaultTarget);

            string strValue = null;
            int intValue = 0;
            bool bValue = false;
            FileInfo fileInfo = null;
            string fileName = null;
            List<string> strList = null;
            List<int> intList = null;
            bool madeAssembly = true;
            bool filesAdded = false;
            bool modulesAdded = false;
            bool resourcesAdded = false;
            Exception excp = null;

            for (int i = 0; i < this.OptionList.Count;++i )
            {
                string[] opt = this.OptionList[i];
                if (opt == null || opt.Length == 0)
                {
                    continue;
                }
                OptionInfoID infoID = GetOptionInfoID(opt);

                switch (infoID)
                {
                    //------------------------------------------------
                    // (-1) WrongUse
                    //------------------------------------------------
                    case OptionInfoID.WrongUse:
                        this.Controller.ReportError(
                            ALERRID.ERR_BadOptionValue,
                            ERRORKIND.WARNING,
                            opt[2], opt[0]);
                        break;

                    //------------------------------------------------
                    // (0) Invalid
                    //------------------------------------------------
                    case OptionInfoID.Invalid:
                        this.Controller.ReportError(
                            CSCERRID.WRN_UnknownOption,
                            ERRORKIND.WARNING,
                            opt[0]);
                        break;

                    //------------------------------------------------
                    // (1) @<responsefile>
                    //------------------------------------------------
                    case OptionInfoID.ResponseFile:
                        // processed by ProcessPreSwitches method.
                        break;

                    //------------------------------------------------
                    // (2) /?, /help
                    //------------------------------------------------
                    case OptionInfoID.Help:
                        // processed by ProcessPreSwitches method.
                        break;

                    //------------------------------------------------
                    // (3) /additionalfile
                    //------------------------------------------------
                    case OptionInfoID.AdditionalFile:
                        this.Controller.ShowMessage(
                            ResNo.CSCSTR_OPT_NOTIMPLEMENTED,
                            opt[2]);
                        break;

                    //------------------------------------------------
                    // (4) /addmodule:file[,file2]
                    //------------------------------------------------
                    case OptionInfoID.AddModule:
                        if (opt.Length >= 4)
                        {
                            List<string> modules = new List<string>();
                            for (int k = 3; k < opt.Length; ++k)
                            {
                                modules.AddRange(Util.SplitString(opt[k], ",;", true));
                            }
                            if (modules.Count > 0)
                            {
                                if (this.ModuleList == null)
                                {
                                    this.ModuleList = new List<string>();
                                }
                                this.ModuleList.AddRange(modules);
                                modulesAdded = true;
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    CSCERRID.ERR_MissingOptionArg,
                                    ERRORKIND.ERROR,
                                    opt[2]);
                            }
                        }
                        else
                        {
                            this.Controller.ReportError(
                                CSCERRID.ERR_MissingOptionArg,
                                ERRORKIND.ERROR,
                                opt[2]);
                        }
                        break;

                    //------------------------------------------------
                    // (5) /analyzer
                    //------------------------------------------------
                    case OptionInfoID.Analyzer:
                        this.Controller.ShowMessage(
                            ResNo.CSCSTR_OPT_NOTIMPLEMENTED,
                            opt[2]);
                        break;

                    //------------------------------------------------
                    // (6) /appconfig:<file>
                    //------------------------------------------------
                    case OptionInfoID.AppConfig:
                        ParseOptionToFile(opt, ref this.AppConfigFileInfo);
                        break;

                    //------------------------------------------------
                    // (7) /baseaddress:<address>
                    //------------------------------------------------
                    case OptionInfoID.BaseAddress:
                        ulong baseAddr = 0;
                        if (ParseOptionToUInt64(opt, ref baseAddr))
                        {
                            currentInputSet.SetImageBase(baseAddr);
                        }
                        break;

                    //------------------------------------------------
                    // (8) /bugreport:<file>
                    //------------------------------------------------
                    case OptionInfoID.BugReport:
                        // processed by ProcessPreSwitches method.
                        break;

                    //------------------------------------------------
                    // (9) checked[+|-]
                    //------------------------------------------------
                    case OptionInfoID.Checked:
                        ParseBooleanOption(opt, ref this.CheckOverflow);
                        break;

                    //------------------------------------------------
                    // (10) /checksumalgorithm:<algrithmID>
                    //------------------------------------------------
                    case OptionInfoID.CheckSumAlgorithm:
                        this.Controller.ShowMessage(
                            ResNo.CSCSTR_OPT_NOTIMPLEMENTED,
                            opt[2]);
                        break;

                    //------------------------------------------------
                    // (11) /codepage:<id>
                    //------------------------------------------------
                    case OptionInfoID.Codepage:
                        // processed by ProcessPreSwitches method.
                        break;

                    //------------------------------------------------
                    // (12) /debug[+|-]
                    //------------------------------------------------
                    case OptionInfoID.Debug:
                        ParseBooleanOption(opt, ref this.GenerateDebugInfo);
                        break;

                    //------------------------------------------------
                    // (13) /debug:{full|pdbonly}
                    //------------------------------------------------
                    case OptionInfoID.DebugType:
                        strValue = null;
                        if (ParseOptionToOneString(opt, ref strValue))
                        {
                            if (String.Compare(strValue, "pdbonly", true) == 0)
                            {
                                this.IsDebugInfoPDBOnly = true;
                            }
                            else if (String.Compare(strValue, "full", true) == 0)
                            {
                                this.IsDebugInfoPDBOnly = false;
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    CSCERRID.ERR_BadDebugType,
                                    ERRORKIND.ERROR,
                                    opt[0]);
                            }
                        }
                        else
                        {
                            // processed in ParseOptionToOneString.
                        }
                        break;

                    //------------------------------------------------
                    // (14) /define:<string>[,<string1>]
                    //------------------------------------------------
                    case OptionInfoID.DefineCCSymbols:
                        ParseOptionToStringList(
                            opt,
                            ref this.DefinedCCSymbolList,
                            ",",
                            true);
                        break;

                    //------------------------------------------------
                    // (15) /delaysign[+|-]
                    //------------------------------------------------
                    case OptionInfoID.DelaySign:
                        if (ParseBooleanOption(opt, ref bValue))
                        {
                            this.DelaySign = bValue;
                        }
                        break;

                    //------------------------------------------------
                    // (16) /doc:<file>
                    //------------------------------------------------
                    case OptionInfoID.XMLDocFile:
                        ParseOptionToFile(opt, ref this.XMLDocFileInfo);
                        break;

                    //------------------------------------------------
                    // (17) /errorreport:{none|prompt|queue|send}
                    //------------------------------------------------
                    case OptionInfoID.ErrorReport:
                        strValue = null;
                        if (ParseOptionToOneString(opt, ref strValue))
                        {
                            if (String.Compare(strValue, "none", true) == 0)
                            {
                                this.ErrorReport = ErrorReportEnum.None;
                            }
                            else if (String.Compare(strValue, "prompt", true) == 0)
                            {
                                this.ErrorReport = ErrorReportEnum.Prompt;
                            }
                            else if (String.Compare(strValue, "queue", true) == 0)
                            {
                                this.ErrorReport = ErrorReportEnum.Queue;
                            }
                            else if (String.Compare(strValue, "send", true) == 0)
                            {
                                this.ErrorReport = ErrorReportEnum.Send;
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    CSCERRID.ERR_BadWatsonMode,
                                    ERRORKIND.ERROR,
                                    opt[0]);
                            }
                        }
                        else
                        {
                            // processed in ParseOptionToOneString.
                        }
                        break;

                    //------------------------------------------------
                    // (18) /filealign:<size>
                    //------------------------------------------------
                    case OptionInfoID.FileAlign:
                        uint uiSize = 0;
                        if (ParseOptionToUInt32(opt, ref uiSize))
                        {
                            if (uiSize == 512 ||
                                uiSize == 1024 ||
                                uiSize == 2048 ||
                                uiSize == 4096 ||
                                uiSize == 8192)
                            {
                                //this.SectionSize = uiSize;
                                currentInputSet.SetFileAlignment(uiSize);
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    ALERRID.ERR_BadOptionValue,
                                    ERRORKIND.ERROR,
                                    opt[2], opt[0]);
                            }
                        }
                        else
                        {
                            // processed in ParseOptionToInt32.
                        }
                        break;

                    //------------------------------------------------
                    // (19) /fullpaths
                    //------------------------------------------------
                    case OptionInfoID.FullPaths:
                        // processed by ProcessPreSwitches method.
                        break;

                    //------------------------------------------------
                    // (20) /highentropyva[+|-]
                    //------------------------------------------------
                    case OptionInfoID.HighEntropyVa:
                        ParseBooleanOption(opt, ref this.HighEntropyVa);
                        break;

                    //------------------------------------------------
                    // (21) /keycontainer:<name>
                    //------------------------------------------------
                    case OptionInfoID.KeyContainer:
                        ParseOptionToOneString(opt, ref this.KeyContainer);
                        break;

                    //------------------------------------------------
                    // (22) /keyfile:<file>
                    //------------------------------------------------
                    case OptionInfoID.KeyFile:
                        ParseOptionToFile(opt, ref this.KeyFileInfo);
                        break;

                    //------------------------------------------------
                    // (23) /langversion:<string>
                    //------------------------------------------------
                    case OptionInfoID.LangVersion:
                        strValue = null;
                        if (ParseOptionToOneString(opt, ref strValue))
                        {
                            if (String.Compare(strValue, "default", true) == 0)
                            {
                                this.LangVersion = LangVersionEnum.Default;
                            }
                            else if (String.Compare(strValue, "iso-1", true) == 0)
                            {
                                this.LangVersion = LangVersionEnum.ISO_1;
                            }
                            else if (String.Compare(strValue, "iso-2", true) == 0)
                            {
                                this.LangVersion = LangVersionEnum.ISO_2;
                            }
                            else if (String.Compare(strValue, "3", true) == 0)
                            {
                                this.LangVersion = LangVersionEnum.CS3;
                            }
                            else if (String.Compare(strValue, "ecma1", true) == 0)
                            {
                                this.LangVersion = LangVersionEnum.ECMA1;
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    ALERRID.ERR_BadOptionValue,
                                    ERRORKIND.ERROR,
                                    opt[2], opt[0]);
                            }
                        }
                        else
                        {
                            // processed in ParseOptionToOneString.
                        }
                        break;

                    //------------------------------------------------
                    // (24) /lib:<dir>[,<div2>]
                    //------------------------------------------------
                    case OptionInfoID.LibPath:
                        ParseOptionToStringList(
                            opt,
                            ref this.LibPathList,
                            ",",
                            true);
                        break;

                    //------------------------------------------------
                    // (25) /link:<file>[,<file2>]
                    //------------------------------------------------
                    case OptionInfoID.Link:
                        ParseOptionToStringList(
                            opt,
                            ref this.LinkList,
                            ",",
                            true);
                        break;

                    //------------------------------------------------
                    // (26) /linkresource:<file>[,<name>,[accessiblity]]
                    //------------------------------------------------
                    case OptionInfoID.LinkResource:
                        DebugUtil.Assert(currentInputSet != null);
                        if (ParseResourceOption(opt, currentInputSet, false))
                        {
                            resourcesAdded = true;
                        }
                        break;

                    //------------------------------------------------
                    // (27) /main:<class>
                    //------------------------------------------------
                    case OptionInfoID.Main:
                        strValue = null;
                        if (ParseOptionToOneString(opt, ref strValue))
                        {
                            currentInputSet.SetMainClass(strValue);
                        }
                        break;

                    //------------------------------------------------
                    // (28) /moduleassemblyname:<name>
                    //------------------------------------------------
                    case OptionInfoID.ModuleAssemblyName:
                        ParseOptionToOneString(opt, ref this.ModuleAssemblyName);
                        break;

                    //------------------------------------------------
                    // (29) /modulename:<name>
                    //------------------------------------------------
                    case OptionInfoID.ModuleName:
                        ParseOptionToOneString(opt, ref this.SourceModuleName);
                        break;

                    //------------------------------------------------
                    // (30) /noconfig
                    //------------------------------------------------
                    case OptionInfoID.NoConfig:
                        // processed by ExamineNoConfigOption method.
                        break;

                    //------------------------------------------------
                    // (31) /nologo
                    //------------------------------------------------
                    case OptionInfoID.NoLogo:
                        // processed by ProcessPreSwitches method.
                        break;

                    //------------------------------------------------
                    // (32) /nostdlib[+|-]
                    //------------------------------------------------
                    case OptionInfoID.NoStdLib:
                        ParseBooleanOption(opt, ref this.NoStdLib);
                        break;

                    //------------------------------------------------
                    // (33) /nowarn:<num>[,<num2>]
                    //------------------------------------------------
                    case OptionInfoID.NoWarnList:
                        ParseOptionToIntergerList(opt, ref this.NoWarnList, ",");
                        this.NoWarnList.Sort();
                        break;

                    //------------------------------------------------
                    // (34) /nowin32manifest
                    //------------------------------------------------
                    case OptionInfoID.NoWin32Manifest:
                        ParseBooleanOption(opt, ref this.NoWin32Manifest);
                        break;

                    //------------------------------------------------
                    // (35) /optimize[+|-]
                    //------------------------------------------------
                    case OptionInfoID.Optimize:
                        ParseBooleanOption(opt, ref this.Optimize);
                        break;

                    //------------------------------------------------
                    // (36) /out:<file>
                    //------------------------------------------------
                    case OptionInfoID.OutputFile:
                        fileInfo = null;
                        if (ParseOptionToFile(opt, ref fileInfo))
                        {
                            this.Controller.BeginNewInputSet(
                                ref currentInputSet,
                                ref filesAdded);
                            currentInputSet.SetOutputFile(fileInfo);
                        }
                        break;

                    //------------------------------------------------
                    // (37) /parallel[+|-]
                    //------------------------------------------------
                    case OptionInfoID.Parallel:
                        ParseBooleanOption(opt, ref this.ConcurrentBuild);
                        break;

                    //------------------------------------------------
                    // (38) /pdb:<file>
                    //------------------------------------------------
                    case OptionInfoID.PDBFile:
                        fileInfo = null;
                        if (ParseOptionToFile(opt, ref fileInfo))
                        {
                            currentInputSet.SetPDBFile(fileInfo);
                        }
                        break;

                    //------------------------------------------------
                    // (39) /platform:<string>
                    //------------------------------------------------
                    case OptionInfoID.Platform:
                        strValue = null;
                        if (ParseOptionToOneString(opt, ref strValue))
                        {
                            if (String.Compare(strValue, "anycpu", true) == 0)
                            {
                                this.Platform = PlatformEnum.AnyCPU;
                            }
                            else if (String.Compare(strValue, "anycpu32bitpreferred", true) == 0)
                            {
                                this.Platform = PlatformEnum.AnyCPU32BitPreferred;
                            }
#if false
                            else if (String.Compare(strValue, "ARM", true) == 0)
                            {
                                this.Platform = PlatformEnum.ARM;
                            }
#endif
                            else if (String.Compare(strValue, "x64", true) == 0)
                            {
                                this.Platform = PlatformEnum.x64;
                            }
                            else if (String.Compare(strValue, "x86", true) == 0)
                            {
                                this.Platform = PlatformEnum.x86;
                            }
                            else if (String.Compare(strValue, "itanium", true) == 0)
                            {
                                this.Platform = PlatformEnum.Itanium;
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    CSCERRID.ERR_BadPlatformType,
                                    ERRORKIND.ERROR,
                                    opt[0]);
                            }
                        }
                        else
                        {
                            // processed in ParseOptionToOneString.
                        }
                        break;

                    //------------------------------------------------
                    // (40) /preferreduilang:<string>
                    //------------------------------------------------
                    case OptionInfoID.PreferredUILang:
                        ParseOptionToOneString(opt, ref this.PreferredUILangName);
                        break;

                    //------------------------------------------------
                    // (41) /recurse:[<dir>\]<file>
                    //------------------------------------------------
                    case OptionInfoID.Recurse:
                        fileName = opt[3];
                        IOUtil.RemoveQuotes(ref fileName);
                        this.Controller.AddInputFiles(
                            fileName,
                            currentInputSet,
                            true,
                            ref filesAdded);
                        break;

                    //------------------------------------------------
                    // (42) /reference:[<alias>=]<file>
                    //------------------------------------------------
                    case OptionInfoID.Reference:
                        if (this.ImportList == null)
                        {
                            this.ImportList = new List<string[]>();
                        }

                        strList = null;
                        if (!ParseOptionToStringList(opt, ref strList, "=", false))
                        {
                            break;
                        }
                        string[] refarr = { null, null };

                        if (strList == null || strList.Count == 0)
                        {
                            this.Controller.ReportError(
                                CSCERRID.ERR_MissingOptionArg,
                                ERRORKIND.ERROR,
                                opt[0]);
                            break;
                        }
                        else if (strList.Count == 1)
                        {
                            fileName = strList[0];
                            if (!String.IsNullOrEmpty(fileName))
                            {
                                refarr[1] = fileName;
                                this.ImportList.Add(refarr);
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    CSCERRID.ERR_MissingOptionArg,
                                    ERRORKIND.ERROR,
                                    opt[0]);
                                break;
                            }
                        }
                        else if (strList.Count == 2)
                        {
                            if (!String.IsNullOrEmpty(strList[0]))
                            {
                                refarr[0] = strList[0];
                            }
                            if (!String.IsNullOrEmpty(strList[1]))
                            {
                                refarr[1] = strList[1];
                            }

                            if (refarr[1] != null)
                            {
                                this.ImportList.Add(refarr);
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    CSCERRID.ERR_AliasMissingFile,
                                    ERRORKIND.ERROR,
                                    refarr[0]);
                            }
                        }
                        else // strList.Count > 2
                        {
                            // error message.
                            this.Controller.ReportError(
                                ALERRID.ERR_BadOptionValue,
                                ERRORKIND.ERROR,
                                opt[2], opt[0]);
                            break;
                        }
                        break;

                    //------------------------------------------------
                    // (43) /resource:<file>[,<name>[,<accesiblity>]]
                    //------------------------------------------------
                    case OptionInfoID.Resource:
                        DebugUtil.Assert(currentInputSet != null);
                        if (ParseResourceOption(opt, currentInputSet, true))
                        {
                            resourcesAdded = true;
                        }
                        break;

                    //------------------------------------------------
                    // (44) /ruleset:<file>
                    //------------------------------------------------
                    case OptionInfoID.RuleSet:
                        ParseOptionToFile(opt, ref this.RuleSetFileInfo);
                        break;

                    //------------------------------------------------
                    // (45) /subsystemversion:<major>.<minor>
                    //------------------------------------------------
                    case OptionInfoID.SubSystemVersion:
                        ParseOptionToVersion(opt, ref this.SubSystemVersion);
                        break;

                    //------------------------------------------------
                    // (46) /target:<target>
                    //------------------------------------------------
                    case OptionInfoID.Target:
                        strValue = null;
                        TargetType ttype = TargetType.None;

                        if (ParseOptionToOneString(opt, ref strValue))
                        {
                            if (String.Compare(strValue, "exe", true) == 0)
                            {
                                ttype = TargetType.Exe;
                                //currentInputSet.SetTargetType(TargetType.Exe);
                            }
                            else if (
                                String.Compare(strValue, "lib", true) == 0 ||
                                String.Compare(strValue, "library", true) == 0)
                            {
                                ttype = TargetType.Library;
                                //currentInputSet.SetTargetType(TargetType.Library);
                            }
                            else if (
                                String.Compare(strValue, "win", true) == 0 ||
                                String.Compare(strValue, "winexe", true) == 0)
                            {
                                ttype = TargetType.WinExe;
                                //currentInputSet.SetTargetType(TargetType.WinExe);
                            }
                            else if (command == CommandID.CSC)
                            {
                                if (String.Compare(strValue, "appcontainerexe", true) == 0)
                                {
                                    ttype = TargetType.AppContainerExe;
                                    //currentInputSet.SetTargetType(TargetType.AppContainerExe);
                                }
                                else if (String.Compare(strValue, "module", true) == 0)
                                {
                                    ttype = TargetType.Module;
                                    //currentInputSet.SetTargetType(TargetType.Module);
                                    if (Controller.InputSetCount == 1)
                                    {
                                        madeAssembly = false;
                                    }
                                }
                                else if (String.Compare(strValue, "winmdobj", true) == 0)
                                {
                                    ttype = TargetType.WinMdObj;
                                    //currentInputSet.SetTargetType(TargetType.WinMdObj);
                                    if (Controller.InputSetCount == 1)
                                    {
                                        madeAssembly = false;
                                    }
                                }
                                else
                                {
                                    this.Controller.ReportError(
                                        CSCERRID.FTL_InvalidTarget,
                                        ERRORKIND.FATAL);
                                }
                            }
                            else
                            {
                                this.Controller.ReportError(
                                    CSCERRID.FTL_InvalidTarget,
                                    ERRORKIND.FATAL);
                            }
                        }
                        if (ttype != TargetType.None)
                        {
                            this.Controller.BeginNewInputSet(
                                ref currentInputSet,
                                ref filesAdded);
                            currentInputSet.SetTargetType(ttype);
                        }
                        break;

                    //------------------------------------------------
                    // (47) /unsafe
                    //------------------------------------------------
                    case OptionInfoID.Unsafe:
                        ParseBooleanOption(opt, ref this.Unsafe);
                        break;

                    //------------------------------------------------
                    // (48) /utf8output
                    //------------------------------------------------
                    case OptionInfoID.UTF8Output:
                        // processed by ProcessPreSwitches method.
                        break;

                    //------------------------------------------------
                    // (49) /warn:<level>
                    //------------------------------------------------
                    case OptionInfoID.WarningLevel:
                        intValue = 4;   // default level
                        if (ParseOptionToInt32(opt, ref intValue))
                        {
                            switch (intValue)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    this.WarningLevel = intValue;
                                    break;

                                default:
                                    this.Controller.ReportError(
                                        CSCERRID.ERR_BadWarningLevel,
                                        ERRORKIND.ERROR);
                                    break;
                            }
                        }
                        break;

                    //------------------------------------------------
                    // (50) /warnaserror[+|-]
                    //------------------------------------------------
                    case OptionInfoID.WarningsAsErrors:
                        bValue = false;
                        if (ParseBooleanOption(opt, ref bValue))
                        {
                            SetWarnAsErrorArray(bValue);
                        }
                        break;

                    //------------------------------------------------
                    // (51) /warnaserror:<number>[,<number2>]
                    //------------------------------------------------
                    case OptionInfoID.WarnAsErrorList:
                        intList = null;
                        if (ParseOptionToIntergerList(opt, ref intList, ","))
                        {
                            if (intList != null)
                            {
                                foreach (int no in intList)
                                {
                                    SetWarnAsError(no, true);
                                }
                            }
                        }
                        break;

                    //------------------------------------------------
                    // (52) /win32icon:<file>
                    //------------------------------------------------
                    case OptionInfoID.Win32Icon:
                        fileInfo = null;
                        if (ParseOptionToFile(opt, ref fileInfo))
                        {
                            currentInputSet.SetWin32Icon(fileInfo);
                        }
                        break;

                    //------------------------------------------------
                    // (53) /win32manifest:<file>
                    //------------------------------------------------
                    case OptionInfoID.Win32Manifest:
                        ParseOptionToFile(opt, ref this.Win32ManifestFileInfo);
                        break;

                    //------------------------------------------------
                    // (54) /win32res:<file>
                    //------------------------------------------------
                    case OptionInfoID.Win32Resource:
                        fileInfo = null;
                        if (ParseOptionToFile(opt, ref fileInfo))
                        {
                            currentInputSet.SetWin32Resource(fileInfo);
                        }
                        break;

                    //------------------------------------------------
                    // (55) /algid:<string>
                    //------------------------------------------------
                    case OptionInfoID.ArgorithmID:
                        this.Controller.ShowMessage(
                            ResNo.CSCSTR_OPT_NOTIMPLEMENTED,
                            opt[2]);
                        break;

                    //------------------------------------------------
                    // (56) /comp[any]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Company:
                        ParseOptionToOneString(opt, ref this.Company);
                        break;

                    //------------------------------------------------
                    // (57) /config[uration]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Configuration:
                        ParseOptionToOneString(opt, ref this.Configuration);
                        break;

                    //------------------------------------------------
                    // (58) /copy[right]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Copyright:
                        ParseOptionToOneString(opt, ref this.Copyright);
                        break;

                    //------------------------------------------------
                    // (59) /c[ulture]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Culture:
                        strValue = null;
                        if (ParseOptionToOneString(opt, ref strValue) &&
                            strValue != null)
                        {
                            if (!Util.CreateCultureInfo(strValue, out this.Culture,out excp))
                            {
                                if (excp != null)
                                {
                                    this.Controller.ReportError(ERRORKIND.ERROR, excp);
                                }
                                else
                                {
                                    this.Controller.ReportInternalCompilerError(
                                        "COptionManager.ProcessOptions:/c[ulture]");
                                }
                            }
                        }
                        break;

                    //------------------------------------------------
                    // (60) /descr[iption]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Description:
                        ParseOptionToOneString(opt, ref this.Description);
                        break;

                    //------------------------------------------------
                    // (61) /e[vidence]:<file>
                    //------------------------------------------------
                    case OptionInfoID.Evidence:
                        ParseOptionToFile(opt, ref this.EvidenceFileInfo);
                        break;

                    //------------------------------------------------
                    // (62) /fileversion:<string>
                    //------------------------------------------------
                    case OptionInfoID.FileVersion:
                        ParseOptionToOneString(opt, ref this.AssemblyFileVersion);
                        break;

                    //------------------------------------------------
                    // (63) /flags:<int>
                    //------------------------------------------------
                    case OptionInfoID.Flags:
                        intValue = 0;
                        if (ParseOptionToInt32(opt, ref intValue))
                        {
                            this.AssemblyNameFlags = (AssemblyNameFlags)intValue;
                        }
                        break;

                    //------------------------------------------------
                    // (64) /prod[uct]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Product:
                        ParseOptionToOneString(opt, ref this.Product);
                        break;

                    //------------------------------------------------
                    // (65) productv[ersion]:<string>
                    //------------------------------------------------
                    case OptionInfoID.ProductVersion:
                        ParseOptionToOneString(opt, ref this.ProductVersion);
                        break;

                    //------------------------------------------------
                    // (66) /template:<file>
                    //------------------------------------------------
                    case OptionInfoID.Template:
                        ParseOptionToFile(opt, ref this.TemplateFileInfo);
                        break;

                    //------------------------------------------------
                    // (67) /title:<string>
                    //------------------------------------------------
                    case OptionInfoID.Title:
                        ParseOptionToOneString(opt, ref this.Title);
                        break;

                    //------------------------------------------------
                    // (68) /trade[mark]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Trademark:
                        ParseOptionToOneString(opt, ref this.Trademark);
                        break;

                    //------------------------------------------------
                    // (69) /v[ersion]:<string>
                    //------------------------------------------------
                    case OptionInfoID.Version:
                        ParseOptionToVersion(opt, ref this.Version);
                        break;

                    //------------------------------------------------
                    // Source
                    //------------------------------------------------
                    case OptionInfoID.Source:
                        DebugUtil.Assert(opt.Length >= 2);
                        fileName = opt[1];
                        IOUtil.RemoveQuotes(ref fileName);
                        this.Controller.AddInputFiles(
                            fileName,
                            currentInputSet,
                            false,
                            ref filesAdded);
                        break;

                    //------------------------------------------------
                    // default
                    //------------------------------------------------
                    default:
                        continue;
                }

                this.OptionList[i] = null;
            }

            //--------------------------------------------------------
            // Check
            //--------------------------------------------------------
            if ((!madeAssembly && !filesAdded) ||
                (madeAssembly &&
                !filesAdded &&
                (this.Controller.InputSetCount > 1 || (!resourcesAdded && !modulesAdded))))
            {
                if (this.Controller.InputSetCount <= 1)
                {
                    this.Controller.ReportError(
                        CSCERRID.ERR_NoSources,
                        ERRORKIND.FATAL);
                }
                else
                {
                    this.Controller.ReportError(
                        CSCERRID.ERR_NoSourcesInLastInputSet,
                        ERRORKIND.FATAL);
                }
            }
        }

        //------------------------------------------------------------
        // COptionManager.AddAssemblyAttribute
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        /// <param name="attrName"></param>
        /// <param name="posArgs"></param>
        //------------------------------------------------------------
        internal void AddAssemblyAttribute(
            COMPILER comp,
            string attrName,
            object[] posArgs)
        {
            if (comp == null || String.IsNullOrEmpty(attrName))
            {
                return;
            }

            GLOBALATTRSYM attrSym = comp.GlobalSymbolManager.CreateGlobalAttribute(
                "assembly",
                comp.MainSymbolManager.RootNamespaceSym.FirstDeclSym);
            DebugUtil.Assert(attrSym != null);

            attrSym.ElementKind = AttributeTargets.Assembly;
            attrSym.AttributeName = attrName;
            attrSym.PositionalArguments = posArgs;
            comp.AddAssemblyAttribute(attrSym);
            return;
        }

        //------------------------------------------------------------
        // COptionManager.AddAssemblyAttributes
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        internal void AddAssemblyAttributes(COMPILER comp)
        {
            // System.Reflection.AssemblyAlgorithmIdAttribute

            if (!String.IsNullOrEmpty(this.Company))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyCompanyAttribute",
                    new object[] { this.Company });
            }

            if (!String.IsNullOrEmpty(this.Configuration))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyConfigurationAttribute",
                    new object[] { this.Configuration });
            }

            if (!String.IsNullOrEmpty(this.Copyright))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyCopyrightAttribute",
                    new object[] { this.Copyright });
            }

            if (this.Culture != null)
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyCultureAttribute",
                    new object[] { this.Culture.Name });
            }

            if (!String.IsNullOrEmpty(this.Description))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyDescriptionAttribute",
                    new object[] { this.Description });
            }

            if (!String.IsNullOrEmpty(this.AssemblyFileVersion))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyFileVersionAttribute",
                    new object[] { this.AssemblyFileVersion });
            }

            if (this.AssemblyNameFlags.HasValue)
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyFlagsAttribute",
                    new object[] { this.AssemblyNameFlags.Value });
            }

            if (!String.IsNullOrEmpty(this.Product))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyProductAttribute",
                    new object[] { this.Product });
            }

            if (!String.IsNullOrEmpty(this.ProductVersion))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyInformationalVersionAttribute",
                    new object[] { this.ProductVersion });
            }

            if (!String.IsNullOrEmpty(this.Title))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyTitleAttribute",
                    new object[] { this.Title});
            }

            if (!String.IsNullOrEmpty(this.Trademark))
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyTrademarkAttribute",
                    new object[] { this.Trademark });
            }

            if (this.Version != null)
            {
                AddAssemblyAttribute(
                    comp,
                    "System.Reflection.AssemblyVersionAttribute",
                    new object[] { this.Version.ToString() });
            }
        }

        //------------------------------------------------------------
        // COptionManager.CopyFrom
        //
        /// <summary></summary>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal void CopyFrom(COptionManager src)
        {
            if (src == null)
            {
                return;
            }

            // CSC_ResponseFile
            this.ShowHelp = src.ShowHelp;
            // CSC_AdditionalFile
            this.ModuleList = new List<string>(src.ModuleList);
            // CSC_Analyzer
            this.AppConfigFileInfo = src.AppConfigFileInfo;
            //this.BaseAddress=src.BaseAddress;
            this.BugReportFileInfo = src.BugReportFileInfo;
            this.CheckOverflow = src.CheckOverflow;
            // CSC_CheckSumAlgorithm
            this.Encoding = src.Encoding;
            this.GenerateDebugInfo = src.GenerateDebugInfo;
            this.IsDebugInfoPDBOnly = src.IsDebugInfoPDBOnly;
            this.DefinedCCSymbolList = new List<string>(src.DefinedCCSymbolList);
            this.DelaySign = src.DelaySign;
            this.XMLDocFileInfo = src.XMLDocFileInfo;
            this.ErrorReport = src.ErrorReport;
            //this.SectionSize=src.SectionSize;
            this.FullPaths = src.FullPaths;
            this.HighEntropyVa = src.HighEntropyVa;
            //this.IncremantalBuild = src.IncremantalBuild;
            this.KeyContainer = src.KeyContainer;
            this.KeyFileInfo = src.KeyFileInfo;
            this.LangVersion = src.LangVersion;
            this.LibPathList = new List<string>(src.LibPathList);
            this.LinkList = new List<string>(src.LinkList);
            //this.LinkResourceList=;
            //this.EntryPoint=src.EntryPoint;
            this.ModuleAssemblyName = src.ModuleAssemblyName;
            this.SourceModuleName = src.SourceModuleName;
            this.NoConfig = src.NoConfig;
            this.NoLogo = src.NoLogo;
            this.NoStdLib = src.NoStdLib;
            this.NoWarnList = new List<int>(src.NoWarnList);
            this.NoWin32Manifest = src.NoWin32Manifest;
            this.Optimize = src.Optimize;
            //this.OutputFileInfo=src.OutputFileInfo;
            this.ConcurrentBuild = src.ConcurrentBuild;
            //this.PDBFileInfo=src.PDBFileInfo;
            this.Platform = src.Platform;
            this.PreferredUILangName = src.PreferredUILangName;
            this.Recurse = src.Recurse;
            this.ImportList = new List<string[]>(src.ImportList);
            //this.ResourceList=src.ResourceList;
            this.RuleSetFileInfo = src.RuleSetFileInfo;
            this.SubSystemVersion = src.SubSystemVersion;
            //this.Target=src.Target;
            this.Unsafe = src.Unsafe;
            //this.UTF8Output = src.UTF8Output;
            this.WarningLevel = src.WarningLevel;
            //this.WarningsAsErrors = src.WarningsAsErrors;
            this.WarnAsErrorArray = new bool[src.WarnAsErrorArray.Length];
            src.WarnAsErrorArray.CopyTo(this.WarnAsErrorArray, 0);
            //this.Win32IconFileInfo=src.Win32IconFileInfo;
            this.Win32ManifestFileInfo = src.Win32ManifestFileInfo;
            //this.Win32ResourceFileInfo=src.Win32ResourceFileInfo;

            // CSC_EmitDebugInfo
            // CSC_InternalTests
            // CSC_NoCodeGen
            // CSC_Timing
            // CSC_WatsonMode
            // CSC_PDBAltPath
            // CSC_SourceMap
            // CSC_CompilerSkeleton

            // AL_Resource, CSC_Resource
            // AL_LinkResource, CSC_LinkResource

            // AL_ArgorithmID
            // AL_BaseAddress -> CSC_BaseAddress
            // AL_BugReport -> CSC_BugReport
            this.Company = src.Company;
            this.Configuration = src.Configuration;
            this.Copyright = src.Copyright;
            this.Culture = src.Culture;
            // AL_DelaySign -> CSC_DelaySign
            this.Description = src.Description;
            this.EvidenceFileInfo = src.EvidenceFileInfo;
            this.AssemblyFileVersion = src.AssemblyFileVersion;
            this.AssemblyNameFlags = src.AssemblyNameFlags;
            // AL_FullPaths -> CSC_FullPaths
            // AL_KeyFile -> CSC_KeyFile
            // AL_KeyContainer -> CSC_KeyContainer
            // AL_Main -> CSC_Main
            // AL_NoLogo -> CSC_NoLogo
            // AL_OutputFile -> CSC_OutputFile
            // AL_Platform -> CSC_Platform
            this.Product = src.Product;
            this.ProductVersion = src.ProductVersion;
            // AL_Target -> CSC_Target
            this.TemplateFileInfo = src.TemplateFileInfo;
            this.Title = src.Title;
            this.Trademark = src.Trademark;
            this.Version = src.Version;
            // AL_Win32Icon -> CSC_Win32Icon
            // AL_Win32Resource -> CSC_Win32Resource
            // AL_ResponseFile
            // AL_Help

            // AL_OS
            // AL_SatelliteVersion
        }
    }
}

