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
// File: fncbind.h
//
// Defines the structure which contains information necessary to bind and generate
// code for a single function.
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
// File: fncpbind.cpp
//
// Routines for analyzing the bound body of a function
// ===========================================================================

//============================================================================
// RewriteTree.cs
//     (Separated from FncBind.cs)
//
// 2015/06/24 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC.RecurseAndRewriteExprTree
        //
        /// <summary>
        /// Recursively walks the expr tree, making type substitutions and
        /// rewriting the tree using the provided call-back function pointer
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="rwInfo"></param>
        //------------------------------------------------------------
        internal void RecurseAndRewriteExprTree(ref EXPR firstExpr, RewriteInfo rwInfo)
        {
            DebugUtil.Assert(rwInfo != null);

            if (firstExpr == null)
            {
                return;
            }

            EXPR expr = firstExpr;
            EXPR prevExpr = null;
            //bool updateFirstExpr = false;
            EXPR tempExpr = null;

        LRepeat:
            EXPRSTMT nextStmtExpr = null;
            if (expr.Kind < EXPRKIND.StmtLim)
            {
                // Process each in isolation.
                nextStmtExpr = (expr as EXPRSTMT).NextStatement;
                (expr as EXPRSTMT).NextStatement = null;

                // An unreachable switch means that the switch expression is a constant,
                // so at least one of the switch labels is reachable.
                // Thus we need to process the switch.
                if (!(expr as EXPRSTMT).Reachable() && expr.Kind != EXPRKIND.SWITCH)
                {
                    expr = nextStmtExpr;
                    if (expr == null)
                    {
                        return;
                    }
                    goto LRepeat;
                }
            }

            //--------------------------------------------------------
            // Call out to do 2 things:
            //  * transform this expr if needed
            //  * find out if we need to recurse into this expr (post-transform)
            //--------------------------------------------------------
            if (rwInfo.RewriteFunc(this, ref expr))
            {
                //----------------------------------------------------
                // Substitue all the types
                // NOTE: the order of type substitution is based on
                // the declaration order, so expr->type is last
                //----------------------------------------------------
                if (TypeArray.Size(classTypeVariablesForMethod) > 0)
                {
                    switch (expr.Kind)
                    {
                        default:
                            DebugUtil.Assert(expr.Kind > EXPRKIND.COUNT);
                            DebugUtil.Assert((expr.Flags & EXPRFLAG.BINOP) != 0);
                            goto WALK_TYPE_EXPRBINOP;
                        WALK_BASE_TYPE_EXPRSTMT:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRSTMT:
                            goto WALK_BASE_TYPE_EXPRSTMT;

                        case EXPRKIND.BINOP:
                            goto WALK_TYPE_EXPRBINOP;
                        WALK_BASE_TYPE_EXPRBINOP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRBINOP:
                            goto WALK_BASE_TYPE_EXPRBINOP;

                        case EXPRKIND.USERLOGOP:
                            goto WALK_TYPE_EXPRUSERLOGOP;
                        WALK_BASE_TYPE_EXPRUSERLOGOP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRUSERLOGOP:
                            goto WALK_BASE_TYPE_EXPRUSERLOGOP;

                        case EXPRKIND.DBLQMARK:
                            goto WALK_TYPE_EXPRDBLQMARK;
                        WALK_BASE_TYPE_EXPRDBLQMARK:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRDBLQMARK:
                            goto WALK_BASE_TYPE_EXPRDBLQMARK;

                        case EXPRKIND.TYPEOF:
                            goto WALK_TYPE_EXPRTYPEOF;
                        WALK_BASE_TYPE_EXPRTYPEOF:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRTYPEOF:
                            //(static_cast<EXPRTYPEOF*>(*expr)).sourceType = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRTYPEOF*>(*expr)).sourceType,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).PARENTSYM::asTYPESYM ();
                            // PARENTSYM does not have its own asTYPESYM(),
                            // so we have no occasion to call PARENTSYM::asTYPESYM () ?
                            (expr as EXPRTYPEOF).SourceTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRTYPEOF).SourceTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as TYPESYM;
                            goto WALK_BASE_TYPE_EXPRTYPEOF;

                        case EXPRKIND.SIZEOF:
                            goto WALK_TYPE_EXPRSIZEOF;
                        WALK_BASE_TYPE_EXPRSIZEOF:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRSIZEOF:
                            //(static_cast<EXPRSIZEOF*>(*expr)).sourceType = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRSIZEOF*>(*expr)).sourceType,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).PARENTSYM::asTYPESYM ();
                            (expr as EXPRSIZEOF).SourceTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRSIZEOF).SourceTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as TYPESYM;
                            goto WALK_BASE_TYPE_EXPRSIZEOF;

                        case EXPRKIND.CAST:
                            goto WALK_TYPE_EXPRCAST;
                        WALK_BASE_TYPE_EXPRCAST:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRCAST:
                            goto WALK_BASE_TYPE_EXPRCAST;

                        case EXPRKIND.ZEROINIT:
                            goto WALK_TYPE_EXPRZEROINIT;
                        WALK_BASE_TYPE_EXPRZEROINIT:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRZEROINIT:
                            goto WALK_BASE_TYPE_EXPRZEROINIT;

                        case EXPRKIND.BLOCK:
                            goto WALK_TYPE_EXPRBLOCK;
                        WALK_BASE_TYPE_EXPRBLOCK:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRBLOCK:
                            goto WALK_BASE_TYPE_EXPRBLOCK;

                        case EXPRKIND.STMTAS:
                            goto WALK_TYPE_EXPRSTMTAS;
                        WALK_BASE_TYPE_EXPRSTMTAS:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRSTMTAS:
                            goto WALK_BASE_TYPE_EXPRSTMTAS;

                        case EXPRKIND.MEMGRP:
                            goto WALK_TYPE_EXPRMEMGRP;
                        WALK_BASE_TYPE_EXPRMEMGRP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRMEMGRP:
                            //(static_cast<EXPRMEMGRP*>(*expr)).typeArgs = compiler().getBSymmgr().SubstTypeArray(
                            //    (static_cast<EXPRMEMGRP*>(*expr)).typeArgs,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars);
                            //(static_cast<EXPRMEMGRP*>(*expr)).typePar = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRMEMGRP*>(*expr)).typePar,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).PARENTSYM::asTYPESYM ();
                            //(static_cast<EXPRMEMGRP*>(*expr)).types = compiler().getBSymmgr().SubstTypeArray(
                            //    (static_cast<EXPRMEMGRP*>(*expr)).types,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars);
                            (expr as EXPRMEMGRP).TypeArguments = compiler.MainSymbolManager.SubstTypeArray(
                                (expr as EXPRMEMGRP).TypeArguments,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone);
                            (expr as EXPRMEMGRP).ParentTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRMEMGRP).ParentTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as TYPESYM;
                            (expr as EXPRMEMGRP).ContainingTypeArray = compiler.MainSymbolManager.SubstTypeArray(
                                (expr as EXPRMEMGRP).ContainingTypeArray,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone);
                            goto WALK_BASE_TYPE_EXPRMEMGRP;

                        case EXPRKIND.CALL:
                            goto WALK_TYPE_EXPRCALL;
                        WALK_BASE_TYPE_EXPRCALL:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRCALL:
                            //(static_cast<EXPRCALL*>(*expr)).mwi.typeArgs = compiler().getBSymmgr().SubstTypeArray(
                            //    (static_cast<EXPRCALL*>(*expr)).mwi.typeArgs,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars);
                            //(static_cast<EXPRCALL*>(*expr)).mwi.ats = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRCALL*>(*expr)).mwi.ats,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).asAGGTYPESYM();
                            (expr as EXPRCALL).MethodWithInst.TypeArguments = compiler.MainSymbolManager.SubstTypeArray(
                                (expr as EXPRCALL).MethodWithInst.TypeArguments,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone);
                            (expr as EXPRCALL).MethodWithInst.AggTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRCALL).MethodWithInst.AggTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
                            goto WALK_BASE_TYPE_EXPRCALL;

                        case EXPRKIND.PROP:
                            goto WALK_TYPE_EXPRPROP;
                        WALK_BASE_TYPE_EXPRPROP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRPROP:
                            //(static_cast<EXPRPROP*>(*expr)).pwtSlot.ats = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRPROP*>(*expr)).pwtSlot.ats,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).asAGGTYPESYM();
                            //(static_cast<EXPRPROP*>(*expr)).mwtGet.ats = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRPROP*>(*expr)).mwtGet.ats,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).asAGGTYPESYM();
                            //(static_cast<EXPRPROP*>(*expr)).mwtSet.ats = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRPROP*>(*expr)).mwtSet.ats,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).asAGGTYPESYM();
                            (expr as EXPRPROP).SlotPropWithType.AggTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRPROP).SlotPropWithType.AggTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
                            (expr as EXPRPROP).GetMethodWithType.AggTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRPROP).GetMethodWithType.AggTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
                            (expr as EXPRPROP).SetMethodWithType.AggTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRPROP).SetMethodWithType.AggTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
                            goto WALK_BASE_TYPE_EXPRPROP;

                        case EXPRKIND.FIELD:
                            goto WALK_TYPE_EXPRFIELD;
                        WALK_BASE_TYPE_EXPRFIELD:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRFIELD:
                            //(static_cast<EXPRFIELD*>(*expr)).fwt.ats = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRFIELD*>(*expr)).fwt.ats,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).asAGGTYPESYM();
                            (expr as EXPRFIELD).FieldWithType.AggTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRFIELD).FieldWithType.AggTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
                            goto WALK_BASE_TYPE_EXPRFIELD;

                        case EXPRKIND.EVENT:
                            goto WALK_TYPE_EXPREVENT;
                        WALK_BASE_TYPE_EXPREVENT:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPREVENT:
                            //(static_cast<EXPREVENT*>(*expr)).ewt.ats = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPREVENT*>(*expr)).ewt.ats,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).asAGGTYPESYM();
                            (expr as EXPREVENT).EventWithType.AggTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPREVENT).EventWithType.AggTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
                            goto WALK_BASE_TYPE_EXPREVENT;

                        case EXPRKIND.DECL:
                            goto WALK_TYPE_EXPRDECL;
                        WALK_BASE_TYPE_EXPRDECL:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRDECL:
                            goto WALK_BASE_TYPE_EXPRDECL;

                        case EXPRKIND.LOCAL:
                            goto WALK_TYPE_EXPRLOCAL;
                        WALK_BASE_TYPE_EXPRLOCAL:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRLOCAL:
                            goto WALK_BASE_TYPE_EXPRLOCAL;

                        case EXPRKIND.RETURN:
                            goto WALK_TYPE_EXPRRETURN;
                        WALK_BASE_TYPE_EXPRRETURN:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRRETURN:
                            goto WALK_BASE_TYPE_EXPRRETURN;

                        case EXPRKIND.THROW:
                            goto WALK_TYPE_EXPRTHROW;
                        WALK_BASE_TYPE_EXPRTHROW:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRTHROW:
                            goto WALK_BASE_TYPE_EXPRTHROW;

                        case EXPRKIND.CONSTANT:
                        case EXPRKIND.SYSTEMTYPE:   // CS3
                        case EXPRKIND.FIELDINFO:    // CS3
                        case EXPRKIND.METHODINFO:   // CS3
                        case EXPRKIND.CONSTRUCTORINFO:   // CS3
                            goto WALK_TYPE_EXPRCONSTANT;
                        WALK_BASE_TYPE_EXPRCONSTANT:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRCONSTANT:
                            goto WALK_BASE_TYPE_EXPRCONSTANT;

                        case EXPRKIND.CLASS:
                            goto WALK_TYPE_EXPRCLASS;
                        WALK_BASE_TYPE_EXPRCLASS:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRCLASS:
                            goto WALK_BASE_TYPE_EXPRCLASS;

                        case EXPRKIND.NSPACE:
                            goto WALK_TYPE_EXPRNSPACE;
                        WALK_BASE_TYPE_EXPRNSPACE:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRNSPACE:
                            goto WALK_BASE_TYPE_EXPRNSPACE;

                        case EXPRKIND.LABEL:
                            goto WALK_TYPE_EXPRLABEL;
                        WALK_BASE_TYPE_EXPRLABEL:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRLABEL:
                            goto WALK_BASE_TYPE_EXPRLABEL;

                        case EXPRKIND.GOTO:
                            goto WALK_TYPE_EXPRGOTO;
                        WALK_BASE_TYPE_EXPRGOTO:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRGOTO:
                            goto WALK_BASE_TYPE_EXPRGOTO;

                        case EXPRKIND.GOTOIF:
                            goto WALK_TYPE_EXPRGOTOIF;
                        WALK_BASE_TYPE_EXPRGOTOIF:
                            goto WALK_TYPE_EXPRGOTO;
                        WALK_TYPE_EXPRGOTOIF:
                            goto WALK_BASE_TYPE_EXPRGOTOIF;

                        case EXPRKIND.FUNCPTR:
                            goto WALK_TYPE_EXPRFUNCPTR;
                        WALK_BASE_TYPE_EXPRFUNCPTR:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRFUNCPTR:
                            //(static_cast<EXPRFUNCPTR*>(*expr)).mwi.typeArgs = compiler().getBSymmgr().SubstTypeArray(
                            //    (static_cast<EXPRFUNCPTR*>(*expr)).mwi.typeArgs,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars);
                            //(static_cast<EXPRFUNCPTR*>(*expr)).mwi.ats = compiler().getBSymmgr().SubstType(
                            //    (static_cast<EXPRFUNCPTR*>(*expr)).mwi.ats,
                            //    (TypeArray*)null,
                            //    taClsVarsForMethVars).asAGGTYPESYM();
                            (expr as EXPRFUNCPTR).MethWithInst.TypeArguments = compiler.MainSymbolManager.SubstTypeArray(
                                (expr as EXPRFUNCPTR).MethWithInst.TypeArguments,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone);
                            (expr as EXPRFUNCPTR).MethWithInst.AggTypeSym = compiler.MainSymbolManager.SubstType(
                                (expr as EXPRFUNCPTR).MethWithInst.AggTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
                            goto WALK_BASE_TYPE_EXPRFUNCPTR;

                        case EXPRKIND.SWITCH:
                            goto WALK_TYPE_EXPRSWITCH;
                        WALK_BASE_TYPE_EXPRSWITCH:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRSWITCH:
                            goto WALK_BASE_TYPE_EXPRSWITCH;

                        case EXPRKIND.HANDLER:
                            goto WALK_TYPE_EXPRHANDLER;
                        WALK_BASE_TYPE_EXPRHANDLER:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRHANDLER:
                            goto WALK_BASE_TYPE_EXPRHANDLER;

                        case EXPRKIND.TRY:
                            goto WALK_TYPE_EXPRTRY;
                        WALK_BASE_TYPE_EXPRTRY:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRTRY:
                            goto WALK_BASE_TYPE_EXPRTRY;

                        case EXPRKIND.SWITCHLABEL:
                            goto WALK_TYPE_EXPRSWITCHLABEL;
                        WALK_BASE_TYPE_EXPRSWITCHLABEL:
                            goto WALK_TYPE_EXPRLABEL;
                        WALK_TYPE_EXPRSWITCHLABEL:
                            goto WALK_BASE_TYPE_EXPRSWITCHLABEL;

                        case EXPRKIND.MULTIGET:
                            goto WALK_TYPE_EXPRMULTIGET;
                        WALK_BASE_TYPE_EXPRMULTIGET:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRMULTIGET:
                            goto WALK_BASE_TYPE_EXPRMULTIGET;

                        case EXPRKIND.MULTI:
                            goto WALK_TYPE_EXPRMULTI;
                        WALK_BASE_TYPE_EXPRMULTI:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRMULTI:
                            goto WALK_BASE_TYPE_EXPRMULTI;

                        case EXPRKIND.STTMP:
                            goto WALK_TYPE_EXPRSTTMP;
                        WALK_BASE_TYPE_EXPRSTTMP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRSTTMP:
                            goto WALK_BASE_TYPE_EXPRSTTMP;

                        case EXPRKIND.LDTMP:
                            goto WALK_TYPE_EXPRLDTMP;
                        WALK_BASE_TYPE_EXPRLDTMP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRLDTMP:
                            goto WALK_BASE_TYPE_EXPRLDTMP;

                        case EXPRKIND.FREETMP:
                            goto WALK_TYPE_EXPRFREETMP;
                        WALK_BASE_TYPE_EXPRFREETMP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRFREETMP:
                            goto WALK_BASE_TYPE_EXPRFREETMP;

                        case EXPRKIND.WRAP:
                            goto WALK_TYPE_EXPRWRAP;
                        WALK_BASE_TYPE_EXPRWRAP:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRWRAP:
                            goto WALK_BASE_TYPE_EXPRWRAP;

                        case EXPRKIND.CONCAT:
                            goto WALK_TYPE_EXPRCONCAT;
                        WALK_BASE_TYPE_EXPRCONCAT:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRCONCAT:
                            goto WALK_BASE_TYPE_EXPRCONCAT;

                        case EXPRKIND.ARRINIT:
                            goto WALK_TYPE_EXPRARRINIT;
                        WALK_BASE_TYPE_EXPRARRINIT:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRARRINIT:
                            goto WALK_BASE_TYPE_EXPRARRINIT;

                        case EXPRKIND.NOOP:
                            goto WALK_TYPE_EXPRNOOP;
                        WALK_BASE_TYPE_EXPRNOOP:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRNOOP:
                            goto WALK_BASE_TYPE_EXPRNOOP;

                        case EXPRKIND.DEBUGNOOP:
                            goto WALK_TYPE_EXPRDEBUGNOOP;
                        WALK_BASE_TYPE_EXPRDEBUGNOOP:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRDEBUGNOOP:
                            goto WALK_BASE_TYPE_EXPRDEBUGNOOP;

                        case EXPRKIND.ANONMETH:
                        case EXPRKIND.LAMBDAEXPR:   // CS3
                            goto WALK_TYPE_EXPRANONMETH;
                        WALK_BASE_TYPE_EXPRANONMETH:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRANONMETH:
                            goto WALK_BASE_TYPE_EXPRANONMETH;

                        case EXPRKIND.DELIM:
                            goto WALK_TYPE_EXPRDELIM;
                        WALK_BASE_TYPE_EXPRDELIM:
                            goto WALK_TYPE_EXPRSTMT;
                        WALK_TYPE_EXPRDELIM:
                            goto WALK_BASE_TYPE_EXPRDELIM;

                        case EXPRKIND.ERROR:
                            goto WALK_TYPE_EXPRERROR;
                        WALK_BASE_TYPE_EXPRERROR:
                            goto WALK_TYPE_EXPR;
                        WALK_TYPE_EXPRERROR:
                            goto WALK_BASE_TYPE_EXPRERROR;
                    } // switch (expr.Kind)

                WALK_TYPE_EXPR:
                    expr.TypeSym = compiler.MainSymbolManager.SubstType(
                        expr.TypeSym,
                        (TypeArray)null,
                        this.classTypeVariablesForMethod,
                        SubstTypeFlagsEnum.NormNone);
                } // if (TypeArray.Size(classTypeVariablesForMethod) > 0)

                //----------------------------------------------------
                // Recursively rewrite all the nested expr trees
                // NOTE: the order of type is based on the declaration order,
                // not the semmantic order
                //----------------------------------------------------
                switch (expr.Kind)
                {
                    default:
                        DebugUtil.Assert(expr.Kind > EXPRKIND.COUNT);
                        DebugUtil.Assert((expr.Flags & EXPRFLAG.BINOP) != 0);
                        goto WALK_EXPRBINOP;
                    WALK_BASE_EXPRSTMT:
                        goto WALK_EXPR;
                    WALK_EXPRSTMT:
                        goto WALK_BASE_EXPRSTMT;

                    case EXPRKIND.BINOP:
                        goto WALK_EXPRBINOP;
                    WALK_BASE_EXPRBINOP:
                        goto WALK_EXPR;
                    WALK_EXPRBINOP:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRBINOP*>(*expr)).p1, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRBINOP*>(*expr)).p2, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRBINOP).Operand1, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRBINOP).Operand2, rwInfo);
                    goto WALK_BASE_EXPRBINOP;

                    case EXPRKIND.USERLOGOP:
                        goto WALK_EXPRUSERLOGOP;
                    WALK_BASE_EXPRUSERLOGOP:
                        goto WALK_EXPR;
                    WALK_EXPRUSERLOGOP:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRUSERLOGOP*>(*expr)).opX, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRUSERLOGOP*>(*expr)).callTF, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRUSERLOGOP*>(*expr)).callOp, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRUSERLOGOP).CallOp, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRUSERLOGOP).CallTF, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRUSERLOGOP).CallOp, rwInfo);
                        goto WALK_BASE_EXPRUSERLOGOP;

                    case EXPRKIND.DBLQMARK:
                        goto WALK_EXPRDBLQMARK;
                    WALK_BASE_EXPRDBLQMARK:
                        goto WALK_EXPR; 
                    WALK_EXPRDBLQMARK:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRDBLQMARK*>(*expr)).exprTest, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRDBLQMARK*>(*expr)).exprConv, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRDBLQMARK*>(*expr)).exprElse, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRDBLQMARK).TestExpr, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRDBLQMARK).ConvertExpr, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRDBLQMARK).ElseExpr, rwInfo);
                        goto WALK_BASE_EXPRDBLQMARK;

                    case EXPRKIND.TYPEOF:
                        goto WALK_EXPRTYPEOF;
                    WALK_BASE_EXPRTYPEOF:
                        goto WALK_EXPR;
                    WALK_EXPRTYPEOF:
                        goto WALK_BASE_EXPRTYPEOF;

                    case EXPRKIND.SIZEOF:
                        goto WALK_EXPRSIZEOF;
                    WALK_BASE_EXPRSIZEOF:
                        goto WALK_EXPR;
                    WALK_EXPRSIZEOF:
                        goto WALK_BASE_EXPRSIZEOF;

                    case EXPRKIND.CAST:
                        goto WALK_EXPRCAST;
                    WALK_BASE_EXPRCAST:
                        goto WALK_EXPR;
                    WALK_EXPRCAST:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRCAST*>(*expr)).p1, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRCAST).Operand, rwInfo);
                        goto WALK_BASE_EXPRCAST;

                    case EXPRKIND.ZEROINIT:
                        goto WALK_EXPRZEROINIT;
                    WALK_BASE_EXPRZEROINIT:
                        goto WALK_EXPR;
                    WALK_EXPRZEROINIT:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRZEROINIT*>(*expr)).p1, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRZEROINIT).Operand, rwInfo);
                        goto WALK_BASE_EXPRZEROINIT;

                    case EXPRKIND.BLOCK:
                        goto WALK_EXPRBLOCK;
                    WALK_BASE_EXPRBLOCK:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRBLOCK:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRBLOCK*>(*expr)).statements, rwInfo);
                        tempExpr = (expr as EXPRBLOCK).StatementsExpr;
                        RecurseAndRewriteExprTree(ref tempExpr, rwInfo);
                        (expr as EXPRBLOCK).StatementsExpr = tempExpr as EXPRSTMT;
                        goto WALK_BASE_EXPRBLOCK;

                    case EXPRKIND.STMTAS:
                        goto WALK_EXPRSTMTAS;
                    WALK_BASE_EXPRSTMTAS:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRSTMTAS:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRSTMTAS*>(*expr)).expression, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRSTMTAS).Expr, rwInfo);
                        goto WALK_BASE_EXPRSTMTAS;

                    case EXPRKIND.MEMGRP:
                        goto WALK_EXPRMEMGRP;
                    WALK_BASE_EXPRMEMGRP:
                        goto WALK_EXPR;
                    WALK_EXPRMEMGRP:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRMEMGRP*>(*expr)).object, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRMEMGRP).ObjectExpr, rwInfo);
                        goto WALK_BASE_EXPRMEMGRP;

                    case EXPRKIND.CALL:
                        goto WALK_EXPRCALL;
                    WALK_BASE_EXPRCALL:
                        goto WALK_EXPR;
                    WALK_EXPRCALL:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRCALL*>(*expr)).object, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRCALL*>(*expr)).args, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRCALL).ObjectExpr, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRCALL).ArgumentsExpr, rwInfo);
                        goto WALK_BASE_EXPRCALL;

                    case EXPRKIND.PROP:
                        goto WALK_EXPRPROP;
                    WALK_BASE_EXPRPROP:
                        goto WALK_EXPR;
                    WALK_EXPRPROP:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRPROP*>(*expr)).object, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRPROP*>(*expr)).args, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRPROP).ObjectExpr, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRPROP).ArgumentsExpr, rwInfo);
                        goto WALK_BASE_EXPRPROP;

                    case EXPRKIND.FIELD:
                        goto WALK_EXPRFIELD;
                    WALK_BASE_EXPRFIELD:
                        goto WALK_EXPR;
                    WALK_EXPRFIELD:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRFIELD*>(*expr)).object, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRFIELD).ObjectExpr, rwInfo);
                        goto WALK_BASE_EXPRFIELD;

                    case EXPRKIND.EVENT:
                        goto WALK_EXPREVENT;
                    WALK_BASE_EXPREVENT:
                        goto WALK_EXPR;
                    WALK_EXPREVENT:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPREVENT*>(*expr)).object, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPREVENT).ObjectExpr, rwInfo);
                        goto WALK_BASE_EXPREVENT;

                    case EXPRKIND.DECL:
                        goto WALK_EXPRDECL;
                    WALK_BASE_EXPRDECL:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRDECL:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRDECL*>(*expr)).init, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRDECL).InitialExpr, rwInfo);
                        goto WALK_BASE_EXPRDECL;

                    case EXPRKIND.LOCAL:
                        goto WALK_EXPRLOCAL;
                    WALK_BASE_EXPRLOCAL:
                        goto WALK_EXPR;
                    WALK_EXPRLOCAL:
                        goto WALK_BASE_EXPRLOCAL;

                    case EXPRKIND.RETURN:
                        goto WALK_EXPRRETURN;
                    WALK_BASE_EXPRRETURN:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRRETURN:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRRETURN*>(*expr)).object, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRRETURN).ObjectExpr, rwInfo);
                        goto WALK_BASE_EXPRRETURN;

                    case EXPRKIND.THROW:
                        goto WALK_EXPRTHROW;
                    WALK_BASE_EXPRTHROW:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRTHROW:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRTHROW*>(*expr)).object, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRTHROW).ObjectExpr, rwInfo);
                        goto WALK_BASE_EXPRTHROW;

                    case EXPRKIND.CONSTANT:
                    case EXPRKIND.SYSTEMTYPE:   // CS3
                    case EXPRKIND.FIELDINFO:    // CS3
                    case EXPRKIND.METHODINFO:   // CS3
                    case EXPRKIND.CONSTRUCTORINFO:   // CS3
                        goto WALK_EXPRCONSTANT;
                    WALK_BASE_EXPRCONSTANT:
                        goto WALK_EXPR;
                    WALK_EXPRCONSTANT:
                        goto WALK_BASE_EXPRCONSTANT;

                    case EXPRKIND.CLASS:
                        goto WALK_EXPRCLASS;
                    WALK_BASE_EXPRCLASS:
                        goto WALK_EXPR;
                    WALK_EXPRCLASS:
                        goto WALK_BASE_EXPRCLASS;

                    case EXPRKIND.NSPACE:
                        goto WALK_EXPRNSPACE;
                    WALK_BASE_EXPRNSPACE:
                        goto WALK_EXPR;
                    WALK_EXPRNSPACE:
                        goto WALK_BASE_EXPRNSPACE;

                    case EXPRKIND.LABEL:
                        goto WALK_EXPRLABEL;
                    WALK_BASE_EXPRLABEL:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRLABEL:
                        goto WALK_BASE_EXPRLABEL;

                    case EXPRKIND.GOTO:
                        goto WALK_EXPRGOTO;
                    WALK_BASE_EXPRGOTO:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRGOTO:
                        goto WALK_BASE_EXPRGOTO;

                    case EXPRKIND.GOTOIF:
                        goto WALK_EXPRGOTOIF;
                    WALK_BASE_EXPRGOTOIF:
                        goto WALK_EXPRGOTO;
                    WALK_EXPRGOTOIF:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRGOTOIF*>(*expr)).condition, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRGOTOIF).ConditionExpr, rwInfo);
                        goto WALK_BASE_EXPRGOTOIF;

                    case EXPRKIND.FUNCPTR:
                        goto WALK_EXPRFUNCPTR;
                    WALK_BASE_EXPRFUNCPTR:
                        goto WALK_EXPR;
                    WALK_EXPRFUNCPTR:
                        goto WALK_BASE_EXPRFUNCPTR;

                    case EXPRKIND.SWITCH:
                        goto WALK_EXPRSWITCH;
                    WALK_BASE_EXPRSWITCH:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRSWITCH:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRSWITCH*>(*expr)).arg, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRSWITCH*>(*expr)).bodies, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRSWITCH).ArgumentExpr, rwInfo);
                        tempExpr = (expr as EXPRSWITCH).BodiesExpr;
                        RecurseAndRewriteExprTree(ref tempExpr, rwInfo);
                        (expr as EXPRSWITCH).BodiesExpr = tempExpr as EXPRSWITCHLABEL;
                        goto WALK_BASE_EXPRSWITCH;

                    case EXPRKIND.HANDLER:
                        goto WALK_EXPRHANDLER;
                    WALK_BASE_EXPRHANDLER:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRHANDLER:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRHANDLER*>(*expr)).handlerBlock, rwInfo);
                        tempExpr = (expr as EXPRHANDLER).HandlerBlock;
                        RecurseAndRewriteExprTree(ref tempExpr, rwInfo);
                        (expr as EXPRHANDLER).HandlerBlock = tempExpr as EXPRBLOCK;
                        goto WALK_BASE_EXPRHANDLER;

                    case EXPRKIND.TRY :
                        goto WALK_EXPRTRY;
                    WALK_BASE_EXPRTRY :
                        goto WALK_EXPRSTMT;
                    WALK_EXPRTRY:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRTRY*>(*expr)).tryblock, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRTRY*>(*expr)).handlers, rwInfo);
                        tempExpr = (expr as EXPRTRY).TryBlockExpr;
                        RecurseAndRewriteExprTree(ref tempExpr, rwInfo);
                        (expr as EXPRTRY).TryBlockExpr = tempExpr as EXPRBLOCK;
                        tempExpr = (expr as EXPRTRY).HandlersExpr;
                        RecurseAndRewriteExprTree(ref tempExpr, rwInfo);
                        (expr as EXPRTRY).HandlersExpr = tempExpr as EXPRSTMT;
                        goto WALK_BASE_EXPRTRY;

                    case EXPRKIND.SWITCHLABEL:
                        goto WALK_EXPRSWITCHLABEL;
                    WALK_BASE_EXPRSWITCHLABEL:
                        goto WALK_EXPRLABEL;
                    WALK_EXPRSWITCHLABEL:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRSWITCHLABEL*>(*expr)).key, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRSWITCHLABEL*>(*expr)).statements, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRSWITCHLABEL).KeyExpr, rwInfo);
                        tempExpr = (expr as EXPRSWITCHLABEL).StatementsExpr;
                        RecurseAndRewriteExprTree(ref tempExpr, rwInfo);
                        (expr as EXPRSWITCHLABEL).StatementsExpr = tempExpr as EXPRSTMT;
                        goto WALK_BASE_EXPRSWITCHLABEL;

                    case EXPRKIND.MULTIGET:
                        goto WALK_EXPRMULTIGET;
                    WALK_BASE_EXPRMULTIGET:
                        goto WALK_EXPR;
                    WALK_EXPRMULTIGET:
                        goto WALK_BASE_EXPRMULTIGET;

                    case EXPRKIND.MULTI:
                        goto WALK_EXPRMULTI;
                    WALK_BASE_EXPRMULTI:
                        goto WALK_EXPR;
                    WALK_EXPRMULTI:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRMULTI*>(*expr)).left, rwInfo);
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRMULTI*>(*expr)).op, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRMULTI).LeftExpr, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRMULTI).OperandExpr, rwInfo);
                        goto WALK_BASE_EXPRMULTI;

                    case EXPRKIND.STTMP:
                        goto WALK_EXPRSTTMP;
                    WALK_BASE_EXPRSTTMP:
                        goto WALK_EXPR;
                    WALK_EXPRSTTMP:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRSTTMP*>(*expr)).src, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRSTTMP).SourceExpr, rwInfo);
                        goto WALK_BASE_EXPRSTTMP;

                    case EXPRKIND.LDTMP:
                        goto WALK_EXPRLDTMP;
                    WALK_BASE_EXPRLDTMP:
                        goto WALK_EXPR;
                    WALK_EXPRLDTMP:
                        goto WALK_BASE_EXPRLDTMP;

                    case EXPRKIND.FREETMP:
                        goto WALK_EXPRFREETMP;
                    WALK_BASE_EXPRFREETMP:
                        goto WALK_EXPR;
                    WALK_EXPRFREETMP:
                        goto WALK_BASE_EXPRFREETMP;

                    case EXPRKIND.WRAP:
                        goto WALK_EXPRWRAP;
                    WALK_BASE_EXPRWRAP:
                        goto WALK_EXPR;
                    WALK_EXPRWRAP:
                        goto WALK_BASE_EXPRWRAP;

                    case EXPRKIND.CONCAT:
                        goto WALK_EXPRCONCAT;
                    WALK_BASE_EXPRCONCAT:
                        goto WALK_EXPR;
                    WALK_EXPRCONCAT:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRCONCAT*>(*expr)).list, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRCONCAT).List, rwInfo);
                        goto WALK_BASE_EXPRCONCAT;

                    case EXPRKIND.ARRINIT:
                        goto WALK_EXPRARRINIT;
                    WALK_BASE_EXPRARRINIT:
                        goto WALK_EXPR;
                    WALK_EXPRARRINIT:
                        //RecurseAndRewriteExprTree((EXPR **)&(static_cast<EXPRARRINIT*>(*expr)).args, rwInfo);
                        RecurseAndRewriteExprTree(ref (expr as EXPRARRINIT).ArgumentsExpr, rwInfo);
                        goto WALK_BASE_EXPRARRINIT;

                    case EXPRKIND.NOOP:
                        goto WALK_EXPRNOOP;
                    WALK_BASE_EXPRNOOP:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRNOOP:
                        goto WALK_BASE_EXPRNOOP;

                    case EXPRKIND.DEBUGNOOP:
                        goto WALK_EXPRDEBUGNOOP;
                    WALK_BASE_EXPRDEBUGNOOP:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRDEBUGNOOP:
                        goto WALK_BASE_EXPRDEBUGNOOP;

                    case EXPRKIND.ANONMETH:
                    case EXPRKIND.LAMBDAEXPR:   // CS3
                        goto WALK_EXPRANONMETH;
                    WALK_BASE_EXPRANONMETH:
                        goto WALK_EXPR;
                    WALK_EXPRANONMETH:
                        goto WALK_BASE_EXPRANONMETH;

                    case EXPRKIND.DELIM:
                        goto WALK_EXPRDELIM;
                    WALK_BASE_EXPRDELIM:
                        goto WALK_EXPRSTMT;
                    WALK_EXPRDELIM:
                        goto WALK_BASE_EXPRDELIM;

                    case EXPRKIND.ERROR:
                        goto WALK_EXPRERROR;
                    WALK_BASE_EXPRERROR:
                        goto WALK_EXPR;
                    WALK_EXPRERROR:
                        goto WALK_BASE_EXPRERROR;
                } // switch (expr.Kind)

            WALK_EXPR:
                // no nested exprs in EXPR node
                ;
            } // if (rwInfo.RewriteFunc(this, ref expr))

            //if (!updateFirstExpr)
            if (prevExpr == null)
            {
                firstExpr = expr;
                //updateFirstExpr = true;
            }
            else
            {
                EXPRSTMT stmt = prevExpr as EXPRSTMT;
                if (stmt != null)
                {
                    stmt.NextStatement = expr as EXPRSTMT;
                }
            }

            if (nextStmtExpr != null)
            {
                //while (*pexpr)
                //    pexpr = (EXPR**)&(*pexpr)->asSTMT()->stmtNext;
                //*pexpr = stmtNext;
                //goto LRepeat;

                while ((expr as EXPRSTMT).NextStatement != null)
                {
                    expr = (expr as EXPRSTMT).NextStatement;
                }
                (expr as EXPRSTMT).NextStatement = nextStmtExpr;
                prevExpr = expr;
                expr = nextStmtExpr;
                goto LRepeat;
            }
        }
    }
}
