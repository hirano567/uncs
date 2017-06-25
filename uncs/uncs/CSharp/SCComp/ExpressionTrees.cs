#if false // (Official) ExpressionTrees.cs

//============================================================================
// ExpressionTrees.cs
//
// 2016/04/29 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        private AGGTYPESYM typeAts = null;
        private AGGTYPESYM fieldInfoAts = null;
        private AGGTYPESYM methodInfoAts = null;

        private AGGTYPESYM expressionAts = null;
        private AGGSYM expressionAggSym = null;
        private ARRAYSYM expressionArraySym = null;

        private NSSYM expressionsNsSym = null;
        private NSDECLSYM expressionsDeclSym = null;
        private NSAIDSYM expressionsNsAidSym = null;

        //------------------------------------------------------------
        // FUNCBREC.SetExpressionTreeFields
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool SetExpressionTreeFields()
        {
            if (expressionsNsAidSym != null)
            {
                return true;
            }

            NSSYM systemNsSym = Compiler.LookupInBagAid(
                "System",
                Compiler.MainSymbolManager.RootNamespaceSym,
                0,
                0,
                SYMBMASK.NSSYM) as NSSYM;
            if (systemNsSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("System"));
                return false;
            }

            NSSYM reflectionNsSym = Compiler.LookupInBagAid(
                "Reflection",
                systemNsSym,
                0,
                0,
                SYMBMASK.NSSYM) as NSSYM;
            if (reflectionNsSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("Reflection"));
                return false;
            }

            AGGSYM typeAggSym = Compiler.LookupInBagAid(
                "Type",
                systemNsSym,
                0,
                0,
                SYMBMASK.AGGSYM) as AGGSYM;
            if (typeAggSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("Type"));
                return false;
            }

            AGGSYM fieldInfoAggSym = Compiler.LookupInBagAid(
                "FieldInfo",
                reflectionNsSym,
                0,
                0,
                SYMBMASK.AGGSYM) as AGGSYM;
            if (fieldInfoAggSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("FieldInfo"));
                return false;
            }

            AGGSYM methodInfoAggSym = Compiler.LookupInBagAid(
                "MethodInfo",
                reflectionNsSym,
                0,
                0,
                SYMBMASK.AGGSYM) as AGGSYM;
            if (methodInfoAggSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("MethodInfo"));
                return false;
            }

            typeAts = typeAggSym.GetThisType();
            fieldInfoAts = fieldInfoAggSym.GetThisType();
            methodInfoAts = methodInfoAggSym.GetThisType();

            expressionAts = Compiler.GetOptPredefType(PREDEFTYPE.LINQ_EXPRESSIONS_EXPRESSION, true);
            if (expressionAts == null)
            {
                return false;
            }
            expressionAggSym = expressionAts.GetAggregate();
            expressionArraySym = Compiler.MainSymbolManager.GetArray(
                expressionAts,
                1,
                expressionAggSym.Type.MakeArrayType());

            expressionsNsSym = expressionAggSym.ParentBagSym as NSSYM;
            expressionsDeclSym = expressionsNsSym.FirstDeclSym;
            expressionsNsAidSym = Compiler.MainSymbolManager.GetNsAid(expressionsNsSym, Kaid.Global);

            return true;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateExpressionTree
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <param name="srcLambdaExpr"></param>
        /// <param name="dstAts"></param>
        /// <param name="dstExpr"></param>
        /// <param name="reportErrors"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool CreateExpressionTree(
            BASENODE lambdaNode,
            EXPRLAMBDAEXPR srcLambdaExpr,
            AGGTYPESYM dstAts,
            ref EXPR dstExpr,
            bool reportErrors)
        {
            if (dstAts == null ||
                dstAts.GetAggregate() != Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true))
            {
                return false;
            }
            if (dstAts.TypeArguments == null || dstAts.TypeArguments.Count == 0)
            {
                return false;
            }

            AnonMethInfo anonInfo = srcLambdaExpr.AnonymousMethodInfo;
            if (anonInfo == null)
            {
                return false;
            }

            if (anonInfo.ConversionCompleted)
            {
                dstExpr = srcLambdaExpr;
                return true;
            }

            if (!SetExpressionTreeFields())
            {
                return false;
            }

            AGGTYPESYM funcAts = dstAts.TypeArguments[0] as AGGTYPESYM;
            if (funcAts == null)
            {
                return false;
            }

            AGGSYM funcAggSym = null;
            int paramCount = anonInfo.ParameterArray.Count;

            switch (paramCount)
            {
                case 0:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G1_FUNC, true);
                    break;

                case 1:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                    break;

                case 2:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G3_FUNC, true);
                    break;

                case 3:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G4_FUNC, true);
                    break;

                case 4:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G5_FUNC, true);
                    break;

                default:
                    return false;
            }
            if (funcAts.GetAggregate() != funcAggSym)
            {
                return false;
            }
            TypeArray funcTypeArgs = funcAts.TypeArguments;

            //--------------------------------------------------------
            // Prepare to bind the lambda expression: set the parameter types.
            //--------------------------------------------------------
            for (int i = 0; i < paramCount; ++i)
            {
                LOCVARSYM locSym = srcLambdaExpr.ParameterList[i];

                if (locSym.TypeSym == null ||
                    locSym.TypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                {
                    TYPESYM argTypeSym = funcTypeArgs[i];
                    if (argTypeSym != null &&
                        argTypeSym.Kind != SYMKIND.IMPLICITTYPESYM)
                    {
                        locSym.TypeSym = argTypeSym;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            //--------------------------------------------------------
            // Bind the lambda expression
            //--------------------------------------------------------
            if (anonInfo.BodyBlockExpr == null)
            {
                BindLambdaExpressionInner(anonInfo);
            }
#if DEBUG
            StringBuilder sbDebug = new StringBuilder();
            DebugUtil.DebugSymsOutput(sbDebug);
            sbDebug.Length = 0;
            DebugUtil.DebugExprsOutput(sbDebug);
#endif
            if (!anonInfo.Compiled)
            {
                return false;
            }

            EXPR argList = null;
            EXPR argListLast = null;

            //--------------------------------------------------------
            // local variables
            //--------------------------------------------------------
            StatementListBuilder builder = new StatementListBuilder();

            Dictionary<string, LOCVARSYM> paramDic = new Dictionary<string, LOCVARSYM>();
            List<LOCVARSYM> paramList = new List<LOCVARSYM>();

            //--------------------------------------------------------
            // Define the parameters
            //--------------------------------------------------------
            TYPESYM paramExprTypeSym = LookupExpressionsType("ParameterExpression");
            DebugUtil.Assert(paramExprTypeSym != null);

            StatementListBuilder lambdaStatementBuilder = new StatementListBuilder();

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

                //this.currentScopeSym = anonInfo.ParametersScopeSym;
                this.currentScopeSym = this.currentBlockExpr.ScopeSym;

                SCOPESYM scpSym = anonInfo.ScopeSym;
                while (scpSym != null && scpSym.NestingOrder > 0)
                {
                    scpSym = scpSym.ParentSym as SCOPESYM;
                }
                this.OuterScopeSym = scpSym;

                this.innermostCatchScopeSym = this.OuterScopeSym;
                this.innermostTryScopeSym = this.OuterScopeSym;
                this.innermostFinallyScopeSym = this.OuterScopeSym;

                for (int i = 0; i < paramCount; ++i)
                {
                    LOCVARSYM locSym = srcLambdaExpr.ParameterList[i];
                    string paramLocName
                        = CreateSpecialName(SpecialNameKindEnum.SavedParamOrThis, locSym.Name);

                    //------------------------------------------------
                    // (LHS) Define the SYM and the EXPR for the parameter
                    //------------------------------------------------
                    LOCVARSYM paramLocSym = DeclareVar(
                        null,
                        paramLocName,
                        paramExprTypeSym,
                        false);

                    this.uninitedVarCount++;
                    //paramLocSym.LocSlotInfo.SetJbitDefAssg(this.uninitedVarCount + 1);
                    //int cbit = FlowChecker.GetCbit(Compiler, paramLocSym.TypeSym);
                    //this.uninitedVarCount += cbit;
                    //this.uninitedVarCount -= cbit;
                    paramLocSym.LocSlotInfo.SetJbitDefAssg(0);

                    EXPR paramLocExpr = BindToLocal(null, paramLocSym, BindFlagsEnum.Arguments);

                    //------------------------------------------------
                    // (RHS) Call Expression.Parameter method.
                    //------------------------------------------------
                    argList = null;
                    argListLast = null;

                    Compiler.Emitter.EmitAggregateDef(locSym.TypeSym.GetAggregate());
                    (locSym.TypeSym as AGGTYPESYM).GetConstructedType(null, null, false);
                    EXPR typeExpr = BindSystemType(locSym.TypeSym.Type);

                    EXPR nameExpr = NewExprConstant(
                        null,
                        Compiler.GetReqPredefType(PREDEFTYPE.STRING, true),
                        new ConstValInit(locSym.Name));

                    NewList(typeExpr, ref argList, ref argListLast);
                    NewList(nameExpr, ref argList, ref argListLast);

                    EXPR callParamExpr
                        = BindExpressionMethod("Parameter", BSYMMGR.EmptyTypeArray, argList);

                    //------------------------------------------------
                    // Assign
                    //------------------------------------------------
                    EXPR assgExpr = BindAssignment(null, paramLocExpr, callParamExpr, false);
                    paramLocSym.LocSlotInfo.HasInit = true;

                    EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
                    declExpr.LocVarSym = paramLocSym;
                    declExpr.InitialExpr = assgExpr;

                    paramDic.Add(locSym.Name, paramLocSym);
                    paramList.Add(paramLocSym);
                    //this.initializerBuilder1.Add(declExpr);
                    lambdaStatementBuilder.Add(declExpr);
                }
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

            //--------------------------------------------------------
            // Convert to the expression tree
            //--------------------------------------------------------
            EXPRBLOCK bodyExpr = anonInfo.BodyBlockExpr;
            if (bodyExpr == null)
            {
                return false;
            }
            EXPRSTMT stmtExpr = bodyExpr.StatementsExpr;
            if (stmtExpr.Kind != EXPRKIND.RETURN ||
                stmtExpr.NextStatement != null)
            {
                Compiler.Error(stmtExpr.TreeNode, CSCERRID.ERR_CannotConvertToExprTree);
                return false;
            }

            EXPRRETURN returnExpr = stmtExpr as EXPRRETURN;
            EXPR exTreeExpr = ConvertLambdaExpressionToExpressionTree(
                returnExpr.ObjectExpr,
                paramDic);
#if DEBUG
            sbDebug.Length = 0;
            DebugUtil.DebugExprsOutput(sbDebug);
#endif

            //--------------------------------------------------------
            // Call Expression.Lambda method
            //--------------------------------------------------------
            argList = null;
            argListLast = null;

            NewList(exTreeExpr, ref argList, ref argListLast);

            TYPESYM paramExpressionAts = LookupExpressionsType("ParameterExpression");
            ARRAYSYM paramExpressionArraySym = Compiler.MainSymbolManager.GetArray(
                paramExpressionAts,
                1,
                null);

            EXPR argList2 = null;
            EXPR argListLast2 = null;
            int count = 0;

            for (int i = 0; i < paramList.Count; ++i)
            {
                EXPR locExpr = BindToLocal(null, paramList[i], BindFlagsEnum.RValueRequired);
                NewList(locExpr, ref argList2, ref argListLast2);
                ++count;
            }

            EXPRARRINIT arrInitExpr
                = NewExpr(
                    null,
                    EXPRKIND.ARRINIT,
                    paramExpressionArraySym) as EXPRARRINIT;
            arrInitExpr.ArgumentsExpr = argList2;
            arrInitExpr.DimSizes.Add(count);

            NewList(arrInitExpr, ref argList, ref argListLast);

            returnExpr.ObjectExpr = BindExpressionMethod(
                "Lambda",
                dstAts.TypeArguments,
                argList);
            lambdaStatementBuilder.Add(returnExpr);

            anonInfo.ToExpressionTree = true;
            anonInfo.ReturnTypeSym = dstAts;
            anonInfo.ParameterArray = BSYMMGR.EmptyTypeArray;

            TypeArray dgTypeArgs = new TypeArray();
            dgTypeArgs.Add(dstAts);
            dgTypeArgs = Compiler.MainSymbolManager.AllocParams(dgTypeArgs);
#if false
            anonInfo.DelegateTypeSym = Compiler.MainSymbolManager.SubstType(
                Compiler.GetOptPredefType(PREDEFTYPE.G1_FUNC, true),
                new SubstContext(
                      dgTypeArgs,
                      null,
                      SubstTypeFlagsEnum.NormNone)) as AGGTYPESYM;
#endif
            anonInfo.DelegateTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G1_FUNC, true).GetAggregate(),
                dgTypeArgs);

            bodyExpr.StatementsExpr = lambdaStatementBuilder.GetList();
            dstExpr = srcLambdaExpr;
            srcLambdaExpr.AnonymousMethodInfo.ConversionCompleted = true;
            return true;
        }

        //------------------------------------------------------------
        // FUNCBREC.LookupExpressionsType
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM LookupExpressionsType(string name)
        {
            AGGSYM aggSym = Compiler.LookupInBagAid(
                name,
                expressionsNsSym,
                0,
                0,
                SYMBMASK.AGGSYM) as AGGSYM;

            if (aggSym != null)
            {
                return aggSym.GetThisType();
            }
            return null;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindExpressionMethod
        //
        /// <summary></summary>
        /// <param name="methodName"></param>
        /// <param name="argExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindExpressionMethod(
            string methodName,
            TypeArray typeArgs,
            EXPR argExpr)
        {
            EXPRCLASS expressionExpr = NewExpr(null, EXPRKIND.CLASS, expressionAts) as EXPRCLASS;

            MemberLookup mem = new MemberLookup();
            if (!mem.Lookup(
                    Compiler,
                    this.expressionAts,
                    expressionExpr,
                    this.parentDeclSym,
                    methodName,
                    0,
                    MemLookFlagsEnum.UserCallable))
            {
                throw new ArgumentException("FUNCBREC.BindExpressionMethod");
            }

            EXPRMEMGRP groupExpr = NewExpr(
                null,
                EXPRKIND.MEMGRP,
                Compiler.MainSymbolManager.MethodGroupTypeSym) as EXPRMEMGRP;

            groupExpr.Name = methodName;
            groupExpr.SymKind = SYMKIND.METHSYM;
            groupExpr.TypeArguments = typeArgs;
            groupExpr.ParentTypeSym = expressionAts;
            groupExpr.MethPropSym = null;
            groupExpr.ObjectExpr = null;
            groupExpr.ContainingTypeArray = mem.GetAllTypes();
            groupExpr.Flags = EXPRFLAG.USERCALLABLE;

            return BindGrpToArgs(null, BindFlagsEnum.RValueRequired, groupExpr, argExpr);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindSystemType
        //
        /// <summary>
        /// Bind a System.Type instance.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRSYSTEMTYPE BindSystemType(System.Type type)
        {
            if (type == null)
            {
                return null;
            }

            EXPRSYSTEMTYPE typeExpr = NewExpr(
                null,
                EXPRKIND.SYSTEMTYPE,
                typeAts) as EXPRSYSTEMTYPE;
            typeExpr.Type = type;
            return typeExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindFieldInfo
        //
        /// <summary></summary>
        /// <param name="fieldSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRFIELDINFO BindFieldInfo(MEMBVARSYM fieldSym)
        {
            if (fieldSym == null)
            {
                return null;
            }
            if (fieldSym.ClassSym.Type == null)
            {
                Compiler.Emitter.EmitAggregateDef(fieldSym.ClassSym);
            }
            if (fieldSym.FieldInfo == null)
            {
                Compiler.Emitter.EmitMembVarDef(fieldSym);
            }
            if (fieldSym.FieldInfo == null)
            {
                return null;
            }

            EXPRFIELDINFO fiExpr = NewExpr(
                null,
                EXPRKIND.FIELDINFO,
                fieldInfoAts) as EXPRFIELDINFO;
            fiExpr.FieldInfo = fieldSym.FieldInfo;
            return fiExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindMethodInfo
        //
        /// <summary></summary>
        /// <param name="info"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRMETHODINFO BindMethodInfo(MethodInfo info)
        {
            if (info == null)
            {
                return null;
            }

            EXPRMETHODINFO miExpr = NewExpr(
                null,
                EXPRKIND.METHODINFO,
                methodInfoAts) as EXPRMETHODINFO;
            miExpr.MethodInfo = info;
            return miExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.IsCheckOverflowSet
        //
        /// <summary></summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsCheckOverflowSet(EXPRFLAG flags)
        {
            return (flags & EXPRFLAG.CHECKOVERFLOW) != 0;
        }

        //------------------------------------------------------------
        // FUNCBREC.ConvertLambdaExpressionToExpressionTree
        //
        /// <summary></summary>
        /// <param name="lambdaExpr"></param>
        /// <param name="paramDic"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR ConvertLambdaExpressionToExpressionTree(
            EXPR srcExpr,
            Dictionary<string, LOCVARSYM> paramDic)
        {
            string exMethodName = null;
            CSCERRID errorID = CSCERRID.ERR_NONE;
            EXPR argExpr1 = null;
            EXPR argExpr2 = null;

            EXPR argList1 = null;
            EXPR argListLast1 = null;
            EXPR argList2 = null;
            EXPR argListLast2 = null;

            EXPR tempExpr1 = null;
            EXPR tempExpr2 = null;

            switch (srcExpr.Kind)
            {
                case EXPRKIND.BLOCK:
                case EXPRKIND.STMTAS:
                case EXPRKIND.RETURN:
                case EXPRKIND.DECL:
                case EXPRKIND.LABEL:
                case EXPRKIND.GOTO:
                case EXPRKIND.GOTOIF:
                case EXPRKIND.SWITCH:
                case EXPRKIND.SWITCHLABEL:
                case EXPRKIND.TRY:
                case EXPRKIND.HANDLER:
                case EXPRKIND.THROW:
                case EXPRKIND.NOOP:
                case EXPRKIND.DEBUGNOOP:
                case EXPRKIND.DELIM:
                    errorID = CSCERRID.ERR_CannotConvertToExprTree;
                    break;

                case EXPRKIND.BINOP:
                    break;

                //----------------------------------------------------
                // CALL
                //
                // public static MethodCallExpression Call(
                // 	Expression instance,
                // 	MethodInfo method
                // )
                // public static MethodCallExpression Call(
                // 	MethodInfo method,
                // 	params Expression[] arguments
                // )
                // public static MethodCallExpression Call(
                // 	Expression instance,
                // 	MethodInfo method,
                // 	params Expression[] arguments
                // )
                //----------------------------------------------------
                case EXPRKIND.CALL:
                    EXPRCALL callExpr = srcExpr as EXPRCALL;
                    DebugUtil.Assert(callExpr != null);

                    Compiler.Emitter.EmitMethodDef(callExpr.MethodWithInst.MethSym);
                    string methodName = callExpr.MethodWithInst.MethSym.Name;

                    //------------------------------------------------
                    // (1) op_Equality, op_Inequality
                    //------------------------------------------------
                    if (methodName == "op_Equality" || methodName == "op_Inequality")
                    {
                        argList1 = null;
                        argListLast1 = null;

                        //--------------------------------------------
                        // (1-1) Arguments
                        //--------------------------------------------
                        DebugUtil.Assert(callExpr.ArgumentsExpr != null);
                        EXPR expr = callExpr.ArgumentsExpr;
                        tempExpr1 = null;
                        int count = 0;

                        while (expr != null)
                        {
                            if (expr.Kind == EXPRKIND.LIST)
                            {
                                tempExpr1 = (expr as EXPRBINOP).Operand1;
                                expr = (expr as EXPRBINOP).Operand2;
                            }
                            else
                            {
                                tempExpr1 = expr;
                                expr = null;
                            }

                            tempExpr2 = ConvertLambdaExpressionToExpressionTree(
                                tempExpr1,
                                paramDic);
                            if (tempExpr2 != null)
                            {
                                //tempExpr2 = MustConvert(tempExpr2, expressionAts, 0);
                                NewList(tempExpr2, ref argList1, ref argListLast1);
                                ++count;
                            }
                        }

                        if (count != 2)
                        {
                        }

                        //--------------------------------------------
                        // (1-2) LiftToNull
                        //--------------------------------------------
                        tempExpr1 = NewExprConstant(
                            null,
                            Compiler.GetReqPredefType(PREDEFTYPE.BOOL, true),
                            new ConstValInit(false));
                        tempExpr1.Flags |= EXPRFLAG.LITERALCONST;

                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        //--------------------------------------------
                        // (1-3) MethodInfo
                        //--------------------------------------------
                        tempExpr1 = BindMethodInfo(callExpr.MethodWithInst.MethSym.MethodInfo);
                        DebugUtil.Assert(tempExpr1 != null);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        if (methodName == "op_Equality")
                        {
                            return BindExpressionMethod("Equal", BSYMMGR.EmptyTypeArray, argList1);
                        }
                        else
                        {
                            return BindExpressionMethod("NotEqual", BSYMMGR.EmptyTypeArray, argList1);
                        }
                    }
                    //------------------------------------------------
                    // (2) Otherwise
                    //------------------------------------------------
                    else
                    {
                        argList1 = null;
                        argListLast1 = null;

                        //--------------------------------------------
                        // Object
                        //--------------------------------------------
                        if (callExpr.ObjectExpr != null)
                        {
                            tempExpr1 = ConvertLambdaExpressionToExpressionTree(
                                callExpr.ObjectExpr,
                                paramDic);
                            NewList(tempExpr1, ref argList1, ref argListLast1);
                        }

                        //--------------------------------------------
                        // method
                        //--------------------------------------------
                        tempExpr1 = BindMethodInfo(callExpr.MethodWithInst.MethSym.MethodInfo);
                        DebugUtil.Assert(tempExpr1 != null);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        //--------------------------------------------
                        // arguments
                        //--------------------------------------------
                        if (callExpr.ArgumentsExpr != null)
                        {
                            EXPR expr = callExpr.ArgumentsExpr;
                            tempExpr1 = null;
                            argList2 = null;
                            argListLast2 = null;
                            int count = 0;

                            while (expr != null)
                            {
                                if (expr.Kind == EXPRKIND.LIST)
                                {
                                    tempExpr1 = (expr as EXPRBINOP).Operand1;
                                    expr = (expr as EXPRBINOP).Operand2;
                                }
                                else
                                {
                                    tempExpr1 = expr;
                                    expr = null;
                                }

                                tempExpr2 = ConvertLambdaExpressionToExpressionTree(
                                    tempExpr1,
                                    paramDic);
                                if (tempExpr2 != null)
                                {
                                    //tempExpr2 = MustConvert(tempExpr2, expressionAts, 0);
                                    NewList(tempExpr2, ref argList2, ref argListLast2);
                                    ++count;
                                }
                            }

                            if (count > 0)
                            {
                                EXPRARRINIT arrInitExpr
                                    = NewExpr(
                                        null,
                                        EXPRKIND.ARRINIT,
                                        expressionArraySym) as EXPRARRINIT;
                                arrInitExpr.ArgumentsExpr = argList2;
                                arrInitExpr.DimSizes.Add(count);

                                NewList(arrInitExpr, ref argList1, ref argListLast1);
                            }
                        }

                        return BindExpressionMethod("Call", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // EVENT
                //----------------------------------------------------
                case EXPRKIND.EVENT:
                    break;

                //----------------------------------------------------
                // FIELD
                //----------------------------------------------------
                case EXPRKIND.FIELD:
                    {
                        EXPRFIELD fieldExpr = srcExpr as EXPRFIELD;
                        DebugUtil.Assert(fieldExpr != null);
                        MEMBVARSYM fieldSym = fieldExpr.FieldWithType.FieldSym;

                        Compiler.Emitter.EmitMembVarDef(fieldSym);

                        argList1 = null;
                        argListLast1 = null;

                        //--------------------------------------------
                        // Object
                        //--------------------------------------------
                        DebugUtil.Assert(fieldExpr.ObjectExpr != null);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTree(
                            fieldExpr.ObjectExpr,
                            paramDic);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        //--------------------------------------------
                        // Field
                        //--------------------------------------------
                        tempExpr1 = BindFieldInfo(fieldSym);
                        DebugUtil.Assert(tempExpr1 != null);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Field", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // LOCAL
                //----------------------------------------------------
                case EXPRKIND.LOCAL:
                    {
                        EXPRLOCAL localExpr = srcExpr as EXPRLOCAL;
                        LOCVARSYM localSym = localExpr.LocVarSym;
                        DebugUtil.Assert(localSym != null);

                        //--------------------------------------------
                        // parameter
                        //--------------------------------------------
                        LOCVARSYM valSym = null;
                        if (paramDic.TryGetValue(localSym.Name, out valSym))
                        {
                            return BindToLocal(null, valSym, BindFlagsEnum.RValueRequired);
                        }

                        //--------------------------------------------
                        // otherwise
                        //--------------------------------------------
                        localExpr.ToExpressionTree = true;
                        localExpr.RealTypeSym = localExpr.TypeSym;
                        localExpr.TypeSym = LookupExpressionsType("MemberExpression");
                        return localExpr;
                    }

                //----------------------------------------------------
                // CONSTANT
                //----------------------------------------------------
                case EXPRKIND.CONSTANT:
                    {
                        EXPRCONSTANT constExpr = srcExpr as EXPRCONSTANT;
                        DebugUtil.Assert(constExpr != null);

                        argList1 = null;
                        argListLast1 = null;

                        tempExpr1 = MustConvert(
                            constExpr,
                            Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true),
                            0);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        tempExpr1 = BindSystemType(constExpr.TypeSym.Type);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Constant", BSYMMGR.EmptyTypeArray, argList1);
                    }

                case EXPRKIND.CLASS:
                case EXPRKIND.NSPACE:
                case EXPRKIND.ERROR:
                    break;

                case EXPRKIND.FUNCPTR:
                    break;

                //----------------------------------------------------
                // PROP
                //----------------------------------------------------
                case EXPRKIND.PROP:
                    {
                        EXPRPROP propExpr = srcExpr as EXPRPROP;
                        DebugUtil.Assert(propExpr != null);

                        Compiler.Emitter.EmitPropertyDef(propExpr.SlotPropWithType.PropSym);

                        argList1 = null;
                        argListLast1 = null;

                        //--------------------------------------------
                        // Object
                        //--------------------------------------------
                        DebugUtil.Assert(propExpr.ObjectExpr != null);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTree(
                            propExpr.ObjectExpr,
                            paramDic);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        //--------------------------------------------
                        // Accessor
                        //--------------------------------------------
                        if ((propExpr.Flags & EXPRFLAG.MEMBERSET) != 0)
                        {
                            tempExpr1 = BindMethodInfo(propExpr.SetMethodWithType.MethSym.MethodInfo);
                        }
                        else
                        {
                            tempExpr1 = BindMethodInfo(propExpr.GetMethodWithType.MethSym.MethodInfo);
                        }
                        DebugUtil.Assert(tempExpr1 != null);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Property", BSYMMGR.EmptyTypeArray, argList1);
                    }

                case EXPRKIND.MULTI:
                case EXPRKIND.MULTIGET:
                case EXPRKIND.STTMP:
                case EXPRKIND.LDTMP:
                case EXPRKIND.FREETMP:
                case EXPRKIND.WRAP:
                    break;

                case EXPRKIND.CONCAT:
                    break;

                case EXPRKIND.ARRINIT:
                    break;

                //----------------------------------------------------
                // CAST
                //----------------------------------------------------
                case EXPRKIND.CAST:
                    {
                        EXPRCAST castExpr = srcExpr as EXPRCAST;
                        DebugUtil.Assert(
                            castExpr != null &&
                            castExpr.TypeSym != null &&
                            castExpr.TypeSym.Type != null);

                        tempExpr1 = ConvertLambdaExpressionToExpressionTree(
                                        castExpr.Operand,
                                        paramDic);
                        EXPR typeExpr = BindSystemType(castExpr.TypeSym.Type);

                        argList1 = null;
                        argListLast1 = null;
                        NewList(tempExpr1, ref argList1, ref argListLast1);
                        NewList(typeExpr, ref argList1, ref argListLast1);

                        exMethodName = IsCheckOverflowSet(srcExpr.Flags) ? "ConvertChecked" : "Convert";
                        return BindExpressionMethod(exMethodName, BSYMMGR.EmptyTypeArray, argList1);
                    }

                case EXPRKIND.TYPEOF:
                    break;

                case EXPRKIND.SIZEOF:
                    break;

                case EXPRKIND.ZEROINIT:
                case EXPRKIND.USERLOGOP:
                case EXPRKIND.MEMGRP:
                case EXPRKIND.ANONMETH:
                case EXPRKIND.DBLQMARK:
                case EXPRKIND.COUNT:
                case EXPRKIND.LIST:
                case EXPRKIND.ASSG:
                case EXPRKIND.MAKERA:
                case EXPRKIND.VALUERA:
                case EXPRKIND.TYPERA:
                case EXPRKIND.ARGS:
                case EXPRKIND.EQUALS:
                case EXPRKIND.COMPARE:
                case EXPRKIND.TRUE:
                case EXPRKIND.FALSE:
                    break;

                case EXPRKIND.INC:
                case EXPRKIND.DEC:
                    break;

                //----------------------------------------------------
                // LOGNOT
                // NEG, UPLUS, BITNOT
                //----------------------------------------------------
                case EXPRKIND.LOGNOT:
                case EXPRKIND.NEG:
                case EXPRKIND.UPLUS:
                case EXPRKIND.BITNOT:
                    {
                        EXPRBINOP binOpExpr = srcExpr as EXPRBINOP;

                        argExpr1 = ConvertLambdaExpressionToExpressionTree(
                            binOpExpr.Operand1,
                            paramDic);

                        switch (srcExpr.Kind)
                        {
                            case EXPRKIND.LOGNOT:
                                exMethodName = "Not";
                                break;
                            case EXPRKIND.NEG:
                                exMethodName = IsCheckOverflowSet(srcExpr.Flags)
                                    ? "NegateChecked" : "Negate";
                                break;
                            case EXPRKIND.UPLUS:
                                exMethodName = "UnaryPlus";
                                break;
                            case EXPRKIND.BITNOT:
                                exMethodName = "Not";
                                break;

                            default:
                                DebugUtil.Assert(false);
                                return null;
                        }

                        return BindExpressionMethod(exMethodName, BSYMMGR.EmptyTypeArray, argExpr1);
                    }

                //----------------------------------------------------
                // EQ, NE, LT, LE, GT, GE
                // ADD, SUB, MUL, DIV, MOD
                // BITAND, BITOR, BITXOR, LSHIFT, RSHIFT
                // LOGAND, LOGOR
                //----------------------------------------------------
                case EXPRKIND.EQ:
                case EXPRKIND.NE:
                case EXPRKIND.LT:
                case EXPRKIND.LE:
                case EXPRKIND.GT:
                case EXPRKIND.GE:

                case EXPRKIND.ADD:
                case EXPRKIND.SUB:
                case EXPRKIND.MUL:
                case EXPRKIND.DIV:
                case EXPRKIND.MOD:

                case EXPRKIND.BITAND:
                case EXPRKIND.BITOR:
                case EXPRKIND.BITXOR:
                case EXPRKIND.LSHIFT:
                case EXPRKIND.RSHIFT:

                case EXPRKIND.LOGAND:
                case EXPRKIND.LOGOR:
                    {
                        EXPRBINOP binOpExpr = srcExpr as EXPRBINOP;

                        argExpr1 = ConvertLambdaExpressionToExpressionTree(
                            binOpExpr.Operand1,
                            paramDic);
                        argExpr2 = ConvertLambdaExpressionToExpressionTree(
                            binOpExpr.Operand2,
                            paramDic);

                        argList1 = null;
                        argListLast1 = null;
                        NewList(argExpr1, ref argList1, ref argListLast1);
                        NewList(argExpr2, ref argList1, ref argListLast1);

                        switch (srcExpr.Kind)
                        {
                            case EXPRKIND.EQ:
                                exMethodName = "Equal";
                                break;
                            case EXPRKIND.NE:
                                exMethodName = "NotEqual";
                                break;
                            case EXPRKIND.LT:
                                exMethodName = "LessThan";
                                break;
                            case EXPRKIND.LE:
                                exMethodName = "LessThanOrEqual";
                                break;
                            case EXPRKIND.GT:
                                exMethodName = "GreaterThan";
                                break;
                            case EXPRKIND.GE:
                                exMethodName = "GreaterThanOrEqual";
                                break;

                            case EXPRKIND.ADD:
                                exMethodName = IsCheckOverflowSet(srcExpr.Flags)
                                    ? "AddChecked" : "Add";
                                break;
                            case EXPRKIND.SUB:
                                exMethodName = IsCheckOverflowSet(srcExpr.Flags)
                                    ? "SubtractChecked" : "Subtract";
                                break;
                            case EXPRKIND.MUL:
                                exMethodName = IsCheckOverflowSet(srcExpr.Flags)
                                    ? "MultiplyChecked" : "Multiply";
                                break;
                            case EXPRKIND.DIV:
                                exMethodName = "Divide";
                                break;
                            case EXPRKIND.MOD:
                                exMethodName = "Modulo";
                                break;

                            case EXPRKIND.BITAND:
                                exMethodName = "And";
                                break;
                            case EXPRKIND.BITOR:
                                exMethodName = "Or";
                                break;
                            case EXPRKIND.BITXOR:
                                exMethodName = "ExclusiveOr";
                                break;
                            case EXPRKIND.LSHIFT:
                                exMethodName = "LeftShift";
                                break;
                            case EXPRKIND.RSHIFT:
                                exMethodName = "RightShift";
                                break;

                            case EXPRKIND.LOGAND:
                                exMethodName = "And";
                                break;
                            case EXPRKIND.LOGOR:
                                exMethodName = "Or";
                                break;

                            default:
                                DebugUtil.Assert(false);
                                return null;
                        }

                        return BindExpressionMethod(exMethodName, BSYMMGR.EmptyTypeArray, argList1);
                    }

                case EXPRKIND.IS:
                case EXPRKIND.AS:
                case EXPRKIND.ARRINDEX:
                case EXPRKIND.NEWARRAY:
                    break;

                //----------------------------------------------------
                // ternary operator ? :
                //----------------------------------------------------
                case EXPRKIND.QMARK:
                    {
                        EXPRBINOP qmarkExpr = srcExpr as EXPRBINOP;
                        DebugUtil.Assert(qmarkExpr != null);
                        EXPRBINOP choiceExpr = qmarkExpr.Operand2 as EXPRBINOP;
                        DebugUtil.Assert(choiceExpr != null);

                        argList1 = null;
                        argListLast1 = null;

                        tempExpr1 = ConvertLambdaExpressionToExpressionTree(qmarkExpr.Operand1, paramDic);
                        NewList(tempExpr1, ref argList1, ref argListLast1);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTree(choiceExpr.Operand1, paramDic);
                        NewList(tempExpr1, ref argList1, ref argListLast1);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTree(choiceExpr.Operand2, paramDic);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Condition", BSYMMGR.EmptyTypeArray, argList1);
                    }

                case EXPRKIND.SEQUENCE:
                case EXPRKIND.SEQREV:
                case EXPRKIND.SAVE:
                case EXPRKIND.SWAP:
                case EXPRKIND.ARGLIST:
                case EXPRKIND.INDIR:
                case EXPRKIND.ADDR:
                case EXPRKIND.LOCALLOC:
                case EXPRKIND.LAMBDAEXPR: // CS3
                case EXPRKIND.MULTIOFFSET:
                defalut:
                    break;
            }

            if (errorID == CSCERRID.ERR_NONE)
            {
                Compiler.Error(srcExpr.TreeNode, CSCERRID.ERR_CannotConvertToExprTree);
            }
            return null;
        }

        //------------------------------------------------------------
        // FUNCBREC.ConvertLambdaExpressionToExpressionTree
        //
        /// <summary></summary>
        /// <param name="lambdaExpr"></param>
        /// <param name="paramDic"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR ConvertLocalToExpressionTree(
            MEMBVARSYM fieldSym,
            EXPR objectExpr)
        {
            EXPR argExpr1 = null;
            EXPR argExpr2 = null;
            EXPR argList = null;
            EXPR argListLast = null;

            EXPR objExpr = MustConvert(
                      objectExpr,
                      Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true),
                      0);
            NewList(objExpr, ref argList, ref argListLast);

            argExpr1 = BindExpressionMethod("Constant", BSYMMGR.EmptyTypeArray, argList);
            argExpr2 = BindFieldInfo(fieldSym);

            argList = null;
            argListLast = null;
            NewList(argExpr1, ref argList, ref argListLast);
            NewList(argExpr2, ref argList, ref argListLast);

            return BindExpressionMethod("Field", BSYMMGR.EmptyTypeArray, argList);
        }
    }
}
#endif
