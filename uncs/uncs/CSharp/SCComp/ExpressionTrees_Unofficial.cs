//============================================================================
// ExpressionTrees_Unofficial.cs
//
// 2016/05/19 (hirano567@hotmail.co.jp)
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
        //============================================================
        // class FUNCBREC.ExpressionTreeConvertInfo
        //============================================================
        internal class ExpressionTreeConvertInfo
        {
            internal StatementListBuilder Builder = null;
            internal Dictionary<string, LOCVARSYM> ParameterDic = null;

            internal SCOPESYM StartScopeSym = null;
            internal List<LOCVARSYM> StartScopeLocalList = null;

            internal List<EXPR> ReturnExprList = new List<EXPR>();
            internal TYPESYM ReturnTypeSym = null;
            internal EXPR ReturnLabelTargetExpr = null;

            internal int ReturnCount = 0;
            internal bool HasMultipleReturns
            {
                get { return this.ReturnCount > 1; }
            }

            internal Dictionary<string, EXPR> TargetDic = null;

            internal EXPR LocVarList = null;
            internal EXPR LocVarListLast = null;
            internal int LocVarCount = 0;

            internal EXPR ExpressionList = null;
            internal EXPR ExpressionListLast = null;
            internal int ExpressionCount = 0;

            //--------------------------------------------------------
            // FUNCBREC.ExpressionTreeConvertInfo.AddLocVar
            //
            /// <summary></summary>
            /// <param name="fbr"></param>
            /// <param name="locExpr"></param>
            //--------------------------------------------------------
            internal void AddLocVar(FUNCBREC fbr, EXPR locExpr)
            {
                fbr.NewList(locExpr, ref LocVarList, ref LocVarListLast);
                ++LocVarCount;
            }

            //--------------------------------------------------------
            // FUNCBREC.ExpressionTreeConvertInfo.AddExpression
            //
            /// <summary></summary>
            /// <param name="fbr"></param>
            /// <param name="expr"></param>
            //--------------------------------------------------------
            internal void AddExpression(FUNCBREC fbr, EXPR expr)
            {
                fbr.NewList(expr, ref ExpressionList, ref ExpressionListLast);
                ++ExpressionCount;
            }

            //--------------------------------------------------------
            // FUNCBREC.ExpressionTreeConvertInfo.Inherit
            //
            /// <summary></summary>
            /// <param name="srcInfo"></param>
            //--------------------------------------------------------
            internal void Inherit(ExpressionTreeConvertInfo srcInfo)
            {
                this.Builder = srcInfo.Builder;
                this.ParameterDic = new Dictionary<string, LOCVARSYM>(srcInfo.ParameterDic);

                this.StartScopeSym = srcInfo.StartScopeSym;
                this.StartScopeLocalList = srcInfo.StartScopeLocalList;

                this.ReturnExprList = srcInfo.ReturnExprList;
                this.ReturnTypeSym = srcInfo.ReturnTypeSym;
                this.ReturnLabelTargetExpr = srcInfo.ReturnLabelTargetExpr;
                this.ReturnCount = srcInfo.ReturnCount;
                this.TargetDic = srcInfo.TargetDic;
            }
        }

        //------------------------------------------------------------
        // FUNCBREC Fields and Properties for expression trees
        //------------------------------------------------------------
        //private AGGTYPESYM typeAts = null;
        //private AGGTYPESYM systemTypeAggTypeSym = null;
        private AGGTYPESYM fieldInfoAts = null;
        private AGGTYPESYM methodInfoAts = null;

        private NSSYM expressionsNsSym = null;
        private NSDECLSYM expressionsDeclSym = null;
        private NSAIDSYM expressionsNsAidSym = null;

        private AGGTYPESYM expressionAts = null;
        private AGGSYM expressionAggSym = null;
        private ARRAYSYM expressionArraySym = null;

        private AGGTYPESYM parameterExpressionAts = null;
        private AGGSYM parameterExpressionAggSym = null;
        private ARRAYSYM parameterExpressionArraySym = null;

        private AGGTYPESYM newArrayExpressionAts = null;
        private AGGSYM newArrayExpressionAggSym = null;

        private AGGTYPESYM switchCaseAts = null;
        private ARRAYSYM switchCaseArraySym = null;

        private AGGTYPESYM catchBlockAts = null;
        private ARRAYSYM catchBlockArraySym = null;

        private EXPR stringConcatInfoExpr1 = null;  // Concat(string, string)
        private EXPR stringConcatInfoExpr2 = null;  // Concat(object, object)

        //------------------------------------------------------------
        // FUNCBREC.SetExpressionTreeFields
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool SetExpressionTreeFields()
        {
            if (parameterExpressionArraySym != null)
            {
                return true;
            }
            if (!SetFncBrecExtFields())
            {
                return false;
            }

#if false
            //NSSYM systemNsSym = Compiler.LookupInBagAid(
            if (systemNsSym == null)
            {
                systemNsSym = Compiler.LookupInBagAid(
                    "System",
                    Compiler.MainSymbolManager.RootNamespaceSym,
                    0,
                    0,
                    SYMBMASK.NSSYM) as NSSYM;
            }
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
#endif

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

            //systemTypeAggTypeSym = typeAggSym.GetThisType();
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

            parameterExpressionAts = LookupExpressionsType("ParameterExpression") as AGGTYPESYM;
            parameterExpressionAggSym = parameterExpressionAts.GetAggregate();
            parameterExpressionArraySym = Compiler.MainSymbolManager.GetArray(
                parameterExpressionAts,
                1,
                parameterExpressionAggSym.Type.MakeArrayType());

            newArrayExpressionAts = LookupExpressionsType("NewArrayExpression") as AGGTYPESYM;
            newArrayExpressionAggSym = newArrayExpressionAts.GetAggregate();

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
            DebugUtil.DebugNodesOutput(sbDebug);
            sbDebug.Length = 0;
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
            ExpressionTreeConvertInfo convertInfo = new ExpressionTreeConvertInfo();
            convertInfo.TargetDic = new Dictionary<string, EXPR>();

            Dictionary<string, LOCVARSYM> paramDic = new Dictionary<string, LOCVARSYM>();
            convertInfo.ParameterDic = paramDic;

            StatementListBuilder builder = new StatementListBuilder();
            convertInfo.Builder = builder;

            List<LOCVARSYM> paramList = new List<LOCVARSYM>();

            //--------------------------------------------------------
            // Define the parameters and convert to the expression tree
            //--------------------------------------------------------
            AnonMethInfo oldCurrentAnonymousMethodInfo = this.currentAnonymousMethodInfo;
            EXPRBLOCK oldBlockExpr = this.currentBlockExpr;

            SCOPESYM oldOuterScopeSym = this.OuterScopeSym;
            SCOPESYM oldCurrentScopeSym = this.currentScopeSym;
            SCOPESYM outerTryScope = this.innermostTryScopeSym;
            SCOPESYM outerCatchScope = this.innermostCatchScopeSym;
            SCOPESYM outerFinallyScope = this.innermostFinallyScopeSym;

            EXPRBLOCK bodyExpr = anonInfo.BodyBlockExpr;
            EXPR exTreeExpr = null;

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

                //----------------------------------------------------
                // Convert the locals of the current scope
                //----------------------------------------------------
                convertInfo.StartScopeSym = this.currentScopeSym;

                List<LOCVARSYM> locVarList = new List<LOCVARSYM>();
                SYM childSym = this.currentScopeSym.FirstChildSym;
                while (childSym != null)
                {
                    if (childSym.Kind == SYMKIND.LOCVARSYM)
                    {
                        LOCVARSYM locSym = childSym as LOCVARSYM;
                        LOCVARSYM tempSym = null;
                        if (locSym != null &&
                            !convertInfo.ParameterDic.TryGetValue(locSym.Name, out tempSym))
                        {
                            locVarList.Add(locSym);
                        }
                    }
                    childSym = childSym.NextSym;
                }
                convertInfo.StartScopeLocalList = locVarList;

                //----------------------------------------------------
                // Convert the parameters
                //----------------------------------------------------
                for (int i = 0; i < paramCount; ++i)
                {
                    LOCVARSYM locSym = srcLambdaExpr.ParameterList[i];
                    string paramLocName
                        = CreateSpecialName(SpecialNameKindEnum.SavedParamOrThis, locSym.Name);
                    EXPRLOCAL locExpr = BindToParameterExpression(
                        null,
                        locSym.Name,
                        locSym.TypeSym,
                        paramLocName,
                        convertInfo) as EXPRLOCAL;
                    if (locExpr != null)
                    {
                        paramList.Add(locExpr.LocVarSym);
                    }
                }

                //----------------------------------------------------
                // Convert to the expression tree
                //----------------------------------------------------
                if (bodyExpr == null)
                {
                    return false;
                }
                EXPRSTMT stmtExpr = bodyExpr.StatementsExpr;

                if (stmtExpr.Kind == EXPRKIND.RETURN &&
                    stmtExpr.NextStatement == null)
                {
                    EXPRRETURN returnExpr = stmtExpr as EXPRRETURN;
                    exTreeExpr = ConvertLambdaExpressionToExpressionTreeCore(
                        returnExpr.ObjectExpr,
                        convertInfo);
                }
                else
                {
                    ScanBlockForReturnAndLabel(
                        bodyExpr.StatementsExpr,
                        convertInfo);

                    convertInfo.ReturnLabelTargetExpr = BindToLabelTarget(
                        null,
                        null,
                        convertInfo.ReturnTypeSym,
                        CreateParameterExpressionLocalName("returnLabelTarget"),
                        convertInfo);

                    Dictionary<string, EXPR> newTargetDic = new Dictionary<string, EXPR>();
                    foreach (KeyValuePair<string, EXPR> kv in convertInfo.TargetDic)
                    {
                        string locName = CreateParameterExpressionLocalName(
                            "LabelTarget" + kv.Key);
                        EXPR expr = BindToLabelTarget(
                            null,
                            kv.Key,
                            null,
                            locName,
                            convertInfo);
                        newTargetDic.Add(kv.Key, expr);
                    }
                    convertInfo.TargetDic = newTargetDic;

                    exTreeExpr = ConvertLambdaExpressionToExpressionTreeTop(
                        bodyExpr,
                        convertInfo);
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

            EXPRRETURN returnLambdaExpr = NewExpr(
                anonInfo.ParseTreeNode,
                EXPRKIND.RETURN,
                null) as EXPRRETURN;
            returnLambdaExpr.ObjectExpr = BindExpressionMethod(
                "Lambda",
                dstAts.TypeArguments,
                argList);
            convertInfo.Builder.Add(returnLambdaExpr);

            anonInfo.ToExpressionTree = true;
            anonInfo.ReturnTypeSym = dstAts;
            anonInfo.ParameterArray = BSYMMGR.EmptyTypeArray;

            TypeArray dgTypeArgs = new TypeArray();
            dgTypeArgs.Add(dstAts);
            dgTypeArgs = Compiler.MainSymbolManager.AllocParams(dgTypeArgs);

            anonInfo.DelegateTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G1_FUNC, true).GetAggregate(),
                dgTypeArgs);

            bodyExpr.StatementsExpr = convertInfo.Builder.GetList();
            dstExpr = srcLambdaExpr;
            srcLambdaExpr.AnonymousMethodInfo.ConversionCompleted = true;

#if DEBUG
            sbDebug.Length = 0;
            DebugUtil.DebugSymsOutput(sbDebug);
            sbDebug.Length = 0;
            DebugUtil.DebugExprsOutput(sbDebug);
#endif

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
            EXPRCLASS expressionExpr =
                NewExpr(null, EXPRKIND.CLASS, expressionAts) as EXPRCLASS;

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
                systemTypeAggTypeSym) as EXPRSYSTEMTYPE;
            typeExpr.Type = type;
            return typeExpr;
        }

#if false
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
#endif
        //------------------------------------------------------------
        // FUNCBREC.BindFieldInfo
        //
        /// <summary></summary>
        /// <param name="fieldSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRFIELDINFO BindFieldInfo(MEMBVARSYM fieldSym,AGGTYPESYM aggTypeSym)
        {
            if (fieldSym == null || aggTypeSym == null)
            {
                return null;
            }

            FieldInfo fieldInfo = ReflectionUtil.GetConstructedFieldInfo(
                Compiler,
                fieldSym,
                aggTypeSym);

            EXPRFIELDINFO fiExpr = NewExpr(
                null,
                EXPRKIND.FIELDINFO,
                fieldInfoAts) as EXPRFIELDINFO;
            fiExpr.FieldInfo = fieldInfo;
            return fiExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindMethodInfo (1)
        //
        /// <summary></summary>
        /// <param name="info"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRMETHODINFO BindMethodInfo(MethodInfo methInfo, Type declaringType)
        {
            if (methInfo == null)
            {
                return null;
            }

            EXPRMETHODINFO miExpr = NewExpr(
                null,
                EXPRKIND.METHODINFO,
                methodInfoAts) as EXPRMETHODINFO;
            miExpr.MethodInfo = methInfo;
            miExpr.DeclaringType = declaringType;
            return miExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindMethodInfo (2)
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <param name="methTypeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRMETHODINFO BindMethodInfo(
            METHSYM methodSym,
            AGGTYPESYM aggTypeSym,
            TypeArray methTypeArray)
        {
            if (methodSym == null)
            {
                return null;
            }

            MethodInfo methodInfo = ReflectionUtil.GetConstructedMethodInfo(
                Compiler,
                methodSym,
                aggTypeSym,
                methTypeArray);

            EXPRMETHODINFO miExpr = NewExpr(
                null,
                EXPRKIND.METHODINFO,
                methodInfoAts) as EXPRMETHODINFO;
            miExpr.MethodInfo = methodInfo;
            miExpr.DeclaringType = aggTypeSym.Type;
            return miExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindConstructorInfo
        //
        /// <summary></summary>
        /// <param name="info"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRCONSTRUCTORINFO BindConstructorInfo2(ConstructorInfo info)
        {
            if (info == null)
            {
                return null;
            }

            EXPRCONSTRUCTORINFO ciExpr = NewExpr(
                null,
                EXPRKIND.CONSTRUCTORINFO,
                methodInfoAts) as EXPRCONSTRUCTORINFO;
            ciExpr.ConstructorInfo = info;
            return ciExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindConstructorInfo
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPRCONSTRUCTORINFO BindConstructorInfo(
            METHSYM constructorSym,
            AGGTYPESYM aggTypeSym)
        {
            if (constructorSym == null)
            {
                return null;
            }

            ConstructorInfo ciInfo = ReflectionUtil.GetConstructedConstructorInfo(
                Compiler,
                constructorSym,
                aggTypeSym);

            EXPRCONSTRUCTORINFO ciExpr = NewExpr(
                null,
                EXPRKIND.CONSTRUCTORINFO,
                methodInfoAts) as EXPRCONSTRUCTORINFO;
            ciExpr.ConstructorInfo = ciInfo;
            return ciExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindToParameterExpression
        //
        /// <summary>
        /// Create a ParameterExpression instance with paramName and paramTypeSym.
        /// Define a local variable with locvarName.
        /// Then assign the instance to the local variable.
        /// Return the EXPR of the local variable.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="paramName"></param>
        /// <param name="paramTypeSym"></param>
        /// <param name="locvarName"></param>
        /// <param name="convertInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindToParameterExpression(
            BASENODE treeNode,
            string paramName,
            TYPESYM paramTypeSym,
            string locvarName,
            ExpressionTreeConvertInfo convertInfo)
        {
            //------------------------------------------------
            // (LHS) Define the SYM and the EXPR for the parameter
            //------------------------------------------------
            LOCVARSYM paramLocSym = DeclareVar(
                null,
                locvarName,
                parameterExpressionAts,
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
            EXPR argList = null;
            EXPR argListLast = null;

            SymUtil.EmitParentSym(Compiler.Emitter, paramTypeSym);
            SymUtil.GetSystemTypeFromSym(paramTypeSym, null, null);
            EXPR typeExpr = BindSystemType(paramTypeSym.Type);

            EXPR nameExpr = NewExprConstant(
                null,
                Compiler.GetReqPredefType(PREDEFTYPE.STRING, true),
                new ConstValInit(paramName));

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

            convertInfo.ParameterDic.Add(paramName, paramLocSym);
            convertInfo.Builder.Add(declExpr);

            return paramLocExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindToLabelTarget
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="targetTypeSym"></param>
        /// <param name="locvarName"></param>
        /// <param name="convertInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindToLabelTarget(
            BASENODE treeNode,
            string targetName,
            TYPESYM targetTypeSym,
            string locvarName,
            ExpressionTreeConvertInfo convertInfo)
        {
            AGGTYPESYM labelTargetAts = LookupExpressionsType("LabelTarget") as AGGTYPESYM;
            AGGSYM labeTargetAggSym = labelTargetAts.GetAggregate();

            if (targetName == null && targetTypeSym == null)
            {
                return null;
            }

            //------------------------------------------------
            // (LHS) Define the SYM and the EXPR for the parameter
            //------------------------------------------------
            LOCVARSYM targetLocSym = DeclareVar(
                null,
                locvarName,
                labelTargetAts,
                false);

            this.uninitedVarCount++;
            //paramLocSym.LocSlotInfo.SetJbitDefAssg(this.uninitedVarCount + 1);
            //int cbit = FlowChecker.GetCbit(Compiler, paramLocSym.TypeSym);
            //this.uninitedVarCount += cbit;
            //this.uninitedVarCount -= cbit;
            targetLocSym.LocSlotInfo.SetJbitDefAssg(0);

            EXPR targetLocExpr = BindToLocal(null, targetLocSym, BindFlagsEnum.Arguments);

            //------------------------------------------------
            // (RHS) Call Expression.Label method.
            //------------------------------------------------
            EXPR argList = null;
            EXPR argListLast = null;

            if (targetTypeSym != null)
            {
                //Compiler.Emitter.EmitAggregateDef(targetTypeSym.GetAggregate());
                SymUtil.EmitParentSym(Compiler.Emitter, targetTypeSym);
                (targetTypeSym as AGGTYPESYM).GetConstructedType(null, null, false);
                EXPR typeExpr = BindSystemType(targetTypeSym.Type);

                NewList(typeExpr, ref argList, ref argListLast);
            }
            if (targetName != null)
            {
                EXPR nameExpr = NewExprConstant(
                    null,
                    Compiler.GetReqPredefType(PREDEFTYPE.STRING, true),
                    new ConstValInit(targetName));

                NewList(nameExpr, ref argList, ref argListLast);
            }

            EXPR callParamExpr
                = BindExpressionMethod("Label", BSYMMGR.EmptyTypeArray, argList);

            //------------------------------------------------
            // Assign
            //------------------------------------------------
            EXPR assgExpr = BindAssignment(null, targetLocExpr, callParamExpr, false);
            targetLocSym.LocSlotInfo.HasInit = true;

            EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = targetLocSym;
            declExpr.InitialExpr = assgExpr;

            convertInfo.Builder.Add(declExpr);

            return targetLocExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateParameterExpressionLocalName
        //
        /// <summary></summary>
        /// <param name="str"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateParameterExpressionLocalName(string name)
        {
            return CreateAnonLocalName("param", name);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateExpressionWrapName
        //
        /// <summary></summary>
        /// <param name="str"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateExpressionWrapName(EXPRWRAP wrapExpr)
        {
            string prefix;

            switch (wrapExpr.TempKind)
            {
                case TEMP_KIND.SHORTLIVED:
                    prefix = "shortlived";
                    break;
                case TEMP_KIND.RETURN:
                    prefix = "return";
                    break;
                case TEMP_KIND.LOCK:
                    prefix = "lock";
                    break;
                case TEMP_KIND.USING:
                    prefix = "using";
                    break;
                case TEMP_KIND.DURABLE:
                    prefix = "durable";
                    break;
                case TEMP_KIND.FOREACH_GETENUM:
                    prefix = "getenum";
                    break;
                case TEMP_KIND.FOREACH_ARRAY:
                    prefix = "foreach_array";
                    break;
                case TEMP_KIND.FOREACH_ARRAYINDEX_0:
                    prefix = "foreach_arrayindex_0";
                    break;
                case TEMP_KIND.FOREACH_ARRAYLIMIT_0:
                    prefix = "foreach_arraylimit_0";
                    break;
                case TEMP_KIND.FIXED_STRING_0:
                    prefix = "fixed_string_0";
                    break;
                default:
                    prefix = "?";
                    break;
            }

            return String.Format("<wrap:{0}>_E{1}", prefix, wrapExpr.ExprID.ToString());
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
        // FUNCBREC.ScanBlockForReturnAndLabel
        //
        /// <summary>
        /// <para>This method does not show error messages.</para>
        /// <para>If the specified return types conflict, selects the former.</para>
        /// </summary>
        ///<param name="stmtExpr"></param>
        ///<param name="convertInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void ScanBlockForReturnAndLabel(
            EXPRSTMT stmtExpr,
            ExpressionTreeConvertInfo convertInfo)
        {
            while (stmtExpr != null)
            {
                switch (stmtExpr.Kind)
                {
                    case EXPRKIND.BLOCK:
                        ScanBlockForReturnAndLabel(
                            (stmtExpr as EXPRBLOCK).StatementsExpr,
                            convertInfo);
                        break;

                    case EXPRKIND.RETURN:
                        EXPRRETURN retExpr = stmtExpr as EXPRRETURN;
                        DebugUtil.Assert(retExpr != null);
                        ++(convertInfo.ReturnCount);

                        if (retExpr.ObjectExpr != null)
                        {
                            TYPESYM tySym = retExpr.ObjectExpr.TypeSym;
                            if (tySym != null &&
                                tySym.Kind != SYMKIND.VOIDSYM &&
                                tySym.Kind != SYMKIND.IMPLICITTYPESYM)
                            {
                                CompareAndSetTypeSym(tySym,
                                    ref convertInfo.ReturnTypeSym);
                            }
                        }
                        break;

                    case EXPRKIND.LABEL:
                        {
                            EXPR expr = null;
                            string name = (stmtExpr as EXPRLABEL).GetIDString();

                            if (!convertInfo.TargetDic.TryGetValue(name, out expr))
                            {
                                convertInfo.TargetDic.Add(name, null);
                            }
                        }
                        break;

                    case EXPRKIND.GOTO:
                    case EXPRKIND.GOTOIF:
                        {
                            EXPRGOTO gotoExpr = stmtExpr as EXPRGOTO;
                            DebugUtil.Assert(gotoExpr != null && gotoExpr.LabelExpr != null);
                            string name = gotoExpr.LabelExpr.GetIDString();
                            EXPR expr = null;

                            if (!convertInfo.TargetDic.TryGetValue(name, out expr))
                            {
                                convertInfo.TargetDic.Add(name, null);
                            }
                        }
                        break;

                    case EXPRKIND.SWITCH:
                        {
                            EXPRSWITCH switchExpr = stmtExpr as EXPRSWITCH;
                            DebugUtil.Assert(switchExpr != null);
                            foreach (EXPRSWITCHLABEL labelExpr in switchExpr.LabelArray)
                            {
                                ScanBlockForReturnAndLabel(
                                    labelExpr.StatementsExpr,
                                    convertInfo);
                            }
                        }
                        break;

                    //case EXPRKIND.SWITCHLABEL:
                    //    break;

                    case EXPRKIND.TRY:
                        {
                            EXPRTRY tryExpr = stmtExpr as EXPRTRY;
                            DebugUtil.Assert(tryExpr != null);

                            ScanBlockForReturnAndLabel(
                                tryExpr.TryBlockExpr.StatementsExpr,
                                convertInfo);

                            EXPRHANDLER handlerExpr = tryExpr.HandlersExpr as EXPRHANDLER;
                            while (handlerExpr != null)
                            {
                                ScanBlockForReturnAndLabel(
                                    handlerExpr,
                                    convertInfo);
                                handlerExpr = handlerExpr.NextStatement as EXPRHANDLER;
                            }
                        }
                        break;

                    case EXPRKIND.HANDLER:
                        {
                            ScanBlockForReturnAndLabel(
                                (stmtExpr as EXPRHANDLER).HandlerBlock.StatementsExpr,
                                convertInfo);
                        }
                        break;

                    default:
                        break;
                }

                stmtExpr = stmtExpr.NextStatement;
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.CompareAndSetTypeSym
        //
        /// <summary></summary>
        /// <param name="newSym"></param>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void CompareAndSetTypeSym(TYPESYM newSym, ref TYPESYM typeSym)
        {
            if (newSym != null)
            {
                if (typeSym == null)
                {
                    typeSym = newSym;
                    return;
                }
                else
                {
                    if (typeSym == newSym ||
                        CanConvert(newSym, typeSym, 0))
                    {
                        // do nothing.
                        return;
                    }
                    else if (CanConvert(typeSym, newSym, 0))
                    {
                        typeSym = newSym;
                        return;
                    }
                    else
                    {
                        // do not show error messages here.
                    }
                }
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.ConvertLambdaExpressionToExpressionTreeTop
        //
        /// <summary></summary>
        /// <param name="srcExpr"></param>
        /// <param name="convertInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR ConvertLambdaExpressionToExpressionTreeTop(
            EXPR srcExpr,
            ExpressionTreeConvertInfo convertInfo)
        {
            EXPR exTreeExpr = ConvertLambdaExpressionToExpressionTreeCore(
                srcExpr,
                convertInfo);

            if (convertInfo.ReturnLabelTargetExpr == null)
            {
                return exTreeExpr;
            }

            EXPR argList1 = null;
            EXPR argListLast1 = null;

            EXPR zeroExpr = NewExprZero(null, convertInfo.ReturnTypeSym);
            EXPR returnTypeExpr = BindSystemType(convertInfo.ReturnTypeSym.Type);
            NewList(zeroExpr, ref argList1, ref argListLast1);
            NewList(returnTypeExpr, ref argList1, ref argListLast1);

            EXPR defaultExpr = BindExpressionMethod(
                "Constant",
                BSYMMGR.EmptyTypeArray,
                argList1);

            EXPR argList2 = null;
            EXPR argListLast2 = null;
            NewList(convertInfo.ReturnLabelTargetExpr, ref argList2, ref argListLast2);
            NewList(defaultExpr, ref argList2, ref argListLast2);

            EXPR returnLabelExpr = BindExpressionMethod(
                "Label",
                BSYMMGR.EmptyTypeArray,
                argList2);

            EXPR argList3 = null;
            EXPR argListLast3 = null;
            NewList(exTreeExpr, ref argList3, ref argListLast3);
            NewList(returnLabelExpr, ref argList3, ref argListLast3);

            return BindExpressionMethod("Block", BSYMMGR.EmptyTypeArray, argList3);
        }

        //------------------------------------------------------------
        // FUNCBREC.ConvertLambdaExpressionToExpressionTreeCore
        //
        /// <summary></summary>
        /// <param name="srcExpr"></param>
        /// <param name="convertInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR ConvertLambdaExpressionToExpressionTreeCore(
            EXPR srcExpr,
            ExpressionTreeConvertInfo convertInfo)
        {
            if (srcExpr == null)
            {
                return null;
            }

            string exMethodName = null;
            CSCERRID errorID = CSCERRID.ERR_NONE;

            switch (srcExpr.Kind)
            {
                //----------------------------------------------------
                // BLOCK
                //----------------------------------------------------
                case EXPRKIND.BLOCK:
                    {
                        EXPRBLOCK blockExpr = srcExpr as EXPRBLOCK;

                        ExpressionTreeConvertInfo blockConvInfo = new ExpressionTreeConvertInfo();
                        blockConvInfo.Inherit(convertInfo);

                        //------------------------------------------------
                        // (1) Define local variables
                        //------------------------------------------------
                        SCOPESYM scopeSym = blockExpr.ScopeSym;
                        if (scopeSym == convertInfo.StartScopeSym &&
                            convertInfo.StartScopeLocalList != null)
                        {
                            foreach (LOCVARSYM locSym in convertInfo.StartScopeLocalList)
                            {
                                string locName = locSym.Name;
                                LOCVARSYM tempSym = null;
                                if (!blockConvInfo.ParameterDic.TryGetValue(locName, out tempSym))
                                {
                                    EXPR locExpr = BindToParameterExpression(
                                        null,
                                        locName,
                                        locSym.TypeSym,
                                        CreateParameterExpressionLocalName(locName),
                                        blockConvInfo);

                                    blockConvInfo.AddLocVar(this, locExpr);
                                }
                            }
                        }
                        else
                        {
                            SYM childSym = scopeSym != null ? scopeSym.FirstChildSym : null;
                            while (childSym != null)
                            {
                                if (childSym.Kind == SYMKIND.LOCVARSYM)
                                {
                                    LOCVARSYM locSym = childSym as LOCVARSYM;
                                    string locName = locSym.Name;
                                    LOCVARSYM tempSym = null;
                                    if (!blockConvInfo.ParameterDic.TryGetValue(locName, out tempSym))
                                    {
                                        EXPR locExpr = BindToParameterExpression(
                                            null,
                                            locName,
                                            locSym.TypeSym,
                                            CreateParameterExpressionLocalName(locName),
                                            blockConvInfo);

                                        blockConvInfo.AddLocVar(this, locExpr);
                                    }
                                }
                                childSym = childSym.NextSym;
                            }
                        }

                        //------------------------------------------------
                        // BLOCK (2) Convert each statement.
                        //------------------------------------------------
                        for (EXPRSTMT stmtExpr = blockExpr.StatementsExpr;
                            stmtExpr != null;
                            stmtExpr = stmtExpr.NextStatement)
                        {
#if DEBUG
                            if (stmtExpr.ExprID == 7984)
                            {
                                ;
                            }
#endif
                            EXPR exprExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                stmtExpr,
                                blockConvInfo);
                            if (exprExpr != null)
                            {
                                blockConvInfo.AddExpression(this, exprExpr);
                            }
                        }

                        //------------------------------------------------
                        // BLOCK (3) Call Expression.Block method.
                        //------------------------------------------------
                        EXPRARRINIT varArrInitExpr = NewExpr(
                            null,
                            EXPRKIND.ARRINIT,
                            parameterExpressionArraySym) as EXPRARRINIT;

                        varArrInitExpr.ArgumentsExpr = blockConvInfo.LocVarList;
                        varArrInitExpr.DimSizes.Add(blockConvInfo.LocVarCount);

                        EXPRARRINIT exprArrInitExpr = NewExpr(
                            null,
                            EXPRKIND.ARRINIT,
                            expressionArraySym) as EXPRARRINIT;

                        if (blockConvInfo.ExpressionCount == 0)
                        {
                            blockConvInfo.AddExpression(this, BindExpressionNoOp());
                        }

                        exprArrInitExpr.ArgumentsExpr = blockConvInfo.ExpressionList;
                        exprArrInitExpr.DimSizes.Add(blockConvInfo.ExpressionCount);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        NewList(varArrInitExpr, ref argList1, ref argListLast1);
                        NewList(exprArrInitExpr, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Block", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // STMTAS
                //----------------------------------------------------
                case EXPRKIND.STMTAS:
                    {
                        return ConvertLambdaExpressionToExpressionTreeCore(
                            (srcExpr as EXPRSTMTAS).Expr,
                            convertInfo);
                    }

                //----------------------------------------------------
                // RETURN
                //----------------------------------------------------
                case EXPRKIND.RETURN:
                    {
                        EXPRRETURN retExpr = srcExpr as EXPRRETURN;
                        DebugUtil.Assert(retExpr != null);

                        TYPESYM retTypeSym = null;
                        if (retExpr.ObjectExpr != null)
                        {
                            retTypeSym = retExpr.ObjectExpr.TypeSym;
                        }

                        if (convertInfo.ReturnTypeSym == retTypeSym)
                        {
                            // normal
                        }
                        else if (convertInfo.ReturnTypeSym != null && retTypeSym == null)
                        {
                            Compiler.Error(
                                retExpr.TreeNode,
                                CSCERRID.ERR_RetObjectRequired,
                                new ErrArg(convertInfo.ReturnTypeSym));
                        }
                        else if (convertInfo.ReturnTypeSym == null && retTypeSym != null)
                        {
                            // This cannot happen.
                            Compiler.Error(
                                retExpr.TreeNode,
                                CSCERRID.ERR_RetNoObjectRequired,
                                new ErrArg("lambda expression"));
                        }
                        else if (CanConvert(retTypeSym, convertInfo.ReturnTypeSym, 0))
                        {
                            // normal
                        }
                        else
                        {
                            Compiler.Error(
                                retExpr.ObjectExpr.TreeNode,
                                CSCERRID.ERR_NoImplicitConv,
                                new ErrArg(retTypeSym),
                                new ErrArg(convertInfo.ReturnTypeSym));
                        }

                        EXPR retObjExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            MustConvert(retExpr.ObjectExpr, convertInfo.ReturnTypeSym, 0),
                            convertInfo);
                        EXPR retTypeExpr = BindSystemType(convertInfo.ReturnTypeSym.Type);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        NewList(convertInfo.ReturnLabelTargetExpr, ref argList1, ref argListLast1);
                        NewList(retObjExpr, ref argList1, ref argListLast1);
                        NewList(retTypeExpr, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Return", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // DECL
                //----------------------------------------------------
                case EXPRKIND.DECL:
                    {
                        EXPRDECL declExpr = srcExpr as EXPRDECL;
                        DebugUtil.Assert(declExpr != null && declExpr.LocVarSym != null);

                        if (declExpr.InitialExpr != null)
                        {
                            EXPR initExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                declExpr.InitialExpr,
                                convertInfo);
                            if (initExpr != null)
                            {
                                convertInfo.AddExpression(this, initExpr);
                            }
                        }
                        return null;
                    }

                //----------------------------------------------------
                // LABEL
                //----------------------------------------------------
                case EXPRKIND.LABEL:
                    {
                        EXPR targetExpr = null;
                        string name = (srcExpr as EXPRLABEL).GetIDString();

                        if (!convertInfo.TargetDic.TryGetValue(
                                name,
                                out targetExpr) ||
                            targetExpr == null)
                        {
                            DebugUtil.Assert(false);
                            return null;
                        }

                        return BindExpressionMethod("Label", BSYMMGR.EmptyTypeArray, targetExpr);
                    }

                //----------------------------------------------------
                // GOTO
                //----------------------------------------------------
                case EXPRKIND.GOTO:
                    {
                        EXPRGOTO gotoExpr = srcExpr as EXPRGOTO;
                        DebugUtil.Assert(gotoExpr != null && gotoExpr.LabelExpr != null);

                        EXPR targetExpr = null;
                        string name = gotoExpr.LabelExpr.GetIDString();

                        if (!convertInfo.TargetDic.TryGetValue(
                                name,
                                out targetExpr) ||
                                targetExpr == null)
                        {
                            DebugUtil.Assert(false);
                            return null;
                        }

                        return BindExpressionMethod("Goto", BSYMMGR.EmptyTypeArray, targetExpr);
                    }

                //----------------------------------------------------
                // GOTOIF
                //----------------------------------------------------
                case EXPRKIND.GOTOIF:
                    {
                        EXPRGOTOIF gotoIfExpr = srcExpr as EXPRGOTOIF;
                        DebugUtil.Assert(
                            gotoIfExpr != null &&
                            gotoIfExpr.ConditionExpr != null &&
                            gotoIfExpr.LabelExpr != null);

                        EXPR conditionExpr = null;
                        EXPR conditionExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            gotoIfExpr.ConditionExpr,
                            convertInfo);

                        if (gotoIfExpr.HasSense)
                        {
                            conditionExpr = conditionExpr1;
                        }
                        else
                        {
                            conditionExpr = BindExpressionMethod(
                                "Not",
                                BSYMMGR.EmptyTypeArray,
                                conditionExpr1);
                        }

                        EXPR targetExpr = null;
                        string name = gotoIfExpr.LabelExpr.GetIDString();

                        if (!convertInfo.TargetDic.TryGetValue(
                                name,
                                out targetExpr) ||
                                targetExpr == null)
                        {
                            DebugUtil.Assert(false);
                            return null;
                        }
                        EXPR gotoExpr
                            = BindExpressionMethod("Goto", BSYMMGR.EmptyTypeArray, targetExpr);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        NewList(conditionExpr, ref argList1, ref argListLast1);
                        NewList(gotoExpr, ref argList1, ref argListLast1);

                        return BindExpressionMethod("IfThen", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // SWITCH
                //----------------------------------------------------
                case EXPRKIND.SWITCH:
                    {
                        if (this.switchCaseAts == null)
                        {
                            this.switchCaseAts = LookupExpressionsType("SwitchCase") as AGGTYPESYM;
                            this.switchCaseArraySym = Compiler.MainSymbolManager.GetArray(
                                this.switchCaseAts,
                                1,
                                this.switchCaseAts.Type.MakeArrayType());

                            DebugUtil.Assert(this.switchCaseAts != null);
                            DebugUtil.Assert(this.switchCaseArraySym != null);
                        }

                        EXPRSWITCH switchExpr = srcExpr as EXPRSWITCH;
                        DebugUtil.Assert(switchExpr != null);

                        EXPR breakLabelExpr = null;
                        string breakLabelName = switchExpr.BreakLabelExpr.GetIDString();
                        if (!convertInfo.TargetDic.TryGetValue(
                                breakLabelName,
                                out breakLabelExpr))
                        {
                            DebugUtil.Assert(false);
                            break;
                        }

                        EXPR argList = null;
                        EXPR argListLast = null;

                        //--------------------------------------------
                        // Switch value
                        //--------------------------------------------
                        EXPR switchValueExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            switchExpr.ArgumentExpr,
                            convertInfo);

                        //--------------------------------------------
                        // Cases
                        //--------------------------------------------
                        List<EXPRSWITCHLABEL> switchLabelList = switchExpr.LabelArray;

                        EXPR defaultCaseExpr = null;
                        EXPR switchCaseList = null;
                        EXPR switchCaseListLast = null;
                        int switchCaseCount = 0;

                        EXPR testValList = null;
                        EXPR testValListLast = null;
                        int testValCount = 0;
                        bool isDefault = false;
                        bool foundDefault = false;

                        for (int i = 0; i < switchLabelList.Count; ++i)
                        {
                            EXPRSWITCHLABEL swLabelExpr = switchLabelList[i];

                            //----------------------------------------
                            // Key
                            //----------------------------------------
                            if (swLabelExpr.KeyExpr == null)
                            {
                                DebugUtil.Assert(!foundDefault);
                                isDefault = true;
                                foundDefault = true;
                            }
                            else
                            {
                                EXPR testValueExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                    swLabelExpr.KeyExpr,
                                    convertInfo
                                    );
                                if (testValueExpr == null)
                                {
                                    continue;
                                }
                                NewList(testValueExpr, ref testValList, ref testValListLast);
                                ++testValCount;
                            }

                            if (swLabelExpr.StatementsExpr == null)
                            {
                                continue;
                            }

                            //----------------------------------------
                            // Statements
                            //----------------------------------------
                            ExpressionTreeConvertInfo caseConvInfo = new ExpressionTreeConvertInfo();
                            caseConvInfo.Inherit(convertInfo);

                            EXPRSTMT stmtExpr = swLabelExpr.StatementsExpr;
                            while (stmtExpr != null)
                            {
                                EXPR extrExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                    stmtExpr,
                                    caseConvInfo);

                                if (extrExpr != null)
                                {
                                    caseConvInfo.AddExpression(this, extrExpr);
                                }
                                stmtExpr = stmtExpr.NextStatement;
                            }

                            if (caseConvInfo.ExpressionCount > 1 ||
                                caseConvInfo.ExpressionList.Kind != EXPRKIND.GOTO ||
                                (caseConvInfo.ExpressionList as EXPRGOTO).LabelExpr
                                    != switchExpr.BreakLabelExpr)
                            {
                                EXPR gtExpr = BindExpressionMethod(
                                    "Goto",
                                    BSYMMGR.EmptyTypeArray,
                                    breakLabelExpr);
                                caseConvInfo.AddExpression(this, gtExpr);
                            }

                            EXPRARRINIT caseBodyArrInitExpr = NewExpr(
                                null,
                                EXPRKIND.ARRINIT,
                                expressionArraySym) as EXPRARRINIT;

                            caseBodyArrInitExpr.ArgumentsExpr = caseConvInfo.ExpressionList;
                            caseBodyArrInitExpr.DimSizes.Add(caseConvInfo.ExpressionCount);

                            EXPR caseBodyExpr = BindExpressionMethod(
                                "Block",
                                BSYMMGR.EmptyTypeArray,
                                caseBodyArrInitExpr);

                            if (isDefault)
                            {
                                defaultCaseExpr = caseBodyExpr;
                                isDefault = false;
                            }
                            else
                            {
                                EXPRARRINIT testValArrInitExpr = NewExpr(
                                    null,
                                    EXPRKIND.ARRINIT,
                                    expressionArraySym) as EXPRARRINIT;
                                testValArrInitExpr.ArgumentsExpr = testValList;
                                testValArrInitExpr.DimSizes.Add(testValCount);

                                argList = null;
                                argListLast = null;
                                NewList(caseBodyExpr, ref argList, ref argListLast);
                                NewList(testValArrInitExpr, ref argList, ref argListLast);

                                EXPR switchCaseExpr = BindExpressionMethod(
                                    "SwitchCase",
                                    BSYMMGR.EmptyTypeArray,
                                    argList);
                                NewList(switchCaseExpr, ref switchCaseList, ref switchCaseListLast);
                                ++switchCaseCount;
                            }

                            testValList = null;
                            testValListLast = null;
                            testValCount = 0;
                        }

                        //--------------------------------------------
                        // Bind to Expression.Switch method.
                        //--------------------------------------------
                        argList = null;
                        argListLast = null;

                        NewList(switchValueExpr, ref argList, ref argListLast);

                        if (defaultCaseExpr != null)
                        {
                            NewList(defaultCaseExpr, ref argList, ref argListLast);
                        }

                        EXPRARRINIT switchCaseArrInitExpr = NewExpr(
                            null,
                            EXPRKIND.ARRINIT,
                            this.switchCaseArraySym) as EXPRARRINIT;
                        switchCaseArrInitExpr.ArgumentsExpr = switchCaseList;
                        switchCaseArrInitExpr.DimSizes.Add(switchCaseCount);
                        NewList(switchCaseArrInitExpr, ref argList, ref argListLast);

                        return BindExpressionMethod("Switch", BSYMMGR.EmptyTypeArray, argList);
                    }

                //----------------------------------------------------
                // SWITCHLABEL
                //----------------------------------------------------
                case EXPRKIND.SWITCHLABEL:
                    {
                        DebugUtil.Assert(false);
                        break;
                    }

                //----------------------------------------------------
                // TRY
                //----------------------------------------------------
                case EXPRKIND.TRY:
                    {
                        if (this.catchBlockAts == null)
                        {
                            this.catchBlockAts = LookupExpressionsType("CatchBlock") as AGGTYPESYM;
                            this.catchBlockArraySym = Compiler.MainSymbolManager.GetArray(
                                this.catchBlockAts,
                                1,
                                this.catchBlockAts.Type.MakeArrayType());

                            DebugUtil.Assert(this.catchBlockAts != null);
                            DebugUtil.Assert(this.catchBlockArraySym != null);
                        }

                        EXPRTRY tryExpr = srcExpr as EXPRTRY;
                        DebugUtil.Assert(tryExpr != null);

                        EXPR bodyExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            tryExpr.TryBlockExpr,
                            convertInfo);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;

                        NewList(bodyExpr, ref argList1, ref argListLast1);

                        if ((tryExpr.Flags & EXPRFLAG.ISFINALLY) != 0)
                        {
                            EXPR finallyExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                tryExpr.HandlersExpr,
                                convertInfo);
                            NewList(finallyExpr, ref argList1, ref argListLast1);

                            return BindExpressionMethod(
                                "TryFinally",
                                BSYMMGR.EmptyTypeArray,
                                argList1);
                        }
                        else
                        {
                            EXPR argList2 = null;
                            EXPR argListLast2 = null;
                            int catchCount = 0;

                            EXPRSTMT currentExpr = tryExpr.HandlersExpr as EXPRSTMT;
                            while (currentExpr != null)
                            {
                                if (currentExpr.Kind == EXPRKIND.HANDLER)
                                {
                                    EXPR handlerExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                        currentExpr,
                                        convertInfo);
                                    if (handlerExpr != null)
                                    {
                                        NewList(handlerExpr, ref argList2, ref argListLast2);
                                        ++catchCount;
                                    }
                                }
                                currentExpr = currentExpr.NextStatement as EXPRSTMT;
                            }

                            EXPRARRINIT catchArrInitExpr = NewExpr(
                                null,
                                EXPRKIND.ARRINIT,
                                catchBlockArraySym) as EXPRARRINIT;
                            catchArrInitExpr.ArgumentsExpr = argList2;
                            catchArrInitExpr.DimSizes.Add(catchCount);

                            NewList(catchArrInitExpr, ref argList1, ref argListLast1);

                            return BindExpressionMethod(
                                "TryCatch",
                                BSYMMGR.EmptyTypeArray,
                                argList1);
                        }
                    }

                //----------------------------------------------------
                // HANDLER
                //----------------------------------------------------
                case EXPRKIND.HANDLER:
                    {
                        EXPRHANDLER handlerExpr = srcExpr as EXPRHANDLER;
                        if (handlerExpr == null)
                        {
                            return null;
                        }

                        ExpressionTreeConvertInfo catchConvInfo = new ExpressionTreeConvertInfo();
                        catchConvInfo.Inherit(convertInfo);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;

                        //--------------------------------------------
                        // with an exception object
                        //--------------------------------------------
                        if (handlerExpr.ParameterSym != null)
                        {
                            string paramName = handlerExpr.ParameterSym.Name;
                            EXPR paramExpr = BindToParameterExpression(
                                null,
                                paramName,
                                handlerExpr.ParameterSym.TypeSym,
                                CreateParameterExpressionLocalName(paramName),
                                catchConvInfo);

                            NewList(paramExpr, ref argList1, ref argListLast1);
                        }
                        //--------------------------------------------
                        // with no exception object
                        //--------------------------------------------
                        else
                        {
                            TYPESYM handlerTypeSym = handlerExpr.TypeSym;
                            //Compiler.Emitter.EmitAggregateDef(handlerTypeSym.GetAggregate());
                            SymUtil.EmitParentSym(Compiler.Emitter, handlerTypeSym);
                            AGGTYPESYM handlerAts = handlerTypeSym as AGGTYPESYM;
                            if (handlerAts != null)
                            {
                                SymUtil.GetSystemTypeFromSym(handlerAts, null, null);
                            }
                            EXPR typeExpr = BindSystemType(handlerExpr.TypeSym.Type);
                            NewList(typeExpr, ref argList1, ref argListLast1);
                        }

                        EXPR catchBlockExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            handlerExpr.HandlerBlock,
                            catchConvInfo);
                        if (catchBlockExpr != null)
                        {
                            NewList(catchBlockExpr, ref argList1, ref argListLast1);

                            return BindExpressionMethod(
                                "Catch",
                                BSYMMGR.EmptyTypeArray,
                                argList1);
                        }

                        return null;
                    }

                //----------------------------------------------------
                // THROW
                //----------------------------------------------------
                case EXPRKIND.THROW:
                    {
                        EXPR argList1 = ConvertLambdaExpressionToExpressionTreeCore(
                            (srcExpr as EXPRTHROW).ObjectExpr,
                            convertInfo);

                        return BindExpressionMethod("Throw", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // NOOP
                //----------------------------------------------------
                case EXPRKIND.NOOP:
                    {
                        return null;
                    }

                //----------------------------------------------------
                // DEBUGNOOP
                //----------------------------------------------------
                case EXPRKIND.DEBUGNOOP:
                    {
                        return null;
                    }

                //----------------------------------------------------
                // 
                //----------------------------------------------------
                case EXPRKIND.DELIM:
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
                    {
                        EXPRCALL callExpr = srcExpr as EXPRCALL;
                        DebugUtil.Assert(callExpr != null);
                        METHSYM methodSym = callExpr.MethodWithInst.MethSym;

                        Compiler.Emitter.EmitMethodDef(methodSym);
                        string methodName = methodSym.Name;
                        bool isConstructor = (methodSym.MethodKind == MethodKindEnum.Ctor);

                        //--------------------------------------------
                        // (1) op_Equality, op_Inequality
                        //--------------------------------------------
                        if (methodName == "op_Equality" || methodName == "op_Inequality")
                        {
                            EXPR argList1 = null;
                            EXPR argListLast1 = null;
                            EXPR tempExpr1 = null;
                            EXPR tempExpr2 = null;

                            //----------------------------------------
                            // (1-1) Arguments
                            //----------------------------------------
                            DebugUtil.Assert(callExpr.ArgumentsExpr != null);
                            EXPR expr = callExpr.ArgumentsExpr;
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

                                tempExpr2 = ConvertLambdaExpressionToExpressionTreeCore(
                                    tempExpr1,
                                    convertInfo);
                                if (tempExpr2 != null)
                                {
                                    //tempExpr2 = MustConvert(tempExpr2, expressionAts, 0);
                                    NewList(tempExpr2, ref argList1, ref argListLast1);
                                    ++count;
                                }
                            }

                            if (count != 2)
                            {
                                // error
                            }

                            //----------------------------------------
                            // (1-2) LiftToNull
                            //----------------------------------------
                            tempExpr1 = NewExprConstant(
                                null,
                                Compiler.GetReqPredefType(PREDEFTYPE.BOOL, true),
                                new ConstValInit(false));
                            tempExpr1.Flags |= EXPRFLAG.LITERALCONST;

                            NewList(tempExpr1, ref argList1, ref argListLast1);

                            //----------------------------------------
                            // (1-3) MethodInfo
                            //----------------------------------------
                            tempExpr1 = BindMethodInfo(
                                callExpr.MethodWithInst.MethSym,
                                callExpr.MethodWithInst.AggTypeSym,
                                callExpr.MethodWithInst.TypeArguments);
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
                        //--------------------------------------------
                        // (2) Constructor with no argument.
                        //--------------------------------------------
                        else if (isConstructor && callExpr.ArgumentsExpr == null)
                        {
                            AGGTYPESYM aggTypeSym = callExpr.MethodWithInst.AggTypeSym;
                            Compiler.Emitter.EmitAggregateDef(aggTypeSym.GetAggregate());
                            SymUtil.GetSystemTypeFromSym(aggTypeSym, null, null);

                            return BindExpressionMethod("New",
                                BSYMMGR.EmptyTypeArray,
                                BindSystemType(aggTypeSym.Type));
                        }
                        //--------------------------------------------
                        // (3) Otherwise
                        //--------------------------------------------
                        else
                        {
                            EXPR argList1 = null;
                            EXPR argListLast1 = null;
                            EXPR argList2 = null;
                            EXPR argListLast2 = null;
                            EXPR tempExpr1 = null;
                            EXPR tempExpr2 = null;

                            //--------------------------------------------
                            // Object
                            //--------------------------------------------
                            if (callExpr.ObjectExpr != null)
                            {
                                tempExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                                    callExpr.ObjectExpr,
                                    convertInfo);
                                NewList(tempExpr1, ref argList1, ref argListLast1);
                            }

                            //--------------------------------------------
                            // method
                            //--------------------------------------------
                            if (isConstructor)
                            {
                                tempExpr1 = BindConstructorInfo(
                                    callExpr.MethodWithInst.MethSym,
                                    callExpr.MethodWithInst.AggTypeSym);
                            }
                            else
                            {
                                tempExpr1 = BindMethodInfo(
                                    callExpr.MethodWithInst.MethSym,
                                    callExpr.MethodWithInst.AggTypeSym,
                                    callExpr.MethodWithInst.TypeArguments);
                            }
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

                                    tempExpr2 = ConvertLambdaExpressionToExpressionTreeCore(
                                        tempExpr1,
                                        convertInfo);
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

                            if (isConstructor)
                            {
                                return BindExpressionMethod("New", BSYMMGR.EmptyTypeArray, argList1);
                            }
                            else
                            {
                                return BindExpressionMethod("Call", BSYMMGR.EmptyTypeArray, argList1);
                            }
                        }
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
                        AGGTYPESYM aggTypeSym = fieldExpr.FieldWithType.AggTypeSym;

                        Compiler.Emitter.EmitMembVarDef(fieldSym);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        EXPR tempExpr1 = null;

                        //--------------------------------------------
                        // Object
                        //--------------------------------------------
                        DebugUtil.Assert(fieldExpr.ObjectExpr != null);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            fieldExpr.ObjectExpr,
                            convertInfo);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        //--------------------------------------------
                        // Field
                        //--------------------------------------------
                        tempExpr1 = BindFieldInfo(fieldSym, aggTypeSym);
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
                        if (convertInfo.ParameterDic.TryGetValue(localSym.Name, out valSym))
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

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        EXPR tempExpr1 = null;

                        tempExpr1 = MustConvert(
                            constExpr,
                            Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true),
                            0);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        tempExpr1 = BindSystemType(constExpr.TypeSym.Type);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Constant", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // 
                //----------------------------------------------------
                case EXPRKIND.CLASS:
                case EXPRKIND.NSPACE:
                case EXPRKIND.ERROR:
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

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        EXPR tempExpr1 = null;

                        //--------------------------------------------
                        // Object
                        //--------------------------------------------
                        DebugUtil.Assert(propExpr.ObjectExpr != null);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            propExpr.ObjectExpr,
                            convertInfo);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        //--------------------------------------------
                        // Accessor
                        //--------------------------------------------
                        if ((propExpr.Flags & EXPRFLAG.MEMBERSET) != 0)
                        {
                            tempExpr1 = BindMethodInfo(
                                propExpr.SetMethodWithType.MethSym,
                                propExpr.SetMethodWithType.AggTypeSym,
                                null);
                        }
                        else
                        {
                            tempExpr1 = BindMethodInfo(
                                propExpr.GetMethodWithType.MethSym,
                                propExpr.GetMethodWithType.AggTypeSym,
                                null);
                        }
                        DebugUtil.Assert(tempExpr1 != null);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Property", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // MULTI
                //----------------------------------------------------
                case EXPRKIND.MULTI:
                    {
                        EXPRMULTI multiExpr = srcExpr as EXPRMULTI;

                        EXPR leftExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            multiExpr.LeftExpr,
                            convertInfo);

                        EXPRBINOP operandExpr = multiExpr.OperandExpr as EXPRBINOP;
                        DebugUtil.Assert(operandExpr != null);

                        string methodName = null;
                        bool isPost = (multiExpr.Flags & EXPRFLAG.ISPOSTOP) != 0;

                        switch (operandExpr.Kind)
                        {
                            case EXPRKIND.ADD:
                                if (isPost)
                                {
                                    return BindExpressionMethod(
                                        "PostIncrementAssign",
                                        BSYMMGR.EmptyTypeArray,
                                        leftExpr);
                                }
                                methodName = IsCheckOverflowSet(srcExpr.Flags) ?
                                    "AddAssignChecked" : "AddAssign";
                                break;

                            case EXPRKIND.SUB:
                                if (isPost)
                                {
                                    return BindExpressionMethod(
                                        "PostDecrementAssign",
                                        BSYMMGR.EmptyTypeArray,
                                        leftExpr);
                                }
                                methodName = IsCheckOverflowSet(srcExpr.Flags) ?
                                    "SubtractAssignChecked" : "SubtractAssign";
                                break;

                            case EXPRKIND.MUL:
                                methodName = IsCheckOverflowSet(srcExpr.Flags) ?
                                    "MultiplyAssignChecked" : "MultiplyAssign";
                                break;

                            case EXPRKIND.DIV:
                                methodName = "DivideAssign";
                                break;

                            case EXPRKIND.MOD:
                                methodName = "ModuloAssign";
                                break;

                            case EXPRKIND.BITAND:
                                methodName = "AndAssign";
                                break;

                            case EXPRKIND.BITXOR:
                                methodName = "ExclusiveOrAssign";
                                break;

                            case EXPRKIND.BITOR:
                                methodName = "OrAssign";
                                break;

                            case EXPRKIND.LSHIFT:
                                methodName = "LeftShiftAssign";
                                break;

                            case EXPRKIND.RSHIFT:
                                methodName = "RightShiftAssign";
                                break;

                            default:
                                DebugUtil.Assert(false);
                                return null;
                        }

                        EXPR valueExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            operandExpr.Operand2,
                            convertInfo);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        NewList(leftExpr, ref argList1, ref argListLast1);
                        NewList(valueExpr, ref argList1, ref argListLast1);

                        return BindExpressionMethod(methodName, BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // MULTIGET
                //----------------------------------------------------
                case EXPRKIND.MULTIGET:
                    {
                        break;
                    }

                //----------------------------------------------------
                // 
                //----------------------------------------------------
                case EXPRKIND.STTMP:
                case EXPRKIND.LDTMP:
                case EXPRKIND.FREETMP:
                    break;

                //----------------------------------------------------
                // WRAP
                //----------------------------------------------------
                case EXPRKIND.WRAP:
                    {
                        EXPRWRAP wrapExpr = srcExpr as EXPRWRAP;

                        // if Expr fields is of EXPRWRAP,
                        // it means that temporary slot should be free.
                        // So, this EXPR does not have to be converted. 
                        if (wrapExpr.Expr != null &&
                            wrapExpr.Expr.Kind == EXPRKIND.WRAP)
                        {
                            return null;
                        }

                        switch (wrapExpr.TempKind)
                        {
                            case TEMP_KIND.SHORTLIVED:
                            case TEMP_KIND.RETURN:
                            case TEMP_KIND.LOCK:
                            case TEMP_KIND.USING:
                            case TEMP_KIND.DURABLE:
                                break;

                            //----------------------------------------
                            // FOREACH_GETENUM
                            //----------------------------------------
                            case TEMP_KIND.FOREACH_GETENUM:
                                {
                                    DebugUtil.Assert(wrapExpr.Expr != null);

                                    TYPESYM enumSym = wrapExpr.Expr.TypeSym;
                                    DebugUtil.Assert(enumSym != null);

                                    string wrapName = CreateExpressionWrapName(wrapExpr);

                                    LOCVARSYM locSym = null;
                                    if (convertInfo.ParameterDic.TryGetValue(wrapName, out locSym))
                                    {
                                        return BindToLocal(null, locSym, BindFlagsEnum.RValueRequired);
                                    }

                                    string locName = CreateAnonLocalName(wrapName);

                                    EXPR locExpr = BindToParameterExpression(
                                        null,
                                        wrapName,
                                        enumSym,
                                        locName,
                                        convertInfo);
                                    convertInfo.AddLocVar(this, locExpr);
                                    return locExpr;
                                }

                            //----------------------------------------
                            // FOREACH_ARRAY
                            //----------------------------------------
                            case TEMP_KIND.FOREACH_ARRAY:
                                {
                                    DebugUtil.Assert(wrapExpr.Expr != null);

                                    TYPESYM arraySym = wrapExpr.Expr.TypeSym;
                                    DebugUtil.Assert(arraySym != null);

                                    string wrapName = CreateExpressionWrapName(wrapExpr);

                                    LOCVARSYM locSym = null;
                                    if (convertInfo.ParameterDic.TryGetValue(wrapName, out locSym))
                                    {
                                        return BindToLocal(null, locSym, BindFlagsEnum.RValueRequired);
                                    }

                                    string locName = CreateAnonLocalName(wrapName);

                                    EXPR locExpr = BindToParameterExpression(
                                        null,
                                        wrapName,
                                        arraySym,
                                        locName,
                                        convertInfo);
                                    convertInfo.AddLocVar(this, locExpr);
                                    return locExpr;
                                }

                            //----------------------------------------
                            // FOREACH_ARRAYINDEX_0
                            //----------------------------------------
                            case TEMP_KIND.FOREACH_ARRAYINDEX_0:
                                {
                                    string wrapName = CreateExpressionWrapName(wrapExpr);

                                    LOCVARSYM locSym = null;
                                    if (convertInfo.ParameterDic.TryGetValue(wrapName, out locSym))
                                    {
                                        return BindToLocal(null, locSym, BindFlagsEnum.RValueRequired);
                                    }

                                    string locName = CreateAnonLocalName(wrapName);

                                    EXPR locExpr = BindToParameterExpression(
                                        null,
                                        wrapName,
                                        Compiler.GetReqPredefType(PREDEFTYPE.INT, true),
                                        locName,
                                        convertInfo);
                                    convertInfo.AddLocVar(this, locExpr);
                                    return locExpr;
                                }

                            case TEMP_KIND.FOREACH_ARRAYLIMIT_0:
                            case TEMP_KIND.FIXED_STRING_0:
                                break;
                        }
                        break;
                    }

                //----------------------------------------------------
                // CONCAT
                //----------------------------------------------------
                case EXPRKIND.CONCAT:
                    {
                        EXPRCONCAT concatExpr = srcExpr as EXPRCONCAT;
                        DebugUtil.Assert(
                            concatExpr.List != null &&
                            concatExpr.List.Kind == EXPRKIND.LIST);

                        EXPR expr = concatExpr.List.AsBIN.Operand2;
                        EXPR argExpr1 = concatExpr.List.AsBIN.Operand1;
                        EXPR argExpr2 = null;

                        //--------------------------------------------
                        // first + second
                        //--------------------------------------------
                        if (expr.Kind == EXPRKIND.LIST)
                        {
                            argExpr2 = expr.AsBIN.Operand1;
                            expr = expr.AsBIN.Operand2;
                        }
                        else
                        {
                            argExpr2 = expr;
                            expr = null;
                        }

                        argExpr1 = ConvertConcatToExpressionTree(
                            argExpr1,
                            false,
                            argExpr1.TypeSym.IsPredefType(PREDEFTYPE.STRING),
                            argExpr2,
                            false,
                            argExpr2.TypeSym.IsPredefType(PREDEFTYPE.STRING),
                            convertInfo);

                        //--------------------------------------------
                        // + third ...
                        //--------------------------------------------
                        while (expr != null)
                        {
                            if (expr.Kind == EXPRKIND.LIST)
                            {
                                argExpr2 = expr.AsBIN.Operand1;
                                expr = expr.AsBIN.Operand2;
                            }
                            else
                            {
                                argExpr2 = expr;
                                expr = null;
                            }

                            argExpr1 = ConvertConcatToExpressionTree(
                                argExpr1,
                                true,
                                true,
                                argExpr2,
                                false,
                                argExpr2.TypeSym.IsPredefType(PREDEFTYPE.STRING),
                                convertInfo);
                        }

                        return argExpr1;
                    }

                //----------------------------------------------------
                // ARRINIT
                //----------------------------------------------------
                case EXPRKIND.ARRINIT:
                    {
                        EXPRARRINIT arrInitExpr = srcExpr as EXPRARRINIT;
                        DebugUtil.Assert(arrInitExpr != null);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        EXPR argList2 = null;
                        EXPR argListLast2 = null;

                        ARRAYSYM arrTypeSym = arrInitExpr.TypeSym as ARRAYSYM;
                        DebugUtil.Assert(arrTypeSym != null);
                        SymUtil.EmitParentSym(Compiler.Emitter, arrTypeSym);
                        SymUtil.GetSystemTypeFromSym(arrTypeSym, null, null);

                        TYPESYM elementTypeSym = arrTypeSym.ElementTypeSym;
                        //Compiler.Emitter.EmitAggregateDef(elementTypeSym.GetAggregate());
                        SymUtil.GetSystemTypeFromSym(elementTypeSym, null, null);

                        EXPR typeExpr = BindSystemType(elementTypeSym.Type);
                        //NewList(typeExpr, ref argList1, ref argListLast1);

                        //--------------------------------------------
                        // one-dimensional
                        //--------------------------------------------
                        if (arrInitExpr.DimSizes.Count == 1)
                        {
                            EXPR arg = arrInitExpr.ArgumentsExpr;
                            argList2 = null;
                            argListLast2 = null;

                            while (arg != null)
                            {
                                EXPR elem;
                                if (arg.Kind == EXPRKIND.LIST)
                                {
                                    elem = arg.AsBIN.Operand1;
                                    arg = arg.AsBIN.Operand2;
                                }
                                else
                                {
                                    elem = arg;
                                    arg = null;
                                }
                                EXPR argExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                    elem,
                                    convertInfo);
                                DebugUtil.Assert(argExpr != null);
                                NewList(argExpr, ref argList2, ref argListLast2);
                            }

                            EXPRARRINIT exprArrInitExpr = NewExpr(
                                null,
                                EXPRKIND.ARRINIT,
                                expressionArraySym) as EXPRARRINIT;
                            exprArrInitExpr.ArgumentsExpr = argList2;
                            exprArrInitExpr.DimSizes.Add(arrInitExpr.DimSizes[0]);

                            argList1 = null;
                            argListLast1 = null;
                            NewList(typeExpr, ref argList1, ref argListLast1);
                            NewList(exprArrInitExpr, ref argList1, ref argListLast1);

                            return BindExpressionMethod(
                                "NewArrayInit",
                                BSYMMGR.EmptyTypeArray,
                                argList1);
                        }
                        //--------------------------------------------
                        // multi-dimensional
                        //--------------------------------------------
                        else if (arrInitExpr.DimSizes.Count > 1)
                        {
                            string arrayName = String.Format("<array>_<{0}>", this.localCount++);
                            string locvarName = CreateParameterExpressionLocalName(arrayName);

                            //----------------------------------------
                            // (LHS) Define the ParameterExpression local variable.
                            //----------------------------------------
                            LOCVARSYM arrayLocSym = DeclareVar(
                                null,
                                locvarName,
                                parameterExpressionAts,
                                false);

                            this.uninitedVarCount++;
                            //paramLocSym.LocSlotInfo.SetJbitDefAssg(this.uninitedVarCount + 1);
                            //int cbit = FlowChecker.GetCbit(Compiler, paramLocSym.TypeSym);
                            //this.uninitedVarCount += cbit;
                            //this.uninitedVarCount -= cbit;
                            arrayLocSym.LocSlotInfo.SetJbitDefAssg(0);

                            EXPR arrayLocExpr =
                                BindToLocal(null, arrayLocSym, BindFlagsEnum.Arguments);
                            convertInfo.AddLocVar(this, arrayLocExpr);

                            //----------------------------------------
                            // (RHS) Call Expression.Parameter method.
                            //----------------------------------------
                            argList2 = null;
                            argListLast2 = null;

                            EXPR newArrTypeExpr = BindSystemType(arrTypeSym.Type);

                            EXPR nameExpr = NewExprConstant(
                                null,
                                Compiler.GetReqPredefType(PREDEFTYPE.STRING, true),
                                new ConstValInit(arrayName));

                            NewList(newArrTypeExpr, ref argList2, ref argListLast2);
                            NewList(nameExpr, ref argList2, ref argListLast2);

                            EXPR callParamExpr
                                = BindExpressionMethod("Parameter", BSYMMGR.EmptyTypeArray, argList2);

                            //----------------------------------------
                            // Assign to the local variable.
                            //----------------------------------------
                            EXPR assgExpr = BindAssignment(null, arrayLocExpr, callParamExpr, false);
                            arrayLocSym.LocSlotInfo.HasInit = true;

                            EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
                            declExpr.LocVarSym = arrayLocSym;
                            declExpr.InitialExpr = assgExpr;

                            convertInfo.ParameterDic.Add(arrayName, arrayLocSym);
                            convertInfo.Builder.Add(declExpr);

                            //----------------------------------------
                            //
                            //----------------------------------------
                            EXPR newArrBndExpr = BindToNewArrayBounds(
                                arrInitExpr.TypeSym as ARRAYSYM,
                                arrInitExpr.DimSizes);

                            argList2 = null;
                            argListLast2 = null;
                            NewList(arrayLocExpr, ref argList2, ref argListLast2);
                            NewList(newArrBndExpr, ref argList2, ref argListLast2);
                            EXPR assgParamArrExpr = BindExpressionMethod(
                                "Assign",
                                BSYMMGR.EmptyTypeArray,
                                argList2);

                            convertInfo.AddExpression(this, assgParamArrExpr);

                            //----------------------------------------
                            //
                            //----------------------------------------
                            EXPR arg = arrInitExpr.ArgumentsExpr;
                            MultiDimensionalCounter counter =
                                new MultiDimensionalCounter(arrInitExpr.DimSizes);

                            while (arg != null && !counter.HasError)
                            {
                                EXPR expr;
                                if (arg.Kind == EXPRKIND.LIST)
                                {
                                    expr = arg.AsBIN.Operand1;
                                    arg = arg.AsBIN.Operand2;
                                }
                                else
                                {
                                    expr = arg;
                                    arg = null;
                                }

                                EXPR valueExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                    expr,
                                    convertInfo);

                                EXPR argList3 = null;
                                EXPR argListLast3 = null;
                                for (int i = 0; i < counter.Size; ++i)
                                {
                                    EXPR constExpr = NewExprConstant(
                                        null,
                                        GetRequiredPredefinedType(PREDEFTYPE.INT),
                                        new ConstValInit(counter[i]));
                                    EXPR indexExpr = BindExpressionMethod(
                                        "Constant",
                                        BSYMMGR.EmptyTypeArray,
                                        constExpr);
                                    NewList(indexExpr, ref argList3, ref argListLast3);
                                }

                                EXPRARRINIT arrIndexExpr = NewExpr(
                                    null,
                                    EXPRKIND.ARRINIT,
                                    expressionArraySym) as EXPRARRINIT;
                                arrIndexExpr.ArgumentsExpr = argList3;
                                arrIndexExpr.DimSizes.Add(counter.Size);

                                argList2 = null;
                                argListLast2 = null;
                                NewList(arrayLocExpr, ref argList2, ref argListLast2);
                                NewList(arrIndexExpr, ref argList2, ref argListLast2);

                                EXPR arrAccessExpr = BindExpressionMethod(
                                    "ArrayAccess",
                                    BSYMMGR.EmptyTypeArray,
                                    argList2);

                                argList1 = null;
                                argListLast1 = null;
                                NewList(arrAccessExpr, ref argList1, ref argListLast1);
                                NewList(valueExpr, ref argList1, ref argListLast1);

                                EXPR assignExpr = BindExpressionMethod(
                                    "Assign",
                                    BSYMMGR.EmptyTypeArray,
                                    argList1);

                                convertInfo.AddExpression(this, assignExpr);
                                counter.Inc();
                            }

                            return arrayLocExpr;
                        }
                        //--------------------------------------------
                        // otherwise
                        //--------------------------------------------
                        else
                        {
                            DebugUtil.Assert(false);
                        }
                        break;
                    }

                //----------------------------------------------------
                // CAST
                //----------------------------------------------------
                case EXPRKIND.CAST:
                    {
                        EXPRCAST castExpr = srcExpr as EXPRCAST;
                        DebugUtil.Assert(
                            castExpr != null &&
                            castExpr.TypeSym != null);
                        SymUtil.EmitParentSym(Compiler.Emitter, castExpr.TypeSym);
                        SymUtil.GetSystemTypeFromSym(castExpr.TypeSym, null, null);

                        EXPR tempExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                                        castExpr.Operand,
                                        convertInfo);
                        EXPR typeExpr = BindSystemType(castExpr.TypeSym.Type);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        NewList(tempExpr1, ref argList1, ref argListLast1);
                        NewList(typeExpr, ref argList1, ref argListLast1);

                        exMethodName = IsCheckOverflowSet(srcExpr.Flags) ? "ConvertChecked" : "Convert";
                        return BindExpressionMethod(exMethodName, BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // TYPEOF
                //----------------------------------------------------
                case EXPRKIND.TYPEOF:
                    {
                        EXPRTYPEOF typeofExpr = srcExpr as EXPRTYPEOF;
                        DebugUtil.Assert(typeofExpr != null);
                        DebugUtil.Assert(typeofExpr.SourceTypeSym != null);

                        TYPESYM srcTypeSym = typeofExpr.SourceTypeSym;
                        SymUtil.EmitParentSym(Compiler.Emitter, srcTypeSym);
                        SymUtil.GetSystemTypeFromSym(srcTypeSym, null, null);

                        return BindSystemType(srcTypeSym.Type);
                    }

                //----------------------------------------------------
                // SIZEOF
                //----------------------------------------------------
                case EXPRKIND.SIZEOF:
                    {
                        // 'sizeof(*)' has already been converted to the value
                        // by BindSizeOf method.
                        DebugUtil.Assert(false);
                        break;
                    }

                //----------------------------------------------------
                // ZEROINIT
                //----------------------------------------------------
                case EXPRKIND.ZEROINIT:
                    {
                        // provisional
                        // EXPRZEROINIT is used for the types
                        // which are neither the fundamental types nor the reference types. 

                        EXPRZEROINIT ziExpr = srcExpr as EXPRZEROINIT;
                        TYPESYM typeSym = ziExpr.TypeSym;
                        SymUtil.EmitParentSym(Compiler.Emitter, typeSym);
                        SymUtil.GetSystemTypeFromSym(typeSym, null, null);
                        EXPR typeExpr = BindSystemType(typeSym.Type);

                        return BindExpressionMethod("New", BSYMMGR.EmptyTypeArray, typeExpr);
                    }

                //----------------------------------------------------
                // 
                //----------------------------------------------------
                case EXPRKIND.USERLOGOP:
                case EXPRKIND.MEMGRP:
                case EXPRKIND.ANONMETH:
                    break;

                //----------------------------------------------------
                // DBLQMARK
                //----------------------------------------------------
                case EXPRKIND.DBLQMARK:
                    {
                        EXPRDBLQMARK dblQmarkExpr = srcExpr as EXPRDBLQMARK;
                        DebugUtil.Assert(dblQmarkExpr != null);

                        EXPR leftExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            dblQmarkExpr.TestExpr,
                            convertInfo);
                        EXPR rightExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            dblQmarkExpr.ElseExpr,
                            convertInfo);

                        EXPR argList = null;
                        EXPR argListLast = null;
                        NewList(leftExpr, ref argList, ref argListLast);
                        NewList(rightExpr, ref argList, ref argListLast);

                        return BindExpressionMethod("Coalesce", BSYMMGR.EmptyTypeArray, argList);
                    }

                //----------------------------------------------------
                // 
                //----------------------------------------------------
                case EXPRKIND.COUNT:
                case EXPRKIND.LIST:
                    break;

                //----------------------------------------------------
                // ASSG
                //----------------------------------------------------
                case EXPRKIND.ASSG:
                    {
                        EXPRBINOP binopExpr = srcExpr as EXPRBINOP;
                        DebugUtil.Assert(binopExpr != null);

                        EXPR leftExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            binopExpr.Operand1,
                            convertInfo);
                        EXPR rightExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            binopExpr.Operand2,
                            convertInfo);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        NewList(leftExpr, ref argList1, ref argListLast1);
                        NewList(rightExpr, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Assign", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // 
                //----------------------------------------------------
                case EXPRKIND.MAKERA:
                case EXPRKIND.VALUERA:
                case EXPRKIND.TYPERA:
                case EXPRKIND.ARGS:
                case EXPRKIND.EQUALS:
                case EXPRKIND.COMPARE:
                case EXPRKIND.TRUE:
                case EXPRKIND.FALSE:
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

                        EXPR argExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            binOpExpr.Operand1,
                            convertInfo);

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

                        EXPR argExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            binOpExpr.Operand1,
                            convertInfo);
                        EXPR argExpr2 = ConvertLambdaExpressionToExpressionTreeCore(
                            binOpExpr.Operand2,
                            convertInfo);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
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

                //----------------------------------------------------
                // ARRLEN
                //----------------------------------------------------
                case EXPRKIND.ARRLEN:
                    {
                        EXPRBINOP arrLenExpr = srcExpr as EXPRBINOP;
                        DebugUtil.Assert(arrLenExpr != null);

                        EXPR arrExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            arrLenExpr.Operand1,
                            convertInfo);
                        return BindExpressionMethod("ArrayLength", BSYMMGR.EmptyTypeArray, arrExpr);
                    }

                //----------------------------------------------------
                // IS
                //----------------------------------------------------
                case EXPRKIND.IS:
                    {
                        EXPRBINOP isExpr = srcExpr as EXPRBINOP;

                        EXPR opExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            isExpr.Operand1,
                            convertInfo);

                        DebugUtil.Assert(isExpr.Operand2.Kind == EXPRKIND.TYPEOF);
                        EXPR typeExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            isExpr.Operand2,
                            convertInfo);

                        EXPR argList = null;
                        EXPR argListLast = null;
                        NewList(opExpr, ref argList, ref argListLast);
                        NewList(typeExpr, ref argList, ref argListLast);

                        return BindExpressionMethod("TypeIs", BSYMMGR.EmptyTypeArray, argList);
                    }

                //----------------------------------------------------
                // AS
                //----------------------------------------------------
                case EXPRKIND.AS:
                    {
                        EXPRBINOP isExpr = srcExpr as EXPRBINOP;

                        EXPR opExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            isExpr.Operand1,
                            convertInfo);

                        DebugUtil.Assert(isExpr.Operand2.Kind == EXPRKIND.TYPEOF);
                        EXPR typeExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            isExpr.Operand2,
                            convertInfo);

                        EXPR argList = null;
                        EXPR argListLast = null;
                        NewList(opExpr, ref argList, ref argListLast);
                        NewList(typeExpr, ref argList, ref argListLast);

                        return BindExpressionMethod("TypeAs", BSYMMGR.EmptyTypeArray, argList);
                    }

                //----------------------------------------------------
                // ARRINDEX
                //----------------------------------------------------
                case EXPRKIND.ARRINDEX:
                    {
                        EXPRBINOP arrIndexExpr = srcExpr as EXPRBINOP;
                        DebugUtil.Assert(arrIndexExpr != null);

                        EXPR arrayExpr = ConvertLambdaExpressionToExpressionTreeCore(
                            arrIndexExpr.Operand1,
                            convertInfo);

                        EXPR arg = arrIndexExpr.Operand2;
                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        EXPR argList2 = null;
                        EXPR argListLast2 = null;
                        int indexCount = 0;

                        while (arg != null)
                        {
                            EXPR idx;
                            if (arg.Kind == EXPRKIND.LIST)
                            {
                                idx = arg.AsBIN.Operand1;
                                arg = arg.AsBIN.Operand2;
                            }
                            else
                            {
                                idx = arg;
                                arg = null;
                            }
                            EXPR indexExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                idx,
                                convertInfo);
                            DebugUtil.Assert(indexExpr != null);
                            NewList(indexExpr, ref argList2, ref argListLast2);
                            ++indexCount;
                        }

                        EXPRARRINIT arrIndexsExpr = NewExpr(
                            null,
                            EXPRKIND.ARRINIT,
                            expressionArraySym) as EXPRARRINIT;
                        arrIndexsExpr.ArgumentsExpr = argList2;
                        arrIndexsExpr.DimSizes.Add(indexCount);
                        NewList(arrIndexsExpr, ref argList1, ref argListLast1);

                        argList1 = null;
                        argListLast1 = null;
                        NewList(arrayExpr, ref argList1, ref argListLast1);
                        NewList(arrIndexsExpr, ref argList1, ref argListLast1);

                        return BindExpressionMethod("ArrayAccess", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // NEWARRAY
                //----------------------------------------------------
                case EXPRKIND.NEWARRAY:
                    {
                        EXPRBINOP newArrExpr = srcExpr as EXPRBINOP;

                        ARRAYSYM arrTypeSym = newArrExpr.TypeSym as ARRAYSYM;
                        DebugUtil.Assert(arrTypeSym != null);

                        List<int> sizeList = new List<int>();
                        EXPR arg = newArrExpr.Operand1;

                        while (arg != null)
                        {
                            EXPR expr;
                            if (arg.Kind == EXPRKIND.LIST)
                            {
                                expr = arg.AsBIN.Operand1;
                                arg = arg.AsBIN.Operand2;
                            }
                            else
                            {
                                expr = arg;
                                arg = null;
                            }
                            DebugUtil.Assert(expr.Kind == EXPRKIND.CONSTANT);
                            CONSTVAL cv = (expr as EXPRCONSTANT).ConstVal;
                            int siz = cv.GetInt();
                            DebugUtil.Assert(siz > 0);
                            sizeList.Add(siz);
                        }

                        return BindToNewArrayBounds(arrTypeSym, sizeList);
                    }

                //----------------------------------------------------
                // ternary operator ? :
                //----------------------------------------------------
                case EXPRKIND.QMARK:
                    {
                        EXPRBINOP qmarkExpr = srcExpr as EXPRBINOP;
                        DebugUtil.Assert(qmarkExpr != null);
                        EXPRBINOP choiceExpr = qmarkExpr.Operand2 as EXPRBINOP;
                        DebugUtil.Assert(choiceExpr != null);

                        EXPR argList1 = null;
                        EXPR argListLast1 = null;
                        EXPR tempExpr1 = null;

                        tempExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            qmarkExpr.Operand1,
                            convertInfo);
                        NewList(tempExpr1, ref argList1, ref argListLast1);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            choiceExpr.Operand1,
                            convertInfo);
                        NewList(tempExpr1, ref argList1, ref argListLast1);
                        tempExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                            choiceExpr.Operand2,
                            convertInfo);
                        NewList(tempExpr1, ref argList1, ref argListLast1);

                        return BindExpressionMethod("Condition", BSYMMGR.EmptyTypeArray, argList1);
                    }

                //----------------------------------------------------
                // SEQUENCE
                //----------------------------------------------------
                case EXPRKIND.SEQUENCE:
                    {
                        EXPRBINOP seqExpr = srcExpr as EXPRBINOP;
                        DebugUtil.Assert(seqExpr != null);

                        //--------------------------------------------
                        // ??
                        //--------------------------------------------
                        if (seqExpr.TreeNode != null &&
                            seqExpr.TreeNode.Operator == OPERATOR.VALORDEF)
                        {
                            EXPRBINOP bopExpr = seqExpr;
                            EXPRBINOP qmarkExpr = null;

                            while (bopExpr != null)
                            {
                                if (bopExpr.Operand1.Kind == EXPRKIND.QMARK)
                                {
                                    qmarkExpr = bopExpr.Operand1 as EXPRBINOP;
                                    break;
                                }
                                bopExpr = bopExpr.Operand2 as EXPRBINOP;
                            }
                            if (qmarkExpr == null)
                            {
                                DebugUtil.Assert(false);
                                return null;
                            }

                            EXPR op1 = null;
                            EXPR op2 = null;

                            if (qmarkExpr.Operand1.Kind == EXPRKIND.PROP)
                            {
                                op1 = (qmarkExpr.Operand1 as EXPRPROP).ObjectExpr;

                                switch (op1.Kind)
                                {
                                    case EXPRKIND.LDTMP:
                                        {
                                            EXPRLDTMP ldtExpr = op1 as EXPRLDTMP;
                                            if (ldtExpr.TmpExpr.Kind != EXPRKIND.STTMP)
                                            {
                                                return null;
                                            }
                                            op1 = ldtExpr.TmpExpr;
                                            goto case EXPRKIND.STTMP;
                                        }

                                    case EXPRKIND.STTMP:
                                        op1 = (op1 as EXPRSTTMP).SourceExpr;
                                        break;

                                    default:
                                        break;
                                }
                            }

                            EXPRBINOP altExpr = qmarkExpr.Operand2 as EXPRBINOP;
                            if (altExpr != null)
                            {
                                op2 = altExpr.Operand2;
                            }

                            if (op1 == null || op2 == null)
                            {
                                return null;
                            }

                            EXPR leftExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                op1,
                                convertInfo);
                            EXPR rightExpr = ConvertLambdaExpressionToExpressionTreeCore(
                                op2,
                                convertInfo);

                            EXPR argList = null;
                            EXPR argListLast = null;
                            NewList(leftExpr, ref argList, ref argListLast);
                            NewList(rightExpr, ref argList, ref argListLast);

                            return BindExpressionMethod("Coalesce", BSYMMGR.EmptyTypeArray, argList);
                        }

                        //--------------------------------------------
                        //
                        //--------------------------------------------
                        break;
                    }

                //----------------------------------------------------
                // 
                //----------------------------------------------------
                case EXPRKIND.SEQREV:
                case EXPRKIND.SAVE:
                case EXPRKIND.SWAP:
                case EXPRKIND.ARGLIST:
                case EXPRKIND.INDIR:
                case EXPRKIND.ADDR:
                case EXPRKIND.LOCALLOC:
                case EXPRKIND.LAMBDAEXPR:   // CS3
                case EXPRKIND.SYSTEMTYPE:   // CS3
                case EXPRKIND.FIELDINFO:    // CS3
                case EXPRKIND.METHODINFO:   // CS3
                case EXPRKIND.CONSTRUCTORINFO:  // CS3
                    break;

                //----------------------------------------------------
                // 
                //----------------------------------------------------
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
        // FUNCBREC.ConvertLocalToExpressionTree
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

            AGGTYPESYM aggTypeSym = objectExpr.TypeSym as AGGTYPESYM;

            EXPR objExpr = MustConvert(
                      objectExpr,
                      Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true),
                      0);
            NewList(objExpr, ref argList, ref argListLast);

            argExpr1 = BindExpressionMethod("Constant", BSYMMGR.EmptyTypeArray, argList);
            argExpr2 = BindFieldInfo(fieldSym, aggTypeSym);

            argList = null;
            argListLast = null;
            NewList(argExpr1, ref argList, ref argListLast);
            NewList(argExpr2, ref argList, ref argListLast);

            return BindExpressionMethod("Field", BSYMMGR.EmptyTypeArray, argList);
        }

        //------------------------------------------------------------
        // FUNCBREC.ConvertConcatToExpressionTree
        //
        /// <summary></summary>
        /// <param name="argExpr1"></param>
        /// <param name="isConverted1"></param>
        /// <param name="isString1"></param>
        /// <param name="argExpr2"></param>
        /// <param name="isConverted2"></param>
        /// <param name="isString2"></param>
        /// <param name="convertInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR ConvertConcatToExpressionTree(
            EXPR argExpr1,
            bool isConverted1,
            bool isString1,
            EXPR argExpr2,
            bool isConverted2,
            bool isString2,
            ExpressionTreeConvertInfo convertInfo)
        {
            EXPR etArgExpr1 = null;
            EXPR etArgExpr2 = null;

            if (!isConverted1)
            {
                etArgExpr1 = ConvertLambdaExpressionToExpressionTreeCore(
                    argExpr1,
                    convertInfo);
            }
            else
            {
                etArgExpr1 = argExpr1;
            }

            if (!isConverted2)
            {
                etArgExpr2 = ConvertLambdaExpressionToExpressionTreeCore(
                    argExpr2,
                    convertInfo);
            }
            else
            {
                etArgExpr2 = argExpr2;
            }

            EXPR argList = null;
            EXPR argListLast = null;

            //--------------------------------------------------------
            // string + string
            //--------------------------------------------------------
            if (isString1 && isString2)
            {
                if (stringConcatInfoExpr1 == null)
                {
                    Type stringType = typeof(string);
                    MethodInfo concatInfo = stringType.GetMethod(
                        "Concat",
                        new Type[] { typeof(string), typeof(string) });
                    DebugUtil.Assert(concatInfo != null);
                    stringConcatInfoExpr1 = BindMethodInfo(concatInfo, stringType);
                }

                NewList(etArgExpr1, ref argList, ref argListLast);
                NewList(etArgExpr2, ref argList, ref argListLast);
                NewList(stringConcatInfoExpr1, ref argList, ref argListLast);

                return BindExpressionMethod("Add", BSYMMGR.EmptyTypeArray, argList);
            }
            //--------------------------------------------------------
            // object + object
            //--------------------------------------------------------
            else
            {
                if (stringConcatInfoExpr2 == null)
                {
                    Type stringType = typeof(string);
                    MethodInfo concatInfo = stringType.GetMethod(
                        "Concat",
                        new Type[] { typeof(object), typeof(object) });
                    DebugUtil.Assert(concatInfo != null);
                    stringConcatInfoExpr2 = BindMethodInfo(concatInfo, stringType);
                }

                EXPR objectTypeExpr = BindSystemType(typeof(object));

                argList = null;
                argListLast = null;

                NewList(etArgExpr1, ref argList, ref argListLast);
                NewList(objectTypeExpr, ref argList, ref argListLast);

                etArgExpr1 = BindExpressionMethod("Convert", BSYMMGR.EmptyTypeArray, argList);

                argList = null;
                argListLast = null;

                NewList(etArgExpr2, ref argList, ref argListLast);
                NewList(objectTypeExpr, ref argList, ref argListLast);

                etArgExpr2 = BindExpressionMethod("Convert", BSYMMGR.EmptyTypeArray, argList);

                argList = null;
                argListLast = null;

                NewList(etArgExpr1, ref argList, ref argListLast);
                NewList(etArgExpr2, ref argList, ref argListLast);
                NewList(stringConcatInfoExpr2, ref argList, ref argListLast);

                return BindExpressionMethod("Add", BSYMMGR.EmptyTypeArray, argList);
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.BindToNewArrayBounds
        //
        /// <summary></summary>
        /// <param name="arrTypeSym"></param>
        /// <param name="sizeList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindToNewArrayBounds(
            ARRAYSYM arrTypeSym,
            List<int> sizeList)
        {
            EXPR argList1 = null;
            EXPR argListLast1 = null;

            DebugUtil.Assert(arrTypeSym != null);
            TYPESYM elementTypeSym = arrTypeSym.ElementTypeSym;
            //Compiler.Emitter.EmitAggregateDef(elementTypeSym.GetAggregate());
            SymUtil.EmitParentSym(Compiler.Emitter, arrTypeSym);
            SymUtil.GetSystemTypeFromSym(elementTypeSym, null, null);

            EXPR typeExpr = BindSystemType(elementTypeSym.Type);
            NewList(typeExpr, ref argList1, ref argListLast1);

            EXPR argList2 = null;
            EXPR argListLast2 = null;

            for (int i = 0; i < sizeList.Count; ++i)
            {
                EXPR sizeConstExpr = NewExprConstant(
                    null,
                    GetRequiredPredefinedType(PREDEFTYPE.INT),
                    new ConstValInit(sizeList[i]));
                EXPR sizeExpr = BindExpressionMethod(
                    "Constant",
                    BSYMMGR.EmptyTypeArray,
                    sizeConstExpr);
                NewList(sizeExpr, ref argList2, ref argListLast2);
            }

            EXPRARRINIT newArrSizesExpr = NewExpr(
                null,
                EXPRKIND.ARRINIT,
                expressionArraySym) as EXPRARRINIT;
            newArrSizesExpr.ArgumentsExpr = argList2;
            newArrSizesExpr.DimSizes.Add(sizeList.Count);
            NewList(newArrSizesExpr, ref argList1, ref argListLast1);

            return BindExpressionMethod(
                "NewArrayBounds",
                BSYMMGR.EmptyTypeArray,
                argList1);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindMultiDimensionalArrayInit
        //
        /// <summary></summary>
        /// <param name="sizeList"></param>
        /// <param name="arrayExpr"></param>
        /// <param name="argumentsExpr"></param>
        /// <param name="convertInfo"></param>
        //------------------------------------------------------------
        internal void BindMultiDimensionalArrayInit(
            List<int> sizeList,
            EXPR arrayExpr,
            EXPR argumentsExpr,
            ExpressionTreeConvertInfo convertInfo)
        {
            EXPR arg = argumentsExpr;
            MultiDimensionalCounter counter = new MultiDimensionalCounter(sizeList);

            EXPR stmtList = null;
            EXPR stmtListLast = null;

            while (arg != null && !counter.HasError)
            {
                EXPR expr;
                if (arg.Kind == EXPRKIND.LIST)
                {
                    expr = arg.AsBIN.Operand1;
                    arg = arg.AsBIN.Operand2;
                }
                else
                {
                    expr = arg;
                    arg = null;
                }

                EXPR argExpr = ConvertLambdaExpressionToExpressionTreeCore(
                    arg,
                    convertInfo);

                EXPR argList3 = null;
                EXPR argListLast3 = null;
                for (int i = 0; i < counter.Size; ++i)
                {
                    EXPR indexExpr = NewExprConstant(
                        null,
                        GetRequiredPredefinedType(PREDEFTYPE.INT),
                        new ConstValInit(counter[i]));
                    NewList(indexExpr, ref argList3, ref argListLast3);
                }

                EXPRARRINIT arrIndexsExpr = NewExpr(
                    null,
                    EXPRKIND.ARRINIT,
                    expressionArraySym) as EXPRARRINIT;
                arrIndexsExpr.ArgumentsExpr = argList3;
                arrIndexsExpr.DimSizes.Add(counter.Size);

                EXPR argList2 = null;
                EXPR argListLast2 = null;
                NewList(arrayExpr, ref argList2, ref argListLast2);
                NewList(arrIndexsExpr, ref argList2, ref argListLast2);

                EXPR arrAccessExpr = BindExpressionMethod(
                    "ArrayAccess",
                    BSYMMGR.EmptyTypeArray,
                    argList2);

                EXPR argList1 = null;
                EXPR argListLast1 = null;
                NewList(arrAccessExpr, ref argList1, ref argListLast1);
                NewList(argumentsExpr, ref argList1, ref argListLast1);

                EXPR assignExpr = BindExpressionMethod(
                    "Assign",
                    BSYMMGR.EmptyTypeArray,
                    argList1);

                NewList(assignExpr, ref stmtList, ref stmtListLast);
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.BindExpressionNoOp
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindExpressionNoOp()
        {
            EXPR argList = null;
            EXPR argListLast = null;

            NewList(
                NewExprConstant(
                    null,
                    Compiler.GetReqPredefType(PREDEFTYPE.INT, true),
                    new ConstValInit((int)0)),
                ref argList,
                ref argListLast);
            NewList(
                BindSystemType(typeof(int)),
                ref argList,
                ref argListLast);

            return BindExpressionMethod(
                "Constant",
                BSYMMGR.EmptyTypeArray,
                argList);
        }
    }
}
