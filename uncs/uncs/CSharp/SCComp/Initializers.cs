//============================================================================
// ImplicitlyTypedArray.cs
//
// 2016/01/12 (hirano567@hotmail.co.jp)
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
        // CParser.ParseObjectInitializer
        //
        /// <summary></summary>
        /// <param name="newNode"></param>
        //------------------------------------------------------------
        internal void ParseObjectInitializer(NEWNODE newNode)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.OPENCURLY);
            NextToken();

            DECLSTMTNODE stmtNode = AllocNode(NODEKIND.DECLSTMT, newNode) as DECLSTMTNODE;

            CListMaker list = new CListMaker(this);
            int comma = -1;

            while (
                CurrentTokenID() != TOKENID.CLOSECURLY &&
                CurrentTokenID() != TOKENID.ENDFILE)
            {
                if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                    PeekToken(1) == TOKENID.EQUAL)
                {
                    list.Add(
                        ParseVariableDeclarator(
                            stmtNode,
                            stmtNode,
                            (uint)PARSEDECLFLAGS.LOCAL,
                            false,  // isFirst,
                            -1),
                        comma);
                }
                else
                {
                    Error(CSCERRID.ERR_InvalidInitializerDeclarator);

                    TOKENID tid = CurrentTokenID();
                    while (
                        tid != TOKENID.COMMA &&
                        tid != TOKENID.CLOSECURLY &&
                        tid != TOKENID.ENDFILE)
                    {
                        NextToken();
                        tid = CurrentTokenID();
                    }
                }

                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
            }
            Eat(TOKENID.CLOSECURLY);

            stmtNode.VariablesNode = list.GetList(stmtNode);
            newNode.InitialNode = stmtNode;
            newNode.Flags |= NODEFLAGS.NEW_HAS_OBJECT_INITIALIZER;

            if (newNode.ParentNode != null &&
                newNode.ParentNode.Kind == NODEKIND.VARDECL)
            {
                VARDECLNODE vdNode = newNode.ParentNode as VARDECLNODE;
                if (vdNode != null)
                {
                    newNode.Flags |= NODEFLAGS.NEW_IN_VARDECL;
                    vdNode.NewFlags |= NODEFLAGS.NEW_HAS_OBJECT_INITIALIZER;
                }
            }
            else if (newNode.ParentNode != null &&
                newNode.ParentNode.Kind == NODEKIND.BINOP &&
                newNode.Operator == OPERATOR.ASSIGN)
            {
                VARDECLNODE vdNode = newNode.ParentNode.ParentNode as VARDECLNODE;
                if (vdNode != null)
                {
                    newNode.Flags |= NODEFLAGS.NEW_IN_VARDECL;
                    vdNode.NewFlags |= NODEFLAGS.NEW_HAS_OBJECT_INITIALIZER;
                }
            }

            BASENODE node = newNode.ParentNode;
            while (node != null && !node.IsStatement)
            {
                node = node.ParentNode;
            }
            if (node != null)
            {
                (node as STATEMENTNODE).NewFlags |= NODEFLAGS.NEW_HAS_OBJECT_INITIALIZER;
            }
        }

        //------------------------------------------------------------
        // CParser.ParseCollectionInitializer
        //
        /// <summary></summary>
        /// <param name="newNode"></param>
        //------------------------------------------------------------
        internal void ParseCollectionInitializer(NEWNODE newNode)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.OPENCURLY);
            NextToken();

            UNOPNODE initNode = AllocNode(NODEKIND.COLLECTIONINIT, newNode).AsCOLLECTIONINIT;

            CListMaker list = new CListMaker(this);
            int comma = -1;

            while (CurrentTokenID() != TOKENID.CLOSECURLY)
            {
                BASENODE nd = ParseVariableInitializer(initNode, false, -1);
                if (nd.Kind == NODEKIND.BINOP && nd.Operator == OPERATOR.ASSIGN)
                {
                    Error(CSCERRID.ERR_InvalidInitializerDeclarator);
                }
                else
                {
                    list.Add(nd, comma);
                }

                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
            }
            Eat(TOKENID.CLOSECURLY);

            initNode.Operand = list.GetList(initNode);
            if (initNode.Operand == null)
            {
                newNode.InitialNode = null;
                return;
            }

            newNode.InitialNode = initNode;
            newNode.Flags |= NODEFLAGS.NEW_HAS_COLLECTION_INITIALIZER;

            if (newNode.ParentNode != null &&
                newNode.ParentNode.Kind == NODEKIND.VARDECL)
            {
                VARDECLNODE vdNode = newNode.ParentNode as VARDECLNODE;
                if (vdNode != null)
                {
                    newNode.Flags |= NODEFLAGS.NEW_IN_VARDECL;
                    vdNode.NewFlags |= NODEFLAGS.NEW_HAS_COLLECTION_INITIALIZER;
                }
            }
            else if (newNode.ParentNode != null &&
                newNode.ParentNode.Kind == NODEKIND.BINOP &&
                newNode.Operator == OPERATOR.ASSIGN)
            {
                VARDECLNODE vdNode = newNode.ParentNode.ParentNode as VARDECLNODE;
                if (vdNode != null)
                {
                    newNode.Flags |= NODEFLAGS.NEW_IN_VARDECL;
                    vdNode.NewFlags |= NODEFLAGS.NEW_HAS_COLLECTION_INITIALIZER;
                }
            }

            BASENODE node = newNode.ParentNode;
            while (node != null && !node.IsStatement)
            {
                node = node.ParentNode;
            }
            if (node != null)
            {
                (node as STATEMENTNODE).NewFlags |= NODEFLAGS.NEW_HAS_COLLECTION_INITIALIZER;
            }
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// (CS3) for build the initializer. (CSharp\SCComp\Initializer.cs)
        /// </summary>
        private StatementListBuilder initializerBuilder1 = null;

        /// <summary>
        /// (CS3) for build the initializer. (CSharp\SCComp\Initializer.cs)
        /// </summary>
        private StatementListBuilder initializerBuilder2 = null;

        //------------------------------------------------------------
        // FUNCBREC.BindObjectInitializer
        //
        /// <summary></summary>
        /// <param name="newNode"></param>
        /// <param name="typeSym"></param>
        /// <param name="locVarSym"></param>
        /// <param name="objectExpr"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindObjectInitializer(
            NEWNODE newNode,
            TYPESYM typeSym,
            LOCVARSYM locVarSym,
            EXPR objectExpr,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(newNode != null && typeSym != null && builder != null);
            DebugUtil.Assert(
                (locVarSym != null && objectExpr == null) ||
                (locVarSym == null && objectExpr != null));

            BindFlagsEnum bindFlags = BindFlagsEnum.RValueRequired;
            string localFormat = "<{0}><{1}>__local";

            //--------------------------------------------------------
            // Bind the local variable.
            //--------------------------------------------------------
            if (locVarSym == null)
            {
                string typeName = typeSym.IsAGGTYPESYM ?
                    (typeSym as AGGTYPESYM).GetAggregate().Name :
                    typeSym.Name;
                string locName = String.Format(localFormat, typeName, (this.localCount)++);

                locVarSym = Compiler.LocalSymbolManager.CreateLocalSym(
                    SYMKIND.LOCVARSYM,
                    locName,
                    this.currentScopeSym) as LOCVARSYM;
                locVarSym.TypeSym = typeSym;
                locVarSym.LocSlotInfo.HasInit = true;
                locVarSym.DeclTreeNode = newNode;

                StoreInCache(newNode, locName, locVarSym, null, true);
            }

            EXPR locVarExpr = BindToLocal(
                newNode,
                locVarSym,
                bindFlags | BindFlagsEnum.MemberSet);

            //--------------------------------------------------------
            // If objectExpr is not null, assign it to the local variable.
            //--------------------------------------------------------
            if (objectExpr != null)
            {
                EXPR assignLocExpr = BindAssignment(
                    newNode,
                    locVarExpr,
                    objectExpr,
                    false);

                builder.Add(SetNodeStmt(newNode, MakeStmt(newNode, assignLocExpr, 0)));
            }

            //--------------------------------------------------------
            // Assign each value.
            //--------------------------------------------------------
            DECLSTMTNODE decNode = newNode.InitialNode as DECLSTMTNODE;
            DebugUtil.Assert(decNode != null);
            BASENODE node = decNode.VariablesNode;

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

                //----------------------------------------------------
                // LHS
                //----------------------------------------------------
                NAMENODE nameNode = assignNode.Operand1 as NAMENODE;
                DebugUtil.Assert(nameNode != null);

                bindFlags = BindFlagsEnum.MemberSet;
                MemberLookup mem = new MemberLookup();

                EXPR leftExpr = null;
                if (mem.Lookup(
                        Compiler,
                        typeSym,
                        locVarExpr,
                        this.parentDeclSym,
                        nameNode.Name,
                        0,
                        MemLookFlagsEnum.UserCallable))
                {
                    SymWithType swt = mem.FirstSymWithType;
                    DebugUtil.Assert(swt != null && swt.IsNotNull);

                    switch (swt.Sym.Kind)
                    {
                        case SYMKIND.MEMBVARSYM:
                            leftExpr = BindToField(
                                assignNode,
                                locVarExpr,
                                FieldWithType.Convert(swt),
                                bindFlags);
                            break;

                        case SYMKIND.PROPSYM:
                            leftExpr = BindToProperty(
                                assignNode,
                                locVarExpr,
                                PropWithType.Convert(swt),
                                bindFlags,
                                null,
                                null);
                            break;

                        default:
                            leftExpr = NewError(nameNode, null);
                            break;
                    }
                }
                else
                {
                    mem.ReportErrors(nameNode);
                    leftExpr = NewError(nameNode, null);
                }

                //----------------------------------------------------
                // Collection initializer
                //----------------------------------------------------
                if (assignNode.Operand2.Kind == NODEKIND.ARRAYINIT)
                {
                    if (!leftExpr.TypeSym.IsARRAYSYM)
                    {
                        BindCollectionInitializer(
                            newNode,
                            (assignNode.Operand2 as UNOPNODE).Operand,
                            leftExpr.TypeSym,   //typeSym,
                            null,   //locVarSym,
                            leftExpr,
                            null,
                            builder);
                        continue;
                    }
                }

                //----------------------------------------------------
                // RHS
                //----------------------------------------------------
                EXPR rightExpr = BindExpr(
                    assignNode.Operand2,
                    BindFlagsEnum.RValueRequired);

                //----------------------------------------------------
                // Assign
                //----------------------------------------------------
                EXPR assignExpr = BindAssignment(
                    assignNode,
                    leftExpr,
                    rightExpr,
                    false);

                builder.Add(SetNodeStmt(decNode, MakeStmt(decNode, assignExpr, 0)));
            }

            return locVarExpr;
        }


        //------------------------------------------------------------
        // FUNCBREC.BindCollectionInitializer
        //
        /// <summary></summary>
        /// <param name="newNode"></param>
        /// <param name="typeSym"></param>
        /// <param name="locVarSym"></param>
        /// <param name="objectExpr"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindCollectionInitializer(
            NEWNODE newNode,
            BASENODE elementsNode,
            TYPESYM typeSym,
            LOCVARSYM locVarSym,
            EXPR leftExpr,
            EXPR rightExpr,
            StatementListBuilder builder)
        {
            DebugUtil.Assert(newNode != null && typeSym != null && builder != null);
            DebugUtil.Assert(locVarSym != null || leftExpr != null || rightExpr != null);

            string addMethName = Compiler.NameManager.GetPredefinedName(PREDEFNAME.ADD);

            //--------------------------------------------------------
            // typeSym should implement IEnumerable.
            //--------------------------------------------------------
            AGGTYPESYM enumerableSym = HasIEnumerable(typeSym);
            if (enumerableSym == null)
            {
                Compiler.Error(
                    newNode.TypeNode,
                    CSCERRID.ERR_CollectInitRequiresIEnumerable,
                    new ErrArg(typeSym));
                return rightExpr;
            }

            TYPESYM paramTypeSym = null;
            TypeArray typeArgs = enumerableSym.TypeArguments;
            if (typeArgs != null && typeArgs.Count > 0)
            {
                DebugUtil.Assert(typeArgs.Count == 1);
                paramTypeSym = typeArgs[0];
            }
            else
            {
                paramTypeSym
                    = Compiler.GetReqPredefAgg(PREDEFTYPE.OBJECT, true).GetThisType();
                if (typeArgs == null)
                {
                    typeArgs = new TypeArray();
                }
                typeArgs.Add(paramTypeSym);
                typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);
            }

            BindFlagsEnum bindFlags = BindFlagsEnum.RValueRequired;
            string localFormat = "<{0}><{1}>__local";

            //--------------------------------------------------------
            // Bind the local variable.
            //--------------------------------------------------------
            if (leftExpr == null)
            {
                if (locVarSym == null)
                {
                    string locName
                        = String.Format(localFormat, typeSym.Name, (this.localCount)++);

                    locVarSym = Compiler.LocalSymbolManager.CreateLocalSym(
                        SYMKIND.LOCVARSYM,
                        locName,
                        this.currentScopeSym) as LOCVARSYM;
                    locVarSym.TypeSym = typeSym;
                    locVarSym.LocSlotInfo.HasInit = true;
                    locVarSym.DeclTreeNode = newNode;

                    StoreInCache(newNode, locName, locVarSym, null, true);
                }

                leftExpr = BindToLocal(
                    newNode,
                    locVarSym,
                    bindFlags | BindFlagsEnum.MemberSet);
            }

            //--------------------------------------------------------
            // If objectExpr is not null, assign it to the local variable.
            //--------------------------------------------------------
            if (rightExpr != null)
            {
                EXPR assignLocExpr = BindAssignment(
                    newNode,
                    leftExpr,
                    rightExpr,
                    false);

                builder.Add(SetNodeStmt(newNode, MakeStmt(newNode, assignLocExpr, 0)));
            }

            //--------------------------------------------------------
            // Get "Add" method.
            //--------------------------------------------------------
            MemberLookup mem = new MemberLookup();

            if (!mem.Lookup(
                    Compiler,
                    typeSym,
                    leftExpr,
                    this.parentDeclSym,
                    addMethName,
                    0,
                    MemLookFlagsEnum.UserCallable))
            {
                Compiler.Error(
                    newNode.TypeNode,
                    CSCERRID.ERR_NoSuchMember,
                    new ErrArg(typeSym),
                    new ErrArg("Add"));
                return NewError(newNode, null);
            }
            if (mem.FirstSym == null || !mem.FirstSym.IsMETHSYM)
            {
                return NewError(newNode, null);
            }

            TypeArray typeGroup = mem.GetAllTypes();

            EXPRMEMGRP grpExpr = NewExpr(
                newNode,
                EXPRKIND.MEMGRP,
                Compiler.MainSymbolManager.MethodGroupTypeSym) as EXPRMEMGRP;

            grpExpr.Name = addMethName;
            grpExpr.SymKind = SYMKIND.METHSYM;
            grpExpr.TypeArguments = BSYMMGR.EmptyTypeArray;
            grpExpr.ParentTypeSym = typeSym;
            grpExpr.MethPropSym = null;
            grpExpr.ObjectExpr = leftExpr;
            grpExpr.ContainingTypeArray = typeGroup;
            grpExpr.Flags |= EXPRFLAG.USERCALLABLE;

            //--------------------------------------------------------
            // Add each value.
            //--------------------------------------------------------
            DebugUtil.Assert(newNode.InitialNode != null);
#if false
            BASENODE node = null;
            switch (newNode.InitialNode.Kind)
            {
                case NODEKIND.DECLSTMT:
                    node = (newNode.InitialNode as DECLSTMTNODE).VariablesNode;
                    break;

                case NODEKIND.UNOP:
                    node = (newNode.InitialNode as UNOPNODE).Operand;
                    break;

                default:
                    DebugUtil.Assert(false);
                    break;
            }
#endif
            BASENODE node = elementsNode;
            while (node != null)
            {
                BASENODE elementNode;
                if (node.Kind == NODEKIND.LIST)
                {
                    elementNode = node.AsLIST.Operand1;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    elementNode = node;
                    node = null;
                }

                //----------------------------------------------------
                // Bind the elements
                //----------------------------------------------------
                EXPR elementExpr = BindExpr(
                    elementNode,
                    BindFlagsEnum.RValueRequired);

                //----------------------------------------------------
                // Add the elements
                //----------------------------------------------------
                EXPR addExpr = BindGrpToArgs(newNode, bindFlags, grpExpr, elementExpr);

                if (addExpr != null)
                {
                    builder.Add(SetNodeStmt(newNode, MakeStmt(newNode, addExpr, 0)));
                }
            }

            return leftExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.HasIEnumerable (2)
        //
        /// <summary>
        /// Rewrite HasIEnumerable for collection initializer.
        /// Return the IEnumerable or IEnumerable&lt;T&gt; instance.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="tree"></param>
        /// <param name="badType"></param>
        /// <param name="badMember"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private AGGTYPESYM HasIEnumerable(
            TYPESYM collectionTypeSym/*,
            BASENODE treeNode,
            TYPESYM badTypeSym,
            PREDEFNAME badMemberName*/
                                      )
        {
            AGGTYPESYM ifaceCandidateAts = null;
            // First try the generic interfaces
            AGGSYM gEnumAggSym
                = Compiler.GetOptPredefAgg(PREDEFTYPE.G_IENUMERABLE, true);
            TypeArray allIfacesTypeArray = null;
            AGGTYPESYM baseAts = null;

            // If generics don't exist or the type isn't an AGGTYPESYM
            // then we can't check the interfaces (and base-class interfaces)
            // for IEnumerable<T> so immediately try the non-generic IEnumerable
            if (gEnumAggSym == null)
            {
                goto NO_GENERIC;
            }

            if (collectionTypeSym.IsAGGTYPESYM)
            {
                if (collectionTypeSym.GetAggregate() == gEnumAggSym ||
                    collectionTypeSym.IsPredefType(PREDEFTYPE.IENUMERABLE))
                {
                    DebugUtil.Assert(false, "IEnumerable/ator types are bad!");
                    goto LERROR;
                }

                AGGTYPESYM tempAts = collectionTypeSym as AGGTYPESYM;
                allIfacesTypeArray = tempAts.GetIfacesAll();
                baseAts = tempAts.GetBaseClass();
            }
            else if (collectionTypeSym.IsTYVARSYM)
            {
                // Note:
                // we'll search the interface list before the class constraint,
                // but it doesn't matter since we require a unique instantiation of IEnumerable<T>.

                // Note:
                // The pattern search will usually find the interface constraint
                // - but if the class constraint has a non-public or non-applicable
                // or non-method GetEnumerator,
                // the interfaces are hidden in which case we will find them here.

                TYVARSYM tempTvSym = collectionTypeSym as TYVARSYM;
                allIfacesTypeArray = tempTvSym.AllInterfaces;
                baseAts = tempTvSym.BaseClassSym;
            }
            else
            {
                goto NO_GENERIC;
            }

            DebugUtil.Assert(allIfacesTypeArray != null);

            // If the type implements exactly one instantiation of
            // IEnumerable<T> then it's the one.
            //
            // If it implements none then try the non-generic interface.
            //
            // If it implements more than one, then it's an error.
            //
            // Search the direct and indirect interfaces via allIfacesTypeArray,
            // going up the base chain...
            // Work up the base chain
            for (; ; )
            {
                // Now work across all the interfaces
                for (int i = 0; i < allIfacesTypeArray.Count; ++i)
                {
                    AGGTYPESYM iface = allIfacesTypeArray[i] as AGGTYPESYM;
                    if (iface.GetAggregate() == gEnumAggSym)
                    {
                        if (ifaceCandidateAts == null)
                        {
                            // First implementation
                            ifaceCandidateAts = iface;
                        }
                        else if (iface != ifaceCandidateAts)
                        {
                            // If this really is a different instantiation report an error
                            Compiler.Error(
                                treeNode,
                                CSCERRID.ERR_MultipleIEnumOfT,
                                new ErrArgRef(collectionTypeSym),
                                new ErrArg(gEnumAggSym.GetThisType()));
                            return null;
                        }
                    }
                }
                // Check the base class.
                if (baseAts == null)
                {
                    break;
                }
                allIfacesTypeArray = baseAts.GetIfacesAll();
                baseAts = baseAts.GetBaseClass();
            }

            // Report the one and only generic interface
            if (ifaceCandidateAts != null)
            {
                DebugUtil.Assert(
                    CanConvert(collectionTypeSym, ifaceCandidateAts, ConvertTypeEnum.NOUDC));
                return ifaceCandidateAts;
            }

        NO_GENERIC:
            if (collectionTypeSym.IsPredefType(PREDEFTYPE.IENUMERABLE))
            {
                DebugUtil.VsFail("Why didn't IEnumerator match the pattern?");
                goto LERROR;
            }

            // No errors, no generic interfaces, try the non-generic interface
            ifaceCandidateAts = GetRequiredPredefinedType(PREDEFTYPE.IENUMERABLE);
            if (CanConvert(collectionTypeSym, ifaceCandidateAts, ConvertTypeEnum.NOUDC))
            {
                return ifaceCandidateAts;
            }

        LERROR:
            return null;
        }
    }
}
