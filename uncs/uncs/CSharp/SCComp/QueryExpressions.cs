//============================================================================
// QueryExpressions.cs
//
// 2016/01/02 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
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
        // enum CParser.QueryKeywordEnum
        //------------------------------------------------------------
        private enum QueryKeywordEnum
        {
            None = 0,
            From,
            Join,
            On,
            Eequals,
            Into,
            Let,
            Orderby,
            Ascending,
            Descending,
            Select,
            Group,
            By,
        }

        private static Dictionary<string, QueryKeywordEnum> queryKeywordDictionary = null;

        //------------------------------------------------------------
        // CParser.InitQueryKeywordDictionary
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void InitQueryKeywordDictionary()
        {
            if (queryKeywordDictionary != null)
            {
                return;
            }
            queryKeywordDictionary = new Dictionary<string, QueryKeywordEnum>();

            queryKeywordDictionary.Add("from", QueryKeywordEnum.From);
            queryKeywordDictionary.Add("join", QueryKeywordEnum.Join);
            queryKeywordDictionary.Add("on", QueryKeywordEnum.On);
            queryKeywordDictionary.Add("equals", QueryKeywordEnum.Eequals);
            queryKeywordDictionary.Add("into", QueryKeywordEnum.Into);
            queryKeywordDictionary.Add("let", QueryKeywordEnum.Let);
            queryKeywordDictionary.Add("orderby", QueryKeywordEnum.Orderby);
            queryKeywordDictionary.Add("ascending", QueryKeywordEnum.Ascending);
            queryKeywordDictionary.Add("descending", QueryKeywordEnum.Descending);
            queryKeywordDictionary.Add("select", QueryKeywordEnum.Select);
            queryKeywordDictionary.Add("group", QueryKeywordEnum.Group);
            queryKeywordDictionary.Add("by", QueryKeywordEnum.By);
        }

        //------------------------------------------------------------
        // CParser.GetQueryKeywordID
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private QueryKeywordEnum GetQueryKeywordID(string str)
        {
            InitQueryKeywordDictionary();

            try
            {
                QueryKeywordEnum id;
                if (queryKeywordDictionary.TryGetValue(str, out id))
                {
                    return id;
                }
                return QueryKeywordEnum.None;
            }
            catch (ArgumentException)
            {
            }
            return QueryKeywordEnum.None;
        }

        //------------------------------------------------------------
        // CParser.IsQueryKeyword
        //
        /// <summary></summary>
        /// <param name="str"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsQueryKeyword(string str)
        {
            InitQueryKeywordDictionary();

            try
            {
                QueryKeywordEnum id;
                return queryKeywordDictionary.TryGetValue(str, out id);
            }
            catch (ArgumentException)
            {
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.ParseFromClause
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="outerParamName"></param>
        /// <param name="isFollowingFromClause"></param>
        /// <param name="showError"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FROMCLAUSENODE ParseFromClause(
            BASENODE parentNode,
            bool showError)
        {
            FROMCLAUSENODE fromNode = null;
            PARAMETERNODE paramNode = null;

            int mark = CurrentTokenIndex();

            //--------------------------------------------------------
            // (1) "from"  identifier "in"
            //--------------------------------------------------------
            if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                PeekToken(1) == TOKENID.IN)
            {

                fromNode = AllocNode(NODEKIND.FROMCLAUSE, null) as FROMCLAUSENODE;
                fromNode.ParentNode = parentNode;

                paramNode = AllocNode(NODEKIND.PARAMETER, fromNode) as PARAMETERNODE;
                paramNode.TypeNode = AllocNode(
                    NODEKIND.IMPLICITTYPE,
                    fromNode) as IMPLICITTYPENODE;
                paramNode.NameNode = ParseIdentifier(fromNode);
                if (IsQueryKeyword(paramNode.NameNode.Name))
                {
                    Error(CSCERRID.ERR_InvalidExprTerm, new ErrArg(paramNode.NameNode.Name));
                    Rewind(mark);
                    return null;
                }
                fromNode.ParameterNode = paramNode;

                Eat(TOKENID.IN);

                PushParseMode(ParseModeEnum.QueryExpression);
                try
                {
                    fromNode.ExpressionNode = ParseExpression(fromNode, -1);
                }
                finally
                {
                    PopParseMode();
                }
                return fromNode;
            }

            //--------------------------------------------------------
            // (2) "from"  type  identifier "in" or not from-clause
            //--------------------------------------------------------
            if (!MayBeTypeToken(CurrentTokenID()))
            {
                return null;
            }

            TYPEBASENODE typeNode = ParseType(parentNode, false);
            if (IsInvalidTypeNode(typeNode))
            {
                Rewind(mark);
                return null;
            }

            if (CurrentTokenID() != TOKENID.IDENTIFIER)
            {
                Rewind(mark);
                return null;
            }

            // Reparse as a query expression.
            NAMENODE idNode = null;

            Rewind(mark);
            PushParseMode(ParseModeEnum.QueryExpression);
            try
            {
                typeNode = ParseType(parentNode, false);
                idNode = ParseIdentifier(parentNode);
            }
            finally
            {
                PopParseMode();
            }

            if (CurrentTokenID() != TOKENID.IN)
            {
                if (showError)
                {
                    Error(
                        CSCERRID.ERR_ExpectContextualKeyword,
                        new ErrArg("in"));
                }
                return null;
            }
            NextToken();

            fromNode = AllocNode(NODEKIND.FROMCLAUSE, null) as FROMCLAUSENODE;
            fromNode.ParentNode = parentNode;

            paramNode = AllocNode(NODEKIND.PARAMETER, fromNode) as PARAMETERNODE;
            typeNode.ParentNode = paramNode;
            idNode.ParentNode = paramNode;
            paramNode.TypeNode = typeNode;
            paramNode.NameNode = idNode;
            paramNode.ParentNode = fromNode;
            fromNode.ParameterNode = paramNode;

            PushParseMode(ParseModeEnum.QueryExpression);
            try
            {
                fromNode.ExpressionNode = ParseExpression(fromNode, -1);
            }
            finally
            {
                PopParseMode();
            }

            return fromNode;
        }

        //------------------------------------------------------------
        // CParser.ParseFromClause2
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="outerParamName"></param>
        /// <param name="isFollowingFromClause"></param>
        /// <param name="showError"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FROMCLAUSENODE2 ParseFromClause2(
            BASENODE parentNode,
            ref string outerParamName,
            bool showError)
        {
            FROMCLAUSENODE2 fromNode = null;
            LAMBDAEXPRNODE lambdaNode = null;

            //--------------------------------------------------------
            // (1) "from"  identifier  "in"  expression
            //--------------------------------------------------------
            if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                PeekToken(1) == TOKENID.IN)
            {
                fromNode = AllocNode(NODEKIND.FROMCLAUSE2, null) as FROMCLAUSENODE2;

                fromNode.ParentNode = parentNode;
                fromNode.ParameterNode
                    = AllocNode(NODEKIND.PARAMETER, fromNode) as PARAMETERNODE;
                fromNode.ParameterNode.TypeNode = AllocNode(
                    NODEKIND.IMPLICITTYPE,
                    fromNode) as IMPLICITTYPENODE;

                fromNode.ParameterNode.NameNode = ParseIdentifier(fromNode);
                if (IsQueryKeyword(fromNode.ParameterNode.NameNode.Name))
                {
                    Error(CSCERRID.ERR_InvalidExprTerm, new ErrArg(fromNode.ParameterNode.NameNode));
                }

                Eat(TOKENID.IN);

                lambdaNode = AllocNode(
                    NODEKIND.LAMBDAEXPR,
                    fromNode) as LAMBDAEXPRNODE;
                lambdaNode.CloseParenIndex = -1;

                PushParseMode(ParseModeEnum.QueryExpression);
                try
                {
                    ParseLambdaExpressionRHS(lambdaNode, -1);
                }
                finally
                {
                    PopParseMode();
                }
                //ParsePossibleQueryExpression(lambdaNode);
                fromNode.ExpressionNode = lambdaNode;

                goto PARAMETER_PRODUCT;
            }
            //--------------------------------------------------------
            // (2) "from"  type  identifier  "in"  expression
            //--------------------------------------------------------
            TYPEBASENODE typeNode = null;
            PushParseMode(ParseModeEnum.QueryExpression);
            try
            {
                typeNode = ParseType(parentNode, false);
            }
            finally
            {
                PopParseMode();
            }
            if (typeNode == null)
            {
                return null;
            }

            NAMENODE idNode = ParseIdentifier(parentNode);
            if (idNode == null)
            {
                return null;
            }
            if (IsQueryKeyword(idNode.Name))
            {
                Error(CSCERRID.ERR_InvalidExprTerm, new ErrArg(idNode.Name));
            }

            if (CurrentTokenID() != TOKENID.IN)
            {
                if (showError)
                {
                    Error(
                        CSCERRID.ERR_ExpectContextualKeyword,
                        new ErrArg("in"));
                }
                return null;
            }
            NextToken();

            fromNode = AllocNode(NODEKIND.FROMCLAUSE2, null) as FROMCLAUSENODE2;
            fromNode.ParentNode = parentNode;
            fromNode.ParameterNode
                = AllocNode(NODEKIND.PARAMETER, fromNode) as PARAMETERNODE;
            fromNode.ParameterNode.TypeNode = typeNode;
            fromNode.ParameterNode.NameNode = idNode;

            lambdaNode = AllocNode(
                NODEKIND.LAMBDAEXPR,
                fromNode) as LAMBDAEXPRNODE;
            lambdaNode.CloseParenIndex = -1;

            PushParseMode(ParseModeEnum.QueryExpression);
            try
            {
                ParseLambdaExpressionRHS(lambdaNode, -1);
            }
            finally
            {
                PopParseMode();
            }
            //ParsePossibleQueryExpression(lambdaNode);
            fromNode.ExpressionNode = lambdaNode;

        PARAMETER_PRODUCT:
            string innerParamName = fromNode.ParameterNode.NameNode.Name;

            fromNode.OutputLambdaExpressionNode
                = FabricateParameterProductOutput(
                    outerParamName,
                    null,
                    innerParamName,
                    null,
                    fromNode);

            outerParamName = QueryUtil.CreateParameterProductName(
                outerParamName,
                innerParamName);
            fromNode.ParameterProductName = outerParamName;

            return fromNode;
        }

        //------------------------------------------------------------
        // CParser.ParsePossibleQueryExpression
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void ParsePossibleQueryExpression(
            LAMBDAEXPRNODE lambdaNode)
        {
            if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                tokenArray[CurrentTokenIndex()].Name == "from")
            {
                PushParseMode(ParseModeEnum.None);
                try
                {
                    ParseLambdaExpressionRHS(lambdaNode, -1);
                }
                finally
                {
                    PopParseMode();
                }
            }
            else
            {
                PushParseMode(ParseModeEnum.QueryExpression);
                try
                {
                    ParseLambdaExpressionRHS(lambdaNode, -1);
                }
                finally
                {
                    PopParseMode();
                }
            }
        }

        //------------------------------------------------------------
        // CParser.ParseQueryExpression
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="fromNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseQueryExpression(
            BASENODE parentNode,
            FROMCLAUSENODE fromNode)
        {
            DebugUtil.Assert(fromNode != null);

            if (CurrentTokenID() != TOKENID.IDENTIFIER)
            {
                Error(CSCERRID.ERR_QueryBodyHasNoSelectOrGroup);
                return null;
            }

            QUERYEXPRNODE queryNode
                = AllocNode(NODEKIND.QUERYEXPR, parentNode) as QUERYEXPRNODE;

            queryNode.FromNode = fromNode;
            fromNode.ParentNode = queryNode;

            queryNode.QueryBodyNode = ParseQueryBody(
                queryNode,
                fromNode.ParameterNode.NameNode.Name);
            return queryNode;
        }

        //------------------------------------------------------------
        // CParser.ParseQueryBody
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="outerParameterName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseQueryBody(
            BASENODE parentNode,
            string outerParameterName)
        {
            CListMaker bodyList = new CListMaker(this);
            bool toSelectOrGroup = false;
            LAMBDAEXPRNODE lambdaNode = null;
            //string outerParameterName = queryNode.FromNode.ParameterNode.NameNode.Name;

            while (!toSelectOrGroup && CurrentTokenID() != TOKENID.ENDFILE)
            {
                switch (this.tokenArray[CurrentTokenIndex()].Name)
                {
                    //------------------------------------------------
                    // from-clause
                    //------------------------------------------------
                    case "from":
                        NextToken();
                        FROMCLAUSENODE fromNode2 = ParseFromClause2(
                            parentNode,
                            ref outerParameterName,
                            true);
                        bodyList.Add(fromNode2, -1);
                        continue;

                    //------------------------------------------------
                    // let-clause
                    //------------------------------------------------
                    case "let":
                        NextToken();
                        LETCLAUSENODE letNode = AllocNode(
                            NODEKIND.LETCLAUSE,
                            parentNode) as LETCLAUSENODE;

                        PushParseMode(ParseModeEnum.QueryExpression);
                        try
                        {
                            letNode.NameNode = ParseIdentifier(letNode);
                        }
                        finally
                        {
                            PopParseMode();
                        }

                        Eat(TOKENID.EQUAL);

                        PushParseMode(ParseModeEnum.QueryExpression);
                        try
                        {
                            letNode.ExpressionNode = ParseExpression(letNode, -1);
                        }
                        finally
                        {
                            PopParseMode();
                        }

                        letNode.OutputLambdaExpressionNode
                            = FabricateParameterProductOutput(
                                outerParameterName,
                                null,
                                letNode.NameNode.Name,
                                letNode.ExpressionNode,
                                letNode);

                        outerParameterName = QueryUtil.CreateParameterProductName(
                            outerParameterName,
                            letNode.NameNode.Name);
                        letNode.ParameterProductName = outerParameterName;

                        bodyList.Add(letNode, -1);
                        continue;

                    //------------------------------------------------
                    // where-clause
                    //------------------------------------------------
                    case "where":
                        NextToken();
                        WHERECLAUSENODE whereNode = AllocNode(
                            NODEKIND.WHERECLAUSE,
                            parentNode) as WHERECLAUSENODE;
                        whereNode.LambdaExpressionNode = AllocNode(
                            NODEKIND.LAMBDAEXPR,
                            whereNode) as LAMBDAEXPRNODE;
                        whereNode.LambdaExpressionNode.CloseParenIndex = -1;

                        PushParseMode(ParseModeEnum.QueryExpression);
                        try
                        {
                            ParseLambdaExpressionRHS(whereNode.LambdaExpressionNode, -1);
                        }
                        finally
                        {
                            PopParseMode();
                        }

                        bodyList.Add(whereNode, -1);
                        continue;

                    //------------------------------------------------
                    // join-clause, join-into-clause
                    //------------------------------------------------
                    case "join":
                        JOINCLAUSENODE joinNode = AllocNode(
                            NODEKIND.JOINCLAUSE,
                            parentNode) as JOINCLAUSENODE;

                        //--------------------------------------------
                        // "join" [type] identifier "in" expression
                        //--------------------------------------------
                        NextToken();
                        joinNode.InNode = ParseFromClause(
                            joinNode,
                            true);

                        string outerParam = outerParameterName;
                        string innerParam
                            = joinNode.InNode.ParameterNode.NameNode.Name;

                        //--------------------------------------------
                        // "on"
                        //--------------------------------------------
                        if (CurrentTokenID() != TOKENID.IDENTIFIER ||
                            this.tokenArray[CurrentTokenIndex()].Name != "on")
                        {
                            Error(
                                CSCERRID.ERR_ExpectContextualKeyword,
                                new ErrArg("on"));
                        }
                        NextToken();

                        //--------------------------------------------
                        // expression "equals" expression
                        //--------------------------------------------
                        joinNode.EqualLeftLambdaExpressionNode = AllocNode(
                            NODEKIND.LAMBDAEXPR,
                            joinNode) as LAMBDAEXPRNODE;
                        joinNode.EqualLeftLambdaExpressionNode.CloseParenIndex = -1;

                        PushParseMode(ParseModeEnum.QueryExpression);
                        try
                        {
                            ParseLambdaExpressionRHS(joinNode.EqualLeftLambdaExpressionNode, -1);
                        }
                        finally
                        {
                            PopParseMode();
                        }

                        if (CurrentTokenID() != TOKENID.IDENTIFIER ||
                            this.tokenArray[CurrentTokenIndex()].Name != "equals")
                        {
                            Error(
                                CSCERRID.ERR_ExpectContextualKeyword,
                                new ErrArg("equals"));
                        }
                        NextToken();

                        joinNode.EqualRightLambdaExpressionNode = AllocNode(
                            NODEKIND.LAMBDAEXPR,
                            joinNode) as LAMBDAEXPRNODE;
                        joinNode.EqualRightLambdaExpressionNode.CloseParenIndex = -1;

                        PushParseMode(ParseModeEnum.QueryExpression);
                        try
                        {
                            ParseLambdaExpressionRHS(joinNode.EqualRightLambdaExpressionNode, -1);
                        }
                        finally
                        {
                            PopParseMode();
                        }

                        //--------------------------------------------
                        // "into" identifier
                        //--------------------------------------------
                        if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                            this.tokenArray[CurrentTokenIndex()].Name == "into")
                        {
                            NextToken();
                            joinNode.IntoNameNode = ParseIdentifier(parentNode);
                            if (IsQueryKeyword(joinNode.IntoNameNode.Name))
                            {
                                Error(CSCERRID.ERR_InvalidExprTerm, new ErrArg(joinNode.IntoNameNode.Name));
                            }
                        }
                        else
                        {
                            joinNode.IntoNameNode = null;
                        }

                        //--------------------------------------------
                        // Fabricate OutputLambdaExpressionNode
                        //--------------------------------------------
                        if (joinNode.IntoNameNode != null &&
                            !String.IsNullOrEmpty(joinNode.IntoNameNode.Name))
                        {
                            innerParam = joinNode.IntoNameNode.Name;
                        }

                        joinNode.OutputLambdaExpressionNode
                            = FabricateParameterProductOutput(
                                outerParam,
                                null,
                                innerParam,
                                null,
                                joinNode);

                        outerParameterName = QueryUtil.CreateParameterProductName(
                            outerParam,
                            innerParam);
                        joinNode.ParameterProductName = outerParameterName;

                        bodyList.Add(joinNode, -1);
                        continue;

                    //------------------------------------------------
                    // orderby-clause
                    //------------------------------------------------
                    case "orderby":
                        NextToken();
                        ORDERBYCLAUSENODE orderbyNode = AllocNode(
                            NODEKIND.ORDERBYCLAUSE,
                            parentNode) as ORDERBYCLAUSENODE;

                        while (CurrentTokenID() != TOKENID.ENDFILE)
                        {
                            lambdaNode = AllocNode(
                                NODEKIND.LAMBDAEXPR,
                                orderbyNode) as LAMBDAEXPRNODE;
                            lambdaNode.CloseParenIndex = -1;
                            bool descend = false;

                            PushParseMode(ParseModeEnum.QueryExpression);
                            try
                            {
                                ParseLambdaExpressionRHS(lambdaNode, -1);
                            }
                            finally
                            {
                                PopParseMode();
                            }

                            if (CurrentTokenID() == TOKENID.IDENTIFIER)
                            {
                                string id = this.tokenArray[CurrentTokenIndex()].Name;
                                if (id == "ascending")
                                {
                                    descend = false;
                                    NextToken();
                                }
                                else if (id == "descending")
                                {
                                    descend = true;
                                    NextToken();
                                }
                                else
                                {
                                    // nothing to do.
                                }
                            }
                            orderbyNode.Add(lambdaNode, descend);
                            if (CurrentTokenID() != TOKENID.COMMA)
                            {
                                break;
                            }
                            NextToken();
                        }
                        bodyList.Add(orderbyNode, -1);
                        continue;

                    //------------------------------------------------
                    // Otherwise, exit the loop.
                    //------------------------------------------------
                    default:
                        toSelectOrGroup = true;
                        break;
                }
            }

            //--------------------------------------------------------
            // select-clause or group-clause
            //--------------------------------------------------------
            if (CurrentTokenID() != TOKENID.IDENTIFIER)
            {
                Error(CSCERRID.ERR_QueryBodyHasNoSelectOrGroup);
                return null;
            }

            switch (this.tokenArray[CurrentTokenIndex()].Name)
            {
                //----------------------------------------------------
                // "select"  expression
                //----------------------------------------------------
                case "select":
                    NextToken();
                    SELECTCLAUSENODE selectNode = AllocNode(
                        NODEKIND.SELECTCLAUSE,
                        parentNode) as SELECTCLAUSENODE;
                    selectNode.LambdaExpressionNode = AllocNode(
                        NODEKIND.LAMBDAEXPR,
                        selectNode) as LAMBDAEXPRNODE;
                    selectNode.LambdaExpressionNode.CloseParenIndex = -1;

                    PushParseMode(ParseModeEnum.QueryExpression);
                    try
                    {
                        ParseLambdaExpressionRHS(selectNode.LambdaExpressionNode, -1);
                    }
                    finally
                    {
                        PopParseMode();
                    }
                    bodyList.Add(selectNode, -1);
                    break;

                //----------------------------------------------------
                // "group"  expression
                //----------------------------------------------------
                case "group":
                    NextToken();
                    GROUPCLAUSENODE groupNode = AllocNode(
                        NODEKIND.GROUPCLAUSE,
                        parentNode) as GROUPCLAUSENODE;

                    groupNode.ElementExpressionNode = AllocNode(
                        NODEKIND.LAMBDAEXPR,
                        groupNode) as LAMBDAEXPRNODE;
                    groupNode.ElementExpressionNode.CloseParenIndex = -1;

                    PushParseMode(ParseModeEnum.QueryExpression);
                    try
                    {
                        ParseLambdaExpressionRHS(groupNode.ElementExpressionNode, -1);
                    }
                    finally
                    {
                        PopParseMode();
                    }

                    if (CurrentTokenID() != TOKENID.IDENTIFIER ||
                        this.tokenArray[CurrentTokenIndex()].Name != "by")
                    {
                        Error(CSCERRID.ERR_ExpectContextualKeyword, new ErrArg("by"));
                    }
                    NextToken();

                    groupNode.ByExpressionNode = AllocNode(
                        NODEKIND.LAMBDAEXPR,
                        groupNode) as LAMBDAEXPRNODE;
                    groupNode.ByExpressionNode.CloseParenIndex = -1;

                    PushParseMode(ParseModeEnum.QueryExpression);
                    try
                    {
                        ParseLambdaExpressionRHS(groupNode.ByExpressionNode, -1);
                    }
                    finally
                    {
                        PopParseMode();
                    }
                    bodyList.Add(groupNode, -1);
                    break;

                default:
                    Error(CSCERRID.ERR_QueryBodyHasNoSelectOrGroup);
                    return null;
            }

            //--------------------------------------------------------
            // query-continuation
            //--------------------------------------------------------
            if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                this.tokenArray[CurrentTokenIndex()].Name == "into")
            {
                NextToken();
                QUERYCONTINUATIONNODE contNode = AllocNode(
                    NODEKIND.QUERYCONTINUATION,
                    parentNode) as QUERYCONTINUATIONNODE;

                contNode.NameNode = ParseIdentifier(contNode);
                if (IsQueryKeyword(contNode.NameNode.Name))
                {
                    Error(CSCERRID.ERR_InvalidExprTerm, new ErrArg(contNode.NameNode.Name));
                }

                contNode.QueryBodyNode = ParseQueryBody(
                    contNode,
                    contNode.NameNode.Name);

                bodyList.Add(contNode, -1);
            }

            return bodyList.GetList(parentNode);
        }

        //------------------------------------------------------------
        // CParser.MayBeTypeToken
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool MayBeTypeToken(TOKENID id)
        {
            if ((TokenInfoArray[(int)id].Flags & TOKFLAGS.F_PREDEFINED) != 0)
            {
                return true;
            }
            else if (IsNameStart(id))
            {
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.IsInvalidTypeNode
        //
        /// <summary></summary>
        /// <param name="tbNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsInvalidTypeNode(TYPEBASENODE tbNode)
        {
            if (tbNode == null)
            {
                return true;
            }
            if (tbNode.Kind == NODEKIND.NAMEDTYPE)
            {
                NAMEDTYPENODE ntNode = tbNode as NAMEDTYPENODE;
                DebugUtil.Assert(ntNode != null);
                if ((ntNode.NameNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.FabricateParameterProductOutput
        //
        /// <summary></summary>
        /// <param name="outerParamName"></param>
        /// <param name="innerParamName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal LAMBDAEXPRNODE FabricateParameterProductOutput(
            string outerParamName,
            BASENODE outerArgNode,
            string innerParamName,
            BASENODE innerArgNode,
            BASENODE parentNode)
        {
            DebugUtil.Assert(
                !String.IsNullOrEmpty(outerParamName) &&
                !String.IsNullOrEmpty(innerParamName));

            LAMBDAEXPRNODE lambdaNode
                = AllocNode(NODEKIND.LAMBDAEXPR, parentNode) as LAMBDAEXPRNODE;
            lambdaNode.ArgumentsNode = null;
            lambdaNode.CloseParenIndex = -1;

            lambdaNode.BodyNode
                = AllocNode(NODEKIND.BLOCK, lambdaNode) as BLOCKNODE;

            EXPRSTMTNODE returnNode
                = AllocNode(NODEKIND.RETURN, lambdaNode.BodyNode).AsRETURN;
            lambdaNode.BodyNode.StatementsNode = returnNode;

            NEWNODE newNode = AllocNode(NODEKIND.NEW, returnNode) as NEWNODE;
            newNode.Flags |= NODEFLAGS.NEW_ANONYMOUS_OBJECT_CREATION;
            returnNode.ArgumentsNode = newNode;

            DECLSTMTNODE stmtNode = AllocNode(NODEKIND.DECLSTMT, newNode) as DECLSTMTNODE;
            newNode.InitialNode = stmtNode;

            BINOPNODE listNode = AllocNode(NODEKIND.LIST, stmtNode, -1).AsLIST;
            stmtNode.VariablesNode = listNode;

            //--------------------------------------------------------
            // Parameter 1
            //--------------------------------------------------------
            VARDECLNODE varDeclNode1 = AllocNode(NODEKIND.VARDECL, listNode) as VARDECLNODE;
            varDeclNode1.DeclarationsNode = stmtNode;

            NAMENODE nameNode1 = AllocNode(NODEKIND.NAME, null, -1) as NAMENODE;
            nameNode1.Name = outerParamName;
            nameNode1.ParentNode = varDeclNode1;
            varDeclNode1.NameNode = nameNode1;

            NAMENODE nameNode2 = AllocNode(NODEKIND.NAME, null, -1) as NAMENODE;
            nameNode2.Name = outerParamName;

            if (outerArgNode == null)
            {
                NAMENODE nameNode3 = AllocNode(NODEKIND.NAME, null, -1) as NAMENODE;
                nameNode3.Name = outerParamName;
                outerArgNode = nameNode3;
            }

            varDeclNode1.ArgumentsNode = AllocBinaryOpNode(
                OPERATOR.ASSIGN,
                -1,
                varDeclNode1,
                nameNode2,
                outerArgNode);
            nameNode2.ParentNode = varDeclNode1.ArgumentsNode;
            outerArgNode.ParentNode = varDeclNode1.ArgumentsNode;

            listNode.Operand1 = varDeclNode1;

            //--------------------------------------------------------
            // Parameter 2
            //--------------------------------------------------------
            VARDECLNODE varDeclNode2 = AllocNode(NODEKIND.VARDECL, listNode) as VARDECLNODE;
            varDeclNode2.DeclarationsNode = stmtNode;

            nameNode1 = AllocNode(NODEKIND.NAME, null, -1) as NAMENODE;
            nameNode1.Name = innerParamName;
            nameNode1.ParentNode = varDeclNode2;
            varDeclNode2.NameNode = nameNode1;

            nameNode2 = AllocNode(NODEKIND.NAME, null, -1) as NAMENODE;
            nameNode2.Name = innerParamName;

            if (innerArgNode == null)
            {
                NAMENODE nameNode3 = AllocNode(NODEKIND.NAME, null, -1) as NAMENODE;
                nameNode3.Name = innerParamName;
                innerArgNode = nameNode3;
            }

            varDeclNode2.ArgumentsNode = AllocBinaryOpNode(
                OPERATOR.ASSIGN,
                -1,
                varDeclNode2,
                nameNode2,
                innerArgNode);
            nameNode2.ParentNode = varDeclNode2.ArgumentsNode;
            innerArgNode.ParentNode = varDeclNode2.ArgumentsNode;

            listNode.Operand2 = varDeclNode2;

            return lambdaNode;
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //============================================================
        // class FUNCBREC.QueryExpressionInfo
        //============================================================
        internal class QueryExpressionInfo
        {
            //--------------------------------------------------------
            // QueryExpressionInfo Fields and Properties
            //--------------------------------------------------------
            private List<PARAMINFO> parameterList
                = new List<PARAMINFO>();

            internal List<PARAMINFO> ParameterList
            {
                get { return this.parameterList; }
            }

            internal int ParameterCount
            {
                get { return this.parameterList.Count; }
            }

            internal EXPR CollectionExpr = null;

            internal bool IsOK
            {
                get
                {
                    return (
                        CollectionExpr != null &&
                        CollectionExpr.Kind != EXPRKIND.ERROR);
                }
            }

            internal TYPESYM ElementTypeSym = null;

            internal bool ToExpressionTrees = false;

            //--------------------------------------------------------
            // QueryExpressionInfo.AddParamter (1)
            //
            /// <summary></summary>
            /// <param name="node"></param>
            /// <param name="type"></param>
            //--------------------------------------------------------
            internal void AddParamter(PARAMETERNODE node, TYPESYM type)
            {
                PARAMINFO paramInfo = new PARAMINFO();
                paramInfo.ParameterNode = node;
                paramInfo.TypeSym = type;
                paramInfo.Name = node.NameNode.Name;

                this.parameterList.Add(paramInfo);
            }

            //--------------------------------------------------------
            // QueryExpressionInfo.AddParamter (2)
            //
            /// <summary></summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            //--------------------------------------------------------
            internal void AddParamter(
                string name,
                TYPESYM type,
                bool isParameterProduct)
            {
                PARAMINFO paramInfo = new PARAMINFO();
                paramInfo.ParameterNode = null;
                paramInfo.TypeSym = type;
                paramInfo.Name = name;
                paramInfo.TypeSym.GetAggregate().IsParameterProcuct
                    = isParameterProduct;

                this.parameterList.Add(paramInfo);
            }

            //--------------------------------------------------------
            // QueryExpressionInfo.ClearParameterInfos
            //
            /// <summary></summary>
            //--------------------------------------------------------
            internal void ClearParameterInfos()
            {
                this.parameterList.Clear();
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.BindQueryExpression
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindQueryExpression(
            QUERYEXPRNODE queryNode,
            BindFlagsEnum bindFlags)
        {
            DebugUtil.Assert(
                queryNode != null &&
                queryNode.FromNode != null &&
                queryNode.QueryBodyNode != null);

            QueryExpressionInfo queryInfo = new QueryExpressionInfo();
            queryInfo.ToExpressionTrees = (bindFlags & BindFlagsEnum.AssignToIQueryable) != 0;
            bool br = false;

            try
            {
                CreateNewScope();
                br = BindFromClause(queryNode.FromNode, queryInfo);
            }
            finally
            {
                CloseScope();
            }

            if (br)
            {
                return BindQueryBody(queryNode.QueryBodyNode, queryInfo);
            }
            return null;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindQueryBody
        //
        /// <summary></summary>
        /// <param name="queryBodyNode"></param>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindQueryBody(
            BASENODE queryBodyNode,
            QueryExpressionInfo queryInfo)
        {
            if (queryBodyNode == null ||
                queryInfo == null ||
                queryInfo.CollectionExpr == null)
            {
                return null;
            }

            StatementListBuilder builder = new StatementListBuilder();

            BASENODE node = queryBodyNode;
            BASENODE clauseNode = null;
            bool exitLoop = false;

            while (!exitLoop && node != null)
            {
                if (node.Kind == NODEKIND.LIST)
                {
                    clauseNode = node.AsLIST.Operand1;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    clauseNode = node;
                    node = null;
                }

                switch (clauseNode.Kind)
                {
                    //------------------------------------------------
                    // from-clause
                    //------------------------------------------------
                    case NODEKIND.FROMCLAUSE:
                        DebugUtil.Assert(false);
                        break;

                    case NODEKIND.FROMCLAUSE2:
                        BindFromClause2(
                            clauseNode as FROMCLAUSENODE2,
                            queryInfo);
                        break;

                    //------------------------------------------------
                    // let-clause
                    //------------------------------------------------
                    case NODEKIND.LETCLAUSE:
                        BindLetClause(
                            clauseNode as LETCLAUSENODE,
                            queryInfo);
                        break;

                    //------------------------------------------------
                    // where-clause
                    //------------------------------------------------
                    case NODEKIND.WHERECLAUSE:
                        BindWhereClause(
                            clauseNode as WHERECLAUSENODE,
                            queryInfo);
                        break;

                    //------------------------------------------------
                    // join-clause, join-into-clause
                    //------------------------------------------------
                    case NODEKIND.JOINCLAUSE:
                        BindJoinClause(
                            clauseNode as JOINCLAUSENODE,
                            queryInfo);
                        break;

                    //------------------------------------------------
                    // orderby-clause
                    //------------------------------------------------
                    case NODEKIND.ORDERBYCLAUSE:
                        BindOrderByClause(
                            clauseNode as ORDERBYCLAUSENODE,
                            queryInfo);
                        break;

                    default:
                        exitLoop = true;
                        break;
                }
            }

            EXPR retExpr = null;

            if (clauseNode != null)
            {
                switch (clauseNode.Kind)
                {
                    //------------------------------------------------
                    // select-clause
                    //------------------------------------------------
                    case NODEKIND.SELECTCLAUSE:
                        retExpr = BindSelectClause(
                            clauseNode as SELECTCLAUSENODE,
                            queryInfo);
                        clauseNode = null;
                        break;

                    //------------------------------------------------
                    // group-clause
                    //------------------------------------------------
                    case NODEKIND.GROUPCLAUSE:
                        retExpr = BindGroupClause(
                            clauseNode as GROUPCLAUSENODE,
                            queryInfo);
                        clauseNode = null;
                        break;

                    default:
                        break;
                }
            }

            if (clauseNode == null && node != null)
            {
                clauseNode = node;
                node = null;
            }

            if (clauseNode != null)
            {
                //----------------------------------------------------
                // query-continuation
                //----------------------------------------------------
                if (clauseNode.Kind == NODEKIND.QUERYCONTINUATION)
                {
                    QUERYCONTINUATIONNODE contNode = clauseNode as QUERYCONTINUATIONNODE;

                    QueryExpressionInfo contQueryInfo = new QueryExpressionInfo();
                    contQueryInfo.AddParamter(
                        contNode.NameNode.Name,
                        queryInfo.ElementTypeSym,
                        false);
                    contQueryInfo.CollectionExpr = queryInfo.CollectionExpr;
                    contQueryInfo.ElementTypeSym = queryInfo.ElementTypeSym;

                    retExpr = BindQueryBody(
                        contNode.QueryBodyNode,
                        contQueryInfo);
                }
                else
                {
                    // error
                }
            }
            if (node != null)
            {
                // error messsage
            }

            return retExpr;
        }

        //============================================================
        // enum FUNCBREC.LinqMethodEnum
        //
        /// <summary></summary>
        //============================================================
        internal enum LinqMethodEnum : int
        {
            None = 0,
            GroupBy,
            GroupJoin,
            Join,
            OrderBy,
            OrderByDescending,
            Select,
            SelectMany,
            ThenBy,
            ThenByDescending,
            Where,
        }

        //------------------------------------------------------------
        // FUNCBREC.LinqMethodName
        //------------------------------------------------------------
        internal static string[] LinqMethodName =
        {
            null,
            "GroupBy",
            "GroupJoin",
            "Join",
            "OrderBy",
            "OrderByDescending",
            "Select",
            "SelectMany",
            "ThenBy",
            "ThenByDescending",
            "Where",
        };

        //------------------------------------------------------------
        // FUNCBREC.BindLinqMethod
        //
        /// <summary></summary>
        /// <param name="useQueryable"></param>
        /// <param name="methID"></param>
        /// <param name="treeNode"></param>
        /// <param name="typeArguments"></param>
        /// <param name="argsExpr"></param>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindLinqMethod(
            bool useQueryable,
            LinqMethodEnum methID,
            BASENODE treeNode,
            TypeArray typeArguments,
            EXPR argsExpr,
            //TYPESYM lambdaReturnTypeSym,
            QueryExpressionInfo queryInfo)
        {
            AGGTYPESYM methodContainerAts = null;
            AGGSYM methodContainerAggSym = null;
            EXPR methodContainerExpr = null;

            if (useQueryable)
            {
                methodContainerAts
                    = GetOptionalPredefinedType(PREDEFTYPE.QUERYABLE);
            }
            else
            {
                methodContainerAts
                    = GetOptionalPredefinedType(PREDEFTYPE.LINQ_ENUMERABLE);
            }
            methodContainerAggSym = methodContainerAts.GetAggregate();
            methodContainerExpr = NewExpr(null, EXPRKIND.CLASS, methodContainerAts);

            MemberLookup mem = new MemberLookup();
            string methodName = LinqMethodName[(int)methID];

            if (!mem.Lookup(
                    Compiler,
                    methodContainerExpr.TypeSym,
                    methodContainerExpr,
                    this.parentDeclSym,
                    methodName,
                    typeArguments.Count,
                    MemLookFlagsEnum.UserCallable))
            {
                DebugUtil.Assert(false);
                goto ERROR_PROCESSING;
            }

            EXPRMEMGRP grpExpr = NewExpr(
                null,
                EXPRKIND.MEMGRP,
                Compiler.MainSymbolManager.MethodGroupTypeSym) as EXPRMEMGRP;
            grpExpr.NameNode = null;
            grpExpr.Name = methodName;
            grpExpr.SymKind = SYMKIND.METHSYM;
            grpExpr.TypeArguments = typeArguments;
            grpExpr.ParentTypeSym = methodContainerExpr.TypeSym;
            grpExpr.MethPropSym = null;
            grpExpr.ObjectExpr = null;
            grpExpr.ContainingTypeArray = mem.GetAllTypes();
            grpExpr.Flags |= EXPRFLAG.USERCALLABLE;

            return BindGrpToArgs(
                treeNode,
                BindFlagsEnum.RValueRequired,
                grpExpr,
                argsExpr);

        ERROR_PROCESSING:
            return NewError(treeNode, null);
        }

        //------------------------------------------------------------
        // FUNCBREC.GetElementType
        //
        /// <summary></summary>
        /// <param name="typeNode"></param>
        /// <param name="expressionNode"></param>
        /// <param name="collectionTypeSym"></param>
        /// <param name="collectionExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private TYPESYM GetElementType(
            TYPEBASENODE typeNode,
            BASENODE expressionNode,
            TYPESYM collectionTypeSym,
            EXPR collectionExpr)
        {
            TYPESYM elementTypeSym = null;

            //--------------------------------------------------------
            // (1) If specified,
            //--------------------------------------------------------
            if (typeNode != null && typeNode.Kind != NODEKIND.IMPLICITTYPE)
            {
                elementTypeSym = BindType(typeNode);
            }
            //--------------------------------------------------------
            // (2) array
            //--------------------------------------------------------
            else if (collectionTypeSym.IsARRAYSYM)
            {
                elementTypeSym = (collectionTypeSym as ARRAYSYM).ElementTypeSym;
            }
            //--------------------------------------------------------
            // (3) string
            //--------------------------------------------------------
            else if (collectionTypeSym.IsPredefType(PREDEFTYPE.STRING))
            {
                elementTypeSym = GetRequiredPredefinedType(PREDEFTYPE.CHAR);
            }
            //--------------------------------------------------------
            // (4) IEnumerable
            //--------------------------------------------------------
            else
            {
                EXPR callGetEnumExpr = BindPatternToMethod(
                    expressionNode,
                    PREDEFNAME.GETENUMERATOR,
                    collectionTypeSym,
                    collectionExpr,
                    null,
                    ResNo.CSCSTR_Collection);
                if (callGetEnumExpr == null)
                {
                    return null;
                }

                TYPESYM enumratorTypeSym = callGetEnumExpr.TypeSym;

                EXPRWRAP atorWrapExpr = NewExprWrap(callGetEnumExpr, TEMP_KIND.FOREACH_GETENUM);
                atorWrapExpr.Flags |= EXPRFLAG.WRAPASTEMP;
                atorWrapExpr.DoNotFree = true;

                MemberLookup mem = new MemberLookup();
                if (!mem.Lookup(
                    Compiler,
                    enumratorTypeSym,
                    atorWrapExpr,
                    this.parentDeclSym,
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.CURRENT),
                    0,
                    MemLookFlagsEnum.None))
                {
                    mem.ReportErrors(expressionNode);
                    //goto LBadRetType;
                    return null;
                }
                if (!mem.FirstSym.IsPROPSYM ||
                    mem.FirstSym.Access != ACCESS.PUBLIC ||
                    (mem.FirstSym as PROPSYM).IsStatic)
                {
                    //goto LBadRetType;
                    return null;
                }

                EXPR callCurrentExpr = BindToProperty(
                    expressionNode,
                    atorWrapExpr,
                    PropWithType.Convert(mem.FirstSymWithType),
                    BindFlagsEnum.RValueRequired, null, null);
                if (!callCurrentExpr.IsOK)
                {
                    //goto LBadRetType;
                    return null;
                }

                elementTypeSym = callCurrentExpr.TypeSym;
            }

            return elementTypeSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindFromClause
        //
        /// <summary></summary>
        /// <param name="fromNode"></param>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BindFromClause(
            FROMCLAUSENODE fromNode,
            QueryExpressionInfo queryInfo)
        {
            //--------------------------------------------------------
            // (1) Bind the source collection.
            //--------------------------------------------------------
            EXPR srcCollectionExpr = BindExpr(
                fromNode.ExpressionNode,
                BindFlagsEnum.RValueRequired);

            TYPESYM srcCollectionTypeSym = srcCollectionExpr.TypeSym;
            if (srcCollectionTypeSym == null)
            {
                return false;
            }

            queryInfo.CollectionExpr = srcCollectionExpr;

            queryInfo.ElementTypeSym = GetElementType(
                fromNode.ParameterNode.TypeNode,
                fromNode.ExpressionNode,
                srcCollectionTypeSym,
                srcCollectionExpr);

            queryInfo.AddParamter(
                fromNode.ParameterNode,
                queryInfo.ElementTypeSym);

            return (queryInfo.ElementTypeSym != null);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindFromClause2
        //
        /// <summary></summary>
        /// <param name="fromNode"></param>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BindFromClause2(
            FROMCLAUSENODE2 fromNode,
            QueryExpressionInfo queryInfo)
        {
            // public static IEnumerable<TResult>
            // SelectMany<TSource, TCollection, TResult>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, IEnumerable<TCollection>> collectionSelector,
            //     Func<TSource, TCollection, TResult> resultSelector
            // )

            // public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(
            //     this IQueryable<TSource> source,
            //     Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector,
            //     Expression<Func<TSource, TCollection, TResult>> resultSelector
            // )

            if (!queryInfo.IsOK)
            {
                return false;
            }
            DebugUtil.Assert(fromNode.ExpressionNode.Kind == NODEKIND.LAMBDAEXPR);

            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            //--------------------------------------------------------
            // IEnumerable<TSource> source
            //--------------------------------------------------------
            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // Func<TSource, IEnumerable<TCollection>> collectionSelector or
            // Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector
            //--------------------------------------------------------
            List<PARAMINFO> parameterList = new List<PARAMINFO>();
            parameterList.AddRange(queryInfo.ParameterList);

            EXPRLAMBDAEXPR collectSelectExpr = BindLambdaExpression2(
                fromNode.ExpressionNode as LAMBDAEXPRNODE,
                parameterList) as EXPRLAMBDAEXPR;

            TYPESYM collectElemTypeSym = GetElementType(
                fromNode.ParameterNode.TypeNode,
                fromNode.ExpressionNode,
                collectSelectExpr.AnonymousMethodInfo.ReturnTypeSym,
                collectSelectExpr);

            if (collectElemTypeSym == null)
            {
                return false;
            }

            if (queryInfo.ToExpressionTrees)
            {
                AGGSYM ienumAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G_IENUMERABLE, true);
                TypeArray ienumTypeArgs = new TypeArray();
                ienumTypeArgs.Add(collectElemTypeSym);
                ienumTypeArgs = Compiler.MainSymbolManager.AllocParams(ienumTypeArgs);

                TYPESYM ienumCollectTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                    ienumAggSym,
                    ienumTypeArgs);

                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(queryInfo.ElementTypeSym);
                funcTypeArgs.Add(ienumCollectTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                TYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                CreateExpressionTree(
                    fromNode.ExpressionNode,
                    collectSelectExpr,
                    expressionAts,
                    ref exTreeExpr,
                    true);

                NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
            else
            {
                NewList(collectSelectExpr, ref firstArgsExpr, ref lastArgsExpr);
            }

            //--------------------------------------------------------
            // Func<TSource, TCollection, TResult> resultSelector or
            // Expression<Func<TSource, TCollection, TResult>> resultSelector
            //--------------------------------------------------------
            QueryExpressionInfo innerQueryInfo = new QueryExpressionInfo();
            innerQueryInfo.AddParamter(fromNode.ParameterNode, collectElemTypeSym);
            parameterList.AddRange(innerQueryInfo.ParameterList);

            EXPRLAMBDAEXPR resSelectExpr = BindLambdaExpression2(
                fromNode.OutputLambdaExpressionNode as LAMBDAEXPRNODE,
                parameterList) as EXPRLAMBDAEXPR;
            TYPESYM resTypeSym = resSelectExpr.AnonymousMethodInfo.ReturnTypeSym;

            if (queryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G3_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(queryInfo.ElementTypeSym);
                funcTypeArgs.Add(collectElemTypeSym);
                funcTypeArgs.Add(resTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                CreateExpressionTree(
                    fromNode.OutputLambdaExpressionNode,
                    resSelectExpr,
                    expressionAts,
                    ref exTreeExpr,
                    true);

                NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
            else
            {
                NewList(resSelectExpr, ref firstArgsExpr, ref lastArgsExpr);
            }

            //--------------------------------------------------------
            // <TSource, TCollection, TResult>
            //--------------------------------------------------------
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);
            typeArgs.Add(collectElemTypeSym);
            typeArgs.Add(resTypeSym);
            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            //----------------------------------------------------
            // Bind to SelectMany
            //----------------------------------------------------
            queryInfo.CollectionExpr = BindLinqMethod(
                queryInfo.ToExpressionTrees,
                LinqMethodEnum.SelectMany,
                fromNode,
                typeArgs,
                firstArgsExpr,
                queryInfo);
            queryInfo.ElementTypeSym = resTypeSym;

            queryInfo.ClearParameterInfos();
            queryInfo.AddParamter(
                fromNode.ParameterProductName,
                resTypeSym,
                true);

            return true;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindLetClause
        //
        /// <summary></summary>
        /// <param name="letNode"></param>
        /// <param name="queryInfo"></param>
        //------------------------------------------------------------
#if false   // old
        internal void BindLetClause(
            LETCLAUSENODE letNode,
            QueryExpressionInfo queryInfo)
        {
            // public static IEnumerable<TResult> Select<TSource, TResult>(
            // 	   this IEnumerable<TSource> source,
            // 	   Func<TSource, TResult> selector
            // )

            // public static IQueryable<TResult> Select<TSource, TResult>(
            // 	this IQueryable<TSource> source,
            // 	Expression<Func<TSource, TResult>> selector
            // )

            if (!queryInfo.IsOK)
            {
                return;
            }

            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            //--------------------------------------------------------
            // IEnumerable<TSource> source
            //--------------------------------------------------------
            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // Func<TSource, TResult> selector
            //--------------------------------------------------------
            List<PARAMINFO> parameterList = new List<PARAMINFO>();
            parameterList.AddRange(queryInfo.ParameterList);

            EXPRLAMBDAEXPR selectExpr = BindLambdaExpression2(
                letNode.OutputLambdaExpressionNode,
                parameterList) as EXPRLAMBDAEXPR;

            NewList(selectExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // <TSource, TResult>
            //--------------------------------------------------------
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);
            TYPESYM resTypeSym = selectExpr.AnonymousMethodInfo.ReturnTypeSym;
            typeArgs.Add(resTypeSym);
            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            //----------------------------------------------------
            // Bind to Select
            //----------------------------------------------------
            queryInfo.CollectionExpr = BindLinqMethod(
                false,
                LinqMethodEnum.Select,
                letNode,
                typeArgs,
                firstArgsExpr,
                queryInfo);
            queryInfo.ElementTypeSym = resTypeSym;

            queryInfo.ClearParameterInfos();
            queryInfo.AddParamter(
                letNode.ParameterProductName,
                resTypeSym,
                true);
        }
#endif
        internal void BindLetClause(
            LETCLAUSENODE letNode,
            QueryExpressionInfo queryInfo)
        {
            // public static IEnumerable<TResult> Select<TSource, TResult>(
            // 	   this IEnumerable<TSource> source,
            // 	   Func<TSource, TResult> selector
            // )

            // public static IQueryable<TResult> Select<TSource, TResult>(
            // 	this IQueryable<TSource> source,
            // 	Expression<Func<TSource, TResult>> selector
            // )

            if (!queryInfo.IsOK)
            {
                return;
            }

            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            //--------------------------------------------------------
            // IEnumerable<TSource> source
            //--------------------------------------------------------
            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // Func<TSource, TResult> or Expression<Func<TSource, TResult>> selector
            //--------------------------------------------------------
            List<PARAMINFO> parameterList = new List<PARAMINFO>();
            parameterList.AddRange(queryInfo.ParameterList);

            EXPRLAMBDAEXPR selectExpr = BindLambdaExpression2(
                letNode.OutputLambdaExpressionNode,
                parameterList) as EXPRLAMBDAEXPR;
            TYPESYM resTypeSym = selectExpr.AnonymousMethodInfo.ReturnTypeSym;

            if (queryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(queryInfo.ElementTypeSym);
                funcTypeArgs.Add(resTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                CreateExpressionTree(
                    letNode.OutputLambdaExpressionNode,
                    selectExpr,
                    expressionAts,
                    ref exTreeExpr,
                    true);

                NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
            else
            {
                NewList(selectExpr, ref firstArgsExpr, ref lastArgsExpr);
            }

            //--------------------------------------------------------
            // <TSource, TResult>
            //--------------------------------------------------------
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);
            typeArgs.Add(resTypeSym);
            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            //----------------------------------------------------
            // Bind to Select
            //----------------------------------------------------
            queryInfo.CollectionExpr = BindLinqMethod(
                queryInfo.ToExpressionTrees,
                LinqMethodEnum.Select,
                letNode,
                typeArgs,
                firstArgsExpr,
                queryInfo);
            queryInfo.ElementTypeSym = resTypeSym;

            queryInfo.ClearParameterInfos();
            queryInfo.AddParamter(
                letNode.ParameterProductName,
                resTypeSym,
                true);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindWhereClause
        //
        /// <summary></summary>
        /// <param name="whereNode"></param>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void BindWhereClause(
            WHERECLAUSENODE whereNode,
            QueryExpressionInfo queryInfo)
        {
            // public static IEnumerable<TSource> Where<TSource>(
            // 	this IEnumerable<TSource> source,
            // 	Func<TSource, bool> predicate
            // )

            if (!queryInfo.IsOK)
            {
                return;
            }

            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // Bind the expression (1) Create a EXPRLAMBDAEXPR instance.
            //--------------------------------------------------------
            EXPRLAMBDAEXPR whereLambdaExpr = BindLambdaExpression(
                whereNode.LambdaExpressionNode,
                queryInfo.ParameterList) as EXPRLAMBDAEXPR;

            if (whereLambdaExpr == null)
            {
                return;
            }

            //--------------------------------------------------------
            // Bind the expression (2-1) To expression trees
            //--------------------------------------------------------
            if (queryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(queryInfo.ElementTypeSym);
                funcTypeArgs.Add(Compiler.GetReqPredefType(PREDEFTYPE.BOOL, true));
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                if (!CreateExpressionTree(
                        whereNode.LambdaExpressionNode,
                        whereLambdaExpr,
                        expressionAts,
                        ref exTreeExpr,
                        true))
                {
                    goto ERROR_PROCESSING;
                }

                NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
            //--------------------------------------------------------
            // Bind the expression (2-2) Otherwise
            //--------------------------------------------------------
            else
            {
                BindLambdaExpressionInner(
                    whereLambdaExpr.AnonymousMethodInfo);
                if (!whereLambdaExpr.AnonymousMethodInfo.Compiled)
                {
                    goto ERROR_PROCESSING;
                }

                NewList(whereLambdaExpr, ref firstArgsExpr, ref lastArgsExpr);
            }

            //--------------------------------------------------------
            // Bind Enumerable or Queryable method.
            //--------------------------------------------------------
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);
            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            queryInfo.CollectionExpr =
                BindLinqMethod(
                    queryInfo.ToExpressionTrees,
                    LinqMethodEnum.Where,
                    whereNode,
                    typeArgs,
                    firstArgsExpr,
                    queryInfo);
            return;

        ERROR_PROCESSING:
            queryInfo.CollectionExpr = NewError(whereNode, null);
            return;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindJoinClause
        //
        /// <summary></summary>
        /// <param name="joinNode"></param>
        /// <param name="queryInfo"></param>
        //------------------------------------------------------------
        internal void BindJoinClause(
            JOINCLAUSENODE joinNode,
            QueryExpressionInfo outerQueryInfo)
        {
            // public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            // 	this IEnumerable<TOuter> outer,
            // 	IEnumerable<TInner> inner,
            // 	Func<TOuter, TKey> outerKeySelector,
            // 	Func<TInner, TKey> innerKeySelector,
            // 	Func<TOuter, TInner, TResult> resultSelector
            // )

            // public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            // 	this IEnumerable<TOuter> outer,
            // 	IEnumerable<TInner> inner,
            // 	Func<TOuter, TKey> outerKeySelector,
            // 	Func<TInner, TKey> innerKeySelector,
            // 	Func<TOuter, IEnumerable<TInner>, TResult> resultSelector
            // )

            // public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
            //     this IQueryable<TOuter> outer,
            //     IEnumerable<TInner> inner,
            //     Expression<Func<TOuter, TKey>> outerKeySelector,
            //     Expression<Func<TInner, TKey>> innerKeySelector,
            //     Expression<Func<TOuter, TInner, TResult>> resultSelector
            // )

            if (!outerQueryInfo.IsOK)
            {
                return;
            }

            DebugUtil.Assert(joinNode != null && outerQueryInfo != null);

            AGGTYPESYM gIEnumerableAts = GetOptionalPredefinedType(PREDEFTYPE.G_IENUMERABLE);
            AGGSYM gIEnumerableAggSym = gIEnumerableAts.GetAggregate();

            EXPR argExpr = null;
            EXPR argExprLast = null;
            TypeArray typeArgArray = new TypeArray();

            //--------------------------------------------------------
            // TOuter, Argument 1
            //--------------------------------------------------------
            NewList(outerQueryInfo.CollectionExpr, ref argExpr, ref argExprLast);
            typeArgArray.Add(outerQueryInfo.ElementTypeSym);

            //--------------------------------------------------------
            // TInner, Argument 2
            //--------------------------------------------------------
            QueryExpressionInfo innerQueryInfo = new QueryExpressionInfo();
            BindFromClause(joinNode.InNode, innerQueryInfo);

            //innerTypeSym = innerQueryInfo.ParameterList[0].TypeSym;

            NewList(innerQueryInfo.CollectionExpr, ref argExpr, ref argExprLast);
            typeArgArray.Add(innerQueryInfo.ParameterList[0].TypeSym);

#if false // (old) TKey, Argument 3 and 4
            //--------------------------------------------------------
            // TKey, argument 3 and 4
            //--------------------------------------------------------
            List<PARAMINFO> parameterList = new List<PARAMINFO>();
            parameterList.AddRange(outerQueryInfo.ParameterList);

            EXPRLAMBDAEXPR tempArgExpr = BindLambdaExpression2(
                joinNode.EqualLeftLambdaExpressionNode,
                parameterList) as EXPRLAMBDAEXPR;

            if (tempArgExpr == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_QueryTypeInferenceFailed,
                    new ErrArg("Join"));
            }
            AnonMethInfo tempAnonInfo = tempArgExpr.AnonymousMethodInfo;
            TYPESYM keyTypeSym = tempAnonInfo.ReturnTypeSym;

            NewList(tempArgExpr, ref argExpr, ref argExprLast);

            parameterList.Clear();
            parameterList.AddRange(innerQueryInfo.ParameterList);

            tempArgExpr = BindLambdaExpression2(
                joinNode.EqualRightLambdaExpressionNode,
                parameterList) as EXPRLAMBDAEXPR;

            if (tempArgExpr == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_QueryTypeInferenceFailed,
                    new ErrArg("Join"));
            }
            tempAnonInfo = tempArgExpr.AnonymousMethodInfo;

            if (keyTypeSym != tempAnonInfo.ReturnTypeSym)
            {
                Compiler.Error(
                    CSCERRID.ERR_QueryTypeInferenceFailed,
                    new ErrArg("Join"));
            }

            NewList(tempArgExpr, ref argExpr, ref argExprLast);
            typeArgArray.Add(keyTypeSym);
#endif
            //--------------------------------------------------------
            // TKey, Argument 3
            //--------------------------------------------------------
            List<PARAMINFO> parameterList = new List<PARAMINFO>();
            parameterList.AddRange(outerQueryInfo.ParameterList);

            EXPRLAMBDAEXPR tempArgExpr = BindLambdaExpression2(
                joinNode.EqualLeftLambdaExpressionNode,
                parameterList) as EXPRLAMBDAEXPR;

            if (tempArgExpr == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_QueryTypeInferenceFailed,
                    new ErrArg("Join"));
            }
            AnonMethInfo tempAnonInfo = tempArgExpr.AnonymousMethodInfo;
            TYPESYM keyTypeSym = tempAnonInfo.ReturnTypeSym;

            if (outerQueryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(outerQueryInfo.ElementTypeSym);
                funcTypeArgs.Add(keyTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                CreateExpressionTree(
                    joinNode.EqualLeftLambdaExpressionNode,
                    tempArgExpr,
                    expressionAts,
                    ref exTreeExpr,
                    true);

                NewList(exTreeExpr, ref argExpr, ref argExprLast);
            }
            else
            {
                NewList(tempArgExpr, ref argExpr, ref argExprLast);
            }

            //--------------------------------------------------------
            // Argument 4
            //--------------------------------------------------------
            parameterList.Clear();
            parameterList.AddRange(innerQueryInfo.ParameterList);

            tempArgExpr = BindLambdaExpression2(
                joinNode.EqualRightLambdaExpressionNode,
                parameterList) as EXPRLAMBDAEXPR;

            if (tempArgExpr == null)
            {
                Compiler.Error(
                    CSCERRID.ERR_QueryTypeInferenceFailed,
                    new ErrArg("Join"));
            }
            tempAnonInfo = tempArgExpr.AnonymousMethodInfo;

            if (keyTypeSym != tempAnonInfo.ReturnTypeSym)
            {
                Compiler.Error(
                    CSCERRID.ERR_QueryTypeInferenceFailed,
                    new ErrArg("Join"));
            }

            if (outerQueryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(innerQueryInfo.ElementTypeSym);
                funcTypeArgs.Add(keyTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                CreateExpressionTree(
                    joinNode.EqualRightLambdaExpressionNode,
                    tempArgExpr,
                    expressionAts,
                    ref exTreeExpr,
                    true);

                NewList(exTreeExpr, ref argExpr, ref argExprLast);
            }
            else
            {
                NewList(tempArgExpr, ref argExpr, ref argExprLast);
            }

            typeArgArray.Add(keyTypeSym);

#if false // (old) TResult, Argument 5 and Bind
            //--------------------------------------------------------
            // TResult, Argument 5 and Bind (1) Join
            //--------------------------------------------------------
            if (!joinNode.IsJoinIntoClause)
            {
                parameterList.Clear();
                parameterList.AddRange(outerQueryInfo.ParameterList);
                parameterList.AddRange(innerQueryInfo.ParameterList);

                tempArgExpr = BindLambdaExpression2(
                    joinNode.OutputLambdaExpressionNode,
                    parameterList) as EXPRLAMBDAEXPR;
                tempAnonInfo = tempArgExpr.AnonymousMethodInfo;
                TYPESYM resTypeSym = tempAnonInfo.ReturnTypeSym;

                NewList(tempArgExpr, ref argExpr, ref argExprLast);
                typeArgArray.Add(resTypeSym);
                typeArgArray = Compiler.MainSymbolManager.AllocParams(typeArgArray);

                //----------------------------------------------------
                // Bind Join
                //----------------------------------------------------
                outerQueryInfo.CollectionExpr = BindLinqMethod(
                    false,
                    LinqMethodEnum.Join,
                    joinNode,
                    typeArgArray,
                    argExpr,
                    outerQueryInfo);
                outerQueryInfo.ElementTypeSym = resTypeSym;

                outerQueryInfo.ClearParameterInfos();
                outerQueryInfo.AddParamter(
                    joinNode.ParameterProductName,
                    resTypeSym,
                    true);
            }
            //--------------------------------------------------------
            // TResult, Argument 5 and Bind (2) GroupJoin
            //--------------------------------------------------------
            else
            {
                parameterList.Clear();
                parameterList.AddRange(outerQueryInfo.ParameterList);
                DebugUtil.Assert(innerQueryInfo.ParameterCount > 0);

                TYPESYM innerTypeSym = innerQueryInfo.ParameterList[0].TypeSym;
                TypeArray ta = new TypeArray();
                ta.Add(innerTypeSym);
                ta = Compiler.MainSymbolManager.AllocParams(ta);

                TYPESYM ienumSym = GetOptionalPredefinedType(PREDEFTYPE.G_IENUMERABLE);
                TYPESYM intoTypeSym
                    = Compiler.MainSymbolManager.GetInstAgg(ienumSym.GetAggregate(), ta);

                PARAMINFO intoParamInfo = new PARAMINFO();
                intoParamInfo.TypeSym = intoTypeSym;
                intoParamInfo.Name = joinNode.IntoNameNode.Name;

                parameterList.Add(intoParamInfo);

                tempArgExpr = BindLambdaExpression2(
                    joinNode.OutputLambdaExpressionNode,
                    parameterList) as EXPRLAMBDAEXPR;
                tempAnonInfo = tempArgExpr.AnonymousMethodInfo;
                TYPESYM resTypeSym = tempAnonInfo.ReturnTypeSym;

                NewList(tempArgExpr, ref argExpr, ref argExprLast);
                typeArgArray.Add(resTypeSym);
                typeArgArray = Compiler.MainSymbolManager.AllocParams(typeArgArray);

                //----------------------------------------------------
                // Bind GroupJoin
                //----------------------------------------------------
                outerQueryInfo.CollectionExpr = BindLinqMethod(
                    false,
                    LinqMethodEnum.GroupJoin,
                    joinNode,
                    typeArgArray,
                    argExpr,
                    outerQueryInfo);
                outerQueryInfo.ElementTypeSym = resTypeSym;

                outerQueryInfo.ClearParameterInfos();
                outerQueryInfo.AddParamter(
                    joinNode.ParameterProductName,
                    resTypeSym,
                    true);
            }
#endif
            //--------------------------------------------------------
            // TResult, Argument 5 and Bind (1) Join
            //--------------------------------------------------------
            if (!joinNode.IsJoinIntoClause)
            {
                parameterList.Clear();
                parameterList.AddRange(outerQueryInfo.ParameterList);
                parameterList.AddRange(innerQueryInfo.ParameterList);

                tempArgExpr = BindLambdaExpression2(
                    joinNode.OutputLambdaExpressionNode,
                    parameterList) as EXPRLAMBDAEXPR;
                tempAnonInfo = tempArgExpr.AnonymousMethodInfo;
                TYPESYM resTypeSym = tempAnonInfo.ReturnTypeSym;

                if (outerQueryInfo.ToExpressionTrees)
                {
                    AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G3_FUNC, true);
                    TypeArray funcTypeArgs = new TypeArray();
                    funcTypeArgs.Add(outerQueryInfo.ElementTypeSym);
                    funcTypeArgs.Add(innerQueryInfo.ElementTypeSym);
                    funcTypeArgs.Add(resTypeSym);
                    funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                    AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                        funcAggSym,
                        funcTypeArgs);

                    AGGSYM expressionAggSym
                        = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                    TypeArray exTypeArgs = new TypeArray();
                    exTypeArgs.Add(funcAts);
                    exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                    AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                        expressionAggSym,
                        exTypeArgs);

                    EXPR exTreeExpr = null;
                    CreateExpressionTree(
                        joinNode.OutputLambdaExpressionNode,
                        tempArgExpr,
                        expressionAts,
                        ref exTreeExpr,
                        true);

                    NewList(exTreeExpr, ref argExpr, ref argExprLast);
                }
                else
                {
                    NewList(tempArgExpr, ref argExpr, ref argExprLast);
                }

                typeArgArray.Add(resTypeSym);
                typeArgArray = Compiler.MainSymbolManager.AllocParams(typeArgArray);

                //----------------------------------------------------
                // Bind Join
                //----------------------------------------------------
                outerQueryInfo.CollectionExpr = BindLinqMethod(
                    outerQueryInfo.ToExpressionTrees,
                    LinqMethodEnum.Join,
                    joinNode,
                    typeArgArray,
                    argExpr,
                    outerQueryInfo);
                outerQueryInfo.ElementTypeSym = resTypeSym;

                outerQueryInfo.ClearParameterInfos();
                outerQueryInfo.AddParamter(
                    joinNode.ParameterProductName,
                    resTypeSym,
                    true);
            }
            //--------------------------------------------------------
            // TResult, Argument 5 and Bind (2) GroupJoin
            //--------------------------------------------------------
            else
            {
                parameterList.Clear();
                parameterList.AddRange(outerQueryInfo.ParameterList);
                DebugUtil.Assert(innerQueryInfo.ParameterCount > 0);

                TYPESYM innerTypeSym = innerQueryInfo.ParameterList[0].TypeSym;
                TypeArray ta = new TypeArray();
                ta.Add(innerTypeSym);
                ta = Compiler.MainSymbolManager.AllocParams(ta);

                TYPESYM ienumSym = GetOptionalPredefinedType(PREDEFTYPE.G_IENUMERABLE);
                TYPESYM intoTypeSym
                    = Compiler.MainSymbolManager.GetInstAgg(ienumSym.GetAggregate(), ta);

                PARAMINFO intoParamInfo = new PARAMINFO();
                intoParamInfo.TypeSym = intoTypeSym;
                intoParamInfo.Name = joinNode.IntoNameNode.Name;

                parameterList.Add(intoParamInfo);

                tempArgExpr = BindLambdaExpression2(
                    joinNode.OutputLambdaExpressionNode,
                    parameterList) as EXPRLAMBDAEXPR;
                tempAnonInfo = tempArgExpr.AnonymousMethodInfo;
                TYPESYM resTypeSym = tempAnonInfo.ReturnTypeSym;

                if (outerQueryInfo.ToExpressionTrees)
                {
                    AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                    TypeArray funcTypeArgs = new TypeArray();
                    funcTypeArgs.Add(outerQueryInfo.ElementTypeSym);

                    AGGSYM ienumAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G_IENUMERABLE, true);
                    TypeArray ienumTypeArgs = new TypeArray();
                    ienumTypeArgs.Add(innerQueryInfo.ElementTypeSym);
                    ienumTypeArgs = Compiler.MainSymbolManager.AllocParams(ienumTypeArgs);
                    AGGTYPESYM ienumInnerTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                        ienumAggSym,
                        ienumTypeArgs);
                    funcTypeArgs.Add(ienumInnerTypeSym);

                    funcTypeArgs.Add(resTypeSym);
                    funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                    AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                        funcAggSym,
                        funcTypeArgs);

                    AGGSYM expressionAggSym
                        = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                    TypeArray exTypeArgs = new TypeArray();
                    exTypeArgs.Add(funcAts);
                    exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                    AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                        expressionAggSym,
                        exTypeArgs);

                    EXPR exTreeExpr = null;
                    CreateExpressionTree(
                        joinNode.OutputLambdaExpressionNode,
                        tempArgExpr,
                        expressionAts,
                        ref exTreeExpr,
                        true);

                    NewList(exTreeExpr, ref argExpr, ref argExprLast);
                }
                else
                {
                    NewList(tempArgExpr, ref argExpr, ref argExprLast);
                }

                typeArgArray.Add(resTypeSym);
                typeArgArray = Compiler.MainSymbolManager.AllocParams(typeArgArray);

                //----------------------------------------------------
                // Bind GroupJoin
                //----------------------------------------------------
                outerQueryInfo.CollectionExpr = BindLinqMethod(
                    outerQueryInfo.ToExpressionTrees,
                    LinqMethodEnum.GroupJoin,
                    joinNode,
                    typeArgArray,
                    argExpr,
                    outerQueryInfo);
                outerQueryInfo.ElementTypeSym = resTypeSym;

                outerQueryInfo.ClearParameterInfos();
                outerQueryInfo.AddParamter(
                    joinNode.ParameterProductName,
                    resTypeSym,
                    true);
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.BindOrderByClause
        //
        /// <summary></summary>
        /// <param name="orderbyNode"></param>
        /// <param name="queryInfo"></param>
        //------------------------------------------------------------
        internal void BindOrderByClause(
            ORDERBYCLAUSENODE orderbyNode,
            QueryExpressionInfo queryInfo)
        {
            // public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, TKey> keySelector
            // )

            // public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, TKey> keySelector
            // )

            // public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            //     this IOrderedEnumerable<TSource> source,
            //     Func<TSource, TKey> keySelector
            // )

            // public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            //     this IOrderedEnumerable<TSource> source,
            //     Func<TSource, TKey> keySelector
            // )

            if (!queryInfo.IsOK)
            {
                return;
            }

            ORDERBYCLAUSENODE.Ordering ordering
                = orderbyNode.FirstOrdering;
            if (ordering == null)
            {
                return;
            }

            if (ordering.ByDesending)
            {
                BindOrderByClauseCore(
                    ordering.LambdaExpressionNode,
                    LinqMethodEnum.OrderByDescending,
                    queryInfo);
            }
            else
            {
                BindOrderByClauseCore(
                    ordering.LambdaExpressionNode,
                    LinqMethodEnum.OrderBy,
                    queryInfo);
            }

            ordering = ordering.Next;
            while (ordering != null)
            {
                if (ordering.ByDesending)
                {
                    BindOrderByClauseCore(
                        ordering.LambdaExpressionNode,
                        LinqMethodEnum.ThenByDescending,
                        queryInfo);
                }
                else
                {
                    BindOrderByClauseCore(
                        ordering.LambdaExpressionNode,
                        LinqMethodEnum.ThenBy,
                        queryInfo);
                }
                ordering = ordering.Next;
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.BindOrderByClauseCore
        //
        /// <summary></summary>
        /// <param name="lambdaNode"></param>
        /// <param name="methodID"></param>
        /// <param name="queryInfo"></param>
        //------------------------------------------------------------
        internal void BindOrderByClauseCore(
            LAMBDAEXPRNODE lambdaNode,
            LinqMethodEnum methodID,
            QueryExpressionInfo queryInfo)
        {
            //--------------------------------------------------------
            // Get the arguments (1) source
            //--------------------------------------------------------
            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // Bind the expression.
            //--------------------------------------------------------
            EXPRLAMBDAEXPR orderbyLambdaExpr = BindLambdaExpression(
                lambdaNode,
                queryInfo.ParameterList) as EXPRLAMBDAEXPR;
            if (orderbyLambdaExpr == null)
            {
                goto ERROR_PROCESSING;
            }

            BindLambdaExpressionInner(
                orderbyLambdaExpr.AnonymousMethodInfo);
            if (!orderbyLambdaExpr.AnonymousMethodInfo.Compiled)
            {
                goto ERROR_PROCESSING;
            }

            TYPESYM lambdaReturnTypeSym
                = orderbyLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;

            //--------------------------------------------------------
            // Get the arguments (2) keySelector
            //--------------------------------------------------------
            if (queryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(queryInfo.ElementTypeSym);
                funcTypeArgs.Add(lambdaReturnTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                if (!CreateExpressionTree(
                        lambdaNode,
                        orderbyLambdaExpr,
                        expressionAts,
                        ref exTreeExpr,
                        true))
                {
                    goto ERROR_PROCESSING;
                }

                NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
            else
            {
                NewList(orderbyLambdaExpr, ref firstArgsExpr, ref lastArgsExpr);
            }

            //--------------------------------------------------------
            // Bind System.Linq.Enumerable method.
            //--------------------------------------------------------
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);
            typeArgs.Add(lambdaReturnTypeSym);
            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            queryInfo.CollectionExpr =
                BindLinqMethod(
                    queryInfo.ToExpressionTrees,
                    methodID,
                    lambdaNode,
                    typeArgs,
                    firstArgsExpr,
                    queryInfo);
            return;

        ERROR_PROCESSING:
            queryInfo.CollectionExpr = NewError(lambdaNode, null);
            return;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindSelectClause
        //
        /// <summary></summary>
        /// <param name="selectNode"></param>
        /// <param name="elementTypeSym"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal EXPR BindSelectClause(
            SELECTCLAUSENODE selectNode,
            QueryExpressionInfo queryInfo)
        {
            if (!queryInfo.IsOK)
            {
                return queryInfo.CollectionExpr;
            }

            LAMBDAEXPRNODE lambdaNode = selectNode.LambdaExpressionNode;

            //--------------------------------------------------------
            // If returns the argument as it is.
            //--------------------------------------------------------
            STATEMENTNODE stmtNode = lambdaNode.BodyNode.StatementsNode;
            if (stmtNode.Kind == NODEKIND.RETURN &&
                stmtNode.NextNode == null &&
                (stmtNode as EXPRSTMTNODE).ArgumentsNode.Kind == NODEKIND.NAME &&
                queryInfo.ParameterList.Count == 1)
            {
                NAMENODE nameNode = (stmtNode as EXPRSTMTNODE).ArgumentsNode as NAMENODE;
                if (nameNode.Name == queryInfo.ParameterList[0].Name)
                {
                    return queryInfo.CollectionExpr;
                }
            }

            //--------------------------------------------------------
            // The first argument of Enumerable or Queryable method.
            //--------------------------------------------------------
            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // Bind the expression and get the type of the return value.
            //--------------------------------------------------------
            EXPRLAMBDAEXPR selectLambdaExpr = BindLambdaExpression(
                selectNode.LambdaExpressionNode,
                queryInfo.ParameterList) as EXPRLAMBDAEXPR;

            //selectLambdaExpr = BindLambdaExpressionInner(
            //    selectLambdaExpr.AnonymousMethodInfo) as EXPRLAMBDAEXPR;
            BindLambdaExpressionInner(
                selectLambdaExpr.AnonymousMethodInfo);

            if (selectLambdaExpr == null)
            {
                return null;
            }
            TYPESYM lambdaReturnTypeSym
                = selectLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;

            if (queryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(queryInfo.ElementTypeSym);
                funcTypeArgs.Add(lambdaReturnTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                CreateExpressionTree(
                    selectNode.LambdaExpressionNode,
                    selectLambdaExpr,
                    expressionAts,
                    ref exTreeExpr,
                    true);

                NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
            else
            {
                NewList(selectLambdaExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
#if false
            //--------------------------------------------------------
            // Get the return type of the select-clause.
            //--------------------------------------------------------
            AGGTYPESYM ireturnAts = null;

            if (queryInfo.ToExpressionTrees)
            {
                ireturnAts = GetOptionalPredefinedType(PREDEFTYPE.G_IQUERYABLE);
            }
            else
            {
                ireturnAts = GetOptionalPredefinedType(PREDEFTYPE.G_IENUMERABLE);
            }

            AGGSYM ireturnAggSym = ireturnAts.GetAggregate();
            TypeArray tempTypeArray = new TypeArray();
            tempTypeArray.Add(lambdaReturnTypeSym);
            tempTypeArray = Compiler.MainSymbolManager.AllocParams(tempTypeArray);
            AGGTYPESYM selectReturnTypeSym = Compiler.MainSymbolManager.GetInstAgg(
                ireturnAggSym,
                tempTypeArray);

            //--------------------------------------------------------
            // Get the arguments of Enumerable or Queryable method.
            //--------------------------------------------------------
            //NewList(selectLambdaExpr, ref firstArgsExpr, ref lastArgsExpr);
#endif
            //--------------------------------------------------------
            // Bind Enumerable or Queryable method.
            //--------------------------------------------------------
            //List<PARAMINFO> paramList = queryInfo.ParameterList;
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);
            typeArgs.Add(lambdaReturnTypeSym);
            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            queryInfo.CollectionExpr = BindLinqMethod(
                queryInfo.ToExpressionTrees,
                LinqMethodEnum.Select,
                selectNode,
                typeArgs,
                firstArgsExpr,
                queryInfo);
            queryInfo.ElementTypeSym = lambdaReturnTypeSym;
            return queryInfo.CollectionExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindGroupClause
        //
        /// <summary></summary>
        /// <param name="groupNode"></param>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
#if false
        internal EXPR BindGroupClause(
            GROUPCLAUSENODE groupNode,
            QueryExpressionInfo queryInfo)
        {
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);

            EXPRLAMBDAEXPR byLambdaExpr = BindLambdaExpression2(
                groupNode.ByExpressionNode,
                queryInfo.ParameterList) as EXPRLAMBDAEXPR;
            if (byLambdaExpr == null)
            {
                return null;
            }
            TYPESYM resTypeSym = byLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
            typeArgs.Add(resTypeSym);
            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);
            NewList(byLambdaExpr, ref firstArgsExpr, ref lastArgsExpr);

            queryInfo.CollectionExpr = BindLinqEnumerableMethod(
                LinqEnumerableMethodEnum.GroupBy,
                groupNode,
                typeArgs,
                firstArgsExpr,
                queryInfo);
            queryInfo.ElementTypeSym = GetElementType(
                null,
                null,
                queryInfo.CollectionExpr.TypeSym,
                queryInfo.CollectionExpr);
            return queryInfo.CollectionExpr;
        }
#endif
        internal EXPR BindGroupClause(
            GROUPCLAUSENODE groupNode,
            QueryExpressionInfo queryInfo)
        {
            // public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            // 	this IEnumerable<TSource> source,
            // 	Func<TSource, TKey> keySelector,
            // 	Func<TSource, TElement> elementSelector
            // )

            // public static IQueryable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            // 	this IQueryable<TSource> source,
            // 	Expression<Func<TSource, TKey>> keySelector,
            // 	Expression<Func<TSource, TElement>> elementSelector
            // )

            if (!queryInfo.IsOK)
            {
                return queryInfo.CollectionExpr;
            }

            //--------------------------------------------------------
            // Check if the element is same to the parameter.
            //--------------------------------------------------------
            bool needElement = true;

            EXPRSTMTNODE stmtNode = groupNode.ElementExpressionNode.BodyNode.StatementsNode as EXPRSTMTNODE;
            if (stmtNode!=null&&
                stmtNode.Kind == NODEKIND.RETURN &&
                stmtNode.NextNode == null)
            {
                NAMENODE argNode = stmtNode.ArgumentsNode as NAMENODE;
                if (argNode != null &&
                    argNode.Name == queryInfo.ParameterList[0].Name)
                {
                    needElement = false;
                }
            }

            //--------------------------------------------------------
            // TSource
            //--------------------------------------------------------
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(queryInfo.ElementTypeSym);

            //--------------------------------------------------------
            // TKey
            //--------------------------------------------------------
            EXPRLAMBDAEXPR byLambdaExpr = BindLambdaExpression2(
                groupNode.ByExpressionNode,
                queryInfo.ParameterList) as EXPRLAMBDAEXPR;
            if (byLambdaExpr == null)
            {
                return null;
            }
            TYPESYM byTypeSym = byLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
            typeArgs.Add(byTypeSym);

            //--------------------------------------------------------
            // TElement, and alloc the type arguments
            //--------------------------------------------------------
            EXPRLAMBDAEXPR elemLambdaExpr = null;
            TYPESYM elemTypeSym = null;

            if (needElement)
            {
                elemLambdaExpr = BindLambdaExpression2(
                    groupNode.ElementExpressionNode,
                    queryInfo.ParameterList) as EXPRLAMBDAEXPR;
                if (elemLambdaExpr == null)
                {
                    return null;
                }
                elemTypeSym = elemLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
                typeArgs.Add(elemTypeSym);
            }

            typeArgs = Compiler.MainSymbolManager.AllocParams(typeArgs);

            //--------------------------------------------------------
            // source
            //--------------------------------------------------------
            EXPR firstArgsExpr = null;
            EXPR lastArgsExpr = null;

            NewList(queryInfo.CollectionExpr, ref firstArgsExpr, ref lastArgsExpr);

            //--------------------------------------------------------
            // keySelector
            //--------------------------------------------------------
            if (queryInfo.ToExpressionTrees)
            {
                AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                TypeArray funcTypeArgs = new TypeArray();
                funcTypeArgs.Add(queryInfo.ElementTypeSym);
                funcTypeArgs.Add(byTypeSym);
                funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                    funcAggSym,
                    funcTypeArgs);

                AGGSYM expressionAggSym
                    = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                TypeArray exTypeArgs = new TypeArray();
                exTypeArgs.Add(funcAts);
                exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                    expressionAggSym,
                    exTypeArgs);

                EXPR exTreeExpr = null;
                CreateExpressionTree(
                    groupNode.ByExpressionNode,
                    byLambdaExpr,
                    expressionAts,
                    ref exTreeExpr,
                    true);

                NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
            }
            else
            {
                NewList(byLambdaExpr, ref firstArgsExpr, ref lastArgsExpr);
            }

            //--------------------------------------------------------
            // elementSelector
            //--------------------------------------------------------
            if (needElement)
            {
                if (queryInfo.ToExpressionTrees)
                {
                    AGGSYM funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                    TypeArray funcTypeArgs = new TypeArray();
                    funcTypeArgs.Add(queryInfo.ElementTypeSym);
                    funcTypeArgs.Add(elemTypeSym);
                    funcTypeArgs = Compiler.MainSymbolManager.AllocParams(funcTypeArgs);
                    AGGTYPESYM funcAts = Compiler.MainSymbolManager.GetInstAgg(
                        funcAggSym,
                        funcTypeArgs);

                    AGGSYM expressionAggSym
                        = Compiler.GetOptPredefAgg(PREDEFTYPE.LINQ_EXPRESSIONS_G_EXPRESSION, true);
                    TypeArray exTypeArgs = new TypeArray();
                    exTypeArgs.Add(funcAts);
                    exTypeArgs = Compiler.MainSymbolManager.AllocParams(exTypeArgs);
                    AGGTYPESYM expressionAts = Compiler.MainSymbolManager.GetInstAgg(
                        expressionAggSym,
                        exTypeArgs);

                    EXPR exTreeExpr = null;
                    CreateExpressionTree(
                        groupNode.ElementExpressionNode,
                        elemLambdaExpr,
                        expressionAts,
                        ref exTreeExpr,
                        true);

                    NewList(exTreeExpr, ref firstArgsExpr, ref lastArgsExpr);
                }
                else
                {
                    NewList(elemLambdaExpr, ref firstArgsExpr, ref lastArgsExpr);
                }
            }

            //--------------------------------------------------------
            // Bind group
            //--------------------------------------------------------
            queryInfo.CollectionExpr = BindLinqMethod(
                queryInfo.ToExpressionTrees,
                LinqMethodEnum.GroupBy,
                groupNode,
                typeArgs,
                firstArgsExpr,
                queryInfo);

            queryInfo.ElementTypeSym = GetElementType(
                null,
                null,
                queryInfo.CollectionExpr.TypeSym,
                queryInfo.CollectionExpr);
            return queryInfo.CollectionExpr;
        }
    }

    //======================================================================
    // class QueryUtil
    //======================================================================
    internal static class QueryUtil
    {
        private static int parameterProductCount = 0;

        private const string parameterProductFormat1 = "<{0}>";
        private const string parameterProductFormat2 = "ParameterProduct<{0}>";

        //------------------------------------------------------------
        // FUNCBREC.CreateParameterProductName (1)
        //
        /// <summary></summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string CreateParameterProductName(string name1, string name2)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(parameterProductFormat1, name1);
            sb.AppendFormat(parameterProductFormat1, name2);
            sb.AppendFormat(parameterProductFormat2, parameterProductCount++);
            return sb.ToString();
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateParameterProductName (2)
        //
        /// <summary></summary>
        /// <param name="paramInfoList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string CreateParameterProductName(List<PARAMINFO> paramInfoList)
        {
            DebugUtil.Assert(paramInfoList != null && paramInfoList.Count > 0);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < paramInfoList.Count; ++i)
            {
                sb.AppendFormat(parameterProductFormat1, paramInfoList[i].Name);
            }
            sb.AppendFormat(parameterProductFormat2, parameterProductCount++);
            return sb.ToString();
        }
    }
}
