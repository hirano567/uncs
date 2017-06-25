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
// Iterator.cs
//     (Separated from FncBind.cs)
//
// 2015/07/05 (hirano567@hotmail.co.jp)
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
    // enum IteratorStatesEnum
    //
    /// <summary>
    /// <para>(CSharp\SCComp\Iterator.cs)</para>
    /// </summary>
    //======================================================================
    internal enum IteratorStatesEnum : int
    {
        /// <summary>
        /// -2
        /// </summary>
        UnusedIEnumerable = -2,

        /// <summary>
        /// -1
        /// </summary>
        RunningIEnumerator = -1,

        /// <summary>
        /// 0
        /// </summary>
        NotStartedIEnumerator = 0,

        /// <summary>
        /// 1
        /// </summary>
        FirstUnusedState = 1,
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //--------------------------------------------------
        // FUNCBREC.RewriteMoveNext
        //
        /// <summary>
        /// rewrite the Iterator method as a valid MoveNext method
        /// This should never report any errors.
        /// It will create fields for all locals (and emit all parameters for aggregate)
        /// </summary>
        /// <param name="outerMethodSym"></param>
        /// <param name="iteratorExpr"></param>
        /// <param name="methInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR RewriteMoveNext(METHSYM outerMethodSym, EXPR iteratorExpr, METHINFO methInfo)
        {
            DebugUtil.Assert(methInfo.IteratorInfo != null);
            Compiler.SetLocation(COMPILER.STAGE.TRANSFORM);

            // Fix the METHINFO to reflect the real MoveNext method
            this.methodInfo = methInfo;

            IteratorRewriteInfo rewriteInfo = new IteratorRewriteInfo();
            string tempName = null;
            AGGSYM saveParentAggSym = this.parentAggSym;
            DECLSYM saveParentDeclSym = this.parentDeclSym;

            this.methodSym = methInfo.MethodSym;
            this.localCount = 0;

            // We need to blow away the locals from the current method and move them to the class
            // this also does parameters
            this.innermostCatchScopeSym
                = this.innermostTryScopeSym
                = this.innermostFinallyScopeSym
                = this.currentScopeSym
                = this.OuterScopeSym
                = methInfo.OuterScopeSym;
            this.parentAggSym
                = rewriteInfo.InnerAggSym
                = methInfo.MethodSym.ClassSym;
            this.parentDeclSym = methInfo.MethodSym.ContainingDeclaration();
            this.classTypeVariablesForMethod = this.parentAggSym.GetThisType().TypeArguments;

            // Count up the iterator locals
            int cIteratorLocals = MoveLocalsToIter(
                methInfo.OuterScopeSym,
                methInfo.IteratorInfo.IteratorAggSym,
                0);
            compiler.Emitter.BeginIterator(cIteratorLocals);

            //re-point these guys
            MEMBVARSYM hoistedThisSym = compiler.MainSymbolManager.LookupAggMember(
                compiler.NameManager.GetPredefinedName(PREDEFNAME.HOISTEDTHIS),
                this.parentAggSym,
                SYMBMASK.MEMBVARSYM) as MEMBVARSYM;

            LOCVARSYM otherThisPointerSym = compiler.LocalSymbolManager.LookupLocalSym(
                compiler.NameManager.GetPredefinedName(PREDEFNAME.THIS),
                methInfo.OuterScopeSym,
                SYMBMASK.LOCVARSYM) as LOCVARSYM;

            while (otherThisPointerSym != null)
            {
                if (hoistedThisSym != null)
                {
                    otherThisPointerSym.MovedToFieldSym = hoistedThisSym;
                }
                else
                {
                    otherThisPointerSym.MovedToFieldSym = null;
                    hoistedThisSym = MoveLocalToField(
                        otherThisPointerSym,
                        methInfo.IteratorInfo.IteratorAggSym,
                        methInfo.IteratorInfo.IteratorAggSym.TypeVariables,
                        false,
                        null);
                }
                otherThisPointerSym = otherThisPointerSym.NextSameNameSym as LOCVARSYM;
            }

            InitThisPointer();
            rewriteInfo.LocalThisExpr = BindThisImplicit(null);

            tempName = compiler.NameManager.GetPredefinedName(PREDEFNAME.ITERSTATE);
            rewriteInfo.StateExpr = MakeFieldAccess(rewriteInfo.LocalThisExpr, tempName, 0);

            tempName = compiler.NameManager.GetPredefinedName(PREDEFNAME.ITERCURRENT);
            rewriteInfo.CurrentExpr = MakeFieldAccess(rewriteInfo.LocalThisExpr, tempName, 0);

            rewriteInfo.ReturnFailLabelExpr = MakeFreshLabel();
            rewriteInfo.NextState = IteratorStatesEnum.FirstUnusedState;
            rewriteInfo.InnermostFinallyState = IteratorStatesEnum.RunningIEnumerator;

            // Now re-write the EXPR tree to get a valid MoveNext (done recursively)
            EXPR bodyExpr = iteratorExpr;
            RecurseAndRewriteExprTree(ref bodyExpr, rewriteInfo);
            DebugUtil.Assert((bodyExpr as EXPRBLOCK).StatementsExpr != null);
            bodyExpr.Flags &= ~EXPRFLAG.NEEDSRET;

            EXPRSWITCHLABEL switchLabelExpr = MakeSwitchLabel(
                MakeIntConst((int)IteratorStatesEnum.NotStartedIEnumerator),
                this.currentScopeSym);
            EXPRLABEL startLabelExpr = MakeFreshLabel();

            switchLabelExpr.StatementsExpr = MakeGoto(
                null,
                startLabelExpr,
                EXPRFLAG.NODEBUGINFO);
            switchLabelExpr = MakeSwitchLabel(null, this.currentScopeSym);
            switchLabelExpr.StatementsExpr = MakeGoto(
                null,
                rewriteInfo.ReturnFailLabelExpr as EXPRLABEL,
                EXPRFLAG.NODEBUGINFO);

            // Now put together the switch block that will either jump after the last yield
            // or jump to the end
            //EXPRSTMT  prologueExpr = null;
            StatementListBuilder builder = new StatementListBuilder();  // (&prologueExpr);
            builder.Add(MakeIteratorSwitch(
                this.currentScopeSym,
                rewriteInfo,
                startLabelExpr,
                rewriteInfo.ReturnFailLabelExpr as EXPRLABEL));
            builder.Add(startLabelExpr);
            builder.Add(MakeAssignment(
                rewriteInfo.StateExpr,
                MakeIntConst((int)IteratorStatesEnum.RunningIEnumerator)));
            builder.Add(bodyExpr as EXPRBLOCK);
            builder.Add(rewriteInfo.ReturnFailLabelExpr);
            builder.Add(MakeReturn(
                this.currentScopeSym,
                MakeBoolConst(false),
                methInfo.IteratorInfo.DisposeMethodBodyExpr != null ? EXPRFLAG.ASLEAVE : 0));

            bodyExpr = NewExprBlock(null);
            //(bodyExpr as EXPRBLOCK).StatementsExpr = prologueExpr;
            (bodyExpr as EXPRBLOCK).StatementsExpr = builder.GetList();

            if (methInfo.IteratorInfo.DisposeMethodBodyExpr != null)
            {
                DebugUtil.Assert(methInfo.HasYieldAsLeave);

                // Make a try/fault block to call Dispose
                EXPRTRY tryExpr = NewExpr(null, EXPRKIND.TRY, null) as EXPRTRY;
                tryExpr.Flags |= (EXPRFLAG.ISFINALLY | EXPRFLAG.ISFAULT);
                tryExpr.TryBlockExpr = bodyExpr as EXPRBLOCK;

                EXPR callExpr = BindPredefMethToArgs(
                    null,
                    PREDEFNAME.DISPOSE,
                    GetRequiredPredefinedType(PREDEFTYPE.IDISPOSABLE),
                    BindThisImplicit(null),
                    null,
                    null);

                EXPRSTMT stmtExpr = MakeStmt(null, callExpr, EXPRFLAG.NODEBUGINFO);
                bodyExpr = NewExprBlock(null);
                (bodyExpr as EXPRBLOCK).StatementsExpr = stmtExpr;

                tryExpr.HandlersExpr = bodyExpr as EXPRBLOCK;
                bodyExpr = NewExprBlock(null);
                (bodyExpr as EXPRBLOCK).StatementsExpr = tryExpr;

                methInfo.HasReturnAsLeave = true;
            }
            this.methodSym = outerMethodSym;

            this.parentAggSym = saveParentAggSym;
            this.parentDeclSym = saveParentDeclSym;

            this.methodInfo = null;
            this.classTypeVariablesForMethod = null;

            return bodyExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeIterGet
        //
        /// <summary></summary>
        /// <param name="amNode"></param>
        /// <param name="methodInfo"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR MakeIterGet(
            BASENODE amNode,
            METHINFO methodInfo,
            AGGINFO classInfo)
        {
            DebugUtil.Assert(methodInfo.IteratorInfo != null);
            InitMethod(methodInfo, amNode, classInfo);
            DebugUtil.Assert(!methodInfo.MethodSym.ClassSym.IsFabricated);
            DeclareMethodParameters(methodInfo);

            EXPRBLOCK blockExpr = NewExprBlock(null);
            AGGTYPESYM iterAts = compiler.MainSymbolManager.SubstType(
                methodInfo.IteratorInfo.IteratorAggSym.GetThisType(),
                compiler.MainSymbolManager.ConcatParams(
                    methodInfo.MethodSym.ClassSym.AllTypeVariables,
                    methodInfo.MethodSym.TypeVariables),
                null,
                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;

            blockExpr.StatementsExpr = null;
            StatementListBuilder builder = new StatementListBuilder();	// (&blockExpr.statements);

            CreateNewScope();
            this.currentBlockExpr = blockExpr;
            this.currentBlockExpr.ScopeSym = this.currentScopeSym;

            LOCVARSYM localSym = DeclareVar(
                null,
                CreateSpecialName(SpecialNameKindEnum.IteratorInstance,null),
                iterAts,
                false);
            localSym.IsCompilerGenerated = true;
            localSym.LocSlotInfo.HasInit = true;
            localSym.LocSlotInfo.IsReferenced = true;
            localSym.LocSlotInfo.IsUsed = true;

            EXPR localExpr = BindToLocal(null, localSym, BindFlagsEnum.MemberSet);
            EXPR ctorExpr = CreateConstructorCall(
                this.treeNode,
                null,
                iterAts,
                null,
                MakeIntConst(methodInfo.IteratorInfo.IsEnumerable ?
                    (int)IteratorStatesEnum.UnusedIEnumerable :
                    (int)IteratorStatesEnum.NotStartedIEnumerator),
                MemLookFlagsEnum.NewObj);
            builder.Add(MakeAssignment(localExpr, ctorExpr));
            localExpr = BindToLocal(null, localSym, BindFlagsEnum.RValueRequired);

            // Now we need to pass in all the parameters

            for (SYM child = this.OuterScopeSym.FirstChildSym; child != null; child = child.NextSym)
            {
                if (child.IsLOCVARSYM)
                {
                    DebugUtil.Assert((child as LOCVARSYM).LocSlotInfo.IsParameter);
                    string name = (child as LOCVARSYM).IsThis ?
                        compiler.NameManager.GetPredefinedName(PREDEFNAME.HOISTEDTHIS) :
                        child.Name;
                    EXPR lhsExpr = MakeFieldAccess(localExpr, name, EXPRFLAG.MEMBERSET | EXPRFLAG.LVALUE);

                    if (lhsExpr != null)
                    {
                        // Sometimes, they just aren't hoisted because they're not used
                        if ((lhsExpr as EXPRFIELD).FieldWithType.FieldSym.IsHoistedParameter)
                        {
                            DebugUtil.Assert(methodInfo.IteratorInfo.IsEnumerable);
                            (lhsExpr as EXPRFIELD).FieldWithType.Sym
                                = (lhsExpr as EXPRFIELD).FieldWithType.FieldSym.GetOriginalCopy();
                        }
                        else
                        {
                            // Don't need a copy of the this pointer because it can't change
                            DebugUtil.Assert(
                                child == this.thisPointerSym ||
                                !(child as LOCVARSYM).LocSlotInfo.IsUsed);
                        }

                        EXPR rhsExpr = BindToLocal(null, child as LOCVARSYM, BindFlagsEnum.RValueRequired);
                        builder.Add(MakeAssignment(lhsExpr, rhsExpr));
                    }
                }
            }

            EXPR castExpr = MustCastCore(localExpr, methodInfo.MethodSym.ReturnTypeSym, null, 0);
            builder.Add(MakeReturn(this.OuterScopeSym, castExpr,0));

            blockExpr.StatementsExpr = builder.GetList();
            CloseScope();

            return blockExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeIterGetEnumerator
        //
        /// <summary></summary>
        /// <param name="amNode"></param>
        /// <param name="methInfo"></param>
        /// <param name="classInfo"></param>
        /// <param name="getEnumeratorMethodSym"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR MakeIterGetEnumerator(
            BASENODE amNode,
            METHINFO methInfo,
            AGGINFO classInfo,
            ref METHSYM getEnumeratorMethodSym)	// METHSYM ** pmethGetEnumerator)
        {
            DebugUtil.Assert(methInfo.IteratorInfo != null);
            InitMethod(methInfo, amNode, classInfo);
            DebugUtil.Assert(methInfo.ParameterInfoCount == 0 && methInfo.MethodSym.ParameterTypes.Count == 0);

            EXPRBLOCK blockExpr = NewExprBlock(null);
            this.currentBlockExpr = blockExpr;

            StatementListBuilder builder = new StatementListBuilder();	// (&blockExpr.statements);

            if (getEnumeratorMethodSym == null)
            {
                // Save this for later
                getEnumeratorMethodSym = methInfo.MethodSym;
                CreateNewScope();
                this.currentBlockExpr.ScopeSym = this.currentScopeSym;
                AGGTYPESYM iterAts = methInfo.MethodSym.ClassSym.GetThisType();

                LOCVARSYM localSym = DeclareVar(
                    null,
                    CreateSpecialName(SpecialNameKindEnum.IteratorInstance, null),
                    iterAts,
                    false);
                localSym.IsCompilerGenerated = true;
                localSym.LocSlotInfo.HasInit = true;
                localSym.LocSlotInfo.IsReferenced = true;
                localSym.LocSlotInfo.IsUsed = true;

                EXPR localExpr = BindToLocal(null, localSym, BindFlagsEnum.MemberSet);
                EXPR argsExpr = null;
                //EXPR ** pexprArgLast = &argsExpr;
                EXPR lastArgExpr = null;
                NewList(
                    MakeFieldAccess(
                        BindThisImplicit(null),
                        compiler.NameManager.GetPredefinedName(PREDEFNAME.ITERSTATE),
                        EXPRFLAG.MEMBERSET | EXPRFLAG.LVALUE),
                    //&pexprArgLast);
                    ref argsExpr,
                    ref lastArgExpr);
                argsExpr.TypeSym = compiler.MainSymbolManager.GetParamModifier(argsExpr.TypeSym, false);
                NewList(
                    MakeIntConst((int)IteratorStatesEnum.NotStartedIEnumerator),
                    //&pexprArgLast);
                    ref argsExpr,
                    ref lastArgExpr);
                NewList(
                    MakeIntConst((int)IteratorStatesEnum.UnusedIEnumerable),
                    //&pexprArgLast);
                    ref argsExpr,
                    ref lastArgExpr);

                EXPR cmpExchExpr = BindPredefMethToArgs(
                    null,
                    PREDEFNAME.COMPAREEXCHANGE,
                    GetOptionalPredefinedType(PREDEFTYPE.INTERLOCKED),
                    null,
                    argsExpr,
                    null);
                EXPR compareExpr = NewExprBinop(
                    null,
                    EXPRKIND.NE,
                    GetRequiredPredefinedType(PREDEFTYPE.BOOL),
                    cmpExchExpr,
                    MakeIntConst((int)IteratorStatesEnum.UnusedIEnumerable));

                EXPRLABEL elseLabelExpr = MakeFreshLabel();
                EXPRLABEL endIfLabelExpr = MakeFreshLabel();
                builder.Add(MakeGotoIf(null, compareExpr, elseLabelExpr, true, 0));

                builder.Add(MakeAssignment(localExpr, BindThisImplicit(null)));
                builder.Add(MakeGoto(null, endIfLabelExpr, EXPRFLAG.NODEBUGINFO));
                builder.Add(elseLabelExpr);

                EXPR ctorCallExpr = CreateConstructorCall(
                    this.treeNode,
                    null,
                    iterAts,
                    null,
                    MakeIntConst((int)IteratorStatesEnum.NotStartedIEnumerator),
                    MemLookFlagsEnum.NewObj);
                builder.Add(MakeAssignment(localExpr, ctorCallExpr));

                localExpr = BindToLocal(null, localSym, BindFlagsEnum.RValueRequired);
                MEMBVARSYM hoistedThisSym = compiler.MainSymbolManager.LookupAggMember(
                    compiler.NameManager.GetPredefinedName(PREDEFNAME.HOISTEDTHIS),
                    iterAts.GetAggregate(),
                    SYMBMASK.MEMBVARSYM) as MEMBVARSYM;
                if (hoistedThisSym != null)
                {
                    EXPR lhsExpr = MakeFieldAccess(localExpr, hoistedThisSym, 0);
                    EXPR rhsExpr = MakeFieldAccess(BindThisImplicit(null), hoistedThisSym, 0);
                    builder.Add(MakeAssignment(lhsExpr, rhsExpr));
                }
                builder.Add(endIfLabelExpr);

                // Now we need to pass in all the parameters
                for (SYM child = this.parentAggSym.FirstChildSym; child != null; child = child.NextSym)
                {
                    if (child.IsMEMBVARSYM && (child as MEMBVARSYM).IsHoistedParameter)
                    {
                        EXPR lhsExpr = MakeFieldAccess(localExpr, child as MEMBVARSYM, 0);
                        EXPR rhsExpr = MakeFieldAccess(BindThisImplicit(null), (child as MEMBVARSYM).GetOriginalCopy(), 0);
                        builder.Add(MakeAssignment(lhsExpr, rhsExpr));
                    }
                }

                EXPR castExpr = MustCastCore(localExpr, methInfo.MethodSym.ReturnTypeSym, null, 0);
                builder.Add(MakeReturn(this.OuterScopeSym, castExpr, 0));
                CloseScope();
            }
            else
            {
                // We've alreayd code-gen'd one GetEnumerator, so just call it and cast the result
                EXPRCALL callExpr = NewExpr(null, EXPRKIND.CALL, getEnumeratorMethodSym.ReturnTypeSym) as EXPRCALL;
                callExpr.ObjectExpr = BindThisImplicit(null);
                callExpr.MethodWithInst.Set(getEnumeratorMethodSym, callExpr.ObjectExpr.TypeSym as AGGTYPESYM, null);
                callExpr.ArgumentsExpr = null;
                builder.Add(MakeReturn(
                    this.OuterScopeSym,
                    MustCastCore(callExpr, methInfo.MethodSym.ReturnTypeSym, null, 0),
                    0));
            }

            blockExpr.StatementsExpr = builder.GetList();
            return blockExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeIterCur
        //
        /// <summary></summary>
        /// <param name="amNode"></param>
        /// <param name="methodInfo"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR MakeIterCur(BASENODE amNode, METHINFO methodInfo, AGGINFO classInfo)
        {
            InitMethod(methodInfo, amNode, classInfo);
            DebugUtil.Assert(
                methodInfo.ParameterInfoCount == 0 &&
                methodInfo.MethodSym.ParameterTypes.Count == 0);

            string currentName = compiler.NameManager.GetPredefinedName(PREDEFNAME.ITERCURRENT);
            EXPRBLOCK blockExpr = NewExprBlock(null);
            EXPR valueExpr = MakeFieldAccess(BindThisImplicit(null), currentName, 0);
            valueExpr = MustCastCore(valueExpr, methodInfo.MethodSym.ReturnTypeSym, null, 0);
            blockExpr.StatementsExpr = MakeReturn(this.OuterScopeSym, valueExpr, 0);
            return blockExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeIterReset
        //
        /// <summary></summary>
        /// <param name="amNode"></param>
        /// <param name="methodInfo"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR MakeIterReset(BASENODE amNode, METHINFO methodInfo, AGGINFO classInfo)
        {
            InitMethod(methodInfo, amNode, classInfo);
            DebugUtil.Assert(
                methodInfo.ParameterInfoCount == 0 &&
                methodInfo.MethodSym.ParameterTypes.Count == 0);

            EXPRBLOCK blockExpr = NewExprBlock(null);
            AGGTYPESYM exceptionAts = GetOptionalPredefinedType(PREDEFTYPE.NOTSUPPORTEDEXCEPTION);
            blockExpr.StatementsExpr = NewExpr(EXPRKIND.THROW) as EXPRTHROW;
            if (exceptionAts != null)
            {
                (blockExpr.StatementsExpr as EXPRTHROW).ObjectExpr = CreateConstructorCall(
                    this.treeNode,
                    null,
                    exceptionAts,
                    null,
                    null,
                    MemLookFlagsEnum.NewObj);
            }
            blockExpr.Flags |= EXPRFLAG.NEEDSRET;
            return blockExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeIterDispose
        //
        /// <summary></summary>
        /// <param name="amNode"></param>
        /// <param name="methodInfo"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR MakeIterDispose(BASENODE amNode, METHINFO methodInfo, AGGINFO classInfo)
        {
            // Find the locals scope so we can stitch it into the new method
            // for debug methodInfo on locals
            SCOPESYM moveNextLocalsScopeSym = null;

            SYM sym = methodInfo.OuterScopeSym.FirstChildSym;
            // Clear out everything except SCOPESYMs
            //SYM ** pnextChild = (SYM**)&moveNextLocalsScopeSym;
            SYM currentChild = null;
            do
            {
                if (sym.IsSCOPESYM)
                {
                    //*pnextChild = sym;
                    //pnextChild = &sym.nextChild;
                    if (moveNextLocalsScopeSym == null)
                    {
                        moveNextLocalsScopeSym = sym as SCOPESYM;
                    }
                    if (currentChild != null)
                    {
                        currentChild.NextSym = sym;
                    }
                    currentChild = sym;
                }
                sym = sym.NextSym;
            } while (sym != null);
            //*pnextChild = null;
            if (currentChild != null)
            {
                currentChild.NextSym = null;
            }

            DebugUtil.Assert(moveNextLocalsScopeSym != null);
            DebugUtil.Assert(moveNextLocalsScopeSym.NestingOrder == 1);

            InitMethod(methodInfo, amNode, classInfo);
            DebugUtil.Assert(
                methodInfo.ParameterInfoCount == 0 &&
                methodInfo.MethodSym.ParameterTypes.Count == 0);

            EXPRBLOCK blockExpr = NewExprBlock(null);

            DebugUtil.Assert(
                (this.OuterScopeSym.FirstChildSym as LOCVARSYM).IsThis &&
                this.OuterScopeSym.FirstChildSym.NextSym == null);
            this.OuterScopeSym.FirstChildSym.NextSym
                = blockExpr.ScopeSym
                = moveNextLocalsScopeSym;
            blockExpr.StatementsExpr = methodInfo.IteratorInfo.DisposeMethodBodyExpr;
            blockExpr.Flags |= EXPRFLAG.NEEDSRET;

            FncBindUtil.EraseDebugInfo(moveNextLocalsScopeSym);

            return blockExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeIterCtor
        //
        /// <summary></summary>
        /// <param name="amNode"></param>
        /// <param name="methodInfo"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR MakeIterCtor(BASENODE amNode, METHINFO methodInfo, AGGINFO classInfo)
        {
            DebugUtil.Assert(methodInfo.MethodSym != null);
            InitMethod(methodInfo, amNode, classInfo);
            DeclareMethodParameters(methodInfo);

            EXPRBLOCK blockExpr = NewExprBlock(null);
            blockExpr.Flags |= EXPRFLAG.NEEDSRET;
            StatementListBuilder builder = new StatementListBuilder();  // (&blockExpr.statements);

            // can't call createBaseConstructorCall
            // because it sucks in the arguments from this.treeNode if pAMSym is a CTOR
            // It also does some unneeded checks, so we just put the important part here.
            builder.Add(MakeStmt(
                this.treeNode,
                CreateConstructorCall(
                    this.treeNode,
                    null,
                    this.parentAggSym.BaseClassSym,
                    BindThisImplicit(this.treeNode),
                    null,
                    MemLookFlagsEnum.BaseCall),
                EXPRFLAG.NODEBUGINFO));

            // Now we need to pass in all the parameters
            for (SYM child = this.OuterScopeSym.FirstChildSym; child != null; child = child.NextSym)
            {
                if (child.IsLOCVARSYM && child != this.thisPointerSym)
                {
                    DebugUtil.Assert((child as LOCVARSYM).LocSlotInfo.IsParameter);

                    EXPR lhsExpr = MakeFieldAccess(
                        BindThisImplicit(null),
                        child.Name,
                        EXPRFLAG.MEMBERSET | EXPRFLAG.LVALUE);

                    EXPR rhsExpr = BindToLocal(
                        null,
                        child as LOCVARSYM,
                        BindFlagsEnum.RValueRequired);
                    builder.Add(MakeAssignment(lhsExpr, rhsExpr));
                }
            }

            blockExpr.StatementsExpr = builder.GetList();
            return blockExpr;
        }


        //------------------------------------------------------------
        // FUNCBREC.RewriteIteratorFunc
        //
        /// <summary>
        /// Function called by RecurseAndRewriteExprTree to do the actual rewriting of important EXPRs
        /// for rewriting of iterator bodies (happens after anonymous methods)
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="rewriteInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool RewriteIteratorFunc(ref EXPR expr, IteratorRewriteInfo rewriteInfo)
        {
            StatementListBuilder builder = null;
            EXPRSWITCHLABEL switchLabelExpr;
            EXPR tempExpr = null;

            switch (expr.Kind)
            {
                case EXPRKIND.ANONMETH:
                case EXPRKIND.MEMGRP:
                case EXPRKIND.LAMBDAEXPR:
                    DebugUtil.Assert(false, "Illegal expr kind in transform stage!");
                    return false;

                case EXPRKIND.LOCAL:
                    EXPRLOCAL localExpr = expr as EXPRLOCAL;

                    // Parameters or locals in a scope with a (nested) yield return
                    // that are used or the this pointer have been hoisted and need to be rewritten

                    if ((localExpr.LocVarSym.LocSlotInfo.IsParameter ||
                        (localExpr.LocVarSym.DeclarationScope().ScopeFlags & SCOPEFLAGS.HASYIELDRETURN) != 0)
                        &&
                        (localExpr.LocVarSym.LocSlotInfo.IsUsed ||
                        ((localExpr.Flags & EXPRFLAG.IMPLICITTHIS) != 0 && localExpr.LocVarSym != this.thisPointerSym)))
                    {
                        // An anonymous method might have hoisted it already
                        if (localExpr.LocVarSym.MovedToFieldSym.ClassSym != this.methodInfo.IteratorInfo.IteratorAggSym)
                        {
                            expr = MakeFieldAccess(
                                rewriteInfo.LocalThisExpr,
                                localExpr.LocVarSym.MovedToFieldSym.Name,
                                (localExpr.Flags & EXPRFLAG.LVALUE) != 0 ? (EXPRFLAG.LVALUE | EXPRFLAG.MEMBERSET) : 0);
                        }
                        else
                        {
                            expr = MakeFieldAccess(
                                rewriteInfo.LocalThisExpr,
                                localExpr.LocVarSym.MovedToFieldSym,
                                (localExpr.Flags & EXPRFLAG.LVALUE) != 0 ? (EXPRFLAG.LVALUE | EXPRFLAG.MEMBERSET) : 0);
                        }
                        DebugUtil.Assert(expr != null);

                        if (localExpr.TypeSym.IsPARAMMODSYM)
                        {
                            expr.TypeSym = compiler.MainSymbolManager.GetParamModifier(
                                expr.TypeSym,
                                (localExpr.TypeSym as PARAMMODSYM).IsOut);
                        }
                        return false;
                    }
                    break;

                case EXPRKIND.RETURN:
                    EXPRRETURN returnExpr = expr as EXPRRETURN;
                    DebugUtil.Assert((returnExpr.Flags & EXPRFLAG.RETURNISYIELD) != 0);
                    //EXPRSTMT  firstStmtExpr = null;
                    builder = new StatementListBuilder();	//(&firstStmtExpr);

                    if (returnExpr.ObjectExpr != null)
                    {
                        RecurseAndRewriteExprTree(ref returnExpr.ObjectExpr, rewriteInfo);

                        EXPRCONSTANT newStateValueExpr = MakeIntConst((int)(rewriteInfo.NextState++));

                        builder.Add(MakeAssignment(
                            returnExpr.TreeNode,
                            rewriteInfo.CurrentExpr,
                            returnExpr.ObjectExpr));
                        builder.Add(MakeAssignment(rewriteInfo.StateExpr, newStateValueExpr));

                        // If any yield return is in a try-finally,
                        // then all the iterator code is put in a try-fault
                        // (to ensure the Dispose method is invoked,
                        // since the Dispose method contains the user's finally code).
                        builder.Add(MakeReturn(
                            this.OuterScopeSym,
                            MakeBoolConst(true),
                            this.methodInfo.HasYieldAsLeave ? EXPRFLAG.ASLEAVE : 0));

                        switchLabelExpr = MakeSwitchLabel(newStateValueExpr, this.OuterScopeSym);
                        if (!returnExpr.Reachable())
                        {
                            // This yield is not reachable, so don't make anything after it reachable
                            // by sending the switch to the failure case.
                            switchLabelExpr.StatementsExpr = MakeGoto(
                                null,
                                rewriteInfo.ReturnFailLabelExpr,
                                EXPRFLAG.NODEBUGINFO);
                        }
                        else
                        {
                            EXPRLABEL nextLabelExpr = MakeFreshLabel();
                            switchLabelExpr.StatementsExpr = MakeGoto(null, nextLabelExpr, EXPRFLAG.NODEBUGINFO);
                            builder.Add(nextLabelExpr);
                            this.currentScopeSym.ScopeFlags |= SCOPEFLAGS.HASYIELDRETURN;
                        }
                        builder.Add(MakeAssignment(
                            rewriteInfo.StateExpr,
                            MakeIntConst((int)rewriteInfo.InnermostFinallyState)));

                        if (this.currentScopeSym != this.OuterScopeSym)
                        {
                            // We're inside a finally, so create a switch label for the dispose
                            MakeSwitchLabel(newStateValueExpr, this.currentScopeSym);
                        }
                    }
                    else // if (returnExpr.ObjectExpr != null)
                    {
                        EXPRSTMT stmtExpr = MakeGoto(
                            expr.TreeNode,
                            rewriteInfo.ReturnFailLabelExpr,
                            EXPRFLAG.NODEBUGINFO);
                        if (this.currentScopeSym != this.OuterScopeSym)
                        {
                            stmtExpr.Flags |= EXPRFLAG.ASLEAVE;
                        }

                        if (rewriteInfo.InnermostFinallyState != IteratorStatesEnum.RunningIEnumerator)
                        {
                            // If we're inside a try/finally we need to call
                            // Dispose before returning
                            DebugUtil.Assert(this.currentScopeSym != this.OuterScopeSym);

                            EXPR callExpr = BindPredefMethToArgs(
                                expr.TreeNode,
                                PREDEFNAME.DISPOSE,
                                GetRequiredPredefinedType(PREDEFTYPE.IDISPOSABLE),
                                rewriteInfo.LocalThisExpr,
                                null,
                                null);

                            this.currentScopeSym.ScopeFlags |= SCOPEFLAGS.HASYIELDBREAK;
                            DebugUtil.Assert(callExpr.Kind == EXPRKIND.CALL);
                            builder.Add(MakeStmt(expr.TreeNode, callExpr, 0));
                        }
                        else
                        {
                            stmtExpr.Flags &= ~EXPRFLAG.NODEBUGINFO;
                        }
                        builder.Add(stmtExpr);
                    } // if (returnExpr.ObjectExpr != null)
                    //expr = firstStmtExpr;
                    expr = builder.GetList();
                    return false; // Do not recurse (we already handled the one case of returning a value)

                case EXPRKIND.TRY:
                    EXPRTRY tryExpr = expr as EXPRTRY;
                    // Save the outer finally state
                    IteratorStatesEnum outerFinallyState = rewriteInfo.InnermostFinallyState;

                    // and create a new state for the nested finally
                    //int innerFinallyState = rewriteInfo.iFinallyState = rewriteInfo.iNextState++;
                    IteratorStatesEnum innerFinallyState
                        = rewriteInfo.InnermostFinallyState
                        = rewriteInfo.NextState++;

                    SCOPESYM oldCurrentScopeSym = this.currentScopeSym;
                    CreateNewScope();
                    SCOPESYM finallyScopeSym = this.currentScopeSym;
                    this.currentScopeSym.ScopeFlags |= SCOPEFLAGS.SWITCHSCOPE;
                    EXPRSTMT prevStmtExpr = this.methodInfo.IteratorInfo.DisposeMethodBodyExpr;
                    this.methodInfo.IteratorInfo.DisposeMethodBodyExpr = null;

                    EXPRBLOCK tryBlockExpr = tryExpr.TryBlockExpr;
                    tempExpr = tryBlockExpr;
                    RecurseAndRewriteExprTree(ref tempExpr, rewriteInfo);
                    tryBlockExpr = tempExpr as EXPRBLOCK;
                    DebugUtil.Assert(tryBlockExpr.Kind == EXPRKIND.BLOCK);

                    // Now restore to the outer Finally state now that we're done with the try body
                    rewriteInfo.InnermostFinallyState = outerFinallyState;

                    EXPRSTMT handlersExpr = tryExpr.HandlersExpr;
                    tempExpr = handlersExpr;
                    RecurseAndRewriteExprTree(ref tempExpr, rewriteInfo);
                    handlersExpr = tempExpr as EXPRSTMT;

                    this.currentScopeSym = oldCurrentScopeSym;
                    if (this.currentScopeSym != this.OuterScopeSym) // Propigate out this flag
                    {
                        this.currentScopeSym.ScopeFlags |=
                            (finallyScopeSym.ScopeFlags & (SCOPEFLAGS.HASYIELDBREAK | SCOPEFLAGS.HASYIELDRETURN));
                    }

                    //if ((finallyScopeSym.ScopeFlags & (SCOPEFLAGS.HASYIELDBREAK | SCOPEFLAGS.HASYIELDRETURN))==0)
                    if ((finallyScopeSym.ScopeFlags & SCOPEFLAGS.HASYIELDRETURN) == 0)
                    {
                        // No yields in this try-block, so leave it alone;
                        this.methodInfo.IteratorInfo.DisposeMethodBodyExpr = prevStmtExpr;
                        tryExpr.TryBlockExpr = tryBlockExpr;
                        tryExpr.HandlersExpr = handlersExpr;
                    }
                    else
                    {
                        // There was a yield, so we need to save the finally block
                        //EXPRSTMT pList = null;
                        builder = new StatementListBuilder();	//(&pList);

                        // Set the $__state on entry of the try block (This stuff only goes into the MoveNext method)
                        EXPRCONSTANT tryStateExpr = MakeIntConst((int)innerFinallyState);
                        builder.Add(MakeAssignment(rewriteInfo.StateExpr, tryStateExpr));
                        builder.Add(tryBlockExpr.StatementsExpr);
                        //tryBlockExpr.StatementsExpr = pList;
                        tryBlockExpr.StatementsExpr = builder.GetList();
                        //pList = null;
                        //builder.Init(&pList);
                        builder.Clear();

                        // Reset it on entry of the finally block (This stuff goes into MoveNext and Dispose)
                        EXPRCONSTANT finallyStateExpr = MakeIntConst((int)outerFinallyState);
                        builder.Add(MakeAssignment(rewriteInfo.StateExpr, finallyStateExpr));
                        builder.Add((handlersExpr as EXPRBLOCK).StatementsExpr);
                        FncBindUtil.SetDisposeScopeFlag(handlersExpr as EXPRBLOCK);
                        //handlersExpr.asBLOCK().statements = pList;
                        (handlersExpr as EXPRBLOCK).StatementsExpr = builder.GetList();
                        //pList = null;
                        builder.Clear();

                        // Test to skip over the finally (This only goes into the Dispose method)
                        EXPRLABEL goIntoLabelExpr = MakeFreshLabel();
                        EXPRLABEL skipLabelExpr = MakeFreshLabel();
                        switchLabelExpr = MakeSwitchLabel(tryStateExpr, finallyScopeSym);
                        switchLabelExpr = MakeSwitchLabel(null, finallyScopeSym);
                        EXPRTRY disposeTryExpr = NewExpr(EXPRKIND.TRY) as EXPRTRY;
                        DebugUtil.Assert((tryExpr.Flags & EXPRFLAG.ISFINALLY) != 0);
                        disposeTryExpr.TryBlockExpr = NewExprBlock(null);
                        disposeTryExpr.TryBlockExpr.StatementsExpr = null;
                        disposeTryExpr.TryBlockExpr.ScopeSym = tryExpr.TryBlockExpr.ScopeSym;

                        // Now put together the switch block that will either jump into or over the try block
                        // which might contain nested try/finally stuff
                        //builder.Init(&disposeTryExpr.tryblock.statements);
                        builder.Add(goIntoLabelExpr);
                        builder.Add(this.methodInfo.IteratorInfo.DisposeMethodBodyExpr);
                        disposeTryExpr.TryBlockExpr.StatementsExpr = builder.GetList();
                        builder.Clear();

                        // tryExpr.tryblock now consists of switch label targets and any nested EH stuff
                        disposeTryExpr.TryBlockExpr.ScopeSym = tryBlockExpr.ScopeSym;
                        disposeTryExpr.HandlersExpr = handlersExpr;
                        disposeTryExpr.Flags = tryExpr.Flags;
                        disposeTryExpr.TreeNode = tryExpr.TreeNode;
                        disposeTryExpr.TypeSym = tryExpr.TypeSym;

                        // For dispose
                        // switch, try/finally, skipLabelExpr
                        this.methodInfo.IteratorInfo.DisposeMethodBodyExpr = null;
                        //builder.Init(&this.methodInfo.IteratorInfo.disposeBody);
                        builder.Add(prevStmtExpr);
                        builder.Add(MakeIteratorSwitch(finallyScopeSym, rewriteInfo, goIntoLabelExpr, skipLabelExpr));
                        builder.Add(disposeTryExpr);
                        builder.Add(skipLabelExpr);
                        this.methodInfo.IteratorInfo.DisposeMethodBodyExpr = builder.GetList();
                        builder.Clear();

                        // Replace with a flattened try/finally for the MoveNext method.
                        expr = null;
                        //builder.Init((EXPRSTMT **)pexpr);
                        builder.Add(tryBlockExpr);
                        builder.Add(handlersExpr);
                        expr = builder.GetList();
                    }

                    // Already rewritten inner stuff so no need to recurse
                    return false;

                case EXPRKIND.WRAP:
                    EXPRWRAP wrapExpr = expr as EXPRWRAP;
                    DebugUtil.Assert(
                        !wrapExpr.TypeSym.IsPARAMMODSYM &&
                        (wrapExpr.Expr == null || !wrapExpr.Expr.TypeSym.IsPARAMMODSYM));
                    if ((wrapExpr.Flags & EXPRFLAG.REPLACEWRAP) != 0)
                    {
                        // This wrapped expression should be replaced with the contained expr.
                        expr = wrapExpr.Expr;
                        return false;
                    }

                    if ((wrapExpr.Flags & EXPRFLAG.WRAPASTEMP) != 0 &&
                        (wrapExpr.ContainingScopeSym.ScopeFlags & SCOPEFLAGS.HASYIELDRETURN) != 0)
                    {
                        // We only need to hoist the wrap if there's a yield in this scope
                        string wrapName = CreateSpecialName(SpecialNameKindEnum.HoistedWrap, null);
                        EXPR pField = MakeFieldAccess(rewriteInfo.LocalThisExpr, wrapName, EXPRFLAG.LVALUE);
                        if (pField == null)
                        {
                            // Add the 'wrap' field if it doesn't already exist
                            // We use the pointer value to keep it unique
                            MEMBVARSYM memb = compiler.MainSymbolManager.CreateMembVar(
                                wrapName,
                                rewriteInfo.InnerAggSym,
                                rewriteInfo.InnerAggSym.DeclOnly());
                            memb.Access = ACCESS.PUBLIC;
                            memb.IsReferenced = memb.IsAssigned = true;
                            memb.TypeSym = compiler.MainSymbolManager.SubstType(
                                wrapExpr.TypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone);
                            pField = MakeFieldAccess(rewriteInfo.LocalThisExpr, wrapName, EXPRFLAG.LVALUE);
                        }
                        wrapExpr.Expr = pField;
                        wrapExpr.Flags |= EXPRFLAG.REPLACEWRAP;
                        expr = pField;
                        return false;
                    }
                    else if (
                        wrapExpr.Expr != null &&
                        wrapExpr.Expr.Kind == EXPRKIND.WRAP &&
                        (wrapExpr.Expr.Flags & EXPRFLAG.REPLACEWRAP) != 0)
                    {
                        // There is no longer a temp to free.
                        expr = NewExpr(null, EXPRKIND.NOOP, null);
                        return false;
                    }
                    break;

                default:
                    CheckForNonvirtualAccessFromClosure(expr);
                    break;
            }
            return true;
        } // RewriteIteratorFunc

        //------------------------------------------------------------
        // FUNCBREC.MoveLocalsToIter
        //
        /// <summary>
        /// Recursively move locals from the local scope to a fabricated class.
        /// Only move used locals/parameters into the class.
        /// After moving, orphan the locals.
        /// </summary>
        /// <param name="scopeSym"></param>
        /// <param name="iteratorAggSym"></param>
        /// <param name="iteratorLocalCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int MoveLocalsToIter(SCOPESYM scopeSym, AGGSYM iteratorAggSym, int iteratorLocalCount)
        {
            //SYM ** ppNextChild = &scopeSym.firstChild;
            SYM currentSym = scopeSym;
            bool isParameter = (scopeSym == this.OuterScopeSym);

            // If this scope doesn't have a yield return then we don't need to hoist
            // but we still need to substitute type parameters (don't forget that parameters are always hoisted)
            bool needHoist = (scopeSym.ScopeFlags & SCOPEFLAGS.HASYIELDRETURN) != 0 || isParameter;

            //WCHAR szNameBuffer[MAX_IDENT_LEN + cchOriginalPrefix + 1];
            //HRESULT hr;
            StringBuilder nameBuffer = new StringBuilder();

            if (!needHoist && TypeArray.Size(this.classTypeVariablesForMethod) == 0)
            {
                // No hoisting and nothing to substitute, so we're done
                return iteratorLocalCount;
            }
            DebugUtil.Assert(!isParameter || iteratorLocalCount == 0);

            string prefix = null;
            if (isParameter && this.methodInfo.IteratorInfo.IsEnumerable)
            {
                //hr = StringCchCopyW( szNameBuffer, lengthof(szNameBuffer), szOriginalPrefix);
                //DebugUtil.Assert(SUCCEEDED(hr));
                prefix = FUNCBREC.originalPrefix;
            }

            DebugUtil.Assert(iteratorAggSym.IsFabricated);
            DebugUtil.Assert(this.methodInfo.IteratorInfo != null);

            for (SYM innerSym = scopeSym.FirstChildSym; innerSym != null; innerSym = innerSym.NextSym)
            {
                //*ppNextChild = innerSym;
                if (currentSym == scopeSym)
                {
                    scopeSym.FirstChildSym = innerSym;
                }
                else
                {
                    currentSym.NextSym = innerSym;
                }

                if (innerSym.IsLOCVARSYM)
                {
                    nameBuffer.Length = 0;
                    if (prefix != null)
                    {
                        nameBuffer.Append(prefix);
                    }

                    LOCVARSYM localSym = innerSym as LOCVARSYM;
                    if (!localSym.LocSlotInfo.IsUsed ||
                        !isParameter && localSym.HoistForAnonMeth && !localSym.IsCatch)
                    {
                        // It's not used or only used in an anonymous method, so NUKE it!
                        goto ORPHAN_CHILD;
                    }

                    if (!needHoist)
                    {
                        DebugUtil.Assert(TypeArray.Size(this.classTypeVariablesForMethod) > 0);

                        localSym.TypeSym = compiler.MainSymbolManager.SubstType(
                            localSym.TypeSym,
                            (TypeArray)null,
                            this.classTypeVariablesForMethod,
                            SubstTypeFlagsEnum.NormNone);
                    }
                    else
                    {
                        // If you get an DebugUtil.Assert here, that means somehow
                        // We didn't report an error when the address was taken of a localSym
                        // inside an iterator!!!!
                        DebugUtil.Assert(!localSym.IsAssumedPinned());
                        DebugUtil.Assert(
                            isParameter ||
                            localSym.MovedToFieldSym == null ||
                            (localSym.TypeSym.IsAGGTYPESYM &&
                            (localSym.TypeSym as AGGTYPESYM).GetAggregate().IsFabricated));

                        // Add an appropriately named member to our "$localSym" class
                        MEMBVARSYM memberSym = MoveLocalToField(
                            localSym,
                            iteratorAggSym,
                            this.classTypeVariablesForMethod,
                            !isParameter,
                            null);
                        localSym.MovedToFieldSym = memberSym;

                        if (isParameter &&
                            this.methodInfo.IteratorInfo.IsEnumerable &&
                            (!localSym.IsThis || memberSym.TypeSym.IsStructOrEnum()))
                        {
                            DebugUtil.Assert(localSym.LocSlotInfo.IsParameter);

                            //hr = StringCchCopyW(
                            //    szNameBuffer + cchOriginalPrefix,
                            //    lengthof(szNameBuffer) - cchOriginalPrefix,
                            //    memberSym.name.text);
                            nameBuffer.Append(memberSym.Name);
                            //DebugUtil.Assert(SUCCEEDED(hr));
                            string copyName = nameBuffer.ToString();
                            compiler.NameManager.AddString(copyName);
                            MEMBVARSYM copyMemberSym = compiler.MainSymbolManager.CreateMembVar(
                                copyName,
                                iteratorAggSym,
                                iteratorAggSym.DeclOnly());
                            copyMemberSym.Access = ACCESS.PUBLIC;
                            copyMemberSym.IsReferenced = copyMemberSym.IsAssigned = true;
                            copyMemberSym.TypeSym = memberSym.TypeSym;
                            memberSym.SetOriginalCopy(copyMemberSym);
                        }

                        localSym.IsIteratorLocal = true;
                        if (isParameter)
                        {
                            // Parameters are always live so don't need to keep the locals
                            goto ORPHAN_CHILD;
                        }

                        memberSym.LocalIteratorIndex = iteratorLocalCount++;
                    }
                }
                else if (innerSym.IsSCOPESYM)
                {
                    // recurse (nested scopes never have parameters)
                    iteratorLocalCount = MoveLocalsToIter(innerSym as SCOPESYM, iteratorAggSym, iteratorLocalCount);
                }
                //ppNextChild = &innerSym.nextChild;
                currentSym = innerSym;
            ORPHAN_CHILD:
                ;
            }

            // Terminate the list.
            //*ppNextChild = null;
            if (currentSym == scopeSym)
            {
                scopeSym.FirstChildSym = null;
            }
            else
            {
                currentSym.NextSym = null;
            }

            return iteratorLocalCount;
        }

        //------------------------------------------------------------
        // FUNCBREC.MakeIteratorSwitch
        //
        /// <summary></summary>
        /// <param name="scopeSym"></param>
        /// <param name="rewriteInfo"></param>
        /// <param name="keyLabelExpr"></param>
        /// <param name="noKeyLabelExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPRSWITCH MakeIteratorSwitch(
            SCOPESYM scopeSym,
            IteratorRewriteInfo rewriteInfo,
            EXPRLABEL keyLabelExpr,
            EXPRLABEL noKeyLabelExpr)
        {
            int labelCount = 0;
            bool isNested = (this.currentScopeSym != this.OuterScopeSym);

            for (SYM sym = scopeSym.FirstChildSym; sym != null; sym = sym.NextSym)
            {
                if (!sym.IsLABELSYM || (sym as LABELSYM).LabelExpr.Kind != EXPRKIND.SWITCHLABEL)
                {
                    continue;
                }

                if (!isNested || ((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).StatementsExpr == null)
                {
                    labelCount++;
                }

                if (isNested &&
                    ((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).StatementsExpr == null &&
                    ((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).KeyExpr != null)
                {
                    // This is an unbound 'finally label'
                    // Thats says for this case we need to jump into an inner try-block
                    // Create an unbound 'finally' label so the outer finally scope knows to jump into
                    // us for each of these values too
                    EXPRSWITCHLABEL outerLabelExpr;
                    outerLabelExpr = MakeSwitchLabel(
                        ((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).KeyExpr as EXPRCONSTANT,
                        this.currentScopeSym);
                }
            }

            EXPRSWITCH switchExpr = NewExpr(null, EXPRKIND.SWITCH, null) as EXPRSWITCH;
            switchExpr.HashTableInfo = null;
            switchExpr.BreakLabelExpr
                = switchExpr.NullLabelExpr
                = null;
            switchExpr.ArgumentExpr = rewriteInfo.StateExpr;
            switchExpr.Flags |= (EXPRFLAG.HASDEFAULT | EXPRFLAG.NODEBUGINFO);
            //EXPRSWITCHLABEL ** labArray = (EXPRSWITCHLABEL**) allocator.Alloc(sizeof(EXPRSWITCHLABEL*) * labelCount);
            //switchExpr.labels = labArray;
            switchExpr.BodiesExpr = null;
            StatementListBuilder labelBuilder = new StatementListBuilder();	// ((EXPRSTMT **)&switchExpr.bodies);

            for (SYM sym = scopeSym.FirstChildSym; sym != null; sym = sym.NextSym)
            {
                if (!sym.IsLABELSYM || (sym as LABELSYM).LabelExpr.Kind != EXPRKIND.SWITCHLABEL)
                {
                    continue;
                }
                if (!isNested || ((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).StatementsExpr == null)
                {
                    //*labArray++ = sym.asLABELSYM().labelExpr.asSWITCHLABEL();
                    switchExpr.LabelArray.Add((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL);
                    if (((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).StatementsExpr == null)
                    {
                        if (((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).KeyExpr != null)
                        {
                            ((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).StatementsExpr
                                = MakeGoto(null, keyLabelExpr, EXPRFLAG.NODEBUGINFO);
                        }
                        else
                        {
                            ((sym as LABELSYM).LabelExpr as EXPRSWITCHLABEL).StatementsExpr
                                = MakeGoto(null, noKeyLabelExpr, EXPRFLAG.NODEBUGINFO);
                        }
                    }
                    labelBuilder.Add((sym as LABELSYM).LabelExpr);
                }
            }
            switchExpr.BodiesExpr = labelBuilder.GetList() as EXPRSWITCHLABEL;

            //qsort(switchExpr.labels, labelCount, sizeof(EXPR*), &FUNCBREC::compareSwitchLabels);
            switchExpr.LabelArray.Sort(new CCompareSwitchLabels());
            switchExpr.LabelCount = labelCount;

            return switchExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.MakeSwitchLabel
        //
        /// <summary></summary>
        /// <param name="keyExpr"></param>
        /// <param name="scopeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPRSWITCHLABEL MakeSwitchLabel(EXPRCONSTANT keyExpr, SCOPESYM scopeSym)
        {
            string labelName = GetSwitchLabelName(keyExpr);
            LABELSYM labelSym = compiler.LocalSymbolManager.CreateLocalSym(
                SYMKIND.LABELSYM,
                labelName,
                scopeSym) as LABELSYM;

            EXPRSWITCHLABEL labelExpr = NewExpr(null, EXPRKIND.SWITCHLABEL, null) as EXPRSWITCHLABEL;
            labelExpr.KeyExpr = keyExpr;
            labelExpr.LabelSym = labelSym;
            labelSym.LabelExpr = labelExpr;
            return labelExpr;
        }
    }
}
