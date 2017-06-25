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
// File: exprlist.h
//
// Defines the nodes of the bound expression tree
// ===========================================================================

//============================================================================
//  SCComp_ExprList.cs
//
//  2015/04/15
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq.Expressions;

namespace Uncs
{
    //BEGIN_NAMESPACE_CSHARP
    //class IVisualizerData;
    //END_NAMESPACE_CSHARP

    //======================================================================
    // EXPRKIND  (EK_)
    //
    /// <summary>
    /// (CSharp\SCComp\ExprList.cs)
    /// </summary>
    //======================================================================
    internal enum EXPRKIND : int
    {
        //    #define EXPRDEF(kind) EK_ ## kind,
        //    #define EXPRKINDDEF(kind) EK_ ## kind,
        //    #define EXPRKIND_EXTRA(expr) expr,
        //
        //    #include "exprkind.h"

        BLOCK,
        STMTAS,
        RETURN,
        DECL,
        LABEL,
        GOTO,
        GOTOIF,
        SWITCH,
        SWITCHLABEL,
        TRY,
        HANDLER,
        THROW,
        NOOP,
        DEBUGNOOP,
        DELIM,
        // derived from EXPRSTMT up here.

        BINOP,
        CALL,
        EVENT,
        FIELD,
        LOCAL,
        CONSTANT,
        CLASS,
        NSPACE,
        ERROR,
        FUNCPTR,
        PROP,
        MULTI,
        MULTIGET,
        STTMP,
        LDTMP,
        FREETMP,
        WRAP,
        CONCAT,
        ARRINIT,
        CAST,
        TYPEOF,
        SIZEOF,
        ZEROINIT,
        USERLOGOP,
        MEMGRP,
        ANONMETH,
        DBLQMARK,

        COUNT,

        LIST,
        ASSG,

        MAKERA,
        VALUERA,
        TYPERA,

        ARGS,

        EQUALS,
        FIRSTOP = EQUALS,
        COMPARE,

        TRUE,
        FALSE,

        INC,
        DEC,

        LOGNOT,

        EQ,
        RELATIONAL_MIN = EQ,
        NE,
        LT,
        LE,
        GT,
        GE,
        RELATIONAL_MAX = GE,

        ADD,
        ARITH_MIN = ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        NEG,
        UPLUS,
        ARITH_MAX = UPLUS,

        BITAND,
        BIT_MIN = BITAND,
        BITOR,
        BITXOR,
        BITNOT,
        BIT_MAX = BITNOT,

        LSHIFT,
        RSHIFT,
        ARRLEN,

        LOGAND,
        LOGOR,

        IS,
        AS,
        ARRINDEX,
        NEWARRAY,
        QMARK,
        SEQUENCE,
        SEQREV,
        SAVE,
        SWAP,

        ARGLIST,

        INDIR,
        ADDR,
        LOCALLOC,

        // CS3
        LAMBDAEXPR,
        SYSTEMTYPE,
        FIELDINFO,
        METHODINFO,
        CONSTRUCTORINFO,

        // CS4
        RUNTIMEBINDEDMEMBER,
        RUNTIMEBINDEDUNAOP,
        RUNTIMEBINDEDBINOP,
        RUNTIMEBINDEDINVOCATION,

        // EK_MULTIOFFSET,
        // This has to be last!!!
        // To deal /w multiops we add this to the op to obtain the ek in the op table
        MULTIOFFSET,

        // Statements are all before expressions and the first expression is EK_BINOP
        //    EK_ExprMin = EK_BINOP,
        //    EK_StmtLim = EK_ExprMin,
        ExprMin = BINOP,
        StmtLim = ExprMin,
    }

    //======================================================================
    // enum EXPRFLAG (EXF_)
    //
    /// <summary>
    /// (CSharp\SCComp\ExprList.cs)
    /// </summary>
    //======================================================================
    internal enum EXPRFLAG : int
    {
        // These are specific to various node types.
        // Order these by value.
        // If you need a new flag,
        // search for the first value that isn't currently valid on your expr kind.

        //------------------------------------------------------------
        // 0x1
        //------------------------------------------------------------

        /// <summary>
        /// On ** Many Non Statement Exprs! ** This gets its own BIT! (0x1)
        /// </summary>
        BINOP = 0x1,

        //------------------------------------------------------------
        // 0x2
        //------------------------------------------------------------
        /// <summary>
        /// Only on EXPRMEMGRP, indicates a constructor. (0x2)
        /// </summary>
        CTOR = 0x2,

        /// <summary>
        /// Only on EXPRBLOCK (0x2)
        /// </summary>
        NEEDSRET = 0x2,

        /// <summary>
        /// Only on EXPRGOTO, EXPRRETURN, means use leave instead of br instruction (0x2)
        /// </summary>
        ASLEAVE = 0x2,

        /// <summary>
        /// Only on EXPRTRY, used for fabricated try/fault (no user code) (0x2)
        /// </summary>
        ISFAULT = 0x2,

        /// <summary>
        /// Only on EXPRSWITCH (0x2)
        /// </summary>
        HASHTABLESWITCH = 0x2,

        /// <summary>
        /// Only on EXPRCAST, indicates a boxing operation (value type -&gt; object) (0x2)
        /// </summary>
        BOX = 0x2,

        /// <summary>
        /// Only on EXPRARRINIT, indicates that we should init using a memory block (0x2)
        /// </summary>
        ARRAYCONST = 0x2,

        /// <summary>
        /// Only on EXPRFIELD, indicates that the reference is for set purposes (0x2)
        /// </summary>
        MEMBERSET = 0x2,

        /// <summary>
        /// Only on EXPRTYPEOF. Indicates that the type is an open type. (0x2)
        /// </summary>
        OPENTYPE = 0x2,

        /// <summary>
        /// Only on EXPRLABEL. Indicates the label was targeted by a goto. (0x2)
        /// </summary>
        LABELREFERENCED = 0x2,

        //------------------------------------------------------------
        // 0x4
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRMEMGRP, indicates an indexer. (0x4)
        /// </summary>
        INDEXER = 0x4,

        /// <summary>
        /// Only on EXPRGOTO, means goto case or goto default (0x4)
        /// </summary>
        GOTOCASE = 0x4,

        /// <summary>
        /// Only on EXPRSWITCH (0x4)
        /// </summary>
        HASDEFAULT = 0x4,

        /// <summary>
        /// Only on EXPRTRY, means that the try-finally should be converted to normal code (0x4)
        /// </summary>
        REMOVEFINALLY = 0x4,

        /// <summary>
        /// Only on EXPRCAST, indicates a unboxing operation (object -&gt; value type) (0x4)
        /// </summary>
        UNBOX = 0x4,

        /// <summary>
        /// Only on EXPRARRINIT, indicated that all elems are constant (must also have ARRAYCONST set) (0x4)
        /// </summary>
        ARRAYALLCONST = 0x4,

        /// <summary>
        /// Only on EXPRBLOCK, indicates that the block is the preamble of a constructor
        /// - contains field inits and base ctor call (0x4)
        /// </summary>
        CTORPREAMBLE = 0x4,

        //------------------------------------------------------------
        // 0x8
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRMEMGRP, indicates an operator. (0x8)
        /// </summary>
        OPERATOR = 0x8,

        /// <summary>
        /// Only on EXPRMULTI, indicates &lt;x&gt;++ (0x8)
        /// </summary>
        ISPOSTOP = 0x8,

        /// <summary>
        /// Only on EXPRCONCAT, means that we need to realize the list to a constructor call... (0x8)
        /// </summary>
        UNREALIZEDCONCAT = 0x8,

        /// <summary>
        /// Only on EXPRTRY, EXPRGOTO, EXPRRETURN, means that FINALLY block end is unreachable (0x8)
        /// </summary>
        FINALLYBLOCKED = 0x8,

        /// <summary>
        /// Only on EXPRCAST, indicates an reference checked cast is required (0x8)
        /// </summary>
        REFCHECK = 0x8,

        /// <summary>
        /// Only on EXPRWRAP, indicates that this wrap represents an actual local (0x8)
        /// </summary>
        WRAPASTEMP = 0x8,

        //------------------------------------------------------------
        // 0x10
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRCONSTANT, means this was not a folded constant (0x10)
        /// </summary>
        LITERALCONST = 0x10,

        /// <summary>
        /// Only on EXPRGOTO, indicates an unrealizable goto (0x10)
        /// </summary>
        BADGOTO = 0x10,

        /// <summary>
        /// Only on EXPRRETURN, means this is really a yield, and flow continues (0x10)
        /// </summary>
        RETURNISYIELD = 0x10,

        /// <summary>
        /// Only on EXPRTRY (0x10)
        /// </summary>
        ISFINALLY = 0x10,

        /// <summary>
        /// Only on EXPRCALL and EXPRMEMGRP, to indicate new &lt;...&gt;(...) (0x10)
        /// </summary>
        NEWOBJCALL = 0x10,

        /// <summary>
        /// Only on EXPRCAST, indicates a special cast for array indexing (0x10)
        /// </summary>
        INDEXEXPR = 0x10,

        /// <summary>
        /// Only on EXPRWRAP, it means the wrap should be replaced with its expr (during rewriting) (0x10)
        /// </summary>
        REPLACEWRAP = 0x10,

        //------------------------------------------------------------
        // 0x20
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRGOTO, means target unknown (0x20)
        /// </summary>
        UNREALIZEDGOTO = 0x20,

        /// <summary>
        /// Only on EXPRCALL, EXPRPROP, indicates a call through a method or prop on a type variable or value type (0x20)
        /// </summary>
        CONSTRAINED = 0x20,

        /// <summary>
        /// Only on EXPRCAST, GENERICS: indicates a "forcing" boxing operation
        /// (if type parameter boxed then nop, i.e. object -&gt; object, else value type -&gt; object) (0x20)
        /// </summary>
        FORCE_BOX = 0x20,

        //------------------------------------------------------------
        // 0x40
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRGOTO, EXPRRETURN, means leave through a finally, ASLEAVE must also be set (0x40)
        /// </summary>
        ASFINALLYLEAVE = 0x40,

        /// <summary>
        /// Only on EXPRCALL, EXPRFNCPTR, EXPRPROP, EXPREVENT, and EXPRMEMGRP, indicates a "base.XXXX" call (0x40)
        /// </summary>
        BASECALL = 0x40,

        /// <summary>
        /// Only on EXPRCAST, GENERICS: indicates a "forcing" unboxing operation
        /// (if type parameter boxed then castclass, i.e. object -&gt; object, else object -&gt; value type) (0x40)
        /// </summary>
        FORCE_UNBOX = 0x40,

        /// <summary>
        /// Only on EXPRBINOP, with kind == EK_ADDR, indicates that a conv.u should NOT be emitted. (0x40)
        /// </summary>
        ADDRNOCONV = 0x40,

        //------------------------------------------------------------
        // 0x80
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRGOTO, means the goto is known to not pass through a finally which does not terminate (0x80)
        /// </summary>
        GOTONOTBLOCKED = 0x80,

        /// <summary>
        /// Only on EXPRMEMGRP, indicates an implicit invocation of a delegate: d() vs d.Invoke(). (0x80)
        /// </summary>
        DELEGATE = 0x80,

        /// <summary>
        /// Only on EXPRCALL, indicates that some of the params are out or ref (0x80)
        /// </summary>
        HASREFPARAM = 0x80,

        /// <summary>
        /// Only on EXPRCAST, indicates a static cast is required.
        /// We implement with stloc, ldloc to a temp of the correct type. (0x80)
        /// </summary>
        STATIC_CAST = 0x80,

        //------------------------------------------------------------
        // 0x100
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRMEMGRP, indicates a user callable member group. (0x100)
        /// </summary>
        USERCALLABLE = 0x100,

        //------------------------------------------------------------
        // 0x200
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRCALL, indicates that this is a constructor call which assigns to object (0x200)
        /// </summary>
        NEWSTRUCTASSG = 0x200,

        /// <summary>
        /// Only on statement exprs. Indicates that the statement is compiler generated (0x200)
        /// </summary>
        GENERATEDSTMT = 0x200,

        // so we shouldn't report things like "unreachable code" on it. (0x200)

        //------------------------------------------------------------
        // 0x400
        //------------------------------------------------------------

        /// <summary>
        /// Only on EXPRCALL, indicates that this an implicit struct assg call (0x400)
        /// </summary>
        IMPLICITSTRUCTASSG = 0x400,

        /// <summary>
        /// Only on statement exprs. Indicates that we're currently marking (0x400)
        /// </summary>
        MARKING = 0x400,

        // its children for reachability (it's up the stack).

        // *** The following are usable on multiple node types. ***

        //------------------------------------------------------------
        // 0x000800 and above
        //------------------------------------------------------------

        /// <summary>
        /// indicates an unreachable statement (0x800)
        /// </summary>
        UNREACHABLEBEGIN = 0x000800,

        /// <summary>
        /// indicates the end of the statement is unreachable (0x1000)
        /// </summary>
        UNREACHABLEEND = 0x001000,

        /// <summary>
        /// Only set on EXPRDEBUGNOOP, but tested generally.
        /// Indicates foreach node should not be overridden to in token (0x2000)
        /// </summary>
        USEORIGDEBUGINFO = 0x002000,

        /// <summary>
        /// indicates override tree to set debuginfo on last brace (0x4000)
        /// </summary>
        LASTBRACEDEBUGINFO = 0x004000,

        /// <summary>
        /// indicates no debug info for this statement (0x8000)
        /// </summary>
        NODEBUGINFO = 0x008000,

        /// <summary>
        /// indicates a compiler provided this pointer (in the EE, when doing autoexp, this can be anything) (0x10000)
        /// </summary>
        IMPLICITTHIS = 0x010000,

        /// <summary>
        /// indicate this expression can't ever be null (e.g., "this"). (0x20000)
        /// </summary>
        CANTBENULL = 0x020000,

        /// <summary>
        /// indicates that operation should be checked for overflow (0x40000)
        /// </summary>
        CHECKOVERFLOW = 0x040000,

        /// <summary>
        /// On any expr, indicates that the first operand must be placed on the stack before (0x100000)
        /// </summary>
        PUSH_OP_FIRST = 0x100000,

        // anything else - this is needed for multi-ops involving string concat.

        /// <summary>
        /// On any non stmt exprs, indicates assignment node... (0x200000)
        /// </summary>
        ASSGOP = 0x200000,

        /// <summary>
        /// On any exprs. An lvalue - whether it's legal to assign. (0x400000)
        /// </summary>
        LVALUE = 0x400000,

        /// <summary>
        /// THIS IS THE HIGHEST FLAG:
        /// Indicates that the expression came from a LOCVARSYM, MEMBVARSYM, or PROPSYM whose type has the same name
        /// so it's OK to use the type instead of the element if using the element would generate an error. (0x800000)
        /// </summary>
        SAMENAMETYPE = 0x800000,

        MASK_ANY =
            UNREACHABLEBEGIN |
            UNREACHABLEEND |
            USEORIGDEBUGINFO |
            LASTBRACEDEBUGINFO |
            NODEBUGINFO |
            IMPLICITTHIS |
            CANTBENULL |
            CHECKOVERFLOW |
            PUSH_OP_FIRST |
            ASSGOP |
            LVALUE |
            SAMENAMETYPE
        ,

        /// <summary>
        /// Used to mask the cast flags off an EXPRCAST.
        /// </summary>
        CAST_ALL = BOX | UNBOX | REFCHECK | INDEXEXPR | FORCE_BOX | FORCE_UNBOX | STATIC_CAST
        ,

        LASTSSCLIFLAG = SAMENAMETYPE,   // hirano567@hotmail.co.jp

        //------------------------------------------------------------
        // Addition (2016/12/23 hirano567@hotmail.co.jp)
        //------------------------------------------------------------
        RUNTIMEBINDED = LASTSSCLIFLAG << 1, // CS4
    }

    //// Typedefs for some pointer types up front.
    //typedef class EXPR * PEXPR;
    //typedef class EXPRLIST * PEXPRLIST;

    //======================================================================
    // EXPR
    //
    /// <summary>
    /// The base expression node. 
    /// </summary>
    //======================================================================
    internal class EXPR
    {
        //------------------------------------------------------------
        // enum EXPR.CONSTRESKIND
        //------------------------------------------------------------
        internal enum CONSTRESKIND : int
        {
            True,
            False,
            NotConst,
        }

        //------------------------------------------------------------
        // EXPR Fields and Properties
        //------------------------------------------------------------
        /// <summary>
        /// the exprnode kind
        /// </summary>
        internal EXPRKIND Kind;     // EXPRKIND kind: 8;

        internal EXPRFLAG Flags;    // int flags : 24;

        // We have more bits available here!

        internal BASENODE TreeNode = null;  // * tree

        internal TYPESYM TypeSym = null;

        internal readonly int ExprID = CObjectID.GenerateID();

        //------------------------------------------------------------
        // EXPR Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal EXPR()
        {
#if DEBUG
            DebugUtil.DebugExprsAdd(this);
            //if (this.ExprID == 5254)
            if (this.ExprID == 5238)
            {
                ;
            }
#endif
        }

        //------------------------------------------------------------
        // EXPR.UpdateType
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        private void UpdateType() { }

        // We define the casts by hand so that statement completion will be aware of them...

        //------------------------------------------------------------
        // EXPR As* Properties
        //
        // FUNCBREC.NewExprCore method can create a instance whose EXPRKIND is different from the actual kind.
        // Therefore, check Kind and convert by as operator.
        //------------------------------------------------------------
        //internal EXPRBLOCK AsBLOCK
        //{
        //    get { return (this.Kind == EXPRKIND.BLOCK ? this as EXPRBLOCK : null); }
        //}
        //internal EXPRBINOP AsBINOP
        //{
        //    get { return (this.Kind == EXPRKIND.BINOP ? this as EXPRBINOP : null); }
        //}
        //internal EXPRSTMTAS AsSTMTAS
        //{
        //    get { return (this.Kind == EXPRKIND.STMTAS ? this as EXPRSTMTAS : null); }
        //}
        //internal EXPRCALL AsCALL
        //{
        //    get { return (this.Kind == EXPRKIND.CALL ? this as EXPRCALL : null); }
        //}
        //internal EXPRCAST AsCAST
        //{
        //    get { return (this.Kind == EXPRKIND.CAST ? this as EXPRCAST : null); }
        //}
        //internal EXPRDELIM AsDELIM
        //{
        //    get { return (this.Kind == EXPRKIND.DELIM ? this as EXPRDELIM : null); }
        //}
        //internal EXPREVENT AsEVENT
        //{
        //    get { return (this.Kind == EXPRKIND.EVENT ? this as EXPREVENT : null); }
        //}
        //internal EXPRFIELD AsFIELD
        //{
        //    get { return (this.Kind == EXPRKIND.FIELD ? this as EXPRFIELD : null); }
        //}
        //internal EXPRLOCAL AsLOCAL
        //{
        //    get { return (this.Kind == EXPRKIND.LOCAL ? this as EXPRLOCAL : null); }
        //}
        //internal EXPRRETURN AsRETURN
        //{
        //    get { return (this.Kind == EXPRKIND.RETURN ? this as EXPRRETURN : null); }
        //}
        //internal EXPRCONSTANT AsCONSTANT
        //{
        //    get { return (this.Kind == EXPRKIND.CONSTANT ? this as EXPRCONSTANT : null); }
        //}
        //internal EXPRCLASS AsCLASS
        //{
        //    get { return (this.Kind == EXPRKIND.CLASS ? this as EXPRCLASS : null); }
        //}

        //internal EXPRTYPE asTYPE()   // EXPRTYPE の定義を探すこと。

        //internal EXPRNSPACE AsNSPACE
        //{
        //    get { return (this.Kind == EXPRKIND.NSPACE ? this as EXPRNSPACE : null); }
        //}
        //internal EXPRERROR AsERROR
        //{
        //    get { return (this.Kind == EXPRKIND.ERROR ? this as EXPRERROR : null); }
        //}
        //internal EXPRDECL AsDECL
        //{
        //    get { return (this.Kind == EXPRKIND.DECL ? this as EXPRDECL : null); }
        //}
        //internal EXPRLABEL AsLABEL
        //{
        //    get { return (this.Kind == EXPRKIND.LABEL || this.Kind == EXPRKIND.SWITCHLABEL ? this as EXPRLABEL : null); }
        //}
        //internal EXPRGOTO AsGOTO
        //{
        //    get { return (this.Kind == EXPRKIND.GOTO || this.Kind == EXPRKIND.GOTOIF ? this as EXPRGOTO : null); }
        //}
        //internal EXPRGOTOIF AsGOTOIF
        //{
        //    get { return (this.Kind == EXPRKIND.GOTOIF ? this as EXPRGOTOIF : null); }
        //}
        //internal EXPRFUNCPTR AsFUNCPTR
        //{
        //    get { return (this.Kind == EXPRKIND.FUNCPTR ? this as EXPRFUNCPTR : null); }
        //}
        //internal EXPRSWITCH AsSWITCH
        //{
        //    get { return (this.Kind == EXPRKIND.SWITCH ? this as EXPRSWITCH : null); }
        //}
        //internal EXPRSWITCHLABEL AsSWITCHLABEL
        //{
        //    get { return (this.Kind == EXPRKIND.SWITCHLABEL ? this as EXPRSWITCHLABEL : null); }
        //}
        //internal EXPRPROP AsPROP
        //{
        //    get { return (this.Kind == EXPRKIND.PROP ? this as EXPRPROP : null); }
        //}
        //internal EXPRHANDLER AsHANDLER
        //{
        //    get { return (this.Kind == EXPRKIND.HANDLER ? this as EXPRHANDLER : null); }
        //}
        //internal EXPRTRY AsTRY
        //{
        //    get { return (this.Kind == EXPRKIND.TRY ? this as EXPRTRY : null); }
        //}
        //internal EXPRTHROW AsTHROW
        //{
        //    get { return (this.Kind == EXPRKIND.THROW ? this as EXPRTHROW : null); }
        //}
        //internal EXPRMULTI AsMULTI
        //{
        //    get { return (this.Kind == EXPRKIND.MULTI ? this as EXPRMULTI : null); }
        //}
        //internal EXPRMULTIGET AsMULTIGET
        //{
        //    get { return (this.Kind == EXPRKIND.MULTIGET ? this as EXPRMULTIGET : null); }
        //}
        //internal EXPRSTTMP AsSTTMP
        //{
        //    get { return (this.Kind == EXPRKIND.STTMP ? this as EXPRSTTMP : null); }
        //}
        //internal EXPRLDTMP AsLDTMP
        //{
        //    get { return (this.Kind == EXPRKIND.LDTMP ? this as EXPRLDTMP : null); }
        //}
        //internal EXPRFREETMP AsFREETMP
        //{
        //    get { return (this.Kind == EXPRKIND.FREETMP ? this as EXPRFREETMP : null); }
        //}
        //internal EXPRWRAP AsWRAP
        //{
        //    get { return (this.Kind == EXPRKIND.WRAP ? this as EXPRWRAP : null); }
        //}
        //internal EXPRCONCAT AsCONCAT
        //{
        //    get { return (this.Kind == EXPRKIND.CONCAT ? this as EXPRCONCAT : null); }
        //}
        //internal EXPRARRINIT AsARRINIT
        //{
        //    get { return (this.Kind == EXPRKIND.ARRINIT ? this as EXPRARRINIT : null); }
        //}
        //internal EXPRTYPEOF AsTYPEOF
        //{
        //    get { return (this.Kind == EXPRKIND.TYPEOF ? this as EXPRTYPEOF : null); }
        //}
        //internal EXPRSIZEOF AsSIZEOF
        //{
        //    get { return (this.Kind == EXPRKIND.SIZEOF ? this as EXPRSIZEOF : null); }
        //}
        //internal EXPRZEROINIT AsZEROINIT
        //{
        //    get { return (this.Kind == EXPRKIND.ZEROINIT ? this as EXPRZEROINIT : null); }
        //}
        //internal EXPRNOOP AsNOOP
        //{
        //    get { return (this.Kind == EXPRKIND.NOOP ? this as EXPRNOOP : null); }
        //}
        //internal EXPRDEBUGNOOP AsDEBUGNOOP
        //{
        //    get { return (this.Kind == EXPRKIND.DEBUGNOOP ? this as EXPRDEBUGNOOP : null); }
        //}
        //internal EXPRUSERLOGOP AsUSERLOGOP
        //{
        //    get { return (this.Kind == EXPRKIND.USERLOGOP ? this as EXPRUSERLOGOP : null); }
        //}
        //internal EXPRMEMGRP AsMEMGRP
        //{
        //    get { return (this.Kind == EXPRKIND.MEMGRP ? this as EXPRMEMGRP : null); }
        //}
        //internal EXPRANONMETH AsANONMETH
        //{
        //    get { return (this.Kind == EXPRKIND.ANONMETH ? this as EXPRANONMETH : null); }
        //}
        //internal EXPRDBLQMARK AsDBLQMARK
        //{
        //    get { return (this.Kind == EXPRKIND.DBLQMARK ? this as EXPRDBLQMARK : null); }
        //}

        /// <summary>
        /// Return this as is.
        /// </summary>
        internal EXPR AsEXPR
        {
            get { return this; }
        }

        internal EXPRSTMT AsSTMT
        {
            get
            {
                DebugUtil.Assert(this.Kind < EXPRKIND.StmtLim);
                return this as EXPRSTMT;
            }
        }

        /// <summary>
        /// <para>This property checks EXPRKIND by (this.Flags &amp; EXPRFLAG.BINOP) != 0.
        /// This holds in the case of BINOP, WRAP, TYPERA, GE, INDIR.</para>
        /// <para>(Property AsBINOP returns a BINOP instance only when EXPRKIND is BINOP.
        /// Usually call AsBINOP.)</para>
        /// </summary>
        internal EXPRBINOP AsBIN    // asBIN()
        {
            get
            {
                DebugUtil.Assert((this.Flags & EXPRFLAG.BINOP) != 0);
                return this as EXPRBINOP;
            }
        }

        //// Use AsGOTO
        //internal EXPRGOTO AsGT    // asGT()
        //{
        //    get{DebugUtil.Assert(this.Kind == EXPRKIND.GOTO || this.Kind == EXPRKIND.GOTOIF);return this as EXPRGOTO;}
        //}

        internal EXPRLABEL AsANYLABEL   // asANYLABEL()
        {
            get
            {
                DebugUtil.Assert(this.Kind == EXPRKIND.LABEL || this.Kind == EXPRKIND.SWITCHLABEL);
                return this as EXPRLABEL;
            }
        }

        // Allocate from a no-release allocator.
        // #ifdef _MSC_VER
        // #pragma push_macro("new")
        // #undef new
        // #endif // _MSC_VER
        //     void * operator new(size_t sz, NRHEAP * allocator)
        // #ifdef _MSC_VER
        // #pragma pop_macro("new")
        // #endif // _MSC_VER
        // {
        //     return allocator->AllocZero(sz);
        // };

        //------------------------------------------------------------
        // EXPR.Is* Properties
        //------------------------------------------------------------

        /// <summary>
        /// True if not ERROR.
        /// </summary>
        internal bool IsOK
        {
            get { return this.Kind != EXPRKIND.ERROR; }
        }

        internal bool IsFixedAggField
        {
            get
            {
                return this.Kind == EXPRKIND.FIELD && (this as EXPRFIELD).FixedAgg() != null;
            }
        }

        //------------------------------------------------------------
        // EXPR.IsTrue
        //
        /// <summary></summary>
        /// <param name="sense"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IsTrue(bool sense)	// = true);
        {
            bool br = this.Kind == EXPRKIND.CONSTANT && ((this as EXPRCONSTANT).ConstVal.GetInt() ^ (sense ? 1 : 0)) == 0;
            return (br ? 1 : 0);
        }

        //------------------------------------------------------------
        // EXPR.IsFalse
        //
        /// <summary></summary>
        /// <param name="sense"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsFalse(bool sense)	// = true);
        {
            return (this.Kind == EXPRKIND.CONSTANT && ((this as EXPRCONSTANT).ConstVal.GetInt() ^ (sense ? 1 : 0)) != 0);
        }

        //------------------------------------------------------------
        // EXPR.IsTrueResult
        //
        /// <summary>
        /// Checks for EXPRKIND.SEQUENCE as well
        /// </summary>
        //------------------------------------------------------------
        internal bool IsTrueResult(bool sense)	// = true);
        {
            return IsFalseResult(!sense);
        }

        //------------------------------------------------------------
        // EXPR.IsFalseResult
        //
        /// <summary>
        /// Checks for EXPRKIND.SEQUENCE as well
        /// </summary>
        //------------------------------------------------------------
        internal bool IsFalseResult(bool sense)	// = true);
        {
            return ConstantMatchesSense(GetConstantResult(), !sense);
        }

        //------------------------------------------------------------
        // EXPR.IsConstantResult
        //
        /// <summary>
        /// Checks for EXPRKIND.SEQUENCE as well
        /// </summary>
        //------------------------------------------------------------
        internal bool IsConstantResult()
        {
            return (this.GetConstantResult() != CONSTRESKIND.NotConst);
        }

        //------------------------------------------------------------
        // EXPR.GetConstantResult
        //
        /// <summary>
        /// True if this is a valid constant.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CONSTRESKIND GetConstantResult()
        {
            for (EXPR expr = this; expr != null;)
            {
                switch (expr.Kind)
                {
                    case EXPRKIND.CONSTANT:
                        DebugUtil.Assert((expr as EXPRCONSTANT).ConstVal != null);
                        return (expr as EXPRCONSTANT).ConstVal.GetBool() ?
                            CONSTRESKIND.True : CONSTRESKIND.False;

                    case EXPRKIND.SEQUENCE:
                        expr = (expr as EXPRBINOP).Operand2;
                        break;

                    default:
                        return CONSTRESKIND.NotConst;
                }
            }
            throw new LogicError("EXPR.GetConstantResult");
        }

        //------------------------------------------------------------
        // EXPR.ConstantMatchesSense
        //
        /// <summary></summary>
        /// <param name="crk"></param>
        /// <param name="sense"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool ConstantMatchesSense(CONSTRESKIND crk, bool sense)
        {
            if (crk == CONSTRESKIND.NotConst) return false;
            return ((crk == CONSTRESKIND.True) == sense);
        }

        //------------------------------------------------------------
        // EXPR.IsNull
        //
        /// <summary>
        /// True if this is a constant and this has null value.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsNull()
        {
            return (
                this.Kind == EXPRKIND.CONSTANT &&
                this.TypeSym != null && this.TypeSym.FundamentalType() == FUNDTYPE.REF &&
                (this as EXPRCONSTANT).ConstVal.IsNull);
        }

        //------------------------------------------------------------
        // EXPR.IsZero
        //
        /// <summary></summary>
        /// <param name="fDefValue"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsZero(bool fDefValue)
        {
            if (Kind == EXPRKIND.CONSTANT)
            {
                switch (this.TypeSym.FundamentalType())
                {
                    case FUNDTYPE.I1:
                    case FUNDTYPE.U1:
                    case FUNDTYPE.I2:
                    case FUNDTYPE.U2:
                    case FUNDTYPE.I4:
                        return (this as EXPRCONSTANT).ConstVal.GetInt() == 0; ;

                    case FUNDTYPE.U4:
                        return (this as EXPRCONSTANT).ConstVal.GetUInt() == 0;

                    case FUNDTYPE.I8:
                        return (this as EXPRCONSTANT).ConstVal.GetLong() == 0;

                    case FUNDTYPE.U8:
                        return (this as EXPRCONSTANT).ConstVal.GetULong() == 0;

                    case FUNDTYPE.R4:
                    case FUNDTYPE.R8:
                        return (this as EXPRCONSTANT).ConstVal.GetDouble() == 0.0;

                    case FUNDTYPE.STRUCT: // Decimal
                        return (this as EXPRCONSTANT).ConstVal.GetDecimal() == 0;

                    case FUNDTYPE.REF:
                        return fDefValue && this.IsNull();
                    default:
                        break;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // EXPR.GetOffset
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetOffset()
        {
            if (this.Kind == EXPRKIND.LOCAL)
            {
                DebugUtil.Assert(!(this as EXPRLOCAL).LocVarSym.IsConst);
                return (this as EXPRLOCAL).LocVarSym.LocSlotInfo.JbitDefAssg();
            }
            else if (this.Kind == EXPRKIND.FIELD)
            {
                return (this as EXPRFIELD).Offset;
            }
            else
            {
                return 0;
            }
        }

        //------------------------------------------------------------
        // EXPR.HasSideEffects
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasSideEffects(COMPILER compiler)
        {
            EXPR expr;

            if ((this.Flags & (EXPRFLAG.ASSGOP | EXPRFLAG.CHECKOVERFLOW)) != 0)
            {
                return true;
            }

            if ((this.Flags & EXPRFLAG.BINOP) != 0)
            {
                return (
                    (this as EXPRBINOP).Operand1.HasSideEffects(compiler) ||
                    ((this as EXPRBINOP).Operand2 != null && (this as EXPRBINOP).Operand2.HasSideEffects(compiler)));
            }

            switch (this.Kind)
            {
                // Always true
                case EXPRKIND.PROP:
                case EXPRKIND.CONCAT:
                case EXPRKIND.CALL:
                case EXPRKIND.ERROR:
                    return true;

                // Always false
                case EXPRKIND.DELIM:
                case EXPRKIND.NOOP:
                case EXPRKIND.LOCAL:
                case EXPRKIND.CONSTANT:
                case EXPRKIND.FUNCPTR:
                case EXPRKIND.TYPEOF:
                case EXPRKIND.SIZEOF:
                case EXPRKIND.LDTMP:
                case EXPRKIND.ANONMETH:
                case EXPRKIND.MEMGRP:
                case EXPRKIND.LAMBDAEXPR:
                    return false;

                case EXPRKIND.ZEROINIT:
                    expr = (this as EXPRZEROINIT).Operand;
                    return (expr != null &&
                        (expr.Kind != EXPRKIND.LOCAL || (expr as EXPRLOCAL).LocVarSym.LocSlotInfo.TypeSym != null));

                case EXPRKIND.ARRINIT:
                    return (
                        (this as EXPRARRINIT).ArgumentsExpr != null &&
                        (this as EXPRARRINIT).ArgumentsExpr.HasSideEffects(compiler));

                case EXPRKIND.FIELD:
                    expr = (this as EXPRFIELD).ObjectExpr;
                    // a static field has the sideeffect of executing the cctor
                    // Volatile fields always have side-effects
                    if (expr == null || (this as EXPRFIELD).FieldWithType.FieldSym.IsVolatile)
                    {
                        return true;
                    }
                    DebugUtil.Assert(compiler != null);

                    if (expr.TypeSym.FundamentalType() == FUNDTYPE.REF && !compiler.FuncBRec.IsThisPointer(expr))
                    {
                        return true;
                    }
                    return expr.HasSideEffects(compiler);

                case EXPRKIND.WRAP:
                    return (this as EXPRWRAP).Expr.HasSideEffects(compiler);

                case EXPRKIND.CAST:
                    if ((Flags &
                        (EXPRFLAG.BOX |
                        EXPRFLAG.UNBOX |
                        EXPRFLAG.FORCE_UNBOX |
                        EXPRFLAG.REFCHECK |
                        EXPRFLAG.CHECKOVERFLOW)) != 0)
                    {
                        return true;
                    }
                    return (this as EXPRCAST).Operand.HasSideEffects(compiler);

                case EXPRKIND.DBLQMARK:
                    return (
                        (this as EXPRDBLQMARK).TestExpr.HasSideEffects(compiler) ||
                        (this as EXPRDBLQMARK).ConvertExpr.HasSideEffects(compiler) ||
                        (this as EXPRDBLQMARK).ElseExpr.HasSideEffects(compiler));

                default:
                    DebugUtil.Assert(false, "bad expr");
                    return true;
            }
        }

        //------------------------------------------------------------
        // EXPR.GetArgs
        //------------------------------------------------------------
        internal EXPR GetArgs()
        //__forceinline EXPR * EXPR::getArgs()
        {
            throw new NotImplementedException("EXPR.getArgs");

            //RETAILVERIFY(kind == EK_CALL || kind == EK_PROP || kind == EK_FIELD || kind == EK_ARRINDEX);
            //if (kind == EK_FIELD)
            //    return NULL;
            //ASSERT(offsetof(EXPRCALL, args) == offsetof(EXPRPROP, args));
            //ASSERT(offsetof(EXPRCALL, args) == offsetof(EXPRBINOP, p2));
            //return (static_cast<EXPRCALL*>(this))->args;
        }

        //------------------------------------------------------------
        // EXPR.GetArgsPtr
        //------------------------------------------------------------
        internal EXPR GetArgsPtr()
        //__forceinline EXPR ** EXPR::getArgsPtr()
        {
            throw new NotImplementedException("EXPR.getArgsPtr");

            //RETAILVERIFY(kind == EK_CALL || kind == EK_PROP);
            //ASSERT(offsetof(EXPRCALL, args) == offsetof(EXPRPROP, args));
            //return &((static_cast<EXPRCALL*>(this))->args);
        }

        //------------------------------------------------------------
        // EXPR.getObject
        //------------------------------------------------------------
        internal EXPR GetObject()
        //__forceinline EXPR * EXPR::getObject()
        {
            throw new NotImplementedException("EXPR.getObject");

            //RETAILVERIFY(kind == EK_CALL || kind == EK_PROP || kind == EK_FIELD);
            //ASSERT(offsetof(EXPRCALL, object) == offsetof(EXPRPROP, object));
            //ASSERT(offsetof(EXPRCALL, object) == offsetof(EXPRFIELD, object));
            //return (static_cast<EXPRCALL*>(this))->object;
        }

        //------------------------------------------------------------
        // EXPR.GetSymWithType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SymWithType GetSymWithType()
        {
            switch (Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad expr kind in EXPR::GetSymWithType"); // Fall through.
                    return null;

                case EXPRKIND.CALL:
                    return ((this as EXPRCALL).MethodWithInst as SymWithType);

                case EXPRKIND.PROP:
                    return ((this as EXPRPROP).SlotPropWithType as SymWithType);

                case EXPRKIND.FIELD:
                    return ((this as EXPRFIELD).FieldWithType as SymWithType);

                case EXPRKIND.EVENT:
                    return ((this as EXPREVENT).EventWithType as SymWithType);
            }
        }

        //------------------------------------------------------------
        // EXPR.GetSeqVal
        //
        /// <summary>
        /// Scan through EK_SEQUENCE and EK_SEQREV exprs to get the real value.
        /// </summary>
        /// <remarks>
        /// In the case of linked list, GetSeqVal() returns this, and this is proper. 
        /// </remarks>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR GetSeqVal()
        {
            // Scan through EXPRKIND.SEQUENCE and EXPRKIND.SEQREV exprs to get the real value.
            //if (!this) return null;

            EXPR valueExpr = this;
            for (;;)
            {
                switch (valueExpr.Kind)
                {
                    default:
                        return valueExpr;

                    case EXPRKIND.SEQUENCE:
                        valueExpr = valueExpr.AsBIN.Operand2;
                        break;

                    case EXPRKIND.SEQREV:
                        valueExpr = valueExpr.AsBIN.Operand1;
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // EXPR.GetConst
        //
        /// <summary>
        /// Determine whether this expr has a constant value (EK_CONSTANT or EK_ZEROINIT),
        /// possibly with side effects (via EK_SEQUENCE or EK_SEQREV).
        /// Returns NULL if not, or the constant expr if so.
        /// The returned EXPR will always be an EK_CONSTANT or EK_ZEROINIT.
        /// </summary>
        /// <remarks>
        /// In the case of linked list, GetSeqVal() returns this, and this is proper. 
        /// </remarks>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR GetConst()
        {
            EXPR valueExpr = GetSeqVal();
            if (valueExpr == null || valueExpr.Kind != EXPRKIND.CONSTANT && valueExpr.Kind != EXPRKIND.ZEROINIT)
            {
                return null;
            }
            return valueExpr;
        }

        //------------------------------------------------------------
        // EXPR.Set
        //
        /// <summary></summary>
        /// <param name="src"></param>
        //------------------------------------------------------------
        virtual internal void Set(EXPR src)
        {
            this.Kind = src.Kind;
            this.Flags = src.Flags;
            this.TreeNode = src.TreeNode;
            this.TypeSym = src.TypeSym;
        }

        //------------------------------------------------------------
        // EXPR.SetRuntimeBinded (1)
        //
        /// <summary>
        /// <para>(CS4)</para>
        /// <para>(2016/12/23 hirano567@hotmail.co.jp)</para>
        /// </summary>
        //------------------------------------------------------------
        internal void SetRuntimeBinded()
        {
            this.Flags |= EXPRFLAG.RUNTIMEBINDED;
        }

        //------------------------------------------------------------
        // EXPR.SetRuntimeBinded (2)
        //
        /// <summary>
        /// <para>(CS4)</para>
        /// <para>(2016/12/31 hirano567@hotmail.co.jp)</para>
        /// </summary>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal static void SetRuntimeBinded(ref EXPRFLAG flags)
        {
            flags |= EXPRFLAG.RUNTIMEBINDED;
        }

        //------------------------------------------------------------
        // EXPR.IsRuntimeBinded (property)
        //
        /// <summary>
        /// <para>(CS4)</para>
        /// <para>(2016/12/23 hirano567@hotmail.co.jp)</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsRuntimeBinded
        {
            get { return ((this.Flags & EXPRFLAG.RUNTIMEBINDED) != 0); }
        }

#if DEBUG
        virtual internal void zDummy() { }

        //------------------------------------------------------------
        // EXPR.Debug
        //------------------------------------------------------------
        virtual internal void Debug(StringBuilder sb)
        {
            DebugExpr(sb);
            sb.Append("\n");
        }

        //------------------------------------------------------------
        // EXPR.DebugExpr
        //------------------------------------------------------------
        internal void DebugExpr(StringBuilder sb)
        {
            sb.AppendFormat("No.{0}: {1}\n", this.ExprID, this.Kind);
            sb.AppendFormat("Flags       : {0}\n", this.Flags);
            if (this.TreeNode != null)
            {
                sb.AppendFormat("TreeNode    : No.{0} ({1})\n",
                    this.TreeNode.NodeID, this.TreeNode.Kind);
            }
            if (this.TypeSym != null)
            {
                sb.AppendFormat("TypeSym     : No.{0} ({1}) {2}\n",
                    this.TypeSym.SymID, this.TypeSym.Kind, this.TypeSym.Name);
            }
        }
#endif
    }

    //======================================================================
    // enum DELIMKIND
    //
    /// <summary>
    /// <para>START or END.</para>
    /// <para>(CSharp\SCComp\ExprList.cs)</para>
    /// </summary>
    //======================================================================
    internal enum DELIMKIND : int
    {
        START,
        END
    }

    //// Declare the EXPR node trees
    //#define DECLARE_EXPR( name, base)           class EXPR ## name: public base { public:
    //#define DECLARE_EXPR_NO_EK( name, base)     class EXPR ## name: public base { public:
    //#define CHILD_EXPR( name, expr_type)            expr_type * name;
    //#define CHILD_EXPR_NO_RECURSE( name, expr_type) expr_type * name;
    //#define NEXT_EXPR( name, expr_type)             expr_type * name;
    //#define CHILD_SYM( name, sym_type)              sym_type *  name;
    //#define CHILD_TYPESYM( name, sym_type)          sym_type *  name;
    //#define CHILD_TYPEARRAY( name)                  TypeArray * name;
    //#define CHILD_OTHER( name, type)                type        name;
    //#define CHILD_OTHER_SZ( name, type, size)       type        name : size;
    //#define CHILD_WITHINST( name, type)             type        name;
    //#define CHILD_WITHTYPE( name, type)             type        name;
    //#define END_EXPR( )                         };
    //#define INCLUDE_IMPL
    //
    //#include "exprnodes.h"

    //======================================================================
    // class EXPRSTMT
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRSTMT : EXPR
    {
        //------------------------------------------------------------
        // EXPRSTMT Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// next statement
        /// </summary>
        internal EXPRSTMT NextStatement;    // * stmtNext;

        /// <summary>
        /// parent statement
        /// </summary>
        internal EXPRSTMT ParentStatement;  // * stmtPar;

        //------------------------------------------------------------
        // EXPRSTMT.Reachable
        //
        /// <summary>
        /// Return true if UNREACHABLEBEGIN flag is not set.
        /// </summary>
        /// <remarks>
        /// (sscli) FReachable()
        /// </remarks>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Reachable()
        {
            return (Flags & EXPRFLAG.UNREACHABLEBEGIN) == 0;
        }

        //------------------------------------------------------------
        // EXPRSTMT.ReachableEnd
        //
        /// <summary>
        /// <para>Return true if UNREACHABLEEND flag is not set.</para>
        /// <para>(FReachableEnd() in sscli.)</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ReachableEnd()
        {
            return (Flags & EXPRFLAG.UNREACHABLEEND) == 0;
        }

        //------------------------------------------------------------
        // EXPRSTMT.SetReachable
        //
        /// <summary>
        /// Set UNREACHABLEBEGIN flag.
        /// </summary>
        //------------------------------------------------------------
        internal void SetReachable()
        {
            Flags &= ~EXPRFLAG.UNREACHABLEBEGIN;
        }

        //------------------------------------------------------------
        // EXPRSTMT.SetReachableEnd
        //
        /// <summary>
        /// Clear UNREACHABLEBEGIN flag and UNREACHABLEEND flag.
        /// </summary>
        //------------------------------------------------------------
        internal void SetReachableEnd()
        {
            Flags &= ~(EXPRFLAG.UNREACHABLEBEGIN | EXPRFLAG.UNREACHABLEEND);
        }

        //------------------------------------------------------------
        // EXPRSTMT.ClearReachable
        //
        /// <summary>
        /// Set UNREACHABLEBEGIN flag and UNREACHABLEEND flag.
        /// </summary>
        //------------------------------------------------------------
        internal void ClearReachable()
        {
            Flags |= (EXPRFLAG.UNREACHABLEBEGIN | EXPRFLAG.UNREACHABLEEND);
        }

        //------------------------------------------------------------
        // EXPRSTMT.ClearReachableEnd
        //
        /// <summary>
        /// Set UNREACHABLEEND flag.
        /// </summary>
        //------------------------------------------------------------
        internal void ClearReachableEnd()
        {
            Flags |= EXPRFLAG.UNREACHABLEEND;
        }

        //------------------------------------------------------------
        // EXPRSTMT.Set
        //
        /// <summary>
        /// Set fields by src.
        /// </summary>
        /// <remarks>
        /// 2015/01/07 hirano567@hotmail.co.jp
        /// </remarks>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal override void Set(EXPR src)
        {
            base.Set(src);
            EXPRSTMT stmt = src as EXPRSTMT;
            if (stmt != null)
            {
                this.NextStatement = stmt.NextStatement;
                this.ParentStatement = stmt.ParentStatement;
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRSTMT.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.ParentStatement != null)
            {
                sb.AppendFormat("Parent      : No.{0} ({1})\n", this.ParentStatement.ExprID, this.ParentStatement.Kind);
            }
            else
            {
                sb.Append("Parent      :\n");
            }
            if (this.NextStatement != null)
            {
                sb.AppendFormat("Next        : No.{0}\n", this.NextStatement.ExprID);
            }
            else
            {
                sb.Append("Next        :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRBINOP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRBINOP : EXPR
    {
        /// <summary>
        /// first member
        /// </summary>
        internal EXPR Operand1 = null;  // * p1;

        /// <summary>
        /// last member
        /// </summary>
        internal EXPR Operand2 = null;  // * p2;

#if DEBUG
        //------------------------------------------------------------
        // EXPRBINOP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.Operand1 != null)
            {
                sb.AppendFormat(
                    "Operand1    : No.{0} ({1})\n",
                    this.Operand1.ExprID,
                    this.Operand1.Kind);
            }
            else
            {
                sb.Append("Operand1    :\n");
            }

            if (this.Operand2 != null)
            {
                sb.AppendFormat(
                    "Operand2    : No.{0} ({1})\n",
                    this.Operand2.ExprID,
                    this.Operand2.Kind);
            }
            else
            {
                sb.Append("Operand2    :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRUSERLOGOP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRUSERLOGOP : EXPR
    {
        internal EXPR OpX = null;       // * opX;
        internal EXPR CallTF = null;    // * callTF;
        internal EXPR CallOp = null;    // * callOp;

#if DEBUG
        //------------------------------------------------------------
        // EXPRUSERLOGOP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.OpX != null)
            {
                sb.AppendFormat("OpX         : No.{0}\n", this.OpX.ExprID);
            }
            else
            {
                sb.Append("OpX         :\n");
            }
            if (this.CallTF != null)
            {
                sb.AppendFormat("CallTF      : No.{0}\n", this.CallTF);
            }
            else
            {
                sb.Append("CallTF      :\n");
            }
            if (this.CallOp != null)
            {
                sb.AppendFormat("CallOp      : No.{0}\n", this.CallOp);
            }
            else
            {
                sb.Append("CallOp      :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRDBLQMARK
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRDBLQMARK : EXPR
    {
        internal EXPR TestExpr = null;      // * exprTest;
        internal EXPR ConvertExpr = null;   // * exprConv;
        internal EXPR ElseExpr = null;      // * exprElse;

#if DEBUG
        //------------------------------------------------------------
        // EXPRDBLQMARK.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.TestExpr != null)
            {
                sb.AppendFormat("TestExpr    : No.{0} ({1})\n", TestExpr.ExprID, TestExpr.Kind);
            }
            else
            {
                sb.Append("TestExpr    :\n");
            }
            if (this.ConvertExpr != null)
            {
                sb.AppendFormat("ConvertExpr : No.{0} ({1})\n", ConvertExpr.ExprID, ConvertExpr.Kind);
            }
            else
            {
                sb.Append("ConvertExpr :\n");
            }
            if (this.ElseExpr != null)
            {
                sb.AppendFormat("ElseExpr    : No.{0} ({1})\n", ElseExpr.ExprID, ElseExpr.Kind);
            }
            else
            {
                sb.Append("ElseExpr    :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRTYPEOF
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRTYPEOF : EXPR
    {
        internal TYPESYM SourceTypeSym = null;  // * sourceType;
        internal METHSYM MethodSym = null;      // * method;

#if DEBUG
        //------------------------------------------------------------
        // EXPRTYPEOF.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (SourceTypeSym != null)
            {
                sb.AppendFormat("SourceTypeSym   : No.{0}\n", SourceTypeSym.SymID);
            }
            else
            {
                sb.Append("SourceTypeSym   :\n");
            }
            if (MethodSym != null)
            {
                sb.AppendFormat("MethodSym       : No.{0}\n", MethodSym.SymID);
            }
            else
            {
                sb.Append("MethodSym       :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRSIZEOF
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRSIZEOF : EXPR
    {
        internal TYPESYM SourceTypeSym = null;  // * sourceType;

#if DEBUG
        //------------------------------------------------------------
        // EXPRSIZEOF.Debug
        //------------------------------------------------------------
        override internal void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.SourceTypeSym != null)
            {
                sb.AppendFormat("TypeSym     : No.{0} {1} {2}\n",
                    this.SourceTypeSym.SymID,
                    this.SourceTypeSym.Kind,
                    this.SourceTypeSym.Name);
            }
            else
            {
                sb.Append("TypeSym     :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRCAST
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRCAST : EXPR
    {
        /// <summary>
        /// thing being cast
        /// </summary>
        internal EXPR Operand = null;   // * p1;

#if DEBUG
        //------------------------------------------------------------
        // EXPRCAST.Debug
        //------------------------------------------------------------
        override internal void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.Operand != null)
            {
                sb.AppendFormat("Operand     : No.{0} {1}\n",
                    this.Operand.ExprID,
                    this.Operand.Kind);
            }
            else
            {
                sb.Append("Operand     :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRZEROINIT
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRZEROINIT : EXPR
    {
        /// <summary>
        /// thing being inited, if any...
        /// </summary>
        internal EXPR Operand = null;   // * p1;

#if DEBUG
        //------------------------------------------------------------
        // EXPRZEROINIT.Debug
        //------------------------------------------------------------
        override internal void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.Operand != null)
            {
                sb.AppendFormat("Operand     : No.{0} {1}\n",
                    this.Operand.ExprID,
                    this.Operand.Kind);
            }
            else
            {
                sb.Append("Operand     :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRBLOCK
    //
    /// <summary>
    /// <para>Derives from EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRBLOCK : EXPRSTMT
    {
        internal EXPRSTMT StatementsExpr = null; // * statements;

        /// <summary>
        /// the block in which this block appears...
        /// </summary>
        internal EXPRBLOCK OwingBlockExpr = null;   // * owningBlock;

        /// <summary>
        /// corresponding scope symbol (could be NULL if none).
        /// </summary>
        internal SCOPESYM ScopeSym = null;  // * scopeSymbol;

        internal BitSet ExitBitSet = new BitSet();  // bsetExit;

#if DEBUG
        //------------------------------------------------------------
        // EXPRBLOCK.Debug
        //------------------------------------------------------------
        override internal void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.OwingBlockExpr != null)
            {
                sb.AppendFormat("OwingBlock  : No.{0}\n", OwingBlockExpr.ExprID);
            }
            else
            {
                sb.Append("OwingBlock  :\n");
            }
            if (this.ScopeSym != null)
            {
                sb.AppendFormat("ScopeSym    : No.{0}\n", ScopeSym.SymID);
            }
            else
            {
                sb.Append("ScopeSym    :\n");
            }
            if (this.StatementsExpr != null)
            {
                sb.AppendFormat("Statements  : No.{0} {1}\n",
                    this.StatementsExpr.ExprID,
                    this.StatementsExpr.Kind);
            }
            else
            {
                sb.Append("Statements  :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRSTMTAS
    //
    /// <summary>
    /// <para>Derives from EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRSTMTAS : EXPRSTMT
    {
        internal EXPR Expr = null;   // * expression;

#if DEBUG
        //------------------------------------------------------------
        // EXPRSTMTAS.Debug
        //------------------------------------------------------------
        override internal void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.Expr != null)
            {
                sb.AppendFormat("Expr        : No.{0} {1}\n", this.Expr.ExprID, this.Expr.Kind);
            }
            else
            {
                sb.Append("Expr        :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRMEMGRP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRMEMGRP : EXPR
    {
        /// <summary>
        /// The node that indicates just the "name" portion.
        /// </summary>
        internal BASENODE NameNode = null;          // * nodeName;

        internal string Name = null;                // NAME * name;

        internal TypeArray TypeArguments = null;    // * typeArgs;

        internal SYMKIND SymKind;                   // sk;

        /// <summary>
        /// The type containing the members.
        /// This may be a TYVARSYM or an AGGTYPESYM.
        /// This may be NULL (if types is not NULL).
        /// </summary>
        internal TYPESYM ParentTypeSym = null;      // * typePar;

        internal METHPROPSYM MethPropSym = null;    // * mps;

        /// <summary>
        /// The object expression. NULL for a static invocation.
        /// </summary>
        internal EXPR ObjectExpr = null;            // * object;

        /// <summary>
        /// The types (within type) that contain the members.
        /// These are computed by MemberLookup.Lookup.
        /// This may be null if typePar is a class.
        /// </summary>
        internal TypeArray ContainingTypeArray = null;  // * types;

#if DEBUG
        //------------------------------------------------------------
        // EXPRMEMGRP.Debug
        //------------------------------------------------------------
        override internal void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (NameNode != null)
            {
                NAMENODE node = NameNode as NAMENODE;
                string name = (node != null ? node.Name : "");
                sb.AppendFormat("NameNode            : No.{0} {1}\n", NameNode.NodeID, name);
            }
            else
            {
                sb.Append("NameNode            :\n");
            }

            if (String.IsNullOrEmpty(Name))
            {
                sb.Append("Name                :\n");
            }
            else
            {
                sb.AppendFormat("Name                : {0}\n", Name);
            }

            sb.AppendFormat("SYMKIND             : {0}\n", SymKind);

            if (TypeSym != null)
            {
                sb.AppendFormat("TypeSym             : No.{0}\n", TypeSym.SymID);
            }
            else
            {
                sb.Append("TypeSym             :\n");
            }

            if (MethPropSym != null)
            {
                sb.AppendFormat("MethPropSym         : No.{0}\n", MethPropSym.SymID);
            }
            else
            {
                sb.Append("MethPropSym         :\n");
            }

            if (ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr          : No.{0}\n", ObjectExpr.ExprID);
            }
            else
            {
                sb.Append("ObjectExpr          :\n");
            }

            sb.Append("ContainingTypeArray :");

            if (this.ContainingTypeArray != null)
            {
                for (int i = 0; i < ContainingTypeArray.Count; ++i)
                {
                    sb.AppendFormat(" No.{0}", ContainingTypeArray[i].SymID);
                }
            }
        }
#endif
    }

    //======================================================================
    // class EXPRCALL
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRCALL : EXPR
    {
        /// <summary>
        /// must be 1st
        /// </summary>
        internal EXPR ObjectExpr = null;                            // * object;

        /// <summary>
        /// must be 2nd to match EXPRBINOP.p2
        /// </summary>
        internal EXPR ArgumentsExpr = null;                         // * args;

        internal MethWithInst MethodWithInst = new MethWithInst();  // mwi;

#if DEBUG
        //------------------------------------------------------------
        // EXPRCALL.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat("MethodWithInst  : {0}\n", MethodWithInst.Debug());

            if (this.ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr      : No.{0} {1}\n", this.ObjectExpr.ExprID, this.ObjectExpr.Kind);
            }
            else
            {
                sb.Append("ObjectExpr      :\n");
            }
            if (this.ArgumentsExpr != null)
            {
                sb.AppendFormat("ArgumentsExpr   : No.{0} {1}\n",
                    this.ArgumentsExpr.ExprID,
                    this.ArgumentsExpr.Kind);
            }
            else
            {
                sb.Append("ArgumentsExpr   :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRPROP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRPROP : EXPR
    {
        /// <summary>
        /// must be 1st
        /// </summary>
        internal EXPR ObjectExpr = null;                                // * object;

        /// <summary>
        /// must be 2nd to match EXPRBINOP.p2
        /// </summary>
        internal EXPR ArgumentsExpr = null;                             // * args;

        internal PropWithType SlotPropWithType = new PropWithType();    // pwtSlot;

        internal MethWithType GetMethodWithType = new MethWithType();   // mwtGet;

        internal MethWithType SetMethodWithType = new MethWithType();   // mwtSet;

#if DEBUG
        //------------------------------------------------------------
        // EXPRPROP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (this.SlotPropWithType != null)
            {
                sb.AppendFormat("SlotPropWithType    : {0}\n", this.SlotPropWithType.Debug());
            }
            else
            {
                sb.Append("SlotPropWithType    :\n");
            }
            if (this.GetMethodWithType != null)
            {
                sb.AppendFormat("GetMethodWithType   : {0}\n", this.GetMethodWithType.Debug());
            }
            else
            {
                sb.Append("GetMethodWithType   :\n");
            }
            if (this.SetMethodWithType != null)
            {
                sb.AppendFormat("SetMethodWithType   : {0}\n", this.SetMethodWithType.Debug());
            }
            else
            {
                sb.Append("SetMethodWithType   :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRFIELD
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRFIELD : EXPR
    {
        //------------------------------------------------------------
        // EXPRFIELD Fields and Properties
        //------------------------------------------------------------

        /// <remarks>
        /// // must be 1st
        /// </remarks>
        internal EXPR ObjectExpr = null;    // * object;

        internal FieldWithType FieldWithType = new FieldWithType(); // fwt;

        internal int Offset = -1;   // uint offset;

#if DEBUG
        internal bool CheckedMarshalByRef = false;  // fCheckedMarshalByRef
#endif

        //------------------------------------------------------------
        // EXPRFIELD.FixedAgg
        //------------------------------------------------------------
        internal AGGSYM FixedAgg()
        {
            return (this.FieldWithType != null ?
                FieldWithType.FieldSym.FixedAggSym : null);
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRFIELD.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr      : No.{0}\n", ObjectExpr.ExprID);
            }
            else
            {
                sb.Append("ObjectExpr      :\n");
            }
            if (this.FieldWithType != null)
            {
                sb.AppendFormat("FieldWithType   : {0}\n", this.FieldWithType.Debug());
            }
            else
            {
                sb.Append("FieldWithType   :\n");
            }
            sb.AppendFormat("Offset          : No.{0}\n", Offset);
        }
#endif
    }

    //======================================================================
    // class EXPREVENT
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPREVENT : EXPR
    {
        /// <summary>
        /// must be 1st
        /// </summary>
        internal EXPR ObjectExpr = null;    // * object;

        internal EventWithType EventWithType = new EventWithType(); // ewt

#if DEBUG
        //------------------------------------------------------------
        // EXPREVENT.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr      : No.{0}\n", ObjectExpr.ExprID);
            }
            else
            {
                sb.Append("ObjectExpr      :\n");
            }
            if (this.EventWithType != null)
            {
                sb.AppendFormat("EventWithType   : {0}\n",
                    this.EventWithType.Debug());
            }
            else
            {
                sb.Append("EventWithType   :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRDECL
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRDECL : EXPRSTMT
    {
        internal LOCVARSYM LocVarSym = null;    // * sym;
        internal EXPR InitialExpr = null;       // * init;

#if DEBUG
        //------------------------------------------------------------
        // EXPRDECL.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (LocVarSym != null)
            {
                sb.AppendFormat("LocVarSym   : No.{0}\n", LocVarSym.SymID);
            }
            else
            {
                sb.Append("LocVarSym   :\n");
            }
            if (InitialExpr != null)
            {
                sb.AppendFormat("InitialExpr : No.{0}\n", InitialExpr.ExprID);
            }
            else
            {
                sb.Append("InitialExpr :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRLOCAL
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRLOCAL : EXPR
    {
        internal LOCVARSYM LocVarSym = null;  // * local;

        /// <summary>
        /// (CS3) for expression trees 
        /// </summary>
        internal bool ToExpressionTree = false;

        /// <summary>
        /// (CS3) for expression trees, set MemberExpression to TypeSym.
        /// So, set the real TypeSym to this field.
        /// </summary>
        internal TYPESYM RealTypeSym = null;

#if DEBUG
        //------------------------------------------------------------
        // EXPRLOCAL.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (LocVarSym != null)
            {
                sb.AppendFormat("LocVarSym   : No.{0}\n", LocVarSym.SymID);
            }
            else
            {
                sb.Append("LocVarSym   :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRRETURN
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRRETURN : EXPRSTMT
    {
        internal SCOPESYM CurrentScopeSym = null;   // * currentScope;
        internal EXPR ObjectExpr = null;            // * object;

#if DEBUG
        //------------------------------------------------------------
        // EXPRRETURN.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (CurrentScopeSym != null)
            {
                sb.AppendFormat("CurrentScopeSym : No.{0}\n", CurrentScopeSym.SymID);
            }
            else
            {
                sb.Append("LocVarSym   :\n");
            }
            if (ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr      : No.{0}\n", ObjectExpr.ExprID);
            }
            else
            {
                sb.Append("ObjectExpr      :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRTHROW
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRTHROW : EXPRSTMT
    {
        internal EXPR ObjectExpr = null;    // * object;

#if DEBUG
        //------------------------------------------------------------
        // EXPRTHROW.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr  : No.{0}\n", ObjectExpr.ExprID);
            }
            else
            {
                sb.Append("ObjectExpr  :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRCONSTANT
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRCONSTANT : EXPR
    {
        //------------------------------------------------------------
        // EXPRCONSTANT Fields and Properties
        //------------------------------------------------------------
        private CONSTVAL constValue = new CONSTVAL(); // val

        internal CONSTVAL ConstVal
        {
            get { return this.constValue; } // getConstVal()
            set { this.constValue = value; }
        }

        //internal List<EXPR> ExprList = null;  // EXPR ** pList; EXPR * list;
        internal EXPR List = null;              // EXPR * list;
        internal EXPR ListLast = null;          // EXPR ** pList;

        //------------------------------------------------------------
        // EXPRCONSTANT.ClearList
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearList()
        {
            this.List = null;
            this.ListLast = null;
        }

        //------------------------------------------------------------
        // EXPRCONSTANT.RealizeStringConstant
        //
        /// <summary>
        /// The expressions in ExprList convert to a string and set it to constVal.
        /// </summary>
        //------------------------------------------------------------
        internal void RealizeStringConstant()
        {
            //if (this.ExprList == null || this.ExprList.Count == 0) return;
            if (this.List == null)
            {
                return;
            }

            StringBuilder buffer = new StringBuilder();
            string strVal = constValue.GetString();
            EXPR nd = null, item = null;
            string str;

            if (strVal != null)
            {
                buffer.Append(strVal);
            }

            //nd = ExprList[0];
            nd = this.List;
            while (nd != null)
            {
                if (nd.Kind == EXPRKIND.LIST)
                {
                    item = nd.AsBIN.Operand1;
                    nd = nd.AsBIN.Operand2;
                }
                else
                {
                    item = nd;
                    nd = null;
                }
                EXPRCONSTANT econst = item as EXPRCONSTANT;
                if (econst != null)
                {
                    str = econst.constValue.GetString();
                    if (str != null)
                    {
                        buffer.Append(str);
                    }
                }
            }

            constValue.SetString(buffer.ToString());
            //ExprList.Clear();
            this.List = null;
            this.ListLast = null;
        }

        //------------------------------------------------------------
        // EXPRCONSTANT.IsNull
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool IsNull()
        {
            return (TypeSym.FundamentalType() == FUNDTYPE.REF && constValue.IsNull);
        }

        //------------------------------------------------------------
        // EXPRCONSTANT.IsEqual
        //
        /// <summary></summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsEqual(EXPRCONSTANT expr)
        {
            FUNDTYPE ft1 = TypeSym.FundamentalType();
            FUNDTYPE ft2 = expr.TypeSym.FundamentalType();
            if ((ft1 == FUNDTYPE.REF) ^ (ft2 == FUNDTYPE.REF)) return false;

            //if (ExprList.Count > 0 && ExprList[0] != null) RealizeStringConstant();
            //if (expr.ExprList.Count > 0 && expr.ExprList[0] != null) expr.RealizeStringConstant();
            if (this.List != null) this.RealizeStringConstant();
            if (expr.List != null) expr.RealizeStringConstant();

            if (ft1 == FUNDTYPE.REF)
            {
                if (constValue.GetString() == null)
                {
                    return (expr.constValue.GetString() == null);
                }
                //return expr.val.strVal && val.strVal.length == expr.val.strVal.length && 
                //    !memcmp(val.strVal.text, expr.val.strVal.text, sizeof(WCHAR) * val.strVal.length );
                if (expr.constValue.GetString() != null &&
                    constValue.GetString().Length == expr.constValue.GetString().Length)
                {
                    constValue.SetString(expr.constValue.GetString());
                    return true;
                }
                return false;
            }
            else
            {
                return (GetI64Value() == expr.GetI64Value());
            }
        }

        //------------------------------------------------------------
        // EXPRCONSTANT.GetVal
        //------------------------------------------------------------
        //internal Object GetVal()
        //{
        //    return constValue.ObjectValue;
        //}

        //------------------------------------------------------------
        // EXPRCONSTANT.GetStringValue
        //
        /// <summary>
        /// <para>(getSVal() in sscli)</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetStringValue()
        {
            if (this.List != null)
            {
                RealizeStringConstant();
            }
            return constValue.GetString();
        }

        //------------------------------------------------------------
        // EXPRCONSTANT.GetI64Value
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal long GetI64Value()
        {
            FUNDTYPE ft = TypeSym.FundamentalType();

            switch (ft)
            {
                case FUNDTYPE.I8:
                case FUNDTYPE.U8:
                case FUNDTYPE.U4:
                case FUNDTYPE.I1:
                case FUNDTYPE.I2:
                case FUNDTYPE.I4:
                case FUNDTYPE.U1:
                case FUNDTYPE.U2:
                    return (constValue.GetLong());

                default:
                    DebugUtil.Assert(false, "Bad fundType in getI64Value");
                    return 0;
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRTHROW.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (constValue != null && constValue.GetObject() != null)
            {
                sb.AppendFormat("constValue  : {0}\n", constValue.GetObject());
            }
            else
            {
                sb.Append("constValue  :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRCLASS
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRCLASS : EXPR
    {
    }

    //======================================================================
    // class EXPRTYPE
    //
    /// <summary>
    /// No definition in sscli.
    /// </summary>
    //======================================================================
    internal class EXPRTYPE : EXPR
    {
    }

    //======================================================================
    // class EXPRNSPACE
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRNSPACE : EXPR
    {
        internal NSAIDSYM NsAidSym = null;  // * nsa;

#if DEBUG
        //------------------------------------------------------------
        // EXPRNSPACE.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (NsAidSym != null)
            {
                sb.AppendFormat("NsAidSym    : No.{0}\n", NsAidSym.SymID);
            }
            else
            {
                sb.Append("NsAidSym    :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRLABEL
    //
    /// <summary>
    /// <para>Derives from EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRLABEL : EXPRSTMT
    {
        //------------------------------------------------------------
        // EXPRLABEL Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// the emitted block which follows this label
        /// </summary>
        internal BBLOCK BBlock = null;          // * block

        internal SCOPESYM ScopeSym = null;      // * scope

        /// <summary>
        /// the symbol, if any (only defined on explicit, non-switch, user labels)
        /// </summary>
        internal LABELSYM LabelSym = null;      // * label

        internal BitSet EnterBitSet = new BitSet(); // bsetEnter
        internal int TsFinallyScan = 0;             // tsFinallyScan

        /// <summary>
        /// For temporary lists during reachability and definite assignment analysis.
        /// </summary>
        private EXPRLABEL NextLabel = null;     // * labNext

        //------------------------------------------------------------
        // EXPRLABEL.PushOnStack
        //
        /// <summary></summary>
        /// <param name="topLabel"></param>
        //------------------------------------------------------------
        internal void PushOnStack(ref EXPRLABEL topLabel)
        {
            DebugUtil.Assert(this.Kind == EXPRKIND.LABEL);
            DebugUtil.Assert(this.NextLabel == null);

            this.NextLabel = topLabel;
            topLabel = this;

            if (this.NextLabel == null)
            {
                this.NextLabel = this;
            }
        }

        //------------------------------------------------------------
        // EXPRLABEL.PopFromStack
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal EXPRLABEL PopFromStack(ref EXPRLABEL topLabel)
        {
            EXPRLABEL currentLabel = topLabel;
            if (currentLabel == null)
            {
                return null;
            }
            topLabel = currentLabel.NextLabel;
            currentLabel.NextLabel = null;

            if (topLabel == currentLabel)
            {
                topLabel = null;
            }
            return currentLabel;
        }

        //------------------------------------------------------------
        // EXPRLABEL.InStack
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InStack()
        {
            DebugUtil.Assert(this.NextLabel == null || this.Kind == EXPRKIND.LABEL);
            return (this.NextLabel != null);
        }

        //------------------------------------------------------------
        // EXPRLABEL.Set
        //
        /// <summary>
        /// Set fields by src.
        /// </summary>
        /// <remarks>
        /// 2015/01/07 hirano567@hotmail.co.jp
        /// </remarks>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal override void Set(EXPR src)
        {
            base.Set(src);
            EXPRLABEL lab = src as EXPRLABEL;
            if (lab != null)
            {
                this.BBlock = lab.BBlock;
                this.ScopeSym = lab.ScopeSym;
                this.LabelSym = lab.LabelSym;
                this.EnterBitSet = (lab.EnterBitSet == null) ? null : new BitSet(lab.EnterBitSet);
                this.TsFinallyScan = lab.TsFinallyScan;
                this.NextLabel = lab.NextLabel;
            }
        }

        //------------------------------------------------------------
        // EXPRLABEL.GetID
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetIDString()
        {
            if (this.LabelSym != null)
            {
                return String.Format("<Lable>_S{0}", this.LabelSym.SymID);
            }
            return String.Format("<Lable>_E{0}", this.ExprID);
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRLABEL.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (BBlock != null)
            {
                sb.AppendFormat("BBlock          : No.{0}\n", BBlock.BBlockID);
            }
            else
            {
                sb.Append("BBlock          :\n");
            }
            if (ScopeSym != null)
            {
                sb.AppendFormat("ScopeSym        : No.{0}\n", ScopeSym.SymID);
            }
            else
            {
                sb.Append("ScopeSym        :\n");
            }
            if (LabelSym != null)
            {
                sb.AppendFormat("LabelSym        : No.{0}\n", LabelSym.SymID);
            }
            else
            {
                sb.Append("LabelSym        :\n");
            }
            if (EnterBitSet != null)
            {
                sb.AppendFormat("EnterBitSet     : No.{0}\n", EnterBitSet.Debug());
            }
            else
            {
                sb.Append("EnterBitSet     :\n");
            }
            sb.AppendFormat("TsFinallyScan   : {0}\n", TsFinallyScan);
            if (NextLabel != null)
            {
                sb.AppendFormat("NextLabel       : No.{0}\n", NextLabel.ExprID);
            }
            else
            {
                sb.Append("NextLabel       :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRGOTO
    //
    /// <summary>
    /// <para>Derives from EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRGOTO : EXPRSTMT
    {
        private object labelObjct = null;

        /// <summary>
        /// if we need to realized in the def-use pass.
        /// </summary>
        internal string LabelName   // NAME * labelName;
        {
            get { return this.labelObjct as string; }
            set { this.labelObjct = value; }
        }

        /// <summary>
        /// <para>if know the code location of the dest</para>
        /// </summary>
        internal EXPRLABEL LabelExpr    // EXPRLABEL * label;
        {
            get { return this.labelObjct as EXPRLABEL; }
            set { this.labelObjct = value; }
        }

        /// <summary>
        /// the goto's location scope
        /// </summary>
        internal SCOPESYM CurrentScopeSym = null;   // * currentScope;

        /// <summary>
        /// the scope of the label (only goto case, and break&cont knows that scope)
        /// </summary>
        internal SCOPESYM TargetScopeSym = null;    // * targetScope;

        internal EXPRGOTO PreviousExpr = null;      // * prev;

#if DEBUG
        //------------------------------------------------------------
        // EXPRGOTO.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (LabelName != null)
            {
                sb.AppendFormat("LabelName       : No.{0}\n", LabelName);
            }
            if (LabelExpr != null)
            {
                sb.AppendFormat("LabelExpr       : No.{0}\n", LabelExpr.ExprID);
            }
            else
            {
                sb.Append("labelObject     :\n");
            }
            if (CurrentScopeSym != null)
            {
                sb.AppendFormat("CurrentScopeSym : {0}\n", CurrentScopeSym.SymID);
            }
            else
            {
                sb.Append("CurrentScopeSym :\n");
            }
            if (TargetScopeSym != null)
            {
                sb.AppendFormat("TargetScopeSym  : {0}\n", TargetScopeSym.SymID);
            }
            else
            {
                sb.Append("TargetScopeSym  :\n");
            }
            if (PreviousExpr != null)
            {
                sb.AppendFormat("PreviousExpr    : {0}\n", PreviousExpr.ExprID);
            }
            else
            {
                sb.Append("PreviousExpr    :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRGOTOIF
    //
    /// <summary>
    /// <para>Derives from eXPRGOTO.</para>
    /// </summary>
    //======================================================================
    internal class EXPRGOTOIF : EXPRGOTO
    {
        internal EXPR ConditionExpr = null; // * condition;
        internal bool HasSense = false;     // sense;

        internal bool AlwaysJumps()    // FAlwaysJumps()
        {
            return ConditionExpr.IsTrueResult(HasSense);
        }

        internal bool FNeverJumps() // FNeverJumps()
        {
            return ConditionExpr.IsFalseResult(HasSense);
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRGOTOIF.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ConditionExpr != null)
            {
                sb.AppendFormat("ConditionExpr   : {0}\n", ConditionExpr.ExprID);
            }
            else
            {
                sb.Append("ConditionExpr   :\n");
            }
            sb.AppendFormat("HasSense        : {0}\n", HasSense);
        }
#endif
    }

    //======================================================================
    // class EXPRFUNCPTR
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRFUNCPTR : EXPR
    {
        internal MethWithInst MethWithInst = new MethWithInst();  // mwi;
        internal EXPR ObjectExpr = null;    // * object;

#if DEBUG
        //------------------------------------------------------------
        // EXPRFUNCPTR.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (MethWithInst != null)
            {
                sb.AppendFormat("MethWithInst    : {0}\n", MethWithInst.Debug());
            }
            else
            {
                sb.Append("MethWithInst    :\n");
            }
            if (ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr      : {0}\n", ObjectExpr.ExprID);
            }
            else
            {
                sb.Append("ObjectExpr      :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRSWITCH
    //
    /// <summary>
    /// <para>Derives form EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRSWITCH : EXPRSTMT
    {
        /// <summary>
        /// what are we switching on?
        /// </summary>
        internal EXPR ArgumentExpr = null;  // * arg;

        /// <summary>
        /// count of case statements + default statement
        /// </summary>
        internal int LabelCount = 0;    // uint labelCount;

        /// <summary>
        /// NOT A LIST!!! this is an array of label expressions
        /// </summary>
        internal List<EXPRSWITCHLABEL> LabelArray = new List<EXPRSWITCHLABEL>(); // EXPRSWITCHLABEL ** labels

        /// <summary>
        /// lists of switch labels
        /// </summary>
        internal EXPRSWITCHLABEL BodiesExpr = null; // EXPRSWITCHLABEL * bodies;

        internal EXPRLABEL BreakLabelExpr = null;   // * breakLabel;

        internal EXPRLABEL NullLabelExpr = null;    // * nullLabel;

        //internal int HashTableToken;    // mdToken hashtableToken;
        internal FieldInfo HashTableInfo = null;

#if DEBUG
        //------------------------------------------------------------
        // EXPRSWITCH.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ArgumentExpr != null)
            {
                sb.AppendFormat("ArgumentExpr    : {0}\n", ArgumentExpr.ExprID);
            }
            else
            {
                sb.Append("ArgumentExpr    :\n");
            }
            sb.AppendFormat("LabelCount      : {0}\n", LabelCount);
            sb.Append("LabelArray      :");
            if (LabelArray != null)
            {
                for (int i = 0; i < LabelArray.Count; ++i)
                {
                    sb.AppendFormat(" No.{0}", LabelArray[i].ExprID);
                }
            }
            sb.Append("\n");
            if (BodiesExpr != null)
            {
                sb.AppendFormat("BodiesExpr      : {0}\n", BodiesExpr.ExprID);
            }
            else
            {
                sb.Append("BodiesExpr      :\n");
            }
            if (BreakLabelExpr != null)
            {
                sb.AppendFormat("BreakLabelExpr  : {0}\n", BreakLabelExpr.ExprID);
            }
            else
            {
                sb.Append("BreakLabelExpr  :\n");
            }
            if (NullLabelExpr != null)
            {
                sb.AppendFormat("NullLabelExpr   : {0}\n", NullLabelExpr.ExprID);
            }
            else
            {
                sb.Append("NullLabelExpr   :\n");
            }
            if (this.HashTableInfo != null)
            {
                sb.AppendFormat("HashTableInfo  : {0}\n", HashTableInfo);
            }
        }
#endif
    }

    //======================================================================
    // class EXPRHANDLER
    //
    /// <summary>
    /// <para>Derives from EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRHANDLER : EXPRSTMT
    {
        internal EXPRBLOCK HandlerBlock = null; // * handlerBlock;
        internal LOCVARSYM ParameterSym = null; // * param;

#if DEBUG
        //------------------------------------------------------------
        // EXPRHANDLER.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (HandlerBlock != null)
            {
                sb.AppendFormat("HandlerBlock    : {0}\n", HandlerBlock.ExprID);
            }
            else
            {
                sb.Append("HandlerBlock    :");
            }
            if (ParameterSym != null)
            {
                sb.AppendFormat("ParameterSym    : {0}\n", ParameterSym.SymID);
            }
            else
            {
                sb.Append("ParameterSym    :");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRTRY
    //
    /// <summary>
    /// <para>Derives from EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRTRY : EXPRSTMT
    {
        internal EXPRBLOCK TryBlockExpr = null; // * tryblock;

        /// <summary>
        /// either a block, or a chain of EXPRHANDLERs
        /// </summary>
        internal EXPRSTMT HandlersExpr = null;   // * handlers;

        //------------------------------------------------------------
        // EXPRTRY.IsFinallyBlocked
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsFinallyBlocked()
        {
            if ((Flags & EXPRFLAG.ISFINALLY) == 0)
            {
                return false;
            }
            DebugUtil.Assert(HandlersExpr.Reachable());
            DebugUtil.Assert((HandlersExpr.Flags & EXPRFLAG.MARKING) == 0);
            return !HandlersExpr.ReachableEnd();
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRTRY.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (TryBlockExpr != null)
            {
                sb.AppendFormat("TryBlockExpr    : {0}\n", TryBlockExpr.ExprID);
            }
            else
            {
                sb.Append("TryBlockExpr    :\n");
            }
            if (HandlersExpr != null)
            {
                sb.AppendFormat("HandlersExpr    : {0}\n", HandlersExpr.ExprID);
            }
            else
            {
                sb.Append("HandlersExpr    :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRSWITCHLABEL
    //
    /// <summary>
    /// <para>Derives from EXPRLABEL.</para>
    /// </summary>
    //======================================================================
    internal class EXPRSWITCHLABEL : EXPRLABEL
    {
        /// <summary>
        /// the key of the case statement, or null if default
        /// </summary>
        internal EXPR KeyExpr = null;           // * key;

        /// <summary>
        /// statements under this label
        /// </summary>
        internal EXPRSTMT StatementsExpr = null; // * statements;

        /// <summary>
        /// did we already error about this fallthrough before?
        /// </summary>
        internal bool FallThroughProcessed = false;      // fellThrough;

        //------------------------------------------------------------
        // EXPRSWITCHLABEL.Set
        //
        /// <summary>
        /// Set fields by src.
        /// </summary>
        /// <remarks>
        /// 2015/01/07 hirano567@hotmail.co.jp
        /// </remarks>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal override void Set(EXPR src)
        {
            base.Set(src);
            EXPRSWITCHLABEL lab = src as EXPRSWITCHLABEL;
            if (lab != null)
            {
                this.KeyExpr = lab.KeyExpr;
                this.StatementsExpr = lab.StatementsExpr;
                this.FallThroughProcessed = lab.FallThroughProcessed;
            }
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRSWITCHLABEL.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (KeyExpr != null)
            {
                sb.AppendFormat("KeyExpr             : {0}\n", KeyExpr.ExprID);
            }
            else
            {
                sb.Append("KeyExpr             :\n");
            }
            if (StatementsExpr != null)
            {
                sb.AppendFormat("StatementsExpr      : {0}\n", StatementsExpr.ExprID);
            }
            else
            {
                sb.Append("StatementsExpr      :\n");
            }
            sb.AppendFormat("FallThroughProcessed: {0}\n", FallThroughProcessed);
        }
#endif
    }

    //======================================================================
    // class EXPRMULTIGET
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRMULTIGET : EXPR
    {
        internal EXPRMULTI MultiExpr = null;    // * multi;

#if DEBUG
        //------------------------------------------------------------
        // EXPRMULTIGET.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (MultiExpr != null)
            {
                sb.AppendFormat("MultiExpr   : {0}\n", MultiExpr.ExprID);
            }
            else
            {
                sb.Append("MultiExpr   :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRMULTI
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRMULTI : EXPR
    {
        internal EXPR LeftExpr = null;              // * left;
        internal EXPR OperandExpr = null;           // * op;
        internal MultiOpInfo MultiOpInfo = null;    // * pinfo;

#if DEBUG
        //------------------------------------------------------------
        // EXPRMULTI.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (LeftExpr != null)
            {
                sb.AppendFormat("LeftExpr    : {0}\n", LeftExpr.ExprID);
            }
            else
            {
                sb.Append("LeftExpr    :\n");
            }
            if (OperandExpr != null)
            {
                sb.AppendFormat("OperandExpr : {0}\n", OperandExpr.ExprID);
            }
            else
            {
                sb.Append("OperandExpr :\n");
            }
            sb.AppendFormat("MultiOpInfo :\n");
        }
#endif
    }

    //======================================================================
    // class EXPRSTTMP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRSTTMP : EXPR
    {
        internal EXPR SourceExpr = null;            // * src;
        internal LOCSLOTINFO LocSlotInfo = null;    // * slot;

#if DEBUG
        //------------------------------------------------------------
        // EXPRSTTMP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (SourceExpr != null)
            {
                sb.AppendFormat("SourceExpr  : {0}\n", SourceExpr.ExprID);
            }
            else
            {
                sb.Append("LeftExpr    :\n");
            }
            if (LocSlotInfo != null)
            {
                sb.AppendFormat("LocSlotInfo : {0}\n", LocSlotInfo.Debug());
            }
            else
            {
                sb.Append("LocSlotInfo :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRLDTMP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRLDTMP : EXPR
    {
        internal EXPRSTTMP TmpExpr = null;    // * tmp;

#if DEBUG
        //------------------------------------------------------------
        // EXPRLDTMP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (TmpExpr != null)
            {
                sb.AppendFormat("TmpExpr     : {0}\n", TmpExpr.ExprID);
            }
            else
            {
                sb.Append("TmpExpr     :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRFREETMP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRFREETMP : EXPR
    {
        internal EXPRSTTMP TmpExpr = null;  // * tmp;

#if DEBUG
        //------------------------------------------------------------
        // EXPRFREETMP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (TmpExpr != null)
            {
                sb.AppendFormat("TmpExpr     : {0}\n", TmpExpr.ExprID);
            }
            else
            {
                sb.Append("TmpExpr     :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRWRAP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRWRAP : EXPR
    {
        internal EXPR Expr = null;                      // * expr;
        internal LOCSLOTINFO LocSlotInfo = null;        // * slot;
        internal bool DoNotFree = false;                // doNotFree : 1;
        internal bool NeedEmptySlot = false;            // needEmptySlot : 1;
        internal bool IsPinned = false;                 // pinned : 1;
        internal TEMP_KIND TempKind;                    // tempKind;
        internal SCOPESYM ContainingScopeSym = null;    // * containingScope;

#if DEBUG
        //------------------------------------------------------------
        // EXPRWRAP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (Expr != null)
            {
                sb.AppendFormat("Expr                : {0}\n", Expr.ExprID);
            }
            else
            {
                sb.Append("Expr                :\n");
            }
            if (LocSlotInfo != null)
            {
                sb.AppendFormat("LocSlotInfo         : {0}\n", LocSlotInfo.Debug());
            }
            else
            {
                sb.Append("LocSlotInfo         :\n");
            }
            sb.AppendFormat("DoNotFree           : {0}\n", DoNotFree);
            sb.AppendFormat("NeedEmptySlot       : {0}\n", NeedEmptySlot);
            sb.AppendFormat("IsPinned            : {0}\n", IsPinned);
            sb.AppendFormat("TempKind            : {0}\n", TempKind);
            if (ContainingScopeSym != null)
            {
                sb.AppendFormat("ContainingScopeSym  : {0}\n", ContainingScopeSym.SymID);
            }
            else
            {
                sb.Append("ContainingScopeSym  :\n");
            }

        }
#endif
    }

    //======================================================================
    // class EXPRCONCAT
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRCONCAT : EXPR
    {
        internal EXPR List = null;       // EXPR * list;
        internal EXPR ListLast = null;   // EXPR ** pList;
        internal int Count = 0;          // unsigned count;

        //internal List<EXPR> ExprList = new List<EXPR>();    // EXPR * list; EXPR ** pList;

        //internal int Count  // unsigned count;
        //{
        //    get { return this.ExprList.Count; }
        //}

        //------------------------------------------------------------
        // EXPRCONCAT.ClearList
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearList()
        {
            this.List = null;
            this.ListLast = null;
            this.Count = 0;
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRCONCAT.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            //if (Expr != null)
            //{
            //    sb.AppendFormat("Expr                : {0}\n", Expr.ExprID);
            //}
            //else
            //{
            //    sb.Append("Expr                :\n");
            //}
            sb.AppendFormat("Count       : {0}\n", Count);
            sb.Append("List        :");
            sb.AppendFormat(" {0}", this.List.ExprID);
            //EXPR expr = this.List;
            //while (expr != null)
            //{
            //}
            sb.Append("\n");
        }
#endif
    }

    //======================================================================
    // class EXPRARRINIT
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRARRINIT : EXPR
    {
        internal EXPR ArgumentsExpr = null;             // * args;
        internal List<int> DimSizes = new List<int>();  // int* dimSizes; int dimSize;
        // In sscli, if an array has one rank-specifier, use dimSize and dimSizes refers to dimSize.
        // If an array has more than one rank-specifiers, allocate memory to dimSizes and store there.

#if DEBUG
        //------------------------------------------------------------
        // EXPRARRINIT.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ArgumentsExpr != null)
            {
                sb.AppendFormat("ArgumentsExpr   : {0}\n", ArgumentsExpr.ExprID);
            }
            else
            {
                sb.Append("ArgumentsExpr   :\n");
            }
            sb.Append("DimSizes    :");
            if (DimSizes != null)
            {
                for (int i = 0; i < DimSizes.Count; ++i)
                {
                    sb.AppendFormat(" {0}", DimSizes[i]);
                }
            }
            sb.Append("\n");
        }
#endif
    }

    //======================================================================
    // class EXPRNOOP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRNOOP : EXPRSTMT
    {
    }

    //======================================================================
    // class EXPRDEBUGNOOP
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRDEBUGNOOP : EXPRSTMT
    {
    }

    //======================================================================
    // class EXPRANONMETH
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRANONMETH : EXPR
    {
        /// <summary>
        /// ILGEN should never see these guys
        /// They are transformed into something like this:
        ///     ((cachedDelegate == null)
        ///         ? (cachedDelegate = new DelegateType($locals.Method))
        ///         : cachedDelegate)
        /// </summary>
        internal AnonMethInfo AnonymousMethodInfo = null;   // * pInfo;

        internal bool Compiled
        {
            get { return (AnonymousMethodInfo != null && AnonymousMethodInfo.Compiled); }
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRANONMETH.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (AnonymousMethodInfo != null)
            {
                if (AnonymousMethodInfo.BodyBlockExpr != null)
                {
                    sb.AppendFormat(
                        "BodyBlockExpr : No.{0}\n",
                        AnonymousMethodInfo.BodyBlockExpr.ExprID);
                }
            }
        }
#endif
    }

    //======================================================================
    // class EXPRLAMBDAEXPR
    //
    /// <summary>
    /// <para>(CS3) Derives from EXPRANONMETH.</para>
    /// </summary>
    //======================================================================
    internal class EXPRLAMBDAEXPR : EXPRANONMETH
    {
        internal List<LOCVARSYM> ParameterList = null;

        internal bool ParameterTypesAreImplicit = false;

        //internal bool BodyIsBlock = false;

#if DEBUG
        //------------------------------------------------------------
        // EXPRLAMBDAEXPR.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ParameterList != null && ParameterList.Count > 0)
            {
                sb.Append("Parameters  : ");
                for (int i = 0; i < ParameterList.Count; ++i)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.AppendFormat("No.{0}", ParameterList[i].SymID);
                }
                sb.Append("\n");
            }
            else
            {
                sb.Append("Parameters  :\n");
            }
        }
#endif
    }

    //======================================================================
    // class EXPRSYSTEMTYPE
    //
    /// <summary>
    /// (CS3) For System.Type instances.
    /// </summary>
    //======================================================================
    internal class EXPRSYSTEMTYPE : EXPR
    {
        internal Type Type = null;

#if DEBUG
        //------------------------------------------------------------
        // EXPRSYSTEMTYPE.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat(
                "Type : {0}\n",
                this.Type != null ? this.Type.Name : null);
        }
#endif
    }

    //======================================================================
    // class EXPRFIELDINFO
    //
    /// <summary>
    /// (CS3)
    /// </summary>
    //======================================================================
    internal class EXPRFIELDINFO : EXPR
    {
        internal FieldInfo FieldInfo = null;

#if DEBUG
        //------------------------------------------------------------
        // EXPRFIELDINFO.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat(
                "FieldInfo : {0}\n",
                this.FieldInfo != null ? this.FieldInfo.Name : null);
        }
#endif
    }

    //======================================================================
    // class EXPRMETHODINFO
    //
    /// <summary>
    /// (CS3)
    /// </summary>
    //======================================================================
    internal class EXPRMETHODINFO : EXPR
    {
        internal MethodInfo MethodInfo = null;
        internal Type DeclaringType = null;

#if DEBUG
        //------------------------------------------------------------
        // EXPRMETHODINFO.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat(
                "MethodInfo    : {0}\n",
                this.MethodInfo != null ? this.MethodInfo.Name : null);
            sb.AppendFormat(
                "DeclaringType : {0}\n",
                this.DeclaringType != null ? this.DeclaringType.Name : null);
        }
#endif
    }

    //======================================================================
    // class EXPRCONSTRUCTORINFO
    //
    /// <summary>
    /// (CS3)
    /// </summary>
    //======================================================================
    internal class EXPRCONSTRUCTORINFO : EXPR
    {
        internal ConstructorInfo ConstructorInfo = null;

#if DEBUG
        //------------------------------------------------------------
        // EXPRCONSTRUCTORINFO.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat(
                "ConstructorInfo : {0}\n",
                this.ConstructorInfo != null ? this.ConstructorInfo.Name : null);
        }
#endif
    }

    //======================================================================
    // class EXPRNOP
    //
    /// <summary>
    /// (CS3)
    /// </summary>
    //======================================================================
    internal class EXPRNOP : EXPR
    {
    }

    //======================================================================
    // class EXPRDELIM
    //
    /// <summary>
    /// <para>Derives from EXPRSTMT.</para>
    /// </summary>
    //======================================================================
    internal class EXPRDELIM : EXPRSTMT
    {
        internal DELIMKIND DelimKind;           // delim;
        internal BitSet BitSet = new BitSet();  // bset;

#if DEBUG
        //------------------------------------------------------------
        // EXPRDELIM.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat("DelimKind   : {0}\n", DelimKind);
            sb.AppendFormat("BitSet      : {0}\n", BitSet.Debug());
        }
#endif
    }

    //======================================================================
    // class EXPRERROR
    //
    /// <summary></summary>
    //======================================================================
    internal class EXPRERROR : EXPR
    {
        internal string ErrorString = null; // * errorString;

#if DEBUG
        //------------------------------------------------------------
        // EXPRERROR.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat("ErrorString : {0}\n", ErrorString);
        }
#endif
    }

    //======================================================================
    // class EXPRRUNTIMEBINDEDMEMBER
    //
    /// <summary>
    /// (CS4) Represents the runtime binded expressions.
    /// </summary>
    //======================================================================
    internal class EXPRRUNTIMEBINDEDMEMBER : EXPR
    {
        internal EXPR ObjectExpr = null;
        internal CSharpBinderFlags BindFlags = CSharpBinderFlags.None;

        internal string MemberName = null;
        internal TypeArray TypeArguments = null;
        internal EXPR ArgumentsExpr = null;

        internal AGGTYPESYM ContextSym = null;

        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDMEMBER.GetObjectSym
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetObjectSym()
        {
            if (ObjectExpr == null)
            {
                return null;
            }

            switch (ObjectExpr.Kind)
            {
                case EXPRKIND.LOCAL:
                    return (ObjectExpr as EXPRLOCAL).LocVarSym;

                case EXPRKIND.FIELD:
                    FieldWithType fwt = (ObjectExpr as EXPRFIELD).FieldWithType;
                    return (fwt != null ? fwt.FieldSym : null);

                default:
                    break;
            }
            return null;
        }

        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDMEMBER.ChangeTypeSym
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void ChangeTypeSym(TYPESYM typeSym)
        {
            //this.TypeSym = typeSym;

            if (ObjectExpr == null)
            {
                return;
            }

            switch (ObjectExpr.Kind)
            {
                case EXPRKIND.LOCAL:
                    {
                        EXPRLOCAL locExpr = ObjectExpr as EXPRLOCAL;
                        DebugUtil.Assert(locExpr != null);
                        locExpr.TypeSym = typeSym;

                        LOCVARSYM locSym = locExpr.LocVarSym;
                        if (locSym != null)
                        {
                            locSym.TypeSym = typeSym;
                        }
                    }
                    break;

                case EXPRKIND.FIELD:
                    {
                        EXPRFIELD fieldExpr = ObjectExpr as EXPRFIELD;
                        DebugUtil.Assert(fieldExpr != null);
                        fieldExpr.TypeSym = typeSym;

                        FieldWithType fwt = fieldExpr.FieldWithType;
                        MEMBVARSYM fieldSym = (fwt != null ? fwt.FieldSym : null);
                        if (fieldSym != null)
                        {
                            fieldSym.TypeSym = typeSym;
                        }
                    }
                    break;

                default:
                    break;
            }
            return;
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDMEMBER.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            if (ObjectExpr != null)
            {
                sb.AppendFormat("ObjectExpr      : No.{0}\n", ObjectExpr.ExprID);
            }
            else
            {
            }

            sb.AppendFormat("BindFlags       : {0}\n", BindFlags);

            if (MemberName != null)
            {
                sb.AppendFormat("MemberName      : {0}\n", MemberName);
            }
            else
            {
            }

            sb.Append("TypeArguments   :");
            if (TypeArguments != null)
            {
                for (int i = 0; i < TypeArguments.Count; ++i)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    sb.AppendFormat(" No.{0}", TypeArguments[i].SymID);
                }
            }
            sb.Append("\n");

            if (ArgumentsExpr != null)
            {
                sb.AppendFormat("ArgumentsExpr   : No.{0}\n", ArgumentsExpr.ExprID);
            }
            else
            {
            }

            if (ContextSym != null)
            {
                sb.AppendFormat("ContextSym      : No.{0}\n", ContextSym.SymID);
            }
            else
            {
            }
        }
#endif
    }

    //======================================================================
    // class EXPRRUNTIMEBINDEDUNAOP
    //
    /// <summary>
    /// (CS4) Represents the runtime binded unary operations.
    /// </summary>
    //======================================================================
    internal class EXPRRUNTIMEBINDEDUNAOP : EXPRBINOP
    {
        internal ExpressionType ExpressionType = (ExpressionType)0;

#if DEBUG
        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDUNAOP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);
            sb.AppendFormat("ExpressionType : {0}\n", ExpressionType);
        }
#endif
    }

    //======================================================================
    // class EXPRRUNTIMEBINDEDBINOP
    //
    /// <summary>
    /// (CS4) Represents the runtime binded binary operations.
    /// </summary>
    //======================================================================
    internal class EXPRRUNTIMEBINDEDBINOP : EXPRBINOP
    {
        internal ExpressionType ExpressionType = (ExpressionType)0;

#if DEBUG
        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDBINOP.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);
            sb.AppendFormat("ExpressionType : {0}\n", ExpressionType);
        }
#endif
    }

    //======================================================================
    // class EXPRRUNTIMEBINDEDINVOCATION
    //
    /// <summary>
    /// (CS4) Represents the runtime binded invocations.
    /// </summary>
    //======================================================================
    internal class EXPRRUNTIMEBINDEDINVOCATION : EXPRCALL
    {
        internal CSharpBinderFlags BindFlags = CSharpBinderFlags.None;
        internal string MemberName = null;
        internal TypeArray TypeArguments = null;
        internal AGGTYPESYM ContextSym = null;

        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDINVOCATION.Set
        //
        /// <summary></summary>
        /// <param name="memberExpr"></param>
        //------------------------------------------------------------
        internal void Set(EXPRRUNTIMEBINDEDMEMBER memberExpr)
        {
            DebugUtil.Assert(memberExpr != null);

            this.ObjectExpr = memberExpr.ObjectExpr;
            this.ArgumentsExpr = memberExpr.ArgumentsExpr;

            this.BindFlags = memberExpr.BindFlags;
            this.MemberName = memberExpr.MemberName;
            this.TypeArguments = memberExpr.TypeArguments;
            this.ContextSym = memberExpr.ContextSym;
        }

        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDINVOCATION.GetObjectSym
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetObjectSym()
        {
            if (ObjectExpr == null)
            {
                return null;
            }

            switch (ObjectExpr.Kind)
            {
                case EXPRKIND.LOCAL:
                    return (ObjectExpr as EXPRLOCAL).LocVarSym;

                case EXPRKIND.FIELD:
                    FieldWithType fwt = (ObjectExpr as EXPRFIELD).FieldWithType;
                    return (fwt != null ? fwt.FieldSym : null);

                default:
                    break;
            }
            return null;
        }

        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDINVOCATION.ChangeTypeSym
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void ChangeTypeSym(TYPESYM typeSym)
        {
            //this.TypeSym = typeSym;

            if (ObjectExpr == null)
            {
                return;
            }

            switch (ObjectExpr.Kind)
            {
                case EXPRKIND.LOCAL:
                    {
                        EXPRLOCAL locExpr = ObjectExpr as EXPRLOCAL;
                        DebugUtil.Assert(locExpr != null);
                        locExpr.TypeSym = typeSym;

                        LOCVARSYM locSym = locExpr.LocVarSym;
                        if (locSym != null)
                        {
                            locSym.TypeSym = typeSym;
                        }
                    }
                    break;

                case EXPRKIND.FIELD:
                    {
                        EXPRFIELD fieldExpr = ObjectExpr as EXPRFIELD;
                        DebugUtil.Assert(fieldExpr != null);
                        fieldExpr.TypeSym = typeSym;

                        FieldWithType fwt = fieldExpr.FieldWithType;
                        MEMBVARSYM fieldSym = (fwt != null ? fwt.FieldSym : null);
                        if (fieldSym != null)
                        {
                            fieldSym.TypeSym = typeSym;
                        }
                    }
                    break;

                default:
                    break;
            }
            return;
        }

#if DEBUG
        //------------------------------------------------------------
        // EXPRRUNTIMEBINDEDINVOCATION.Debug
        //------------------------------------------------------------
        internal override void Debug(StringBuilder sb)
        {
            base.Debug(sb);

            sb.AppendFormat("BindFlags       : {0}\n", BindFlags);

            if (MemberName != null)
            {
                sb.AppendFormat("MemberName      : {0}\n", MemberName);
            }
            else
            {
            }

            sb.Append("TypeArguments   :");
            if (TypeArguments != null)
            {
                for (int i = 0; i < TypeArguments.Count; ++i)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    sb.AppendFormat(" No.{0}", TypeArguments[i].SymID);
                }
            }
            sb.Append("\n");

            if (ContextSym != null)
            {
                sb.AppendFormat("ContextSym      : No.{0}\n", ContextSym.SymID);
            }
            else
            {
            }
        }
#endif
    }

    //#undef INCLUDE_IMPL
    //
    //// Casts from the base type:
    //#define EXPRDEF(k) \
    //    __forceinline EXPR ## k * EXPR::as ## k () {   \
    //        RETAILVERIFY(this == NULL || this->kind == EK_ ## k);  \
    //        return static_cast< EXPR ## k *>(this);     \
    //    }
    //#include "exprkind.h"
    //#undef EXPRDEF
    //
    //__forceinline int EXPR::isTrue(bool sense) 
    //{
    //    ASSERT((int)sense == 0 || (int)sense == 1);
    //    return this && kind == EK_CONSTANT && !(AsCONSTANT->getVal().iVal ^ (int)sense);
    //}
    //
    //__forceinline int EXPR::isFalse(bool sense) 
    //{
    //    ASSERT((int)sense == 0 || (int)sense == 1);
    //    return this && kind == EK_CONSTANT && (AsCONSTANT->getVal().iVal ^ (int)sense);
    //}
    //
    //__forceinline int EXPR::isTrueResult(bool sense)
    //{
    //    return isFalseResult(!sense);
    //}
    //
    //__forceinline int EXPR::isConstantResult()
    //{
    //    return EXPR::ConstNotConst != GetConstantResult();
    //}
    //
    //__forceinline int EXPR::ConstantMatchesSense(EXPR::CONSTRESKIND crk, bool sense)
    //{
    //    if (crk == EXPR::ConstNotConst) return false;
    //    return ((crk == EXPR::ConstTrue) == sense);
    //}
    //
    //__forceinline EXPR::CONSTRESKIND EXPR::GetConstantResult()
    //{
    //    if (!this)
    //        return EXPR::ConstNotConst;
    //
    //    for (EXPR * expr = this; ; ) {
    //        switch (expr->kind) {
    //        case EK_CONSTANT:
    //            return expr->AsCONSTANT->getVal().iVal ? EXPR::ConstTrue : EXPR::ConstFalse;
    //        case EK_SEQUENCE:
    //            expr = expr->AsBIN->p2;
    //            break;
    //        default:
    //            return EXPR::ConstNotConst;
    //        }
    //    }
    //}
    //
    //__forceinline int EXPR::isFalseResult(bool sense)
    //{
    //    ASSERT((int)sense == 0 || (int)sense == 1);
    //
    //    return ConstantMatchesSense(GetConstantResult(), !sense);
    //}
    //
    //__forceinline bool EXPR::isNull()
    //{
    //    return kind == EK_CONSTANT && (type->fundType() == FT_REF) && !AsCONSTANT->getSVal().strVal;
    //}
    //
    //__forceinline bool EXPR::isZero(bool fDefValue)
    //{
    //    if (kind == EK_CONSTANT) {
    //        switch( type->fundType() ) {
    //        case FT_I1: case FT_U1:
    //        case FT_I2: case FT_U2:
    //        case FT_I4: 
    //            return AsCONSTANT->getVal().iVal == 0;
    //        case FT_U4:
    //            return AsCONSTANT->getVal().uiVal == 0;
    //        case FT_I8: 
    //            return *AsCONSTANT->getVal().longVal == 0;
    //        case FT_U8:
    //            return *AsCONSTANT->getVal().ulongVal == 0;
    //        case FT_R4: case FT_R8:
    //            return *AsCONSTANT->getVal().doubleVal == 0.0;
    //        case FT_STRUCT: // Decimal
    //            {
    //                // if we are looking for the default value, then check the scale to make sure the value is 0M,
    //                // otherwise return true for any amount of zeros (i.e. 0.000M)
    //                DECIMAL *pdec = AsCONSTANT->getVal().decVal;
    //                return (DECIMAL_HI32(*pdec) == 0 && DECIMAL_MID32(*pdec) == 0 && DECIMAL_LO32(*pdec) == 0 && (!fDefValue || DECIMAL_SIGNSCALE(*pdec) == 0));
    //            }
    //        case FT_REF:
    //            return fDefValue && isNull();
    //        default:
    //            break;
    //        }
    //    }
    //    return false;
    //}
    //
    //
    //__forceinline int EXPR::getOffset()
    //{
    //    if (kind == EK_LOCAL) {
    //        ASSERT(!AsLOCAL->local->isConst);
    //        return AsLOCAL->local->slot.JbitDefAssg();
    //    } else if (kind == EK_FIELD) {
    //        return AsFIELD->offset;
    //    } else {
    //        return 0;
    //    }
    //};
    //
    //__forceinline EXPR * EXPR::getArgs()
    //{
    //    RETAILVERIFY(kind == EK_CALL || kind == EK_PROP || kind == EK_FIELD || kind == EK_ARRINDEX);
    //    if (kind == EK_FIELD)
    //        return NULL;
    //    ASSERT(offsetof(EXPRCALL, args) == offsetof(EXPRPROP, args));
    //    ASSERT(offsetof(EXPRCALL, args) == offsetof(EXPRBINOP, p2));
    //    return (static_cast<EXPRCALL*>(this))->args;
    //}
    //
    //__forceinline EXPR ** EXPR::getArgsPtr()
    //{
    //    RETAILVERIFY(kind == EK_CALL || kind == EK_PROP);
    //    ASSERT(offsetof(EXPRCALL, args) == offsetof(EXPRPROP, args));
    //    return &((static_cast<EXPRCALL*>(this))->args);
    //}
    //
    //__forceinline EXPR * EXPR::getObject()
    //{
    //    RETAILVERIFY(kind == EK_CALL || kind == EK_PROP || kind == EK_FIELD);
    //    ASSERT(offsetof(EXPRCALL, object) == offsetof(EXPRPROP, object));
    //    ASSERT(offsetof(EXPRCALL, object) == offsetof(EXPRFIELD, object));
    //    return (static_cast<EXPRCALL*>(this))->object;
    //}
    //
    //
    //
}


