//============================================================================
// LambdaExpressions.cs
//
// 2016/01/23 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //======================================================================
    // class CParser
    //======================================================================
    internal partial class CParser
    {
        //------------------------------------------------------------
        // CParser.SearchCloseParenthesis
        //
        /// <summary></summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SearchCloseParenthesis(ref int index)
        {
            DebugUtil.Assert(index >= 0 && index < this.tokenCount);
            DebugUtil.Assert(this.tokenArray[index].TokenID == TOKENID.OPENPAREN);

            TOKENID tokenID = TOKENID.UNDEFINED;
            int nest = 0;

            ++index;

            while (index < this.tokenCount)
            {
                tokenID = this.tokenArray[index].TokenID;
                if (nest == 0 && tokenID == TOKENID.CLOSEPAREN)
                {
                    return true;
                }
                else if (tokenID == TOKENID.OPENPAREN)
                {
                    ++nest;
                }
                else if (tokenID == TOKENID.CLOSEPAREN)
                {
                    --nest;
                }
                else if (tokenID == TOKENID.ENDFILE)
                {
                    return false;
                }
                ++index;
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.IsLambdaExpression
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsLambdaExpression()
        {
            int index = this.currentTokenIndex;
            if (this.tokenArray[index].TokenID == TOKENID.OPENPAREN)
            {
                if (!SearchCloseParenthesis(ref index))
                {
                    return false;
                }
            }
            ++index;

            if (index < this.tokenCount)
            {
                if (this.tokenArray[index].TokenID == TOKENID.EQUALGREATER)
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.ParseLambdaExpression
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseLambdaExpression(BASENODE parentNode, int closeIndex)
        {
            LAMBDAEXPRNODE lambdaNode
                = AllocNode(NODEKIND.LAMBDAEXPR, parentNode) as LAMBDAEXPRNODE;
            lambdaNode.ArgumentsNode = null;
            lambdaNode.CloseParenIndex = -1;

            //--------------------------------------------------------
            // Parse the signature (1) ( , ...) =>
            //--------------------------------------------------------
            if (CurrentTokenID() == TOKENID.OPENPAREN)
            {
                NextToken();
                CListMaker list = new CListMaker(this);
                int comma = -1;
                int paramStyle = 0;
                // 0:not specified, 1: implicit, 2:explicit, -1: inconsistent

                //----------------------------------------------------
                // Parse each parameter
                //----------------------------------------------------
                while (CurrentTokenID() != TOKENID.CLOSEPAREN)
                {
                    PARAMETERNODE paramNode
                        = AllocNode(NODEKIND.PARAMETER, lambdaNode) as PARAMETERNODE;

                    //------------------------------------------------
                    // implicit-anonymous-function-signature:
                    //     identifier
                    //------------------------------------------------
                    if (PeekToken() == TOKENID.COMMA ||
                        PeekToken() == TOKENID.CLOSEPAREN)
                    {
                        paramNode.NameNode = ParseIdentifier(paramNode);
                        paramNode.TypeNode = AllocNode(
                            NODEKIND.IMPLICITTYPE,
                            lambdaNode) as IMPLICITTYPENODE;

                        switch (paramStyle)
                        {
                            case 0: paramStyle = 1; break;
                            case 1:break;
                            case 2: paramStyle = -1; break;
                            case -1:break;
                            default:
                                DebugUtil.Assert(false);
                                break;
                        }
                    }
                    //------------------------------------------------
                    // explicit-anonymous-function-signature:
                    //     ["ref"|"out"] type identifier
                    //------------------------------------------------
                    else
                    {
                        if (CurrentTokenID() == TOKENID.REF)
                        {
                            paramNode.Flags |= NODEFLAGS.PARMMOD_REF;
                            NextToken();
                        }
                        else if (CurrentTokenID() == TOKENID.OUT)
                        {
                            paramNode.Flags |= NODEFLAGS.PARMMOD_OUT;
                            NextToken();
                        }

                        TYPEBASENODE tbNode = ParseType(paramNode, false);

                        if (tbNode.Kind == NODEKIND.PREDEFINEDTYPE &&
                            (tbNode as PREDEFINEDTYPENODE).Type == PREDEFTYPE.VOID)
                        {
                            Error(CSCERRID.ERR_NoVoidParameter);
                        }
                        else
                        {
                            paramNode.TypeNode = tbNode;
                        }

                        paramNode.NameNode = ParseIdentifier(paramNode);

                        switch (paramStyle)
                        {
                            case 0: paramStyle = 2; break;
                            case 1: paramStyle = -1; break;
                            case 2: break;
                            case -1: break;
                            default:
                                DebugUtil.Assert(false);
                                break;
                        }
                    }
                    list.Add(paramNode, comma);

                    if (CurrentTokenID() != TOKENID.COMMA)
                    {
                        break;
                    }
                    comma = CurrentTokenIndex();
                    NextToken();
                }

                if (paramStyle < 0)
                {
                    Error(CSCERRID.ERR_InconsistentLambdaParameters);
                }

                lambdaNode.ArgumentsNode = list.GetList(lambdaNode);
                Eat(TOKENID.CLOSEPAREN);
            }
            //--------------------------------------------------------
            // Parse the signature (2) p =>
            //--------------------------------------------------------
            else
            {
                PARAMETERNODE paramNode
                    = AllocNode(NODEKIND.PARAMETER, lambdaNode) as PARAMETERNODE;

                if (CurrentTokenID() == TOKENID.IDENTIFIER)
                {
                    paramNode.NameNode = ParseIdentifier(paramNode);
                    paramNode.TypeNode = AllocNode(
                        NODEKIND.IMPLICITTYPE,
                        lambdaNode) as IMPLICITTYPENODE;
                }
                else
                {
                    Error(
                        CSCERRID.ERR_InvalidExprTerm,
                        new ErrArg(this.tokenArray[this.currentTokenIndex].Name));
                }
                lambdaNode.ArgumentsNode = paramNode;
            }

            //--------------------------------------------------------
            // Parse RHS
            //--------------------------------------------------------
            Eat(TOKENID.EQUALGREATER);
            ParseLambdaExpressionRHS(lambdaNode, closeIndex);

            return lambdaNode;
        }

        //------------------------------------------------------------
        // CParser.ParseLambdaExpressionRHS
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <param name="closeIndex"></param>
        //------------------------------------------------------------
        internal void ParseLambdaExpressionRHS(
            LAMBDAEXPRNODE lambdaNode,
            int closeIndex)
        {
            if (CurrentTokenID() == TOKENID.OPENCURLY)
            {
                lambdaNode.BodyNode = ParseBlock(lambdaNode, closeIndex) as BLOCKNODE;
            }
            else
            {
                lambdaNode.BodyNode
                    = AllocNode(NODEKIND.BLOCK, lambdaNode) as BLOCKNODE;

                EXPRSTMTNODE stmtNode
                    = AllocNode(NODEKIND.RETURN, lambdaNode.BodyNode).AsRETURN;
                lambdaNode.BodyNode.StatementsNode = stmtNode;

                stmtNode.ArgumentsNode = ParseExpression(stmtNode, closeIndex);
                lambdaNode.ExpressionNode = stmtNode.ArgumentsNode;
            }
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //============================================================
        // class FUNCBREC.StateStrage
        //
        /// <summary>
        /// <para>(CSharp\SCComp\LambdaExpressions.cs)</para>
        /// </summary>
        //============================================================
        internal class StateStrage
        {
            internal EXPRBLOCK oldBlockExpr = null;

            internal SCOPESYM oldOuterScopeSym = null;
            internal SCOPESYM oldCurrentScopeSym = null;
            internal SCOPESYM oldInnermostTryScopeSym = null;
            internal SCOPESYM oldInnermostCatchScopeSym = null;
            internal SCOPESYM oldInnermostFinallyScopeSym = null;

            internal LOCVARSYM oldThisPointerSym = null;
            internal LOCVARSYM oldOuterThisPointerSym = null;

            //--------------------------------------------------------
            // FUNCBREC.StateStrage.Store
            //
            /// <summary></summary>
            /// <param name="obj"></param>
            //--------------------------------------------------------
            internal void Store(FUNCBREC obj)
            {
                oldBlockExpr = obj.currentBlockExpr;

                oldOuterScopeSym = obj.OuterScopeSym;
                oldCurrentScopeSym = obj.currentScopeSym;
                oldInnermostTryScopeSym = obj.innermostTryScopeSym;
                oldInnermostCatchScopeSym = obj.innermostCatchScopeSym;
                oldInnermostFinallyScopeSym = obj.innermostFinallyScopeSym;

                oldThisPointerSym = obj.thisPointerSym;
                oldOuterThisPointerSym = obj.outerThisPointerSym;
            }

            //--------------------------------------------------------
            // FUNCBREC.StateStrage.Restore
            //
            /// <summary></summary>
            /// <param name="obj"></param>
            //--------------------------------------------------------
            internal void Restore(FUNCBREC obj)
            {
                obj.outerThisPointerSym = oldOuterThisPointerSym;
                obj.thisPointerSym = oldThisPointerSym;

                obj.innermostFinallyScopeSym = oldInnermostFinallyScopeSym;
                obj.innermostCatchScopeSym = oldInnermostCatchScopeSym;
                obj.innermostTryScopeSym = oldInnermostTryScopeSym;
                obj.currentScopeSym = oldCurrentScopeSym;
                obj.OuterScopeSym = oldOuterScopeSym;

                obj.currentBlockExpr = oldBlockExpr;
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.StoreMembersInCache
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        /// <param name="objSym"></param>
        //------------------------------------------------------------
        internal void StoreMembersInCache(AGGTYPESYM aggTypeSym, EXPR parentExpr)
        {
            if (aggTypeSym == null ||
                !aggTypeSym.IsParameterProcuct)
            {
                return;
            }

            //AGGTYPESYM aggTypeSym = typeSym as AGGTYPESYM;
            AGGSYM aggSym = aggTypeSym.GetAggregate();
            TypeArray ta = new TypeArray();
            ta.Add(aggTypeSym);
            ta = Compiler.MainSymbolManager.AllocParams(ta);

            for (SYM sym = aggSym.FirstChildSym;
                sym != null;
                sym = sym.NextSym)
            {
                ACCESS acc = sym.Access;

                if (acc == ACCESS.PRIVATE ||
                    acc == ACCESS.PROTECTED ||
                    acc == ACCESS.INTERNALPROTECTED)
                {
                    continue;
                }

                EXPR objectExpr = null;
                TYPESYM ts1 = null;
                TYPESYM ts2 = null;

                switch (sym.Kind)
                {
                    case SYMKIND.MEMBVARSYM:
                        ts1 = (sym as MEMBVARSYM).TypeSym;
                        break;

                    case SYMKIND.PROPSYM:
                        ts1 = (sym as PROPSYM).ReturnTypeSym;
                        break;

                    default:
                        continue;
                }
                ts2 = Compiler.MainSymbolManager.SubstType(
                    ts1,
                    aggTypeSym.TypeArguments,
                    null,
                    SubstTypeFlagsEnum.NormAll);
                AGGTYPESYM ats = ts2 as AGGTYPESYM;

                switch (sym.Kind)
                {
                    case SYMKIND.MEMBVARSYM:
                        objectExpr = BindToField(
                            null,
                            parentExpr,
                            new FieldWithType(sym as MEMBVARSYM, aggTypeSym),
                            BindFlagsEnum.RValueRequired);
                        break;

                    case SYMKIND.PROPSYM:
                        objectExpr = BindToProperty(
                            null,
                            parentExpr,
                            new PropWithType(sym as PROPSYM, aggTypeSym),
                            BindFlagsEnum.RValueRequired,
                            null,
                            null);
                        break;

                    default:
                        continue;
                }

                StoreInCache(
                    null,
                    sym.Name,
                    sym,
                    ta,
                    objectExpr,
                    false);

                if (ats != null && ats.IsParameterProcuct)
                {
                    StoreMembersInCache(ats, objectExpr);
                }
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.BindLambdaExpression
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindLambdaExpression(
            LAMBDAEXPRNODE lambdaNode,
            List<PARAMINFO> paramInfoList)
        {
            AnonMethInfo lambdaInfo = null;

            if (this.methodSym == null)
            {
                Compiler.Error(lambdaNode, CSCERRID.ERR_AnonMethNotAllowed);
                CreateNewScope();
            }
            else
            {
                lambdaInfo = new AnonMethInfo();
                lambdaInfo.ParseTreeNode = lambdaNode;
                lambdaInfo.OuterInfo = this.currentAnonymousMethodInfo;

                if (this.methodSym.Name == null)
                {
                    StringBuilder strBuilder = new StringBuilder();
                    MetaDataHelper hlpr = new MetaDataHelper("+");
                    hlpr.GetExplicitImplName(this.methodSym, strBuilder);
                    lambdaInfo.Name = CreateSpecialName(
                        SpecialNameKindEnum.AnonymousMethod,
                        strBuilder.ToString());
                }
                else
                {
                    lambdaInfo.Name = CreateSpecialName(
                        SpecialNameKindEnum.AnonymousMethod,
                        methodSym.Name);
                }

                if (this.currentAnonymousMethodInfo != null)
                {
                    AddAnonMethInfo(ref this.currentAnonymousMethodInfo.ChildInfo, lambdaInfo);
                }
                else
                {
                    AddAnonMethInfo(ref this.firstAnonymousMethodInfo, lambdaInfo);
                }
                lambdaInfo.ScopeSym = this.OuterScopeSym;

                ANONSCOPESYM anonScopeSym = Compiler.LocalSymbolManager.CreateLocalSym(
                    SYMKIND.ANONSCOPESYM,
                    null,
                    this.currentScopeSym) as ANONSCOPESYM;

                lambdaInfo.ParametersScopeSym = Compiler.LocalSymbolManager.CreateLocalSym(
                    SYMKIND.SCOPESYM,
                    null,
                    null) as SCOPESYM;
                lambdaInfo.ParametersScopeSym.ParentSym = this.currentScopeSym;
                lambdaInfo.ParametersScopeSym.NestingOrder = this.currentScopeSym.NestingOrder + 1;
                this.currentScopeSym = lambdaInfo.ParametersScopeSym;
                anonScopeSym.ScopeSym = this.currentScopeSym;

                lambdaInfo.ParametersScopeSym.ScopeFlags |= SCOPEFLAGS.DELEGATESCOPE;
                lambdaInfo.JBitMin = this.uninitedVarCount + 1;

                LOCVARSYM nestedThisSym = AddParam(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.THIS),
                    GetVoidType(),
                    0,
                    this.currentScopeSym);
                nestedThisSym.LocSlotInfo.IsUsed = true;
                nestedThisSym.IsThis = true;
                lambdaInfo.ThisPointerSym = nestedThisSym;
            }

            //--------------------------------------------------------
            // Bind the parameters.
            //--------------------------------------------------------
            TypeArray paramArray = null;
            List<LOCVARSYM> paramList = new List<LOCVARSYM>();
                bool implicitType = false;

            if (paramInfoList != null)
            {
                paramArray = new TypeArray();

                for (int i = 0; i < paramInfoList.Count; ++i)
                {
                    PARAMINFO pInfo = paramInfoList[i];
                    TYPESYM typeSym = pInfo.TypeSym;
                    paramArray.Add(typeSym);

                    if (typeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        implicitType = true;
                    }

                    BASENODE paramNameNode = pInfo.ParameterNode != null ?
                            pInfo.ParameterNode.NameNode :
                            null;

                    LOCVARSYM paramSym = DeclareParam(
                        pInfo.Name,
                        typeSym,
                        0,
                        paramNameNode,
                        this.currentScopeSym);
                    DebugUtil.Assert(paramSym != null);

                    // Because we put the parameters at an inner scope,
                    // we have to put them in the outer cache to be found
                    StoreInCache(
                        paramNameNode,
                        paramSym.Name,
                        paramSym,
                        null,
                        true);
                    paramList.Add(paramSym);

                    EXPR paramExpr = BindToLocal(
                        null,
                        paramSym,
                        BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

                    if (pInfo.IsParameterProcuct)
                    {
                        StoreMembersInCache(typeSym as AGGTYPESYM, paramExpr);
                    }
                }
            } // if (paramInfoList != null)
            else if (lambdaNode.ArgumentsNode != null)
            {
                // The user had some parameters
                DebugUtil.Assert(
                    (lambdaNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_VARARGS) == 0 &&
                    (lambdaNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) == 0);

                // Pass in the outer method as a context
                // so we get it's type parameters (we just re-use it's symbols)
                bool hasParams = false;
                Compiler.ClsDeclRec.DefineParameters(
                    this.ContextForTypeBinding(),
                    lambdaNode.ArgumentsNode,
                    false,
                    ref paramArray,
                    ref hasParams);

                // Now add all of the declared method arguments to the scope
                int p = 0;

                BASENODE node = lambdaNode.ArgumentsNode;
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
                    if (typeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        implicitType = true;
                    }

                    LOCVARSYM paramSym = DeclareParam(
                        paramNode.NameNode.Name,
                        typeSym,
                        0,
                        paramNode,
                        this.currentScopeSym);
                    if (paramSym != null)
                    {
                        // Because we put the parameters at an inner scope,
                        // we have to put them in the outer cache to be found
                        StoreInCache(
                            paramNode,
                            paramSym.Name,
                            paramSym,
                            null,
                            true);
                    }
                    paramList.Add(paramSym);

                    EXPR paramExpr = BindToLocal(
                        null,
                        paramSym,
                        BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

                    AGGTYPESYM ats = typeSym as AGGTYPESYM;
                    if (ats != null && ats.GetAggregate().IsParameterProcuct)
                    {
                        StoreMembersInCache(typeSym as AGGTYPESYM, paramExpr);
                    }
                }
            } // if (lambdaNode.ArgumentsNode != null)

            if (paramArray == null)
            {
                paramArray = BSYMMGR.EmptyTypeArray;
            }
            else
            {
                paramArray = Compiler.MainSymbolManager.AllocParams(paramArray);
            }

            if (lambdaInfo != null)
            {
                lambdaInfo.ParameterArray = paramArray;
            }

            //--------------------------------------------------------
            // Create the EXPRLAMBDAEXPR instance.
            //--------------------------------------------------------
            EXPRLAMBDAEXPR lambdaExpr = NewExpr(
                lambdaNode,
                EXPRKIND.LAMBDAEXPR,
                Compiler.MainSymbolManager.LambdaExpressionSym) as EXPRLAMBDAEXPR;
            lambdaExpr.AnonymousMethodInfo = lambdaInfo;
            lambdaExpr.ParameterList = paramList;
            lambdaExpr.ParameterTypesAreImplicit = implicitType;

            if (lambdaInfo != null)
            {
                lambdaInfo.JBitLim = uninitedVarCount + 1;
            }
            CloseScope();
            return lambdaExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindLambdaExpressionInner
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <param name="anonInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindLambdaExpressionInner(AnonMethInfo anonInfo)
        {
            DebugUtil.Assert(anonInfo != null);

            // Save the scopes so we don't think we're inside anything
            // (because we aren't when inside the AM block)

            AnonMethInfo oldCurrentAnonymousMethodInfo = this.currentAnonymousMethodInfo;
            EXPRBLOCK oldBlockExpr = this.currentBlockExpr;

            SCOPESYM oldOuterScopeSym = this.OuterScopeSym;
            SCOPESYM oldCurrentScopeSym = this.currentScopeSym;
            SCOPESYM outerTryScope = this.innermostTryScopeSym;
            SCOPESYM outerCatchScope = this.innermostCatchScopeSym;
            SCOPESYM outerFinallyScope = this.innermostFinallyScopeSym;

            EXPR lambdaExpr = null;

            try
            {
                this.currentAnonymousMethodInfo = anonInfo;
                this.currentBlockExpr = anonInfo.BodyBlockExpr;

                this.currentScopeSym = anonInfo.ParametersScopeSym;
                SCOPESYM scpSym = anonInfo.ScopeSym;
                while (scpSym != null && scpSym.NestingOrder > 0)
                {
                    scpSym = scpSym.ParentSym as SCOPESYM;
                }
                this.OuterScopeSym = scpSym;

                this.innermostCatchScopeSym = this.OuterScopeSym;
                this.innermostTryScopeSym = this.OuterScopeSym;
                this.innermostFinallyScopeSym = this.OuterScopeSym;

                lambdaExpr = BindLambdaExpressionInnerCore(anonInfo);
            }
            finally
            {
                // Restore everything
                this.innermostTryScopeSym = outerTryScope;
                this.innermostCatchScopeSym = outerCatchScope;
                this.innermostFinallyScopeSym = outerFinallyScope;

                this.OuterScopeSym = oldOuterScopeSym;
                this.currentScopeSym = oldCurrentScopeSym;

                this.currentBlockExpr = oldBlockExpr;
                this.currentAnonymousMethodInfo = oldCurrentAnonymousMethodInfo;
            }
            return lambdaExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindLambdaExpressionInnerCore
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <param name="anonInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindLambdaExpressionInnerCore(AnonMethInfo anonInfo)
        {
            LAMBDAEXPRNODE lambdaNode = anonInfo.ParseTreeNode as LAMBDAEXPRNODE;
            EXPR lambdaExpr = null;

            //DebugUtil.Assert(this.currentAnonymousMethodInfo == anonInfo.OuterInfo);
            this.currentAnonymousMethodInfo = anonInfo;
            CreateNewScope();

            //--------------------------------------------------------
            // Bind the block.
            //--------------------------------------------------------
            //anonInfo.BodyBlockExpr
            anonInfo.BodyBlockExpr = BindMethOrPropBody(lambdaNode.BodyNode as BLOCKNODE);

            //--------------------------------------------------------
            // Check reachability
            //--------------------------------------------------------
            ReachabilityChecker reach = new ReachabilityChecker(Compiler);
            reach.SetReachability(anonInfo.BodyBlockExpr, true);
            anonInfo.HasReturnAsLeave = reach.HasReturnAsLeave;

            //--------------------------------------------------------
            // Convert the return values
            //--------------------------------------------------------
            EXPR expr2 = anonInfo.ReturnExprList;
            TYPESYM returnTypeSym = anonInfo.ReturnTypeSym;
            bool returnResult = true;
            bool reportErrors = false;
            bool implicitReturnType = (
                returnTypeSym == null ||
                returnTypeSym.IsIMPLICITTYPESYM);

            while (expr2 != null)
            {
                EXPR operandExpr;
                if (expr2.Kind == EXPRKIND.LIST)
                {
                    operandExpr = expr2.AsBIN.Operand1;
                    expr2 = expr2.AsBIN.Operand2;
                }
                else
                {
                    operandExpr = expr2;
                    expr2 = null;
                }
                EXPRRETURN returnExpr = operandExpr as EXPRRETURN;

                if (implicitReturnType)
                {
                    anonInfo.ReturnTypeSym
                        = returnTypeSym
                        = returnExpr.ObjectExpr.TypeSym;
                    continue;
                }

                EXPR retObjExpr = returnExpr.ObjectExpr;

                if (retObjExpr == null ||
                    !CanConvert(retObjExpr, returnTypeSym, 0))
                {
                    if (implicitReturnType &&
                        CanConvert(returnTypeSym, retObjExpr.TypeSym, 0))
                    {
                        anonInfo.ReturnTypeSym
                            = returnTypeSym
                            = retObjExpr.TypeSym;
                    }
                    else
                    {
                        if (!reportErrors)
                        {
                            return null;
                        }
                        if (returnResult)
                        {
                            compiler.Error(
                                treeNode,
                                CSCERRID.ERR_CantConvAnonMethReturns,
                                new ErrArg("anonymous method Block"),
                                new ErrArg(anonInfo.DelegateTypeSym));
                        }
                        if ((returnExpr as EXPRRETURN).ObjectExpr != null)
                        {
                            MustConvert(
                                (returnExpr as EXPRRETURN).ObjectExpr,
                                anonInfo.ReturnTypeSym,
                                0);
                        }
                        else
                        {
                            compiler.Error(
                                treeNode,
                                CSCERRID.ERR_RetObjectRequired,
                                new ErrArg(anonInfo.ReturnTypeSym));
                        }
                        returnResult = false;
                    }
                }
            }

            DebugUtil.Assert(this.currentAnonymousMethodInfo == anonInfo);
            //this.currentAnonymousMethodInfo = anonInfo.OuterInfo;

            anonInfo.BodyBlockExpr.OwingBlockExpr = null;

            CloseScope();

            lambdaExpr = NewExpr(
                lambdaNode,
                EXPRKIND.LAMBDAEXPR,
                Compiler.MainSymbolManager.LambdaExpressionSym);
            (lambdaExpr as EXPRLAMBDAEXPR).AnonymousMethodInfo = anonInfo;

            return lambdaExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindLambdaExpression2
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindLambdaExpression2(
            LAMBDAEXPRNODE lambdaNode,
            List<PARAMINFO> paramInfoList)
        {
            AnonMethInfo lambdaInfo = null;

            if (this.methodSym == null)
            {
                Compiler.Error(lambdaNode, CSCERRID.ERR_AnonMethNotAllowed);
                CreateNewScope();
            }
            else
            {
                lambdaInfo = new AnonMethInfo();
                lambdaInfo.ParseTreeNode = lambdaNode;
                lambdaInfo.OuterInfo = this.currentAnonymousMethodInfo;

                if (this.methodSym.Name == null)
                {
                    StringBuilder strBuilder = new StringBuilder();
                    MetaDataHelper hlpr = new MetaDataHelper("+");
                    hlpr.GetExplicitImplName(this.methodSym, strBuilder);
                    lambdaInfo.Name = CreateSpecialName(
                        SpecialNameKindEnum.AnonymousMethod,
                        strBuilder.ToString());
                }
                else
                {
                    lambdaInfo.Name = CreateSpecialName(
                        SpecialNameKindEnum.AnonymousMethod,
                        methodSym.Name);
                }

                if (this.currentAnonymousMethodInfo != null)
                {
                    AddAnonMethInfo(ref this.currentAnonymousMethodInfo.ChildInfo, lambdaInfo);
                }
                else
                {
                    AddAnonMethInfo(ref this.firstAnonymousMethodInfo, lambdaInfo);
                }
                lambdaInfo.ScopeSym = this.OuterScopeSym;

                ANONSCOPESYM anonScopeSym = Compiler.LocalSymbolManager.CreateLocalSym(
                    SYMKIND.ANONSCOPESYM,
                    null,
                    this.currentScopeSym) as ANONSCOPESYM;

                lambdaInfo.ParametersScopeSym = Compiler.LocalSymbolManager.CreateLocalSym(
                    SYMKIND.SCOPESYM,
                    null,
                    null) as SCOPESYM;
                lambdaInfo.ParametersScopeSym.ParentSym = this.currentScopeSym;
                lambdaInfo.ParametersScopeSym.NestingOrder = this.currentScopeSym.NestingOrder + 1;
                this.currentScopeSym = lambdaInfo.ParametersScopeSym;
                anonScopeSym.ScopeSym = this.currentScopeSym;

                lambdaInfo.ParametersScopeSym.ScopeFlags |= SCOPEFLAGS.DELEGATESCOPE;
                lambdaInfo.JBitMin = this.uninitedVarCount + 1;

                LOCVARSYM nestedThisSym = AddParam(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.THIS),
                    GetVoidType(),
                    0,
                    this.currentScopeSym);
                nestedThisSym.LocSlotInfo.IsUsed = true;
                nestedThisSym.IsThis = true;
                lambdaInfo.ThisPointerSym = nestedThisSym;
            }

            //--------------------------------------------------------
            // Bind the parameters.
            //--------------------------------------------------------
            TypeArray paramArray = null;
            List<LOCVARSYM> paramList = new List<LOCVARSYM>();
            bool implicitType = false;

            if (paramInfoList != null)
            {
                paramArray = new TypeArray();

                for (int i = 0; i < paramInfoList.Count; ++i)
                {
                    PARAMINFO pInfo = paramInfoList[i];
                    TYPESYM typeSym = pInfo.TypeSym;
                    paramArray.Add(typeSym);

                    if (typeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        implicitType = true;
                    }

                    LOCVARSYM paramSym = DeclareParam(
                        pInfo.Name,
                        typeSym,
                        0,
                        pInfo.ParameterNode != null ? pInfo.ParameterNode.NameNode : null,
                        this.currentScopeSym);
                    if (paramSym != null)
                    {
                        // Because we put the parameters at an inner scope,
                        // we have to put them in the outer cache to be found
                        StoreInCache(
                            pInfo.ParameterNode != null ? pInfo.ParameterNode.NameNode : null,
                            paramSym.Name,
                            paramSym,
                            null,
                            true);
                    }
                    paramList.Add(paramSym);

                    EXPR paramExpr = BindToLocal(
                        null,
                        paramSym,
                        BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

                    if (pInfo.IsParameterProcuct)
                    {
                        StoreMembersInCache(typeSym as AGGTYPESYM, paramExpr);
                    }
                }
            } // if (paramInfoList != null)
            else if (lambdaNode.ArgumentsNode != null)
            {
                // The user had some parameters
                DebugUtil.Assert(
                    (lambdaNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_VARARGS) == 0 &&
                    (lambdaNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) == 0);

                // Pass in the outer method as a context
                // so we get it's type parameters (we just re-use it's symbols)
                bool hasParams = false;
                Compiler.ClsDeclRec.DefineParameters(
                    this.ContextForTypeBinding(),
                    lambdaNode.ArgumentsNode,
                    false,
                    ref paramArray,
                    ref hasParams);

                // Now add all of the declared method arguments to the scope
                int p = 0;

                BASENODE node = lambdaNode.ArgumentsNode;
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
                    if (typeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        implicitType = true;
                    }

                    LOCVARSYM paramSym = DeclareParam(
                        paramNode.NameNode.Name,
                        typeSym,
                        0,
                        paramNode,
                        this.currentScopeSym);
                    if (paramSym != null)
                    {
                        // Because we put the parameters at an inner scope,
                        // we have to put them in the outer cache to be found
                        StoreInCache(paramNode, paramSym.Name, paramSym, null, true);
                    }
                    paramList.Add(paramSym);

                    EXPR paramExpr = BindToLocal(
                        null,
                        paramSym,
                        BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

                    AGGTYPESYM ats = typeSym as AGGTYPESYM;
                    if (ats != null && ats.GetAggregate().IsParameterProcuct)
                    {
                        StoreMembersInCache(typeSym as AGGTYPESYM, paramExpr);
                    }
                }
            } // if (lambdaNode.ArgumentsNode != null)

            if (paramArray == null)
            {
                paramArray = BSYMMGR.EmptyTypeArray;
            }
            else
            {
                paramArray = Compiler.MainSymbolManager.AllocParams(paramArray);
            }

            if (lambdaInfo != null)
            {
                lambdaInfo.ParameterArray = paramArray;
            }

            //--------------------------------------------------------
            // Create the EXPRLAMBDAEXPR instance.
            //--------------------------------------------------------
            EXPR lambdaExpr = BindLambdaExpressionInner2(lambdaInfo);

            if (lambdaInfo != null)
            {
                lambdaInfo.JBitLim = uninitedVarCount + 1;
            }
            CloseScope();
            return lambdaExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindLambdaExpressionInner2
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <param name="anonInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindLambdaExpressionInner2(AnonMethInfo anonInfo)
        {
            DebugUtil.Assert(anonInfo != null);

            LAMBDAEXPRNODE lambdaNode = anonInfo.ParseTreeNode as LAMBDAEXPRNODE;
            EXPR lambdaExpr = null;

            // Save the scopes so we don't think we're inside anything
            // (because we aren't when inside the AM block)
            SCOPESYM outerTryScope = this.innermostTryScopeSym;
            SCOPESYM outerCatchScope = this.innermostCatchScopeSym;
            SCOPESYM outerFinallyScope = this.innermostFinallyScopeSym;
            this.innermostCatchScopeSym = this.OuterScopeSym;
            this.innermostTryScopeSym = this.OuterScopeSym;
            this.innermostFinallyScopeSym = this.OuterScopeSym;

            //DebugUtil.Assert(this.currentAnonymousMethodInfo == anonInfo.OuterInfo);
            this.currentAnonymousMethodInfo = anonInfo;
            CreateNewScope();

            //--------------------------------------------------------
            // Bind the block.
            //--------------------------------------------------------
                //anonInfo.BodyBlockExpr
                anonInfo.BodyBlockExpr = BindMethOrPropBody(lambdaNode.BodyNode as BLOCKNODE);

            //--------------------------------------------------------
            // Check reachability
            //--------------------------------------------------------
            ReachabilityChecker reach = new ReachabilityChecker(Compiler);
            reach.SetReachability(anonInfo.BodyBlockExpr, true);
            anonInfo.HasReturnAsLeave = reach.HasReturnAsLeave;

            //--------------------------------------------------------
            // Convert the return values
            //--------------------------------------------------------
            EXPR expr2 = anonInfo.ReturnExprList;
            TYPESYM returnTypeSym = anonInfo.ReturnTypeSym;
            bool returnResult = true;
            bool reportErrors = false;
            bool implicitReturnType = (
                returnTypeSym == null ||
                returnTypeSym.IsIMPLICITTYPESYM);

            while (expr2 != null)
            {
                EXPR operandExpr;
                if (expr2.Kind == EXPRKIND.LIST)
                {
                    operandExpr = expr2.AsBIN.Operand1;
                    expr2 = expr2.AsBIN.Operand2;
                }
                else
                {
                    operandExpr = expr2;
                    expr2 = null;
                }
                EXPRRETURN returnExpr = operandExpr as EXPRRETURN;

                if (implicitReturnType)
                {
                    anonInfo.ReturnTypeSym
                        = returnTypeSym
                        = returnExpr.ObjectExpr.TypeSym;
                    continue;
                }

                EXPR retObjExpr = returnExpr.ObjectExpr;

                if (retObjExpr != null)
                {
                    if (CanConvert(returnExpr.ObjectExpr, returnTypeSym, 0))
                    {
                        continue;
                    }
                    else if (
                        anonInfo.ReturnTypeSym == null &&
                        CanConvert(
                            returnTypeSym,
                            retObjExpr.TypeSym,
                            0))
                    {
                        anonInfo.ReturnTypeSym
                            = returnTypeSym
                            = retObjExpr.TypeSym;
                        continue;
                    }
                }

                //----------------------------------------------------
                // Error processing
                //----------------------------------------------------
                if (!reportErrors)
                {
                    return null;
                }
                if (returnResult)
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_CantConvAnonMethReturns,
                        new ErrArg("anonymous method Block"),
                        new ErrArg(anonInfo.DelegateTypeSym));
                }
                if (returnExpr.ObjectExpr != null)
                {
                    MustConvert(
                        returnExpr.ObjectExpr,
                        anonInfo.ReturnTypeSym,
                        0);
                }
                else
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_RetObjectRequired,
                        new ErrArg(anonInfo.ReturnTypeSym));
                }
                returnResult = false;
            }

            if (anonInfo.ReturnTypeSym == null)
            {
                anonInfo.ReturnTypeSym = returnTypeSym;
            }

            anonInfo.BodyBlockExpr.OwingBlockExpr = null;

            CloseScope();

            lambdaExpr = NewExpr(
                lambdaNode,
                EXPRKIND.LAMBDAEXPR,
                Compiler.MainSymbolManager.LambdaExpressionSym);
            (lambdaExpr as EXPRLAMBDAEXPR).AnonymousMethodInfo = anonInfo;

            // Restore everything

            DebugUtil.Assert(this.currentAnonymousMethodInfo == anonInfo);
            this.currentAnonymousMethodInfo = anonInfo.OuterInfo;

            this.innermostTryScopeSym = outerTryScope;
            this.innermostCatchScopeSym = outerCatchScope;
            this.innermostFinallyScopeSym = outerFinallyScope;

            return lambdaExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.RebindLambdaExpression2
        //
        /// <summary></summary>
        /// <param name="oldLambdaExpr"></param>
        /// <param name="paramInfoList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRLAMBDAEXPR RebindLambdaExpression2(
            EXPRLAMBDAEXPR oldLambdaExpr,
            List<PARAMINFO> paramInfoList)
        {
            AnonMethInfo anonInfo = oldLambdaExpr.AnonymousMethodInfo;
            EXPRLAMBDAEXPR newLambdaExpr = null;

            AnonMethInfo oldCurrentAnonymousMethodInfo = this.currentAnonymousMethodInfo;
            EXPRBLOCK oldBlockExpr = this.currentBlockExpr;

            SCOPESYM oldOuterScopeSym = this.OuterScopeSym;
            SCOPESYM oldCurrentScopeSym = this.currentScopeSym;
            SCOPESYM outerTryScope = this.innermostTryScopeSym;
            SCOPESYM outerCatchScope = this.innermostCatchScopeSym;
            SCOPESYM outerFinallyScope = this.innermostFinallyScopeSym;

            try
            {
                this.currentAnonymousMethodInfo = anonInfo;
                this.currentBlockExpr = anonInfo.BodyBlockExpr;

                this.currentScopeSym = anonInfo.ParametersScopeSym;
                SCOPESYM scpSym = anonInfo.ScopeSym;
                while (scpSym != null && scpSym.NestingOrder > 0)
                {
                    scpSym = scpSym.ParentSym as SCOPESYM;
                }
                this.OuterScopeSym = scpSym;

                this.innermostCatchScopeSym = this.OuterScopeSym;
                this.innermostTryScopeSym = this.OuterScopeSym;
                this.innermostFinallyScopeSym = this.OuterScopeSym;

                newLambdaExpr = RebindLambdaExpressionCore2(oldLambdaExpr, paramInfoList);
            }
            finally
            {
                // Restore everything
                this.innermostTryScopeSym = outerTryScope;
                this.innermostCatchScopeSym = outerCatchScope;
                this.innermostFinallyScopeSym = outerFinallyScope;

                this.OuterScopeSym = oldOuterScopeSym;
                this.currentScopeSym = oldCurrentScopeSym;

                this.currentBlockExpr = oldBlockExpr;
                this.currentAnonymousMethodInfo = oldCurrentAnonymousMethodInfo;
            }
            return newLambdaExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.RebindLambdaExpressionCore2
        //
        /// <summary></summary>
        /// <param name="lambdaExpr"></param>
        /// <param name="paramInfoList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRLAMBDAEXPR RebindLambdaExpressionCore2(
            EXPRLAMBDAEXPR oldLambdaExpr,
            List<PARAMINFO> paramInfoList)
        {
            AnonMethInfo lambdaInfo = oldLambdaExpr.AnonymousMethodInfo;
            LAMBDAEXPRNODE lambdaNode = lambdaInfo.ParseTreeNode as LAMBDAEXPRNODE;

            //--------------------------------------------------------
            // Bind the parameters.
            //--------------------------------------------------------
            TypeArray paramArray = null;
            List<LOCVARSYM> paramList = new List<LOCVARSYM>();
            bool implicitType = false;

            if (paramInfoList != null)
            {
                paramArray = new TypeArray();

                for (int i = 0; i < paramInfoList.Count; ++i)
                {
                    PARAMINFO pInfo = paramInfoList[i];
                    TYPESYM typeSym = pInfo.TypeSym;
                    paramArray.Add(typeSym);

                    if (typeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        implicitType = true;
                    }

                    LOCVARSYM paramSym = DeclareParam(
                        pInfo.Name,
                        typeSym,
                        0,
                        pInfo.ParameterNode != null ? pInfo.ParameterNode.NameNode : null,
                        this.currentScopeSym);
                    if (paramSym != null)
                    {
                        // Because we put the parameters at an inner scope,
                        // we have to put them in the outer cache to be found
                        StoreInCache(
                            pInfo.ParameterNode != null ? pInfo.ParameterNode.NameNode : null,
                            paramSym.Name,
                            paramSym,
                            null,
                            true);
                    }
                    paramList.Add(paramSym);

                    EXPR paramExpr = BindToLocal(
                        null,
                        paramSym,
                        BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

                    if (pInfo.IsParameterProcuct)
                    {
                        StoreMembersInCache(typeSym as AGGTYPESYM, paramExpr);
                    }
                }
            } // if (paramInfoList != null)
            else if (lambdaNode.ArgumentsNode != null)
            {
                // The user had some parameters
                DebugUtil.Assert(
                    (lambdaNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_VARARGS) == 0 &&
                    (lambdaNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) == 0);

                // Pass in the outer method as a context
                // so we get it's type parameters (we just re-use it's symbols)
                bool hasParams = false;
                Compiler.ClsDeclRec.DefineParameters(
                    this.ContextForTypeBinding(),
                    lambdaNode.ArgumentsNode,
                    false,
                    ref paramArray,
                    ref hasParams);

                // Now add all of the declared method arguments to the scope
                int p = 0;

                BASENODE node = lambdaNode.ArgumentsNode;
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
                    if (typeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        implicitType = true;
                    }

                    LOCVARSYM paramSym = DeclareParam(
                        paramNode.NameNode.Name,
                        typeSym,
                        0,
                        paramNode,
                        this.currentScopeSym);
                    if (paramSym != null)
                    {
                        // Because we put the parameters at an inner scope,
                        // we have to put them in the outer cache to be found
                        StoreInCache(paramNode, paramSym.Name, paramSym, null, true);
                    }
                    paramList.Add(paramSym);

                    EXPR paramExpr = BindToLocal(
                        null,
                        paramSym,
                        BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

                    AGGTYPESYM ats = typeSym as AGGTYPESYM;
                    if (ats != null && ats.GetAggregate().IsParameterProcuct)
                    {
                        StoreMembersInCache(typeSym as AGGTYPESYM, paramExpr);
                    }
                }
            } // if (lambdaNode.ArgumentsNode != null)

            if (paramArray == null)
            {
                paramArray = BSYMMGR.EmptyTypeArray;
            }
            else
            {
                paramArray = Compiler.MainSymbolManager.AllocParams(paramArray);
            }

            if (lambdaInfo != null)
            {
                lambdaInfo.ParameterArray = paramArray;
            }

            //--------------------------------------------------------
            // Create the EXPRLAMBDAEXPR instance.
            //--------------------------------------------------------
            EXPRLAMBDAEXPR newLambdaExpr
                = BindLambdaExpressionInner2(lambdaInfo) as EXPRLAMBDAEXPR;

            if (lambdaInfo != null)
            {
                lambdaInfo.JBitLim = uninitedVarCount + 1;
            }
            return newLambdaExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindLambdaExpressionConversion
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <param name="srcExpr"></param>
        /// <param name="destTypeSym"></param>
        /// <param name="destExpr"></param>
        /// <param name="reportErrors"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool BindLambdaExpressionConversion(
            BASENODE lambdaNode,
            EXPR srcExpr,
            TYPESYM destTypeSym,
            ref EXPR destExpr,
            bool reportErrors)
        {
            DebugUtil.Assert(
                srcExpr != null &&
                srcExpr.TypeSym.IsLAMBDAEXPRSYM &&
                srcExpr.Kind == EXPRKIND.LAMBDAEXPR);
            destExpr = null;

            //--------------------------------------------------------
            // If destTypeSym is System.Linq.Expressions.Expression<Func<>>,
            // the lambda expression should be converted to the expression tree.
            //--------------------------------------------------------
#if DEBUG
            AGGSYM as1 = destTypeSym.GetAggregate();
            AGGSYM as2 = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
#endif
            if (destTypeSym.GetAggregate()
                == Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true))
            {
#if false
                return CreateExpressionTree(
                    lambdaNode,
                    srcExpr as EXPRLAMBDAEXPR,
                    destTypeSym as AGGTYPESYM,
                    ref destExpr,
                    reportErrors);
#endif
                EXPR resExpr = null;
                AGGTYPESYM destAts = destTypeSym as AGGTYPESYM;

                bool brEt = CreateExpressionTree(
                    lambdaNode,
                    srcExpr as EXPRLAMBDAEXPR,
                    destAts,
                    ref resExpr,
                    reportErrors);
                EXPRLAMBDAEXPR resLambdaExpr = resExpr as EXPRLAMBDAEXPR;
                if (!brEt || resLambdaExpr == null)
                {
                    return false;
                }
                else if (CanConvert(
                    resLambdaExpr.AnonymousMethodInfo.ReturnTypeSym,
                    destAts,
                    0))
                {
                    destExpr = resLambdaExpr;
                    return true;
                }
                return false;
            }
            //--------------------------------------------------------
            // Otherwise, destTypeSym should be a delegate.
            //--------------------------------------------------------
            if (!destTypeSym.IsDelegateType())
            {
                if (reportErrors)
                {
                    compiler.Error(
                        lambdaNode,
                        CSCERRID.ERR_AnonMethToNonDel,
                        new ErrArg("lambda expression"),
                        new ErrArg(destTypeSym));
                }
                return false;
            }

            if (srcExpr == null || srcExpr.Kind != EXPRKIND.LAMBDAEXPR)
            {
                return false;
            }

            EXPRLAMBDAEXPR srcLambdaExpr = srcExpr as EXPRLAMBDAEXPR;
            AGGTYPESYM aggTypeSym = destTypeSym as AGGTYPESYM;

            METHSYM invokeSym
                = compiler.MainSymbolManager.LookupInvokeMeth(aggTypeSym.GetAggregate());
            if (invokeSym == null || !invokeSym.IsInvoke)
            {
                return false;
            }

            AnonMethInfo anonInfo = (srcExpr as EXPRLAMBDAEXPR).AnonymousMethodInfo;

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
                //----------------------------------------------------
                // If the parameters are implicitly typed,
                //----------------------------------------------------
                if (srcLambdaExpr.ParameterTypesAreImplicit &&
                    anonInfo.ParameterArray.Count == invokeParamTypes.Count)
                {
                    anonInfo.ParameterArray = new TypeArray();
                    anonInfo.ParameterArray.Add(invokeParamTypes);

                    List<LOCVARSYM> paramList = srcLambdaExpr.ParameterList;
                    DebugUtil.Assert(paramList.Count == invokeParamTypes.Count);

                    for (int i = 0; i < paramList.Count; ++i)
                    {
                        paramList[i].TypeSym = invokeParamTypes[i];
                    }
                    anonInfo.ParameterArray
                        = Compiler.MainSymbolManager.AllocParams(anonInfo.ParameterArray);
                }
                //----------------------------------------------------
                // Check parameter lists if the user gave one
                //----------------------------------------------------
                else if (anonInfo.ParameterArray != invokeParamTypes)
                {
                    // the error case, parameter lists don't match exactly
                    if (!reportErrors)
                    {
                        return false;
                    }

                    compiler.Error(
                        lambdaNode,
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
            //----------------------------------------------------
            // The user gave no parameter list
            // so this AM is compatible with any signature containing no out parameters
            //----------------------------------------------------
            else
            {
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
                                lambdaNode,
                                CSCERRID.ERR_CantConvAnonMethNoParams,
                                new ErrArg(aggTypeSym));
                            result = false;
                        }

                        // They need to add an 'out' if they want t^o use this signature
                        compiler.Error(
                            lambdaNode,
                            CSCERRID.ERR_BadParamRef,
                            new ErrArg(p),
                            new ErrArg("out"));
                    }
                }
            }

            //--------------------------------------------------------
            // Return Type
            //--------------------------------------------------------
            anonInfo.ReturnTypeSym = invokeReturnTypeSym;
            bool returnResult = true;

            //--------------------------------------------------------
            // Check (and possibly cast) return types (1) void
            //--------------------------------------------------------
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
                                lambdaNode,
                                CSCERRID.ERR_CantConvAnonMethReturns,
                                new ErrArg("lambda expression"),
                                new ErrArg(aggTypeSym));
                        }
                        // return non-empty
                        compiler.Error(
                            lambdaNode,
                            CSCERRID.ERR_RetNoObjectRequired,
                            new ErrArg(aggTypeSym));
                        returnResult = false;
                    }
                }
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
                //anonInfo.ReturnTypeSym = invokeReturnTypeSym;

                SetNodeExpr(lambdaNode, destExpr);
            }

            return (result && returnResult);
        }
    }
}
