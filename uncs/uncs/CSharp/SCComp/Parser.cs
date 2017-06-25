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
// File: parser.h
//
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
// File: parser.cpp
//
// ===========================================================================

//============================================================================
// Parser.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Uncs
{
    //======================================================================
    // enum ScanTypeFlagsEnum
    //
    /// <summary>
    /// Result of examination of type.
    /// (CSharp\SCComp\Parser.cs)
    /// </summary>
    //======================================================================
    internal enum ScanTypeFlagsEnum : int
    {
        Unknown,
        NotType,
        NonGenericTypeOrExpr,
        GenericTypeOrExpr,
        PointerOrMult,
        MustBeType,
        NullableType,

        /// <summary>
        /// Namespace or Type
        /// </summary>
        AliasQualName
    }

    //======================================================================
    // enum ScanNamedTypePartEnum
    //
    /// <summary>
    /// (CSharp\SCComp\Parser.cs)
    /// </summary>
    //======================================================================
    internal enum ScanNamedTypePartEnum : int
    {
        NotName,
        SimpleName,
        GenericName
    }

    //======================================================================
    // enum MemberNameEnum
    //
    /// <summary>
    /// (CSharp\SCComp\Parser.cs)
    /// </summary>
    //======================================================================
    internal enum MemberNameEnum : int
    {
        /// <summary>
        /// anything else
        /// </summary>
        NotMemberName,

        /// <summary>
        /// any bad member name which contains a dot
        /// </summary>
        NotMemberNameWithDot,

        /// <summary>
        /// I<R>.M<T> 
        /// </summary>
        GenericMethodName,

        /// <summary>
        /// I<R>.this
        /// </summary>
        IndexerName,

        PropertyOrEventOrMethodName,
        // I<R>.M

        /// <summary>
        /// M
        /// </summary>
        SimpleName,
    }

    //======================================================================
    // CParser
    //
    /// <summary>
    /// This is the base class for the CSharp parser.  Like CLexer, it is abstract;
    /// you must create a derivation and implement the pure virtuals.
    /// </summary>
    //======================================================================
    abstract internal partial class CParser
    {
        //============================================================
        // enum CParser.SPECIALNAME (SN_)
        //
        /// <summary>
        /// <para>Contextual keywords</para>
        /// <para>(CSharp\SCComp\Parser.cs)</para>
        /// </summary>
        //============================================================
        protected enum SPECIALNAME : int
        {
            MISSING,
            GET,
            SET,
            ADD,
            REMOVE,
            WHERE,
            PARTIAL,
            GLOBAL,
            YIELD,
            ALIAS,

            LIM
        };

        //============================================================
        // enum CParser.PARSEDECLFLAGS
        //
        /// <summary>(CSharp\SCComp\Parser.cs)</summary>
        //============================================================
        protected enum PARSEDECLFLAGS
        {
            /// <summary>
            /// Local variable declaration (i.e. not a field of a type)
            /// </summary>
            LOCAL = 0x0001,

            /// <summary>
            /// Variable is CONST
            /// </summary>
            CONST = 0x0002,

            /// <summary>
            /// Fixed buffer declaration
            /// </summary>
            FIXED = 0x0004,
        }

        //============================================================
        // enum CParser.PARSERTYPE  (EParse)
        //
        /// <summary>(CSharp\SCComp\Parser.cs)</summary>
        //============================================================
        internal enum PARSERTYPE : int
        {
            None,
            Statement,
            Block,
            BreakStatement,
            ConstStatement,
            LabeledStatement,
            DoStatement,
            ForStatement,
            ForEachStatement,
            GotoStatement,
            IfStatement,
            ReturnStatement,
            SwitchStatement,
            ThrowStatement,
            TryStatement,
            WhileStatement,
            YieldStatement,
            Declaration,
            DeclarationStatement,
            ExpressionStatement,
            LockStatement,
            FixedStatement,
            UsingStatement,
            CheckedStatement,
            UnsafeStatement,
        }

        //------------------------------------------------------------
        // CParser  Fileds and Properties
        //------------------------------------------------------------

        protected CNameManager nameManager = null;

        internal CNameManager NameManager
        {
            get { return nameManager; }
        }

        protected int currentTokenIndex = 0;

        /// <summary>
        /// NOTE:  -1 means we must search for it...
        /// </summary>
        protected int previousTokenIndex = -1;

        protected CSTOKEN[] tokenArray = null;
        protected int tokenCount = 0;
        protected int tokenLastIndex = -1;
        
        protected string sourceText = null;
        
        protected List<int> lineList = null;
        //protected int[] lineArray = null;
        //protected int lineCount = 0;
        
        protected bool isErrorOnCurrentToken = false;
        
        protected int parseErrorCount = 0;
        
        //protected string sourceFileName;
        protected FileInfo sourceFileInfo = null;

        protected string[] specialNames = new string[(int)SPECIALNAME.LIM];

        // The members below are defined at the end of this file.
        //    static internal TOKINFO[] TokenInfoArray
        //    static internal OPINFO[] OperatorInfoArray

        //------------------------------------------------------------
        // CParser (CS3) ParseModeEnum for query expressions
        //------------------------------------------------------------
        [Flags]
        private enum ParseModeEnum : int
        {
            None = 0,
            QueryExpression = (1 << 0),

            AllowNestedQueryExpression = (1 << 16),
        }

        //------------------------------------------------------------
        // CParser (CS3) parseModeStack for query expressions
        //------------------------------------------------------------
        private Stack<ParseModeEnum> parseModeStack = new Stack<ParseModeEnum>();

        //------------------------------------------------------------
        // CParser.PushParseMode (CS3) for query expressions
        //------------------------------------------------------------
        private void PushParseMode(ParseModeEnum mode)
        {
            parseModeStack.Push(mode);
        }

        //------------------------------------------------------------
        // CParser.PopParseMode (CS3) for query expressions
        //------------------------------------------------------------
        private void PopParseMode()
        {
            try
            {
                parseModeStack.Pop();
            }
            catch (InvalidOperationException)
            {
            }
        }

        //------------------------------------------------------------
        // CParser.GetCurrentParseMode (CS3) for query expressions
        //------------------------------------------------------------
        private ParseModeEnum GetCurrentParseMode()
        {
            if (this.parseModeStack.Count == 0)
            {
                return ParseModeEnum.None;
            }
            return this.parseModeStack.Peek();
        }

        //------------------------------------------------------------
        // CParser.IsQueryExpressionParseMode (CS3) for query expressions
        //------------------------------------------------------------
        private bool IsQueryExpressionParseMode()
        {
            if (this.parseModeStack.Count == 0)
            {
                return false;
            }
            return (parseModeStack.Peek() & ParseModeEnum.QueryExpression) != 0;
        }

        //------------------------------------------------------------
        // CParser.IsNestedQueryExpressionAllowed (CS3) for query expressions
        //------------------------------------------------------------
        private bool IsNestedQueryExpressionAllowed()
        {
            if (this.parseModeStack.Count == 0)
            {
                return false;
            }
            return (parseModeStack.Peek() & ParseModeEnum.AllowNestedQueryExpression) != 0;
        }

        //------------------------------------------------------------
        // CParser.SpecName
        //
        /// <summary>
        /// Return a contextual keyword string.
        /// </summary>
        /// <param name="sn">Id of a contextual keyword.</param>
        /// <returns></returns>
        /// <remarks>
        /// The IDs of contextual keywords are represented by enum SPECIALANAME,
        /// and their strings are stored in specialNames array.
        /// </remarks>
        //------------------------------------------------------------
        protected string SpecName(SPECIALNAME sn)
        {
            try
            {
                return specialNames[(int)sn];
            }
            catch (IndexOutOfRangeException)
            {
            }
            DebugUtil.Assert(false, "CParser.SpecName");
            return null;
        }

        //------------------------------------------------------------
        // CParser.CheckForName
        //
        /// <summary>
        /// Determine if a given token matches a given string.
        /// </summary>
        /// <param name="index">Token index</param>
        /// <param name="name"></param>
        /// <returns>Return true if matches.</returns>
        //------------------------------------------------------------
        protected bool CheckForName(int index, string name)
        {
            try
            {
                return (
                    (tokenArray[index].TokenID == TOKENID.IDENTIFIER) &&
                    (tokenArray[index].Name == name));
            }
            catch (IndexOutOfRangeException)
            {
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.CheckForSpecName
        //
        /// <summary>
        /// Determine if the current token is a given contextual keyword.
        /// </summary>
        /// <param name="sn">ID of a contextual keyword.</param>
        /// <returns>Return true if matches.</returns>
        //------------------------------------------------------------
        protected bool CheckForSpecName(SPECIALNAME sn)
        {
            return CheckForName(CurrentTokenIndex(), SpecName(sn));
        }

        //------------------------------------------------------------
        // CParser.ReportFeatureUse
        //
        /// <summary>
        /// Not defined in sscli.
        /// </summary>
        /// <param name="dwFeatureId"></param>
        //------------------------------------------------------------
        virtual protected void ReportFeatureUse(string strID)
        {
            //throw new NotImplementedException("CParser.ReportFeatureUse");
        }

        //------------------------------------------------------------
        // CParser.PeekTokenIndexFromInternalEx
        //
        /// <summary></summary>
        /// <param name="tokArray"></param>
        /// <param name="currentIndex"></param>
        /// <param name="flags"></param>
        /// <param name="peek"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static protected int PeekTokenIndexFromInternalEx(
            CSTOKEN[] tokArray,
            int currentIndex,
            ExtentFlags flags,
            int peek)
        {
            if ((flags & ExtentFlags.IGNORE_TOKEN_STREAM) != 0)
            {
                return currentIndex + peek;
            }
            else
            {
                //return Math.Min(
                //    tokArray.Length - 1,
                //    PeekTokenIndexFromInternal(tokArray, currentIndex, peek));
                int i1 = tokArray.Length - 1;
                int i2 = PeekTokenIndexFromInternal(tokArray, currentIndex, peek);
                return (i1 <= i2 ? i1 : i2);
            }
        }

        //------------------------------------------------------------
        // CParser.PeekTokenIndexFromInternal
        //
        /// <summary>
        /// currentIndex より後のトークンで有効なもの（コメント以外）のうち
        /// peekCount 個先のインデックスを返す。
        /// peekCount &lt; 0 の場合は逆方向に検索する。
        /// </summary>
        /// <param name="tokArray">対象のトークンリスト。</param>
        /// <param name="currentIndex">現在の位置を示すインデックス。</param>
        /// <param name="peekCount">何個先かを指定する。</param>
        /// <returns>得られたトークンのインデックス。</returns>
        //------------------------------------------------------------
        static protected int PeekTokenIndexFromInternal(
            CSTOKEN[] tokArray,
            int currentIndex,
            int peekCount)
        {
            if (peekCount > 0)
            {
                int i;
                for (i = currentIndex + 1; i < tokArray.Length - 1; i++)
                {
                    if ((TokenInfoArray[(int)tokArray[i].TokenID].Flags & TOKFLAGS.F_NOISE) == 0)
                    {
                        if (--peekCount == 0)
                        {
                            break;
                        }
                    }
                }
                //return Math.Min(i, tokArray.Length - 1);
                int j = tokArray.Length - 1;
                return (i <= j ? i : j);
            }
            else if (peekCount < 0)
            {
                // We need to adjust peekCount when currentIndex is out of bound of the token stream
                // Adjust peekCount to ignore out of stream indices

                //peekCount += Math.Max(0, currentIndex - tokArray.Length);
                int j1 = currentIndex - tokArray.Length;
                peekCount += (j1 >= 0 ? j1 : 0);

                // Ensure starting index in in bound

                //int i = Math.Min(tokArray.Length, currentIndex) - 1;
                int j2 = tokArray.Length;
                int i = (j2 <= currentIndex ? j2 : currentIndex) - 1;

                // If peekCount is still negative, go for it

                if (peekCount < 0)
                {
                    for (; i >= 0; i--)
                    {
                        if ((TokenInfoArray[(int)tokArray[i].TokenID].Flags & TOKFLAGS.F_NOISE) == 0)
                        {
                            if (++peekCount == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                //return Math.Max(i, 0);
                return (i >= 0 ? i : 0);
            }
            return currentIndex;
        }

        //private:

        //------------------------------------------------------------
        // CParser.IsLocalDeclaration
        //
        /// <summary>
        /// Determine if the current token is of a declaration of local variables.
        /// Use the ScanTypeFlagsEnum value of the previous token.
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsLocalDeclaration(ScanTypeFlagsEnum st)
        {
            if (st == ScanTypeFlagsEnum.MustBeType && CurrentTokenID() != TOKENID.DOT)
            {
                return true;
            }
            if (st == ScanTypeFlagsEnum.NotType || CurrentTokenID() != TOKENID.IDENTIFIER)
            {
                return false;
            }
            if (st == ScanTypeFlagsEnum.NullableType)
            {
            }
            return true;
        }

        //------------------------------------------------------------
        // CParser CheckIfGeneric
        //
        // type-argument-list:
        // 	"<"  type-arguments  ">"
        //
        // type-arguments:
        // 	type-argument
        // 	type-arguments  ","  type-argument
        //
        // type-argument:
        // 	type
        //
        /// <summary>
        /// <para>Determine if the token sequence from the next token represents generic.</para>
        /// <para>Not advance the current token.</para>
        /// </summary>
        /// <param name="inExpr">Set true if parsing an expression.</param>
        /// <param name="definitelyGeneric">(out) True if it is surely generic.</param>
        /// <param name="possiblyGeneric">(out) True if it may be generic.</param>
        //------------------------------------------------------------
        internal void CheckIfGeneric(
            bool inExpr,
            out bool definitelyGeneric,
            out bool possiblyGeneric)
        {
            bool definitely = definitelyGeneric = false;
            bool possibly = possiblyGeneric = false;

            // If the next token is "<", it may be generic.
            if (PeekToken() == TOKENID.OPENANGLE)
            {
                // In expression, it must be "<", list of types, ">".
                if (inExpr)
                {
                    int mark = CurrentTokenIndex();
                    NextToken();

                    definitely = possibly = ScanOptionalInstantiation();
                    if (definitely)
                    {
                        TOKENID tid = CurrentTokenID();

                        // F_TERM means that
                        //   Token belongs to first set for term-or-unary-operator (follows casts),
                        //   but is not a predefined type.
                        //
                        // F_TERM is set to the tokens below:
                        //
                        // "__arglist", "__makeref", "__reftype", "__refvalue", "base",
                        // "checked", "default", "delegate", "false", "new", "null",
                        // "sizeof", "this", "true", "typeof", "unchecked", "(",
                        // TID_IDENTIFIER, TID_NUMBER, TID_STRINGLIT, TID_VSLITERAL, TID_CHARLIT, 

                        if (tid != TOKENID.OPENPAREN &&
                            ((TokenInfoArray[(int)tid].Flags & (TOKFLAGS.F_TERM | TOKFLAGS.F_PREDEFINED)) != 0 ||
                            IsUnaryOperator(tid)))
                        {
                            definitely = false;
                        }
                    }
                    // 処理位置を元に戻す。
                    Rewind(mark);
                }
                // 式中でなければ確定である。
                else
                {
                    definitely = true;
                } // if (inExpr)s
            } // if (PeekToken() == TOKENID.OPENANGLE)

            definitelyGeneric = definitely;
            possiblyGeneric = possibly;
        }

        //------------------------------------------------------------
        // CParser.PastCloseToken
        //
        /// <summary>
        /// Return true if the current index &gt;= argument closeIndex.
        /// </summary>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool PastCloseToken(int closeIndex)
        {
            if (closeIndex == -1)
            {
                return false;
            }
            return CurrentTokenIndex() >= closeIndex;
        }

        //------------------------------------------------------------
        // CParser.AtSwitchCase
        //
        /// <summary>
        /// Return true if not passed the given index and
        /// if the current token is TOKENID.CASE or TOKENID.DEFAULT
        /// which represents the label of switch sentence.
        /// </summary>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool AtSwitchCase(int closeIndex)
        {
            return (
                !PastCloseToken(closeIndex) && (CurrentTokenID() == TOKENID.CASE) ||
                (CurrentTokenID() == TOKENID.DEFAULT && PeekToken() != TOKENID.OPENPAREN));
        }

        //protected:

        //------------------------------------------------------------
        // CParser.SupportsErrorSuppression
        //
        /// <summary>
        /// Not implemented. Retun false.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual protected bool SupportsErrorSuppression()
        {
            return false;
        }

        //------------------------------------------------------------
        // CParser.GetErrorSuppression
        //
        /// <summary>
        /// Create and return a CErrorSuppression.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual protected CErrorSuppression GetErrorSuppression()
        {
            DebugUtil.Assert(false, "Someone subclassed incorrectly");
            return new CErrorSuppression();
        }

        //internal:
        //------------------------------------------------------------
        // CParser Constructor
        //
        /// <summary></summary>
        /// <param name="manager"></param>
        /// <remarks>
        /// (In sscli, CParser (CController *pController);)
        /// </remarks>
        //------------------------------------------------------------
        internal CParser(CNameManager manager)
        {
            if (manager != null)
            {
                Init(manager);
            }
        }

        //------------------------------------------------------------
        // CParser.Init
        //
        /// <summary></summary>
        /// <param name="manager"></param>
        //------------------------------------------------------------
        internal void Init(CNameManager manager)
        {
            DebugUtil.Assert(manager != null);
            this.nameManager = manager;

            this.nameManager.AddString((specialNames[(int)SPECIALNAME.MISSING] = "?"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.GET] = "get"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.SET] = "set"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.ADD] = "add"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.REMOVE] = "remove"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.WHERE] = "where"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.PARTIAL] = "partial"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.GLOBAL] = "global"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.YIELD] = "yield"));
            this.nameManager.AddString((specialNames[(int)SPECIALNAME.ALIAS] = "alias"));
        }

        // Initialization

        //------------------------------------------------------------
        // CParser.SetInputData
        //
        /// <summary>
        /// このインスタンスに入力データをセットする。
        /// </summary>
        /// <param name="fileName">ソースを格納したファイルの名前。</param>
        /// <param name="lines">各行の開始インデックスを格納したリスト。</param>
        /// <param name="source">ソーステキスト。</param>
        /// <param name="tokens">ソースから作成したトークンリスト。</param>
        //------------------------------------------------------------
        internal void SetInputData(
            FileInfo srcFileInfo,
            CSTOKEN[] tokens,
            string source,
            List<int> lines)
        {
            DebugUtil.Assert(srcFileInfo != null && tokens != null);

            this.tokenArray = tokens;
            this.tokenCount = this.tokenArray.Length;
            this.tokenLastIndex = this.tokenCount - 1;

            sourceText = source;
            lineList = lines;
            sourceFileInfo = srcFileInfo;
            nameManager.AddString(srcFileInfo.FullName);
        }

        //------------------------------------------------------------
        // CParser.SetInputData
        //
        /// <summary>
        /// このインスタンスに入力データをセットする。
        /// </summary>
        /// <param name="fileName">ソースを格納したファイルの名前。</param>
        /// <param name="lexData">入力データを格納している LEXDATA。</param>
        //------------------------------------------------------------
        internal void SetInputData(FileInfo srcFileInfo, LEXDATA lexData)
        {
            DebugUtil.Assert(srcFileInfo != null && lexData != null);

            this.tokenArray = lexData.TokenArray;
            this.tokenCount = this.tokenArray.Length;
            this.tokenLastIndex = this.tokenCount - 1;

            this.lineList = lexData.LineList;

            sourceText = lexData.UnsafeExposeSource();
            sourceFileInfo = srcFileInfo;
            nameManager.AddString(srcFileInfo.FullName);
        }

        //------------------------------------------------------------
        // CParser.CurrentTokenID
        //
        /// <summary>
        /// <para>Simple parsing/token indexing helpers</para>
        /// <para>Return the ID representing the kind of the current token.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TOKENID CurrentTokenID()
        {
            try
            {
                return tokenArray[currentTokenIndex].TokenID;
            }
            catch (IndexOutOfRangeException)
            {
            }
            return TOKENID.UNDEFINED;
        }

        //------------------------------------------------------------
        // CParser.CurrentTokenIndex
        //
        /// <summary>
        /// Return the index of the current token.
        /// (This index means its position.)
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int CurrentTokenIndex()
        {
            return this.currentTokenIndex;
        }

        //------------------------------------------------------------
        // CParser.Rewind
        //
        /// <summary>
        /// Move this.currentTokenIndex to a given index.
        /// </summary>
        /// <param name="idx"></param>
        //------------------------------------------------------------
        internal void Rewind(int idx)
        {
            currentTokenIndex = idx;
            previousTokenIndex = -1;
            isErrorOnCurrentToken = false;
        }

        //------------------------------------------------------------
        // CParser.CurrentTokenPos
        //
        /// <summary>
        /// Return the POSDATA instance of the current token.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal POSDATA CurrentTokenPos()
        {
            try
            {
                return (tokenArray[currentTokenIndex] as POSDATA);
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // CParser.TokenAt
        //
        /// <summary>
        /// Return the index representing the kind of the specifed token.
        /// </summary>
        /// <param name="index">Index of the order of token.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TOKENID TokenAt(int index)
        {
            try
            {
                return tokenArray[index].TokenID;
            }
            catch (IndexOutOfRangeException)
            {
            }
            return TOKENID.UNDEFINED;
        }

        //------------------------------------------------------------
        // CParser.NextToken
        //
        /// <summary>
        /// Advance to the next token. Skip comments.
        /// </summary>
        /// <returns>Index of next token.</returns>
        //------------------------------------------------------------
        internal TOKENID NextToken()
        {
            int prevIdx = currentTokenIndex;
            while (currentTokenIndex < tokenCount - 1)
            {
                currentTokenIndex++;
                isErrorOnCurrentToken = false;
                if ((TokenInfoArray[(int)tokenArray[currentTokenIndex].TokenID].Flags & TOKFLAGS.F_NOISE)
                    == 0)
                {
                    previousTokenIndex = prevIdx;
                    return CurrentTokenID();
                }
            }

            return CurrentTokenID();
        }

        //------------------------------------------------------------
        // CParser.PrevToken
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TOKENID PrevToken()
        {
            if (previousTokenIndex != -1)
            {
                // Previous token is known...
                currentTokenIndex = previousTokenIndex;
            }
            else
            {
                // Previous token isn't known, must search for it
                while (currentTokenIndex > 0)
                {
                    if ((TokenInfoArray[(int)tokenArray[--currentTokenIndex].TokenID].Flags &
                        TOKFLAGS.F_NOISE) == 0)
                    {
                        break;
                    }
                }
            }

            // Must search for the next "previous" token
            previousTokenIndex = -1;
            isErrorOnCurrentToken = false;
            return CurrentTokenID();
        }

        //------------------------------------------------------------
        // CParser.PeekToken    (1)
        //
        /// <summary>
        /// peekCount 個先のトークンの ID を返す。
        /// </summary>
        /// <param name="peekCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TOKENID PeekToken(int peekCount)
        {
            return tokenArray[PeekTokenIndexFrom(currentTokenIndex, peekCount)].TokenID;
        }

        //------------------------------------------------------------
        // CParser.PeekToken    (2)
        //
        /// <summary>
        /// 次のトークンの ID を返す。
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TOKENID PeekToken()
        {
            return PeekToken(1);
        }

        //------------------------------------------------------------
        // CParser.PeekTokenIndex
        //
        /// <summary>
        /// 現在のトークンから（コメントなどは飛ばして） peek 個先のトークンのインデックスを返す。
        /// </summary>
        /// <param name="peek"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int PeekTokenIndex(int peek)
        {
            return PeekTokenIndexFrom(currentTokenIndex, peek);
        }

        //------------------------------------------------------------
        // CParser.PeekTokenIndex
        //
        /// <summary>
        /// 現在のトークンの（コメントなどは飛ばして）次のトークンのインデックスを返す。
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int PeekTokenIndex()
        {
            return PeekTokenIndexFrom(currentTokenIndex, 1);
        }

        //------------------------------------------------------------
        // CParser.PeekTokenIndexFrom (1)
        //
        /// <summary>
        /// <para>currentIndex より後のトークンで有効なもの（コメント以外）のうち
        /// peekCount 個先のインデックスを返す。</para>
        /// <para>peekCount &lt; 0 の場合は逆方向に検索する。</para>
        /// </summary>
        /// <param name="currentIndex">現在の位置を示すインデックス。</param>
        /// <param name="peekCount">何個先かを指定する。</param>
        /// <returns>得られたトークンのインデックス。</returns>
        //------------------------------------------------------------
        internal int PeekTokenIndexFrom(
            int currentIndex,
            int peekCount)  // = 1 (sscli)
        {
            return PeekTokenIndexFromInternal(tokenArray, currentIndex, peekCount);
        }

        //------------------------------------------------------------
        // CParser.PeekTokenIndexFromEx
        //
        /// <summary></summary>
        /// <param name="iCur"></param>
        /// <param name="flags"></param>
        /// <param name="iPeek"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int PeekTokenIndexFromEx(
            int iCur,
            ExtentFlags flags,
            int iPeek)  // = 1 (sscli)
        {
            return PeekTokenIndexFromInternalEx(tokenArray, iCur, flags, iPeek);
        }

        // More advanced (non-inlined) parsing/token indexing helpers
        // returns true if it successfully ate that token

        //------------------------------------------------------------
        // CParser.Eat
        //
        /// <summary>
        /// <para>If the current token is matches to the specified one, advance to next token.
        /// Otherwise, show an error message.</para>
        /// <para>If not match, assuming that the specified token is found, contine to process.</para>
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Eat(TOKENID tokenId)
        {
            TOKENID tokenIdActual = CurrentTokenID();
            if (tokenIdActual == tokenId)
            {
                NextToken();
                return true;
            }
            else
            {
                ErrorAfterPrevToken(
                    ExpectedErrorFromToken(tokenId, tokenIdActual),
                    new ErrArg(GetTokenInfo(tokenId).Text),
                    new ErrArg(GetTokenInfo(tokenIdActual).Text));
                return false;
            }
        }

        //------------------------------------------------------------
        // CParser.CheckToken
        //
        /// <summary>
        /// <para>Check if the current token ID is equal to the specified.</para>
        /// <para>If not equal, show an error message and return false.</para>
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckToken(TOKENID tokenId)
        {
            TOKENID actualId;
            if ((actualId = CurrentTokenID()) != tokenId)
            {
                ErrorAfterPrevToken(
                    ExpectedErrorFromToken(tokenId, actualId),
                    new ErrArg(GetTokenInfo(tokenId).Text),
                    new ErrArg(GetTokenInfo(actualId).Text));
                return false;
            }
            return true;
        }

        // Error reporting

        //------------------------------------------------------------
        // CParser.Error
        //
        /// <summary></summary>
        /// <remarks>In sscli,
        /// internal void ErrorArgs(CSCERRID id, params ErrArg[] args)
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Error(CSCERRID id, params ErrArg[] args)
        {
            if (!isErrorOnCurrentToken)
            {
                POSDATA pos = tokenArray[CurrentTokenIndex()];
                POSDATA posEnd = tokenArray[CurrentTokenIndex()].StopPosition();
                if (args == null)
                {
                    args = new ErrArg[0];
                }
                isErrorOnCurrentToken = CreateNewError(id, pos, posEnd, args);
            }
            parseErrorCount++;
        }

        //------------------------------------------------------------
        // CParser.Error
        //
        // Produce the given error/warning at the current token.
        //------------------------------------------------------------
        //internal void Error(CSCERRID id) { ErrorArgs(id, null); }
        //internal void Error(CSCERRID id, ErrArg a) { ErrorArgs(id, a); }
        //internal void Error(CSCERRID id, ErrArg a, ErrArg b) { ErrorArgs(id, a, b); }

        //------------------------------------------------------------
        // CParser.ErrorAtTokenArgs
        //
        /// <summary></summary>
        /// <param name="tokenIndex"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ErrorAtTokenArgs(int tokenIndex, CSCERRID id, params ErrArg[] args)
        {
            POSDATA pos = tokenArray[tokenIndex];
            POSDATA posEnd = tokenArray[tokenIndex].StopPosition();

            CreateNewError(id, pos, posEnd, args);
            parseErrorCount++;
        }

        //------------------------------------------------------------
        // CParser.ErrorAtToken
        //
        /// <summary></summary>
        /// <param name="tokenIndex"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ErrorAtToken(int tokenIndex, CSCERRID id, params ErrArg[] args)
        {
            ErrorAtTokenArgs(tokenIndex, id, args);
        }

        //------------------------------------------------------------
        // CParser.ErrorAfterPrevTokenArgs
        //
        /// <summary>
        /// If the current token is on the same line as the previous token,
        /// then behave just like Error.
        /// Otherwise, report the error 1 character beyond the previous token.
        /// Useful for "expected semicolon" type messages.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ErrorAfterPrevTokenArgs(CSCERRID id, params ErrArg[] args)
        {
            int currentIndex = CurrentTokenIndex();
            int previousIndex = PeekTokenIndex(-1);
            POSDATA currentPos = tokenArray[currentIndex];
            POSDATA previousPos = tokenArray[previousIndex];
            POSDATA pos, endPos;
            bool isError = false;

            if (previousPos.LineIndex == currentPos.LineIndex && CurrentTokenID() != TOKENID.ENDFILE)
            {
                pos = currentPos;
                endPos = tokenArray[currentIndex].StopPosition();
                isError = true;
            }
            else
            {
                pos = endPos = tokenArray[previousIndex].StopPosition();
                endPos.CharIndex++;
            }

            if (!isErrorOnCurrentToken)
            {
                if (args == null)
                {
                    args = new ErrArg[0];
                }
                isError = (CreateNewError(id, pos, endPos, args) & isError);
                parseErrorCount++;

                // We only set this if we actually reported an error, not just a warning
                // And we're reporting it on the current token

                if (isError)
                {
                    this.isErrorOnCurrentToken = true;
                }
            }
        }

        //------------------------------------------------------------
        // CParser.ErrorAfterPrevToken  (1)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        //------------------------------------------------------------
        internal void ErrorAfterPrevToken(CSCERRID id)
        {
            ErrorAfterPrevTokenArgs(id, null);
        }

        //------------------------------------------------------------
        // CParser.ErrorAfterPrevToken  (2)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="a"></param>
        //------------------------------------------------------------
        internal void ErrorAfterPrevToken(CSCERRID id, ErrArg a)
        {
            ErrorAfterPrevTokenArgs(id, a);
        }

        //------------------------------------------------------------
        // CParser.ErrorAfterPrevToken  (3)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        //------------------------------------------------------------
        internal void ErrorAfterPrevToken(CSCERRID id, ErrArg a, ErrArg b)
        {
            ErrorAfterPrevTokenArgs(id, a, b);
        }

        //void ErrorAtPositionArgs(int iLine, int iCol, int iExtent, CSCERRID id, int carg, ErrArg * prgarg);
        //void ErrorAtPosition(int iLine, int iCol, int iExtent, CSCERRID id) { ErrorAtPositionArgs(iLine, iCol, iExtent, id, 0, null); }
        //void __cdecl ErrorAtPosition (int iLine, int iCol, int iExtent, CSCERRID iErrorId, ...);

        //------------------------------------------------------------
        // CParser.ExpectedErrorFromToken
        //
        /// <summary>
        /// トークンの ID から Expected エラーの CSCERRID を返す。
        /// 適当なものが見つからない場合は CSUIE.ERR_SyntaxError を返す。
        /// </summary>
        /// <param name="tokId"></param>
        /// <param name="actualTokId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CSCERRID ExpectedErrorFromToken(TOKENID tokId, TOKENID actualTokId)
        {
            CSCERRID errorId;

            switch (tokId)
            {
                case TOKENID.IDENTIFIER:
                    if (actualTokId < TOKENID.IDENTIFIER)
                    {
                        errorId = CSCERRID.ERR_IdentifierExpectedKW;  // A keyword -- use special message.
                    }
                    else
                    {
                        errorId = CSCERRID.ERR_IdentifierExpected;
                    }
                    break;

                case TOKENID.SEMICOLON:
                    errorId = CSCERRID.ERR_SemicolonExpected;
                    break;

                case TOKENID.COLON:
                    errorId = CSCERRID.ERR_SyntaxError; // CSCERRID.ERR_ColonExpected;
                    break;

                case TOKENID.OPENPAREN:
                    errorId = CSCERRID.ERR_ExpectedDotOrParen;  // CSCERRID.ERR_LparenExpected;
                    break;

                case TOKENID.CLOSEPAREN:
                    errorId = CSCERRID.ERR_CloseParenExpected;
                    break;

                case TOKENID.OPENCURLY:
                    errorId = CSCERRID.ERR_LbraceExpected;
                    break;

                case TOKENID.CLOSECURLY:
                    errorId = CSCERRID.ERR_RbraceExpected;
                    break;

                case TOKENID.CLOSESQUARE:
                    errorId = CSCERRID.ERR_SyntaxError; // CSCERRID.ERR_CloseSquareExpected;
                    break;

                default:
                    errorId = CSCERRID.ERR_SyntaxError;
                    break;
            }

            return errorId;
        }

        //------------------------------------------------------------
        // CParser.GetTokenText
        //
        /// <summary></summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetTokenText(int tokenIndex)
        {
            switch (tokenArray[tokenIndex].TokenID)
            {
                case TOKENID.IDENTIFIER:
                    return tokenArray[tokenIndex].Name;

                case TOKENID.UNKNOWN:
                    return tokenArray[tokenIndex].Name;

                case TOKENID.NUMBER:
                    return tokenArray[tokenIndex].Literal.Text;

                case TOKENID.CHARLIT:
                    return tokenArray[tokenIndex].CharLiteral.Value.ToString();

                case TOKENID.VSLITERAL:
                    return tokenArray[tokenIndex].VSLiteral.Str;

                case TOKENID.STRINGLIT:
                    return tokenArray[tokenIndex].StringLiteral.Str;

                default:
                    return GetTokenInfo(tokenArray[tokenIndex].TokenID).Text;
            }

            //ASSERT( !"How did we get here?");
            //return null;
        }

        //------------------------------------------------------------
        // CParser.ErrorInvalidMemberDecl
        //
        /// <summary>
        /// メンバーの宣言が無効な場合の処理。
        /// </summary>
        /// <param name="tokenIndex">対象となるトークンのインデックス。</param>
        //------------------------------------------------------------
        internal void ErrorInvalidMemberDecl(int tokenIndex)
        {
            // An invalid token was found in a member declaration.
            // If the token is a modifier, give a special error message that seems better  

            if ((GetTokenInfo(tokenArray[tokenIndex].TokenID).Flags & TOKFLAGS.F_MODIFIER) != 0)
            {
                Error(CSCERRID.ERR_BadModifierLocation, new ErrArg(GetTokenText(tokenIndex)));
            }
            else
            {
                Error(CSCERRID.ERR_InvalidMemberDecl, new ErrArg(GetTokenText(tokenIndex)));
            }
        }

        // Top-level parse/skip/scan helpers

        //------------------------------------------------------------
        // CParser.SkipToNamespaceElement
        //
        /// <summary>
        /// Skip ahead to something that looks like it could be the start of a namespace element.
        /// Return true if one is found;
        /// or false if end-of-file or unmatched close-curly is found first.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SkipToNamespaceElement()
        {
            int curlyCount = 0;

            while (true)
            {
                TOKENID tokenId = NextToken();

                if ((TokenInfoArray[(int)tokenId].Flags & TOKFLAGS.F_NSELEMENT) != 0)
                {
                    return true;
                }

                switch (tokenId)
                {
                    case TOKENID.IDENTIFIER:
                        if (tokenArray[CurrentTokenIndex()].Name == SpecName(SPECIALNAME.PARTIAL) &&
                            (TokenInfoArray[(int)PeekToken()].Flags & TOKFLAGS.F_NSELEMENT) != 0)
                        {
                            return true;
                        }
                        break;

                    case TOKENID.NAMESPACE:
                        return true;

                    case TOKENID.OPENCURLY:
                        curlyCount++;
                        break;

                    case TOKENID.CLOSECURLY:
                        if (curlyCount-- == 0)
                            return false;
                        break;

                    case TOKENID.ENDFILE:
                        return false;

                    default:
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CParser.SkipToMember
        //
        /// <summary>
        /// Skip ahead to something that looks like it could be the start of a member.
        /// Return true if one is found; or false if end-of-file or unmatched close-curly
        /// is found first.
        /// </summary>
        /// <returns>次のメンバーと思われるトークンが見つかったら true を返す。</returns>
        //------------------------------------------------------------
        internal bool SkipToMember()
        {
            int curlyCount = 0;

            while (true)
            {
                TOKENID tokenId = NextToken();

                // If this token can start a member, we're done
                if ((TokenInfoArray[(int)tokenId].Flags & TOKFLAGS.F_MEMBER) != 0 &&
                    !(tokenId == TOKENID.DELEGATE &&
                    (PeekToken() == TOKENID.OPENCURLY || PeekToken() == TOKENID.OPENPAREN)))
                {
                    return true;
                }

                // Watch curlies and look for end of file/close curly
                switch (tokenId)
                {
                    case TOKENID.OPENCURLY:
                        curlyCount++;
                        break;

                    case TOKENID.CLOSECURLY:
                        if (curlyCount-- == 0)
                            return false;
                        break;

                    case TOKENID.ENDFILE:
                        return false;

                    default:
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CParser.SkipBlock
        //
        /// <summary>
        /// The purpose of this function is to skip method/constructor/destructor/accessor
        /// bodies during the top-level parse.  The trick is that while scanning for the
        /// matching close-curly, we check for patterns that indicate a missing close-
        /// curly situation.  These patterns include class member declarations, as well
        /// as namespace member declarations.
        /// </summary>
        /// <param name="closeIndex">ブロックの終了位置のインデックスを返す。</param>
        /// <param name="flags">スキップの仕方を指示するフラグ。</param>
        /// <returns>どのようなスキップをしたかを示す SKIPFLAGS 型の値。</returns>
        //------------------------------------------------------------
        //internal DWORD SkipBlock(DWORD dwFlags,out int piClose);
        internal SKIPFLAGS SkipBlock(SKIPFLAGS flags, out int closeIndex)
        {
            // We're always called with the open curly as the current token...
            Eat(TOKENID.OPENCURLY);

            int curlyCount = 1;

            for (; ; NextToken())
            {
                TOKENID tokenId = CurrentTokenID();
                TOKFLAGS tokenFlags = TokenInfoArray[(int)tokenId].Flags;

                if (tokenId == TOKENID.OPENCURLY)
                {
                    // Increment curly nesting
                    curlyCount++;
                    continue;
                }

                if (tokenId == TOKENID.CLOSECURLY)
                {
                    // Decrement curly nesting, and bail if 0
                    curlyCount--;
                    if (curlyCount == 0)
                    {
                        // Here's the end of our block.  Remember the close token
                        // index, and indicate to the caller that this was a normal skip.
                        closeIndex = CurrentTokenIndex();
                        NextToken();
                        if (CurrentTokenID() == TOKENID.SEMICOLON)
                        {
                            Error(CSCERRID.ERR_UnexpectedSemicolon);
                        }
                        return SKIPFLAGS.NORMALSKIP;
                    }
                    continue;
                }

                if (tokenId == TOKENID.ENDFILE)
                {
                    // Whoops!  We hit the end of the file without finding our match.
                    // Report the error and indicate what happened to the caller.

                    closeIndex = CurrentTokenIndex();
                    CheckToken(TOKENID.CLOSECURLY);
                    return SKIPFLAGS.ENDOFFILE;
                }

                if (tokenId == TOKENID.NAMESPACE)
                {
                    closeIndex = CurrentTokenIndex();
                    CheckToken(TOKENID.CLOSECURLY);
                    return SKIPFLAGS.NAMESPACEMEMBER;
                }

                if ((tokenFlags & TOKFLAGS.F_MEMBER) != 0)
                {
                    SKIPFLAGS scan;
                    int mark = CurrentTokenIndex();

                    // This token can start a type member declaration.  Scan it to see
                    // if it really looks like a member declaration that is NOT also a
                    // valid statement (such as a local variable declaration)

                    scan = ScanMemberDeclaration(flags);
                    Rewind(mark);
                    if (scan != SKIPFLAGS.NOTAMEMBER)
                    {
                        // This is a member declaration, so it's out of place here.
                        // Assume a close-curly is missing, and indicate what we found
                        // to the caller

                        closeIndex = CurrentTokenIndex();
                        CheckToken(TOKENID.CLOSECURLY);
                        return scan;
                    }
                }
            }
        }

        //------------------------------------------------------------
        // CParser.ScanMemberDeclaration
        //
        /// <summary>
        /// This function is used by SkipBlock to check to see if the current token is
        /// the start of a valid type member declaration that is NOT a valid statement.
        /// Note that field declarations look like local variable declarations, so only
        /// certain kinds of fields will trigger this (such as const, or those with an
        /// access modifier such as 'public').  But methods, constructors, properties,
        /// etc., will always return true provided that they are correct/complete enough
        /// (i.e. up to their own block or semicolon).  If we're inside a property
        /// accessor (indicated by dwSkipFlags) then we also check for another accessor.
        ///
        /// Note that the current token is UNDEFINED after calling this function...
        /// </summary>
        /// <param name="skipFlags">スキップの仕方を示す SKIPFLAGS 型の値。</param>
        /// <returns>実際に行われたスキップを示す SKIPFLAGS 型の値。</returns>
        //------------------------------------------------------------
        internal SKIPFLAGS ScanMemberDeclaration(SKIPFLAGS skipFlags)
        {
            // First, check to see if we're in an accessor and handle detection of
            // another one.

            if ((skipFlags & SKIPFLAGS.F_INACCESSOR) != 0)
            {
                int mark = CurrentTokenIndex();

                // The situation we're trying to detect here is a missing close-curly
                // before the second accessor in a property declaration.

                while ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_MODIFIER) != 0)
                {
                    NextToken();
                }

                if (CurrentTokenID() == TOKENID.IDENTIFIER && PeekToken() == TOKENID.OPENCURLY)
                {
                    string name = tokenArray[CurrentTokenIndex()].Name;

                    if (name == SpecName(SPECIALNAME.GET) ||
                        name == SpecName(SPECIALNAME.SET) ||
                        name == SpecName(SPECIALNAME.ADD) ||
                        name == SpecName(SPECIALNAME.REMOVE))
                        return SKIPFLAGS.ACCESSOR;
                }

                // Not an accessor...
                Rewind(mark);
            }

            // The remaining logic in this function should closely match that in
            // ParseMember; it just doesn't construct a parse tree.

            // The [ token will land us here, but we don't look at attributes.  That means
            // if a member DOES has attributes, we'll miss them -- but we don't care, since
            // that only means an error occurred so we won't do anything with them anyway.
            // If we cared enough, we could scan backwards for attributes if we found a
            // member declaration -- but I don't think it's worth it.

            if (CurrentTokenID() == TOKENID.OPENSQUARE)
            {
                return SKIPFLAGS.NOTAMEMBER;
            }

            bool hadModifiers = false;

            // Skip any modifiers, remembering if there were any

            while (
                (TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_MODIFIER) != 0 ||
                CheckForSpecName(SPECIALNAME.PARTIAL) &&
                (TokenInfoArray[(int)PeekToken()].Flags & (TOKFLAGS.F_MODIFIER | TOKFLAGS.F_NSELEMENT)) != 0)
            {
                switch (CurrentTokenID())
                {
                    // These guys are really unlikely to be put in a method body
                    // and so they indicate that this is probably a real modifier on a real member

                    case TOKENID.PRIVATE:
                    case TOKENID.PROTECTED:
                    case TOKENID.INTERNAL:
                        {
                            if ((skipFlags & SKIPFLAGS.F_INPROPERTY) != 0)
                            {
                                break;
                            }
                        }
                        //fall through
                        goto default;

                    case TOKENID.PUBLIC:
                    case TOKENID.SEALED:
                    case TOKENID.ABSTRACT:
                    case TOKENID.VIRTUAL:
                    case TOKENID.EXTERN:
                    case TOKENID.OVERRIDE:
                    default:
                        hadModifiers = true;
                        break;

                    // These are likely to appear inside a method as local modifiers
                    // (especially for C/C++ programmers) and so don't treat it as a modifier
                    // until we see something that couldn't be inside a method body

                    case TOKENID.STATIC:
                    case TOKENID.READONLY:
                    case TOKENID.VOLATILE:
                    case TOKENID.UNSAFE:
                    case TOKENID.NEW:
                        break;
                }

                NextToken();
            }

            // See if this looks like a constructor (or method w/ missing type)
            // 識別子 "(" の場合

            if (CurrentTokenID() == TOKENID.IDENTIFIER && PeekToken() == TOKENID.OPENPAREN)
            {
                bool hadParms = false;

                // Skip to the open paren and scan an argument list.  If we don't
                // get a valid one, this is not a member.

                NextToken();
                if (!ScanParameterList(false, out hadParms))
                {
                    return SKIPFLAGS.NOTAMEMBER;
                }

                // NOTE:  ScanParameterList leaves the current token at what follows
                // the close paren if it was successful...

                // If the parameter list had confirmed parameters, then this MUST
                // have been a method/ctor declaration.  Also, if followed by an
                // open curly, it could only have been a method/ctor.

                // [grantri, 7/31/2002] - this isn't true anymore because of anonymous delegates!
                // an anonymous delegate looks almost identical to a constructor!!!
                // so we can't check for the open curly

                if (hadParms || (hadModifiers && CurrentTokenID() == TOKENID.OPENCURLY))
                {
                    return SKIPFLAGS.TYPEMEMBER;
                }

                // Check for ":[this|base](" -- which clearly indicates a constructor

                if (CurrentTokenID() == TOKENID.COLON &&
                    (PeekToken(1) == TOKENID.THIS || PeekToken(1) == TOKENID.BASE) &&
                    PeekToken(2) == TOKENID.OPENPAREN)
                {
                    return SKIPFLAGS.TYPEMEMBER;
                }

                // If followed by a semicolon, and there were modifiers,
                // it must have been a member

                if (CurrentTokenID() == TOKENID.SEMICOLON && hadModifiers)
                {
                    return SKIPFLAGS.TYPEMEMBER;
                }

                // This wasn't a member...
                return SKIPFLAGS.NOTAMEMBER;
            }

            // ~ の場合はデストラクタであるかを調べる。

            if (CurrentTokenID() == TOKENID.TILDE)
            {
                // Check for destructor
                if (PeekToken(1) == TOKENID.IDENTIFIER &&
                    PeekToken(2) == TOKENID.OPENPAREN &&
                    PeekToken(3) == TOKENID.CLOSEPAREN &&
                    PeekToken(4) == TOKENID.OPENCURLY)
                {
                    return SKIPFLAGS.TYPEMEMBER;
                }
                return SKIPFLAGS.NOTAMEMBER;
            }

            // "const" の場合

            if (CurrentTokenID() == TOKENID.CONST)
            {
                // If preceded by modifiers, this must be a member
                if (hadModifiers)
                {
                    return SKIPFLAGS.TYPEMEMBER;
                }

                // Can't tell a const member from a const local...
                // So keep looking
                return SKIPFLAGS.NOTAMEMBER;
            }

            // "event" の場合

            if (CurrentTokenID() == TOKENID.EVENT)
            {
                // Events are always SKIPFLAGS.TYPEMEMBER
                return SKIPFLAGS.TYPEMEMBER;
            }

            // 型の定義（"class","delegate","enum","interface","struct"）の場合

            if ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_TYPEDECL) != 0)
            {
                // Nested types ALWAYS trigger SKIPFLAGS.TYPEMEMBER (unless it's an anonymous method)

                if (CurrentTokenID() == TOKENID.DELEGATE &&
                    (PeekToken() == TOKENID.OPENPAREN || PeekToken() == TOKENID.OPENCURLY))
                {
                    return SKIPFLAGS.NOTAMEMBER;
                }
                return SKIPFLAGS.TYPEMEMBER;
            }

            // "implicit operator" または "explicit operator" の場合

            if ((CurrentTokenID() == TOKENID.IMPLICIT || CurrentTokenID() == TOKENID.EXPLICIT)
                && PeekToken() == TOKENID.OPERATOR)
            {
                // Conversion operators -- look no further
                return SKIPFLAGS.TYPEMEMBER;
            }

            // Everything else must have a [return] type
            // 以上に当てはまらないメンバは返り値の型が指定されていなければならない。

            if (ScanType() == ScanTypeFlagsEnum.NotType)
            {
                return SKIPFLAGS.NOTAMEMBER;
            }

            // Anything that had modifiers is considered a member
            if (hadModifiers)
            {
                return SKIPFLAGS.TYPEMEMBER;
            }

            // Operators...
            if (CurrentTokenID() == TOKENID.OPERATOR)
            {
                return SKIPFLAGS.TYPEMEMBER;
            }

            // Indexers...
            if (CurrentTokenID() == TOKENID.THIS)
            {
                int iMark = CurrentTokenIndex();
                NextToken();
                bool hadParms;
                if (ScanParameterList(true, out hadParms))
                {
                    Rewind(iMark);
                    return SKIPFLAGS.TYPEMEMBER;
                }
                return SKIPFLAGS.NOTAMEMBER;
            }

            // NOTE (peterhal): can't have a dot here

            bool notProperty = false;
            switch (ScanMemberName())
            {
                case MemberNameEnum.NotMemberName:
                case MemberNameEnum.NotMemberNameWithDot:
                default:
                    return SKIPFLAGS.NOTAMEMBER;

                case MemberNameEnum.GenericMethodName:
                    notProperty = true;
                    break;

                case MemberNameEnum.PropertyOrEventOrMethodName:
                case MemberNameEnum.SimpleName:
                    break;

                case MemberNameEnum.IndexerName:
                    if (CurrentTokenID() == TOKENID.OPENSQUARE)
                    {
                        return SKIPFLAGS.TYPEMEMBER;
                    }
                    // Explicit interface impl of indexer
                    else
                    {
                        return SKIPFLAGS.NOTAMEMBER;
                    }
            }

            if (CurrentTokenID() == TOKENID.OPENPAREN)
            {
                bool hadArgs = false;

                // See if this really is a method
                if (!ScanParameterList(false, out hadArgs))
                {
                    return SKIPFLAGS.NOTAMEMBER;
                }

                if (CurrentTokenID() == TOKENID.OPENCURLY || CheckForSpecName(SPECIALNAME.WHERE))
                {
                    return SKIPFLAGS.TYPEMEMBER;
                }

                return SKIPFLAGS.NOTAMEMBER;
            }

            if (!notProperty && CurrentTokenID() == TOKENID.OPENCURLY)
            {
                // Check to see if this really looks like a property
                NextToken();
                while ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_MODIFIER) != 0)
                {
                    NextToken();
                }
                if (CurrentTokenID() == TOKENID.IDENTIFIER)
                {
                    string name = tokenArray[CurrentTokenIndex()].Name;

                    if (name == SpecName(SPECIALNAME.GET) ||
                        name == SpecName(SPECIALNAME.SET) ||
                        name == SpecName(SPECIALNAME.ADD) ||
                        name == SpecName(SPECIALNAME.REMOVE))
                    {
                        return SKIPFLAGS.TYPEMEMBER;
                    }
                }
                return SKIPFLAGS.NOTAMEMBER;
            }
            return SKIPFLAGS.NOTAMEMBER;
        }

        //------------------------------------------------------------
        // CParser.ScanParameterList
        //
        /// <summary>
        /// メソッドとインデクサのパラメータ部を解析する。
        /// </summary>
        /// <param name="isIndexer">インデクサである場合に true をセットする。</param>
        /// <param name="hadParms">実際にパラメータがあるなら true を返す。</param>
        /// <returns>正常に解析できた場合は true を返す。</returns>
        //------------------------------------------------------------
        internal bool ScanParameterList(bool isIndexer, out bool hadParms)
        {
            // NOTE:  This function completely barfs on parameters with attributes.  Need
            // the ability to scan an expression, which we don't (yet) have...
            // NOTE:  barfing on attributes is exactly what we want when we call this to
            // detect anonymous methods, so if you change this to recognize attributes
            // you might break anonymous methods

            TOKENID openTokenId = isIndexer ? TOKENID.OPENSQUARE : TOKENID.OPENPAREN;
            TOKENID closeTokenId = isIndexer ? TOKENID.CLOSESQUARE : TOKENID.CLOSEPAREN;

            hadParms = false;
            if (CurrentTokenID() != openTokenId)
            {
                return false;
            }

            NextToken();
            if (CurrentTokenID() != closeTokenId)
            {
                while (true)
                {
                    if (CurrentTokenID() == TOKENID.ARGS)
                    {
                        NextToken();
                        break;
                    }

                    if (CurrentTokenID() == TOKENID.PARAMS)
                    {
                        NextToken();
                    }

                    if (CurrentTokenID() == TOKENID.REF || CurrentTokenID() == TOKENID.OUT)
                    {
                        NextToken();
                    }

                    ScanTypeFlagsEnum st = ScanType();

                    if (st == ScanTypeFlagsEnum.NotType)
                    {
                        return false;
                    }

                    if (CurrentTokenID() != TOKENID.IDENTIFIER)
                    {
                        return false;
                    }

                    if (st == ScanTypeFlagsEnum.MustBeType || st == ScanTypeFlagsEnum.AliasQualName)
                    {
                        hadParms = true;
                    }

                    NextToken();
                    if (CurrentTokenID() == TOKENID.COMMA)
                    {
                        NextToken();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (CurrentTokenID() == closeTokenId)
            {
                NextToken();
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.ScanCast
        //
        /// <summary>
        /// <para>This function returns true if the current token begins the following form:
        /// <code>
        /// ( &lt;type&gt; ) &lt;term-or-unary-operator&gt;
        /// </code></para>
        /// <para>調べたところまで処理位置が移動する。</para>
        /// </summary>
        /// <returns>キャストであるかを示す bool 値。</returns>
        //------------------------------------------------------------
        internal bool ScanCast()
        {
            if (CurrentTokenID() != TOKENID.OPENPAREN)
            {
                return false;
            }

            NextToken();
            ScanTypeFlagsEnum isType = ScanType();
            if (isType == ScanTypeFlagsEnum.NotType)
            {
                return false;
            }

            if (CurrentTokenID() != TOKENID.CLOSEPAREN)
            {
                return false;
            }

            if (isType == ScanTypeFlagsEnum.PointerOrMult ||
                isType == ScanTypeFlagsEnum.NullableType)
            {
                return true;
            }

            NextToken();

            // check for ambiguous type or
            // expression followed by disambiguating token
            //                  - or -
            // non-ambiguous type

            // TOKFLAGS.F_CASTEXPR がセットされているトークンは以下の通り。
            //
            // "as", "is", ";", ")", "]", "{", "}", ",",
            // "=", "+=", "-=", "*=", "/=", "%=", "&=", "^=", "|=", "<<=", ">>=",
            // "?", ":", "||", "&&", "|", "^", "&", "==", "!=", "<", "<=", ">", ">=",
            // "<<", ">>", "+", "-", "*", "/", "%", "++", "--", "[", ".", "->", "??", ""

            if ((IsAnyTypeOrExpr(isType) &&
                (TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_CASTEXPR) == 0)
                ||
                (isType == ScanTypeFlagsEnum.MustBeType || isType == ScanTypeFlagsEnum.AliasQualName))
            {
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.ScanType
        //
        /// <summary>
        /// <para>Returns true if current token begins a valid type construct
        /// (doesn't build a parse tree for it)</para>
        /// <para>現在のトークン列が型を表しているかを調べる。
        /// ノード等は作成しないが、正常に処理できたところまで処理位置が移動する。</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ScanTypeFlagsEnum ScanType()
        {
            ScanTypeFlagsEnum result = ScanTypeFlagsEnum.Unknown;

            // 識別子の場合
            if (IsNameStart(CurrentTokenID()))
            {
                // 識別子を読み取る。
                // SimpleName でも GenericName でもないなら NotType を返す。
                ScanNamedTypePartEnum partResult = ScanNamedTypePart();
                if (partResult == ScanNamedTypePartEnum.NotName)
                {
                    return ScanTypeFlagsEnum.NotType;
                }

                if (result != ScanTypeFlagsEnum.MustBeType)
                {
                    result = PartScanToTypeScan(partResult);
                }

                bool isAlias = (CurrentTokenID() == TOKENID.COLONCOLON);

                // Scan a name
                // 得られた Name の後が . か :: ならさらに識別子を読み取る。
                for (bool firstLoop = true;
                    (CurrentTokenID() == TOKENID.DOT || CurrentTokenID() == TOKENID.COLONCOLON);
                    firstLoop = false)
                {
                    if (!firstLoop && isAlias)
                    {
                        isAlias = false;
                    }
                    NextToken();

                    // SimpleName か GenericName か、または名前ではないか。
                    partResult = ScanNamedTypePart();

                    if (partResult == ScanNamedTypePartEnum.NotName)
                    {
                        return ScanTypeFlagsEnum.NotType;
                    }

                    // ScanNamedTypePartEnum を ScanTypeFlagsEnum へ変換
                    result = PartScanToTypeScan(partResult);
                }

                if (isAlias)
                {
                    result = ScanTypeFlagsEnum.AliasQualName;
                }
            }
            else if ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_PREDEFINED) != 0)
            {
                // Simple type...
                NextToken();
                result = ScanTypeFlagsEnum.MustBeType;
            }
            else
            {
                // Can't be a type!
                return ScanTypeFlagsEnum.NotType;
            }

            //ASSERT(result != ScanTypeFlags::Unknown);

            if (CurrentTokenID() == TOKENID.QUESTION)
            {
                NextToken();
                result = ScanTypeFlagsEnum.NullableType;
            }

            // Now check for pointer type(s)
            while (CurrentTokenID() == TOKENID.STAR)
            {
                NextToken();
                if (IsAnyTypeOrExpr(result))
                {
                    result = ScanTypeFlagsEnum.PointerOrMult;
                }
            }

            // Finally, check for array types and nullables.
            // 最後に配列でないかどうか調べる。
            while (CurrentTokenID() == TOKENID.OPENSQUARE)
            {
                NextToken();
                if (CurrentTokenID() != TOKENID.CLOSESQUARE)
                {
                    while (CurrentTokenID() == TOKENID.COMMA)
                    {
                        NextToken();
                    }
                    if (CurrentTokenID() != TOKENID.CLOSESQUARE)
                    {
                        return ScanTypeFlagsEnum.NotType;
                    }
                }
                NextToken();
                result = ScanTypeFlagsEnum.MustBeType;
            }

            return result;
        }

        //------------------------------------------------------------
        // CParser.ScanNamedTypePart
        //
        /// <summary>
        /// <para>Examin the current token and
        /// return the value of type ScanNamedTypePartEnum.</para>
        /// <list type="bullet">
        /// <item>NotName: not an identifier.</item>
        /// <item>GenericName: identifier &lt;...&gt;.</item>
        /// <item>SimpleName: otherwise.</item>
        /// </list>
        /// <para>Advance the current token to next of the name.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ScanNamedTypePartEnum ScanNamedTypePart()
        {
            if (CurrentTokenID() != TOKENID.IDENTIFIER)
            {
                return ScanNamedTypePartEnum.NotName;
            }

            // 識別子である場合は次のトークンへ移動する。
            if (NextToken() == TOKENID.OPENANGLE)
            {
                // < , > の形式であるかどうか調べる。
                if (!ScanOptionalInstantiation())
                {
                    return ScanNamedTypePartEnum.NotName;
                }
                return ScanNamedTypePartEnum.GenericName;
            }
            else
            {
                return ScanNamedTypePartEnum.SimpleName;
            }
        }

        //------------------------------------------------------------
        // CParser.ScanOptionalInstantiation
        //
        /// <summary>
        /// <para>型引数の指定 "&lt;" 型 "," ... "&gt;" が適切に指定されているかを調べる。
        /// 適切な場合、型引数がない場合は true を返す。間違っていれば false を返す。
        /// 途中で "[" が現れたら true を返して終了する。</para>
        /// <para>調べるだけであるが、適切に記述されているところまで処理位置が移動する。</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ScanOptionalInstantiation()
        {
            if (CurrentTokenID() == TOKENID.OPENANGLE)
            {
                do
                {
                    NextToken();

                    // We currently do not have the ability to scan attributes,
                    // so if this is an open square, we early out and assume it is an attribute

                    if (CurrentTokenID() == TOKENID.OPENSQUARE)
                    {
                        return true;
                    }
                    if (ScanType() == ScanTypeFlagsEnum.NotType)
                    {
                        return false;
                    }

                } while (CurrentTokenID() == TOKENID.COMMA);

                if (CurrentTokenID() != TOKENID.CLOSEANGLE)
                {
                    return false;
                }
                NextToken();
            }
            return true;
        }

        //------------------------------------------------------------
        // CParser.ScanMemberName
        //
        /// <summary>
        /// <para>Get a member name from a sequence of tokens.</para>
        /// <para>This method advances the current token to the next of the name.</para>
        /// </summary>
        /// <returns>Value of type MemberNameEnum which represent a kind of a member name.</returns>
        /// <remarks>
        /// Scans a member name. Does NOT handle:
        ///      operators
        ///      conversions
        ///      constructors
        ///      destructors
        ///      nested types
        ///
        /// Return Value:
        ///        GENERIC_METHOD_NAME,                // alias::I<R>.M<T>
        ///        INDEXER_NAME,                       // alias::I<R>.this
        ///        PROPERTY_OR_EVENT_OR_METHOD_NAME,   // alias::I<R>.M
        ///        SIMPLE_NAME,                        // M
        ///        NOT_MEMBER_NAME,                    // anything else
        ///
        /// Leaves the current token after the member name.
        /// </remarks>
        //------------------------------------------------------------
        internal MemberNameEnum ScanMemberName()
        {
            // The keyword "this" is used as a member name.
            // Exactly, "this" is at the position of a member name in the declaration of indexer.
            if (!IsNameStart(CurrentTokenID()))
            {
                if (CurrentTokenID() == TOKENID.THIS)
                {
                    NextToken();
                    return MemberNameEnum.IndexerName;
                }
                return MemberNameEnum.NotMemberName;
            }
            DebugUtil.Assert(CurrentTokenID() == TOKENID.IDENTIFIER);

            // simplest case of a single id
            // If the next token is not "." or "<" or "::", the name is simple.

            TOKENID tokenId = PeekToken();
            if (tokenId != TOKENID.DOT && tokenId != TOKENID.OPENANGLE && tokenId != TOKENID.COLONCOLON)
            {
                NextToken();
                return MemberNameEnum.SimpleName;
            }

            while (true)
            {
                if (CurrentTokenID() == TOKENID.THIS)
                {
                    NextToken();
                    return MemberNameEnum.IndexerName;
                }

                ScanNamedTypePartEnum typePart = ScanNamedTypePart();
                if (typePart == ScanNamedTypePartEnum.NotName)
                {
                    return MemberNameEnum.NotMemberNameWithDot;
                }

                if (CurrentTokenID() != TOKENID.DOT && CurrentTokenID() != TOKENID.COLONCOLON)
                {
                    if (typePart == ScanNamedTypePartEnum.SimpleName)
                    {
                        return MemberNameEnum.PropertyOrEventOrMethodName;
                    }
                    else
                    {
                        return MemberNameEnum.GenericMethodName;
                    }
                }
                NextToken();
            }
            //DebugUtil.Assert(false);
        }

        // Literals

        //------------------------------------------------------------
        // CParser.ScanStringLiteral
        //
        /// <summary>
        /// <para>Returns the STRCONST for either a TID_STRINGLIT or TID_VSLITERAL string,
        /// which was constructed at lex time.</para>
        /// <para>STRINGLIT か VSLITERAL の場合は文字列を返す。</para>
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string ScanStringLiteral(int tokenIndex)
        {
            if (tokenArray[tokenIndex].TokenID == TOKENID.VSLITERAL)
            {
                return tokenArray[tokenIndex].VSLiteral.Str;
            }

            if (tokenArray[tokenIndex].TokenID == TOKENID.STRINGLIT)
            {
                return tokenArray[tokenIndex].StringLiteral.Str;
            }

            throw new LogicError("CParser.ScanStringLiteral");
        }

        //------------------------------------------------------------
        // CParser.ScanCharLiteral
        //
        /// <summary>
        /// Scan a character literal and return its value.
        /// </summary>
        /// <param name="iTokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char ScanCharLiteral(int iTokenIndex)
        {
            return tokenArray[iTokenIndex].CharValue();
        }

        //------------------------------------------------------------
        // CParser.ScanNumericLiteral
        //
        // integer-literal:
        //     decimal-integer-literal
        //     hexadecimal-integer-literal
        //
        // decimal-integer-literal:
        //     decimal-digits  [integer-type-suffix]
        //
        // decimal-digits:
        //     decimal-digit
        //     decimal-digits  decimal-digit
        //
        // decimal-digit:  one of
        //     "0"  "1"  "2"  "3"  "4"  "5"  "6"  "7"  "8"  "9"
        //
        // integer-type-suffix:  one of
        //     "U"  "u"  "L"  "l"  "UL"  "Ul"  "uL"  "ul"  "LU"  "Lu"  "lU"  "lu"
        //
        // hexadecimal-integer-literal:
        //     "0x"  hex-digits  [integer-type-suffix]
        //     "0X"  hex-digits  [integer-type-suffix]
        //
        // hex-digits:
        //     hex-digit
        //     hex-digits  hex-digit
        //
        // hex-digit:  one of
        //     "0"  "1"  "2"  "3"  "4"  "5"  "6"  "7"  "8"  "9"
        //     "A"  "B"  "C"  "D"  "E"  "F"  "a"  "b"  "c"  "d"  "e"  "f"
        //
        // real-literal:
        //     decimal-digits  "."  decimal-digits  [exponent-part]  [real-type-suffix]
        //     "."  decimal-digits  [exponent-part]  [real-type-suffix]
        //     decimal-digits  exponent-part  real-type-suffixopt
        //     decimal-digits  real-type-suffix
        //
        // exponent-part:
        //     "e"  [sign]  decimal-digits
        //     "E"  [sign]  decimal-digits
        //
        // sign:  one of
        //     "+"  "-"
        //
        // real-type-suffix:  one of
        //     "F"  "f"  "D"  "d"  "M"  "m"
        //
        /// <summary>
        /// <para>Re-scan a number token and convert it to a numeric value,
        /// filling in the provided CONSTVALNODE structure.</para>
        /// </summary>
        /// <param name="iTokenIndex"></param>
        /// <param name="constNode"></param>
        //------------------------------------------------------------
        internal void ScanNumericLiteral(int iTokenIndex, CONSTVALNODE constNode)
        {
            // Create a working copy of the number (removing any escapes)
            string tokenString = tokenArray[iTokenIndex].Literal.Text;
            if (String.IsNullOrEmpty(tokenString))
            {
                return;
            }
            int tokenLength = tokenString.Length;

            if (constNode.Value == null)
            {
                constNode.Value = new CONSTVAL();
            }

            string numberString = null;
            int numberStrLength = 0;
            int index = 0;
            char ch = (char)0;
            char lastCharConverted = (char)0;
            char lastCharNotConverted = (char)0;
            bool isReal = false;
            bool isExponent = false;
            bool isNonZero = false; // for floating point only
            bool isUnsigned = false;
            bool isLong = false;
            int suffixLength = 0;
            Exception excp = null;

            //--------------------------------------------------------
            // CParser.ScanNumericLiteral
            // 接尾辞の直前までの部分列を取り出す。
            // その際、整数か実数か、指数部のインデックス、接尾辞のインデックスを調べる。
            //--------------------------------------------------------
            while (index < tokenLength)
            {
                ch = tokenString[index];
                switch (ch)
                {
                    case '.':
                        isReal = true;
                        break;

                    case 'e':
                    case 'E':
                        isExponent = true;
                        //exponentIndex = buffer.Length - 1;
                        break;

                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        isNonZero = (isNonZero || !isExponent);
                        break;
                }
                ++index;
            }

            // At this point, ch will contain the suffix, if any.  Make it the lower-case version
            // save away possible uppercase version (just for warning purposes)
            lastCharConverted = (char)(ch | (char)0x20);    // convert to lower case.
            lastCharNotConverted = ch;

            TOKENID tokenType = TOKENID.UNDEFINED;
            bool isHex =
                tokenLength >= 2 &&
                (tokenString[0] == '0' && (tokenString[1] == 'x' || tokenString[1] == 'X'));

            //--------------------------------------------------------
            // For error reporting, determine the type name of this constant
            // based on the suffix
            //
            // CParser.ScanNumericLiteral
            // 接尾辞を調べ型を求める。
            //--------------------------------------------------------
            if (lastCharConverted == 'd')
            {
                if (!isHex)
                {
                    tokenType = TOKENID.DOUBLE;
                    isReal = true;
                    suffixLength = 1;
                }
            }
            else if (lastCharConverted == 'f')
            {
                if (!isHex)
                {
                    tokenType = TOKENID.FLOAT;
                    isReal = true;
                    suffixLength = 1;
                }
            }
            else if (lastCharConverted == 'm')
            {
                tokenType = TOKENID.DECIMAL;
                isReal = true;
                suffixLength = 1;
            }
            else if (lastCharConverted == 'l')
            {
                isLong = true;
                if (tokenLength >= 2)
                {
                    isUnsigned = ((tokenString[tokenLength - 2] | 0x20) == 'u');
                }
                if (lastCharNotConverted == 'l' && !isUnsigned)
                {
                    // warn about 'l' instead of 'L' (but not 'ul' -- that not a probl
                    Error(CSCERRID.WRN_LowercaseEllSuffix);
                }

                suffixLength = isUnsigned ? 2 : 1;
            }
            else if (lastCharConverted == 'u')
            {
                isUnsigned = true;
                if (tokenLength >= 2)
                {
                    isLong = ((tokenString[tokenLength - 2] | 0x20) == 'l');
                }
                if (isLong && tokenString[tokenLength - 2] == 'l')
                {
                    // warn about 'l' instead of 'L'
                    Error(CSCERRID.WRN_LowercaseEllSuffix);
                }

                suffixLength = isLong ? 2 : 1;
            }
            else
            {
                tokenType = (!isHex && (isReal || isExponent)) ? TOKENID.DOUBLE : TOKENID.INT;
            }

            if (isReal && isHex)
            {
                Error(CSCERRID.ERR_InvalidNumber);
            }

            if (suffixLength > 0)
            {
                numberString = tokenString.Substring(0, tokenLength - suffixLength);
                numberStrLength = numberString.Length;
            }
            else
            {
                numberString = tokenString;
                numberStrLength = tokenLength;
            }

            //--------------------------------------------------------
            // CParser.ScanNumericLiteral   16 進表記の場合。
            //--------------------------------------------------------
            if (isHex)
            {
                // Hex number.
                ulong val = 0;

                for (int i = 2; i < numberStrLength; ++i)
                {
                    char curChar = numberString[i];
                    if (!CharUtil.IsHexDigit(curChar))
                    {
                        break;
                    }

                    try
                    {
                        val = checked((((ulong)val) << 4) | ((ulong)(uint)CharUtil.HexValue(curChar)));
                    }
                    catch (OverflowException)
                    {
                        Error(CSCERRID.ERR_IntOverflow, new ErrArg(tokenString));
                        break;
                    }
                }

                if (numberStrLength <= 2)
                {
                    Error(CSCERRID.ERR_InvalidNumber, new ErrArg(tokenString));
                }

                if (!isLong && ((val >> 32) == 0))
                {
                    if (isUnsigned || (int)val < 0)
                    {
                        constNode.Value.SetUInt((uint)val);
                        constNode.PredefinedType = PREDEFTYPE.UINT;
                    }
                    else
                    {
                        constNode.Value.SetInt((int)val);
                        constNode.PredefinedType = PREDEFTYPE.INT;
                    }
                }
                else
                {
                    if (isUnsigned || (long)val < 0)
                    {
                        constNode.Value.SetULong((ulong)val);
                        constNode.PredefinedType = PREDEFTYPE.ULONG;
                    }
                    else
                    {
                        constNode.Value.SetLong((long)val);
                        constNode.PredefinedType = PREDEFTYPE.LONG;
                    }
                }
                return;
            }

            //--------------------------------------------------------
            // CParser.ScanNumericLiteral   浮動小数点数の場合。
            //--------------------------------------------------------
            if (isReal || isExponent)
            {
                // Floating point number.
                // If it is a decimal, parse it as such to preserve significant digits.
                // Otherwise:  Check and remove the suffix (if there) and scan it as a double,
                // then downcast to float if the suffix is 'f'.

                bool isConvertError = false;

                //----------------------------------------------------
                // Decimal が指定されている場合
                //----------------------------------------------------
                if (tokenType == TOKENID.DECIMAL)
                {
                    Decimal dec;

                    try
                    {
                        dec = Decimal.Parse(numberString);
                    }
                    catch (ArgumentNullException ex)
                    {
                        excp = ex;
                        dec = (decimal)0;
                    }
                    catch (FormatException ex)
                    {
                        excp = ex;
                        dec = (decimal)0;
                    }
                    catch (OverflowException ex)
                    {
                        excp = ex;
                        dec = (decimal)0;
                    }

                    if (excp != null)
                    {
                        Error(CSCERRID.ERR_FloatOverflow, new ErrArg(tokenString));
                    }

                    constNode.Value.SetDecimal(dec);
                    constNode.PredefinedType = PREDEFTYPE.DECIMAL;
                    return;
                }
                //----------------------------------------------------
                // otherwise it is either a double or a float
                //----------------------------------------------------
                else if (tokenType == TOKENID.FLOAT)
                {
                    float sval = 0;

                    if (isNonZero)
                    {
                        try
                        {
                            sval = Single.Parse(numberString);
                        }
                        catch (ArgumentNullException ex)
                        {
                            excp = ex;
                            isConvertError = true;
                            sval = 0;
                        }
                        catch (FormatException ex)
                        {
                            excp = ex;
                            isConvertError = true;
                            sval = 0;
                        }
                        catch (OverflowException ex)
                        {
                            excp = ex;
                            isConvertError = true;
                            sval = 0;
                        }
                    }
                    else
                    {
                        sval = 0;
                    }

                    constNode.Value.SetFloat(sval);
                    constNode.PredefinedType = PREDEFTYPE.FLOAT;
                    return;
                }

                //----------------------------------------------------
                // otherwise it is a double
                //----------------------------------------------------
                DebugUtil.Assert(tokenType == TOKENID.DOUBLE);
                double dval = 0;

                if (isNonZero)
                {
                    // Convert to a double, set errConvert on error.
                    // Note that we can't use atof here because it is sensitive to setlocale,
                    // and we can't call setlocale ourselves because it is process wide instead of per-thread. (VS7:181428)

                    try
                    {
                        dval = Double.Parse(numberString);
                    }
                    catch (ArgumentNullException ex)
                    {
                        excp = ex;
                        isConvertError = true;
                        dval = 0.0;
                    }
                    catch (FormatException ex)
                    {
                        excp = ex;
                        isConvertError = true;
                        dval = 0.0;
                    }
                    catch (OverflowException ex)
                    {
                        excp = ex;
                        isConvertError = true;
                        dval = 0.0;
                    }
                }
                else
                {
                    dval = 0.0;
                }

                constNode.Value.SetDouble(dval);
                constNode.PredefinedType = PREDEFTYPE.DOUBLE;
                return;
            }

            //--------------------------------------------------
            // Must be simple integral form.  We always have to 64bit math because literals can
            // be ints or longs
            //
            // CParser.ScanNumericLiteral   整数の場合。
            //--------------------------------------------------
            // unsigned __int64 val = 0;
            ulong ulval = 0;

            try
            {
                ulval = UInt64.Parse(numberString);
            }
            catch (OverflowException)
            {
                Error(
                    CSCERRID.ERR_FloatOverflow,
                    new ErrArg(TokenInfoArray[(int)tokenType].Text));
            }
            catch (FormatException)
            {
                Error(
                    CSCERRID.ERR_InvalidNumber,
                    new ErrArg(TokenInfoArray[(int)tokenType].Text));
            }
            catch (ArgumentException)
            {
                Error(
                    CSCERRID.ERR_InvalidNumber,
                    new ErrArg(TokenInfoArray[(int)tokenType].Text));
            }

            if (isUnsigned)
            {
                if (isLong || (ulval >> 32) != 0)
                {
                    constNode.Value.SetULong(ulval);
                    constNode.PredefinedType = PREDEFTYPE.ULONG;
                    tokenType = TOKENID.ULONG;
                }
                else
                {
                    constNode.Value.SetUInt((uint)ulval);
                    constNode.PredefinedType = PREDEFTYPE.UINT;
                    tokenType = TOKENID.UINT;
                }
            }
            else
            {
                if (!isLong && (ulval >> 32) == 0)
                {
                    if ((int)ulval >= 0)
                    {
                        constNode.Value.SetInt((int)ulval);
                        constNode.PredefinedType = PREDEFTYPE.INT;
                        tokenType = TOKENID.INT;
                    }
                    else
                    {
                        constNode.Value.SetUInt((uint)ulval);
                        constNode.PredefinedType = PREDEFTYPE.UINT;
                        tokenType = TOKENID.UINT;
                        if (ulval == 0x80000000)
                        {
                            constNode.Flags |= NODEFLAGS.CHECK_FOR_UNARY_MINUS;
                        }
                    }
                }
                else
                {
                    if ((long)ulval >= 0)
                    {
                        constNode.Value.SetLong((long)ulval);
                        constNode.PredefinedType = PREDEFTYPE.LONG;
                        tokenType = TOKENID.LONG;
                    }
                    else
                    {
                        constNode.Value.SetULong((ulong)ulval);
                        constNode.PredefinedType = PREDEFTYPE.ULONG;
                        tokenType = TOKENID.ULONG;
                        if (ulval == 0x8000000000000000)
                        {
                            constNode.Flags |= NODEFLAGS.CHECK_FOR_UNARY_MINUS;
                        }
                    }
                }
            }
            return;
        }

        // Node allocation

        //------------------------------------------------------------
        // CParser.AllocNode
        //
        /// <summary>
        /// <para>This is the base node allocator.  Based on the node kind provided, the right
        /// amount of memory is allocated from the virtual allocation function MemAlloc.</para>
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="parentNode"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE AllocNode(NODEKIND kind, BASENODE parentNode, int tokenIndex)
        {
            BASENODE newNode;

            switch (kind)
            {
                //#if DEBUG
                //#define NODEKIND(n,s,g,p) \
                //    case NK_##n: \
                //        pb = (byte *)MemAlloc (sizeof (s##NODE) + cbExtra); \
                //        pNew = (s##NODE*)(pb + cbExtra); \
                //        new (pNew) s##NODE; \
                //        pNew.pszNodeKind = #s "NODE (NK_" #n ")"; \
                //        pNew.fHasNid = !!cbExtra; \
                //        break;
                //#else
                //#define NODEKIND(n,s,g,p) \
                //    case NK_##n: \
                //        pb = (byte *)MemAlloc (sizeof (s##NODE) + cbExtra); \
                //        pNew = (s##NODE*)(pb + cbExtra); \
                //        break;
                //#endif
                // #include "nodekind.h"

#if DEBUG
                case NODEKIND.ACCESSOR:
                    newNode = new ACCESSORNODE();
                    newNode.NodeKindName = "ACCESSORNODE (NODEKIND.ACCESSOR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ALIASNAME:
                    newNode = new NAMENODE();
                    newNode.NodeKindName = "NAMENODE (NODEKIND.ALIASNAME)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ANONBLOCK:
                    newNode = new ANONBLOCKNODE();
                    newNode.NodeKindName = "ANONBLOCKNODE (NODEKIND.ANONBLOCK)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ARRAYINIT:
                    newNode = new UNOPNODE();
                    newNode.NodeKindName = "UNOPNODE (NODEKIND.ARRAYINIT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ARRAYTYPE:
                    newNode = new ARRAYTYPENODE();
                    newNode.NodeKindName = "ARRAYTYPENODE (NODEKIND.ARRAYTYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ARROW:
                    newNode = new BINOPNODE();
                    newNode.NodeKindName = "BINOPNODE (NODEKIND.ARROW)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ATTR:
                    newNode = new ATTRNODE();
                    newNode.NodeKindName = "ATTRNODE (NODEKIND.ATTR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ATTRARG:
                    newNode = new ATTRNODE();
                    newNode.NodeKindName = "ATTRNODE (NODEKIND.ATTRARG)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ATTRDECL:
                    newNode = new ATTRDECLNODE();
                    newNode.NodeKindName = "ATTRDECLNODE (NODEKIND.ATTRDECL)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.BINOP:
                    newNode = new BINOPNODE();
                    newNode.NodeKindName = "BINOPNODE (NODEKIND.BINOP)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.BLOCK:
                    newNode = new BLOCKNODE();
                    newNode.NodeKindName = "BLOCKNODE (NODEKIND.BLOCK)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.BREAK:
                    newNode = new EXPRSTMTNODE();
                    newNode.NodeKindName = "EXPRSTMTNODE (NODEKIND.BREAK)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CALL:
                    newNode = new CALLNODE();
                    newNode.NodeKindName = "CALLNODE (NODEKIND.CALL)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CASE:
                    newNode = new CASENODE();
                    newNode.NodeKindName = "CASENODE (NODEKIND.CASE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CASELABEL:
                    newNode = new UNOPNODE();
                    newNode.NodeKindName = "UNOPNODE (NODEKIND.CASELABEL)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CATCH:
                    newNode = new CATCHNODE();
                    newNode.NodeKindName = "CATCHNODE (NODEKIND.CATCH)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CHECKED:
                    newNode = new LABELSTMTNODE();
                    newNode.NodeKindName = "LABELSTMTNODE (NODEKIND.CHECKED)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CLASS:
                    newNode = new CLASSNODE();
                    newNode.NodeKindName = "CLASSNODE (NODEKIND.CLASS)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CONST:
                    newNode = new FIELDNODE();
                    newNode.NodeKindName = "FIELDNODE (NODEKIND.CONST)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CONSTRAINT:
                    newNode = new CONSTRAINTNODE();
                    newNode.NodeKindName = "CONSTRAINTNODE (NODEKIND.CONSTRAINT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CONSTVAL:
                    newNode = new CONSTVALNODE();
                    newNode.NodeKindName = "CONSTVALNODE (NODEKIND.CONSTVAL)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CONTINUE:
                    newNode = new EXPRSTMTNODE();
                    newNode.NodeKindName = "EXPRSTMTNODE (NODEKIND.CONTINUE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.CTOR:
                    newNode = new CTORMETHODNODE();
                    newNode.NodeKindName = "CTORMETHODNODE (NODEKIND.CTOR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.DECLSTMT:
                    newNode = new DECLSTMTNODE();
                    newNode.NodeKindName = "DECLSTMTNODE (NODEKIND.DECLSTMT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.DELEGATE:
                    newNode = new DELEGATENODE();
                    newNode.NodeKindName = "DELEGATENODE (NODEKIND.DELEGATE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.DEREF:
                    newNode = new CALLNODE();
                    newNode.NodeKindName = "CALLNODE (NODEKIND.DEREF)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.DO:
                    newNode = new LOOPSTMTNODE();
                    newNode.NodeKindName = "LOOPSTMTNODE (NODEKIND.DO)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.DOT:
                    newNode = new BINOPNODE();
                    newNode.NodeKindName = "BINOPNODE (NODEKIND.DOT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.DTOR:
                    newNode = new METHODBASENODE();
                    newNode.NodeKindName = "METHODBASENODE (NODEKIND.DTOR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.EMPTYSTMT:
                    newNode = new STATEMENTNODE();
                    newNode.NodeKindName = "STATEMENTNODE (NODEKIND.EMPTYSTMT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ENUM:
                    newNode = new ENUMNODE();
                    newNode.NodeKindName = "ENUMNODE (NODEKIND.ENUM)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ENUMMBR:
                    newNode = new ENUMMBRNODE();
                    newNode.NodeKindName = "ENUMMBRNODE (NODEKIND.ENUMMBR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.EXPRSTMT:
                    newNode = new EXPRSTMTNODE();
                    newNode.NodeKindName = "EXPRSTMTNODE (NODEKIND.EXPRSTMT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.FIELD:
                    newNode = new FIELDNODE();
                    newNode.NodeKindName = "FIELDNODE (NODEKIND.FIELD)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.FOR:
                    newNode = new FORSTMTNODE();
                    newNode.NodeKindName = "FORSTMTNODE (NODEKIND.FOR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.GENERICNAME:
                    newNode = new GENERICNAMENODE();
                    newNode.NodeKindName = "GENERICNAMENODE (NODEKIND.GENERICNAME)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.GOTO:
                    newNode = new EXPRSTMTNODE();
                    newNode.NodeKindName = "EXPRSTMTNODE (NODEKIND.GOTO)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.IF:
                    newNode = new IFSTMTNODE();
                    newNode.NodeKindName = "IFSTMTNODE (NODEKIND.IF)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.INTERFACE:
                    newNode = new INTERFACENODE();
                    newNode.NodeKindName = "INTERFACENODE (NODEKIND.INTERFACE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.LABEL:
                    newNode = new LABELSTMTNODE();
                    newNode.NodeKindName = "LABELSTMTNODE (NODEKIND.LABEL)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.LIST:
                    newNode = new BINOPNODE();
                    newNode.NodeKindName = "BINOPNODE (NODEKIND.LIST)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.LOCK:
                    newNode = new LOOPSTMTNODE();
                    newNode.NodeKindName = "LOOPSTMTNODE (NODEKIND.LOCK)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.MEMBER:
                    newNode = new MEMBERNODE();
                    newNode.NodeKindName = "MEMBERNODE (NODEKIND.MEMBER)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.METHOD:
                    newNode = new METHODNODE();
                    newNode.NodeKindName = "METHODNODE (NODEKIND.METHOD)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.NAME:
                    newNode = new NAMENODE();
                    newNode.NodeKindName = "NAMENODE (NODEKIND.NAME)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.NAMEDTYPE:
                    newNode = new NAMEDTYPENODE();
                    newNode.NodeKindName = "NAMEDTYPENODE (NODEKIND.NAMEDTYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.NAMESPACE:
                    newNode = new NAMESPACENODE();
                    newNode.NodeKindName = "NAMESPACENODE (NODEKIND.NAMESPACE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.NESTEDTYPE:
                    newNode = new NESTEDTYPENODE();
                    newNode.NodeKindName = "NESTEDTYPENODE (NODEKIND.NESTEDTYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.NEW:
                    newNode = new NEWNODE();
                    newNode.NodeKindName = "NEWNODE (NODEKIND.NEW)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.NULLABLETYPE:
                    newNode = new NULLABLETYPENODE();
                    newNode.NodeKindName = "NULLABLETYPENODE (NODEKIND.NULLABLETYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.OP:
                    newNode = new BASENODE();
                    newNode.NodeKindName = "BASENODE (NODEKIND.OP)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.OPENNAME:
                    newNode = new OPENNAMENODE();
                    newNode.NodeKindName = "OPENNAMENODE (NODEKIND.OPENNAME)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.OPENTYPE:
                    newNode = new NAMEDTYPENODE();
                    newNode.NodeKindName = "NAMEDTYPENODE (NODEKIND.OPENTYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.OPERATOR:
                    newNode = new OPERATORMETHODNODE();
                    newNode.NodeKindName = "OPERATORMETHODNODE (NODEKIND.OPERATOR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.PARAMETER:
                    newNode = new PARAMETERNODE();
                    newNode.NodeKindName = "PARAMETERNODE (NODEKIND.PARAMETER)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.PARTIALMEMBER:
                    newNode = new PARTIALMEMBERNODE();
                    newNode.NodeKindName = "PARTIALMEMBERNODE (NODEKIND.PARTIALMEMBER)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.POINTERTYPE:
                    newNode = new POINTERTYPENODE();
                    newNode.NodeKindName = "POINTERTYPENODE (NODEKIND.POINTERTYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.PREDEFINEDTYPE:
                    newNode = new PREDEFINEDTYPENODE();
                    newNode.NodeKindName = "PREDEFINEDTYPENODE (NODEKIND.PREDEFINEDTYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.PROPERTY:
                    newNode = new PROPERTYNODE();
                    newNode.NodeKindName = "PROPERTYNODE (NODEKIND.PROPERTY)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.INDEXER:
                    newNode = new PROPERTYNODE();
                    newNode.NodeKindName = "PROPERTYNODE (NODEKIND.INDEXER)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.RETURN:
                    newNode = new EXPRSTMTNODE();
                    newNode.NodeKindName = "EXPRSTMTNODE (NODEKIND.RETURN)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.THROW:
                    newNode = new EXPRSTMTNODE();
                    newNode.NodeKindName = "EXPRSTMTNODE (NODEKIND.THROW)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.TRY:
                    newNode = new TRYSTMTNODE();
                    newNode.NodeKindName = "TRYSTMTNODE (NODEKIND.TRY)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.TYPEWITHATTR:
                    newNode = new TYPEWITHATTRNODE();
                    newNode.NodeKindName = "TYPEWITHATTRNODE (NODEKIND.TYPEWITHATTR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.STRUCT:
                    newNode = new STRUCTNODE();
                    newNode.NodeKindName = "STRUCTNODE (NODEKIND.STRUCT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.SWITCH:
                    newNode = new SWITCHSTMTNODE();
                    newNode.NodeKindName = "SWITCHSTMTNODE (NODEKIND.SWITCH)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.UNOP:
                    newNode = new UNOPNODE();
                    newNode.NodeKindName = "UNOPNODE (NODEKIND.UNOP)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.UNSAFE:
                    newNode = new LABELSTMTNODE();
                    newNode.NodeKindName = "LABELSTMTNODE (NODEKIND.UNSAFE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.USING:
                    newNode = new USINGNODE();
                    newNode.NodeKindName = "USINGNODE (NODEKIND.USING)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.VARDECL:
                    newNode = new VARDECLNODE();
                    newNode.NodeKindName = "VARDECLNODE (NODEKIND.VARDECL)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.WHILE:
                    newNode = new LOOPSTMTNODE();
                    newNode.NodeKindName = "LOOPSTMTNODE (NODEKIND.WHILE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.YIELD:
                    newNode = new EXPRSTMTNODE();
                    newNode.NodeKindName = "EXPRSTMTNODE (NODEKIND.YIELD)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.IMPLICITTYPE: // CS3
                    newNode = new IMPLICITTYPENODE();
                    newNode.NodeKindName = "IMPLICITTYPENODE (NODEKIND.IMPLICITTYPE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.COLLECTIONINIT:   // CS3
                    newNode = new UNOPNODE();
                    newNode.NodeKindName = "UNOPNODE (NODEKIND.COLLECTIONINIT)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.LAMBDAEXPR:  // CS3
                    newNode = new LAMBDAEXPRNODE();
                    newNode.NodeKindName = "LAMBDAEXPRNODE (NODEKIND.LAMBDABLOCK)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.QUERYEXPR:
                    newNode = new QUERYEXPRNODE();
                    newNode.NodeKindName = "QUERYEXPRNODE (NODEKIND.QUERYEXPR)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.FROMCLAUSE:
                    newNode = new FROMCLAUSENODE();
                    newNode.NodeKindName = "FROMCLAUSENODE (NODEKIND.FROMCLAUSE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.FROMCLAUSE2:
                    newNode = new FROMCLAUSENODE2();
                    newNode.NodeKindName = "FROMCLAUSENODE2 (NODEKIND.FROMCLAUSE2)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.LETCLAUSE:
                    newNode = new LETCLAUSENODE();
                    newNode.NodeKindName = "LETCLAUSENODE (NODEKIND.LETCLAUSE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.WHERECLAUSE:
                    newNode = new WHERECLAUSENODE();
                    newNode.NodeKindName = "WHERECLAUSENODE (NODEKIND.WHERECLAUSE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.JOINCLAUSE:
                    newNode = new JOINCLAUSENODE();
                    newNode.NodeKindName = "JOINCLAUSENODE (NODEKIND.JOINCLAUSE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.ORDERBYCLAUSE:
                    newNode = new ORDERBYCLAUSENODE();
                    newNode.NodeKindName = "ORDERBYCLAUSENODE (NODEKIND.ORDERBYCLAUSE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.SELECTCLAUSE:
                    newNode = new SELECTCLAUSENODE();
                    newNode.NodeKindName = "SELECTCLAUSENODE (NODEKIND.SELECTCLAUSE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.GROUPCLAUSE:
                    newNode = new GROUPCLAUSENODE();
                    newNode.NodeKindName = "GROUPCLAUSENODE (NODEKIND.GROUPCLAUSE)";
                    newNode.HasNid = true;
                    break;

                case NODEKIND.QUERYCONTINUATION:
                    newNode = new QUERYCONTINUATIONNODE();
                    newNode.NodeKindName = "QUERYCONTINUATIONNODE (NODEKIND.QUERYCONTINUATION)";
                    newNode.HasNid = true;
                    break;

#else   // #if DEBUG

                case NODEKIND.ACCESSOR:
                    newNode = new ACCESSORNODE();
                    break;

                case NODEKIND.ALIASNAME:
                    newNode = new NAMENODE();
                    break;

                case NODEKIND.ANONBLOCK:
                    newNode = new ANONBLOCKNODE();
                    break;

                case NODEKIND.ARRAYINIT:
                    newNode = new UNOPNODE();
                    break;

                case NODEKIND.ARRAYTYPE:
                    newNode = new ARRAYTYPENODE();
                    break;

                case NODEKIND.ARROW:
                    newNode = new BINOPNODE();
                    break;

                case NODEKIND.ATTR:
                    newNode = new ATTRNODE();
                    break;

                case NODEKIND.ATTRARG:
                    newNode = new ATTRNODE();
                    break;

                case NODEKIND.ATTRDECL:
                    newNode = new ATTRDECLNODE();
                    break;

                case NODEKIND.BINOP:
                    newNode = new BINOPNODE();
                    break;

                case NODEKIND.BLOCK:
                    newNode = new BLOCKNODE();
                    break;

                case NODEKIND.BREAK:
                    newNode = new EXPRSTMTNODE();
                    break;

                case NODEKIND.CALL:
                    newNode = new CALLNODE();
                    break;

                case NODEKIND.CASE:
                    newNode = new CASENODE();
                    break;

                case NODEKIND.CASELABEL:
                    newNode = new UNOPNODE();
                    break;

                case NODEKIND.CATCH:
                    newNode = new CATCHNODE();
                    break;

                case NODEKIND.CHECKED:
                    newNode = new LABELSTMTNODE();
                    break;

                case NODEKIND.CLASS:
                    newNode = new CLASSNODE();
                    break;

                case NODEKIND.CONST:
                    newNode = new FIELDNODE();
                    break;

                case NODEKIND.CONSTRAINT:
                    newNode = new CONSTRAINTNODE();
                    break;

                case NODEKIND.CONSTVAL:
                    newNode = new CONSTVALNODE();
                    break;

                case NODEKIND.CONTINUE:
                    newNode = new EXPRSTMTNODE();
                    break;

                case NODEKIND.CTOR:
                    newNode = new CTORMETHODNODE();
                    break;

                case NODEKIND.DECLSTMT:
                    newNode = new DECLSTMTNODE();
                    break;

                case NODEKIND.DELEGATE:
                    newNode = new DELEGATENODE();
                    break;

                case NODEKIND.DEREF:
                    newNode = new CALLNODE();
                    break;

                case NODEKIND.DO:
                    newNode = new LOOPSTMTNODE();
                    break;

                case NODEKIND.DOT:
                    newNode = new BINOPNODE();
                    break;

                case NODEKIND.DTOR:
                    newNode = new METHODBASENODE();
                    break;

                case NODEKIND.EMPTYSTMT:
                    newNode = new STATEMENTNODE();
                    break;

                case NODEKIND.ENUM:
                    newNode = new ENUMNODE();
                    break;

                case NODEKIND.ENUMMBR:
                    newNode = new ENUMMBRNODE();
                    break;

                case NODEKIND.EXPRSTMT:
                    newNode = new EXPRSTMTNODE();
                    break;

                case NODEKIND.FIELD:
                    newNode = new FIELDNODE();
                    break;

                case NODEKIND.FOR:
                    newNode = new FORSTMTNODE();
                    break;

                case NODEKIND.GENERICNAME:
                    newNode = new GENERICNAMENODE();
                    break;

                case NODEKIND.GOTO:
                    newNode = new EXPRSTMTNODE();
                    break;

                case NODEKIND.IF:
                    newNode = new IFSTMTNODE();
                    break;

                case NODEKIND.INTERFACE:
                    newNode = new INTERFACENODE();
                    break;

                case NODEKIND.LABEL:
                    newNode = new LABELSTMTNODE();
                    break;

                case NODEKIND.LIST:
                    newNode = new BINOPNODE();
                    break;

                case NODEKIND.LOCK:
                    newNode = new LOOPSTMTNODE();
                    break;

                case NODEKIND.MEMBER:
                    newNode = new MEMBERNODE();
                    break;

                case NODEKIND.METHOD:
                    newNode = new METHODNODE();
                    break;

                case NODEKIND.NAME:
                    newNode = new NAMENODE();
                    break;

                case NODEKIND.NAMEDTYPE:
                    newNode = new NAMEDTYPENODE();
                    break;

                case NODEKIND.NAMESPACE:
                    newNode = new NAMESPACENODE();
                    break;

                case NODEKIND.NESTEDTYPE:
                    newNode = new NESTEDTYPENODE();
                    break;

                case NODEKIND.NEW:
                    newNode = new NEWNODE();
                    break;

                case NODEKIND.NULLABLETYPE:
                    newNode = new NULLABLETYPENODE();
                    break;

                case NODEKIND.OP:
                    newNode = new BASENODE();
                    break;

                case NODEKIND.OPENNAME:
                    newNode = new OPENNAMENODE();
                    break;

                case NODEKIND.OPENTYPE:
                    newNode = new NAMEDTYPENODE();
                    break;

                case NODEKIND.OPERATOR:
                    newNode = new OPERATORMETHODNODE();
                    break;

                case NODEKIND.PARAMETER:
                    newNode = new PARAMETERNODE();
                    break;

                case NODEKIND.PARTIALMEMBER:
                    newNode = new PARTIALMEMBERNODE();
                    break;

                case NODEKIND.POINTERTYPE:
                    newNode = new POINTERTYPENODE();
                    break;

                case NODEKIND.PREDEFINEDTYPE:
                    newNode = new PREDEFINEDTYPENODE();
                    break;

                case NODEKIND.PROPERTY:
                    newNode = new PROPERTYNODE();
                    break;

                case NODEKIND.INDEXER:
                    newNode = new PROPERTYNODE();
                    break;

                case NODEKIND.RETURN:
                    newNode = new EXPRSTMTNODE();
                    break;

                case NODEKIND.THROW:
                    newNode = new EXPRSTMTNODE();
                    break;

                case NODEKIND.TRY:
                    newNode = new TRYSTMTNODE();
                    break;

                case NODEKIND.TYPEWITHATTR:
                    newNode = new TYPEWITHATTRNODE();
                    break;

                case NODEKIND.STRUCT:
                    newNode = new STRUCTNODE();
                    break;

                case NODEKIND.SWITCH:
                    newNode = new SWITCHSTMTNODE();
                    break;

                case NODEKIND.UNOP:
                    newNode = new UNOPNODE();
                    break;

                case NODEKIND.UNSAFE:
                    newNode = new LABELSTMTNODE();
                    break;

                case NODEKIND.USING:
                    newNode = new USINGNODE();
                    break;

                case NODEKIND.VARDECL:
                    newNode = new VARDECLNODE();
                    break;

                case NODEKIND.WHILE:
                    newNode = new LOOPSTMTNODE();
                    break;

                case NODEKIND.YIELD:
                    newNode = new EXPRSTMTNODE();
                    break;

                case NODEKIND.IMPLICITTYPE: // CS3
                    newNode = new IMPLICITTYPENODE();
                    break;

                case NODEKIND.COLLECTIONINIT:
                    newNode = new UNOPNODE();
                    break;

                case NODEKIND.LAMBDAEXPR:  // CS3
                    newNode = new LAMBDAEXPRNODE();
                    break;

                case NODEKIND.QUERYEXPR:
                    newNode = new QUERYEXPRNODE();
                    break;

                case NODEKIND.FROMCLAUSE:
                    newNode = new FROMCLAUSENODE();
                    break;

                case NODEKIND.FROMCLAUSE2:
                    newNode = new FROMCLAUSENODE2();
                    break;

                case NODEKIND.LETCLAUSE:
                    newNode = new LETCLAUSENODE();
                    break;

                case NODEKIND.WHERECLAUSE:
                    newNode = new WHERECLAUSENODE();
                    break;

                case NODEKIND.JOINCLAUSE:
                    newNode = new JOINCLAUSENODE();
                    break;

                case NODEKIND.ORDERBYCLAUSE:
                    newNode = new ORDERBYCLAUSENODE();
                    break;

                case NODEKIND.SELECTCLAUSE:
                    newNode = new SELECTCLAUSENODE();
                    break;

                case NODEKIND.GROUPCLAUSE:
                    newNode = new GROUPCLAUSENODE();
                    break;

                case NODEKIND.QUERYCONTINUATION:
                    newNode = new QUERYCONTINUATIONNODE();
                    break;

#endif  // #if DEBUG
                default:
                    //VSFAIL("Unknown node kind passed to AllocNode!");
                    //pb = (byte*)MemAlloc(sizeof(BASENODE) + cbExtra);
                    newNode = new BASENODE();
                    break;
            }

            newNode.Kind = kind;
            newNode.Flags = 0;
            newNode.NodeFlagsEx = 0;
            newNode.Operator = 0;
            newNode.PredefinedType = 0;
            newNode.TokenIndex = tokenIndex;
            newNode.ParentNode = parentNode;

            return newNode;
        }

        //------------------------------------------------------------
        // CParser.AllocNode
        //
        /// <summary>
        /// <para>Create a node of a given type.</para>
        /// <para>tokenIndex is set by the return value of CurrentTokenIndex().</para>
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="nodePar"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE AllocNode(NODEKIND kind, BASENODE nodePar)
        {
            return AllocNode(kind, nodePar, CurrentTokenIndex());
        }

        //------------------------------------------------------------
        // CParser.AllocDotNode
        //
        /// <summary>
        /// <para>Create a BINOPNODE instance
        /// to represent a dot representing member-access.</para>
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <param name="parent"></param>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BINOPNODE AllocDotNode(
            int tokenIndex,
            BASENODE parent,
            BASENODE op1,
            BASENODE op2)
        {
            BINOPNODE dotNode = AllocNode(NODEKIND.DOT, parent, tokenIndex).AsDOT;
            dotNode.Operand1 = op1;
            dotNode.Operand2 = op2;
            op1.ParentNode = op2.ParentNode = dotNode;
            return dotNode;
        }

        //------------------------------------------------------------
        // CParser.AllocBinaryOpNode
        //
        /// <summary></summary>
        /// <param name="operatorId"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="parentNode"></param>
        /// <param name="operandNode1"></param>
        /// <param name="operandNode2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BINOPNODE AllocBinaryOpNode(
            OPERATOR operatorId,
            int tokenIndex,
            BASENODE parentNode,
            BASENODE operandNode1,
            BASENODE operandNode2)
        {
            DebugUtil.Assert(operandNode1 != null);
            //ASSERT(operandNode1 && operandNode2);

            BINOPNODE binopNode
                = AllocNode(NODEKIND.BINOP, parentNode, tokenIndex) as BINOPNODE;
            binopNode.Operand1 = operandNode1;
            binopNode.Operand2 = operandNode2;
            binopNode.Operator = operatorId;
            //ASSERT (operandNode1 != null);
            operandNode1.ParentNode = binopNode;
            if (operandNode2 != null)
            {
                operandNode2.ParentNode = binopNode;
            }
            return binopNode;
        }

        //------------------------------------------------------------
        // CParser.AllocUnaryOpNode
        //
        /// <summary>単項演算子が付いた項を表す UNOPNODE インスタンスを作成する。</summary>
        /// <param name="operatorId">演算子に種別を表す OPERATOR 型の値。</param>
        /// <param name="tokenIndex">UNOPNODE に設定するトークンインデックス。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <param name="operandNode">オペランドとなる項を表すノード。</param>
        /// <returns>作成した UNOPNODE。</returns>
        //------------------------------------------------------------
        internal UNOPNODE AllocUnaryOpNode(
            OPERATOR operatorId,
            int tokenIndex,
            BASENODE parentNode,
            BASENODE operandNode)
        {
            //ASSERT(p1);
            UNOPNODE opNode
                = AllocNode(NODEKIND.UNOP, parentNode, tokenIndex) as UNOPNODE;
            opNode.Operand = operandNode;
            opNode.Operator = operatorId;
            operandNode.ParentNode = opNode;
            return opNode;
        }

        //------------------------------------------------------------
        // CParser.AllocOpNode
        //
        /// <summary></summary>
        /// <param name="op"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        BASENODE AllocOpNode(OPERATOR op, int tokenIndex, BASENODE parentNode)
        {
            BASENODE pOp = AllocNode(NODEKIND.OP, parentNode, tokenIndex).AsOP;
            pOp.Operator = op;
            return pOp;
        }

        //------------------------------------------------------------
        // CParser.AllocNameNode
        //
        /// <summary>
        /// <para>Create a NAMENODE instance and set a name.</para>
        /// <para>If argument name is null, set the name of the specified token.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NAMENODE AllocNameNode(string name, int tokenIndex)
        {
            NAMENODE nameNode = AllocNode(NODEKIND.NAME, null, tokenIndex) as NAMENODE;
            InitNameNode(nameNode, name);
            return nameNode;
        }

        //------------------------------------------------------------
        // CParser.AllocGenericNameNode
        //
        /// <summary>
        /// <para>Create a GENERICNAMENODE instance and set a name.</para>
        /// <para>If argument name is null, set the name of the specified token.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal GENERICNAMENODE AllocGenericNameNode(string name, int tokenIndex)
        {
            GENERICNAMENODE nameNode =
                AllocNode(NODEKIND.GENERICNAME, null, tokenIndex) as GENERICNAMENODE;
            InitNameNode(nameNode, name);
            nameNode.OpenAngleIndex = -1;
            nameNode.CloseAngleIndex = -1;
            return nameNode;
        }

        //------------------------------------------------------------
        // CParser.InitNameNode
        //
        /// <summary>
        /// <para>Set a name to a NAMENODE instance.</para>
        /// <para>If argument name is null, set the name of the specified token.</para>
        /// </summary>
        /// <param name="nameNode"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NAMENODE InitNameNode(NAMENODE nameNode, string name)
        {
            DebugUtil.Assert(0 <= nameNode.TokenIndex && nameNode.TokenIndex < tokenCount);

            if (name == null)
            {
                nameNode.Name = tokenArray[nameNode.TokenIndex].Name;
            }
            else
            {
                nameNode.Name = name;
            }

            // (CS3) Query Expression
            if (IsQueryExpressionParseMode() &&CurrentTokenID()==TOKENID.IDENTIFIER)
            {
                QueryKeywordEnum qkw = GetQueryKeywordID(nameNode.Name);
                if (qkw != QueryKeywordEnum.None &&
                    qkw != QueryKeywordEnum.From)
                {
                    Error(CSCERRID.ERR_IdentifierExpected);
                    return ParseMissingName(nameNode.ParentNode, CurrentTokenIndex());
                }
            }

            if ((tokenArray[nameNode.TokenIndex].UserBits & (int)TOKFLAGS.VERBATIMSTRING) != 0)
            {
                nameNode.Flags |= NODEFLAGS.NAME_LITERAL;
            }
            nameNode.PossibleGenericName = null;
            return nameNode;
        }

        // Top-level parsing methods

        //------------------------------------------------------------
        // CParser.ParseSourceModule
        //
        /// <summary>
        /// Call ParseRootNamespace method.
        /// This method creates a NAMENODENODE representing the top namespace.
        /// </summary>
        /// <returns>NAMESPACENODE instance representing the root namespace.</returns>
        //------------------------------------------------------------
        virtual internal BASENODE ParseSourceModule()
        {
            DebugUtil.Assert(tokenArray != null, "Parser state not set");

            // Not that it matters, but start the error count at 0
            this.parseErrorCount = 0;
            this.isErrorOnCurrentToken = false;

            // Source modules always start at the first non-noise token in any stream.
            // Skip Noisy tokens.
            this.currentTokenIndex = 0;
            while ((TokenInfoArray[(int)tokenArray[currentTokenIndex].TokenID].Flags & TOKFLAGS.F_NOISE) != 0)
            {
                currentTokenIndex++;
            }

            // Every source module belongs in an unnamed namespace...
            // Create a top namespace.
            // BREAKPOINT_PARSE
            NAMESPACENODE nsNode = ParseRootNamespace();

            // We're done!  Thus, we should be at end-of-file.
            if (CurrentTokenID() != TOKENID.ENDFILE)
            {
                Error(CSCERRID.ERR_EOFExpected);
            }
            return nsNode;
        }

        //------------------------------------------------------------
        // CParser.ParseRootNamespace
        //
        /// <summary>
        /// <list type="number">
        /// <para>Create a top namespace whose name is null and key is "".</para>
        /// <para>Call ParseNamespaceBody method to parse its body.</para>
        /// </list>
        /// </summary>
        /// <returns>NAMESPACENODE instance representing the root namespace.</returns>
        //------------------------------------------------------------
        internal NAMESPACENODE ParseRootNamespace()
        {
            // Create top namespace with token index -1.
            NAMESPACENODE nsNode = AllocNode(NODEKIND.NAMESPACE, null, -1) as NAMESPACENODE;
            nsNode.NameNode = null;
            nsNode.GlobalAttributeNode = null;
            nsNode.Key = NameManager.GetPredefinedName(PREDEFNAME.EMPTY);
            nsNode.OpenCurlyIndex = nsNode.CloseCurlyIndex = -1;
            nsNode.SourceFileInfo = this.sourceFileInfo;
#if DEBUG
            nsNode.DebugComment = "Root Namespace";
#endif

            // ...and the contents of the source is just a namespace body
            ParseNamespaceBody(nsNode);

            // We're done!  Thus, we should be at end-of-file.
            nsNode.CloseCurlyIndex = CurrentTokenIndex();
            return nsNode;
        }

        //------------------------------------------------------------
        // CParser.ParseNamespaceBody
        //
        // namespace-declaration:
        //     "namespace"  qualified-identifier  namespace-body  [;]
        //
        // qualified-identifier:
        //     identifier
        //     qualified-identifier  "."  identifier
        //
        // namespace-body:
        //     "{"  [extern-alias-directives]  [using-directives]  [namespace-member-declarations]  "}"
        //
        // extern-alias-directives:
        //     extern-alias-directive
        //     extern-alias-directives  extern-alias-directive
        //
        // extern-alias-directive:
        //     "extern"  "alias"  identifier  ";"
        //
        // using-directives:
        //     using-directive
        //     using-directives  using-directive
        //
        // using-directive:
        //     using-alias-directive
        //     using-namespace-directive
        //
        // using-alias-directive:
        //     "using"  identifier  "="  namespace-or-type-name  ";"
        //
        // using-namespace-directive:
        //     "using"  namespace-name  ";"
        //
        // namespace-member-declarations:
        //     namespace-member-declaration
        //     namespace-member-declarations  namespace-member-declaration
        //
        // namespace-member-declaration:
        //     namespace-declaration
        //     type-declaration
        //
        // type-declaration:
        //     class-declaration
        //     struct-declaration
        //     interface-declaration
        //     enum-declaration
        //     delegate-declaration
        //
        // qualified-alias-member:
        //     identifier  "::"  identifier  [type-argument-list]
        //
        /// <summary>
        /// Parse a namespace body.
        /// </summary>
        /// <param name="nsNode">NAMESPACENODE instance to be parsed.</param>
        //------------------------------------------------------------
        internal void ParseNamespaceBody(NAMESPACENODE nsNode)
        {
            // A namespace body consists of (in order):
            // * zero or more externs
            // * zero or more using clauses
            // * zero or more namespace element declarations

            CListMaker usingList = new CListMaker(this);

            //--------------------------------------------------
            // Parse extern-alias-directives and using-directives and create nodes.
            // These nodes are linked to the node list starting at nsNode.UsingNode.
            //--------------------------------------------------
            for (bool seenUsing = false; ; )
            {
                switch (CurrentTokenID())
                {
                    case TOKENID.EXTERN:
                        if (seenUsing)
                        {
                            // Externs must come before usings.
                            Error(CSCERRID.ERR_ExternAfterElements);
                        }
                        usingList.Add(ParseExternAliasClause(nsNode), -1);
                        continue;

                    case TOKENID.USING:
                        seenUsing = true;
                        usingList.Add(ParseUsingClause(nsNode), -1);
                        continue;

                    default:
                        break;
                }
                break;
            }
            nsNode.UsingNode = usingList.GetList(nsNode);

            //--------------------------------------------------
            // The global (file level) namespace can also contain 'global' attributes
            //--------------------------------------------------
            if (nsNode.ParentNode == null)
            {
                nsNode.GlobalAttributeNode = ParseGlobalAttributes(nsNode);
            }

            //--------------------------------------------------
            // Parse namespace-member-declarations.
            //--------------------------------------------------
            CListMaker elementList = new CListMaker(this);
            for (; ; )
            {
                TOKENID tokenId = CurrentTokenID();

                // F_NSELEMENT flag is for
                // "abstract", "class", "delegate", "enum", "interface", "internal", "new",
                // "private", "protected", "public", "sealed", "static", "struct", "unsafe", "["

                if ((TokenInfoArray[(int)tokenId].Flags & TOKFLAGS.F_NSELEMENT) != 0)
                {
                    // This token can start a type declaration
                    elementList.Add(ParseTypeDeclaration(nsNode, null), -1);
                    continue;
                }

                switch (tokenId)
                {
                    case TOKENID.NAMESPACE:
                        // A nested namespace
                        elementList.Add(ParseNamespace(nsNode), -1);
                        break;

                    case TOKENID.CLOSECURLY:
                    case TOKENID.ENDFILE:
                        // This token marks the end of a namespace body
                        goto END_PROCESSING;

                    case TOKENID.USING:
                        Error(CSCERRID.ERR_UsingAfterElements);
                        usingList.Add(ParseUsingClause(nsNode), -1);
                        break;

                    case TOKENID.EXTERN:
                        Error(CSCERRID.ERR_ExternAfterElements);
                        usingList.Add(ParseExternAliasClause(nsNode), -1);
                        break;

                    case TOKENID.IDENTIFIER:
                        if (CheckForSpecName(SPECIALNAME.PARTIAL))
                        {
                            if ((TokenInfoArray[(int)PeekToken()].Flags & TOKFLAGS.F_NSELEMENT) != 0)
                            {
                                // partial can start a type declaration
                                elementList.Add(ParseTypeDeclaration(nsNode, null), -1);
                                continue;
                            }
                            if (PeekToken() == TOKENID.NAMESPACE)
                            {
                                Error(CSCERRID.ERR_BadModifiersOnNamespace);
                                NextToken();
                                continue;
                            }
                        }
                        // Fall through.
                        goto default;

                    default:
                        // Whoops, the code is unrecognizable.  Give an error and try to get
                        // sync'd up with intended reality
                        Error(CSCERRID.ERR_NamespaceUnexpected);
                        if (!SkipToNamespaceElement())
                        {
                            goto END_PROCESSING;
                        }
                        break;
                }
            }

        END_PROCESSING:
            nsNode.ElementsNode = elementList.GetList(nsNode);
        }

        //------------------------------------------------------------
        // CParser.ParseUsingClause
        //
        // using-directive:
        //     using-alias-directive
        //     using-namespace-directive
        //
        // using-alias-directive:
        //     "using" identifier "=" namespace-or-type-name ";"
        //
        // using-namespace-directive:
        //     "using" namespace-name ";"
        //
        /// <summary>
        /// Parse using-directive.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseUsingClause(BASENODE parentNode)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.USING);

            // Create the using node, checking for namespace keyword
            USINGNODE usingNode = AllocNode(NODEKIND.USING, parentNode) as USINGNODE;

            NextToken();

            if (CurrentTokenID() == TOKENID.IDENTIFIER && PeekToken(1) == TOKENID.EQUAL)
            {
                // "using" identifier "=" namespace-or-type-name ";"

                // Warn on "using global = X".
                if (CheckForSpecName(SPECIALNAME.GLOBAL))
                {
                    Error(CSCERRID.WRN_GlobalAliasDefn);
                }

                // This is a using-alias directive.
                // Create a NAMENODE instance representing alias.
                usingNode.AliasNode = ParseIdentifier(usingNode);
                Eat(TOKENID.EQUAL);
                usingNode.NameNode = ParseGenericQualifiedNameList(usingNode, false);
            }
            else
            {
                // This is a using-namespace directive.
                //   "using" namespace-name ";"
                usingNode.AliasNode = null;

                // This name is of a namespace, which has no type parameters,
                // so call ParseDottedName , not ParseGenericQualifiedNameList.
                usingNode.NameNode = ParseDottedName(usingNode, true);
            }

            Eat(TOKENID.SEMICOLON);
            return usingNode;
        }

        //------------------------------------------------------------
        // CParser.ParseExternAliasClause
        //
        // extern-alias-directive:
        //     "extern" "alias" identifier ;
        //
        /// <summary>extern-alias-directive を解析する。</summary>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>USINGNODE instance representing extern sentence.</returns>
        //------------------------------------------------------------
        internal BASENODE ParseExternAliasClause(BASENODE parentNode)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.EXTERN);

            // Create the using node, checking for namespace keyword
            USINGNODE usingNode = AllocNode(NODEKIND.USING, parentNode) as USINGNODE;

            NextToken();

            // The current token must be "alias".
            if (!CheckForSpecName(SPECIALNAME.ALIAS))
            {
                Error(CSCERRID.ERR_SyntaxError, new ErrArg(SpecName(SPECIALNAME.ALIAS)));
            }
            else
            {
                ReportFeatureUse("CSCSTR_FeatureExternAlias");
                NextToken();
            }

            // Error on "extern alias global".
            if (CheckForSpecName(SPECIALNAME.GLOBAL))
                Error(CSCERRID.ERR_GlobalExternAlias);

            usingNode.AliasNode = ParseIdentifier(usingNode);
            usingNode.NameNode = null;

            Eat(TOKENID.SEMICOLON);
            return usingNode;
        }

        //------------------------------------------------------------
        // CParser.ParseGlobalAttributes
        //
        // global-attributes:
        //     global-attribute-sections
        //
        // global-attribute-sections:
        //     global-attribute-section
        //     global-attribute-sections  global-attribute-section
        //
        // global-attribute-section:
        //     "["  global-attribute-target-specifier  attribute-list  "]"
        //     "["  global-attribute-target-specifier  attribute-list  ","  "]"
        //
        // global-attribute-target-specifier:
        //     global-attribute-target  ":"
        //
        // global-attribute-target:
        //     "assembly"
        //     "module"
        //
        // attribute-list:
        //     attribute
        //     attribute-list  ","  attribute
        //
        // attribute:
        //     attribute-name  [attribute-arguments]
        //
        // attribute-name:
        //     type-name
        //
        // attribute-arguments:
        //     "("  [positional-argument-list]  ")"
        //     "("  positional-argument-list  ","  named-argument-list  ")"
        //     "("  named-argument-list  ")"
        //
        // positional-argument-list:
        //     positional-argument
        //     positional-argument-list  ","  positional-argument
        //
        // positional-argument:
        //     [argument-name]  attribute-argument-expression
        //
        // named-argument-list:
        //     named-argument
        //     named-argument-list  ","  named-argument
        //
        // named-argument:
        //     identifier  "="  attribute-argument-expression
        //
        // attribute-argument-expression:
        //     expression
        //
        // In the case of global-attribute, target must be "assembly" or "module".
        //
        /// <summary>Parse the attributes of the root namespace.</summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseGlobalAttributes(BASENODE parentNode)
        {
            // Global attributes must be  "[" global-attribute-target ":" .

            if (CurrentTokenID() != TOKENID.OPENSQUARE)
            {
                return null;
            }
            int index = PeekTokenIndex(1);
            if (TokenAt(index) > TOKENID.IDENTIFIER || PeekToken(2) != TOKENID.COLON)
            {
                return null;
            }

            //BASENODE attrNode = AllocNode(NODEKIND.ATTR, parentNode);
            CListMaker list = new CListMaker(this);
            bool attributeHadErrorThrowaway;

            while (
                TokenAt(index) != TOKENID.RETURN &&
                (TokenAt(index) != TOKENID.IDENTIFIER ||
                tokenArray[index].Name != NameManager.GetPredefinedName(PREDEFNAME.TYPE)))
            {
                // if neither "assembly" nor "module", show an error messages.
                list.Add(ParseAttributeSection(
                    parentNode,
                    ATTRTARGET.ASSEMBLY,
                    (ATTRTARGET.ASSEMBLY | ATTRTARGET.MODULE),
                    out attributeHadErrorThrowaway),
                    -1);

                if (CurrentTokenID() != TOKENID.OPENSQUARE ||
                    TokenAt(index = PeekTokenIndex(1)) > TOKENID.IDENTIFIER ||
                    PeekToken(2) != TOKENID.COLON)
                {
                    break;
                }
            }
            //attrNode = list.List;
            //return attrNode;
            return list.GetList(parentNode);
        }

        //------------------------------------------------------------
        // CParser.ParseNamespace
        //
        // namespace-declaration:
        // 	"namespace"  qualified-identifier  namespace-body  [;]
        //
        // qualified-identifier:
        // 	identifier
        // 	qualified-identifier  "."  identifier
        //
        // namespace-body:
        // 	"{"  [extern-alias-directives]  [using-directives]  [namespace-member-declarations]  "}"
        //
        /// <summary>Parse namespace &lt;name&gt; {...} and create an NAMESPACENODE instance.</summary>
        /// <param name="parentNode"></param>
        /// <returns>a NAMESPACENODE instance.</returns>
        //------------------------------------------------------------
        internal BASENODE ParseNamespace(BASENODE parentNode)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.NAMESPACE);

            // Create the namespace node
            NAMESPACENODE nsNode = AllocNode(NODEKIND.NAMESPACE, parentNode) as NAMESPACENODE;
            nsNode.GlobalAttributeNode = null;
            nsNode.Key = null;
            nsNode.SourceFileInfo = null;

            NextToken();

            // Grab the name
            nsNode.NameNode = ParseDottedName(nsNode, false);

            // Now there should be an open curly
            nsNode.OpenCurlyIndex = CurrentTokenIndex();
            Eat(TOKENID.OPENCURLY);

            // Followed by a namespace body
            ParseNamespaceBody(nsNode);

            // And lastly a close curly
            nsNode.CloseCurlyIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSECURLY);

            // optional trailing semi-colon
            if (CurrentTokenID() == TOKENID.SEMICOLON)
            {
                NextToken();
            }

            // CParser.AddToNodeTable is abstract.
            AddToNodeTable(nsNode);
            return nsNode;
        }

        //------------------------------------------------------------
        // CParser.ParseTypeDeclaration
        //
        // type-declaration:
        //     class-declaration
        //     struct-declaration
        //     interface-declaration
        //     enum-declaration
        //     delegate-declaration
        //
        // class-declaration:
        //     [attributes]  [class-modifiers]  ["partial"]  "class"  identifier  [type-parameter-list]
        //         [class-base]  [type-parameter-constraints-clauses]  class-body  [;]
        //
        // struct-declaration:
        //     [attributes]  [struct-modifiers]  ["partial"]  "struct"  identifier  [type-parameter-list]
        //         [struct-interfaces]  [type-parameter-constraints-clauses]  struct-body  [";"]
        //
        // interface-declaration:
        //     [attributes]  [interface-modifiers]  ["partial"]  "interface"
        //         identifier  [variant-type-parameter-list]  [interface-base]
        //         [type-parameter-constraints-clauses]  interface-body  [";"]
        //
        // enum-declaration:
        //     [attributes]  [enum-modifiers]  "enum"  identifier  [enum-base]  enum-body  [";"]
        //
        // delegate-declaration:
        //     [attributes]  [delegate-modifiers]  "delegate"  return-type
        //         identifier  [variant-type-parameter-list]
        //         "("  [formal-parameter-list]  ")"  [type-parameter-constraints-clauses]  ";"
        //
        /// <summary>
        /// Parse type declarations and create instances of AGGREGATENODE or ENUMNODE or DELEGATENODE.
        /// </summary>
        /// <param name="attrNode"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseTypeDeclaration(BASENODE parentNode, BASENODE attrNode)
        {
            int tokenIndex = CurrentTokenIndex();

            // All types can start with modifiers, so parse them now,
            // IF we weren't given any to start with
            // (it's possible the caller already parsed them for us).
            // ParseAttributes will return null if there are none there...

            bool attributeHadError = false;
            if (attrNode == null)
            {
                attrNode = ParseAttributes(parentNode, ATTRTARGET.TYPE, (ATTRTARGET)0, out attributeHadError);
            }
            else
            {
                bool bTmp;
                tokenIndex = GetFirstToken(attrNode, ExtentFlags.FULL, out bTmp);
            }

            // This is the first non-attribute token index
            int declStartIndex = CurrentTokenIndex();

            //--------------------------------------------------
            // We got an attribute, but the attribute had an error in it.
            // This means we may have consumed too many tokens (like the token "class" in "class Foo")
            // Back up a little to see if we can recover
            //--------------------------------------------------
            if (attrNode != null && attributeHadError)
            {
                // The attribute had an error.  This means that we may have consumed
                // too many tokens.  Look back a few to see if we can find one that
                // is valid (like TOKFLAGS.F_NS_ELEMENT) and the start parsing from there
                // Note: 2 is arbitrary.  Feel free to replace with a better value

                for (int i = 0; i < 2 && CurrentTokenIndex() > tokenIndex; i++)
                {
                    if ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_NSELEMENT) != 0)
                    {
                        //ok!  found one!
                        declStartIndex = CurrentTokenIndex();
                        break;
                    }
                    PrevToken();
                }

                //once we've found a NS_ELEMENT token keep on eating
                //ns element tokens before us

                while (
                    (CurrentTokenIndex() > tokenIndex) &&
                    ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_NSELEMENT)) != 0)
                {
                    declStartIndex = CurrentTokenIndex();
                    PrevToken();
                    if (!(CurrentTokenIndex() > tokenIndex &&
                        (TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_NSELEMENT) != 0))
                    {
                        //We consumed one too many tokens
                        NextToken();
                        break;
                    }
                }
            }   // end of error recovery

            //--------------------------------------------------
            // Type declarations can begin with modifiers
            // 'public', 'private', 'sealed', and 'abstract'.
            // Not all combinations are legal, but we parse them all into a bitfield anyway.
            //
            // "partial" is processed in ParseModifiers method.
            //--------------------------------------------------
            NODEFLAGS modifiers = ParseModifiers(parentNode.Kind == NODEKIND.NESTEDTYPE ? false : true);

            BASENODE typedefNode = null;

            if ((modifiers & NODEFLAGS.MOD_STATIC) != 0)
            {
                ReportFeatureUse("CSCSTR_FeatureStaticClasses");
            }

            //--------------------------------------------------
            // The next token should be the one indicating what type to parse
            // (class, enum, struct, interface, or delegate)
            //--------------------------------------------------
            switch (CurrentTokenID())
            {
                case TOKENID.CLASS:
                case TOKENID.STRUCT:
                case TOKENID.INTERFACE:
                    attrNode = SetDefaultAttributeTarget(attrNode, ATTRTARGET.TYPE, ATTRTARGET.TYPE);
                    typedefNode = ParseAggregate(parentNode, tokenIndex, declStartIndex, attrNode, modifiers);
                    break;

                case TOKENID.DELEGATE:
                    attrNode = SetDefaultAttributeTarget(attrNode, ATTRTARGET.TYPE, (ATTRTARGET)(ATTRTARGET.TYPE | ATTRTARGET.RETURN));
                    typedefNode = ParseDelegate(parentNode, tokenIndex, declStartIndex, attrNode, modifiers);
                    break;

                case TOKENID.ENUM:
                    attrNode = SetDefaultAttributeTarget(attrNode, ATTRTARGET.TYPE, ATTRTARGET.TYPE);
                    typedefNode = ParseEnum(parentNode, tokenIndex, declStartIndex, attrNode, modifiers);
                    break;

                case TOKENID.NAMESPACE:
                    if ((modifiers != 0 || attrNode != null) && parentNode.Kind == NODEKIND.NAMESPACE)
                    {
                        ErrorAtToken(declStartIndex, CSCERRID.ERR_BadModifiersOnNamespace);
                        typedefNode = ParseNamespace(parentNode);
                        // We store the attributes, even though this is an error case,
                        // so that the IDE can correctly show completion lists.
                        (typedefNode as NAMESPACENODE).GlobalAttributeNode = attrNode;
                        break;
                    }
                    // Fall-through
                    goto default;

                default:
                    Error(CSCERRID.ERR_BadTokenInType);
                    break;
            }

            // NOTE:  If we didn't parse anything, we return null.  These are added to
            // lists using the CListMaker object, which deals with a new node of null and
            // doesn't mess with the current list.
            //
            //cyrusn 4/1/03
            //If we didn't parse a type we could have still parsed an attribute
            //so we create a dummy type whose only interesting component is the attribute
            //this way we can still do intellisense on attributes in a namespace not
            //attached to a type

            if (null == typedefNode && null != attrNode)
            {
                return ParseMissingType(parentNode, tokenIndex, declStartIndex, modifiers, attrNode);
            }
            else
            {
                return typedefNode;
            }
        }

        //------------------------------------------------------------
        // CParser.ParseAggregate
        //
        // class-declaration:
        //     [attributes]  [class-modifiers]  ["partial"]  "class"  identifier  [type-parameter-list]
        //         [class-base]  [type-parameter-constraints-clauses]  class-body  [;]
        //
        // struct-declaration:
        //     [attributes]  [struct-modifiers]  ["partial"]  "struct"  identifier  [type-parameter-list]
        //         [struct-interfaces]  [type-parameter-constraints-clauses]  struct-body  [";"]
        //
        // interface-declaration:
        //     [attributes]  [interface-modifiers]  ["partial"]  "interface"
        //         identifier  [variant-type-parameter-list]  [interface-base]
        //         [type-parameter-constraints-clauses]  interface-body  [";"]
        //
        /// <summary>
        /// Parse and Create an AGGREGATENODE instance.
        /// AGGREAGETNODE represents class, struct and interface.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="tokenIndex">The index of the first token of the type definition (Including attributes).</param>
        /// <param name="startIndex">The index of the first token after attributes.</param>
        /// <param name="attrNode"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseAggregate(
            BASENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers)
        {
            TOKENID tokenId = CurrentTokenID();
            NODEKIND kind;
            DebugUtil.Assert(
                tokenId == TOKENID.CLASS ||
                tokenId == TOKENID.STRUCT ||
                tokenId == TOKENID.INTERFACE);

            switch (tokenId)
            {
                default:
                    DebugUtil.Assert(false, "Bad token kind");
                    goto case TOKENID.CLASS;

                case TOKENID.CLASS:
                    kind = NODEKIND.CLASS;
                    break;

                case TOKENID.INTERFACE:
                    kind = NODEKIND.INTERFACE;
                    break;

                case TOKENID.STRUCT:
                    kind = NODEKIND.STRUCT;
                    break;
            }

            // Set the index of token "class" or "struct" or "interface" to the index of node
            AGGREGATENODE aggregateNode = AllocNode(kind, parentNode, tokenIndex).AsAGGREGATE;

            aggregateNode.StartTokenIndex = startIndex;
            aggregateNode.Flags = modifiers;
            aggregateNode.AttributesNode = attrNode;
            aggregateNode.Key = null;
            if (attrNode != null)
            {
                attrNode.ParentNode = aggregateNode;
            }
            NextToken();

            // Get type name from token.
            aggregateNode.NameNode = ParseIdentifier(aggregateNode);
            int closeIndex = -1;

            // Parse type parameters.
            aggregateNode.TypeParametersNode = ParseInstantiation(aggregateNode, true, ref closeIndex);

            // Parse the base class and interfaces.
            aggregateNode.BasesNode = ParseBaseTypesClause(aggregateNode);

            // Parse constraints.
            aggregateNode.ConstraintsNode
                = ParseConstraintClause(aggregateNode, aggregateNode.TypeParametersNode != null);
            aggregateNode.MembersNode = null;

            // Parse class body
            aggregateNode.OpenCurlyIndex = CurrentTokenIndex();

            bool parseMembers = true;
            if (!Eat(TOKENID.OPENCURLY))
            {
                int curIdx = CurrentTokenIndex();

                //ok, interesting case.  The user might have something like:
                //class C :    //<-- they're typing here
                //class B {}
                //In this case it's most likely that B is a sibling of C and not a child.
                //So if we don't see an { and we're followed by the beginning of a 
                //non-member declaration then we bail out and don't treat it as a nested
                //type.
                //
                //Note: this won't handle the case where the type below has an attribute
                //on it, but maybe we can revisit that later

                ParseModifiers(false);  // fReportErrors
                switch (CurrentTokenID())
                {
                    case TOKENID.CLASS:
                    case TOKENID.INTERFACE:
                    case TOKENID.STRUCT:
                    case TOKENID.NAMESPACE:
                        parseMembers = false;
                        break;

                    default:
                        break;
                }
                Rewind(curIdx);
            }

            //even if we saw a { or think we shoudl parse members bail out early since 
            //we know namespaces can't be nested inside types

            if (parseMembers)
            {
                bool foundEnd = false;
                MEMBERNODE lastMemberNode = null;

                // ignore members if missing type name and no open curly
                if (aggregateNode.NameNode.Name == SpecName(SPECIALNAME.MISSING) &&
                    TokenAt(aggregateNode.OpenCurlyIndex) != TOKENID.OPENCURLY)
                {
                    foundEnd = true;
                }

                while (!foundEnd)
                {
                    TOKENID tokId = CurrentTokenID();

                    if ((TokenInfoArray[(int)tokId].Flags & TOKFLAGS.F_MEMBER) != 0)
                    {
                        int tokIndex = CurrentTokenIndex();

                        // This token can start a member -- go parse it
                        MEMBERNODE node = ParseMember(aggregateNode);
                        //ASSERT ((unsigned)(*ppNext).startIndex != 0xcccccccc);
                        //ASSERT ((unsigned)(*ppNext).iClose != 0xcccccccc);

                        // MEMBERNODE は NextMemberNode でノード列を作る。
                        if (lastMemberNode == null)
                        {
                            aggregateNode.MembersNode = node;
                            lastMemberNode = node;
                        }
                        else
                        {
                            lastMemberNode.NextMemberNode = node;
                            lastMemberNode = node;
                        }

                        // If we haven't advanced the token stream, skip
                        // this token (an error will have been given)                              
                        if (tokIndex == CurrentTokenIndex())
                        {
                            NextToken();
                        }
                    }
                    else if (tokId == TOKENID.CLOSECURLY || tokId == TOKENID.ENDFILE)
                    {
                        // This marks the end of members of this class
                        foundEnd = true;
                    }
                    else
                    {
                        // Error -- try to sync up with intended reality
                        ErrorInvalidMemberDecl(CurrentTokenIndex());
                        foundEnd = !SkipToMember();
                    }
                }
            }

            aggregateNode.CloseCurlyIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSECURLY);

            if (CurrentTokenID() == TOKENID.SEMICOLON)
            {
                NextToken();
            }

            AddToNodeTable(aggregateNode);
            return aggregateNode;
        }

        //------------------------------------------------------------
        // CParser.ParseDelegate
        //
        /// <summary>Parse and create a DELEGATE instance.</summary>
        /// <param name="parentNode"></param>
        /// <param name="tokenIndex">The index of the first token of the type definition (Including attributes).</param>
        /// <param name="startIndex">The index of the first token after attributes.</param>
        /// <param name="attrNode"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        //
        // delegate-declaration:
        //     [attributes] [delegate-modifiers] "delegate" return-type identifier [type-parameter-list]
        //         "(" [formal-parameter-list] ")" [type-parameter-constraints-clauses] ";"
        //------------------------------------------------------------
        internal BASENODE ParseDelegate(
            BASENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers)
        {
            //ASSERT(CurrentTokenID() == TOKENID.DELEGATE);
            if (CurrentTokenID() != TOKENID.DELEGATE)
            {
                return null;
            }

            DELEGATENODE delegateNode
                = AllocNode(NODEKIND.DELEGATE, parentNode, tokenIndex) as DELEGATENODE;

            delegateNode.StartTokenIndex = startIndex;
            delegateNode.Flags = modifiers;
            delegateNode.AttributesNode = attrNode;
            delegateNode.Key = null;
            if (attrNode != null)
            {
                attrNode.ParentNode = delegateNode;
            }

            NextToken();
            delegateNode.ReturnTypeNode = ParseReturnType(delegateNode);
            delegateNode.NameNode = ParseIdentifier(delegateNode);

            // Check for type parameters
            int closeIndex = -1;
            bool btemp = false;

            delegateNode.ParametersNode = ParseInstantiation(delegateNode, true, ref closeIndex);

            ParseParameterList(
                delegateNode,
                out delegateNode.ParametersNode,
                out delegateNode.OpenParenIndex,
                out delegateNode.CloseParenIndex,
                out btemp,
                (int)kppo.NoVarargs);

            // parse constraints
            delegateNode.ConstraintsNode
                = ParseConstraintClause(delegateNode, delegateNode.TypeParametersNode != null);

            delegateNode.SemiColonIndex = CurrentTokenIndex();
            Eat(TOKENID.SEMICOLON);

            AddToNodeTable(delegateNode);
            return delegateNode;
        }

        //------------------------------------------------------------
        // CParser.ParseEnum
        //
        // enum-declaration:
        //     [attributes]  [enum-modifiers]  "enum"  identifier  [enum-base]  enum-body  [";"]
        // enum-base:
        //     ":"  integral-type
        // enum-body:
        //     "{"  [enum-member-declarations]  "}"
        //     "{"  enum-member-declarations  ","  "}"
        //
        /// <summary>
        /// Parse and create an ENUMNODE instance.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="tokenIndex">The index of the first token of the type definition (Including attributes).</param>
        /// <param name="startIndex">The index of the first token after attributes.</param>
        /// <param name="attrNode"></param>
        /// <param name="modifiers"></param>
        //------------------------------------------------------------
        internal BASENODE ParseEnum(
            BASENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.ENUM);

            ENUMNODE enumNode = AllocNode(NODEKIND.ENUM, parentNode, tokenIndex) as ENUMNODE;

            enumNode.StartTokenIndex = startIndex;
            enumNode.Flags = modifiers;
            enumNode.AttributesNode = attrNode;
            enumNode.Key = null;
            enumNode.TypeParametersNode = null;
            enumNode.ConstraintsNode = null;
            if (attrNode != null)
            {
                attrNode.ParentNode = enumNode;
            }
            NextToken();

            //--------------------------------------------------
            // name
            //--------------------------------------------------
            enumNode.NameNode = ParseIdentifier(enumNode);

            if (CurrentTokenID() == TOKENID.OPENANGLE)
            {
                Error(CSCERRID.ERR_InvalidGenericEnum);
                int closeIndex = -1;
                ParseInstantiation(enumNode, true, ref closeIndex);
            }

            //--------------------------------------------------
            // base type
            //--------------------------------------------------
            if (CurrentTokenID() == TOKENID.COLON)
            {
                NextToken();

                TYPEBASENODE baseNode = ParseType(enumNode, false);

                if ((baseNode.Kind != NODEKIND.PREDEFINEDTYPE) ||
                    ((baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.BYTE &&
                    (baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.SHORT &&
                    (baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.INT &&
                    (baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.LONG &&
                    (baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.SBYTE &&
                    (baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.USHORT &&
                    (baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.UINT &&
                    (baseNode as  PREDEFINEDTYPENODE).Type != PREDEFTYPE.ULONG))
                {
                    ErrorAtToken(baseNode.TokenIndex, CSCERRID.ERR_IntegralTypeExpected);
                }

                enumNode.BasesNode = baseNode;
            }
            else
            {
                enumNode.BasesNode = null;
            }

            enumNode.OpenCurlyIndex = CurrentTokenIndex();

            //--------------------------------------------------
            // Members
            //--------------------------------------------------
            Eat(TOKENID.OPENCURLY);
            MEMBERNODE lastMemberNode = null;

            while (CurrentTokenID() != TOKENID.CLOSECURLY)
            {
                ENUMMBRNODE memberNode = AllocNode(NODEKIND.ENUMMBR, enumNode) as ENUMMBRNODE;

                memberNode.AttributesNode = ParseAttributes(memberNode, ATTRTARGET.FIELD, ATTRTARGET.FIELD);
                memberNode.StartTokenIndex = CurrentTokenIndex();
                memberNode.NameNode = ParseIdentifier(memberNode);
                if (CurrentTokenID() == TOKENID.EQUAL)
                {
                    NextToken();
                    if (CurrentTokenID() == TOKENID.COMMA || CurrentTokenID() == TOKENID.CLOSECURLY)
                    {
                        Error(CSCERRID.ERR_ConstantExpected);
                        memberNode.ValueNode = null;
                    }
                    else
                    {
                        memberNode.ValueNode = ParseExpression(memberNode, -1);
                    }
                }
                else
                {
                    memberNode.ValueNode = null;
                }

                memberNode.CloseIndex = CurrentTokenIndex();
                AddToNodeTable(memberNode);

                // Link this to the member list.
                if (lastMemberNode == null)
                {
                    enumNode.MembersNode = memberNode;
                    lastMemberNode = memberNode;
                }
                else
                {
                    lastMemberNode.NextNode = memberNode;
                    lastMemberNode = memberNode;
                }

                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    if (CurrentTokenID() == TOKENID.IDENTIFIER)
                    {
                        // See if they just forgot a comma.
                        switch (PeekToken())
                        {
                            case TOKENID.EQUAL:
                            case TOKENID.COMMA:
                            case TOKENID.CLOSECURLY:
                            case TOKENID.SEMICOLON:
                                CheckToken(TOKENID.COMMA);
                                continue;

                            default:
                                break;
                        }
                    }
                    else if (CurrentTokenID() == TOKENID.SEMICOLON)
                    {
                        CheckToken(TOKENID.COMMA);
                        NextToken();
                        continue;
                    }

                    memberNode.CloseIndex = PeekTokenIndexFrom(memberNode.CloseIndex, -1);
                    // Don't include the } as the close for the last member...
                    break;
                }
                NextToken();
            }

            enumNode.CloseCurlyIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSECURLY);

            if (CurrentTokenID() == TOKENID.SEMICOLON)
            {
                NextToken();
            }

            AddToNodeTable(enumNode);
            return enumNode;
        }

        //------------------------------------------------------------
        // CParser.ParseTypeDef
        //
        //------------------------------------------------------------
        //BASENODE *ParseTypeDef (BASENODE *pParent, int iTokIdx, BASENODE *pAttrs, unsigned iMods);

        //------------------------------------------------------------
        // CParser.ParseMember
        //
        // class-member-declarations:
        //     class-member-declaration
        //     class-member-declarations  class-member-declaration
        // class-member-declaration:
        //     constant-declaration
        //     field-declaration
        //     method-declaration
        //     property-declaration
        //     event-declaration
        //     indexer-declaration
        //     operator-declaration
        //     constructor-declaration
        //     destructor-declaration
        //     static-constructor-declaration
        //     type-declaration
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBERNODE ParseMember(AGGREGATENODE parentNode)
        {
            int tokenIndex = CurrentTokenIndex();
            NODEFLAGS modifiers;
            BASENODE attrNode;
            bool methodWithoutType = false;
            bool isEvent = false;
            bool isFixed = false;
            MemberNameEnum memberNameType;
            TOKENID tokenId;

            // Attributes are potentially at the beginning of everything here.  Parse 'em,
            // we'll get null back if there aren't any.

            attrNode = ParseAttributes(parentNode, (ATTRTARGET)0, (ATTRTARGET)0);

            int markedIndex = CurrentTokenIndex();
            int startIndex = markedIndex;

            // All things here can start with modifiers -- parse them into a bitfield.
            modifiers = ParseModifiers(true);

            //--------------------------------------------------
            // Check for [static] constructor form
            // if type name and '(', this member is a constructor.
            //--------------------------------------------------
            if (CurrentTokenID() == TOKENID.IDENTIFIER && PeekToken(1) == TOKENID.OPENPAREN)
            {
                if (tokenArray[CurrentTokenIndex()].Name == parentNode.NameNode.Name)
                {
                    return ParseConstructor(parentNode, tokenIndex, startIndex, attrNode, modifiers);
                }

                methodWithoutType = true;
            }

            //--------------------------------------------------
            // Check for destructor form
            //--------------------------------------------------
            if (CurrentTokenID() == TOKENID.TILDE)
            {
                return ParseDestructor(parentNode, tokenIndex, startIndex, attrNode, modifiers);
            }

            //--------------------------------------------------
            // Check for constant
            //--------------------------------------------------
            if (CurrentTokenID() == TOKENID.CONST)
            {
                return ParseConstant(parentNode, tokenIndex, startIndex, attrNode, modifiers);
            }

            //--------------------------------------------------
            // Check for event. Remember for later processing.
            //--------------------------------------------------
            if (CurrentTokenID() == TOKENID.EVENT)
            {
                Eat(TOKENID.EVENT);
                isEvent = true;
            }

            //--------------------------------------------------
            // It's valid to have a type declaration here -- check for those
            // F_TYPEDECL: CLASS DELEGATE ENUM INTERFACE STRUCT
            //--------------------------------------------------
            if ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_TYPEDECL) != 0 &&
                !isEvent)   // ? "event" is not in F_TYPEDECL group.
            {
                bool isMissing;

                CBasenodeLookupCache cache = new CBasenodeLookupCache();
                Rewind(markedIndex);
                NESTEDTYPENODE nestedTypeNode
                    = AllocNode(NODEKIND.NESTEDTYPE, parentNode, tokenIndex) as NESTEDTYPENODE;
                nestedTypeNode.AttributesNode = null;
                nestedTypeNode.TypeNode = ParseTypeDeclaration(nestedTypeNode, attrNode);
                nestedTypeNode.StartTokenIndex
                    = GetFirstToken(nestedTypeNode.TypeNode, ExtentFlags.FULL, out isMissing);
                nestedTypeNode.CloseIndex
                    = GetLastToken(cache, nestedTypeNode.TypeNode, ExtentFlags.FULL, out isMissing);
                return nestedTypeNode;
            }

            //--------------------------------------------------
            // Check for conversion operators (implicit/explicit)
            // Also allow operator here so we can give a good error message
            //--------------------------------------------------
            if ((CurrentTokenID() == TOKENID.IMPLICIT ||
                CurrentTokenID() == TOKENID.EXPLICIT ||
                CurrentTokenID() == TOKENID.OPERATOR)
                && !isEvent)
            {
                return ParseOperator(parentNode, tokenIndex, startIndex, attrNode, modifiers, null);
            }

            if (!isEvent && CurrentTokenID() == TOKENID.FIXED)
            {
                ReportFeatureUse("CSCSTR_FeatureFixedBuffer");
                Eat(TOKENID.FIXED);
                isFixed = true;
            }

            //--------------------------------------------------
            // Everything that's left --
            //     methods, fields, properties, and non-conversion operators
            // -- starts with a type (possibly void).  Parse one.
            // Unless we have a method without a return type (identifier followed by a '(')
            //--------------------------------------------------

            TYPEBASENODE TypeNode = null;
            if (methodWithoutType)
            {
                // If found a method without return type, report the error,
                // assume that return type is void and continue parsing.
                Error(CSCERRID.ERR_MemberNeedsType);
                TypeNode = AllocNode(NODEKIND.PREDEFINEDTYPE, null) as PREDEFINEDTYPENODE;
                (TypeNode as PREDEFINEDTYPENODE).Type = PREDEFTYPE.VOID;
            }
            else if (CurrentTokenID() == TOKENID.ENDFILE)
            {
                Eat(TOKENID.CLOSECURLY);
                TypeNode = null;
            }
            else
            {
                TypeNode = ParseReturnType(null);
            }


            if (isFixed)
            {
                goto PARSE_FIELD;
            }

            //--------------------------------------------------
            // Check here for operators
            // Allow old-style implicit/explicit casting operator syntax,
            // just so we can give a better error
            //--------------------------------------------------
            if ((CurrentTokenID() == TOKENID.OPERATOR ||
                CurrentTokenID() == TOKENID.IMPLICIT ||
                CurrentTokenID() == TOKENID.EXPLICIT) && !isEvent)
            {
                return ParseOperator(parentNode, tokenIndex, startIndex, attrNode, modifiers, TypeNode);
            }

            markedIndex = CurrentTokenIndex();
            memberNameType = ScanMemberName();
            tokenId = CurrentTokenID();
            Rewind(markedIndex);

            switch (memberNameType)
            {
                case MemberNameEnum.IndexerName:
                    if (!isEvent)
                    {
                        return ParseProperty(
                            NODEKIND.INDEXER,
                            parentNode,
                            tokenIndex,
                            startIndex,
                            attrNode,
                            modifiers,
                            TypeNode,
                            false);
                    }
                    break;

                case MemberNameEnum.GenericMethodName:
                    if (!isEvent)
                    {
                        return ParseMethod(parentNode, tokenIndex, startIndex, attrNode, modifiers, TypeNode);
                    }
                    break;

                case MemberNameEnum.NotMemberNameWithDot:
                    // Check for the situation where an incomplete explicit member is being entered.
                    // In this case, the code may look something like "void ISomeInterface." followed
                    // by either the end of the type of some other member (valid or not).  For statement
                    // completion purposes, we need the name to be a potentially dotted name, so we
                    // can't default into a field declaration (since it's name field is a NAMENODE).

                    // Okay, whichever of these we call will fail -- however, they can handle
                    // dotted names.
                    if (isEvent)
                    {
                        return ParseProperty(
                            NODEKIND.PROPERTY,
                            parentNode,
                            tokenIndex,
                            startIndex,
                            attrNode,
                            modifiers,
                            TypeNode,
                            isEvent);
                    }
                    else
                    {
                        return ParseMethod(
                            parentNode,
                            tokenIndex,
                            startIndex,
                            attrNode,
                            modifiers,
                            TypeNode);
                    }

                case MemberNameEnum.SimpleName:
                case MemberNameEnum.PropertyOrEventOrMethodName:
                    if (tokenId == TOKENID.OPENPAREN && !isEvent)
                    {
                        return ParseMethod(parentNode, tokenIndex, startIndex, attrNode, modifiers, TypeNode);
                    }
                    if (tokenId == TOKENID.OPENCURLY)
                    {
                        return ParseProperty(
                            NODEKIND.PROPERTY,
                            parentNode,
                            tokenIndex,
                            startIndex,
                            attrNode,
                            modifiers,
                            TypeNode,
                            isEvent);
                    }
                    break;

                case MemberNameEnum.NotMemberName:
                default:
                    // We don't know what it is yet, and it's incomplete.  Create a partial member
                    // node to contain the type we parsed and return that.
                    ErrorInvalidMemberDecl(CurrentTokenIndex());

                    //If the user explicitly typed "event" then at least try to parse out an event.
                    //It helps out the language service a lot
                    if (isEvent && TypeNode != null)
                    {
                        break;
                    }

                    PARTIALMEMBERNODE pMbr = AllocNode(NODEKIND.PARTIALMEMBER, parentNode, tokenIndex) as PARTIALMEMBERNODE;
                    pMbr.StartTokenIndex = startIndex;
                    pMbr.Node = TypeNode;
                    if (TypeNode != null)
                    {
                        TypeNode.ParentNode = pMbr;
                    }
                    pMbr.AttributesNode = attrNode;
                    if (attrNode != null)
                    {
                        attrNode.ParentNode = pMbr;
                    }
                    pMbr.CloseIndex = CurrentTokenIndex();
                    return pMbr;

            }

        PARSE_FIELD:

            return ParseField(parentNode, tokenIndex, startIndex, attrNode, modifiers, TypeNode, isEvent, isFixed);
        }

        //------------------------------------------------------------
        // CParser.ParseConstructor
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="startIndex"></param>
        /// <param name="attrNode"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBERNODE ParseConstructor(
            AGGREGATENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers)
        {
            CTORMETHODNODE ctorNode = AllocNode(NODEKIND.CTOR, parentNode, tokenIndex) as CTORMETHODNODE;

            ctorNode.StartTokenIndex = startIndex;
            ctorNode.Flags = modifiers;
            ctorNode.AttributesNode
                = SetDefaultAttributeTarget(attrNode, ATTRTARGET.METHOD, ATTRTARGET.METHOD);
            if (attrNode != null)
            {
                attrNode.ParentNode = ctorNode;
            }
            ctorNode.InteriorNode = null;

            string tokenName = tokenArray[CurrentTokenIndex()].Name;

            // Current token is an identifier.  Check to see that the name matches that of the class
            DebugUtil.Assert(tokenName != null);
            DebugUtil.Assert(tokenName == parentNode.NameNode.Name);

            ctorNode.ReturnTypeNode = null;
            NextToken();

            // Get parameters
            bool btemp = false;

            ParseParameterList(
                ctorNode,
                out ctorNode.ParametersNode,
                out ctorNode.OpenParenIndex,
                out ctorNode.CloseParenIndex,
                out btemp,
                (int)kppo.AllowAll);

            // Check for :base(args) or :this(args)
            ctorNode.ThisOrBaseCallNode = null;
            if (CurrentTokenID() == TOKENID.COLON)
            {
                TOKENID iTok = NextToken();

                if (iTok == TOKENID.BASE || iTok == TOKENID.THIS)
                {
                    ctorNode.NodeFlagsEx |=
                        (iTok == TOKENID.BASE ? NODEFLAGS.EX_CTOR_BASE : NODEFLAGS.EX_CTOR_THIS);

                    ctorNode.ThisOrBaseCallNode =
                        AllocNode(NODEKIND.CALL, ctorNode, PeekTokenIndex()) as CALLNODE;
                    ctorNode.ThisOrBaseCallNode.NodeFlagsEx |=
                        (iTok == TOKENID.BASE ? NODEFLAGS.EX_CTOR_BASE : NODEFLAGS.EX_CTOR_THIS);

                    {
                        //parse the "this/base" name into p1
                        string callName = (iTok == TOKENID.BASE ?
                            NameManager.KeywordName(TOKENID.BASE) :
                            NameManager.KeywordName(TOKENID.THIS));
                        NAMENODE nameNode = AllocNameNode(callName, this.CurrentTokenIndex());
                        nameNode.ParentNode = ctorNode.ThisOrBaseCallNode;
                        ctorNode.ThisOrBaseCallNode.Operand1 = nameNode;
                    }

                    NextToken();

                    //now try to parse the arguments
                    long iErrorCount = parseErrorCount;
                    if (CheckToken(TOKENID.OPENPAREN))
                    {
                        ctorNode.ThisOrBaseCallNode.Operand2
                            = ParseArgumentList(ctorNode.ThisOrBaseCallNode);
                        int closeIndex = PeekTokenIndex(-1);
                        // NOTE:  -1 because ParseArgumentList advanced past it...
                        if (tokenArray[closeIndex].TokenID != TOKENID.CLOSEPAREN)
                        {
                            closeIndex = PeekTokenIndexFrom(closeIndex, 1);
                        }
                        ctorNode.ThisOrBaseCallNode.CloseParenIndex = closeIndex;
                    }
                    else
                    {
                        //we didn't read in a parenthesis.  So our arugments will be null               
                        ctorNode.ThisOrBaseCallNode.Operand2 = null;
                    }

                    if (iErrorCount != parseErrorCount)
                    {
                        ctorNode.ThisOrBaseCallNode.Flags |= NODEFLAGS.CALL_HADERROR;
                    }
                    // Parameter tips key on this for window placement...
                }
                else
                {
                    // Static constructor can't have this or base
                    if ((modifiers & NODEFLAGS.MOD_STATIC) != 0)
                    {
                        Error(
                            CSCERRID.ERR_StaticConstructorWithExplicitConstructorCall,
                            new ErrArg(tokenName));
                    }
                    else
                    {
                        Error(CSCERRID.ERR_ThisOrBaseExpected);
                    }
                }
            }

            // Next should be the block.  Remember where it started for the interior parse.
            ctorNode.OpenIndex = CurrentTokenIndex();
            ctorNode.BodyNode = null;
            if (CurrentTokenID() == TOKENID.OPENCURLY ||
                (ctorNode.NodeFlagsEx & (NODEFLAGS.EX_CTOR_BASE | NODEFLAGS.EX_CTOR_THIS)) != 0)
            {
                SkipBlock(SKIPFLAGS.NORMALSKIP, out ctorNode.CloseIndex);
                if (CurrentTokenID() == TOKENID.SEMICOLON)
                {
                    ctorNode.CloseIndex = CurrentTokenIndex();
                    NextToken();
                }
            }
            else
            {
                ctorNode.CloseIndex = CurrentTokenIndex();
                Eat(TOKENID.SEMICOLON);
                ctorNode.NodeFlagsEx |= NODEFLAGS.EX_METHOD_NOBODY;
            }

            AddToNodeTable(ctorNode);
            return ctorNode;
        }

        //------------------------------------------------------------
        // CParser.ParseDestructor
        //
        /// <summary>トークン列からデストラクタの定義を取得する。</summary>
        /// <param name="attrNode">デストラクタに指定された属性のノード。</param>
        /// <param name="modifiers">修飾子のビットフラグ。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <param name="startIndex">デストラクタに設定する開始インデックス。</param>
        /// <returns>デストラクタを表す MEMBERNODE を返す。</returns>
        /// <remarks>メソッド本体は解析しない。</remarks>
        //------------------------------------------------------------
        internal MEMBERNODE ParseDestructor(
            AGGREGATENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers)
        {
            METHODBASENODE dtorNode = AllocNode(NODEKIND.DTOR, parentNode, tokenIndex).AsDTOR;

            dtorNode.StartTokenIndex = startIndex;
            dtorNode.Flags = modifiers;
            dtorNode.AttributesNode
                = SetDefaultAttributeTarget(attrNode, ATTRTARGET.METHOD, ATTRTARGET.METHOD);
            dtorNode.ReturnTypeNode = null;
            dtorNode.ParametersNode = null;
            if (attrNode != null)
            {
                attrNode.ParentNode = dtorNode;
            }
            dtorNode.InteriorNode = null;

            // Current token is an identifier.  Rescan it, and check to see that the name
            // matches that of the class
            //ASSERT(CurrentTokenID() == TOKENID.TILDE);
            // "~" は呼び出し側で確認しておくこと。

            // ~ の後の識別子がクラス名と同じ名前であるか確認する。
            if (NextToken() != TOKENID.IDENTIFIER)
            {
                CheckToken(TOKENID.IDENTIFIER); // issue error.
            }
            else
            {
                if (tokenArray[CurrentTokenIndex()].Name != parentNode.NameNode.Name)
                {
                    Error(CSCERRID.ERR_BadDestructorName);
                }
                NextToken();
            }

            // Get parameters
            // パラメータ指定部分は "()" でなければならない。
            dtorNode.OpenParenIndex = CurrentTokenIndex();
            Eat(TOKENID.OPENPAREN);
            dtorNode.CloseParenIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSEPAREN);

            // Next should be the block.  Remember where it started for the interior parse.
            // メソッド本体は読み飛ばす。
            dtorNode.OpenIndex = CurrentTokenIndex();
            dtorNode.BodyNode = null;
            if (CurrentTokenID() == TOKENID.OPENCURLY)
            {
                SkipBlock(SKIPFLAGS.NORMALSKIP, out dtorNode.CloseIndex);
            }
            else
            {
                dtorNode.CloseIndex = CurrentTokenIndex();
                Eat(TOKENID.SEMICOLON);
                dtorNode.NodeFlagsEx |= NODEFLAGS.EX_METHOD_NOBODY;
            }

            AddToNodeTable(dtorNode);
            return dtorNode;
        }

        //------------------------------------------------------------
        // CParser.ParseConstant
        //
        /// <summary>
        /// Parse definitions of constants and create a MEMBERNODE instance.
        /// </summary>
        /// <param name="attrNode"></param>
        /// <param name="modifiers"></param>
        /// <param name="parentNode"></param>
        /// <param name="startIndex"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBERNODE ParseConstant(
            AGGREGATENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers)
        {
            FIELDNODE constNode = AllocNode(NODEKIND.CONST, parentNode, tokenIndex).AsCONST;
            bool isFirst = true;

            constNode.StartTokenIndex = startIndex;
            constNode.Flags = modifiers;
            constNode.AttributesNode
                = SetDefaultAttributeTarget(attrNode, ATTRTARGET.FIELD, ATTRTARGET.FIELD);
            if (attrNode != null)
            {
                attrNode.ParentNode = constNode;
            }

            NextToken();
            constNode.TypeNode = ParseType(constNode, false);

            CListMaker list = new CListMaker(this);
            int comma = -1;

            while (true)
            {
                BASENODE varDeclNode = ParseVariableDeclarator(
                    constNode,
                    constNode,
                    (uint)PARSEDECLFLAGS.CONST,
                    isFirst,
                    -1);

                AddToNodeTable(varDeclNode);
                list.Add(varDeclNode, comma);
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
                isFirst = false;
            }
            constNode.DeclarationsNode = list.GetList(constNode);

            constNode.CloseIndex = CurrentTokenIndex();
            Eat(TOKENID.SEMICOLON);
            return constNode;
        }

        //------------------------------------------------------------
        // CParser.ParseMethod
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="startIndex"></param>
        /// <param name="attrNode"></param>
        /// <param name="modifiers"></param>
        /// <param name="typeNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBERNODE ParseMethod(
            AGGREGATENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers,
            TYPEBASENODE typeNode)
        {
            METHODNODE methodNode = AllocNode(NODEKIND.METHOD, parentNode, tokenIndex) as METHODNODE;

            // Store info given to us
            methodNode.StartTokenIndex = startIndex;
            methodNode.Flags = modifiers;
            methodNode.ReturnTypeNode = typeNode;
            if (typeNode != null)
            {
                typeNode.ParentNode = methodNode;
            }
            methodNode.AttributesNode = SetDefaultAttributeTarget(
                attrNode,
                ATTRTARGET.METHOD,
                (ATTRTARGET.METHOD | ATTRTARGET.RETURN));
            if (attrNode != null)
            {
                attrNode.ParentNode = methodNode;
            }
            methodNode.InteriorNode = null;

            // Parse the name (it could be qualified)
            methodNode.NameNode = ParseMethodName(methodNode);

            // Get parameters
            ParseParameterList(
                methodNode,
                out methodNode.ParametersNode,
                out methodNode.OpenParenIndex,
                out methodNode.CloseParenIndex,
                out methodNode.IsExtensionMethod,
                (int)kppo.AllowAll);

            // parse constraints
            methodNode.ConstraintsNode = ParseConstraintClause(
                methodNode,
                methodNode.NameNode.LastNameOfDottedName.Kind == NODEKIND.GENERICNAME);

            // When a generic method overrides a generic method declared in a base class,
            // or is an explicit interface member implementation of a method in a base interface,
            // the method shall not specify any type-parameter-constraints-clauses.
            // In these cases, the type parameters of the method inherit constraints
            // from the method being overridden or implemented

            if (methodNode.ConstraintsNode != null &&
                ((modifiers & NODEFLAGS.MOD_OVERRIDE) != 0 ||
                    (methodNode.NameNode != null && methodNode.NameNode.Kind == NODEKIND.DOT)))
            {
                bool missingName;
                int iTok = GetFirstToken(methodNode.ConstraintsNode, ExtentFlags.FULL, out missingName);
                ErrorAtToken(iTok, CSCERRID.ERR_OverrideWithConstraints);
            }

            // Next should be the block.  Remember where it started for the interior parse.
            methodNode.OpenIndex = CurrentTokenIndex();
            methodNode.BodyNode = null;
            if (CurrentTokenID() == TOKENID.OPENCURLY)
            {
                SkipBlock(SKIPFLAGS.NORMALSKIP, out methodNode.CloseIndex);
            }
            else
            {
                methodNode.CloseIndex = CurrentTokenIndex();
                Eat(TOKENID.SEMICOLON);
                methodNode.NodeFlagsEx |= NODEFLAGS.EX_METHOD_NOBODY;
            }

            AddToNodeTable(methodNode);
            return methodNode;
        }

        //------------------------------------------------------------
        // CParser.ParseProperty
        //
        /// <summary></summary>
        /// <param name="nodeKind"></param>
        /// <param name="parentNode"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="startIndex"></param>
        /// <param name="attrNode"></param>
        /// <param name="modifiers"></param>
        /// <param name="typeNode"></param>
        /// <param name="isEvent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBERNODE ParseProperty(
            NODEKIND nodeKind,
            AGGREGATENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers,
            TYPEBASENODE typeNode,
            bool isEvent)
        {
            PROPERTYNODE propertyNode = AllocNode(nodeKind, parentNode, tokenIndex).AsANYPROPERTY;
            int openIndex = -1;

            propertyNode.StartTokenIndex = startIndex;
            propertyNode.Flags = modifiers;
            propertyNode.TypeNode = typeNode;
            propertyNode.NodeFlagsEx = (isEvent ? NODEFLAGS.EX_EVENT : 0);
            typeNode.ParentNode = propertyNode;
            propertyNode.AttributesNode = isEvent ?
                SetDefaultAttributeTarget(attrNode, ATTRTARGET.EVENT, ATTRTARGET.EVENT) :
                SetDefaultAttributeTarget(attrNode, ATTRTARGET.PROPERTY, ATTRTARGET.PROPERTY);
            if (attrNode != null)
            {
                attrNode.ParentNode = propertyNode;
            }
            propertyNode.OpenSquare = -1;
            propertyNode.ParametersNode = null;
            propertyNode.CloseSquare = -1;

            if (nodeKind == NODEKIND.PROPERTY)
            {
                propertyNode.NameNode = ParseGenericQualifiedNameList(propertyNode, false);
            }
            else
            {
                propertyNode.NameNode = ParseIndexerName(propertyNode);
                bool btemp = false;

                ParseParameterList(
                    propertyNode,
                    out propertyNode.ParametersNode,
                    out propertyNode.OpenSquare,
                    out propertyNode.CloseSquare,
                    out btemp,
                    (int)(kppo.Square | kppo.NoRefOrOut | kppo.NoVarargs));

                if (propertyNode.ParametersNode == null)
                {
                    ErrorAtToken(propertyNode.CloseSquare, CSCERRID.ERR_IndexerNeedsParam);
                }
            }

            // Parse body.

            propertyNode.OpenCurlyIndex = CurrentTokenIndex();
            if (CurrentTokenID() == TOKENID.OPENCURLY)
            {
                openIndex = CurrentTokenIndex();
            }
            Eat(TOKENID.OPENCURLY);

            bool isInterface = (parentNode.Kind == NODEKIND.INTERFACE);

            propertyNode.GetNode = propertyNode.SetNode = null;

            for (int i = 0; i < 4; i++)
            {
                if (CurrentTokenID() == TOKENID.CLOSECURLY) break;

                BASENODE accessorAttrNode = ParseAttributes(propertyNode, ATTRTARGET.METHOD, (ATTRTARGET)0);
                int accessorTokenIndex = CurrentTokenIndex();
                NODEFLAGS accessorModifiers = (NODEFLAGS)0;

                const int UNKNOWN = 0;
                const int GET = 1;
                const int SET = 2;
                //ACCESSORNODE** targetAccessor = null;
                int targetAccessor = UNKNOWN;
                ACCESSORNODE targetAccessorNode = null;

                if (isEvent)
                {
                    while ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_MODIFIER) != 0)
                    {
                        Error(CSCERRID.ERR_NoModifiersOnAccessor);
                        NextToken();
                    }
                }
                else
                {
                    accessorModifiers = ParseModifiers(true);
                    if (accessorModifiers != 0)
                    {
                        ReportFeatureUse("CSCSTR_FeaturePropertyAccessorMods");
                    }
                }

                if (accessorAttrNode != null && CurrentTokenID() != TOKENID.IDENTIFIER)
                {
                    PrevToken();
                    CSTOKEN token = tokenArray[CurrentTokenIndex()];
                    if (token.TokenID != TOKENID.IDENTIFIER)
                    {
                        NextToken();
                    }
                    else
                    {
                        string name = token.Name;
                        if (name != SpecName(SPECIALNAME.REMOVE) &&
                            name != SpecName(SPECIALNAME.ADD) &&
                            name != SpecName(SPECIALNAME.GET) &&
                            name != SpecName(SPECIALNAME.SET))
                        {
                            NextToken();
                        }
                    }
                }

                if (CurrentTokenID() == TOKENID.IDENTIFIER)
                {
                    string name = tokenArray[CurrentTokenIndex()].Name;

                    if (isEvent)
                    {
                        if (name == SpecName(SPECIALNAME.REMOVE))
                        {
                            targetAccessor = GET;
                        }
                        else if (name == SpecName(SPECIALNAME.ADD))
                        {
                            targetAccessor = SET;
                        }
                        else
                        {
                            Error(CSCERRID.ERR_AddOrRemoveExpected);
                        }

                        if (isInterface && (targetAccessor != 0))
                        {
                            Error(CSCERRID.ERR_EventPropertyInInterface);
                        }
                    }
                    else
                    {
                        if (name == SpecName(SPECIALNAME.GET))
                        {
                            targetAccessor = GET;
                        }
                        else if (name == SpecName(SPECIALNAME.SET))
                        {
                            targetAccessor = SET;
                        }
                        else
                        {
                            Error(CSCERRID.ERR_GetOrSetExpected);
                        }
                    }

                    NextToken();
                }
                else
                {
                    Error(isEvent ? CSCERRID.ERR_AddOrRemoveExpected : CSCERRID.ERR_GetOrSetExpected);
                }

                if (targetAccessor == 0 && accessorAttrNode != null)
                {
                    if (propertyNode.GetNode == null)
                    {
                        targetAccessor = GET;
                    }
                    else if (propertyNode.SetNode == null)
                    {
                        targetAccessor = SET;
                    }
                }

                if (targetAccessor != UNKNOWN)
                {
                    DebugUtil.Assert(targetAccessor == GET || targetAccessor == SET);

                    if (targetAccessor == GET && propertyNode.GetNode != null ||
                        targetAccessor == SET && propertyNode.SetNode != null)
                    {
                        ErrorAtToken(accessorTokenIndex, CSCERRID.ERR_DuplicateAccessor);
                    }

                    ACCESSORNODE accessorNode =
                        AllocNode(NODEKIND.ACCESSOR, propertyNode, accessorTokenIndex) as ACCESSORNODE;
                    accessorNode.AttributesNode = SetDefaultAttributeTarget(
                        accessorAttrNode,
                        ATTRTARGET.METHOD,
                        ((targetAccessor == GET && !isEvent) ?
                        ATTRTARGET.METHOD | ATTRTARGET.RETURN :
                        ATTRTARGET.METHOD | ATTRTARGET.RETURN | ATTRTARGET.PARAMETER));
                    accessorNode.Flags = accessorModifiers;
                    accessorNode.BodyNode = null;
                    accessorNode.OpenCurlyIndex = CurrentTokenIndex();
                    accessorNode.InteriorNode = null;
                    if (targetAccessor == GET)
                    {
                        propertyNode.GetNode = accessorNode;
                        targetAccessorNode = propertyNode.GetNode;
                    }
                    else
                    {
                        propertyNode.SetNode = accessorNode;
                        targetAccessorNode = propertyNode.SetNode;
                    }
                }

                if (CurrentTokenID() == TOKENID.OPENCURLY)
                {
                    if (targetAccessor != 0)
                    {
                        SkipBlock(SKIPFLAGS.F_INACCESSOR, out targetAccessorNode.CloseCurlyIndex);
                    }
                    else
                    {
                        int idx;
                        SkipBlock(SKIPFLAGS.F_INACCESSOR, out idx);
                    }
                }
                else
                {
                    if (isEvent && !isInterface)
                    {
                        Error(CSCERRID.ERR_AddRemoveMustHaveBody);
                    }

                    if (targetAccessor != 0)
                    {
                        targetAccessorNode.NodeFlagsEx |= NODEFLAGS.EX_METHOD_NOBODY;
                        targetAccessorNode.CloseCurlyIndex = CurrentTokenIndex();
                    }

                    if (CurrentTokenID() == TOKENID.SEMICOLON)
                    {
                        NextToken();
                    }
                    else if (openIndex != -1)
                    {
                        if (targetAccessorNode != null || openIndex == -1)
                        {
                            ErrorAfterPrevToken(CSCERRID.ERR_SemiOrLBraceExpected);
                        }
                        else
                        {
                            Rewind(openIndex);
                            SkipBlock(SKIPFLAGS.F_INPROPERTY, out propertyNode.CloseIndex);

                            int getterClose =
                                propertyNode.GetNode != null ? propertyNode.GetNode.CloseCurlyIndex : -1;
                            int setterClose =
                                propertyNode.SetNode != null ? propertyNode.SetNode.CloseCurlyIndex : -1;

                            propertyNode.CloseIndex =
                                Math.Max(propertyNode.CloseIndex, Math.Max(setterClose, getterClose));
                            goto FAIL;
                        }
                    }
                }
            }

            propertyNode.CloseIndex = CurrentTokenIndex();
            if (CheckToken(TOKENID.CLOSECURLY) && (NextToken() == TOKENID.SEMICOLON))
            {
                Error(CSCERRID.ERR_UnexpectedSemicolon);
                NextToken();
            }

        FAIL:
            AddToNodeTable(propertyNode);
            return propertyNode;
        }

        //------------------------------------------------------------
        // CParser.ParseField
        //
        /// <summary>1 つのフィールドを解析する。</summary>
        /// <param name="attrNode">このフィールドに設定されている属性のノード。</param>
        /// <param name="isEvent">event の場合は true を指定する。</param>
        /// <param name="isFixed">fixed の場合は true を指定する。</param>
        /// <param name="modifiers">このフィールに設定されている修飾子。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <param name="startIndex">非属性ノードのうち最初のもののインデックス。</param>
        /// <param name="tokenIndex">作成するノードに対応させるトークンのインデックス。</param>
        /// <param name="typeNode">このフィールドに設定されている型のノード。</param>
        /// <returns>作成された MEMBERNODE を返す。</returns>
        // 
        // field-declaration:
        //     [attributes] [field-modifiers] type variable-declarators ";"
        //
        // variable-declarators:
        // 	variable-declarator
        // 	variable-declarators "," variable-declarator
        //
        // variable-declarator:
        // 	identifier
        // 	identifier "=" variable-initializer
        //------------------------------------------------------------
        internal MEMBERNODE ParseField(
            AGGREGATENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers,
            TYPEBASENODE typeNode,
            bool isEvent,
            bool isFixed)
        {
            FIELDNODE fieldNode = AllocNode(NODEKIND.FIELD, parentNode, tokenIndex) as FIELDNODE;
            bool isFirst = true;

            fieldNode.StartTokenIndex = startIndex;
            fieldNode.TypeNode = typeNode;
            typeNode.ParentNode = fieldNode;
            fieldNode.Flags = modifiers;
            fieldNode.NodeFlagsEx = (isEvent ? NODEFLAGS.EX_EVENT : 0);

            ATTRTARGET alAllowed;

            if (!isEvent)
            {
                alAllowed = ATTRTARGET.FIELD;
            }
            else
            {
                alAllowed = (ATTRTARGET.EVENT | ATTRTARGET.METHOD);
                switch (parentNode.Kind)
                {
                    default:
                        DebugUtil.VsFail("Bad parent kind in ParseField");
                        break;

                    case NODEKIND.INTERFACE:
                        break;

                    case NODEKIND.CLASS:
                        if ((modifiers & NODEFLAGS.MOD_ABSTRACT) != 0)
                        {
                            break;
                        }
                        goto case NODEKIND.STRUCT;

                    case NODEKIND.STRUCT:
                        alAllowed = (ATTRTARGET.EVENT | ATTRTARGET.FIELD | ATTRTARGET.METHOD);
                        break;
                }
            }

            fieldNode.AttributesNode = SetDefaultAttributeTarget(
                attrNode,
                (isEvent ? ATTRTARGET.EVENT : ATTRTARGET.FIELD), alAllowed);
            if (attrNode != null)
            {
                attrNode.ParentNode = fieldNode;
            }

            CListMaker list = new CListMaker(this);
            int comma = -1;

            while (true)
            {
                BASENODE varDeclNode = ParseVariableDeclarator(
                    fieldNode,
                    fieldNode,
                    (uint)(isFixed ? PARSEDECLFLAGS.FIXED : 0),
                    isFirst,
                    -1);

                AddToNodeTable(varDeclNode);
                list.Add(varDeclNode, comma);
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
                isFirst = false;
            }
            fieldNode.DeclarationsNode = list.GetList(fieldNode);

            fieldNode.CloseIndex = CurrentTokenIndex();
            if (isEvent && CurrentTokenID() == TOKENID.DOT)
            {
                Error(CSCERRID.ERR_ExplicitEventFieldImpl);
            }

            Eat(TOKENID.SEMICOLON);
            return fieldNode;
        }

        //------------------------------------------------------------
        // CParser.ParseOperator
        //
        /// <summary>演算子のオーバーロードを取得する。</summary>
        /// <param name="attrNode">指定された属性のノード。</param>
        /// <param name="modifiers">修飾子のビットフラグ。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <param name="startIndex">開始トークンのインデックス。</param>
        /// <param name="tokenIndex">ノードに設定するトークンのインデックス。</param>
        /// <param name="typeNode">返り値の型。</param>
        /// <returns>演算子のオーバーロードを表すノードを返す。</returns>
        //------------------------------------------------------------
        internal MEMBERNODE ParseOperator(
            AGGREGATENODE parentNode,
            int tokenIndex,
            int startIndex,
            BASENODE attrNode,
            NODEFLAGS modifiers,
            TYPEBASENODE typeNode)
        {
            OPERATORMETHODNODE operatorNode
                = AllocNode(NODEKIND.OPERATOR, parentNode, tokenIndex) as OPERATORMETHODNODE;

            operatorNode.StartTokenIndex = startIndex;
            operatorNode.ReturnTypeNode = typeNode;
            operatorNode.Flags = modifiers;
            operatorNode.AttributesNode = SetDefaultAttributeTarget(
                attrNode, ATTRTARGET.METHOD, (ATTRTARGET.METHOD | ATTRTARGET.RETURN));
            if (attrNode != null)
            {
                attrNode.ParentNode = operatorNode;
            }
            operatorNode.InteriorNode = null;

            TOKENID opTokId;
            int errorTokenIndex;

            if (CurrentTokenID() == TOKENID.IMPLICIT || CurrentTokenID() == TOKENID.EXPLICIT)
            {
                errorTokenIndex = CurrentTokenIndex();
                opTokId = CurrentTokenID();
                NextToken();

                // 変換演算子の場合、返り値を記述してはならない。

                if (CheckToken(TOKENID.OPERATOR) && typeNode != null)
                {
                    ErrorAtToken(
                        typeNode.TokenIndex,
                        CSCERRID.ERR_BadOperatorSyntax,
                        new ErrArg(TokenInfoArray[(int)opTokId].Text));
                }
            }
            else
            {
                DebugUtil.Assert(CurrentTokenID() == TOKENID.OPERATOR);

                opTokId = NextToken();
                errorTokenIndex = CurrentTokenIndex();
                if (opTokId == TOKENID.IMPLICIT || opTokId == TOKENID.EXPLICIT)
                {
                    ErrorAtToken(
                        typeNode != null ? typeNode.TokenIndex : errorTokenIndex,
                        CSCERRID.ERR_BadOperatorSyntax,
                        new ErrArg(TokenInfoArray[(int)opTokId].Text));
                }

                // ECMA334 では right-shift に対応する記号は定義されていないため、
                // CLexer は ">>" を連続する TOKENID.GREATER と解釈するだけである。
                // そこでこの段階で ">>" を right-shift とする。

                if (CurrentTokenID() == TOKENID.GREATER && PeekToken() == TOKENID.GREATER)
                {
                    POSDATA posFirst = new POSDATA(CurrentTokenPos());
                    POSDATA posNext = new POSDATA(tokenArray[PeekTokenIndex()]);
                    if (posFirst.LineIndex == posNext.LineIndex &&
                        (posFirst.CharIndex + 1) == posNext.CharIndex)
                    {
                        opTokId = TOKENID.SHIFTRIGHT;
                        NextToken();
                    }
                }
            }

            NextToken();

            // typeNode は返り値の型を示すノードである。
            // これが必要ないのは変換演算子の場合だけである。

            if (typeNode == null)
            {
                if (opTokId != TOKENID.IMPLICIT && opTokId != TOKENID.EXPLICIT)
                {
                    ErrorAtToken(
                        CurrentTokenIndex(),
                        CSCERRID.ERR_BadOperatorSyntax2,
                        new ErrArg(TokenInfoArray[(int)opTokId].Text));
                }
                typeNode = ParseType(operatorNode, false);
                operatorNode.ReturnTypeNode = typeNode;
            }
            else
            {
                typeNode.ParentNode = operatorNode;
            }

            bool btemp = false;
            ParseParameterList(
                operatorNode,
                out operatorNode.ParametersNode,
                out operatorNode.OpenParenIndex,
                out operatorNode.CloseParenIndex,
                out btemp,
                (int)(kppo.NoRefOrOut | kppo.NoParams | kppo.NoVarargs));
            operatorNode.TokenId = opTokId;

            // パラメータの個数を確認する。
            int argCount = NodeUtil.CountAnyBinOpListNode(operatorNode.ParametersNode, NODEKIND.LIST);

            switch (argCount)
            {
                case 1:
                    if ((TokenInfoArray[(int)opTokId].Flags & TOKFLAGS.F_OVLUNOP) == 0)
                    {
                        ErrorAtToken(errorTokenIndex, CSCERRID.ERR_OvlUnaryOperatorExpected);
                        operatorNode.Operator = OPERATOR.NONE;
                    }
                    else
                    {
                        operatorNode.Operator = (OPERATOR)TokenInfoArray[(int)opTokId].UnaryOperator;
                    }
                    break;
                case 2:
                    if ((TokenInfoArray[(int)opTokId].Flags & TOKFLAGS.F_OVLBINOP) == 0)
                    {
                        ErrorAtToken(errorTokenIndex, CSCERRID.ERR_OvlBinaryOperatorExpected);
                        operatorNode.Operator = OPERATOR.NONE;
                    }
                    else
                    {
                        operatorNode.Operator = (OPERATOR)TokenInfoArray[(int)opTokId].BinaryOperator;
                    }
                    break;

                default:
                    if ((TokenInfoArray[(int)opTokId].Flags & TOKFLAGS.F_OVLBINOP) != 0)
                    {
                        ErrorAtToken(
                            errorTokenIndex,
                            CSCERRID.ERR_BadBinOpArgs,
                            new ErrArg(TokenInfoArray[(int)opTokId].Text));
                    }
                    else if ((TokenInfoArray[(int)opTokId].Flags & TOKFLAGS.F_OVLUNOP) != 0)
                    {
                        ErrorAtToken(
                            errorTokenIndex,
                            CSCERRID.ERR_BadUnOpArgs,
                            new ErrArg(TokenInfoArray[(int)opTokId].Text));
                    }
                    else
                    {
                        ErrorAtToken(errorTokenIndex, CSCERRID.ERR_OvlOperatorExpected);
                    }
                    operatorNode.Operator = OPERATOR.NONE;
                    break;
            }

            operatorNode.OpenIndex = CurrentTokenIndex();
            operatorNode.BodyNode = null;
            if (CurrentTokenID() == TOKENID.SEMICOLON)
            {
                operatorNode.CloseIndex = CurrentTokenIndex();
                Eat(TOKENID.SEMICOLON);
                operatorNode.NodeFlagsEx |= NODEFLAGS.EX_METHOD_NOBODY;
            }
            else
            {
                // 本体が記述されている場合、この段階ではスキップする。
                SkipBlock(SKIPFLAGS.NORMALSKIP, out operatorNode.CloseIndex);
            }

            AddToNodeTable(operatorNode);
            return operatorNode;
        }

        //------------------------------------------------------------
        // CParser.ParseType
        //
        /// <summary>
        /// <para>Create a node representing a type.</para>
        /// <para>Precess "?", "*", "[]" as follows:</para>
        /// <list type="number">
        /// <item>If "?" follows a type name, create a NULLABLETYPE instance,
        /// and set the underlying type to ElementTypeNode.</item>
        /// <item>If "*" follows a type name, create a POINTERTYPENODE instance.
        /// If multiple "*"s are, regard as nesting,
        /// For example, regard "int**" as "(int*)*".</item>
        /// <item>If "[]" follows a type name, create a ARRAYTYPENODE instance.
        /// If multiple "[]"s are, regard as nesting,
        /// For example, regard "int[][]" as "(int[])[]".</item>
        /// </list>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="withIsOrAs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPEBASENODE ParseType(BASENODE parent, bool withIsOrAs)   // = false);
        {
            //--------------------------------------------------
            // Parse the underling type.
            //--------------------------------------------------
            bool hadError = false;
            TYPEBASENODE typeNode = ParseUnderlyingType(parent, out hadError);
            if (hadError)
            {
                return typeNode;
            }

            //--------------------------------------------------
            // If "?" follows a type name,
            //--------------------------------------------------
            if (CurrentTokenID() == TOKENID.QUESTION)
            {
                TOKENID tidNext = PeekToken();
                if (withIsOrAs &&
                    ((TokenInfoArray[(int)tidNext].Flags &
                    (TOKFLAGS.F_TERM | TOKFLAGS.F_PREDEFINED)) != 0 ||
                    IsUnaryOperator(tidNext)))
                {
                    return typeNode;
                }

                ReportFeatureUse("CSCSTR_FeatureNullable");
                NULLABLETYPENODE nodeNull = AllocNode(NODEKIND.NULLABLETYPE, parent) as NULLABLETYPENODE;
                nodeNull.ElementTypeNode = typeNode;
                typeNode.ParentNode = nodeNull;
                typeNode = nodeNull;
                NextToken();
            }

            //--------------------------------------------------
            // If "*" follows a type name,
            //--------------------------------------------------
            typeNode = ParsePointerTypeMods(parent, typeNode);

            //--------------------------------------------------
            // If "[" follows a type name,
            //--------------------------------------------------
            if (CurrentTokenID() == TOKENID.OPENSQUARE)
            {
                ARRAYTYPENODE nodeTop = AllocNode(NODEKIND.ARRAYTYPE, parent) as ARRAYTYPENODE;
                nodeTop.Dimensions = ParseArrayRankSpecifier(nodeTop, true);
                DebugUtil.Assert(nodeTop.Dimensions > 0);
                ARRAYTYPENODE nodeLast = nodeTop;

                while (CurrentTokenID() == TOKENID.OPENSQUARE)
                {
                    ARRAYTYPENODE nodeT = AllocNode(NODEKIND.ARRAYTYPE, nodeLast) as ARRAYTYPENODE;
                    nodeLast.ElementTypeNode = nodeT;
                    nodeT.Dimensions = ParseArrayRankSpecifier(nodeT, true);
                    DebugUtil.Assert(nodeT.Dimensions > 0);
                    nodeLast = nodeT;
                }
                nodeLast.ElementTypeNode = typeNode;
                typeNode.ParentNode = nodeLast;
                typeNode = nodeTop;
            }
            return typeNode;
        }

        //------------------------------------------------------------
        // CParser.ParsePredefinedType
        //
        /// <summary>
        /// <para>If the current token represents a predefined type,
        /// create a PREDEFINEDTYPENODE instance.</para>
        /// <para>Otherwise, throw an exception.</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PREDEFINEDTYPENODE ParsePredefinedType(BASENODE parent)
        {
            DebugUtil.Assert((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_PREDEFINED) != 0);
            PREDEFINEDTYPENODE typeNode = AllocNode(NODEKIND.PREDEFINEDTYPE, parent) as PREDEFINEDTYPENODE;

            typeNode.TokenIndex = CurrentTokenIndex();
            typeNode.ParentNode = parent;
            typeNode.Type = TokenInfoArray[(int)CurrentTokenID()].PredefinedType;

            NextToken();
            return typeNode;
        }

        //------------------------------------------------------------
        // CParser.ParseUnderlyingType
        //
        /// <summary>
        /// <para>Get a base type and create a TYPEBASENODE instance reprsenting the base type.</para>
        /// <para>If the current token is 
        /// 不適切なトークンの場合も名前なしで TYPEBASENODE を作成する。
        /// </para>
        /// </summary>
        /// <param name="hadError">型を取得できなかった場合は false をセットする。</param>
        /// <param name="parent">親となるノード。</param>
        //------------------------------------------------------------
        internal TYPEBASENODE ParseUnderlyingType(BASENODE parent, out bool hadError)
        {
            hadError = false;
            TYPEBASENODE typeBaseNode;

            // Predefined type
            if ((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_PREDEFINED) != 0)
            {
                // Create PREDEFINEDTYPENODE from the current token.
                typeBaseNode = ParsePredefinedType(parent);

                // void must be used int form "void *" if not for return types of methods
                if ((typeBaseNode as PREDEFINEDTYPENODE).Type == PREDEFTYPE.VOID &&
                    CurrentTokenID() != TOKENID.STAR)
                {
                    hadError = true;
                    ErrorAtToken(
                        typeBaseNode.TokenIndex,
                        (parent != null && parent.Kind == NODEKIND.PARAMETER)
                        ? CSCERRID.ERR_NoVoidParameter : CSCERRID.ERR_NoVoidHere);
                }
            }
            // Identifiers
            else if (IsNameStart(CurrentTokenID()))
            {
                typeBaseNode = ParseNamedType(parent);
            }
            // Otherwise, we have an error. Create a NAMEDTYPENODE instance.
            else
            {
                typeBaseNode = AllocNode(NODEKIND.NAMEDTYPE, parent) as NAMEDTYPENODE;

                Error(CSCERRID.ERR_TypeExpected);
                (typeBaseNode as NAMEDTYPENODE).NameNode =
                    ParseMissingName(typeBaseNode, CurrentTokenIndex());
                hadError = true;
            }
            return typeBaseNode;
        }

        //------------------------------------------------------------
        // CParser.ParsePointerTypeMods
        //
        /// <summary>
        /// <para>現在のトークンが * の場合、先行する型と合わせて POINTERTYPE のインスタンスを作成する。</para>
        /// <para>* が連続している場合は ElementTypeNode フィールドを使って入れ子状に構成する。</para>
        /// <para>現在のトークンが * でない場合は baseTypeNode をそのまま返す。</para>
        /// </summary>
        /// <param name="baseTypeNode">* の直前にある型のノード</param>
        /// <param name="parentNode">親となるノード</param>
        /// <returns>作成した POINTERTYPE インスタンス。</returns>
        /// <remarks>
        /// T **x の場合、引数の baseType には T を表す TYPEBASENODE を指定する。
        /// 戻り値は T** を表す（入れ子状に構成された）TYPEBASENODE である。
        /// </remarks>
        //------------------------------------------------------------
        internal TYPEBASENODE ParsePointerTypeMods(BASENODE parentNode, TYPEBASENODE baseTypeNode)
        {
            while (CurrentTokenID() == TOKENID.STAR)
            {
                POINTERTYPENODE pointerNode
                    = AllocNode(NODEKIND.POINTERTYPE, parentNode) as POINTERTYPENODE;
                NextToken();
                pointerNode.ElementTypeNode = baseTypeNode;
                baseTypeNode.ParentNode = pointerNode;
                baseTypeNode = pointerNode;
            }
            return baseTypeNode;
        }

        //------------------------------------------------------------
        // CParser.ParseNamedType
        //
        /// <summary>
        /// <para>We parse type names as a sequence of identifiers with generic.</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NAMEDTYPENODE ParseNamedType(BASENODE parent)
        {
            NAMEDTYPENODE typeNode = AllocNode(NODEKIND.NAMEDTYPE, parent) as NAMEDTYPENODE;

            typeNode.NameNode = ParseGenericQualifiedNameList(typeNode, false);

            return typeNode;
        }

        //------------------------------------------------------------
        // CParser.ParseClassType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPEBASENODE ParseClassType(BASENODE parent)
        {
            TYPEBASENODE typeNode;

            if (CurrentTokenID() == TOKENID.OBJECT || CurrentTokenID() == TOKENID.STRING)
            {
                typeNode = ParsePredefinedType(parent);
            }
            else
            {
                if (!IsNameStart(CurrentTokenID()))
                {
                    Error(CSCERRID.ERR_ClassTypeExpected);
                }

                typeNode = ParseNamedType(parent);
            }
            return typeNode;
        }

        //------------------------------------------------------------
        // CParser.ParseReturnType
        //
        /// <summary>
        /// Create a TYPEBASENODE instance which represents a type of a return value.
        /// However, if "void" except "void *", create a PREDEFINEDTYPENODE instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPEBASENODE ParseReturnType(BASENODE parent)
        {
            if (CurrentTokenID() == TOKENID.VOID && PeekToken(1) != TOKENID.STAR)
            {
                return ParsePredefinedType(parent);
            }
            return ParseType(parent, false);
        }

        //------------------------------------------------------------
        // CParser.ParseBaseTypesClause
        //
        // class-base:
        //     ":" class-type
        //     ":" interface-type-list
        //     ":" class-type "," interface-type-list
        // 
        // interface-type-list:
        //     interface-type
        //     interface-type-list "," interface-type
        //
        /// <summary>
        /// Parse the base class and interfaces.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseBaseTypesClause(BASENODE parentNode)
        {
            if (CurrentTokenID() == TOKENID.COLON)
            {
                NextToken();

                CListMaker list = new CListMaker(this);
                int comma = -1;

                while (true)
                {
                    TYPEBASENODE typeBaseNode = ParseTypeName(parentNode, false);
                    if (typeBaseNode != null)
                    {
                        list.Add(typeBaseNode, comma);
                    }
                    if (CurrentTokenID() != TOKENID.COMMA)
                    {
                        break;
                    }
                    comma = CurrentTokenIndex();
                    NextToken();
                }
                return list.GetList(parentNode);
            }
            else
            {
                return null;
            }
        }

        //------------------------------------------------------------
        // CParser.ParseTypeName
        //
        /// <summary>
        /// <para>Get tokens of a type with "?", "*", "[]", and create a TYPBASENODE instance.</para>
        /// <para>This TYPEBASENODE instance is of PREDEFINEDTYPE or NAMEDTYPE.</para>
        /// </summary>
        /// <param name="hasConstraint">制約がついている場合は true を指定する。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>得られた型から作成した TYPEBASENODE。</returns>
        //------------------------------------------------------------
        internal TYPEBASENODE ParseTypeName(BASENODE parentNode, bool hasConstraint)
        {
            TYPEBASENODE typeBaseNode = ParseType(parentNode, false);
            if (typeBaseNode.Kind != NODEKIND.PREDEFINEDTYPE && typeBaseNode.Kind != NODEKIND.NAMEDTYPE)
            {
                if (hasConstraint)
                {
                    ErrorAtToken(typeBaseNode.TokenIndex, CSCERRID.ERR_BadConstraintType);
                }
                else
                {
                    ErrorAtToken(typeBaseNode.TokenIndex, CSCERRID.ERR_BadBaseType);
                }
            }

            return typeBaseNode;
        }

        //------------------------------------------------------------
        // CParser.ParseInstantiation
        //
        // type-parameter-list:
        // 	"<" type-parameters ">"
        //
        // type-parameters:
        // 	[attributes] type-parameter
        // 	type-parameters "," [attributes] type-parameter
        //
        // type-parameter:
        // 	identifier
        //
        /// <summary>
        /// Parse type arguments to the type parameters of generic,
        /// and return them as a list of TYPENODE instances.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="allowAttrs">may have attributes?</param>
        /// <param name="closeTokenIndex">The index of "&gt;"</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseInstantiation(BASENODE parent, bool allowAttrs, ref int closeTokenIndex)
        {
            if (CurrentTokenID() != TOKENID.OPENANGLE)
            {
                if (closeTokenIndex != 0)
                {
                    closeTokenIndex = -1;
                }
                return null;
            }

            ReportFeatureUse("CSCSTR_FeatureGenerics");
            NextToken();

            CListMaker listMaker = new CListMaker(this);
            int comma = -1;

            while (true)
            {
                TYPEBASENODE typeNode;

                if (CurrentTokenID() == TOKENID.OPENSQUARE && allowAttrs)
                {
                    TYPEWITHATTRNODE node = AllocNode(NODEKIND.TYPEWITHATTR, parent) as TYPEWITHATTRNODE;
                    node.AttributesNode = ParseAttributes(node, ATTRTARGET.TYPEVAR, ATTRTARGET.TYPEVAR);
                    node.TypeBaseNode = ParseType(node, false);
                    typeNode = node;
                }
                else
                {
                    typeNode = ParseType(parent, false);
                }

                listMaker.Add(typeNode, comma);
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
            }

            if (closeTokenIndex != 0)
            {
                closeTokenIndex = CurrentTokenIndex();
            }
            Eat(TOKENID.CLOSEANGLE);
            return listMaker.GetList(parent);
        }

        //------------------------------------------------------------
        // CParser.ScanOpenType
        //
        /// <summary>
        /// typeof 演算子のオペランドが open であるかを調べる。
        /// typeof の対象となるのは、非 generic な型、"void"、unbound な generic 型である。
        /// 返り値は型パラメータの個数。
        /// </summary>
        /// <returns>型パラメータの個数。</returns>
        //
        // unbound-type-name:
        // 	identifier  [generic-dimension-specifier]
        // 	identifier  "::"  identifier  [generic-dimension-specifier]
        // 	unbound-type-name  "."  identifier  [generic-dimension-specifier]
        //
        // generic-dimension-specifier:
        // 	"<"  [commas]  ">"
        //
        // commas:
        // 	","
        // 	commas  ","
        //------------------------------------------------------------
        internal int ScanOpenType()
        {
            int paramCount = 0;

            if (PeekToken() == TOKENID.COLONCOLON)
            {
                if (CurrentTokenID() != TOKENID.IDENTIFIER)
                {
                    return 0;
                }
                NextToken();
                NextToken();
            }

            for (; ; )
            {
                if (CurrentTokenID() != TOKENID.IDENTIFIER)
                {
                    return 0;
                }
                NextToken();

                if (CurrentTokenID() == TOKENID.OPENANGLE)
                {
                    NextToken();
                    paramCount++;
                    while (CurrentTokenID() == TOKENID.COMMA)
                    {
                        NextToken();
                        paramCount++;
                    }
                    if (CurrentTokenID() != TOKENID.CLOSEANGLE)
                    {
                        return 0;
                    }
                    NextToken();
                }

                if (CurrentTokenID() != TOKENID.DOT &&
                    CurrentTokenID() != TOKENID.COLONCOLON)
                {
                    return paramCount;
                }
                NextToken();
                //if (CurrentTokenID() != TOKENID.IDENTIFIER)
                //    return 0;
            }
        }

        //------------------------------------------------------------
        // CParser.ParseOpenType
        //
        /// <summary>
        /// typeof 演算子のオペランドを解析し、それを表す BASENODE を作成する。
        /// typeof の対象となるのは、非 generic 型、"void"、unbound な generic 型である。
        /// </summary>
        /// <param name="parentNode">親となるノード。</param>
        //------------------------------------------------------------
        internal TYPEBASENODE ParseOpenType(BASENODE parentNode)
        {
            ReportFeatureUse("CSCSTR_FeatureGenerics");

            NAMEDTYPENODE nodeType
                = AllocNode(NODEKIND.OPENTYPE, parentNode).AsOPENTYPE;
            nodeType.NameNode = null;

            int dotIndex = -1;
            NAMENODE nodeCur;

            for (; ; )
            {
                //ASSERT(CurrentTokenID() == TOKENID.IDENTIFIER);
                if (CurrentTokenID() != TOKENID.IDENTIFIER)
                {
                    return nodeType;
                }

                if (PeekToken() == TOKENID.OPENANGLE)
                {
                    OPENNAMENODE nodeOpen
                        = AllocNode(NODEKIND.OPENNAME, nodeType) as OPENNAMENODE;
                    InitNameNode(nodeOpen, null);
                    NextToken();

                    //ASSERT(CurrentTokenID() == TOKENID.OPENANGLE);  // 確認済み
                    nodeOpen.OpenAngleIndex = CurrentTokenIndex();
                    NextToken();
                    nodeOpen.CountOfBlankParameters = 1;
                    while (CurrentTokenID() == TOKENID.COMMA)
                    {
                        NextToken();
                        nodeOpen.CountOfBlankParameters++;
                    }
                    //ASSERT(CurrentTokenID() == TOKENID.CLOSEANGLE);
                    nodeOpen.CloseAngleIndex = CurrentTokenIndex();
                    NextToken();

                    nodeCur = nodeOpen;
                }
                else
                {
                    nodeCur = ParseIdentifier(nodeType);
                }

                if (nodeType.NameNode != null)
                {
                    //ASSERT(dotIndex > 0);
                    //nodeType.NameNode
                    //    = AllocDotNode(
                    //    dotIndex, nodeType, nodeType.NameNode, nodeCur);

                    // 得られた名前を nodeType.NameNode の後ろに繋ぐ。
                    // nodeType.NameNode は名前の最後の部分を指すようにしておく。
                    BASENODE.LinkNodes(nodeType.NameNode, nodeCur);
                    nodeType.NameNode = nodeCur;
                }
                else
                {
                    //ASSERT(dotIndex == -1);
                    CheckForAlias(nodeCur);
                    nodeType.NameNode = nodeCur;
                }

                if (CurrentTokenID() != TOKENID.DOT &&
                    CurrentTokenID() != TOKENID.COLONCOLON)
                {
                    return nodeType;
                }
                dotIndex = CurrentTokenIndex();

                if (nodeType.NameNode.Kind != NODEKIND.ALIASNAME)
                {
                    CheckToken(TOKENID.DOT);
                }
                NextToken();
            }
        }

        //------------------------------------------------------------
        // CParser.ParseConstraint
        //
        /// <summary>
        /// トークン列から一つの制約（"where" の後の部分）を取得し、CONSTRAINTNODE を作成する。
        /// </summary>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>作成した CONSTRAINTNODE。</returns>
        //
        // type-parameter-constraints-clause:
        //     "where" type-parameter ":" type-parameter-constraints
        //
        // type-parameter-constraints:
        //     primary-constraint
        //     secondary-constraints
        //     constructor-constraint
        //     primary-constraint "," secondary-constraints
        //     primary-constraint "," constructor-constraint
        //     secondary-constraints "," constructor-constraint
        //     primary-constraint "," secondary-constraints "," constructor-constraint
        //
        // primary-constraint:
        //     class-type
        //     "class"
        //     "struct"
        //
        // secondary-constraints:
        //     interface-type
        //     type-parameter
        //     secondary-constraints "," interface-type
        //     secondary-constraints "," type-parameter
        //
        // constructor-constraint:
        //     "new" "(" ")"
        //------------------------------------------------------------
        internal CONSTRAINTNODE ParseConstraint(BASENODE parentNode)
        {
            CONSTRAINTNODE constraintNode = AllocNode(NODEKIND.CONSTRAINT, parentNode) as CONSTRAINTNODE;

            // 制約の対象となる型パラメータ名を取得する。
            constraintNode.NameNode = ParseIdentifier(constraintNode);

            constraintNode.Flags = 0;
            if (CurrentTokenID() == TOKENID.COLON)
            {
                constraintNode.EndTokenIndex = CurrentTokenIndex();
                NextToken();
            }
            else
            {
                // 型パラメータ名の次が : でない場合は、エラーメッセージを表示し、
                // 次に制約が記述してあると仮定して処理を続ける。
                constraintNode.EndTokenIndex = -1;
                Eat(TOKENID.COLON);
            }

            CListMaker list = new CListMaker(this);
            int commaIndex = -1;

            while (true)
            {
                switch (CurrentTokenID())
                {
                    case TOKENID.NEW:
                        if ((constraintNode.Flags & NODEFLAGS.CONSTRAINT_VALTYPE) != 0)
                        {
                            Error(CSCERRID.ERR_NewBoundWithVal);
                        }
                        Eat(TOKENID.NEW);
                        Eat(TOKENID.OPENPAREN);
                        constraintNode.EndTokenIndex = CurrentTokenIndex();
                        Eat(TOKENID.CLOSEPAREN);
                        constraintNode.Flags |= NODEFLAGS.CONSTRAINT_NEWABLE;

                        // 制約 "new()" は最後に指定しなければならない。
                        if (CurrentTokenID() == TOKENID.COMMA)
                        {
                            Error(CSCERRID.ERR_NewBoundMustBeLast);
                        }
                        break;

                    case TOKENID.CLASS:
                    case TOKENID.STRUCT:
                        constraintNode.Flags |= ((CurrentTokenID() == TOKENID.CLASS) ?
                            NODEFLAGS.CONSTRAINT_REFTYPE : NODEFLAGS.CONSTRAINT_VALTYPE);

                        // 制約 "class"、"struct" は最初でなければならない。
                        if (commaIndex >= 0)
                        {
                            Error(CSCERRID.ERR_RefValBoundMustBeFirst);
                        }
                        constraintNode.EndTokenIndex = CurrentTokenIndex();
                        NextToken();
                        break;

                    default:
                        BASENODE typeNode = ParseTypeName(constraintNode, true);
                        list.Add(typeNode, commaIndex);
                        constraintNode.EndTokenIndex = -1;
                        break;
                }
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                commaIndex = CurrentTokenIndex();
                constraintNode.EndTokenIndex = CurrentTokenIndex();
                NextToken();
            }
            constraintNode.BoundsNode = list.GetList(constraintNode);
            return constraintNode;
        }

        //------------------------------------------------------------
        // CParser.ParseConstraintClause
        //
        /// <summary>
        /// トークン列から型パラメータの制約を解析し、CONSTRAINTNODE 列を作成する。
        /// </summary>
        /// <param name="isAllowed">where 節を使用できる場合は true を指定する。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>type-parameter-constraints-clauses を表す CONSTRAINTNODE 列の先頭。</returns>
        //
        // type-parameter-constraints-clauses:
        //     type-parameter-constraints-clause
        //     type-parameter-constraints-clauses type-parameter-constraints-clause
        //
        // type-parameter-constraints-clause:
        //     "where" type-parameter ":" type-parameter-constraints
        //------------------------------------------------------------
        internal BASENODE ParseConstraintClause(BASENODE parentNode, bool isAllowed)
        {
            if (!CheckForSpecName(SPECIALNAME.WHERE))
            {
                return null;
            }

            if (!isAllowed)
            {
                Error(CSCERRID.ERR_ConstraintOnlyAllowedOnGenericDecl);
            }

            CListMaker list = new CListMaker(this);

            do
            {
                NextToken();

                CONSTRAINTNODE constraintNode = ParseConstraint(parentNode);
                list.Add(constraintNode, -1);
            } while (CheckForSpecName(SPECIALNAME.WHERE));
            return isAllowed ? list.GetList(parentNode) : null;
        }

        //------------------------------------------------------------
        // CParser.ParseIdentifier
        //
        /// <summary>
        /// <para>If the current token is an identifier, create a NAMENODE instance,
        /// with the name of the current token. then advance to next.</para>
        /// <para>If not identifier, create a NAMENODE with NAME_MISSING flag.
        /// In this case, the current index is not advanced.</para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NAMENODE ParseIdentifier(BASENODE parentNode)
        {
            if (!CheckToken(TOKENID.IDENTIFIER))
            {
                return ParseMissingName(parentNode, CurrentTokenIndex());
            }
#if false
            // (CS3) Query Expression
            if (IsQueryExpressionParseMode())
            {
                string str = tokenArray[CurrentTokenIndex()].Name;
                QueryKeywordEnum qkw = GetQueryKeywordID(str);
                if (qkw != QueryKeywordEnum.None)
                {
                    Error(CSCERRID.ERR_IdentifierExpected);
                    return ParseMissingName(parentNode, CurrentTokenIndex());
                }
            }
#endif
            NAMENODE nameNode = AllocNameNode(null, CurrentTokenIndex());
            nameNode.ParentNode = parentNode;
            NextToken();
            return nameNode;
        }

        //------------------------------------------------------------
        // CParser.ParseIdentifierOrKeyword
        //
        /// <summary>
        /// <para>If the current token is an identifier, create a NAMENODE instance with it.</para>
        /// <para>Or if the token is a keyword, create a NAMENODE instance.</para>
        /// <para>Otherwise, throw an exception.</para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NAMENODE ParseIdentifierOrKeyword(BASENODE parentNode)
        {
            DebugUtil.Assert(CurrentTokenID() <= TOKENID.IDENTIFIER);

            if (CurrentTokenID() == TOKENID.IDENTIFIER)
            {
                return ParseIdentifier(parentNode);
            }
            else
            {
                NAMENODE nameNode = AllocNode(NODEKIND.NAME, parentNode) as NAMENODE;

                nameNode.Name = NameManager.KeywordName(CurrentTokenID());
                nameNode.PossibleGenericName = null;
                NextToken();
                return nameNode;
            }
        }

        //------------------------------------------------------------
        // CParser.ParseDottedName
        //
        /// <summary>
        /// <para>Parse a sequence of identities delimited by "." or "::" and create a node list.</para>
        /// <para>This method cannot parse type parameters.</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="allowQualifier">true if "::" is available.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseDottedName(BASENODE parent, bool allowQualifier)
        {
            // Create a NAMENODE instance and name it the name of the identifier.
            // If the current token is not identifier, create a NAMENODE with NAME_MISSING flag.
            BASENODE currentListNode = ParseIdentifier(parent);

            if ((currentListNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
            {
                return currentListNode;
            }

            // If the current token is alias, set Kind and Flags.
            if (allowQualifier)
            {
                CheckForAlias(currentListNode);
            }

            while (CurrentTokenID() == TOKENID.DOT || CurrentTokenID() == TOKENID.COLONCOLON)
            {
                int dotTokenIndex = CurrentTokenIndex();
                if (!allowQualifier)
                {
                    // If specified names are not available and if the token is not DOT, 
                    CheckToken(TOKENID.DOT);
                }
                NextToken();

                // Create a NAMENODE instance with the name following the dot.
                NAMENODE nameNode = ParseIdentifier(parent);

                // Create a BINOPNODE instance whose operands are currentListNode and nameNode,
                // and assign its refernece to currentListNode.
                currentListNode = AllocDotNode(dotTokenIndex, parent, currentListNode, nameNode);

                if ((nameNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                {
                    return currentListNode;
                }
                allowQualifier = false;
            }
            DebugUtil.Assert(currentListNode.ParentNode == parent);
            return currentListNode;
        }

        //------------------------------------------------------------
        // CParser.ParseMethodName
        //
        /// <summary>
        /// Create a method name.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseMethodName(BASENODE parentNode)
        {
            // We let the binding code check for invalid type variable names and for
            // attributes in invalid locations.
            BASENODE node = ParseGenericQualifiedNamePart(parentNode, false, true);

            if ((node.Flags & NODEFLAGS.NAME_MISSING) != 0)
            {
                return node;
            }

            int dotTokenIndex = CurrentTokenIndex();

            CheckForAlias(node);

            BASENODE lastNode = node;
            while (CurrentTokenID() == TOKENID.DOT || CurrentTokenID() == TOKENID.COLONCOLON)
            {
                dotTokenIndex = CurrentTokenIndex();
                if (node.Kind != NODEKIND.ALIASNAME) CheckToken(TOKENID.DOT);
                NextToken();
                lastNode = ParseGenericQualifiedNamePart(parentNode, false, true);
                node = AllocDotNode(dotTokenIndex, parentNode, node, lastNode);
            }

            if (node.IsDoubleColon)
            {
                ErrorAtToken(dotTokenIndex, CSCERRID.ERR_AliasQualAsExpression);
            }
            return node;
        }

        //------------------------------------------------------------
        // CParser.ParseQualifiedName
        //------------------------------------------------------------
        //BASENODE *ParseQualifiedName(BASENODE* pParent, bool fInExpr);

        //------------------------------------------------------------
        // CParser.ParseGenericQualifiedNamePart
        //
        /// <summary>
        /// <para>Create a node representing a simple name
        /// which consists of one identifier and optional generic specification.</para>
        /// <para>This method returns one of</para>
        /// <list type="bullet">
        /// <item>
        /// <term>GENERICNAMENODE</term>
        /// <description>If there is type parameters of generic.</description>
        /// </item>
        /// <item>
        /// <term>NAMENODE</term>
        /// <description>
        /// <para>Not generic.</para>
        /// <para>If it is possible that there are type parameters,
        /// set the name to PossibleGenericName field.</para>
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="inExpre">Set true if parsing expression.</param>
        /// <param name="isTypeVarList">Set true if has type parameters.</param>
        /// <param name="parentNode"></param>
        /// <returns>NAMENODE or GENERICNAMENODE.</returns>
        //------------------------------------------------------------
        internal NAMENODE ParseGenericQualifiedNamePart(
            BASENODE parentNode,
            bool inExpre,
            bool isTypeVarList)
        {
            DebugUtil.Assert(!(inExpre && isTypeVarList));

            if (!CheckToken(TOKENID.IDENTIFIER))
            {
                return ParseMissingName(parentNode, CurrentTokenIndex());
            }

            // Determine if generic.

            GENERICNAMENODE genericNameNode = null;
            bool definitelyGeneric;
            bool possibleGeneric;
            CheckIfGeneric(inExpre, out definitelyGeneric, out possibleGeneric);

            //--------------------------------------------------
            // Surely generic.
            //--------------------------------------------------
            if (definitelyGeneric)
            {
                genericNameNode = AllocGenericNameNode(null, CurrentTokenIndex());
                genericNameNode.ParentNode = parentNode;
                NextToken();

                genericNameNode.OpenAngleIndex = CurrentTokenIndex();
                genericNameNode.ParametersNode = ParseInstantiation(
                    genericNameNode,
                    isTypeVarList,
                    ref genericNameNode.CloseAngleIndex);

                return genericNameNode;
            }

            //--------------------------------------------------
            // Maybe generic or not generic.
            //--------------------------------------------------
            NAMENODE nameNode = AllocNameNode(null, CurrentTokenIndex());
            nameNode.ParentNode = parentNode;

            if (possibleGeneric && this.SupportsErrorSuppression())
            {
                int startIndex = CurrentTokenIndex();

                CErrorSuppression es = this.GetErrorSuppression();
                {
                    genericNameNode = AllocGenericNameNode(null, CurrentTokenIndex());
                    genericNameNode.ParentNode = nameNode;
                    NextToken();
                    DebugUtil.Assert(CurrentTokenID() == TOKENID.OPENANGLE);

                    genericNameNode.OpenAngleIndex = CurrentTokenIndex();
                    genericNameNode.ParametersNode = ParseInstantiation(
                        genericNameNode,
                        isTypeVarList,
                        ref genericNameNode.CloseAngleIndex);
                }
                Rewind(startIndex);
            }

            nameNode.PossibleGenericName = genericNameNode;
            NextToken();
            return nameNode;
        }

        //------------------------------------------------------------
        // CParser.ParseGenericQualifiedNameList
        //
        /// <summary>
        /// 識別子（generic 可）の列からノード列を作成する。
        /// 最後の名前に対応するノードを返す。
        /// </summary>
        /// <param name="inExpr">式中の場合は true を指定する。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>最後の名前に対応するノード。</returns>
        /// <remarks>
        /// ParseDottedName は
        /// ・generic が付いていてはならない。
        /// ・引数 allowQualifier が false なら :: は使用できないとする。
        /// の点がこの関数とは異なる。
        /// </remarks>
        //------------------------------------------------------------
        internal BASENODE ParseGenericQualifiedNameList(BASENODE parentNode, bool inExpr)
        {
            // get one part in form of "identifier ['<' parameter-list '>']".
            BASENODE nameNode = ParseGenericQualifiedNamePart(parentNode, inExpr, false);
            if ((nameNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
            {
                return nameNode;
            }

            int dotIndex = CurrentTokenIndex();

            // if the current token is "::", nameNode is of an alias.
            CheckForAlias(nameNode);

            while (CurrentTokenID() == TOKENID.DOT || CurrentTokenID() == TOKENID.COLONCOLON)
            {
                dotIndex = CurrentTokenIndex();
                if (nameNode.Kind != NODEKIND.ALIASNAME)
                {
                    CheckToken(TOKENID.DOT);
                }
                NextToken();

                // ここで得られた名前（の二分木）を Operand1、
                // 以降の部分名を Operand2 とする。
                nameNode = AllocDotNode(
                    dotIndex,
                    parentNode,
                    nameNode,
                    ParseGenericQualifiedNamePart(parentNode, inExpr, false));
            }

            if (inExpr && nameNode.IsDoubleColon)
            {
                ErrorAtToken(dotIndex, CSCERRID.ERR_AliasQualAsExpression);
            }
            return nameNode;
        }

        //------------------------------------------------------------
        // CParser.ParseMissingName
        //
        /// <summary>
        /// <para>Create a NAMENODE instance which has no name.
        /// Its index is of the nearest valid token before the currrent token.</para>
        /// <para>Set "?" to its name and set NAME_MISSING flag.</para>
        /// </summary>
        //------------------------------------------------------------
        internal NAMENODE ParseMissingName(BASENODE parentNode, int tokenIndex)
        {
            NAMENODE name = AllocNode(
                NODEKIND.NAME,
                parentNode,
                PeekTokenIndexFrom(tokenIndex, -1)) as NAMENODE;

            name.Name = SpecName(SPECIALNAME.MISSING);
            name.PossibleGenericName = null;
            name.Flags |= NODEFLAGS.NAME_MISSING;
            return name;
        }

        //------------------------------------------------------------
        // CParser.ParseMissingType
        //
        /// <summary>
        /// Create a CLASSNODE instance representing an undefined type.
        /// Set "?" to its name.</summary>
        //------------------------------------------------------------
        internal BASENODE ParseMissingType(
            BASENODE parentNode,
            int tokenIndex,
            int startIndex,
            NODEFLAGS modifiers,
            BASENODE attrNode)
        {
            CLASSNODE aggregateNode = AllocNode(NODEKIND.CLASS, parentNode, tokenIndex) as CLASSNODE;

            aggregateNode.StartTokenIndex = startIndex;
            aggregateNode.Flags = modifiers;
            aggregateNode.AttributesNode = attrNode;
            aggregateNode.Key = null;
            aggregateNode.NameNode = ParseMissingName(aggregateNode, startIndex);
            aggregateNode.TypeParametersNode = null;
            aggregateNode.BasesNode = null;
            aggregateNode.ConstraintsNode = null;
            aggregateNode.MembersNode = null;

            //These is the best value I can come up with. It's necessary to set them so that 
            //GetExtent doesn't fail when you ask for the end position.

            aggregateNode.OpenCurlyIndex = CurrentTokenIndex();
            aggregateNode.CloseCurlyIndex = CurrentTokenIndex();

            attrNode.ParentNode = aggregateNode;

            AddToNodeTable(aggregateNode);
            return aggregateNode;
        }

        //------------------------------------------------------------
        // CParser.ParseIndexerName
        //
        /// <summary>インデクサの名前の部分を取得しノードを作成する。</summary>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>名前から作成したノード。</returns>
        //
        // indexer-declarator:
        //     type "this" "[" formal-parameter-list "]"
        //     type interface-type "." "this" "[" formal-parameter-list "]"
        //------------------------------------------------------------
        internal BASENODE ParseIndexerName(BASENODE parentNode)
        {
            if (CurrentTokenID() == TOKENID.THIS)
            {
                NextToken();
                return null;
            }

            BASENODE node = ParseGenericQualifiedNamePart(parentNode, false, false);

            if (PeekToken(2) != TOKENID.THIS)
            {
                CheckForAlias(node);
            }

            while (CurrentTokenID() == TOKENID.DOT || CurrentTokenID() == TOKENID.COLONCOLON)
            {
                int itokDot = CurrentTokenIndex();
                if (node.Kind != NODEKIND.ALIASNAME)
                {
                    CheckToken(TOKENID.DOT);
                }
                NextToken();
                if (CurrentTokenID() == TOKENID.THIS)
                {
                    NextToken();
                    return node;
                }
                node = AllocDotNode(
                    itokDot,
                    parentNode,
                    node,
                    ParseGenericQualifiedNamePart(parentNode, false, false));
            }
            //VSFAIL("Why didn't we find 'this'?");
            return node;
        }

        //------------------------------------------------------------
        // CParser.ParseModifiers
        //
        /// <summary>
        /// Convert literals of modifiers to bit flags of NODEFLAGS.MOD_*.
        /// </summary>
        /// <param name="reportErrors">If true, show error messages.</param>
        /// <returns>Bit flags of type NODEFLAGS.</returns>
        //------------------------------------------------------------
        internal NODEFLAGS ParseModifiers(bool reportErrors)
        {
            NODEFLAGS mods = 0, newMod;
            bool noDups = true, noDupAccess = true;

            while (true)
            {
                TOKENID tokenId = CurrentTokenID();

                switch (tokenId)
                {
                    case TOKENID.PRIVATE:
                        newMod = NODEFLAGS.MOD_PRIVATE;
                        break;

                    case TOKENID.PROTECTED:
                        newMod = NODEFLAGS.MOD_PROTECTED;
                        break;

                    case TOKENID.INTERNAL:
                        newMod = NODEFLAGS.MOD_INTERNAL;
                        break;

                    case TOKENID.PUBLIC:
                        newMod = NODEFLAGS.MOD_PUBLIC;
                        break;

                    case TOKENID.SEALED:
                        newMod = NODEFLAGS.MOD_SEALED;
                        break;

                    case TOKENID.ABSTRACT:
                        newMod = NODEFLAGS.MOD_ABSTRACT;
                        break;

                    case TOKENID.STATIC:
                        newMod = NODEFLAGS.MOD_STATIC;
                        break;

                    case TOKENID.VIRTUAL:
                        newMod = NODEFLAGS.MOD_VIRTUAL;
                        break;

                    case TOKENID.EXTERN:
                        newMod = NODEFLAGS.MOD_EXTERN;
                        break;

                    case TOKENID.NEW:
                        newMod = NODEFLAGS.MOD_NEW;
                        break;

                    case TOKENID.OVERRIDE:
                        newMod = NODEFLAGS.MOD_OVERRIDE;
                        break;

                    case TOKENID.READONLY:
                        newMod = NODEFLAGS.MOD_READONLY;
                        break;

                    case TOKENID.VOLATILE:
                        newMod = NODEFLAGS.MOD_VOLATILE;
                        break;

                    case TOKENID.UNSAFE:
                        newMod = NODEFLAGS.MOD_UNSAFE;
                        break;

                    case TOKENID.IDENTIFIER:

                        // We can regard identifier "partial" as modifier at this position.
                        // If we have other identifier, return mods and exit this method.

                        if (tokenArray[CurrentTokenIndex()].Name != SpecName(SPECIALNAME.PARTIAL))
                        {
                            return mods;
                        }

                        // "partial" can be just before "class" or "struct" or "interface".
                        // (CS3) before "void".
                        switch (PeekToken())
                        {
                            case TOKENID.CLASS:
                                break;

                            case TOKENID.STRUCT:
                                break;

                            case TOKENID.INTERFACE:
                                break;

                            case TOKENID.VOID:  // (CS3)
                                break;

                            default:
                                // (1) If "partial" is at the position of modifiers, we have an error.
                                // (2) Otherwise, regard "partial" as an identifier, and return mods.
                                //     mods has the modifiers before "partial".

                                if ((TokenInfoArray[(int)PeekToken()].Flags &
                                    (TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MODIFIER)) == 0)
                                {
                                    return mods;
                                }
                                if (reportErrors)
                                {
                                    Error(CSCERRID.ERR_PartialMisplaced);
                                }
                                break;
                        }

                        newMod = NODEFLAGS.MOD_PARTIAL;
                        ReportFeatureUse("CSCSTR_FeaturePartialTypes");
                        break;

                    default:
                        return mods;
                }

                if ((mods & newMod) != 0 && reportErrors)
                {
                    // If modifiers duplicate
                    if (noDups && tokenId != TOKENID.IDENTIFIER)
                    {
                        Error(
                            CSCERRID.ERR_DuplicateModifier,
                            new ErrArg(GetTokenInfo(CurrentTokenID()).Text));
                        noDups = false;
                    }
                }
                else
                {
                    if ((mods & NODEFLAGS.MOD_ACCESSMODIFIERS) != 0 &&
                        (newMod & NODEFLAGS.MOD_ACCESSMODIFIERS) != 0)
                    {
                        // In accesibility modifiers, only "protected" and "internal" can be together.

                        if (!(((newMod == NODEFLAGS.MOD_PROTECTED) &&
                            (mods & NODEFLAGS.MOD_INTERNAL) != 0) ||
                            ((newMod == NODEFLAGS.MOD_INTERNAL) &&
                            (mods & NODEFLAGS.MOD_PROTECTED) != 0)))
                        {
                            if (reportErrors)
                            {
                                if (noDupAccess)
                                    Error(CSCERRID.ERR_BadMemberProtection);
                                noDupAccess = false;
                            }
                            newMod = 0;
                        }
                    }
                }
                mods |= newMod;
                NextToken();
            }
        }

        //------------------------------------------------------------
        // CParser.CheckForAlias
        //
        /// <summary>
        /// If the argument node is a NAMENODE instance and if the current token is COLONCOLON,
        /// set ALIASNAME to Kind.
        /// Furthermore if its name is "global", set GLOBAL_QUALIFIER flag.
        /// </summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void CheckForAlias(BASENODE node)
        {
            if (CurrentTokenID() == TOKENID.COLONCOLON && node.Kind == NODEKIND.NAME)
            {
                ReportFeatureUse("CSCSTR_FeatureGlobalNamespace");

                if ((node as NAMENODE).Name == SpecName(SPECIALNAME.GLOBAL))
                {
                    node.Flags |= NODEFLAGS.GLOBAL_QUALIFIER;
                }
                node.Kind = NODEKIND.ALIASNAME;
            }
        }

        //------------------------------------------------------------
        // enum kppo
        //------------------------------------------------------------
        internal enum kppo : int
        {
            AllowAll = 0x00,
            Square = 0x01,  // Square brackets
            NoNames = 0x02,
            NoParams = 0x04,
            NoAttrs = 0x08,
            NoVarargs = 0x10,
            NoRefOrOut = 0x20,
        }

        //------------------------------------------------------------
        // CParser.ParseParameter
        //
        /// <summary>
        /// <para>Parse one parameter and create a PARAMETERNODE instance.</para>
        /// <para>If invalid, return null.</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        BASENODE ParseParameter(BASENODE parent, int flags)
        {
            PARAMETERNODE paramNode = AllocNode(NODEKIND.PARAMETER, parent) as PARAMETERNODE;

            // Parse attributes.
            if ((flags & (int)kppo.NoAttrs) != 0)
            {
                paramNode.AttributesNode = null;
            }
            else
            {
                paramNode.AttributesNode
                    = ParseAttributes(paramNode, ATTRTARGET.PARAMETER, ATTRTARGET.PARAMETER);
            }

            // If "__arglist", set NODEFLAGS.EX_METHOD_VARARGS flag to the parent node and return null.

            if (CurrentTokenID() == TOKENID.ARGS)
            {
                if (parent == null || ((flags & (int)kppo.NoVarargs) != 0))
                {
                    Error(CSCERRID.ERR_IllegalVarArgs);
                }
                else
                {
                    parent.NodeFlagsEx |= NODEFLAGS.EX_METHOD_VARARGS;
                }
                NextToken();
                return null;
            }

            // If "params", set NODEFLAGS.EX_METHOD_PARAMS flag to the parent node

            if (CurrentTokenID() == TOKENID.PARAMS)
            {
                if (parent == null || ((flags & (int)kppo.NoParams) != 0))
                {
                    Error(CSCERRID.ERR_IllegalParams);
                }
                else
                {
                    parent.NodeFlagsEx |= NODEFLAGS.EX_METHOD_PARAMS;
                }
                NextToken();
            }

            // If "ref" or "out", set NODEFLAGS.PARMMOD_REF or NODEFLAGS.PARMMOD_OUT flag.

            if (CurrentTokenID() == TOKENID.REF)
            {
                if ((flags & (int)kppo.NoRefOrOut) != 0)
                {
                    Error(CSCERRID.ERR_IllegalRefParam);
                }
                else
                {
                    paramNode.Flags = NODEFLAGS.PARMMOD_REF;
                }
                NextToken();
            }
            else if (CurrentTokenID() == TOKENID.OUT)
            {
                if ((flags & (int)kppo.NoRefOrOut) != 0)
                {
                    Error(CSCERRID.ERR_IllegalRefParam);
                }
                else
                {
                    paramNode.Flags = NODEFLAGS.PARMMOD_OUT;
                }
                NextToken();
            }

            paramNode.TypeNode = ParseType(paramNode, false);

            // If "void)", reutrn null.

            if ((paramNode.TypeNode.Kind == NODEKIND.PREDEFINEDTYPE) &&
                ((paramNode.TypeNode as PREDEFINEDTYPENODE).Type == PREDEFTYPE.VOID) &&
                CurrentTokenID() == TOKENID.CLOSEPAREN)
            {
                return null;
            }

            // Get a name and set it to NameNode of paramNode.

            if ((flags & (int)kppo.NoNames) == 0)
            {
                paramNode.NameNode = ParseIdentifier(paramNode);

                if (CurrentTokenID() == TOKENID.OPENSQUARE && PeekToken() == TOKENID.CLOSESQUARE)
                {
                    ErrorAtToken(CurrentTokenIndex(), CSCERRID.ERR_BadArraySyntax);
                    NextToken();
                    NextToken();
                }
            }

            // 既定値が指定されている場合は、エラーメッセージを表示して読み飛ばす。
            // （パラメータの既定値は C# 4 から。）

            if (CurrentTokenID() == TOKENID.EQUAL)
            {
                Error(CSCERRID.ERR_NoDefaultArgs);
                NextToken();
                ParseExpression(parent, -1);
            }

            return paramNode;
        }

        //------------------------------------------------------------
        // CParser.ParseParameterList
        //
        /// <summary>
        /// <para>Parse parameters between "(" and ")" or between "[" and "]".</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="paramsNode">(out) The first PRAMETERNODE instance.</param>
        /// <param name="openIndex">(out)</param>
        /// <param name="closeIndex">(out)</param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal void ParseParameterList(
            BASENODE parent,
            out BASENODE paramsNode,
            out int openIndex,
            out int closeIndex,
            out bool isExtensionMethod,
            int flags)
        {
            openIndex = CurrentTokenIndex();
            closeIndex = -1;
            isExtensionMethod = false;
            bool isMethod = (parent.Kind == NODEKIND.METHOD);

            Eat((flags & (int)kppo.Square) != 0 ? TOKENID.OPENSQUARE : TOKENID.OPENPAREN);

            CListMaker list = new CListMaker(this);
            int comma = -1;

            // If neither "()" nor "[]", parse parameters.

            if (CurrentTokenID() !=
                ((flags & (int)kppo.Square) != 0 ? TOKENID.CLOSESQUARE : TOKENID.CLOSEPAREN))
            {
                int itokParams = -1;
                bool fParams = false;

                // (CS3) Extension methods
                if (isMethod && CurrentTokenID() == TOKENID.THIS)
                {
                    isExtensionMethod = true;
                    NextToken();
                }

                while (true)
                {
                    int itok = CurrentTokenIndex();

                    // Parse a parameter

                    // (CS3) 'this' modifier must be on the first parameter.
                    if (isMethod && CurrentTokenID() == TOKENID.THIS)
                    {
                        NAMENODE nNode = (parent as METHODNODE).NameNode as NAMENODE;
                        ErrorAtToken(
                            itok,
                            CSCERRID.ERR_ThisModifierNotOnFirstParam,
                            new ErrArg((nNode != null && nNode.Name != null) ? nNode.Name : "?"));
                        NextToken();
                    }

                    list.Add(ParseParameter(parent, flags), comma);

                    if (CurrentTokenID() != TOKENID.COMMA)
                    {
                        break;
                    }

                    // Remember __arglist / params location for error reporting
                    if ((parent.NodeFlagsEx &
                        (NODEFLAGS.EX_METHOD_VARARGS | NODEFLAGS.EX_METHOD_PARAMS)) != 0
                        && itokParams < 0)
                    {
                        itokParams = itok;
                        fParams = ((parent.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0);
                    }
                    comma = CurrentTokenIndex();
                    NextToken();
                }

                if (itokParams >= 0)
                {
                    ErrorAtToken(
                        itokParams,
                        fParams ? CSCERRID.ERR_ParamsLast : CSCERRID.ERR_VarargsLast);
                    // Clear the bits so downstream code isn't confused.
                    parent.NodeFlagsEx &=
                        unchecked((~(NODEFLAGS.EX_METHOD_VARARGS | NODEFLAGS.EX_METHOD_PARAMS)));
                    // EX_METHOD_VARARGS = 0x10 and EX_METHOD_PARAMS = 0x20, so no problem.

                }
            }

            paramsNode = list.GetList(parent);
            closeIndex = CurrentTokenIndex();
            Eat((flags & (int)kppo.Square) != 0 ? TOKENID.CLOSESQUARE : TOKENID.CLOSEPAREN);
        }

        //------------------------------------------------------------
        // CParser.ParseAttributeSection
        //
        /// <summary>attribute-section を解析して ATTRDECLNODE を作成する。</summary>
        /// <param name="attributeHadError">エラーがあった場合は true がセットされる。</param>
        /// <param name="defaultTarget">属性を指定する対象の既定値。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <param name="validTargets">属性の対象として有効なもの（ビットフラグ）。</param>
        /// <returns>attribute-section から作成した ATTRDECLNODE。</returns>
        //
        // global-attribute-section:
        //     "["  global-attribute-target-specifier  attribute-list  "]"
        //     "["  global-attribute-target-specifier  attribute-list  ","  "]"
        //
        // global-attribute-target-specifier:
        //     global-attribute-target  ":"
        //
        // global-attribute-target:
        //     "assembly"
        //     "module"
        //
        // attribute-section:
        //     "["  [attribute-target-specifier]  attribute-list  "]"
        //     "["  [attribute-target-specifier]  attribute-list  ","  "]"
        //
        // attribute-target-specifier:
        //     attribute-target  ":"
        //
        // attribute-target:
        //     "field"
        //     "event"
        //     "method"
        //     "param"
        //     "property"
        //     "return"
        //     "type"
        //
        // attribute-list:
        //     attribute
        //     attribute-list  ","  attribute
        //
        // attribute:
        //     attribute-name  [attribute-arguments]
        //
        // attribute-name:
        //     type-name
        //
        // attribute-arguments:
        //     "("  [positional-argument-list]  ")"
        //     "("  positional-argument-list  ","  named-argument-list  ")"
        //     "("  named-argument-list  ")"
        //
        // positional-argument-list:
        //     positional-argument
        //     positional-argument-list  ","  positional-argument
        //
        // positional-argument:
        //     [argument-name]  attribute-argument-expression
        //
        // named-argument-list:
        //     named-argument
        //     named-argument-list  ","  named-argument
        //
        // named-argument:
        //     identifier  "="  attribute-argument-expression
        //
        // attribute-argument-expression:
        //     expression
        //------------------------------------------------------------
        internal BASENODE ParseAttributeSection(
            BASENODE parentNode,
            ATTRTARGET defaultTarget,
            ATTRTARGET validTargets,
            out bool attributeHadError)
        {
            attributeHadError = false;
            if (CurrentTokenID() != TOKENID.OPENSQUARE)
            {
                return null;
            }

            ATTRDECLNODE attrDeclNode = AllocNode(NODEKIND.ATTRDECL, parentNode) as ATTRDECLNODE;

            attrDeclNode.Target = ATTRTARGET.NONE;  // (ATTRTARGET) 0;
            attrDeclNode.NameNode = null;

            NextToken();    //Eat(TOKENID.OPENSQUARE); // チェック済み、必要ない。

            //--------------------------------------------------
            // attribute-target ":"
            //--------------------------------------------------
            if (CurrentTokenID() <= TOKENID.IDENTIFIER && PeekToken() == TOKENID.COLON)
            {
                if (CurrentTokenID() == TOKENID.IDENTIFIER &&
                    tokenArray[CurrentTokenIndex()].Name ==
                    NameManager.GetPredefinedName(PREDEFNAME.MODULE))
                {
                    ReportFeatureUse("CSCSTR_FeatureModuleAttrLoc");
                }

                attrDeclNode.NameNode = ParseIdentifierOrKeyword(attrDeclNode);
                Eat(TOKENID.COLON);

                if (!NameManager.IsAttributeTarget(
                    attrDeclNode.NameNode.Name, out attrDeclNode.Target))
                {
                    // 対象名が間違っている場合はエラーメッセージを表示する。
                    ErrorAtToken(
                        attrDeclNode.NameNode.TokenIndex,
                        CSCERRID.WRN_InvalidAttributeLocation,
                        new ErrArg(attrDeclNode.NameNode.Name));
                    attrDeclNode.Target = ATTRTARGET.UNKNOWN;
                }
                else if (validTargets != 0)
                {
                    // 対象が不適切な場合はエラーメッセージを表示する。
                    // ただし、対象が指定されていない場合は既定のものを設定する。
                    SetDefaultAttributeTarget(attrDeclNode, defaultTarget, validTargets);
                }
            }

            // 対象が指定されていない場合は既定のものとする。
            if (attrDeclNode.Target == 0)
            {
                attrDeclNode.NameNode = null;
                attrDeclNode.Target = defaultTarget;
            }

            //--------------------------------------------------
            // attribute-list
            //--------------------------------------------------
            CListMaker list = new CListMaker(this);
            int comma = -1;
            while (true)
            {
                list.Add(ParseAttribute(attrDeclNode, attributeHadError), comma);
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
                if (CurrentTokenID() == TOKENID.CLOSESQUARE)
                {
                    break;
                }
            }
            attrDeclNode.AttributesNode = list.GetList(attrDeclNode);

            attrDeclNode.CloseSquareIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSESQUARE);
            return attrDeclNode;
        }

        //------------------------------------------------------------
        // CParser.ParseAttributes  (1)
        //
        /// <summary>
        /// Parse blocks of attributes and create lists of ATTRDECLNODE instances.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="defaultLocation">Default target of attributes</param>
        /// <param name="validLocations">NODEFLAGS bit flags which represent valid targets.</param>
        /// <returns>The first ATTRDECLENODE of the attribute list.</returns>
        //------------------------------------------------------------
        internal BASENODE ParseAttributes(
            BASENODE parentNode,
            ATTRTARGET defaultLocation,
            ATTRTARGET validLocations)
        {
            bool attributeHadErrorThrowaway;
            return ParseAttributes(
                parentNode,
                defaultLocation,
                validLocations,
                out attributeHadErrorThrowaway);
        }

        //------------------------------------------------------------
        // CParser.ParseAttributes  (2)
        //
        /// <summary>
        /// Parse blocks of attributes and create lists of ATTRDECLNODE instances.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="defaultLocation">Default target of attributes</param>
        /// <param name="validLocations">NODEFLAGS bit flags which represent valid targets.</param>
        /// <param name="attributeHadError"></param>
        /// <returns>The first ATTRDECLENODE of the attribute list.</returns>
        //
        // attributes:
        //     attribute-sections
        //
        // attribute-sections:
        //     attribute-section
        //     attribute-sections  attribute-section
        //------------------------------------------------------------
        internal BASENODE ParseAttributes(
            BASENODE parentNode,
            ATTRTARGET defaultLocation,
            ATTRTARGET validLocations,
            out bool attributeHadError)
        {
            attributeHadError = false;

            if (CurrentTokenID() != TOKENID.OPENSQUARE)
            {
                return null;
            }

            CListMaker list = new CListMaker(this);
            BASENODE sectionNode;

            while (
                null != (sectionNode = ParseAttributeSection(
                parentNode, defaultLocation, validLocations, out attributeHadError)))
            {
                list.Add(sectionNode, -1);
            }
            return list.GetList(parentNode);
        }

        //------------------------------------------------------------
        // CParser.ParseAttribute
        //
        /// <summary>属性を表す ATTRNODE インスタンスを作成する。</summary>
        /// <param name="attributeHadError">エラーがあったら true がセットされる。</param>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>作成した ATTRNODE インスタンス。</returns>
        //
        // attribute:
        //     attribute-name  [attribute-arguments]
        //
        // attribute-name:
        //     type-name
        //
        // attribute-arguments:
        //     "("  [positional-argument-list]  ")"
        //     "("  positional-argument-list  ","  named-argument-list  ")"
        //     "("  named-argument-list  ")"
        //------------------------------------------------------------
        internal BASENODE ParseAttribute(BASENODE parentNode, bool attributeHadError)
        {
            ATTRNODE attrNode = AllocNode(NODEKIND.ATTR, parentNode) as ATTRNODE;
            //attrNode.OpenParenIndex = attrNode.CloseParenIndex = -1; // 初期値として設定されている。

            int mark = CurrentTokenIndex();

            // 名前を解析する。
            attrNode.NameNode = ParseDottedName(attrNode, true);

            // generic が指定されている場合は名前の解析をやり直す。
            if (CurrentTokenID() == TOKENID.OPENANGLE)
            {
                ErrorAfterPrevToken(CSCERRID.ERR_AttributeCantBeGeneric);
                Rewind(mark);
                ParseGenericQualifiedNameList(attrNode, false);
            }

            CListMaker list = new CListMaker(this);

            // attribute-arguments はオプション。
            if (CurrentTokenID() == TOKENID.OPENPAREN)
            {
                attrNode.OpenParenIndex = CurrentTokenIndex();
                NextToken();

                int iTokOpen = CurrentTokenIndex();
                int iTokIdx = iTokOpen;
                bool fNamed = false;
                int iErrors = parseErrorCount;

                if (CurrentTokenID() != TOKENID.CLOSEPAREN)
                {
                    while (true)
                    {
                        list.Add(ParseAttributeArgument(attrNode, out fNamed), -1);
                        if (CurrentTokenID() != TOKENID.COMMA)
                        {
                            break;
                        }
                        NextToken();
                    }
                }

                attrNode.CloseParenIndex = CurrentTokenIndex();
                Eat(TOKENID.CLOSEPAREN);
                if (iErrors != parseErrorCount)
                {
                    attributeHadError = true;
                    attrNode.Flags |= NODEFLAGS.CALL_HADERROR;
                }
            }
            attrNode.ArgumentsNode = list.GetList(attrNode);
            return attrNode;
        }

        //------------------------------------------------------------
        // CParser.ParseAttributeArgument
        //
        /// <summary>属性指定のパラメータを一つ取得し、ATTRNODE を作成する。</summary>
        /// <param name="named">名前で指定するパラメータの場合に true がセットされる。</param>
        //
        // positional-argument-list:
        //     positional-argument
        //     positional-argument-list  ","  positional-argument
        //
        // positional-argument:
        //     [argument-name]  attribute-argument-expression
        //
        // named-argument-list:
        //     named-argument
        //     named-argument-list  ","  named-argument
        //
        // named-argument:
        //     identifier  "="  attribute-argument-expression
        //
        // attribute-argument-expression:
        //     expression
        //------------------------------------------------------------
        internal BASENODE ParseAttributeArgument(BASENODE parent, out bool named)
        {
            named = false;

            ATTRNODE attrNode = AllocNode(NODEKIND.ATTRARG, parent).AsATTRARG;
            //attrNode.OpenParenIndex = attrNode.CloseParenIndex = -1;  // set by initializer

            if (CurrentTokenID() == TOKENID.IDENTIFIER && PeekToken(1) == TOKENID.EQUAL)
            {
                attrNode.NameNode = ParseIdentifier(attrNode);
                NextToken();    //Eat(TOKENID.EQUAL); // チェック済み。
                named = true;
            }
            else
            {
                if (named == true)
                {
                    Error(CSCERRID.ERR_NamedArgumentExpected);
                }
                attrNode.NameNode = null;
            }

            attrNode.ArgumentsNode = ParseExpression(attrNode, -1);
            return attrNode;
        }

        //------------------------------------------------------------
        // CParser.SetDefaultAttributeTarget
        //
        /// <summary>
        /// Check that the ATTRTARGET flags of all elements of the ATTRDECLNODE list are valid.
        /// Or set the default value to the flags which are not set.
        /// </summary>
        /// <param name="attrNodeList">
        /// The first node of attribute node list. 
        /// </param>
        /// <param name="defaultTarget">
        /// The flag of the default target.
        /// </param>
        /// <param name="validTargets">
        /// Flags of valid targets.
        /// </param>
        /// <returns>Return argument attrNodeList itself.</returns>
        //------------------------------------------------------------
        internal BASENODE SetDefaultAttributeTarget(
            BASENODE attrNodeList,
            ATTRTARGET defaultTarget,
            ATTRTARGET validTargets)
        {
            DebugUtil.Assert(defaultTarget != 0);
            DebugUtil.Assert(validTargets != 0);
            DebugUtil.Assert((defaultTarget & validTargets) != 0);

            ATTRDECLNODE node = (attrNodeList != null ? (attrNodeList as ATTRDECLNODE) : null);
            while (node != null)
            {
                if (node.Target == ATTRTARGET.NONE)
                {
                    node.Target = defaultTarget;
                }
                else if (((validTargets | ATTRTARGET.UNKNOWN) & node.Target) == 0)
                {
                    // If all the flags of node.Target does not match to validTarget, show a error message.
                    // Concatenate the string of the valid targets to show a error message.

                    System.Text.StringBuilder sbValidTargets = new StringBuilder();
                    int validTemp = (int)validTargets;
                    for (int i = 0; validTemp != 0; i += 1, validTemp = (validTemp >> 1))
                    {
                        if ((validTemp & 1) != 0)
                        {
                            string targetStr = NameManager.GetAttributeTarget(i);
                            if (!String.IsNullOrEmpty(targetStr))
                            {
                                if (sbValidTargets.Length > 0)
                                {
                                    sbValidTargets.Append(", ");
                                }
                                sbValidTargets.Append(targetStr);
                            }
                        }
                    }
                    ErrorAtToken(
                        node.NameNode.TokenIndex,
                        CSCERRID.WRN_AttributeLocationOnBadDeclaration,
                        new ErrArg(node.NameNode.Name),
                        new ErrArg(sbValidTargets.ToString()));
                    node.Target = ATTRTARGET.UNKNOWN;
                }
                // Otherwise, node.Target is valid

                node = (node.NextNode != null ? (node.NextNode as ATTRDECLNODE) : null);
            }

            return attrNodeList;
        }

        // For XML or EE member parsing.

        //    METHODNODE * ParseMemberRefSpecial();
        //    void ParseDottedNameSpecial(METHODNODE * meth);
        //    void AddToDottedListSpecial(BASENODE ** pnodeAll, BASENODE * node, int itokDot);
        //    int ParseParamListSpecial(METHODNODE * meth, bool fThis);
        //    void CheckEofSpecial(METHODNODE * meth, bool fThis);
        //    NAMENODE * ParseSingleNameSpecial(BASENODE * nodePar);
        //    BASENODE * ParseOperatorSpecial(METHODNODE * meth);

        //    // Interior node parsing methods
        //#define PARSERDEF(name) STATEMENTNODE   *Parse##name (BASENODE *pParent, int iClose = -1);
        //#include "parsertype.h"

        //------------------------------------------------------------
        // CParser.ParseStatement
        //
        /// <summary>
        /// 1 つの文を解析し、STATEMENTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //
        // statement:
        //     labeled-statement                        ParseLabeledStatement
        //
        //     declaration-statement:
        //         local-variable-declaration  ";"      ParseDeclarationStatement
        //         local-constant-declaration  ";"      ParseConstStatement
        //
        //     embedded-statement:
        //         block                                ParseBlock
        //         empty-statement                      このメソッド内で処理する。
        //         expression-statement                 ParseExpressionStatement
        //
        //         selection-statement:
        //             if-statement                     ParseIfStatement
        //             switch-statement                 ParseSwitchStatement
        //
        //         iteration-statement:
        //             while-statement                  ParseWhileStatement
        //             do-statement                     ParseDoStatement
        //             for-statement                    ParseForStatement
        //             foreach-statement                ParseForEachStatement
        //
        //         jump-statement:
        //             break-statement                  ParseBreakStatement
        //             continue-statement               ParseBreakStatement
        //             goto-statement                   ParseGotoStatement
        //             return-statement                 ParseReturnStatement
        //             throw-statement                  ParseThrowStatement
        //
        //         try-statement                        ParseTryStatement
        //         checked-statement                    CheckedStatement
        //         unchecked-statement                  CheckedStatement
        //         lock-statement                       ParseLockStatement
        //         using-statement                      UsingStatement
        //         yield-statement                      ParseYieldStatement
        //
        //                                              ParseUnsafeStatement
        //                                              ParseFixedStatement
        //------------------------------------------------------------
        internal STATEMENTNODE ParseStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            TOKENID tokenId = CurrentTokenID();
            STATEMENTNODE stmtNode;
            int errorCount = parseErrorCount;

            //--------------------------------------------------
            // 現在のトークンで文の種別が決定するもの。
            //--------------------------------------------------
            if (TokenInfoArray[(int)tokenId].StatementParser != 0)
            {
                switch (TokenInfoArray[(int)tokenId].StatementParser)
                {
                    // なし
                    case PARSERTYPE.Statement:
                        stmtNode = ParseStatement(parentNode, closeIndex);
                        break;

                    // "{"
                    case PARSERTYPE.Block:
                        stmtNode = ParseBlock(parentNode, closeIndex);
                        break;

                    // "break" と "continue"
                    case PARSERTYPE.BreakStatement:
                        stmtNode = ParseBreakStatement(parentNode, closeIndex);
                        break;

                    // "const"
                    case PARSERTYPE.ConstStatement:
                        stmtNode = ParseConstStatement(parentNode, closeIndex);
                        break;

                    // なし
                    // 後にある 識別子 ":" にマッチするので、そこで処理する。
                    case PARSERTYPE.LabeledStatement:
                        stmtNode = ParseLabeledStatement(parentNode, closeIndex);
                        break;

                    // "do"
                    case PARSERTYPE.DoStatement:
                        stmtNode = ParseDoStatement(parentNode, closeIndex);
                        break;

                    // "for"
                    case PARSERTYPE.ForStatement:
                        stmtNode = ParseForStatement(parentNode, closeIndex);
                        break;

                    // "foreach"
                    case PARSERTYPE.ForEachStatement:
                        stmtNode = ParseForEachStatement(parentNode, closeIndex);
                        break;

                    // "goto"
                    case PARSERTYPE.GotoStatement:
                        stmtNode = ParseGotoStatement(parentNode, closeIndex);
                        break;

                    // "if"
                    case PARSERTYPE.IfStatement:
                        stmtNode = ParseIfStatement(parentNode, closeIndex);
                        break;

                    // "return"
                    case PARSERTYPE.ReturnStatement:
                        stmtNode = ParseReturnStatement(parentNode, closeIndex);
                        break;

                    //"switch"
                    case PARSERTYPE.SwitchStatement:
                        stmtNode = ParseSwitchStatement(parentNode, closeIndex);
                        break;

                    // "throw"
                    case PARSERTYPE.ThrowStatement:
                        stmtNode = ParseThrowStatement(parentNode, closeIndex);
                        break;

                    // "catch" と "finally" と "try"
                    case PARSERTYPE.TryStatement:
                        stmtNode = ParseTryStatement(parentNode, closeIndex);
                        break;

                    // "while"
                    case PARSERTYPE.WhileStatement:
                        stmtNode = ParseWhileStatement(parentNode, closeIndex);
                        break;

                    // なし。
                    // yield は return か break の直前にあるときにだけ特別な意味を持つ。
                    // したがって、1 つのトークンだけでは yield 文かどうかわからない。
                    case PARSERTYPE.YieldStatement:
                        stmtNode = ParseYieldStatement(parentNode, closeIndex);
                        break;

                    // なし。
                    case PARSERTYPE.Declaration:
                        stmtNode = ParseDeclaration(parentNode, closeIndex);
                        break;

                    // なし。
                    case PARSERTYPE.DeclarationStatement:
                        stmtNode = ParseDeclarationStatement(parentNode, closeIndex);
                        break;

                    // なし。
                    case PARSERTYPE.ExpressionStatement:
                        stmtNode = ParseExpressionStatement(parentNode, closeIndex);
                        break;

                    //"lock"
                    case PARSERTYPE.LockStatement:
                        stmtNode = ParseLockStatement(parentNode, closeIndex);
                        break;

                    // "fixed"
                    case PARSERTYPE.FixedStatement:
                        stmtNode = ParseFixedStatement(parentNode, closeIndex);
                        break;

                    // "using"
                    case PARSERTYPE.UsingStatement:
                        stmtNode = ParseUsingStatement(parentNode, closeIndex);
                        break;

                    // "checked" と "unchecked"
                    case PARSERTYPE.CheckedStatement:
                        stmtNode = ParseCheckedStatement(parentNode, closeIndex);
                        break;

                    // "unsafe"
                    case PARSERTYPE.UnsafeStatement:
                        stmtNode = ParseUnsafeStatement(parentNode, closeIndex);
                        break;

                    default:
                        //VSFAIL("Unexpected parser type");
                        stmtNode = null;
                        break;
                }

                //{
                //#define PARSERDEF(name) \
                //case EParse##name: \
                //    stmtNode = Parse##name (parentNode, closeIndex); \
                //    break;
                //#include "parsertype.h"
                //default:
                //    //VSFAIL("Unexpected parser type");
                //    stmtNode = null;
                //    break;
                //}
            }
            //--------------------------------------------------
            // 識別子 ":" は labeled-statement として処理する。
            //--------------------------------------------------
            else if (tokenId == TOKENID.IDENTIFIER && PeekToken(1) == TOKENID.COLON)
            {
                stmtNode = ParseLabeledStatement(parentNode, closeIndex);
            }
            //--------------------------------------------------
            // "yield" "return" または "yield" "break" の場合。
            //--------------------------------------------------
            else if (
                CheckForSpecName(SPECIALNAME.YIELD) &&
                (PeekToken(1) == TOKENID.RETURN || PeekToken(1) == TOKENID.BREAK))
            {
                stmtNode = ParseYieldStatement(parentNode, -1);
            }
            //--------------------------------------------------
            // ";" のみ（空文）の場合は
            // Kind が NODEKIND.EMPTYSTMT の STATEMENTNODE を作成する。
            //--------------------------------------------------
            else if (tokenId == TOKENID.SEMICOLON)
            {
                stmtNode = AllocNode(NODEKIND.EMPTYSTMT, parentNode).AsEMPTYSTMT;
                NextToken();
            }
            //--------------------------------------------------
            // ここまでで処理されていないのは
            // ・declaration-statement の local-variable-declaration  ";"
            // ・expression-statement
            //--------------------------------------------------
            else
            {
                int mark = CurrentTokenIndex();
                ScanTypeFlagsEnum st;
                bool wasRewinded = false;

                // "static" を指定できるのは
                // クラス、フィールド、メソッド、プロパティ、演算子、イベント、コンストラクタ。
                // "readonly" と "volatile" はフィールドに指定する。
                // いずれも局所変数には指定できない。これらを読み飛ばして処理を続ける。

                if (CurrentTokenID() == TOKENID.STATIC ||
                    CurrentTokenID() == TOKENID.READONLY ||
                    CurrentTokenID() == TOKENID.VOLATILE)
                {
                    while (
                        CurrentTokenID() == TOKENID.STATIC ||
                        CurrentTokenID() == TOKENID.READONLY ||
                        CurrentTokenID() == TOKENID.VOLATILE)
                    {
                        NextToken();
                    }

                    int newMark = CurrentTokenIndex();
                    st = ScanType();
                    if (IsLocalDeclaration(st))
                    {
                        ErrorAtToken(
                            mark,
                            CSCERRID.ERR_BadMemberFlag,
                            new ErrArg(GetTokenText(mark)));

                        Rewind(mark = newMark);
                        wasRewinded = true;
                        goto PARSE_DECLARATION;
                    }
                    Rewind(mark);
                }

                st = ScanType();

            PARSE_DECLARATION:

                if (IsLocalDeclaration(st))
                {
                    if (!wasRewinded)
                    {
                        Rewind(mark);
                    }
                    stmtNode = ParseDeclarationStatement(parentNode, closeIndex);
                }
                else
                {
                    Rewind(mark);
                    stmtNode = ParseExpressionStatement(parentNode, closeIndex);
                }
            }

            //--------------------------------------------------
            // 終了処理
            //--------------------------------------------------
            stmtNode.NextNode = null;
            if (errorCount != parseErrorCount)
            {
                stmtNode.Flags |= NODEFLAGS.STMT_HADERROR;
            }
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseBlock
        //
        // block:
        //     "{"  [statement-list]  "}"
        //
        // statement-list:
        //     statement
        //     statement-list  statement
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseBlock(BASENODE parentNode, int closeIndex)	// = -1);
        {
            BLOCKNODE blockNode = AllocNode(NODEKIND.BLOCK, parentNode) as BLOCKNODE;

            blockNode.NextNode = null;
            blockNode.StatementsNode = null;

            if (parentNode != null)
            {
                switch (parentNode.Kind)
                {
                    case NODEKIND.ACCESSOR:
                        closeIndex = (parentNode as ACCESSORNODE).CloseCurlyIndex;
                        break;

                    case NODEKIND.METHOD:
                    case NODEKIND.CTOR:
                    case NODEKIND.DTOR:
                    case NODEKIND.OPERATOR:
                        closeIndex = parentNode.AsANYMETHOD.CloseIndex;
                        break;

                    case NODEKIND.INDEXER:
                    case NODEKIND.PROPERTY:
                        closeIndex = parentNode.AsANYPROPERTY.CloseIndex;
                        break;

                    default:
                        closeIndex = -1;
                        break;
                }
            }

            Eat(TOKENID.OPENCURLY);

            //STATEMENTNODE** nextNode = &blockNode.StatementsNode;
            STATEMENTNODE stmtList = null;
            STATEMENTNODE stmtListLast = null;

            while (!PastCloseToken(closeIndex) && CurrentTokenID() != TOKENID.ENDFILE)
            {
                if ((closeIndex == -1 &&
                    (CurrentTokenID() == TOKENID.CLOSECURLY ||
                    CurrentTokenID() == TOKENID.FINALLY ||
                    CurrentTokenID() == TOKENID.CATCH)))
                {
                    break;
                }
                if (CurrentTokenID() == TOKENID.CLOSECURLY)
                {
                    Eat(TOKENID.OPENCURLY);
                    NextToken();
                    continue;
                }

                CEnsureParserProgress epp = new CEnsureParserProgress(this);

                try
                {
                    STATEMENTNODE temp = ParseStatement(blockNode, closeIndex);
                    if (temp != null)
                    {
                        if (stmtList == null)
                        {
                            stmtList = temp;
                            stmtListLast = temp;
                        }
                        else
                        {
                            DebugUtil.Assert(stmtListLast != null);

                            stmtListLast.NextNode = temp;
                            stmtListLast = temp;
                        }
                    }
                    //ASSERT (*nextNode == null);
                }
                finally
                {
                    epp.EnsureProgress();
                }
            }
            blockNode.StatementsNode = stmtList;

            blockNode.CloseCurlyIndex = CurrentTokenIndex();
            if (closeIndex != blockNode.CloseCurlyIndex)
            {
                Eat(TOKENID.CLOSECURLY);
            }
            else if (CurrentTokenID() == TOKENID.CLOSECURLY)
            {
                NextToken();
            }
            else
            {
                DebugUtil.Assert(parseErrorCount > 0);
            }

            return blockNode;
        }

        //------------------------------------------------------------
        // CParser.ParseBreakStatement
        //
        /// <summary>
        /// break 文と continue 文に対応する EXPRSTMTNODE インスタンスを作成する。
        /// </summary>
        //
        // break-statement:
        //     "break" ";"
        //
        // continue-statement:
        //     "continue" ";"
        //
        //------------------------------------------------------------
        internal STATEMENTNODE ParseBreakStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.BREAK || CurrentTokenID() == TOKENID.CONTINUE);

            EXPRSTMTNODE stmtNode = AllocNode(
                CurrentTokenID() == TOKENID.BREAK ? NODEKIND.BREAK : NODEKIND.CONTINUE,
                parentNode).AsANYEXPRSTMT;
            stmtNode.ArgumentsNode = null;
            NextToken();
            Eat(TOKENID.SEMICOLON);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseConstStatement
        //
        // local-constant-declaration:
        //     "const"  type  constant-declarators
        //
        // constant-declarators:
        //     constant-declarator
        //     constant-declarators  ,  constant-declarator
        //
        // constant-declarator:
        //     identifier  "="  constant-expression
        //
        // constant-expression:
        //     expression
        //
        /// <summary>
        /// const 文を解析し DECLSTMTNODE インスタンスを作成する。
        /// const であること示すために Flags に CONST_DECL をセットする。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseConstStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            Eat(TOKENID.CONST);

            DECLSTMTNODE stmtNode = AllocNode(NODEKIND.DECLSTMT, parentNode) as DECLSTMTNODE;
            stmtNode.TypeNode = ParseType(stmtNode, false);
            stmtNode.Flags |= NODEFLAGS.CONST_DECL;

            bool isFirst = true;
            CListMaker list = new CListMaker(this);
            int comma = -1;

            while (true)
            {
                list.Add(
                    ParseVariableDeclarator(
                        stmtNode,
                        stmtNode,
                        (uint)PARSEDECLFLAGS.CONST,
                        isFirst,
                        comma),
                    -1);
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
                isFirst = false;
            }
            Eat(TOKENID.SEMICOLON);
            stmtNode.VariablesNode = list.GetList(stmtNode);
            stmtNode.NextNode = null;
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseLabeledStatement
        //
        // labeled-statement:
        //     identifier  ":"  statement
        //
        /// <summary>
        /// ラベル文を解析し、LABELSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseLabeledStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.IDENTIFIER);

            LABELSTMTNODE stmtNode = AllocNode(NODEKIND.LABEL, parentNode) as LABELSTMTNODE;

            stmtNode.LabelNode = ParseIdentifier(stmtNode);
            Eat(TOKENID.COLON);
            stmtNode.StatementNode = ParseStatement(stmtNode, closeIndex);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseDoStatement
        //
        // do-statement:
        //     "do"  embedded-statement  "while"  "("  boolean-expression  ")"  ";"
        //
        /// <summary>
        /// do 文を解析して LOOPSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseDoStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.DO);

            LOOPSTMTNODE stmtNode = AllocNode(NODEKIND.DO, parentNode).AsDO;
            NextToken();
            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, false);
            Eat(TOKENID.WHILE);
            Eat(TOKENID.OPENPAREN);
            stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
            Eat(TOKENID.CLOSEPAREN);
            Eat(TOKENID.SEMICOLON);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseForStatement
        //
        // for-statement:
        //     "for"  "("  [for-initializer]  ";"  [for-condition]  ";"  [for-iterator]  ")"
        //         embedded-statement
        //
        // for-initializer:
        //     local-variable-declaration
        //     statement-expression-list
        //
        // for-condition:
        //     boolean-expression
        //
        // for-iterator:
        //     statement-expression-list
        //
        // statement-expression-list:
        //     statement-expression
        //     statement-expression-list  ","  statement-expression
        //
        /// <summary>
        /// for 文を解析して FORSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseForStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.FOR);

            FORSTMTNODE stmtNode = AllocNode(NODEKIND.FOR, parentNode) as FORSTMTNODE;
            NextToken();
            Eat(TOKENID.OPENPAREN);

            int iMark = CurrentTokenIndex();

            ScanTypeFlagsEnum st = ScanType();

            if (st != ScanTypeFlagsEnum.NotType && CurrentTokenID() == TOKENID.IDENTIFIER)
            {
                // "(" の次のトークンが型と思われるなら local-variable-declaration と考える。

                Rewind(iMark);
                stmtNode.InitialNode = ParseDeclarationStatement(stmtNode, closeIndex);
            }
            else
            {
                // "(" の次のトークンが型ではないと思われるなら statement-expression-list と考える。

                Rewind(iMark);
                if (CurrentTokenID() == TOKENID.SEMICOLON)
                {
                    stmtNode.InitialNode = null;
                    NextToken();
                }
                else
                {
                    stmtNode.InitialNode = ParseExpressionList(stmtNode);
                    Eat(TOKENID.SEMICOLON);
                }
            }

            // for-condition を解析する。

            stmtNode.ExpressionNode =
                (CurrentTokenID() != TOKENID.SEMICOLON) ? ParseExpression(stmtNode, -1) : null;
            Eat(TOKENID.SEMICOLON);

            // for-iterator を解析する。

            stmtNode.IncrementNode =
                (CurrentTokenID() != TOKENID.CLOSEPAREN) ? ParseExpressionList(stmtNode) : null;

            stmtNode.CloseParenIndex = CurrentTokenIndex();
            stmtNode.InKeyword = -1;
            Eat(TOKENID.CLOSEPAREN);

            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, true);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseForEachStatement
        //
        // foreach-statement:
        //     "foreach"  "("  local-variable-type  identifier  "in"  expression  ")"  embedded-statement
        //
        /// <summary>
        /// foreach 文を解析して FORSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseForEachStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.FOREACH);

            FORSTMTNODE stmtNode = AllocNode(NODEKIND.FOR, parentNode) as FORSTMTNODE;
            stmtNode.Flags = NODEFLAGS.FOR_FOREACH;
            stmtNode.IncrementNode = null;
            stmtNode.InKeyword = -1;
            NextToken();
            Eat(TOKENID.OPENPAREN);


            DECLSTMTNODE declNode = AllocNode(NODEKIND.DECLSTMT, stmtNode) as DECLSTMTNODE;
            declNode.TypeNode = ParseType(declNode, false);
            declNode.NextNode = null;

            if (CurrentTokenID() == TOKENID.IN)
            {
                Error(CSCERRID.ERR_BadForeachDecl);
            }

            VARDECLNODE varDeclNode = AllocNode(NODEKIND.VARDECL, declNode) as VARDECLNODE;
            varDeclNode.ArgumentsNode = null;
            varDeclNode.DeclarationsNode = declNode;
            varDeclNode.NameNode = ParseIdentifier(varDeclNode);
            declNode.VariablesNode = varDeclNode;
            stmtNode.InitialNode = declNode;

            if (CurrentTokenID() != TOKENID.IN)
            {
                Error(CSCERRID.ERR_InExpected);
            }
            else
            {
                stmtNode.InKeyword = CurrentTokenIndex();
                NextToken();
            }

            stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
            stmtNode.CloseParenIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSEPAREN);

            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, true);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseGotoStatement
        //
        /// <summary>
        /// goto 文を解析して EXPRSTMTNODE インスタンスを作成する。
        /// </summary>
        //
        // goto-statement:
        //     "goto"  identifier  ";"
        //     "goto"  "case"  constant-expression  ";"
        //     "goto"  "default"  ";"
        //------------------------------------------------------------
        internal STATEMENTNODE ParseGotoStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.GOTO);

            EXPRSTMTNODE stmtNode = AllocNode(NODEKIND.GOTO, parentNode).AsGOTO;
            NextToken();
            if (CurrentTokenID() == TOKENID.CASE || CurrentTokenID() == TOKENID.DEFAULT)
            {
                // "goto" "case" constant-expression ";"
                // の場合は、Flags に NODEFLAGS.GOTO_CASE、ArgumentsNode に式のノードをセットする。
                // "goto" "default" ";"
                // の場合は、Flags に NODEFLAGS.GOTO_CASE、ArgumentsNode に null をセットする。

                TOKENID tok = CurrentTokenID();

                stmtNode.Flags = NODEFLAGS.GOTO_CASE;
                NextToken();
                stmtNode.ArgumentsNode = (tok == TOKENID.CASE) ? ParseExpression(stmtNode, -1) : null;
            }
            else
            {
                // "goto"  identifier  ";"
                stmtNode.ArgumentsNode = ParseIdentifier(stmtNode);
            }

            Eat(TOKENID.SEMICOLON);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseIfStatement
        //
        // if-statement:
        //     "if"  "("  boolean-expression  ")"  embedded-statement
        //     "if"  "("  boolean-expression  ")"  embedded-statement  "else"  embedded-statement
        //
        /// <summary>
        /// if 文を解析して IFSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseIfStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.IF);

            IFSTMTNODE stmtNode = AllocNode(NODEKIND.IF, parentNode) as IFSTMTNODE;
            NextToken();
            Eat(TOKENID.OPENPAREN);
            stmtNode.ConditionNode = ParseExpression(stmtNode, -1);
            Eat(TOKENID.CLOSEPAREN);
            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, false);
            if (CurrentTokenID() == TOKENID.ELSE)
            {
                NextToken();
                stmtNode.ElseNode = ParseEmbeddedStatement(stmtNode, false);
            }
            else
            {
                stmtNode.ElseNode = null;
            }
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseReturnStatement
        //
        // return-statement:
        //     "return"  [expression]  ";"
        //
        /// <summary>
        /// return 文を解析して EXPRSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseReturnStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.RETURN);

            EXPRSTMTNODE stmtNode = AllocNode(NODEKIND.RETURN, parentNode).AsRETURN;
            NextToken();
            if (CurrentTokenID() == TOKENID.SEMICOLON)
            {
                stmtNode.ArgumentsNode = null;
            }
            else
            {
                stmtNode.ArgumentsNode = ParseExpression(stmtNode, -1);
            }
            Eat(TOKENID.SEMICOLON);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseSwitchStatement
        //
        // switch-statement:
        //     "switch"  "("  expression  ")"  switch-block
        //
        // switch-block:
        //     "{"  [switch-sections]  "}"
        //
        // switch-sections:
        //     switch-section
        //     switch-sections  switch-section
        //
        // switch-section:
        //     switch-labels  statement-list
        //
        // switch-labels:
        //     switch-label
        //     switch-labels  switch-label
        //
        // switch-label:
        //     "case"  constant-expression  ":"
        //     "default"  ":"
        //
        /// <summary>
        /// switch 文を解析して SWITCHSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseSwitchStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.SWITCH);

            SWITCHSTMTNODE stmtNode = AllocNode(NODEKIND.SWITCH, parentNode) as SWITCHSTMTNODE;
            NextToken();
            Eat(TOKENID.OPENPAREN);
            stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
            Eat(TOKENID.CLOSEPAREN);
            stmtNode.OpenCurlyIndex = CurrentTokenIndex();
            Eat(TOKENID.OPENCURLY);

            CListMaker list = new CListMaker(this);

            if (CurrentTokenID() == TOKENID.CLOSECURLY)
            {
                Error(CSCERRID.WRN_EmptySwitch);
            }

            while (AtSwitchCase(closeIndex))
            {
                CEnsureParserProgress epp = new CEnsureParserProgress(this);
                try
                {
                    list.Add(ParseSwitchCase(stmtNode, closeIndex), closeIndex);
                }
                finally
                {
                    epp.EnsureProgress();
                }
            }

            stmtNode.CloseCurlyIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSECURLY);
            stmtNode.CasesNode = list.GetList(stmtNode);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseThrowStatement
        //
        // throw-statement:
        //     "throw"  [expression]  ";"
        //
        /// <summary>
        /// throw 文を解析して EXPRSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseThrowStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.THROW);

            EXPRSTMTNODE stmtNode = AllocNode(NODEKIND.THROW, parentNode).AsTHROW;
            NextToken();
            stmtNode.ArgumentsNode =
                (CurrentTokenID() != TOKENID.SEMICOLON) ? ParseExpression(stmtNode, -1) : null;
            Eat(TOKENID.SEMICOLON);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseTryStatement
        //
        // catch-clauses:
        //     specific-catch-clauses  [general-catch-clause]
        //     [specific-catch-clauses]  general-catch-clause
        //
        // specific-catch-clauses:
        //     specific-catch-clause
        //     specific-catch-clauses  specific-catch-clause
        //
        // specific-catch-clause:
        //     "catch"  "("  class-type  [identifier]  ")"  block
        //
        // general-catch-clause:
        //     "catch"  block
        //
        // finally-clause:
        //     "finally"  block
        //
        /// <summary>
        /// <para>try 文を解析して TRYSTMTNODE インスタンスを作成する。</para>
        /// <para>tyr-catch-finally は try{ try-catch }finally{} として構成する。</para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseTryStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            TRYSTMTNODE stmtNode = AllocNode(NODEKIND.TRY, parentNode) as TRYSTMTNODE;

            switch (CurrentTokenID())
            {
                case TOKENID.TRY:
                    NextToken();
                    stmtNode.BlockNode = ParseBlock(stmtNode, -1) as BLOCKNODE;
                    break;

                case TOKENID.CATCH:
                case TOKENID.FINALLY:
                    Eat(TOKENID.TRY);
                    stmtNode.BlockNode = null;
                    break;

                default:
                    DebugUtil.Assert(false, "CurrentTokenID() not try, catch, or finally");
                    break;
            }

            if (CurrentTokenID() == TOKENID.CATCH)
            {
                bool isEmpty = false;
                BASENODE lastAddedNode = null;
                CListMaker list = new CListMaker(this);

                while (CurrentTokenID() == TOKENID.CATCH)
                {
                    lastAddedNode = ParseCatchClause(stmtNode, ref isEmpty);
                    list.Add(lastAddedNode, -1);
                }

                stmtNode.Flags |= NODEFLAGS.TRY_CATCH;
                stmtNode.CatchNode = list.GetList(stmtNode);

                if (CurrentTokenID() == TOKENID.FINALLY)
                {
                    TRYSTMTNODE innerTryNode = stmtNode;

                    stmtNode = AllocNode(NODEKIND.TRY, parentNode, innerTryNode.TokenIndex) as TRYSTMTNODE;

                    BLOCKNODE blockNode
                        = AllocNode(NODEKIND.BLOCK, stmtNode, stmtNode.TokenIndex) as BLOCKNODE;
                    blockNode.NextNode = null;
                    blockNode.StatementsNode = innerTryNode;
                    blockNode.CloseCurlyIndex = (lastAddedNode as CATCHNODE).BlockNode.CloseCurlyIndex;

                    innerTryNode.ParentNode = blockNode;
                    innerTryNode.NextNode = null;
                    stmtNode.BlockNode = blockNode;

                    NextToken();
                    stmtNode.CatchNode = ParseBlock(stmtNode, -1);
                    stmtNode.Flags |= NODEFLAGS.TRY_FINALLY;
                }
            }
            else if (CurrentTokenID() == TOKENID.FINALLY)
            {
                NextToken();
                stmtNode.CatchNode = ParseBlock(stmtNode, -1);
                stmtNode.Flags |= NODEFLAGS.TRY_FINALLY;
            }
            else
            {
                Error(CSCERRID.ERR_ExpectedEndTry);
            }
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseWhileStatement
        //
        // while-statement:
        //     "while"  "("  boolean-expression  ")"  embedded-statement
        //
        /// <summary></summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseWhileStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.WHILE);

            LOOPSTMTNODE stmtNode = AllocNode(NODEKIND.WHILE, parentNode).AsWHILE;
            NextToken();
            Eat(TOKENID.OPENPAREN);
            stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
            Eat(TOKENID.CLOSEPAREN);
            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, true);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseYieldStatement
        //
        // yield-statement:
        //     "yield"  "return"  expression  ";"
        //     "yield"  "break"  ";"
        //
        /// <summary>
        /// <para>yield 文を解析して EXPRSTMTNODE インスタンスを作成する。</para>
        /// <para>yield return の場合、ArgumentsNode フィールドに expression をセットする。</para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseYieldStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            DebugUtil.Assert(CheckForSpecName(SPECIALNAME.YIELD));

            EXPRSTMTNODE stmtNode = AllocNode(NODEKIND.YIELD, parentNode).AsYIELD;
            stmtNode.ArgumentsNode = null;

            ReportFeatureUse("CSCSTR_FeatureIterators");

            NextToken();
            if (CurrentTokenID() == TOKENID.BREAK)
            {
                Eat(TOKENID.BREAK);
            }
            else
            {
                Eat(TOKENID.RETURN);
                if (CurrentTokenID() == TOKENID.SEMICOLON)
                {
                    Error(CSCERRID.ERR_EmptyYield);
                }
                else
                {
                    stmtNode.ArgumentsNode = ParseExpression(stmtNode, -1);
                }
            }

            Eat(TOKENID.SEMICOLON);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseDeclaration
        //
        // declaration-statement:
        //     local-variable-declaration  ";"
        //     local-constant-declaration  ";"
        //
        // local-variable-declaration:
        //     local-variable-type  local-variable-declarators
        //
        // local-variable-type:
        //     type
        //     "var"
        //
        // local-variable-declarators:
        //     local-variable-declarator
        //     local-variable-declarators  ","  local-variable-declarator
        //
        // local-variable-declarator:
        //     identifier
        //     identifier  "="  local-variable-initializer
        //
        /// <summary>
        /// local-variable-declaration を解析し、DECLSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseDeclaration(BASENODE parentNode, int closeIndex)	// = -1);
        {
            DECLSTMTNODE stmtNode = AllocNode(NODEKIND.DECLSTMT, parentNode) as DECLSTMTNODE;

            // 型を取得する。

            stmtNode.TypeNode = ParseType(stmtNode, false);

            bool isFirst = true;
            CListMaker list = new CListMaker(this);
            int comma = -1;

            // 変数（とその初期値）のリストを作成し stmtNode に設定する。

            while (!PastCloseToken(closeIndex))
            {
                list.Add(
                    ParseVariableDeclarator(
                        stmtNode,
                        stmtNode,
                        (uint)PARSEDECLFLAGS.LOCAL,
                        isFirst,
                        closeIndex),
                    comma);

                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
                NextToken();
                isFirst = false;
            }
            stmtNode.VariablesNode = list.GetList(stmtNode);

            // 変数が宣言されていない場合は、名前なし、初期値 null の変数を設定する。

            if (stmtNode.VariablesNode == null)
            {
                VARDECLNODE varDeclNode = AllocNode(NODEKIND.VARDECL, stmtNode) as VARDECLNODE;
                varDeclNode.DeclarationsNode = stmtNode;
                varDeclNode.NameNode = ParseMissingName(varDeclNode, CurrentTokenIndex());
                varDeclNode.ArgumentsNode = null;

                stmtNode.VariablesNode = varDeclNode;
            }
            stmtNode.NextNode = null;
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseDeclarationStatement
        //
        /// <summary>
        /// <para>局所変数の宣言 local-variable-declaration を解析して
        /// STATEMENTNODE インスタンスを作成する。</para>
        /// <para>実際の処理のほとんどは ParseDeclaration メソッドが行う。
        /// このメソッドでは ";" の有無を調べるだけである。</para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseDeclarationStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            STATEMENTNODE node = ParseDeclaration(parentNode, closeIndex);
            Eat(TOKENID.SEMICOLON);
            return node;
        }

        //------------------------------------------------------------
        // CParser.ParseExpressionStatement
        //
        // expression-statement:
        //     statement-expression  ";"
        //
        /// <summary>
        /// <para>expression-statement を解析して EXPRSTMTNODE を作成する。</para>
        /// <para>実際の処理は ParseExpression メソッドが行う。
        /// このメソッドは ";" を調べるだけである。</para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseExpressionStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            EXPRSTMTNODE stmtNode = AllocNode(NODEKIND.EXPRSTMT, parentNode) as EXPRSTMTNODE;
            stmtNode.ArgumentsNode = ParseExpression(stmtNode, closeIndex);
            Eat(TOKENID.SEMICOLON);
            stmtNode.NextNode = null;
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseLockStatement
        //
        // lock-statement:
        //     "lock"  "("  expression  ")"  embedded-statement
        //
        /// <summary>
        /// lock 文を解析して LOOPSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseLockStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            //ASSERT (CurrentTokenID() == TOKENID.LOCK);

            LOOPSTMTNODE stmtNode = AllocNode(NODEKIND.LOCK, parentNode).AsLOCK;
            NextToken();
            Eat(TOKENID.OPENPAREN);
            stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
            Eat(TOKENID.CLOSEPAREN);
            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, false);
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseFixedStatement
        //
        // fixed-statement:
        //     "fixed"  "("  pointer-type  fixed-pointer-declarators  ")"  embedded-statement
        //
        // fixed-pointer-declarators:
        //     fixed-pointer-declarator
        //     fixed-pointer-declarators  ","  fixed-pointer-declarator
        //
        // fixed-pointer-declarator:
        //     identifier  "="  fixed-pointer-initializer
        //
        // fixed-pointer-initializer:
        //     "&"  variable-reference
        //     expression
        //
        // variable-reference:
        //     expression
        //
        /// <summary>
        /// <para>fixed 文を解析して FORSTMTNODE インスタンスを作成する。</para>
        /// <para>FORSTMTNODE の Flags には NODEFLAGS.FIXED_DECL をセットする。</para>
        /// <para>固定ポインタの宣言の解析結果は InitialNode に設定する。</para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseFixedStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            FORSTMTNODE stmtNode = AllocNode(NODEKIND.FOR, parentNode) as FORSTMTNODE;

            stmtNode.Flags |= NODEFLAGS.FIXED_DECL;
            NextToken();
            Eat(TOKENID.OPENPAREN);

            stmtNode.InitialNode = ParseDeclaration(stmtNode, closeIndex);
            stmtNode.InitialNode.Flags |= NODEFLAGS.FIXED_DECL;

            stmtNode.ExpressionNode = null;
            stmtNode.IncrementNode = null;
            stmtNode.CloseParenIndex = CurrentTokenIndex();
            stmtNode.InKeyword = -1;
            Eat(TOKENID.CLOSEPAREN);

            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, false);
            stmtNode.NextNode = null;

            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseUsingStatement
        //
        // using-statement:
        //     "using"  "("  resource-acquisition  ")"  embedded-statement
        //
        // resource-acquisition:
        //     local-variable-declaration
        //     expression
        //
        /// <summary>
        /// <para>using 文を解析して FORSTMTNODE インスタンスを作成する。</para>
        /// <para>FORSTMTNODE の Flags には NODEFLAGS.USING_DECL をセットする。</para>
        /// <para>
        /// リソース取得の解析結果は InitialNode か ConditionNode に設定する。
        /// </para>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseUsingStatement(BASENODE parentNode, int closeIndex)	// = -1);
        {
            FORSTMTNODE stmtNode = AllocNode(NODEKIND.FOR, parentNode) as FORSTMTNODE;

            stmtNode.Flags |= NODEFLAGS.USING_DECL;
            NextToken();
            Eat(TOKENID.OPENPAREN);


            int mark = CurrentTokenIndex();
            ScanTypeFlagsEnum st = ScanType();

            //--------------------------------------------------
            // (1) 直前のトークンが null 許容型を示している場合。
            //--------------------------------------------------
            if (st == ScanTypeFlagsEnum.NullableType)
            {
                // 現在のトークンが識別子でない場合は expression として最初から処理する。

                if (CurrentTokenID() != TOKENID.IDENTIFIER)
                {
                    //goto LExpr;
                    // LExpr はスコープ外なので直接ここへ記述する。
                    Rewind(mark);
                    stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
                    stmtNode.InitialNode = null;
                    goto COMMON;
                }

                // 現在のトークンが識別子の場合は次のトークンで処理を決める。

                switch (PeekToken())
                {
                    case TOKENID.COMMA:
                    case TOKENID.CLOSEPAREN:
                        // "," か ")" の場合は local-variable-declaration として最初から処理する。

                        //goto LDecl;
                        // LDecl はスコープ外なので直接ここへ記述する。
                        Rewind(mark);
                        stmtNode.InitialNode = ParseDeclaration(stmtNode, -1);
                        stmtNode.InitialNode.Flags |= NODEFLAGS.USING_DECL;
                        stmtNode.ExpressionNode = null;
                        goto COMMON;

                    case TOKENID.EQUAL:
                        // "=" の場合の処理は switch を抜けてから。
                        break;

                    default:
                        // 以上に当てはまらない場合は expression として最初から処理する。

                        //goto LExpr;
                        // LExpr はスコープ外なので直接ここへ記述する。
                        Rewind(mark);
                        stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
                        stmtNode.InitialNode = null;
                        goto COMMON;
                }

                // "=" の場合。

                Rewind(mark);
                DECLSTMTNODE declNode = ParseDeclaration(stmtNode, -1) as DECLSTMTNODE;
                //ASSERT(declNode.pType.kind == NODEKIND.NULLABLETYPE ||
                //       declNode.pType.kind == NODEKIND.POINTERTYPE);

                if (CurrentTokenID() == TOKENID.COLON &&
                    (declNode.TypeNode as NULLABLETYPENODE).ElementTypeNode.Kind == NODEKIND.NAMEDTYPE &&
                    declNode.VariablesNode.Kind == NODEKIND.VARDECL)
                {
                    NAMEDTYPENODE nodeType =
                        (declNode.TypeNode as NULLABLETYPENODE).ElementTypeNode as NAMEDTYPENODE;
                    VARDECLNODE nodeVar = declNode.VariablesNode as VARDECLNODE;
                    //ASSERT(nodeVar.pName && nodeVar.pArg);

                    int itok = CurrentTokenIndex();
                    NextToken();
                    BINOPNODE nodeColon = AllocBinaryOpNode(
                        OPERATOR.COLON, itok, stmtNode, nodeVar.ArgumentsNode,
                        ParseExpression(stmtNode, -1));
                    stmtNode.ExpressionNode = AllocBinaryOpNode(
                        OPERATOR.QUESTION, mark, stmtNode, nodeType.NameNode, nodeColon);
                    stmtNode.InitialNode = null;
                }
                else
                {
                    stmtNode.InitialNode = declNode;
                    stmtNode.InitialNode.Flags |= NODEFLAGS.USING_DECL;
                    stmtNode.ExpressionNode = null;
                }
            }
            //--------------------------------------------------
            // (2) 直前のトークンが null 許容型ではなく、
            //     ・直前のトークンが型で現在のトークンが "." ではない、または、
            //     ・直前のトークンが generic ではない型の可能性があり現在のトークンが識別子、または、
            //     ・直前のトークンが型の可能性があり次のトークンが "="
            //     のいずれかである場合は local-variable-declaration として処理する。
            //--------------------------------------------------
            else if (
                st == ScanTypeFlagsEnum.MustBeType && CurrentTokenID() != TOKENID.DOT
                ||
                st != ScanTypeFlagsEnum.NotType &&
                CurrentTokenID() == TOKENID.IDENTIFIER &&
                (st == ScanTypeFlagsEnum.NonGenericTypeOrExpr || PeekToken() == TOKENID.EQUAL))
            {
                //LDecl:
                Rewind(mark);
                stmtNode.InitialNode = ParseDeclaration(stmtNode, -1);
                stmtNode.InitialNode.Flags |= NODEFLAGS.USING_DECL;
                stmtNode.ExpressionNode = null;
            }
            //--------------------------------------------------
            // (3) 以上に当てはまらない場合は expression として処理する。
            //--------------------------------------------------
            else
            {
                //LExpr:
                Rewind(mark);
                stmtNode.ExpressionNode = ParseExpression(stmtNode, -1);
                stmtNode.InitialNode = null;
            }

            //--------------------------------------------------
            // 共通の処理
            //--------------------------------------------------
        COMMON:
            stmtNode.IncrementNode = null;
            stmtNode.CloseParenIndex = CurrentTokenIndex();
            Eat(TOKENID.CLOSEPAREN);

            stmtNode.StatementNode = ParseEmbeddedStatement(stmtNode, false);
            stmtNode.NextNode = null;
            stmtNode.InKeyword = -1;

            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseCheckedStatement
        //
        // checked-statement:
        //     "checked"  block
        //
        // unchecked-statement:
        //     "unchecked"  block
        //
        /// <summary>
        /// <para>check 文と uncheck 文を解析して LABELSTMTNODE インスタンスを作成する。</para>
        /// <para>unchecked 文の場合は Flags に NODEFLAGS.UNCHECKED をセットする。</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseCheckedStatement(BASENODE parent, int closeIndex)	// = -1);
        {
            //ASSERT(CurrentTokenID() == TOKENID.CHECKED || CurrentTokenID() == TOKENID.UNCHECKED);

            // "(" が続いている場合は、checked 式、unchecked 式として処理する。
            if (PeekToken() == TOKENID.OPENPAREN)
            {
                return ParseExpressionStatement(parent, closeIndex);
            }

            LABELSTMTNODE stmtNode = AllocNode(NODEKIND.CHECKED, parent).AsANYLABELSTMT;

            stmtNode.LabelNode = null;
            if (CurrentTokenID() == TOKENID.UNCHECKED)
            {
                stmtNode.Flags |= NODEFLAGS.UNCHECKED;
            }
            NextToken();
            stmtNode.StatementNode = ParseBlock(stmtNode, -1);

            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseUnsafeStatement
        //
        // unsafe-statement:
        //     "unsafe"  block
        //
        /// <summary>
        /// unsafe 文を解析して LABELSTMTNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseUnsafeStatement(BASENODE parent, int closeIndex)	// = -1);
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.UNSAFE);

            LABELSTMTNODE stmtNode = AllocNode(NODEKIND.UNSAFE, parent).AsANYLABELSTMT;

            stmtNode.LabelNode = null;
            NextToken();
            stmtNode.StatementNode = ParseBlock(stmtNode, -1);

            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseEmbeddedStatement
        //
        // statement:
        //     labeled-statement
        //     declaration-statement
        //     embedded-statement
        //
        // embedded-statement:
        //     block
        //     empty-statement
        //     expression-statement
        //     selection-statement
        //     iteration-statement
        //     jump-statement
        //     try-statement
        //     checked-statement
        //     unchecked-statement
        //     lock-statement
        //     using-statement
        //     yield-statement
        //
        /// <summary>
        /// ParseStatement メソッドを呼び出して STATEMENTNODE インスタンスを作成する。
        /// 宣言文かラベル文が得られたらエラーメッセージを出力する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="complexCheck"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE ParseEmbeddedStatement(BASENODE parentNode, bool complexCheck)
        {
            // ";" と "{" が連続していたらエラーメッセージを出力する。
            // ";" を読み飛ばしておかないと "while" が見つからないことになるが?
            if (CurrentTokenID() == TOKENID.SEMICOLON)
            {
                if (!complexCheck || PeekToken() == TOKENID.OPENCURLY)
                {
                    Error(CSCERRID.WRN_PossibleMistakenNullStatement);
                }
            }

            STATEMENTNODE stmtNode = ParseStatement(parentNode, -1);

            if (stmtNode != null)
            {
                if (stmtNode.Kind == NODEKIND.LABEL || stmtNode.Kind == NODEKIND.DECLSTMT)
                {
                    ErrorAtToken(stmtNode.TokenIndex, CSCERRID.ERR_BadEmbeddedStmt);
                }
            }

            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.ParseVariableDeclarator
        //
        // B.2.5 Statements
        //
        // local-variable-declarator:
        //     identifier
        //     identifier  "="  local-variable-initializer
        //
        // local-variable-initializer:
        //     expression
        //     array-initializer
        //
        // B.2.7 Classes
        //
        // variable-declarator:
        //     identifier
        //     identifier "=" variable-initializer
        //
        // variable-initializer:
        //     expression
        //     array-initializer
        //
        /// <summary>
        /// Parse local-variable-declarator or variable-declarator.
        /// Get a variable name and an initial value (if exists),
        /// and create VARDECLNODE by them.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="parentDeclNode"></param>
        /// <param name="pvdFlags"></param>
        /// <param name="isFirst"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseVariableDeclarator(
            BASENODE parentNode,
            BASENODE parentDeclNode,
            uint pvdFlags,
            bool isFirst,
            int closeIndex)  // = -1);
        {
            VARDECLNODE varDeclNode = AllocNode(NODEKIND.VARDECL, parentNode) as VARDECLNODE;
            varDeclNode.NameNode = ParseIdentifier(varDeclNode);
            varDeclNode.DeclarationsNode = parentDeclNode;
            varDeclNode.ArgumentsNode = null;

            switch (CurrentTokenID())
            {
                //----------------------------------------
                // "="
                //----------------------------------------
                case TOKENID.EQUAL:
                    {
                        if ((pvdFlags & (uint)PARSEDECLFLAGS.FIXED) != 0)
                        {
                            goto default;
                        }
                        varDeclNode.Flags |= NODEFLAGS.VARDECL_EXPR;
                        int iIdxEqual = CurrentTokenIndex();
                        Eat(TOKENID.EQUAL);

                        DebugUtil.Assert(
                            parentNode.Kind == NODEKIND.FIELD ||
                            parentNode.Kind == NODEKIND.CONST ||
                            parentNode.Kind == NODEKIND.DECLSTMT);

                        varDeclNode.ArgumentsNode = ParseVariableInitializer(
                            varDeclNode,
                            (pvdFlags & ((uint)PARSEDECLFLAGS.LOCAL | (uint)PARSEDECLFLAGS.CONST))
                            == (uint)PARSEDECLFLAGS.LOCAL,
                            closeIndex);

                        // Create a BINOPNODE instance to represent an assignment "name = value",
                        // and set it to ArgumentsNode.
                        
                        varDeclNode.ArgumentsNode = AllocBinaryOpNode(
                            OPERATOR.ASSIGN,
                            iIdxEqual,
                            varDeclNode,
                            AllocNameNode(varDeclNode.NameNode.Name, varDeclNode.NameNode.TokenIndex),
                            varDeclNode.ArgumentsNode);
                        break;
                    }

                //----------------------------------------
                // "(" の場合。
                // This is an error, but regarding arguments in ( and ) as initial values, parse them.
                //----------------------------------------
                case TOKENID.OPENPAREN:
                    Error(CSCERRID.ERR_BadVarDecl);

                    if (PeekToken() == TOKENID.CLOSEPAREN)
                    {
                        NextToken();
                        NextToken();
                    }
                    else
                    {
                        int itok = CurrentTokenIndex();
                        varDeclNode.ArgumentsNode = ParseArgumentList(varDeclNode);
                        if (varDeclNode.ArgumentsNode.Kind == NODEKIND.LIST)
                        {
                            CALLNODE nodeCall = AllocNode(NODEKIND.CALL, varDeclNode, itok) as CALLNODE;
                            nodeCall.Operator = OPERATOR.CALL;
                            nodeCall.Operand2 = varDeclNode.ArgumentsNode;
                            nodeCall.Operand2.ParentNode = nodeCall;
                            nodeCall.Operand1 = ParseMissingName(nodeCall, itok);
                            nodeCall.CloseParenIndex = PeekTokenIndex(-1);
                            varDeclNode.ArgumentsNode = nodeCall;
                        }
                        varDeclNode.ArgumentsNode = AllocBinaryOpNode(
                            OPERATOR.ASSIGN,
                            itok,
                            varDeclNode,
                            AllocNameNode(varDeclNode.NameNode.Name, varDeclNode.NameNode.TokenIndex),
                            varDeclNode.ArgumentsNode);
                    }
                    break;

                //----------------------------------------
                // "["
                // This is only the case to define fixed size buffers in unsafe context.
                //----------------------------------------
                case TOKENID.OPENSQUARE:
                    if (pvdFlags == (uint)PARSEDECLFLAGS.FIXED)
                    {
                        NextToken();
                        varDeclNode.Flags |= NODEFLAGS.VARDECL_ARRAY;
                        if (CurrentTokenID() == TOKENID.CLOSESQUARE)
                        {
                            varDeclNode.ArgumentsNode
                                = ParseMissingName(varDeclNode, CurrentTokenIndex());
                            Error(CSCERRID.ERR_ValueExpected);
                        }
                        else
                        {
                            varDeclNode.ArgumentsNode = ParseExpression(varDeclNode, -1);
                        }
                        Eat(TOKENID.CLOSESQUARE);
                        break;
                    }
                    else
                    {
                        Error(CSCERRID.ERR_CStyleArray);
                        int dims;
                        dims = ParseArrayRankSpecifier(varDeclNode, true);
                        varDeclNode.ArgumentsNode = null;
                    }
                    break;

                //----------------------------------------
                // if identifier after identifier, it's error and skip.
                // if "=" follows, skip it and following value.
                //----------------------------------------
                case TOKENID.IDENTIFIER:
                    if (!isFirst)
                    {
                        Error(CSCERRID.ERR_MultiTypeInDeclaration);
                        NextToken();
                        if (CurrentTokenID() == TOKENID.EQUAL)
                        {
                            NextToken();
                            ParseVariableInitializer(varDeclNode, true, -1);
                        }
                        break;
                    }
                    goto default;

                //----------------------------------------
                //
                //----------------------------------------
                default:
                    if ((pvdFlags & (uint)PARSEDECLFLAGS.CONST) != 0)
                    {
                        Error(CSCERRID.ERR_ConstValueRequired);
                    }
                    else if ((pvdFlags & (uint)PARSEDECLFLAGS.FIXED) != 0)
                    {
                        if (parentDeclNode != null &&
                            (parentDeclNode as FIELDNODE).TypeNode != null &&
                            (parentDeclNode as FIELDNODE).TypeNode.Kind == NODEKIND.ARRAYTYPE)
                        {
                            Error(CSCERRID.ERR_FixedDimsRequired);
                        }
                        else
                        {
                            Eat(TOKENID.OPENSQUARE);
                        }
                    }
                    varDeclNode.ArgumentsNode = null;
                    break;
            }
            return varDeclNode;
        }

        //------------------------------------------------------------
        // CParser.ParseVariableInitializer
        //
        // variable-initializer:
        //     expression
        //     array-initializer
        //
        /// <summary>
        /// 変数を初期化するときの右辺値を解析し、BASENODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="allowStackAlloc"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseVariableInitializer(
            BASENODE parentNode,
            bool allowStackAlloc,
            int closeIndex) // = -1);
        {
            if (allowStackAlloc && CurrentTokenID() == TOKENID.STACKALLOC)
            {
                return ParseStackAllocExpression(parentNode);
            }

            if (CurrentTokenID() == TOKENID.OPENCURLY)
            {
                return ParseArrayInitializer(parentNode);
            }

            return ParseExpression(parentNode, closeIndex);
        }

        //------------------------------------------------------------
        // CParser.ParseArrayInitializer
        //
        // array-initializer:
        //     "{"  [variable-initializer-list]  "}"
        //     "{"  variable-initializer-list  ","  "}"
        //
        // variable-initializer-list:
        //     variable-initializer
        //     variable-initializer-list  ","  variable-initializer
        //
        /// <summary>
        /// 配列の初期化子を解析し、各要素の初期値のノードを連結したリストの先頭を返す。
        /// </summary>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>各要素の初期値のノードを連結したリストの先頭のノード。</returns>
        //------------------------------------------------------------
        BASENODE ParseArrayInitializer(BASENODE parentNode)
        {
            UNOPNODE initNode = AllocNode(NODEKIND.ARRAYINIT, parentNode).AsARRAYINIT;

            Eat(TOKENID.OPENCURLY);

            CListMaker list = new CListMaker(this);
            int comma = -1;

            while (CurrentTokenID() != TOKENID.CLOSECURLY)
            {
                list.Add(ParseVariableInitializer(initNode, false, -1), comma);
                if (CurrentTokenID() != TOKENID.COMMA) break;
                comma = CurrentTokenIndex();
                NextToken();
            }

            Eat(TOKENID.CLOSECURLY);
            initNode.Operand = list.GetList(initNode);
            return initNode;
        }

        //------------------------------------------------------------
        // CParser.ParseStackAllocExpression
        //
        // stackalloc-initializer:
        //     "stackalloc" unmanaged-type "[" expression "]"
        //
        /// <summary>stackalloc 節を解析する。</summary>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>得られた NEWNODE。</returns>
        //------------------------------------------------------------
        internal BASENODE ParseStackAllocExpression(BASENODE parentNode)
        {
            //ASSERT (CurrentTokenID() == TOKENID.STACKALLOC);

            NEWNODE newNode = AllocNode(NODEKIND.NEW, parentNode) as NEWNODE;

            Eat(TOKENID.STACKALLOC);

            POINTERTYPENODE pointerNode = AllocNode(NODEKIND.POINTERTYPE, newNode) as POINTERTYPENODE;

            newNode.Flags |= NODEFLAGS.NEW_STACKALLOC;
            newNode.TypeNode = pointerNode;
            newNode.InitialNode = null;
            newNode.ArgumentsNode = null;
            newNode.OpenParenIndex = newNode.CloseParenIndex = -1;

            bool hadError = false;
            pointerNode.ElementTypeNode = ParseUnderlyingType(pointerNode, out hadError);
            if (hadError)
            {
                return newNode;
            }

            pointerNode.ElementTypeNode
                = ParsePointerTypeMods(pointerNode, pointerNode.ElementTypeNode);

            if (CurrentTokenID() != TOKENID.OPENSQUARE)
            {
                Error(CSCERRID.ERR_BadStackAllocExpr);
            }
            else
            {
                newNode.OpenParenIndex = CurrentTokenIndex();
                Eat(TOKENID.OPENSQUARE);
                newNode.ArgumentsNode = ParseExpression(newNode, -1);
                newNode.CloseParenIndex = CurrentTokenIndex();
                Eat(TOKENID.CLOSESQUARE);
            }
            return newNode;
        }

        //------------------------------------------------------------
        // CParser.ParseSwitchCase
        //
        // switch-block:
        //     "{"  [switch-sections]  "}"
        //
        // switch-sections:
        //     switch-section
        //     switch-sections  switch-section
        //
        // switch-section:
        //     switch-labels  statement-list
        //
        // switch-labels:
        //     switch-label
        //     switch-labels  switch-label
        //
        // switch-label:
        //     "case"  constant-expression  ":"
        //     "default"  ":"
        //
        /// <summary>
        /// switch 文中の 1 つの switch-section を解析し、CASENODE インスタンスを作成する。
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        BASENODE ParseSwitchCase(BASENODE parent, int closeIndex)
        {
            CASENODE caseNode = AllocNode(NODEKIND.CASE, parent) as CASENODE;
            DebugUtil.Assert(AtSwitchCase(closeIndex));
            CListMaker list = new CListMaker(this);

            do
            {
                UNOPNODE labelNode = AllocNode(NODEKIND.CASELABEL, caseNode).AsCASELABEL;
                if (CurrentTokenID() == TOKENID.CASE)
                {
                    NextToken();
                    if (CurrentTokenID() == TOKENID.COLON)
                    {
                        Error(CSCERRID.ERR_ConstantExpected);
                        labelNode.Operand = ParseMissingName(labelNode, CurrentTokenIndex());
                    }
                    else
                    {
                        labelNode.Operand = ParseExpression(labelNode, -1);
                    }
                }
                else
                {
                    //ASSERT(CurrentTokenID() == TOKENID.DEFAULT);

                    NextToken();
                    labelNode.Operand = null;
                }

                Eat(TOKENID.COLON);
                list.Add(labelNode, -1);
            } while (AtSwitchCase(closeIndex));

            caseNode.LabelsNode = list.GetList(caseNode);
            STATEMENTNODE firstStmtNode = null;
            STATEMENTNODE lastStmtNode = null;

            while (!PastCloseToken(closeIndex) &&
                   !AtSwitchCase(closeIndex) &&
                   CurrentTokenID() != TOKENID.ENDFILE &&
                   CurrentTokenID() != TOKENID.CLOSECURLY)
            {
                CEnsureParserProgress epp = new CEnsureParserProgress(this);
                try
                {
                    if (firstStmtNode == null)
                    {
                        firstStmtNode = ParseStatement(caseNode, -1);
                        lastStmtNode = firstStmtNode;
                    }
                    else
                    {
                        STATEMENTNODE temp = ParseStatement(caseNode, -1);
                        lastStmtNode.NextNode = temp;
                        lastStmtNode = temp;
                    }
                    lastStmtNode.NextNode = null;
                }
                finally
                {
                    epp.EnsureProgress();
                }
            }
            caseNode.StatementsNode = firstStmtNode;
            return caseNode;
        }

        //------------------------------------------------------------
        // CParser.ParseCatchClause
        //------------------------------------------------------------
        internal BASENODE ParseCatchClause(BASENODE parent, ref bool isEmpty)
        {
            //ASSERT (CurrentTokenID() == TOKENID.CATCH);

            CATCHNODE catchNode = AllocNode(NODEKIND.CATCH, parent) as CATCHNODE;

            if (isEmpty)
            {
                Error(CSCERRID.ERR_TooManyCatches);
            }

            NextToken();

            if (CurrentTokenID() == TOKENID.OPENPAREN)
            {
                NextToken();
                catchNode.TypeNode = ParseClassType(catchNode);
                if (CurrentTokenID() == TOKENID.IDENTIFIER)
                {
                    catchNode.NameNode = ParseIdentifier(catchNode);
                }
                else
                {
                    catchNode.NameNode = null;
                }
                Eat(TOKENID.CLOSEPAREN);
            }
            else
            {
                catchNode.TypeNode = null;
                catchNode.NameNode = null;
                isEmpty = true;
            }

            catchNode.BlockNode = ParseBlock(catchNode, -1) as BLOCKNODE;
            return catchNode;
        }

        //------------------------------------------------------------
        // CParser.ParseExpression
        //
        /// <summary>
        /// 式を解析し BASENODE で表す。実際の処理は ParseSubExpression が行う。
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseExpression(BASENODE parent, int closeIndex)  // = -1);
        {
            BASENODE p = ParseSubExpression(parent, 0, closeIndex);
            p.ParentNode = parent;
            return p;
        }

        //------------------------------------------------------------
        // CParser.ParseSubExpression
        //
        // unary-expression:
        //     primary-expression
        //     "+"  unary-expression
        //     "-"  unary-expression
        //     "!"  unary-expression
        //     "~"  unary-expression
        //     pre-increment-expression
        //     pre-decrement-expression
        //     cast-expression
        //
        /// <summary>
        /// <para>unary-expression と二項演算子から成るブロックを解析し、BASENODE インスタンスを作成する。</para>
        /// <para>ブロックは指定された優先度と結合性によって決まる。</para>
        /// </summary>
        /// <param name="parentNode">親となるノード。</param>
        /// <param name="precedence">優先度。</param>
        /// <param name="closeIndex">終了位置のインデックス。（または -1）</param>
        /// <returns>部分式を表す BASENODE インスタンス。</returns>
        //------------------------------------------------------------
        internal BASENODE ParseSubExpression(
            BASENODE parentNode,
            uint precedence,
            int closeIndex)	// = -1);
        {
            BASENODE leftOperand = null;
            uint newPrecedence = 0;
            OPERATOR operatorId = OPERATOR.NONE;

            // 部分式で使えないトークン（下記）ならエラーメッセージを表示し、
            // 名前なしの NAMENODE インスタンスを返す。
            //
            // "break", "case", "catch", "const", "continue", "do", "finally", "for", "foreach",
            // "goto", "if", "lock", "return", "switch", "throw", "try", "using", "while", 

            if ((TokenInfoArray[(int)CurrentTokenID()].Flags &
                TOKFLAGS.F_INVALIDSUBEXPRESSION) != 0)
            {
                Error(
                    CSCERRID.ERR_InvalidExprTerm,
                    new ErrArg(TokenInfoArray[(int)CurrentTokenID()].Text));
                return ParseMissingName(parentNode, CurrentTokenIndex());
            }

            //--------------------------------------------------
            // unary-expression を取得する。
            //--------------------------------------------------
            // 現在のトークンが単項演算子の場合（cast は除く）
            if (IsUnaryOperator(CurrentTokenID(), out operatorId, out newPrecedence))
            {
                int tokenIndex = CurrentTokenIndex();
                NextToken();
                BASENODE termNode = ParseSubExpression(parentNode, newPrecedence, -1);

                if (operatorId == OPERATOR.NEG &&
                    termNode.Kind == NODEKIND.CONSTVAL &&
                    (termNode.Flags & NODEFLAGS.CHECK_FOR_UNARY_MINUS) != 0)
                {
                    // int の場合、2^31 は int の範囲外だが、-2^31 は範囲内である。
                    // 2^31 に単項演算子 - が作用しているものは int 値 -2^31 で置き換えることにする。
                    // long についても同様に処理する。

                    leftOperand = termNode;
                    CONSTVALNODE constNode=leftOperand as CONSTVALNODE;
                    //leftOperand.PredefinedType =
                    //    ((termNode.PredefinedType == PREDEFTYPE.ULONG) ?
                    //        PREDEFTYPE.LONG :
                    //        PREDEFTYPE.INT);
                    if (termNode.PredefinedType == PREDEFTYPE.ULONG)
                    {
                        leftOperand.PredefinedType = PREDEFTYPE.LONG;
                        ulong ulval = constNode.Value.GetULong();
                        constNode.Value.SetLong((long)ulval);
                    }
                    else
                    {
                        leftOperand.PredefinedType = PREDEFTYPE.INT;
                        uint uival=constNode.Value.GetUInt();
                        constNode.Value.SetInt((int)uival);
                    }
                    leftOperand.TokenIndex = tokenIndex;
                }
                else
                {
                    leftOperand = AllocUnaryOpNode(
                        operatorId, tokenIndex, parentNode, termNode);
                }
            }
            else if (IsLambdaExpression())
            {
                return ParseLambdaExpression(parentNode, closeIndex);
            }
            // 単項演算子ではない場合
            // cast-expression も ParseTerm メソッドが処理する。
            else
            {
                leftOperand = ParseTerm(parentNode, closeIndex);
            }

            //--------------------------------------------------
            // 二項演算子で連結されている
            //--------------------------------------------------
            while (!PastCloseToken(closeIndex))
            {
                if (!IsBinaryOperator(CurrentTokenID(), out operatorId, out newPrecedence))
                {
                    break;
                }
                DebugUtil.Assert(newPrecedence > 0);
                bool isDoubleOp = false;

                //----------------------------------------
                // right-shift ">>" と right-shift-assignment ">>=" は
                // 字句解析時には取得されていないので、
                // IsBinaryOperator メソッドは TOKENID.GREATER を返している。
                //----------------------------------------
                if (CurrentTokenID() == TOKENID.GREATER &&
                    (PeekToken() == TOKENID.GREATER ||
                    PeekToken() == TOKENID.GREATEREQUAL))
                {
                    POSDATA posFirst = new POSDATA(CurrentTokenPos());
                    POSDATA posNext = new POSDATA(tokenArray[PeekTokenIndex()]);
                    if (posFirst.LineIndex == posNext.LineIndex &&
                        (posFirst.CharIndex + 1) == posNext.CharIndex)
                    {
                        if (PeekToken() == TOKENID.GREATER)
                        {
                            operatorId = OPERATOR.RSHIFT;
                        }
                        else
                        {
                            operatorId = OPERATOR.RSHIFTEQ;
                        }
                        newPrecedence = (uint)GetOperatorPrecedence(operatorId);
                        isDoubleOp = true;
                    }
                }

                //----------------------------------------
                // 得られた二項演算子の優先度が低い、同じだが右結合でない場合は処理を終了する。
                // そうでない場合は二項演算子の次へ処理位置を移動させる。
                //----------------------------------------
                if (newPrecedence < precedence)
                {
                    break;
                }
                if ((newPrecedence == precedence) && !IsRightAssociative(operatorId))
                {
                    break;
                }

                int opTokIndex = CurrentTokenIndex();
                NextToken();
                if (isDoubleOp)
                {
                    NextToken();
                }

                //----------------------------------------
                // 演算子 ?
                //----------------------------------------
                if (operatorId == OPERATOR.QUESTION)
                {
                    BASENODE colonLeft = ParseSubExpression(parentNode, newPrecedence - 1, -1);
                    BASENODE rightOperand;
                    int colonIndex = CurrentTokenIndex();
                    Eat(TOKENID.COLON);

                    if (PeekToken(-1) == TOKENID.COLON)
                    {
                        //there was a : the right side of the ? is a binop whose lhs is 
                        //the expr before the : and whose RHS is the expr after it

                        BASENODE colonRight
                            = ParseSubExpression(parentNode, newPrecedence - 1, -1);
                        rightOperand = AllocBinaryOpNode(
                            OPERATOR.COLON,
                            colonIndex,
                            parentNode,
                            colonLeft,
                            colonRight);
                    }
                    else
                    {
                        // this should only happen in the error case,
                        // and we should always have an error because of the EAT(TID_COLON) above.
                        //ASSERT(parseErrorCount > 0);

                        //there wasn't any colon, the right side of the ? is just the 
                        //expression we parsed after the ?

                        rightOperand = colonLeft;
                    }
                    leftOperand = AllocBinaryOpNode(
                        OPERATOR.QUESTION,
                        opTokIndex,
                        parentNode,
                        leftOperand,
                        rightOperand);
                }
                //----------------------------------------
                // is, as の右オペランドは型でなければならない。
                //----------------------------------------
                else if (operatorId == OPERATOR.IS || operatorId == OPERATOR.AS)
                {
                    leftOperand = AllocBinaryOpNode(
                        operatorId,
                        opTokIndex,
                        parentNode,
                        leftOperand,
                        ParseType(parentNode, true));
                }
                //----------------------------------------
                // 以上に当てはまらない場合は、
                // 以降のトークン列を右オペランドとして解析する。
                //----------------------------------------
                else
                {
                    leftOperand = AllocBinaryOpNode(
                        operatorId,
                        opTokIndex,
                        parentNode,
                        leftOperand,
                        ParseSubExpression(parentNode, newPrecedence, closeIndex));
                }
            }

            //--------------------------------------------------------
            // (CS3) Query Expression
            //--------------------------------------------------------
            if (leftOperand.Kind == NODEKIND.NAME &&
                (leftOperand as NAMENODE).Name == "from")
            {
#if false
                BASENODE queryNode = null;
                int mark = CurrentTokenIndex();

                FROMCLAUSENODE fromNode = ParseFromClause(parentNode, false);
                if (fromNode != null)
                {
                    queryNode = ParseQueryExpression(parentNode, fromNode);
                }

                if (queryNode == null)
                {
                    Rewind(mark);
                    return ParseIdentifier(parentNode);
                }
                return queryNode;
#endif
                FROMCLAUSENODE fromNode = ParseFromClause(parentNode, true);
                if (fromNode != null)
                {
                    return ParseQueryExpression(parentNode, fromNode);
                }
                else if (IsQueryExpressionParseMode())
                {
                    Error(CSCERRID.ERR_InvalidExprTerm, new ErrArg("from"));
                }
            }

            leftOperand.ParentNode = parentNode;
            return leftOperand;
        }

        //------------------------------------------------------------
        // CParser.ParseTerm
        //
        // primary-expression:
        //     primary-no-array-creation-expression
        //
        //     array-creation-expression:               (12)
        //         "new"  non-array-type  "["  expression-list  "]"
        //             [rank-specifiers]  [array-initializer]
        //         "new"  array-type  array-initializer
        //         "new"  rank-specifier  array-initializer （C# 3.0 から）
        //
        // primary-no-array-creation-expression:
        //     literal                                  (9), (10), (11)
        //
        //     simple-name:
        //         identifier  [type-argument-list]
        //
        //     member-access:   1、3 番目は (6) と ParsePostFixOperator、2 番目は (14)
        //         primary-expression  "."  identifier  [type-argument-list]
        //         predefined-type  "."  identifier  [type-argument-list]
        //         qualified-alias-member  "."  identifier
        //
        //     invocation-expression:                   (8)
        //         primary-expression  "("  [argument-list]  ")"
        //
        //     element-access:                          (ParsePostFixOperator)
        //         primary-no-array-creation-expression  "["  argument-list  "]"
        //
        //     this-access                              (7)
        //     base-access                              (7)
        //     post-increment-expression                (6) と ParsePostFixOperator
        //     post-decrement-expression                (6) と ParsePostFixOperator
        //
        //     object-creation-expression:              (12)
        //         "new"  type  "("  [argument-list]  )  [object-or-collection-initializer]
        //         "new"  type  object-or-collection-initializer
        //
        //     delegate-creation-expression:            (12)
        //         "new"  delegate-type  "("  expression  ")"
        //
        //     anonymous-object-creation-expression:    (12)    （C# 3.0 から）
        //         "new"  anonymous-object-initializer
        //
        //     typeof-expression                        (1)
        //     checked-expression                       (3)
        //     unchecked-expression                     (3)
        //     default-value-expression                 (2)
        //
        //     anonymous-method-expression:             (13)
        //         "delegate"  [explicit-anonymous-function-signature]  block
        //
        // 上記のほかに
        //     false, true, null                        (7)
        //
        /// <summary>
        /// primary-expression と unary-expression のうちの cast-expression を解析し、
        /// BASENODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="closeIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseTerm(BASENODE parentNode, int closeIndex)   // = -1);
        {
            TOKENID tokenId = CurrentTokenID();
            int tokenIndex = CurrentTokenIndex();
            BASENODE exprNode = null;

            switch (tokenId)
            {
                //----------------------------------------
                // (1) typeof
                //----------------------------------------
                case TOKENID.TYPEOF:
                    NextToken();
                    Eat(TOKENID.OPENPAREN);
                    {
                        int mark = CurrentTokenIndex();
                        int paramCount = ScanOpenType();
                        TOKENID tokNext = CurrentTokenID();
                        Rewind(mark);
                        if (paramCount > 0 && tokNext == TOKENID.CLOSEPAREN)
                            exprNode = ParseOpenType(parentNode);
                        else
                            exprNode = ParseReturnType(parentNode);
                    }

                    Eat(TOKENID.CLOSEPAREN);

                    exprNode = AllocUnaryOpNode(
                        OPERATOR.TYPEOF, tokenIndex, parentNode, exprNode);
                    break;

                //----------------------------------------
                // (2) default, sizeof
                //----------------------------------------
                case TOKENID.DEFAULT:
                case TOKENID.SIZEOF:
                    NextToken();
                    Eat(TOKENID.OPENPAREN);

                    exprNode = ParseType(parentNode, false);

                    Eat(TOKENID.CLOSEPAREN);

                    if (tokenId == TOKENID.DEFAULT)
                    {
                        ReportFeatureUse("CSCSTR_FeatureDefault");
                    }

                    exprNode = AllocUnaryOpNode(
                        (OPERATOR)TokenInfoArray[(int)tokenId].SelfOperator,
                        tokenIndex,
                        parentNode,
                        exprNode);
                    break;

                // check for special operators with function-like syntax

                //----------------------------------------
                // (3) __makeref, __reftype, checked, unchecked
                //----------------------------------------
                case TOKENID.MAKEREFANY:
                case TOKENID.REFTYPE:
                case TOKENID.CHECKED:
                case TOKENID.UNCHECKED:
                    NextToken();
                    Eat(TOKENID.OPENPAREN);

                    exprNode = ParseSubExpression(parentNode, 0, -1);

                    Eat(TOKENID.CLOSEPAREN);

                    exprNode = AllocUnaryOpNode(
                        (OPERATOR)TokenInfoArray[(int)tokenId].SelfOperator,
                        tokenIndex,
                        parentNode,
                        exprNode);
                    break;

                //----------------------------------------
                // (4) __refvalue
                //----------------------------------------
                case TOKENID.REFVALUE:
                    {
                        NextToken();
                        Eat(TOKENID.OPENPAREN);

                        exprNode = ParseSubExpression(parentNode, 0, -1);

                        Eat(TOKENID.COMMA);

                        BASENODE pType = ParseType(parentNode, false);

                        Eat(TOKENID.CLOSEPAREN);

                        exprNode = AllocBinaryOpNode(
                            OPERATOR.REFVALUE,
                            tokenIndex, parentNode, exprNode, pType);
                        break;
                    }

                //----------------------------------------
                // (5) :: はここでは現れないはず。
                // エラーメッセージを表示し、次の文字から処理を続ける。
                //----------------------------------------
                case TOKENID.COLONCOLON:
                    // Report the error
                    Error(CSCERRID.ERR_IdentifierExpected);
                    // Eat the '::'
                    NextToken();
                    return ParseTerm(parentNode, -1);

                //----------------------------------------
                // (6) 最初のトークンが識別子になるのは
                //      simple-name
                //      member-access
                //      invocation-expression
                //      element-access
                //  の場合である。後置演算子を ParsePostFixOperator が処理する。
                //----------------------------------------
                case TOKENID.IDENTIFIER:
                    // Parse in 
                    //     - references to static members of instantiaions ArrayList<String>.Sort
                    //     - calls to generic methods....e.g. ArrayList.Syncronized<String>(foo,bar)
                    //
                    // "<" takes precedence over its use as a binary operator in 
                    // this circumstance.   We look to see if we can
                    // successfully scan in something that looks like a type list
                    // immediately after this.  This rules
                    // out accidently commiting to parsing "a < b" as a type application.

                    exprNode = ParseGenericQualifiedNameList(parentNode, true);
                    break;

                //----------------------------------------
                // (7) __arglist, base, false, this, true, null
                //----------------------------------------
                case TOKENID.ARGS:
                case TOKENID.BASE:
                case TOKENID.FALSE:
                case TOKENID.THIS:
                case TOKENID.TRUE:
                case TOKENID.NULL:
                    exprNode = AllocOpNode(
                        (OPERATOR)TokenInfoArray[(int)tokenId].SelfOperator,
                        CurrentTokenIndex(),
                        parentNode);
                    NextToken();
                    break;

                //----------------------------------------
                // (8) "("
                //----------------------------------------
                case TOKENID.OPENPAREN:
                    exprNode = ParseCastOrExpression(parentNode);
                    break;

                //----------------------------------------
                // (9) 数値
                //----------------------------------------
                case TOKENID.NUMBER:
                    exprNode = ParseNumber(parentNode);
                    break;

                //----------------------------------------
                // (10) STRINGLIT, VSLITERAL の場合は CONSTVALNODE を作成する。
                //----------------------------------------
                case TOKENID.STRINGLIT:
                case TOKENID.VSLITERAL:
                    exprNode = ParseStringLiteral(parentNode);
                    break;

                //----------------------------------------
                // (11) CHARLIT の場合は CONSTVALNODE を作成する。
                //----------------------------------------
                case TOKENID.CHARLIT:
                    exprNode = ParseCharLiteral(parentNode);
                    break;

                //----------------------------------------
                // (12) new
                //
                // array-creation-expression:
                //     "new"  non-array-type  "["  expression-list  "]"
                //         [rank-specifiers]  [array-initializer]
                //     "new"  array-type  array-initializer
                //     "new"  rank-specifier  array-initializer （C# 3.0 から）
                //
                // object-creation-expression:
                //     "new"  type  "("  [argument-list]  )  [object-or-collection-initializer]
                //     "new"  type  object-or-collection-initializer
                //
                // delegate-creation-expression:
                //     "new"  delegate-type  "("  expression  ")"
                //
                // anonymous-object-creation-expression:    （C# 3.0 から）
                //     "new"  anonymous-object-initializer
                //----------------------------------------
                case TOKENID.NEW:
                    exprNode = ParseNewExpression(parentNode);
                    break;

                //----------------------------------------
                // (13) delegate
                //----------------------------------------
                case TOKENID.DELEGATE:
                    exprNode = ParseAnonymousMethodExpr(parentNode);
                    break;

                //----------------------------------------
                // (14) 以上に当てはまらない場合はエラーである。
                //----------------------------------------
                default:

                    // TOKFLAGS.F_PREDEFINED
                    // "bool", "byte", "char", "decimal", "double", "float", "int", "long",
                    // "object", "sbyte", "short", "string", "uint", "ulong", "ushort", "void", 

                    // check for intrinsic type followed by '.'
                    if (((TokenInfoArray[(int)CurrentTokenID()].Flags & TOKFLAGS.F_PREDEFINED) != 0) &&
                        (TokenInfoArray[(int)CurrentTokenID()].PredefinedType != PREDEFTYPE.VOID))
                    {
                        // primary-expression の最初のトークンが標準の型名となるのは、
                        // その型を表すクラスや構造体のメンバを使用する場合に限られる。
                        //     predefined-type  "."  identifier  [type-argument-list]

                        TYPEBASENODE pType = ParsePredefinedType(parentNode);

                        if (CurrentTokenID() != TOKENID.DOT)
                        {
                            exprNode = AllocOpNode(OPERATOR.NOP, CurrentTokenIndex(), parentNode);
                            Error(
                                CSCERRID.ERR_InvalidExprTerm,
                                new ErrArg(TokenInfoArray[(int)tokenId].Text));
                            NextToken();
                        }
                        else
                        {
                            exprNode = pType;
                            // fall through to post-fix check below
                        }
                    }
                    else
                    {
                        //exprNode = AllocOpNode (OPERATOR.NOP, CurrentTokenIndex(), parentNode);
                        exprNode = ParseMissingName(parentNode, CurrentTokenIndex());
                        Error(
                            CSCERRID.ERR_InvalidExprTerm,
                            new ErrArg(TokenInfoArray[(int)tokenId].Text));
                        NextToken();
                    }
                    break;
            }

            return ParsePostFixOperator(parentNode, exprNode, closeIndex);
        }

        //------------------------------------------------------------
        // CParser.ParsePostFixOperator
        //
        /// <summary>
        /// ParseTerm の補助メソッド。
        /// primary-expression の後置演算子を解析し、
        /// 対象となるノードと合わせて式を表すノードを作成する。
        /// </summary>
        /// <param name="parent">親となるノード。</param>
        /// <param name="exprNode">postfix の対象となるノード。</param>
        /// <param name="closeIndex">終了位置のインデックス。</param>
        /// <returns>作成した BASENODE.</returns>
        //------------------------------------------------------------
        internal BASENODE ParsePostFixOperator(
            BASENODE parent,
            BASENODE exprNode,
            int closeIndex)   // = -1);
        {
            //ASSERT(exprNode);

            // Look for post-fix operators
            while (true)
            {
                TOKENID tokenId = CurrentTokenID();
                int tokenIndex = CurrentTokenIndex();

                if (PastCloseToken(closeIndex))
                {
                    return exprNode;
                }

                switch (tokenId)
                {
                    //------------------------------
                    // この段階でトークンが "(" となるのは invocation-expression のときである。
                    //
                    // invocation-expression:
                    //     primary-expression  "("  [argument-list]  ")"
                    //------------------------------
                    case TOKENID.OPENPAREN:
                        {
                            int iErrorCount = parseErrorCount;

                            CALLNODE callNode = AllocNode(NODEKIND.CALL, parent, tokenIndex) as CALLNODE;
                            callNode.Operand1 = exprNode;
                            callNode.Operator = OPERATOR.CALL;
                            callNode.Operand2 = ParseArgumentList(callNode);
                            exprNode.ParentNode = callNode;
                            callNode.CloseParenIndex = PeekTokenIndex(-1);

                            // NOTE:  -1 because ParseArgumentList advanced past it...
                            if (tokenArray[callNode.CloseParenIndex].TokenID != TOKENID.CLOSEPAREN)
                            {
                                callNode.CloseParenIndex
                                    = PeekTokenIndexFrom(callNode.CloseParenIndex, 1);
                            }
                            if (iErrorCount != parseErrorCount)
                            {
                                callNode.Flags |= NODEFLAGS.CALL_HADERROR;
                            }

                            // Parameter tips key on this for window placement...
                            exprNode = callNode;
                            break;
                        }

                    //------------------------------
                    // この段階でトークンが "[" となるのは element-access のときである。
                    //
                    // element-access:
                    //     primary-no-array-creation-expression  "["  argument-list  "]"
                    //------------------------------
                    case TOKENID.OPENSQUARE:
                        {
                            int errorCount = parseErrorCount;
                            int dims;

                            CALLNODE callNode = AllocNode(NODEKIND.DEREF, parent, tokenIndex).AsDEREF;
                            callNode.Operator = OPERATOR.DEREF;
                            callNode.Operand1 = exprNode;
                            callNode.Operand2 = ParseDimExpressionList(callNode, out dims);
                            callNode.CloseParenIndex = PeekTokenIndex(-1);
                            exprNode.ParentNode = callNode;

                            // NOTE:  -1 because ParseDimExpressionList advanced past it...
                            if (tokenArray[callNode.CloseParenIndex].TokenID != TOKENID.CLOSESQUARE)
                            {
                                callNode.CloseParenIndex
                                    = PeekTokenIndexFrom(callNode.CloseParenIndex, 1);
                            }
                            if (errorCount != parseErrorCount)
                            {
                                callNode.Flags |= NODEFLAGS.CALL_HADERROR;
                            }

                            // Parameter tips key on this for window placement...
                            exprNode = callNode;
                            break;
                        }

                    //------------------------------
                    // "++" と "--"
                    //------------------------------
                    case TOKENID.PLUSPLUS:
                    case TOKENID.MINUSMINUS:
                        exprNode = AllocUnaryOpNode(
                            tokenId == TOKENID.PLUSPLUS ? OPERATOR.POSTINC : OPERATOR.POSTDEC,
                            tokenIndex, parent, exprNode);
                        NextToken();
                        break;

                    //------------------------------
                    // "::"
                    //------------------------------
                    case TOKENID.COLONCOLON:
                        CheckToken(TOKENID.DOT);
                        // Fall through.
                        goto case TOKENID.DOT;

                    //------------------------------
                    // "->" と "."
                    //
                    // member-access:
                    //     primary-expression  "."  identifier  [type-argument-list]
                    //     predefined-type  "."  identifier  [type-argument-list] (これは除く）
                    //     qualified-alias-member  "."  identifier
                    //------------------------------
                    case TOKENID.ARROW:
                    case TOKENID.DOT:
                        {
                            NextToken();
                            // Parse in calls to generic methods....
                            // e.g. ArrayList.Syncronized<String>(foo,bar)

                            // "<" takes precedence over its use as a binary operator in 
                            // this circumstance.   We look to see if we can
                            // successfully scan in something that looks like a type list, and we can see a "(", ")", or "."
                            // immediately after this.  This rules
                            // out accidently commiting to parsing "a < b" as a type application.

                            exprNode = AllocDotNode(
                                tokenIndex, parent, exprNode,
                                ParseGenericQualifiedNamePart(parent, true, false));
                            if (tokenId == TOKENID.ARROW)
                            {
                                exprNode.Kind = NODEKIND.ARROW;
                            }
                            break;
                        }

                    //------------------------------
                    // 以上に当てはまらない場合は、対象のノードをそのまま返す。
                    //------------------------------
                    default:
                        return exprNode;
                }
            }
        }

        //------------------------------------------------------------
        // CParser.ParseCastOrExpression
        //
        /// <summary>
        /// 式中に ( が現れた時に、以降のトークン列を処理する。
        /// <list type="bullet">
        /// <item>キャストの場合は BINOPNODE インスタンスを作成する。</item>
        /// <item>括弧で囲まれた式の場合は UNOPNODE インスタンスを作成する。</item>
        /// </list>
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseCastOrExpression(BASENODE parentNode)
        {
            //ASSERT (CurrentTokenID() == TOKENID.OPENPAREN);

            int startIndex = CurrentTokenIndex();

            //--------------------------------------------------
            // We have a decision to make
            // -- is this a cast, or is it a parenthesized expression?
            // Because look-ahead is cheap with our token stream, we check
            // to see if this "looks like" a cast
            // (without constructing any parse trees)
            // to help us make the decision.
            //--------------------------------------------------
            if (ScanCast())
            {
                // Looks like a cast, so parse it as one.
                Rewind(startIndex);
                int tokenIndex = CurrentTokenIndex();
                NextToken();
                TYPEBASENODE typeNode = ParseType(null, false);
                Eat(TOKENID.CLOSEPAREN);
                return AllocBinaryOpNode(
                    OPERATOR.CAST,
                    tokenIndex,
                    parentNode,
                    typeNode,
                    ParseSubExpression(parentNode, (uint)GetCastPrecedence(), -1));
            }

            //--------------------------------------------------
            // Doesn't look like a cast,
            // so parse this as a parenthesized expression.
            //--------------------------------------------------
            Rewind(startIndex);
            int index = CurrentTokenIndex();
            NextToken();
            BASENODE exprNode = ParseSubExpression(parentNode, 0, -1);
            Eat(TOKENID.CLOSEPAREN);
            return AllocUnaryOpNode(OPERATOR.PAREN, index, parentNode, exprNode);
        }

        //------------------------------------------------------------
        // CParser.ParseAnonymousMethodExpr
        //
        // anonymous-method-expression:
        //     "delegate"  [explicit-anonymous-function-signature]  block
        //
        // anonymous-function-signature:
        //     explicit-anonymous-function-signature
        //     implicit-anonymous-function-signature
        //
        // explicit-anonymous-function-signature:
        //     "("  [explicit-anonymous-function-parameter-list]  ")"
        //
        // explicit-anonymous-function-parameter-list:
        //     explicit-anonymous-function-parameter
        //     explicit-anonymous-function-parameter-list  ","  explicit-anonymous-function-parameter
        //
        // explicit-anonymous-function-parameter:
        //     [anonymous-function-parameter-modifier]  type  identifier
        //
        // anonymous-function-parameter-modifier:
        //     "ref"
        //     "out"
        //
        // implicit-anonymous-function-signature:
        // 	"("  implicit-anonymous-function-parameter-listopt  ")"
        // 	implicit-anonymous-function-parameter
        //
        // implicit-anonymous-function-parameter-list:
        // 	implicit-anonymous-function-parameter
        // 	implicit-anonymous-function-parameter-list  ","  implicit-anonymous-function-parameter
        //
        // implicit-anonymous-function-parameter:
        // 	identifier
        //
        /// <summary>
        /// 匿名メソッドの定義を解析し、ANONBLOCKNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseAnonymousMethodExpr(BASENODE parentNode)
        {
            ANONBLOCKNODE anonNode = AllocNode(NODEKIND.ANONBLOCK, parentNode) as ANONBLOCKNODE;

            anonNode.ArgumentsNode = null;
            anonNode.CloseParenIndex = -1;

            ReportFeatureUse("CSCSTR_FeatureAnonDelegates");
            Eat(TOKENID.DELEGATE);

            if (CurrentTokenID() == TOKENID.OPENPAREN)
            {
                int iOpen;
                bool btemp = false;
                ParseParameterList(
                    anonNode,
                    out anonNode.ArgumentsNode,
                    out iOpen,
                    out anonNode.CloseParenIndex,
                    out btemp,
                    (int)(kppo.NoParams | kppo.NoVarargs | kppo.NoAttrs));
                //ASSERT(iOpen == PeekTokenIndexFrom(anonNode.tokidx));
            }

            anonNode.BodyNode = ParseBlock(anonNode, -1) as BLOCKNODE;
            return anonNode;
        }



        //------------------------------------------------------------
        // CParser.ParseNumber
        //
        /// <summary>
        /// 数値を表すトークン列を解析し、CONSTVALNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        BASENODE ParseNumber(BASENODE parentNode)
        {
            //ASSERT (CurrentTokenID() == TID_NUMBER);

            CONSTVALNODE constNode
                = AllocNode(NODEKIND.CONSTVAL, parentNode) as CONSTVALNODE;

            // Scan number into actual numeric value
            ScanNumericLiteral(CurrentTokenIndex(), constNode);
            NextToken();
            return constNode;
        }

        //------------------------------------------------------------
        // CParser.ParseStringLiteral
        //
        /// <summary>
        /// 標準リテラル文字列、逐語的リテラル文字列から
        /// CONSTVALNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseStringLiteral(BASENODE parent)
        {
            //ASSERT (CurrentTokenID() == TID_STRINGLIT || CurrentTokenID() == TID_VSLITERAL);
            // ScanStringLiteral 内でチェックする。

            CONSTVALNODE constNode = AllocNode(NODEKIND.CONSTVAL, parent) as CONSTVALNODE;

            // Scan string into STRCONST object
            constNode.PredefinedType = PREDEFTYPE.STRING;
            constNode.Value.SetString(ScanStringLiteral(CurrentTokenIndex()));
            NextToken();
            return constNode;
        }

        //------------------------------------------------------------
        // CParser.ParseCharLiteral
        //
        /// <summary>
        /// 文字リテラルから CONSTVALNODE インスタンスを作成する。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        BASENODE ParseCharLiteral(BASENODE parent)
        {
            //ASSERT (CurrentTokenID() == TID_CHARLIT);
            if (CurrentTokenID() != TOKENID.CHARLIT)
                throw new LogicError("CParser.ParseCharLiteral");

            CONSTVALNODE constNode = AllocNode(NODEKIND.CONSTVAL, parent) as CONSTVALNODE;

            // Scan char into actual value
            constNode.PredefinedType = PREDEFTYPE.CHAR;
            constNode.Value.SetChar(ScanCharLiteral(CurrentTokenIndex()));
            NextToken();
            return constNode;
        }

        //------------------------------------------------------------
        // CParser.ParseNewExpression
        //
        // array-creation-expression:
        //     "new"  non-array-type  "["  expression-list  "]"
        //         [rank-specifiers]  [array-initializer]
        //     "new"  array-type  array-initializer
        //     "new"  rank-specifier  array-initializer （C# 3.0 から）
        //
        // object-creation-expression:
        //     "new"  type  "("  [argument-list]  ")"  [object-or-collection-initializer]
        //         （object-or-collection-initializer は C# 3.0 から。）
        //     "new"  type  object-or-collection-initializer    （C# 3.0 から）
        //
        //
        // delegate-creation-expression:
        //     "new"  delegate-type  "("  expression  ")"
        //
        // anonymous-object-creation-expression:    （C# 3.0 から）
        //     "new"  anonymous-object-initializer
        //
        // array-initializer:
        //     "{"  [variable-initializer-list]  "}"
        //     "{"  variable-initializer-list  ","  "}"
        //
        // variable-initializer-list:
        //     variable-initializer
        //     variable-initializer-list  ","  variable-initializer
        //
        // variable-initializer:
        //     expression
        //     array-initializer
        //
        /// <summary>
        /// ParseTerm メソッドの補助メソッド。
        /// primary-expression のうち "new" を持つものを解析して BASENODE インスタンスを作成する。
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE ParseNewExpression(BASENODE parentNode)
        {
            DebugUtil.Assert(CurrentTokenID() == TOKENID.NEW);

            NEWNODE newNode = AllocNode(NODEKIND.NEW, parentNode) as NEWNODE;

            NextToken();
            TOKENID tokenID = CurrentTokenID();

            //--------------------------------------------------------
            // (CS3) anonymous-object-creation-expression
            //--------------------------------------------------------
            if (CurrentTokenID() == TOKENID.OPENCURLY)
            {
                ParseAnonymousObjectInitializer(newNode);
                return newNode;
            }

            bool hadError = false;

            //--------------------------------------------------------
            // (CS3) implicitly typed array
            //--------------------------------------------------------
            if (CurrentTokenID() == TOKENID.OPENSQUARE &&
                PeekToken(1) == TOKENID.CLOSESQUARE &&
                PeekToken(2) == TOKENID.OPENCURLY)
            {
                newNode.TypeNode =
                    AllocNode(NODEKIND.IMPLICITTYPE, parentNode) as IMPLICITTYPENODE;
                newNode.Flags |= NODEFLAGS.NEW_IMPLICITLY_TYPED_ARRAY;
            }
            //--------------------------------------------------------
            // 以上に当てはまらない場合、
            // "new" の次のトークンはすべて型を表すものである。
            // 型を取得し、NEWNODE の TypeNode フィールドにセットする。
            //--------------------------------------------------------
            else
            {
                newNode.TypeNode = ParseUnderlyingType(newNode, out hadError);
            }

            newNode.InitialNode = null;
            newNode.ArgumentsNode = null;
            newNode.OpenParenIndex = newNode.CloseParenIndex = -1;

            if (hadError)
            {
                return newNode;
            }

            //--------------------------------------------------------
            // Check for nullable
            //--------------------------------------------------------
            if (CurrentTokenID() == TOKENID.QUESTION)
            {
                ReportFeatureUse("CSCSTR_FeatureNullable");
                NULLABLETYPENODE nullNode = AllocNode(NODEKIND.NULLABLETYPE, newNode) as NULLABLETYPENODE;
                nullNode.ElementTypeNode = newNode.TypeNode;
                newNode.TypeNode.ParentNode = nullNode;
                newNode.TypeNode = nullNode;
                NextToken();
            }

            //--------------------------------------------------------
            // Check for pointer types
            //--------------------------------------------------------
            newNode.TypeNode = ParsePointerTypeMods(parentNode, newNode.TypeNode);

            //--------------------------------------------------------
            // C# 2.0 まででは、型の後は ( ) か [ ] でなければならない。
            //--------------------------------------------------------
            switch (CurrentTokenID())
            {
                //----------------------------------------------------
                // Object-creation, which is a class/struct type followed by '('
                //
                // ( の場合は、
                // 引数のリストを表すノード列を ArgumentsNode フィールドにセットする。
                //----------------------------------------------------
                case TOKENID.OPENPAREN:
                    {
                        if (newNode.TypeNode.Kind == NODEKIND.POINTERTYPE)
                        {
                            // if it has pointer modifiers, it can only be a new array
                            CheckToken(TOKENID.OPENSQUARE);
                            newNode.Flags |= NODEFLAGS.CALL_HADERROR;
                            break;
                        }

                        int iErrors = parseErrorCount;

                        // This is a valid object-creation-expression (assuming the arg list is okay)
                        newNode.OpenParenIndex = CurrentTokenIndex();
                        newNode.ArgumentsNode = ParseArgumentList(newNode);
                        newNode.CloseParenIndex = PeekTokenIndex(-1);
                        // -1 because ParseArgumentList skipped it
                        // ParseArgumentList から戻ってきたときには処理位置は ) の次になっている。
                        if (tokenArray[newNode.CloseParenIndex].TokenID != TOKENID.CLOSEPAREN)
                        {
                            newNode.CloseParenIndex = PeekTokenIndexFrom(newNode.CloseParenIndex, -1);
                        }

                        if (iErrors != parseErrorCount)
                        {
                            newNode.Flags |= NODEFLAGS.CALL_HADERROR;
                        }

                        //--------------------------------------------
                        // (CS3) type () { }
                        //    object creation or collection initializer
                        //--------------------------------------------
                        if (CurrentTokenID() == TOKENID.OPENCURLY)
                        {
                            if (PeekToken(1) == TOKENID.IDENTIFIER &&
                                PeekToken(2) == TOKENID.EQUAL)
                            {
                                ParseObjectInitializer(newNode);
                            }
                            else
                            {
                                ParseCollectionInitializer(newNode);
                            }
                        }
                    }
                    break;

                //----------------------------------------------------
                // Similar to ParseType (in the array case) we must build a tree
                // in a top-down fashion to generate the appropriate left-to-right reading
                // of array expressions.
                // However, we must check the first (leftmost) rank specifier
                // for an expression list, which is allowed on a 'new expression'.
                // So, if we don't get a ']' OR ',' parse an expression list and
                // store it in the 'new' node, otherwise we need an initializer.
                //
                // [ の場合。
                //----------------------------------------------------
                case TOKENID.OPENSQUARE:
                    {
                        newNode.OpenParenIndex = CurrentTokenIndex();

                        ARRAYTYPENODE nodeTop = AllocNode(NODEKIND.ARRAYTYPE, newNode) as ARRAYTYPENODE;
                        ARRAYTYPENODE nodeLast = nodeTop;

                        // 最初の次元指定子の引数の式からノード列を作成し、
                        // newNode の ArgumentsNode フィールドにセットする。

                        if (PeekToken() != TOKENID.CLOSESQUARE && PeekToken() != TOKENID.COMMA)
                        {
                            // "[," や "[]" でない場合は "[ 式" とみなして解析する。
                            newNode.ArgumentsNode
                                = ParseDimExpressionList(newNode, out nodeTop.Dimensions);
                        }
                        else
                        {
                            // [, または [] の場合。
                            //ASSERT(!newNode.pArgs);
                            nodeTop.Dimensions = ParseArrayRankSpecifier(newNode, false);
                        }

                        // 2 番目以降の次元指定子がある場合は、
                        // nodeTop を先頭とする ARRAYTYPENODE インスタンスの列を作成し、
                        // newNode の TypeNode フィールドにセットする。

                        while (CurrentTokenID() == TOKENID.OPENSQUARE)
                        {
                            ARRAYTYPENODE nodeT = AllocNode(NODEKIND.ARRAYTYPE, nodeLast) as ARRAYTYPENODE;
                            nodeLast.ElementTypeNode = nodeT;
                            nodeT.Dimensions = ParseArrayRankSpecifier(nodeT, false);
                            //ASSERT(nodeT.iDims > 0);
                            nodeLast = nodeT;
                        }

                        newNode.CloseParenIndex = PeekTokenIndex(-1);
                        // -1 because we skipped the ']'

                        newNode.TypeNode.ParentNode = nodeLast;
                        nodeLast.ElementTypeNode = newNode.TypeNode;
                        newNode.TypeNode = nodeTop;

                        // Check for an initializer.

                        if (CurrentTokenID() == TOKENID.OPENCURLY)
                        {
                            newNode.InitialNode = ParseArrayInitializer(newNode);
                        }
                        else if (newNode.ArgumentsNode == null)
                        {
                            Error(CSCERRID.ERR_MissingArraySize);
                        }
                    }
                    break;

                //----------------------------------------------------
                // (CS3) type {
                //    object creation or collection initializer
                //----------------------------------------------------
                case TOKENID.OPENCURLY: // CS3
                    if (PeekToken(1) == TOKENID.IDENTIFIER &&
                        PeekToken(2) == TOKENID.EQUAL)
                    {
                        ParseObjectInitializer(newNode);
                    }
                    else
                    {
                        ParseCollectionInitializer(newNode);
                    }
                    break;

                default:
                    //------------------------------------
                    // 以上に当てはまらない場合はエラーとする。
                    //------------------------------------
                    Error(CSCERRID.ERR_BadNewExpr);
                    newNode.Flags |= NODEFLAGS.CALL_HADERROR;
                    // the user may have just left the '[]' off of the array initializer.
                    if (CurrentTokenID() == TOKENID.OPENCURLY)
                    {
                        newNode.InitialNode = ParseArrayInitializer(newNode);
                    }
                    break;
            }
            return newNode;
        }

        //------------------------------------------------------------
        // CParser.ParseArgument
        //
        // argument:
        //     expression
        //     "ref" variable-reference
        //     "out" variable-reference
        //
        /// <summary></summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        BASENODE ParseArgument(BASENODE parent)
        {
            NODEFLAGS flags = 0;

            if (CurrentTokenID() == TOKENID.REF)
            {
                flags |= NODEFLAGS.PARMMOD_REF;
                NextToken();
            }
            else if (CurrentTokenID() == TOKENID.OUT)
            {
                flags |= NODEFLAGS.PARMMOD_OUT;
                NextToken();
            }

            BASENODE exprNode = ParseExpression(parent, -1);
            exprNode.Flags |= flags;
            return exprNode;
        }

        //------------------------------------------------------------
        // CParser.ParseArgumentList
        //
        // argument-list:
        // 	argument
        // 	argument-list  ","  argument
        //
        // 名前付き引数は 4.0 以降
        //
        // argument:
        // 	[argument-name]  argument-value
        //
        // argument-name:
        // 	identifier  ":"
        //
        /// <summary>
        /// トークン列から引数リストを表す BASENODE インスタンスのリストを作成し、
        /// 先頭のインスタンスを返す。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        BASENODE ParseArgumentList(BASENODE parent)
        {
            Eat(TOKENID.OPENPAREN);

            CListMaker list = new CListMaker(this);
            int comma = -1;

            if (CurrentTokenID() != TOKENID.CLOSEPAREN)
            {
                while (true)
                {
                    BASENODE argNode = ParseArgument(parent);
                    list.Add(argNode, comma);

                    if (CurrentTokenID() != TOKENID.COMMA)
                    {
                        break;
                    }
                    comma =  CurrentTokenIndex();
                    NextToken();
                }
            }

            Eat(TOKENID.CLOSEPAREN);
            return list.GetList(parent);
        }

        //------------------------------------------------------------
        // CParser.ParseArrayRankSpecifier
        //
        // rank-specifier:
        //     "[" [dim-separators] "]"
        //
        // dim-separators:
        //     ","
        //     dim-separators ","
        //
        /// <summary>
        /// <para>配列の一つの次元指定子から次元を求める。</para>
        /// <para>次元指定子は [ , , ... ] と [ 式, 式 ... ] のどちらでもよい。</para>
        /// <para>いずれの場合も次元を求めるだけだが、処理位置は ] の次へ移動する。</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isDeclaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int ParseArrayRankSpecifier(
            BASENODE parent,
            bool isDeclaration)
        {
            //ASSERT (CurrentTokenID() == TOKENID.OPENSQUARE);
            //if (dims) dims = 0;
            int dims = 0;
            if (CurrentTokenID() != TOKENID.OPENSQUARE)
            {
                return 0;
            }
            bool isError = false;

            while (true)
            {
                NextToken();
                dims++;

                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    if (CurrentTokenID() == TOKENID.CLOSESQUARE)
                    {
                        break;
                    }

                    if (!isError)
                    {
                        Error(isDeclaration ?
                        CSCERRID.ERR_ArraySizeInDeclaration : CSCERRID.ERR_InvalidArray);
                        isError = true;
                    }

                    ParseExpression(parent, -1);
                    if (CurrentTokenID() != TOKENID.COMMA)
                    {
                        break;
                    }
                }
            }

            // Eat the close brace and we're done.
            Eat(TOKENID.CLOSESQUARE);
            return dims;
        }
        //internal BASENODE ParseArrayRankSpecifier(
        //    BASENODE parent,
        //    bool isDeclaration,
        //    out int dims)   // = null);
        //{
        //    //ASSERT (CurrentTokenID() == TOKENID.OPENSQUARE);
        //    //if (dims) dims = 0;
        //    dims = 0;
        //    if (CurrentTokenID() != TOKENID.OPENSQUARE) return null;
        //    bool isError = false;
        //
        //    while (true)
        //    {
        //        NextToken();
        //        dims++;
        //
        //        if (CurrentTokenID() != TOKENID.COMMA)
        //        {
        //            if (CurrentTokenID() == TOKENID.CLOSESQUARE)
        //                break;
        //
        //            if (!isError)
        //            {
        //                Error(isDeclaration ?
        //                CSCERRID.ERR_ArraySizeInDeclaration : CSCERRID.ERR_InvalidArray);
        //                isError = true;
        //            }
        //
        //            ParseExpression(parent, -1);
        //            if (CurrentTokenID() != TOKENID.COMMA)
        //                break;
        //        }
        //    }
        //
        //    // Eat the close brace and we're done.
        //    Eat(TOKENID.CLOSESQUARE);
        //    return null;
        //}

        //------------------------------------------------------------
        // CParser.ParseDimExpressionList
        //
        /// <summary>
        /// [ 式 , 式, ... ] に対して、ノード列（各ノードは式に対応する）を作成する。
        /// </summary>
        /// <param name="parent">親となるノード。</param>
        /// <param name="dims">次元がセットされる。</param>
        /// <returns>ノード列の先頭。</returns>
        //------------------------------------------------------------
        internal BASENODE ParseDimExpressionList(BASENODE parent, out int dims)  // = null);
        {
            //ASSERT (CurrentTokenID() == TOKENID.OPENSQUARE);

            dims = 0;
            CListMaker list = new CListMaker(this);
            int comma = -1;

            while (true)
            {
                NextToken();

                BASENODE argNode;
                if (CurrentTokenID() == TOKENID.COMMA || CurrentTokenID() == TOKENID.CLOSESQUARE)
                {
                    Error(CSCERRID.ERR_ValueExpected);
                    argNode = ParseMissingName(parent, CurrentTokenIndex());
                }
                else
                {
                    argNode = ParseExpression(parent, -1);
                }
                list.Add(argNode, comma);

                ++dims;
                if (CurrentTokenID() != TOKENID.COMMA)
                {
                    break;
                }
                comma = CurrentTokenIndex();
            }

            // Eat the close brace and we're done
            Eat(TOKENID.CLOSESQUARE);
            return list.GetList(parent);
        }

        //------------------------------------------------------------
        // CParser.ParseExpressionList
        //
        // statement-expression-list:
        //     statement-expression
        //     statement-expression-list  ","  statement-expression
        //
        /// <summary>
        /// <para>Parse a list of expressions separated by commas,
        /// terminated by semicolon or close-paren (currently only called from ParseForStatement).
        /// The terminating token is NOT consumed.</para>
        /// <para>statement-expression-list を解析して、BASENODE インスタンスの列を作成する。</para>
        /// </summary>
        /// <param name="parentNode">親となるノード。</param>
        /// <returns>作成した BASENODE インスタンス列の先頭。</returns>
        //------------------------------------------------------------
        internal BASENODE ParseExpressionList(BASENODE parentNode)
        {
            //BASENODE argsNode;
            CListMaker list = new CListMaker(this);
            int comma = -1;

            if (CurrentTokenID() != TOKENID.CLOSEPAREN && CurrentTokenID() != TOKENID.SEMICOLON)
            {
                while (true)
                {
                    list.Add(ParseExpression(parentNode, -1), comma);
                    if (CurrentTokenID() != TOKENID.COMMA)
                    {
                        break;
                    }
                    comma = CurrentTokenIndex();
                    NextToken();
                }
            }

            // we're done
            return list.GetList(parentNode);
        }

        // Extent/Leaf calculators

        //------------------------------------------------------------
        // CParser.GetExtent
        //
        //internal int GetExtent(
        //    CBasenodeLookupCache lastTokenCache,
        //    BASENODE node,
        //    ExtentFlags flags,
        //    POSDATA posStart,
        //    POSDATA posEnd);
        //
        /// <summary></summary>
        /// <param name="cache"></param>
        /// <param name="node"></param>
        /// <param name="flags"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetExtent(
            CBasenodeLookupCache cache,
            BASENODE node,
            ExtentFlags flags,
            POSDATA posStart,
            POSDATA posEnd)
        {
            bool missingName;

            if (posStart != null)
            {
                int tokenIndex = GetFirstToken(node, flags, out missingName);

                if (tokenIndex < 0)
                {
                    posStart.SetZero();
                    return false;
                }

                if (tokenIndex >= tokenCount)
                {
                    POSDATA posEOF = (tokenCount >= 1) ?
                        tokenArray[tokenCount - 1] : new POSDATA(0, 0);
                    posStart.CopyFrom(posEOF);
                    return false;
                }

                //ASSERT (tokenIndex >= 0 && tokenIndex < m_tokenIndexens);
                posStart.CopyFrom(tokenArray[tokenIndex]);
                if (missingName)
                {
                    // The first token in this span was the "missing name" node.  So, the
                    // start position of that token is the first character FOLLOWING the
                    // token at the specified index.  Note that we only need to rescan the
                    // token for that if it isn't a 'fixed' token.

                    if (TokenInfoArray[(int)tokenArray[tokenIndex].TokenID].Length == 0)
                    {
                        posStart.CopyFrom(tokenArray[tokenIndex].StopPosition());
                    }
                    else
                    {
                        posStart.CharIndex += TokenInfoArray[(int)tokenArray[tokenIndex].TokenID].Length;
                    }
                }
            }

            if (posEnd != null)
            {
                int tokenIndex = GetLastToken(cache, node, flags, out missingName);

                if (tokenIndex < 0)
                {
                    posEnd.SetZero();
                    return false;   // E_FAIL;
                }

                // If the last token in the node exceeds teh number of tokens we have, 
                // then the end extent is the end of the file.
                if (tokenIndex >= tokenCount)
                {
                    POSDATA posEOF = (tokenCount >= 1) ?
                        tokenArray[tokenCount - 1] : new POSDATA(0, 0);
                    posEnd.CopyFrom(posEOF);
                    return false;   // E_FAIL;
                }

                //ASSERT (tokenIndex >= 0 && tokenIndex < m_tokenIndexens);

                posEnd.CopyFrom(tokenArray[tokenIndex].StopPosition());
                if (missingName)
                {
                    int nextNonNoisy = this.PeekTokenIndexFrom(tokenIndex, 1);
                    if (nextNonNoisy > tokenIndex && nextNonNoisy < tokenCount)
                    {
                        posEnd.CopyFrom(tokenArray[nextNonNoisy]);
                    }
                    // Adjust the end of this span to include all trailing whitespace.  The
                    // token found is the "missing name" token, whose index is actually
                    // the token PRECEDING the intended identifier.
                    //   while ((sourceText + lineList[posEnd.iLine])[posEnd.iChar] == ' ' ||
                    //          (sourceText + lineList[posEnd.iLine])[posEnd.iChar] == '\t')
                    //       posEnd.iChar++;
                }
            }
            return true;    // S_OK;
        }

        //------------------------------------------------------------
        // CParser.GetTokenExtent
        //
        /// <summary>
        /// <para>Not implemented.</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal void GetTokenExtent(BASENODE node, out int first, out int last, ExtentFlags flags)
        {
            throw new NotImplementedException();
        }

        //------------------------------------------------------------
        // CParser.FindContainingNodeInChain
        //
        /// <summary>
        /// chain から始まるノードリスト内で、
        /// pos の位置にある（または最も近い）ノードを求める。
        /// </summary>
        /// <param name="lastTokenCache"></param>
        /// <param name="pos"></param>
        /// <param name="chain"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE FindContainingNodeInChain(
            CBasenodeLookupCache lastTokenCache,
            POSDATA pos,
            BASENODE chain,
            ExtentFlags flags)
        {
            // It's prettyu costly to keep adding individual elements to an atlarray
            // so we determine the total number of elements, and preset the size of the
            // array accordingly.
            int nodeCount = 0;
            CChainIterator iterator = new CChainIterator(chain);
            while (iterator.MoveNext())
            {
                nodeCount++;
            }

            List<BASENODE> nodes = new List<BASENODE>();
            iterator.Reset();
            int i = 0;
            while (iterator.MoveNext())
            {
                nodes[i++] = iterator.Current();
            }

            CContainingNodeFinder finder =
                new CContainingNodeFinder(this, lastTokenCache, pos, nodes, flags);
            return finder.Do();
        }

        //------------------------------------------------------------
        // CParser.FindLeaf
        //
        /// <summary></summary>
        /// <param name="pos"></param>
        /// <param name="pNode"></param>
        /// <param name="pData"></param>
        /// <param name="ppTree"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE FindLeaf(
            POSDATA pos,
            BASENODE pNode,
            CSourceData pData,
            //ICSInteriorTree **ppTree)
            out CInteriorTree ppTree)
        {
            CBasenodeLookupCache cache = new CBasenodeLookupCache();
            return FindLeafEx(cache, pos, pNode, pData, ExtentFlags.FULL, out ppTree);
        }
        //BASENODE* FindLeaf(
        //    const POSDATA &pos,
        //    BASENODE *pNode,
        //    CSourceData *pData,
        //    ICSInteriorTree **ppTree);


        //------------------------------------------------------------
        // CParser.FindLeafIgnoringTokenStream
        //
        /// <summary></summary>
        /// <param name="pos"></param>
        /// <param name="node"></param>
        /// <param name="source"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE FindLeafIgnoringTokenStream(
            POSDATA pos,
            BASENODE node,
            //List<BASENODE> treeNodeList,
            CSourceData source,
            //ICSInteriorTree **tree)
            out CInteriorTree tree)
        {
            CBasenodeLookupCache lastTokenCache = new CBasenodeLookupCache();
            return FindLeafEx(
                lastTokenCache,
                pos,
                node,
                source,
                ExtentFlags.IGNORE_TOKEN_STREAM,
                out tree);
        }

        //------------------------------------------------------------
        // CParser.FindLeafEx
        //
        /// <summary>
        /// currentNode の種類に応じて
        ///  METHOD、OPERATOR、CTOR、DTOR なら currentNode（取得できなかった場合）
        ///  ACCESSOR なら currentNode（sourceData == null の場合）
        ///  PREDEFINEDTYPE なら currentNode
        ///  OP、CONSTVAL なら currentNode
        /// leafNode が取得できなかった場合は currentNode
        /// </summary>
        /// <param name="lastTokenCache"></param>
        /// <param name="currentPos"></param>
        /// <param name="currentNode"></param>
        /// <param name="sourceData"></param>
        /// <param name="flags"></param>
        /// <param name="ppTree"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE FindLeafEx(
            CBasenodeLookupCache lastTokenCache,
            POSDATA currentPos,
            BASENODE currentNode, // List<BASENODE> を使う
            //List<BASENODE> treeNodeList,
            CSourceData sourceData,
            ExtentFlags flags,
            //ICSInteriorTree** ppTree)
            out CInteriorTree ppTree)
        {
            ppTree = null;
            // Check here rather than at every call site...
            if (currentNode == null)
            {
                return null;
            }
            ppTree = null;

            BASENODE leafNode = null;
            BASENODE possibleLeaf = null;

            POSDATA posStart = new POSDATA();
            POSDATA posEnd = new POSDATA();
            int loopCount = 0;

            // If this is an intrinsically chained node, run the chain until we
            // find one that "contains" the given position
            // currentNode が鎖状に連結されている場合は
            // その中から currentPos に対応するノードで置き換える。
            if (currentNode.IsStatement || currentNode.InGroup(NODEGROUP.MEMBER))
            {
                // Search this chain (might just be currentNode)
                currentNode = FindContainingNodeInChain(
                    lastTokenCache,
                    currentPos,
                    currentNode,
                    flags);
            }

            // posStart と posEnd を currentNode を含むプログラム要素の範囲に設定する。
            if (currentNode != null)
            {
                POSDATA posEOF =
                    (tokenCount >= 1) ? tokenArray[tokenCount - 1] : new POSDATA(0, 0);

                // If this node doesn't "contain" this position, we can bail here.
                GetExtent(lastTokenCache, currentNode, flags, posStart, posEnd);

                // Fix up posStart and posEnd if GetExtent failed for some reason
                // See the comment in srcmod.cpp:
                if (posStart.IsUninitialized)
                {
                    posStart = new POSDATA(0, 0);
                }

                if (posEnd.IsUninitialized)
                {
                    posEnd = posEOF;
                }

                bool fAtEOF = (posStart == posEOF) && (posEnd == posEOF);
                if (fAtEOF || currentPos < posStart || currentPos > posEnd)
                {
                    currentNode = null;
                }
            }

            if (currentNode == null)
            {
                return null;
            }

            if (posStart > posEnd)
            {
                DebugUtil.VsFail("Invalid extent found!");
                return null;
            }


        REDO:
            switch (currentNode.Kind)
            {
                case NODEKIND.NAMESPACE:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NAMESPACENODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NAMESPACENODE).UsingNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NAMESPACENODE).GlobalAttributeNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NAMESPACENODE).ElementsNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.USING:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as USINGNODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as USINGNODE).AliasNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.CLASS:
                case NODEKIND.ENUM:
                case NODEKIND.INTERFACE:
                case NODEKIND.STRUCT:
                    {
                        AGGREGATENODE pClass = currentNode.AsAGGREGATE;

                        if ((leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                pClass.TypeParametersNode,
                                sourceData,
                                flags,
                                out ppTree)) == null &&
                            (leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                pClass.ConstraintsNode,
                                sourceData,
                                flags,
                                out ppTree)) == null &&
                            (leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                pClass.BasesNode,
                                sourceData,
                                flags,
                                out ppTree)) == null &&
                            (leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                pClass.MembersNode,
                                sourceData,
                                flags,
                                out ppTree)) == null)
                        {
                            possibleLeaf = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                pClass.AttributesNode,
                                sourceData,
                                flags,
                                out ppTree);

                            if ((leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    pClass.NameNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null
                                ||
                                (leafNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                            {
                                if (possibleLeaf != null)
                                {
                                    leafNode = possibleLeaf;
                                }
                            }
                        }
                        break;
                    }

                case NODEKIND.DELEGATE:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DELEGATENODE).TypeParametersNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DELEGATENODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DELEGATENODE).TypeParametersNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DELEGATENODE).ParametersNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DELEGATENODE).ConstraintsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DELEGATENODE).AttributesNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.MEMBER:
                    DebugUtil.Assert(false);
                    return null;

                case NODEKIND.ENUMMBR:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as ENUMMBRNODE).ValueNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as ENUMMBRNODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as ENUMMBRNODE).AttributesNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.CONST:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsCONST.DeclarationsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsCONST.TypeNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsCONST.AttributesNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.FIELD:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as FIELDNODE).DeclarationsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as FIELDNODE).TypeNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as FIELDNODE).AttributesNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.METHOD:
                case NODEKIND.OPERATOR:
                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                    {
                        METHODBASENODE methodNode = currentNode.AsANYMETHOD;

                        if ((methodNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0 &&
                            currentPos >= tokenArray[methodNode.OpenIndex])
                        {
                            if (sourceData == null) return methodNode;

                            //ASSERT(*ppTree == null);

                            GetInteriorTree(sourceData, methodNode, ref ppTree);
                            leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                methodNode.BodyNode,
                                sourceData,
                                flags,
                                out ppTree);
                            return leafNode != null ? leafNode : currentNode;
                        }

                        if (methodNode.Kind == NODEKIND.CTOR)
                        {
                            // For NK_CTOR nodes, there is no name, but constructor arguments to
                            // this/base, which FOLLOW the arguments to the constructor itself,
                            // so the order must be this:
                            if ((leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    (methodNode as CTORMETHODNODE).ThisOrBaseCallNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null &&
                                (leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.ParametersNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null &&
                                (leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.ReturnTypeNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null)
                            {
                                leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.AttributesNode,
                                    sourceData,
                                    flags,
                                    out ppTree);
                            }
                        }
                        else if (methodNode.Kind == NODEKIND.METHOD)
                        {
                            // Methods have names, which come before the parameters.
                            if ((leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    (methodNode as METHODNODE).NameNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null &&
                                (leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.ParametersNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null &&
                                (leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    (methodNode as METHODNODE).ConstraintsNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null &&
                                (leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.ReturnTypeNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null)
                            {
                                leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.AttributesNode,
                                    sourceData,
                                    flags,
                                    out ppTree);
                            }
                        }
                        else
                        {
                            // Operators and destructors have no name and no constructor args.
                            // ASSERT(methodNode.kind == NK_OPERATOR || methodNode.kind == NK_DTOR);
                            if ((leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.ReturnTypeNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null &&
                                (leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.ParametersNode,
                                    sourceData,
                                    flags,
                                    out ppTree)) == null)
                            {
                                leafNode = FindLeafEx(
                                    lastTokenCache,
                                    currentPos,
                                    methodNode.AttributesNode,
                                    sourceData,
                                    flags,
                                    out ppTree);
                            }
                        }

                        if (leafNode != null)
                        {
                            return leafNode;
                        }
                        break;
                    }

                case NODEKIND.PROPERTY:
                case NODEKIND.INDEXER:
                    {
                        PROPERTYNODE propNode = currentNode.AsANYPROPERTY;

                        if ((leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                propNode.GetNode,
                                sourceData,
                                flags,
                                out ppTree)) == null &&
                            (leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                propNode.SetNode,
                                sourceData,
                                flags,
                                out ppTree)) == null &&
                            (leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                propNode.ParametersNode,
                                sourceData,
                                flags,
                                out ppTree)) == null &&
                            (leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                propNode.NameNode,
                                sourceData,
                                flags,
                                out ppTree)) == null &&
                            (leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                propNode.TypeNode,
                                sourceData,
                                flags,
                                out ppTree)) == null)
                        {
                            leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                propNode.AttributesNode,
                                sourceData,
                                flags,
                                out ppTree);
                        }
                        break;
                    }

                case NODEKIND.ACCESSOR:
                    {
                        // Accessors are interior nodes.  If we weren't given a source data, we can't
                        // build an interior tree so return this node.
                        if (sourceData == null)
                            return currentNode;

                        GetInteriorTree(sourceData, currentNode, ref ppTree);
                        if ((leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                (currentNode as ACCESSORNODE).BodyNode,
                                sourceData,
                                flags,
                                out ppTree)) == null)
                        {
                            leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                (currentNode as ACCESSORNODE).AttributesNode,
                                sourceData,
                                flags,
                                out ppTree);
                        }
                        break;
                    }

                case NODEKIND.PARAMETER:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as PARAMETERNODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree)) == null
                        || (leafNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                    {
                        if ((leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                (currentNode as PARAMETERNODE).TypeNode,
                                sourceData,
                                flags,
                                out ppTree)) == null
                            || (leafNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                        {
                            //cyrusn  4/2/03
                            //if the user has written:
                            // public void foo([    //<-- cursor is after the [
                            //we want to return the attribute declaration node, not the missing name node
                            //
                            //If there isn't a name node, or the name node is missing
                            //use the attribute node if we can find one.
                            possibleLeaf = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                (currentNode as PARAMETERNODE).AttributesNode,
                                sourceData,
                                flags,
                                out ppTree);
                            if (possibleLeaf != null)
                            {
                                leafNode = possibleLeaf;
                            }
                        }
                    }
                    break;

                case NODEKIND.NESTEDTYPE:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as NESTEDTYPENODE).TypeNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.PARTIALMEMBER:
                    possibleLeaf = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as PARTIALMEMBERNODE).AttributesNode,
                        sourceData,
                        flags,
                        out ppTree);
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as PARTIALMEMBERNODE).Node,
                        sourceData,
                        flags,
                        out ppTree);
                    if (leafNode == null ||
                        (leafNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                    {
                        //If the name is missing, try to find the attribute.
                        //If we're not able to, then default to the missing name
                        if (possibleLeaf != null)
                        {
                            leafNode = possibleLeaf;
                        }
                    }
                    break;

                case NODEKIND.GENERICNAME:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as GENERICNAMENODE).ParametersNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.CONSTRAINT:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as CONSTRAINTNODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as CONSTRAINTNODE).BoundsNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.PREDEFINEDTYPE:
                    //
                    // PREDEFINEDTYPE の場合、currentNode を leafNode とする。
                    //
                    leafNode = currentNode;
                    break;

                case NODEKIND.NAMEDTYPE:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as NAMEDTYPENODE).NameNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.OPENTYPE:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        currentNode.AsOPENTYPE.NameNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.POINTERTYPE:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as POINTERTYPENODE).ElementTypeNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.NULLABLETYPE:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as NULLABLETYPENODE).ElementTypeNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.ARRAYTYPE:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as ARRAYTYPENODE).ElementTypeNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.BLOCK:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as BLOCKNODE).StatementsNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.ANONBLOCK:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as ANONBLOCKNODE).ArgumentsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as ANONBLOCKNODE).BodyNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.BREAK:
                case NODEKIND.CONTINUE:
                case NODEKIND.EXPRSTMT:
                case NODEKIND.GOTO:
                case NODEKIND.RETURN:
                case NODEKIND.YIELD:
                case NODEKIND.THROW:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        currentNode.AsANYEXPRSTMT.ArgumentsNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.CHECKED:
                case NODEKIND.LABEL:
                case NODEKIND.UNSAFE:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYLABELSTMT.StatementNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYLABELSTMT.LabelNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.DO:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsDO.ExpressionNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsDO.StatementNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.LOCK:
                case NODEKIND.WHILE:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYLOOPSTMT.StatementNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYLOOPSTMT.ExpressionNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.FOR:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as FORSTMTNODE).InitialNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as FORSTMTNODE).ExpressionNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as FORSTMTNODE).IncrementNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as FORSTMTNODE).StatementNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.IF:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as IFSTMTNODE).ConditionNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as IFSTMTNODE).StatementNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as IFSTMTNODE).ElseNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.DECLSTMT:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DECLSTMTNODE).VariablesNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as DECLSTMTNODE).TypeNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.SWITCH:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as SWITCHSTMTNODE).CasesNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as SWITCHSTMTNODE).ExpressionNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.CASE:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as CASENODE).StatementsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as CASENODE).LabelsNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.TRY:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as TRYSTMTNODE).CatchNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as TRYSTMTNODE).BlockNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.CATCH:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as CATCHNODE).BlockNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as CATCHNODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as CATCHNODE).TypeNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.ARRAYINIT:
                case NODEKIND.CASELABEL:
                case NODEKIND.UNOP:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        currentNode.AsANYUNOP.Operand,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.CALL:
                case NODEKIND.DEREF:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYBINOP.Operand2,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYBINOP.Operand1,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.BINOP:
                    //If they're looking for possible generic, then we have to try searching down 
                    //both sides of a binop.  
                    if ((flags & ExtentFlags.POSSIBLE_GENERIC_NAME) != 0)
                    {
                        if ((leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                currentNode.AsANYBINOP.Operand1,
                                sourceData,
                                flags,
                                out ppTree)) == null)
                        {
                            leafNode = FindLeafEx(
                                lastTokenCache,
                                currentPos,
                                currentNode.AsANYBINOP.Operand2,
                                sourceData,
                                flags,
                                out ppTree);
                        }
                        break;
                    }
                    //fallthrough;
                    goto case NODEKIND.DOT;

                case NODEKIND.ARROW:
                case NODEKIND.LIST:
                case NODEKIND.DOT:
                    {
                        BINOPNODE opNode = currentNode.AsANYBINOP;
                        if (loopCount++ > 0x01000000)
                        {
                            //VSFAIL("Expression too complex for FindLeafEx");
                            break; // Expression too complex
                        }
                        if (opNode.Operand2 != null)
                        {
                            //ASSERT(!b.p2.IsStatement() && !b.p2.InGroup(NODEGROUP.MEMBER));
                            POSDATA savedStart = posStart;
                            // Don't re-get (or test) the ending position because it's the same as mine
                            POSDATA posTemp = new POSDATA();
                            if (!GetExtent(lastTokenCache, opNode.Operand2, flags, posStart, posTemp))
                            {
                                return null;
                            }
                            if (currentPos >= posStart)
                            {
                                //ASSERT(currentPos <= posEnd);
                                currentNode = opNode.Operand2;
                                goto REDO;
                            }
                            posStart = savedStart;
                        }
                        if (opNode.Operand1 != null)
                        {
                            //ASSERT(!opNode.p1.IsStatement() && !b.p1.InGroup(NODEGROUP.MEMBER));
                            // Don't re-get (or test) the starting position because it's the same as mine
                            POSDATA posTemp = new POSDATA();
                            if (!GetExtent(lastTokenCache, opNode.Operand1, flags, posTemp, posEnd))
                            {
                                return null;
                            }
                            if (currentPos <= posEnd)
                            {
                                // We can't really assert this if the caller asks us to ignore the
                                // token stream, because the positions may be wrong.
                                // See the comment in srcmod.cpp:
                                //VSIMPLIES((flags & EF_IGNORE_TOKEN_STREAM) == 0,
                                //           currentPos >= posStart, "Wrong position.");
                                currentNode = opNode.Operand1;
                                goto REDO;
                            }
                        }
                    }
                    break;

                case NODEKIND.OP:
                case NODEKIND.CONSTVAL:
                    leafNode = currentNode;
                    break;

                case NODEKIND.VARDECL:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as VARDECLNODE).ArgumentsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as VARDECLNODE).NameNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.NEW:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NEWNODE).InitialNode,
                            sourceData,
                            flags,
                            out ppTree)) == null &&
                        (leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NEWNODE).ArgumentsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NEWNODE).TypeNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.ATTR:
                case NODEKIND.ATTRARG:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYATTR.ArgumentsNode,
                            sourceData,
                            flags,
                            out ppTree)) == null)
                    {
                        leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            currentNode.AsANYATTR.NameNode,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    break;

                case NODEKIND.ATTRDECL:
                    leafNode = FindLeafEx(
                        lastTokenCache,
                        currentPos,
                        (currentNode as ATTRDECLNODE).AttributesNode,
                        sourceData,
                        flags,
                        out ppTree);
                    break;

                case NODEKIND.TYPEWITHATTR:
                    if ((leafNode = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as TYPEWITHATTRNODE).TypeBaseNode,
                            sourceData,
                            flags,
                            out ppTree)) == null
                        || (leafNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                    {
                        //if the user has written:
                        // public class foo<[    //<-- cursor is after the [
                        //we want to return the attribute declaration node, not the missing name node
                        //
                        //If there isn't a name node, or the name node is missing
                        //use the attribute node if we can find one.
                        possibleLeaf = FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as TYPEWITHATTRNODE).AttributesNode,
                            sourceData,
                            flags,
                            out ppTree);
                        if (possibleLeaf != null)
                        {
                            leafNode = possibleLeaf;
                        }
                    }
                    break;

                case NODEKIND.NAME:
                    if ((flags & ExtentFlags.POSSIBLE_GENERIC_NAME) != 0 &&
                        (currentNode as NAMENODE).PossibleGenericName != null)
                    {
                        return FindLeafEx(
                            lastTokenCache,
                            currentPos,
                            (currentNode as NAMENODE).PossibleGenericName,
                            sourceData,
                            flags,
                            out ppTree);
                    }
                    else
                    {
                        break;
                    }

                default:
                    break;
            }

            if (leafNode == null)
            {
                leafNode = currentNode;
            }
            return leafNode;
        }

        //------------------------------------------------------------
        // CParser.FindLeafNodeForTokenEx
        //------------------------------------------------------------
        //int FindLeafNodeForTokenEx(
        //    int iToken,
        //    BASENODE* pNode,
        //    CSourceData* pData,
        //    ExtentFlags flags,
        //    BASENODE** ppNode,
        //    ICSInteriorTree** ppTree);

        //------------------------------------------------------------
        // CParser.GetFirstToken
        //
        /// <summary>
        /// Return the first token of the program element which includes the given node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="flags"></param>
        /// <param name="missingName"></param>
        /// <returns>Index of the first node of the program element.</returns>
        //------------------------------------------------------------
        internal int GetFirstToken(BASENODE node, ExtentFlags flags, out bool missingName)
        {
            missingName = false;
        REDO:
            if (node == null)
            {
                DebugUtil.Assert(false, "null node given to GetFirstToken!!!");
                return unchecked((int)0xf0000000);
            }

            switch (node.Kind)
            {
                case NODEKIND.ACCESSOR:
                    {
                        ACCESSORNODE accessorNode = node as ACCESSORNODE;
                        if (accessorNode.AttributesNode != null)
                        {
                            // If this node has attributes, return the first index of the first attribute.
                            return GetFirstToken(accessorNode.AttributesNode, flags, out missingName);
                        }
                        else
                        {
                            return node.TokenIndex;
                        }
                    }
                //break;

                case NODEKIND.BINOP:
                    // Special case the check for a cast -- the tokidx of the OPERATOR.CAST binop
                    // node has the token index of the open paren
                    if (node.Operator == OPERATOR.CAST)
                    {
                        return node.TokenIndex;
                    }
                    // fall through...
                    goto case NODEKIND.LIST;

                case NODEKIND.CALL:
                case NODEKIND.DEREF:
                case NODEKIND.ARROW:
                case NODEKIND.DOT:
                case NODEKIND.LIST:
                    node = node.AsANYBINOP.Operand1;
                    goto REDO;

                case NODEKIND.UNOP:
                    if (node.Operator == OPERATOR.POSTINC || node.Operator == OPERATOR.POSTDEC)
                    {
                        return GetFirstToken((node as UNOPNODE).Operand, flags, out missingName);
                    }
                    return node.TokenIndex;

                case NODEKIND.PREDEFINEDTYPE:
                    return node.TokenIndex;

                case NODEKIND.NAMEDTYPE:
                    return GetFirstToken((node as NAMEDTYPENODE).NameNode, flags, out missingName);

                case NODEKIND.OPENTYPE:
                    return GetFirstToken(node.AsOPENTYPE.NameNode, flags, out missingName);

                case NODEKIND.ARRAYTYPE:
                    return GetFirstToken((node as ARRAYTYPENODE).ElementTypeNode, flags, out missingName);

                case NODEKIND.POINTERTYPE:
                    return GetFirstToken((node as POINTERTYPENODE).ElementTypeNode, flags, out missingName);

                case NODEKIND.NULLABLETYPE:
                    return GetFirstToken((node as NULLABLETYPENODE).ElementTypeNode, flags, out missingName);

                case NODEKIND.NAMESPACE:
                    if (node.TokenIndex == -1)
                    {
                        return 0;
                    }
                    return node.TokenIndex;

                case NODEKIND.NAME:
                case NODEKIND.ALIASNAME:
                    missingName = ((node.Flags & NODEFLAGS.NAME_MISSING) != 0);
                    return node.TokenIndex;

                case NODEKIND.NESTEDTYPE:
                    return GetFirstToken((node as NESTEDTYPENODE).TypeNode, flags, out missingName);

                case NODEKIND.DO:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        bool fMissing = false;
                        return PeekTokenIndexFromEx(
                            GetFirstToken(node.AsDO.ExpressionNode, flags, out fMissing), flags, -2);
                        // Include 'while ('
                    }
                    else
                    {
                        goto default;
                    }

                case NODEKIND.DECLSTMT:
                    if ((node.Flags & NODEFLAGS.CONST_DECL) != 0)
                    {
                        return PeekTokenIndexFromEx(node.TokenIndex, flags, -1);
                    }
                    else
                    {
                        goto default;
                    }

                case NODEKIND.OP:
                    //The parser wasn't able to read in anything successfully.
                    //So it will have read off past the current token to the next
                    //
                    //Like for (int ^
                    //We want to be considered in the "init" part of the for
                    if (node.Operator == OPERATOR.NOP && node.TokenIndex > 0)
                    {
                        return PeekTokenIndexFromEx(node.TokenIndex, flags, -1);
                    }
                    else
                    {
                        goto default;
                    }

                case NODEKIND.CONSTRAINT:
                    {
                        //need to consume the "where" token
                        int prevToken = PeekTokenIndexFromEx(node.TokenIndex, flags, -1);

                        if (prevToken < 0 || prevToken >= tokenCount)
                        {
                            // This should only happen if we ignore the token stream (i.e. if the caller
                            // knows the parse tree might be outdated).
                            //VSASSERT((flags & EF_IGNORE_TOKEN_STREAM) == EF_IGNORE_TOKEN_STREAM,
                            //    "How come we don't have a valid token if we're not ignoring the token stream?");
                            goto default;
                        }
                        else
                        {
                            // Ok, the prev index is valid. Check we have one the "where" token
                            if (CheckForName(prevToken, SpecName(SPECIALNAME.WHERE)))
                            {
                                return prevToken;
                            }
                            else
                            {
                                goto default;
                            }
                        }
                    }

                case NODEKIND.ATTRARG:
                    {
                        ATTRNODE attrNode = node.AsANYATTR;
                        if (attrNode.NameNode != null)
                        {
                            return GetFirstToken(attrNode.NameNode, flags, out missingName);
                        }
                        else if (attrNode.ArgumentsNode != null)
                        {
                            return GetFirstToken(attrNode.ArgumentsNode, flags, out missingName);
                        }
                        else
                        {
                            goto default;
                        }
                    }

                case NODEKIND.ATTR:
                    {
                        ATTRNODE attrNode = node as ATTRNODE;
                        if (attrNode.NameNode != null)
                        {
                            return GetFirstToken(attrNode.NameNode, flags, out missingName);
                        }
                        else
                        {
                            goto default;
                        }
                    }

                case NODEKIND.PARAMETER:
                    {
                        PARAMETERNODE paramNode = node as PARAMETERNODE;

                        BASENODE parentMethod = paramNode.ParentNode;
                        while (parentMethod != null && parentMethod.Kind == NODEKIND.LIST)
                        {
                            parentMethod = parentMethod.ParentNode;
                        }

                        bool fIsLastParam =
                            (paramNode.ParentNode.Kind != NODEKIND.LIST) ||
                            ((paramNode.ParentNode.Kind == NODEKIND.LIST) &&
                            (paramNode.ParentNode.AsLIST.Operand2 == paramNode));

                        if ((paramNode.Flags & NODEFLAGS.PARMMOD_REF) != 0 ||
                            (paramNode.Flags & NODEFLAGS.PARMMOD_OUT) != 0 ||
                            (fIsLastParam && (parentMethod != null) &&
                            (parentMethod.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0))
                        {
                            //we had an out/ref/params flag.  Stick to the default
                            goto default;
                        }
                        else if (paramNode.AttributesNode != null)
                        {
                            return GetFirstToken(paramNode.AttributesNode, flags, out missingName);
                        }
                        else
                        {
                            return GetFirstToken(paramNode.TypeNode, flags, out missingName);
                        }
                    }

                default:
                    //DEFAULT:
                    // MOST nodes' token index values are the first for their construct
                    //ASSERT(node.tokidx != -1);
                    // If you hit this, you'll either need to fix the parser or special-case this node type!
                    return node.TokenIndex;
            }
        }

        //------------------------------------------------------------
        // CParser.GetLastToken
        //
        /// <summary></summary>
        /// <param name="lastTokenCache"></param>
        /// <param name="node"></param>
        /// <param name="flags"></param>
        /// <param name="missingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetLastToken(
            CBasenodeLookupCache lastTokenCache,
            BASENODE node,
            ExtentFlags flags,
            out bool missingName)
        {
            return GetLastTokenWorker(lastTokenCache, node, flags, out missingName);
        }

        //int GetLastTokenWorker(
        //  CBasenodeLookupCache& lastTokenCache,
        //  BASENODE *pNode,
        //  ExtentFlags flags,
        //  bool *pfMissingName);

        //------------------------------------------------------------
        // CParser.GetLastTokenWorker
        //
        /// <summary></summary>
        /// <param name="lastTokenCache"></param>
        /// <param name="node"></param>
        /// <param name="flags"></param>
        /// <param name="missingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetLastTokenWorker(
            CBasenodeLookupCache lastTokenCache,
            BASENODE node,
            ExtentFlags flags,
            out bool missingName)
        {
            missingName = false;

        REDO:
            if (node == null)
            {
                DebugUtil.VsFail("null node given to GetLastToken!!!");
                return unchecked((int)0xf0000000);
            }

            switch (node.Kind)
            {
                case NODEKIND.ACCESSOR:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        return (node as ACCESSORNODE).OpenCurlyIndex;
                    }
                    return (node as ACCESSORNODE).CloseCurlyIndex;

                case NODEKIND.ARRAYINIT:
                    // Add one for the close curly
                    if (node.AsARRAYINIT.Operand == null)
                    {
                        return IncludeCloseCurly(node.TokenIndex, flags, out missingName);
                    }
                    else
                    {
                        return IncludeCloseCurly(
                            IncludeComma(
                                GetLastToken(lastTokenCache, node.AsARRAYINIT.Operand, flags, out missingName),
                                flags,
                                out missingName),
                            flags,
                            out missingName);
                    }

                case NODEKIND.CALL:
                case NODEKIND.DEREF:
                    if (node.AsANYCALL.Operand2 == null)
                    {
                        return IncludeCloseParenOrCloseSquare(node.TokenIndex, flags, out missingName);
                        // Get to the ) or ] in empty arg list
                    }
                    return IncludeCloseParenOrCloseSquare(
                        GetLastToken(lastTokenCache, node.AsANYCALL.Operand2, flags, out missingName),
                        flags,
                        out missingName); // Gets the ) or ]

                case NODEKIND.BINOP:
                    if ((flags & ExtentFlags.POSSIBLE_GENERIC_NAME) != 0)
                    {
                        return Math.Max(
                            GetLastToken(
                                lastTokenCache,
                                node.AsANYBINOP.Operand1,
                                flags,
                                out missingName),
                            GetLastToken(
                                lastTokenCache,
                                node.AsANYBINOP.Operand2,
                                flags,
                                out missingName));
                    }
                    //fallthrough
                    goto case NODEKIND.LIST;

                case NODEKIND.ARROW:
                case NODEKIND.DOT:
                case NODEKIND.LIST:
                    node = node.AsANYBINOP.Operand2;
                    goto REDO;

                case NODEKIND.ATTR:
                    if ((node as ATTRNODE).CloseParenIndex != -1)
                    {
                        return (node as ATTRNODE).CloseParenIndex;
                    }
                    //fallthrough ...
                    goto case NODEKIND.ATTRARG;

                case NODEKIND.ATTRARG:
                    if (node.AsANYATTR.ArgumentsNode != null)
                    {
                        node = node.AsANYATTR.ArgumentsNode;
                    }
                    else
                    {
                        node = node.AsANYATTR.NameNode;
                    }
                    goto REDO;

                case NODEKIND.ATTRDECL:
                    return (node as ATTRDECLNODE).CloseSquareIndex;

                case NODEKIND.BLOCK:
                    return (node as BLOCKNODE).CloseCurlyIndex;

                case NODEKIND.ANONBLOCK:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        // For debug info we only want everything up to the open-curly
                        // which is either the first token, or iClose

                        // -1 means no parens
                        if ((node as ANONBLOCKNODE).CloseParenIndex == -1)
                        {
                            return node.TokenIndex;
                        }
                        return (node as ANONBLOCKNODE).CloseParenIndex;
                    }

                    return GetLastToken(
                        lastTokenCache,
                        (node as ANONBLOCKNODE).BodyNode,
                        flags,
                        out missingName);

                case NODEKIND.GOTO:
                    if (node.AsGOTO.ArgumentsNode == null &&
                        (node.AsGOTO.Flags & NODEFLAGS.GOTO_CASE) != 0)
                    {
                        return IncludeSemiColon(
                            IncludeNextToken(node.TokenIndex, flags, out missingName, TOKENID.DEFAULT),
                            flags,
                            out missingName);
                        // Includes the semicolon...
                    }
                    //fallthrough
                    goto case NODEKIND.RETURN;

                case NODEKIND.BREAK:
                case NODEKIND.CONTINUE:
                case NODEKIND.EXPRSTMT:
                case NODEKIND.THROW:
                case NODEKIND.RETURN:
                    if (node.AsANYEXPRSTMT.ArgumentsNode != null)
                    {
                        return IncludeSemiColon(
                            GetLastToken(
                                lastTokenCache,
                                node.AsANYEXPRSTMT.ArgumentsNode,
                                flags,
                                out missingName),
                            flags,
                            out missingName);
                        // Includes semicolon...
                    }
                    // Includes the semicolon...
                    return IncludeSemiColon(node.TokenIndex, flags, out missingName);

                case NODEKIND.YIELD:
                    if (node.AsYIELD.ArgumentsNode != null)
                    {
                        return IncludeSemiColon(
                            GetLastToken(
                                lastTokenCache,
                                node.AsYIELD.ArgumentsNode,
                                flags,
                                out missingName),
                            flags,
                            out missingName);
                        // Includes semicolon...
                    }
                    // might be yield; yield return; or yield break;
                    return IncludeSemiColon(
                        IncludeOneOfNextToken(
                        node.TokenIndex, flags, out missingName, TOKENID.BREAK, TOKENID.RETURN),
                        flags, out missingName);    // Includes the semicolon...

                case NODEKIND.CASE:
                    if ((node as CASENODE).StatementsNode != null)
                    {
                        return GetLastToken(
                            lastTokenCache,
                            GetLastStatement((node as CASENODE).StatementsNode),
                            flags,
                            out missingName);
                    }
                    return GetLastToken(
                        lastTokenCache,
                        (node as CASENODE).LabelsNode,
                        flags,
                        out missingName);

                case NODEKIND.CASELABEL:
                    if (node.AsCASELABEL.Operand != null)
                    {
                        return IncludeColon(
                            GetLastToken(
                                lastTokenCache,
                                node.AsCASELABEL.Operand,
                                flags,
                                out missingName),
                            flags,
                            out missingName);
                        // Includes the colon...
                    }
                    return IncludeColon(node.TokenIndex, flags, out missingName);
                // Includes the colon...

                case NODEKIND.CATCH:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        if ((node as CATCHNODE).NameNode != null)
                        {
                            return IncludeCloseParen(
                                GetLastToken(
                                    lastTokenCache,
                                    (node as CATCHNODE).NameNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                            // Includes the ')'
                        }
                        if ((node as CATCHNODE).TypeNode != null)
                        {
                            return IncludeCloseParen(
                                GetLastToken(
                                    lastTokenCache,
                                    (node as CATCHNODE).TypeNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                            // Includes the ')'
                        }
                    }
                    else
                    {
                        if ((node as CATCHNODE).BlockNode != null)
                        {
                            return (node as CATCHNODE).BlockNode.CloseCurlyIndex;
                        }
                    }
                    return node.TokenIndex; // Nothing to include

                case NODEKIND.CHECKED:
                case NODEKIND.LABEL:
                case NODEKIND.UNSAFE:
                    return GetLastToken(
                        lastTokenCache,
                        node.AsANYLABELSTMT.StatementNode,
                        flags,
                        out missingName);

                case NODEKIND.CLASS:
                case NODEKIND.ENUM:
                case NODEKIND.INTERFACE:
                case NODEKIND.STRUCT:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        return PeekTokenIndexFromEx(
                            node.AsAGGREGATE.OpenCurlyIndex,
                            flags,
                            -1);
                        // Don't include the open curly
                    }
                    return node.AsAGGREGATE.CloseCurlyIndex;
                // all aggregates can be followed by an optional semi-colon.
                //return IncludeSemiColon(node.AsAGGREGATE.CloseCurlyIndex, flags, out missingName);

                case NODEKIND.FIELD:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        if ((node as FIELDNODE).DeclarationsNode.Kind == NODEKIND.LIST)
                        {
                            return GetLastToken(
                                lastTokenCache,
                                ((node as FIELDNODE).DeclarationsNode.AsANYBINOP).Operand1,
                                flags,
                                out missingName);
                            // Semicolon included by the NODEKIND.VARDECL
                        }
                        return GetLastToken(
                            lastTokenCache,
                            (node as FIELDNODE).DeclarationsNode,
                            flags,
                            out missingName);
                        // Semicolon included by the NODEKIND.VARDECL
                    }
                    // fallthrough
                    goto case NODEKIND.OPERATOR;

                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                case NODEKIND.METHOD:
                case NODEKIND.OPERATOR:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        return PeekTokenIndexFromEx(node.AsANYMETHOD.OpenIndex, flags, -1);
                        // Don't Include the open curly
                    }
                    goto case NODEKIND.CONST;

                case NODEKIND.NESTEDTYPE:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        return GetLastToken(
                            lastTokenCache,
                            (node as NESTEDTYPENODE).TypeNode,
                            flags,
                            out missingName);
                    }
                    goto case NODEKIND.CONST;

                case NODEKIND.INDEXER:
                case NODEKIND.PROPERTY:
                case NODEKIND.PARTIALMEMBER:
                case NODEKIND.ENUMMBR:
                case NODEKIND.CONST:
                    return node.AsANYMEMBER.CloseIndex;

                case NODEKIND.CONSTVAL:
                    // Must special case check for the collapsed unary minus expression,
                    // which is two tokens instead of one.
                    if ((node.Flags & NODEFLAGS.CHECK_FOR_UNARY_MINUS) != 0 &&
                        (node.PredefinedType == PREDEFTYPE.LONG || node.PredefinedType == PREDEFTYPE.INT))
                    {
                        return PeekTokenIndexFromEx(node.TokenIndex, flags, 1);
                    }
                    return node.TokenIndex;

                case NODEKIND.EMPTYSTMT:
                case NODEKIND.OP:
                    return node.TokenIndex;

                case NODEKIND.DECLSTMT:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        if ((node as DECLSTMTNODE).VariablesNode.Kind == NODEKIND.LIST)
                        {
                            return GetLastToken(
                                lastTokenCache,
                                ((node as DECLSTMTNODE).VariablesNode.AsANYBINOP).Operand1,
                                flags,
                                out missingName);
                            // Semicolon included by the NODEKIND.VARDECL
                        }
                        return GetLastToken(
                            lastTokenCache,
                            (node as DECLSTMTNODE).VariablesNode,
                            flags,
                            out missingName);
                        // Semicolon included by the NODEKIND.VARDECL
                    }
                    else
                    {
                        int tokIdx = GetLastToken(
                            lastTokenCache,
                            (node as DECLSTMTNODE).VariablesNode,
                            flags,
                            out missingName);
                        if (node.ParentNode != null &&
                            node.ParentNode.Kind == NODEKIND.FOR &&
                            (node.ParentNode.Flags &
                            (NODEFLAGS.FIXED_DECL | NODEFLAGS.USING_DECL | NODEFLAGS.FOR_FOREACH)) != 0)
                        {
                            return tokIdx;
                        }
                        return IncludeSemiColon(tokIdx, flags, out missingName);
                        // Include the semicolon
                    }

                case NODEKIND.DELEGATE:
                    return (node as DELEGATENODE).SemiColonIndex;

                case NODEKIND.DO:
                    return IncludeSemiColon(
                        // Includes close paren and semicolon
                        IncludeCloseParen(
                            GetLastToken(lastTokenCache, node.AsDO.ExpressionNode, flags, out missingName),
                            flags,
                            out missingName),
                        flags,
                        out missingName);

                case NODEKIND.FOR:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        return node.TokenIndex; // This is just a foreach
                    }
                    return GetLastToken(
                        lastTokenCache,
                        (node as FORSTMTNODE).StatementNode,
                        flags,
                        out missingName);

                case NODEKIND.IF:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        if ((node as IFSTMTNODE).ConditionNode != null)
                        {
                            return IncludeCloseParen(
                                GetLastToken(
                                    lastTokenCache,
                                    (node as IFSTMTNODE).ConditionNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                            // Includes close paren
                        }
                        return node.TokenIndex;
                    }
                    else
                    {
                        if ((node as IFSTMTNODE).ElseNode != null)
                        {
                            return GetLastToken(
                                lastTokenCache,
                                (node as IFSTMTNODE).ElseNode,
                                flags,
                                out missingName);
                        }
                        return GetLastToken(
                            lastTokenCache,
                            (node as IFSTMTNODE).StatementNode,
                            flags,
                            out missingName);
                    }

                case NODEKIND.LOCK:
                case NODEKIND.WHILE:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        if (node.AsANYLOOPSTMT.ExpressionNode != null)
                        {
                            return IncludeCloseParen(
                                GetLastToken(
                                    lastTokenCache,
                                    node.AsANYLOOPSTMT.ExpressionNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                            // Includes close paren
                        }
                        return node.TokenIndex;
                    }
                    else
                    {
                        return GetLastToken(
                            lastTokenCache,
                            node.AsANYLOOPSTMT.StatementNode,
                            flags,
                            out missingName);
                    }

                case NODEKIND.NAME:
                    if ((flags & ExtentFlags.POSSIBLE_GENERIC_NAME) != 0 &&
                        (node as NAMENODE).PossibleGenericName != null)
                    {
                        return GetLastToken(
                            lastTokenCache,
                            (node as NAMENODE).PossibleGenericName,
                            flags,
                            out missingName);
                    }
                    // fallthrough
                    goto case NODEKIND.ALIASNAME;

                case NODEKIND.ALIASNAME:
                    missingName = ((node.Flags & NODEFLAGS.NAME_MISSING) != 0);
                    return node.TokenIndex;

                case NODEKIND.GENERICNAME:
                    {
                        GENERICNAMENODE genericNameNode = node as GENERICNAMENODE;
                        if ((genericNameNode.CloseAngleIndex > (genericNameNode.OpenAngleIndex + 1)) ||
                            genericNameNode.ParametersNode == null)
                        {
                            return genericNameNode.CloseAngleIndex;
                        }
                        else
                        {
                            // we don't have a valid close position.
                            // return the end position of the parameters
                            return GetLastToken(
                                lastTokenCache,
                                genericNameNode.ParametersNode,
                                flags,
                                out missingName);
                        }
                    }
                //break;

                case NODEKIND.OPENNAME:
                    {
                        OPENNAMENODE nodeOpen = node as OPENNAMENODE;
                        //ASSERT(nodeOpen.iClose > nodeOpen.iOpen);
                        return nodeOpen.CloseAngleIndex;
                    }
                //break;

                case NODEKIND.TYPEWITHATTR:
                    return GetLastToken(
                        lastTokenCache,
                        (node as TYPEWITHATTRNODE).TypeBaseNode,
                        flags,
                        out missingName);

                case NODEKIND.CONSTRAINT:
                    if ((node as CONSTRAINTNODE).EndTokenIndex >= 0)
                    {
                        return (node as CONSTRAINTNODE).EndTokenIndex;
                    }
                    if ((node as CONSTRAINTNODE).BoundsNode != null)
                    {
                        return GetLastToken(
                            lastTokenCache,
                            (node as CONSTRAINTNODE).NameNode,
                            flags,
                            out missingName);
                    }
                    return GetLastToken(
                        lastTokenCache,
                        (node as CONSTRAINTNODE).BoundsNode,
                        flags,
                        out missingName);

                case NODEKIND.NAMESPACE:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        return (node as NAMESPACENODE).OpenCurlyIndex;
                    }
                    return (node as NAMESPACENODE).CloseCurlyIndex;

                case NODEKIND.NEW:
                    {
                        NEWNODE newNode = node as NEWNODE;

                        if (newNode.InitialNode != null)
                        {
                            return GetLastToken(
                                lastTokenCache,
                                newNode.InitialNode,
                                flags,
                                out missingName);
                        }
                        if (newNode.CloseParenIndex != -1)
                        {
                            return newNode.CloseParenIndex;
                        }

                        // Special case for stack alloc:
                        // The pType member is a NODEKIND.POINTERTYPE,
                        // although the parser parsed a NODEKIND.ARRAYTYPE.
                        // For "normal" new, only check the type is an array
                        bool isArrayType =
                            (newNode.TypeNode != null) &&
                            ((newNode.TypeNode.Kind == NODEKIND.ARRAYTYPE) ||
                             (newNode.TypeNode.Kind == NODEKIND.POINTERTYPE &&
                             newNode.Flags == NODEFLAGS.NEW_STACKALLOC));

                        if (isArrayType && newNode.ArgumentsNode != null)
                            return IncludeCloseSquare(
                                GetLastToken(
                                    lastTokenCache,
                                    newNode.ArgumentsNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);

                        return GetLastToken(
                            lastTokenCache,
                            newNode.TypeNode,
                            flags,
                            out missingName);
                    }

                case NODEKIND.PARAMETER:
                    return GetLastToken(
                        lastTokenCache,
                        (node as PARAMETERNODE).NameNode,
                        flags,
                        out missingName);

                case NODEKIND.TRY:
                    if ((flags & ExtentFlags.SINGLESTMT) == 0)
                    {
                        if ((node as TRYSTMTNODE).CatchNode != null)
                        {
                            return GetLastToken(
                                lastTokenCache,
                                (node as TRYSTMTNODE).CatchNode,
                                flags,
                                out missingName);
                        }
                        if ((node as TRYSTMTNODE).BlockNode != null)
                        {
                            return GetLastToken(
                                lastTokenCache,
                                (node as TRYSTMTNODE).BlockNode,
                                flags,
                                out missingName);
                        }
                    }
                    return node.TokenIndex;

                case NODEKIND.PREDEFINEDTYPE:
                    return node.TokenIndex;

                case NODEKIND.POINTERTYPE:
                    return node.TokenIndex;

                case NODEKIND.NULLABLETYPE:
                    return node.TokenIndex;

                case NODEKIND.NAMEDTYPE:
                    return GetLastToken(
                        lastTokenCache,
                        (node as NAMEDTYPENODE).NameNode,
                        flags,
                        out missingName);

                case NODEKIND.OPENTYPE:
                    return GetLastToken(
                        lastTokenCache,
                        node.AsOPENTYPE.NameNode,
                        flags,
                        out missingName);

                case NODEKIND.ARRAYTYPE:
                    BASENODE pArrayType, pNextType;
                    pArrayType = pNextType = node;
                    while (pNextType.Kind == NODEKIND.ARRAYTYPE)
                    {
                        pArrayType = pNextType;
                        pNextType = (pNextType as ARRAYTYPENODE).ElementTypeNode;
                    }
                    // For array types, the token index is the open '['.  Add the number of
                    // dimensions and you land on the ']' (1==[], 2==[,], 3==[,,], etc).
                    if ((pArrayType as ARRAYTYPENODE).Dimensions == -1)
                        return IncludeCloseSquare(
                            IncludeQuestion(node.TokenIndex, flags, out missingName),
                            flags,
                            out missingName);
                    // unknown rank is [?]
                    return PeekTokenIndexFromEx(
                        pArrayType.TokenIndex,
                        flags,
                        (pArrayType as ARRAYTYPENODE).Dimensions);

                case NODEKIND.SWITCH:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        if ((node as SWITCHSTMTNODE).ExpressionNode != null)
                            return IncludeCloseParen(
                                GetLastToken(
                                    lastTokenCache,
                                    (node as SWITCHSTMTNODE).ExpressionNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                        // Includes close paren
                    }
                    return (node as SWITCHSTMTNODE).CloseCurlyIndex;

                case NODEKIND.UNOP:
                    switch (node.Operator)
                    {
                        case OPERATOR.POSTINC:
                        case OPERATOR.POSTDEC:
                            return node.TokenIndex;

                        case OPERATOR.DEFAULT:
                        case OPERATOR.PAREN:
                        case OPERATOR.UNCHECKED:
                        case OPERATOR.CHECKED:
                        case OPERATOR.TYPEOF:
                        case OPERATOR.SIZEOF:
                            return IncludeCloseParen(
                                GetLastToken(
                                    lastTokenCache,
                                    (node as UNOPNODE).Operand,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                        // Get the close paren

                        default:
                            return GetLastToken(
                                lastTokenCache,
                                (node as UNOPNODE).Operand,
                                flags,
                                out missingName);
                    }
                //break;

                case NODEKIND.USING:
                    if ((node as USINGNODE).NameNode != null)
                    {
                        return IncludeSemiColon(
                            GetLastToken(
                                lastTokenCache,
                                (node as USINGNODE).NameNode,
                                flags,
                                out missingName),
                            flags,
                            out missingName);
                    }
                    else
                    {
                        return IncludeSemiColon(
                            GetLastToken(
                                lastTokenCache,
                                (node as USINGNODE).AliasNode,
                                flags,
                                out missingName),
                            flags,
                            out missingName);
                        // Includes semicolon
                    }


                case NODEKIND.VARDECL:
                    if ((flags & ExtentFlags.SINGLESTMT) != 0)
                    {
                        if ((node.Flags & NODEFLAGS.VARDECL_ARRAY) != 0)
                        {
                            return IncludeSemiColonOrComma(
                                IncludeCloseSquare(
                                    GetLastToken(
                                        lastTokenCache,
                                        (node as VARDECLNODE).ArgumentsNode,
                                        flags,
                                        out missingName),
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                            // Includes ']' and semicolon or comma
                        }
                        if ((node.Flags & NODEFLAGS.VARDECL_EXPR) != 0)
                        {
                            return IncludeSemiColonOrComma(
                                GetLastToken(
                                    lastTokenCache,
                                    (node as VARDECLNODE).ArgumentsNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                            // Includes semicolon or comma
                        }
                        return IncludeSemiColonOrComma(
                            GetLastToken(
                                lastTokenCache,
                                (node as VARDECLNODE).NameNode,
                                flags,
                                out missingName),
                            flags,
                            out missingName);
                        // Includes semicolon or comma
                    }
                    else
                    {
                        if ((node.Flags & NODEFLAGS.VARDECL_ARRAY) != 0)
                        {
                            return IncludeCloseSquare(
                                GetLastToken(
                                    lastTokenCache,
                                    (node as VARDECLNODE).ArgumentsNode,
                                    flags,
                                    out missingName),
                                flags,
                                out missingName);
                            // Includes ']'
                        }
                        if ((node.Flags & NODEFLAGS.VARDECL_EXPR) != 0)
                        {
                            return GetLastToken(
                                lastTokenCache,
                                (node as VARDECLNODE).ArgumentsNode,
                                flags,
                                out missingName);
                        }
                        return GetLastToken(
                            lastTokenCache,
                            (node as VARDECLNODE).NameNode,
                            flags,
                            out missingName);
                    }

                default:
                    // RARELY are nodes singularly represented (only names, constants, or other terms).
                    // Represent them all explicitly!
                    DebugUtil.VsFail("Unhandled node type in GetLastToken!");
                    return node.TokenIndex;
            }
        }

        // Token processing

        //------------------------------------------------------------
        // CParser.GetLastTokenWorker
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <param name="iTok1"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeNextToken(
            int iTokIdx,
            ExtentFlags flags,
            out bool pfMissingName,
            TOKENID iTok1)
        {
            return IncludeOneOfNextToken(iTokIdx, flags, out pfMissingName, iTok1, TOKENID.UNDEFINED);
        }

        //------------------------------------------------------------
        // CParser.IncludeOneOfNextToken
        //
        /// <summary>
        /// Skip noisy tokens from "tokenIndex" up to a token "tokenId1" or "tokenId2".
        /// Return the token index of "tokenId1" or "tokenId2",
        /// or return "tokenIndex" is the token found doesn't match.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <param name="flags"></param>
        /// <param name="missingName"></param>
        /// <param name="tokenId1"></param>
        /// <param name="tokenId2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeOneOfNextToken(
            int tokenIndex,
            ExtentFlags flags,
            out bool missingName,
            TOKENID tokenId1,
            TOKENID tokenId2)
        {
            // This assert is not quite right if we're called from the FindLeaf call
            // in ParseTopLevel method, because the parse tree may not be up to date
            // with the token stream. In that case, we will just return a bogus token
            // index (but within the range of the token stream array)
            //ASSERT(tokenIndex >= 0);
            //ASSERT(tokenIndex <= m_iTokens);
            missingName = true;

            if ((flags & ExtentFlags.IGNORE_TOKEN_STREAM) != 0)
            {
                // if we're going to read a token ahead no matter what,
                // then we have to assume that we're no longer at a missing name
                missingName = false;
                return Math.Min(tokenCount - 1, tokenIndex + 1);
            }
            else
            {
                int iNextTokenIdx = PeekTokenIndexFrom(tokenIndex, 1);
                //ASSERT(iNextTokenIdx >= 0);
                //ASSERT(iNextTokenIdx < m_iTokens);
                if (iNextTokenIdx < 0 || iNextTokenIdx >= tokenCount)
                {
                    return tokenIndex;
                }

                // Ok, the token index is valid. Check we have one of the "expected" tokens
                TOKENID iNextTokId = tokenArray[iNextTokenIdx].TokenID;
                if (iNextTokId == tokenId1 || iNextTokId == tokenId2)
                {
                    // we read ahead one token, we're no longer at a missing name
                    missingName = false;
                    return iNextTokenIdx;
                }
                return tokenIndex;
            }
        }

        //------------------------------------------------------------
        // CParser.IncludeSemiColon
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeSemiColon(int iTokIdx, ExtentFlags flags, out bool pfMissingName)
        {
            return IncludeNextToken(iTokIdx, flags, out pfMissingName, TOKENID.SEMICOLON);
        }

        //------------------------------------------------------------
        // CParser.IncludeSemiColon
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeColon(int iTokIdx, ExtentFlags flags, out bool pfMissingName)
        {
            return IncludeNextToken(iTokIdx, flags, out pfMissingName, TOKENID.COLON);
        }

        //------------------------------------------------------------
        // CParser.IncludeCloseCurly
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeCloseParen(int iTokIdx, ExtentFlags flags, out bool pfMissingName)
        {
            return IncludeNextToken(iTokIdx, flags, out pfMissingName, TOKENID.CLOSEPAREN);
        }

        //------------------------------------------------------------
        // CParser.IncludeCloseCurly
        //
        /// <summary></summary>
        /// <param name="tokenIndex"></param>
        /// <param name="flags"></param>
        /// <param name="missingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeCloseCurly(int tokenIndex, ExtentFlags flags, out bool missingName)
        {
            return IncludeNextToken(tokenIndex, flags, out missingName, TOKENID.CLOSECURLY);
        }

        //------------------------------------------------------------
        // CParser.IncludeCloseSquare
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeCloseSquare(int iTokIdx, ExtentFlags flags, out bool pfMissingName)
        {
            return IncludeNextToken(iTokIdx, flags, out pfMissingName, TOKENID.CLOSESQUARE);
        }

        //------------------------------------------------------------
        // CParser.IncludeCloseCurly
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeCloseParenOrCloseSquare(
            int iTokIdx,
            ExtentFlags flags,
            out bool pfMissingName)
        {
            return IncludeOneOfNextToken(
                iTokIdx, flags, out pfMissingName, TOKENID.CLOSEPAREN, TOKENID.CLOSESQUARE);
        }

        //------------------------------------------------------------
        // CParser.IncludeComma
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeSemiColonOrComma(int iTokIdx, ExtentFlags flags, out bool pfMissingName)
        {
            return IncludeOneOfNextToken(
                iTokIdx, flags, out pfMissingName, TOKENID.SEMICOLON, TOKENID.COMMA);
        }

        //------------------------------------------------------------
        // CParser.IncludeComma
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeQuestion(int iTokIdx, ExtentFlags flags, out bool pfMissingName)
        {
            return IncludeOneOfNextToken(
                iTokIdx, flags, out pfMissingName, TOKENID.QUESTION, unchecked((TOKENID)(-1)));
        }

        //------------------------------------------------------------
        // CParser.IncludeComma
        //
        /// <summary></summary>
        /// <param name="iTokIdx"></param>
        /// <param name="flags"></param>
        /// <param name="pfMissingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IncludeComma(int iTokIdx, ExtentFlags flags, out bool pfMissingName)
        {
            return IncludeNextToken(iTokIdx, flags, out pfMissingName, TOKENID.COMMA);
        }

        // Here are the pure virtuals that must be implemented by parser implementations

        //    virtual void    *MemAlloc (int iSize) = 0;

        //------------------------------------------------------------
        // CParser.CreateNewError (abstract)
        //
        /// <summary></summary>
        /// <param name="iErrorId"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        abstract internal bool CreateNewError(
            CSCERRID iErrorId,
            POSDATA posStart,
            POSDATA posEnd,
            params ErrArg[] args);

        //------------------------------------------------------------
        // CParser.AddToNodeTable (abstract)
        //
        /// <summary>
        /// <para>Does nothing. </para>
        /// <para>In sscli, this is pure virtual. Define in derived classes.</para>
        /// </summary>
        //------------------------------------------------------------
        abstract internal void AddToNodeTable(BASENODE node);

        //------------------------------------------------------------
        // CParser.GetInteriorTree (abstract)
        //
        /// <summary></summary>
        /// <param name="sourceData"></param>
        /// <param name="node"></param>
        /// <param name="interiorTree"></param>
        //------------------------------------------------------------
        abstract internal void GetInteriorTree(
            CSourceData sourceData,
            BASENODE node,
            ref CInteriorTree interiorTree); // ICSInteriorTree** ppTree
  

        //------------------------------------------------------------
        // CParser.GetLexData (abstract)
        //
        /// <summary></summary>
        /// <param name="lexData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        abstract internal bool GetLexData(LEXDATA lexData);

        // Static token/operator information grabbers
        // static  const   TOKINFO TokenInfoArray[TID_NUMTOKENS];
        // static  const   OPINFO  OperatorInfoArray[];
        // 上の２つのメンバはクラスの最後で定義する。

        //------------------------------------------------------------
        // CParser.GetTokenInfo (static)
        //
        /// <summary></summary>
        /// <param name="tokId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal TOKINFO GetTokenInfo(TOKENID tokId)
        {
            try
            {
                return TokenInfoArray[(int)tokId];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // CParser.GetOperatorInfo (static)
        //
        /// <summary></summary>
        /// <param name="opId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal OPINFO GetOperatorInfo(OPERATOR opId)
        {
            try
            {
                return OperatorInfoArray[(int)opId];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // CParser.IsRightAssociative (static)
        //
        /// <summary></summary>
        /// <param name="iOp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool IsRightAssociative(OPERATOR iOp)
        {
            return OperatorInfoArray[(int)iOp].RightAssociativity;
        }

        //------------------------------------------------------------
        // CParser.GetOperatorPrecedence (static)
        //
        /// <summary>演算子の優先度を取得する。</summary>
        /// <param name="operatorId">オペレータの ID。</param>
        /// <returns>優先度を示す int 値。</returns>
        //------------------------------------------------------------
        static internal int GetOperatorPrecedence(OPERATOR operatorId)
        {
            int prec = -1;
            try
            {
                prec = OperatorInfoArray[(int)operatorId].Precedence;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return prec;
        }

        //------------------------------------------------------------
        // CParser.GetCastPrecedence (static)
        //
        /// <summary>変換演算子の優先度を取得する。</summary>
        /// <returns>変換演算子の優先度を示す int 値。</returns>
        //------------------------------------------------------------
        static internal int GetCastPrecedence()
        {
            return GetOperatorPrecedence(OPERATOR.CAST);
        }

        //------------------------------------------------------------
        // CParser.IsUnaryOperator  (1)
        //
        /// <summary>
        /// トークン ID から単項演算子としての情報
        /// （演算子の種別を示す OPERATOR 型の ID、優先度）を取得する。
        /// </summary>
        /// <param name="tokenId">トークン ID を指定する。</param>
        /// <param name="operatorId">どの演算子かを示す OPERATOR 型の値がセットされる。</param>
        /// <param name="precedence">優先度がセットされる。</param>
        /// <returns>単項演算子である場合は true を返す。</returns>
        //------------------------------------------------------------
        internal bool IsUnaryOperator(
            TOKENID tokenId,
            out OPERATOR operatorId,
            out uint precedence)
        {
            operatorId = OPERATOR.NONE;
            precedence = 0;

            if (!IsUnaryOperator(tokenId))
            {
                return false;
            }

            operatorId = TokenInfoArray[(int)tokenId].UnaryOperator;
            precedence = (uint)GetOperatorPrecedence(operatorId);
            return true;
        }

        //------------------------------------------------------------
        // CParser.IsUnaryOperator  (2)
        //
        /// <summary>トークンが単項演算子であるかを調べる。</summary>
        /// <param name="iTok">調べたいトークンの ID を指定する。</param>
        /// <returns>単項演算子である場合は true を返す。</returns>
        //------------------------------------------------------------
        internal bool IsUnaryOperator(TOKENID tokenId)
        {
            bool br = false;

            try
            {
                // "explicit", "false", "implicit", "true" は演算子のオーバーラップの対象なので
                // その UnaryOp は NONE でないが、実際には単項演算子ではない。

                br = (TokenInfoArray[(int)tokenId].UnaryOperator != OPERATOR.NONE &&
                    (TokenInfoArray[(int)tokenId].Flags & TOKFLAGS.F_OVLOPKWD) == 0);
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return br;
        }

        //------------------------------------------------------------
        // CParser.IsBinaryOperator (static)
        //
        /// <summary>トークン ID から二項演算子としての情報を取得する。</summary>
        /// <param name="tokenId">トークン ID を指定する。</param>
        /// <param name="operatorId">どの演算子かを示す OPERATOR 型の値がセットされる。</param>
        /// <param name="precedence">優先度がセットされる。</param>
        /// <returns>二項演算子である場合は true を返す。</returns>
        //------------------------------------------------------------
        static internal bool IsBinaryOperator(
            TOKENID tokenId,
            out OPERATOR operatorId,
            out uint precedence)
        {
            operatorId = OPERATOR.NONE;
            precedence = 0;

            if (TokenInfoArray[(int)tokenId].BinaryOperator != OPERATOR.NONE &&
                (TokenInfoArray[(int)tokenId].Flags & TOKFLAGS.F_OVLOPKWD) == 0)
            {
                operatorId = (OPERATOR)TokenInfoArray[(int)tokenId].BinaryOperator;
                precedence = (uint)GetOperatorPrecedence(operatorId);
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CParser.GetLastStatement
        //
        /// <summary></summary>
        /// <param name="stmtNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal STATEMENTNODE GetLastStatement(STATEMENTNODE stmtNode)
        {
            while (stmtNode != null && stmtNode.NextNode != null) stmtNode = stmtNode.NextStatementNode;
            return stmtNode;
        }

        //------------------------------------------------------------
        // CParser.IsNameStart (static)
        //
        /// <summary>
        /// Reurn true if the current token is an identifier.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool IsNameStart(TOKENID tokenId)
        {
            return tokenId == TOKENID.IDENTIFIER;
        }

        //------------------------------------------------------------
        // CParser.IsAnyTypeOrExpr (static)
        //
        /// <summary>
        /// 引数が GenericTypeOrExpr か NonGenericTypeOrExpr なら true を返す。
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool IsAnyTypeOrExpr(ScanTypeFlagsEnum st)
        {
            return (
                st == ScanTypeFlagsEnum.GenericTypeOrExpr ||
                st == ScanTypeFlagsEnum.NonGenericTypeOrExpr);
        }

        //------------------------------------------------------------
        // CParser.PartScanToTypeScan (static)
        //
        /// <summary>
        /// ScanNamedTypePartEnum を ScanTypeFlagsEnum へ変換する。
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal ScanTypeFlagsEnum PartScanToTypeScan(ScanNamedTypePartEnum st)
        {
            //ASSERT(st != ScanNamedTypePart::NotName);
            if (st == ScanNamedTypePartEnum.NotName)
                throw new LogicError("CParser.PartScanToTypeScan");

            return st == ScanNamedTypePartEnum.GenericName ?
                ScanTypeFlagsEnum.GenericTypeOrExpr : ScanTypeFlagsEnum.NonGenericTypeOrExpr;
        }

        //------------------------------------------------------------
        // CParser.PeekTokenIndexFrom (2)
        //
        /// <summary>
        /// <para>This should really be on LEXDATA, but it makes it easier to share the token info table
        /// and share the implementation.</para>
        /// <para>LEXDATA.PeekTokenIndexFrom を呼び出す。</para>
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="iCur"></param>
        /// <param name="iPeek"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal int PeekTokenIndexFrom(
            LEXDATA ld,
            int iCur,
            int iPeek)  // = 1) 
        {
            return ld.PeekTokenIndexFrom(iCur, iPeek);
        }

        //------------------------------------------------------------
        // TOKINFO[] TokenInfoArray
        //------------------------------------------------------------
        static internal TOKINFO[] TokenInfoArray =
		{
            // 1番目は UNDEFINED 用のダミーデータ
			new TOKINFO(null, 0, 0, CParser.PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE),

			new TOKINFO( "__arglist", TOKFLAGS.F_MSKEYWORD | TOKFLAGS.F_TERM, "__arglist".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.ARGS ),
			new TOKINFO( "__makeref", TOKFLAGS.F_MSKEYWORD | TOKFLAGS.F_TERM, "__makeref".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.MAKEREFANY ),
			new TOKINFO( "__reftype", TOKFLAGS.F_MSKEYWORD | TOKFLAGS.F_TERM, "__reftype".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.REFTYPE ),
			new TOKINFO( "__refvalue", TOKFLAGS.F_MSKEYWORD | TOKFLAGS.F_TERM, "__refvalue".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.REFVALUE ),
			new TOKINFO( "abstract", TOKFLAGS.F_MEMBER | TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MODIFIER, "abstract".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "as", TOKFLAGS.F_CASTEXPR, "as".Length, PARSERTYPE.None, 0, OPERATOR.AS, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "base", TOKFLAGS.F_TERM, "base".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.BASE ),
			new TOKINFO( "bool", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "bool".Length, PARSERTYPE.None, PREDEFTYPE.BOOL, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "break", TOKFLAGS.F_INVALIDSUBEXPRESSION, "break".Length, PARSERTYPE.BreakStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "byte", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "byte".Length, PARSERTYPE.None, PREDEFTYPE.BYTE, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "case", TOKFLAGS.F_INVALIDSUBEXPRESSION, "case".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "catch", TOKFLAGS.F_INVALIDSUBEXPRESSION, "catch".Length, PARSERTYPE.TryStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "char", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "char".Length, PARSERTYPE.None, PREDEFTYPE.CHAR, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "checked", TOKFLAGS.F_TERM, "checked".Length, PARSERTYPE.CheckedStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.CHECKED ),
			new TOKINFO( "class", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_TYPEDECL | TOKFLAGS.F_MEMBER, "class".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "const", TOKFLAGS.F_MEMBER | TOKFLAGS.F_INVALIDSUBEXPRESSION, "const".Length, PARSERTYPE.ConstStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "continue", TOKFLAGS.F_INVALIDSUBEXPRESSION, "continue".Length, PARSERTYPE.BreakStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "decimal", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "decimal".Length, PARSERTYPE.None, PREDEFTYPE.DECIMAL, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "default", TOKFLAGS.F_TERM, "default".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.DEFAULT ),
			new TOKINFO( "delegate", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_TYPEDECL | TOKFLAGS.F_MEMBER | TOKFLAGS.F_TERM, "delegate".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "do", TOKFLAGS.F_INVALIDSUBEXPRESSION, "do".Length, PARSERTYPE.DoStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "double", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "double".Length, PARSERTYPE.None, PREDEFTYPE.DOUBLE, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "else", 0, "else".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "enum", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_TYPEDECL | TOKFLAGS.F_MEMBER, "enum".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "event", TOKFLAGS.F_MEMBER, "event".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "explicit", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_OVLOPKWD, "explicit".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.EXPLICIT, OPERATOR.NONE ),
			new TOKINFO( "extern", TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "extern".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "false", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_OVLOPKWD | TOKFLAGS.F_TERM, "false".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.FALSE, OPERATOR.FALSE ),
			new TOKINFO( "finally", TOKFLAGS.F_INVALIDSUBEXPRESSION, "finally".Length, PARSERTYPE.TryStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "fixed", TOKFLAGS.F_MEMBER, "fixed".Length, PARSERTYPE.FixedStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "float", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "float".Length, PARSERTYPE.None, PREDEFTYPE.FLOAT, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "for", TOKFLAGS.F_INVALIDSUBEXPRESSION, "for".Length, PARSERTYPE.ForStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "foreach", TOKFLAGS.F_INVALIDSUBEXPRESSION, "foreach".Length, PARSERTYPE.ForEachStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "goto", TOKFLAGS.F_INVALIDSUBEXPRESSION, "goto".Length, PARSERTYPE.GotoStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "if", TOKFLAGS.F_INVALIDSUBEXPRESSION, "if".Length, PARSERTYPE.IfStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "in", 0, "in".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "implicit", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_OVLOPKWD, "implicit".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.IMPLICIT, OPERATOR.NONE ),
			new TOKINFO( "int", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "int".Length, PARSERTYPE.None, PREDEFTYPE.INT, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "interface", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_TYPEDECL | TOKFLAGS.F_MEMBER, "interface".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "internal", TOKFLAGS.F_MEMBER | TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MODIFIER, "internal".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "is", TOKFLAGS.F_CASTEXPR, "is".Length, PARSERTYPE.None, 0, OPERATOR.IS, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "lock", TOKFLAGS.F_INVALIDSUBEXPRESSION, "lock".Length, PARSERTYPE.LockStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "long", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "long".Length, PARSERTYPE.None, PREDEFTYPE.LONG, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "namespace", 0, "namespace".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "new", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MEMBER | TOKFLAGS.F_TERM | TOKFLAGS.F_MODIFIER, "new".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "null", TOKFLAGS.F_TERM, "null".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NULL ),
			new TOKINFO( "object", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "object".Length, PARSERTYPE.None, PREDEFTYPE.OBJECT, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "operator", 0, "operator".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "out", 0, "out".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "override", TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "override".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "params", 0, "params".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "private", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "private".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "protected", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "protected".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "public", TOKFLAGS.F_MEMBER | TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MODIFIER, "public".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "readonly", TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "readonly".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "ref", 0, "ref".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "return", TOKFLAGS.F_INVALIDSUBEXPRESSION, "return".Length, PARSERTYPE.ReturnStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "sbyte", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "sbyte".Length, PARSERTYPE.None, PREDEFTYPE.SBYTE, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "sealed", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "sealed".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "short", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "short".Length, PARSERTYPE.None, PREDEFTYPE.SHORT, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "sizeof", TOKFLAGS.F_TERM, "sizeof".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.SIZEOF ),
			new TOKINFO( "stackalloc", 0, "stackalloc".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "static", TOKFLAGS.F_MEMBER | TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MODIFIER, "static".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "string", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "string".Length, PARSERTYPE.None, PREDEFTYPE.STRING, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "struct", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_TYPEDECL | TOKFLAGS.F_MEMBER, "struct".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "switch", TOKFLAGS.F_INVALIDSUBEXPRESSION, "switch".Length, PARSERTYPE.SwitchStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "this", TOKFLAGS.F_TERM, "this".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.THIS ),
			new TOKINFO( "throw", TOKFLAGS.F_INVALIDSUBEXPRESSION, "throw".Length, PARSERTYPE.ThrowStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "true", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_OVLOPKWD | TOKFLAGS.F_TERM, "true".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.TRUE, OPERATOR.TRUE ),
			new TOKINFO( "try", TOKFLAGS.F_INVALIDSUBEXPRESSION, "try".Length, PARSERTYPE.TryStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "typeof", TOKFLAGS.F_TERM, "typeof".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.TYPEOF ),
			new TOKINFO( "uint", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "uint".Length, PARSERTYPE.None, PREDEFTYPE.UINT, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "ulong", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "ulong".Length, PARSERTYPE.None, PREDEFTYPE.ULONG, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "unchecked", TOKFLAGS.F_TERM, "unchecked".Length, PARSERTYPE.CheckedStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.UNCHECKED ),
			new TOKINFO( "unsafe", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "unsafe".Length, PARSERTYPE.UnsafeStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "ushort", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "ushort".Length, PARSERTYPE.None, PREDEFTYPE.USHORT, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "using", TOKFLAGS.F_INVALIDSUBEXPRESSION, "using".Length, PARSERTYPE.UsingStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "virtual", TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "virtual".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "void", TOKFLAGS.F_MEMBER | TOKFLAGS.F_PREDEFINED, "void".Length, PARSERTYPE.None, PREDEFTYPE.VOID, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "volatile", TOKFLAGS.F_MEMBER | TOKFLAGS.F_MODIFIER, "volatile".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "while", TOKFLAGS.F_INVALIDSUBEXPRESSION, "while".Length, PARSERTYPE.WhileStatement, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_MEMBER | TOKFLAGS.F_TERM, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_TERM, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_TERM, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_TERM, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_TERM, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_NOISE, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_NOISE, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_NOISE, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", TOKFLAGS.F_NOISE, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ";", TOKFLAGS.F_CASTEXPR, ";".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ")", TOKFLAGS.F_CASTEXPR, ")".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "]", TOKFLAGS.F_CASTEXPR, "]".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "{", TOKFLAGS.F_CASTEXPR, "{".Length, PARSERTYPE.Block, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "}", TOKFLAGS.F_CASTEXPR, "}".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ",", TOKFLAGS.F_CASTEXPR, ",".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "=", TOKFLAGS.F_CASTEXPR, "=".Length, PARSERTYPE.None, 0, OPERATOR.ASSIGN, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "+=", TOKFLAGS.F_CASTEXPR, "+=".Length, PARSERTYPE.None, 0, OPERATOR.ADDEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "-=", TOKFLAGS.F_CASTEXPR, "-=".Length, PARSERTYPE.None, 0, OPERATOR.SUBEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "*=", TOKFLAGS.F_CASTEXPR, "*=".Length, PARSERTYPE.None, 0, OPERATOR.MULEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "/=", TOKFLAGS.F_CASTEXPR, "/=".Length, PARSERTYPE.None, 0, OPERATOR.DIVEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "%=", TOKFLAGS.F_CASTEXPR, "%=".Length, PARSERTYPE.None, 0, OPERATOR.MODEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "&=", TOKFLAGS.F_CASTEXPR, "&=".Length, PARSERTYPE.None, 0, OPERATOR.ANDEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "^=", TOKFLAGS.F_CASTEXPR, "^=".Length, PARSERTYPE.None, 0, OPERATOR.XOREQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "|=", TOKFLAGS.F_CASTEXPR, "|=".Length, PARSERTYPE.None, 0, OPERATOR.OREQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "<<=", TOKFLAGS.F_CASTEXPR, "<<=".Length, PARSERTYPE.None, 0, OPERATOR.LSHIFTEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ">>=", TOKFLAGS.F_CASTEXPR, ">>=".Length, PARSERTYPE.None, 0, OPERATOR.RSHIFTEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "?", TOKFLAGS.F_CASTEXPR, "?".Length, PARSERTYPE.None, 0, OPERATOR.QUESTION, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ":", TOKFLAGS.F_CASTEXPR, ":".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "::", TOKFLAGS.F_MEMBER, "::".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "||", TOKFLAGS.F_CASTEXPR, "||".Length, PARSERTYPE.None, 0, OPERATOR.LOGOR, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "&&", TOKFLAGS.F_CASTEXPR, "&&".Length, PARSERTYPE.None, 0, OPERATOR.LOGAND, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "|", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "|".Length, PARSERTYPE.None, 0, OPERATOR.BITOR, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "^", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "^".Length, PARSERTYPE.None, 0, OPERATOR.BITXOR, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "&", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "&".Length, PARSERTYPE.None, 0, OPERATOR.BITAND, OPERATOR.ADDR, OPERATOR.NONE ),
			new TOKINFO( "==", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "==".Length, PARSERTYPE.None, 0, OPERATOR.EQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "!=", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "!=".Length, PARSERTYPE.None, 0, OPERATOR.NEQ, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "<", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "<".Length, PARSERTYPE.None, 0, OPERATOR.LT, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "<=", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "<=".Length, PARSERTYPE.None, 0, OPERATOR.LE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ">", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, ">".Length, PARSERTYPE.None, 0, OPERATOR.GT, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ">=", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, ">=".Length, PARSERTYPE.None, 0, OPERATOR.GE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "<<", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "<<".Length, PARSERTYPE.None, 0, OPERATOR.LSHIFT, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ">>", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, ">>".Length, PARSERTYPE.None, 0, OPERATOR.RSHIFT, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "+", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "+".Length, PARSERTYPE.None, 0, OPERATOR.ADD, OPERATOR.UPLUS, OPERATOR.NONE ),
			new TOKINFO( "-", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "-".Length, PARSERTYPE.None, 0, OPERATOR.SUB, OPERATOR.NEG, OPERATOR.NONE ),
			new TOKINFO( "*", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "*".Length, PARSERTYPE.None, 0, OPERATOR.MUL, OPERATOR.INDIR, OPERATOR.NONE ),
			new TOKINFO( "/", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "/".Length, PARSERTYPE.None, 0, OPERATOR.DIV, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "%", TOKFLAGS.F_OVLBINOP | TOKFLAGS.F_CASTEXPR, "%".Length, PARSERTYPE.None, 0, OPERATOR.MOD, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "~", TOKFLAGS.F_MEMBER | TOKFLAGS.F_OVLUNOP, "~".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.BITNOT, OPERATOR.NONE ),
			new TOKINFO( "!", TOKFLAGS.F_OVLUNOP, "!".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.LOGNOT, OPERATOR.NONE ),
			new TOKINFO( "++", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_CASTEXPR, "++".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.PREINC, OPERATOR.NONE ),
			new TOKINFO( "--", TOKFLAGS.F_OVLUNOP | TOKFLAGS.F_CASTEXPR, "--".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.PREDEC, OPERATOR.NONE ),
			new TOKINFO( "(", TOKFLAGS.F_TERM, "(".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "[", TOKFLAGS.F_NSELEMENT | TOKFLAGS.F_MEMBER | TOKFLAGS.F_CASTEXPR, "[".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( ".", TOKFLAGS.F_CASTEXPR, ".".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "->", TOKFLAGS.F_CASTEXPR, "->".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "??", TOKFLAGS.F_CASTEXPR, "??".Length, PARSERTYPE.None, 0, OPERATOR.VALORDEF, OPERATOR.NONE, OPERATOR.NONE ),
            
            // CS3
            new TOKINFO( "=>", TOKFLAGS.F_CASTEXPR, "=>".Length, PARSERTYPE.None, 0, OPERATOR.LAMBDA, OPERATOR.NONE, OPERATOR.NONE ),

            // Special
            new TOKINFO( "", TOKFLAGS.F_CASTEXPR, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", 0, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
			new TOKINFO( "", 0, "".Length, PARSERTYPE.None, 0, OPERATOR.NONE, OPERATOR.NONE, OPERATOR.NONE ),
		};

        //------------------------------------------------------------
        // OPINFO[] OperatorInfoArray
        //------------------------------------------------------------
        static internal OPINFO[] OperatorInfoArray =
		{
			new OPINFO(TOKENID.UNKNOWN, 0, false),
			new OPINFO(TOKENID.EQUAL, 1, true),
			new OPINFO(TOKENID.PLUSEQUAL, 1, true),
			new OPINFO(TOKENID.MINUSEQUAL, 1, true),
			new OPINFO(TOKENID.SPLATEQUAL, 1, true),
			new OPINFO(TOKENID.SLASHEQUAL, 1, true),
			new OPINFO(TOKENID.MODEQUAL, 1, true),
			new OPINFO(TOKENID.ANDEQUAL, 1, true),
			new OPINFO(TOKENID.HATEQUAL, 1, true),
			new OPINFO(TOKENID.BAREQUAL, 1, true),
			new OPINFO(TOKENID.SHIFTLEFTEQ, 1, true),
			new OPINFO(TOKENID.SHIFTRIGHTEQ, 1, true),
			new OPINFO(TOKENID.QUESTION, 2, false),
			new OPINFO(TOKENID.QUESTQUEST, 3, true),
			new OPINFO(TOKENID.LOG_OR, 4, false),
			new OPINFO(TOKENID.LOG_AND, 5, false),
			new OPINFO(TOKENID.BAR, 6, false),
			new OPINFO(TOKENID.HAT, 7, false),
			new OPINFO(TOKENID.AMPERSAND, 8, false),
			new OPINFO(TOKENID.EQUALEQUAL, 9, false),
			new OPINFO(TOKENID.NOTEQUAL, 9, false),
			new OPINFO(TOKENID.LESS, 10, false),
			new OPINFO(TOKENID.LESSEQUAL, 10, false),
			new OPINFO(TOKENID.GREATER, 10, false),
			new OPINFO(TOKENID.GREATEREQUAL, 10, false),
			new OPINFO(TOKENID.IS, 10, false),
			new OPINFO(TOKENID.AS, 10, false),
			new OPINFO(TOKENID.SHIFTLEFT, 11, false),
			new OPINFO(TOKENID.SHIFTRIGHT, 11, false),
			new OPINFO(TOKENID.PLUS, 12, false),
			new OPINFO(TOKENID.MINUS, 12, false),
			new OPINFO(TOKENID.STAR, 13, false),
			new OPINFO(TOKENID.SLASH, 13, false),
			new OPINFO(TOKENID.PERCENT, 13, false),
			new OPINFO(TOKENID.UNKNOWN, 14, false),
			new OPINFO(TOKENID.PLUS, 14, false),
			new OPINFO(TOKENID.MINUS, 14, false),
			new OPINFO(TOKENID.TILDE, 14, false),
			new OPINFO(TOKENID.BANG, 14, false),
			new OPINFO(TOKENID.PLUSPLUS, 14, false),
			new OPINFO(TOKENID.MINUSMINUS, 14, false),
			new OPINFO(TOKENID.TYPEOF, 14, false),
			new OPINFO(TOKENID.SIZEOF, 14, false),
			new OPINFO(TOKENID.CHECKED, 14, false),
			new OPINFO(TOKENID.UNCHECKED, 14, false),
			new OPINFO(TOKENID.MAKEREFANY, 14, false),
			new OPINFO(TOKENID.REFVALUE, 14, false),
			new OPINFO(TOKENID.REFTYPE, 14, false),
			new OPINFO(TOKENID.ARGS, 0, false),
			new OPINFO(TOKENID.UNKNOWN, 15, false),                   
			new OPINFO(TOKENID.STAR, 16, false),
			new OPINFO(TOKENID.AMPERSAND, 17, false),
			new OPINFO(TOKENID.COLON, 0, false),                   
			new OPINFO(TOKENID.THIS, 0, false),
			new OPINFO(TOKENID.BASE, 0, false),
			new OPINFO(TOKENID.NULL, 0, false),
			new OPINFO(TOKENID.TRUE, 0, false),
			new OPINFO(TOKENID.FALSE, 0, false),
			new OPINFO(TOKENID.UNKNOWN, 0, false),
			new OPINFO(TOKENID.UNKNOWN, 0, false),
			new OPINFO(TOKENID.UNKNOWN, 0, false),
			new OPINFO(TOKENID.PLUSPLUS, 0, false),
			new OPINFO(TOKENID.MINUSMINUS, 0, false),
			new OPINFO(TOKENID.DOT, 0, false),
			new OPINFO(TOKENID.IMPLICIT, 0, false),
			new OPINFO(TOKENID.EXPLICIT, 0, false),
			new OPINFO(TOKENID.UNKNOWN, 0, false),
			new OPINFO(TOKENID.UNKNOWN, 0, false),
			new OPINFO(TOKENID.UNKNOWN, 0, false),
			new OPINFO(TOKENID.EQUALGREATER, 1, true), // CS3
			new OPINFO(TOKENID.UNKNOWN, 0, false)
		};
    }

    //======================================================================
    // CSimpleParser
    //
    /// <summary></summary>
    //======================================================================
    class CSimpleParser : CParser
    {
        //------------------------------------------------------------
        // CSimpleParser Fields and Properties
        //------------------------------------------------------------

        //------------------------------------------------------------
        // CSimpleParser Constructor
        //
        /// <summary></summary>
        /// <param name="controller"></param>
        //------------------------------------------------------------
        internal CSimpleParser(CController controller)
            : base(controller.NameManager)
        {
        }

        //------------------------------------------------------------
        // CSimpleParser.CreateNewError
        //
        /// <summary></summary>
        /// <param name="iErrorId"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <param name="prgarg"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool CreateNewError(
            CSCERRID iErrorId,
            POSDATA posStart,
            POSDATA posEnd,
            params ErrArg[] prgarg)
        {
            return false;
        }

        //------------------------------------------------------------
        // CSimpleParser.GetLastToken
        //
        /// <summary></summary>
        /// <param name="lastTokenCache"></param>
        /// <param name="node"></param>
        /// <param name="flags"></param>
        /// <param name="missingName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        new internal bool GetLastToken(
            CBasenodeLookupCache lastTokenCache,
            BASENODE node,
            ExtentFlags flags,
            out bool missingName)
        {
            missingName = false;
            return false;
        }

        //------------------------------------------------------------
        // CSimpleParser.AddToNodeTable
        //
        /// <summary></summary>
        /// <param name="pNode"></param>
        //------------------------------------------------------------
        override internal void AddToNodeTable(BASENODE pNode)
        {
        }

        //------------------------------------------------------------
        // CSimpleParser.GetInteriorTree
        //
        /// <summary></summary>
        /// <param name="sourceData"></param>
        /// <param name="node"></param>
        /// <param name="interiorTree"></param>
        //------------------------------------------------------------
        internal override void GetInteriorTree(
            CSourceData sourceData,
            BASENODE node,
            ref CInteriorTree interiorTree)
        {
        }

        //------------------------------------------------------------
        // CSimpleParser.GetLexData
        //
        /// <summary></summary>
        /// <param name="lexData"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override bool GetLexData(LEXDATA lexData)
        {
            DebugUtil.Assert(false, "CSimpleParser::GetLexData:Not yet implemented");
            return false;
        }
    }

    //======================================================================
    // CTextParser
    //
    /// <summary>
    /// This is the object that exposes ICSParser for external clients
    /// </summary>
    //======================================================================
    internal class CTextParser    // : ICSParser
    {
        private CSimpleParser m_pParser = null;
        private CTextLexer m_spLexer = null;

        internal CTextParser()
        {
        }

        internal void Initialize(CController pController, LangVersionEnum cm)
        //// CTextParser::Initialize
        //
        //HRESULT CTextParser::Initialize (CController *pController, CompatibilityMode cm)
        {
            m_pParser = new CSimpleParser (pController);
        //    if (m_pParser == null)
        //        return E_OUTOFMEMORY;
        //
        //    return CTextLexer::CreateInstance (pController.GetNameMgr(), cm, &m_spLexer);
        }

        // ICSParser
        //------------------------------------------------------------
        // CTextParser.SetInputText (ICSParser)
        //------------------------------------------------------------
        virtual internal void SetInputText(string text, int length)
        {
            m_spLexer.SetInput(text, length);
        }

#if false
        //------------------------------------------------------------
        // CTextParser.CreateParseTree  (ICSParser)
        //------------------------------------------------------------
        virtual internal void CreateParseTree(
            ParseTreeScope iScope, BASENODE pParent, out BASENODE ppNode)
        {
            ppNode = null;
            throw new NotImplementedException("CTextParser.CreateParseTree");

        //    LEXDATA     ld;
        //    HRESULT     hr;
        //
        //    if (FAILED (hr = m_spLexer.GetLexResults (&ld)))
        //        return hr;
        //
        //    m_pParser.SetInputData (L"", &ld);  
        //    m_pParser.Rewind (0);
        //
        //    switch (iScope)
        //    {
        //        case PTS_TYPEBODY:
        //        case PTS_ENUMBODY:
        //            return E_NOTIMPL;
        //
        //        case PTS_NAMESPACEBODY: 
        //            {
        //                if (pParent == null)
        //                {
        //                    *ppNode = m_pParser.ParseRootNamespace ();
        //                }
        //                else
        //                {
        //                    *ppNode = m_pParser.ParseNamespace(pParent);
        //                }
        //            }
        //            break;
        //
        //        case PTS_MEMBER_REF_SPECIAL:*ppNode = m_pParser.ParseMemberRefSpecial();   break;
        //        case PTS_STATEMENT:         *ppNode = m_pParser.ParseStatement (pParent);  break;
        //        case PTS_EXPRESION:         *ppNode = m_pParser.ParseExpression (pParent); break;
        //        case PTS_TYPE:              *ppNode = m_pParser.ParseType (pParent);       break;
        //        case PTS_MEMBER:
        //            {
        //                MEMBERNODE* pNode = m_pParser.ParseMember (pParent.AsAGGREGATE);
        //                if (pNode != null)
        //                {
        //                    pNode.pNext = null;
        //                }
        //
        //                *ppNode = pNode;
        //            }
        //            break;
        //        case PTS_PARAMETER:     *ppNode = m_pParser.ParseParameter(pParent, CParser::kppoAllowAll); break;
        //
        //        default:
        //            return E_INVALIDARG;
        //    }
        //
        //    return S_OK;
        }
#endif

        //------------------------------------------------------------
        // CTextParser.AllocateNode (ICSParser)
        //------------------------------------------------------------
        virtual internal void AllocateNode(int iKind, out BASENODE ppNode)
        {
            ppNode = null;
        }

        //------------------------------------------------------------
        // CTextParser.FindLeafNode (ICSParser)
        //------------------------------------------------------------
        virtual internal bool FindLeafNode(POSDATA pos, BASENODE pNode, out BASENODE ppLeafNode)
        {
            ppLeafNode = null;
            throw new NotImplementedException("CTextParser.FindLeafNode");

            //    if (ppLeafNode && m_pParser)
        //    {
        //        *ppLeafNode = m_pParser.FindLeaf(pos, pNode, null, null);
        //        
        //        return (*ppLeafNode == null) ? E_FAIL : S_OK;
        //    }
        //    else
        //    {
        //        return E_FAIL;
        //    }
            //return false;
        }

        //------------------------------------------------------------
        // CTextParser.FindLeafNodeForToken (ICSParser)
        //------------------------------------------------------------
        virtual internal bool FindLeafNodeForToken(int iToken, BASENODE pNode, out BASENODE ppNode)
        {
            ppNode = null;
            throw new NotImplementedException("CTextParser.FindLeafNodeForToken");

            //return m_pParser.FindLeafNodeForTokenEx(iToken, pNode, NULL, EF_FULL, ppNode, NULL);
            //return false;
        }

        //------------------------------------------------------------
        // CTextParser.GetLexResults    (ICSParser)
        //------------------------------------------------------------
        virtual internal int GetLexResults(LEXDATA pLexData)
        {
        //    if (m_spLexer)
        //    {
        //        return m_spLexer.GetLexResults(pLexData);
        //    }
        //    else
        //    {
        //        return E_FAIL;
        //    }
            //return 0;

            throw new NotImplementedException("CTextParser.GetLexResults");
        }

        //------------------------------------------------------------
        // CTextParser.GetExtent    (ICSParser)
        //------------------------------------------------------------
        virtual internal bool GetExtent(
            BASENODE node,out POSDATA posStart,out POSDATA posEnd, ExtentFlags flags)
        {
            posStart = null;
            posEnd = null;
        //    if (m_pParser)
        //    {
        //        CBasenodeLookupCache cache;
        //        return m_pParser.GetExtent(cache, node, flags, posStart, posEnd);
        //    }
        //    else
        //    {
        //        return E_FAIL;
        //    }
            //return false;

            throw new NotImplementedException("CTextParser.GetExtent");
        }

        //------------------------------------------------------------
        // CTextParser.GetTokenExtent   (ICSParser)
        //------------------------------------------------------------
        virtual internal bool GetTokenExtent(
            BASENODE pNode, out int piFirstToken, out int piLastToken, ExtentFlags flags)
        {
            piFirstToken = 0;
            piLastToken = 0;

            //    if (m_pParser)
            //    {
            //        m_pParser.GetTokenExtent(pNode, piFirstToken, piLastToken, flags);
            //        return NOERROR;
            //    }
            //    else
            //    {
            //        return E_FAIL;
            //    }
            //return false;

            throw new NotImplementedException("CTextParser.GetTokenExtent");
        }
    }

    //======================================================================
    // CListMaker
    //
    /// <summary>
    /// Create lists by binary trees.
    /// </summary>
    ////======================================================================
    internal class CListMaker
    {
        //------------------------------------------------------------
        // CListMaker   Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// <para>A starting node of a list.</para>
        /// <para>If the list has only one element, this is the element.</para>
        /// <para>Otherwise, this is a BINOPNODE withe NODEKIND.LIST whose left operand is the first element.</para>
        /// </summary>
        private BASENODE rootListNode = null;

        /// <summary>
        /// <para>If this list has no element, this is null.</para>
        /// <para>If this list has only one element, this is the element.</para>
        /// <para>Otherwise, this is the last BINOPNODE of this list.</para>
        /// </summary>
        private BASENODE currentListNode = null;

        private CParser parser = null;

        //------------------------------------------------------------
        // CListMaker   Constructor
        //
        /// <summary></summary>
        /// <param name="psr"></param>
        //------------------------------------------------------------
        internal CListMaker(CParser parser)
        {
            DebugUtil.Assert(parser != null);
            this.parser = parser;
        }

        //------------------------------------------------------------
        // CListMaker.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            this.rootListNode = null;
            this.currentListNode = null;
        }

        //------------------------------------------------------------
        // CListMaker.Add
        //
        /// <summary>
        /// <para>Add a node to the list.</para>
        /// <para>(In sscli, tokenIndex has the default value -1.)</para>
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE Add(BASENODE newNode, int tokenIndex)	// = -1)
        {
            // Handle null -- this is not an error case; some functions return null after
            // reporting an error, which should not corrupt the list.
            if (newNode == null)
            {
                return null;
            }

            //ASSERT (pNew->pParent != NULL && pNew->pParent != (BASENODE *)I64(0xcccccccccccccccc));
            DebugUtil.Assert(newNode.ParentNode != null);

            // If null, *currentListNode just gets the value of newNode, which should already have
            // it's parent relationship set up (we assert that above).

            if (this.rootListNode == null)
            {
                this.rootListNode = newNode;
                this.currentListNode = newNode;
                return this.currentListNode;
            }

            BINOPNODE listNode;

            if (this.currentListNode.Kind == NODEKIND.LIST)
            {
                listNode = this.parser.AllocNode(
                    NODEKIND.LIST, currentListNode, tokenIndex).AsLIST;

                listNode.Operand1 = this.currentListNode.AsLIST.Operand2;
                listNode.Operand2 = newNode;
                currentListNode.AsLIST.Operand2 = listNode;
            }
            else
            {
                listNode = this.parser.AllocNode(
                    NODEKIND.LIST, currentListNode.ParentNode, tokenIndex).AsLIST;

                // this list has only one element and rootListNode points the element.
                listNode.Operand1 = this.currentListNode;
                listNode.Operand2 = newNode;
                this.rootListNode = listNode;
            }

            //DebugUtil.Assert(listNode.ParentNode == listNode.Operand1.ParentNode);//There was one failure, why?
            listNode.Operand1.ParentNode = listNode.Operand2.ParentNode = listNode;
            this.currentListNode = listNode;
            DebugUtil.Assert(this.currentListNode.AsLIST.Operand2.Kind !=NODEKIND.LIST);
            return listNode;
            // Return the new node in case the caller wants to store flags/position, etc.
        }

        //------------------------------------------------------------
        // CListMaker.GetList
        //
        /// <summary>
        /// Set the parent node of the list and return the starting node of the list.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE GetList(BASENODE parent)
        {
            if (this.rootListNode != null)
            {
                this.rootListNode.ParentNode = parent;
            }
            return this.rootListNode;
        }
    }

    //======================================================================
    // class CEnsureParserProgress
    //======================================================================
    internal class CEnsureParserProgress
    {
        private CParser parser = null;
        private int startIndex;

        //------------------------------------------------------------
        // CEnsureParserProgress Constructor
        //------------------------------------------------------------
        internal CEnsureParserProgress(CParser psr)
        {
            DebugUtil.Assert(psr != null);

            parser = psr;
            startIndex = parser.CurrentTokenIndex();
        }

        //------------------------------------------------------------
        // CEnsureParserProgress.EnsureProgress
        //
        /// <summary>
        /// <para>Substitution of the destructor in sscli.</para>
        /// <para>Call this before exiting the methods.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void EnsureProgress()
        {
            // if the cursor wasn't advanced then this wasn't a valid Statement.
            // Move forward one and start again.
            if (parser.CurrentTokenIndex() == startIndex)
            {
                parser.NextToken();
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //// CParser::TokenInfoArray
    //
    //#define TOK(name, id, flags, type, stmtfunc, binop, unop, selfop, color) { name, flags, sizeof(name)/sizeof(WCHAR)-1, stmtfunc, type, binop, unop, selfop },
    //#define PARSEFN(func) CParser::EParse##func
    //#define NOPARSEFN CParser::EParseNone
    //const TOKINFO   CParser::m_rgTokenInfo[TID_NUMTOKENS] = {
    //    #include "tokens.h"
    //};
    //#undef  PARSEFN
    //#undef  NOPARSEFN
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CParser::OperatorInfoArray
    //
    //#define OP(n,p,a,stmt,t,pn,e) {t,p,a},
    //const OPINFO    CParser::OperatorInfoArray[] = {
    //    #include "ops.h"
    //    { TID_UNKNOWN, 0, 0}
    //};

    //////////////////////////////////////////////////////////////////////////////////
    //// CParser::~CParser
    //
    //CParser::~CParser ()
    //{
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CParser::ErrorAtPosition
    //
    //void CParser::ErrorAtPositionArgs(long iLine, long iCol, long iExtent, CSCERRID id, int carg, ErrArg * prgarg)
    //{
    //    POSDATA pos(iLine, iCol);
    //    POSDATA posEnd(iLine, iCol + max (iExtent, 1));
    //    CreateNewError(id, carg, prgarg, pos, posEnd);
    //    parseErrorCount++;
    //}

    //// Include the code stolen from COM+ for numeric conversions...
    //namespace NumLit
    //{
    //#include "numlit.h"
    //};
    //
    //using namespace NumLit;

    //////////////////////////////////////////////////////////////////////////////////
    //// CParser::ParseMemberRef
    ////
    //// This list is out of date. It also parses id<typeargs> etc and (for CSEE) id.id.~id(...)
    //// This function will parse a member reference that looks like:
    ////    id
    ////    id(arglist)
    ////    id.id.id
    ////    id.id(arglist)
    ////    this
    ////    this[arglist]
    ////    id.id.this
    ////    id.id.this[arglist]
    ////    operator <op>
    ////    operator <op> (arglist)
    ////    id.id.operator <op>
    ////    id.id.operator <op> (arglist)
    ////    <implicit|explicit> operator <type>
    ////    <implicit|explicit> operator <type> (arglist)
    ////    id.id.<implicit|explicit> operator <type>
    ////    id.id.<implicit|explicit> operator <type> (arglist)
    ////    alias::id
    ////    alias::id(arglist)
    ////    alias::id.id.id
    ////    alias::id.id(arglist)
    ////    alias::id.id.this
    ////    alias::id.id.this[arglist]
    ////    alias::id.id.operator <op>
    ////    alias::id.id.operator <op> (arglist)
    ////    alias::id.id.<implicit|explicit> operator <type>
    ////    alias::id.id.<implicit|explicit> operator <type> (arglist)
    ////
    ////    (First id can also be a predefined type (int, string, etc.)
    ////
    //// This is used for parsing cref references in XML comments. The returned
    //// NK_METHOD node has only the following fields filled in:
    ////    pName, pParms (if parameters), pType (if conversion operator).
    ////
    //// The pName field can actually have a NK_OP node in it for an operator <op>
    //// or "this" (OP_THIS). This is different than how operator/indexer are usually
    //// represented.
    ////
    //// CSEE: in the ee this is also used to parse named breakpoint locations
    //// CSEE: which consist of a function reference possibly followed by a
    //// CSEE: comma
    //METHODNODE * CParser::ParseMemberRefSpecial()
    //{
    //    METHODNODE * meth = AllocNode(NK_METHOD, null, -1).asMETHOD();
    //    meth.pAttr         = null;
    //    meth.pNext         = null;
    //    meth.iStart        = -1;
    //    meth.iClose        = -1;
    //
    //    meth.pType         = null;
    //    meth.iOpenParen    = -1;
    //    meth.pParms        = null;
    //    meth.iCloseParen   = -1;
    //    meth.iOpen         = -1;
    //    meth.pBody         = null;
    //    meth.pInteriorNode = null;
    //
    //    meth.pName         = null;
    //    meth.pConstraints  = null;
    //
    //    ParseDottedNameSpecial(meth);
    //
    //    return meth;
    //}
    //
    //
    //void CParser::ParseDottedNameSpecial(METHODNODE * meth)
    //{
    //    ASSERT(!meth.pName);
    //    ASSERT(!meth.pParms);
    //
    //    long itokDot = -1;
    //
    //    for (bool isFirst = true; ; isFirst = false) {
    //        // Get a name component: id, this, operator.
    //        TOKENID tokCur = CurrentTokenID();
    //        BASENODE * node;
    //
    //        switch (tokCur) {
    //        default:
    //            if (!meth.pName && (TokenInfoArray[tokCur].dwFlags & TFF_PREDEFINED)) {
    //                // This is a predefined type.
    //                node = ParsePredefinedType(meth);
    //                break;
    //            }
    //            // Fall through.
    //        case TID_VOID:
    //            CheckToken(TID_IDENTIFIER);
    //            return;
    //
    //        case TID_IDENTIFIER:
    //            node = ParseSingleNameSpecial(meth);
    //            if (isFirst)
    //                CheckForAlias(node);
    //            break;
    //
    //        case TID_THIS:
    //            AddToDottedListSpecial(&meth.pName, AllocOpNode(OP_THIS, CurrentTokenIndex(), meth), itokDot);
    //            NextToken();
    //            ParseParamListSpecial(meth, true);
    //            return;
    //
    //
    //        case TID_OPERATOR:
    //        case TID_IMPLICIT:
    //        case TID_EXPLICIT:
    //            AddToDottedListSpecial(&meth.pName, ParseOperatorSpecial(meth), itokDot);
    //            // ParserOperatorSpecial parses the params and checks eof as well.
    //            return;
    //        }
    //
    //        AddToDottedListSpecial(&meth.pName, node, itokDot);
    //
    //        if (CurrentTokenID() != TID_DOT && CurrentTokenID() != TID_COLONCOLON) {
    //            ParseParamListSpecial(meth, false);
    //            return;
    //        }
    //
    //        itokDot = CurrentTokenIndex();
    //        if (node.kind != NK_ALIASNAME)
    //            CheckToken(TID_DOT);
    //        NextToken();
    //    }
    //}
    //
    //
    //void CParser::AddToDottedListSpecial(BASENODE ** pnodeAll, BASENODE * node, int itokDot)
    //{
    //    ASSERT(pnodeAll);
    //    ASSERT(node);
    //
    //    if (!*pnodeAll)
    //        *pnodeAll = node;
    //    else {
    //        ASSERT(itokDot >= 0);
    //        *pnodeAll = AllocDotNode(itokDot, node.pParent, *pnodeAll, node);
    //    }
    //}
    //
    //
    //// Parses the parameter list for XML references and EE breakpoints.
    //// Checks for EOF.
    //int CParser::ParseParamListSpecial(METHODNODE * meth, bool fThis)
    //{
    //    ASSERT(!meth.pParms);
    //
    //    TOKENID tokCur = CurrentTokenID();
    //    TOKENID tokEnd;
    //
    //    switch (tokCur) {
    //    default:
    //        CheckEofSpecial(meth, fThis);
    //        return 0;
    //
    //    case TID_OPENPAREN:
    //        if (fThis) {
    //            CheckToken(TID_OPENSQUARE);
    //            return 0;
    //        }
    //        tokEnd = TID_CLOSEPAREN;
    //        break;
    //
    //    case TID_OPENSQUARE:
    //        if (!fThis) {
    //            CheckToken(TID_OPENPAREN);
    //            return 0;
    //        }
    //        tokEnd = TID_CLOSESQUARE;
    //        break;
    //    }
    //    NextToken();
    //
    //    // Distinguish empty arg list from no args.
    //    if (CurrentTokenID() == tokEnd) {
    //        NextToken();
    //        meth.flags |= NF_MEMBERREF_EMPTYARGS;
    //        CheckEofSpecial(meth, fThis);
    //        return 0;
    //    }
    //
    //    CListMaker list(this, &meth.pParms);
    //    long itokComma = -1;
    //    int carg = 0;
    //
    //    for (;;) {
    //        list.Add(ParseParameter(meth, kppoNoNames | kppoNoParams | kppoNoVarargs | kppoNoAttrs), itokComma);
    //        carg++;
    //        if (CurrentTokenID() != TID_COMMA)
    //            break;
    //        itokComma = CurrentTokenIndex();
    //        NextToken();
    //    }
    //
    //    Eat(tokEnd);
    //    CheckEofSpecial(meth, fThis);
    //
    //    return carg;
    //}
    //
    //
    //void CParser::CheckEofSpecial(METHODNODE * meth, bool fThis)
    //{
    //    switch (CurrentTokenID()) {
    //    default:
    //        if (meth.pParms || (meth.flags & NF_MEMBERREF_EMPTYARGS)) {
    //            // A param list was specified.
    //            Error(ERR_SyntaxError, L"\"");
    //            break;
    //        }
    //        CheckToken(fThis ? TID_OPENSQUARE : TID_OPENPAREN);
    //        break;
    //
    //
    //    case TID_ENDFILE:
    //        break;
    //    }
    //}
    //
    //
    //NAMENODE * CParser::ParseSingleNameSpecial(BASENODE * nodePar)
    //{
    //    if (!CheckToken(TID_IDENTIFIER))
    //        return ParseMissingName(nodePar, CurrentTokenIndex());
    //
    //    TOKENID tokNext = PeekToken();
    //    if (tokNext == TID_OPENANGLE) {
    //        GENERICNAMENODE * gen = AllocGenericNameNode(null, CurrentTokenIndex());
    //        gen.pParent = nodePar;
    //        NextToken();
    //
    //        gen.iOpen = CurrentTokenIndex();
    //        gen.pParams = ParseInstantiation(gen, false, &gen.iClose);
    //
    //        return gen;
    //    }
    //
    //    NAMENODE * name = AllocNameNode(null, CurrentTokenIndex());
    //    name.pParent = nodePar;
    //    NextToken();
    //
    //    return name;
    //}
    //
    //
    //BASENODE * CParser::ParseOperatorSpecial(METHODNODE * meth)
    //{
    //    BASENODE * node;
    //    TOKENID tokOp = CurrentTokenID();
    //
    //    switch (tokOp) {
    //    default:
    //        ASSERT(0);
    //        return null;
    //
    //    case TID_OPERATOR:
    //        NextToken();
    //
    //        tokOp = CurrentTokenID();
    //        if (tokOp == TID_IMPLICIT || tokOp == TID_EXPLICIT) {
    //            Error(ERR_BadOperatorSyntax, TokenInfoArray[tokOp].pszText);
    //            return null;
    //        }
    //
    //        // check for >>
    //        if (tokOp == TID_GREATER && PeekToken() == TID_GREATER) {
    //            POSDATA &posFirst(CurTokenPos());
    //            POSDATA &posNext(tokenList[PeekTokenIndex()]);
    //            if (posFirst.iLine == posNext.iLine && (posFirst.iChar + 1) == posNext.iChar)
    //            {
    //                tokOp = TID_SHIFTRIGHT;
    //                NextToken();
    //            }
    //        }
    //
    //        node = AllocOpNode((OPERATOR)0, CurrentTokenIndex(), meth); // actual operator filled in later...
    //        NextToken();
    //        break;
    //
    //    case TID_IMPLICIT:
    //    case TID_EXPLICIT:
    //        NextToken();
    //
    //        if (CurrentTokenID() != TID_ENDFILE)
    //            Eat(TID_OPERATOR);
    //
    //        node = AllocOpNode((OPERATOR)0, CurrentTokenIndex(), meth); // actual operator filled in later...
    //
    //        meth.pType = ParseType(meth);
    //        break;
    //    }
    //
    //    // We can't validate the operator until we know the number of parameters.
    //    int carg = ParseParamListSpecial(meth, false);
    //
    //    if (!carg) {
    //        // No parms given -- pick the right one (binary over unary if both).
    //        if (TokenInfoArray[tokOp].dwFlags & TFF_OVLBINOP)
    //            node.other = (OPERATOR)TokenInfoArray[tokOp].iBinaryOp;
    //        else if (TokenInfoArray[tokOp].dwFlags & TFF_OVLUNOP)
    //            node.other = (OPERATOR)TokenInfoArray[tokOp].iUnaryOp;
    //        else
    //            ErrorAtToken(node.tokidx, ERR_OvlOperatorExpected);
    //    }
    //    else if (carg == 1) {
    //        node.other = (OPERATOR)TokenInfoArray[tokOp].iUnaryOp;
    //        if (!(TokenInfoArray[tokOp].dwFlags & TFF_OVLUNOP))
    //            ErrorAtToken(node.tokidx, ERR_OvlUnaryOperatorExpected);
    //    }
    //    else {
    //        node.other = (OPERATOR)TokenInfoArray[tokOp].iBinaryOp;
    //        if (!(TokenInfoArray[tokOp].dwFlags & TFF_OVLBINOP))
    //            ErrorAtToken(node.tokidx, ERR_OvlBinaryOperatorExpected);
    //    }
    //
    //    return node;
    //}

    //======================================================================
    // class CBasenodeLookupCache
    //
    /// <summary>
    /// <para>(sscli) csharp\sccomp\basenodelookupcache.h</para>
    /// </summary>
    //======================================================================
    internal class CBasenodeLookupCache
    {
    }

    //======================================================================
    // interface IPComparer
    //======================================================================
    internal interface IPComparer<T1, T2>
    {
        int Compare(T1 t1, T2 t2);
    }

    //======================================================================
    // class CContainingNodeFinder
    //======================================================================
    internal class CContainingNodeFinder : IPComparer<POSDATA, BASENODE>
    {
        //------------------------------------------------------------
        // CContainingNodeFinder    Fields
        //------------------------------------------------------------
        // immutable state
        private CParser parser;
        private CBasenodeLookupCache lastTokenChache;
        private ExtentFlags flags;
        private POSDATA pos;
        private List<BASENODE> array;

        //------------------------------------------------------------
        // CContainingNodeFinder.StableBinarySearch
        //
        /// <summary>
        /// <para>Finds the first element in the list that matches the search criteria.
        /// Useful for when the list contains duplicates</para>
        /// <para>In sscli, csharp\inc\atlarrayqsort.h</para>
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="ar"></param>
        /// <param name="elem"></param>
        /// <param name="pComparer"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal int StableBinarySearch<T1, T2>(
            List<T1> ar,
            T2 elem,
            IPComparer<T2, T1> pComparer)
        {
            int low = 0;
            int high = (int)ar.Count - 1;
            while (low < high)
            {
                // dont' touch this code under penalty of death.  It's subtle!
                // (Talk to renaud if yoiu absolutely have to)
                int median = (low + high) / 2;
                int compareResult = pComparer.Compare(elem, ar[median]);
                if (compareResult > 0)
                {
                    low = median + 1;
                }
                else
                {
                    high = median;
                }
            }

            // min is position of 1st elem or insertion pos
            return low;
        }

        //------------------------------------------------------------
        // CContainingNodeFinder    Constructor
        //------------------------------------------------------------
        internal CContainingNodeFinder
            (CParser pParser,
            CBasenodeLookupCache lastTokenCache,
            POSDATA pos,
            List<BASENODE> array,
            ExtentFlags flags)
        {
            this.parser = pParser;
            this.lastTokenChache = lastTokenCache;
            this.flags = flags;
            this.pos = pos;
            this.array = array;
        }

        //------------------------------------------------------------
        // CContainingNodeFinder.Do
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal BASENODE Do()
        {
            if (array.Count == 0)
            {
                return null;
            }

            int bestIndex = StableBinarySearch<BASENODE, POSDATA>(array, pos, this);
            int lastArrayElement = array.Count - 1;

            // If the element is not in the array, and would be inserted at the 
            // end of the array.  Then just return.  We didn't find it.

            if (bestIndex > lastArrayElement)
            {
                return null;
            }

            // CHeck that we actually found a valid element (as opposed ot 
            // just a position where the element would have gone).

            BASENODE bestNode = array[bestIndex];
            int bestNodeCompare = this.Compare(pos, bestNode);
            if (bestNodeCompare != 0)
            {
                return null;
            }

            // If we're the last element of the array, then there's nothing more to look at
            // we're good to go.

            if (bestIndex == lastArrayElement)
            {
                return bestNode;
            }

            // if the caller has told us not to look at the right side of the cursor
            // then just return the best node we've found so far.

            if ((flags & ExtentFlags.PREFER_LEFT_NODE) != 0)
            {
                return bestNode;
            }

            // Ok, the cursor could be right at the end of the member or statement.  It 
            // could be code like:
            //
            //s DoFoo() {
            //}s DoBoo() {}

            // In which case we need to check the next member

            POSDATA posStart1 = new POSDATA();
            POSDATA posEnd1 = new POSDATA();
            POSDATA posStart2 = new POSDATA();
            POSDATA posEnd2 = new POSDATA();
            BASENODE nextNode = array[bestIndex + 1];
            if (parser.GetExtent(lastTokenChache, bestNode, flags, posStart1, posEnd1) ||
                parser.GetExtent(lastTokenChache, nextNode, flags, posStart2, posEnd2))
            {
                return bestNode;
            }

            if (pos == posEnd1 && pos == posStart2)
            {
                return nextNode;
            }

            return bestNode;
        }

        //------------------------------------------------------------
        // CContainingNodeFinder.Compare
        //
        /// <summary>
        /// pos が cpNode を含むプログラム構造内を指している場合に等しいとする。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="cpNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public int Compare(POSDATA pos, BASENODE cpNode)
        {
            POSDATA posStart=new POSDATA();
            POSDATA posEnd = new POSDATA();
            BASENODE pNode = cpNode;

            parser.GetExtent(lastTokenChache, pNode, flags, posStart, posEnd);

            if (pos < posStart)
            {
                return -1;
            }
            else if (pos > posEnd)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    //HRESULT CParser::FindLeafNodeForTokenEx(long iToken, BASENODE* pNode, CSourceData* pData, ExtentFlags flags, BASENODE** ppNode, ICSInteriorTree** ppTree)
    //{
    //    HRESULT hr;
    //    LEXDATA lex;
    //    if (FAILED (hr = this.GetLexData (&lex)))
    //        return hr;
    //
    //    if (iToken < 0 || iToken >= lex.TokenCount())
    //        return E_INVALIDARG;
    //
    //    const CSTOKEN& token = lex.TokenAt(iToken);
    //    POSDATA pos = token;
    //
    //    CBasenodeLookupCache cache;
    //    *ppNode = this.FindLeafEx (cache, pos, pNode, ppTree == null ? null : pData, flags, ppTree);
    //    if (*ppNode == null)
    //    {
    //        return E_FAIL;
    //    }
    //
    //
    //    // Now, check that FindLeafNode did a good job for corner cases
    //    // and compensate if it didn't
    //    long    iTokenFirst;
    //    long    iTokenLast;
    //    this.GetTokenExtent (*ppNode, &iTokenFirst, &iTokenLast, EF_FULL);
    //
    //    // The 1st token is past the given token, we need to go backward a little bit
    //    if ((iTokenFirst > iToken) && (pos.iChar > 0))
    //    {
    //        pos.iChar--;
    //        if (ppTree && (*ppTree))
    //        {
    //            (*ppTree).Release();
    //            (*ppTree) = null;
    //        }
    //        *ppNode = this.FindLeafEx (cache, pos, pNode, ppTree == null ? null : pData, flags, ppTree);
    //    }
    //    // The last token is before the given token, we need to go forward a little bit
    //    else if (iTokenLast < iToken)
    //    {
    //        if (ppTree && (*ppTree))
    //        {
    //            (*ppTree).Release();
    //            (*ppTree) = null;
    //        }
    //        *ppNode = this.FindLeafEx(cache, token.StopPosition(), pNode, ppTree == null ? null : pData, flags, ppTree);
    //    }
    //
    //    //null is how FindLeafEx indicates failure;
    //    return *ppNode == null ? E_FAIL : S_OK;
    //}
    //
    //////////////////////////////////////////////////////////////////////////////////
    //
    //
    //////////////////////////////////////////////////////////////////////////////////
    //// CParser::GetTokenExtent
    //
    //void CParser::GetTokenExtent (BASENODE *pNode, long *piFirst, long *piLast, ExtentFlags flags)
    //{
    //    BOOL    fMissingName;
    //
    //    if (piFirst != null)
    //        *piFirst = GetFirstToken (pNode, flags, &fMissingName);
    //
    //    CBasenodeLookupCache cache;
    //    if (piLast != null)
    //        *piLast = GetLastToken (cache, pNode, flags, &fMissingName);
    //}

    //////////////////////////////////////////////////////////////////////////////////
    //// CTextParser::~CTextParser
    //
    //CTextParser::~CTextParser ()
    //{
    //    if (m_pParser != null)
    //        delete m_pParser;
    //}
}
