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
// File: compiler.h
//
// Defined the main compiler class, which contains all the other
// sub-parts of the compiler.
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
// File: compiler.cpp
//
// Defined the main compiler class.
// ===========================================================================

//============================================================================
// Compiler.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
//#define PROP_TO_FIELD

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace Uncs
{
    // This NT exception code is used to propagate a fatal
    // exception within the compiler. It has the "customer" bit
    // set and is chosen to probably not collide with any other
    // code.

    // Structure for custom debug information:
    internal enum CDIVERSION : byte
    {
        DEFAULT = 4,
    }

    //======================================================================
    // enum CDIKind
    //======================================================================
    internal enum CDIKind : int
    {
        UsingInfo = 0,
        ForwardInfo = 1,
        ForwardToModuleInfo = 2,
        IteratorLocals = 3,
        ForwardIterator = 4,
    }

    //======================================================================
    // class CDIGlobalInfo
    //======================================================================
    internal class CDIGlobalInfo
    {
        internal CDIVERSION version;
        internal byte count;

        internal CDIGlobalInfo(byte count)
        {
            version = CDIVERSION.DEFAULT;
            this.count = count;
        }

        internal void CopyInto(CDIGlobalInfo dst)
        {
            dst.version = this.version;
            dst.count = this.count;
        }
    }

    //======================================================================
    // class CDIBaseInfo
    //======================================================================
    internal class CDIBaseInfo
    {
        internal CDIVERSION version;
        internal CDIKind kind;
        //uint size;

        internal CDIBaseInfo() { }

        internal CDIBaseInfo(
            //uint size,
             CDIKind kind)
        {
            version = CDIVERSION.DEFAULT;
            this.kind = kind;
            //this.size= size;
        }

        //Object operator new(uint sz, Object space)
        //{
        //    return space;
        //}
        //static uint Size() { return RoundUpAllocSizeTo(sizeof (CDIBaseInfo), 4); }

        internal void CopyInto(CDIBaseInfo dst)
        {
            dst.version = this.version;
            dst.kind = this.kind;
            //dst.size = this.size;
        }
    }

    //======================================================================
    // class CDIUsingBucket
    //======================================================================
    internal class CDIUsingBucket
    {
        internal int countOfUsing;
        //static size_t Size() { return sizeof (CDIUsingBucket); }
    }

    //======================================================================
    // class CDIIteratorLocalBucket
    //======================================================================
    internal class CDIIteratorLocalBucket
    {
        internal int ilOffsetStart;
        internal int ilOffsetEnd;
        //static size_t Size() { return sizeof (CDIIteratorLocalBucket); }
    }

    //======================================================================
    // class CDIIteratorLocalsInfo
    //======================================================================
    internal class CDIIteratorLocalsInfo : CDIBaseInfo
    {
        //internal int cBuckets;
        internal CDIIteratorLocalBucket[] Buckets = null;

        internal CDIIteratorLocalsInfo(int cLocals)
            //: CDIBaseInfo(ComputeSize(cLocals), CDIKindIteratorLocals)
            : base(CDIKind.IteratorLocals)
        {
            //cBuckets=cLocals;
            ClearOffsets();
        }

        internal void ClearOffsets()
        {
            Buckets = null;
        }

        //static DWORD ComputeSize(int cLocals) {
        //return (
        //  (DWORD)(RoundUpAllocSizeTo(((DWORD)(offsetof(CDIIteratorLocalsInfo, rgBuckets)  + ComputeSizeOfBuckets(cLocals))), 4)));
        //}

        //bool Verify() {
        //return ComputeSize(cBuckets) <= size;
        //}

        //private:
        //static DWORD ComputeSizeOfBuckets(int cLocals) {
        //return (DWORD) (CDIIteratorLocalBucket::Size() * cLocals);
        //}
    }

    //======================================================================
    // class CDIIteratorLocalsInfo
    //======================================================================
    internal class CDIForwardIteratorInfo : CDIBaseInfo
    {
    //    WCHAR szIteratorClassName[];
    //
    //    CDIForwardIteratorInfo (PCWSTR szIteratorClassName) : CDIBaseInfo(ComputeSize(szIteratorClassName), CDIKindForwardIterator) {
    //        size_t len = wcslen(szIteratorClassName);
    //        memcpy( this.szIteratorClassName, szIteratorClassName, len * sizeof(WCHAR));
    //        memset( this.szIteratorClassName + len, 0, size - (len * sizeof(WCHAR) + offsetof(CDIForwardIteratorInfo, szIteratorClassName)));
    //    }
    //
    //    static DWORD ComputeSize(PCWSTR szIteratorClassName) {
    //        return ((DWORD)(RoundUpAllocSize(((DWORD)(offsetof(CDIForwardIteratorInfo, szIteratorClassName) + 
    //            (wcslen(szIteratorClassName) + 1) * sizeof(WCHAR))))));
    //    }
    //
    //    bool Verify() {
    //        return ComputeSize(szIteratorClassName) <= size;
    //    }
    //
    }

    //struct CDIUsingInfo : public CDIBaseInfo {
    //    unsigned short countOfUsingNamespaces;
    //    CDIUsingBucket usingCounts[];
    //
    //    CDIUsingInfo (unsigned short ImpliedCount) : CDIBaseInfo(ComputeSize(ImpliedCount), CDIKindUsingInfo), countOfUsingNamespaces(ImpliedCount) {
    //        memset(usingCounts, 0, ComputeSizeOfBuckets(countOfUsingNamespaces));
    //    }
    //
    //    static DWORD ComputeSize(unsigned short countOfUsingNamespaces) {
    //        return RoundUpAllocSizeTo((DWORD)(offsetof(CDIUsingInfo, usingCounts) + ComputeSizeOfBuckets(countOfUsingNamespaces)), 4);
    //    }
    //
    //    bool Verify() {
    //        return ComputeSize(countOfUsingNamespaces) <= size;
    //    }
    //
    //private:
    //    static DWORD ComputeSizeOfBuckets(unsigned short countOfUsingNamespaces) {
    //        return (DWORD)(CDIUsingBucket::Size() * (countOfUsingNamespaces));
    //    }
    //};
    //
    //struct CDIForwardingInfo : public CDIBaseInfo{
    //    DWORD tokenToForwardTo;
    //    CDIForwardingInfo (DWORD token) : CDIBaseInfo(sizeof(CDIForwardingInfo), CDIKindForwardInfo), tokenToForwardTo(token) {}
    //    bool Verify() {
    //        return Size() <= size;
    //    }
    //    static size_t Size() { return RoundUpAllocSizeTo(sizeof (CDIForwardingInfo), 4); }
    //};
    //
    //struct CDIForwardToModuleInfo : public CDIBaseInfo {
    //    DWORD tokenOfModuleInfo;
    //    CDIForwardToModuleInfo () : CDIBaseInfo(sizeof(CDIForwardToModuleInfo), CDIKindForwardToModuleInfo), tokenOfModuleInfo(0) {}
    //    bool Verify() {
    //        return Size() <= size;
    //    }
    //    static size_t Size() { return RoundUpAllocSizeTo(sizeof (CDIForwardToModuleInfo), 4); }
    //};
    //
    //
    //#define MSCUSTOMDEBUGINFO (L"MD2")

    //======================================================================
    // interface BSYMHOST
    //
    /// <summary>
    /// <para>Implemented by COMPILER</para>
    /// </summary>
    //======================================================================
    //internal interface BSYMHOST
    //{
    //    CNameManager NameManager { get; }
    //    void ErrorLocArgs(ERRLOC loc, CSCERRID id, params ErrArg[] args);
    //}

    //======================================================================
    // interface LSYMHOST 
    //
    /// <summary>
    /// <para>Implemented by COMPILER.</para>
    /// </summary>
    //======================================================================
    //internal interface LSYMHOST
    //{
    //    BSYMMGR MainSymbolManager { get; }
    //}

    //======================================================================
    // enum CompilerPhaseEnum
    //
    /// <summary>
    /// The phases of compilation.
    /// </summary>
    //======================================================================
    internal enum CompilerPhaseEnum : int
    {
        None,
        Init,

        DeclareTypes,
        ImportTypes,
        InitPredefTypes,
        ResolveInheritance,
        DefineBounds,
        DefineMembers,

        // We shouldn't ever be in FUNCBREC before here.
        EvalConstants,

        Prepare,
        PostPrepare,

        CompileMembers,

        Lim
    }

    //======================================================================
    // class CCompileProgress
    //
    /// <summary>
    /// In sscli20_20060311, there is no class implementing ICSCompileProgress.
    /// So, we define class CCompileProgress with ReportProgress method,
    /// which shows arguments simply.
    /// </summary>
    //======================================================================
#if COMPILERHOST
    internal class CCompileProgress //: ICSCompileProgress (sscli20\prebuilt\idl\csiface.h)
    {
        CCompilerHost host;

        internal CCompileProgress(CCompilerHost host)
        {
            this.host = host;
        }

        /// <summary>
        /// <para>This method simply outputs arguments.</para>
        /// <para>Nonzero return value means a cancel, but always returns 0 now.</para>
        /// </summary>
        public int ReportProgress(string task, int left, int total)
        {
            host.Print(String.Format("{0}: {1}/{2}\n", task, left, total));
            return 0;
        }
    }
#else
    internal class CCompileProgress //: ICSCompileProgress (sscli20\prebuilt\idl\csiface.h)
    {
        CController controller;

        internal CCompileProgress(CController cntr)
        {
            this.controller= cntr;
        }

        /// <summary>
        /// <para>This method simply outputs arguments.</para>
        /// <para>Nonzero return value means a cancel, but always returns 0 now.</para>
        /// </summary>
        public int ReportProgress(string task, int left, int total)
        {
            controller.WriteLine(String.Format("{0}: {1}/{2}", task, left, total));
            return 0;
        }
    }
#endif

    //======================================================================
    // class COMPILER
    //
    /// <summary>
    /// <para>The main class that holds everything together.</para>
    /// <para>This has a reference of the name manager,
    /// an instance of the symbol manager which was created in this constructor.</para>
    /// </summary>
    //======================================================================
    internal class COMPILER // : BSYMHOST, LSYMHOST
    {
        // Has friend classes, so accessibilities of all members are internal.

        //------------------------------------------------------------
        // enum COMPILER.STAGE
        //
        /// <summary>
        /// <para>ID of stages of Compilation.</para>
        /// <para>Defined in class COMPILER (CSharp\SCComp\Compiler.cs)</para>
        /// </summary>
        //------------------------------------------------------------
        internal enum STAGE : int
        {
            BEGIN,
            PARSE,
            DECLARE,
            IMPORT,
            DEFINE,
            PREPARE,
            EMIT,
            INTERIORPARSE,
            COMPILE,
            BIND,
            SCAN,
            TRANSFORM,
            CODEGEN,
            EMITIL,
        }

        //============================================================
        // class COMPILER.AssemblyRefList
        //============================================================
        internal class AssemblyRefList
        {
            /// <summary>
            /// The stringized assembly ref
            /// </summary>
            internal string NameRef = null; // NAME * nameRef;

            /// <summary>
            /// <para>(sscli) ListModSrc:
            /// List of MODULESYMs where the ref came from SYMLIST listModSrc;</para>
            /// <para>List of SYMs. These syms are used to get INFILESYM.FriendAccessUsed field
            /// in the post compile processing.</para>
            /// </summary>
            internal List<SYM> SymList = new List<SYM>(); // SYMLIST * listModSrc;

            //AssemblyRefList next;

            /// <summary>
            /// If this is just a friend assembly,
            /// we change the error for circular refs which do not match output.
            /// </summary>
            internal bool IsFriendAssemblyRefOnly = false;  // fIsFriendAssemblyRefOnly;
        }

        //------------------------------------------------------------
        // COMPILER Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// This is our parent "controller". Set by constructor.
        /// </summary>
        internal CController Controller = null; // pController

        /// <summary>
        /// Compiler options, kindly provided by our controller
        /// Set to COptionData which Controller has.
        /// </summary>
        //internal COptionData Options = null;    // options
        internal COptionManager OptionManager = null;

        // Embedded components of the compiler.

        /// <summary>
        /// Record for compiling an individual function
        /// </summary>
        public FUNCBREC FuncBRec = null;    // funcBRec

        /// <summary>
        /// Record for declaring a namespace &amp; classes in it
        /// </summary>
        internal CLSDREC ClsDeclRec = null; // clsDeclRec

        // Record for generating il for a method
        internal ILGENREC IlGenRec = null;

        /// <summary>
        /// <para>Catches ALink OnError calls</para>
        /// <para>Implements IMetaDataError</para>
        /// </summary>
        //internal ALinkError ALinkError = null;  // alError
        internal CAsmLink ALinkErro = null;

#if PROP_TO_FIELD
        public CNameManager NameManager = null; // NAMEMGR *namemgr; getNamemgr()
#else
        /// <summary>
        /// Name table (NOTE:  This is a referenced pointer!)
        /// </summary>
        private CNameManager nameManger = null; // NAMEMGR *namemgr;

        /// <summary>
        /// (R) Name table (NOTE:  This is a referenced pointer!)
        /// </summary>
        public CNameManager NameManager
        {
            get { return nameManger; }  // getNamemgr()
        }
#endif

        // Allocator for global symbols & names.  Again, specific to this compiler instance.
        // NRHEAP privGlobalSymAlloc;

#if PROP_TO_FIELD
        /// <summary>
        /// (R) the local symbol manager
        /// </summary>
        internal LSYMMGR LocalSymbolManager = new LSYMMGR();    // privlsymmgr
#else
        protected LSYMMGR localSymbolManager = new LSYMMGR();    // privlsymmgr

        /// <summary>
        /// (R) the local symbol manager
        /// </summary>
        internal LSYMMGR LocalSymbolManager
        {
            get { return this.localSymbolManager; } // getLSymmgr()
        }
#endif

#if PROP_TO_FIELD
        /// <summary>
        /// Global Symbol manager (also local to compiler in cscomp scenario)
        /// </summary>
        internal SYMMGR GlobalSymbolManager = null; // privsymmgr, getCSymmgr()

        /// <summary>
        /// Global symbol manager (type of BSYMMGR), GlobalSymbolManager as BSYMMGR.
        /// </summary>
        internal BSYMMGR MainSymbolManager = null;  // getBSymmgr()
#else
        /// <summary>
        /// Global Symbol manager (also local to compiler in cscomp scenario)
        /// </summary>
        protected SYMMGR globalSymbolManager = null;    // privsymmgr

        /// <summary>
        /// (R) Global Symbol manager (also local to compiler in cscomp scenario)
        /// </summary>
        internal SYMMGR GlobalSymbolManager
        {
            get { return this.globalSymbolManager; }    // getCSymmgr()
        }

        /// <summary>
        /// (R) Global symbol manager (type of BSYMMGR), GlobalSymbolManager as BSYMMGR.
        /// </summary>
        public BSYMMGR MainSymbolManager
        {
            get { return (globalSymbolManager as BSYMMGR); }    // getBSymmgr()
        }
#endif

        /// <summary>
        /// The parent for extern aliases
        /// </summary>
        protected PARENTSYM privExternAliasParentSym = null;  // PARENTSYM *privExternAliasContainer;

        /// <summary>
        /// (R) SYM instance which is parent of  extern aliases.
        /// </summary>
        internal PARENTSYM ExternalAilasParentSym
        {
            get { return privExternAliasParentSym; }    // GetExternAliasContainer()
        }

        /// <summary>
        /// Keep track of all assembly references that we bind to the current output
        /// Then validate them when the compilation is finished and we can compare against
        /// the actual assembly def record
        /// </summary>
        internal List<AssemblyRefList> ArlRefsToOutput = new List<AssemblyRefList>();

        /// <summary>
        /// Metadata importer
        /// </summary>
        internal IMPORTER Importer = null;  // importer

        /// <summary>
        /// table of CC symbols （type of NAMETABLE）
        /// </summary>
        internal NAMETABLE CCSymbolTable = null;    // ccsymbols

        /// <summary>
        /// the file emitter.
        /// </summary>
        internal EMITTER Emitter = null;    // emitter

        /// <summary>
        /// The file currently being emitted
        /// </summary>
        internal OUTFILESYM CurrentOutfileSym = null;   // PEFile *curFile;

        /// <summary>
        /// The file with the Assembly Manifest
        /// </summary>
        internal OUTFILESYM AssemblyOutfileSym = null;  // PEFile assemFile;

        /// <summary>
        /// the attributes for the current assembly
        /// </summary>
        internal GLOBALATTRSYM AssemblyAttributes = null;   // *assemblyAttributes
        internal GLOBALATTRSYM LastAssemblyAttributes = null;

        /// <summary>
        /// Set to an Assembly or an AssemblyName instance when an Assembly instance is created.
        /// </summary>
        internal CAssemblyInitialAttributes AssemblyInitialAttributes = null;

        /// <summary>
        /// global attributes which have an unknown location
        /// </summary>
        internal GLOBALATTRSYM UnknownGlobalAttributes = null;  // *unknownGlobalAttributes
        internal GLOBALATTRSYM LastUnknownGlobalAttributes = null;

        /// <summary>
        /// The global assembly ID for the ALink interface
        /// </summary>
        internal uint AssemblyID;    // mdAssembly assemID;

        /// <summary>
        /// Number of input files
        /// </summary>
        internal int InputFileCount = 0;    // ULONG cInputFiles;

        /// <summary>
        /// Number of output files
        /// </summary>
        internal int OutputFileCount = 0;   // ULONG cOutputFiles;

        /// <summary>
        /// <para>IALink2 is implemented by CAsmLink only.</para>
        /// <para>(IALink2 is defined in csharp\alink\inc\alink.h)</para>
        /// <para>(CAsmLink is defined in csharp\alink\dll\asmlink.h)</para>
        /// </summary>
        internal CAsmLink Linker = null;    // IALink2 * linker; // Assembly linker

        //
        // CONSIDER: may want to remove this for release builds
        //

        /// <summary>
        /// <para>head of current location stack</para>
        /// <para>In sscli, used in COMPILER::ReportICE which is called in CompilerExceptionFilter.</para>
        /// <para>COMPILER::UndeclarableType,</para>
        /// </summary>
        internal LOCATION Location = null;  // location

        /// <summary>
        /// The integers in this array indicate rough proportional amount of time each phase takes.
        /// </summary>
        static int[] relativeTimeInPhase = 
        {
            12, // COMPILE_PHASE_ENUM.DECLARETYPES
            2,  // COMPILE_PHASE_ENUM.IMPORTTYPES
            1,  // COMPILE_PHASE_ENUM.DEFINE
            5,  // COMPILE_PHASE_ENUM.PREPARE
            1,  // COMPILE_PHASE_ENUM.CHECKIFACECHANGE
            40, // COMPILE_PHASE_ENUM.COMPILE
            15, // COMPILE_PHASE_ENUM.WRITEOUTPUT
            1,  // COMPILE_PHASE_ENUM.WRITEINCFILE
        };

        internal SYM StackOverflowLocation = null;  // stackOverflowLocation

        /// <summary>
        /// If true, the global CLS compliant attribute is set and we should enforce CLS compliance.
        /// If false, compile anything (don't care about CLS rules).
        /// </summary>
        private bool checkCLS = true;   // checkCLS

        /// <summary>
        /// (R) If true, the global CLS compliant attribute is set and we should enforce CLS compliance.
        /// If false, compile anything (don't care about CLS rules).
        /// </summary>
        internal bool CheckForCLS
        {
            get { return checkCLS; }
        }

        /// <summary>
        /// True if we are building an assembly (i.e. the first output file is NOT a module)
        /// </summary>
        private bool buildAssembly = false; // m_fAssemble

        /// <summary>
        /// (R) True if we are building an assembly (i.e. the first output file is NOT a module)
        /// </summary>
        internal bool BuildAssembly
        {
            get { return buildAssembly; }   // BuildAssembly()
        }

        /// <summary>
        /// True if source or any added modules contain a friend declaration.
        /// </summary>
        /// <remarks>
        /// FriendsDeclared(), SetFriendsDeclared()
        /// </remarks>
        internal bool IsFriendDeclared = false; // m_fFriendsDeclared

        /// <summary>
        /// True if no user CompilationRelaxationsAttribute was found
        /// </summary>
        private bool noRelaxationsAttribute = true; // m_fEmitRelaxations

        /// <summary>
        /// True if no user RuntimeCompatibilityAttribute was specified by the user
        /// </summary>
        private bool noRuntimeCompatibility = true; // m_fEmitRuntimeCompatibility

        /// <summary>
        /// True if non-exception exceptions are wrapped
        /// </summary>
        private bool wrapNonException = true;   // m_fWrapNonExceptionThrows

        // Compiler hosting data.

        // COM reference count
        //private int cRef = 0; // ULONG cRef;

        /// <summary>
        /// Have we been initialized (and how far did Init() succeed)?
        /// </summary>
        private bool isInited = false;  // isInited

        /// <summary>
        /// Has compile been canceled?
        /// </summary>
        private bool isCanceled = false;    // isCanceled

        internal bool IsCanceled
        {
            get { return this.isCanceled; } // IsCanceled()
        }

        // These evolve together

        /// <summary>
        /// The compilation phase.
        /// </summary>
        private CompilerPhaseEnum compilationPhase = CompilerPhaseEnum.None;    // compPhase

        /// <summary>
        /// (R) The compilation phase.
        /// </summary>
        internal CompilerPhaseEnum CompilationPhase
        {
            get { return compilationPhase; }
        }

        /// <summary>
        /// The max aggSym state that we can handle at this point.
        /// </summary>
        private AggStateEnum aggStateMax = AggStateEnum.None;    // aggStateMax

        /// <summary>
        /// (R) The max aggSym state that we can handle at this point.
        /// </summary>
        internal AggStateEnum AggStateMax
        {
            get { return aggStateMax; }  // AggStateMax()
        }

        private int sidLast = 0;    // sidLast

        // Error handling methods and data (error.cpp)

        ////int cWarn, cError;
        private int warningCount = 0;   // cWarn
        private int errorCount = 0;     // cError

        //// Buffer for accumulating error messages; cleared when error is reported.
        //// 2MB is Reservced and individual pages are committed as needed
        //#define ERROR_BUFFER_MAX_WCHARS (1024*1024)
        //#define ERROR_BUFFER_MAX_BYTES  (ERROR_BUFFER_MAX_WCHARS*sizeof(WCHAR))
        //friend LONG CompilerExceptionFilter(EXCEPTION_POINTERS* exceptionInfo, LPVOID pvData);

        internal StringBuilder ErrorBuffer = new StringBuilder();
        internal int ErrorBufferStart
        {
            get { return this.ErrorBuffer.Length; }
        }
        //internal int ErrorBufferStart = 0;
        //internal int errBufferNext = 0;

        // Miscellaneous.

        //internal Disp Dispenser = null;

        //CComPtr<ITypeNameFactory> m_qtypenamefactory;
        //CComPtr<ITypeNameBuilder> m_qtypenamebuilder;
        internal int m_cnttnbUsage = 0; // m_cnttnbUsage

        /// <summary>
        /// DirectoryInfo of the directory where mscorlib.dll is.
        /// </summary>
        private DirectoryInfo corSystemDirectoryInfo = null;

        /// <summary>
        /// (R) DirectoryInfo of the directory where mscorlib.dll is.
        /// </summary>
        internal DirectoryInfo CorSystemDirectoryInfo
        {
            get { return corSystemDirectoryInfo; }
        }

        /// <summary>
        /// (R) Name of the directory where mscorlib.dll is.
        /// </summary>
        internal string CorSystemDirectoryName
        {
            get { return (corSystemDirectoryInfo != null ? corSystemDirectoryInfo.FullName : null); }
        }

        /// <summary>
        /// List of the pathes of the directories where we search files.
        /// </summary>
        private List<DirectoryInfo> libPathList = new List<DirectoryInfo>();

        /// <summary>
        /// (R) List of the pathes of the directories where we search files.
        /// </summary>
        internal List<DirectoryInfo> LibPathList
        {
            get { return libPathList; }
        }

        /// <summary>
        /// Directory name where assembly and modules are saved when not specified.
        /// </summary>
        internal string DefaultOutputDirectory = null;

        // HMODULE hmodALink;
        // HMODULE hmodCorPE;

        //#define TEMPORARY_NAME_PREFIX L"CS$"
        //#define DELETED_NAME_PREFIX L"__Deleted$"

#if DEBUG
        internal bool HaveDefinedAnyType = false;   // haveDefinedAnyType

        private StringBuilder debugSb = new StringBuilder();
        private string debugStr = null;
#endif

        //------------------------------------------------------------
        // Methods
        //------------------------------------------------------------
        //internal NRHEAP & getGlobalSymAlloc() { return privGlobalSymAlloc; }

        // WARNING: compiler only function!!! Does not exist on EE
        //internal HINSTANCE getMessageDll() { return hModuleMessages; }

        //------------------------------------------------------------
        // COMPILER.ReportStackOverflow
        //
        /// <summary>
        /// This is where we put stuff for reporting a stack overflow
        /// </summary>
        //------------------------------------------------------------
        internal void ReportStackOverflow()
        {
            SYM sym = this.StackOverflowLocation;

            if (sym != null)
            {
                INFILESYM infile = sym.GetSomeInputFile();
                if (infile != null)
                {
                    Error(
                        new ERRLOC(infile, this.OptionManager.FullPaths),
                        CSCERRID.FTL_StackOverflow,
                        new ErrArg(sym));
                }
                else
                {
                    Error(new ERRLOC(), CSCERRID.FTL_StackOverflow, new ErrArg(sym));
                }
            }
            else
            {
                Error(new ERRLOC(), CSCERRID.FTL_StackOverflow, new ErrArg(""));
            }
        }

        // Create/destruction.
        //#ifdef _MSC_VER
        //#pragma push_macro("new")
        //#undef new
        //#endif // _MSC_VER
        //DECLARE_CLASS_NEW(size) { return VSAlloc(size); }
        //#ifdef _MSC_VER
        //#pragma pop_macro("new")
        //#endif // _MSC_VER
        //void operator delete(void * p) { VSFree(p); }

        //------------------------------------------------------------
        // COMPILER Constructor
        //
        /// <summary></summary>
        /// <param name="controller"></param>
        /// <param name="manager"></param>
        //------------------------------------------------------------
        internal COMPILER(CController controller, CNameManager manager)
        {
            this.Controller = controller;
#if PROP_TO_FIELD
            this.GlobalSymbolManager = new SYMMGR(this.Controller, this);
            this.MainSymbolManager = this.GlobalSymbolManager as BSYMMGR;
#else
            this.globalSymbolManager = new SYMMGR(this.Controller, this);
#endif
            this.CCSymbolTable = new NAMETABLE();

            //localSymAlloc = this;
            //globalHeap(this, true),
            //privGlobalSymAlloc(this),

            //pfnCreateCeeFileGen(0),
            //pfnDestroyCeeFileGen(0),
            //hmodALink(NULL),
            //hmodCorPE(NULL)

            //cError = cWarn = 0;

            this.OptionManager = this.Controller.OptionManager;

#if PROP_TO_FIELD
            NameManager = manager;
#else
            nameManger = manager;
#endif

            this.Importer = new IMPORTER(this);
            this.FuncBRec = new FUNCBREC(this);
            this.ClsDeclRec = new CLSDREC(this);
            this.IlGenRec = new ILGENREC(this);
            //this.ALinkError = new ALinkError(this);
            this.Emitter = new EMITTER(this);
        }

        //------------------------------------------------------------
        // COMPILER.Init
        //
        /// <summary>
        /// <para>Initialize the compiler.
        /// This does the heavy lifting of setting up the memory management,
        /// and so forth.</para>
        /// <list type="bullet">
        /// <item>Init Linker, Importer, MainSymbolManager.</item>
        /// <item>Set the search paths and the output directory.</item>
        /// <item>For each referenced assembly, create INFILESYM and
        /// add it to MainSymbolManager.MetadataFileRootSym</item>
        /// <item>For each added module, create INFILESYM and
        /// add it to MainSymbolManager.MetadataFileRootSym</item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Init()
        {
            if (this.isInited)
            {
                // This should never happen
                //VSFAIL ("Compiler initialization called twice?");
                return false;
            }

            bool br = true;
            try
            {
                br = InitWorker();
            }
            catch (Exception excp)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, excp);
                return false;
            }
            return br;
        }

        //------------------------------------------------------------
        // COMPILER.InitWorker
        //
        /// <summary>
        /// <list type="bullet">
        /// <item>Init Linker, Importer, MainSymbolManager.</item>
        /// <item>Set the search paths and the output directory.</item>
        /// <item>For each referenced assembly, create INFILESYM and
        /// add it to MainSymbolManager.MetadataFileRootSym</item>
        /// <item>For each added module, create INFILESYM and
        /// add it to MainSymbolManager.MetadataFileRootSym</item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InitWorker()
        {
            bool br = true;

            // isInited controls if we tear down in COMPILER::Term()
            // Make sure things can handle calling Term without Init
            this.isInited = true;

            // Initialize the error buffer
            InitErrorBuffer();

            // Load ALINK.DLL and initialize it. Do this in a late bound way so
            // we load the correct copy via the shim.
            // In this project, ALINK.DLL is included, so we can normally create its instance.
            this.Linker = new CAsmLink(this.Controller);
            this.Linker.Init();

            //--------------------------------------------------
            // Initialize global symbols that we always use.
            //--------------------------------------------------
            this.MainSymbolManager.Init();
            this.privExternAliasParentSym = MainSymbolManager.CreateGlobalSym(
                SYMKIND.SCOPESYM,
                NameManager.GetPredefinedName(PREDEFNAME.EXTERNALIASCONTAINER),
                null) as PARENTSYM;
            this.Importer.Init();

            //--------------------------------------------------
            // Force us to check the search path for warnings
            //--------------------------------------------------
            GetSearchPath();

            //--------------------------------------------------
            // Set the current directory to DefaultOutputDirectory.
            //--------------------------------------------------
            SetDefaultOutputDirectory(null);

            //--------------------------------------------------
            // Add each of the imports (must be done after initialization of symmgr...)
            //--------------------------------------------------
            List<string[]> importList = this.OptionManager.ImportList;
            if (importList != null && importList.Count > 0)
            {
                for (int i = 0; i < importList.Count; ++i)
                {
                    string[] imp = importList[i];
                    // imp[0]: alias, imp[1]: filename
                    if (imp == null || imp.Length != 2 || String.IsNullOrEmpty(imp[1]))
                    {
                        continue;
                    }

                    INFILESYM inFileRef = FindAndAddMetadataFile(imp[1], Kaid.Nil, true);
                    if (inFileRef != null)
                    {
                        if (!String.IsNullOrEmpty(imp[0]))
                        {
                            AddInfileToExternAliasWithErrors(inFileRef, imp[0]);
                        }
                        else
                        {
                            inFileRef.AddToAlias(Kaid.Global);
                            MainSymbolManager.GlobalAssemblyBitset.SetBit(inFileRef.GetAssemblyID());
                        }
                    }
                }
            }

            // Add each of the addmodules
            List<string> moduleList = this.OptionManager.ModuleList;
            if (moduleList != null && moduleList.Count > 0)
            {
                for (int i = 0; i < moduleList.Count; ++i)
                {
                    FindAndAddMetadataFile(moduleList[i], Kaid.ThisAssembly, false);
                }
            }

            return br;
        }

        //------------------------------------------------------------
        // COMPILER.SetDefaultOutputDirectory
        //
        /// <summary>
        /// If a given directory path is valid, set it to COMPILER.DefaultOutputDirectory.
        /// Or set the current directory to default.
        /// </summary>
        /// <param name="dir"></param>
        //------------------------------------------------------------
        internal void SetDefaultOutputDirectory(string dir)
        {
            DirectoryInfo dirInfo = null;
            Exception excp = null;
            FileAttributes flags =
                FileAttributes.ReadOnly |
                FileAttributes.System |
                FileAttributes.Hidden;

            if (!String.IsNullOrEmpty(dir))
            {
                if (IOUtil.CreateDirectoryInfo(dir, out dirInfo, out excp) &&
                    dirInfo.Exists &&
                    (dirInfo.Attributes & flags) == 0)
                {
                    this.DefaultOutputDirectory = dirInfo.FullName;
                    return;
                }
            }
            this.DefaultOutputDirectory = Directory.GetCurrentDirectory();
        }

        //------------------------------------------------------------
        // COMPILER.Term
        //
        /// <summary>
        /// Terminate the compiler.
        /// Terminates all subsystems and frees all allocated memory.
        /// </summary>
        /// <param name="normalTerm">Do not use for now.</param>
        //------------------------------------------------------------
        internal void Term(bool normalTerm)
        {
            // nothing to do.
            if (!isInited) return;

            // set this to false to prevent recalling Term()
            isInited = false;

            // Terminate everything. Check leaks on a normal termination.

            // Shutdown the major components first
            this.Emitter.Term();
            //assemFile.Term(); // PEFile に Term はない。

            this.Importer.Term();

            this.MainSymbolManager.Term();

            // Free the search path
            if (libPathList != null)
            {
                libPathList = null;
            }

            // Free the link to alink.dll
            if (Linker != null)
            {
                Linker = null;
            }

            // Free the metadata dispenser
            //if (this.Dispenser != null)
            //{
            //    this.Dispenser = null;
            //}

            // free link to mscorpe.dll
            //pfnCreateCeeFileGen = NULL;
            //pfnDestroyCeeFileGen = NULL;

            // Free the various heaps
            this.CCSymbolTable.ClearAll();

            // Last but not least, release the error buffer
            if (ErrorBuffer != null)
            {
                ErrorBuffer.Length = 0;
            }
        }

        //------------------------------------------------------------
        // COMPILER.CleanUp
        //
        /// <summary></summary>
        /// <param name="succeeded"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CleanUp(bool succeeded)
        {
            bool br = succeeded;

            if (succeeded)
            {
                if (GetFirstOutFile() == MainSymbolManager.MetadataFileRootSym &&
                    GetFirstOutFile().NextOutputFile() == null)
                {
                    // didn't compile any source files
                    br = false;
                }
                else if (FAbortEarly(0, null))
                {
                    br = false;
                }
            }

            // Normal termination; check memory leaks.
            Term(true);
            return br;
        }

        //------------------------------------------------------------
        // COMPILER.InitErrorBuffer
        //
        /// <summary>
        /// Clear COMPILER.ErrorBuffer.
        /// </summary>
        //------------------------------------------------------------
        internal void InitErrorBuffer()
        {
            if (this.ErrorBuffer == null)
            {
                this.ErrorBuffer = new StringBuilder();
            }
            this.ErrorBuffer.Length = 0;
        }

        //------------------------------------------------------------
        // COMPILER.DiscardLocalState
        //
        /// <summary>
        /// Discards all state accumulated in the local heap and local symbols
        /// </summary>
        //------------------------------------------------------------
        internal void DiscardLocalState()
        {
            this.LocalSymbolManager.DestroyLocalSymbols();
            FuncBRec.DiscardLocalState();
        }

        // Error building and handling methods.
        // NOTE: We explicitly DO NOT want these to be inlined
        // - for the sake of code size and stack usage.

        //------------------------------------------------------------
        // COMPILER.AddLocationToError
        //
        /// <summary>
        /// <para>This function adds the given ERRLOC data as a location to the given error.
        /// If there is any kind of failure, the host is told that things are toast via
        /// OnCatastrophicError().</para>
        /// <para>Add the error location of an ERRLOC instance
        /// to locationList and mappedLocationList of a CError instance.</para>
        /// </summary>
        /// <param name="error"></param>
        /// <param name="errLoc"></param>
        //------------------------------------------------------------
        internal void AddLocationToError(CError error, ERRLOC errLoc)
        {
            // No file name means no location.
            if (error == null || errLoc == null || String.IsNullOrEmpty(errLoc.SourceFileName))
            {
                return;
            }

            POSDATA posStart = new POSDATA();
            POSDATA posEnd = new POSDATA();
            POSDATA mapStart = new POSDATA();
            POSDATA mapEnd = new POSDATA();

            // See if there's a line/column location
            if (errLoc.HasLocation)
            {
                posStart.LineIndex = errLoc.LineIndex;
                posStart.CharIndex = errLoc.CharIndex;

                posEnd.LineIndex = errLoc.LineIndex;
                posEnd.CharIndex = posStart.CharIndex + errLoc.Extent;

                if (errLoc.MappedLineIndex < 0)
                {
                    mapStart.SetUninitialized();
                    mapEnd.SetUninitialized();
                }
                else
                {
                    mapStart.LineIndex = errLoc.MappedLineIndex;
                    mapStart.CharIndex = errLoc.CharIndex;

                    mapEnd.LineIndex = errLoc.MappedLineIndex;
                    mapEnd.CharIndex = mapStart.CharIndex + errLoc.Extent;
                }
            }

            if (!error.AddLocation(
                    errLoc.SourceFileName,
                    posStart,
                    posEnd,
                    errLoc.SourceMapFileName,
                    mapStart,
                    mapEnd))
            {
                Controller.OnCatastrophicError("CError.AddLocation");
            }
        }

        //------------------------------------------------------------
        // COMPILER.AddLocationToError
        //------------------------------------------------------------
        //void AddLocationToError(CError * err, const ERRLOC loc) { AddLocationToError(err, &loc); }

        //------------------------------------------------------------
        // COMPILER.AddRelatedSymLoc
        //
        /// <summary>
        /// <para>Get an error location from a given SYM instace and
        /// Set it to a CError instance.</para>
        /// </summary>
        /// <param name="err"></param>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal void AddRelatedSymLoc(CError err, SYM sym)
        {
        LRestart:
            switch (sym.Kind)
            {
                default:
                    if (sym.IsTYPESYM)
                    {
                        sym = (sym as TYPESYM).GetNakedAgg(false);
                        if (sym != null) goto LRestart;
                    }
                    else
                    {
                        this.AddLocationToError(
                            err,
                            new ERRLOC(
                                sym.GetInputFile(),
                                sym.GetParseTree(),
                                OptionManager.FullPaths));
                    }
                    break;

                case SYMKIND.AGGTYPESYM:
                    DebugUtil.Assert((sym as AGGTYPESYM) != null);
                    sym = (sym as AGGTYPESYM).GetAggregate();
                    goto LRestart;

                case SYMKIND.AGGSYM:
                    // For unresolved classes, use the module's input file for the location.
                    if ((sym as AGGSYM).IsUnresolved)
                    {
                        DebugUtil.Assert((sym as AGGSYM) != null);
                        DebugUtil.Assert((sym as AGGSYM).AsUnresolved() != null);
                        DebugUtil.Assert((sym as AGGSYM).AsUnresolved().ModuleErr != null);

                        this.AddLocationToError(
                            err,
                            new ERRLOC(
                                (sym as AGGSYM).AsUnresolved().ModuleErr.GetInputFile(),
                                null,
                                OptionManager.FullPaths));
                    }
                    else
                    {
                        // We have a class -- dump all declarations of it.
                        for (AGGDECLSYM decl = (sym as AGGSYM).FirstDeclSym;
                            decl != null; decl = decl.NextDeclSym)
                        {
                            this.AddLocationToError(
                                err,
                                new ERRLOC(
                                    decl.GetInputFile(),
                                    decl.GetParseTree(),
                                    OptionManager.FullPaths));
                        }
                    }
                    break;

                case SYMKIND.NSSYM:
                    // Dump all declarations of the namespace.
                    for (NSDECLSYM decl = (sym as NSSYM).FirstDeclSym;
                        decl != null; decl = decl.NextDeclSym)
                    {
                        AddRelatedSymLoc(err, decl);
                    }
                    break;

                case SYMKIND.NSAIDSYM:
                    {
                        // Dump all declarations of the namespace that are in the aid.
                        int aid = (sym as NSAIDSYM).GetAssemblyID();
                        for (NSDECLSYM decl = (sym as NSAIDSYM).NamespaceSym.FirstDeclSym;
                            decl != null; decl = decl.NextDeclSym)
                        {
                            if (decl.GetInputFile().InAlias(aid))
                            {
                                AddRelatedSymLoc(err, decl);
                            }
                        }
                    }
                    break;

                case SYMKIND.TYVARSYM:
                    if (sym.GetParseTree() != null)
                    {
                        AddLocationToError(
                            err,
                            new ERRLOC(
                                MainSymbolManager,
                                sym.GetParseTree(),
                                OptionManager.FullPaths));
                    }
                    else if (sym.ParentSym != null)
                    {
                        AddRelatedSymLoc(err, sym.ParentSym);
                    }
                    break;

                case SYMKIND.LOCVARSYM:
                    break;

                case SYMKIND.INFILESYM:
                    AddLocationToError(
                        err,
                        new ERRLOC(
                            sym as INFILESYM,
                            this.OptionManager.FullPaths));
                    break;

                case SYMKIND.MODULESYM:
                    AddLocationToError(
                        err,
                        new ERRLOC(
                            sym.GetInputFile(),
                            this.OptionManager.FullPaths));
                    break;
            }
        }

        //------------------------------------------------------------
        // COMPILER.SubmitError (1)
        //
        /// <summary>
        /// This function submits the given error to the controller,
        /// and if it's a fatal error, throws the fatal exception.
        /// </summary>
        /// <param name="error"></param>
        //------------------------------------------------------------
        internal void SubmitError(CError error)
        {
            //ResetErrorBuffer();
            if (error == null)
            {
                return;
            }
            bool shouldThrow = (error.Kind == ERRORKIND.FATAL && error.ErrorID != CSCERRID.FTL_StackOverflow);

            if (error.LocationCount > 0)
            {
                string fileName = null;
                INFILESYM infileSym = null;
                POSDATA pos1, pos2;

                if (error.GetUnmappedLocationAt(0, out fileName, out pos1, out pos2)
                    && !String.IsNullOrEmpty(fileName)
                    //&& this.NameManager.Lookup(fileName) == true
                    && (infileSym = MainSymbolManager.FindInfileSym(fileName)) != null
                    && infileSym.SourceData != null
                    && infileSym.SourceData.Module.IsWarningDisabled(error))
                {
                    return;
                }
            }

            this.Controller.SubmitError(error);

#if DEBUG
            //if (GetRegDWORD("Error"))
            //{
            //if (MessageBoxW(0, error.GetText(), L"ASSERT?", MB_YESNO) == IDYES)
            //{
            //    ASSERT(FALSE);
            //}
            //}
#endif

            // Stack overflow error is reported from inside the exception handler
            // so we don't need to raise another exception
            if (shouldThrow)
            {
                ThrowFatalException();
            }
        }

        //------------------------------------------------------------
        // COMPILER.MakeErrorLocArgs
        //
        /// <summary>
        /// <para>Methods to make an error object. These DO NOT submit the error.</para>
        /// <para>errArgs の中には、エラーメッセージ書式の引数ではなく、追加の位置情報を示すものがある。
        /// それを考慮して CError インスタンスを作成する。</para>
        /// </summary>
        /// <param name="errLoc"></param>
        /// <param name="csuId"></param>
        /// <param name="errArgs"></param>
        /// <param name="warnOverride"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CError MakeErrorLocArgs(
            ERRLOC errLoc,
            ERRORKIND errKind,  // 2016/05/12 hirano567@hotmail.co.jp
            string errText,     // 2016/05/12 hirano567@hotmail.co.jp
            Exception excp,     // 2016/05/12 hirano567@hotmail.co.jp
            CSCERRID csuId,
            ErrArg[] errArgs,
            bool warnOverride) // = false)
        {
            if (errArgs == null)
            {
                errArgs = new ErrArg[0];
            }

            // Create an arg array manually using the type information in the ErrArgs.
            string[] argStrArray = new string[errArgs.Length];
            int[] argRecArray = new int[errArgs.Length];

            int strIndex = 0;    // argStrArray のインデックス;
            int recIndex = 0;    // argRecArray のインデックス;
            int uniqueArgCount = 0;

            //--------------------------------------------------
            // ErrArg[] errArgs の各要素に対して以下の処理を施す。
            // ・NoStr が設定されているなら argStrArray、argRecArray では考慮しない。
            // ・各要素を表す文字列作成して argStrArray に格納する。
            // ・Unique が設定されていないなら argRecArray の対応する要素の値を -1 とする。
            // ・Unique が設定されているなら errArgs のインデックスを argRecArray の要素の値とする。
            //--------------------------------------------------
            for (int iarg = 0; iarg < errArgs.Length; iarg++)
            {
                ErrArg earg = errArgs[iarg];

                // If the NoStr bit is set we don't add it to argStrArray.
                // NoStr は ErrArg が位置を示すだけでメッセージには含めないものであることを示す。
                if ((earg.ErrorArgumentFlags & ErrArgFlagsEnum.NoStr) != 0)
                {
                    continue;
                }

                string str = null;
                int rec = -1;

                switch (earg.ErrorArgumentKind)
                {
                    case ErrArgKindEnum.Int:
                        //str = (PCWSTR)(INT_PTR)earg.n;
                        str = earg.Int.ToString();
                        break;

                    case ErrArgKindEnum.ResNo:
                        str = ErrId(earg.ResNo);
                        break;

                    case ErrArgKindEnum.SymKind:
                        str = ErrSK(earg.SymKind);
                        break;

                    case ErrArgKindEnum.AggKind:
                        str = ErrAggKind(earg.AggKind);
                        break;

                    case ErrArgKindEnum.Sym:
                        str = ErrSym(earg.Sym, null, true);
                        rec = iarg;
                        break;

                    case ErrArgKindEnum.Name:
                        str = ErrName(earg.Name);
                        break;

                    case ErrArgKindEnum.Str:
                        str = earg.Str;
                        // 元のソースでは、不適な文字を使用していた場合は
                        // ErrAppendString 関数で複製しそれを str としていたが、
                        // ここではその必要はない。
                        break;

                    case ErrArgKindEnum.PredefName:
                        str = ErrName(NameManager.GetPredefinedName(earg.PredefinedName));
                        break;

                    case ErrArgKindEnum.NameNode:
                        str = ErrNameNode(earg.BaseNode);
                        rec = iarg;
                        break;

                    case ErrArgKindEnum.TypeNode:
                        str = ErrTypeNode(earg.TypeNode.AsTYPEBASE);
                        rec = iarg;
                        break;

                    case ErrArgKindEnum.Ptr:
                        str = earg.Ptr.ToString();
                        break;

                    case ErrArgKindEnum.SymWithType:
                        {
                            SubstContext ctx = new SubstContext(
                                earg.SymWithType.AggTypeSym,
                                null,
                                SubstTypeFlagsEnum.NormNone);
                            str = ErrSym(earg.SymWithType.Sym, ctx, true);
                            rec = iarg;
                        }
                        break;

                    case ErrArgKindEnum.MethWithInst:
                        {
                            SubstContext ctx = new SubstContext(
                                earg.MethPropWithInst.AggTypeSym,
                                earg.MethPropWithInst.TypeArguments,
                                SubstTypeFlagsEnum.NormNone);
                            str = ErrSym(earg.MethPropWithInst.Sym, ctx, true);
                            rec = iarg;
                        }
                        break;

                    default:
                        //VSFAIL("Bad arg kind");
                        continue;
                }

                if ((earg.ErrorArgumentFlags & ErrArgFlagsEnum.Unique) == 0)
                {
                    rec = -1;
                }
                else if (rec >= 0)
                {
                    uniqueArgCount++;
                }

                argStrArray[strIndex] = str;
                ++strIndex;
                argRecArray[recIndex] = rec;
                ++recIndex;
            }

            // NoStr は除外してある。
            int arrayLength = strIndex;

            //--------------------------------------------------
            // errArg の要素の中に Unique が設定されているものがある場合、
            // それらの中に同じ文字列を持つが位置が異なるので
            // 区別できるようにしなければならないものがあるなら、
            // それぞれの文字列に位置情報を追加して区別できるようにする。
            //--------------------------------------------------
            if (uniqueArgCount > 1)
            {
                // Copy the strings over to another buffer.
                string[] argStrArrayNew = new string[arrayLength];
                argStrArray.CopyTo(argStrArrayNew, 0);

                for (int i = 0; i < arrayLength; i++)
                {
                    // Unique でない場合、位置情報を追加している場合は次の要素へ。
                    if (argRecArray[i] < 0 || argStrArrayNew[i] != argStrArray[i])
                    {
                        continue;
                    }

                    ErrArg earg1 = errArgs[argRecArray[i]];
                    DebugUtil.Assert(
                      (earg1.ErrorArgumentFlags & ErrArgFlagsEnum.Unique) != 0 &&
                      (earg1.ErrorArgumentFlags & ErrArgFlagsEnum.NoStr) == 0);

                    SYM sym1 = null;
                    BASENODE node1 = null;
                    bool isSource = false;
                    bool isMunge = false;   // 位置情報を追加する必要があるなら true にする。

                    switch (earg1.ErrorArgumentKind)
                    {
                        case ErrArgKindEnum.Sym:
                            sym1 = earg1.Sym;
                            break;

                        case ErrArgKindEnum.NameNode:
                            node1 = earg1.NameNode;
                            break;

                        case ErrArgKindEnum.TypeNode:
                            node1 = earg1.TypeNode;
                            break;

                        case ErrArgKindEnum.SymWithType:
                            sym1 = earg1.SymWithType.Sym;
                            break;

                        case ErrArgKindEnum.MethWithInst:
                            sym1 = earg1.MethPropWithInst.Sym;
                            break;

                        default:
                            DebugUtil.Assert(false, "Shouldn't be here!");
                            continue;
                    }
                    DebugUtil.Assert((sym1 == null) != (node1 == null));

                    for (int j = i + 1; j < arrayLength; j++)
                    {
                        // Unique でない場合、earg1 と文字列として異なる場合は次の要素へ。
                        if (argRecArray[j] < 0)
                        {
                            continue;
                        }
                        if ((errArgs[argRecArray[i]].ErrorArgumentFlags & ErrArgFlagsEnum.Unique) == 0)
                        {
                            continue;
                        }
                        if (String.Compare(argStrArray[i], argStrArray[j]) != 0)
                        {
                            continue;
                        }

                        // The strings are identical. If they are the same symbol, leave them alone.
                        // Otherwise, munge both strings. If j has already been munged, just make
                        // sure we munge i.

                        // 位置情報が既に追加されている場合
                        if (argStrArrayNew[j] != argStrArray[j])
                        {
                            // argStrArray[j] は異なる位置で使用されるので、
                            // 区別できるようにしなければならない。
                            isMunge = true;
                            continue;
                        }

                        ErrArg earg2 = errArgs[argRecArray[j]];
                        DebugUtil.Assert(
                            (earg2.ErrorArgumentFlags & ErrArgFlagsEnum.Unique) != 0 &&
                            (earg2.ErrorArgumentFlags & ErrArgFlagsEnum.NoStr) == 0);

                        SYM sym2 = null;
                        BASENODE node2 = null;

                        switch (earg2.ErrorArgumentKind)
                        {
                            case ErrArgKindEnum.Sym:
                                sym2 = earg2.Sym;
                                break;

                            case ErrArgKindEnum.NameNode:
                                node2 = earg2.NameNode;
                                break;

                            case ErrArgKindEnum.TypeNode:
                                node2 = earg2.TypeNode;
                                break;

                            case ErrArgKindEnum.SymWithType:
                                sym2 = earg2.SymWithType.Sym;
                                break;

                            case ErrArgKindEnum.MethWithInst:
                                sym2 = earg2.MethPropWithInst.Sym;
                                break;

                            default:
                                DebugUtil.Assert(false, "Shouldn't be here!");
                                continue;
                        }
                        DebugUtil.Assert((sym2 == null) != (node2 == null));

                        if (sym2 == sym1 && node2 == node1 && !isMunge)
                        {
                            continue;
                        }

                        // ここへくるのは、earg1 と earg2 が argStrArray に同じ文字列を持つが、
                        // node か sym が異なる場合か、
                        // 区別できるようにしなければならないことが判明している場合である。

                        argStrArrayNew[j] = ErrStrWithLoc(argStrArray[j], sym2, node2, out isSource);
                        if (isSource)
                        {
                            earg2.ErrorArgumentFlags = earg2.ErrorArgumentFlags | ErrArgFlagsEnum.Ref;
                        }
                        isMunge = true;
                    }   // for (int j = i + 1; j < arrayLength; j++)

                    if (isMunge)
                    {
                        argStrArrayNew[i] = ErrStrWithLoc(argStrArray[i], sym1, node1, out isSource);
                        if (isSource)
                        {
                            earg1.ErrorArgumentFlags = earg1.ErrorArgumentFlags | ErrArgFlagsEnum.Ref;
                        }
                    }
                }   // for (int i = 0; i < arrayLength; i++)
                argStrArray = argStrArrayNew;   // argStrArray の有効なサイズは arrayLength
            }   // if (uniqueArgCount > 1)

            //--------------------------------------------------
            // CError インスタンス err を作成する。
            // ・argStrArray から ErrArg[] を作成する。
            // ・引数 errLoc が有効なら err に設定する。
            // ・errArgs に位置情報を持つものがあればそれも err に設定する。
            //--------------------------------------------------
            CError errObj;

            // argStrArray の有効なサイズは arrayLength
            ErrArg[] eargs = new ErrArg[argStrArray.Length];
            for (int i = 0; i < argStrArray.Length; ++i)
            {
                eargs[i] = new ErrArg(argStrArray[i]);
            }

            if (!String.IsNullOrEmpty(errText))  // 2016/05/12 hirano567@hotmail.co.jp
            {
                if (!Controller.CreateError(errKind, errText, out errObj))
                {
                    return null;
                }
            }
            else if (excp != null)  // 2016/05/12 hirano567@hotmail.co.jp
            {
                if (!Controller.CreateError(errKind, excp, out errObj))
                {
                    return null;
                }
            }
            else if (!Controller.CreateError(csuId, eargs, out errObj, warnOverride))
            {
                return null;
            }

            AddLocationsToCError(errObj, errLoc, errArgs);
            return errObj;
        }

        //------------------------------------------------------------
        // COMPILER.AddLocationsToCError
        //
        /// <summary></summary>
        /// <param name="errObj"></param>
        /// <param name="errLoc"></param>
        /// <param name="errArgs"></param>
        //------------------------------------------------------------
        internal void AddLocationsToCError(CError errObj, ERRLOC errLoc, ErrArg[] errArgs)
        {
            if (errObj == null)
            {
                return;
            }

            if (errLoc != null)
            {
                AddLocationToError(errObj, errLoc);
            }

            // Add other locations as appropriate.

            if (errArgs == null || errArgs.Length == 0)
            {
                return;
            }
            for (int i = 0; i < errArgs.Length; ++i)
            {
                ErrArg earg = errArgs[i];
                if ((earg.ErrorArgumentFlags & ErrArgFlagsEnum.Ref) == 0)
                {
                    continue;
                }

                switch (earg.ErrorArgumentKind)
                {
                    case ErrArgKindEnum.Sym:
                        AddRelatedSymLoc(errObj, earg.Sym);
                        break;

                    case ErrArgKindEnum.LocNode:
                        AddLocationToError(
                            errObj,
                            new ERRLOC(
                                MainSymbolManager,
                                earg.LocNode,
                                OptionManager.FullPaths));
                        break;

                    case ErrArgKindEnum.NameNode:
                        AddLocationToError(
                            errObj,
                            new ERRLOC(
                                MainSymbolManager,
                                earg.NameNode,
                                OptionManager.FullPaths));
                        break;

                    case ErrArgKindEnum.TypeNode:
                        AddLocationToError(
                            errObj,
                            new ERRLOC(
                                MainSymbolManager,
                                earg.TypeNode,
                                OptionManager.FullPaths));
                        break;

                    case ErrArgKindEnum.SymWithType:
                        AddRelatedSymLoc(errObj, earg.SymWithType.Sym);
                        break;

                    case ErrArgKindEnum.MethWithInst:
                        AddRelatedSymLoc(errObj, earg.MethPropWithInst.Sym);
                        break;

                    default:
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.MakeErrorTreeArgs
        //
        /// <summary>
        /// <para>Create an ERRLOC instance with a given node and
        /// call MakeErrorLocArgs method to create a CError instance.</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CError MakeErrorTreeArgs(
            BASENODE node,
            ERRORKIND errKind,
            string errText,
            Exception excp,
            CSCERRID id,
            params ErrArg[] args)
        {
            if (node != null)
            {
                ERRLOC errloc = new ERRLOC(
                    MainSymbolManager,
                    node,
                    OptionManager.FullPaths);

                return MakeErrorLocArgs(
                    errloc,
                    errKind,
                    errText,
                    excp,
                    id,
                    args,
                    false);
            }
            else
            {
                return MakeErrorLocArgs(
                    null,
                    errKind,
                    errText,
                    excp,
                    id,
                    args,
                    false);
            }
        }

        // csharp\sccomp\compiler.h (549)
        // NOTE: We explicitly DO NOT want these to be inlined - for the sake of code size and stack usage.
        // By default these DO NOT add related locations. To add a related location, pass an ErrArgRef.

        // csharp\sccomp\error.cpp (1826)
        // NOTE: We'd like to skip creating the temp array (by making these __cdecl and passing &a instead of args),
        // but we're not guaranteed that the compiler will do the right thing. Maybe do this on x86?

        //------------------------------------------------------------
        // COMPILER.MakeError (1)
        //
        /// <summary>
        /// <para>Call MakeErrorLocArgs method to create a CError instance.</para>
        /// <para>MakeErrorLocArgs calls MakeErrorLocArgs method.</para>
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CError MakeError(BASENODE tree, CSCERRID id, params ErrArg[] args)
        {
            return MakeErrorTreeArgs(
                tree,
                ERRORKIND.NONE,
                null,
                null,
                id,
                args);
        }

        //------------------------------------------------------------
        // COMPILER.MakeError (2)
        //
        /// <summary>
        /// <para>Call MakeErrorLocArgs method to create a CError instance.</para>
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CError MakeError(ERRLOC loc, CSCERRID id, params ErrArg[] args)
        {
            return MakeErrorLocArgs(
                loc,
                ERRORKIND.NONE,
                null,
                null,
                id,
                args,
                false);
        }

        //------------------------------------------------------------
        // COMPILER.ErrorLocArgs
        //
        /// <summary>
        /// <para>Call MakeErrorLocArgs method to create a CError instance.
        /// Then call SubmitError method with it to show a error message.</para>
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public void ErrorLocArgs(ERRLOC loc, CSCERRID id, params ErrArg[] args)
        {
            SubmitError(MakeErrorLocArgs(
                loc,
                ERRORKIND.NONE,
                null,
                null,
                id,
                args,
                false));
        }

        //------------------------------------------------------------
        // COMPILER.ErrorTreeArgs
        //
        /// <summary>
        /// <para>Create a CError instance with id, args, tree and call method Submit.</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ErrorTreeArgs(BASENODE node, CSCERRID id, params ErrArg[] args)
        {
            SubmitError(
                MakeErrorTreeArgs(
                    node,
                    ERRORKIND.NONE,
                    null,
                    null,
                    id,
                    args));
        }

        //------------------------------------------------------------
        // COMPILER.Error (1-1)
        //
        /// <summary>
        /// <para>By default these DO NOT add related locations.
        /// To add a related location, pass an ErrArgRef.</para>
        /// <para>エラーコード id、エラーの内容 args、位置 tree から CError インスタンスを作成し、
        /// これを引数として Submit を呼び出す。</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Error(BASENODE node, CSCERRID id, params ErrArg[] args)
        {
            ErrorTreeArgs(node, id, args);
        }

        //------------------------------------------------------------
        // COMPILER.Error (1-2)
        //
        /// <summary>
        /// <para>エラーコード id、エラーの内容 args、位置 loc から CError インスタンスを作成し、
        /// これを引数として Submit を呼び出す。</para>
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Error(ERRLOC loc, CSCERRID id, params ErrArg[] args)
        {
            ErrorLocArgs(loc, id, args);
        }

        //------------------------------------------------------------
        // COMPILER.Error (1-3)
        //
        /// <summary>
        /// <para>エラーコード id、エラーの内容 args から CError インスタンスを作成し、
        /// これを引数として Submit を呼び出す。</para>
        /// <para>位置情報を指定しない場合はこれを使えばよい。</para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Error(CSCERRID id, params ErrArg[] args)
        {
            ErrorLocArgs(null, id, args);
        }

        //------------------------------------------------------------
        // COMPILER.Error (2-1)
        //
        /// <summary></summary>
        /// <param name="loc"></param>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void Error(ERRLOC loc, ERRORKIND kind, Exception excp, params ErrArg[] errArgs)
        {
#if false
            CError errObj = new CError();
            errObj.Initialize(kind, excp);
            AddLocationsToCError(errObj, loc, errArgs);
            SubmitError(errObj);
#endif
            SubmitError(
                MakeErrorLocArgs(
                    loc,
                    kind,
                    null,
                    excp,
                    CSCERRID.Invalid,
                    errArgs,
                    false));
        }

        //------------------------------------------------------------
        // COMPILER.Error (2-2)
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void Error(BASENODE node, ERRORKIND kind, Exception excp, params ErrArg[] errArgs)
        {
            if (node == null)
            {
                Error(kind, excp);
            }
            else
            {
                ERRLOC errloc = new ERRLOC(
                    MainSymbolManager,
                    node,
                    OptionManager.FullPaths);
                Error(errloc, kind, excp);
            }
        }

        //------------------------------------------------------------
        // COMPILER.Error (2-3)
        //
        /// <summary></summary>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void Error(ERRORKIND kind, Exception excp)
        {
            Error((ERRLOC)null, kind, excp);
        }

        //------------------------------------------------------------
        // COMPILER.Error (3-1)
        //
        /// <summary></summary>
        /// <param name="loc"></param>
        /// <param name="kind"></param>
        /// <param name="msg"></param>
        /// <param name="errArgs"></param>
        //------------------------------------------------------------
        internal void Error(ERRLOC loc, ERRORKIND kind, string msg, params ErrArg[] errArgs)
        {
#if false
            CError err = new CError();
            err.Initialize(kind, msg);
            AddLocationsToCError(err, loc, errArgs);
            SubmitError(err);
#endif
            SubmitError(
                MakeErrorLocArgs(
                    loc,
                    kind,
                    msg,
                    null,
                    CSCERRID.Invalid,
                    errArgs,
                    false));
        }

        //------------------------------------------------------------
        // COMPILER.Error (3-2)
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <param name="kind"></param>
        /// <param name="msg"></param>
        /// <param name="errArgs"></param>
        //------------------------------------------------------------
        internal void Error(
            BASENODE node,
            ERRORKIND kind,
            string msg,
            params ErrArg[] errArgs)
        {
            if (node == null)
            {
                Error(kind, msg);
            }
            else
            {
                ERRLOC errloc = new ERRLOC(
                    MainSymbolManager,
                    node,
                    OptionManager.FullPaths);
                Error(errloc, kind, msg, errArgs);
            }
        }

        //------------------------------------------------------------
        // COMPILER.Error (3-3)
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal void Error(ERRORKIND kind, string msg)
        {
            Error((ERRLOC)null, kind, msg);
        }

        //------------------------------------------------------------
        // COMPILER.ErrorRef
        //
        /// <summary>
        /// <para>By default these DO add related locations.</para>
        /// <para>Create a CError instance with id, args, tree, and call Submit method.</para>
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="args"></param>
        /// <param name="id"></param>
        //------------------------------------------------------------
        internal void ErrorRef(BASENODE tree, CSCERRID id, params ErrArgRef[] args)
        {
            ErrorTreeArgs(tree, id, args);
        }

        // csharp\sccomp\compiler.cpp (1923)
        // CompilerExceptionFilter
        //
        //  This hits whenever we hit an unhandled ASSERT or GPF in the compiler
        //  Here we dump the entire LOCATION stack to the error channel.

        //------------------------------------------------------------
        // COMPILER.ReportICE
        //------------------------------------------------------------
        //void COMPILER::ReportICE(EXCEPTION_POINTERS * exceptionInfo)
        //{
        //    // We shouldn't get here on a fatal error exception
        //    ASSERT(exceptionInfo->ExceptionRecord->ExceptionCode != FATAL_EXCEPTION_CODE);
        //    if (exceptionInfo->ExceptionRecord->ExceptionCode == FATAL_EXCEPTION_CODE) {
        //        return;
        //    }
        //
        //    LOCATION * loc = location;
        //    if (exceptionInfo->ExceptionRecord->ExceptionCode == STATUS_STACK_OVERFLOW) {
        //        // Don't try for a source location because the SetLine, SetStart, and SetENd will also overflow
        //        while (loc && !stackOverflowLocation) {
        //            stackOverflowLocation = loc->getSymbol();
        //            loc = loc->getPrevious();
        //        }
        //        if (stackOverflowLocation == NULL && location)
        //            stackOverflowLocation = location->getFile();
        //    } else if (loc) {
        //
        //        //
        //        // dump probable culprit
        //        //
        //        Error(NULL, ERR_ICE_Culprit, exceptionInfo->ExceptionRecord->ExceptionCode, g_stages[loc->getStage()],
        //            ErrArgPtr(exceptionInfo->ExceptionRecord->ExceptionAddress));
        //
        //        //
        //        // dump location stack
        //        //
        //        do {
        //
        //            PCWSTR stage = g_stages[loc->getStage()];
        //
        //            //
        //            // dump one location
        //            //
        //            SYM *symbol = loc->getSymbol();
        //            if (symbol) {
        //                //
        //                // we have a symbol report it nicely
        //                //
        //                ErrorRef(NULL, ERR_ICE_Symbol, symbol, stage);
        //            } else {
        //                INFILESYM *file = loc->getFile();
        //                if (file) {
        //                    BASENODE *node = loc->getNode();
        //                    if (node) {
        //                        //
        //                        // we have stage, file and node
        //                        //
        //                        compiler()->Error(node, ERR_ICE_Node, stage);
        //                    } else {
        //                        //
        //                        // we have stage and file
        //                        //
        //                        Error(ERRLOC(file), ERR_ICE_File, stage);
        //                    }
        //                } else {
        //                    //
        //                    // only thing we have is the stage
        //                    //
        //                    Error(NULL, ERR_ICE_Stage, stage);
        //                }
        //            }
        //
        //            loc = loc->getPrevious();
        //        } while (loc);
        //    } else {
        //        //
        //        // no location at all!
        //        //
        //        Error(NULL, ERR_ICE_Culprit, exceptionInfo->ExceptionRecord->ExceptionCode, g_stages[0],
        //            ErrArgPtr(exceptionInfo->ExceptionRecord->ExceptionAddress));
        //    }
        //}

        //------------------------------------------------------------
        // COMPILER.HandleException
        //------------------------------------------------------------
        //void HandleException(DWORD exceptionCode);

#if DEBUG
        //------------------------------------------------------------
        // COMPILER.GetRegDWORD
        //
        /// <summary>
        /// Return the dword which lives under HKCU\Software\Microsoft\C# Compiler&lt;value&gt;
        /// If any problems are encountered, return 0
        /// </summary>
        /// <remarks>
        /// (sscli) static DWORD GetRegDWORD(PCSTR value);
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal int GetRegDWORD(string value)
        {
            return 0;
        }

        //------------------------------------------------------------
        // COMPILER.IsRegString
        //
        /// <summary>
        /// <para>Return true if the registry string
        /// which lives under HKCU\Software\Microsoft\C# Compiler&lt;value&gt;
        /// is the same one as the string provided.</para>
        /// <para>Return false for now.</para>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool IsRegString(string str, string value)
        {
            bool rval = false;
            return rval;
        }
#endif

        //static DWORD GetRegDWORDRet(PCSTR value);
        //static BSTR GetRegStringRet(PCSTR value);
        //static bool IsRegStringRet(PCWSTR string, PCWSTR value);

        //------------------------------------------------------------
        // COMPILER.ErrAppendString
        //
        /// <summary>
        /// <para>Add a string to COMPILER.errorBuffer.</para>
        /// <para>In sscli, if too long, replace "...".</para>
        /// </summary>
        /// <param name="str"></param>
        //------------------------------------------------------------
        internal void ErrAppendString(string str)
        {
            if (str == null)
            {
                return;
            }
            this.ErrorBuffer.Append(str);
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendChar
        //
        /// <summary>
        /// <para>Add a character to COMPILER.errorBuffer.</para>
        /// </summary>
        /// <param name="ch"></param>
        //------------------------------------------------------------
        internal void ErrAppendChar(char ch)
        {
            this.ErrorBuffer.Append(ch);
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendPrintf
        //
        /// <summary>
        /// Add a formatted string by a given format and arguments to COMPILER.errorBuffer.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ErrAppendPrintf(string format, params object[] args)
        {
            this.ErrorBuffer.AppendFormat(format, args);
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendId (1)
        //
        /// <summary>
        /// <para>Find the string resource fot a given id and add it to COMPILER.ErrorBuffer.</para>
        /// <para>If not found, add id itself.</para>
        /// </summary>
        /// <param name="sid"></param>
        /// <remarks>
        /// <para>In sscli, this method create an error message by Win32API functions.</para>
        /// </remarks>
        //------------------------------------------------------------
        internal void ErrAppendId(ResNo resNo)
        {
            string text;
            Exception excp = null;

            if (CResources.GetString(resNo, out text, out excp) && !String.IsNullOrEmpty(text))
            {
                ErrorBuffer.Append(text);
            }
            else
            {
                ErrorBuffer.AppendFormat("{0}", resNo);
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendId (2)
        //
        /// <summary>
        /// <para>Find the string resource fot a given id and add it to COMPILER.ErrorBuffer.</para>
        /// <para>If not found, add id itself.</para>
        /// </summary>
        /// <param name="eid"></param>
        /// <remarks>
        /// <para>In sscli, this method create an error message by Win32API functions.</para>
        /// </remarks>
        //------------------------------------------------------------
        internal void ErrAppendId(CSCERRID eid)
        {
            ResNo resNo = CSCErrorInfo.Manager.GetResourceNumber(eid);
            ErrAppendId(resNo);
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendSym
        //
        /// <summary>
        /// <para>Create a fill-in string describing a symbol.</para>
        /// <para>Add a string representing a sym instance to COMPILER.errorBuffer.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="context"></param>
        /// <param name="fArgs"></param>
        //------------------------------------------------------------
        internal void ErrAppendSym(
            SYM sym,
            SubstContext context,
            bool fArgs) //  = true
        {
            if (sym.IsTYPESYM && context != null)
            {
                if (!context.FNop())
                {
                    sym = MainSymbolManager.SubstType(sym as TYPESYM, context);
                }
                // We shouldn't use the SubstContext again so set it to NULL.
                context = null;
            }

            switch (sym.Kind)
            {
                case SYMKIND.NSDECLSYM:
                    // for namespace declarations just convert the namespace
                    ErrAppendSym((sym as NSDECLSYM).NamespaceSym, null, true);
                    break;

                case SYMKIND.EXTERNALIASSYM:
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.ALIASSYM:
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.GLOBALATTRSYM:
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.AGGDECLSYM:
                    // AGGDECLSYM は DECLSYM の派生クラスなので、BAGSYM 型のフィールドを持っている。
                    // そして、それを AGGSYM 型のインスタンスとして返す AggSym プロパティを持っている。
                    // これにより AGGSYM インスタンスを取得し、それを処理する。
                    ErrAppendSym((sym as AGGDECLSYM).AggSym, context, true);
                    break;

                case SYMKIND.AGGSYM:
                    // Check for a predefined class with a special "nice" name for error reported.
                    string text = BSYMMGR.GetNiceName(sym as AGGSYM);
                    if (text != null)
                    {
                        // Found a nice name.
                        ErrAppendString(text);
                    }
                    else
                    {
                        ErrAppendParentSym(sym, context);
                        ErrAppendName(sym.Name);
                        ErrAppendTypeParameters((sym as AGGSYM).TypeVariables, context, true);
                    }
                    break;

                case SYMKIND.AGGTYPESYM:
                    // Check for a predefined class with a special "nice" name for
                    // error reported.
                    text = BSYMMGR.GetNiceName((sym as AGGTYPESYM).GetAggregate());
                    if (text != null)
                    {
                        // Found a nice name.
                        ErrAppendString(text);
                    }
                    else
                    {
                        if ((sym as AGGTYPESYM).OuterTypeSym != null)
                        {
                            ErrAppendSym((sym as AGGTYPESYM).OuterTypeSym, context, true);
                            ErrAppendChar('.');
                        }
                        else
                        {
                            // In a namespace.
                            ErrAppendParentSym((sym as AGGTYPESYM).GetAggregate(), context);
                        }
                        ErrAppendName((sym as AGGTYPESYM).GetAggregate().Name);
                    }
                    ErrAppendTypeParameters((sym as AGGTYPESYM).TypeArguments, context, true);
                    break;

                case SYMKIND.METHSYM:
                    ErrAppendMethod(sym as METHSYM, context, fArgs);
                    break;

                case SYMKIND.PROPSYM:
                    ErrAppendProperty(sym as PROPSYM, context);
                    break;

                case SYMKIND.EVENTSYM:
                    ErrAppendEvent(sym as EVENTSYM, context);
                    break;

                case SYMKIND.NSAIDSYM:
                    int aid = (sym as NSAIDSYM).GetAssemblyID();
                    sym = (sym as NSAIDSYM).NamespaceSym;

                    if (aid != Kaid.Global)
                    {
                        // Spit out the alias name
                        ErrAppendSym(MainSymbolManager.GetSymForAid(aid), null, true);
                        // If there's nothing besides the alias, stop here
                        if (sym == MainSymbolManager.RootNamespaceSym)
                        {
                            break;
                        }
                        // Otherwise  append ::Namespace
                        ErrAppendChar(':');
                        ErrAppendChar(':');
                    }
                    // Fall through.
                    goto case SYMKIND.NSSYM;

                case SYMKIND.NSSYM:
                    if (sym == MainSymbolManager.RootNamespaceSym)
                    {
                        ErrAppendId(ResNo.CSCSTR_GlobalNamespace);
                    }
                    else
                    {
                        ErrAppendParentSym(sym, null);
                        ErrAppendName(sym.Name);
                    }
                    break;

                case SYMKIND.MEMBVARSYM:
                    ErrAppendParentSym(sym, context);
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.TYVARSYM:
                    if (sym.Name == null)
                    {
                        // It's a standard type variable.
                        if ((sym as TYVARSYM).IsMethodTypeVariable)
                        {
                            ErrAppendChar('!');
                        }
                        ErrAppendChar('!');
                        ErrAppendPrintf("{0:d}", (int)(sym as TYVARSYM).TotalIndex);
                    }
                    else
                    {
                        ErrAppendName(sym.Name);
                    }
                    break;

                case SYMKIND.INFILESYM:
                    // Generate symbol name.
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.MODULESYM:
                    // Generate symbol name.
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.OUTFILESYM:
                    // Generate symbol name.
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.LOCVARSYM:
                    // Generate symbol name.
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.LABELSYM:
                    // Generate symbol name.
                    ErrAppendName(sym.Name);
                    break;

                case SYMKIND.ERRORSYM:
                    if (sym.ParentSym != null)
                    {
                        DebugUtil.Assert(
                            !String.IsNullOrEmpty((sym as ERRORSYM).ErrorName) &&
                            (sym as ERRORSYM).TypeArguments != null);

                        ErrAppendParentSym(sym, context);
                        ErrAppendName((sym as ERRORSYM).ErrorName);
                        ErrAppendTypeParameters((sym as ERRORSYM).TypeArguments, context, true);
                    }
                    else
                    {
                        // Load the string "<error>".
                        DebugUtil.Assert(sym.ParentSym == null);
                        DebugUtil.Assert((sym as ERRORSYM).TypeArguments == null);
                        ErrAppendId(ResNo.CSCSTR_ERRORSYM);
                    }
                    break;

                case SYMKIND.NULLSYM:
                    // Load the string "<null>".
                    ErrAppendId(ResNo.CSCSTR_NULL);
                    break;

                case SYMKIND.UNITSYM:
                    // Leave blank.
                    break;

                case SYMKIND.ANONMETHSYM:
                    ErrAppendId(ResNo.CSCSTR_AnonMethod);
                    break;

                case SYMKIND.METHGRPSYM:
                    ErrAppendId(ResNo.CSCSTR_MethodGroup);
                    break;

                case SYMKIND.ARRAYSYM:
                    TYPESYM elementType;
                    int rank;

                    // 配列の要素の型を求める。それがまた配列ならさらにその要素の型を求める。
                    // 配列でない要素が見つかるまで続ける。
                    for (elementType = sym as TYPESYM;
                        elementType != null && elementType.IsARRAYSYM;
                        elementType = (elementType as ARRAYSYM).ElementTypeSym)
                    {
                        ;
                    }

                    if (elementType == null)
                    {
                        DebugUtil.Assert(false);
                        break;
                    }

                    // 配列でない要素の Sym について処理する。
                    ErrAppendSym(elementType, context, true);

                    // rank-specifiers を表す文字列を追加する。
                    for (elementType = sym as TYPESYM;
                        elementType != null && elementType.IsARRAYSYM;
                        elementType = (elementType as ARRAYSYM).ElementTypeSym)
                    {
                        rank = (elementType as ARRAYSYM).Rank;

                        // Add [] with (rank-1) commas inside
                        ErrAppendChar('[');

                        // known rank.
                        if (rank > 1)
                        {
                            ErrAppendChar('*');
                        }
                        for (int i = rank; i > 1; --i)
                        {
                            ErrAppendChar(',');
                            ErrAppendChar('*');
                        }

                        ErrAppendChar(']');
                    }
                    break;

                case SYMKIND.VOIDSYM:
                    // VOIDSYM の場合は "void" を追加する。
                    ErrAppendName(NameManager.KeywordName(TOKENID.VOID));
                    break;

                case SYMKIND.PARAMMODSYM:
                    // add ref or out
                    if ((sym as PARAMMODSYM).IsRef)
                    {
                        ErrAppendString("ref ");
                    }
                    else
                    {
                        //ASSERT(sym.AsPARAMMODSYM.isOut);
                        ErrAppendString("out ");
                    }

                    // add base type name
                    ErrAppendSym((sym as PARAMMODSYM).ParamTypeSym, context, true);
                    break;

                case SYMKIND.MODOPTTYPESYM:
                    ErrAppendSym((sym as MODOPTTYPESYM).BaseTypeSym, context, true);
                    break;

                case SYMKIND.PTRSYM:
                    // Generate the base type.
                    ErrAppendSym((sym as PTRSYM).BaseTypeSym, context, true);
                    // add the trailing *
                    ErrAppendChar('*');
                    break;

                case SYMKIND.NUBSYM:
                    ErrAppendSym((sym as NUBSYM).BaseTypeSym, context, true);
                    ErrAppendChar('?');
                    break;

                case SYMKIND.SCOPESYM:
                    // Shouldn't happen.
                    DebugUtil.Assert(false, "Bad symbol kind");
                    break;

                case SYMKIND.ANONSCOPESYM:
                    // Shouldn't happen.
                    DebugUtil.Assert(false, "Bad symbol kind");
                    break;

                case SYMKIND.LAMBDAEXPRSYM: // CS3
                    ErrAppendId(ResNo.CSCSTR_LambdaExpression);
                    break;

                default:
                    // Shouldn't happen.
                    DebugUtil.Assert(false, "Bad symbol kind");
                    break;
            }

            //ASSERT(!*errBufferNext); // StringBuilder を使っているのでこの ASSERT は必要ない。
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendName
        //
        /// <summary>
        /// <para>Add a string to COMPILER.errorBuffer.</para>
        /// <para>If the given string match the string for indexer, add "this".</para>
        /// </summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        internal void ErrAppendName(string name)
        {
            if (name == this.NameManager.GetPredefinedName(PREDEFNAME.INDEXERINTERNAL))
            {
                ErrAppendString("this");
            }
            else
            {
                ErrAppendString(name);
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendNameNode
        //
        /// <summary>
        /// Create a fill-in string describing a possibly fully qualified name.
        /// </summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        internal void ErrAppendNameNode(BASENODE name)
        {
            if (name.PrevNode != null)
            {
                NAMENODE prev = name.PrevNode.AsANYNAME;
                if (prev != null)
                {
                    ErrAppendNameNode(prev);
                    if (prev.Kind == NODEKIND.ALIASNAME)
                    {
                        ErrorBuffer.Append("::");
                    }
                    else
                    {
                        ErrorBuffer.Append('.');
                    }
                }
            }

            switch (name.Kind)
            {
                case NODEKIND.NAME:
                    // non-dotted name, just do the regular name thing
                    ErrAppendName(name.AsSingleName.Name);
                    break;

                case NODEKIND.ALIASNAME:
                    // non-dotted name, just do the regular name thing
                    ErrAppendName(name.AsSingleName.Name);
                    break;

                case NODEKIND.GENERICNAME:
                    ErrAppendName(name.AsANYNAME.Name);
                    ErrAppendString("<...>");
                    break;

                case NODEKIND.OPENNAME:
                    ErrAppendName((name as OPENNAMENODE).Name);
                    ErrAppendChar('<');
                    for (int carg = (name as OPENNAMENODE).CountOfBlankParameters; --carg > 0; )
                    {
                        ErrAppendChar(',');
                    }
                    ErrAppendChar('>');
                    break;

                case NODEKIND.DOT:
                    {
                        // now, find the first name:
                        BASENODE first = name.AsDOT.Operand1;
                        while (first.Kind == NODEKIND.DOT)
                        {
                            first = first.AsDOT.Operand1;
                        }

                        // add the first name, unless this is a fully qualified name
                        if (first.AsANYNAME.Name != this.NameManager.GetPredefinedName(PREDEFNAME.EMPTY))
                        {
                            ErrAppendNameNode(first.AsANYNAME);
                        }

                        // add the remaining names
                        do
                        {
                            // loop until we add all the names
                            first = first.ParentNode;
                            DebugUtil.Assert(first.Kind == NODEKIND.DOT && first.AsDOT.Operand2.IsAnyName);

                            if (first.IsDoubleColon)
                            {
                                ErrAppendString("::");
                            }
                            else
                            {
                                ErrAppendChar('.');
                            }
                            ErrAppendNameNode(first.AsDOT.Operand2);

                            // is this the rightmost name?
                        } while (first != name);
                    }
                    break;

                default:
                    DebugUtil.VsFail("Bad node");
                    break;
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendParamList
        //
        /// <summary>
        /// Create a fill-in string describing a parameter list.
        /// Does NOT include ()
        /// </summary>
        /// <param name="types"></param>
        /// <param name="isVarargs"></param>
        /// <param name="isParamArray"></param>
        //------------------------------------------------------------
        internal void ErrAppendParamList(TypeArray types, bool isVarargs, bool isParamArray)
        {
            if (types == null)
            {
                return;
            }

            for (int i = 0; i < types.Count; i++)
            {
                if (i > 0)
                {
                    ErrAppendString(", ");
                }

                if (isParamArray && i == types.Count - 1)
                {
                    ErrAppendString("types ");
                }

                // parameter type name
                ErrAppendSym(types[i], null, true);
            }

            if (isVarargs)
            {
                if (types.Count != 0)
                {
                    ErrAppendString(", ");
                }
                ErrAppendString("...");
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendTypeNode
        //
        /// <summary></summary>
        /// <param name="type"></param>
        //------------------------------------------------------------
        internal void ErrAppendTypeNode(TYPEBASENODE type)
        {
            switch (type.Kind)
            {
                case NODEKIND.PREDEFINEDTYPE:
                    // NOTE: we may not have predefined types installed when we call this
                    ErrAppendString(BSYMMGR.GetNiceName((PREDEFTYPE)(type as PREDEFINEDTYPENODE).Type));
                    break;

                case NODEKIND.NAMEDTYPE:
                    ErrAppendNameNode((type as NAMEDTYPENODE).NameNode);
                    break;

                case NODEKIND.OPENTYPE:
                    ErrAppendNameNode(type.AsOPENTYPE.NameNode);
                    break;

                case NODEKIND.ARRAYTYPE:
                    ErrAppendTypeNode((type as ARRAYTYPENODE).ElementTypeNode);
                    ErrAppendChar('[');
                    for (int i = (type as ARRAYTYPENODE).Dimensions; --i > 0; )
                    {
                        ErrAppendChar(',');
                    }
                    ErrAppendChar(']');
                    break;

                case NODEKIND.POINTERTYPE:
                    ErrAppendTypeNode((type as POINTERTYPENODE).ElementTypeNode);
                    ErrAppendChar('*');
                    break;

                case NODEKIND.NULLABLETYPE:
                    //ErrAppendTypeNode(type.asNULLABLETYPE().pElementType);
                    ErrAppendChar('?');
                    break;

                default:
                    DebugUtil.Assert(false, "NYI. Handle other type node types");
                    break;
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrStrWithLoc
        //
        /// <summary>
        /// <para>strBase に位置情報（"[&lt;マップファイル名&gt;(&lt;行&gt;)]" の書式）を追加して返す。</para>
        /// <para>情報が足りない場合は strBase だけをそのまま返す。</para>
        /// </summary>
        /// <param name="strBase"></param>
        /// <param name="sym"></param>
        /// <param name="node"></param>
        /// <param name="isSource"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrStrWithLoc(string strBase, SYM sym, BASENODE node, out bool isSource)
        {
            isSource = false;
            ERRLOC loc;

            if (node == null)
            {
                if (sym == null)
                {
                    return strBase;
                }
                node = sym.GetSomeParseTree();
            }

            if (node == null)
            {
                INFILESYM infile = sym.GetSomeInputFile();
                if (infile == null)
                {
                    return strBase;
                }
                // このコンストラクタで作成した場合は、位置情報が設定されない。
                loc = new ERRLOC(infile, this.OptionManager.FullPaths);
            }
            else
            {
                // このコンストラクタで作成した場合は、位置情報が設定される。
                loc = new ERRLOC(MainSymbolManager, node, OptionManager.FullPaths);
                isSource = true;
            }

            int start = ErrorBufferStart;
            ErrAppendString(strBase);
            ErrAppendString(" [");
            ErrAppendString(loc.SourceMapFileName);
            if (loc.MappedLineIndex >= 0)
            {
                ErrAppendPrintf("{0:d}", loc.MappedLineIndex + 1);
            }
            ErrAppendChar(']');
            return FlushErrorBuffer(start);
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendParentSym
        //
        /// <summary>
        /// <para>Add a string representing the parent sym to COMPILER.errorBuffer.</para>
        /// <para>If tha parent is the global namespace, add none.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="context"></param>
        //------------------------------------------------------------
        internal void ErrAppendParentSym(SYM sym, SubstContext context)
        {
            if (sym == null)
            {
                return;
            }
            PARENTSYM parent = sym.ParentSym;
            if (parent == null)
            {
                return;
            }

            if (parent.IsNSAIDSYM)
            {
                int aid = (parent as NSAIDSYM).GetAssemblyID();
                parent = (parent as NSAIDSYM).NamespaceSym;

                if (aid != Kaid.Global)
                {
                    // Spit out the alias name
                    ErrAppendSym(MainSymbolManager.GetSymForAid(aid), null, true);
                    ErrAppendChar(':');
                    ErrAppendChar(':');
                }
            }

            if (parent == MainSymbolManager.RootNamespaceSym)
            {
                return;
            }

            if (context != null && !context.FNop())
            {
                AGGSYM parentAggSym = parent as AGGSYM;
                if (parentAggSym != null && parentAggSym.AllTypeVariables.Count != 0)
                {
                    parent = MainSymbolManager.SubstType((parent as AGGSYM).GetThisType(), context);
                }
            }
            ErrAppendSym(parent, null, true);
            ErrAppendChar('.');
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendMethodParentSym
        //
        /// <summary>
        /// substMethTyParams を null を代入してから ErrAppendParentSym を呼び出す。
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="context"></param>
        /// <param name="substMethTyParams"></param>
        //------------------------------------------------------------
        internal void ErrAppendMethodParentSym(
            METHSYM sym,
            SubstContext context,
            out TypeArray substMethTyParams)
        {
            substMethTyParams = null;
            ErrAppendParentSym(sym, context);
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendTypeParameters
        //
        /// <summary>
        /// Add a string representing a list of type parameters to COMPILER.errorBuffer.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="context"></param>
        /// <param name="forClass"></param>
        //------------------------------------------------------------
        internal void ErrAppendTypeParameters(TypeArray types, SubstContext context, bool forClass)
        {
            if (types != null && types.Count != 0)
            {
                ErrAppendChar('<');
                ErrAppendSym(types[0], context, true);
                for (int i = 1; i < types.Count; i++)
                {
                    ErrAppendString(",");
                    ErrAppendSym(types[i], context, true);
                }
                ErrAppendChar('>');
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendMethod
        //
        /// <summary>
        /// メソッドを表す文字列を errorBuffer に追加する。
        /// </summary>
        /// <param name="methSym"></param>
        /// <param name="context"></param>
        /// <param name="fArgs"></param>
        //------------------------------------------------------------
        internal void ErrAppendMethod(METHSYM methSym, SubstContext context, bool fArgs)
        {
            SYM sym = null;
            AGGTYPESYM aggSymTypeSym = null;

            //--------------------------------------------------
            // Explicit Implementation の場合
            //--------------------------------------------------
            if (methSym.IsExplicitImplementation && methSym.SlotSymWithType != null)
            {
                ErrAppendParentSym(methSym, context);

                // Get the type args from the explicit impl type and substitute using context (if there is one).
                // このメソッドを実装しているインターフェースに型引数を代入する。
                sym = MainSymbolManager.SubstType(methSym.SlotSymWithType.AggTypeSym, context);
                if (sym != null)
                {
                    aggSymTypeSym = sym as AGGTYPESYM;
                }
                DebugUtil.Assert(aggSymTypeSym != null);

                // 代入した状態で SubstContext を作成する。
                SubstContext ctx = new SubstContext(sym as AGGTYPESYM, null, SubstTypeFlagsEnum.NormNone);
                // 元のメソッド（代入後）を ErrAppendSym で処理する。
                ErrAppendSym(methSym.SlotSymWithType.Sym, ctx, fArgs);

                // args already added
                return;
            }

            //--------------------------------------------------
            // Accessor of properties
            //
            // A string of a sym, followd by string ".get" or ".set".
            //--------------------------------------------------
            if (methSym.IsPropertyAccessor)
            {
                PROPSYM propSym = methSym.PropertySym;

                // this includes the parent class
                ErrAppendSym(propSym, context, true);

                // add accessor name
                if (propSym.GetMethodSym == methSym)
                {
                    ErrAppendString(".get");
                }
                else
                {
                    DebugUtil.Assert(propSym.SetMethodSym == methSym);
                    ErrAppendString(".set");
                }
                // args already added
                return;
            }

            //--------------------------------------------------
            // イベントのアクセッサ
            //--------------------------------------------------
            if (methSym.IsEventAccessor)
            {
                EVENTSYM eventSym = methSym.EventSym;

                // this includes the parent class
                ErrAppendSym(eventSym, context, true);

                // add accessor name
                if (eventSym.AddMethodSym == methSym)
                {
                    ErrAppendString(".add");
                }
                else
                {
                    DebugUtil.Assert(eventSym.RemoveMethodSym == methSym);
                    ErrAppendString(".remove");
                }
                // args already added
                return;
            }

            TypeArray replacementTypeArray = null;
            ErrAppendMethodParentSym(methSym, context, out replacementTypeArray);

            //--------------------------------------------------
            // コンストラクタ、デストラクタの場合は、クラス名を追加する。
            //--------------------------------------------------
            if (methSym.IsCtor)
            {
                // Use the name of the parent class instead of the name "<ctor>".
                ErrAppendName(methSym.ClassSym.Name);
            }
            else if (methSym.IsDtor)
            {
                // Use the name of the parent class instead of the name "Finalize".
                ErrAppendChar('~');
                ErrAppendName(methSym.ClassSym.Name);
            }
            //--------------------------------------------------
            // 型変換の場合、"implicit" か "explicit"、"operator"、変換先の型名。
            //--------------------------------------------------
            else if (methSym.IsConversionOperator)
            {
                // implicit/explicit
                ErrAppendString(methSym.IsImplicit ? "implicit" : "explicit");
                ErrAppendString(" operator ");

                // destination type name
                ErrAppendSym(methSym.ReturnTypeSym, context, true);
            }
            //--------------------------------------------------
            // 演算子のオーバーロード
            //--------------------------------------------------
            else if (methSym.IsOperator)
            {
                // handle user defined operators
                // map from CLS predefined names to "operator <X>"
                ErrAppendString("operator ");

                //
                //
                string operatorName = CParser.GetTokenInfo(
                    (TOKENID)CParser.GetOperatorInfo(ClsDeclRec.OperatorOfName(methSym.Name)).TokenID).Text;
                if (operatorName[0] == 0)
                {
                    //
                    // either equals or compare
                    //
                    if (methSym.Name == this.NameManager.GetPredefinedName(PREDEFNAME.OPEQUALS))
                    {
                        operatorName = "equals";
                    }
                    else
                    {
                        //ASSERT(meth.name == namemgr.GetPredefName(PN_OPCOMPARE));
                        operatorName = "compare";
                    }
                }
                ErrAppendString(operatorName);
            }
            //--------------------------------------------------
            // Explicit Implementation
            //--------------------------------------------------
            else if (methSym.IsExplicitImplementation)
            {
                if (methSym.ExpImplErrorSym != null)
                {
                    ErrAppendSym(methSym.ExpImplErrorSym, context, fArgs);
                }
                else
                {
                    // explicit impl that hasn't been prepared yet
                    // can't be a property accessor
                    ErrAppendNameNode((methSym.ParseTreeNode as METHODNODE).NameNode);
                }
            }
            //--------------------------------------------------
            // regular method
            //--------------------------------------------------
            else
            {
                ErrAppendName(methSym.Name);
            }

            if (replacementTypeArray == null)
            {
                ErrAppendTypeParameters(methSym.TypeVariables, context, false);
            }

            //--------------------------------------------------
            // append argument types
            //--------------------------------------------------
            if (fArgs)
            {
                ErrAppendChar('(');

                if (!CheckBogusNoEnsure(methSym))
                {
                    ErrAppendParamList(
                        MainSymbolManager.SubstTypeArray(methSym.ParameterTypes, context),
                        methSym.IsVarargs,
                        methSym.IsParameterArray);
                }

                ErrAppendChar(')');
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendIndexer
        //
        /// <summary></summary>
        /// <param name="indexer"></param>
        /// <param name="pctx"></param>
        //------------------------------------------------------------
        internal void ErrAppendIndexer(INDEXERSYM indexer, SubstContext pctx)
        {
            ErrAppendString("this[");
            ErrAppendParamList(
                MainSymbolManager.SubstTypeArray(indexer.ParameterTypes, pctx),
                false,
                indexer.IsParameterArray);
            ErrAppendChar(']');
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendProperty
        //
        /// <summary></summary>
        /// <param name="propSym"></param>
        /// <param name="context"></param>
        //------------------------------------------------------------
        internal void ErrAppendProperty(PROPSYM propSym, SubstContext context)
        {
            // 親 Sym の情報を errorBuffer に追加する。
            ErrAppendParentSym(propSym, context);

            // Explicit Implementation で実装元の情報がある場合。
            if (propSym.IsExplicitImplementation && propSym.SlotSymWithType.Sym != null)
            {
                SubstContext ctx = new SubstContext(
                    MainSymbolManager.SubstType(propSym.SlotSymWithType.AggTypeSym, context) as AGGTYPESYM,
                    null,
                    SubstTypeFlagsEnum.NormNone);
                ErrAppendSym(propSym.SlotSymWithType.Sym, ctx, true);
            }
            // Explicit Implementation で実装元の情報がない場合。
            else if (propSym.IsExplicitImplementation)
            {
                if (propSym.ExpImplErrorSym != null)
                {
                    ErrAppendSym(propSym.ExpImplErrorSym, context, false);
                }
                else
                {
                    // must be explicit impl not prepared yet.
                    ErrAppendNameNode(propSym.ParseTreeNode.AsANYPROPERTY.NameNode);
                }
                if (propSym.IsIndexer)
                {
                    ErrAppendChar('.');
                    ErrAppendIndexer(propSym.AsINDEXERSYM, context);
                }
            }
            else if (propSym.IsIndexer)
            {
                ErrAppendIndexer(propSym.AsINDEXERSYM, context);
            }
            else
            {
                ErrAppendName(propSym.Name);
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrAppendEvent
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="context"></param>
        //------------------------------------------------------------
        internal void ErrAppendEvent(EVENTSYM sym, SubstContext context)
        {
            // Qualify with parent symbol, if any.
            ErrAppendParentSym(sym, context);

            if (sym.SlotEventWithType.Sym != null && sym.IsExpImpl)
            {
                SubstContext ctx = new SubstContext(
                    MainSymbolManager.SubstType(sym.SlotEventWithType.AggTypeSym, context) as AGGTYPESYM,
                    null,
                    SubstTypeFlagsEnum.NormNone);
                ErrAppendSym(sym.SlotEventWithType.EventSym, ctx, true);
            }
            else if (sym.IsExpImpl)
            {
                // must be explicit impl not prepared yet.
                ErrAppendNameNode((sym.ParseTreeNode as PROPERTYNODE).NameNode);
            }
            else
            {
                ErrAppendName(sym.Name);
            }
        }

        //------------------------------------------------------------
        // COMPILER.ErrId (1)
        //
        /// <summary>
        /// <para>Convert id to a string and return it.</para>
        /// <para>In sscli, create a error message by Win32API.</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrId(CSCERRID id)
        {
            return id.ToString();
        }

        //------------------------------------------------------------
        // COMPILER.ErrId (2)
        //
        /// <summary>
        /// <para>Convert id to a string and return it.</para>
        /// <para>In sscli, create a error message by Win32API.</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrId(ResNo resNo)
        {
            string str = null;
            Exception excp = null;
            if (CResources.GetString(resNo, out str, out excp))
            {
                return str;
            }
            return String.Format("{0}", resNo);
        }

        //------------------------------------------------------------
        // COMPILER.ErrSym
        //
        /// <summary>
        /// <para>Call ErrorAppendSym to create error messages in errorBuffer, and
        /// return the created message.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="context"></param>
        /// <param name="fArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrSym(
            SYM sym,
            SubstContext context,   // = NULL,
            bool fArgs)              // = true
        {
            int start = ErrorBufferStart;
            ErrAppendSym(sym, context, fArgs);
            return FlushErrorBuffer(start);
        }

        //------------------------------------------------------------
        // COMPILER.ErrName
        //
        /// <summary>
        /// <para>For now, just return the text of the name...</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrName(string name)
        {
            int start = ErrorBufferStart;
            ErrAppendName(name);
            return FlushErrorBuffer(start);
        }

        //------------------------------------------------------------
        // COMPILER.ErrNameNode
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrNameNode(BASENODE node)
        {
            int start = ErrorBufferStart;
            ErrAppendNameNode(node);
            return FlushErrorBuffer(start);
        }

        //------------------------------------------------------------
        // COMPILER.ErrParamList
        //------------------------------------------------------------
        //PCWSTR ErrParamList(TypeArray *params, bool isVarargs, bool isParamArray);
        //PCWSTR COMPILER::ErrParamList(TypeArray *params, bool isVarargs, bool isParamArray)
        //{
        //    START_ERR_STRING(this);
        //    ErrAppendParamList(params, isVarargs, isParamArray);
        //    return END_ERR_STRING(this);
        //}

        // The following routines create "fill-in" strings to be used as insertion
        // strings in an error message. They allocate memory from a single static
        // buffer used for the purpose, and freed when the error message is
        // reported.
        // The buffer initially reserves 2MB worth of memory, but only commits
        // 1 page at a time.  It commits new pages as needed.  The
        // LoadAndFormat routines used to actually create the error message are
        // try a 4K buffer, a 32K buffer, and then a 2MB buffer.
        // Although ErrBufferLast does tell how much more memory can be used/allocated
        // We specifically don't ever commit the last page, so that most of this
        // code does not have to do accurate buffer-length calculations.
        // If the user really does have a 2MB error message, than an ICE is
        // acceptable.

        //------------------------------------------------------------
        // COMPILER.ErrHR
        //
        /// <summary>
        /// <para>Create a fill-in string describing an HRESULT.</para>
        /// <para>In this project, we do not use HRESULT value, so do not call.</para>
        /// </summary>
        //------------------------------------------------------------
        internal string ErrHR(bool br, bool useGetErrorInfo)
        //PCWSTR COMPILER::ErrHR(HRESULT hr, bool useGetErrorInfo)
        {
            throw new NotImplementedException("COMPILER.ErrHR");
        }

        //PCWSTR ErrGetLastError();

        //------------------------------------------------------------
        // COMPILER.ErrDelegate
        //------------------------------------------------------------
        //PCWSTR ErrDelegate(AGGTYPESYM * type);
        ///*
        // * Create a fill-in string describing a delegate.
        // * Different from ErrSym because this one also adds the return type and the arguments
        // */
        //PCWSTR COMPILER::ErrDelegate(AGGTYPESYM * type)
        //{
        //    VERIFYLOCALHANDLER;
        //
        //    ASSERT(type.isDelegateType());
        //    START_ERR_STRING(this);
        //
        //    METHSYM * pInvoke = getMainSymbolManager().LookupInvokeMeth(type.getAggregate());
        //    if (!pInvoke) {
        //        // We can't find the Invoke method, so fall-back to reporting the plain delegate name
        //        ErrAppendSym(type, NULL);
        //    } else {
        //        SubstContext ctx(type);
        //
        //        // return type
        //        ErrAppendSym(pInvoke.retType, &ctx);
        //        ErrAppendChar(L' ');
        //
        //        // Delegate Name
        //        ErrAppendSym(type, NULL);
        //
        //        // Parameter list
        //        ErrAppendChar(L'(');
        //        if (pInvoke.hasBogus() && !pInvoke.checkBogus()) {
        //            ErrAppendParamList(getMainSymbolManager().SubstTypeArray(pInvoke.params, &ctx), pInvoke.isVarargs, pInvoke.isParamArray);
        //        }
        //        ErrAppendChar(L')');
        //    }
        //
        //    return END_ERR_STRING(this);
        //}

        //------------------------------------------------------------
        // COMPILER.ErrSK
        //
        /// <summary>
        /// SYMKIND 値に対してそれを表す文字列を返す。
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrSK(SYMKIND sk)
        {
            ResNo resNo;
            string str;

            switch (sk)
            {
                case SYMKIND.METHSYM:
                    resNo = ResNo.CSCSTR_SK_METHOD; // CSCSTRID.SK_METHOD;
                    break;

                case SYMKIND.NUBSYM:
                    resNo = ResNo.CSCSTR_SK_CLASS; // CSCSTRID.SK_CLASS;
                    break;

                case SYMKIND.AGGSYM:
                    resNo = ResNo.CSCSTR_SK_CLASS; // CSCSTRID.SK_CLASS;
                    break;

                case SYMKIND.AGGTYPESYM:
                    resNo = ResNo.CSCSTR_SK_CLASS; // CSCSTRID.SK_CLASS;
                    break;

                case SYMKIND.NSSYM:
                    resNo = ResNo.CSCSTR_SK_NAMESPACE; // CSCSTRID.SK_NAMESPACE;
                    break;

                case SYMKIND.MEMBVARSYM:
                    resNo = ResNo.CSCSTR_SK_FIELD; // CSCSTRID.SK_FIELD;
                    break;

                case SYMKIND.LOCVARSYM:
                    resNo = ResNo.CSCSTR_SK_VARIABLE; // CSCSTRID.SK_VARIABLE;
                    break;

                case SYMKIND.PROPSYM:
                    resNo = ResNo.CSCSTR_SK_PROPERTY; // CSCSTRID.SK_PROPERTY;
                    break;

                case SYMKIND.EVENTSYM:
                    resNo = ResNo.CSCSTR_SK_EVENT; // CSCSTRID.SK_EVENT;
                    break;

                case SYMKIND.TYVARSYM:
                    resNo = ResNo.CSCSTR_SK_TYVAR; // CSCSTRID.SK_TYVAR;
                    break;

                case SYMKIND.ALIASSYM:
                    resNo = ResNo.CSCSTR_SK_ALIAS; // CSCSTRID.SK_ALIAS;
                    break;

                case SYMKIND.EXTERNALIASSYM:
                    resNo = ResNo.CSCSTR_SK_EXTERNALIAS; // CSCSTRID.SK_EXTERNALIAS;
                    break;

                case SYMKIND.NSAIDSYM:
                    DebugUtil.VsFail("Illegal sk");
                    resNo = ResNo.CSCSTR_SK_ALIAS; // CSCSTRID.SK_ALIAS;
                    break;

                default:
                    DebugUtil.VsFail("impossible sk");
                    resNo = ResNo.CSCSTR_AK_UNKNOWN; // CSCSTRID.SK_UNKNOWN;
                    break;
            }

            Exception excp = null;
            if (CResources.GetString(resNo, out str, out excp))
            {
                return str;
            }
            DebugUtil.Assert(false);
            return null;
        }

        //------------------------------------------------------------
        // COMPILER.ErrAggKind
        //
        /// <summary>
        /// AggKindEnum 値に対してそれを表す文字列を返す。
        /// </summary>
        /// <param name="ak"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrAggKind(AggKindEnum ak)
        {
            ResNo resNo;

            switch (ak)
            {
                case AggKindEnum.Class:
                    resNo = ResNo.CSCSTR_AK_CLASS; // CSCSTRID.AK_CLASS;
                    break;

                case AggKindEnum.Delegate:
                    resNo = ResNo.CSCSTR_AK_DELEGATE; // CSCSTRID.AK_DELEGATE;
                    break;

                case AggKindEnum.Interface:
                    resNo = ResNo.CSCSTR_AK_INTERFACE; // CSCSTRID.AK_INTERFACE;
                    break;

                case AggKindEnum.Struct:
                    resNo = ResNo.CSCSTR_AK_STRUCT; // CSCSTRID.AK_STRUCT;
                    break;

                case AggKindEnum.Enum:
                    resNo = ResNo.CSCSTR_AK_ENUM; // CSCSTRID.AK_ENUM;
                    break;

                default:
                    //VSFAIL("impossible AggKind");
                    resNo = ResNo.CSCSTR_AK_UNKNOWN; // CSCSTRID.AK_UNKNOWN;
                    break;
            }
            return ErrId(resNo);
        }

        //------------------------------------------------------------
        // COMPILER.ErrTypeNode
        //
        /// <summary>
        /// Create a fill-in string describing a parsed type node
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note: currently only works for predefined types, and named types
        ///       does not work for arrays, pointers, etc.
        /// </remarks>
        //------------------------------------------------------------
        internal string ErrTypeNode(TYPEBASENODE type)
        {
            int start = ErrorBufferStart;
            ErrAppendTypeNode(type);
            return FlushErrorBuffer(start);
        }

        //------------------------------------------------------------
        // COMPILER.ErrAccess
        //
        /// <summary>
        /// For now, just return the text of the access modifier
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ErrAccess(ACCESS acc)
        {
            //VERIFYLOCALHANDLER; // #define VERIFYLOCALHANDLER DebugUtil.Assert(1) // csharp\sccomp\compiler.h (30)

            switch (acc)
            {
                case ACCESS.PUBLIC:
                    return CParser.GetTokenInfo(TOKENID.PUBLIC).Text;

                case ACCESS.PROTECTED:
                    return CParser.GetTokenInfo(TOKENID.PROTECTED).Text;

                case ACCESS.INTERNAL:
                    return CParser.GetTokenInfo(TOKENID.INTERNAL).Text;

                case ACCESS.INTERNALPROTECTED:
                    {
                        //START_ERR_STRING(this);
                        //
                        //ErrAppendString(CParser.GetTokenInfo( TOKENID.PROTECTED).Text);
                        //ErrAppendChar(' ');
                        //ErrAppendString(CParser.GetTokenInfo( TOKENID.INTERNAL).Text);
                        //
                        //return END_ERR_STRING(this);

                        StringBuilder sb = new StringBuilder();
                        sb.Append(CParser.GetTokenInfo(TOKENID.PROTECTED).Text);
                        sb.Append(' ');
                        sb.Append(CParser.GetTokenInfo(TOKENID.INTERNAL).Text);
                        return sb.ToString();
                    }

                case ACCESS.PRIVATE:
                    return CParser.GetTokenInfo(TOKENID.PRIVATE).Text;

                default:
                    DebugUtil.Assert(false, "Unknown access modifier");
                    return String.Empty;
            }
        }

        //------------------------------------------------------------
        // COMPILER.NotifyHostOfBinaryFile
        //
        /// <summary>
        /// Send a filename to CCompilerHost.NotifyBinaryFile method
        /// which does nothing.
        /// </summary>
        /// <param name="filename"></param>
        //------------------------------------------------------------
        internal void NotifyHostOfBinaryFile(string filename)
        {
            //if (this.CommandLineCompilerHost != null)
            //{
            //    this.CommandLineCompilerHost.NotifyBinaryFile(filename);
            //}
        }

        //------------------------------------------------------------
        // COMPILER.NotifyHostOfMetadataFile
        //
        /// <summary></summary>
        /// <param name="filename"></param>
        //------------------------------------------------------------
        internal void NotifyHostOfMetadataFile(string filename)
        {
            // In sscli,
            // calls ICSCommandLineCompilerHost::NotifyMetadataFile.
            // CompilerHost::NotifyMetadataFile calls CompilerHost::NotifyBinaryFile.
            // CompilerHost::NotifyBinaryFile calls ConsoleOutput::OutputBinaryFileToRepro.
            // ConsoleOutput::OutputBinaryFileToRepro does nothing.
        }

        //------------------------------------------------------------
        // COMPILER.FAbortEarly
        //
        /// <summary>
        /// <para>Determine if compiling must be canceled.</para>
        /// <para>If the count of reported errors does not equal to argument prevCount, return true.
        /// Otherwise, return this.isCanceled.</para>
        /// </summary>
        /// <param name="prevCount"></param>
        /// <param name="pes">Do not use for now.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FAbortEarly(
            int prevCount,
            CErrorSuppression pes)  // = NULL
        {
            if (prevCount == ErrorCount())
            {
                return this.isCanceled;
            }
            return true;
        }

        //------------------------------------------------------------
        // COMPILER.FAbortCodeGen
        //
        /// <summary></summary>
        /// <param name="prevCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FAbortCodeGen(int prevCount)
        {
            return prevCount != ErrorCount();
        }

        //------------------------------------------------------------
        // COMPILER.FAbortOutFile
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FAbortOutFile()
        {
            //return pController->ErrorsReported() && !options.m_fCompileSkeleton;
            return Controller.CountOfReportedErrors != 0;
        }

        //------------------------------------------------------------
        // COMPILER.ErrorCount
        //
        /// <summary>
        /// Return the count of reported errors, excluding warnings reported as error.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int ErrorCount()
        {
            return (Controller.CountOfReportedErrors - Controller.CountOfWarnsReportedAsErrors);
        }

        //------------------------------------------------------------
        // COMPILER.GetCheckedMode
        //
        /// <summary>
        /// Return COMPILER.Options.Checked.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetCheckedMode()
        {
            return this.OptionManager.CheckOverflow;  // for now...
        }

        //------------------------------------------------------------
        // COMPILER.Compile
        //
        /// <summary>
        /// <para>Do the compilation.</para>
        /// <para>This method calls CompileAll method to compile and handles exceptions. </para>
        /// </summary>
        /// <param name="progressSink"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Compile(CCompileProgress progressSink)
        {
            bool br = false;

            try
            {
                // Do compilation here.
                CompileAll(progressSink);
                br = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#if DEBUG
                string str = ex.ToString();
#endif
            }
            finally
            {
                br = CleanUp(br);
            }
            return br;
        }

        //------------------------------------------------------------
        // COMPILER.ReportCompileProgress
        //
        /// <summary></summary>
        /// <param name="progressSink"></param>
        /// <param name="iFile"></param>
        /// <param name="iFileCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ReportCompileProgress(
            CCompileProgress progressSink,  // ICSCompileProgress progressSink,
            int iFile,
            int iFileCount)
        {
            return ReportProgress(progressSink, COMPILE_PHASE_ENUM.COMPILE, iFile, iFileCount);
        }

        //// ALLOCHOST methods
        //_declspec(noreturn) void NoMemory () { Error (NULL, FTL_NoMemory); ALLOCHOST::ThrowOutOfMemoryException(); }
        //MEMHEAP     *GetStandardHeap () { return &globalHeap; }
        //PAGEHEAP    *GetPageHeap () { return &pageheap; }

        //// ISTHOST methods

        //internal CNameManager getNameManager() { return NameManager; }

        //------------------------------------------------------------
        // COMPILER.PrepareAggregate
        //
        /// <summary></summary>
        /// <param name="cls"></param>
        //------------------------------------------------------------
        internal void PrepareAggregate(AGGSYM cls)
        {
            this.ClsDeclRec.PrepareAggregate(cls);
        }

        //NRHEAP * getLocalSymHeap() { return &localSymAlloc;}

        //------------------------------------------------------------
        // COMPILER.IsBuildingMSCORLIB
        //
        /// <summary>
        /// Return true if predefined type "object" is defined.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsBuildingMSCORLIB()
        {
            AGGSYM obj = GetReqPredefAgg(PREDEFTYPE.OBJECT, false);
            return obj.IsSource;
        }

        //------------------------------------------------------------
        // COMPILER.CheckForValidIdentifier
        //
        /// <summary></summary>
        /// <param name="str"></param>
        /// <param name="isPreprocessorString"></param>
        /// <param name="tree"></param>
        /// <param name="id"></param>
        /// <param name="errArg"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckForValidIdentifier(
            string str,
            bool isPreprocessorString,
            BASENODE tree,
            CSCERRID id,
            ErrArg errArg)
        {

            if (str != null &&
                this.NameManager.IsValidIdentifier(
                    str,
                    this.OptionManager.LangVersion,
                    (isPreprocessorString ?
                        CheckIdentifierFlagsEnum.Simple :
                        CheckIdentifierFlagsEnum.StandardSource)))
            {
                return true;
            }

            Error(tree, id, errArg);
            return false;
        }

        //
        // Miscellaneous.
        //

        //------------------------------------------------------------
        // COMPILER::GetMetadataDispenser (sscli)
        //
        // Get the metadata dispenser.
        //------------------------------------------------------------
        //IMetaDataDispenserEx * COMPILER::GetMetadataDispenser()
        //{
        //    if (! dispenser) {
        //        // Obtain the dispenser.
        //        HRESULT hr = E_FAIL;
        //        if (FAILED(hr = PAL_CoCreateInstance(CLSID_CorMetaDataDispenser,
        //                                         IID_IMetaDataDispenserEx,
        //                                         (LPVOID *) & dispenser)))
        //        {
        //            Error(NULL, FTL_ComPlusInit, ErrHR(hr));
        //        }
        //
        //        SetDispenserOptions();
        //
        //        if (FAILED(hr = linker->Init(dispenser, &alError)))
        //        {
        //            Error(NULL, FTL_ComPlusInit, ErrHR(hr));
        //        }
        //    }
        //
        //    return dispenser;
        //}

        //ITypeNameFactory * GetTypeNameFactory();
        //ITypeNameBuilder * GetTypeNameBuilder();
        //void ReleaseTypeNameBuilder() { m_cnttnbUsage--; }

        //------------------------------------------------------------
        // COMPILER.GetCorSystemDirectory
        //
        /// <summary>
        /// get the cor system directory
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal DirectoryInfo GetCorSystemDirectory()
        {
            if (corSystemDirectoryInfo == null)
            {
                Exception excp;
                if (!IOUtil.CreateDirectoryInfo(
                        Path.GetDirectoryName(Assembly.GetAssembly(typeof(Object)).Location),
                        out this.corSystemDirectoryInfo,
                        out excp))
                {
                    string msg = null;
                    if (excp != null)
                    {
                        msg = excp.Message;
                    }
                    else
                    {
                        msg = "internal compiler error";
                    }
                    corSystemDirectoryInfo = null;
                    Error(CSCERRID.ERR_CantGetCORSystemDir, new ErrArg(msg));
                }
            }
            return corSystemDirectoryInfo;
        }

        //------------------------------------------------------------
        // COMPILER.GetFirstOutFile
        //
        /// <summary>
        /// Return COMPILER.MainSymbolManager.FileRootSym.FirstChildSym as OUTFILESYM.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal OUTFILESYM GetFirstOutFile()
        {
            return (MainSymbolManager.FileRootSym.FirstChildSym as OUTFILESYM);
        }

        //------------------------------------------------------------
        // COMPILER.GetManifestOutFile
        //
        /// <summary>
        /// Return an OUTFILESYM instance whose IsManifest flag is set.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal OUTFILESYM GetManifestOutFile()
        {
            if (BuildAssembly)
            {
                for (OUTFILESYM outfile = GetFirstOutFile();
                    outfile != null;
                    outfile = outfile.NextOutputFile())
                {
                    if (outfile.IsManifest)
                    {
                        return outfile;
                    }
                }
                // BuildAssembly shouldn't be true if none of the outfiles is a manifest file.
                DebugUtil.Assert(false);
            }
            return null;
        }

        //------------------------------------------------------------
        // COMPILER.AddInputSet
        //
        /// <summary>
        /// <para>Call AddInputSetWorker method.</para>
        /// <para>Catch no exception in this method.</para>
        /// </summary>
        /// <param name="inputSet"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AddInputSet(CInputSet inputSet)
        {
            try
            {
                return AddInputSetWorker(inputSet);
            }
            catch (CSException ex)
            {
                Error(ERRORKIND.ERROR, ex);
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.AddInputSetWorker
        //
        /// <summary>
        /// <para>Register the input and output files of argument inputSet
        /// to GlobalSymbolManager.</para>
        /// <list type="bullet">
        /// <item>Create a OUTFILESYM of the output files of inputSet,
        /// and add to the child list of GlobalSymbolManager.FileRootSym.</item>
        /// <item>Create INFILESYMs of input files and resource files of inputSet,
        /// and Add to the child list of the OUTFILESYM.</item>
        /// </list>
        /// </summary>
        /// <param name="inputSet">CInputSet instance.</param>
        /// <returns>If inputSet has no input file and no output file, return false.</returns>
        //------------------------------------------------------------
        internal bool AddInputSetWorker(CInputSet inputSet)
        {
            OUTFILESYM outfileSym = null;
            INFILESYM infileSym = null;
            RESFILESYM resfileSym = null;

            // If inputSet has no input file and no output file, return false.

            if (inputSet.SourceFileDictionary.Count == 0 &&
                String.IsNullOrEmpty(inputSet.OutputFileName))
            {
                Error(CSCERRID.ERR_OutputNeedsName);
                return false;
            }

            //--------------------------------------------------------
            // Create the output file, and
            // Register it to GlobalSymbolManager.globalSymbolTable.
            // Add to the child list of GlobalSymbolManager.FileRootSym.
            //--------------------------------------------------------
            outfileSym = GlobalSymbolManager.CreateOutFile(
                inputSet.OutputFileName,
                inputSet.Target,
                inputSet.MainClassName,
                inputSet.Win32ResourceFileInfo,
                inputSet.Win32IconFileInfo,
                inputSet.PDBFileName);

            outfileSym.SetFileInfo(inputSet.OutputFileInfo);
            outfileSym.ImageBaseAddress = inputSet.ImageBaseAddress;
            outfileSym.FileAlignment = inputSet.FileAlignment;

            this.buildAssembly |= outfileSym.IsManifest;

            // Get a module id for this output file

            outfileSym.SetModuleID(MainSymbolManager.AllocateAssemblyID(outfileSym));

            //--------------------------------------------------------
            // create the input files
            //
            // Create INFILESYMs for each source files and resource files.
            // Register it to MainSymbolManager.globalSymbolTable.
            // Determine the parent SYM and Set the relationship.
            //
            // Someone must have changes the inputset asyncronously!
            //--------------------------------------------------------
            Dictionary<string, CSourceFileInfo> sourceFiles
                = inputSet.SourceFileDictionary.Dictionary;

            foreach (KeyValuePair<string, CSourceFileInfo> kv in sourceFiles)
            {
                infileSym = GlobalSymbolManager.CreateSourceFile(kv.Value.SearchName, outfileSym);
                if (infileSym != null)
                {
                    infileSym.SetFileInfo(kv.Value.FileInfo);
                }
            }

            // Someone must have changes the inputset asyncronously!

            //--------------------------------------------------------
            // Resource files
            //--------------------------------------------------------
            Dictionary<string, CResourceFileInfo> resourceFiles
                = inputSet.ResourceFileDictionary.Dictionary;

            foreach (KeyValuePair<string, CResourceFileInfo> kv in resourceFiles)
            {
                string logName = kv.Value.LogicalName;
                if (String.IsNullOrEmpty(logName))
                {
                    logName = kv.Value.FileInfo.Name;
                }

                if (kv.Value.Embed)
                {
                    // if Embed is true, the parent SYM is this.MainSymbolManager.mdfileroot.
                    resfileSym = GlobalSymbolManager.CreateEmbeddedResourceFile(
                        kv.Value.FileInfo,
                        logName,
                        kv.Value.Access);
                    if (resfileSym != null)
                    {
                        resfileSym.IsEmbedded = true;
                    }
                }
                else
                {
                    // Embed でない場合は、同じ名前の OUTFILESYM を作成しそれを親とする。
                    OUTFILESYM ofSym = GlobalSymbolManager.CreateOutFile(
                        kv.Value.SearchName,
                        TargetType.None,
                        null,
                        null,
                        null,
                        null);
                    ofSym.IsResource = true;

                    resfileSym = GlobalSymbolManager.CreateSeperateResourceFile(
                        kv.Value.FileInfo,
                        ofSym,
                        logName,
                        kv.Value.Access);
                    if (resfileSym != null)
                    {
                        resfileSym.IsEmbedded = false;
                    }
                }
                if (resfileSym == null)
                {
                    Error(
                        CSCERRID.ERR_ResourceNotUnique,
                        new ErrArg(kv.Value.LogicalName));
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // COMPILER.FindAndAddMetadataFile
        //
        /// <summary>
        /// Search the specified file in search paths.
        /// If found, create INFILESYM and register it to GlobalSymbolManager and
        /// set its parent to MainSymbolManager.MetadataFileRootSym.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="assemblyId"></param>
        /// <param name="useFullName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM FindAndAddMetadataFile(string file, int assemblyId, bool useFullName)
        {
            FileInfo fileInfo = null;
            INFILESYM sym = null;

            // If NoStdLib is not set,
            // AddStandardMetadata methods processes some metadata files.

            if (!this.OptionManager.NoStdLib)
            {
                string fileNameL = (Path.GetFileName(file)).ToLower();
                if (fileNameL == "mscorlib.dll" ||
                    fileNameL == "system.dll" ||
                    fileNameL == "system.core.dll")
                {
                    return null;
                }
            }

            if (SearchPathDual(GetSearchPath(), file, out fileInfo) ||
                FindSpecialMetadataFile(file, out fileInfo))
            {
                string name = (useFullName ? fileInfo.FullName : fileInfo.Name);

                if (name != null)
                {
                    sym = AddOneMetadataFile(name, assemblyId);
                    sym.SetFileInfo(fileInfo);
                    return sym;
                }
            }

            // Error handling

            if (fileInfo != null && (int)fileInfo.Attributes != -1 &&
                (fileInfo.Attributes & FileAttributes.Directory) != 0)
            {
                Error(CSCERRID.ERR_CantIncludeDirectory, new ErrArg(file));
            }
            else
            {
                Error(CSCERRID.ERR_NoMetadataFile, new ErrArg(file));
            }

            return null;
        }

        //------------------------------------------------------------
        // COMPILER.FindSpecialMetadataFile
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <remarks>2016/04/23 hirano567@hotmail.co.jp</remarks>
        //------------------------------------------------------------
        private bool FindSpecialMetadataFile(string fileName, out FileInfo fileInfo)
        {
            fileInfo = null;
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }

            Assembly asm = null;

            switch (fileName.ToLower())
            {
                case "accessibility.dll":
                    asm = Assembly.GetAssembly(typeof(Accessibility.IAccessible));
                    break;

                case "microsoft.csharp.dll":
                    asm = Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.Binder));
                    break;

#if false
                case "microsoft.vsa.dll":
                    asm = Assembly.GetAssembly(typeof(Microsoft.Vsa.IVsaEngine));
                    break;
#endif
                case "system.configuration.dll":
                    asm = Assembly.GetAssembly(typeof(System.Configuration.Configuration));
                    break;

                case "system.configuration.install.dll":
                    asm = Assembly.GetAssembly(typeof(System.Configuration.Install.Installer));
                    break;

                case "system.core.dll":
                    asm = Assembly.GetAssembly(typeof(System.Linq.Enumerable));   // System.Core.dll
                    break;

                case "system.data.dll":
                    asm = Assembly.GetAssembly(typeof(System.Data.DataTable));
                    break;

                case "system.data.datasetextensions.dll":
                    asm = Assembly.GetAssembly(typeof(System.Data.DataTableExtensions));
                    break;

                case "system.data.linq.dll":
                    asm = Assembly.GetAssembly(typeof(System.Data.Linq.DataContext));
                    break;

                case "system.data.oracleclient.dll":
                    asm = Assembly.GetAssembly(typeof(System.Data.OracleClient.OracleConnection));
                    break;

                case "system.deployment.dll":
                    asm = Assembly.GetAssembly(typeof(System.Deployment.Application.DeploymentServiceCom));
                    break;

                case "system.design.dll":
                    break;

                case "system.directoryservices.dll":
                    asm = Assembly.GetAssembly(typeof(System.DirectoryServices.DirectoryEntry));
                    break;

                case "system.dll":
                    asm = Assembly.GetAssembly(typeof(System.Text.RegularExpressions.Regex)); // System.dll
                    break;

                case "system.drawing.design.dll":
                    asm = Assembly.GetAssembly(typeof(System.Drawing.Design.BitmapEditor));
                    break;

                case "system.drawing.dll":
                    asm = Assembly.GetAssembly(typeof(System.Drawing.Bitmap));
                    break;

                case "system.enterpriseservices.dll":
                    asm = Assembly.GetAssembly(typeof(System.EnterpriseServices.ServiceDomain));
                    break;

                case "system.management.dll":
                    asm = Assembly.GetAssembly(typeof(System.Management.ManagementClass));
                    break;

                case "system.messaging.dll":
                    asm = Assembly.GetAssembly(typeof(System.Messaging.Message));
                    break;

                case "system.runtime.remoting.dll":
                    break;

                case "system.runtime.serialization.dll":
                    asm = Assembly.GetAssembly(typeof(System.Runtime.Serialization.XmlObjectSerializer));
                    break;

                case "system.runtime.serialization.formatters.soap.dll":
                    asm = Assembly.GetAssembly(typeof(System.Runtime.Serialization.Formatters.Soap.SoapFormatter));
                    break;

                case "system.security.dll":
                    break;

                case "system.servicemodel.dll":
                    asm = Assembly.GetAssembly(typeof(System.ServiceModel.ServiceHost));
                    break;

                case "system.servicemodel.web.dll":
                    asm = Assembly.GetAssembly(typeof(System.ServiceModel.Web.WebServiceHost));
                    break;

                case "system.serviceprocess.dll":
                    asm = Assembly.GetAssembly(typeof(System.ServiceProcess.ServiceController));
                    break;

                case "system.transactions.dll":
                    asm = Assembly.GetAssembly(typeof(System.Transactions.Transaction));
                    break;

                case "system.web.dll":
                    asm = Assembly.GetAssembly(typeof(System.Web.HttpApplication));
                    break;

                case "system.web.extensions.design.dll":
                    break;

                case "system.web.extensions.dll":
                    break;

                case "system.web.mobile.dll":
                    asm = Assembly.GetAssembly(typeof(System.Web.Mobile.MobileCapabilities));
                    break;

                case "system.web.regularexpressions.dll":
                    asm = Assembly.GetAssembly(typeof(System.Web.RegularExpressions.TagRegex));
                    break;

                case "system.web.services.dll":
                    asm = Assembly.GetAssembly(typeof(System.Web.Services.WebService));
                    break;

                case "system.windows.forms.dll":
                    asm = Assembly.GetAssembly(typeof(System.Windows.Forms.Form));
                    break;

                case "system.workflow.activities.dll":
                    asm = Assembly.GetAssembly(typeof(System.Workflow.Activities.WorkflowRole));
                    break;

                case "system.workflow.componentmodel.dll":
                    asm = Assembly.GetAssembly(typeof(System.Workflow.ComponentModel.CompositeActivity));
                    break;

                case "system.workflow.runtime.dll":
                    asm = Assembly.GetAssembly(typeof(System.Workflow.Runtime.WorkflowRuntime));
                    break;

                case "system.xml.dll":
                    asm = Assembly.GetAssembly(typeof(System.Xml.XmlDocument));
                    break;

                case "system.xml.linq.dll":
                    asm = Assembly.GetAssembly(typeof(System.Xml.Linq.XDocument));
                    break;

                default:
                    return false;
            }

            if (asm != null)
            {
                Exception excp = null;

                if (IOUtil.CreateFileInfo(asm.Location, out fileInfo, out excp))
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.AddOneMetadataFile
        //
        /// <summary>
        /// <para>Private helper function to add one metadata file with a given assembly id.
        /// assumes a fully qualified filename</para>
        /// <para>Call GlobalSymbolManager.CreateMetadataFile.
        /// This method creates an INFILESYM from the FileInfo of metadata file,
        /// and registers to its globalSymbolTable.</para>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="assemlyId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM AddOneMetadataFile(string file, int assemlyId)
        {
            return GlobalSymbolManager.CreateMetadataFile(
                file,
                assemlyId,
                MainSymbolManager.MetadataFileRootSym);
        }

        //------------------------------------------------------------
        // COMPILER.AddStandardMetadata
        //
        /// <summary>
        /// <para>Add the standard metadata library, unless disabled.
        /// This goes last, so explicit libraries can override it.</para>
        /// <para>Create and register the INFILESYM of
        ///     mscorlib.dll, system.dll and system.core.dll</para>
        /// </summary>
        //------------------------------------------------------------
        internal void AddStandardMetadata()
        {
            FileInfo mscorlibFileInfo;
            FileInfo systemFileInfo;
            FileInfo systemcoreFileInfo;
            FileInfo microsoftCSharpInfo;
            Exception excp = null;
            CSCERRID errorId = CSCERRID.ERR_NoStdLib;
            ErrArg earg = null;

            if (!this.OptionManager.NoStdLib)
            {
                //----------------------------------------
                // mscorlib.dll
                //----------------------------------------
                if (!IOUtil.CreateFileInfo(
                        Assembly.GetAssembly(typeof(Object)).Location,
                        out mscorlibFileInfo,
                        out excp))
                {
                    earg = new ErrArg("mscorlib.dll");
                    goto ERROR_HANDLING;
                }

                if (mscorlibFileInfo == null ||
                    String.IsNullOrEmpty(mscorlibFileInfo.Name))
                {
                    earg = new ErrArg("mscorlib.dll");
                    goto ERROR_HANDLING;
                }

                // Found it. Add it as an input file.
                // Create the INFILESYM instance of mscorlib.dll.
                // Assgin new assembly ID.

                INFILESYM corInfileSym
                    = AddOneMetadataFile(mscorlibFileInfo.FullName, Kaid.Nil);
                if (corInfileSym != null)
                {
                    corInfileSym.SetFileInfo(mscorlibFileInfo);
                    corInfileSym.AddToAlias(Kaid.Global);
                    //MainSymbolManager.MsCorLibAssemblyID = infile.GetAssemblyID();
                    MainSymbolManager.MsCorLibSym = corInfileSym;
                    // 2015/01/14 hirano567@hotmail.co.jp
                    MainSymbolManager.GlobalAssemblyBitset.SetBit(
                        corInfileSym.GetAssemblyID());
                }

                //----------------------------------------
                // system.dll
                //----------------------------------------
                try
                {
                    if (!IOUtil.CreateFileInfo(
                        Assembly.GetAssembly(
                            typeof(System.Text.RegularExpressions.Regex)).Location,
                        out systemFileInfo,
                        out excp))
                    {
                        string sysPath
                            = Path.Combine(systemFileInfo.DirectoryName, "system.dll");
                        if (!IOUtil.CreateFileInfo(sysPath, out systemFileInfo, out excp))
                        {
                            earg = new ErrArg("system.dll");
                            goto ERROR_HANDLING;
                        }
                    }
                }
                catch (Exception)
                {
                    earg = new ErrArg("system.dll");
                    goto ERROR_HANDLING;
                }

                INFILESYM sysInfileSym
                    = AddOneMetadataFile(systemFileInfo.FullName, Kaid.Nil);
                if (sysInfileSym != null)
                {
                    sysInfileSym.SetFileInfo(systemFileInfo);
                    sysInfileSym.AddToAlias(Kaid.Global);
                    MainSymbolManager.SystemDllSym = sysInfileSym;
                    // 2015/01/14 hirano567@hotmail.co.jp
                    MainSymbolManager.GlobalAssemblyBitset.SetBit(
                        sysInfileSym.GetAssemblyID());
                }
            
                //----------------------------------------
                // system.core.dll
                //----------------------------------------
                try
                {
                    if (!IOUtil.CreateFileInfo(
                        Assembly.GetAssembly(typeof(System.Linq.Enumerable)).Location,
                        out systemcoreFileInfo,
                        out excp))
                    {
                        string sysPath = Path.Combine(
                            systemcoreFileInfo.DirectoryName,
                            "system.core.dll");
                        if (!IOUtil.CreateFileInfo(
                                sysPath, out systemcoreFileInfo, out excp))
                        {
                            earg = new ErrArg("system.core.dll");
                            goto ERROR_HANDLING;
                        }
                    }
                }
                catch (Exception)
                {
                    earg = new ErrArg("system.core.dll");
                    goto ERROR_HANDLING;
                }

                INFILESYM syscoreInfileSym
                    = AddOneMetadataFile(systemcoreFileInfo.FullName, Kaid.Nil);
                if (syscoreInfileSym != null)
                {
                    syscoreInfileSym.SetFileInfo(systemcoreFileInfo);
                    syscoreInfileSym.AddToAlias(Kaid.Global);
                    MainSymbolManager.SystemCoreDllSym = syscoreInfileSym;
                    // 2016/02/03 hirano567@hotmail.co.jp
                    MainSymbolManager.GlobalAssemblyBitset.SetBit(
                        syscoreInfileSym.GetAssemblyID());
                }

                //----------------------------------------
                // Microsoft.CSharp.dll
                //----------------------------------------
                try
                {
                    if (!IOUtil.CreateFileInfo(
                        Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.Binder)).Location,
                        out microsoftCSharpInfo,
                        out excp))
                    {
                        string sysPath = Path.Combine(
                            microsoftCSharpInfo.DirectoryName,
                            "Microsoft.CSharp.dll");
                        if (!IOUtil.CreateFileInfo(
                                sysPath, out microsoftCSharpInfo, out excp))
                        {
                            earg = new ErrArg("Microsoft.CSharp.dll");
                            goto ERROR_HANDLING;
                        }
                    }
                }
                catch (Exception)
                {
                    earg = new ErrArg("Microsoft.CSharp.dll");
                    goto ERROR_HANDLING;
                }

                INFILESYM microsoftCSharpInfileSym
                    = AddOneMetadataFile(microsoftCSharpInfo.FullName, Kaid.Nil);
                if (microsoftCSharpInfileSym != null)
                {
                    microsoftCSharpInfileSym.SetFileInfo(microsoftCSharpInfo);
                    microsoftCSharpInfileSym.AddToAlias(Kaid.Global);
                    MainSymbolManager.MicrosoftCSharpDllSym = microsoftCSharpInfileSym;
                    MainSymbolManager.GlobalAssemblyBitset.SetBit(
                        microsoftCSharpInfileSym.GetAssemblyID());
                }
            }

            return;

        ERROR_HANDLING: ;
            Error(errorId, earg);
            return;
        }

        //------------------------------------------------------------
        // COMPILER.AddInfileToExternAliasWithErrors
        //
        /// <summary>
        /// <para>Private helper to create an alias
        /// if it doesn't already exist and add this infile sym to it</para>
        /// <para>Determine if an alias is valid, and if vaild call AddInfileToExternAlias method.</para>
        /// </summary>
        /// <param name="infile"></param>
        /// <param name="alias"></param>
        //------------------------------------------------------------
        internal void AddInfileToExternAliasWithErrors(INFILESYM infile, string alias)
        {
            string aliasName = alias;
            NameManager.AddString(alias);
            if (!NameManager.IsValidIdentifier(
                    alias,
                    LangVersionEnum.Default,
                    CheckIdentifierFlagsEnum.Simple))
            {
                // Argument alias is invalid identifier
                Error(new ERRLOC(), CSCERRID.ERR_BadExternIdentifier, new ErrArg(alias));
            }
            else if (aliasName == NameManager.GetPredefinedName(PREDEFNAME.GLOBAL))
            {
                // Do not user "global" as alias.
                Error(new ERRLOC(), CSCERRID.ERR_GlobalExternAlias);
            }
            else
            {
                AddInfileToExternAlias(infile, aliasName);
            }
        }

        //------------------------------------------------------------
        // COMPILER.AddInfileToExternAlias
        //
        /// <summary>
        /// Create an EXTERNALIASSYM instance with aliasName and register infile to it.
        /// And set assembly ID of each Bitset.
        /// </summary>
        /// <param name="infile"></param>
        /// <param name="aliasName"></param>
        //------------------------------------------------------------
        internal void AddInfileToExternAlias(INFILESYM infile, string aliasName)
        {
            // Search ailasName.
            SYM sym = null;
            EXTERNALIASSYM aliasSym = null;

            sym = LookupGlobalSym(
                aliasName,
                ExternalAilasParentSym,
                SYMBMASK.EXTERNALIASSYM);
            if (sym != null)
            {
                aliasSym = sym as EXTERNALIASSYM;
            }

            // If not found, create an EXTERNALALIASSYM with aliasName.
            if (aliasSym == null)
            {
                aliasSym = MainSymbolManager.CreateGlobalSym(
                    SYMKIND.EXTERNALIASSYM,
                    aliasName,
                    ExternalAilasParentSym
                    ) as EXTERNALIASSYM;

                aliasSym.SetAssemblyID(MainSymbolManager.AllocateAssemblyID(aliasSym));
                aliasSym.NsAidSym = MainSymbolManager.GetRootNsAid(aliasSym.GetAssemblyID());
            }

            if (!aliasSym.InFileList.Contains(infile))
            {
                // Register the INFILESYM to the list of EXTERNALIASSYM.
                MainSymbolManager.AddToGlobalSymList(infile, aliasSym.InFileList);
                infile.AddToAlias(aliasSym.GetAssemblyID());
                aliasSym.AssembliesBitset.SetBit(infile.GetAssemblyID());
            }
        }

        //------------------------------------------------------------
        // COMPILER.ClearErrorBuffer
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearErrorBuffer()
        {
            this.ErrorBuffer.Length = 0;
        }

        //------------------------------------------------------------
        // COMPILER.ResetErrorBuffer
        //
        /// <summary>
        /// Adjst COMPILER.ErrorBuffer to a given length.
        /// </summary>
        /// <param name="length"></param>
        //------------------------------------------------------------
        internal void ResetErrorBuffer(int length)
        {
            if (length >= 0 && length < ErrorBuffer.Length)
            {
                this.ErrorBuffer.Length = length;
            }
        }

        //------------------------------------------------------------
        // COMPILER.GetErrorBufferCurrent
        //------------------------------------------------------------
        //PCWSTR GetErrorBufferCurrent () { return this.errBufferNext; }

        //------------------------------------------------------------
        // COMPILER.FlushErrorBuffer (FinishErrorString in sscli)
        //
        /// <summary>
        /// <para>Return the substring of COMPILER.errorBuffer starting at index start.</para>
        /// <para>If keep is false, erase the substring from COMPILER.errorBuffer.</para>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="keep"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string FlushErrorBuffer(int start, bool keep)
        {
            string message = "";
            try
            {
                message = ErrorBuffer.ToString().Substring(start);
                if (!keep)
                {
                    ErrorBuffer.Length = start;
                }
                if (String.IsNullOrEmpty(message))
                {
                    CharUtil.ClobberBadChars(ref message);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            return message;
        }

        //------------------------------------------------------------
        // COMPILER.FlushErrorBuffer (2)    (END_ERR_STRING in sscli)
        //
        /// <summary>
        /// Return the substring of COMPILER.errorBuffer starting at index start and erase the substring.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string FlushErrorBuffer(int start)
        {
            return FlushErrorBuffer(start, false);
        }

        //------------------------------------------------------------
        // COMPILER.ScanAttributesForCLS
        //
        /// <summary>
        /// returns if CLS complianceChecking is enabled in any of these attributes.
        /// pbVal is set to indicate the CLS value
        /// (true means compliant, false means non-compliant)
        /// </summary>
        /// <param name="attrSym"></param>
        /// <param name="clsAttrSym"></param>
        /// <param name="isCLS"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ScanAttributesForCLS(
            GLOBALATTRSYM attrSym,
            ref GLOBALATTRSYM clsAttrSym,
            ref bool isCLS)
        {
            // Compile the CLS attrSym
            // (this will loop through all of them, so only call it once!)
            // Becuase this will also give regular compiler errors
            // for all badly formed global attrSym
            if (attrSym != null)
            {
                this.FuncBRec.SetUnsafe(false);
                EarlyGlobalAttrBind.Compile(this, attrSym);
                this.FuncBRec.ResetUnsafe();
            }

            while (attrSym != null)
            {
                if (attrSym.HasCLSAttribute)
                {
                    isCLS = attrSym.IsCLS;
                    clsAttrSym = attrSym;
                    return true;
                }
                attrSym = attrSym.NextAttributeSym;
            }

            isCLS = false;
            clsAttrSym = null;
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.AllowCLSErrors
        //
        /// <summary>
        /// Return true if COMPILER.OptionData.CompileSkeleton is false.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AllowCLSErrors()
        {
            //return !this.Options.CompileSkeleton && !FEncBuild();
            return !FEncBuild();
        }
        // (sscli)
        //bool AllowCLSErrors() { return !options.m_fCompileSkeleton && !FEncBuild(); }

        //------------------------------------------------------------
        // COMPILER.IsCLSAccessible
        //
        /// <summary></summary>
        /// <param name="contextSym"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsCLSAccessible(AGGSYM contextSym, TYPESYM typeSym)
        {
            if (contextSym == null || !typeSym.IsAGGTYPESYM)
            {
                return true;
            }

            AGGSYM aggSym = typeSym.GetAggregate();

            if (aggSym.Access != ACCESS.PROTECTED && aggSym.Access != ACCESS.INTERNALPROTECTED)
            {
                return true;
            }

            AGGTYPESYM typeAts = typeSym as AGGTYPESYM;
            AGGTYPESYM outerAts = typeAts.OuterTypeSym;

            if (outerAts.AllTypeArguments.Count <= 0)
            {
                return true;
            }

            AGGSYM outerAggSym = outerAts.GetAggregate();

            // does type appear in context's derivation chain?
            while (contextSym != null)
            {
                if (contextSym == outerAggSym)
                {
                    return MainSymbolManager.SubstEqualTypeArrays(
                        contextSym.AllTypeVariables,
                        outerAts.AllTypeArguments,
                        (SubstContext)null);
                }
                else
                {
                    AGGTYPESYM matchAts = contextSym.GetThisType().FindBaseType(outerAggSym);
                    if (matchAts != null)
                    {
                        return MainSymbolManager.SubstEqualTypeArrays(
                            matchAts.AllTypeArguments,
                            outerAts.AllTypeArguments,
                            (SubstContext)null);
                    }
                }
                if (contextSym.ParentSym == null || !contextSym.ParentSym.IsAGGSYM)
                {
                    return false;
                }
                contextSym = contextSym.ParentSym as AGGSYM;
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.IsCLS_Type
        //
        /// <summary>
        /// <para>True if a type is not listed as a non-CLS type
        /// See spec.</para>
        /// </summary>
        /// <param name="contextSym"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsCLS_Type(SYM contextSym, TYPESYM typeSym)
        {
            if (typeSym.IsQSimpleType() || typeSym.IsPTRSYM)
            {
                return false;
            }

            if (typeSym.IsPredefined())
            {
                return (
                    typeSym.GetPredefType() != PREDEFTYPE.REFANY &&
                    typeSym.GetPredefType() != PREDEFTYPE.UINTPTR);
            }

            if (typeSym.IsARRAYSYM)
            {
                // arrays of arrays are CLS compliant
                TYPESYM elementType = (typeSym as ARRAYSYM).ElementTypeSym;
                return IsCLS_Type(contextSym, elementType);
            }

            if (typeSym.IsPARAMMODSYM)
            {
                return IsCLS_Type(contextSym, (typeSym as PARAMMODSYM).ParamTypeSym);
            }

            if (typeSym.IsVOIDSYM)
            {
                return true;
            }

            if (typeSym.IsNUBSYM)
            {
                return IsCLS_Type(contextSym, (typeSym as NUBSYM).BaseTypeSym);
            }

            if (contextSym == null &&
                contextSym.IsAGGSYM &&
                !IsCLSAccessible(contextSym as AGGSYM, typeSym))
            {
                return false;
            }

            if (typeSym.IsAGGTYPESYM)
            {
                AGGTYPESYM ats = typeSym as AGGTYPESYM;
                if (ats.TypeArguments.Count > 0)
                {
                    for (int i = 0, n = ats.TypeArguments.Count; i < n; i++)
                    {
                        TYPESYM arg = ats.TypeArguments[i];
                        if (!this.IsCLS_Type(contextSym, arg)) return false;
                    }
                }

                return CheckSymForCLS((typeSym as AGGTYPESYM).GetAggregate(), false);
            }

            return CheckSymForCLS(typeSym, false);
        }

        //------------------------------------------------------------
        // COMPILER.CheckSymForCLS
        //
        /// <summary>
        /// <para>true iff this symbol should be CLS compliant based on it's attributes
        /// and the attributes of it's declaration scope</para>
        /// <para>If FailIfCantBeDeclared is false, this might cause types ot be declared</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="FailIfCantBeDeclared"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckSymForCLS(SYM sym, bool FailIfCantBeDeclared)
        {
            DebugUtil.Assert(sym != null && !sym.IsNSSYM);

            if (sym.IsERRORSYM || sym.IsVOIDSYM)
            {
                return true;
            }

            if (sym.IsNSDECLSYM)
            {
                // Since there are no attributes on namespaces
                // skip directly to the 'global' assembly attribute.
                // If there is no assembly level attribute and we are
                // doing CLS checking, assume it should be Compliant
                //GET_ASSEMBLY:
                INFILESYM infine = sym.GetInputFile();
                return infine.HasCLSAttribute && infine.IsCLS;
            }

            if (sym.IsAGGSYM)
            {
                EnsureState(sym as AGGSYM, AggStateEnum.DefinedMembers);
                if (FailIfCantBeDeclared && !CanAggSymBeDeclared(sym as AGGSYM)) return false;
                EnsureState(sym as AGGSYM, AggStateEnum.Prepared);
            }
            // If the type can't be declared then we shouldn't have even gotten into CLS checking
            DebugUtil.Assert(!sym.IsAGGSYM || (sym as AGGSYM).IsPrepared);

            if (sym.HasCLSAttribute)
            {
                return sym.IsCLS;
            }

            // For AGGSYM, just use the first declaration of the class, since all must be equivalent.
            PARENTSYM temp;
            if (sym.IsAGGSYM)
            {
                temp = (sym as AGGSYM).FirstDeclSym.ParentDeclSym;  //.DeclFirst().DeclPar();
            }
            else if (sym.IsAGGDECLSYM)
            {
                temp = (sym as AGGDECLSYM).AggSym;
            }
            else
            {
                temp = sym.ParentSym;
            }

            return CheckSymForCLS(temp, FailIfCantBeDeclared);
        }

        //------------------------------------------------------------
        // COMPILER.EmitRelaxations
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool EmitRelaxations()
        {
            return noRelaxationsAttribute && buildAssembly;
        }

        //------------------------------------------------------------
        // COMPILER.SuppressRelaxations
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void SuppressRelaxations()
        {
            noRelaxationsAttribute = false;
        }

        //------------------------------------------------------------
        // COMPILER.EmitRuntimeCompatibility
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool EmitRuntimeCompatibility()
        {
            return (
                noRuntimeCompatibility &&
                GetOptPredefType(PREDEFTYPE.RUNTIMECOMPATIBILITY, true) != null);
        }

        //------------------------------------------------------------
        // COMPILER.SuppressRuntimeCompatibility
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void SuppressRuntimeCompatibility()
        {
            noRuntimeCompatibility = false;
        }

        //------------------------------------------------------------
        // COMPILER.WrapNonExceptionThrows
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool WrapNonExceptionThrows()
        {
            return (
                this.wrapNonException &&
                this.GetOptPredefType(PREDEFTYPE.RUNTIMECOMPATIBILITY, true) != null);
        }

        //------------------------------------------------------------
        // COMPILER.SuppressWrapNonExceptionThrows
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void SuppressWrapNonExceptionThrows()
        {
            wrapNonException = false;
        }

        //------------------------------------------------------------
        // COMPILER.RecordAssemblyRefToOutput
        //
        /// <summary></summary>
        /// <param name="nameRef"></param>
        /// <param name="infileSym"></param>
        /// <param name="isFriendAssemblyRef"></param>
        //------------------------------------------------------------
        internal void RecordAssemblyRefToOutput(string nameRef, SYM infileSym, bool isFriendAssemblyRef)
        {
            if (infileSym == null)
            {
                return;
            }

            // Find the end of the list, or the element that has the same name
            AssemblyRefList arl = null;
            for (int i = 0; i < this.ArlRefsToOutput.Count; ++i)
            {
                AssemblyRefList arlTemp = ArlRefsToOutput[i];
                if (String.Compare(nameRef, arlTemp.NameRef, true) == 0)
                {
                    arl = arlTemp;
                    break;
                }
            }

            // If this name doesn't already exist, add it
            if (arl == null)
            {
                arl = new AssemblyRefList();
                arl.NameRef = nameRef;
                arl.IsFriendAssemblyRefOnly = isFriendAssemblyRef;
                this.ArlRefsToOutput.Add(arl);
            }
            else
            {
                arl.IsFriendAssemblyRefOnly &= isFriendAssemblyRef;
            }

            // Shouldn't add the same source twice!
            // Now add the module
            if (!arl.SymList.Contains(infileSym))
            {
                arl.SymList.Add(infileSym);
            }
        }

        //------------------------------------------------------------
        // COMPILER.MarkUsedFriendAssemblyRef
        //
        /// <summary>
        /// Mark infile as having it's friend reference to the assembly being built as used.  
        /// If this turns out to be a bad reference, we will not error if it is never marked as being used.
        /// </summary>
        /// <param name="infileSym"></param>
        //------------------------------------------------------------
        internal void MarkUsedFriendAssemblyRef(INFILESYM infileSym)
        {
            DebugUtil.Assert(!infileSym.IsSource && infileSym.InternalsVisibleTo(Kaid.ThisAssembly));
            infileSym.FriendAccessUsed = true;
        }

        //// Returns the max state that EnsureState will force.
        //AggStateEnum AggStateMax() {
        //    return aggSymStateMax;
        //}

        //------------------------------------------------------------
        // COMPILER.ForceAggStates (1)
        //
        /// <summary>
        /// <para>This attempts to bring the type up to at least the indicated aggSym state.
        /// What this means depends on the kind of type we're dealing with:
        /// <list type="bullet">
        /// <item>
        /// <para>For pointers, arrays, etc where there is a single constituent,
        /// the state of the type is the state of the constituent.</para>
        /// </item>
        /// <item>
        /// <para>For an AGGTYPESYM, the state of the type is the state of the AGGSYM.
        /// This does not check the states of type arguments!</para>
        /// <para>This avoid infinite recursion
        /// when a type variable is used as a type argument in one of its bounds.
        /// It also fulfills the general rule
        /// that EnsureState propogates to base types but not type arguments.</para>
        /// </item>
        /// <item>
        /// <para>For a TYVARSYM, the state of the type is
        /// the min of the state of each AGGTYPESYM used as a bound.</para>
        /// </item>
        /// </list>
        /// </para>
        /// <para>Type variables and aggSyms depend on base types.
        /// AGGSYMs and TypeArrays are handled by overloads of ForceAggStates.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="minState"></param>
        //------------------------------------------------------------
        internal void ForceAggStates(TYPESYM typeSym, AggStateEnum minState)
        {
            DebugUtil.Assert(this.IsBelow(typeSym, minState));
            DebugUtil.Assert(minState <= this.AggStateMax);

            switch (typeSym.Kind)
            {
                default:
                    break;

                case SYMKIND.PTRSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.ARRAYSYM:
                    EnsureState(typeSym.ParentSym as TYPESYM, minState);
                    break;

                case SYMKIND.NUBSYM:
                    AGGTYPESYM ats = (typeSym as NUBSYM).GetAggTypeSym();
                    if (ats != null)
                    {
                        EnsureState(ats, minState);
                    }
                    break;

                case SYMKIND.AGGTYPESYM:
                    EnsureState((typeSym as AGGTYPESYM).GetAggregate(), minState);
                    break;

                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym.IsTYPESYM)
                    {
                        EnsureState(typeSym.ParentSym as TYPESYM, minState);
                    }
                    break;

                case SYMKIND.TYVARSYM:
                    // NOTE: This may change var.bnds!
                    if (typeSym.ParentSym != null)
                    {
                        DebugUtil.Assert(
                            typeSym.ParentSym == null ||
                            typeSym.ParentSym.IsAGGSYM ||
                            typeSym.ParentSym.IsMETHSYM);

                        EnsureState(
                            typeSym.ParentSym.IsAGGSYM ?
                                typeSym.ParentSym as AGGSYM :
                                (typeSym.ParentSym as METHSYM).ClassSym,
                            minState);
                    }

                    if (TypeArray.Size((typeSym as TYVARSYM).BoundArray) > 0)
                    {
                        EnsureState((typeSym as TYVARSYM).BoundArray, minState);
                    }
                    break;
            }

            ComputeAggState(typeSym);
            DebugUtil.Assert(typeSym.AggState >= minState);
        }

        //------------------------------------------------------------
        // COMPILER.ForceAggStates (2)
        //
        /// <summary>
        /// Just call EnsureState on all of the types.
        /// </summary>
        /// <param name="typeArray"></param>
        /// <param name="minState"></param>
        //------------------------------------------------------------
        private void ForceAggStates(TypeArray typeArray, AggStateEnum minState)
        {
            //DebugUtil.Assert(ta.Count> 0);
            DebugUtil.Assert(IsBelow(typeArray, minState));

            for (int i = 0; i < typeArray.Count; i++)
            {
                EnsureState(typeArray[i], minState);
            }
            ComputeAggState(typeArray);
        }

        //------------------------------------------------------------
        // COMPILER.ForceAggStates (3)
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        /// <param name="minState"></param>
        //------------------------------------------------------------
        internal void ForceAggStates(AGGSYM aggSym, AggStateEnum minState)
        {
            DebugUtil.Assert(this.IsBelow(aggSym, minState));
            DebugUtil.Assert(minState <= this.aggStateMax);

            if (aggSym.IsSource)
            {
                DebugUtil.Assert(false, "Shouldn't be forcing a source aggSym state!");
                return;
            }
            if (aggSym.IsUnresolved)
            {
                UndeclarableType(aggSym.AsUnresolved());
                return;
            }

            // These all take care of base types and interfaces as well.
            if (minState <= AggStateEnum.Inheritance)
            {
                // They're asking for at most inheritance.
                DebugUtil.VsVerify(
                    ClsDeclRec.ResolveInheritanceRec(aggSym),
                    "ResolveInheritanceRec failed ForceAggStates!");
            }
            else if (minState <= AggStateEnum.Bounds)
            {
                // They're asking for at most bounds.
                DebugUtil.VsVerify(
                    ClsDeclRec.ResolveInheritanceRec(aggSym),
                    "ResolveInheritanceRec failed ForceAggStates!");
                this.Importer.DefineBounds(aggSym);
            }
            else if (minState <= AggStateEnum.DefinedMembers)
            {
                // They're asking for at most members.
                // Note that this takes care of inheritance.
                this.Importer.DefineImportedType(aggSym);
            }
            else
            {
                // Anything beyond members means prepare.
                // Note that this takes care of inheritance and members.
                PrepareAggregate(aggSym);
            }
        }

        //------------------------------------------------------------
        // COMPILER.IsBelow (1)
        //
        /// <summary>
        /// <para>(In sscli, FBelow)</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="minState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsBelow(TYPESYM typeSym, AggStateEnum minState)
        {
            return typeSym.AggState < minState;
        }

        //------------------------------------------------------------
        // COMPILER.IsBelow (2)
        //
        /// <summary>
        /// <para>(In sscli, FBelow)</para>
        /// </summary>
        /// <param name="typeArray"></param>
        /// <param name="minState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsBelow(TypeArray typeArray, AggStateEnum minState)
        {
            return typeArray.AggState < minState;
        }

        //------------------------------------------------------------
        // COMPILER.IsBelow (3)
        //
        /// <summary>
        /// <para>(In sscli, FBelow)</para>
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="minState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsBelow(AGGSYM aggSym, AggStateEnum minState)
        {
            return aggSym.AggState < minState;
        }

        //------------------------------------------------------------
        // COMPILER.EnsureState (1)
        //
        /// <summary>
        /// <para></para>
        /// <para>The default value of 2nd argument is AggStateEnum.Prepared.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="minState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AggStateEnum EnsureState(
            TYPESYM typeSym,
            AggStateEnum minState)   // = AggState::Prepared)
        {
            DebugUtil.Assert(minState < AggStateEnum.Lim);

            if (minState > this.AggStateMax)
            {
                minState = this.AggStateMax;
            }
            if (this.IsBelow(typeSym, minState))
            {
                ForceAggStates(typeSym, minState);
            }
            return typeSym.AggState;
        }

        //------------------------------------------------------------
        // COMPILER.EnsureState (2)
        //
        /// <summary></summary>
        /// <param name="typeArray"></param>
        /// <param name="minState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AggStateEnum EnsureState(
            TypeArray typeArray,
            AggStateEnum minState) // = AggState::Prepared)
        {
            DebugUtil.Assert(minState < AggStateEnum.Lim);

            if (minState > this.AggStateMax)
            {
                minState = this.AggStateMax;
            }
            if (this.IsBelow(typeArray, minState))
            {
                ForceAggStates(typeArray, minState);
            }
            return typeArray.AggState;
        }
        //------------------------------------------------------------
        // COMPILER.EnsureState (3)
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        /// <param name="minState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AggStateEnum EnsureState(
            AGGSYM aggSym,
            AggStateEnum minState)   // = AggState::Prepared)
        {
            DebugUtil.Assert(minState < AggStateEnum.Lim);

            if (minState > this.AggStateMax)
            {
                minState = this.AggStateMax;
            }
            if (this.IsBelow(aggSym, minState))
            {
                ForceAggStates(aggSym, minState);
            }
            return aggSym.AggState;
        }

        // Update the value of aggSymStateMin.
        // For the EE this also recomputes fDirty and tsDirty.

        //------------------------------------------------------------
        // COMPILER.ComputeAggState (1)
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void ComputeAggState(TYPESYM typeSym)
        {
            TYPESYM srcTypeSym;
            AGGTYPESYM ats;
            AGGSYM aggSym;

            switch (typeSym.Kind)
            {
                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.ANONMETHSYM:
                case SYMKIND.METHGRPSYM:
                case SYMKIND.IMPLICITTYPESYM:           // CS3
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM:   // CS3
                    typeSym.AggState = AggStateEnum.Last;
                    break;

                case SYMKIND.PTRSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.ARRAYSYM:
                    srcTypeSym = typeSym.ParentSym as TYPESYM;
                    typeSym.AggState = srcTypeSym.AggState;
                    break;

                case SYMKIND.NUBSYM:
                    ats = (typeSym as NUBSYM).GetAggTypeSym();
                    if (ats != null)
                    {
                        typeSym.AggState = ats.AggState;
                    }
                    else
                    {
                        typeSym.AggState = AggStateEnum.Last;
                    }
                    break;

                case SYMKIND.AGGTYPESYM:
                case SYMKIND.DYNAMICSYM:    // CS4
                    ats = typeSym as AGGTYPESYM;
                    aggSym = ats.GetAggregate();

                    if (aggSym.AggState > ats.AggState)
                    {
                        ats.ConstraintsChecked = false;
                        ats.HasConstraintError = false;
                    }
                    ats.AggState = aggSym.AggState;
                    break;

                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym == null)
                    {
                        typeSym.AggState = AggStateEnum.Last;
                    }
                    else
                    {
                        ERRORSYM err = typeSym as ERRORSYM;
                        DebugUtil.Assert(
                            err.ParentSym != null &&
                            err.Name != null &&
                            err.TypeArguments != null);

                        if (err.ParentSym.IsTYPESYM)
                        {
                            err.AggState = (err.ParentSym as TYPESYM).AggState;
                        }
                        else
                        {
                            err.AggState = AggStateEnum.Last;
                        }
                    }
                    break;

                case SYMKIND.TYVARSYM:
                    if (typeSym.ParentSym == null)
                    {
                        // This should be a standard TYVARSYM used for emitting.
                        DebugUtil.Assert((typeSym as TYVARSYM).ParseTreeNode == null);
                        typeSym.AggState = AggStateEnum.Last;
                    }
                    else
                    {
                        DebugUtil.Assert(
                            typeSym.ParentSym != null &&
                            (typeSym.ParentSym.IsAGGSYM || typeSym.ParentSym.IsMETHSYM));

                        TYVARSYM var = typeSym as TYVARSYM;
                        TypeArray bnds = var.BoundArray; // May be NULL.

                        var.AggState = AggStateEnum.Last;

                        if (bnds == null)
                        {
                            if (var.AggState > AggStateEnum.Declared)
                            {
                                var.AggState = AggStateEnum.Declared;
                            }
                        }
                        else if (bnds.Count == 0)
                        {
                            bnds.AggState = AggStateEnum.Last;
                        }
                        else if (var.AggState > bnds.AggState)
                        {
                            var.AggState = bnds.AggState;
                        }
                    }
                    break;

                default:
                    DebugUtil.Assert(false, "unknown type");
                    break;
            }
        }

        //------------------------------------------------------------
        // COMPILER.ComputeAggState (2)
        //
        /// <summary>
        /// The minimum value of the ComputeAggState(TYPESYM) values of all elements.
        /// </summary>
        /// <param name="ta"></param>
        //------------------------------------------------------------
        internal void ComputeAggState(TypeArray typeArray)
        {
            if (typeArray == null)
            {
                return;
            }
            typeArray.AggState = AggStateEnum.Last;

            for (int i = 0; i < typeArray.Count; i++)
            {
                TYPESYM type = typeArray[i];
                if (typeArray.AggState > type.AggState)
                {
                    typeArray.AggState = type.AggState;
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.LookupGlobalSym
        //
        /// <summary>
        /// <para>Lookup in the given parent.
        /// If symPar is an NSSYM or AGGSYM and mask includes MASK_AGGSYM,
        /// this makes sure all relevant types are loaded.</para>
        /// <para>If not imported, import it and
        /// call MainSymbolManager.LookupGlobalSymCore.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mask"></param>
        /// <param name="parentSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM LookupGlobalSym(string name, PARENTSYM parentSym, SYMBMASK mask)
        {
            if (parentSym.IsNSSYM &&
                (parentSym as NSSYM).AnyTypesUnloaded() &&
                (mask & SYMBMASK.AGGSYM) != 0 &&
                this.compilationPhase > CompilerPhaseEnum.ImportTypes)
            {
                // Load the imported types in this namespace (all assemblies).
                this.Importer.LoadTypesInNsAid(parentSym as NSSYM, Kaid.Nil, null);
            }

            return MainSymbolManager.LookupGlobalSymCore(name, parentSym, mask);
        }

        //------------------------------------------------------------
        // COMPILER.LookupInBagAid
        //
        /// <summary>
        /// Lookup in the given bag, restricting to the given assemblyId.
        /// Aid should be an assembly id or extern alias id - NOT a module id.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bagSym"></param>
        /// <param name="assemblyId"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM LookupInBagAid(
            string name,
            BAGSYM bagSym,
            int arity,
            int assemblyId,
            SYMBMASK mask)
        {
            DebugUtil.Assert(assemblyId < Kaid.MinModule);

            if (!bagSym.InAlias(assemblyId))
            {
                return null;
            }

            if (bagSym.IsNSSYM &&
                (bagSym as NSSYM).AnyTypesUnloaded() &&
                (mask & SYMBMASK.AGGSYM) != 0 &&
                this.compilationPhase > CompilerPhaseEnum.ImportTypes)
            {
                EnsureTypesInNsAid(name, bagSym as NSSYM, arity, assemblyId);
            }

            for (SYM sym = MainSymbolManager.LookupGlobalSymCore(name, bagSym, mask);
                sym != null;
                sym = sym.NextSameNameSym)
            {
                if ((sym.Mask & mask) == 0)
                {
                    continue;
                }
                switch (sym.Kind)
                {
                    case SYMKIND.AGGSYM:
                        if ((sym as AGGSYM).InAlias(assemblyId))
                        {
                            return sym;
                        }
                        break;

                    case SYMKIND.NSSYM:
                        if ((sym as NSSYM).InAlias(assemblyId))
                        {
                            return sym;
                        }
                        break;

                    case SYMKIND.FWDAGGSYM:
                        if ((sym as FWDAGGSYM).InAlias(assemblyId))
                        {
                            return sym;
                        }
                        break;

                    default:
                        return sym;
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // COMPILER.LookupNextInAid
        //
        /// <summary>
        /// Lookup the next same name symbol in the given assemblyId.
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="assemblyId"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM LookupNextInAid(SYM sym, int assemblyId, SYMBMASK mask)
        {
            DebugUtil.Assert(assemblyId < Kaid.MinModule);
            DebugUtil.Assert(sym != null);

            while ((sym = sym.NextSameNameSym) != null)
            {
                if ((sym.Mask & mask) == 0)
                {
                    continue;
                }
                switch (sym.Kind)
                {
                    case SYMKIND.AGGSYM:
                        if ((sym as AGGSYM).InAlias(assemblyId))
                        {
                            return sym;
                        }
                        break;

                    case SYMKIND.NSSYM:
                        if ((sym as NSSYM).InAlias(assemblyId))
                        {
                            return sym;
                        }
                        break;

                    case SYMKIND.FWDAGGSYM:
                        if ((sym as FWDAGGSYM).InAlias(assemblyId))
                        {
                            return sym;
                        }
                        break;

                    default:
                        return sym;
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // COMPILER.SetBaseType
        //
        /// <summary>
        /// Set the baseClass field of the AGGSYM.
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="baseAggTypeSym"></param>
        //------------------------------------------------------------
        internal void SetBaseType(AGGSYM aggSym, AGGTYPESYM baseAggTypeSym)
        {
            DebugUtil.Assert(aggSym.BaseClassSym == null);
#if DEBUG
            if (!(
                baseAggTypeSym == null ||
                baseAggTypeSym.GetAggregate().HasResolvedBaseClasses))
            {
                ;
            }
#endif
            DebugUtil.Assert(
                baseAggTypeSym == null ||
                baseAggTypeSym.GetAggregate().HasResolvedBaseClasses);
            DebugUtil.Assert(
                baseAggTypeSym != null ||
                aggSym.IsInterface ||
                aggSym.IsPredefAgg(PREDEFTYPE.OBJECT));

            aggSym.BaseClassSym = baseAggTypeSym;
        }

        //------------------------------------------------------------
        // COMPILER.SetIfaces
        //
        /// <summary>
        /// <para>Set the interfaces of the AGGSYM.</para>
        /// <para>Builds the full list of supported interfaces for a type.
        /// Also checks for conflicting members between interfaces in the list,
        /// and all their inherited interfaces.</para>
        /// <para>It is important that we form the lists in a given order.
        /// Basically, an interface may not be preceeded by any interfaces
        /// that it descends from.  So, we always add to the front of the list.</para>
        /// <para>If interfaces is null, set as AGGTYPESYM[].</para>
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="ifaces"></param>
        //------------------------------------------------------------
        internal void SetIfaces(AGGSYM aggSym, TypeArray ifaceArray)
        {
            DebugUtil.Assert(aggSym.TypeVariables != null);
            DebugUtil.Assert(aggSym.AllTypeVariables != null);
            //DebugUtil.Assert(aggSym.AllInterfacesArray == null);
            //DebugUtil.Assert(aggSym.InterfacesArray == null);
            DebugUtil.Assert(aggSym.InterfaceCountAll == 0);
            DebugUtil.Assert(aggSym.InterfaceCount == 0);

            TypeArray ifaces = MainSymbolManager.AllocParams(ifaceArray);
            int ciface = ifaces.Count;
            TypeArray ifacesAll;

            if (ciface == 0)
            {
                DebugUtil.Assert(ifaces == BSYMMGR.EmptyTypeArray);
                ifacesAll = ifaces;
            }
            else if (aggSym.AllTypeVariables.Count == 0)
            {
                DebugUtil.Assert(aggSym.TypeVariables.Count == 0);

                ifacesAll = MainSymbolManager.BuildIfacesAll(
                    aggSym,
                    ifaces.List.ConvertAll<AGGTYPESYM>(SystemConverter.TypeSymToAggTypeSym),
                    null);
            }
            else
            {
                BSYMMGR.UnifyContext context = new BSYMMGR.UnifyContext(
                    aggSym.AllTypeVariables,
                    BSYMMGR.EmptyTypeArray);

                ifacesAll = MainSymbolManager.BuildIfacesAll(
                    aggSym,
                    ifaces.List.ConvertAll<AGGTYPESYM>(SystemConverter.TypeSymToAggTypeSym),
                    context);
            }

            aggSym.Interfaces = ifaces;
            aggSym.AllInterfaces = ifacesAll;
        }

        //------------------------------------------------------------
        // COMPILER.SetIfaces
        //
        /// <summary>
        /// Set the interfaces of the AGGSYM.
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="ifaceArray"></param>
        //------------------------------------------------------------
        internal void SetIfaces(AGGSYM aggSym, AGGTYPESYM[] ifaceArray)
        {
            // We call MainSymbolManager.AllocParams in SetIfaces(AGGSYM, TypeArray).
            TypeArray ifaces = new TypeArray(ifaceArray);
            SetIfaces(aggSym, ifaces);
        }

        //------------------------------------------------------------
        // COMPILER.SetBounds
        //
        /// <summary>
        /// <para>Set the bounds of the type variable to the given set.
        /// This just sets the boundArray field.
        /// After all siblings TYVARSYMs have their bounds set, call ResolveBounds on each.</para>
        /// <para>(sscli) reset has the default value false.</para>
        /// </summary>
        /// <param name="typeVarSym"></param>
        /// <param name="boundArray"></param>
        /// <param name="reset"></param>
        //------------------------------------------------------------
        internal void SetBounds(
            TYVARSYM typeVarSym,
            TypeArray boundArray,
            bool reset)	// = false
        {
            DebugUtil.Assert(boundArray != null);
            DebugUtil.Assert(typeVarSym.BoundArray == null || reset);
            DebugUtil.Assert(!typeVarSym.FResolved() || reset);
            DebugUtil.Assert(!typeVarSym.IsResolving || reset);

            List<TYPESYM> typeList = new List<TYPESYM>();

            // Filter out dups and System.Object.
            for (int i = 0; i < boundArray.Count; ++i)
            {
                // Don't record System.Object.
                if (boundArray[i].IsPredefType(PREDEFTYPE.OBJECT))
                {
                    continue;
                }

                for (int j = typeList.Count; ; )
                {
                    if (j == 0)
                    {
                        // The type doesn't match a previous type so record it.
                        typeList.Add(boundArray[i]);
                        break;
                    }
                    --j;
                    if (boundArray[i] == typeList[j])
                    {
                        // It matches a previous type so don't record it.
                        break;
                    }
                }
            }

            if (typeList.Count < boundArray.Count)
            {
                boundArray = MainSymbolManager.AllocParams(typeList);
            }

            typeVarSym.SetBounds(boundArray);
        }

        //------------------------------------------------------------
        // COMPILER.ResolveBounds
        //
        /// <summary>
        /// Recursively compute the ifacesAll and atsCls members of a type variable.
        /// This may recurse since a type variable may have another type variable as a bound.
        /// </summary>
        /// <param name="tyVarSym"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ResolveBounds(TYVARSYM tyVarSym, bool inherited)
        {
            DebugUtil.Assert(tyVarSym != null);
            DebugUtil.Assert(tyVarSym.BoundArray != null);

            if (tyVarSym.FResolved())
            {
                DebugUtil.Assert(!tyVarSym.IsResolving);
                return true;
            }

            if (tyVarSym.IsResolving)
            {
                // Circularity. Caller should report an error.
                return false;
            }

            tyVarSym.IsResolving = true;

            bool isValueType = false;
            TypeArray boundArray = tyVarSym.BoundArray;
            AGGTYPESYM baseAggTypeSym = GetReqPredefType(PREDEFTYPE.OBJECT, false);
            TYPESYM absoluteBaseTypeSym = baseAggTypeSym;

            // Resolve base type vars, compute the max number of interfaces and
            // determine atsCls - the most derived base class. Also filter out any
            // bounds that cause cycles in the bound hierarchy.

            //TYPESYM ** prgtype = STACK_ALLOC(TYPESYM *, boundArray.size);
            //int ctype = 0;
            int cifaceMax = 0;
            List<TYPESYM> typeList = new List<TYPESYM>();

            for (int i = 0; i < boundArray.Count; i++)
            {
                TYPESYM boundTypeSym = boundArray[i];
                AGGTYPESYM tempBaseAggTypeSym = null; // Effective base class.
                TYPESYM tempBaseTypeSym = null; // Other base type, like object[], int, etc.

                switch (boundTypeSym.Kind)
                {
                    case SYMKIND.AGGTYPESYM:
                        DebugUtil.Assert(boundTypeSym.GetAggregate().HasResolvedBaseClasses);
                        switch (boundTypeSym.GetAggregate().AggKind)
                        {
                            case AggKindEnum.Interface:
                                cifaceMax += (boundTypeSym as AGGTYPESYM).GetIfacesAll().Count + 1;
                                //prgtype[ctype++] = boundTypeSym;
                                typeList.Add(boundTypeSym);
                                continue;

                            case AggKindEnum.Class:
                                tempBaseTypeSym = tempBaseAggTypeSym = boundTypeSym as AGGTYPESYM;
                                break;

                            case AggKindEnum.Delegate:
                                DebugUtil.Assert(inherited);
                                tempBaseTypeSym = tempBaseAggTypeSym = boundTypeSym as AGGTYPESYM;
                                break;

                            case AggKindEnum.Struct:
                                // Effective base class is System.ValueType.
                                DebugUtil.Assert(inherited);
                                tempBaseAggTypeSym = GetReqPredefType(PREDEFTYPE.VALUE, false);
                                tempBaseTypeSym = boundTypeSym;
                                break;

                            case AggKindEnum.Enum:
                                // Effective base class is System.Enum.
                                DebugUtil.Assert(inherited);
                                tempBaseAggTypeSym = GetReqPredefType(PREDEFTYPE.ENUM, false);
                                tempBaseTypeSym = boundTypeSym;
                                break;

                            default:
                                DebugUtil.Assert(false, "Bad AggKind");
                                tempBaseTypeSym = boundTypeSym;
                                break;
                        }
                        DebugUtil.Assert(
                            tempBaseAggTypeSym == null ||
                            tempBaseAggTypeSym.IsClassType());
                        break;

                    case SYMKIND.TYVARSYM:
                        TYVARSYM tempBoundTvSym = boundTypeSym as TYVARSYM;
                        if (!ResolveBounds(tempBoundTvSym, inherited))
                        {
                            DebugUtil.Assert(tempBoundTvSym.AllInterfaces == null);
                            ErrorRef(
                                null,
                                CSCERRID.ERR_CircularConstraint,
                                new ErrArgRef(tempBoundTvSym),
                                new ErrArgRef(tyVarSym));
                            continue;
                        }
                        DebugUtil.Assert(tempBoundTvSym.FResolved());

                        if (tempBoundTvSym.HasValueConstraint())
                        {
                            if (!inherited)
                            {
                                ErrorRef(
                                    null,
                                    CSCERRID.ERR_ConWithValCon,
                                    new ErrArgRef(tyVarSym),
                                    new ErrArgRef(tempBoundTvSym));
                                continue;
                            }
                            isValueType = true;
                        }
                        cifaceMax += tempBoundTvSym.AllInterfaces.Count;
                        tempBaseAggTypeSym = tempBoundTvSym.BaseClassSym;
                        tempBaseTypeSym = tempBoundTvSym.AbsoluteBaseTypeSym;
                        break;

                    case SYMKIND.NUBSYM:
                        // This should only happen if we're computing bounds automagically....
                        DebugUtil.Assert(inherited);
                        tempBaseAggTypeSym = GetReqPredefType(PREDEFTYPE.VALUE, false);
                        tempBaseTypeSym = boundTypeSym;
                        break;

                    case SYMKIND.ARRAYSYM:
                        // This should only happen if we're computing bounds automagically....
                        DebugUtil.Assert(inherited);
                        tempBaseAggTypeSym = GetReqPredefType(PREDEFTYPE.ARRAY, false);
                        tempBaseTypeSym = boundTypeSym;
                        break;

                    default:
                        // Some other type. This should only happen if we're computing bounds
                        // automagically....
                        DebugUtil.Assert(inherited);
                        tempBaseTypeSym = boundTypeSym;
                        break;
                }
                //prgtype[ctype++] = boundTypeSym;
                typeList.Add(boundTypeSym);
                DebugUtil.Assert(
                    tempBaseTypeSym!=null &&
                    (tempBaseAggTypeSym == null || IsBaseType(tempBaseTypeSym, tempBaseAggTypeSym)));

                if (!IsBaseType(absoluteBaseTypeSym, tempBaseTypeSym))
                {
                    if (!IsBaseType(tempBaseTypeSym, absoluteBaseTypeSym))
                    {
                        Error(
                            CSCERRID.ERR_BaseConstraintConflict,
                            new ErrArg(tyVarSym),
                            new ErrArg(tempBaseTypeSym),
                            new ErrArg(absoluteBaseTypeSym));
                    }
                    else if (
                        tempBaseAggTypeSym != null &&
                        !IsBaseType(tempBaseAggTypeSym, baseAggTypeSym))
                    {
                        DebugUtil.Assert(false,
                            "Bad logic - tempBaseTypeSym derives from absoluteBaseTypeSym," +
                            " but tempBaseAggTypeSym doesn't derive from baseAggTypeSym");
                    }
                    else
                    {
                        absoluteBaseTypeSym = tempBaseTypeSym;
                        if (tempBaseAggTypeSym != null)
                        {
                            baseAggTypeSym = tempBaseAggTypeSym;
                        }
                    }
                }
                else if (
                    tempBaseAggTypeSym != null &&
                    !IsBaseType(baseAggTypeSym, tempBaseAggTypeSym))
                {
                    DebugUtil.Assert(false,
                        "Bad logic - absoluteBaseTypeSym derives from tempBaseTypeSym," +
                        "but baseAggTypeSym doesn't derive from tempBaseAggTypeSym");
                }
            }

            // If circularity is detected, our actual set of bounds is less than
            // the original set.

            //DebugUtil.Assert(0 <= ctype && ctype <= boundArray.Count);
            DebugUtil.Assert(typeList.Count <= boundArray.Count);

            if (typeList.Count < boundArray.Count)
            {
                boundArray = MainSymbolManager.AllocParams(typeList);
                tyVarSym.SetBounds(boundArray);
            }
            DebugUtil.Assert(IsBaseType(absoluteBaseTypeSym, baseAggTypeSym));

            tyVarSym.SetBaseTypes(absoluteBaseTypeSym, baseAggTypeSym);
            DebugUtil.Assert(!tyVarSym.HasReferenceBound && !tyVarSym.HasValueBound);

            if (isValueType || absoluteBaseTypeSym.IsValueType())
            {
                tyVarSym.HasValueBound = true;
            }
            if (absoluteBaseTypeSym.IsReferenceType())
            {
                if (!absoluteBaseTypeSym.IsPredefined())
                {
                    tyVarSym.HasReferenceBound = true;
                }
                else
                {
                    PREDEFTYPE pt = absoluteBaseTypeSym.GetPredefType();
                    tyVarSym.HasReferenceBound =
                        (pt != PREDEFTYPE.OBJECT && pt != PREDEFTYPE.VALUE && pt != PREDEFTYPE.ENUM);
                }
            }

            TypeArray ifacesAll;

            if (cifaceMax > 0)
            {
                //AGGTYPESYM ** pifaceMin = STACK_ALLOC(AGGTYPESYM *, cifaceMax);
                //AGGTYPESYM ** pifaceLim = pifaceMin + cifaceMax;
                //AGGTYPESYM ** piface = pifaceLim;
                List<AGGTYPESYM> interfaceList = new List<AGGTYPESYM>();

                // Always add to the front so process the interfaces in the reverse order.
                for (int i = boundArray.Count; --i >= 0; )
                {
                    TYPESYM tempBoundTypeSym = boundArray[i];

                    if (tempBoundTypeSym.IsAGGTYPESYM)
                    {
                        AGGTYPESYM tempBoundAggTypeSym = tempBoundTypeSym as AGGTYPESYM;
                        if (!tempBoundAggTypeSym.GetAggregate().IsInterface)
                        {
                            continue;
                        }

                        // If tempBoundAggTypeSym is already in this array then the interface has already been seen
                        // so all of its ifaces have also been seen.
                        if (BSYMMGR.FindAts(tempBoundAggTypeSym, interfaceList))
                        {
                            continue;
                        }
                        ifacesAll = tempBoundAggTypeSym.GetIfacesAll();
                    }
                    else if (tempBoundTypeSym.IsTYVARSYM)
                    {
                        DebugUtil.Assert((tempBoundTypeSym as TYVARSYM).FResolved());
                        ifacesAll = (tempBoundTypeSym as TYVARSYM).AllInterfaces;
                    }
                    else
                    {
                        DebugUtil.Assert(inherited);
                        continue;
                    }

                    // Add everything in ifacesAll.
                    for (int j = ifacesAll.Count; --j >= 0; )
                    {
                        AGGTYPESYM atsChild = ifacesAll[j] as AGGTYPESYM;
                        DebugUtil.Assert(atsChild.IsInterfaceType());

                        if (!BSYMMGR.FindAts(atsChild, interfaceList))
                        {
                            //DebugUtil.Assert(pifaceMin < piface);
                            //*--piface = atsChild;
                            interfaceList.Insert(0, atsChild);
                        }
                    }

                    if (tempBoundTypeSym.IsAGGTYPESYM)
                    {
                        DebugUtil.Assert(!BSYMMGR.FindAts(tempBoundTypeSym as AGGTYPESYM, interfaceList));
                        //DebugUtil.Assert(pifaceMin < piface);
                        //*--piface = tempBoundTypeSym.asAGGTYPESYM();
                        interfaceList.Insert(0, tempBoundTypeSym as AGGTYPESYM);
                    }
                }

                ifacesAll = MainSymbolManager.AllocParams(
                    interfaceList.ConvertAll<TYPESYM>(SystemConverter.AggTypeSymToTypeSym));
            }
            else
            {
                ifacesAll = BSYMMGR.EmptyTypeArray;
            }

            tyVarSym.AllInterfaces = ifacesAll;
            tyVarSym.IsResolving = false;
            DebugUtil.Assert(tyVarSym.FResolved());

            tyVarSym.AggState = AggStateEnum.None;
            ComputeAggState(tyVarSym);

            return true;
        }

        //------------------------------------------------------------
        // COMPILER.IsBaseAggregate
        //
        /// <summary>
        /// Determine if a class/struct/interface (base) is
        /// a base of another class/struct/interface (derived),
        /// considering only the head aggregate names.
        /// Object is NOT considered a base of an interface
        /// but is considered as base of a struct.
        /// This is used for visibility rules only.
        /// </summary>
        /// <param name="derivedSym"></param>
        /// <param name="baseSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsBaseAggregate(AGGSYM derivedSym, AGGSYM baseSym)
        {
            DebugUtil.Assert(!derivedSym.IsEnum && !baseSym.IsEnum);

            EnsureState(derivedSym, AggStateEnum.Inheritance);
            EnsureState(baseSym, AggStateEnum.Inheritance);

            if (derivedSym == baseSym)
            {
                return true;    // identity.
            }

            if (baseSym.IsInterface)
            {
                // Search the direct and indirect interfaces via ifacesAll,
                // going up the baseSym chain...
                while (derivedSym != null)
                {
                    for (int i = 0; i < derivedSym.AllInterfaces.Count; ++i)
                    {
                        AGGTYPESYM iface = derivedSym.AllInterfaces[i] as AGGTYPESYM;
                        if (iface.GetAggregate() == baseSym) return true;
                    }
                    derivedSym = derivedSym.GetBaseAgg();
                }
                return false;
            }

            // baseSym is a class. Just go up the baseSym class chain to look for it.
            while (derivedSym.BaseClassSym != null)
            {
                derivedSym = derivedSym.BaseClassSym.GetAggregate();
                if (derivedSym == baseSym) return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.IsBaseType (1)
        //
        /// <summary>
        /// Determine if a class/struct/interface (baseAggTypeSym) is a base of
        /// another class/struct/interface (derivedAggTypeSym).
        /// Object IS considered a base of an interface or struct.
        /// This operation takes into account "generic inheritance",
        /// e.g. class Foo&lt;T&gt; : Baz&lt;List&lt;T&gt;&gt;
        /// </summary>
        /// <param name="derivedAggTypeSym"></param>
        /// <param name="baseAggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsBaseType(AGGTYPESYM derivedAggTypeSym, AGGTYPESYM baseAggTypeSym)
        {
            EnsureState(derivedAggTypeSym, AggStateEnum.Inheritance);
            EnsureState(baseAggTypeSym, AggStateEnum.Inheritance);

            // One nice optimization to have would be to just return false
            // if the type is marked as sealed.  
            // This is not possible because in the refactoring scenario
            // we may have code that derives from sealed types.

            // ResolveInheritance will give an error
            // if we derive from a sealed type, for types defined in source.
            // Imported types that are derived from a sealed type are treated as bogus.  
            // IsBaseType(), then, will simply return wether or not it is specified as the base type,
            // not whether or not it is a _valid_ base type.


            // The test for object is more than an optimization. It makes this return true
            // when derivedAggTypeSym is an interface.
            if (derivedAggTypeSym == baseAggTypeSym ||
                baseAggTypeSym.IsPredefType(PREDEFTYPE.OBJECT))
            {
                return true;
            }

            if (baseAggTypeSym.GetAggregate().IsInterface)
            {
                // Search the direct and indirect interfaces via ifacesAll, going up the base chain...
                while (derivedAggTypeSym != null)
                {
                    TypeArray allInterfaces = derivedAggTypeSym.GetIfacesAll();
                    for (int i = 0; i < allInterfaces.Count; ++i)
                    {
                        if ((allInterfaces[i] as AGGTYPESYM) == baseAggTypeSym)
                        {
                            return true;
                        }
                    }
                    derivedAggTypeSym = derivedAggTypeSym.GetBaseClass();
                }
                return false;
            }

            // Base is a class. Just go up the base class chain to look for it.
            while ((derivedAggTypeSym = derivedAggTypeSym.GetBaseClass()) != null)
            {
                if (derivedAggTypeSym == baseAggTypeSym)
                    return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.IsBaseType (2)
        //
        /// <summary>
        /// Returns true if derivedTypeSym has baseTypeSym as a base type.
        /// Implemented interfaces are considered to be base types.
        /// Also, array covariance is considered:
        /// object[] is considered to be a base type of string[].
        /// </summary>
        /// <param name="derivedTypeSym"></param>
        /// <param name="baseTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsBaseType(TYPESYM derivedTypeSym, TYPESYM baseTypeSym)
        {
            AGGSYM ilistAggSym;

        LAgain:
            EnsureState(derivedTypeSym, AggStateEnum.Inheritance);
            EnsureState(baseTypeSym, AggStateEnum.Inheritance);

            switch (derivedTypeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    if (derivedTypeSym == baseTypeSym)
                    {
                        return true;
                    }
                    return (
                        baseTypeSym.IsAGGTYPESYM &&
                        IsBaseType(derivedTypeSym as AGGTYPESYM, baseTypeSym as AGGTYPESYM));

                case SYMKIND.TYVARSYM:
                    if (derivedTypeSym == baseTypeSym ||
                        baseTypeSym.IsPredefType(PREDEFTYPE.OBJECT))
                    {
                        return true;
                    }
                    // Object is handled above.
                    TypeArray boundArray = (derivedTypeSym as TYVARSYM).BoundArray;
                    for (int i = 0; i < boundArray.Count; ++i)
                    {
                        if (IsBaseType(boundArray[i], baseTypeSym))
                        {
                            return true;
                        }
                    }
                    return false;

                case SYMKIND.NUBSYM:
                    if (derivedTypeSym == baseTypeSym)
                    {
                        return true;
                    }
                    derivedTypeSym = (derivedTypeSym as NUBSYM).GetAggTypeSym();
                    if (derivedTypeSym != null)
                    {
                        goto LAgain;
                    }
                    return false;

                case SYMKIND.ARRAYSYM:
                    if (derivedTypeSym == baseTypeSym)
                    {
                        return true;
                    }

                    // Handle IList<T> and its base interfaces.
                    // NOTE: This code assumes that any base interface of IList<T> has arity one
                    // and is instantiated at T. When this code was written (May, 2004) we had
                    //     IList<T> : ICollection<T> : IEnumerable<T>
                    // so this assumption is/was true.

                    if ((derivedTypeSym as ARRAYSYM).Rank == 1 &&
                        baseTypeSym.IsInterfaceType() &&
                        (baseTypeSym as AGGTYPESYM).AllTypeArguments.Count == 1 &&
                        (ilistAggSym = GetOptPredefAgg(PREDEFTYPE.G_ILIST, false)) != null &&
                        IsBaseAggregate(ilistAggSym, (baseTypeSym as AGGTYPESYM).GetAggregate()))
                    {
                        derivedTypeSym = (derivedTypeSym as ARRAYSYM).ElementTypeSym;
                        baseTypeSym = (baseTypeSym as AGGTYPESYM).AllTypeArguments[0];
                        if (derivedTypeSym == baseTypeSym)
                        {
                            return true;
                        }
                        // Recurse if derivedTypeSym is a reference type.
                        // Note: If both types are type variables and derivedTypeSym
                        // has baseTypeSym as a constraint,
                        // this will work even if baseTypeSym.IsRefType() returns false....
                        if (!derivedTypeSym.IsReferenceType())
                        {
                            return false;
                        }
                    }
                    else if (baseTypeSym.IsARRAYSYM)
                    {
                        if ((derivedTypeSym as ARRAYSYM).Rank != (baseTypeSym as ARRAYSYM).Rank)
                        {
                            return false;
                        }
                        derivedTypeSym = (derivedTypeSym as ARRAYSYM).ElementTypeSym;
                        baseTypeSym = (baseTypeSym as ARRAYSYM).ElementTypeSym;
                        // Recurse if derivedTypeSym is a reference type.
                        // Note: If both types are type variables and derivedTypeSym
                        // has baseTypeSym as a constraint,
                        // this will work even if baseTypeSym.IsRefType() returns false....
                        if (!derivedTypeSym.IsReferenceType())
                        {
                            return false;
                        }
                    }
                    else
                    {
                        derivedTypeSym = GetReqPredefType(PREDEFTYPE.ARRAY, false);
                    }

                    // Tail recursion.
                    goto LAgain;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // COMPILER.CheckBogus
        //
        /// <summary>
        /// <para>If sym.IsBogus is already set, return it.
        /// Otherwise, call CheckBogusCore method.</para>
        /// <para>If sym.IsBogus is not set,
        /// CheckBogusCore check its parent sym or base type sym
        /// or return type sym and type arguments and so on by kind.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckBogus(SYM sym)
        {
            bool br = false;

            if (sym == null)
            {
                return false;
            }
            if (sym.HasBogus)
            {
                return sym.IsBogus;
            }
            return CheckBogusCore(sym, this.aggStateMax < AggStateEnum.Prepared, ref br);
        }

        //------------------------------------------------------------
        // COMPILER.CheckBogusNoEnsure
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckBogusNoEnsure(SYM sym)
        {
            bool br = false;

            if (sym == null)
            {
                return false;
            }
            if (sym.HasBogus)
            {
                return sym.IsBogus;
            }
            return CheckBogusCore(sym, true, ref br);
        }

        //------------------------------------------------------------
        // COMPILER.ReportStaticClassError
        //
        /// <summary>
        // Generates an error for static classes
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="err"></param>
        //------------------------------------------------------------
        internal void ReportStaticClassError(
            BASENODE tree,
            SYM context,
            TYPESYM type,
            CSCERRID err)
        {
            if (context != null)
            {
                Error(tree, err, new ErrArg(type), new ErrArgRef(context));
            }
            else
            {
                Error(tree, err, new ErrArg(type));
            }
        }

        //------------------------------------------------------------
        // COMPILER.CheckForStaticClass
        //
        /// <summary>
        /// <para>Generate an error if type is static.</para>
        /// <para>If argument type is not static, only return false.
        /// If static, show an error message.</para>
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckForStaticClass(
            BASENODE tree,
            SYM context,
            TYPESYM type,
            CSCERRID err)
        {
            if (!type.IsStaticClass())
            {
                return false;
            }
            ReportStaticClassError(tree, context, type, err);
            return true;
        }

        //------------------------------------------------------------
        // COMPILER.GetReqPredefAgg
        //
        /// <summary>
        /// Get one of the required predefined AGGSYMs. Optionally calls EnsureState.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="ensureState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetReqPredefAgg(PREDEFTYPE pt, bool ensureState) // = true);
        {
            AGGSYM aggSym = MainSymbolManager.GetReqPredefAgg(pt);
            DebugUtil.Assert(aggSym != null);

            if (ensureState && aggSym != null && !aggSym.IsSource)
            {
                EnsureState(aggSym, AggStateEnum.Prepared);
            }
            return aggSym;
        }

        //------------------------------------------------------------
        // COMPILER.GetReqPredefType
        //
        /// <summary>
        /// <para>(In sscli, the default value of endureState is true.)</para>
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="ensureState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetReqPredefType(
            PREDEFTYPE pt,
            bool ensureState)    // = true
        {
            AGGSYM aggSym = MainSymbolManager.GetReqPredefAgg(pt);
            if (aggSym == null)
            {
                DebugUtil.VsFail("Required predef type missing");
                return null;
            }

            AGGTYPESYM ats = aggSym.GetThisType();
            if (ensureState && !aggSym.IsSource)
            {
                EnsureState(ats, AggStateEnum.Prepared);
            }
            return ats;
        }

        //------------------------------------------------------------
        // COMPILER.GetOptPredefAgg
        //
        /// <summary>
        /// <para>Get one of the optional predefined AGGSYMs.
        /// Optionally calls EnsureState if the aggSym exists.</para>
        /// <para>(ensureState has the default value true in sscli.)</para>
        /// </summary>
        //------------------------------------------------------------
        internal AGGSYM GetOptPredefAgg(PREDEFTYPE pt, bool ensureState)    // = true);
        {
            AGGSYM aggSym = MainSymbolManager.GetOptPredefAgg(pt);

            if (ensureState && aggSym != null && !aggSym.IsSource)
            {
                EnsureState(aggSym, AggStateEnum.Prepared);
            }
            return aggSym;
        }

        //------------------------------------------------------------
        // COMPILER.GetOptPredefType
        //
        /// <summary>
        /// <para>Call BSYMMGR.GetOptPredefAgg method
        /// to get AGGSYM instance for a given PREDEFTYPE value.</para>
        /// <para>(In sscli, ensureState has the default value true.)</para>
        /// <para></para>
        /// </summary>
        /// <param name="predefType"></param>
        /// <param name="ensureState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetOptPredefType(
            PREDEFTYPE predefType,
            bool ensureState)   //  = true
        {
            AGGSYM aggSym = this.MainSymbolManager.GetOptPredefAgg(predefType);
            if (aggSym == null)
            {
                return null;
            }

            AGGTYPESYM aggTypeSym = aggSym.GetThisType();
            if (ensureState && !aggSym.IsSource)
            {
                EnsureState(aggTypeSym, AggStateEnum.Prepared);
            }
            return aggTypeSym;
        }

        //------------------------------------------------------------
        // COMPILER.GetOptPredefAggErr
        //
        /// <summary>
        /// <para>Get one of the optional predefined AGGSYMs.
        /// If the agg doesn't exist, generates an error.
        /// Optionally calls EnsureState if the agg exists.</para>
        /// <para>(In sscli, ensureState has the default value true.)</para>
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="ensureState"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetOptPredefAggErr(
            PREDEFTYPE pt,
            bool ensureState)	// = true
        {
            DebugUtil.Assert(pt >= 0 && pt < PREDEFTYPE.COUNT);

            AGGSYM agg = this.MainSymbolManager.GetOptPredefAgg(pt);
            if (agg == null)
            {
                MainSymbolManager.ReportMissingPredefTypeError(pt);
                return null;
            }

            if (ensureState && !agg.IsSource)
            {
                EnsureState(agg, AggStateEnum.Prepared);
            }
            return agg;
        }

        //------------------------------------------------------------
        // COMPILER.GetOptPredefTypeErr
        //
        /// <summary>
        /// <para>(Arguemnt ensureState has the default value true in sscli.)</para>
        /// </summary>
        //------------------------------------------------------------
        internal AGGTYPESYM GetOptPredefTypeErr(
            PREDEFTYPE predefType,
            bool ensureState)   // = true
        {
            AGGSYM agg = MainSymbolManager.GetOptPredefAgg(predefType);
            if (agg == null)
            {
                MainSymbolManager.ReportMissingPredefTypeError(predefType);
                return null;
            }

            AGGTYPESYM ats = agg.GetThisType();
            if (ensureState && !agg.IsSource)
            {
                EnsureState(ats, AggStateEnum.Prepared);
            }
            return ats;
        }

        //------------------------------------------------------------
        // COMPILER.CanAggSymBeDeclared
        //
        /// <summary>
        /// Using the same logic as ForceAggStates, determine if
        /// declaring the type could be expected to be something that
        /// could succeed. This returns false only if a type was referenced
        /// by metadata, but wasn't defined by metadata or source.
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CanAggSymBeDeclared(AGGSYM sym)
        {
            if (sym.IsPrepared)
            {
                return true;
            }
            if (sym.HasParseTree)
            {
                return true;
            }
            if (sym.IsSource)
            {
                return true;
            }
            //if (TypeFromToken(sym.tokenImport) == mdtTypeDef)
            if (sym.Type != null)
            {
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.UndeclarableType
        //
        /// <summary>
        /// <para>Create an error message by an UNRESAGGSYM instance, and show it.</para>
        /// </summary>
        /// <param name="unresAggSym"></param>
        //------------------------------------------------------------
        internal void UndeclarableType(UNRESAGGSYM unresAggSym)
        {
            if (!unresAggSym.SuppressError)
            {
                // type was imported, but we have no definition for it
                LOCATION location = this.Location;

                // find last spot which wasn't imported metadata
                BASENODE node = null;
                INFILESYM file = null;
                if (this.Location != null)
                {
                    while (
                        location != null &&
                        location.GetFile() != null &&
                        !location.GetFile().IsSource)
                    {
                        location = location.PreviousLocation;
                    }

                    if (location != null)
                    {
                        node = location.GetNode();
                        file = location.GetFile();
                    }
                }

                // Get the assembly this would be found in.
                //string assemblyName = null;
                //importer.GetTypeRefAssemblyName(
                //    unresAggSym.ModuleErr, unresAggSym.TokenErr, out assemblyName);
                string assemblyName = unresAggSym.AssemblyName;

                // try and generate a good error message for it
                CError pError = MakeError(
                    (node != null && file != null) ? node : null,
                    CSCERRID.ERR_NoTypeDef,
                    new ErrArgRef(unresAggSym),
                    new ErrArg(assemblyName));

                // dump all intervening import symbols on the location stack
                // to show the dependency chain which caused the importing of the unknown symbol
                LOCATION currentLocation = this.Location;
                SYM currentSymbol = null;
                while (currentLocation != location)
                {
                    SYM topSymbol = currentLocation.GetSymbol();
                    if (topSymbol != null && topSymbol != currentSymbol)
                    {
                        currentSymbol = topSymbol;
                        if (currentSymbol != unresAggSym)
                        {
                            AddRelatedSymLoc(pError, topSymbol);
                        }
                    }
                    currentLocation = currentLocation.PreviousLocation;
                }

                // Submit the error
                SubmitError(pError);
            }

            // fake up the type so that it looks reasonable
            // for the rest of the compile
            DebugUtil.Assert(unresAggSym.AggState == AggStateEnum.None);

            unresAggSym.AggState = AggStateEnum.PreparedMembers;
            unresAggSym.AggKind = AggKindEnum.Class;

            if (!unresAggSym.IsPredefAgg(PREDEFTYPE.OBJECT))
            {
                DebugUtil.Assert(
                    unresAggSym.BaseClassSym == null &&
                    unresAggSym.InterfaceCount == 0 &&
                    unresAggSym.InterfaceCountAll== 0);

                SetBaseType(unresAggSym, GetReqPredefType(PREDEFTYPE.OBJECT, false));
                SetIfaces(unresAggSym, new AGGTYPESYM[0]);
            }
        }

        //------------------------------------------------------------
        // COMPILER.GetPredefIndex
        //
        /// <summary>
        /// <para>If not found, return -1.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal PREDEFTYPE GetPredefIndex(TYPESYM typeSym)
        {
            if (typeSym.IsPTRSYM)
            {
                return PREDEFTYPE.UINTPTR;
            }
            else if (typeSym.IsAGGTYPESYM && typeSym.IsPredefined())
            {
                PREDEFTYPE pt = typeSym.GetPredefType();
                if (typeSym.IsSimpleType() ||
                    pt == PREDEFTYPE.INTPTR ||
                    pt == PREDEFTYPE.UINTPTR)
                {
                    return pt;
                }
            }
            return PREDEFTYPE.UNDEFINED;	// UNDEFINEDINDEX;
        }

        //------------------------------------------------------------
        // COMPILER.FCanLift
        //
        /// <summary>
        /// True if Nullable type is available.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FCanLift()
        {
            return (GetOptPredefAgg(PREDEFTYPE.G_OPTIONAL, false) != null);
        }

        // Refactoring

        //------------------------------------------------------------
        // COMPILER.CompileCallback
        //------------------------------------------------------------
        private CCompileCallbackForward compileCallback = new CCompileCallbackForward();

        internal CCompileCallbackForward CompileCallback
        {
            get { return compileCallback; }
        }

        // Edit And Continue

        //------------------------------------------------------------
        // COMPILER.CreateCeeFileGen
        //
        // Loads mscorpe.dll and gets an ICeeFileGen interface from it.
        // The ICeeFileGen interface is used for the entire compile.
        //
        // mscorpe.dll exports two functions.
        //     HRESULT CreateICeeFileGen([out] ICeeFileGen**)
        //     HRESULT DestroyICeeFileGen([in] ICeeFileGen**)
        // ICeeFileGen object creates CLR portable executable(PE).
        //------------------------------------------------------------
        //ICeeFileGen* CreateCeeFileGen()
        //{
        //    // Dynamically bind to ICeeFileGen functions.
        //    if (!pfnCreateCeeFileGen || !pfnDestroyCeeFileGen) {
        //
        //        HRESULT hr = LoadLibraryShim(MSCORPE_NAME, NULL, NULL, &hmodCorPE);
        //        if (SUCCEEDED(hr) && hmodCorPE) {
        //            // Get the required methods.
        //            pfnCreateCeeFileGen  =
        //                (HRESULT (__stdcall *)(ICeeFileGen **ceeFileGen)) GetProcAddress(hmodCorPE, "CreateICeeFileGen");
        //            pfnDestroyCeeFileGen =
        //                (HRESULT (__stdcall *)(ICeeFileGen **ceeFileGen)) GetProcAddress(hmodCorPE, "DestroyICeeFileGen");
        //            if (!pfnCreateCeeFileGen || !pfnDestroyCeeFileGen)
        //                Error(NULL, FTL_ComPlusInit, ErrGetLastError());
        //        }
        //        else {
        //            // MSCorPE.DLL wasn't found.
        //            Error(NULL, FTL_RequiredFileNotFound, MSCORPE_NAME);
        //        }
        //    }
        //
        //    ICeeFileGen *ceefilegen = NULL;
        //    HRESULT hr = pfnCreateCeeFileGen(& ceefilegen);
        //    if (FAILED(hr)) {
        //        Error(NULL, FTL_ComPlusInit, ErrHR(hr));
        //    }
        //
        //    return ceefilegen;
        //}

        //------------------------------------------------------------
        // COMPILER.DestroyCeeFileGen
        //------------------------------------------------------------
        //void DestroyCeeFileGen(ICeeFileGen *ceefilegen)
        //{
        //    pfnDestroyCeeFileGen(&ceefilegen);
        //}

        //private:

        // General compilation.

        //------------------------------------------------------------
        // COMPILER.CompileAll
        //
        /// <summary>
        /// Do the full compile.
        /// The main process of compilation after all options have been accumulated.
        /// </summary>
        /// <param name="progressSink"></param>
        //------------------------------------------------------------
        internal void CompileAll(CCompileProgress progressSink)
        {
            // sccomp\compiler.h(1086):
            // #define SETLOCATIONSTAGE(stage)
            //   STAGELOCATION   _stageLocation(&compiler().location,   (COMPILER::stage));

            SetLocation(STAGE.BEGIN); //SETLOCATIONSTAGE(BEGIN);

            //OUTFILESYM assemblyOutfileSym = null;
            OUTFILESYM outputFileSym = null;
            SourceOutFileIterator outfileIterator = new SourceOutFileIterator();    //files;
            SourceFileIterator infileIterator = new SourceFileIterator();
            INFILESYM infileSym = null;
            bool br = true;
            Exception excp = null;

            //this.CurrentPEFile = null;
            this.CurrentOutfileSym = null;
            this.AssemblyID = AssemblyUtil.AssemblyIsUBM;
#if DEBUG
            this.HaveDefinedAnyType = false;
#endif
            CErrorSuppression es = new CErrorSuppression();

            if (!AllowCLSErrors())
            {
                checkCLS = false;
            }

            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.None);
            compilationPhase = CompilerPhaseEnum.Init;
            DebugUtil.Assert(this.aggStateMax == AggStateEnum.None);

            int oldErrors = ErrorCount();
            isCanceled = false;
            int iFile;

            // We have to check this here
            // because the moduleassemblyname flag is set per compilation
            // but the output files are set per inputset.
            if (this.OptionManager.ModuleAssemblyName != null && BuildAssembly)
            {
                Error(CSCERRID.ERR_AssemblyNameOnNonModule);
                goto ENDCOMPILE;
            }

            // Clear any existing error info object, so we don't report stale errors.
            //SetErrorInfo(0, null);

            // Add the standard metadata to the list of files to process.
            AddStandardMetadata();

            //--------------------------------------------------------
            // declare all the input files
            //
            // adds symbol table entries for all namespaces and
            // user defined types (classes, enums, structs, interfaces, delegates)
            // sets access modifiers on all user defined types
            //--------------------------------------------------------
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.Init);
            compilationPhase = CompilerPhaseEnum.DeclareTypes;

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "Declare");

            // Parse all the source files (not parse method body at this point),
            // and create SYM instances of the namespaces and the aggregates.
            DeclareTypes(progressSink);
            ReportProgress(progressSink, COMPILE_PHASE_ENUM.DECLARETYPES, 1, 1);
#if DEBUG
            debugSb.Length = 0;
            DebugUtil.DebugNodesOutput(debugSb);
            debugSb.Length = 0;
            DebugUtil.DebugSymsOutput(debugSb);
            debugSb.Length = 0;
            this.MainSymbolManager.GlobalSymbolTable.Debug(debugSb);
#endif
            if (FAbortEarly(oldErrors, es))
            {
                goto ENDCOMPILE;
            }

            // Initialize the alinker by getting the common dispenser
            //GetMetadataDispenser();

            //--------------------------------------------------------
            // Import meta-data.
            //--------------------------------------------------------
            SetLocation(STAGE.PARSE);  //SETLOCATIONSTAGE(PARSE);

            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.DeclareTypes);
            compilationPhase = CompilerPhaseEnum.ImportTypes;

            this.Importer.ImportAllTypes();
            ReportProgress(progressSink, COMPILE_PHASE_ENUM.IMPORTTYPES, 1, 1);
#if DEBUG
            debugSb.Length = 0;
            DebugUtil.DebugSymsOutput(debugSb);
            debugSb.Length = 0;
            this.MainSymbolManager.GlobalSymbolTable.Debug(debugSb);
#endif
            if (FAbortEarly(oldErrors, es))
            {
                goto ENDCOMPILE;
            }

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "Import");

            //--------------------------------------------------------
            // Initialize predefined types. This is done after a declaration
            // for every predefined type has already been seen.
            //--------------------------------------------------------
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.ImportTypes);
            compilationPhase = CompilerPhaseEnum.InitPredefTypes;
            if (!MainSymbolManager.InitPredefinedTypes() ||
                FAbortEarly(oldErrors, es))
            {
                goto ENDCOMPILE;
            }
            this.MainSymbolManager.InitFundamentalTypes();
            this.MainSymbolManager.CreateDynamicSym();  // CS4

            //--------------------------------------------------------
            // resolves all using clauses
            // resolves all base classes and implemented interfaces
            //--------------------------------------------------------
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.InitPredefTypes);
            compilationPhase = CompilerPhaseEnum.ResolveInheritance;
            DebugUtil.Assert(this.aggStateMax == AggStateEnum.None);
            this.aggStateMax = AggStateEnum.Inheritance;

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "Inheritance");

            ResolveInheritanceHierarchy();
#if DEBUG
            debugSb.Length = 0;
            DebugUtil.DebugSymsOutput(debugSb);
#endif
            MainSymbolManager.InitAuxiliaryTypes();

            //--------------------------------------------------------
            // define bounds on all type parameters on types
            //--------------------------------------------------------
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.ResolveInheritance);
            compilationPhase = CompilerPhaseEnum.DefineBounds;
            DebugUtil.Assert(this.aggStateMax == AggStateEnum.Inheritance);
            this.aggStateMax = AggStateEnum.Bounds;

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "DefineBounds");

            DefineBounds();
#if DEBUG
            debugSb.Length = 0;
            DebugUtil.DebugSymsOutput(debugSb);
#endif

            // cannot define any types until the inheritance hierarchy is resolved
            //
            // if this assert fires then set a breakpoint at the 2 locations
            // where this variable is set to true and rerun your build
#if DEBUG
            DebugUtil.Assert(!this.HaveDefinedAnyType);
#endif
            //--------------------------------------------------------
            // Add assmbly attributes which specified by options.
            //--------------------------------------------------------
            this.OptionManager.AddAssemblyAttributes(this);

            //--------------------------------------------------------
            // define all members of types
            //--------------------------------------------------------
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.DefineBounds);
            compilationPhase = CompilerPhaseEnum.DefineMembers;
            DebugUtil.Assert(this.aggStateMax == AggStateEnum.Bounds);

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "DefineMembers");

            DefineMembers();
#if DEBUG
            debugSb.Length = 0;
            DebugUtil.DebugSymsOutput(debugSb);
#endif

            // DefineMembers changes aggSymStateMax
            DebugUtil.Assert(this.aggStateMax == AggStateEnum.DefinedMembers);
            ReportProgress(progressSink, COMPILE_PHASE_ENUM.DEFINE, 1, 1);

            //--------------------------------------------------------
            // Evaluate constants and compile attributes.
            //--------------------------------------------------------
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.DefineMembers);
            compilationPhase = CompilerPhaseEnum.EvalConstants;

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "EvaluateConstants");

            this.AssemblyInitialAttributes = new CAssemblyInitialAttributes();
            EvaluateConstants();

            if (FAbortEarly(oldErrors, es))
            {
                goto ENDCOMPILE;
            }

            //--------------------------------------------------------
            // prepare all the input files
            //
            // evaluates constants
            // checks field & method modifiers between classes (overriding & shadowing)
            //--------------------------------------------------------
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.EvalConstants);
            compilationPhase = CompilerPhaseEnum.Prepare;

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "PrepareNamespace");

            iFile = 0;
            for (infileSym = infileIterator.Reset(this);
                infileSym != null;
                infileSym = infileIterator.Next())
            {
                SetLocation(STAGE.PREPARE);
                SetLocation(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.PrepareNamespace(infileSym.RootNsDeclSym);
                }

                if (ReportProgress(
                        progressSink,
                        COMPILE_PHASE_ENUM.PREPARE,
                        ++iFile,
                        this.InputFileCount))
                {
                    goto ENDCOMPILE;
                }
            }
#if DEBUG
            debugSb.Length = 0;
            DebugUtil.DebugSymsOutput(debugSb);
#endif

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "CheckForTypeErrors");

            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.Prepare);
            compilationPhase = CompilerPhaseEnum.PostPrepare;
            DebugUtil.Assert(this.AggStateMax == AggStateEnum.DefinedMembers);
            this.aggStateMax = AggStateEnum.Last;

            //--------------------------------------------------------
            // Right after prepare we check all types used in members for things
            // like bogus, deprecated, constraints.
            //--------------------------------------------------------
            CheckForTypeErrors();
#if DEBUG
            debugSb.Length = 0;
            DebugUtil.DebugSymsOutput(debugSb);
#endif
            if (FAbortEarly(oldErrors, es))
            {
                goto ENDCOMPILE;
            }

            if (FEncBuild())
            {
                goto ENDCOMPILE;
            }

            //--------------------------------------------------------
            // This loop forces each file to find it's Main()
            // It also cause each output file to have a definite filename
            //--------------------------------------------------------
            for (outputFileSym = outfileIterator.Reset(this);
                outputFileSym != null;
                outputFileSym = outfileIterator.Next())
            {
                Emitter.FindEntryPoint(outputFileSym);
                if (outputFileSym.IsUnnamed)
                {
                    GlobalSymbolManager.SetOutFileName(outputFileSym.FirstInFileSym());
                }
            }

            if (FAbortEarly(oldErrors, es))
            {
                goto ENDCOMPILE;
            }

            //--------------------------------------------------------
            // initialize the assembly manifest emitter
            // even if we don't emit a manifest, we still emit scoped typerefs
            //--------------------------------------------------------
            // decide on an Assembly file and create it!

            // Get an OUTFILESYM instance whose IsManifest field is set.
            this.AssemblyOutfileSym = GetManifestOutFile();
            if (this.AssemblyOutfileSym == null)
            {
                SourceOutFileIterator files = new SourceOutFileIterator();

                // Just need the first one in the module building case.
                this.AssemblyOutfileSym = files.Reset(this);
                DebugUtil.Assert(this.AssemblyOutfileSym != null);
            }

            DebugUtil.Assert((!BuildAssembly) == (!this.AssemblyOutfileSym.IsManifest));
            this.AssemblyOutfileSym.FileID = Cor.mdTokenNil;
            this.AssemblyOutfileSym.AssemblyBuilderFlags =
                AssemblyFlagsEnum.NoDupTypeCheck |
                AssemblyFlagsEnum.NoRefHash |
                AssemblyFlagsEnum.DupeCheckTypeFwds;

            //----------------------------------------------------
            // Create the assemblyBuilder and ModuleBuilder instances.
            //----------------------------------------------------
            //this.AssemblyPEFile.BeginOutputFile(this, assemblyFileSym);
            //this.AssemblyOutfileSym.BeginOutputFile(this);

            OUTFILESYM tempAsmSym = this.AssemblyOutfileSym;

            for (OUTFILESYM outfile = GetFirstOutFile();
                outfile != null;
                outfile = outfile.NextOutputFile())
            {
                if (outfile.Target == TargetType.Module)
                {
                    outfile.BeginModuleOutputFile(this, tempAsmSym);
                    continue;
                }

                if (!outfile.IsManifest)
                {
                    continue;
                }

                outfile.BeginAssemblyOutputFile(this);
                tempAsmSym = outfile;

                Dictionary<SecurityAction, PermissionSet> permissionSets
                    = new Dictionary<SecurityAction, PermissionSet>();
                PermissionSet requiredSet;
                PermissionSet optionalSet;
                PermissionSet refusedSet;

                GlobalAttrBind.Compile(
                    this,
                    this.AssemblyOutfileSym,
                    this.AssemblyAttributes,
                    false,
                    permissionSets);

                if (!permissionSets.TryGetValue(SecurityAction.RequestMinimum, out requiredSet))
                {
                    requiredSet = null;
                }
                if (!permissionSets.TryGetValue(SecurityAction.RequestOptional, out optionalSet))
                {
                    optionalSet = null;
                }
                if (!permissionSets.TryGetValue(SecurityAction.RequestRefuse, out refusedSet))
                {
                    refusedSet = null;
                }

                if (!outfile.DefineAssembly(
                        this.OptionManager,
                        this.AssemblyInitialAttributes,
                        requiredSet,
                        optionalSet,
                        refusedSet,
                        out excp))
                {
                    this.Error(ERRORKIND.ERROR, excp);
                }
                outfile.CreateManifestModuleBuilder();
            }

            if (BuildAssembly)
            {
                if (Linker.SetAssemblyFile(this.AssemblyOutfileSym.AssemblyBuilderEx))
                {
                    Linker.SetPEKind(
                        PlatformPEKind[(int)OptionManager.Platform],
                        PlatformMachine[(int)OptionManager.Platform]);

                    //--------------------------------------------
                    // modules
                    //--------------------------------------------
                    InFileIterator infiles = new InFileIterator();
                    for (INFILESYM infile = infiles.Reset(MainSymbolManager.MetadataFileRootSym);
                        infile != null;
                        infile = infiles.Next())
                    {
                        if (infile.IsModule)
                        {
                            if (Linker.AddImport(infile))
                            {
                                CModuleEx modEx = infile.ModuleEx;
                                if (modEx != null)
                                {
                                    if (modEx.SetImageToAssemblyBuilder(
                                            this.AssemblyOutfileSym.AssemblyBuilderEx,
                                            false))
                                    {
                                        foreach (AGGSYM aggSym in modEx.AggSymList)
                                        {
                                            modEx.ReplaceReflectionInfo(aggSym);
                                        }
                                    }
                                }
                                else
                                {
                                    Error(CSCERRID.ERR_ALinkFailed, new ErrArg(infile.Name));
                                }
                            }
                            else
                            {
                                // If errors occured, they are reported in AddImport method.
                                //Error(CSCERRID.ERR_ALinkFailed, new ErrArg(infile.Name));
                            }
                        }
                    }

                    //--------------------------------------------
                    // exported types
                    //--------------------------------------------
                    if (this.IsFriendDeclared)
                    {
                        if (!Linker.EmitInternalExportedTypes())
                        {
                            Error(
                                CSCERRID.ERR_ALinkFailed,
                                new ErrArg("EmitInternalExportedTypes"));
                        }
                    }
                }
                else
                {
                    Error(
                        CSCERRID.ERR_ALinkFailed,
                        new ErrArg(this.AssemblyOutfileSym.Name));
                }
            }
            else // if (BuildAssembly)
            {
                //assemID = AssemblyIsUBM;
                //if (FAILED(hr = linker.SetNonAssemblyFlags(
                //    (AssemblyFlags)(afNoDupTypeCheck | afNoRefHash | afDupeCheckTypeFwds)))
                //    ||
                //    FAILED(hr = linker.AddFile(
                //        assemID,
                //        assemblyFileSym.name.text,
                //        0,
                //        this.AssemblyPEFile.GetEmit(),
                //        &assemblyFileSym.idFile))
                //    ||
                //    FAILED(hr = linker.SetPEKind(
                //        assemID,
                //        assemblyFileSym.idFile,
                //        rgPEKind[CompilerOptions.m_platform],
                //        rgMachine[CompilerOptions.m_platform])))
                //{
                //    Error( null,CSCERRID.CSCERRID.ERR_ALinkFailed, ErrHR(br, true));
                //}

                throw new NotImplementedException("initialize the assembly manifest emitter");
            } // if (BuildAssembly)

            if (FAbortEarly(oldErrors, es))
            {
                goto ENDCOMPILE;
            }
            DebugUtil.Assert(compilationPhase == CompilerPhaseEnum.PostPrepare);
            compilationPhase = CompilerPhaseEnum.CompileMembers;

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "Compile");

            //--------------------------------------------------------
            // compile all the input files
            //
            // emits all user defined types including fields, & method signatures
            // constant field values.
            //
            // compiles and emits all methods
            //
            // For each output file, emission of metadata happens in three phases in order to
            // have the metadata emitting work most efficiently. Each phase happens in the same
            // order.
            //   1. Emit typedefs for all types
            //   2. Emit memberdefs for all members
            //   3. Compile members and emit code, additional metadata (param tokens) onto the
            //      type and memberdefs emitted previously.
            //
            // Always does the assembly file first, then the other files.
            //--------------------------------------------------------
            Emitter.AllocTypeMemberMap();

            outfileIterator.Reset(this);
            if ((outputFileSym = this.AssemblyOutfileSym) == null)
            {
                outputFileSym = outfileIterator.Current;
                outfileIterator.Next();
            }

            InFileIterator iterInfiles = new InFileIterator();
            iFile = 0;
            do
            {
                //INFILESYM infileSym;
                //iterInfiles = new InFileIterator();
                //PEFile nonManifestOutputFile = new PEFile();

                SetLocation(STAGE.EMIT);
                this.CurrentOutfileSym = outputFileSym;

                //----------------------------------------------------
                // Create TypeBuilders, MethodBuilders, ...
                //----------------------------------------------------
                Emitter.BeginOutputFile(outputFileSym);
#if DEBUG
                debugSb.Length = 0;
                DebugUtil.DebugSymsOutput(debugSb);
#endif
                oldErrors = ErrorCount();
                EmitTokens(outputFileSym);

                if (oldErrors != ErrorCount())
                {
                    //goto NEXT_FILE;
                    goto ENDCOMPILE;
                }
                if (outfileIterator.Current != null)
                {
                    //outfileIterator.Next();
                    outputFileSym = outfileIterator.Next();
                }
            } while (outputFileSym != null);

            //--------------------------------------------------------
            // Emit Attributes to Assembly
            //--------------------------------------------------------
            if (this.AssemblyOutfileSym == outputFileSym)
            {
                // do assembly attributes

                GlobalAttrBind.Compile(
                    this,
                    this.AssemblyOutfileSym,
                    this.AssemblyAttributes,
                    false,
                    null);
                UnknownAttrBind.Compile(this, this.UnknownGlobalAttributes);

                //SetSigningOptions(this.AssemblyOutfileSym);
                if (this.BuildAssembly)
                {
                    CErrorSuppression esLoc = new CErrorSuppression();
                    if (!FAbortEarly(0, esLoc))
                    {
                        // Write the manifest and save space for the crypto-keys
                        //this.AssemblyPEFile.WriteCryptoKey();
                    }
                }
            }

            //--------------------------------------------------------
            // Compile
            //--------------------------------------------------------
            outfileIterator.Reset(this);
            if ((outputFileSym = this.AssemblyOutfileSym) == null)
            {
                outputFileSym = outfileIterator.Current;
                outfileIterator.Next();
            }

            iFile = 0;
            do
            {
                SetLocation(STAGE.COMPILE);
                this.CurrentOutfileSym = outputFileSym;

                for (infileSym = iterInfiles.Reset(outputFileSym);
                    infileSym != null;
                    infileSym = iterInfiles.Next())
                {
                    SetLocation(infileSym);
                    if (infileSym.RootNsDeclSym != null)
                    {
#if DEBUG
                        debugSb.Length = 0;
                        DebugUtil.DebugNodesOutput(debugSb);
                        debugSb.Length = 0;
                        DebugUtil.DebugSymsOutput(debugSb);
                        debugSb.Length = 0;
                        this.MainSymbolManager.GlobalSymbolTable.Debug(debugSb);
#endif
                        this.ClsDeclRec.CompileNamespace(infileSym.RootNsDeclSym);
                    }

                    if (ReportCompileProgress(progressSink, ++iFile, this.InputFileCount))
                    {
                        goto ENDCOMPILE;
                    }
                }

                if (outfileIterator.Current != null)
                {
                    //outfileIterator.Next();
                    outputFileSym = outfileIterator.Next();
                }
            } while (outputFileSym != null);

            //--------------------------------------------------------
            // Create types
            //--------------------------------------------------------
            outfileIterator.Reset(this);
            if ((outputFileSym = this.AssemblyOutfileSym) == null)
            {
                outputFileSym = outfileIterator.Current;
                outfileIterator.Next();
            }

            iFile = 0;
            do
            {
                this.CurrentOutfileSym = outputFileSym;
                Queue<AGGSYM> aggQueue = new Queue<AGGSYM>();

                for (infileSym = iterInfiles.Reset(outputFileSym);
                    infileSym != null;
                    infileSym = iterInfiles.Next())
                {
                    SetLocation(infileSym);
                    if (infileSym.RootNsDeclSym != null)
                    {
                        this.ClsDeclRec.CreateEnumTypesInNamespace(infileSym.RootNsDeclSym, aggQueue);
                    }

                    if (ReportCompileProgress(progressSink, ++iFile, this.InputFileCount))
                    {
                        goto ENDCOMPILE;
                    }
                }

                this.ClsDeclRec.CreateTypesInNamespace(aggQueue);

                outputFileSym.SetEntryPoint();
                this.Emitter.CreateGlobalType();

            NEXT_FILE: ;
                // Write the executable file if no errors occured.
                Emitter.EndOutputFile(!FAbortOutFile());
                if (outputFileSym != this.AssemblyOutfileSym)
                {
                    //this.CurrentPEFile.EndOutputFile(!FAbortOutFile());
                    outputFileSym.EndAssemblyOutputFile(!FAbortOutFile());
                    if (ReportProgress(progressSink, COMPILE_PHASE_ENUM.WRITEOUTPUT, 1, 1))
                    {
                        goto ENDCOMPILE;
                    }
                }
                //this.CurrentPEFile = null;
                if (outfileIterator.Current != null)
                {
                    //outfileIterator.Next();
                    outputFileSym = outfileIterator.Next();
                }
            } while (outputFileSym != null);

            DebugUtil.StopStopwatch(true);
            DebugUtil.StartStopwatch(true, "PostCompile");

            if (ErrorCount() == 0)  // && !Options.CompileSkeleton)
            {
                PostCompileChecks(); // Only give these warnings if we had no errors.
            }

            //--------------------------------------------------------
            // Add resources and save the assembly
            //--------------------------------------------------------
            if (this.AssemblyOutfileSym != null)
            {
                CAssemblyBuilderEx assemblyBuilerEx = this.AssemblyOutfileSym.AssemblyBuilderEx;

                //----------------------------------------------------
                // Include the resources
                //----------------------------------------------------
                //DebugUtil.Assert(this.CurrentPEFile == null);
                //this.CurrentPEFile = this.AssemblyPEFile;
                this.CurrentOutfileSym = this.AssemblyOutfileSym;
                if (ErrorCount() == 0)  // && !Options.CompileSkeleton)
                {
                    RESFILESYM res;

                    //------------------------------------------------
                    // Managed resources
                    //------------------------------------------------
                    for (outputFileSym = GetFirstOutFile();
                        outputFileSym != null;
                        outputFileSym = outputFileSym.NextOutputFile())
                    {
                        for (res = outputFileSym.FirstResourceFileSym();
                            res != null;
                            res = res.NextResfile())
                        {
                            assemblyBuilerEx.AddResource(res);
                        }
                    }

                    //------------------------------------------------
                    // Unmanaged resources
                    //------------------------------------------------
                    this.AssemblyOutfileSym.DefineUnmanagedEmbeddedResource();
                }

                if (BuildAssembly && !(br = Linker.PreCloseAssembly(this.AssemblyID)))
                {
                    Error(CSCERRID.ERR_ALinkFailed, new ErrArg("Linker.PreCloseAssembly"));
                }

                //----------------------------------------------------
                // Save the assembly.
                //----------------------------------------------------
                this.AssemblyOutfileSym.EndAssemblyOutputFile(!FAbortOutFile());

                if (ReportProgress(progressSink, COMPILE_PHASE_ENUM.WRITEOUTPUT, 1, 1))
                {
                    goto ENDCOMPILE;
                }

                {
                    CErrorSuppression esLoc = new CErrorSuppression();
                    if (!FAbortEarly(0, esLoc) && !(br = Linker.CloseAssembly(this.AssemblyID)))
                    {
                        Error(CSCERRID.ERR_ALinkCloseFailed, new ErrArg("Linker.CloseAssembly"));
                    }
                }

            }
            else // if (this.AssemblyOutfileSym != null)
            {
                // Error about including resources in non-Assembly
                RESFILESYM res;
                for (outputFileSym = GetFirstOutFile();
                    outputFileSym != null;
                    outputFileSym = outputFileSym.NextOutputFile())
                {
                    for (res = outputFileSym.FirstResourceFileSym();
                        res != null;
                        res = res.NextResfile())
                    {
                        Error(CSCERRID.ERR_CantRefResource, new ErrArg(res.Name));
                    }
                }
            }

            if (ReportProgress(progressSink, COMPILE_PHASE_ENUM.WRITEINCFILE, 1, 1))
            {
                //goto ENDCOMPILE;
            }
        ENDCOMPILE: ;
        }

        //------------------------------------------------------------
        // COMPILER.ParseOneFile
        //
        /// <summary>
        /// <para>Create SYMs of the namespaces, the classes, the structs, the interfaces,
        /// the delegates in the source files, and register them.
        /// Access modifiers are also parsed.</para>
        /// <list type="number">
        /// <item>Create a CSourceModule instance from argument infile.</item>
        /// <item>Create a CSourceData by CSourceModule.GetSourceData method.</item>
        /// <item>Call CSourceData.ParseTopLevel(out BASENODE) method to parse.
        /// The resulting node tree is set to the argument.</item>
        /// <item>Set the resulting node tree to INFILESYM.NamespaceNode
        /// and set the CSourceData instance to INFILESYM.SourceData.</item>
        /// </list>
        /// </summary>
        /// <param name="infile"></param>
        //------------------------------------------------------------
        internal void ParseOneFile(INFILESYM infile)
        {
            CSourceModule sourceModule = null;
            CSourceData sourceData = null;
            //HRESULT hr;
            //bool br = false;

            SetLocation(STAGE.PARSE);  //SETLOCATIONSTAGE(PARSE);
            SetLocation(infile);       //SETLOCATIONFILE(infile);

            // Read the source file of infile and
            // create CSourceModule instance and CSourceData instance.
            sourceModule = this.Controller.GetSourceModule(
                infile.FileInfo,
                this.OptionManager.GenerateDebugInfo);
            sourceData = sourceModule.GetSourceData(true);
            if (sourceData == null)
            {
                Controller.ReportError(
                    ERRORKIND.ERROR,
                    String.Format(
                        "ParseOneFile: Failed to create a SourceData of {0}.",
                        infile.Name));
                return;
            }

            BASENODE baseNode;
            //CErrorContainer errorContainer;

            // Build the top-level parse tree.  Note that this may already be done...
            // CSourceData.ParseTopLevel only calls CSourceModule.ParseTopLevel.
            sourceData.ParseTopLevel(out baseNode);

            // Get any tokenization errors that may have occurred and send them to the host.
            Controller.ReportErrors(sourceData.GetErrors(ERRORCATEGORY.TOKENIZATION));

            // Same for top-level parse errors
            Controller.ReportErrors(sourceData.GetErrors(ERRORCATEGORY.TOPLEVELPARSE));

            infile.NamespaceNode = baseNode as NAMESPACENODE;
            infile.SourceData = sourceData;
            // NOTE:  Ref ownership transferred here...
        }

        //------------------------------------------------------------
        // COMPILER.DeclareOneFile
        //
        /// <summary>
        /// <para>Call ParseOneFile.
        /// Create SYMs of namespaces and user defined type in the source files.</para>
        /// </summary>
        /// <param name="infile"></param>
        //------------------------------------------------------------
        internal void DeclareOneFile(INFILESYM infile)
        {
            // do parsing on demand
            // The parse tree is set to INFILESYM.NamespaceNode.
            ParseOneFile(infile);

            if (infile.NamespaceNode != null)
            {
                SetLocation(infile);           //SETLOCATIONFILE(infile);
                SetLocation(STAGE.DECLARE);    //SETLOCATIONSTAGE(DECLARE);
#if DEBUG
                debugSb.Length = 0;
                infile.SourceData.LexData.DebugTokenList(debugSb);
                debugSb.Length = 0;
                DebugUtil.DebugNodesOutput(debugSb);
                debugSb.Length = 0;
                DebugUtil.DebugSymsOutput(debugSb);
#endif
                // infile.NamespaceNode is the root node of the parse tree.
                // Create NSSYMs, NSDECLSYMs, AGGSYMs, AGGDECLSYMs
                // from namespaces and types defined in the source.
                // Register NSSYMs and AGGSYMs to this.MainSymbolManager.GlobalSymbolTable.
                ClsDeclRec.DeclareInputfile(infile.NamespaceNode, infile);
#if DEBUG
                debugSb.Length = 0;
                DebugUtil.DebugSymsOutput(debugSb);
#endif
            }
        }

        //------------------------------------------------------------
        // COMPILER.ResolveInheritanceHierarchy
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ResolveInheritanceHierarchy()
        {
            // do object first to avoid special cases later
            AGGSYM sym = GetReqPredefAgg(PREDEFTYPE.OBJECT, false);
            if (sym.Interfaces == null)
            {
                sym.Interfaces = BSYMMGR.EmptyTypeArray;
                sym.AllInterfaces = BSYMMGR.EmptyTypeArray;
            }
            DebugUtil.Assert(sym.AggState == AggStateEnum.Declared);
            sym.AggState = AggStateEnum.Inheritance;

            SourceFileIterator srcFiles = new SourceFileIterator();
            for (INFILESYM infileSym = srcFiles.Reset(this);
                infileSym != null;
                infileSym = srcFiles.Next())
            {
                SetLocation(STAGE.DEFINE); //SETLOCATIONSTAGE(DEFINE);
                SetLocation(infileSym);    //SETLOCATIONFILE(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.ResolveInheritance(infileSym.RootNsDeclSym);
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.DefineBounds
        //
        /// <summary>
        /// Resolve the bounds for all types defined in source
        /// Call CSLDREC.DefineBounds method with a NSDECLSYM instance.
        /// </summary>
        //------------------------------------------------------------
        internal void DefineBounds()
        {
            SourceFileIterator infiles = new SourceFileIterator();
            for (INFILESYM infileSym = infiles.Reset(this);
                infileSym != null;
                infileSym = infiles.Next())
            {
                SetLocation(STAGE.DEFINE); //SETLOCATIONSTAGE(DEFINE);
                SetLocation(infileSym);    //SETLOCATIONFILE(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.DefineBounds(infileSym.RootNsDeclSym);
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.CheckForTypeErrors
        //
        /// <summary>
        /// This is called after all types are prepared to check for bogus,
        /// deprecated and constraints.
        /// </summary>
        //------------------------------------------------------------
        internal void CheckForTypeErrors()
        {
            SourceFileIterator infiles = new SourceFileIterator();
            for (INFILESYM infileSym = infiles.Reset(this);
                infileSym != null;
                infileSym = infiles.Next())
            {
                SetLocation(STAGE.DEFINE); // SETLOCATIONSTAGE(DEFINE);
                SetLocation(infileSym);    // SETLOCATIONFILE(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.CheckForTypeErrors(infileSym.RootNsDeclSym);
                }
            }
        }

        // define all the input files
        //
        // adds symbols for all fields & methods including types
        // does name conflict checking within a class
        // checks field & method modifiers within a class
        // does access checking for all types
        //
        // 1 - define members of changed files only
        // 2 - check interface changes for changed files
        // 3 a - on iface changed       - do a full rebuild
        // 3 b - small iface changed    - define from source dependant files
        // 4 - define unchanged (and undependant) files from metadata

        //------------------------------------------------------------
        // COMPILER.DefineMembers
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void DefineMembers()
        {
            DebugUtil.Assert(this.aggStateMax == AggStateEnum.Bounds);

            // bring object up to declared/defined state first
            // so that we don't have to special case it elsewhere

            ClsDeclRec.DefineObject();

            SourceFileIterator infiles = new SourceFileIterator();
            for (infiles.Reset(this); infiles.Current != null; infiles.Next())
            {
                INFILESYM infileSym = infiles.Current;

                if (infileSym.NamespaceNode != null)
                {
                    DebugUtil.Assert(!infileSym.IsDefined);
                    SetLocation(infileSym);   //SETLOCATIONFILE(infile);
                    DefineOneFile(infileSym);
                    DebugUtil.Assert(infileSym.IsDefined && !infileSym.AreConstsEvaled);
                }
            }
            DebugUtil.Assert(this.aggStateMax == AggStateEnum.Bounds);
            this.aggStateMax = AggStateEnum.DefinedMembers;

            AGGSYM predefAggSym;
            if ((predefAggSym = GetOptPredefAgg(PREDEFTYPE.ATTRIBUTEUSAGE, false)) != null)
            {
                EnsureState(predefAggSym, AggStateEnum.DefinedMembers);
            }
            if ((predefAggSym = GetOptPredefAgg(PREDEFTYPE.OBSOLETE, false)) != null)
            {
                EnsureState(predefAggSym, AggStateEnum.DefinedMembers);
            }
            if ((predefAggSym = GetOptPredefAgg(PREDEFTYPE.CONDITIONAL, false)) != null)
            {
                EnsureState(predefAggSym, AggStateEnum.DefinedMembers);
            }
            if ((predefAggSym = GetOptPredefAgg(PREDEFTYPE.CLSCOMPLIANT, false)) != null)
            {
                EnsureState(predefAggSym, AggStateEnum.DefinedMembers);
            }

            // CS3
            if ((predefAggSym = GetOptPredefAgg(PREDEFTYPE.LINQ_ENUMERABLE, false)) != null)
            {
                EnsureState(predefAggSym, AggStateEnum.DefinedMembers);
            }
        }

        //------------------------------------------------------------
        // COMPILER.DefineOneFile
        //
        /// <summary>
        /// <para>Call CLSDREC.DefineNamespace. This method does:
        /// <list type="bullet">
        /// <item>Resolve all the using clauses.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="infileSym"></param>
        //------------------------------------------------------------
        internal void DefineOneFile(INFILESYM infileSym)
        {
            if (!infileSym.IsDefined)
            {
                SetLocation(STAGE.DEFINE); //SETLOCATIONSTAGE(DEFINE);
                SetLocation(infileSym);    //SETLOCATIONFILE(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.DefineNamespace(infileSym.RootNsDeclSym);
                }
                infileSym.IsDefined = true;
            }
        }

        //------------------------------------------------------------
        // COMPILER.EvaluateConstants
        //
        /// <summary>
        /// Evaluates all the constants and compiles all the attributes.
        /// </summary>
        //------------------------------------------------------------
        internal void EvaluateConstants()
        {
            SourceFileIterator infiles = new SourceFileIterator();
            for (infiles.Reset(this); infiles.Current != null; infiles.Next())
            {
                INFILESYM infileSym = infiles.Current;

                if (infileSym.NamespaceNode != null && !infileSym.AreConstsEvaled)
                {
                    SetLocation(infileSym);   //SETLOCATIONFILE(infile);

                    AggIterator aggSyms = new AggIterator();
                    for (AGGSYM aggSym = aggSyms.Reset(infileSym);
                        aggSym != null;
                        aggSym = aggSyms.Next())
                    {
                        ClsDeclRec.EvaluateConstants(aggSym);
                    }
                    infileSym.AreConstsEvaled = true;
                }
            }
            CheckCLS();
        }

        //------------------------------------------------------------
        // COMPILER.CheckCLS
        //
        /// <summary>
        /// Set the global CLS attribute on the infiles
        /// </summary>
        //------------------------------------------------------------
        internal void CheckCLS()
        {
            this.checkCLS = false;
            if (!AllowCLSErrors())
            {
                return;
            }

            // The location of the CLS attribute -- either a GLOBALATTRSYM or an INFILESYM.
            SYM assemblyAttrSym = null;
            bool isCLSAssembly = false;
            bool checkCLSAssembly;

            GLOBALATTRSYM tempAttrSym = null;
            checkCLSAssembly = ScanAttributesForCLS(
                this.AssemblyAttributes,
                ref tempAttrSym,
                ref isCLSAssembly);
            assemblyAttrSym = tempAttrSym;

            if (checkCLSAssembly)
            {
                this.checkCLS = true;
            }

            SourceOutFileIterator files = new SourceOutFileIterator();
            INFILESYM infileSym;
            InFileIterator infiles = new InFileIterator();

            // differentiate between in-source and the global settings
            bool hasCLS = checkCLSAssembly;
            bool isCLS = isCLSAssembly;

            if (!hasCLS)
            {
                // check imports for attributes
                for (infileSym = infiles.Reset(MainSymbolManager.MetadataFileRootSym);
                    infileSym != null;
                    infileSym = infiles.Next())
                {
                    if (infileSym.IsModule && infileSym.HasCLSAttribute)
                    {
                        if (assemblyAttrSym == null || (!isCLS && infileSym.IsCLS))
                        {
                            // point to the place where it's true!
                            assemblyAttrSym = infileSym;
                        }
                        hasCLS = true;
                        isCLS = infileSym.IsCLS;
                        break;
                    }
                }
            }

            if (hasCLS)
            {
                this.checkCLS = true;
            }

            // Now we know that somewhere there was a CLS Compliant attribute
            // isCLS is the logical OR of that/those value(s)

            // Set the bit on each INFILESYM and OUTFILESYM
            // so the symbols don't have to check back with the compiler
            // and check for consistency

            for (OUTFILESYM outfileSym = files.Reset(this);
                outfileSym != null;
                outfileSym = files.Next())
            {
                bool checkCLSModule = false;	// checking on for this module
                bool isCLSModule = false;		// always the same as the assembly or an error
                GLOBALATTRSYM moduleAttrSym = null;

                checkCLSModule = ScanAttributesForCLS(
                    outfileSym.AttributesSym,
                    ref moduleAttrSym,
                    ref isCLSModule);
                if (hasCLS && checkCLSModule && isCLSModule != isCLS)
                {
                    // If the module doesn't match the assembly, it's an warning
                    CError errorObj = MakeError(
                        moduleAttrSym.ParseTreeNode,
                        CSCERRID.WRN_CLS_NotOnModules2);

                    if (assemblyAttrSym.IsINFILESYM)
                    {
                        AddLocationToError(
                            errorObj,
                            new ERRLOC(
                                assemblyAttrSym as INFILESYM,
                                this.OptionManager.FullPaths));
                    }
                    else
                    {
                        AddLocationToError(
                            errorObj,
                            new ERRLOC(
                                this.MainSymbolManager,
                                (assemblyAttrSym as GLOBALATTRSYM).ParseTreeNode,
                                OptionManager.FullPaths));
                    }
                    SubmitError(errorObj);
                }
                else if (!hasCLS && checkCLSModule)
                {
                    // If the module doesn't match the assembly, it's an warning
                    Error(moduleAttrSym.ParseTreeNode, CSCERRID.WRN_CLS_NotOnModules);
                }

                if (hasCLS)
                {
                    outfileSym.HasCLSAttribute = true;
                    outfileSym.IsCLS = isCLS;

                    for (infileSym = infiles.Reset(outfileSym);
                        infileSym != null;
                        infileSym = infiles.Next())
                    {
                        infileSym.HasCLSAttribute = true;
                        infileSym.IsCLS = isCLS;
                    }
                }
            }

            // check imports (and set them too)
            for (infileSym = infiles.Reset(MainSymbolManager.MetadataFileRootSym);
                infileSym != null;
                infileSym = infiles.Next())
            {
                if (infileSym.IsModule)
                {
                    if (this.checkCLS)
                    {
                        if (infileSym.HasCLSAttribute || infileSym.HasModuleCLSAttribute)
                        {
                            if (isCLS != infileSym.IsCLS)
                            {
                                // Error, module differs from assembly
                                CError errorObj = MakeError(
                                    new ERRLOC(infileSym,this.OptionManager.FullPaths),
                                    CSCERRID.WRN_CLS_NotOnModules2);

                                if (assemblyAttrSym.IsINFILESYM)
                                {
                                    AddLocationToError(
                                        errorObj,
                                        new ERRLOC(
                                            assemblyAttrSym as INFILESYM,
                                            this.OptionManager.FullPaths));
                                }
                                else
                                {
                                    AddLocationToError(
                                        errorObj,
                                        new ERRLOC(
                                            MainSymbolManager,
                                            (assemblyAttrSym as GLOBALATTRSYM).ParseTreeNode,
                                            OptionManager.FullPaths));
                                }
                                SubmitError(errorObj);
                            }
                        }
                        else
                        {
                            // Error, assembly is marked, but module has no attribute
                            // Added modules must be CLS compliant (or at least have the bit set),
                            // so we know that all defined classes are properly marked and checked
                            Error(
                                new ERRLOC(infileSym, this.OptionManager.FullPaths),
                                CSCERRID.WRN_CLS_ModuleMissingCLS);
                        }
                    }
                    else if (infileSym.HasModuleCLSAttribute)
                    {
                        // Warn, ignored attribute on module
                        Error(
                            new ERRLOC(infileSym, this.OptionManager.FullPaths),
                            CSCERRID.WRN_CLS_NotOnModules);
                    }
                    infileSym.HasCLSAttribute = hasCLS;
                    infileSym.IsCLS = isCLS;
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.PostCompileChecks
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void PostCompileChecks()
        {
            // check if any non-external fields are never assigned to
            // or private fields and events are never referenced.

            INFILESYM infileSym;
            SourceFileIterator infileIterator = new SourceFileIterator();
            bool considerInternal = (IsFriendDeclared || !BuildAssembly);
            for (infileSym = infileIterator.Reset(this);
                infileSym != null;
                infileSym = infileIterator.Next())
            {
                AggIterator aggIterator = new AggIterator();
                for (AGGSYM aggSym = aggIterator.Reset(infileSym);
                    aggSym != null;
                    aggSym = aggIterator.Next())
                {
                    // Don't Check Certain structs
                    if (aggSym.HasExternReference)
                    {
                        continue;
                    }

                    bool hasExternalVisibility = considerInternal ?
                        aggSym.HasExternalOrFriendAccess() : aggSym.HasExternalAccess();

                    for (SYM memberSym = aggSym.FirstChildSym;
                        memberSym != null;
                        memberSym = memberSym.NextSym)
                    {
                        if (memberSym.IsMEMBVARSYM)
                        {
                            MEMBVARSYM fieldSym = memberSym as MEMBVARSYM;
                            // Only check non-Const internall-only fields and events
                            if (fieldSym.IsConst ||
                                (hasExternalVisibility &&
                                    considerInternal
                                        ? fieldSym.HasExternalOrFriendAccess()
                                        : fieldSym.HasExternalAccess()))
                            {
                                continue;
                            }

                            if (!fieldSym.IsReferenced && fieldSym.Access == ACCESS.PRIVATE)
                            {
                                if (!fieldSym.IsEvent)
                                {
                                    ErrorRef(
                                        null,
                                        fieldSym.IsAssigned
                                            ? CSCERRID.WRN_UnreferencedFieldAssg
                                            : CSCERRID.WRN_UnreferencedField,
                                        new ErrArgRef(fieldSym));
                                }
                                else
                                {
                                    ErrorRef(null, CSCERRID.WRN_UnreferencedEvent, new ErrArgRef(fieldSym));
                                }
                            }
                            else if (!fieldSym.IsAssigned && !fieldSym.IsEvent)
                            {
                                string zeroString;
                                if (fieldSym.TypeSym.IsNumericType() || fieldSym.TypeSym.IsEnumType())
                                {
                                    zeroString = "0";
                                }
                                else if (fieldSym.TypeSym.IsPredefType(PREDEFTYPE.BOOL))
                                {
                                    zeroString = "false";
                                }
                                else if (fieldSym.TypeSym.FundamentalType() == FUNDTYPE.REF)
                                {
                                    zeroString = "null";
                                }
                                else
                                {
                                    zeroString = "";
                                }
                                ErrorRef(null, CSCERRID.WRN_UnassignedInternalField,
                                    new ErrArgRef(fieldSym), new ErrArgRef(zeroString));
                            }
                        }
#if USAGEHACK
                else if (memberSym.IsMETHSYM)
                {
                    METHSYM methodSym = memberSym as METHSYM;
                    if (!methodSym.IsPropertyAccessor() &&
                    	!methodSym.IsUsed &&
                    	(methodSym.Access == ACCESS.INTERNAL ||
                    	methodSym.Access == ACCESS.PRIVATE) &&
                    	!methodSym.IsOverride &&
                    	methodSym.GetInputFile().IsSource)
                    {
                        Console.WriteLine("%ls : %ls", methodSym.GetInputFile().Name, ErrSym(methodSym));
                    }
                }
                else if (memberSym.IsPROPSYM)
                {
                    PROPSYM  propertySym = memberSym as PROPSYM;
                    if ((propertySym.Access == ACCESS.INTERNAL ||
                    	propertySym.Access == ACCESS.PRIVATE)&&
                    	!propertySym.IsOverride &&
                    	propertySym.GetInputFile().IsSource)
                    {
                        if (propertySym.GetMethodSym!=null && !propertySym.GetMethodSym.IsUsed)
                        {
                            Console.WriteLine("%ls : %ls",
                            	propertySym.GetInputFile().Name, ErrSym(propertySym.GetMethodSym));
                        }
                        if (propertySym.SetMethodSym!=null && !propertySym.SetMethodSym.IsUsed)
                        {
                            Console.WriteLine("%ls : %ls\n",
                            	propertySym.GetInputFile().Name, ErrSym(propertySym.SetMethodSym));
                        }
                    }
                }
#endif
                    }
                }
            }

            // Now that we have an actual AssemblyDef record,
            // compare it against all of the assumed matched references
            // If we are not building an assembly, then we will not have an AssemblyDef record
            // we assume the module matches the reference because the shortname matched before.

            if (BuildAssembly && ArlRefsToOutput != null)
            {
                //CComPtr<IAssemblyName> panOutput;
                AssemblyName outputAsmName = null;
                string outputName;

                outputAsmName = Importer.GetOutputAssemblyName(this.AssemblyOutfileSym, out outputName);
                foreach (AssemblyRefList arlCurrent in ArlRefsToOutput)
                {
                    Importer.ConfirmMatchesThisAssembly(
                        outputAsmName,
                        outputName,
                        arlCurrent.NameRef,
                        arlCurrent.SymList,
                        arlCurrent.IsFriendAssemblyRefOnly);
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.EmitTokens
        //
        /// <summary></summary>
        /// <param name="outfileSym"></param>
        //------------------------------------------------------------
        internal void EmitTokens(OUTFILESYM outfileSym)
        {
            InFileIterator infiles = new InFileIterator();
            INFILESYM infileSym;
            int ErrCnt = ErrorCount();

            //--------------------------------------------------------
            // Types
            //--------------------------------------------------------
            Queue<AGGSYM> aggQueue = new Queue<AGGSYM>();

            for (infileSym = infiles.Reset(outfileSym);
                infileSym != null;
                infileSym = infiles.Next())
            {
                SetLocation(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.EmitEnumdefsNamespace(infileSym.RootNsDeclSym, aggQueue);
                }
            }

            ClsDeclRec.EmitTypedefsNamespace(aggQueue);

            CErrorSuppression es = new CErrorSuppression();

            // This means we might not have tokens for all typedefs
            // so bail early
            if (FAbortEarly(ErrCnt, es))
            {
                return;
            }

            //--------------------------------------------------------
            // Base types
            //--------------------------------------------------------
            for (infileSym = infiles.Reset(outfileSym);
                infileSym != null;
                infileSym = infiles.Next())
            {
                SetLocation(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.EmitBasesNamespace(infileSym.RootNsDeclSym);
                }
            }

            //--------------------------------------------------------
            // Members
            //--------------------------------------------------------
            for (infileSym = infiles.Reset(outfileSym);
                infileSym != null;
                infileSym = infiles.Next())
            {
                SetLocation(infileSym);

                if (infileSym.RootNsDeclSym != null)
                {
                    ClsDeclRec.EmitMemberdefsNamespace(infileSym.RootNsDeclSym);
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.DeclareTypes
        //
        /// <summary>
        /// <para>declare all the input files</para>
        /// <para>adds symbol table entries for all namespaces and
        /// user defined types (classes, enums, structs, interfaces, delegates)
        /// sets access modifiers on all user defined types</para>
        /// <para>Call DeclareOneFile method with each input file.</para>
        /// </summary>
        /// <param name="progressSink"></param>
        //------------------------------------------------------------
        internal void DeclareTypes(CCompileProgress progressSink)
        {
            // parse & declare the input files

            int doneFileCount = 0;
            SourceFileIterator infileItr = new SourceFileIterator();

            for (infileItr.Reset(this); infileItr.Current != null; infileItr.Next())
            {
                DeclareOneFile(infileItr.Current);

                if (ReportProgress(
                    progressSink,
                    COMPILE_PHASE_ENUM.DECLARETYPES,
                    ++doneFileCount,
                    InputFileCount))
                {
                    break;
                }
            }
        }

        //------------------------------------------------------------
        // COMPILER.AddConditionalSymbol
        //------------------------------------------------------------
        //void AddConditionalSymbol(PNAME name);

        /// <summary>
        /// return this;
        /// </summary>
        /// <returns></returns>
        //private COMPILER compiler() { return this; }

        //------------------------------------------------------------
        // COMPILER.SetDispenserOptions
        //
        /// <summary>
        /// sets the options on the current metadata dispenser
        /// </summary>
        //------------------------------------------------------------
        internal void SetDispenserOptions()
        {
            //    VARIANT v;
            //
            //    if (dispenser) {
            //        // Set the emit options for maximum speed: no token remapping, no ref to def optimization,
            //        // no duplicate checking. We do all these optimizations
            //        // ourselves.
            //
            //        // Only check typerefs, member refs, modulerefs and assembly refs -- we need to do this
            //        // because DefineImportMember or ALink may create these refs for us.
            //
            //        // We do not set the duplicate checking flags here, ALink sets them when we call Init().
            //        // MetaDataCheckDuplicatesFor is set to:  MDDupTypeRef | MDDupMemberRef | MDDupModuleRef | MDDupAssemblyRef | MDDupExportedType;
            //
            //        // Never change refs to defs
            //        V_VT(&v) = VT_UI4;
            //        V_UI4(&v) = MDRefToDefNone;
            //        dispenser.SetOption(MetaDataRefToDefCheck, &v);
            //
            //        // Don't give error if emitting out of order because we'll just be reordering it
            //        V_VT(&v) = VT_UI4;
            //        V_UI4(&v) = MDErrorOutOfOrderNone;
            //        dispenser.SetOption(MetaDataErrorIfEmitOutOfOrder, &v);
            //
            //        // Notify of all token remaps
            //        V_VT(&v) = VT_UI4;
            //        V_UI4(&v) = MDNotifyAll;
            //        dispenser.SetOption(MetaDataNotificationForTokenMovement, &v);
            //
            //        // Turn on full update build for the schema
            //        V_VT(&v) = VT_UI4;
            //        V_UI4(&v) = MDUpdateFull;
            //
            //        dispenser.SetOption(MetaDataSetUpdate, &v);
            //    }
        }

        //------------------------------------------------------------
        // COMPILER.AddAssemblyAttribute
        //
        /// <summary>
        /// Add a GLOBALATTRSYM to the list starting from AssemblyAttributes.
        /// </summary>
        /// <param name="attrSym"></param>
        //------------------------------------------------------------
        internal void AddAssemblyAttribute(GLOBALATTRSYM attrSym)
        {
            if (attrSym == null)
            {
                return;
            }

            if (this.AssemblyAttributes == null)
            {
                this.AssemblyAttributes = attrSym;
                this.LastAssemblyAttributes = attrSym;
            }
            else
            {
                DebugUtil.Assert(this.LastAssemblyAttributes != null);
                this.LastAssemblyAttributes.NextAttributeSym = attrSym;
                this.LastAssemblyAttributes = attrSym;
            }
            attrSym.NextAttributeSym = null;
        }

        //------------------------------------------------------------
        // COMPILER.ReportProgress
        //
        /// <summary>
        /// <para>Report compiler progress and give the user a chance to cleanly cancel the compile.
        /// The compiler is now in the middle of phase "phase",
        /// and has completed "itemsComplete" of the "itemsTotal" in this phase.</para>
        /// <para>returns TRUE if the compilation should be canceled (an error has already been reported.</para>
        /// </summary>
        /// <param name="progressSink"></param>
        /// <param name="phase"></param>
        /// <param name="itemsComplete"></param>
        /// <param name="itemsTotal"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ReportProgress(
            CCompileProgress progressSink,
            COMPILE_PHASE_ENUM phase,
            int itemsComplete,
            int itemsTotal)
        {
            if (progressSink == null)
            {
                return false;
            }

            bool cancel = false;
            int totItemsComplete, totItems;
            const int SCALE = 1024;
            DebugUtil.Assert(phase < COMPILE_PHASE_ENUM.MAX);

            // convert phase, itemsComplex, itemsTotal into totItemsComplex and totItems,
            // using the relative time in phase array.
            totItems = totItemsComplete = 0;

            int iMaxPhase = (int)COMPILE_PHASE_ENUM.MAX;

            for (int i = 0; i < iMaxPhase; ++i)
            {
                totItems += SCALE * relativeTimeInPhase[i];
                if (i < (int)phase)
                {
                    totItemsComplete += SCALE * relativeTimeInPhase[i];
                }
            }
            if (itemsTotal > 0)
            {
                try
                {
                    totItemsComplete +=
                        (SCALE * itemsComplete / itemsTotal) * relativeTimeInPhase[(int)phase];
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            cancel = (progressSink.ReportProgress("", totItems - totItemsComplete, totItems) > 0);

            if (cancel && !this.isCanceled)
            {
                Error(new ERRLOC(), CSCERRID.ERR_CompileCancelled);
                this.isCanceled = true;
            }

            return this.isCanceled;
        }

        //------------------------------------------------------------
        // COMPILER.CheckBogusCore
        //
        /// <summary>
        /// <para>If argument sym is already set sym.IsBogus, return it.</para>
        /// <para>If sym.IsBogus is not set, check its parent sym or base type sym
        /// or return type sym and type arguments and so on by kind.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="noEnsure"></param>
        /// <param name="undeclared"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckBogusCore(SYM sym, bool noEnsure, ref bool undeclared)
        {
            if (sym == null)
            {
                return false;
            }
            if (sym.HasBogus)
            {
                return sym.IsBogus;
            }

            bool isBogusTemp = false;
            bool undeclaredTemp = false;

            switch (sym.Kind)
            {
                case SYMKIND.PROPSYM:
                case SYMKIND.METHSYM:
                case SYMKIND.FAKEMETHSYM:
                    METHPROPSYM meth = sym as METHPROPSYM;

                    isBogusTemp = CheckBogusCore(meth.ReturnTypeSym, noEnsure, ref undeclaredTemp);

                    // We need to check the parameters as well.
                    if (meth.ParameterTypes != null)
                    {
                        for (int i = 0; !isBogusTemp && i < meth.ParameterTypes.Count; i++)
                        {
                            isBogusTemp |= CheckBogusCore(meth.ParameterTypes[i], noEnsure, ref undeclaredTemp);
                        }
                    }
                    else
                    {
                        // Params should only be NULL if noEnsure is set,
                        // in which case we don't want to call setBogus(false)!
                        DebugUtil.Assert(noEnsure);
                        undeclaredTemp = true;
                    }
                    break;

                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.ARRAYSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.PINNEDSYM:
                    isBogusTemp = CheckBogusCore(sym.ParentSym as TYPESYM, noEnsure, ref undeclaredTemp);
                    break;

                case SYMKIND.EVENTSYM:
                    isBogusTemp = CheckBogusCore((sym as EVENTSYM).TypeSym, noEnsure, ref undeclaredTemp);
                    break;

                case SYMKIND.MEMBVARSYM:
                    isBogusTemp = CheckBogusCore((sym as VARSYM).TypeSym, noEnsure, ref undeclaredTemp);
                    break;

                case SYMKIND.ERRORSYM:
                    sym.SetBogus(false);
                    break;

                case SYMKIND.AGGTYPESYM:
                    isBogusTemp = CheckBogusCore((sym as AGGTYPESYM).GetAggregate(), noEnsure, ref undeclaredTemp);
                    for (int i = 0; !isBogusTemp && i < (sym as AGGTYPESYM).AllTypeArguments.Count; i++)
                    {
                        isBogusTemp |= CheckBogusCore(
                            (sym as AGGTYPESYM).AllTypeArguments[i], noEnsure, ref undeclaredTemp);
                    }
                    break;

                case SYMKIND.TYVARSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.NULLSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.LOCVARSYM:
                case SYMKIND.DYNAMICSYM:    // CS4
                    sym.SetBogus(false);
                    break;

                case SYMKIND.AGGSYM:
                    if (!noEnsure)
                    {
                        EnsureState(sym as AGGSYM, AggStateEnum.Prepared);
                    }
                    undeclaredTemp = !(sym as AGGSYM).IsPrepared;
                    isBogusTemp = sym.HasBogus && sym.IsBogus;
                    break;

                case SYMKIND.SCOPESYM:
                case SYMKIND.ANONSCOPESYM:
                case SYMKIND.NSSYM:
                case SYMKIND.NSDECLSYM:
                default:
                    DebugUtil.Assert(false, "CheckBogus with invalid Symbol kind");
                    sym.SetBogus(false);
                    break;
            }

            if (!undeclaredTemp || isBogusTemp)
            {
                // Only set this if everything is declared or
                // at least 1 declared thing is bogus
                sym.SetBogus(isBogusTemp);
            }

            undeclared |= undeclaredTemp;
            return sym.HasBogus && sym.IsBogus;
        }

        //------------------------------------------------------------
        // COMPILER.EnsureTypesInNsAid (1)
        //
        /// <summary>
        /// <para>Make sure all the types in the given (nsSym, assemblyId) pair are loaded.
        /// Aid should be an assembly id or extern alias id, NOT a module id.</para>
        /// <para>If not all the types of a specified assembly are loaded, load them.</para>
        /// </summary>
        /// <param name="nsSym"></param>
        /// <param name="assemblyId"></param>
        //------------------------------------------------------------
        internal void EnsureTypesInNsAid(NSSYM nsSym, int assemblyId)
        {
            DebugUtil.Assert(assemblyId < Kaid.MinModule);

            SYM sym;
            INFILESYM infileSym = null;

            switch (assemblyId)
            {
                case Kaid.ThisAssembly:
                    if (!nsSym.TypesUnloaded(assemblyId))
                    {
                        return;
                    }
                    break;

                case Kaid.Global:
                    if (!nsSym.AnyTypesUnloaded(MainSymbolManager.GlobalAssemblyBitset))
                    {
                        return;
                    }
                    break;

                default:
                    sym = MainSymbolManager.GetSymForAid(assemblyId);
                    if (sym == null)
                    {
                        return;
                    }
                    if (sym.IsINFILESYM)
                    {
                        DebugUtil.Assert((sym as INFILESYM).GetAssemblyID() == assemblyId);

                        if (!nsSym.TypesUnloaded(assemblyId))
                        {
                            return;
                        }
                        infileSym = sym as INFILESYM;
                    }
                    else if (sym.IsEXTERNALIASSYM)
                    {
                        // Enumerate assemblies and check for being in assemblyId.
                        if (!nsSym.AnyTypesUnloaded((sym as EXTERNALIASSYM).AssembliesBitset))
                        {
                            return;
                        }
                    }
                    break;
            }

            this.Importer.LoadTypesInNsAid(nsSym, assemblyId, infileSym);
        }

        //------------------------------------------------------------
        // COMPILER.EnsureTypesInNsAid (2)
        //
        /// <summary>
        /// <para>Make sure all the types in the given (nsSym, assemblyId) pair are loaded.
        /// Aid should be an assembly id or extern alias id, NOT a module id.</para>
        /// <para>If not all the types of a specified assembly are loaded, load them.</para>
        /// </summary>
        /// <param name="nsSym"></param>
        /// <param name="assemblyId"></param>
        //------------------------------------------------------------
        internal void EnsureTypesInNsAid(string name, NSSYM nsSym, int arity, int assemblyId)
        {
            DebugUtil.Assert(assemblyId < Kaid.MinModule);

            SYM sym;
            INFILESYM infileSym = null;

            switch (assemblyId)
            {
                case Kaid.ThisAssembly:
                    if (!nsSym.TypesUnloaded(assemblyId))
                    {
                        return;
                    }
                    break;

                case Kaid.Global:
                    if (!nsSym.AnyTypesUnloaded(MainSymbolManager.GlobalAssemblyBitset))
                    {
                        return;
                    }
                    break;

                default:
                    sym = MainSymbolManager.GetSymForAid(assemblyId);
                    if (sym == null)
                    {
                        return;
                    }
                    if (sym.IsINFILESYM)
                    {
                        DebugUtil.Assert((sym as INFILESYM).GetAssemblyID() == assemblyId);

                        if (!nsSym.TypesUnloaded(assemblyId))
                        {
                            return;
                        }
                        infileSym = sym as INFILESYM;
                    }
                    else if (sym.IsEXTERNALIASSYM)
                    {
                        // Enumerate assemblies and check for being in assemblyId.
                        if (!nsSym.AnyTypesUnloaded((sym as EXTERNALIASSYM).AssembliesBitset))
                        {
                            return;
                        }
                    }
                    break;
            }

            this.Importer.LoadTypesInNsAid(name, nsSym, arity, assemblyId, infileSym);
        }

        // Error handling methods and data (error.cpp)

        //------------------------------------------------------------
        // COMPILER.ErrBufferLeftTot
        //------------------------------------------------------------
        //int ErrBufferLeftTot() // WARNING: Don't ever pass this to wcsncpy_s! It zeros out the entire remaining buffer!
        //    { return (int)(ERROR_BUFFER_MAX_WCHARS - (errBufferNext - errBuffer)); }
        //size_t ErrBufferLeft(size_t cchNeed)
        //{
        //    size_t cch = ERROR_BUFFER_MAX_WCHARS - (errBufferNext - errBuffer);
        //    return min(cch, cchNeed);
        //}

        //------------------------------------------------------------
        // COMPILER.ThrowFatalException
        //
        // ThrowFatalException. After a fatal error occurs, this calls to throw
        // an exception out to the outer-most code to abort the compilation.
        //------------------------------------------------------------
        internal void ThrowFatalException()
        {
            //RaiseException(FATAL_EXCEPTION_CODE, 0, 0, NULL);
            CSException excp = new CSException();
            throw excp;
        }

        // Miscellaneous.

        //void CheckSearchPath(__inout PWSTR wzPathList, int idsSource);

        //------------------------------------------------------------
        // COMPILER.GetSearchPath
        //
        /// <summary>
        /// Get the path to search for imports
        /// = Current Directory, CORSystemDir, /LIB otpion, %LIB%
        /// </summary>
        //------------------------------------------------------------
        internal List<DirectoryInfo> GetSearchPath()
        {
            DirectoryInfo dirInfo = null;
            Exception excp = null;

            if (libPathList == null)
            {
                libPathList = new List<DirectoryInfo>();
            }

            if (libPathList.Count == 0)
            {
                // add current directory
                string currentDir = Directory.GetCurrentDirectory();
                if (!String.IsNullOrEmpty(currentDir))
                {
                    if (IOUtil.CreateDirectoryInfo(currentDir, out dirInfo, out excp) &&
                        dirInfo.Exists)
                    {
                        libPathList.Add(dirInfo);
                    }
                    else
                    {
                        if (excp != null)
                        {
                            Controller.ReportError(ERRORKIND.ERROR, excp);
                        }
                        else
                        {
                            Controller.OnCatastrophicError(currentDir);
                        }
                    }
                }

                // add CorSystemDir
                DirectoryInfo cordir = GetCorSystemDirectory();
                if (cordir != null && !String.IsNullOrEmpty(cordir.FullName))
                {
                    libPathList.Add(cordir);
                }

                // add /lib
                List<string> libList = this.OptionManager.LibPathList;
                if (libList == null)
                {
                    libList = new List<string>();
                }

                if (Config.AdditionalLibPaths.Length > 0)
                {
                    libList.AddRange(Config.AdditionalLibPaths);
                }

                for (int i = 0; i < libList.Count; ++i)
                {
                    if (IOUtil.CreateDirectoryInfo(libList[i], out dirInfo, out excp))
                    {
                        if (dirInfo != null && dirInfo.Exists)
                        {
                            libPathList.Add(dirInfo);
                        }
                    }
                }

                // add LIB= environment variable
                string envlib = System.Environment.GetEnvironmentVariable("LIB");
                if (!String.IsNullOrEmpty(envlib))
                {
                    string[] paths = envlib.Split(';');
                    if (paths != null && paths.Length > 0)
                    {
                        for (int i = 0; i < paths.Length; ++i)
                        {
                            if (IOUtil.CreateDirectoryInfo(paths[i], out dirInfo, out excp))
                            {
                                if (dirInfo != null && dirInfo.Exists)
                                {
                                    libPathList.Add(dirInfo);
                                }
                            }
                        }
                    }
                }
            }
            return libPathList;
        }

        // No import library is provided for these guys, so we
        // bind to them dynamically.
        // HRESULT (__stdcall *pfnCreateCeeFileGen)(ICeeFileGen **ceeFileGen); // call this to instantiate
        // HRESULT (__stdcall *pfnDestroyCeeFileGen)(ICeeFileGen **ceeFileGen); // call this to delete

        //internal:

        //------------------------------------------------------------
        // COMPILER.FEncBuild
        //
        /// <summary>
        /// Return false for now.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FEncBuild()
        {
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.RecordEncMethRva
        //
        /// <summary>
        /// <para>Does nothing for now.</para>
        /// </summary>
        /// <param name="meth"></param>
        /// <param name="rva"></param>
        //------------------------------------------------------------
        internal void RecordEncMethRva(METHSYM meth, ulong rva)
        {
        }

        // macros used for building up strings in the error buffer

        //------------------------------------------------------------
        // COMPILER.StartErrorString
        //
        // #define START_ERR_STRING(compiler) \
        //     PCWSTR __errstr = (compiler).GetErrorBufferCurrent();
        //
        /// <summary>
        /// Return Compiler.errorBuffer lenght.
        /// This means the start index when we add a new substring.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        //private int StartErrorString()
        //{
        //    return ErrorBuffer.Length;
        //}

        //------------------------------------------------------------
        // COMPILER.SetLocation (1)
        //
        /// <summary>
        /// <para>Push a LOCATION instance to the stack at COMPILER.Location.</para>
        /// </summary>
        /// <param name="loc"></param>
        //------------------------------------------------------------
        internal void SetLocation(LOCATION loc)
        {
            DebugUtil.Assert(loc != null);

            if (this.Location == null)
            {
                this.Location = loc;
                this.Location.PreviousLocation = null;
            }
            else
            {
                loc.PreviousLocation = this.Location;
                this.Location = loc;
            }
        }

        //------------------------------------------------------------
        // COMPILER.SetLocation (2)
        //------------------------------------------------------------
        internal void SetLocation(SYM sym)
        {
            this.SetLocation((new SYMLOCATION(sym)) as LOCATION); 
        }

        //------------------------------------------------------------
        // COMPILER.SetLocation (3)
        //------------------------------------------------------------
        internal void SetLocation(BASENODE node)
        {
            this.SetLocation((new NODELOCATION(node)) as LOCATION);
        }

        //------------------------------------------------------------
        // COMPILER.SetLocation (4)
        //------------------------------------------------------------
        internal void SetLocation(INFILESYM file)
        {
            this.SetLocation((new FILELOCATION(file)) as LOCATION);
        }

        //------------------------------------------------------------
        // COMPILER.SetLocation (5)
        //------------------------------------------------------------
        internal void SetLocation(COMPILER.STAGE stage)
        {
            this.SetLocation((new STAGELOCATION(stage)) as LOCATION);
        }

        //------------------------------------------------------------
        // COMPILER.PopLocation
        //
        /// <summary>
        /// <para>Pop a LOCATION instance from the stack at COMPILER.Location.</para>
        /// <para>If the stack is empty, return null.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal LOCATION PopLocation()
        {
            if (this.Location != null)
            {
                LOCATION top = this.Location;
                this.Location = top.PreviousLocation;
                return top;
            }
            return null;
        }

        //------------------------------------------------------------
        // COMPILER.DebugLocations
        //
        /// <summary></summary>
        //------------------------------------------------------------
        [Conditional("DEBUG")]
        internal void DebugLocations()
        {
            int count = 0;
            LOCATION loc = this.Location;
            while (loc != null)
            {
                ++count;
                loc = loc.PreviousLocation;
            }
            Console.WriteLine("Locations : {0}", count);
        }

        //------------------------------------------------------------
        // COMPILER.IsValidInputFile (1)
        //
        /// <summary></summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool IsValidInputFile(FileInfo fi)
        {
            if (fi == null) return false;

            if (!fi.Exists ||
                (fi.Attributes & FileAttributes.Compressed) != 0 ||
                (fi.Attributes & FileAttributes.Directory) != 0 ||
                (fi.Attributes & FileAttributes.Encrypted) != 0)
            {
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // COMPILER.IsValidInputFile (2)
        //
        /// <summary></summary>
        /// <param name="file"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool IsValidInputFile(string file)
        {
            FileInfo fi;
            Exception excp = null;

            if (IOUtil.CreateFileInfo(file, out fi, out excp))
            {
                return IsValidInputFile(fi);
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.SearchPathDual
        //
        /// <summary>
        /// <para>Replacement routine for SearchPath.
        /// Uses pipe (|) character to separate path elements
        /// (so that paths can contain semicolons).
        /// Returns TRUE on success.
        /// Will return FALSE but set the filename if a matching file is found
        /// but the attributes are invalid (i.e. a directory).</para>
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="file"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool SearchPathDual(
            List<DirectoryInfo> dirs,
            string file,
            out FileInfo fileInfo)
        {
            fileInfo = null;
            Exception excp = null;
            if (String.IsNullOrEmpty(file))
            {
                return false;
            }

            // 指定されたファイル名が有効なフルパスの場合はそれを返すことにする。
            if (IOUtil.CreateFileInfo(file, out fileInfo, out excp))
            {
                if (IsValidInputFile(fileInfo))
                {
                    return true;
                }
            }

            // dirs の各ディレクトリ内で file を探す。
            StringBuilder buffer = new StringBuilder();
            foreach (DirectoryInfo dir in dirs)
            {
                if (dir == null || !dir.Exists || String.IsNullOrEmpty(dir.FullName)) continue;
                buffer.Length = 0;
                buffer.Append(dir.FullName);
                if (buffer[buffer.Length - 1] != '\\') buffer.Append('\\');
                buffer.Append(file);
                if (IOUtil.CreateFileInfo(buffer.ToString(), out fileInfo, out excp))
                {
                    if (IsValidInputFile(fileInfo)) return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // COMPILER.PlatformPEKind
        //
        /// <summary>
        /// (CSharp\SCComp\Compiler.cs)
        /// </summary>
        /// <remarks>
        /// static const DWORD rgPEKind [] (sscli)
        /// </remarks>
        //------------------------------------------------------------
        static internal PortableExecutableKinds[] PlatformPEKind =
        {
            // AnyCPU = 0
            PortableExecutableKinds.ILOnly,

            // AnyCPU32BitPreferred
            PortableExecutableKinds.ILOnly | PortableExecutableKinds.Required32Bit,

            // ARM
            PortableExecutableKinds.ILOnly | PortableExecutableKinds.Required32Bit,

            // x86
            PortableExecutableKinds.ILOnly | PortableExecutableKinds.Required32Bit,

            // Itanium
            PortableExecutableKinds.ILOnly | PortableExecutableKinds.PE32Plus,

            // x64
            PortableExecutableKinds.ILOnly | PortableExecutableKinds.PE32Plus,
        };

        //------------------------------------------------------------
        // COMPILER.PlatformMachine
        //
        /// <summary>
        /// (CSharp\SCComp\Compiler.cs)
        /// </summary>
        /// <remarks>
        /// static const DWORD rgMachine [] (sscli)
        /// </remarks>
        //------------------------------------------------------------
        static internal ImageFileMachine[] PlatformMachine =
        {
            // AnyCPU = 0
            ImageFileMachine.I386,

            // AnyCPU32BitPreferred
            ImageFileMachine.I386,

            // ARM
            ImageFileMachine.I386,

            // x86
            ImageFileMachine.I386,

            // Itanium
            ImageFileMachine.IA64,

            // x64
            ImageFileMachine.AMD64,
        };
    }
    // end of COMPILER

    // Define inline routine for getting to the COMPILER class from
    // various embedded classes.

    //__forceinline BSYMHOST * BSYMMGR::host()
    //{ return (COMPILER *) (((BYTE *)this) - offsetof(COMPILER, GlobalSymmgr)); }

    //__forceinline COMPILER * SYMMGR::compiler()
    //{ return (COMPILER *) (((BYTE *)this) - offsetof(COMPILER, GlobalSymmgr)); }
    //

    //__forceinline COMPILER * IMPORTER::compiler()
    //{ return (COMPILER *) (((BYTE *)this) - offsetof(COMPILER, importer)); }

    //__forceinline COMPILER * FUNCBREC::compiler()
    //{ return (COMPILER *) (((BYTE *)this) - offsetof(COMPILER, funcBRec)); }

    //__forceinline COMPILER * CLSDREC::compiler()
    //{ return (COMPILER *) (((BYTE *)this) - offsetof(COMPILER, clsDeclRec)); }

    //__forceinline COMPILER * EMITTER::compiler()
    //{ return (COMPILER *) (((BYTE *)this) - offsetof(COMPILER, emitter)); }

    //__forceinline COMPILER * ILGENREC::compiler()
    //{ return (COMPILER *) (((BYTE *)this) - offsetof(COMPILER, ilGenRec)); }

    //__forceinline CController * CLSDREC::controller() { return compiler().CompilerController; }

    //================================================================================
    // LOCATION for reporting errors
    //
    /// <summary>
    /// <para>they link together in a stack, top of the stack is the most recent location.</para>
    /// <para>they should only be created as local variables using the SETLOCATIONXXX(xxx) macros below</para>
    /// <para>these guys are designed for fast construction/destruction
    /// and slow query since they are only queried after an ICE</para>
    /// <para>CONSIDER:
    /// <list type="bullet">
    /// <item>making an empty version of the stage/location for release builds</item>
    /// <item>making super debug/loggin versions of SETLOCATIONXXX(xxx)</item>
    /// </list>
    /// </para>
    /// </summary>
    //================================================================================
    abstract internal class LOCATION
    {
        //------------------------------------------------------------
        // LOCATION Fields and Properties
        //------------------------------------------------------------
        internal LOCATION PreviousLocation = null;
        //internal LOCATION ListHead;

        //------------------------------------------------------------
        // LOCATION Constructor (1)
        //------------------------------------------------------------
        protected LOCATION() { }

        //------------------------------------------------------------
        // LOCATION Constructor (2)
        //------------------------------------------------------------
        protected LOCATION(LOCATION prev)
        {
            this.PreviousLocation = prev;
        }

        //------------------------------------------------------------
        // LOCATION.GetStage
        //------------------------------------------------------------
        virtual internal COMPILER.STAGE GetStage()
        {
            return this.PreviousLocation.GetStage();
        }

        //------------------------------------------------------------
        // LOCATION abstract methods.
        //------------------------------------------------------------
        abstract internal SYM GetSymbol();
        abstract internal INFILESYM GetFile();
        abstract internal BASENODE GetNode();
    }

    //================================================================================
    // class SYMLOCATION
    //================================================================================
    class SYMLOCATION : LOCATION
    {
        //------------------------------------------------------------
        // SYMLOCATION Fields and Properties
        //------------------------------------------------------------
        internal SYM Symbol;

        //------------------------------------------------------------
        // SYMLOCATION Constructor (1)
        //------------------------------------------------------------
        internal SYMLOCATION(SYM sym)
        {
            Symbol = sym;
        }

        //------------------------------------------------------------
        // SYMLOCATION Constructor (2)
        //------------------------------------------------------------
        internal SYMLOCATION(LOCATION prev, SYM sym)
            : base(prev)
        {
            Symbol = sym;
        }

        //------------------------------------------------------------
        // SYMLOCATION.GetSymbol
        //------------------------------------------------------------
        override internal SYM GetSymbol()
        {
            return this.Symbol;
        }

        //------------------------------------------------------------
        // SYMLOCATION.GetFile
        //------------------------------------------------------------
        override internal INFILESYM GetFile()
        {
            return this.Symbol.GetSomeInputFile();
        }

        //------------------------------------------------------------
        // SYMLOCATION.GetNode
        //------------------------------------------------------------
        override internal BASENODE GetNode()
        {
            return this.Symbol.GetSomeParseTree();
        }
    }

    //================================================================================
    // class NODELOCATION
    //================================================================================
    internal class NODELOCATION : LOCATION
    {
        //------------------------------------------------------------
        // NODELOCATION Fields and Properties
        //------------------------------------------------------------
        internal BASENODE Node = null;

        //------------------------------------------------------------
        // NODELOCATION Constructor (1)
        //------------------------------------------------------------
        internal NODELOCATION(LOCATION prev, BASENODE node)
            : base(prev)
        {
            this.Node = node;
        }

        //------------------------------------------------------------
        // NODELOCATION Constructor (2)
        //------------------------------------------------------------
        internal NODELOCATION(BASENODE node)
            : base()
        {
            this.Node = node;
        }

        //------------------------------------------------------------
        // NODELOCATION.GetSymbol
        //------------------------------------------------------------
        internal override SYM GetSymbol()
        {
            return null;
        }

        //------------------------------------------------------------
        // NODELOCATION.GetFile
        //------------------------------------------------------------
        internal override INFILESYM GetFile()
        {
            return this.PreviousLocation.GetFile();
        }

        //------------------------------------------------------------
        // NODELOCATION.GetNode
        //------------------------------------------------------------
        internal override BASENODE GetNode()
        {
            return this.Node;
        }
    }

    //================================================================================
    // FILELOCATION
    //================================================================================
    class FILELOCATION : LOCATION
    {
        internal INFILESYM InFileSym = null;


        //------------------------------------------------------------
        // FILELOCATION Constructor (1)
        //------------------------------------------------------------
        internal FILELOCATION(LOCATION prev, INFILESYM sym)
            : base(prev)
        {
            this.InFileSym = sym;
        }

        //------------------------------------------------------------
        // FILELOCATION Constructor (2)
        //------------------------------------------------------------
        internal FILELOCATION(INFILESYM sym)
            : base()
        {
            this.InFileSym = sym;
        }

        //------------------------------------------------------------
        // FILELOCATION.GetSymbol
        //------------------------------------------------------------
        override internal SYM GetSymbol()
        {
            return null;
        }

        //------------------------------------------------------------
        // FILELOCATION.GetFile
        //------------------------------------------------------------
        override internal INFILESYM GetFile()
        {
            return this.InFileSym;
        }

        //------------------------------------------------------------
        // FILELOCATION.GetNode
        //------------------------------------------------------------
        override internal BASENODE GetNode()
        {
            return null;
        }
    }

    //================================================================================
    // class STAGELOCATION
    //================================================================================
    internal class STAGELOCATION : LOCATION
    {
        //------------------------------------------------------------
        // STAGELOCATION
        //------------------------------------------------------------
        internal COMPILER.STAGE Stage;

        //------------------------------------------------------------
        // STAGELOCATION Constructor (1)
        //------------------------------------------------------------
        internal STAGELOCATION(LOCATION prev, COMPILER.STAGE stage)
            : base(prev)
        {
            this.Stage = stage;
        }

        //------------------------------------------------------------
        // STAGELOCATION Constructor (2)
        //------------------------------------------------------------
        internal STAGELOCATION(COMPILER.STAGE stage)
            : base()
        {
            this.Stage = stage;
        }

        //------------------------------------------------------------
        // STAGELOCATION.GetSymbol
        //------------------------------------------------------------
        override internal SYM GetSymbol()
        {
            return this.PreviousLocation != null ? this.PreviousLocation.GetSymbol() : null;
        }

        //------------------------------------------------------------
        // STAGELOCATION.GetFile
        //------------------------------------------------------------
        override internal INFILESYM GetFile()
        {
            return this.PreviousLocation != null ? this.PreviousLocation.GetFile() : null;
        }

        //------------------------------------------------------------
        // STAGELOCATION.GetNode
        //------------------------------------------------------------
        override internal BASENODE GetNode()
        {
            return this.PreviousLocation != null ? this.PreviousLocation.GetNode() : null;
        }

        //------------------------------------------------------------
        // STAGELOCATION.GetStage
        //------------------------------------------------------------
        override internal COMPILER.STAGE GetStage()
        {
            return this.Stage;
        }
    }

    //#define SETLOCATIONFILE(file)   FILELOCATION    _fileLocation(&compiler().location,    (file));
    //#define SETLOCATIONNODE(node)   NODELOCATION    _nodeLocation(&compiler().location,    (node));
    //#define SETLOCATIONSYM(sym)     SYMLOCATION     _symLocation (&compiler().location,    (sym));
    //#define SETLOCATIONSTAGE(stage) STAGELOCATION   _stageLocation(&compiler().location,   (COMPILER::stage));

    //// Utility function I couldn't find a better place for.
    //extern void RoundToFloat(double d, float * f);
    //extern BOOL SearchPathDual(PCWSTR lpPath, PCWSTR lpFileName, WCBuffer lpBuffer);
    //extern HINSTANCE FindAndLoadHelperLibrary(PCWSTR filename);

    //// Return the predefined void type
    //__forceinline TYPESYM * FUNCBREC::getVoidType()
    //{
    //    return compiler().MainSymbolManager.GetVoid();
    //}

    //bool GUIDFromString( PCWSTR szGuidStart,  PCWSTR szGuidEnd, GUID * pGUID);

    //#endif //__compiler_h__

    // INFILESYM  firstInfile()
    //    {
    //        SYM sym;
    //        for( sym = firstChild; sym!=null && !(sym.IsINFILESYM); sym = sym.nextChild)	;
    //        return ((sym!=null) ? sym.AsINFILESYM : null);
    //        }
    //
    //    internal RESFILESYM firstResfile()
    //    { 
    //        SYM sym;
    //        for( sym = firstChild; sym!=null && !(sym.IsRESFILESYM); sym = sym.nextChild);
    //        return ((sym!=null) ? sym.AsRESFILESYM : null);
    //        }
    //    
    //    POUTFILESYM nextOutfile() { return nextChild.AsOUTFILESYM; }
    //    bool isUnnamed() { return !wcscmp(name.text, L"?"); }

    /////////////////////////////////////////////////////////////////
    //
    // Compiler.cpp
    //
    /////////////////////////////////////////////////////////////////

    //#ifdef PLATFORM_UNIX
    //#define ENVIRONMENT_SEPARATOR L':'
    //#else   // PLATFORM_UNIX
    //#define ENVIRONMENT_SEPARATOR L';'
    //#endif  // PLATFORM_UNIX

    //#define MSCORPE_NAME        MAKEDLLNAME_W(L"mscorpe")
    //#define ALINK_NAME          MAKEDLLNAME_W(L"alink")

    //static const DWORD rgPEKind [] = {
    //    peILonly,                   // platformAgnostic
    //    peILonly | pe32BitRequired, // platformX86
    //    peILonly | pe32Plus,        // platformIA64
    //    peILonly | pe32Plus,        // platformAMD64
    //};

    //static const DWORD rgMachine [] = {
    //    IMAGE_FILE_MACHINE_I386,    // platformAgnostic
    //    IMAGE_FILE_MACHINE_I386,    // platformX86
    //    IMAGE_FILE_MACHINE_IA64,    // platformIA64
    //    IMAGE_FILE_MACHINE_AMD64,   // platformAMD64
    //};

    //C_ASSERT(lengthof(rgPEKind) == platformLast);
    //C_ASSERT(lengthof(rgMachine) == platformLast);

    //// Routine to load a helper DLL from a particular search path.
    //// We search the following path:
    ////   1. First, the directory where the compiler DLL is.
    ////   2. Second, the shim DLL.

    //HINSTANCE FindAndLoadHelperLibrary(PCWSTR filename)
    //{
    //    WCHAR path[MAX_PATH];
    //    WCHAR * pEnd;
    //    HINSTANCE hInstance;
    //
    //    // 1. The directory where the compiler DLL is.
    //    if (PAL_GetPALDirectoryW(path, lengthof(path)))
    //    {
    //        pEnd = wcsrchr(path, L'\\');
    //#ifdef PLATFORM_UNIX
    //        WCHAR *pEndSlash = wcschr(path, L'/');
    //        if (pEndSlash > pEnd) {
    //            pEnd = pEndSlash;
    //        }
    //#endif  // PLATFORM_UNIX
    //        if (pEnd && wcslen(filename) + pEnd - path + 1 < (int)lengthof(path)) {
    //            ++pEnd;  // point just beyond.
    //
    //            // Append new file
    //            if (SUCCEEDED(StringCchCopyW(pEnd, lengthof(path) - (pEnd - path), filename))) {
    //
    //                // Try to load it.
    //                if ((hInstance = LoadLibraryW(path)))
    //                    return hInstance;
    //            }
    //        }
    //    }
    //
    //    // 2. The shim DLL.
    //    HRESULT hr = LoadLibraryShim(filename, NULL, NULL, &hInstance);
    //    if (SUCCEEDED(hr) && hInstance)
    //        return hInstance;
    //        
    //    return 0;
    //}

    ///* Construct a compiler. All the real work
    // * is done in the Init() routine. This primary initializes
    // * all the sub-components.
    // */
    //#if defined(_MSC_VER)
    //#pragma warning(disable:4355)  // allow "this" in member initializer list
    //#endif  // defined(_MSC_VER)

    //#if defined(_MSC_VER)
    //#pragma warning(default:4355)
    //#endif  // defined(_MSC_VER)

    ///* Destruct the compiler. Make sure we've
    // * been deinitialized.
    // */
    //COMPILER::~COMPILER()
    //{
    //    if (isInited) {
    //        //ASSERT(0);      // We should have been terminated by now.
    //        Term(false);
    //    }
    //}

    ///*
    // * checks each element of the path list (separated by '|')
    // * to see if it exists, is accessible, and is a directory
    // *
    // * assumes that all ENVIRONMENT_SEPARATOR characters have
    // * been replaced by '|', and quotes have been stripped already
    // * idsSource is the source of the directory string
    // * (currently on "/LIB" or "LIB environment variable")
    // */
    //void COMPILER::CheckSearchPath(__inout PWSTR wzPathList, int idsSource)
    //{
    //    if (wzPathList == NULL)
    //        return;
    //
    //    WCHAR *pchContext = NULL;
    //    PWSTR wzDirName = wcstok_s(wzPathList, L"|", &pchContext);
    //    while (wzDirName) {
    //        // Only check non-empty path parts
    //        if (*wzDirName) {
    //            DWORD dwAttrib = W_GetFileAttributes(wzDirName);
    //            if (dwAttrib == INVALID_FILE_ATTRIBUTES) {
    //                // report the OS error
    //                DWORD dwErr = GetLastError();
    //                if (dwErr == ERROR_FILE_NOT_FOUND) // but change file-not-found to path-not-found
    //                    dwErr = ERROR_PATH_NOT_FOUND;
    //                Error(NULL, WRN_InvalidSearchPathDir, wzDirName, ErrArgIds(idsSource), ErrHR(HRESULT_FROM_WIN32(dwErr)));
    //            } else if (0 == (dwAttrib & FILE_ATTRIBUTE_DIRECTORY)) {
    //                // we found a file not a directory
    //                Error(NULL, WRN_InvalidSearchPathDir, wzDirName, ErrArgIds(idsSource), ErrHR(HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND)));
    //            }
    //        }
    //        wzDirName = wcstok_s(NULL, L"|", &pchContext);
    //
    //        if (wzDirName) {
    //            // Stick back in the separator, since wcstok_s set it to nul
    //            wzDirName[-1] = L'|';
    //        }
    //    }
    //}

    ///*
    // * Discards all state accumulated in the local heap
    // * and local symbols
    // */
    //void COMPILER::DiscardLocalState()
    //{
    //    localSymAlloc.FreeHeap();
    //    getLSymmgr().DestroyLocalSymbols();
    //    funcBRec.DiscardLocalState();
    //}

    //ITypeNameFactory * COMPILER::GetTypeNameFactory()
    //{
    //    if (! m_qtypenamefactory) {
    //        // Obtain the ITypeNameFactory.
    //        HRESULT hr = E_FAIL;
    //        if (FAILED(hr = PAL_CoCreateInstance(CLSID_TypeNameFactory,
    //                                         IID_ITypeNameFactory,
    //                                         (LPVOID *) & m_qtypenamefactory)))
    //        {
    //            Error(NULL, FTL_ComPlusInit, ErrHR(hr));
    //        }
    //    }
    //
    //    return m_qtypenamefactory;
    //}

    //ITypeNameBuilder * COMPILER::GetTypeNameBuilder()
    //{
    //    if (m_cnttnbUsage != 0) {
    //        // This guy is still being used, so get a new one
    //        m_qtypenamebuilder = NULL;
    //        m_cnttnbUsage = 0;
    //    }
    //
    //    if (! m_qtypenamebuilder) {
    //        // Obtain the ITypeNameFactory.
    //        HRESULT hr = E_FAIL;
    //        ITypeNameFactory * ptnf = GetTypeNameFactory();
    //        if (FAILED(hr = ptnf.GetTypeNameBuilder(&m_qtypenamebuilder)))
    //        {
    //            Error(NULL, FTL_ComPlusInit, ErrHR(hr));
    //        }
    //    }
    //
    //    m_cnttnbUsage++;
    //    ASSERT(m_cnttnbUsage == 1);
    //    return m_qtypenamebuilder;
    //}

    //#undef IfFailRet
    //#define IfFailRet(expr) if (FAILED((hr = (expr)))) return hr;
    //#undef IfFailGo
    //#define IfFailGo(expr) if (FAILED((hr = (expr)))) goto Error;

    //======================================================================
    // enum COMPILE_PHASE_ENUM  (PHASE_)
    //
    // These indicate phases in the compile. These are confusingly similar to the "compile stages",
    // yet different. They are only used for progress reporting.
    //======================================================================
    internal enum COMPILE_PHASE_ENUM : int
    {
        DECLARETYPES,
        IMPORTTYPES,
        DEFINE,
        PREPARE,
        CHECKIFACECHANGE,
        COMPILE,
        WRITEOUTPUT,
        WRITEINCFILE,

        MAX,
    }

    //static const int relativeTimeInPhase[PHASE_MAX]
    //  は class COMPILER の static メンバーとして定義する。

    ///*
    // * Add a conditional symbol to be processed by the lexer.
    // */
    //void COMPILER::AddConditionalSymbol(PNAME name)
    //{
    //    ccsymbols.Define (name);
    //}

    //#if DEBUG

    //#endif

    //// Return the dword which lives under HKCU\Software\Microsoft\C# Compiler<value>
    //// If any problems are encountered, return 0
    //DWORD COMPILER::GetRegDWORDRet(PCSTR value)
    //{
    //    return 0;
    //}

    //// Return true if the registry string which lives under HKCU\Software\Microsoft\C# Compiler<value>
    //// is the same one as the string provided.
    //bool COMPILER::IsRegStringRet(PCWSTR string, PCWSTR value)
    //{
    //    bool rval = false;
    //    return rval;
    //}

    //BSTR COMPILER::GetRegStringRet(PCSTR value)
    //{
    //    BSTR rval = NULL;
    //    return rval;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CompilerExceptionFilter
    ////
    ////  This hits whenever we hit an unhandled ASSERT or GPF in the compiler
    ////  Here we dump the entire LOCATION stack to the error channel.
    ////

    //void COMPILER::ReportICE(EXCEPTION_POINTERS * exceptionInfo)
    //{
    //    // We shouldn't get here on a fatal error exception
    //    ASSERT(exceptionInfo.ExceptionRecord.ExceptionCode != FATAL_EXCEPTION_CODE);
    //    if (exceptionInfo.ExceptionRecord.ExceptionCode == FATAL_EXCEPTION_CODE) {
    //        return;
    //    }
    //
    //    LOCATION * loc = location;
    //    if (exceptionInfo.ExceptionRecord.ExceptionCode == STATUS_STACK_OVERFLOW) {
    //        // Don't try for a source location because the SetLine, SetStart, and SetENd will also overflow
    //        while (loc && !stackOverflowLocation) {
    //            stackOverflowLocation = loc.getSymbol();
    //            loc = loc.getPrevious();
    //        }
    //        if (stackOverflowLocation == NULL && location)
    //            stackOverflowLocation = location.getFile();
    //    } else if (loc) {
    //
    //        //
    //        // dump probable culprit
    //        //
    //        Error(NULL, ERR_ICE_Culprit, exceptionInfo.ExceptionRecord.ExceptionCode, g_stages[loc.getStage()],
    //            ErrArgPtr(exceptionInfo.ExceptionRecord.ExceptionAddress));
    //
    //        //
    //        // dump location stack
    //        //
    //        do {
    //
    //            PCWSTR stage = g_stages[loc.getStage()];
    //
    //            //
    //            // dump one location
    //            //
    //            SYM *symbol = loc.getSymbol();
    //            if (symbol) {
    //                //
    //                // we have a symbol report it nicely
    //                //
    //                ErrorRef(NULL, ERR_ICE_Symbol, symbol, stage);
    //            } else {
    //                INFILESYM *file = loc.getFile();
    //                if (file) {
    //                    BASENODE *node = loc.getNode();
    //                    if (node) {
    //                        //
    //                        // we have stage, file and node
    //                        //
    //                        compiler().Error(node, ERR_ICE_Node, stage);
    //                    } else {
    //                        //
    //                        // we have stage and file
    //                        //
    //                        Error(ERRLOC(file), ERR_ICE_File, stage);
    //                    }
    //                } else {
    //                    //
    //                    // only thing we have is the stage
    //                    //
    //                    Error(NULL, ERR_ICE_Stage, stage);
    //                }
    //            }
    //
    //            loc = loc.getPrevious();
    //        } while (loc);
    //    } else {
    //        //
    //        // no location at all!
    //        //
    //        Error(NULL, ERR_ICE_Culprit, exceptionInfo.ExceptionRecord.ExceptionCode, g_stages[0],
    //            ErrArgPtr(exceptionInfo.ExceptionRecord.ExceptionAddress));
    //    }
    //}

    //void COMPILER::SetSigningOptions(mdToken FileToken)
    //{
    //    HRESULT hr = S_OK;
    //    const AssemblyOptions opt[] = { optAssemHalfSign, optAssemKeyFile, optAssemKeyName };
    //    const int optIndex[] = { OPTID_DELAYSIGN, OPTID_KEYFILE, OPTID_KEYNAME };
    //    const PCWSTR optCAName [] = { L"System.Reflection.AssemblyDelaySignAttribute",
    //        L"System.Reflection.AssemblyKeyFileAttribute",
    //        L"System.Reflection.AssemblyKeyNameAttribute" };
    //
    //    for (int i = 0; i < (int)lengthof(opt); i++) {
    //        CComBSTR bstrTemp;
    //        VARIANT var;
    //        switch (opt[i]) {
    //        case optAssemHalfSign:
    //            // COF_DEFAULTON actually doesn't equal TRUE or FALSE
    //            // we use that to detect if the option was set on the command-line
    //            ASSERT(TRUE != COF_DEFAULTON && FALSE != COF_DEFAULTON);
    //            if (CompilerOptions.m_fDELAYSIGN == COF_DEFAULTON) {
    //                continue;
    //            }
    //            else {
    //                V_VT (&var) = VT_BOOL;
    //                V_BOOL (&var) = (CompilerOptions.m_fDELAYSIGN != FALSE);
    //            }
    //            break;
    //        case optAssemKeyFile:
    //            if (CompilerOptions.m_sbstrKEYFILE == NULL)
    //                // Don't set it if it's NULL
    //                continue;
    //            if (CompilerOptions.m_fCompileSkeleton && PathIsRelativeW(CompilerOptions.m_sbstrKEYFILE)) {
    //                bstrTemp = L"..\\";
    //                bstrTemp.Append(CompilerOptions.m_sbstrKEYFILE);
    //                V_BSTR (&var) = bstrTemp;
    //            } else {
    //                V_BSTR (&var) = CompilerOptions.m_sbstrKEYFILE;
    //            }
    //            V_VT (&var) = VT_BSTR;
    //            break;
    //        case optAssemKeyName:
    //            if (CompilerOptions.m_sbstrKEYNAME == NULL)
    //                // Don't set it if it's NULL
    //                continue;
    //            V_VT (&var) = VT_BSTR;
    //            V_BSTR (&var) = CompilerOptions.m_sbstrKEYNAME;
    //            break;
    //        default:
    //            VSFAIL("Need to handle this option");
    //        }
    //     
    //        hr = linker.SetAssemblyProps(assemID, FileToken, opt[i], var);
    //        if (FAILED(hr)) {
    //            // Some unknown failure
    //            Error( NULL, ERR_ALinkFailed, ErrHR(hr));
    //        } else if (hr == S_FALSE) {
    //            // This means this is a dup
    //            Error(NULL, WRN_CmdOptionConflictsSource,
    //                CompilerOptions.m_rgOptionTable[COptionData::GetOptionIndex(optIndex[i])].pszDescSwitch, optCAName[i]);
    //        } else if (hr != S_OK) {
    //            Error( NULL, WRN_ALinkWarn, ErrHR(hr));
    //        }
    //    }
    //}

    //// Function to round a double to float precision. Does as an out-of-line
    //// function that isn't inlined so that the compiler won't optimize it
    //// away and keep higher precision.
    //void RoundToFloat(double d, float * f)
    //{
    //    *f = (float) d;
    //}

    //unsigned int COMPILER::getPredefIndex(TYPESYM * type)
    //{
    //    if (type.IsPTRSYM) {
    //        return PT_UINTPTR;
    //    } else if (type.IsAGGTYPESYM && type.isPredefined()) {
    //        PREDEFTYPE pt = type.getPredefType();
    //        if (type.isSimpleType() || pt == PT_INTPTR || pt == PT_UINTPTR) {
    //            return pt;
    //        }
    //    }
    //    return UNDEFINEDINDEX;
    //}

    //bool COMPILER::EmitRuntimeCompatibility()
    //{
    //    return noRuntimeCompatibility && GetOptPredefType(PT_RUNTIMECOMPATIBILITY);
    //}

    //bool COMPILER::WrapNonExceptionThrows()
    //{
    //    return wrapNonException && GetOptPredefType(PT_RUNTIMECOMPATIBILITY);
    //}

    //bool GUIDFromString( PCWSTR szGuidStart,  PCWSTR szGuidEnd, GUID * pGUID)
    //{
    //    ASSERT(szGuidStart);
    //    ASSERT(pGUID);
    //
    //    if (szGuidEnd == NULL)
    //        szGuidEnd = szGuidStart + wcslen(szGuidStart);
    //
    //    DWORD Data1;
    //    DWORD Data2, Data3, Data4, Data5;
    //    DWORD Data6, Data7, Data8, Data9, Data10, Data11;
    //    int cchScanned = 0;
    //    if (szGuidEnd - szGuidStart == 36) {
    //        if (11 != swscanf_s(szGuidStart, L"%8x-%4x-%4x-%2x%2x-%2x%2x%2x%2x%2x%2x%n", &Data1, &Data2,
    //            &Data3, &Data4, &Data5, &Data6, &Data7, &Data8, &Data9, &Data10, &Data11, &cchScanned) ||
    //            cchScanned != 36) 
    //        {
    //            return false;
    //        }
    //    } else if (szGuidEnd - szGuidStart != 38 ||
    //        11 != swscanf_s(szGuidStart, L"{%8x-%4x-%4x-%2x%2x-%2x%2x%2x%2x%2x%2x}%n", &Data1, &Data2,
    //            &Data3, &Data4, &Data5, &Data6, &Data7, &Data8, &Data9, &Data10, &Data11, &cchScanned) ||
    //        cchScanned != 38)
    //    {
    //        return false;
    //    }
    //
    //    pGUID.Data1 = Data1;
    //    pGUID.Data2 = (WORD)Data2;
    //    pGUID.Data3 = (WORD)Data3;
    //    pGUID.Data4[0] = (CHAR)Data4;
    //    pGUID.Data4[1] = (CHAR)Data5;
    //    pGUID.Data4[2] = (CHAR)Data6;
    //    pGUID.Data4[3] = (CHAR)Data7;
    //    pGUID.Data4[4] = (CHAR)Data8;
    //    pGUID.Data4[5] = (CHAR)Data9;
    //    pGUID.Data4[6] = (CHAR)Data10;
    //    pGUID.Data4[7] = (CHAR)Data11;
    //
    //    return true;
    //}

    //======================================================================
    //  class GuidUtil
    //
    /// <summary>
    /// <para>(CSharp\SCComp\Compiler.cs)</para>
    /// </summary>
    //======================================================================
    static internal class GuidUtil
    {
        /// <summary>
        /// <para>(CSharp\SCComp\Compiler.cs)</para>
        /// </summary>
        static internal Guid GuidZero = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        //------------------------------------------------------------
        // GuidUtil.GuidFromString
        //
        /// <summary>
        /// <para>(CSharp\SCComp\Compiler.cs)</para>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool GuidFromString(string str, out Guid id)
        {
            try
            {
                id = new Guid(str);
                return true;
            }
            catch (ArgumentNullException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            id = GuidZero;
            return false;
        }
    }
}
