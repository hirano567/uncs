//============================================================================
// OptionIDs.cs
//
// 2015/10/25 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Resources;

namespace Uncs
{
    //======================================================================
    // enum CommandOptionID (OPTID_)(optAssem)
    //
    /// <summary>
    /// <para>In sscli, with prefix OPTID_.</para>
    /// <para>(Options\OptionID.cs)</para>
    /// </summary>
    //======================================================================
    internal enum CommandOptionID : int
    {
        None = 0,
        First = 1,

        // Options of csc.exe
        CSC_First= First,

        CSC_ResponseFile = CSC_First,
        CSC_Help,
        CSC_AdditionalFile,
        CSC_AddModule,          // OPTID_MODULES
        CSC_Analyzer,
        CSC_AppConfig,
        CSC_BaseAddress,
        CSC_BugReport,
        CSC_Checked,            // OPTID_CHECKED
        CSC_CheckSumAlgorithm,
        CSC_Codepage,
        CSC_Debug,
        CSC_DebugType,          // OPTID_DEBUGTYPE
        CSC_DefineCCSymbols,    // OPTID_CCSYMBOLS
        CSC_DelaySign,          // OPTID_DELAYSIGN
        CSC_XMLDocFile,         // OPTID_XML_DOCFILE
        CSC_ErrorReport,
        CSC_FileAlign,
        CSC_FullPaths,
        CSC_HighEntropyVa,
        CSC_KeyContainer,       // OPTID_KEYNAME
        CSC_KeyFile,            // OPTID_KEYFILE
        CSC_LangVersion,        // OPTID_COMPATIBILITY
        CSC_LibPath,            // OPTID_LIBPATH
        CSC_Link,
        CSC_LinkResource,
        CSC_Main,
        CSC_ModuleAssemblyName, // OPTID_MODULEASSEMBLY
        CSC_ModuleName,
        CSC_NoConfig,
        CSC_NoLogo,
        CSC_NoStdLib,           // OPTID_NOSTDLIB
        CSC_NoWarnList,         // OPTID_NOWARNLIST
        CSC_NoWin32Manifest,
        CSC_Optimize,           // OPTID_OPTIMIZATIONS
        CSC_OutputFile,
        CSC_Parallel,
        CSC_PDBFile,
        CSC_Platform,           // OPTID_PLATFORM
        CSC_PreferredUILang,
        CSC_Recurse,
        CSC_Reference,          // OPTID_IMPORTS
        CSC_Resource,
        CSC_RuleSet,
        CSC_SubSystemVersion,
        CSC_Target,
        CSC_Unsafe,             // OPTID_UNSAFE
        CSC_UTF8Output,
        CSC_WarningLevel,       // OPTID_WARNINGLEVEL
        CSC_WarningsAsErrors,   // OPTID_WARNINGSAREERRORS
        CSC_WarnAsErrorList,    // OPTID_WARNASERRORLIST
        CSC_Win32Icon,
        CSC_Win32Manifest,
        CSC_Win32Resource,

        CSC_Last = CSC_Win32Resource,

        // Options of al.exe.
        // Several options which are not conflict with the ones of csc.exe can be used by csc.exe.

        AL_First,

        AL_Resource = AL_First,
        AL_LinkResource,

        AL_ArgorithmID,         // optAssemAlgID
        AL_BaseAddress,
        AL_BugReport,
        AL_Company,             // optAssemCompany
        AL_Configuration,       // optAssemConfig
        AL_Copyright,           // optAssemCopyright
        AL_Culture,             // optAssemLocale
        AL_DelaySign,           // optAssemHalfSign
        AL_Description,         // optAssemDescription
        AL_Evidence,
        AL_FileVersion,         // optAssemFileVersion
        AL_Flags,               // optAssemFlags
        AL_FullPaths,
        AL_KeyFile,             // optAssemKeyFile
        AL_KeyContainer,        // optAssemKeyName
        AL_Main,
        AL_NoLogo,
        AL_OutputFile,
        AL_Platform,            // optAssemProcessor
        AL_Product,             // optAssemProduct
        AL_ProductVersion,      // optAssemProductVersion
        AL_Target,
        AL_Template,
        AL_Title,               // optAssemTitle
        AL_Trademark,           // optAssemTrademark
        AL_Version,             // optAssemVersion
        AL_Win32Icon,
        AL_Win32Resource,
        AL_ResponseFile,
        AL_Help,

        AL_Last = AL_Help,
        Last = AL_Last,

        // These options are obsolete. (csc.exe)

        CSC_IncrementalBuild,   // OPTID_INCBUILD

        // These IDs are defined after sscli. (csc.exe)
        // But no option corresponds to these IDs.

        CSC_EmitDebugInfo,      // OPTID_EMITDEBUGINFO
        CSC_InternalTests,      // OPTID_INTERNALTESTS
        CSC_NoCodeGen,          // OPTID_NOCODEGEN
        CSC_Timing,             // OPTID_TIMING
        CSC_WatsonMode,         // OPTID_WATSONMODE
        CSC_PDBAltPath,         // OPTID_PDBALTPATH
        CSC_SourceMap,          // OPTID_SOURCEMAP
        CSC_CompilerSkeleton,   // OPTID_CompileSkeleton

        // These options are obsolete. (al.exe)

        // These IDs are defined after sscli. (al.exe)
        // But no option corresponds to these IDs.

        AL_OS,                  // optAssemOS
        AL_SatelliteVersion,    // optAssemSatelliteVer
    }

    //======================================================================
    // enum ErrorReportEnum
    //
    /// <summary></summary>
    //======================================================================
    internal enum ErrorReportEnum : int
    {
        /// <summary>
        /// Not send reports of compiler errors to Microsoft.
        /// </summary>
        None = 0,

        /// <summary>
        /// Prompts to send a report.
        /// </summary>
        Prompt,

        /// <summary>
        /// Queues the error report.
        /// </summary>
        Queue,

        /// <summary>
        /// Automatically sends reports to Microsoft.
        /// </summary>
        Send,
    }

    //======================================================================
    // enum LangVersionEnum
    //
    /// <summary>
    /// <para>In place of CompatibilityMode of sscli.</para>
    /// </summary>
    //======================================================================
    internal enum LangVersionEnum : int
    {
        Default = 0,
        NONE = Default,

        ISO_1,
        ISO_2,
        CS3,

        ECMA1,
    }
    //----------------------------------------------------------------------
    // CompatibilityMode (sscli, prebuilt\idl\csiface.h)
    //----------------------------------------------------------------------
    //typedef 
    //enum _CompatibilityMode
    //    {   CompatibilityECMA1  = 0,
    //    CompatibilityNone   = 1
    //    }   CompatibilityMode;

    //======================================================================
    // enum TargetType (OUTPUT_)(tt)
    //
    /// <summary>
    /// <para>In sscli, OutputFileTypes which has prefix "OUTPUT_" (prebuilt\idl\csiface.h),
    /// and TargetType which has prefix "tt" (csharp\alink\exe\alinkexe.h).</para>
    /// <para>(Options\OptionID.cs)</para>
    /// </summary>
    //======================================================================
    internal enum TargetType : int
    {
        None = 0,

        /// <summary>
        /// <para>(sscli) OutputFileTypes::CONSOLE, TargetType::ttConsole</para>
        /// </summary>
        Exe,

        /// <summary>
        /// <para>(sscli) OutputFileTypes::LIBRARY, TargetType::ttDll</para>
        /// </summary>
        Library,

        /// <summary>
        /// <para>(sscli) OutputFileTypes::MODULE</para>
        /// </summary>
        Module,

        /// <summary>
        /// <para>(sscli) OutputFileTypes::WINDOWS, TargetType::ttWinApp</para>
        /// </summary>
        WinExe,

        AppContainerExe,

        WinMdObj,
    }

    //======================================================================
    // enum PlatformEnum    (platform)
    //
    /// <summary>
    /// <para>In sscli, with prefix platform. (prebuilt\idl\csiface.h)</para>
    /// </summary>
    //======================================================================
    internal enum PlatformEnum : int
    {
        AnyCPU = 0,
        AnyCPU32BitPreferred,
        ARM,
        x86,

        // certainly 64-bit.
        Itanium,
        x64,
    }
    //typedef 
    //enum _PlatformType
    //    {    platformAgnostic    = 0,
    //    platformX86    = platformAgnostic + 1,
    //    platformIA64    = platformX86 + 1,
    //    platformAMD64    = platformIA64 + 1,
    //    platformLast    = platformAMD64 + 1
    //    }     PlatformType;

    //======================================================================
    // CompilerOptionFlags  (COF_)
    //
    /// <summary>
    /// <para>In sscli, with prefix COF_. (prebuilt\idl\csiface.h)</para>
    /// </summary>
    //======================================================================
    [Flags]
    internal enum CompilerOptionFlags : int
    {
        BOOLEAN = 0x1,
        HIDDEN = 0x2,
        DEFAULTON = 0x4,
        HASDEFAULT = 0x4,
        WARNONOLDUSE = 0x8,
        GRP_OUTPUT = 0x1000,
        GRP_INPUT = 0x2000,
        GRP_RES = 0x3000,
        GRP_CODE = 0x4000,
        GRP_ERRORS = 0x5000,
        GRP_LANGUAGE = 0x6000,
        GRP_MISC = 0x7000,
        GRP_ADVANCED = 0x8000,
        GRP_MASK = 0xff000,
        ARG_NONE = 0,
        ARG_FILELIST = 0x100000,
        ARG_FILE = 0x200000,
        ARG_SYMLIST = 0x300000,
        ARG_WILDCARD = 0x400000,
        ARG_TYPE = 0x500000,
        ARG_RESINFO = 0x600000,
        ARG_WARNLIST = 0x700000,
        ARG_ADDR = 0x800000,
        ARG_NUMBER = 0x900000,
        ARG_DEBUGTYPE = 0xa00000,
        ARG_STRING = 0xb00000,
        ARG_ALIAS = 0xc00000,
        ARG_NOCOLON = 0x8000000,
        ARG_BOOLSTRING = 0x10000000,
        ARG_MASK = 0x7f00000
    }

#if false
    //======================================================================
    // enum CompatibilityMode   (Compatibility)
    //
    /// <summary>
    /// <para>In sscli, with prefix Compatibility.
    /// (prebuilt\idl\csiface.h)</para>
    /// </summary>
    //======================================================================
    internal enum CompatibilityMode : int
    {
        ECMA1 = 0,
        None = 1
    }
#endif
}
