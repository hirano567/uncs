//============================================================================
// RuntimeBind.cs
//
// 2016/06/25 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace Uncs
{
    //======================================================================
    // class BSYMMGR
    //======================================================================
    internal partial class BSYMMGR
    {
        //------------------------------------------------------------
        // BSYMMGR.CreateDynamicSym
        //
        /// <summary>(CS4) Create a DYNAMICSYM instance
        /// and set to this.dynamicSym.</summary>
        //------------------------------------------------------------
        internal void CreateDynamicSym()
        {
            this.dynamicSym = CreateGlobalSym(
                SYMKIND.DYNAMICSYM,
                null,
                GetReqPredefAgg(PREDEFTYPE.OBJECT)) as DYNAMICSYM;
            this.dynamicSym.Access = ACCESS.PUBLIC;
            this.dynamicSym.AggState = AggStateEnum.PreparedMembers;
            SymUtil.GetSystemTypeFromSym(this.dynamicSym, null, null);
            this.dynamicSym.AllTypeArguments = BSYMMGR.EmptyTypeArray;
            this.dynamicSym.TypeArguments = BSYMMGR.EmptyTypeArray;
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC Fields and Properties (1)
        //------------------------------------------------------------

        /// <summary>
        /// AGGSYM of System.Runtime.CompilerServices.CallSite Class.
        /// </summary>
        private AGGSYM callSiteClassSym = null;

        /// <summary>
        /// AGGTYPESYM of System.Runtime.CompilerServices.CallSite Class.
        /// </summary>
        private AGGTYPESYM callSiteTypeSym = null;

        /// <summary>
        /// AGGSYM of System.Runtime.CompilerServices.CallSite<T> Class.
        /// </summary>
        private AGGSYM callSiteG1ClassSym = null;

        /// <summary>
        /// AGGTYPESYM of System.Runtime.CompilerServices.CallSite<T> Class.
        /// </summary>
        private AGGTYPESYM callSiteG1TypeSym = null;

        private METHSYM callSiteG1CreateMethodSym = null;
        private MEMBVARSYM callSiteG1TargetFieldSym = null;

        /// <summary>
        /// AGGSYM of System.Runtime.CompilerServices.CallSiteBinder Class.
        /// </summary>
        private AGGSYM callSiteBinderClassSym = null;

        /// <summary>
        /// AGGTYPESYM of System.Runtime.CompilerServices.CallSiteBinder Class.
        /// </summary>
        private AGGTYPESYM callSiteBinderTypeSym = null;
#if false
        private NSSYM microsoftNsSym = null;
        private NSSYM csharpNsSym = null;
        private NSSYM runtimeBinderNsSym = null;
#endif
        /// <summary>
        /// AGGSYM of Microsoft.CSharp.RuntimeBinder.Binder
        /// </summary>
        private AGGSYM binderClassSym = null;

        /// <summary>
        /// AGGTYPESYM of Microsoft.CSharp.RuntimeBinder.Binder
        /// </summary>
        private AGGTYPESYM binderTypeSym = null;

        /// <summary>
        /// METHSYM of Microsoft.CSharp.RuntimeBinder.Binder.Convert
        /// </summary>
        private METHSYM binderConvertMethodSym = null;

        /// <summary>
        /// METHSYM of Microsoft.CSharp.RuntimeBinder.Binder.SetMember
        /// </summary>
        private METHSYM binderSetMemberMethodSym = null;

        /// <summary>
        /// METHSYM of Microsoft.CSharp.RuntimeBinder.Binder.GetMember
        /// </summary>
        private METHSYM binderGetMemberMethodSym = null;

        /// <summary>
        /// METHSYM of Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember
        /// </summary>
        private METHSYM binderInvokeMemberMethodSym = null;

        /// <summary>
        /// METHSYM of Microsoft.CSharp.RuntimeBinder.Binder.UnaryOperation
        /// </summary>
        private METHSYM binderUnaryOperationMethodSym = null;

        /// <summary>
        /// METHSYM of Microsoft.CSharp.RuntimeBinder.Binder.BinaryOperation
        /// </summary>
        private METHSYM binderBinaryOperationMethodSym = null;

        /// <summary>
        /// AGGSYM of Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo
        /// </summary>
        private AGGSYM csharpArgumentInfoClassSym = null;

        /// <summary>
        /// AGGTYPESYM of Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo
        /// </summary>
        private AGGTYPESYM csharpArgumentInfoTypeSym = null;

        /// <summary>
        /// ARRAYSYM of Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo[]
        /// </summary>
        private ARRAYSYM csharpArgumentInfoArraySym = null;

        /// <summary>
        /// METHSYM of Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create
        /// </summary>
        private METHSYM csharpArgumentInfoCreateMethodSym = null;

        /// <summary>
        /// AGGSYM of Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags
        /// </summary>
        private AGGSYM csharpArgumentInfoFlagsEnumSym = null;

        /// <summary>
        /// AGGTYPESYM of Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags
        /// </summary>
        private AGGTYPESYM csharpArgumentInfoFlagsTypeSym = null;

        /// <summary>
        /// AGGSYM of Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags
        /// </summary>
        private AGGSYM csharpBinderFlagsEnumSym = null;

        /// <summary>
        /// AGGTYPESYM of Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags
        /// </summary>
        private AGGTYPESYM csharpBinderFlagsTypeSym = null;

        /// <summary>
        /// AGGSYM of System.Linq.Expressions.ExpressionType
        /// </summary>
        private AGGSYM expressionTypeEnumSym = null;

        /// <summary>
        /// AGGTYPESYM of System.Linq.Expressions.ExpressionType
        /// </summary>
        private AGGTYPESYM expressionTypeTypeSym = null;

        //------------------------------------------------------------
        // FUNCBREC.SetRuntimeBindFields
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool SetRuntimeBindFields()
        {
            if (!SetFncBrecExtFields())
            {
                return false;
            }

            //--------------------------------------------------------
            // System.Runtime.CompilerServices.CallSite
            //--------------------------------------------------------
            if (callSiteTypeSym == null)
            {
                callSiteTypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.CALLSITE,
                    true);
                callSiteClassSym = callSiteTypeSym.GetAggregate();
            }
            if (callSiteTypeSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("CallSite"));
                return false;
            }

            //--------------------------------------------------------
            // System.Runtime.CompilerServices.CallSite<T>
            //--------------------------------------------------------
            if (callSiteG1TargetFieldSym == null)
            {
                callSiteG1TypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.G_CALLSITE,
                    true);
                callSiteG1ClassSym = callSiteG1TypeSym.GetAggregate();

                this.callSiteG1CreateMethodSym = Compiler.MainSymbolManager.LookupAggMember(
                    "Create",
                    this.callSiteG1ClassSym,
                    SYMBMASK.METHSYM) as METHSYM;

                this.callSiteG1TargetFieldSym = Compiler.MainSymbolManager.LookupAggMember(
                    "Target",
                    this.callSiteG1ClassSym,
                    SYMBMASK.MEMBVARSYM) as MEMBVARSYM;
            }
            if (callSiteG1TargetFieldSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("CallSite`1"));
                return false;
            }

            //--------------------------------------------------------
            // System.Runtime.CompilerServices.CallSiteBinder
            //--------------------------------------------------------
            if (callSiteBinderTypeSym == null)
            {
                callSiteBinderTypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.CALLSITEBINDER,
                    true);
                callSiteBinderClassSym = callSiteBinderTypeSym.GetAggregate();
            }
            if (callSiteBinderTypeSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("CallSiteBinder"));
                return false;
            }

            //--------------------------------------------------------
            // Microsoft.CSharp.RuntimeBinder.Binder
            //--------------------------------------------------------
            if (binderInvokeMemberMethodSym == null)
            {
                binderTypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.RUNTIMEBINDER_BINDER,
                    true);
                binderClassSym = binderTypeSym.GetAggregate();

                this.binderConvertMethodSym = Compiler.MainSymbolManager.LookupAggMember(
                    "Convert",
                    this.binderClassSym,
                    SYMBMASK.METHSYM) as METHSYM;

                this.binderSetMemberMethodSym= Compiler.MainSymbolManager.LookupAggMember(
                    "SetMember",
                    this.binderClassSym,
                    SYMBMASK.METHSYM) as METHSYM;

                this.binderGetMemberMethodSym = Compiler.MainSymbolManager.LookupAggMember(
                    "GetMember",
                    this.binderClassSym,
                    SYMBMASK.METHSYM) as METHSYM;

                this.binderInvokeMemberMethodSym = Compiler.MainSymbolManager.LookupAggMember(
                    "InvokeMember",
                    this.binderClassSym,
                    SYMBMASK.METHSYM) as METHSYM;

                this.binderUnaryOperationMethodSym = Compiler.MainSymbolManager.LookupAggMember(
                    "UnaryOperation",
                    this.binderClassSym,
                    SYMBMASK.METHSYM) as METHSYM;

                this.binderBinaryOperationMethodSym = Compiler.MainSymbolManager.LookupAggMember(
                    "BinaryOperation",
                    this.binderClassSym,
                    SYMBMASK.METHSYM) as METHSYM;
            }
            if (binderBinaryOperationMethodSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("RuntimeBinder.Binder"));
                return false;
            }

            //--------------------------------------------------------
            // Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo
            //--------------------------------------------------------
            if (csharpArgumentInfoCreateMethodSym == null)
            {
                csharpArgumentInfoTypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.CSHARPARGUMENTINFO,
                    true);
                csharpArgumentInfoClassSym = csharpArgumentInfoTypeSym.GetAggregate();
                Compiler.EnsureState(this.csharpArgumentInfoClassSym, AggStateEnum.Prepared);

                csharpArgumentInfoArraySym = Compiler.MainSymbolManager.GetArray(
                    csharpArgumentInfoTypeSym,
                    1,
                    csharpArgumentInfoTypeSym.Type.MakeArrayType());

                this.csharpArgumentInfoCreateMethodSym = Compiler.MainSymbolManager.LookupAggMember(
                    "Create",
                    this.csharpArgumentInfoClassSym,
                    SYMBMASK.METHSYM) as METHSYM;
            }
            if (csharpArgumentInfoCreateMethodSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("CSharpArgumentInfo"));
                return false;
            }

            //--------------------------------------------------------
            // Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags
            //--------------------------------------------------------
            if (csharpArgumentInfoFlagsTypeSym == null)
            {
                csharpArgumentInfoFlagsTypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.CSHARPARGUMENTINFOFLAGS,
                    true);
                csharpArgumentInfoFlagsEnumSym = csharpArgumentInfoFlagsTypeSym.GetAggregate();
            }
            if (csharpArgumentInfoFlagsTypeSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("CSharpArgumentInfoFlags"));
                return false;
            }

            //--------------------------------------------------------
            // Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags
            //--------------------------------------------------------
            if (csharpBinderFlagsTypeSym == null)
            {
                csharpBinderFlagsTypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.CSHARPBINDERFLAGS,
                    true);
                csharpBinderFlagsEnumSym = csharpBinderFlagsTypeSym.GetAggregate();
            }
            if (csharpBinderFlagsTypeSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("CSharpBinderFlags"));
                return false;
            }

            //--------------------------------------------------------
            // System.Linq.Expressions.ExpressionType
            //--------------------------------------------------------
            if (expressionTypeTypeSym == null)
            {
                expressionTypeTypeSym = Compiler.GetOptPredefType(
                    PREDEFTYPE.LINQ_EXPRESSIONTYPE,
                    true);
                expressionTypeEnumSym = expressionTypeTypeSym.GetAggregate();
            }
            if (expressionTypeEnumSym == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg("ExpressionType"));
                return false;
            }

            return true;
        }

        //------------------------------------------------------------
        // FUNCBREC Fields and Properties (2)
        //------------------------------------------------------------
        private int dynamicObjectCount = 0;

        internal int DynamicObjectCount
        {
            get { return dynamicObjectCount; }
        }

        /// <summary>
        /// (CS4) Stores the System.Runtime.CompilerServices.CallSite instants
        /// for runtime-binding.
        /// </summary>
        internal AGGSYM runtimeBindAnonAggSym = null;

        internal AGGTYPESYM runtimeBindAnonTypeSym = null;

        /// <summary>
        /// Base EXPRSTMT in process.
        /// </summary>
        internal EXPRSTMT currentStatementExpr = null;

        /// <summary>
        /// (CS4)
        /// </summary>
        private int runtimeAnonFieldCount = 0;

        //------------------------------------------------------------
        // FUNCBREC.IncrementDynamicObjectCount
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void IncrementDynamicObjectCount()
        {
            ++dynamicObjectCount;
        }

        //------------------------------------------------------------
        // FUNCBREC.ResetDynamicObjectCount
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ResetDynamicObjectCount()
        {
            dynamicObjectCount = 0;
        }

        //------------------------------------------------------------
        // FUNCBREC.FoundRuntimeBindedObject
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal bool FoundRuntimeBindedObject()
        {
            return (dynamicObjectCount > 0);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindAnonAggName
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateRuntimeBindAnonAggName(
            int id,
            string name)
        {
            return String.Format("<runtimebind>_class_<{0}><{1}>", id, name);
        }

        //------------------------------------------------------------
        // FUNCBREC.AssignRuntimeAnonFieldID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int AssignRuntimeAnonFieldID()
        {
            return this.runtimeAnonFieldCount++;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindAnonFieldName
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateRuntimeBindAnonFieldName(int id)
        {
            return String.Format("<runtimebind>_field_<{0}>", id);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindAnonymousClass
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal AGGTYPESYM CreateRuntimeBindAnonymousClass()
        {
            if (this.runtimeBindAnonAggSym != null &&
                this.runtimeBindAnonTypeSym != null)
            {
                return this.runtimeBindAnonTypeSym;
            }

            DECLSYM outerDeclSym = this.methodSym.ContainingAggDeclSym.ParentDeclSym;
            BAGSYM outerBagSym = outerDeclSym.BagSym;
            AGGDECLSYM outerAggDeclSym = outerDeclSym as AGGDECLSYM;

            AGGSYM outerAggSym = null;
            TypeArray outerTypeVariables = null;

            if (outerAggDeclSym != null)
            {
                outerAggSym = outerAggDeclSym.AggSym;
            }
            if (outerAggSym != null)
            {
                outerTypeVariables = outerAggSym.AllTypeVariables;
            }
            else
            {
                outerTypeVariables = BSYMMGR.EmptyTypeArray;
            }
            int outerTvCount = outerTypeVariables.Count;

            string name = CreateRuntimeBindAnonAggName(
                this.methodInfo.MethodSym.SymID,
                this.methodInfo.MethodSym.Name);

            AGGSYM aggSym = Compiler.MainSymbolManager.CreateAgg(name, outerDeclSym);
            AGGDECLSYM aggDeclSym = Compiler.MainSymbolManager.CreateAggDecl(aggSym, outerDeclSym);

            aggSym.IsArityInName = false;
            aggSym.HasParseTree = false;
            aggDeclSym.ParseTreeNode = null;
            aggSym.AggKind = AggKindEnum.Class;
            aggSym.IsSealed = true;
            aggSym.IsAbstract = true;
            aggSym.Access = ACCESS.INTERNAL;
            aggSym.IsFabricated = true;
            aggSym.Interfaces = BSYMMGR.EmptyTypeArray;
            aggSym.AllTypeVariables = outerTypeVariables;
            aggSym.TypeVariables = BSYMMGR.EmptyTypeArray;

            aggSym.AggState = AggStateEnum.Declared;

            AGGTYPESYM baseAggTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true);
            AGGSYM baseAggSym = baseAggTypeSym.GetAggregate();
            Compiler.SetBaseType(aggSym, baseAggTypeSym);

            this.runtimeBindAnonAggSym = aggSym;
            this.runtimeBindAnonTypeSym = this.runtimeBindAnonAggSym.GetThisType();

            return this.runtimeBindAnonTypeSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindAnonField
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBVARSYM CreateRuntimeBindAnonField(
            string name,
            TYPESYM typeSym)
        {
            CreateRuntimeBindAnonymousClass();

            string fieldName = CreateRuntimeBindAnonFieldName(this.runtimeAnonFieldCount++);

            MEMBVARSYM fieldSym = CreateFieldSym(
                fieldName,
                runtimeBindAnonAggSym.FirstDeclSym,
                ACCESS.INTERNAL,
                typeSym);

            DebugUtil.Assert(fieldSym != null);
            fieldSym.IsStatic = true;

            AGGTYPESYM parentAts = fieldSym.ParentAggSym.GetThisType();

            EXPR fieldExpr = BindToField(
                null,
                null,//parentExpr,
                new FieldWithType(fieldSym, parentAts),
                BindFlagsEnum.MemberSet); // bindFlags を確認すること。

            bool isUserDef = false;
            NubInfo nin = null;
            EXPR conditionExpr = BindStdBinOp(
                null,
                EXPRKIND.EQ,
                fieldExpr, BindNull(null),
                ref isUserDef,
                ref nin);

            return fieldSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.EmitRuntimeBindAnonymousClass
        //
        /// <summary></summary>
        /// <param name="funcBRec"></param>
        //------------------------------------------------------------
        internal void EmitRuntimeBindAnonymousClass()
        {
            if (this.runtimeBindAnonAggSym == null)
            {
                return;
            }
            this.runtimeBindAnonAggSym.AggState = AggStateEnum.PreparedMembers;

            Compiler.ClsDeclRec.EmitTypedefsAggregate(this.runtimeBindAnonAggSym);
            Compiler.ClsDeclRec.EmitBasesAggregate(this.runtimeBindAnonAggSym);
            Compiler.ClsDeclRec.EmitMemberdefsAggregate(this.runtimeBindAnonAggSym);

            this.runtimeBindAnonTypeSym = this.runtimeBindAnonAggSym.GetThisType();
        }

        //------------------------------------------------------------
        // FUNCBREC.IsRuntimeBindingExpr
        //
        /// <summary></summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsRuntimeBindingExpr(EXPR expr)
        {
            return (
                expr.Kind == EXPRKIND.RUNTIMEBINDEDBINOP ||
                expr.Kind == EXPRKIND.RUNTIMEBINDEDINVOCATION ||
                expr.Kind == EXPRKIND.RUNTIMEBINDEDMEMBER ||
                expr.Kind == EXPRKIND.RUNTIMEBINDEDUNAOP
                );
        }

        //------------------------------------------------------------
        // FUNCBREC.BindRuntimeBindedMember
        //
        /// <summary>
        /// (CS4)
        /// </summary>
        /// <param name="dotNode"></param>
        /// <param name="parentExpr"></param>
        /// <param name="memberName"></param>
        /// <param name="contextSym"></param>
        /// <param name="typeArguments"></param>
        /// <param name="argumentExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindRuntimeBindedMember(
            BINOPNODE dotNode,
            EXPR objectExpr,
            string memberName,
            AGGTYPESYM contextSym,
            TypeArray typeArguments,
            EXPR argumentExpr)
        {
            if (String.IsNullOrEmpty(memberName))
            {
                goto ERROR_HANDLING;
            }

            EXPRRUNTIMEBINDEDMEMBER expr = NewExprCore(
                dotNode,
                EXPRKIND.RUNTIMEBINDEDMEMBER,
                GetRequiredPredefinedType(PREDEFTYPE.OBJECT),
                0,
                EXPRKIND.RUNTIMEBINDEDMEMBER) as EXPRRUNTIMEBINDEDMEMBER;
            DebugUtil.Assert(expr != null);

            expr.ObjectExpr = objectExpr;
            expr.MemberName = memberName;
            expr.TypeArguments = typeArguments;
            expr.ArgumentsExpr = argumentExpr;
            expr.ContextSym = contextSym;

            this.IncrementDynamicObjectCount();
            return expr;

        ERROR_HANDLING:
            return NewError(dotNode, null);
        }

        //------------------------------------------------------------
        // FUNCBREC.ConvertToRuntimeBindedStatement
        //
        /// <summary></summary>
        /// <param name="stmtExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void ConvertToRuntimeBindedStatement(
            EXPRSTMT stmtExpr,
            StatementListBuilder builder)
        {
            if (!SetRuntimeBindFields())
            {
                builder.Add(MakeStmt(null, NewError(stmtExpr.TreeNode, null), 0));
                return;
            }

            ConvertToRuntimeBindedExpr(stmtExpr, builder);
            return;
        }

        //------------------------------------------------------------
        // FUNCBREC.ConvertToRuntimeBindedExpr
        //
        /// <summary></summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR ConvertToRuntimeBindedExpr(
            EXPR expr,
            StatementListBuilder stmtBuilder)
        {
            if (expr == null)
            {
                return null;
            }

            ExpressionType expressionType = ExpressionType.Add;
            EXPR operandExpr1 = null;
            EXPR operandExpr2 = null;

            switch (expr.Kind)
            {
                //----------------------------------------------------
                // BLOCK (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.BLOCK:
                    {
                        EXPRBLOCK blockExpr = expr as EXPRBLOCK;
                        EXPRSTMT stmtExpr = blockExpr.StatementsExpr;

                        StatementListBuilder builder = new StatementListBuilder();
                        while (stmtExpr != null)
                        {
                            ConvertToRuntimeBindedExpr(stmtExpr, builder);
                        }
                        blockExpr.StatementsExpr = builder.GetList();
                        stmtBuilder.Add(blockExpr);
                        return null;
                    }

                //----------------------------------------------------
                // STMTAS (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.STMTAS:
                    {
                        EXPRSTMTAS stmtasExpr = expr as EXPRSTMTAS;
                        DebugUtil.Assert(stmtasExpr != null);

                        ConvertToRuntimeBindedExpr(
                        stmtasExpr.Expr,
                        stmtBuilder);
                    }
                    return null;

                //----------------------------------------------------
                // RETURN (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.RETURN:
                    {
                        EXPRRETURN retExpr = expr as EXPRRETURN;
                        DebugUtil.Assert(retExpr != null);

                        retExpr.ObjectExpr = ConvertToRuntimeBindedExpr(
                            retExpr.ObjectExpr,
                            stmtBuilder);
                    }
                    return null;

                //----------------------------------------------------
                // DECL (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.DECL:
                    {
                        EXPRDECL declExpr = expr as EXPRDECL;
                        if (declExpr.InitialExpr != null)
                        {
                            declExpr.InitialExpr = ConvertToRuntimeBindedExpr(
                                declExpr.InitialExpr,
                                stmtBuilder);
                        }
                        stmtBuilder.Add(declExpr);
                    }
                    return null;

                //----------------------------------------------------
                // LABEL (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.LABEL:
                    {
                        stmtBuilder.Add(expr as EXPRLABEL);
                    }
                    return null;

                //----------------------------------------------------
                // GOTO (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.GOTO:
                    {
                        stmtBuilder.Add(expr as EXPRGOTO);
                    }
                    return null;

                //----------------------------------------------------
                // GOTOIF (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.GOTOIF:
                    {
                        EXPRGOTOIF gotoifExpr = expr as EXPRGOTOIF;
                        DebugUtil.Assert(gotoifExpr != null);

                        EXPR condExpr= ConvertToRuntimeBindedExpr(
                            gotoifExpr.ConditionExpr,
                            stmtBuilder);

                        gotoifExpr.ConditionExpr = ConvertToRuntimeBindedExpr(
                            condExpr,
                            stmtBuilder);

                        stmtBuilder.Add(gotoifExpr);
                    }
                    return null;

                //----------------------------------------------------
                // SWITCH (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.SWITCH:
                    {
                        DebugUtil.Assert(false);
                    }
                    return null;

                //----------------------------------------------------
                // SWITCHLABEL (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.SWITCHLABEL:
                    {
                        stmtBuilder.Add(expr as EXPRSWITCHLABEL);
                    }
                    return null;

                //----------------------------------------------------
                // TRY (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.TRY:
                    {
                        DebugUtil.Assert(false);
                    }
                    return null;

                //----------------------------------------------------
                // HANDLER (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.HANDLER:
                    {
                        DebugUtil.Assert(false);
                    }
                    return null;

                //----------------------------------------------------
                // THROW (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.THROW:
                    {
                        DebugUtil.Assert(false);
                    }
                    return null;

                //----------------------------------------------------
                // NOOP (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.NOOP:
                    {
                        stmtBuilder.Add(expr as EXPRNOOP);
                    }
                    return null;

                //----------------------------------------------------
                // DEBUGNOOP (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.DEBUGNOOP:
                    {
                        stmtBuilder.Add(expr as EXPRDEBUGNOOP);
                    }
                    return null;

                //----------------------------------------------------
                // DELIM (: EXPRSTMT)
                //----------------------------------------------------
                case EXPRKIND.DELIM:
                    {
                        stmtBuilder.Add(expr as EXPRDELIM);
                    }
                    return null;

                //----------------------------------------------------
                // BINOP
                //----------------------------------------------------
                case EXPRKIND.BINOP:
                    break;

                //----------------------------------------------------
                // BINOP
                //----------------------------------------------------
                case EXPRKIND.CALL:
                case EXPRKIND.RUNTIMEBINDEDINVOCATION:
                    {
                        EXPRCALL callExpr = expr as EXPRCALL;
                        DebugUtil.Assert(callExpr != null);

                        return CreateRuntimeBindInvokeMember(callExpr, stmtBuilder);
                    }
                    break;

                //----------------------------------------------------
                //
                //----------------------------------------------------
                case EXPRKIND.EVENT:
                case EXPRKIND.FIELD:
                case EXPRKIND.LOCAL:
                case EXPRKIND.CONSTANT:
                case EXPRKIND.CLASS:
                case EXPRKIND.NSPACE:
                case EXPRKIND.ERROR:
                case EXPRKIND.FUNCPTR:
                case EXPRKIND.PROP:
                case EXPRKIND.MULTI:
                case EXPRKIND.MULTIGET:
                case EXPRKIND.STTMP:
                case EXPRKIND.LDTMP:
                case EXPRKIND.FREETMP:
                case EXPRKIND.WRAP:
                case EXPRKIND.CONCAT:
                case EXPRKIND.ARRINIT:
                case EXPRKIND.CAST:
                case EXPRKIND.TYPEOF:
                case EXPRKIND.SIZEOF:
                case EXPRKIND.ZEROINIT:
                case EXPRKIND.USERLOGOP:
                case EXPRKIND.MEMGRP:
                case EXPRKIND.ANONMETH:
                case EXPRKIND.DBLQMARK:

                case EXPRKIND.COUNT:

                case EXPRKIND.LIST:
                    break;

                //----------------------------------------------------
                // ASSG
                //----------------------------------------------------
                case EXPRKIND.ASSG:
                    {
                        EXPRBINOP assgExpr = expr as EXPRBINOP;
                        EXPR leftExpr = assgExpr.Operand1;
                        EXPR rightExpr = null;

                        if (leftExpr.Kind == EXPRKIND.RUNTIMEBINDEDMEMBER)
                        {
                            if (this.DynamicObjectCount == 1)
                            {
                                rightExpr = assgExpr.Operand2;
                            }
                            else
                            {
                                rightExpr =
                                    ConvertToRuntimeBindedExpr(assgExpr.Operand2, stmtBuilder);
                            }

                            CreateRuntimeBindSetMemberExpr(
                                leftExpr as EXPRRUNTIMEBINDEDMEMBER,
                                rightExpr,
                                stmtBuilder);
                            return null;
                        }

                        rightExpr =
                            ConvertToRuntimeBindedExpr(assgExpr.Operand2, stmtBuilder);

                        if (leftExpr.TypeSym != null && leftExpr.TypeSym.Kind != SYMKIND.DYNAMICSYM)
                        {
                            rightExpr = CreateRuntimeBindConvertExpr(
                                leftExpr.TypeSym,
                                this.methodSym.ParentAggSym.GetThisType(),
                                rightExpr,
                                stmtBuilder);
                        }
                        assgExpr.Operand2 = rightExpr;
                        return expr;
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

                case EXPRKIND.LOGNOT:

                case EXPRKIND.EQ:
                case EXPRKIND.NE:
                case EXPRKIND.LT:
                case EXPRKIND.LE:
                case EXPRKIND.GT:
                case EXPRKIND.GE:
                    break;

                case EXPRKIND.ADD:
                    {
                        EXPRBINOP binopExpr = expr as EXPRBINOP;

                        operandExpr1 = ConvertToRuntimeBindedExpr(
                            binopExpr.Operand1,
                            stmtBuilder);
                        operandExpr2 = ConvertToRuntimeBindedExpr(
                            binopExpr.Operand2,
                            stmtBuilder);

                        return CreateRuntimeBindBinaryOperation(
                            expressionType,
                            operandExpr1,
                            operandExpr2,
                            stmtBuilder);
                        // expressionType has the initial value ExpressionType.Add.
                    }

                case EXPRKIND.SUB:
                case EXPRKIND.MUL:
                case EXPRKIND.DIV:
                case EXPRKIND.MOD:
                case EXPRKIND.NEG:
                case EXPRKIND.UPLUS:

                case EXPRKIND.BITAND:
                case EXPRKIND.BITOR:
                case EXPRKIND.BITXOR:
                case EXPRKIND.BITNOT:

                case EXPRKIND.LSHIFT:
                case EXPRKIND.RSHIFT:
                case EXPRKIND.ARRLEN:

                case EXPRKIND.LOGAND:
                case EXPRKIND.LOGOR:

                case EXPRKIND.IS:
                case EXPRKIND.AS:
                case EXPRKIND.ARRINDEX:
                case EXPRKIND.NEWARRAY:
                case EXPRKIND.QMARK:
                case EXPRKIND.SEQUENCE:
                case EXPRKIND.SEQREV:
                case EXPRKIND.SAVE:
                case EXPRKIND.SWAP:

                case EXPRKIND.ARGLIST:

                case EXPRKIND.INDIR:
                case EXPRKIND.ADDR:
                case EXPRKIND.LOCALLOC:

                // CS3
                case EXPRKIND.LAMBDAEXPR:
                case EXPRKIND.SYSTEMTYPE:
                case EXPRKIND.FIELDINFO:
                case EXPRKIND.METHODINFO:
                case EXPRKIND.CONSTRUCTORINFO:
                    break;

                //----------------------------------------------------
                // RUNTIMEBINDEDMEMBER  (CS4)
                //----------------------------------------------------
                case EXPRKIND.RUNTIMEBINDEDMEMBER:
                    return CreateRuntimeBindGetMemberExpr(
                        expr as EXPRRUNTIMEBINDEDMEMBER,
                        stmtBuilder);

                //----------------------------------------------------
                // RUNTIMEBINDEDUNAOP  (CS4)
                //----------------------------------------------------
                case EXPRKIND.RUNTIMEBINDEDUNAOP:
                    {
                        EXPRRUNTIMEBINDEDUNAOP rtUnaopExpr = expr as EXPRRUNTIMEBINDEDUNAOP;

                        EXPR operandExpr = ConvertToRuntimeBindedExpr(
                            rtUnaopExpr.Operand1,
                            stmtBuilder);

                        return CreateRuntimeBindUnaryOperation(
                            rtUnaopExpr.ExpressionType,
                            operandExpr,
                            stmtBuilder);
                    }
                    break;

                //----------------------------------------------------
                // RUNTIMEBINDEDBINOP  (CS4)
                //----------------------------------------------------
                case EXPRKIND.RUNTIMEBINDEDBINOP:
                    {
                        EXPRRUNTIMEBINDEDBINOP rtBinopExpr = expr as EXPRRUNTIMEBINDEDBINOP;

                        EXPR leftExpr = ConvertToRuntimeBindedExpr(
                            rtBinopExpr.Operand1,
                            stmtBuilder);
                        EXPR rightExpr = ConvertToRuntimeBindedExpr(
                            rtBinopExpr.Operand2,
                            stmtBuilder);

                        return CreateRuntimeBindBinaryOperation(
                            rtBinopExpr.ExpressionType,
                            leftExpr,
                            rightExpr,
                            stmtBuilder);
                    }
                    break;

                //----------------------------------------------------
                // EK_MULTIOFFSET:
                // This has to be last!!!
                // To deal /w multiops we add this to the op to obtain the ek in the op table
                //----------------------------------------------------
                case EXPRKIND.MULTIOFFSET:
                default:
                    break;
            }

            return expr;    // not converted.
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindConvertExpr
        //
        /// <summary></summary>
        /// <param name="destTypeSym"></param>
        /// <param name="objectExpr"></param>
        /// <param name="builder"></param>
        /// <returns>Returns an EXPR
        /// which invokes the delegate Func<CallSite, object, *>.</returns>
        //------------------------------------------------------------
        internal EXPR CreateRuntimeBindConvertExpr(
            TYPESYM destTypeSym,
            TYPESYM contextTypeSym,
            EXPR objectExpr,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(destTypeSym != null);

            EXPR callSiteFieldExpr = null;
            EXPR callExpr = null;
            EXPR assignExpr = null;
            EXPRDECL declExpr = null;
            MethWithInst mwi = null;
            EXPR argList = null, argListLast = null;

            //--------------------------------------------------------
            // Create the TYPESYMs of
            //   Func<CallSite, object, *> and
            //   CallSite<Func<CallSite, object, *>>
            //--------------------------------------------------------
            TypeArray funcTypeArguments = new TypeArray();
            funcTypeArguments.Add(this.callSiteTypeSym);
            funcTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);
            funcTypeArguments.Add(destTypeSym);
            funcTypeArguments = Compiler.MainSymbolManager.AllocParams(funcTypeArguments);

            AGGTYPESYM funcInstTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                GetFuncTypeSym(3).GetAggregate(),
                null,
                funcTypeArguments,
                null);

            TypeArray callSiteTypeArguments = new TypeArray();
            callSiteTypeArguments.Add(funcInstTypeSym);
            callSiteTypeArguments = Compiler.MainSymbolManager.AllocParams(callSiteTypeArguments);

            AGGTYPESYM callSiteInstTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G_CALLSITE, true).GetAggregate(),
                null,
                callSiteTypeArguments,
                null);

            //--------------------------------------------------------
            // Create the CallSite field with its parent static anonymous class.
            //--------------------------------------------------------
            AGGTYPESYM runtimeBindAnonTypeSym = CreateRuntimeBindAnonymousClass();

            MEMBVARSYM callSiteFieldSym = CreateRuntimeBindAnonField(
                null,
                callSiteInstTypeSym);

            //--------------------------------------------------------
            // if-Condition
            //--------------------------------------------------------
            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, this.runtimeBindAnonTypeSym),
                0);

            bool isUserDef = false;
            NubInfo nin = null;
            EXPR condExpr = BindStdBinOp(
                null,
                EXPRKIND.EQ,
                callSiteFieldExpr,
                BindNull(null),
                ref isUserDef,
                ref nin);

            EXPRLABEL labelExpr = NewExprLabel();

            builder.Add(MakeGotoIf(
                null,
                condExpr,
                labelExpr,
                false,
                0));

            //EXPRFIELD convertFieldExpr = null;

            //--------------------------------------------------------
            // if-Body (1) Create a CallSiteBinder of Binder.Convert method.
            //--------------------------------------------------------
            //string binderLocName = CreateAnonLocalName("CallSiteBinder");

            LOCVARSYM binderLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("CallSiteBinder"),//binderLocName,
                this.callSiteBinderTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(binderLocSym != null);

            EXPR binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            argList = null;
            argListLast = null;

            // Argument 1

            EXPR flagsExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)CSharpBinderFlags.None));

            EXPR castExpr = null;
            BindSimpleCast(
                null,
                flagsExpr,
                this.csharpBinderFlagsTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 2

            EXPRTYPEOF destTypeOfExpr = BindTypeOf(destTypeSym);
            NewList(destTypeOfExpr, ref argList, ref argListLast);

            // Argument 3

            EXPRTYPEOF contextTypeOfExpr = BindTypeOf(contextTypeSym);
            NewList(contextTypeOfExpr, ref argList, ref argListLast);

            mwi = new MethWithInst(
                this.binderConvertMethodSym,
                this.binderTypeSym,
                BSYMMGR.EmptyTypeArray);

            callExpr = BindToMethod(
                null,
                null,
                mwi,
                argList,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            // assign

            NoteReference(binderLocExpr);
            binderLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                binderLocExpr.TypeSym,
                binderLocExpr,
                callExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = binderLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // if-Body (2)
            //--------------------------------------------------------
            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, this.runtimeBindAnonTypeSym),
                0);

            binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            mwi = new MethWithInst(
                this.callSiteG1CreateMethodSym,
                callSiteInstTypeSym,    //this.callSiteG1TypeSym,
                BSYMMGR.EmptyTypeArray);

            callExpr = BindToMethod(
                null,
                null,
                mwi,
                binderLocExpr,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                callSiteFieldExpr.TypeSym,
                callSiteFieldExpr,
                callExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            builder.Add(MakeStmt(null, assignExpr, 0));

            //--------------------------------------------------------
            // if-Label
            //--------------------------------------------------------
            builder.Add(labelExpr);

            //--------------------------------------------------------
            // Create the delegate.
            //--------------------------------------------------------
            LOCVARSYM funcLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("Func"),
                funcInstTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(funcLocSym != null);

            // Left

            EXPR funcLocExpr = BindToLocal(null, funcLocSym, BindFlagsEnum.MemberSet);

            // Right

            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, runtimeBindAnonTypeSym),
                BindFlagsEnum.RValueRequired);

            EXPR targetExpr = BindToField(
                null,
                callSiteFieldExpr,
                new FieldWithType(callSiteG1TargetFieldSym, callSiteInstTypeSym),
                BindFlagsEnum.RValueRequired);

            // Assign

            NoteReference(funcLocExpr);

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                funcLocExpr.TypeSym,
                funcLocExpr,
                targetExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            //builder.Add(MakeStmt(null, assignExpr, 0));

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = funcLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // Invoke the delegate.
            //--------------------------------------------------------
            funcLocExpr = BindToLocal(null, funcLocSym, BindFlagsEnum.MemberSet);

            argList = null;
            argListLast = null;

            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, runtimeBindAnonTypeSym),
                BindFlagsEnum.RValueRequired);

            NewList(callSiteFieldExpr, ref argList, ref argListLast);
            NewList(objectExpr, ref argList, ref argListLast);

            METHSYM invokeSym = GetFuncInvokeMethod(3);

            EXPR invokeExpr = BindToMethod(
                null,
                funcLocExpr,
                new MethWithInst(invokeSym, funcInstTypeSym, BSYMMGR.EmptyTypeArray),
                argList,
                MemLookFlagsEnum.UserCallable);

            return invokeExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindSetMemberExpr
        //
        /// <summary></summary>
        /// <param name="memberExpr"></param>
        /// <param name="valueExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void CreateRuntimeBindSetMemberExpr(
            EXPRRUNTIMEBINDEDMEMBER memberExpr,
            EXPR valueExpr,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(memberExpr != null && valueExpr != null && builder != null);
            TYPESYM valueTypeSym = valueExpr.TypeSym;

            EXPR callSiteFieldExpr = null;
            EXPRDECL declExpr = null;
            EXPR callExpr = null;
            EXPR assignExpr = null;
            MethWithInst mwi = null;
            EXPR argList = null, argListLast = null;

            //--------------------------------------------------------
            // Create the TYPESYMs of
            //   Action<CallSite, object, *>
            //   CallSite<Action<CallSite, object, *>>
            //--------------------------------------------------------
            TypeArray actionTypeArguments = new TypeArray();
            actionTypeArguments.Add(this.callSiteTypeSym);
            actionTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);
            actionTypeArguments.Add(valueTypeSym);
            actionTypeArguments = Compiler.MainSymbolManager.AllocParams(actionTypeArguments);

            AGGTYPESYM actionInstTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                GetActionTypeSym(3).GetAggregate(),
                null,
                actionTypeArguments,
                null);

            TypeArray callSiteTypeArguments = new TypeArray();
            callSiteTypeArguments.Add(actionInstTypeSym);
            callSiteTypeArguments = Compiler.MainSymbolManager.AllocParams(callSiteTypeArguments);

            AGGTYPESYM callSiteInstTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G_CALLSITE,true).GetAggregate(),
                null,
                callSiteTypeArguments,
                null);

            //--------------------------------------------------------
            // Create the CallSite field with its parent static anonymous class.
            //--------------------------------------------------------
            AGGTYPESYM runtimeBindAnonTypeSym = CreateRuntimeBindAnonymousClass();

            MEMBVARSYM callSiteFieldSym = CreateRuntimeBindAnonField(
                null,
                callSiteInstTypeSym);

            //--------------------------------------------------------
            // if-Condition
            //--------------------------------------------------------
            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, this.runtimeBindAnonTypeSym),
                0);

            bool isUserDef = false;
            NubInfo nin = null;
            EXPR condExpr = BindStdBinOp(
                null,
                EXPRKIND.EQ,
                callSiteFieldExpr,
                BindNull(null),
                ref isUserDef,
                ref nin);

            EXPRLABEL labelExpr = NewExprLabel();

            builder.Add(MakeGotoIf(
                null,
                condExpr,
                labelExpr,
                false,
                0));

            //--------------------------------------------------------
            // if-Body (1) Create CSharpArgumentInfo[2]
            //--------------------------------------------------------
            EXPRDECL locDeclExpr = CreateCSharpArgumentInfoArray(2);
            DebugUtil.Assert(locDeclExpr != null);
            builder.Add(locDeclExpr);

            LOCVARSYM argInfoArrayLocSym = locDeclExpr.LocVarSym;

            //--------------------------------------------------------
            // if-Body (2) Set the elements of CSharpArgumentInfo[2]
            //--------------------------------------------------------
            EXPRSTMTAS setArgExpr = SetCSharpArgumentinfoArray(
                argInfoArrayLocSym,
                0,
                CSharpArgumentInfoFlags.None,
                null);
            DebugUtil.Assert(setArgExpr != null);
            builder.Add(setArgExpr);

            setArgExpr = SetCSharpArgumentinfoArray(
                argInfoArrayLocSym,
                1,
                CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant,
                null);
            DebugUtil.Assert(setArgExpr != null);
            builder.Add(setArgExpr);

            //--------------------------------------------------------
            // if-Body (3) Create a CallSiteBinder by Binder.Setmember method.
            //--------------------------------------------------------
            string binderLocName = CreateAnonLocalName("CallSiteBinder");

            LOCVARSYM binderLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("CallSiteBinder"),
                this.callSiteBinderTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(binderLocSym != null);

            EXPR binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            argList = null;
            argListLast = null;

            // Argument 1

            EXPR flagsExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)CSharpBinderFlags.ResultDiscarded));

            EXPR castExpr = null;
            BindSimpleCast(
                null,
                flagsExpr,
                this.csharpBinderFlagsTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 2

            EXPR memberNameExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.STRING),
                new ConstValInit(memberExpr.MemberName));

            NewList(memberNameExpr, ref argList, ref argListLast);

            // Argument 3

            EXPRTYPEOF contextTypeOfExpr = BindTypeOf(memberExpr.ContextSym);
            NewList(contextTypeOfExpr, ref argList, ref argListLast);

            // Argument 4

            EXPR infoArrayExpr = BindToLocal(
                null,
                argInfoArrayLocSym,
                BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

            NewList(infoArrayExpr, ref argList, ref argListLast);

            mwi = new MethWithInst(
                this.binderSetMemberMethodSym,
                this.binderTypeSym,
                BSYMMGR.EmptyTypeArray);

            callExpr = BindToMethod(
                null,
                null,
                mwi,
                argList,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            // assign

            NoteReference(binderLocExpr);
            binderLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                binderLocExpr.TypeSym,
                binderLocExpr,
                callExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = binderLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // if-Body (4)
            //--------------------------------------------------------
            //runtimeBindAnonClassExpr = NewExpr(null, EXPRKIND.CLASS, this.runtimeBindAnonTypeSym);

            callSiteFieldExpr = BindToField(
                null,
                null,//runtimeBindAnonClassExpr,
                new FieldWithType(callSiteFieldSym, this.runtimeBindAnonTypeSym),
                0);

            binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            mwi = new MethWithInst(
                this.callSiteG1CreateMethodSym,
                callSiteInstTypeSym,    //this.callSiteG1TypeSym,
                BSYMMGR.EmptyTypeArray);

            callExpr = BindToMethod(
                null,
                null,
                mwi,
                binderLocExpr,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                callSiteFieldExpr.TypeSym,
                callSiteFieldExpr,
                callExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            builder.Add(MakeStmt(null, assignExpr, 0));

            //--------------------------------------------------------
            // if-Label
            //--------------------------------------------------------
            builder.Add(labelExpr);

            //--------------------------------------------------------
            // Create the delegate.
            //--------------------------------------------------------
            LOCVARSYM actionLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("Action"),
                actionInstTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(actionLocSym != null);

            // Left

            EXPR actionLocExpr = BindToLocal(null, actionLocSym, BindFlagsEnum.MemberSet);

            // Right

            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, runtimeBindAnonTypeSym),
                BindFlagsEnum.RValueRequired);

            EXPR targetExpr = BindToField(
                null,
                callSiteFieldExpr,
                new FieldWithType(callSiteG1TargetFieldSym, callSiteInstTypeSym),
                BindFlagsEnum.RValueRequired);

            // Assign

            NoteReference(actionLocExpr);
            actionLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                actionLocExpr.TypeSym,
                actionLocExpr,
                targetExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            //builder.Add(MakeStmt(null, assignExpr, 0));

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = actionLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // Invoke the delegate.
            //--------------------------------------------------------
            METHSYM invokeSym = GetActionInvokeMethod(3);
            actionLocExpr = BindToLocal(null, actionLocSym, BindFlagsEnum.MemberSet);

            argList = null;
            argListLast = null;

            NewList(callSiteFieldExpr, ref argList, ref argListLast);
            NewList(memberExpr.ObjectExpr, ref argList, ref argListLast);
            NewList(valueExpr, ref argList, ref argListLast);

            EXPR invokeExpr = BindToMethod(
                null,
                actionLocExpr,
                new MethWithInst(invokeSym, actionInstTypeSym, BSYMMGR.EmptyTypeArray),
                argList,
                MemLookFlagsEnum.UserCallable);

            builder.Add(MakeStmt(null, invokeExpr, 0));
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindGetMemberExpr
        //
        /// <summary></summary>
        /// <param name="memberExpr"></param>
        /// <param name="builder"></param>
        /// <returns>Returns an EXPR
        /// which invokes the delegate Func<CallSite, object, object>.</returns>
        //------------------------------------------------------------
        internal EXPR CreateRuntimeBindGetMemberExpr(
            EXPRRUNTIMEBINDEDMEMBER memberExpr,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(memberExpr != null && builder != null);

            METHSYM invokeSym = GetFuncInvokeMethod(3);

            EXPR callSiteFieldExpr = null;
            EXPRDECL declExpr = null;
            EXPR callExpr = null;
            EXPR assignExpr = null;
            MethWithInst mwi = null;
            EXPR argList = null, argListLast = null;

            //--------------------------------------------------------
            // Create the TYPESYMs of
            //   Func<CallSite, object, object>
            //   CallSite<Func<CallSite, object, object>>
            //--------------------------------------------------------
            TypeArray funcTypeArguments = new TypeArray();
            funcTypeArguments.Add(this.callSiteTypeSym);
            funcTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);
            funcTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);
            funcTypeArguments = Compiler.MainSymbolManager.AllocParams(funcTypeArguments);

            AGGTYPESYM funcInstTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                GetFuncTypeSym(3).GetAggregate(),
                null,
                funcTypeArguments,
                null);

            TypeArray callSiteTypeArguments = new TypeArray();
            callSiteTypeArguments.Add(funcInstTypeSym);
            callSiteTypeArguments = Compiler.MainSymbolManager.AllocParams(callSiteTypeArguments);

            AGGTYPESYM callSiteInstTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G_CALLSITE, true).GetAggregate(),
                null,
                callSiteTypeArguments,
                null);

            //--------------------------------------------------------
            // Create the CallSite field with its parent static anonymous class.
            //--------------------------------------------------------
            AGGTYPESYM runtimeBindAnonTypeSym = CreateRuntimeBindAnonymousClass();

            MEMBVARSYM callSiteFieldSym = CreateRuntimeBindAnonField(
                null,
                callSiteInstTypeSym);

            //--------------------------------------------------------
            // if-Condition
            //--------------------------------------------------------
            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, this.runtimeBindAnonTypeSym),
                0);

            bool isUserDef = false;
            NubInfo nin = null;
            EXPR condExpr = BindStdBinOp(
                null,
                EXPRKIND.EQ,
                callSiteFieldExpr,
                BindNull(null),
                ref isUserDef,
                ref nin);

            EXPRLABEL labelExpr = NewExprLabel();

            builder.Add(MakeGotoIf(
                null,
                condExpr,
                labelExpr,
                false,
                0));

            //--------------------------------------------------------
            // if-Body (1) Create CSharpArgumentInfo[1]
            //--------------------------------------------------------
            EXPRDECL locDeclExpr = CreateCSharpArgumentInfoArray(1);
            DebugUtil.Assert(locDeclExpr != null);
            builder.Add(locDeclExpr);

            LOCVARSYM argInfoArrayLocSym = locDeclExpr.LocVarSym;

            //--------------------------------------------------------
            // if-Body (2) Set the elements of CSharpArgumentInfo[2]
            //--------------------------------------------------------
            EXPRSTMTAS setArgExpr = SetCSharpArgumentinfoArray(
                argInfoArrayLocSym,
                0,
                CSharpArgumentInfoFlags.None,
                null);
            DebugUtil.Assert(setArgExpr != null);
            builder.Add(setArgExpr);

            //--------------------------------------------------------
            // if-Body (3) Create a CallSiteBinder by Binder.Getmember method.
            //--------------------------------------------------------
            //string binderLocName = CreateAnonLocalName("CallSiteBinder");

            LOCVARSYM binderLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("CallSiteBinder"),
                this.callSiteBinderTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(binderLocSym != null);

            EXPR binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            argList = null;
            argListLast = null;

            // Argument 1

            EXPR flagsExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)CSharpBinderFlags.None));

            EXPR castExpr = null;
            BindSimpleCast(
                null,
                flagsExpr,
                this.csharpBinderFlagsTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 2

            EXPR memberNameExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.STRING),
                new ConstValInit(memberExpr.MemberName));

            NewList(memberNameExpr, ref argList, ref argListLast);

            // Argument 3

            EXPRTYPEOF contextTypeOfExpr = BindTypeOf(memberExpr.ContextSym);
            NewList(contextTypeOfExpr, ref argList, ref argListLast);

            // Argument 4

            EXPR infoArrayExpr = BindToLocal(
                null,
                argInfoArrayLocSym,
                BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

            NewList(infoArrayExpr, ref argList, ref argListLast);

            mwi = new MethWithInst(
                this.binderGetMemberMethodSym,
                this.binderTypeSym,
                BSYMMGR.EmptyTypeArray);

            callExpr = BindToMethod(
                null,
                null,
                mwi,
                argList,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            // assign

            NoteReference(binderLocExpr);
            binderLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                binderLocExpr.TypeSym,
                binderLocExpr,
                callExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = binderLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // if-Body (4) Create the CallSite<...>
            //--------------------------------------------------------
            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, this.runtimeBindAnonTypeSym),
                0);

            binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            mwi = new MethWithInst(
                this.callSiteG1CreateMethodSym,
                callSiteInstTypeSym,    //this.callSiteG1TypeSym,
                BSYMMGR.EmptyTypeArray);

            callExpr = BindToMethod(
                null,
                null,
                mwi,
                binderLocExpr,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                callSiteFieldExpr.TypeSym,
                callSiteFieldExpr,
                callExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            builder.Add(MakeStmt(null, assignExpr, 0));

            //--------------------------------------------------------
            // if-Label
            //--------------------------------------------------------
            builder.Add(labelExpr);

            //--------------------------------------------------------
            // Create the delegate.
            //--------------------------------------------------------
            LOCVARSYM funcLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("Func"),
                funcInstTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(funcLocSym != null);

            // Left

            EXPR funcLocExpr = BindToLocal(null, funcLocSym, BindFlagsEnum.MemberSet);

            // Right

            callSiteFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteFieldSym, runtimeBindAnonTypeSym),
                BindFlagsEnum.RValueRequired);

            EXPR targetExpr = BindToField(
                null,
                callSiteFieldExpr,
                new FieldWithType(callSiteG1TargetFieldSym, callSiteInstTypeSym),
                BindFlagsEnum.RValueRequired);

            // Assign

            NoteReference(funcLocExpr);
            funcLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                funcLocExpr.TypeSym,
                funcLocExpr,
                targetExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            //builder.Add(MakeStmt(null, assignExpr, 0));

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = funcLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // Invoke the delegate.
            //--------------------------------------------------------
            funcLocExpr = BindToLocal(null, funcLocSym, BindFlagsEnum.MemberSet);

            argList = null;
            argListLast = null;

            NewList(callSiteFieldExpr, ref argList, ref argListLast);
            NewList(memberExpr.ObjectExpr, ref argList, ref argListLast);

            EXPR invokeExpr = BindToMethod(
                null,
                funcLocExpr,
                new MethWithInst(invokeSym, funcInstTypeSym, BSYMMGR.EmptyTypeArray),
                argList,
                MemLookFlagsEnum.UserCallable);
#if DEBUG
            StringBuilder sb = new StringBuilder();
            DebugUtil.DebugSymsOutput(sb);
            sb.Length = 0;
            DebugUtil.DebugExprsOutput(sb);
#endif
            return invokeExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindInvokeMember
        //
        /// <summary></summary>
        /// <param name="callExpr"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR CreateRuntimeBindInvokeMember(
            EXPRCALL callExpr,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(callExpr != null && builder != null);

            string methodName = null;
            TypeArray methodTypeArguments = null;

            switch (callExpr.Kind)
            {
                case EXPRKIND.CALL:
                    {
                        MethWithInst mi = callExpr.MethodWithInst;
                        methodName = mi.MethSym.Name;
                        methodTypeArguments = mi.TypeArguments;
                    }
                    break;

                case EXPRKIND.RUNTIMEBINDEDINVOCATION:
                    {
                        EXPRRUNTIMEBINDEDINVOCATION invExpr
                            = callExpr as EXPRRUNTIMEBINDEDINVOCATION;
                        methodName = invExpr.MemberName;
                        methodTypeArguments = invExpr.TypeArguments;
                    }
                    break;

                default:
                    DebugUtil.Assert(false);
                    break;
            }

            bool isStatic = (callExpr.ObjectExpr == null);
            bool discardResult = callExpr.TypeSym.IsVoidType;

            AGGTYPESYM callSiteGenTypeSym = null;
            TypeArray callSiteGenTypeArguments = new TypeArray();
            AGGTYPESYM delegateAggTypeSym = null;
            TypeArray delegateTypeArguments = new TypeArray();
            int delegateArgCount = 0;
            METHSYM delegateInvokeSym = null;
            string delegateName = null;

            CSharpBinderFlags binderFlags = CSharpBinderFlags.None;

            delegateTypeArguments.Add(this.callSiteTypeSym);
            if (isStatic)
            {
                delegateTypeArguments.Add(this.systemTypeAggTypeSym);
            }
            else
            {
                delegateTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);
            }

            //--------------------------------------------------------
            // Count and Convert the arguments
            //--------------------------------------------------------
            EXPR methArgExpr = null;
            EXPR methArgExprLast = null;

            EXPR expr = callExpr.ArgumentsExpr;
            int methArgCount = 0;
            while (expr != null)
            {
                EXPR arg1 = null;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg1 = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg1 = expr;
                    expr = null;
                }
                EXPR arg2 = ConvertToRuntimeBindedExpr(arg1, builder);
                NewList(arg2, ref methArgExpr, ref methArgExprLast);
                methArgCount++;
                delegateTypeArguments.Add(arg2.TypeSym);
            }

            if (discardResult)
            {
                delegateTypeArguments
                    = Compiler.MainSymbolManager.AllocParams(delegateTypeArguments);
                binderFlags |= CSharpBinderFlags.ResultDiscarded;

                delegateAggTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                    GetActionTypeSym(delegateTypeArguments.Count).GetAggregate(),
                    null,
                    delegateTypeArguments,
                    null);

                delegateInvokeSym = GetActionInvokeMethod(delegateTypeArguments.Count);
                delegateName = "Action";
            }
            else
            {
                delegateTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);
                delegateTypeArguments
                    = Compiler.MainSymbolManager.AllocParams(delegateTypeArguments);

                delegateAggTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                    GetFuncTypeSym(delegateTypeArguments.Count).GetAggregate(),
                    null,
                    delegateTypeArguments,
                    null);

                delegateInvokeSym = GetFuncInvokeMethod(delegateTypeArguments.Count);
                delegateName = "Func";
            }
            delegateArgCount = delegateTypeArguments.Count;

            callSiteGenTypeArguments.Add(delegateAggTypeSym);

            callSiteGenTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G_CALLSITE, true).GetAggregate(),
                null,
                callSiteGenTypeArguments,
                null);

            //--------------------------------------------------------
            // Create the CallSite field with its parent static anonymous class.
            //--------------------------------------------------------
            AGGTYPESYM runtimeBindAnonTypeSym = CreateRuntimeBindAnonymousClass();

            MEMBVARSYM callSiteGenFieldSym = CreateRuntimeBindAnonField(
                null,
                callSiteGenTypeSym);

            //--------------------------------------------------------
            // if-Condition
            //--------------------------------------------------------
            EXPR callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, this.runtimeBindAnonTypeSym),
                0);

            bool isUserDef = false;
            NubInfo nin = null;
            EXPR condExpr = BindStdBinOp(
                null,
                EXPRKIND.EQ,
                callSiteGenFieldExpr,
                BindNull(null),
                ref isUserDef,
                ref nin);

            EXPRLABEL labelExpr = NewExprLabel();

            builder.Add(MakeGotoIf(
                null,
                condExpr,
                labelExpr,
                false,
                0));

            //--------------------------------------------------------
            // if-Body (1) Create CSharpArgumentInfo[];
            //--------------------------------------------------------
            CSharpArgumentInfoFlags argInfoFlags = CSharpArgumentInfoFlags.None;

            EXPRDECL argInfoArrayDeclExpr = CreateCSharpArgumentInfoArray(methArgCount + 1);
            DebugUtil.Assert(argInfoArrayDeclExpr != null);
            builder.Add(argInfoArrayDeclExpr);
            LOCVARSYM argInfoArrayLocSym = argInfoArrayDeclExpr.LocVarSym;

            if (isStatic)
            {
                argInfoFlags |=
                    CSharpArgumentInfoFlags.UseCompileTimeType |
                    CSharpArgumentInfoFlags.IsStaticType;
            }

            EXPRSTMTAS setArgExpr = SetCSharpArgumentinfoArray(
                argInfoArrayLocSym,
                0,
                argInfoFlags,
                null);
            DebugUtil.Assert(setArgExpr != null);
            builder.Add(setArgExpr);

            int idx = 0;
            expr = methArgExpr;
            while (expr != null)
            {
                EXPR arg1 = null;
                argInfoFlags = CSharpArgumentInfoFlags.None;

                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg1 = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg1 = expr;
                    expr = null;
                }
                if (arg1.Kind == EXPRKIND.CONSTANT)
                {
                    argInfoFlags |=
                        CSharpArgumentInfoFlags.UseCompileTimeType |
                        CSharpArgumentInfoFlags.Constant;
                }

                ++idx;
                setArgExpr = SetCSharpArgumentinfoArray(
                    argInfoArrayLocSym,
                    idx,
                    argInfoFlags,
                    null);
                DebugUtil.Assert(setArgExpr != null);
                builder.Add(setArgExpr);
            }

            //--------------------------------------------------------
            // if-Body (2) Create a CallSiteBinder by Binder.InvokeMember method.
            //
            //   public static CallSiteBinder InvokeMember(
            //       CSharpBinderFlags flags,
            //       string name,
            //       IEnumerable<Type> typeArguments,
            //       Type context,
            //       IEnumerable<CSharpArgumentInfo> argumentInfo
            //   )
            //--------------------------------------------------------
            LOCVARSYM binderLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("CallSiteBinder"),
                this.callSiteBinderTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(binderLocSym != null);

            EXPR binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            EXPR argList = null;
            EXPR argListLast = null;

            // Argument 1 : flags

            EXPR flagsExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)binderFlags));

            EXPR castExpr = null;
            BindSimpleCast(
                null,
                flagsExpr,
                this.csharpBinderFlagsTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 2 : name

            EXPR memberNameExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.STRING),
                new ConstValInit(methodName));

            NewList(memberNameExpr, ref argList, ref argListLast);

            // Argument 3 : typeArguments

            EXPR typeArgsExpr = null;
            if (methodTypeArguments.Count > 0)
            {
                LOCVARSYM taLocSym = CreateSystemTypeArray(methodTypeArguments, builder);
                typeArgsExpr = MakeLocal(null, taLocSym, true);
            }
            else
            {
                typeArgsExpr = BindNull(null);
            }
            NewList(typeArgsExpr, ref argList, ref argListLast);

            // Argument 4 : context

            EXPRTYPEOF contextTypeOfExpr = BindTypeOf(this.methodSym.ParentAggSym.GetThisType());
            NewList(contextTypeOfExpr, ref argList, ref argListLast);

            // Argument 5 argumentInfo

            EXPR infoArrayExpr = BindToLocal(
                null,
                argInfoArrayLocSym,
                BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

            NewList(infoArrayExpr, ref argList, ref argListLast);

            // Call

            MethWithInst mwi = new MethWithInst(
                this.binderInvokeMemberMethodSym,
                this.binderTypeSym,
                BSYMMGR.EmptyTypeArray);

            EXPR callMethExpr = BindToMethod(
                null,
                null,
                mwi,
                argList,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            // assign

            NoteReference(binderLocExpr);
            binderLocSym.LocSlotInfo.HasInit = true;

            EXPR assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                binderLocExpr.TypeSym,
                binderLocExpr,
                callMethExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = binderLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // if-Body (3) Create the CallSite<...>
            //--------------------------------------------------------
            callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, this.runtimeBindAnonTypeSym),
                0);

            binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            mwi = new MethWithInst(
                this.callSiteG1CreateMethodSym,
                callSiteGenTypeSym,
                BSYMMGR.EmptyTypeArray);

            EXPR callsiteGenCreateExpr = BindToMethod(
                null,
                null,
                mwi,
                binderLocExpr,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                callSiteGenFieldExpr.TypeSym,
                callSiteGenFieldExpr,
                callsiteGenCreateExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            builder.Add(MakeStmt(null, assignExpr, 0));

            //--------------------------------------------------------
            // if-Label
            //--------------------------------------------------------
            builder.Add(labelExpr);

            //--------------------------------------------------------
            // Create the delegate.
            //--------------------------------------------------------
            LOCVARSYM delegateLocSym = CreateNewLocVarSym(
                CreateAnonLocalName(delegateName),
                delegateAggTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(delegateLocSym != null);

            // Left

            EXPR delegateLocExpr = BindToLocal(null, delegateLocSym, BindFlagsEnum.MemberSet);

            // Right

            callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, runtimeBindAnonTypeSym),
                BindFlagsEnum.RValueRequired);

            EXPR targetExpr = BindToField(
                null,
                callSiteGenFieldExpr,
                new FieldWithType(callSiteG1TargetFieldSym, callSiteGenTypeSym),
                BindFlagsEnum.RValueRequired);

            // Assign

            NoteReference(delegateLocExpr);
            delegateLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                delegateLocExpr.TypeSym,
                delegateLocExpr,
                targetExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            //builder.Add(MakeStmt(null, assignExpr, 0));

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = delegateLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // Invoke the delegate.
            //--------------------------------------------------------
            delegateLocExpr = BindToLocal(null, delegateLocSym, BindFlagsEnum.MemberSet);

            argList = null;
            argListLast = null;

            NewList(callSiteGenFieldExpr, ref argList, ref argListLast);

            if (isStatic)
            {
                EXPRTYPEOF parentTypeOfExpr = BindTypeOf(methodSym.ParentAggSym.GetThisType());
                NewList(parentTypeOfExpr, ref argList, ref argListLast);
            }
            else
            {
                NewList(callExpr.ObjectExpr, ref argList, ref argListLast);
            }

            if (methArgExpr != null)
            {
                NewList(methArgExpr, ref argList, ref argListLast);
            }

            EXPR invokeExpr = BindToMethod(
                null,
                delegateLocExpr,
                new MethWithInst(delegateInvokeSym, delegateAggTypeSym, BSYMMGR.EmptyTypeArray),
                argList,
                MemLookFlagsEnum.UserCallable);
#if DEBUG
            StringBuilder sb = new StringBuilder();
            DebugUtil.DebugSymsOutput(sb);
            sb.Length = 0;
            DebugUtil.DebugExprsOutput(sb);
#endif
            return invokeExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindRuntimeBindedUnaOp
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="op"></param>
        /// <param name="operandExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindRuntimeBindedUnaOp(
            BASENODE treeNode,
            OPERATOR op,
            EXPR operandExpr)
        {
            EXPRRUNTIMEBINDEDUNAOP expr = NewExprCore(
                treeNode,
                EXPRKIND.RUNTIMEBINDEDUNAOP,
                GetRequiredPredefinedType(PREDEFTYPE.OBJECT),
                EXPRFLAG.BINOP,
                EXPRKIND.RUNTIMEBINDEDUNAOP) as EXPRRUNTIMEBINDEDUNAOP;

            expr.ExpressionType = OperatorToExpressionType(op);
            expr.Operand1 = operandExpr;

            return expr;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindUnaryOperation
        //
        /// <summary></summary>
        /// <param name="exType"></param>
        /// <param name="operandExpr"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR CreateRuntimeBindUnaryOperation(
            ExpressionType exType,
            EXPR operandExpr,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(operandExpr != null);

            AGGTYPESYM callSiteGenTypeSym = null;
            TypeArray callSiteGenTypeArguments = new TypeArray();
            AGGTYPESYM delegateAggTypeSym = null;
            TypeArray delegateTypeArguments = new TypeArray();
            METHSYM delegateInvokeSym = null;
            string delegateName = null;

            CSharpBinderFlags binderFlags = CSharpBinderFlags.None;

            //--------------------------------------------------------
            // Create the TYPESYMs of the CallSite and the delegate.
            //--------------------------------------------------------
            delegateTypeArguments.Add(this.callSiteTypeSym);
            delegateTypeArguments.Add(operandExpr.TypeSym);
            delegateTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);

            delegateTypeArguments
                = Compiler.MainSymbolManager.AllocParams(delegateTypeArguments);

            delegateAggTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                GetFuncTypeSym(delegateTypeArguments.Count).GetAggregate(),
                null,
                delegateTypeArguments,
                null);

            delegateInvokeSym = GetFuncInvokeMethod(delegateTypeArguments.Count);

            callSiteGenTypeArguments.Add(delegateAggTypeSym);
            callSiteGenTypeArguments
                = Compiler.MainSymbolManager.AllocParams(callSiteGenTypeArguments);

            callSiteGenTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G_CALLSITE, true).GetAggregate(),
                null,
                callSiteGenTypeArguments,
                null);

            //--------------------------------------------------------
            // Create the CallSite field with its parent static anonymous class.
            //--------------------------------------------------------
            AGGTYPESYM runtimeBindAnonTypeSym = CreateRuntimeBindAnonymousClass();

            MEMBVARSYM callSiteGenFieldSym = CreateRuntimeBindAnonField(
                null,
                callSiteGenTypeSym);

            //--------------------------------------------------------
            // if-Condition
            //--------------------------------------------------------
            EXPR callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, this.runtimeBindAnonTypeSym),
                0);

            bool isUserDef = false;
            NubInfo nin = null;
            EXPR condExpr = BindStdBinOp(
                null,
                EXPRKIND.EQ,
                callSiteGenFieldExpr,
                BindNull(null),
                ref isUserDef,
                ref nin);

            EXPRLABEL labelExpr = NewExprLabel();

            builder.Add(MakeGotoIf(
                null,
                condExpr,
                labelExpr,
                false,
                0));

            //--------------------------------------------------------
            // if-Body (1) Create CSharpArgumentInfo[];
            //--------------------------------------------------------
            CSharpArgumentInfoFlags argInfoFlags = CSharpArgumentInfoFlags.None;

            EXPRDECL argInfoArrayDeclExpr = CreateCSharpArgumentInfoArray(1);
            DebugUtil.Assert(argInfoArrayDeclExpr != null);
            builder.Add(argInfoArrayDeclExpr);
            LOCVARSYM argInfoArrayLocSym = argInfoArrayDeclExpr.LocVarSym;

            argInfoFlags = CSharpArgumentInfoFlags.None;
            if (operandExpr.Kind == EXPRKIND.CONSTANT)
            {
                argInfoFlags |=
                    CSharpArgumentInfoFlags.UseCompileTimeType |
                    CSharpArgumentInfoFlags.Constant;
            }

            EXPRSTMTAS setArgExpr = SetCSharpArgumentinfoArray(
                argInfoArrayLocSym,
                0,
                argInfoFlags,
                null);
            DebugUtil.Assert(setArgExpr != null);
            builder.Add(setArgExpr);

            //--------------------------------------------------------
            // if-Body (2) Create a CallSiteBinder by Binder.InvokeMember method.
            //
            //   public static CallSiteBinder InvokeMember(
            //       CSharpBinderFlags flags,
            //       string name,
            //       IEnumerable<Type> typeArguments,
            //       Type context,
            //       IEnumerable<CSharpArgumentInfo> argumentInfo
            //   )
            //--------------------------------------------------------
            LOCVARSYM binderLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("CallSiteBinder"),
                this.callSiteBinderTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(binderLocSym != null);

            EXPR binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            EXPR argList = null;
            EXPR argListLast = null;

            // Argument 1 : flags

            EXPR flagsExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)binderFlags));

            EXPR castExpr = null;
            BindSimpleCast(
                null,
                flagsExpr,
                this.csharpBinderFlagsTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 2 : ExpressionType

            EXPR extypeExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new CONSTVAL((int)exType));

            castExpr = null;
            BindSimpleCast(
                null,
                extypeExpr,
                this.expressionTypeTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 3 : context

            EXPRTYPEOF contextTypeOfExpr = BindTypeOf(this.methodSym.ParentAggSym.GetThisType());
            NewList(contextTypeOfExpr, ref argList, ref argListLast);

            // Argument 4 argumentInfo

            EXPR infoArrayExpr = BindToLocal(
                null,
                argInfoArrayLocSym,
                BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

            NewList(infoArrayExpr, ref argList, ref argListLast);

            // Call

            MethWithInst mwi = new MethWithInst(
                this.binderUnaryOperationMethodSym,
                this.binderTypeSym,
                BSYMMGR.EmptyTypeArray);

            EXPR callMethExpr = BindToMethod(
                null,
                null,
                mwi,
                argList,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            // assign

            NoteReference(binderLocExpr);
            binderLocSym.LocSlotInfo.HasInit = true;

            EXPR assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                binderLocExpr.TypeSym,
                binderLocExpr,
                callMethExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = binderLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // if-Body (3) Create the CallSite<...>
            //--------------------------------------------------------
            callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, this.runtimeBindAnonTypeSym),
                0);

            binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            mwi = new MethWithInst(
                this.callSiteG1CreateMethodSym,
                callSiteGenTypeSym,
                BSYMMGR.EmptyTypeArray);

            EXPR callsiteGenCreateExpr = BindToMethod(
                null,
                null,
                mwi,
                binderLocExpr,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                callSiteGenFieldExpr.TypeSym,
                callSiteGenFieldExpr,
                callsiteGenCreateExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            builder.Add(MakeStmt(null, assignExpr, 0));

            //--------------------------------------------------------
            // if-Label
            //--------------------------------------------------------
            builder.Add(labelExpr);

            //--------------------------------------------------------
            // Create the delegate.
            //--------------------------------------------------------
            LOCVARSYM delegateLocSym = CreateNewLocVarSym(
                CreateAnonLocalName(delegateName),
                delegateAggTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(delegateLocSym != null);

            // Left

            EXPR delegateLocExpr = BindToLocal(null, delegateLocSym, BindFlagsEnum.MemberSet);

            // Right

            callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, runtimeBindAnonTypeSym),
                BindFlagsEnum.RValueRequired);

            EXPR targetExpr = BindToField(
                null,
                callSiteGenFieldExpr,
                new FieldWithType(callSiteG1TargetFieldSym, callSiteGenTypeSym),
                BindFlagsEnum.RValueRequired);

            // Assign

            NoteReference(delegateLocExpr);
            delegateLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                delegateLocExpr.TypeSym,
                delegateLocExpr,
                targetExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = delegateLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // Invoke the delegate.
            //--------------------------------------------------------
            delegateLocExpr = BindToLocal(null, delegateLocSym, BindFlagsEnum.MemberSet);

            argList = null;
            argListLast = null;

            NewList(callSiteGenFieldExpr, ref argList, ref argListLast);
            NewList(operandExpr, ref argList, ref argListLast);

            EXPR invokeExpr = BindToMethod(
                null,
                delegateLocExpr,
                new MethWithInst(delegateInvokeSym, delegateAggTypeSym, BSYMMGR.EmptyTypeArray),
                argList,
                MemLookFlagsEnum.UserCallable);
#if DEBUG
            StringBuilder sb = new StringBuilder();
            DebugUtil.DebugSymsOutput(sb);
            sb.Length = 0;
            DebugUtil.DebugExprsOutput(sb);
#endif
            return invokeExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindRuntimeBindedBinOp
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="op"></param>
        /// <param name="leftExpr"></param>
        /// <param name="rightExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindRuntimeBindedBinOp(
            BASENODE treeNode,
            OPERATOR op,
            EXPR leftExpr,
            EXPR rightExpr)
        {
            EXPRRUNTIMEBINDEDBINOP expr = NewExprCore(
                treeNode,
                EXPRKIND.RUNTIMEBINDEDBINOP,
                GetRequiredPredefinedType(PREDEFTYPE.OBJECT),
                EXPRFLAG.BINOP,
                EXPRKIND.RUNTIMEBINDEDBINOP) as EXPRRUNTIMEBINDEDBINOP;

            expr.ExpressionType = OperatorToExpressionType(op);
            expr.Operand1 = leftExpr;
            expr.Operand2 = rightExpr;

            return expr;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateRuntimeBindBinaryOperation
        //
        /// <summary></summary>
        /// <param name="binopExpr"></param>
        /// <param name="exType"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR CreateRuntimeBindBinaryOperation(
            ExpressionType exType,
            EXPR operand1,
            EXPR operand2,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(operand1 != null && operand2 != null);

            AGGTYPESYM callSiteGenTypeSym = null;
            TypeArray callSiteGenTypeArguments = new TypeArray();
            AGGTYPESYM delegateAggTypeSym = null;
            TypeArray delegateTypeArguments = new TypeArray();
            METHSYM delegateInvokeSym = null;
            string delegateName = null;

            CSharpBinderFlags binderFlags = CSharpBinderFlags.None;

            //--------------------------------------------------------
            // Create the TYPESYMs of the CallSite and the delegate.
            //--------------------------------------------------------
            delegateTypeArguments.Add(this.callSiteTypeSym);
            delegateTypeArguments.Add(operand1.TypeSym);
            delegateTypeArguments.Add(operand2.TypeSym);
            delegateTypeArguments.Add(Compiler.MainSymbolManager.ObjectTypeSym);

            delegateTypeArguments
                = Compiler.MainSymbolManager.AllocParams(delegateTypeArguments);

            delegateAggTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                GetFuncTypeSym(delegateTypeArguments.Count).GetAggregate(),
                null,
                delegateTypeArguments,
                null);

            delegateInvokeSym = GetFuncInvokeMethod(delegateTypeArguments.Count);

            callSiteGenTypeArguments.Add(delegateAggTypeSym);
            callSiteGenTypeArguments
                = Compiler.MainSymbolManager.AllocParams(callSiteGenTypeArguments);

            callSiteGenTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                Compiler.GetOptPredefType(PREDEFTYPE.G_CALLSITE, true).GetAggregate(),
                null,
                callSiteGenTypeArguments,
                null);

            //--------------------------------------------------------
            // Create the CallSite field with its parent static anonymous class.
            //--------------------------------------------------------
            AGGTYPESYM runtimeBindAnonTypeSym = CreateRuntimeBindAnonymousClass();

            MEMBVARSYM callSiteGenFieldSym = CreateRuntimeBindAnonField(
                null,
                callSiteGenTypeSym);

            //--------------------------------------------------------
            // if-Condition
            //--------------------------------------------------------
            EXPR callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, this.runtimeBindAnonTypeSym),
                0);

            bool isUserDef = false;
            NubInfo nin = null;
            EXPR condExpr = BindStdBinOp(
                null,
                EXPRKIND.EQ,
                callSiteGenFieldExpr,
                BindNull(null),
                ref isUserDef,
                ref nin);

            EXPRLABEL labelExpr = NewExprLabel();

            builder.Add(MakeGotoIf(
                null,
                condExpr,
                labelExpr,
                false,
                0));

            //--------------------------------------------------------
            // if-Body (1) Create CSharpArgumentInfo[];
            //--------------------------------------------------------
            CSharpArgumentInfoFlags argInfoFlags = CSharpArgumentInfoFlags.None;

            EXPRDECL argInfoArrayDeclExpr = CreateCSharpArgumentInfoArray(2);
            DebugUtil.Assert(argInfoArrayDeclExpr != null);
            builder.Add(argInfoArrayDeclExpr);
            LOCVARSYM argInfoArrayLocSym = argInfoArrayDeclExpr.LocVarSym;

            EXPR termExpr;
            for (int i = 0; i < 2; ++i)
            {
                termExpr = (i == 0 ? operand1 : operand2);

                argInfoFlags = CSharpArgumentInfoFlags.None;
                if (termExpr.Kind == EXPRKIND.CONSTANT)
                {
                    argInfoFlags |=
                        CSharpArgumentInfoFlags.UseCompileTimeType |
                        CSharpArgumentInfoFlags.Constant;
                }

                EXPRSTMTAS setArgExpr = SetCSharpArgumentinfoArray(
                    argInfoArrayLocSym,
                    i,
                    argInfoFlags,
                    null);
                DebugUtil.Assert(setArgExpr != null);
                builder.Add(setArgExpr);
            }

            //--------------------------------------------------------
            // if-Body (2) Create a CallSiteBinder by Binder.InvokeMember method.
            //
            //   public static CallSiteBinder InvokeMember(
            //       CSharpBinderFlags flags,
            //       string name,
            //       IEnumerable<Type> typeArguments,
            //       Type context,
            //       IEnumerable<CSharpArgumentInfo> argumentInfo
            //   )
            //--------------------------------------------------------
            LOCVARSYM binderLocSym = CreateNewLocVarSym(
                CreateAnonLocalName("CallSiteBinder"),
                this.callSiteBinderTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(binderLocSym != null);

            EXPR binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            EXPR argList = null;
            EXPR argListLast = null;

            // Argument 1 : flags

            EXPR flagsExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)binderFlags));

            EXPR castExpr = null;
            BindSimpleCast(
                null,
                flagsExpr,
                this.csharpBinderFlagsTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 2 : ExpressionType

            EXPR extypeExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new CONSTVAL((int)exType));

            castExpr = null;
            BindSimpleCast(
                null,
                extypeExpr,
                this.expressionTypeTypeSym,
                ref castExpr,
                0);

            NewList(castExpr, ref argList, ref argListLast);

            // Argument 3 : context

            EXPRTYPEOF contextTypeOfExpr = BindTypeOf(this.methodSym.ParentAggSym.GetThisType());
            NewList(contextTypeOfExpr, ref argList, ref argListLast);

            // Argument 4 argumentInfo

            EXPR infoArrayExpr = BindToLocal(
                null,
                argInfoArrayLocSym,
                BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

            NewList(infoArrayExpr, ref argList, ref argListLast);

            // Call

            MethWithInst mwi = new MethWithInst(
                this.binderBinaryOperationMethodSym,
                this.binderTypeSym,
                BSYMMGR.EmptyTypeArray);

            EXPR callMethExpr = BindToMethod(
                null,
                null,
                mwi,
                argList,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            // assign

            NoteReference(binderLocExpr);
            binderLocSym.LocSlotInfo.HasInit = true;

            EXPR assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                binderLocExpr.TypeSym,
                binderLocExpr,
                callMethExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = binderLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // if-Body (3) Create the CallSite<...>
            //--------------------------------------------------------
            callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, this.runtimeBindAnonTypeSym),
                0);

            binderLocExpr = BindToLocal(null, binderLocSym, BindFlagsEnum.MemberSet);
            DebugUtil.Assert(binderLocExpr != null);

            mwi = new MethWithInst(
                this.callSiteG1CreateMethodSym,
                callSiteGenTypeSym,
                BSYMMGR.EmptyTypeArray);

            EXPR callsiteGenCreateExpr = BindToMethod(
                null,
                null,
                mwi,
                binderLocExpr,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                callSiteGenFieldExpr.TypeSym,
                callSiteGenFieldExpr,
                callsiteGenCreateExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            builder.Add(MakeStmt(null, assignExpr, 0));

            //--------------------------------------------------------
            // if-Label
            //--------------------------------------------------------
            builder.Add(labelExpr);

            //--------------------------------------------------------
            // Create the delegate.
            //--------------------------------------------------------
            LOCVARSYM delegateLocSym = CreateNewLocVarSym(
                CreateAnonLocalName(delegateName),
                delegateAggTypeSym,
                this.currentScopeSym);
            DebugUtil.Assert(delegateLocSym != null);

            // Left

            EXPR delegateLocExpr = BindToLocal(null, delegateLocSym, BindFlagsEnum.MemberSet);

            // Right

            callSiteGenFieldExpr = BindToField(
                null,
                null,
                new FieldWithType(callSiteGenFieldSym, runtimeBindAnonTypeSym),
                BindFlagsEnum.RValueRequired);

            EXPR targetExpr = BindToField(
                null,
                callSiteGenFieldExpr,
                new FieldWithType(callSiteG1TargetFieldSym, callSiteGenTypeSym),
                BindFlagsEnum.RValueRequired);

            // Assign

            NoteReference(delegateLocExpr);
            delegateLocSym.LocSlotInfo.HasInit = true;

            assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                delegateLocExpr.TypeSym,
                delegateLocExpr,
                targetExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = delegateLocSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            //--------------------------------------------------------
            // Invoke the delegate.
            //--------------------------------------------------------
            delegateLocExpr = BindToLocal(null, delegateLocSym, BindFlagsEnum.MemberSet);

            argList = null;
            argListLast = null;

            NewList(callSiteGenFieldExpr, ref argList, ref argListLast);
            NewList(operand1, ref argList, ref argListLast);
            NewList(operand2, ref argList, ref argListLast);

            EXPR invokeExpr = BindToMethod(
                null,
                delegateLocExpr,
                new MethWithInst(delegateInvokeSym, delegateAggTypeSym, BSYMMGR.EmptyTypeArray),
                argList,
                MemLookFlagsEnum.UserCallable);
#if DEBUG
            StringBuilder sb = new StringBuilder();
            DebugUtil.DebugSymsOutput(sb);
            sb.Length = 0;
            DebugUtil.DebugExprsOutput(sb);
#endif
            return invokeExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateCSharpArgumentInfoArray
        //
        /// <summary></summary>
        /// <param name="size"></param>
        /// <returns></returns>
            //------------------------------------------------------------
        private EXPRDECL CreateCSharpArgumentInfoArray(int size)
        {
            //--------------------------------------------------------
            // Create a LOCVARSYM and an EXPRLOCAL of a local variable
            // of CSharpArgumentInfo[] type.
            //--------------------------------------------------------
            LOCVARSYM locSym = CreateNewLocVarSym(
                CreateAnonLocalName("CSharpArgumentInfos"),
                this.csharpArgumentInfoArraySym,
                this.currentScopeSym);
            DebugUtil.Assert(locSym != null);

            EXPRLOCAL locExpr = MakeLocal(null, locSym, true);
            locExpr.Flags |= (EXPRFLAG)BindFlagsEnum.MemberSet;

            EXPRCONSTANT sizeExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)size));
            sizeExpr.Flags |= EXPRFLAG.LITERALCONST;

            EXPR argsExpr = null, argsExprLast = null;
            NewList(sizeExpr, ref argsExpr, ref argsExprLast);

            EXPR newExpr = NewExprBinop(
                null,
                EXPRKIND.NEWARRAY,
                this.csharpArgumentInfoArraySym,
                argsExpr,
                null);

            EXPR assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                locExpr.TypeSym,
                locExpr,
                newExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            NoteReference(locExpr);
            locSym.LocSlotInfo.HasInit = true;

            EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = locSym;
            declExpr.InitialExpr = assignExpr;

            return declExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.SetCSharpArgumentInfoArray
        //
        /// <summary></summary>
        /// <param name="infoArray"></param>
        /// <param name="index"></param>
        /// <param name="flags"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPRSTMTAS SetCSharpArgumentinfoArray(
            LOCVARSYM infoArrayLocSym,
            int index,
            CSharpArgumentInfoFlags flags,
            string name)
        {
            ARRAYSYM arrayTypeSym = infoArrayLocSym.TypeSym as ARRAYSYM;
            if (arrayTypeSym == null)
            {
                return null;
            }

            //--------------------------------------------------------
            // LHS
            //--------------------------------------------------------
            EXPR infoArrayExpr = MakeLocal(null, infoArrayLocSym, true);

            EXPRCONSTANT indexExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit(index));

            EXPR leftExpr = NewExprBinop(
                null,
                EXPRKIND.ARRINDEX,
                arrayTypeSym.ElementTypeSym,
                infoArrayExpr,
                indexExpr);
            leftExpr.Flags |= EXPRFLAG.LVALUE | EXPRFLAG.ASSGOP;

            //--------------------------------------------------------
            // RHS
            //--------------------------------------------------------
            EXPR classExpr = NewExpr(null, EXPRKIND.CLASS, this.csharpArgumentInfoTypeSym);

            EXPR flagsExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit((int)flags));

            EXPR argsExpr = null, argsExprLast = null;

            EXPR argExpr1 = null;
            if (!BindSimpleCast(
                null,
                flagsExpr,
                this.csharpArgumentInfoFlagsTypeSym,
                ref argExpr1,
                0))
            {
                DebugUtil.Assert(false);
            }

            NewList(argExpr1, ref argsExpr, ref argsExprLast);

            EXPR argExpr2 = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.STRING),
                new ConstValInit((string)name));

            NewList(argExpr2, ref argsExpr, ref argsExprLast);

            MethWithInst mwi = new MethWithInst(
                this.csharpArgumentInfoCreateMethodSym,
                this.csharpArgumentInfoTypeSym,
                BSYMMGR.EmptyTypeArray);

            EXPR createExpr = BindToMethod(
                null,
                null,
                mwi,
                argsExpr,
                (MemLookFlagsEnum)EXPRFLAG.USERCALLABLE);

            //--------------------------------------------------------
            // Assign
            //--------------------------------------------------------
            EXPR assignExpr = BindAssignment(null, leftExpr, createExpr, false);

            return MakeStmt(null, assignExpr, 0);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateSystemTypeArray
        //
        /// <summary></summary>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private LOCVARSYM CreateSystemTypeArray(
            TypeArray typeArgs,
            StatementListBuilder builder)
        {
            //--------------------------------------------------------
            // Create a LOCVARSYM and an EXPRLOCAL of a local variable
            // of System.Type[] type.
            //--------------------------------------------------------
            LOCVARSYM locSym = CreateNewLocVarSym(
                CreateAnonLocalName("Types"),
                this.systemTypeArraySym,
                this.currentScopeSym);
            DebugUtil.Assert(locSym != null);

            EXPRLOCAL locExpr = MakeLocal(null, locSym, true);
            locExpr.Flags |= (EXPRFLAG)BindFlagsEnum.MemberSet;

            EXPRCONSTANT sizeExpr = NewExprConstant(
                null,
                GetRequiredPredefinedType(PREDEFTYPE.INT),
                new ConstValInit(typeArgs.Count));
            sizeExpr.Flags |= EXPRFLAG.LITERALCONST;

            EXPR argsExpr = null, argsExprLast = null;
            NewList(sizeExpr, ref argsExpr, ref argsExprLast);

            EXPR newExpr = NewExprBinop(
                null,
                EXPRKIND.NEWARRAY,
                this.systemTypeArraySym,
                argsExpr,
                null);

            EXPR assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                locExpr.TypeSym,
                locExpr,
                newExpr);
            assignExpr.Flags |= EXPRFLAG.ASSGOP;

            NoteReference(locExpr);
            locSym.LocSlotInfo.HasInit = true;

            EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
            declExpr.LocVarSym = locSym;
            declExpr.InitialExpr = assignExpr;

            builder.Add(declExpr);

            for (int i = 0; i < typeArgs.Count; ++i)
            {
                // LHS
                EXPR typeArrayExpr = MakeLocal(null, locSym, true);

                EXPR indexExpr = NewExprConstant(
                    null,
                    GetRequiredPredefinedType(PREDEFTYPE.INT),
                    new ConstValInit(i));

                EXPR arrElemExpr = NewExprBinop(
                    null,
                    EXPRKIND.ARRINDEX,
                    systemTypeAggTypeSym,
                    typeArrayExpr,
                    indexExpr);
                arrElemExpr.Flags |= (EXPRFLAG.LVALUE | EXPRFLAG.ASSGOP);

                // RHS
                EXPRTYPEOF typeOfExpr = BindTypeOf(typeArgs[i]);

                // assign
                EXPR asgExpr = BindAssignment(null, arrElemExpr, typeOfExpr, false);

                builder.Add(MakeStmt(null, asgExpr, 0));
            }

            return locSym;
        }
    }
}

