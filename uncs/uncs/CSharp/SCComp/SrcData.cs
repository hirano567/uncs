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
// File: srcdata.h
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
// File: srcdata.cpp
//
// ===========================================================================

//============================================================================
// SrcData.cs
//
// 2015/03/16
//============================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // CSourceData
    //
    /// <summary>
    /// <para>This class has only one field which refers to a CSourceModuleBase instance.</para>
    /// <para>The constructor is private. Call CreateInstance method to create an instance.</para>
    /// </summary>
    //======================================================================
    internal class CSourceData
    {
        //------------------------------------------------------------
        // CSourceData Fields and Properties
        //------------------------------------------------------------
        private CSourceModuleBase sourceModuleBase = null;  // CSourceModuleBasePtr m_spModule;

        /// <summary>
        /// (R) Return sourceModuleBase.
        /// </summary>
        internal CSourceModuleBase Module
        {
            get { return sourceModuleBase; }    // CSourceModuleBase * GetModule()
        }

        /// <summary>
        /// (R) Return sourceModuleBase.SourceModule.
        /// </summary>
        internal CSourceModule SourceModule
        {
            get // GetSourceModule (ICSSourceModule **ppModule)
            {
                return (sourceModuleBase != null ? sourceModuleBase.SourceModule : null);
            }
        }

        /// <summary>
        /// (R) Return sourceModuleBase.LexData.
        /// </summary>
        internal LEXDATA LexData
        {
            get { return (sourceModuleBase != null ? sourceModuleBase.LexData : null); }
        }

        //------------------------------------------------------------
        // CSourceData Constructor (private)
        //
        /// <summary>
        /// This is private. Call CreateInstance method to create an instance.
        /// </summary>
        /// <param name="module"></param>
        //------------------------------------------------------------
        private CSourceData(CSourceModuleBase module)
        {
            DebugUtil.Assert(module != null);
            sourceModuleBase = module;
        }

        //------------------------------------------------------------
        // CSourceData.CreateInstance
        //
        /// <summary>
        /// <para>The constructor is private. Call this method to create a instance.</para>
        /// <para>Create and set a given reference to CSourceModuleBase instance.</para>
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal CSourceData CreateInstance(CSourceModuleBase module)
        {
            return new CSourceData(module);
        }

        //------------------------------------------------------------
        // CSourceData.Release
        //
        /// <summary>
        /// Clear CSourceModuleBase instance.
        /// </summary>
        //------------------------------------------------------------
        //internal void Release()
        //{
        //    if (SourceModule != null) SourceModule.ReleaseDataRef();
        //    sourceModuleBase = null;
        //}

        //------------------------------------------------------------
        // CSourceData.GetLexResults
        //
        /// <summary>
        /// Overwrite argument lexData with the data of SourceModule.lexDataBlock.
        /// </summary>
        /// <param name="lexData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool GetLexResults(LEXDATA lexData)
        {
            return this.SourceModule.GetLexResults(this, lexData);
        }

        //------------------------------------------------------------
        // CSourceData.GetSingleTokenPos
        //
        /// <summary>
        /// <para>Get the POSDATA instance of the specified token index.</para>
        /// <para>Call CSourceModuleBase.GetSingleTokenPos method.</para>
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal POSDATA GetSingleTokenPos(int tokenIndex)
        {
            return sourceModuleBase.GetSingleTokenPos(tokenIndex);
        }

        //------------------------------------------------------------
        // CSourceData.GetSingleTokenData
        //------------------------------------------------------------
        //virtual internal int GetSingleTokenData(int iToken, TOKENDATA pTokenData)
        //{
        //    return sourceModuleBase.GetSingleTokenData (this, iToken, pTokenData);
        //}

        //------------------------------------------------------------
        // CSourceData.GetTokenText
        //------------------------------------------------------------
        //virtual internal int GetTokenText(int iTokenId, out string ppszText, out int piLength)
        //{
        //    return sourceModuleBase.GetTokenText (iTokenId, ppszText, piLength);
        //}

        //------------------------------------------------------------
        // CSourceData.IsInsideComment
        //------------------------------------------------------------
        //virtual internal int IsInsideComment(POSDATA pos, ref bool pfInComment)
        //{
        //    return sourceModuleBase.IsInsideComment (this, pos, pfInComment);
        //}

        //------------------------------------------------------------
        // CSourceData.ParseTopLevel
        //
        /// <summary>
        /// Call CSourceModule.ParseTopLevel.
        /// </summary>
        /// <param name="tree">To Which the resulting node tree is set.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool ParseTopLevel(out BASENODE tree)
        {
            return sourceModuleBase.ParseTopLevel(this, out tree, false);
        }

        //------------------------------------------------------------
        // CSourceData.ParseTopLevel2
        //------------------------------------------------------------
        //virtual internal bool ParseTopLevel2(out BASENODE ppTree, bool fCreateParseDiffs)
        //{
        //    return sourceModuleBase.ParseTopLevel (this, ppTree, fCreateParseDiffs);
        //}

        //------------------------------------------------------------
        // CSourceData.GetErrors
        //------------------------------------------------------------
        virtual internal CErrorContainer GetErrors(ERRORCATEGORY iCategory)
        {
            return sourceModuleBase.GetErrors(this, iCategory);
        }

        //------------------------------------------------------------
        // CSourceData.GetInteriorParseTree
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal CInteriorTree GetInteriorParseTree(BASENODE node)
        {
            return sourceModuleBase.GetInteriorParseTree (this, node);
        }

        //------------------------------------------------------------
        // CSourceData.LookupNode
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::LookupNode (
        //    NAME *pKey, long iOrdinal, BASENODE **ppNode, long *piGlobalOrdinal)
        //virtual internal int LookupNode(
        //    ref string pKey, int iOrdinal, ref BASENODE ppNode, ref int piGlobalOrdinal)
        //{
        //    return sourceModuleBase.LookupNode (this, pKey, iOrdinal, ppNode, piGlobalOrdinal);
        //}

        //------------------------------------------------------------
        // CSourceData.GetNodeKeyOrdinal
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::GetNodeKeyOrdinal (BASENODE *pNode, NAME **ppKey, long *piKeyOrdinal)
        //virtual internal int GetNodeKeyOrdinal(BASENODE pNode, ref string ppKey, ref int piKeyOrdinal)
        //{
        //    return sourceModuleBase.GetNodeKeyOrdinal (this, pNode, ppKey, piKeyOrdinal);
        //}

        //------------------------------------------------------------
        // CSourceData.GetGlobalKeyArray
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::GetGlobalKeyArray (KEYEDNODE *pKeyedNodes, long iSize, long *piCopied)
        //virtual internal int GetGlobalKeyArray(KEYEDNODE pKeyedNodes, int iSize, ref int piCopied)
        //{
        //    return sourceModuleBase.GetGlobalKeyArray (this, pKeyedNodes, iSize, piCopied);
        //}

        //------------------------------------------------------------
        // CSourceData.ParseForErrors
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::ParseForErrors ()
        //virtual internal int ParseForErrors()
        //{
        //    return sourceModuleBase.ParseForErrors (this);
        //}

        //------------------------------------------------------------
        // CSourceData.FindLeafNode
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::FindLeafNode (
        //  const POSDATA pos, BASENODE **ppNode, ICSInteriorTree **ppTree)
        //virtual internal int FindLeafNode(POSDATA pos, ref BASENODE ppNode, ref CInteriorTree ppTree)
        //{
        //    return FindLeafNodeEx(pos, EF_FULL, ppNode, ppTree);
        //}

        //------------------------------------------------------------
        // CSourceData.FindLeafNodeForToken
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::FindLeafNodeForToken (
        //  long iToken, BASENODE **ppNode, ICSInteriorTree **ppTree)
        //virtual internal int FindLeafNodeForToken(
        //    int iToken, ref BASENODE ppNode, ref CInteriorTree ppTree)
        //{
        //    return FindLeafNodeForTokenEx(iToken, EF_FULL, ppNode, ppTree);
        //}

        //------------------------------------------------------------
        // CSourceData.FindLeafNodeEx
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::FindLeafNodeEx (
        //    const POSDATA pos, ExtentFlags flags, BASENODE **ppNode, ICSInteriorTree **ppTree)
        //virtual internal int FindLeafNodeEx(
        //    POSDATA pos, ExtentFlags flags, ref BASENODE ppNode, ref CInteriorTree ppTree)
        //{
        //    return sourceModuleBase.FindLeafNodeEx (this, pos, flags, ppNode, ppTree);
        //}

        //------------------------------------------------------------
        // CSourceData.FindLeafNodeForTokenEx
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::FindLeafNodeForTokenEx (
        //  long iToken, ExtentFlags flags, BASENODE **ppNode, ICSInteriorTree **ppTree)
        //virtual internal int FindLeafNodeForTokenEx(
        //    int iToken, ExtentFlags flags, ref BASENODE ppNode, ref CInteriorTree ppTree)
        //{
        //    return sourceModuleBase.FindLeafNodeForTokenEx (this, iToken, flags, ppNode, ppTree);
        //}

        //------------------------------------------------------------
        // CSourceData.GetExtent
        //------------------------------------------------------------
        //virtual internal int GetExtent(BASENODE pNode, POSDATA pposStart, POSDATA pposEnd)
        //{
        //}

        //STDMETHODIMP CSourceData::GetExtent(BASENODE *pNode, POSDATA *pposStart, POSDATA *pposEnd) {
        //    return GetExtentEx( pNode, pposStart, pposEnd, EF_FULL);
        //};

        //------------------------------------------------------------
        // CSourceData.GetExtentEx
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool GetExtentEx(
            BASENODE node,
            POSDATA start,
            POSDATA end,
            ExtentFlags flags)
        {
            return sourceModuleBase.GetExtent (node, start, end, flags);
        }

        //------------------------------------------------------------
        // CSourceData.GetTokenExtent
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::GetTokenExtent (
        //    BASENODE *pNode, long *piFirstToken, long *piLastToken)
        //virtual internal int GetTokenExtent(BASENODE pNode, ref int piFirstToken, ref int piLastToken)
        //{
        //    return sourceModuleBase.GetTokenExtent (pNode, piFirstToken, piLastToken);
        //}

        //------------------------------------------------------------
        // CSourceData.GetDocComment
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::GetDocComment (BASENODE *pNode, long *piComment, long *piCount)
        //virtual internal int GetDocComment(BASENODE pNode, ref int piComment, ref int piCount)
        //{
        //    return sourceModuleBase.GetDocComment (pNode, piComment, piCount);
        //}

        //------------------------------------------------------------
        // CSourceData.GetDocCommentXML
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::GetDocCommentXML (BASENODE *pNode, BSTR *pbstrDoc)
        //virtual internal int GetDocCommentXML(BASENODE pNode, ref string pbstrDoc)
        //{
        //    return sourceModuleBase.GetDocCommentXML (pNode, pbstrDoc);
        //}

        //------------------------------------------------------------
        // CSourceData.IsUpToDate
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::IsUpToDate (BOOL *pfTokenized, BOOL *pfTopParsed)
        //virtual internal int IsUpToDate(ref bool pfTokenized, ref bool pfTopParsed)
        //{
        //    return sourceModuleBase.IsUpToDate (pfTokenized, pfTopParsed);
        //}

        //------------------------------------------------------------
        // CSourceData.MapSourceLine
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::MapSourceLine(
        //    long iLine, long *piMappedLine, PCWSTR *ppszFilename, BOOL *pbIsHidden)
        //virtual internal int MapSourceLine(
        //    int iline, out int piMappedLine, out string ppszFilename, out bool pbIsHidden)
        //{
        //    return sourceModuleBase.MapSourceLine(this, iLine, piMappedLine, ppszFilename, pbIsHidden);
        //}

        //------------------------------------------------------------
        // CSourceData.GetLastRenamedTokenIndex
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::GetLastRenamedTokenIndex(long *piTokIdx, NAME **ppPreviousName)
        //virtual internal int GetLastRenamedTokenIndex(ref int piTokIdx, ref string ppPreviousName)
        //{
        //    return sourceModuleBase.GetLastRenamedTokenIndex(piTokIdx, ppPreviousName);
        //}

        //------------------------------------------------------------
        // CSourceData.ResetRenamedTokenData
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::ResetRenamedTokenData()
        //virtual internal int ResetRenamedTokenData()
        //{
        //    return sourceModuleBase.ResetRenamedTokenData();
        //}

        //------------------------------------------------------------
        // CSourceData.CloneToNonLocking
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::CloneToNonLocking(ICSSourceData **ppData)
        //virtual internal int CloneToNonLocking(ref CSourceData ppData)
        //{
        //}

        //------------------------------------------------------------
        // CSourceData.OnRename
        //------------------------------------------------------------
        //STDMETHODIMP CSourceData::OnRename(NAME * pNewName)
        //virtual internal int OnRename(ref string pNewName)
        //{
        //    return sourceModuleBase.OnRename(pNewName);
        //}
    }
}
