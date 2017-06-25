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
// File: import.h
//
// Defines the structures used to import COM+ metadata into the symbol table.
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
// File: import.cpp
//
// Routines for importing COM+ metadata into the symbol table.
// ===========================================================================

//================================================================================
// Import.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//================================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Uncs
{
    static internal class ImportUtil
    {
        /// <summary>
        /// Maximum identifier size we allow.  This is the max the compiler allows
        /// </summary>
        internal const int MAX_IDENT_SIZE = 512;

        /// <summary>
        /// Maximum namespace or fully qualified identifier size we allow. Comes from corhdr.h
        /// </summary>
        internal const int MAX_FULLNAME_SIZE = Cor.MAX_CLASS_NAME;

        /// <summary>
        /// Separator for nested classes.
        /// </summary>
        internal const string NESTED_CLASS_SEP = "+";

        //#define MAGIC_SYMBOL_VALUE ((SYM*)-1)
    }

    //======================================================================
    // enum AssemblyComparisonResult (ACR_)
    //
    /// <summary>
    /// <para>(uncs\CSharp\Import.cs.
    /// In sscli, with prefix ACR_ defined in prebuilt\idl\fusion.h)</para>
    /// </summary>
    //======================================================================
    internal enum AssemblyComparisonResult : uint
    {
        Unknown = 0,
        EquivalentFullMatch = Unknown + 1,
        EquivalentWeakNamed = EquivalentFullMatch + 1,
        EquivalentFXUnified = EquivalentWeakNamed + 1,
        EquivalentUnified = EquivalentFXUnified + 1,
        NonEquivalentVersion = EquivalentUnified + 1,
        NonEquivalent = NonEquivalentVersion + 1,
        EquivalentPartialMatch = NonEquivalent + 1,
        EquivalentPartialWeakNamed = EquivalentPartialMatch + 1,
        EquivalentPartialUnified = EquivalentPartialWeakNamed + 1,
        EquivalentPartialFXUnified = EquivalentPartialUnified + 1,
        NonEquivalentPartialVersion = EquivalentPartialFXUnified + 1
    }

    //======================================================================
    // delegate CompareAssemblyIdentity
    //
    /// <summary>
    /// <para>Compare two assemblies.</para>
    /// </summary>
    /// <remarks>(sscli)
    /// typedef HRESULT (__stdcall *PfnCompareAssemblyIdentity)(
    ///     PCWSTR pwzAssemblyIdentity1,
    ///     BOOL fUnified1,
    ///     PCWSTR pwzAssemblyIdentity2,
    ///     BOOL fUnified2,
    ///     BOOL *pfEquivalent,
    ///     AssemblyComparisonResult *pResult); 
    /// </remarks>
    /// <param name="assembly1"></param>
    /// <param name="assemblyName1"></param>
    /// <param name="assembly2"></param>
    /// <param name="assemblyName2"></param>
    /// <returns></returns>
    //======================================================================
    internal delegate AssemblyComparisonResult CompareAssemblyIdentity(
        Assembly assembly1,
        AssemblyName assemblyName1,
        Assembly assembly2,
        AssemblyName assemblyName2);

    //======================================================================
    // class IMPORTED_CUSTOM_ATTRIBUTES
    //
    /// <summary>
    /// (CSharp\SCComp\Import.cs)
    /// </summary>
    //======================================================================
    internal class IMPORTED_CUSTOM_ATTRIBUTES
    {
        // decimal literal
        internal bool HasDecimalLiteral = false;    // bool hasDecimalLiteral;
        internal decimal DecimalLiteral = 0;        // DECIMAL decimalLiteral;

        // deprecated
        internal bool IsDeprecated = false;         // bool isDeprecated;
        internal bool IsDeprecatedError = false;    // bool isDeprecatedError;
        internal string DeprecatedString = null;    // WCHAR *deprecatedString;

        // CLS
        internal bool HasCLSattribute = false;      // WCHAR *deprecatedString;
        internal bool IsCLS = false;                // bool isCLS;

        // attribute
        internal bool AllowMultiple = false;                // bool allowMultiple;
        internal AttributeTargets AttributeKind = 0;        // CorAttributeTargets attributeKind;

        // conditional
        internal List<string> ConditionalSymbols = null;    // NAMELIST **conditionalHead;
        // NAMELIST *conditionalSymbols;

        // parameter lists
        internal bool IsParamListArray = false;     // bool isParamListArray;

        // RequiredCustomAttribute
        internal bool HasRequiredAttribute = false; // bool hasRequiredAttribute;

        // default member
        internal string DefaultMember = null;	    // WCHAR * defaultMember;

        // ComImport/CoClass
        internal Type CoClass = null;               // WCHAR * CoClassName;

        // For fixed sized buffers
        internal TYPESYM FixedBufferTypeSym = null;	// TYPESYM * fixedBuffer;
        internal int FixedBufferElementCount;       // int fixedBufferElementCount;

        // For CompilationRelaxationsAttribute(CompilationsRelaxations.NoStringInterning)
        // Only checked at the assembly level (on added modules)
        internal bool HasCompilationRelaxations = false;

        // For RuntimeCompatibilityAttribute
        // Only checked at the assembly level (on added modules)
        internal bool HasRuntimeCompatibility = false;  // bool fRuntimeCompatibility;
        internal bool WrapNonExceptionThrows = false;   // bool fWrapNonExceptionThrows;

        // for security attribute warning
        internal bool HasLinkDemand = false;    // bool hasLinkDemand;	

        // Whether the assembly or module declares any friends.
        internal bool HasFriends = false;       // bool fHasFriends;

        // (CS3) Extension method?
        internal bool IsExtensionMethod = false;    // 2016/02/12 hirano567@hotmail.co.jp
    }

    //======================================================================
    // interface ImportScope
    //
    /// <summary>
    /// <para>(sscli) implemented by ImportScopeModule.</para>
    /// <para>(CSharp\SCComp\Import.cs)</para>
    /// </summary>
    //======================================================================
    internal interface ImportScope
    {
        //virtual IMetaDataImport * GetMetaImport() = 0;
        //virtual IMetaDataAssemblyImport * GetAssemblyImport() = 0;

        // May return NULL!
        Module GetModule();                 // virtual MODULESYM * GetModule() = 0;

        Assembly GetAssembly();             // virtual int GetAssemblyID() = 0;

        // May return NULL!
        MODULESYM GetModuleSym();           // virtual MODULESYM * GetModule() = 0;

        int GetAssemblyID();                // virtual int GetAssemblyID() = 0;

        int GetModuleID();                  // virtual int GetModuleID() = 0;

        string GetFileName(bool fullPath);  // virtual PCWSTR GetFileName() = 0;
    }

    //======================================================================
    // class ImportScopeModule
    //
    /// <summary>
    /// <para>Has an IMPORTER instance and a MODULESYM instance.</para>
    /// <para>(CSharp\SCComp\Import.cs)</para>
    /// </summary>
    //======================================================================
    internal class ImportScopeModule : ImportScope
    {
        private IMPORTER importer;      // IMPORTER * m_import;
        private MODULESYM moduleSym;    // MODULESYM * m_mod;

        //------------------------------------------------------------
        // ImportScopeModule Constructor
        //
        /// <summary></summary>
        /// <param name="import"></param>
        /// <param name="mod"></param>
        //------------------------------------------------------------
        public ImportScopeModule(IMPORTER import, MODULESYM mod)
        {
            importer = import;
            moduleSym = mod;
        }

        //------------------------------------------------------------
        // ImportScopeModule.GetMetaImport
        //------------------------------------------------------------
        //virtual public AssemblyEx GetMetaImport()
        //{
        //    return moduleSym.GetMetaImport(importer.Compiler);
        //}

        //------------------------------------------------------------
        // ImportScopeModule.GetAssemblyImport
        //------------------------------------------------------------
        //virtual public AssemblyEx GetAssemblyImport()
        //{
        //    return moduleSym.GetAssemblyImport(importer.Compiler);
        //}

        //------------------------------------------------------------
        // ImportScopeModule.GetModule
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public Module GetModule()
        {
            return (moduleSym != null ? moduleSym.Module : null);
        }

        //------------------------------------------------------------
        // ImportScopeModule.GetAssembly
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public Assembly GetAssembly()
        {
            Module mod = GetModule();
            return (mod != null ? mod.Assembly : null);
        }

        //------------------------------------------------------------
        // ImportScopeModule.GetModuleSym
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public MODULESYM GetModuleSym()
        {
            return moduleSym;
        }

        //------------------------------------------------------------
        // ImportScopeModule.GetAssemblyID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public int GetAssemblyID()
        {
            return moduleSym.GetAssemblyID();
        }

        //------------------------------------------------------------
        // ImportScopeModule.GetModuleID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public int GetModuleID()
        {
            return moduleSym.GetModuleID();
        }

        //------------------------------------------------------------
        // ImportScopeModule.GetFileName
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual public string GetFileName(bool fullPath)
        {
            if (fullPath)
            {
                return moduleSym.GetInputFile().FullName;
            }
            return moduleSym.GetInputFile().Name;
        }
    }

    //======================================================================
    // class AssemblyIdentityComparison
    //
    /// <summary>
    /// <para>wrapper class for comparing assembly references via the Fusion APIs</para>
    /// <para>(CSharp\SCComp\Import.cs)</para>
    /// </summary>
    //======================================================================
    internal class AssemblyIdentityComparison
    {
        /// <summary></summary>
        /// <remarks>
        /// (sscli) PfnCompareAssemblyIdentity m_pfnCompareAssemblyIdentity;
        /// </remarks>
        private CompareAssemblyIdentity compareAssembly;

        /// <summary></summary>
        /// <remarks>(sscli) BOOL m_fEqual;</remarks>
        private bool isEqual = false;

        /// <summary></summary>
        /// <remarks>
        /// (sscli) AssemblyComparisonResult m_acrResult;
        /// </remarks>
        private AssemblyComparisonResult result = AssemblyComparisonResult.Unknown;

        /// <summary></summary>
        /// <remarks>
        /// (sscli) bool m_fAllowUnification;
        /// </remarks>
        private bool allowUnification = false;

        //------------------------------------------------------------
        // AssemblyIdentityComparison   Constructor
        //
        /// <summary>
        /// Constructor. need a valid DelegateCompareAssembly.
        /// </summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        internal AssemblyIdentityComparison(CompareAssemblyIdentity compare)
        {
            DebugUtil.Assert(compare != null);
            compareAssembly = compare;
        }

        //------------------------------------------------------------
        // AssemblyIdentityComparison.Compare
        //
        /// <summary>
        /// <para>Compare two assemblies.</para>
        /// <para>Use GetResult method to get the result.</para>
        /// </summary>
        /// <param name="assembly1"></param>
        /// <param name="assemblyName1"></param>
        /// <param name="assembly2"></param>
        /// <param name="assemblyName2"></param>
        //------------------------------------------------------------
        internal void Compare(
            Assembly assembly1, AssemblyName assemblyName1,
            Assembly assembly2, AssemblyName assemblyName2)
        {

            DebugUtil.Assert(compareAssembly != null);
            //allowUnification = fUnify1 || fUnify2;
            this.result = AssemblyComparisonResult.Unknown;
            this.isEqual = false;
            this.result = compareAssembly(assembly1, assemblyName1, assembly2, assemblyName2);

            DebugUtil.Assert(!isEqual || IsEquivalentNoUnify() || CouldUnify(false));
        }

        //------------------------------------------------------------
        // AssemblyIdentityComparison.IsEquivalent
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsEquivalent()
        {
            return isEqual;
        }

        //------------------------------------------------------------
        // AssemblyIdentityComparison.IsEquivalentNoUnify
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsEquivalentNoUnify()
        {
            return (
                // all fields match
                (result == AssemblyComparisonResult.EquivalentFullMatch) ||
                // match based on weak-name, version numbers ignored
                (result == AssemblyComparisonResult.EquivalentWeakNamed) ||
                (result == AssemblyComparisonResult.EquivalentPartialMatch) ||
                (result == AssemblyComparisonResult.EquivalentPartialWeakNamed));
        }

        //------------------------------------------------------------
        // AssemblyIdentityComparison.CouldUnify
        //
        /// <summary>
        /// <para>(In sscli, fOnlyFXUnification has the defautl value false.)</para>
        /// </summary>
        /// <param name="fOnlyFXUnification"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CouldUnify(bool fOnlyFXUnification)   // = false
        {
            bool fCouldUnify =
                ((result == AssemblyComparisonResult.EquivalentFXUnified) ||
                // match based on FX-unification of version numbers
                (result == AssemblyComparisonResult.EquivalentPartialFXUnified));

            if (!fOnlyFXUnification)
            {
                fCouldUnify |=
                    ((result == AssemblyComparisonResult.EquivalentUnified) ||
                    // match based on legacy-unification of version numbers
                    (result == AssemblyComparisonResult.EquivalentPartialUnified));
            }
            return fCouldUnify;
        }

        //------------------------------------------------------------
        // AssemblyIdentityComparison.NonEquivalentDueToVersions
        //
        /// <summary>
        /// This only returns true when the assemblies are determined to not be equal and do not unify
        /// (i.e. it does not handle version differences if assemblies unify,
        /// as is the case of AssemblyComparisonResult.EquivalentUnified.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool NonEquivalentDueToVersions()
        {
            DebugUtil.Assert(!IsEquivalent() && allowUnification);
            return (
                // all fields match except version field
                (result == AssemblyComparisonResult.NonEquivalentVersion) ||
                (result == AssemblyComparisonResult.NonEquivalentPartialVersion));
        }

        //------------------------------------------------------------
        // AssemblyIdentityComparison.GetResult
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AssemblyComparisonResult GetResult()
        {
            return result;
        }
    }

    //======================================================================
    // enum NameResOptionsEnum
    //
    /// <summary>
    /// Options for ResolveTypeName.
    /// When the name is unresolvable for some reason,
    /// these values determine whether ResolveTypeName creates and
    /// returns an UNRESAGGSYM or returns NULL.
    /// </summary>
    //======================================================================
    internal enum NameResOptionsEnum : int
    {
        /// <summary>
        /// Create UNRESAGGSYM only if the aid is for an unresolved module we resolve
        /// to a type forwarder which is not resolvable.
        /// </summary>
        Normal,

        /// <summary>
        /// Never create an UNRESAGGSYM
        /// </summary>
        FavorNull,

        /// <summary>
        /// Create an UNRESAGGSYM (avoid NULL) whenever possible
        /// </summary>
        FavorUnres,
    }

    //======================================================================
    // Imported Custom Attributes
    //======================================================================
    static internal partial class Util
    {
    }

    //======================================================================
    // class IMPORTER
    //
    /// <summary></summary>
    //======================================================================
    internal partial class IMPORTER
    {
        //------------------------------------------------------------
        // enum ImportSigOptions (kfiso)
        //------------------------------------------------------------
        [Flags]
        internal enum ImportSigOptions : int
        {
            None = 0x00,
            AllowVoid = 0x01,
            AllowByref = 0x02,
            IncludeModOpts = 0x04,
        }

        //------------------------------------------------------------
        // delegate IMPORTER.DelegateCreateAssemblyNameObject
        //
        /// <summary></summary>
        /// <remarks>(sscli)
        /// typedef HRESULT (__stdcall *PfnCreateAssemblyNameObject)(
        ///     LPASSEMBLYNAME *ppAssemblyNameObj,
        ///     PCWSTR szAssemblyName,
        ///     DWORD dwFlags,
        ///     LPVOID pvReserved);
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="reserved"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal delegate AssemblyName DelegateCreateAssemblyNameObject(
            string name,
            int flags,
            object reserved);

        //------------------------------------------------------------
        // IMPORTER Fields and Properties
        //------------------------------------------------------------
        private COMPILER compiler = null;

        internal COMPILER Compiler
        {
            get { return compiler; }
        }

        private bool inited = false;        // inited
        private bool loadingTypes = false;  // fLoadingTypes

        //private PfnCreateAssemblyNameObject m_pfnCreateAssemblyNameObject;
        //private PfnCompareAssemblyIdentity m_pfnCompareAssemblyIdentity;

        private string ErrorAssemblyName = null;    // NAME * m_nameErrorAssem;

        private const BindingFlags defaultBindingFlags =
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static;

#if DEBUG
        StringBuilder debugSb = null;
#endif

        //------------------------------------------------------------
        // IMPORTER Constructor
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        internal IMPORTER(COMPILER comp)
        {
            this.compiler = comp;
            DebugUtil.Assert(this.compiler != null);
        }

        //------------------------------------------------------------
        // IMPORTER.Init
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Init()
        {
            this.inited = true;
        }

        //------------------------------------------------------------
        // IMPORTER.Term
        //
        /// <summary>
        /// <para>Terminate everything.</para>
        /// <para>In sscli, free memories, so nothing to do.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void Term() { }

        //------------------------------------------------------------
        // IMPORTER.ImportAllTypes
        //
        /// <summary>
        /// <para>Import all top level types from all metadata scopes.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void ImportAllTypes()
        {
            Compiler.SetLocation(COMPILER.STAGE.IMPORT);   //SETLOCATIONSTAGE(IMPORT);

            OUTFILESYM outfile;
            INFILESYM infile;

            outfile = Compiler.MainSymbolManager.MetadataFileRootSym;

            if (outfile != null)
            {
                int oldErrors = Compiler.ErrorCount();

                //--------------------------------------------------
                // Go through all the inputfile symbols and do an open scope.
                // Get the information of assemblies and its attribute, definitions of types
                // from each INFILESYM which is a child of MetadataFileRootSym.
                //--------------------------------------------------
                for (infile = outfile.FirstInFileSym();
                    infile != null;
                    infile = infile.NextInFileSym())
                {
                    if (!infile.IsSource)
                    {
                        OpenMetadataFile(infile);
                    }
                }

                CErrorSuppression es = null;

                // if we couldn't open one of the references, stop here and don't import types
                if (Compiler.FAbortEarly(oldErrors, es))
                {
                    return;
                }

                //--------------------------------------------------
                // Go through all the inputfile symbols
                // and check for unification conflicts
                //--------------------------------------------------
                for (infile = outfile.FirstInFileSym();
                    infile != null;
                    infile = infile.NextInFileSym())
                {
                    if (!infile.IsSource && !infile.IsModule && !infile.IsBogus)
                    {
                        for (INFILESYM infileOther = outfile.FirstInFileSym();
                            infileOther != infile;
                            infileOther = infileOther.NextInFileSym())
                        {
                            if (infileOther.IsSource ||
                                infileOther.IsModule ||
                                infileOther.IsBogus)
                            {
                                continue;
                            }
                            if (CompareImports(infileOther, infile))
                            {
                                break;
                            }
                        }
                    }
                }

                //--------------------------------------------------
                // Go through all the inputfile symbols.
                // and import the types.
                //
                // We get custom attributes for assembly, but not import types now.
                // We shall import types when needs come.
                //--------------------------------------------------
                for (infile = outfile.FirstInFileSym();
                    infile != null;
                    infile = infile.NextInFileSym())
                {
                    if (!infile.IsSource && !infile.IsBogus)
                    {
                        ImportNamespaces(infile, false);
                    }
                }
            }
        }

        //------------------------------------------------------------
        // IMPORTER::ImportAllTypes (sscli)
        //------------------------------------------------------------
        //void IMPORTER::ImportAllTypes()
        //{
        //    SETLOCATIONSTAGE(IMPORT);
        //
        //    POUTFILESYM outfile;
        //    PINFILESYM  infile;
        //
        //    outfile = compiler()->getBSymmgr().GetMDFileRoot();
        //
        //    if (outfile) {
        //        int oldErrors = compiler()->ErrorCount();
        //        // Go through all the inputfile symbols
        //        // and do an open scope.
        //        for (infile = outfile->firstInfile();
        //             infile != NULL;
        //             infile = infile->nextInfile())
        //        {
        //            if (!infile->isSource) {
        //                OpenAssembly(infile);
        //            }
        //        }
        //
        //        CErrorSuppression es;
        //
        //        // if we couldn't open one of the references, stop here and don't import types
        //        if (compiler()->FAbortEarly(oldErrors, &es))
        //            return;
        //
        //        // Go through all the inputfile symbols
        //        // and check for unification conflicts
        //        for (infile = outfile->firstInfile();
        //             infile != NULL;
        //             infile = infile->nextInfile())
        //        {
        //            if (!infile->isSource && !infile->isAddedModule && !infile->getBogus())
        //            {
        //                for ( INFILESYM * infileOther = outfile->firstInfile();
        //                    infileOther != infile;
        //                    infileOther = infileOther->nextInfile())
        //                {
        //                    if (infileOther->isSource || infileOther->isAddedModule || infileOther->getBogus())
        //                        continue;
        //                    if (CompareImports(infileOther, infile))
        //                        break;
        //                }
        //            }
        //        }
        //
        //        // Go through all the inputfile symbols.
        //        // and import the types.
        //        for (infile = outfile->firstInfile();
        //             infile != NULL;
        //             infile = infile->nextInfile())
        //        {
        //            if (!infile->isSource && !infile->getBogus()) {
        //                ImportNamespaces(infile,
        //                    false
        //                    );
        //            }
        //        }
        //    }
        //}

        //------------------------------------------------------------
        // IMPORTER.LoadTypesInNsAid (1)
        //
        /// <summary>
        /// Load types in the given namespace and aid.
        /// If aid is kaidNil, don't filter on aid.
        /// If infile is not null, only load types from that infile.
        /// </summary>
        /// <param name="assemblyId"></param>
        /// <param name="inFileSym"></param>
        /// <param name="nsSym"></param>
        //------------------------------------------------------------
        internal void LoadTypesInNsAid(NSSYM nsSym, int assemblyId, INFILESYM infileSym)
        {
            // CAUTION: Make sure that this doesn't call anything that might cause recursion into here!
            if (loadingTypes)
            {
                DebugUtil.Assert(false, "Shouldn't be recursing in loading types!");
                return;
            }

            NSDECLSYM nsdSym = null;
            if (infileSym == null)
            {
                nsdSym = nsSym.FirstDeclSym;
                if (nsdSym == null)
                {
                    return;
                }
            }

            loadingTypes = true;
            bool clearThisAssembly = false;

            while (true)
            {
                DebugUtil.Assert(nsdSym == null || nsdSym.BagSym == nsSym);
                int aid;

                if (nsdSym != null)
                {
                    infileSym = nsdSym.InFileSym;
                }

                if (!infileSym.IsSource &&
                    (assemblyId == Kaid.Nil || infileSym.InAlias(assemblyId)) &&
                    nsSym.TypesUnloaded(aid = infileSym.GetAssemblyID()))
                {
                    if (infileSym.IsModule)
                    {
                        // We can't clear the bit for added modules yet since there may be more than
                        // one module (they share the same assembly id).
                        DebugUtil.Assert(aid == Kaid.ThisAssembly);
                        clearThisAssembly = true;
                    }
                    else
                    {
                        nsSym.ClearTypesUnloaded(aid);
                    }

                    //for (SYM sym = inFileSym.FirstChild; sym != null; sym = sym.NextSym)
                    //{
                    //    if (sym.IsMODULESYM)
                    //    {
                    //        LoadTypesInNsFromMetadataFile(nsSym, inFileSym);
                    //    }
                    //}
                    LoadTypesInNsFromMetadataFile(nsSym, infileSym);
                }
                // Get the next infile.
                if (nsdSym == null || (nsdSym = nsdSym.NextDeclSym) == null)
                {
                    break;
                }
            }

            DebugUtil.Assert(loadingTypes);
            loadingTypes = false;
        }

        //------------------------------------------------------------
        // IMPORTER.LoadTypesInNsAid (2)
        //
        /// <summary>
        /// Load types in the given namespace and aid.
        /// If aid is kaidNil, don't filter on aid.
        /// If infile is not null, only load types from that infile.
        /// </summary>
        /// <param name="assemblyId"></param>
        /// <param name="inFileSym"></param>
        /// <param name="nsSym"></param>
        //------------------------------------------------------------
        internal void LoadTypesInNsAid(
            string name,
            NSSYM nsSym,
            int arity,
            int assemblyId,
            INFILESYM infileSym)
        {
            // CAUTION: Make sure that this doesn't call anything that might cause recursion into here!
            if (loadingTypes)
            {
                DebugUtil.Assert(false, "Shouldn't be recursing in loading types!");
                return;
            }

            NSDECLSYM nsdSym = null;
            if (infileSym == null)
            {
                nsdSym = nsSym.FirstDeclSym;
                if (nsdSym == null)
                {
                    return;
                }
            }

            this.loadingTypes = true;
            bool clearThisAssembly = false;

            while (true)
            {
                DebugUtil.Assert(nsdSym == null || nsdSym.BagSym == nsSym);
                int aid;

                if (nsdSym != null)
                {
                    infileSym = nsdSym.InFileSym;
                }

                if (!infileSym.IsSource &&
                    (assemblyId == Kaid.Nil || infileSym.InAlias(assemblyId)) &&
                    nsSym.TypesUnloaded(aid = infileSym.GetAssemblyID()))
                {
                    if (infileSym.IsModule)
                    {
                        // We can't clear the bit for added modules yet since there may be more than
                        // one module (they share the same assembly id).
                        DebugUtil.Assert(aid == Kaid.ThisAssembly);
                        clearThisAssembly = true;
                    }
                    else
                    {
                        //nsSym.ClearTypesUnloaded(aid);
                    }

                    //for (SYM sym = inFileSym.FirstChild; sym != null; sym = sym.NextSym)
                    //{
                    //    if (sym.IsMODULESYM)
                    //    {
                    //        LoadTypesInNsFromMetadataFile(nsSym, inFileSym);
                    //    }
                    //}
                    //LoadTypesInNsFromMetadataFile(nsSym, infileSym);
                    LoadOneTypeInNsFromMetadataFile(name, nsSym, arity, infileSym);
                }
                // Get the next infile.
                if (nsdSym == null || (nsdSym = nsdSym.NextDeclSym) == null)
                {
                    break;
                }
            }

            DebugUtil.Assert(loadingTypes);
            loadingTypes = false;
        }

        //------------------------------------------------------------
        // IMPORTER.LoadTypesInNsFromMetadataFile (LoadTypesInNsMod in sscli)
        //
        /// <summary>
        /// <para>(sscli)Load the types in the given namespace from the given module.</para>
        /// <para>Load from the given metadata file.</para>
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="inFileSym"></param>
        //------------------------------------------------------------
        internal void LoadTypesInNsFromMetadataFile(NSSYM ns, INFILESYM inFileSym)
        {
            DebugUtil.Assert(loadingTypes);
            if (inFileSym == null || inFileSym.MetadataFile == null)
            {
                return;
            }
            CNameToTypeDictionary dic = inFileSym.MetadataFile.NameToTypeDictionary;

            foreach (KeyValuePair<CNameToTypeDictionary.Key, CNameToTypeDictionary.TypeInfo> kv
                in dic.TypeDictionary)
            {
                if (kv.Key.parentSym == ns)
                {
                    CNameToTypeDictionary.TypeInfo info = kv.Value;
                    // We need to load this type.
                    // Don't increment itnpDst - we don't need it anymore.
                    if (!info.Imported)
                    {
                        info.Sym = ImportOneType(inFileSym, info.Type);
                    }
                }
            }
            DebugUtil.Assert(loadingTypes);
        }

        //------------------------------------------------------------
        // IMPORTER.LoadOneTypeInNsFromMetadataFile
        //
        /// <summary>
        /// <para>(sscli)Load the types in the given namespace from the given module.</para>
        /// <para>Load from the given metadata file.</para>
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="inFileSym"></param>
        //------------------------------------------------------------
        internal void LoadOneTypeInNsFromMetadataFile(
            string name,
            NSSYM ns,
            int arity,
            INFILESYM infileSym)
        {
            DebugUtil.Assert(this.loadingTypes);
            if (infileSym == null || infileSym.MetadataFile == null)
            {
                return;
            }

            string fullName = ReflectionUtil.CreateTypeSearchName(ns, name, arity);
            Type type = infileSym.MetadataFile.GetType(fullName);
            if (infileSym != null && type != null)
            {
                ImportOneType(infileSym, type);
            }

            DebugUtil.Assert(loadingTypes);
        }

        //------------------------------------------------------------
        // IMPORTER.GetBaseTokenAndFlags
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        /// <param name="baseAggTypeSym"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal void GetBaseTokenAndFlags(
            AGGSYM aggSym,
            out AGGTYPESYM baseAggTypeSym,
            out TypeAttributes flags)
        {
            Compiler.SetLocation(COMPILER.STAGE.IMPORT);    //SETLOCATIONSTAGE(IMPORT);
            Compiler.SetLocation(aggSym);                   //SETLOCATIONSYM(aggSym);
            DebugUtil.Assert(aggSym != null && aggSym.Type != null);

            flags = aggSym.Type.Attributes;

            if (aggSym.Type.BaseType != null)
            {
                baseAggTypeSym = ResolveBaseRef(aggSym.InFileSym, aggSym.Type.BaseType, aggSym, true);
            }
            else
            {
                baseAggTypeSym = null;
            }
        }
        //------------------------------------------------------------
        // IMPORTER::GetBaseTokenAndFlags (sscli)
        //------------------------------------------------------------
        //void IMPORTER::GetBaseTokenAndFlags(AGGSYM *sym, AGGTYPESYM  **base, DWORD *flags)
        //{
        //    SETLOCATIONSTAGE(IMPORT);
        //    SETLOCATIONSYM(sym);
        //
        //    ASSERT(TypeFromToken(sym->tokenImport) == mdtTypeDef);
        //
        //    mdToken baseToken;
        //
        //    // Get the meta-data import interface.
        //    IMetaDataImport * metaimport = sym->GetMetaImport(compiler());
        //    ASSERT(metaimport);
        //
        //    CheckHR(metaimport->GetTypeDefProps(sym->tokenImport,
        //            NULL, 0, NULL,            // Type name
        //            flags,                    // Flags
        //            &baseToken                // Extends
        //           ), sym);
        //
        //    if (!IsNilToken(baseToken))
        //        *base = ResolveBaseRef(sym->GetModule(), baseToken, sym, true);
        //    else
        //        *base = NULL;
        //}

        //------------------------------------------------------------
        // IMPORTER.AccessFromTypeFlags
        //
        /// <summary>
        /// Given type flags, get the access level.
        /// The sym is passed in just to make a determination on what assembly it is in; it is not changed.
        /// </summary>
        /// <param name="typeAttr"></param>
        /// <param name="inFileSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ACCESS AccessFromTypeFlags(TypeAttributes typeAttr, INFILESYM inFileSym)
        {
            switch (typeAttr & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.NotPublic:
                    return ACCESS.INTERNAL;
                case TypeAttributes.Public:
                    return ACCESS.PUBLIC;
                case TypeAttributes.NestedPublic:
                    return ACCESS.PUBLIC;
                case TypeAttributes.NestedPrivate:
                    return ACCESS.PRIVATE;
                case TypeAttributes.NestedFamily:
                    return ACCESS.PROTECTED;
                case TypeAttributes.NestedAssembly:
                    return ACCESS.INTERNAL;
                case TypeAttributes.NestedFamORAssem:
                    return ACCESS.INTERNALPROTECTED;
                case TypeAttributes.NestedFamANDAssem:
                    if (inFileSym != null &&
                        (inFileSym.GetAssemblyID() == Kaid.ThisAssembly ||
                        inFileSym.InternalsVisibleTo(Kaid.ThisAssembly)))
                    {
                        return ACCESS.PROTECTED;
                    }
                    return ACCESS.INTERNAL;
                default:
                    return ACCESS.PRIVATE;
            }
        }

        //------------------------------------------------------------
        // IMPORTER::AccessFromTypeFlags (sscli)
        //------------------------------------------------------------
        //    ACCESS AccessFromTypeFlags(uint flags, INFILESYM * infile);
        //ACCESS IMPORTER::AccessFromTypeFlags(uint flags, INFILESYM * infile)
        //{
        //    switch (flags & tdVisibilityMask)
        //    {
        //    case tdNotPublic:
        //        return ACC_INTERNAL;
        //    case tdPublic:
        //        return ACC_PUBLIC;
        //    case tdNestedPublic:
        //        return ACC_PUBLIC;
        //    case tdNestedPrivate:
        //        return ACC_PRIVATE;
        //    case tdNestedFamily:
        //        return ACC_PROTECTED;
        //    case tdNestedAssembly:
        //        return ACC_INTERNAL;
        //    case tdNestedFamORAssem:
        //        return ACC_INTERNALPROTECTED;
        //    case tdNestedFamANDAssem:
        //        // We don't support this directly. Treat as protected if in this assembly or in one that
        //        // gave us friend right; internal otherwise.
        //        if (infile->GetAssemblyID() == kaidThisAssembly || infile->InternalsVisibleTo(kaidThisAssembly))
        //            return ACC_PROTECTED;
        //        return ACC_INTERNAL;
        //    default:
        //        ASSERT(0);
        //        return ACC_PRIVATE;
        //    }
        //}

        //------------------------------------------------------------
        // IMPORTER.GetEnumUnderlyingType
        //
        /// <summary>
        /// If aggSym is of enum type, Find the AGGSYM instance for underlying type
        /// and set it to UnderlyingTypeSym field.
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void GetEnumUnderlyingType(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.IsEnum);
            if (aggSym.UnderlyingTypeSym != null)
            {
                return;
            }

            Type enumType = aggSym.Type;
            Type underlyingType = null;
            if (enumType == null) return;

            // In enum type, its underling type is set to the type of a instance field.
            // (enum type has only one instance field.)
            // The underling type is set to the type of "value__" field.

            FieldInfo[] fis = enumType.GetFields();
            foreach (FieldInfo fi in fis)
            {
                if (fi.IsStatic)
                {
                    continue;
                }
                underlyingType = fi.FieldType;
                break;
            }

            // The underlying types of enums are integer types.
            // They are registered by BSYMMGR.InitPredefinedTypes in COMPILER.CompileAll.

            AGGSYM sym = Compiler.MainSymbolManager.InfoToSymbolTable.GetSymFromInfo(underlyingType) as AGGSYM;
            if (sym != null&&
                compiler.ClsDeclRec.ResolveInheritanceRec(sym))
            {
                aggSym.UnderlyingTypeSym = sym.GetThisType();
            }
        }
        //------------------------------------------------------------
        // IMPORTER::GetEnumUnderlyingType (sscli)
        //------------------------------------------------------------
        //void IMPORTER::GetEnumUnderlyingType(AGGSYM * agg, MODULESYM * scope)
        //{
        //    ASSERT(agg->IsEnum() && !agg->underlyingType);
        //
        //    IMetaDataImport * metaimport = scope->GetMetaImport(compiler());
        //    HCORENUM corenum = 0;
        //    mdToken tokens[32];
        //    ULONG cTokens, iToken;
        //
        //    do {
        //        // Get next batch of fields.
        //        CheckHR(metaimport->EnumFields(&corenum, agg->tokenImport, tokens, lengthof(tokens), &cTokens), agg);
        //
        //        // Process each field.
        //        for (iToken = 0; iToken < cTokens; ++iToken) {
        //        
        //            if (TypeFromToken(tokens[iToken]) == mdtFieldDef) {
        //
        //                DWORD flags;
        //                PCCOR_SIGNATURE signature;
        //                ULONG cbSignature;
        //
        //                // Get properties of the field from metadata.
        //                CheckHR(metaimport->GetFieldProps(
        //                    tokens[iToken],                                         // The field for which to get props.
        //                    NULL,                                                   // Put field's class here.
        //                    NULL, NULL, NULL,                                       // Field name
        //                    & flags,                                                // Field flags
        //                    & signature, & cbSignature,                             // Field signature
        //                    NULL, NULL, NULL),                  // Field constant value
        //                            agg);
        //
        //                // Enums are a bit special. Non-static fields serve only to record the
        //                // underlying integral type, and are otherwise ignored. Static fields are
        //                // enumerators and must be of the enum type. (We change other integral ones to the
        //                // enum type because it's probably what the emitting compiler meant.)
        //                if (!(flags & fdStatic)) {
        //
        //                    // Import the type of the field.
        //                    PTYPESYM type;
        //                    bool dummy;
        //                    type = ImportFieldType(scope, signature, signature + cbSignature, &dummy, NULL);
        //
        //                    // Assuming its an integral type, use it to set the
        //                    // enum base type.
        //                    if (type && type->isNumericType() && compiler()->clsDeclRec.ResolveInheritanceRec(type->getAggregate()) &&
        //                        type->fundType() <= FT_LASTINTEGRAL)
        //                    {
        //                        if (!agg->underlyingType) {
        //                            agg->underlyingType = type->AsAGGTYPESYM;
        //                        }
        //                        else {
        //                            // Cannot have more than one non-static field used to determine underlying type
        //                            agg->setBogus(true);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    } while (cTokens > 0);
        //    metaimport->CloseEnum(corenum);
        //}

        //------------------------------------------------------------
        // IMPORTER.ResolveInheritance
        //
        /// <summary>
        /// Resolves the inheritance hierarchy (and interfaces) for a type.
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void ResolveInheritance(AGGSYM aggSym)
        {
#if DEBUG
            if (aggSym.SymID == 1399)
            {
                ;
            }
#endif
            Compiler.SetLocation(COMPILER.STAGE.IMPORT);   //SETLOCATIONSTAGE(IMPORT);
            Compiler.SetLocation(aggSym);                  //SETLOCATIONSYM(aggSym);

            //IMetaDataImport * metaimport;
            TypeAttributes flags = 0;
            AGGTYPESYM baseAggTypeSym = null;

            //HCORENUM corenum;           // For enumerating tokens.
            //mdToken tokens[32];
            //ULONG cTokens, iToken;

            // CLSDREC::ResolveInheritanceRec should have set this.
            // We should only be called by CLSDREC::ResolveInheritanceRec.
            DebugUtil.Assert(aggSym.AggState == AggStateEnum.ResolvingInheritance);

            // Get the meta-data import interface.
            //MODULESYM * scope = aggSym.GetModule();
            //metaimport = scope.GetMetaImport(Compiler);
            //DebugUtil.Assert(metaimport);

            //--------------------------------------------------------
            // Import base class. Set underlying type.
            //--------------------------------------------------------
            DebugUtil.Assert(aggSym.BaseClassSym == null);
            DebugUtil.Assert(aggSym.UnderlyingTypeSym == null);

            GetBaseTokenAndFlags(aggSym, out baseAggTypeSym, out flags);

            // The flags should match what we saw before....
            DebugUtil.Assert(aggSym.Access == AccessFromTypeFlags(flags,
                (aggSym.DeclOnly() != null ? aggSym.DeclOnly().GetInputFile() : null)));

            if (baseAggTypeSym != null &&
                !Compiler.ClsDeclRec.ResolveInheritanceRec(baseAggTypeSym.GetAggregate()))
            {
                // Detected a cycle
                if (baseAggTypeSym.GetAggregate().IsResolvingBaseClasses)
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_ImportedCircularBase,
                        new ErrArgRef(baseAggTypeSym),
                        new ErrArgRef(aggSym));
                }
                baseAggTypeSym.GetAggregate().SetBogus(true);
                aggSym.SetBogus(true);

                // Redirect to object.
                baseAggTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, false);
            }

            switch (aggSym.AggKind)
            {
                case AggKindEnum.Interface:
                    DebugUtil.Assert(Cor.IsTypeDefInterface((int)flags));
                    DebugUtil.Assert(aggSym.IsAbstract && !aggSym.IsSealed);
                    DebugUtil.Assert(baseAggTypeSym == null);
                    break;

                case AggKindEnum.Class:
#if false
                    DebugUtil.Assert(
                        !aggSym.IsAbstract == (((CorTypeAttr)flags & CorTypeAttr.Abstract) == 0));
                    DebugUtil.Assert(
                        !aggSym.IsSealed == (((CorTypeAttr)flags & CorTypeAttr.Sealed) == 0));
#endif
                    DebugUtil.Assert(
                        !aggSym.IsAbstract == (((TypeAttributes)flags & TypeAttributes.Abstract) == 0));
                    DebugUtil.Assert(
                        !aggSym.IsSealed == (((TypeAttributes)flags & TypeAttributes.Sealed) == 0));
                    DebugUtil.Assert(
                        baseAggTypeSym == null ||
                        !baseAggTypeSym.IsPredefType(PREDEFTYPE.VALUE) ||
                        aggSym.IsPredefAgg(PREDEFTYPE.ENUM));
                    if (baseAggTypeSym == null)
                    {
                        AGGSYM obj = Compiler.GetReqPredefAgg(PREDEFTYPE.OBJECT, false);
                        if (aggSym != obj) baseAggTypeSym = obj.GetThisType();
                    }
                    Compiler.SetBaseType(aggSym, baseAggTypeSym);
                    break;

                case AggKindEnum.Delegate:
                    DebugUtil.Assert(
                        baseAggTypeSym != null &&
                        (baseAggTypeSym.IsPredefType(PREDEFTYPE.DELEGATE) ||
                        baseAggTypeSym.IsPredefType(PREDEFTYPE.MULTIDEL)));
#if false
                    DebugUtil.Assert(
                        !aggSym.IsAbstract == (((CorTypeAttr)flags & CorTypeAttr.Abstract) == 0));
                    DebugUtil.Assert(
                        !aggSym.IsSealed == (((CorTypeAttr)flags & CorTypeAttr.Sealed) == 0));
#endif
                    DebugUtil.Assert(
                        !aggSym.IsAbstract == (((TypeAttributes)flags & TypeAttributes.Abstract) == 0));
                    DebugUtil.Assert(
                        !aggSym.IsSealed == (((TypeAttributes)flags & TypeAttributes.Sealed) == 0));
                    Compiler.SetBaseType(aggSym, baseAggTypeSym);

                    // Note: we may morph this into a class later if we don't find the invoke method or ctor.
                    // if it is marked abstract, then change it now since it is an invalid delegate
                    if (aggSym.IsAbstract)
                    {
                        aggSym.AggKind = AggKindEnum.Class;
                    }
                    break;

                case AggKindEnum.Struct:
                    // If the enum has type parameters, we treat it as a struct.
                    DebugUtil.Assert(
                        baseAggTypeSym != null &&
                        (baseAggTypeSym.IsPredefType(PREDEFTYPE.VALUE) ||
                            baseAggTypeSym.IsPredefType(PREDEFTYPE.ENUM) && aggSym.TypeVariables.Count > 0)
                        );
                    DebugUtil.Assert(!aggSym.IsPredefAgg(PREDEFTYPE.ENUM));
                    DebugUtil.Assert(!aggSym.IsAbstract && aggSym.IsSealed);
                    Compiler.SetBaseType(aggSym, Compiler.GetReqPredefType(PREDEFTYPE.VALUE, false));
                    break;

                case AggKindEnum.Enum:
                    DebugUtil.Assert(baseAggTypeSym != null && baseAggTypeSym.IsPredefType(PREDEFTYPE.ENUM));
                    DebugUtil.Assert(!aggSym.IsAbstract && aggSym.IsSealed);
                    Compiler.SetBaseType(aggSym, Compiler.GetReqPredefType(PREDEFTYPE.ENUM, false));

                    GetEnumUnderlyingType(aggSym);
                    if (aggSym.UnderlyingTypeSym == null)
                    {
                        // Treat it as a struct....
                        aggSym.AggKind = AggKindEnum.Struct;
                    }
                    break;

                default:
                    break;
            }

            //--------------------------------------------------------
            // Import interfaces.
            //--------------------------------------------------------
            //AGGTYPESYM * rgiface[8];
            //int cifaceMax = lengthof(rgiface);
            //AGGTYPESYM ** prgiface = rgiface;
            //int ciface = 0;

            //corenum = 0;
            //do {
            //    // Get next batch of interfaces.
            //    CheckHR(metaimport.EnumInterfaceImpls(
            //        &corenum, aggSym.tokenImport, tokens, lengthof(tokens), &cTokens), aggSym);

            Type[] interfaceImpls = aggSym.Type.GetInterfaces();
            TypeArray ifaceArray = new TypeArray();

            // Process each interface.
            foreach (Type ifaceType in interfaceImpls)
            {
                AGGTYPESYM ifaceAts = null;

                // Get the interface.
                ifaceAts = ImportInterface(aggSym.InFileSym, ifaceType, aggSym);

                // Add to the interface list if interesting.
                if (ifaceAts != null)
                {
                    if (!Compiler.ClsDeclRec.ResolveInheritanceRec(ifaceAts.GetAggregate()))
                    {
                        if (baseAggTypeSym.GetAggregate().IsResolvingBaseClasses)
                        {
                            Compiler.ErrorRef(
                                null,
                                CSCERRID.ERR_ImportedCircularBase,
                                new ErrArgRef(ifaceAts.GetAggregate()),
                                new ErrArgRef(aggSym));
                        }
                        continue;
                    }

                    if (!ifaceAts.IsInterfaceType())
                    {
                        continue;
                    }
                    ifaceArray.Add(ifaceAts);

                    //if (ciface >= cifaceMax) {
                    //    DebugUtil.Assert(ciface == cifaceMax);
                    //    int cifaceMaxNew = 2 * cifaceMax;
                    //    AGGTYPESYM ** prgifaceNew = STACK_ALLOC(AGGTYPESYM *, cifaceMaxNew);
                    //    memcpy(prgifaceNew, prgiface, cifaceMax * sizeof(AGGTYPESYM *));
                    //    prgiface = prgifaceNew;
                    //    cifaceMax = cifaceMaxNew;
                    //}
                    //DebugUtil.Assert(ciface < cifaceMax);
                    //prgiface[ciface++] = ifaceAts;
                }
            }
            //} while (cTokens > 0);

            //metaimport.CloseEnum(corenum);

            Compiler.SetIfaces(aggSym, ifaceArray);

            DebugUtil.Assert(aggSym.AggState == AggStateEnum.ResolvingInheritance);
            aggSym.AggState = AggStateEnum.Inheritance;
        }

        //------------------------------------------------------------
        // IMPORTER.DefineBounds
        //
        /// <summary>
        /// <para></para>
        /// </summary>
        /// <param name="parentSym"></param>
        //------------------------------------------------------------
        internal void DefineBounds(PARENTSYM parentSym)
        {
            DebugUtil.Assert(parentSym.IsAGGSYM || parentSym.IsMETHSYM);

            TypeArray typeVars = null;
            TypeArray outerTypeVars = null;
            TypeArray classTypeVars = null;
            TypeArray methodTypeVars = null;
            //mdToken tokImport;
            AGGSYM aggSym = null;
            Type[] genericParameterTypes = null;

            switch (parentSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SK in DefineBounds!");
                    return;

                case SYMKIND.AGGSYM:
                    aggSym = parentSym as AGGSYM;
                    DebugUtil.Assert(aggSym != null);

                    // Advance the state.
                    DebugUtil.Assert(aggSym.HasResolvedBaseClasses);
                    if (aggSym.AggState >= AggStateEnum.Bounds)
                    {
                        DebugUtil.VsFail("Why are we here?");
                        return;
                    }

                    AGGSYM outerAggSym = aggSym.GetOuterAgg();
                    if (outerAggSym != null)
                    {
                        Compiler.EnsureState(outerAggSym, AggStateEnum.Bounds);
                    }

                    aggSym.AggState = AggStateEnum.Bounds;

                    typeVars = aggSym.AllTypeVariables;
                    if (typeVars.Count == 0)
                    {
                        return;
                    }
                    outerTypeVars = parentSym.ParentSym.IsAGGSYM ?
                        (parentSym.ParentSym as AGGSYM).AllTypeVariables : BSYMMGR.EmptyTypeArray;
                    DebugUtil.Assert(
                        outerTypeVars != null &&
                        outerTypeVars.Count == typeVars.Count - aggSym.TypeVariables.Count);
                    classTypeVars = typeVars;
                    methodTypeVars = null;
                    //tokImport = aggSym.tokenImport;

                    DebugUtil.Assert(aggSym.Type != null);
                    genericParameterTypes = aggSym.Type.GetGenericArguments();
                    break;

                case SYMKIND.METHSYM:
                    METHSYM methodSym = parentSym as METHSYM;
                    DebugUtil.Assert(methodSym != null);
                    typeVars = methodSym.TypeVariables;
                    if (typeVars.Count == 0)
                    {
                        return;
                    }
                    outerTypeVars = BSYMMGR.EmptyTypeArray;
                    classTypeVars = methodSym.ClassSym.AllTypeVariables;
                    methodTypeVars = typeVars;
                    //tokImport = parentSym.AsMETHSYM.tokenImport;

                    DebugUtil.Assert(methodSym.MethodInfo != null);
                    genericParameterTypes = methodSym.MethodInfo.GetGenericArguments();
                    break;
            }
            //DebugUtil.Assert(tokImport);
            DebugUtil.Assert(genericParameterTypes != null);

            int cvar = typeVars.Count;
            DebugUtil.Assert(cvar > 0);
            bool isBogus = false;

            if (!parentSym.HasBogus || !parentSym.CheckBogus())
            {
                //MODULESYM * scope = parentSym.GetModule();
                //IMetaDataImport2 * metaimport = scope.GetMetaImportV2(Compiler);
                //DebugUtil.Assert(metaimport);

                //mdGenericParam * prgtokVars = STACK_ALLOC_ZERO(mdGenericParam, cvar);
                //HCORENUM enumVars = 0;
                //LONG ctok = 0;

                //----------------------------------------
                // Get the tokens.
                //----------------------------------------
                //CheckHR(metaimport.EnumGenericParams(
                //    &enumVars,
                //    tokImport,
                //    prgtokVars,
                //    cvar,
                //    (ULONG *)&ctok),
                //    scope.getInputFile());
                //metaimport.CloseEnum(enumVars);

                DebugUtil.Assert(genericParameterTypes.Length == cvar);

                //TYPESYM * rgtypeBnd[8];
                //mdGenericParamConstraint rgtokCon[8];
                //DebugUtil.Assert(lengthof(rgtypeBnd) == lengthof(rgtokCon));
                //int cbndMax = lengthof(rgtokCon);
                //TYPESYM ** prgtypeBnd = rgtypeBnd;
                //mdGenericParamConstraint * prgtokCon = rgtokCon;

                //----------------------------------------
                // Process each type variable.
                //----------------------------------------
                for (int itok = 0; itok < cvar; ++itok)
                {
                    //mdToken tokPar;
                    //WCHAR rgchName[MAX_FULLNAME_SIZE];
                    //ULONG cchName;

                    //------------------------------
                    // Get index, flags, owner, name.
                    //------------------------------
                    //CheckHR(metaimport.GetGenericParamProps(prgtokVars[itok], (ULONG *)&ivar, &flags, &tokPar, null,
                    //        rgchName, lengthof(rgchName), &cchName),
                    //    scope.getInputFile());
                    //DebugUtil.Assert(tokPar == tokImport);

                    Type genericType = genericParameterTypes[itok];
                    int ivar = genericType.GenericParameterPosition;
                    GenericParameterAttributes flags = genericType.GenericParameterAttributes;

                    DebugUtil.Assert(0 <= ivar && ivar < cvar);
                    TYVARSYM tvSym = typeVars.ItemAsTYVARSYM(ivar);

                    TypeArray bnds = new TypeArray();
                    int ctype = 0;

                    // Get the tokens for the bounds.

                    Type[] boundTypes = genericType.GetGenericParameterConstraints();
                    int cbnd = boundTypes.Length;
                    //HCORENUM enumCons = 0;

                    //CheckHR(metaimport.EnumGenericParamConstraints(
                    //    &enumCons, prgtokVars[itok], null, 0, (ULONG *)&cbnd), scope.getInputFile());
                    //CheckHR(metaimport.CountEnum(enumCons, (ULONG *)&cbnd), scope.getInputFile());

                    //if (cbnd > 0)
                    //{
                    //    if (cbnd > cbndMax) {
                    //        cbndMax += cbndMax;
                    //        if (cbndMax < cbnd)
                    //            cbndMax = cbnd;
                    //        prgtypeBnd = STACK_ALLOC_ZERO(TYPESYM *, cbndMax);
                    //        prgtokCon = STACK_ALLOC_ZERO(mdGenericParamConstraint, cbndMax);
                    //    }
                    //    CheckHR(metaimport.EnumGenericParamConstraints(
                    //        &enumCons, prgtokVars[itok], prgtokCon, cbndMax, (ULONG *)&ctok), scope.getInputFile());
                    //    DebugUtil.Assert(ctok == cbnd);
                    //}
                    //metaimport.CloseEnum(enumCons);

                    //ctype = 0;
                    for (int ibnd = 0; ibnd < cbnd; ibnd++)
                    {
                        //mdGenericParam tokVar;
                        //mdToken tokBnd;

                        //--------------------
                        // Get the owner and the type of each constraint.
                        //--------------------
                        //CheckHR(metaimport.GetGenericParamConstraintProps(
                        //    prgtokCon[ibnd], &tokVar, &tokBnd), scope.getInputFile());

                        //TYPESYM * type = ResolveTypeRefOrSpec(scope, tokBnd, typeVarsCls, typeVarsMeth);
                        //if (!type) {
                        //    parentSym.setBogus(true);
                        //    goto LBogus;
                        //}
                        //if (!type.GetNakedType().IsTYVARSYM)
                        //    Compiler.EnsureState(type, AggState::Inheritance);
                        //// Don't keep object in the list.
                        //if (!type.isPredefType(PREDEFTYPE.OBJECT))
                        //    prgtypeBnd[ctype++] = type;
                    }

                    bnds = Compiler.MainSymbolManager.AllocParams(bnds);

                    SpecialConstraintEnum cons = SpecialConstraintEnum.None;
                    if ((flags & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                    {
                        cons |= SpecialConstraintEnum.Reference;
                    }
                    if ((flags & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                    {
                        cons |= SpecialConstraintEnum.Value;
                    }
                    if ((cons & SpecialConstraintEnum.Value) == 0 &&
                        (flags & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                    {
                        cons |= SpecialConstraintEnum.New;
                    }

                    if (ivar < outerTypeVars.Count)
                    {
                        // Verify that the constraints match those on the outer type!

                        //DebugUtil.Assert(tvSym.FResolved());
                        //if ((uint)cons != tvSym.cons || bnds.size != tvSym.GetBnds().size) {
                        //    parentSym.setBogus(true);
                        //    goto LBogus;
                        //}
                        //TypeArray * bndsOuter = tvSym.GetBnds();
                        //if (bndsOuter != bnds) {
                        //    for (int i = 0; i < bnds.size; i++) {
                        //        if (!bndsOuter.Contains(bnds.Item(i))) {
                        //            parentSym.setBogus(true);
                        //            goto LBogus;
                        //        }
                        //        if (!bnds.Contains(bndsOuter.Item(i))) {
                        //            parentSym.setBogus(true);
                        //            goto LBogus;
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        DebugUtil.Assert(tvSym.BoundArray == null);
                        Compiler.SetBounds(tvSym, bnds, false);
                        tvSym.Constraints = cons;

                        // If the interface list is bogus the sym will get set to bogus.

                        if (parentSym.HasBogus && parentSym.CheckBogus())
                        {
                            isBogus = true;
                            goto LBogus;
                        }
                    }
                }
            }
            else
            {
                isBogus = true;
            }

        LBogus:
            if (isBogus)
            {
                // The parent is already known to be bogus, so just set all the constraints to empty.
                for (int i = outerTypeVars.Count; i < cvar; ++i)
                {
                    TYVARSYM tvSym = typeVars.ItemAsTYVARSYM(i);
                    if (tvSym.BoundArray == null)
                    {
                        Compiler.SetBounds(tvSym, BSYMMGR.EmptyTypeArray, false);
                    }
                }
            }

            // Resolve all the bounds.
            for (int i = outerTypeVars.Count; i < cvar; i++)
            {
                TYVARSYM tvSym = typeVars.ItemAsTYVARSYM(i);
                DebugUtil.VsVerify(Compiler.ResolveBounds(tvSym, true), "ResolveBounds failed!");   // VSVERIFY
                DebugUtil.Assert(tvSym.FResolved());
            }
        }

        //------------------------------------------------------------
        // IMPORTER.DefineImportedType
        //
        /// <summary>
        /// Given a type that previous imported via ImportAllTypes,
        /// make it "declared" by importing all of its members and
        /// declaring its base classes/interfaces.
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void DefineImportedType(AGGSYM aggSym)
        {
            DebugUtil.Assert(Compiler.CompilationPhase >= CompilerPhaseEnum.DefineMembers);

            Compiler.SetLocation(COMPILER.STAGE.IMPORT);	// SETLOCATIONSTAGE(IMPORT);
            Compiler.SetLocation(aggSym);			        // SETLOCATIONSYM(aggSym);

#if DEBUG
            if (aggSym.SymID == 2479)
            {
                ;
            }
            DebugUtil.Assert(!COMPILER.IsRegString(aggSym.Name, "DefineImportedType"));
#endif

            if (aggSym.IsDefined)
            {
                return;
            }

#if DEBUG
            Compiler.HaveDefinedAnyType = true;
#endif

            if (!aggSym.HasResolvedBaseClasses)
            {
                DebugUtil.VsVerify(
                    Compiler.ClsDeclRec.ResolveInheritanceRec(aggSym),
                    "ResolveInheritanceRec failed in DefineImportedType!");
                DebugUtil.Assert(aggSym.HasResolvedBaseClasses);
            }

            if (aggSym.AggState < AggStateEnum.Bounds)
            {
                DefineBounds(aggSym);
                DebugUtil.Assert(aggSym.AggState >= AggStateEnum.Bounds);
            }

            // Bogus types aren't handled at all -- don't import anything about them.
            if (aggSym.IsBogus)
            {
                aggSym.AggState = AggStateEnum.DefinedMembers;
                return;
            }

            // ResolveInheritance determined the underlyingType of an enum.
            DebugUtil.Assert(!aggSym.IsEnum || aggSym.UnderlyingTypeSym != null);

            DebugUtil.Assert(aggSym.AggState < AggStateEnum.DefiningMembers);
            aggSym.AggState = AggStateEnum.DefiningMembers;

            // No point importing anything for inaccessible types - except structs.
            // We need their fields for managed/unmanaged and checking recursion.
            if (aggSym.IsPrivateMetadata && !aggSym.IsStruct)
            {
                DebugUtil.Assert(aggSym.AggState == AggStateEnum.DefiningMembers);
                aggSym.AggState = AggStateEnum.DefinedMembers;
                return;
            }

            // Get the meta-data import interface.
            AGGDECLSYM aggDeclSym = aggSym.DeclOnly();
            //MODULESYM * scope = aggSym.GetModule();
            INFILESYM infileSym = aggDeclSym.GetInputFile();
            //IMetaDataImport * metaimport = scope.GetMetaImport(Compiler);
            //DebugUtil.Assert(metaimport);

            //HCORENUM corenum;           // For enumerating tokens.
            //mdToken tokens[32];
            //ULONG cTokens, iToken;
            IMPORTED_CUSTOM_ATTRIBUTES attributes = new IMPORTED_CUSTOM_ATTRIBUTES();

            Type aggType = aggSym.Type;
            DebugUtil.Assert(aggType != null);

            //--------------------------------------------------
            // Check to see if the class has any explicit interface impls
            //--------------------------------------------------
            if (aggSym.IsStruct)
            {
                //corenum = 0;
                //mdToken tokensBody[1];
                //mdToken tokensDecl[1];

                // Get next batch of fields.

                //CheckHR(metaimport.EnumMethodImpls(
                //    &corenum, aggSym.tokenImport, tokensBody, tokensDecl, lengthof(tokensBody), &cTokens), infileSym);
                //
                //if (cTokens)
                //{
                //    aggSym.hasExplicitImpl = true;
                //}
                //metaimport.CloseEnum(corenum);

                if (aggSym.InterfaceCountAll > 0)
                {
                    aggSym.HasExplicitImpl = true;
                }
            }

            //--------------------------------------------------
            // Import all the attributes we are interested in at compile time
            //--------------------------------------------------
            //ImportCustomAttributes(scope, null, &attributes, aggSym.tokenImport); 
            ImportCustomAttributes(infileSym, aggType.GetCustomAttributes(true), attributes);

            aggSym.SetBogus(attributes.HasRequiredAttribute || (aggSym.HasBogus && aggSym.IsBogus));
            if (!aggSym.IsInterface)
            {
                //corenum = 0;
                //do
                //{
                //    // Get next batch of fields.
                //    CheckHR(metaimport.EnumFields(
                //    	&corenum, aggSym.tokenImport, tokens, lengthof(tokens), &cTokens), infileSym);
                //
                //    // Process each field.
                //    for (iToken = 0; iToken < cTokens; ++iToken)
                //    {
                //        ImportField(aggSym, aggDeclSym, tokens[iToken]);
                //    }
                //} while (cTokens > 0);
                //metaimport.CloseEnum(corenum);

                FieldInfo[] infos = aggType.GetFields(IMPORTER.defaultBindingFlags);
                foreach (FieldInfo info in infos)
                {
                    ImportField(infileSym, aggSym, aggDeclSym, info);
                }
            }
            else
            {
                if (aggSym.IsComImport && attributes.CoClass != null)
                {
                    DebugUtil.Assert(aggSym.UnderlyingTypeSym == null);
                    aggSym.ComImportCoClass = attributes.CoClass.Name;
                }
            }
            aggSym.HasLinkDemand = attributes.HasLinkDemand;

            //--------------------------------------------------
            // Import methods and method impls. Enums don't have them.
            //--------------------------------------------------
            if (!aggSym.IsEnum && !aggSym.IsPrivateMetadata)
            {
                //corenum = 0;
                //do
                //{
                //    // Get next batch of methods.
                //    CheckHR(metaimport.EnumMethods(
                //    	&corenum, aggSym.tokenImport, tokens, lengthof(tokens), &cTokens), infileSym);

                //    // Process each method.
                //    for (iToken = 0; iToken < cTokens; ++iToken)
                //    {
                //        ImportMethod(aggSym, aggDeclSym, tokens[iToken]);
                //    }
                //} while (cTokens > 0);
                //metaimport.CloseEnum(corenum);

                MethodInfo[] methInfos = aggType.GetMethods(IMPORTER.defaultBindingFlags);
                foreach (MethodInfo methInfo in methInfos)
                {
                    ImportMethod(infileSym, aggSym, aggDeclSym, methInfo);
                }

                ConstructorInfo[] ctorInfos = aggType.GetConstructors(defaultBindingFlags);
                foreach (ConstructorInfo ctorInfo in ctorInfos)
                {
                    ImportMethod(infileSym, aggSym, aggDeclSym, ctorInfo);
                }

                // get method impls
                //corenum = 0;
                //mdToken tokensDecl[32];

                //----------------------------------------
                // MethodImpl
                //
                // if we need the value of isOverride,
                // use MethodInfo.GetBaseDefinition and get it.
                //----------------------------------------
                //do // MethodImpl
                //{
                //    // get next batch of methodimpls
                //    CheckHR(metaimport.EnumMethodImpls(
                //        &corenum, aggSym.tokenImport, tokens, tokensDecl, lengthof(tokens), &cTokens), infileSym);

                //    for (iToken = 0; iToken < cTokens; iToken += 1)
                //    {
                //        // find the type token of the impl's aggDeclSym
                //        mdToken tokenClassOfImpl;
                //        if (TypeFromToken(tokensDecl[iToken]) == mdtMethodDef && tokensDecl[iToken] != mdMethodDefNil)
                //        {
                //            // Get method properties.
                //            CheckHR(metaimport.GetMethodProps(
                //                tokensDecl[iToken],  // The method for which to get props.
                //                &tokenClassOfImpl,   // Put method's class here.
                //                0,  0, 0,            // Method name
                //                null,                // Put flags here.
                //                null, null,          // Method signature
                //                null,                // codeRVA
                //                null), infileSym);   // Impl. Flags
                //        }
                //        else if (
                //        	TypeFromToken(tokensDecl[iToken]) == mdtMemberRef &&
                //        	tokensDecl[iToken] != mdMemberRefNil)
                //        {
                //            CheckHR(metaimport.GetMemberRefProps(
                //            	tokensDecl[iToken], &tokenClassOfImpl, 0, 0, 0, 0, 0), infileSym);
                //        }
                //        else
                //        {
                //            // Unknown method impl kind .. ignore it below
                //            tokenClassOfImpl = 0;
                //        }

                //        // a method impl to a class type means the method must be an override
                //        TYPESYM * typeImpl = ResolveTypeRefOrDef(scope, tokenClassOfImpl, null);
                //        if (typeImpl && typeImpl.isAGGTYPESYM() && !typeImpl.isInterfaceType()) {
                //            METHSYM *method;
                //            method = FindMethodDef(aggSym, tokens[iToken]);
                //            if (method && method.isVirtual)
                //                method.isOverride = true;
                //        }
                //    }
                //}
                //while (cTokens > 0);
                //metaimport.CloseEnum(corenum);

                //----------------------------------------
                // Import properties.
                // These must be done after methods, because properties refer to methods.
                //----------------------------------------
                string defaultMemberName = null;

                if (attributes.DefaultMember != null)
                {
                    defaultMemberName = attributes.DefaultMember;
                    Compiler.NameManager.AddString(defaultMemberName);
                }

                //corenum = 0;
                //do
                //{
                //    // Get next batch of properties.
                //    CheckHR(metaimport.EnumProperties(
                //    	&corenum, aggSym.tokenImport, tokens, lengthof(tokens), &cTokens), infileSym);

                //    // Process each property.
                //    for (iToken = 0; iToken < cTokens; ++iToken)
                //    {
                //        ImportProperty(aggSym, aggDeclSym, tokens[iToken], defaultMemberName);
                //    }
                //}
                //while (cTokens > 0);
                //metaimport.CloseEnum(corenum);

#if DEBUG
                if (aggSym.SymID == 393)
                {
                    ;
                }
#endif
                PropertyInfo[] propInfos = aggType.GetProperties(IMPORTER.defaultBindingFlags);
                foreach (PropertyInfo propInfo in propInfos)
                {
                    ImportProperty(infileSym, aggSym, aggDeclSym, propInfo, defaultMemberName);
                }

                //----------------------------------------
                // Import events.
                // These must be done after methods, because events refer to methods.
                //----------------------------------------
                //corenum = 0;
                //do {
                //    // Get next batch of events.
                //    CheckHR(metaimport.EnumEvents(
                //    	&corenum, aggSym.tokenImport, tokens, lengthof(tokens), &cTokens), infileSym);
                //
                //    // Process each event.
                //    for (iToken = 0; iToken < cTokens; ++iToken) {
                //        ImportEvent(aggSym, aggDeclSym, tokens[iToken]);
                //    }
                //} while (cTokens > 0);
                //metaimport.CloseEnum(corenum);

                EventInfo[] eventInfos = aggType.GetEvents();
                foreach (EventInfo eventInfo in eventInfos)
                {
                    ImportEvent(infileSym, aggSym, aggDeclSym, eventInfo);
                }

            } // if (!aggSym.IsEnum && !aggSym.IsPrivateMetadata)

            if (aggSym.IsDelegate)
            {
                DebugUtil.Assert(
                    aggSym.BaseClassSym != null &&
                    !aggSym.IsAbstract &&
                    (aggSym.BaseClassSym.IsPredefType(PREDEFTYPE.DELEGATE) ||
                    aggSym.BaseClassSym.IsPredefType(PREDEFTYPE.MULTIDEL)));

                // We have something which looks like a delegate. Make sure that it has the correct
                // constructor and an invoke method.

                METHSYM invokeMethodSym = null;
                METHSYM ctorMethodSym = null;

                for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
                {
                    switch (child.Kind)
                    {
                        case SYMKIND.METHSYM:
                            {
                                // check for constructor or invoke
                                METHSYM method = child as METHSYM;

                                if (method.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE))
                                {
                                    if (invokeMethodSym == null && method.Access == ACCESS.PUBLIC)
                                    {
                                        invokeMethodSym = method;
                                        continue;
                                    }
                                }
                                else if (method.IsCtor)
                                {
                                    if (ctorMethodSym == null &&
                                        method.Access == ACCESS.PUBLIC &&
                                        method.ParameterTypes.Count == 2 &&
                                        method.ParameterTypes[0].IsPredefType(PREDEFTYPE.OBJECT) &&
                                        (method.ParameterTypes[1].IsPredefType(PREDEFTYPE.UINTPTR) ||
                                            method.ParameterTypes[1].IsPredefType(PREDEFTYPE.INTPTR)))
                                    {
                                        ctorMethodSym = method;
                                        continue;
                                    }
                                }
                                // found a non-delegatelike method
                                break;
                            }
                        // other members are ignored
                        default:
                            break;
                    }
                }

                if (invokeMethodSym != null && ctorMethodSym != null)
                {
                    invokeMethodSym.MethodKind = MethodKindEnum.Invoke;
                    if (invokeMethodSym.IsBogus || ctorMethodSym.IsBogus)
                    {
                        aggSym.SetBogus(true);
                    }
                }
                else
                {
                    // Treat it as a class.
                    aggSym.AggKind = AggKindEnum.Class;
                }
            }

            if (aggSym.BaseClassSym != null)
            {
                // if there is a sealed type in the inheritance heirarchy
                //  we treat it as a bogus type in order to get an error at compile time if this is used.
                //  If we allow it, the frameworks will throw a TypeLoadException at runtime.
                if (aggSym.BaseClassSym.GetAggregate().IsSealed)
                {
                    aggSym.SetBogus(true);
                }
                aggSym.HasConversion |= aggSym.BaseClassSym.GetAggregate().HasConversion;
            }

            aggSym.SetDeprecated(
                attributes.IsDeprecated,
                attributes.IsDeprecatedError,
                attributes.DeprecatedString);

            if (attributes.HasCLSattribute)
            {
                aggSym.HasCLSAttribute = true;
                aggSym.IsCLS = attributes.IsCLS;
            }

            if (aggSym.IsAttribute)
            {
                aggSym.ConditionalSymbolNameList = attributes.ConditionalSymbols;

                // don't overwrite default attributeClass for security attributes
                if (attributes.AttributeKind != 0)
                {
                    aggSym.AttributeClass = attributes.AttributeKind;
                    aggSym.IsMultipleAttribute = attributes.AllowMultiple;
                }
                else
                {
                    AGGSYM baseSym = aggSym.BaseClassSym.GetAggregate();
                    Compiler.EnsureState(baseSym, AggStateEnum.DefinedMembers);
                    aggSym.AttributeClass = baseSym.AttributeClass;
                    aggSym.IsMultipleAttribute = baseSym.IsMultipleAttribute;
                }
            }

            DebugUtil.Assert(aggSym.AggState == AggStateEnum.DefiningMembers);
            aggSym.AggState = AggStateEnum.DefinedMembers;
        }

        //------------------------------------------------------------
        // IMPORTER.ImportMethodProps (1)
        //------------------------------------------------------------
        //void ImportMethodProps(METHSYM * sym);

        //------------------------------------------------------------
        // IMPORTER.ImportMethodProps (2)
        //
        /// <summary>
        /// <para>In sscli, this medhod reads a signeture of MethodDef table and
        /// set it to fields of methodSym.</para>
        /// <para>In this project, this method calls ImportMethodPropsWorker
        /// to set type arguments and fields of methodSym
        /// by data of methodInfo.
        /// And set methodSym.HasParamsDefined.</para>
        /// <para></para>
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="parentAggSym"></param>
        /// <param name="methodInfo"></param>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void ImportMethodProps(
            INFILESYM infileSym,
            AGGSYM parentAggSym,
            MethodInfo methodInfo,
            METHSYM methodSym)
        {

            if (methodSym == null || methodSym.HasParamsDefined)
            {
                return;
            }

            //AGGSYM parentAggSym = methodSym.ClassSym;

            //MODULESYM * scope = parentAggSym.GetModule();
            //DebugUtil.Assert(scope.GetMetaImport(compiler()));
            //IMetaDataImport * metaimport = scope.GetMetaImport(compiler());
            //PCCOR_SIGNATURE signature;
            //ULONG cbSignature;
            //mdToken tokenMethod = methodSym.tokenImport;

            //// Get method properties.
            //CheckHR(metaimport.GetMethodProps(
            //    tokenMethod,                  // The method for which to get props.
            //    null,                         // Put method's class here.
            //    null, 0, null,   				// Method name
            //    null,                         // Put flags here.
            //    & signature, & cbSignature,   // Method signature
            //    null,                         // codeRVA
            //    null), scope);                // Impl. Flags

            ImportMethodPropsWorker(infileSym, parentAggSym, methodInfo, methodSym);
            methodSym.HasParamsDefined = true;
        }

        //------------------------------------------------------------
        // IMPORTER.GetTypeRefAssemblyName
        //
        /// <summary>
        /// <para>(sscli)
        /// Given a typeref, get the name of the assembly (or module)
        /// that the typeref refers to. Used for better error reporting.</para>
        /// <para>Do not use this method.
        /// We import types by System.Reflection.Assembly instances.
        /// So, when we have import errors, we can save assembly names.</para>
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="token"></param>
        /// <param name="assemblyName"></param>
        //------------------------------------------------------------
        internal void GetTypeRefAssemblyName(
            MODULESYM mod,
            uint token,
            out string assemblyName)
        {
            throw new NotImplementedException("IMPORTER.GetTypeRefAssemblyName");
        }

        //#ifdef DEBUG
        //    void DeclareAllTypes(PPARENTSYM parent);
        //#endif //DEBUG

        //------------------------------------------------------------
        // IMPORTER.GetTypeSymFromCache
        //
        /// <summary>
        /// <para>Look for a SYM in BSYMMGR.InfoToSymbolTable.</para>
        /// <para>(GetTypeFromCache in sscli.)</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetTypeSymFromCache(
            System.Type type,
            TypeArray typeArgs,
            out TYPESYM typeSym)
        {
            typeSym = null;
            SYM sym = this.GetSymFromCache(type);

            if (sym == null &&
                type.IsGenericType &&
                !type.IsGenericTypeDefinition)
            {
                Type typeDef = type.GetGenericTypeDefinition();
                sym = this.GetSymFromCache(typeDef);
            }

            if (sym == null)
            {
                return false;
            }

            if (sym.IsAGGSYM)
            {
                typeSym = (sym as AGGSYM).GetThisType();
            }
            else if (sym.IsTYPESYM)
            {
                typeSym = sym as TYPESYM;
            }
            else
            {
                //DebugUtil.Assert(false, "Why are we here?");
                return false;
            }

            if (typeArgs == null || typeArgs.Count == 0 || typeSym == null)
            {
                return true;
            }

            //--------------------------------------------------
            // If AGGTYPESYM, instantiate it.
            //--------------------------------------------------
            if (typeSym.IsAGGTYPESYM)
            {
                AGGTYPESYM aggTypeSym = typeSym as AGGTYPESYM;
                if (aggTypeSym.AllTypeArguments == typeArgs)
                {
                    return true;
                }
                if (aggTypeSym.AllTypeArguments.Count == 0)
                {
                    typeSym = aggTypeSym;
                    return true;
                }
                if (aggTypeSym.AllTypeArguments.Count != typeArgs.Count)
                {
                    DebugUtil.Assert(false, "Why are we here?");
                    typeSym = null;
                    return false;
                }
                typeSym = Compiler.MainSymbolManager.GetInstAgg(
                    aggTypeSym.GetAggregate(),
                    typeArgs);
                return true;
            }

            //--------------------------------------------------
            // ERRORSYM
            //--------------------------------------------------
            if (typeSym.IsERRORSYM)
            {
                ERRORSYM errorSym = typeSym as ERRORSYM;
                DebugUtil.Assert(
                    errorSym.ParentSym != null &&
                    !String.IsNullOrEmpty(errorSym.Name) &&
                    errorSym.TypeArguments.Count > 0);

                // Type args work differently for ERRORSYMs.
                // The first type arg is the parent type.
                TYPESYM parentTypeSym = null;
                TypeArray actualTypeArgs = null;

                if (typeArgs != null && typeArgs.Count > 0)
                {
                    // The parent type is encoded as the first type argument.
                    // void * indicates no parent type.
                    if (!(typeArgs[0].IsPTRSYM))
                    {
                        parentTypeSym = typeArgs[0];
                    }
                    else
                    {
                        DebugUtil.Assert((typeArgs[0] as PTRSYM).BaseTypeSym.IsVOIDSYM);
                        DebugUtil.Assert(parentTypeSym == null);
                    }

                    if (typeArgs.Count > 1)
                    {
                        // Get the the real type args.
                        TypeArray arrayTemp = null;
                        if (typeArgs.GetSubArray(1, typeArgs.Count - 1, out arrayTemp, Compiler))
                        {
                            actualTypeArgs = Compiler.MainSymbolManager.AllocParams(arrayTemp);
                        }
                    }
                }

                if (parentTypeSym != null)
                {
                    DebugUtil.Assert(
                        errorSym.ParentSym.IsTYPESYM ||
                        errorSym.ParentSym == Compiler.MainSymbolManager.GetRootNsAid(Kaid.Global));

                    typeSym = Compiler.MainSymbolManager.GetErrorType(
                        parentTypeSym,
                        errorSym.Name,
                        actualTypeArgs);
                }
                else
                {
                    NSAIDSYM naSym = null;
                    if (errorSym.ParentSym.IsNSAIDSYM)
                    {
                        naSym = errorSym.ParentSym as NSAIDSYM;
                    }
                    typeSym = Compiler.MainSymbolManager.GetErrorType(
                        naSym,
                        errorSym.Name,
                        actualTypeArgs);
                }
                return true;
            }

            //--------------------------------------------------
            // Otherwise,
            //--------------------------------------------------
            DebugUtil.Assert(false, "Why are we here?");
            if (typeArgs != null && typeArgs.Count > 0)
            {
                typeSym = null;
                return false;
            }
            return true;
        }
        //------------------------------------------------------------
        // IMPORTER::GetTypeFromCache (sscli)
        //
        // Looks for a value in the (module, tok) -> sym cache.
        // For the EE, if m_fIgnoreCache is clear, we're trying to resolve a previously unresolved type,
        // so always return false.
        //------------------------------------------------------------
        //bool GetTypeFromCache(ImportScope & scope, mdToken tok, TypeArray * typeArgs, TYPESYM ** ptype)
        //{
        //    SYM * symT;
        //
        //    if (!GetSymFromCache(scope, tok, &symT)) {
        //        *ptype = NULL;
        //        return false;
        //    }
        //
        //    if (!symT)
        //        *ptype = NULL;
        //    else if (symT->IsAGGSYM)
        //        *ptype = symT->AsAGGSYM->getThisType();
        //    else if (symT->IsTYPESYM)
        //        *ptype = symT->AsTYPESYM;
        //    else {
        //        VSFAIL("Why are we here?");
        //        *ptype = NULL;
        //        return false;
        //    }
        //
        //    if (!typeArgs || !*ptype)
        //        return true;
        //
        //    if ((*ptype)->IsAGGTYPESYM) {
        //        AGGTYPESYM * ats = (*ptype)->AsAGGTYPESYM;
        //        if (typeArgs == ats->typeArgsAll)
        //            return true;
        //        if (typeArgs->size != ats->typeArgsAll->size) {
        //            VSFAIL("Why are we here?");
        //            *ptype = NULL;
        //            return false;
        //        }
        //        *ptype = compiler()->getBSymmgr().GetInstAgg(ats->getAggregate(), typeArgs);
        //        return true;
        //    }
        //
        //    if ((*ptype)->IsERRORSYM) {
        //        ERRORSYM * err = (*ptype)->AsERRORSYM;
        //        ASSERT(err->parent && err->nameText && err->typeArgs);
        //
        //        // Type args work differently for ERRORSYMs. The first type arg is the parent type.
        //        TYPESYM * typePar = NULL;
        //        TypeArray * typeArgsActual = NULL;
        //
        //        if (typeArgs && typeArgs->size > 0) {
        //            // The parent type is encoded as the first type argument. void * indicates no parent type.
        //            if (!typeArgs->Item(0)->IsPTRSYM) {
        //                typePar = typeArgs->Item(0);
        //            }
        //            else {
        //                ASSERT(typeArgs->Item(0)->AsPTRSYM->baseType()->IsVOIDSYM);
        //                ASSERT(!typePar);
        //            }
        //            if (typeArgs->size > 1) {
        //                // Get the the real type args.
        //                typeArgsActual = compiler()->getBSymmgr().AllocParams(typeArgs->size - 1, typeArgs->ItemPtr(1));
        //            }
        //        }
        //
        //        if (typePar) {
        //            ASSERT(err->parent->IsTYPESYM || err->parent == compiler()->getBSymmgr().GetRootNsAid(kaidGlobal));
        //            *ptype = compiler()->getBSymmgr().GetErrorType(typePar, err->nameText, typeArgsActual);
        //        }
        //        else {
        //            NSAIDSYM * nsa = NULL;
        //            if (err->parent->IsNSAIDSYM)
        //                nsa = err->parent->AsNSAIDSYM;
        //            *ptype = compiler()->getBSymmgr().GetErrorType(nsa, err->nameText, typeArgsActual);
        //        }
        //        return true;
        //    }
        //
        //    VSFAIL("Why are we here?");
        //    if (typeArgs->size) {
        //        *ptype = NULL;
        //        return false;
        //    }
        //    return true;
        //}

        //------------------------------------------------------------
        // IMPORTER.GetAggFromCache
        //
        /// <summary>
        /// Get an AGGSYM from the cache. Don't load any types.
        /// </summary>
        //------------------------------------------------------------
        internal AGGSYM GetAggFromCache(Type type)
        {
            return Compiler.MainSymbolManager.InfoToSymbolTable.GetSymFromInfo(type) as AGGSYM;
        }
        //------------------------------------------------------------
        // IMPORTER::GetAggFromCache
        //------------------------------------------------------------
        //bool IMPORTER::GetAggFromCache(ImportScope & scope, mdTypeDef tok, AGGSYM ** pagg)
        //{
        //    ASSERT(scope.GetMetaImport());
        //    ASSERT(TypeFromToken(tok) == mdtTypeDef);
        //
        //    if (tok == mdTypeDefNil) {
        //        *pagg = NULL;
        //        return false;
        //    }
        //
        //    SYM * symT;
        //    if (!GetSymFromCache(scope, tok, &symT)) {
        //        *pagg = NULL;
        //        return false;
        //    }
        //
        //    ASSERT(!symT || !symT->isAGGTYPESYM());
        //
        //    *pagg = symT && symT->isAGGSYM() ? symT->asAGGSYM() : NULL;
        //    return true;
        //}

        //------------------------------------------------------------
        // IMPORTER.GetSymFromCache
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetSymFromCache(System.Type type)
        {
            return Compiler.MainSymbolManager.InfoToSymbolTable.GetSymFromInfo(type);
        }
        //------------------------------------------------------------
        // IMPORTER::GetSymFromCache (sscli)
        //------------------------------------------------------------
        //bool IMPORTER::GetSymFromCache(ImportScope & scope, mdToken tok, SYM ** psym)
        //{
        //    TokenToSymTable * ptst = compiler()->getBSymmgr().GetTokenToSymTable();
        //
        //    if (!ptst->GetSymFromToken(scope.GetModule(), tok, psym))
        //        return false;
        //    ASSERT(!*psym || !(*psym)->isAGGTYPESYM());
        //
        //    return true;
        //}

        //------------------------------------------------------------
        // IMPORTER.SetSymInCache
        //
        /// <summary>
        /// Store AGGSYMs, not AGGTYPESYMs.
        /// This allows us to defer creation of the instance AGGTYPESYM until first use.
        /// This saves a ton of memory because many imported types are never used.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal void SetSymInCache(System.Type type, SYM sym)
        {
            //DebugUtil.Assert(sym == null || !sym.IsAGGTYPESYM);
            Compiler.MainSymbolManager.InfoToSymbolTable.SetSymForInfo(type, sym);
        }
        //------------------------------------------------------------
        // IMPORTER::SetSymInCache (sscli)
        //------------------------------------------------------------
        //void IMPORTER::SetSymInCache(ImportScope & scope, mdToken tok, SYM * sym)
        //{
        //    ASSERT(!sym || !sym->IsAGGTYPESYM);
        //    compiler()->getBSymmgr().GetTokenToSymTable()->SetSymForToken(scope.GetModule(), tok, sym);
        //}

        //------------------------------------------------------------
        // IMPORTER.OpenMetadataFile (IMPORTER::OpenAssembly in sscli)
        //
        /// <summary>
        /// <para>Opens an Assembly and adds all metadata files,
        /// but does not add an AssemblyRef yet.
        /// Returns true iff we imported the Assembly
        /// Returns false if we had to 'suck-in' the file</para>
        /// <para>If the file of infile is an assembly, create CAssembly instance.</para>
        /// <para>If the file is not assembly, show error message.</para>
        /// </summary>
        /// <param name="infile"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool OpenMetadataFile(INFILESYM infile)
        {
            //ASSERT(!infile.assemimport && IsNilToken(infile.idLocalAssembly));

            //uint tkAssem;
            //int cscope = 0;

            //--------------------------------------------------
            // Create CAssembly instance and set it to INFILESYM.CAssembly.
            // Get its custom attributes
            //--------------------------------------------------
            CheckResult(
                Compiler.Linker.ImportFileEx2(infile, null),
                CSCERRID.FTL_MetadataCantOpenFile,
                infile);
            //infile.CScope = cscope;

            //--------------------------------------------------
            // if a module has been loaded in infile, not an assembly.
            //--------------------------------------------------
            if (infile.IsModuleLoaded)
            {
                //ASSERT(infile->cscope == 1);
                //if (!infile->isAddedModule) {
                //    // This is not an assembly and it wasn't added via addmodule
                //    compiler()->Error( NULL, ERR_ImportNonAssembly, infile->name->text);
                //    infile->setBogus(true);
                //    infile->isAddedModule = true;
                //    return false;
                //}

                // importing into the current assembly
                DebugUtil.Assert(infile.GetAssemblyID() == Kaid.ThisAssembly);
                DebugUtil.Assert(infile.InAlias(Kaid.ThisAssembly));
                DebugUtil.Assert(infile.InAlias(Kaid.Global));

                infile.LocalAssemblyID = Cor.mdTokenNil;
            }
            //--------------------------------------------------
            // if an assembly has been loaded in infile, not a module.
            //--------------------------------------------------
            else if (infile.IsAssemblyLoaded)
            {
                //if (infile->isAddedModule)
                //{
                //    // This is an assembly and it was added via addmodule!
                //    compiler()->Error(NULL, ERR_AddModuleAssembly, infile->name->text);
                //    infile->setBogus(true);
                //    return false;
                //}

                // assign this imported assembly a new assembly index
                // which we'll use until we begin emitting

                DebugUtil.Assert(infile.GetAssemblyID() != Kaid.ThisAssembly);

                //CheckHR(infile->assemimport->GetAssemblyFromScope(&tkAssem), infile);

                //if (compiler()->cmdHost) {
                //    // Walk all of the files in a possibly multi-file assembly
                //    // and report them to the host
                //
                //    HCORENUM enumFiles;
                //    mdFile filedefs[16];
                //    ULONG cFiledefs, iFiledef;
                //    WCHAR *FileName = NULL, *filepart = NULL;
                //    DWORD len, cchName;
                //    HRESULT hr;
                //
                //    cchName = (DWORD)wcslen(infile->name->text) + MAX_PATH;
                //    FileName = STACK_ALLOC(WCHAR, cchName);
                //    StringCchCopyW (FileName, cchName, infile->name->text);
                //    filepart = wcsrchr(FileName, L'\\');
                //    if (filepart)
                //        filepart++;
                //    else
                //        filepart = FileName;
                //
                //    len = cchName - (DWORD)(filepart - FileName);
                //    
                //    // Enumeration all the Files in this assembly.
                //    enumFiles= 0;
                //    do {
                //
                //        // Get next batch of files.
                //        hr = infile->assemimport->EnumFiles(&enumFiles, filedefs, lengthof(filedefs), &cFiledefs);
                //        if (FAILED(hr))
                //            break;
                //
                //        // Process each file.
                //        for (iFiledef = 0; iFiledef < cFiledefs && SUCCEEDED(hr); ++iFiledef) {
                //            hr = infile->assemimport->GetFileProps(
                //                filedefs[iFiledef], filepart, len, &cchName, NULL, NULL, NULL);
                //            if (FAILED(hr))
                //                continue;
                //            compiler()->NotifyHostOfMetadataFile(FileName);
                //
                //        }
                //    } while (cFiledefs > 0 && SUCCEEDED(hr));
                //    
                //    infile->assemimport->CloseEnum(enumFiles);
                //}

                CAssemblyEx asm = infile.MetadataFile as CAssemblyEx;
                if (asm.Assembly != null)
                {
                    Compiler.MainSymbolManager.AddToAssemblyToInfileSymDictionary(asm.Assembly, infile);
                }
            }
            else
            {
                // If errors occured, Compiler.Linker.ImportFile showed messages.
                return false;
            }
            return true;
        }
        //------------------------------------------------------------
        // IMPORTER::OpenAssembly (sscli)
        //------------------------------------------------------------
        //bool IMPORTER::OpenAssembly(PINFILESYM infile)
        //{
        //    ASSERT(!infile->assemimport && IsNilToken(infile->idLocalAssembly));
        //
        //    mdAssembly tkAssem;
        //
        //    DWORD cscope;
        //
        //    //--------------------------------------------------
        //    // Search a RegMeta instance of the specified assembly.
        //    // If found, return it.
        //    // Otherwise, create a new RegMeta instance and store the
        //    // information of assembly.
        //    // In each case, set the RegMeta instance to infile.MetadataAssemblyImport.
        //    //--------------------------------------------------
        //    CheckHR(FTL_MetadataCantOpenFile,
        //        compiler()->linker->ImportFile(
        //            infile->name->text, NULL, FALSE, &infile->mdImpFile, &infile->assemimport, &cscope),
        //        infile);
        //
        //    infile->cscope = cscope;
        //
        //    //--------------------------------------------------
        //    // If the file specified for infile is not an assembly,
        //    //--------------------------------------------------
        //    if (infile->assemimport == NULL)
        //    {
        //        ASSERT(infile->cscope == 1);
        //        if (!infile->isAddedModule) {
        //            // This is not an assembly and it wasn't added via addmodule
        //            compiler()->Error( NULL, ERR_ImportNonAssembly, infile->name->text);
        //            infile->setBogus(true);
        //            infile->isAddedModule = true;
        //            return false;
        //        }
        //
        //        // importing into the current assembly
        //        ASSERT(infile->GetAssemblyID() == kaidThisAssembly);
        //        ASSERT(infile->InAlias(kaidThisAssembly));
        //        ASSERT(infile->InAlias(kaidGlobal));
        //
        //        infile->idLocalAssembly = mdTokenNil;
        //    }
        //    //--------------------------------------------------
        //    // アセンブリの場合、
        //    // アセンブリを構成するファイル名を compilerHost に伝える。
        //    //--------------------------------------------------
        //    else
        //    {
        //        // 既に登録されている場合はエラーとして処理する。
        //        if (infile->isAddedModule)
        //        {
        //            // This is an assembly and it was added via addmodule!
        //            compiler()->Error(NULL, ERR_AddModuleAssembly, infile->name->text);
        //            infile->setBogus(true);
        //            return false;
        //        }
        //
        //        //
        //        // assign this imported assembly a new assembly index
        //        // which we'll use until we begin emitting
        //        //
        //        ASSERT(infile->GetAssemblyID() != kaidThisAssembly);
        //
        //        CheckHR(infile->assemimport->GetAssemblyFromScope(&tkAssem), infile);
        //
        //        // アセンブリを構成するファイル名を cmpilerHost に伝える。
        //        if (compiler()->cmdHost) {
        //            // Walk all of the files in a possibly multi-file assembly
        //            // and report them to the host
        //
        //            HCORENUM enumFiles;
        //            mdFile filedefs[16];
        //            ULONG cFiledefs, iFiledef;
        //            WCHAR *FileName = NULL, *filepart = NULL;
        //            DWORD len, cchName;
        //            HRESULT hr;
        //
        //            cchName = (DWORD)wcslen(infile->name->text) + MAX_PATH;
        //            FileName = STACK_ALLOC(WCHAR, cchName);
        //            StringCchCopyW (FileName, cchName, infile->name->text);
        //            filepart = wcsrchr(FileName, L'\\');
        //            if (filepart)
        //                filepart++;
        //            else
        //                filepart = FileName;
        //
        //            len = cchName - (DWORD)(filepart - FileName);
        //    
        //            // Enumeration all the Files in this assembly.
        //            enumFiles= 0;
        //            do {
        //
        //                // Get next batch of files.
        //                hr = infile->assemimport->EnumFiles(&enumFiles, filedefs, lengthof(filedefs), &cFiledefs);
        //                if (FAILED(hr))
        //                    break;
        //
        //                // Process each file.
        //                for (iFiledef = 0; iFiledef < cFiledefs && SUCCEEDED(hr); ++iFiledef) {
        //                    // File テーブルからファイル名を取得し、compilerHost に伝える。
        //                    hr = infile->assemimport->GetFileProps(
        //                        filedefs[iFiledef], filepart, len, &cchName, NULL, NULL, NULL);
        //                    if (FAILED(hr))
        //                        continue;
        //                    compiler()->NotifyHostOfMetadataFile(FileName);
        //
        //                }
        //            } while (cFiledefs > 0 && SUCCEEDED(hr));
        //    
        //            infile->assemimport->CloseEnum(enumFiles);
        //        }
        //    }
        //
        //    return true;
        //}

        //------------------------------------------------------------
        // IMPORTER.ImportOneType
        //
        /// <summary>
        /// <para>Import one type. Create a symbol for the type in the symbol table.
        /// The symbol is created "undeclared" - no information about it is known except its:
        /// <list type="bullet">
        /// <item>fully qualified name</item>
        /// <item>type variables (but not constraints)</item>
        /// <item>accessibility</item>
        /// <item>whether it is a value type (struct or enum) / interface / or other</item>
        /// </list></para>
        /// <para>This expressly does not determine whether it's a delegate or attribute.</para>
        /// <para>Returns NULL if a fatal (for this input file) error occured.</para>
        /// </summary>
        /// <remarks>
        /// <para>(sscli)
        /// REVIEW ShonK: is determining whether the type is a value type strictly necessary?
        /// We used to do when it was cheap, but now that it requires checking the base class
        /// we should see if we could get rid of returning that information.
        /// I'm not sure why we really need to know this information
        /// before the type is fully imported.</para>
        /// </remarks>
        /// <param name="inFileSym"></param>
        /// <param name="typeToImport"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM ImportOneType(INFILESYM inFileSym, Type typeToImport)
        {
#if DEBUG
            //if (typeToImport.FullName == "System.Array")
            if (typeToImport.FullName == "System.Collections.Generic.List`1")
            {
                ;
            }
#endif
            DebugUtil.Assert(inFileSym != null && typeToImport != null);

            if (typeToImport.IsGenericType == true &&
                typeToImport.IsGenericTypeDefinition == false)
            {
                typeToImport = typeToImport.GetGenericTypeDefinition();
            }

            //--------------------------------------------------------
            // Search in the cache.
            //--------------------------------------------------------
            SYM sym = this.GetSymFromCache(typeToImport);
            if (sym != null)
            {
                return sym;
            }

            //--------------------------------------------------------
            // Generic parameter types are imported with its declaring types or methods.
            // So, it should be found in the cache and
            // typeToImport should not be a generic parameter.
            //--------------------------------------------------------
            if (typeToImport.IsGenericParameter)
            {
                DebugUtil.Assert(false);
            }

            //--------------------------------------------------------
            // Aggregate Type
            //--------------------------------------------------------
            AGGSYM aggSym = null;
            string aggFullName = null;
            BAGSYM parentBagSym = null;
            DECLSYM parentDeclSym = null;
            int aid = Kaid.Nil;
            ACCESS access = AccessFromTypeFlags(typeToImport.Attributes, inFileSym);
            bool isPrivateMetadata = false;

            switch (access)
            {
                case ACCESS.UNKNOWN:
                    DebugUtil.Assert(false);
                    break;

                case ACCESS.PRIVATE:
                    isPrivateMetadata = (inFileSym.GetAssemblyID() != Kaid.ThisAssembly);
                    break;

                case ACCESS.INTERNAL:
                    isPrivateMetadata = (
                        inFileSym.GetAssemblyID() != Kaid.ThisAssembly &&
                        !inFileSym.InternalsVisibleTo(Kaid.ThisAssembly)
                        );
                    break;

                case ACCESS.PROTECTED:
                case ACCESS.INTERNALPROTECTED:
                case ACCESS.PUBLIC:
                    DebugUtil.Assert(!isPrivateMetadata);
                    break;
            }

            //--------------------------------------------------------
            // Nested type
            // First, import the outer types.
            //--------------------------------------------------------
            if (typeToImport.IsNested)
            {
                parentBagSym = this.ImportOneType(inFileSym, typeToImport.DeclaringType) as AGGSYM;
                if (parentBagSym == null)
                {
                    goto LDONE;
                }

                AGGSYM parentAggSym = parentBagSym as AGGSYM;
                DebugUtil.Assert(parentAggSym != null);
                parentDeclSym = parentAggSym.DeclOnly();
                isPrivateMetadata |= parentAggSym.IsPrivateMetadata;
                //aid = parentAggSym.GetModuleID();
                aid = parentAggSym.GetAssemblyID();
                aggFullName = ReflectionUtil.GetFullNameOfType(typeToImport);
            }
            //--------------------------------------------------------
            // Not nested type.
            //--------------------------------------------------------
            else
            {
                aggFullName = ReflectionUtil.GetFullNameOfType(typeToImport);

                //SYM* symPar = ResolveParentScope(scope, mdModuleNil, &pszAgg);
                //NSAIDSYM parentNsAidSym = ResolveNamespaceOfClassName(ref aggFullName, inFileSym.GetAssemblyID());
                NSAIDSYM parentNsAidSym = ResolveNamespace(typeToImport.Namespace, inFileSym.GetAssemblyID());

                // If the first character of aggFullName is '.', parentNsAidSym is null.
                if (parentNsAidSym == null)
                {
                    goto LDONE;
                }
                parentBagSym = parentNsAidSym.NamespaceSym;
                aid = parentNsAidSym.GetAssemblyID();
                parentDeclSym = GetNsDecl(parentBagSym as NSSYM, inFileSym);
            }

            //--------------------------------------------------------
            // Count the type variables for the agg.
            //
            // If typeToImport.IsGenericTypeDefinition == false, it needs no type argument.
            // Otherwise, count the generic arguments with IsGenericParameter == true.
            //--------------------------------------------------------
            // This method does not treat constructed types.
            // If the argument typeToImport is generic, it is a generic definition.

            int totalTypeVariableCount = typeToImport.IsGenericTypeDefinition ?
                typeToImport.GetGenericArguments().Length : 0;

            int outerTypeVariableCount = parentBagSym.IsAGGSYM ?
                (parentBagSym as AGGSYM).AllTypeVariables.Count : 0;

            //NAME * nameAgg;
            //NAME * nameWithArity;
            //int cvarFromName; // Incremental arity in name.
            //
            //// Strip any arity and place name of type in name table.
            //cvarFromName = StripArityFromName(pszAgg, &nameAgg, &nameWithArity);
            //ASSERT(!cvarFromName == !nameWithArity);
            //
            //// If the name contained the arity, it must match the imported metadata.
            //if (cvarFromName != 0 && cvarFromName + cvarOuter != cvar) {
            //    nameAgg = nameWithArity;
            //    cvarFromName = 0;
            //}

            string aggName = typeToImport.Name;
            string aggNameWithArity = null;
            string aggNameWithoutArity = null;
            int arityFromName;
            //int startOfArity;

            // Strip any arity and place name of type in name table.
            //arityFromName = StripArityFromName(typeToImport.Name, out aggName, out aggNameWithArity);
            // If typeToImport.Name has no arity, aggName is typeToImport.Name and aggNameWithArity is null.

            //arityFromName = ComputeArityFromName(aggName, out startOfArity);
            arityFromName = StripArityFromName(aggName, ref aggNameWithoutArity, ref aggNameWithArity);

            // If the name contained the arity, it must match the imported metadata.
            if (arityFromName != 0 && (arityFromName + outerTypeVariableCount != totalTypeVariableCount))
            {
                int tvCount = totalTypeVariableCount - outerTypeVariableCount;
                if (tvCount == 0)
                {
                    // If not match, the part which seems to be arity part is not artiy part,
                    // that is, the name has no arity.
                    aggNameWithoutArity = aggName;
                    aggNameWithArity = null;
                    arityFromName = 0;
                }
                else if (tvCount > 0)
                {
                    arityFromName = tvCount;
                }
                else
                {
                    DebugUtil.VsFail("IMPORTER.ImportOneType: arityFromName");
                }
            }

            //--------------------------------------------------------
            // See if we've already imported it.
            // This can happen if we encountered a nested type before this one.
            //--------------------------------------------------------
            aggSym = FindAggName(aggNameWithoutArity, parentBagSym, aid, typeToImport);
            if (aggSym != null)
            {
                goto LDONE;
            }

            if (inFileSym.GetAssemblyID() == Kaid.ThisAssembly)
            {
                //----------------------------------------------------
                // Check for any conflicting classes or namespaces in this assembly
                //----------------------------------------------------
                BAGSYM conflictBagSym = null;
                SYMBMASK mask = (arityFromName > 0 ? SYMBMASK.AGGSYM : (SYMBMASK.AGGSYM | SYMBMASK.NSSYM));

                BAGSYM bagSym = this.compiler.MainSymbolManager.LookupGlobalSymCore(
                    aggNameWithoutArity,
                    parentBagSym,
                    mask) as BAGSYM;

                for (; bagSym != null;
                    bagSym = BSYMMGR.LookupNextSym(bagSym, parentBagSym, mask) as BAGSYM)
                {
                    if (!bagSym.InAlias(Kaid.ThisAssembly)) continue;
                    if (bagSym.IsNSSYM)
                    {
                        DebugUtil.Assert(arityFromName == 0);
                        DebugUtil.Assert(conflictBagSym == null);
                        conflictBagSym = bagSym;
                        // Keep looking in case there's a class
                    }
                    else
                    {
                        AGGSYM tempAggSym = bagSym as AGGSYM;
                        DebugUtil.Assert(tempAggSym != null);
                        DebugUtil.Assert(!tempAggSym.IsArityInName || tempAggSym.TypeVariables.Count > 0);
                        if (arityFromName == (tempAggSym.IsArityInName ? tempAggSym.TypeVariables.Count : 0))
                        {
                            conflictBagSym = bagSym;
                            break;
                        }
                    }
                }

                if (conflictBagSym != null)
                {
                    if (parentBagSym.IsAGGSYM)
                    {
                        compiler.Error(
                            CSCERRID.ERR_DuplicateNameInClass,
                            new ErrArg(aggNameWithoutArity),
                            new ErrArg(parentBagSym.Name));
                    }
                    else
                    {
                        compiler.Error(
                            CSCERRID.ERR_DuplicateNameInNS,
                            new ErrArg(aggNameWithoutArity),
                            new ErrArg(parentBagSym.Name));
                    }
                    goto LDONE;
                }
            }

            //--------------------------------------------------------
            // Create the symbol for the new type.
            //--------------------------------------------------------
            aggSym = compiler.MainSymbolManager.CreateAgg(aggNameWithoutArity, parentDeclSym);
            compiler.MainSymbolManager.CreateAggDecl(aggSym, parentDeclSym);

            // Remember the token.
            DebugUtil.Assert(aggSym.AggState == AggStateEnum.None);
            //aggSym.ImportedToken = typeToImport.MetadataToken;
            //aggSym.ModuleSym = moduleSym;
            aggSym.Type = typeToImport;
            aggSym.Access = access;
            aggSym.IsPrivateMetadata = isPrivateMetadata;

            // Set what we can on the symbol structure from the flags.
            if (typeToImport.IsInterface)
            {
                // interface
                aggSym.AggKind = AggKindEnum.Interface;
                aggSym.IsAbstract = true;
            }
            // in sscli, ImportOneType_SetAggKind method
            // Check to see if we extend System.ValueType, and are thus a value type.
            else if (typeToImport.IsEnum)
            {
                aggSym.AggKind = AggKindEnum.Enum;
                aggSym.IsSealed = true;
            }
            else if (typeToImport.IsValueType)
            {
                aggSym.AggKind = AggKindEnum.Struct;
                aggSym.IsSealed = true;
            }
            else
            {
                Type baseType = typeToImport.BaseType;
                string baseTypeName = null;
                // System.Object has no base type.
                if (baseType != null)
                {
                    baseTypeName = ReflectionUtil.GetFullNameOfType(baseType);
                }
                if (!String.IsNullOrEmpty(baseTypeName))
                {
                    string delegateFullName = BSYMMGR.GetPredefinedFullName(PREDEFTYPE.DELEGATE);
                    string muldelFullName = BSYMMGR.GetPredefinedFullName(PREDEFTYPE.MULTIDEL);

                    if (muldelFullName.CompareTo(baseTypeName) == 0 ||
                        (delegateFullName.CompareTo(baseTypeName) == 0 && muldelFullName.CompareTo(aggFullName) != 0))
                    {
                        aggSym.AggKind = AggKindEnum.Delegate;
                        aggSym.IsSealed = typeToImport.IsSealed;
                    }
                    else
                    {
                        aggSym.AggKind = AggKindEnum.Class;
                        aggSym.IsAbstract = typeToImport.IsAbstract;
                        aggSym.IsSealed = typeToImport.IsSealed;
                    }
                }
                else
                {
                    aggSym.AggKind = AggKindEnum.Class;
                    aggSym.IsAbstract = typeToImport.IsAbstract;
                    aggSym.IsSealed = typeToImport.IsSealed;
                }

            }

            aggSym.IsComImport = typeToImport.IsImport;
            aggSym.IsArityInName = (arityFromName > 0);

            // If the inner type has fewer arguments than the outer type, it's bogus.
            if (totalTypeVariableCount < outerTypeVariableCount)
            {
                goto LBOGUS;
            }

            //--------------------------------------------------------
            // Now set the type variables, but not the bounds.
            //--------------------------------------------------------
            if (totalTypeVariableCount == 0)
            {
                aggSym.TypeVariables = BSYMMGR.EmptyTypeArray;
                aggSym.AllTypeVariables = BSYMMGR.EmptyTypeArray;
            }
            else
            {
                if (aggSym.IsStruct)
                {
                    aggSym.IsManagedStruct = true;
                }

                Type[] genArgs = typeToImport.GetGenericArguments();
                List<TYPESYM> typeVarArray = new List<TYPESYM>();
                List<TYPESYM> allTypeVarArray = new List<TYPESYM>();
                AGGSYM parentAggSym = parentBagSym as AGGSYM;
                DebugUtil.Assert(outerTypeVariableCount == 0 || parentAggSym != null);

                // Process each type variable.
                // In sscli, ImportOneType_ProcessTypeVariable.
                for (int i = 0; i < totalTypeVariableCount; ++i)
                {
                    if (i < outerTypeVariableCount)
                    {
                        allTypeVarArray.Add(parentAggSym.AllTypeVariables[i] as TYVARSYM);
                    }
                    else
                    {
                        TYVARSYM typeVarSym = compiler.MainSymbolManager.CreateTyVar(genArgs[i].Name, aggSym);
                        typeVarSym.SetSystemType(genArgs[i], false);
                        typeVarSym.Access = ACCESS.PRIVATE;
                        typeVarSym.TotalIndex = i;
                        typeVarSym.Index = i - outerTypeVariableCount;
                        typeVarSym.ParseTreeNode = null;
                        allTypeVarArray.Add(typeVarSym);
                        typeVarArray.Add(typeVarSym);

                        SetSymInCache(genArgs[i], typeVarSym);
                    }
                }

                if (outerTypeVariableCount > 0)
                {
                    aggSym.AllTypeVariables = compiler.MainSymbolManager.AllocParams(allTypeVarArray);
                    aggSym.TypeVariables = compiler.MainSymbolManager.AllocParams(typeVarArray);
                }
                else
                {
                    aggSym.AllTypeVariables = compiler.MainSymbolManager.AllocParams(allTypeVarArray);
                    aggSym.TypeVariables = aggSym.AllTypeVariables;
                }
            }

            aggSym.AggState = AggStateEnum.Declared;
            goto LDONE;

        LBOGUS:
            // Set it bogus. Inherit outer type variables and assume no new ones.
            aggSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            if (outerTypeVariableCount > 0)
            {
                AGGSYM tempAggSym = parentBagSym as AGGSYM;
                aggSym.AllTypeVariables = (tempAggSym != null ? tempAggSym.AllTypeVariables : BSYMMGR.EmptyTypeArray);
            }
            else
            {
                aggSym.AllTypeVariables = aggSym.TypeVariables;
            }
            aggSym.SetBogus(true);
            aggSym.AggState = AggStateEnum.Declared;

        LDONE:
            SetSymInCache(typeToImport, aggSym);
            return aggSym;
        }

        //------------------------------------------------------------
        // IMPORTER.
        //------------------------------------------------------------
        //    mdToken SigUncompressToken(ImportScope & scope, PCCOR_SIGNATURE * sigPtr, PCCOR_SIGNATURE sigPtrEnd);
        //    ULONG SigUncompressData(ImportScope & scope, PCCOR_SIGNATURE * sigPtr, PCCOR_SIGNATURE sigPtrEnd);
        //    BYTE SigGetByte(ImportScope & scope, PCCOR_SIGNATURE * sigPtr, PCCOR_SIGNATURE sigPtrEnd);
        //    BYTE SigPeekByte(ImportScope & scope, PCCOR_SIGNATURE sigPtr, PCCOR_SIGNATURE sigPtrEnd);
        //
        //    ULONG SigPeekUncompressData(MODULESYM * mod, PCCOR_SIGNATURE sigPtr, PCCOR_SIGNATURE sigPtrEnd);
        //
        //    const void *CheckBufferAccess(MODULESYM * mod, const void *buffer, size_t cbRequired, size_t cbActual);
#if false
        //------------------------------------------------------------
        // IMPORTER.ImportSigType
        //
        /// <summary>
        /// <para>Import a single signature definition type.
        /// Return NULL if we don't have a corresponding type.
        /// sigPtr is updated to point just beyond the end of the type.</para>
        /// <para>Note that the type returned is not necessarily declared,
        /// which is what we want.</para>
        /// <para>Signatures don't distinguish between ref and out parameters,
        /// so we just return the base type and return that some byref is present
        /// via the returned isByref flags.</para>
        /// <para>In the case of generics this is complicated by the fact that type variables
        /// (which become TYVARSYMs) must "point" directly to the TYVARSYM corresponding to
        /// the declaration of the type variable.
        /// For example, a generic class C&lt;T&gt; has one child TYVARSYM for the declaration of T.
        /// All uses of T inside that class must point directly to (i.e. actually are) that TYVARSYM.
        /// Similarly for methods. As such, we pass arrays corresponding to the type variables that are in scope,
        /// similar to the arrays passed into SubstType.</para>
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="importedType"></param>
        /// <param name="sigOptions"></param>
        /// <param name="classTypeVariables"></param>
        /// <param name="methodTypevariables"></param>
        /// <param name="convertPinned"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM ImportSigType(
            INFILESYM infileSym,
            System.Type importedType,
            ImportSigOptions sigOptions,
            ref int modOptCount,
            TypeArray classTypeVariables,   // = null
            TypeArray methodTypevariables,  // = null
            bool convertPinned)             // = true
        {
            DebugUtil.Assert(importedType != null);
            ImportSigOptions sigOptionsModOpts = sigOptions & ImportSigOptions.IncludeModOpts;

            TYPESYM typeSym = null;

            TypeArray allTypeArray = new TypeArray();
            if (TypeArray.Size(classTypeVariables) > 0)
            {
                allTypeArray.Add(classTypeVariables);
            }
            if (TypeArray.Size(methodTypevariables) > 0)
            {
                allTypeArray.Add(methodTypevariables);
            }

            // case ELEMENT_TYPE_END:

            //--------------------------------------------------
            // case ELEMENT_TYPE_VOID:
            //--------------------------------------------------
            if (importedType == typeof(void))
            {
                if ((sigOptions & ImportSigOptions.AllowVoid) != 0)
                {
                    return Compiler.MainSymbolManager.VoidSym;
                }
            }

            //--------------------------------------------------
            // case ELEMENT_TYPE_BOOLEAN:
            // case ELEMENT_TYPE_CHAR:
            // case ELEMENT_TYPE_U1:
            // case ELEMENT_TYPE_I2:
            // case ELEMENT_TYPE_I4:
            // case ELEMENT_TYPE_I8:
            // case ELEMENT_TYPE_R4:
            // case ELEMENT_TYPE_R8:
            // case ELEMENT_TYPE_STRING:
            // case ELEMENT_TYPE_OBJECT:
            // case ELEMENT_TYPE_I1:      
            // case ELEMENT_TYPE_U2:    
            // case ELEMENT_TYPE_U4:
            // case ELEMENT_TYPE_U8:
            // case ELEMENT_TYPE_I:
            // case ELEMENT_TYPE_U:
            // case ELEMENT_TYPE_TYPEDBYREF:
            //--------------------------------------------------
            TypeArray reqTypeArray;
            if (importedType.IsGenericType)
            {
                reqTypeArray = SelectTypeArguments(
                    infileSym,
                    importedType.GetGenericArguments(),
                    classTypeVariables,
                    methodTypevariables);
            }
            else
            {
                reqTypeArray = BSYMMGR.EmptyTypeArray;
            }

            if (GetTypeSymFromCache(importedType, reqTypeArray, out typeSym))
            {
                return typeSym;
            }

            //--------------------------------------------------
            // case ELEMENT_TYPE_SZARRAY:
            //--------------------------------------------------
            //else if (importedType.IsArray && importedType.GetArrayRank() == 1)
            //{
            //    // processed in the case of ELEMENT_TYPE_ARRAY
            //}
            //--------------------------------------------------
            // case ELEMENT_TYPE_VAR:   // class
            // case ELEMENT_TYPE_MVAR:  // method
            //--------------------------------------------------
            if (importedType.IsGenericParameter)
            {
                int index = importedType.GenericParameterPosition;

                if (importedType.DeclaringMethod == null)
                {
                    if (0 <= index && index < TypeArray.Size(classTypeVariables))
                    {
                        typeSym = classTypeVariables[index];
                    }
                }
                else
                {
                    if (index < 0)
                    {
                    }
                    else if (methodTypevariables == null)
                    {
                        typeSym = compiler.MainSymbolManager.GetStdMethTypeVar(index);
                    }
                    else if (index < methodTypevariables.Count)
                    {
                        typeSym = methodTypevariables[index];
                    }
                }
            }
            //--------------------------------------------------
            // case ELEMENT_TYPE_GENERICINST:
            // Instantiated generic type
            //--------------------------------------------------
            else if (importedType.IsGenericType && !importedType.IsGenericTypeDefinition)
            {
                Type[] typeArgs = importedType.GetGenericArguments();
                int count = typeArgs.Length;
                TypeArray typeArray = new TypeArray();

                bool hasError = false;

                for (int i = 0; i < count; ++i)
                {
                    TYPESYM tempSym = ImportSigType(
                        infileSym,
                        typeArgs[i],
                        sigOptionsModOpts,
                        ref modOptCount,
                        classTypeVariables,
                        methodTypevariables,
                        true);
                    typeArray.Add(tempSym);
                    if (tempSym == null)
                    {
                        hasError = true;
                    }
                }

                if (hasError == false)
                {
                    typeArray = Compiler.MainSymbolManager.AllocParams(typeArray);
                    typeSym = ResolveType(
                        infileSym,
                        importedType,
                        classTypeVariables,
                        methodTypevariables);

                    // Translate "Nullable<T>" to "T?"
                    if (count == 1 && typeSym.IsPredefType(PREDEFTYPE.G_OPTIONAL))
                    {
                        typeSym = Compiler.MainSymbolManager.GetNubFromNullable(typeSym as AGGTYPESYM);
                    }

                    if (importedType.IsClass &&
                        typeSym != null &&
                        typeSym.IsStructOrEnum())
                    {
                        typeSym = null;
                    }
                }

                //typeArray = Compiler.MainSymbolManager.AllocParams(typeArray);
                typeSym = ResolveType(
                    infileSym,
                    importedType,
                    classTypeVariables,
                    methodTypevariables);

                // Translate "Nullable<T>" to "T?"
                if (count == 1 && typeSym.IsPredefType(PREDEFTYPE.G_OPTIONAL))
                {
                    typeSym = Compiler.MainSymbolManager.GetNubFromNullable(typeSym as AGGTYPESYM);
                }

                if (importedType.IsClass &&
                    typeSym != null &&
                    typeSym.IsStructOrEnum())
                {
                    typeSym = null;
                }
            }
            //--------------------------------------------------
            // case ELEMENT_TYPE_ARRAY:
            //--------------------------------------------------
            else if (importedType.IsArray)
            {
                if (importedType.HasElementType)
                {
                    TYPESYM elementSym = ImportSigType(
                        infileSym,
                        importedType.GetElementType(),
                        sigOptionsModOpts,
                        ref modOptCount,
                        classTypeVariables,
                        methodTypevariables,
                        true);

                    int rank = importedType.GetArrayRank();
                    if (elementSym != null && rank > 0)
                    {
#if false
                        object[] actArgs = new object[rank];
                        for (int i = 0; i < rank; ++i)
                        {
                            actArgs[i] = (int)0;
                        }
                        Array obj = Activator.CreateInstance(importedType, actArgs) as Array;
                        if (obj != null)
                        {
                            bool validArray = true;
                            for (int i = 0; i < rank; ++i)
                            {
                                if (obj.GetLowerBound(i) != 0)
                                {
                                    validArray = false;
                                    break;
                                }
                            }

                            if (validArray)
                            {
                                typeSym = Compiler.MainSymbolManager.GetArray(elementSym, rank);
                            }
                        }
#endif
                        typeSym = Compiler.MainSymbolManager.GetArray(elementSym, rank, importedType);
                    }
                }
            }
            //--------------------------------------------------
            // case ELEMENT_TYPE_PTR:
            //--------------------------------------------------
            else if (importedType.IsPointer)
            {
                // Pointer type. Note that void * is a valid type here.

                if (importedType.HasElementType)
                {
                    TYPESYM elementSym = ImportSigType(
                        infileSym,
                        importedType.GetElementType(),
                        sigOptionsModOpts | ImportSigOptions.AllowVoid,
                        ref modOptCount,
                        classTypeVariables,
                        methodTypevariables,
                        true);
                    //if ((sigOptions & ImportSigOptions.AllowByref) != 0 && elementSym != null)
                    if (elementSym != null)
                    {
                        typeSym = Compiler.MainSymbolManager.GetPtrType(elementSym);
                    }
                }
            }
            //--------------------------------------------------
            // case ELEMENT_TYPE_BYREF:
            //--------------------------------------------------
            else if (importedType.IsByRef)
            {
                // Byref param - could be ref or out, so just return indication of that.

                if (importedType.HasElementType)
                {
                    TYPESYM elementSym = ImportSigType(
                        infileSym,
                        importedType.GetElementType(),
                        sigOptionsModOpts,
                        ref modOptCount,
                        classTypeVariables,
                        methodTypevariables,
                        true);
                    if (elementSym != null)
                    {
                        typeSym = Compiler.MainSymbolManager.GetParamModifier(elementSym, false);
                    }
                }
            }

            //--------------------------------------------------
            // case ELEMENT_TYPE_CMOD_OPT:
            //--------------------------------------------------
            //--------------------------------------------------
            // case ELEMENT_TYPE_CMOD_REQD:
            //--------------------------------------------------
            //--------------------------------------------------
            // case ELEMENT_TYPE_PINNED:
            //--------------------------------------------------

            //--------------------------------------------------
            // case ELEMENT_TYPE_VALUETYPE:
            //--------------------------------------------------
            else if (importedType.IsValueType)
            {
                typeSym = ResolveType(
                    infileSym,
                    importedType,
                    classTypeVariables,
                    methodTypevariables);
                DebugUtil.Assert(
                    typeSym == null ||
                    !typeSym.IsAGGTYPESYM ||
                    (typeSym as AGGTYPESYM).IsInstType());

                // Arity should be zero.
                if (typeSym != null &&
                    typeSym.IsAGGTYPESYM &&
                    typeSym.IsStructOrEnum() &&
                    (typeSym as AGGTYPESYM).AllTypeArguments.Count > 0)
                {
                    // Bogus signature. It should have specified type args.
                    typeSym = null;
                }
            }
            //--------------------------------------------------
            // case ELEMENT_TYPE_CLASS:
            //--------------------------------------------------
            else if (importedType.IsClass)
            {
                // Element of class or struct type.
                typeSym = ResolveType(
                    infileSym,
                    importedType,
                    classTypeVariables,
                    methodTypevariables);
                DebugUtil.Assert(
                    typeSym == null ||
                    !typeSym.IsAGGTYPESYM ||
                    (typeSym as AGGTYPESYM).IsInstType());

                // ELEMENT_TYPE_CLASS followed by value type means the "boxed" version, which 
                // we don't support. Check for this case and return NULL.
                // Arity should be zero.
                if (typeSym != null &&
                    typeSym.IsAGGTYPESYM &&
                    typeSym.IsStructOrEnum() &&
                    (typeSym as AGGTYPESYM).AllTypeArguments.Count > 0)
                {
                    typeSym = null;
                }
            }
            //--------------------------------------------------
            // Otherwise, create TYPESYM from importedType and register it.
            //--------------------------------------------------
            else
            {
                ImportOneType(infileSym, importedType);

                if (GetTypeSymFromCache(importedType, classTypeVariables, out typeSym))
                {
                    return typeSym;
                }
                DebugUtil.VsFail("IMPORTER.ImportSigType");
                return null;
            }

        if (typeSym.GetSystemType() == null)
            {
                typeSym.SetSystemType(importedType);
            }
            return typeSym;
        }
#endif
        //------------------------------------------------------------
        // IMPORTER::ImportSigType (sscli)
        //------------------------------------------------------------
        //PTYPESYM ImportSigType(
        //    ImportScope & scope,
        //    PCCOR_SIGNATURE * sigPtr,
        //    PCCOR_SIGNATURE sigPtrEnd,
        //    int grfiso,
        //    int *pmodOptCount,
        //    TypeArray * typeVarsCls = NULL,
        //    TypeArray * typeVarsMeth = NULL,
        //    bool convertPinned = true)
        //{
        //    PCCOR_SIGNATURE sig = *sigPtr;
        //    TYPESYM * type;
        //    mdTypeRef token;
        //    int grfisoRec = grfiso & kfisoIncludeModOpts;
        //
        //    if (sig >= sigPtrEnd) {
        //        BogusMetadataFailure(scope);
        //    }
        //
        //    switch (*sig++) {
        //    case ELEMENT_TYPE_END:
        //        // Bogus.
        //        type = NULL;
        //        break;
        //
        //    case ELEMENT_TYPE_VOID:
        //        if (grfiso & kfisoAllowVoid)
        //            type = compiler()->getBSymmgr().GetVoid();
        //        else
        //            type = NULL;
        //        break;
        //
        //    case ELEMENT_TYPE_BOOLEAN:
        //        type = compiler()->GetReqPredefType(PT_BOOL, false);
        //        break;
        //
        //    case ELEMENT_TYPE_CHAR:
        //        type = compiler()->GetReqPredefType(PT_CHAR, false);
        //        break;
        //
        //    case ELEMENT_TYPE_U1:
        //        type = compiler()->GetReqPredefType(PT_BYTE, false);
        //        break;
        //
        //    case ELEMENT_TYPE_I2:
        //        type = compiler()->GetReqPredefType(PT_SHORT, false);
        //        break;
        //
        //    case ELEMENT_TYPE_I4:
        //        type = compiler()->GetReqPredefType(PT_INT, false);
        //        break;
        //
        //    case ELEMENT_TYPE_I8:
        //        type = compiler()->GetReqPredefType(PT_LONG, false);
        //        break;
        //
        //    case ELEMENT_TYPE_R4:
        //        type = compiler()->GetReqPredefType(PT_FLOAT, false);
        //        break;
        //
        //    case ELEMENT_TYPE_R8:
        //        type = compiler()->GetReqPredefType(PT_DOUBLE, false);
        //        break;
        //
        //    case ELEMENT_TYPE_STRING:
        //        type = compiler()->GetReqPredefType(PT_STRING, false);
        //        break;
        //
        //    case ELEMENT_TYPE_OBJECT:
        //        type = compiler()->GetReqPredefType(PT_OBJECT, false);
        //        break;
        //
        //    case ELEMENT_TYPE_I1:      
        //        type = compiler()->GetReqPredefType(PT_SBYTE, false);
        //        break;
        //
        //    case ELEMENT_TYPE_U2:    
        //        type = compiler()->GetReqPredefType(PT_USHORT, false);
        //        break;
        //
        //    case ELEMENT_TYPE_U4:
        //        type = compiler()->GetReqPredefType(PT_UINT, false);
        //        break;
        //
        //    case ELEMENT_TYPE_U8:
        //        type = compiler()->GetReqPredefType(PT_ULONG, false);
        //        break;
        //
        //    case ELEMENT_TYPE_I:
        //        type = compiler()->GetReqPredefType(PT_INTPTR, false);
        //        break;
        //
        //    case ELEMENT_TYPE_U:
        //        type = compiler()->GetReqPredefType(PT_UINTPTR, false);
        //        break;
        //
        //    case ELEMENT_TYPE_TYPEDBYREF:
        //        type = compiler()->GetOptPredefType(PT_REFANY, false);
        //        break;
        //
        //    case ELEMENT_TYPE_VALUETYPE:
        //        token = SigUncompressToken(scope, &sig, sigPtrEnd);  // updates sig.
        //        type = ResolveTypeRefOrDef(scope, token, NULL);
        //        ASSERT(!type || !type->IsAGGTYPESYM || type->AsAGGTYPESYM->IsInstType());
        //
        //        // Arity should be zero.
        //        if (type && type->IsAGGTYPESYM && type->AsAGGTYPESYM->typeArgsAll->size) {
        //            // Bogus signature. It should have specified type args.
        //            type = NULL;
        //        }
        //        break;
        //
        //    case ELEMENT_TYPE_CLASS:
        //        // Element of class or struct type.
        //        token = SigUncompressToken(scope, &sig, sigPtrEnd);  // updates sig.
        //        type = ResolveTypeRefOrDef(scope, token, NULL);
        //        ASSERT(!type || !type->IsAGGTYPESYM || type->AsAGGTYPESYM->IsInstType());
        //
        //        // ELEMENT_TYPE_CLASS followed by value type means the "boxed" version, which 
        //        // we don't support. Check for this case and return NULL.
        //        // Arity should be zero.
        //        if (type && type->IsAGGTYPESYM && (type->isStructOrEnum() || type->AsAGGTYPESYM->typeArgsAll->size)) {
        //            type = NULL;
        //        }
        //        break;
        //
        //    case ELEMENT_TYPE_SZARRAY:
        //        // Single-dimensional array with 0 lower bound
        //        type = ImportSigType(scope, &sig, sigPtrEnd, grfisoRec, pmodOptCount, typeVarsCls, typeVarsMeth);  // Get element type.
        //        if (type) {
        //            type = compiler()->getBSymmgr().GetArray(type, 1);
        //        }
        //        break;
        //
        //    case ELEMENT_TYPE_VAR: {
        //        int index = CorSigUncompressData(sig);
        //        if (index < TypeArray::Size(typeVarsCls) && 0 <= index)
        //            type = typeVarsCls->Item(index);
        //        else
        //            type = NULL;
        //        break;
        //    }
        //    case ELEMENT_TYPE_MVAR: {
        //        int index = CorSigUncompressData(sig);
        //        if (index < 0)
        //            type = NULL;
        //        else if (!typeVarsMeth)
        //            type = compiler()->getBSymmgr().GetStdMethTypeVar(index);
        //        else if (index < typeVarsMeth->size)
        //            type = typeVarsMeth->Item(index);
        //        else
        //            type = NULL;
        //        break;
        //    }
        //    case ELEMENT_TYPE_GENERICINST: {
        //        // Instantiated generic type
        //        byte b = (*sig++);
        //        token = SigUncompressToken(scope, &sig, sigPtrEnd);  // updates sig.
        //        int ctype = CorSigUncompressData(sig);
        //        TYPESYM ** prgtype = STACK_ALLOC(TYPESYM *, ctype);
        //        bool fErrors = false;
        //        for (int i = 0; i < ctype; i++) {
        //            prgtype[i] = ImportSigType(scope, &sig, sigPtrEnd, grfisoRec, pmodOptCount, typeVarsCls, typeVarsMeth);  // Get element type.
        //            if (!prgtype[i])
        //                fErrors = true;
        //        }
        //        if (fErrors) {
        //            type = NULL;
        //            break;
        //        }
        //        TypeArray * typeArgs = compiler()->getBSymmgr().AllocParams(ctype, prgtype);
        //        type = ResolveTypeRefOrDef(scope, token, typeArgs);
        //
        //        // Translate "Nullable<T>" to "T?"
        //        if (ctype == 1 && type->isPredefType(PT_G_OPTIONAL))
        //            type = compiler()->getBSymmgr().GetNubFromNullable(type->AsAGGTYPESYM);
        //
        //        // ELEMENT_TYPE_CLASS followed by value type means the "boxed" version, which 
        //        // we don't support. Check for this case and return NULL.
        //        if (b == ELEMENT_TYPE_CLASS && type && type->isStructOrEnum())
        //            type = NULL;
        //        break;
        //    }
        //
        //    case ELEMENT_TYPE_ARRAY:
        //        // Multi-dimensional array. We only support arrays
        //        // with unspecified length and zero lower bound.
        //
        //        int rank, lowBound, numRanks;
        //
        //        type = ImportSigType(scope, &sig, sigPtrEnd, grfisoRec, pmodOptCount, typeVarsCls, typeVarsMeth);  // Get element type.
        //
        //        rank = SigUncompressData(scope, &sig, sigPtrEnd);  // Get rank.
        //        if (rank == 0) {
        //            type = NULL;
        //            break;
        //        }
        //
        //        // Get sizes of each rank.
        //        numRanks = SigUncompressData(scope, &sig, sigPtrEnd);
        //        if (numRanks > 0) {
        //            // We don't support sizing of arrays.
        //            type = NULL;
        //            for (int i = 0; i < numRanks; ++i)
        //                SigUncompressData(scope, &sig, sigPtrEnd); // skip the sizes.
        //        }
        //
        //        // Get lower bounds of each rank.
        //        numRanks = SigGetByte(scope, &sig, sigPtrEnd);
        //        while (numRanks--) {
        //            lowBound = SigUncompressData(scope, &sig, sigPtrEnd);
        //            if (lowBound != 0)
        //                type = NULL;     // We don't support non-zero lower bounds.
        //        }
        //
        //        // Get the array symbol, if its an array type that we support.
        //        if (type)
        //            type = compiler()->getBSymmgr().GetArray(type, rank);
        //        break;
        //
        //    case ELEMENT_TYPE_PTR:
        //        // Pointer type. Note that void * is a valid type here.
        //        type = ImportSigType(scope, &sig, sigPtrEnd, kfisoAllowVoid | grfisoRec, pmodOptCount, typeVarsCls, typeVarsMeth);
        //        if (type)
        //            type = compiler()->getBSymmgr().GetPtrType(type);
        //        break;
        //
        //    case ELEMENT_TYPE_BYREF:
        //        // Byref param - could be ref or out, so just return indication of that.
        //        type = ImportSigType(scope, &sig, sigPtrEnd, grfisoRec, pmodOptCount, typeVarsCls, typeVarsMeth);
        //        if ((grfiso & kfisoAllowByref) && type)
        //            type = compiler()->getBSymmgr().GetParamModifier(type, false);
        //        else
        //            type = NULL;     // byref isn't allowed here.
        //        break;
        //
        //    case ELEMENT_TYPE_CMOD_OPT:
        //        token = SigUncompressToken(scope, &sig, sigPtrEnd);  // Ignore the following optional token
        //
        //        // get the 'real' type here
        //        type = ImportSigType(scope, &sig, sigPtrEnd, grfiso, pmodOptCount, typeVarsCls, typeVarsMeth);
        //        if (pmodOptCount) {
        //            *pmodOptCount += 1;
        //        }
        //        if ((grfiso & kfisoIncludeModOpts) && type) {
        //            type = scope.GetModule() ? compiler()->getBSymmgr().GetModOptType(type, token, scope.GetModule()) : NULL;
        //        }
        //        break;
        //
        //    case ELEMENT_TYPE_CMOD_REQD:
        //        token = SigUncompressToken(scope, &sig, sigPtrEnd);  // Ignore the following optional token
        //
        //        // get the 'real' type here
        //        ImportSigType(scope, &sig, sigPtrEnd, grfiso, pmodOptCount, typeVarsCls, typeVarsMeth);
        //
        //        // We are required to understand this, since we don't, just return NULL.
        //        type = NULL;
        //        break;
        //
        //    case ELEMENT_TYPE_PINNED:
        //        type = ImportSigType(scope, &sig, sigPtrEnd, kfisoAllowVoid | kfisoAllowByref | grfisoRec, pmodOptCount, typeVarsCls, typeVarsMeth);
        //        if (type && type->IsPARAMMODSYM) {
        //            type = compiler()->getBSymmgr().GetPtrType(type->AsPARAMMODSYM->paramType());
        //        }
        //        if (type) {
        //            if (!convertPinned) {
        //                type = compiler()->getBSymmgr().GetPinnedType(type);
        //            }
        //            else if(!type->IsPTRSYM) {
        //                type = NULL;
        //            }
        //        }
        //        break;
        //
        //    default:
        //        // Something we don't know about.
        //        if (CorIsModifierElementType((CorElementType) *(sig - 1))) {
        //            // Consume what is modified.
        //            ImportSigType(scope, &sig, sigPtrEnd, kfisoAllowVoid | kfisoAllowByref | grfisoRec, pmodOptCount, typeVarsCls, typeVarsMeth);
        //        }
        //        type = NULL;
        //        break;
        //    }
        //
        //    // Update the signature pointer of the called.
        //    *sigPtr = sig;
        //    return type;
        //}

        //    void ImportSignatureWithModOpts(MODULESYM *scope, PCCOR_SIGNATURE sig, PCCOR_SIGNATURE sigEnd, TYPESYM **retType, TypeArray ** params,
        //        int *pmodOptCount, TypeArray *pClassTypeFormals, TypeArray *pMethTypeFormals);

        //------------------------------------------------------------
        // IMPORTER.GetAssemblyName
        //------------------------------------------------------------
        internal string GetAssemblyName(SYM sym)
        {
            INFILESYM inFileSym = null;

            // Strip off ARRAYSYM, PTRSYM, etc.
            if (sym.IsTYPESYM)
            {
                sym = (sym as TYPESYM).GetNakedType(true);
            }

            // For type variables, just look at the parent.
            if (sym.IsTYVARSYM)
            {
                sym = sym.ParentSym;
            }

            // An AGGSYM can't be used to find an input file if it's defined in source.
            // But we don't care in that case anyway, so luckily not a problem
            if ((!sym.IsAGGSYM || !(sym as AGGSYM).IsSource) &&
                (!sym.IsAGGTYPESYM || !(sym as AGGTYPESYM).GetAggregate().IsSource))
            {
                inFileSym = sym.GetInputFile();
            }

            if (inFileSym == null || inFileSym.GetAssemblyID() == Kaid.ThisAssembly) return null;
            return inFileSym.AssemblyNameString;
        }

        //------------------------------------------------------------
        // IMPORTER.GetAssemblyName
        //------------------------------------------------------------
        //    HRESULT GetAssemblyName(IMetaDataAssemblyImport * assemImport, NAME ** nameAsNAME, BSTR * nameAsBSTR, WORD * assemblyVersion);

        //HRESULT IMPORTER::GetAssemblyName(IMetaDataAssemblyImport * assemimport, NAME ** nameAsNAME, BSTR * nameAsBSTR, WORD* assemblyVersion)
        //{
        //
        //    HRESULT HR;
        //
        //    if (nameAsBSTR) {
        //        *nameAsBSTR = NULL;
        //    }
        //    if (nameAsNAME) {
        //        *nameAsNAME = NULL;
        //    }
        //    mdAssembly tkAsm;
        //    HR = assemimport->GetAssemblyFromScope( &tkAsm);
        //    if (FAILED(HR)) goto LERROR;
        //
        //    if (TypeFromToken(tkAsm) != mdtAssembly || tkAsm == mdAssemblyNil) {
        //        HR = S_OK;
        //        goto LERROR;
        //    }
        //
        //    //
        //    // get required sizes for stuff
        //    //
        //    ASSEMBLYMETADATA data;
        //    ULONG cbOriginator;
        //    ULONG cchName;
        //    DWORD flags;
        //    MEM_SET_ZERO(data);
        //    HR = assemimport->GetAssemblyProps( 
        //        tkAsm, 
        //        NULL, &cbOriginator,// originator
        //        NULL,               // hask alg
        //        NULL, 0, &cchName,  // name
        //        &data,              // data
        //        &flags);
        //    if (FAILED(HR)) goto LERROR;
        //    flags |= afPublicKey; // AssemblyDefs always have the full public key (not just the token) but they don't set this bit
        //
        //    //
        //    // Stack allocate and get the actual values
        //    //
        //    LPCVOID pbOriginator;
        //    WCHAR *szName; szName = STACK_ALLOC(WCHAR, cchName);
        //    data.szLocale = data.cbLocale ? STACK_ALLOC(WCHAR, data.cbLocale) : NULL;
        //    HR = assemimport->GetAssemblyProps( 
        //        tkAsm, 
        //        &pbOriginator, &cbOriginator,// originator
        //        NULL,               // hask alg
        //        szName, cchName, &cchName,  // name
        //        &data,              // data
        //        NULL);
        //    if (FAILED(HR)) goto LERROR;
        //
        //
        //    HR = MakeAssemblyName(szName, cchName, data, (LPBYTE)pbOriginator, cbOriginator, flags, nameAsBSTR, nameAsNAME);
        //
        //    if (SUCCEEDED(HR) && assemblyVersion != NULL) {
        //        assemblyVersion[0] = data.usMajorVersion;
        //        assemblyVersion[1] = data.usMinorVersion;
        //        assemblyVersion[2] = data.usBuildNumber;
        //        assemblyVersion[3] = data.usRevisionNumber;
        //    }
        //
        //LERROR:
        //
        //    return HR;
        //}

        //------------------------------------------------------------
        // IMPORTER.GetAssemblyName
        //------------------------------------------------------------
        //    NAME * GetAssemblyName(ImportScope & scopeSource, mdAssemblyRef tkAsmRef);

        //NAME * IMPORTER::GetAssemblyName(ImportScope & scopeSource, mdAssemblyRef tkAssemblyRef)
        //{
        //    IMetaDataAssemblyImport * assemimport = scopeSource.GetAssemblyImport();
        //
        //    // get required sizes for stuff
        //    ULONG cbOriginator;
        //    ULONG cchName;
        //    DWORD flags;
        //    ASSEMBLYMETADATA data;
        //    MEM_SET_ZERO(data);
        //    CheckHR(assemimport->GetAssemblyRefProps(
        //        tkAssemblyRef, 
        //        NULL, &cbOriginator,// originator
        //        NULL, 0, &cchName,  // name
        //        &data,              // data
        //        NULL, NULL,         // hash
        //        &flags), scopeSource);
        //
        //    //
        //    // Stack allocate and get the actual values
        //    //
        //    LPCVOID pbOriginator;
        //    WCHAR *szName = STACK_ALLOC(WCHAR, cchName);
        //    data.szLocale = data.cbLocale ? STACK_ALLOC(WCHAR, data.cbLocale) : NULL;
        //    CheckHR(assemimport->GetAssemblyRefProps( 
        //        tkAssemblyRef,
        //        &pbOriginator, &cbOriginator,// originator
        //        szName, cchName, &cchName,  // name
        //        &data,                      // data
        //        NULL, NULL,                 // hash
        //        NULL), scopeSource);
        //
        //    //
        //    // Now actually format and create the NAME
        //    //
        //    NAME * nameAssemblyRef;
        //    CheckHR(MakeAssemblyName(szName, cchName, data, (LPBYTE)pbOriginator, cbOriginator, flags, NULL, &nameAssemblyRef), scopeSource);
        //
        //    return nameAssemblyRef;
        //}

        //------------------------------------------------------------
        // IMPORTER.GetOutputAssemblyName
        //
        /// <summary>
        /// <para>(sscli)
        /// Creates a fusion IAssemblyName object for the given output file
        /// Also returns the NAME*</para>
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="outputName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal AssemblyName GetOutputAssemblyName(PEFile outputFile, out string outputName)
        internal AssemblyName GetOutputAssemblyName(OUTFILESYM outfileSym, out string outputName)
        {
            outputName = null;
            if (outfileSym != null && outfileSym.AssemblyBuilderEx != null)
            {
                outputName = outfileSym.AssemblyBuilderEx.FileName;
                return outfileSym.AssemblyBuilderEx.AssemblyNameObject;
            }
            return null;
        }

        //IAssemblyName * IMPORTER::GetOutputAssemblyName( PEFile * fileOutput, NAME ** pnameOutput)
        //{
        //    HRESULT hr;
        //    CComPtr<IMetaDataAssemblyImport> pmaiOutput;
        //    NAME * nameOutput;
        //    CComPtr<IAssemblyName> panOutput;
        //
        //    *pnameOutput = NULL;
        //    if (FAILED(InitFusionAPIs()))
        //        return NULL;
        //
        //    if (SUCCEEDED(hr = fileOutput->GetEmit()->QueryInterface( IID_IMetaDataAssemblyImport, (void**)&pmaiOutput)) &&
        //        SUCCEEDED(hr = GetAssemblyName( pmaiOutput, &nameOutput, NULL, NULL)))
        //    {
        //        hr = m_pfnCreateAssemblyNameObject( &panOutput, nameOutput->text, CANOF_PARSE_DISPLAY_NAME, NULL);
        //    }
        //    if (FAILED(hr)) {
        //        compiler()->Error(NULL, FTL_MetadataEmitFailure, compiler()->ErrHR(hr), fileOutput->GetOutFile()->name->text);
        //        return NULL;
        //    }
        //
        //    *pnameOutput = nameOutput;    
        //    return panOutput.Detach();
        //}

        //------------------------------------------------------------
        // IMPORTER.ConfirmMatchesThisAssembly
        //          UNDER CONSTRUCTION
        /// <summary>
        /// Compares panOutput to nameAssemblyRef
        /// Gives a warning if a circular reference does not match.
        /// If the reference is purely a friend assembly reference (i.e. InternalsVisibleTo("xxx")) then
        /// we give an error if the friend access is used and the references for sure do not match.
        /// We give a warning if the friend access is used,
        /// the references do not match but they could unify,
        /// and we do not report anything if the friend access is never used.
        /// </summary>
        /// <remarks>
        /// REVIEW GrantRi: Should we give unification warnings here?
        /// </remarks>
        /// <param name="outputAsmName"></param>
        /// <param name="outputName"></param>
        /// <param name="nameAssemblyRef"></param>
        /// <param name="listModSrc"></param>
        /// <param name="fIsFriendAssemblyRefOnly"></param>
        //------------------------------------------------------------
        internal void ConfirmMatchesThisAssembly(
            AssemblyName outputAsmName,
            string outputName,
            string nameAssemblyRef,
            List<SYM> listModSrc,	//SYMLIST * listModSrc,
            bool fIsFriendAssemblyRefOnly)
        {
            ImportScopeModule scope = new ImportScopeModule(this, listModSrc[0] as MODULESYM);
            //AssemblyIdentityComparison asmCompare=new AssemblyIdentityComparison(m_pfnCompareAssemblyIdentity);
            //HRESULT hr;

            // check whether or not a friend assembly reference was used.
            bool friendUsage = false;

            if (listModSrc != null)
            {
                foreach (SYM sym in listModSrc)
                {
                    MODULESYM mod = sym as MODULESYM;
                    if (mod != null && mod.GetInputFile().FriendAccessUsed)
                    {
                        friendUsage = true;
                        break;
                    }
                }
            }

            CError err = null;
            if (friendUsage)
            {
                // If friend access was used, we need to use the IsEqual API
                // in order to mimic the lookup the runtime will do at execution.
                // Friend assembly checks at runtime are not affected by policy,
                // so the assemblies either match or they don't,
                // and we can give an error in the case where they don't.
                // Note:  Even though the friend assembly check is not influenced by policy,
                // it's possible that the assembly loading could be.

                //CComPtr<IAssemblyName> panRef;
                //CComPtr<IAssemblyName> panDef;
                //AssemblyName asmNameRef = null;
                //AssemblyName asmNameDef = null;
                //ParseAssemblyName(nameAssemblyRef, scope,out asmNameRef);
                //ParseAssemblyName(outputName, scope,out asmNameDef);
                //DebugUtil.Assert(asmNameRef!=null && asmNameDef!=null);
                //HRESULT hr = S_OK;

                //CheckHR(hr = panRef.IsEqual(panDef, ASM_CMPF_DEFAULT), scope);

                //if (hr == S_FALSE)
                //{
                //    err = compiler().MakeError(null, CSCERRID.ERR_FriendRefNotEqualToThis,
                //    	nameAssemblyRef, outputName);
                //}
            }
            else // if (friendUsage)
            {
                //CheckHR(hr = asmCompare.Compare(nameAssemblyRef.text, false, outputName.text, true), scope);
                //if (FAILED(hr))
                //{
                //    CheckHR(hr, scope);
                //    return;
                //}

                // there is a circular reference (without using a friend assembly reference)
                // so we give a unification warning since the assemblies could unify at runtime.
                //if (!fIsFriendAssemblyRefOnly && !asmCompare.IsEquivalentNoUnify() && asmCompare.CouldUnify())
                //    err = compiler().MakeError(null, CSCERRID.WRN_AssumedMatchThis, nameAssemblyRef, outputName);
            } //if (friendUsage)

            if (err == null)
            {
                return;
            }

            if (listModSrc != null)
            {
                foreach (SYM sym in listModSrc)
                {
                    MODULESYM mod = sym as MODULESYM;
                    Compiler.AddLocationToError(
                        err,
                        new ERRLOC(
                            mod.GetInputFile(),
                            null,
                            Compiler.OptionManager.FullPaths));
                }

                Compiler.SubmitError(err);
            }
        }

        //------------------------------------------------------------
        // IMPORTER.MapAssemblyRefToAid
        //
        /// <summary>
        /// <para>Use the Assembly Equivalence API to match the ref to the closest def amongst our imports</para>
        /// <para>Under Construction. Compare assembly names only, for now.</para>
        /// </summary>
        //------------------------------------------------------------
        internal int MapAssemblyRefToAid(
            string assemblyNameString,
            SYM inFileSym,
            bool isFriendAssemblyRef)
        {
            AssemblyName asmName = ParseAssemblyName(assemblyNameString);
            if (asmName == null) return Kaid.Unresolved;
            int aid = Kaid.Unresolved;

            // Just return any cached value (which includes failures).
            SYM sym = this.compiler.MainSymbolManager.NameToSymbolTable.GetSymFromName(asmName.Name);
            if (sym != null)
            {
                aid = (sym as NSAIDSYM).GetAssemblyID();
                if (aid == Kaid.ThisAssembly)
                {
                    Compiler.RecordAssemblyRefToOutput(asmName.Name, inFileSym, isFriendAssemblyRef);
                }
                return aid;
            }

            OUTFILESYM mdFileRootSym = Compiler.MainSymbolManager.MetadataFileRootSym;
            INFILESYM inFileCompare = null;
            //INFILESYM inFileCandidate = null;
            //INFILESYM inFileBadVersion = null;

            // Go through all the inputfile symbols. checking for an exact match
            for (inFileCompare = mdFileRootSym.FirstInFileSym();
                inFileCompare != null;
                inFileCompare = inFileCompare.NextInFileSym())
            {
                if (!inFileCompare.IsAssemblyLoaded || inFileCompare.IsBogus) continue;
                if (String.Compare(inFileCompare.AssemblyNameString, asmName.Name) == 0)
                {
                    aid = inFileCompare.GetAssemblyID();
                    goto FOUND;
                }
            }

            // Check if it is a valid assembly name (including partials)
            if (asmName == null) goto FOUND;    // Couldn't be parsed, so it will be unresolved

            // Go through all the inputfiles again looking for match
            // and keep track of any partial matches
            for (inFileCompare = mdFileRootSym.FirstInFileSym();
                inFileCompare != null;
                inFileCompare = inFileCompare.NextInFileSym())
            {
            }

        FOUND:
            //SetSymInCache(asmName.Name, Compiler.MainSymbolManager.GetRootNsAid(aid));
            this.compiler.MainSymbolManager.NameToSymbolTable.SetSymForName(
                asmName.Name,
                Compiler.MainSymbolManager.GetRootNsAid(aid));
            //Compiler.MainSymbolManager.NameToSymbolTable.SetSymForName(
            //    asmName.Name,
            //    Compiler.MainSymbolManager.GetRootNsAid(aid)
            //    );
            return aid;
        }
        //------------------------------------------------------------
        // IMPORTER::MapAssemblyRefToAid (sscli)
        //------------------------------------------------------------
        //int MapAssemblyRefToAid(NAME * nameAssemblyRef, ImportScope & scopeSource, bool fIsFriendAssemblyRef);
        //int IMPORTER::MapAssemblyRefToAid(NAME * nameAssemblyRef, ImportScope & scopeSource, bool fIsFriendAssemblyRef)
        //{
        //    // Just return any cached value (which includes failures).
        //    SYM * symT;
        //    if (compiler()->getBSymmgr().GetNameToSymTable()->GetSymFromName(nameAssemblyRef, &symT)) {
        //        int aid = symT->AsNSAIDSYM->GetAid();
        //        if (aid == kaidThisAssembly)
        //            compiler()->RecordAssemblyRefToOutput(nameAssemblyRef, scopeSource.GetModule(), fIsFriendAssemblyRef);
        //        return aid;
        //    }
        //
        //    HRESULT hr;
        //    if (FAILED(hr = InitFusionAPIs()))
        //        return kaidUnresolved;
        //
        //    // In the EE case S_FALSE means that we couldn't load the Comparison API, so we must be running
        //    // On a down-level platform, so fall-back to our own comparison API, which just plain isn't as robust
        //    ASSERT(hr == S_OK);
        //
        //    OUTFILESYM * mdfileroot;
        //    INFILESYM * infileCompare;
        //    INFILESYM * infileCandidate = NULL;
        //    INFILESYM * infileBadVersion = NULL;
        //    int aid = kaidUnresolved;
        //    bool fPlatformUnify = false;
        //    CComPtr<IAssemblyName> panRef;
        //
        //    mdfileroot = compiler()->getBSymmgr().GetMDFileRoot();
        //
        //    // Go through all the inputfile symbols. checking for an exact match
        //    for (infileCompare = mdfileroot->firstInfile();
        //            infileCompare != NULL;
        //            infileCompare = infileCompare->nextInfile())
        //    {
        //        if (infileCompare->isAddedModule || infileCompare->getBogus())
        //            continue;
        //
        //        GetAssemblyName(infileCompare);
        //
        //        if (infileCompare->assemblyName == nameAssemblyRef) {
        //            aid = infileCompare->GetAssemblyID();
        //            goto FOUND;
        //        }
        //    }
        //
        //    // Check if it is a valid assembly name (including partials)
        //    ParseAssemblyName( nameAssemblyRef->text, scopeSource, &panRef);
        //    if (panRef == NULL) // Couldn't be parsed, so it will be unresolved
        //        goto FOUND;
        //
        //    // Go through all the inputfiles again looking for match
        //    // and keep track of any partial matches
        //    for (infileCompare = mdfileroot->firstInfile();
        //        infileCompare != NULL;
        //        infileCompare = infileCompare->nextInfile())
        //    {
        //        if (infileCompare->isAddedModule || infileCompare->getBogus())
        //            continue;
        //
        //        ASSERT(infileCompare->assemblyName);
        //        ASSERT(panRef != NULL);
        //        AssemblyIdentityComparison aic(m_pfnCompareAssemblyIdentity); 
        //        {
        //            hr = aic.Compare(nameAssemblyRef->text, false, infileCompare->assemblyName->text, true);
        //            if (hr == HRESULT_FROM_WIN32(ERROR_INVALID_DATA) || hr == FUSION_E_INVALID_NAME) {
        //                compiler()->Error(scopeSource, WRN_InvalidAssemblyName, nameAssemblyRef->text);
        //                goto FOUND;
        //            }
        //            CheckHR(hr, scopeSource);
        //        }
        //        if (FAILED(hr))
        //            continue;
        //
        //        if (!aic.IsEquivalent()) {
        //            if (aic.NonEquivalentDueToVersions() &&
        //                (infileBadVersion == NULL || infileBadVersion->CompareVersions( infileCompare) < 0)) {
        //                infileBadVersion = infileCompare;
        //            }
        //            continue;
        //        }
        //
        //        if (aic.CouldUnify()) {
        //            fPlatformUnify = aic.CouldUnify(true);
        //            if (infileCandidate == NULL || infileCandidate->CompareVersions( infileCompare) > 0) {
        //                infileCandidate = infileCompare;
        //            }
        //            continue;
        //        }
        //
        //        if (aic.IsEquivalentNoUnify()) {
        //            aid = infileCompare->GetAssemblyID();
        //            goto FOUND;
        //        }
        //            
        //        VSFAIL("Invalid AssemblyComparisonResult and fEquivalent combo");
        //    }
        //
        //    if (infileCandidate != NULL) {
        //        // If we had an exact match, we wouldn't be here
        //        if (!fPlatformUnify) {
        //            int diff = 0;
        //            diff = CompareVersions( scopeSource, panRef, infileCandidate);
        //            if (diff == 0) {
        //                // partial match that unfied on version, but there's no diff?
        //                VSFAIL("Can't have a partial unified with the same version");
        //            }
        //            else if (diff <= 2) {
        //                compiler()->Error( scopeSource, WRN_UnifyReferenceMajMin, nameAssemblyRef->text, infileCandidate->assemblyName->text, ErrArgRefOnly(infileCandidate));
        //            }
        //            else {
        //                compiler()->Error( scopeSource, WRN_UnifyReferenceBldRev, nameAssemblyRef->text, infileCandidate->assemblyName->text, ErrArgRefOnly(infileCandidate));
        //            }
        //        }
        //        aid = infileCandidate->GetAssemblyID();
        //        goto FOUND;
        //    }
        //
        //    if (MatchesThisAssembly(nameAssemblyRef, scopeSource)) {
        //        compiler()->RecordAssemblyRefToOutput(nameAssemblyRef, scopeSource.GetModule(), fIsFriendAssemblyRef);
        //        aid = kaidThisAssembly;
        //    }
        //    else 
        //        if (infileBadVersion) {
        //        ASSERT(!fPlatformUnify && panRef);
        //        ASSERT(!infileBadVersion->isBCL);
        //        ASSERT(scopeSource.GetModule());
        //
        //        PCWSTR pszAsm = NULL;
        //        ASSERT(scopeSource.GetModule());
        //        if (scopeSource.GetModule())
        //            pszAsm = GetAssemblyName(scopeSource.GetModule());
        //        if (!pszAsm)
        //            pszAsm = scopeSource.GetFileName();
        //        compiler()->Error(scopeSource, ERR_AssemblyMatchBadVersion, pszAsm, nameAssemblyRef->text, infileBadVersion->assemblyName->text, ErrArgRefOnly(infileBadVersion));
        //        aid = infileBadVersion->GetAssemblyID();
        //    }
        //
        //FOUND:
        //    compiler()->getBSymmgr().GetNameToSymTable()->SetSymForName(nameAssemblyRef, compiler()->getBSymmgr().GetRootNsAid(aid));
        //    return aid;
        //}

        //------------------------------------------------------------
        // IMPORTER.MapAssemblyRefToAid
        //
        /// Look up an AssemblyRef token and find the aid that it matches. If the assembly ref can be
        /// resolved, this returns the assembly id. Otherwise, it fabricates a module in the unresolved
        /// infile and returns the module id.
        //------------------------------------------------------------
        //------------------------------------------------------------
        // IMPORTER::MapAssemblyRefToAid (sscli)
        //------------------------------------------------------------
        //int MapAssemblyRefToAid(ImportScope & scopeSource, mdAssemblyRef tkAssemblyRef);
        //int IMPORTER::MapAssemblyRefToAid(ImportScope & scopeSource, mdAssemblyRef tkAssemblyRef)
        //{
        //    ASSERT(TypeFromToken(tkAssemblyRef) == mdtAssemblyRef);
        //
        //    // Return any cached value.
        //    SYM * symT;
        //    if (GetSymFromCache(scopeSource, tkAssemblyRef, &symT))
        //        return symT->AsNSAIDSYM->GetAid();
        //
        //    // Make sure we have the error assembly name.
        //    if (!m_nameErrorAssem) {
        //        ASSEMBLYMETADATA data;
        //        NAME * name = compiler()->namemgr->GetPredefName(PN_ERROR_ASSEM);
        //        memset(&data, 0, sizeof(data));
        //        // NOTE: cchName shoudl include the nul-terminator, hence the wcslen + 1.
        //        HRESULT hr; hr = MakeAssemblyName( name->text, (ULONG)wcslen(name->text) + 1, data, NULL, 0, 0, NULL, &m_nameErrorAssem);
        //        ASSERT(SUCCEEDED(hr));
        //    }
        //
        //    // Get the ref's assembly name
        //    NAME * nameAssemblyRef = GetAssemblyName(scopeSource, tkAssemblyRef);
        //    
        //    int aid = (nameAssemblyRef == m_nameErrorAssem) ? kaidErrorAssem : MapAssemblyRefToAid(nameAssemblyRef, scopeSource, false);
        //
        //    if (aid == kaidUnresolved) {
        //        // Create a module in the unresolved infile.
        //        MODULESYM * moduleUnres = GetUnresolvedModule(scopeSource, tkAssemblyRef);
        //        ASSERT(moduleUnres);
        //
        //        aid = moduleUnres->GetModuleID();
        //    }
        //
        //    ASSERT(aid != kaidUnresolved);
        //
        //    SetSymInCache(scopeSource, tkAssemblyRef, compiler()->getBSymmgr().GetRootNsAid(aid));
        //    return aid;
        //}

        //------------------------------------------------------------
        // IMPORTER.ResolveNamespace (1)
        //
        /// <summary>
        /// <para>Convert a fully qualified namespace string to a namespace symbol.
        /// An existing namespace symbol is returned if present, or a new namespace symbol is created.</para>
        /// <para>If the namespace string has errors, it returns NULL without reporting an error).</para>
        /// <para>Find or create NSSYMs for each namespace of argument namespaceText.
        /// Argument namespaceText cannot include class names.</para>
        /// </summary>
        /// <param name="namespaceText">String of a namespace.</param>
        /// <returns>NSSYM for the innermost namespace.</returns>
        //------------------------------------------------------------
        internal NSSYM ResolveNamespace(string namespaceText)
        {
            // start at the root namespace.
            NSSYM nsSym = this.Compiler.MainSymbolManager.RootNamespaceSym;
            if (String.IsNullOrEmpty(namespaceText))
            {
                return nsSym;
            }

            // Namespace can't start with . or contain ..
            if (namespaceText[0] == '.') return null;

            string[] nsNesting = namespaceText.Split('.');
            NSSYM nextNsSym = null;

            for (int i = 0; i < nsNesting.Length; ++i)
            {
                // Check for existing namespace.
                nextNsSym = Compiler.MainSymbolManager.LookupGlobalSymCore(
                    nsNesting[i], nsSym, SYMBMASK.NSSYM) as NSSYM;
                if (nextNsSym != null)
                {
                    nsSym = nextNsSym;
                }
                else
                {
                    nsSym = Compiler.MainSymbolManager.CreateNamespace(nsNesting[i], nsSym);
                }
            }
            return nsSym;
        }

        //------------------------------------------------------------
        // IMPORTER.ResolveNamespace (2)
        //
        /// <summary>
        /// Return NSAIDSYM for the inner most namespace in nsText with assembly ID.
        /// </summary>
        /// <param name="nsText">Qualified namespace name.</param>
        /// <param name="assemblyId"></param>
        /// <returns>Reference of the NSAIDSYM instance.</returns>
        //------------------------------------------------------------
        internal NSAIDSYM ResolveNamespace(string nsText, int assemblyId)
        {
            NSSYM nsSym = this.ResolveNamespace(nsText);
            // If nsText is null or empty, nsSym is RootNamespace.
            // If the first character of nsText is '.', nsSysm is null.
            if (nsSym == null) return null;
            return Compiler.MainSymbolManager.GetNsAid(nsSym, assemblyId);
        }

        //------------------------------------------------------------
        // IMPORTER.
        //------------------------------------------------------------
        //TYPESYM * ResolveTypeName(
        //    PCWSTR className,
        //    BAGSYM * bagPar,
        //    int aid,
        //    TypeArray * typeArgs,
        //    NameResOptionsEnum nro,
        //    mdTypeDef token = mdTypeDefNil);

        //------------------------------------------------------------
        // IMPORTER.
        //------------------------------------------------------------
        //UNRESAGGSYM *CreateUnresolvedAgg(NAME *name, BAGSYM *bagPar, int cvar);

        //------------------------------------------------------------
        // IMPORTER.ResolveNamespaceOfClassName
        //
        /// <summary>
        /// Divide the specified name of a class which is not nested
        /// and get the namespace part.
        /// Then create NSAIDSYM instance which the class is in.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assemblyId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NSAIDSYM ResolveNamespaceOfClassName(ref string name, int assemblyId)
        {
            string nsText = null;
            if (String.IsNullOrEmpty(name)) return null;

            int idx = name.LastIndexOf('.');
            if (idx > 0)
            {
                nsText = name.Substring(0, idx);
                name = name.Substring(idx + 1);
            }
            return ResolveNamespace(nsText, assemblyId);
        }

        //------------------------------------------------------------
        // IMPORTER.ResolveFullMetadataTypeName
        //------------------------------------------------------------
        internal TYPESYM ResolveFullMetadataTypeName(
            MODULESYM moduleSym,
            string className,
            out bool isInvalidSig)
        {
            throw new NotImplementedException("IMPORTER.ResolveFullMetadataTypeName");

            //isInvalidSig = false;

            //DWORD ichErrorLoc;
            //CComPtr<ITypeName> qtnType;
            //HRESULT hr = compiler().GetTypeNameFactory().ParseTypeName( className, &ichErrorLoc, &qtnType);
            //CheckHR(hr, moduleSym);
            //if (hr == S_FALSE) {
            //    DebugUtil.Assert(!qtnType);
            //    if (isInvalidSig)
            //        *isInvalidSig = true;
            //    return null;
            //}

            //DebugUtil.Assert(!!qtnType);
            //return ResolveTypeNameCore(moduleSym, qtnType);
        }

        //------------------------------------------------------------
        // IMPORTER::ResolveTypeRefOrSpec (sscli)
        //------------------------------------------------------------
        //TYPESYM * IMPORTER::ResolveTypeRefOrSpec(MODULESYM * mod, mdToken token,
        //    TypeArray * pClassTypeFormals, TypeArray * pMethTypeFormals)
        //{
        //    ImportScopeModule scope(this, mod);
        //
        //    if (TypeFromToken(token) != mdtTypeSpec) {
        //        // A typeref or typedef so the arity should be zero (if I understand when this called).
        //        TYPESYM * type = ResolveTypeRefOrDef(scope, token, BSYMMGR::EmptyTypeArray());
        //        ASSERT(!type || !type->isAGGTYPESYM() || type->asAGGTYPESYM()->typeArgsAll->size == 0);
        //        return type;
        //    }
        //
        //    ASSERT(scope.GetMetaImport());
        //    IMetaDataImport * metaimport = scope.GetMetaImport();
        //    PCCOR_SIGNATURE sig;
        //    ULONG sigsz;
        //    CheckHR(metaimport->GetTypeSpecFromToken(token, &sig, &sigsz), mod);
        //
        //    return ImportSigType(scope, &sig, sig + sigsz, kfisoNone, NULL, pClassTypeFormals, pMethTypeFormals);
        //}

        //------------------------------------------------------------
        // IMPORTER::ResolveTypeRefOrDef (sscli) (1)
        //------------------------------------------------------------
        //TYPESYM * IMPORTER::ResolveTypeRefOrDef(MODULESYM * mod, mdToken token, TypeArray * typeArgs)
        //{
        //    ImportScopeModule scope(this, mod);
        //    return ResolveTypeRefOrDef(scope, token, typeArgs);
        //}

        //------------------------------------------------------------
        // IMPORTER.ResolveTypeRefOrDef (sscli) (2)
        //------------------------------------------------------------
        //TYPESYM * IMPORTER::ResolveTypeRefOrDef(ImportScope & scope, mdToken token, TypeArray * typeArgs)
        //{
        //    switch (TypeFromToken(token)) {
        //    case mdtTypeRef:
        //        return ResolveTypeRef(scope, token, typeArgs);
        //
        //    case mdtTypeDef:
        //        return ResolveTypeDef(scope, token, typeArgs);
        //
        //    default:
        //        return NULL;
        //    }
        //}

        //------------------------------------------------------------
        // IMPORTER::ResolveTypeRef (sscli)
        //
        // Resolves a typeref to an actual class.
        // If the assembly ref is found, but the class is not then a warning is reported and NULL is returned.
        // If the assembly ref is not found then a fake class is created and
        // no error is reported to the user until they use the fake class
        //------------------------------------------------------------
        //TYPESYM * IMPORTER::ResolveTypeRef(ImportScope & scope, mdTypeRef token, TypeArray * typeArgs)
        //{
        //    ASSERT(scope.GetMetaImport());
        //    ASSERT(TypeFromToken(token) == mdtTypeRef);
        //
        //    if (token == mdTypeRefNil)
        //        return NULL;
        //
        //    TYPESYM * type;
        //    if (GetTypeFromCache(scope, token, typeArgs, &type))
        //        return type;
        //
        //    IMetaDataImport * metaimport = scope.GetMetaImport();
        //    WCHAR rgch[MAX_FULLNAME_SIZE];
        //    ULONG cch;
        //
        //    mdToken tkPar;
        //
        //    // Get the full name of the referenced type.
        //    CheckHR(metaimport->GetTypeRefProps(token, &tkPar, rgch, lengthof(rgch), &cch), scope);
        //    CheckTruncation(cch, lengthof(rgch), scope);
        //
        //    PCWSTR prgch; prgch = rgch;
        //    SYM * symPar; symPar = ResolveParentScope(scope, tkPar, &prgch);
        //
        //    if (!symPar)
        //        goto LDone;
        //
        //    ASSERT(symPar->isTYPESYM() || symPar->isNSAIDSYM());
        //    int aid;
        //
        //    // Resolve the type name.
        //    if (symPar->isAGGTYPESYM()) {
        //        aid = symPar->asAGGTYPESYM()->getAggregate()->GetModuleID();
        //        type = ResolveTypeName(prgch, symPar->asAGGTYPESYM()->getAggregate(), aid, typeArgs, NameResOptions::Normal);
        //    }
        //    else if (symPar->isNSAIDSYM()) {
        //        aid = symPar->asNSAIDSYM()->GetAid();
        //
        //        type = ResolveTypeName(prgch, symPar->asNSAIDSYM()->GetNS(), aid, typeArgs,
        //            NameResOptions::Normal
        //            );
        //    }
        //    else {
        //        VSFAIL("Why are we here?");
        //        goto LDone;
        //    }
        //
        //    ASSERT(aid != kaidUnresolved);
        //
        //    if (type && type->isAGGTYPESYM() && type->getAggregate()->IsUnresolved()) {
        //        // We have a typeref that couldn't be resolved to a class. Remember the typeref token so that
        //        // we can give a good error message if we ever attempt to use this type.
        //        // ResolveTypeName just fabricated the type for us.
        //
        //        // If the scope is the output file for ENC, then we shouldn't end up with an unresolved type,
        //        // otherwise the set of imports has changed and we shouldn't be doing an ENC!
        //        ASSERT(scope.GetModule());
        //
        //        UNRESAGGSYM * ura = type->getAggregate()->AsUnresolved();
        //
        //        if (aid < kaidMinModule) {
        //            MODULESYM * module = GetUnresolvedModule(scope, tkPar);
        //            aid = module->GetModuleID();
        //        }
        //
        //        if (!ura->moduleRef) {
        //            ura->module = compiler()->getBSymmgr().GetSymForAid(aid)->asMODULESYM();
        //            ura->moduleRef = scope.GetModule();
        //            ura->tokRef = token;
        //            if (!ura->moduleErr) {
        //                ura->moduleErr = ura->moduleRef;
        //                ura->tokErr = ura->tokRef;
        //            }
        //        }
        //    }
        //
        //    if (!type) {
        //        // The assembly reference (or outer type) was succesfully resolved
        //        // But this type couldn't be found.  Report a warning to the user
        //        // indicating possibly corrupt metadata
        //        if (symPar->isAGGTYPESYM()) {
        //            compiler()->Error(scope, WRN_MissingTypeNested, rgch, symPar);
        //        }
        //        else if (aid == kaidThisAssembly) {
        //            compiler()->Error(scope, WRN_MissingTypeInSource, rgch);
        //        }
        //        else {
        //            INFILESYM * infile = compiler()->getBSymmgr().GetInfileForAid(aid);
        //            ASSERT(infile->GetAssemblyID() != kaidUnresolved);
        //            compiler()->Error(scope, WRN_MissingTypeInAssembly, rgch, infile);
        //        }
        //    }
        //
        //LDone:
        //    if (type && type->isAGGTYPESYM())
        //        SetSymInCache(scope, token, type->asAGGTYPESYM()->getAggregate());
        //    else
        //        SetSymInCache(scope, token, type);
        //
        //    return type;
        //}

        //------------------------------------------------------------
        // IMPORTER::ResolveTypeDef (sscli)
        //
        // This checks the cache for a type with the given token.
        // If the type is not found in the cache, it attempts to load the type.
        // If the newly loaded type has any nested types or is itself nested,
        // we force loading all the types in the same namespace and module.
        //------------------------------------------------------------
        //TYPESYM * IMPORTER::ResolveTypeDef(ImportScope & scope, mdTypeDef token, TypeArray * typeArgs)
        //{
        //    ASSERT(!fLoadingTypes);
        //    ASSERT(scope.GetMetaImport());
        //    ASSERT(TypeFromToken(token) == mdtTypeDef);
        //
        //    if (token == mdTypeDefNil)
        //        return NULL;
        //
        //    TYPESYM * type;
        //    if (GetTypeFromCache(scope, token, typeArgs, &type))
        //        return type; // NOTE: type may be NULL
        //
        //    // The module will be NULL if the ImportScope is an Enc scope, in which case
        //    // the ImportOneType isn't necessary.
        //    MODULESYM * mod = scope.GetModule();
        //    if (!mod)
        //        return NULL;
        //
        //    if (fLoadingTypes) {
        //        VSFAIL("Shouldn't be recursing in loading types!");
        //        return NULL;
        //    }
        //
        //    // First load this AGGSYM (and its outer types).
        //    fLoadingTypes = true;
        //    AGGSYM * agg = ImportOneType(mod, token);
        //
        //    if (!agg) {
        //        ASSERT(fLoadingTypes);
        //        fLoadingTypes = false;
        //        return NULL;
        //    }
        //
        //    // Now get the outermost agg.
        //    while (agg->isNested())
        //        agg = agg->GetOuterAgg();
        //
        //    // The token of agg is the root token of any nested types.
        //    // See if we have any to import.
        //
        //    mdToken tokRoot = agg->tokenImport;
        //    ASSERT(RidFromToken(tokRoot) != 0);
        //
        //    int imtiMin = 0;
        //    int imtiLim = mod->cmti;
        //
        //    while (imtiMin < imtiLim) {
        //        int imtiMid = (imtiMin + imtiLim) / 2;
        //        if (mod->prgmti[imtiMid].tokRoot < tokRoot)
        //            imtiMin = imtiMid + 1;
        //        else
        //            imtiLim = imtiMid;
        //    }
        //    ASSERT(imtiMin == imtiLim);
        //
        //    for ( ; imtiLim < mod->cmti && mod->prgmti[imtiLim].tokRoot == tokRoot; imtiLim++) {
        //        ImportOneType(mod, mod->prgmti[imtiLim].tok);
        //    }
        //
        //    memmove(mod->prgmti + imtiMin, mod->prgmti + imtiLim, (mod->cmti - imtiLim) * sizeof(ModTypeInfo));
        //    mod->cmti -= (imtiLim - imtiMin);
        //
        //    ASSERT(fLoadingTypes);
        //    fLoadingTypes = false;
        //
        //    // Now we should be able to fetch from the cache.
        //    if (GetTypeFromCache(scope, token, typeArgs, &type))
        //        return type; // NOTE: type may be NULL
        //    VSFAIL("Why isn't the result in the cache now?");
        //    return NULL;
        //}

        //------------------------------------------------------------
        // IMPORTER.SelectTypeArguments
        //
        /// <summary></summary>
        /// <param name="infileSym"></param>
        /// <param name="requiredParams"></param>
        /// <param name="classTypeArgs"></param>
        /// <param name="methodTypeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray SelectTypeArguments(
            INFILESYM infileSym,
            Type[] requiredParams,
            TypeArray classTypeArgs,
            TypeArray methodTypeArgs)
        {
            if (requiredParams == null || requiredParams.Length == 0)
            {
                return BSYMMGR.EmptyTypeArray;
            }

            TypeArray reqiuredTypeArray = new TypeArray();
            int classTvCount = TypeArray.Size(classTypeArgs);
            int methodTvCount = TypeArray.Size(methodTypeArgs);

            for (int i = 0; i < requiredParams.Length; ++i)
            {
                Type reqParam = requiredParams[i];
                TYPESYM typeSym;

                if (reqParam.IsGenericParameter)
                {
                    typeSym = null;
                    int index = reqParam.GenericParameterPosition;
                    if (reqParam.DeclaringMethod != null)
                    {
                        if (0 <= index && index < methodTvCount)
                        {
                            typeSym = methodTypeArgs[index];
                        }
                        else
                        {
                            typeSym = Compiler.MainSymbolManager.GetStdClsTypeVar(index);
                        }
                    }
                    else
                    {
                        if (0 <= index && index < classTvCount)
                        {
                            typeSym = classTypeArgs[index];
                        }
                        else
                        {
                            typeSym = Compiler.MainSymbolManager.GetStdClsTypeVar(index);
                        }
                    }
                }
                else
                {
                    typeSym = ResolveType(
                        infileSym,
                        reqParam,
                        classTypeArgs,
                        methodTypeArgs);
                }

                reqiuredTypeArray.Add(typeSym);
            }
            return Compiler.MainSymbolManager.AllocParams(reqiuredTypeArray);
        }

        //------------------------------------------------------------
        // IMPORTER.GetTypeArrayFromSystemTypes
        //
        /// <summary></summary>
        /// <param name="infileSym"></param>
        /// <param name="systemTypes"></param>
        /// <param name="allTypeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray GetTypeArrayFromSystemTypes(
            INFILESYM infileSym,
            Type[] systemTypes,
            TypeArray classTypeArgs,
            TypeArray methodTypeArgs)
        {
            TypeArray typeArray = new TypeArray();
            for (int i = 0; i < systemTypes.Length; ++i)
            {
                typeArray.Add(ResolveType(
                    infileSym,
                    systemTypes[i],
                    classTypeArgs,
                    methodTypeArgs));
            }
            return this.compiler.MainSymbolManager.AllocParams(typeArray);
        }

        //------------------------------------------------------------
        // IMPORTER.ResolveType
        //
        /// <summary>
        /// <para>Use this instead of the method below:
        /// ResolveTypeRefOrSpec, ResolveTypeRefOrDef,ResolveTypeRef, ResolveTypeDef (sscli)</para>
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="typeToResolve"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM ResolveType(
            INFILESYM infileSym,
            System.Type typeToResolve,
            TypeArray classTypeArgs,
            TypeArray methodTypeArgs)
        {
            DebugUtil.Assert(!this.loadingTypes);
            if (typeToResolve == null)
            {
                return null;
            }

            TYPESYM typeSym = null;

            //--------------------------------------------------------
            // void
            //--------------------------------------------------------
            if (typeToResolve == typeof(void))
            {
                return compiler.MainSymbolManager.VoidSym;
            }

            //--------------------------------------------------------
            // Generic Parameter
            //
            // Generic parameter types should be imported
            // when aggregate types or mehods were imported.
            //--------------------------------------------------------
            if (typeToResolve.IsGenericParameter)
            {
                if (GetTypeSymFromCache(typeToResolve, null, out typeSym) &&
                    typeSym != null)
                {
                    return typeSym;
                }

                typeSym = null;
                int pos = typeToResolve.GenericParameterPosition;
                if (typeToResolve.DeclaringMethod != null)
                {
                    if (methodTypeArgs != null && pos < methodTypeArgs.Count)
                    {
                        typeSym = methodTypeArgs[pos];
                    }
                }
                else
                {
                    if (classTypeArgs != null && pos < classTypeArgs.Count)
                    {
                        typeSym = classTypeArgs[pos];
                    }
                }
                DebugUtil.Assert(
                    typeSym != null &&
                    typeSym.Name == typeToResolve.Name);
                return typeSym;
            }

            //--------------------------------------------------------
            // Array Type
            //--------------------------------------------------------
            if (typeToResolve.IsArray)
            {
                if (GetTypeSymFromCache(typeToResolve, null, out typeSym))
                {
                    return typeSym;
                }

                TYPESYM elementTypeSym = ResolveType(
                    infileSym,
                    typeToResolve.GetElementType(),
                    classTypeArgs,
                    methodTypeArgs);
                DebugUtil.Assert(elementTypeSym != null);

                return this.compiler.MainSymbolManager.GetArray(
                    elementTypeSym,
                    typeToResolve.GetArrayRank(),
                    typeToResolve);
            }

            //--------------------------------------------------------
            // ByRef Type
            //--------------------------------------------------------
            if (typeToResolve.IsByRef)
            {
                if (GetTypeSymFromCache(typeToResolve, null, out typeSym))
                {
                    return typeSym;
                }

                TYPESYM elementTypeSym = ResolveType(
                    infileSym,
                    typeToResolve.GetElementType(),
                    classTypeArgs,
                    methodTypeArgs);
                DebugUtil.Assert(elementTypeSym != null);

                return this.compiler.MainSymbolManager.GetParamModifier(elementTypeSym, false);
            }

            //--------------------------------------------------------
            // Pointer Type
            //--------------------------------------------------------
            if (typeToResolve.IsPointer)
            {
                if (GetTypeSymFromCache(typeToResolve, null, out typeSym))
                {
                    return typeSym;
                }

                TYPESYM elementTypeSym = ResolveType(
                    infileSym,
                    typeToResolve.GetElementType(),
                    classTypeArgs,
                    methodTypeArgs);
                DebugUtil.Assert(elementTypeSym != null);

                return this.compiler.MainSymbolManager.GetPtrType(elementTypeSym);
            }

            //--------------------------------------------------------
            // Otherwise
            //--------------------------------------------------------
            Type[] genericArgTypes = null;
            TypeArray typeArgs = null;

            if (typeToResolve.IsGenericType)
            {
                genericArgTypes = typeToResolve.GetGenericArguments();
                typeArgs = SelectTypeArguments(
                    infileSym,
                    genericArgTypes,
                    classTypeArgs,
                    methodTypeArgs);

                if (!typeToResolve.IsGenericTypeDefinition)
                {
                    typeToResolve = typeToResolve.GetGenericTypeDefinition();
                }
            }
            else
            {
                typeArgs = BSYMMGR.EmptyTypeArray;
            }

            //--------------------------------------------------------
            // If found in the cache, return it.
            //--------------------------------------------------------
            if (GetTypeSymFromCache(typeToResolve, typeArgs, out typeSym))
            {
                return typeSym;
            }

            //--------------------------------------------------------
            // If not found, import its type definition.
            //--------------------------------------------------------
            this.loadingTypes = true;
            AGGSYM aggSym = ImportOneType(infileSym, typeToResolve) as AGGSYM;

            if (aggSym == null)
            {
                DebugUtil.Assert(this.loadingTypes);
                this.loadingTypes = false;
                return null;
            }

            DebugUtil.Assert(this.loadingTypes);
            this.loadingTypes = false;

            //--------------------------------------------------------
            // Nullable
            //--------------------------------------------------------
            if (aggSym == this.compiler.MainSymbolManager.GetNullable())
            {
                DebugUtil.Assert(genericArgTypes.Length == 1);
                TYPESYM baseTypeSym = ResolveType(
                    infileSym,
                    genericArgTypes[0],
                    classTypeArgs,
                    methodTypeArgs);
                return compiler.MainSymbolManager.GetNubType(baseTypeSym);
            }

            //--------------------------------------------------------
            // Search in the cache again.
            //--------------------------------------------------------
            if (GetTypeSymFromCache(typeToResolve, typeArgs, out typeSym))
            {
                return typeSym; // NOTE: typeSym may be null
            }
            DebugUtil.VsFail("Why isn't the result in the cache now?");
            return null;
        }

        //------------------------------------------------------------
        // IMPORTER.ResolveParentScope
        //
        /// <summary>
        /// Always returns either a TYPESYM or an NSAIDSYM.
        /// In theory an ERRORSYM could be returned, but we should never produce meta-data resulting in one.
        /// </summary>
        //------------------------------------------------------------
        internal SYM ResolveParentScope()
        {
            DebugUtil.Assert(false, "ResolveParentScope is not implemented.");
            return null;
        }
        //------------------------------------------------------------
        // IMPORTER::ResolveParentScope (sscli)
        //------------------------------------------------------------
        //SYM * ResolveParentScope(ImportScope & scope, mdToken tkPar, PCWSTR * ppsz)
        //{
        //    int aid;
        //
        //    switch (TypeFromToken(tkPar)) {
        //    default:
        //        return NULL;
        //
        //    case mdtTypeDef:
        //        VSFAIL("Why is a type def a parent scope of a type ref?");
        //        return NULL;
        //
        //    case mdtTypeRef:
        //        // Nested class. Get the parent class from the resolution scope.
        //        // NOTE: we don't know the arity of the parent here so can't pass it through!
        //        // The arity annotation is the best we can hope for.
        //        return ResolveTypeRef(scope, tkPar, NULL);
        //
        //    case mdtAssemblyRef:
        //        aid = MapAssemblyRefToAid(scope, tkPar);
        //        break;
        //
        //    case mdtAssembly:
        //        aid = scope.GetAssemblyID();
        //        break;
        //
        //    case mdtModule:
        //        aid = scope.GetModuleID();
        //        break;
        //
        //    case mdtModuleRef:
        //        aid = MapModuleRefToAid(scope, tkPar);
        //        if (aid == kaidUnresolved)
        //            return NULL;
        //        break;
        //    }
        //
        //    return ResolveNamespaceOfClassName(ppsz, aid);
        //
        //}

        // This never causes types to be loaded.

        //------------------------------------------------------------
        // IMPORTER.FindAggName
        //
        /// <summary>
        /// Looks for a type that is already loaded.
        /// The aid MUST be a module id.
        /// The name should be the imported name (not including arity decoration).
        /// This is used by ImportOneType to see if the type has already been loaded.
        /// </summary>
        //------------------------------------------------------------
        internal AGGSYM FindAggName(string name, BAGSYM parentBagSym, int assemblyId, Type ty)
        {
            for (SYM sym = Compiler.MainSymbolManager.LookupGlobalSymCore(name, parentBagSym, SYMBMASK.AGGSYM);
                sym != null;
                sym = BSYMMGR.LookupNextSym(sym, parentBagSym, SYMBMASK.AGGSYM))
            {
                AGGSYM aggSym = sym as AGGSYM;
                if (aggSym != null &&
                    //aggSym.Type!=null && // ?
                    aggSym.Type == ty &&
                    aggSym.InAlias(assemblyId))
                {
                    return aggSym;
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // IMPORTER.ComputeArityFromName
        //
        /// <summary>
        /// <para>Get the count of type parameters from the name.</para>
        /// <para>argument className cannot be FullName.</para>
        /// </summary>
        /// <param name="className">Name with arity.</param>
        /// <param name="startOfArity">Index of '!' or '`'. If not found, set -1.</param>
        /// <returns>Count of the type parameters.</returns>
        //------------------------------------------------------------
        static internal int ComputeArityFromName(string className, out int startOfArity)
        {
            startOfArity = -1;
            if (className == null || className.Length <= 2)
            {
                return 0;
            }

            char c;
            int start = 0;
            int last = className.Length - 2;

            for (start = 0; (c = className[start]) != '!' && c != '`'; ++start)
            {
                if (start >= last)
                {
                    return 0;
                }
            }

            int arity = 0;
            if (System.Int32.TryParse(className.Substring(start + 1), out arity))
            {
                startOfArity = start;
                return arity;
            }
            // System.Int32.TryParse method does not throw any exception.
            return 0;
        }

        //------------------------------------------------------------
        // IMPORTER.CheckFriendAssemblyName
        //------------------------------------------------------------
        //    bool IsValidAssemblyName(PCWSTR szAsmName);

        //------------------------------------------------------------
        // IMPORTER.CheckFriendAssemblyName
        //------------------------------------------------------------
        //bool CheckFriendAssemblyName (BASENODE *tree, PCWSTR szAsmName, OUTFILESYM *context);
        internal bool CheckFriendAssemblyName(BASENODE tree, string szAsmName, OUTFILESYM context)
        {
            throw new NotImplementedException("IMPORTER.CheckFriendAssemblyName");
        }

        //------------------------------------------------------------
        // IMPORTER.CheckResult (1)
        //
        /// <summary>
        /// <para>(CheckHR in sscli.)</para>
        /// </summary>
        /// <param name="br"></param>
        /// <param name="infileSym"></param>
        //------------------------------------------------------------
        internal void CheckResult(bool br, INFILESYM infileSym)
        {
            DebugUtil.Assert(infileSym != null);
            if (!br)
            {
                MetadataFailure(infileSym.GetFileName(compiler.OptionManager.FullPaths));
            }
            //SetErrorInfo(0, NULL);
        }

        //------------------------------------------------------------
        // IMPORTER.CheckResult (2)
        //
        /// <summary>
        /// <para>(CheckHR in sscli.)</para>
        /// </summary>
        /// <param name="br"></param>
        /// <param name="modSym"></param>
        //------------------------------------------------------------
        internal void CheckResult(bool br, MODULESYM modSym)
        {
            DebugUtil.Assert(modSym != null);
            if (!br)
            {
                MetadataFailure(
                    modSym.GetInputFile().GetFileName(
                        compiler.OptionManager.FullPaths));
            }
            //SetErrorInfo(0, NULL);
        }

        //------------------------------------------------------------
        // IMPORTER.CheckResult (3)
        //
        /// <summary>
        /// <para>(CheckHR in sscli.)</para>
        /// </summary>
        /// <param name="br"></param>
        /// <param name="scopeSym"></param>
        //------------------------------------------------------------
        internal void CheckResult(bool br, ImportScopeModule scopeSym)
        {
            if (!br)
            {
                MetadataFailure(
                    scopeSym.GetFileName(compiler.OptionManager.FullPaths));
            }
            //SetErrorInfo(0, NULL);
        }

        //------------------------------------------------------------
        // IMPORTER.CheckResult (4)
        //
        /// <summary>
        /// <para>(CheckHR in sscli.)</para>
        /// </summary>
        /// <param name="br"></param>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void CheckResult(bool br, AGGSYM aggSym)
        {
            if (!br)
            {
                MetadataFailure(
                    aggSym.GetModule().GetInputFile().GetFileName(
                        compiler.OptionManager.FullPaths));
            }
            //SetErrorInfo(0, NULL);
        }

        //private:

        //------------------------------------------------------------
        // IMPORTER.CheckResult (5)
        //
        /// <summary>
        /// <para>(CheckHR in sscli.)</para>
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="br"></param>
        /// <param name="infileSym"></param>
        //------------------------------------------------------------
        private void CheckResult(bool br, CSCERRID errorId, INFILESYM infileSym)
        {
            DebugUtil.Assert(infileSym != null);
            if (!br)
            {
                MetadataFailure(
                    errorId,
                    infileSym.GetFileName(compiler.OptionManager.FullPaths));
            }
            //SetErrorInfo(0, NULL);
        }

        //------------------------------------------------------------
        // IMPORTER.CheckResult (6)
        //
        /// <summary>
        /// <para>(CheckHR in sscli.)</para>
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="br"></param>
        /// <param name="modSym"></param>
        //------------------------------------------------------------
        private void CheckResult(CSCERRID errorId, bool br, MODULESYM modSym)
        {
            DebugUtil.Assert(modSym != null);
            if (!br)
            {
                MetadataFailure(
                    errorId,
                    modSym.GetInputFile().GetFileName(
                        compiler.OptionManager.FullPaths));
            }
            //SetErrorInfo(0, NULL);
        }

        //------------------------------------------------------------
        // IMPORTER.MetadataFailure (1)
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        //------------------------------------------------------------
        private void MetadataFailure(string fileName)
        {
            MetadataFailure(CSCERRID.FTL_MetadataImportFailure, fileName);
        }

        //------------------------------------------------------------
        // IMPORTER.MetadataFailure (2)
        //
        /// <summary></summary>
        /// <param name="errorId"></param>
        /// <param name="fileName"></param>
        //------------------------------------------------------------
        private void MetadataFailure(CSCERRID errorId, string fileName)
        {
            Compiler.Error(errorId, new ErrArg(fileName));
        }

        //    void BogusMetadataFailure(PCWSTR pszFile);
        //    void BogusMetadataFailure(ImportScope & scope) { BogusMetadataFailure(scope.GetFileName()); }
        //    void CheckTruncation(int requiredSize, int bufferLength, INFILESYM * infile);
        //    void CheckTruncation(int requiredSize, int bufferLength, MODULESYM * scope);
        //    void CheckTruncation(int requiredSize, int bufferLength, ImportScope & scope);
        //    IMetaDataDispenser * GetDispenser();

        //------------------------------------------------------------
        // IMPORTER.ConvertAccessLevel (1) FieldAttributes
        //
        /// <summary>
        /// <para>From a flags field, return the access level</para>
        /// <para>For fields.</para>
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="infileSym"></param>
        /// <param name="dontHide"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ACCESS ConvertAccessLevel(FieldAttributes flags, INFILESYM infileSym, bool dontHide)
        {
            //DebugUtil.Assert((int)fdPublic          == (int)mdPublic);
            //DebugUtil.Assert((int)fdPrivate         == (int)mdPrivate);
            //DebugUtil.Assert((int)fdFamily          == (int)mdFamily);
            //DebugUtil.Assert((int)fdAssembly        == (int)mdAssem);
            //DebugUtil.Assert((int)fdFamANDAssem     == (int)mdFamANDAssem);
            //DebugUtil.Assert((int)fdFamORAssem      == (int)mdFamORAssem);
            //DebugUtil.Assert((int)fdFieldAccessMask == (int)mdMemberAccessMask);


            //flags &= mdMemberAccessMask;
            flags &= FieldAttributes.FieldAccessMask;

            switch (flags)
            {
                case FieldAttributes.Private:
                    return ACCESS.PRIVATE;

                case FieldAttributes.Family:
                    return ACCESS.PROTECTED;

                case FieldAttributes.Public:
                    return ACCESS.PUBLIC;

                case FieldAttributes.Assembly:
                    // Treat as internal if we may have access to it. Otherwise treat it as private.
                    // This is so we will never allow access to it - even if it's parent is the base
                    // of an accessible type.
                    if (dontHide ||
                        infileSym.GetAssemblyID() == Kaid.ThisAssembly ||
                        infileSym.InternalsVisibleTo(Kaid.ThisAssembly))
                    {
                        return ACCESS.INTERNAL;
                    }
                    return ACCESS.PRIVATE;

                case FieldAttributes.FamANDAssem:
                    // We don't support this directly.
                    // Treat as protected if we may have access to it. Otherwise treat it as private.
                    // This is so we will never allow access to it - even if it's parent is the base
                    // of an accessible type.
                    if (infileSym.GetAssemblyID() == Kaid.ThisAssembly ||
                        infileSym.InternalsVisibleTo(Kaid.ThisAssembly))
                    {
                        return ACCESS.PROTECTED;
                    }
                    if (dontHide)
                    {
                        return ACCESS.INTERNAL;
                    }
                    return ACCESS.PRIVATE;

                case FieldAttributes.FamORAssem:
                    // Treat as internal protected if it's in this assembly or in one that gave us
                    // friend rights. Otherwise treat it as protected since we can't see the internal
                    // part and want overrides to be protected.
                    if (infileSym.GetAssemblyID() == Kaid.ThisAssembly ||
                        infileSym.InternalsVisibleTo(Kaid.ThisAssembly))
                    {
                        return ACCESS.INTERNALPROTECTED;
                    }
                    return ACCESS.PROTECTED;

                default:
                    return ACCESS.PRIVATE;
            }
        }

        //------------------------------------------------------------
        // IMPORTER.ConvertAccessLevel (2) MethodAttributes
        //
        /// <summary>
        /// <para>From a flags field, return the access level</para>
        /// <para>For fields.</para>
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="infileSym"></param>
        /// <param name="dontHide"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ACCESS ConvertAccessLevel(MethodAttributes flags, INFILESYM infileSym, bool dontHide)
        {
            //flags &= mdMemberAccessMask;
            flags &= MethodAttributes.MemberAccessMask;

            switch (flags)
            {
                case MethodAttributes.Private:
                    return ACCESS.PRIVATE;

                case MethodAttributes.Family:
                    return ACCESS.PROTECTED;

                case MethodAttributes.Public:
                    return ACCESS.PUBLIC;

                case MethodAttributes.Assembly:
                    // Treat as internal if we may have access to it. Otherwise treat it as private.
                    // This is so we will never allow access to it - even if it's parent is the base
                    // of an accessible type.
                    if (dontHide ||
                        infileSym.GetAssemblyID() == Kaid.ThisAssembly ||
                        infileSym.InternalsVisibleTo(Kaid.ThisAssembly))
                    {
                        return ACCESS.INTERNAL;
                    }
                    return ACCESS.PRIVATE;

                case MethodAttributes.FamANDAssem:
                    // We don't support this directly.
                    // Treat as protected if we may have access to it. Otherwise treat it as private.
                    // This is so we will never allow access to it - even if it's parent is the base
                    // of an accessible type.
                    if (infileSym.GetAssemblyID() == Kaid.ThisAssembly ||
                        infileSym.InternalsVisibleTo(Kaid.ThisAssembly))
                    {
                        return ACCESS.PROTECTED;
                    }
                    if (dontHide)
                    {
                        return ACCESS.INTERNAL;
                    }
                    return ACCESS.PRIVATE;

                case MethodAttributes.FamORAssem:
                    // Treat as internal protected if it's in this assembly or in one that gave us
                    // friend rights. Otherwise treat it as protected since we can't see the internal
                    // part and want overrides to be protected.
                    if (infileSym.GetAssemblyID() == Kaid.ThisAssembly ||
                        infileSym.InternalsVisibleTo(Kaid.ThisAssembly))
                    {
                        return ACCESS.INTERNALPROTECTED;
                    }
                    return ACCESS.PROTECTED;

                default:
                    return ACCESS.PRIVATE;
            }
        }

        //    bool ImportConstant(CONSTVAL * constVal, ULONG cch, PTYPESYM valType, DWORD constType, const void * constValue);

        //------------------------------------------------------------
        // IMPORTER.ImportInterface
        //
        /// <summary>
        /// <para>Imports an interface declared on a class or interface.</para>
        /// </summary>
        //------------------------------------------------------------
        internal AGGTYPESYM ImportInterface(INFILESYM inFileSym, Type interfaceImpl, AGGSYM derivedAggSym)
        {
            TYPESYM sym = ResolveType(
                inFileSym,
                interfaceImpl,
                derivedAggSym.AllTypeVariables,
                null);

            return (sym as AGGTYPESYM);
        }
        //------------------------------------------------------------
        // IMPORTER::ImportInterface (sscli)
        //------------------------------------------------------------
        //PAGGTYPESYM IMPORTER::ImportInterface(MODULESYM *scope, mdInterfaceImpl tokenIntf, PAGGSYM symDerived)
        //{
        //    ASSERT(scope->GetMetaImport(compiler()));
        //
        //    if (TypeFromToken(tokenIntf) != mdtInterfaceImpl || tokenIntf == mdInterfaceImplNil)
        //        return NULL;
        //
        //    IMetaDataImport * metaimport = scope->GetMetaImport(compiler());
        //    mdToken tokenInterface;
        //
        //    // Get typeref and flags.
        //    CheckHR(metaimport->GetInterfaceImplProps(tokenIntf, NULL, &tokenInterface), scope);
        //
        //    if (IsNilToken(tokenInterface))
        //        return NULL;
        //
        //    return ResolveBaseRef(scope, tokenInterface, symDerived, symDerived->IsInterface());
        //}

        //------------------------------------------------------------
        // IMPORTER.ImportField
        //
        /// <summary>
        /// Import a fielddef and create a corresponding MEMBVARSYM symbol.
        /// If we can't import the field
        /// because it has attributes that we don't support and can't safely ignore,
        /// we set the "isBogus" flag on the field.
        /// If we ever try to use that field, we'll give an error
        /// (but if we don't use the field, the user never knows or cares).
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="parentAggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="fieldInfo"></param>
        //------------------------------------------------------------
        internal MEMBVARSYM ImportField(
            INFILESYM infileSym,
            AGGSYM parentAggSym,
            AGGDECLSYM aggDeclSym,
            FieldInfo fieldInfo)
        {
            DebugUtil.Assert(fieldInfo != null);
            DebugUtil.Assert(!parentAggSym.IsPrivateMetadata || parentAggSym.IsStruct || parentAggSym.IsEnum);

            SYM sym = Compiler.MainSymbolManager.InfoToSymbolTable.GetSymFromInfo(fieldInfo);
            if (sym != null)
            {
                return sym as MEMBVARSYM;
            }

            MEMBVARSYM fieldSym;
            TYPESYM typeSym;
            bool isVolatile = false;

            //--------------------------------------------------
            // Check the access.
            //--------------------------------------------------
            ACCESS access = ConvertAccessLevel(fieldInfo.Attributes, infileSym, false);

            if (parentAggSym.IsPrivateMetadata)
            {
                // We treat anything in a private type as private.
                access = ACCESS.PRIVATE;
            }

            // ConvertAccessLevel nukes things we can't see by returning ACCESS.PRIVATE.
            bool isInaccess = (access < ACCESS.INTERNAL);

            // Don't import inaccessible fields except in structs, and then only look at them enough to see
            // if they impact whether the type is managed.
            if (isInaccess && !parentAggSym.IsStruct)
            {
                return null;
            }

            //--------------------------------------------------
            // Get name.
            //--------------------------------------------------
            Compiler.NameManager.AddString(fieldInfo.Name);

            //--------------------------------------------------
            // Import the type of the field.
            //--------------------------------------------------
            typeSym = ImportFieldType(infileSym, fieldInfo, out isVolatile, parentAggSym.AllTypeVariables);

            //--------------------------------------------------
            // isInaccess
            //--------------------------------------------------
            if (isInaccess)
            {
                DebugUtil.Assert(parentAggSym.IsStruct);

                // We need any struct fields (other than standard known ones) for recursion checking.
                // We need instance struct fields for checking managed/unmanaged.

                if (typeSym.IsAGGTYPESYM)
                {
                    AGGSYM tempAggSym = typeSym.GetAggregate();

                    if (!tempAggSym.IsValueType)
                    {
                        // If it's not a value type, we only care if it's an instance field.
                        if (!fieldInfo.IsStatic)
                        {
                            parentAggSym.IsManagedStruct = true;
                        }
                        return null;
                    }

                    // Convert enums to their underlying type.
                    if (tempAggSym.IsEnum && tempAggSym.UnderlyingTypeSym != null)
                    {
                        tempAggSym = tempAggSym.UnderlyingTypeSym.GetAggregate();
                        DebugUtil.Assert(tempAggSym.IsStruct);
                    }
                    if (parentAggSym.IsPredefinedType && parentAggSym.PredefinedTypeID < PREDEFTYPE.OBJECT)
                    {
                        // Known to be unmanaged and non-recursive.
                        return null;
                    }
                }
                else if (typeSym.IsPTRSYM)  // if (typeSym.IsAGGTYPESYM)
                {
                    // Pointers are unmanaged.
                    return null;
                }
                else
                {
                    if (!fieldInfo.IsStatic)
                    {
                        parentAggSym.IsManagedStruct = true;
                    }
                    // We need to keep TYVARSYM-valued fields for struct layout errors.
                    if (!typeSym.IsTYVARSYM)
                    {
                        return null;
                    }
                }
            } // if (isInaccess)

            //--------------------------------------------------
            // Enums are a bit special. Non-static fields serve only to record the
            // underlying integral type, and are otherwise ignored. Static fields are
            // enumerators and must be of the enum type. (We change other integral ones to the
            // enum type because it's probably what the emitting compiler meant.)
            //--------------------------------------------------
            if (parentAggSym.IsEnum)
            {
                if (!fieldInfo.IsStatic)
                {
                    // NOTE: The underlying type is set in the resolve inheritance phase now
                    DebugUtil.Assert(
                        typeSym == null ||
                        !typeSym.IsNumericType() ||
                        typeSym.FundamentalType() > FUNDTYPE.LASTINTEGRAL ||
                        parentAggSym.UnderlyingTypeSym == (typeSym as AGGTYPESYM));
                    return null;
                }

                if (typeSym != parentAggSym.GetThisType())
                {
                    if (typeSym == parentAggSym.UnderlyingTypeSym)
                    {
                        typeSym = parentAggSym.GetThisType();
                        // If it's the underlying type, assume it's meant to be the enum type.
                    }
                    else
                    {
                        typeSym = null;     // Bogus type.
                    }
                }
            } // if (parentAggSym.IsEnum)

            //--------------------------------------------------
            // Declare a field.
            // If we get a name conflict, just ignore the whole field,
            // since there's not much usefulness in report this to the user.
            //--------------------------------------------------

            // Check for conflict.
            //if (Compiler.MainSymbolManager.LookupAggMember(fieldInfo.Name, parentAggSym, SYMBMASK.ALL) != null)
            //{
            //    return;  // Already declared one.
            //}

            fieldSym = Compiler.MainSymbolManager.CreateMembVar(fieldInfo.Name, parentAggSym, aggDeclSym);
            //fieldSym.ImportedToken = tokenField;
            fieldSym.FieldInfo = fieldInfo;
            Compiler.MainSymbolManager.InfoToSymbolTable.SetSymForInfo(fieldInfo, fieldSym);

            // Record the type.
            if (typeSym == null)
            {
                fieldSym.SetBogus(true);
                typeSym = Compiler.MainSymbolManager.ErrorSym;
            }
            fieldSym.TypeSym = typeSym;

            //--------------------------------------------------
            // Import all the attributes we are interested in at compile time
            //--------------------------------------------------
            IMPORTED_CUSTOM_ATTRIBUTES attributes = new IMPORTED_CUSTOM_ATTRIBUTES();
            ImportCustomAttributes(infileSym, fieldInfo.GetCustomAttributes(true), attributes);

            fieldSym.SetDeprecated(
                attributes.IsDeprecated,
                attributes.IsDeprecatedError,
                attributes.DeprecatedString);

            if (attributes.HasCLSattribute)
            {
                fieldSym.HasCLSAttribute = true;
                fieldSym.IsCLS = attributes.IsCLS;
            }

            if (attributes.FixedBufferTypeSym != null)
            {
                bool notFixed = false;
                if (!typeSym.IsAGGTYPESYM)
                {
                    goto NOT_FIXED;
                }
                Compiler.EnsureState(typeSym, AggStateEnum.Prepared);
                if (parentAggSym.IsStruct &&
                    !typeSym.IsGenericInstance() &&
                    !fieldInfo.IsStatic &&
                    attributes.FixedBufferTypeSym.IsSimpleType() &&
                    !attributes.FixedBufferTypeSym.IsPredefType(PREDEFTYPE.BOOL) &&
                    !attributes.FixedBufferTypeSym.IsPredefType(PREDEFTYPE.DECIMAL))
                {
                    // Do some basic checking to make sure this type only has 1 field
                    string fixedElementName = Compiler.NameManager.GetPredefinedName(PREDEFNAME.FIXEDELEMENT);
                    MEMBVARSYM fixedFieldSym = null;

                    for (SYM memb = (typeSym.GetAggregate()).FirstChildSym;
                        memb != null;
                        memb = memb.NextSym)
                    {
                        switch (memb.Kind)
                        {
                            case SYMKIND.MEMBVARSYM:
                                if (fixedFieldSym != null || memb.Name != fixedElementName ||
                                    (memb as MEMBVARSYM).TypeSym != attributes.FixedBufferTypeSym)
                                {
                                    notFixed = true;
                                    goto NOT_FIXED;
                                }
                                fixedFieldSym = memb as MEMBVARSYM;
                                break;

                            case SYMKIND.AGGTYPESYM:
                                break;

                            default:
                                notFixed = true;
                                goto NOT_FIXED;
                        }
                    }

                    if (fixedFieldSym == null) goto NOT_FIXED;

                    DebugUtil.Assert(fixedFieldSym == Compiler.MainSymbolManager.LookupAggMember(
                        fixedElementName, typeSym.GetAggregate(), SYMBMASK.MEMBVARSYM));
                    fieldSym.TypeSym = Compiler.MainSymbolManager.GetPtrType(attributes.FixedBufferTypeSym);
                    fieldSym.FixedAggSym = typeSym.GetAggregate();
                    fieldSym.ConstVal.SetInt(attributes.FixedBufferElementCount);
                }
                else // if (!typeSym.IsAGGTYPESYM)
                {
                    notFixed = true;
                    goto NOT_FIXED;
                }

            NOT_FIXED:
                if (notFixed)
                {
                    fieldSym.SetBogus(true);
                }
            } // if (attributes.FixedBufferTypeSym != null)

            fieldSym.Access = access;
            fieldSym.IsStatic = fieldInfo.IsStatic;
            fieldSym.IsVolatile = isVolatile;

            if (parentAggSym.IsStatic && !fieldSym.IsStatic)
            {
                // The class is static but has a non-static member, so treat the member as bogus.
                fieldSym.SetBogus(true);
            }

            //--------------------------------------------------
            // const (1)
            //--------------------------------------------------
            if (fieldInfo.IsLiteral)
            {
                object constVal = fieldInfo.GetValue(null);
                if (constVal != null &&
                    constVal.GetType() != CTypeOf.System.Void &&
                    (!fieldSym.TypeSym.IsPredefType(PREDEFTYPE.DECIMAL)))
                {
                    // A compile time constant.
                    fieldSym.IsConst = true;
                    fieldSym.IsStatic = true;

                    if (!fieldSym.IsBogus)
                    {
                        // Try to import the value from COM+ metadata.
                        //if (!ImportConstant(&fieldSym.constVal, constLen, typeSym, constType, constValue))
                        //{
                        //    fieldSym.IsBogus=true;
                        //}
                        fieldSym.ConstVal = new CONSTVAL(constVal);
                    }
                }
            }
            else if (fieldInfo.IsInitOnly)
            {
                // A readonly field.
                fieldSym.IsReadOnly = true;
            }

            //--------------------------------------------------
            // const (2) decimal
            //
            // decimal literals are stored in a custom blob since they can't be represented MD directly
            //
            // In the case of "const decimal", FieldInfo.IsLeteral flag is not set.
            // Alternatively, IsStatic and IsInitOly flags are set.
            //--------------------------------------------------
            if (fieldInfo.IsInitOnly &&
                fieldInfo.IsStatic &&
                fieldSym.TypeSym.IsPredefType(PREDEFTYPE.DECIMAL))
            {
                if (attributes.HasDecimalLiteral)
                {
                    fieldSym.ConstVal = new CONSTVAL(attributes.DecimalLiteral);
                    fieldSym.IsConst = true;
                    fieldSym.IsReadOnly = false;
                }
            }

            if (parentAggSym.IsEnum && !fieldSym.IsConst)
            {
                // Enum members better be read-only constants.
                fieldSym.SetBogus(true);
            }

            return fieldSym;
        }

        //------------------------------------------------------------
        // IMPORTER.ImportMethod
        //
        /// <summary>
        /// <para>Import a methoddef and create a corresponding METHSYM symbol.
        /// If we can't import the method because it has attributes/types
        /// that we don't support and can't safely ignore, we set the "isBogus" flag on the method.
        /// If we ever try to use that method, we'll give an error
        /// (but if we don't use the method, the user never knows or cares).</para>
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="parentAggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="methodBase"></param>
        /// <returns><para>void in sscli.</para></returns>
        //------------------------------------------------------------
        internal METHSYM ImportMethod(
            INFILESYM infileSym,
            AGGSYM parentAggSym,
            AGGDECLSYM aggDeclSym,
            MethodBase methodBase)
        {
            DebugUtil.Assert(!parentAggSym.IsPrivateMetadata);
            if (methodBase == null)
            {
                return null;
            }

            SYM sym = Compiler.MainSymbolManager.InfoToSymbolTable.GetSymFromInfo(methodBase);
            if (sym != null)
            {
                return sym as METHSYM;
            }

            METHSYM methodSym = null;

            MethodInfo methodInfo = methodBase as MethodInfo;
            ConstructorInfo constructorInfo = null;
            if (methodInfo == null)
            {
                constructorInfo = methodBase as ConstructorInfo;
                if (constructorInfo == null)
                {
                    return null;
                }
            }
            bool IsConstructor = (constructorInfo != null);

            const bool defineParams = true;

            bool isAbstract = methodBase.IsAbstract;
            bool isVirtual = methodBase.IsVirtual;
            bool forceImport = parentAggSym.IsAbstract && (isAbstract || isVirtual);

            //--------------------------------------------------
            // Check the modifiers.
            //--------------------------------------------------
            ACCESS access = ConvertAccessLevel(methodBase.Attributes, infileSym, forceImport);

            // Interfaces with non-public methods are bogus.
            if (parentAggSym.IsInterface && access != ACCESS.PUBLIC)
            {
                parentAggSym.SetBogus(true);
            }

            // ConvertAccessLevel nukes things we can't see by returning ACCESS.PRIVATE.
            if (access < ACCESS.INTERNAL && !forceImport)
            {
                return null;
            }

            //
            if ((methodBase.Attributes & MethodAttributes.RTSpecialName) == 0 &&
                methodBase.Name.IndexOf('.') >= 0)
            {
                return null;
            }

            // Interfaces in C# do not contain static methods, but can in the CLR. If we see a static
            // method in an interface, we must be importing from not C# metadata, so just ignore that method.
            if (methodBase.IsStatic && parentAggSym.IsInterface)
            {
                return null;
            }

            Compiler.NameManager.AddString(methodBase.Name);

            //--------------------------------------------------
            // Declare a method.
            // Create a METHSYM instance and register to the symbol manager.
            //--------------------------------------------------
            methodSym = Compiler.MainSymbolManager.CreateMethod(methodBase.Name, parentAggSym, aggDeclSym);
            DebugUtil.Assert(methodSym != null);

            if (IsConstructor)
            {
                methodSym.ConstructorInfo = constructorInfo;
            }
            else
            {
                methodSym.MethodInfo = methodInfo;
            }

            Compiler.MainSymbolManager.InfoToSymbolTable.SetSymForInfo(methodBase, methodSym);

            //--------------------------------------------------
            // Import all the attributes we are interested in at compile time
            //--------------------------------------------------
            IMPORTED_CUSTOM_ATTRIBUTES attributes = new IMPORTED_CUSTOM_ATTRIBUTES();
            ImportCustomAttributes(infileSym, methodBase.GetCustomAttributes(true), attributes);

            // Set attributes of the method.
            methodSym.Access = access;

            methodSym.ConditionalSymbolNameList = attributes.ConditionalSymbols;
            methodSym.SetDeprecated(
                attributes.IsDeprecated,
                attributes.IsDeprecatedError,
                attributes.DeprecatedString);

            if (attributes.HasCLSattribute)
            {
                methodSym.HasCLSAttribute = true;
                methodSym.IsCLS = attributes.IsCLS;
            }
            methodSym.HasLinkDemand = attributes.HasLinkDemand;

            methodSym.IsStatic = methodBase.IsStatic;
            methodSym.IsAbstract = isAbstract;
            if ((methodBase.Attributes & MethodAttributes.RTSpecialName) != 0 &&
                (methodBase.Name == Compiler.NameManager.GetPredefinedName(
                    methodBase.IsStatic ? PREDEFNAME.STATCTOR : PREDEFNAME.CTOR)))
            {
                DebugUtil.Assert(IsConstructor);
                methodSym.MethodKind = MethodKindEnum.Ctor;
            }
            methodSym.IsVirtual = isVirtual && !methodSym.IsCtor && !methodBase.IsFinal;
            methodSym.IsMetadataVirtual = isVirtual;
            methodSym.IsOperator = false;
            methodSym.HideByName = !methodBase.IsHideBySig;

            if (parentAggSym.IsStatic && !methodSym.IsStatic)
            {
                // The class is static but has a non-static member, so treat the member as bogus.
                methodSym.SetBogus(true);
            }

            // we are importing an abstract method which
            // is not marked as virtual, currently code generation
            // keys on the is^Virtual property to set the dispatch type
            // on function calls.
            //
            // if this fires, find out who generated the metadata
            // and get them to fix their metadata generation

            DebugUtil.Assert(methodSym.IsVirtual || !methodSym.IsAbstract);
            methodSym.IsVirtual = (methodSym.IsVirtual || methodSym.IsAbstract);

            methodSym.IsVirtual = methodSym.IsVirtual && !methodSym.IsStatic;

            if (defineParams)
            {
                ImportMethodPropsWorker(infileSym, parentAggSym, methodBase, methodSym);
            }

            // convert special methods into operators.
            // If we don't recognize this as a C#-like operator then we just ignore the special
            // bit and make it a regular method

            if (methodBase.IsSpecialName &&
                (methodBase.Attributes & MethodAttributes.RTSpecialName) == 0)
            {
                if (methodBase.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPEXPLICITMN))
                {
                    methodSym.IsOperator = true;
                    methodSym.MethodKind = MethodKindEnum.ExplicitConv;

                    // Add it to the conversion list.
                    methodSym.NextConvertMethSym = parentAggSym.FirstConversionMethSym;
                    parentAggSym.FirstConversionMethSym = methodSym;

                    // The flag is set whenever this class or any of its bases has a conversion operator.
                    parentAggSym.HasConversion = true;
                }
                else if (methodBase.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPIMPLICITMN))
                {
                    methodSym.IsOperator = true;
                    methodSym.MethodKind = MethodKindEnum.ImplicitConv;

                    // Add it to the conversion list.
                    methodSym.NextConvertMethSym = parentAggSym.FirstConversionMethSym;
                    parentAggSym.FirstConversionMethSym = methodSym;

                    parentAggSym.HasConversion = true;
                }
                else
                {
                    OPERATOR opId = Compiler.ClsDeclRec.OperatorOfName(methodBase.Name);
                    if (OPERATOR.LAST != opId &&
                        OPERATOR.EXPLICIT != opId &&
                        OPERATOR.IMPLICIT != opId)
                    {
                        methodSym.IsOperator = true;

                        if (opId == OPERATOR.EQ &&
                            methodSym.ParameterTypes[0] == methodSym.ParameterTypes[1] &&
                            methodSym.ParameterTypes[0] == parentAggSym.GetThisType())
                        {
                            parentAggSym.HasSelfEquality = true;
                        }
                        else if (
                            opId == OPERATOR.NEQ &&
                            methodSym.ParameterTypes[0] == methodSym.ParameterTypes[1] &&
                            methodSym.ParameterTypes[0] == parentAggSym.GetThisType())
                        {
                            parentAggSym.HasSelfNonEquality = true;
                        }
                    }
                }
            }

            if (isVirtual &&
                (methodBase.Attributes & MethodAttributes.NewSlot) == 0 &&
                parentAggSym.BaseClassSym != null)
            {
                // NOTE: The isOverride bit is NOT valid until
                // AFTER the prepare stage.
                // for now just set to keep track of the !mdNewSlot bit
                // We will fix this up later in CLSDREC::setOverrideBits()
                // during the prepare stage
                methodSym.IsOverride = true;
            }

            if (methodSym.IsCtor &&
                methodSym.ParameterTypes.Count == 0 &&
                !methodSym.IsStatic)
            {
                parentAggSym.HasNoArgCtor = true;
                if (methodSym.Access == ACCESS.PUBLIC)
                {
                    parentAggSym.HasPubNoArgCtor = true;
                }
            }

            if (defineParams)
            {
                methodSym.HasParamsDefined = true;
            }

            DebugUtil.Assert(
                !methodSym.HasParamsDefined
                == !(methodSym.TypeVariables != null && methodSym.ParameterTypes != null));

            // (CS3) Extension method
            if (attributes.IsExtensionMethod)
            {
                Compiler.ClsDeclRec.DefineExtensionMethodCore(methodSym);
            }

            return methodSym;
        }

        //------------------------------------------------------------
        // IMPORTER.ImportMethodPropsWorker
        //
        /// <summary>
        /// <para>Set fields of a given METHSYM instance.
        /// <list type="bullet">
        /// <item>TypeVariables</item>
        /// <item>ReturnTypeSym</item>
        /// <item>ParameterTypes</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="parent"></param>
        /// <param name="info"></param>
        /// <param name="meth"></param>
        //------------------------------------------------------------
        internal void ImportMethodPropsWorker(
            INFILESYM infileSym,
            AGGSYM parent,
            MethodBase info,
            METHSYM meth)
        {
            meth.TypeVariables = DefineMethodTypeFormals(info, meth);

            // Set the method signature.
            ImportMethodSignature(infileSym, info, meth);

            if (meth.TypeVariables.Count > 0)
            {
                DefineBounds(meth);
            }
        }

        //------------------------------------------------------------
        // IMPORTER.ImportProperty
        //
        /// <summary>
        /// Import a propertydef.
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="parentAggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="defaultMemberName"></param>
        //------------------------------------------------------------
        internal PROPSYM ImportProperty(
            INFILESYM infileSym,
            AGGSYM parentAggSym,
            AGGDECLSYM aggDeclSym,
            //mdProperty tokenProperty,
            PropertyInfo propertyInfo,
            string defaultMemberName)
        {
            DebugUtil.Assert(propertyInfo != null);
            SYM sym = Compiler.MainSymbolManager.InfoToSymbolTable.GetSymFromInfo(propertyInfo);
            if (sym != null)
            {
                return sym as PROPSYM;
            }

            string propName = propertyInfo.Name;
            PROPSYM propertySym = null;
            METHSYM getMethodSym = null;
            METHSYM setMethodSym = null;
            MethodInfo tempMethInfo = null;

            //if (TypeFromToken(tokenProperty) != mdtProperty || tokenProperty == mdPropertyNil) {
            //    return;
            //}

            // Get property properties.

            //CheckHR(metaimport.GetPropertyProps(
            //    tokenProperty,                                  			// The method for which to get props.
            //    null,                                               		// Put method's class here.
            //    propnameText, lengthof(propnameText), &cchPropnameText,	// Method name
            //    & flags,                                            		// Put flags here.
            //    & signature, & cbSignature,                             	// Method signature
            //            null, null, null,                                 // Default value
            //            & tokenSetter, & tokenGetter,                     // Setter, getter,
            //            null, 0, null), scope);                           // Other methods
            //CheckTruncation(cchPropnameText, lengthof(propnameText), scope);

            //--------------------------------------------------
            // Check name.
            //--------------------------------------------------
            DebugUtil.Assert(propName != "ItemByIndex");

            // In the compiler, if the name contains a '.', then we won't be able to access if,
            // and its quite probabaly a explicit property implementation. Just don't import it. 
            if (propName.IndexOf('.') >= 0)
            {
                return null;
            }

            if (propName.Length <= 1)
            {
                return null;
            }
            Compiler.NameManager.AddString(propName);

            //--------------------------------------------------
            // Import all the attributes we are interested in at compile time
            //--------------------------------------------------
            IMPORTED_CUSTOM_ATTRIBUTES attributes = new IMPORTED_CUSTOM_ATTRIBUTES();
            ImportCustomAttributes(infileSym, propertyInfo.GetCustomAttributes(true), attributes);

            //--------------------------------------------------
            // Declare a property. Default properties with >0 args are actually indexers.
            //--------------------------------------------------
            //if (tokenGetter != mdMethodDefNil) {
            //    getMethodSym = FindMethodDef(parentAggSym, tokenGetter);
            //    ImportMethodProps(getMethodSym);
            //}
            //if (tokenSetter != mdMethodDefNil) {
            //    setMethodSym = FindMethodDef(parentAggSym, tokenSetter);
            //    ImportMethodProps(setMethodSym);
            //}

            //--------------------------------------------------
            // Import accessors
            //--------------------------------------------------
            tempMethInfo = propertyInfo.GetGetMethod(true);
            if (tempMethInfo != null)
            {
                //getMethodSym = FindMethodDef(parentAggSym, tempMethInfo);
                // getMethodSym != null ?
                //ImportMethodProps(infileSym, parentAggSym, tempMethInfo, getMethodSym);

                getMethodSym = ImportMethod(infileSym, parentAggSym, aggDeclSym, tempMethInfo);
            }
            else
            {
                getMethodSym = null;
            }

            tempMethInfo = propertyInfo.GetSetMethod(true);
            if (tempMethInfo != null)
            {
                //setMethodSym = FindMethodDef(parentAggSym, tempMethInfo);
                // setMethodSym != null ?
                //ImportMethodProps(infileSym, parentAggSym, tempMethInfo, setMethodSym);

                setMethodSym = ImportMethod(infileSym, parentAggSym, aggDeclSym, tempMethInfo);
            }
            else
            {
                setMethodSym = null;
            }

            // Make sure it's accessible.
            if (getMethodSym == null && setMethodSym == null)
            {
                return null;
            }

            //--------------------------------------------------
            // Create a PROPSYM instance and
            // set accessors and PropertyInfo
            //--------------------------------------------------
            if (//SigPeekUncompressData(scope, signature + 1, signature + cbSignature) > 0 &&
                (propName == defaultMemberName ||
                (getMethodSym != null && getMethodSym.Name == defaultMemberName) ||
                (setMethodSym != null && setMethodSym.Name == defaultMemberName)))
            {
                propertySym = Compiler.MainSymbolManager.CreateIndexer(propName, parentAggSym, aggDeclSym);
            }
            else
            {
                propertySym = Compiler.MainSymbolManager.CreateProperty(propName, parentAggSym, aggDeclSym);
            }

            DebugUtil.Assert(propertySym != null);
            //propertySym.tokenImport = tokenProperty;
            propertySym.PropertyInfo = propertyInfo;
            //propertySym.GetMethodSym = getMethodSym;
            //propertySym.SetMethodSym = setMethodSym;
            Compiler.MainSymbolManager.InfoToSymbolTable.SetSymForInfo(propertyInfo, propertySym);

            //--------------------------------------------------
            // Set the method signature.
            //--------------------------------------------------
            ImportPropSignature(infileSym, propertyInfo, propertySym);

            //--------------------------------------------------
            // handle indexed properties
            //--------------------------------------------------
            if (propertySym.ParameterTypes.Count > 0)
            {
                if (!propertySym.IsIndexer)
                {
                    // non-default indexed property
                    propertySym.SetBogus(true);
                }
                else
                {
                    for (int i = 0; i < propertySym.ParameterTypes.Count; ++i)
                    {
                        if (propertySym.ParameterTypes[i].IsPARAMMODSYM)
                        {
                            // INDEXERS can't have ref or out parameters
                            propertySym.SetBogus(true);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (propertySym.IsIndexer)
                {
                    propertySym.SetBogus(true);
                }
            }

            //--------------------------------------------------
            // Find the accessor methods. (1) get
            //--------------------------------------------------
            if (getMethodSym != null)
            {
                propertySym.GetMethodSym = getMethodSym;

                // check that the accessor is OK and it matches the property signature

                if (getMethodSym.IsBogus ||
                    propertySym.ReturnTypeSym != getMethodSym.ReturnTypeSym ||
                    propertySym.ParameterTypes.Count != getMethodSym.ParameterTypes.Count)
                {
                    propertySym.SetBogus(true);
                }
                else
                {
                    if (getMethodSym.ParameterTypes != propertySym.ParameterTypes)
                    {
                        // The parameters must be the same.
                        // Although it is legal to allow the parameters to differ in ref/out-ness
                        // all properties and indexers are bogus if they have ref or out parameters
                        // so it doesn't matter
                        propertySym.SetBogus(true);
                    }
                    if (getMethodSym.IsParameterArray)
                    {
                        propertySym.IsParameterArray = true;
                    }
                }
            }

            //--------------------------------------------------
            // Find the accessor methods. (2) set
            //
            // The parameters of set-accessor consist of
            // the parameters of indexer and value-parameter.
            //--------------------------------------------------
            if (setMethodSym != null)
            {
                propertySym.SetMethodSym = setMethodSym;

                // check that the accessor is OK and it matches the property signature

#if DEBUG
                bool db_IsBogus = setMethodSym.IsBogus;
                TYPESYM ret0 = Compiler.MainSymbolManager.VoidSym;
                TYPESYM ret1 = setMethodSym.ReturnTypeSym;
                TYPESYM ret2 = propertySym.ReturnTypeSym;
                TypeArray param1 = setMethodSym.ParameterTypes;
                TypeArray param2 = propertySym.ParameterTypes;
                if (propertySym.SymID == 1091)
                {
                    ;
                }
#endif

                if (setMethodSym.IsBogus ||
                    !setMethodSym.ReturnTypeSym.IsVoidType ||   // != Compiler.MainSymbolManager.VoidSym ||
                    setMethodSym.ParameterTypes.Count != (propertySym.ParameterTypes.Count + 1) ||
                    propertySym.ReturnTypeSym != setMethodSym.ParameterTypes[propertySym.ParameterTypes.Count])
                {
                    propertySym.SetBogus(true);
                }
                else
                {
                    // Can't do "if (setMethodSym.ParameterTypes != sym.params)" because
                    // of the set value argument
                    if (!TypeArray.EqualRange(
                        setMethodSym.ParameterTypes, 0,
                        propertySym.ParameterTypes, 0,
                        propertySym.ParameterTypes.Count))
                    {
                        // The parameters must be the same.
                        // Although it is legal to allow the parameters to differ in ref/out-ness
                        // all properties and indexers are bogus if they have ref or out parameters
                        // so it doesn't matter
                        propertySym.SetBogus(true);
                    }

                    // check for set only indexer with paramarray
                    if (setMethodSym.ParameterTypes.Count > 1 &&
                        setMethodSym.ParameterTypes[setMethodSym.ParameterTypes.Count - 2].IsARRAYSYM &&
                        (setMethodSym.ParameterTypes[setMethodSym.ParameterTypes.Count - 2] as ARRAYSYM).Rank == 1)
                    {
                        setMethodSym.IsParameterArray = propertySym.IsParameterArray
                            = IsParamArray(infileSym, setMethodSym.MethodInfo, setMethodSym.ParameterTypes.Count - 1);
                    }
                }
            }

            // Set attributes of the property by synthesizing them from the accessors.
            // If no accessors, or accessors disagree, it is bogus.
            DebugUtil.Assert(setMethodSym != null || getMethodSym != null);

            if (getMethodSym != null)
            {
                // a property's accessibility is that of the more visible methysm.
                if (setMethodSym != null)
                {
                    propertySym.Access = getMethodSym.Access > setMethodSym.Access ?
                        getMethodSym.Access : setMethodSym.Access;
                }
                else
                {
                    propertySym.Access = getMethodSym.Access;
                }
                propertySym.IsStatic = getMethodSym.IsStatic;
                propertySym.HideByName = getMethodSym.HideByName;
                if (setMethodSym != null &&
                    (getMethodSym.IsStatic != setMethodSym.IsStatic ||
                    getMethodSym.HideByName != setMethodSym.HideByName ||
                    setMethodSym.IsParameterArray != getMethodSym.IsParameterArray))
                {
                    propertySym.SetBogus(true);
                }
            }
            else // if (getMethodSym!=null)
            {
                DebugUtil.Assert(setMethodSym != null);
                propertySym.Access = setMethodSym.Access;
                propertySym.IsStatic = setMethodSym.IsStatic;
                propertySym.HideByName = setMethodSym.HideByName;
            }

            DebugUtil.Assert(ACCESS.PROTECTED > ACCESS.INTERNAL);
            if (getMethodSym != null &&
                setMethodSym != null &&
                propertySym.Access == ACCESS.PROTECTED &&
               (getMethodSym.Access == ACCESS.INTERNAL || setMethodSym.Access == ACCESS.INTERNAL))
            {
                // one of the accessors must be explicity more visible
                propertySym.SetBogus(true);
            }

            // only flag imported methods as accessors if the property is accesible

            if (!propertySym.IsBogus)
            {
                if (propertySym.GetMethodSym != null)
                {
                    propertySym.GetMethodSym.MethodKind = MethodKindEnum.PropAccessor;
                    propertySym.GetMethodSym.PropertySym = propertySym;
                }
                if (propertySym.SetMethodSym != null)
                {
                    propertySym.SetMethodSym.MethodKind = MethodKindEnum.PropAccessor;
                    propertySym.SetMethodSym.PropertySym = propertySym;
                }
                DebugUtil.Assert(!propertySym.UseMethodInstead);
            }
            else
            {
                propertySym.UseMethodInstead =
                    (propertySym.SetMethodSym != null && !propertySym.SetMethodSym.IsBogus) ||
                    (propertySym.GetMethodSym != null && !propertySym.GetMethodSym.IsBogus);
            }

            propertySym.SetDeprecated(
                attributes.IsDeprecated,
                attributes.IsDeprecatedError,
                attributes.DeprecatedString);

            if (attributes.HasCLSattribute)
            {
                propertySym.HasCLSAttribute = true;
                propertySym.IsCLS = attributes.IsCLS;
                if (getMethodSym != null)
                {
                    getMethodSym.HasCLSAttribute = true;
                    getMethodSym.IsCLS = attributes.IsCLS;
                }
                if (setMethodSym != null)
                {
                    setMethodSym.HasCLSAttribute = true;
                    setMethodSym.IsCLS = attributes.IsCLS;
                }
            }

            if (propertySym.IsDeprecated())
            {
                if (getMethodSym != null)
                {
                    getMethodSym.CopyDeprecatedFrom(propertySym);
                }
                if (setMethodSym != null)
                {
                    setMethodSym.CopyDeprecatedFrom(propertySym);
                }
            }

            return propertySym;
        }

        //------------------------------------------------------------
        // IMPORTER.ImportEvent
        //
        /// <summary>
        /// Import an eventdef.
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="parentAggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="eventInfo"></param>
        //------------------------------------------------------------
        internal void ImportEvent(
            INFILESYM infileSym,
            AGGSYM parentAggSym,
            AGGDECLSYM aggDeclSym,
            //mdEvent tokenEvent)
            EventInfo eventInfo)
        {
            DebugUtil.Assert(eventInfo != null);

            //MODULESYM * scope = parentAggSym.GetModule();
            //DebugUtil.Assert(scope.GetMetaImport(Compiler));
            //IMetaDataImport * metaimport = scope.GetMetaImport(Compiler);
            //WCHAR eventnameText[MAX_IDENT_SIZE];     // name of event
            //ULONG cchEventnameText;
            //PNAME name;
            string eventName = eventInfo.Name;
            EVENTSYM eventSym = null;
            //DWORD flags;
            //mdToken tokenEventType, tokenAdd, tokenRemove;
            METHSYM addMethodSym = null;
            METHSYM removeMethodSym = null;

            //if (TypeFromToken(tokenEvent) != mdtEvent || tokenEvent == mdEventNil) {
            //    return;
            //}

            //// Get Event propertys
            //CheckHR(metaimport.GetEventProps(
            //    tokenEvent,                         // [IN] event token 
            //    null,                               // [OUT] typedef containing the event declarion.    
            //    eventnameText,                      // [OUT] Event name 
            //    lengthof(eventnameText),            // [IN] the count of wchar of szEvent   
            //    &cchEventnameText,                  // [OUT] actual count of wchar for event's name 
            //    & flags,                            // [OUT] Event flags.   
            //    & tokenEventType,                   // [OUT] EventType class    
            //    & tokenAdd,                         // [OUT] AddOn method of the event  
            //    & tokenRemove,                      // [OUT] RemoveOn method of the event   
            //    null,                               // [OUT] Fire method of the event   
            //    null,                               // [OUT] other method of the event  
            //    0,                                  // [IN] size of rmdOtherMethod  
            //    null), scope);                      // [OUT] total number of other method of this event 
            //CheckTruncation(cchEventnameText, lengthof(eventnameText), scope);

            //if (wcschr(eventnameText, L'.') != null)
            //    return;

            // Get name.
            //if (cchEventnameText <= 1)
            //    return;
            //name = Compiler.namemgr.AddString(eventnameText, cchEventnameText - 1);

            if (eventName.IndexOf('.') >= 0 || eventName.Length <= 1)
            {
                return;
            }
            Compiler.NameManager.AddString(eventName);

            //--------------------------------------------------
            // Find the accessor methods.
            // They must be present, and have a signature of void XXX(EventType handler);
            //--------------------------------------------------
            //addMethodSym = removeMethodSym = null;
            //if (tokenAdd != mdMethodDefNil) {
            //    addMethodSym = FindMethodDef(parentAggSym, tokenAdd);
            //    ImportMethodProps(addMethodSym);
            //}
            //if (tokenRemove != mdMethodDefNil) {
            //    removeMethodSym = FindMethodDef(parentAggSym, tokenRemove);
            //    ImportMethodProps(removeMethodSym);
            //}
            MethodInfo tempInfo = eventInfo.GetAddMethod();
            if (tempInfo != null)
            {
                addMethodSym = FindMethodDef(parentAggSym, tempInfo);
                ImportMethodProps(infileSym, parentAggSym, tempInfo, addMethodSym);
            }

            tempInfo = eventInfo.GetRemoveMethod();
            if (tempInfo != null)
            {
                removeMethodSym = FindMethodDef(parentAggSym, tempInfo);
                ImportMethodProps(infileSym, parentAggSym, tempInfo, removeMethodSym);
            }

            if (addMethodSym == null && removeMethodSym == null)
            {
                return;
            }

            //--------------------------------------------------
            // Declare an event symbol.
            //--------------------------------------------------
            eventSym = Compiler.MainSymbolManager.CreateEvent(eventName, parentAggSym, aggDeclSym);
            //eventSym.tokenImport = tokenEvent;
            eventSym.EventInfo = eventInfo;

            // Get the event type.
            //eventSym.TypeSym = ResolveTypeRefOrSpec(scope, tokenEventType, parentAggSym.typeVarsAll);
            eventSym.TypeSym = ResolveType(
                infileSym,
                eventInfo.EventHandlerType,
                parentAggSym.AllTypeVariables,
                null);
            if (eventSym.TypeSym == null ||
                !eventSym.TypeSym.IsAGGTYPESYM && !eventSym.TypeSym.IsERRORSYM)
            {
                eventSym.SetBogus(true);
                eventSym.TypeSym = Compiler.MainSymbolManager.ErrorSym;
            }

            // Find the accessor methods. They must be present, and have a signature of void XXX(EventType handler);
            if (addMethodSym != null)
            {
                eventSym.AddMethodSym = addMethodSym;
                if (addMethodSym.ParameterTypes.Count != 1 ||
                    addMethodSym.ParameterTypes[0] != eventSym.TypeSym ||
                    !addMethodSym.ReturnTypeSym.IsVoidType) // != Compiler.MainSymbolManager.VoidSym)
                {
                    eventSym.SetBogus(true);
                }
            }
            if (removeMethodSym != null)
            {
                eventSym.RemoveMethodSym = removeMethodSym;
                if (removeMethodSym.ParameterTypes.Count != 1 ||
                    removeMethodSym.ParameterTypes[0] != eventSym.TypeSym ||
                    !removeMethodSym.ReturnTypeSym.IsVoidType)  // != Compiler.MainSymbolManager.VoidSym)
                {
                    eventSym.SetBogus(true);
                }
            }

            // Set attributes of the event by synthesizing from the accessors. If accessors disagree, it is bogus.
            DebugUtil.Assert(addMethodSym != null || removeMethodSym != null);
            if (addMethodSym != null && removeMethodSym != null)
            {
                eventSym.Access = addMethodSym.Access;
                eventSym.IsStatic = addMethodSym.IsStatic;
                if (removeMethodSym.Access != addMethodSym.Access ||
                    removeMethodSym.IsStatic != addMethodSym.IsStatic ||
                    !removeMethodSym.HideByName != !addMethodSym.HideByName)
                {
                    eventSym.SetBogus(true);
                }
            }
            else
            {
                eventSym.Access = addMethodSym != null ? addMethodSym.Access : removeMethodSym.Access;
                eventSym.SetBogus(true);
            }

            // If the event is OK, flags the accessors.
            if (!eventSym.IsBogus)
            {
                eventSym.AddMethodSym.MethodKind = MethodKindEnum.EventAccessor;
                eventSym.AddMethodSym.EventSym = eventSym;
                eventSym.RemoveMethodSym.MethodKind = MethodKindEnum.EventAccessor;
                eventSym.RemoveMethodSym.EventSym = eventSym;
                DebugUtil.Assert(!eventSym.UseMethodInstead);
            }
            else
            {
                eventSym.UseMethodInstead = (
                    (eventSym.AddMethodSym != null && !eventSym.AddMethodSym.IsBogus) ||
                    (eventSym.RemoveMethodSym != null && !eventSym.RemoveMethodSym.IsBogus));
            }

            //--------------------------------------------------
            // Import all the attributes we are interested in at compile time
            //--------------------------------------------------
            IMPORTED_CUSTOM_ATTRIBUTES attributes = new IMPORTED_CUSTOM_ATTRIBUTES();
            ImportCustomAttributes(infileSym, eventInfo.GetCustomAttributes(true), attributes);

            eventSym.SetDeprecated(
                attributes.IsDeprecated,
                attributes.IsDeprecatedError,
                attributes.DeprecatedString);

            if (attributes.HasCLSattribute)
            {
                eventSym.HasCLSAttribute = true;
                eventSym.IsCLS = attributes.IsCLS;
            }

            if (eventSym.IsDeprecated())
            {
                if (addMethodSym != null)
                {
                    addMethodSym.CopyDeprecatedFrom(eventSym);
                }
                if (removeMethodSym != null)
                {
                    removeMethodSym.CopyDeprecatedFrom(eventSym);
                }
            }
        }

        //------------------------------------------------------------
        // IMPORTER.FindMethodDef
        //
        /// <summary>
        /// <para>Given a methoddef token and a parent, find the corresponding symbol.
        /// There are two possible strategies here.
        /// One, do a linear search over all methods in the parent and match on the token.
        /// Two, get the name of the method def and then do a regular lookup.
        /// We chose the second, which should be faster,
        /// unless the metadata call to get the name is really slow.</para>
        /// <para>Find the METHSYM instance with the specified MethodInfo in the global symbol manager.</para>
        /// </summary>
        /// <param name="parentAggSym"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal METHSYM FindMethodDef(AGGSYM parentAggSym, MethodInfo methodInfo)
        {
            DebugUtil.Assert(methodInfo != null);

            //MODULESYM * scope = parentAggSym.GetModule();
            //DebugUtil.Assert(scope.GetMetaImport(compiler()));
            //IMetaDataImport * metaimport = scope.GetMetaImport(compiler());
            //WCHAR methodnameText[MAX_IDENT_SIZE];     // name of method
            //ULONG cchMethodnameText;
            //PNAME name;

            METHSYM methodSym = null;

            //if (TypeFromToken(token) != mdtMethodDef || token == mdMethodDefNil) {
            //    return null;
            //}

            //// Get method properties.
            //CheckHR(metaimport.GetMethodProps(
            //    token,                                                          // The method for which to get props.
            //    null,                                                           // Put method's class here.
            //    methodnameText, lengthof(methodnameText), &cchMethodnameText,   // Method name
            //    null,                                                           // Put flags here.
            //    null, null,                                                     // Method signature
            //    null,                                                           // codeRVA
            //    null), scope);                                                  // Impl. Flags
            //CheckTruncation(cchMethodnameText, lengthof(methodnameText), scope);
            //
            //// Convert name to a NAME.
            //if (cchMethodnameText <= 1)
            //    return null;
            //name = compiler().namemgr.LookupString(methodnameText, cchMethodnameText - 1);
            //if (!name)
            //    return null;

            //--------------------------------------------------
            // Search the parent for methods with this name
            // until one with a matching token is found.
            //--------------------------------------------------
            //methodSym = compiler().getBSymmgr().LookupAggMember(name, parentAggSym, SYMBMASK.METHSYM).asMETHSYM();
            //while (methodSym) {
            //    // Found the correct one?
            //    if (methodSym.tokenImport == token)
            //        return methodSym;

            //    // Got to the next one with the same name.
            //    methodSym = compiler().getBSymmgr().LookupNextSym(methodSym, parentAggSym, SYMBMASK.METHSYM).asMETHSYM();
            //}

            methodSym = (Compiler.MainSymbolManager.LookupAggMember(methodInfo.Name, parentAggSym, SYMBMASK.METHSYM) as METHSYM);
            while (methodSym != null)
            {
                if (methodSym.MethodInfo == methodInfo)
                {
                    return methodSym;
                }
                methodSym = BSYMMGR.LookupNextSym(methodSym, parentAggSym, SYMBMASK.METHSYM) as METHSYM;
            }

            // No method with this name and token in the parent.
            return null;
        }

        //------------------------------------------------------------
        // IMPORTER.ImportFieldType
        //
        /// <summary>
        /// Import a field type from a signature. Return NULL is type is not supported.
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="isVolatile"></param>
        /// <param name="classTypeVariables"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM ImportFieldType(
            INFILESYM infileSym,
            FieldInfo fieldInfo,
            out bool isVolatile,
            TypeArray classTypeVariables)
        {
            isVolatile = false;
            int cmodCount = 0;

            // Format of field signature is IMAGE_CEE_CS_CALLCONV_FIELD, followed by one type.
            //ImportScopeModule scope(this, mod);
            //
            //if (SigGetByte(scope, &sig, sigEnd) != IMAGE_CEE_CS_CALLCONV_FIELD)
            //    return NULL;        // Bogus!
            //*isVolatile = false;
            //BYTE b = SigPeekByte(scope, sig, sigEnd);
            //if (b == ELEMENT_TYPE_CMOD_REQD || b == ELEMENT_TYPE_CMOD_OPT) {
            //    bool isRequired = (b == ELEMENT_TYPE_CMOD_REQD);
            //
            //    ++sig;
            //    mdToken token = SigUncompressToken(scope, &sig, sigEnd); // updates sig
            //    TYPESYM * typeMod = ResolveTypeRefOrDef(mod, token, NULL);
            //
            //    if (typeMod && typeMod->isPredefType(PT_VOLATILEMOD)) {
            //        *isVolatile = true;
            //    }
            //    else if (isRequired) {
            //        return NULL;  // required modifier that we don't understand, can't import type at all.
            //    }
            //}

            // In sscli, accept only System.Runtime.CompilerServices.IsVolatile.
            // If other type is required, it is an error and return null.
            // If other optional type is specified, ignore it.

            Type[] reqCMods = fieldInfo.GetRequiredCustomModifiers();
            Type[] optCMods = fieldInfo.GetOptionalCustomModifiers();

            if (reqCMods.Length > 0)
            {
                foreach (Type ty in reqCMods)
                {
                    if (!(ty == CTypeOf.System.Runtime.CompilerServices.IsVolatile)) return null;
                }
                isVolatile = true;
            }

            foreach (Type ty in optCMods)
            {
                if (ty == CTypeOf.System.Runtime.CompilerServices.IsVolatile) isVolatile = true;
            }

            //return ImportSigType(
            //    infileSym,
            //    fieldInfo.FieldType,
            //    ImportSigOptions.None,
            //    ref cmodCount,
            //    classTypeVariables,
            //    null,
            //    true);
            return ResolveType(
                infileSym,
                fieldInfo.FieldType,
                classTypeVariables,
                null);
        }

        //------------------------------------------------------------
        // IMPORTER.StripArityFromName (1)
        //
        /// <summary>
        /// <para>If className has an arity specification (eg, !2):
        /// <list type="bullet">
        /// <item>Sets nameWithoutArity to the name without arity</item>
        /// <item>Sets nameWithoutArity to className</item>
        /// <item>Returns the arity</item>
        /// </list>
        /// </para>
        /// <para>If className has no arity specification or an invalid specification (eg, !0):
        /// <list type="bullet">
        /// <item>Sets nameWithoutArity to className</item>
        /// <item>Sets nameWithoutArity to null</item>
        /// <item>Returns 0</item>
        /// </list>
        /// </para>
        /// </summary>
        //------------------------------------------------------------
        static internal int StripArityFromName(string className, ref string nameWithoutArity, ref string nameWithArity)
        {
            nameWithoutArity = className;
            nameWithArity = null;
            int beginningOfArity;

            int arity = ComputeArityFromName(className, out beginningOfArity);
            if (beginningOfArity > 0)
            {
                nameWithoutArity = className.Substring(0, beginningOfArity);
                nameWithArity = className;
            }
            return arity;
        }

        //------------------------------------------------------------
        // IMPORTER.StripArityFromName (1)
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string StripArityFromName(string name)
        {
            if (name == null || name.Length <= 2)
            {
                return name;
            }

            char c;
            int start = 0;
            int last = name.Length - 2;

            for (start = 0; (c = name[start]) != '!' && c != '`'; ++start)
            {
                if (start >= last)
                {
                    return name;
                }
            }
            return name.Substring(0, start);
        }

        //------------------------------------------------------------
        // IMPORTER.CreateAggNameWithArity
        //
        /// <summary></summary>
        /// <remarks>
        /// (2015/01/14 hirano567@hotmail.co.jp)
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string CreateAggNameWithArity(string name, int arity)
        {
            if (String.IsNullOrEmpty(name)) return name;
            if (arity == 0) return name;
            return String.Format("{0}`{1}", name, arity);
        }

        //    int GetSignatureCParams(PCCOR_SIGNATURE sig);

        //------------------------------------------------------------
        // IMPORTER.ImportMethodOrPropSignature
        //
        /// Import a method/property ret type and params from a signature. Sets the isBogus flag
        /// if the signature has a type we don't handle.
        //------------------------------------------------------------
        //void ImportMethodOrPropSignature(mdMethodDef token, PCCOR_SIGNATURE sig, PCCOR_SIGNATURE sigEnd, PMETHPROPSYM sym);

        //------------------------------------------------------------
        // IMPORTER.ImportMethodSignature
        //
        /// <summary>
        /// <para>Import a method ret type and params from a signature.
        /// Sets the isBogus flag if the signature has a type we don't handle.</para>
        /// <para>Set the fields of methodSym by methodInfo.
        /// <list type="bullet">
        /// <item>ReturnTypeSym</item>
        /// <item>ParameterTypes</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <remarks>(sscli)
        /// void ImportMethodOrPropSignature(
        ///     mdMethodDef token,
        ///     PCCOR_SIGNATURE sig,
        ///     PCCOR_SIGNATURE sigEnd,
        ///     PMETHPROPSYM sym);
        /// </remarks>
        /// <param name="infileSym"></param>
        /// <param name="methodBase"></param>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void ImportMethodSignature(
            INFILESYM infileSym,
            MethodBase methodBase,
            METHSYM methodSym)
        {
#if DEBUG
            if (methodSym.SymID == 3715)
            {
                ;
            }
#endif
            DebugUtil.Assert(
                infileSym != null &&
                methodBase != null &&
                methodSym != null);
            DebugUtil.Assert(methodSym.ParentSym.IsAGGSYM);
            int modOptCount = 0;
            CallingConventions callConv;
            bool isVarargs = false;
            TypeArray classTypeValiables = methodSym.ClassSym.AllTypeVariables;
            TypeArray methodTypeVariables = methodSym.TypeVariables;
            Type[] genericArguments = null;
            TypeArray paramTypeArray = null;

            DebugUtil.Assert(methodTypeVariables != null);

            //ImportScopeModule scope(this, methodSym.GetModule());

            // (ImportSignature in sscli begin)

            //--------------------------------------------------
            // Calling Convention and VarArgs
            //--------------------------------------------------
            callConv = methodBase.CallingConvention;
            isVarargs = ((callConv & CallingConventions.VarArgs) != 0);

            //--------------------------------------------------
            // Count of Type Variables and Generic Arguments
            //--------------------------------------------------
            if (methodBase.IsGenericMethod)
            {
                genericArguments = methodBase.GetGenericArguments();
                if (methodTypeVariables != null &&
                    methodTypeVariables.Count != genericArguments.Length)
                {
                    DebugUtil.VsFail("Why is the method arity wrong?");
                    goto LBogus;
                }
            }

            //--------------------------------------------------
            // Return Type
            //--------------------------------------------------
            MethodInfo methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
            {
                methodSym.ReturnTypeSym = ResolveType(
                    infileSym,
                    methodInfo.ReturnType,
                    classTypeValiables,
                    methodTypeVariables);
                if (methodSym.ReturnTypeSym == null)
                {
                    goto LBogus;
                }
            }
            else if (methodBase is ConstructorInfo)
            {
                methodSym.ReturnTypeSym = Compiler.MainSymbolManager.VoidSym;
            }
            else
            {
                DebugUtil.Assert(false, "invalid methodBase");
            }

            //--------------------------------------------------
            // Parameters
            //--------------------------------------------------
            ParameterInfo[] paramInfos = methodBase.GetParameters();
            int paramCount = paramInfos.Length;
            int vaParamCount = paramCount + (isVarargs ? 1 : 0);
            paramTypeArray = vaParamCount > 0 ? new TypeArray(vaParamCount) : null;

            for (int i = 0; i < paramCount; ++i)
            {
                ParameterInfo param = paramInfos[i];
                TYPESYM paramTypeSym = ResolveType(
                    infileSym,
                    param.ParameterType,
                    classTypeValiables,
                    methodTypeVariables);

                if (paramTypeSym.IsPARAMMODSYM &&
                    (paramTypeSym as PARAMMODSYM).IsRef)
                {
                    if ((param.Attributes & (ParameterAttributes.In | ParameterAttributes.Out))
                        == ParameterAttributes.Out)
                    {
                        paramTypeSym = Compiler.MainSymbolManager.GetParamModifier(
                            (paramTypeSym as PARAMMODSYM).ParamTypeSym, true);
                    }
                }

                if (paramTypeSym == null)
                {
                    goto LBogus;
                }
                paramTypeArray[i] = paramTypeSym;
            }

            if (isVarargs)
            {
                paramTypeArray[paramCount] = Compiler.MainSymbolManager.ArgListSym;
            }
            methodSym.ParameterTypes = Compiler.MainSymbolManager.AllocParams(paramTypeArray);

            // (ImportSignature in sscli end)

            DebugUtil.Assert(methodSym.ParameterTypes != null);

            // We don't support generic properties.
            //if ((callConv & IMAGE_CEE_CS_CALLCONV_GENERIC) && !isMethod) goto LBogus;

            if ((callConv & CallingConventions.HasThis) != 0 && methodSym.IsStatic)
            {
                goto LBogus;
            }

            // Deal with calling convention. Must be default or varargs for methods, default or property for properties.

            //switch (callConv & IMAGE_CEE_CS_CALLCONV_MASK) {
            //case IMAGE_CEE_CS_CALLCONV_DEFAULT:
            //case IMAGE_CEE_CS_CALLCONV_UNMGD:
            //    if (sym->params->size > 0 && sym->isMETHSYM()) {
            //        // Check for params...
            //        TYPESYM * typeLast = sym->params->Item(sym->params->size - 1);
            //        if (typeLast->isARRAYSYM() && typeLast->asARRAYSYM()->rank == 1)
            //            sym->isParamArray = !!IsParamArray(sym->GetModule(), token, sym->params->size);
            //    }
            //    break;
            //
            //case IMAGE_CEE_CS_CALLCONV_PROPERTY:
            //    if (isMethod)
            //        goto LBogus;
            //    break;
            //
            //case IMAGE_CEE_CS_CALLCONV_VARARG:
            //    if (!isMethod || typeVarsMeth->size || sym->getClass()->typeVarsAll->size)
            //        goto LBogus;
            //    sym->asMETHSYM()->isVarargs = true;
            //    break;
            //
            //default:
            //    // We don't support that calling convention.
            //    goto LBogus;
            //}

            if ((callConv & CallingConventions.VarArgs) != 0)
            {
                if (methodTypeVariables.Count > 0 || methodSym.ClassSym.AllTypeVariables.Count > 0)
                {
                    goto LBogus;
                }
                (methodSym as METHSYM).IsVarargs = true;
            }
            else
            {
                if (methodSym.ParameterTypes.Count > 0)
                {
                    // Check for params...
                    TYPESYM typeLast = methodSym.ParameterTypes[methodSym.ParameterTypes.Count - 1];
                    if (typeLast.IsARRAYSYM && (typeLast as ARRAYSYM).Rank == 1)
                    {
                        methodSym.IsParameterArray
                            = IsParamArray(infileSym, methodBase, methodSym.ParameterTypes.Count - 1);
                    }
                }
            }

            methodSym.CModifierCount = modOptCount;
            return;

        LBogus:
            methodSym.SetBogus(true);
            if (methodSym.ReturnTypeSym == null)
            {
                methodSym.ReturnTypeSym = Compiler.MainSymbolManager.VoidSym;
            }
            if (methodSym.ParameterTypes == null)
            {
                methodSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            }
            return;
        }

        //------------------------------------------------------------
        // IMPORTER.ImportPropSignature
        //
        /// <summary>
        /// Import a property ret type and params from a signature. Sets the isBogus flag
        /// if the signature has a type we don't handle.
        /// </summary>
        /// <remarks>(sscli)
        /// void ImportMethodOrPropSignature(
        ///     mdMethodDef token,
        ///     PCCOR_SIGNATURE sig,
        ///     PCCOR_SIGNATURE sigEnd,
        ///     PMETHPROPSYM sym);
        /// </remarks>
        /// <param name="infileSym"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="propertySym"></param>
        //------------------------------------------------------------
        internal void ImportPropSignature(
            INFILESYM infileSym,
            PropertyInfo propertyInfo,
            PROPSYM propertySym)
        {
            DebugUtil.Assert(
                infileSym != null &&
                propertyInfo != null &&
                propertySym != null);
            DebugUtil.Assert(propertySym.ParentSym.IsAGGSYM);
            int modOptCount = 0;
            TypeArray classTypeValiables = propertySym.ClassSym.AllTypeVariables;
            TypeArray paramTypeArray = null;
            //byte callConv = 0;
            //bool isMethod = sym.isMETHSYM();
            //TypeArray * typeVarsMeth = isMethod ? sym.asMETHSYM().typeVars : BSYMMGR.EmptyTypeArray;

            //DebugUtil.Assert(typeVarsMeth);

            //ImportScopeModule scope(this, sym.GetModule());

            // ImportSignature begin

            //--------------------------------------------------
            // Retyrn Type
            //--------------------------------------------------
            //propertySym.ReturnTypeSym = ImportSigType(
            //    infileSym,
            //    propertyInfo.PropertyType,
            //    ImportSigOptions.AllowVoid,
            //    ref modOptCount,
            //    classTypeValiables,
            //    null,
            //    true);
            propertySym.ReturnTypeSym = ResolveType(
                infileSym,
                propertyInfo.PropertyType,
                classTypeValiables,
                null);
            if (propertySym.ReturnTypeSym == null)
            {
                goto LBogus;
            }

            //--------------------------------------------------
            // Parameters
            //
            // Process index parameters of indexer.
            //--------------------------------------------------
            ParameterInfo[] paramInfos = propertyInfo.GetIndexParameters();
            int paramCount = paramInfos.Length;
            paramTypeArray = new TypeArray(paramCount);

            for (int i = 0; i < paramCount; ++i)
            {
                ParameterInfo param = paramInfos[i];
                //TYPESYM paramTypeSym = ImportSigType(
                //    infileSym,
                //    param.ParameterType,
                //    ImportSigOptions.AllowByref,
                //    ref modOptCount,
                //    classTypeValiables,
                //    null,
                //    true);
                TYPESYM paramTypeSym = ResolveType(
                    infileSym,
                    param.ParameterType,
                    classTypeValiables,
                    null);

                if (paramTypeSym.IsPARAMMODSYM &&
                    (paramTypeSym as PARAMMODSYM).IsRef)
                {
                    if ((param.Attributes & (ParameterAttributes.In | ParameterAttributes.Out))
                        == ParameterAttributes.Out)
                    {
                        paramTypeSym = Compiler.MainSymbolManager.GetParamModifier(
                            (paramTypeSym as PARAMMODSYM).ParamTypeSym, true);
                    }
                }

                if (paramTypeSym == null) goto LBogus;
                paramTypeArray[i] = paramTypeSym;
            }

            propertySym.ParameterTypes = Compiler.MainSymbolManager.AllocParams(paramTypeArray);

            // ImportSignature end

            //DebugUtil.Assert(sym.params);

            // We don't support generic properties.
            //if ((callConv & IMAGE_CEE_CS_CALLCONV_GENERIC) && !isMethod)
            //    goto LBogus;

            //if ((callConv & IMAGE_CEE_CS_CALLCONV_HASTHIS) && sym.isStatic)
            //    goto LBogus;

            //--------------------------------------------------
            // Deal with calling convention. Must be default or varargs for methods, default or property for properties.
            //
            // PropertyInfo does not have CallingConvention, so cut off.
            //--------------------------------------------------
            //switch (callConv & IMAGE_CEE_CS_CALLCONV_MASK) {
            //case IMAGE_CEE_CS_CALLCONV_DEFAULT:
            //case IMAGE_CEE_CS_CALLCONV_UNMGD:
            //    if (sym.params.size > 0 && sym.isMETHSYM()) {
            //        // Check for params...
            //        TYPESYM * typeLast = sym.params.Item(sym.params.size - 1);
            //        if (typeLast.isARRAYSYM() && typeLast.asARRAYSYM().rank == 1)
            //            sym.isParamArray = !!IsParamArray(sym.GetModule(), token, sym.params.size);
            //    }
            //    break;
            //case IMAGE_CEE_CS_CALLCONV_PROPERTY:
            //    if (isMethod)
            //        goto LBogus;
            //    break;
            //case IMAGE_CEE_CS_CALLCONV_VARARG:
            //    if (!isMethod || typeVarsMeth.size || sym.getClass().typeVarsAll.size)
            //        goto LBogus;
            //    sym.asMETHSYM().isVarargs = true;
            //    break;
            //default:
            //    // We don't support that calling convention.
            //    goto LBogus;
            //}

            propertySym.CModifierCount = modOptCount;
            return;

        LBogus:
            propertySym.SetBogus(true);
            if (propertySym.ReturnTypeSym == null)
            {
                propertySym.ReturnTypeSym = Compiler.MainSymbolManager.VoidSym;
            }
            if (propertySym.ParameterTypes == null)
            {
                propertySym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            }
            return;
        }

        //    bool ImportSignature(ImportScope & scope, mdMethodDef token, PCCOR_SIGNATURE sig, PCCOR_SIGNATURE sigEnd,
        //        TYPESYM ** ptypeRet, TypeArray ** pparams, byte * pcallConv, int * pcmod, int * pcvar,
        //        TypeArray * typeVarsCls, TypeArray * typeVarsMeth);

        //------------------------------------------------------------
        // IMPORTER.IsParamArray
        //
        /// <summary>
        /// Determine whether the specified parameter is param array.
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="methodBase"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsParamArray(
            INFILESYM infileSym,
            MethodBase methodBase,
            int index)
        {
            ParameterInfo[] infos = methodBase.GetParameters();
            if (0 <= index && index < infos.Length)
            {
                IMPORTED_CUSTOM_ATTRIBUTES attributes = new IMPORTED_CUSTOM_ATTRIBUTES();
                ImportCustomAttributes(infileSym, infos[index].GetCustomAttributes(true), attributes);

                if (attributes.IsParamListArray)
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // IMPORTER.GetNSDecl
        //
        /// <summary>
        /// Given a name space, find or create a corresponding NSDECL symbol for this input file.
        /// </summary>
        /// <param name="nsSym">NSSYM of the declared namespace.</param>
        /// <param name="infileSym">where the namespace is declared.</param>
        /// <returns>Reference to the NSDECLSYM instance.</returns>
        //------------------------------------------------------------
        internal NSDECLSYM GetNsDecl(NSSYM nsSym, INFILESYM infileSym)
        {
            DebugUtil.Assert(infileSym != null);

            // first search existing declarations for this namespace
            // for a declaration with this inputfile
            NSDECLSYM nsdSym = null;
            for (nsdSym = nsSym.FirstDeclSym; nsdSym != null; nsdSym = nsdSym.NextDeclSym)
            {
                if (nsdSym.GetInputFile() == infileSym)
                {
                    return nsdSym;
                }
            }

            // Didn't find an existing declaration for this namespace/file combo so create one.
            NSSYM parentNsSym = nsSym.ParentNsSym;
            NSDECLSYM parentNsDeclSym = (parentNsSym != null ? GetNsDecl(parentNsSym, infileSym) : null);
            nsdSym = Compiler.MainSymbolManager.CreateNamespaceDecl(nsSym, parentNsDeclSym, infileSym, null);
            nsdSym.UsingClausesResolved = true;
            nsdSym.IsDefined = true;
            if (parentNsDeclSym == null)
            {
                infileSym.RootNsDeclSym = nsdSym;
            }
            return nsdSym;
        }

        //======================================================================
        // IMPORTER.GetTypeRefFullName
        //
        /// Resolves a typeref or typedef to just a top-level class name.
        /// If it can't be resolved, or resolves to a nested class, then false is returned.
        //======================================================================
        internal bool GetTypeRefFullName(MODULESYM moduleSym, Type type, out string fullName)
        {
            DebugUtil.Assert(false);
            fullName = null;
            return true;
        }
        //======================================================================
        // IMPORTER::GetTypeRefFullName
        //======================================================================
        //bool IMPORTER::GetTypeRefFullName(
        //    MODULESYM *scope,
        //    mdToken token,
        //    __out_ecount(cchBuffer) PWSTR fullnameText,
        //    ULONG cchBuffer)
        //{
        //    ASSERT(scope->GetMetaImport(compiler()));
        //    IMetaDataImport * metaimport = scope->GetMetaImport(compiler());
        //    mdToken tkResolutionScope = mdTokenNil;
        //    ULONG cchFullNameText = 0;
        //    DWORD flags;
        //    HRESULT hr;
        //
        //    *fullnameText = 0;
        //
        //    if (TypeFromToken(token) == mdtTypeRef && token != mdTypeRefNil) {
        //        // Get the full name of the referenced type.
        //        hr = metaimport->GetTypeRefProps(
        //                    token,                                          // typeref token
        //                    &tkResolutionScope,                             // resolution scope
        //                    fullnameText, cchBuffer, &cchFullNameText);     // Type name
        //
        //        fullnameText[min(cchBuffer - 1, cchFullNameText)] = 0;
        //        if (FAILED(hr) || TypeFromToken(tkResolutionScope) == mdtTypeRef || TypeFromToken(tkResolutionScope) == mdtTypeDef || cchFullNameText > cchBuffer)
        //            return false; // failure or nested type.
        //    }
        //    else if (TypeFromToken(token) == mdtTypeDef && token != mdTypeDefNil) {
        //        hr = metaimport->GetTypeDefProps(token,
        //                fullnameText, cchBuffer, &cchFullNameText,      // Type name
        //                &flags,                                         // Flags
        //                NULL);                                          // Extends
        //
        //        fullnameText[min(cchBuffer - 1, cchFullNameText)] = 0;
        //        if (FAILED(hr) || IsTdNested(flags) || cchFullNameText > cchBuffer)
        //            return false; // failure or nested type
        //    } else {
        //        return false;
        //    }
        //
        //    return true;
        //}

        //    bool GetTypeRefFullNameWithOuter(IMetaDataImport * pmdi, mdToken tok, StringBldr & str);

        //------------------------------------------------------------
        // IMPORTER.ResolveBaseRef
        //
        /// <summary>
        /// <para>Resolve a base class or interface name. Similar to ResolveTypeRefOrSpec,
        /// but reports error on failure, and forces the base class or interface to be declared.</para>
        /// <para>fRequired indicates whether it is an error if the type can't be resolved.
        /// This is typically false iff symDerived is a class/struct and
        /// tokenBase came from the interface list.</para>
        /// <para>Call ResolveTypeRefOrSpec to get the TYPESYM instance.
        /// Return it as AGGTYPESYM.</para>
        /// </summary>
        /// <param name="inFileSym"></param>
        /// <param name="baseType"></param>
        /// <param name="derivedSym"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        ///------------------------------------------------------------
        internal AGGTYPESYM ResolveBaseRef(INFILESYM inFileSym, Type baseType, AGGSYM derivedSym, bool required)
        {
            DebugUtil.Assert(inFileSym != null);
            DebugUtil.Assert(baseType != null);

            TYPESYM baseTypeSym = ResolveType(
                inFileSym,
                baseType,
                derivedSym.AllTypeVariables,
                null);

            if (baseTypeSym != null && baseTypeSym.IsAGGTYPESYM)
            {
                return (baseTypeSym as AGGTYPESYM);
            }

            //--------------------------------------------------
            // Error processing
            // If argument required is true, show an error message.
            //--------------------------------------------------
            if (required)
            {
                if (baseType != null)
                {
                    Compiler.Error(
                        new ERRLOC(
                            inFileSym,
                            compiler.OptionManager.FullPaths),
                        CSCERRID.ERR_ImportBadBase,
                        new ErrArg(derivedSym));
                }
                else
                {
                    Compiler.Error(
                        new ERRLOC(
                            inFileSym,
                            compiler.OptionManager.FullPaths),
                        CSCERRID.ERR_CantImportBase,
                        new ErrArg(derivedSym),
                        new ErrArg(baseType.Name),
                        new ErrArg(baseType.Assembly.GetName().Name));
                }
            }
            return null;
        }
        //------------------------------------------------------------
        // IMPORTER::ResolveBaseRef (sscli)
        //------------------------------------------------------------
        //PAGGTYPESYM IMPORTER::ResolveBaseRef(MODULESYM *scope, mdToken tokenBase, PAGGSYM symDerived, bool fRequired)
        //{
        //    ASSERT(!IsNilToken(tokenBase));
        //    ASSERT(scope->GetMetaImport(compiler()));
        //
        //    PTYPESYM symBase;
        //
        //    symBase = ResolveTypeRefOrSpec(scope, tokenBase, symDerived->typeVarsAll);
        //
        //    if (!symBase || !symBase->isAGGTYPESYM()) {
        //        if (!fRequired)
        //            return NULL;
        //
        //        if (TypeFromToken(tokenBase) == mdtTypeSpec) {
        //            compiler()->Error(ERRLOC(scope->getInputFile()), ERR_ImportBadBase, symDerived);
        //        }
        //        else {
        //            ASSERT(!symBase); // If symBase is found it should be an AGGTYPESYM!
        //
        //            // Couldn't resolve base class. Give a good error message.
        //            WCHAR rgchType[MAX_FULLNAME_SIZE * 2];
        //            StringBldrFixed str(rgchType, lengthof(rgchType));
        //            WCHAR szAssem[MAX_FULLNAME_SIZE];
        //
        //            szAssem[0] = 0;
        //
        //            GetTypeRefFullNameWithOuter(scope->GetMetaImport(compiler()), tokenBase, str);
        //
        //            GetTypeRefAssemblyName(scope, tokenBase, szAssem, lengthof(szAssem));
        //            szAssem[lengthof(szAssem) - 1] = 0;
        //
        //            compiler()->Error(ERRLOC(scope->getInputFile()), ERR_CantImportBase, symDerived, str.Str(), szAssem);
        //        }
        //
        //        symBase = NULL;
        //    }
        //
        //    return symBase->asAGGTYPESYM();
        //}

        //------------------------------------------------------------
        // IMPORTER.DefineMethodTypeFormals
        //
        /// <summary>
        /// <para>If meth is NULL, we produce a list of the standard method type variables.
        /// Creates the type variables but doesn't set their bounds.</para>
        /// <para>Create a TypeArray from methodBase.GetGenericArguments().
        /// methSym is the parent of each element of the TypeArray.</para>
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="methSym"></param>
        /// <param name="contextSym"></param>
        /// <param name="typeVariables"></param>
        //------------------------------------------------------------
        internal TypeArray DefineMethodTypeFormals(
            MethodBase methodBase,
            METHSYM methSym)
        {
            // Set the type variables for the sym, but not the bounds.

            Type[] genericArguments = null;
            int tvCount = 0;

            if (methodBase.IsGenericMethod)
            {
                genericArguments = methodBase.GetGenericArguments();
                tvCount = genericArguments.Length;
            }
            else
            {
                return BSYMMGR.EmptyTypeArray;
            }

            TypeArray typeArray = new TypeArray(tvCount);

            if (methSym == null)
            {
                // Just use the standard method type variables.
                for (int ivar = 0; ivar < tvCount; ++ivar)
                {
                    typeArray[ivar] = Compiler.MainSymbolManager.GetStdMethTypeVar(ivar);
                }
                return Compiler.MainSymbolManager.AllocParams(typeArray);
            }

            bool isBadList = false;

            for (int index = 0; index < tvCount; ++index)
            {
                Type tvType = genericArguments[index];
                int pos = tvType.GenericParameterPosition;

                if (pos < 0 || pos >= tvCount || typeArray[pos] != null)
                {
                    // Bad or duplicate index.
                    isBadList = true;
                    continue;
                }

                Compiler.NameManager.AddString(tvType.Name);
                TYVARSYM tvSym = Compiler.MainSymbolManager.CreateTyVar(tvType.Name, methSym);
                DebugUtil.Assert(tvSym.IsMethodTypeVariable);
                tvSym.Access = ACCESS.PRIVATE;
                tvSym.TotalIndex = pos;
                tvSym.Index = pos;
                tvSym.ParseTreeNode = null;
                tvSym.SetSystemType(tvType, false);
                typeArray[pos] = tvSym;

                SetSymInCache(tvType, tvSym);
            }

            if (isBadList)
            {
                methSym.SetBogus(true);

                // Fill in blank slots.
                for (int index = 0; index < tvCount; index++)
                {
                    if (typeArray[index] != null)
                    {
                        continue;
                    }

                    string name = String.Format("__TyVar{0}", index);
                    Compiler.NameManager.AddString(name);

                    TYVARSYM tvSym = Compiler.MainSymbolManager.CreateTyVar(name, methSym);
                    DebugUtil.Assert(tvSym.IsMethodTypeVariable);
                    tvSym.Access = ACCESS.PRIVATE;
                    tvSym.TotalIndex = index;
                    tvSym.Index = index;
                    tvSym.ParseTreeNode = null;
                    typeArray[index] = tvSym;
                }
            }

            // Create the TypeArray.
            return Compiler.MainSymbolManager.AllocParams(typeArray);
        }

        //------------------------------------------------------------
        // IMPORTER.
        //------------------------------------------------------------
        //    TYPESYM * ResolveTypeNameCore(MODULESYM * mod, ITypeName * ptnType);
        //    AGGTYPESYM * ResolveTypeNames(MODULESYM * mod, int aid, bool fTryMsCorLib, ITypeName * ptnType);
        //    AGGTYPESYM * ResolveTypeArgs(MODULESYM * mod, AGGTYPESYM * ats, ITypeName * ptnType);
        //    TYPESYM * ResolveModifiers(MODULESYM * mod, AGGTYPESYM * ats, ITypeName * ptnType);

        //------------------------------------------------------------
        // IMPORTER.ImportCustomAttributes
        //
        /// <summary>
        /// Read all the custom attributes on a members and handle them appropriately
        /// </summary>
        /// <param name="inFileSym"></param>
        /// <param name="attributes"></param>
        /// <param name="attrSumUp"></param>
        //------------------------------------------------------------
        internal void ImportCustomAttributes(
            INFILESYM inFileSym,
            Object[] attributes,
            IMPORTED_CUSTOM_ATTRIBUTES attrSumUp)
        {
            if (attributes == null || attributes.Length == 0 || attrSumUp == null)
            {
                return;
            }

            foreach (Object attribute in attributes)
            {
                ImportOneCustomAttribute(inFileSym, attrSumUp, attribute);
            }
        }

        //------------------------------------------------------------
        // IMPORTER::ImportCustomAttributes (sscli)
        //------------------------------------------------------------
        //void IMPORTER::ImportCustomAttributes(
        //    MODULESYM *mod,
        //    INFILESYM * infile,
        //    IMPORTED_CUSTOM_ATTRIBUTES * attributes,
        //    mdToken token)
        //{
        //    IMetaDataImport * metaimport = mod->GetMetaImport(compiler());
        //    HCORENUM corenum;           // For enumerating tokens.
        //    HALINKENUM hEnum;           // For enumerating tokens
        //    mdToken attributesBuffer[32];
        //    ULONG cAttributes, iAttribute;
        //    corenum = 0;
        //
        //    MEM_SET_ZERO(*attributes);
        //    attributes->conditionalHead = &attributes->conditionalSymbols;
        //    if (token == mdtAssembly) {
        //        CheckHR(compiler()->linker->ImportTypes(compiler()->assemID, mod->getInputFile()->mdImpFile , mod->GetIndex(), &hEnum, NULL, NULL), mod);
        //    }
        //
        //    do {
        //        // Get next batch of attributes.
        //        if (token == mdtAssembly) 
        //            CheckHR(compiler()->linker->EnumCustomAttributes(hEnum, 0, attributesBuffer, lengthof(attributesBuffer), &cAttributes), mod);
        //        else
        //            CheckHR(metaimport->EnumCustomAttributes(&corenum, token, 0, attributesBuffer, lengthof(attributesBuffer), &cAttributes), mod);
        //
        //        // Process each attribute.
        //        for (iAttribute = 0; iAttribute < cAttributes; ++iAttribute) {
        //            mdToken attrToken;
        //            const void * pvData;
        //            ULONG cbSize;
        //
        //            CheckHR(metaimport->GetCustomAttributeProps(
        //                attributesBuffer[iAttribute],
        //                NULL,
        //                &attrToken,
        //                &pvData,
        //                &cbSize), mod);
        //
        //            ImportOneCustomAttribute(mod, infile, attributes, attrToken, pvData, cbSize);
        //        }
        //    } while (cAttributes > 0);
        //
        //    if (token == mdtAssembly) {
        //        CheckHR(compiler()->linker->CloseEnum(hEnum), mod);
        //    }
        //    else {
        //        metaimport->CloseEnum(corenum);
        //    }
        //    
        //    if (token != mdtAssembly)
        //    {
        //        corenum = 0;
        //        CheckHR(metaimport->EnumPermissionSets(&corenum, token, dclLinktimeCheck, attributesBuffer, 1, &cAttributes), mod);
        //        metaimport->CloseEnum(corenum);
        //        
        //        attributes->hasLinkDemand = (cAttributes != 0);
        //    }
        //}

        //------------------------------------------------------------
        //
        //------------------------------------------------------------
        private static Type ObsoluteAttrType = typeof(System.ObsoleteAttribute);
        private static Type AttributeUsageAttrType = typeof(System.AttributeUsageAttribute);
        private static Type ConditionalAttrType = typeof(System.Diagnostics.ConditionalAttribute);
        private static Type DecimalConstAttrType = typeof(System.Runtime.CompilerServices.DecimalConstantAttribute);
        private static Type ClsCompliantAttrType = typeof(System.CLSCompliantAttribute);
        // Deprecated
        private static Type ParamsArrayAttrType = typeof(System.ParamArrayAttribute);
        private static Type RequiredAttrType = typeof(System.Runtime.CompilerServices.RequiredAttributeAttribute);
        private static Type DefaultMemberAttrType = typeof(System.Reflection.DefaultMemberAttribute);
        // System.Runtime.InteropServices.DefaultMemberAttribute
        private static Type CoClassAttrType = typeof(System.Runtime.InteropServices.CoClassAttribute);
        private static Type FixedBufferAttrType = typeof(System.Runtime.CompilerServices.FixedBufferAttribute);
        private static Type CompilationRelaxAttrType = typeof(System.Runtime.CompilerServices.CompilationRelaxationsAttribute);
        private static Type RumtimeCompatAttrType = typeof(System.Runtime.CompilerServices.RuntimeCompatibilityAttribute);
        private static Type FriendAssemblyAttrType = typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute);

        // CS3
        private static Type ExtensionMethodAttrType = typeof(System.Runtime.CompilerServices.ExtensionAttribute);

        //------------------------------------------------------------
        // IMPORTER.ImportOneCustomAttribute
        //
        /// <summary>
        /// <para>Handle one custom attribute read in.</para>
        /// <para>Examine several specified attributes, and record to attrSumUp.</para>
        /// </summary>
        /// <param name="inFileSym"></param>
        /// <param name="attrSumUp"></param>
        /// <param name="attribute"></param>
        //------------------------------------------------------------
        internal void ImportOneCustomAttribute(
            INFILESYM inFileSym,
            IMPORTED_CUSTOM_ATTRIBUTES attrSumUp,
            Object attribute)
        {
            if (attrSumUp == null || attribute == null)
            {
                return;
            }

            Type type = attribute.GetType();
            if (ObsoluteAttrType.Equals(type))
            {
                ObsoleteAttribute attr = attribute as ObsoleteAttribute;
                attrSumUp.IsDeprecated = true;
                attrSumUp.IsDeprecatedError = attr.IsError;
                attrSumUp.DeprecatedString = attr.Message;
            }
            else if (AttributeUsageAttrType.Equals(type))
            {
                AttributeUsageAttribute attr = attribute as AttributeUsageAttribute;
                attrSumUp.AllowMultiple = attr.AllowMultiple;
                attrSumUp.AttributeKind = attr.ValidOn;
            }
            else if (ConditionalAttrType.Equals(type))
            {
                System.Diagnostics.ConditionalAttribute attr
                    = attribute as System.Diagnostics.ConditionalAttribute;
                if (attrSumUp.ConditionalSymbols == null)
                {
                    attrSumUp.ConditionalSymbols = new List<string>();
                }
                attrSumUp.ConditionalSymbols.Add(attr.ConditionString);
            }
            else if (DecimalConstAttrType.Equals(type))
            {
                System.Runtime.CompilerServices.DecimalConstantAttribute attr
                    = attribute as System.Runtime.CompilerServices.DecimalConstantAttribute;
                attrSumUp.HasDecimalLiteral = true;
                attrSumUp.DecimalLiteral = attr.Value;
            }
            else if (ClsCompliantAttrType.Equals(type))
            {
                System.CLSCompliantAttribute attr = attribute as System.CLSCompliantAttribute;
                attrSumUp.HasCLSattribute = true;
                attrSumUp.IsCLS = attr.IsCompliant;
            }
            else if (ParamsArrayAttrType.Equals(type))
            {
                attrSumUp.IsParamListArray = true;
            }
            else if (RequiredAttrType.Equals(type))
            {
                attrSumUp.HasRequiredAttribute = true;
            }
            else if (DefaultMemberAttrType.Equals(type))
            {
                System.Reflection.DefaultMemberAttribute attr
                    = attribute as System.Reflection.DefaultMemberAttribute;
                attrSumUp.DefaultMember = attr.MemberName;
            }
            else if (CoClassAttrType.Equals(type))
            {
                System.Runtime.InteropServices.CoClassAttribute attr
                    = attribute as System.Runtime.InteropServices.CoClassAttribute;
                attrSumUp.CoClass = attr.CoClass;
            }
            else if (FixedBufferAttrType.Equals(type))
            {
                System.Runtime.CompilerServices.FixedBufferAttribute attr
                    = attribute as System.Runtime.CompilerServices.FixedBufferAttribute;
                attrSumUp.FixedBufferTypeSym = new TYPESYM();
                //attrSumUp.FixedBufferTypeSym.SetType(attr.ElementType);
                attrSumUp.FixedBufferElementCount = attr.Length;
            }
            else if (CompilationRelaxAttrType.Equals(type))
            {
                attrSumUp.HasCompilationRelaxations = true;
            }
            else if (RumtimeCompatAttrType.Equals(type))
            {
                System.Runtime.CompilerServices.RuntimeCompatibilityAttribute attr
                    = attribute as System.Runtime.CompilerServices.RuntimeCompatibilityAttribute;
                attrSumUp.HasRuntimeCompatibility = true;
                attrSumUp.WrapNonExceptionThrows = attr.WrapNonExceptionThrows;
            }
            else if (FriendAssemblyAttrType.Equals(type))
            {
                System.Runtime.CompilerServices.InternalsVisibleToAttribute attr
                    = attribute as System.Runtime.CompilerServices.InternalsVisibleToAttribute;
                attrSumUp.HasFriends = true;
                string friendAsmName = attr.AssemblyName;
                if (!String.IsNullOrEmpty(friendAsmName))
                {
                    //ImportScopeModule scope = new ImportScopeModule(this, modSym);
                    int aid = MapAssemblyRefToAid(friendAsmName, inFileSym, true);
                    inFileSym.AddInternalsVisibleTo(Kaid.ThisAssembly);
                }
            }
            // (CS3) Extension method
            else if (ExtensionMethodAttrType.Equals(type))
            {
                attrSumUp.IsExtensionMethod = true;
            }
        }

        //------------------------------------------------------------
        // IMPORTER::ImportOneCustomAttribute (sscli)
        //------------------------------------------------------------
        //void ImportOneCustomAttribute(
        //    MODULESYM *mod,
        //    INFILESYM * infile,
        //    IMPORTED_CUSTOM_ATTRIBUTES * attributes,
        //    mdToken attrToken,
        //    const void * pvData,
        //    ULONG cbSize)
        //{
        //    IMetaDataImport * metaimport = mod->GetMetaImport(compiler());
        //    mdToken typeToken;
        //    WCHAR ctornameText[MAX_IDENT_SIZE];     // name of field
        //    ULONG cchCtornameText;
        //    WCHAR fullNameText[MAX_FULLNAME_SIZE];
        //    ULONG cchFullNameText;
        //    PNAME typeName;
        //    PCCOR_SIGNATURE signature;
        //    ULONG cbSignature = 0;
        //    IMPORTEDCUSTOMATTRKIND attrKind;
        //
        //
        //    // a 0 length blob is allowed, but not for any of the attributes we are looking for
        //    if (cbSize == 0)
        //        return;
        //
        //    if (TypeFromToken(attrToken) == mdtMemberRef && attrToken != mdMemberRefNil) {
        //
        //        // Get name of member and signature.
        //        CheckHR(metaimport->GetMemberRefProps(
        //                    attrToken,
        //                    &typeToken,
        //                    ctornameText, lengthof(ctornameText), & cchCtornameText, // Field name
        //                    &signature, &cbSignature), mod);
        //        CheckTruncation(cchCtornameText, lengthof(ctornameText), mod);
        //
        //        if (cchCtornameText == 0 || wcscmp(ctornameText, compiler()->namemgr->GetPredefName(PN_CTOR)->text) != 0)
        //            return ;  // Member ref didn't reference a constructor.
        //    } else if (TypeFromToken(attrToken) == mdtMethodDef && attrToken != mdMethodDefNil) {
        //
        //        // Get name of member and signature.
        //        CheckHR(metaimport->GetMethodProps(
        //                    attrToken,
        //                    &typeToken,
        //                    ctornameText, lengthof(ctornameText), & cchCtornameText, // Field name
        //                    NULL,
        //                    &signature, &cbSignature,
        //                    NULL, NULL), mod);
        //        CheckTruncation(cchCtornameText, lengthof(ctornameText), mod);
        //
        //        if (wcscmp(ctornameText, compiler()->namemgr->GetPredefName(PN_CTOR)->text) != 0)
        //            return ;  // Member ref didn't reference a constructor.
        //    }
        //    else
        //        return;  // Only MethodDefs or MemberRefs are allowed
        //
        //    if (TypeFromToken(typeToken) == mdtTypeRef && typeToken != mdTypeRefNil) {
        //        mdToken resolutionScope;
        //        // Get name of type.
        //        CheckHR(metaimport->GetTypeRefProps(
        //                    typeToken,                                              // typeref token                 
        //                    &resolutionScope,                                       // resolution scope
        //                    fullNameText, lengthof(fullNameText), &cchFullNameText),// name
        //            mod);
        //        CheckTruncation(cchFullNameText, lengthof(fullNameText), mod);
        //
        //        if (TypeFromToken(resolutionScope) != mdtModule && TypeFromToken(resolutionScope) != mdtModuleRef &&
        //             TypeFromToken(resolutionScope) != mdtAssembly && TypeFromToken(resolutionScope) != mdtAssemblyRef)
        //        {
        //            // Not looking for nested types
        //            return;
        //        }
        //    } else if (TypeFromToken(typeToken) == mdtTypeDef && typeToken != mdTypeDefNil) {
        //        DWORD flags;
        //        // Get name of type.
        //        CheckHR(metaimport->GetTypeDefProps(
        //                    typeToken,                          // typeref token                    
        //                    fullNameText, lengthof(fullNameText), &cchFullNameText,  // name
        //                    &flags, NULL),
        //            mod);
        //        CheckTruncation(cchFullNameText, lengthof(fullNameText), mod);
        //
        //        if (IsTdNested(flags)) {
        //            // Not looking for any nested types
        //            return;
        //        }
        //    } else {
        //        // Unrecognized Member Ref
        //        return;
        //    }
        //
        //    ASSERT(!COMPILER::IsRegString(fullNameText, L"attribute"));
        //
        //    // Convert name to a NAME by looking up in hash table.
        //    typeName = compiler()->namemgr->LookupString(fullNameText);
        //    if (typeName == NULL)
        //        return;  // Not a known name.
        //
        //    // Match type name and signature against list of predefined ones.
        //    attrKind = CUSTOMATTR_NONE;
        //    for (unsigned int i = 0; i < lengthof(g_importedAttributes); ++i) {
        //        if (compiler()->namemgr->GetPredefName(g_importedAttributes[i].className) == typeName &&
        //            (g_importedAttributes[i].cbSig < 0 ||
        //             ((unsigned) g_importedAttributes[i].cbSig == cbSignature &&
        //             memcmp(g_importedAttributes[i].sig, signature, cbSignature) == 0)))
        //        {
        //            attrKind = g_importedAttributes[i].attrKind;
        //            break;
        //        }
        //    }
        //
        //    if (attrKind == CUSTOMATTR_NONE)
        //        return;         // Not a predefined custom attribute kind.
        //
        //    // Make sure that data is in the correct format. Check the prolog, then
        //    // move beyond it.
        //    if (cbSize < sizeof(WORD) || GET_UNALIGNED_VAL16 (pvData) != 1) {
        //        return;
        //    }
        //    cbSize -= sizeof(WORD);
        //    pvData = (WORD *)pvData + 1;
        //
        //    // We found an attribute kind that we know about. Grab any information from the binary data, and
        //    // apply it to the symbol.
        //    switch (attrKind) {
        //    case CUSTOMATTR_DEPRECATED_VOID:
        //        attributes->isDeprecated = true;
        //        break;
        //
        //    case CUSTOMATTR_DEPRECATED_STR:
        //        attributes->isDeprecated = true;
        //        attributes->deprecatedString = ImportCustomAttrArgString(mod, & pvData, & cbSize);
        //        break;
        //
        //    case CUSTOMATTR_DEPRECATED_STRBOOL:
        //        attributes->isDeprecated = true;
        //        attributes->deprecatedString = ImportCustomAttrArgString(mod, & pvData, & cbSize);
        //        attributes->isDeprecatedError = ImportCustomAttrArgBool(mod, & pvData, & cbSize);
        //        break;
        //
        //    case CUSTOMATTR_ATTRIBUTEUSAGE_VOID:
        //        {
        //            attributes->attributeKind = catAll;
        //            int numberOfNamedArguments = ImportCustomAttrArgWORD(mod, &pvData, &cbSize);
        //            for (int i = 0; i < numberOfNamedArguments; i += 1) {
        //                PCWSTR name;
        //                TYPESYM *type;
        //                ImportNamedCustomAttrArg(mod, &pvData, &cbSize, &name, &type);
        //                if (!name || !type)
        //                    break;
        //                if (!wcscmp(name, compiler()->namemgr->GetPredefName(PN_VALIDON)->text)) {
        //                    attributes->allowMultiple = !!(CorAttributeTargets) ImportCustomAttrArgInt(mod, &pvData, &cbSize);
        //                } else if (!wcscmp(name, compiler()->namemgr->GetPredefName(PN_ALLOWMULTIPLE)->text)) {
        //                    attributes->allowMultiple = (ImportCustomAttrArgBYTE(mod, &pvData, &cbSize) ? true : false);
        //                } else if (!wcscmp(name, compiler()->namemgr->GetPredefName(PN_INHERITED)->text)) {
        //                    ImportCustomAttrArgBYTE(mod, &pvData, &cbSize);
        //                } else {
        //                    ASSERT(!"Unknown named argument for imported attributeusage");
        //                    break;
        //                }
        //            }
        //        }
        //        break;
        //
        //    case CUSTOMATTR_ATTRIBUTEUSAGE_VALIDON:
        //        {
        //            attributes->attributeKind = (CorAttributeTargets) ImportCustomAttrArgInt(mod, & pvData, & cbSize);
        //            int numberOfNamedArguments = ImportCustomAttrArgWORD(mod, &pvData, &cbSize);
        //            for (int i = 0; i < numberOfNamedArguments; i += 1) {
        //                PCWSTR name;
        //                TYPESYM *type;
        //                ImportNamedCustomAttrArg(mod, &pvData, &cbSize, &name, &type);
        //                if (!name || !type)
        //                    break;
        //                if (!wcscmp(name, compiler()->namemgr->GetPredefName(PN_ALLOWMULTIPLE)->text)) {
        //                    attributes->allowMultiple = (ImportCustomAttrArgBYTE(mod, &pvData, &cbSize) ? true : false);
        //                } else if (!wcscmp(name, compiler()->namemgr->GetPredefName(PN_INHERITED)->text)) {
        //                    ImportCustomAttrArgBYTE(mod, &pvData, &cbSize);
        //                } else {
        //                    ASSERT(!"Unknown named argument for imported attributeusage");
        //                    break;
        //                }
        //            }
        //        }
        //        break;
        //
        //    case CUSTOMATTR_CONDITIONAL:
        //        // convert the string to a name and return it
        //        compiler()->getBSymmgr().AddToGlobalNameList(
        //                compiler()->namemgr->AddString(ImportCustomAttrArgString(mod, & pvData, & cbSize)),
        //                &attributes->conditionalHead);
        //        break;
        //
        //    case CUSTOMATTR_DECIMALLITERAL:
        //        if ((cbSize + sizeof(WORD)) == sizeof(DecimalConstantBuffer))
        //        {
        //            DecimalConstantBuffer *buffer = (DecimalConstantBuffer*)(((BYTE*)pvData)-(int)sizeof(WORD));
        //            DECIMAL_SCALE(attributes->decimalLiteral)   = buffer->scale;
        //            DECIMAL_SIGN(attributes->decimalLiteral)    = buffer->sign;
        //            DECIMAL_HI32(attributes->decimalLiteral)    = GET_UNALIGNED_VAL32(&buffer->hi);
        //            DECIMAL_MID32(attributes->decimalLiteral)   = GET_UNALIGNED_VAL32(&buffer->mid);
        //            DECIMAL_LO32(attributes->decimalLiteral)    = GET_UNALIGNED_VAL32(&buffer->low);
        //            attributes->hasDecimalLiteral = true;
        //        }
        //        break;
        //
        //    case CUSTOMATTR_DEPRECATEDHACK:
        //        attributes->isDeprecated = true;
        //        break;
        //
        //    case CUSTOMATTR_CLSCOMPLIANT:
        //        attributes->hasCLSattribute = true;
        //        attributes->isCLS = ImportCustomAttrArgBool(mod, & pvData, & cbSize);
        //        break;
        //
        //    case CUSTOMATTR_PARAMS:
        //        attributes->isParamListArray = true;
        //        break;
        //
        //    case CUSTOMATTR_REQUIRED:
        //        attributes->hasRequiredAttribute = true;
        //        break;
        //
        //    case CUSTOMATTR_DEFAULTMEMBER2:
        //    case CUSTOMATTR_DEFAULTMEMBER:
        //        attributes->defaultMember = ImportCustomAttrArgString(mod, & pvData, & cbSize);
        //        break;
        //
        //    case CUSTOMATTR_COCLASS:
        //        attributes->CoClassName = ImportCustomAttrArgString(mod, & pvData, & cbSize);
        //        break;
        //
        //    case CUSTOMATTR_FIXEDBUFFER:
        //        {
        //            bool fInvalidSig;
        //            PWSTR szBufferType = ImportCustomAttrArgString(mod, & pvData, & cbSize);
        //            attributes->fixedBuffer = ResolveFullMetadataTypeName(mod, szBufferType, &fInvalidSig);
        //            attributes->fixedBufferElementCount = ImportCustomAttrArgInt(mod, & pvData, & cbSize);
        //            ASSERT(!fInvalidSig || attributes->fixedBuffer == NULL);
        //            if (attributes->fixedBuffer == NULL) {
        //                attributes->fixedBuffer = compiler()->GetReqPredefType(PT_OBJECT);
        //            }
        //        }
        //        break;
        //
        //    case CUSTOMATTR_COMPILATIONRELAXATIONS:
        //        attributes->fCompilationRelaxations = true;
        //        break;
        //
        //    case CUSTOMATTR_RUNTIMECOMPATIBILITY:
        //        {
        //            attributes->fRuntimeCompatibility = true;
        //
        //            int numberOfNamedArguments = ImportCustomAttrArgWORD(mod, &pvData, &cbSize);
        //            for (int i = 0; i < numberOfNamedArguments; i += 1) {
        //                PCWSTR name;
        //                // Don't get the type, as these attributes need to be imported before the predef types are loaded
        //                ImportNamedCustomAttrArg(mod, &pvData, &cbSize, &name, NULL);
        //                if (!name)
        //                    break;
        //                if (!wcscmp(name, compiler()->namemgr->GetPredefName(PN_WRAPNONEXCEPTIONTHROWS)->text)) {
        //                    attributes->fWrapNonExceptionThrows = (ImportCustomAttrArgBYTE(mod, &pvData, &cbSize) ? true : false);
        //                } else {
        //                    ASSERT(!"Unknown named argument for imported RuntimeCompatibility");
        //                    break;
        //                }
        //            }
        //        }
        //        break;
        //        
        //    case CUSTOMATTR_FRIENDASSEMBLY:
        //        if (infile) {
        //            const WCHAR * psz = ImportCustomAttrArgString(mod, &pvData, &cbSize);
        //            if (psz) {
        //                ImportScopeModule scope(this, mod);
        //                NAME *nameAssemblyRef = compiler()->namemgr->AddString(psz);
        //                int aid = MapAssemblyRefToAid(nameAssemblyRef, scope, true);
        //                if (aid != kaidUnresolved && infile->GetAssemblyID() != aid) {
        //                    infile->AddInternalsVisibleTo(aid, &compiler()->getGlobalSymAlloc());
        //                    if (aid != kaidThisAssembly && infile->GetAssemblyID() != kaidThisAssembly && MatchesThisAssembly(nameAssemblyRef, scope)) {
        //                        compiler()->RecordAssemblyRefToOutput(nameAssemblyRef, scope.GetModule(), true);
        //                        infile->AddInternalsVisibleTo(kaidThisAssembly, &compiler()->getGlobalSymAlloc());
        //                    }
        //                }
        //                attributes->fHasFriends = true;
        //            }
        //        }
        //        break;
        //
        //
        //    default:
        //        break;
        //    }
        //}

        //    bool ImportCustomAttrArgBool(MODULESYM *scope, LPCVOID * ppvData, ULONG * pcbSize);
        //    WCHAR * ImportCustomAttrArgString(MODULESYM *scope, LPCVOID * ppvData, ULONG * pcbSize);
        //    int ImportCustomAttrArgInt(MODULESYM *scope, LPCVOID * ppvData, ULONG * pcbSize);
        //    WORD ImportCustomAttrArgWORD(MODULESYM *scope, LPCVOID * ppvData, ULONG * pcbSize);
        //    BYTE ImportCustomAttrArgBYTE(MODULESYM *scope, LPCVOID * ppvData, ULONG * pcbSize);

        //------------------------------------------------------------
        // IMPORTER.ImportNamedCustomAttrArg
        //
        // Imports a single named custom attribute argument.
        //------------------------------------------------------------
        //    void ImportNamedCustomAttrArg(MODULESYM *scope, LPCVOID *ppvData, ULONG *pcbSize, PCWSTR *pname, TYPESYM **type);
        //void IMPORTER::ImportNamedCustomAttrArg(MODULESYM * scope, LPCVOID *ppvData, ULONG *pcbSize, PCWSTR *pname, TYPESYM **type)
        //{
        //    CorSerializationType fieldOrProp;
        //    fieldOrProp = (CorSerializationType) ImportCustomAttrArgBYTE(scope, ppvData, pcbSize);
        //
        //    //
        //    // read in the type
        //    //
        //    CorSerializationType st = (CorSerializationType) ImportCustomAttrArgBYTE(scope, ppvData, pcbSize);
        //    if (st >= SERIALIZATION_TYPE_BOOLEAN && st <= SERIALIZATION_TYPE_STRING) {
        //        if (type)
        //            *type = compiler()->GetOptPredefType(g_stTOptMap[st], false);
        //    } else {
        //        switch (st) {
        //        case SERIALIZATION_TYPE_SZARRAY:
        //        case SERIALIZATION_TYPE_TYPE:
        //        case SERIALIZATION_TYPE_TAGGED_OBJECT:
        //        default:
        //                ASSERT(!"UNDONE: named custom attr arg type");
        //                if (type)
        //                    *type = NULL;
        //                *pname = NULL;
        //                return;
        //
        //        case SERIALIZATION_TYPE_ENUM:
        //            {
        //                PCWSTR className;
        //                className = ImportCustomAttrArgString(scope, ppvData, pcbSize);
        //                ASSERT(!wcscmp(className, L"System.AttributeTargets"));
        //                if (type)
        //                    *type = compiler()->GetOptPredefType(PT_ATTRIBUTETARGETS, false);
        //            }
        //        }
        //    }
        //
        //    //
        //    // read in the name
        //    //
        //    *pname = ImportCustomAttrArgString(scope, ppvData, pcbSize);
        //}

        //    HRESULT MakeAssemblyName(PCWSTR szName, ULONG cchName, const ASSEMBLYMETADATA & data, LPBYTE pbPublicKey, const ULONG cbPublicKey, const DWORD dwFlags, BSTR * nameAsBSTR, NAME ** nameAsNAME);
        //    MODULESYM * GetUnresolvedModule(ImportScope & scopeSource, mdToken token);
        //    int MapModuleRefToAid(ImportScope & scopeSource, mdModuleRef tkModuleRef);
        //    bool MatchAssemblySimpleName(IAssemblyName * asmnameRef, ImportScope & scopeSource, OUTFILESYM * manifest);
        //    bool MatchAssemblySimpleName(IAssemblyName * asmnameRef, ImportScope & scopeSource, NAME *simpleName);
        //    bool CompareAssemblySimpleNames(LPCWSTR assemblyRef, LPCWSTR assemblyDef);
        //    bool MatchAssemblyPublicKeyToken(IAssemblyName * asmnameRef, ImportScope & scopeSource, OUTFILESYM * manifest);
        //    bool MatchesThisAssembly(NAME * nameAssemblyRef, ImportScope & scopeSource);

        //------------------------------------------------------------
        // IMPORTER.CompareImports
        //
        /// <summary>
        /// Compares two imports and issues appropriate errors for duplicates
        /// if duplicates are found infile2 is 'ignored' by setting it to bogus
        /// returns true if error was reported
        /// </summary>
        //------------------------------------------------------------
        internal bool CompareImports(INFILESYM infile1, INFILESYM infile2)
        {
            // In the EE case we may not have access tot he CompareAssemblyIdentity API
            // But it doesn't matter since in the EE case we only see references to assemblies
            // that are actually loaded

            //if (FAILED(InitFusionAPIs())) return false;

            //AssemblyIdentityComparison aic(m_pfnCompareAssemblyIdentity);
            //HRESULT hr;
            //CheckHR(hr = aic.Compare(GetAssemblyName(infile1), false, GetAssemblyName(infile2), false), infile1);
            //AssemblyComparisonResult result;

            if (infile1.IsAssemblyLoaded && infile2.IsAssemblyLoaded)
            {
                return infile1.AssemblyEx.Equals(infile2.AssemblyEx);
            }
            else if (infile1.IsModuleLoaded && infile2.IsModuleLoaded)
            {
                return infile1.ModuleEx.Equals(infile2.ModuleEx);
            }
            return false;
        }

        //------------------------------------------------------------
        // IMPORTER.CompareImports
        //------------------------------------------------------------
        //    int CompareVersions(ImportScope & scopeSource, IAssemblyName * panRef, INFILESYM * infileCompare);

        //    static HRESULT ComparePartialAssemblyIdentity(IAssemblyName * panRef, IAssemblyName * panDef, BOOL * pfEquivalent, AssemblyComparisonResult * pResult);
        //    typedef HRESULT (__stdcall *PfnCreateAssemblyNameObject)(LPASSEMBLYNAME *ppAssemblyNameObj, PCWSTR szAssemblyName, DWORD dwFlags, LPVOID pvReserved);
        //    HRESULT InitFusionAPIs();

        //------------------------------------------------------------
        // IMPORTER.ParseAssemblyNameNoError
        //
        // Creates a fusion IAssemblyName object from the given string, and does not report an error on failure.
        //------------------------------------------------------------
        //HRESULT ParseAssemblyNameNoError(PCWSTR szAsmName, IAssemblyName **ppAssemblyNameObj);

        //------------------------------------------------------------
        // IMPORTER.ParseAssemblyName
        //
        /// <summary>
        /// <para>(sscli) Creates a fusion IAssemblyName object from the given string</para>
        /// <para>Create System.Reflection.AssemblyName instance from the given string.</para>
        /// </summary>
        //------------------------------------------------------------
        internal AssemblyName ParseAssemblyName(string name)
        {
            try
            {
                return new AssemblyName(name);
            }
            catch (ArgumentException)
            {
            }
            catch (System.IO.FileLoadException)
            {
            }
            return null;
        }
        //    void ParseAssemblyName(PCWSTR szAsmName, ImportScope &scopeSource, IAssemblyName ** ppan);
        //void IMPORTER::ParseAssemblyName(PCWSTR szAsmName, ImportScope & scopeSource, IAssemblyName ** ppan)
        //{
        //    ASSERT(ppan);
        //
        //    *ppan = NULL;
        //    HRESULT hr;
        //
        //    CComPtr<IAssemblyName> tempAssemblyName;
        //    if (FAILED(hr = ParseAssemblyNameNoError(szAsmName, &tempAssemblyName))) {
        //        if (hr == FUSION_E_INVALID_NAME || hr == E_INVALIDARG)
        //            compiler()->Error(scopeSource, WRN_InvalidAssemblyName, szAsmName);
        //        else
        //            CheckHR(hr, scopeSource);
        //    } else {
        //        *ppan = tempAssemblyName.Detach();
        //    }
        //}

        //------------------------------------------------------------
        // IMPORTER.ImportOneTypeForwarder
        //
        //// Given an exported type specified by the token passed in,
        //// this method will first determine if it is a valid type forwarder
        //// and, if so, will create a FWDAGGSYM to represent it and add that sym to the symbol table.
        ////
        //// If the specified type
        ////======================================================================
        //FWDAGGSYM *IMPORTER::ImportOneTypeForwarder(MODULESYM * mod, mdExportedType token)
        //{
        //    if (TypeFromToken(token) != mdtExportedType || token == mdExportedTypeNil)
        //        return NULL;
        //
        //    ImportScopeModule scope(this, mod);
        //
        //    DWORD flags;
        //    WCHAR szFull[MAX_FULLNAME_SIZE]; 
        //    ULONG cchFull;
        //
        //    IMetaDataAssemblyImport *assemblyimport = mod->GetAssemblyImport(compiler());
        //    INFILESYM *infile = mod->getInputFile();
        //
        //    mdToken tkAssemblyRef = mdTokenNil;
        //    mdToken tkTypeDef = mdTokenNil;
        //
        //    // Get namespace, name, and flags for the type.
        //    assemblyimport->GetExportedTypeProps(
        //        token,              // [IN] The ExportedType for which to get the properties.
        //        szFull,             // [OUT] Buffer to fill with name.
        //        lengthof(szFull),   // [IN] Size of buffer in wide chars.
        //        &cchFull,           // [OUT] Actual # of wide chars in name.
        //        &tkAssemblyRef,     // [OUT] mdFile or mdAssemblyRef or mdExportedType.
        //        &tkTypeDef,         // [OUT] TypeDef token within the file.
        //        &flags);            // [OUT] Flags.
        //    CheckTruncation(cchFull, lengthof(szFull), mod);
        //
        //    if (TypeFromToken(tkAssemblyRef) != mdtAssemblyRef || !(flags & tdForwarder))
        //        return NULL;
        //
        //    FWDAGGSYM * fwd = NULL;
        //    PCWSTR pszAgg = szFull;
        //    NSAIDSYM * nsaPar = ResolveParentScope(scope, mdModuleNil, &pszAgg)->AsNSAIDSYM;
        //
        //    if (nsaPar) {
        //        ASSERT(nsaPar->GetAid() == mod->GetModuleID());
        //        NSDECLSYM * nsdPar = GetNSDecl(nsaPar->GetNS(), infile);
        //
        //        NAME * nameAgg;
        //        NAME * nameWithArity;
        //        int cvarFromName; // Incremental arity in name.
        //
        //        // Strip any arity and place name of type in name table.
        //        cvarFromName = StripArityFromName(pszAgg, &nameAgg, &nameWithArity);
        //        ASSERT(!cvarFromName == !nameWithArity);
        //
        //        // Create the symbol for the new forwarder.
        //        fwd = compiler()->getBSymmgr().CreateFwdAgg(nameAgg, nsdPar);
        //        fwd->tokenImport = token;
        //        fwd->module = mod;
        //        fwd->tkAssemblyRef = tkAssemblyRef;
        //        fwd->cvar = cvarFromName;
        //    }
        //
        //    SetSymInCache(scope, token, fwd);
        //
        //    return fwd;
        //}

        //------------------------------------------------------------
        // IMPORTER.ImportTypeForwarders
        //
        //// Import all type forwarders specified in the given module as FWDAGGSYMs.  
        //------------------------------------------------------------
        //void IMPORTER::ImportTypeForwarders(MODULESYM *mod)
        //{
        //    IMetaDataImport *pImport = mod->GetMetaImport(compiler());
        //    IMetaDataAssemblyImport * pmdi = mod->GetAssemblyImport(compiler());
        //
        //    HCORENUM enumTypeDefs = NULL;
        //    ulong ctok;
        //
        //    ASSERT(sizeof(ulong) == sizeof(ULONG));
        //    CheckHR(pmdi->EnumExportedTypes(&enumTypeDefs, NULL, 0, (ULONG *)&ctok), mod);
        //
        //    CheckHR(pImport->CountEnum(enumTypeDefs, (ULONG *)&ctok), mod);
        //
        //    if (!ctok) {
        //        pmdi->CloseEnum(enumTypeDefs);
        //        return;
        //    }
        //
        //    mdToken rgtok[32];
        //
        //    do {
        //        // Get next batch of types.
        //        CheckHR(pmdi->EnumExportedTypes(&enumTypeDefs, rgtok, lengthof(rgtok), (ULONG *)&ctok), mod);
        //
        //        // Process each type.
        //        for (ulong i = 0; i < ctok; i++) {
        //            ImportOneTypeForwarder(mod, rgtok[i]);
        //        }
        //    } while (ctok > 0);
        //
        //    pmdi->CloseEnum(enumTypeDefs);
        //    return;
        //}

        // Helpers for ImportOneType to save some stack space in that method

        //------------------------------------------------------------
        // IMPORTER::ImportOneType_SetAggKind (sscli)
        //
        /// <summary>
        /// Helper for ImportOneType.
        /// Extrated out of ImportOneType to save some stack space in that function and
        /// help prevent stack overflows for very deeply nested classes.
        /// </summary>
        //------------------------------------------------------------
        //void IMPORTER::ImportOneType_SetAggKind(
        //    /*inout*/ AGGSYM * agg,
        //    MODULESYM * mod,
        //    LPCWSTR szFull,
        //    mdToken tkExtends,
        //    bool cvarEqualcvarOuter,
        //    DWORD flags )
        //{
        //    // Check to see if we extend System.ValueType, and are thus a value type.
        //    WCHAR baseClassText[MAX_FULLNAME_SIZE]; // name of the base class.
        //
        //    // Default to class.
        //    agg->SetAggKind(AggKind::Class);
        //
        //    if (!IsNilToken(tkExtends) && GetTypeRefFullName(mod, tkExtends, baseClassText, lengthof(baseClassText))) {
        //        if (!wcscmp(baseClassText, compiler()->getBSymmgr().GetFullName(PT_ENUM))) {
        //            agg->SetAggKind((cvarEqualcvarOuter) ? AggKind::Enum : AggKind::Struct);
        //            agg->isSealed = true;
        //        }
        //        else if (!(flags & tdAbstract) &&
        //            (!wcscmp(baseClassText, compiler()->getBSymmgr().GetFullName(PT_MULTIDEL)) ||
        //            !wcscmp(baseClassText, compiler()->getBSymmgr().GetFullName(PT_DELEGATE)) &&
        //            wcscmp(szFull, compiler()->getBSymmgr().GetFullName(PT_MULTIDEL))))
        //        {
        //            // We'll verify later that it has the correct constructor and an invoke method.
        //            agg->SetAggKind(AggKind::Delegate);
        //            agg->isSealed = !!(flags & tdSealed);
        //        }
        //        else if (!wcscmp(baseClassText, compiler()->getBSymmgr().GetFullName(PT_VALUE)) &&
        //            wcscmp(szFull, compiler()->getBSymmgr().GetFullName(PT_ENUM)))
        //        {
        //            agg->SetAggKind(AggKind::Struct);
        //            agg->isSealed = true;
        //        }
        //    }
        //
        //    if (agg->AggKind() == AggKind::Class) {
        //        agg->isAbstract = !!(flags & tdAbstract);
        //        agg->isSealed = !!(flags & tdSealed);
        //    }
        //}

        //------------------------------------------------------------
        // IMPORTER.ImportOneType_ProcessTypeVariable
        //------------------------------------------------------------
        //bool IMPORTER::ImportOneType_ProcessTypeVariable(
        //    IMetaDataImport2 * metaimportV2,
        //    mdGenericParam tokVar,
        //    INFILESYM *infile,
        //    LONG cvar,
        //    LONG cvarOuter,
        //    TYVARSYM ** prgvar,
        //    BAGSYM * bagPar,
        //    AGGSYM * agg )
        //{
        //    LONG ivar;
        //    DWORD flagsTypeVar;
        //    mdToken tokPar;
        //    WCHAR rgchName[MAX_FULLNAME_SIZE];
        //    ULONG cchName;
        //
        //    CheckHR(metaimportV2->GetGenericParamProps(
        //        tokVar,
        //        (ULONG *)&ivar,
        //        &flagsTypeVar,
        //        &tokPar,
        //        NULL,
        //        rgchName,
        //        lengthof(rgchName),
        //        &cchName),
        //        infile);
        //
        //    if (ivar < 0 || ivar >= cvar || prgvar[ivar]) {
        //        // Bad or duplicate index.
        //        return false;
        //    }
        //
        //    if (ivar < cvarOuter) {
        //        prgvar[ivar] = bagPar->AsAGGSYM->typeVarsAll->ItemAsTYVARSYM(ivar);
        //        ASSERT(prgvar[ivar]->indexTotal == ivar);
        //    }
        //    else {
        //        NAME * name = compiler()->namemgr->AddString(rgchName);
        //        TYVARSYM * var = compiler()->getBSymmgr().CreateTyVar(name, agg);
        //        ASSERT(!var->isMethTyVar);
        //        var->SetAccess(ACC_PRIVATE);
        //        var->indexTotal = (short)ivar;
        //        var->index = (short)(ivar - cvarOuter);
        //        var->parseTree = NULL;
        //        prgvar[ivar] = var;
        //    }
        //
        //    return true;
        //}

        //------------------------------------------------------------
        // IMPORTER.ImportNamespaces (1)
        //
        /// <summary>
        /// Create namespace declarations for all the namespaces in the given infile.
        /// If fTypes is true, also load the types.
        /// Otherwise, record all the type defs for deferred loading.
        /// </summary>
        /// <param name="inFileSym"></param>
        /// <param name="loadTypes">If true, record all the TypeDef.</param>
        //------------------------------------------------------------
        internal void ImportNamespaces(INFILESYM inFileSym, bool loadTypes)
        {
            if (inFileSym == null)
            {
                return;
            }

            // Import all types for added modules because they will be emitted to this assembly and 
            // we need to make sure we do not have duplicate types
            // (i.e. a declared type and an exported type to the module).

            if (inFileSym.IsAssemblyLoaded)
            {
                ImportNamespaces(inFileSym, inFileSym.AssemblyEx.GetAllTypes(), loadTypes);
            }
            else if (inFileSym.IsModuleLoaded)
            {
                // In sscli, check and set some module attributes.

                ImportNamespaces(inFileSym, inFileSym.ModuleEx.GetAllTypes(), true);
            }
        }

        //------------------------------------------------------------
        // IMPORTER.ImportNamespaces (2)
        //
        /// <summary>
        /// <para>Create namespace declarations for all the namespaces in the given module.
        /// If fTypes is true, also load the types.
        /// Otherwise, record all the type defs for deferred loading.
        /// Asks the global symbol table whether a type needs to be pre-loaded.</para>
        /// <para>This is for predefined types.</para>
        /// <para>Assume that an assembly or a module already loaded. if not, an exception occuers.</para>
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="types"></param>
        /// <param name="loadTypes"></param>
        //------------------------------------------------------------
        internal void ImportNamespaces(INFILESYM infileSym, Type[] types, bool loadTypes)
        {
            if (infileSym == null || types == null || types.Length == 0)
            {
                return;
            }

            //--------------------------------------------------
            // If loadTypes == true, load all the types.
            //--------------------------------------------------
            if (loadTypes)
            {
                CModuleEx moduleEx = infileSym.ModuleEx;

                foreach (Type type in types)
                {
                    AGGSYM aggSym = this.ImportOneType(infileSym, type) as AGGSYM;

                    if (moduleEx != null && aggSym!=null)
                    {
                        moduleEx.AggSymList.Add(aggSym);
                    }
                }
            }
            //--------------------------------------------------
            // If loadTypes == false,
            //--------------------------------------------------
            else
            {
                DebugUtil.Assert(infileSym.MetadataFile != null);
                CNameToTypeDictionary typeDic = infileSym.MetadataFile.NameToTypeDictionary;
                SYM parentSym = null;

                foreach (Type type in types)
                {
                    // aid doesn't matter (sscli)
                    NSAIDSYM nsaidSym = ResolveNamespace(type.Namespace, Kaid.Global);
                    if (nsaidSym == null)
                    {
                        continue;
                    }
                    NSSYM nsSym = nsaidSym.NamespaceSym;
                    DebugUtil.Assert(nsSym != null);
                    // Tell the namespace that this module has some types in it.
                    // This happens by simply making sure the NSDECLSYM exists.
                    GetNsDecl(nsSym, infileSym);
                    DebugUtil.Assert(nsSym.InAlias(infileSym.GetAssemblyID()));

                    // Nested classes are handled differently.
                    if (type.IsNested)
                    {
                        // Get the class this class is nested within.
                        Type outerType = type.DeclaringType;
                        parentSym = GetAggFromCache(outerType);

                        if (parentSym == null)
                        {
                            ImportOneType(infileSym, type);
                            parentSym = GetAggFromCache(outerType);
                        }
                        DebugUtil.Assert(parentSym != null);
                    }
                    else
                    {
                        parentSym = nsSym;
                    }

                    if (Compiler.MainSymbolManager.FPreLoad(nsSym, type.Name))
                    {
                        ImportOneType(infileSym, type);
                        continue;
                    }

                    string nameWithoutArity = StripArityFromName(type.Name);
                    if (!String.IsNullOrEmpty(nameWithoutArity))
                    {
                        typeDic.Add(parentSym, nameWithoutArity, type);
                        nsSym.SetTypesUnloaded(infileSym.GetAssemblyID());
                    }
                }
            }
        }

        //------------------------------------------------------------
        // IMPORTER.SortModTypeInfos
        //------------------------------------------------------------
        //void SortModTypeInfos(ModTypeInfo * prgmti, int cmti);

#if false
        //------------------------------------------------------------
        // IMPORTER.ImportMemberInAgg
        //
        /// <summary>
        /// <para>Import members with the specified name in the specified type.</para>
        /// <para>If some members are added to the symbol table, return true.</para>
        /// <para>Do not call this method first.
        /// If not found in the symbol table, then call this method and search again.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="aggSym"></param>
        /// <param name="symMask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ImportMemberInAgg(
            string name,
            AGGSYM aggSym,
            SYMBMASK symMask)
        {
            DebugUtil.Assert(aggSym != null && !String.IsNullOrEmpty(name));

            if (aggSym.TypeBuilder != null)
            {
                return false;
            }
            if (aggSym.Type == null)
            {
                return false;
            }

            MemberInfo[] memberInfos = aggSym.Type.GetMembers();
            if (memberInfos.Length == 0)
            {
                return false;
            }

            bool isImported = false;
            foreach (MemberInfo info in memberInfos)
            {
                if (info.Name != name)
                {
                    continue;
                }
                if (Compiler.MainSymbolManager.InfoToSymbolTable.Contains(info))
                {
                    continue;
                }

                //----------------------------------------------------
                // Field
                //----------------------------------------------------
                if ((symMask & SYMBMASK.MEMBVARSYM) != 0)
                {
                    FieldInfo fInfo = info as FieldInfo;
                    if (fInfo == null)
                    {
                        goto END_FIELD;
                    }

                    AGGDECLSYM declSym = aggSym.FirstDeclSym;
                    DebugUtil.Assert(declSym != null);

                    MEMBVARSYM fieldSym = ImportField(
                        declSym.GetInputFile(),
                        aggSym,
                        declSym,
                        fInfo);

                    if (fieldSym != null)
                    {
                        isImported = true;
                    }

                END_FIELD: ;
                }

                //----------------------------------------------------
                //
                //----------------------------------------------------
                //if ((symMask & SYMBMASK.LOCVARSYM) != 0)
                //{
                //}

                //----------------------------------------------------
                // Method, Constructor
                //----------------------------------------------------
                if ((symMask & SYMBMASK.METHSYM) != 0)
                {
                    MethodBase methodBase = info as MethodBase;
                    if (methodBase == null)
                    {
                        goto END_METHOD;
                    }

                    MethodInfo mInfo = info as MethodInfo;
                    ConstructorInfo cInfo = null;

                    if (mInfo != null)
                    {
                        methodBase = mInfo;
                    }
                    else
                    {
                        cInfo = info as ConstructorInfo;
                        if (cInfo != null)
                        {
                            methodBase = cInfo;
                        }
                    }

                    if (methodBase == null)
                    {
                        goto END_METHOD;
                    }

                    AGGDECLSYM declSym = aggSym.FirstDeclSym;
                    DebugUtil.Assert(declSym != null);

                    METHSYM methodSym = ImportMethod(
                        declSym.GetInputFile(),
                        aggSym,
                        declSym,
                        methodBase);

                    if (methodSym != null)
                    {
                        isImported = true;
                    }

                END_METHOD: ;
                }

                //----------------------------------------------------
                // Property
                //----------------------------------------------------
                if ((symMask & SYMBMASK.PROPSYM) != 0)
                {
                    PropertyInfo pInfo = info as PropertyInfo;
                    if (pInfo == null)
                    {
                        goto END_PROPERTY;
                    }

                    AGGDECLSYM declSym = aggSym.FirstDeclSym;
                    DebugUtil.Assert(declSym != null);

                    PROPSYM propertySym = ImportProperty(
                        declSym.GetInputFile(),
                        aggSym,
                        declSym,
                        pInfo,
                        null);

                    if (propertySym != null)
                    {
                        isImported = true;
                    }

                END_PROPERTY: ;
                }

                //----------------------------------------------------
                //
                //----------------------------------------------------
                //if ((symMask & SYMBMASK.TYVARSYM) != 0)
                //{
                //}

                //----------------------------------------------------
                // Event
                //----------------------------------------------------
                if ((symMask & SYMBMASK.EVENTSYM) != 0)
                {
                    EventInfo eInfo = info as EventInfo;
                    if (eInfo == null)
                    {
                        goto END_EVENT;
                    }

                END_EVENT: ;
                }

                //----------------------------------------------------
                //
                //----------------------------------------------------
                //if ((symMask & SYMBMASK.FAKEMETHSYM) != 0)
                //{
                //}
            }
            return isImported;
        }
#endif
    }
}
