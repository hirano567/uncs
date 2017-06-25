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
// File: alink.h
//
// main ALink interface
// ===========================================================================

//============================================================================
//  ALink.cs
//
//  2013/11/11
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // enum AssemblyFlagsEnum (af)
    //
    /// <summary>
    /// AssemblyFlags in sscli.
    /// </summary>
    //======================================================================
    [Flags]
    internal enum AssemblyFlagsEnum : int
    {
        /// <summary>
        /// Normal case
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// An InMemory single-file assembly the filename == AssemblyName
        /// </summary>
        InMemory = 0x00000001,

        /// <summary>
        /// Use DeleteToken and Merging to remove the AssemblyAttributesGoHere
        /// </summary>
        CleanModules = 0x00000002,

        /// <summary>
        /// Do not generate hashes for AssemblyRefs
        /// </summary>
        NoRefHash = 0x00000004,

        /// <summary>
        /// Do not check for duplicate types (ExportedType table + manifest file's TypeDef table)
        /// </summary>
        NoDupTypeCheck = 0x00000008,

        /// <summary>
        /// Do dupe checking for type forwarders.
        /// This is so you can specify afNoDupTypeCheck for regular typedefs + afDupeCheckTypeFwds.
        /// </summary>
        DupeCheckTypeFwds = 0x00000010,
    }
}
