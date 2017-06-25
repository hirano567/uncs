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
// File: inttree.h
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
// File: inttree.cpp
//
// ===========================================================================

//============================================================================
// inttree.cs
//
// 2015/03/11
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Uncs
{
    //======================================================================
    // CInteriorTree
	//
    /// <summary>
    /// <para>This is the COM object handed out to people requesting interior parses thru
	/// ISCSourceData::GetInteriorTree().  It holds a reference on the originating
	/// source data object, and a reference on the underlying parse tree (which is
    /// shared by potentially more than one of these objects).</para>
    /// </summary>
    /// <remarks>
    /// ICSInteriorTree is implemented by CInteriorTree only.
    /// <code>
    /// ICSInteriorTree : public IUnknown
    /// {
    /// public:
    ///     virtual HRESULT STDMETHODCALLTYPE GetTree( 
    ///         MIDL_BASENODE **ppNode) = 0;
    ///        
    ///     virtual HRESULT STDMETHODCALLTYPE GetErrors( 
    ///         ICSErrorContainer **ppErrors) = 0;
    /// };
    /// </code>
    /// </remarks>
    //======================================================================
    internal class CInteriorTree  // : ICSInteriorTree
    {
        /// <summary>
        /// NOTE:  This is a ref'd pointer!
        /// </summary>
        private CSourceData sourceData = null;      // *m_pSrcData;

        /// <summary>
        /// The interior node we refer to
        /// </summary>
        private CInteriorNode interiorNode = null;  // *m_pInteriorNode;

#if DEBUG
        /// <summary>
        /// These should only be used by one thread!
        /// </summary>
        Thread debugThread = null; // DWORD m_dwThreadId;
#endif
        //------------------------------------------------------------
        // CInteriorTree    Constructor
        //
        /// <summary>
        /// Does nothing.
        /// </summary>
        //------------------------------------------------------------
        private CInteriorTree()
        {
        }

        //private ~CInteriorTree();

        //------------------------------------------------------------
        // CInteriorTree.CreateInstance
        //
        /// <summary>
        /// Create a CInteriorTree instance and initialize it with the arguments.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal CInteriorTree CreateInstance(CSourceData data, BASENODE node)
        {
            CInteriorTree tree = new CInteriorTree();
            if (tree.Initialize(data, node))
            {
                return tree;
            }
            return null;
        }

        //------------------------------------------------------------
        // CInteriorTree.Initialize
        //
        /// <summary>
        /// Set this.interiorNode and return true.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Initialize(CSourceData data, BASENODE node)
        {
            sourceData = data;
#if DEBUG
            debugThread = Thread.CurrentThread;
#endif
            interiorNode = data.Module.GetInteriorNode(data, node);
            return true;
        }

        // ICSInteriorTree

        //------------------------------------------------------------
        // CInteriorTree.GetTree
        //
        /// <summary>
        /// Return this.interiorNode.RootNode.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE GetTree()
        {
            DebugUtil.Assert(interiorNode != null);
#if DEBUG
            DebugUtil.Assert(debugThread == Thread.CurrentThread);
#endif
            return interiorNode.RootNode;
        }

        //------------------------------------------------------------
        // CInteriorTree.GetErrors
        //
        /// <summary>
        /// Return this.interiorNode.ErrorContainer.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CErrorContainer GetErrors()
        {
            DebugUtil.Assert(interiorNode != null);
#if DEBUG
            DebugUtil.Assert(debugThread == Thread.CurrentThread);
#endif
            return interiorNode.ErrorContainer;
        }
    }

    //======================================================================
    // CInteriorNode
    //
    /// <summary>
    /// This is the base class for the internal objects that "hold" interior parse trees.
    /// One of these exists for each interior node that is parsed.
    /// </summary>
    //======================================================================
    internal class CInteriorNode
    {
        //------------------------------------------------------------
        // CInteriorNode    Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// NOTE:  this is NOT a ref'd pointer (unnecessary)
        /// </summary>
        protected CSourceModuleBase sourceModuleBase = null;    // * m_pModule;

        /// <summary>
        /// This is the interior node "container" whose child tree we contain
        /// </summary>
        protected BASENODE containerNode = null;    // * m_pContainerNode;

        /// <summary>
        /// (R) This is the interior node "container" whose child tree we contain
        /// </summary>
        internal BASENODE RootNode
        {
            get { return containerNode; }   // GetRootNode()
        }

        /// <summary>
        /// Parse errors
        /// </summary>
        protected CErrorContainer errorContainer = null; // * m_pErrors;

        /// <summary>
        /// (R) Parse errors
        /// </summary>
        internal CErrorContainer ErrorContainer
        {
            get { return errorContainer; }  // GetErrorContainer()
        }

        //------------------------------------------------------------
        // CInteriorNode Constructor
        //
        /// <summary></summary>
        /// <param name="module"></param>
        /// <param name="root"></param>
        //------------------------------------------------------------
        internal CInteriorNode(CSourceModuleBase module, BASENODE root)
        {
            sourceModuleBase = module;
            containerNode = root;
        }

        //------------------------------------------------------------
        // CInteriorNode.CreateErrorContainer
        //
        /// <summary>
        /// Create an CErrorContainer instance
        /// and set it to this.errorContainer.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CErrorContainer CreateErrorContainer()
        {
            this.errorContainer = CErrorContainer.CreateInstance(
                 ERRORCATEGORY.METHODPARSE,
                 (uint)0);
            return this.errorContainer;
        }
    }
    
    //======================================================================
    // CPrimaryInteriorNode
    //
    /// <summary>
    /// This is a CInteriorNode that holds a parsed interior node
    /// in the NRHEAP built into the corresponding source module.
    /// This is the most-often-used one;
    /// it is created when no other interior node is parsed
    /// whose node memory is allocated from the main module heap.
    /// </summary>
    //======================================================================
    internal class CPrimaryInteriorNode : CInteriorNode
    {
        internal CPrimaryInteriorNode(CSourceModule module, BASENODE container)
            : base(module, container)
        {
        }

        static internal CPrimaryInteriorNode CreateInstance(
            CSourceModule module,
            BASENODE container)
        {
            return new CPrimaryInteriorNode(module, container);
        }
    }
    
    //======================================================================
    // CSecondaryInteriorNode
    //
    /// <summary>
    /// This CInteriorNode derivation is used when a CPrimaryInteriorNode already exists,
    /// and is thus using the "second half" of the module's node allocation heap.
    /// This object has its own such heap from which the interior nodes are allocated.
    /// </summary>
    //======================================================================
    internal class CSecondaryInteriorNode : CInteriorNode
    {
        internal CSecondaryInteriorNode(CSourceModuleBase module, BASENODE container)
            : base(module, container)
        {
        }

        static internal CSecondaryInteriorNode CreateInstance(
            CSourceModuleBase module,
            BASENODE container)
        {
            return new CSecondaryInteriorNode(module, container);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    // inttree.cpp
    //
    ////////////////////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////////////////////
    //// CInteriorTree::~CInteriorTree
    //
    //CInteriorTree::~CInteriorTree ()
    //{
    //    if (interiorNode != NULL)
    //        interiorNode->Release();
    //
    //    // NOTE:  Must release this last -- the above might destroy our heap!
    //    if (sourceData != NULL)
    //        sourceData->Release();
    //}
    //

    //////////////////////////////////////////////////////////////////////////////////
    //// CInteriorTree::AddRef
    //
    //STDMETHODIMP_(ULONG) CInteriorTree::AddRef ()
    //{
    //    ASSERT (threadId == GetCurrentThreadId());
    //    return ++m_iRef;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CInteriorTree::Release
    //
    //STDMETHODIMP_(ULONG) CInteriorTree::Release ()
    //{
    //    ASSERT (threadId == GetCurrentThreadId());
    //
    //    if (--m_iRef == 0)
    //    {
    //        delete this;
    //        return 0;
    //    }
    //
    //    return m_iRef;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CInteriorTree::QueryInterface
    //
    //STDMETHODIMP CInteriorTree::QueryInterface (REFIID riid, void **ppObj)
    //{
    //    ASSERT (threadId == GetCurrentThreadId());
    //
    //    *ppObj = NULL;
    //
    //    if (riid == IID_IUnknown || riid == IID_ICSInteriorTree)
    //    {
    //        *ppObj = (ICSInteriorTree *)this;
    //        AddRef();
    //        return S_OK;
    //    }
    //
    //    return E_NOINTERFACE;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CInteriorNode::~CInteriorNode
    //
    //CInteriorNode::~CInteriorNode ()
    //{
    //    // When the interior node is destroyed, it must clear out the appropriate
    //    // fields of the container node to indicate that it is no longer parsed.
    //    switch (m_pContainerNode->kind)
    //    {
    //        case NK_CTOR:
    //        case NK_DTOR:
    //        case NK_METHOD:
    //        case NK_OPERATOR:
    //            {
    //                WriteToggler allowWrites(ProtectedEntityFlags::ParseTree, m_pContainerNode->AsANYMETHOD->pBody, m_pContainerNode->AsANYMETHOD->pInteriorNode);
    //                m_pContainerNode->AsANYMETHOD->pBody = NULL;
    //                m_pContainerNode->AsANYMETHOD->pInteriorNode = NULL;
    //            }
    //            break;
    //
    //        case NK_ACCESSOR:
    //            {
    //                WriteToggler allowWrites(ProtectedEntityFlags::ParseTree, m_pContainerNode->asACCESSOR()->pBody, m_pContainerNode->asACCESSOR()->pInteriorNode);
    //                m_pContainerNode->asACCESSOR()->pBody = NULL;
    //                m_pContainerNode->asACCESSOR()->pInteriorNode = NULL;
    //            }
    //            break;
    //
    //        default:
    //            VSFAIL ("Interior node kind not handled in CInteriorNode destruction!");
    //            break;
    //    }
    //
    //    // Get rid of our errors
    //    if (m_pErrors != NULL)
    //        m_pErrors->Release();
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CInteriorNode::AddRef
    //
    //void CInteriorNode::AddRef ()
    //{
    //    // Interior nodes can be used by more than one thread.  To serialize access
    //    // to its ref count and be assured that full creation and destruction of
    //    // these guys are serialized.
    //    CTinyGate   gate (m_pModule->GetStateLock());
    //    m_iRef++;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CInteriorNode::Release
    //
    //void CInteriorNode::Release ()
    //{
    //    CTinyGate   gate (m_pModule->GetStateLock());
    //    long iRef = --m_iRef;
    //    if (iRef == 0)
    //        delete this;
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CPrimaryInteriorNode::~CPrimaryInteriorNode
    //
    //CPrimaryInteriorNode::~CPrimaryInteriorNode ()
    //{
    //    // When a primary interiorn node is destroyed, it must notify the module that
    //    // its internal heap is available for another primary interior node parse.
    //    ((CSourceModule *)m_pModule)->ResetHeapBusyFlag ();
    //}
}
