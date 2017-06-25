//============================================================================
// AnonymousTypes.cs
//
// 2016/01/02 (hirano567@hotmail.co.jp)
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
    abstract internal partial class CParser
    {
        //------------------------------------------------------------
        // CParser.ParseAnonymousObjectInitializer
        //
        /// <summary></summary>
        /// <param name="newNode"></param>
        //------------------------------------------------------------
        internal void ParseAnonymousObjectInitializer(NEWNODE newNode)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.OPENCURLY);
            NextToken();

            DECLSTMTNODE stmtNode = AllocNode(NODEKIND.DECLSTMT, newNode) as DECLSTMTNODE;

            CListMaker list = new CListMaker(this);
            int comma = -1;
            VARDECLNODE vdNode = null;

            while (
                CurrentTokenID() != TOKENID.CLOSECURLY &&
                CurrentTokenID() != TOKENID.ENDFILE)
            {
                if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                    PeekToken(1) == TOKENID.EQUAL)
                {
                    vdNode = ParseVariableDeclarator(
                            stmtNode,
                            stmtNode,
                            (uint)PARSEDECLFLAGS.LOCAL,
                            false,  // isFirst,
                            -1) as VARDECLNODE;

                    if (vdNode == null ||
                        vdNode.NameNode == null ||
                        String.IsNullOrEmpty(vdNode.NameNode.Name))
                    {
                        continue;
                    }
                }
                else
                {
                    BASENODE argNode = ParseTerm(stmtNode, -1);
                    vdNode = AllocNode(NODEKIND.VARDECL, stmtNode) as VARDECLNODE;
                    vdNode.DeclarationsNode = stmtNode;

                    NAMENODE nameNode = null;
                    string name = null;
                    int idx = -1;

                    switch (argNode.Kind)
                    {
                        case NODEKIND.NAME:
                            nameNode = argNode as NAMENODE;
                            name = nameNode.Name;
                            idx = nameNode.TokenIndex;
                            break;

                        case NODEKIND.DOT:
                            BINOPNODE bopNode = argNode as BINOPNODE;
                            if (bopNode.Operand2 != null &&
                                bopNode.Operand2.Kind == NODEKIND.NAME)
                            {
                                nameNode = bopNode.Operand2 as NAMENODE;
                                name = (bopNode.Operand2 as NAMENODE).Name;
                                idx = nameNode.TokenIndex;
                            }
                            break;

                        default:
                            break;
                    }

                    if (name == null)
                    {
                        Error(CSCERRID.ERR_InvalidAnonTypeMemberDeclarator);
                        goto NEXT_DECLARATOR;
                    }

                    // for NameNode
                    NAMENODE nameNode1 = AllocNode(NODEKIND.NAME, null, idx) as NAMENODE;
                    nameNode1.Name = name;
                    nameNode1.PossibleGenericName = null;
                    nameNode1.ParentNode = vdNode;

                    // for ArgumentsNode
                    NAMENODE nameNode2 = AllocNode(NODEKIND.NAME, null, idx) as NAMENODE;
                    nameNode2.Name = name;
                    nameNode2.PossibleGenericName = null;

                    // assign
                    BINOPNODE asgNode = AllocNode(NODEKIND.BINOP, vdNode, idx) as BINOPNODE;
                    asgNode.Operand1 = nameNode2;
                    asgNode.Operand2 = argNode;
                    asgNode.Operator = OPERATOR.ASSIGN;

                    nameNode2.ParentNode = asgNode;
                    argNode.ParentNode = asgNode;

                    vdNode.NameNode = nameNode1;
                    vdNode.ArgumentsNode = asgNode;
                }

                list.Add(vdNode, comma);

            NEXT_DECLARATOR:
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
            }

            if (CurrentTokenID() != TOKENID.CLOSECURLY)
            {
                Error(CSCERRID.ERR_RbraceExpected);
                newNode.Flags |= NODEFLAGS.CALL_HADERROR;
            }
            NextToken();

            stmtNode.VariablesNode = list.GetList(stmtNode);
            newNode.InitialNode = stmtNode;
            newNode.Flags |= NODEFLAGS.NEW_ANONYMOUS_OBJECT_CREATION;
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC.CreateAnonymousTypeFieldName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAnonymousTypeFieldName(string name)
        {
            return String.Format("<{0}>i__Field", name);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAnonymousParameterName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAnonymousParameterName(string name)
        {
            return String.Format("<{0}>j__Param", name);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAnonymousTypeParameterName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAnonymousTypeParameterName(int index)
        {
            return String.Format("<{0}>k__TypeParam", index);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAnonymousLocalName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAnonymousLocalName(string name)
        {
            return String.Format("<{0}>l__Local", name);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAnonymousTypeName
        //
        /// <summary></summary>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAnonymousTypeName(List<string> nameList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nameList.Count; ++i)
            {
                if (i == 0)
                {
                    sb.AppendFormat("{0}", nameList[i]);
                }
                else
                {
                    sb.AppendFormat(",{0}", nameList[i]);
                }
            }
            return String.Format("<{0}>f__AnonymousType", sb.ToString());
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAnonymousType
        //
        /// <summary></summary>
        /// <param name="newNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM CreateAnonymousType(NEWNODE newNode)
        {
            DebugUtil.Assert(
                newNode != null &&
                newNode.IsAnonymousObjectCreation &&
                newNode.InitialNode != null);

            //--------------------------------------------------------
            // Get the information of the type variables.
            //--------------------------------------------------------
            List<string> nameList = new List<string>();
            CListMaker argList = new CListMaker(
                this.methodSym.GetInputFile().SourceData.SourceModule.Parser);

            DECLSTMTNODE declNode = newNode.InitialNode as DECLSTMTNODE;
            BASENODE node = (declNode != null ? declNode.VariablesNode : null);

            while (node != null)
            {
                VARDECLNODE varDecl;
                if (node.Kind == NODEKIND.LIST)
                {
                    varDecl = node.AsLIST.Operand1 as VARDECLNODE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    varDecl = node as VARDECLNODE;
                    node = null;
                }

                BINOPNODE assignNode = varDecl.ArgumentsNode as BINOPNODE;
                if (assignNode == null ||
                    assignNode.Operator != OPERATOR.ASSIGN ||
                    assignNode.Operand1 == null ||
                    assignNode.Operand1.Kind != NODEKIND.NAME ||
                    assignNode.Operand2 == null)
                {
                    continue;
                }

                nameList.Add((assignNode.Operand1 as NAMENODE).Name);
                argList.Add(assignNode.Operand2, -1);
            }
            newNode.ArgumentsNode = argList.GetList(newNode);

            return CreateAnonymousTypeCore(nameList);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAggSym
        //
        /// <summary></summary>
        /// <param name="aggKind"></param>
        /// <param name="outerDeclSym"></param>
        /// <param name="name"></param>
        /// <param name="access"></param>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM CreateAggSym(
            AggKindEnum aggKind,
            DECLSYM outerDeclSym,
            string name,
            ACCESS access,
            int arity)
        {
            if (String.IsNullOrEmpty(name) ||
                outerDeclSym == null)
            {
                DebugUtil.Assert(false);
                return null;
            }

            AGGSYM aggSym = null;
            BAGSYM outerBagSym = outerDeclSym.BagSym;

            //--------------------------------------------------------
            // Search the AGGSYM in the outerBagSym.
            //--------------------------------------------------------
            aggSym = Compiler.LookupInBagAid(
                name,
                outerBagSym,
                arity,
                Kaid.ThisAssembly,
                SYMBMASK.AGGSYM) as AGGSYM;

            if (aggSym != null)
            {
                return aggSym;
            }

            //--------------------------------------------------------
            // Create the AGGSYM.
            //--------------------------------------------------------
            aggSym = Compiler.MainSymbolManager.CreateAgg(name, outerDeclSym);
            AGGDECLSYM aggDeclSym
                = Compiler.MainSymbolManager.CreateAggDecl(aggSym, outerDeclSym);

            aggSym.IsArityInName = false;
            aggSym.HasParseTree = false;
            aggDeclSym.ParseTreeNode = null;
            aggSym.AggKind = AggKindEnum.Class;
            aggSym.Access = access;
            aggSym.IsFabricated = true;
            aggSym.Interfaces = BSYMMGR.EmptyTypeArray;

            return aggSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateFieldSym
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="access"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBVARSYM CreateFieldSym(
            string name,
            AGGDECLSYM aggDeclSym,
            ACCESS access,
            TYPESYM typeSym)
        {

            MEMBVARSYM fieldSym = null;

            fieldSym = Compiler.MainSymbolManager.LookupAggMember(
                name,
                aggDeclSym.AggSym,
                SYMBMASK.MEMBVARSYM) as MEMBVARSYM;

            if (fieldSym != null)
            {
                return fieldSym;
            }

            fieldSym = Compiler.MainSymbolManager.CreateMembVar(
                name,
                aggDeclSym.AggSym,
                aggDeclSym);

            fieldSym.Access = access;
            fieldSym.IsReferenced = true;
            fieldSym.IsAssigned = true;
            fieldSym.IsFabricated = true;
            fieldSym.TypeSym = typeSym;

            return fieldSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAnonymousTypeCore
        //
        /// <summary></summary>
        /// <param name="nameList">names of the type variables.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM CreateAnonymousTypeCore(List<string> nameList)
        {
            DECLSYM outerDeclSym = this.methodSym.ContainingAggDeclSym.ParentDeclSym;
            BAGSYM outerBagSym = outerDeclSym.BagSym;
            AGGDECLSYM outerAggDeclSym = outerDeclSym as AGGDECLSYM;

            AGGSYM outerAggSym = null;
            if (outerAggDeclSym != null)
            {
                outerAggSym = outerAggDeclSym.AggSym;
            }

            TypeArray outerTypeVariables = null;
            if (outerAggSym != null)
            {
                outerTypeVariables = outerAggSym.AllTypeVariables;
            }
            else
            {
                outerTypeVariables = BSYMMGR.EmptyTypeArray;
            }
            int outerTvCount = outerTypeVariables.Count;

            TypeArray typeArray = new TypeArray();
            int tvCount = 0;

            List<MEMBVARSYM> fieldList = new List<MEMBVARSYM>();
            List<PROPSYM> propertyList = new List<PROPSYM>();
            List<METHSYM> accList = new List<METHSYM>();

            tvCount = nameList.Count;

            //--------------------------------------------------------
            // Search the anonymous type.
            //--------------------------------------------------------
            AGGSYM aggSym = null;
            string typeName = CreateAnonymousTypeName(nameList);
            AGGTYPESYM objAggTypeSym = Compiler.GetOptPredefType(PREDEFTYPE.OBJECT, true);

            aggSym = Compiler.LookupInBagAid(
                typeName,
                outerBagSym,
                tvCount,
                Kaid.ThisAssembly,
                SYMBMASK.AGGSYM) as AGGSYM;

            if (aggSym != null)
            {
                return aggSym;
            }

            //--------------------------------------------------------
            // Declare the anonymous type.
            //--------------------------------------------------------
            aggSym = Compiler.MainSymbolManager.CreateAgg(typeName, outerDeclSym);
            AGGDECLSYM aggDeclSym = Compiler.MainSymbolManager.CreateAggDecl(aggSym, outerDeclSym);

            aggSym.IsArityInName = false;
            aggSym.HasParseTree = false;
            aggDeclSym.ParseTreeNode = null;
            aggSym.AggKind = AggKindEnum.Class;
            aggSym.IsSealed = true;
            aggSym.Access = ACCESS.INTERNAL;
            aggSym.IsFabricated = true;
            aggSym.Interfaces = BSYMMGR.EmptyTypeArray;

            // type variables

            for (int i = 0; i < tvCount; ++i)
            {
                int tIndex = outerTvCount + i;

                TYVARSYM tvSym = Compiler.MainSymbolManager.CreateTyVar(
                    CreateAnonymousTypeParameterName(tIndex),
                    aggSym);

                tvSym.Access = ACCESS.PRIVATE;
                tvSym.Index = i;
                tvSym.TotalIndex = tIndex;
                tvSym.ParseTreeNode = null;
                tvSym.SetBaseTypes(objAggTypeSym, objAggTypeSym);
                tvSym.AllInterfaces = BSYMMGR.EmptyTypeArray;
                tvSym.AggState = AggStateEnum.Last;

                typeArray.Add(tvSym);
            }

            if (outerTvCount == 0)
            {
                aggSym.TypeVariables = Compiler.MainSymbolManager.AllocParams(typeArray);
                aggSym.AllTypeVariables = aggSym.TypeVariables;
            }
            else
            {
                aggSym.TypeVariables = Compiler.MainSymbolManager.AllocParams(typeArray);
                TypeArray tempArray = new TypeArray();
                tempArray.Add(outerTypeVariables);
                tempArray.Add(typeArray);
                aggSym.TypeVariables = Compiler.MainSymbolManager.AllocParams(tempArray);
            }

            aggSym.AggState = AggStateEnum.Declared; //AggStateEnum.Bounds;

            AGGTYPESYM baseAggTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true);
            AGGSYM baseAggSym = baseAggTypeSym.GetAggregate();
            Compiler.SetBaseType(aggSym, baseAggTypeSym);

            //--------------------------------------------------------
            // Define the fields, the properties and the accessors.
            //--------------------------------------------------------
            for (int i = 0; i < nameList.Count; ++i)
            {
                TYPESYM fldTypeSym = typeArray[i];

                //----------------------------------------------------
                // field
                //----------------------------------------------------
                MEMBVARSYM fldSym = Compiler.MainSymbolManager.CreateMembVar(
                    CreateAnonymousTypeFieldName(nameList[i]),
                    aggSym,
                    aggDeclSym);

                fldSym.Access = ACCESS.PRIVATE;
                fldSym.IsReferenced = true;
                fldSym.IsAssigned = true;
                fldSym.IsFabricated = true;
                fldSym.TypeSym = fldTypeSym;

                fieldList.Add(fldSym);

                //----------------------------------------------------
                // property
                //----------------------------------------------------
                PROPSYM propSym = Compiler.MainSymbolManager.CreateProperty(
                    nameList[i],
                    aggSym,
                    aggDeclSym);

                propSym.Access = ACCESS.INTERNAL;
                propSym.ReturnTypeSym = fldTypeSym;
                propSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
                propSym.BackFieldSym = fldSym;

                propertyList.Add(propSym);

                //----------------------------------------------------
                // accessor
                //----------------------------------------------------
                string accName = Compiler.ClsDeclRec.CreateAccessorName(
                    nameList[i],
                    "get_");

                METHSYM getMethSym = Compiler.MainSymbolManager.CreateMethod(
                    accName,
                    aggSym,
                    aggDeclSym);

                getMethSym.Access = ACCESS.INTERNAL;
                getMethSym.MethodKind = MethodKindEnum.PropAccessor;
                getMethSym.PropertySym = propSym;
                getMethSym.ReturnTypeSym = fldTypeSym;
                getMethSym.IsFabricated = true;
                getMethSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
                getMethSym.TypeVariables = BSYMMGR.EmptyTypeArray;

                propSym.GetMethodSym = getMethSym;
                accList.Add(getMethSym);
            }

            //--------------------------------------------------------
            // Define the constructor.
            //--------------------------------------------------------
            METHSYM ctorSym = Compiler.MainSymbolManager.CreateMethod(
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR),
                aggSym,
                aggDeclSym);
            ctorSym.MethodKind = MethodKindEnum.Ctor;
            ctorSym.Access = ACCESS.INTERNAL;
            ctorSym.ReturnTypeSym = Compiler.MainSymbolManager.VoidSym;
            ctorSym.ParameterTypes = typeArray;
            ctorSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            ctorSym.IsFabricated = true;

            //aggSym.AggState = AggStateEnum.DefinedMembers;

            SymWithType hiddenSwt = null;
            SymWithType ambigSwt = null;
            bool needMethImpl = false;

            //--------------------------------------------------------
            // Define Equals method.
            //--------------------------------------------------------
            METHSYM equalsSym = Compiler.MainSymbolManager.CreateMethod(
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.EQUALS),
                aggSym,
                aggDeclSym);
            equalsSym.Access = ACCESS.PUBLIC;
            equalsSym.ReturnTypeSym = GetRequiredPredefinedType(PREDEFTYPE.BOOL);

            TypeArray equalsParamArray = new TypeArray();
            equalsParamArray.Add(GetRequiredPredefinedType(PREDEFTYPE.OBJECT));
            equalsParamArray = Compiler.MainSymbolManager.AllocParams(equalsParamArray);
            equalsSym.ParameterTypes = equalsParamArray;

            equalsSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            equalsSym.IsFabricated = true;

            equalsSym.IsOverride = true;
            equalsSym.IsVirtual = true;
            equalsSym.IsMetadataVirtual = true;

            hiddenSwt = new SymWithType();
            ambigSwt = new SymWithType();
            needMethImpl = false;

            if (Compiler.ClsDeclRec.FindSymHiddenByMethPropAgg(
                    equalsSym,
                    aggSym.BaseClassSym,
                    aggSym,
                    hiddenSwt,
                    ambigSwt,
                    ref needMethImpl))
            {
                equalsSym.SlotSymWithType = hiddenSwt;
                equalsSym.CModifierCount = hiddenSwt.MethSym.CModifierCount;
            }

            //--------------------------------------------------------
            // Define GetHashCode Method.
            //--------------------------------------------------------
            String getHashCodName = Compiler.NameManager.GetPredefinedName(PREDEFNAME.GETHASHCODE);
            METHSYM getHashCodeSym = Compiler.MainSymbolManager.CreateMethod(
                getHashCodName,
                aggSym,
                aggDeclSym);
            getHashCodeSym.Access = ACCESS.PUBLIC;
            getHashCodeSym.ReturnTypeSym = Compiler.GetOptPredefType(PREDEFTYPE.INT, true);
            getHashCodeSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            getHashCodeSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            getHashCodeSym.IsFabricated = true;

            getHashCodeSym.IsOverride = true;
            getHashCodeSym.IsVirtual = true;
            getHashCodeSym.IsMetadataVirtual = true;

            hiddenSwt = new SymWithType();
            ambigSwt = new SymWithType();
            needMethImpl = false;

            if (Compiler.ClsDeclRec.FindSymHiddenByMethPropAgg(
                    getHashCodeSym,
                    aggSym.BaseClassSym,
                    aggSym,
                    hiddenSwt,
                    ambigSwt,
                    ref needMethImpl))
            {
                getHashCodeSym.SlotSymWithType = hiddenSwt;
                getHashCodeSym.CModifierCount = hiddenSwt.MethSym.CModifierCount;
            }

            //--------------------------------------------------------
            // Define ToString Method.
            //--------------------------------------------------------
            String toStringName = Compiler.NameManager.GetPredefinedName(PREDEFNAME.TOSTRING);
            METHSYM toStringSym = Compiler.MainSymbolManager.CreateMethod(
                toStringName,
                aggSym,
                aggDeclSym);
            toStringSym.Access = ACCESS.PUBLIC;
            toStringSym.ReturnTypeSym = Compiler.GetOptPredefType(PREDEFTYPE.STRING, true);
            toStringSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            toStringSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            toStringSym.IsFabricated = true;

            toStringSym.IsOverride = true;
            toStringSym.IsVirtual = true;
            toStringSym.IsMetadataVirtual = true;

            hiddenSwt = new SymWithType();
            ambigSwt = new SymWithType();
            needMethImpl = false;

            if (Compiler.ClsDeclRec.FindSymHiddenByMethPropAgg(
                    toStringSym,
                    aggSym.BaseClassSym,
                    aggSym,
                    hiddenSwt,
                    ambigSwt,
                    ref needMethImpl))
            {
                toStringSym.SlotSymWithType = hiddenSwt;
                toStringSym.CModifierCount = hiddenSwt.MethSym.CModifierCount;
            }

            //--------------------------------------------------------
            // Emit the type and members.
            //--------------------------------------------------------
            aggSym.AggState = AggStateEnum.PreparedMembers;

            Compiler.ClsDeclRec.EmitTypedefsAggregate(aggSym);
            Compiler.ClsDeclRec.EmitBasesAggregate(aggSym);
            Compiler.ClsDeclRec.EmitMemberdefsAggregate(aggSym);

            AGGTYPESYM aggTypeSym = aggSym.GetThisType();

#if DEBUG
            StringBuilder sbDebug = null;
#endif

            //--------------------------------------------------------
            // Compile the accessors.
            //--------------------------------------------------------
            METHINFO methInfo = null;
            AGGINFO aggInfo = new AGGINFO();

            FUNCBREC funcbrecBackup = new FUNCBREC(this.Compiler);
            this.Store(funcbrecBackup);

            try
            {
                foreach (METHSYM methSym in accList)
                {
                    methInfo = new METHINFO();
                    methInfo.MethodSym = methSym;
                    InitMethod2(methInfo, aggInfo);

                    EXPR expr = CreateAutoImplementedGetAccessor(methSym);
                    Compiler.IlGenRec.Compile(methSym, methInfo, expr);
                }
            }
            finally
            {
                this.Restore(funcbrecBackup);
            }

            //--------------------------------------------------------
            // Compile the constructor.
            //--------------------------------------------------------
            methInfo = new METHINFO();
            methInfo.MethodSym = ctorSym;
            PARAMINFO pInfo = null;

            for (int i = 0; i < ctorSym.ParameterTypes.Count; ++i)
            {
                pInfo = new PARAMINFO();
                pInfo.Name = CreateAnonymousParameterName(nameList[i]);
                methInfo.ParameterInfos.Add(pInfo);
            }

            try
            {
                InitMethod2(methInfo, aggInfo);

                CreateNewScope();
                SCOPESYM scopeSym = this.currentScopeSym;
                this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

                this.currentBlockExpr = NewExprBlock(treeNode);
                this.currentBlockExpr.ScopeSym = this.currentScopeSym;

                //----------------------------------------------------
                // Declare the parameters
                //----------------------------------------------------
                List<LOCVARSYM> paramList = new List<LOCVARSYM>();
                EXPR firstParam = null, lastParam = null;

                for (int i = 0; i < methInfo.ParameterInfos.Count; ++i)
                {
                    TYPESYM ptSym = ctorSym.ParameterTypes[i];
                    LOCVARSYM paramLocSym = DeclareParam(
                        methInfo.ParameterInfos[i].Name,
                        ptSym,
                        0,
                        null,
                        null);
                    paramList.Add(paramLocSym);
                    NewList(MakeLocal(null, paramLocSym, true), ref firstParam, ref lastParam);
                }

                //----------------------------------------------------
                // Call the base constructor.
                //----------------------------------------------------
                StatementListBuilder listBuilder = new StatementListBuilder();
                EXPR thisExpr = BindThisImplicit(null);

                EXPR callBaseCtorExpr = CreateConstructorCall(
                    null,
                    null,
                    baseAggTypeSym,
                    thisExpr,
                    null,
                    MemLookFlagsEnum.BaseCall);

                listBuilder.Add(MakeStmt(null, callBaseCtorExpr, 0));

                //----------------------------------------------------
                // Assign the values of the parameters to the properties.
                //----------------------------------------------------
                for (int i = 0; i < ctorSym.ParameterTypes.Count; ++i)
                {
                    // Bind the field.

                    FieldWithType fwt = new FieldWithType();
                    fwt.Set(fieldList[i], aggTypeSym);
                    EXPR fieldExpr = BindToField(
                        null,
                        BindThisImplicit(null),
                        fwt,
                        BindFlagsEnum.RValueRequired);

                    // Bind the parameter.

                    SymWithType paramSwt = new SymWithType();
                    paramSwt.Set(paramList[i], null);
                    EXPR paramExpr = BindToLocal(null, paramList[i], BindFlagsEnum.RValueRequired);

                    // assign

                    EXPR assignExpr = NewExprBinop(
                        null,
                        EXPRKIND.ASSG,
                        fieldExpr.TypeSym,
                        fieldExpr,
                        paramExpr);

                    // Add to the builder

                    EXPRSTMTAS stmt = NewExpr(null, EXPRKIND.STMTAS, null) as EXPRSTMTAS;
                    stmt.Expr = assignExpr;

                    listBuilder.Add(stmt);
                }

                EXPRRETURN retExpr = NewExpr(null, EXPRKIND.RETURN, null) as EXPRRETURN;
                listBuilder.Add(retExpr);

                this.currentBlockExpr.StatementsExpr = listBuilder.GetList();
                EXPRBLOCK blockExpr = this.currentBlockExpr;
                this.currentBlockExpr = blockExpr.OwingBlockExpr;

                CloseScope();
                CorrectAnonMethScope(blockExpr.ScopeSym);

                Compiler.IlGenRec.Compile(ctorSym, methInfo, blockExpr);
            }
            finally
            {
                this.Restore(funcbrecBackup);
            }

            //--------------------------------------------------------
            // Compile Equals Method.
            //--------------------------------------------------------
            methInfo = new METHINFO();
            methInfo.MethodSym = toStringSym;
            StatementListBuilder builder = new StatementListBuilder();

            string paramName = CreateAnonymousParameterName("arg0");
            pInfo = new PARAMINFO();
            pInfo.Name = paramName;
            methInfo.ParameterInfos.Add(pInfo);

            try
            {
                InitMethod2(methInfo, aggInfo);

                CreateNewScope();
                SCOPESYM scopeSym = this.currentScopeSym;
                this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

                this.currentBlockExpr = NewExprBlock(treeNode);
                this.currentBlockExpr.ScopeSym = this.currentScopeSym;

                //----------------------------------------------------
                // Declare the parameter
                //----------------------------------------------------
                List<LOCVARSYM> paramList = new List<LOCVARSYM>();

                TYPESYM paramTypeSym = GetRequiredPredefinedType(PREDEFTYPE.OBJECT);
                LOCVARSYM paramSym = DeclareParam(
                    paramName,
                    paramTypeSym,
                    0,
                    null,
                    null);
                paramList.Add(paramSym);
                EXPR paramExpr = MakeLocal(null, paramSym, true);

                //----------------------------------------------------
                // (1) convert the parameter
                //----------------------------------------------------
                string otherName = CreateSpecialName(SpecialNameKindEnum.DisplayClassInstance, null);

                LOCVARSYM otherSym = DeclareVar(null, otherName, aggTypeSym, false);
                this.unreferencedVarCount++;

                otherSym.LocSlotInfo.SetJbitDefAssg(this.uninitedVarCount + 1);
                int cbit = FlowChecker.GetCbit(Compiler, otherSym.TypeSym);
                this.uninitedVarCount += cbit;

                EXPR leftExpr1 = BindToLocal(null, otherSym, BindFlagsEnum.MemberSet);

                EXPRTYPEOF typeofExpr = NewExpr(
                    null,
                    EXPRKIND.TYPEOF,
                    GetRequiredPredefinedType(PREDEFTYPE.TYPE)) as EXPRTYPEOF;
                DebugUtil.Assert(typeofExpr != null);
                typeofExpr.SourceTypeSym = aggTypeSym;
                typeofExpr.MethodSym = null;

                EXPR rightExpr1 = NewExprBinop(null, EXPRKIND.AS, aggTypeSym, paramExpr, typeofExpr);

                EXPRDECL declExpr = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
                declExpr.LocVarSym = otherSym;
                declExpr.InitialExpr = BindAssignment(null, leftExpr1, rightExpr1, false);

                builder.Add(declExpr);

                //----------------------------------------------------
                // (2-1) check whether converted or not converted.
                //----------------------------------------------------
                EXPRLABEL labelExpr2 = NewExprLabel();

                EXPR leftExpr2 = BindToLocal(null, otherSym, BindFlagsEnum.RValueRequired);
                EXPR rightExpr2 = NewExprConstant(null, Compiler.MainSymbolManager.NullSym, new ConstValInit());

                bool isUserDef = false;
                NubInfo nin = null;
                EXPR conditionExpr2 = BindStdBinOp(null, EXPRKIND.NE, leftExpr2, rightExpr2, ref isUserDef, ref nin);

                TYPESYM boolTypeSym = GetRequiredPredefinedType(PREDEFTYPE.BOOL);
                EXPR boolExpr2 = TryConvert(conditionExpr2, boolTypeSym, 0);

                EXPRGOTOIF gotoIfExpr2 = NewExpr(null, EXPRKIND.GOTOIF, null) as EXPRGOTOIF;
                gotoIfExpr2.LabelExpr = labelExpr2;
                gotoIfExpr2.ConditionExpr = conditionExpr2;
                gotoIfExpr2.Flags = 0;

                builder.Add(gotoIfExpr2);

                //----------------------------------------------------
                // (3) Compare each property.
                //----------------------------------------------------
                CreateNewScope();
                scopeSym = this.currentScopeSym;
                this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

                this.currentBlockExpr = NewExprBlock(treeNode);
                this.currentBlockExpr.ScopeSym = this.currentScopeSym;

                AGGSYM eqCompSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G_EQUALITYCOMPARER, true);
                DebugUtil.Assert(eqCompSym != null);

                try
                {
                    foreach (PROPSYM prSym in propertyList)
                    {
                        //--------------------------------------------
                        // (3-2) if-condition
                        //--------------------------------------------
                        TYPESYM ptypeSym = prSym.ReturnTypeSym;
                        TypeArray typeArgs = new TypeArray();
                        typeArgs.Add(ptypeSym);
                        typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

                        AGGTYPESYM eqCompAts = Compiler.MainSymbolManager.GetInstAgg(
                            eqCompSym,
                            typeArgs);
                        EXPR eqCompExpr = NewExpr(null, EXPRKIND.CLASS, eqCompAts);

                        // EqualityComparer<T>.Default

                        MemberLookup mem = new MemberLookup();

                        if (!mem.Lookup(
                                Compiler,
                                eqCompAts,
                                eqCompExpr,
                                this.parentDeclSym,
                                "Default",
                                0,
                                MemLookFlagsEnum.UserCallable))
                        {
                            DebugUtil.Assert(false);
                        }

                        EXPR ecDefaultExpr = BindToProperty(
                            null,
                            null,
                            PropWithType.Convert(mem.FirstSymWithType),
                            BindFlagsEnum.RValueRequired,
                            null,
                            null);

                        // EqualityComparer<T>.Default.Equals
                        mem = new MemberLookup();
                        string ecEqualsName = "Equals";

                        if (!mem.Lookup(
                                Compiler,
                                eqCompAts,
                                ecDefaultExpr,
                                this.parentDeclSym,
                                ecEqualsName,
                                0,
                                MemLookFlagsEnum.UserCallable))
                        {
                            DebugUtil.Assert(false);
                        }

                        EXPRMEMGRP groupExpr = NewExpr(
                            null,
                            EXPRKIND.MEMGRP,
                            Compiler.MainSymbolManager.MethodGroupTypeSym) as EXPRMEMGRP;
                        groupExpr.Name = ecEqualsName;
                        groupExpr.SymKind = SYMKIND.METHSYM;
                        groupExpr.TypeArguments = BSYMMGR.EmptyTypeArray;
                        groupExpr.ParentTypeSym = eqCompAts;
                        groupExpr.MethPropSym = null;
                        groupExpr.ObjectExpr = ecDefaultExpr;
                        groupExpr.ContainingTypeArray = mem.GetAllTypes();
                        groupExpr.Flags |= EXPRFLAG.USERCALLABLE;

                        // argument 1

                        EXPR thisObjectExpr = BindThisImplicit(null);

                        EXPR argExpr1 = BindToProperty(
                            null,
                            thisObjectExpr,
                            new PropWithType(prSym, aggTypeSym),
                            BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments,
                            null,
                            null);

                        // argument 2

                        EXPR argLeftExpr2 = BindToLocal(
                            null,
                            otherSym,
                            BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments);

                        mem = new MemberLookup();
                        if (!mem.Lookup(
                                Compiler,
                                aggTypeSym,
                                argLeftExpr2,
                                this.parentDeclSym,
                                prSym.Name,
                                0,
                                MemLookFlagsEnum.UserCallable))
                        {
                            DebugUtil.Assert(false);
                        }

                        EXPR argExpr2 = BindToProperty(
                            null,
                            argLeftExpr2,
                            PropWithType.Convert(mem.FirstSymWithType),
                            BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments,
                            null,
                            null);

                        EXPR argsExpr = null;
                        EXPR argsExprLast = null;

                        NewList(argExpr1, ref argsExpr, ref argsExprLast);
                        NewList(argExpr2, ref argsExpr, ref argsExprLast);

                        EXPR ecCallExpr = BindGrpToArgs(
                            null,
                            BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments,
                            groupExpr,
                            argsExpr);

                        EXPR conditionExpr3 = BindStdUnaOp(null, OPERATOR.LOGNOT, ecCallExpr);
                        EXPR boolExpr3 = TryConvert(conditionExpr3, boolTypeSym, 0);

                        EXPRLABEL labelExpr3 = NewExprLabel();
                        EXPRGOTOIF gotoifExpr3 = MakeGotoIf(null, boolExpr3, labelExpr3, false, 0);
                        builder.Add(gotoifExpr3);

                        //--------------------------------------------
                        // (3-2) if-statement
                        //--------------------------------------------
                        CreateNewScope();
                        scopeSym = this.currentScopeSym;
                        this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

                        this.currentBlockExpr = NewExprBlock(treeNode);
                        this.currentBlockExpr.ScopeSym = this.currentScopeSym;

                        try
                        {
                            EXPR rvExpr32 = NewExprConstant(
                                null,
                                GetRequiredPredefinedType(PREDEFTYPE.BOOL),
                                new ConstValInit(false));
                            rvExpr32.Flags |= EXPRFLAG.LITERALCONST;
                            EXPRRETURN retExpr32 = NewExpr(null, EXPRKIND.RETURN, null) as EXPRRETURN;
                            retExpr32.ObjectExpr = rvExpr32;

                            builder.Add(retExpr32);
                        }
                        finally
                        {
                            EXPRBLOCK rval = this.currentBlockExpr;
                            this.currentBlockExpr = rval.OwingBlockExpr;

                            CloseScope();
                        }

                        builder.Add(labelExpr3);
                    }

                    //------------------------------------------------
                    // (3-3) If all the values of the properties are equal, return true.
                    //------------------------------------------------
                    EXPR rvExpr33 = NewExprConstant(
                        null,
                        GetRequiredPredefinedType(PREDEFTYPE.BOOL),
                        new ConstValInit(true));
                    rvExpr33.Flags |= EXPRFLAG.LITERALCONST;
                    EXPRRETURN retExpr33 = NewExpr(null, EXPRKIND.RETURN, null) as EXPRRETURN;
                    retExpr33.ObjectExpr = rvExpr33;

                    builder.Add(retExpr33);
                }
                finally
                {
                    EXPRBLOCK rval = this.currentBlockExpr;
                    this.currentBlockExpr = rval.OwingBlockExpr;

                    CloseScope();
                }

                //----------------------------------------------------
                // (2-2) If not converted, return false
                //----------------------------------------------------
                builder.Add(labelExpr2);

                EXPR rvExpr22 = NewExprConstant(
                    null,
                    GetRequiredPredefinedType(PREDEFTYPE.BOOL),
                    new ConstValInit(false));
                rvExpr22.Flags |= EXPRFLAG.LITERALCONST;
                EXPRRETURN retExpr22 = NewExpr(null, EXPRKIND.RETURN, null) as EXPRRETURN;
                retExpr22.ObjectExpr = rvExpr22;

                builder.Add(retExpr22);

                //----------------------------------------------------
                // Post-processing
                //----------------------------------------------------
                this.currentBlockExpr.StatementsExpr = builder.GetList();
                EXPRBLOCK blockExpr = this.currentBlockExpr;
                this.currentBlockExpr = blockExpr.OwingBlockExpr;

                CloseScope();
                CorrectAnonMethScope(blockExpr.ScopeSym);
#if DEBUG
                sbDebug = new StringBuilder();
                DebugUtil.DebugExprsOutput(sbDebug);
                sbDebug.Length = 0;
                DebugUtil.DebugSymsOutput(sbDebug);
#endif
                Compiler.IlGenRec.Compile(equalsSym, methInfo, blockExpr);
            }
            finally
            {
                this.Restore(funcbrecBackup);
            }

            //--------------------------------------------------------
            // Compile GetHashCode Method.
            //--------------------------------------------------------
            methInfo = new METHINFO();
            methInfo.MethodSym = toStringSym;

            try
            {
                InitMethod2(methInfo, aggInfo);

                CreateNewScope();
                SCOPESYM scopeSym = this.currentScopeSym;
                this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

                this.currentBlockExpr = NewExprBlock(treeNode);
                this.currentBlockExpr.ScopeSym = this.currentScopeSym;

                StatementListBuilder getHashCodebuilder = new StatementListBuilder();

                TYPESYM intTypeSym = GetRequiredPredefinedType(PREDEFTYPE.INT);
                string getHashCodeName=Compiler.NameManager.GetPredefinedName(PREDEFNAME.GETHASHCODE);
                string hashLocName = CreateAnonymousLocalName("hash");

                //----------------------------------------------------
                // (1) Define a local variable.
                //----------------------------------------------------
                LOCVARSYM hashLocSym = DeclareVar(null, hashLocName, intTypeSym, false);

                this.unreferencedVarCount++;
                hashLocSym.LocSlotInfo.SetJbitDefAssg(this.uninitedVarCount + 1);
                int cbit = FlowChecker.GetCbit(Compiler, hashLocSym.TypeSym);
                this.uninitedVarCount += cbit;

                EXPR leftExpr1 = BindToLocal(null, hashLocSym, BindFlagsEnum.MemberSet);
                EXPR rightExpr1 = NewExprConstant(null, intTypeSym, new ConstValInit((int)0));

                EXPRDECL declExpr1 = NewExpr(null, EXPRKIND.DECL, this.GetVoidType()) as EXPRDECL;
                declExpr1.LocVarSym = hashLocSym;
                declExpr1.InitialExpr = BindAssignment(null, leftExpr1, rightExpr1, false);

                getHashCodebuilder.Add(declExpr1);

                //----------------------------------------------------
                // (2) Calculate the hash code by the hash codes of the fields.
                //----------------------------------------------------
                for (int i = 0; i < fieldList.Count; ++i)
                {
                    EXPR leftExpr2 = BindToLocal(null, hashLocSym, BindFlagsEnum.RValueRequired);

                    EXPR thisObjectExpr = BindThisImplicit(null);

                    EXPR fieldExpr = BindToField(
                        null,
                        thisObjectExpr,
                        new FieldWithType(fieldList[i], aggTypeSym),
                        BindFlagsEnum.RValueRequired);

                    MemberLookup mem = new MemberLookup();
                    if (!mem.Lookup(
                            Compiler,
                            fieldExpr.TypeSym,
                            fieldExpr,
                            this.parentDeclSym,
                            getHashCodeName,
                            0,
                            MemLookFlagsEnum.UserCallable))
                    {
                        DebugUtil.Assert(false);
                    }

                    EXPRMEMGRP groupExpr = NewExpr(
                        null,
                        EXPRKIND.MEMGRP,
                        Compiler.MainSymbolManager.MethodGroupTypeSym) as EXPRMEMGRP;
                    groupExpr.Name = getHashCodeName;
                    groupExpr.SymKind = SYMKIND.METHSYM;
                    groupExpr.TypeArguments = BSYMMGR.EmptyTypeArray;
                    groupExpr.ParentTypeSym = fieldExpr.TypeSym;
                    groupExpr.MethPropSym = null;
                    groupExpr.ObjectExpr = fieldExpr;
                    groupExpr.ContainingTypeArray = mem.GetAllTypes();
                    groupExpr.Flags |= EXPRFLAG.USERCALLABLE;

                    EXPR rightExpr2 = BindGrpToArgs(null, BindFlagsEnum.RValueRequired, groupExpr, null);

                    EXPR multiExpr2 = BindMultiOp(null, EXPRKIND.BITXOR, leftExpr2, rightExpr2);
                    if (multiExpr2 != null)
                    {
                        getHashCodebuilder.Add(MakeStmt(null, multiExpr2, 0));
                    }
                }

                //----------------------------------------------------
                // (3) apply the bit mask.
                //----------------------------------------------------
                EXPR leftExpr3 = BindToLocal(null, hashLocSym, BindFlagsEnum.RValueRequired);
                EXPR rightExpr3 = NewExprConstant(null, intTypeSym, new ConstValInit((int)0x7FFFFFFF));

                EXPR multiExpr3 = BindMultiOp(null, EXPRKIND.BITAND, leftExpr3, rightExpr3);
                if (multiExpr3 != null)
                {
                    getHashCodebuilder.Add(MakeStmt(null, multiExpr3, 0));
                }

                //----------------------------------------------------
                // (4) return the hash code.
                //----------------------------------------------------
                EXPRRETURN retExpr4 = NewExpr(null, EXPRKIND.RETURN, null) as EXPRRETURN;
                retExpr4.ObjectExpr = BindToLocal(null, hashLocSym, BindFlagsEnum.RValueRequired);

                getHashCodebuilder.Add(retExpr4);

                this.currentBlockExpr.StatementsExpr = getHashCodebuilder.GetList();
                EXPRBLOCK blockExpr = this.currentBlockExpr;
                this.currentBlockExpr = blockExpr.OwingBlockExpr;

                CloseScope();
                CorrectAnonMethScope(blockExpr.ScopeSym);
#if DEBUG
                sbDebug = new StringBuilder();
                DebugUtil.DebugExprsOutput(sbDebug);
                sbDebug.Length = 0;
                DebugUtil.DebugSymsOutput(sbDebug);
#endif
                Compiler.IlGenRec.Compile(getHashCodeSym, methInfo, blockExpr);
            }
            finally
            {
                this.Restore(funcbrecBackup);
            }

            //--------------------------------------------------------
            // Compile ToString Method.
            //--------------------------------------------------------
            methInfo = new METHINFO();
            methInfo.MethodSym = toStringSym;

            try
            {
                InitMethod2(methInfo, aggInfo);

                CreateNewScope();
                SCOPESYM scopeSym = this.currentScopeSym;
                this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

                this.currentBlockExpr = NewExprBlock(treeNode);
                this.currentBlockExpr.ScopeSym = this.currentScopeSym;

                //----------------------------------------------------
                // Bind String.Format Method
                //----------------------------------------------------
                const string fmtName = "Format";
                AGGTYPESYM strAts = Compiler.GetOptPredefType(PREDEFTYPE.STRING, true);
                EXPR strClassExpr = NewExpr(null, EXPRKIND.CLASS, strAts);

                MemberLookup mem = new MemberLookup();
                if (!mem.Lookup(
                        Compiler,
                        strAts,
                        strClassExpr,
                        this.parentDeclSym,
                        fmtName,
                        0,
                        MemLookFlagsEnum.UserCallable))
                {
                    DebugUtil.Assert(false);
                }

                TypeArray typesGroup = mem.GetAllTypes();

                EXPRMEMGRP grpExpr = NewExpr(
                    null,
                    EXPRKIND.MEMGRP,
                    Compiler.MainSymbolManager.MethodGroupTypeSym) as EXPRMEMGRP;
                grpExpr.Name = fmtName;
                grpExpr.SymKind = SYMKIND.METHSYM;
                grpExpr.TypeArguments = BSYMMGR.EmptyTypeArray;
                grpExpr.ParentTypeSym = strAts;
                grpExpr.MethPropSym = null;
                grpExpr.ObjectExpr = null;
                grpExpr.ContainingTypeArray = typesGroup;
                grpExpr.Flags |= EXPRFLAG.USERCALLABLE;

                //----------------------------------------------------
                // Bind the argument of String.Format method (1)
                //----------------------------------------------------
                EXPR argsExpr = null;
                EXPR lastArgsExpr = null;

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{{{{ {0} = {{0}}", nameList[0]);
                for (int i = 1; i < nameList.Count; ++i)
                {
                    sb.AppendFormat(", {0} = {{{1}}}", nameList[i], i);
                }
                sb.Append(" }}");

                CONSTVAL argv0 = new CONSTVAL();
                argv0.SetString(sb.ToString());
                EXPRCONSTANT argvExpr0 = NewExprConstant(
                    null,
                    Compiler.GetOptPredefType(PREDEFTYPE.STRING, true),
                    argv0);

                NewList(argvExpr0, ref argsExpr, ref lastArgsExpr);

                //----------------------------------------------------
                // Bind the argument of String.Format method (2)
                //----------------------------------------------------
                EXPR thisObjectExpr = BindThisImplicit(null);
                BindFlagsEnum fldBindFlags
                    = BindFlagsEnum.RValueRequired | BindFlagsEnum.Arguments;

                for (int i = 0; i < fieldList.Count; ++i)
                {
                    FieldWithType fwt = new FieldWithType(fieldList[i], aggTypeSym);
                    EXPR fieldExpr = BindToField(
                        null,
                        thisObjectExpr,
                        fwt,
                        fldBindFlags);
                    NewList(fieldExpr, ref argsExpr, ref lastArgsExpr);
                }

                //----------------------------------------------------
                // Bind String.Format method and its arguments
                //----------------------------------------------------
                EXPR strFmtExpr = BindGrpToArgs(
                    null,
                    BindFlagsEnum.RValueRequired,
                    grpExpr,
                    argsExpr);

                EXPRRETURN retExpr = NewExpr(null, EXPRKIND.RETURN, null) as EXPRRETURN;
                retExpr.ObjectExpr = strFmtExpr;

                this.currentBlockExpr.StatementsExpr = retExpr;
                EXPRBLOCK blockExpr = this.currentBlockExpr;
                this.currentBlockExpr = blockExpr.OwingBlockExpr;

                CloseScope();
                CorrectAnonMethScope(blockExpr.ScopeSym);

                Compiler.IlGenRec.Compile(toStringSym, methInfo, blockExpr);
            }
            finally
            {
                this.Restore(funcbrecBackup);
            }

            //--------------------------------------------------------
            // Return the fabricated type.
            //--------------------------------------------------------
            return aggSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.InitMethod2
        //
        /// <summary></summary>
        /// <param name="methodInfo"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        private void InitMethod2(METHINFO methodInfo, AGGINFO aggInfo)
        {
            DebugUtil.Assert(
                methodInfo != null &&
                methodInfo.MethodSym != null &&
                aggInfo != null);

            this.methodSym = methodInfo.MethodSym;
            this.currentFieldSym = null;
            this.classTypeVariablesForMethod = null;
            this.treeNode = null;
            this.classInfo = aggInfo;
            this.methodInfo = methodInfo;

            this.parentAggSym = this.methodSym.ClassSym;
            this.parentDeclSym = this.methodSym.ContainingDeclaration();

            this.typeBindFlags = this.methodSym.IsContainedInDeprecated() ?
                TypeBindFlagsEnum.NoDeprecated : TypeBindFlagsEnum.None;

            this.OuterScopeSym
                = Compiler.LocalSymbolManager.CreateLocalSym(SYMKIND.SCOPESYM, null, null) as SCOPESYM;
            this.OuterScopeSym.NestingOrder = 0;
#if DEBUG
            this.OuterScopeSym.SetName("Top Scope of Method");
#endif
            methodInfo.OuterScopeSym = this.OuterScopeSym;
            this.innermostCatchScopeSym
                = this.innermostTryScopeSym
                = this.innermostFinallyScopeSym
                = this.currentScopeSym
                = this.OuterScopeSym;
            this.innermostSwitchScopeSym = null;

            // this.InitLabels is initialzed by the initializer.
            this.InitLabels.BreakLabel = null;
            this.InitLabels.ContinueLabel = null;
            this.LoopLabels = InitLabels;

            this.uninitedVarCount = 0;
            this.unreferencedVarCount = 0;
            this.finallyNestingCount = 0;
            this.localCount = 0;
            this.firstAnonymousMethodInfo = null;
            this.currentAnonymousMethodInfo = null;
            this.gotoExprs = null;

            //userLabelList = null;
            //pUserLabelList = &userLabelList;
            this.userLabelList = null;
            this.userLabelListLast = null;

            this.areForwardGotos = false;

            InitThisPointer();

            this.currentBlockExpr = null;
            this.lastNode = null;
        }

        //------------------------------------------------------------
        // FUNCBREC.ConstructAnonymousType
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        /// <param name="newNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM ConstructAnonymousType(AGGSYM aggSym, NEWNODE newNode)
        {
            AGGTYPESYM aggTypeSym = null;
            TypeArray typeArray = new TypeArray();

            DECLSTMTNODE declNode = newNode.InitialNode as DECLSTMTNODE;
            BASENODE node = (declNode != null ? declNode.VariablesNode : null);

            while (node != null)
            {
                VARDECLNODE varDecl;
                if (node.Kind == NODEKIND.LIST)
                {
                    varDecl = node.AsLIST.Operand1 as VARDECLNODE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    varDecl = node as VARDECLNODE;
                    node = null;
                }

                BINOPNODE assignNode = varDecl.ArgumentsNode as BINOPNODE;
                if (assignNode == null ||
                    assignNode.Operator != OPERATOR.ASSIGN ||
                    assignNode.Operand1 == null ||
                    assignNode.Operand1.Kind != NODEKIND.NAME ||
                    assignNode.Operand2 == null)
                {
                    continue;
                }

                EXPR expr = BindExpr(
                    assignNode.Operand2,
                    BindFlagsEnum.RValueRequired);
                typeArray.Add(expr.TypeSym);
            }

            typeArray = Compiler.MainSymbolManager.AllocParams(typeArray);

            if (typeArray.Count > 0)
            {
                aggTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                    aggSym,
                    null,
                    typeArray,
                    null);
            }
            else
            {
                aggTypeSym = aggSym.GetThisType();
            }

            aggTypeSym.ConstraintsChecked = true;
            aggTypeSym.HasConstraintError = false;
            aggTypeSym.AggState = aggSym.AggState;
            return aggTypeSym;
        }
    }
}
