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
// File: asmlink.h
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
// File: asmlink.cpp
//
// ===========================================================================

//============================================================================
// AsmLink.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

// palrt\inc\rotor_palrt.h(240):
// #define STDMETHODCALLTYPE    __stdcall

// palrt\inc\rotor_palrt.h(252-):
// #define STDMETHOD(method)       virtual HRESULT STDMETHODCALLTYPE method
// #define STDMETHOD_(type,method) virtual type STDMETHODCALLTYPE method

namespace Uncs
{
    //======================================================================
    // CAsmLink
    //
    /// <summary>
    /// This class is the real assembly linker.
    /// </summary>
    /// <remarks>In sscli, derives
    /// : public CComObjectRoot,
    ///   public CComCoClass<CAsmLink, &CLSID_AssemblyLinker>,
    ///   public ISupportErrorInfoImpl< &IID_IALink>,
    ///   public IALink2
    /// IALink2 is derived by CAsmLink only,
    /// </remarks>
    //======================================================================
    internal class CAsmLink
    {
        // friend class CFile;

        //------------------------------------------------------------
        // CAsmLink Fields and Properties
        //------------------------------------------------------------
        private CController controller = null;

        internal CController Controller
        {
            get { return this.controller; }
        }

        private CAssemblyBuilderEx assemblyBuilderEx = null;    // CAssembly *m_pAssem;
        // private CAssembly *m_pImports;
        // private CAssembly *m_pModules;
        private CAssemblyEx msCorLib = null;  // *m_pStdLib;

        /// <summary>
        /// (R) CAssembly instance of mscorlib.dll
        /// </summary>
        internal CAssemblyEx MsCorLib
        {
            get { return this.msCorLib; }   // protected CAssembly *GetStdLib();
        }

        /// <summary>
        /// Used to import modules. (instead of m_pModules in sscli).
        /// </summary>
        private CAssemblyBuilderEx tempAssemblyBuilder = null;

        private const string tempAssemblyBuilderName = "_temp";

        /// <summary>
        /// Store the referenced assemblies.
        /// </summary>
        private CDictionary<CAssemblyEx> importedAssemblyDic = new CDictionary<CAssemblyEx>();

        /// <summary>
        /// (R) Store the referenced assemblies.
        /// </summary>
        internal CDictionary<CAssemblyEx> ImportedAssemblyDic
        {
            get { return this.importedAssemblyDic; }
        }

        /// <summary>
        /// Store the referenced modules.
        /// </summary>
        private CDictionary<CModuleEx> importedModuleDic = new CDictionary<CModuleEx>();

        /// <summary>
        /// (R) Store the referenced modules.
        /// </summary>
        internal CDictionary<CModuleEx> ImportedModuleDic
        {
            get { return this.importedModuleDic; }
        }

        private bool inited = false;            // m_bInited            : 1;    // true if m_pDisp is valid. (sscli)
        private bool preClosed = false;         // m_bPreClosed         : 1;
        private bool manifestEmitted = false;   // m_bManifestEmitted   : 1;
        private bool assemblyEmitted = false;   // m_bAssemblyEmitted   : 1;
        private bool dontDoHashes = false;      // m_bDontDoHashes      : 1;

        //------------------------------------------------------------
        // CAsmLink Constructor
        //
        /// <summary></summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal CAsmLink(CController cntr)
        {
            DebugUtil.Assert(cntr != null);
            this.controller = cntr;
        }

        //------------------------------------------------------------
        // CAsmLink.Init (1)
        //
        /// <summary>
        /// <para>This method does not set this.inited.</para>
        /// </summary>
        //------------------------------------------------------------
        //internal void Init()
        //{
        //}

        //------------------------------------------------------------
        // CAsmLink::Init (sscli) (1)
        //------------------------------------------------------------
        //HRESULT CAsmLink::Init()
        //{
        //    HRESULT hr = S_OK;
        //    m_pImports = new CAssembly();
        //    if (m_pImports == NULL) {
        //        hr = E_OUTOFMEMORY;
        //        goto CLEANUP;
        //    }
        //    if (FAILED(hr = m_pImports->Init(NULL, this, NULL)))
        //        goto CLEANUP;
        //    
        //    m_pModules = new CAssembly();
        //    if (m_pModules == NULL) {
        //        hr = E_OUTOFMEMORY;
        //        goto CLEANUP;
        //    }
        //    hr = m_pModules->Init(NULL, this, NULL);
        //
        //CLEANUP:
        //    if (FAILED(hr)) {
        //        if (m_pImports != NULL) {
        //            delete m_pImports;
        //            m_pImports = NULL;
        //        }
        //        if (m_pModules != NULL) {
        //            delete m_pImports;
        //            m_pImports = NULL;
        //        }
        //    }
        //
        //    return hr;
        //}

        // virtual internal HRESULT  InterfaceSupportsErrorInfo(REFIID riid);

        // Interface methods here

        //------------------------------------------------------------
        // CAsmLink.Init (2)
        //
        /// <summary>
        /// <para>In sscli, if IMetaDataDispenserEx is valid, this.m_bInited is set.</para>
        /// </summary>
        /// <param name="error"></param>
        //------------------------------------------------------------
        internal void Init()
        {
            this.inited = true;
        }

        //------------------------------------------------------------
        // CAsmLink::Init (sscli) (2)
        //------------------------------------------------------------
        //virtual internal HRESULT  Init(IMetaDataDispenserEx *pDispenser, IMetaDataError *pError);
        //{
        //
        //#ifdef _DEBUG
        //    DEBUG_BEGIN_IF_NZ("Break")
        //        DebugBreak();
        //    DEBUG_END_IF
        //#endif
        //    VARIANT v;
        //    HRESULT hr;
        //    ASSERT(!m_bInited && pDispenser != NULL);
        //    m_pDisp = pDispenser;
        //    m_pDisp->AddRef();
        //
        //    VariantInit(&v);
        //    if (SUCCEEDED(hr = m_pDisp->GetOption(MetaDataCheckDuplicatesFor, &v))) {
        //        V_UI4(&v) |= MDDupModuleRef | MDDupAssemblyRef | MDDupAssembly | MDDupManifestResource | MDDupExportedType;
        //        hr = m_pDisp->SetOption(MetaDataCheckDuplicatesFor, &v);
        //    }
        //
        //    if (pError != NULL) {
        //        m_pError = pError;
        //        m_pError->AddRef();
        //    } else
        //        m_pError = NULL;
        //    m_bInited = true;
        //    return hr;
        //}

        //------------------------------------------------------------
        // CAsmLink.SetAssemblyFile (1)
        //
        /// <summary>
        /// Create a CAssemblyBuilder instance.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetAssemblyFile(
            string fileName,
            AssemblyFlagsEnum flags)
        {
            if (fileName == null)
            {
                fileName = "";
            }

            this.assemblyBuilderEx = new CAssemblyBuilderEx(this.controller);
            return this.assemblyBuilderEx.Init(flags, this);
        }
        //------------------------------------------------------------
        // CAsmLink::SetAssemblyFile (sscli)
        //
        // #define TokenFromRid(rid,tktype) ((rid) | (tktype)) : src\inc\corhdr.h(1411)
        //------------------------------------------------------------
        //virtual internal HRESULT SetAssemblyFile(
        //    LPCWSTR pszFilename,
        //    IMetaDataEmit *pEmitter,
        //    AssemblyFlags afFlags,
        //    mdAssembly *pAssemblyID)
        //{
        //    ASSERT(m_bInited && !m_bAssemblyEmitted && !m_bPreClosed && !m_pAssem);
        //    ASSERT(pEmitter != NULL && pAssemblyID != NULL);
        //    HRESULT hr = E_FAIL;
        //    
        //    // NULL filename means 'InMemory' assembly, but we need a filename so convert it to the
        //    // empty string.
        //    if (pszFilename == NULL) {
        //        ASSERT(afFlags & afInMemory);
        //        pszFilename = L"";
        //    }
        //
        //    if (FAILED(hr = SetNonAssemblyFlags(afFlags)))
        //        return hr;
        //
        //    if (wcslen(pszFilename) > MAX_PATH)
        //        return FileNameTooLong(pszFilename); // File name too long
        //
        //    CComPtr<IMetaDataAssemblyEmit> pAEmitter;
        //    hr = pEmitter->QueryInterface(IID_IMetaDataAssemblyEmit, (void**)&pAEmitter);
        //    if (SUCCEEDED(hr)) {
        //        m_pAssem = new CAssembly();
        //        if (m_pAssem == NULL) {
        //            hr = E_OUTOFMEMORY;
        //        } else if (SUCCEEDED(hr = m_pAssem->Init(pszFilename, afFlags, pAEmitter, pEmitter, m_pError, this))) {
        //            *pAssemblyID = TokenFromRid(mdtAssembly, 1);
        //        }
        //    }
        //
        //    return hr;
        //}

        //------------------------------------------------------------
        // CAsmLink.SetAssemblyFile (2)
        //
        /// <summary></summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetAssemblyFile(CAssemblyBuilderEx builder)
        {
            this.assemblyBuilderEx = builder;
            return this.assemblyBuilderEx != null;
        }

        //------------------------------------------------------------
        // CAsmLink.
        //------------------------------------------------------------
        // virtual internal HRESULT  SetAssemblyFile2(
        //    LPCWSTR pszFilename,
        //    IMetaDataEmit2 *pEmitter,
        //    AssemblyFlags afFlags,
        //    mdAssembly *pAssemblyID);

        //------------------------------------------------------------
        // CAsmLink.CreateTempAssemblyBuilder
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void CreateTempAssemblyBuilder()
        {
            Exception excp = null;

            this.tempAssemblyBuilder = new CAssemblyBuilderEx(this.controller);
            this.tempAssemblyBuilder.Init(AssemblyFlagsEnum.None, this);
            this.tempAssemblyBuilder.DefineAssembly(
                tempAssemblyBuilderName,
                null,
                AssemblyBuilderAccess.RunAndSave,
                this.controller.OptionManager,
                null,
                null,
                null,
                null,
                out excp);
        }

        //------------------------------------------------------------
        // CAsmLink.SetNonAssemblyFlags
        //
        // If afNoRefHash is specified and this.m_bDontDoHashes is not set,
        // set m_bDoHash false for all CAssembly instants in m_pImports,
        // If failed to get a CFile instance of a CAssembly instant, return error code.
        //------------------------------------------------------------
        //virtual internal HRESULT SetNonAssemblyFlags(AssemblyFlags afFlags)
        //{
        //    ASSERT(m_bInited && !m_bAssemblyEmitted && !m_bPreClosed);
        //    HRESULT hr = S_FALSE;
        //    
        //    if (afFlags & afNoRefHash) {
        //        if (!m_bDontDoHashes) {
        //            // This just changed, so tell all the previously imported assemblies
        //            DWORD cnt = m_pImports->CountFiles(); 
        //            for (DWORD d = 0; d < cnt; d++) {
        //                CAssembly *pFile = NULL;
        //                if (FAILED(hr = m_pImports->GetFile(d, (CFile**)&pFile)))
        //                    return hr;
        //                pFile->DontDoHash();
        //            }
        //        }
        //        m_bDontDoHashes = true;
        //    }
        //
        //    return hr;
        //}

        // Files and importing

        //------------------------------------------------------------
        // CAsmLink.ImportFile  (Do not use, call ImportFileEx2)
        //------------------------------------------------------------
        // virtual internal HRESULT  ImportFile(
        //    LPCWSTR pszFilename,
        //    LPCWSTR pszTargetName,
        //    BOOL fSmartImport,
        //    mdToken *pFileToken,
        //    IMetaDataAssemblyImport **ppAssemblyScope,
        //    DWORD *pdwCountOfScopes)
        //{
        //    return ImportFileEx2(
        //        pszFilename,
        //        pszTargetName,
        //        NULL,
        //        fSmartImport,
        //        ofReadOnly | ofNoTypeLib,
        //        pFileToken,
        //        ppAssemblyScope,
        //        pdwCountOfScopes);
        //}

        //------------------------------------------------------------
        // CAsmLink.ImportFile2 (Do not use, call ImportFileEx2)
        //------------------------------------------------------------
        // virtual internal HRESULT  ImportFile2(
        //    LPCWSTR pszFilename,
        //    LPCWSTR pszTargetName,
        //    IMetaDataAssemblyImport *pAssemblyScopeIn,
        //    BOOL fSmartImport,
        //    mdToken *pFileToken,
        //    IMetaDataAssemblyImport **ppAssemblyScope,
        //    DWORD *pdwCountOfScopes)
        //{
        //    return ImportFileEx2(
        //        pszFilename,
        //        pszTargetName,
        //        pAssemblyScopeIn,
        //        fSmartImport,
        //        ofReadOnly | ofNoTypeLib,
        //        pFileToken,
        //        ppAssemblyScope,
        //        pdwCountOfScopes);
        //}

        //------------------------------------------------------------
        // CAsmLink.ImportFileEx    (Do not use, call ImportFileEx2)
        //------------------------------------------------------------
        // virtual internal HRESULT  ImportFileEx(
        //    LPCWSTR pszFilename,
        //    LPCWSTR pszTargetName,
        //    BOOL fSmartImport,
        //    DWORD dwOpenFlags,
        //    mdToken *pFileToken,
        //    IMetaDataAssemblyImport **ppAssemblyScope,
        //    DWORD *pdwCountOfScopes)
        //{
        //    return ImportFileEx2(
        //        pszFilename,
        //        pszTargetName,
        //        NULL,
        //        fSmartImport,
        //        dwOpenFlags,
        //        pFileToken,
        //        ppAssemblyScope,
        //        pdwCountOfScopes);
        //}

        //------------------------------------------------------------
        // CAsmLink.ImportFileEx2
        //
        /// <summary>
        /// Create a CModuleEx instance or CAssemblyEx instance for a given INFILESYM.
        /// </summary>
        /// <param name="infileSym"></param>
        /// <param name="targetName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ImportFileEx2(INFILESYM infileSym, string targetName)
        {
            //DebugUtil.Assert(m_bInited);
            if (infileSym == null || infileSym.FileInfo == null)
            {
                return false;
            }

            string newTarget = null;
            bool isNewTarget = false;
            Exception excp = null;

            //--------------------------------------------------
            // Check if pszFilename is valid.
            // Then, if pszTargetName is NULL, assign pszFilename to it.
            //--------------------------------------------------
            FileInfo fileInfo = infileSym.FileInfo;
            FileInfo targetInfo = null;

            if (!String.IsNullOrEmpty(targetName))
            {
                if (!IOUtil.CreateFileInfo(targetName, out targetInfo, out excp))
                {
                    return false;
                }
            }

            if (targetInfo == null || String.IsNullOrEmpty(targetInfo.Name))
            {
                newTarget = infileSym.Name;
                isNewTarget = false;
            }
            else
            {
                newTarget = targetName;
                isNewTarget = (String.Compare(infileSym.Name, newTarget, true) != 0);
            }

            // If cassembly is null, create from fileName.

            //--------------------------------------------------
            // Module
            //
            // If the file is a module, load it as a module of this.assemblyBuilder.
            // And create CModule instance, add to this.importedModuleDic
            //--------------------------------------------------
            if (infileSym.IsModule)
            {
                if (infileSym.IsModuleLoaded)
                {
                    return true;
                }
                if (this.tempAssemblyBuilder == null)
                {
                    CreateTempAssemblyBuilder();
                }

                CModuleEx cmod = this.tempAssemblyBuilder.AddModule(
                    infileSym,
                    null,
                    targetName,
                    false);
                if (cmod == null)
                {
                    return false;
                }
                if (!this.importedModuleDic.Add(cmod.Name, cmod))
                {
                    // If the name is already registered, CDictionary.Add returns false.
                    // Show warning and contiue processing.
                }
                return true;
            }
            //--------------------------------------------------
            // Assembly
            //--------------------------------------------------
            else if (infileSym.IsAssembly)
            {
                if (infileSym.IsAssemblyLoaded)
                {
                    return true;
                }

                CAssemblyEx casm = new CAssemblyEx(this.controller);
                if (!casm.Init(infileSym.FileInfo, this))
                {
                    return false;
                }
                //if (this.m_bDontDoHashes == true) casm.DontDoHash();
                //casm.SetInMemory();
                if (casm.ImportAssembly(true))
                {
                    if (isNewTarget)
                    {
                        this.controller.ReportError(
                            ALERRID.ERR_CantRenameAssembly,
                            ERRORKIND.ERROR,
                            infileSym.Name);
                    }

                    if (!this.importedAssemblyDic.Add(casm.FileFullName, casm))
                    {
                        // Duplicate. Show warning.
                        infileSym.SetMetadataFile(importedAssemblyDic[casm.FileFullName]);
                    }
                    else
                    {
                        infileSym.SetMetadataFile(casm);
                        if (casm.IsStdLib)
                        {
                            this.msCorLib = casm;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // CAsmLink::ImportFileEx2 (sscli)
        //------------------------------------------------------------
        //virtual internal HRESULT  ImportFileEx2(
        //    LPCWSTR pszFilename,
        //    LPCWSTR pszTargetName,
        //    IMetaDataAssemblyImport *pAssemblyScopeIn, 
        //    BOOL fSmartImport,
        //    DWORD dwOpenFlags,
        //    mdToken *pFileToken,
        //    IMetaDataAssemblyImport **ppAssemblyScope, 
        //    DWORD *pdwCountOfScopes)
        //{
        //    ASSERT(m_bInited);
        //    LPWSTR newTarget = NULL;
        //    bool bNewTarget = false;
        //    bool fIsInMemory = (pAssemblyScopeIn != NULL);
        //
        //    IMetaDataAssemblyImport *pAImport = NULL;
        //    IMetaDataImport *pImport = NULL;
        //    mdAssembly tkAssembly;
        //
        //    //--------------------------------------------------
        //    // Check if pszFilename is valid.
        //    // If pszTargetName is NULL, assign pszFilename to it.
        //    //--------------------------------------------------
        //    if (pszFilename == NULL)
        //        return E_INVALIDARG;
        //    if (wcslen(pszFilename) > MAX_PATH)
        //        return FileNameTooLong(pszFilename);
        //    if (pszTargetName && wcslen(pszTargetName) > MAX_PATH)
        //        return FileNameTooLong(pszTargetName); // File name too long
        //
        //    if (pszTargetName == NULL)
        //        newTarget = VSAllocStr(pszFilename);
        //    else
        //        newTarget = VSAllocStr(pszTargetName);
        //    if (newTarget == NULL)
        //        return E_OUTOFMEMORY;
        //    
        //    if (_wcsicmp(newTarget, pszFilename) == 0) {
        //    } else {
        //        bNewTarget = true;
        //    }
        //
        //    //--------------------------------------------------
        //    // If pAssemblyScopeIn is valid, use it.
        //    //--------------------------------------------------
        //    if ((pAImport = pAssemblyScopeIn) != NULL)
        //    {
        //        //EDMAURER If bNewTarget is true and the source file is not an assembly, then pAImport 
        //        //must be an object from which one can QI for IID_IMetaDataEmit. And it may not be 
        //        //depending on the openflags used when it was created. 
        //        //Becomes an issue with UpdateModuleName () below.
        //        pAImport->AddRef();
        //    }
        //    //--------------------------------------------------
        //    // If pAssemblyScopeIn is null, search the RegMeta instance by pszFilename,
        //    // if not found, create a RegMeta instace, and use it as pAImport.
        //    //--------------------------------------------------
        //    else
        //    {
        //        if (bNewTarget && IsOfReadOnly (dwOpenFlags)) {
        //            //EDMAURER would like to also check for !IsAssembly (pszFilename) in this IF.
        //            //Massage OpenFlags such that ofReadOnly is not used. Fall back on ofRead
        //            //which was used before the introduction of ofReadOnly. That permits QI for IMetadataEmit.
        //            //This is done to satisfy the UpdateModuleName () call below.
        //            dwOpenFlags &= ~ofReadOnly;
        //            dwOpenFlags |= ofRead;
        //        }
        //
        //        // Search or create the RegMeta instance.
        //        // The metadata section is opened in the RegMeta instance.
        //        HRESULT hr = m_pDisp->OpenScope(pszFilename, dwOpenFlags, IID_IMetaDataAssemblyImport, (IUnknown**)&pAImport);
        //        if (FAILED(hr)) {
        //            *pFileToken = 0;
        //            if (ppAssemblyScope) *ppAssemblyScope = NULL;
        //            if (pdwCountOfScopes) *pdwCountOfScopes = 0;
        //            delete [] newTarget;
        //            return hr;
        //        }
        //    }
        //
        //    HRESULT hr;
        //    if (FAILED(hr = pAImport->QueryInterface( IID_IMetaDataImport, (void**)&pImport))) {
        //        *pFileToken = 0;
        //        if (ppAssemblyScope) *ppAssemblyScope = NULL;
        //        if (pdwCountOfScopes) *pdwCountOfScopes = 0;
        //        delete [] newTarget;
        //        return hr;
        //    }
        //
        //    //--------------------------------------------------
        //    // this file is not an assembly (does not have a manifest).
        //    //--------------------------------------------------
        //    if (FAILED(hr = pAImport->GetAssemblyFromScope( &tkAssembly))) {
        //
        //        hr = S_FALSE;
        //        // This is NOT an assembly
        //        if (ppAssemblyScope) *ppAssemblyScope = NULL;
        //        if (pdwCountOfScopes) *pdwCountOfScopes = 1;
        //        *pFileToken = mdTokenNil;
        //        CFile* file = new CFile();
        //        if (file == NULL) {
        //            hr = E_OUTOFMEMORY;
        //        } else if (SUCCEEDED(hr = file->Init(newTarget, pImport, m_pError, this))) {
        //            if (bNewTarget) {
        //                if (SUCCEEDED(hr = file->SetSource(pszFilename)))
        //                    hr = file->UpdateModuleName();
        //            }
        //        } else {
        //            delete file;
        //            file = NULL;
        //        }
        //        pAImport->Release();
        //        pImport->Release();
        //
        //        if (SUCCEEDED(hr) && SUCCEEDED(hr = m_pModules->AddFile( file, 0, pFileToken))) {
        //            if (fSmartImport)
        //                hr = file->ImportFile( NULL, NULL);
        //            *pFileToken = TokenFromRid(RidFromToken(*pFileToken), mdtModule);
        //        } else if (file != NULL) {
        //            delete file;
        //            file = NULL;
        //        }
        //        if (ppAssemblyScope) *ppAssemblyScope = NULL;
        //        if (SUCCEEDED(hr) && pdwCountOfScopes) *pdwCountOfScopes = 1;
        //        delete [] newTarget;
        //        return hr == S_OK ? S_FALSE : hr;
        //    }
        //    //--------------------------------------------------
        //    // this file is an assembly (has a manifest).
        //    //--------------------------------------------------
        //    else {
        //        // It is an Assembly
        //        CAssembly* assembly = new CAssembly();
        //        if (assembly == NULL) {
        //            hr = E_OUTOFMEMORY;
        //        } else if (SUCCEEDED(hr = assembly->Init( pszFilename, pAImport, pImport, m_pError, this))){
        //            if (ppAssemblyScope)
        //                *ppAssemblyScope = pAImport;
        //            else
        //                pAImport->Release();
        //            if (m_bDontDoHashes)
        //                assembly->DontDoHash();
        //            if (fIsInMemory)
        //                assembly->SetInMemory();
        //            // Get the informations of assembly, custum attributes, types.
        //            hr = assembly->ImportAssembly(pdwCountOfScopes, fSmartImport, dwOpenFlags, m_pDisp);
        //        } else {
        //            delete assembly;
        //            assembly = NULL;
        //        }
        //        pImport->Release();
        //
        //        if (SUCCEEDED(hr) && bNewTarget)
        //            hr = assembly->ReportError( ERR_CantRenameAssembly, mdTokenNil, NULL, pszFilename);
        //
        //        if (SUCCEEDED(hr) && m_pAssem != NULL)
        //            hr = m_pAssem->ComparePEKind( assembly, false);
        //
        //        delete [] newTarget;
        //        if (FAILED(hr)) {
        //            delete assembly;
        //            assembly = NULL;
        //            return hr;
        //        }
        //
        //        if (FAILED(hr = m_pImports->AddFile( assembly, 0, pFileToken)))
        //            delete assembly;
        //        return SUCCEEDED(hr) ? S_OK : hr;
        //    }
        //}

        //------------------------------------------------------------
        // CAsmLink.AddFile
        //------------------------------------------------------------
        virtual internal bool AddFile(
            OUTFILESYM outfileSym)
        {
            DebugUtil.Assert(this.inited && !this.assemblyEmitted && !this.preClosed);

            if (outfileSym.AssemblyBuilderEx != null)
            {
                this.assemblyBuilderEx = outfileSym.AssemblyBuilderEx;
                return true;
            }
            this.assemblyBuilderEx = null;
            return false;
        }

        //------------------------------------------------------------
        // CAsmLink.AddFile (sscli)
        //------------------------------------------------------------
        //virtual internal HRESULT AddFile(
        //    mdAssembly AssemblyID,
        //    LPCWSTR pszFilename,
        //    DWORD dwFlags,
        //    IMetaDataEmit *pEmitter,
        //    mdFile * pFileToken)
        //{
        //    ASSERT(m_bInited && !m_bAssemblyEmitted && !m_bPreClosed);
        //    ASSERT(AssemblyID == TokenFromRid(mdtAssembly, 1) || AssemblyID == AssemblyIsUBM);
        //
        //    // NULL filename means 'InMemory' module, but we need a filename so convert it to the
        //    // empty string.
        //    if (pszFilename == NULL)
        //        pszFilename = L"";
        //
        //    HRESULT hr = E_FAIL;
        //
        //    if (AssemblyID == AssemblyIsUBM) {
        //        if (m_pAssem == NULL) {
        //
        //            CComPtr<IMetaDataAssemblyEmit> pAEmitter;
        //            hr = pEmitter->QueryInterface(IID_IMetaDataAssemblyEmit, (void**)&pAEmitter);
        //            if (FAILED(hr)) {
        //                return hr;
        //            }
        //
        //            m_pAssem = new CAssembly();
        //            if (m_pAssem == NULL)
        //                return E_OUTOFMEMORY;
        //
        //            if (FAILED(hr = m_pAssem->Init(m_pError, this, pAEmitter))) {
        //                delete m_pAssem;
        //                m_pAssem = NULL;
        //                return hr;
        //            }
        //        }
        //    }
        //
        //    if (wcslen(pszFilename) > MAX_PATH)
        //        return FileNameTooLong(pszFilename); // File name too long
        //
        //    CFile *file = new CFile();
        //    if (file == NULL) {
        //        return E_OUTOFMEMORY;
        //    }    
        //    if (FAILED(hr = file->Init(pszFilename, pEmitter, m_pError, this)) ||
        //        FAILED(hr = m_pAssem->AddFile(file, dwFlags, pFileToken)))
        //        delete file;
        //
        //    return hr;
        //}

        // virtual internal HRESULT  AddFile2(mdAssembly AssemblyID, LPCWSTR pszFilename, DWORD dwFlags,
        //     IMetaDataEmit2 *pEmitter, mdFile * pFileToken);

        //------------------------------------------------------------
        // CAsmLink.AddImport
        //
        /// <summary>
        /// Add module specified by infileSym to this.assemblyBuilder.
        /// </summary>
        /// <param name="infileSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool AddImport(INFILESYM infileSym)
        {
            CModuleEx cmod = this.assemblyBuilderEx.AddModule(
                infileSym,
                null,
                null,
                false);
            return !(cmod == null);
        }

        //------------------------------------------------------------
        // CAsmLink::AddImport (sscli)
        //------------------------------------------------------------
        //virtual internal HRESULT AddImport(
        //    mdAssembly AssemblyID,
        //    mdToken ImportToken,
        //    DWORD dwFlags,
        //    mdFile * pFileToken)
        //{
        //    // If we have already emitted the manifest, and then we import
        //    // a file with a CA that maps to an assembly option, we're in trouble!
        //    ASSERT(m_bInited && !m_bAssemblyEmitted && !m_bPreClosed && !m_bManifestEmitted);
        //
        //    HRESULT hr;
        //    CFile *file = NULL;
        //    if (TypeFromToken(ImportToken) == mdtModule) {
        //        ASSERT(RidFromToken(ImportToken) < m_pModules->CountFiles());
        //        hr = m_pModules->GetFile( ImportToken, &file);
        //    } else {
        //        ASSERT(TypeFromToken(ImportToken) == mdtAssemblyRef && RidFromToken(ImportToken) < m_pImports->CountFiles());
        //        hr = m_pImports->GetFile( ImportToken, &file);
        //    }
        //    if (FAILED(hr))
        //        return hr;
        //    ASSERT(file != NULL);
        //    if (FAILED(hr = m_pAssem->AddFile(file, dwFlags, pFileToken)))
        //        return hr;
        //    else if (AssemblyID == AssemblyIsUBM) {
        //        if (pFileToken)
        //            *pFileToken = ImportToken;
        //        return S_FALSE;
        //    }
        //    ASSERT(AssemblyID == TokenFromRid(mdtAssembly, 1));
        //    if (FAILED(hr = m_pModules->RemoveFile( ImportToken)))
        //        return hr;
        //    if (FAILED(hr = file->ImportFile( NULL, m_pAssem)))
        //        return hr;
        //    return file->ImportResources(m_pAssem);
        //}

        // virtual internal HRESULT  GetScope(mdAssembly AssemblyID, mdToken FileToken, DWORD dwScope, IMetaDataImport** ppImportScope);
        // virtual internal HRESULT  GetScope2(mdAssembly AssemblyID, mdToken FileToken, DWORD dwScope, IMetaDataImport2** ppImportScope);
        // virtual internal HRESULT  GetAssemblyRefHash(mdToken FileToken, const void** ppvHash, DWORD* pcbHash);
        // virtual internal HRESULT  GetPublicKeyToken(LPCWSTR pszKeyFile, LPCWSTR pszKeyContainer, void * pvPublicKeyToken, DWORD * pcbPublicKeyToken);

        //------------------------------------------------------------
        // CAsmLink.SetPEKind (1)
        //
        /// <summary>
        /// Set PE kind and machin of a given CAssemblyBuilder instance.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="peKind"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetPEKind(
            CAssemblyBuilderEx builder,
            PortableExecutableKinds peKind,
            ImageFileMachine machine)
        {
            if (builder == null)
            {
                return false;
            }
            builder.SetPEKind(peKind, machine);
            return true;
        }

        //------------------------------------------------------------
        // CAsmLink.SetPEKind (2)
        //
        /// <summary>
        /// Set PE kind and machin of this.assemblyBuilder.
        /// </summary>
        /// <param name="peKind"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetPEKind(
            PortableExecutableKinds peKind,
            ImageFileMachine machine)
        {
            return SetPEKind(this.assemblyBuilderEx, peKind, machine);
        }

        //------------------------------------------------------------
        // CAsmLink::SetPEKind (sscli)
        //------------------------------------------------------------
        //virtual internal HRESULT SetPEKind(
        //    mdAssembly AssemblyID,
        //    mdToken FileToken,
        //    DWORD dwPEKind,
        //    DWORD dwMachine)
        //{
        //    ASSERT(m_bInited && !m_bAssemblyEmitted && !m_bPreClosed);
        //    ASSERT(AssemblyID == TokenFromRid(mdtAssembly, 1) || AssemblyID == AssemblyIsUBM);
        //    ASSERT((FileToken == AssemblyID) ||
        //        (TypeFromToken(FileToken) == mdtFile && RidFromToken(FileToken) < m_pAssem->CountFiles()) || 
        //        (TypeFromToken(FileToken) == mdtAssemblyRef && RidFromToken(FileToken) < m_pImports->CountFiles()) || 
        //        (TypeFromToken(FileToken) == mdtModule && RidFromToken(FileToken) < m_pModules->CountFiles()));
        //
        //    HRESULT hr = E_INVALIDARG;
        //    CFile* file = NULL;
        //
        //    if (TypeFromToken(FileToken) == mdtAssemblyRef) {
        //        hr = m_pImports->GetFile( FileToken, &file);
        //    } else if (TypeFromToken(FileToken) == mdtModule) {
        //        hr = m_pModules->GetFile( FileToken, &file);
        //    } else if (FileToken == AssemblyID) {
        //        file = m_pAssem;
        //        hr = S_OK;
        //    } else if (TypeFromToken(FileToken) == mdtFile) {
        //        if (AssemblyID == AssemblyIsUBM)
        //            m_pAssem->SetPEKind( dwPEKind, dwMachine);
        //        hr = m_pAssem->GetFile( FileToken, &file);
        //    }
        //
        //    if (SUCCEEDED(hr)) {
        //        file->SetPEKind( dwPEKind, dwMachine);
        //
        //        DWORD cnt = m_pImports->CountFiles();
        //        for (DWORD i = 0; i < cnt; i++) {
        //            CFile * ref = NULL;
        //            if (FAILED(hr = m_pImports->GetFile( i, &ref)) ||
        //                S_OK != (hr = m_pAssem->ComparePEKind( ref, false)))
        //                return hr;
        //        }
        //    }
        //
        //    return hr;
        //}

        //------------------------------------------------------------
        // CAsmLink.ImportTypes
        //------------------------------------------------------------
        // virtual internal HRESULT  ImportTypes(mdAssembly AssemblyID, mdToken FileToken, DWORD dwScope, HALINKENUM* phEnum,
        //     IMetaDataImport **ppImportScope, DWORD* pdwCountOfTypes);
        // virtual internal HRESULT  ImportTypes2(mdAssembly AssemblyID, mdToken FileToken, DWORD dwScope, HALINKENUM* phEnum,
        //     IMetaDataImport2 **ppImportScope, DWORD* pdwCountOfTypes);
        // virtual internal HRESULT  EnumCustomAttributes(HALINKENUM hEnum, mdToken tkType, mdCustomAttribute rCustomValues[],
        //     ULONG cMax, ULONG *pcCustomValues);
        // virtual internal HRESULT  EnumImportTypes(HALINKENUM hEnum, DWORD dwMax, mdTypeDef aTypeDefs[], DWORD* pdwCount); 
        // virtual internal HRESULT  CloseEnum(HALINKENUM hEnum);

        // Exporting

        // virtual internal HRESULT  ExportType(mdAssembly AssemblyID, mdToken FileToken, mdTypeDef TypeToken,
        //     LPCWSTR pszTypename, DWORD dwFlags, mdExportedType* pType); 
        // virtual internal HRESULT  ExportNestedType(mdAssembly AssemblyID, mdToken FileToken, mdTypeDef TypeToken, 
        //     mdExportedType ParentType, LPCWSTR pszTypename, DWORD dwFlags, mdExportedType* pType); 

        //------------------------------------------------------------
        // CAsmLink.EmitInternalExportedTypes
        //------------------------------------------------------------
        virtual internal bool EmitInternalExportedTypes()
        {
            DebugUtil.Assert(this.inited && !this.assemblyEmitted && !this.preClosed);
            return this.assemblyBuilderEx.EmitInternalTypes();
        }

        //------------------------------------------------------------
        // CAsmLink::EmitInternalExportedTypes (sscli)
        //------------------------------------------------------------
        //virtual internal HRESULT EmitInternalExportedTypes(mdAssembly AssemblyID)
        //{
        //    DebugUtil.Assert(m_bInited && !m_bAssemblyEmitted && !m_bPreClosed);
        //    DebugUtil.Assert(AssemblyID == TokenFromRid(mdtAssembly, 1));
        //
        //    return m_pAssem.EmitInternalTypes();
        //}

        // virtual internal HRESULT  ExportTypeForwarder(mdAssembly AssemblyID, LPCWSTR pszTypename, DWORD dwFlags, mdExportedType* pType); 
        // virtual internal HRESULT  ExportNestedTypeForwarder(mdAssembly AssemblyID, mdToken FileToken, mdTypeDef TypeToken, 
        //     mdExportedType ParentType, LPCWSTR pszTypename, DWORD dwFlags, mdExportedType* pType); 

        // Resources

        // virtual internal HRESULT  EmbedResource(mdAssembly AssemblyID, mdToken FileToken, LPCWSTR pszResourceName, DWORD dwOffset, DWORD dwFlags); 
        // virtual internal HRESULT  LinkResource(mdAssembly AssemblyID, LPCWSTR pszFileName, LPCWSTR pszNewLocation, LPCWSTR pszResourceName, DWORD dwFlags); 

        // virtual internal HRESULT  GetFileDef(mdAssembly AssemblyID, mdFile TargetFile, mdFile* pScope);
        // virtual internal HRESULT  GetResolutionScope(mdAssembly AssemblyID, mdToken FileToken, mdToken TargetFile, mdToken* pScope); 
        // virtual internal HRESULT  GetWin32ResBlob(mdAssembly AssemblyID, mdToken FileToken, BOOL fDll, LPCWSTR pszIconFile,
        //     const void **ppResBlob, DWORD *pcbResBlob);
        // virtual internal HRESULT  FreeWin32ResBlob(const void **ppResBlob);

        // Custom Attributes and Assembly properties

        //------------------------------------------------------------
        // CAsmLink.SetAssemblyProps
        //
        /// <summary>
        /// Set an assembly custom attribute.
        /// </summary>
        /// <param name="caConstInfo"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetAssemblyProps(
            ConstructorInfo caConstInfo,
            params object[] args)
        {
            DebugUtil.Assert(this.assemblyBuilderEx != null);
            return this.assemblyBuilderEx.SetCustomAttribute(caConstInfo, args);
        }

        //STDMETHOD(SetAssemblyProps)(mdAssembly AssemblyID, mdToken FileToken, AssemblyOptions Option, VARIANT Value); 
 
        //------------------------------------------------------------
        // CAsmLink.
        //------------------------------------------------------------
        // virtual internal HRESULT  EmitAssemblyCustomAttribute(mdAssembly AssemblyID, mdToken FileToken, mdToken tkType, 
        //     void const* pCustomValue, DWORD cbCustomValue, BOOL bSecurity, BOOL bAllowMultiple); 
        // virtual internal HRESULT  EndMerge(mdAssembly AssemblyID);
        // virtual internal HRESULT  EmitManifest(mdAssembly AssemblyID, DWORD* pdwReserveSize, mdAssembly* ptkManifest);

        //  Emit assembly to the MetaEmit interface

        // virtual internal HRESULT  EmitAssembly(mdAssembly AssemblyID);

        //  Finish everything off


        //------------------------------------------------------------
        // CAsmLink.PreCloseAssembly
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void SaveModuleImages()
        {
            if (this.importedAssemblyDic == null)
            {
                return;
            }

            foreach (KeyValuePair<string, CModuleEx> kv in this.ImportedModuleDic.Dictionary)
            {
                kv.Value.SaveImage();
            }
        }

        //------------------------------------------------------------
        // CAsmLink.PreCloseAssembly
        //------------------------------------------------------------
        virtual internal bool PreCloseAssembly(uint AssemblyID)
        {
            DebugUtil.Assert(this.inited && !this.preClosed);
            return true;
        }

        //------------------------------------------------------------
        // CAsmLink::PreCloseAssembly (sscli)
        //------------------------------------------------------------
        //HRESULT CAsmLink::PreCloseAssembly(mdAssembly AssemblyID)
        //{
        //    ASSERT(m_bInited && !m_bPreClosed);
        //    ASSERT(AssemblyID == TokenFromRid(mdtAssembly, 1));	
        //
        //    HRESULT hr = S_OK;
        //    if (!m_bAssemblyEmitted)
        //    {
        //        hr = EmitAssembly(AssemblyID);
        //    }
        //
        //    m_bPreClosed = true;
        //    m_pAssem->PreClose();
        //    return hr;
        //}

        //------------------------------------------------------------
        // CAsmLink.CloseAssembly
        //
        /// <summary>
        /// <para>Clear this.assemblyBuilderEx.</para>
        /// </summary>
        /// <param name="assemblyId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool CloseAssembly(uint assemblyId)
        {
            DebugUtil.Assert(this.inited && (this.preClosed || (assemblyId == AssemblyUtil.AssemblyIsUBM)));

            this.preClosed = false;
            this.manifestEmitted = false;
            this.assemblyEmitted = false;
            bool br = false;

            if (assemblyId == AssemblyUtil.AssemblyIsUBM)
            {
                return true;
            }
            else
            {
                // Sign the assembly
                br = this.assemblyBuilderEx.SignAssembly();
            }

            this.assemblyBuilderEx = null;
            return br;
        }

        //------------------------------------------------------------
        // CAsmLink::CloseAssembly (sscli)
        //------------------------------------------------------------
        //HRESULT CAsmLink::CloseAssembly(mdAssembly AssemblyID)
        //{
        //    ASSERT(m_bInited && (m_bPreClosed || AssemblyID == AssemblyIsUBM));
        //    m_bPreClosed = false;
        //    m_bManifestEmitted = false;
        //    m_bAssemblyEmitted = false;
        //    HRESULT hr;
        //
        //    if (AssemblyID == AssemblyIsUBM) {
        //        hr = S_FALSE;
        //    } else {
        //        // Sign the assembly
        //        hr = m_pAssem->SignAssembly();
        //    }
        //
        //    delete m_pAssem;
        //    m_pAssem = NULL;
        //    return hr;
        //
        //    // After this is done, should be able to re-use the interface
        //}

        //private:

        //------------------------------------------------------------
        // CAsmLink.
        //------------------------------------------------------------
        // private HRESULT FileNameTooLong(LPCWSTR filename);
        // private HRESULT GetScopeImpl(mdAssembly AssemblyID, mdToken FileToken, DWORD dwScope, CFile ** ppFile);
        // private HRESULT ImportTypesImpl(mdAssembly AssemblyID, mdToken FileToken, DWORD dwScope, HALINKENUM* phEnum,
        //     IMetaDataImport **ppImportScope, IMetaDataImport2 **ppImportScope2, DWORD* pdwCountOfTypes);
    }
}
