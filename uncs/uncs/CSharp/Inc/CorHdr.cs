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
/*****************************************************************************
 **                                                                         **
 ** CorHdr.h - contains definitions for the Runtime structures,             **
 **            needed to work with metadata.                                **
 **                                                                         **
 *****************************************************************************/

//============================================================================
// CorHdr.cs
//
// (sscli) sscli20\clr\src\inc\corhdr.h
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Uncs
{
    static internal partial class Cor
    {
        internal const int MAX_CLASS_NAME = 1024;
    }

#if false   // enum CorTypeAttr
    //======================================================================
    // enum CorTypeAttr (td)
    //
    /// <summary>
    /// <para>TypeDef/ExportedType attr bits, used by DefineTypeDef.</para>
    /// <para>Has the same members as System.Reflection.TypeAttributes enumeration except Forwarder.</para>
    /// <para>(CLR\src\Inc\CorHdr.cs)</para>
    /// </summary>
    //======================================================================
    internal enum CorTypeAttr : int
    {
        // Use this mask to retrieve the type visibility information.
        VisibilityMask = 0x00000007,    //                                                      (tdVisibilityMask)
        NotPublic = 0x00000000,         // Class is not public scope.                           (tdNotPublic)
        Public = 0x00000001,            // Class is public scope.                               (tdPublic)
        NestedPublic = 0x00000002,      // Class is nested with public visibility.              (tdNestedPublic)
        NestedPrivate = 0x00000003,     // Class is nested with private visibility.             (tdNestedPrivate)
        NestedFamily = 0x00000004,      // Class is nested with family visibility.              (tdNestedFamily)
        NestedAssembly = 0x00000005,    // Class is nested with assembly visibility.            (tdNestedAssembly)
        NestedFamANDAssem = 0x00000006, // Class is nested with family and assembly visibility. (tdNestedFamANDAssem)
        NestedFamORAssem = 0x00000007,  // Class is nested with family or assembly visibility.  (tdNestedFamORAssem)

        // Use this mask to retrieve class layout information
        LayoutMask = 0x00000018,        //                                          (tdLayoutMask)
        AutoLayout = 0x00000000,        // Class fields are auto-laid out           (tdAutoLayout)
        SequentialLayout = 0x00000008,  // Class fields are laid out sequentially   (tdSequentialLayout)
        ExplicitLayout = 0x00000010,    // Layout is supplied explicitly            (tdExplicitLayout)
        // end layout mask

        // Use this mask to retrieve class semantics information.
        ClassSemanticsMask = 0x00000060,    //                          (tdClassSemanticsMask)
        Class = 0x00000000,                 // Type is a class.         (tdClass)
        Interface = 0x00000020,             // Type is an interface.    (tdInterface)
        // end semantics mask

        // Special semantics in addition to class semantics.
        Abstract = 0x00000080,          // Class is abstract                            (tdAbstract)
        Sealed = 0x00000100,            // Class is concrete and may not be extended    (tdSealed)
        SpecialName = 0x00000400,       // Class name is special.  Name describes how.  (tdSpecialName)

        // Implementation attributes.
        Import = 0x00001000,            // Class / interface is imported    (tdImport)
        Serializable = 0x00002000,      // The class is Serializable.       (tdSerializable)

        // Use tdStringFormatMask to retrieve string information for native interop
        StringFormatMask = 0x00030000,  // (tdStringFormatMask)
        AnsiClass = 0x00000000,         // LPTSTR is interpreted as ANSI in this class  (tdAnsiClass)
        UnicodeClass = 0x00010000,      // LPTSTR is interpreted as UNICODE             (tdUnicodeClass)
        AutoClass = 0x00020000,         // LPTSTR is interpreted automatically          (tdAutoClass)
        CustomFormatClass = 0x00030000, // A non-standard encoding specified by CustomFormatMask (tdCustomFormatClass)
        CustomFormatMask = 0x00C00000,  // Use this mask to retrieve non-standard encoding information for native interop.
                                        //The meaning of the values of these 2 bits is unspecified. (tdCustomFormatMask)

        // end string format mask

        BeforeFieldInit = 0x00100000,   // Initialize the class any time before first static field access.  (tdBeforeFieldInit)
        Forwarder = 0x00200000,         // This ExportedType is a type forwarder.                           (tdForwarder)

        // Flags reserved for runtime use.
        ReservedMask = 0x00040800,      //                                          (tdReservedMask)
        RTSpecialName = 0x00000800,     // Runtime should check name encoding.      (td RTSpecialName)
        HasSecurity = 0x00040000,       // Class has security associate with it.    (tdHasSecurity)
    }
    // CorTypeAttr;
#endif

    static internal partial class Cor
    {
        //------------------------------------------------------------
        // Cor.IsTypeDefInterface
        //
        // #define IsTdInterface(x) (((x) & tdClassSemanticsMask) == tdInterface) // sscli
        //------------------------------------------------------------
        static internal bool IsTypeDefInterface(int x)
        {
            // CorTypeAttr.ClassSemanticsMask = 0x0060
            // TypeAttributes.ClassSemanticsMask = 0x0020
            // CorTypeAttr.Interface = 0x0020

            //return (((x) & (int)CorTypeAttr.ClassSemanticsMask) == (int)CorTypeAttr.Interface);
            return (((x) & (int)TypeAttributes.ClassSemanticsMask) == (int)TypeAttributes.Interface);
        }
    }

    //======================================================================
    // enum CorDeclSecurity (dcl)
    //
    /// <summary>
    /// DeclSecurity attr bits, used by DefinePermissionSet.
    /// </summary>
    /// <remarks>
    /// In sscli, each member has prefix "dcl".
    /// </remarks>
    //======================================================================
    internal enum CorDeclSecurity : int
    {
        // Mask allows growth of enum.
        ActionMask = 0x001f,	    // dclActionMask

        ActionNil = 0x0000,	        // dclActionNil
        Request = 0x0001,	        // dclRequest
        Demand = 0x0002,	        // dclDemand
        Assert = 0x0003,	        // dclAssert
        Deny = 0x0004,	            // dclDeny
        PermitOnly = 0x0005,	    // dclPermitOnly
        LinktimeCheck = 0x0006,	    // dclLinktimeCheck
        InheritanceCheck = 0x0007,	// dclInheritanceCheck
        RequestMinimum = 0x0008,	// dclRequestMinimum
        RequestOptional = 0x0009,	// dclRequestOptional
        RequestRefuse = 0x000a,	    // dclRequestRefuse

        // Persisted grant set at prejit time
        PrejitGrant = 0x000b,	    // dclPrejitGrant

        // Persisted denied set at prejit time
        PrejitDenied = 0x000c,	    // dclPrejitDenied

        NonCasDemand = 0x000d,	    // dclNonCasDemand
        NonCasLinkDemand = 0x000e,	// dclNonCasLinkDemand
        NonCasInheritance = 0x000f,	// dclNonCasInheritance

        // Maximum legal value
        MaximumValue = 0x000f,	    // dclMaximumValue
    }

    //======================================================================
    // CorElementType   (ELEMENT_TYPE_)
    //
    /// <summary>
    /// Element type for Cor signature
    /// (CSharp\Inc\CorHdr.cs)
    /// </summary>
    //======================================================================
    internal enum CorElementType : int
    {
        END = 0x0,
        VOID = 0x1,
        BOOLEAN = 0x2,
        CHAR = 0x3,
        I1 = 0x4,
        U1 = 0x5,
        I2 = 0x6,
        U2 = 0x7,
        I4 = 0x8,
        U4 = 0x9,
        I8 = 0xa,
        U8 = 0xb,
        R4 = 0xc,
        R8 = 0xd,
        STRING = 0xe,

        // every type above PTR will be simple type

        /// <summary>
        /// PTR &lt;type&gt;
        /// </summary>
        PTR = 0xf,

        /// <summary>
        /// BYREF &lt;type&gt;
        /// </summary>
        BYREF = 0x10,

        // Please use VALUETYPE. VALUECLASS is deprecated.

        /// <summary>
        /// VALUETYPE &lt;class Token&gt;
        /// </summary>
        VALUETYPE = 0x11,

        /// <summary>
        /// CLASS &lt;class Token&gt;
        /// </summary>
        CLASS = 0x12,

        /// <summary>
        /// a class type variable VAR &lt;U1&gt;
        /// </summary>
        VAR = 0x13,

        /// <summary>
        /// MDARRAY &lt;type&gt; &lt;rank&gt; &lt;bcount&gt; &lt;bound1&gt;
        /// ... &lt;lbcount&gt; &lt;lb1&gt; ...
        /// </summary>
        ARRAY = 0x14,

        /// <summary>
        /// GENERICINST &lt;generic type&gt; &lt;argCnt&gt; &lt;arg1&gt;
        /// ... &lt;argn&gt;
        /// </summary>
        GENERICINST = 0x15,

        /// <summary>
        /// TYPEDREF  (it takes no args) a typed referece to some other type
        /// </summary>
        TYPEDBYREF = 0x16,

        /// <summary>
        /// native integer size
        /// </summary>
        I = 0x18,

        /// <summary>
        /// native unsigned integer size
        /// </summary>
        U = 0x19,

        /// <summary>
        /// FNPTR &lt;complete sig for the function including calling convention&gt;
        /// </summary>
        FNPTR = 0x1B,

        /// <summary>
        /// Shortcut for System.Object
        /// </summary>
        OBJECT = 0x1C,

        /// <summary>
        /// Shortcut for single dimension zero lower bound array
        /// </summary>
        SZARRAY = 0x1D,

        // SZARRAY <type>

        /// <summary>
        /// a method type variable MVAR &lt;U1&gt;
        /// </summary>
        MVAR = 0x1e,

        // This is only for binding

        /// <summary>
        /// required C modifier : E_T_CMOD_REQD &lt;mdTypeRef/mdTypeDef&gt;
        /// </summary>
        CMOD_REQD = 0x1F,

        /// <summary>
        /// optional C modifier : E_T_CMOD_OPT &lt;mdTypeRef/mdTypeDef&gt;
        /// </summary>
        CMOD_OPT = 0x20,

        // This is for signatures generated internally (which will not be persisted in any way).

        /// <summary>
        /// INTERNAL &lt;typehandle&gt;
        /// </summary>
        INTERNAL = 0x21,

        // Note that this is the max of base type excluding modifiers

        /// <summary>
        /// first invalid element type
        /// </summary>
        MAX = 0x22,

        MODIFIER = 0x40,

        /// <summary>
        /// sentinel for varargs
        /// </summary>
        SENTINEL = 0x01 | MODIFIER,

        PINNED = 0x05 | MODIFIER,

        /// <summary>
        /// used only internally for R4 HFA types
        /// </summary>
        R4_HFA = 0x06 | MODIFIER,

        /// <summary>
        /// used only internally for R8 HFA types
        /// </summary>
        R8_HFA = 0x07 | MODIFIER,
    }

    //======================================================================
    // CorSerializationType   (SERIALIZATION_TYPE_)
    //
    /// <summary>
    /// Serialization types for Custom attribute support
    /// (CSharp\Inc\CorHdr.cs)
    /// </summary>
    //======================================================================
    internal enum CorSerializationType : byte
    {
        UNDEFINED = (byte)0,
        BOOLEAN = (byte)CorElementType.BOOLEAN,
        CHAR = (byte)CorElementType.CHAR,
        I1 = (byte)CorElementType.I1,
        U1 = (byte)CorElementType.U1,
        I2 = (byte)CorElementType.I2,
        U2 = (byte)CorElementType.U2,
        I4 = (byte)CorElementType.I4,
        U4 = (byte)CorElementType.U4,
        I8 = (byte)CorElementType.I8,
        U8 = (byte)CorElementType.U8,
        R4 = (byte)CorElementType.R4,
        R8 = (byte)CorElementType.R8,
        STRING = (byte)CorElementType.STRING,

        /// <summary>
        /// Shortcut for single dimension zero lower bound array
        /// </summary>
        SZARRAY = (byte)CorElementType.SZARRAY,

        TYPE = (byte)0x50,
        TAGGED_OBJECT = (byte)0x51,
        FIELD = (byte)0x53,
        PROPERTY = (byte)0x54,
        ENUM = (byte)0x55
    }

    static internal partial class Cor
    {
        //============================================================
        // Token tags.
        //
        // テーブル番号は最上位バイト。
        //============================================================

        //internal const int mdtModule = 0x00000000;
        //internal const int mdtTypeRef = 0x01000000;
        //internal const int mdtTypeDef = 0x02000000;
        //internal const int mdtFieldDef = 0x04000000;
        //internal const int mdtMethodDef = 0x06000000;
        //internal const int mdtParamDef = 0x08000000;
        //internal const int mdtInterfaceImpl = 0x09000000;
        //internal const int mdtMemberRef = 0x0a000000;
        //internal const int mdtCustomAttribute = 0x0c000000;
        //internal const int mdtPermission = 0x0e000000;
        //internal const int mdtSignature = 0x11000000;
        //internal const int mdtEvent = 0x14000000;
        //internal const int mdtProperty = 0x17000000;
        //internal const int mdtModuleRef = 0x1a000000;
        //internal const int mdtTypeSpec = 0x1b000000;
        internal const int mdtAssembly = 0x20000000;
        //internal const int mdtAssemblyRef = 0x23000000;
        //internal const int mdtFile = 0x26000000;
        //internal const int mdtExportedType = 0x27000000;
        //internal const int mdtManifestResource = 0x28000000;
        //internal const int mdtGenericParam = 0x2a000000;
        //internal const int mdtMethodSpec = 0x2b000000;
        //internal const int mdtGenericParamConstraint = 0x2c000000;

        //internal const int mdtString = 0x70000000;
        //internal const int mdtName = 0x71000000;
        //internal const int mdtBaseType = 0x72000000;       // Leave this on the high end value. This does not correspond to metadata table

        //============================================================
        // Build / decompose tokens.
        //============================================================
#if false
        //------------------------------------------------------------
        // RidToToken
        //------------------------------------------------------------
        //#define RidToToken(rid,tktype) ((rid) |= (tktype))
        static internal void RidToToken(ref int rid, int tktype) { rid |= tktype; }

        //------------------------------------------------------------
        // TokenFromRid
        //
        /// <summary>
        /// インデックスとトークンタイプのビット和を返す。
        /// </summary>
        //------------------------------------------------------------
        //#define TokenFromRid(rid,tktype) ((rid) | (tktype))
        static internal int TokenFromRid(int rid, int tktype) { return (int)(rid | tktype); }

        //------------------------------------------------------------
        // RidFromToken
        //------------------------------------------------------------
        //#define RidFromToken(tk) ((RID) ((tk) & 0x00ffffff))
        static internal int RidFromToken(int tk) { return (int)(tk & 0x00FFFFFF); }

        //------------------------------------------------------------
        // TypeFromToken
        //------------------------------------------------------------
        //#define TypeFromToken(tk) ((ULONG32)((tk) & 0xff000000))
        static internal int TypeFromToken(int tk) { return (int)(tk & 0xff000000); }
        static internal int TypeFromToken(TOKENID tk) { return (int)((int)tk & 0xff000000); }

        //------------------------------------------------------------
        // IsNilToken
        //------------------------------------------------------------
        //#define IsNilToken(tk) ((RidFromToken(tk)) == 0)
        static internal bool IsNilToken(int tk) { return (RidFromToken(tk) == 0); }
#endif

        internal const int mdTokenNil = ((int)0);
        //internal const int mdModuleNil = ((int)mdtModule);
        //internal const int mdTypeRefNil = ((int)mdtTypeRef);
        //internal const int mdTypeDefNil = ((int)mdtTypeDef);
        //internal const int mdFieldDefNil = ((int)mdtFieldDef);
        //internal const int mdMethodDefNil = ((int)mdtMethodDef);
        //internal const int mdParamDefNil = ((int)mdtParamDef);
        //internal const int mdInterfaceImplNil = ((int)mdtInterfaceImpl);
        //internal const int mdMemberRefNil = ((int)mdtMemberRef);
        //internal const int mdCustomAttributeNil = ((int)mdtCustomAttribute);
        //internal const int mdPermissionNil = ((int)mdtPermission);
        //internal const int mdSignatureNil = ((int)mdtSignature);
        //internal const int mdEventNil = ((int)mdtEvent);
        //internal const int mdPropertyNil = ((int)mdtProperty);
        //internal const int mdModuleRefNil = ((int)mdtModuleRef);
        //internal const int mdTypeSpecNil = ((int)mdtTypeSpec);
        internal const int mdAssemblyNil = ((int)mdtAssembly);
        //internal const int mdAssemblyRefNil = ((int)mdtAssemblyRef);
        //internal const int mdFileNil = ((int)mdtFile);
        //internal const int mdExportedTypeNil = ((int)mdtExportedType);
        //internal const int mdManifestResourceNil = ((int)mdtManifestResource);

        //internal const int mdGenericParamNil = ((int)mdtGenericParam);
        //internal const int mdGenericParamConstraintNil = ((int)mdtGenericParamConstraint);
        //internal const int mdMethodSpecNil = ((int)mdtMethodSpec);

        //internal const int mdStringNil = ((int)mdtString);
    }

    //======================================================================
    // Opaque type for an enumeration handle.
    //======================================================================
    //internal class HCORENUM : System.Object { }   //typedef void *HCORENUM;

    static internal partial class Cor
    {
        internal const AttributeTargets AttributeTargetClassMembers =
            AttributeTargets.Class |
            AttributeTargets.Struct |
            AttributeTargets.Enum |
            AttributeTargets.Constructor |
            AttributeTargets.Method |
            AttributeTargets.Property |
            AttributeTargets.Field |
            AttributeTargets.Event |
            AttributeTargets.Delegate |
            AttributeTargets.Interface;
    }
}
