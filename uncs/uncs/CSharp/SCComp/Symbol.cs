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
// File: symbol.h
//
// Defines the shapes of various symbols in the symbol table.
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
// File: symmgr.cpp
//
// Routines for storing and handling symbols.
// ===========================================================================

//============================================================================
// Symbol.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.CSharp.RuntimeBinder;

namespace Uncs
{
    //======================================================================
    // class Kaid (kaid)
    //
    /// <summary>
    /// <para>Alias ID's are indices into BitSets.</para>
    /// <list type="bullet">
    /// <item><description>0 is reserved for the global namespace alias.</description></item>
    /// <item><description>1 is reserved for this assembly.</description></item>
    /// <item><description>Start assigning at kaidStartAssigning.</description></item>
    /// </list>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    /// <remarks>
    /// In sscli source, enum without name, each member has prefix "kaid" 
    /// </remarks>
    //======================================================================
    static internal class Kaid
    {
        /// <summary>
        /// = -1
        /// </summary>
        internal const int Nil = -1;

        /// <summary>
        /// = 0
        /// </summary>
        internal const int Global = 0;

        /// <summary>
        /// = 1
        /// </summary>
        internal const int ErrorAssem = 1;

        /// <summary>
        /// = 2
        /// </summary>
        internal const int ThisAssembly = 2;

        /// <summary>
        /// = 3
        /// </summary>
        internal const int Unresolved = 3;

        /// <summary>
        /// = 4
        /// </summary>
        internal const int StartAssigning = 4;

        /// <summary>
        /// <para>Module id's are in their own range.</para>
        /// <para>= 0x10000000</para>
        /// </summary>
        internal const int MinModule = 0x10000000;
    }

    //======================================================================
    // Number of simple types in predeftype.h. You must
    // change appropriate conversion table definitions
    // if this changes.
    //======================================================================
    static internal partial class SymbolUtil
    {
        internal const int NUM_SIMPLE_TYPES = (int)PREDEFTYPE.INTPTR;   // 14;
        internal const int NUM_QSIMPLE_TYPES = 4;
        internal const int NUM_EXT_TYPES = 15;
    }

    namespace AUTOEXP
    {
        internal class ATTRIBUTE { }
    }

    //======================================================================
    // FUNDTYPE (FT_)
    //
    /// <summary>
    /// Fundemental types.
    /// These are the fundemental storage types that are available and used by the code generator.
    /// (CSharp\SCComp\Symbol.cs)
    /// </summary>
    //======================================================================
    internal enum FUNDTYPE : int
    {
        /// <summary>
        /// No fundemental type
        /// </summary>
        NONE,

        // integral types

        I1,
        I2,
        I4,
        U1,
        U2,
        U4,

        /// <summary>
        /// Last one that fits in a int.
        /// </summary>
        LASTNONLONG = U4,
        
        I8,

        U8,
        LASTINTEGRAL = U8,

        // floating types

        R4,
        R8,
        LASTNUMERIC = R8,

        /// <summary>
        /// reference type
        /// </summary>
        REF,

        /// <summary>
        /// structure type
        /// </summary>
        STRUCT,

        /// <summary>
        /// pointer to unmanaged memory
        /// </summary>
        PTR,

        /// <summary>
        /// polymorphic, unbounded, not yet committed
        /// </summary>
        VAR,

        /// <summary>
        /// number of enumerators.
        /// </summary>
        COUNT
    }

    //======================================================================
    // ACCESS   (ACC_)
    //
    /// <summary>
    /// <para>Define the different access levels that symbols can have.</para>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal enum ACCESS : int
    {
        UNKNOWN = 0,    // Not yet determined.
        PRIVATE,
        INTERNAL,
        PROTECTED,
        INTERNALPROTECTED,  // internal OR protected
        PUBLIC
    }

    // #include "symkind.h"

    //======================================================================
    // Forward declare all symbol types.
    //======================================================================
    // #define SYMBOLDEF(kind, global, local) typedef class kind * P ## kind;
    // #include "symkinds.h"

    // typedef __int64 symbmask_t;

    //======================================================================
    // SYMBMASK (MASK_)
    //
    /// <summary>
    /// <para>Define values for symbol masks.
    /// (CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    // #define SYMBOLDEF(kind, global, local) const symbmask_t MASK_ ## kind = (((symbmask_t)1) << SK_ ## kind);
    // #define SYMBOLDEF_EXTRA(kind, global, local)
    // #include "symkinds.h"

    [Flags]
    internal enum SYMBMASK : ulong
    {
        // Namespace
        NSSYM = (((ulong)1) << SYMKIND.NSSYM),
        NSDECLSYM = (((ulong)1) << SYMKIND.NSDECLSYM),
        NSAIDSYM = (((ulong)1) << SYMKIND.NSAIDSYM),

        // Aggregate
        AGGSYM = (((ulong)1) << SYMKIND.AGGSYM),
        AGGDECLSYM = (((ulong)1) << SYMKIND.AGGDECLSYM),
        AGGTYPESYM = (((ulong)1) << SYMKIND.AGGTYPESYM),
        FWDAGGSYM = (((ulong)1) << SYMKIND.FWDAGGSYM),

        // "Members" of aggs.
        TYVARSYM = (((ulong)1) << SYMKIND.TYVARSYM),
        MEMBVARSYM = (((ulong)1) << SYMKIND.MEMBVARSYM),
        LOCVARSYM = (((ulong)1) << SYMKIND.LOCVARSYM),
        METHSYM = (((ulong)1) << SYMKIND.METHSYM),
        FAKEMETHSYM = (((ulong)1) << SYMKIND.FAKEMETHSYM), // this has to be immediately after METHSYM
        PROPSYM = (((ulong)1) << SYMKIND.PROPSYM),
        EVENTSYM = (((ulong)1) << SYMKIND.EVENTSYM),

        // Primitive types.
        VOIDSYM = (((ulong)1) << SYMKIND.VOIDSYM),
        NULLSYM = (((ulong)1) << SYMKIND.NULLSYM),
        UNITSYM = (((ulong)1) << SYMKIND.UNITSYM),
        ANONMETHSYM = (((ulong)1) << SYMKIND.ANONMETHSYM),
        METHGRPSYM = (((ulong)1) << SYMKIND.METHGRPSYM),
        ERRORSYM = (((ulong)1) << SYMKIND.ERRORSYM),

        // Derived types - Parent is the base type.
        ARRAYSYM = (((ulong)1) << SYMKIND.ARRAYSYM),
        PTRSYM = (((ulong)1) << SYMKIND.PTRSYM),
        PINNEDSYM = (((ulong)1) << SYMKIND.PINNEDSYM),
        PARAMMODSYM = (((ulong)1) << SYMKIND.PARAMMODSYM),
        MODOPTSYM = (((ulong)1) << SYMKIND.MODOPTSYM),
        MODOPTTYPESYM = (((ulong)1) << SYMKIND.MODOPTTYPESYM),
        NUBSYM = (((ulong)1) << SYMKIND.NUBSYM), // Nullable type as a "derived" type - the parent is the base type.

        // Files
        INFILESYM = (((ulong)1) << SYMKIND.INFILESYM),
        MODULESYM = (((ulong)1) << SYMKIND.MODULESYM),
        RESFILESYM = (((ulong)1) << SYMKIND.RESFILESYM),
        OUTFILESYM = (((ulong)1) << SYMKIND.OUTFILESYM),
        XMLFILESYM = (((ulong)1) << SYMKIND.XMLFILESYM),
        SYNTHINFILESYM = (((ulong)1) << SYMKIND.SYNTHINFILESYM),

        // Aliases
        ALIASSYM = (((ulong)1) << SYMKIND.ALIASSYM),
        EXTERNALIASSYM = (((ulong)1) << SYMKIND.EXTERNALIASSYM),

        // Other
        SCOPESYM = (((ulong)1) << SYMKIND.SCOPESYM),
        CACHESYM = (((ulong)1) << SYMKIND.CACHESYM),
        LABELSYM = (((ulong)1) << SYMKIND.LABELSYM),
        MISCSYM = (((ulong)1) << SYMKIND.MISCSYM),
        GLOBALATTRSYM = (((ulong)1) << SYMKIND.GLOBALATTRSYM),
        ANONSCOPESYM = (((ulong)1) << SYMKIND.ANONSCOPESYM),

        //  SYMBOLDEF_EXTRA // マクロで処理    使用しない

        // CS3
        IMPLICITTYPESYM = (((ulong)1) << SYMKIND.IMPLICITTYPESYM),
        IMPLICITLYTYPEDARRAYSYM = (((ulong)1) << SYMKIND.IMPLICITLYTYPEDARRAYSYM),
        LAMBDAEXPRSYM = (((ulong)1) << SYMKIND.LAMBDAEXPRSYM),

        // CS4
        DYNAMICSYM = (((ulong)1) << SYMKIND.DYNAMICSYM),

        // const symbmask_t MASK_ALL = ~(MASK_FAKEMETHSYM | MASK_FWDAGGSYM);
        ALL = ~(FAKEMETHSYM | FWDAGGSYM),

        /// <summary>
        /// <para>In sscli, LOOKUPMASK is a macro defined in fncbind.h</para>
        /// </summary>
        LOOKUPMASK = (
           SYMBMASK.NSAIDSYM |
           SYMBMASK.AGGTYPESYM |
           SYMBMASK.NUBSYM |
           SYMBMASK.MEMBVARSYM |
           SYMBMASK.TYVARSYM |
           SYMBMASK.LOCVARSYM |
           SYMBMASK.METHSYM |
           SYMBMASK.PROPSYM),
    }

    //======================================================================
    // Typedefs for some pointer types up front.
    //======================================================================
    // typedef class SYM * PSYM;
    // typedef class PARENTSYM * PPARENTSYM;
    // typedef class TYPESYM * PTYPESYM;

    //internal class METHODNODE { }
    //internal class BASENODE { }
    //internal class CSourceData { }

    //======================================================================
    // SYMLIST - a list of symbols.
    //
    // struct SYMLIST {
    //     PSYM sym;
    //     SYMLIST * next;
    // 
    //     bool contains(PSYM sym);
    // };
    //======================================================================
    // Use System.Collections.Generic.List<SYM> for SYMLIST.

    //======================================================================
    // NAMELIST - a list of names.
    //
    // struct NAMELIST {
    //     PNAME name;
    //     NAMELIST * next;
    //
    //     bool contains(PNAME name);
    // };
    //======================================================================
    // Use string for NAME, System.Collections.Generic.List<string> for NAMELIST.

    //======================================================================
    // class ATTRLIST
    //
    // ATTRLIST - a list of attribute nodes and their context (i.e. decl).
    //
    // struct ATTRLIST {
    //     BASENODE    *attr;
    //     PARENTSYM   *context;    // Context of this attribute (AGGDECL for agg tyvars, METHSYM for method type variables)
    //     bool         fHadError;  // whether or not there was an error binding this attribute
    //     ATTRLIST    *next;
    // };
    //======================================================================
    // Use List<ATTRINFO> for ATTRLIST.

    //======================================================================
    // class ATTRINFO
    //
    /// <summary>
    /// <para>Use in place of ATTRLIST in sscli.
    /// Create an attribute list by List&lt;ATTRINFO&gt;.</para>
    /// <para>Has an attribute (BASENODE) and its context (PARENTSYM).</para>
    /// </summary>
    //======================================================================
    internal class ATTRINFO
    {
        internal BASENODE AttributeNode = null; // attr

        /// <summary>
        /// Context of this attribute (AGGDECL for agg tyvars, METHSYM for method type variables)
        /// </summary>
        internal PARENTSYM ContextSym = null;   // context

        /// <summary>
        /// whether or not there was an error binding this attribute
        /// </summary>
        internal bool HadError = false;         // fHadError

        //--------------------------------------------------
        // ATTRINFO Constructor
        //--------------------------------------------------
        internal ATTRINFO() { }
        internal ATTRINFO(BASENODE attr, PARENTSYM context)
        {
            this.AttributeNode = attr;
            this.ContextSym = context;
        }
    }

    //// used to give compile time errors when asking a symbol for something which
    //// it can never be
    //#define NOT_A(k) \
    //    void as ## k(); \
    //    void is ## k();
    //
    //// used to give a compile time error when asking a  symbol
    //// to be something which you already know it is
    //#define IS_A(k) \
    //    void as ## k(); \
    //    void is ## k();

    //======================================================================
    // enum AggStateEnum
    //
    /// <summary>
    /// enum of compiling states of aggrigate types.
    /// (CSharp\SSComp\Symbol.cs)
    /// </summary>
    //======================================================================
    internal enum AggStateEnum : int
    {
        None,

        /// <summary>
        /// <para>The symbol of this aggregate type was created.</para>
        /// <para>The access modifier was checked.</para>
        /// <para>The symbols of this type variables were created and set to this type.</para>
        /// <para>The nested types were similarly processed.</para>
        /// </summary>
        Declared,

        /// <summary>
        /// <para>In ResolveInheritanceRec.</para>
        /// </summary>
        ResolvingInheritance,

        /// <summary>
        /// <para>Done with ResolveInheritanceRec.</para>
        /// <para>The symbols ofits base class and interfaces were set.</para>
        /// <para>If this agg is nested, the outer agg was processed first.</para>
        /// <para>If its includes some aggs, they were also processed.</para>
        /// </summary>
        Inheritance,

        Bounds,

        DefiningMembers,
        DefinedMembers,

        /// <summary>
        /// Preparing base types of this AGG
        /// </summary>
        Preparing,

        /// <summary>
        /// Base types are prepared
        /// </summary>
        Prepared,

        /// <summary>
        /// Members are prepared
        /// </summary>
        PreparedMembers,

        Compiled,

        /// <summary>
        /// = AggStateEnum.Compiled
        /// </summary>
        Last = Compiled,

        Lim
    }

    //======================================================================
    // class SYM
    //
    /// <summary></summary>
    //======================================================================
    internal class SYM
    {
        //------------------------------------------------------------
        // Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// (R) Index starting from 1 assigned when a SYM instance is created.
        /// </summary>
        internal readonly int SymID = CObjectID.GenerateID();

        /// <summary>
        /// the symbol kind
        /// </summary>
        internal SYMKIND Kind = SYMKIND.UNDEFINED;  // uint kind: 8;

        /// <summary>
        /// Set Kind-th bit and return it. if Kind is not set, return 0.
        /// </summary>
        internal SYMBMASK Mask // mask()
        {
            get
            {
                //return (SYMBMASK)(IsValid(Kind) ? ((ulong)1 << (int)Kind) : 0);
                return (SYMBMASK)((Kind >= 0 && Kind < SYMKIND.LIM) ? ((ulong)1 << (int)Kind) : 0);
            }
        }

        /// <summary>
        /// <para>can't be used in our language -- unsupported type(s)</para>
        /// <para>Use SetBogus(bool bogus) method to set the value.</para>
        /// </summary>
        internal bool IsBogus = false;   // uint isBogus: 1;

        /// <summary>
        /// <para>Have we checked a method args/return for bogus types</para>
        /// <para>Use SetBogus(bool bogus) method to set the value.</para>
        /// </summary>
        internal bool HasBogus = false;  // uint checkedBogus: 1; hasBogus()

        internal void SetBogus(bool bogus)
        {
            IsBogus = bogus;
            HasBogus = true;
        }

        /// <summary>
        /// symbol is deprecated and should give a warning when used
        /// </summary>
        private bool isDeprecated = false;  // uint isDeprecated: 1;

        /// <summary>
        /// If isDeprecated, indicates error rather than warning
        /// </summary>
        private bool isDeprecatedError = false; // uint isDeprecatedError: 1;

        /// <summary>
        /// access level
        /// </summary>
        internal ACCESS Access = ACCESS.UNKNOWN;

        /// <summary>
        /// a local or global symbol?
        /// </summary>
        internal bool IsLocal = false;  // uint isLocal: 1;

        /// <summary>
        /// Whether the symbol was given an "ID" - used for testing refactoring support
        /// </summary>
        internal bool HasSID = false;   // uint fHasSid: 1;

        /// <summary>
        /// Used for testing refactoring
        /// </summary>
        internal bool IsDumped = false; // uint fDumped: 1;

        /// <summary>
        /// True iff this symbol has an attribute
        /// </summary>
        internal bool HasCLSAttribute = false;  // uint hasCLSattribute: 1;

        /// <summary>
        /// if hasCLSattribute, indicates whether symbol is CLS compliant
        /// </summary>
        internal bool IsCLS = false;    // uint isCLS: 1;

        /// <summary>
        /// If this is true, then this sym has been removed from the symbol table
        /// so will not be returned from LookupSym
        /// </summary>
        internal bool IsDead = false;   // uint isDead: 1;

        /// <summary>
        /// Is compiler generated
        /// </summary>
        internal bool IsFabricated = false; // uint isFabricated: 1;

        /// <summary>
        /// set for symbols where we check attributes twice
        /// (i.e. parameters for indexers or delegates, CLSCompliance in EarlyAttrBind, etc.  
        /// If this is true, then we had an error the first time
        /// so do not give an error the second time.
        /// </summary>
        internal bool HadAttributeError = false;    // uint fHadAttributeError: 1;

        //------------------------------------------------------------
        // Fields and Properties
        //
        // Name and SearchName
        //------------------------------------------------------------

        /// <summary>
        /// name of the symbol
        /// SYM uses symbolName as Name and SearchName.
        /// </summary>
        protected string symbolName = null; // PNAME name;

        /// <summary>
        /// <para>(RW) name of the symbol</para>
        /// <para>Use SetName method to set this name.</para>
        /// </summary>
        internal virtual string Name
        {
            get { return symbolName; }
        }

        /// <summary>
        /// Set SYM.symbolName.
        /// </summary>
        /// <param name="name"></param>
        internal virtual void SetName(string name)
        {
            this.symbolName = name;
        }

        /// <summary>
        /// (R) name for SYMTBL.
        /// </summary>
        internal virtual string SearchName
        {
            get { return symbolName; }
        }

        /// <summary>
        /// Set symbolName. (in SYM)
        /// </summary>
        /// <param name="name"></param>
        internal virtual void SetSearchName(string name)
        {
            this.symbolName = name;
        }

        //------------------------------------------------------------
        // Fields and Properties
        //
        // Parent and siblings
        //------------------------------------------------------------

        /// <summary>
        /// parent of the symbol
        /// </summary>
        internal PARENTSYM ParentSym = null;    // PPARENTSYM parent;

        /// <summary>
        /// next child of this parent
        /// </summary>
        internal SYM NextSym = null;    // PSYM nextChild;

        /// <summary>
        /// next child of this parent with same name.
        /// </summary>
        internal SYM NextSameNameSym = null;    // PSYM nextSameName;

        internal SYM PrevSameNameSym = null;

        internal SYM LastSameNameSym = null;

        // We have member functions here to do casts that, in DEBUG, check the
        // symbol kind to make sure it is right. For example, the casting method
        // for METHODSYM is called "asMETHODSYM".
        //
        // Define all the concrete casting methods.
        // We define them explicitly so that VC's ide knows about them

        // used for FOREACHSYMLIST(list, elem, SYM)

        //------------------------------------------------------------
        // SYM As* Properties
        //------------------------------------------------------------
        //internal SYM AsSYM
        //{
        //    get { return this; }
        //}
        //internal BAGSYM AsBAGSYM
        //{
        //    get { return this as BAGSYM; }
        //}
        //internal DECLSYM AsDECLSYM
        //{
        //    get { return this as DECLSYM; }
        //}
        //internal NSSYM AsNSSYM
        //{
        //    get { return this as NSSYM; }
        //}
        //internal NSAIDSYM AsNSAIDSYM
        //{
        //    get { return this as NSAIDSYM; }
        //}
        //internal NSDECLSYM AsNSDECLSYM
        //{
        //    get { return this as NSDECLSYM; }
        //}
        //internal AGGSYM AsAGGSYM
        //{
        //    get { return this as AGGSYM; }
        //}
        //internal AGGDECLSYM AsAGGDECLSYM
        //{
        //    get { return this as AGGDECLSYM; }
        //}
        //internal FWDAGGSYM AsFWDAGGSYM
        //{
        //    get { return this as FWDAGGSYM; }
        //}
        //internal INFILESYM AsINFILESYM
        //{
        //    get { return this as INFILESYM; }
        //}
        //internal MODULESYM AsMODULESYM
        //{
        //    get { return this as MODULESYM; }
        //}
        //internal RESFILESYM AsRESFILESYM
        //{
        //    get { return this as RESFILESYM; }
        //}
        //internal OUTFILESYM AsOUTFILESYM
        //{
        //    get { return this as OUTFILESYM; }
        //}
        //internal MEMBVARSYM AsMEMBVARSYM
        //{
        //    get { return this as MEMBVARSYM; }
        //}
        //internal LOCVARSYM AsLOCVARSYM
        //{
        //    get { return this as LOCVARSYM; }
        //}
        //internal METHSYM AsMETHSYM
        //{
        //    get { return this as METHSYM; }
        //}
        //internal FAKEMETHSYM AsFAKEMETHSYM
        //{
        //    get { return this as FAKEMETHSYM; }
        //}
        //internal PROPSYM AsPROPSYM
        //{
        //    get { return this as PROPSYM; }
        //}
        //internal METHPROPSYM AsMETHPROPSYM
        //{
        //    get { return this as METHPROPSYM; }
        //}
        //internal SCOPESYM AsSCOPESYM
        //{
        //    get { return this as SCOPESYM; }
        //}
        //internal ANONSCOPESYM AsANONSCOPESYM
        //{
        //    get { return this as ANONSCOPESYM; }
        //}
        //internal ARRAYSYM AsARRAYSYM
        //{
        //    get { return this as ARRAYSYM; }
        //}
        //internal PTRSYM AsPTRSYM
        //{
        //    get { return this as PTRSYM; }
        //}
        //internal NUBSYM AsNUBSYM
        //{
        //    get { return this as NUBSYM; }
        //}
        //internal TYVARSYM AsTYVARSYM
        //{
        //    get { return this as TYVARSYM; }
        //}
        //internal AGGTYPESYM AsAGGTYPESYM
        //{
        //    get { return this as AGGTYPESYM; }
        //}
        //internal PINNEDSYM AsPINNEDSYM
        //{
        //    get { return this as PINNEDSYM; }
        //}
        //internal PARAMMODSYM AsPARAMMODSYM
        //{
        //    get { return this as PARAMMODSYM; }
        //}
        //internal MODOPTSYM AsMODOPTSYM
        //{
        //    get { return this as MODOPTSYM; }
        //}
        //internal MODOPTTYPESYM AsMODOPTTYPESYM
        //{
        //    get { return this as MODOPTTYPESYM; }
        //}
        //internal VOIDSYM AsVOIDSYM
        //{
        //    get { return this as VOIDSYM; }
        //}
        //internal NULLSYM AsNULLSYM
        //{
        //    get { return this as NULLSYM; }
        //}
        //internal UNITSYM AsUNITSYM
        //{
        //    get { return this as UNITSYM; }
        //}
        //internal ANONMETHSYM AsANONMETHSYM
        //{
        //    get { return this as ANONMETHSYM; }
        //}
        //internal METHGRPSYM AsMETHGRPSYM
        //{
        //    get { return this as METHGRPSYM; }
        //}
        //internal CACHESYM AsCACHESYM
        //{
        //    get { return this as CACHESYM; }
        //}
        //internal LABELSYM AsLABELSYM
        //{
        //    get { return this as LABELSYM; }
        //}
        //internal ERRORSYM AsERRORSYM
        //{
        //    get { return this as ERRORSYM; }
        //}
        //internal ALIASSYM AsALIASSYM
        //{
        //    get { return this as ALIASSYM; }
        //}
        //internal GLOBALATTRSYM AsGLOBALATTRSYM
        //{
        //    get { return this as GLOBALATTRSYM; }
        //}
        //internal EVENTSYM AsEVENTSYM
        //{
        //    get { return this as EVENTSYM; }
        //}
        //internal XMLFILESYM AsXMLFILESYM
        //{
        //    get { return this as XMLFILESYM; }
        //}
        //internal SYNTHINFILESYM AsSYNTHINFILESYM
        //{
        //    get { return this as SYNTHINFILESYM; }
        //}
        //internal MISCSYM AsMISCSYM
        //{
        //    get { return this as MISCSYM; }
        //}
        //internal EXTERNALIASSYM AsEXTERNALIASSYM
        //{
        //    get { return this as EXTERNALIASSYM; }
        //}

        // Define the ones for the abstract es.

        //internal PARENTSYM AsPARENTSYM
        //{
        //    get { return this as PARENTSYM; }
        //}
        //internal TYPESYM AsTYPESYM
        //{
        //    get { return this as TYPESYM; }
        //}
        //internal VARSYM AsVARSYM
        //{
        //    get { return this as VARSYM; }
        //}

        //------------------------------------------------------------
        // Define the ones which traverse sub relationships:
        //------------------------------------------------------------

        internal METHSYM AsFMETHSYM
        {
            get { return (IsFMETHSYM ? (this as METHSYM) : null); }
        }

        internal INFILESYM AsANYINFILESYM
        {
            get
            {
                if (!this.IsANYINFILESYM)
                {
                    return null; // RETAILVERIFY
                }
                return this as INFILESYM;
            }
        }

        //------------------------------------------------------------
        // type testing
        //------------------------------------------------------------
        internal bool IsBAGSYM
        {
            get { return (this.IsAGGSYM || this.IsNSSYM); }
        }
        internal bool IsDECLSYM
        {
            get { return (this.IsAGGDECLSYM || this.IsNSDECLSYM); }
        }
        internal bool IsNSSYM
        {
            get { return (this.Kind == SYMKIND.NSSYM); }
        }
        internal bool IsNSAIDSYM
        {
            get { return (this.Kind == SYMKIND.NSAIDSYM); }
        }
        internal bool IsNSDECLSYM
        {
            get { return (this.Kind == SYMKIND.NSDECLSYM); }
        }
        internal bool IsAGGSYM
        {
            get { return (this.Kind == SYMKIND.AGGSYM); }
        }
        internal bool IsAGGDECLSYM
        {
            get { return (this.Kind == SYMKIND.AGGDECLSYM); }
        }
        internal bool IsFWDAGGSYM
        {
            get { return (this.Kind == SYMKIND.FWDAGGSYM); }
        }
        internal bool IsINFILESYM
        {
            get { return (this.Kind == SYMKIND.INFILESYM); }
        }
        internal bool IsMODULESYM
        {
            get { return (this.Kind == SYMKIND.MODULESYM); }
        }
        internal bool IsRESFILESYM
        {
            get { return (this.Kind == SYMKIND.RESFILESYM); }
        }
        internal bool IsOUTFILESYM
        {
            get { return (this.Kind == SYMKIND.OUTFILESYM); }
        }
        internal bool IsMEMBVARSYM
        {
            get { return (this.Kind == SYMKIND.MEMBVARSYM); }
        }
        internal bool IsLOCVARSYM
        {
            get { return (this.Kind == SYMKIND.LOCVARSYM); }
        }
        internal bool IsMETHSYM
        {
            get { return (this.Kind == SYMKIND.METHSYM); }
        }
        internal bool IsFAKEMETHSYM
        {
            get { return (this.Kind == SYMKIND.FAKEMETHSYM); }
        }
        internal bool IsPROPSYM
        {
            get { return (this.Kind == SYMKIND.PROPSYM); }
        }
        internal bool IsMETHPROPSYM
        {
            get { return (this.IsMETHSYM || this.IsPROPSYM || this.IsFAKEMETHSYM); }
        }
        internal bool IsSCOPESYM
        {
            get { return (this.Kind == SYMKIND.SCOPESYM); }
        }
        internal bool IsANONSCOPESYM
        {
            get { return (this.Kind == SYMKIND.ANONSCOPESYM); }
        }
        internal bool IsARRAYSYM
        {
            get { return (this.Kind == SYMKIND.ARRAYSYM); }
        }
        internal bool IsPTRSYM
        {
            get { return (this.Kind == SYMKIND.PTRSYM); }
        }
        internal bool IsNUBSYM
        {
            get { return (this.Kind == SYMKIND.NUBSYM); }
        }
        internal bool IsTYVARSYM
        {
            get { return (this.Kind == SYMKIND.TYVARSYM); }
        }
        internal bool IsAGGTYPESYM
        {
            get
            {
                return (
                  this.Kind == SYMKIND.AGGTYPESYM ||
                  this.Kind == SYMKIND.DYNAMICSYM);     // CS4
            }
        }
        internal bool IsPINNEDSYM
        {
            get { return (this.Kind == SYMKIND.PINNEDSYM); }
        }
        internal bool IsPARAMMODSYM
        {
            get { return (this.Kind == SYMKIND.PARAMMODSYM); }
        }
        internal bool IsMODOPTSYM
        {
            get { return (this.Kind == SYMKIND.MODOPTSYM); }
        }
        internal bool IsMODOPTTYPESYM
        {
            get { return (this.Kind == SYMKIND.MODOPTTYPESYM); }
        }
        internal bool IsVOIDSYM
        {
            get { return (this.Kind == SYMKIND.VOIDSYM); }
        }
        internal bool IsNULLSYM
        {
            get { return (this.Kind == SYMKIND.NULLSYM); }
        }
        internal bool IsUNITSYM
        {
            get { return (this.Kind == SYMKIND.UNITSYM); }
        }
        internal bool IsANONMETHSYM
        {
            get { return (this.Kind == SYMKIND.ANONMETHSYM); }
        }
        internal bool IsMETHGRPSYM
        {
            get { return (this.Kind == SYMKIND.METHGRPSYM); }
        }
        internal bool IsCACHESYM
        {
            get { return (this.Kind == SYMKIND.CACHESYM); }
        }
        internal bool IsLABELSYM
        {
            get { return (this.Kind == SYMKIND.LABELSYM); }
        }
        internal bool IsERRORSYM
        {
            get { return (this.Kind == SYMKIND.ERRORSYM); }
        }
        internal bool IsALIASSYM
        {
            get { return (this.Kind == SYMKIND.ALIASSYM); }
        }
        internal bool IsGLOBALATTRSYM
        {
            get { return (this.Kind == SYMKIND.GLOBALATTRSYM); }
        }
        internal bool IsEVENTSYM
        {
            get { return (this.Kind == SYMKIND.EVENTSYM); }
        }

        //internal bool isIFACEIMPLMETHSYM();   // No definition in sscli.

        internal bool IsXMLFILESYM
        {
            get { return (this.Kind == SYMKIND.XMLFILESYM); }
        }
        internal bool IsSYNTHINFILESYM
        {
            get { return (this.Kind == SYMKIND.SYNTHINFILESYM); }
        }
        internal bool IsMISCSYM
        {
            get { return (this.Kind == SYMKIND.MISCSYM); }
        }
        internal bool IsEXTERNALIASSYM
        {
            get { return (this.Kind == SYMKIND.EXTERNALIASSYM); }
        }

        // CS3
        internal bool IsIMPLICITTYPESYM
        {
            get { return (this.Kind == SYMKIND.IMPLICITTYPESYM); }
        }
        internal bool IsIMPLICITLYTYPEDARRAY
        {
            get { return (this.Kind == SYMKIND.IMPLICITLYTYPEDARRAYSYM); }
        }
        internal bool IsLAMBDAEXPRSYM
        {
            get { return (this.Kind == SYMKIND.LAMBDAEXPRSYM); }
        }

        // CS4
        internal bool IsDYNAMICSYM
        {
            get { return (this.Kind == SYMKIND.DYNAMICSYM); }
        }

        // Define the ones for the abstract classes.

        internal bool IsPARENTSYM
        {
            get
            {
                return (
                    this.IsBAGSYM ||
                    this.IsDECLSYM ||
                    this.IsTYPESYM ||
                    this.IsNSAIDSYM ||
                    this.IsSCOPESYM ||
                    this.IsOUTFILESYM ||
                    this.IsARRAYSYM ||
                    this.IsMETHSYM ||
                    this.IsFAKEMETHSYM);
            }
        }

        /// <summary>
        /// <para>True if type of this derives from TYPESYM.</para>
        /// <para>False for TYPESYM itself.</para>
        /// </summary>
        internal bool IsTYPESYM
        {
            get
            {
                switch (this.Kind)
                {
                    case SYMKIND.AGGTYPESYM:
                    case SYMKIND.ARRAYSYM:
                    case SYMKIND.VOIDSYM:
                    case SYMKIND.PARAMMODSYM:
                    case SYMKIND.TYVARSYM:
                    case SYMKIND.PTRSYM:
                    case SYMKIND.NUBSYM:
                    case SYMKIND.NULLSYM:
                    case SYMKIND.ERRORSYM:
                    case SYMKIND.MODOPTTYPESYM:
                    case SYMKIND.ANONMETHSYM:
                    case SYMKIND.METHGRPSYM:
                    case SYMKIND.UNITSYM:
                    case SYMKIND.IMPLICITTYPESYM:           // CS3
                    case SYMKIND.IMPLICITLYTYPEDARRAYSYM:   // CS3
                    case SYMKIND.LAMBDAEXPRSYM:             // CS3
                    case SYMKIND.DYNAMICSYM:                // CS4
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// True if MEMBVARSYM or LOCVARSYM
        /// </summary>
        internal bool IsVARSYM
        {
            get
            {
                return (this.IsMEMBVARSYM || this.IsLOCVARSYM);
            }
        }

        // Define the ones which traverse subclass relationships:

        /// <summary>
        /// True if METHSYM or FAKEMETHSYM.
        /// </summary>
        internal bool IsFMETHSYM
        {
            get
            {
                return (this.IsMETHSYM || this.IsFAKEMETHSYM);
            }
        }

        /// <summary>
        /// True if INFILESYM or SYNTHINFILESYM.
        /// </summary>
        internal bool IsANYINFILESYM
        {
            get
            {
                return (this.IsINFILESYM || this.IsSYNTHINFILESYM);
            }
        }

        /// <summary>
        /// True if VOIDSYM or TYPESYM of void.
        /// </summary>
        internal bool IsVoidType
        {
            get
            {
                return (this.IsVOIDSYM ||
                    (this.IsTYPESYM &&
                    !this.IsTYVARSYM &&
                    (this as TYPESYM).Type == typeof(void)));
            }
        }

        //------------------------------------------------------------
        // IsValid(SYMKIND)
        //
        /// <summary>
        /// SYMKIND 値が有効な値か調べる。
        /// </summary>
        //------------------------------------------------------------
        static internal bool IsValid(SYMKIND kind)
        {
            return (kind >= 0 && kind < SYMKIND.LIM);
        }

        //------------------------------------------------------------
        // Allocate zeroed from a no-release allocator.
        // In C#, cannot overload new operator.
        //------------------------------------------------------------
        //void * operator new(size_t sz, NRHEAP * allocator, int * psidLast) { }

        //------------------------------------------------------------
        // SYM Constructor
        //------------------------------------------------------------
        internal SYM()
        {
#if DEBUG
            DebugUtil.DebugSymsAdd(this);
            if (SymID == 3860)
            {
                string name = this.Name;
            }
#endif
        }

        //------------------------------------------------------------
        // SYM.copyInto
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal void copyInto(SYM sym)
        {
            sym.Access = this.Access;
            sym.IsBogus = this.IsBogus;
            sym.HasBogus = this.HasBogus;
        }

        //------------------------------------------------------------
        // SYM.HasExternalAccess
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasExternalAccess()
        {
            DebugUtil.Assert(ParentSym != null || IsNSSYM || IsNSDECLSYM);

            if (IsAGGDECLSYM)
            {
                return (this as AGGDECLSYM).AggSym.HasExternalAccess();
            }

            return (
                IsNSSYM ||
                IsNSDECLSYM ||
                ((int)Access >= (int)ACCESS.PROTECTED && ParentSym.HasExternalAccess()));
        }

        //------------------------------------------------------------
        // SYM.HasExternalOrFriendAccess
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasExternalOrFriendAccess()
        {
            DebugUtil.Assert(ParentSym != null || IsNSSYM || IsNSDECLSYM);

            if (IsAGGDECLSYM)
            {
                return (this as AGGDECLSYM).AggSym.HasExternalOrFriendAccess();
            }

            return (
                IsNSSYM ||
                IsNSDECLSYM ||
                ((int)Access >= (int)ACCESS.INTERNAL && ParentSym.HasExternalOrFriendAccess()));
        }

        //------------------------------------------------------------
        // SYM.GetParseTree
        //
        /// <summary>
        /// <para>returns parse tree for classes, and class members
        /// Given a symbol, get its parse tree</para>
        /// <para>Return ParseTreeNode except the cases below.
        /// Return DeclTreeNode for LOCVARSYM, null for AGGSYM.</para>
        /// <para>Unlike GetSomeParseTree, if no parse tree, return null</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE GetParseTree()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    return (this as METHPROPSYM).ParseTreeNode;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).ParseTreeNode;

                case SYMKIND.TYVARSYM:
                    return (this as TYVARSYM).ParseTreeNode;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).ParseTreeNode;

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).ParseTreeNode;

                case SYMKIND.NSDECLSYM:
                    return (this as NSDECLSYM).ParseTreeNode;

                case SYMKIND.ALIASSYM:
                    return (this as ALIASSYM).ParseTreeNode;

                case SYMKIND.GLOBALATTRSYM:
                    return (this as GLOBALATTRSYM).ParseTreeNode;

                case SYMKIND.LOCVARSYM:
                    return (this as LOCVARSYM).DeclTreeNode;

                case SYMKIND.ERRORSYM:
                    return null;

                case SYMKIND.NSSYM:
                case SYMKIND.NSAIDSYM:
                    return null;

                case SYMKIND.LABELSYM:
                    return null;

                case SYMKIND.PTRSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.ARRAYSYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.AGGTYPESYM:
                case SYMKIND.AGGSYM:  // an AGGSYM could have multiple parse trees, due to partial classes!
                default:
                    // should never call this with any other type
                    DebugUtil.Assert(false);
                    return null;
            }
        }

        //------------------------------------------------------------
        // SYM.GetSomeParseTree
        //
        /// <summary>
        /// <para>
        /// Given a symbol, get its parse tree
        /// </para>
        /// <para>
        /// Return ParseTreeNode except the cases below.
        /// Return DeclFirst().ParseTreeNode for AGGSYM, DeclTreeNode for LOCVARSYM.
        /// </para>
        /// <para>
        /// Unlike GetParseTree, if no parse tree, return the ParseTreeNode of ParentSym.
        /// </para>
        /// </summary>
        //------------------------------------------------------------
        internal BASENODE GetSomeParseTree()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    return (this as METHPROPSYM).ParseTreeNode;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).ParseTreeNode;

                case SYMKIND.TYVARSYM:
                    return (this as TYVARSYM).ParseTreeNode;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).ParseTreeNode;

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).ParseTreeNode;

                case SYMKIND.NSDECLSYM:
                    return (this as NSDECLSYM).ParseTreeNode;

                case SYMKIND.ALIASSYM:
                    return (this  as ALIASSYM).ParseTreeNode;

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).FirstDeclSym.ParseTreeNode;

                case SYMKIND.LOCVARSYM:
                    return (this as LOCVARSYM).DeclTreeNode;

                default:
                    if (this.ParentSym != null) return this.ParentSym.GetSomeParseTree();
                    break;
            }
            return null;
        }

        //------------------------------------------------------------
        // SYM.GetInputFile
        //
        /// <summary>
        /// <para>returns the inputfile where a symbol was declared.</para>
        /// <para>returns NULL for namespaces because they can be declaredmultiple files.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM GetInputFile()
        {
            switch (Kind)
            {
                case SYMKIND.NSSYM:
                case SYMKIND.NSAIDSYM:
                    // namespaces don't have input files
                    // call with a NSDECLSYM instead
                    //ASSERT(0);
                    return null;

                case SYMKIND.NSDECLSYM:
                    return (this as NSDECLSYM).InFileSym;

                case SYMKIND.AGGSYM:
                    {
                        AGGSYM aggsym = this as AGGSYM;
                        if (!aggsym.IsSource)
                        {
                            return aggsym.DeclOnly().GetInputFile();
                        }

                        // Because an AGGSYM that isn't metadata can be defined across multiple
                        // files, getInputFile isn't a reasonable operation.
                        //DebugUtil.Assert(false);
                        return null;
                    }

                case SYMKIND.AGGTYPESYM:
                    return ((SYM)(this as AGGTYPESYM).GetAggregate()).GetInputFile();

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).GetInputFile();

                case SYMKIND.TYVARSYM:
                    if (this.ParentSym.IsAGGSYM)
                    {
                        // Because an AGGSYM that isn't metadata can be defined across multiple
                        // files, getInputFile isn't a reasonable operation.
                        //ASSERT(0);
                        return null;
                    }
                    else if (this.ParentSym.IsMETHSYM)
                    {
                        return (this.ParentSym as METHSYM).GetInputFile();
                    }
                    //ASSERT(0); 
                    break;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.FAKEMETHSYM:
                    return (this as FAKEMETHSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.METHSYM:
                    return (this as METHSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.PROPSYM:
                    return (this as PROPSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.ALIASSYM:
                    return (this.ParentSym as NSDECLSYM).GetInputFile();

                case SYMKIND.PTRSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.ARRAYSYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                    return (ParentSym as TYPESYM).GetInputFile();

                case SYMKIND.GLOBALATTRSYM:
                    return ParentSym.GetInputFile();

                case SYMKIND.MODOPTSYM:
                    return (this as MODOPTSYM).GetModule().GetInputFile();

                case SYMKIND.OUTFILESYM:
                    return (this as OUTFILESYM).FirstInFileSym();

                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                    return null;

                case SYMKIND.INFILESYM:
                    return (this as INFILESYM);

                case SYMKIND.MODULESYM:
                    return (this as MODULESYM).GetInputFile();

                default:
                    //ASSERT(0);
                    break;
            }

            return null;
        }

        //------------------------------------------------------------
        // SYM.GetSomeInputFile
        //
        /// <summary>
        /// <para>Return some input file for the symbol. Doesn't matter which.
        /// This returns NULL for some symbol kinds, but doesn't assert for any.</para>
        /// <para>Return an INFILESYM instance where this is defined.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM GetSomeInputFile()
        {
            switch (Kind)
            {
                case SYMKIND.INFILESYM:
                    return (this as INFILESYM);

                case SYMKIND.MODULESYM:
                    return (this as MODULESYM).GetInputFile();

                case SYMKIND.OUTFILESYM:
                    return (this as OUTFILESYM).FirstInFileSym();

                case SYMKIND.NSDECLSYM:
                    return (this as NSDECLSYM).InFileSym;

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).FirstDeclSym.GetInputFile();

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).GetInputFile();

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.FAKEMETHSYM:
                    return (this as FAKEMETHSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.METHSYM:
                    return (this as METHSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.PROPSYM:
                    return (this as PROPSYM).ContainingDeclaration().GetInputFile();

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).ContainingDeclaration().GetInputFile();

                default:
                    if (this.ParentSym != null)
                    {
                        return this.ParentSym.GetSomeInputFile();
                    }
                    return null;
            }
        }

        //------------------------------------------------------------
        // SYM.GetModule
        //
        /// <summary>
        /// returns the inputfile scope where a symbol was declared.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MODULESYM GetModule()
        {
            switch (Kind)
            {
                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).GetModule();

                case SYMKIND.TYVARSYM:
                    if (this.ParentSym.IsAGGSYM)
                    {
                        return (this.ParentSym as AGGSYM).GetModule();
                    }
                    else if (this.ParentSym.IsMETHSYM)
                    {
                        return (this.ParentSym as METHSYM).GetModule();
                    }
                    DebugUtil.Assert(false, "SYM.GetModule TYVARSYM:");
                    return null;

                case SYMKIND.MEMBVARSYM:
                case SYMKIND.AGGTYPESYM:
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                case SYMKIND.EVENTSYM:
                    return (this.ParentSym as AGGSYM).GetModule();

                case SYMKIND.MODOPTSYM:
                    return (this as MODOPTSYM).GetModule();

                default:
                    DebugUtil.Assert(false, "SYM.GetModule default:");
                    return null;
            }
        }

        //------------------------------------------------------------
        // SYM.GetAssemblyID
        //
        /// <summary>
        /// returns the assembly id for the declaration of this symbol
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetAssemblyID()
        {
            switch (this.Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                case SYMKIND.MEMBVARSYM:
                case SYMKIND.EVENTSYM:
                case SYMKIND.TYVARSYM:
                case SYMKIND.AGGTYPESYM:
                    return (ParentSym as AGGSYM).GetAssemblyID();

                case SYMKIND.PTRSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.ARRAYSYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                    return ParentSym.GetAssemblyID();

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).GetAssemblyID();

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).GetAssemblyID();

                case SYMKIND.NSDECLSYM:
                    return (this as NSDECLSYM).GetAssemblyID();

                case SYMKIND.INFILESYM:
                    return (this as INFILESYM).GetAssemblyID();

                case SYMKIND.MODULESYM:
                    return (this as MODULESYM).GetInputFile().GetAssemblyID();

                case SYMKIND.EXTERNALIASSYM:
                    return (this as EXTERNALIASSYM).GetAssemblyID();

                case SYMKIND.NSSYM:
                case SYMKIND.NSAIDSYM:
                default:
                    // Should never call this with any other kind.
                    //VSFAIL("GetAssemblyID called on bad sym kind");
                    DebugUtil.Assert(false, "GetAssemblyID called on bad sym kind");
                    return Kaid.Nil;
            }
        }

        //------------------------------------------------------------
        // SYM.GetAssemblyID
        //
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="id"></param>
        //------------------------------------------------------------
        internal void SetAssemblyID(int id) { }

        //------------------------------------------------------------
        // SYM.InternalsVisibleTo
        //
        /// <summary>
        /// returns the assembly id for the declaration of this symbol
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InternalsVisibleTo(int aid)
        {
            switch (this.Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                case SYMKIND.MEMBVARSYM:
                case SYMKIND.EVENTSYM:
                case SYMKIND.TYVARSYM:
                case SYMKIND.AGGTYPESYM:
                    return (ParentSym as AGGSYM).InternalsVisibleTo(aid);

                case SYMKIND.PTRSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.ARRAYSYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                    return ParentSym.InternalsVisibleTo(aid);

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).InternalsVisibleTo(aid);

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).InternalsVisibleTo(aid);

                case SYMKIND.NSDECLSYM:
                    return (this as NSDECLSYM).InternalsVisibleTo(aid);

                case SYMKIND.INFILESYM:
                    return (this as INFILESYM).InternalsVisibleTo(aid);

                case SYMKIND.MODULESYM:
                    return (this as MODULESYM).GetInputFile().InternalsVisibleTo(aid);

                case SYMKIND.EXTERNALIASSYM:
                case SYMKIND.NSSYM:
                case SYMKIND.NSAIDSYM:
                default:
                    // Should never call this with any other kind.
                    //VSFAIL("InternalsVisibleTo called on bad sym kind");
                    DebugUtil.Assert(false, "InternalsVisibleTo called on bad sym kind");
                    return false;
            }
        }

        //------------------------------------------------------------
        // SYM.SameAssemOrFriend
        //
        /// <summary>
        /// Return true if sym is in the same assambly or visible to this.
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SameAssemOrFriend(SYM sym)
        {
            int aid = this.GetAssemblyID();
            return (aid == sym.GetAssemblyID() || sym.InternalsVisibleTo(aid));
        }

        //------------------------------------------------------------
        // SYM.GetTokenEmitPosition
        //
        /// <summary>
        /// Given a symbol, returns the address of its metadata token for emitting
        /// or NULL if this symbol is not emitted
        /// </summary>
        /// <returns></returns>
        // mdToken *SYM::getTokenEmitPosition()
        //------------------------------------------------------------
        internal object GetTokenEmitPosition()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                    return (this as METHSYM).EmittedMemberToken;

                case SYMKIND.PROPSYM:
                    return (this as PROPSYM).EmittedMemberToken;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).EmittedMemberToken;

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).EmittedTypeToken;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).EmittedEventToken;

                default:
                    break;
            }
            return null;
        }

        //------------------------------------------------------------
        // SYM.GetTokenEmit
        //
        /// <summary>
        /// Given a symbol, returns the address of its metadata token for emitting
        /// or NULL if this symbol is not emitted
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetTokenEmit()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                    return (this as METHSYM).EmittedMemberToken;

                case SYMKIND.PROPSYM:
                    return (this as PROPSYM).EmittedMemberToken;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).EmittedMemberToken;

                case SYMKIND.TYVARSYM:
                    return Cor.mdTokenNil;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).EmittedEventToken;

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).EmittedTypeToken;

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).AggSym.EmittedTypeToken;

                default:
                    //ASSERT(!"Bad Symbol type");
                    break;
            }

            return Cor.mdTokenNil;
        }

        //------------------------------------------------------------
        // SYM.GetBuilder
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal object GetBuilder()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                    return (this as METHSYM).MethodBuilder;

                case SYMKIND.PROPSYM:
                    return (this as PROPSYM).PropertyBuilder;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).FieldBuilder;

                case SYMKIND.TYVARSYM:
                    return null;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).EventBuilder;

                case SYMKIND.AGGSYM:
                    //AGGSYM aggSym = this as AGGSYM;
                    //if (aggSym.TypeBuilder!=null)
                    //{
                    //    return aggSym.TypeBuilder;
                    //}
                    //else if (aggSym.EnumBuilder != null)
                    //{
                    //    return aggSym.EnumBuilder;
                    //}
                    //return null;
                    return (this as AGGSYM).TypeBuilder;

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).AggSym.TypeBuilder;

                default:
                    //ASSERT(!"Bad Symbol type");
                    break;
            }

            return null;
        }

        //------------------------------------------------------------
        // SYM.GetTokenImport
        //
        /// <summary>
        /// This one asserts if there is no decl.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetTokenImport()
        {
            switch (Kind)
            {
                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).ImportedClassToken;

                case SYMKIND.METHSYM:
                    return (this as METHSYM).ImportedMethPropToken;

                case SYMKIND.PROPSYM:
                    return (this as PROPSYM).ImportedMethPropToken;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).ImportedFieldToken;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).ImportedEventToken;

                case SYMKIND.TYVARSYM:
                    return Cor.mdTokenNil;

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).AggSym.ImportedClassToken;

                default:
                    return Cor.mdTokenNil;
            }
        }

        //------------------------------------------------------------
        // SYM.IsUserCallable
        //
        /// <summary>
        /// returns true if this symbol is a normal symbol visible to the user
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsUserCallable()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                    return (this as METHSYM).IsUserCallable();

                default:
                    break;
            }
            return true;
        }

        //------------------------------------------------------------
        // SYM.GetElementKind
        //
        /// <summary>
        /// <para>Given a symbol, returns its element kind for attribute purpose</para>
        /// <para>Return AttributeTargets value corresponding to SYM.Kind.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AttributeTargets GetElementKind()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                    return ((this as METHSYM).IsCtor ?
                        AttributeTargets.Constructor :
                        AttributeTargets.Method);

                case SYMKIND.PROPSYM:
                    return AttributeTargets.Property;

                case SYMKIND.MEMBVARSYM:
                    return AttributeTargets.Field;

                case SYMKIND.TYVARSYM:
                    return AttributeTargets.GenericParameter;

                case SYMKIND.AGGTYPESYM:
                    //ASSERT(!"Bad Symbol type: SYMKIND.AGGTYPESYM"); // GENERICS - fix me
                    return AttributeTargets.Field;

                case SYMKIND.EVENTSYM:
                    return AttributeTargets.Event;

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).GetElementKind();

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).AggSym.GetElementKind();

                case SYMKIND.GLOBALATTRSYM:
                    return (this as GLOBALATTRSYM).ElementKind;

                default:
                    DebugUtil.Assert(false, "Bad Symbol type");
                    break;
            }
            return (AttributeTargets)0;
        }

        //------------------------------------------------------------
        // SYM.ContainingDeclaration
        //
        /// <summary>
        /// This one asserts if there is no decl.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal DECLSYM ContainingDeclaration()
        {
            DECLSYM decl = GetDecl();
#if DEBUG
            if (!(decl != null || IsNSDECLSYM))
            {
                ;
            }
#endif
            DebugUtil.Assert(decl != null || IsNSDECLSYM);
            return decl;
        }

        //------------------------------------------------------------
        // SYM.GetDecl
        //
        /// <summary>
        /// <para>This one returns null if there is no decl.</para>
        /// <para>returns the containing declaration of a symbol or NULL if there isn't one.
        /// will be an AGGDECLSYM for members of a type, or a NSDECLSYM for members of a namespace.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal DECLSYM GetDecl()
        {
            switch (Kind)
            {
                case SYMKIND.NSSYM:
                case SYMKIND.NSAIDSYM:
                    return null;

                case SYMKIND.AGGTYPESYM:
                case SYMKIND.AGGSYM:
                    // an AGGSYM doesn't have a containing declaration, due to partial classes.
                    return null;

                case SYMKIND.NSDECLSYM:
                case SYMKIND.AGGDECLSYM:
                    return (this as DECLSYM).ParentDeclSym;

                case SYMKIND.TYVARSYM:
                    if (this.ParentSym.IsMETHSYM)
                    {
                        return (this.ParentSym as METHSYM).ContainingDeclaration();
                    }
                    // an AGGSYM doesn't have a containing declaration, due to partial classes.
                    return null;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).ContainingDeclaration();

                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    return (this as METHPROPSYM).ContainingDeclaration();

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).ContainingDeclaration();

                case SYMKIND.GLOBALATTRSYM:
                    return ParentSym as NSDECLSYM;

                default:
                    return null;
            }
        }

        //------------------------------------------------------------
        // SYM.GetAttributesNode
        //
        /// <summary>
        /// returns the parse node for the type's attributes
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE GetAttributesNode()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                    return (this as METHSYM).GetAttributesNode();

                case SYMKIND.AGGDECLSYM:
                    return (this as AGGDECLSYM).GetAttributesNode();

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).GetAttributesNode();

                case SYMKIND.TYVARSYM:
                    return (this as TYVARSYM).GetAttributesNode();

                case SYMKIND.PROPSYM:
                    return (this as PROPSYM).GetAttributesNode();

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).GetAttributesNode();

                default:
                    DebugUtil.Assert(false, "Bad Symbol Type");
                    return null;
            }
        }

        //------------------------------------------------------------
        // SYM.IsContainedInDeprecated
        //
        /// <summary> </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsContainedInDeprecated()
        {
            if (this == null)
            {
                return false;
            }
            if (this.isDeprecated)
            {
                return true;
            }
            if (ParentSym != null && (ParentSym.IsAGGSYM))
            {
                return ParentSym.IsContainedInDeprecated();
            }
            return false;
        }

        //------------------------------------------------------------
        // SYM.IsVirtual
        //
        /// <summary>
        /// Returns if the symbol is virtual.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsVirtual()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                    return (this as METHSYM).IsVirtual;

                case SYMKIND.EVENTSYM:
                    return ((this as EVENTSYM).AddMethodSym != null &&
                        (this as EVENTSYM).AddMethodSym.IsVirtual);

                case SYMKIND.PROPSYM:
                    PROPSYM propSym = this as PROPSYM;
                    return (propSym.GetMethodSym != null && propSym.GetMethodSym.IsVirtual) ||
                           (propSym.SetMethodSym != null &&
                           propSym.SetMethodSym.IsVirtual);

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // SYM.IsOverride
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsOverride()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    return (this as METHPROPSYM).IsOverride;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).IsOverride;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // SYM.IsHideByName
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsHideByName()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    return (this as METHPROPSYM).HideByName;

                case SYMKIND.EVENTSYM:
                    return (
                        (this as EVENTSYM).AddMethodSym != null &&
                        (this as EVENTSYM).AddMethodSym.HideByName);

                default:
                    return true;
            }
        }

        //------------------------------------------------------------
        // SYM.SymBaseVirtual
        //
        /// <summary>
        /// Returns the virtual that this sym overrides (if IsOverride() is true), NULL otherwise.
        /// (if its an override or explicit interface member impl), null otherwise.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM SymBaseVirtual()
        {
            switch (Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    return (this as METHPROPSYM).SlotSymWithType.Sym;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).SlotEventWithType.EventSym;

                default:
                    return null;
            }
        }

        //------------------------------------------------------------
        // SYM.CheckBogus
        //
        /// <summary>
        /// <para>if this ASSERT fires then call COMPILER::CheckBogus() instead</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckBogus()
        {
#if false
            DebugUtil.Assert(this.checkedBogus);
#else
            DebugUtil.Assert(this.HasBogus);
#endif
            return (this.IsBogus);
        }

        //------------------------------------------------------------
        // SYM.getBogus
        // SYM.hasBogus
        // SYM.setBogus
        //------------------------------------------------------------
        //internal bool getBogus() { return (this.isBogus); }
        //internal bool hasBogus() { return (this.checkedBogus); }
        //internal void setBogus(bool isBogus)
        //{
        //    this.isBogus = (isBogus ? true : false);
        //    this.checkedBogus = true;
        //}

        //------------------------------------------------------------
        // SYM.IsUnsafe
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsUnsafe()
        {
            if (this.IsMEMBVARSYM)
            {
                return (this as MEMBVARSYM).IsUnsafe;
            }
            else if (this.IsMETHPROPSYM)
            {
                return (this as METHPROPSYM).IsUnsafe;
            }
            else if (this.IsAGGDECLSYM)
            {
                return (this as AGGDECLSYM).IsUnsafe;
            }
            else if (this.IsEVENTSYM)
            {
                return (this as EVENTSYM).IsUnsafe;
            }
            else
            {
                DebugUtil.Assert(false, "Undefined unsafe check");
                return false;
            }
        }

        //------------------------------------------------------------
        // SYM.IsDeprecated
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsDeprecated()
        {
            switch (this.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    return (this as AGGTYPESYM).GetAggregate().IsDeprecated();

                case SYMKIND.ARRAYSYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTSYM:
                case SYMKIND.MODOPTTYPESYM:
                    if (this.ParentSym != null)
                    {
                        return this.ParentSym.IsDeprecated();
                    }
                    break;

                default:
                    return this.isDeprecated;
            }
            return false;
        }

        //------------------------------------------------------------
        // SYM.IsDeprecatedError
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsDeprecatedError()
        {
            switch (this.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    return (this as AGGTYPESYM).GetAggregate().IsDeprecatedError();

                case SYMKIND.ARRAYSYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTSYM:
                case SYMKIND.MODOPTTYPESYM:
                    if (this.ParentSym != null)
                    {
                        return this.ParentSym.IsDeprecatedError();
                    }
                    break;

                default:
                    return this.isDeprecatedError;
            }
            return false;
        }

        //------------------------------------------------------------
        // SYM.DeprecatedMessage()
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string DeprecatedMessage()
        {
            switch (this.Kind)
            {
                case SYMKIND.METHSYM:
                    return (this as METHPROPSYM).deprecatedMessage;

                case SYMKIND.PROPSYM:
                    return (this as METHPROPSYM).deprecatedMessage;

                case SYMKIND.EVENTSYM:
                    return (this as EVENTSYM).deprecatedMessage;

                case SYMKIND.MEMBVARSYM:
                    return (this as MEMBVARSYM).deprecatedMessage;

                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).deprecatedMessage;

                case SYMKIND.AGGTYPESYM:
                    return (this as AGGTYPESYM).GetAggregate().deprecatedMessage;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.PINNEDSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTSYM:
                case SYMKIND.MODOPTTYPESYM:
                    if (this.ParentSym != null)
                    {
                        return this.ParentSym.DeprecatedMessage();
                    }
                    break;

                default:
                    return null;
            }
            return null;
        }

        //------------------------------------------------------------
        // SYM.SetDeprecated
        //
        /// <summary></summary>
        /// <param name="isDep"></param>
        /// <param name="isError"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void SetDeprecated(bool isDep, bool isError, string message)
        {
            switch (this.Kind)
            {
                default:
                    return;

                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    (this as METHPROPSYM).deprecatedMessage = message;
                    break;

                case SYMKIND.EVENTSYM:
                    (this as EVENTSYM).deprecatedMessage = message;
                    break;

                case SYMKIND.MEMBVARSYM:
                    (this as MEMBVARSYM).deprecatedMessage = message;
                    break;

                case SYMKIND.AGGSYM:
                    (this as AGGSYM).deprecatedMessage = message;
                    break;
            }
            this.isDeprecated = isDep;
            this.isDeprecatedError = isError;
        }

        //------------------------------------------------------------
        // SYM.CopyDeprecatedFrom
        //
        /// <summary></summary>
        /// <param name="symSrc"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void CopyDeprecatedFrom(SYM symSrc)
        {
            SetDeprecated(
                symSrc.IsDeprecated(),
                symSrc.IsDeprecatedError(),
                symSrc.DeprecatedMessage());
        }

        //------------------------------------------------------------
        // SYM.GetSID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetSID()
        {
            //ASSERT(hasSid);
            throw new System.NotImplementedException("GetSid");
            //return hasSid ? (int)(INT_PTR)((void **)this)[-1] : 0;
        }

        //------------------------------------------------------------
        // SYM.CopyFrom
        //
        /// <summary></summary>
        /// <param name="other"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal void CopyFrom(SYM other)
        {
            if (other == null)
            {
                return;
            }

            this.Kind = other.Kind;
#if false
            this.isBogus = other.IsBogus;
            this.checkedBogus = other.checkedBogus;
#else
            this.IsBogus = other.IsBogus;
            this.HasBogus = other.HasBogus;
#endif
            this.isDeprecated = other.isDeprecated;
            this.isDeprecatedError = other.isDeprecatedError;
            this.Access = other.Access;
            this.IsLocal = other.IsLocal;
            this.HasSID = other.HasSID;
            this.IsDumped = other.IsDumped;
            this.HasCLSAttribute = other.HasCLSAttribute;
            this.IsCLS = other.IsCLS;
            this.IsDead = other.IsDead;
            this.IsFabricated = other.IsFabricated;
            this.HadAttributeError = other.HadAttributeError;
            this.symbolName = other.symbolName;

            //this.ParentSym = other.ParentSym;
            //this.NextSym = other.NextSym;
            //this.NextSameNameSym = other.NextSameNameSym;
        }

#if DEBUG
        //------------------------------------------------------------
        // SYM Debug
        //------------------------------------------------------------

        virtual internal string DebugName
        {
            get { return this.Name; }
        }

        protected string debugComment = null;
        virtual internal string DebugComment
        {
            get { return this.debugComment; }
            set { this.debugComment = value; }
        }

        virtual internal void zDummy() { }

        virtual internal string _DebugInfoString
        {
            get
            {
                if (String.IsNullOrEmpty(this.DebugName))
                {
                    return String.Format(
                        "No.{0}: {1}",
                        this.SymID, this.DebugComment);
                }
                else
                {
                    return String.Format(
                        "No.{0}: {1} {2}",
                        this.SymID, this.DebugName, this.DebugComment);
                }
            }
        }

        //------------------------------------------------------------
        // SYM.DebugPrefix
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugPrefix(StringBuilder sb)
        {
            string temp;
            StringBuilder sbTemp = new StringBuilder();
            if (Name != null)
            {
                temp = CharUtil.ReplaceControlCharacters(Name, null);
            }
            else
            {
                temp = "(no name)";
            }
            sb.AppendFormat("No.{0,-5}: {1} ({2})\n", SymID, temp, Kind);
            if (ParentSym != null) sb.AppendFormat("Parent : No.{0} ({1}) {2}\n",
                ParentSym.SymID,
                ParentSym.Kind,
                ParentSym.Name);
            else sb.Append("Parent  : null\n");

            sbTemp.Length = 0;
            if (!String.IsNullOrEmpty(this.DebugComment))
            {
                if (sbTemp.Length > 0)
                    sbTemp.AppendFormat(", {0}", this.DebugComment);
                else
                    sbTemp.Append(this.DebugComment);
            }
            if (sbTemp.Length > 0)
            {
                sb.AppendFormat("Debug Info       : {0}\n", sbTemp.ToString());
            }

            sb.AppendFormat("Mask             : {0}\n", Mask);
            //sb.AppendFormat("IsBougs          : {0}\n", isBogus);
            //sb.AppendFormat("HasBougs         : {0}\n", HasBogus);
            //sb.AppendFormat("isDeprecate      : {0}\n", isDeprecated);
            sb.AppendFormat("Access           : {0}\n", Access);
            //sb.AppendFormat("IsLocal          : {0}\n", IsLocal);
            //sb.AppendFormat("HasSID           : {0}\n", HasSID);
            //sb.AppendFormat("IsDumped         : {0}\n", IsDumped);
            //sb.AppendFormat("HasCLSaAttribute : {0}\n", HasCLSAttribute);
            //sb.AppendFormat("IsCLS            : {0}\n", IsCLS);
            //sb.AppendFormat("IsDead           : {0}\n", IsDead);
            //sb.AppendFormat("IsFabricated     : {0}\n", IsFabricated);
            //sb.AppendFormat("HadAttributeError: {0}\n", HadAttributeError);
            sb.Append("\n");
        }

        //------------------------------------------------------------
        // SYM.DebugPostfix
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugPostfix(StringBuilder sb)
        {
            if (NextSym != null)
            {
                sb.AppendFormat("NextSym          : No.{0}\n", NextSym.SymID);
            }
            else
            {
                sb.Append("NextSym          : ---\n");
            }
            if (NextSameNameSym != null)
            {
                sb.AppendFormat("NextSameNameSym  : No.{0}\n", NextSameNameSym.SymID);
            }
            else
            {
                sb.Append("NextSameNameSym  : ---\n");
            }
            sb.Append("\n");
        }

        //------------------------------------------------------------
        // SYM.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        virtual internal void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
        }
#endif
    }

    //======================================================================
    // class TypeArray
    //
    /// <summary>
    /// <para>Encapsulates a type list, including its size and metadata token.
    /// This class has List&lt;TYPESYM&gt; and the auxiliary members. (CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal class TypeArray
    {
        //------------------------------------------------------------
        // TypeArray Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// (R) Index starting from 1 assigned when an object is created.
        /// </summary>
        internal readonly int TypeArrayID = CObjectID.GenerateID();

        /// <summary>
        /// List of TYPESYM instances.
        /// </summary>
        private List<TYPESYM> items = null;

        /// <summary>
        /// <para>If argument is invalid, return null.</para>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal TYPESYM this[int index]
        {
            get
            {
                if (this.items != null &&
                    index >= 0 &&
                    index < this.items.Count)
                {
                    return items[index];
                }
                return null;
            }
            set
            {
                if (this.items != null &&
                    index >= 0 &&
                    index < this.items.Count)
                {
                    this.items[index] = value;
                }
            }
        }

        internal List<TYPESYM> List
        {
            get { return items; }
        }

        internal int Count
        {
            get { return items.Count; }
        }

        /// <summary>
        /// This is for an optimization that avoids calling EnsureState on each item.
        /// </summary>
        internal AggStateEnum AggState;

        /// <summary>
        /// Whether any constituents have errors. This is immutable.
        /// </summary>
        internal bool HasErrors = false;    // uint fHasErrors: 1;

        /// <summary>
        /// (R) Whether any constituents are unresolved. This is immutable.
        /// </summary>
        internal bool Unresolved = false;   // uint fUnres: 1;

        //internal uint tok = 0;

        private int hashCode = -1;

#if DEBUG
        internal string debugComment = null;
        internal int HashValue
        {
            get { return this.GetHashCode(); }
        }
        internal bool Registerd = false;
#endif

        //------------------------------------------------------------
        // TypeArray.Size (static)
        //
        /// <summary></summary>
        /// <param name="ta"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal int Size(TypeArray ta)
        {
            return (ta != null ? ta.Count : 0);
        }

        //------------------------------------------------------------
        // TypeArray.Equals (override)
        //
        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            TypeArray other = obj as TypeArray;
            if (other == null)
            {
                return false;
            }
            if (this.items == null || other.items == null)
            {
                return false;
            }
            if (this.TypeArrayID == other.TypeArrayID)
            {
                return true;
            }

            int count = this.items.Count;
            if (count != other.items.Count)
            {
                return false;
            }

            for (int i = 0; i < count; ++i)
            {
                if (this.items[i] != other.items[i])
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // TypeArray.GetHashCode (override)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override int GetHashCode()
        {
            if (this.hashCode >= 0)
            {
                return this.hashCode;
            }
            return CalcHashCode();
        }

        //------------------------------------------------------------
        // TypeArray.CalcHashCode
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int CalcHashCode()
        {
            int hash = 0;
            if (this.items != null)
            {
                foreach (TYPESYM sym in this.items)
                {
                    if (sym != null)
                    {
                        hash ^= sym.GetHashCode();
                    }
                }
                hash &= 0x7FFFFFFF;
                this.hashCode = hash;
            }
            return this.hashCode;
        }

#if DEBUG
        internal const int DebugID = 5229;
#endif

        //------------------------------------------------------------
        // TypeArray Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal TypeArray()
        {
            items = new List<TYPESYM>();
#if DEBUG
            if (this.TypeArrayID == DebugID)
            {
                ;
            }
#endif
        }

        //------------------------------------------------------------
        // TypeArray Constructor (2)
        //
        /// <summary>
        /// <para>Add nulls of given number</para>
        /// </summary>
        /// <param name="capacity"></param>
        //------------------------------------------------------------
        internal TypeArray(int capacity)
        {
            items = new List<TYPESYM>(capacity);
            while (--capacity >= 0)
            {
                items.Add(null);
            }
#if DEBUG
            if (this.TypeArrayID == DebugID)
            {
                ;
            }
#endif
        }

        //------------------------------------------------------------
        // TypeArray Constructor (3)
        //
        /// <summary></summary>
        /// <param name="syms"></param>
        //------------------------------------------------------------
        internal TypeArray(List<TYPESYM> syms)
        {
            this.items = syms;
#if DEBUG
            if (this.TypeArrayID == DebugID)
            {
                ;
            }
#endif
        }

        //------------------------------------------------------------
        // TypeArray Constructor (4)
        //
        /// <summary></summary>
        /// <param name="syms"></param>
        //------------------------------------------------------------
        internal TypeArray(TYPESYM[] syms)
        {
            items = new List<TYPESYM>();
            if (syms != null)
            {
                items.AddRange(syms);
            }
#if DEBUG
            if (this.TypeArrayID == DebugID)
            {
                ;
            }
#endif
        }

        //------------------------------------------------------------
        // TypeArray.Init
        //
        /// <summary>Call Clear method.</summary>
        //------------------------------------------------------------
        internal void Init()
        {
            DebugUtil.Assert(this.items != null);

            items.Clear();
        }

        //------------------------------------------------------------
        // TypeArray.Clear
        //
        /// <summary>Call Clear method.</summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            items.Clear();
        }

        //------------------------------------------------------------
        // TypeArray.SetAggState
        //
        /// <summary></summary>
        /// <param name="state"></param>
        //------------------------------------------------------------
        //internal void SetAggState(AggStateEnum state)
        //{
        //    this.aggState = state;
        //}

        //------------------------------------------------------------
        // TypeArray.Add (1)
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal void Add(TYPESYM sym)
        {
            DebugUtil.Assert(this.items != null);

            if (sym != null)
            {
                items.Add(sym);
            }
        }

        //------------------------------------------------------------
        // TypeArray.Add (2)
        //
        /// <summary></summary>
        /// <param name="typeArray"></param>
        //------------------------------------------------------------
        internal void Add(TypeArray typeArray)
        {
            DebugUtil.Assert(this.items != null);

            if (typeArray != null &&
                typeArray.items != null)
            {
                items.AddRange(typeArray.items);
            }
        }

        //------------------------------------------------------------
        // TypeArray.Add (3)
        //
        /// <summary></summary>
        /// <param name="typeSymList"></param>
        //------------------------------------------------------------
        internal void Add(IEnumerable<TYPESYM> syms)
        {
            DebugUtil.Assert(this.items != null);

            if (syms != null)
            {
                items.AddRange(syms);
            }
        }

        //------------------------------------------------------------
        // TypeArray.Add (4)
        //
        /// <summary></summary>
        /// <param name="typeSymArray"></param>
        //------------------------------------------------------------
        internal void Add(TYPESYM[] syms)
        {
            DebugUtil.Assert(this.items != null);

            if (syms != null)
            {
                items.AddRange(syms);
            }
        }

        //------------------------------------------------------------
        // TypeArray.Add (5)
        //------------------------------------------------------------
        //internal void Add(Type type)
        //{
        //    TYPESYM typeSym = new TYPESYM();
        //    //typeSym.SetType(type);
        //    items.Add(typeSym);
        //}

        //------------------------------------------------------------
        // TypeArray.Add (6)
        //------------------------------------------------------------
        //internal void Add(Type[] types, int start, int end)
        //{
        //    if (types == null || start < 0 || start >= end) return;
        //    if (end > types.Length) end = types.Length;
        //    for (int i = 0; i < end; ++i)
        //    {
        //        this.Add(types[i]);
        //    }
        //}

        //------------------------------------------------------------
        // TypeArray.Add (7)
        //------------------------------------------------------------
        //internal void Add(Type[] types, int start)
        //{
        //    if (types == null) return;
        //    this.Add(types, start, start + types.Length);
        //}

        //------------------------------------------------------------
        // TypeArray.Add (8)
        //------------------------------------------------------------
        //internal void Add(Type[] types)
        //{
        //    if (types == null) return;
        //    this.Add(types, 0, types.Length);
        //}

#if false
        //------------------------------------------------------------
        // TypeArray.Item
        //------------------------------------------------------------
        internal TYPESYM Item(int i)
        {
            try
            {
                return items[i];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }
#endif

        //------------------------------------------------------------
        // TypeArray.ItemAsTYVARSYM
        //
        /// <summary></summary>
        /// <param name="i"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYVARSYM ItemAsTYVARSYM(int i)
        {
            if (this.items != null &&
                i >= 0 &&
                i < this.items.Count)
            {
                return this.items[i] as TYVARSYM;
            }
            return null;
        }

        //------------------------------------------------------------
        // TypeArray.CopyItems (1)
        //
        /// <summary></summary>
        /// <param name="i"></param>
        /// <param name="c"></param>
        /// <param name="dest"></param>
        //------------------------------------------------------------
        internal void CopyItems(int i, int c, List<TYPESYM> dest)
        {
            DebugUtil.Assert(this.items != null);

            try
            {
                dest.AddRange(new List<TYPESYM>(this.items.GetRange(i, c)));
            }
            catch (ArgumentException)
            {
            }
        }

        //------------------------------------------------------------
        // TypeArray.CopyItems (2)
        //
        /// <summary></summary>
        /// <param name="i"></param>
        /// <param name="c"></param>
        /// <param name="dest"></param>
        //------------------------------------------------------------
        internal void CopyItems(int i, int c, TYPESYM[] dest)
        {
            DebugUtil.Assert(this.items != null);

            if (i < 0 ||
                i >= this.items.Count ||
                dest.Length < c)
            {
                return;
            }

            if (i + c > this.items.Count)
            {
                c = this.items.Count - i;
            }

            try
            {
                for (int j = 0; j <  c; ++j)
                {
                    dest[j] = this.items[i + j];
                }
            }
            catch (ArgumentException)
            {
            }
            catch (IndexOutOfRangeException)
            {
            }
        }

        //------------------------------------------------------------
        // TypeArray.ItemPtr
        //
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <remarks>
        /// (sscli) TYPESYM ** ItemPtr(int i)
        /// </remarks>
        //------------------------------------------------------------
        //internal TYPESYM ItemPtr(int i)
        //{
        //    throw new NotImplementedException("TypeArray.ItemPtr");
        //}

        //------------------------------------------------------------
        // TypeArray.AssertValid
        //
        /// <summary>
        /// [Conditional("DEBUG")]
        /// </summary>
        //------------------------------------------------------------
        [Conditional("DEBUG")]
        internal void AssertValid()
        {
            //ASSERT(size >= 0);
            //for (int i = 0; i < size; i++) {
            //    ASSERT(items[i]);
            //}
        }

        //------------------------------------------------------------
        // TypeArray.EqualRange
        //
        /// <summary>
        /// Compare elements.
        /// </summary>
        /// <param name="typeArray1"></param>
        /// <param name="i1"></param>
        /// <param name="typeArray2"></param>
        /// <param name="i2"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool EqualRange(TypeArray typeArray1, int i1, TypeArray typeArray2, int i2, int c)
        {
            //ASSERT(SizeAdd(i1, c) <= (size_t)typeArray1->size);
            //ASSERT(SizeAdd(i2, c) <= (size_t)typeArray2->size);
            //return !memcmp(typeArray1->items + i1, typeArray2->items + i2, c * sizeof(typeArray1->items[0]));
            int ie1 = i1 + c;
            if (ie1 > typeArray1.items.Count || i2 + c > typeArray2.items.Count) return false;
            for (; i1 < ie1; ++i1, ++i2)
            {
                if (typeArray2.items[i2] != typeArray1.items[i1]) return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // TypeArray.Find
        //------------------------------------------------------------
        internal int Find(TYPESYM type)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (type == items[i]) return i;
            }
            return -1;
        }

        //------------------------------------------------------------
        // TypeArray.Contains
        //------------------------------------------------------------
        internal bool Contains(TYPESYM type)
        {
            return (this.Find(type) >= 0);
        }

        //------------------------------------------------------------
        // TypeArray.GetSubArray (1)
        //
        /// <summary>
        /// <para>Get specifiled elements and return them as an array of type TYPESYM.</para>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="symArray">(out)</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetSubArray(int start, int count, out TypeArray symArray, COMPILER comp)
        {
            try
            {
                symArray = new TypeArray(this.items.GetRange(start, count));
                if (comp != null)
                {
                    symArray = comp.MainSymbolManager.AllocParams(symArray);
                }
                return true;
            }
            catch (ArgumentException)
            {
            }
            symArray = null;
            return false;
        }

        //------------------------------------------------------------
        // TypeArray.GetSubArray (2)
        //
        /// <summary>
        /// <para>Get specifiled elements and return them as an array of type TYPESYM.</para>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="symArray">(out)</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetSubArray(int start, int count, out TYPESYM[] symArray)
        {
            symArray = null;
            if (start < 0 || count < 0 || start + count > items.Count)
            {
                return false;
            }

            symArray = new TYPESYM[count];
            if (count > 0)
            {
                for (int i = 0, j = start; i < count; ++i, ++j)
                {
                    symArray[i] = items[j];
                }
            }
            return true;
        }

#if DEBUG
        //------------------------------------------------------------
        // TypeArray.GetDebugString
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal string GetDebugString()
        {
            StringBuilder sbt = new StringBuilder();
            for (int i = 0; i < items.Count; ++i)
            {
                if (sbt.Length > 0)
                {
                    sbt.Append(", ");
                }
                TYPESYM sym = items[i];
                if (sym != null)
                {
                    sbt.AppendFormat("({0} \"{1}\")",
                        sym.SymID,
                        sym.Name != null ? sym.Name : "");
                }
            }
            return sbt.ToString();
        }

        //------------------------------------------------------------
        // TypeArray.DebugSymID
        //
        /// <summary>
        /// Append sb a string containing SymIDs delimited by comma.
        /// </summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugSymID(StringBuilder sb)
        {
            StringBuilder sbt = new StringBuilder();
            for (int i = 0; i < items.Count; ++i)
            {
                if (sbt.Length > 0)
                {
                    sbt.Append(", ");
                }
                if (items[i] != null)
                {
                    sbt.AppendFormat("{0}", items[i].SymID);
                }
            }
            sb.Append(sbt.ToString());
        }

        //------------------------------------------------------------
        // TypeArray.DebugGeneric
        //
        /// <summary>
        /// <para>Append sb a string formatted as "&lt; , ... &gt;".</para>
        /// <para>If no types, appends nothing.</para>
        /// </summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugGeneric(StringBuilder sb)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            sb.Append("<");
            for (int i = 0; i < items.Count; ++i)
            {
                if (i > 0) sb.Append(", ");
                items[i].DebugGeneric(sb);
            }
            sb.Append(">");
        }
#endif
    }

    //======================================================================
    // enum NameCacheFlagsEnum
    //
    /// <summary>
    /// (CSharp\SCComp\Symbol.cs)
    /// </summary>
    //======================================================================
    [Flags]
    internal enum NameCacheFlagsEnum
    {
        None = 0x00,
        TypeSameName = 0x01,
        NoTypeSameName = 0x02,
    }

    //======================================================================
    // class CACHESYM
    //
    /// <summary>
    /// <para>CACHESYM - a symbol which wraps other symbols
    /// so that they can be cached in the local scope by name
    /// LOCVARSYMs are never cached in the introducing scope</para>
    /// </summary>
    //======================================================================
    internal class CACHESYM : SYM
    {
        /// <summary>
        /// The symbol this cache entry points to
        /// </summary>
        internal SYM EntrySym = null;   // PSYM sym;

        /// <summary>
        /// The types containing the symbol(s)
        /// </summary>
        internal TypeArray TypeArray = null;    // * types

        internal NameCacheFlagsEnum Flags;  // flags

        /// <summary>
        /// The scope in which this name is bound to that symbol
        /// </summary>
        internal SCOPESYM ScopeSym = null;  // PSCOPESYM scope;

        /// <summary>
        /// (CS3) If EntrySym has already bound, set the EXPR instance.
        /// </summary>
        internal EXPR ObjectExpr = null;

#if DEBUG
        //------------------------------------------------------------
        // CACHESYM._DebugInfoString
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class LABELSYM
    //
    /// <summary>
    /// <para>LABELSYM - a symbol representing a label. </para>
    /// </summary>
    //======================================================================
    internal class LABELSYM : SYM
    {
        /// <summary>
        /// The corresponding label statement
        /// </summary>
        internal EXPRLABEL LabelExpr;   // * labelExpr

#if DEBUG
        //------------------------------------------------------------
        // LABELSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class PARENTSYM
    //
    /// <summary>
    /// <para>PARENTSYM - a symbol that can contain other symbols as children.</para>
    /// </summary>
    //======================================================================
    internal class PARENTSYM : SYM
    {
        //------------------------------------------------------------
        // PARENTSYM    Fields and Properties
        //------------------------------------------------------------
        internal SYM FirstChildSym = null;   // public SYM * firstChild;
        internal SYM LastChildSym = null;    // public SYM ** psymAttachChild;

        //------------------------------------------------------------
        // PARENTSYM.AddToChildList
        //
        /// <summary>
        /// This adds the sym to the child list but doesn't associate it
        /// in the symbol table.
        /// </summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal void AddToChildList(SYM sym)
        {
            DebugUtil.Assert(sym != null);
            // If parent is set it should be set to us!
            DebugUtil.Assert(sym.ParentSym == null || sym.ParentSym == this);

            // There shouldn't be a nextChild.
            DebugUtil.Assert(sym.NextSym == null);

            if (LastChildSym == null)
            {
                FirstChildSym = LastChildSym = sym;
            }
            else
            {
                LastChildSym.NextSym = sym;
                LastChildSym = sym;
            }
            sym.ParentSym = this;
        }

        //------------------------------------------------------------
        // PARENTSYM.RemoveFromChildList
        //
        /// <summary>
        /// <para>Remove the specifiled child SYM instance from the child list.</para>
        /// <para>If not found, return false.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool RemoveFromChildList(SYM sym)
        {
            DebugUtil.Assert(sym != null && sym.ParentSym == this);
            DebugUtil.Assert(FirstChildSym != null);

            SYM rsym = null;
            if (FirstChildSym == sym)
            {
                rsym = FirstChildSym;
                FirstChildSym = rsym.NextSym;

                rsym.NextSym = null;
                rsym.ParentSym = null;
                return true;
            }
            else
            {
                SYM psym = FirstChildSym;
                for (SYM csym = FirstChildSym.NextSym;
                    csym != null;
                    psym = csym, csym = csym.NextSym)
                {
                    if (csym == sym)
                    {
                        psym.NextSym = csym.NextSym;
                        csym.NextSym = null;
                        csym.ParentSym = null;
                        return true;
                    }
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // PARENTSYM.ClearChildList
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearChildList()
        {
            this.FirstChildSym = null;
            this.LastChildSym = null;
        }

        //------------------------------------------------------------
        // PARENTSYM.CopyForm
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal override void CopyFrom(SYM sym)
        {
            PARENTSYM other = sym as PARENTSYM;
            if (other == null)
            {
                return;
            }
            base.CopyFrom(other);

            this.FirstChildSym = other.FirstChildSym;
            this.LastChildSym = other.LastChildSym;
        }

#if DEBUG
        //------------------------------------------------------------
        // PARENTSYM Debug
        //------------------------------------------------------------
        internal void DebugParent(StringBuilder sb)
        {
            StringBuilder sbp = new StringBuilder();
            SYM sym = this.FirstChildSym;
            while (sym != null)
            {
                if (sbp.Length > 0) sbp.Append(", ");
                sbp.AppendFormat("{0}", sym.SymID);
                sym = sym.NextSym;
            }
            if (sbp.Length > 0)
            {
                sb.AppendFormat("Child SYM : No.{0}\n", sbp.ToString());
            }
            else
            {
                sb.Append("Child SYM :\n");
            }
            sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // PREDEFATTR
    /// <summary>
    /// <para>enum of predefined attributes</para>
    /// <para>(PA_) CSharp\SCComp\Symbol.cs</para>
    /// </summary>
    //======================================================================
    internal enum PREDEFATTR
    {
        //#define PREDEFATTRDEF(id,iPredef) id,
        //#include "predefattr.h"
        //#undef PREDEFATTRDEF

        ATTRIBUTEUSAGE,
        OBSOLETE,
        CLSCOMPLIANT,
        CONDITIONAL,
        REQUIRED,
        FIXED,
        DEBUGGABLE,

        NAME,

        DLLIMPORT,
        COMIMPORT,
        GUID,
        IN,
        OUT,
        STRUCTOFFSET,
        STRUCTLAYOUT,
        PARAMARRAY,
        COCLASS,
        DEFAULTCHARSET,
        DEFAULTVALUE,
        UNMANAGEDFUNCTIONPOINTER,

        COMPILATIONRELAXATIONS,
        RUNTIMECOMPATIBILITY,
        FRIENDASSEMBLY,
        KEYFILE,
        KEYNAME,
        DELAYSIGN,

        DEFAULTMEMBER,

        TYPEFORWARDER,

        COUNT
    }

    //======================================================================
    // class TYPESYM
    //
    /// <summary>
    /// <para>TYPESYM - a symbol that can be a type. Our handling of derived types
    /// (like arrays and pointers) requires that all types extend PARENTSYM.</para>
    /// <para>Derives from PARENTSYM.</para>
    /// </summary>
    //======================================================================
    internal class TYPESYM : PARENTSYM
    {
        //------------------------------------------------------------
        // TYPESYM  System.Type and System.Reflection.Emit.TypeBuilder
        //------------------------------------------------------------
        protected Type type = null;

        virtual internal Type Type
        {
            get { return this.type; }
        }

        internal string AssemblyQualifiedName = null;   // PWSTR pszAssemblyQualifiedName;

        //------------------------------------------------------------
        // TYPESYM  Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// The minimum of the states of the constituent AGGSYMs.
        /// </summary>
        /// <remarks>
        /// AggStateEnum AggState()
        /// void SetAggState(AggStateEnum aggState)
        /// </remarks>
        internal AggStateEnum AggState;  // uint aggState: 8;

        internal bool IsPrepared    // IsPrepared()
        {
            get { return (AggState >= AggStateEnum.Prepared); }
        }

        /// <summary>
        /// Whether any constituents have errors. This is immutable.
        /// </summary>
        /// <remarks>
        /// bool HasErrors()
        /// void SetHasErrors(bool bv)
        /// </remarks>
        internal bool HasErrors = false;    // uint fHasErrors: 1;

        /// <summary>
        /// (R) Whether any constituents are unresolved. This is immutable.
        /// </summary>
        /// <remarks>
        /// bool IsUnresolved()
        /// void SetUnresolved(bool bv)
        /// </remarks>
        internal bool Unresolved = false;   // uint fUnres: 1;
 
        //------------------------------------------------------------
        // TYPESYM.SetSystemType
        //
        /// <summary></summary>
        /// <param name="ty"></param>
        //------------------------------------------------------------
        virtual internal void SetSystemType(Type ty, bool replace)
        {
            if (this.type == null || replace)
            {
                this.type = ty;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.InitFromParent
        //
        /// <summary>
        /// Fill in the state tracking information for a "derived" type
        /// from the information in the parent.
        /// </summary>
        //------------------------------------------------------------
        internal void InitFromParent()
        {
            DebugUtil.Assert(!IsAGGTYPESYM);

            TYPESYM typePar = ParentSym as TYPESYM;
            if (typePar != null)
            {
                this.HasErrors = typePar.HasErrors;
                this.Unresolved = typePar.Unresolved;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.FundamentalType
        //
        /// <summary>
        /// <para>
        /// Given a symbol, determine its fundemental type.
        /// This is the type that indicate how the item is stored
        /// and what instructions are used to reference if.
        /// The fundemental types are:
        ///     one of the integral/float types (includes enums with that underlying type)
        ///     reference type
        ///     struct/value type
        /// </para>
        /// <para>(fundType() in sscli)</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FUNDTYPE FundamentalType()
        {
            switch (this.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    AGGSYM sym = (this as AGGTYPESYM).GetAggregate();
                    DebugUtil.Assert(
                        sym.HasResolvedBaseClasses ||
                        (sym.IsPredefinedType && sym.PredefinedTypeID < PREDEFTYPE.OBJECT));
                    // Treat enums like their underlying types.
                    if (sym.IsEnum)
                    {
                        sym = sym.UnderlyingTypeSym.GetAggregate();
                        DebugUtil.Assert(sym.IsStruct);
                    }

                    if (sym.IsStruct)
                    {
                        // Struct type could be predefined (int, long, etc.) or some other struct.
                        if (sym.IsPredefinedType)
                        {
                            return PredefType.InfoTable[(int)sym.PredefinedTypeID].ft;
                        }
                        return FUNDTYPE.STRUCT;
                    }
                    return FUNDTYPE.REF;  // Interfaces, classes, delegates are reference types.

                case SYMKIND.TYVARSYM:
                    return FUNDTYPE.VAR;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.NULLSYM:
                case SYMKIND.DYNAMICSYM:    // CS4
                    return FUNDTYPE.REF;

                case SYMKIND.PTRSYM:
                    return FUNDTYPE.PTR;

                case SYMKIND.NUBSYM:
                    return FUNDTYPE.STRUCT;

                default:
                    return FUNDTYPE.NONE;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.UnderlyingType
        //
        /// <summary>
        /// <para>If enum, return its underlying type. Otherwise return itself.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM UnderlyingType()
        {
            if (this.IsAGGTYPESYM && GetAggregate().IsEnum)
            {
                return GetAggregate().UnderlyingTypeSym;
            }
            return this;
        }

        //------------------------------------------------------------
        // TYPESYM.GetNakedType
        //
        /// <summary>
        /// <para>Strips off ARRAYSYM, PARAMMODSYM, PTRSYM, PINNEDSYM and optionally NUBSYM
        /// and returns the result.</para>
        /// <para>(In sscli, the default value of stripNum is false.)</para>
        /// </summary>
        /// <param name="stripNub">If true, get the underlying types of nullable types.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM GetNakedType(bool stripNub)   // = false);   
        {
            for (TYPESYM type = this; ; )
            {
                switch (type.Kind)
                {
                    case SYMKIND.NUBSYM:
                        if (!stripNub) return type;
                        goto case SYMKIND.PINNEDSYM;

                    // Fall through.
                    case SYMKIND.ARRAYSYM:
                    case SYMKIND.PARAMMODSYM:
                    case SYMKIND.MODOPTTYPESYM:
                    case SYMKIND.PTRSYM:
                    case SYMKIND.PINNEDSYM:
                        type = type.ParentSym as TYPESYM;
                        break;

                    default:
                        return type;
                }
            }
        }

        //------------------------------------------------------------
        // TYPESYM.GetNakedAgg
        //
        /// <summary>
        /// <para>If the naked type of this is AGGTYPESYM, convert this to AGGSYM and return it.
        /// Otherwise, return null.</para>
        /// <para>(In sscli, stripNub has the default value false.)</para>
        /// </summary>
        /// <param name="stripNub">If true, get the underlying types of nullable types.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetNakedAgg(bool stripNub)  // = false);
        {
            TYPESYM type = GetNakedType(stripNub);
            if (type.IsAGGTYPESYM)
            {
                return (type as AGGTYPESYM).GetAggregate();
            }
            return null;
        }

        //------------------------------------------------------------
        // TYPESYM.GetAggregate
        //
        /// <summary>
        /// <para>This must be of AGGTYPESYM.
        /// If holds, return the parent sym as AGGSYM.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetAggregate()
        {
            DebugUtil.Assert(IsAGGTYPESYM);
            return (this as AGGTYPESYM).ParentAggSym;
        }

        //------------------------------------------------------------
        // TYPESYM.StripNubs (1)
        //
        /// <summary>
        /// Return the TYPESYMs representing underlying types of nullable types.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM StripNubs()
        {
            TYPESYM type;
            for (type = this; type != null && type.IsNUBSYM; type = type.ParentSym as TYPESYM)
            {
                ;
            }
            return type;
        }

        //------------------------------------------------------------
        // TYPESYM.StripNubs (2)
        //
        /// <summary>
        /// Return the TYPESYMs representing underlying types of nullable types.
        /// </summary>
        /// <param name="stripCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM StripNubs(out int stripCount)
        {
            stripCount = 0;
            TYPESYM type;
            for (type = this; type != null && type.IsNUBSYM; type = type.ParentSym as TYPESYM)
            {
                ++stripCount;
            }
            return type;
        }

        //------------------------------------------------------------
        // TYPESYM.StripAllBufOneNub
        //
        /// <summary>
        /// Return the TYPESYM for Nullable&lt;T&gt; (T is the underlying type and is not nullable).
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM StripAllButOneNub()
        {
            if (this.IsNUBSYM == false)
            {
                return this;
            }
            TYPESYM type;
            for (type = this; type.ParentSym.IsNUBSYM; type = type.ParentSym as TYPESYM)
            {
                ;
            }
            return type;
        }

        //------------------------------------------------------------
        // TYPESYM.AsATSorNUBSYM
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM AsATSorNUBSYM()
        {
            if (IsNUBSYM)
            {
                return (this as NUBSYM).GetAggTypeSym();
            }
            else
            {
                return (this as AGGTYPESYM);
            }
        }

        //------------------------------------------------------------
        // TYPESYM.IsATSorNUBSYM
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsATSorNUBSYM()
        {
            return (IsNUBSYM == true || IsAGGTYPESYM == true);
        }

        //------------------------------------------------------------
        // TYPESYM.IsDelegateType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsDelegateType()
        {
            return (this.IsAGGTYPESYM && this.GetAggregate().IsDelegate);
        }

        //------------------------------------------------------------
        // TYPESYM.IsSimpleType
        //
        /// <summary>
        /// A few types are considered "simple" types for purposes of conversions and so on.
        /// They are the fundemental types the compiler knows about for operators and conversions.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsSimpleType()
        {
            return (this.IsPredefined() &&
                    PredefType.InfoTable[(int)this.GetPredefType()].isSimple);
        }

        //------------------------------------------------------------
        // TYPESYM.IsSimpleOrEnum
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsSimpleOrEnum()
        {
            return IsSimpleType() || IsEnumType();
        }

        //------------------------------------------------------------
        // TYPESYM.IsSimpleOrEnumOrString
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsSimpleOrEnumOrString()
        {
            return IsSimpleType() ||
                this.IsPredefType(PREDEFTYPE.STRING) ||
                this.IsEnumType();
        }

        //------------------------------------------------------------
        // TYPESYM.IsSimpleOrEnumOrStringOrPtr
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsSimpleOrEnumOrStringOrPtr()
        {
            return IsSimpleType() ||
                this.IsPredefType(PREDEFTYPE.STRING) ||
                this.IsEnumType() ||
                this.IsPTRSYM;
        }

        //------------------------------------------------------------
        // TYPESYM.IsSimpleOrEnumOrStringOrAnyPtr
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsSimpleOrEnumOrStringOrAnyPtr()
        {
            return IsSimpleType() ||
                this.IsPredefType(PREDEFTYPE.STRING) ||
                this.IsPTRSYM ||
                this.IsPredefType(PREDEFTYPE.INTPTR) ||
                this.IsPredefType(PREDEFTYPE.UINTPTR) ||
                this.IsEnumType();
        }

        //------------------------------------------------------------
        // TYPESYM.IsPointerLike
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsPointerLike()
        {
            return IsPTRSYM ||
                this.IsPredefType(PREDEFTYPE.INTPTR) ||
                this.IsPredefType(PREDEFTYPE.UINTPTR);
        }

        //------------------------------------------------------------
        // TYPESYM.IsQSimpleType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsQSimpleType()
        {
            return (this.IsPredefined() &&
                PredefType.InfoTable[(int)this.GetPredefType()].isQSimple);
        }

        //------------------------------------------------------------
        // TYPESYM.IsNumericType
        //
        /// <summary>
        /// A few types are considered "numeric" types.
        /// They are the fundemental number types
        /// the compiler knows about for operators and conversions.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsNumericType()
        {
            return (this.IsPredefined() &&
                  PredefType.InfoTable[(int)this.GetPredefType()].isNumeric);
        }

        //------------------------------------------------------------
        // TYPESYM.IsStructOrEnum
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsStructOrEnum()
        {
            return (IsAGGTYPESYM &&
                (GetAggregate().IsStruct || GetAggregate().IsEnum)) || IsNUBSYM;
        }

        //------------------------------------------------------------
        // TYPESYM.IsStructType
        //
        /// <summary>
        /// Returns true if the type is any struct type
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsStructType()
        {
            return this.IsAGGTYPESYM && this.GetAggregate().IsStruct || this.IsNUBSYM;
        }

        //------------------------------------------------------------
        // TYPESYM.IsEnumType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsEnumType()
        {
            return (IsAGGTYPESYM && GetAggregate().IsEnum);
        }

        //------------------------------------------------------------
        // TYPESYM.IsInterfaceType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsInterfaceType()
        {
            return (this.IsAGGTYPESYM && this.GetAggregate().IsInterface);
        }

        //------------------------------------------------------------
        // TYPESYM.IsClassType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsClassType()
        {
            return (this.IsAGGTYPESYM && this.GetAggregate().IsClass);
        }

        //------------------------------------------------------------
        // TYPESYM.UnderlyingEnumType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM UnderlyingEnumType()
        {
            DebugUtil.Assert(IsEnumType());

            if (!IsEnumType())
            {
                return null;
            }
            return GetAggregate().UnderlyingTypeSym;
        }

        //------------------------------------------------------------
        // TYPESYM.IsUnsigned
        //
        /// <summary>
        /// byte, ushort, uint, ulong, and enums of the above
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsUnsigned()
        {
            if (this.IsAGGTYPESYM)
            {
                AGGTYPESYM sym = this as AGGTYPESYM;
                if (sym.IsEnumType())
                {
                    sym = sym.UnderlyingEnumType();
                }
                if (sym.IsPredefined())
                {
                    PREDEFTYPE pt = sym.GetPredefType();
                    return pt == PREDEFTYPE.UINTPTR ||
                        pt == PREDEFTYPE.BYTE ||
                        (pt >= PREDEFTYPE.USHORT && pt <= PREDEFTYPE.ULONG);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return this.IsPTRSYM;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.IsUnsafe
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool IsUnsafe()
        {
            // Pointer types are the only unsafe types.
            // Note that generics may not be instantiated with pointer types
            return (this != null &&
                (this.IsPTRSYM || (this.IsARRAYSYM && (this as ARRAYSYM).ElementTypeSym.IsUnsafe())));
        }

        //------------------------------------------------------------
        // TYPESYM.IsPredefType
        //
        /// <summary>
        /// Is this type a particular predefined type?
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsPredefType(PREDEFTYPE pt)
        {
            if (this.IsAGGTYPESYM)
            {
#if DEBUG
                bool br = (this as AGGTYPESYM).GetAggregate().IsPredefinedType;
                PREDEFTYPE pttmp = (this as AGGTYPESYM).GetAggregate().PredefinedTypeID;
#endif
                return (
                    (this as AGGTYPESYM).GetAggregate().IsPredefinedType &&
                    ((this as AGGTYPESYM).GetAggregate().PredefinedTypeID == pt));
            }
            return (this.IsVOIDSYM && pt == PREDEFTYPE.VOID);
        }

        //------------------------------------------------------------
        // TYPESYM.IsPredefined
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsPredefined()
        {
            return this.IsAGGTYPESYM && this.GetAggregate().IsPredefinedType;
        }

        //------------------------------------------------------------
        // TYPESYM.GetPredefType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PREDEFTYPE GetPredefType()
        {
            DebugUtil.Assert(IsPredefined());
            return (PREDEFTYPE)this.GetAggregate().PredefinedTypeID;
        }

        //------------------------------------------------------------
        // TYPESYM.IsSpecialByRefType
        //
        /// <summary>
        /// Is this type below ?
        /// <list type="bullet">
        /// <item>System.TypedReference</item>
        /// <item>System.ArgIterator</item>
        /// <item>System.RuntimeArgumentHandle</item>
        /// </list>
        /// (used for errors because these types can't go certain places)
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsSpecialByRefType()
        {
            if (this == null)
            {
                return false;
            }
            else if (this.IsPredefined())
            {
                PREDEFTYPE pt = this.GetPredefType();
                return (
                    pt == PREDEFTYPE.REFANY ||
                    pt == PREDEFTYPE.ARGITERATOR ||
                    pt == PREDEFTYPE.ARGUMENTHANDLE
                    );
            }
            return false;
        }

        //------------------------------------------------------------
        // TYPESYM.IsGenericInstance
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsGenericInstance()
        {
            return (
                IsAGGTYPESYM &&
                GetAggregate().TypeVariables.Count != 0);
        }

        //------------------------------------------------------------
        // TYPESYM.IsSecurityAttribute
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsSecurityAttribute()
        {
            return IsAGGTYPESYM && GetAggregate().IsSecurityAttribute;
        }

        //------------------------------------------------------------
        // TYPESYM.IsFabricated
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool IsFabricated()
        {
            return (
                this != null &&
                this.IsAGGTYPESYM && this.GetAggregate().IsFabricated);
        }

        //------------------------------------------------------------
        // TYPESYM.IsStaticClass
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsStaticClass()
        {
            if (this == null)
            {
                return false;
            }

            AGGSYM agg = this.GetNakedAgg(false);
            if (agg == null)
            {
                return false;
            }
            if (!agg.IsStatic)
            {
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // TYPESYM.IsValueType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsValueType()
        {
            switch (this.Kind)
            {
                case SYMKIND.TYVARSYM:
                    return (this as TYVARSYM).IsValueType();

                case SYMKIND.AGGTYPESYM:
                    return (this as AGGTYPESYM).GetAggregate().IsValueType;

                case SYMKIND.NUBSYM:
                    return true;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.IsNonNullableValueType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsNonNullableValueType()
        {
            switch (this.Kind)
            {
                case SYMKIND.TYVARSYM:
                    return (this as TYVARSYM).IsNonNullableValueType();

                case SYMKIND.AGGTYPESYM:
                    return (this as AGGTYPESYM).GetAggregate().IsValueType;

                case SYMKIND.NUBSYM:
                    return false;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.IsReferenceType
        //
        /// <summary>
        /// <para>is this type known to be a reference type by the C# spec</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsReferenceType()
        {
            switch (this.Kind)
            {
                case SYMKIND.ARRAYSYM:
                case SYMKIND.NULLSYM:
                    return true;

                case SYMKIND.TYVARSYM:
                    return (this as TYVARSYM).IsReferenceType();

                case SYMKIND.AGGTYPESYM:
                    return (this as AGGTYPESYM).GetAggregate().IsReferenceType;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.IsReferenceTypeInVerifier
        //
        /// <summary>
        /// <para>is this type known to be a reference type by the JIT</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsReferenceTypeInVerifier()
        {
            // NOTE: type variables are never considered reference types by the Verifier
            switch (this.Kind)
            {
                case SYMKIND.ARRAYSYM:
                case SYMKIND.NULLSYM:
                    return true;

                case SYMKIND.AGGTYPESYM:
                    return (this as AGGTYPESYM).GetAggregate().IsReferenceType;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // TYPESYM.IsNeverSameType
        //
        /// <summary>
        /// True if ANONMETHSYM or METHGRPSYM, or if ERRORSYM and ParentSym is null.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsNeverSameType()
        {
            return (
                IsANONMETHSYM ||
                IsMETHGRPSYM ||
                IsLAMBDAEXPRSYM ||
                (IsERRORSYM && ParentSym == null));
        }

        //------------------------------------------------------------
        // TYPESYM.CanBeConst
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CanBeConst()
        {
            return IsReferenceType() || IsSimpleOrEnumOrString();
        }

        //------------------------------------------------------------
        // TYPESYM.CanBeVolatile
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CanBeVolatile()
        {
            FUNDTYPE ft = FundamentalType();

            bool result = (
                ft == FUNDTYPE.REF ||
                ft == FUNDTYPE.PTR ||
                ft == FUNDTYPE.R4 ||
                ft == FUNDTYPE.I4 ||
                ft == FUNDTYPE.U4 ||
                ft == FUNDTYPE.I2 ||
                ft == FUNDTYPE.U2 ||
                ft == FUNDTYPE.I1 ||
                ft == FUNDTYPE.U1 ||
                (ft == FUNDTYPE.VAR && (this as TYVARSYM).IsReferenceType()));

            if (!result && IsPredefined())
            {
                PREDEFTYPE pt = GetPredefType();
                result = (pt == PREDEFTYPE.INTPTR || pt == PREDEFTYPE.UINTPTR);
            }
            return result;
        }

        //------------------------------------------------------------
        // TYPESYM.CopyFrom
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal override void CopyFrom(SYM sym)
        {
            TYPESYM other = sym as TYPESYM;
            if (other == null)
            {
                return;
            }
            base.CopyFrom(other);

            this.type = other.type;
            this.AssemblyQualifiedName = other.AssemblyQualifiedName;
            this.AggState = other.AggState;
            this.HasErrors = other.HasErrors;
            this.Unresolved = other.Unresolved;
        }

#if DEBUG
        //------------------------------------------------------------
        // TYPESYM Debug
        //------------------------------------------------------------
        virtual internal Type DebugGetSystemType()
        {
            if (this.IsTYVARSYM)
            {
                return (this as TYVARSYM).GetGenericParameterType();
            }
            return this.Type;
        }

        internal void DebugType(StringBuilder sb)
        {
            if (this.SymID == 1394)
            {
                ;
            }

            string strTemp;

            if (this.DebugGetSystemType() != null)
            {
                sb.AppendFormat("Type                  : {0}\n", this.DebugGetSystemType().FullName);
            }
            else
            {
                sb.AppendFormat("Type                  :\n");
            }

            strTemp = String.IsNullOrEmpty(this.AssemblyQualifiedName) ? "" : this.AssemblyQualifiedName;
            sb.AppendFormat("AssemblyQualifiedName : {0}\n", strTemp);

            //sb.AppendFormat("AggState              : {0}\n", this.AggState);
            //sb.AppendFormat("HasErrors             : {0}\n", this.HasErrors);
            //sb.AppendFormat("Unresolved            : {0}\n", this.Unresolved);
            sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugType(sb);
        }

        internal virtual void DebugGeneric(StringBuilder sb)
        {
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class OUTFILESYM
    //
    /// <summary>
    /// <para>OUTFILESYM -- a symbol that represents an output file we are creating.
    /// Its children all all input files that contribute.
    /// The symbol name is the file name.</para>
    /// <para>Derives from PARENTSYM.</para>
    /// </summary>
    //======================================================================
    internal class OUTFILESYM : PARENTSYM
    {
        /// <summary>
        /// If out: option is not specified, provisionally use this name.
        /// </summary>
        internal const string ProvisionalFileName = "*";

        //------------------------------------------------------------
        // OUTFILESYM Fields and Properties (1) Assembly ID
        //------------------------------------------------------------
        /// <summary>
        /// <para>The module id for the output module
        /// Only set if this is a real output file with  real sources as INFILESYMs</para>
        /// <para>Use assembly ID as module ID in sscli.</para>
        /// </summary>
        private int assemblyID = 0; // aid

        //------------------------------------------------------------
        // OUTFILESYM Fields and Properties (2) FileInfo and file names.
        //------------------------------------------------------------
        protected FileInfo fileInfo = null;

        internal FileInfo FileInfo
        {
            get { return this.fileInfo; }
        }

        internal void SetFileInfo(FileInfo fi)
        {
            this.fileInfo = fi;
        }

        internal bool HasFileInfo
        {
            get { return (this.fileInfo != null); }
        }

        /// <summary>
        /// (R) A simple file name, with an extension and without directory.
        /// </summary>
        internal override string Name
        {
            get
            {
                return (fileInfo != null ? fileInfo.Name : base.Name);
            }
        }
        internal string FullName
        {
            get { return (fileInfo != null ? fileInfo.FullName : null); }
        }

        internal string Directory
        {
            get { return (this.fileInfo != null ? fileInfo.Directory.FullName : null); }
        }

        //------------------------------------------------------------
        // OUTFILESYM Fields and Properties (3) AssemblyBuilder and ModuleBuilder.
        //------------------------------------------------------------
        //protected AssemblyBuilderAccess builderAccessFlag = AssemblyBuilderAccess.Run;
        //protected AssemblyName assemblyName = null;
        //protected AssemblyBuilder assemblyBuilder = null;

        protected CAssemblyBuilderEx assemblyBuilderEx = null;

        internal CAssemblyBuilderEx AssemblyBuilderEx
        {
            get { return this.assemblyBuilderEx; }
        }

        internal AssemblyFlagsEnum AssemblyBuilderFlags = 0;

        protected CModuleBuilderEx moduleBuilderEx = null;

        internal CModuleBuilderEx ModuleBuilderEx
        {
            get { return this.moduleBuilderEx; }
        }

        //------------------------------------------------------------
        // OUTFILESYM Fields and Properties (4) Target Type
        //------------------------------------------------------------
        internal TargetType Target = TargetType.None;

        /// <summary>
        /// Is exe or winexe?
        /// </summary>
        internal bool IsExe
        {
            get
            {
                return (
                    this.Target == TargetType.Exe ||
                    this.Target == TargetType.WinExe);
            }
        }

        /// <summary>
        /// A dll or an exe?
        /// </summary>
        //internal bool IsDLL = false;
        internal bool IsDLL // isDll: 1;
        {
            get { return (this.Target == TargetType.Library); }
        }

        /// <summary>
        /// Whether to make this an assembly.
        /// </summary>
        //internal bool IsManifest = false;
        internal bool IsManifest    // isManifest: 1;
        {
            get
            {
                return (
                    this.Target == TargetType.Exe ||
                    this.Target == TargetType.Library ||
                    this.Target == TargetType.WinExe);
            }
        }

        /// <summary>
        /// A console application?
        /// </summary>
        //internal bool IsConsoleApp = true;
        internal bool IsConsoleApp  // isConsoleApp: 1;
        {
            get { return (this.Target == TargetType.Exe); }
        }

        /// <summary>
        /// Is this a resource file that we linked?
        /// </summary>
        internal bool IsResource = false;   // isResource: 1;

        //------------------------------------------------------------
        // OUTFILESYM Fields and Properties (5) etc
        //------------------------------------------------------------
        /// <summary>
        /// Has the 'Multiple Entry Points' error been reported for this file?
        /// </summary>
        internal bool HasMultiEntryPointsErrorReported = false; // multiEntryReported: 1;

        /// <summary>
        /// True if we autogenerate .RES file.  False if we use resourceFile
        /// </summary>
        internal bool MakeResFile = false;  // makeResFile: 1;

        /// <summary>
        /// True if this assembly will be strong named signed via an attribute
        /// (i.e. either KeyFile or DelaySign attributes)
        /// </summary>
        internal bool HasSigningAttribute = false;  // fHasSigningAttribute: 1;

        /// <summary>
        /// Default character set marshalling (from module level attribute)
        /// </summary>
        internal Encoding DefaultEncoding = Encoding.Default;   // int defaultCharSet;

        /// <summary>
        /// (RW) Default codepage marshalling (from module level attribute)
        /// </summary>
        internal int DefaultCodePage
        {
            get { return DefaultEncoding.CodePage; }
            set { DefaultEncoding = System.Text.Encoding.GetEncoding(value); }
        }

        /// <summary>
        /// (RW) Default codepage marshalling (from module level attribute)
        /// </summary>
        /// <remarks> (MSDN)
        /// The default is Unicode on Windows NT, Windows 2000, Windows XP,
        /// and the Windows Server 2003 family;
        /// the default is Ansi on Windows 98 and Windows Me.
        /// Although the common language runtime default is Auto,
        /// languages may override this default.
        /// For example, by default C# marks all methods and types as Ansi. 
        /// </remarks>
        internal System.Runtime.InteropServices.CharSet DefaultCharSet
            = System.Runtime.InteropServices.CharSet.Ansi;

        /// <summary>
        /// Image base (or 0 for default)
        /// </summary>
        internal ulong ImageBaseAddress = 0;    // ULONGLONG imageBase;

        /// <summary>
        /// File Alignment (or 0 for default)
        /// </summary>
        internal uint FileAlignment = 0;    // ULONG fileAlign;

        /// <summary>
        /// Win32 Resource file.
        /// </summary>
        internal FileInfo Win32ResourceFileInfo = null; // PWSTR resourceFile;

        /// <summary>
        /// Win32 Icon file.
        /// </summary>
        internal FileInfo Win32IconFileInfo = null; // PWSTR iconFile;

        /// <summary>
        /// global class of which we hang native methodrefs
        /// </summary>
        internal AGGSYM GlobalClassSym = null;  // * globalClass;

        /// <summary>
        /// User specified entryPoint Fully-Qualified Class name
        /// </summary>
        internal string EntryClassName = null;  // PWSTR entryClassName;

        /// <summary>
        /// 'Main' method symbol (for EXEs only)
        /// </summary>
        internal METHSYM EntryMethodSym = null; // PMETHSYM entrySym;

        /// <summary>
        /// MetaData token for the file
        /// </summary>
        internal int FileID = 0;   // mdFile idFile;

        /// <summary>
        /// Used for scoped TypeRefs
        /// </summary>
        internal int ModRefID = 0; // mdModuleRef idModRef;

        /// <summary>
        /// Attributes for this module
        /// </summary>
        internal GLOBALATTRSYM AttributesSym = null;    // PGLOBALATTRSYM attributes;
        internal GLOBALATTRSYM LastAttributesSym = null;

        /// <summary>
        /// The module name as it will appear in metadata
        /// </summary>
        internal string ModuleName = null;  // NAME * nameModule;

        /// <summary>
        /// The assembly's simple name (only set if isManifest)
        /// </summary>
        internal string SimpleAssemblyName = null;  // NAME * simpleName;

        /// <summary>
        /// Chached value of public key token (gotten from command-line arguments)
        /// </summary>
        private List<byte> publicKeyToken = new List<byte>();
        /// <summary>
        /// (R) Chached value of public key token (gotten from command-line arguments)
        /// </summary>
        internal List<byte> PublicKeyToken
        {
            get { return publicKeyToken; }
        }

        /// <summary>
        /// (R) size of public key token (-1 for un-inited, 0 for 'null')
        /// </summary>
        internal int PublicKeyTokenCount
        {
            get { return (publicKeyToken != null ? publicKeyToken.Count : 0); }
        }

        internal bool HasNoPublicKeyToken
        {
            get { return (publicKeyToken.Count == 0); }
        }

        /// <summary>
        /// nummber of input files
        /// </summary>
        internal int InputFileCount = 0;

        internal METHSYM methodWithEmittedModuleInfo = null;

        /// <summary>
        /// if !null, then is the filename for the output file, specified by /pdb:filename.
        /// This may be either a path or a file.
        /// if this is not set, we default to the output filename as the name for the pdb.
        /// </summary>
        internal string PDBFileName = null;

        //------------------------------------------------------------
        // OUTFILESYM Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal OUTFILESYM() { }

        //------------------------------------------------------------
        // OUTFILESYM.GetModuleID
        //
        /// <summary>
        /// <para>The module id for the output module.</para>
        /// <para>Use assembly ID as module ID in sscli.</para>
        /// </summary>
        //------------------------------------------------------------
        internal int GetModuleID()
        {
            INFILESYM sym = FirstInFileSym();
            DebugUtil.Assert(sym != null);

            int id = sym.GetAssemblyID();
            DebugUtil.Assert(id == Kaid.ThisAssembly);

            return assemblyID;
        }

        //------------------------------------------------------------
        // OUTFILESYM.SetModuleID
        //
        /// <summary>
        /// <para>The module id for the output module.
        /// Only set if this is a real output file with  real sources as INFILESYMs.</para>
        /// <para>Use assembly ID as module ID in sscli.</para>
        /// </summary>
        /// <param name="id"></param>
        //------------------------------------------------------------
        internal void SetModuleID(int id)
        {
            DebugUtil.Assert(this.assemblyID <= 0);
            assemblyID = id;
        }

        //------------------------------------------------------------
        // OUTFILESYM.FirstInFileSym
        //
        /// <summary>
        /// Return the first INFILESYM instance from the child SYM instances.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM FirstInFileSym() // PINFILESYM  firstInfile()
        {
            SYM sym = this.FirstChildSym;
            while (sym != null)
            {
                INFILESYM infile = sym as INFILESYM;
                if (infile != null) return infile;
                sym = sym.NextSym;
            }
            return null;
        }

        //------------------------------------------------------------
        // OUTFILESYM.FirstResourceFileSym
        //
        /// <summary>
        /// Return the first RESFILESYM instance from the child SYM instances.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal RESFILESYM FirstResourceFileSym()
        {
            SYM sym = this.FirstChildSym;
            while (sym != null)
            {
                if (sym.IsRESFILESYM)
                {
                    return (sym as RESFILESYM);
                }
                sym = sym.NextSym;
            }
            return null;
        }

        //------------------------------------------------------------
        // OUTFILESYM.NextOutputFile
        //
        /// <summary>
        /// Find a OUTFILESYM instance in the sym list linked by NextSym field.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal OUTFILESYM NextOutputFile()
        {
            SYM sym = NextSym;
            while (sym != null)
            {
                if (sym.IsOUTFILESYM)
                {
                    return (sym as OUTFILESYM);
                }
                sym = sym.NextSym;
            }
            return null;
        }

        //------------------------------------------------------------
        // OUTFILESYM.IsUnnamed (Property)
        //
        /// <summary>
        /// Return true if this.Name is "?".
        /// </summary>
        //------------------------------------------------------------
        internal bool IsUnnamed
        {
            get
            {
                return (
                    this.fileInfo == null ||
                    String.IsNullOrEmpty(this.Name) ||
                    this.Name == OUTFILESYM.ProvisionalFileName ||
                    this.Name == "?");
            }
        }

        //------------------------------------------------------------
        // OUTFILESYM.AddModule
        //
        /// <summary>
        /// Does nothing, return true.
        /// </summary>
        /// <param name="infileSym"></param>
        //------------------------------------------------------------
        internal bool AddModule(INFILESYM infileSym)
        {
            return true;
        }

        //------------------------------------------------------------
        // OUTFILESYM.SetEntryPoint
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetEntryPoint()
        {
            if (this.IsDLL ||
                this.assemblyBuilderEx == null ||
                this.EntryMethodSym == null)
            {
                return false;
            }

            return this.assemblyBuilderEx.SetEntryMethod(this.EntryMethodSym.MethodInfo);
        }

        //------------------------------------------------------------
        // OUTFILESYM.BeginOutputFile
        //
        /// <summary>
        /// <para>Create the AssemblyBuilder instance.</para>
        /// <para>PEFile::BeginOutputFile in sscli.</para>
        /// </summary>
        /// <param name="compiler"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BeginAssemblyOutputFile(COMPILER compiler)
        {
            if (this.assemblyBuilderEx != null || compiler == null)
            {
                DebugUtil.Assert(false);
                //EndOutputFile(false);
                return false;
            }
            DebugUtil.Assert(!this.IsUnnamed);

            this.CreateAssemblyBuilderEx(
                compiler.Controller,
                this.AssemblyBuilderFlags,
                compiler.Linker);

            return (this.AssemblyBuilderEx != null);
        }

        //------------------------------------------------------------
        // OUTFILESYM.EndAssemblyOutputFile
        //
        /// <summary>
        /// <para>Save tha assmebly file.</para>
        /// <para>PEFile::EndOutputFile in sscli.</para>
        /// </summary>
        /// <param name="writeFile"></param>
        //------------------------------------------------------------
        internal void EndAssemblyOutputFile(bool writeFile)
        {
            if (this.assemblyBuilderEx != null && writeFile)
            {
                this.AssemblyBuilderEx.Save();
            }
        }

        //------------------------------------------------------------
        // OUTFILESYM.BeginModuleOutputFile
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="asmOutfileSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BeginModuleOutputFile(COMPILER compiler, OUTFILESYM asmOutfileSym)
        {
            DebugUtil.Assert(compiler != null);

            if (this.IsManifest)
            {
                this.moduleBuilderEx
                    = this.assemblyBuilderEx.CreateManifestModuleBuilderEx();
            }
            else
            {
                CAssemblyBuilderEx asmBuilderEx = asmOutfileSym.AssemblyBuilderEx;
                if (asmBuilderEx == null)
                {
                    return false;
                }

                string modName = this.fileInfo.Name;
                DebugUtil.Assert(!String.IsNullOrEmpty(modName));

                this.moduleBuilderEx
                    = asmOutfileSym.assemblyBuilderEx.CreateModuleBuilderEx(
                    modName,
                    this.FileInfo);
            }

            return (this.ModuleBuilderEx != null);
        }

        //------------------------------------------------------------
        // OUTFILESYM.CreateAssemblyBuilderEx
        //
        /// <summary>
        /// Create an CAssemblyBuilderEx instance.
        /// This instance has no AssemblyBuilder instance yet.
        /// </summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        internal CAssemblyBuilderEx CreateAssemblyBuilderEx(
            CController cntr,
            AssemblyFlagsEnum flags,
            CAsmLink linker)
        {
            flags |= this.AssemblyBuilderFlags;

            this.assemblyBuilderEx = new CAssemblyBuilderEx(cntr);
            if (!this.assemblyBuilderEx.Init(flags, linker))
            {
                this.assemblyBuilderEx = null;
            }
            return this.assemblyBuilderEx;
        }

        //------------------------------------------------------------
        // OUTFILESYM.DefineAssembly
        //
        /// <summary></summary>
        /// <param name="requiredSet"></param>
        /// <param name="optionalSet"></param>
        /// <param name="refusedSet"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool DefineAssembly(
            COptionManager options,
            CAssemblyInitialAttributes initalAttrs,
            PermissionSet requiredSet,
            PermissionSet optionalSet,
            PermissionSet refusedSet,
            out Exception excp)
        {
            DebugUtil.Assert(this.assemblyBuilderEx != null);
            DebugUtil.Assert(!String.IsNullOrEmpty(this.Name));

            return this.assemblyBuilderEx.DefineAssembly(
                Path.GetFileNameWithoutExtension(this.Name),
                this.fileInfo,
                AssemblyBuilderAccess.RunAndSave,
                options,
                initalAttrs,
                requiredSet,
                optionalSet,
                refusedSet,
                out excp);
        }

        //------------------------------------------------------------
        // OUTFILESYM.CreateManifestModuleBuilder
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CreateManifestModuleBuilder()
        {
            this.moduleBuilderEx = this.assemblyBuilderEx.CreateManifestModuleBuilderEx();
            return (this.moduleBuilderEx != null);
        }

        //------------------------------------------------------------
        // OUTFILESYM.DefineUnmanagedEmbeddedResource
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void DefineUnmanagedEmbeddedResource()
        {
            if (this.AssemblyBuilderEx == null)
            {
                return;
            }

            if (this.Win32ResourceFileInfo != null)
            {
                this.AssemblyBuilderEx.DefineUnmanagedEmbeddedResources(
                    this.Win32ResourceFileInfo);
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // OUTFILESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class RESFILESYM
    //
    /// <summary>
    /// <para>RESFILESYM - a symbol that represents a resource input file.
    /// Its parent is the output file it contributes to, or Default
    /// if it will be embeded in the Assembly file.
    /// The symbol name is the resource Identifier.</para>
    /// </summary>
    //======================================================================
    internal class RESFILESYM : SYM
    {
        //------------------------------------------------------------
        // RESFILESYM Fields and Properties
        //------------------------------------------------------------
        protected FileInfo fileInfo = null;

        internal string LogicalName = null;

        internal FileInfo FileInfo
        {
            get { return this.fileInfo; }
        }

        internal override string Name
        {
            get { return (this.fileInfo != null ? fileInfo.Name : null); }
        }

        internal string FileName
        {
            get { return (this.fileInfo != null ? this.fileInfo.Name : null); }
        }

        internal string FullName
        {
            get { return (this.fileInfo != null ? this.fileInfo.FullName : null); }
        }

        internal override string SearchName
        {
            get { return this.LogicalName; }
        }

        internal ResourceAttributes Accessibility = ResourceAttributes.Public;

        internal bool IsEmbedded = true;

        /// <summary>
        /// (R) Return the parent sym as OUTPUTFILESYM.
        /// </summary>
        internal OUTFILESYM OutputFile
        {
            get { return (ParentSym != null ? (ParentSym as OUTFILESYM) : null); }
        }

        //------------------------------------------------------------
        // RESFILESYM.Set
        //
        /// <summary></summary>
        /// <param name="fi"></param>
        //------------------------------------------------------------
        internal void Set(FileInfo fi, string logName)
        {
            DebugUtil.Assert(fi != null && !String.IsNullOrEmpty(fi.Name));

            this.fileInfo = fi;
            if (String.IsNullOrEmpty(logName))
            {
                this.LogicalName=this.fileInfo.Name;
            }
            else
            {
                this.LogicalName = logName;
            }
        }

        //------------------------------------------------------------
        // RESFILESYM.NextResfile
        //
        /// <summary>
        /// Return the next RESFILESYM instance.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal RESFILESYM NextResfile()
        {
            SYM sym;
            for (sym = NextSym; sym != null && !sym.IsRESFILESYM; sym = sym.NextSym)
            {
                ;
            }
            return (sym as RESFILESYM);
        }

#if DEBUG
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class INFILESYM
    //
    /// <summary>
    /// <para>INFILESYM - a symbol that represents an input file, either source
    /// code or meta-data, of a file we may read. Its parent is the output
    /// file it contributes to. The symbol name is the file name.
    /// Children include MODULESYMs.</para>
    /// <para>Derived from PARENTSYM.para>
    /// </summary>
    //======================================================================
    internal class INFILESYM : PARENTSYM
    {
        //------------------------------------------------------------
        // INFILESYM    Fields and Properties
        //
        // Assembly ID
        //------------------------------------------------------------
        /// <summary>
        /// Which aliases this INFILE is in.
        /// For source INFILESYMs, only bits Kaid.ThisAssembly and Kaid.Global should be set.
        /// </summary>
        private BitSet bitsetFilter = new BitSet(); // bsetFilter

        /// <summary>
        /// Assembly ID
        /// </summary>
        private int assemblyID = Kaid.Nil;  // aid

        /// <summary>
        /// Which assemblies this one grants rights to. May include kaidThisAssembly.
        /// </summary>
        private BitSet bitsetFriend = new BitSet(); // bsetFriend

        //------------------------------------------------------------
        // INFILESYM    Fields and Properties
        //
        // FileInfo and names.
        //------------------------------------------------------------
        protected FileInfo fileInfo = null;
        protected string searchName = null;

        internal FileInfo FileInfo
        {
            get { return this.fileInfo; }
        }

        internal void SetFileInfo(FileInfo fi)
        {
            DebugUtil.Assert(fi != null && !String.IsNullOrEmpty(fi.Name));
            this.fileInfo = fi;
            searchName = this.fileInfo.FullName.ToLower();
        }

        /// <summary>
        /// (R) A simple file name without a directory name.
        /// </summary>
        internal override string Name
        {
            get
            {
                return (fileInfo != null ? fileInfo.Name : base.Name);
            }
        }
        internal string FullName
        {
            get
            {
                return (fileInfo != null ? fileInfo.FullName : Name);
            }
        }

        internal override string SearchName
        {
            get { return this.searchName; }
        }

        //------------------------------------------------------------
        // INFILESYM    Fields and Properties
        //
        // A source file or an added module or an referenced assembly?
        //------------------------------------------------------------
        /// <summary>
        /// If true, source code, if false, metadata
        /// </summary>
        internal bool IsSource = true;  // bool isSource: 1;

        /// <summary>
        /// Specified by /addmodule option. Added to this assembly.
        /// </summary>
        protected bool isModule = false;    // bool isAddedModule: 1;

        /// <summary>
        /// (RW) Specified by /addmodule option. Added to this assembly.
        /// </summary>
        internal bool IsModule
        {
            get { return (IsSource == false && isModule == true); }
            set
            {
                if (value == true)
                {
                    isModule = true;
                    IsSource = false;
                }
                else
                {
                    DebugUtil.Assert(false);
                }
            }
        }

        /// <summary>
        /// (RW) Specified by /outerence or /r, /R option.
        /// </summary>
        internal bool IsAssembly
        {
            get { return (IsSource == false && isModule == false); }
            set
            {
                if (value == true)
                {
                    isModule = false;
                    IsSource = false;
                }
                else
                {
                    // Do not set false. Set IsSource ro IsModule.
                    DebugUtil.Assert(false);
                }
            }
        }

        internal bool HasGlobalAttr = false;    // bool hasGlobalAttr: 1;

        /// <summary>
        /// have symbols for this file been defined
        /// </summary>
        internal bool IsDefined = false;    // bool isDefined: 1;

        /// <summary>
        /// have compile time constants been evaluated for this file
        /// </summary>
        internal bool AreConstsEvaled = false;  // bool isConstsEvaled: 1;

        /// <summary>
        /// is this the infilesym for mscorlib.dll
        /// </summary>
        //internal bool IsBCL = false;
        internal bool IsBaseClassLibrary = false;   // bool isBCL: 1;

        /// <summary>
        /// We need to distinguish between CLSCompliantAttribute on the assembly
        /// </summary>
        internal bool HasModuleCLSAttribute = false;    // bool hasModuleCLSattribute: 1;

        //------------------------------------------------------------
        // INFILESYM    Fields and Properties
        //
        // and on the module of added .netmodules
        //------------------------------------------------------------

        /// <summary>
        /// For tracking if we've already reported too many lines for debug info on this file
        /// (only valid if isSource)
        /// </summary>
        internal bool TooManyLineReported = false;  // bool fTooManyLinesReported: 1;

        internal bool FriendAccessUsed = false; // bool fFriendAccessUsed: 1;

        /// <summary>
        /// the top level declaration for this file
        /// </summary>
        internal NSDECLSYM RootNsDeclSym = null;    // * rootDeclaration

        //------------------------------------------------------------
        // INFILESYM    Fields and Properties
        //
        // If metadata, then the following are available.
        //------------------------------------------------------------
        //internal bool IsMetadataImported = false;

        protected CMetadataFile metadataFile = null;

        internal void SetMetadataFile(CMetadataFile md)
        {
            this.metadataFile = md;
        }

        internal CMetadataFile MetadataFile
        {
            get { return this.metadataFile; }
        }

        internal bool IsModuleLoaded
        {
            get { return (metadataFile is CModuleEx); }
        }

        internal CModuleEx ModuleEx
        {
            get { return (metadataFile as CModuleEx); }
        }

        internal bool IsAssemblyLoaded
        {
            get { return (metadataFile is CAssemblyEx); }
        }

        internal CAssemblyEx AssemblyEx
        {
            get { return (metadataFile as CAssemblyEx); }
        }

        internal bool IsMetadataLoaded
        {
            get { return this.IsAssemblyLoaded || this.IsModuleLoaded; }
        }

        internal int CScope = 0;    // cscope

        //internal uint MetadataImportFile = 0;

        /// <summary>
        /// assembly meta-data import interface.
        /// </summary>
        //internal AssemblyEx MetadataAssemblyImport;

        /// <summary>
        /// <para>text version of assembly for attributes</para>
        /// <para></para>
        /// </summary>
        internal string AssemblyNameString  // NAME * assemblyName;
        {
            get { return (metadataFile != null ? metadataFile.AssemblyNameString : null); }
        }
        internal AssemblyName AssemblyNameInstance
        {
            get { return (metadataFile != null ? metadataFile.AssemblyNameObject : null); }
        }

        /// <summary>
        /// (R) The version of assembly.
        /// </summary>
        internal Version AssemblyVersion    // WORD assemblyVersion[4];
        {
            get
            {
                if (this.metadataFile is CAssemblyEx)
                {
                    return (metadataFile as CAssemblyEx).AssemblyVersion;
                }
                return null;
            }
        }

        /// <summary>
        /// Assembly id for use in scoped TypeRefs.
        /// </summary>
        internal int LocalAssemblyID;   // mdAssemblyRef idLocalAssembly;

        /// <summary>
        /// The main module - the one with the assembly manifest
        /// </summary>
        internal MODULESYM ManifestModuleSym = null;    // * moduleManifest

        //------------------------------------------------------------
        // INFILESYM    Fields and Properties
        //
        // If a source file, then the following are available.
        //------------------------------------------------------------

        /// <summary>
        /// <para>The top level namespace associated w/ the file</para>
        /// <para>To which the node tree created by parsing is set.</para>
        /// </summary>
        internal NAMESPACENODE NamespaceNode;   // * nspace;

        /// <summary>
        /// <para>Associated source module data</para>
        /// <para>class CSourceData has a field of type CSourceModuleBase, and we can get its value by Module property.
        /// class CSourceModuleBase has SourceText field of type ICSSourceText, which has a source text.</para>
        /// <para>This has the results of parsing.</para>
        /// </summary>
        internal CSourceData SourceData;    // CSourceData * pData;

        // In sscli, ISymUnmanagedDocumentWriter is defined in prebuilt\idl\corsym.h
        //internal ISymUnmanagedDocumentWriter UnmanagedDocumentWriter = null;    // * documentWriter;

#if DEBUG
        private bool fUnionCalled;
#endif

        //------------------------------------------------------------
        // INFILESYM.GetFileName
        //
        /// <summary></summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetFileName(bool fullPath)
        {
            if (this.fileInfo != null)
            {
                return (fullPath ? this.fileInfo.FullName : this.fileInfo.Name);
            }
            return null;
        }

        //------------------------------------------------------------
        // INFILESYM.GetAssemblyID
        //
        /// <summary>
        /// INFILESYM has assemblyID field. return this value.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal int GetAssemblyID()
        {
            DebugUtil.Assert(assemblyID >= Kaid.ThisAssembly);
            return assemblyID;
        }

        //------------------------------------------------------------
        // INFILESYM.SetAssemblyID
        //
        /// <summary>
        /// Set assemblyID and set the flag in bitsetFilter.
        /// </summary>
        /// <param name="aid"></param>
        //------------------------------------------------------------
        new internal void SetAssemblyID(int aid)
        {
            DebugUtil.Assert(this.assemblyID <= 0);
            DebugUtil.Assert(Kaid.ThisAssembly <= aid && aid < Kaid.MinModule);

            this.assemblyID = aid;
            bitsetFilter.SetBit(aid);
            if (aid == Kaid.ThisAssembly) bitsetFilter.SetBit(Kaid.Global);
        }

        //------------------------------------------------------------
        // INFILESYM.AddInternalsVisibleTo
        //
        /// <summary>
        /// Set the flag in bitsetFriend.
        /// </summary>
        /// <param name="aid"></param>
        //------------------------------------------------------------
        internal void AddInternalsVisibleTo(int aid)
        {
            DebugUtil.Assert(0 <= assemblyID && assemblyID < Kaid.MinModule);
            //NOTE: No need to keep track for this assembly.
            DebugUtil.Assert(this.assemblyID > Kaid.ThisAssembly);

            bitsetFriend.SetBit(aid);
        }

        //------------------------------------------------------------
        // INFILESYM.InternalsVisibleTo
        //
        /// <summary>
        /// Return true if the specified flag in bitsetFriend is set.
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool InternalsVisibleTo(int aid)
        {
            return bitsetFriend.TestBit(aid);
        }

        //------------------------------------------------------------
        // INFILESYM.AddToAlias
        //
        /// <summary>
        /// Set a bit of bitsetFilter which corresponds to an assembly ID.
        /// </summary>
        /// <param name="aid"></param>
        //------------------------------------------------------------
        internal void AddToAlias(int aid)
        {
            DebugUtil.Assert(0 <= assemblyID && assemblyID < Kaid.MinModule);
            // NOTE: Anything in this assembly should not be added to other aliases!
            DebugUtil.Assert(this.assemblyID > Kaid.ThisAssembly);
            DebugUtil.Assert(bitsetFilter.TestBit(this.assemblyID));

#if DEBUG
            // If this assert fires, then AddToAlias is being called too late
            // or UnionAliasFilter is being called too early.
            DebugUtil.Assert(fUnionCalled == false);
#endif
            bitsetFilter.SetBit(aid);
        }

        //------------------------------------------------------------
        // INFILESYM.InAlias
        //
        /// <summary></summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InAlias(int aid)
        {
            DebugUtil.Assert(0 <= assemblyID);
            return bitsetFilter.TestBit(aid);
        }

        //------------------------------------------------------------
        // INFILESYM.UnionAliasFilter
        //
        /// <summary>
        /// Unions this INFILESYM's bitsetFilter into the given destination bitset.
        /// In DEBUG we assert if AddToAlias is ever called after this has been called.
        /// </summary>
        /// <param name="bsetDst"></param>
        //------------------------------------------------------------
        internal void UnionAliasFilter(BitSet bsetDst)
        {
            bsetDst.Union(bitsetFilter);
#if DEBUG
            fUnionCalled = true;
#endif
        }

        //------------------------------------------------------------
        // INFILESYM.NextInfileSym
        //
        /// <summary>
        /// Find the INFILESYM instance in the Sym list linked by NextSym field.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM NextInFileSym()
        {
            SYM sym;
            for (sym = NextSym; sym != null && !sym.IsINFILESYM; sym = sym.NextSym) ;
            return sym != null ? (sym as INFILESYM) : null;
        }

        //------------------------------------------------------------
        // INFILESYM.GetOutFileSym
        //
        /// <summary>
        /// If the parent sym is OUTFILESYM, return it,
        /// Or find OUTFILESYM instance in ancestors.
        /// </summary>
        /// <remarks>In sscli, getOutputFile().</remarks>
        /// <returns></returns>
        //------------------------------------------------------------
        internal OUTFILESYM GetOutFileSym()
        {
            if (ParentSym != null)
            {
                OUTFILESYM osym = ParentSym as OUTFILESYM;
                if (osym != null)
                {
                    return osym;
                }

                INFILESYM isym = ParentSym as INFILESYM;
                if (isym != null)
                {
                    return isym.GetOutFileSym();
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // INFILESYM.IsSymbolDefined
        //
        /// <summary>
        /// returns TRUE if a preprocessor symbol is defined
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsSymbolDefined(string symbol)
        {
            return SourceData.Module.IsSymbolDefined(symbol);
        }

        //------------------------------------------------------------
        // INFILESYM.CompareVersions
        //------------------------------------------------------------
        internal int CompareVersions(INFILESYM infile2)
        {
            if (infile2 == null)
            {
                return 1;
            }
            return this.AssemblyVersion.CompareTo(infile2.AssemblyVersion);
        }

#if DEBUG
        //------------------------------------------------------------
        // INFILESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class ModTypeInfo
    //
    /// <summary>
    /// A MODULESYM has an array of these - one for each unloaded type def in the module.
    /// They are sorted by (tokRoot, tok).
    /// tokRoot is outermost (just inside the namespace) type containing tok.
    /// </summary>
    //======================================================================
    internal class ModTypeInfo
    {
        internal NSSYM NsSym = null;

        internal int RootTypeToken = 0;

        internal int TypeToken = 0;

        static public bool operator <(ModTypeInfo mti1, ModTypeInfo mti2)
        {
            return (
                mti1.RootTypeToken < mti2.RootTypeToken ||
                mti1.RootTypeToken == mti2.RootTypeToken && mti1.TypeToken < mti2.TypeToken);
        }

        static public bool operator <=(ModTypeInfo mti1, ModTypeInfo mti2)
        {
            return (
                mti1.RootTypeToken < mti2.RootTypeToken ||
                mti1.RootTypeToken == mti2.RootTypeToken && mti1.TypeToken <= mti2.TypeToken
                );
        }

        static public bool operator >(ModTypeInfo mti1, ModTypeInfo mti2)
        {
            return (
                mti1.RootTypeToken > mti2.RootTypeToken ||
                mti1.RootTypeToken == mti2.RootTypeToken && mti1.TypeToken > mti2.TypeToken
                );
        }

        static public bool operator >=(ModTypeInfo mti1, ModTypeInfo mti2)
        {
            return (
                mti1.RootTypeToken > mti2.RootTypeToken ||
                mti1.RootTypeToken == mti2.RootTypeToken && mti1.TypeToken >= mti2.TypeToken
                );
        }
    }

    //======================================================================
    // class MODULESYM
    //
    /// <summary>
    /// <para>Represents an imported module. Parented by a metadata INFILESYM.
    /// Name is the module name. Parents MODOPTSYMs.</para>
    /// <para>Derives from PARENTSYM.</para>
    /// </summary>
    //======================================================================
    internal class MODULESYM : PARENTSYM
    {
        protected Module module = null;
        internal Module Module
        {
            get { return this.module; }
        }

        /// <summary>
        /// Assembly ID.
        /// </summary>
        private int assemblyID;

        /// <summary>
        /// Needed to communicate back to Alink.
        /// </summary>
        private int scopeIndex;
        /// <summary>
        /// (R) Index of scope.
        /// </summary>
        internal int Index
        {
            get { return scopeIndex; }
        }

        // IMetaDataAssemblyImport * assemimport;
        // IMetaDataImport2 * metaimportV2;

        // These are set and consumed by IMPORTER. The ModTypeInfo array contains
        // all type defs and their corresponding NSSYM (if it hasn't been loaded yet).
        // This is used to defer loading of types until the containing namespace is being
        // accessed for the first time.

        //internal int cmti;
        private List<ModTypeInfo> modTypeInfoList = new List<ModTypeInfo>();
        internal List<ModTypeInfo> ModTypeInfoList { get { return modTypeInfoList; } }
        internal int ModTypeInfoCount
        {
            get { return ModTypeInfoList.Count; }
        }

        //------------------------------------------------------------
        // MODULESYM.SetModule
        //------------------------------------------------------------
        internal void SetModule(Module mod)
        {
            this.module = mod;
            if (this.module == null) return;
        }

        //------------------------------------------------------------
        // MODULESYM.GetModuleID
        //
        /// <summary>
        /// Return assemblyID as a Module ID.
        /// </summary>
        //------------------------------------------------------------
        internal int GetModuleID()
        {
            DebugUtil.Assert(assemblyID >= Kaid.StartAssigning);
            return assemblyID;
        }

        //------------------------------------------------------------
        // MODULESYM.SetModuleID
        //
        /// <summary>
        /// Use assemblyID as a Module ID in sscli.
        /// </summary>
        //------------------------------------------------------------
        internal void SetModuleID(int id)
        {
            DebugUtil.Assert(this.assemblyID <= 0);
            assemblyID = id;
        }

        //------------------------------------------------------------
        // MODULESYM.getInputFile
        //
        /// <summary>
        /// Return ParentSym as INFILESYM.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal INFILESYM GetInputFile()
        {
            return (ParentSym as INFILESYM);
        }

        //------------------------------------------------------------
        // MODULESYM.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            this.module = null;
        }
        //__forceinline void MODULESYM::Clear()
        //{
        //    ASSERT(parent->isINFILESYM());
        //    if (this->metaimportV2) {
        //        this->metaimportV2->Release(); 
        //        this->metaimportV2 = NULL;
        //    }
        //    if (this->assemimport) {
        //        this->assemimport->Release(); 
        //        this->assemimport = NULL;
        //    }
        //}

        //------------------------------------------------------------
        // MODULESYM.Init
        //
        /// <summary></summary>
        /// <param name="iscope"></param>
        /// <param name="metaimport"></param>
        //------------------------------------------------------------
        internal void Init(int iscope, CAssemblyEx metaimport)
        {
            DebugUtil.Assert(ParentSym.IsINFILESYM);
            //DebugUtil.Assert(this.metaDataImport2 == null);
            //DebugUtil.Assert(this.MetaDataAssemblyImport == null);

            this.scopeIndex = iscope;
            //this.metaDataImport2 = metaimport;
            //this.metaDataAssemblyImport = null;
        }

        //------------------------------------------------------------
        // MODULESYM.GeMetaImport
        //------------------------------------------------------------
        //override internal AssemblyEx GetMetaImport(COMPILER compiler)
        //{
        //    DebugUtil.Assert(false, "MODULESYM.GeMetaImport");
        //    DebugUtil.Assert(ParentSym.IsINFILESYM);
        //    //DebugUtil.Assert(metaDataImport2 != null);
        //    //return metaDataImport2;
        //    return null;
        //}

        //------------------------------------------------------------
        // MODULESYM.GetMetaImportV2
        //------------------------------------------------------------
        //override internal AssemblyEx GetMetaImportV2(COMPILER compiler)
        //{
        //    DebugUtil.Assert(false, "MODULESYM.GetMetaImportV2");
        //    DebugUtil.Assert(ParentSym.IsINFILESYM);
        //    //DebugUtil.Assert(metaDataImport2 != null);
        //    //return metaDataImport2;
        //    return null;
        //}

        //------------------------------------------------------------
        // MODULESYM.GetAssemblyImport
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CAssemblyEx GetAssemblyImport(COMPILER compiler)
        {
            DebugUtil.Assert(false, "MODULESYM.GetAssemblyImport");
            DebugUtil.Assert(ParentSym.IsINFILESYM);

            //DebugUtil.Assert(metaDataAssemblyImport != null);
            //return metaDataAssemblyImport;
            return null;
        }

#if DEBUG
        //------------------------------------------------------------
        // MODULESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class SYNTHINFILESYM
    //
    /// <summary>
    /// <para>Same as above. Only used for #line directives</para>
    /// <para>INFILESYM の派生クラス。</para>
    /// </summary>
    //======================================================================
    internal class SYNTHINFILESYM : INFILESYM
    {
        internal ERRLOC ErrlocChecksum = null;

        internal Guid GuidChecksumID;

        private List<byte> checksumData=new List<byte>();
        internal List<byte> ChecksumData
        {
            get { return checksumData; }
        }
        internal int ChecksumDataCont { get { return checksumData.Count; } }

#if DEBUG
        //------------------------------------------------------------
        // SYNTHINFILESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class BAGSYM
    //
    /// <summary>
    /// <para>Base class for NSSYM and AGGSYM. Bags have DECLSYMs.
    /// Parent is another BAG. Children are other BAGs, members, type vars, etc.</para>
    /// <para>Drives from PARENTSYM. Has a list of DECLSYM.</para>
    /// </summary>
    //======================================================================
    internal class BAGSYM : PARENTSYM
    {
        //------------------------------------------------------------
        // BAGSYM Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// Top of the DECLSYM list.
        /// </summary>
        private DECLSYM firstDeclSym = null;

        /// <summary>
        /// (R) Top of the DECLSYM list.
        /// </summary>
        internal DECLSYM FirstDeclSym
        {
            get { return firstDeclSym; }    // DeclFirst()
        }

        /// <summary>
        /// Last DECLSYM of the list.
        /// </summary>
        private DECLSYM lastDeclSym = null;

        /// <summary>
        /// (R) Last DECLSYM of the list.
        /// </summary>
        internal DECLSYM LastDeclSym
        {
            get { return lastDeclSym; }
        }

        /// <summary>
        /// (R) Return ParentSym as BAGSYM.
        /// </summary>
        internal BAGSYM ParentBagSym
        {
            get { return (ParentSym as BAGSYM); }   // BagPar()
        }

        //------------------------------------------------------------
        // BAGSYM.AddDeclSym
        //
        /// <summary>
        /// <para>Add DECLSYM to the list.</para>
        /// <para>If decl is NSDECLSYM, update the bitset of its NSSYM.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void AddDeclSym(DECLSYM decl)
        {
            DebugUtil.Assert(decl != null && this != null);
            DebugUtil.Assert(this.IsNSSYM || this.IsAGGSYM);
            DebugUtil.Assert(decl.IsNSDECLSYM || decl.IsAGGDECLSYM);
            DebugUtil.Assert(!this.IsNSSYM == !decl.IsNSDECLSYM);

            // If parent is set it should be set to us!
            DebugUtil.Assert(decl.BagSym == null || decl.BagSym == this);
            // There shouldn't be a declNext.
            DebugUtil.Assert(decl.NextDeclSym == null);

            if (this.lastDeclSym == null)
            {
                DebugUtil.Assert(this.firstDeclSym == null);

                firstDeclSym = lastDeclSym = decl;
                decl.NextDeclSym = null;
            }
            else
            {
                lastDeclSym.NextDeclSym = decl;
                lastDeclSym = decl;
            }
            decl.BagSym = this;

            // If NSDECLSYM, set bitset.
            if (decl.IsNSDECLSYM)
            {
                (decl as NSDECLSYM).BagNsSym.DeclAdded(decl as NSDECLSYM);
            }
        }

        //------------------------------------------------------------
        // BAGSYM.InAlias
        //------------------------------------------------------------
        internal bool InAlias(int aid)
        {
            DebugUtil.Assert(this.IsAGGSYM || this.IsNSSYM);

            switch (this.Kind)
            {
                case SYMKIND.AGGSYM:
                    return (this as AGGSYM).InAlias(aid);

                case SYMKIND.NSSYM:
                    return (this as NSSYM).InAlias(aid);

                default:
                    DebugUtil.Assert(false);
                    return false;
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // BAGSYM Debug
        //------------------------------------------------------------
        internal void DebugBag(StringBuilder sb)
        {
            StringBuilder sbp = new StringBuilder();
            SYM sym = this.firstDeclSym;
            while (sym != null)
            {
                if (sbp.Length > 0) sbp.Append(", ");
                sbp.AppendFormat("{0}", sym.SymID);
                sym = sym.NextSym;
            }
            if (sbp.Length > 0)
            {
                sb.AppendFormat("Declaration SYM  : No.{0}\n", sbp.ToString());
            }
            else
            {
                sb.Append("Declaration SYM  : ---\n");
            }
            sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugBag(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class DECLSYM
    //
    /// <summary>
    /// <para>Base class for NSDECLSYM and AGGDECLSYM.
    /// Parent is another DECL. Children are DECLs.</para>
    /// <para>Registered to a DECLSYM list of BAGSYM.</para>
    /// <para>Derives from PARENTSYM.</para>
    /// </summary>
    //======================================================================
    internal class DECLSYM : PARENTSYM
    {
        //------------------------------------------------------------
        // DECLSYM Fields and Properties
        //------------------------------------------------------------
        /// <summary>
        /// Owner of the list to which this DECLSYM belongs.
        /// </summary>
        internal BAGSYM BagSym; // bag

        /// <summary>
        /// Next DECLSYM in the list of BAGSYM.
        /// </summary>
        internal DECLSYM NextDeclSym;   // declNext, DeclNext()

        internal DECLSYM ParentDeclSym
        {
            get { return (ParentSym as DECLSYM); }  // DeclPar()
        }

        internal bool IsFirst
        {
            get { return this == BagSym.FirstDeclSym; } // IsFirst()
        }

        //------------------------------------------------------------
        // DECLSYM.ContainingDeclaration
        //
        /// <summary>
        /// Hides SYM::containingDeclaration. Use DeclPar instead.
        /// </summary>
        //------------------------------------------------------------
        new internal void ContainingDeclaration() { }   // containingDeclaration()

        //------------------------------------------------------------
        // DECLSYM.GetNsDecl
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NSDECLSYM GetNsDecl()
        {
            for (DECLSYM decl = this; ; decl = decl.ParentDeclSym)
            {
                if (decl.IsNSDECLSYM)
                {
                    return decl as NSDECLSYM;
                }
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // DECLSYM Debug
        //------------------------------------------------------------
        internal void DebugDecl(StringBuilder sb)
        {
            sb.AppendFormat("BagSym : No.{0}\n", this.BagSym.SymID);
            sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugParent(sb);
            DebugDecl(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    // Namespaces, Namespace Declarations, and their members.
    // 
    // The parent, child, nextChild relationships are overloaded for namespaces.
    // The cause of all of this is that a namespace can be declared in multiple
    // places. This would not be a problem except that the using clauses(which
    // effect symbol lookup) are related to the namespace declaration not the
    // namespace itself. The result is that each namespace needs lists of all of
    // its declarations, and its members. Each namespace declaration needs a list
    // the declarations and types declared within it. Each member of a namespace
    // needs to access both the namespace it is contained in and the namespace
    // declaration it is contained in.

    //======================================================================
    // NSSYM
    //
    /// <summary>
    /// <para>a symbol representing a name space.
    /// parent is the containing namespace.</para>
    /// <para>Derives fromBAGSYM.</para>
    /// </summary>
    //======================================================================
    internal class NSSYM : BAGSYM
    {
        /// <summary>
        /// Does this namespace contain predefined classes?
        /// </summary>
        internal bool HasPredefineds;   // bool fHasPredefs : 1;

        /// <summary>
        /// Have we already checked children for CLS name clashes?
        /// </summary>
        internal bool CheckedForCLS = false;    // bool checkedForCLS : 1;

        /// <summary>
        /// Have we added this NSSYM to it's parent's list?
        /// </summary>
        internal bool CheckingForCLS;   // bool checkingForCLS : 1;

        /// <summary>
        /// Is this namespace (or rather one of it's NSDECLS) defined in source?
        /// </summary>
        internal bool IsDefinedInSource;    // bool isDefinedInSource : 1;

        /// <summary>
        /// <para>This is a cache of bsetNeedLoaded.TestAnyBits()</para>
        /// <para>True if there is a necessary assembly which have not been loaded yet.</para>
        /// </summary>
        private bool anyTypesUnloaded = false;  // bool fAnyTypesUnloaded : 1;

        private BitSet bitsetFilter = new BitSet(); // bsetFilter

        /// <summary>
        /// Bitset to which register necessary assemblies.
        /// </summary>
        private BitSet bitsetNeedLoaded = new BitSet(); // bsetNeedLoaded

        /// <summary>
        /// (R) Return ParentSym as NSSYM.
        /// </summary>
        internal NSSYM ParentNsSym
        {
            get { return ParentSym != null ? (ParentSym as NSSYM) : null; }
        }

        /// <summary>
        /// (R) Return ParentSym as NSSYM.
        /// </summary>
        new internal NSSYM ParentBagSym
        {
            get { return ParentNsSym; }
        }

        /// <summary>
        /// (R) Return the first NSDECLSYM instance of this.
        /// </summary>
        new internal NSDECLSYM FirstDeclSym
        {
            get
            {
                DECLSYM sym = base.FirstDeclSym;
                return (sym != null ? (sym as NSDECLSYM) : null);
            }
        }

        //------------------------------------------------------------
        // NSSYM.DeclAdded
        //
        /// <summary>
        /// Called by BAGSYM::AddDecl whenever an NSDECLSYM is added.
        /// This allows the NSSYM to update its alias bitset.
        /// </summary>
        //------------------------------------------------------------
        internal void DeclAdded(NSDECLSYM decl)
        {
            DebugUtil.Assert(decl != null && decl.BagSym == this);
            DebugUtil.Assert(this.LastDeclSym == decl);

            INFILESYM infile = decl.GetInputFile();
            if (infile == null) return;

            if (infile.IsSource)
            {
                bitsetFilter.SetBit(Kaid.Global);
                bitsetFilter.SetBit(Kaid.ThisAssembly);
            }
            else
            {
                infile.UnionAliasFilter(bitsetFilter);
            }
        }


        //------------------------------------------------------------
        // NSSYM.InAlias
        //
        /// <summary>
        /// Return the bit flag of bitsetFilter.
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        /// <remarks>
        /// In sscli, two InAlias methods are defined.
        ///     <code>internal bool InAlias(COMPILER comp, int aid)</code>
        ///     <code>internal bool InAlias(BSYMMGR bsymmgr, int aid)</code>
        /// But, their codes are same:
        ///<code>ASSERT(0 &lt;= aid);
        ///return bsetFilter.TestBit(aid);</code>
        /// and argument comp and argument bsymmgr are not used.
        /// </remarks>
        //------------------------------------------------------------
        new internal bool InAlias(int aid)
        {
            return bitsetFilter.TestBit(aid);
        }

        //------------------------------------------------------------
        // NSSYM.SetTypesUnloaded
        //
        /// <summary>
        /// Set the bit flag of bitsetNeedLoaded,
        /// and set areAnyTypesUnloaded true.
        /// </summary>
        /// <param name="aid"></param>
        //------------------------------------------------------------
        internal void SetTypesUnloaded(int aid)
        {
            DebugUtil.Assert(Kaid.ThisAssembly <= aid && aid < Kaid.MinModule);

            bitsetNeedLoaded.SetBit(aid);
            anyTypesUnloaded = true;
        }

        //------------------------------------------------------------
        // NSSYM.ClearTypesUnloaded
        //
        /// <summary>
        /// Clear the specified flags of bitsetNeedLoaded.
        /// </summary>
        /// <param name="aid"></param>
        //------------------------------------------------------------
        internal void ClearTypesUnloaded(int aid)
        {
            DebugUtil.Assert(Kaid.ThisAssembly <= aid && aid < Kaid.MinModule);

            bitsetNeedLoaded.ClearBit(aid);
            anyTypesUnloaded = bitsetNeedLoaded.TestAnyBits();
        }

        //------------------------------------------------------------
        // NSSYM.TypesUnloaded
        //
        /// <summary>
        /// Return the bit flag value of bitsetNeedLoaded,
        /// </summary>
        /// <param name="aid"></param>
        //------------------------------------------------------------
        internal bool TypesUnloaded(int aid)
        {
            DebugUtil.Assert(0 <= aid);
            return bitsetNeedLoaded.TestBit(aid);
        }

        //------------------------------------------------------------
        // NSSYM.AnyTypesUnloaded (1)
        //
        /// <summary>
        /// Determin if there is a bit flag
        /// that is set in both this.bitsetNeedLoaded and bsetCheck.
        /// </summary>
        /// <param name="bsetCheck"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AnyTypesUnloaded(BitSet bsetCheck)
        {
            DebugUtil.Assert(!anyTypesUnloaded == !bitsetNeedLoaded.TestAnyBits());
            return anyTypesUnloaded && bitsetNeedLoaded.TestAnyBits(bsetCheck);
        }

        //------------------------------------------------------------
        // NSSYM.AnyTypesUnloaded (2)
        //
        /// <summary>
        /// Return areAnyTypesUnloaded.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AnyTypesUnloaded()
        {
            DebugUtil.Assert(!anyTypesUnloaded == !bitsetNeedLoaded.TestAnyBits());
            return anyTypesUnloaded;
        }

        //------------------------------------------------------------
        // NSSYM.GetQualifiedName
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void GetQualifiedName(StringBuilder sb)
        {
            if (this.ParentNsSym == null || this.ParentNsSym == this)
            {
                return;
            }
            if (String.IsNullOrEmpty(this.Name))
            {
                return;
            }
            ParentNsSym.GetQualifiedName(sb);
            if (sb.Length > 0)
            {
                sb.Append('.');
            }
            sb.Append(this.Name);
        }

#if DEBUG
        //------------------------------------------------------------
        // NSSYM Debug
        //------------------------------------------------------------
        internal void DebugNs(StringBuilder sb)
        {
            //sb.AppendFormat("Has predefined classes : {0}\n", HasPredefineds);
            //sb.AppendFormat("Has checked CLS names  : {0}\n", CheckedForCLS);
            //sb.AppendFormat("Added this to parent   : {0}\n", CheckingForCLS);
            //sb.AppendFormat("Defined in source      : {0}\n", IsDefinedInSource);
            //sb.AppendFormat("Are any types loaded   : {0}\n", areAnyTypesUnloaded);
            //sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugBag(sb);
            DebugParent(sb);
            DebugNs(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // NSAIDSYM
    //
    /// <summary>
    /// <para>Parented by an NSSYM.
    /// Represents an NSSYM within an aid (assembly/alias id).
    /// The name is a form of the aid.</para>
    /// <para>Derives from PARENTSYM.</para>
    /// </summary>
    //======================================================================
    internal class NSAIDSYM : PARENTSYM
    {
        private int assemblyID; // aid

        /// <summary>
        /// Return the parent sym as NSSYM.
        /// </summary>
        internal NSSYM NamespaceSym // GetNS()
        {
            get { return ParentSym != null ? (ParentSym as NSSYM) : null; }
        }

        //------------------------------------------------------------
        // NSAIDSYM.GetAssemblyID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal int GetAssemblyID()    // GetAid()
        {
            return assemblyID;
        }

        //------------------------------------------------------------
        // NSAIDSYM.SetAssemblyID
        //
        /// <summary></summary>
        /// <param name="id"></param>
        //------------------------------------------------------------
        new internal void SetAssemblyID(int id)
        {
            assemblyID = id;
        }

#if DEBUG
        //------------------------------------------------------------
        // NSAIDSYM Debug
        //------------------------------------------------------------
        internal void DebugNsAid(StringBuilder sb)
        {
            sb.AppendFormat("Assembly ID : {0}\n", this.assemblyID);
            if (this.NamespaceSym != null)
            {
                sb.AppendFormat("Namespace   : No.{0} {1}\n", this.NamespaceSym.SymID, this.NamespaceSym.Name);
            }
            else
            {
                sb.Append("Namespace   :n");
            }
            sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugNsAid(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class NSDECLSYM
    //
    /// <summary>
    /// <para>NSDECLSYM - a symbol representing a declaration
    /// of a namspace in the source. </para>
    /// <para>firstChild/firstChild->nextChild enumerates the 
    /// NSDECLs and AGGDECLs declared within this declaration.</para>
    /// <para>parent is the containing namespace declaration.</para>
    /// <para>Bag() is the namespace corresponding to this declaration.</para>
    /// <para>DeclNext() is the next declaration for the same namespace.</para>
    /// <para>Derives from DECLSYM. (DECLSYM derives from PARENTSYM.)</para>
    /// </summary>
    //======================================================================
    internal class NSDECLSYM : DECLSYM
    {
        internal INFILESYM InFileSym = null;    // * inputfile

        /// <summary>
        /// A node tree created from codes included in the namespace declaration.
        /// </summary>
        internal NAMESPACENODE ParseTreeNode = null;    // * parseTree

        private List<SYM> usingClauseSymList = new List<SYM>(); // SYMLIST * usingClauses;
        internal List<SYM> UsingClauseSymList
        {
            get { return usingClauseSymList; }
        }

        internal METHSYM WithEmittedUsingMethodSym = null;  // * methodWithEmittedUsings

        internal bool IsDefined = false;    // bool isDefined : 1;

        internal bool UsingClausesResolved = false;  // bool usingClausesResolved:1;

        /// <summary>
        /// Return the parent sym as NSDECLSYM
        /// </summary>
        internal NSDECLSYM ParentNsDeclSym
        {
            get { return ParentSym as NSDECLSYM; }  // DeclPar()
        }

        /// <summary>
        /// return DECLSYM.BagSym as NSSYM.
        /// </summary>
        internal NSSYM NamespaceSym
        {
            get { return BagSym as NSSYM; } // NameSpace()
        }

        /// <summary>
        /// return DECLSYM.BagSym as NSSYM.
        /// </summary>
        internal NSSYM BagNsSym
        {
            get { return NamespaceSym; }    // Bag()
        }

        internal bool IsRootDeclaration
        {
            get { return (ParentSym == null); }
        }

        //------------------------------------------------------------
        // NSDECLSYM.NextDeclSym
        //------------------------------------------------------------
        new internal NSDECLSYM NextDeclSym
        {
            get // DeclNext()
            {
                return (base.NextDeclSym as NSDECLSYM);
            }
        }

        //------------------------------------------------------------
        // NSDECLSYM.GetNsDecl
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        new internal void GetNsDecl() { }
        
        //------------------------------------------------------------
        // NSDECLSYM.GetAssemblyID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal int GetAssemblyID()
        {
            return InFileSym != null ? InFileSym.GetAssemblyID() : -1;
        }

        //------------------------------------------------------------
        // NSDECLSYM.InternalsVisibleTo
        //
        /// <summary></summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool InternalsVisibleTo(int aid)
        {
            return InFileSym.InternalsVisibleTo(aid);
        }

        //------------------------------------------------------------
        // NSDECLSYM.IsDottedDeclaration
        //
        /// <summary>
        /// true for a&b in namespace a.b.c {}
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsDottedDeclaration()
        {
            return this.ParseTreeNode.NameNode != null &&
                   (this.ParseTreeNode.NameNode.Kind == NODEKIND.DOT) &&
                    this.FirstChildSym != null &&
                   (this.FirstChildSym.IsNSDECLSYM) &&
                   ((this.FirstChildSym as NSDECLSYM).ParseTreeNode == this.ParseTreeNode);
        }

#if DEBUG
        //------------------------------------------------------------
        // NSDECLSYM Debug
        //------------------------------------------------------------
        internal void DebugNsDecl(StringBuilder sb)
        {
            StringBuilder sbt = new StringBuilder();

            if (NamespaceSym != null)
            {
                sb.AppendFormat("Namespace                 : No.{0}\n", NamespaceSym.SymID);
            }
            else
            {
                sb.Append("Namespace                 : Rootnamespace\n");
            }

            if (this.InFileSym != null && this.InFileSym.FileInfo != null)
            {
                sb.AppendFormat("Source File               : {0}\n", this.InFileSym.FileInfo.Name);
            }
            else
            {
                sb.Append("Source file                     :\n");
            }
            if (this.ParseTreeNode != null)
            {
                sb.AppendFormat("Parse Tree                : No.{0}\n", this.ParseTreeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("Parse Tree                :\n");
            }

            sbt.Length = 0;
            for (int i = 0; i < this.usingClauseSymList.Count; ++i)
            {
                if (sbt.Length > 0) sbt.Append(", ");
                sbt.AppendFormat("{0}", usingClauseSymList[i].SymID);
            }
            if (sbt.Length > 0)
            {
                sb.AppendFormat("Using Clauses             : No.{0}\n", sbt.ToString());
                sb.AppendFormat("     Resolved             : {0}\n", this.UsingClausesResolved);
            }
            else
            {
                sb.Append("Using Clauses             :\n");
            }

            if (this.WithEmittedUsingMethodSym != null)
            {
                sb.AppendFormat("WithEmittedUsingMethodSym : No.{0}\n", this.WithEmittedUsingMethodSym.SymID);
            }
            else
            {
                sb.Append("WithEmittedUsingMethodSym :\n");
            }
            sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugNsDecl(sb);
            DebugParent(sb);
            DebugDecl(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // AGGDECLSYM
    //
    /// <summary>
    /// <para>represents a declaration of a aggregate type.
    /// With partial classes, an aggregate type might be declared in multiple places.
    /// This symbol represents on of the declarations.</para>
    /// <para>parent is the containing DECLSYM.</para>
    /// <para>Derives from DECLSYM. Represents aggregate types.</para>
    /// </summary>
    //======================================================================
    internal class AGGDECLSYM : DECLSYM
    {
        //------------------------------------------------------------
        // AGGDECLSYM Fields and Properties
        //------------------------------------------------------------
        internal BASENODE ParseTreeNode = null; // parseTree

        new internal bool IsUnsafe = false; // isUnsafe

        internal bool IsPartial = false;    // isPartial

        /// <summary>
        /// (R) Return DECLSYM.BasSym as AGGSYM.
        /// </summary>
        internal AGGSYM AggSym  // Agg()
        {
            get { return BagSym != null ? (BagSym as AGGSYM) : null; }
        }

        /// <summary>
        /// (R) Return DECLSYM.BasSym as AGGSYM.
        /// </summary>
        internal AGGSYM BagAggSym   // Bag()
        {
            get { return AggSym; }
        }

        new internal AGGDECLSYM NextDeclSym // DeclNext()
        {
            get
            {
                DECLSYM sym = base.NextDeclSym;
                return (sym != null ? (sym as AGGDECLSYM) : null);
            }
        }

        //------------------------------------------------------------
        // AGGDECLSYM.GetInputFile
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal INFILESYM GetInputFile()
        {
            NSDECLSYM sym = GetNsDecl();
            return sym != null ? sym.InFileSym : null;
        }

        //------------------------------------------------------------
        // AGGDECLSYM.GetAssemblyID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal int GetAssemblyID()
        {
            NSDECLSYM sym = GetNsDecl();
            return sym != null ? sym.GetAssemblyID() : -1;
        }

        //------------------------------------------------------------
        // AGGDECLSYM.InternalsVisibleTo
        //
        /// <summary></summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool InternalsVisibleTo(int aid)
        {
            NSDECLSYM sym = GetNsDecl();
            return sym != null ? sym.InternalsVisibleTo(aid) : false;
        }

        //------------------------------------------------------------
        // AGGDECLSYM.GetAttributesNode
        //
        /// <summary>
        /// returns the parse node for the type's attributes
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal BASENODE GetAttributesNode()
        {
            if (ParseTreeNode == null)
            {
                DebugUtil.Assert(this.AggSym.IsFabricated);
                return null;
            }
            return (
                ParseTreeNode.Kind == NODEKIND.DELEGATE ?
                (ParseTreeNode as DELEGATENODE).AttributesNode :
                ParseTreeNode.AsAGGREGATE.AttributesNode);
        }

#if DEBUG
        //------------------------------------------------------------
        // AGGDECLSYM Debug
        //------------------------------------------------------------
        internal void DebugAggDecl(StringBuilder sb)
        {
            if (this.AggSym != null)
            {
                sb.AppendFormat("AggSym     : No.{0}\n", this.AggSym.SymID);
            }
            else
            {
                sb.Append("AggSym     :\n");
            }

            if (this.ParseTreeNode != null)
            {
                sb.AppendFormat("Parse Tree : Node No.{0}\n", this.ParseTreeNode.NodeID);
            }
            else
            {
                sb.Append("Parse Tree :\n");
            }

            sb.AppendFormat("Unsafe     : {0}\n", this.IsUnsafe);
            sb.AppendFormat("Partial    : {0}\n", this.IsPartial);
            sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugAggDecl(sb);
            DebugParent(sb);
            DebugDecl(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // AGGSYM
    //
    /// <summary>
    /// <para>a symbol representing an aggregate type. These are classes,
    /// interfaces, and structs. Parent is a namespace or class. Children are methods,
    /// properties, and member variables, and types (including its own AGGTYPESYMs).</para>
    /// <para>Derives form BAGSYM.</para>
    /// </summary>
    //======================================================================
    internal class AGGSYM : BAGSYM
    {
        //------------------------------------------------------------
        // AGGSYM   Fields and Properties (1)
        //------------------------------------------------------------
        internal string deprecatedMessage = null;

        protected Type type = null;
        protected TypeBuilder typeBuilder = null;

        internal Type Type
        {
            get
            {
                return this.type;
            }
            set
            {
                if (value is TypeBuilder)
                {
                    this.type = value;
                    this.typeBuilder = value as TypeBuilder;
                }
                else
                {
                    this.type = value;
                    this.typeBuilder = null;
                }
            }
        }

        internal TypeBuilder TypeBuilder
        {
            get
            {
                return this.typeBuilder;
            }
            set
            {
                this.type = value;
                this.typeBuilder = value;
            }
        }

        protected bool typeCreated = false;

        internal bool TypeCreated
        {
            get
            {
                return (
                    (this.type != null && this.typeBuilder == null) ||
                    (this.typeBuilder != null && this.typeCreated == true));
            }
        }

        /// <summary>
        /// This INFILESYM is some infile for the assembly containing this AGGSYM.
        /// It is used for fast access to the filter BitSet and assembly ID.
        /// </summary>
        internal INFILESYM InFileSym = null;    // infile

        /// <summary>
        /// The instance type. Created when first needed.
        /// </summary>
        internal AGGTYPESYM InstanceAggTypeSym = null;  // atsInst

        //------------------------------------------------------------
        // AGGSYM   Fields and Properties (2) AggState and its properties
        //------------------------------------------------------------
        /// <summary>
        /// <para>Compiling state of AggSym.</para>
        /// </summary>
        /// <remarks>
        /// <para>(AggStateEnum AggState(), void SetAggState(AggStateEnum aggState))</para>
        /// </remarks>
        internal AggStateEnum AggState; // uint state : 8;

        /// <summary>
        /// True if AggState is ResolvingInheritance.
        /// </summary>
        internal bool IsResolvingBaseClasses    // IsResolvingBaseClasses()
        {
            get { return AggState == AggStateEnum.ResolvingInheritance; }
        }

        internal bool HasResolvedBaseClasses    // HasResolvedBaseClasses()
        {
            get { return AggState >= AggStateEnum.Inheritance; }
        }

        internal bool IsDefined // IsDefined()
        {
            get { return AggState >= AggStateEnum.DefinedMembers; }
        }
        internal bool IsPreparing   // IsPreparing()
        {
            get { return AggState == AggStateEnum.Preparing; }
        }
        internal bool IsPrepared    // IsPrepared()
        {
            get { return AggState >= AggStateEnum.Prepared; }
        }

        internal bool IsCompiled    // IsCompiled()
        {
            get { return AggState >= AggStateEnum.Compiled; }
        }

        //------------------------------------------------------------
        // AGGSYM   Fields and Properties (3) AggKind and its properties
        //------------------------------------------------------------

        /// <summary></summary>
        /// <remarks>
        /// <para>(AggKindEnum AggKind(), void SetAggKind(AggKindEnum aggKind))</para>
        /// </remarks>
        internal AggKindEnum AggKind;   // uint aggKind : 8;

        internal bool IsClass   // IsClass()
        {
            get { return AggKind == AggKindEnum.Class; }
        }

        internal bool IsDelegate    // IsDelegate()
        {
            get { return AggKind == AggKindEnum.Delegate; }
        }

        internal bool IsInterface   // IsInterface()
        {
            get { return AggKind == AggKindEnum.Interface; }
        }

        internal bool IsStruct  // IsStruct()
        {
            get { return AggKind == AggKindEnum.Struct; }
        }

        internal bool IsEnum    // IsEnum()
        {
            get { return AggKind == AggKindEnum.Enum; }
        }

        /// <summary>
        /// struct, enum
        /// </summary>
        internal bool IsValueType   // IsValueType()
        {
            get { return (AggKind == AggKindEnum.Struct || AggKind == AggKindEnum.Enum); }
        }

        /// <summary>
        /// class, interface, delegate
        /// </summary>
        internal bool IsReferenceType   // IsRefType()
        {
            get
            {
                return (
                    AggKind == AggKindEnum.Class ||
                    AggKind == AggKindEnum.Interface ||
                    AggKind == AggKindEnum.Delegate);
            }
        }

        //------------------------------------------------------------
        // AGGSYM   Fields and Properties (4) Predefined Type
        //------------------------------------------------------------
        /// <summary>
        /// A special predefined type.
        /// </summary>
        internal bool IsPredefinedType = false; // uint isPredefined: 1;

        /// <summary>
        /// index of the predefined type, if isPredefined.
        /// </summary>
        internal PREDEFTYPE PredefinedTypeID = 0;   // uint iPredef: 7;

        //------------------------------------------------------------
        // AGGSYM   Fields and Properties (5)
        //------------------------------------------------------------
        // 各種の状態を示すフィールドと属性

        /// <summary>
        /// has type defs been emitted?
        /// </summary>
        internal bool IsTypeDefEmitted = false; // uint isTypeDefEmitted: 1;

        /// <summary>
        /// has the bases for the type def been emitted?
        /// </summary>
        internal bool IsBasesEmitted = false;   // uint isBasesEmitted: 1;

        /// <summary>
        /// have all member defs been emitted?
        /// </summary>
        internal bool IsMemberDefsEmitted = false;  // uint isMemberDefsEmitted: 1;

        /// <summary>
        /// Set if we've previously checked the instance type of this struct for cycles.
        /// </summary>
        internal bool LayoutChecked = false;    // uint fLayoutChecked: 1;

        internal bool LayoutErrorOccurred = false;  // uint fLayoutError: 1;

        /// <summary>
        /// This class is defined in source,
        /// although the source might not be being read during this compile.
        /// </summary>
        internal bool IsSource = false; // uint isSource: 1;

        /// <summary>
        /// Do one or more parse trees exist for this class?  
        /// </summary>
        internal bool HasParseTree = false; // uint hasParseTree: 1;

        /// <summary>
        /// Can it be instantiated?
        /// </summary>
        internal bool IsAbstract = false;   // uint isAbstract: 1;

        /// <summary>
        /// Can it be derived from?
        /// </summary>
        internal bool IsSealed = false; // uint isSealed: 1;

        /// <summary>
        /// App-domain bound or context bound?
        /// </summary>
        internal bool IsMarshalByRef = false;   // uint isMarshalByRef: 1;

        /// <summary>
        /// If the AGGSYM has non-zero arity and this is set,
        /// the arity is encoded in the metadata name (eg !2)
        /// </summary>
        internal bool IsArityInName = false;    // uint isArityInName: 1;

        /// <summary>
        /// is this a class which derives from System.Attribute
        /// </summary>
        internal bool IsAttribute = false;  // uint isAttribute: 1;

        /// <summary>
        /// is this a class which derives from System.Security.CodeAccessPermission
        /// </summary>
        internal bool IsSecurityAttribute = false;  // uint isSecurityAttribute: 1;

        /// <summary>
        /// set if this class is an attribute class 
        /// which can be applied multiple times to a single symbol
        /// </summary>
        internal bool IsMultipleAttribute = false;  // uint isMultipleAttribute: 1;

        /// <summary>
        /// set if this type or any base type has user defined conversion operators
        /// </summary>
        internal bool HasConversion = false;    // uint hasConversion: 1;

        /// <summary>
        /// Set if the struct is known to be un-managed (for unsafe code). Set in FUNCBREC.
        /// </summary>
        internal bool IsUnmanagedStruct = false;    // uint isUnmanagedStruct: 1;

        /// <summary>
        /// Set if the struct is known to be managed (for unsafe code). Set during import.
        /// </summary>
        internal bool IsManagedStruct = false;  // uint isManagedStruct: 1;

        /// <summary>
        /// Set if the type is known to be inaccessible to this assembly.
        /// </summary>
        internal bool IsPrivateMetadata = false;    // uint isMDPrivate: 1;

        /// <summary>
        /// Set if the type is a fixed size buffer struct
        /// </summary>
        internal bool IsFixedBufferStruct = false;  // uint isFixedBufferStruct: 1;

        /// <summary>
        /// Whether it has an instance constructor taking no args
        /// </summary>
        internal bool HasNoArgCtor = false; // uint hasNoArgCtor: 1;

        /// <summary>
        /// Whether it has a public instance constructor taking no args
        /// </summary>
        internal bool HasPubNoArgCtor = false;  // uint hasPubNoArgCtor: 1;

        /// <summary>
        /// Class has a user-defined static constructor
        /// </summary>
        internal bool HasUserDefinedStaticCtor = false; // uint hasUDStaticCtor: 1;

        /// <summary>
        /// class has explicit impls (used only on structs)
        /// </summary>
        internal bool HasExplicitImpl = false;  // uint hasExplicitImpl: 1;

        /// <summary>
        /// private struct members should not be checked for assignment or refencees
        /// </summary>
        internal bool HasExternReference = false;   // uint hasExternReference: 1;

        /// <summary>
        /// below bits are valid
        /// </summary>
        internal bool IsSelfCmpValid = false;   // uint fSelfCmpValid: 1;

        /// <summary>
        /// has operator == defined on itself
        /// </summary>
        internal bool HasSelfEquality = false;  // uint fHasSelfEq: 1;

        /// <summary>
        /// has operator != defined on itself
        /// </summary>
        internal bool HasSelfNonEquality = false;   // uint fHasSelfNe: 1;

        /// <summary>
        /// Never check for user defined operators on this type (eg, decimal, string, delegate).
        /// </summary>
        internal bool SkipUserDefinedOperators; // uint fSkipUDOps: 1;

        /// <summary>
        /// Does it have [ComImport]
        /// </summary>
        internal bool IsComImport = false;  // uint isComImport: 1;

        /// <summary>
        /// iface which is implemented by a method impl
        /// </summary>
        internal bool HasMethodImpl = false;    // uint hasMethodImpl: 1;

        /// <summary>
        /// has a security attribute of type link demand
        /// </summary>
        internal bool HasLinkDemand = false;    // uint hasLinkDemand: 1;

        /// <summary>
        /// true iff conditionalSymbols already includes symbols from base aggregates
        /// </summary>
        internal bool CheckedConditionalSymbols = false;    // uint fCheckedCondSymbols: 1;

        /// <summary>
        /// <para>The type variables for this generic class, as declarations.</para>
        /// <para>If not generic type, set null.</para>
        /// </summary>
        internal TypeArray TypeVariables = null;    // typeVarsThis

        /// <summary>
        /// <para>The type variables for this generic class and all containing classes.</para>
        /// <para>If not generic type, set null.</para>
        /// </summary>
        internal TypeArray AllTypeVariables = null; // typeVarsAll

        /// <summary>
        /// The emitted tokens for all the type variables
        /// </summary>
        internal int TypeVariablesEmittedTokens = 0;    // mdToken * toksEmitTypeVars;

        /// <summary>
        /// <para>Use in place of toksEmitTypeVars in sscli.</para>
        /// </summary>
        //internal GenericTypeParameterBuilder[] TypeParameterBuilders = null;
        internal Type[] GenericParameterTypes = null;

        /// <summary>
        /// For a class/struct/enum, the base class. For iface: unused.
        /// </summary>
        internal AGGTYPESYM BaseClassSym;   // baseClass

        /// <summary>
        /// <para>For enum, the underlying type.</para>
        /// <para>For iface, the resolved CoClass.</para>
        /// <para>Not used for class/struct.</para>
        /// </summary>
        internal AGGTYPESYM UnderlyingTypeSym = null;   // underlyingType

        /// <summary>
        /// The explicit base interfaces for a class or interface.
        /// </summary>
        internal TypeArray Interfaces = null;   // ifaces

        internal int InterfaceCount
        {
            get { return Interfaces != null ? Interfaces.Count : 0; }
        }

        /// <summary>
        /// Recursive closure of base interfaces
        /// ordered so an iface appears before all of its base ifaces.
        /// </summary>
        internal TypeArray AllInterfaces = null;    // ifacesAll

        internal int InterfaceCountAll
        {
            get { return AllInterfaces != null ? AllInterfaces.Count : 0; }
        }

        /// <summary>
        /// list of symbols which attributes are conditional on.
        /// </summary>
        internal List<string> ConditionalSymbolNameList;   // NAMELIST * conditionalSymbols;

        /// <summary>
        /// This is so AGGTYPESYMs can instantiate their baseClass and
        /// ifacesAll members on demand.
        /// </summary>
        internal BSYMMGR SymbolManager; // symmgr

        /// <summary>
        /// <para>Meta-data token for imported class.</para>
        /// <para>We can get information from this.Type.</para>
        /// </summary>
        internal int ImportedClassToken // mdToken tokenImport;
        {
            get { return (this.Type != null ? this.Type.MetadataToken : 0); }
        }

        /// <summary>
        /// <para>(sscli) MODULESYM * module: Meta-data module imported from</para>
        /// <para>Use INFILESYM for module.</para>
        /// </summary>
        //internal MODULESYM ModuleSym; // module

        /// <summary>
        /// The ComType token used for nested classes
        /// </summary>
        internal int ComTypeToken = 0;  // mdExportedType tokenComType;

        /// <summary>
        /// Meta-data token (typeRef or typeDef) in the current output file.
        /// </summary>
        internal int EmittedTypeToken = 0;  // mdToken tokenEmit;

        /// <summary>
        /// If tokenEmit is a typeDef, this is the corresponding typeRef.
        /// </summary>
        internal int TypeRefEmittedToken = 0;   // mdToken tokenEmitRef;

        /// <summary>
        /// For abstract classes the list of all unoverriden abstract methods (inherited and new).
        /// These must be in derived to base order.
        /// </summary>
        internal List<SYM> AbstractMethodSymList = null;   // PSYMLIST abstractMethods;

        /// <summary>
        /// First UD conversion operator. This chain is for this type only (not base types).
        /// The hasConversion flag indicates whether this or any base types have UD conversions.
        /// </summary>
        internal METHSYM FirstConversionMethSym;  // convFirst

        /// <summary>
        /// symbol type this type can be an attribute on. 
        ///     0 == not an attribute class
        ///    -1 == unknown (not imported)
        /// </summary>
        internal AttributeTargets AttributeClass;   // CorAttributeTargets attributeClass;

        /// <summary>
        /// If IsInterface() and class has ComImport and CoClass attributes,
        /// this is the unresolved CoClass string
        /// </summary>
        internal string ComImportCoClass;   // comImportCoClass

        /// <summary>
        /// (CS3) This type represents a direct product of parameters.
        /// </summary>
        internal bool IsParameterProcuct = false;

        //------------------------------------------------------------
        // AGGSYM   Properties
        //------------------------------------------------------------
        internal BAGSYM BagSym  // Bag()
        {
            get { return (ParentSym as BAGSYM); }
        }

        new internal BAGSYM ParentBagSym    // Parent()
        {
            get { return (ParentSym as BAGSYM); }
        }

        internal bool IsUnresolved  // IsUnresolved()
        {
            get { return (InFileSym != null) && InFileSym.GetAssemblyID() == (int)Kaid.Unresolved; }
        }

        /// <summary>
        /// abstrace and sealed
        /// </summary>
        internal bool IsStatic  // IsStatic()
        {
            get { return (IsAbstract && IsSealed); }
        }

        /// <summary>
        /// A AGGSYM represents a nested class
        /// if its parent is a AGGSYM or SYM derived from AGGSYM.
        /// </summary>
        internal bool IsNested  // isNested()
        {
            get { return (ParentSym != null && ParentSym.IsAGGSYM); }
        }

        new internal AGGDECLSYM FirstDeclSym    // DeclFirst()
        {
            get
            {
                DECLSYM sym = base.FirstDeclSym;
                return (sym as AGGDECLSYM);
            }
        }

        //------------------------------------------------------------
        // AGGSYM.SetType
        //------------------------------------------------------------
#if false
        internal void SetType(Type tp,INFILESYM inFileSym)
        {
            if (tp == null) return;
            this.type = tp;

            if (this.type.IsEnum)
            {
                this.AggKind = AggKindEnum.Enum;
            }
            else if (this.type.IsSubclassOf(Util.typeOfDelegate))
            {
                this.AggKind = AggKindEnum.Delegate;
            }
            else if (this.type.IsInterface)
            {
                this.AggKind = AggKindEnum.Interface;
            }
            else if (this.type.IsClass)
            {
                this.AggKind = AggKindEnum.Class;
            }
            else if (this.type.IsValueType)
            {
                this.AggKind = AggKindEnum.Struct;
            }
            else
            {
                this.AggKind = AggKindEnum.Unknown;
            }

            this.IsTypeDefEmitted = true;
            this.IsBasesEmitted = true;
            this.IsMemberDefsEmitted = true;

            this.IsAbstract = this.type.IsAbstract;
            this.IsSealed = this.type.IsSealed;
            this.IsMarshalByRef = this.type.IsMarshalByRef;
            this.IsArityInName = false;
            this.IsAttribute = this.type.IsSubclassOf(Util.typeOfAttribute);
            this.IsSecurityAttribute = this.type.IsSubclassOf(Util.typeofSecurityCodeAccessPermission);
            if (this.IsAttribute)
            {
                this.IsMultipleAttribute = Util.IsAllowMultiple(this.type);
            }
            this.HasConversion = false;
            this.IsUnmanagedStruct = false;
            this.IsManagedStruct = true;
            this.IsPrivateMetadata = false;
            this.IsFixedBufferStruct = false;

            ConstructorInfo ci = this.type.GetConstructor(Type.EmptyTypes);
            this.HasPubNoArgCtor = (ci != null ? (ci.IsPublic && !ci.IsStatic) : false);
            this.HasUserDefinedStaticCtor = false;
            this.HasExplicitImpl = false;
            this.HasExternReference = false;

            MethodInfo mi;
            try
            {
                this.HasSelfEquality = (mi = this.type.GetMethod("op_Equality")) != null;
            }
            catch (AmbiguousMatchException)
            {
                this.HasSelfEquality = true;
            }
            try
            {
                this.HasSelfNonEquality = (mi = this.type.GetMethod("op_Inequality")) != null;
            }
            catch (AmbiguousMatchException)
            {
                this.HasSelfNonEquality = true;
            }

            this.SkipUserDefinedOperators = false;
            this.IsComImport = false;
            this.HasMethodImpl = false;
            this.HasLinkDemand = false;
            this.AreConditionalSymbolsChecked = false;
            if (this.type.IsGenericType)
            {
                int arity = Util.ComputeGenericArityFromName(this.type.Name);
                Type[] argsAll = this.type.GetGenericArguments();
                int arityAll = argsAll.Length;
                DebugUtil.Assert(arityAll >= arity);
                if (arity > 0)
                {
                    this.TypeVariables = new TypeArray();
                    this.TypeVariables.Add(argsAll, argsAll.Length - arity);
                }
                else
                {
                    this.TypeVariables = null;
                }
                this.AllTypeVariables = new TypeArray();
                this.AllTypeVariables.Add(argsAll);
            }
            else
            {
                this.TypeVariables = null;
                this.AllTypeVariables = null;
            }
            //this.TypeVariablesEmittedTokens
            this.BaseClassSym = null;
            this.UnderlyingTypeSym = null;

            this.BaseInterfacesArray = null;

            Type[] ifaces = this.Type.GetInterfaces();
            if (ifaces.Length > 0)
            {
                this.AllInterfacesArray = new TypeArray();
                this.AllInterfacesArray.Add(ifaces);
            }
            else
            {
                this.AllInterfacesArray = null;
            }

            //this.ImportedToken = this.type.MetadataToken;
            this.InFileSym = inFileSym;
            //this.ComTypeToken
            //this.EmittedToken
            //this.TypeRefEmittedToken

            this.AbstractMethodSymList = null;
            this.FirstConversionMethodSym = null;

            //this.AttributeClass
            //this.ComImportCoClass
        }
#endif
        //------------------------------------------------------------
        // AGGSYM.DeclOnly
        //
        /// <summary>
        /// If this has only one DECLSYM instance, return it.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGDECLSYM DeclOnly()
        {
            DebugUtil.Assert(!HasParseTree || IsDelegate || IsEnum || IsFabricated);
            DebugUtil.Assert(FirstDeclSym != null && FirstDeclSym.NextDeclSym == null);

            return FirstDeclSym;
        }

        //------------------------------------------------------------
        // AGGSYM.InitFromOuterDecl
        //
        /// <summary></summary>
        /// <param name="declOuter"></param>
        //------------------------------------------------------------
        internal void InitFromOuterDecl(DECLSYM declOuter)
        {
            InFileSym = declOuter.GetInputFile();
            IsSource = InFileSym.IsSource;
        }

        //------------------------------------------------------------
        // AGGSYM.InAlias
        //
        /// <summary></summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool InAlias(int aid)
        {
            DebugUtil.Assert(InFileSym != null);
            DebugUtil.Assert(
                FirstDeclSym != null ||
                FirstDeclSym.GetAssemblyID() == InFileSym.GetAssemblyID());
            DebugUtil.Assert(0 <= aid);

            if (aid < Kaid.MinModule)
            {
                return InFileSym.InAlias(aid);
            }
            //return (aid == GetModuleID());
            return (aid == GetAssemblyID());
        }

        //------------------------------------------------------------
        // AGGSYM.GetModuleID
        //------------------------------------------------------------
        //internal int GetModuleID()
        //{
        //    return (IsSource ? GetOutputFile().GetModuleID() : ModuleSym.GetModuleID());
        //}

        //------------------------------------------------------------
        // AGGSYM.GetAssemblyID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal int GetAssemblyID()
        {
            DebugUtil.Assert(InFileSym != null);
            DebugUtil.Assert(FirstDeclSym == null || FirstDeclSym.GetAssemblyID() == InFileSym.GetAssemblyID());

            return InFileSym.GetAssemblyID();
        }

        //------------------------------------------------------------
        // AGGSYM.InternalsVisibleTo
        //
        /// <summary></summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool InternalsVisibleTo(int aid)
        {
            DebugUtil.Assert(InFileSym != null);
            DebugUtil.Assert(FirstDeclSym == null || FirstDeclSym.GetAssemblyID() == InFileSym.GetAssemblyID());

            return InFileSym.InternalsVisibleTo(aid);
        }

        //------------------------------------------------------------
        // AGGSYM.AsUnresolved
        //
        /// <summary>Return this as UNRESAGGSYM.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal UNRESAGGSYM AsUnresolved()
        {
            DebugUtil.Assert(InFileSym != null && InFileSym.GetAssemblyID() == Kaid.Unresolved);
            return this as UNRESAGGSYM;
        }

        //------------------------------------------------------------
        // AGGSYM.IsCLRAmbigStruct
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsCLRAmbigStruct()
        {
            if (!this.IsPredefinedType)
            {
                return this.IsEnum;
            }

            switch (GetThisType().GetPredefType())
            {
                case PREDEFTYPE.BYTE:
                case PREDEFTYPE.SHORT:
                case PREDEFTYPE.INT:
                case PREDEFTYPE.LONG:
                case PREDEFTYPE.CHAR:
                case PREDEFTYPE.BOOL:
                case PREDEFTYPE.SBYTE:
                case PREDEFTYPE.USHORT:
                case PREDEFTYPE.UINT:
                case PREDEFTYPE.ULONG:
                case PREDEFTYPE.INTPTR:
                case PREDEFTYPE.UINTPTR:
                case PREDEFTYPE.FLOAT:
                case PREDEFTYPE.DOUBLE:
                case PREDEFTYPE.TYPEHANDLE:
                case PREDEFTYPE.FIELDHANDLE:
                case PREDEFTYPE.METHODHANDLE:
                case PREDEFTYPE.ARGUMENTHANDLE:
                    return true;
                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // AGGSYM.GetBaseAgg
        //
        /// <summary>Return this.BaseClassSym as AGGSYM.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetBaseAgg()
        {
            return BaseClassSym == null ? null : BaseClassSym.GetAggregate();
        }

        //------------------------------------------------------------
        // AGGSYM.FindBaseAgg
        //
        /// <summary>
        /// Return true if argument agg is one of the base aggs of this.
        /// </summary>
        /// <param name="agg"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindBaseAgg(AGGSYM agg)
        {
            for (AGGSYM aggT = this; aggT != null; aggT = aggT.GetBaseAgg())
            {
                if (aggT == agg)
                    return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // AGGSYM.IsNested
        //------------------------------------------------------------
        //internal bool IsNested { get { return (ParentSym != null && ParentSym.IsAGGSYM); } }

        //------------------------------------------------------------
        // AGGSYM.GetOuterAgg
        //
        /// <summary>
        /// Return tha parent sym as AGGSYM (may be null).
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetOuterAgg()
        {
            return (ParentSym != null && ParentSym.IsAGGSYM ? (ParentSym as AGGSYM) : null);
        }

        //------------------------------------------------------------
        // AGGSYM.AllowableMemberAccess
        //
        /// <summary>
        /// returns the allowable access modifiers on members of this aggregate symbol
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NODEFLAGS AllowableMemberAccess()
        {
            switch (this.AggKind)
            {
                case AggKindEnum.Class:
                    return NODEFLAGS.MOD_ACCESSMODIFIERS;
                case AggKindEnum.Struct:
                    return NODEFLAGS.MOD_ACCESSMODIFIERS;
                case AggKindEnum.Interface:
                    return 0;
                default:
                    DebugUtil.Assert(false);
                    return 0;
            }
        }

        //------------------------------------------------------------
        // AGGSYM.GetModule
        //------------------------------------------------------------
        //new internal MODULESYM GetModule() { return this.module; }

        //------------------------------------------------------------
        // AGGSYM.GetMetaImportV2
        //------------------------------------------------------------
        //new internal AssemblyEx GetMetaImportV2(COMPILER compiler)
        //{
        //    return this.GetModule().GetMetaImportV2(compiler);
        //}

        //------------------------------------------------------------
        // AGGSYM.GetMetaImport
        //------------------------------------------------------------
        //new internal AssemblyEx GetMetaImport(COMPILER compiler)
        //{
        //    return this.GetModule().GetMetaImport(compiler);
        //}

        //------------------------------------------------------------
        // AGGSYM.GetElementKind
        //
        /// <summary>
        /// Given a symbol, returns its element kind for attribute purpose
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal AttributeTargets GetElementKind()
        {
            switch (AggKind)
            {
                case AggKindEnum.Interface:
                    return AttributeTargets.Interface;
                case AggKindEnum.Enum:
                    return AttributeTargets.Enum;
                case AggKindEnum.Class:
                    return AttributeTargets.Class;
                case AggKindEnum.Struct:
                    return AttributeTargets.Struct;
                case AggKindEnum.Delegate:
                    return AttributeTargets.Delegate;
                default:
                    //ASSERT(!"Bad aggsym");
                    break;
            }
            return (AttributeTargets)0;
        }

        //------------------------------------------------------------
        // AGGSYM.GetThisType
        //
        /// <summary>
        /// Create the AGGTYPESYM instance by substituting its type variables
        /// as the type arguments.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetThisType()
        {
            if (this.InstanceAggTypeSym == null)
            {
                DebugUtil.Assert(this.TypeVariables == this.AllTypeVariables || this.IsNested);

                this.InstanceAggTypeSym = SymbolManager.GetInstAgg(
                    this,
                    IsNested ? GetOuterAgg().GetThisType() : null,
                    TypeVariables,
                    AllTypeVariables);

                this.InstanceAggTypeSym.SetSystemType(this.type, false);
            }
            DebugUtil.Assert(this.TypeVariables.Count == this.InstanceAggTypeSym.TypeArguments.Count);
            return this.InstanceAggTypeSym;
        }

        //------------------------------------------------------------
        // AGGSYM.IsPredefAgg
        //
        /// <summary>Return true if this is of type of PREDEFTYPE pt.</summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsPredefAgg(PREDEFTYPE pt)
        {
            return this.IsPredefinedType && (PREDEFTYPE)this.PredefinedTypeID == pt;
        }

        new internal void GetParseTree() { }
        new internal void GetInputFile() { }
        new internal void ContainingDeclaration() { }

        //------------------------------------------------------------
        // AGGSYM.GetOutputFile
        //
        /// <summary>
        /// returns the output file for this aggregate
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal OUTFILESYM GetOutputFile()
        {
            // This is safe, since all partial parts of a class must be in the same output file.
            return FirstDeclSym.GetInputFile().GetOutFileSym();
        }

        //------------------------------------------------------------
        // AGGSYM.SetCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetCustomAttribute(CustomAttributeBuilder caBuilder, out Exception excp)
        {
            excp = null;

            if (this.TypeBuilder != null && caBuilder != null)
            {
                try
                {
                    this.TypeBuilder.SetCustomAttribute(caBuilder);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    excp = ex;
                }
                catch (InvalidOperationException ex)
                {
                    excp = ex;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // AGGSYM.CreateType
        //
        /// <summary>
        /// Call TypeBuilder.CreateType().
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CreateType(out Exception excp)
        {
            excp = null;

            if (this.TypeCreated)
            {
                return true;
            }
            if (this.Type == null)
            {
                return false;
            }
            if (typeBuilder == null)
            {
                return true;
            }

            Type newType = null;
            try
            {
                newType = this.typeBuilder.CreateType();
                if (newType != null)
                {
                    this.type = newType;
                    this.typeCreated = true;
                    return true;
                }
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }

            this.typeCreated = false;
            if (excp == null)
            {
                excp = new Exception("Cannot create the System.Type of " + this.Name);
            }
            return false;
        }

        //internal void AsTYPESYM; void IsTYPESYM;
        //internal void AsTYVARSYM; void IsTYVARSYM;
        //internal void AsARRAYSYM; void IsARRAYSYM;
        //internal void AsPTRSYM; void IsPTRSYM;
        //internal void AsPINNEDSYM; void IsPINNEDSYM;
        //internal void AsPARAMMODSYM; void IsPARAMMODSYM;
        //internal void AsVOIDSYM; void IsVOIDSYM;
        //internal void AsNULLSYM; void IsNULLSYM;
        //internal void AsERRORSYM; void IsERRORSYM;
        //internal void AsAGGTYPESYM; void IsAGGTYPESYM;

#if DEBUG

#if false
        internal override string DebugName
        {
            get
            {
                if (this.Type != null)
                {
                    if (this.Name != null)
                    {
                        return String.Format("{0} {1}", this.Name, this.Type.Name);
                    }
                    return this.Type.Name;
                }
                return base.DebugName;
            }
        }
#endif
        //------------------------------------------------------------
        // AGGSYM Debug
        //------------------------------------------------------------
        internal void DebugAgg(StringBuilder sb)
        {
            StringBuilder sbt = new StringBuilder();

            sb.AppendFormat("Kind                        : {0}\n", this.AggKind);
            if (this.Type != null)
            {
                sb.AppendFormat("Type                        : {0}\n", this.Type.FullName);
            }
            else
            {
                sb.Append("Type                        :\n");
            }
            if (this.BaseClassSym != null)
            {
                sb.AppendFormat("Base Type                   : No.{0} {1}\n", BaseClassSym.SymID, BaseClassSym.Name);
            }
            else
            {
                sb.Append("Base Type                   :\n");
            }
            if (this.UnderlyingTypeSym != null)
            {
                sb.AppendFormat("Underlying Type             : No.{0} {1}\n", UnderlyingTypeSym.SymID, UnderlyingTypeSym.Name);
            }
            else
            {
                sb.Append("Underlying Type             :\n");
            }

            if (this.Interfaces != null && this.Interfaces.Count > 0)
            {
                sbt.Length = 0;
                this.Interfaces.DebugSymID(sbt);
                sb.AppendFormat("Interfaces                  : No.{0}\n", sbt.ToString());
            }
            else
            {
                sb.Append("Interfaces                  :\n");
            }
            if (this.AllInterfaces != null && this.AllInterfaces.Count > 0)
            {
                sbt.Length = 0;
                this.AllInterfaces.DebugSymID(sbt);
                sb.AppendFormat("All Interfaces              : No.{0}\n", sbt.ToString());
            }
            else
            {
                sb.Append("All Interfaces              :\n");
            }

            if (this.TypeVariables != null && this.TypeVariables.Count > 0)
            {
                sbt.Length = 0;
                this.TypeVariables.DebugGeneric(sbt);
                sb.AppendFormat("Type Variables              : {0}\n", sbt.ToString());
            }
            else
            {
                sb.Append("Type Variables              :\n");
            }
            if (this.AllTypeVariables != null && this.AllTypeVariables.Count > 0)
            {
                sbt.Length = 0;
                this.AllTypeVariables.DebugGeneric(sbt);
                sb.AppendFormat("All Type Variables          : {0}\n", sbt.ToString());
            }
            else
            {
                sb.Append("All Type Variables          :\n");
            }

            sb.AppendFormat("Predefined Type             : {0}\n", this.IsPredefinedType);
            sb.AppendFormat("State                       : {0}\n", this.AggState);
            //sb.AppendFormat("Is Source                   : {0}\n", this.IsSource);
            //sb.AppendFormat("Has Parse Tree              : {0}\n", this.HasParseTree);
            //sb.AppendFormat("Abstract                    : {0}\n", this.IsAbstract);
            //sb.AppendFormat("Sealed                      : {0}\n", this.IsSealed);
            //sb.AppendFormat("Is Attribute                : {0}\n", this.IsAttribute);
            //sb.AppendFormat("Is Security Attribute       : {0}\n", this.IsSecurityAttribute);
            //sb.AppendFormat("Is Multiple Attribute       : {0}\n", this.IsMultipleAttribute);
            //sb.AppendFormat("Has UD Conversion           : {0}\n", this.HasConversion);
            //sb.AppendFormat("Unmanaged Struct            : {0}\n", this.IsUnmanagedStruct);
            //sb.AppendFormat("Managed Struct              : {0}\n", this.IsManagedStruct);
            //sb.AppendFormat("Private Metadata            : {0}\n", this.IsPrivateMetadata);
            //sb.AppendFormat("Marshal by Ref              : {0}\n", this.IsMarshalByRef);
            //sb.AppendFormat("Is Fixed Buffer Struct      : {0}\n", this.IsFixedBufferStruct);
            //sb.AppendFormat("Has No-Argument Constructor : {0}, public: {1}\n", this.HasNoArgCtor, this.HasPubNoArgCtor);
            //sb.AppendFormat("Has UD Static Constructor   : {0}\n", this.HasUserDefinedStaticCtor);
            //sb.AppendFormat("Has Explicit Impl           : {0}\n", this.HasExplicitImpl);
            //sb.AppendFormat("Has Extern References       : {0}\n", this.HasExternReference);
            //sb.AppendFormat("Is SelfComp Valid           : {0}\n", this.IsSelfCmpValid);
            sb.AppendFormat("Has Self-Equality           : {0}\n", this.HasSelfEquality);
            sb.AppendFormat("Has Self-NonEquality        : {0}\n", this.HasSelfNonEquality);
            //sb.AppendFormat("Skip UD Operator            : {0}\n", this.SkipUserDefinedOperators);
            //sb.AppendFormat("Is COM import               : {0}\n", this.IsComImport);
            sb.AppendFormat("Has Method Impl             : {0}\n", this.HasMethodImpl);
            //sb.AppendFormat("Has Link Demand             : {0}\n", this.HasLinkDemand);
            //sb.AppendFormat("Arity in Name               : {0}\n", this.IsArityInName);
            sb.AppendFormat("Typedef Emitted             : {0}\n", this.IsTypeDefEmitted);
            sb.AppendFormat("Bases Emitted               : {0}\n", this.IsBasesEmitted);
            sb.AppendFormat("Memberdef Emitted           : {0}\n", this.IsMemberDefsEmitted);
            //sb.AppendFormat("Layout Checked              : {0}\n", this.IsLayoutChecked);
            //sb.AppendFormat("Layout Errors               : {0}\n", this.HasLayoutError);
            //sb.AppendFormat("Defined in Source           : {0}\n", this.IsSource);
            //sb.AppendFormat("Check Conditional Symbols   : {0}\n", this.CheckedConditionalSymbols);
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugPostfix(sb);
            DebugAgg(sb);
            DebugBag(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class FWDAGGSYM
    //
    /// <summary>
    /// <para>SYM representing a forwarded type
    /// (one which previously existed in one assembly, but has been moved to a new assembly).  
    /// We need this sym in order to resolve typerefs which point to the old assembly,
    /// and should get forwarded to the new one.</para>
    /// </summary>
    //======================================================================
    internal class FWDAGGSYM : SYM
    {
        /// <summary>
        /// Actual AGGSYM this forwarder resolves to. Note that there may be a chain of forwarders.
        /// This will NEVER by an unresolved AGGSYM. If the agg can't be resolved, moduleBreak and
        /// tokBreak are set to where the resolution failed.
        /// </summary>
        protected AGGSYM aggResolve = null;

        protected MODULESYM moduleBreak = null;
        /// <summary>
        /// (R) If resolution of the forwarder fails,
        /// these are set to the unresolvable assembly ref token and the module containing the token.
        /// </summary>
        internal MODULESYM ModuleBreak { get { return moduleBreak; } }

        protected uint tokenBreak = 0;
        internal uint AssemBlyRefBreak { get { return tokenBreak; } }

        protected bool isCycle = false;
        internal bool IsCycle { get { return isCycle; } }

        /// <summary>
        /// Used for cycle detection while resolving.
        /// </summary>
        protected bool isResolving = false;
        

        //------------------------------------------------------------
        // FWDAGGSYM.Resolve
        //
        /// <summary>
        /// Given a type forwarder, resolve it
        /// so that we know which AGGSYM in which assembly it points to.
        /// After resolving the type forwarder,
        /// the target assembly ID and AGGSYM will point to the final valid AGGSYM, 
        /// so in the case where multiple FWDAGGSYMs are chained together,
        /// once the FWDAGGSYM is resolved, there will be no context of the path 
        /// used to get to the final AGGSYM (aside from the assemblyRef token,
        /// which could be used to start back at the beginning).
        /// </summary>
        //------------------------------------------------------------
        protected void Resolve(COMPILER compiler)
        //void FWDAGGSYM::Resolve(COMPILER * compiler)
        {
        //    ASSERT(!aggResolve && !moduleBreak);
        //
        //    ImportScopeModule scope(&compiler.importer, module); 
        //    int aid = compiler.importer.MapAssemblyRefToAid(scope, tkAssemblyRef);
        //    ASSERT(aid != kaidUnresolved); // MapAssemblyRefToAid will create a fake module if the assembly does not exist.
        //    ASSERT(!this.InAlias(aid));
        //
        //    if (isResolving) {
        //        // This is being resolved so this must be a cycle in the type forwarder list.
        //        // Thus, aggResolve should not be set.
        //        compiler.Error(NULL, ERR_CycleInTypeForwarder, this.name, ErrArg(module));
        //        moduleBreak = module;
        //        tokenBreak = tkAssemblyRef;
        //        isCycle = true;
        //    }
        //    // Type forwarders always point to assemblies, if the target AID is a module, then it means MapAssemblyRefToAid
        //    // created one for us because the actual assembly does not exist.
        //    else if (aid < kaidMinModule) {
        //        isResolving = true;
        //
        //        FWDAGGSYM * fwdTmp = NULL;
        //        for (SYM * sym = compiler.LookupInBagAid(this.name, this.parent.AsNSSYM, aid, MASK_AGGSYM | MASK_FWDAGGSYM);
        //            sym;
        //            sym = compiler.LookupNextInAid(sym, aid, MASK_AGGSYM | MASK_FWDAGGSYM))
        //        {
        //            ASSERT(sym.IsAGGSYM || sym.IsFWDAGGSYM);
        //            // check arity to make sure this is the correct one.
        //            if (sym.IsAGGSYM && sym.AsAGGSYM.typeVarsThis.size == this.cvar) {
        //                aggResolve = sym.AsAGGSYM;
        //                break;
        //            }
        //            if (sym.IsFWDAGGSYM && !fwdTmp && sym.AsFWDAGGSYM.cvar == this.cvar)
        //                fwdTmp = sym.AsFWDAGGSYM;
        //        }
        //
        //        if (!aggResolve && fwdTmp) {
        //            // Recursively resolve the forwarder.
        //            aggResolve = fwdTmp.GetAgg(compiler);
        //            if (!aggResolve) {
        //                // Propogate the error information.
        //                moduleBreak = fwdTmp.moduleBreak;
        //                tokenBreak = fwdTmp.tokenBreak;
        //                isCycle = fwdTmp.isCycle;
        //            }
        //        }
        //
        //        ASSERT(isResolving);
        //        isResolving = false;
        //    }
        //
        //    if (!aggResolve && !moduleBreak) {
        //        moduleBreak = module;
        //        tokenBreak = tkAssemblyRef;
        //    }
        //
        //    ASSERT(aggResolve || moduleBreak);
        //
        }

        /// <summary>
        /// number of type parameters on this type.  This is derived from the name, since that is all we have to go on.
        /// </summary>
        internal int TypeParamsCount = 0;

        /// <summary>
        /// Meta-data token used for importing.
        /// </summary>
        internal int ImportedToken = 0;

        /// <summary>
        /// Meta-data module imported from
        /// </summary>
        internal MODULESYM Module = null;

        /// <summary>
        /// Target assembly of this forwarder.  Instead of using this, you should just resolve this forwarder and use
        /// GetTargetAssemblyID() instead, that will point to the final assembly (if multiple forwarders are strung together).
        /// </summary>
        internal uint AssemblyRefToken = 0;

        internal INFILESYM InFileSym;

        //------------------------------------------------------------
        // FWDAGGSYM.GetAgg
        //
        /// <summary>
        /// Get the AGGSYM that the FWDAGGSYM resolves to.
        /// </summary>
        //------------------------------------------------------------
        internal AGGSYM GetAgg(COMPILER compiler)
        {
            if (aggResolve == null && moduleBreak == null) Resolve(compiler);
            return aggResolve;
        }

        // These are only valid after GetAgg has been called (and returned NULL).
        //internal MODULESYM GetModuleBreak() { return moduleBreak; }
        //internal uint GetAssemRefBreak() { return tokenBreak; }
        //internal bool FCycle() { return isCycle; }

        //------------------------------------------------------------
        // FWDAGGSYM.InAlias
        //------------------------------------------------------------
        internal bool InAlias(int aid)
        {
            DebugUtil.Assert(InFileSym != null);
            DebugUtil.Assert(0 <= aid);

            if (aid < Kaid.MinModule)
            {
                return InFileSym.InAlias(aid);
            }
            return (aid == Module.GetModuleID());
        }

#if DEBUG
        //------------------------------------------------------------
        // FWDAGGSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class UNRESAGGSYM
    //
    /// <summary>
    /// <para>A fabricated AGGSYM to represent an imported type that we couldn't resolve.
    /// Used for error reporting.
    /// In the EE this is used as a place holder until the real AGGSYM is created.</para>
    /// <para>Drives from AGGSYM.</para>
    /// </summary>
    //======================================================================
    internal class UNRESAGGSYM : AGGSYM
    {
        /// <summary>
        /// TypeRef information that couldn't be resolved.
        /// </summary>
        internal MODULESYM ModuleRef = null;
        internal uint TokenRef = 0;

        internal CMetadataFile MetadataFile = null;
        internal string AssemblyName
        {
            get { return (this.MetadataFile != null ? this.MetadataFile.AssemblyNameString : null); }
        }

        /// <summary>
        /// Module and typeref or assembly ref for error information.
        /// This may be different than the above if there are type forwarders involved.
        /// </summary>
        internal MODULESYM ModuleErr = null;
        internal uint TokenErr = 0;

        /// <summary>
        /// An error was issued when the UNRESAGGSYM was created, so don't issue one on use.
        /// This is used to supress a bogus error when there is a type-forwarder cycle, for example.
        /// </summary>
        internal bool SuppressError = false;

#if DEBUG
        //------------------------------------------------------------
        // UNRESAGGSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // PERMSETINFO
    //
    /// <summary>
    /// capability security on a symbol
    /// (CSharp\SCComp\Symbol.cs)
    /// </summary>
    //======================================================================
    internal class PERMSETINFO
    {
        private bool isAnySet;
        internal bool IsAnySet
        {
            get { return isAnySet; }
            set { isAnySet = value; }
        }

        private bool[] isGuid = new bool[10];
        internal bool[] IsGuid { get { return isGuid; } }

        private string[] str = new string[10];
        internal string[] StringArray { get { return str; } }

        internal bool isSet(int index) { return str[index] != null; }
    }
    //typedef PERMSETINFO * PPERMSETINFO;

    //======================================================================
    // AGGINFO
    //
    /// <summary>
    /// <para>Additional information about an aggregate symbol
    /// that isn't needed by other code binding against this aggregate.
    /// This structure lives only when this particular aggregate is being compiled.</para>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal class AGGINFO
    {
        internal bool HasStructLayout = false;      // hasStructLayout:1;
        internal bool HasExplicitLayout = false;    // hasExplicitLayout:1;
        internal bool HasUUID = false;              // hasUuid:1; // COM classic attribute
        internal bool IsComImport = false;          // isComimport:1; // COM classic attribute
    }

    //======================================================================
    // enum ARRAYMETHOD
    //
    /// <summary>
    /// The pseudo-methods uses for accessing arrays (except in the optimized 1-d case).
    /// (CSharp\SCComp\Symbol.cs)
    /// </summary>
    //======================================================================
    internal enum ARRAYMETHOD : int
    {
        LOAD,
        LOADADDR,
        STORE,
        CTOR,
        GETAT,  // Keep these in this order!!!

        COUNT
    }

    //======================================================================
    // class ALIASSYM
    //
    /// <summary>
    /// <para>ALIASSYM - a symbol representing an using alias clause</para>
    /// <para>Its parent is an NSDECLSYM,
    /// but it is not linked into the child list or in the symbol table.</para>
    /// </summary>
    //======================================================================
    internal class ALIASSYM : SYM
    {
        internal bool HasBeenBound;
        
        internal bool IsExtern;

        /// <summary>
        /// Can be NSAIDSYM or AGGTYPESYM.
        /// </summary>
        internal SYM Sym;

        /// <summary>
        /// (RW) sym as NSAIDSYM
        /// </summary>
        internal NSAIDSYM NsAidSym
        {
            get { return Sym != null ? (Sym as NSAIDSYM) : null; }
            set { Sym = value; }
        }
        /// <summary>
        /// (RW) Use sym field as AggTypeSym.
        /// </summary>
        internal AGGTYPESYM AggTypeSym
        {
            get { return Sym != null ? (Sym as AGGTYPESYM) : null; }
            set { Sym = value; }
        }
        
        /// <summary>
        /// Duplicate symbol - report error on use with . if this is non-null.
        /// </summary>
        internal SYM DuplicatedSym;
        
        internal BASENODE ParseTreeNode;

#if DEBUG
        //------------------------------------------------------------
        // ALIASSYM Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            if (this.NsAidSym != null)
            {
                sb.AppendFormat("Alias Type : {0}\n\n", "NSAIDSYM");
                this.NsAidSym.Debug(sb);
            }
            else if (this.AggTypeSym != null)
            {
                sb.AppendFormat("Alias Type : {0}\n\n", "AGGTYPESYM");
                this.AggTypeSym.Debug(sb);
            }
            else
            {
                DebugPrefix(sb);
                DebugPostfix(sb);
                sb.AppendFormat("Alias Type :\n\n", "NSAIDSYM");
            }
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class ARRAYSYM
    //
    /// <summary>
    /// <para>ARRAYSYM - a symbol representing an array.</para>
    /// <para>Derives from TYPESYM</para>
    /// </summary>
    //======================================================================
    internal class ARRAYSYM : TYPESYM
    {
        /// <summary>
        /// rank of the array.                                                 
        /// zero means unknown rank int [?].
        /// </summary>
        internal int Rank;  // rank

        /// <summary>
        /// Metadata token (typeRef) in the current output file.
        /// </summary>
        //internal uint EmittedMetadataToken = 0; // mdTypeRef  tokenEmit
        
        /// <summary>
        /// parent is the element type.
        /// </summary>
        internal TYPESYM ElementTypeSym // elementType()
        {
            get { return (ParentSym as TYPESYM); }
        }

        /// <summary>
        /// (sscli) Metadata tokens for ctor/load/loadaddr/store special methods.
        /// </summary>
        private uint[] tokenEmitPseudoMethods = new uint[(int)ARRAYMETHOD.COUNT];

        /// <summary>
        /// <para>(R) MethodInfos for ctor/load/loadaddr/store special methods.</para>
        /// <para>In place of MetadataTokensEmittingPseudoMethods</para>
        /// </summary>
        internal MethodInfo[] ArrayPseudoMethods = { null, null, null, null, null };

        /// <summary>
        /// <para>(R) ConstructorInfo for ctor special methods.</para>
        /// <para>In place of MetadataTokensEmittingPseudoMethods</para>
        /// </summary>
        //internal ConstructorInfo ArrayConstructor = null;
        internal MethodInfo ArrayConstructorMethodInfo = null;

        //------------------------------------------------------------
        // ARRAYSYM.GetMostBaseType
        //
        /// <summary>
        /// Returns the first non-array type in the parent chain.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM GetMostBaseType()
        {
            TYPESYM type;
            for (type = ParentSym as TYPESYM; type.IsARRAYSYM; type = type.ParentSym as TYPESYM) ;
            return type;
        }

        //------------------------------------------------------------
        // ARRAYSYM.MakeSystemType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type MakeSystemType()
        {
            Type elementType = null;
            if (this.ElementTypeSym == null ||
                (elementType = this.ElementTypeSym.Type) == null ||
                this.Rank == 0)
            {
                return null;
            }

            try
            {
                if (this.Rank == 1)
                {
                    return elementType.MakeArrayType();
                }
                else if (this.Rank > 1)
                {
                    return elementType.MakeArrayType(this.Rank);
                }
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

#if DEBUG
        //------------------------------------------------------------
        // ARRAYSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class IMPLICITLYTYPEDARRAYSYM
    //
    /// <summary>
    /// (CS3) implicitly typed array
    /// </summary>
    //======================================================================
    internal class IMPLICITLYTYPEDARRAYSYM : ARRAYSYM
    {
#if DEBUG
        //------------------------------------------------------------
        // ARRAYSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class PARAMMODSYM
    //
    /// <summary>
    /// <para>PARAMMODSYM
    /// - a symbol representing parameter modifier -- either out or ref.</para>
    /// </summary>
    //======================================================================
    internal class PARAMMODSYM : TYPESYM
    {
        /// <summary>
        /// One of these two bits must be set,
        /// </summary>
        internal bool IsRef = false;    // bool isRef: 1;

        /// <summary>
        /// indication a ref or out parameter.
        /// </summary>
        internal bool IsOut = false;    // bool isOut: 1;

        /// <summary>
        /// <para>parent is the parameter type.</para>
        /// <para>The modified type is the parent sym. return the parent sym.</para>
        /// </summary>
        internal TYPESYM ParamTypeSym
        {
            get // paramType()
            {
                DebugUtil.Assert(this.ParentSym != null);
                return (ParentSym as TYPESYM);
            }
        }
#if false
        //------------------------------------------------------------
        // PARAMMODSYM.GetSystemType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override Type GetSystemType(
            TypeArray classTypeArguments,
            TypeArray methodTypeArguments)
        {
            if (this.type == null)
            {
                TYPESYM parentTypeSym = this.ParentSym as TYPESYM;
                DebugUtil.Assert(parentTypeSym != null);
                Type parentType = parentTypeSym.GetSystemType(null, null);
                if (parentType != null)
                {
                    SetSystemType(parentType.MakeByRefType());
                }
            }
            return this.type;
        }
#endif
#if DEBUG
        //------------------------------------------------------------
        // PARAMMODSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class MODOPTSYM
    //
    /// <summary>
    /// MODOPTSYM - a symbol representing a modopt from an imported signature.
    /// Parented by a MODULESYM.
    /// Contains the import token.
    /// Caches the emit token.
    /// </summary>
    //======================================================================
    internal class MODOPTSYM : SYM
    {
        /// <summary>
        /// <para>(R) Return the parent as MODULESYM.</para>
        /// 親を MODULESYM として返す。
        /// </summary>
        internal MODULESYM ModuleSym
        {
            get { return ParentSym as MODULESYM; }  // MODULESYM * GetModule()
        }

        internal int ImportedToken; // mdToken tokImport;
        internal int EmittedToken;  // mdToken tokEmit;

#if DEBUG
        //------------------------------------------------------------
        // MODOPTSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class MODOPTTYPESYM
    //
    /// <summary>
    /// MODOPTTYPESYM - a symbol representing a modopt modifying a type.
    /// Parented by a TYPESYM.
    /// Contains a MODOPTSYM.
    /// </summary>
    //======================================================================
    internal class MODOPTTYPESYM : TYPESYM
    {
        internal MODOPTSYM ModOptSym = null;    // * opt

        /// <summary>
        /// <para>parent is modified type.</para>
        /// </summary>
        internal TYPESYM BaseTypeSym
        {
            get { return ParentSym as TYPESYM; }    // baseType()
        }

#if DEBUG
        //------------------------------------------------------------
        // MODOPTTYPESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // AGGTYPESYM
    //
    /// <summary>
    /// <para>Represents a generic constructed (or instantiated) type.
    /// Parent is the AGGSYM.</para>
    /// <para>Derives from TYPESYM.</para>
    /// </summary>
    //======================================================================
    internal class AGGTYPESYM : TYPESYM
    {
        //------------------------------------------------------------
        // AGGTYPESYM Fields and Properties
        //------------------------------------------------------------
        // The derived field "type" holds the constructed type.

        /// <summary>
        /// This is the result of calling SubstTypeArray on the aggregate's baseClass.
        /// </summary>
        private AGGTYPESYM baseTypeSym = null;  // baseType

        /// <summary>
        /// This is the result of calling SubstTypeArray on the aggregate's ifacesAll.
        /// </summary>
        private TypeArray allInterfaces = null; // ifacesAll

        /// <summary>
        /// Array of arguments, e.g. "[String]" in "List&lt;String&gt;"
        /// </summary>
        internal TypeArray TypeArguments = null; // typeArgsThis

        /// <summary>
        /// <para>includes args from outer types</para>
        /// </summary>
        internal TypeArray AllTypeArguments = null;   // typeArgsAll

        /// <summary>
        /// the outer type if this is a nested type
        /// </summary>
        internal AGGTYPESYM OuterTypeSym = null;    // outerType

        /// <summary>
        /// Metadata token (typeSpec) in the current output file.
        /// </summary>
        internal int EmittedTypeSpecToken = 0;  // tokenEmit

        /// <summary>
        /// Have the constraints been checked yet?
        /// </summary>
        internal bool ConstraintsChecked = false;   // uint fConstraintsChecked: 1;

        /// <summary>
        /// Did the constraints check produce an error?
        /// </summary>
        internal bool HasConstraintError = false;   // uint fConstraintError: 1;

        // These two flags are used to track hiding within interfaces.
        // Their use and validity is always localized. See e.g. MemberLookup::LookupInInterfaces.

        /// <summary>
        /// All members are hidden by a derived interface member.
        /// </summary>
        internal bool AllHidden = false;    // uint fAllHidden: 1;

        /// <summary>
        /// Members other than a specific kind are hidden by a derived interface member or class member.
        /// </summary>
        internal bool DiffHidden = false;   // fDiffHidden: 1;

        private int bitDefAssgCount = 0;    // uint cbitDefAssg: 24;

        /// <summary>
        /// The number of bits required to represent this type (for definite assignment) plus one.
        /// It is zero if it hasn't yet been set.
        /// </summary>
        internal int BitDefAssgCount
        {
            get // GetCbitDefAssg()
            {
                DebugUtil.Assert(bitDefAssgCount > 0);
                return bitDefAssgCount - 1;
            }
            set // SetCbitDefAssg(int cbit)
            {
                DebugUtil.Assert(value >= 0);

                if (value >= 0)
                {
                    bitDefAssgCount = value + 1;
                }
            }
        }

        internal bool IsBitDefAssgCountSet  // FCbitDefAssgSet()
        {
            get { return bitDefAssgCount > 0; }
        }
#if false
        //------------------------------------------------------------
        // AGGTYPESYM.GetSystemType
        //
        /// <summary></summary>
        /// <param name="classTypeArguments">dummy, set null.</param>
        /// <param name="methodTypeArguments">dummy, set null.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override Type GetSystemType(
            TypeArray classTypeArguments,
            TypeArray methodTypeArguments)
        {
            if (this.type != null)
            {
                return this.type;
            }
            return GetConstructedType(false);
        }
#endif

        /// <summary>
        /// (CS3) This type represents a direct product of parameters.
        /// </summary>
        internal bool IsParameterProcuct
        {
            get { return this.GetAggregate().IsParameterProcuct; }
        }

        //------------------------------------------------------------
        // AGGTYPESYM.GetBaseClass
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetBaseClass()
        {
            if (baseTypeSym == null)
            {
                baseTypeSym = GetAggregate().SymbolManager.SubstType(
                    GetAggregate().BaseClassSym,
                    AllTypeArguments,
                    null,
                    SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
            }
            return baseTypeSym;
        }

        //------------------------------------------------------------
        // AGGTYPESYM.GetIfacesAll
        //
        /// <summary>
        /// Instanciate all interfaces and return them as a TypeArray.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray GetIfacesAll()
        {
            if (allInterfaces == null)
            {
                allInterfaces = GetAggregate().SymbolManager.SubstTypeArray(
                    GetAggregate().AllInterfaces,
                    AllTypeArguments,
                    null,
                    SubstTypeFlagsEnum.NormNone);
            }
            return allInterfaces;
        }

        //------------------------------------------------------------
        // AGGTYPESYM.FindBaseType (1)
        //
        /// <summary>
        /// This looks for the given AGGSYM among this type
        /// and its base types and returns the corresponding type.
        /// </summary>
        /// <param name="agg"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM FindBaseType(AGGSYM agg)
        {
            for (AGGTYPESYM ats = (this as AGGTYPESYM); ats != null; ats = ats.GetBaseClass())
            {
                if (ats.GetAggregate() == agg)
                {
                    return ats;
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // AGGTYPESYM.FindBaseType (2)
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindBaseType(AGGTYPESYM type)
        {
            return FindBaseType(type.GetAggregate()) == type;
        }

        //------------------------------------------------------------
        // AGGTYPESYM.ParentAggSym    (Property)
        //------------------------------------------------------------
        internal AGGSYM ParentAggSym
        {
            get { return (ParentSym as AGGSYM); }
        }

        //------------------------------------------------------------
        // AGGTYPESYM.IsInstType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsInstType()
        {
            AGGSYM sym = this.ParentAggSym;
            return sym != null ? sym.GetThisType() == this : false;
        }

        //------------------------------------------------------------
        // AGGTYPESYM.GetInstType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetInstType()
        {
            AGGSYM sym = this.ParentAggSym;
            return sym != null ? sym.GetThisType() : null;
        }

        //------------------------------------------------------------
        // AGGTYPESYM.GetCbitDefAssg
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetCbitDefAssg()
        {
            //ASSERT(cbitDefAssg > 0);
            return (int)bitDefAssgCount - 1;
        }

        //------------------------------------------------------------
        // AGGTYPESYM.SetCbitDefAssg
        //
        /// <summary></summary>
        /// <param name="cbit"></param>
        //------------------------------------------------------------
        internal void SetCbitDefAssg(int cbit)
        {
            //ASSERT(cbit >= 0);
            bitDefAssgCount = (int)(cbit + 1);
        }

        //------------------------------------------------------------
        // AGGTYPESYM.GetConstructedType
        //
        /// <summary></summary>
        /// <param name="remake"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type GetConstructedType(
            AGGSYM tvAggSym,
            METHSYM tvMethodSym,
            bool remake)
        {
            if (this.type != null &&
                !remake &&
                !this.type.IsGenericTypeDefinition)
            {
                return this.type;
            }

            this.type = SymUtil.GetSystemTypeFromSym(
                this,
                tvAggSym,
                tvMethodSym);
            return this.type;
#if false
            AGGSYM aggSym = this.ParentSym as AGGSYM;
            if (aggSym == null)
            {
                this.type = null;
                return null;
            }

            Type tempType = aggSym.Type;
            if (tempType == null)
            {
                this.type = null;
                return null;
            }

            if (this.AllTypeArguments == null || this.AllTypeArguments.Count == 0)
            {
                this.type = tempType;
                return this.type;
            }

            Type[] typeArgs = null;
            if (this.AllTypeArguments.GetSystemTypeArray(null, null, out typeArgs))
            {
                try
                {
                    this.type = tempType.MakeGenericType(typeArgs);
                }
                catch (ArgumentException)
                {
                    this.type = null;
                }
            }
            else
            {
                this.type = null;
            }
            return this.type;
#endif
        }

#if DEBUG
        //------------------------------------------------------------
        // AGGTYPESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }

        internal override Type DebugGetSystemType()
        {
            return this.type;
        }
#endif
    }

    //======================================================================
    // enum SpecialConstraintEnum
    //
    /// <summary>
    /// <para>In sscli, SpecCons.</para>
    /// <para>None, New, Reference, Value (CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal enum SpecialConstraintEnum : int
    {
        None = 0x00,

        New = 0x01,
        Reference = 0x02,   // Ref
        Value = 0x04,       // Val
    }

    //======================================================================
    // TYVARSYM
    //
    /// <summary>
    /// <para>Represents a type variable within an aggregate or method.
    /// Parent is the owning AGGSYM or METHSYM.
    /// There are canonical TYVARSYMs for each index used for normalization of emitted metadata.</para>
    /// <para>Derives from TYPESYM.</para>
    /// </summary>
    //======================================================================
    internal class TYVARSYM : TYPESYM
    {
        internal override Type Type
        {
            get
            {
                throw new NotImplementedException("Do not call TYVARSYM.Type");
            }
        }

        //protected GenericTypeParameterBuilder typeParameterBuilder = null;

        /// <summary>
        /// Special constraints.
        /// </summary>
        internal SpecialConstraintEnum Constraints = SpecialConstraintEnum.None;    // uint cons: 8;

        /// <summary>
        /// true for method type variable, false for class type variable.
        /// </summary>
        internal bool IsMethodTypeVariable = false; // uint isMethTyVar: 1;

        /// <summary>
        /// Used locally by DefineBounds.
        /// </summary>
        internal bool SeenWhere = false;    // uint seenWhere: 1;

        /// <summary>
        /// Use for recursion detection while computing ifacesAll (in ResolveBounds).
        /// </summary>
        internal bool IsResolving = false;  // fResolving: 1;

        /// <summary>
        /// Whether typeBaseAbs implies that this type variable is a reference type.
        /// </summary>
        internal bool HasReferenceBound = false;    // uint fHasRefBnd: 1;

        /// <summary>
        /// Whether typeBaseAbs implies that this type variable is a value type. Rarely set....
        /// </summary>
        internal bool HasValueBound = false;    // uint fHasValBnd: 1;

        /// <summary>
        /// Bounds. Contains: class (0 or 1), type vars (0 or more), interfaces (0 or more).
        /// </summary>
        private TypeArray boundArray = null;    // bnds

        internal TypeArray BoundArray
        {
            get { return this.boundArray; } // GetBnds()
        }

        private AGGTYPESYM baseClassSym = null; // atsBaseCls

        /// <summary>
        /// (R) The effective base class.
        /// </summary>
        internal AGGTYPESYM BaseClassSym    // GetBaseCls()
        {
            get { return baseClassSym; }
        }

        private TYPESYM absoluteBaseTypeSym = null; // typeBaseAbs

        /// <summary>
        /// (R) Most derived type bound. This is usually an AGGTYPESYM, but may be a NUBSYM or ARRAYSYM.
        /// </summary>
        internal TYPESYM AbsoluteBaseTypeSym    // GetAbsoluteBaseType()
        {
            get { return absoluteBaseTypeSym; }
        }

        /// <summary>
        /// <para>Recursive closure of the interface bounds. The effective interface list (and base ifaces).</para>
        /// </summary>
        /// <remarks> in sscli
        /// ifacesAll,
        /// TypeArray * GetIfacesAll(),
        /// void SetIfacesAll(TypeArray * ifacesAll)
        /// </remarks>
        internal TypeArray AllInterfaces = null;

        /// <summary>
        /// parse tree
        /// </summary>
        internal TYPEBASENODE ParseTreeNode = null; // parseTree

        /// <summary>
        /// no. of tyvar in declaration list
        /// </summary>
        internal int Index = -1;    // index

        /// <summary>
        /// no. of tyvar starting at outer most type
        /// </summary>
        internal int TotalIndex = -1;   // indexTotal

        /// <summary>
        /// Metadata token (typeSpec) in the current output file.
        /// (this is only used for standard type variables)
        /// </summary>
        internal int EmittedToken = 0;  // tokenEmit

        internal List<ATTRINFO> AttributeList = new List<ATTRINFO>();    // attributeList

        internal ATTRINFO AttributeListTail
        {
            get { return (AttributeList.Count != 0 ? AttributeList[AttributeList.Count - 1] : null); }
        }

        internal AGGSYM ParentAggSym
        {
            get { return ParentSym as AGGSYM; }
        }

        internal AGGSYM ClassSym   // getClass()
        {
            get { return ParentSym as AGGSYM; }
        }
#if false
        //------------------------------------------------------------
        // TYVARSYM.GetSystemType
        //
        /// <summary>
        /// If type arguments are specified, returns the corresponding type.
        /// else returns the System.Type of this.
        /// </summary>
        /// <param name="classTypeArguments"></param>
        /// <param name="methodTypeArguments"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override Type GetSystemType(
            TypeArray classTypeArguments,
            TypeArray methodTypeArguments)
        {
            int cCount = TypeArray.Size(classTypeArguments);
            int mCount = TypeArray.Size(methodTypeArguments);

            if (cCount == 0 && mCount == 0)
            {
                return this.type;
            }

            if (this.IsMethodTypeVariable)
            {
                if (this.Index < mCount)
                {
                    return methodTypeArguments[this.Index].GetSystemType(
                        classTypeArguments,
                        methodTypeArguments);
                }
            }
            else
            {
                if (this.Index < cCount)
                {
                    return classTypeArguments[this.Index].GetSystemType(
                        classTypeArguments,
                        methodTypeArguments);
                }
            }
            return this.type;
        }
#endif
#if false
        //------------------------------------------------------------
        // TYVARSYM.SetSystemType
        //
        /// <summary></summary>
        /// <param name="ty"></param>
        //------------------------------------------------------------
        internal override void SetSystemType(Type ty)
        {
            if (ty is GenericTypeParameterBuilder)
            {
                SetGenericTypeParameterBuilder(ty as GenericTypeParameterBuilder);
            }
            else
            {
                this.type = ty;
            }
        }
#endif
        //------------------------------------------------------------
        // TYVARSYM.GetGenericParameterType (1)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type GetGenericParameterType(AGGSYM aggSym)
        {
            if (this.IsMethodTypeVariable || this.TotalIndex < 0)
            {
                return null;
            }
            if (aggSym == null ||
                aggSym.GenericParameterTypes == null ||
                this.TotalIndex >= aggSym.GenericParameterTypes.Length)
            {
                return null;
            }
            return aggSym.GenericParameterTypes[this.TotalIndex];
        }

        //------------------------------------------------------------
        // TYVARSYM.GetGenericParameterType (2)
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type GetGenericParameterType(METHSYM methodSym)
        {
            if (!this.IsMethodTypeVariable || this.TotalIndex < 0)
            {
                return null;
            }
            if (methodSym == null ||
                methodSym.GenericParameterTypes == null ||
                this.TotalIndex >= methodSym.GenericParameterTypes.Length)
            {
                return null;
            }
            return methodSym.GenericParameterTypes[this.TotalIndex];
        }

        //------------------------------------------------------------
        // TYVARSYM.GetGenericParameterType (3)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type GetGenericParameterType()
        {
            if (this.IsMethodTypeVariable)
            {
                return GetGenericParameterType(this.ParentSym as METHSYM);
            }
            else
            {
                return GetGenericParameterType(this.ParentSym as AGGSYM);
            }
        }

#if false
        //------------------------------------------------------------
        // TYVARSYM.SetGenericTypeParameterBuilder
        //
        /// <summary></summary>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal void SetGenericTypeParameterBuilder(GenericTypeParameterBuilder builder)
        {
            this.typeParameterBuilder = builder;
            this.type = builder;
        }
#endif
        //------------------------------------------------------------
        // TYVARSYM.SetBounds
        //
        /// <summary></summary>
        /// <param name="bnds"></param>
        //------------------------------------------------------------
        internal void SetBounds(TypeArray bnds)
        {
            this.boundArray = bnds;
            this.baseClassSym = null;
            this.absoluteBaseTypeSym = null;
            this.AllInterfaces = null;
            this.HasReferenceBound = false;
            this.HasValueBound = false;
        }

        //------------------------------------------------------------
        // TYVARSYM.SetBaseTypes
        //
        /// <summary></summary>
        /// <param name="typeBaseAbs"></param>
        /// <param name="atsBaseCls"></param>
        //------------------------------------------------------------
        internal void SetBaseTypes(TYPESYM typeBaseAbs, AGGTYPESYM atsBaseCls)
        {
            this.absoluteBaseTypeSym = typeBaseAbs;
            this.baseClassSym = atsBaseCls;
        }

        //------------------------------------------------------------
        // TYVARSYM.SetAllInterfaces
        //
        /// <summary></summary>
        /// <param name="ifaces"></param>
        //------------------------------------------------------------
        //internal void SetAllInterfaces(TypeArray ifaces)
        //{
        //    this.allInterfaces = ifaces;
        //}

        //------------------------------------------------------------
        // TYVARSYM.FResolved
        //
        /// <summary>
        /// Return true if AllInterfaces is already set.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FResolved()
        {
            DebugUtil.Assert(BoundArray != null || AbsoluteBaseTypeSym == null);
            DebugUtil.Assert((AbsoluteBaseTypeSym == null) == (BaseClassSym == null) &&
                (AbsoluteBaseTypeSym == null) == (AllInterfaces == null));

            return (AllInterfaces != null);
        }

        //------------------------------------------------------------
        // TYVARSYM.IsReferenceType
        //
        /// <summary>
        /// Returns true iff the type variable must be a reference type.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool IsReferenceType()
        {
            return (Constraints & SpecialConstraintEnum.Reference) != 0 || HasReferenceBound;
        }

        //------------------------------------------------------------
        // TYVARSYM.IsValueType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool IsValueType()
        {
            return (Constraints & SpecialConstraintEnum.Value) != 0 || HasValueBound;
        }

        //------------------------------------------------------------
        // TYVARSYM.IsNonNullableValueType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool IsNonNullableValueType()
        {
            return (Constraints & SpecialConstraintEnum.Value) != 0 || HasValueBound && !AbsoluteBaseTypeSym.IsNUBSYM;
        }

        //------------------------------------------------------------
        // TYVARSYM.HasNewConstraint
        //
        /// <summary>
        /// <para>FNewCon() in sscli.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasNewConstraint()    
        {
            return (Constraints & SpecialConstraintEnum.New) != 0;
        }

        //------------------------------------------------------------
        // TYVARSYM.HasReferenceConstraint
        //
        /// <summary>
        /// <para>FRefCon() in sscli.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasReferenceConstraint()
        {
            return (Constraints & SpecialConstraintEnum.Reference) != 0;
        }

        //------------------------------------------------------------
        // TYVARSYM.HasValueConstraint
        //
        /// <summary>
        /// <para>(FValCon() in sscli.)</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasValueConstraint()
        {
            return (Constraints & SpecialConstraintEnum.Value) != 0;
        }

        //------------------------------------------------------------
        // TYVARSYM.CanNew
        //
        /// <summary>
        /// <para>(In sscli, FCanNew())</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CanNew()
        {
            return (
                (Constraints & (SpecialConstraintEnum.New | SpecialConstraintEnum.Value)) != 0 ||
                HasValueBound);
        }

        //------------------------------------------------------------
        // TYVARSYM.CopyFrom
        //
        /// <summary></summary>
        /// <param name="other"></param>
        //------------------------------------------------------------
        internal override void CopyFrom(SYM sym)
        {
            TYVARSYM other = sym as TYVARSYM;
            if (other == null)
            {
                return;
            }

            base.CopyFrom(other);

            //this.typeParameterBuilder = other.typeParameterBuilder;
            this.Constraints = other.Constraints;
            this.IsMethodTypeVariable = other.IsMethodTypeVariable;
            this.SeenWhere = other.SeenWhere;
            this.IsResolving = other.IsResolving;
            this.HasReferenceBound = other.HasReferenceBound;
            this.HasValueBound = other.HasValueBound;
            this.boundArray = other.boundArray;
            this.baseClassSym = other.baseClassSym;
            this.absoluteBaseTypeSym = other.absoluteBaseTypeSym;
            this.AllInterfaces = other.AllInterfaces;
            this.ParseTreeNode = other.ParseTreeNode;
            this.Index = other.Index;
            this.TotalIndex = other.TotalIndex;
            this.EmittedToken = other.EmittedToken;
            this.AttributeList = other.AttributeList;
        }

#if DEBUG
        //------------------------------------------------------------
        // TYVARSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class PTRSYM
    //
    /// <summary>
    /// <para>PTRSYM - a symbol representing a pointer type</para>
    /// <para>Derives from TYPESYM.</para>
    /// </summary>
    //======================================================================
    internal class PTRSYM : TYPESYM
    {
        /// <summary>
        /// Metadata token (typeRef) in the current output file.
        /// </summary>
        internal int EmittedToken = 0;

        /// <summary>
        /// parent is the base type.
        /// </summary>
        internal TYPESYM BaseTypeSym
        {
            get { return (ParentSym as TYPESYM); }  // baseType()
        }

        //------------------------------------------------------------
        // PTRSYM.GetMostBaseType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM GetMostBaseType()
        {
            TYPESYM type;
            for (type = ParentSym as TYPESYM; type.IsPTRSYM; type = type.ParentSym as TYPESYM)
                ;
            return type;
        }
#if false
        //------------------------------------------------------------
        // PTRSYM.GetSystemType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override Type GetSystemType(
            TypeArray classTypeArguments,
            TypeArray methodTypeArguments)
        {
            if (this.type != null && this.type.IsPointer)
            {
                return this.type;
            }
            this.type = MakeSystemType();
            return this.type;
        }
#endif
        //------------------------------------------------------------
        // PTRSYM.MakeSystemType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Type MakeSystemType()
        {
            Type baseType = null;
            if (this.BaseTypeSym == null ||
                (baseType = this.BaseTypeSym.Type) == null)
            {
                return null;
            }

            return baseType.MakePointerType();
        }

#if DEBUG
        //------------------------------------------------------------
        // PTRSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // NUBSYM
    //
    /// <summary>
    /// <para>A "derived" type representing Nullable&lt;T&gt;. The base type T is the parent.</para>
    /// <para>Derives from TYPESYM.</para>
    /// </summary>
    //======================================================================
    internal class NUBSYM : TYPESYM
    {
        //friend class BSYMMGR; // So it can set hasErrors and fUnres.

        private AGGTYPESYM aggTypeSym = null;   // ats

        internal BSYMMGR SymbolManager = null;   // symmgr

        /// <summary>
        /// Syms of base types of nullable types are the parent syms.
        /// Return the parent sym as TYPESYM.
        /// </summary>
        internal TYPESYM BaseTypeSym    // baseType()
        {
            get { return ParentSym as TYPESYM; }
        }

        //------------------------------------------------------------
        // NUBSYM.GetAggTypeSym
        //
        /// <summary>
        /// <para>Get the equivalent Nullable&lt;T&gt; for the given T?.</para>
        /// <para>WARNING: This may return NULL if the Nullable predefined type is not found!</para>
        /// <para>(GetAts() in sscli.)</para>
        /// </summary>
        //------------------------------------------------------------
        internal AGGTYPESYM GetAggTypeSym()
        {
            if (this.aggTypeSym == null)
            {
                AGGSYM aggNullable = this.SymbolManager.GetNullable();
                if (aggNullable == null)
                {
                    this.SymbolManager.ReportMissingPredefTypeError(PREDEFTYPE.G_OPTIONAL);
                    return null;
                }

                TypeArray ta = this.SymbolManager.AllocParams(this.ParentSym as TYPESYM);
                this.aggTypeSym = this.SymbolManager.GetInstAgg(aggNullable, ta);
            }
            return this.aggTypeSym;
        }

#if DEBUG
        //------------------------------------------------------------
        // NUBSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class PINNEDSYM
    //
    /// <summary>
    /// <para>PINNEDSYM - a symbol representing a pinned type
    /// used only to communicate between ilgen &amp; emitter</para>
    /// </summary>
    //======================================================================
    internal class PINNEDSYM : TYPESYM
    {
        /// <summary>
        /// parent is the base type.
        /// </summary>
        internal TYPESYM BaseTypeSym
        {
            get { return ParentSym != null ? (ParentSym as TYPESYM) : null; }
        }

#if DEBUG
        //------------------------------------------------------------
        // PINNEDSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class VOIDSYM
    //
    /// <summary>
    /// <para>VOIDSYM - represents the type "void".</para>
    /// <para>Derives from TYPESYM. Has no own member.</para>
    /// </para>
    /// </summary>
    //======================================================================
    internal class VOIDSYM : TYPESYM
    {
        internal VOIDSYM()
        {
            this.SetSystemType(typeof(void), false);
        }

#if DEBUG
        //------------------------------------------------------------
        // VOIDSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class NULLSYM
    //
    /// <summary>
    /// <para>NULLSYM - represents the null type -- the type of the "null constant".</para>
    /// <para>Derives from TYPESYM. Has no own member.</para>
    /// </summary>
    //======================================================================
    internal class NULLSYM : TYPESYM
    {

#if DEBUG
        //------------------------------------------------------------
        // NULLSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class UNITSYM
    //
    /// <summary>
    /// <para>UNITSYM - a placeholder typesym used only in type argument lists for open types.
    /// There is exactly one of these.</para>
    /// <para>Derives from TYPESYM. Has no own member.</para>
    /// </summary>
    //======================================================================
    internal class UNITSYM : TYPESYM
    {

#if DEBUG
        //------------------------------------------------------------
        // UNITSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class ANONMETHSYM
    //
    /// <summary>
    /// <para>ANONMETHSYM - a placeholder typesym
    /// used only as the type of an anonymous method expression.
    /// There is exactly one of these.</para>
    /// <para>Derives from TYPESYM. Has no own member.</para>
    /// </summary>
    //======================================================================
    internal class ANONMETHSYM : TYPESYM
    {

#if DEBUG
        //------------------------------------------------------------
        // ANONMETHSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class LAMBDAEXPRSYM
    //
    /// <summary>
    /// (CS3) Simular to ANONMETHSYM
    /// </summary>
    //======================================================================
    internal class LAMBDAEXPRSYM : ANONMETHSYM
    {

#if DEBUG
        //------------------------------------------------------------
        // LAMBDAEXPRSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class METHGRPSYM
    //
    /// <summary>
    /// <para>METHGRPSYM - a placeholder typesym used only as the type of an method groupe expression.
    /// There is exactly one of these.</para>
    /// <para>Derives from TYPESYM. Has no own member.</para>
    /// </summary>
    //======================================================================
    internal class METHGRPSYM : TYPESYM
    {

#if DEBUG
        //------------------------------------------------------------
        // METHGRPSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class IMPLICITTYPESYM
    //
    /// <summary>
    /// <para>CS3</para>
    /// <para>Represents the implicit type specified by "var".</para>
    /// <para>Derives from TYPESYM. Has no own member.</para>
    /// </summary>
    //======================================================================
    internal class IMPLICITTYPESYM : TYPESYM
    {

#if DEBUG
        //------------------------------------------------------------
        // IMPLICITTYPESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class DYNAMICSYM
    //
    /// <summary>
    /// <para>CS4</para>
    /// <para>Represents the runtime-binding type specified by "dynamic".</para>
    /// <para>Derives from AGGTYPESYM. Has no own member.</para>
    /// </summary>
    //======================================================================
    internal class DYNAMICSYM : AGGTYPESYM
    {

#if DEBUG
        //------------------------------------------------------------
        // DYNAMICSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class METHPROPSYM
    //
    /// <summary>
    /// <para>METHPROPSYM - abstract class representing a method or a property. There
    /// are a bunch of algorithms in the compiler (e.g., override and overload resolution)
    /// that want to treat methods and properties the same. This abstract base class
    /// has the common parts. </para>
    /// <para>Changed to a PARENTSYM to allow generic methods to parent their type variables.</para>
    /// <para>Derives from PARENTSYM.</para>
    /// </summary>
    //======================================================================
    internal class METHPROPSYM : PARENTSYM
    {
        internal string deprecatedMessage = null;

        /// <summary>
        /// number of CMOD_OPTs in signature and return type
        /// </summary>
        internal int CModifierCount = 0;    // modOptCount

        /// <summary>
        /// params have been defined
        /// </summary>
        internal bool HasParamsDefined = false; // hasParamsDefined

        /// <summary>
        /// Static member?
        /// </summary>
        internal bool IsStatic = false; // isStatic

        /// <summary>
        /// Overrides an inherited member. Only valid if isVirtual is set.
        /// false implies that a new vtable slot is required for this method.
        /// </summary>
        new internal bool IsOverride = false;   // isOverride

        /// <summary>
        /// member is unsafe (either marked as such, or is in an unsafe context)
        /// </summary>
        new internal bool IsUnsafe = false; // isUnsafe

        /// <summary>
        /// Only valid iff isBogus == TRUE && IsPROPSYM.
        /// If this is true then tell the user to call the accessors directly.
        /// </summary>
        internal bool UseMethodInstead = false;    // useMethInstead

        /// <summary>
        /// a user defined operator (or default indexed property)
        /// </summary>
        internal bool IsOperator = false;   // isOperator

        /// <summary>
        /// <para>new style varargs</para>
        /// <para>Is params is used in arguments list.</para>
        /// </summary>
        internal bool IsParameterArray = false; // isParamArray

        /// <summary>
        /// this property hides all below it regardless of signature
        /// </summary>
        internal bool HideByName = false;   // isHideByName

        /// <summary>
        /// Needs a method impl to swtSlot.
        /// </summary>
        internal bool NeedsMethodImp = false;   // fNeedsMethodImp

        /// <summary>
        /// This indicates the base member that this member overrides or implements.
        /// </summary>
        /// <remarks>
        /// For an explicit interface member implementation,
        /// this is the interface member (and type) that the member implements.
        /// For an override member, this is the base member that is being overridden.
        /// This is not affected by implicit interface member implementation.
        /// If this symbol is a property and an explicit interface member implementation,
        /// the swtSlot may be an event.
        /// This is filled in during prepare.
        /// </remarks>
        internal SymWithType SlotSymWithType = new SymWithType();   // swtSlot

        /// <summary>
        /// If name == NULL but swtExpImpl couldn't be resolved, this contains error information.
        /// </summary>
        internal ERRORSYM ExpImplErrorSym = null;    // errExpImpl

        /// <summary>
        /// Meta-data token for imported method.
        /// </summary>
        internal int ImportedMethPropToken = 0; // tokenImport

        /// <summary>
        /// <para>Use this in place of tokenEmit in sscli.</para>
        /// </summary>
        internal MemberInfo ImportedMemberInfo = null;

        /// <summary>
        /// Metadata token (memberRef or memberDef) in the current output file.
        /// </summary>
        internal int EmittedMemberToken = 0;  // tokenEmit

        /// <summary>
        /// <para>Use this in place of tokenEmit in sscli.</para>
        /// </summary>
        internal MemberInfo EmittedMemberInfo = null;

        /// <summary>
        /// Return type.
        /// </summary>
        internal TYPESYM ReturnTypeSym = null;  // retType

        /// <summary>
        /// array of cParams parameter types.
        /// </summary>
        internal TypeArray ParameterTypes = null;   // params

        internal List<ParameterBuilder> PrameterBuilderList = null;

        /// <summary>
        /// Valid only between define &amp; prepare stages...
        /// </summary>
        internal BASENODE ParseTreeNode = null; // parseTree

        /// <summary>
        /// containing declaration (declaration in sscli)
        /// </summary>
        internal AGGDECLSYM ContainingAggDeclSym = null;    // declaration

        /// <summary>
        /// (R) Return ParentSym as AGGSYM.
        /// </summary>
        internal AGGSYM ParentAggSym
        {
            get { return ParentSym as AGGSYM; }
        }

        /// <summary>
        /// (R) Return ParentSym as AGGSYM, same to ParentAggSym.
        /// </summary>
        internal AGGSYM ClassSym   // getClass()
        {
            get { return ParentAggSym; }
        }

        /// <summary>
        /// <para>Explicit interface member implementations are recognized by having no name.</para>
        /// </summary>
        internal bool IsExplicitImplementation  // IsExpImpl()
        {
            get { return Name == null; }
        }

        new internal AGGDECLSYM ContainingDeclaration() // containingDeclaration()
        {
            return this.ContainingAggDeclSym;
        }

        //------------------------------------------------------------
        // METHPROPSYM.CopyInto
        //
        /// <summary></summary>
        /// <param name="mpsDst"></param>
        /// <param name="typeSrc"></param>
        /// <param name="compiler"></param>
        //------------------------------------------------------------
        internal void CopyInto(METHPROPSYM mpsDst, AGGTYPESYM typeSrc, COMPILER compiler)
        {
            base.copyInto(mpsDst);
            mpsDst.ContainingAggDeclSym = this.ContainingAggDeclSym;

            mpsDst.ClearChildList();

            mpsDst.IsStatic = this.IsStatic;
            mpsDst.ReturnTypeSym = typeSrc != null ?
                compiler.MainSymbolManager.SubstType(this.ReturnTypeSym, typeSrc, null) :
                this.ReturnTypeSym;
            mpsDst.ParameterTypes = typeSrc != null ?
                compiler.MainSymbolManager.SubstTypeArray(this.ParameterTypes, typeSrc, null) :
            this.ParameterTypes;
            mpsDst.IsParameterArray = this.IsParameterArray;
            mpsDst.CModifierCount = this.CModifierCount;
        }

#if DEBUG
        //------------------------------------------------------------
        // METHPROPSYM Debug
        //------------------------------------------------------------
        internal void DebugMethProp(StringBuilder sb)
        {
            //sb.Append("\n");
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugMethProp(sb);
            DebugPostfix(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class AnonScopeInfo
    //
    /// <summary>
    /// At most one of these per SCOPESYM.
    /// The lifetime of this is limited to the rewrite phase.
    /// During this time the AnonMethInfo list is stable.
    /// </summary>
    //======================================================================
    internal class AnonScopeInfo
    {
        internal AnonMethInfo FirstAnonyousMethodInfo = null;   // * pamiFirst

        /// <summary>
        /// The compiler generated class (for locals and instance methods)
        /// </summary>
        internal AGGSYM HoistedAggSym = null;   // * aggHoist

        /// <summary>
        /// a pointer to the "$local" local for this scope
        /// </summary>
        internal EXPRLOCAL LocalExpr = null;    // * exprLoc
    }

    //======================================================================
    // enum MethodKindEnum
    //
    /// <summary>
    /// (CSharp\SSComp\Symbol.cs)
    /// </summary>
    //======================================================================
    internal enum MethodKindEnum : int
    {
        None = 0,

        /// <summary>
        /// Ctor or static ctor
        /// </summary>
        Ctor = 1,
        
        Dtor = 2,
        PropAccessor = 3,
        EventAccessor = 4,

        /// <summary>
        /// Explicit user defined conversion
        /// </summary>
        ExplicitConv = 5,

        /// <summary>
        /// Implicit user defined conversion
        /// </summary>
        ImplicitConv = 6,
        
        Anonymous = 7,

        /// <summary>
        /// Invoke method of a delegate type
        /// </summary>
        Invoke = 8,
    }

    //======================================================================
    // enum AccessorKindEnum
    //
    /// <summary>
    /// (CS3) For auto-implemented accessors.
    /// </summary>
    //======================================================================
    internal enum AccessorKindEnum : int
    {
        None = 0,
        Get,
        Set,
    }

    //======================================================================
    // class METHSYM
    //
    /// <summary>
    /// <para>METHSYM - a symbol representing a method.
    /// Parent is a struct, interface or class (aggregate). No children.</para>
    /// <para>Derives from METHPROPSYM.</para>
    /// </summary>
    //======================================================================
    internal class METHSYM : METHPROPSYM
    {
        //------------------------------------------------------------
        // enum CanInferStateEnum
        //------------------------------------------------------------
        internal enum CanInferStateEnum : int
        {
            /// <summary>
            /// Set to cisMaybe if we haven't set this yet.
            /// </summary>
            Maybe = 0,  // METHSYM::cisMaybe

            /// <summary>
            /// Set to cisYes if all type variables are used in the method signature
            /// so inferencing might work.
            /// </summary>
            Yes = 1,    // METHSYM::cisYes

            /// <summary>
            /// Set to cisNo if either it has no type variables or
            /// not all type variables are used so inferencing will never work.
            /// </summary>
            No = 2  // METHSYM::cisNo
        }
        // We use three bits for this so we don't have sign extension issues.

        //------------------------------------------------------------
        // METHSYM Fields and Properties (1)
        // MethoInfo, MethodBuilder, ConstructorInfo, ConstructorBuilder
        //------------------------------------------------------------

        private MethodInfo methodInfo = null;
        private MethodBuilder methodBuilder = null;
        private ConstructorInfo constructorInfo = null;
        private ConstructorBuilder constructorBuilder = null;

        internal MethodInfo MethodInfo
        {
            get
            {
                return this.methodInfo;
            }
            set
            {
                DebugUtil.Assert(!this.IsCtor);
                DebugUtil.Assert(this.methodInfo == null && this.constructorInfo == null);

                this.methodInfo = value;
                this.methodBuilder = null;
            }
        }

        internal MethodBuilder MethodBuilder
        {
            get
            {
                return this.methodBuilder;
            }
            set
            {
                DebugUtil.Assert(!this.IsCtor);
                DebugUtil.Assert(this.methodInfo == null && this.constructorInfo == null);

                this.methodBuilder = value;
                this.methodInfo = value;
            }
        }

        internal ConstructorInfo ConstructorInfo
        {
            get
            {
                return this.constructorInfo;
            }
            set
            {
                DebugUtil.Assert(this.methodInfo == null && this.constructorInfo == null);

                this.constructorInfo = value;
                this.constructorBuilder = null;
            }
        }

        internal ConstructorBuilder ConstructorBuilder
        {
            get
            {
                return this.constructorBuilder;
            }
            set
            {
                DebugUtil.Assert(this.methodInfo == null && this.constructorInfo == null);

                this.constructorBuilder = value;
                this.constructorInfo = value;
            }
        }

        internal bool HasMethodInfo
        {
            get { return (this.methodInfo != null); }
        }

        internal bool HasMethodBuilder
        {
            get { return (this.methodBuilder != null); }
        }

        //------------------------------------------------------------
        // METHSYM Fields and Properties (2)
        //------------------------------------------------------------

        /// <summary>
        /// Has external definition.
        /// </summary>
        internal bool IsExternal = false;   // uint isExternal : 1;

        /// <summary>
        /// Has definition implemented by the runtime.
        /// </summary>
        internal bool IsSysNative = false;  // uint isSysNative :1;

        /// <summary>
        /// Virtual member?
        /// </summary>
        new internal bool IsVirtual = false;    // uint isVirtual: 1;

        /// <summary>
        /// Marked as virtual in the metadata
        /// (if mdVirtual + mdSealed, this will be true, but isVirtual will be false).
        /// </summary>
        internal bool IsMetadataVirtual = false;    // uint isMetadataVirtual: 1;

        /// <summary>
        /// Abstract method?
        /// </summary>
        internal bool IsAbstract = false;   // uint isAbstract: 1;

        /// <summary>
        /// is really a IFACEIMPLMETHSYM
        /// </summary>
        internal bool IsInterfaceImpl = false;  // uint isIfaceImpl: 1;

        /// <summary>
        /// conditionalSymbols already includes parent symbols if override
        /// </summary>
        internal bool CheckedConditionalSymbols = false;    // uint checkedCondSymbols: 1;

        /// <summary>
        /// has a security attribute of type link demand
        /// </summary>
        internal bool HasLinkDemand = false;    // uint hasLinkDemand: 1;

        /// <summary>
        /// has varargs
        /// </summary>
        internal bool IsVarargs = false;    // uint isVarargs: 1;

        /// <summary>
        /// had or needs mdNewSlot bit set
        /// </summary>
        internal bool IsNewSlot = false;    // uint isNewSlot: 1;

#if USAGEHACK
    internal bool isUsed;   // uint isUsed: 1;
#endif

        /// <summary>
        /// An extra bit to prevent sign-extension
        /// </summary>
        private MethodKindEnum methodKind = MethodKindEnum.None;  // uint methKind : 5;

        /// <summary>
        /// (RW) An extra bit to prevent sign-extension
        /// </summary>
        internal MethodKindEnum MethodKind
        {
            get // MethodKindEnum MethKind() const
            {
                return methodKind;
            }
            set // void SetMethKind(MethodKindEnum mk)
            {
                DebugUtil.Assert(methodKind == MethodKindEnum.None || methodKind == value);
                methodKind = value;
            }
        }

        //------------------------------------------------------------
        // METHSYM Fields and Properties (3)
        //------------------------------------------------------------

        internal CanInferStateEnum CanInferState;   // uint cisCanInfer: 3;

        /// <summary>
        /// All the type variables for a generic method, as declarations.
        /// </summary>
        internal TypeArray TypeVariables;   // typeVars

        /// <summary>
        /// The tokens of the emitted type variables.
        /// </summary>
        internal uint EmittedTypeVariablestoken = 0;    // mdToken * toksEmitTypeVars;

        /// <summary>
        /// <para>Use in place of toksEmitTypeVars in sscli.</para>
        /// </summary>
        //internal GenericTypeParameterBuilder[] TypeParameterBuilders = null;
        internal Type[] GenericParameterTypes = null;

        /// <summary>
        /// set if a conditional symbols for method
        /// </summary>
        internal List<string> ConditionalSymbolNameList = null; // conditionalSymbols

        //------------------------------------------------------------
        // METHSYM Fields and Properties (4)
        //------------------------------------------------------------

        private object obj;

        /// <summary>
        /// For linked list of conversion operators.
        /// </summary>
        internal METHSYM NextConvertMethSym
        {
            get // METHSYM * ConvNext()
            {
                DebugUtil.Assert(IsImplicit || IsExplicit);
                return obj != null ? obj as METHSYM : null;
            }
            set // void SetConvNext(METHSYM * conv)
            {
#if DEBUG
                DebugUtil.Assert(IsImplicit || IsExplicit);
                if (value != null)
                {
                    METHSYM methSym = value as METHSYM;
                    DebugUtil.Assert(methSym != null && (methSym.IsImplicit || methSym.IsExplicit));
                }
#endif
                obj = value;
            }
        }

        /// <summary>
        /// (RW) For property accessors, this is the PROPSYM.
        /// </summary>
        internal PROPSYM PropertySym
        {
            get // PROPSYM *getProperty()
            {
                DebugUtil.Assert(IsPropertyAccessor);
                return obj != null ? obj as PROPSYM : null;
            }
            set // void SetProperty(PROPSYM * prop)
            {
                DebugUtil.Assert(IsPropertyAccessor);
                obj = value;
            }
        }

        /// <summary>
        /// (RW) For event accessors, this is the EVENTSYM.
        /// </summary>
        internal EVENTSYM EventSym
        {
            get // EVENTSYM *getEvent()
            {
                DebugUtil.Assert(IsEventAccessor);
                return obj != null ? obj as EVENTSYM : null;
            }
            set // void SetEvent(EVENTSYM * evt)
            {
                DebugUtil.Assert(IsEventAccessor);
                obj = value;
            }
        }

        /// <summary>
        /// (CS3) For auto-implemented accessors.
        /// </summary>
        internal AccessorKindEnum AccessorKind = AccessorKindEnum.None;

        internal bool IsAutoImplementedAccessor
        {
            get
            {
                return (this.PropertySym != null &&
                    this.PropertySym.BackFieldSym != null);
            }
        }

        //------------------------------------------------------------
        // METHSYM Fields and Properties (5) Is*
        //------------------------------------------------------------

        /// <summary>
        /// (R) Is a constructor or static constructor (depending on isStatic).
        /// </summary>
        internal bool IsCtor
        {
            get { return methodKind == MethodKindEnum.Ctor; } // bool isCtor() const
        }

        /// <summary>
        /// (R) Is a destructor
        /// </summary>
        internal bool IsDtor
        {
            get { return methodKind == MethodKindEnum.Dtor; } // bool isDtor() const
        }

        /// <summary>
        /// (R) true if this method is a property set or get method
        /// </summary>
        internal bool IsPropertyAccessor
        {
            get { return methodKind == MethodKindEnum.PropAccessor; } // bool isPropertyAccessor() const
        }

        /// <summary>
        /// (R) true if this method is an event add/remove method 
        /// </summary>
        internal bool IsEventAccessor
        {
            get { return methodKind == MethodKindEnum.EventAccessor; }    // bool isEventAccessor() const
        }

        /// <summary>
        /// (R) is user defined explicit conversion operator
        /// </summary>
        internal bool IsExplicit
        {
            get { return methodKind == MethodKindEnum.ExplicitConv; } // bool isExplicit() const
        }

        /// <summary>
        /// (R) is user defined implicit conversion operator
        /// </summary>
        internal bool IsImplicit
        {
            get { return methodKind == MethodKindEnum.ImplicitConv; } // bool isImplicit() const
        }

        /// <summary>
        /// (R) is an Anonymous Method
        /// </summary>
        internal bool IsAnonymous
        {
            get { return methodKind == MethodKindEnum.Anonymous; }    // bool isAnonymous() const
        }

        /// <summary>
        /// (R) Invoke method on a delegate - isn't user callable
        /// </summary>
        internal bool IsInvoke
        {
            get { return methodKind == MethodKindEnum.Invoke; }   // bool isInvoke() const
        }

        /// <summary>
        /// (R) is this a compiler generated constructor
        /// </summary>
        internal bool IsCompilerGeneratedCtor
        {
            get // isCompilerGeneratedCtor()
            {
                BASENODE parseTree = GetParseTree();
                if (parseTree != null)
                    return (
                        this.IsCtor &&
                        (parseTree.Kind == NODEKIND.CLASS || parseTree.Kind == NODEKIND.STRUCT));
                else
                    return false;
            }
        }

        /// <summary>
        /// (R) returns true if this property is a get accessor. Only valid if isPropertyAcessor is true.
        /// </summary>
        internal bool IsGetAccessor
        {
            get // isGetAccessor()
            {
                DebugUtil.Assert(this.IsPropertyAccessor);

                PROPSYM property = this.PropertySym;
                if (property != null)
                {
                    return (this == property.GetMethodSym);
                }
                DebugUtil.Assert(false, "cannot find property for accessor");
                return false;
            }
        }

        /// <summary>
        /// (R) True if this.IsExplicit or this.IsImplicit is true.
        /// </summary>
        internal bool IsConversionOperator
        {
            get { return (IsExplicit || IsImplicit); }  // isConversionOperator()
        }

        /// <summary>
        /// (R) True if this method is an accessor of a property or an event.
        /// </summary>
        internal bool IsAnyAccessor
        {
            get { return IsPropertyAccessor || IsEventAccessor; }   // isAnyAccessor()
        }

        //------------------------------------------------------------
        // METHSYM Fields and Properties (6) Extension Method
        //------------------------------------------------------------

        /// <summary>
        /// <para>(CS3)</para>
        /// </summary>
        internal METHSYM StaticExtensionMethodSym = null;

        /// <summary>
        /// <para>(CS3) (R)</para>
        /// </summary>
        internal bool IsInstanceExtensionMethod
        {
            get { return (StaticExtensionMethodSym != null); }
        }

        //------------------------------------------------------------
        // METHSYM Fields and Properties (7) Partial Method
        //------------------------------------------------------------

        /// <summary>
        /// (CS3)
        /// </summary>
        internal bool IsPartialMethod = false;

        /// <summary>
        /// (CS3) True if this is a partioal method and has no method body.
        /// </summary>
        internal bool HasNoBody = false;

        /// <summary>
        /// (CS3) if this sym of the partial method has no method body,
        /// set the implementing sym to this field.
        /// </summary>
        internal METHSYM PartialMethodImplSym = null;

        //------------------------------------------------------------
        // METHSYM.IsUserCallable
        //
        /// <summary>
        /// (R) True if this method is not an operator or an accessor of a property or an event.
        /// </summary>
        //------------------------------------------------------------
        new internal bool IsUserCallable()
        {
            return (!IsOperator && !IsAnyAccessor); // isUserCallable()
        }

        //------------------------------------------------------------
        // METHSYM.asIFACEIMPLMETHSYM
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal IFACEIMPLMETHSYM AsIFACEIMPLMETHSYM()
        {
            DebugUtil.Assert(IsInterfaceImpl);

            if (!this.IsInterfaceImpl)
            {
                return null;
            }
            return (IFACEIMPLMETHSYM)this;
        }

        //------------------------------------------------------------
        // METHSYM.CopyInto
        //
        /// <summary></summary>
        /// <param name="methDst"></param>
        /// <param name="typeSrc"></param>
        /// <param name="compiler"></param>
        //------------------------------------------------------------
        internal void CopyInto(METHSYM methDst, AGGTYPESYM typeSrc, COMPILER compiler)
        {
            base.CopyInto(methDst, typeSrc, compiler);

            methDst.methodInfo = this.methodBuilder;
            methDst.methodBuilder = this.methodBuilder;
            methDst.constructorInfo = this.constructorInfo;
            methDst.constructorBuilder = this.constructorBuilder;

            methDst.IsVirtual = this.IsVirtual;
            methDst.IsMetadataVirtual = this.IsMetadataVirtual;
            methDst.IsAbstract = this.IsAbstract;
            methDst.IsVarargs = this.IsVarargs;
            methDst.methodKind = (methodKind == MethodKindEnum.Ctor ? MethodKindEnum.Ctor : MethodKindEnum.None);
            methDst.TypeVariables = this.TypeVariables;
        }

        //------------------------------------------------------------
        // METHSYM.GetAttributesNode
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal BASENODE GetAttributesNode()
        {
            BASENODE tree = GetParseTree();
            if (tree == null) return null;

            switch (tree.Kind)
            {
                case NODEKIND.METHOD:
                case NODEKIND.CTOR:
                case NODEKIND.OPERATOR:
                case NODEKIND.DTOR:
                    return tree.AsANYMETHOD.AttributesNode;

                case NODEKIND.ACCESSOR:
                    DebugUtil.Assert(this.IsAnyAccessor);
                    return (tree as ACCESSORNODE).AttributesNode;

                case NODEKIND.VARDECL:
                    // simple event declaration auto-generated accessors.
                    DebugUtil.Assert(this.IsEventAccessor);
                    BASENODE fieldTree = (ParseTreeNode as VARDECLNODE).ParentNode;
                    while (fieldTree.Kind == NODEKIND.LIST)
                    {
                        fieldTree = fieldTree.ParentNode;
                    }
                    return fieldTree.asANYFIELD().AttributesNode;

                case NODEKIND.INDEXER:
                case NODEKIND.PROPERTY:
                    // This only happens when there is no parse tree for an event accessor (error case),
                    // or when a property is a sealed override but only implements one accessor.
                    // In this case we create the accessor anyway and give it the property parse tree.
                    // Just ignore any attributes on the property.
                    DebugUtil.Assert(this.IsAnyAccessor && this.IsFabricated);
                    return null;

                default:
                    DebugUtil.Assert(
                        this.IsCompilerGeneratedCtor ||
                        this.IsInterfaceImpl ||
                        this.ClassSym.IsDelegate ||
                        this.IsAnonymous);
                    return null;
            }
        }

        //------------------------------------------------------------
        // METHSYM SetCustomAttribue
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetCustomAttribute(CustomAttributeBuilder caBuilder, out Exception excp)
        {
            excp = null;
            if (caBuilder == null)
            {
                return false;
            }

            try
            {
                if (this.methodBuilder != null)
                {
                    this.methodBuilder.SetCustomAttribute(caBuilder);
                    return true;
                }
                else if (this.constructorBuilder != null)
                {
                    this.constructorBuilder.SetCustomAttribute(caBuilder);
                    return true;
                }
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            return false;
        }

#if DEBUG
        //------------------------------------------------------------
        // METHSYM Debug
        //------------------------------------------------------------
        internal void DebugMeth(StringBuilder sb)
        {
            if (this.MethodBuilder != null)
            {
                sb.AppendFormat("MethodBuilder : {0}\n", this.MethodBuilder.Name);
            }
            else if (this.MethodInfo != null)
            {
                sb.AppendFormat("MethodInfo    : {0}\n", this.MethodInfo.Name);
            }
            else
            {
                sb.Append("MethodInfo    :\n");
            }
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugMeth(sb);
            DebugMethProp(sb);
            DebugPostfix(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class FAKEMETHSYM
    //
    /// <summary>
    /// <para>Used for varargs.</para>
    /// <para>Derives from METHSYM.</para>
    /// </summary>
    //======================================================================
    internal class FAKEMETHSYM : METHSYM
    {
        internal METHSYM ParentMethodSym; // * parentMethSym;

#if DEBUG
        //------------------------------------------------------------
        // FAKEMETHSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class IFACEIMPLMETHSYM
    //
    /// <summary>
    /// <para>an explicit method impl generated by the compiler
    /// usef for CMOD_OPT interop</para>
    /// <para>Derives from METHSYM.</para>
    /// </summary>
    //======================================================================
    internal class IFACEIMPLMETHSYM : METHSYM
    {
        internal METHSYM ImplMethSym = null;    // * implMethod

#if DEBUG
        //------------------------------------------------------------
        // IFACEIMPLMETHSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class PARAMINFO
    //
    /// <summary>
    /// <para>PARAMINFO - Additional information about a parameter symbol
    /// that isn't needed by other code binding against this method.</para>
    /// <para>This structure lives only when this particular method is being compiled.</para>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal class PARAMINFO
    {
        internal string Name = null;                    // NAME * name;

        /// <summary>
        /// <remarks>(2016/02/05 hirano567@hotmail.co.jp)</remarks>
        /// </summary>
        internal TYPESYM TypeSym = null;

        /// <summary>
        /// Corresponding parse tree node if there is one.
        /// </summary>
        internal PARAMETERNODE ParameterNode = null;    // PARAMETERNODE * node;

        internal BASENODE AttrBaseNode = null;          // BASENODE * nodeAttr;

        /// <summary>
        /// COM classic attributes
        /// </summary>
        internal bool IsIn = false;                     // bool isIn:1;

        internal bool IsOut = false;                    // bool isOut:1;

        internal bool IsParametersArray = false;        // bool isParamArray:1;

        /// <summary>
        /// (CS3) This parameter represents a direct product of parameters.
        /// </summary>
        internal bool IsParameterProcuct   // 2016/02/22 hirano567@hotmailc.co.jp
        {
            get
            {
                AGGTYPESYM ats = this.TypeSym as AGGTYPESYM;
                return (ats != null) ? ats.IsParameterProcuct : false;
            }
        }

        internal ParameterBuilder ParameterBuilder = null;

        //------------------------------------------------------------
        // PARAMINFO.GetParameterAttributes
        //------------------------------------------------------------
        internal ParameterAttributes GetParameterAttributes()
        {
            ParameterAttributes attr = ParameterAttributes.None;
            if (this.IsIn)
            {
                attr |= ParameterAttributes.In;
            }
            if (this.IsOut)
            {
                attr |= ParameterAttributes.Out;
            }
            return attr;
        }
    }

    //======================================================================
    // class IterInfo
    //
    /// <summary>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal class IterInfo
    {
        /// <summary>
        /// This is the compiler generated class that implements everything
        /// </summary>
        internal AGGSYM IteratorAggSym = null;  // * aggIter

        /// <summary>
        /// This is where we hang the dispose method body statements
        /// </summary>
        internal EXPRSTMT DisposeMethodBodyExpr = null; // * disposeBody

        /// <summary>
        /// Does the class implement the generic interfaces
        /// </summary>
        internal bool IsGeneric = false;    // fGeneric

        /// <summary>
        /// Does the class implement the IEnumerable pattern or just the IEnumerator pattern
        /// </summary>
        internal bool IsEnumerable = false; // fEnumerable
    }

    //======================================================================
    // class METHINFO
    //
    /// <summary>
    /// <para>METHINFO - Additional information about an method symbol
    /// that isn't needed by other code binding against this method.</para>
    /// <para>This structure lives only when this particular method is being compiled.</para>
    /// <para>(Defined in CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal class METHINFO
    {
        internal METHSYM MethodSym = null;    // meth

        /// <summary>
        /// The arg scope of this method, if any...
        /// </summary>
        internal SCOPESYM OuterScopeSym = null; // outerScope

        /// <summary>
        /// This is shared among multiple METHINFOs - hence the indirection.
        /// </summary>
        internal IterInfo IteratorInfo = null;  // piin

        internal TYPESYM YieldTypeSym = null;   // yieldType

        /// <summary>
        /// The forest of anonymous method info structs.
        /// </summary>
        internal AnonMethInfo FirstAnonymousMethodInfo = null;  // pamiFirst

        /// <summary>
        /// First return node found in the method.
        /// </summary>
        internal EXPRSTMTNODE FirstReturnNode = null;   // nodeRet

        /// <summary>
        /// This is a "magic" method with run-time supplied implementation:
        /// </summary>
        internal bool IsMagicImpl = false;  // isMagicImpl
        // e.g.: delegate Invoke or delegate ctor.

        /// <summary>
        /// has a return inside of a try or catch
        /// </summary>
        internal bool HasReturnAsLeave = false; // hasRetAsLeave

        /// <summary>
        /// has a yield inside a try/finally
        /// </summary>
        internal bool HasYieldAsLeave = false;  // hasYieldAsLeave

        /// <summary>
        /// Don't generate debug information. Used for compiler-created methods.
        /// </summary>
        internal bool NoDebugInfo = false;  // noDebugInfo

        /// <summary>
        /// Should emit DebuggerHiddenAttribute (should also set noDebugInfo)
        /// </summary>
        internal bool EmitDebuggerHiddenAttribute = false;  // debuggerHidden

        /// <summary>
        /// synchronized bit (only used for event accessor's; not settable from code).
        /// </summary>
        internal bool IsSynchronized = false;   // isSynchronized

        /// <summary>
        /// First occurance of unsafe type or code block
        /// </summary>
        internal BASENODE UnsafeTreeNode = null;    // unsafeTree

        /// <summary>
        /// The parse tree for the params (if there is one).
        /// This may not cover all of the params
        /// (e.g. for a set accessor or a BeginInvoke method on a delegate type),
        /// or it may contain some non-params (e.g. for an EndInvoke).
        /// </summary>
        internal BASENODE ParametersNode = null;    // nodeParams

        /// <summary>
        /// The attributes for the "method". This may come from a non-method node (eg delegate).
        /// </summary>
        internal BASENODE AttributesNode = null;    // nodeAttr

        internal PARAMINFO ReturnValueInfo = new PARAMINFO();   // returnValueInfo

        /// <summary>
        /// <para>Must be last!</para>
        /// </summary>
        internal List<PARAMINFO> ParameterInfos = new List<PARAMINFO>();    // PARAMINFO rgpin[1];

        /// <summary>
        /// number of PARAMINFOs. If meth is a old-style varargs, this will be one less than meth.params.size.
        /// </summary>
        //internal int ParameterInfoCount = 0;   // cpin
        internal int ParameterInfoCount
        {
            get { return ParameterInfos.Count; }
        }

        internal bool IsIterator
        {
            get { return YieldTypeSym != null; }
        }

        //------------------------------------------------------------
        // METHINFO.Size
        //
        /// <summary>
        /// <para>Typically a METHINFO is allocated with a STACK_ALLOC_ZERO(byte, Size(params.size)).</para>
        /// </summary>
        /// <param name="cpin"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal int Size(int cpin)
        {
            throw new System.NotImplementedException("METHINFO.Size");
            //return sizeof(METHINFO) + sizeof(PARAMINFO) * (cpin - 1);
        }

        //------------------------------------------------------------
        // METHINFO.InitFromIterInfo
        //
        /// <summary></summary>
        /// <param name="srcInfo"></param>
        /// <param name="srcMeth"></param>
        /// <param name="cpinMax"></param>
        //------------------------------------------------------------
        internal void InitFromIterInfo(METHINFO srcInfo, METHSYM srcMeth, int cpinMax)
        {
            //memset(this, 0, Size(cpinMax));
            this.OuterScopeSym = srcInfo.OuterScopeSym;
            this.IteratorInfo = srcInfo.IteratorInfo;
            this.HasYieldAsLeave = srcInfo.HasYieldAsLeave;
            this.YieldTypeSym = srcInfo.YieldTypeSym;
            this.MethodSym = srcMeth;
        }

        //------------------------------------------------------------
        // METHINFO.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            this.MethodSym = null;
            this.OuterScopeSym = null;
            this.IteratorInfo = null;
            this.YieldTypeSym = null;
            this.FirstAnonymousMethodInfo = null;
            this.FirstReturnNode = null;
            this.IsMagicImpl = false;
            this.HasReturnAsLeave = false;
            this.HasYieldAsLeave = false;
            this.NoDebugInfo = false;
            this.EmitDebuggerHiddenAttribute = false;
            this.IsSynchronized = false;
            this.UnsafeTreeNode = null;
            this.ParametersNode = null;
            this.AttributesNode = null;
            this.ParameterInfos.Clear();
        }
    }

    //======================================================================
    // class PROPSYM
    //
    /// <summary>
    /// <para>PROPSYM - a symbol representing a property.
    /// Parent is a struct, interface or class (aggregate).
    /// No children.</para>
    /// <para>Derives from METHPROPSYM.</para>
    /// </summary>
    //======================================================================
    internal class PROPSYM : METHPROPSYM
    {
        //------------------------------------------------------------
        // PROPSYM Fields and Properties
        //------------------------------------------------------------

        private PropertyInfo propertyInfo = null;
        private PropertyBuilder propertyBuilder = null;

        internal PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
            set
            {
                DebugUtil.Assert(this.propertyInfo == null);

                this.propertyInfo = value;
                this.propertyBuilder = null;
            }
        }

        internal PropertyBuilder PropertyBuilder
        {
            get { return this.propertyBuilder; }
            set
            {
                DebugUtil.Assert(this.propertyInfo == null);

                this.propertyBuilder = value;
                this.propertyInfo = value;
            }
        }

        //private PropertyInfo importedPropertyInfo = null;

        //internal PropertyInfo ImportedPropertyInfo
        //{
        //    get { return this.importedPropertyInfo; }
        //    set
        //    {
        //        DebugUtil.Assert(this.propertyInfo == null);
        //        this.importedPropertyInfo = value;
        //        this.propertyInfo = value;
        //    }
        //}

        //private PropertyBuilder emittedPropertyBuilder = null;

        //internal PropertyBuilder EmittedPropertyBuilder
        //{
        //    get { return this.emittedPropertyBuilder; }
        //    set
        //    {
        //        DebugUtil.Assert(this.propertyInfo == null);
        //        this.emittedPropertyBuilder = value;
        //        this.propertyInfo = null;
        //    }
        //}

        //private PropertyInfo propertyInfo = null;

        //internal PropertyInfo PropertyInfo
        //{
        //    get { return this.propertyInfo; }
        //}

        /// <summary>
        /// This field is the implementation for an event.
        /// </summary>
        internal bool IsEvent = false;  // bool isEvent : 1;

        /// <summary>
        /// There was an error binding parameter attributes on the first accessor.  
        /// If this flag is set we won't attempt to bind attributes on the second accessor.
        /// </summary>
        new internal bool HadAttributeError = false;    // bool fHadAttributeError : 1;

        /// <summary>
        /// Getter method (always has same parent)
        /// </summary>
        internal METHSYM GetMethodSym = null;   // methGet

        /// <summary>
        /// Setter method (always has same parent)
        /// </summary>
        internal METHSYM SetMethodSym = null;   // methSet

        internal bool IsIndexer // isIndexer()
        {
            get { return IsOperator; }
        }

        internal INDEXERSYM AsINDEXERSYM    // asINDEXERSYM()
        {
            get
            {
                DebugUtil.Assert(IsIndexer);
                return (this as INDEXERSYM);
            }
        }

        /// <summary>
        /// (CS3) For auto-implemented properties.
        /// </summary>
        internal MEMBVARSYM BackFieldSym = null;

        //------------------------------------------------------------
        // PROPSYM.GetRealName
        //
        /// <summary>
        /// If this is indexer, return INDEXERSYM.RealName.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetRealName()
        {
            if (IsIndexer)
            {
                return this.AsINDEXERSYM.RealName;
            }
            else
            {
                return Name;
            }
        }

        //------------------------------------------------------------
        // PROPSYM.CopyInto
        //
        /// <summary></summary>
        /// <param name="propDst"></param>
        /// <param name="typeSrc"></param>
        /// <param name="compiler"></param>
        //------------------------------------------------------------
        internal void CopyInto(PROPSYM propDst, AGGTYPESYM typeSrc, COMPILER compiler)
        {
            base.CopyInto(propDst, typeSrc, compiler);
        }

        //------------------------------------------------------------
        // PROPSYM.GetEvent
        //
        /// <summary>
        /// returns event. Only valid to call if isEvent is true
        /// </summary>
        /// <param name="symmgr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EVENTSYM GetEvent(BSYMMGR symmgr)
        {
            DebugUtil.Assert(IsEvent);

            SYM sym = null;
            EVENTSYM eventSym = null;
            sym = symmgr.LookupAggMember(this.Name, this.ClassSym, SYMBMASK.EVENTSYM);
            if (sym != null)
            {
                eventSym = sym as EVENTSYM;
            }
            if (eventSym != null && eventSym.EventImplementSym == this)
            {
                return eventSym;
            }
            return null;
        }

        //------------------------------------------------------------
        // PROPSYM.GetAttributesNode
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal BASENODE GetAttributesNode()
        {
            BASENODE parseTree = GetParseTree();
            if (parseTree != null)
            {
                PROPERTYNODE node = parseTree.AsANYPROPERTY;
                return (node != null ? node.AttributesNode : null);
            }
            return null;
        }

        //------------------------------------------------------------
        // PROPSYM.SetCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetCustomAttribute(CustomAttributeBuilder caBuilder, out Exception excp)
        {
            excp = null;

            if (this.propertyBuilder != null && caBuilder != null)
            {
                try
                {
                    this.propertyBuilder.SetCustomAttribute(caBuilder);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    excp = ex;
                }
                catch (InvalidOperationException ex)
                {
                    excp = ex;
                }
            }
            return false;
        }

#if DEBUG
        //------------------------------------------------------------
        // PROPSYM Debug
        //------------------------------------------------------------
        internal void DebugProp(StringBuilder sb)
        {
            if (this.PropertyBuilder != null)
            {
                sb.AppendFormat("PropertyBuilder : {0}\n", this.PropertyBuilder.Name);
            }
            else if (this.PropertyInfo != null)
            {
                sb.AppendFormat("PropertyInfo    : {0}\n", this.PropertyInfo.Name);
            }
            else
            {
                sb.Append("PropertyInfo    :\n");
            }

            if (GetMethodSym != null)
            {
                sb.AppendFormat("GetMethodSym    : No.{0}\n", GetMethodSym.SymID);
            }
            else
            {
                sb.Append("GetMethodSym    :\n");
            }

            if (SetMethodSym != null)
            {
                sb.AppendFormat("SetMethodSym    : No.{0}\n", SetMethodSym.SymID);
            }
            else
            {
                sb.Append("SetMethodSym    :\n");
            }
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugProp(sb);
            DebugMethProp(sb);
            DebugPostfix(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class INDEXERSYM
    //
    /// <summary>
    /// <para>INDEXERSYM - a symbol representing an indexed property.
    /// Parent is a struct, interface or class (aggregate). No children.</para>
    /// <para>Derives from PROPSYM. Has RealName field.</para>
    /// </summary>
    /// <remarks>
    /// Has kind == SK_PROPSYM.
    /// </remarks>
    //======================================================================
    internal class INDEXERSYM : PROPSYM
    {
        /// <summary>
        /// the 'real' name of the indexer. All indexers have the same name.
        /// </summary>
        internal string RealName;   // realName

#if DEBUG
        //------------------------------------------------------------
        // INDEXERSYM Debug
        //------------------------------------------------------------
        internal void DebugIndexer(StringBuilder sb)
        {
            sb.AppendFormat("RealName        : No.{0}\n", RealName);
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugIndexer(sb);
            DebugProp(sb);
            DebugMethProp(sb);
            DebugPostfix(sb);
            DebugParent(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class PROPINFO
    //
    /// <summary>
    /// PROPINFO - Additional information about an property symbol
    /// that isn't needed by other code binding against this method.
    /// This structure lives only when this particular method is being compiled.
    /// </summary>
    //======================================================================
    internal class PROPINFO
    {
        /// <summary>
        /// Parameter name info (has PROPSYM::cParams elements)
        /// </summary>
        internal PARAMINFO[] ParmeterInfos = null;
    }

    //======================================================================
    // class VARSYM
    //
    /// <summary>
    /// VARSYM - a symbol representing a variable. Specific subclasses are used
    /// - MEMBVARSYM for member variables, LOCVARSYM for local variables and formal parameters, 
    /// </summary>
    //======================================================================
    internal class VARSYM : SYM
    {
        /// <summary>
        /// Type of the field.
        /// </summary>
        internal TYPESYM TypeSym = null;    // type

#if DEBUG
        //------------------------------------------------------------
        // VARSYM Debug
        //------------------------------------------------------------
        internal void DebugVar(StringBuilder sb)
        {
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugVar(sb);
            DebugPostfix(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class MEMBVARSYM
    //
    /// <summary>
    /// <para>MEMBVARSYM - a symbol representing a member variable of a class.
    /// Parent is a struct or class.</para>
    /// <para>Derives from VARSYM.</para>
    /// </summary>
    //======================================================================
    internal class MEMBVARSYM : VARSYM
    {
        private FieldInfo fieldInfo = null;
        private FieldBuilder fieldBuilder = null;

        internal FieldInfo FieldInfo
        {
            get { return this.fieldInfo; }
            set
            {
                DebugUtil.Assert(this.fieldInfo == null);

                this.fieldInfo = value;
                this.fieldBuilder = null;
            }
        }

        internal FieldBuilder FieldBuilder
        {
            get { return this.fieldBuilder; }
            set
            {
                //DebugUtil.Assert(this.fieldInfo == null);
                this.fieldBuilder = value;
                this.fieldInfo = value;
            }
        }

        internal string deprecatedMessage = null;

        /// <summary>
        /// Static member?
        /// </summary>
        internal bool IsStatic = false; // isStatic

        /// <summary>
        /// Is a compile-time constant; see constVal for value.
        /// </summary>
        internal bool IsConst = false;  // isConst

        /// <summary>
        /// Can only be changed from within constructor.
        /// </summary>
        internal bool IsReadOnly = false;   // isReadOnly

        /// <summary>
        /// This field is the implementation for an event.
        /// </summary>
        internal bool IsEvent = false;  // isEvent

        /// <summary>
        /// This fields is marked volatile
        /// </summary>
        internal bool IsVolatile = false;   // isVolatile

        /// <summary>
        /// This has an unevaluated constant value
        /// </summary>
        internal bool IsUnevaled = false;   // isUnevaled

        /// <summary>
        /// Has this been referenced by the user?
        /// </summary>
        internal bool IsReferenced = false; // isReferenced

        /// <summary>
        /// Has this ever been assigned by the user?
        /// </summary>
        internal bool IsAssigned = false;   // isAssigned

        /// <summary>
        /// member is unsafe (either marked as such, or is in an unsafe context)
        /// </summary>
        new internal bool IsUnsafe = false; // isUnsafe

        /// <summary>
        /// Member has 'original' copy of hoisted parameter
        /// </summary>
        internal bool IsHoistedParameter = false;   // isHoistedParameter

        /// <summary>
        /// Set if the field's ibit (for definite assignment checking) varies
        /// depending on the generic instantiation of the containing type.
        /// </summary>
        /// <remarks>
        /// For example:
        ///    struct S<T> { T x; int y; }
        /// The ibit value for y depends on what T is bound to. For S<Point>, y's ibit is 2. For S<int>, y's
        /// ibit is 1. This flag is set the first time a calculated ibit for the member is found to not
        /// match the return result of GetIbitInst().
        /// </remarks>
        internal bool IbitVaries;   // fIbitVaries

        /// <summary>
        /// If isConst is set, a constant value.
        /// If fixedAgg is non-NULL, the constant of the fixed buffer length
        /// </summary>
        internal CONSTVAL ConstVal = null;  // constVal

        /// <summary>
        /// parse tree, could be a VARDECLNODE or a ENUMMEMBRNODE
        /// </summary>
        internal BASENODE ParseTreeNode = null; // parseTree

        /// <summary>
        /// <para>Meta-data token for imported variable.</para>
        /// <para>Use FiledInfo to get matadata.</para>
        /// </summary>
        internal int ImportedFieldToken = -1;    // tokenImport

        /// <summary>
        /// Metadata token (memberRef or memberDef) in the current output file.
        /// </summary>
        internal int EmittedMemberToken = -1;   // tokenEmit

        /// <summary>
        /// index into iterator local array
        /// </summary>
        internal int LocalIteratorIndex = -1;   // iIteratorLocal

        /// <summary>
        /// 1-based bit index for definite assignment. Zero means it hasn't been set yet.
        /// This is applicable only when used within the instance type
        /// - not other generic instantiations.
        /// </summary>
        internal int Jbit = 0;   // int jbit;

        //union
        //{
        //    int jbit;
        //    MEMBVARSYM membPreviousEnumerator;
        //    MEMBVARSYM membOriginalCopy;
        //};
        private object obj = null;

        /// <summary>
        /// used for enumerator values only
        /// </summary>
        internal MEMBVARSYM PreviousEnumeratorSym   // MEMBVARSYM *membPreviousEnumerator;
        {
            get { return obj as MEMBVARSYM; }
            set { obj = (object)value; }
        }

        /// <summary>
        /// <para>pointer to previous enumerator in enum declaration
        /// pointer to member holding the original copy of the hoisted parameter</para>
        /// <para>only valid in fabircated classes when isHoistedParameter is set</para>
        /// </summary>
        internal MEMBVARSYM OriginalCopySym // MEMBVARSYM *membOriginalCopy;
        {
            get { return obj as MEMBVARSYM; }
            set { obj = (object)value; }
        }

        /// <summary>
        /// containing declaration
        /// </summary>
        internal AGGDECLSYM ContainingAggDeclSym; // declaration
        
        /// <summary>
        /// This is the nested struct the compiler creates for fixed sized buffers
        /// </summary>
        internal AGGSYM FixedAggSym = null; // fixedAgg

        internal AGGSYM ParentAggSym
        {
            get { return ParentSym as AGGSYM; }
        }

        internal AGGSYM ClassSym    // getClass()
        {
            get { return ParentAggSym; }
        }

        //------------------------------------------------------------
        // MEMBVARSYM.GetBaseExprTree
        //
        /// <summary>
        /// returns the base of the expression tree for this initializer
        /// ie. the entire assignment expression
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE GetBaseExprTree()
        {
            DebugUtil.Assert(!this.ClassSym.IsEnum);
            return (this.ParseTreeNode as VARDECLNODE).ArgumentsNode;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.GetConstExprTree
        //
        /// <summary>
        /// <para>returns the constant expression tree(after the =) or null</para>
        /// <para>If enum, return ENUMMBRNODE.ValueNode.</para>
        /// <para>If fixed agg (or constant-declaration), return VARDECLNODE.ArgumentsNode.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE GetConstExprTree()
        {
            if (this.ClassSym.IsEnum)
            {
                return (this.ParseTreeNode as ENUMMBRNODE).ValueNode;
            }
            else if (this.FixedAggSym !=null)
            {
                return GetBaseExprTree();
            }
            else
            {
                DebugUtil.Assert(this.IsConst);
                return (GetBaseExprTree() as BINOPNODE).Operand2;
            }
        }

        //------------------------------------------------------------
        // MEMBVARSYM.GetEvent
        //
        /// <summary>
        /// returns event. Only valid to call if isEvent is true
        /// </summary>
        /// <param name="symmgr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EVENTSYM GetEvent(BSYMMGR symmgr)
        {
            DebugUtil.Assert(this.IsEvent);

            EVENTSYM sym;
            sym = symmgr.LookupAggMember(
                this.Name, this.ClassSym, SYMBMASK.EVENTSYM) as EVENTSYM;
            if (sym != null && sym.EventImplementSym == this)
            {
                return sym;
            }
            else
            {
                return null;
            }
        }

        //------------------------------------------------------------
        // MEMBVARSYM.GetAttributesNode
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal BASENODE GetAttributesNode()
        {
            BASENODE attr;
            BASENODE parseTree = GetParseTree();

            if (parseTree == null)
                return null;
            else if (parseTree.Kind == NODEKIND.ENUMMBR)
            {
                // enumerators currently don't have attributes
                attr = (parseTree as ENUMMBRNODE).AttributesNode;
            }
            else
            {
                BASENODE fieldTree = (parseTree as VARDECLNODE).ParentNode;
                while (fieldTree.Kind == NODEKIND.LIST)
                {
                    fieldTree = fieldTree.ParentNode;
                }
                attr = fieldTree.asANYFIELD().AttributesNode;
            }
            return attr;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.ContainingDeclaration
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal AGGDECLSYM ContainingDeclaration()
        {
            return ContainingAggDeclSym;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.SetIbitInst
        //
        /// <summary>
        /// Ibit is the bit offset for this field within its parent instance type.
        /// </summary>
        /// <param name="ibit"></param>
        //------------------------------------------------------------
        internal void SetIbitInst(int ibit)
        {
            DebugUtil.Assert(ClassSym.IsStruct);
            DebugUtil.Assert(Jbit == 0);
            DebugUtil.Assert(ibit >= 0);

            this.Jbit = ibit + 1;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.GetIbitInst
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetIbitInst()
        {
            DebugUtil.Assert(Jbit > 0);
            return Jbit - 1;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.GetPreviousEnumerator
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBVARSYM GetPreviousEnumerator()
        {
            DebugUtil.Assert(ClassSym.IsEnum);
            return this.PreviousEnumeratorSym;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.SetPreviousEnumerator
        //
        /// <summary></summary>
        /// <param name="prevSym"></param>
        //------------------------------------------------------------
        internal void SetPreviousEnumerator(MEMBVARSYM prevSym)
        {
            DebugUtil.Assert(ClassSym.IsEnum);
            this.PreviousEnumeratorSym = prevSym;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.GetOriginalCopy
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBVARSYM GetOriginalCopy()
        {
            DebugUtil.Assert(ClassSym.IsFabricated &&
                       IsHoistedParameter &&
                       !ClassSym.IsStruct &&
                       !ClassSym.IsEnum);

            return OriginalCopySym;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.SetOriginalCopy
        //
        /// <summary></summary>
        /// <param name="membOriginalCopy"></param>
        //------------------------------------------------------------
        internal void SetOriginalCopy(MEMBVARSYM membOriginalCopy)
        {
            DebugUtil.Assert(ClassSym.IsFabricated && !ClassSym.IsStruct && !ClassSym.IsEnum); 

            IsHoistedParameter = true;
            this.OriginalCopySym = membOriginalCopy;
        }

        //------------------------------------------------------------
        // MEMBVARSYM.SetCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetCustomAttribute(CustomAttributeBuilder caBuilder, out Exception excp)
        {
            excp = null;

            if (this.fieldBuilder != null && caBuilder != null)
            {
                try
                {
                    this.fieldBuilder.SetCustomAttribute(caBuilder);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    excp = ex;
                }
                catch (InvalidOperationException ex)
                {
                    excp = ex;
                }
            }
            return false;
        }

#if DEBUG
        //------------------------------------------------------------
        // MEMBVARSYM Debug
        //------------------------------------------------------------
        internal void DebugMembVar(StringBuilder sb)
        {
            if (this.FieldBuilder != null)
            {
                sb.AppendFormat("FieldBuilder : {0}\n", this.FieldBuilder.Name);
            }
            else if (this.FieldInfo != null)
            {
                sb.AppendFormat("FieldInfo    : {0}\n", this.FieldInfo.Name);
            }
            else
            {
                sb.Append("FieldInfo    :\n");
            }
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugMembVar(sb);
            DebugVar(sb);
            DebugPostfix(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class MEMBVARINFO
    //
    /// <summary>
    /// <para>MEMBVARINFO - Additional information about an meber variable symbol
    /// that isn't needed by other code binding against this variable.
    /// This structure lives only when this particular variable is being compiled.</para>
    /// <para>Has only one field,  FoundOffset.</para>
    /// </summary>
    //======================================================================
    internal class MEMBVARINFO
    {
        internal bool FoundOffset = false;  // foundOffset
    }

    //======================================================================
    // class EVENTSYM
    //
    /// <summary>
    /// <para>EVENTSYM - a symbol representing an event.
    /// The symbol points to the AddOn and RemoveOn methods that handle adding and removing delegates to the event.
    /// If the event wasn't imported, it also points to the "implementation" of the event
    /// -- a field or property symbol that is always private.</para>
    /// <para>Derives from SYM.</para>
    /// </summary>
    //======================================================================
    internal class EVENTSYM : SYM
    {
        internal EventInfo EventInfo = null;

        internal EventBuilder EventBuilder = null;

        internal string deprecatedMessage;

        /// <summary>
        /// Static member?
        /// </summary>
        internal bool IsStatic = false; // bool isStatic: 1;

        /// <summary>
        /// Only valid iff isBogus == TRUE.
        /// If this is true then tell the user to call the accessors directly.
        /// </summary>
        internal bool UseMethodInstead = false;    // bool useMethInstead: 1;

        /// <summary>
        /// event is unsafe (either marked as such, or is in an unsafe context)
        /// </summary>
        new internal bool IsUnsafe = false; // bool isUnsafe: 1;

        new internal bool IsOverride = false;   // bool isOverride: 1;

        /// <summary>
        /// Type of the event.
        /// </summary>
        internal TYPESYM TypeSym = null;    // type

        /// <summary>
        /// Adder method (always has same parent)
        /// </summary>
        internal METHSYM AddMethodSym = null;   // methAdd

        /// <summary>
        /// Remover method (always has same parent)
        /// </summary>
        internal METHSYM RemoveMethodSym = null;    // methRemove

        /// <summary>
        /// underlying field or property that implements the event.
        /// </summary>
        internal SYM EventImplementSym = null;  // implementation
        
        /// <summary>
        /// <para>For an explicit impl, this is the base event we're implementing.</para>
        /// <para>For an override, it's the base virtual/abstract event.</para>
        /// </summary>
        internal EventWithType SlotEventWithType = new EventWithType(); // ewtSlot

        /// <summary>
        /// For an explicit impl, if ewtSlot couldn't be resolved, this contains error information.
        /// </summary>
        internal ERRORSYM ExpImplErrorSym = null; // errExpImpl

        /// <summary>
        /// Meta-data token for imported event.
        /// </summary>
        internal int ImportedEventToken = 0;    // tokenImport

        /// <summary>
        /// Metadata token (memberRef or memberDef) in the current output file.
        /// </summary>
        internal int EmittedEventToken = 0; // tokenEmit

        /// <summary>
        /// parse tree, could be a VARDECLNODE or a PROPDECLNODE
        /// </summary>
        internal BASENODE ParseTreeNode = null; // parseTree

        /// <summary>
        /// containing declaration
        /// </summary>
        internal AGGDECLSYM ContainingAggDeclSym = null;    // declaration

        internal AGGSYM ParentAggSym
        {
            get { return ParentSym as AGGSYM; }
        }

        internal AGGSYM ClassSym   // getClass()
        {
            get { return ParentAggSym; }
        }

        //------------------------------------------------------------
        // EVENTSYM.GetAttributesNode
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal BASENODE GetAttributesNode()
        {
            BASENODE parseTree = GetParseTree();
            BASENODE attributes = null;
        
            // parseTree could be field or property parse tree.
            if (parseTree == null)
            {
                return null;
            }
            else if (parseTree.Kind == NODEKIND.VARDECL)
            {
                BASENODE fieldTree = (parseTree as VARDECLNODE).ParentNode;
                while (fieldTree.Kind == NODEKIND.LIST)
                {
                    fieldTree = fieldTree.ParentNode;
                }
                attributes = fieldTree.asANYFIELD().AttributesNode;
            }
            else if (parseTree.Kind == NODEKIND.PROPERTY)
            {
                attributes = (parseTree as PROPERTYNODE).AttributesNode;
            }
            else
            {
                DebugUtil.Assert(false, "BadEvent nodes");
                attributes = null;
            }
           return attributes;
        }

        //------------------------------------------------------------
        // EVENTSYM.GetParseFlags
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NODEFLAGS GetParseFlags()
        {
            if (ParseTreeNode == null) return (NODEFLAGS)0;

            if (ParseTreeNode.Kind == NODEKIND.PROPERTY)
            {
                return ParseTreeNode.Flags;
            }
            else
            {
                VARDECLNODE node = ParseTreeNode as VARDECLNODE;
                return (node != null ? node.DeclarationsNode.Flags : (NODEFLAGS)0);
            }
        }

        /// <summary>
        /// <para>(R) True if its Name is null.</para>
        /// </summary>
        /// <returns></returns>
        internal bool IsExpImpl
        {
            get { return Name == null; }
        }

        //------------------------------------------------------------
        // EVENTSYM.ContainingDeclaration
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal AGGDECLSYM ContainingDeclaration()
        {
            return ContainingAggDeclSym;
        }

        //------------------------------------------------------------
        // EVENTSYM.SetCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetCustomAttribute(CustomAttributeBuilder caBuilder, out Exception excp)
        {
            excp = null;

            if (this.EventBuilder != null && caBuilder != null)
            {
                try
                {
                    this.EventBuilder.SetCustomAttribute(caBuilder);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    excp = ex;
                }
                catch (InvalidOperationException ex)
                {
                    excp = ex;
                }
            }
            return false;
        }

#if DEBUG
        //------------------------------------------------------------
        // EVENTSYM Debug
        //------------------------------------------------------------
        internal void DebugEvent(StringBuilder sb)
        {
            if (this.EventInfo != null)
            {
                sb.AppendFormat("EventInfo    : {0}\n", this.EventInfo.Name);
            }
            else
            {
                sb.Append("EventInfo    :\n");
            }

            if (this.EventBuilder != null)
            {
                sb.AppendFormat("EventBuilder : {0}\n", "this.EmittedEventBuilder");
            }
            else
            {
                sb.Append("EventBuilder :\n");
            }
        }

        internal override void Debug(StringBuilder sb)
        {
            DebugPrefix(sb);
            DebugEvent(sb);
            DebugPostfix(sb);
        }

        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class EVENTINFO
    //
    /// <summary>
    /// EVENTINFO - Additional information about an event symbol
    /// that isn't needed by other code binding against this event.
    /// This structure lives only when this particular variable is being compiled.
    /// </summary>
    //======================================================================
    internal class EVENTINFO
    {
    }

    //======================================================================
    // enum TEMP_KIND	(TK_)
    //
    /// <summary>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal enum TEMP_KIND : int
    {
        SHORTLIVED,
        RETURN,
        LOCK,
        USING,
        DURABLE,
        FOREACH_GETENUM,
        FOREACH_ARRAY,
        FOREACH_ARRAYINDEX_0,
        // NOTE: this must be the last one. 
        // NOTE: additional kinds are created based on the rank of the foreached array
        FOREACH_ARRAYLIMIT_0 = FOREACH_ARRAYINDEX_0 + 256,
        // NOTE: OK, I lied. we need two extendible kinds of temps, so limit the arrayindexes in EnC to 256
        FIXED_STRING_0 = FOREACH_ARRAYLIMIT_0 + 256,
    }

    //======================================================================
    // class LOCSLOTINFO
    //
    /// <summary>
    /// <para>Has a TYPESYM field and a slot index.</para>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal class LOCSLOTINFO
    {
        //------------------------------------------------------------
        // LOCSLOTINFO Fields and Properties
        //------------------------------------------------------------

        internal LocalBuilder LocalBuilder = null;

        internal TYPESYM TypeSym = null;    // PTYPESYM type;
        
        /// <summary>
        /// Used for definite assignment tracking and again (independently) for code gen
        /// </summary>
        private int slotIndex = 0;  // uint islot;

        internal int Index
        {
            get // Index()
            {
                DebugUtil.Assert(this.HasIndex || this.slotIndex == 0);
                return this.slotIndex;
            }
            set // SetIndex(int index)
            {
                this.slotIndex = value;
                this.hasIndex = true;
            }
        }

        /// <summary>
        /// Is islot a valid slot #
        /// </summary>
        private bool hasIndex = false;  // bool hasIndex:1;

        internal bool HasIndex  // HasIndex()
        {
            get { return hasIndex; }
        }

        /// <summary>
        /// <para>void SetUsed(bool fUsed), bool IsUsed()</para>
        /// </summary>
        internal bool IsUsed = false;    // bool fUsed:1;


        internal bool IsParameter = false;  // bool isParam:1;
        
        /// <summary>
        /// also set if outparam...
        /// </summary>
        internal bool IsReferenceParameter = false; // bool isRefParam:1;

        internal bool IsTemporary = false;  // bool isTemporary:1;

        internal bool HasInit = false;  // bool hasInit:1;

        internal bool IsReferenced = false; // bool isReferenced:1;

        internal bool IsReferencedAssignement = false;  // bool isReferencedAssg:1;

        /// <summary>
        /// this is set when the variable is declared
        /// </summary>
        internal bool MustBePinned = false; //bool mustBePinned:1;

        /// <summary>
        /// and this is set when it is first assigned to
        /// </summary>
        internal bool IsPinned = false; // bool isPinned:1;

        /// <summary>
        /// cannot be verifed to be unaliased
        /// </summary>
        internal bool AliasPossible = false;    // bool aliasPossible:1;
        
        // The following fields apply to temporaries only:

        internal bool IsTaken = false;  // bool isTaken:1;

        internal TEMP_KIND TempKind = 0;    // tempKind

#if DEBUG
        internal string LastFile = null;    // lastFile
        internal int LastLine = 0;          // lastLine
#endif

        static internal readonly LOCSLOTINFO INVALID = new LOCSLOTINFO();

        //------------------------------------------------------------
        // LOCSLOTINFO.SetJbitDefAssg
        //
        /// <summary></summary>
        /// <param name="jbit"></param>
        //------------------------------------------------------------
        internal void SetJbitDefAssg(int jbit)
        {
            DebugUtil.Assert(!hasIndex);
            slotIndex = jbit;
        }

        //------------------------------------------------------------
        // LOCSLOTINFO.JbitDefAssg
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int JbitDefAssg()
        {
            DebugUtil.Assert(!hasIndex);
            return slotIndex;
        }

#if DEBUG
        //------------------------------------------------------------
        // LOCSLOTINFO.Debug
        //------------------------------------------------------------
        internal string Debug()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("No.{0}", slotIndex);
            if (TypeSym != null)
            {
                sb.AppendFormat(" Type {0}", TypeSym.SymID);
            }

            return sb.ToString();
        }
#endif
    }

    //======================================================================
    // class GLOBALATTRSYM
    //
    /// <summary>
    /// GLOBALATTRSYM - a symbol representing a global attribute on an assembly or module
    /// </summary>
    //======================================================================
    internal class GLOBALATTRSYM : SYM
    {
        internal AttributeTargets ElementKind = 0;      // CorAttributeTargets elementKind;

        internal BASENODE ParseTreeNode = null;         // struct BASENODE * parseTree;

        internal string AttributeName = null;
        internal object[] PositionalArguments = null;

        internal GLOBALATTRSYM NextAttributeSym = null; // GLOBALATTRSYM * nextAttr;

#if DEBUG
        //------------------------------------------------------------
        // GLOBALATTRSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class LOCVARSYM
    //
    /// <summary>
    /// <para>LOCVARSYM - a symbol representing a local variable or parameter.
    /// Parent is a scope.</para>
    /// <para>Derives from VARSYM.</para>
    /// </summary>
    //======================================================================
    internal class LOCVARSYM : VARSYM
    {
        //------------------------------------------------------------
        // LOCVARSYM Fields and Properties
        //------------------------------------------------------------

        //internal LocalBuilder LocalBuilder = null;

        internal LOCSLOTINFO LocSlotInfo = new LOCSLOTINFO();   // slot

        internal int IsLockOrDisposeTargetCount = 0;    // isLockOrDisposeTargetCount

        internal bool IsConst = false;  // bool isConst : 1;

        /// <summary>
        /// used for catch variables, and fixed variables
        /// </summary>
        internal bool IsNonWriteable = false;   // bool isNonWriteable : 1;

        /// <summary>
        /// Is this the one and only <this> pointer?
        /// </summary>
        internal bool IsThis = false;   // bool isThis : 1;

        /// <summary>
        /// This local has been hoisted for an interator
        /// </summary>
        internal bool IsIteratorLocal = false;  // bool fIsIteratorLocal : 1;
        
        // movedToField should have iIteratorLocal set appropriately

        /// <summary>
        /// The local is compiler generated and has a managled name
        /// </summary>
        internal bool IsCompilerGenerated = false;  // bool fIsCompilerGenerated : 1;

        /// <summary>
        /// Set if the local is ever used in an anon method
        /// </summary>
        internal bool UsedInAnonMeth = false;   // bool fUsedInAnonMeth : 1;

        /// <summary>
        /// Set if the local is used in an anon method and the anon method should be emitted
        /// </summary>
        internal bool HoistForAnonMeth = false; // bool fHoistForAnonMeth : 1;

        // For better lvalue errors

        internal bool IsForeach = false;    // bool isForeach : 1;

        internal bool IsUsing = false;  // bool isUsing : 1;

        internal bool IsFixed = false;  // bool isFixed : 1;

        internal bool IsCatch = false;  // bool isCatch : 1;

        internal CONSTVAL ConstVal = new CONSTVAL();    // constVal

        /// <summary>
        /// line of decl...
        /// </summary>
        internal POSDATA FirstUsedPos = new POSDATA();   // firstUsed

        /// <summary>
        /// If debug info on: IL location of first use
        /// </summary>
        internal BBLOCK DegubBlockFirstUsed = null; // * debugBlockFirstUsed

        internal int DebugOffsetFirstUsed = 0;  // unsigned debugOffsetFirstUsed;

        internal BASENODE DeclTreeNode = null;  // * declTree

        /// <summary>
        /// Indicates the field that a local was hoisted to
        /// </summary>
        internal MEMBVARSYM MovedToFieldSym = null; // * movedToField

        /// <summary>
        /// The first place where this local had it's address taken (for error reporting)
        /// </summary>
        internal BASENODE AddressTakenNode = null;  // * nodeAddrTaken

        /// <summary>
        /// The first place where this local was used inside an anonymous method (for error reporting)
        /// </summary>
        internal BASENODE NodeUsedInAnonMethod = null;  // * nodeAnonMethUse

        //------------------------------------------------------------
        // LOCVARSYM.DeclarationScope
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SCOPESYM DeclarationScope()
        {
            return (ParentSym as SCOPESYM);
        }

        //------------------------------------------------------------
        // LOCVARSYM.IsAssumedPinned
        //------------------------------------------------------------
        internal bool IsAssumedPinned()
        {
            //ASSERT(nodeAnonMethUse == null || nodeAddrTaken == null);
            if (this.NodeUsedInAnonMethod != null && AddressTakenNode != null) return false;
            return (AddressTakenNode != null);
        }

#if DEBUG
        //------------------------------------------------------------
        // LOCVARSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // enum SCOPEFLAGS	(SF_)
    //
    /// <summary>
    /// <para>Flags which any scope may have</para>
    /// <para>(CSharp\SCComp\Symbol.cs)</para>
    /// </summary>
    //======================================================================
    internal enum SCOPEFLAGS : int
    {
        NONE = 0x00,
        CATCHSCOPE = 0x01,
        TRYSCOPE = 0x02,
        SWITCHSCOPE = 0x04,
        FINALLYSCOPE = 0x08,
        DELEGATESCOPE = 0x10,
        KINDMASK = 0x1F,

        /// <summary>
        /// special scope for base (or this) call args
        /// </summary>
        ARGSCOPE = 0x40,

        /// <summary>
        /// this, or a child scope, has locals of interest
        /// </summary>
        HASVARS = 0x80,

        /// <summary>
        /// this finally does little work in its finally clause (always terminates quickly)
        /// </summary>
        LAZYFINALLY = 0x200,

        /// <summary>
        /// has a yield break; statement (set and used during rewrite)
        /// </summary>
        HASYIELDBREAK = 0x400,

        /// <summary>
        /// has a yield return <expr>; statement (set during post bind, set and used during rewrite)
        /// </summary>
        HASYIELDRETURN = 0x800,

        /// <summary>
        /// Scope has been copied (at least partially) into dispose method of iterators
        /// </summary>
        DISPOSESCOPE = 0x2000,
    }

    //======================================================================
    // SCOPESYM
    //
    /// <summary>
    /// <para>a symbol represent a scope that holds other symbols. Typically unnamed.</para>
    /// <para>Derives from PARENTSYM.</para>
    /// </summary>
    //======================================================================
    internal class SCOPESYM : PARENTSYM
    {
        /// <summary>
        /// the nesting order of this scopes. outermost == 0
        /// </summary>
        internal int NestingOrder = 0;  // unsigned nestingOrder;

        //union {
        // the associated block... (not for try scopes)
        //private EXPRBLOCK block = null;

        // for try scopes only...
        //private SCOPESYM finallyScope = null;
        //};

        private object obj;

        /// <summary>
        /// <para>(RW) the associated block... (not for try scopes)</para>
        /// <para>obj as EXPRBLOCK.</para>
        /// </summary>
        internal EXPRBLOCK BlockExpr    // block
        {
            get // GetBlock()
            {
                return (ScopeFlags & SCOPEFLAGS.TRYSCOPE) != 0 ? null : obj as EXPRBLOCK;
            }
            set // SetBlock(EXPRBLOCK* in)
            {
                DebugUtil.Assert(FinallyScopeSym == null);
                DebugUtil.Assert((ScopeFlags & SCOPEFLAGS.TRYSCOPE) == 0);

                obj = value;
            }
        }

        /// <summary>
        /// <para>(RW) for try scopes only...</para>
        /// <para>obj as SCOPESYM.</para>
        /// </summary>
        internal SCOPESYM FinallyScopeSym   // finallyScope
        {
            get // GetFinallyScope()
            {
                return (ScopeFlags & SCOPEFLAGS.TRYSCOPE) != 0 ? obj as SCOPESYM : null;
            }
            set // SetFinallyScope(SCOPESYM* in)
            {
                DebugUtil.Assert(BlockExpr == null);
                DebugUtil.Assert((ScopeFlags & SCOPEFLAGS.TRYSCOPE) != 0);

                obj = value;
            }
        }

        internal AnonScopeInfo AnonymousScopeInfo = null;   // pasi

        /// <summary>
        /// last statement in this scope...
        /// </summary>
        internal BASENODE TreeNode = null;  // tree

        /// <summary>
        /// If debug info on: location of first IL instruction in scope
        /// </summary>
        internal BBLOCK DebugStartBBlock = null;    // debugBlockStart

        /// <summary>
        /// If debug info on: location of first IL instruction after scope
        /// </summary>
        internal BBLOCK DebugEndBBlock = null;  // debugBlockEnd

        internal int DebugStartOffset;          // unsigned debugOffsetStart;

        internal int DebugEndOffset;            // unsigned debugOffsetEnd;

        internal SCOPEFLAGS ScopeFlags = 0;     // scopeFlags

#if DEBUG
        //------------------------------------------------------------
        // SCOPESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // ANONSCOPESYM
    //======================================================================
    internal class ANONSCOPESYM : SYM
    {
        internal SCOPESYM ScopeSym; // * scope

#if DEBUG
        //------------------------------------------------------------
        // INFILESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class ERRORSYM
    //
    /// <summary>
    /// <para>ERRORSYM - a symbol representing an error that has been reported.</para>
    /// <para>Derived from TYPESYM.</para>
    /// </summary>
    //======================================================================
    internal class ERRORSYM : TYPESYM
    {
        /// <summary>
        /// Special name of ERRORSYM.
        /// </summary>
        internal string ErrorName = null;   // nameText

        /// <summary>
        /// (R) Has no special name.
        /// </summary>
        internal bool NoErrorName
        {
            get { return String.IsNullOrEmpty(ErrorName); }
        }

        internal TypeArray TypeArguments = null;    // typeArgs

        internal bool NoTypeArguments
        {
            get { return (TypeArguments == null || TypeArguments.Count == 0); }
        }

        /// <summary>
        /// Type ref - doesn't include type args and parent.
        /// </summary>
        internal int EmittedToken = 0;  // mdToken tokenEmit;

        /// <summary>
        /// Type spec - includes type args and parent.
        /// </summary>
        internal int EmittedSpecToken = 0;  // mdToken tokenEmitSpec;

#if DEBUG
        //------------------------------------------------------------
        // ERRORSYM Construcotr
        //------------------------------------------------------------
        internal ERRORSYM()
            : base()
        {
            ;
        }

        //------------------------------------------------------------
        // ERRORSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // XMLFILESYM
    //
    /// <summary>
    /// a symbol representing an XML file that has been included
    /// via the <include> element in a DocComment
    /// </summary>
    //======================================================================
    internal class XMLFILESYM : SYM
    {

#if DEBUG
        //------------------------------------------------------------
        // XMLFILESYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    // class SORTEDLIST
    //======================================================================
    internal class SORTEDLIST
    {
        internal List<SYM> symlist = null;  // This list is NULL terminated
    }

    //======================================================================
    // class MISCSYM
    //======================================================================
    internal class MISCSYM : SYM
    {
        internal enum TYPE : int { TYPE, };

        internal void Init(TYPE typ)
        {
            MiscKind = typ;
        }

        internal TYPE MiscKind;

#if DEBUG
        //------------------------------------------------------------
        // MISCSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif
    }

    //======================================================================
    //  EXTERNALIASSYM
    //
    /// <summary>
    /// a symbol representing an alias for a referenced file
    /// </summary>
    //======================================================================
    internal class EXTERNALIASSYM : SYM
    {
        private int assemblyID;

        /// <summary>
        /// Cache of "Foo::" (global namespace in aid). 
        /// </summary>
        internal NSAIDSYM NsAidSym;

        private List<SYM> inFileList = new List<SYM>();
        internal List<SYM> InFileList
        {
            get { return inFileList; }
        }

        /// <summary>
        /// The assembly ids of all contained assemblies.
        /// </summary>
        private BitSet bsetAssemblies = new BitSet();
        /// <summary>
        /// (R) The assembly ids of all contained assemblies.
        /// </summary>
        internal BitSet AssembliesBitset
        {
            get { return bsetAssemblies; }
        }

        //------------------------------------------------------------
        // EXTERNALIASSYM.GetAssemblyID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal int GetAssemblyID()
        {
            return assemblyID;
        }

        //------------------------------------------------------------
        // EXTERNALIASSYM.SetAssemblyID
        //
        /// <summary></summary>
        /// <param name="id"></param>
        //------------------------------------------------------------
        new internal void SetAssemblyID(int id)
        {
            assemblyID = id;
        }

        //------------------------------------------------------------
        // EXTERNALIASSYM.GetInfileListTail
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetInfileListTail()
        {
            if (inFileList != null && inFileList.Count > 0)
                return inFileList[inFileList.Count - 1];
            return null;
        }

        // Doesn't have an in-source representation, so prevent some bad calls
        // returns parse tree for classes, and class members
        new internal void GetParseTree() { }
        new internal void GetInputFile() { }
        new internal void GetModule() { }
        //new internal void GetMetaImport(COMPILER compiler) { }
        new internal void ContainingDeclaration() { }
        new internal void GetAttributesNode() { }
        new internal void IsContainedInDeprecated() { }
        new internal void IsVirtual() { }
        new internal void IsOverride() { }

#if DEBUG
        //------------------------------------------------------------
        // EXTERNALIASSYM Debug
        //------------------------------------------------------------
        internal override string _DebugInfoString
        {
            get { return base._DebugInfoString; }
        }
#endif

    }

    //======================================================================
    // enum RuntimeBindKindEnum
    //
    /// <summary>
    /// (CS4)
    /// </summary>
    //======================================================================
    internal enum RuntimeBindKindEnum : int
    {
        Undefined = 0,
        Operand,
        GetMember,
        SetMember,
        InvokeMember,
    }

    //======================================================================
    //  RUNTIMEBINDSYM
    //
    /// <summary>
    /// (CS4) Base class to represent runtime binded expressions.
    /// </summary>
    //======================================================================
    internal class RUNTIMEBINDSYM : SYM
    {
        internal RuntimeBindKindEnum BindEnum = RuntimeBindKindEnum.Undefined;
        internal SYM DynamicObjectSym = null;
        internal CSharpBinderFlags BindFlags = CSharpBinderFlags.None;

        internal string MemberName = null;
        internal TypeArray TypeArguments = null;
        internal EXPR argumentsExpr = null;

        internal AGGTYPESYM ContextSym = null;
    }

    //======================================================================
    // SymUtil
    //======================================================================
    internal static class SymUtil
    {
        //------------------------------------------------------------
        // SymUtil.EmitParentSym
        //
        /// <summary></summary>
        /// <param name="emitter"></param>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal static void EmitParentSym(EMITTER emitter,TYPESYM typeSym)
        {
            if (typeSym == null || typeSym.Type != null)
            {
                return;
            }
            DebugUtil.Assert(emitter != null);

            switch (typeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    emitter.EmitAggregateDef(
                        (typeSym as AGGTYPESYM).GetAggregate());
                    break;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.PARAMMODSYM:
                    EmitParentSym(emitter, typeSym.ParentSym as TYPESYM);
                    break;

                case SYMKIND.TYVARSYM:
                    TYVARSYM tvSym = typeSym as TYVARSYM;
                    if (tvSym.IsMethodTypeVariable)
                    {
                        METHSYM pmethSym = tvSym.ParentSym as METHSYM;
                        DebugUtil.Assert(pmethSym != null);
                        AGGSYM paggSym = pmethSym.ParentAggSym;
                        DebugUtil.Assert(paggSym != null);
                        emitter.EmitAggregateDef(paggSym);
                        emitter.EmitMethodDef(pmethSym);
                    }
                    else
                    {
                        AGGSYM paggSym = tvSym.ParentAggSym;
                        DebugUtil.Assert(paggSym != null);
                        emitter.EmitAggregateDef(paggSym);
                    }
                    break;

                case SYMKIND.PTRSYM:
                    EmitParentSym(emitter, (typeSym as PTRSYM).BaseTypeSym);
                    break;

                case SYMKIND.NUBSYM:
                    EmitParentSym(emitter, (typeSym as NUBSYM).BaseTypeSym);
                    break;

                case SYMKIND.PINNEDSYM:
                    EmitParentSym(emitter, (typeSym as PINNEDSYM).BaseTypeSym);
                    break;

                case SYMKIND.VOIDSYM:
                default:
                    break;
            }
        }

        //------------------------------------------------------------
        // SymUtil.GetSystemTypeFromSym
        //
        /// <summary>
        /// Get the System.Type instance from the given TYPESYM.
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="classTypeArguments"></param>
        /// <param name="methodTypeArguments"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static Type GetSystemTypeFromSym(
            TYPESYM typeSym,
            AGGSYM typeVarAggSym,
            METHSYM typeVarMethodSym)
        {
            if (typeSym == null)
            {
                return null;
            }
            Type type = null;
            int count = 0;
            int index = 0;

            switch (typeSym.Kind)
            {
                //----------------------------------------------------
                // AGGTYPESYM
                //----------------------------------------------------
                case SYMKIND.AGGTYPESYM:
                case SYMKIND.DYNAMICSYM:
                    if (typeSym.Type != null &&
                        !typeSym.Type.IsGenericTypeDefinition)
                    {
                        return typeSym.Type;
                    }

                    AGGTYPESYM ats = typeSym as AGGTYPESYM;
                    DebugUtil.Assert(ats != null);

                    //type = ats.Type;
                    //if (type != null)
                    //{
                    //    return type;
                    //}
                    TypeArray typeArgs = ats.AllTypeArguments;

                    AGGSYM aggSym = ats.GetAggregate();
                    DebugUtil.Assert(aggSym != null);
                    type = aggSym.Type;
                    if (type == null)
                    {
                        return null;
                    }

                    if (typeArgs != null && (count = typeArgs.Count) > 0)
                    {
                        Type[] targs = GetSystemTypesFromTypeArray(
                            typeArgs,
                            typeVarAggSym,
                            typeVarMethodSym);
                        if (targs == null)
                        {
                            return null;
                        }
                        type = type.MakeGenericType(targs);
                    }
                    ats.SetSystemType(type, true);
                    return ats.Type;

                //----------------------------------------------------
                // ARRAYSYM
                //----------------------------------------------------
                case SYMKIND.ARRAYSYM:
                    if (typeSym.Type != null)
                    {
                        return typeSym.Type;
                    }

                    type = GetSystemTypeFromSym(
                        typeSym.ParentSym as TYPESYM,
                        typeVarAggSym,
                        typeVarMethodSym);
                    if (type != null)
                    {
                        int rank = (typeSym as ARRAYSYM).Rank;
                        if (rank == 1)
                        {
                            return type.MakeArrayType();
                        }
                        if (rank > 1)
                        {
                            return type.MakeArrayType(rank);
                        }
                    }
                    return null;

                //----------------------------------------------------
                // VOIDSYM
                //----------------------------------------------------
                case SYMKIND.VOIDSYM:
                    return SystemType.VoidType;

                //----------------------------------------------------
                // PARAMMODSYM
                //----------------------------------------------------
                case SYMKIND.PARAMMODSYM:
                    if (typeSym.Type != null)
                    {
                        return typeSym.Type;
                    }

                    type = GetSystemTypeFromSym(
                        typeSym.ParentSym as TYPESYM,
                        typeVarAggSym,
                        typeVarMethodSym);
                    if (type != null)
                    {
                        return type.MakeByRefType();
                    }
                    break;

                //----------------------------------------------------
                // TYVARSYM
                //----------------------------------------------------
                case SYMKIND.TYVARSYM:
                    TYVARSYM tvSym = typeSym as TYVARSYM;
                    Type[] tvTypes = null;

                    if (tvSym.IsMethodTypeVariable)
                    {
                        if (typeVarMethodSym == null ||
                            (tvTypes = typeVarMethodSym.GenericParameterTypes) == null)
                        {
                            return null;
                        }
                        count = tvTypes.Length;
                        index = tvSym.Index;
                        if (count > 0 && index >= 0 && index < count)
                        {
                            return tvTypes[index];
                        }
                        return null;
                    }
                    else // if (tvSym.IsMethodTypeVariable)
                    {
                        if (typeVarAggSym == null ||
                            (tvTypes = typeVarAggSym.GenericParameterTypes) == null)
                        {
                            return null;
                        }
                        count = tvTypes.Length;
                        index = tvSym.TotalIndex;
                        if (count > 0 && index >= 0 && index < count)
                        {
                            return tvTypes[index];
                        }
                        return null;
                    }

                //----------------------------------------------------
                // PTRSYM
                //----------------------------------------------------
                case SYMKIND.PTRSYM:
                    if (typeSym.Type != null)
                    {
                        return typeSym.Type;
                    }

                    type = GetSystemTypeFromSym(
                        (typeSym as PTRSYM).BaseTypeSym,
                        typeVarAggSym,
                        typeVarMethodSym);
                    if (type != null)
                    {
                        return type.MakePointerType();
                    }
                    break;

                //----------------------------------------------------
                // NUBSYM
                //----------------------------------------------------
                case SYMKIND.NUBSYM:
                    if (typeSym.Type != null)
                    {
                        return typeSym.Type;
                    }

                    type = GetSystemTypeFromSym(
                        (typeSym as NUBSYM).BaseTypeSym,
                        typeVarAggSym,
                        typeVarMethodSym);
                    if (type != null)
                    {
                        return SystemType.GetNullable().MakeGenericType(type);
                    }
                    break;

                //----------------------------------------------------
                // PINNEDSYM
                //----------------------------------------------------
                case SYMKIND.PINNEDSYM:
                    if (typeSym.Type != null)
                    {
                        return typeSym.Type;
                    }

                    type = GetSystemTypeFromSym(
                        (typeSym as PINNEDSYM).BaseTypeSym,
                        typeVarAggSym,
                        typeVarMethodSym);
                    if (type != null)
                    {
                        return type.MakeByRefType();
                    }
                    break;

                //----------------------------------------------------
                // otherwise
                //----------------------------------------------------
                case SYMKIND.NULLSYM:
                case SYMKIND.ERRORSYM:
                    break;

                case SYMKIND.MODOPTTYPESYM:
                    throw new NotImplementedException("SymbolUtil.MakeSystemType: MODOPTTYPESYM");

                case SYMKIND.ANONMETHSYM:
                case SYMKIND.METHGRPSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.LAMBDAEXPRSYM:
                    break;

                default:
                    break;
            }
            DebugUtil.Assert(false);
            return null;
        }

        //------------------------------------------------------------
        // SymUtil.GetSystemTypesFromTypeArray
        //
        /// <summary></summary>
        /// <param name="typeArray"></param>
        /// <param name="classTypeArguments"></param>
        /// <param name="methodTypeArguments"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static Type[] GetSystemTypesFromTypeArray(
            TypeArray typeArray,
            AGGSYM typeVarAggSym,
            METHSYM typeVarMethodSym)
        {
            int count = TypeArray.Size(typeArray);
            if (count == 0)
            {
                return Type.EmptyTypes;
            }
            Type[] systemTypes = new Type[count];

            for (int i = 0; i < count; ++i)
            {
                Type sysType = SymUtil.GetSystemTypeFromSym(
                    typeArray[i],
                    typeVarAggSym,
                    typeVarMethodSym);
                if (sysType == null)
                {
                    return null;
                }
                systemTypes[i] = sysType;
            }
            return systemTypes;
        }
    }
   
}
