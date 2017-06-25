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
// File: enum.h
//
// Defines various enumerations such as TOKENID, etc.
//
// ===========================================================================

//============================================================================
// Enum.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // enum TOKENID
    //
    /// <summary>
    /// (TID_)
    /// (CSharp\Inc\Enum.cs)
    /// </summary>
    //======================================================================
    internal enum TOKENID : uint
    {
        UNDEFINED = 0,  // hirano567@hotmail.co.jp

        ARGS,
        MAKEREFANY,
        REFTYPE,
        REFVALUE,
        ABSTRACT,
        AS,
        BASE,
        BOOL,
        BREAK,
        BYTE,
        CASE,
        CATCH,
        CHAR,
        CHECKED,
        CLASS,
        CONST,
        CONTINUE,
        DECIMAL,
        DEFAULT,
        DELEGATE,
        DO,
        DOUBLE,
        ELSE,
        ENUM,
        EVENT,
        EXPLICIT,
        EXTERN,
        FALSE,
        FINALLY,
        FIXED,
        FLOAT,
        FOR,
        FOREACH,
        GOTO,
        IF,
        IN,
        IMPLICIT,
        INT,
        INTERFACE,
        INTERNAL,
        IS,
        LOCK,
        LONG,
        NAMESPACE,
        NEW,
        NULL,
        OBJECT,
        OPERATOR,
        OUT,
        OVERRIDE,
        PARAMS,
        PRIVATE,
        PROTECTED,
        PUBLIC,
        READONLY,
        REF,
        RETURN,
        SBYTE,
        SEALED,
        SHORT,
        SIZEOF,
        STACKALLOC,
        STATIC,
        STRING,
        STRUCT,
        SWITCH,
        THIS,
        THROW,
        TRUE,
        TRY,
        TYPEOF,
        UINT,
        ULONG,
        UNCHECKED,
        UNSAFE,
        USHORT,
        USING,
        VIRTUAL,
        VOID,
        VOLATILE,
        WHILE,
        IDENTIFIER, // この ID より前のものはキーワードである。
        NUMBER,
        STRINGLIT,
        VSLITERAL,
        CHARLIT,
        SLCOMMENT,
        DOCCOMMENT,
        MLCOMMENT,
        MLDOCCOMMENT,
        SEMICOLON,
        CLOSEPAREN,
        CLOSESQUARE,
        OPENCURLY,
        CLOSECURLY,
        COMMA,
        EQUAL,
        PLUSEQUAL,
        MINUSEQUAL,
        SPLATEQUAL,
        SLASHEQUAL,
        MODEQUAL,
        ANDEQUAL,
        HATEQUAL,
        BAREQUAL,
        SHIFTLEFTEQ,
        SHIFTRIGHTEQ,
        QUESTION,
        COLON,
        COLONCOLON,
        LOG_OR,
        LOG_AND,
        BAR,
        HAT,
        AMPERSAND,
        EQUALEQUAL,
        NOTEQUAL,
        LESS,
        LESSEQUAL,
        GREATER,
        GREATEREQUAL,
        SHIFTLEFT,
        SHIFTRIGHT,
        PLUS,
        MINUS,
        STAR,
        SLASH,
        PERCENT,
        TILDE,
        BANG,
        PLUSPLUS,
        MINUSMINUS,
        OPENPAREN,
        OPENSQUARE,
        DOT,
        ARROW,
        QUESTQUEST,
        EQUALGREATER,   // CS3
        ENDFILE,
        UNKNOWN,
        INVALID,
        NUMTOKENS,
        OPENANGLE = LESS,
        CLOSEANGLE = GREATER,
    }

    //======================================================================
    // enum OPERATOR
    //
    /// <summary>
    /// Represents operators.
    /// (OP_)
    /// (CSharp\Inc\Enum.cs)
    /// </summary>
    //======================================================================
    internal enum OPERATOR : uint
    {
        //UNDEFINED,  // hirano567@hotmail.co.jp

        NONE,
        ASSIGN,
        ADDEQ,
        SUBEQ,
        MULEQ,
        DIVEQ,
        MODEQ,
        ANDEQ,
        XOREQ,
        OREQ,
        LSHIFTEQ,
        RSHIFTEQ,
        QUESTION,
        VALORDEF,
        LOGOR,
        LOGAND,
        BITOR,
        BITXOR,
        BITAND,
        EQ,
        NEQ,
        LT,
        LE,
        GT,
        GE,
        IS,
        AS,
        LSHIFT,
        RSHIFT,
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        NOP,
        UPLUS,
        NEG,
        BITNOT,
        LOGNOT,
        PREINC,
        PREDEC,
        TYPEOF,
        SIZEOF,
        CHECKED,
        UNCHECKED,
        MAKEREFANY,
        REFVALUE,
        REFTYPE,
        ARGS,
        CAST,
        INDIR,
        ADDR,
        COLON,
        THIS,
        BASE,
        NULL,
        TRUE,
        FALSE,
        CALL,
        DEREF,
        PAREN,
        POSTINC,
        POSTDEC,
        DOT,
        IMPLICIT,
        EXPLICIT,
        EQUALS,
        COMPARE,
        DEFAULT,
        LAMBDA, // CS3
        LAST
    }

    //======================================================================
    // enum PREDEFTYPE
    //
    // Defined in PredefType.cs
    //======================================================================

    //======================================================================
    // enum NODEKIND
    //
    /// <summary>
    /// (NK_)
    /// (CSharp\Inc\Enum.cs)
    /// </summary>
    //======================================================================
    internal enum NODEKIND : uint
    {
        UNDEFINED = 0,  // hirano567@hotmail.co.jp

        /// <summary>
        /// アクセッサを表すノード。
        /// </summary>
        ACCESSOR,

        /// <summary>
        /// NAMENODE を使う。
        /// </summary>
        ALIASNAME,

        /// <summary>
        /// <para>Derives from BASENODE. Represents anonymous methods.</para>
        /// <para>Has a list of arguments, block of codes, the index of the closing "}".</para>
        /// </summary>
        ANONBLOCK,

        /// <summary>
        /// Use UNOPNODE.
        /// </summary>
        ARRAYINIT,

        /// <summary>
        /// TYPEBASENODE の派生クラス。配列の要素の型と次元を格納する。
        /// </summary>
        ARRAYTYPE,

        /// <summary>
        /// BINOPNODE を使う。
        /// </summary>
        ARROW,

        /// <summary>
        /// 属性の指定を表すためのクラス。引数のノードをフィールドに持つ。
        /// </summary>
        ATTR,

        /// <summary>
        /// ATTRNODE を使う。
        /// </summary>
        ATTRARG,

        /// <summary>
        /// 属性の宣言を表すノード。
        /// </summary>
        ATTRDECL,

        /// <summary>
        /// 二項演算子を表すノード。それの対象となる項をフィールドに持つ。
        /// </summary>
        BINOP,

        /// <summary>
        /// <para>STATEMENTNODE の派生クラス。コードブロックを表す。</para>
        /// <para>文のリストと終了インデックスを持つ。</para>
        /// </summary>
        BLOCK,

        /// <summary>
        /// EXPRSTMTNODE を使う。
        /// </summary>
        BREAK,

        /// <summary>
        /// 関数の呼び出しを表す BINOPNODE の派生クラス。
        /// </summary>
        CALL,

        /// <summary>
        /// case 文を表すための BASENODE の派生クラス。
        /// </summary>
        CASE,

        /// <summary>
        /// UNOPNODE を使う。
        /// </summary>
        CASELABEL,

        /// <summary>
        /// <para>catch ブロックまたは finally ブロックを表すための BASENODE の派生クラス。</para>
        /// <para>親となる TRYSTMTNODE インスタンスの Flags を使って catch か finally かを示す。</para>
        /// <para>型、名前、ブロックを表すノードを持つ。</para>
        /// </summary>
        CATCH,

        /// <summary>
        /// LABELSTMTNODE を使う。
        /// </summary>
        CHECKED,

        /// <summary>
        /// AGGREGATENODE の派生ノード。クラスを表す。
        /// </summary>
        CLASS,

        /// <summary>
        /// FIELDNODE を使う。
        /// </summary>
        CONST,

        /// <summary>
        /// 制約を表すためのノード。
        /// </summary>
        CONSTRAINT,

        /// <summary>
        /// 各種の定数値を格納するためのクラス。CONSTVAL 型のフィールドを持つ。
        /// </summary>
        CONSTVAL,

        /// <summary>
        /// EXPRSTMTNODE を使う。
        /// </summary>
        CONTINUE,

        /// <summary>
        /// CTORMETHODNODE を使う。
        /// </summary>
        CTOR,

        /// <summary>
        /// 変数の宣言文を表すための STATEMENTNODE の派生クラス。
        /// 1 つの型と複数の変数を保持する。
        /// </summary>
        DECLSTMT,

        /// <summary>
        /// デリゲートを表すノード。
        /// </summary>
        DELEGATE,

        /// <summary>
        /// CALLNODE を使う。
        /// </summary>
        DEREF,

        /// <summary>
        /// LOOPSTMTNODE を使う。
        /// </summary>
        DO,

        /// <summary>
        /// BINOPNODE を使う。
        /// </summary>
        DOT,

        /// <summary>
        /// METHODBASENODE を使う。
        /// </summary>
        DTOR,

        /// <summary>
        /// <para>EXPRSTMTNODE -- a multi-purpose statement node
        /// (expression statements, goto, case, default, return, etc.)</para>
        /// <para>STATEMENTNODE の派生クラス。ArgumentsNode フィールドを持つ。</para>
        /// </summary>
        EMPTYSTMT,

        /// <summary>
        /// AGGREGATENODE の派生ノード。列挙型を表す。
        /// </summary>
        ENUM,

        /// <summary>
        /// <para>member for enums</para>
        /// <para>MEMBERNODE の派生クラスで、列挙型の各メンバを表す。名前と値を表すノードを持つ。</para>
        /// </summary>
        ENUMMBR,

        /// <summary>
        /// EXPRSTMTNODE。
        /// </summary>
        EXPRSTMT,

        /// <summary>
        /// MEMBERNODE の派生クラス。フィールドを表す。
        /// </summary>
        FIELD,

        /// <summary>
        /// <para>for 文、foreach 文を表すための STATEMENTNODE の派生クラス。</para>
        /// <para>foreach 文の場合、Flags に NODEFLAGS.FOR_FOREACH をセットする。</para>
        /// <para>fixed 文もこのクラスを利用する。
        /// この場合、Flags に NODEFLAGS.FIXED_DECL をセットする。
        /// 固定ポインタの宣言の解析結果は InitialNode に設定する。</para>
        /// <para>using 文もこのクラスを利用する。
        /// この場合、Flags に NODEFLAGS.USING_DECL をセットする。
        /// リソース取得の解析結果は InitialNode か ConditionNode に設定する。</para>
        /// </summary>
        FOR,

        /// <summary>
        /// <para>generic 名を表すための NAMENODE の派生クラス。</para>
        /// <para>パラメータを表すノード列と "&lt;"、"&gt;" のインデックスを保持する。</para>
        /// </summary>
        GENERICNAME,
        
        /// <summary>
        /// EXPRSTMTNODE を使う。
        /// </summary>
        GOTO,

        /// <summary>
        /// if 文を表すための STATEMENTNODE の派生クラス。
        /// </summary>
        IF,

        /// <summary>
        /// AGGREGATENODE の派生ノード。インターフェースを表す。
        /// </summary>
        INTERFACE,

        /// <summary>
        /// <para>ラベル文を表すための STATEMENTNODE の派生クラス。</para>
        /// <para>check 文、uncheck 文にも使用される。（NODEKIND.CHECKED）</para>
        /// <para>unsafe 文にも使用される。（NODEKIND.UNSAFE）</para>
        /// </summary>
        LABEL,

        /// <summary>
        /// BINOPNODE を使う。
        /// </summary>
        LIST,

        /// <summary>
        /// LOOPSTMTNODE を使う。
        /// </summary>
        LOCK,

        /// <summary>
        /// 集合型のメンバを表すノードの基本クラス。
        /// </summary>
        MEMBER,

        /// <summary>
        /// METHODBASENODE の派生クラス。メソッドを表す。
        /// </summary>
        METHOD,

        /// <summary>
        /// Name（string 型）と PossibleGenericName（GENERICNAMENODE 型）をフィールドに持つ。
        /// </summary>
        NAME,

        /// <summary>
        /// NAMENODE（の派生クラス）を持つ TYPEBASENODE の派生クラス。
        /// </summary>
        NAMEDTYPE,

        /// <summary>
        /// 名前空間を表すノード。
        /// </summary>
        NAMESPACE,

        /// <summary>
        /// NESTEDTYPENODE。
        /// </summary>
        NESTEDTYPE,

        /// <summary>
        /// new による creation-expression を表すためのクラス。
        /// </summary>
        NEW,

        /// <summary>
        /// null 許容型を表す TYPEBASENODE の派生クラス。
        /// </summary>
        NULLABLETYPE,

        /// <summary>
        /// BASENODE を使う。
        /// </summary>
        OP,

        /// <summary>
        /// <para>型付けされていない generic 名を表すための NAMENODE 派生クラス。</para>
        /// <para>"&lt;" と "&gt;" の位置とパラメータ数を格納する。</para>
        /// </summary>
        OPENNAME,

        /// <summary>
        /// NAMEDTYPENODE を使う。
        /// </summary>
        OPENTYPE,

        /// <summary>
        /// OPERATORMETHODNODE を使う。
        /// </summary>
        OPERATOR,

        /// <summary>
        /// 1 つのパラメータを表すためのクラス。属性、型、名前を保持する。
        /// </summary>
        PARAMETER,

        /// <summary>
        /// PARTIALMEMBERNODE。
        /// </summary>
        PARTIALMEMBER,

        /// <summary>
        /// TYPEBASENODE の派生クラス。対象の型を表す TYPEBASENODE 型フィールドを持つ。
        /// </summary>
        POINTERTYPE,

        /// <summary>
        /// 既定の型を表すための TYPEBASENODE の派生クラス。
        /// PREDEFTYPE 型のフィールドを持つ。
        /// </summary>
        PREDEFINEDTYPE,

        /// <summary>
        /// MEMBERNODE の派生クラス。プロパティを表す。
        /// </summary>
        PROPERTY,

        /// <summary>
        /// PROPERTYNODE を使う。
        /// </summary>
        INDEXER,

        /// <summary>
        /// EXPRSTMTNODE を使う。
        /// </summary>
        RETURN,

        /// <summary>
        /// EXPRSTMTNODE を使う。
        /// </summary>
        THROW,

        /// <summary>
        /// <para>try-catch 文または try-finally 文を表すための STATEMENTNODE の派生クラス。</para>
        /// <para>try-catch-finally は入れ子 try{tyr-catch}finally{} と考えて処理する。</para>
        /// <para>catch か finally かは Flags を使って示す。
        /// try-catch とする場合は NODEFLAGS.TRY_CATCH ビットを、
        /// try-finally とする場合は NODEFLAGS.TRY_FINALLY ビットをセットする。</para>
        /// </summary>
        TRY,

        /// <summary>
        /// 属性付きの型を表すノード。
        /// </summary>
        TYPEWITHATTR,

        /// <summary>
        /// AGGREGATENODE の派生ノード。構造体を表す。
        /// </summary>
        STRUCT,

        /// <summary>
        /// switch 文を表すための STATEMENTNODE の派生クラス。
        /// </summary>
        SWITCH,

        /// <summary>
        /// 単項演算子を表すノード。それの対象となる項をフィールドに持つ。
        /// </summary>
        UNOP,

        /// <summary>
        /// LABELSTMTNODE を使う。
        /// </summary>
        UNSAFE,

        /// <summary>
        /// using-directive を表すノード。
        /// </summary>
        USING,

        /// <summary>
        /// 変数名とその初期値を表すためのクラス。
        /// 変数名のノード、初期値のノード、宣言全体のノードへの参照を持つ。
        /// </summary>
        VARDECL,

        /// <summary>
        /// LOOPSTMTNODE を使う。
        /// </summary>
        WHILE,

        /// <summary>
        /// EXPRSTMTNODE を使う。
        /// </summary>
        YIELD,

        // CS3

        /// <summary>
        /// (CS3) Represents a Implicit type.
        /// </summary>
        IMPLICITTYPE,

        /// <summary>
        /// (CS3) Represetns a lambda expression.
        /// </summary>
        LAMBDAEXPR,

        /// <summary>
        /// (CS3) Represents a collection-initializer.
        /// </summary>
        COLLECTIONINIT,

        /// <summary>
        /// (CS3) Represents a query expression.
        /// </summary>
        QUERYEXPR,

        /// <summary>
        /// (CS3) Represets a first from clause of a query expression.
        /// </summary>
        FROMCLAUSE,

        /// <summary>
        /// (CS3) Represets a first from clause of a query expression.
        /// </summary>
        FROMCLAUSE2,

        /// <summary>
        /// (CS3) Represets a let clause of a query expression.
        /// </summary>
        LETCLAUSE,

        /// <summary>
        /// (CS3) Represets a where clause of a query expression.
        /// </summary>
        WHERECLAUSE,

        /// <summary>
        /// (CS3) Represets a join clause of a query expression.
        /// </summary>
        JOINCLAUSE,

        /// <summary>
        /// (CS3) Represets a orderby clause of a query expression.
        /// </summary>
        ORDERBYCLAUSE,

        /// <summary>
        /// (CS3) Represets a select clause of a query expression.
        /// </summary>
        SELECTCLAUSE,

        /// <summary>
        /// (CS3) Represets a group clause of a query expression.
        /// </summary>
        GROUPCLAUSE,

        /// <summary>
        /// (CS3) Represets a query continuation of a query expression.
        /// </summary>
        QUERYCONTINUATION,

        COUNT
    }

    //======================================================================
    // enum PREDEFNAME
    //
    // Defined in PredefName.cs.
    //======================================================================

    //======================================================================
    // enum PPTOKENID
    //
    /// <summary>
    /// ID of preprocessor tokens.
    /// (PPT_)
    /// (CSharp\Inc\Enum.cs)
    /// </summary>
    //======================================================================
    internal enum PPTOKENID : uint
    {
        UNDEFINED,  // hirano567@hotmail.co.jp

        DEFINE,
        UNDEF,
        ERROR,
        WARNING,
        IF,
        ELIF,
        ELSE,
        ENDIF,
        REGION,
        ENDREGION,
        LINE,
        PRAGMA,
        TRUE,
        FALSE,
        HIDDEN,
        DEFAULT,
        DISABLE,
        RESTORE,
        CHECKSUM,
        IDENTIFIER,
        NUMBER,
        STRING,
        OPENPAREN,
        CLOSEPAREN,
        COMMA,
        OR,
        AND,
        EQUAL,
        NOTEQUAL,
        NOT,
        EOL,
        UNKNOWN,
    }

    //#define MAX_PPTOKEN_LEN 128   // moved to CSourceModuleBase

    //======================================================================
    // enum ATTRTARGET
    //
    /// <summary>
    /// ATTRLOC in ssli. Flags for attributes targets.
    /// (AL_)
    /// (CSharp\Inc\Enum.cs)
    /// </summary>
    //======================================================================
    [Flags]
    internal enum ATTRTARGET : uint
    {
        NONE = 0x0000,  // hirano567@hotmail.co.jp

        ASSEMBLY = 0x0001,
        MODULE = 0x0002,
        TYPE = 0x0004,
        METHOD = 0x0008,
        FIELD = 0x0010,
        PROPERTY = 0x0020,
        EVENT = 0x0040,
        PARAMETER = 0x0080,
        RETURN = 0x0100,
        TYPEVAR = 0x0200,
        UNKNOWN = 0x0400,

        COUNT = 12
    }
}
