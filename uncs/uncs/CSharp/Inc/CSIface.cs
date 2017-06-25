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

/* this ALWAYS GENERATED file contains the definitions for the interfaces */

/* File created by MIDL compiler version 6.00.0366 */
//@@MIDL_FILE_HEADING(  )

//============================================================================
// CSIface.cs
//
// (sscli) sscli20\prebuilt\idl\csiface.h
// 2015/02/06
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Uncs
{
    //======================================================================
    // enum CompilerCreationFlags   (CCF_)
    //
    /// <summary>
    /// <para>KEEPIDENTTABLES, KEEPNODETABLES, TRACKCOMMENTS,
    /// and IDEUSAGE means that all bits are set.</para>
    /// <para>In sscli, with prefix CCF_ . (CSharp\Inc\CSIface.cs)</para>
    /// </summary>
    //======================================================================
    [Flags]
    internal enum CompilerCreationFlags : int
    {
        KEEPIDENTTABLES = 0x1,
        KEEPNODETABLES = 0x2,
        TRACKCOMMENTS = 0x4,
        IDEUSAGE = KEEPIDENTTABLES | KEEPNODETABLES | TRACKCOMMENTS
    }

    //======================================================================
    // enum ExtentFlags (EF_)
    //
    /// <summary>
    /// <para>In sscli, with prefix EF_. (CSharp\Inc\CSIface.cs)</para>
    /// </summary>
    //======================================================================
    [Flags]
    internal enum ExtentFlags : int
    {
        FULL = 0,
        SINGLESTMT = FULL + 1,
        POSSIBLE_GENERIC_NAME = SINGLESTMT + 1,
        PREFER_LEFT_NODE = 0x4,
        IGNORE_TOKEN_STREAM = 0x80
    }

    //======================================================================
    // class CHECKSUM
    //
    /// <summary>
    /// <para>(CSharp\Inc\CSIface.cs)</para>
    /// </summary>
    //======================================================================
    internal class CHECKSUM
    {
        internal System.Guid GuidID;
        internal int DataCount = 0;
        internal System.Object Data = null;   // [size_is]
    }

    //======================================================================
    // class KEYEDNODE
    //
    /// <summary>
    /// <para>(CSharp\Inc\CSIface.cs)</para>
    /// </summary>
    //======================================================================
    internal class KEYEDNODE
    {
        internal string Key = null;
        internal BASENODE Node = null;
    }
}
