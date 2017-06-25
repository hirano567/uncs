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
// File: nodes.h
//
// Definitions of all parse tree nodes
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
// File: node.cpp
//
// ===========================================================================

//============================================================================
// Nodes.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Uncs
{
    //======================================================================
    // class CompilerVersion
    //
    // COMPILER_VERSION
    //
    /// <summary>
    /// <para>This value is used to ensure that a consumer of the compiler DLL is compatible
    /// with it, meaning they are using the same, or at least binary compatible,
    /// versions of the header files defining structures, constants, interfaces, etc.
    /// This value must be incremented when a change is made that breaks compatibility
    /// to avoid mismatched versions of the compiler and language service from being
    /// used together.  Change COMPILER_BASE_VERSION since we set the high order bit
    /// for debug builds so we can distinguish debug from retail, which are
    /// incompatible, and should cause an error</para>
    /// <para>NOTE: This includes the Predefined Types and Names tables!</para>
    /// </summary>
    //======================================================================
    internal class CompilerVersion
    {
        private uint beforeNtVersion = 0;
        private uint baseVersion = 0;
        private uint currentVersion = 0;

        internal CompilerVersion()
        {
            beforeNtVersion = 0x00000057;
            baseVersion = (beforeNtVersion | (((uint)PREDEFNAME.COUNT << 24) & 0x7F000000));
#if DEBUG
            currentVersion = (0x80000000 | baseVersion);
#else
            currentVersion=baseVersion;
#endif
        }

        internal uint Version
        {
            get { return currentVersion; }
        }
    }

    //======================================================================
    // NODEFLAGS   (NF_)
    //
    /// <summary>
    /// <para>NOTE:  NF_MOD_* values must be kept in sync with CLSDREC::accessTokens</para>
    /// <para>We have 16 bits of space for flags in BASENODE.  We have more than 16 flags.
    /// Thus, we compact them into groups such that no two flags that could be set on
    /// the same node have the same value.  This is easier thought of as groups of
    /// flags; no two flag GROUPS that could have representation on the same node
    /// can have overlap.</para>
    /// <para>In sscli, with prefix NF_ . (Nodes.cs)</para>
    /// </summary>
    //======================================================================
    [Flags]
    internal enum NODEFLAGS : uint
    {
        //------------------------------------------------------------
        // MODIFIER flags (0x0000-0x0FFF)
        //------------------------------------------------------------
        MOD_ABSTRACT = 0x0001,
        MOD_NEW = 0x0002,
        MOD_OVERRIDE = 0x0004,
        MOD_PRIVATE = 0x0008,
        MOD_PROTECTED = 0x0010,
        MOD_INTERNAL = 0x0020,
        MOD_PUBLIC = 0x0040,
        MOD_ACCESSMODIFIERS = MOD_PUBLIC | MOD_INTERNAL | MOD_PROTECTED | MOD_PRIVATE,
        MOD_SEALED = 0x0080,
        MOD_STATIC = 0x0100,
        MOD_VIRTUAL = 0x0200,
        MOD_EXTERN = 0x0400,
        MOD_READONLY = 0x0800,
        MOD_VOLATILE = 0x1000,
        MOD_UNSAFE = 0x2000,    // ok to overlap with PARMMOD_REF -- can't ever appear on an expression
        MOD_LAST_KWD = MOD_UNSAFE,
        MOD_PARTIAL = 0x4000,   // (same)
        MOD_LAST = MOD_PARTIAL,
        MOD_MASK = 0x7FFF,

        //------------------------------------------------------------
        // EXPRESSION flags -- KEEP THESE UNIQUE, they are tested for without regard to node kind
        //------------------------------------------------------------
        PARMMOD_REF = 0x2000,
        PARMMOD_OUT = 0x4000,
        CALL_HADERROR = 0x8000,

        //------------------------------------------------------------
        // STATEMENT flags
        //------------------------------------------------------------
        UNCHECKED = 0x0001,
        GOTO_CASE = 0x0002,
        TRY_CATCH = 0x0004,
        TRY_FINALLY = 0x0008,
        CONST_DECL = 0x0010,
        FOR_FOREACH = 0x0020,
        FIXED_DECL = 0x0040,
        USING_DECL = 0x0080,
        STMT_HADERROR = 0x0100,

        //------------------------------------------------------------
        // VARDECL flags
        //------------------------------------------------------------
        VARDECL_EXPR = 0x0001,
        VARDECL_ARRAY = 0x0002,

        //------------------------------------------------------------
        // Numeric constant flags
        //------------------------------------------------------------
        CHECK_FOR_UNARY_MINUS = 0x0001,

        //------------------------------------------------------------
        // Member ref flags
        //------------------------------------------------------------
        MEMBERREF_EMPTYARGS = 0x0001,

        //------------------------------------------------------------
        // Name, GenericName or Alias flags
        //------------------------------------------------------------

        /// <summary>
        /// NK_NAME or NK_ALIASNAME where no identifier existed
        /// </summary>
        NAME_MISSING = 0x0001,

        /// <summary>
        /// NK_NAME or NK_ALIASNAME with a '@' prefix
        /// </summary>
        NAME_LITERAL = 0x0002,

        /// <summary>
        /// NK_ALIASNAME whose identifier is 'global'
        /// </summary>
        GLOBAL_QUALIFIER = 0x0004,

        //------------------------------------------------------------
        // 'new' flags
        //------------------------------------------------------------

        /// <summary>
        /// NEWNODE contains a 'stackalloc' expression
        /// </summary>
        NEW_STACKALLOC = 0x0001,

        /// <summary>
        /// (CS3) This NEWNODE is in a declaration statement.
        /// Used when processing object initializers.
        /// </summary>
        NEW_IN_VARDECL = 0x0004,

        /// <summary>
        /// <para>(CS3) object-initializer</para>
        /// </summary>
        NEW_HAS_OBJECT_INITIALIZER = 0x0008,

        /// <summary>
        /// <para>(CS3) collection-initializer</para>
        /// </summary>
        NEW_HAS_COLLECTION_INITIALIZER = 0x0010,

        /// <summary>
        /// <para>(CS3) anonymous-object-creation-expression</para>
        /// </summary>
        NEW_ANONYMOUS_OBJECT_CREATION = 0x0020,

        /// <summary>
        /// <para>(CS3) implicitly typed array</para>
        /// </summary>
        NEW_IMPLICITLY_TYPED_ARRAY = 0x0040,

        //------------------------------------------------------------
        // constraint flags
        //------------------------------------------------------------

        // NK_CONSTRAINT has default constructor - 'new()'
        CONSTRAINT_NEWABLE = 0x0001,

        // NK_CONSTRAINT has reference type constraint
        CONSTRAINT_REFTYPE = 0x0002,

        // NK_CONSTRAINT has value type constraint
        CONSTRAINT_VALTYPE = 0x0004,

        //------------------------------------------------------------
        // MEMBER flags (EX -- stored in BASENODE::other)
        //------------------------------------------------------------

        EX_CTOR_BASE = 0x01,
        EX_CTOR_THIS = 0x02,
        EX_METHOD_NOBODY = 0x04,

        /// <summary>
        /// This interior node has already been parsed for errors...
        /// </summary>
        EX_INTERIOR_PARSED = 0x08,
        
        EX_METHOD_VARARGS = 0x10,
        EX_METHOD_PARAMS = 0x20,

        /// <summary>
        /// field or property is actually an event.
        /// </summary>
        EX_EVENT = 0x40,
    }

    //======================================================================
    // NODEGROUP   (NG_)
    //
    /// <summary>
    /// <para>These are used to categorize nodes into groups
    /// that can easily be tested for by using BASENODE::InGroup(group),
    /// or given a node kind, checking BASENODE::m_rgNodeGroups[kind] &amp; group.
    /// <list type="bullet">
    /// <item>BASENODE::InGroup(group)</item>
    /// <item>BASENODE::m_rgNodeGroups[kind] &amp; group</item>
    /// </list>
    /// </para>
    /// <para>In sscli, with prefix NG_. (Nodes.cs)</para>
    /// </summary>
    //======================================================================
    internal enum NODEGROUP : uint
    {
        NONGROUP = 0x00000000,

        /// <summary>
        /// Any non-namespace member of a namespace
        /// </summary>
        TYPE = 0x00000001,

        /// <summary>
        /// Class, struct, enum, interface
        /// </summary>
        AGGREGATE = 0x00000002,

        /// <summary>
        /// Method, ctor, dtor, operator
        /// </summary>
        METHOD = 0x00000004,

        /// <summary>
        /// Property, indexer
        /// </summary>
        PROPERTY = 0x00000008,

        /// <summary>
        /// Field, const
        /// </summary>
        FIELD = 0x00000010,

        /// <summary>
        /// Any statement node
        /// </summary>
        STATEMENT = 0x00000020,

        /// <summary>
        /// Any BINOP node
        /// </summary>
        BINOP = 0x00000040,

        /// <summary>
        /// Any node which can be 'keyed'
        /// </summary>
        KEYED = 0x00000080,

        /// <summary>
        /// Any node that has an interior, parsed on 2nd pass
        /// </summary>
        INTERIOR = 0x00000100,

        /// <summary>
        /// Any member of a type (MEMBERNODE)
        /// </summary>
        MEMBER = 0x00000200,

        /// <summary>
        /// Allow "blank-line" completion on this node
        /// </summary>
        GLOBALCOMPLETION = 0x00000400,

        /// <summary>
        /// Statement that 'break' works for
        /// </summary>
        BREAKABLE = 0x00000800,

        /// <summary>
        /// Statement that 'continue' works for
        /// </summary>
        CONTINUABLE = 0x00001000,

        /// <summary>
        /// Statement that 'owns' embedded stmts
        /// </summary>
        EMBEDDEDSTMTOWNER = 0x00002000,
    }

    //======================================================================
    // PROTOFLAGS (PTF_)
    //
    /// <summary>
    /// Flags to control output of BASENODE::BuildPrototype
    /// </summary>
    //======================================================================
    internal enum PROTOFLAGS : uint
    {
        //------------------------------------------------------------
        // Name control
        //------------------------------------------------------------

        /// <summary>
        /// Base name only (default)
        /// </summary>
        BASENAME = 0x00000000,

        /// <summary>
        /// Fully-qualified name (namespace included)
        /// </summary>
        FULLNAME = 0x00000001,

        /// <summary>
        /// Name of entity plus type (if member)
        /// </summary>
        TYPENAME = 0x00000002,

        PARAMETER_TYPE = 0x00000004,
        
        PARAMETER_NAME = 0x00000008,

        //------------------------------------------------------------
        // Parameter control
        //------------------------------------------------------------
        PARAMETERS = PARAMETER_TYPE | PARAMETER_NAME,

        //------------------------------------------------------------
        // Typevars control
        //------------------------------------------------------------

        /// <summary>
        /// Include type vars
        /// </summary>
        TYPEVARS = 0x00000010,
        
        FAILONMISSINGNAME = 0x00000020,
    }

    //======================================================================
    // enum PARAMTYPES (PRT_)
    //
    /// <summary>
    /// Flags to control output of BASENODE::AppendParametersTo*
    /// </summary>
    //======================================================================
    [Flags]
    internal enum PARAMTYPES : uint
    {
        /// <summary>
        /// a method, use "(" and ")" (default)
        /// </summary>
        METHOD = 0,

        /// <summary>
        /// an indexer, use "[" and "]"
        /// </summary>
        INDEXER,

        /// <summary>
        /// a generic type speicifer, use "&lt;" and "&gt;"
        /// </summary>
        GENERIC,
    }

    //internal class NRHEAP { }   //  defined in sccomp
    //internal class CInteriorNode { }    // defined in SCComp_inttree.cs

    //======================================================================
    // class BASENODE
    //
    /// <summary>
    /// Base class for all parse tree nodes
    /// </summary>
    //======================================================================
    internal class BASENODE
    {
        //------------------------------------------------------------
        // BASENODE Fields and Properties
        //------------------------------------------------------------

#if DEBUG
        /// <summary>
        /// Debug only...
        /// </summary>
        internal string NodeKindName = null;    // pszNodeKind

        /// <summary>
        /// add a vtable for debugging
        /// </summary>
        internal virtual void Dummy() { }

        internal bool HasNid = false;   // fHasNid

        internal string DebugComment = null;
#endif

        // In sscli, node ID is created from the address of instance.
        // In this program, instead assign unique numbers to objects when they created.

        /// <summary>
        /// Index starting from 1 assigned when objects are created.
        /// </summary>
        internal readonly int NodeID = CObjectID.GenerateID();

        /// <summary>
        /// Node Kind
        /// </summary>
        internal NODEKIND Kind = NODEKIND.UNDEFINED;    // kind:8;

        /// <summary>
        /// <para>Flags (NF_* bits)</para>
        /// <para>Set a value of type NODEFLAGS except EX_.（EX_ is set to Other.)</para>
        /// </summary>
        internal NODEFLAGS Flags = 0;   // unsigned flags:16;

        /// <summary>
        /// Holds a NODEFLAGS value with prefix EX_.
        /// </summary>
        internal NODEFLAGS NodeFlagsEx;

        /// <summary>
        /// Holds a OPERATOR value.
        /// </summary>
        internal OPERATOR Operator;

        /// <summary>
        /// Holds a PREDEFTYPE value.
        /// </summary>
        internal PREDEFTYPE PredefinedType;

        /// <summary>
        /// Token index (for access to position info)
        /// </summary>
        internal int TokenIndex = 0;    // long tokidx;

        /// <summary>
        /// All nodes have parent pointers
        /// </summary>
        internal BASENODE ParentNode = null;    // *pParent;

        /// <summary>
        /// Table of node group bits for each kind 
        /// </summary>
        internal static NODEGROUP[] NodeGroupTable =    // DWORD m_rgNodeGroups[];
        {
            NODEGROUP.NONGROUP, // UNDEFINED

            NODEGROUP.INTERIOR,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.BINOP,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.BINOP,
            NODEGROUP.STATEMENT,
            NODEGROUP.STATEMENT,
            NODEGROUP.BINOP,
            NODEGROUP.BREAKABLE,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.STATEMENT | NODEGROUP.EMBEDDEDSTMTOWNER,
            NODEGROUP.TYPE | NODEGROUP.AGGREGATE | NODEGROUP.KEYED,
            NODEGROUP.FIELD | NODEGROUP.MEMBER,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.STATEMENT,
            NODEGROUP.METHOD | NODEGROUP.KEYED | NODEGROUP.INTERIOR | NODEGROUP.MEMBER,
            NODEGROUP.STATEMENT,
            NODEGROUP.TYPE | NODEGROUP.KEYED,
            NODEGROUP.BINOP,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION | NODEGROUP.BREAKABLE | NODEGROUP.CONTINUABLE | NODEGROUP.EMBEDDEDSTMTOWNER,
            NODEGROUP.BINOP,
            NODEGROUP.METHOD | NODEGROUP.KEYED | NODEGROUP.INTERIOR | NODEGROUP.MEMBER,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION,
            NODEGROUP.TYPE | NODEGROUP.AGGREGATE | NODEGROUP.KEYED,
            NODEGROUP.KEYED | NODEGROUP.MEMBER,
            NODEGROUP.STATEMENT,
            NODEGROUP.FIELD | NODEGROUP.MEMBER,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION | NODEGROUP.BREAKABLE | NODEGROUP.CONTINUABLE | NODEGROUP.EMBEDDEDSTMTOWNER,
            NODEGROUP.NONGROUP,
            NODEGROUP.STATEMENT,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION | NODEGROUP.EMBEDDEDSTMTOWNER,
            NODEGROUP.TYPE | NODEGROUP.AGGREGATE | NODEGROUP.KEYED,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION | NODEGROUP.EMBEDDEDSTMTOWNER,
            NODEGROUP.BINOP,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION | NODEGROUP.EMBEDDEDSTMTOWNER,
            NODEGROUP.NONGROUP,
            NODEGROUP.METHOD | NODEGROUP.KEYED | NODEGROUP.INTERIOR | NODEGROUP.MEMBER,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.KEYED,
            NODEGROUP.MEMBER,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.METHOD | NODEGROUP.KEYED | NODEGROUP.INTERIOR | NODEGROUP.MEMBER,
            NODEGROUP.NONGROUP,
            NODEGROUP.MEMBER,
            NODEGROUP.NONGROUP,
            NODEGROUP.NONGROUP,
            NODEGROUP.PROPERTY | NODEGROUP.KEYED | NODEGROUP.MEMBER,
            NODEGROUP.PROPERTY | NODEGROUP.KEYED | NODEGROUP.MEMBER,
            NODEGROUP.STATEMENT,
            NODEGROUP.STATEMENT,
            NODEGROUP.STATEMENT,
            NODEGROUP.NONGROUP,
            NODEGROUP.TYPE | NODEGROUP.AGGREGATE | NODEGROUP.KEYED,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION | NODEGROUP.BREAKABLE,
            NODEGROUP.NONGROUP,
            NODEGROUP.STATEMENT,
            NODEGROUP.NONGROUP,
            NODEGROUP.KEYED,
            NODEGROUP.STATEMENT | NODEGROUP.GLOBALCOMPLETION | NODEGROUP.BREAKABLE | NODEGROUP.CONTINUABLE | NODEGROUP.EMBEDDEDSTMTOWNER,
            NODEGROUP.STATEMENT,

            // CS3

            NODEGROUP.TYPE,     // IMPLICITTYPE
            NODEGROUP.NONGROUP, // COLLECTIONINIT

            NODEGROUP.NONGROUP, //  0};
        };

        //------------------------------------------------------------
        // BASENODE.OperatorToTokenTable
        //
        /// <summary>
        /// <para>OPERATOR から TOKENID を取得するための配列。</para>
        /// <para>(CSharp\Inc\Node.cs)</para>
        /// </summary>
        //------------------------------------------------------------
        static internal TOKENID[] OperatorToTokenTable =
        {
            TOKENID.UNKNOWN,
            TOKENID.EQUAL,
			TOKENID.PLUSEQUAL,
			TOKENID.MINUSEQUAL,
			TOKENID.SPLATEQUAL,
			TOKENID.SLASHEQUAL,
			TOKENID.MODEQUAL,
			TOKENID.ANDEQUAL,
			TOKENID.HATEQUAL,
			TOKENID.BAREQUAL,
			TOKENID.SHIFTLEFTEQ,
			TOKENID.SHIFTRIGHTEQ,
			TOKENID.QUESTION,
			TOKENID.QUESTQUEST,
			TOKENID.LOG_OR,
			TOKENID.LOG_AND,
			TOKENID.BAR,
			TOKENID.HAT,
			TOKENID.AMPERSAND,
			TOKENID.EQUALEQUAL,
			TOKENID.NOTEQUAL,
			TOKENID.LESS,
			TOKENID.LESSEQUAL,
			TOKENID.GREATER,
			TOKENID.GREATEREQUAL,
			TOKENID.IS,
			TOKENID.AS,
			TOKENID.SHIFTLEFT,
			TOKENID.SHIFTRIGHT,
			TOKENID.PLUS,
			TOKENID.MINUS,
			TOKENID.STAR,
			TOKENID.SLASH,
			TOKENID.PERCENT,
			
			TOKENID.UNKNOWN,
			TOKENID.PLUS,
			TOKENID.MINUS,
			TOKENID.TILDE,
			TOKENID.BANG,
			TOKENID.PLUSPLUS,
			TOKENID.MINUSMINUS,
			TOKENID.TYPEOF,
			TOKENID.SIZEOF,
			TOKENID.CHECKED,
			TOKENID.UNCHECKED,
			
			TOKENID.MAKEREFANY,
			TOKENID.REFVALUE,
			TOKENID.REFTYPE,
			TOKENID.ARGS,
			
			TOKENID.UNKNOWN,                   
			TOKENID.STAR,
			TOKENID.AMPERSAND,
			
			TOKENID.COLON,                   
			TOKENID.THIS,
			TOKENID.BASE,
			TOKENID.NULL,
			TOKENID.TRUE,
			TOKENID.FALSE,
			TOKENID.UNKNOWN,
			TOKENID.UNKNOWN,
			TOKENID.UNKNOWN,
			TOKENID.PLUSPLUS,
			TOKENID.MINUSMINUS,
			TOKENID.DOT,
			TOKENID.IMPLICIT,
			TOKENID.EXPLICIT,
			
			TOKENID.UNKNOWN,
			TOKENID.UNKNOWN,
			
			TOKENID.UNKNOWN,
        };

        //------------------------------------------------------------
        // BASENODE Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal BASENODE()
        {
#if DEBUG
            DebugUtil.DebugNodesAdd(this);
            if (this.NodeID == 2082)
            {
                ;
            }
#endif
        }

        //------------------------------------------------------------
        // BASENODE.GetParent
        //
        /// <summary>
        /// <para>VARDECLNODE overrides this method
        /// to return the DeclarationsNode.</para>
        /// <para>Other nodes return the ParentNode.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal virtual BASENODE GetParent()
        {
            return ParentNode;
        }

        //------------------------------------------------------------
        // BASENODE As Methods
        //
        // Define all the concrete casting methods.
        //------------------------------------------------------------

        /// <summary>
        /// <para>(R) Return this node as ACCESSORNODE.</para>
        /// </summary>
        //internal ACCESSORNODE AsACCESSOR
        //{
        //    get { return this as ACCESSORNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is ALIASNAME, return this node as NAMENODE.</para>
        /// </summary>
        internal NAMENODE AsALIASNAME
        {
            get { return (this.Kind == NODEKIND.ALIASNAME ? this as NAMENODE : null); }
        }

        /// <summary>
        /// <para>(R) Return this node as ANONBLOCKNODE.</para>
        /// </summary>
        //internal ANONBLOCKNODE AsANONBLOCK
        //{
        //    get { return this as ANONBLOCKNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is ARRAYINIT, return this node as UNOPNODE.</para>
        /// </summary>
        internal UNOPNODE AsARRAYINIT
        {
            get { return (this.Kind == NODEKIND.ARRAYINIT ? this as UNOPNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as ARRAYTYPENODE.</para>
        /// </summary>
        //internal ARRAYTYPENODE AsARRAYTYPE
        //{
        //    get { return this as ARRAYTYPENODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is ARROW, return this node as BINOPNODE.</para>
        /// </summary>
        internal BINOPNODE AsARROW
        {
            get { return (this.Kind == NODEKIND.ARROW ? this as BINOPNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as ATTRNODE.</para>
        /// </summary>
        //internal ATTRNODE AsATTR
        //{
        //    get { return this as ATTRNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is ATTRARG, return this node as ATTRNODE.</para>
        /// </summary>
        internal ATTRNODE AsATTRARG
        {
            get { return (this.Kind == NODEKIND.ATTRARG ? this as ATTRNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as ATTRDECL.</para>
        /// </summary>
        //internal ATTRDECLNODE AsATTRDECL
        //{
        //    get { return this as ATTRDECLNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as BINOPNODE.</para>
        /// </summary>
        //internal BINOPNODE AsBINOP
        //{
        //    get { return this as BINOPNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as BLOCKNODE.</para>
        /// </summary>
        //internal BLOCKNODE AsBLOCK
        //{
        //    get { return this as BLOCKNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is BREAK, return this node as EXPRSTMTNODE.</para>
        /// </summary>
        internal EXPRSTMTNODE AsBREAK
        {
            get { return (this.Kind == NODEKIND.BREAK ? this as EXPRSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as CALLNODE.</para>
        /// </summary>
        //internal CALLNODE AsCALL
        //{
        //    get { return this as CALLNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as CASENODE.</para>
        /// </summary>
        //internal CASENODE AsCASE
        //{
        //    get { return this as CASENODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is CASELABEL, return this node as UNOPNODE.</para>
        /// </summary>
        internal UNOPNODE AsCASELABEL
        {
            get { return (this.Kind == NODEKIND.CASELABEL ? this as UNOPNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as CATCHNODE.</para>
        /// </summary>
        //internal CATCHNODE AsCATCH
        //{
        //    get { return this as CATCHNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is CHECKED, return this node as LABELSTMTNODE.</para>
        /// </summary>
        internal LABELSTMTNODE AsCHECKED
        {
            get { return (this.Kind == NODEKIND.CHECKED ? this as LABELSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as CLASSNODE.</para>
        /// </summary>
        //internal CLASSNODE AsCLASS
        //{
        //    get { return this as CLASSNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is CONST, return this node as FIELDNODE.</para>
        /// </summary>
        internal FIELDNODE AsCONST
        {
            get { return (this.Kind == NODEKIND.CONST ? this as FIELDNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as CONSTRAINTNODE.</para>
        /// </summary>
        //internal CONSTRAINTNODE AsCONSTRAINT
        //{
        //    get { return this as CONSTRAINTNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as CONSTVALNODE.</para>
        /// </summary>
        //internal CONSTVALNODE AsCONSTVAL
        //{
        //    get { return this as CONSTVALNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is CONTINUE, return this node as EXPRSTMTNODE.</para>
        /// </summary>
        internal EXPRSTMTNODE AsCONTINUE
        {
            get { return (this.Kind == NODEKIND.CONTINUE ? this as EXPRSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is CTOR, return this node as CTORMETHODNODE.</para>
        /// </summary>
        //internal CTORMETHODNODE AsCTOR
        //{
        //    get { return this as CTORMETHODNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as DECLSTMTNODE.</para>
        /// </summary>
        //internal DECLSTMTNODE AsDECLSTMT
        //{
        //    get { return this as DECLSTMTNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as DELGATENODE.</para>
        /// </summary>
        //internal DELEGATENODE AsDELEGATE
        //{
        //    get { return this as DELEGATENODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is DEREF, return this node as CALLNODE.</para>
        /// </summary>
        internal CALLNODE AsDEREF
        {
            get { return (this.Kind == NODEKIND.DEREF ? this as CALLNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is DO, return this node as LOOPSTMTNODE.</para>
        /// </summary>
        internal LOOPSTMTNODE AsDO
        {
            get { return (this.Kind == NODEKIND.DO ? this as LOOPSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is DOT, return this node as BINOPNODE.</para>
        /// </summary>
        internal BINOPNODE AsDOT
        {
            get { return (this.Kind == NODEKIND.DOT ? this as BINOPNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is DTOR, return this node as METHODBASENODE.</para>
        /// </summary>
        internal METHODBASENODE AsDTOR
        {
            get { return (this.Kind == NODEKIND.DTOR ? this as METHODBASENODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is EMPTYSTMT, return this node as STATEMENTNODE.</para>
        /// </summary>
        internal STATEMENTNODE AsEMPTYSTMT
        {
            get { return (this.Kind == NODEKIND.EMPTYSTMT ? this as STATEMENTNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as ENUMNODE.</para>
        /// </summary>
        //internal ENUMNODE AsENUM
        //{
        //    get { return this as ENUMNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as ENUMBERNODE.</para>
        /// </summary>
        //internal ENUMMBRNODE AsENUMMBR
        //{
        //    get { return this as ENUMMBRNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as EXPRSTMTNODE.</para>
        /// </summary>
        //internal EXPRSTMTNODE AsEXPRSTMT
        //{
        //    get { return this as EXPRSTMTNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as FIELDNODE.</para>
        /// </summary>
        //internal FIELDNODE AsFIELD
        //{
        //    get { return this as FIELDNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is FOR, return this node as FORSTMTNODE.</para>
        /// </summary>
        //internal FORSTMTNODE AsFOR
        //{
        //    get { return this as FORSTMTNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as GENERICNAMENODE.</para>
        /// </summary>
        //internal GENERICNAMENODE AsGENERICNAME
        //{
        //    get { return this as GENERICNAMENODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is GOTO, return this node as EXPRSTMTNODE.</para>
        /// </summary>
        internal EXPRSTMTNODE AsGOTO
        {
            get { return (this.Kind == NODEKIND.GOTO ? this as EXPRSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is IF, return this node as IFSTMTNODE.</para>
        /// </summary>
        //internal IFSTMTNODE AsIF
        //{
        //    get { return this as IFSTMTNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as INTERFACENODE.</para>
        /// </summary>
        //internal INTERFACENODE AsINTERFACE
        //{
        //    get { return this as INTERFACENODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is LABEL, return this node as LABELSTMTNODE.</para>
        /// </summary>
        //internal LABELSTMTNODE AsLABEL
        //{
        //    get { return this as LABELSTMTNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is LIST, return this node as BINOPNODE.</para>
        /// </summary>
        internal BINOPNODE AsLIST
        {
            get { return (this.Kind == NODEKIND.LIST ? this as BINOPNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is LOCK, return this node as LOOPSTMTNODE.</para>
        /// </summary>
        internal LOOPSTMTNODE AsLOCK
        {
            get { return (this.Kind == NODEKIND.LOCK ? this as LOOPSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as MEMBERNODE.</para>
        /// </summary>
        //internal MEMBERNODE AsMEMBER
        //{
        //    get { return this as MEMBERNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as METHODNODE.</para>
        /// </summary>
        //internal METHODNODE AsMETHOD
        //{
        //    get { return this as METHODNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as NAMENODE.</para>
        /// </summary>
        //internal NAMENODE AsNAME
        //{
        //    get { return this as NAMENODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as NAMEDTYPENODE.</para>
        /// </summary>
        //internal NAMEDTYPENODE AsNAMEDTYPE
        //{
        //    get { return this as NAMEDTYPENODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as NAMESPACENODE.</para>
        /// </summary>
        //internal NAMESPACENODE AsNAMESPACE
        //{
        //    get { return this as NAMESPACENODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as NESTEDTYPENODE.</para>
        /// </summary>
        //internal NESTEDTYPENODE AsNESTEDTYPE
        //{
        //    get { return this as NESTEDTYPENODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as NEWNODE.</para>
        /// </summary>
        //internal NEWNODE AsNEW
        //{
        //    get { return this as NEWNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as NULLABLETYPENODE.</para>
        /// </summary>
        //internal NULLABLETYPENODE AsNULLABLETYPE
        //{
        //    get { return this as NULLABLETYPENODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is OP, return this node as BASENODE.</para>
        /// </summary>
        internal BASENODE AsOP
        {
            get { return (this.Kind == NODEKIND.OP ? this as BASENODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as OPENNAMENODE.</para>
        /// </summary>
        //internal OPENNAMENODE AsOPENNAME
        //{
        //    get { return this as OPENNAMENODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is OPENTYPE, return this node as NAMEDTYPENODE.</para>
        /// </summary>
        internal NAMEDTYPENODE AsOPENTYPE
        {
            get { return (this.Kind == NODEKIND.OPENTYPE ? this as NAMEDTYPENODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is OPERATOR, return this node as OPERATORMETHODNODE.</para>
        /// </summary>
        //internal OPERATORMETHODNODE AsOPERATOR
        //{
        //    get { return this as OPERATORMETHODNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as PARAMETERNODE.</para>
        /// </summary>
        //internal PARAMETERNODE AsPARAMETER
        //{
        //    get
        //    {
        //        if (this == null || this.Kind != NODEKIND.PARAMETER) return null;
        //        return this as PARAMETERNODE;
        //    }
        //}

        /// <summary>
        /// <para>(R) return this node as PARTIALMEMBERNODE.</para>
        /// </summary>
        //internal PARTIALMEMBERNODE AsPARTIALMEMBER
        //{
        //    get { return this as PARTIALMEMBERNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as POINTERTYPENODE.</para>
        /// </summary>
        //internal POINTERTYPENODE AsPOINTERTYPE
        //{
        //    get { return this as POINTERTYPENODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as PREDEFINEDTYPENODE.</para>
        /// </summary>
        //internal PREDEFINEDTYPENODE AsPREDEFINEDTYPE
        //{
        //    get { return this as PREDEFINEDTYPENODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as PROPERTYNODE.</para>
        /// </summary>
        //internal PROPERTYNODE AsPROPERTY
        //{
        //    get { return this as PROPERTYNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is INDEXER, return this node as PROPERTYNODE.</para>
        /// </summary>
        internal PROPERTYNODE AsINDEXER
        {
            get { return (this.Kind == NODEKIND.INDEXER ? this as PROPERTYNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is RETURN, return this node as EXPRSTMTNODE.</para>
        /// </summary>
        internal EXPRSTMTNODE AsRETURN
        {
            get { return (this.Kind == NODEKIND.RETURN ? this as EXPRSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is THROW, return this node as EXPRSTMTNODE.</para>
        /// </summary>
        internal EXPRSTMTNODE AsTHROW
        {
            get { return (this.Kind == NODEKIND.THROW ? this as EXPRSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is TRY, return this node as TRYSTMTNODE.</para>
        /// </summary>
        //internal TRYSTMTNODE AsTRY
        //{
        //    get { return this as TRYSTMTNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as TYPEWITHATTRNODE.</para>
        /// </summary>
        //internal TYPEWITHATTRNODE AsTYPEWITHATTR
        //{
        //    get { return this as TYPEWITHATTRNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as STRUCTNODE.</para>
        /// </summary>
        //internal STRUCTNODE AsSTRUCT
        //{
        //    get { return this as STRUCTNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is SWITCH, return this node as SWITCHSTMTNODE.</para>
        /// </summary>
        //internal SWITCHSTMTNODE AsSWITCH
        //{
        //    get { return this as SWITCHSTMTNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as UNOPNODE.</para>
        /// </summary>
        //internal UNOPNODE AsUNOP
        //{
        //    get { return this as UNOPNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is UNSAFE, return this node as LABELSTMTNODE.</para>
        /// </summary>
        internal LABELSTMTNODE AsUNSAFE
        {
            get { return (this.Kind == NODEKIND.UNSAFE ? this as LABELSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) return this node as USINGNODE.</para>
        /// </summary>
        //internal USINGNODE AsUSING
        //{
        //    get { return this as USINGNODE; }
        //}

        /// <summary>
        /// <para>(R) return this node as VERDECLNODE.</para>
        /// </summary>
        //internal VARDECLNODE AsVARDECL
        //{
        //    get { return this as VARDECLNODE; }
        //}

        /// <summary>
        /// <para>(R) If Kind is WHILE, return this node as LOOPSTMTNODE.</para>
        /// </summary>
        internal LOOPSTMTNODE AsWHILE
        {
            get { return (this.Kind == NODEKIND.WHILE ? this as LOOPSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R) If Kind is YIELD, return this node as EXPRSTMTNODE.</para>
        /// </summary>
        internal EXPRSTMTNODE AsYIELD
        {
            get { return (this.Kind == NODEKIND.YIELD ? this as EXPRSTMTNODE : null); }
        }

        /// <summary>
        /// <para>(R)(CS3)</para>
        /// </summary>
        internal UNOPNODE AsCOLLECTIONINIT
        {
            get { return (this.Kind == NODEKIND.COLLECTIONINIT ? this as UNOPNODE : null); }
        }

        /// <summary>
        /// <para>Little noop to enable looping:</para>
        /// <para>return this node as BASENODE.</para>
        /// </summary>
        internal BASENODE AsBASE
        {
            get { return this; }
        }

        //------------------------------------------------------------
        // BASENODE
        //
        // Some more complex casting helpers
        //------------------------------------------------------------

        /// <summary>
        /// <para>(R) If this node belongs to AGGREGATE group,
        /// return this node as AGGREGATENODE.</para>
        /// </summary>
        internal AGGREGATENODE AsAGGREGATE
        {
            get
            {
                if (this == null || !InGroup(NODEGROUP.AGGREGATE))
                {
                    return null;
                }
                return this as AGGREGATENODE;
            }
        }

        /// <summary>
        /// <para>(R) If this node belongs to METHOD group,
        /// return this node as METHODBASENODE.</para>
        /// </summary>
        internal METHODBASENODE AsANYMETHOD
        {
            get
            {
                if (this == null || !InGroup(NODEGROUP.METHOD))
                {
                    return null;
                }
                return this as METHODBASENODE;
            }
        }

        /// <summary>
        /// <para>(R) If this node belongs to PROPERTY group,
        /// return this node as PROPERTYNODE.</para>
        /// </summary>
        internal PROPERTYNODE AsANYPROPERTY
        {
            get
            {
                if (this == null || !InGroup(NODEGROUP.PROPERTY))
                {
                    return null;
                }
                return this as PROPERTYNODE;
            }
        }

        /// <summary>
        /// <para>(R) If this node belongs to FIELD group,
        /// return this node as FIELDNODE.</para>
        /// </summary>
        internal FIELDNODE asANYFIELD()
        {
            if (this == null || !InGroup(NODEGROUP.FIELD))
            {
                return null;
            }
            return this as FIELDNODE;
        }

        /// <summary>
        /// <para>(R) If this node belongs to MEMBER group,
        /// return this node as MEMBERNODE.</para>
        /// </summary>
        internal MEMBERNODE AsANYMEMBER
        {
            get
            {
                if (this == null || !InGroup(NODEGROUP.MEMBER))
                {
                    return null;
                }
                return this as MEMBERNODE;
            }
        }

        /// <summary>
        /// (R) If IsAnyName is true, return this node as NAMENODE.
        /// </summary>
        internal NAMENODE AsANYNAME
        {
            get
            {
                if (this == null || !IsAnyName)
                {
                    return null;
                }
                return this as NAMENODE;
            }
        }

        /// <summary>
        /// (R) If IsSingleNameis true, return this node as NAMENODE.
        /// </summary>
        internal NAMENODE AsSingleName
        {
            get
            {
                if (this == null || !IsSingleName)
                {
                    return null;
                }
                return this as NAMENODE;
            }
        }

        /// <summary>
        /// (R) If InBinOpGroup is true, return this node as BINOPNODE.
        /// </summary>
        internal BINOPNODE AsANYBINOP
        {
            get
            {
                if (this == null || !IsAnyBinOp)
                {
                    return null;
                }
                return this as BINOPNODE;
            }
        }

        /// <summary>
        /// (R) If IsBinOpEx is true, return this node as BINOPNODE.
        /// </summary>
        internal BINOPNODE AsANYBINARYOPERATOR
        {
            get
            {
                if (this == null || !IsAnyBinaryOperator)
                {
                    return null;
                }
                return this as BINOPNODE;
            }
        }

        /// <summary>
        /// (R) If IsAnyUnOp is true, return this node as UNOPNODE.
        /// </summary>
        internal UNOPNODE AsANYUNOP
        {
            get
            {
                if (this == null || !IsAnyUnOp)
                {
                    return null;
                }
                return this as UNOPNODE;
            }
        }

        /// <summary>
        /// (R) If Kind is CALL or DEREF, return this node as CALLNODE.
        /// </summary>
        internal CALLNODE AsANYCALL
        {
            get
            {
                if (this == null || Kind != NODEKIND.CALL && Kind != NODEKIND.DEREF)
                {
                    return null;
                }
                return this as CALLNODE;
            }
        }

        /// <summary>
        /// (R) If this node belongs to STATEMENT group、return this node as STATEMENTNODE.
        /// </summary>
        internal STATEMENTNODE AsANYSTATEMENT
        {
            get
            {
                if (this == null || !InGroup(NODEGROUP.STATEMENT))
                {
                    return null;
                }
                return this as STATEMENTNODE;
            }
        }

        /// <summary>
        /// (R) If IsAnyType is true, return this node as TYPEBASENODE.
        /// </summary>
        internal TYPEBASENODE AsANYTYPE
        {
            get
            {
                if (this == null || !IsAnyType)
                {
                    return null;
                }
                return this as TYPEBASENODE;
            }
        }

        /// <summary>
        /// (R) If IsAnyExprStmt is true, return this node as EXPRSTMTNODE.
        /// </summary>
        internal EXPRSTMTNODE AsANYEXPRSTMT
        {
            get
            {
                if (this == null || !IsAnyExprStmt)
                {
                    return null;
                }
                return this as EXPRSTMTNODE;
            }
        }

        /// <summary>
        /// (R) If IsAnyLabelStmt is true, return this node as LABELSTMTNODE.
        /// </summary>
        internal LABELSTMTNODE AsANYLABELSTMT
        {
            get
            {
                if (this == null || !IsAnyLabelStmt)
                {
                    return null;
                }
                return this as LABELSTMTNODE;
            }
        }

        /// <summary>
        /// (R) If IsAnyLoopStmt is true, return this node as LOOPSTMTNODE.
        /// </summary>
        internal LOOPSTMTNODE AsANYLOOPSTMT
        {
            get
            {
                if (this == null || !IsAnyLoopStmt)
                {
                    return null;
                }
                return this as LOOPSTMTNODE;
            }
        }

        /// <summary>
        /// (R) If IsAnyAttr is true, return this node as ATTRNODE.
        /// </summary>
        internal ATTRNODE AsANYATTR
        {
            get
            {
                if (this == null || !IsAnyAttr)
                {
                    return null;
                }
                return this as ATTRNODE;
            }
        }

        /// <summary>
        /// <para>(R) If IsAnyType is true, return this node as TYPEBASENODE.
        /// （Same to AsANYTYPE.）</para>
        /// </summary>
        internal TYPEBASENODE AsTYPEBASE
        {
            get
            {
                return AsANYTYPE;
            }
        }

        //------------------------------------------------------------
        // BASENODE Is* Properties
        //------------------------------------------------------------

        /// <summary>
        /// (R) If this node is a name node of any kind, return true.
        /// </summary>
        /// <remarks>
        /// if you change this, make sure to update CNameBinder in the Language Service
        /// </remarks>
        internal bool IsAnyName // IsAnyName()
        {
            get
            {
                return (
                    Kind == NODEKIND.NAME ||
                    Kind == NODEKIND.GENERICNAME ||
                    Kind == NODEKIND.ALIASNAME ||
                    Kind == NODEKIND.OPENNAME);
            }
        }

        /// <summary>
        /// (R)
        /// </summary>
        internal bool IsSingleName  // IsSingleName()
        {
            get { return Kind == NODEKIND.NAME || Kind == NODEKIND.ALIASNAME; }
        }

        /// <summary>
        /// (R)
        /// </summary>
        internal bool IsMissingName // IsMissingName()
        {
            get { return IsSingleName && ((Flags & NODEFLAGS.NAME_MISSING) != 0); }
        }

        /// <summary>
        /// <para>(R) True if this node is in BINOP group.
        /// </summary>
        internal bool IsAnyBinOp    // IsAnyBinOp()
        {
            get { return InGroup(NODEGROUP.BINOP); }
        }

        /// <summary>
        /// (R) Kind is BINOP or CALL or DEREF.
        /// </summary>
        internal bool IsAnyBinaryOperator   // IsAnyBinaryOperator()
        {
            get
            {
                return (
                    Kind == NODEKIND.BINOP ||
                    Kind == NODEKIND.CALL ||
                    Kind == NODEKIND.DEREF);
            }
        }

        /// <summary>
        /// (R) True if ARRAYINIT, CASELABEL or UNOP.
        /// </summary>
        internal bool IsAnyUnOp // IsAnyUnOp()
        {
            get
            {
                return (
                  Kind == NODEKIND.ARRAYINIT ||
                  Kind == NODEKIND.CASELABEL ||
                  Kind == NODEKIND.UNOP);
            }
        }

        /// <summary>
        /// (R) True if CALL or DEREF.
        /// </summary>
        internal bool IsAnyCall // IsAnyCall()
        {
            get { return Kind == NODEKIND.CALL || Kind == NODEKIND.DEREF; }
        }

        /// <summary>(R)</summary>
        /// <remarks>
        /// if you change this, make sure to update CTypeBinder in the Language Service
        /// </remarks>
        internal bool IsAnyType // IsAnyType()
        {
            get
            {
                switch (Kind)
                {
                    case NODEKIND.PREDEFINEDTYPE:
                    case NODEKIND.NAMEDTYPE:
                    case NODEKIND.ARRAYTYPE:
                    case NODEKIND.POINTERTYPE:
                    case NODEKIND.OPENTYPE:
                    case NODEKIND.TYPEWITHATTR:
                    case NODEKIND.NULLABLETYPE:
                    case NODEKIND.IMPLICITTYPE: // CS3
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// (R)
        /// </summary>
        internal bool IsAnyExprStmt // IsAnyExprStmt()
        {
            get
            {
                return (
                    Kind == NODEKIND.BREAK ||
                    Kind == NODEKIND.CONTINUE ||
                    Kind == NODEKIND.EXPRSTMT ||
                    Kind == NODEKIND.GOTO ||
                    Kind == NODEKIND.RETURN ||
                    Kind == NODEKIND.YIELD ||
                    Kind == NODEKIND.THROW);
            }
        }

        /// <summary>
        /// (R) True if CHECKED, LABEL or UNSAFE.
        /// </summary>
        internal bool IsAnyLabelStmt    // IsAnyLabelStmt()
        {
            get { return Kind == NODEKIND.CHECKED || Kind == NODEKIND.LABEL || Kind == NODEKIND.UNSAFE; }
        }

        /// <summary>
        /// (R) True if LOCK, WHILE or DO.
        /// </summary>
        internal bool IsAnyLoopStmt // IsAnyLoopStmt()
        {
            get { return Kind == NODEKIND.LOCK || Kind == NODEKIND.WHILE || Kind == NODEKIND.DO; }
        }

        /// <summary>
        /// (R) True if ATTR or ATTRARG.
        /// </summary>
        internal bool IsAnyAttr // IsAnyAttr()
        {
            get { return Kind == NODEKIND.ATTR || Kind == NODEKIND.ATTRARG; }
        }

        /// <summary>
        /// (R) True if in MEMBER group.
        /// </summary>
        internal bool IsAnyMember   // IsAnyMember()
        {
            get { return InGroup(NODEGROUP.MEMBER); }
        }

        /// <summary>
        /// (R) True if in AGGREGATE group.
        /// </summary>
        internal bool IsAnyAggregate    // IsAnyAggregate()
        {
            get { return InGroup(NODEGROUP.AGGREGATE); }
        }

        /// <summary>
        /// (R) True if DOT and the first operand is ALIASNAME.
        /// </summary>
        internal bool IsDoubleColon // IsDblColon()
        {
            get
            {
                return Kind == NODEKIND.DOT && AsDOT.Operand1.Kind == NODEKIND.ALIASNAME;
            }
        }

        /// <summary>(R)</summary>
        /// <remarks>
        /// Another joy of anonymous methods: 
        /// A STATEMENT node (i.e. a NODEKIND.BLOCK) in this case
        /// is not always a statement anymore.
        /// We need to check the tree context.
        /// </remarks>
        internal bool IsStatement   // IsStatement()
        {
            get
            {
                return
                    InGroup(NODEGROUP.STATEMENT) &&
                    (ParentNode == null ||
                        (ParentNode.Kind != NODEKIND.ANONBLOCK &&
                         ParentNode.Kind != NODEKIND.TRY &&
                         ParentNode.Kind != NODEKIND.UNSAFE &&
                         ParentNode.Kind != NODEKIND.CHECKED &&
                         ParentNode.Kind != NODEKIND.CATCH &&
                         !ParentNode.InGroup(NODEGROUP.METHOD)));
            }
        }

        /// <summary>
        /// (R) True if IsStatement is true or BLOCK.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// BASENODE.IsStatment doesn't return true on a block in certain cases
        /// (like the block of a method)
        /// </remarks>
        internal bool IsStatementOrBlock    // IsStatementOrBlock()
        {
            get { return Kind == NODEKIND.BLOCK || IsStatement; }
        }

        /// <summary>
        /// (R) True if this node represents the last name (including single name).
        /// </summary>
        internal NAMENODE LastNameOfDottedName  // LastNameOfDottedName()
        {
            get
            {
                if (IsAnyName)
                {
                    return AsANYNAME;
                }
                DebugUtil.Assert(Kind == NODEKIND.DOT);
                return AsDOT.Operand2.AsANYNAME;
            }
        }

        //------------------------------------------------------------
        // BASENODE Other Properties
        //------------------------------------------------------------
        /// <summary>
        /// (R) Not implemented.
        /// </summary>
        internal long Glyph // GetGlyph ()
        {
            get
            {
                DebugUtil.Assert(false);
                return 0;
            }
        }

        //  Handy functions

        //------------------------------------------------------------
        // BASENODE.InGroup
        //
        /// <summary>
        /// Determine if this node belongs to the specified group.
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InGroup(NODEGROUP groups)
        {
            groups &= NodeGroupTable[(uint)Kind];
            return (
                (groups != 0) &&
                (   (groups & ~(NODEGROUP.BREAKABLE | NODEGROUP.CONTINUABLE)) != 0 ||
                    (Flags & (NODEFLAGS.USING_DECL | NODEFLAGS.FIXED_DECL)) == 0 ||
                    Kind != NODEKIND.FOR
                ));
        }

        //------------------------------------------------------------
        // BASENODE.GetOperatorToken
        //
        /// <summary></summary>
        /// <param name="iOp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TOKENID GetOperatorToken(long iOp)
        {
            return OperatorToTokenTable[iOp];
        }

        //------------------------------------------------------------
        // BASENODE.GetContainingFileName
        //
        /// <summary>
        /// return SouceFileName of RootNameSpaceNode of this node.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetContainingFileName(bool fullPath)
        {
            // The top of the parse tree must be a namespace node.
            return IOUtil.SelectFileName(GetRootNamespace().SourceFileInfo, fullPath);
        }

        //------------------------------------------------------------
        // BASENODE.GetRootNamespace
        //
        /// <summary>
        /// Search the top level parent node and return it as NAMESPACENODE.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NAMESPACENODE GetRootNamespace()
        {
            BASENODE pNode = this;

            while (pNode.ParentNode != null)
            {
                pNode = pNode.ParentNode;
            }
            return (pNode as NAMESPACENODE);
        }

        //------------------------------------------------------------
        // BASENODE.IsStatementOrBlock
        // 
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------

        //------------------------------------------------------------
        // BASENODE.GetFirstNonAttributeToken
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetFirstNonAttributeToken()
        {
            if (this.InGroup(NODEGROUP.AGGREGATE))
            {
                return this.AsAGGREGATE.StartTokenIndex;
            }
            else if (this.InGroup(NODEGROUP.MEMBER))
            {
                return this.AsANYMEMBER.StartTokenIndex;
            }
            else if (this.Kind == NODEKIND.DELEGATE)
            {
                return (this as DELEGATENODE).StartTokenIndex;
            }
            return this.TokenIndex;
        }

        //------------------------------------------------------------
        // BASENODE.GetFirstNonAttributeToken
        //
        /// <summary>
        /// <para>GetDefaultAttrLocation in sscli.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ATTRTARGET GetDefaultAttributeTarget()
        {
            // Used as defaults:
            // AL_PARAM, AL_TYPE, AL_METHOD, AL_FIELD, AL_PROPERTY, AL_EVENT, AL_TYPEVAR
            // Not used as defaults:
            // AL_ASSEMBLY, AL_MODULE, AL_RETURN, AL_UNKNOWN

            if (this.InGroup(NODEGROUP.AGGREGATE) || this.Kind == NODEKIND.DELEGATE)
            {
                return ATTRTARGET.TYPE;
            }
            else if (this.Kind == NODEKIND.PROPERTY)
            {
                if ((this.NodeFlagsEx & NODEFLAGS.EX_EVENT) != 0)
                {
                    return ATTRTARGET.EVENT;
                }
                else
                {
                    return ATTRTARGET.PROPERTY;
                }
            }
            else if (this.Kind == NODEKIND.FIELD)
            {
                if ((this.NodeFlagsEx & NODEFLAGS.EX_EVENT) != 0)
                {
                    return ATTRTARGET.EVENT;
                }
                else
                {
                    return ATTRTARGET.FIELD;
                }
            }
            else if (this.Kind == NODEKIND.PARAMETER)
            {
                return ATTRTARGET.PARAMETER;
            }
            else if (this.InGroup(NODEGROUP.METHOD) || this.Kind == NODEKIND.ACCESSOR)
            {
                return ATTRTARGET.METHOD;
            }
            else if (this.Kind == NODEKIND.TYPEWITHATTR)
            {
                return ATTRTARGET.TYPEVAR;
            }
            return ATTRTARGET.NONE;
        }

        // Name and key construction methods (NEW, BETTER, FASTER)

        //------------------------------------------------------------
        // BASENODE.AppendNameText
        // 
        /// <summary>
        /// Names that are keywords are @-escaped unless pNameTable is NULL
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="nameManager"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AppendNameText(
            StringBuilder stringBuilder,
            CNameManager nameManager)
        {
            return AppendNameTextToKey(stringBuilder, nameManager);
        }

        //------------------------------------------------------------
        // BASENODE.AppendNameTextToPrototype
        //
        /// <summary>
        /// Names that are keywords are @-escaped unless pNameTable is NULL
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="nameManager"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AppendNameTextToPrototype(
            StringBuilder stringBuilder,
            CNameManager nameManager)
        {
            if (this.IsAnyName)
            {
                if (PrevNode != null)
                {
                    if (PrevNode.AppendNameTextToPrototype(stringBuilder, nameManager))
                    {
                        if (PrevNode.Kind == NODEKIND.ALIASNAME)
                        {
                            stringBuilder.Append("::");
                        }
                        else
                        {
                            stringBuilder.Append('.');
                        }
                    }
                }

                int dummy;
                if (nameManager != null &&
                    nameManager.IsKeyword(this.AsANYNAME.Name, LangVersionEnum.Default, out dummy))
                {
                    stringBuilder.Append('@');
                }
                stringBuilder.Append(this.AsANYNAME.Name);

                if (this.Kind == NODEKIND.GENERICNAME)
                {
                    AppendParametersToPrototype(
                        PARAMTYPES.GENERIC,
                        (this as GENERICNAMENODE).ParametersNode,
                        stringBuilder,
                        false);
                }
                return true;
            }
            else if (this.Kind == NODEKIND.DOT)
            {
                this.AsDOT.Operand1.AppendNameTextToPrototype(stringBuilder, nameManager);
                if (this.IsDoubleColon)
                {
                    stringBuilder.Append("::");
                }
                else
                {
                    stringBuilder.Append('.');
                }
                this.AsDOT.Operand2.AppendNameTextToPrototype(stringBuilder, nameManager);
                return true;
            }
            DebugUtil.Assert(false, "AppendNameText called on non-name node...");
            return false;
        }

        //------------------------------------------------------------
        // BASENODE.AppendNameTextToKey
        // 
        /// <summary>
        /// <para>Names that are keywords are @-escaped unless pNameTable is NULL</para>
        /// <para>If this node is a name node of any kind, add the name to stringBuilder.
        /// If the name is same as a keyword, add prefix '@'.</para>
        /// <para>Otherwise, return false and do nothing.</para>
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="nameManager"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AppendNameTextToKey(
            StringBuilder stringBuilder,
            CNameManager nameManager)
        {
            if (this.IsAnyName)
            {
                if (PrevNode != null)
                {
                    if (PrevNode.AppendNameTextToKey(stringBuilder, nameManager))
                    {
                        if (PrevNode.Kind == NODEKIND.ALIASNAME)
                        {
                            stringBuilder.Append("::");
                        }
                        else
                        {
                            stringBuilder.Append('.');
                        }
                    }
                }

                // If same as a keyword, add prefix '@'.
                int dummy;
                if (nameManager != null &&
                    nameManager.IsKeyword(this.AsANYNAME.Name, LangVersionEnum.Default, out dummy))
                {
                    stringBuilder.Append('@');
                }
                stringBuilder.Append(this.AsANYNAME.Name);

                if (this.Kind == NODEKIND.GENERICNAME)
                {
                    AppendParametersToKey(
                        PARAMTYPES.GENERIC,
                        (this as GENERICNAMENODE).ParametersNode,
                        stringBuilder);
                }
                return true;
            }
            else if (this.Kind == NODEKIND.DOT)
            {
                this.AsDOT.Operand1.AppendNameTextToKey(stringBuilder, nameManager);
                if (this.IsDoubleColon)
                {
                    stringBuilder.Append("::");
                }
                else
                {
                    stringBuilder.Append('.');
                }
                this.AsDOT.Operand2.AppendNameTextToKey(stringBuilder, nameManager);
                return true;
            }
            DebugUtil.Assert(false, "AppendNameTextToKey called on non-name node...");
            return false;
        }

        //------------------------------------------------------------
        // BASENODE.AppendTypeText
        //
        /// <summary></summary>
        /// <param name="stringBuilder"></param>
        /// <param name="nameManager"></param>
        /// <returns></returns>
        /// <remarks>Ditto.</remarks>
        //------------------------------------------------------------
        internal bool AppendTypeText(StringBuilder stringBuilder, CNameManager nameManager)
        {
            // Back up to a type node
            BASENODE aggNodeOuter;
            for (aggNodeOuter = this.GetParent();
                aggNodeOuter != null;
                aggNodeOuter = aggNodeOuter.GetParent())
            {
                if (aggNodeOuter.InGroup(NODEGROUP.AGGREGATE))
                {
                    break;
                }
            }

            // If nothing is added, then this isn't a type or contained by one
            bool br = false;
            bool needDot = false;

            if (aggNodeOuter != null)
            {
                // Put it's type text in the bstr first...
                if (!aggNodeOuter.AppendTypeText(stringBuilder, nameManager)) return false;

                needDot = true;
            }

            // Now this, if it is in fact a type itself
            if (this.InGroup(NODEGROUP.AGGREGATE) || this.Kind == NODEKIND.DELEGATE)
            {
                if (needDot)
                {
                    stringBuilder.Append('.');
                }

                NAMENODE nameNode = (this.Kind == NODEKIND.DELEGATE) ?
                    (this as DELEGATENODE).NameNode :
                    this.AsAGGREGATE.NameNode;

                int dummy;
                if (nameManager != null &&
                    nameManager.IsKeyword(nameNode.Name, LangVersionEnum.Default, out dummy))
                {
                    stringBuilder.Append('@');
                }

                stringBuilder.Append(nameNode.Name);
                BASENODE typeParams = (this.Kind == NODEKIND.DELEGATE) ?
                    (this as DELEGATENODE).TypeParametersNode : this.AsAGGREGATE.TypeParametersNode;
                if (typeParams != null)
                {
                    AppendParametersToKey(PARAMTYPES.GENERIC, typeParams, stringBuilder);
                }
                br = true;
            }

            return br;
        }

        //------------------------------------------------------------
        // BASENODE.AppendPrototype
        //
        /// <summary>
        /// If this node belongs to KEYED group, create a name according to flags.
        /// </summary>
        /// <param name="nameManager"></param>
        /// <param name="protoFlags"></param>
        /// <param name="stringBuilder"></param>
        /// <param name="escapedName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AppendPrototype(
            CNameManager nameManager,
            PROTOFLAGS protoFlags,
            StringBuilder stringBuilder,
            bool escapedName) // = false
        {
            // Must be in the group of key-able nodes...
            if (!this.InGroup(NODEGROUP.KEYED))
            {
                return false;
            }

            // NODEGROUP.KEYED contains:
            //  NODEKIND.CLASS            *
            //  NODEKIND.CTOR
            //  NODEKIND.DELEGATE         *
            //  NODEKIND.DTOR
            //  NODEKIND.ENUM             *
            //  NODEKIND.ENUMMBR
            //  NODEKIND.INTERFACE        *
            //  NODEKIND.METHOD
            //  NODEKIND.NAMESPACE        *
            //  NODEKIND.OPERATOR
            //  NODEKIND.PROPERTY
            //  NODEKIND.INDEXER
            //  NODEKIND.STRUCT           *
            //  NODEKIND.VARDECL      NOTE:  You'll notice FIELD and CONST do not belong;
            //                               only VARDECL, which is used for each field/const declaration

            //bool br;
            CNameManager escNameManager = escapedName ? nameManager : null;

            // The starred ones are nodes for which the key will do fine if PROTOFLAGS.FULLNAME is specified,
            // and just this node's name will do otherwise...

            //--------------------------------------------------
            // TYPE group（CLASS, DELEGATE, ENUM, INTERFACE, STRUCT）or NAMESPACE
            //--------------------------------------------------
            if (this.InGroup(NODEGROUP.TYPE) || this.Kind == NODEKIND.NAMESPACE)
            {
                if ((protoFlags & PROTOFLAGS.FULLNAME) != 0)
                {
                    // Search the nearest KEYED node which include this node.
                    BASENODE parentNode = this.ParentNode;
                    for (;
                        parentNode != null && !parentNode.InGroup(NODEGROUP.KEYED);
                        parentNode = parentNode.ParentNode)
                    {
                        ;
                    }

                    if (parentNode != null)
                    {
                        if (!parentNode.AppendPrototype(nameManager, protoFlags, stringBuilder, escapedName))
                        {
                            return false;
                        }
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append('.');
                        }
                    }

                    string keyString;
                    if (!this.BuildKey(nameManager, false, false, out keyString)) // no <,,,>
                    {
                        return false;
                    }

                    if (escapedName)
                    {
                        escNameManager.AppendPossibleKeyword(keyString, stringBuilder);
                    }
                    else
                    {
                        stringBuilder.Append(keyString);
                    }

                }
                else
                {
                    if (this.Kind == NODEKIND.NAMESPACE)
                    {
                        return (this as NAMESPACENODE).NameNode.AppendNameTextToPrototype(
                            stringBuilder,
                            escNameManager);
                    }

                    if (this.Kind == NODEKIND.DELEGATE)
                    {
                        (this as DELEGATENODE).NameNode.AppendNameTextToPrototype(
                            stringBuilder,
                            escNameManager);
                    }
                    else
                    {
                        this.AsAGGREGATE.NameNode.AppendNameTextToPrototype(
                            stringBuilder,
                            escNameManager);
                    }
                }

                if ((protoFlags & PROTOFLAGS.TYPEVARS) != 0)
                {
                    if ((this.Kind == NODEKIND.DELEGATE) &&
                        (this as DELEGATENODE).TypeParametersNode != null)
                    {
                        AppendParametersToPrototype(
                            PARAMTYPES.GENERIC,
                            (this as DELEGATENODE).TypeParametersNode,
                            stringBuilder,
                            false);
                    }
                    else if (
                        this.InGroup(NODEGROUP.AGGREGATE) &&
                        this.AsAGGREGATE.TypeParametersNode != null)
                    {
                        AppendParametersToPrototype(
                            PARAMTYPES.GENERIC,
                            this.AsAGGREGATE.TypeParametersNode,
                            stringBuilder,
                            false);
                    }
                }

                if ((protoFlags & PROTOFLAGS.FAILONMISSINGNAME) != 0)
                {
                    if (stringBuilder.ToString().IndexOf('?') >= 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            // Okay, this is a member.
            // If PROTOFLAGS.FULLNAME or PROTOFLAGS.TYPENAME is provided, get our parent's prototype.
            if ((protoFlags & (PROTOFLAGS.FULLNAME | PROTOFLAGS.TYPENAME)) != 0)
            {
                BASENODE parentNode = this.ParentNode;
                for (;
                    parentNode != null && !parentNode.InGroup(NODEGROUP.KEYED);
                    parentNode = parentNode.ParentNode)
                {
                    ;
                }

                if (parentNode.AppendPrototype(nameManager, protoFlags, stringBuilder, escapedName))
                {
                    return false;
                }

                stringBuilder.Append('.');
            }

            // Okay, now do the right thing based on the kind of this node...
            switch (this.Kind)
            {
                case NODEKIND.ENUMMBR:
                    if (escapedName)
                    {
                        nameManager.AppendPossibleKeyword(
                            (this as ENUMMBRNODE).NameNode.Name,
                            stringBuilder);
                    }
                    else
                    {
                        stringBuilder.Append((this as ENUMMBRNODE).NameNode.Name);
                    }
                    break;

                case NODEKIND.VARDECL:
                    if (escapedName)
                    {
                        nameManager.AppendPossibleKeyword(
                            (this as VARDECLNODE).NameNode.Name,
                            stringBuilder);
                    }
                    else
                    {
                        stringBuilder.Append((this as VARDECLNODE).NameNode.Name);
                    }
                    break;

                case NODEKIND.METHOD:
                    {
                        METHODNODE methNode = this as METHODNODE;
                        if (methNode.NameNode.IsAnyName)
                        {
                            if (escapedName)
                            {
                                nameManager.AppendPossibleKeyword(
                                    methNode.NameNode.AsANYNAME.Name,
                                    stringBuilder);
                            }
                            else
                            {
                                stringBuilder.Append(methNode.NameNode.AsANYNAME.Name);
                            }

                            if ((protoFlags & PROTOFLAGS.TYPEVARS) != 0 &&
                                (methNode.NameNode.Kind == NODEKIND.GENERICNAME))
                            {
                                AppendParametersToPrototype(
                                    PARAMTYPES.GENERIC,
                                    (methNode.NameNode as GENERICNAMENODE).ParametersNode,
                                    stringBuilder,
                                    false);
                            }
                        }
                        else
                        {
                            DebugUtil.Assert(methNode.NameNode.Kind == NODEKIND.DOT);
                            if ((protoFlags & PROTOFLAGS.FULLNAME) == 0)
                            {
                                methNode.NameNode.AsDOT.Operand1.AppendNameTextToPrototype(
                                    stringBuilder,
                                    escNameManager);
                                stringBuilder.Append('.');
                            }

                            if ((protoFlags & PROTOFLAGS.TYPEVARS) != 0)
                            {
                                methNode.NameNode.LastNameOfDottedName.AppendNameTextToPrototype(
                                    stringBuilder,
                                    escNameManager);
                            }
                            else
                            {
                                if (escapedName)
                                {
                                    nameManager.AppendPossibleKeyword(
                                        methNode.NameNode.LastNameOfDottedName.Name,
                                        stringBuilder);
                                }
                                else
                                {
                                    stringBuilder.Append(methNode.NameNode.LastNameOfDottedName.Name);
                                }
                            }
                        }

                        if ((protoFlags & PROTOFLAGS.PARAMETERS) != 0)
                        {
                            AppendParametersToPrototype(
                                PARAMTYPES.METHOD,
                                (this as METHODNODE).ParametersNode,
                                stringBuilder,
                                (protoFlags & PROTOFLAGS.PARAMETER_NAME) != 0);
                        }
                    }
                    break;

                case NODEKIND.OPERATOR:
                    {
                        string opStr;
                        OPERATOR opId = (this as OPERATORMETHODNODE).Operator;

                        // Map operator to operator name.  implicit/explicit must be
                        // handled differently.
                        opStr = nameManager.GetTokenText(OperatorToTokenTable[(int)opId]);
                        if (opId == OPERATOR.IMPLICIT || opId == OPERATOR.EXPLICIT)
                        {
                            stringBuilder.Append(opStr);
                            stringBuilder.Append(" operator");
                        }
                        else
                        {
                            stringBuilder.Append("operator ");
                            stringBuilder.Append(opStr);
                        }

                        if ((protoFlags & PROTOFLAGS.PARAMETERS) != 0)
                        {
                            AppendParametersToPrototype(
                                PARAMTYPES.METHOD,
                                (this as OPERATORMETHODNODE).ParametersNode,
                                stringBuilder,
                                (protoFlags & PROTOFLAGS.PARAMETER_NAME) != 0);
                        }
                        break;
                    }

                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                    {
                        BASENODE node = this.ParentNode;
                        for (;
                            node != null && !node.InGroup(NODEGROUP.AGGREGATE);
                            node = node.ParentNode.ParentNode)
                        {
                            ;
                        }

                        if (this.Kind == NODEKIND.DTOR)
                        {
                            stringBuilder.Append("~");
                        }

                        if (escapedName)
                        {
                            nameManager.AppendPossibleKeyword(
                                node.AsAGGREGATE.NameNode.Name,
                                stringBuilder);
                        }
                        else
                        {
                            stringBuilder.Append(node.AsAGGREGATE.NameNode.Name);
                        }

                        if ((protoFlags & PROTOFLAGS.PARAMETERS) != 0)
                        {
                            AppendParametersToPrototype(
                                PARAMTYPES.METHOD,
                                this.AsANYMETHOD.ParametersNode,
                                stringBuilder,
                                (protoFlags & PROTOFLAGS.PARAMETER_NAME) != 0
                                );
                        }

                        break;
                    }

                case NODEKIND.INDEXER:
                case NODEKIND.PROPERTY:
                    if (this.AsANYPROPERTY.NameNode != null)
                    {
                        this.AsANYPROPERTY.NameNode.AppendNameTextToPrototype(
                            stringBuilder,
                            escNameManager);
                    }

                    if (this.Kind == NODEKIND.INDEXER)
                    {
                        if (this.AsANYPROPERTY.NameNode != null)
                        {
                            stringBuilder.Append('.');
                        }

                        stringBuilder.Append("this");

                        if ((protoFlags & PROTOFLAGS.PARAMETERS) != 0)
                        {
                            AppendParametersToPrototype(
                                PARAMTYPES.INDEXER,
                                this.AsANYPROPERTY.ParametersNode,
                                stringBuilder,
                                (protoFlags & PROTOFLAGS.PARAMETER_NAME) != 0);
                        }
                    }
                    break;

                default:
                    DebugUtil.Assert(false, "Unhandled node type in AppendPrototype!");
                    return false;
            }

            if ((protoFlags & PROTOFLAGS.FAILONMISSINGNAME) != 0)
            {
                if (stringBuilder.ToString().IndexOf('?') >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        //------------------------------------------------------------
        // BASENODE.AppendTypeParametersToKey
        //
        /// <summary>
        /// Add a type parameter string like "&lt;,,,&gt;".
        /// </summary>
        /// <param name="typeParametersNode"></param>
        /// <param name="stringBuilder"></param>
        //------------------------------------------------------------
        internal void AppendTypeParametersToKey(
            BASENODE typeParametersNode,
            StringBuilder stringBuilder)
        {
            //don't do anything if there were no type parameters
            if (typeParametersNode == null)
            {
                return;
            }

            stringBuilder.Append('<');

            //consume the first type param, it doesn't get a space
            BASENODE node = typeParametersNode.NextNode;
            while (node != null)
            {
                stringBuilder.Append(',');
                node = node.NextNode;
            }
            stringBuilder.Append('>');
        }

        //------------------------------------------------------------
        // BASENODE.AppendParametersToKey
        //
        /// <summary></summary>
        /// <param name="paramType"></param>
        /// <param name="parametersNode"></param>
        /// <param name="stringBuilder"></param>
        //------------------------------------------------------------
        internal void AppendParametersToKey(
            PARAMTYPES paramType,
            BASENODE parametersNode,
            StringBuilder stringBuilder)
        {
            switch (paramType)
            {
                default:
                    DebugUtil.Assert(false, "Invalid PARAMTYPES");
                    break;

                case PARAMTYPES.METHOD:
                    stringBuilder.Append("(");
                    break;

                case PARAMTYPES.INDEXER:
                    stringBuilder.Append("[");
                    break;

                case PARAMTYPES.GENERIC:
                    stringBuilder.Append("<");
                    break;
            }

            int count = 0;

            BASENODE node = parametersNode;
            for (; node != null; node = node.NextNode)
            {
                if (count > 0)
                {
                    stringBuilder.Append(',');
                }

                if (paramType != PARAMTYPES.GENERIC)
                {
                    if ((node.Flags & NODEFLAGS.PARMMOD_REF) != 0)
                    {
                        stringBuilder.Append("ref ");
                    }
                    else if ((node.Flags & NODEFLAGS.PARMMOD_OUT) != 0)
                    {
                        stringBuilder.Append("out ");
                    }

                    if ((parametersNode.ParentNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0 &&
                        node.NextNode == null)
                    {
                        stringBuilder.Append("params ");
                    }
                    AppendTypeToKey((node as PARAMETERNODE).TypeNode, stringBuilder);
                }
                ++count;
            }

            switch (paramType)
            {
                default:
                // Already ASSERTed above
                case PARAMTYPES.METHOD:
                    stringBuilder.Append(")");
                    break;

                case PARAMTYPES.INDEXER:
                    stringBuilder.Append("]");
                    break;

                case PARAMTYPES.GENERIC:
                    stringBuilder.Append(">");
                    break;
            }
        }

        //------------------------------------------------------------
        // BASENODE.AppendTypeToKey
        //
        /// <summary>
        /// Add a string of a type to a key.
        /// </summary>
        /// <param name="typeNode"></param>
        /// <param name="stringBuilder"></param>
        //------------------------------------------------------------
        internal void AppendTypeToKey(TYPEBASENODE typeNode, StringBuilder stringBuilder)
        {
            if (typeNode == null)
            {
                stringBuilder.Append('?');
                return;
            }

            switch (typeNode.Kind)
            {
                case NODEKIND.NAMEDTYPE:
                    {
                        NAMEDTYPENODE namedTypeNode = typeNode as NAMEDTYPENODE;
                        if (namedTypeNode.NameNode.Kind == NODEKIND.GENERICNAME)
                        {
                            stringBuilder.Append((namedTypeNode.NameNode as GENERICNAMENODE).Name);

                            //We don't use AppendParametersToKey because we need the full names 
                            //of the params.  This way we can distinguish:
                            //foo(List<int> a) and
                            //foo(List<string> a)

                            AppendParametersToPrototype(
                                PARAMTYPES.GENERIC,
                                (namedTypeNode.NameNode as GENERICNAMENODE).ParametersNode,
                                stringBuilder,
                                false);
                        }
                        else
                        {
                            namedTypeNode.NameNode.AppendNameTextToKey(stringBuilder, null);
                        }
                    }
                    break;

                case NODEKIND.PREDEFINEDTYPE:
                    if (PredefType.NameTable[(int)(typeNode as PREDEFINEDTYPENODE).Type, 1] != null)
                    {
                        // If has nicename, append it.
                        stringBuilder.Append(PredefType.NameTable[(int)(typeNode as PREDEFINEDTYPENODE).Type, 1]);
                    }
                    else
                    {
                        // If no nicename, append fullname.
                        stringBuilder.Append(PredefType.NameTable[(int)(typeNode as PREDEFINEDTYPENODE).Type, 0]);
                    }
                    break;

                case NODEKIND.ARRAYTYPE:
                    AppendTypeToKey((typeNode as ARRAYTYPENODE).ElementTypeNode, stringBuilder);
                    stringBuilder.Append('[');
                    for (int dim = 1; dim < (typeNode as ARRAYTYPENODE).Dimensions; ++dim)
                    {
                        stringBuilder.Append(',');
                    }
                    stringBuilder.Append(']');
                    break;

                case NODEKIND.POINTERTYPE:
                    stringBuilder.Append('*');
                    AppendTypeToKey((typeNode as POINTERTYPENODE).ElementTypeNode, stringBuilder);
                    break;

                case NODEKIND.NULLABLETYPE:
                    stringBuilder.Append('?');
                    AppendTypeToKey((typeNode as NULLABLETYPENODE).ElementTypeNode, stringBuilder);
                    break;

                default:
                    DebugUtil.Assert(false, "Bad type kind");
                    break;
            }
        }

        //------------------------------------------------------------
        // BASENODE.AppendParametersToPrototype
        //
        /// <summary></summary>
        /// <param name="paramType"></param>
        /// <param name="parametersNode"></param>
        /// <param name="stringBuilder"></param>
        /// <param name="paramName"></param>
        //------------------------------------------------------------
        internal void AppendParametersToPrototype(
            PARAMTYPES paramType,
            BASENODE parametersNode,
            StringBuilder stringBuilder,
            bool paramName)
        {
            switch (paramType)
            {
                case PARAMTYPES.METHOD:
                    stringBuilder.Append('(');
                    break;

                case PARAMTYPES.INDEXER:
                    stringBuilder.Append('[');
                    break;

                case PARAMTYPES.GENERIC:
                    stringBuilder.Append('<');
                    break;

                default:
                    DebugUtil.Assert(false, "Invalid PARAMTYPES");
                    break;
            }

            BASENODE node = parametersNode;
            int count = 0;
            for (; node != null; node = node.NextNode)
            {
                if (count > 0)
                {
                    stringBuilder.Append(", ");
                }

                if (paramType == PARAMTYPES.GENERIC)
                {
                    if (node.IsAnyName)
                    {
                        node.AppendNameTextToPrototype(stringBuilder, null);
                    }
                    else
                    {
                        AppendTypeToPrototype(node.AsANYTYPE, stringBuilder);
                    }
                }
                else
                {
                    if ((node.Flags & NODEFLAGS.PARMMOD_REF) != 0)
                    {
                        stringBuilder.Append("ref ");
                    }
                    else if ((node.Flags & NODEFLAGS.PARMMOD_OUT) != 0)
                    {
                        stringBuilder.Append("out ");
                    }

                    if ((parametersNode.ParentNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0 &&
                        node.NextNode == null)
                    {
                        stringBuilder.Append("params ");
                    }

                    AppendTypeToPrototype((node as PARAMETERNODE).TypeNode, stringBuilder);

                    // add parameter names
                    if (paramName)
                    {
                        stringBuilder.Append(' ');
                        stringBuilder.Append((node as PARAMETERNODE).NameNode.Name);
                    }
                }
                ++count;
            }

            switch (paramType)
            {
                case PARAMTYPES.METHOD:
                    stringBuilder.Append(')');
                    break;

                case PARAMTYPES.INDEXER:
                    stringBuilder.Append(']');
                    break;

                case PARAMTYPES.GENERIC:
                    stringBuilder.Append('>');
                    break;

                default:
                    // Already asserted above
                    break;
            }
        }

        //------------------------------------------------------------
        // BASENODE.AppendTypeToPrototype
        //
        /// <summary></summary>
        /// <param name="typeBaseNode"></param>
        /// <param name="stringBuilder"></param>
        //------------------------------------------------------------
        internal void AppendTypeToPrototype(TYPEBASENODE typeBaseNode, StringBuilder stringBuilder)
        {
            if (typeBaseNode == null)
            {
                stringBuilder.Append('?');
                return;
            }

            switch (typeBaseNode.Kind)
            {
                case NODEKIND.NAMEDTYPE:
                    {
                        NAMEDTYPENODE namedTypeNode = typeBaseNode as NAMEDTYPENODE;
                        if (namedTypeNode.NameNode.Kind == NODEKIND.GENERICNAME)
                        {
                            stringBuilder.Append((namedTypeNode.NameNode as GENERICNAMENODE).Name);
                            AppendParametersToPrototype(
                                PARAMTYPES.GENERIC,
                                (namedTypeNode.NameNode as GENERICNAMENODE).ParametersNode,
                                stringBuilder,
                                false);
                        }
                        else
                        {
                            namedTypeNode.NameNode.AppendNameTextToPrototype(stringBuilder, null);
                        }
                    }
                    break;

                case NODEKIND.PREDEFINEDTYPE:
                    int pt = (int)(typeBaseNode as PREDEFINEDTYPENODE).Type;
                    stringBuilder.Append(
                        (PredefType.NameTable[pt, 1] == null) ?
                        PredefType.NameTable[pt, 0] :
                        PredefType.NameTable[pt, 1]);
                    //stringBuilder.Append(
                    //    (Util.PredefTypeNameTable[(int)(typeBaseNode as PREDEFINEDTYPENODE).Type, 1] == null) ?
                    //    Util.PredefTypeNameTable[(int)(typeBaseNode as PREDEFINEDTYPENODE).Type, 0] :
                    //    Util.PredefTypeNameTable[(int)(typeBaseNode as PREDEFINEDTYPENODE).Type, 1]);
                    break;

                case NODEKIND.ARRAYTYPE:
                    AppendTypeToPrototype((typeBaseNode as ARRAYTYPENODE).ElementTypeNode, stringBuilder);
                    stringBuilder.Append('[');
                    for (int dim = 1; dim < (typeBaseNode as ARRAYTYPENODE).Dimensions; ++dim)
                    {
                        stringBuilder.Append(',');
                    }
                    stringBuilder.Append(']');
                    break;

                case NODEKIND.POINTERTYPE:
                    AppendTypeToPrototype((typeBaseNode as POINTERTYPENODE).ElementTypeNode, stringBuilder);
                    stringBuilder.Append('*');
                    break;

                case NODEKIND.NULLABLETYPE:
                    AppendTypeToPrototype((typeBaseNode as NULLABLETYPENODE).ElementTypeNode, stringBuilder);
                    stringBuilder.Append('?');
                    break;

                case NODEKIND.TYPEWITHATTR:
                    AppendTypeToPrototype((typeBaseNode as TYPEWITHATTRNODE).TypeBaseNode, stringBuilder);
                    break;

                default:
                    DebugUtil.Assert(false, "Bad type kind");
                    break;
            }
        }

        //------------------------------------------------------------
        // BASENODE.BuildKey
        //
        /// <summary></summary>
        /// <param name="nameManager"></param>
        /// <param name="includeParent"></param>
        /// <param name="includeGenerics"></param>
        /// <param name="keyString"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BuildKey(
            CNameManager nameManager,
            bool includeParent,
            bool includeGenerics,
            out string keyString)
        {
            keyString = null;

            // Must be in the group of key-able nodes...
            if (!this.InGroup(NODEGROUP.KEYED)) return false;

            // NODEGROUP.KEYED contains:
            //  NODEKIND.CLASS
            //  NODEKIND.CTOR
            //  NODEKIND.DELEGATE
            //  NODEKIND.DTOR
            //  NODEKIND.ENUM
            //  NODEKIND.ENUMMBR
            //  NODEKIND.INTERFACE
            //  NODEKIND.METHOD
            //  NODEKIND.NAMESPACE
            //  NODEKIND.OPERATOR
            //  NODEKIND.PROPERTY
            //  NODEKIND.INDEXER
            //  NODEKIND.STRUCT
            //  NODEKIND.VARDECL      NOTE:  You'll notice FIELD and CONST do not belong;
            //                               only VARDECL, which is used for each field/const declaration

            string cachedKey = null;
            StringBuilder stringBuilder = new StringBuilder();

            if (includeParent)
            {
                // Some nodes cache their keys.
                // Check for those nodes here, and return the cached key (if created).
                // Note that the global namespace (the root of everything) has an empty key,
                // which is assigned at parse time -- so this is gauranteed to stop there at least.
                // Also, note that we only do this if our caller asked to include the parent key,
                // since only "full" keys are cached.
                // Likewise, for elements that can contain Type params,
                // we only cache if the called asked to include the type params     

                if (this.Kind == NODEKIND.CLASS && includeGenerics)
                {
                    cachedKey = this.AsAGGREGATE.Key;
                }
                else if (this.InGroup(NODEGROUP.AGGREGATE) && includeGenerics)
                {
                    cachedKey = this.AsAGGREGATE.Key;
                }
                else if (this.Kind == NODEKIND.NAMESPACE)
                {
                    cachedKey = (this as NAMESPACENODE).Key;
                }
                else if (this.Kind == NODEKIND.DELEGATE && includeGenerics)
                {
                    cachedKey = (this as DELEGATENODE).Key;
                }

                if (cachedKey != null)
                {
                    keyString = cachedKey;
                    return true;
                }

                // Okay, we have to build it.  All keys consist of <parentkey> + "." + <thiskey>
                // (unless our parent key is empty, in which case it's just <thiskey>), so find our parent's key.

                BASENODE parentNode = this.GetParent();
                string parentKey = null;

                for (;
                    parentNode != null && !parentNode.InGroup(NODEGROUP.KEYED);
                    parentNode = parentNode.GetParent())
                {
                    ;
                }
                if (parentNode != null &&
                    !parentNode.BuildKey(nameManager, includeParent, includeGenerics, out parentKey))
                {
                    return false;
                }
                DebugUtil.Assert(parentKey != null);
                stringBuilder.Append(parentKey);
                if (parentKey.Length != 0)
                {
                    stringBuilder.Append(".");
                }
            }

            // Okay, now do the right thing based on the kind of this node...
            switch (this.Kind)
            {
                //----------------------------------------
                // NAMESPACE の場合、その名前を追加する。
                //----------------------------------------
                case NODEKIND.NAMESPACE:
                    if ((this as NAMESPACENODE).NameNode != null)
                    {
                        (this as NAMESPACENODE).NameNode.AppendNameTextToKey(stringBuilder, null);
                    }
                    break;

                //----------------------------------------
                // CLASS、STRUCT、INTERFACE、ENUM の場合、その名前を追加する。
                // includeGenerics == true なら、型パラメータがあればそれも追加する。
                //----------------------------------------
                case NODEKIND.CLASS:
                case NODEKIND.STRUCT:
                case NODEKIND.INTERFACE:
                case NODEKIND.ENUM:
                    {
                        AGGREGATENODE node = this.AsAGGREGATE;
                        node.NameNode.AppendNameTextToKey(stringBuilder, null);

                        if (includeGenerics)
                        {
                            AppendTypeParametersToKey(node.TypeParametersNode, stringBuilder);
                        }
                    }
                    break;

                //----------------------------------------
                // If DELEGATE, add the name.
                // If includeGenerics == true and has type parameters, add them.
                //----------------------------------------
                case NODEKIND.DELEGATE:
                    {
                        DELEGATENODE node = this as DELEGATENODE;
                        node.NameNode.AppendNameTextToKey(stringBuilder, null);
                        if (includeGenerics)
                        {
                            AppendTypeParametersToKey(node.TypeParametersNode, stringBuilder);
                        }

                        //TODO: append parameters?
                    }
                    break;

                //----------------------------------------
                // If ENUMMBR, add the name.
                //----------------------------------------
                case NODEKIND.ENUMMBR:
                    stringBuilder.Append((this as ENUMMBRNODE).NameNode.Name);
                    break;

                //----------------------------------------
                // If VARDECL, add the name.
                //----------------------------------------
                case NODEKIND.VARDECL:
                    stringBuilder.Append((this as VARDECLNODE).NameNode.Name);
                    break;

                //----------------------------------------
                // If METHOD, add the name.
                // If has parameters, add them.
                //----------------------------------------
                case NODEKIND.METHOD:
                    (this as METHODNODE).NameNode.AppendNameTextToKey(stringBuilder, null);
                    AppendParametersToKey(
                        PARAMTYPES.METHOD,
                        (this as METHODNODE).ParametersNode,
                        stringBuilder);
                    break;

                //----------------------------------------
                // If OPERATOR, add op" and its index.
                // If has parameters, add them.
                //----------------------------------------
                case NODEKIND.OPERATOR:
                    {
                        stringBuilder.AppendFormat("#op{0}", (uint)(this as OPERATORMETHODNODE).Operator);
                        AppendParametersToKey(
                            PARAMTYPES.METHOD,
                            (this as OPERATORMETHODNODE).ParametersNode,
                            stringBuilder);
                        break;
                    }

                //----------------------------------------
                // If CTOR, add sctor"（static) or "#ctor" (instance).
                // If has parameters, add them.
                //----------------------------------------
                case NODEKIND.CTOR:
                    if ((this.Flags & NODEFLAGS.MOD_STATIC) != 0)
                        stringBuilder.Append("#sctor");
                    else
                        stringBuilder.Append("#ctor");
                    AppendParametersToKey(
                        PARAMTYPES.METHOD,
                        (this as CTORMETHODNODE).ParametersNode,
                        stringBuilder);
                    break;

                //----------------------------------------
                // If DTOR, add dtor()".
                //----------------------------------------
                case NODEKIND.DTOR:
                    stringBuilder.Append("#dtor()");
                    break;

                //----------------------------------------
                // If INDEXER or PROPERTY, and if has a NameNode, add the name.
                // If INDEXER, add "#this" too.
                //----------------------------------------
                case NODEKIND.INDEXER:
                case NODEKIND.PROPERTY:
                    if (this.AsANYPROPERTY.NameNode != null)
                        this.AsANYPROPERTY.NameNode.AppendNameTextToKey(stringBuilder, null);
                    if (this.Kind == NODEKIND.INDEXER)
                    {
                        if (this.AsANYPROPERTY.NameNode != null)
                        {
                            stringBuilder.Append('.');
                        }
                        stringBuilder.Append("#this");
                        AppendParametersToKey(
                            PARAMTYPES.INDEXER,
                            this.AsANYPROPERTY.ParametersNode,
                            stringBuilder);
                    }
                    break;

                //----------------------------------------
                // Otherwise return false.
                //----------------------------------------
                default:
                    DebugUtil.Assert(false, "Unhandled node type in BuildKey!");
                    return false;
            }

            // Make it into a name and return it, caching it if this is a key caching node

            // In sscli,
            //     if (SUCCEEDED (hr = sb.CreateName (pNameTable, ppKey)))
            // CStringBuilder::CreateName
            //     if an error is found in a string, return a error code and null string.
            //     Register the string by NAMEMGR::Add method.
            //     If the name has already been registered, return S_OK and its NAME instance.

            keyString = stringBuilder.ToString();
            if (String.IsNullOrEmpty(keyString))
            {
                return false;
            }
            nameManager.AddString(keyString);
            if (cachedKey != null)
            {
                DebugUtil.Assert(includeParent);
                cachedKey = keyString;
            }
            return true;
        }

        // ONLY ACCESSIBLE FROM THE LANGUAGE SERVICE 
        internal uint AppendFullName(ref string sbstr) { throw new System.NotImplementedException(); }
        internal uint BuildFullName(ref string pName) { throw new System.NotImplementedException(); }

        //------------------------------------------------------------
        // BASENODE  (2013/12/17, 2014/01/02, 2014/05/13)
        //
        // In sscli, create a node list with BINOPNODE instance.
        // In this project, add two fields referring other BASENODE instance
        // to create a node list.
        //------------------------------------------------------------

        /// <summary>
        /// A linked BASENODE instance.
        /// </summary>
        internal BASENODE PrevNode = null;

        /// <summary>
        /// A linked BASENODE instance.
        /// </summary>
        internal BASENODE NextNode = null;

        //------------------------------------------------------------
        // BASENODE.LinkPrevNode
        //------------------------------------------------------------
        internal void LinkPrevNode(BASENODE node)
        {
            if (node != null)
            {
                LinkNodes(node, this);
            }
            else
            {
                PrevNode = null;
            }
        }

        //------------------------------------------------------------
        // BASENODE.LinkNextNode
        //
        /// <summary>
        /// Set a node to this.NextNode.
        /// </summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void LinkNextNode(BASENODE node)
        {
            if (node != null)
            {
                LinkNodes(this, node);
            }
            else
            {
                NextNode = null;
            }
        }

        //------------------------------------------------------------
        // BASENODE.LinkNodes
        //
        /// <summary>
        /// Link two nodes bi-directionally.
        /// </summary>
        /// <param name="prevNode"></param>
        /// <param name="nextNode"></param>
        //------------------------------------------------------------
        static internal void LinkNodes(BASENODE prevNode, BASENODE nextNode)
        {
            if (prevNode != null)
            {
                // prevNode != null && nextNode != null
                if (nextNode != null)
                {
                    prevNode.NextNode = nextNode;
                    nextNode.PrevNode = prevNode;
                    return;
                }
                // prevNode != null && nextNode == null
                else
                {
                    prevNode.NextNode = null;
                    return;
                }
            }
            // prevNode == null && nextNode != null
            else if (nextNode != null)
            {
                nextNode.PrevNode = null;
                return;
            }
            // prevNode == null && nextNode == null
            return;
        }

#if DEBUG
        //------------------------------------------------------------
        // BASENODE._DebugInfoString
        //------------------------------------------------------------
        virtual internal string _DebugInfoString
        {
            get
            {
                return String.Format("No.{0}: {1}", this.NodeID, this.Kind);
            }
        }

        //------------------------------------------------------------
        // BASENODE.DebugBase1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugBase1(StringBuilder sb)
        {
            sb.AppendFormat("{0}: {1}({2})\n", NodeID, NodeKindName, (int)Kind);
            if (!String.IsNullOrEmpty(this.DebugComment))
            {
                sb.AppendFormat("{0}: {1}\n", NodeID, DebugComment);
            }
            sb.AppendFormat("{0}: Flags          : (0x{1,0:X8}) {2}\n",
                NodeID, (int)Flags, NodeUtil.DebugNodeInfo.NodeFlagsModToString(Flags));
            sb.AppendFormat("{0}: NodeFlagsEx    : {1}\n", NodeID, NodeFlagsEx.ToString());
            sb.AppendFormat("{0}: Operator       : {1}\n", NodeID, Operator.ToString());
            sb.AppendFormat("{0}: PredefinedType : {1}\n", NodeID, PredefinedType.ToString());
            sb.AppendFormat("{0}: TokenIndex     : {1}\n", NodeID, TokenIndex);

            if (ParentNode != null)
            {
                sb.AppendFormat("{0}: ParentNode     : No.{1}\n", NodeID, ParentNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ParentNode     : null\n", NodeID);
            }

            if (NextNode != null)
            {
                sb.AppendFormat("{0}: NextNode       : No.{1}\n", NodeID, NextNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NextNode     :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // BASENODE.DebugBase2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugBase2(StringBuilder sb)
        {
            if (NextNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NextNode of No.{0}]\n", NodeID);
                NextNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // BASENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        virtual internal void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // BASENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        virtual internal void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
        }
#endif
    }
    // end of BASENODE

    //======================================================================
    // Use these macros to define nodes.  Node kind names are derived from the name
    // provided, as are the node structures themselves, i.e.,
    //
    //  DECLARE_NODE(FOO, EXPR)
    //  ...
    //  END_NODE()
    //
    // creates a FOONODE derived from EXPRNODE.
    //======================================================================

    //	#define DECLARE_NODE(nodename, nodebase) struct nodename##NODE : internal nodebase##NODE {
    //	#define END_NODE() };

    //	#define CHILD_NODE(type, name) type ## NODE * name;
    //	#define CHILD_OPTIONAL_NODE(type, name) type ## NODE * name;
    //	#define FIRST_NODE(type, name) type ## NODE * name;
    //	#define PARENT_NODE(type, name) type ## NODE * name;
    //	#define NEXT_NODE(type, name) type ## NODE * name;
    //	#define CHILD_NAME(name) NAME * name;
    //	#define INTERIOR_NODE(name) CInteriorNode * name;
    //	#define NODE_INDEX(name) long name;
    //	#define CHILD_MEMBER(type, name) type name;

    //	#include "allnodes.h"

    //======================================================================
    // class NAMESPACENODE
    //
    /// <summary>
    /// Class representing a name space node.
    /// </summary>
    //======================================================================
    internal class NAMESPACENODE : BASENODE
    {
        /// <summary>
        /// Name of namespace (possibly empty, or ""?)
        /// </summary>
        internal BASENODE NameNode = null;  // * pName

        /// <summary>
        /// List of using clauses
        /// </summary>
        internal BASENODE UsingNode = null; // * pUsing

        /// <summary>
        /// List of global attributes
        /// </summary>
        internal BASENODE GlobalAttributeNode = null;   // * pGlobalAttr

        /// <summary>
        /// Top of the list of the elements declared in this namespace
        /// </summary>
        internal BASENODE ElementsNode = null;  // * pElements

        /// <summary>
        /// Open curly position
        /// </summary>
        internal int OpenCurlyIndex = 0;    // long iOpen;

        /// <summary>
        /// Close curly position
        /// </summary>
        internal int CloseCurlyIndex = 0;   // long iClose;

        internal string Key = null; // NAME * pKey;

        //internal string SourceFileName = null;  // NAME * pNameSourceFile;
        internal FileInfo SourceFileInfo = null;

#if DEBUG
        //------------------------------------------------------------
        // NAMESPACENODE.DebugNamespace1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNamespace1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode        : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode        :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenCurlyIndex  : {1}\n", NodeID, OpenCurlyIndex);
            sb.AppendFormat("{0}: CloseCurlyIndex : {1}\n", NodeID, CloseCurlyIndex);
            sb.AppendFormat("{0}: SourceFileName  : {1}\n",
                NodeID, SourceFileInfo != null ? SourceFileInfo.Name : "null");

            if (UsingNode != null)
            {
                sb.AppendFormat("{0}: UsingNode       : No.{1}\n", NodeID, UsingNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: UsingNode       :\n", NodeID);
            }

            if (GlobalAttributeNode != null)
            {
                sb.AppendFormat("{0}: GlobalAttrNode  : No.{1}\n", NodeID, GlobalAttributeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: GlobalAttrNode  :\n", NodeID);
            }

            if (ElementsNode != null)
            {
                sb.AppendFormat("{0}: ElementNode     : No.{1}\n", NodeID, ElementsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ElementNode     :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // NAMESPACENODE.DebugNamespace2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNamespace2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (UsingNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[UsingNode of No.{0}]\n", NodeID);
                UsingNode.Debug(sb);
            }

            if (GlobalAttributeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("GlobalAttributeNode of No.{0}]\n", NodeID);
                GlobalAttributeNode.Debug(sb);
            }

            if (ElementsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ElementNode of No.{0}]\n", NodeID);
                ElementsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // NAMESPACENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNamespace1(sb);
            DebugNamespace2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // NAMESPACENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNamespace1(sb);
        }
#endif
    }

    //======================================================================
    // class USINGNODE
    //
    /// <summary>
    /// Represents extern-alias-directives and using-directives.
    /// </summary>
    //
    // extern-alias-directives:
    //     extern-alias-directive
    //     extern-alias-directives  extern-alias-directive
    // extern-alias-directive:
    //     "extern"  "alias"  identifier  ";"
    //
    // using-directive:
    //     using-alias-directive
    //     using-namespace-directive
    // using-alias-directive:
    //     "using" identifier "=" namespace-or-type-name ";"
    // using-namespace-directive:
    //     "using" namespace-name ";"
    //======================================================================
    internal class USINGNODE : BASENODE
    {
        /// <summary>
        /// Name used (null if an extern alias)
        /// </summary>
        internal BASENODE NameNode = null;

        /// <summary>
        /// Alias (null indicates using-namespace)
        /// </summary>
        internal NAMENODE AliasNode = null;

#if DEBUG
        //------------------------------------------------------------
        // USINGNODE.DebugUsing1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugUsing1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode   : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode   :\n", NodeID);
            }

            if (AliasNode != null)
            {
                sb.AppendFormat("{0}: AliasNode  : No.{1}\n", NodeID, AliasNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: AliasNode  :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // USINGNODE.DebugUsing2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugUsing2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (AliasNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[AliasNode of No.{0}]\n", NodeID);
                AliasNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // USINGNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugUsing1(sb);
            DebugUsing2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // USINGNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugUsing1(sb);
        }
#endif
    }

    //======================================================================
    // class NAMENODE
    //
    /// <summary>
    /// <para>Has Name field of type string and PossibleGenericName field of type GENERICNAMENODE.</para>
    /// <para>No other node class has a Name field of type string.</para>
    /// </summary>
    //======================================================================
    internal class NAMENODE : BASENODE
    {
        /// <summary>
        /// Name text
        /// </summary>
        internal string Name = null;    // pName

        internal GENERICNAMENODE PossibleGenericName = null;    // pPossibleGenericName

        internal bool IsMissing
        {
            get { return (this.Flags & NODEFLAGS.NAME_MISSING) != 0; }
        }

#if DEBUG
        //------------------------------------------------------------
        // NAMENODE.DebugName (1)
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        /// <param name="node"></param>
        /// <param name="format"></param>
        //------------------------------------------------------------
        static internal void DebugName(StringBuilder sb, BASENODE node, string format)
        {
            BINOPNODE listNode = null;
            if (node != null)
            {
                listNode = node.AsDOT;
            }

            if (listNode == null)
            {
                NAMENODE name = null;
                string str;
                if (node != null)
                {
                    name = node as NAMENODE;  // as NAMEDTYPENODE;
                }

                if (name != null && name.Name != null)
                {
                    str = name.Name;
                }
                else if (name == null)
                {
                    str = "null node or not NAMENODE";
                }
                else
                {
                    str = "null name";
                }

                if (format == null)
                {
                    format = "NameNode : {0}\n";
                }
                sb.AppendFormat(format, str);
            }
            else
            {
                sb.AppendFormat("NameNode : No.{0}\n", listNode.NodeID);
                listNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // NAMENODE.DebugName (2)
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        /// <param name="node"></param>
        //------------------------------------------------------------
        static internal void DebugName(StringBuilder sb, BASENODE node)
        {
            DebugName(sb, node, null);
        }

        //------------------------------------------------------------
        // NAMENODE.DebugName1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void DebugName1(StringBuilder sb)
        {
            sb.AppendFormat("{0}: Name                : {1}\n", NodeID, this.Name);

            if (this.PossibleGenericName != null)
            {
                sb.AppendFormat("{0}: PossibleGenericName : No.{1}]\n",
                    NodeID,
                    PossibleGenericName.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: PossibleGenericName :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // NAMENODE.DebugName2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void DebugName2(StringBuilder sb)
        {
            if (this.PossibleGenericName != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[PossibleGenericName of No.{0}]\n", NodeID);
                PossibleGenericName.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // NAMENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugName1(sb);
            DebugName2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // NAMENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugName1(sb);
        }
#endif
    }

    //======================================================================
    // class GENERICNAMENODE
    //
    /// <summary>
    /// <para>Derives form NAMENODE, representing a name with type parameters.</para>
    /// <para>Has a sequence of nodes which represent type parameters
    /// and indices of "&lt;" and "&gt;".</para>
    /// </summary>
    //======================================================================
    internal class GENERICNAMENODE : NAMENODE
    {
        /// <summary>
        /// Parameters to qualified type name
        /// </summary>
        internal BASENODE ParametersNode = null;    // pParams

        /// <summary>
        /// Open "&lt;" token index
        /// </summary>
        internal int OpenAngleIndex = -1;   // iOpen

        /// <summary>
        /// Close "&gt;" token index
        /// </summary>
        internal int CloseAngleIndex = -1;  // iClose

#if DEBUG
        //------------------------------------------------------------
        // GENERICNAMENODE.DebugGenericName1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugGenericName1(StringBuilder sb)
        {
            if (ParametersNode != null)
            {
                sb.AppendFormat("{0}: ParametersNode : No.{1}\n", NodeID,ParametersNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ParametersNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenAngleIndex  : {1}\n", NodeID, OpenAngleIndex);
            sb.AppendFormat("{0}: CloseAngleIndex : {1}\n", NodeID, CloseAngleIndex);
        }

        //------------------------------------------------------------
        // GENERICNAMENODE.DebugGenericName2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugGenericName2(StringBuilder sb)
        {
            if (ParametersNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ParametersNode of No.{0}]\n", NodeID);
                ParametersNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // GENERICNAMENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugName1(sb);
            DebugGenericName1(sb);
            DebugGenericName2(sb);
            DebugName2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // GENERICNAMENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugName1(sb);
            DebugGenericName1(sb);
        }
#endif
    }

    //======================================================================
    // class OPENNAMENODE
    //
    /// <summary>
    /// <para>Derives from NAMENODE. Represents not binded generic name.para>
    /// <para>Has the positions of '&lt;' and '&gt;' and the number of parameters.</para>
    /// </summary>
    //======================================================================
    internal class OPENNAMENODE : NAMENODE
    {
        /// <summary>
        /// Open '&lt;' token index
        /// </summary>
        internal int OpenAngleIndex = -1;

        /// <summary>
        /// Close '&gt;' token index
        /// </summary>
        internal int CloseAngleIndex = -1;

        /// <summary>
        /// Number of blank parameters
        /// </summary>
        internal int CountOfBlankParameters = 0;

#if DEBUG
        //------------------------------------------------------------
        // OPENNAMENODE.DebugOpenName
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugOpenName(StringBuilder sb)
        {
            sb.AppendFormat("{0}: OpenAngleIndex         : {1}\n", OpenAngleIndex);
            sb.AppendFormat("{0}: CloseAngleIndex        : {1}\n", CloseAngleIndex);
            sb.AppendFormat("{0}: CountOfBlankParameters : {1}\n\n", CountOfBlankParameters);
            sb.Append("\n");
        }

        //------------------------------------------------------------
        // OPENNAMENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugName1(sb);
            DebugOpenName(sb);
            DebugName2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // OPENNAMENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugName1(sb);
            DebugOpenName(sb);
        }
#endif
    }

    //======================================================================
    // class AGGREGATENODE
    //
    /// <summary>
    /// Base class for classes representing aggregate nodes.
    /// </summary>
    //======================================================================
    internal class AGGREGATENODE : BASENODE
    {
        /// <summary>
        /// Attributes
        /// </summary>
        internal BASENODE AttributesNode = null;    // pAttr

        /// <summary>
        /// Name of class
        /// </summary>
        internal NAMENODE NameNode = null;  // pName

        /// <summary>
        /// <para>Top of the list of base class/interfaces</para>
        /// <para>If Enum, represents the base type.</para>
        /// </summary>
        internal BASENODE BasesNode = null; // pBases

        /// <summary>
        /// <para>Top of the list of type parameters</para>
        /// </summary>
        internal BASENODE TypeParametersNode = null;    // pTypeParams

        /// <summary>
        /// List of Constraints on type parameters
        /// </summary>
        internal BASENODE ConstraintsNode = null;   // pConstraints

        /// <summary>
        /// List of members
        /// </summary>
        internal MEMBERNODE MembersNode = null; // pMembers

        /// <summary>
        /// First non-attribute token
        /// </summary>
        internal int StartTokenIndex = -1;  // iStart

        /// <summary>
        /// Open curly
        /// </summary>
        internal int OpenCurlyIndex = -1;   // iOpen

        /// <summary>
        /// Close curly
        /// </summary>
        internal int CloseCurlyIndex = -1;  // iClose

        /// <summary>
        /// <para>Key</para>
        /// </summary>
        internal string Key = null; // pKey

#if DEBUG
        //------------------------------------------------------------
        // AGGREGATENODE.DebugAggregate1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAggregate1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (BasesNode != null)
            {
                sb.AppendFormat("{0}: BasesNode : No.{1}\n", NodeID, BasesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: BasesNode :\n", NodeID);
            }

            if (TypeParametersNode != null)
            {
                sb.AppendFormat("{0}: TypeParametersNode : No.{1}\n", NodeID, TypeParametersNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeParametersNode :\n", NodeID);
            }

            if (ConstraintsNode != null)
            {
                sb.AppendFormat("{0}: ConstraintsNode : No.{1}\n", NodeID, ConstraintsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ConstraintsNode :\n", NodeID);
            }

            if (AttributesNode != null)
            {
                sb.AppendFormat("{0}: AttributesNode  : No.{1}\n", NodeID, AttributesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: AttributesNode  :\n", NodeID);
            }

            if (MembersNode != null)
            {
                sb.AppendFormat("{0}: MembersNode : No.{1}\n", NodeID, MembersNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: MembersNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: StartTokenIndex : {1}\n", NodeID, StartTokenIndex);
            sb.AppendFormat("{0}: OpenCurlyIndex  : {1}\n", NodeID, OpenCurlyIndex);
            sb.AppendFormat("{0}: CloseCurlyIndex : {1}\n", NodeID, CloseCurlyIndex);
        }

        //------------------------------------------------------------
        // AGGREGATENODE.DebugAggregate2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAggregate2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (BasesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[BasesNode of No.{0}]\n", NodeID);
            }

            if (TypeParametersNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeParametersNode of No.{0}]\n", NodeID);
                TypeParametersNode.Debug(sb);
            }

            if (ConstraintsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ConstraintsNode of No.{0}]\n", NodeID);
                ConstraintsNode.Debug(sb);
            }

            if (MembersNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[MembersNode of No.{0}]\n", NodeID);
                MembersNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // AGGREGATENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAggregate1(sb);
            DebugAggregate2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // AGGREGATENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAggregate1(sb);
        }
#endif
    }

    //======================================================================
    // class CLASSNODE
    //
    /// <summary>
    /// Derives from AGGREGATENODE.
    /// Represents class.
    /// </summary>
    //======================================================================
    internal class CLASSNODE : AGGREGATENODE
    {
    }

    //======================================================================
    // class ENUMNODE
    //
    /// <summary>
    /// <para>Represents enum. Derives from AGGREGATENODE.</para>
    /// <para>The base type is set to BasesNode field.</para>
    /// </summary>
    //======================================================================
    internal class ENUMNODE : AGGREGATENODE
    {
    }

    //======================================================================
    // class STRUCTNODE
    //
    /// <summary>
    /// Derives from AGGREGATENODE.
    /// Represents struct.
    /// </summary>
    //======================================================================
    internal class STRUCTNODE : AGGREGATENODE
    {
    }

    //======================================================================
    // class INTERFACENODE
    //
    /// <summary>
    /// Derives from AGGREGATENODE.
    /// Represents interface.
    /// </summary>
    //======================================================================
    internal class INTERFACENODE : AGGREGATENODE
    {
    }

    //======================================================================
    // class CONSTRAINTNODE
    //
    /// <summary>
    /// Represents constraints on generic parameters.
    /// <list type="bullet">
    /// <item><description>
    /// "new()", "class", struct" are set on Flags (defined in BASENODE). (use NODEFLAGS.CONSTRAINT_.)
    /// </description></item>
    /// <item><description>
    /// Other constraints are stored in the node list starting at BoundsNode
    /// </description></item>
    /// </list>
    /// </summary>
    //======================================================================
    internal class CONSTRAINTNODE : BASENODE
    {
        /// <summary>
        /// Name of type variable
        /// </summary>
        internal NAMENODE NameNode = null;

        /// <summary>
        /// <para>List of type bounds</para>
        /// <para>"new()"、 "class"、"struct" 以外の制約を表すノード列の先頭。</para>
        /// </summary>
        internal BASENODE BoundsNode = null;

        /// <summary>
        /// Last token index
        /// </summary>
        internal int EndTokenIndex = 0;

#if DEBUG
        //------------------------------------------------------------
        // CONSTRAINTNODE.DebugConstraints1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugConstraints1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (BoundsNode != null)
            {
                sb.AppendFormat("{0}: BoundsNode : No.{1}\n", NodeID, BoundsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: BoundsNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: EndTokenIndex : {1}\n", NodeID, EndTokenIndex);
        }

        //------------------------------------------------------------
        // CONSTRAINTNODE.DebugConstraints2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugConstraints2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (BoundsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[BoundsNode of No.{0}]\n", NodeID);
                BoundsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // CONSTRAINTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugConstraints1(sb);
            DebugConstraints2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // CONSTRAINTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugConstraints1(sb);
        }
#endif
    }

    //======================================================================
    // class DELEGATENODE
    //
    /// <summary>
    /// Represents delegate.
    /// </summary>
    //======================================================================
    internal class DELEGATENODE : BASENODE
    {
        internal BASENODE AttributesNode = null;    // pAttr

        /// <summary>
        /// TYPEBASENODE representing a return type.
        /// </summary>
        internal TYPEBASENODE ReturnTypeNode = null;    // pType

        internal NAMENODE NameNode = null;          // pName

        internal int OpenParenIndex = 0;            // iOpenParen

        internal BASENODE ParametersNode = null;    // pParms

        internal int CloseParenIndex = 0;           // iCloseParen

        /// <summary>
        /// <para>Type parameters and their bounds</para>
        /// <para>Top of the list of type parameters</para>
        /// </summary>
        internal BASENODE TypeParametersNode = null;    // pTypeParams

        /// <summary>
        /// List of Constraints on type parameters
        /// </summary>
        internal BASENODE ConstraintsNode = null;   // pConstraints

        /// <summary>
        /// First non-attribute token
        /// </summary>
        internal int StartTokenIndex = 0;   // iStart

        /// <summary>
        /// Semicolon position
        /// </summary>
        internal int SemiColonIndex = 0;    // iSemi

        /// <summary>
        /// Key
        /// </summary>
        internal string Key = "";           // pKey

#if DEBUG
        //------------------------------------------------------------
        // DELEGATENODE.DebugDelegate1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugDelegate1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenParenIndex  : {1}\n", NodeID, OpenParenIndex);
            sb.AppendFormat("{0}: CloseParenIndex : {1}\n", NodeID, CloseParenIndex);
            sb.AppendFormat("{0}: StartTokenIndex : {1}\n", NodeID, StartTokenIndex);
            sb.AppendFormat("{0}: SemicolonIndex  : {1}\n", NodeID, SemiColonIndex);
            sb.AppendFormat("{0}: Key             : {1}\n", NodeID, Key);

            if (ReturnTypeNode != null)
            {
                sb.AppendFormat("{0}: ReturnTypeNode : No.{1}\n", NodeID, ReturnTypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ReturnTypeNode :\n", NodeID);
            }

            if (ParametersNode != null)
            {
                sb.AppendFormat("{0}: ParametersNode : No.{1}\n", NodeID, ParametersNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ParametersNode :\n", NodeID);
            }

            if (TypeParametersNode != null)
            {
                sb.AppendFormat("{0}: TypeParametersNode : No.{1}\n", NodeID, TypeParametersNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeParametersNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // DELEGATENODE.DebugDelegate2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugDelegate2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (ReturnTypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ReturnTypeNode of No.{0}]\n", NodeID);
                ReturnTypeNode.Debug(sb);
            }

            if (ParametersNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ParametersNode of No.{0}]\n", NodeID);
                ParametersNode.Debug(sb);
            }

            if (TypeParametersNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeParametersNode of No.{0}]\n", NodeID);
                TypeParametersNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // DELEGATENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugDelegate1(sb);
            DebugDelegate2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // DELEGATENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugDelegate1(sb);
        }
#endif
    }

    //======================================================================
    // class MEMBERNODE
    //
    /// <summary>
    /// Base class representing nodes of members of aggregate type.
    /// </summary>
    //======================================================================
    internal class MEMBERNODE : BASENODE
    {
        internal BASENODE AttributesNode = null;    // pAttr
        
        //internal MEMBERNODE NextNode = null;
        /// <summary>
        /// (RW) Set next member to NextNode
        /// </summary>
        internal MEMBERNODE NextMemberNode  // pNext
        {
            get { return (NextNode != null) ? NextNode as MEMBERNODE : null; }
            set { NextNode = value; }
        }

        /// <summary>
        /// First non-attribute token
        /// </summary>
        internal int StartTokenIndex = 0;   // iStart

        /// <summary>
        /// Semicolon or close-curly terminator
        /// </summary>
        internal int CloseIndex = 0;    // iClose

#if DEBUG
        //------------------------------------------------------------
        // MEMBERNODE.DebugMember1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugMember1(StringBuilder sb)
        {

            if (AttributesNode != null)
            {
                sb.AppendFormat("{0}: AttributeNode : No.{1}\n", NodeID, AttributesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: AttributeNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: StartTokenIndex : {1}\n", NodeID, StartTokenIndex);
            sb.AppendFormat("{0}: CloseIndex      : {1}\n", NodeID, CloseIndex);
        }

        //------------------------------------------------------------
        // MEMBERNODE.DebugMember2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugMember2(StringBuilder sb)
        {
            if (AttributesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[AttributeNode of No.{0}]\n", NodeID);
                AttributesNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // MEMBERNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugMember2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // MEMBERNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
        }
#endif
    }

    //======================================================================
    // class ENUMMBRNODE
    //
    /// <summary>
    /// <para>member for enums</para>
    /// <para>Derives from MEMBERNODE. Represents members of type of enum.
    /// Has a name node and a value node.</para>
    /// </summary>
    //======================================================================
    internal class ENUMMBRNODE : MEMBERNODE
    {
        internal NAMENODE NameNode = null;  // pName
        internal BASENODE ValueNode = null; // pValue

#if DEBUG
        //------------------------------------------------------------
        // ENUMMBRNODE.DebugEnumMbr1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugEnumMbr1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (ValueNode != null)
            {
                sb.AppendFormat("{0}: ValueNode : No.{1}\n", NodeID, ValueNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ValueNode : null.\n\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // ENUMMBRNODE.DebugEnumMbr2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugEnumMbr2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (ValueNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ValueNode of No.{0}]\n", NodeID);
                ValueNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ENUMMBRNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugEnumMbr1(sb);
            DebugEnumMbr2(sb);
            DebugMember2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // ENUMMBRNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugEnumMbr1(sb);
        }
#endif
    }

    //======================================================================
    // class FIELDNODE
    //
    /// <summary>
    /// <para>Drives from MEMBERNODE. Represents fields.</para>
    /// <para>Used in case of NODEKIND.CONST.</para>
    /// </summary>
    //======================================================================
    internal class FIELDNODE : MEMBERNODE
    {
        internal TYPEBASENODE TypeNode = null;      // pType
        internal BASENODE DeclarationsNode = null;  // pDecls

#if DEBUG
        //------------------------------------------------------------
        // FIELDNODE.DebugField1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugField1(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, TypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode :\n", NodeID);
            }

            if (DeclarationsNode != null)
            {
                sb.AppendFormat("{0}: DeclarationsNode : No.{1}\n", NodeID, DeclarationsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: DeclarationsNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // FIELDNODE.DebugField2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugField2(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeNode of No.{0}]\n", NodeID);
                TypeNode.Debug(sb);
            }

            if (DeclarationsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[DeclarationsNode of No.{0}]\n", NodeID);
                DeclarationsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // FIELDNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugField1(sb);
            DebugField2(sb);
            DebugMember2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // FIELDNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugField1(sb);
        }
#endif
    }

    //======================================================================
    // class METHODBASENODE
    //
    /// <summary>
    /// Derives from MEMBERNODE. 
    /// Base class of classes representing method and others.
    /// </summary>
    //======================================================================
    internal class METHODBASENODE : MEMBERNODE
    {
        /// <summary>
        /// Return type of method
        /// </summary>
        internal TYPEBASENODE ReturnTypeNode = null;  // pType

        internal int OpenParenIndex = -1;   // iOpenParen

        internal BASENODE ParametersNode = null;    // pParms

        internal int CloseParenIndex = -1;  // iCloseParen

        /// <summary>
        /// Token index of open curly/semicolon
        /// </summary>
        internal int OpenIndex = -1;    // iOpen

        /// <summary>
        /// Method body (if parsed)
        /// </summary>
        internal BLOCKNODE BodyNode = null; // pBody

        /// <summary>
        /// Interior node container object (if parsed)
        /// </summary>
        internal CInteriorNode InteriorNode = null; // pInteriorNode

#if DEBUG
        //------------------------------------------------------------
        // METHODBASENODE.DebugMethodBase1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugMethodBase1(StringBuilder sb)
        {
            if (ReturnTypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, ReturnTypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode :\n", NodeID);
            }

            if (ParametersNode != null)
            {
                sb.AppendFormat("{0}: ParametersNode : No.{1}\n", NodeID, ParametersNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ParametersNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenParenIndex  : {1}\n", NodeID, OpenParenIndex);
            sb.AppendFormat("{0}: CloseParenIndex : {1}\n", NodeID, CloseParenIndex);
            sb.AppendFormat("{0}: OpenIndex       : {1}\n", NodeID, OpenIndex);

            if (BodyNode != null)
            {
                sb.AppendFormat("{0}: BodyNode : No.{1}\n", NodeID, BodyNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: BodyNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: CInteriorNode: not implemented.\n\n", NodeID);
        }

        //------------------------------------------------------------
        // METHODBASENODE.DebugMethodBase2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugMethodBase2(StringBuilder sb)
        {
            if (ReturnTypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ReturnTypeNode of No.{0}]\n", NodeID);
                ReturnTypeNode.Debug(sb);
            }

            if (ParametersNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ParametersNode of No.{0}]\n", NodeID);
                ParametersNode.Debug(sb);
            }

            if (BodyNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[BodyNode of No.{0}]\n", NodeID);
                BodyNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // METHODBASENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
            DebugMethodBase2(sb);
            DebugMember2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // METHODBASENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
        }
#endif
    }

    //======================================================================
    // class METHODNODE
    //
    /// <summary>
    /// Derives from METHODBASENODE. Represents methods.
    /// </summary>
    //======================================================================
    internal class METHODNODE : METHODBASENODE
    {
        /// <summary>
        /// (NK_METHOD only -- Name of method)
        /// </summary>
        internal BASENODE NameNode = null;  // pName

        /// <summary>
        /// List of Constraints on type parameters
        /// </summary>
        internal BASENODE ConstraintsNode = null;   // pConstraints

        /// <summary>
        /// <para>(R) (CS3) Extension method or not.</para>
        /// </summary>
        internal bool IsExtensionMethod = false;

#if DEBUG
        //------------------------------------------------------------
        // METHODNODE.DebugMethod1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugMethod1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (ConstraintsNode != null)
            {
                sb.AppendFormat("{0}: ConstraintsNode  : No.{1}\n", NodeID, ConstraintsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ConstraintsNode  :\n", NodeID);
            }

            if (IsExtensionMethod == true)
            {
                sb.AppendFormat("{0}: IsExtensionMethod: {1}\n", NodeID, IsExtensionMethod);
            }
        }

        //------------------------------------------------------------
        // METHODNODE.DebugMethod2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugMethod2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (ConstraintsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("ConstraintsNode of No.{0}]\n", NodeID);
                ConstraintsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // METHODNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMethod1(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
            DebugMethod2(sb);
            DebugMember2(sb);
            DebugMethodBase2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // METHODNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMethod1(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
        }
#endif
    }

    //======================================================================
    // class OPERATORMETHODNODE
    //
    /// <summary>
    /// Derives from METHODBASENODE. Represents overloaded operators.
    /// </summary>
    //======================================================================
    internal class OPERATORMETHODNODE : METHODBASENODE
    {
        /// <summary>
        /// (NK_OPERATOR only -- overloaded operator)
        /// </summary>
        new internal OPERATOR Operator = OPERATOR.NONE; // iOp

        /// <summary>
        /// Used when iOp is OP_NONE (error cases)
        /// </summary>
        internal TOKENID TokenId = TOKENID.UNDEFINED;   // tok

#if DEBUG
        //------------------------------------------------------------
        // OPERATORMETHODNODE.DebugMethod
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugOperatorMethod(StringBuilder sb)
        {
            sb.AppendFormat("{0}: Oprator : {1}\n", NodeID, Operator);
            sb.AppendFormat("{0}: TokenId : {1}\n\n", NodeID, TokenId);
        }

        //------------------------------------------------------------
        // OPERATORMETHODNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugOperatorMethod(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
            DebugMember2(sb);
            DebugMethodBase2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // OPERATORMETHODNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugOperatorMethod(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
        }
#endif
    }

    //======================================================================
    // class CTORMETHODNODE
    //
    /// <summary>
    /// Derives from METHODBASENODE. Represents constructors.
    /// </summary>
    //======================================================================
    internal class CTORMETHODNODE : METHODBASENODE
    {
        /// <summary>
        /// The call to this( or to base(
        /// </summary>
        internal CALLNODE ThisOrBaseCallNode = null;  // pThisBaseCall

#if DEBUG
        //------------------------------------------------------------
        // CTORMETHODNODE.DebugCtorMethod1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugCtorMethod1(StringBuilder sb)
        {
            if (ThisOrBaseCallNode != null)
            {
                sb.AppendFormat("{0}: ThisOrBaseCallNode : No.{1}\n", NodeID,ThisOrBaseCallNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ThisOrBaseCallNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // CTORMETHODNODE.DebugCtorMethod2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugCtorMethod2(StringBuilder sb)
        {
            if (ThisOrBaseCallNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ThisOrBaseCallNode of No.{0}]\n", NodeID);
                ThisOrBaseCallNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // CTORMETHODNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
            DebugCtorMethod1(sb);
            DebugMember2(sb);
            DebugMethodBase2(sb);
            DebugCtorMethod2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // CTORMETHODNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugMember1(sb);
            DebugMethodBase1(sb);
            DebugCtorMethod1(sb);
        }
#endif
    }

    //======================================================================
    // class PROPERTYNODE
    //
    /// <summary>
    /// Derives from MEMBERNODE. Represents properties.
    /// </summary>
    //======================================================================
    internal class PROPERTYNODE : MEMBERNODE
    {
        /// <summary>
        /// Name of property, or name of interface for indexer
        /// </summary>
        internal BASENODE NameNode = null;  // * pName

        /// <summary>
        /// Type of property
        /// </summary>
        internal TYPEBASENODE TypeNode = null;  // * pType

        internal int OpenSquare = -1;    // iOpenSquare

        /// <summary>
        /// Index parameters
        /// </summary>
        internal BASENODE ParametersNode = null;    // * pParms

        internal int CloseSquare = -1;   // iCloseSquare

        /// <summary>
        /// Token index of opening '{'
        /// </summary>
        internal int OpenCurlyIndex = 0;    // iOpen

        /// <summary>
        /// Get accessor
        /// </summary>
        internal ACCESSORNODE GetNode = null;   // * pGet

        /// <summary>
        /// Set accessor
        /// </summary>
        internal ACCESSORNODE SetNode = null;   // * pSet

#if DEBUG
        //------------------------------------------------------------
        // PROPERTYNODE.DebugProperty1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugProperty1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenSquare     : {1}\n", NodeID, OpenSquare);
            sb.AppendFormat("{0}: CloseSquare    : {1}\n", NodeID, CloseSquare);
            sb.AppendFormat("{0}: OpenCurlyIndex : {1}\n", NodeID, OpenCurlyIndex);

            if (TypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, TypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode :\n", NodeID);
            }

            if (GetNode != null)
            {
                sb.AppendFormat("{0}: GetNode : No.{1}\n", NodeID, GetNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: GetNode :\n", NodeID);
            }

            if (SetNode != null)
            {
                sb.AppendFormat("{0}: SetNode : No.{1}\n", NodeID, SetNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: SetNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // PROPERTYNODE.DebugProperty2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugProperty2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (TypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeNode of No.{0}]\n", NodeID);
                TypeNode.Debug(sb);
            }

            if (GetNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[GetNode of No.{0}]\n", NodeID);
                GetNode.Debug(sb);
            }

            if (SetNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[SetNode of No.{0}]\n", NodeID);
                SetNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // PROPERTYNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugProperty1(sb);
            DebugMember1(sb);
            DebugProperty2(sb);
            DebugMember2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // PROPERTYNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugProperty1(sb);
            DebugMember1(sb);
        }
#endif
    }

    //======================================================================
    // class ACCESSORNODE
    //
    /// <summary>
    /// Represents a accessor.
    /// </summary>
    //======================================================================
    internal class ACCESSORNODE : BASENODE
    {
        internal BASENODE AttributesNode = null;    // pAttr

        /// <summary>
        /// Token index of open curly
        /// </summary>
        internal int OpenCurlyIndex = 0;    // iOpen

        /// <summary>
        /// Token index of close curly
        /// </summary>
        internal int CloseCurlyIndex = 0;   // iClose

        /// <summary>
        /// Body of node
        /// </summary>
        internal BLOCKNODE BodyNode = null; // pBody

        /// <summary>
        /// Interior node container object (if parsed)
        /// </summary>
        internal CInteriorNode InteriorNode = null; // pInteriorNode

        /// <summary>
        /// (CS3)
        /// </summary>
        internal bool IsAutoImplemented = false;

#if DEBUG
        //------------------------------------------------------------
        // ACCESSORNODE.DebugAccessor1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAccessor1(StringBuilder sb)
        {
            sb.AppendFormat("{0}: OpenCurlyNode  : {1}\n", NodeID, OpenCurlyIndex);
            sb.AppendFormat("{0}: CloseCurlyNode : {1}\n\n", NodeID, CloseCurlyIndex);

            if (AttributesNode != null)
            {
                sb.AppendFormat("{0}: AttributesNode : No.{1}\n", NodeID, AttributesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: AttributesNode :\n", NodeID);
            }

            if (BodyNode != null)
            {
                sb.AppendFormat("{0}: BodyNode : No.{1}\n", NodeID, BodyNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: BodyNode : null.\n\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // ACCESSORNODE.DebugAccessor2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAccessor2(StringBuilder sb)
        {
            if (AttributesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[AttributesNode of No.{0}]\n", NodeID);
                AttributesNode.Debug(sb);
            }

            if (BodyNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[BodyNode of No.{0}]\n", NodeID);
                BodyNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ACCESSORNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAccessor1(sb);
            DebugAccessor2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // ACCESSORNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAccessor1(sb);
        }
#endif
    }

    //======================================================================
    // class PARAMETERNODE
    //
    /// <summary>
    /// Represents a parameter. Has attributes, type, name.
    /// </summary>
    //======================================================================
    internal class PARAMETERNODE : BASENODE
    {
        /// <summary>
        /// Attributes of a parameter.
        /// </summary>
        internal BASENODE AttributesNode = null;    // pAttr

        /// <summary>
        /// Type of a parameter.
        /// </summary>
        internal TYPEBASENODE TypeNode = null;  // pType

        /// <summary>
        /// Name of a parameter.
        /// </summary>
        internal NAMENODE NameNode = null;  // pName

#if DEBUG
        //------------------------------------------------------------
        // PARAMETERNODE.DebugParameter1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugParameter1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (TypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, TypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode :\n", NodeID);
            }

            if (AttributesNode != null)
            {
                sb.AppendFormat("{0}: AttributesNode : No.{1}\n", NodeID, AttributesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: AttributesNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // PARAMETERNODE.DebugParameter2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugParameter2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (TypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeNode of No.{0}]\n", NodeID);
                TypeNode.Debug(sb);
            }

            if (AttributesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[AttributesNode of No.{0}]\n", NodeID);
                AttributesNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // PARAMETERNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugParameter1(sb);
            DebugParameter2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // PARAMETERNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugParameter1(sb);
        }
#endif
    }

    //======================================================================
    // class NESTEDTYPENODE
    //
    /// <summary>
    /// <para>Derives from MEMBERNODE.</para>
    /// <para>Has field TypeNode of type BASENODE.</para>
    /// </summary>
    //======================================================================
    internal class NESTEDTYPENODE : MEMBERNODE
    {
        /// <summary>
        /// Nested type
        /// </summary>
        internal BASENODE TypeNode = null;  // pType

#if DEBUG
        //------------------------------------------------------------
        // NESTEDTYPENODE.DebugNestedType1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNestedType1(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, TypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode : null\n\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // NESTEDTYPENODE.DebugNestedType2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNestedType2(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeNode of No.{0}]\n", NodeID);
                TypeNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // NESTEDTYPENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNestedType1(sb);
            DebugMember1(sb);
            DebugNestedType2(sb);
            DebugMember2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // NESTEDTYPENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNestedType1(sb);
            DebugMember1(sb);
        }
#endif
    }

    //======================================================================
    // class PARTIALMEMBERNODE
    //
    /// <summary>
    /// Derives from MEMBERNODE.
    /// </summary>
    //======================================================================
    internal class PARTIALMEMBERNODE : MEMBERNODE
    {
        /// <summary>
        /// <para>Whatever</para>
        /// </summary>
        internal BASENODE Node = null;  // pNode

#if DEBUG
        //------------------------------------------------------------
        // PARTIALMEMBERNODE.DebugPartialMember1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugPartialMember1(StringBuilder sb)
        {
            if (Node != null)
            {
                sb.AppendFormat("{0}: Node : No.{1}\n", NodeID, Node.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: Node : null\n\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // PARTIALMEMBERNODE.DebugPartialMember2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugPartialMember2(StringBuilder sb)
        {
            if (Node != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[Node of No.{0}]\n", NodeID);
                Node.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // PARTIALMEMBERNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugPartialMember1(sb);
            DebugMember1(sb);
            DebugPartialMember2(sb);
            DebugMember2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // PARTIALMEMBERNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugPartialMember1(sb);
            DebugMember1(sb);
        }
#endif
    }

    //======================================================================
    // class TYPEBASENODE
    //
    /// <summary>
    /// Drives from BASENODE. No paticular member.
    /// </summary>
    //======================================================================
    internal class TYPEBASENODE : BASENODE
    {
    }

    //======================================================================
    // class PREDEFINEDTYPENODE
    //
    /// <summary>
    /// Derives from TYPEASENODE. Represents predefined types.
    /// Has a filed of type PREDEFTYPE.
    /// </summary>
    //======================================================================
    internal class PREDEFINEDTYPENODE : TYPEBASENODE
    {
        internal PREDEFTYPE Type = PREDEFTYPE.UNDEFINED;    // iType

#if DEBUG
        //------------------------------------------------------------
        // PREDEFINEDTYPENODE.DebugPredefinedType
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugPredefinedType(StringBuilder sb)
        {
            sb.AppendFormat("{0}: PredefType : {1}\n", NodeID, Type);
        }

        //------------------------------------------------------------
        // PREDEFINEDTYPENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugPredefinedType(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // PREDEFINEDTYPENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugPredefinedType(sb);
        }
#endif
    }

    //======================================================================
    // class NAMEDTYPENODE
    //
    /// <summary>
    /// <para>Has NameNode field of type BASENODE.</para>
    /// <para>Derives from TYPEBASENODE.</para>
    /// </summary>
    //======================================================================
    internal class NAMEDTYPENODE : TYPEBASENODE
    {
        internal BASENODE NameNode = null;  // pName

#if DEBUG
        //------------------------------------------------------------
        // NAMEDTYPENODE.DebugNamedType1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNamedType1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode : null\n\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // NAMEDTYPENODE.DebugNamedType2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNamedType2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // NAMEDTYPENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNamedType1(sb);
            DebugNamedType2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // NAMEDTYPENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNamedType1(sb);
        }
#endif
    }

    //======================================================================
    // class ARRAYTYPENODE
    //
    /// <summary>
    /// Derives from TYPEBASENODE. Has the type and the dimensions of an array.
    /// </summary>
    //======================================================================
    internal class ARRAYTYPENODE : TYPEBASENODE
    {
        /// <summary>
        /// Type of an array.
        /// </summary>
        internal TYPEBASENODE ElementTypeNode = null;   // pElementType

        /// <summary>
        /// <para>Number of dimensions (-1 == unknown, i.e. [?])</para>
        /// </summary>
        internal int Dimensions = -1;    // iDims

#if DEBUG
        //------------------------------------------------------------
        // ARRAYTYPENODE.DebugArrayType1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugArrayType1(StringBuilder sb)
        {
            if (ElementTypeNode != null)
            {
                sb.AppendFormat("{0}: ElementTypeNode : No.{1}\n", NodeID, ElementTypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ElementTypeNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: Dimensions : {1}\n", NodeID, Dimensions);
        }

        //------------------------------------------------------------
        // ARRAYTYPENODE.DebugArrayType2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugArrayType2(StringBuilder sb)
        {
            if (ElementTypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ElementTypeNode of No.{0}]\n", NodeID);
                ElementTypeNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ARRAYTYPENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugArrayType1(sb);
            DebugArrayType2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // ARRAYTYPENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugArrayType1(sb);
        }
#endif
    }

    //======================================================================
    // class POINTERTYPENODE
    //
    /// <summary>
    /// Derives from TYPEBASENODE. Represents referred types.
    /// Has a field of type TYPEBASENODE.
    /// </summary>
    //======================================================================
    internal class POINTERTYPENODE : TYPEBASENODE
    {
        internal TYPEBASENODE ElementTypeNode = null;   // pElementType

#if DEBUG
        //------------------------------------------------------------
        // POINTERTYPENODE.DebugPointerType1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugPointerType1(StringBuilder sb)
        {
            if (ElementTypeNode != null)
            {
                sb.AppendFormat("{0}: ElementTypeNode : No.{1}\n", NodeID, ElementTypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ElementTypeNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // POINTERTYPENODE.DebugPointerType2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugPointerType2(StringBuilder sb)
        {
            if (ElementTypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ElementTypeNode of No.{0}]\n", NodeID);
                ElementTypeNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // POINTERTYPENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugPointerType1(sb);
            DebugPointerType2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // POINTERTYPENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugPointerType1(sb);
        }
#endif
    }

    //======================================================================
    // class IMPLICITTYPENODE
    //
    /// <summary>
    /// (CS3) Derived from TYPEBASENODE. Represents implicit type.
    /// </summary>
    //======================================================================
    internal class IMPLICITTYPENODE : TYPEBASENODE
    {
    }

    //======================================================================
    // class NULLABLETYPENODE
    //
    /// <summary>
    /// Derives from TYPEBASENODE. Represents nullable types.
    /// </summary>
    //======================================================================
    internal class NULLABLETYPENODE : TYPEBASENODE
    {
        internal TYPEBASENODE ElementTypeNode = null;   // pElementType

#if DEBUG
        //------------------------------------------------------------
        // NULLABLETYPENODE.DebugNullableType1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNullableType1(StringBuilder sb)
        {
            if (ElementTypeNode != null)
            {
                sb.AppendFormat("{0}: ElementTypeNode : No.{1}\n", NodeID, ElementTypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ElementTypeNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // NULLABLETYPENODE.DebugNullableType2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNullableType2(StringBuilder sb)
        {
            if (ElementTypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ElementTypeNode of No.{0}]\n", NodeID);
                ElementTypeNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // NULLABLETYPENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNullableType1(sb);
            DebugNullableType2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // NULLABLETYPENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNullableType1(sb);
        }
#endif
    }

    //======================================================================
    // class STATEMENTNODE
    //
    /// <summary>
    /// Represents a statement.
    /// Use NextNode as nest StatementNode.
    /// </summary>
    //======================================================================
    internal class STATEMENTNODE : BASENODE
    {
        /// <summary>
        /// Next statement.
        /// </summary>
        internal STATEMENTNODE NextStatementNode    // pNext
        {
            get { return this.NextNode as STATEMENTNODE; }
            set { this.NextNode = value; }
        }

        /// <summary>
        /// <para>(CS3)Set NODEFLAGS.NEW_* flags</para>
        /// </summary>
        internal NODEFLAGS NewFlags = 0;

        /// <summary>
        /// (CS3) This statement includes the object initializers.
        /// </summary>
        internal bool HasObjectInitializer
        {
            get { return (NewFlags & NODEFLAGS.NEW_HAS_OBJECT_INITIALIZER) != 0; }
        }

        /// <summary>
        /// (CS3) This statement includes the collection initializers.
        /// </summary>
        internal bool HasCollectionInitializer
        {
            get { return (NewFlags & NODEFLAGS.NEW_HAS_COLLECTION_INITIALIZER) != 0; }
        }

#if DEBUG
        //------------------------------------------------------------
        // STATEMENTNODE.DebugStatement1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugStatement1(StringBuilder sb)
        {
            if (NextStatementNode != null)
            {
                sb.AppendFormat("{0}: NextStatementNode   : No.{1}\n", NodeID, NextStatementNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NextStatementNode   :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // STATEMENTNODE.DebugStatement2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugStatement2(StringBuilder sb)
        {
            if (NextStatementNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NextStatementNode of No.{0}]\n", NodeID);
                NextStatementNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // STATEMENTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugStatement1(sb);
            DebugStatement2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // STATEMENTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugStatement1(sb);
        }
#endif
    }

    //======================================================================
    // class BLOCKNODE
    //
    /// <summary>
    /// <para>Derives from STATEMENTNODE. Represents code block.</para>
    /// <para>Has  the node of a first statement and the index of last '}'.</para>
    /// </summary>
    //======================================================================
    internal class BLOCKNODE : STATEMENTNODE
    {
        /// <summary>
        /// <para>List of statements.</para>
        /// <para>The first statement node of the list of statements.</para>
        /// </summary>
        internal STATEMENTNODE StatementsNode = null;   // pStatements

        /// <summary>
        /// Close curly
        /// </summary>
        internal int CloseCurlyIndex = -1;  // iClose

#if DEBUG
        //------------------------------------------------------------
        // BLOCKNODE.DebugBlock1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugBlock1(StringBuilder sb)
        {
            sb.AppendFormat("{0}: CloseCurlyIndex : {1}\n\n", NodeID, CloseCurlyIndex);

            if (StatementsNode != null)
            {
                sb.AppendFormat("{0}: StatementNode : No.{1}\n", NodeID, StatementsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: StatementNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // BLOCKNODE.DebugBlock2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugBlock2(StringBuilder sb)
        {
            if (StatementsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[StatementNode of No.{0}]\n", NodeID);
                StatementsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // BLOCKNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugBlock1(sb);
            DebugBlock2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // BLOCKNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugBlock1(sb);
        }
#endif
    }

    //======================================================================
    // class EXPRSTMTNODE
    //
    /// <summary>
    /// <para>EXPRSTMTNODE -- a multi-purpose statement node
    /// (expression statements, goto, case, default, return, etc.)</para>
    /// <para>Derives from STATEMENTNODE. Has ArgumentsNode field.
    /// <list type="bullet">
    /// <item>expression-statement    (NODEKIND.EXPRSTMT)</item>
    /// <item>yield-statement (NODEKIND.YIELD)</item>
    /// </list></para>
    /// </summary>
    //======================================================================
    internal class EXPRSTMTNODE : STATEMENTNODE
    {
        internal BASENODE ArgumentsNode = null; // pArg

#if DEBUG
        //------------------------------------------------------------
        // EXPRSTMTNODE.DebugExprStmt1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugExprStmt1(StringBuilder sb)
        {
            if (ArgumentsNode != null)
            {
                sb.AppendFormat("{0}: ArgumentsNode : No.{1}\n", NodeID, ArgumentsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ArgumentsNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // EXPRSTMTNODE.DebugExprStmt2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugExprStmt2(StringBuilder sb)
        {
            if (ArgumentsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ArgumentsNode of No.{0}]\n", NodeID);
                ArgumentsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // EXPRSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugExprStmt1(sb);
            DebugExprStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // EXPRSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugExprStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class LABELSTMTNODE
    //
    /// <summary>
    /// <para>Derives from STATEMENTNODE. Represents label statements.</para>
    /// <para>Used for check statement, uncheck statement. (NODEKIND.CHECKED)para>
    /// <para>Used for unsafe statement (NODEKIND.UNSAFE)</para>
    /// </summary>
    //======================================================================
    internal class LABELSTMTNODE : STATEMENTNODE
    {
        /// <summary>
        /// Label name
        /// </summary>
        internal BASENODE LabelNode = null; // pLabel

        /// <summary>
        /// Statement
        /// </summary>
        internal STATEMENTNODE StatementNode = null;    // pStmt

#if DEBUG
        //------------------------------------------------------------
        // LABELSTMTNODE.DebugLabelStmt1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLabelStmt1(StringBuilder sb)
        {
            if (LabelNode != null)
            {
                sb.AppendFormat("{0}: LabelNode : No.{1}\n", NodeID, LabelNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: LabelNode :\n", NodeID);
            }

            if (StatementNode != null)
            {
                sb.AppendFormat("{0}: StatementNode : No.{1}\n", NodeID, StatementNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: StatementNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // LABELSTMTNODE.DebugLabelStmt2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLabelStmt2(StringBuilder sb)
        {
            if (LabelNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[LabelNode of No.{0}]\n", NodeID);
                LabelNode.Debug(sb);
            }

            if (StatementNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[StatementNode of No.{0}]\n", NodeID);
                StatementNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // LABELSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLabelStmt1(sb);
            DebugLabelStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // LABELSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLabelStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class LOOPSTMTNODE
    //
    /// <summary>
    /// <para>Derives from STATEMENTNODE. Represents while statement or do statement.</para>
    /// <para>Kind field is set NODEKIND.WHILE or NODEKIND.DO respectively.</para>
    /// <para>Used for lock statement. (NODEKIND.LOCK)</para>
    /// </summary>
    //======================================================================
    internal class LOOPSTMTNODE : STATEMENTNODE
    {
        /// <summary>
        /// Expression
        /// </summary>
        internal BASENODE ExpressionNode = null;    // * pExpr

        /// <summary>
        /// Statement
        /// </summary>
        internal STATEMENTNODE StatementNode = null;    // * pStmt

#if DEBUG
        //------------------------------------------------------------
        // LOOPSTMTNODE.DebugLoopStmt1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLoopStmt1(StringBuilder sb)
        {
            if (ExpressionNode != null)
            {
                sb.AppendFormat("{0}: ExpressionNode : No.{1}\n", NodeID, ExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ExpressionNode :\n", NodeID);
            }

            if (StatementNode != null)
            {
                sb.AppendFormat("{0}: StatementNode : No.{1}\n", NodeID, StatementNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: StatementNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // LOOPSTMTNODE.DebugLoopStmt2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLoopStmt2(StringBuilder sb)
        {
            if (ExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ExpressionNode of No.{0}]\n", NodeID);
                ExpressionNode.Debug(sb);
            }

            if (StatementNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[StatementNode of No.{0}]\n", NodeID);
                StatementNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // LOOPSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLoopStmt1(sb);
            DebugLoopStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // LOOPSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLoopStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class FORSTMTNODE
    //
    /// <summary>
    /// <para>Derives from STATEMENTNODE. Represents for statement or foreach statement</para>
    /// <para>if foreach, set NODEFLAGS.FOR_FOREACH to Flags field.</para>
    /// <para>Used for fixed statement. In this case,
    /// set NODEFLAGS.FIXED_DECL to Flags and
    /// set nodes of fixed pointer declaration to InitialNode fields.</para>
    /// <para>Used for using statement. In this case,
    /// set NODEFLAGS.USING_DECL to Flags and
    /// set nodes of resources to InitialNode or ConditionNode.</para>
    /// </summary>
    //======================================================================
    internal class FORSTMTNODE : STATEMENTNODE
    {
        /// <summary>
        /// Init statement/expression(s)
        /// </summary>
        internal BASENODE InitialNode = null;   // pInit

        /// <summary>
        /// Condition expression
        /// </summary>
        internal BASENODE ExpressionNode = null; // pExpr

        /// <summary>
        /// Increment expression(s)
        /// </summary>
        internal BASENODE IncrementNode = null; // pInc

        /// <summary>
        /// Token index of close paren
        /// </summary>
        internal int CloseParenIndex = -1;  // iCloseParen

        /// <summary>
        /// Token index of in keyword in foreach stmt
        /// </summary>
        internal int InKeyword = 0; // iInKeyword

        /// <summary>
        /// Iterated statement
        /// </summary>
        internal STATEMENTNODE StatementNode = null;    // pStmt

#if DEBUG
        //------------------------------------------------------------
        // FORSTMTNODE.DebugForStmt1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugForStmt1(StringBuilder sb)
        {
            if (InitialNode != null)
            {
                sb.AppendFormat("{0}: InitialNode : No.{1}\n", NodeID, InitialNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: InitialNode :\n", NodeID);
            }

            if (ExpressionNode != null)
            {
                sb.AppendFormat("{0}: ConditionNode : No.{1}\n", NodeID, ExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ConditionNode :\n", NodeID);
            }

            if (IncrementNode != null)
            {
                sb.AppendFormat("{0}: IncrementNode : No.{1}\n", NodeID, IncrementNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: IncrementNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: CloseParenIndex : {1}\n", NodeID, CloseParenIndex);
            sb.AppendFormat("{0}: InKeyword       : {1}\n", NodeID, InKeyword);

            if (StatementNode != null)
            {
                sb.AppendFormat("{0}: StatementNode : No.{1}\n", NodeID, StatementNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: StatementNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // FORSTMTNODE.DebugForStmt2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugForStmt2(StringBuilder sb)
        {
            if (InitialNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[InitialNode of No.{0}]\n", NodeID);
                InitialNode.Debug(sb);
            }

            if (ExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ConditionNode of No.{0}]\n", NodeID);
                ExpressionNode.Debug(sb);
            }

            if (IncrementNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[IncrementNode of No.{0}]\n", NodeID);
                IncrementNode.Debug(sb);
            }

            if (StatementNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[StatementNode of No.{0}]\n", NodeID);
                StatementNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // FORSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugForStmt1(sb);
            DebugForStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // FORSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugForStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class IFSTMTNODE
    //
    /// <summary>
    /// Derives from STATEMENTNODE. Represents if statement.
    /// </summary>
    //======================================================================
    internal class IFSTMTNODE : STATEMENTNODE
    {
        /// <summary>
        /// Condition
        /// </summary>
        internal BASENODE ConditionNode = null; // pExpr

        /// <summary>
        /// TRUE statement
        /// </summary>
        internal STATEMENTNODE StatementNode = null;    // pStmt

        /// <summary>
        /// FALSE statement
        /// </summary>
        internal STATEMENTNODE ElseNode = null; // pElse

#if DEBUG
        //------------------------------------------------------------
        // IFSTMTNODE.DebugIfStmt1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugIfStmt1(StringBuilder sb)
        {
            if (ConditionNode != null)
            {
                sb.AppendFormat("{0}: ConditionNode : No.{1}\n", NodeID, ConditionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ConditionNode :\n", NodeID);
            }

            if (StatementNode != null)
            {
                sb.AppendFormat("{0}: StatementNode : No.{1}\n", NodeID, StatementNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: StatementNode :\n", NodeID);
            }

            if (ElseNode != null)
            {
                sb.AppendFormat("{0}: ElseNode : No.{1}\n", NodeID, ElseNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ElseNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // IFSTMTNODE.DebugIfStmt2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugIfStmt2(StringBuilder sb)
        {
            if (ConditionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ConditionNode of No.{0}]\n", NodeID);
                ConditionNode.Debug(sb);
            }

            if (StatementNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[StatementNode of No.{0}]\n", NodeID);
                StatementNode.Debug(sb);
            }

            if (ElseNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ElseNode of No.{0}]\n", NodeID);
                ElseNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // IFSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugIfStmt1(sb);
            DebugIfStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // IFSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugIfStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class DECLSTMTNODE
    //
    /// <summary>
    /// <para>Derives from STATEMENTNODE. Represents declarations of variables.</para>
    /// <para>This node represents local-variable-declaration
    /// <code>type id ["=" initializer] ["," id ["=" initializer]]";"</code>
    /// </para>
    /// </summary>
    //======================================================================
    internal class DECLSTMTNODE : STATEMENTNODE
    {
        /// <summary>
        /// // Type of declaration
        /// </summary>
        internal TYPEBASENODE TypeNode = null;  // pType

        /// <summary>
        /// <para>Declared variable list</para>
        /// <para>This is the first node of the list.</para>
        /// </summary>
        internal BASENODE VariablesNode = null; // pVars

#if DEBUG
        //------------------------------------------------------------
        // DECLSTMTNODE.DebugBlock1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugDeclStmt1(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, TypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode :\n", NodeID);
            }

            if (VariablesNode != null)
            {
                sb.AppendFormat("{0}: VariablesNode : No.{1}\n", NodeID, VariablesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: VariablesNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // DECLSTMTNODE.DebugBlock2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugDeclStmt2(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeNode of No.{0}]\n", NodeID);
                TypeNode.Debug(sb);
            }

            if (VariablesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[VariablesNode of No.{0}]\n", NodeID);
                VariablesNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // DECLSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugDeclStmt1(sb);
            DebugDeclStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // DECLSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugDeclStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class SWITCHSTMTNODE
    //
    /// <summary>
    /// switch 文を表すための STATEMENTNODE の派生クラス。
    /// </summary>
    //======================================================================
    internal class SWITCHSTMTNODE : STATEMENTNODE
    {
        internal BASENODE ExpressionNode = null;    // * pExpr

        internal BASENODE CasesNode = null; // * pCases

        /// <summary>
        /// Open curly
        /// </summary>
        internal int OpenCurlyIndex = -1;   // long iOpen;

        /// <summary>
        /// Close curly
        /// </summary>
        internal int CloseCurlyIndex = -1;  // long iClose;

#if DEBUG
        //------------------------------------------------------------
        // SWITCHSTMTNODE.DebugBlock1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugSwitchStmt1(StringBuilder sb)
        {
            if (ExpressionNode != null)
            {
                sb.AppendFormat("{0}: ExpressionNode : No.{1}\n", NodeID, ExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ExpressionNode :\n", NodeID);
            }

            if (CasesNode != null)
            {
                sb.AppendFormat("{0}: CasesNode : No.{1}\n", NodeID, CasesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: CasesNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenCurlyIndex  : {1}\n", NodeID, OpenCurlyIndex);
            sb.AppendFormat("{0}: CloseCurlyIndex : {1}\n", NodeID, CloseCurlyIndex);
        }

        //------------------------------------------------------------
        // SWITCHSTMTNODE.DebugBlock2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugSwitchStmt2(StringBuilder sb)
        {
            if (ExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ExpressionNode of No.{0}]\n", NodeID);
                ExpressionNode.Debug(sb);
            }

            if (CasesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[CasesNode of No.{0}]\n", NodeID);
                CasesNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // SWITCHSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugSwitchStmt1(sb);
            DebugSwitchStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // SWITCHSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugSwitchStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class CASENODE
    //
    /// <summary>
    /// case 文を表すための BASENODE の派生クラス。
    /// </summary>
    //======================================================================
    internal class CASENODE : BASENODE
    {
        internal BASENODE LabelsNode = null;            // * pLabels;
        internal STATEMENTNODE StatementsNode = null;   // * pStmts;

#if DEBUG
        //------------------------------------------------------------
        // CASENODE.DebugCase1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugCase1(StringBuilder sb)
        {
            if (LabelsNode != null)
            {
                sb.AppendFormat("{0}: LabelsNode : No.{1}\n", NodeID, LabelsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: LabelsNode :\n", NodeID);
            }

            if (StatementsNode != null)
            {
                sb.AppendFormat("{0}: StatementsNode : No.{1}\n", NodeID, StatementsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: StatementsNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // CASENODE.DebugCase2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugCase2(StringBuilder sb)
        {
            if (LabelsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[LabelsNode of No.{0}]\n", NodeID);
                LabelsNode.Debug(sb);
            }

            if (StatementsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[StatementsNode of No.{0}]\n", NodeID);
                StatementsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // CASENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugCase1(sb);
            DebugCase2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // CASENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugCase1(sb);
        }
#endif
    }

    //======================================================================
    // class TRYSTMTNODE
    //
    /// <summary>
    /// <para>try-catch 文または try-finally 文を表すための STATEMENTNODE の派生クラス。</para>
    /// <para>try-catch-finally は入れ子 try{tyr-catch}finally{} と考えて処理する。</para>
    /// <para>catch か finally かは Flags を使って示す。
    /// try-catch とする場合は NODEFLAGS.TRY_CATCH ビットを、
    /// try-finally とする場合は NODEFLAGS.TRY_FINALLY ビットをセットする。</para>
    /// </summary>
    //======================================================================
    internal class TRYSTMTNODE : STATEMENTNODE
    {
        internal BLOCKNODE BlockNode = null;    // * pBlock

        /// <summary>
        /// catch ブロックの列、または finally ブロック。
        /// </summary>
        internal BASENODE CatchNode = null; // * pCatch

#if DEBUG
        //------------------------------------------------------------
        // TRYSTMTNODE.DebugTryStmt1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugTryStmt1(StringBuilder sb)
        {
            if (BlockNode != null)
            {
                sb.AppendFormat("{0}: BlockNode : No.{1}\n", NodeID, BlockNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: BlockNode :\n", NodeID);
            }

            if (CatchNode != null)
            {
                sb.AppendFormat("{0}: CatchNode : No.{1}\n", NodeID, CatchNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: CatchNode :\n", NodeID);
            }

            sb.Append("\n");
        }

        //------------------------------------------------------------
        // TRYSTMTNODE.DebugTryStmt2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugTryStmt2(StringBuilder sb)
        {
            if (BlockNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[BlockNode of No.{0}]\n", NodeID);
                BlockNode.Debug(sb);
            }

            if (CatchNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[CatchNode of No.{0}]\n", NodeID);
                CatchNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // TRYSTMTNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugTryStmt1(sb);
            DebugTryStmt2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // TRYSTMTNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugTryStmt1(sb);
        }
#endif
    }

    //======================================================================
    // class CATCHNODE
    //
    /// <summary>
    /// <para>catch ブロックまたは finally ブロックを表すための BASENODE の派生クラス。</para>
    /// <para>親となる TRYSTMTNODE インスタンスの Flags を使って catch か finally かを示す。</para>
    /// <para>型、名前、ブロックを表すノードを持つ。</para>
    /// </summary>
    //======================================================================
    internal class CATCHNODE : BASENODE
    {
        internal BASENODE TypeNode = null;      // * pType
        internal NAMENODE NameNode = null;      // * pName
        internal BLOCKNODE BlockNode = null;    // * pBlock

#if DEBUG
        //------------------------------------------------------------
        // CATCHNODE.DebugCatch1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugCatch1(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, TypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode :\n", NodeID);
            }

            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (BlockNode != null)
            {
                sb.AppendFormat("{0}: BlockNode : No.{1}\n", NodeID, BlockNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: BlockNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // CATCHNODE.DebugCatch2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugCatch2(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeNode of No.{0}]\n", NodeID);
                TypeNode.Debug(sb);
            }

            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (BlockNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[BlockNode of No.{0}]\n", NodeID);
                BlockNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // CATCHNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugCatch1(sb);
            DebugCatch2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // CATCHNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugCatch1(sb);
        }
#endif
    }

    //======================================================================
    // class UNOPNODE
    //
    /// <summary>
    /// 単項演算子を表すノード。それの対象となる項をフィールドに持つ。
    /// </summary>
    //======================================================================
    internal class UNOPNODE : BASENODE
    {
        // OPERATOR は BASENODE で定義されている。

        internal BASENODE Operand = null;   // p1

#if DEBUG
        //------------------------------------------------------------
        // UNOPNODE.DebugUniOp1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugUniOp1(StringBuilder sb)
        {
            if (Operand != null)
            {
                sb.AppendFormat("{0}: Operand : No.{1}\n", NodeID, Operand.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: Operand :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // UNOPNODE.DebugUniOp2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugUniOp2(StringBuilder sb)
        {
            if (Operand != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[Operand of No.{0}]\n", NodeID);
                Operand.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // UNOPNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugUniOp1(sb);
            DebugUniOp2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // UNOPNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugUniOp1(sb);
        }
#endif
    }

    //======================================================================
    // class BINOPNODE
    //
    /// <summary>
    /// Represents a binary operator.
    /// Has two nodes for operands.
    /// </summary>
    //======================================================================
    internal class BINOPNODE : BASENODE
    {
        // OPERATOR is defined in BASENODE.

        internal BASENODE Operand1 = null;  // p1
        internal BASENODE Operand2 = null;  // p2

#if DEBUG
        //------------------------------------------------------------
        // BINOPNODE.DebugBinOp1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugBinOp1(StringBuilder sb)
        {
            sb.AppendFormat("{0}: Operator : {1}\n", NodeID, this.Operator);

            if (Operand1 != null)
            {
                sb.AppendFormat("{0}: Operand1 : No.{1}\n", NodeID, Operand1.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: Operand1 :\n", NodeID);
            }

            if (Operand2 != null)
            {
                sb.AppendFormat("{0}: Operand2 : No.{1}\n", NodeID, Operand2.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: Operand2 :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // BINOPNODE.DebugBinOp2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugBinOp2(StringBuilder sb)
        {
            if (Operand1 != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[Operand1 of No.{0}]\n", NodeID);
                Operand1.Debug(sb);
            }

            if (Operand2 != null)
            {
                sb.AppendFormat("[Operand2 of No.{0}]\n", NodeID);
                Operand2.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // BINOPNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugBinOp1(sb);
            DebugBinOp2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // BINOPNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugBinOp1(sb);
        }
#endif
    }

    //======================================================================
    // class CALLNODE
    //
    /// <summary>
    /// Derives form BINOPNODE. Represents calling functions.
    /// </summary>
    //======================================================================
    internal class CALLNODE : BINOPNODE
    {
        /// <summary>
        /// Token index of close paren
        /// </summary>
        internal int CloseParenIndex = 0;   // iClose

#if DEBUG
        //------------------------------------------------------------
        // CALLNODE.DebugCall
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugCall(StringBuilder sb)
        {
            sb.AppendFormat("{0}: CloseParenIndex : {1}\n", NodeID, CloseParenIndex);
        }

        //------------------------------------------------------------
        // CALLNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugBinOp1(sb);
            DebugCall(sb);
            DebugBinOp2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // CALLNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugBinOp1(sb);
            DebugCall(sb);
        }
#endif
    }

    //======================================================================
    // class CONSTVALNODE
    //
    /// <summary>
    /// Class to store constant values. Has one field of type CONSTVAL.
    /// </summary>
    //======================================================================
    internal class CONSTVALNODE : BASENODE
    {
        /// <summary>
        /// Value of constant
        /// </summary>
        internal CONSTVAL Value = new CONSTVAL();   // val

#if DEBUG
        //------------------------------------------------------------
        // CONSTVALNODE.DebugCall
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugConstVal(StringBuilder sb)
        {
            sb.AppendFormat("{0}: Value: {1}\n", NodeID, Value.GetObject());
        }

        //------------------------------------------------------------
        // CONSTVALNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugConstVal(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // CONSTVALNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugConstVal(sb);
        }
#endif
    }

    //======================================================================
    // class VARDECLNODE
    //
    /// <summary>
    /// <para>Represents a declaration of a variable.
    /// <code>identifier [= initializer]</code>
    /// </para>
    /// <para>Have a reference to the node reprsenting whole of this declaration.</para>
    /// </summary>
    //======================================================================
    internal class VARDECLNODE : BASENODE
    {
        /// <summary>
        /// Name of variable.
        /// </summary>
        internal NAMENODE NameNode = null;  // pName

        /// <summary>
        /// <para>Init expression or array dim expression</para>
        /// </summary>
        internal BASENODE ArgumentsNode = null; // pArg

        /// <summary>
        /// <para>Pointer to parent decl node</para>
        /// </summary>
        internal BASENODE DeclarationsNode = null;  // pDecl

        /// <summary>
        /// <para>(CS3)Set NODEFLAGS.NEW_* flags</para>
        /// </summary>
        internal NODEFLAGS NewFlags = 0;

        /// <summary>
        /// (CS3) This statement includes the object initializers.
        /// </summary>
        internal bool HasObjectInitializer
        {
            get { return (NewFlags & NODEFLAGS.NEW_HAS_OBJECT_INITIALIZER) != 0; }
        }

        /// <summary>
        /// (CS3) This statement includes the collection initializers.
        /// </summary>
        internal bool HasCollectionInitializer
        {
            get { return (NewFlags & NODEFLAGS.NEW_HAS_COLLECTION_INITIALIZER) != 0; }
        }

        //------------------------------------------------------------
        // VARDECLNODE.GetParent
        //
        /// <summary>
        /// <para>Returns the DeclarationsNode.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override BASENODE GetParent()
        {
            return this.DeclarationsNode;
        }
        //__forceinline BASENODE *BASENODE::GetParent () 
        //{
        //    if (kind == NK_VARDECL) 
        //        return this->asVARDECL()->pDecl; 
        //    return this->pParent; 
        //}

#if DEBUG
        //------------------------------------------------------------
        // VARDECLNODE.DebugVarDecl1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugVarDecl1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (ArgumentsNode != null)
            {
                sb.AppendFormat("{0}: ArgumentsNode : No.{1}\n", NodeID, ArgumentsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ArgumentsNode :\n", NodeID);
            }

            if (DeclarationsNode != null)
            {
                sb.AppendFormat("{0}: DeclarationsNode : No.{1}\n", NodeID, DeclarationsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: DeclarationsNode : \n", NodeID);
            }

            if (HasObjectInitializer == true)
            {
                sb.AppendFormat("{0}: HasObjectInitializer : {1}\n", NodeID, true);
            }
        }

        //------------------------------------------------------------
        // VARDECLNODE.DebugVarDecl2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugVarDecl2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (ArgumentsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ArgumentsNode of No.{0}]\n", NodeID);
                ArgumentsNode.Debug(sb);
            }

            if (DeclarationsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[DeclarationsNode of No.{0}]\n", NodeID);
                DeclarationsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // VARDECLNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugVarDecl1(sb);
            DebugVarDecl2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // VARDECLNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugVarDecl1(sb);
        }
#endif
    }

    //======================================================================
    // class NEWNODE
    //
    /// <summary>
    /// Represents creation-expressions by new.
    /// </summary>
    //======================================================================
    internal class NEWNODE : BASENODE
    {
        /// <summary></summary>
        internal TYPEBASENODE TypeNode = null;  // * pType

        /// <summary>
        /// Constructor args or dim expression list
        /// </summary>
        internal BASENODE ArgumentsNode = null; // * pArgs

        /// <summary>
        /// <para>Array initializer</para>
        /// <para>(CS3) anonymous-object-initializer,
        /// object-or-collection-initializer</para>
        /// </summary>
        internal BASENODE InitialNode = null;   // * pInit

        /// <summary>
        /// Token index of '('
        /// </summary>
        internal int OpenParenIndex = 0;    // long iOpen;

        /// <summary>
        /// Token index of ')' or ']' for array
        /// </summary>
        internal int CloseParenIndex = 0;   // long iClose;

        /// <summary>
        /// (CS3)
        /// </summary>
        internal bool InVarDeclStatement
        {
            get { return (this.Flags & NODEFLAGS.NEW_IN_VARDECL) != 0; }
        }

        /// <summary>
        /// (CS3)
        /// </summary>
        internal bool HasObjectInitializer
        {
            get { return (this.Flags & NODEFLAGS.NEW_HAS_OBJECT_INITIALIZER) != 0; }
        }

        /// <summary>
        /// (CS3)
        /// </summary>
        internal bool HasCollectionInitializer
        {
            get { return (this.Flags & NODEFLAGS.NEW_HAS_COLLECTION_INITIALIZER) != 0; }
        }

        /// <summary>
        /// (CS3)
        /// </summary>
        internal bool IsAnonymousObjectCreation
        {
            get { return (this.Flags & NODEFLAGS.NEW_ANONYMOUS_OBJECT_CREATION) != 0; }
        }

        /// <summary>
        /// (CS3)
        /// </summary>
        internal bool IsImplicitlyTypedArray
        {
            get { return (this.Flags & NODEFLAGS.NEW_IMPLICITLY_TYPED_ARRAY) != 0; }
        }

#if DEBUG
        //------------------------------------------------------------
        // NEWNODE.DebugNew1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNew1(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.AppendFormat("{0}: TypeNode : No.{1}\n", NodeID, TypeNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeNode :\n", NodeID);
            }

            if (ArgumentsNode != null)
            {
                sb.AppendFormat("{0}: ArgumentsNode : No.{1}\n", NodeID, ArgumentsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ArgumentsNode :\n", NodeID);
            }

            if (InitialNode != null)
            {
                sb.AppendFormat("{0}: InitialNode : No.{1}\n", NodeID, InitialNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: InitialNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenParenIndex  : {1}\n", NodeID, OpenParenIndex);
            sb.AppendFormat("{0}: CloseParenIndex : {1}\n", NodeID, CloseParenIndex);
        }

        //------------------------------------------------------------
        // NEWNODE.DebugNew2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugNew2(StringBuilder sb)
        {
            if (TypeNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeNode of No.{0}]\n", NodeID);
                TypeNode.Debug(sb);
            }

            if (ArgumentsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ArgumentsNode of No.{0}]\n", NodeID);
                ArgumentsNode.Debug(sb);
            }

            if (InitialNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[InitialNode of No.{0}]\n", NodeID);
                InitialNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // NEWNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNew1(sb);
            DebugNew2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // NEWNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugNew1(sb);
        }
#endif
    }

    //======================================================================
    // class ATTRNODE
    //
    /// <summary>
    /// 属性の指定を表すためのクラス。引数のノードをフィールドに持つ。
    /// </summary>
    //======================================================================
    internal class ATTRNODE : BASENODE
    {
        /// <summary>
        /// Attribute name (possibly dotted, or possibly NULL if NK_ATTRARG)
        /// </summary>
        internal BASENODE NameNode = null;  // pName
        
        /// <summary>
        /// <para>Arguments (NK_ATTR) or expression (NK_ATTRARG)</para>
        /// <para>The first node of the linked nodes which represent arguments of attribute.</para>
        /// </summary>
        internal BASENODE ArgumentsNode = null; // pArgs

        /// <summary>
        /// Token index of (
        /// </summary>
        internal int OpenParenIndex = -1;   // iOpen

        /// <summary>
        /// Token index of )
        /// </summary>
        internal int CloseParenIndex = -1;  // iClose

        /// <summary>
        /// True if this attribute has been processed.
        /// </summary>
        internal bool Emitted = false;

#if DEBUG
        //------------------------------------------------------------
        // ATTRNODE.DebugAttr1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAttr1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            if (ArgumentsNode != null)
            {
                sb.AppendFormat("{0}: ArgumentsNode : No.{1}\n", NodeID, ArgumentsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ArgumentsNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: OpenParenIndex  : {1}\n", NodeID, OpenParenIndex);
            sb.AppendFormat("{0}: CloseParenIndex : {1}\n", NodeID, CloseParenIndex);
        }

        //------------------------------------------------------------
        // ATTRNODE.DebugAttr2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAttr2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (ArgumentsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ArgumentsNode of No.{0}]\n", NodeID);
                ArgumentsNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ATTRNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAttr1(sb);
            DebugAttr2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // ATTRNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAttr1(sb);
        }
#endif
    }

    //======================================================================
    // class ATTRDECLNODE
    //
    /// <summary>
    /// Class for declaration of attributes.
    /// </summary>
    //======================================================================
    internal class ATTRDECLNODE : BASENODE
    {
        /// <summary>
        /// User specified attr location. null if default.
        /// </summary>
        internal NAMENODE NameNode = null;  // pNameNode

        /// <summary>
        /// <para>Attribute location allways valid.</para>
        /// <para>Flags of type ATTRTARGET to indicate targes of attribute.</para>
        /// </summary>
        internal ATTRTARGET Target = ATTRTARGET.NONE;   // location

        /// <summary>
        /// Token index of ']'
        /// </summary>
        internal int CloseSquareIndex = 0;  // iClose

        /// <summary>
        /// Attribute list
        /// </summary>
        internal BASENODE AttributesNode = null;    // pAttr

#if DEBUG
        //------------------------------------------------------------
        // ATTRDECLNODE.DebugAttrDecl1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAttrDecl1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode : No.{1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: Target           : {1}\n", NodeID, Target);
            sb.AppendFormat("{0}: CloseSquareIndex : {1}\n", NodeID, CloseSquareIndex);

            if (AttributesNode != null)
            {
                sb.AppendFormat("{0}: AttributesNode : No.{1}\n", NodeID, AttributesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: AttributesNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // ATTRDECLNODE.DebugAttrDecl2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAttrDecl2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (AttributesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[AttributesNode of No.{0}]\n", NodeID);
                AttributesNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ATTRDECLNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAttrDecl1(sb);
            DebugAttrDecl2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // ATTRDECLNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAttrDecl1(sb);
        }
#endif
    }

    //======================================================================
    // class ANONBLOCKNODE
    //
    /// <summary>
    /// <para>Derives from BASENODE. Represents anonymous methods.</para>
    /// <para>Has a list of arguments, code block, token index of last paren.</para>
    /// </summary>
    //======================================================================
    internal class ANONBLOCKNODE : BASENODE
    {
        /// <summary>
        /// List of parameters
        /// </summary>
        internal BASENODE ArgumentsNode = null; // * pArgs

        /// <summary>
        /// <para>Body of the anonymous method block</para>
        /// </summary>
        internal BLOCKNODE BodyNode = null; // * pBody // sscli2.0
        //internal BASENODE BodyNode = null;  // (CS3) for LAMBDAEXPRNODE

        /// <summary>
        /// Token index of the close paren (if it exists)
        /// </summary>
        internal int CloseParenIndex = 0;   // iClose

#if DEBUG
        //------------------------------------------------------------
        // ANONBLOCKNODE.DebugAnonBlock1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAnonBlock1(StringBuilder sb)
        {
            if (ArgumentsNode != null)
            {
                sb.AppendFormat("{0}: ArgumentsNode : No.{1}\n", NodeID, ArgumentsNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ArgumentsNode :\n", NodeID);
            }

            if (BodyNode != null)
            {
                sb.AppendFormat("{0}: BodyNode : No.{1}\n", NodeID, BodyNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: BodyNode :\n", NodeID);
            }

            sb.AppendFormat("{0}: CloseParenIndex : {1}\n", NodeID, CloseParenIndex);
        }

        //------------------------------------------------------------
        // ANONBLOCKNODE.DebugAnonBlock2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugAnonBlock2(StringBuilder sb)
        {
            if (ArgumentsNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ArgumentsNode of No.{0}]\n", NodeID);
                ArgumentsNode.Debug(sb);
            }

            if (BodyNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[BodyNode of No.{0}]\n", NodeID);
                BodyNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ANONBLOCKNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAnonBlock1(sb);
            DebugAnonBlock2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // ANONBLOCKNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugAnonBlock1(sb);
        }
#endif
    }

    //======================================================================
    // class LAMBDAEXPRNODE
    //
    /// <summary>
    /// (CS3) Represents a lambda expression. Derives from ANONBLOCKNODE.
    /// </summary>
    //======================================================================
    internal class LAMBDAEXPRNODE : ANONBLOCKNODE
    {
        /// <summary>
        /// If the body is an expression, set it.
        /// </summary>
        internal BASENODE ExpressionNode = null;

        /// <summary>
        /// True if the body is an expression.
        /// </summary>
        internal bool HasExpressionBody
        {
            get { return (this.ExpressionNode != null); }
        }

#if DEBUG
        //------------------------------------------------------------
        // LAMBDAEXPRNODE.DebugLambdaBlock1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLambdaBlock1(StringBuilder sb)
        {
            DebugAnonBlock1(sb);
        }

        //------------------------------------------------------------
        // LAMBDAEXPRNODE.DebugLambdaBlock2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLambdaBlock2(StringBuilder sb)
        {
            DebugAnonBlock2(sb);
        }

        //------------------------------------------------------------
        // LAMBDAEXPRNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLambdaBlock1(sb);
            DebugLambdaBlock2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // LAMBDAEXPRNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLambdaBlock1(sb);
        }
#endif
    }

    //======================================================================
    // class TYPEWITHATTRNODE
    //
    /// <summary>
    /// Represents a node for a type with attributes.
    /// </summary>
    //======================================================================
    internal class TYPEWITHATTRNODE : TYPEBASENODE
    {
        /// <summary>
        /// Attributes
        /// </summary>
        internal BASENODE AttributesNode = null;    // pAttr

        /// <summary>
        /// Real type
        /// </summary>
        internal TYPEBASENODE TypeBaseNode = null;  // pType

#if DEBUG
        //------------------------------------------------------------
        // TYPEWITHATTRNODE.DebugTypeWithAttr1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugTypeWithAttr1(StringBuilder sb)
        {
            if (AttributesNode != null)
            {
                sb.AppendFormat("{0}: AttriburesNode : No.{1}\n", NodeID, AttributesNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: AttriburesNode :\n", NodeID);
            }

            if (TypeBaseNode != null)
            {
                sb.AppendFormat("{0}: TypeBaseNode : No.{1}\n", NodeID, TypeBaseNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: TypeBaseNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // TYPEWITHATTRNODE.DebugTypeWithAttr2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugTypeWithAttr2(StringBuilder sb)
        {
            if (AttributesNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[AttriburesNode of No.{0}]\n", NodeID);
                AttributesNode.Debug(sb);
            }

            if (TypeBaseNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[TypeBaseNode of No.{0}]\n", NodeID);
                TypeBaseNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // TYPEWITHATTRNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugTypeWithAttr1(sb);
            DebugTypeWithAttr2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // TYPEWITHATTRNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugTypeWithAttr1(sb);
        }
#endif
    }

    //======================================================================
    // class QUERYEXPRNODE
    //
    /// <summary>
    /// (CS3) Represents a query expression.
    /// </summary>
    //======================================================================
    internal class QUERYEXPRNODE : BASENODE
    {
        internal FROMCLAUSENODE FromNode = null;
        internal BASENODE QueryBodyNode = null;

#if DEBUG
        //------------------------------------------------------------
        // QUERYEXPRNODE.DebugQueryExpr1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugQueryExpr1(StringBuilder sb)
        {
            if (FromNode != null)
            {
                sb.AppendFormat("{0}: FromNode      : No.{1}\n", NodeID, FromNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: FromNode      :\n", NodeID);
            }

            if (QueryBodyNode != null)
            {
                sb.AppendFormat("{0}: QueryBodyNode : No.{1}\n", NodeID, QueryBodyNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: QueryBodyNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // QUERYEXPRNODE.DebugQueryExpr2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugQueryExpr2(StringBuilder sb)
        {
            if (FromNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[FromNode of No.{0}]\n", NodeID);
                FromNode.Debug(sb);
            }

            if (QueryBodyNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[QueryBodyNode of No.{0}]\n", NodeID);
                QueryBodyNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // QUERYEXPRNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugQueryExpr1(sb);
            DebugQueryExpr2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // QUERYEXPRNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugQueryExpr1(sb);
        }
#endif
    }

    //======================================================================
    // class FROMCLAUSENODE
    //
    /// <summary>
    /// (CS3) Represents a from clause of a query expression.
    /// </summary>
    //======================================================================
    internal class FROMCLAUSENODE : BASENODE
    {
        internal PARAMETERNODE ParameterNode = null;

        internal BASENODE ExpressionNode = null;

#if DEBUG
        //------------------------------------------------------------
        // FROMCLAUSENODE.DebugFromClause1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugFromClause1(StringBuilder sb)
        {
            if (ParameterNode != null)
            {
                sb.AppendFormat(
                    "{0}: ParameterNode  : No.{1}\n",
                    NodeID,
                    ParameterNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ParameterNode  :\n", NodeID);
            }

            if (ExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: ExpressionNode : No.{1}\n",
                    NodeID,
                    ExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ExpressionNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // FROMCLAUSENODE.DebugFromClause2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugFromClause2(StringBuilder sb)
        {
            if (ParameterNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ParameterNode of No.{0}]\n", NodeID);
                ParameterNode.Debug(sb);
            }

            if (ExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ExpressionNode of No.{0}]\n", NodeID);
                ExpressionNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // FROMCLAUSENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugFromClause1(sb);
            DebugFromClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // FROMCLAUSENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugFromClause1(sb);
        }
#endif
    }

    //======================================================================
    // class FROMCLAUSENODE2
    //
    /// <summary>
    /// (CS3) Represents a following from clause of a query expression.
    /// </summary>
    //======================================================================
    internal class FROMCLAUSENODE2 : FROMCLAUSENODE
    {
        internal LAMBDAEXPRNODE OutputLambdaExpressionNode = null;

        internal string ParameterProductName = null;

#if DEBUG
        //------------------------------------------------------------
        // FROMCLAUSENODE2.DebugFromClause21
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugFromClause21(StringBuilder sb)
        {
            if (OutputLambdaExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: OutputNode        : No.{1}\n",
                    NodeID,
                    OutputLambdaExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat(
                    "{0}: OutputNode        :\n",
                    NodeID);
            }

            sb.AppendFormat(
                "{0}: ParameterProduct  : No.{1}\n",
                NodeID,
                ParameterProductName);
        }

        //------------------------------------------------------------
        // FROMCLAUSENODE2.DebugFromClause22
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugFromClause22(StringBuilder sb)
        {
            if (OutputLambdaExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[OutputNode of No.{0}]\n", NodeID);
                OutputLambdaExpressionNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // FROMCLAUSENODE2.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugFromClause1(sb);
            DebugFromClause21(sb);
            DebugFromClause22(sb);
            DebugFromClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // FROMCLAUSENODE2.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugFromClause1(sb);
            DebugFromClause21(sb);
        }
#endif
    }

    //======================================================================
    // class LETCLAUSENODE
    //
    /// <summary>
    /// (CS3) Represents a let clause of a query expression.
    /// </summary>
    //======================================================================
    internal class LETCLAUSENODE : BASENODE
    {
        internal NAMENODE NameNode = null;

        internal BASENODE ExpressionNode = null;

        internal LAMBDAEXPRNODE OutputLambdaExpressionNode = null;

        internal string ParameterProductName = null;

#if DEBUG
        //------------------------------------------------------------
        // LETCLAUSENODE.DebugLetClause1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLetClause1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode       : {1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode       :\n", NodeID);
            }

            if (ExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: ExpressionNode : No.{1}\n",
                    NodeID,
                    ExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ExpressionNode :\n", NodeID);
            }

            if (OutputLambdaExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: OutputNode     : No.{1}\n",
                    NodeID,
                    OutputLambdaExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: OutputNode     :\n", NodeID);
            }

            sb.AppendFormat(
                "{0}: ParameterProduct : No.{1}\n",
                NodeID,
                ParameterProductName);
        }

        //------------------------------------------------------------
        // LETCLAUSENODE.DebugLetClause2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugLetClause2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (ExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ExpressionNode of No.{0}]\n", NodeID);
                ExpressionNode.Debug(sb);
            }

            if (OutputLambdaExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ExpressionNode of No.{0}]\n", NodeID);
                OutputLambdaExpressionNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // LETCLAUSENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLetClause1(sb);
            DebugLetClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // LETCLAUSENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugLetClause1(sb);
        }
#endif
    }

    //======================================================================
    // class WHERECLAUSENODE
    //
    /// <summary>
    /// (CS3) Represents a where clause of a query expression.
    /// </summary>
    //======================================================================
    internal class WHERECLAUSENODE : BASENODE
    {
        internal LAMBDAEXPRNODE LambdaExpressionNode = null;

#if DEBUG
        //------------------------------------------------------------
        // WHERECLAUSENODE.DebugWhereClause1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugWhereClause1(StringBuilder sb)
        {
            if (LambdaExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: LambdaExpressionNode : No.{1}\n",
                    NodeID,
                    LambdaExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: LambdaExpressionNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // WHERECLAUSENODE.DebugWhereClause2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugWhereClause2(StringBuilder sb)
        {
            if (LambdaExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[LambdaExpressionNode of No.{0}]\n", NodeID);
                LambdaExpressionNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // WHERECLAUSENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugWhereClause1(sb);
            DebugWhereClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // WHERECLAUSENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugWhereClause1(sb);
        }
#endif
    }

    //======================================================================
    // class JOINCLAUSENODE
    //
    /// <summary>
    /// (CS3) Represents a join clause of a query expression.
    /// </summary>
    //======================================================================
    internal class JOINCLAUSENODE : BASENODE
    {
        internal FROMCLAUSENODE InNode = null;

        internal LAMBDAEXPRNODE EqualLeftLambdaExpressionNode = null;

        internal LAMBDAEXPRNODE EqualRightLambdaExpressionNode = null;

        internal NAMENODE IntoNameNode = null;

        internal bool IsJoinIntoClause
        {
            get { return IntoNameNode != null; }
        }

        internal LAMBDAEXPRNODE OutputLambdaExpressionNode = null;

        internal string ParameterProductName = null;

#if DEBUG
        //------------------------------------------------------------
        // JOINCLAUSENODE.DebugJoinClause1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugJoinClause1(StringBuilder sb)
        {
            if (InNode != null)
            {
                sb.AppendFormat(
                    "{0}: InNode            : No.{1}\n",
                    NodeID,
                    InNode.NodeID);
            }
            else
            {
                sb.AppendFormat(
                    "{0}: InNode            :\n",
                    NodeID);
            }

            if (EqualLeftLambdaExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: EqualLeftNode     : No.{1}\n",
                    NodeID,
                    EqualLeftLambdaExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat(
                    "{0}: EqualLeftNode     :\n",
                    NodeID);
            }

            if (EqualRightLambdaExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: EqualRightNode    : No.{1}\n",
                    NodeID,
                    EqualRightLambdaExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat(
                    "{0}: EqualRightNode    :\n",
                    NodeID);
            }

            if (IntoNameNode != null)
            {
                sb.AppendFormat(
                    "{0}: IntoName          : {1}\n",
                    NodeID,
                    IntoNameNode.Name);
            }
            else
            {
                sb.AppendFormat(
                    "{0}: IntoName          :\n",
                    NodeID);
            }

            if (OutputLambdaExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: OutputNode        : No.{1}\n",
                    NodeID,
                    OutputLambdaExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat(
                    "{0}: OutputNode        :\n",
                    NodeID);
            }

            sb.AppendFormat(
                "{0}: ParameterProduct  : No.{1}\n",
                NodeID,
                ParameterProductName);
        }

        //------------------------------------------------------------
        // JOINCLAUSENODE.DebugJoinClause2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugJoinClause2(StringBuilder sb)
        {
            if (InNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[InNode of No.{0}]\n", NodeID);
                InNode.Debug(sb);
            }

            if (EqualLeftLambdaExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[EqualLeft of No.{0}]\n", NodeID);
                EqualLeftLambdaExpressionNode.Debug(sb);
            }

            if (EqualRightLambdaExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[EqualRightNode of No.{0}]\n", NodeID);
                EqualRightLambdaExpressionNode.Debug(sb);
            }

            if (IntoNameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[IntoNode of No.{0}]\n", NodeID);
                IntoNameNode.Debug(sb);
            }

            if (OutputLambdaExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[OutputNode of No.{0}]\n", NodeID);
                OutputLambdaExpressionNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // JOINCLAUSENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugJoinClause1(sb);
            DebugJoinClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // JOINCLAUSENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugJoinClause1(sb);
        }
#endif
    }

    //======================================================================
    // class ORDERBYCLAUSENODE
    //
    /// <summary>
    /// (CS3) Represents a orderby clause of a query expression.
    /// </summary>
    //======================================================================
    internal class ORDERBYCLAUSENODE : BASENODE
    {
        //============================================================
        // class ORDERBYCLAUSENODE.Ordering
        //============================================================
        internal class Ordering
        {
            internal LAMBDAEXPRNODE LambdaExpressionNode = null;
            internal bool ByDesending = false;

            internal Ordering Next = null;

#if DEBUG
            //--------------------------------------------------------
            // ORDERBYCLAUSENODE.Ordering.Debug
            //--------------------------------------------------------
            internal void Debug(StringBuilder sb)
            {
                sb.AppendFormat("No.{0}:{1}",
                    LambdaExpressionNode.NodeID,
                    ByDesending ? "descending" : "ascending");
            }
#endif
        }

        //------------------------------------------------------------
        // ORDERBYCLAUSENODE Fields and Properties
        //------------------------------------------------------------
        internal Ordering FirstOrdering = null;
        internal Ordering LastOrdering = null;

        //------------------------------------------------------------
        // ORDERBYCLAUSENODE.Add
        //
        /// <summary></summary>
        /// <param name-"expr"></param>
        /// <param name-"dir"></param>
        //------------------------------------------------------------
        internal void Add(LAMBDAEXPRNODE expr, bool descend)
        {
            Ordering newOrd = new Ordering();
            newOrd.LambdaExpressionNode = expr;
            newOrd.ByDesending = descend;

            if (this.FirstOrdering == null)
            {
                this.FirstOrdering = newOrd;
                this.LastOrdering = newOrd;
            }
            else
            {
                this.LastOrdering.Next = newOrd;
                this.LastOrdering = newOrd;
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // ORDERBYCLAUSENODE.DebugOrderByClause1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugOrderByClause1(StringBuilder sb)
        {
            Ordering ord = this.FirstOrdering;
            if (ord != null)
            {
                sb.AppendFormat("{0}: Ordering :", NodeID);
                int count = 0;

                for (; ord != null; ord = ord.Next, ++count)
                {
                    if (count > 0)
                    {
                        sb.Append(", ");
                    }
                    ord.Debug(sb);
                }
                sb.Append("\n");
            }
            else
            {
                sb.AppendFormat("{0}: Ordering :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // ORDERBYCLAUSENODE.DebugOrderByClause2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugOrderByClause2(StringBuilder sb)
        {
            for (Ordering ord = this.FirstOrdering; ord != null; ord = ord.Next)
            {
                sb.Append("\n");
                sb.AppendFormat("[Ordering of No.{0}]\n", NodeID);
                ord.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ORDERBYCLAUSENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugOrderByClause1(sb);
            DebugOrderByClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // ORDERBYCLAUSENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugOrderByClause1(sb);
        }
#endif
    }

    //======================================================================
    // class SELECTCLAUSENODE
    //
    /// <summary>
    /// (CS3) Represents a select clause of a query expression.
    /// </summary>
    //======================================================================
    internal class SELECTCLAUSENODE : BASENODE
    {
        internal LAMBDAEXPRNODE LambdaExpressionNode = null;

#if DEBUG
        //------------------------------------------------------------
        // SELECTCLAUSENODE.DebugSelectClause1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugSelectClause1(StringBuilder sb)
        {
            if (LambdaExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: LambdaExpressionNode : No.{1}\n",
                    NodeID,
                    LambdaExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: LambdaExpressionNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // SELECTCLAUSENODE.DebugSelectClause2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugSelectClause2(StringBuilder sb)
        {
            if (LambdaExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[LambdaExpressionNode of No.{0}]\n", NodeID);
                LambdaExpressionNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // SELECTCLAUSENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugSelectClause1(sb);
            DebugSelectClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // SELECTCLAUSENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugSelectClause1(sb);
        }
#endif
    }

    //======================================================================
    // class GROUPCLAUSENODE
    //
    /// <summary>
    /// (CS3) Represents a group clause of a query expression.
    /// </summary>
    //======================================================================
    internal class GROUPCLAUSENODE : BASENODE
    {
        internal LAMBDAEXPRNODE ElementExpressionNode = null;

        internal LAMBDAEXPRNODE ByExpressionNode = null;

#if DEBUG
        //------------------------------------------------------------
        // GROUPCLAUSENODE.DebugGroupClause1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugGroupClause1(StringBuilder sb)
        {
            if (ElementExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: ElementExpressionNode : No.{1}\n",
                    NodeID,
                    ElementExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ElementExpressionNode :\n", NodeID);
            }

            if (ByExpressionNode != null)
            {
                sb.AppendFormat(
                    "{0}: ByExpressionNode      : No.{1}\n",
                    NodeID,
                    ByExpressionNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: ByExpressionNode      :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // GROUPCLAUSENODE.DebugGroupClause2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugGroupClause2(StringBuilder sb)
        {
            if (ElementExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ElementExpressionNode of No.{0}]\n", NodeID);
                ElementExpressionNode.Debug(sb);
            }

            if (ByExpressionNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[ByExpressionNode of No.{0}]\n", NodeID);
                ByExpressionNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // GROUPCLAUSENODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugGroupClause1(sb);
            DebugGroupClause2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // GROUPCLAUSENODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugGroupClause1(sb);
        }
#endif
    }

    //======================================================================
    // class QUERYCONTINUATIONNODE
    //
    /// <summary>
    /// (CS3) Represents a let clause of a query expression.
    /// </summary>
    //======================================================================
    internal class QUERYCONTINUATIONNODE : BASENODE
    {
        internal NAMENODE NameNode = null;

        internal BASENODE QueryBodyNode = null;

#if DEBUG
        //------------------------------------------------------------
        // QUERYCONTINUATIONNODE.DebugQueryContinuation1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugQueryContinuation1(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.AppendFormat("{0}: NameNode      : {1}\n", NodeID, NameNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: NameNode      :\n", NodeID);
            }

            if (QueryBodyNode != null)
            {
                sb.AppendFormat("{0}: QueryBodyNode : No.{1}\n", NodeID, QueryBodyNode.NodeID);
            }
            else
            {
                sb.AppendFormat("{0}: QueryBodyNode :\n", NodeID);
            }
        }

        //------------------------------------------------------------
        // QUERYCONTINUATIONNODE.DebugQueryContinuation2
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal void DebugQueryContinuation2(StringBuilder sb)
        {
            if (NameNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[NameNode of No.{0}]\n", NodeID);
                NameNode.Debug(sb);
            }

            if (QueryBodyNode != null)
            {
                sb.Append("\n");
                sb.AppendFormat("[QueryBodyNode of No.{0}]\n", NodeID);
                QueryBodyNode.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // QUERYCONTINUATIONNODE.Debug
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugQueryContinuation1(sb);
            DebugQueryContinuation2(sb);
            DebugBase2(sb);
        }

        //------------------------------------------------------------
        // QUERYCONTINUATIONNODE.Debug1
        //
        /// <summary></summary>
        /// <param name="sb"></param>
        //------------------------------------------------------------
        internal override void Debug1(StringBuilder sb)
        {
            DebugBase1(sb);
            DebugQueryContinuation1(sb);
        }
#endif
    }

    //======================================================================
    //
    //======================================================================
    //	class CListIterator
    //	{
    //	private:
    //	    BASENODE    *m_pCurNode;
    //	
    //	internal:
    //	    CListIterator() : m_pCurNode(NULL) {}
    //	    CListIterator(BASENODE* pStartNode) 
    //	    {
    //	        this->Start(pStartNode);
    //	    }
    //	
    //	    void        Start (BASENODE *pNode) { m_pCurNode = pNode; }
    //	    BASENODE    *Next ()
    //	    {
    //	        if (m_pCurNode == NULL)
    //	            return NULL;
    //	        if (m_pCurNode->Kind == NK_LIST)
    //	        {
    //	            BASENODE    *pRet = m_pCurNode->asLIST()->p1;
    //	            m_pCurNode = m_pCurNode->asLIST()->p2;
    //	            return pRet;
    //	        }
    //	        else
    //	        {
    //	            BASENODE    *pRet = m_pCurNode;
    //	            m_pCurNode = NULL;
    //	            return pRet;
    //	        }
    //	    }
    //	
    //	    static BASENODE *LastNode(BASENODE *pNode)
    //	    {
    //	        CListIterator it;
    //	        it.Start(pNode);
    //	
    //	        BASENODE *pPrev = pNode;
    //	        for (BASENODE *pCur = it.Next(); pCur != NULL; pCur = it.Next())
    //	        {
    //	            pPrev = pCur;
    //	        }
    //	
    //	        return pPrev;
    //	    }
    //	
    //	    static long GetCount(BASENODE *pNode)
    //	    {
    //	        CListIterator it;
    //	        it.Start(pNode);
    //	
    //	        long iCount = 0;
    //	        for (BASENODE *pCur = it.Next(); pCur != NULL; pCur = it.Next())
    //	        {
    //	            iCount++;
    //	        }
    //	
    //	        return iCount;
    //	    }
    //	
    //	    //Get nth node in a list
    //	    static HRESULT GetNode(BASENODE* pStartNode, long index, BASENODE** pNode)
    //	    {
    //	        CListIterator it(pStartNode);
    //	        BASENODE *pParm = NULL;
    //	
    //	        // Run to the 'index'th node
    //	        // Expecting a zero based index here
    //	        for(int i = 0; i <= index; ++i)
    //	        {
    //	            pParm = it.Next();
    //	        }
    //	
    //	        // Search did not succeed: either invalid arguments or element not found
    //	        if(pParm == NULL) {
    //	           *pNode = NULL;
    //	           return E_INVALIDARG;
    //	        }
    //		  
    //	        // Return the node if succeded	 
    //	        *pNode = pParm;
    //		    return S_OK;
    //	    }
    //	
    //	    static HRESULT GetIndex(BASENODE* pStartNode, BASENODE* pNode, long* iIndex)
    //	    {
    //	        CListIterator it(pStartNode);
    //	        long          iCount = 0;// returning zero based index
    //	        
    //	        // Compare each of the fields to the given node
    //	        BASENODE *p;
    //	        for (p = it.Next(); p != NULL && p != pNode; p = it.Next())
    //	            iCount++;
    //	
    //	        // Element not found or pNode was NULL
    //	        if (p == NULL) {
    //	            *iIndex = -1;
    //	            return E_INVALIDARG;
    //	        }
    //	
    //	        *iIndex = iCount;
    //	
    //	        return S_OK;
    //	    }
    //	};

    //======================================================================
    // class CChainIterator
    //======================================================================
    class CChainIterator
    {
        //------------------------------------------------------------
        // CChainIterator   Fields
        //------------------------------------------------------------
        private BASENODE chainStartNode = null;
        private BASENODE currentNode = null;

        private bool isStatementChain = false;
        private bool started = false;

        //------------------------------------------------------------
        // CChainIterator   Constructor
        //------------------------------------------------------------
        internal CChainIterator(BASENODE start)
        {
            chainStartNode = start;
            this.Reset();
        }

        //------------------------------------------------------------
        // CChainIterator.Current
        //------------------------------------------------------------
        internal BASENODE Current()
        {
            //ASSERT(currentNode != NULL);
            //ASSERT(started);
            if (currentNode == null || !started) return null;
            return currentNode;
        }

        //------------------------------------------------------------
        // CChainIterator.MoveNext
        //------------------------------------------------------------
        internal bool MoveNext()
        {
            if (!started)
            {
                started = true;
                currentNode = chainStartNode;
            }
            else if (isStatementChain)
            {
                currentNode = ((STATEMENTNODE)currentNode).NextNode;
            }
            else
            {
                currentNode = ((MEMBERNODE)currentNode).NextNode;
            }

            return currentNode != null;
        }

        //------------------------------------------------------------
        // CChainIterator.Reset
        //------------------------------------------------------------
        internal void Reset()
        {
            //ASSERT(chainStartNode == NULL ||
            //       chainStartNode.IsStatement() ||
            //       chainStartNode.InGroup(NG_MEMBER));
            if (chainStartNode == null ||
                (!chainStartNode.IsStatement &&
                !chainStartNode.InGroup(NODEGROUP.MEMBER)))
                throw new LogicError("CChainIterator");

            currentNode = null;
            started = false;
            isStatementChain = (chainStartNode != null) && chainStartNode.IsStatement;
        }
    }

    //======================================================================
    // class NameNodeResult
    //======================================================================
    //	struct NameNodeResult
    //	{
    //	    NAMENODE *  pNameNode;
    //	    bool        fIsAlias;   // If pNameNode is an alias (lhs of "::")
    //	
    //	    NameNodeResult()
    //	    { 
    //	        Reset(); 
    //	    }
    //	    void Reset() 
    //	    { 
    //	        pNameNode = NULL; 
    //	        fIsAlias = false; 
    //	    }
    //	    void SetNode(NAMENODE *pNode)
    //	    {
    //	        pNameNode = pNode;
    //	        fIsAlias = pNode != NULL && pNode->Kind == NK_ALIASNAME;
    //	    }
    //	};
    //	
    //	class CDottedNameIterator
    //	{
    //	private:
    //	    BASENODE    *   m_pRootNode;        // Root of dotted name tree
    //	    NameNodeResult  m_Result;           // Always the next name node to return
    //	    long            m_iCount;
    //	
    //	private:
    //	    BASENODE *left(BASENODE *pNode)
    //	    {
    //	        ASSERT(pNode != NULL);
    //	        ASSERT(pNode->Kind == NK_DOT);
    //	
    //	        switch(pNode->Kind)
    //	        {
    //	        case NK_DOT:
    //	            return pNode->asDOT()->p1;
    //	        default:
    //	            VSFAIL("Wrong Kind of node");
    //	            return NULL;
    //	        }
    //	    }
    //	    BASENODE *right(BASENODE *pNode)
    //	    {
    //	        ASSERT(pNode != NULL);
    //	        ASSERT(pNode->Kind == NK_DOT);
    //	
    //	        switch(pNode->Kind)
    //	        {
    //	        case NK_DOT:
    //	            return pNode->asDOT()->p2;
    //	        default:
    //	            VSFAIL("Wrong kind of node");
    //	            return NULL;
    //	        }
    //	    }
    //	internal:
    //	    CDottedNameIterator() {
    //	        this->Start(NULL);
    //	    }
    //	
    //	    CDottedNameIterator(BASENODE* pNode) {
    //	        this->Start(pNode);
    //	    }
    //	
    //	    void Start (BASENODE *pNode)
    //	    {
    //	        m_iCount = 0;
    //	        m_pRootNode = pNode;
    //	
    //	        if (pNode != NULL)
    //	        {
    //	            m_iCount = 1;
    //	            while (pNode->Kind == NK_DOT)
    //	            {
    //	                m_iCount++;
    //	                pNode = left(pNode);
    //	            }
    //	
    //	            m_Result.SetNode(pNode->AsANYNAME);
    //	        }
    //	    }
    //	
    //	    NameNodeResult Next ()
    //	    {
    //	        NameNodeResult res;
    //	
    //	        if (m_Result.pNameNode == NULL)
    //	            return res;
    //	
    //	        res = m_Result;
    //	        if (res.pNameNode == m_pRootNode)
    //	        {
    //	            // This is the single-name given case
    //	            m_Result.Reset();
    //	            return res;
    //	        }
    //	
    //	        BASENODE *Parent = m_Result.pNameNode->Parent;
    //	        if (Parent->Kind == NK_DOT)
    //	        {
    //	            if (left(Parent) == m_Result.pNameNode)
    //	                m_Result.SetNode(right(Parent)->AsANYNAME);               // Switch from left-side to right-side
    //	            else if (Parent == m_pRootNode)
    //	                m_Result.SetNode(NULL);                                      // Last name in chain
    //	            else
    //	                m_Result.SetNode(right(Parent->Parent)->AsANYNAME);      // Next name is right side of grandparent
    //	        }
    //	
    //	        return res;
    //	    }
    //	
    //	    long GetCount() { return m_iCount; }
    //	
    //	
    //	};

    //======================================================================
    //  class NodeUtil
    //
    /// <summary>
    /// <para>(CSharp\Inc\Node.cs)</para>
    /// </summary>
    //======================================================================
    static internal class NodeUtil
    {
        //------------------------------------------------------------
        //  NodeUtil.CountAnyBinOpListNode
        //
        /// <summary>
        /// <para>Rename CountAnyListNode in sscli.</para>
        /// <para>Count the data nodes in a node list by BINOP node.
        /// The node kind must be in NG_GROUP.
        /// (CSharp\Inc\Node.cs)</para>
        /// </summary>
        /// <param name="listNode"></param>
        /// <param name="listKind">Must be in NG_BINOP.</param>
        /// <returns>Count of nodes of specified kind.</returns>
        //------------------------------------------------------------
        static internal int CountAnyBinOpListNode(BASENODE listNode, NODEKIND listKind)
        {
            int count = 0;

            while (listNode != null && listNode.Kind == listKind)
            {
                BINOPNODE bop = listNode.AsANYBINOP;
                listNode = (bop != null ? bop.Operand2 : null);
                ++count;
            }

            if (listNode != null)
            {
                ++count;
            }
            return count;
        }

        //------------------------------------------------------------
        //  NodeUtil.CountBinOpListNode
        //
        /// <summary>
        /// Count the successive nodes with NODEKIND.LIST in a node list.
        /// (CSharp\Inc\Node.cs)
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns>Count of nodes with NODEKIND.LIST.</returns>
        //------------------------------------------------------------
        static internal int CountBinOpListNode(BASENODE pNode)
        {
            return CountAnyBinOpListNode(pNode, NODEKIND.LIST);
        }

        //------------------------------------------------------------
        //  NodeUtil.CountLinkedListNode
        //
        /// <summary>
        /// Count the nodes in a list by NextNode field.
        /// </summary>
        /// <param name="node">starting node of a list.</param>
        /// <returns>Count of nodes</returns>
        //------------------------------------------------------------
        static internal int CountLinkedListNode(BASENODE node)
        {
            int count = 0;
            while (node != null)
            {
                ++count;
                node = node.NextNode;
            }
            return count;
        }

        //	inline int CountAnyListNode(BASENODE *pNode, NODEKIND listKind)
        //	{
        //	    int rval = 0;
        //	    while (pNode && pNode->Kind == listKind)
        //	    {
        //	        pNode = pNode->AsANYBINOP->p2;
        //	        rval += 1;
        //	    }
        //	    
        //	    if (pNode!= NULL)
        //	        rval += 1;
        //	        
        //	    return rval;
        //	}
        //	
        //	inline int CountListNode(BASENODE *pNode)
        //	{
        //	    return CountAnyListNode(pNode, NK_LIST);
        //	}

#if DEBUG
        internal static CDebugNodeInfo DebugNodeInfo = new CDebugNodeInfo();
#endif
    }

    //======================================================================
    //  NODE_INFO 関係の処理は System.Type などで行うことにする。
    //======================================================================
    //	/////////////////////////////////////////////////////////////////////////////////
    //	// Node Iterator Logic
    //	
    //	enum NODE_FIELD_KIND
    //	{
    //	    NFK_NONE,
    //	    NFK_NODE,               // a child node
    //	    NFK_OPTIONAL_NODE,      // an optional node
    //	    NFK_FIRST_NODE,         // a first child node
    //	    NFK_PARENT_NODE,        // a node which is actually a parent node
    //	    NFK_NEXT_NODE,          // a node which is a link in a list
    //	    NFK_NAME,               // a NAME*
    //	    NFK_INTERIOR,           // an interiorNode
    //	    NFK_INDEX,              // a node index
    //	    NFK_OTHER,              // anything else
    //	};
    //	
    //	struct NODE_FIELD_INFO
    //	{
    //	    int offset;
    //	    NODE_FIELD_KIND Kind;
    //	};
    //	
    //	#define MAX_NODE_INFO_FIELDS 12
    //	
    //	struct NODE_INFO
    //	{
    //	    const NODE_INFO * baseInfo;
    //	    NODE_FIELD_INFO fields[MAX_NODE_INFO_FIELDS];
    //	};

    //----------------------------------------------------------------------
    //  NodeKindToInfo 関数は BASENODE の static メソッドとする。
    //  関数本体は Shared\Node.cpp にある。
    //----------------------------------------------------------------------
    //	const NODE_INFO * NodeKindToInfo(NODEKIND kind);

    //	// abstract base class for iterating over the members
    //	// of a BASENODE. Derived classes must specify
    //	// FIELD_TYPE - the type of fields tehy want to iterate over
    //	// and override FieldKindMatch() to filter which kinds
    //	// of fields they want to return
    //	template <class FIELD_TYPE>
    //	class NodeIterator
    //	{
    //	internal:
    //	    // advances the iterator and returns true
    //	    // if the Current can be called
    //	    bool MoveNext();
    //	    // the current element of the iterator
    //	    // can only be called after MoveNext() has been called
    //	    // and returned true
    //	    FIELD_TYPE &Current();
    //	    
    //	    // Returns wether its ok to call MoveNext() again
    //	    // Note that when initializing the iterator
    //	    // it is ok to call MoveNext() but not ok to call Current()
    //	    bool Done();
    //	
    //	protected:
    //	    NodeIterator(BASENODE *parentNode);
    //	    
    //	    // returns true if the field kind matches
    //	    // the type of field this iterator should return
    //	    virtual bool FieldKindMatch(NODE_FIELD_KIND fk) = 0;
    //	
    //	private:
    //	    void NextNode();
    //	    void NextField();
    //	    
    //	    int offset;
    //	    const NODE_FIELD_INFO * currentField;
    //	    const NODE_INFO * currentNode;
    //	    BASENODE *parentNode;
    //	};
    //	
    //	// Note that it is OK for parentNode to be NULL
    //	// which results in an empty iteration
    //	template <class FIELD_TYPE>
    //	NodeIterator<FIELD_TYPE>::NodeIterator(BASENODE *parentNode) :
    //	    currentField(0),
    //	    currentNode(0),
    //	    parentNode(parentNode)
    //	{
    //	    if (parentNode)
    //	        currentNode = NodeKindToInfo(parentNode->Kind);
    //	}
    //	
    //	// Advances the iterator, and returns true if there is another
    //	// element available. Once false is returned it is illegal to
    //	// call MoveNext() or Current() again
    //	template <class FIELD_TYPE>
    //	bool NodeIterator<FIELD_TYPE>::MoveNext()
    //	{
    //	    ASSERT(currentNode && (!currentField || currentField >= currentNode->fields));
    //	    
    //	    do
    //	    {
    //	        NextField();
    //	    } while (!Done() && !FieldKindMatch(currentField->Kind));
    //	    
    //	    return !Done();
    //	}
    //	
    //	// Returns wether its ok to call MoveNext() again
    //	// Note that when initializing the iterator
    //	// it is ok to call MoveNext() but not ok to call Current()
    //	template <class FIELD_TYPE>
    //	bool NodeIterator<FIELD_TYPE>::Done()
    //	{
    //	    return !currentNode;
    //	}
    //	
    //	// Advances to the NODE_INFO for the base type
    //	// Sets the state to Done() when no bases are left
    //	template <class FIELD_TYPE>
    //	void NodeIterator<FIELD_TYPE>::NextNode()
    //	{
    //	    ASSERT(!Done() && currentField >= currentNode->fields);
    //	    ASSERT(currentField->Kind == NFK_NONE && currentField->offset == 0);
    //	
    //	    currentNode = currentNode->baseInfo;
    //	    if (currentNode)
    //	        currentField = currentNode->fields;
    //	    else
    //	        currentField = NULL;
    //	}
    //	
    //	// Advances to the next NODE_FIELD_KIND
    //	template <class FIELD_TYPE>
    //	void NodeIterator<FIELD_TYPE>::NextField()
    //	{
    //	    ASSERT(!Done());
    //	
    //	    if (!currentField)
    //	    {
    //	        // initialize the iterator
    //	        currentField = currentNode->fields;
    //	    }
    //	    else if (currentField->Kind != NFK_NONE)
    //	    {
    //	        ASSERT(currentField->offset != 0);
    //	        currentField ++;
    //	    }
    //	    else
    //	    {
    //	        ASSERT(currentField->offset == 0);
    //	        NextNode();
    //	    }
    //	}
    //	
    //	// Returns the current element in the iteration
    //	// Can only call after MoveNext() has been called and returns true
    //	template <class FIELD_TYPE>
    //	FIELD_TYPE &NodeIterator<FIELD_TYPE>::Current()
    //	{
    //	    ASSERT(!Done() && currentField);
    //	    ASSERT(currentField->offset != 0);
    //	    
    //	    return *(FIELD_TYPE*) (((BYTE*) parentNode) + currentField->offset);
    //	}
    //	
    //	// Iterates over the child nodes of a given BASENODE. Note that this doesn't
    //	// process FIRST_NODEs or NEXT_NODEs.
    //	class NonOptionalChildNodeIterator : internal NodeIterator<BASENODE*>
    //	{
    //	internal:
    //	    NonOptionalChildNodeIterator(BASENODE *baseNode) :
    //	        NodeIterator<BASENODE*> (baseNode)
    //	    {}
    //	
    //	protected:
    //	    bool FieldKindMatch(NODE_FIELD_KIND fk)
    //	    {
    //	        return fk == NFK_NODE;
    //	    }
    //	};
    //	
    //	// Iterates over all optional child nodes of a given BASENODE. Note that this doesn't
    //	// process FIRST_NODEs or NEXT_NODEs.
    //	class AllChildNodeIterator : internal NodeIterator<BASENODE*>
    //	{
    //	internal:
    //	    AllChildNodeIterator(BASENODE *baseNode) :
    //	        NodeIterator<BASENODE*> (baseNode)
    //	    {}
    //	
    //	protected:
    //	    bool FieldKindMatch(NODE_FIELD_KIND fk)
    //	    {
    //	        return fk == NFK_NODE || fk == NFK_OPTIONAL_NODE;
    //	    }
    //	};
    //	
    //	// Iterates over the node indexes of a given BASENODE
    //	class IndexNodeIterator : internal NodeIterator<long>
    //	{
    //	internal:
    //	    IndexNodeIterator(BASENODE *baseNode) :
    //	        NodeIterator<long> (baseNode)
    //	    {}
    //	    
    //	protected:
    //	    bool FieldKindMatch(NODE_FIELD_KIND fk)
    //	    {
    //	        return fk == NFK_INDEX;
    //	    }
    //	};

#if DEBUG

    //======================================================================
    // CDebugNodeInfo
    //
    /// <summary>
    /// Hold methods which create strings of infomation of node to debug them.
    /// </summary>
    //======================================================================
    internal class CDebugNodeInfo
    {
        Dictionary<NODEFLAGS, string> dicNodeFlagMod = new Dictionary<NODEFLAGS, string>();

        //------------------------------------------------------------
        // CDebugNodeInfo Constructor
        //------------------------------------------------------------
        internal CDebugNodeInfo()
        {
            dicNodeFlagMod.Add(NODEFLAGS.MOD_ABSTRACT, "abstract");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_NEW, "new");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_OVERRIDE, "override");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_PRIVATE, "private");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_PROTECTED, "protected");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_INTERNAL, "internal");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_PUBLIC, "public");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_SEALED, "sealed");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_STATIC, "static");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_VIRTUAL, "virtual");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_EXTERN, "extern");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_READONLY, "readonly");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_VOLATILE, "volatile");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_UNSAFE, "unsafe");
            dicNodeFlagMod.Add(NODEFLAGS.MOD_PARTIAL, "partial");
        }

        //------------------------------------------------------------
        // CDebugNodeInfo.NodeFlagsModToString
        //------------------------------------------------------------
        internal string NodeFlagsModToString(NODEFLAGS flags)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<NODEFLAGS, string> kv in this.dicNodeFlagMod)
            {
                if ((flags & kv.Key) != 0)
                {
                    if (sb.Length > 0) sb.Append(" | ");
                    sb.Append(kv.Value);
                }
            }
            return sb.ToString();
        }
    }
#endif
}
