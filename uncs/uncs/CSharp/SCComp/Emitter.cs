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
// File: emitter.h
//
// Defines the structures used to emit COM+ metadata and create executable files.
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
// File: emitter.cpp
//
// Routines for emitting CLR metadata and creating executable files.
// ===========================================================================

//============================================================================
// Emitter.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //#define CTOKREF 1000   // Number of token refs per block. Fits nicely in a page.

    //======================================================================
    // class SECATTRSAVE
    //
    /// <summary>
    /// structure for saving security attributes.
    /// </summary>
    //======================================================================
    internal class SECATTRSAVE
    {
        internal SECATTRSAVE Next = null;               //next 
        internal uint CtorToken = 0;                    // ctorToken
        internal METHSYM MethodSym = null;              // method
        internal List<byte> Buffer = new List<byte>();  // BYTE * buffer; unsigned bufferSize;
    }

    //======================================================================
    // class TypeMemTokMap
    //
    /// <summary>
    /// Maps from the triple (agg-type-sym, member, type-args) to token.
    /// </summary>
    //======================================================================
    internal class TypeMemTokMap
    {
        //============================================================
        // class TypeMemTokMap.Key
        //============================================================
        internal class Key
        {
            internal SYM sym;
            internal TypeArray ClassTypeArguments = null;   // typeArgsCls;
            internal TypeArray MethodTypeArguments = null;  // typeArgsMeth;

            //--------------------------------------------------------
            // TypeMemTokMap.Key Constructor
            //
            /// <summary></summary>
            /// <param name="sym"></param>
            /// <param name="classArgs"></param>
            /// <param name="methodArgs"></param>
            //--------------------------------------------------------
            internal Key(SYM sym, TypeArray classArgs, TypeArray methodArgs)
            {
                DebugUtil.Assert(
                    classArgs != null && classArgs.Count > 0 ||
                    methodArgs != null && methodArgs.Count > 0);

                if (classArgs != null && classArgs.Count == 0)
                {
                    classArgs = null;
                }
                if (methodArgs != null && methodArgs.Count == 0)
                {
                    methodArgs = null;
                }
                //DebugUtil.Assert(offsetof(Key, sym) == 0 && offsetof(Key, hash) == 3 * sizeof(void *));

                this.sym = sym;
                this.ClassTypeArguments = classArgs;
                this.MethodTypeArguments = methodArgs;
            }

            //--------------------------------------------------------
            // TypeMemTokMap.Key.Equals (override)
            //
            /// <summary></summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            //--------------------------------------------------------
            public override bool Equals(object obj)
            {
                Key key = obj as Key;
                if (key == null) return false;
                return (
                    key.sym == sym &&
                    key.ClassTypeArguments == this.ClassTypeArguments &&
                    key.MethodTypeArguments == this.MethodTypeArguments);
            }

            //--------------------------------------------------------
            // TypeMemTokMap.Key.GetHashCode (override)
            //
            /// <summary></summary>
            /// <returns></returns>
            //--------------------------------------------------------
            public override int GetHashCode()
            {
                return (
                    (sym != null ? sym.GetHashCode() : 0) ^
                    (ClassTypeArguments != null ? ClassTypeArguments.GetHashCode() : 0) ^
                    (MethodTypeArguments != null ? MethodTypeArguments.GetHashCode() : 0)
                ) & 0x7FFFFFFF;
            }
        }

        //============================================================
        // class TypeMemTokMap.Entry
        //============================================================
        internal class Entry
        {
            internal Key Key = null;                // key;
            internal MemberInfo MemberInfo = null;  // mdToken tok;;
            // Entry * next;
        }

        //NRHEAP * heap;
        private COMPILER compiler;
        private Dictionary<Key, Entry> entryDic = new Dictionary<Key, Entry>(); // Entry ** prgent;
        //private int centHash;
        //private int centTot;

        //------------------------------------------------------------
        // TypeMemTokMap.Find
        //
        /// <summary></summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private Entry Find(Key key)
        {
            if (key != null)
            {
                Entry ent = null;
                try
                {
                    if (entryDic.TryGetValue(key, out ent))
                    {
                        return ent;
                    }
                }
                catch (ArgumentException)
                {
                    DebugUtil.Assert(false);
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // TypeMemTokMap.FindOrCreateEntry
        //
        /// <summary></summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private Entry FindOrCreateEntry(Key key)
        {
            Entry ent;
            try
            {
                if (entryDic.TryGetValue(key, out ent))
                {
                    return ent;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }

            ent = new Entry();
            ent.Key = key;
            entryDic.Add(key, ent);
            return ent;
        }

        //private TypeMemTokMap() { }

        //public:
        //static TypeMemTokMap * Create(COMPILER * compiler, NRHEAP * heap);
        //mdToken * GetTokPtr(SYM * sym, TypeArray * typeArgsCls, TypeArray * typeArgsMeth);

        //------------------------------------------------------------
        // TypeMemTokMap Constructor
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        internal TypeMemTokMap(COMPILER comp)
        {
            this.compiler = comp;
        }

        //------------------------------------------------------------
        // TypeMemTokMap.GetEntry
        //
        /// <summary>
        /// <para>Find the entry with the specified key.
        /// If not found, create a new Entry instance.</para>
        /// <para>Use this or GetMemberInfo method in place of GetTokPtr method.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="classTypeArgs"></param>
        /// <param name="methodTypeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Entry GetEntry(
            SYM sym,
            TypeArray classTypeArgs,
            TypeArray methodTypeArgs)
        {
            Key key = new Key(sym, classTypeArgs, methodTypeArgs);
            return this.FindOrCreateEntry(key);
        }

        //------------------------------------------------------------
        // TypeMemTokMap.GetMemberInfo
        //
        /// <summary>
        /// <para>Find the entry with the specified key and return its MemberInfo field.</para>
        /// <para>Use this or GetEntry method in place of GetTokPtr method.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="classTypeArgs"></param>
        /// <param name="methodTypeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MemberInfo GetMemberInfo(
            SYM sym,
            TypeArray classTypeArgs,
            TypeArray methodTypeArgs)
        {
            Key key = new Key(sym, classTypeArgs, methodTypeArgs);
            Entry ent = this.Find(key);
            return (ent != null ? ent.MemberInfo : null); ;
        }
    }

    //======================================================================
    //class EMITTER
    //
    /// <summary>
    /// The class that handles emitted PE files, and generating the metadata
    /// database within the files.
    /// </summary>
    //======================================================================
    internal class EMITTER
    {
        /// <summary>
        /// <para>Number of bytes to grow signature buffer by.</para>
        /// <para>(CSharp\SCComp\Emitter.cs)</para>
        /// </summary>
        internal const int SIGBUFFER_GROW = 256;

        /// <summary>
        /// <para>max size of UTF8 char encoding</para>
        /// <para>(CSharp\SCComp\Emitter.cs)</para>
        /// </summary>
        internal const int MAX_UTF8_CHAR = 3;

        /// <summary>
        /// <para>MAX_FULLNAME_SIZE / MAX_UTF8_CHAR</para>
        /// <para>(CSharp\SCComp\Emitter.cs)</para>
        /// </summary>
        internal const int SAFE_FULLNAME_CHARS = ImportUtil.MAX_FULLNAME_SIZE / MAX_UTF8_CHAR;

        //============================================================
        // class EMITTER.CompGenTypeToken
        //============================================================
        internal class CompGenTypeToken
        {
            internal CompGenTypeToken Next = null;  // * next;
            //internal int tkTypeDef = 0;           // mdToken tkTypeDef;
            internal Type Type = null;              // mdToken tkTypeDef;
            internal int size = 0;                  // unsigned int size;
        }

        //------------------------------------------------------------
        // EMITTER Fields and Properties (1)
        //------------------------------------------------------------
        //protected:
        private COMPILER compiler = null;
        internal COMPILER Compiler
        {
            get // compiler()
            {
                DebugUtil.Assert(compiler != null);
                return compiler;
            }
        }

        //------------------------------------------------------------
        // EMITTER Fields and Properties (2)
        //
        // For accumulating security attributes
        //------------------------------------------------------------
        private List<SECATTRSAVE> savedSecurityAttributeList = null;  // SECATTRSAVE * listSecAttrSave;
        private int savedSecurityAttributeCount = 0;                  // uint cSecAttrSave;
        private int savedSecurityAtributeToken = 0;                   // uint tokenSecAttrSave;

        //------------------------------------------------------------
        // EMITTER Fields and Properties (3)
        //
        // IMetaDataEmit2 (clr\src\inc\cor.h)
        //     implemented by RegMeta (clr\src\md\compiler\regmeta.h)
        // IMetaDataAssemblyEmit (clr\src\inc\cor.h)
        //     implemented by RegMeta (clr\src\md\compiler\regmeta.h)
        // ISymUnmanagedWriter (prebuilt\idl\corsym.h)
        //     implemented by SymWriter (clr\src\tools\ildbsymbols\symwrite.h()
        // ISymUnmanagedWriter2 (prebuilt\idl\corsym.h)
        //     implemented by SymWriter (clr\src\tools\ildbsymbols\symwrite.h()
        //------------------------------------------------------------
        // cache a local copy of these

        //protected IMetaDataEmit2* metaemit;
        //protected IMetaDataAssemblyEmit *metaassememit;
        private CAssemblyBuilderEx assemblyBuilderEx = null;
        private CModuleBuilderEx moduleBuilderEx = null;

        //protected ISymUnmanagedWriter debugemit;
        //protected ISymUnmanagedWriter2 debugemit2;

        //------------------------------------------------------------
        // EMITTER Fields and Properties (4)
        //
        // Scratch area for signature creation.
        //------------------------------------------------------------
#if DEBUG
        protected bool sigBufferInUse = false;
#endif
        // typedef unsigned __int8 COR_SIGNATURE;   // clr\src\inc\corhdr.h(733)
        // typedef COR_SIGNATURE* PCOR_SIGNATURE;   // pointer to a cor sig.  Not void* so that  // clr\src\inc\corhdr.h(735)

        //PCOR_SIGNATURE sigBuffer;
        //PCOR_SIGNATURE sigEnd;    // End of allocated area.
        protected List<int> sigBuffer = new List<int>();

        protected TypeBuilder globalTypeBuilder = null; // mdToken globalTypeToken;
        protected int tokErrAssemRef = 0;               // mdToken tokErrAssemRef;

        protected CDIIteratorLocalsInfo iteratorLocalsInfo = null; // * pIteratorLocalsInfo;

        // Heap for storing token addresses.
        //struct TOKREFS {
        //    struct TOKREFS * next;
        //    mdToken * tokenAddrs[CTOKREF];
        //};
        //NRHEAP tokrefHeap;                  // Heap to allocate from.

        //TOKREFS * tokrefList;               // Head of the tokref list.
        //int iTokrefCur;                     // Current index within tokrefList.
        protected List<Object> mdInfoList = new List<object>();

        protected CompGenTypeToken compGenTypeList = null; // * cgttHead;

        // NRMARK mark;

        protected TypeMemTokMap typeMemberMap = null;   // * pmap;
        //protected int ipScopePrev = 0;

        //public:
        //------------------------------------------------------------
        // EMITTER Constructor
        //------------------------------------------------------------
        internal EMITTER(COMPILER comp)
        {
            this.compiler = comp;
        }

        //    ~EMITTER();

        //------------------------------------------------------------
        // EMITTER.Term
        //
        /// <summary>
        /// Terminate everything.
        /// </summary>
        //------------------------------------------------------------
        internal void Term()
        {
            FreeSavedSecurityAttributes();

            // Free the signature buffer.
            if (sigBuffer.Count > 0)
            {
#if DEBUG
                sigBufferInUse = false;
#endif
                //compiler().globalHeap.Free(sigBuffer);
                sigBuffer.Clear();
            }

            //if (debugemit2 != null) {
            //    debugemit2.Release();
            //    debugemit2 = null;
            //}

            //cgttHead = null;
            //tokrefHeap.Mark( &mark);
            EraseEmitTokens();  // Forget all tokens in this output file.
            //tokrefHeap.FreeHeap();
        }

        //------------------------------------------------------------
        // EMITTER.Error
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void Error(ERRORKIND kind, Exception excp)
        {
            if (this.compiler != null &&
                this.compiler.Controller != null)
            {
                this.compiler.Error(kind, excp);
            }
        }

        //------------------------------------------------------------
        // EMITTER.AllocTypeMemberMap
        //
        /// <summary>
        /// <para>Allocate to EMITTER.typeMemberMap.</para>
        /// <para>Separeted form BeginOutputFile method in sscli.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void AllocTypeMemberMap()
        {
            this.typeMemberMap = new TypeMemTokMap(this.Compiler);
        }

        //------------------------------------------------------------
        // EMITTER.BeginOutputFile (1)
        //
        /// <summary>
        /// Begin emitting an output file.
        /// If an error occurs, it is reported to the user and then false is returned.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BeginOutputFile()
        {
            return true;
        }
        //------------------------------------------------------------
        // EMITTER::BeginOutputFile
        //------------------------------------------------------------
        //bool EMITTER::BeginOutputFile()
        //{
        //    // cache a local copy, don't addRef because we don't own these!
        //    metaemit = compiler()->curFile->GetEmit();
        //    metaassememit = compiler()->curFile->GetAssemblyEmitter();
        //    debugemit = compiler()->curFile->GetDebugEmit();
        //    if (!debugemit ||
        //        FAILED(debugemit->QueryInterface(IID_ISymUnmanagedWriter2, (void**)&debugemit2))) {
        //            debugemit2 = NULL;
        //    }
        //    globalTypeToken = mdTokenNil;
        //    tokErrAssemRef = mdTokenNil;
        //
        //    // Initially, no token is set.
        //    tokrefHeap.Mark( &mark);
        //
        //    // Create the token map.
        //    ASSERT(!pmap);
        //    pmap = TypeMemTokMap::Create(compiler(), &tokrefHeap);
        //    ipScopePrev = 0;
        //
        //    return true;
        //}

        //------------------------------------------------------------
        // EMITTER.BeginOutputFile (1)
        //
        /// <summary>
        /// Begin emitting an output file.
        /// If an error occurs, it is reported to the user and then false is returned.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BeginOutputFile(OUTFILESYM outfile)
        {
            DebugUtil.Assert(outfile != null);

            this.moduleBuilderEx = outfile.ModuleBuilderEx;

            if (this.moduleBuilderEx == null ||
                this.moduleBuilderEx.AssemblyBuilderEx == null)
            {
                return false;
            }

            this.assemblyBuilderEx = this.moduleBuilderEx.AssemblyBuilderEx;
            return true;
        }

        //------------------------------------------------------------
        // EMITTER.EndOutputFile
        //
        /// <summary>
        /// <para>End writing an output file.
        /// If true is passed, the output file is actually written.
        /// If false is passed (e.g., because an error occurred),
        /// the output file is not written.</para>
        /// <para>true is returned if the output file was successfully written.</para>
        /// </summary>
        /// <param name="writeFile"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool EndOutputFile(bool writeFile)
        {
            try
            {
                OUTFILESYM outfileSym = compiler.CurrentOutfileSym;
                int errCount = compiler.ErrorCount();

                if (!compiler.FEncBuild())
                {
                    // do module attributes
                    compiler.FuncBRec.SetUnsafe(false);
                    GlobalAttrBind.Compile(
                        compiler,
                        outfileSym,
                        outfileSym.AttributesSym,
                        false,
                        null);
                    compiler.FuncBRec.ResetUnsafe();
                }

                if (compiler.FAbortEarly(errCount, null))
                {
                    writeFile = false;
                }
                bool isDLL = outfileSym.IsDLL;

                if (!compiler.FEncBuild())
                {
                    if (isDLL)
                    {
                        // Dlls don't have entry points.
                        outfileSym.EntryMethodSym = null;
                    }
                    else if (outfileSym.IsExe)
                    {
                        if (outfileSym.EntryMethodSym == null && writeFile)
                        {
                            DebugUtil.Assert(!outfileSym.IsUnnamed);
                            compiler.Error(
                                CSCERRID.ERR_NoEntryPoint,
                                new ErrArg(outfileSym.Name));
                            writeFile = false;
                        }
                    }

                    if (writeFile)  // && !compiler.Options.NoCodeGen)
                    {
                        // Set output file attributes.
                        // In sscli, call void PEFile::SetAttributes(bool fDll)
                        //compiler.CurrentOutfileSym.SetAttributes(isDLL);

                        // The rest is done by PEFile
                    }
                }
            }
            finally	// PAL_FINALLY
            {
                // This needs to always happen, even if writing failed.
                //EraseEmitTokens();  // Forget all tokens in this output file.

                //metaemit = null;
                //debugemit = null;
                //if (debugemit2)
                //{
                //    debugemit2.Release();
                //    debugemit2 = null;
                //}

                //globalTypeToken = mdTokenNil;
                this.globalTypeBuilder = null;
                this.tokErrAssemRef = 0;	//mdTokenNil;

                // Free any IDocumentWriter interfaces for this output file.
                for (INFILESYM infileSym = compiler.CurrentOutfileSym.FirstInFileSym();
                    infileSym != null;
                    infileSym = infileSym.NextInFileSym())
                {
                    if (!infileSym.IsSource)
                    {
                        continue;	// Not a source file.
                    }
                    //if (infileSym.UnmanagedDocumentWriter != null)
                    //{
                    //    //infileSym.UnmanagedDocumentWriter.Release();
                    //    infileSym.UnmanagedDocumentWriter = null;
                    //}
                }
            }	// PAL_ENDTRY

            // Never return true if writeFile was false.
            return writeFile ? true : false;
        }
        //------------------------------------------------------------
        // EMITTER::EndOutputFile (sscli)
        //------------------------------------------------------------
        //bool EMITTER::EndOutputFile(bool writeFile)
        //{
        //    PAL_TRY {
        //        OUTFILESYM *outfile = compiler()->curFile->GetOutFile();
        //
        //        int cerr = compiler()->ErrorCount();
        //
        //        if (!compiler()->FEncBuild()) {
        //            //
        //            // do module attributes
        //            //
        //            compiler()->funcBRec.setUnsafe(false);
        //            GlobalAttrBind::Compile(
        //                compiler(), outfile, outfile->attributes, GetModuleToken());
        //            compiler()->funcBRec.resetUnsafe();
        //        }
        //
        //        if (compiler()->FAbortEarly(cerr)) {
        //            writeFile = false;
        //        }
        //
        //        bool fDll = outfile->isDll;
        //
        //        if (!compiler()->FEncBuild()) {
        //            if (fDll) {
        //                outfile->entrySym = NULL;  // Dlls don't have entry points.
        //            }
        //            else {
        //                if (outfile->entrySym == NULL && writeFile) {
        //                    ASSERT (!outfile->isUnnamed());
        //                    compiler()->Error(NULL, ERR_NoEntryPoint, outfile->name->text);
        //                    writeFile = false;
        //                }
        //            }
        //
        //            if (writeFile && !compiler()->options.m_fNOCODEGEN) {
        //                
        //                // Set output file attributes.
        //                compiler()->curFile->SetAttributes(fDll);
        //
        //                // The rest is done by PEFile
        //            }
        //        }
        //    }
        //    PAL_FINALLY {
        //        // This needs to always happen, even if writing failed.
        //
        //        EraseEmitTokens();  // Forget all tokens in this output file.
        //        metaemit = NULL;
        //        debugemit = NULL;
        //        if (debugemit2) {
        //            debugemit2->Release();
        //            debugemit2 = NULL;
        //        }
        //
        //        globalTypeToken = mdTokenNil;
        //        tokErrAssemRef = mdTokenNil;
        //
        //        // Free any IDocumentWriter interfaces for this output file.
        //        for (PINFILESYM pInfile = compiler()->curFile->GetOutFile()->firstInfile();
        //             pInfile != NULL;
        //             pInfile = pInfile->nextInfile())
        //        {
        //            if (! pInfile->isSource)
        //                continue;                   // Not a source file.
        //
        //            if (pInfile->documentWriter) {
        //                pInfile->documentWriter->Release();
        //                pInfile->documentWriter = NULL;
        //            }
        //        }
        //    }
        //    PAL_ENDTRY
        //
        //    return writeFile ? true : false;  // Never return true if writeFile was false.
        //}

        //------------------------------------------------------------
        // EMITTER.SetEntryPoint
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void SetEntryPoint(METHSYM methodSym)
        {
            DebugUtil.Assert(methodSym != null && !String.IsNullOrEmpty(methodSym.Name));

            // Normal method
            OUTFILESYM outfileSym = methodSym.GetInputFile().GetOutFileSym();

            // Is this an entry point for the program? We only allow one.
            if (!outfileSym.IsDLL)
            {
                if (outfileSym.EntryMethodSym != null)
                {
                    if (!outfileSym.HasMultiEntryPointsErrorReported)
                    {
                        // If multiple entry points,
                        // we want to report all the duplicate entry points,
                        // including the first one we found.
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_MultipleEntryPoints,
                            new ErrArgRef(outfileSym.EntryMethodSym),
                            new ErrArgRef(outfileSym.Name));
                        outfileSym.HasMultiEntryPointsErrorReported = true;
                    }
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_MultipleEntryPoints,
                        new ErrArgRef(methodSym),
                        new ErrArgRef(outfileSym.Name));
                }
                outfileSym.EntryMethodSym = methodSym;

                if (outfileSym.IsUnnamed)
                {
                    Compiler.GlobalSymbolManager.SetOutFileName(methodSym.GetInputFile());
                }
            }
        }

        //------------------------------------------------------------
        // EMITTER.FindEntryPoint
        //
        /// <summary>
        /// <para>If an output file has a "/main" option
        /// find the class specified and then find the Main method
        /// report errors as needed</para>
        /// </summary>
        /// <param name="outfileSym"></param>
        //------------------------------------------------------------
        internal void FindEntryPoint(OUTFILESYM outfileSym)
        {
            if (String.IsNullOrEmpty(outfileSym.EntryClassName))
            {
                return;
            }

            // Try in this assembly first.
            int aid = Kaid.ThisAssembly;
            BAGSYM foundBagSym = null;

            string[] subNames = outfileSym.EntryClassName.Split('.', '/');
            int lastIndex = subNames.Length - 1;
            BAGSYM currentBagSym = Compiler.MainSymbolManager.RootNamespaceSym;

            for (int i = 0; i < subNames.Length; ++i)
            {
                string subName = subNames[i];

            LRetry:
                foundBagSym = Compiler.LookupInBagAid(
                    subName,
                    currentBagSym,
                    0,
                    aid,
                    SYMBMASK.NSSYM | SYMBMASK.AGGSYM) as BAGSYM;

                for (;
                    foundBagSym != null;
                    foundBagSym = Compiler.LookupNextInAid(
                        foundBagSym,
                        aid,
                        SYMBMASK.NSSYM | SYMBMASK.AGGSYM) as BAGSYM)
                {
                    if (!foundBagSym.IsAGGSYM ||
                        (foundBagSym as AGGSYM).AllTypeVariables.Count == 0)
                    {
                        break;
                    }
                }

                if (foundBagSym == null)
                {
                    if (aid == Kaid.ThisAssembly)
                    {
                        aid = Kaid.Global;
                        goto LRetry;
                    }
                    DebugUtil.Assert(aid == Kaid.Global);

                    if (i < lastIndex)
                    {
                        goto LNotFound;
                    }
                }

                if (i >= lastIndex)
                {
                    SYM otherSym;

                    if (foundBagSym != null)
                    {
                        if (foundBagSym.IsAGGSYM &&
                            ((foundBagSym as AGGSYM).IsClass ||
                            (foundBagSym as AGGSYM).IsStruct))
                        {
                            break;
                        }
                        otherSym = foundBagSym;
                    }
                    else
                    {
                        // Didn't find any AGGSYMs or NSSYMs. Look for anything.
                        otherSym = Compiler.LookupInBagAid(
                            subName,
                            currentBagSym,
                            0,
                            aid,
                            SYMBMASK.ALL);
                    }

                    if (otherSym != null)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_MainClassNotClass,
                            new ErrArgRef(otherSym));
                        return;
                    }

                    goto LNotFound;
                } // if (i >= lastIndex)

                currentBagSym = foundBagSym;
            } // for (int i = 0; i < subNames.Length; ++i)

            DebugUtil.Assert(
                foundBagSym.IsAGGSYM &&
                ((foundBagSym as AGGSYM).IsClass ||
                (foundBagSym as AGGSYM).IsStruct));

            AGGSYM aggSym = foundBagSym as AGGSYM;
            OUTFILESYM otherOutFileSym;

            if ((otherOutFileSym = aggSym.GetOutputFile()) != outfileSym)
            {
                if (aggSym.IsSource)
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_MainClassWrongFile,
                        new ErrArgRef(aggSym));
                }
                else
                {
                    if (outfileSym.IsUnnamed)
                    {
                        Compiler.GlobalSymbolManager.SetOutFileName(
                            outfileSym.FirstInFileSym());
                    }
                }
                Compiler.Error(
                    CSCERRID.ERR_MainClassIsImport,
                    new ErrArg(
                        outfileSym.EntryClassName),
                        new ErrArg(outfileSym.Name));
                return;
            }

            FindEntryPointInClass(aggSym);
            return;

        LNotFound:
            Compiler.Error(
                CSCERRID.ERR_MainClassNotFound,
                new ErrArg(outfileSym.EntryClassName));
            return;
        }

        //------------------------------------------------------------
        // EMITTER.FindEntryPointInClass
        //
        /// <summary>
        /// Find the Main method in the given class.
        /// </summary>
        /// <param name="parentAggSym"></param>
        //------------------------------------------------------------
        internal void FindEntryPointInClass(AGGSYM parentAggSym)
        {
            OUTFILESYM outfileSym = parentAggSym.GetOutputFile();

            // We've found the specified class, now let's look for a Main
            SYM sym = null;
            SYM nextSym = null;
            METHSYM methodSym = null;

            sym = Compiler.MainSymbolManager.LookupAggMember(
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.MAIN),
                parentAggSym,
                SYMBMASK.METHSYM);

            // No sense going through this loop if the class is generic....
            if (sym != null && parentAggSym.AllTypeVariables.Count == 0)
            {
                methodSym = sym as METHSYM;
                while (methodSym != null)
                {
                    // Must be public, static, void/int Main ()
                    // with no args or String[] args
                    // If you change this code also change the code
                    // in CLSDREC::defineMethod
                    // (it does basically the same thing)
                    if (methodSym.IsStatic &&
                        !methodSym.IsPropertyAccessor &&
                        methodSym.TypeVariables.Count == 0 &&
                        (methodSym.ReturnTypeSym.IsVoidType ||
                        methodSym.ReturnTypeSym.IsPredefType(PREDEFTYPE.INT)) &&
                        (methodSym.ParameterTypes.Count == 0 ||
                        ((methodSym.ParameterTypes.Count == 1 &&
                        methodSym.ParameterTypes[0] is ARRAYSYM) &&
                        (methodSym.ParameterTypes[0] as ARRAYSYM).ElementTypeSym.IsPredefType(
                            PREDEFTYPE.STRING))))
                    {
                        SetEntryPoint(methodSym);
                    }
                    nextSym = methodSym.NextSameNameSym;
                    methodSym = null;
                    while (nextSym != null)
                    {
                        if (nextSym is METHSYM)
                        {
                            methodSym = nextSym as METHSYM;
                            break;
                        }
                        nextSym = nextSym.NextSameNameSym;
                    }
                }
            }

            if (outfileSym.EntryMethodSym == null)
            {
                // We didn't find an entry point. Warn on all the Mains.
                Compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_NoMainInClass,
                    new ErrArgRef(parentAggSym));
                if (sym != null)
                {
                    methodSym = sym as METHSYM;

                    while (methodSym != null)
                    {
                        // Report anything that looks like Main () - even instance methods.
                        if (!methodSym.IsPropertyAccessor)
                        {
                            Compiler.ErrorRef(null,
                                (methodSym.TypeVariables.Count > 0 ||
                                parentAggSym.AllTypeVariables.Count > 0)
                                    ? CSCERRID.WRN_MainCantBeGeneric
                                    : CSCERRID.WRN_InvalidMainSig,
                                new ErrArgRef(sym));
                        }

                        nextSym = methodSym.NextSameNameSym;
                        methodSym = null;
                        while (nextSym != null)
                        {
                            if (nextSym is METHSYM)
                            {
                                methodSym = nextSym as METHSYM;
                                break;
                            }
                            nextSym = nextSym.NextSameNameSym;
                        }
                    }
                }
            }
        }

        //------------------------------------------------------------
        // EMITTER.EmitTypeVars
        //
        /// <summary>
        /// Emit type parameters.
        /// </summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal void EmitTypeVars(SYM sym)
        {
            TypeArray typeVars;
            int tvCount = 0;
            int outerTvCount = 0;
            string[] tvNames = null;
            GenericTypeParameterBuilder[] typeParamBuilders = null;
            AGGSYM tvAggSym = null;
            METHSYM tvMethodSym = null;

            switch (sym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SK in EmitTypeVars");
                    return;

                //----------------------------------------------------
                // METHSYM
                //----------------------------------------------------
                case SYMKIND.METHSYM:
                    METHSYM methodSym = sym as METHSYM;
                    DebugUtil.Assert(methodSym != null);
                    if (methodSym.GenericParameterTypes != null)
                    {
                        return;
                    }

                    MethodBuilder methBuilder = methodSym.MethodInfo as MethodBuilder;
                    if (methBuilder == null)
                    {
                        return;
                    }

                    typeVars = methodSym.TypeVariables;
                    tvAggSym = methodSym.ClassSym;

                    tvCount = TypeArray.Size(typeVars);
                    if (tvCount == 0)
                    {
                        return;
                    }

                    tvNames = new string[tvCount];
                    for (int i = 0; i < tvCount; ++i)
                    {
                        tvNames[i] = typeVars[i].Name;
                        DebugUtil.Assert(!String.IsNullOrEmpty(tvNames[i]));
                    }

                    try
                    {
                        typeParamBuilders = methBuilder.DefineGenericParameters(tvNames);
                        methodSym.GenericParameterTypes = typeParamBuilders;
                    }
                    catch (ArgumentException ex)
                    {
                        this.Error(ERRORKIND.ERROR, ex);
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.Error(ERRORKIND.ERROR, ex);
                    }
                    break;

                //----------------------------------------------------
                // AGGSYM
                //----------------------------------------------------
                case SYMKIND.AGGSYM:
                    AGGSYM aggSym = sym as AGGSYM;
                    DebugUtil.Assert(aggSym != null);
                    if (aggSym.GenericParameterTypes != null)
                    {
                        return;
                    }

                    TypeBuilder typeBuilder = aggSym.Type as TypeBuilder;
                    if (typeBuilder == null)
                    {
                        return;
                    }

                    typeVars = aggSym.AllTypeVariables;
                    tvAggSym = aggSym;

                    tvCount = TypeArray.Size(typeVars);
                    if (tvCount == 0)
                    {
                        return;
                    }
                    outerTvCount = tvCount - TypeArray.Size(aggSym.TypeVariables);

                    tvNames = new string[tvCount];
                    for (int i = 0; i < tvCount; ++i)
                    {
                        tvNames[i] = typeVars[i].Name;
                        DebugUtil.Assert(!String.IsNullOrEmpty(tvNames[i]));
                    }

                    try
                    {
                        typeParamBuilders = typeBuilder.DefineGenericParameters(tvNames);
                        aggSym.GenericParameterTypes = typeParamBuilders;
                    }
                    catch (ArgumentException ex)
                    {
                        this.Error(ERRORKIND.ERROR, ex);
                    }
                    break;
            }
            DebugUtil.Assert(typeVars.Count > 0);
            List<Type> ifaceList = null;
            Type baseClassType = null;

            //--------------------------------------------------------
            // Set GenericTypeParameteBuilder to TYVARSYM
            //--------------------------------------------------------
            //for (int index = outerTvCount; index < typeVars.Count; ++index)
            //{
            //    TYVARSYM tvSym = typeVars.ItemAsTYVARSYM(index);
            //    DebugUtil.Assert(tvSym != null && tvSym.FResolved());
            //    tvSym.SetGenericTypeParameterBuilder(typeParamBuilders[index]);
            //}

            //--------------------------------------------------------
            // Set the constraints to GenericTypeParameterBuilder
            //--------------------------------------------------------
            for (int index = 0; index < typeVars.Count; ++index)
            {
                TYVARSYM tvSym = typeVars.ItemAsTYVARSYM(index);
                TypeArray boundArray = tvSym.BoundArray;
                int bndCount = TypeArray.Size(boundArray);
                DebugUtil.Assert(bndCount >= 0);
                ifaceList = new List<Type>();
                baseClassType = null;

                for (int ibnd = 0; ibnd < bndCount; ibnd++)
                {
                    TYPESYM boundSym = boundArray[ibnd];
                    DebugUtil.Assert(boundSym != null);

                    if (boundSym.IsClassType())
                    {
                        if (baseClassType == null)
                        {
                            baseClassType = SymUtil.GetSystemTypeFromSym(
                                boundSym,
                                tvAggSym,
                                tvMethodSym);
                        }
                        else
                        {
                            if (baseClassType != SymUtil.GetSystemTypeFromSym(
                                    boundSym,
                                    tvAggSym,
                                    tvMethodSym))
                            {
                                // This constraint has multiple classes.
                                // This error was checked
                                // by CLSDREC.ResolveInheritanceRec.
                                // So, We should not be here.
                                DebugUtil.Assert(false);
                            }
                            continue;
                        }
                    }
                    else if (boundSym.IsInterfaceType())
                    {
                        ifaceList.Add(SymUtil.GetSystemTypeFromSym(
                            boundSym,
                            tvAggSym,
                            tvMethodSym));
                    }
                    else
                    {
                        DebugUtil.Assert(false);
                        continue;
                    }
                }

                GenericTypeParameterBuilder tpBuilder = typeParamBuilders[index];

                if (baseClassType != null)
                {
                    tpBuilder.SetBaseTypeConstraint(baseClassType);
                }
                if (ifaceList.Count > 0)
                {
                    Type[] ifArray = new Type[ifaceList.Count];
                    ifaceList.CopyTo(ifArray);
                    tpBuilder.SetInterfaceConstraints(ifArray);
                }

                GenericParameterAttributes flags = 0;

                if (tvSym.HasNewConstraint())
                {
                    flags |= GenericParameterAttributes.DefaultConstructorConstraint;
                }
                if (tvSym.HasReferenceConstraint())
                {
                    flags |= GenericParameterAttributes.ReferenceTypeConstraint;
                }
                if (tvSym.HasValueConstraint())
                {
                    flags |=
                        GenericParameterAttributes.NotNullableValueTypeConstraint |
                        GenericParameterAttributes.DefaultConstructorConstraint;
                }

                tpBuilder.SetGenericParameterAttributes(flags);
            }
        }

        //------------------------------------------------------------
        // EMITTER::EmitTypeVars (sscli)
        //------------------------------------------------------------
        //void EMITTER::EmitTypeVars(SYM * sym)
        //{
        //    mdToken ** pprgtokVars;
        //    TypeArray * typeVars;
        //    mdToken tokPar;
        //
        //    switch (sym->getKind()) {
        //    default:
        //        ASSERT(!"Bad SK in EmitTypeVars");
        //        return;
        //    case SK_METHSYM:
        //        typeVars = sym->asMETHSYM()->typeVars;
        //        if (typeVars->size == 0)
        //            return;
        //        pprgtokVars = &sym->asMETHSYM()->toksEmitTypeVars;
        //        tokPar = sym->asMETHSYM()->tokenEmit;
        //        break;
        //    case SK_AGGSYM:
        //        typeVars = sym->asAGGSYM()->typeVarsAll;
        //        if (typeVars->size == 0)
        //            return;
        //        pprgtokVars = &sym->asAGGSYM()->toksEmitTypeVars;
        //        tokPar = sym->asAGGSYM()->tokenEmit;
        //        break;
        //    }
        //
        //    ASSERT(typeVars->size > 0);
        //    ASSERT(*pprgtokVars == NULL);
        //
        //    mdToken rgtokBnd[8];
        //    int ctokMax = lengthof(rgtokBnd);
        //    mdToken * prgtokBnd = rgtokBnd;
        //
        //    *pprgtokVars = (mdToken *)compiler()->getGlobalSymAlloc().Alloc(SizeMul(typeVars->size, sizeof(mdToken)));
        //
        //    for (int ivar = 0; ivar < typeVars->size; ivar++) {
        //        TYVARSYM * var = typeVars->ItemAsTYVARSYM(ivar);
        //        ASSERT(var->FResolved());
        //        TypeArray * bnds = var->GetBnds();
        //        int cbnd = bnds->size;
        //        ASSERT(cbnd >= 0);
        //
        //        if (cbnd >= ctokMax) {
        //            ctokMax += ctokMax;
        //            if (cbnd >= ctokMax)
        //                ctokMax = cbnd + 1;
        //            prgtokBnd = STACK_ALLOC_ZERO(mdToken, ctokMax);
        //        }
        //        ASSERT(cbnd < ctokMax);
        //
        //        for (int ibnd = 0; ibnd < cbnd; ibnd++) {
        //            prgtokBnd[ibnd] = GetTypeRef(bnds->Item(ibnd));
        //        }
        //        ASSERT(cbnd < ctokMax);
        //        StoreAtIndex(prgtokBnd, cbnd, ctokMax, 0); // NULL terminate.
        //
        //        uint flags = 0;
        //
        //        if (var->FNewCon())
        //            flags |= gpDefaultConstructorConstraint;
        //        if (var->FRefCon())
        //            flags |= gpReferenceTypeConstraint;
        //        if (var->FValCon())
        //            flags |= gpNotNullableValueTypeConstraint | gpDefaultConstructorConstraint;
        //
        //        CheckHR(metaemit->DefineGenericParam(
        //            tokPar, var->indexTotal, flags, var->name->text, 0, prgtokBnd,
        //            (*pprgtokVars) + ivar));
        //    }
        //
        //    ASSERT(sizeof(*pprgtokVars) % sizeof(mdToken) == 0);
        //    mdToken * ptok = (mdToken *)pprgtokVars;
        //    for (size_t i = 0; i < sizeof(*pprgtokVars) / sizeof(mdToken); i++) {
        //        RecordEmitToken(ptok + i);
        //    }
        //}

        //------------------------------------------------------------
        // EMITTER.EmitAggregateDef
        //
        /// <summary>
        /// <para>Emit an aggregate type (struct, enum, class, interface) into the metadata.
        /// This does not emit any information about members of the aggregrate,
        /// but must be done before any aggregate members are emitted.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void EmitAggregateDef(AGGSYM aggSym)
        {
            if (aggSym == null || aggSym.Type != null)
            {
                return;
            }

#if true // 2016/06/15
            EmitAggregateDef(aggSym.BaseClassSym);

            TypeArray ifaces = aggSym.AllInterfaces;
            if (ifaces != null && ifaces.Count > 0)
            {
                for (int i = 0; i < ifaces.Count; ++i)
                {
                    EmitAggregateDef(ifaces[i]);
                }
            }

            TypeArray typeVars = aggSym.TypeVariables;
            for (int i = 0; i < typeVars.Count; ++i)
            {
                EmitAggregateDef(typeVars[i]);
            }
#endif
            TypeAttributes flags;
            string typeName = null;

            // If this assert triggers,
            // we're emitting the same aggregate twice into an output scope.
            DebugUtil.Assert(aggSym.IsPrepared);

            //--------------------------------------------------------
            // Get namespace and type name.
            //--------------------------------------------------------
            if (!MetaDataHelper.GetMetaDataName(aggSym, out typeName) ||
                (typeName.Length >= SAFE_FULLNAME_CHARS &&
                CharUtil.UTF8LengthOfUnicode(typeName) >= ImportUtil.MAX_FULLNAME_SIZE))
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_ClassNameTooLong, new ErrArgRef(aggSym));
            }

            //--------------------------------------------------------
            // Determine flags.
            //--------------------------------------------------------
            flags = MetaDataHelper.GetAggregateFlags(aggSym);

#if false
            Type baseType = null;
            if (!aggSym.IsInterface)
            {
                if (aggSym.BaseClassSym != null)
                {
                    baseType = aggSym.BaseClassSym.GetConstructedType(false);
                }

                if (baseType == null)
                {
                    if (aggSym.IsEnum)
                    {
                        baseType = typeof(System.Enum);
                    }
                    else if (aggSym.IsDelegate)
                    {
                        baseType = typeof(System.MulticastDelegate);
                    }
                    else
                    {
                        baseType = typeof(System.Object);
                    }
                }
            }

            //--------------------------------------------------------
            // interfaces
            //--------------------------------------------------------
            Type[] ifaces = null;
            int ifaceCount = TypeArray.Size(aggSym.Interfaces);
            if (ifaceCount > 0)
            {
                ifaces = new Type[ifaceCount];
                for (int i = 0; i < ifaceCount; ++i)
                {
                    AGGTYPESYM ifaceAts = aggSym.Interfaces[i] as AGGTYPESYM;
                    DebugUtil.Assert(ifaceAts != null);
                    ifaces[i] = ifaceAts.GetConstructedType(false);
                }
            }
#endif

            //--------------------------------------------------------
            // emit (1) non-nested type.
            //--------------------------------------------------------
            if (aggSym.ParentBagSym.IsNSSYM)
            {
                // Create the aggregate definition for a top level type.

                aggSym.TypeBuilder = this.moduleBuilderEx.DefineType(
                    typeName,
                    flags);
            }
            //--------------------------------------------------------
            // emit (2) nested type.
            //--------------------------------------------------------
            else
            {
                // Create the aggregate definition for a nested type.
                DebugUtil.Assert((aggSym.ParentBagSym as AGGSYM).IsTypeDefEmitted);
                AGGSYM parentAggSym = aggSym.ParentBagSym as AGGSYM;
                DebugUtil.Assert(parentAggSym != null);

                aggSym.TypeBuilder = this.moduleBuilderEx.DefineNestedType(
                    typeName,
                    flags,
                    parentAggSym.Type as TypeBuilder);
            }

            EmitTypeVars(aggSym);
            RecordEmitToken(aggSym.Type);
        }

#if true // 2016/06/15
        //------------------------------------------------------------
        // EMITTER.EmitAggregateDef (2)
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void EmitAggregateDef(TYPESYM typeSym)
        {
            if (typeSym == null)
            {
                return;
            }

            switch (typeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                case SYMKIND.DYNAMICSYM:
                    EmitAggregateDef((typeSym as AGGTYPESYM).GetAggregate());
                    return;

                case SYMKIND.ARRAYSYM:
                    EmitAggregateDef((typeSym as ARRAYSYM).ElementTypeSym);
                    return;

                case SYMKIND.VOIDSYM:
                    return;

                case SYMKIND.PARAMMODSYM:
                    EmitAggregateDef((typeSym as PARAMMODSYM).ParamTypeSym);
                    return;

                case SYMKIND.TYVARSYM:
                    TypeArray bnds = (typeSym as TYVARSYM).BoundArray;
                    if (bnds != null && bnds.Count > 0)
                    {
                        for (int i = 0; i < bnds.Count; ++i)
                        {
                            EmitAggregateDef(bnds[i]);
                        }
                    }
                    return;

                case SYMKIND.PTRSYM:
                    EmitAggregateDef((typeSym as PTRSYM).BaseTypeSym);
                    return;

                case SYMKIND.NUBSYM:
                    EmitAggregateDef((typeSym as NUBSYM).BaseTypeSym);
                    return;

                case SYMKIND.NULLSYM:
                case SYMKIND.ERRORSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.ANONMETHSYM:
                case SYMKIND.METHGRPSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.IMPLICITTYPESYM:           // CS3
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM:   // CS3
                case SYMKIND.LAMBDAEXPRSYM:             // CS3
                default:
                    return;
            }
        }
#endif


        //------------------------------------------------------------
        // EMITTER.EmitAggregateSpecialFields
        //
        /// <summary>
        /// <para>Emit any "special fields" associated with an aggregate.
        /// This can't be done in EmitAggregateDef or EmiAggregateInfo
        /// due to the special order rules for metadata emitting.</para>
        /// <para>Define "value__" field in enum type.</para>
        /// </summary>
        /// <param name="aggSym"></param>
            //------------------------------------------------------------
        internal void EmitAggregateSpecialFields(AGGSYM aggSym)
        {
            if (aggSym.IsEnum)
            {
                // The underlying type of an enum is represented
                // as a non-static field of that type. Its name is "value__".

                // Create the field definition in the metadata.
                if (aggSym == null || aggSym.TypeBuilder == null)
                {
                    return;
                }
                Type fieldType = null;
                if (aggSym.UnderlyingTypeSym == null ||
                    (fieldType = aggSym.UnderlyingTypeSym.GetConstructedType(aggSym, null, false)) == null)
                {
                    return;
                }

                aggSym.TypeBuilder.DefineField(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.ENUMVALUE),
                    fieldType,
                    FieldAttributes.Public | FieldAttributes.SpecialName);
            }
        }

        //------------------------------------------------------------
        // EMITTER.EmitAggregateBases
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void EmitAggregateBases(AGGSYM aggSym)
        {
            TypeBuilder typeBuilder = null;
            if (aggSym == null ||
                (typeBuilder = aggSym.TypeBuilder) == null)
            {
                return;
            }

            //--------------------------------------------------------
            // base class
            //--------------------------------------------------------
            Type baseType = null;
            if (!aggSym.IsInterface)
            {
                if (aggSym.BaseClassSym != null)
                {
                    baseType = aggSym.BaseClassSym.GetConstructedType(aggSym, null, false);
                }

                if (baseType == null)
                {
                    if (aggSym.IsEnum)
                    {
                        baseType = typeof(System.Enum);
                    }
                    else if (aggSym.IsDelegate)
                    {
                        baseType = typeof(System.MulticastDelegate);
                    }
                    else
                    {
                        baseType = typeof(System.Object);
                    }
                }

                try
                {
                    typeBuilder.SetParent(baseType);
                }
                catch (ArgumentException ex)
                {
                    this.Error(ERRORKIND.ERROR, ex);
                }
                catch (InvalidOperationException ex)
                {
                    this.Error(ERRORKIND.ERROR, ex);
                }
            }

            //--------------------------------------------------------
            // interfaces
            //--------------------------------------------------------
            int ifaceCount = TypeArray.Size(aggSym.Interfaces);
            if (ifaceCount > 0)
            {
                for (int i = 0; i < ifaceCount; ++i)
                {
                    AGGTYPESYM ifaceAts = aggSym.Interfaces[i] as AGGTYPESYM;
                    DebugUtil.Assert(ifaceAts != null);
                    Type ifaceType = ifaceAts.GetConstructedType(aggSym, null, false);
                    DebugUtil.Assert(ifaceType != null);

                    try
                    {
                        typeBuilder.AddInterfaceImplementation(ifaceType);
                    }
                    catch (ArgumentException ex)
                    {
                        this.Error(ERRORKIND.ERROR, ex);
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.Error(ERRORKIND.ERROR, ex);
                    }
                }
            }
        }
        //------------------------------------------------------------
        // EMITTER::EmitAggregateBases (sscli)
        //------------------------------------------------------------
        //void EMITTER::EmitAggregateBases(PAGGSYM sym)
        //{
        //    mdToken tokenBaseClass;
        //    mdToken * tokenInterfaces;
        //    int cInterfaces;
        //
        //    // If any type variables, emit them.
        //    if (sym->typeVarsAll->size)
        //        EmitTypeVars(sym);
        //
        //    // Determine base class.
        //    tokenBaseClass = mdTypeRefNil;
        //    if (sym->baseClass) {
        //        tokenBaseClass = GetTypeRef(sym->baseClass);
        //    }
        //
        //    // Determine base interfaces.
        //
        //    // First, count the number of interfaces.
        //    cInterfaces = sym->ifacesAll->size;
        //
        //    // If any interfaces, allocate array and fill it in.
        //    if (cInterfaces) {
        //        tokenInterfaces = STACK_ALLOC(mdToken, cInterfaces + 1);
        //        for (int i = 0; i < cInterfaces; i++) {
        //            tokenInterfaces[i] = GetTypeRef(sym->ifacesAll->Item(i));
        //        }
        //        tokenInterfaces[cInterfaces] = mdTypeRefNil;
        //    }
        //    else {
        //        // No interfaces.
        //        tokenInterfaces = NULL;
        //    }
        //
        //    CheckHR(metaemit->SetTypeDefProps(
        //            sym->tokenEmit,                 // TypeDef
        //            (DWORD)-1,                      // do not reset flags, it will overwrite any pseudoCAs in an incremental build
        //            tokenBaseClass,                 // base class
        //            tokenInterfaces));              // interfaces
        //}

        //------------------------------------------------------------
        // EMITTER.EmitMembVarDef
        //
        /// <summary>
        /// Emitted field def into the metadata. The parent aggregate must
        /// already have been emitted into the current metadata output file.
        /// </summary>
        /// <param name="fieldSym"></param>
        //------------------------------------------------------------
        internal void EmitMembVarDef(MEMBVARSYM fieldSym)
        {
            // Get typedef token for the containing class/enum/struct. This must
            // be present because the containing class must have been emitted already.

            DebugUtil.Assert(fieldSym != null);
            if (fieldSym.FieldInfo != null)
            {
                return;
            }

            TypeBuilder typeBuilder = fieldSym.ClassSym.Type as TypeBuilder;
            if (typeBuilder == null)
            {
                return;
            }

            // Determine the flags.

            object constObj = null;
            if (fieldSym.IsConst &&
                fieldSym.FixedAggSym == null &&
                !fieldSym.TypeSym.IsPredefType(PREDEFTYPE.DECIMAL))
            {
                constObj = fieldSym.ConstVal.GetObject();
            }

            if (fieldSym.Name.Length >= SAFE_FULLNAME_CHARS &&
                CharUtil.UTF8LengthOfUnicode(fieldSym.Name) >= ImportUtil.MAX_FULLNAME_SIZE)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_IdentifierTooLong, new ErrArgRef(fieldSym));
            }

            // Create the field definition in the metadata.
            fieldSym.FieldBuilder = this.moduleBuilderEx.DefineField(
                typeBuilder,
                fieldSym.Name,
                SymUtil.GetSystemTypeFromSym(fieldSym.TypeSym, fieldSym.ParentAggSym, null),
                GetMembVarFlags(fieldSym),
                constObj);

            RecordEmitToken(fieldSym.FieldBuilder);
        }

        //------------------------------------------------------------
        // EMITTER.EmitPropertyDef
        //
        /// <summary>
        /// Emit a property into the metadata. The parent aggregate, and the
        /// property accessors, must already have been emitted into the current
        /// metadata output file.
        /// </summary>
        /// <param name="propertySym"></param>
        //------------------------------------------------------------
        internal void EmitPropertyDef(PROPSYM propertySym)
        {
            if (propertySym.PropertyInfo != null)
            {
                return;
            }

            string name = null;

            // Set "nameText" to the output name.
            if (propertySym.Name == null)
            {
                // Explicit method implementations don't have a name in the language. Synthesize 
                // a name -- the name has "." characters in it so it can't possibly collide.
                // force truncation using a character size limit that won't exceed our UTF8 bytes max
                MetaDataHelper.GetExplicitImplName(propertySym, out name);
            }
            else
            {
                name = propertySym.GetRealName();
            }
            if (name.Length >= SAFE_FULLNAME_CHARS &&
                CharUtil.UTF8LengthOfUnicode(name) >= ImportUtil.MAX_FULLNAME_SIZE)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_IdentifierTooLong, new ErrArgRef(propertySym));
            }

            // Get typedef token for the containing class/enum/struct. This must
            // be present because the containing class must have been emitted already.

            TypeBuilder typeBuilder = propertySym.ClassSym.Type as TypeBuilder;
            if (typeBuilder == null)
            {
                return;
            }

            // Determine getter/setter methoddef tokens.
            if (propertySym.GetMethodSym != null)
            {
                if (propertySym.GetMethodSym.MethodBuilder == null)
                {
                    EmitMethodDef(propertySym.GetMethodSym);
                }
            }
            if (propertySym.SetMethodSym != null)
            {
                if (propertySym.SetMethodSym.MethodBuilder == null)
                {
                    EmitMethodDef(propertySym.SetMethodSym);
                }
            }
            DebugUtil.Assert(propertySym.ReturnTypeSym != null);

            TypeArray classTypeVars = propertySym.ClassSym.AllTypeVariables;
            Type returnType = SymUtil.GetSystemTypeFromSym(
                propertySym.ReturnTypeSym,
                propertySym.ClassSym,
                null);
            DebugUtil.Assert(returnType != null);

            propertySym.PropertyBuilder = this.moduleBuilderEx.DefineProperty(
                typeBuilder,
                name,
                GetPropertyFlags(propertySym),
                returnType,
                null,
                propertySym.GetMethodSym != null ? propertySym.GetMethodSym.MethodBuilder : null,
                propertySym.SetMethodSym != null ? propertySym.SetMethodSym.MethodBuilder : null,
                propertySym.ParameterTypes != null ?
                    SymUtil.GetSystemTypesFromTypeArray(
                        propertySym.ParameterTypes,
                        propertySym.ClassSym,
                        null) :
                    null);

            RecordEmitToken(propertySym.PropertyBuilder);
        }

        //------------------------------------------------------------
        // EMITTER.EmitEventDef
        //
        /// <summary>
        /// Emit an event into the metadata.
        /// The parent aggregate, and the event accessors, and the event field/property
        /// must already have been emitted into the current metadata output file.
        /// </summary>
        /// <param name="eventSym"></param>
        //------------------------------------------------------------
        internal void EmitEventDef(EVENTSYM eventSym)
        {
            if (eventSym.EventInfo != null)
            {
                return;
            }

            string name = null;

            // Set "nameText" to the output name.
            if (eventSym.Name == null)
            {
                // Explicit method implementations don't have a name in the language.
                // Synthesize a name -- the name has "." characters in it so it can't possibly collide.
                // force truncation using a character size limit that won't exceed our UTF8 bytes max
                MetaDataHelper.GetExplicitImplName(eventSym, out name);
            }
            else
            {
                name = eventSym.Name;
            }

            if (name.Length >= SAFE_FULLNAME_CHARS &&
                CharUtil.UTF8LengthOfUnicode(name) >= ImportUtil.MAX_FULLNAME_SIZE)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_IdentifierTooLong, new ErrArgRef(eventSym));
            }

            // Get typedef token for the containing class/enum/struct.
            // This must be present because the containing class must have been emitted already.

            TypeBuilder typeBuilder = eventSym.ClassSym.Type as TypeBuilder;
            if (typeBuilder == null)
            {
                return;
            }

            // Get typeref token for the delegate type of this event.

            // Determine adder/remover methoddef tokens.
            MethodBuilder addMethBuilder = null;
            MethodBuilder removeMethBuilder = null;

            if (eventSym.AddMethodSym != null)
            {
                addMethBuilder = eventSym.AddMethodSym.MethodBuilder;
            }
            if (eventSym.RemoveMethodSym != null)
            {
                removeMethBuilder = eventSym.RemoveMethodSym.MethodBuilder;
            }

            eventSym.EventBuilder = this.moduleBuilderEx.DefineEvent(
                typeBuilder,
                name,
                GetEventFlags(eventSym),
                SymUtil.GetSystemTypeFromSym(eventSym.TypeSym, eventSym.ClassSym, null),
                addMethBuilder,
                removeMethBuilder);
            DebugUtil.Assert(eventSym.EventBuilder != null);
            RecordEmitToken(eventSym.EventBuilder);
        }

        //------------------------------------------------------------
        // EMITTER.EmitMethodDef
        //
        /// <summary>
        /// Emit the methoddef for a method into the metadata.
        /// </summary>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void EmitMethodDef(METHSYM methodSym)
        {
            if (methodSym.MethodInfo != null ||
                methodSym.ConstructorInfo != null)
            {
                return;
            }

            DebugUtil.Assert(methodSym != null);
            if (methodSym.IsInstanceExtensionMethod)    // (CS3)
            {
                return;
            }
            if (methodSym.IsPartialMethod && methodSym.HasNoBody)   // (CS3)
            {
                return;
            }

            string name = null;
            MethodAttributes flags = 0;
            CallingConventions callConv = 0;
            TypeArray paramArray = methodSym.ParameterTypes;
            if (paramArray == null)
            {
                paramArray = BSYMMGR.EmptyTypeArray;
            }

            // Get typedef token for the containing class/enum/struct. This must
            // be present because the containing class must have been emitted already.

            TypeBuilder typeBuilder = methodSym.ClassSym.Type as TypeBuilder;
            if (typeBuilder == null)
            {
                return;
            }

            // Set "nameText" to the output name.
            if (String.IsNullOrEmpty(methodSym.Name))
            {
                // Explicit method implementations don't have a name in the language. Synthesize 
                // a name -- the name has "." characters in it so it can't possibly collide.
                // force truncation using a character size limit that won't exceed our UTF8 bytes max
                MetaDataHelper.GetExplicitImplName(methodSym, out name);
            }
            else
            {
                name = methodSym.Name;
            }
            //int len = (int) wcslen(nameText);
            if (name.Length >= SAFE_FULLNAME_CHARS &&
                CharUtil.UTF8LengthOfUnicode(name) >= ImportUtil.MAX_FULLNAME_SIZE)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_IdentifierTooLong, new ErrArgRef(methodSym));
            }

            // Determine the flags.
            flags = GetMethodFlags(methodSym);

            if (methodSym.IsStatic)
            {
                callConv = CallingConventions.Standard;
            }
            else
            {
                callConv = CallingConventions.HasThis;
            }

            // __arglist
            if (methodSym.IsVarargs)
            {
                callConv |= CallingConventions.VarArgs;

                int cparam = paramArray.Count;
                if (cparam == 1)
                {
                    paramArray = BSYMMGR.EmptyTypeArray;
                }
                else if (cparam > 1)
                {
                    if (!methodSym.ParameterTypes.GetSubArray(
                        0,
                        cparam - 1,
                        out paramArray,
                        this.compiler))
                    {
                        return;
                    }
                }
                else
                {
                    DebugUtil.Assert(false);
                    return;
                }
            }

            if (methodSym.IsCtor)
            {
                methodSym.ConstructorBuilder = this.moduleBuilderEx.DefineConstructor(
                    typeBuilder,
                    flags,
                    SymUtil.GetSystemTypesFromTypeArray(
                        methodSym.ParameterTypes,
                        methodSym.ClassSym,
                        methodSym));
                if (methodSym.ConstructorBuilder == null)
                {
                    return;
                }
            }
            else if (methodSym.TypeVariables == null || methodSym.TypeVariables.Count == 0)
            {
                methodSym.MethodBuilder = this.moduleBuilderEx.DefineMethod(
                    typeBuilder,
                    name,
                    flags,
                    callConv,   
                    SymUtil.GetSystemTypeFromSym(
                        methodSym.ReturnTypeSym,
                        methodSym.ClassSym,
                        methodSym),
                    SymUtil.GetSystemTypesFromTypeArray(
                        paramArray,
                        methodSym.ClassSym,
                        methodSym));
                if (methodSym.MethodBuilder == null)
                {
                    return;
                }
            }
            else
            {
                methodSym.MethodBuilder = this.moduleBuilderEx.DefineMethod(
                    typeBuilder,
                    name,
                    flags,
                    callConv);
                if (methodSym.MethodBuilder == null)
                {
                    return;
                }

                EmitTypeVars(methodSym);

                methodSym.MethodBuilder.SetParameters(
                    SymUtil.GetSystemTypesFromTypeArray(
                        paramArray,
                        methodSym.ClassSym,
                        methodSym));

                methodSym.MethodBuilder.SetReturnType(
                    SymUtil.GetSystemTypeFromSym(
                        methodSym.ReturnTypeSym,
                        methodSym.ClassSym,
                        methodSym));
            }

            RecordEmitToken(methodSym.MethodBuilder);
            EmitMethodImpl(methodSym);
        }

        //------------------------------------------------------------
        // EMITTER.EmitMethodInfo
        //
        /// <summary>
        /// <para>(sscli) Emit additional method information into the metadata.</para>
        /// <para>Set MethodBuilder MethodImplAttributes flags.</para>
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="methodInfo"></param>
        //------------------------------------------------------------
        internal void EmitMethodInfo(METHSYM methodSym, METHINFO methodInfo)
        {
            MethodImplAttributes implFlags = 0;

            // A method def must already have been assigned via EmitMethodDef.
            DebugUtil.Assert(
                //Compiler.Options.NoCodeGen ||
                (methodSym.MethodBuilder != null || methodSym.ConstructorBuilder != null));
                //(methodSym.tokenEmit && TypeFromToken(methodSym.tokenEmit) == mdtMethodDef));

            //--------------------------------------------------
            // set impl flags
            //--------------------------------------------------
            if (methodInfo.IsMagicImpl)
            {
                if (methodSym.IsSysNative)
                {
                    // COM classic coclass constructor
                    DebugUtil.Assert(methodSym.IsCtor && !methodSym.IsStatic);
                    implFlags =
                        MethodImplAttributes.Managed | MethodImplAttributes.Runtime | MethodImplAttributes.InternalCall;
                }
                else
                {
                    // Magic method with implementation supplied by run-time.
                    // delegate construcotr and Invoke
                    DebugUtil.Assert(methodSym.ClassSym.IsDelegate);
                    implFlags = MethodImplAttributes.Managed | MethodImplAttributes.Runtime;
                }
            }
            else if (methodSym.IsAbstract)
            {
                //implFlags = 0;
            }
            else
            {
                // Our code is always managed IL.
                implFlags = MethodImplAttributes.Managed | MethodImplAttributes.IL;
            }

            if (methodInfo.IsSynchronized)
            {
                implFlags |= MethodImplAttributes.Synchronized;
            }

            //if (Compiler.Options.NoCodeGen)
            //{
            //    return;
            //}

            // Set the impl flags.
            //CheckHR(metaemit.SetMethodImplFlags(methodSym.tokenEmit, implFlags));
            if (methodSym.IsCtor)
            {
                methodSym.ConstructorBuilder.SetImplementationFlags(implFlags);
            }
            else
            {
                methodSym.MethodBuilder.SetImplementationFlags(implFlags);
            }
        }

        //------------------------------------------------------------
        // EMITTER.EmitMethodImpl
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void EmitMethodImpl(METHSYM methodSym)
        {
            DebugUtil.Assert(
                (methodSym.SlotSymWithType.AggTypeSym == null) ==
                (methodSym.SlotSymWithType.Sym == null));

            // Explicit interface method implementations have null name.
            if (methodSym.NeedsMethodImp && methodSym.SlotSymWithType != null)
            {
                EmitMethodImpl(methodSym, methodSym.SlotSymWithType);
            }
        }

        //------------------------------------------------------------
        // EMITTER.EmitMethodImpl
        //
        /// <summary></summary>
        /// <param name="methSym"></param>
        /// <param name="expImplSwt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool EmitMethodImpl(METHSYM methSym, SymWithType expImplSwt)
        {
#if DEBUG
            if (methSym.SymID == 7050)
            {
                ;
            }
#endif
            AGGSYM aggSym = methSym.ParentAggSym;
            DebugUtil.Assert(aggSym != null);
            TypeBuilder typeBuilder = null;
            MethodInfo bodyMethInfo = null;
            MethodInfo declMethInfo = null;

            if ((typeBuilder = aggSym.TypeBuilder) == null)
            {
                return false;
            }

            if (methSym == null || (bodyMethInfo = methSym.MethodInfo) == null)
            {
                return false;
            }
            if (expImplSwt == null ||
                expImplSwt.MethSym == null ||
                (declMethInfo = expImplSwt.MethSym.MethodInfo) == null)
            {
                return false;
            }
            MethodInfo declMethInfo2 = ReflectionUtil.GetConstructedMethodInfo2(
                declMethInfo,
                expImplSwt.AggTypeSym,
                null);

            try
            {
                typeBuilder.DefineMethodOverride(
                    bodyMethInfo,
                    declMethInfo2);
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            return false;
        }
        //------------------------------------------------------------
        // EMITTER.EmitMethodImpl (sscli)
        //------------------------------------------------------------
        //void EMITTER::EmitMethodImpl(PMETHSYM sym, SymWithType swtExpImpl)
        //{
        //    ASSERT(swtExpImpl && !swtExpImpl.Type() == !swtExpImpl.Sym());
        //
        //    // Don't emit method impls if there were any errors since it's not guaranteed that the needed
        //    // method def has been emitted (and we don't need them for skeleton compiles).
        //    if (compiler()->ErrorCount())
        //        return;
        //
        //    HRESULT hr = metaemit->DefineMethodImpl(
        //                sym->getClass()->tokenEmit,     // The class implementing the method
        //                sym->tokenEmit,                 // our methoddef token
        //                GetMethodRef(
        //                    swtExpImpl.Meth(), 
        //                    swtExpImpl.Type(), 
        //                    NULL));  // method being implemented
        //    CheckHR(hr);
        //}

        //------------------------------------------------------------
        // EMITTER.
        //
        //------------------------------------------------------------
        //internal void DefineParam(mdToken tokenMethProp, int index, mdToken *paramToken);

        //------------------------------------------------------------
        // EMITTER.EmitParamProp
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodBuilder"></param>
        /// <param name="index"></param>
        /// <param name="typeSym"></param>
        /// <param name="paramInfo"></param>
        /// <param name="hasDefaultValue"></param>
        /// <param name="etDefaultValue"></param>
        /// <param name="blob"></param>
        //------------------------------------------------------------
        internal void EmitParamProp(
            MethodBuilder methodBuilder,
            int index,
            TYPESYM typeSym,
            PARAMINFO paramInfo,
            bool hasDefaultValue,
            int etDefaultValue,
            List<Object> defaultValues)
        {
            if (methodBuilder == null || paramInfo == null)
            {
                return;
            }

            ParameterBuilder paramBuilder = null;
            ParameterAttributes attr = paramInfo.GetParameterAttributes();
            if (hasDefaultValue)
            {
                attr |= ParameterAttributes.HasDefault;
            }

            try
            {
                paramBuilder = methodBuilder.DefineParameter(
                    index,
                    attr,
                    paramInfo.Name);
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }
            paramInfo.ParameterBuilder = paramBuilder;

            if (hasDefaultValue)
            {
                try
                {
                    paramBuilder.SetConstant(defaultValues[index]);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        //------------------------------------------------------------
        // EMITTER.
        //
        //------------------------------------------------------------
        //internal void *EmitMethodRVA(PMETHSYM sym, ULONG cbCode, ULONG alignment);

        //------------------------------------------------------------
        // EMITTER.EmitLocalVariable (1)
        //
        /// <summary>
        /// Define a System.Reflection.Emit.LocalBuilder instance
        /// and set it to the given LOVARSYM instance.
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="locvarSym"></param>
        //------------------------------------------------------------
        internal LocalBuilder EmitLocalVariable(
            METHSYM methodSym,
            TYPESYM typeSym,
            bool isPinned)
        {
            if (methodSym == null || typeSym == null)
            {
                return null;
            }

            ILGenerator gen = null;
            Exception excp = null;

            if (methodSym.MethodBuilder != null)
            {
                gen = methodSym.MethodBuilder.GetILGenerator();
            }
            else if (methodSym.ConstructorBuilder != null)
            {
                gen = methodSym.ConstructorBuilder.GetILGenerator();
            }
            DebugUtil.Assert(gen != null);

            try
            {
                return gen.DeclareLocal(
                    SymUtil.GetSystemTypeFromSym(typeSym, methodSym.ClassSym, methodSym),
                    isPinned);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // EMITTER.EmitLocalVariable (2)
        //
        /// <summary>
        /// Define a System.Reflection.Emit.LocalBuilder instance
        /// and set it to the given LOVARSYM instance.
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="locvarSym"></param>
        //------------------------------------------------------------
        internal void EmitLocalVariable(METHSYM methodSym, LOCVARSYM locvarSym)
        {
            if (methodSym == null || methodSym.MethodBuilder == null || locvarSym == null)
            {
                return;
            }
            Exception excp = null;

            try
            {
                locvarSym.LocSlotInfo.LocalBuilder =
                    methodSym.MethodBuilder.GetILGenerator().DeclareLocal(
                        SymUtil.GetSystemTypeFromSym(locvarSym.TypeSym, methodSym.ClassSym, methodSym),
                        locvarSym.IsPINNEDSYM);
                locvarSym.LocSlotInfo.Index = locvarSym.LocSlotInfo.LocalBuilder.LocalIndex;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
        }

        //------------------------------------------------------------
        // EMITTER.ResetMethodFlags
        //
        /// <summary>
        /// <para>rewrites the flags for a method</para>
        /// <para>Does nothing for now.</para>
        /// </summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal void ResetMethodFlags(METHSYM sym)
        {
            // We do not reset Code RVA and MethodImpl flags for now.
        }

        //void EMITTER::ResetMethodFlags(METHSYM *sym)
        //{
        //    ASSERT(sym->tokenEmit != 0);
        //
        //    CheckHR(metaemit->SetMethodProps(sym->tokenEmit, GetMethodFlags(sym), UINT_MAX, UINT_MAX));
        //}

        //------------------------------------------------------------
        // EMITTER.GetMethodFlags
        //
        /// <summary>
        /// Returns the flags for a method
        /// </summary>
        /// <param name="methodSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MethodAttributes GetMethodFlags(METHSYM methodSym)
        {
            MethodAttributes flags = 0;

            // Determine the flags.
            //flags = FlagsFromAccess(methodSym.Access);
            switch (methodSym.Access)
            {
                case ACCESS.PUBLIC:
                    flags = MethodAttributes.Public;
                    break;

                case ACCESS.PROTECTED:
                    flags = MethodAttributes.Family;
                    break;

                case ACCESS.PRIVATE:
                    flags = MethodAttributes.Private;
                    break;

                case ACCESS.INTERNAL:
                    flags = MethodAttributes.Assembly;
                    break;

                case ACCESS.INTERNALPROTECTED:
                    flags = MethodAttributes.FamORAssem;
                    break;

                default:
                    DebugUtil.Assert(false);
                    break;
            }

            if (!methodSym.HideByName)
            {
                flags |= MethodAttributes.HideBySig;
            }
            if (methodSym.IsStatic)
            {
                flags |= MethodAttributes.Static;
            }
            if (methodSym.IsCtor)
            {
                flags |= MethodAttributes.SpecialName;
            }

            if (methodSym.IsVirtual)
            {
                DebugUtil.Assert(!methodSym.IsCtor);
                flags |= MethodAttributes.Virtual;
            }
            else if (methodSym.IsMetadataVirtual)
            {
                // Non-virtual in the language, but be virtual in the metadata. Also emit 
                // mdFinal so we read it in as non-virtual/sealed.
                flags |= (MethodAttributes.Virtual | MethodAttributes.Final);
            }

            if (methodSym.IsVirtual || methodSym.IsMetadataVirtual)
            {
                if (methodSym.IsOverride && !methodSym.IsNewSlot)
                {
                    flags |= MethodAttributes.ReuseSlot;
                }
                else
                {
                    flags |= MethodAttributes.NewSlot;
                }
            }

            if (methodSym.IsAbstract)
            {
                flags |= MethodAttributes.Abstract;
            }

            if (methodSym.IsOperator || methodSym.IsAnyAccessor)
            {
                flags |= MethodAttributes.SpecialName;
            }
            DebugUtil.Assert(
                methodSym.SlotSymWithType.Sym == null ||
                methodSym.SlotSymWithType.AggTypeSym != null && methodSym.IsMetadataVirtual);

            // Enforce C#'s notion of internal virtual
            // If the method is private or internal and virtual but not final
            // Set the new bit ot indicate that it can only be overriden
            // by classes that can normally access this member.
            if (((flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Private ||
                (flags & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem ||
                (flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly) &&
                ((flags & (MethodAttributes.Virtual | MethodAttributes.Final)) == MethodAttributes.Virtual))
            {
                flags |= MethodAttributes.CheckAccessOnOverride;
            }

            return flags;
        }

        //------------------------------------------------------------
        // EMITTER.GetMembVarFlags
        //
        /// <summary>
        /// getthe flags for a field
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FieldAttributes GetMembVarFlags(MEMBVARSYM sym)
        {
            FieldAttributes flags;
            // Determine the flags.
            flags = FieldFlagsFromAccess(sym.Access);

            if (sym.IsStatic)
            {
                flags |= FieldAttributes.Static;
            }

            if (sym.IsConst)
            {
                if (sym.TypeSym.IsPredefType(PREDEFTYPE.DECIMAL))
                {
                    // COM+ doesn;t support decimal constants
                    // they are initialized with field initializers in the static constructor
                    flags |= (FieldAttributes.Static | FieldAttributes.InitOnly);
                }
                else
                {
                    flags |= (FieldAttributes.Static | FieldAttributes.Literal);
                }
            }
            else if (sym.IsReadOnly)
            {
                flags |= FieldAttributes.InitOnly;
            }

            return flags;
        }

        //------------------------------------------------------------
        // EMITTER.GetPropertyFlags
        //
        /// <summary>
        /// <para>Return 0 for now.</para>
        /// </summary>
        /// <param name="propertySym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PropertyAttributes GetPropertyFlags(PROPSYM propertySym)
        {
            PropertyAttributes flags = 0;

            return flags;
        }

        //------------------------------------------------------------
        // EMITTER.GetEventFlags
        //
        /// <summary>
        /// <para>Returns the flags to be set for an event.</para>
        /// <para>Return 0 for now.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EventAttributes GetEventFlags(EVENTSYM sym)
        {
            EventAttributes flags = 0;
            return flags;
        }

        //------------------------------------------------------------
        // EMITTER.
        //
        //------------------------------------------------------------
        //internal void EmitDebugMethodInfoStart(METHSYM * sym);
        //internal void EmitDebugMethodInfoStop(METHSYM * sym, int ilOffsetEnd);
        //internal void EmitDebugBlock(METHSYM * sym, int count, unsigned int * offsets, SourceExtent * extents);
        //internal void EmitDebugTemporary(TYPESYM * type, PCWSTR name, mdToken tkLocalVarSig, unsigned slot);
        //internal void EmitDebugLocal(LOCVARSYM * sym, mdToken tkLocalVarSig, int ilOffsetStart, int ilOffsetEnd);
        //internal void EmitDebugLocalConst(LOCVARSYM * sym);
        //internal void EmitDebugScopeStart(int ilOffsetStart);
        //internal void EmitDebugScopeEnd(int ilOffsetEnd);

        //------------------------------------------------------------
        // EMITTER.EmitCustomAttribute
        //
        /// <summary>
        /// <para>Emit a user defined custom attribute.</para>
        /// <para>If sym is null, we emit attributes to the current assembly.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal void EmitCustomAttribute(
            SYM sym,
            CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null)
            {
                return;
            }

            if (sym == null)
            {
                DebugUtil.Assert(this.assemblyBuilderEx != null);
                this.assemblyBuilderEx.SetCustomAttribute(caBuilder);
                return;
            }

            Exception excp = null;
            switch (sym.Kind)
            {
                default:
                    break;

                case SYMKIND.AGGSYM:
                    {
                        AGGSYM aggSym = sym as AGGSYM;
                        if (aggSym != null)
                        {
                            aggSym.SetCustomAttribute(caBuilder, out excp);
                        }
                    }
                    return;

                case SYMKIND.METHSYM:
                    {
                        METHSYM methodSym = sym as METHSYM;
                        if (methodSym != null)
                        {
                            methodSym.SetCustomAttribute(caBuilder, out excp);
                        }
                    }
                    return;

                case SYMKIND.MEMBVARSYM:
                    {
                        MEMBVARSYM fieldSym = sym as MEMBVARSYM;
                        if (fieldSym != null)
                        {
                            fieldSym.SetCustomAttribute(caBuilder, out excp);
                        }
                    }
                    return;

                case SYMKIND.PROPSYM:
                    {
                        PROPSYM propertySym = sym as PROPSYM;
                        if (propertySym != null)
                        {
                            propertySym.SetCustomAttribute(caBuilder, out excp);
                        }
                    }
                    return;

                case SYMKIND.EVENTSYM:
                    {
                        EVENTSYM eventSym = sym as EVENTSYM;
                        if (eventSym != null)
                        {
                            eventSym.SetCustomAttribute(caBuilder, out excp);
                        }
                    }
                    return;
            }
        }
        //------------------------------------------------------------
        // EMITTER::EmitCustomAttribute (sscli)
        //------------------------------------------------------------
        //internal void EmitCustomAttribute(
        //	BASENODE *parseTree,
        //	mdToken token,
        //	METHSYM *method,
        //	BYTE *buffer,
        //	unsigned bufferSize)
        //{
        //    ASSERT(method->getClass()->typeVarsAll->size == 0);
        //    ASSERT(method->typeVars->size == 0);
        //
        //    mdToken ctorToken = GetMethodRef(method, method->getClass()->getThisType(), NULL);
        //    ASSERT(ctorToken != mdTokenNil);
        //    HRESULT hr = S_OK;
        //
        //    if (token == mdtAssembly) {
        //        AGGSYM *cls = method->getClass();
        //        hr = compiler()->linker->EmitAssemblyCustomAttribute(compiler()->assemID, compiler()->curFile->GetOutFile()->idFile,
        //            ctorToken, buffer, bufferSize, cls->isSecurityAttribute, cls->isMultipleAttribute);
        //    } else if (method->getClass()->isSecurityAttribute) {
        //        // Security attributes must be accumulated for later emitting.
        //        SaveSecurityAttribute(token, ctorToken, method, buffer, bufferSize);
        //    } else {
        //        hr = metaemit->DefineCustomAttribute(token, ctorToken, buffer, bufferSize, NULL);
        //    }
        //
        //    if (FAILED(hr)) {
        //        HandleAttributeError(hr, parseTree, method);
        //    }
        //}

        //------------------------------------------------------------
        // EMITTER.HasSecurityAttributes
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasSecurityAttributes()
        {
            return savedSecurityAttributeCount > 0;
        }

        //------------------------------------------------------------
        // EMITTER.EmitSecurityAttributes
        //
        /// <summary>
        /// Emit pending security attributes on a symbol.
        /// </summary>
        //------------------------------------------------------------
        internal void EmitSecurityAttributes(BASENODE parseTree, /*mdToken*/ int token)
        {
            //if (cSecAttrSave == 0)
            //    return;  // nothing to do.
            //
            //ASSERT(token == tokenSecAttrSave);
            //
            //COR_SECATTR * rSecAttrs = STACK_ALLOC(COR_SECATTR, cSecAttrSave);
            //SECATTRSAVE * pSecAttrSave;
            //int i;
            //
            //// Put all the saved attributes into one array.
            //pSecAttrSave = listSecAttrSave;
            //i = 0;
            //while (pSecAttrSave) {
            //    rSecAttrs[i].tkCtor = pSecAttrSave->ctorToken;
            //    rSecAttrs[i].pCustomAttribute = pSecAttrSave->buffer;
            //    rSecAttrs[i].cbCustomAttribute = pSecAttrSave->bufferSize;
            //    ++i;
            //    pSecAttrSave = pSecAttrSave->next;
            //}
            //ASSERT(i == (int)cSecAttrSave);
            //
            //HRESULT hr;
            //hr = metaemit->DefineSecurityAttributeSet(token, rSecAttrs, cSecAttrSave, NULL);
            //if (FAILED(hr)) {
            //    HandleAttributeError(hr, parseTree, listSecAttrSave->method); // use first attribute for error reporting.
            //}
            //
            //FreeSavedSecurityAttributes();
        }

        //------------------------------------------------------------
        // EMITTER.EmitTypeForwarder
        //
        /// <summary>
        /// Emit a type forwarder (exported type pointing to a seperate assembly) for the type specified.
        /// </summary>
        //------------------------------------------------------------
        internal void EmitTypeForwarder(AGGTYPESYM type)
        {
            //WCHAR wszName[MAX_FULLNAME_SIZE];
            //MetaDataHelper::GetFullName(type->getAggregate(), wszName, lengthof(wszName));
            //PAGGSYM pagSym = type->getAggregate();
            //
            //mdToken tkAssemblyRef = GetScopeForTypeRef(pagSym);
            //ASSERT(TypeFromToken(tkAssemblyRef) == mdtAssemblyRef);
            //compiler()->linker->ExportTypeForwarder(tkAssemblyRef, wszName, 0, &pagSym->tokenComType);
            //
            //EmitNestedTypeForwarder(pagSym, tkAssemblyRef);
        }

        //internal void EmitNestedTypeForwarder(AGGSYM *agg, mdAssemblyRef tkAssemblyRef);

        //------------------------------------------------------------
        // EMITTER.GetMethodRef
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <param name="methodTypeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MethodInfo GetMethodRef(
            METHSYM methodSym,
            AGGTYPESYM aggTypeSym,
            TypeArray methodTypeArgs)
        {
            DebugUtil.Assert(aggTypeSym != null && aggTypeSym.GetAggregate() == methodSym.ClassSym);
            TypeArray classTypeArgs = aggTypeSym.AllTypeArguments;
            MethodInfo methodInfo;

            if (classTypeArgs.Count > 0)
            {
                classTypeArgs = Compiler.MainSymbolManager.SubstTypeArray(classTypeArgs, SubstTypeFlagsEnum.NormAll);
            }

            if (methodTypeArgs != null && methodTypeArgs.Count > 0)
            {
                methodTypeArgs = Compiler.MainSymbolManager.SubstTypeArray(methodTypeArgs, SubstTypeFlagsEnum.NormAll);

                // A generic method instance.
                //mdToken * ptok = pmap.GetTokPtr(methodSym, classTypeArgs, methodTypeArgs);
                methodInfo = typeMemberMap.GetMemberInfo(methodSym, classTypeArgs, methodTypeArgs) as MethodInfo;
                if (methodInfo != null)
                {
                    return methodInfo;
                }
                DebugUtil.Assert(!methodSym.IsBogus);
                DebugUtil.Assert(!aggTypeSym.IsBogus);
                //*ptok = GetMethodInstantiation(GetMethodRef(methodSym, aggTypeSym, null), methodTypeArgs);
                methodInfo = GetMethodInstantiation(GetMethodRef(methodSym, aggTypeSym, null), methodTypeArgs);

                //return *ptok;
                return methodInfo;
            }

            if (classTypeArgs.Count > 0)
            {
                // In a generic type.
                //mdToken * ptok = pmap.GetTokPtr(methodSym, classTypeArgs, null);
                methodInfo = typeMemberMap.GetMemberInfo(methodSym, classTypeArgs, null) as MethodInfo;
                if (methodInfo != null)
                {
                    return methodInfo;
                }
                DebugUtil.Assert(!methodSym.IsBogus);
                DebugUtil.Assert(!aggTypeSym.IsBogus);
                //*ptok = GetMethodRefGivenParent(methodSym, GetTypeSpec(aggTypeSym));
                methodInfo = GetMethodRefGivenParent(methodSym, GetTypeSpec(aggTypeSym));

                //return *ptok;
                return methodInfo;
            }

            // Not in a generic type.
            if (methodSym.MethodBuilder == null)
            {
                // Create a memberRef token for this symbol.
                DebugUtil.Assert(!methodSym.IsBogus);
                DebugUtil.Assert(!aggTypeSym.IsBogus);

                // The runtime doesn't allow typedef in this case, even though we might have one
                // in the case of a "base" call where we have a memberref to a non-existent member.

                //methodSym.tokenEmit = GetMethodRefGivenParent(methodSym, GetAggRef(methodSym.getClass(), true));
                //RecordEmitToken(&methodSym.tokenEmit);
                methodSym.MethodBuilder=
                    GetMethodRefGivenParent(methodSym, GetAggRef(methodSym.ClassSym, true)) as MethodBuilder;
                RecordEmitToken(methodSym.MethodBuilder);
            }

            //return methodSym.tokenEmit;
            return methodSym.MethodBuilder;
        }

        //------------------------------------------------------------
        // EMITTER.GetMethodInstantiation
        //------------------------------------------------------------
        internal MethodInfo GetMethodInstantiation(MethodInfo info, TypeArray typeArgs)
        {
            throw new NotImplementedException("EMITTER.GetMethodInstantiation");
        }
        //internal mdToken GetMethodInstantiation(mdToken parent, TypeArray * typeArgsMeth);
        //internal MethodInfo GetMethodInstantiation(mdToken parent, TypeArray *typeArgsMeth)
        //{
        //    mdToken tok;
        //    PCOR_SIGNATURE sig;
        //    int len = 0;
        //
        //    sig = BeginSignature();
        //
        //    sig = EmitSignatureByte(sig, IMAGE_CEE_CS_CALLCONV_INSTANTIATION);
        //    sig = EmitSignatureUInt(sig, typeArgsMeth->size);
        //    for (int i = 0; i < typeArgsMeth->size; i++) {
        //        sig = EmitSignatureType(sig, typeArgsMeth->Item(i));
        //    }
        //
        //    sig = EndSignature(sig, &len);
        //
        //    ASSERT(parent);
        //    CheckHR(metaemit->DefineMethodSpec(parent, sig, len, &tok));
        //
        //    return tok;
        //}

        //------------------------------------------------------------
        // EMITTER.
        //------------------------------------------------------------
        //internal mdToken GetMembVarRef(PMEMBVARSYM sym);

        //------------------------------------------------------------
        // EMITTER.
        //------------------------------------------------------------
        //internal mdToken GetMembVarRef(PMEMBVARSYM sym, AGGTYPESYM * aggType);

        //------------------------------------------------------------
        // EMITTER.GetTypeRef
        //
        /// <summary>
        /// <para>(sscli) Get a type ref for a type for use in emitting code or metadata.
        /// Returns a typeDef, typeRef or typeSpec.  If noDefAllowed is
        /// set then only a typeRef or typeSpec is returned (which could be inefficient).</para>
        /// <para></para>
        /// </summary>
        /// <param name="sym">(sscli) noDefAllowed has the default value false.</param>
        /// <param name="noDefAllowed"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type GetTypeRef(
            TYPESYM sym,
            bool noDefAllowed) // = false
        {
            DebugUtil.Assert(sym != null && !sym.IsVOIDSYM);

            switch (sym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    if ((sym as AGGTYPESYM).AllTypeArguments.Count == 0)
                    {
                        return GetAggRef(sym.GetAggregate(), noDefAllowed);
                    }
                    // Fall through.
                    goto case SYMKIND.ERRORSYM;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.TYVARSYM:
                case SYMKIND.ERRORSYM:
                    // We use typespecs instead...
                    return GetTypeSpec(sym);

                default:
                    DebugUtil.VsFail("Bad type in GetTypeRef");
                    return null;
            }
        }

        //------------------------------------------------------------
        // EMITTER.GetAggRef
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="noDefAllowd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type GetAggRef(
            AGGSYM sym,
            bool noDefAllowd)   // = false
        {
            return sym.Type;
        }

        //------------------------------------------------------------
        // EMITTER.GetArrayMethodRef
        //
        /// <summary>
        /// <para>(sscli)
        /// For accessing arrays, the COM+ EE defines four "pseudo-methods" on arrays:
        /// constructor, load, store, and load address. This function gets the
        /// memberRef for one of these pseudo-methods.</para>
        /// <para>Return the MethodInfo of the specified pseudo-method of the array.</para>
        /// </summary>
        /// <param name="arraySym"></param>
        /// <param name="methodId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MethodInfo GetArrayMethodRef(
            ARRAYSYM arraySym,
            ARRAYMETHOD methodId,
            AGGSYM tvAggSym,
            METHSYM tvMethodSym)
        {
            DebugUtil.Assert(arraySym != null && arraySym.ElementTypeSym != null);
            DebugUtil.Assert(
                methodId >= 0 && methodId <= ARRAYMETHOD.COUNT &&
                methodId != ARRAYMETHOD.CTOR);

            if (arraySym.ArrayPseudoMethods[(int)methodId] != null)
            {
                return arraySym.ArrayPseudoMethods[(int)methodId];
            }

            Type arrayType = SymUtil.GetSystemTypeFromSym(arraySym, tvAggSym, tvMethodSym);
            int rank = arraySym.Rank;

            if (arrayType == null || arraySym.Rank == 0)
            {
                return null;
            }
            Type[] paramTypes;

            string name = null;

            // Get the name and signature for the particular pseudo-method.
            switch (methodId)
            {
                case ARRAYMETHOD.LOAD:
                    name = "Get";
                    paramTypes = new Type[rank];
                    for (int i = 0; i < rank; ++i)
                    {
                        paramTypes[i] = SystemType.IntType;
                    }
                    break;

                case ARRAYMETHOD.GETAT:
                    //name = "GetAt";
                    //break;
                    goto default;

                case ARRAYMETHOD.LOADADDR:
                    name = "Address";
                    paramTypes = new Type[rank];
                    for (int i = 0; i < rank; ++i)
                    {
                        paramTypes[i] = SystemType.IntType;
                    }
                    break;

                case ARRAYMETHOD.STORE:
                    name = "Set";
                    paramTypes = new Type[rank+1];
                    for (int i = 0; i < rank; ++i)
                    {
                        paramTypes[i] = SystemType.IntType;
                    }
                    paramTypes[rank] = SymUtil.GetSystemTypeFromSym(
                        arraySym.ElementTypeSym,
                        tvAggSym,
                        tvMethodSym);
                    break;

                default:
                    DebugUtil.Assert(false);
                    return null;
            }

            try
            {
                arraySym.ArrayPseudoMethods[(int)methodId] = arrayType.GetMethod(name, paramTypes);
            }
            catch (NotSupportedException)
            {
                DebugUtil.Assert(this.assemblyBuilderEx != null);
                Type returnType = null;
                switch (methodId)
                {
                    case ARRAYMETHOD.LOAD:
                        returnType = SymUtil.GetSystemTypeFromSym(
                            arraySym.ElementTypeSym,
                            tvAggSym,
                            tvMethodSym);
                        break;

                    case ARRAYMETHOD.GETAT:
                        goto default;

                    case ARRAYMETHOD.LOADADDR:
                        returnType =
                            (SymUtil.GetSystemTypeFromSym(
                                arraySym.ElementTypeSym,
                                tvAggSym,
                                tvMethodSym)).MakeByRefType();
                        break;

                    case ARRAYMETHOD.STORE:
                        returnType = SystemType.VoidType;
                        break;

                    default:
                        throw new Exception("EMITTER.GetArrayMethodRef: invalid methodId.");
                }

                arraySym.ArrayPseudoMethods[(int)methodId] = this.moduleBuilderEx.GetArrayMethod(
                    arrayType,
                    name,
                    CallingConventions.Standard | CallingConventions.HasThis,
                    returnType,
                    paramTypes);
            }
            catch (ArgumentException)
            {
                arraySym.ArrayPseudoMethods[(int)methodId] = null;
                throw new Exception("EMITTER.GetArrayMethodRef");
            }
            catch (AmbiguousMatchException)
            {
                arraySym.ArrayPseudoMethods[(int)methodId] = null;
                throw new Exception("EMITTER.GetArrayMethodRef");
            }

            if (arraySym.ArrayPseudoMethods[(int)methodId] == null)
            {
                DebugUtil.Assert(false);
            }
            return arraySym.ArrayPseudoMethods[(int)methodId];
        }

        //------------------------------------------------------------
        // EMITTER.GetArrayConstructor
        //
        /// <summary>
        /// <para>Return the ConstructorInfo of the array.</para>
        /// <para>In place of GetArrayMethodRef of sscli.</para>
        /// </summary>
        /// <param name="arraySym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
#if false
        internal ConstructorInfo GetArrayConstructor(ARRAYSYM arraySym)
        {
            if (arraySym.ArrayConstructor != null)
            {
                return arraySym.ArrayConstructor;
            }

            Type arrayType = arraySym.GetSystemType();
            if (arrayType == null || arraySym.Rank == 0)
            {
                return null;
            }

            Type[] argTypes = new Type[arraySym.Rank];
            Type intType = typeof(int);
            for (int i = 0; i < arraySym.Rank; ++i)
            {
                argTypes[i] = intType;
            }

            try
            {
                arraySym.ArrayConstructor = arrayType.GetConstructor(argTypes);
            }
            catch (NotSupportedException)
            {
                DebugUtil.Assert(this.assemblyBuilderEx != null);

                arraySym.ArrayConstructor = this.assemblyBuilderEx.GetArrayMethod(
                    arrayType,
                    ".ctor",
                    CallingConventions.Standard,
                    arrayType,
                    argTypes);
            }
            catch (ArgumentException)
            {
                arraySym.ArrayConstructor = null;
            }
            catch (AmbiguousMatchException)
            {
                arraySym.ArrayConstructor = null;
            }

            if (arraySym.ArrayConstructor == null)
            {
                DebugUtil.Assert(false);
            }
            return arraySym.ArrayConstructor;
        }
#endif
        //------------------------------------------------------------
        // EMITTER.GetArrayConstructMethod
        //
        /// <summary></summary>
        /// <param name="arraySym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MethodInfo GetArrayConstructMethod(
            ARRAYSYM arraySym,
            AGGSYM tvAggSym,
            METHSYM tvMethodSym)
        {
            if (arraySym.ArrayConstructorMethodInfo != null)
            {
                return arraySym.ArrayConstructorMethodInfo;
            }

            Type arrayType = SymUtil.GetSystemTypeFromSym(arraySym, tvAggSym, tvMethodSym);
            if (arrayType == null || arraySym.Rank == 0)
            {
                return null;
            }

            Type[] argTypes = new Type[arraySym.Rank];
            Type intType = typeof(int);
            for (int i = 0; i < arraySym.Rank; ++i)
            {
                argTypes[i] = intType;
            }

            try
            {
                arraySym.ArrayConstructorMethodInfo = this.moduleBuilderEx.GetArrayMethod(
                    arrayType,
                    ".ctor",
                    CallingConventions.Standard | CallingConventions.HasThis,
                    SystemType.VoidType,    //arrayType,
                    argTypes);
            }
            catch (NotSupportedException)
            {
                DebugUtil.Assert(this.assemblyBuilderEx != null);
            }
            catch (ArgumentException)
            {
                arraySym.ArrayConstructorMethodInfo = null;
            }
            catch (AmbiguousMatchException)
            {
                arraySym.ArrayConstructorMethodInfo = null;
            }

            if (arraySym.ArrayConstructorMethodInfo == null)
            {
                DebugUtil.Assert(false);
            }
            return arraySym.ArrayConstructorMethodInfo;
        }

        //------------------------------------------------------------
        // EMITTER.GetSignatureRef
        //------------------------------------------------------------
        //internal mdToken GetSignatureRef(TypeArray * pta);
        //internal mdString GetStringRef(const STRCONST * string);
        //internal mdToken GetModuleToken();

        //------------------------------------------------------------
        // EMITTER.GetGlobalFieldDef (1)
        //
        //------------------------------------------------------------
        internal FieldInfo GetGlobalFieldDef(
            METHSYM methodSym,
            int count,
            TYPESYM typeSym,
            int size)	// = 0
        {
            DebugUtil.Assert(!compiler.FEncBuild());
            DebugUtil.Assert(typeSym != null || size > 0);

            if (this.globalTypeBuilder == null)
            {
                Guid manifestGuid;
                if (this.assemblyBuilderEx != null)
                {
                    manifestGuid = this.assemblyBuilderEx.ManifestModuleVersionID;
                }
                else
                {
                    manifestGuid = Guid.Empty;
                }
                string globalTypeName = String.Format(
                    "<PrivateImplementationDetails>\u005B{0}\u005D",
                    manifestGuid.ToString());

                this.globalTypeBuilder = this.moduleBuilderEx.DefineType(
                    globalTypeName,
                    TypeAttributes.Class | TypeAttributes.NotPublic,
                    typeof(object),
                    Type.EmptyTypes);
                CompilerGeneratedAttrBind.EmitAttribute(compiler, this.globalTypeBuilder);
            }

            if (typeSym == null)
            {
                //DebugUtil.Assert(size > 0);
                switch (size)
                {
                    case 1:
                        typeSym = compiler.GetReqPredefType(PREDEFTYPE.BYTE, true);
                        break;
                    case 2:
                        typeSym = compiler.GetReqPredefType(PREDEFTYPE.SHORT, true);
                        break;
                    case 4:
                        typeSym = compiler.GetReqPredefType(PREDEFTYPE.INT, true);
                        break;
                    case 8:
                        typeSym = compiler.GetReqPredefType(PREDEFTYPE.LONG, true);
                        break;
                    default:
                        {
                            throw new NotImplementedException("EMITTER.GetGlobalFieldDef");
                            //break;
                        }
                }
            }

            int token;
            if (!this.assemblyBuilderEx.GetMetadataToken(methodSym.MethodInfo, out token))
            {
                token = 0;
            }
            string fieldName = String.Format("$$method0x{0:x}-{1}", token, count);

            return this.moduleBuilderEx.DefineField(
                globalTypeBuilder,
                fieldName,
                SymUtil.GetSystemTypeFromSym(typeSym, null, null),
                FieldAttributes.Assembly | FieldAttributes.Static | FieldAttributes.PrivateScope,
                null);
        }
        //------------------------------------------------------------
        // EMITTER::GetGlobalFieldDef (sscli)
        //------------------------------------------------------------
        //mdToken EMITTER::GetGlobalFieldDef(METHSYM * sym, unsigned int count, TYPESYM * type, unsigned int size)
        //{
        //    ASSERT(!compiler()->FEncBuild());
        //
        //    WCHAR bufferW[255];
        //    ASSERT(type || size);
        //
        //    if (mdTokenNil == globalTypeToken) {
        //        const WCHAR szGlobalFieldPrefix[] = L"<PrivateImplementationDetails>";
        //        const int cchGlobalFieldPrefix = lengthof(szGlobalFieldPrefix) - 1;
        //        GUID guidModule;
        //        CComPtr<IMetaDataImport> metaimport;
        //
        //        CheckHR(metaemit->QueryInterface(IID_IMetaDataImport, (void**) &metaimport));
        //        CheckHR(metaimport->GetScopeProps( NULL, 0, NULL, &guidModule));
        //        CheckHR(StringCchCopyW(bufferW, lengthof(bufferW), szGlobalFieldPrefix));
        //        if (!StringFromGUID2( guidModule, bufferW + cchGlobalFieldPrefix, lengthof(bufferW) - cchGlobalFieldPrefix))
        //            CheckHR(STRSAFE_E_INSUFFICIENT_BUFFER);
        //
        //        CheckHR(metaemit->DefineTypeDef(
        //                bufferW,               // Full name of TypeDef
        //                tdClass | tdNotPublic,          // CustomValue flags
        //                GetTypeRef(compiler()->GetReqPredefType(PT_OBJECT)),   // extends this TypeDef or typeref
        //                NULL,                           // Implements interfaces
        //                & globalTypeToken));
        //        CompilerGeneratedAttrBind::EmitAttribute(compiler(), globalTypeToken);
        //    }
        //
        //    mdToken dummyToken = mdTokenNil;
        //    if (!type) {
        //        ASSERT(size);
        //
        //        switch (size) {
        //        case 1:
        //            type = compiler()->GetReqPredefType(PT_BYTE);
        //            break;
        //        case 2:
        //            type = compiler()->GetReqPredefType(PT_SHORT);
        //            break;
        //        case 4:
        //            type = compiler()->GetReqPredefType(PT_INT);
        //            break;
        //        case 8:
        //            type = compiler()->GetReqPredefType(PT_LONG);
        //            break;
        //        default:
        //            {
        //                CompGenTypeToken ** pcgttSearch = &cgttHead;
        //                while (*pcgttSearch && (*pcgttSearch)->size < size)
        //                    pcgttSearch = &(*pcgttSearch)->next;
        //
        //                if (*pcgttSearch && (*pcgttSearch)->size == size) {
        //                    dummyToken = (*pcgttSearch)->tkTypeDef;
        //                    break;
        //                }
        //
        //                CompGenTypeToken * cgttNew = (CompGenTypeToken *) tokrefHeap.Alloc(sizeof(CompGenTypeToken));
        //                StringCchPrintfW(bufferW, lengthof(bufferW), L"__StaticArrayInitTypeSize=%u", size);
        // 
        //                CheckHR(metaemit->DefineNestedType(
        //                    bufferW,                        // Simple Name of TypeDef for nested classes.
        //                    tdExplicitLayout | tdNestedPrivate | tdSealed,     // CustomValue flags
        //                    GetTypeRef(compiler()->GetReqPredefType(PT_VALUE)),   // extends this TypeDef or typeref
        //                    NULL,                // Implements interfaces
        //                    globalTypeToken,
        //                    & dummyToken));
        //    
        //                CheckHR(metaemit->SetClassLayout(
        //                    dummyToken,
        //                    1,
        //                    NULL,
        //                    size));
        //                cgttNew->next = *pcgttSearch;
        //                cgttNew->size = size;
        //                cgttNew->tkTypeDef = dummyToken;
        //                *pcgttSearch = cgttNew;
        //                break;
        //            }
        //        }
        //    }
        //
        //    StringCchPrintfW(bufferW, lengthof(bufferW), L"$$method%#x-%d", sym->tokenEmit, count);
        //
        //    PCOR_SIGNATURE sig;
        //    int cbSig;
        //
        //    mdToken tokenTemp;
        //
        //    sig = BeginSignature();
        //    sig = EmitSignatureByte(sig, IMAGE_CEE_CS_CALLCONV_FIELD);
        //    if (dummyToken != mdTokenNil) {
        //        sig = EmitSignatureByte(sig, ELEMENT_TYPE_VALUETYPE);
        //        sig = EmitSignatureToken(sig, dummyToken);
        //    } else {
        //        sig = EmitSignatureType(sig, type);
        //    }
        //    sig = EndSignature(sig, &cbSig);
        //    
        //    CheckHR(metaemit->DefineField(
        //                globalTypeToken,             // Parent TypeDef
        //                bufferW, // Name of member
        //                fdAssembly | fdStatic | fdPrivateScope,  // Member attributes
        //                sig, cbSig,                 // COM+ signature
        //                ELEMENT_TYPE_VOID,          // const type
        //                NULL, 0,                    // value of constant
        //                & tokenTemp));
        //
        //    return tokenTemp; 
        //}

        //------------------------------------------------------------
        // EMITTER.GetGlobalFieldDef (2)
        //
        /// <summary>
        /// Create a FieldBuilder instance of an Uninitialized data field.
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="count"></param>
        /// <param name="typeSym"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FieldBuilder GetGlobalFieldDef(
            METHSYM methodSym,
            int count,
            byte[] data)
        {
            //string name = String.Format("$$method{0}-{1}", methodSym.MethodInfo.MetadataToken, count);
            string name = String.Format("$$method{0}-{1}", methodSym.SymID, count);

            return this.moduleBuilderEx.DefineInitializedDataField(
                name,
                data,
                FieldAttributes.Assembly | FieldAttributes.Static | FieldAttributes.PrivateScope);
        }

        //------------------------------------------------------------
        // EMITTER.GetGlobalFieldDef (2)
        //
        /// <summary>
        /// Create a FieldBuilder instance of an Uninitialized data field.
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="count"></param>
        /// <param name="typeSym"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FieldBuilder GetGlobalFieldDef(
            METHSYM methodSym,
            int count,
            int size)
        {
            //string name = String.Format("$$method{0}-{1}", methodSym.MethodInfo.MetadataToken, count);
            string name = String.Format("$$method{0}-{1}", methodSym.SymID, count);

            return this.moduleBuilderEx.DefineUninitializedDataField(
                name,
                size,
                FieldAttributes.Assembly | FieldAttributes.Static | FieldAttributes.PrivateScope);
        }

        //------------------------------------------------------------
        // EMITTER.
        //------------------------------------------------------------
        //internal mdToken GetErrRef(ERRORSYM * err);
        //internal mdToken GetErrAssemRef();

        //------------------------------------------------------------
        // EMITTER.BeginIterator
        //
        /// <summary>
        /// Create some storage for tracking iterator locals
        /// </summary>
        /// <param name="iteratorLocalCount"></param>
        //------------------------------------------------------------
        internal void BeginIterator(int iteratorLocalCount)
        {
            DebugUtil.Assert(this.iteratorLocalsInfo == null);
            if (iteratorLocalCount == 0)
            {
                return;
            }

            //this.iteratorLocalsInfo = new (
            //    compiler().getLocalSymHeap().Alloc(CDIIteratorLocalsInfo::ComputeSize(iteratorLocalCount))
            //    ) CDIIteratorLocalsInfo(iteratorLocalCount);
            this.iteratorLocalsInfo = new CDIIteratorLocalsInfo(iteratorLocalCount);
        }

        //------------------------------------------------------------
        // EMITTER.ResetIterator
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ResetIterator()
        {
            if (this.iteratorLocalsInfo != null)
            {
                this.iteratorLocalsInfo.ClearOffsets();
            }
        }

        //------------------------------------------------------------
        // EMITTER.EndIterator
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void EndIterator()
        {
            // Don't worry about free-ing the memory,
            // that will get taken care of by the NRHEAP!
            this.iteratorLocalsInfo = null;
        }

        //------------------------------------------------------------
        // EMITTER.EmitForwardedIteratorDebugInfo
        //
        /// <summary></summary>
        /// <param name="fromMethodSym"></param>
        /// <param name="toMethodSym"></param>
        //------------------------------------------------------------
        internal void EmitForwardedIteratorDebugInfo(METHSYM fromMethodSym, METHSYM toMethodSym)
        {
            //throw new NotImplementedException("EMITTER.EmitForwardedIteratorDebugInfo");
        }
        //void EMITTER::EmitForwardedIteratorDebugInfo(METHSYM * methFrom, METHSYM * methTo)
        //{
        //    if (debugemit != NULL) {
        //        WCHAR szIterClassNameText[MAX_FULLNAME_SIZE];
        //        // Get the arity in the name
        //        if (!MetaDataHelper::GetMetaDataName(methTo->getClass(), szIterClassNameText, lengthof(szIterClassNameText)))
        //            return;
        //
        //        // Create the custom debug info structures
        //        DWORD size = (DWORD)CDIGlobalInfo::Size() + CDIForwardIteratorInfo::ComputeSize(szIterClassNameText);
        //        BYTE * buffer = STACK_ALLOC_ZERO(BYTE, size);
        //        new (buffer) CDIGlobalInfo(1);
        //        new (buffer + CDIGlobalInfo::Size()) CDIForwardIteratorInfo(szIterClassNameText);
        //
        //        // Save them (yes we have to open the method to set its attributes)
        //        CheckHRDbg(debugemit->OpenMethod(methFrom->tokenEmit));
        //        CheckHRDbg(debugemit->SetSymAttribute(methFrom->tokenEmit, MSCUSTOMDEBUGINFO, size, buffer));
        //        CheckHRDbg(debugemit->CloseMethod());
        //    }
        //}

        //protected:

        //protected void CheckHR(HRESULT hr);
        //protected void CheckHR(int errid, HRESULT hr);
        //protected void CheckHRDbg(HRESULT hr);
        //protected void MetadataFailure(HRESULT hr);
        //protected void DebugFailure(HRESULT hr);
        //protected void MetadataFailure(int errid, HRESULT hr);

        //------------------------------------------------------------
        // EMITTER.FieldFlagsFromAccess
        //
        /// <summary>
        /// Translate an access level value into flags.
        /// </summary>
        /// <param name="access"></param>
        /// <returns></returns>
        /// <remarks>
        /// (sscli) DWORD EMITTER::FlagsFromAccess(ACCESS access)
        /// </remarks>
        //------------------------------------------------------------
        protected FieldAttributes FieldFlagsFromAccess(ACCESS access)
        {
            //C_ASSERT((int)fdPublic == (int)mdPublic);
            //C_ASSERT((int)fdPrivate == (int)mdPrivate);
            //C_ASSERT((int)fdFamily == (int)mdFamily);
            //C_ASSERT((int)fdAssembly == (int)mdAssem);
            //C_ASSERT((int)fdFamORAssem == (int)mdFamORAssem);

            switch (access)
            {
                case ACCESS.PUBLIC:
                    return FieldAttributes.Public;

                case ACCESS.PROTECTED:
                    return FieldAttributes.Family;

                case ACCESS.PRIVATE:
                    return FieldAttributes.Private;

                case ACCESS.INTERNAL:
                    return FieldAttributes.Assembly;

                case ACCESS.INTERNALPROTECTED:
                    return FieldAttributes.FamORAssem;

                default:
                    DebugUtil.Assert(false);
                    return (FieldAttributes)0;
            }
        }

        //------------------------------------------------------------
        // EMITTER.
        //------------------------------------------------------------
        //protected PCOR_SIGNATURE BeginSignature();
        //protected PCOR_SIGNATURE EmitSignatureByte(PCOR_SIGNATURE curSig, BYTE b);
        //protected PCOR_SIGNATURE EmitSignatureUInt(PCOR_SIGNATURE curSig, ULONG b);
        //protected PCOR_SIGNATURE EmitSignatureToken(PCOR_SIGNATURE curSig, mdToken token);
        //protected PCOR_SIGNATURE EmitSignatureType(PCOR_SIGNATURE sig, PTYPESYM type);
        //protected PCOR_SIGNATURE EmitSignatureTypeVariables(PCOR_SIGNATURE sig, TypeArray * typeArgs);

        //------------------------------------------------------------
        // EMITTER.GetTypeSpec
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected Type GetTypeSpec(TYPESYM sym)
        {
            return SymUtil.GetSystemTypeFromSym(sym, null, null);
        }
        //protected mdToken GetTypeSpec(PTYPESYM sym);

        //protected PCOR_SIGNATURE GrowSignature(PCOR_SIGNATURE curSig);
        //protected PCOR_SIGNATURE EnsureSignatureSize(ULONG cb);
        //protected PCOR_SIGNATURE EndSignature(PCOR_SIGNATURE curSig, int * cbSig);
        //protected PCOR_SIGNATURE SignatureOfMembVar(PMEMBVARSYM sym, int * cbSig);
        //protected PCOR_SIGNATURE SignatureOfMethodOrProp(PMETHPROPSYM sym, int * cbSig);
        //protected mdSignature GetDebugVarSig(PTYPESYM type);
        //protected BYTE GetConstValue(PMEMBVARSYM sym, LPVOID tempBuff, LPVOID * ppValue, size_t * pcb);
        //protected bool VariantFromConstVal(AGGTYPESYM * type, CONSTVAL * cv, VARIANT * v);
        //protected mdToken GetScopeForTypeRef(AGGSYM *sym);
        //protected void EmitCustomDebugInfo(METHSYM * sym);
        //protected bool EmitDebugNamespace(NSAIDSYM * ns);
        //protected bool ComputeDebugNamespace(NSAIDSYM *ns, StringBldr & bldr);
        //protected void EmitForwardedDebugInfo(METHSYM * methFrom, METHSYM * methTo);
        //protected bool EmitDebugAlias(ALIASSYM * as);
        //protected void EmitExternAliasNames(METHSYM * methsym);
        //protected void EmitExternalAliasName(NAME * name, INFILESYM * infile);
        //protected void InitDocumentWriter(INFILESYM * infile);
        //protected void CheckExtent(SourceExtent & extent);

        //------------------------------------------------------------
        // EMITTER.GetMethodRefGivenParent
        //------------------------------------------------------------
        protected MethodInfo GetMethodRefGivenParent(METHSYM sym, Type parent)
        {
            throw new NotImplementedException("EMITTER.GetMethodRefGivenParent");
        }
        //mdToken EMITTER::GetMethodRefGivenParent(PMETHSYM sym, mdToken parent) 
        //{
        //    INFILESYM *inputfile;
        //    mdMemberRef memberRef;
        //    PCOR_SIGNATURE sig;
        //    int cbSig;
        //    const WCHAR * nameText;
        //    WCHAR nameBuffer[MAX_FULLNAME_SIZE];
        //
        //    // See if the class come from metadata or source code.
        //    inputfile = sym->getInputFile();
        //    if (inputfile->isSource || sym->isFAKEMETHSYM())
        //    {
        //        ASSERT(sym->getClass()->typeVarsAll->size 
        //            || (inputfile->getOutputFile() != compiler()->curFile->GetOutFile())
        //            || (sym->isFAKEMETHSYM()));  // If it's in our file, a def token should already 
        //                                                // have been assigned, unless it's in a generic class
        //
        //        if (sym->isFAKEMETHSYM() && sym->asFAKEMETHSYM()->parentMethSym)
        //        {
        //            METHSYM * meth = sym->asFAKEMETHSYM()->parentMethSym;
        //
        //            // Varargs is illegal in generics so throwing away the particular containing type
        //            // below (parent = newParent) doesn't hurt and is required by the CLI.
        //            ASSERT(!meth->typeVars->size && !meth->getClass()->typeVarsAll->size);
        //
        //            mdToken newParent = GetMethodRef(meth, meth->getClass()->getThisType(), NULL);
        //            if (TypeFromToken(newParent) == mdtMethodDef)
        //            {
        //                parent = newParent;
        //            }
        //        }
        //
        //        // Set "nameText" to the output name.
        //        if (sym->name == NULL)
        //        {
        //            // Explicit method implementations don't have a name in the language. Synthesize 
        //            // a name -- the name has "." characters in it so it can't possibly collide.
        //            MetaDataHelper::GetExplicitImplName(sym, nameBuffer, lengthof(nameBuffer));
        //            nameText = nameBuffer;
        //        }
        //        else
        //        {
        //            nameText = sym->name->text;
        //        }
        //
        //        // Symbol defined by source code. Define a member ref by name & signature.
        //        sig = SignatureOfMethodOrProp(sym, &cbSig);
        //
        //        CheckHR(metaemit->DefineMemberRef(
        //            parent,                         // ClassRef or ClassDef or TypeSpec importing a member.
        //            nameText,                       // member's name
        //            sig, cbSig,                     // point to a blob value of COM+ signature
        //            &memberRef));
        //    }
        //    else	// if (inputfile->isSource || sym->isFAKEMETHSYM())
        //    {
        //        // This symbol was imported from other metadata.
        //        const void *pHash = NULL;
        //        DWORD cbHash = 0;
        //
        //        if (inputfile->GetAssemblyID() != kaidThisAssembly)
        //            CheckHR(compiler()->linker->GetAssemblyRefHash(inputfile->mdImpFile, &pHash, &cbHash));
        //        CheckHR(metaemit->DefineImportMember(             
        //            inputfile->assemimport,             // [IN] Assembly containing the Member. 
        //            pHash, cbHash,                      // [IN] Assembly hash value
        //            sym->GetMetaImport(compiler()),     // [IN] Import scope, with member.  
        //            sym->tokenImport,                   // [IN] Member in import scope.   
        //            inputfile->assemimport ? metaassememit : NULL, // [IN] Assembly into which the Member is imported. (NULL if member isn't being imported from an assembly).
        //            parent,                             // [IN] Classref or classdef or TypeSpec in emit scope.    
        //            &memberRef));                       // [OUT] Put member ref here.   
        //    }	// if (inputfile->isSource || sym->isFAKEMETHSYM())
        //
        //    return memberRef;
        //}

        //------------------------------------------------------------
        // EMITTER.
        //------------------------------------------------------------
        //protected mdToken GetMembVarRefGivenParent(PMEMBVARSYM sym, mdToken parent);
        //protected void DumpIteratorLocals(METHSYM * meth);

        //protected void RecordEmitToken(mdToken * tokref);

        //------------------------------------------------------------
        // EMITTER.EraseEmitTokens
        //
        /// <summary>
        /// Go through all the remembers token addresses and erase them.
        /// </summary>
        //------------------------------------------------------------
        protected void EraseEmitTokens()
        {
            this.mdInfoList.Clear();
        }

        //------------------------------------------------------------
        // EMITTER::EraseEmitTokens (sscli)
        //------------------------------------------------------------
        //protected void EraseEmitTokens()
        //{
        //    TOKREFS * tokref;
        //
        //    // Go through all the token addresses and free them
        //    for (tokref = tokrefList; tokref != NULL; tokref = tokref->next)
        //    {
        //        // All the blocks are full except the first.
        //        int cAddr = (tokref == tokrefList) ? iTokrefCur : CTOKREF;
        //
        //        for (int i = 0; i < cAddr; ++i)
        //            * tokref->tokenAddrs[i] = 0;  // Erase each token.
        //    }
        //
        //    pmap = NULL;
        //
        //    // Free the list of token addresses.
        //    tokrefHeap.Free( &mark);
        //    tokrefList = NULL;
        //    iTokrefCur = CTOKREF;   // Signal that we must allocate a new block right away.
        //    cgttHead = NULL;
        //}

        //------------------------------------------------------------
        // EMITTER.RecordEmitToken
        //
        /// <summary>
        /// Remember that a metadata emission token is stored at this address.
        /// </summary>
        /// <remarks>
        /// protected void RecordEmitToken(mdToken * tokenAddr) // sscli
        /// </remarks>
        /// <param name="mdInfo"></param>
        //------------------------------------------------------------
        protected void RecordEmitToken(object mdInfo)
        {
            //if (iTokrefCur >= CTOKREF)
            //{
            //    // We need to allocate a new block of addresses.
            //    TOKREFS * tokrefNew;
            //
            //    tokrefNew = (TOKREFS *) tokrefHeap.Alloc(sizeof(TOKREFS));
            //    tokrefNew->next = tokrefList;
            //    tokrefList = tokrefNew;
            //    iTokrefCur = 0;
            //}

            // Simple case, just remember the address in the current block.
            //tokrefList->tokenAddrs[iTokrefCur++] = tokenAddr;
            this.mdInfoList.Add(mdInfo);
            return;
        }

        //protected void HandleAttributeError(HRESULT hr, BASENODE *parseTree, METHSYM *method);
        //protected void SaveSecurityAttribute(mdToken token, mdToken ctorToken, METHSYM * method, BYTE * buffer, unsigned bufferSize);

        //------------------------------------------------------------
        // EMITTER.FreeSavedSecurityAttributes
        //
        /// <summary>
        /// Terminate everything.
        /// </summary>
        //------------------------------------------------------------
        protected void FreeSavedSecurityAttributes()
        {
            if (this.savedSecurityAttributeList != null)
            {
                this.savedSecurityAttributeList.Clear();
            }
        }

        //------------------------------------------------------------
        // EMITTER::FreeSavedSecurityAttributes (sscli)
        //------------------------------------------------------------
        //void EMITTER::FreeSavedSecurityAttributes()
        //{
        //    SECATTRSAVE * pSecAttrSave;
        //
        //    // Free the saved attrbutes.
        //    pSecAttrSave = listSecAttrSave;
        //    while (pSecAttrSave) {
        //        SECATTRSAVE *pNext;
        //        pNext = pSecAttrSave->next;
        //        compiler()->globalHeap.Free(pSecAttrSave->buffer);
        //        compiler()->globalHeap.Free(pSecAttrSave);
        //        pSecAttrSave = pNext;
        //    }
        //
        //    cSecAttrSave = 0;
        //    listSecAttrSave = NULL;
        //}

        //------------------------------------------------------------
        // EMITTER.CreateGlobalType
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void CreateGlobalType()
        {
            if (this.globalTypeBuilder == null)
            {
                return;
            }

            try
            {
                this.globalTypeBuilder.CreateType();
            }
            catch (InvalidOperationException ex)
            {
                this.Error(ERRORKIND.ERROR, ex);
            }
            catch (NotSupportedException ex)
            {
                this.Error(ERRORKIND.ERROR, ex);
            }
        }

    }
}
