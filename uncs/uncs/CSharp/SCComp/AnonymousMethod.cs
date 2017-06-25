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
// AnonymousMethod.cs
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
        //--------------------------------------------------
        // FUNCBREC.FixUpAnonMethInfoLists
        //
        /// <summary></summary>
        /// <param name="firstInfo"></param>
        //--------------------------------------------------
        internal void FixUpAnonMethInfoLists(ref AnonMethInfo firstInfo)
        {
            AnonMethInfo prevInfo = null;
            // AnonMethInfo instance including NextInfo field pointed by ppami.

            for (AnonMethInfo currentInfo = firstInfo; currentInfo != null; )
            {
                AnonMethInfo nextInfo = currentInfo.NextInfo;
                if (currentInfo.Seen)
                {
                    FixUpAnonMethInfoLists(ref currentInfo.ChildInfo);

                    if (prevInfo == null)
                    {
                        firstInfo = currentInfo;
                    }
                    else
                    {
                        prevInfo.NextInfo = currentInfo;
                    }
                    prevInfo = currentInfo;
                }
                else
                {
                    // For safety, make sure orphaned ones don't point to valid ones.
                    currentInfo.OuterInfo = null;
                    currentInfo.NextInfo = null;
                }
                currentInfo = nextInfo;
            }

            if (prevInfo == null)
            {
                firstInfo = null;
            }
            else
            {
                prevInfo.NextInfo = null;
            }
        }

        //--------------------------------------------------
        // FUNCBREC.RewriteAnonDelegateBodies
        //
        /// <summary>
        /// Rewrite method bodies that contain anonymous methods
        /// Mostly just moves locals from local scope to $local of type pDelClass
        /// and changes EK_LOCAL to EK_FIELD
        /// (recurses through the EK_ANONMETH exprs in rewriteAnonMeth)
        /// and substitutes types to go from the method type arguments
        /// to the class type arguments
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="argScopeSym"></param>
        /// <param name="firstAnonInfo"></param>
        /// <param name="bodyExpr"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR RewriteAnonDelegateBodies(
            METHSYM methodSym,
            SCOPESYM argScopeSym,
            AnonMethInfo firstAnonInfo,
            EXPR bodyExpr)
        {
            Compiler.SetLocation(COMPILER.STAGE.TRANSFORM);
            Compiler.SetLocation(methodSym);

            SCOPESYM oldCurrentScopeSym = this.currentScopeSym;
            SCOPESYM oldOutScopeSym = this.OuterScopeSym;
            AGGSYM oldParentAggSym = this.parentAggSym;
            DECLSYM oldParentDeclSym = this.parentDeclSym;
            AnonymousMethodRewriteInfo amrwInfo
                = new AnonymousMethodRewriteInfo(methodSym, argScopeSym, firstAnonInfo);

            this.currentScopeSym = this.OuterScopeSym = argScopeSym;
            this.parentAggSym = methodSym.ClassSym;
            this.parentDeclSym = methodSym.ContainingDeclaration();

            RecurseAndRewriteExprTree(ref bodyExpr, amrwInfo);

            this.currentScopeSym = oldCurrentScopeSym;
            this.OuterScopeSym = oldOutScopeSym;
            this.parentAggSym = oldParentAggSym;
            this.parentDeclSym = oldParentDeclSym;

            return bodyExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeAnonCtor
        //
        /// <summary></summary>
        /// <param name="anonMethNode"></param>
        /// <param name="methInfo"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal EXPR MakeAnonCtor(BASENODE anonMethNode, METHINFO methInfo, AGGINFO classInfo)
        {
            DebugUtil.Assert(methInfo.MethodSym != null && methInfo.MethodSym.ParameterTypes.Count == 0);
            InitMethod(methInfo, anonMethNode, classInfo);

            EXPRBLOCK blockExpr = NewExprBlock(null);
            blockExpr.Flags |= EXPRFLAG.NEEDSRET;
            StatementListBuilder builder = new StatementListBuilder();  //(&blockExpr.statements);

            // can't call createBaseConstructorCall
            // because it sucks in the arguments from pTree if pAMSym is a CTOR
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

            blockExpr.StatementsExpr = builder.GetList();
            return blockExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.BindAnonymousMethod
        //
        // Here's where we start anonymous methods.
        // Each METHSYM keeps a list of AnonMethInfo structs
        // that contain information about each anonymous method.
        // Currently no attempts is made to find duplicate anonymous methods.
        // Durings the bind stage we determine
        // if the anonymous method uses any outer locals/parameters.
        // That determines the scope at which the anonymous method's delegate
        // will be cached at and where the actual method lives.
        // We start by setting the pScope to pOuterScope,
        // and then rachet it down to inner scopes as locals are used.
        // So that when the body of the anonymous method is finished
        // binding pScope is the outermost scope at which it can be cached/created.
        // At the end of binding a method any anonymous methods still sitting
        // in pOuterScope have to be manually moved into the real outerscope
        // (pOuterScope is the argument scope).
        // After binding the body various conversion should be applied
        // that determine the delegate type.
        // During post-bind the anonymous method body is checked
        // for accessing the <this> pointer
        // and thus we finally have all the information needed for the transformation.
        //
        // The transformation stage creates all the needed global symbols
        // (display classes, methods, static cache fields, hoisted fields)
        // and some of the local symbols (locals instances of the display classes,
        // local cached instantiated delegates, etc.).
        // As each block is entered, the appropriate symbols are created
        // as well as any initialization code
        // (create the display class, pass in any hoisted parameters, null-init the caches).
        // When each EK_ANONMETH expr is reached during the expr tree walk
        // it is changed into a test-and-init expression on the cached delegate.
        // Once we've finished walking the expr tree, we're all done rewriting.
        // Iterator transformations, if any, happen at this point (so they never see EK_ANONMETH).
        // Then each of the nested classes and no-longer-anonymous methods each get compiled
        // using the info stored in the AnonMethInfo struct.
        //
        // How and Where we cache:
        // * Even though static methods are slower to invoke (through delegates)
        //   than instance methods we will make anonymous methods static
        //   if they don't use <this> or any locals.
        // * If no locals are used the anonymous method is a private member of the user's class
        // * If no locals are used, and <this> is not used, and the outer method does not have
        //   type parameters, then the delegate is cached as a static field of the user's class.
        // * If caching scope is the same as the use scope for the anonymous method
        //   we don't bother to cache it and just recreate the delegate each time.
        // * Otherwise we create a local variable of the delegate type in the caching scope
        //   and initialize it to null on entry of that scope.
        // * If the anonymous method is cached (either as a static field or a local),
        //   then each use becomes a test-and-init-and-use.
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        //--------------------------------------------------
        private EXPR BindAnonymousMethod(ANONBLOCKNODE treeNode)
        {
            AnonMethInfo anonInfo = null;

            if (this.methodSym == null)
            {
                Compiler.Error(treeNode, CSCERRID.ERR_AnonMethNotAllowed);

                // Create arg scope (to be symmetric with non-error case)
                CreateNewScope();
            }
            else
            {
                // Create the method symbol, but don't attach it anyplace, yet
                anonInfo = new AnonMethInfo();
                anonInfo.ParseTreeNode = treeNode;
                anonInfo.OuterInfo = this.currentAnonymousMethodInfo;

                if (this.methodSym.Name == null)
                {
                    StringBuilder strBuilder = new StringBuilder();
                    MetaDataHelper hlpr = new MetaDataHelper("+");
                    hlpr.GetExplicitImplName(this.methodSym, strBuilder);
                    anonInfo.Name = CreateSpecialName(
                        SpecialNameKindEnum.AnonymousMethod,
                        strBuilder.ToString());
                }
                else
                {
                    anonInfo.Name = CreateSpecialName(
                        SpecialNameKindEnum.AnonymousMethod,
                        methodSym.Name);
                }

                if (this.currentAnonymousMethodInfo != null)
                {
                    AddAnonMethInfo(ref this.currentAnonymousMethodInfo.ChildInfo, anonInfo);
                }
                else
                {
                    AddAnonMethInfo(ref this.firstAnonymousMethodInfo, anonInfo);
                }

                // Set the scope to create the cached delegate (and $locals) to the outermost possible,
                // then as variables are bound, this will ratchet down to the inner-most scope with
                // a used local
                anonInfo.ScopeSym = this.OuterScopeSym;
                // This is just a marker until we know the real scope

                ANONSCOPESYM anonScopeSym = Compiler.LocalSymbolManager.CreateLocalSym(
                    SYMKIND.ANONSCOPESYM,
                    null,
                    this.currentScopeSym) as ANONSCOPESYM;

                // We need to verify that there are names for every parameter
                // Introduce a new scope for those names (this scope will become the outerscope
                // of the delegate method)
                // Can't call createNewScope because we don't want this new scope to show up as a child
                anonInfo.ParametersScopeSym = Compiler.LocalSymbolManager.CreateLocalSym(
                    SYMKIND.SCOPESYM,
                    null,
                    null) as SCOPESYM;
                anonInfo.ParametersScopeSym.ParentSym = this.currentScopeSym;
                anonInfo.ParametersScopeSym.NestingOrder = this.currentScopeSym.NestingOrder + 1;
                this.currentScopeSym = anonInfo.ParametersScopeSym;
                anonScopeSym.ScopeSym = this.currentScopeSym;

                // This will eventually be a top-level scope!
                anonInfo.ParametersScopeSym.ScopeFlags |= SCOPEFLAGS.DELEGATESCOPE;

                anonInfo.JBitMin = this.uninitedVarCount + 1;

                // Create the nested <this> pointer in case we need it later
                // Use a dummy type for now until we know the real type
                LOCVARSYM nestedThisSym = AddParam(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.THIS),
                    GetVoidType(),
                    0,
                    this.currentScopeSym);
                nestedThisSym.LocSlotInfo.IsUsed = true;
                nestedThisSym.IsThis = true;
                anonInfo.ThisPointerSym = nestedThisSym;
            }

            // -1 here as a token index indicates that the user did not specify any parameters
            // instead of tree.pArgs == null which means they specified an empty parameter list "()"
            // For anonymous methods with no user-specified parameters we leave this null so it will
            // match any delegate signature parameters (which is different than the delegate with no args)
            TypeArray paramArray = null;
            if (treeNode.CloseParenIndex != -1)
            {
                // The user had some parameters
                DebugUtil.Assert(
                    (treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_VARARGS) == 0 &&
                    (treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) == 0);

                // Pass in the outer method as a context so we get it's type parameters (we just re-use it's symbols)
                bool hasParams = false;
                Compiler.ClsDeclRec.DefineParameters(
                    this.ContextForTypeBinding(),
                    treeNode.ArgumentsNode,
                    false,
                    ref paramArray,
                    ref hasParams);

                // Now add all of the declared method arguments to the scope
                int p = 0;
                BASENODE node = treeNode.ArgumentsNode;
                while (node != null)
                {
                    PARAMETERNODE paramNode;
                    if (node.Kind == NODEKIND.LIST)
                    {
                        paramNode = node.AsLIST.Operand1 as PARAMETERNODE;
                        node = node.AsLIST.Operand2;
                    }
                    else
                    {
                        paramNode = node as PARAMETERNODE;
                        node = null;
                    }
                    if (paramNode == null)
                    {
                        continue;
                    }

                    TYPESYM typeSym = paramArray[p++];
                    LOCVARSYM argSym = DeclareParam(
                        paramNode.NameNode.Name,
                        typeSym,
                        0,
                        paramNode,
                        this.currentScopeSym);
                    if (argSym != null)
                    {
                        // Because we put the parameters at an inner scope,
                        // we have to put them in the outer cache to be found
                        StoreInCache(paramNode, argSym.Name, argSym, null, true);
                    }
                }
            }

            if (anonInfo != null)
            {
                anonInfo.ParameterArray = paramArray;
            }

            EXPR rval = BindAnonymousMethodInner(treeNode, anonInfo);

            if (anonInfo != null)
            {
                anonInfo.JBitLim = uninitedVarCount + 1;
            }
            CloseScope();

            return rval;
        }

        //--------------------------------------------------
        // FUNCBREC.BindAnonymousMethodInner
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="anonInfo"></param>
        /// <returns></returns>
        //--------------------------------------------------
        private EXPR BindAnonymousMethodInner(ANONBLOCKNODE treeNode, AnonMethInfo anonInfo)
        {
            EXPR rval = null;

            // Save the scopes so we don't think we're inside anything
            // (because we aren't when inside the AM block)
            SCOPESYM outerTryScope = this.innermostTryScopeSym;
            SCOPESYM outerCatchScope = this.innermostCatchScopeSym;
            SCOPESYM outerFinallyScope = this.innermostFinallyScopeSym;
            this.innermostCatchScopeSym = this.OuterScopeSym;
            this.innermostTryScopeSym = this.OuterScopeSym;
            this.innermostFinallyScopeSym = this.OuterScopeSym;

            if (anonInfo == null)
            {
                // Bind the block as if it were inline'd code
                SCOPESYM scopSym = null;
                BindBlock(
                    treeNode.BodyNode,
                    SCOPEFLAGS.NONE,
                    ref scopSym,
                    null);
                // Then ignore it
                rval = NewError(treeNode, null);
            }
            else
            {
                DebugUtil.Assert(this.currentAnonymousMethodInfo == anonInfo.OuterInfo);
                this.currentAnonymousMethodInfo = anonInfo;
                CreateNewScope();

                // Bind the block as if it were inline'd code
                anonInfo.BodyBlockExpr
                    = BindMethOrPropBody(treeNode.BodyNode as BLOCKNODE);

                ReachabilityChecker reach = new ReachabilityChecker(Compiler);
                reach.SetReachability(anonInfo.BodyBlockExpr, true);
                anonInfo.HasReturnAsLeave = reach.HasReturnAsLeave;

                DebugUtil.Assert(this.currentAnonymousMethodInfo == anonInfo);
                this.currentAnonymousMethodInfo = anonInfo.OuterInfo;

                anonInfo.BodyBlockExpr.OwingBlockExpr = null;

                CloseScope();

                rval = NewExpr(
                    treeNode,
                    EXPRKIND.ANONMETH,
                    Compiler.MainSymbolManager.AnonymousMethodSym);
                (rval as EXPRANONMETH).AnonymousMethodInfo = anonInfo;
            }

            // Restore everything
            this.innermostTryScopeSym = outerTryScope;
            this.innermostCatchScopeSym = outerCatchScope;
            this.innermostFinallyScopeSym = outerFinallyScope;

            return rval;
        }

        //--------------------------------------------------
        // FUNCBREC.BindAnonMethConversion
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="srcExpr"></param>
        /// <param name="destTypeSym"></param>
        /// <param name="destExpr"></param>
        /// <param name="reportErrors"></param>
        /// <returns></returns>
        //--------------------------------------------------
        private bool BindAnonMethConversion(
            BASENODE treeNode,
            EXPR srcExpr,
            TYPESYM destTypeSym,
            ref EXPR destExpr,
            bool reportErrors)
        {
            DebugUtil.Assert(
                srcExpr != null &&
                srcExpr.TypeSym.IsANONMETHSYM &&
                srcExpr.Kind == EXPRKIND.ANONMETH);
            destExpr = null;

            if (!destTypeSym.IsDelegateType())
            {
                if (reportErrors)
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_AnonMethToNonDel,
                        new ErrArg("anonymous method bloc"),
                        new ErrArg(destTypeSym));
                }
                return false;
            }

            if (srcExpr == null || srcExpr.Kind != EXPRKIND.ANONMETH)
            {
                return false;
            }

            AGGTYPESYM aggTypeSym = destTypeSym as AGGTYPESYM;

            METHSYM invokeSym
                = compiler.MainSymbolManager.LookupInvokeMeth(aggTypeSym.GetAggregate());
            if (invokeSym == null || !invokeSym.IsInvoke)
            {
                return false;
            }

            AnonMethInfo anonInfo = (srcExpr as EXPRANONMETH).AnonymousMethodInfo;
            TypeArray invokeParamTypes = compiler.MainSymbolManager.SubstTypeArray(
                invokeSym.ParameterTypes,
                aggTypeSym,
                null);
            TYPESYM invokeReturnTypeSym = compiler.MainSymbolManager.SubstType(
                invokeSym.ReturnTypeSym,
                aggTypeSym,
                null);
            bool result = true;

            if (anonInfo.ParameterArray != null)
            {
                // Check parameter lists if the user gave one
                if (anonInfo.ParameterArray != invokeParamTypes)
                {
                    // the error case, parameter lists don't match exactly
                    if (!reportErrors)
                    {
                        return false;
                    }

                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_CantConvAnonMethParams,
                        new ErrArg(aggTypeSym));
                    result = false;

                    if (anonInfo.ParameterArray.Count != invokeParamTypes.Count)
                    {
                        compiler.Error(
                            srcExpr.TreeNode,
                            CSCERRID.ERR_BadDelArgCount,
                            new ErrArg(aggTypeSym),
                            new ErrArg(anonInfo.ParameterArray.Count));
                    }
                    else
                    {
                        int p = 0;
                        bool reportedErrorTemp = false;
                        BASENODE node1 = (srcExpr.TreeNode as ANONBLOCKNODE).ArgumentsNode;
                        while (node1 != null)
                        {
                            PARAMETERNODE paramNode;
                            if (node1.Kind == NODEKIND.LIST)
                            {
                                paramNode = node1.AsLIST.Operand1 as PARAMETERNODE;
                                node1 = node1.AsLIST.Operand2;
                            }
                            else
                            {
                                paramNode = node1 as PARAMETERNODE;
                                node1 = null;
                            }

                            TYPESYM fromSym = anonInfo.ParameterArray[p];
                            TYPESYM toSym = invokeParamTypes[p];
                            p++;
                            if (fromSym != toSym)
                            {
                                TYPESYM fromStrippedSym = fromSym.IsPARAMMODSYM ?
                                    (fromSym as PARAMMODSYM).ParamTypeSym :
                                    fromSym;
                                TYPESYM toStrippedSym = toSym.IsPARAMMODSYM ?
                                    (toSym as PARAMMODSYM).ParamTypeSym :
                                    toSym;
                                if (fromStrippedSym == toStrippedSym)
                                {
                                    if (toStrippedSym != toSym)
                                    {
                                        compiler.Error(
                                            paramNode,
                                            CSCERRID.ERR_BadParamRef,
                                            new ErrArg(p),
                                            new ErrArg((toSym as PARAMMODSYM).IsOut ? "out" : "ref"));
                                        reportedErrorTemp = true;
                                    }
                                    else
                                    {
                                        // the argument is decorated, but doesn't needs a 'ref' or 'out'
                                        compiler.Error(
                                            paramNode,
                                            CSCERRID.ERR_BadParamExtraRef,
                                            new ErrArg(p),
                                            new ErrArg((fromSym as PARAMMODSYM).IsOut ? "out" : "ref"));
                                        reportedErrorTemp = true;
                                    }
                                }
                                else
                                {
                                    compiler.Error(
                                        paramNode,
                                        CSCERRID.ERR_BadParamType,
                                        new ErrArg(p),
                                        new ErrArg(fromSym, ErrArgFlagsEnum.Unique),
                                        new ErrArg(toSym, ErrArgFlagsEnum.Unique));
                                    reportedErrorTemp = true;
                                }
                            }
                        }
                        DebugUtil.Assert(reportedErrorTemp);
                    }
                }
            }
            else
            {
                // The user gave no parameter list
                // so this AM is compatible with any signature containing no out parameters
                for (int p = 0; p < invokeParamTypes.Count; )
                {
                    TYPESYM toSym = invokeParamTypes[p];
                    p++;
                    if (toSym.IsPARAMMODSYM && (toSym as PARAMMODSYM).IsOut)
                    {
                        if (!reportErrors)
                        {
                            return false;
                        }

                        if (result)
                        {
                            // only report this once
                            compiler.Error(
                                treeNode,
                                CSCERRID.ERR_CantConvAnonMethNoParams,
                                new ErrArg(aggTypeSym));
                            result = false;
                        }

                        // They need to add an 'out' if they want t^o use this signature
                        compiler.Error(
                            treeNode,
                            CSCERRID.ERR_BadParamRef,
                            new ErrArg(p),
                            new ErrArg("out"));
                    }
                }
            }

            // Check (and possibly cast) return types
            bool returnResult = true;
            if (invokeReturnTypeSym == GetVoidType())
            {
                // delegate returns void, so there must be either no return statements,
                // or only return statements with no value/object
                EXPR expr1 = anonInfo.ReturnExprList;
                while (expr1 != null)
                {
                    EXPR returnExpr;
                    if (expr1.Kind == EXPRKIND.LIST)
                    {
                        returnExpr = expr1.AsBIN.Operand1;
                        expr1 = expr1.AsBIN.Operand2;
                    }
                    else
                    {
                        returnExpr = expr1;
                        expr1 = null;
                    }

                    if ((returnExpr as EXPRRETURN).ObjectExpr != null)
                    {
                        if (!reportErrors)
                        {
                            return false;
                        }

                        if (returnResult)
                        {
                            compiler.Error(
                                treeNode,
                                CSCERRID.ERR_CantConvAnonMethReturns,
                                new ErrArg("anonymous method block"),
                                new ErrArg(aggTypeSym));
                        }
                        // return non-empty
                        compiler.Error(
                            treeNode,
                            CSCERRID.ERR_RetNoObjectRequired,
                            new ErrArg(aggTypeSym));
                        returnResult = false;
                    }
                }
            }
            else if (anonInfo.ReturnExprList != null)
            {
                // delegate returns a value,
                // check that all the return statements are IMPLICITLY convertable
                EXPR expr2 = anonInfo.ReturnExprList;
                while (expr2 != null)
                {
                    EXPR returnExpr;
                    if (expr2.Kind == EXPRKIND.LIST)
                    {
                        returnExpr = expr2.AsBIN.Operand1;
                        expr2 = expr2.AsBIN.Operand2;
                    }
                    else
                    {
                        returnExpr = expr2;
                        expr2 = null;
                    }

                    if ((returnExpr as EXPRRETURN).ObjectExpr == null ||
                        !CanConvert(
                            (returnExpr as EXPRRETURN).ObjectExpr,
                            invokeReturnTypeSym,
                            0))
                    {
                        if (!reportErrors)
                        {
                            return false;
                        }
                        if (returnResult)
                        {
                            compiler.Error(
                                treeNode,
                                CSCERRID.ERR_CantConvAnonMethReturns,
                                new ErrArg("anonymous method Block"),
                                new ErrArg(aggTypeSym));
                        }
                        if ((returnExpr as EXPRRETURN).ObjectExpr != null)
                        {
                            MustConvert(
                                (returnExpr as EXPRRETURN).ObjectExpr,
                                invokeReturnTypeSym,
                                0);
                        }
                        else
                        {
                            compiler.Error(
                                treeNode,
                                CSCERRID.ERR_RetObjectRequired,
                                new ErrArg(invokeSym.ReturnTypeSym));
                        }
                        returnResult = false;
                    }
                }
                if (returnResult && anonInfo.BodyBlockExpr.ReachableEnd())
                {
                    if (!reportErrors)
                    {
                        return false;
                    }
                    compiler.Error(
                        anonInfo.ParseTreeNode,
                        CSCERRID.ERR_AnonymousReturnExpected,
                        new ErrArg(destTypeSym));
                    returnResult = false;
                }
            }
            else if (anonInfo.BodyBlockExpr.ReachableEnd())
            {
                // Non-void returning delegate,
                // and the anonymous method has no return statements and a reachable end.
                if (!reportErrors)
                {
                    return false;
                }
                compiler.Error(
                    anonInfo.ParseTreeNode,
                    CSCERRID.ERR_AnonymousReturnExpected,
                    new ErrArg(destTypeSym));
                returnResult = false;
            }

            if (result && returnResult)
            {
                destExpr = srcExpr;
                destExpr.TypeSym = aggTypeSym;

                DebugUtil.Assert(anonInfo.DelegateTypeSym == null);
                anonInfo.DelegateTypeSym = aggTypeSym;
                if (anonInfo.ParameterArray == null)
                {
                    anonInfo.ParameterArray = invokeParamTypes;
                }
                anonInfo.ReturnTypeSym = invokeReturnTypeSym;

                SetNodeExpr(treeNode, destExpr);

                // Now cast all the return expressions
                if (invokeReturnTypeSym != GetVoidType())
                {
                    EXPR expr3 = anonInfo.ReturnExprList;
                    while (expr3 != null)
                    {
                        EXPR returnExpr;
                        if (expr3.Kind == EXPRKIND.LIST)
                        {
                            returnExpr = expr3.AsBIN.Operand1;
                            expr3 = expr3.AsBIN.Operand2;
                        }
                        else
                        {
                            returnExpr = expr3;
                            expr3 = null;
                        }

                        (returnExpr as EXPRRETURN).ObjectExpr = MustConvert(
                            (returnExpr as EXPRRETURN).ObjectExpr,
                            invokeReturnTypeSym,
                            0);

                        DebugUtil.Assert(
                            (returnExpr as EXPRRETURN).ObjectExpr != null &&
                            (returnExpr as EXPRRETURN).ObjectExpr.IsOK);
                    }
                }
            }

            return (result && returnResult);
        }

        //------------------------------------------------------------
        // FUNCBREC.RewriteAnonymousMethodFunc
        //
        /// <summary>
        /// Function called by RecurseAndRewriteExprTree to do the actual rewriting of important EXPRs
        /// for rewrite anonymous delegates outers and bodies
        /// by changing locals access into appropriate EK_FIELDs
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="anonRewriteInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool RewriteAnonymousMethodFunc(
            ref EXPR expr,	// in, out
            AnonymousMethodRewriteInfo anonRewriteInfo)
        {
            switch (expr.Kind)
            {
                case EXPRKIND.BLOCK:
                    expr = RewriteLocalsBlock(expr as EXPRBLOCK, anonRewriteInfo);
                    return false;

                case EXPRKIND.ANONMETH:
                case EXPRKIND.LAMBDAEXPR:
                    expr = RewriteAnonMethExpr(expr as EXPRANONMETH);
                    return false;

                case EXPRKIND.LOCAL:
                    EXPRLOCAL localExpr = expr as EXPRLOCAL;

                    // if it wasn't hoisted, leave it alone
                    if (!localExpr.LocVarSym.HoistForAnonMeth)
                    {
                        return true;
                    }

                    // Find if this local is defined inside our scopes, or some outer scope
                    // We always treat <this> as if it was defined outside everything
                    if ((localExpr.LocVarSym.ParentSym as SCOPESYM).NestingOrder
                            >= anonRewriteInfo.ArgumentsScopeSym.NestingOrder &&
                        !localExpr.LocVarSym.IsThis)
                    {
                        DebugUtil.Assert(localExpr.LocVarSym.MovedToFieldSym != null);

                        // A real or anonymous method accessing an inner local that has been hoisted
                        // so we now have an extra level of indirection.
                        EXPRFLAG flags = localExpr.Flags & EXPRFLAG.MASK_ANY;
                        if ((flags & EXPRFLAG.LVALUE) != 0)
                        {
                            flags |= EXPRFLAG.MEMBERSET;
                        }

                        //--------------------------------------------
                        // To the expression tree
                        //     2016/05/07 hirano567@hotmail.co.jp
                        //--------------------------------------------
                        if (localExpr.ToExpressionTree)
                        {
                            expr = ConvertLocalToExpressionTree(
                                localExpr.LocVarSym.MovedToFieldSym,
                                (localExpr.LocVarSym.ParentSym as SCOPESYM).AnonymousScopeInfo.LocalExpr);
                        }
                        //--------------------------------------------
                        // Otherwise
                        //--------------------------------------------
                        else
                        {
                            EXPR objectExpr = MakeFieldAccess(
                                (localExpr.LocVarSym.ParentSym as SCOPESYM).AnonymousScopeInfo.LocalExpr,
                                localExpr.LocVarSym.MovedToFieldSym,
                                flags);
                            if (localExpr.TypeSym.IsPARAMMODSYM)
                            {
                                objectExpr.TypeSym = compiler.MainSymbolManager.GetParamModifier(
                                    objectExpr.TypeSym,
                                    (localExpr.TypeSym as PARAMMODSYM).IsOut);
                            }
                            expr = objectExpr;
                        }
                    }
                    else
                    {
                        // An anonymous method accessing a hoisted local defined in an outer scope
                        // (including parameters and <this>)
                        // Or a real method accessing <this>

                        // If optimizations allowed this method to be parented by a user class,
                        // then the only outer local 
                        // it could be accessing is <this> in which case,
                        // we don't need to transform it, but we do need to adjust
                        // which thisPointer LOCVARSYM it uses
                        // Or if this method isn't an anonymous method, then it must be an iterator
                        // of which the <this> access will get transformed later.

                        if (!anonRewriteInfo.AnonymousMethodSym.ClassSym.IsFabricated)
                        {
                            DebugUtil.Assert(
                                localExpr.LocVarSym.Name == compiler.NameManager.GetPredefinedName(PREDEFNAME.THIS) &&
                                localExpr.LocVarSym.IsThis);
                            DebugUtil.Assert(!anonRewriteInfo.AnonymousMethodSym.IsStatic);

                            localExpr.LocVarSym = this.thisPointerSym;
                            return false;
                        }

                        EXPR objectExpr = BindThisImplicit(null);
                        DebugUtil.Assert(objectExpr.TypeSym == anonRewriteInfo.AnonymousMethodSym.ClassSym.GetThisType());

                        if (objectExpr.TypeSym == localExpr.TypeSym)
                        {
                            // This happens because of how we share the exprLoc
                            DebugUtil.Assert(localExpr.TypeSym.GetAggregate().IsFabricated);
                            return false;
                        }

                        EXPRFLAG flags = localExpr.Flags & EXPRFLAG.MASK_ANY;
                        if ((flags & EXPRFLAG.LVALUE) != 0)
                        {
                            flags |= EXPRFLAG.MEMBERSET;
                        }
                        DebugUtil.Assert(localExpr.LocVarSym.MovedToFieldSym != null);

                        //--------------------------------------------
                        // To the expression tree
                        //     2016/05/07 hirano567@hotmail.co.jp
                        //--------------------------------------------
                        if (localExpr.ToExpressionTree)
                        {
                            expr = ConvertLocalToExpressionTree(
                                localExpr.LocVarSym.MovedToFieldSym,
                                objectExpr);
                        }
                        //--------------------------------------------
                        // Otherwise
                        //--------------------------------------------
                        else
                        {
                            // If the local is not in this display class
                            // it must be in an outer one, which requires exactly one more level of indirection
                            if (anonRewriteInfo.AnonymousMethodSym.ClassSym != localExpr.LocVarSym.MovedToFieldSym.ClassSym)
                            {
                                LOCVARSYM parentLocalSym =
                                    (localExpr.LocVarSym.ParentSym as SCOPESYM).AnonymousScopeInfo.LocalExpr.LocVarSym;
                                objectExpr = MakeFieldAccess(objectExpr, parentLocalSym.Name, flags);
                            }

                            // Now objectExpr is either '<this>' or '<this>.someField' where someField is a flattened
                            // outer $locals that has the hoisted variable
                            DebugUtil.Assert(objectExpr.TypeSym.GetAggregate() == localExpr.LocVarSym.MovedToFieldSym.ClassSym);
                            objectExpr = MakeFieldAccess(objectExpr, localExpr.LocVarSym.MovedToFieldSym, flags);
                            if (localExpr.TypeSym.IsPARAMMODSYM)
                            {
                                objectExpr.TypeSym = Compiler.MainSymbolManager.GetParamModifier(
                                    objectExpr.TypeSym,
                                    (localExpr.TypeSym as PARAMMODSYM).IsOut);
                            }
                            expr = objectExpr;
                        }
                    }
                    return false;

                default:
                    //'RewriteAnonymousMethodFunc' is called to process the entire body of a method that contains
                    //an anonymous method, not just the body of the anonymous method. Before emitting
                    //warnings related to non-virtual calls to virt functions within closures, confirm 
                    //that the access is actually from within the method that is put on the closure.
                    if (anonRewriteInfo.AnonymousMethodSym.IsAnonymous)
                    {
                        CheckForNonvirtualAccessFromClosure(expr);
                    }
                    break;
            }

            return true;
        } // RewriteAnonymousMethodFunc

        //------------------------------------------------------------
        // FUNCBREC.MoveLocalToField (1)
        //
        /// <summary>
        /// Using a LOCVARSYM as a template, construct a MEMBVARSYM in cls
        /// Will automatically resolve duplicates
        /// (since 2 locals in different scopes can have the same name)
        /// </summary>
        /// <param name="localSym"></param>
        /// <param name="destAggSym"></param>
        /// <param name="substTypeVariables"></param>
        /// <param name="allowDupNames"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private MEMBVARSYM MoveLocalToField(
            LOCVARSYM localSym,
            AGGSYM destAggSym,
            TypeArray substTypeVariables,
            bool allowDupNames,             // = true
            string memberName)              // = null
        {
            MEMBVARSYM rval = null;

            if (memberName == null)
            {
                // If this guy has already been moved, keep the same name
                if (localSym.MovedToFieldSym != null)
                {
                    DebugUtil.Assert(localSym.TypeSym.IsFabricated() || localSym.LocSlotInfo.IsParameter);
                    memberName = localSym.MovedToFieldSym.Name;
                }
                else if (localSym.IsThis)
                {
                    memberName = compiler.NameManager.GetPredefinedName(PREDEFNAME.HOISTEDTHIS);
                }
                else if (allowDupNames && !localSym.IsCompilerGenerated)
                {
                    // We don't want to re-mangle an already mangled name
                    memberName = CreateSpecialName(SpecialNameKindEnum.HoistedIteratorLocal, localSym.Name);
                }
                else
                {
                    memberName = localSym.Name;
                }
            }

            // The name should not already exist in the class
            DebugUtil.Assert(null == compiler.MainSymbolManager.LookupAggMember(memberName, destAggSym, SYMBMASK.ALL));
            // The class that we move this to should be completely compiler-generated
            DebugUtil.Assert(destAggSym.IsFabricated);

            rval = compiler.MainSymbolManager.CreateMembVar(memberName, destAggSym, destAggSym.DeclOnly());
            rval.Access = ACCESS.PUBLIC;
            rval.IsReferenced = rval.IsAssigned = true;

            if (TypeArray.Size(substTypeVariables) > 0)
            {
                rval.TypeSym = compiler.MainSymbolManager.SubstType(
                    localSym.TypeSym,
                    (TypeArray)null,
                    substTypeVariables,
                    SubstTypeFlagsEnum.NormNone);
            }
            else
            {
                rval.TypeSym = localSym.TypeSym;
            }

            if (localSym.MovedToFieldSym == null)
            {
                localSym.MovedToFieldSym = rval;
            }
            DebugUtil.Assert(!localSym.IsAssumedPinned());
            return rval;
        }

        //------------------------------------------------------------
        // FUNCBREC.MoveLocalToField (2)
        //
        /// <summary></summary>
        /// <param name="localSym"></param>
        /// <param name="expr"></param>
        /// <param name="substTypeVars"></param>
        /// <param name="allowDupNames"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private MEMBVARSYM MoveLocalToField(
            LOCVARSYM localSym,
            EXPR expr,
            TypeArray substTypeVars,
            bool allowDupNames)	// = true
        {
            return MoveLocalToField(
                localSym,
                (expr.TypeSym as AGGTYPESYM).GetAggregate(),
                substTypeVars,
                allowDupNames,
                null);
        }

        //------------------------------------------------------------
        // FUNCBREC.CorrectAnonMethScope
        //
        /// <summary>
        /// Only call this after binding the outermost block
        /// and pass in the scope symbol for that block
        /// This will push all anonymous methods in this method
        /// above that down to the given scope.
        /// </summary>
        /// <param name="trueOutermostScope"></param>
        //------------------------------------------------------------
        private void CorrectAnonMethScope(SCOPESYM trueOutermostScope)
        {
            AnonMethInfo pami =
                this.currentAnonymousMethodInfo != null ?
                    this.currentAnonymousMethodInfo.ChildInfo : this.firstAnonymousMethodInfo;
            for (; pami != null; pami = pami.NextInfo)
            {
                if (pami.ScopeSym == this.OuterScopeSym ||
                    pami.ScopeSym.NestingOrder < trueOutermostScope.NestingOrder)
                {
                    pami.ScopeSym = trueOutermostScope;
                }
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.SubstAndTestLocalUsedInAnon
        //
        /// <summary>
        /// Returns true if any local (besides <this>) have fHoistForAnonMeth set.
        /// Also for locals not used in anonymous methods (meaning they don't need to be hoisted)
        /// We must perform substitution on their types
        /// to go from method-based type parameters to class-based ones
        /// </summary>
        /// <param name="scopeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool SubstAndTestLocalUsedInAnon(SCOPESYM scopeSym)
        {
            bool hasHoistedLocal = false;

            for (SYM child = scopeSym.FirstChildSym; child != null; child = child.NextSym)
            {
                if (child.IsLOCVARSYM)
                {
                    if ((child as LOCVARSYM).HoistForAnonMeth)
                    {
                        if (!hasHoistedLocal && !(child as LOCVARSYM).IsThis)
                        {
                            if (TypeArray.Size(this.classTypeVariablesForMethod) > 0)
                            {
                                hasHoistedLocal = true;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else if (TypeArray.Size(this.classTypeVariablesForMethod) > 0)
                    {
                        (child as LOCVARSYM).TypeSym = compiler.MainSymbolManager.SubstType(
                            (child as LOCVARSYM).TypeSym,
                            (TypeArray)null,
                            this.classTypeVariablesForMethod,
                            SubstTypeFlagsEnum.NormNone);
                    }
                }
            }

            return hasHoistedLocal;
        }

        //------------------------------------------------------------
        // FUNCBREC.RewriteLocalsBlock
        //
        /// <summary>
        /// Recursively rewrites a block and hoists any locals at this scope if needed
        /// Also creates and initializes the local cached delegates for the Anonymous Methods at this
        /// block/scope level
        /// </summary>
        /// <param name="blockExpr"></param>
        /// <param name="anonRewriteInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR RewriteLocalsBlock(EXPRBLOCK blockExpr, AnonymousMethodRewriteInfo anonRewriteInfo)
        {
            // Set needsLocals to true if we need a local display class for hoisted locals in this scope
            bool needsLocals = false;

            // If we hoisted <this>, but we're not the outer-most block,
            // clear the hoisting so sibling blocks don't get confused
            bool clearHoistedThis = false;
            SCOPESYM scopeSym = blockExpr.ScopeSym;

            // All of the expressions stuffed into this list
            // go before any user code in this block, but be careful because
            // they are added AFTER rewriting the block, they must be created using proper
            // <this> pointers and field references!
            EXPRSTMT list = null;
            LOCVARSYM localSym = null;
            StatementListBuilder builder = new StatementListBuilder();	// bldr(&list);
            EXPRSTMT initThisFieldStmtExpr = null;

            if (scopeSym != null)
            {
                // figure out if there are any locals that need to be hoisted in this scope
                needsLocals = SubstAndTestLocalUsedInAnon(scopeSym);

                // if there's no inner anonymous method, we have no need for a local display class
                DebugUtil.Assert(anonRewriteInfo.FirstAnonymousMethodInfo != null || !needsLocals);

                if (anonRewriteInfo.FirstAnonymousMethodInfo != null)
                {
                    // Set if this is the outer-most block and so we need to check for used arguments
                    bool alsoCheckArgsScope = (blockExpr.OwingBlockExpr == null);

                    if (!needsLocals && alsoCheckArgsScope)
                    {
                        // this is the outermost scope so we also have to check for usage of arguments
                        needsLocals = SubstAndTestLocalUsedInAnon(anonRewriteInfo.ArgumentsScopeSym);
                    }

                    // Set setLocals to true if !needsLocals, but we have an Anonymous Method at this scope
                    // This should only happen if this is the outermost block (meaning we couldn't cache any higher up)
                    bool setLocals = false;
                    bool usesThis = false; // Do any of these methods need a <this> pointer?
                    AnonMethInfo anonMethInfo = null;

                    for (anonMethInfo = anonRewriteInfo.FirstAnonymousMethodInfo;
                        anonMethInfo != null;
                        anonMethInfo = anonMethInfo.NextInfo)
                    {
                        DebugUtil.Assert(anonMethInfo.Seen);

                        if (anonMethInfo.ScopeSym != scopeSym)
                        {
                            continue;
                        }

                        if (!anonMethInfo.UsesLocals &&
                            !anonMethInfo.UsesThis &&
                            anonRewriteInfo.AnonymousMethodSym.TypeVariables.Count == 0)
                        {
                            DebugUtil.Assert(blockExpr.OwingBlockExpr == null);
                            // if it really doesn't use anything, we should have pushed top the outermost block

                            // We're going to cache this anonymous method as a static member
                            AGGTYPESYM delegateAts = compiler.MainSymbolManager.SubstType(
                                anonMethInfo.DelegateTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;

                            string spName = CreateSpecialName(SpecialNameKindEnum.CachedDelegateInstance, null);
                            MEMBVARSYM cachedMembSym = compiler.MainSymbolManager.CreateMembVar(spName, null, null);
                            EXPRFIELD fieldExpr = NewExpr(null, EXPRKIND.FIELD, delegateAts) as EXPRFIELD;
#if DEBUG
                            //DebugOnly(fieldExpr.CheckedMarshalByRef = true);
                            fieldExpr.CheckedMarshalByRef = true;
#endif
                            cachedMembSym.ParentSym = anonRewriteInfo.AnonymousMethodSym.ClassSym;
                            cachedMembSym.ContainingAggDeclSym = anonRewriteInfo.AnonymousMethodSym.ContainingAggDeclSym;
                            cachedMembSym.TypeSym = delegateAts;
                            cachedMembSym.IsStatic = true;
                            cachedMembSym.IsReferenced = true;
                            cachedMembSym.IsAssigned = true;
                            cachedMembSym.IsFabricated = true;
                            cachedMembSym.Access = ACCESS.PRIVATE;

                            fieldExpr.FieldWithType.Set(
                                cachedMembSym,
                                anonRewriteInfo.AnonymousMethodSym.ClassSym.GetThisType());
                            fieldExpr.ObjectExpr = null;
                            fieldExpr.Flags = EXPRFLAG.LVALUE;

                            anonMethInfo.DelegateCacheExpr = fieldExpr;
                        }
                        else if (anonMethInfo.ParametersScopeSym.ParentSym != anonMethInfo.ScopeSym)
                        {
                            AGGTYPESYM delegateAts = compiler.MainSymbolManager.SubstType(
                                anonMethInfo.DelegateTypeSym,
                                (TypeArray)null,
                                this.classTypeVariablesForMethod,
                                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;

                            string spName = CreateSpecialName(SpecialNameKindEnum.CachedDelegateInstance, null);
                            localSym = compiler.LocalSymbolManager.CreateLocalSym(
                                SYMKIND.LOCVARSYM,
                                spName,
                                scopeSym) as LOCVARSYM;

                            localSym.IsCompilerGenerated = true;
                            localSym.LocSlotInfo.HasInit = true;
                            localSym.LocSlotInfo.IsReferenced = true;
                            localSym.LocSlotInfo.IsUsed = true;
                            localSym.TypeSym = delegateAts;

                            // Init it to null
                            builder.Add(MakeAssignment(
                                anonMethInfo.DelegateCacheExpr = MakeLocal(null, localSym, true),
                                BindNull(null)));
                        } // if (!anonMethInfo.UsesLocals &&

                        if (!needsLocals && !setLocals)
                        {
                            DebugUtil.Assert(blockExpr.OwingBlockExpr == null);
                            setLocals = true;
                        }

                        // Keep track if any of the anonymous methods need an outer <this> pointer
                        usesThis |= anonMethInfo.UsesThis;
                    } // for (anonMethInfo = anonRewriteInfo.FirstAnonymousMethodInfo;

                    if (needsLocals || setLocals)
                    {
                        //scopeSym.AnonymousScopeInfo = (AnonScopeInfo *)allocator.Alloc(sizeof(AnonScopeInfo));
                        scopeSym.AnonymousScopeInfo = new AnonScopeInfo();
                        scopeSym.AnonymousScopeInfo.FirstAnonyousMethodInfo = anonRewriteInfo.FirstAnonymousMethodInfo;
                        if (needsLocals)
                        {
                            // Create the class and $locals for the hoisted variables and instance anonymous methods
                            scopeSym.AnonymousScopeInfo.HoistedAggSym = compiler.ClsDeclRec.CreateAnonymousMethodClass(
                                anonRewriteInfo.AnonymousMethodSym);
                            string spName = CreateSpecialName(SpecialNameKindEnum.DisplayClassInstance, null);

                            AGGTYPESYM localsAts = compiler.MainSymbolManager.SubstType(
                                scopeSym.AnonymousScopeInfo.HoistedAggSym.GetThisType(),
                                compiler.MainSymbolManager.ConcatParams(
                                    anonRewriteInfo.AnonymousMethodSym.ClassSym.AllTypeVariables,
                                    anonRewriteInfo.AnonymousMethodSym.TypeVariables),
                                    null,
                                    SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;

                            localSym = compiler.LocalSymbolManager.CreateLocalSym(
                                SYMKIND.LOCVARSYM,
                                spName,
                                scopeSym) as LOCVARSYM;

                            localSym.IsCompilerGenerated = true;
                            localSym.LocSlotInfo.HasInit = true;
                            localSym.LocSlotInfo.IsReferenced = true;
                            localSym.LocSlotInfo.IsUsed = true;
                            localSym.TypeSym = localsAts;
                            scopeSym.AnonymousScopeInfo.LocalExpr = MakeLocal(null, localSym, true);

                            // If this is the outermethod, then this.classTypeVariablesForMethod isn't set yet
                            // and we can't just set it here, because there's still EXPR's in the outer method to process
                            TypeArray nestedClassTypeVariables = this.classTypeVariablesForMethod;
                            if (nestedClassTypeVariables == null)
                            {
                                AGGSYM cls = scopeSym.AnonymousScopeInfo.HoistedAggSym;
                                // loop 'outwards' until we find the outermost fabricated type
                                // then we know it's typeVarsThis must match the containing method's typeVars
                                // which are the ones we want to subsitite with
                                while ((cls.ParentBagSym as AGGSYM).IsFabricated)
                                {
                                    cls = cls.ParentBagSym as AGGSYM;
                                }
                                nestedClassTypeVariables = cls.TypeVariables;
                            }

                            // This will add the .ctor call and 'flatten' any outer locals
                            InitAnonDelegateLocals(scopeSym, ref builder, nestedClassTypeVariables);

                            // Move the locals to the new class
                            //SYM ** prevChild = &scopeSym.FirstChildSym;
                            SYM firstSym = null;
                            SYM currentSym = null;

                            for (SYM child = scopeSym.FirstChildSym; child != null; child = child.NextSym)
                            {
                                if (child.IsLOCVARSYM && (child as LOCVARSYM).HoistForAnonMeth)
                                {
                                    MEMBVARSYM fieldSym = MoveLocalToField(
                                        child as LOCVARSYM,
                                        scopeSym.AnonymousScopeInfo.HoistedAggSym,
                                        nestedClassTypeVariables,
                                        false,
                                        null);
                                    if (!(child as LOCVARSYM).IsCatch)
                                    {
                                        // By not updating prevChild we're effectively orphaning these
                                        continue;
                                    }
                                    // Catch variables need to be copied in manually (and left in the user's class)
                                    builder.Add(MakeAssignment(
                                        MakeFieldAccess(scopeSym.AnonymousScopeInfo.LocalExpr, fieldSym, 0),
                                        MakeLocal(null, child as LOCVARSYM, false)));
                                }

                                //*prevChild = child;
                                //prevChild = &child.nextChild;
                                if (firstSym == null)
                                {
                                    firstSym = currentSym = child;
                                }
                                else
                                {
                                    currentSym.NextSym = child;
                                    currentSym = child;
                                }
                                DebugUtil.Assert(child.Name == null || child.NextSameNameSym == null);
                            }

                            //*prevChild = null;
                            //scopeSym.psymAttachChild = prevChild;
                            scopeSym.FirstChildSym = firstSym;
                            scopeSym.LastChildSym = currentSym;

                            scopeSym.AnonymousScopeInfo.LocalExpr.TypeSym = localsAts;

                            if (alsoCheckArgsScope)
                            {
                                // this is the outermost block so we also have to copy used arguments

                                for (SYM child = anonRewriteInfo.ArgumentsScopeSym.FirstChildSym;
                                    child != null;
                                    child = child.NextSym)
                                {
                                    if (child.IsLOCVARSYM && (child as LOCVARSYM).HoistForAnonMeth)
                                    {
                                        DebugUtil.Assert((child as LOCVARSYM).LocSlotInfo.IsParameter);

                                        localSym = child as LOCVARSYM;
                                        MEMBVARSYM fieldSym = MoveLocalToField(
                                            localSym,
                                            scopeSym.AnonymousScopeInfo.HoistedAggSym,
                                            nestedClassTypeVariables,
                                            false,
                                            null);
                                        EXPRSTMT initStmtExpr = MakeAssignment(
                                            MakeFieldAccess(scopeSym.AnonymousScopeInfo.LocalExpr, fieldSym, 0),
                                            MakeLocal(null, localSym, false));
                                        if (localSym.IsThis)
                                        {
                                            DebugUtil.Assert(initThisFieldStmtExpr == null);
                                            initThisFieldStmtExpr = initStmtExpr;
                                        }
                                        else
                                        {
                                            builder.Add(initStmtExpr);
                                        }
                                    }
                                }

                                anonRewriteInfo.ArgumentsScopeSym.AnonymousScopeInfo = scopeSym.AnonymousScopeInfo;
                            }

                            if (this.outerThisPointerSym != null &&
                                usesThis &&
                                outerThisPointerSym.MovedToFieldSym == null)
                            {
                                DebugUtil.Assert(this.outerThisPointerSym.HoistForAnonMeth);

                                // No outer block hoisted <this>
                                // (most likely because they didn't have any locals to hoist).
                                // Hoist <this> for the rewrite of this scope.
                                // The hoisting then gets cleared after the rewrite
                                // (so sibling scopes don't use our hoisting).

                                clearHoistedThis = true;
                                DebugUtil.Assert(this.outerThisPointerSym.LocSlotInfo.IsParameter);

                                MEMBVARSYM fieldSym = MoveLocalToField(
                                    this.outerThisPointerSym,
                                    scopeSym.AnonymousScopeInfo.HoistedAggSym,
                                    null,
                                    false,
                                    null);
                                builder.Add(MakeAssignment(
                                    MakeFieldAccess(scopeSym.AnonymousScopeInfo.LocalExpr, fieldSym, 0),
                                    BindThisImplicit(null)));
                                (this.outerThisPointerSym.ParentSym as SCOPESYM).AnonymousScopeInfo = scopeSym.AnonymousScopeInfo;
                            }
                        }
                        else
                        {
                            // Use the this pointer and class for this method
                            scopeSym.AnonymousScopeInfo.HoistedAggSym = anonRewriteInfo.AnonymousMethodSym.ClassSym;
                            // This is sometimes null (for static methods)
                            scopeSym.AnonymousScopeInfo.LocalExpr = BindThisImplicit(null) as EXPRLOCAL;
                        }
                    } // if (needsLocals || setLocals)

                    // Sets the anonymous method parent to be the newly generated class or pAMSym's class.
                    for (anonMethInfo = anonRewriteInfo.FirstAnonymousMethodInfo;
                        anonMethInfo != null;
                        anonMethInfo = anonMethInfo.NextInfo)
                    {
                        DebugUtil.Assert(anonMethInfo.Seen);
                        if (anonMethInfo.ScopeSym != scopeSym)
                        {
                            continue;
                        }

                        DebugUtil.Assert(anonMethInfo.MethodSym == null);
                        if (anonMethInfo.UsesLocals)
                        {
                            // We should have created a locals class!
                            DebugUtil.Assert(scopeSym.AnonymousScopeInfo.HoistedAggSym.IsFabricated);

                            // Need to attach to this locals class.
                            anonMethInfo.MethodSym = compiler.MainSymbolManager.CreateMethod(
                                anonMethInfo.Name,
                                scopeSym.AnonymousScopeInfo.HoistedAggSym,
                                scopeSym.AnonymousScopeInfo.HoistedAggSym.DeclOnly());
                            anonMethInfo.MethodSym.MethodKind = MethodKindEnum.Anonymous;
                            anonMethInfo.MethodSym.Access = ACCESS.PUBLIC;
                            anonMethInfo.MethodSym.TypeVariables = BSYMMGR.EmptyTypeArray;
                            anonMethInfo.ThisPointerSym.TypeSym = scopeSym.AnonymousScopeInfo.HoistedAggSym.GetThisType();
                        }
                        else
                        {
                            // Need to attach to the next outer method's class.
                            if (anonRewriteInfo.AnonymousMethodSym.ClassSym.IsFabricated)
                            {
                                anonMethInfo.MethodSym = compiler.MainSymbolManager.CreateMethod(
                                    anonMethInfo.Name,
                                    anonRewriteInfo.AnonymousMethodSym.ClassSym,
                                    anonRewriteInfo.AnonymousMethodSym.ClassSym.DeclOnly());
                                // Type variables were moved to the locals class.
                                anonMethInfo.MethodSym.TypeVariables = BSYMMGR.EmptyTypeArray;
                            }
                            else
                            {
                                // We don't want to add it to the child list or symbol table.
                                anonMethInfo.MethodSym = compiler.MainSymbolManager.CreateMethod(
                                    anonMethInfo.Name,
                                    null,
                                    null);
                                anonMethInfo.MethodSym.ParentSym = anonRewriteInfo.AnonymousMethodSym.ClassSym;
                                anonMethInfo.MethodSym.ContainingAggDeclSym
                                    = anonRewriteInfo.AnonymousMethodSym.ContainingDeclaration();
                                anonMethInfo.MethodSym.TypeVariables = anonRewriteInfo.AnonymousMethodSym.TypeVariables;
                            }
                            anonMethInfo.MethodSym.MethodKind = MethodKindEnum.Anonymous;
                            anonMethInfo.MethodSym.Access = ACCESS.PRIVATE;
                            anonMethInfo.MethodSym.IsStatic = !anonMethInfo.UsesThis;
                            anonMethInfo.ThisPointerSym.TypeSym
                                = anonRewriteInfo.AnonymousMethodSym.ClassSym.GetThisType();

                            DebugUtil.Assert(
                                !anonRewriteInfo.AnonymousMethodSym.IsStatic ||
                                anonMethInfo.MethodSym.IsStatic); // if the outer method is static we'd better be!

                            if (anonMethInfo.MethodSym.IsStatic ||
                                anonRewriteInfo.AnonymousMethodSym.ClassSym.IsStruct ||
                                anonMethInfo.IsInCtorPreamble)
                            {
                                DebugUtil.Assert(
                                    !anonMethInfo.IsInCtorPreamble ||
                                    blockExpr.OwingBlockExpr == null);
                                anonMethInfo.MethodSym.IsStatic = true;

                                // remove the nested 'this pointer' because the method is static
                                DebugUtil.Assert(anonMethInfo.ParametersScopeSym.FirstChildSym
                                    == anonMethInfo.ThisPointerSym);
                                anonMethInfo.ParametersScopeSym.RemoveFromChildList(anonMethInfo.ThisPointerSym);

                                anonMethInfo.ThisPointerSym.LocSlotInfo.IsParameter = false;
                                anonMethInfo.ThisPointerSym.LocSlotInfo.HasInit = false;
                            }
                        }
                        anonMethInfo.MethodSym.ParseTreeNode = anonMethInfo.ParseTreeNode;

                        // Find the correct type variables. They're the ones that are on the outermost fabricated type.
                        TypeArray aggTypeArray = anonMethInfo.MethodSym.ClassSym.AllTypeVariables;
                        AGGSYM parentAggSym;
                        if (aggTypeArray.Count > 0 &&
                            (parentAggSym = (aggTypeArray[aggTypeArray.Count - 1].ParentSym as AGGSYM)).IsFabricated)
                        {
                            TypeArray taSubst = parentAggSym.TypeVariables;
                            // All type variables with fabricated parent should belong to the same fabricated class.
                            DebugUtil.Assert(
                                taSubst.Count == aggTypeArray.Count ||
                                taSubst.Count < aggTypeArray.Count &&
                                !(aggTypeArray[aggTypeArray.Count - taSubst.Count - 1].ParentSym as AGGSYM).IsFabricated);

                            anonMethInfo.MethodSym.ReturnTypeSym = compiler.MainSymbolManager.SubstType(
                                anonMethInfo.ReturnTypeSym,
                                (TypeArray)null,
                                taSubst,
                                SubstTypeFlagsEnum.NormNone);
                            anonMethInfo.MethodSym.ParameterTypes = compiler.MainSymbolManager.SubstTypeArray(
                                anonMethInfo.ParameterArray,
                                (TypeArray)null,
                                taSubst,
                                SubstTypeFlagsEnum.NormNone);
                        }
                        else
                        {
                            anonMethInfo.MethodSym.ReturnTypeSym = anonMethInfo.ReturnTypeSym;
                            anonMethInfo.MethodSym.ParameterTypes = anonMethInfo.ParameterArray;
                        }
                    }
                }   // if (anonRewriteInfo.FirstAnonymousMethodInfo != null)
            }   // if (scopeSym != null)

            // Now for the normal rewriting
            //RecurseAndRewriteExprTree((EXPR **)&blockExpr.statements, anonRewriteInfo);
            EXPR tempExpr = blockExpr.StatementsExpr;
            RecurseAndRewriteExprTree(ref tempExpr, anonRewriteInfo);
            blockExpr.StatementsExpr = tempExpr as EXPRSTMT;

            // If we hoisted the this pointer at an inner scope, un-hoist so siblings don't try to use it.
            if (clearHoistedThis)
            {
                this.outerThisPointerSym.MovedToFieldSym = null;
                (this.outerThisPointerSym.ParentSym as SCOPESYM).AnonymousScopeInfo = null;
            }

            // Now combine with the init expressions.
            DebugUtil.Assert(
                (blockExpr.Flags & EXPRFLAG.CTORPREAMBLE) == 0 ||
                blockExpr.OwingBlockExpr != null &&
                blockExpr.OwingBlockExpr.OwingBlockExpr == null);

            // If we're in a ctor that has a preamble (field initializers and/or base ctor invocation),
            // initThisFieldStmtExpr needs to go AFTER the preamble.
            if (initThisFieldStmtExpr == null)
            {
                ;
            }
            else if (
                blockExpr.StatementsExpr != null &&
                blockExpr.StatementsExpr.Kind == EXPRKIND.BLOCK &&
                (blockExpr.StatementsExpr.Flags & EXPRFLAG.CTORPREAMBLE) != 0)
            {
                initThisFieldStmtExpr.NextStatement = blockExpr.StatementsExpr.NextStatement;
                blockExpr.StatementsExpr.NextStatement = initThisFieldStmtExpr;
            }
            else
            {
                builder.Add(initThisFieldStmtExpr);
            }
            builder.Add(blockExpr);

            list = builder.GetList();
            if (list.NextStatement == null)
            {
                DebugUtil.Assert(list.Kind == EXPRKIND.BLOCK);
                return (list as EXPRBLOCK);
            }

            EXPRBLOCK returnBlockExpr = NewExprBlock(null);
            returnBlockExpr.StatementsExpr = list;
            returnBlockExpr.OwingBlockExpr = blockExpr.OwingBlockExpr;
            blockExpr.OwingBlockExpr = returnBlockExpr;
            returnBlockExpr.Flags |= (blockExpr.Flags & EXPRFLAG.NEEDSRET);
            blockExpr.Flags &= ~EXPRFLAG.NEEDSRET;

            return returnBlockExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.RewriteAnonMethExpr
        //
        // This rewrites an Anonymous Methods in 2 ways
        // It recurses into the Anonymous Method code and rewrites that
        // It also transforms the EXPRANONMETH into a real expression something like
        // this pseudo-code: ((cache != NULL ? null : cache = new Delegate(AnonymousMethod)), cache)
        // We use the EK_SEQUENCE expr to prevent some extra dups and branches
        // The generated IL looks something like this:
        //      LDLOC   (cache)
        //      BRTRUE  AlreadySetLabel
        //      LDLOC   ($locals)
        //      LDFTN   AnonymousMethod
        //      NEWOBJ  Delegate
        //      STLOC   (cache)
        //      ; in /o- we get an extraneous "BR AlreadySetLabel" here
        //  AlreadSetLabel:
        //      LDLOC   (cache)
        //
        /// <summary></summary>
        /// <param name="anonMethExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR RewriteAnonMethExpr(EXPRANONMETH anonMethExpr)
        {
            METHSYM delegateMethodSym = anonMethExpr.AnonymousMethodInfo.MethodSym;
            TypeArray outerTypeArray = this.classTypeVariablesForMethod;

            DebugUtil.Assert(delegateMethodSym.ClassSym != null);
            DebugUtil.Assert(
                anonMethExpr.AnonymousMethodInfo.ThisPointerSym.TypeSym
                == delegateMethodSym.ClassSym.GetThisType());

            // If this is the outermost anonymous method,
            // then set the class type variables to use for substitution
            DebugUtil.Assert(
                !delegateMethodSym.ClassSym.IsFabricated ||
                delegateMethodSym.ClassSym.IsNested);
            if (delegateMethodSym.ClassSym.IsFabricated && this.classTypeVariablesForMethod == null)
            {
                DebugUtil.Assert(!delegateMethodSym.ClassSym.GetOuterAgg().IsFabricated);
                this.classTypeVariablesForMethod = delegateMethodSym.ClassSym.TypeVariables;
            }

            // recursively rewrite the inner EXPR tree (and any inner anonymous methods)
            {
                // push in a new this.thisPointerSym
                LOCVARSYM oldThisSym = this.thisPointerSym;
                MEMBVARSYM outerMovedThisSym = null;
                AnonScopeInfo outerAnonScopeInfo = null;

                this.thisPointerSym = anonMethExpr.AnonymousMethodInfo.ThisPointerSym;
                if (this.outerThisPointerSym != null &&
                    this.outerThisPointerSym.HoistForAnonMeth &&
                    !delegateMethodSym.ClassSym.IsFabricated)
                {
                    DebugUtil.Assert(!anonMethExpr.AnonymousMethodInfo.UsesLocals);

                    outerMovedThisSym = this.outerThisPointerSym.MovedToFieldSym;
                    outerAnonScopeInfo = (this.outerThisPointerSym.ParentSym as SCOPESYM).AnonymousScopeInfo;
                    this.outerThisPointerSym.MovedToFieldSym = null;
                    (this.outerThisPointerSym.ParentSym as SCOPESYM).AnonymousScopeInfo = null;
                }

                anonMethExpr.AnonymousMethodInfo.BodyBlockExpr = RewriteAnonDelegateBodies(
                    delegateMethodSym,
                    anonMethExpr.AnonymousMethodInfo.ParametersScopeSym,
                    anonMethExpr.AnonymousMethodInfo.ChildInfo,
                    anonMethExpr.AnonymousMethodInfo.BodyBlockExpr) as EXPRBLOCK;

                // and pop the this.thisPointerSym
                this.thisPointerSym = oldThisSym;
                if (this.outerThisPointerSym != null &&
                    this.outerThisPointerSym.HoistForAnonMeth &&
                    !delegateMethodSym.ClassSym.IsFabricated)
                {
                    this.outerThisPointerSym.MovedToFieldSym = outerMovedThisSym;
                    (this.outerThisPointerSym.ParentSym as SCOPESYM).AnonymousScopeInfo = outerAnonScopeInfo;
                }
            }

            this.classTypeVariablesForMethod = outerTypeArray;

            EXPR objectExpr = null;
            EXPRFUNCPTR funcptrExpr = NewExpr(null, EXPRKIND.FUNCPTR, GetVoidType()) as EXPRFUNCPTR;

            // Now create the cached delegate access/init expression
            // It will replace the EXPRANONMETH so ILGEN will never see this EXPR type
            if (delegateMethodSym.ClassSym.IsFabricated)
            {
                // Anonymous methods get processed/transformed before iterators, so this fabricated class
                // will never be an iterator, and always a display class
                EXPR localExpr = anonMethExpr.AnonymousMethodInfo.ScopeSym.AnonymousScopeInfo.LocalExpr;
                AGGTYPESYM localAts = localExpr.TypeSym as AGGTYPESYM;
                if (delegateMethodSym.ClassSym == localAts.GetAggregate())
                {
                    funcptrExpr.MethWithInst.Set(delegateMethodSym, localAts, BSYMMGR.EmptyTypeArray);
                }
                else
                {
#if DEBUG
                    for (BAGSYM bagTmp = localAts.GetAggregate();
                        bagTmp != delegateMethodSym.ClassSym;
                        bagTmp = bagTmp.ParentBagSym)
                    {
                        if (bagTmp == null)
                        {
                            DebugUtil.VsFail("Expected nested type!");
                            break;
                        }
                    }
#endif
                    funcptrExpr.MethWithInst.Set(
                        delegateMethodSym,
                        compiler.MainSymbolManager.SubstType(
                            delegateMethodSym.ClassSym.GetThisType(),
                            localAts.AllTypeArguments,
                            null,
                            SubstTypeFlagsEnum.NormNone) as AGGTYPESYM,
                        BSYMMGR.EmptyTypeArray);
                    localExpr = BindThisImplicit(null);
                }

                if (delegateMethodSym.IsStatic)
                {
                    funcptrExpr.ObjectExpr = null;
                    objectExpr = BindNull(null);
                }
                else
                {
                    objectExpr = funcptrExpr.ObjectExpr = localExpr;
                }
            }
            else
            {
                // Call on the non-fabricated class.
                DebugUtil.Assert(
                    anonMethExpr.AnonymousMethodInfo.ThisPointerSym.TypeSym
                    == this.parentAggSym.GetThisType());
                DebugUtil.Assert(
                    this.thisPointerSym == null ||
                    anonMethExpr.AnonymousMethodInfo.ThisPointerSym.TypeSym == this.thisPointerSym.TypeSym);

                funcptrExpr.MethWithInst.Set(
                    delegateMethodSym,
                    this.parentAggSym.GetThisType(),
                    this.methodSym.TypeVariables);
                if (delegateMethodSym.IsStatic)
                {
                    funcptrExpr.ObjectExpr = null;
                    objectExpr = BindNull(null);
                }
                else
                {
                    objectExpr = funcptrExpr.ObjectExpr = BindThisImplicit(null);
                    if (this.parentAggSym.IsStruct)
                    {
                        objectExpr = MustCast(objectExpr, GetRequiredPredefinedType(PREDEFTYPE.OBJECT), 0);
                    }
                }
            }

            DebugUtil.Assert(
                funcptrExpr.MethWithInst.MethSym != null &&
                funcptrExpr.MethWithInst.AggTypeSym != null &&
                funcptrExpr.MethWithInst.TypeArguments != null);
            DebugUtil.Assert(
                funcptrExpr.MethWithInst.TypeArguments.Count
                == anonMethExpr.AnonymousMethodInfo.MethodSym.TypeVariables.Count);

            AGGTYPESYM delegateAts = compiler.MainSymbolManager.SubstType(
                anonMethExpr.AnonymousMethodInfo.DelegateTypeSym,
                (TypeArray)null,
                this.classTypeVariablesForMethod,
                SubstTypeFlagsEnum.NormNone) as AGGTYPESYM;
            EXPRCALL callExpr = NewExpr(null, EXPRKIND.CALL, delegateAts) as EXPRCALL;
            callExpr.ArgumentsExpr = NewExprBinop(null, EXPRKIND.LIST, GetVoidType(), objectExpr, funcptrExpr);
            callExpr.ObjectExpr = null;
            callExpr.MethodWithInst.Set(
                FindDelegateCtor(delegateAts, anonMethExpr.TreeNode, true),
                delegateAts,
                null);
            DebugUtil.Assert(callExpr.MethodWithInst.MethSym != null);
            callExpr.Flags |= (EXPRFLAG.NEWOBJCALL | EXPRFLAG.CANTBENULL);

            if (anonMethExpr.AnonymousMethodInfo.DelegateCacheExpr != null)
            {
                DebugUtil.Assert(delegateAts == anonMethExpr.AnonymousMethodInfo.DelegateCacheExpr.TypeSym);

                EXPR compare = NewExprBinop(
                    null,
                    EXPRKIND.NE,
                    GetRequiredPredefinedType(PREDEFTYPE.BOOL),
                    anonMethExpr.AnonymousMethodInfo.DelegateCacheExpr,
                    BindNull(null));

                EXPR temp = NewExprBinop(
                    null,
                    EXPRKIND.ASSG,
                    delegateAts,
                    anonMethExpr.AnonymousMethodInfo.DelegateCacheExpr,
                    callExpr);

                temp.Flags |= EXPRFLAG.ASSGOP;
                temp = NewExprBinop(null, EXPRKIND.BINOP, GetVoidType(), null, temp);
                temp = NewExprBinop(anonMethExpr.TreeNode, EXPRKIND.QMARK, delegateAts, compare, temp);

                //----------------------------------------------------
                // In the case of expression trees, invoke the delegate.
                //     2016/05/06 hirano567@hotmail.co.jp
                //----------------------------------------------------
                if (anonMethExpr.AnonymousMethodInfo.ToExpressionTree)
                {
                    EXPR fieldExpr = anonMethExpr.AnonymousMethodInfo.DelegateCacheExpr;

                    EXPRMEMGRP grpExpr = NewExpr(
                        null,
                        EXPRKIND.MEMGRP,
                        Compiler.MainSymbolManager.MethodGroupTypeSym) as EXPRMEMGRP;
                    grpExpr.Name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE);
                    grpExpr.TypeArguments = BSYMMGR.EmptyTypeArray;
                    grpExpr.SymKind = SYMKIND.METHSYM;
                    grpExpr.ParentTypeSym = fieldExpr.TypeSym;
                    grpExpr.MethPropSym = Compiler.MainSymbolManager.LookupInvokeMeth(
                        (fieldExpr.TypeSym as AGGTYPESYM).GetAggregate());
                    grpExpr.ObjectExpr = fieldExpr;
                    grpExpr.ContainingTypeArray = null;
                    grpExpr.Flags = EXPRFLAG.DELEGATE;

                    EXPR invokeExpr = BindGrpToArgs(null, BindFlagsEnum.RValueRequired, grpExpr, null);

                    return NewExprBinop(
                        anonMethExpr.TreeNode,
                        EXPRKIND.SEQUENCE,
                        delegateAts,
                        temp,
                        invokeExpr);
                }
                //----------------------------------------------------
                // Otherwise, load the delegate.
                //----------------------------------------------------
                else
                {
                    return NewExprBinop(
                        anonMethExpr.TreeNode,
                        EXPRKIND.SEQUENCE,
                        delegateAts,
                        temp,
                        anonMethExpr.AnonymousMethodInfo.DelegateCacheExpr);
                }
            }
            else
            {
                //----------------------------------------------------
                // In the case of expression trees, call the anonymous method.
                //     2016/05/06 hirano567@hotmail.co.jp
                //----------------------------------------------------
                if (anonMethExpr.AnonymousMethodInfo.ToExpressionTree)
                {
                    EXPR locExpr = anonMethExpr.AnonymousMethodInfo.ScopeSym.AnonymousScopeInfo.LocalExpr;
                    MethWithInst mwi = new MethWithInst(
                        delegateMethodSym,
                        locExpr.TypeSym as AGGTYPESYM,
                        BSYMMGR.EmptyTypeArray);

                    return BindToMethod(
                        null,
                        locExpr,
                        mwi,
                        null,
                        MemLookFlagsEnum.UserCallable);
                }
                //----------------------------------------------------
                // Otherwise, create the delegate with the anonymous method.
                //----------------------------------------------------
                else
                {
                    return callExpr;
                }
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.InitAnonDelegateLocals
        //
        /// <summary>
        /// Construct $local and copy in all parameters
        /// Call this for each scope that has hoisted locals
        /// Will add all init expressions to the end of the expr list and pass back out the new end
        /// </summary>
        /// <param name="argScopeSym"></param>
        /// <param name="builder"></param>
        /// <param name="anonMethClasTypeVars"></param>
        //------------------------------------------------------------
        private void InitAnonDelegateLocals(
            SCOPESYM argScopeSym,
            ref StatementListBuilder builder,
            TypeArray anonMethClasTypeVars)
        {
            // If this scope doesn't have any hoisted locals (indicated by a null pasi)
            // or the exprLoc is merely an outer <this> pointer (for anonymous methods
            // hoisted into the user's class or an outer scope display class) there's nothing to do
            if (argScopeSym == null ||
                argScopeSym.AnonymousScopeInfo == null ||
                argScopeSym.AnonymousScopeInfo.LocalExpr == null ||
                (argScopeSym.AnonymousScopeInfo.LocalExpr.Flags & EXPRFLAG.IMPLICITTHIS) != 0)
            {
                DebugUtil.VsFail("Why was InitAnonDelegateLocals called?");
                return;
            }

            EXPRCALL callExpr;
            EXPR objectExpr = null;

            callExpr = CreateConstructorCall(
                null,
                null,
                argScopeSym.AnonymousScopeInfo.LocalExpr.TypeSym as AGGTYPESYM,
                null,
                null,
                MemLookFlagsEnum.NewObj) as EXPRCALL;
            builder.Add(MakeAssignment(argScopeSym.AnonymousScopeInfo.LocalExpr, callExpr));

            // Copy all outer $locals to 'flatten' the derefence time
            // but only if we have a anonymous method that will block
            // accessing the locals directly
            bool hasMethods = false;
            for (AnonMethInfo amInfo = argScopeSym.AnonymousScopeInfo.FirstAnonyousMethodInfo;
                amInfo != null;
                amInfo = amInfo.NextInfo)
            {
                if (amInfo.ScopeSym == argScopeSym)
                {
                    hasMethods = true;
                    break;
                }
            }
            if (hasMethods)
            {
                EXPRLOCAL localExpr = argScopeSym.AnonymousScopeInfo.LocalExpr;
                AGGSYM localAggSym = (localExpr.TypeSym as AGGTYPESYM).GetAggregate();

                // But don't copy the argscope because it is a duplicate of an inner scope
                for (SCOPESYM currentScopeSym = argScopeSym.ParentSym as SCOPESYM;
                    currentScopeSym != null && currentScopeSym.ParentSym != null;
                    currentScopeSym = currentScopeSym.ParentSym as SCOPESYM)
                {
                    if (currentScopeSym.AnonymousScopeInfo == null ||
                        (currentScopeSym.ScopeFlags & SCOPEFLAGS.DELEGATESCOPE) != 0 ||
                        !currentScopeSym.AnonymousScopeInfo.HoistedAggSym.IsFabricated)
                    {
                        continue;
                    }

                    EXPRLOCAL currentLocalExpr = currentScopeSym.AnonymousScopeInfo.LocalExpr;
                    EXPR srcExpr = null;

                    if (currentScopeSym.NestingOrder >= this.OuterScopeSym.NestingOrder)
                    {
                        srcExpr = currentLocalExpr;
                    }
                    else if (objectExpr == null)
                    {
                        // If this.thisPointerSym is not marked as a param, we're in an nested anon method that
                        // is static. Hence there's no more display classes to hoist.
                        if (this.thisPointerSym == null || !this.thisPointerSym.LocSlotInfo.IsParameter)
                        {
                            break;
                        }
                        srcExpr = BindThisImplicit(null);
                        if ((srcExpr.TypeSym as AGGTYPESYM).GetAggregate().IsFabricated)
                        {
                            if (srcExpr.TypeSym.GetAggregate() != currentLocalExpr.TypeSym.GetAggregate())
                            {
                                // skip any extra scopes that exist between my scope
                                // (argScopeSym) and the one associated with my this pointer.
#if DEBUG
                                // verify that we will eventually find the correct hoisted scope
                                bool willFindScope = false;
                                for (SCOPESYM sc = currentScopeSym;
                                    sc != null && sc.ParentSym != null;
                                    sc = sc.ParentSym as SCOPESYM)
                                {
                                    if (sc.AnonymousScopeInfo != null &&
                                        sc.AnonymousScopeInfo.LocalExpr != null &&
                                        sc.AnonymousScopeInfo.LocalExpr.TypeSym.GetAggregate()
                                            == srcExpr.TypeSym.GetAggregate())
                                    {
                                        willFindScope = true;
                                        break;
                                    }
                                }
                                DebugUtil.Assert(willFindScope);
#endif
                                continue;
                            }
                            objectExpr = srcExpr;
                        }
                        else
                        {
                            // The outer anonymous method was optimistically placed on the user's class
                            // so it has no outer display classes that need to be flattened
                            break;
                        }
                    }
                    else
                    {
                        DebugUtil.Assert((objectExpr.TypeSym as AGGTYPESYM).GetAggregate().IsFabricated);

                        if (currentLocalExpr.LocVarSym.MovedToFieldSym == null)
                        {
                            // We're in a nested anonymous method, if the outer display class hasn't already been
                            // hoisted that must mean it's not needed, so ignore it
                            continue;
                        }
                        srcExpr = MakeFieldAccess(
                            objectExpr,
                            currentLocalExpr.LocVarSym.MovedToFieldSym.Name,
                            0);
                        if (srcExpr == null)
                        {
                            // We're in a nested anonymous method, if the outer display class hasn't already been
                            // hoisted into our parent that must mean it's not needed, so ignore it
                            continue;
                        }
                    }

                    // 2 anonymous methods inside two levels of outer anonymous methods
                    // will cause the same field to get hoisted twice
                    string name = null;
                    if (currentLocalExpr.LocVarSym.MovedToFieldSym != null)
                    {
                        name = currentLocalExpr.LocVarSym.MovedToFieldSym.Name;
                    }
                    else if ((currentLocalExpr.Flags & EXPRFLAG.IMPLICITTHIS) != 0)
                    {
                        // Get the name from the original outer local!
                        for (SCOPESYM scp = currentScopeSym.ParentSym as SCOPESYM;
                            scp != null;
                            scp = scp.ParentSym as SCOPESYM)
                        {
                            if (scp.AnonymousScopeInfo != null &&
                                scp.AnonymousScopeInfo.HoistedAggSym == currentScopeSym.AnonymousScopeInfo.HoistedAggSym &&
                                (scp.AnonymousScopeInfo.LocalExpr.Flags & EXPRFLAG.IMPLICITTHIS) == 0)
                            {
                                name = scp.AnonymousScopeInfo.LocalExpr.LocVarSym.Name;
                                break;
                            }
                        }
                        DebugUtil.Assert(name != null);
                    }
                    else
                    {
                        name = currentLocalExpr.LocVarSym.Name;
                    }

                    if (name == null ||
                        compiler.MainSymbolManager.LookupAggMember(name, localAggSym, SYMBMASK.ALL) == null)
                    {
                        MoveLocalToField(
                            currentLocalExpr.LocVarSym,
                            localAggSym,
                            anonMethClasTypeVars,
                            false,
                            name);
                        name = currentLocalExpr.LocVarSym.MovedToFieldSym.Name;

                        EXPR dest = MakeFieldAccess(localExpr, name, 0);
                        DebugUtil.Assert(dest != null);
                        builder.Add(MakeAssignment(dest, srcExpr));
                    }
                }
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.AddAnonMethInfo
        //
        /// <summary>
        /// Add the AnonMethInfo to the list in *ppamiHead.
        /// Note that we always add at the head of the list.
        /// Once we're done with the initial binding, we reverse the list.
        /// This simplifies list maintenance, yet we can still process things in source order.
        /// </summary>
        /// <param name="headInfo"></param>
        /// <param name="info"></param>
        //------------------------------------------------------------
        private void AddAnonMethInfo(ref AnonMethInfo headInfo, AnonMethInfo info)
        {
            DebugUtil.Assert(info != null && !info.Seen);
            info.NextInfo = headInfo;
            headInfo = info;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeFieldAccess (1)
        //
        /// <summary>
        /// ExprFactory routines copied from MSR (todd proebstring et.al.)
        /// </summary>
        /// <param name="objectExpr"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //--------------------------------------------------
        private EXPR MakeFieldAccess(
            EXPR objectExpr,
            string name,
            EXPRFLAG flags) // = 0
        {
            // Return an expression which references a field (1st overload).
            DebugUtil.Assert(name != null);
            DebugUtil.Assert(objectExpr != null);

            AGGTYPESYM ats = objectExpr.TypeSym as AGGTYPESYM;
            MemberLookup mem =new MemberLookup();

            if (!mem.Lookup(
                    compiler,
                    ats,
                    objectExpr,
                    this.parentDeclSym,
                    name,
                    0,
                    MemLookFlagsEnum.UserCallable) ||
                !mem.FirstSym.IsMEMBVARSYM)
            {
                return null;
            }

            DebugUtil.Assert(mem.TypeCount == 1);
            FieldWithType fwt = FieldWithType.Convert(mem.FirstSymWithType);

            EXPRFIELD fieldExpr = NewExpr(null, EXPRKIND.FIELD, fwt.FieldSym.TypeSym) as EXPRFIELD;

#if DEBUG
            fieldExpr.CheckedMarshalByRef = true;
#endif

            fieldExpr.FieldWithType = fwt;
            fieldExpr.ObjectExpr = objectExpr;
            fieldExpr.Flags = flags;

            int jbitPar;
            if (objectExpr != null && (jbitPar = objectExpr.GetOffset()) != 0 && ats.IsStructType())
            {
                (fieldExpr as EXPRFIELD).Offset = jbitPar + FlowChecker.GetIbit(compiler, fwt);
            }
            else
            {
                (fieldExpr as EXPRFIELD).Offset = 0;
            }

            return fieldExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.MakeFieldAccess (2)
        //
        /// <summary></summary>
        /// <param name="objectExpr"></param>
        /// <param name="fieldSym"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //--------------------------------------------------
        private EXPR MakeFieldAccess(
            EXPR objectExpr,
            MEMBVARSYM fieldSym,
            EXPRFLAG flags)	// = 0
        {
            DebugUtil.Assert(objectExpr != null && objectExpr.TypeSym.GetNakedAgg(false) == fieldSym.ClassSym);
            AGGTYPESYM ats = objectExpr.TypeSym.GetNakedType(false) as AGGTYPESYM;

            // Return an expression which references a field.
            EXPRFIELD fieldExpr = NewExpr(
                null,
                EXPRKIND.FIELD,
                compiler.MainSymbolManager.SubstType(
                    fieldSym.TypeSym,
                    ats,
                    null)
                ) as EXPRFIELD;

#if DEBUG
            fieldExpr.CheckedMarshalByRef = true;
#endif

            fieldExpr.FieldWithType.Set(fieldSym, ats);
            fieldExpr.ObjectExpr = objectExpr;
            fieldExpr.Flags = flags;

            int jbitPar;
            if (objectExpr != null &&
                (jbitPar = objectExpr.GetOffset()) != 0 &&
                objectExpr.TypeSym.IsStructType())
            {
                (fieldExpr as EXPRFIELD).Offset = jbitPar + FlowChecker.GetIbit(compiler, fieldExpr.FieldWithType);
            }
            else
            {
                (fieldExpr as EXPRFIELD).Offset = 0;
            }

            return fieldExpr;
        }

        //--------------------------------------------------
        // FUNCBREC.CheckForNonvirtualAccessFromClosure
        //
        /// <summary></summary>
        /// <param name="expr"></param>
        //--------------------------------------------------
        private void CheckForNonvirtualAccessFromClosure(EXPR expr)
        {
            //if this is left around in EE case, ILGENREC::callAsVirt needs to be moved 
            //to a file that is built with the EE.

            EXPR objectExpr = null;
            bool isConstrained = false;
            bool isBaseCall = false;

            switch (expr.Kind)
            {
                case EXPRKIND.FUNCPTR:
                    EXPRFUNCPTR funcptrExpr = expr as EXPRFUNCPTR;
                    if (funcptrExpr.MethWithInst.MethSym.IsVirtual &&
                        (funcptrExpr.MethWithInst.MethSym.ClassSym.IsSealed ||
                        funcptrExpr.ObjectExpr.TypeSym.FundamentalType() != FUNDTYPE.REF ||
                        (funcptrExpr.Flags & EXPRFLAG.BASECALL) != 0))
                    {
                        compiler.Error(
                            funcptrExpr.TreeNode,
                            CSCERRID.WRN_NonVirtualCallFromClosure,
                            new ErrArg(funcptrExpr.MethWithInst.MethSym));
                    }
                    break;

                case EXPRKIND.CALL:
                    EXPRCALL callExpr = expr as EXPRCALL;
                    METHSYM funcSym = callExpr.MethodWithInst.MethSym;
                    objectExpr = callExpr.ObjectExpr;
                    isConstrained = ((callExpr.Flags & EXPRFLAG.CONSTRAINED) != 0);
                    isBaseCall = ((callExpr.Flags & EXPRFLAG.BASECALL) != 0);

                    if (funcSym.IsVirtual &&
                        !isConstrained &&
                        !ILGENREC.CallAsVirtual(funcSym, objectExpr, isBaseCall) &&
                        objectExpr.TypeSym.IsReferenceType())
                    {
                        compiler.Error(
                            callExpr.TreeNode,
                            CSCERRID.WRN_NonVirtualCallFromClosure,
                            new ErrArg(funcSym));
                    }
                    break;

                case EXPRKIND.PROP:
                    EXPRPROP propExpr = expr as EXPRPROP;
                    PROPSYM slotPropSym = propExpr.SlotPropWithType.PropSym;
                    objectExpr = propExpr.ObjectExpr;
                    isConstrained = ((propExpr.Flags & EXPRFLAG.CONSTRAINED) != 0);
                    isBaseCall = ((propExpr.Flags & EXPRFLAG.BASECALL) != 0);

                    MethWithType mwt =
                        (propExpr.GetMethodWithType != null && propExpr.GetMethodWithType.IsNotNull) ?
                        propExpr.GetMethodWithType :
                        propExpr.SetMethodWithType;
                    if (mwt.MethSym.IsVirtual &&
                        !isConstrained &&
                        !ILGENREC.CallAsVirtual(mwt.MethSym, objectExpr, isBaseCall))
                    {
                        compiler.Error(
                            propExpr.TreeNode,
                            CSCERRID.WRN_NonVirtualCallFromClosure,
                            new ErrArg(slotPropSym));
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
