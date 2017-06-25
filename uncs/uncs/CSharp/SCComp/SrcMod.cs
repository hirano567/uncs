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
// File: srcmod.h
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
// File: srcmod.cpp
//
// ===========================================================================

//============================================================================
// SrcMod.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace Uncs
{
    //======================================================================
    // CSourceLexer
    //
    /// <summary>
    /// This is the derivation of CLexer that is used by source modules
    /// to do the main lexing of a source file.
    /// </summary>
    //======================================================================
    internal class CSourceLexer : CLexer
    {
        //------------------------------------------------------------
        // CSourceLexer Fields
        //------------------------------------------------------------
        private CSourceModule sourceModule = null;  // *m_pModule;

        //------------------------------------------------------------
        // CSourceLexer	Constructor
        //
        /// <summary></summary>
        /// <param name="module"></param>
        //------------------------------------------------------------
        internal CSourceLexer(CSourceModule module)
            : base(module.NameManager, module.OptionManager.LangVersion)
        {
            sourceModule = module;
        }

        //------------------------------------------------------------
        // CSourceLexer.TrackLine
        //
        /// <summary>
        /// Context-specific implementation...
        /// </summary>
        /// <param name="newLineIndex"></param>
        //------------------------------------------------------------
        override internal void TrackLine(int newLineIndex)
        {
            sourceModule.TrackLine(this, newLineIndex);
            base.TrackLine(newLineIndex);
        }

        //------------------------------------------------------------
        // CSourceLexer.ScanPreprocessorLine
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool ScanPreprocessorLine()
        {
            if (IsValidPreprocessorToken)
            {
                sourceModule.ScanPreprocessorLine(this, NotScannedLine);
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CSourceLexer.RecordCommentPosition
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="pos"></param>
        //------------------------------------------------------------
        internal void RecordCommentPosition(POSDATA pos)
        {
            throw new NotImplementedException("CSourceLexer.RecordCommentPosition");
        }

        //------------------------------------------------------------
        // CSourceLexer.RepresentNoiseTokens
        //
        /// <summary>
        /// Return true if TRACKCOMMENTS of CompilerCreationFlags is set.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool RepresentNoiseTokens()
        {
            return sourceModule.RepresentNoiseTokens();
        }

        //------------------------------------------------------------
        // CSourceLexer.ErrorPosArgs
        //
        /// <summary></summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="extent"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        override internal void ErrorPosArgs(
            int line,
            int col,
            int extent,
            CSCERRID id,
            params ErrArg[] args)
        {
            sourceModule.ErrorAtPositionArgs(line, col, extent, id, args);
        }
    }

    //======================================================================
    // CSourceParser
    //
    /// <summary>
    /// This is the derivation of CParser that is used by source modules for parsing.
    /// </summary>
    //======================================================================
    internal class CSourceParser : CParser
    {
        //------------------------------------------------------------
        // CSourceParser Fields and Properties
        //------------------------------------------------------------
        private CSourceModule sourceModule; // *m_pModule

        //------------------------------------------------------------
        // CSourceParser    Constructor
        //
        /// <summary></summary>
        /// <param name="mod"></param>
        //------------------------------------------------------------
        internal CSourceParser(CSourceModule mod)
            : base(mod.Controller.NameManager)
        {
            sourceModule = mod;
        }

        //------------------------------------------------------------
        // CSourceParser.ReportFeatureUse
        //
        /// <summary></summary>
        /// <param name="featureNo"></param>
        //------------------------------------------------------------
        virtual protected void ReportFeatureUse(ResNo featureNo)
        {
            if (sourceModule.OptionManager.IsLangVersionDefault)
            {
                return;
            }

            string featureName = null;
            Exception excp = null;

            switch (featureNo)
            {
                // These are all errors
                case ResNo.CSCSTR_FeatureGenerics:              // CSCSTRID.FeatureGenerics:
                case ResNo.CSCSTR_FeatureAnonDelegates:         // CSCSTRID.FeatureAnonDelegates:
                case ResNo.CSCSTR_FeatureGlobalNamespace:       // CSCSTRID.FeatureGlobalNamespace:
                case ResNo.CSCSTR_FeatureFixedBuffer:           // CSCSTRID.FeatureFixedBuffer:
                case ResNo.CSCSTR_FeatureStaticClasses:         // CSCSTRID.FeatureStaticClasses:
                case ResNo.CSCSTR_FeaturePartialTypes:          // CSCSTRID.FeaturePartialTypes:
                case ResNo.CSCSTR_FeaturePropertyAccessorMods:  // CSCSTRID.FeaturePropertyAccessorMods:
                case ResNo.CSCSTR_FeatureExternAlias:           // CSCSTRID.FeatureExternAlias:
                case ResNo.CSCSTR_FeatureIterators:             // CSCSTRID.FeatureIterators:
                case ResNo.CSCSTR_FeatureDefault:               // CSCSTRID.FeatureDefault:
                case ResNo.CSCSTR_FeatureNullable:              // CSCSTRID.FeatureNullable:
                    if (sourceModule.OptionManager.IsLangVersionECMA1)
                    {
                        //BSTR szFeatureName = null;
                        //PAL_TRY
                        //{
                        //    // This feature isn't allowed
                        //    szFeatureName = CError::ComputeString( GetMessageDll(), featureId, null);
                        //    Error(CSCERRID.ERR_NonECMAFeature, szFeatureName);
                        //}
                        //PAL_FINALLY
                        //{
                        //    SysFreeString (szFeatureName);
                        //}
                        //PAL_ENDTRY

                        if (CResources.GetString(featureNo, out featureName, out excp))
                        {
                            Error(CSCERRID.ERR_NonECMAFeature, new ErrArg(featureName));
                        }
                    }
                    break;

                // These are just warnings
                case ResNo.CSCSTR_FeatureModuleAttrLoc: // CSCSTRID.FeatureModuleAttrLoc:
                    if (sourceModule.OptionManager.IsLangVersionECMA1)
                    {
                        //BSTR szFeatureName = null;
                        //PAL_TRY
                        //{
                        //    // This feature isn't allowed
                        //    szFeatureName = CError::ComputeString( GetMessageDll(), featureId, null);
                        //    Error(CSCERRID.WRN_NonECMAFeature, szFeatureName);
                        //}
                        //PAL_FINALLY
                        //{
                        //    SysFreeString (szFeatureName);
                        //}
                        //PAL_ENDTRY

                        if (CResources.GetString(featureNo, out featureName, out excp))
                        {
                            Error(CSCERRID.WRN_NonECMAFeature, new ErrArg(featureName));
                        }
                    }
                    break;

                default:
                    DebugUtil.Assert(false, "Unknown Feature Id");
                    break;
            }

            return;
        }

        //------------------------------------------------------------
        // CSourceParser.SupportsErrorSuppression
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override protected bool SupportsErrorSuppression()
        {
            return true;
        }

        //------------------------------------------------------------
        // CSourceParser.GetErrorSuppression
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override protected CErrorSuppression GetErrorSuppression()
        {
            return new CErrorSuppression(sourceModule.Controller);
        }

        // Virtuals from CParser

        //------------------------------------------------------------
        // CSourceParser.CreateNewError
        //
        /// <summary></summary>
        /// <param name="errorId"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="errArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool CreateNewError(
            CSCERRID errorId,
            POSDATA startPos,
            POSDATA endPos,
            params ErrArg[] errArgs)
        {
#if DEBUG
            //if (IsWarningID(iErrorId))
            //{
            //    WORD num = ErrorNumberFromID(iErrorId);
            //    for (int i = 0; parserWarnNumbers[i] != 0; i++) {
            //        if (parserWarnNumbers[i] == num)
            //            goto FOUND;
            //    }
            //    VSASSERT(false,
            //    	"Parser warning not in allowed list",
            //    	"If you get this assert, you need to update parserWarnNumbers in srcmod.cpp"
            //    	"to include this warning number");
            //FOUND:;
            //}
#endif
            return sourceModule.CreateNewError(errorId, startPos, endPos, errArgs);
        }

        //------------------------------------------------------------
        // CSourceParser.AddToNodeTable
        //
        /// <summary></summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        override internal void AddToNodeTable(BASENODE node)
        {
            sourceModule.AddToNodeTable(node);
        }

        //------------------------------------------------------------
        // CSourceParser.GetInteriorTree
        //
        /// <summary></summary>
        /// <param name="pData"></param>
        /// <param name="pNode"></param>
        /// <param name="ppTree"></param>
        //------------------------------------------------------------
        override internal void GetInteriorTree(
            CSourceData pData,
            BASENODE pNode,
            ref CInteriorTree ppTree)
        {
            ppTree=sourceModule.GetInteriorParseTree(pData, pNode);
        }

        //------------------------------------------------------------
        // CSourceParser.GetLexData
        //
        /// <summary></summary>
        /// <param name="lexData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool GetLexData(LEXDATA lexData)
        {
            return this.sourceModule.UnsafeGetLexData(lexData);
        }

        // Overrides

        //------------------------------------------------------------
        // CSourceParser.ParseSourceModule
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal BASENODE ParseSourceModule()
        {
            return base.ParseSourceModule();
        }
    }

    //======================================================================
    // CModuleEventSource
    //
    /// <summary></summary>
    //======================================================================
    internal class CModuleEventSource
    {
        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireOnModuleModified() { }

        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireOnStateTransitionsChanged(int iFirst, int iLast) { }

        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireOnCommentTableChanged(int iStart, int iOldEnd, int iNewEnd) { }

        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireOnBeginTokenization(int iFirstLine) { }

        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireOnEndTokenization(CSourceData pData, int iFirst, int iLast, int iDelta) { }

        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireOnBeginParse(BASENODE pNode) { }

        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireOnEndParse(BASENODE pNode) { }

        /// <summary>
        /// Does nothing for now.
        /// </summary>
        internal void FireReportErrors(CErrorContainer pErrors) { }
    }

    //======================================================================
    // class THREADDATAREF
    //
    /// <summary></summary>
    //======================================================================
    internal class THREADDATAREF
    {
        internal System.Threading.Thread Thread = null; // DWORD dwTid;
        internal int RefCount = 0;                      // long iRef;
    }

    //======================================================================
    // SKIPFLAGS    (SB_) (SBF_)
    //
    /// <summary>
    /// <para>Flags directing behavior of, and indicating results of, SkipBlock()</para>
    /// <para>(CSharp\SCComp\SrcMod.cs)</para>
    /// </summary> 
    //======================================================================
    internal enum SKIPFLAGS : uint
    {
        // Behavior direction flags

        /// <summary>
        /// The block to be skipped is a getter/setter
        /// </summary>
        F_INACCESSOR = 0x00000001,

        // we're skipping over a block in a property.  So we may have something like:
        // internal int T {
        //      private s
        //
        // In this case, the private s will look like the start of a new member.  But 
        // it's actually the start of a setter.  So we want to detect this and not treat
        // it as the start of a new member
        F_INPROPERTY = 0x00000002,

        // Result flags

        /// <summary>
        /// Normal skip; no errors
        /// </summary>
        NORMALSKIP = 0,

        /// <summary>
        /// Returned by ScanMemberDeclaration in failure case
        /// </summary>
        NOTAMEMBER = 0,

        /// <summary>
        /// Detected a type member declaration (beginning at current token)
        /// </summary>
        TYPEMEMBER,

        /// <summary>
        /// Detected a namespace member declaration (at current token)
        /// </summary>
        NAMESPACEMEMBER,

        /// <summary>
        /// Detected an accessor (at current token)
        /// </summary>
        ACCESSOR,

        /// <summary>
        /// Hit end of file (error reported)
        /// </summary>
        ENDOFFILE,
    }

    //======================================================================
    // CCStateFlags  (CCF_)
    //
    /// <summary>
    /// <para>Conditional Compilation Flags
    /// indicating nuances of CC state changes and stack records</para>
    /// <para>(CSharp\SCComp\SrcMod.cs)</para>
    /// </summary>
    //======================================================================
    internal enum CCStateFlags : uint
    {
        NONE = 0x0000,

        /// <summary>
        /// <para>Record is for #region (as opposed to #if)</para>
        /// <para>0x001</para>
        /// </summary>
        REGION = 0x0001,

        /// <summary>
        /// <para>Indicates an #else block (implies !REGION)</para>
        /// <para>0x0002</para>
        /// </summary>
        ELSE = 0x0002,

        /// <summary>
        /// <para>State change only -- indicates entrance (push) as opposed to exit (pop)</para>
        /// <para>0x0004</para>
        /// </summary>
        ENTER = 0x0004,
    }

    //======================================================================
    // class CCREC
    //
    /// <summary>
    /// <para>conditional compilation stack record.
    /// This includes #region directives as well.</para>
    /// <para>This class is not used. Use Stack<CCStateFlags> for a stack of CC records.</para>
    /// </summary>
    //======================================================================
    internal class CCREC
    {
        internal CCStateFlags Flags = 0;    // DWORD dwFlags;   // CCF_* flags
        internal CCREC PreviousRec= null;   // CCREC *pPrev;    // Previous state record
    }

    //======================================================================
    // CCSTATE
    //
    /// <summary>
    /// <para>Conditional compilation state change record</para>
    /// <para>Has flags and line number.</para>
    /// <para>(CSharp\SCComp\SrcMod.cs)</para>
    /// </summary>
    //======================================================================
    internal class CCSTATE
    {
        //------------------------------------------------------------
        // CCSTATE Fields and Properties
        //------------------------------------------------------------
        /// <summary>
        /// CCF_* flags
        /// </summary>
        internal CCStateFlags Flags = 0;    // unsigned long dwFlags:3;

        /// <summary>
        /// Line number of CC directive causing change
        /// </summary>
        internal int LineIndex = -1;        // unsigned long iLine:29;

        //------------------------------------------------------------
        // CCSTATE Constructors (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CCSTATE()
        {
        }

        //------------------------------------------------------------
        // CCSTATE Constructors (2)
        //
        /// <summary></summary>
        /// <param name="f"></param>
        //------------------------------------------------------------
        internal CCSTATE(CCStateFlags f)
        {
            Flags = f;
        }

        //------------------------------------------------------------
        // CCSTATE Constructors (3)
        //
        /// <summary></summary>
        /// <param name="f"></param>
        /// <param name="i"></param>
        //------------------------------------------------------------
        internal CCSTATE(CCStateFlags f, int i)
        {
            Flags = f;
            LineIndex = i;
        }

        //------------------------------------------------------------
        // CCSTATE.Clone
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CCSTATE Clone()
        {
            return new CCSTATE(this.Flags, this.LineIndex);
        }
    }

    //======================================================================
    // class NODECHAIN
    //
    /// <summary></summary>
    //======================================================================
    internal class NODECHAIN
    {
        /// <summary>
        /// Index of this node in global-ordinal array
        /// </summary>
        internal int GlobalOrdinalIndex = -1;   // long iGlobalOrdinal;

        /// <summary>
        /// Next node with the same key (key-ordinal + 1)
        /// </summary>
        internal NODECHAIN Next = null;         // NODECHAIN *pNext;
    }

    //======================================================================
    // class CNodeDictionary
    //
    /// <summary>
    /// Derives from CTable&lt;NODECHAIN&gt;.
    /// </summary>
    //======================================================================
    internal class CNodeDictionary: CDictionary<NODECHAIN> { }

    //======================================================================
    // LEXDATABLOCK
    //
    /// <summary>
    /// <para>NOTE:
    /// This structure is used by the source module to represent all data calculated and/or generated by lexing.
    /// It derives from LEXDATA, which is the exposed version, and also includes some extra goo.</para>
    /// <para>Defives from LEXDATA, with conditional compilation list and comment table.</para>
    /// </summary>
    //======================================================================
    internal class LEXDATABLOCK : LEXDATA
    {
        // Source text
        //internal int iTextLen;  // Use LEXDATA.SourceTextLength

        // Token stream
        //internal int iAllocTokens;  // Use LEXDATA.TokenCount

        // Line offset table
        //internal int iAllocLines;   // Use LEXDATA.LineCount

        /// <summary>
        /// Comment table
        /// </summary>
        internal int CommentCount;  // long iAllocComments;

        /// <summary>
        /// CC states
        /// </summary>
        private List<CCSTATE> ccStateList = new List<CCSTATE>();    // CCSTATE *pCCStates;

        /// <summary>
        /// (R) Conditional Compilation states (CC states)
        /// </summary>
        internal List<CCSTATE> CCStateList
        {
            get { return ccStateList; }
        }

        /// <summary>
        /// (R) Count of CC states.
        /// </summary>
        internal int CCStateCount   // long iCCStates;
        {
            get
            {
                DebugUtil.Assert(ccStateList != null);
                return ccStateList.Count;
            }
        }

        //------------------------------------------------------------
        // LEXDATABLOCK.CloneTo (1)
        //
        /// <summary></summary>
        /// <param name="controller"></param>
        /// <param name="cloneSourceText"></param>
        /// <param name="lexClone"></param>
        //------------------------------------------------------------
        internal void CloneTo(
            CController controller,
            string cloneSourceText,
            LEXDATABLOCK lexClone)
        {
            // pTokens
            CSTOKEN[] tokenArray= null;
            CloneTokenStream(0, this.TokenCount, out tokenArray);
            //lexClone.TokenList = tokenstream.TokenList;
            lexClone.InitTokens(tokenArray);
            //lexClone.iAllocTokens = iAllocTokens;

            //lexClone.SourceText = cloneSourceText;
            lexClone.InitSource(cloneSourceText);

            // Line table data
            lexClone.InitLineOffsets(new List<int>(this.LineList));

            // Conditional compilation data
            //lexClone.TransitionLineList = new List<int>(this.TransitionLineList);
            lexClone.InitTransition(this.TransitionLineList);

            // Identifier table
            lexClone.IdentDictionary.Clear();
            lexClone.IdentDictionary.CopyContentsFrom(this.IdentDictionary);

            // Region table data
            // Array of line numbers indicating #endregion directives
            lexClone.InitRegion(
                new List<int>(this.RegionStartList),
                new List<int>(this.RegionEndList)
                );

            // #pragma warning map
            // Well, don't care about warnings for now...

            // DONE with LEXDATA members... Take care of LEXDATABLOCK ones
            //lexClone.iTextLen = iTextLen;
            //lexClone.iAllocTokens = this.TokenCount;
            //lexClone.iAllocLines = this.LineCount;
            lexClone.CommentCount = this.CommentCount;

            // ccstates
            lexClone.ccStateList.Clear();
            for (int i = 0; i < this.CCStateCount; ++i)
            {
                lexClone.ccStateList.Add(this.ccStateList[i].Clone());
            }
        }

        //------------------------------------------------------------
        // LEXDATABLOCK.CloneTo (2)
        //
        /// <summary></summary>
        /// <param name="lexClone"></param>
        //------------------------------------------------------------
        internal void CloneTo(LEXDATABLOCK lexClone)
        {
            base.CloneTo(lexClone);

            //lexClone.iTextLen = iTextLen;
            //lexClone.iAllocTokens = this.TokenCount;
            //lexClone.iAllocLines = this.LineCount;
            lexClone.CommentCount = this.CommentCount;

            // ccstates
            lexClone.ccStateList.Clear();
            for (int i = 0; i < this.CCStateCount; ++i)
            {
                lexClone.ccStateList.Add(this.ccStateList[i].Clone());
            }
        }
    }

    //======================================================================
    // CChecksumData
    //
    /// <summary>
    /// <para>(CSharp\SCComp\SrcMod.cs)</para>
    /// </summary>
    //======================================================================
    internal class CChecksumData
    {
        internal string Filename = null;    // NAME * pFilename;

        internal Guid GuidChecksumID;   // GUID guidChecksumID;

        internal List<byte> ChecksumData = null;    // BYTE * pbChecksumData;

        internal int ChecksumDataCount  // int cbChecksumData;
        {
            get { return (ChecksumData != null ? ChecksumData.Count : 0); }
        }

        internal ERRLOC ErrlocChecksum = null;  // ERRLOC * pErrlocChecksum;
    }

    //======================================================================
    // class CSourceModuleBase
    //
    /// <summary>
    /// <para>Base class for a source module as seen by a CSourceData object.
    /// This allows a CSourceData object to be cloned to an immutable source module,
    /// allowing a non-locking ICSSourceData object to be created.</para>
    /// <para>This class has the members below,
    /// <list type="bullet">
    /// <item>ICSSourceText</item>
    /// <item>LEXDATABLOCK -- has token data</item>
    /// <item>CLineMap of #line directive</item>
    /// <item>NAMETABLE for preprocessor</item>
    /// <item>Parse tree</item>
    /// <item>Configuration of compilation</item>
    /// </list>
    /// </para>
    /// </summary>
    //======================================================================
    abstract internal class CSourceModuleBase
    {
        //------------------------------------------------------------
        // CSourceModuleBase    Fields and Properties (1)
        //
        // Controller and Parser
        //------------------------------------------------------------

        /// <summary>
        /// Owning controller
        /// </summary>
        protected CController controller;   // CController * m_pController;

        /// <summary>
        /// (R) Owning controller
        /// </summary>
        internal CController Controller
        {
            get { return controller; }
        }

        /// <summary>
        /// (R) Controller.NameManager
        /// </summary>
        internal CNameManager NameManager
        {
            get // NAMEMGR *GetNameMgr();
            {
                DebugUtil.Assert(Controller != null);
                return Controller.NameManager;
            }
        }

        COptionManager optionManager = null;

        internal COptionManager OptionManager
        {
            get { return this.optionManager; }
        }

        /// <summary>
        /// This would be our parser...
        /// </summary>
        protected CParser parser = null;    // CParser * m_pParser;

        /// <summary>
        /// (R) This would be our parser...
        /// </summary>
        internal CParser Parser
        {
            get { return parser; }
        }

        /// <summary>
        /// Lock for state data
        /// </summary>
        protected object stateLockObject = new object();    // CTinyLock m_StateLock;

        /// <summary>
        /// <para>(R) Access to our state lock for external serialization</para>
        /// <para>Return lockObject.</para>
        /// </summary>
        internal object StateLock
        {
            get { return stateLockObject; }  // CTinyLock *GetStateLock()
        }

        //------------------------------------------------------------
        // CSourceModuleBase    Fields and Properties (2)
        //
        // Source text and edit data
        //------------------------------------------------------------

        /// <summary>
        /// Source text object
        /// </summary>
        internal CSourceText SourceText;  // CComPtr<ICSSourceText> m_spSourceText;

        internal FileInfo SourceFileInfo
        {
            get { return (this.SourceText != null ? this.SourceText.SourceFileInfo : null); }
        }

        //------------------------------------------------------------
        // CSourceModuleBase    Fields and Properties (3)
        //
        // Lexer data
        //------------------------------------------------------------

        /// <summary>
        /// Lexer data block
        /// </summary>
        protected LEXDATABLOCK lexDataBlock = new LEXDATABLOCK();   // LEXDATABLOCK m_lex;

        /// <summary>
        /// (R) Lexer data block
        /// </summary>
        internal LEXDATABLOCK LexDataBlock
        {
            get { return lexDataBlock; }
        }

        /// <summary>
        /// (R) Lexer data block, which derives from LEXDATA.
        /// </summary>
        virtual internal LEXDATA LexData
        {
            get { return LexDataBlock; }
        }

        /// <summary>
        /// Map of source lines to #lines
        /// </summary>
        protected CLineMap lineMap = new CLineMap();    // CLineMap m_LineMap;

        /// <summary>
        /// (R) Map of source lines to #lines
        /// </summary>
        internal CLineMap LineMap
        {
            get { return lineMap; }
        }

        /// <summary>
        /// Table of defined preprocessor symbols
        /// </summary>
        protected NAMETABLE preprocessorSymTable = new NAMETABLE(); // NAMETAB m_tableDefines;

        /// <summary>
        /// (R) Table of defined preprocessor symbols
        /// </summary>
        internal NAMETABLE PreprocessorSymTable
        {
            get { return preprocessorSymTable; }
        }

        //------------------------------------------------------------
        // CSourceModuleBase    Fields and Properties (4)
        //
        // Data accumulated during parse
        //------------------------------------------------------------

        /// <summary>
        /// Top of parse tree
        /// </summary>
        protected BASENODE treeTopNode = null;  // BASENODE *m_pTreeTop;

        /// <summary>
        /// (R) Top of parse tree
        /// </summary>
        internal BASENODE TreeTopNode
        {
            get { return treeTopNode; }
        }

        /// <summary>
        /// Table of key-indexable nodes
        /// </summary>
        protected CNodeDictionary nodeDictionary = null;    // CNodeTable * m_pNodeTable;

        /// <summary>
        /// (R) Table of key-indexable nodes
        /// </summary>
        internal CNodeDictionary NodeDictionary
        {
            get { return nodeDictionary; }
        }

        /// <summary>
        /// Array of key-indexable nodes
        /// </summary>
        protected List<KEYEDNODE> nodeArray = null; // CStructArray<KEYEDNODE> * m_pNodeArray;

        /// <summary>
        /// (R) Array of key-indexable nodes
        /// </summary>
        internal List<KEYEDNODE> NodeArray
        {
            get { return nodeArray; }
        }

        internal bool areOptionsFetched;    // bool m_fFetchedOptions;

        //------------------------------------------------------------
        // CSourceModuleBase.InternalFlush
        //
        /// <summary>
        /// Clear LexDataBlock, LineMap, NodeTable, NodeArray.
        /// </summary>
        //------------------------------------------------------------
        virtual internal void InternalFlush()
        {
            LexDataBlock.ClearTokens();
            LexDataBlock.InitLineOffsets(new List<int>());
            LexDataBlock.InitTransition(new List<int>());
            LexDataBlock.InitRegion(new List<int>(), new List<int>());

            LineMap.Clear();

            if (NodeDictionary != null)
            {
                NodeDictionary.Clear();
            }
            if (NodeArray != null)
            {
                NodeArray.Clear();
            }
        }

        //------------------------------------------------------------
        // CSourceModuleBase.GetInteriorNodeAddress
        //
        /// <summary></summary>
        /// <remarks>(sscli)
        /// internal CInteriorNode **GetInteriorNodeAddress(BASENODE pNode)
        /// </remarks>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CInteriorNode GetInteriorNodeAddress(BASENODE node)
        {
            DebugUtil.Assert(node.InGroup(NODEGROUP.INTERIOR));
        
            // Determine the location of the interior node pointer in the given
            // container node.

            switch (node.Kind)
            {
                case NODEKIND.METHOD:
                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                case NODEKIND.OPERATOR:
                    return node.AsANYMETHOD.InteriorNode;

                case NODEKIND.ACCESSOR:
                    return (node as ACCESSORNODE).InteriorNode;

                default:
                    return null;
            }
        }

        //------------------------------------------------------------
        // CSourceModuleBase.ParseInteriorsForErrors
        //------------------------------------------------------------
        //internal uint ParseInteriorsForErrors(CSourceData pData, BASENODE pNode);	//	HRESULT を返す
        //// CSourceModuleBase::ParseInteriorsForErrors
        //
        //HRESULT CSourceModuleBase::ParseInteriorsForErrors (CSourceData *pData, BASENODE *pNode)
        //{
        //    if (pNode == NULL)
        //        return S_OK;
        //
        //    switch (pNode->kind)
        //    {
        //        case NK_NAMESPACE:
        //            return ParseInteriorsForErrors (pData, pNode->asNAMESPACE()->pElements);
        //
        //        case NK_LIST:
        //        {
        //            CListIterator list(pNode->asLIST());
        //            BASENODE *pNodeCur;
        //            while (NULL != (pNodeCur = list.Next()))
        //            {
        //                HRESULT     hr;
        //                if (FAILED (hr = ParseInteriorsForErrors (pData, pNodeCur)))
        //                    return hr;
        //            }
        //            return S_OK;
        //        }
        //
        //        case NK_CLASS:
        //        case NK_STRUCT:
        //        {
        //            HRESULT     hr;
        //
        //            for (MEMBERNODE *p = pNode->AsAGGREGATE->pMembers; p != NULL; p = p->pNext)
        //            {
        //                if (p->kind == NK_NESTEDTYPE)
        //                {
        //                    if (FAILED (hr = ParseInteriorsForErrors (pData, p->asNESTEDTYPE()->pType)))
        //                        return hr;
        //                    continue;
        //                }
        //
        //                if (p->kind != NK_CTOR && p->kind != NK_METHOD &&
        //                    p->kind != NK_PROPERTY && p->kind != NK_OPERATOR && p->kind != NK_INDEXER &&
        //                    p->kind != NK_DTOR)
        //                    continue;
        //
        //                ICSInteriorTree *pTree;
        //
        //                if (p->kind == NK_PROPERTY || p->kind == NK_INDEXER)
        //                {
        //                    // Before getting an interior parse tree, check each node to see if it
        //                    // has already been parsed for errors (NF_INTERIOR_PARSED)
        //                    if (p->AsANYPROPERTY->pGet != NULL && (p->AsANYPROPERTY->pGet->other & NFEX_INTERIOR_PARSED) == 0)
        //                    {
        //                        if (FAILED (hr = pData->GetInteriorParseTree (p->AsANYPROPERTY->pGet, &pTree)))
        //                            return hr;
        //                        pTree->Release();
        //                    }
        //                    if (p->AsANYPROPERTY->pSet != NULL && (p->AsANYPROPERTY->pSet->other & NFEX_INTERIOR_PARSED) == 0)
        //                    {
        //                        if (FAILED (hr = pData->GetInteriorParseTree (p->AsANYPROPERTY->pSet, &pTree)))
        //                            return hr;
        //                        pTree->Release();
        //                    }
        //                }
        //                else if ((p->other & NFEX_INTERIOR_PARSED) == 0)
        //                {
        //                    if (FAILED (hr = pData->GetInteriorParseTree (p, &pTree)))
        //                        return hr;
        //                    pTree->Release();
        //                }
        //            }
        //            break;
        //        }
        //
        //        default:
        //            break;
        //    }
        //
        //    return S_OK;
        //}

        //------------------------------------------------------------
        // CSourceModuleBase.ParseInteriorNode
        //
        /// <summary>
        /// <para>Parse the method body and create node tree.
        /// Set the tree to argument node.</para>
        /// </summary>
        /// <param name="pNode"></param>
        //------------------------------------------------------------
        internal void ParseInteriorNode(BASENODE node)
        {
            //NRHEAP* activeHeap = GetActiveHeap();
            //NRHeapWriteMaker makeWriteable(activeHeap);
            //NRHeapWriteAllower allowWrite(activeHeap);

            switch (node.Kind)
            {
                case NODEKIND.METHOD:
                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                case NODEKIND.OPERATOR:
                    METHODBASENODE methodNode = node.AsANYMETHOD;

                    Parser.Rewind(methodNode.OpenIndex);
                    if (Parser.CurrentTokenID() == TOKENID.OPENCURLY)
                    {
                        BLOCKNODE temp = Parser.ParseBlock(methodNode, -1) as BLOCKNODE;
                        //WriteToggler toggler(ProtectedEntityFlags::ParseTree, methodNode.pBody);
                        methodNode.BodyNode = temp;
                    }
                    break;

                case NODEKIND.ACCESSOR:
                    ACCESSORNODE accessorNode = node as ACCESSORNODE;

                    Parser.Rewind(accessorNode.OpenCurlyIndex);
                    if (Parser.CurrentTokenID() == TOKENID.OPENCURLY)
                    {
                        BLOCKNODE temp = Parser.ParseBlock(accessorNode, -1) as BLOCKNODE;
                        //WriteToggler toggler(ProtectedEntityFlags::ParseTree, accessorNode.pBody);
                        accessorNode.BodyNode = temp;
                    }
                    break;

                default:
                    DebugUtil.Assert(false, "Unknown interior node");
                    break;
            }
        }

        //------------------------------------------------------------
        // CSourceModuleBase    Constructor
        //
        /// <summary>
        /// Set controller.
        /// </summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal CSourceModuleBase(CController cntr)
        {
            DebugUtil.Assert(cntr != null);
            this.controller= cntr;
            this.optionManager = cntr.OptionManager;
        }

        //------------------------------------------------------------
        // CSourceModuleBase.SetOptionManager
        //
        /// <summary></summary>
        /// <param name="opt"></param>
        //------------------------------------------------------------
        internal void SetOptionManager(COptionManager opt)
        {
            DebugUtil.Assert(opt != null);
            this.optionManager = opt;
        }

        //------------------------------------------------------------
        // CSourceModuleBase.CheckFlags
        //
        /// <summary>
        /// Determine if argument flags matches Controller.creationFlags
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckFlags(CompilerCreationFlags flags)
        {
            return Controller.CheckFlags(flags);
        }

        //
        // Virtual methods
        //

        //------------------------------------------------------------
        // CSourceModuleBase.MapLocation (1)
        //
        /// <summary>
        /// POSDATA の行番号に対して、マップ先のファイル名、
        /// hidden であるか、新しい行番号にマップされているかを返す。
        /// </summary>
        /// <remarks>
        /// Does NOT allocate memory for ppszFilename, it's just a pointer
        /// If no mapping changes the filename, this will use ICSSourceText->GetName()
        /// to supply the ppszFilename
        /// </remarks>
        /// <param name="pos"></param>
        /// <param name="fileName"></param>
        /// <param name="isHidden"></param>
        /// <param name="isMapped"></param>
        //------------------------------------------------------------
        virtual internal void MapLocation(
            POSDATA pos,
            out string fileName,
            out bool isHidden,
            out bool isMapped)
        {
            string name = null;
            LineMap.Map(pos.LineIndex, out name, out isHidden, out isMapped);
            if (this.SourceFileInfo != null)
            {
                fileName = IOUtil.SelectFileName(
                    this.SourceFileInfo,
                    this.OptionManager.FullPaths);
            }
            else
            {
                fileName = null;
            }
        }

        //------------------------------------------------------------
        // CSourceModuleBase.MapLocation (2)
        //
        /// <summary>
        /// 元の行番号に対して、新しい行番号、マップ先のファイル名、
        /// hidden であるか、新しい行番号にマップされているかを返す。
        /// </summary>
        /// <param name="line"></param>
        /// <param name="fileName"></param>
        /// <param name="isHidden"></param>
        /// <param name="isMapped"></param>
        //------------------------------------------------------------
        virtual internal void MapLocation(
            ref int line,
            out string fileName,
            out bool isHidden,
            out bool isMapped)
        {
            line = LineMap.Map(line, out fileName, out isHidden, out isMapped);
        }

        //------------------------------------------------------------
        // CSourceModuleBase.hasMap
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool hasMap()
        {
            return !LineMap.IsEmpty;
        }

        //------------------------------------------------------------
        // CSourceModuleBase.IsSymbolDefined
        //
        /// <summary>
        /// 指定されたシンボル名が preprocessorSymTable に登録されているか調べる。
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool IsSymbolDefined(string symbol)
        {
            return preprocessorSymTable.IsDefined(symbol);
        }

        //------------------------------------------------------------
        // CSourceModuleBase.
        //------------------------------------------------------------
        //virtual internal int GetDocCommentText(
        //    BASENODE pNode,
        //    ref string ppszText,
        //    ref int piFirstComment,
        //    ref int piLastComment,
        //    CXMLMap srcMap,
        //    ref int piLines,
        //    string pszIndent);	//	HRESULT を返す。

        //virtual internal int FreeDocCommentText(ref string ppszText);	//	HRESULT を返す。

        //virtual internal int FindDocCommentPattern(
        //    LEXDATA pLex,
        //    uint iLine,
        //    ref int piPatternCnt,
        //    ref string pszPattern);	//	HRESULT を返す。

        //virtual internal int MatchDocCommentPattern(
        //    string pszLine,
        //    string pszPattern,
        //    ref int piPatternCnt);	//	HRESULT を返す。

        //------------------------------------------------------------
        // CSourceModuleBase.GetInteriorParseTree
        //
        /// <summary>
        /// Create an interior tree object.
        /// It will call back into us to do the parse
        /// (if necessary; or grab the existing tree node if already there).
        /// </summary>
        /// <param name="src"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal CInteriorTree GetInteriorParseTree(
            CSourceData src,
            BASENODE node)
        {
            return CInteriorTree.CreateInstance(src, node);
        }

        //------------------------------------------------------------
        // CSourceModuleBase.
        //------------------------------------------------------------
        //virtual internal int LookupNode(
        //    CSourceData pData,
        //    ref string pKey,
        //    int iOrdinal,
        //    ref BASENODE ppNode,
        //    ref int piGlobalOrdinal);	//	HRESULT を返す。

        //virtual internal int GetNodeKeyOrdinal(
        //    CSourceData pData,
        //    BASENODE pNode,
        //    ref string ppKey,
        //    ref int piKeyOrdinal);	//	HRESULT を返す。

        //virtual internal int GetGlobalKeyArray(
        //    CSourceData pData,
        //    KEYEDNODE pKeyedNodes,
        //    int iSize,
        //    ref int piCopied);	//	HRESULT を返す。

        //virtual internal int GetSingleTokenData(
        //    CSourceData pData,
        //    int iToken,
        //    TOKENDATA pTokenData);	//	HRESULT を返す。

        //------------------------------------------------------------
        // CSourceModuleBase.GetSingleTokenPos
        //
        /// <summary>
        /// <para>Return the CToken at a given index as a POSDATA instance.</para>
        /// <para>CToken instances are stored in CSourceModueBase.LexDataBlock.</para>
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal POSDATA GetSingleTokenPos(int tokenIndex)
        {
            if (tokenIndex < 0 || tokenIndex >= this.lexDataBlock.TokenCount)
            {
                return null;
            }
            return this.lexDataBlock.TokenAt(tokenIndex);
        }

        //virtual internal int IsInsideComment(CSourceData pData, POSDATA pos, ref bool pfInComment);	//	HRESULT を返す。

        //------------------------------------------------------------
        // CSourceModuleBase.GetLexResults
        //
        /// <summary>
        /// Overwrite argument lexData with the data of this.lexDataBlock.
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="lexData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool GetLexResults(CSourceData sourceData, LEXDATA lexData)
        {
            this.lexDataBlock.CloneTo(lexData);
            return true;
        }

        //virtual internal int GetTokenText(
        //    int iTokenId,
        //    ref string ppszText,
        //    ref int piLength);	//	HRESULT を返す。

        //virtual internal int FindLeafNode(
        //    CSourceData pData,
        //    POSDATA pos,
        //    ref BASENODE ppNode,
        //    ref ICSInteriorTree ppTree);	//	HRESULT を返す。

        //virtual internal int FindLeafNodeEx(
        //    CSourceData pData,
        //    POSDATA pos,
        //    ExtentFlags flags,
        //    ref BASENODE ppNode,
        //    ref ICSInteriorTree ppTree);	//	HRESULT を返す。

        //virtual internal int FindLeafNodeForToken(
        //    CSourceData pData,
        //    int iToken,
        //    ref BASENODE ppNode,
        //    ref ICSInteriorTree ppTree);	//	HRESULT を返す。

        //virtual internal int FindLeafNodeForTokenEx(
        //    CSourceData pData,
        //    int iToken,
        //    ExtentFlags flags,
        //    ref BASENODE ppNode,
        //    ref ICSInteriorTree ppTree);	//	HRESULT を返す。

        //------------------------------------------------------------
        // CSourceModuleBase.GetDocComment
        //
        /// <summary>
        /// Under construction
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="comment"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool GetDocComment(BASENODE baseNode, out int comment, out int count)
        {
            // In order to have a doc comment, this node must be a type or member.  If it
            // isn't, look up the parent chain until it is.
            comment = 0;
            count = 0;

            while (!baseNode.InGroup(NODEGROUP.TYPE | NODEGROUP.FIELD | NODEGROUP.METHOD | NODEGROUP.PROPERTY)
                 && baseNode.Kind != NODEKIND.ENUMMBR
                 && baseNode.Kind != NODEKIND.VARDECL)
            {
                baseNode = baseNode.ParentNode;
                if (baseNode == null)
                {
                    return false;   // E_INVALIDARG;
                }
            }

            BASENODE attr = null;
            // Go from VARDECL to parent.
            if (baseNode.Kind == NODEKIND.VARDECL)
            {
                baseNode = (baseNode as VARDECLNODE).ParentNode;
                while (baseNode.Kind == NODEKIND.LIST)
                {
                    baseNode = baseNode.ParentNode;
                }
            }

            // Get the position of the first token of this thing
            // -- which should include its attribute if it has one
            if (baseNode.InGroup(NODEGROUP.AGGREGATE))
            {
                attr = baseNode.AsAGGREGATE.AttributesNode;
            }
            else if (baseNode.Kind == NODEKIND.DELEGATE)
            {
                attr = (baseNode as DELEGATENODE).AttributesNode;
            }
            else if (
                baseNode.InGroup(NODEGROUP.FIELD | NODEGROUP.METHOD | NODEGROUP.PROPERTY) ||
                baseNode.Kind == NODEKIND.ENUMMBR)
            {
                attr = baseNode.AsANYMEMBER.AttributesNode;
            }

            throw new NotImplementedException("CSourceModuleBase.GetDocComment");

            //int iFirstToken, iLastToken;
            //bool fMissing, fFound = false; ;

            // Find the first token of the node
            //    iFirstToken = m_pParser.GetFirstToken ((attr != NULL) ? attr : baseNode, EF_FULL, &fMissing);
            //
            //    // Starting from there, find the first doc comment amonst the noise tokens the precede it.
            //    for (iLastToken = iFirstToken--; 
            //        iFirstToken >= 0 && (CParser::GetTokenInfo (LexDataBlock.TokenAt(iFirstToken).Token()).dwFlags & TFF_NOISE); 
            //        iFirstToken--)
            //    {
            //        if (LexDataBlock.TokenAt(iFirstToken).Token() == TID_DOCCOMMENT || 
            //            LexDataBlock.TokenAt(iFirstToken).Token() == TID_MLDOCCOMMENT)
            //        {
            //            fFound = TRUE;
            //            break;
            //        }
            //    }
            //
            //    // If we found one, continue searching backwards until we find a non-doccomment token.
            //    if (!fFound)
            //        return E_FAIL;
            //
            //    long    iFirstComment = iFirstToken, iLastComment;
            //
            //    for (iLastComment = iFirstComment--; iFirstComment >= 0; iFirstComment--)
            //    {
            //        if (LexDataBlock.TokenAt(iFirstComment).Token() != TID_DOCCOMMENT &&
            //            LexDataBlock.TokenAt(iFirstComment).Token() != TID_MLDOCCOMMENT)
            //        {
            //            break;
            //        }
            //    }
            //    iFirstComment++;
            ///*
            //    UNDONE: Everett code ensures there are no intervening non-comment lines
            //    Whidbey doesn't do that checking here anymore...  See CSourceModule::GetDocCommentText
            //*/
            //
            //    // Our doc comment runs from iFirstComment to iLastComment
            //    *comment = iFirstComment;
            //    *count = (iLastComment - iFirstComment + 1);
            //return true;    // S_OK;
        }

        //virtual internal int GetDocCommentXML(BASENODE pNode, ref string pbstrDoc);	//	HRESULT を返す。

        //------------------------------------------------------------
        // CSourceModuleBase.GetExtent
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool GetExtent(
            BASENODE node,
            POSDATA posStart,
            POSDATA posEnd,
            ExtentFlags flags)
        {
            //CBasenodeLookupCache cache = null;
            return Parser.GetExtent(
                (CBasenodeLookupCache)null,
                node,
                flags,
                posStart,
                posEnd);
        }

        //------------------------------------------------------------
        // CSourceModuleBase.GetTokenExtent
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        //------------------------------------------------------------
        virtual internal void GetTokenExtent(BASENODE node, out int first, out int last)
        {
            Parser.GetTokenExtent(node, out first, out last, ExtentFlags.FULL);
        }

        //------------------------------------------------------------
        // CSourceModuleBase.IsWarningDisabled
        //
        /// <summary>
        /// This checks a warning against any #pragma warning directives and disables/eats
        /// the warning if appropriate
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool IsWarningDisabled(CError error)
        {
            // Check to see if warning is disabled in source
            if (LexDataBlock.WarningMap != null &&
                !LexDataBlock.WarningMap.IsEmpty &&
                error.WasWarning &&
                error.LocationCount > 0)
            {
                // It's a warning that can be disabled  and it has at least 1 location
                // Check to see if that warning number is disabled at the first/primary location
                string fileName = null;
                POSDATA posStart, posEnd;

                if (error.GetUnmappedLocationAt(0, out fileName, out posStart, out posEnd) &&
                    fileName != null &&
                    !posStart.IsUninitialized &&
                    !posEnd.IsUninitialized)
                {
                    int errNo = CSCErrorInfo.Manager.GetErrorNumber(error.ErrorID);

                    if (errNo >= 0 &&
                        LexDataBlock.WarningMap.IsWarningDisabled(
                            errNo, posStart.LineIndex, posEnd.LineIndex))
                    {
                        // Don't report the warning because it's been disabled
                        return true;
                    }
                }
            }
            return false;
        }

        //virtual internal int GetChecksum(Checksum checksum);	//	HRESULT を返す。

        //virtual internal int MapSourceLine(
        //    CSourceData pData,
        //    int iLine,
        //    ref int piMappedLine,
        //    ref string ppszFilename,
        //    ref bool pbIsHidden);	//	HRESULT を返す。

        //------------------------------------------------------------
        // CSourceModuleBase.AddToNodeTable
        //
        /// <summary></summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        virtual internal void AddToNodeTable(BASENODE node)
        {
            if (NodeDictionary == null)
            {
                return;
            }

            lock (stateLockObject)
            {
                // Add the node to the array, so we know its global ordinal
                NODECHAIN nodeChain = new NODECHAIN();
                KEYEDNODE keyedNode = new KEYEDNODE();

                nodeChain.GlobalOrdinalIndex = NodeArray.Count;
                NodeArray.Add(keyedNode);

                nodeChain.Next = null;
                keyedNode.Node = node;

                // Build the key for this node and find/add its node chain
                // we want the <,,,>
                if (node.BuildKey(NameManager, true, true, out keyedNode.Key))
                {
                    NODECHAIN nc;
                    bool br = NodeDictionary.Find(keyedNode.Key, out nc);

                    // The new one must go on the END of the node chain.
                    if (br && nc != null)
                    {
                        // Find the end and add it
                        while (nc.Next != null)
                        {
                            nc = nc.Next;
                        }
                        nc.Next = nodeChain;
                    }
                    else
                    {
                        if (br)
                        {
                            NodeDictionary.Remove(keyedNode.Key);
                        }
                        // This is the first one (most common case by huge margin...)
                        NodeDictionary.Add(keyedNode.Key, nodeChain);
                    }
                }
            }
        }

        //
        // Abstract methods
        //

        //------------------------------------------------------------
        // CSourceModuleBase.GetSourceModule    (abstract property)
        //
        /// <summary>
        /// <para>(R) abstract. Return CSourceModule instance.</para>
        /// <para>In CSourceModule, return itself.</para>
        /// </summary>
        //------------------------------------------------------------
        abstract internal CSourceModule SourceModule { get; }

        //------------------------------------------------------------
        // CSourceModuleBase.Clone  (abstract)
        //------------------------------------------------------------
        abstract internal CSourceModuleBase Clone(CSourceData pData);

        //------------------------------------------------------------
        // CSourceModuleBase.GetInteriorNode    (abstract)
        //------------------------------------------------------------
        virtual internal CInteriorNode GetInteriorNode(CSourceData pData, BASENODE pNode)
        {
            throw new LogicError("CSourceModuleBase.GetInteriorNode is originally pure.");
        }

        //------------------------------------------------------------
        // CSourceModuleBase.GetErrors  (abstract)
        //------------------------------------------------------------
        abstract internal CErrorContainer GetErrors(CSourceData pData, ERRORCATEGORY iCategory);

        //------------------------------------------------------------
        // CSourceModuleBase.ParseTopLevel  (abstract)
        //------------------------------------------------------------
        abstract internal bool ParseTopLevel(
            CSourceData pData,
            out BASENODE ppNode,
            bool fCreateParseDiffs); // = false

        virtual internal int IsUpToDate(out bool pfTokenized, out bool pfTopParsed)
        {
            // 元は純粋仮想関数
            //pfTokenized = false;
            //pfTopParsed = false;
            //return 0;
            throw new NotImplementedException("CSourceModuleBase.IsUpToDate");
        }

        virtual internal int GetLastRenamedTokenIndex(out int piTokIdx, out string ppPreviousName)
        {
            // 元は純粋仮想関数
            //piTokIdx = 0;
            //ppPreviousName = null;
            //return 0;
            throw new NotImplementedException("CSourceModuleBase.GetLastRenamedTokenIndex");
        }

        virtual internal int ResetRenamedTokenData()
        {
            // 元は純粋仮想関数
            //return 0;
            throw new NotImplementedException("CSourceModuleBase.ResetRenamedTokenData");
        }
        virtual internal int ParseForErrors(CSourceData pData)
        {
            // 元は純粋仮想関数
            //return 0;
            throw new NotImplementedException("CSourceModuleBase.ParseForErrors");
        }

        //abstract internal NRHEAP GetActiveHeap();

        internal const int MAX_PPTOKEN_LEN = 128;

        static internal bool[] IsPreprocessorKeywordDirective =
        {
            false,	// UNDEFINED

            true,           
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            false,
            false,
            false,           
            false,
            false,           
            false,
            false,
            false,           
            false,
            false,           
            false,
            false,
            false,
            false,           
            false,
            false,
            false,
            false,
            false,
            false,
        };

        // Keep these 2 lists zero-terminated and up-to-date with any warnings reported
        // from the lexer or parser.  They are used to determine whether or not 
        // an incremental lex or parse error occured based on whether these warnings
        // have been enabled or disabled via #pragma warning directives
        // Also keep them sorted according to their error number

        static internal int[] lexerWarnNumbers =
        {
            1030,   // CSCERRID.WRN_WarningDirective
            1633,   // CSCERRID.WRN_IllegalPragma
            1634,   // CSCERRID.WRN_IllegalPPWarning
            1635,   // CSCERRID.WRN_BadRestoreNumber
            1645,   // CSCERRID.WRN_NonECMAFeature
            1691,   // CSCERRID.WRN_BadWarningNumber
            1692,   // CSCERRID.WRN_InvalidNumber
            1694,   // CSCERRID.WRN_FileNameTooLong
            1695,   // CSCERRID.WRN_IllegalPPChecksum
            1696,   // CSCERRID.WRN_EndOfPPLineExpected
            1709,   // CSCERRID.WRN_EmptyFileName
            0,
        };

        static internal int[] parserWarnNumbers =
        {
            78,     // CSCERRID.WRN_LowercaseEllSuffix
            642,    // CSCERRID.WRN_PossibleMistakenNullStatement
            1522,   // CSCERRID.WRN_EmptySwitch
            1645,   // CSCERRID.WRN_NonECMAFeature
            440,    // CSCERRID.WRN_GlobalAliasDefn
            657,    // CSCERRID.WRN_AttributeLocationOnBadDeclaration
            658,    // CSCERRID.WRN_InvalidAttributeLocation
            0,
        };
    }

    //	typedef CSmartPtr<CSourceModuleBase> CSourceModuleBasePtr;

    //======================================================================
    // class CSourceModule
    //
    /// <summary>
    /// <para>(sscli)
    /// This is the object that implements ICSSourceModule.
    /// It also holds all of the data
    /// to which callers can access ONLY through ICSSourceData objects.</para>
    /// <para>(sscli)
    /// Clearly, there is a LOT of state contained in a source module.
    /// Most of this has to do with the complexities involved in incremental lexing,
    /// compounded by free-threadedness.</para>
    /// <para>(sscli)
    /// When adding any new state/member variables,
    /// please try to conform to the organization of logical groupings,
    /// and be aware of multi-threaded access
    /// and whether or not your change affects incremental lexing.</para>
    /// </summary>
    /// <remarks>
    /// This class has friend classes, so the accessibiities of all members are internal.
    /// </remarks>
    //======================================================================
    internal class CSourceModule :
         CSourceModuleBase  //, ICSSourceTextEvents
    {
        //friend class CSourceLexer;
        //friend class CSourceParser;

        //------------------------------------------------------------
        // CSourceModule    Fields and Properties (1)
        //
        // Source text and edit data
        //------------------------------------------------------------

        /// <summary>
        /// CC data
        /// </summary>
        /// <remarks>
        /// (sscli) CCREC *m_pCCStack;
        /// </remarks>
        internal Stack<CCStateFlags> CompilationStateStack = new Stack<CCStateFlags>();

        //------------------------------------------------------------
        // CSourceModule    Fields and Properties (2)
        //
        // Data accumulated during parse
        //------------------------------------------------------------

        /// <summary>
        /// Event source
        /// </summary>
        internal CModuleEventSource moduleEventSource = new CModuleEventSource();   // m_EventSource

        /// <summary>
        /// Used ONLY to detect errors report during some time span...
        /// </summary>
        internal int ErrorCount = 0;    // m_iErrors

        //------------------------------------------------------------
        // CSourceModule    Fields and Properties (3)
        //
        // #line data
        //------------------------------------------------------------

        /// <summary>
        /// Directory of source file (used only for LineMap)
        /// </summary>
        internal string SourceDirectory = null; // PWSTR m_pszSrcDir;

        /// <summary>
        /// Array of #pragma checksum lines
        /// </summary>
        /// <remarks>
        /// (sscli) CStructArray<ChecksumData> m_rgChecksum;
        /// </remarks>
        internal List<CChecksumData> ChecksumList = new List<CChecksumData>();

        /// <summary>
        /// <para>Parser data</para>
        /// <para>Current token</para>
        /// </summary>
        internal int currentTokenId = 0;   // long m_iCurTok;

        //------------------------------------------------------------
        // CSourceModule    Fields and Properties (4)
        //
        // Error tracking
        //------------------------------------------------------------

        /// <summary>
        /// Container for EC_TOKENIZATION errors
        /// </summary>
        private CErrorContainer tokenErrorContainer = null; // *m_pTokenErrors

        /// <summary>
        /// (R) Container for EC_TOKENIZATION errors
        /// </summary>
        internal CErrorContainer TokenErrorContainer
        {
            get { return tokenErrorContainer; }
        }

        /// <summary>
        /// Container for EC_TOPLEVELPARSE errors
        /// </summary>
        private CErrorContainer parseErrorContainer = null; // *m_pParseErrors

        /// <summary>
        /// (R) Container for EC_TOPLEVELPARSE errors
        /// </summary>
        internal CErrorContainer ParseErrorContainer
        {
            get { return parseErrorContainer; }
        }

        /// <summary>
        /// Depending on mode, points to one of the above or an interior parse container
        /// </summary>
        private CErrorContainer currentErrorContainer = null;   // *m_pCurrentErrors;

        /// <summary>
        /// (R) Depending on mode, points to one of the above or an interior parse container
        /// </summary>
        internal CErrorContainer CurrentErrorContainer
        {
            get { return currentErrorContainer; }
        }

        //------------------------------------------------------------
        // CSourceModule    Fields and Properties (5)
        //
        // Bits/Flags/etc...
        // These bits are protected by LockObject
        //------------------------------------------------------------

        /// <summary>
        /// TRUE if a thread has committed to tokenizing current text
        /// </summary>
        internal bool CommitTokenizing = false;     // unsigned m_fTokenizing:1;

        /// <summary>
        /// TRUE if token stream for current text is available
        /// </summary>
        internal bool IsTokenized = false;          // unsigned m_fTokenized:1;

        /// <summary>
        /// TRUE if this is the first tokenization for this module (token allocs can come from NR heap)
        /// </summary>
        //internal bool IsFirstTokenization = true;   // unsigned m_fFirstTokenization:1;

        /// <summary>
        /// TRUE if a thread has committed to parsing the top level
        /// </summary>
        internal bool CommitParsingTop = false;     // unsigned m_fParsingTop:1;

        /// <summary>
        /// TRUE if the top level parse tree for the current text is available
        /// </summary>
        internal bool IsParseTreeTopAvailable = false;  // unsigned m_fParsedTop:1;

        /// <summary>
        /// TRUE if a thread is parsing an interior node
        /// </summary>
        internal bool IsParsingInterior = false;    // unsigned m_fParsingInterior:1; 

        /// <summary>
        /// This pad ensures that mods to the unprotected bits don't whack the protected ones
        /// </summary>
        internal bool IsBitPad = false; // unsigned m_bitpad:19;

        //------------------------------------------------------------
        // CSourceModule    Fields and Properties (6)
        //
        // Bits/Flags/etc...
        // These bits are NOT PROTECTED and must be kept on the "other side" of the pad
        //------------------------------------------------------------

        /// <summary>
        /// TRUE if line count/length limit exceeded on last tokenization
        /// </summary>
        internal bool LineLimitExceeded;    // unsigned m_fLimitExceeded:1;

        /// <summary>
        /// TRUE if an error has been reported on the current token
        /// </summary>
        internal bool ReportedErrorOnCurrentToken = false;  // unsigned m_fErrorOnCurTok:1;

        /// <summary>
        /// TRUE if a CC symbol was defined/undefined
        /// </summary>
        internal bool CCSymbolChanged;  // unsigned m_fCCSymbolChange:1;

        /// <summary>
        /// TRUE if in ParseForError call only
        /// </summary>
        internal bool InParsingForErrors = false;   // unsigned m_fParsingForErrors:1;

        /// <summary>
        /// TRUE if exception happend during parse/lex
        /// </summary>
        internal bool ParseCrashed = false; // unsigned m_fCrashed:1;

        /// <summary>
        /// TRUE if exception happened during FindLeafNode
        /// </summary>
        internal bool LeafCrashed = false;   // unsigned m_fLeafCrashed:1;

        /// <summary>
        /// TRUE if line tracking should occur (first time tokenization; creates line table)
        /// </summary>
        /// <remarks>
        /// Note: Removed from bitset to avoid concurrency issues with fields above
        /// </remarks>
        internal bool TrackingLines = false;    // unsigned m_fTrackLines;

        //------------------------------------------------------------
        // CSourceModule    Fields and Properties (10)
        //
        // Memory consumption tracking                                                  
        //------------------------------------------------------------

        /// <summary>
        /// Set this if size accumulation is desired (during parse for errors)
        /// </summary>
        internal bool NeedAccumulateSize = false;    // BOOL m_fAccumulateSize;

        /// <summary>
        /// Accumulation of interior parse tree space
        /// </summary>
        internal int InteriorSize = 0; // DWORD m_dwInteriorSize;

        /// <summary>
        /// Number of interior trees parsed (during parse for errors)
        /// </summary>
        internal int InteriorTreeCount = 0;  // long m_iInteriorTrees;

        // Token index to keep track of renaming events...

        /// <summary>
        /// Index of the last token that was renamed
        /// </summary>
        internal int LastRenamedTokenIndex = -1;    // long m_iLastTokenRenamed;

        internal string PreviousTokenName = null;   // NAME * m_pPreviousTokenName;

#if DEBUG
        // Debug-only data

        /// <summary>
        /// These three values are used for asserting thread behavior during parsing...
        /// </summary>
        internal System.Threading.Thread TokenizerThread = null;        // DWORD m_dwTokenizerThread;

        internal System.Threading.Thread ParserThread = null;           // DWORD m_dwParserThread;

        internal System.Threading.Thread InteriorParserThread = null;   // DWORD m_dwInteriorParserThread;

        //------------------------------------------------------------
        // CSourceModule.SetTokenizerThread
        //
        /// <summary>
        /// <para>In DEBUG mode, Sets the current thread to m_dwTokenizerThread.</para>
        /// <para>Otherwise, does nothing.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void SetTokenizerThread()
        {
            this.TokenizerThread = System.Threading.Thread.CurrentThread;
        }

        //------------------------------------------------------------
        // CSourceModule.ClearTokenizerThread
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearTokenizerThread()
        {
            this.TokenizerThread = null;
        }

        //------------------------------------------------------------
        // CSourceModule.SetParserThread
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void SetParserThread()
        {
            this.ParserThread = System.Threading.Thread.CurrentThread;
        }

        //------------------------------------------------------------
        // CSourceModule.ClearParserThread
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearParserThread()
        {
            this.ParserThread = null;
        }

        //------------------------------------------------------------
        // CSourceModule.SetInteriorParserThread
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void SetInteriorParserThread()
        {
            this.InteriorParserThread = System.Threading.Thread.CurrentThread;
        }

        //------------------------------------------------------------
        // CSourceModule.ClearInteriorParserThread
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearInteriorParserThread()
        {
            this.InteriorParserThread = null;
        }
#else
        //------------------------------------------------------------
        // CSourceModule.SetTokenizerThread
        //
        /// <summary>
        /// <para>In DEBUG mode, Sets the current thread to m_dwTokenizerThread.</para>
        /// <para>Otherwise, does nothing.</para>
        /// </summary>
        //------------------------------------------------------------
        public void SetTokenizerThread() { }

        public void ClearTokenizerThread() { }
        public void SetParserThread() { }
        public void ClearParserThread() { }
        public void SetInteriorParserThread() { }
        public void ClearInteriorParserThread() { }
#endif

        //------------------------------------------------------------
        // CSourceModule    Constructor
        //
        /// <summary></summary>
        /// <param name="controller"></param>
        //------------------------------------------------------------
        internal CSourceModule(CController controller)
            : base(controller)
        {
            parser = new CSourceParser(this);

            //    // Initialize the top level heap pointer and pointer to markers
            //    m_pActiveTopLevelHeap= &m_heap1;
            //    m_pMarkInterior = &m_markInterior1;
            //    m_pMarkTokens = &m_markTokens1;

            //OldEditEndPoint.LineIndex = 0;
            //OldEditEndPoint.CharIndex = 0;
        }

        //------------------------------------------------------------
        // CSourceModule.Initialize
        //
        /// <summary>
        /// <list type="number">
        /// <item><description>Set text to SourceText.</description></item>
        /// <item><description>Create nodeTable and nodeArray.</description></item>
        /// <item><description>Create ErrorContainers.</description></item>
        /// <item><description>Set the source text to LexDataBlock.</description></item>
        /// </list>
        /// </summary>
        /// <param name="text"></param>
        //------------------------------------------------------------
        internal void Initialize(CSourceText text)
        {
            lock (StateLock)
            {
                bool br = false;

                SourceText = text;
                if (CheckFlags(CompilerCreationFlags.KEEPNODETABLES))
                {
                    nodeDictionary = new CNodeDictionary();
                    nodeArray = new List<KEYEDNODE>();
                }
                if (CheckFlags(CompilerCreationFlags.KEEPIDENTTABLES))
                {
                    // May be left to the initial setting.
                }

                // Create CErrorContainer instances and Set the source text to LexDataBlock.
                tokenErrorContainer = CErrorContainer.CreateInstance(ERRORCATEGORY.TOKENIZATION, 0);
                parseErrorContainer = CErrorContainer.CreateInstance(ERRORCATEGORY.TOPLEVELPARSE, 0);
                if (tokenErrorContainer != null && parseErrorContainer != null)
                {
                    br = GetTextAndConnect();
                }
            }
        }

        //------------------------------------------------------------
        // CSourceModule.CreateInstance
        //
        /// <summary>
        /// Create a CSourceModule instance and set arguments to it.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal CSourceModule CreateInstance(
            CController controller,
            CSourceText text)
        {
            CSourceModule srcMod = new CSourceModule(controller);
            srcMod.Initialize(text);
            return srcMod;
        }

        // Tokenization routines

        //int FindNearestPosition (POSDATA ppos, int iSize,  POSDATA pos);

        //------------------------------------------------------------
        // CSourceModule.UpdateRegionTable
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void UpdateRegionTable()
        {
            // We can have at most LexDataBlock.iCCStates regions (and that's only if they are
            // all nested and unterminated).  Allocate twice as much as necessary, and
            // point the end array at the midpoint (these never grow -- one less allocation).

            //long    iSize = 2 * LexDataBlock.iCCStates * sizeof (LONG);
            //LexDataBlock.piRegionStart =
            //  (long *)(LexDataBlock.piRegionStart == NULL
            //  ? m_pStdHeap->Alloc (iSize)
            //  : m_pStdHeap->Realloc (LexDataBlock.piRegionStart, iSize));
            //LexDataBlock.piRegionEnd = LexDataBlock.piRegionStart + LexDataBlock.iCCStates;

            // We also need a little stack space
            //long    *piStack = STACK_ALLOC (long, LexDataBlock.iCCStates);
            //long    iStack = 0;

            Stack<int> tempStack = new Stack<int>();

            // Rip through the CC states and look for CCF_REGION enters and corresponding exits
            LexDataBlock.ClearRegions();

            for (int i = 0; i < LexDataBlock.CCStateCount; i++)
            {
                CCStateFlags flags = LexDataBlock.CCStateList[i].Flags;

                if ((flags & CCStateFlags.REGION) != 0)
                {
                    // Here's one we care about.
                    if ((flags & CCStateFlags.ENTER) != 0)
                    {
                        // This is an enter.  Store the line number in the start array
                        // at the current region slot, and push that region slot on the
                        // stack for the next exit.
                        LexDataBlock.RegionStartList.Add(LexDataBlock.CCStateList[i].LineIndex);
                        LexDataBlock.RegionEndList.Add(-1);
                        //StoreAtIndex(piStack, iStack++, LexDataBlock.iCCStates,
                        //  LexDataBlock.iRegions++);
                        tempStack.Push(LexDataBlock.RegionCount - 1);
                    }
                    else
                    {
                        // This is an exit, so pop a slot off the stack and store the
                        // line in the end array at that slot.
                        //ASSERT (iStack > 0);
                        //int itmp = (int)FetchAtIndex(piStack, --iStack, LexDataBlock.CCStateCount);
                        int itmp = tempStack.Pop();
                        LexDataBlock.RegionEndList[itmp] = LexDataBlock.CCStateList[i].LineIndex;
                    }
                }
            }

            // For anything left on the stack, use the line count as the 'end'
            while (tempStack.Count > 0)
            {
                //int itmp=(int)FetchAtIndex(piStack, --iStack, LexDataBlock.iCCStates);
                int itmp = tempStack.Pop();
                LexDataBlock.RegionEndList[itmp] =
                    LexDataBlock.TokenArray[LexDataBlock.TokenCount - 1].LineIndex;
            }
        }

        //static long HandleExceptionFromTokenization(ref EXCEPTION_POINTERS exceptionInfo, Object pv);

        //------------------------------------------------------------
        // CSourceModule.InternalCreateTokenStream
        //
        /// <summary>
        /// <para>Get each token from source text and add it to LexDataBlock.TokenList.</para>
        /// </summary>
        /// <param name="sourceData"></param>
        //------------------------------------------------------------
        internal void InternalCreateTokenStream(CSourceData sourceData)
        {
            if (IsTokenized || CommitTokenizing)
            {
                return;
            }

            CErrorContainer tempEContainer = null;

            // Lock the state bits.  Once we determine that we actually need to do work,
            // we'll set appropriate state flags an unlock the state bits so other threads
            // can make the same determination.

            lock (this.StateLock)
            {
                // If we're currently tokenized, we stop here.
                if (this.IsTokenized)
                {
                    return;
                }

                // Okay, at this point we commit to tokenizing the current text.
                // Reflect that in the state bits, and record our thread ID as the "tokenizer thread".
                // Also start the timer for perf analysis.
                SetTokenizerThread();
                this.CommitTokenizing = true;

                InternalFlush();
                if (LexDataBlock.IdentDictionary != null)
                {
                    LexDataBlock.IdentDictionary.Clear();
                }
                PreviousTokenName = null;

                tempEContainer = TokenErrorContainer.Clone();
                currentErrorContainer = tempEContainer;
            } // lock (this.StateLock)

            LexDataBlock.TransitionLineList.Clear();
            TrackingLines = true;

            CSourceLexer sourceLexer = new CSourceLexer(this);

            sourceLexer.CurrentLineStartIndex = 0;  // LexDataBlock.pszSource;
            //sourceLexer.m_pszCurrent = LexDataBlock.pszSource;
            sourceLexer.TextReader = new CTextReader(this.SourceText.Text, 0);
            sourceLexer.CurrentLineIndex = 0;

            if (LexDataBlock.LineCount == 0)
            {
                LexDataBlock.LineList.Add(0);
            }
            else
            {
                LexDataBlock.LineList[0] = 0;
            }
            sourceLexer.IsValidPreprocessorToken = true;
            sourceLexer.NotScannedLine = true;

            moduleEventSource.FireOnBeginTokenization(sourceLexer.CurrentLineIndex);

            //----------------------------------------------------
            // Get all tokens from source text and store it in LexDataBlock.
            //----------------------------------------------------
            List<CSTOKEN> tempTokenList = new List<CSTOKEN>();

            while (true)
            {
                CSTOKEN currentToken = new CSTOKEN();
                TOKENID currentTokenID = sourceLexer.ScanToken(currentToken);

                //int currentTokenIndex = LexDataBlock.TokenCount;
                //LexDataBlock.TokenList.Add(currentToken);
                int currentTokenIndex = tempTokenList.Count;
                tempTokenList.Add(currentToken);

                if (currentTokenID == TOKENID.IDENTIFIER && LexDataBlock.IdentDictionary != null)
                {
                    LexDataBlock.IdentDictionary.Add(currentToken.Name, currentToken.Name);
                }

                if (currentTokenID == TOKENID.ENDFILE)
                {
                    break;
                }
                if (sourceLexer.IsLineLimitExceeded)
                {
                    break;
                }
            }   // while(true)


            LexDataBlock.InitTokens(tempTokenList);

#if DEBUG
            StringBuilder debugSb = new StringBuilder();
            sourceData.LexData.DebugTokenList(debugSb);
#endif
            // If tokenized newly, fields are in initial states except LexDataBlock.TokenList,
            // which stored CTOKEN instance.

            TrackingLines = false;

            //copyStack.Clear();

            if (CompilationStateStack.Count != 0)
            {
                ErrorAtPosition(
                    LexDataBlock.TokenArray[LexDataBlock.TokenCount - 1].LineIndex,
                    LexDataBlock.TokenArray[LexDataBlock.TokenCount - 1].CharIndex,
                    1,
                    (this.CompilationStateStack.Peek() & CCStateFlags.REGION) != 0
                        ? CSCERRID.ERR_EndRegionDirectiveExpected
                        : CSCERRID.ERR_EndifDirectiveExpected);
                while (PopCCRecord() != 0) ;
            }

            if (sourceLexer.IsLineLimitExceeded)
            {
                //DBGOUT (("COMPILER LIMIT EXCEEDED!!!  Line too long, or too many lines..."));
                LineLimitExceeded = true;

                CSTOKEN[] tokArray = { new CSTOKEN() };
                tokArray[0].TokenID = TOKENID.ENDFILE;
                LexDataBlock.InitTokens(tokArray);

                LexDataBlock.TransitionLineList.Clear();
                LexDataBlock.RegionStartList.Clear();
                LexDataBlock.RegionEndList.Clear();
                LexDataBlock.CCStateList.Clear();
            }

            UpdateRegionTable();

            lock (this.StateLock)
            {
                IsTokenized = true;
                CommitTokenizing = false;
                ClearTokenizerThread();

                currentErrorContainer = null;
                tempEContainer = TokenErrorContainer;

            } // lock (this.StateLock)
        }

        //------------------------------------------------------------
        // CSourceModule.CreateTokenStream
        //
        /// <summary>
        /// Call InternalCreateTokenStream method.
        /// </summary>
        /// <param name="pData"></param>
        //------------------------------------------------------------
        internal void CreateTokenStream(CSourceData pData)
        {
            InternalCreateTokenStream(pData);
            // Not catch exceptions in this methods.
        }

        //------------------------------------------------------------
        // CSourceModule.GetTextAndConnect
        //
        /// <summary>
        /// <para>Make sure that the source text has not been edited,
        /// and set the text of SourceText to LexDataBlock.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetTextAndConnect()
        {
            lock (StateLock)
            {
                POSDATA posEnd = new POSDATA();
                string src;

                src = SourceText.GetText(ref posEnd);
                LexDataBlock.InitSource(src);
                return (src != null);
            }
        }
#if false
        internal bool GetTextAndConnect()
        {
            // Must have acquired StateLock.
            // This method is called
            // by CSourceModule.Initialize and
            // CSourceModule.InternalCreateTokenStream.
            // These methods aquires StateLock.

            string src;

            POSDATA posEnd = new POSDATA();

            // Now, grab the text.  Note that we must unlock our state bits while we
            // do this, because change events from the text also lock the state bits and
            // we could otherwise deadlock.  As such, we need to check to see if any change
            // events came in between our unlock and lock, and if so, grab the text again.
            do
            {
                HaveEventsArrived = false;
                // haveEventsArrived is set to true in CSourceModule.OnEdit method.

                src = SourceText.GetText(ref posEnd);
                LexDataBlock.InitSource(src);
            } while (HaveEventsArrived);
            // If the text has been modified, read it again.

            // areEditsReceived is set to true in CSourceModule.OnEdit method.
            if (!this.EditsReceived)
            {
                //NewEditEndPoint = posEnd;
                //OldEditEndPoint = posEnd;
                //EditStartPoint.SetUninitialized();
            }

        // It's no longer modified
            //this.IsTextModified = false;
            this.EditsReceived = false;
            return (src != null);
        }
#endif

        //------------------------------------------------------------
        // CSourceModule.InternalFlush
        //
        /// <summary>
        /// Clear base, ChecksumList, TopTreeNode and reset flags.
        /// </summary>
        //------------------------------------------------------------
        override internal void InternalFlush()
        {
            // Must have acquired StateLock.

            base.InternalFlush();
            ChecksumList.Clear();
            treeTopNode = null;
            IsParseTreeTopAvailable = false;
            IsTokenized = false;
        }

        //------------------------------------------------------------
        // CSourceModule.TrackLine
        //
        /// <summary>
        /// <para>This function is called from the tokenizer
        /// when a physical end-of-line is found,
        /// to update the line table (growing it if necessary)
        /// and keep the lexer context up to date.</para>
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="newLineIndex"></param>
        //------------------------------------------------------------
        internal void TrackLine(CLexer lexer, int newLineIndex)
        {
            CTextReader text= lexer.TextReader;

            // Lexer limit checks
            // -- can't exceed the max sizes specified by POSDATA structures
            // (MAX_POS_LINE_LEN chars/line, MAX_POS_LINE lines)

            if (lexer.CurrentLineIndex >= POSDATA.MAX_POS_LINE - 1)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    0,
                    1,
                    CSCERRID.ERR_TooManyLines,
                    new ErrArg(POSDATA.MAX_POS_LINE));
                lexer.IsLineLimitExceeded = true;
            }
            else if (
                !lexer.IsTooLongLine &&
                newLineIndex - lexer.CurrentLineStartIndex > POSDATA.MAX_POS_LINE_LEN + 1)
                // (plus one for CRLF)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    POSDATA.MAX_POS_LINE_LEN - 1,
                    1,
                    CSCERRID.ERR_LineTooLong,
                    new ErrArg(POSDATA.MAX_POS_LINE_LEN));
                lexer.IsLineLimitExceeded = true;
            }

            lexer.NextLine();
            if (TrackingLines)
            {
                LexDataBlock.LineList.Add(newLineIndex);
            }
            else
            {
                DebugUtil.Assert(LexDataBlock.LineList[lexer.CurrentLineIndex] == newLineIndex);
            }
            lexer.CurrentLineStartIndex = newLineIndex;
            lexer.NotScannedLine = true;
        }

        //------------------------------------------------------------
        // CSourceModule.RecordCommentPosition
        //------------------------------------------------------------
        //internal void RecordCommentPosition(POSDATA pos);

        //------------------------------------------------------------
        // CSourceModule.RepresentNoiseTokens
        //
        /// <summary>
        /// Return true if TRACKCOMMENTS of CompilerCreationFlags is set.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool RepresentNoiseTokens()
        {
            return CheckFlags(CompilerCreationFlags.TRACKCOMMENTS);
        }

        //internal void TokenMemAlloc(CSTOKEN pToken, uint iSize);

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorLine
        //
        /// <summary>
        /// <para>Scan a preprocessor line.</para>
        /// <para>If this line has been already scanned, show a error message and return.</para>
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="firstOnLine"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorLine(CLexer lexer, bool firstOnLine)
        {
            DebugUtil.Assert(lexer != null);
            CTextReader text = lexer.TextReader;

            if (!firstOnLine)
            {
                // This is an error condition -- preprocessor directives must be first token
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    text.Index - lexer.CurrentLineStartIndex,
                    1,
                    CSCERRID.ERR_BadDirectivePlacement);
            }
            else
            {
                // Scan the actual directive
                ScanPreprocessorDirective(lexer);
            }

            // Make sure we end correctly...
            ScanPreprocessorLineTerminator(lexer);

            // Scan to end-of-line
            while (true)
            {
                char ch1 = text.Char;

                // End of the line.
                // Move to the first character of the next line.
                if (CharUtil.IsEndOfLineChar(ch1))
                {
                    text.Next();
                    int track = text.Index;
                    char ch2 = text.Char;

                    if (ch1 == '\r')
                    {
                        if (ch2 == '\n')
                        {
                            text.Next();
                            track = text.Index;
                        }
                    }

                    if (track >= 0)
                    {
                        // Only track physical (non-escaped) CR/CRLF guys
                        TrackLine(lexer, track);
                    }
                    break;
                }
                // End of the file.
                else if (text.Char == (char)0)
                {
                    break;
                }

                // To the next character.
                text.Next();
            }
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorLineTerminator
        //
        /// <summary>
        /// Check if the current preprocessor token is EOL.
        /// (First, skip white space characters.)
        /// </summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorLineTerminator(CLexer lexer)
        {
            int start;
            PPTOKENID ppId;
            string name;

            ppId = ScanPreprocessorToken(lexer, out start, out name, false);
            if (ppId != PPTOKENID.EOL)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    lexer.TextReader.Index - start,
                    CSCERRID.ERR_EndOfPPLineExpected);
            }
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorDirective
        //
        /// <summary></summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorDirective(CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);

            CTextReader text = lexer.TextReader;
            int start;

            // Scan/skip the '#' character
            DebugUtil.Assert(text.Char == '#');
            text.Next();

            string name;
            PPTOKENID ppId;

            // Scan a PP token, handle accordingly
            ppId = ScanPreprocessorToken(lexer, out start, out name, true);

            switch (ppId)
            {
                // These can't happen beyond the first token!
                case PPTOKENID.DEFINE:
                case PPTOKENID.UNDEF:
                    if (lexer.TokensHaveBeenScanned)
                    {
                        ErrorAtPosition(
                            lexer.CurrentLineIndex,
                            start - lexer.CurrentLineStartIndex,
                            text.Index - start,
                            CSCERRID.ERR_PPDefFollowsToken);
                        SkipRemainingPPTokens(lexer);
                        return;
                    }
                    ScanPreprocessorDeclaration(lexer, ppId == PPTOKENID.DEFINE);
                    break;

                case PPTOKENID.ERROR:
                case PPTOKENID.WARNING:
                    ScanPreprocessorControlLine(lexer, ppId == PPTOKENID.ERROR);
                    break;

                case PPTOKENID.REGION:
                    ScanPreprocessorRegionStart(lexer);
                    break;

                case PPTOKENID.ENDREGION:
                    text.Index = start;
                    ScanPreprocessorRegionEnd(lexer);
                    break;

                case PPTOKENID.IF:
                    ScanPreprocessorIfSection(lexer);
                    break;

                case PPTOKENID.ELIF:
                case PPTOKENID.ELSE:
                case PPTOKENID.ENDIF:
                    // Back the character index to the first character of this token.
                    text.Index = start;
                    ScanPreprocessorIfTrailer(lexer);
                    break;

                case PPTOKENID.LINE:
                    ScanPreprocessorLineDecl(lexer);
                    break;

                case PPTOKENID.PRAGMA:
                    if (OptionManager.IsLangVersionECMA1)
                    {
                        string featureName = null;

                        // This feature isn't allowed
                        Exception excp = null;
                        CResources.GetString(
                            ResNo.CSCSTR_FeaturePragma,
                            out featureName,
                            out excp);
                        ErrorAtPosition(
                            lexer.CurrentLineIndex,
                            start - lexer.CurrentLineStartIndex,
                            text.Index - start,
                            CSCERRID.ERR_NonECMAFeature,
                            new ErrArg(featureName));
                    }
                    ScanPreprocessorPragma(lexer);
                    break;

                default:
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        start - lexer.CurrentLineStartIndex,
                        text.Index - start,
                        CSCERRID.ERR_PPDirectiveExpected);
                    break;
            }
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorToken
        //
        /// <summary>
        /// <para>if allIds is true, any id-like pp-token is returned
        /// (if, define, region, true, ...)</para>
        /// <para>if allIds is false, only "true" and "false" are considered special tokens,
        /// and "if", "region", etc. are considered identifiers.</para>
        /// <para>The character index will be set to the character next to a token, except
        /// <list type="bullet">
        /// <item><term>'/'</term><description>stay at '/'</description></item>
        /// <item><term>identifier</term>
        /// <description>the last character of the token.</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="start">The index of the first character of a token is set.</param>
        /// <param name="name">If the tokein is an identifier, its string is set.</param>
        /// <param name="allIds"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PPTOKENID ScanPreprocessorToken(
            CLexer lexer,
            out int start,
            out string name,
            bool allIds)
        {
            DebugUtil.Assert(lexer != null);
            CTextReader text = lexer.TextReader;
            start = -1;
            name = null;

            char ch, chSurrogate = '\0';
            bool hasEscapes = false;

            // Skip whitespace
            while (CharUtil.IsWhitespaceCharNotEndOfLine(text.Char))
            {
                text.Next();
            }
            start = text.Index;

            // Check for end-of-line
            if (CharUtil.IsEndOfLineChar(text.Char) || text.End())
            {
                return PPTOKENID.EOL;
            }

            ch = text.Char;
            text.Next();
            switch (ch)
            {
                case '(':
                    return PPTOKENID.OPENPAREN;

                case ')':
                    return PPTOKENID.CLOSEPAREN;

                case ',':
                    return PPTOKENID.COMMA;

                case '!':
                    if (text.Char == '=')
                    {
                        text.Next();
                        return PPTOKENID.NOTEQUAL;
                    }
                    return PPTOKENID.NOT;

                case '=':
                    if (text.Char == '=')
                    {
                        text.Next();
                        return PPTOKENID.EQUAL;
                    }
                    break;

                case '&':
                    if (text.Char == '&')
                    {
                        text.Next();
                        return PPTOKENID.AND;
                    }
                    break;

                case '|':
                    if (text.Char == '|')
                    {
                        text.Next();
                        return PPTOKENID.OR;
                    }
                    break;

                case '/':
                    if (text.Char == '/')
                    {
                        text.Index = start;   // Don't go past EOL...
                        return PPTOKENID.EOL;
                    }
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    while (text.Char >= '0' && text.Char <= '9')
                    {
                        text.Next();
                    }
                    return PPTOKENID.NUMBER;

                case '\"':
                    while (text.Char != '\"' && !CharUtil.IsEndOfLineChar(text.Char) && !text.End())
                    {
                        text.Next();
                    }

                    if (text.Char == '\"')
                    {
                        text.Next();
                        return PPTOKENID.STRING;
                    }
                    // Didn't end in a quote, so it's not a string
                    text.Index = start + 1;
                    break;

                case '\\':
                    if (text.Char == 'u' || text.Char == 'U')
                    {
                        hasEscapes = true;
                        ch = lexer.ScanUnicodeEscape(text, out chSurrogate, false);
                        // Fall-through
                        goto default;
                    }
                    else
                    {
                        break;
                    }

                default:
                    {
                        // Only other thing can be identifiers, which are post-checked
                        // for the token identifiers (if, define, etc.)
                        // BUG 424819 : Handle identifier chars > 0xFFFF via surrogate pairs

                        if (!CharUtil.IsIdentifierChar(ch))
                        {
                            break;
                        }

                        int prevIndex;
                        StringBuilder sb = new StringBuilder();
                        do
                        {
                            if (chSurrogate != '\0')
                            {
                                hasEscapes = true;
                                // Keep these guys together
                                if (sb.Length < MAX_PPTOKEN_LEN - 1)
                                {
                                    sb.Append(ch);
                                    sb.Append(chSurrogate);
                                }
                                else if (sb.Length < MAX_PPTOKEN_LEN)
                                {
                                    // We couldn't fit the 2-char surrgate,
                                    // but we could fit a signle char,
                                    // so don't pickup whatever's after the surrogate
                                }
                            }
                            else if (sb.Length < MAX_PPTOKEN_LEN)
                            {
                                sb.Append(ch);
                            }

                            prevIndex = text.Index;
                            ch = lexer.NextChar(text, out chSurrogate);
                        } while (CharUtil.IsIdentifierCharOrDigit(ch));
                        // BUG 424819 : Handle identifier chars > 0xFFFF via surrogate pairs

                        // Backup to the previous character
                        // (because we read one more than we need to)
                        text.Index = prevIndex;

                        name = sb.ToString();
                        this.NameManager.AddString(name);
                        int ppIndex;
                        if (!hasEscapes && this.NameManager.IsPreprocessorKeyword(name, out ppIndex))
                        {
                            if (allIds || !CSourceModuleBase.IsPreprocessorKeywordDirective[ppIndex])
                            {
                                return (PPTOKENID)ppIndex;
                            }
                        }
                        return PPTOKENID.IDENTIFIER;
                    }
            } // switch (ch)

            // If you break out of the above switch, it is assumed you didn't recognize
            // anything...
            return PPTOKENID.UNKNOWN;
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorNumber
        //
        /// <summary></summary>
        /// <param name="number"></param>
        /// <param name="lexer"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ScanPreprocessorNumber(out int number, CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);
            CTextReader text = lexer.TextReader;
            bool isValidNum = true;
            number = 0;

            char ch = text.Char;
            while (ch >= '0' && ch <= '9')
            {
                try
                {
                    if (isValidNum)
                    {
                        number = checked(number * 10 + (ch - '0'));
                    }
                }
                catch (OverflowException)
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        text.Index - lexer.CurrentLineStartIndex,
                        0,
                        CSCERRID.ERR_IntOverflow);
                    isValidNum = false;
                    // Keep parsing the number so we don't get multiple bogus errors
                }
                text.Next();
                ch = text.Char;
            }
            return isValidNum;
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorFilename
        //
        /// <summary>
        /// <para>Return true if there was either no filename
        /// or if there was a complete filename.</para>
        /// <para>false indicates an error or warning was reported.</para>
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="fileName"></param>
        /// <param name="isPragma"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ScanPreprocessorFilename(CLexer lexer, out string fileName, bool isPragma)
        {
            DebugUtil.Assert(lexer != null);
            fileName = null;

            CTextReader text = lexer.TextReader;
            int fileNameStart = text.Index;
            string name = null;
            string path;

            //--------------------------------------------------
            // No string so no filename, backup and return
            //--------------------------------------------------
            if (PPTOKENID.STRING != ScanPreprocessorToken(
                    lexer,
                    out fileNameStart,
                    out name,
                    false))
            {
                text.Index = fileNameStart;
                return true;
            }

            //--------------------------------------------------
            // If a string has been found, check if it is an invalid filename.
            // fileNameStart is of the left '"'
            // and text.Index is of the character next to the right '"'
            //--------------------------------------------------
            if (text.Index - fileNameStart <= 2)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    fileNameStart - lexer.CurrentLineStartIndex,
                    text.Index - fileNameStart,
                    CSCERRID.WRN_EmptyFileName);
            }
            else
            {
                path = text.Text.Substring(fileNameStart + 1, text.Index - fileNameStart - 2);
                try
                {
                    string dir = Path.GetDirectoryName(path);
                }
                catch (ArgumentException)
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        fileNameStart - lexer.CurrentLineStartIndex,
                        text.Index - fileNameStart,
                        isPragma ? CSCERRID.WRN_FileNameTooLong : CSCERRID.ERR_FileNameTooLong);
                    return false;
                }
                catch (PathTooLongException)
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        fileNameStart - lexer.CurrentLineStartIndex,
                        text.Index - fileNameStart,
                        isPragma ? CSCERRID.WRN_FileNameTooLong : CSCERRID.ERR_FileNameTooLong);
                    return false;
                }

                // now make the filename fully-qualified
                // If it is relative, assume it is relative to the source file
                // So start by getting the source file's path (without filename)

                if (IOUtil.IsPathRooted(path))
                {
                    fileName = path;
                    return true;
                }
                else
                {
                    DebugUtil.Assert(
                        this.SourceFileInfo != null &&
                        this.SourceFileInfo != null);

                    try
                    {
                        fileName = Path.Combine(
                            this.SourceFileInfo.DirectoryName,
                            path);
                        return true;
                    }
                    catch (ArgumentException ex)
                    {
                        this.controller.ReportError(ERRORKIND.ERROR, ex);
                    }
                    catch (PathTooLongException ex)
                    {
                        this.controller.ReportError(ERRORKIND.ERROR, ex);
                    }
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorPragma
        //
        // pp-pragma:
        //     [whitespace]  "#"  [whitespace]  "pragma"  whitespace  pragma-body  pp-new-line
        // pragma-body:
        //     pragma-warning-body
        //
        /// <summary></summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorPragma(CLexer lexer)
        {
            string name;
            int start;
            PPTOKENID ppId;

            ppId = ScanPreprocessorToken(lexer, out start, out name, true);

            if (ppId == PPTOKENID.WARNING)
            {
                ScanPreprocessorPragmaWarning(lexer);
            }
            else if (ppId == PPTOKENID.CHECKSUM)
            {
                ScanPreprocessorPragmaChecksum(lexer);
            }
            else
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    lexer.TextReader.Index - start,
                    CSCERRID.WRN_IllegalPragma);
                SkipRemainingPPTokens(lexer);
            }

            // Check for the end of line here rather than the normal place so we can ensure no
            // errors are ever reported on a #pragma directive
            ppId = ScanPreprocessorToken(lexer, out start, out name, false);
            if (ppId != PPTOKENID.EOL)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    lexer.TextReader.Index - start,
                    CSCERRID.WRN_EndOfPPLineExpected);
                SkipRemainingPPTokens(lexer);
            }
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorPragmaWarning
        //
        // pragma-warning-body:
        //     "warning"  whitespace  warning-action
        //     "warning"  whitespace  warning-action  whitespace  warning-list
        // warning-action:
        //     "disable"
        //     "restore"
        // warning-list:
        //     decimal-digits
        //     warning-list  [whitespace]  ","  [whitespace]  decimal-digits
        //
        /// <summary></summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorPragmaWarning(CLexer lexer)
        {
            CTextReader text = lexer.TextReader;
            string name;
            int start;
            PPTOKENID ppId;

            ppId = ScanPreprocessorToken(lexer, out start, out name, true);

            if (ppId == PPTOKENID.DISABLE || ppId == PPTOKENID.RESTORE)
            {
                bool isDisable = (ppId == PPTOKENID.DISABLE);
                bool hadNumbers = false;
                List<int> numberList = new List<int>();

                start = text.Index;
                if (LexDataBlock.WarningMap == null)
                {
                    LexDataBlock.WarningMap = new CWarningMap();
                }

                if (ScanPreprocessorToken(lexer, out  start, out name, false) != PPTOKENID.EOL)
                {
                    text.Index = start;
                    do
                    {
                        int warn;
                        if (ScanPreprocessorToken(lexer, out start, out name, false)
                                == PPTOKENID.NUMBER)
                        {
                            text.Index = start;
                            if (!ScanPreprocessorNumber(out warn, lexer))
                            {
                                goto FAIL;
                            }

                            // Warning numbers are validated later during parsing
                            // so we can track the warnings easier
                            if ((warn & 0xFFFF) != warn ||
                                !CSCErrorInfo.Manager.IsValidWarningNumber((CSCERRID)warn))
                            {
                                ErrorAtPosition(
                                    lexer.CurrentLineIndex,
                                    start - lexer.CurrentLineStartIndex,
                                    text.Index - start,
                                    CSCERRID.WRN_BadWarningNumber,
                                    new ErrArg(text.Text.Substring(start, text.Index - start)));
                            }
                            else if (!this.OptionManager.IsNoWarnNumber(warn))
                            {
                                numberList.Add(warn);
                            }
                            hadNumbers = true;
                        }
                        else
                        {
                            ErrorAtPosition(
                                lexer.CurrentLineIndex,
                                start - lexer.CurrentLineStartIndex,
                                text.Index - start,
                                CSCERRID.WRN_InvalidNumber);
                            goto FAIL;
                        }
                    } while (ScanPreprocessorToken(lexer, out start, out name, false)
                                == PPTOKENID.COMMA);
                }

                // Don't add an empty disable or restore directive
                // unless the user specified it that way!
                if (numberList.Count > 0 || !hadNumbers)
                {
                    LexDataBlock.WarningMap.AddWarning(
                        lexer.CurrentLineIndex,
                        isDisable, numberList);
                }

                text.Index = start;
            }
            else
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    text.Index - start,
                    CSCERRID.WRN_IllegalPPWarning);
            }
            return;

        FAIL:
            SkipRemainingPPTokens(lexer);
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorPragmaChecksum
        //
        // #pragma checksum "filename" "{guid}" "checksum bytes"
        //
        /// <summary></summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorPragmaChecksum(CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);

            CTextReader text = lexer.TextReader;
            string fileName;
            string tokenName;
            int tokenStart;
            Guid guidChecksumID;
            List<byte> checksumBytes = null;

            // Filename
            if (!ScanPreprocessorFilename(lexer, out fileName, true))
            {
                goto FAIL;
            }

            // We should have gotten a filename, followed by whitespace and a string
            if (fileName == null || !CharUtil.IsWhitespaceCharNotEndOfLine(text.Char))
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    text.Index - lexer.CurrentLineStartIndex,
                    1,
                    CSCERRID.WRN_IllegalPPChecksum);
                goto FAIL;
            }

            if (PPTOKENID.STRING != ScanPreprocessorToken(lexer, out tokenStart, out tokenName, true) ||
            text.Index - tokenStart <= 2)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    tokenStart - lexer.CurrentLineStartIndex,
                    text.Index - tokenStart,
                    CSCERRID.WRN_IllegalPPChecksum);
                goto FAIL;
            }

            string strGuid;
            try
            {
                strGuid = text.Text.Substring(tokenStart + 1, text.Index - tokenStart - 2);
            }
            catch (ArgumentOutOfRangeException)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    tokenStart - lexer.CurrentLineStartIndex,
                    text.Index - tokenStart,
                    CSCERRID.WRN_IllegalPPChecksum);
                goto FAIL;
            }

            // Parse the string into a GUID
            if (!GuidUtil.GuidFromString(strGuid, out guidChecksumID))
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    tokenStart - lexer.CurrentLineStartIndex,
                    text.Index - tokenStart,
                    CSCERRID.WRN_IllegalPPChecksum);
                goto FAIL;
            }

            // Whitespace and a string of bytes
            if (!CharUtil.IsWhitespaceCharNotEndOfLine(text.Char))
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    text.Index - lexer.CurrentLineStartIndex,
                    1,
                    CSCERRID.WRN_IllegalPPChecksum);
                goto FAIL;
            }

            if (PPTOKENID.STRING != ScanPreprocessorToken(lexer, out tokenStart, out tokenName, true) ||
                ((text.Index - tokenStart) & 1) != 0 ||
                (text.Index - tokenStart < 4))
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    tokenStart - lexer.CurrentLineStartIndex,
                    text.Index - tokenStart,
                    CSCERRID.WRN_IllegalPPChecksum);
                goto FAIL;
            }

            checksumBytes = new List<byte>();

            // +/-1 to take into account the open and close quotes
            int start = tokenStart + 1;
            int end = text.Index - 1;
            if (text[start] == '{')
            {
                if (end - start < 4 || text[end - 1] != '}')
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        tokenStart - lexer.CurrentLineStartIndex,
                        text.Index - tokenStart,
                        CSCERRID.WRN_IllegalPPChecksum);
                    goto FAIL;
                }
                ++start;
                --end;
            }
            for (int index = start; index < end; index += 2)
            {
                int val1 = CharUtil.HexValue(text.Text[index]);
                int val2 = CharUtil.HexValue(text.Text[index + 1]);
                if (val1 < 0 || val2 < 0)
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        index - lexer.CurrentLineStartIndex,
                        2,
                        CSCERRID.WRN_IllegalPPChecksum);
                    goto FAIL;
                }
                checksumBytes.Add((byte)((val1 << 4) | val2));
            }

            CChecksumData checksum = new CChecksumData();
            checksum.Filename = fileName;
            checksum.GuidChecksumID = guidChecksumID;
            checksum.ChecksumData = checksumBytes;
            checksum.ErrlocChecksum = new ERRLOC(
                this,
                new POSDATA(lexer.CurrentLineIndex, 0),
                new POSDATA(lexer.CurrentLineIndex,text.Index - lexer.CurrentLineStartIndex));

            ChecksumList.Add(checksum);
            return;

        FAIL:
            SkipRemainingPPTokens(lexer);
            return;
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorDeclaration
        //
        // pp-declaration:
        //     [whitespace]  "#"  [whitespace]  "define"  whitespace  conditional-symbol  pp-new-line
        //     [whitespace]  "#"  [whitespace]  "undef"  whitespace  conditional-symbol  pp-new-line
        // pp-new-line:
        //     [whitespace]  [single-line-comment]  new-line
        //
        // These directives must occur before the first token in the source file.
        //
        /// <summary>
        /// <para>Parse #define and #undef derecrive.</para>
        /// <para>If argument define is true, Add a identifier to a preprocessor symbol table,
        /// otherwise remove it.</para>
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="define"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorDeclaration(CLexer lexer, bool define)
        {
            string name;
            int start;

            if (ScanPreprocessorToken(lexer, out start, out name, false) != PPTOKENID.IDENTIFIER)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    lexer.TextReader.Index - start,
                    CSCERRID.ERR_IdentifierExpected);
                return;
            }

            if (define)
            {
                preprocessorSymTable.Define(name);
            }
            else
            {
                preprocessorSymTable.Undef(name);
            }
            CCSymbolChanged = true;
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorControlLine
        //
        // pp-diagnostic:
        //     [whitespace]  "#"  [whitespace]  "error"  pp-message
        //     [whitespace]  "#"  [whitespace]  "warning"  pp-message
        // pp-message:
        //     new-line
        //     whitespace  [input-characters]  new-line
        //
        /// <summary>
        /// <para>Process #error and #warning directive.</para>
        /// <para>Show the rest of the line immediately.</para>
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="isError"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorControlLine(CLexer lexer, bool isError)
        {
            DebugUtil.Assert(lexer != null);
            CTextReader text = lexer.TextReader;
            //WCHAR   szBuf[501];  // 500 is the maximum size of the message we'll support.
            //PWSTR   pszRun = szBuf;
            const int MAX_LENGTH = 500;
            StringBuilder sb = new StringBuilder();

            // The rest of this line (including any single-line comments) is the error/warning text.
            // First, skip leading whitespace
            while (CharUtil.IsWhitespaceCharNotEndOfLine(text.Char))
            {
                text.Next();
            }

            //PCWSTR  pszStart = p;
            int start = text.Index;

            // Copy the rest of the line (up to limit chars) into szBuf
            while (true)
            {
                if (text.End() || CharUtil.IsEndOfLineChar(text.Char)) break;

                if (sb.Length < MAX_LENGTH - 1) sb.Append(text.Char);
                text.Next();
            }

            ErrorAtPosition(
                lexer.CurrentLineIndex,
                start - lexer.CurrentLineStartIndex,
                text.Index  - start,
                isError ? CSCERRID.ERR_ErrorDirective : CSCERRID.WRN_WarningDirective,
                new ErrArg(sb.ToString()));
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorRegionStart
        //
        // pp-start-region:
        //     [whitespace]  "#"  [whitespace]  "region"  pp-message
        //
        /// <summary>
        /// Push a CCStateFlags value to CompilationStateStack and Record to CCStateList.
        /// </summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorRegionStart(CLexer lexer)
        {
            // This is pretty simply -- just push a record on the CC stack so that we
            // don't allow overlapping of regions with #if guys...
            PushCCRecord(CCStateFlags.REGION);
            MarkCCStateChange(lexer.CurrentLineIndex, CCStateFlags.REGION | CCStateFlags.ENTER);

            // We don't care about the text following...
            SkipRemainingPPTokens(lexer);
            return;
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorRegionEnd
        //
        // pp-end-region:
        //     [whitespace]  "#"  [whitespace]  "endregion"  pp-message
        //
        /// <summary>
        /// Pop CompilationStateStack and Record to CCStateList.
        /// </summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorRegionEnd(CLexer lexer)
        {
            string name;
            int start;
            PPTOKENID ppId;

            ppId = ScanPreprocessorToken(lexer, out start, out name, true);
            DebugUtil.Assert(ppId == PPTOKENID.ENDREGION);

            // Make sure we're in the right kind of CC stack state
            if (CompilationStateStack == null ||
                (CompilationStateStack.Peek() & CCStateFlags.REGION) == 0)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    lexer.TextReader.Index - start,
                    CompilationStateStack == null
                        ? CSCERRID.ERR_UnexpectedDirective
                        : CSCERRID.ERR_EndifDirectiveExpected);
            }
            else
            {
                PopCCRecord();
                MarkCCStateChange(lexer.CurrentLineIndex, CCStateFlags.REGION);
            }

            SkipRemainingPPTokens(lexer);
            return;
        }

        // pp-conditional:
        //     pp-if-section  [pp-elif-sections]  [pp-else-section]  pp-endif
        // pp-if-section:
        //     [whitespace]  "#"  [whitespace]  "if"  whitespace  pp-expression  pp-new-line  [conditional-section]
        // pp-elif-sections:
        //     pp-elif-section
        //     pp-elif-sections  pp-elif-section
        // pp-elif-section:
        //     [whitespace]  "#"  [whitespace]  "elif"  whitespace  pp-expression  pp-new-line  [conditional-section]
        // pp-else-section:
        //     [whitespace]  "#"  [whitespace]  "else"  pp-new-line  [conditional-section]
        // pp-endif:
        //     [whitespace]  "#"  [whitespace]  "endif"  pp-new-line
        // conditional-section:
        //     input-section
        //     skipped-section
        // skipped-section:
        //     skipped-section-part
        //     skipped-section  skipped-section-part
        // skipped-section-part:
        //     [skipped-characters]  new-line
        //     pp-directive
        // skipped-characters:
        //     [whitespace]  not-number-sign  [input-characters]
        // not-number-sign:
        //     Any input-character except "#"

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorIfSection
        //
        /// <summary></summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorIfSection(CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);

            CTextReader text = lexer.TextReader;
            string name;
            int start;

            //--------------------------------------------------
            // First, scan and evaluate a preprocessor expression.
            // If TRUE, push an appropriate state record and return
            //--------------------------------------------------
            if (EvalPreprocessorExpression(lexer, PPTOKENID.OR))
            {
                // Push a CC record -- it is NOT an else block.  Also mark a state transition
                // (to reconstruct this stack state at incremental tokenization time)
                PushCCRecord(CCStateFlags.NONE);
                MarkCCStateChange(lexer.CurrentLineIndex, CCStateFlags.ENTER);
                return;
            }

            //--------------------------------------------------
            // The following is a case where #if does not hold.
            //--------------------------------------------------
            Stack<CCStateFlags> skipStack = new Stack<CCStateFlags>();

            // Mark this line as a transition (we're going from include to exclude)
            MarkTransition(lexer.CurrentLineIndex);

            // Make sure there's no other gunk on the end of this line
            ScanPreprocessorLineTerminator(lexer);

            while (true)
            {
                // Scan for the next preprocessor directive
                ScanExcludedCode(lexer);

                // Check for '#' (if not present, ScanExcludedCode will have reported the error...)
                if (text.Char != '#')
                {
                    // Don't leak the now-useless skip stack entries
                    while (skipStack.Count > 0)
                    {
                        skipStack.Pop();
                    }
                    return;     // NOTE:  No transition, so just return from here   
                }

                // Scan the directive
                text.Next();
                PPTOKENID ppId = ScanPreprocessorToken(lexer, out start, out name, true);

                // Bad mojo -- can't overlap #region/#endregion and #if/#else/elif/#endif blocks
                // Skip this directive and search next preprocessor directive.
                if (skipStack.Count > 0
                    &&
                    (((skipStack.Peek() & CCStateFlags.REGION) != 0 &&
                    (ppId == PPTOKENID.ELSE || ppId == PPTOKENID.ELIF || ppId == PPTOKENID.ENDIF)) ||
                    ((skipStack.Peek() & CCStateFlags.REGION) == 0 && (ppId == PPTOKENID.ENDREGION))
                    ))
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        start - lexer.CurrentLineStartIndex,
                        text.Index - start,
                        (skipStack.Peek() & CCStateFlags.REGION) != 0 ?
                            CSCERRID.ERR_EndRegionDirectiveExpected :
                            CSCERRID.ERR_EndifDirectiveExpected);
                    ScanAndIgnoreDirective(ppId, start, lexer);
                    continue;
                }

                //--------------------------------------------------
                // #else
                //--------------------------------------------------
                if (ppId == PPTOKENID.ELSE)
                {
                    if (skipStack.Count == 0)
                    {
                        // Push a CC record -- we ARE in the else block now.  Also mark
                        // a CC state transition to enable stack reconstruction.  Then break
                        // out of the while loop to mark this as a transition line
                        PushCCRecord(CCStateFlags.ELSE);
                        MarkCCStateChange(lexer.CurrentLineIndex, CCStateFlags.ELSE | CCStateFlags.ENTER);
                        break;
                    }
                    DebugUtil.Assert((skipStack.Peek() & CCStateFlags.REGION) == 0);
                }

                //--------------------------------------------------
                // #elif
                //--------------------------------------------------
                if (ppId == PPTOKENID.ELIF)
                {
                    if (skipStack.Count == 0)
                    {
                        if (EvalPreprocessorExpression(lexer, PPTOKENID.OR))
                        {
                            // This is the same as an IF block -- it is NOT an "else" block.
                            // Record the state change for reconstruction on retokenization.
                            // Break from the loop, which will mark this line as a transition
                            // and return.
                            PushCCRecord(CCStateFlags.NONE);
                            MarkCCStateChange(lexer.CurrentLineIndex, CCStateFlags.ENTER);
                            break;
                        }

                        // Make sure the line ends "cleanly" and then go scan another excluded block
                        ScanPreprocessorLineTerminator(lexer);
                        continue;
                    }
                    DebugUtil.Assert((skipStack.Peek() & CCStateFlags.REGION) == 0);
                }

                //--------------------------------------------------
                // #if (nested)
                //--------------------------------------------------
                if (ppId == PPTOKENID.IF)
                {
                    // This is the beginning of another if-section.  Push an 'if' record on to
                    // our skip stack, and then scan (and ignore) the directive to make sure it
                    // is syntactically correct.
                    skipStack.Push(CCStateFlags.NONE);
                    ScanAndIgnoreDirective(ppId, start, lexer);
                    continue;
                }

                //--------------------------------------------------
                // #endif
                //--------------------------------------------------
                if (ppId == PPTOKENID.ENDIF)
                {
                    // Here's the end of an if-section (but not necessarily ours).  If we
                    // are currently at the same depth as when we started, we're done.
                    // Otherwise, pop the top record off of our skip stack and continue.
                    if (skipStack.Count > 0)
                    {
                        skipStack.Pop();
                        ScanAndIgnoreDirective(ppId, start, lexer);
                        continue;
                    }

                    // This was our endif.  Break out, mark this line as transition.
                    break;
                }

                //--------------------------------------------------
                // #region
                //--------------------------------------------------
                if (ppId == PPTOKENID.REGION)
                {
                    // Here's the beginning of a region.  We don't care about it, but to
                    // enforce correct nesting, we push a record on our skip stack
                    skipStack.Push(CCStateFlags.REGION);
                    ScanAndIgnoreDirective(ppId, start, lexer);
                    continue;
                }

                //--------------------------------------------------
                // #endregion
                //--------------------------------------------------
                if (ppId == PPTOKENID.ENDREGION)
                {
                    // In order to be correct this must match the topmost record on
                    // our skip stack
                    if (skipStack.Count == 0 || (skipStack.Peek() & CCStateFlags.REGION) == 0)
                    {
                        ErrorAtPosition(
                            lexer.CurrentLineIndex,
                            start - lexer.CurrentLineStartIndex,
                            text.Index - start,
                            skipStack != null ? CSCERRID.ERR_EndifDirectiveExpected : CSCERRID.ERR_UnexpectedDirective);
                    }
                    else
                    {
                        skipStack.Pop();
                    }
                }

                // Unrecognized/ignored directive/token -- skip it, checking it for errors first
                ScanAndIgnoreDirective(ppId, start, lexer);
            }

            // Mark this line as another transition (from exclude to include)
            MarkTransition(lexer.CurrentLineIndex);

            DebugUtil.Assert(skipStack == null || skipStack.Count == 0);
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorIfTrailer
        //
        /// <summary>
        /// <para>Skip #elif blocks or #else block to the position next to #endif.</para>
        /// <para>The current character should be the first character of one of keywords above.</para>
        /// </summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorIfTrailer(CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);

            CTextReader text = lexer.TextReader;
            string name;
            int start;
            PPTOKENID ppId = ScanPreprocessorToken(lexer, out start, out name, true);

            // Here, we check to see if we are currently in a CC state.
            // If we aren't, the directive found is unexpected
            if (CompilationStateStack == null || CompilationStateStack.Count==0 ||
                (CompilationStateStack.Peek() & CCStateFlags.REGION) != 0)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    text.Index - start,
                    CompilationStateStack != null ? CSCERRID.ERR_EndRegionDirectiveExpected : CSCERRID.ERR_UnexpectedDirective);

                // Scan the rest of the tokens off so we don't get more errors.
                SkipRemainingPPTokens(lexer);
                return;
            }

            Stack<CCStateFlags> skipStack = new Stack<CCStateFlags>();

            // This line marks the state change (exit).  Do that now.
            MarkCCStateChange(lexer.CurrentLineIndex, CCStateFlags.NONE);

            //--------------------------------------------------
            // If this is not a PPTOKENID.ENDIF, we need to skip excluded code blocks
            //--------------------------------------------------
            if (ppId != PPTOKENID.ENDIF)
            {
                // We're transitioning here...
                MarkTransition(lexer.CurrentLineIndex);

                while (true)
                {
                    if (skipStack.Count > 0
                        && (
                        ((skipStack.Peek() & CCStateFlags.REGION) != 0 &&
                        (ppId == PPTOKENID.ELSE || ppId == PPTOKENID.ELIF || ppId == PPTOKENID.ENDIF))
                        ||
                        ((skipStack.Peek() & CCStateFlags.REGION) == 0 && (ppId == PPTOKENID.ENDREGION))
                        ))
                    {
                        // Bad mojo -- can't overlap #region/#endregion and #if/#else/elif/#endif blocks
                        ErrorAtPosition(
                            lexer.CurrentLineIndex,
                            start - lexer.CurrentLineStartIndex,
                            text.Index - start,
                            (skipStack.Peek() & CCStateFlags.REGION) != 0 ?
                            CSCERRID.ERR_EndRegionDirectiveExpected : CSCERRID.ERR_EndifDirectiveExpected);
                    }
                    else
                    {
                        // If there's nothing on our skip stack, we need to sniff this directive a bit...
                        if (skipStack.Count == 0)
                        {
                            DebugUtil.Assert(
                                CompilationStateStack != null &&
                                CompilationStateStack.Count > 0 &&
                                (CompilationStateStack.Peek() & CCStateFlags.REGION) == 0);

                            // If we were in the "else" block, we can't see any more else or elif blocks!
                            if ((CompilationStateStack.Peek() & CCStateFlags.ELSE) != 0 &&
                                (ppId == PPTOKENID.ELSE || ppId == PPTOKENID.ELIF))
                            {
                                ErrorAtPosition(
                                    lexer.CurrentLineIndex,
                                    start - lexer.CurrentLineStartIndex,
                                    text.Index - start,
                                    CSCERRID.ERR_UnexpectedDirective);
                            }

                            // #endregion is also bogus here...
                            if (ppId == PPTOKENID.ENDREGION)
                            {
                                ErrorAtPosition(
                                    lexer.CurrentLineIndex,
                                    start - lexer.CurrentLineStartIndex,
                                    text.Index - start,
                                    CSCERRID.ERR_UnexpectedDirective);
                            }

                            // Is this the else?  If so, mark it in the state record
                            if (ppId == PPTOKENID.ELSE)
                            {
                                CCStateFlags flags = CompilationStateStack.Pop();
                                flags |= CCStateFlags.ELSE;
                                CompilationStateStack.Push(flags);
                            }
                        }

                        // Handle the directives appropriately -- we care about #if/#region, and #endif/#endregion
                        if (ppId == PPTOKENID.IF || ppId == PPTOKENID.REGION)
                        {
                            // Push an appropriate record on the skip stack.
                            skipStack.Push(ppId == PPTOKENID.REGION ? CCStateFlags.REGION : 0);
                        }
                        else if (ppId == PPTOKENID.ENDIF || ppId == PPTOKENID.ENDREGION)
                        {
                            // If the skip stack is empty and this was an #endif, we're done.
                            if (ppId == PPTOKENID.ENDIF && skipStack.Count == 0)
                            {
                                break;
                            }

                            // Make sure the stack corresponds to the directive
                            if (skipStack.Count == 0 ||
                                (ppId == PPTOKENID.ENDIF && (skipStack.Peek() & CCStateFlags.REGION) != 0) ||
                                (ppId == PPTOKENID.ENDREGION && (skipStack.Peek() & CCStateFlags.REGION) == 0))
                            {
                                CSCERRID err;

                                if (skipStack.Count == 0)
                                {
                                    err = (CompilationStateStack.Peek() & CCStateFlags.REGION) != 0 ?
                                        CSCERRID.ERR_EndRegionDirectiveExpected : CSCERRID.ERR_EndifDirectiveExpected;
                                }
                                else
                                {
                                    err = (skipStack.Peek() & CCStateFlags.REGION) != 0 ?
                                        CSCERRID.ERR_EndRegionDirectiveExpected : CSCERRID.ERR_EndifDirectiveExpected;
                                }

                                ErrorAtPosition(
                                    lexer.CurrentLineIndex,
                                    start - lexer.CurrentLineStartIndex,
                                    text.Index - start,
                                    err);
                            }
                            else
                            {
                                skipStack.Pop();
                            }
                        }
                    }

                    // Scan and ignore the rest of whatever this directive was
                    ScanAndIgnoreDirective(ppId, start, lexer);

                    // Continue scanning excluded code for another directive
                    ScanExcludedCode(lexer);

                    // Check for '#' (if not present, ScanExcludedCode will have reported the error...)
                    if (text.Char != '#')
                    {
                        // Don't leak the now worthless skip stack entries...
                        while (skipStack.Count > 0) skipStack.Pop();

                        // Don't mark the reverse transition...
                        return;
                    }

                    // Scan the new directive and continue
                    text.Next();
                    ppId = ScanPreprocessorToken(lexer, out start, out name, true);
                }

                // Transition back out...
                MarkTransition(lexer.CurrentLineIndex);
            }

            // Okay... pop the state record and return...
            PopCCRecord();

            DebugUtil.Assert(skipStack == null || skipStack.Count == 0);
        }

        //------------------------------------------------------------
        // CSourceModule.ScanPreprocessorLineDecl
        //
        // pp-line:
        //     [whitespace]  "#"  [whitespace]  "line"  whitespace  line-indicator  pp-new-line
        // line-indicator:
        //     decimal-digits  whitespace  file-name
        //     decimal-digits
        //     "default"
        //     "hidden"
        // file-name:
        //     '"'  file-name-characters  '"'
        // file-name-characters:
        //     file-name-character
        //     file-name-characters  file-name-character
        // file-name-character:
        //     Any input-character except '"'
        //
        /// <summary></summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanPreprocessorLineDecl(CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);

            CTextReader text = lexer.TextReader;
            int start = -1;
            string name = null;


            // The rest of this line
            // (including any single-line comments) is the line and optional filename

            // First check for  "default" (this will skip past whitespace)
            PPTOKENID ppId = ScanPreprocessorToken(lexer, out start, out name, false);

            //--------------------------------------------------
            // #line default
            //--------------------------------------------------
            if (ppId == PPTOKENID.DEFAULT)
            {
                DebugUtil.Assert(
                    this.SourceText != null &&
                    this.SourceText.SourceFileInfo != null);

                name = IOUtil.SelectFileName(
                    this.SourceText.SourceFileInfo,
                    this.OptionManager.FullPaths);

                NameManager.AddString(name);
                LineMap.AddMap(
                    lexer.CurrentLineIndex,
                    true/*fDefaultLine*/,
                    lexer.CurrentLineIndex + 1, name);
            }
            //--------------------------------------------------
            // #line hidden
            //--------------------------------------------------
            else if (ppId == PPTOKENID.HIDDEN)
            {
                // and "hidden"
                LineMap.HideLines(lexer.CurrentLineIndex);
            }
            //--------------------------------------------------
            // #line <decimal number> [<filename>]
            //--------------------------------------------------
            else
            {
                bool isValidNum = (ppId == PPTOKENID.NUMBER);
                int newLineIndex = 0;

                if (isValidNum)
                {
                    // get the line number
                    text.Index = start;
                    isValidNum = ScanPreprocessorNumber(out newLineIndex, lexer);
                    if (isValidNum && (newLineIndex < 1 || newLineIndex > POSDATA.MAX_POS_LINE))
                    {
                        ErrorAtPosition(
                            lexer.CurrentLineIndex,
                            start - lexer.CurrentLineStartIndex,
                            0,
                            CSCERRID.ERR_InvalidLineNumber);
                    }
                }
                else
                {
                    // Reset to before the token
                    text.Index = start;
                }

                if (!isValidNum)
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        start - lexer.CurrentLineStartIndex,
                        0,
                        CSCERRID.ERR_InvalidLineNumber);
                    goto FAIL;
                }

                // do we have a filename?
                if (ScanPreprocessorFilename(lexer,out name, false))
                {
                    // Subtract 1 because lines are Zero based in the Lexer/Parser
                    LineMap.AddMap(lexer.CurrentLineIndex, false/*fDefaultLine*/, newLineIndex - 1, name);

                    if (name == null)
                    {
                        // Check for the EOL so we can give a better error message
                        start = -1;
                        name = null;
                        if (PPTOKENID.EOL != ScanPreprocessorToken(lexer, out start,out name, false))
                        {
                            ErrorAtPosition(
                                lexer.CurrentLineIndex,
                                start - lexer.CurrentLineStartIndex,
                                text.Index - start,
                                CSCERRID.ERR_MissingPPFile);

                            goto FAIL;
                        }
                    }
                }
                else
                {
                    goto FAIL;
                }
                return;

            FAIL:
                // Skip over these, so we don't get multiple errors
                SkipRemainingPPTokens(lexer);
            }
        }

        //------------------------------------------------------------
        // CSourceModule.EvalPreprocessorExpression
        //
        /// <summary>
        /// <para>Calculate boolean values of terms and operators
        /// with equal or higher precedence than the specified one.</para>
        /// <para>PPTOKENID values are arranged in order of precedence.
        /// The bigger PPTOKENID value has the higher (or equal) precedence.</para>
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="precedence"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool EvalPreprocessorExpression(CLexer lexer, PPTOKENID precedence)
        {
            DebugUtil.Assert(lexer != null);

            CTextReader text = lexer.TextReader;
            string name;
            int start = -1;
            PPTOKENID ppId;
            bool termValue = true;

            //--------------------------------------------------
            // First, we need a term -- possibly prefixed with a ! operator
            //--------------------------------------------------
            ppId = ScanPreprocessorToken(lexer, out start, out name, false);
            switch (ppId)
            {
                case PPTOKENID.NOT:
                    termValue = !EvalPreprocessorExpression(lexer, ppId);
                    break;

                case PPTOKENID.OPENPAREN:
                    termValue = EvalPreprocessorExpression(lexer, PPTOKENID.OR);   // (default (lowest) precedence)
                    if (ScanPreprocessorToken(lexer, out start, out name, false) != PPTOKENID.CLOSEPAREN)
                    {
                        ErrorAtPosition(
                            lexer.CurrentLineIndex,
                            start - lexer.CurrentLineStartIndex,
                            text.Index - start,
                            CSCERRID.ERR_CloseParenExpected);
                        text.Index = start;
                    }
                    break;

                case PPTOKENID.TRUE:
                case PPTOKENID.FALSE:
                    termValue = (ppId == PPTOKENID.TRUE);
                    break;

                case PPTOKENID.IDENTIFIER:
                    termValue = preprocessorSymTable.IsDefined(name);
                    break;

                default:
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        start - lexer.CurrentLineStartIndex,
                        text.Index - start,
                        CSCERRID.ERR_InvalidPreprocExpr);
                    SkipRemainingPPTokens(lexer);
                    return true;
            }

            //--------------------------------------------------
            // Okay, now look for an operator.
            //--------------------------------------------------
            ppId = ScanPreprocessorToken(lexer, out start, out name, false);
            if (ppId >= PPTOKENID.OR && ppId <= PPTOKENID.NOTEQUAL)
            {
                // We'll only 'take' this operator if it is of equal or higher precedence
                // than that which we were given
                if (ppId < precedence)
                {
                    // Back up to the token again so we don't skip it
                    // start is the index of an operator.
                    text.Index = start;
                }
                else
                {
                    bool rhsValue = EvalPreprocessorExpression(
                     lexer, ppId == PPTOKENID.NOTEQUAL ? PPTOKENID.EQUAL : ppId);

                    switch (ppId)
                    {
                        case PPTOKENID.OR:
                            termValue = termValue || rhsValue;
                            break;
                        case PPTOKENID.AND:
                            termValue = termValue && rhsValue;
                            break;
                        case PPTOKENID.EQUAL:
                            termValue = (!termValue) == (!rhsValue);
                            break;
                        case PPTOKENID.NOTEQUAL:
                            termValue = (!termValue) != (!rhsValue);
                            break;
                        default:
                            DebugUtil.Assert(false, "Unrecognized preprocessor expression operator!");
                            break;
                    }
                }
            }
            else
            {
                // Not an operator, so put the token back!
                text.Index = start;
            }

            return termValue;
        }

        //------------------------------------------------------------
        // CSourceModule.ScanExcludedCode
        //
        /// <summary>
        /// Simply skip everything until we see another '#' character
        /// as the first non-white character of a line, or until we hit end of file.
        /// If we DO hit end-of-file first, issue an error
        /// </summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanExcludedCode(CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);
            CTextReader text = lexer.TextReader;
            while (true)
            {
                char ch = text.Char;
                if (CharUtil.IsEndOfLineChar(ch))
                {
                    char temp = ch;
                    text.Next();
                    int trackIndex = text.Index;
                    ch = text.Char;

                    if (temp == '\r')
                    {
                        if (ch == '\n')
                        {
                            text.Next();
                            trackIndex = text.Index;
                            ch = text.Char;
                        }
                    }

                    if (!text.End())
                    {
                        // Only track the line if it's a physical LF (non-escaped)
                        TrackLine(lexer, trackIndex);

                        // Skip whitespace (include end-of-line characters)
                        while (CharUtil.IsWhitespaceChar(ch))
                        {
                            text.Next();
                            ch = text.Char;
                        }
                        // If this is a '#', we're done
                        if (ch == '#')
                        {
                            return;
                        }
                    }
                }
                else if (text.End())
                {
                    ErrorAtPosition(
                    lexer.CurrentLineIndex,
                     text.Index - lexer.CurrentLineStartIndex,
                      1,
                      CSCERRID.ERR_EndifDirectiveExpected);
                    return;
                }
                else
                {
                    text.Next();
                }
            }
        }

        //------------------------------------------------------------
        // CSourceModule.ScanAndIgnoreDirective
        //
        /// <summary>
        /// Skip to the end of a preprocessor directive.
        /// </summary>
        /// <param name="ppId"></param>
        /// <param name="start"></param>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void ScanAndIgnoreDirective(PPTOKENID ppId, int start, CLexer lexer)
        {
            DebugUtil.Assert(lexer != null);
            CTextReader text = lexer.TextReader;

            if (ppId == PPTOKENID.IF || ppId == PPTOKENID.ELIF)
            {
                EvalPreprocessorExpression(lexer,PPTOKENID.OR);
            }
            else if (
                ppId == PPTOKENID.ERROR ||
                ppId == PPTOKENID.WARNING ||
                ppId == PPTOKENID.REGION ||
                ppId == PPTOKENID.ENDREGION ||
                ppId == PPTOKENID.LINE ||
                ppId == PPTOKENID.PRAGMA)
            {
                SkipRemainingPPTokens(lexer);
            }
            else if (ppId == PPTOKENID.DEFINE || ppId == PPTOKENID.UNDEF)
            {
                string name;

                ppId = ScanPreprocessorToken(lexer, out start, out name, false);
                if (ppId != PPTOKENID.IDENTIFIER)
                {
                    ErrorAtPosition(
                        lexer.CurrentLineIndex,
                        start - lexer.CurrentLineStartIndex,
                        text.Index - start,
                        CSCERRID.ERR_IdentifierExpected);
                    SkipRemainingPPTokens(lexer);
                }
            }
            else if (ppId != PPTOKENID.ELSE && ppId != PPTOKENID.ENDIF && ppId != PPTOKENID.ENDREGION)
            {
                ErrorAtPosition(
                    lexer.CurrentLineIndex,
                    start - lexer.CurrentLineStartIndex,
                    text.Index - start,
                    CSCERRID.ERR_PPDirectiveExpected);
                SkipRemainingPPTokens(lexer);
            }

            // Make sure the line ends cleanly
            ScanPreprocessorLineTerminator(lexer);
        }

        //------------------------------------------------------------
        // CSourceModule.SkipRemainingPPTokens
        //
        /// <summary>
        /// Skip to the end of a preprocessor directive line.
        /// </summary>
        /// <param name="lexer"></param>
        //------------------------------------------------------------
        internal void SkipRemainingPPTokens(CLexer lexer)
        {
            int start;
            string name;
            PPTOKENID   ppID;

            do
            {
                ppID = ScanPreprocessorToken(lexer,out start, out name, false);
            }
            while (ppID != PPTOKENID.EOL);
        }

        //------------------------------------------------------------
        // CSourceModule.PushCCRecord (1)
        //
        /// <summary>
        /// Push a CCStateFlags to Stack&lt;CCStateFlags&gt; instance.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal void PushCCRecord(Stack<CCStateFlags> stack, CCStateFlags flags)
        {
            if (stack != null)
            {
                stack.Push(flags);
            }
        }

        //------------------------------------------------------------
        // CSourceModule.PushCCRecord (2)
        //
        /// <summary>
        /// Push a CCStateFlags to CompilationStateStack
        /// </summary>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal void PushCCRecord(CCStateFlags flags)
        {
            this.CompilationStateStack.Push(flags);
        }

        //------------------------------------------------------------
        // CSourceModule.PopCCRecord (1)
        //
        /// <summary>
        /// <para>Pop Stack&lt;CCStateFlags&gt; instance.</para>
        /// <para>If error occured, return CCStateFlage.NONE.</para>
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CCStateFlags PopCCRecord(Stack<CCStateFlags> stack)
        {
            CCStateFlags flags = CCStateFlags.NONE;

            if (stack != null)
            {
                try
                {
                    flags = stack.Pop();
                }
                catch (InvalidOperationException)
                {
                    return CCStateFlags.NONE;
                }
                return flags;
            }
            return CCStateFlags.NONE;
        }

        //------------------------------------------------------------
        // CSourceModule.PopCCRecord (2)
        //
        /// <summary>
        /// <para>Pop CompilationStateStack.</para>
        /// <para>If error occured, return CCStateFlage.NONE.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CCStateFlags PopCCRecord()
        {
            CCStateFlags flags = CCStateFlags.NONE;
            try
            {
                flags = this.CompilationStateStack.Pop();
            }
            catch (InvalidOperationException)
            {
                return CCStateFlags.NONE;
            }
            return flags;
        }

        //------------------------------------------------------------
        // CSourceModule.CompareCompilationStateStacks
        //
        /// <summary>
        /// If two Stack&lt;CCStateFlags&gt; instances have the same flags in the same order,
        /// return true.
        /// </summary>
        /// <param name="ccs1"></param>
        /// <param name="ccs2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CompareCompilationStateStacks(
            Stack<CCStateFlags> ccs1,
            Stack<CCStateFlags> ccs2)
        {

            if (ccs1 == null || ccs2 == null)
            {
                return false;
            }
            if (ccs1.Equals(ccs2))
            {
                return true;
            }
            if (ccs1.Count != ccs2.Count)
            {
                return false;
            }

            CCStateFlags[] arr1 = ccs1.ToArray();
            CCStateFlags[] arr2 = ccs2.ToArray();

            for (int i = 0; i < arr1.Length; ++i)
            {
                if (arr1[i] != arr2[i])
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // CSourceModule.MarkTransition
        //
        /// <summary>
        /// Register a line index to LexDataBlock.TransitionLineList.
        /// </summary>
        /// <param name="lineIndex"></param>
        //------------------------------------------------------------
        internal void MarkTransition(int lineIndex)
        {
            LexDataBlock.TransitionLineList.Add(lineIndex);
        }
        //------------------------------------------------------------
        // CSourceModule::MarkTransition (sscli)
        //------------------------------------------------------------
        //void CSourceModule::MarkTransition (long iLine)
        //{
        //    /* NOTE:  This was taken out for colorization purposes...
        //
        //    // Optimization:  Two consecutive transitions cancel each other out...
        //    if (m_lex.iTransitionLines > 0 &&
        //        ((m_lex.piTransitionLines[m_lex.iTransitionLines - 1]) == (iLine - 1)))
        //    {
        //        m_lex.iTransitionLines--;
        //        return;
        //    }
        //    */
        //
        //    // Make sure there's room
        //    if ((m_lex.iTransitionLines & 7) == 0)
        //    {
        //        size_t iSize = SizeMul(m_lex.iTransitionLines + 8, sizeof (long));
        //        m_lex.piTransitionLines = (long *)((m_lex.piTransitionLines == NULL) ?
        //            VSAlloc (iSize) : VSRealloc (m_lex.piTransitionLines, iSize));
        //        if (!m_lex.piTransitionLines)
        //            m_pController->NoMemory();
        //    }
        //
        //    ASSERT (m_lex.piTransitionLines[m_lex.iTransitionLines] != *(long *)"END!");
        //    m_lex.piTransitionLines[m_lex.iTransitionLines++] = iLine;
        //}

        //------------------------------------------------------------
        // CSourceModule.MarkCCStateChange
        //
        /// <summary>
        /// Push a CCState to LexDataBlock.CCStateList.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal void MarkCCStateChange(int lineIndex, CCStateFlags flags)
        {
            LexDataBlock.CCStateList.Add(new CCSTATE(flags, lineIndex));
        }

        //#define UPDTOK(i) UpdateTokenIndex(i)

        //------------------------------------------------------------
        // CSourceModule.GetInteriorNodeWorker
        //
        /// <summary>
        /// Unlike ssci, create an instance with new operator as usual.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CInteriorNode GetInteriorNodeWorker(BASENODE node)
        {
            this.IsParsingInterior = true;
            SetInteriorParserThread();

            return new CInteriorNode(this, node);
        }

        //------------------------------------------------------------
        // CSourceModule::GetInteriorNodeWorker (sscli)
        //------------------------------------------------------------
        //void CSourceModule::GetInteriorNodeWorker(CInteriorNode ** ppStoredNode, BASENODE * pNode)
        //{
        //   // No, so we need to create one.  Mark ourselves as parsing an
        //    // interior node in the state bits
        //    m_fParsingInterior = TRUE;
        //    SetInteriorParserThread();
        //
        //    // If no interior node is currently using the built-in heap, we'll
        //    // use that.  Otherwise, we need the new interior node to use its own heap.
        //    if (m_fInteriorHeapBusy)
        //    {
        //        CSecondaryInteriorNode  *pIntNode = CSecondaryInteriorNode::CreateInstance (this, m_pController, pNode);
        //        {
        //            WriteToggler allowWrite(ProtectedEntityFlags::ParseTree, *ppStoredNode);
        //            *ppStoredNode = pIntNode;
        //        }
        //        m_pActiveHeap = pIntNode->GetAllocationHeap();
        //        DBGOUT (("SECONDARY INTERIOR NODE CREATED!"));
        //    }
        //    else
        //    {
        //        {
        //            CPrimaryInteriorNode * pIntNode = CPrimaryInteriorNode::CreateInstance (this, pNode);
        //            WriteToggler allowWrite(ProtectedEntityFlags::ParseTree, *ppStoredNode);
        //            *ppStoredNode = pIntNode;
        //        }
        //        m_fInteriorHeapBusy = TRUE;
        //        m_pActiveHeap = m_pActiveTopLevelHeap;
        //        m_pActiveTopLevelHeap->Free(m_pMarkInterior);
        //    }
        //}

        //------------------------------------------------------------
        // CSourceModule.ParseInteriorWorker
        //
        /// <summary></summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void ParseInteriorWorker(BASENODE node)
        {
            moduleEventSource.FireOnBeginParse(node);
            ParseInteriorNode(node);
            moduleEventSource.FireOnEndParse(node);
            if (InParsingForErrors)
            {
                MarkInteriorAsParsed(node, true);
            }
        }

        //------------------------------------------------------------
        // CSourceModule.ParseSourceModuleWorker
        //
        /// <summary>
        /// <para>Call Parser.ParseSourceModule.
        /// This method returns a NAMESPACENODE instance
        /// which represents the root namespace.</para>
        /// <para>Set the root namespace node to this.treeTopNode.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void ParseSourceModuleWorker()
        {
            //NRHeapWriteAllower allowWrite(m_pActiveTopLevelHeap);

            // Must build parse tree
            moduleEventSource.FireOnBeginParse(null);
            treeTopNode = Parser.ParseSourceModule();
            moduleEventSource.FireOnEndParse(null);

            // Mark the allocator -- back to here is where we rewind when
            // parsing a new interior node
            //m_pActiveTopLevelHeap->Mark(m_pMarkInterior);
        }

        //------------------------------------------------------------
        // CSourceModule.MarkInteriorAsParsed
        //
        /// <summary>
        /// Set or unset node.NodeFlagsEx NODEFLAGS.EX_INTERIOR_PARSED
        /// </summary>
        /// <param name="pNode"></param>
        /// <param name="markAsParsed"></param>
        //------------------------------------------------------------
        internal void MarkInteriorAsParsed(BASENODE node, bool markAsParsed)
        {
            //WriteToggler allowWrite(ProtectedEntityFlags::ParseTree, *node);

            NODEFLAGS flags = node.NodeFlagsEx;
            if (markAsParsed)
            {
                flags |= NODEFLAGS.EX_INTERIOR_PARSED;
            }
            else
            {
                flags &= ~NODEFLAGS.EX_INTERIOR_PARSED;
            }
            node.NodeFlagsEx = flags;
        }

        //------------------------------------------------------------
        // CSourceModule.CreateNewError
        //
        /// <summary>
        /// <para>This is the error/warning creation function.
        /// All forms of errors and warnings from the lexer and parser come through here.
        /// Also, all forms of errors and warnings from the lexer/parser have exactly one location,
        /// so it is provided here.</para>
        /// <para>Create a CError instance and Add it to CurrentErrorContainer.</para>
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <param name="errArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CreateNewError(
            CSCERRID errorId,
            POSDATA posStart,
            POSDATA posEnd,
            params ErrArg[] errArgs)
        {
            // We may not have an error container -- if not, then we ignore errors.
            if (this.currentErrorContainer == null)
            {
                return false;
            }

            bool br = false;

            // Since we're going to use the error container,
            // we need to grab the state lock for this.
            lock (this.StateLock)
            {
                CError error = null;
                string str;

                if (!Controller.CreateError(errorId, errArgs, out error, false))
                {
                    return false;
                }

                string fileName, mapFileName;
                POSDATA mapStart = new POSDATA(posStart);
                POSDATA mapEnd = new POSDATA(posEnd);
                bool isHidden, isMapped;

                MapLocation(mapStart, out mapFileName, out isHidden, out isMapped);
                MapLocation(mapEnd, out str, out isHidden, out isMapped);

                fileName = IOUtil.SelectFileName(
                    this.SourceText.SourceFileInfo,
                    this.OptionManager.FullPaths);

                error.AddLocation(fileName, posStart, posEnd, mapFileName, mapStart, mapEnd);

                if (IsWarningDisabled(error))
                {
                    return false;
                }
                ErrorCount++;

                if (Controller.SuppressErrors && error.Kind != ERRORKIND.FATAL)
                {
                    return false;
                }

                // Now, add this error to the current error container.
                this.currentErrorContainer.AddError(error);
                br = (error.Kind != ERRORKIND.WARNING);
            } // lock (StateLock)
            return br;
        }

        //------------------------------------------------------------
        // CSourceModule.ErrorAtPositionArgs
        //
        /// <summary>
        /// <para>Use int's here so we don't have to cast
        /// when calculating positions based on pointer arithmetic</para>
        /// <para>Only called by lexer or preprocessor</para>
        /// </summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="extent"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ErrorAtPositionArgs(
            int line,
            int col,
            int extent,
            CSCERRID id,
            params ErrArg[] args)
        {
            POSDATA pos = new POSDATA(line, col);
            POSDATA posEnd = new POSDATA(line, col + (extent > 1 ? extent : 1));
#if DEBUG
            //    if (IsWarningID(id)) {
            //        WORD num = ErrorNumberFromID(id);
            //        for (int i = 0; lexerWarnNumbers[i] != 0; i++) {
            //            if (lexerWarnNumbers[i] == num)
            //                goto FOUND;
            //        }
            //        VSASSERT(!"Lexer warning not in allowed list",
            //                 "If you get this assert,
            //                  you need to update lexerWarnNumbers in srcmod.cpp
            //                  to include this warning number");
            //FOUND:;
            //    }
#endif
            CreateNewError(id, pos, posEnd, args);
        }

        //------------------------------------------------------------
        // CSourceModule.ErrorAtPosition
        //
        /// <summary></summary>
        /// <param name="iLine"></param>
        /// <param name="iCol"></param>
        /// <param name="iExtent"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ErrorAtPosition(
            int iLine,
            int iCol,
            int iExtent,
            CSCERRID id,
            params ErrArg[] args)
        {
            ErrorAtPositionArgs(iLine, iCol, iExtent, id, args);
        }

        //static int ExceptionFilter(ref EXCEPTION_POINTERS exceptionInfo, Object pv);

        //internal string BuildNodeKey(BASENODE pNode);

        // Source data reference management

        // CSourceModuleBase

        //------------------------------------------------------------
        // CSourceModule.FinalRelease
        //------------------------------------------------------------
        internal void FinalRelease()
        {
        }
        //// CSourceModule::FinalRelease
        //
        //void CSourceModule::FinalRelease ()
        //{
        //    BOOL    fLocked = LockObject.Acquire ();
        //
        //    if (VSFSWITCH (gfUnhandledExceptions))
        //    {
        //        delete this;
        //
        //        // Because of the above, this object is unlocked in the dtor...
        //        fLocked = FALSE;
        //    }
        //    else
        //    {
        //        PAL_TRY
        //        {
        //            delete this;
        //
        //            // Because of the above, this object is unlocked in the dtor...
        //            fLocked = FALSE;
        //        }
        //        EXCEPT_EXCEPTION
        //        {
        //            CLEANUP_STACK // handle stack overflow
        //            SourceModuleController->HandleException (GetExceptionCode());
        //        }
        //        PAL_ENDTRY
        //    }
        //
        //    if (fLocked)
        //    {
        //        ASSERT (LockObject.LockedByMe());
        //        LockObject.Release();
        //    }
        //}

        //------------------------------------------------------------
        // CSourceModule.SourceModule
        //
        /// <summary></summary>
        //------------------------------------------------------------
        override internal CSourceModule SourceModule
        {
            get { return this; }
        }

        //------------------------------------------------------------
        // CSourceModule.Clone
        //
        /// <summary></summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal CSourceModuleBase Clone(CSourceData pData)
        {
            throw new NotImplementedException("CSourceModule.Clone");
            //VSFAIL ("not implemented");
            //return null;
        }

        //internal int GetMemoryConsumptionPrivate(
        //    ref int piTopTree, ref int piInteriorTrees, ref int piInteriorNodes);
        //internal Object AllocFromActiveHeap(uint iSize);
        //internal void PrepareForCloning(ICSSourceData pData);
        //virtual NRHEAP * GetActiveHeap() { return m_pActiveHeap; }

        // Source data accessors

        //------------------------------------------------------------
        // CSourceModule.GetLexResults
        //
        /// <summary>
        /// Overwrite argument lexData with the data of this.lexDataBlock.
        /// (Call CreateTokenStream method.)
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="lexData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool GetLexResults(CSourceData sourceData, LEXDATA lexData)
        {
            CreateTokenStream(sourceData);
            this.lexDataBlock.CloneTo(lexData);
            return true;
        }
        //------------------------------------------------------------
        // CSourceModule::GetLexResults (sscli)
        //
        // #define VSFSWITCH(NAME) FALSE // palrt\inc\vsassert.h(98)
        //------------------------------------------------------------
        //HRESULT CSourceModule::GetLexResults (CSourceData *pData, LEXDATA *pLexData)
        //{
        //    HRESULT     hr = E_FAIL;
        //
        //    if (VSFSWITCH (gfUnhandledExceptions))
        //    {
        //        CreateTokenStream (pData);
        //        *pLexData = m_lex;
        //        hr = S_OK;
        //    }
        //    else
        //    {
        //        PAL_TRY
        //        {
        //            CreateTokenStream (pData);
        //            *pLexData = m_lex;
        //            hr = S_OK;
        //        }
        //        PAL_EXCEPT_FILTER(ExceptionFilter, this)
        //        {
        //            CLEANUP_STACK // handle stack overflow
        //            m_pController->HandleException (GetExceptionCode());
        //            hr = E_FAIL;
        //        }
        //        PAL_ENDTRY
        //    }
        //
        //    return hr;
        //}

        //------------------------------------------------------------
        // CSourceModule.UnsafeGetLexData
        //
        /// <summary></summary>
        /// <param name="lexData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool UnsafeGetLexData(LEXDATA lexData)
        {
            return this.GetLexResults(null, lexData);
        }

        //------------------------------------------------------------
        // CSourceModule.UnsafeGetLexData
        //------------------------------------------------------------
        //// CSourceModule::UnsafeGetLexData
        //HRESULT CSourceModule::UnsafeGetLexData(LEXDATA* pLexData)
        //{
        //    return GetLexResults(NULL, pLexData);
        //}

        //internal int GetSingleTokenData(CSourceData pData, int iToken, TOKENDATA pTokenData);
        //internal int GetSingleTokenPos(CSourceData pData, int iToken, POSDATA pposToken);
        //internal int IsInsideComment(CSourceData pData, POSDATA pos, ref bool pfInComment);
        //internal int InternalIsInsideComment(CSourceData pData, POSDATA pos, ref bool pfInComment);

        //------------------------------------------------------------
        // CSourceModule.ParseTopLevel
        //
        /// <summary>
        /// <list type="number">
        /// <item>Create a sequence of tokens from a source text.</item>
        /// <item>call ParseSourceModuleWorker to parse it.
        /// ParseSourceModuleWorker calls PARSER.ParseSourceModule</item>
        /// </list>
        /// </summary>
        /// <param name="createParseDiffs"></param>
        /// <param name="ppNode"></param>
        /// <param name="sourceData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool ParseTopLevel(
            CSourceData sourceData,
            out BASENODE ppNode,
            bool createParseDiffs)
        {
            bool br = true;
            ppNode = null;

            if (this.ParseCrashed)
            {
                return false;
            }

            // Make sure the token stream is up-to-date...
            // palrt\inc\vsassert.h(98): #define VSFSWITCH(NAME) FALSE

            //--------------------------------------------------
            // Create token sequence from the source text.
            //--------------------------------------------------
            try
            {
                // if isTokenized == true, does nothing.
                CreateTokenStream(sourceData);
            }
            catch (Exception)
            {
                //CLEANUP_STACK // handle stack overflow
                //SourceModuleController->HandleException (GetExceptionCode());
                //VSFAIL ("YIPES!  CRASH DURING TOKENIZATION!");
                //ClearTokenizerThread();

                this.ParseCrashed = true;
                this.CommitTokenizing = false;
                this.IsTokenized = false;
                br = false;
            }
            if (!br)
            {
                return false;
            }

#if DEBUG
            StringBuilder debugSb = new StringBuilder();
            sourceData.LexData.DebugTokenList(debugSb);
#endif

            // Now, check the state to see if we need to do any parsing work.
            //BOOL    fLocked = m_StateLock.Acquire();
            lock (this.StateLock)
            {
                // ParseTopLevel cannot be called while state is locked!
                //ASSERT (fLocked);

                // Get the source file name, if we can, to pass to the parser.
                //PCWSTR szFileName;
                //if (FAILED(SourceText->GetName(&szFileName)))
                //    szFileName = L"";

                //string fileName = SourceText.Name;
                //if (fileName == null)
                //{
                //    fileName = "";
                //}

                //if (commitParsingTop)
                //{
                //    // Someone else is currently parsing the top, so wait until they are done.
                //    //ASSERT (m_dwParserThread != GetCurrentThreadId());
                //    while (commitParsingTop)
                //    {
                //        //LockObject.Release();
                //        //Snooze ();
                //        //LockObject.Acquire();
                //    }
                //    //ASSERT (isParsedTopAvailable);
                //}

                Parser.SetInputData(
                    SourceFileInfo,
                    this.LexDataBlock.TokenArray,
                    this.LexDataBlock.SourceText,
                    this.LexDataBlock.LineList);

                //----------------------------------------------------
                // Error Processing
                //----------------------------------------------------
                if (!this.IsParseTreeTopAvailable)
                {
                    // It's not done, so we need to do it.
                    DebugUtil.Assert(!CommitParsingTop);
                    CommitParsingTop = true;
                    SetParserThread();

                    try
                    {
                        if (NodeDictionary != null)
                        {
                            NodeDictionary.Clear();
                            NodeArray.Clear();
                        }

                        CErrorContainer newErrorContainer
                            = CErrorContainer.CreateInstance(ERRORCATEGORY.TOPLEVELPARSE, 0);

                        int oldErrorCount = this.parseErrorContainer.Count;
                        this.parseErrorContainer = newErrorContainer;
                        this.currentErrorContainer = this.parseErrorContainer;

                        // Release state lock while we parse in case another thread
                        // wants the token stream (for example)

                        // LockObject.Release();

                        try
                        {
                            ParseSourceModuleWorker();
                        }
                        catch (StackOverflowException)
                        {
                            POSDATA pos = Parser.CurrentTokenPos();
                            ErrorAtPosition(pos.LineIndex, pos.CharIndex, 0,
                                CSCERRID.FTL_StackOverflow,
                                new ErrArg(Parser.GetTokenText(Parser.CurrentTokenIndex())));
                            Controller.ReportErrors(currentErrorContainer);
                            ParseCrashed = true;
                            br = false;
                        }
                        catch (Exception e)
                        {
                            string msg = e.ToString();
                            ParseCrashed = true;
                            br = false;
                        }

                        // If necessary, fire the top-level-parse-errors-changed event
                        // before locking the state bits again
                        if (oldErrorCount + currentErrorContainer.Count > 0)
                        {
                            try
                            {
                                moduleEventSource.FireReportErrors(parseErrorContainer);
                            }
                            catch (Exception)
                            {
                                br = false;
                            }
                        }

                        // We're done parsing the top level, so reflect this in the state
                        //fLocked = LockObject.Acquire();
                        //ASSERT (fLocked);
                        currentErrorContainer = null;
                    }
                    finally
                    {
                        ClearParserThread();
                        CommitParsingTop = false;
                        IsParseTreeTopAvailable = br;
                    }
                }
            }
            // We're done with the state lock
            //LockObject.Release();

            ppNode = TreeTopNode;
            return (ppNode != null);    // ? E_FAIL : S_OK;
        }

        //------------------------------------------------------------
        // CSourceModule.GetErrors
        //------------------------------------------------------------
        override internal CErrorContainer GetErrors(CSourceData sourceData, ERRORCATEGORY category)
        {
            BASENODE topNode;
            // ParseTopLevel は isTokenized が true なら何もしない。
            if (!ParseTopLevel(sourceData, out topNode, false))
            {
                return null;
            }

            //CTinyGate   gate (&LockObject);

            if (category == ERRORCATEGORY.TOKENIZATION)
            {
                return TokenErrorContainer;
            }
            else if (category == ERRORCATEGORY.TOPLEVELPARSE)
            {
                return ParseErrorContainer;
            }
            return null;
        }

        //------------------------------------------------------------
        // CSourceModule.GetInteriorParseTree (override)
        //
        /// <summary></summary>
        /// <param name="pData"></param>
        /// <param name="pNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal CInteriorTree GetInteriorParseTree(
            CSourceData pData,
            BASENODE pNode)
        {
            //HRESULT     hr = S_OK;
            //BOOL        fLocked = LockObject.Acquire();

            //ASSERT (fLocked);       // Can't call this when the state is locked!

            // Can't parse interior node without having already done the top level!
            if (!IsParseTreeTopAvailable)
            {
                //if (fLocked)
                //    LockObject.Release();
                //return E_INVALIDARG;
                return null;
            }

            // We're done with the state lock for now
            //if (fLocked)
            //    LockObject.Release();

            //if (VSFSWITCH (gfUnhandledExceptions))
            //{
            //    // Create an interior tree object.  It will call back into us to do
            //    // the parse (if necessary; or grab the existing tree node if already
            //    // there).
            //    hr = CSourceModuleBase::GetInteriorParseTree(pData, pNode, ppTree);
            //}
            //else
            //{
            //    PAL_TRY
            //    {
            //        // Create an interior tree object.  It will call back into us to do
            //        // the parse (if necessary; or grab the existing tree node if already
            //        // there).
            //        hr = CSourceModuleBase::GetInteriorParseTree(pData, pNode, ppTree);
            //    }
            //    PAL_EXCEPT_FILTER(ExceptionFilter, this)
            //    {
            //        CLEANUP_STACK // handle stack overflow
            //        SourceModuleController->HandleException (GetExceptionCode());
            //        hr = E_FAIL;
            //    }
            //    PAL_ENDTRY
            //}

            lock (StateLock)
            {
                return base.GetInteriorParseTree(pData, pNode);
            }
        }

        //------------------------------------------------------------
        // CSourceModule.ParseForErrors
        //------------------------------------------------------------
        override internal int ParseForErrors(CSourceData pData)
        //HRESULT CSourceModule::ParseForErrors (CSourceData *pData)
        {
            throw new NotImplementedException("CSourceModule.ParseForErrors");

            //    HRESULT     hr;
            //    BASENODE    *pTopNode;
            //
            //    STARTTIMER ("parse for errors");
            //
            //    m_fParsingForErrors = TRUE;
            //
            //    // First, parse the top level.
            //    if (FAILED (hr = ParseTopLevel (pData, &pTopNode)))
            //    {
            //        STOPTIMER();
            //        m_fParsingForErrors = FALSE;
            //        return hr;
            //    }
            //
            //    // Now, iterate through the tree and parse all interior nodes.
            //
            //
            //    m_dwInteriorSize = 0;
            //    m_iInteriorTrees = 0;
            //
            //    hr = ParseInteriorsForErrors (pData, pTopNode);
            //
            //
            //    STOPTIMER();
            //    m_fParsingForErrors = FALSE;
            //    return hr;
        }

        //internal int FindLeafNodeEx(CSourceData pData, POSDATA pos, ExtentFlags flags, ref BASENODE ppNode, ref ICSInteriorTree ppTree);
        //int GetFirstToken(BASENODE pNode, ref bool pfMissingName);
        //int GetLastToken(BASENODE pNode, ref bool pfMissingName);
        //STATEMENTNODE GetLastStatement(STATEMENTNODE pStmt);

        //------------------------------------------------------------
        // CSourceModule.IsUpToDate
        //------------------------------------------------------------
        override internal int IsUpToDate(out bool pfTokenized, out bool pfTopParsed)
        //HRESULT CSourceModule::IsUpToDate (BOOL *pfTokenized, BOOL *pfTopParsed)
        {
            throw new NotImplementedException("CSourceModule.IsUpToDate");

            //    HRESULT     hr = ((!isTextModified) && isTokenized) ? S_OK : S_FALSE;
            //
            //    if (pfTokenized != NULL)
            //        *pfTokenized = isTokenized;
            //
            //    if (pfTopParsed != NULL)
            //        *pfTopParsed = isParsedTopAvailable;
            //
            //    return hr;
        }

        //------------------------------------------------------------
        // CSourceModule.GetInteriorNode
        //
        /// <summary></summary>
        /// <param name="srcData"></param>
        /// <param name="srcNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override CInteriorNode GetInteriorNode(CSourceData srcData, BASENODE srcNode)
        {
            CInteriorNode storedNode = null;

            lock (stateLockObject)
            {
                storedNode = GetInteriorNodeAddress(srcNode);

                if (storedNode != null)
                {
                    if ((srcNode.NodeFlagsEx & NODEFLAGS.EX_INTERIOR_PARSED) == 0)
                    {
                        MarkInteriorAsParsed(srcNode, true);
                        //m_StateLock.Release();
                        this.moduleEventSource.FireReportErrors(storedNode.ErrorContainer);
                    }
                    else
                    {
                        //m_StateLock.Release();
                    }
                    return storedNode;
                }

                storedNode = GetInteriorNodeWorker(srcNode);

                DebugUtil.Assert(this.currentErrorContainer == null);
                this.currentErrorContainer = storedNode.CreateErrorContainer();
                // m_StateLock.Release();
            }   // lock (stateLockObject)

            // #define VSFSWITCH(NAME) FALSE // palrt\inc\vsassert.h

            try
            {
                ParseInteriorWorker(srcNode);
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (CSException ex)
            {
                controller.HandleException(ex);
            }

            if (InParsingForErrors)
            {
                moduleEventSource.FireReportErrors(this.currentErrorContainer);
            }

            // m_StateLock.Acquire();
            lock (stateLockObject)
            {
                this.currentErrorContainer = null;
                this.IsParsingInterior = false;
                ClearInteriorParserThread();
                //m_StateLock.Release();
            }   // lock (stateLockObject)
            return storedNode;
        }

        //------------------------------------------------------------
        // CSourceModule::GetInteriorNode (sscli)
        //------------------------------------------------------------
        //HRESULT CSourceModule::GetInteriorNode (CSourceData *pData, BASENODE *pNode, CInteriorNode **ppNode)
        //{
        //    if (pNode == NULL) {
        //        return E_INVALIDARG;
        //    }
        //
        //    BOOL    fLocked;
        //
        //    // Re-obtain the state lock
        //    fLocked = m_StateLock.Acquire ();
        //
        //    ASSERT (fLocked);       // Can't call this while state lock is held
        //    ASSERT (m_fParsedTop);  // Must have already parsed the top (still)
        //
        //    // Wait until no other thread is parsing an interior node
        //    while (m_fParsingInterior)
        //    {
        //        ASSERT (m_dwInteriorParserThread != GetCurrentThreadId());
        //        m_StateLock.Release();
        //        Sleep (1);
        //        m_StateLock.Acquire();
        //    }
        //
        //    CInteriorNode   **ppStoredNode = GetInteriorNodeAddress(pNode);
        //    if (ppStoredNode == NULL)
        //    {
        //        VSFAIL ("Bogus node type passed to CSourceModule::GetInteriorNode()!");
        //        m_StateLock.Release();
        //        return E_FAIL;
        //    }
        //
        //
        //    HRESULT     hr = S_OK;
        //
        //    // Has this node been parsed already?  (If so, it will have an interior
        //    // node object...)
        //    if (*ppStoredNode != NULL)
        //    {
        //        // The node has been parsed.  Before releasing the state lock, addref the
        //        // interior node to protect it from final release below...
        //        *ppNode = *ppStoredNode;
        //        (*ppNode)->AddRef();
        //
        //        // Check to see if it has been marked as being parsed for errors.
        //        // If not, then we need to fire the event now (obviously the node has been
        //        // parsed, it just hasn't had errors reported yet.
        //        if ((pNode->other & NFEX_INTERIOR_PARSED) == 0)
        //        {
        //            // Mark the node as being parsed first, while the state is still 
        //            // locked.
        //            MarkInteriorAsParsed(pNode, true);
        //
        //            // Release the state lock before reporting the errors.   The errors
        //            // are safe because the interior node already has our ref.
        //            m_StateLock.Release();
        //            m_EventSource.FireReportErrors ((*ppStoredNode)->GetErrorContainer());
        //        }
        //        else
        //        {
        //            m_StateLock.Release();
        //        }
        //
        //        return S_OK;
        //    }
        //
        //    GetInteriorNodeWorker(ppStoredNode, pNode);
        //
        //    DWORD   dwSizeBase = 0;
        //    if (m_fAccumulateSize)
        //        dwSizeBase = (DWORD)m_pActiveTopLevelHeap->CalcCommittedSize();
        //
        //    // Have the interior node create an error container for itself
        //    ASSERT (m_pCurrentErrors == NULL);
        //    m_pCurrentErrors = (*ppStoredNode)->CreateErrorContainer();
        //
        //    // Do the interior parse, releasing the state lock first
        //    m_StateLock.Release();
        //    if (VSFSWITCH (gfUnhandledExceptions))
        //    {
        //        ParseInteriorWorker(pNode);
        //    }
        //    else
        //    {
        //        PAL_TRY
        //        {
        //            ParseInteriorWorker(pNode);
        //        }
        //        PAL_EXCEPT_FILTER(ExceptionFilter, this)
        //        {
        //            CLEANUP_STACK // handle stack overflow
        //            m_pController->HandleException (GetExceptionCode());
        //            hr = E_FAIL;
        //        }
        //        PAL_ENDTRY
        //    }
        //
        //    // Fire error reporting event before locking state bits.  Note that
        //    // we fire this event regardless of whether there were errors or not.
        //    // The host uses this event to clear other errors in cases of
        //    // incremental parsing.
        //    if (m_fParsingForErrors)
        //        m_EventSource.FireReportErrors (m_pCurrentErrors);
        //
        //    m_StateLock.Acquire();
        //    m_pCurrentErrors = NULL;
        //    m_pActiveHeap = NULL;
        //
        //    m_fParsingInterior = FALSE;
        //    ClearInteriorParserThread();
        //
        //    if (m_fAccumulateSize)
        //    {
        //        m_dwInteriorSize += (DWORD)(m_pActiveTopLevelHeap->CalcCommittedSize() - dwSizeBase);
        //        m_iInteriorTrees++;
        //    }
        //
        //    // NOTE:  The state lock is still acquired at this point.  The state lock
        //    // is used for serialization to the interior nodes' ref counts.
        //    *ppNode = *ppStoredNode;
        //    (*ppNode)->AddRef();
        //    m_StateLock.Release();
        //    return hr;
        //}

        //internal void ResetHeapBusyFlag() { isInteriorHeapBusy = false; }
        //internal void ResetTokenizedFlag() { isTokenized = false; }
        //internal void ResetTokenizingFlag() { commitTokenizing = false; }
        //internal bool ReportError(int iErrorId, ERRORKIND errKind, int iLine, int iCol, int iExtent, string pszText);
        //internal bool IsSymbolDefined(string symbol) { return preprocessorSymTable.IsDefined(symbol); }

        //------------------------------------------------------------
        // CSourceModule.GetLastRenamedTokenIndex
        //------------------------------------------------------------
        override internal int GetLastRenamedTokenIndex(out int piTokIdx, out string ppPreviousName)
        //HRESULT CSourceModule::GetLastRenamedTokenIndex(long *piTokIdx, NAME **ppPreviousName)
        {
            throw new NotImplementedException("");

            //    CTinyGate gate(&LockObject);
            //    *piTokIdx       = lastRenamedTokenIndex;
            //    *ppPreviousName = previousTokenName;
            //    return S_OK;
        }

        //------------------------------------------------------------
        // CSourceModule.ResetRenamedTokenData
        //------------------------------------------------------------
        override internal int ResetRenamedTokenData()
        //HRESULT CSourceModule::ResetRenamedTokenData()
        {
            throw new NotImplementedException("CSourceModule.ResetRenamedTokenData");

            //    CTinyGate gate(&LockObject);
            //    lastRenamedTokenIndex = -1;
            //    previousTokenName = NULL;
            //    return S_OK;
        }

        //internal bool hasChecksums() { return ChecksumList.Count() > 0; }
        //internal int CountChecksums() { return ChecksumList.Count(); }
        //internal ChecksumData GetChecksum(int index) { return ChecksumList.GetAt(index); }

        //internal void CloneSourceText(ref ICSSourceText ppClone);
        //internal void CloneLexData(string pszText, LEXDATABLOCK lex);

        // Parse diffs (CodeModel eventing)
        //internal int GenerateParseDiffs(BASENODE pOldTree, BASENODE pNewTree, ref EVENTNODE ppEventNode);
        //internal void FreeParseDiffQ();

        // IUnknown
        //STDMETHOD_(ULONG, AddRef)();
        //STDMETHOD_(ULONG, Release)();
        //STDMETHOD(QueryInterface)(REFIID riid, void **ppObj);

        // ICSSourceModule

        //------------------------------------------------------------
        // CSourceModule.GetSourceData
        //
        /// <summary>
        /// <para>If this is not used by a CSourceData instance, create CSourceData instance with this and return it</para>
        /// <para>If this is used by a CSourceData instance on the same thread, return null.</para>
        /// <para>If this is used by a CSourceData instance on the same thread, wait for this to be released.</para>
        /// </summary>
        /// <param name="blockForNewest"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CSourceData GetSourceData(bool blockForNewest)
        {
            lock (this.StateLock)
            {
                return CSourceData.CreateInstance(this);
            }
        }

        //------------------------------------------------------------
        // CSourceModule.GetSourceData
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CSourceText GetSourceText()
        {
            return SourceText;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceLexer::TokenMemAlloc
    //
    //void *CSourceLexer::TokenMemAlloc (CSTOKEN *pToken, size_t iSize)
    //{
    //    return m_pModule->TokenMemAlloc (pToken, iSize);
    //}
    //
    //
    //////////////////////////////////////////////////////////////////////////////////
    //
    //void *CSourceParser::MemAlloc (long iSize)
    //{
    //    return m_pModule->m_pActiveHeap->Alloc (iSize);
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::~CSourceModule
    //
    //CSourceModule::~CSourceModule ()
    //{
    //    BOOL    fLocked = LockObject.LockedByMe ();
    //
    //    InternalFlush ();
    //
    //    // If we were connected to the text, it would have a ref on us...
    //    ASSERT (EditCookieFromText == 0);
    //
    //    if (PerThreadDataRefCount)
    //        VSFree (PerThreadDataRefCount);
    //
    //    m_heap1.FreeHeap();
    //    m_heap2.FreeHeap();
    //
    //    if (TokenErrorContainer != NULL)
    //        TokenErrorContainer->Release();
    //
    //    if (ParseErrorContainer != NULL)
    //        ParseErrorContainer->Release();
    //
    //    if (fLocked)
    //        LockObject.Release();
    //
    //    if (SourceDirectory)
    //        m_pStdHeap->Free(SourceDirectory);
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::PrepareForCloning
    //
    //void CSourceModule::PrepareForCloning(ICSSourceData *pData)
    //{
    //    InternalCreateTokenStream(pData);
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::HandleExceptionFromTokenization
    //
    //LONG CSourceModule::HandleExceptionFromTokenization (EXCEPTION_POINTERS * exceptionInfo, PVOID pv)
    //{
    //    //  1) The original call that caused tokenization to occur returns failure, and
    //    //  2) The state of the source module is such that another attempt to tokenize
    //    //     won't just crash again (which may not be possible)
    //    CSourceModule* pThis = (CSourceModule *)pv;
    //
    //    pThis->ClearTokenizerThread();
    //    pThis->ResetTokenizedFlag();
    //    pThis->ResetTokenizingFlag();
    //
    //    return EXCEPTION_CONTINUE_SEARCH;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::RepresentNoiseTokens
    //
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::TokenMemAlloc
    //
    //void *CSourceModule::TokenMemAlloc (CSTOKEN *pToken, size_t iSize)
    //{
    //    // If this is the first tokenization, we can use the NR heap
    //    if (isFirstTokenization)
    //        return m_pActiveTopLevelHeap->Alloc (iSize);
    //
    //    // This is a special case we'll need to detect when removing this token later,
    //    // either via retokenization or teardown.
    //    pToken->iUserBits |= TF_HEAPALLOCATED;
    //    return m_pStdHeap->Alloc (iSize);
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::AddRef
    //
    //STDMETHODIMP_(ULONG) CSourceModule::AddRef ()
    //{
    //    return InternalAddRef();
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::Release
    //
    //STDMETHODIMP_(ULONG) CSourceModule::Release ()
    //{
    //    return InternalRelease();
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::QueryInterface
    //
    //STDMETHODIMP CSourceModule::QueryInterface (REFIID riid, void **ppObj)
    //{
    //    *ppObj = NULL;
    //
    //    if (riid == IID_IUnknown || riid == IID_ICSSourceModule)
    //    {
    //        *ppObj = (ICSSourceModule *)this;
    //        ((ICSSourceModule *)this)->AddRef();
    //        return S_OK;
    //    }
    //    else if (riid == IID_ICSSourceTextEvents)
    //    {
    //        *ppObj = (ICSSourceTextEvents *)this;
    //        ((ICSSourceTextEvents *)this)->AddRef();
    //        return S_OK;
    //    }
    //
    //    return E_NOINTERFACE;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::GetMemoryConsumption
    //
    //STDMETHODIMP CSourceModule::GetMemoryConsumption (long *piTopTree, long *piInteriorTrees, long *piInteriorNodes)
    //{
    //    // Calculate bytes consumed by top-level parse tree (assumes no interior tree exists)
    //    *piTopTree = (long)m_heap1.CalcCommittedSize() + (long)m_heap2.CalcCommittedSize();
    //
    //    // Return size consumed by interior nodes (assumes ParseForErrors() was called first)
    //    *piInteriorTrees = (long)m_dwInteriorSize;
    //
    //    // Return number of interior trees parsed
    //    *piInteriorNodes = m_iInteriorTrees;
    //    return S_OK;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::FindLeafNodeEx
    //
    //HRESULT CSourceModule::FindLeafNodeEx (CSourceData *pData, const POSDATA pos, ExtentFlags flags, BASENODE **ppNode, ICSInteriorTree **ppTree)
    //{
    //    HRESULT     hr = E_FAIL;
    //
    //    if (m_fLeafCrashed)
    //        return E_FAIL;
    //
    //    if (VSFSWITCH (gfUnhandledExceptions))
    //    {
    //        hr = CSourceModuleBase::FindLeafNodeEx(pData, pos, flags, ppNode, ppTree);
    //    }
    //    else
    //    {
    //        PAL_TRY
    //        {
    //            hr = CSourceModuleBase::FindLeafNodeEx(pData, pos, flags, ppNode, ppTree);
    //        }
    //        PAL_EXCEPT_FILTER(ExceptionFilter, this)
    //        {
    //            CLEANUP_STACK // handle stack overflow
    //            SourceModuleController->HandleException (GetExceptionCode());
    //            m_fLeafCrashed = TRUE;
    //            hr = E_FAIL;
    //        }
    //        PAL_ENDTRY
    //    }
    //
    //    return hr;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::GetSingleTokenPos
    //
    //HRESULT CSourceModule::GetSingleTokenPos (CSourceData *pData, long iToken, POSDATA *pposToken)
    //{
    //    CreateTokenStream (pData);
    //    return CSourceModuleBase::GetSingleTokenPos (pData, iToken, pposToken);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::GetSingleTokenData
    //
    //HRESULT CSourceModule::GetSingleTokenData (CSourceData *pData, long iToken, TOKENDATA *pTokenData)
    //{
    //    CreateTokenStream (pData);
    //    return CSourceModuleBase::GetSingleTokenData (pData, iToken, pTokenData);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::IsInsideComment
    //
    //HRESULT CSourceModule::IsInsideComment (CSourceData *pData, const POSDATA &pos, BOOL *pfInComment)
    //{ 
    //    HRESULT     hr = S_OK;
    //
    //    if (VSFSWITCH (gfUnhandledExceptions))
    //    {
    //        CreateTokenStream(pData);
    //        hr = CSourceModuleBase::IsInsideComment (pData, pos, pfInComment);
    //    }
    //    else
    //    {
    //        PAL_TRY
    //        {
    //            CreateTokenStream(pData);
    //            hr = CSourceModuleBase::IsInsideComment (pData, pos, pfInComment);
    //        }
    //        PAL_EXCEPT_FILTER(ExceptionFilter, this)
    //        {
    //            CLEANUP_STACK // handle stack overflow
    //            SourceModuleController->HandleException (GetExceptionCode());
    //            hr = E_FAIL;
    //        }
    //        PAL_ENDTRY
    //    }
    //
    //    return hr;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::ErrorAtPositionArgs
    //
    //
    //void *CSourceModule::AllocFromActiveHeap(size_t iSize)
    //{
    //    return m_pActiveTopLevelHeap->Alloc (iSize);
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::GetMemoryConsumptionPrivate
    //
    //HRESULT CSourceModule::GetMemoryConsumptionPrivate(long *piTopTree, long *piInteriorTrees, long *piInteriorNodes)
    //{
    //    return GetMemoryConsumption(piTopTree, piInteriorTrees, piInteriorNodes);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::CloneSourceText
    //
    //void CSourceModule::CloneSourceText (ICSSourceText **ppClone)
    //{
    //    if (FAILED (SourceText->CloneInMemory (ppClone)))
    //        SourceModuleController->NoMemory();
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModule::CloneLexData
    //
    //void CSourceModule::CloneLexData(MEMHEAP *pStdHeap, PCWSTR pszText, LEXDATABLOCK &lex)
    //{
    //    LexDataBlock.CloneTo(SourceModuleController, pStdHeap, pszText, lex);
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// ExceptionFilter
    //LONG CSourceModule::ExceptionFilter (EXCEPTION_POINTERS *pExceptionInfo, PVOID pv)
    //{
    //    CSourceModule* pThis = (CSourceModule *)pv;
    //
    //    WatsonOperationKindEnum howToReportWatsons = WatsonOperationKind::Queue;
    //    WCHAR bugreport[MAX_PATH];
    //    bugreport[0] = L'\0';
    //    if (pThis->SourceModuleController)
    //    {
    //        pThis->SourceModuleController->SetExceptionData (pExceptionInfo);
    //        howToReportWatsons = pThis->SourceModuleController->GetWatsonFlags();
    //        CComPtr<ICSCommandLineCompilerHost> cmdHost;
    //        if (SUCCEEDED(pThis->SourceModuleController->GetHost()->QueryInterface(IID_ICSCommandLineCompilerHost, (void**)&cmdHost)) && cmdHost) {
    //            if (FAILED(cmdHost->GetBugReportFileName(bugreport, lengthof(bugreport))))
    //                bugreport[0] = L'\0';
    //        }
    //    }
    //
    //    return EXCEPTION_EXECUTE_HANDLER;
    //}
    //
    //
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::~CSourceModuleBase
    //
    //CSourceModuleBase::~CSourceModuleBase()
    //{
    //    InternalFlush();
    //
    //    SourceText.Release();
    //
    //    if (LexDataBlock.piTransitionLines != NULL)
    //        VSFree (LexDataBlock.piTransitionLines);
    //
    //    if (LexDataBlock.pCCStates != NULL)
    //        VSFree (LexDataBlock.pCCStates);
    //
    //    if (LexDataBlock.pIdentTable)
    //        delete LexDataBlock.pIdentTable;
    //
    //    if (LexDataBlock.pWarningMap)
    //    {
    //        LexDataBlock.pWarningMap->Clear();
    //        delete LexDataBlock.pWarningMap;
    //    }
    //
    //    preprocessorSymTable.ClearAll (TRUE);
    //
    //    if (NodeTable)
    //        delete NodeTable;
    //
    //    if (NodeArray)
    //        delete NodeArray;
    //
    //    delete m_pParser;
    //
    //    SourceModuleController->Release();
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::InternalAddRef  使用しない。
    //
    //ULONG CSourceModuleBase::InternalAddRef ()
    //{
    //    return InterlockedIncrement (&m_iRef);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::InternalRelease 使用しない。
    //
    //ULONG CSourceModuleBase::InternalRelease ()
    //{
    //    ULONG   iNew = InterlockedDecrement (&m_iRef);
    //    if (iNew == 0)
    //        FinalRelease();
    //
    //    return iNew;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::FinalRelease    使用しない。
    //
    //void CSourceModuleBase::FinalRelease()
    //{
    //    delete this;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetDocCommentText
    ////
    //// Allocates memory from pStdHeap for the heap.  Free it yourself
    //// or call FreeDocCommentText.  Sets *ppszText = NULL if there is
    //// no DocComment
    //
    //HRESULT CSourceModuleBase::GetDocCommentText(BASENODE *pNode, __deref_out PWSTR *ppszText, long *piFirstComment, long * piLastComment, CXMLMap * srcMap, long * piLines, PCWSTR pszIndent)
    //{
    //    HRESULT     hr = S_OK;
    //    long        iStart = 0, iLen = 0, i, iBlockStart, x, iPrevLine;
    //    size_t      iTextLen = 0;
    //    int       * iRemoveChars = NULL;
    //    int         iAddChars = 2; // for the "\r\n"
    //    LEXDATA   * lexComments = NULL;
    //    ICSLexer ** lex = NULL;
    //    int         lineCount;
    //    WCHAR     * p = NULL;
    //    size_t      cchLeft;
    //    size_t      len;
    //
    //    ASSERT (ppszText != NULL);
    //    *ppszText = NULL;
    //
    //    // E_FAIL means no doc comment, other FAILED(hr)'s mean something bad.
    //    if (FAILED (hr = GetDocComment (pNode, &iStart, &iLen))) 
    //        return (hr == E_FAIL) ? S_FALSE : hr;
    //
    //    if (piLastComment)
    //        *piLastComment = iStart + (iLen - 1);
    //
    //    if (pszIndent != NULL)
    //        iAddChars += (int)wcslen(pszIndent);        
    //
    //    iRemoveChars = (int*)m_pStdHeap->Alloc(sizeof(int) * iLen);
    //    lexComments = (LEXDATA*)m_pStdHeap->Alloc(sizeof(LEXDATA) * iLen);
    //    lex = (ICSLexer**)m_pStdHeap->AllocZero(sizeof(ICSLexer*) * iLen);
    //
    //    // Scan each comment to see if we can remove the first whitespace
    //    // Work back-wards so we can detect any intervening whitespace
    //    iBlockStart = x = (iLen - 1);
    //    iPrevLine = -1;
    //    for (i = iStart + x; i >= iStart; i--, x--)
    //    {
    //        long iEndLine;
    //        PCWSTR pszLine = LexDataBlock.TokenAt(i).DocLiteral()->szText;
    //        if (LexDataBlock.TokenAt(i).Token() == TID_MLDOCCOMMENT) {
    //            if (FAILED(hr = CTextLexer::CreateInstance(GetNameMgr(), GetOptions()->compatMode, &lex[x])))
    //                goto CLEANUP;
    //            if (FAILED(hr = lex[x]->SetInput(LexDataBlock.TokenAt(i).DocLiteral()->szText, -1)))
    //                goto CLEANUP;
    //            if (FAILED(hr = lex[x]->GetLexResults(&lexComments[x])))
    //                goto CLEANUP;
    //            iEndLine = lexComments[x].iLines + LexDataBlock.TokenAt(i).iLine - 1;
    //        } else {
    //            ASSERT(LexDataBlock.TokenAt(i).Token() == TID_DOCCOMMENT);
    //            iEndLine = LexDataBlock.TokenAt(i).iLine;
    //            memset (&lexComments[x], 0, sizeof(lexComments[x]));
    //        }
    //
    //        if (iPrevLine != -1 && 
    //            iPrevLine != iEndLine && (iPrevLine - 1) != iEndLine) {
    //            // This doc comment starts on a non-contigous line, so stop here
    //            i++;
    //            x++;
    //            iLen -= i - iStart;
    //            iStart = i;
    //            memmove(iRemoveChars, iRemoveChars + x, sizeof(*iRemoveChars) * iLen);
    //            memmove(lexComments, lexComments + x, sizeof(*lexComments) * iLen);
    //            memmove(lex, lex + x, sizeof(*lex) * iLen);
    //            break;
    //        } else {
    //            iPrevLine = LexDataBlock.TokenAt(i).iLine;
    //        }
    //
    //        if (LexDataBlock.TokenAt(i).Token() == TID_DOCCOMMENT) {
    //            // Single-line comment
    //            if (x == iBlockStart || iRemoveChars[x+1] == 4)
    //            {
    //                if (IsWhitespace (pszLine[3]))
    //                    iRemoveChars[x] = 4;
    //                else
    //                {
    //                    // This comment doesn't lead with whitespace
    //                    // So we can't strip anything from this whole block
    //                    for (int y = iBlockStart; y >= x; y--) {
    //                        ASSERT(x == y || iRemoveChars[y] == 4);
    //                        iTextLen++;
    //                        iRemoveChars[y] = 3;
    //                    }
    //                }
    //            } else
    //                iRemoveChars[x] = 3;
    //            iTextLen += (iAddChars - iRemoveChars[x]) + LexDataBlock.TokenAt(i).Length();
    //        } else {
    //            // Multi-line comment
    //            ASSERT(LexDataBlock.TokenAt(i).Token() == TID_MLDOCCOMMENT && lexComments[x].pszSource != NULL);
    //            // The next single-line comment is the start of a new 'block' of comments
    //            iBlockStart = x - 1;
    //
    //            // If a multi-line doc comment is all on one line, treat it just like a
    //            // a single line comment (except it does break the block of single-line comments)
    //            if (lexComments[x].iLines == 1) {
    //                if (IsWhitespace (pszLine[3]))
    //                    iRemoveChars[x] = 4;
    //                else
    //                    iRemoveChars[x] = 3;
    //                iTextLen += (iAddChars - iRemoveChars[x]) + LexDataBlock.TokenAt(i).Length();
    //            } else {
    //                // Guess how many characters (but make sure it's always bigger
    //                iTextLen += iAddChars * (lexComments[x].iLines) +
    //                    wcslen(LexDataBlock.TokenAt(i).DocLiteral()->szText);
    //                iRemoveChars[x] = 0;
    //            }
    //        }
    //    }
    //
    //    if (piFirstComment)
    //        *piFirstComment = iStart;
    //
    //    iAddChars -= 2; // for the "/r/n"
    //
    //    // Allocate and copy in the text
    //    ++iTextLen; // Add one for NULL.
    //    *ppszText = (PWSTR)m_pStdHeap->Alloc(sizeof(WCHAR) * (iTextLen + 1)); // Add one for assert checking below
    //    p = *ppszText;
    //    *p = L'\0';
    //
    //    cchLeft = iTextLen + 1; // always contains the amount of buffer left after p.
    //    len = 0;
    //    x = 0;
    //    *p = L'\0';
    //    
    //    // All the code that copies into *ppszText has been updated to use the "safe string" functions
    //    // to ensure that a buffer overflow can't occur, even if there is a logic error somewhere and
    //    // we didn't allocate enough memory after all (e.g., the calculation of iTextLen was somehow
    //    // flawed.) The equivalent code is commented out immediately above.
    //    lineCount = 0;
    //    for (i = iStart; i < (iStart + iLen); i++, x++)
    //    {
    //        PCWSTR pszLine = LexDataBlock.TokenAt(i).DocLiteral()->szText;
    //        LexDataBlock.TokenAt(i).iUserBits |= TF_USEDCOMMENT;
    //        if (iRemoveChars[x] != 0)
    //        {
    //            ASSERT(iRemoveChars[x] == 3 || iRemoveChars[x] == 4);
    //            len = (long)LexDataBlock.TokenAt(i).Length() - iRemoveChars[x];
    //            if (pszLine[1] == '*')
    //                len -= 2; // Don't copy the trailing "*/"
    //
    //            //wcscpy (p, pszIndent);
    //            //p += iAddChars;
    //            if (pszIndent)
    //                StringCchCopyExW(p, cchLeft, pszIndent, &p, &cchLeft, 0);
    //
    //            //wcsncpy (p, LexDataBlock.pTokens[i].DocLiteral()->szText + iRemoveChars[x], len);
    //            //p += len;
    //            StringCchCopyNExW(p, cchLeft, LexDataBlock.TokenAt(i).DocLiteral()->szText + iRemoveChars[x], len, &p, &cchLeft, 0);
    //            
    //            //*p++ = '\r';
    //            //*p++ = '\n';
    //            StringCchCopyExW(p, cchLeft, L"\r\n", &p, &cchLeft, 0);
    //
    //            if (srcMap != NULL) {
    //                srcMap->AddMap(lineCount, LexDataBlock.TokenAt(i).iLine, LexDataBlock.TokenAt(i).iChar + iRemoveChars[x] - iAddChars);
    //            }
    //
    //            ++lineCount;
    //        }
    //        else
    //        {
    //            PCWSTR pszPattern = NULL;
    //            long cntPattern = 0;
    //            bool bSkipFirst = false;
    //            unsigned long j;
    //            unsigned long last_line = (lexComments[x].iLines - 1);
    //
    //            // If the first line has nothing but whitespace, then skip it entirely
    //            if (lexComments[x].IsLineWhitespaceAfter( 0, 3))
    //                bSkipFirst = true;
    //
    //            // If the last line has nothing but whitespace, then skip it entirely
    //            if (lexComments[x].IsLineWhitespaceBefore(last_line, lexComments[x].GetLineLength(last_line) - 2)) {
    //                // The whole last line is whitespace, so ignore it (except of course the trailing "*/"
    //                last_line--;
    //            }
    //
    //            // Find the 'pattern' if one exists and then strip it.
    //            // to find the pattern, find a common "whitespace*whitespace" on
    //            // every line except the first and maybe the last
    //            j = 1;
    //            if (j <= last_line) {
    //                // setup the 'pattern'
    //                HRESULT hr = FindDocCommentPattern( &lexComments[x], 1, &cntPattern, &pszPattern);
    //                if (hr == S_OK) {
    //                    // found a 'pattern', so match it on each line
    //                    while (++j <= last_line)
    //                    {
    //                        hr = MatchDocCommentPattern( lexComments[x].TextAt(j, 0), pszPattern, &cntPattern);
    //                        if (hr == E_FAIL) {
    //                            // No matchable pattern (or sub-pattern)
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //
    //            // cntPattern now holds the count of common characters that we can safely
    //            // strip from all lines except the first
    //
    //            // Do the first line
    //            if (!bSkipFirst) {
    //                pszLine = lexComments[x].TextAt(0, 3);
    //                if (IsWhitespaceChar(*pszLine))
    //                    pszLine++;
    //                len = lexComments[x].TextAt(1,0) - pszLine;
    //                
    //                //wcscpy (p, pszIndent);
    //                //p += iAddChars;
    //                if (pszIndent)
    //                    StringCchCopyExW(p, cchLeft, pszIndent, &p, &cchLeft, 0);
    //                
    //                //wcsncpy (p, pszLine, len);
    //                //p += len;
    //                StringCchCopyNExW(p, cchLeft, pszLine, len, &p, &cchLeft, 0);
    //
    //                if (srcMap != NULL) {
    //                    srcMap->AddMap(lineCount, LexDataBlock.TokenAt(i).iLine, (long)(LexDataBlock.TokenAt(i).iChar + (pszLine - lexComments[x].TextAt(0, 0)) - iAddChars));
    //                }
    //
    //                ++lineCount;
    //            }
    //            
    //            // Do all the 'middle' lines
    //            for (j = 1; j < (unsigned long)(lexComments[x].iLines - 1); j++)
    //            {
    //                pszLine = lexComments[x].TextAt(j, cntPattern);
    //                len = lexComments[x].TextAt(j+1, 0) - pszLine;
    //                
    //                //wcscpy (p, pszIndent);
    //                //p += iAddChars;
    //                if (pszIndent)
    //                    StringCchCopyExW(p, cchLeft, pszIndent, &p, &cchLeft, 0);
    //                
    //                //wcsncpy (p, pszLine, len);
    //                //p += len;
    //                StringCchCopyNExW(p, cchLeft, pszLine, len, &p, &cchLeft, 0);
    //
    //                if (srcMap != NULL) {
    //                    srcMap->AddMap(lineCount, LexDataBlock.TokenAt(i).iLine + j, cntPattern - iAddChars);
    //                }
    //
    //                ++lineCount;
    //            }
    //
    //            // Do the last line (same as middle line except how it ends)
    //            // unless it's all whitespace and can be skipped
    //            if (last_line == (unsigned)(lexComments[x].iLines - 1)) {
    //                pszLine = lexComments[x].TextAt(last_line, cntPattern);
    //                // Remove the '*' and '/' from the length
    //                len = lexComments[x].GetLineLength(last_line) - (2 + cntPattern);
    //                
    //                //wcscpy (p, pszIndent);
    //                //p += iAddChars;
    //                if (pszIndent)
    //                    StringCchCopyExW(p, cchLeft, pszIndent, &p, &cchLeft, 0);
    //                
    //                //wcsncpy (p, pszLine, len);
    //                //p += len;
    //                StringCchCopyNExW(p, cchLeft, pszLine, len, &p, &cchLeft, 0);
    //                
    //                //*p++ = L'\r';
    //                //*p++ = L'\n';
    //                StringCchCopyExW(p, cchLeft, L"\r\n", &p, &cchLeft, 0);
    //
    //                if (srcMap != NULL) {
    //                    srcMap->AddMap(lineCount, LexDataBlock.TokenAt(i).iLine + j, cntPattern - iAddChars);
    //                }
    //
    //                ++lineCount;
    //            }
    //
    //        }
    //
    //    }
    //    //*p = L'\0';
    //    ASSERT((p - *ppszText) + cchLeft == iTextLen + 1);  // Make sure that cchLeft was updated correctly throughout.
    //    ASSERT(cchLeft > 1);
    //    ASSERT(srcMap == NULL || srcMap->Count() == lineCount);
    //
    //    if (piLines)
    //        *piLines = lineCount;
    //
    //CLEANUP:
    //    ASSERT(iRemoveChars != NULL);
    //    m_pStdHeap->Free(iRemoveChars);
    //    ASSERT(lexComments != NULL);
    //    m_pStdHeap->Free(lexComments);
    //    ASSERT(lex != NULL);
    //    for (x = 0; x < iLen; x++) {
    //        if (lex[x] != NULL)
    //            lex[x]->Release();
    //    }
    //    m_pStdHeap->Free(lex);
    //
    //    return S_OK;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::FreeDocCommentText
    ////      Frees memory allocated in GetDocCommentText() from pStdHeap
    //
    //HRESULT CSourceModuleBase::FreeDocCommentText(__deref_inout PWSTR *ppszText)
    //{
    //    ASSERT(ppszText != NULL);
    //    if (*ppszText != NULL)
    //        m_pStdHeap->Free(*ppszText);
    //    *ppszText = NULL;
    //    return S_OK;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetSingleTokenData
    //
    //HRESULT CSourceModuleBase::GetSingleTokenData (CSourceData *pData, long iToken, TOKENDATA *pTokenData) 
    //{    
    //    if (iToken < 0 || iToken >= LexDataBlock.TokenCount())
    //        return E_INVALIDARG;
    //
    //    CSTOKEN &rToken = LexDataBlock.TokenAt(iToken);
    //    pTokenData->dwFlags = rToken.iUserBits;
    //    pTokenData->iLength = rToken.Length();
    //    pTokenData->iToken = rToken.Token();
    //    pTokenData->pName = (rToken.Token() == TID_IDENTIFIER) ? rToken.Name() : NULL;
    //    pTokenData->posTokenStart = rToken;
    //    pTokenData->posTokenStop = rToken.StopPosition();
    //    return S_OK;;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetSingleTokenPos
    //
    //HRESULT CSourceModuleBase::GetSingleTokenPos (CSourceData *pData, long iToken, POSDATA *pposToken)
    //{
    //    if (iToken < 0 || iToken >= LexDataBlock.TokenCount())
    //        return E_INVALIDARG;
    //
    //    (*pposToken) = LexDataBlock.TokenAt(iToken);
    //    return S_OK;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetTokenText
    //
    //HRESULT CSourceModuleBase::GetTokenText (long iTokenId, PCWSTR *ppszText, long *piLen)
    //{
    //    if (piLen != NULL)
    //        *piLen = 0;
    //    if (ppszText != NULL)
    //        *ppszText = L"";
    //
    //    if (iTokenId < 0 || iTokenId >= TID_NUMTOKENS)
    //        return E_INVALIDARG;
    //
    //    // If the stored length is 0, it's unknown.  Caller must use GetSingleTokenData
    //    // on a specific token index to get the desired data
    //    if (CParser::GetTokenInfo ((TOKENID)iTokenId)->iLen == 0)
    //        return S_FALSE;
    //
    //    if (piLen != NULL)
    //        *piLen = CParser::GetTokenInfo ((TOKENID)iTokenId)->iLen;
    //    if (ppszText != NULL)
    //        *ppszText = CParser::GetTokenInfo ((TOKENID)iTokenId)->pszText;
    //
    //    return S_OK;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::LookupNode
    //
    //HRESULT CSourceModuleBase::LookupNode (CSourceData *pData, NAME *pKey, long iOrdinal, BASENODE **ppNode, long *piGlobalOrdinal)
    //{
    //    HRESULT hr;
    //    BASENODE *pTopNode;
    //    if (FAILED (hr = ParseTopLevel(pData, &pTopNode)))
    //    {
    //        return hr;
    //    }
    //
    //    CTinyGate   gate (&LockObject);
    //
    //    if (NodeTable == NULL)
    //        return E_FAIL;      // Not available -- must create compiler with CCF_KEEPNODETABLES
    //
    //    // First, the easier check -- global-orginal value.
    //    if (pKey == NULL)
    //    {
    //        if (iOrdinal < 0 || iOrdinal >= NodeArray->Count())
    //            return E_INVALIDARG;
    //
    //        if (piGlobalOrdinal != NULL)
    //            *piGlobalOrdinal = iOrdinal;        // Well, duh...
    //
    //        *ppNode = (*NodeArray)[iOrdinal].pNode;
    //        return S_OK;
    //    }
    //
    //    // Okay, we have a key.  Find a node chain in the table.
    //    NODECHAIN   *pChain = NodeTable->Find (pKey);
    //
    //    // Pay respect to the key-ordinal
    //    while (pChain != NULL && iOrdinal--)
    //    {
    //        if (pChain != NULL)
    //            pChain = pChain->pNext;
    //    }
    //
    //    if (pChain != NULL)
    //    {
    //        *ppNode = (*NodeArray)[pChain->iGlobalOrdinal].pNode;
    //        if (piGlobalOrdinal != NULL)
    //            *piGlobalOrdinal = pChain->iGlobalOrdinal;
    //        return S_OK;
    //    }
    //
    //    // Node not found...
    //    return E_FAIL;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetNodeKeyOrdinal
    //
    //HRESULT CSourceModuleBase::GetNodeKeyOrdinal (CSourceData *pData, BASENODE *pNode, NAME **ppKey, long *piKeyOrdinal)
    //{
    //    CTinyGate   gate (&LockObject);
    //
    //    if (NodeTable == NULL)
    //        return E_FAIL;      // Not available -- must create compiler with CCF_KEEPNODETABLES
    //
    //    NAME    *pKey;
    //    HRESULT hr;
    //
    //    // First, build the node key and look up the node chain in the table
    //    if (FAILED (hr = pNode->BuildKey (GetNameMgr(), TRUE, true /* we want the <,,,> */, &pKey)))
    //        return hr;
    //
    //    long        iKeyOrd = 0;
    //
    //    // Find the given node in the chain
    //    NODECHAIN *pChain;
    //    for (pChain = NodeTable->Find (pKey); pChain != NULL; pChain = pChain->pNext)
    //    {
    //        if (pNode == (*NodeArray)[pChain->iGlobalOrdinal].pNode)
    //            break;
    //        iKeyOrd++;
    //    }
    //
    //    if (pChain == NULL)
    //        return E_FAIL;      // Huh?  Maybe not a keyed node, but then BuildKey would have failed!
    //
    //    if (piKeyOrdinal != NULL)
    //        *piKeyOrdinal = iKeyOrd;
    //    if (ppKey != NULL)
    //        *ppKey = pKey;
    //
    //    return S_OK;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetGlobalKeyArray
    //
    //HRESULT CSourceModuleBase::GetGlobalKeyArray (CSourceData *pData, KEYEDNODE *pKeyedNodes, long iSize, long *piCopied)
    //{
    //    CTinyGate   gate (&LockObject);
    //
    //    if (NodeTable == NULL)
    //        return E_FAIL;      // Not available -- must create compiler with CCF_KEEPNODETABLES
    //
    //    // Caller just wants the size?
    //    if (pKeyedNodes == NULL)
    //    {
    //        if (piCopied != NULL)
    //            *piCopied = NodeArray->Count();
    //        return S_OK;
    //    }
    //
    //    // Copy over the amount requested, or the total number of nodes, whichever
    //    // is smaller...
    //    long    iCopy = min (iSize, NodeArray->Count());
    //
    //    memcpy (pKeyedNodes, NodeArray->Base(), iCopy * sizeof (KEYEDNODE));
    //    if (piCopied != NULL)
    //        *piCopied = iCopy;
    //
    //    return iCopy == NodeArray->Count() ? S_OK : S_FALSE;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::IsInsideComment
    //
    //HRESULT CSourceModuleBase::IsInsideComment (CSourceData *pData, const POSDATA &pos, BOOL *pfInComment)
    //{
    //    *pfInComment = FALSE;
    //
    //    // Find the nearest token
    //    long    i = LexDataBlock.FindNearestPosition(pos);
    //
    //    if (i < 0)
    //    {
    //        *pfInComment = FALSE;
    //        return S_OK;
    //
    //    }
    //
    //    CSTOKEN &tk = LexDataBlock.TokenAt(i);
    //    if (tk == pos || !tk.IsComment())
    //    {
    //        // We're either not in the comment token, or it's not a comment token
    //        *pfInComment = FALSE;
    //    }
    //    else
    //    {
    //        // It's a comment token, but we may be beyond it.
    //        if (tk.Token() == TID_MLCOMMENT || tk.Token() == TID_MLDOCCOMMENT)
    //        {
    //            *pfInComment = (tk.StopPosition() > pos);
    //        }
    //        else
    //        {
    //            *pfInComment = (tk.iLine == pos.iLine);
    //        }
    //    }
    //
    //    return S_OK;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::FindDocCommentPattern
    ////
    //// iLine - Pass in the line to look at
    //// piChar - Pass out the character index of the '*'
    //// piPatternCnt - Pass out the length of the pattern
    //// pszPattern - Pass out the pattern string (assumed to be cchPattern buffer size)
    //// Return S_OK if a pattern was found, or S_FALSE if no pattern
    //
    //HRESULT CSourceModuleBase::FindDocCommentPattern (LEXDATA *pLex, unsigned long iLine, long *piPatternCnt, PCWSTR * pszPattern)
    //{
    //    long iChar = 0;
    //    HRESULT hr = pLex->FindFirstNonWhiteChar(iLine, &iChar); 
    //    // Keep iChar as the position of the star within the pattern
    //    if (hr == S_OK) {
    //        PCWSTR p = pLex->pszSource + pLex->piLines[iLine] + iChar;
    //        if (*p != L'*') {
    //            // no pattern on the first line
    //            goto NO_PATTERN;
    //        }
    //        PCWSTR pend = pLex->pszSource + pLex->piLines[iLine] + pLex->GetLineLength(iLine);
    //        p++;
    //        while (p < pend && IsWhitespaceChar(*p))
    //            p++;
    //        *piPatternCnt = (long)(p - (pLex->pszSource + pLex->piLines[iLine]));
    //        *pszPattern = pLex->pszSource + pLex->piLines[iLine];
    //        return S_OK;
    //    } else {
    //        // an empty line (or all whitespace)
    //NO_PATTERN:
    //        *piPatternCnt = 0;
    //        *pszPattern = L"";
    //        return S_FALSE;
    //    }
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::MatchDocCommentPattern
    ////
    //// pszLine - Pass in the comment line to check
    //// pszPattern - Pass in the pattern string
    //// piPatternCnt - Pass in/out the length of the pattern (it might change)
    //// Return S_OK if the full pattern was found,
    //// or S_FALSE if a smaller pattern was found (must include '*')
    //// or E_FAIL if not pattern was found
    //
    //HRESULT CSourceModuleBase::MatchDocCommentPattern (PCWSTR pszLine, PCWSTR pszPattern, long *piPatternCnt)
    //{
    //    const long len = *piPatternCnt;
    //
    //    if (wcsncmp(pszLine, pszPattern, len) == 0)
    //        return S_OK;
    //
    //    long k;
    //    bool bSeenStar = false;
    //    for (k = 0; pszLine[k] == pszPattern[k] && k < len; k++) {
    //        if (pszLine[k] == L'*')
    //            bSeenStar = true;
    //    }
    //    if (bSeenStar == true) {
    //        *piPatternCnt = k;
    //        return S_FALSE;
    //    } else {
    //        *piPatternCnt = 0;
    //        return E_FAIL;
    //    }
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetDocCommentXML
    //
    //HRESULT CSourceModuleBase::GetDocCommentXML (BASENODE *pNode, BSTR *pbstrDoc)
    //{
    //    HRESULT hr;
    //    PWSTR wszText = NULL;
    //
    //    if (FAILED(hr = this->GetDocCommentText (pNode, &wszText)))
    //        return hr;
    //
    //    // hr = S_FALSE is there is no doc comment
    //    CStringBuilder sb;
    //    if (hr != S_FALSE)
    //        sb.Append(wszText);
    //    hr = this->FreeDocCommentText(&wszText);
    //    ASSERT(SUCCEEDED(hr));
    //    return sb.CreateBSTR (pbstrDoc);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::FindLeafNode
    //
    //HRESULT CSourceModuleBase::FindLeafNode (CSourceData *pData, const POSDATA pos, BASENODE **ppNode, ICSInteriorTree **ppTree)
    //{
    //    return FindLeafNodeEx(pData, pos, EF_FULL, ppNode, ppTree);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::FindLeafNodeEx
    //
    //HRESULT CSourceModuleBase::FindLeafNodeEx (CSourceData *pData, const POSDATA pos, ExtentFlags flags, BASENODE **ppNode, ICSInteriorTree **ppTree)
    //{
    //    HRESULT hr;
    //
    //    BASENODE    *pTopNode;
    //
    //    if (ppTree != NULL)
    //        *ppTree = NULL;
    //
    //    *ppNode = NULL;
    //
    //    if (SUCCEEDED (hr = ParseTopLevel (pData, &pTopNode)))
    //    {
    //        // Search this node for the "lowest" containing the given position
    //        CBasenodeLookupCache cache;
    //        BASENODE    *pLeaf = m_pParser->FindLeafEx (cache, pos, pTopNode, ppTree == NULL ? NULL : pData, flags, ppTree);
    //
    //        *ppNode = pLeaf;
    //        hr = pLeaf ? S_OK : E_FAIL;
    //    }
    //
    //    return hr;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::FindLeafNodeForToken
    //
    //HRESULT CSourceModuleBase::FindLeafNodeForToken (CSourceData *pData, long iToken, BASENODE **ppNode, ICSInteriorTree **ppTree)
    //{
    //    return FindLeafNodeForTokenEx(pData, iToken, EF_FULL, ppNode, ppTree);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::FindLeafNodeForTokenEx
    //
    //HRESULT CSourceModuleBase::FindLeafNodeForTokenEx (CSourceData *pData, long iToken, ExtentFlags flags, BASENODE **ppNode, ICSInteriorTree **ppTree)
    //{
    //    HRESULT hr;
    //    BASENODE* pTopNode;
    //    if (FAILED (hr = ParseTopLevel (pData, &pTopNode)))
    //    {
    //        return hr;
    //    }
    //
    //    return m_pParser->FindLeafNodeForTokenEx(iToken, pTopNode, pData, flags, ppNode, ppTree);
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::GetChecksum
    //// Get the MD5 hash for PDB check sum
    //
    //HRESULT CSourceModuleBase::GetChecksum(Checksum * checksum)
    //{
    //    HRESULT hr = E_NOTIMPL;
    //    return hr;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CSourceModuleBase::MapSourceLine
    //
    //HRESULT CSourceModuleBase::MapSourceLine(CSourceData *pData, long iLine, long *piMappedLine, PCWSTR *ppszFilename, BOOL *pbIsHidden)
    //{    
    //    POSDATA pos;
    //    bool    fIsHidden;
    //    bool    fIsMapped;
    //
    //    pos.iLine = iLine;
    //    pos.iChar = 0;
    //
    //    MapLocation(&pos, ppszFilename, &fIsHidden, &fIsMapped);
    //
    //    if (piMappedLine)
    //        *piMappedLine = pos.iLine;
    //    
    //    if (pbIsHidden)
    //        *pbIsHidden = (fIsHidden == true);
    //
    //    // S_OK indicates the line is mapped, S_FALSE indicates it's the actual source line
    //    return fIsMapped ? S_OK : S_FALSE;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //
    //
    //////////////////////////////////////////////////////////////////////////////////
    //
    //class CHeapTokenAllocator : internal ITokenAllocator
    //{
    //internal:
    //    CHeapTokenAllocator(MEMHEAP *pStdHeap) : m_pStdHeap(pStdHeap) {}
    //    void * AllocateMemory(size_t size) { return m_pStdHeap->Alloc(size); }
    //    void Free(void *p) { m_pStdHeap->Free(p); }
    //
    //private:
    //    MEMHEAP *m_pStdHeap;
    //};
    //
    //////////////////////////////////////////////////////////////////////////////////

}
