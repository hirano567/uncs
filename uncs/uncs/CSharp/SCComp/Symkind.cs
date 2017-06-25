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
// File: symkind.h
//
// Define the different symbol kinds. Only non-abstract symbol kinds are here.
// ===========================================================================

//============================================================================
//  SCComp_Symkind.cs
//
//  2013/09/25
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // enum SYMKIND (SK_)
    //
    /// <summary>
    /// (CSharp\SCComp\Symkind.cs)
    /// </summary>
    //======================================================================
    internal enum SYMKIND : int
    {
        //  #define SYMBOLDEF(kind, global, local) SK_ ## kind,
        //  #include "symkinds.h"

        //  SYMBOLDEF マクロで処理

        //------------------------------------------------------------
        // Namespace
        //------------------------------------------------------------

        /// <summary>
        /// <para>a symbol representing a name space. parent is the containing namespace.</para>
        /// <para>BAGSYM の派生クラス。</para>
        /// </summary>
        NSSYM,

        /// <summary>
        /// <para>NSDECLSYM - a symbol representing a declaration
        /// of a namspace in the source.</para>
        /// <para>firstChild/firstChild->nextChild enumerates the 
        /// NSDECLs and AGGDECLs declared within this declaration.</para>
        /// <para>parent is the containing namespace declaration.</para>
        /// <para>Bag() is the namespace corresponding to this declaration.</para>
        /// <para>DeclNext() is the next declaration for the same namespace.</para>
        /// <para>DECLSYM の派生クラス（DECLSYM は PARENTSYM の派生クラス）。
        /// ソース内の名前空間の宣言を表すシンボル。
        /// firstChild と nextChild はこの宣言内の NSDECL と AGGDECL を列挙する。
        /// DeclNext() は同じ名前空間内の次の宣言を出力する。</para>
        /// </summary>
        NSDECLSYM,

        /// <summary>
        /// <para>Parented by an NSSYM. Represents an NSSYM within an aid (assembly/alias id).
        /// The name is a form of the aid.</para>
        /// <para>PARENTSYM の派生クラス。</para>
        /// </summary>
        NSAIDSYM,

        //------------------------------------------------------------
        // Aggregate
        //------------------------------------------------------------

        /// <summary>
        /// <para>a symbol representing an aggregate type. These are classes,
        /// interfaces, and structs. Parent is a namespace or class. Children are methods,
        /// properties, and member variables, and types (including its own AGGTYPESYMs).</para>
        /// <para>BAGSYM の派生クラス。集合型（クラス、インターフェース、構造体が含まれる）を表す。
        /// 親シンボルは名前空間かクラスである。子シンボルはメソッド、属性、メンバ変数、型である。</para>
        /// </summary>
        AGGSYM,

        /// <summary>
        /// <para>represents a declaration of a aggregate type. With partial classes,
        /// an aggregate type might be declared in multiple places.  This symbol represents
        /// on of the declarations.</para>
        /// <para>parent is the containing DECLSYM.</para>
        /// <para>DECLSYM の派生クラス。集合型の宣言を表す。DECLSYM が親となる。</para>
        /// </summary>
        AGGDECLSYM,

        /// <summary>
        /// <para>Represents a generic constructed (or instantiated) type.Parent is the AGGSYM.</para>
        /// <para>TYPESYM の派生クラス。</para>
        /// </summary>
        AGGTYPESYM,

        /// <summary>
        /// <para>SYM representing a forwarded type (one which previously existed in one assembly,
        /// but has been moved to a new assembly).  
        /// We need this sym in order to resolve typerefs which point to the old assembly,
        /// and should get forwarded to the new one.</para>
        /// <para>アセンブリ内に元々あった型で新しいアセンブリに移動させられたものを表す転送シンボル。
        /// 元のアセンブリを指している typerefs を解決して新しいものへ転送するのに必要となる。</para>
        /// </summary>
        FWDAGGSYM,

        //------------------------------------------------------------
        // "Members" of aggs.
        //------------------------------------------------------------

        /// <summary>
        /// <para>Represents a type variable within an aggregate or method.
        /// Parent is the owning AGGSYM or METHSYM.
        /// There are canonical TYVARSYMs for each index used for normalization of emitted metadata.</para>
        /// <para>TYPESYM の派生クラス。</para>
        /// </summary>
        TYVARSYM,

        /// <summary>
        /// <para>MEMBVARSYM - a symbol representing a member variable of a class.
        /// Parent is a struct or class.</para>
        /// <para>VARSYM の派生クラス。</para>
        /// </summary>
        MEMBVARSYM,

        /// <summary>
        /// <para>LOCVARSYM - a symbol representing a local variable or parameter.
        /// Parent is a scope.</para>
        /// <para>VARSYM の派生クラス。</para>
        /// </summary>
        LOCVARSYM,

        /// <summary>
        /// <para>METHSYM - a symbol representing a method.
        /// Parent is a struct, interface or class (aggregate). No children.</para>
        /// <para>METHPROPSYM の派生クラス。</para>
        /// </summary>
        METHSYM,

        /// <summary>
        /// <para>Used for varargs.</para>
        /// <para>this has to be immediately after METHSYM</para>
        /// <para>METHSYM の派生クラス。</para>
        /// </summary>
        FAKEMETHSYM,

        /// <summary>
        /// <para>PROPSYM - a symbol representing a property.
        /// Parent is a struct, interface or class (aggregate). No children.</para>
        /// <para>METHPROPSYM の派生クラス。</para>
        /// </summary>
        PROPSYM,

        /// <summary>
        /// EVENTSYM - a symbol representing an event.
        /// The symbol points to the AddOn and RemoveOn methods that handle adding and removing delegates to the event.
        /// If the event wasn't imported, it also points to the "implementation" of the event
        /// -- a field or property symbol that is always private.
        /// </summary>
        EVENTSYM,

        //------------------------------------------------------------
        // Primitive types.
        //------------------------------------------------------------

        /// <summary>
        /// <para>VOIDSYM - represents the type "void".</para>
        /// <para>TYPESYM の派生クラス。void を表す。固有のメンバは持たない。</para>
        /// </summary>
        VOIDSYM,

        /// <summary>
        /// <para>NULLSYM - represents the null type -- the type of the "null constant".</para>
        /// <para>TYPESYM の派生クラス。null 型を表す。固有のメンバは持たない。</para>
        /// </summary>
        NULLSYM,

        /// <summary>
        /// <para>UNITSYM - a placeholder typesym used only in type argument lists for open types.
        /// There is exactly one of these.</para>
        /// <para>TYPESYM の派生クラス。オープン型の型引数を格納する。固有のメンバは持たない。</para>
        /// </summary>
        UNITSYM,

        /// <summary>
        /// <para>ANONMETHSYM - a placeholder typesym used only as the type of an anonymous method expression.
        /// There is exactly one of these.</para>
        /// <para>TYPESYM の派生クラス。匿名メソッドの型として使用される。固有のメンバは持たない。</para>
        /// </summary>
        ANONMETHSYM,

        /// <summary>
        /// <para>METHGRPSYM - a placeholder typesym used only as the type of an method groupe expression.
        /// There is exactly one of these.</para>
        /// <para>TYPESYM の派生クラス。メソッドグループの型として使用される。固有のメンバは持たない。</para>
        /// </summary>
        METHGRPSYM,

        /// <summary>
        /// <para>ERRORSYM - a symbol representing an error that has been reported.</para>
        /// <para>TYPESYM の派生クラス。</para>
        /// </summary>
        ERRORSYM,

        //------------------------------------------------------------
        // Derived types - Parent is the base type.
        //------------------------------------------------------------

        /// <summary>
        /// <para>ARRAYSYM - a symbol representing an array.</para>
        /// <para>TYPESYM の派生クラス。</para>
        /// </summary>
        ARRAYSYM,

        /// <summary>
        /// <para>PTRSYM - a symbol representing a pointer type</para>
        /// <para>TYPESYM の派生クラス。</para>
        /// </summary>
        PTRSYM,

        /// <summary>
        /// <para>PINNEDSYM - a symbol representing a pinned type
        /// used only to communicate between ilgen &amp; emitter</para>
        /// </summary>
        PINNEDSYM,

        /// <summary>
        /// <para>PARAMMODSYM - a symbol representing parameter modifier
        /// -- either out or ref.</para>
        /// <para>TYPESYM の派生クラス。パラメータ修飾子 out か ref を表す。</para>
        /// </summary>
        PARAMMODSYM,

        /// <summary>
        /// MODOPTSYM - a symbol representing a modopt from an imported signature.
        /// Parented by a MODULESYM.
        /// Contains the import token.
        /// Caches the emit token.
        /// </summary>
        MODOPTSYM,

        /// <summary>
        /// MODOPTTYPESYM - a symbol representing a modopt modifying a type.
        /// Parented by a TYPESYM.
        /// Contains a MODOPTSYM.
        /// </summary>
        MODOPTTYPESYM,

        /// <summary>
        /// Nullable type as a "derived" type - the parent is the base type.
        /// </summary>
        NUBSYM,

        //------------------------------------------------------------
        // Files
        //------------------------------------------------------------

        /// <summary>
        /// <para>INFILESYM - a symbol that represents an input file, either source
        /// code or meta-data, of a file we may read. Its parent is the output
        /// file it contributes to. The symbol name is the file name.
        /// Children include MODULESYMs.</para>
        /// <para>PARENTSYM の派生クラス。
        /// 入力ファイルやソースコード、メタデータ、読み取り用のファイルを表す
        /// シンボル。親は出力ファイルである。ファイル名をシンボル名とする。
        /// 子には MODULESYM が含まれる。</para>
        /// </summary>
        INFILESYM,

        /// <summary>
        /// <para>Represents an imported module. Parented by a metadata INFILESYM.
        /// Name is the module name. Parents MODOPTSYMs.</para>
        /// <para>PARENTSYM の派生クラス。</para>
        /// </summary>
        MODULESYM,

        /// <summary>
        /// <para>RESFILESYM - a symbol that represents a resource input file.
        /// Its parent is the output file it contributes to, or Default
        /// if it will be embeded in the Assembly file.
        /// The symbol name is the resource Identifier.</para>
        /// <para>リソース入力ファイルを表すシンボル。
        /// 親は出力ファイルであるが、例外としてアセンブリファイル内に埋め込まれる場合は Default である。</para>
        /// </summary>
        RESFILESYM,

        /// <summary>
        /// <para>OUTFILESYM -- a symbol that represents an output file we are creating.
        /// Its children all all input files that contribute.
        /// The symbol name is the file name.</para>
        /// <para>PARENTSYM の派生クラス。
        /// 作成する出力ファイルを表すシンボル。
        /// これに必要となる入力ファイルが子となる。
        /// シンボル名はファイル名と同じである。</para>
        /// </summary>
        OUTFILESYM,

        /// <summary>
        /// a symbol representing an XML file that has been included
        /// via the <include> element in a DocComment
        /// </summary>
        XMLFILESYM,

        /// <summary>
        /// <para>Same as above. Only used for #line directives</para>
        /// <para>INFILESYM の派生クラス。
        /// #line ディレクティブのみに使用する。</para>
        /// </summary>
        SYNTHINFILESYM,

        //------------------------------------------------------------
        // Aliases
        //------------------------------------------------------------

        /// <summary>
        /// <para>ALIASSYM - a symbol representing an using alias clause</para>
        /// <para>Its parent is an NSDECLSYM, but it is not linked into the child list
        /// or in the symbol table.</para>
        /// <para>エイリアス節を表すシンボル。
        /// NSDECLSYM が親となるが、子のリストやシンボルテーブルには格納されない。</para>
        /// </summary>
        ALIASSYM,

        /// <summary>
        /// a symbol representing an alias for a referenced file
        /// </summary>
        EXTERNALIASSYM,

        //------------------------------------------------------------
        // Other
        //------------------------------------------------------------

        /// <summary>
        /// <para>a symbol represent a scope that holds other symbols. Typically unnamed.</para>
        /// <para>PARENTSYM の派生クラス。</para>
        /// </summary>
        SCOPESYM,

        /// <summary>
        /// <para>CACHESYM - a symbol which wraps other symbols so that
        /// they can be cached in the local scope by name
        /// LOCVARSYMs are never cached in the introducing scope</para>
        /// <para>ほかのシンボルをラップしてローカルスコープ外から見えないようにする
        /// ためのシンボル。</para>
        /// </summary>
        CACHESYM,

        /// <summary>
        /// <para>LABELSYM - a symbol representing a label.</para>
        /// <para>ラベルを表すシンボル。</para>
        /// </summary>
        LABELSYM,

        MISCSYM,

        /// <summary>
        /// GLOBALATTRSYM - a symbol representing a global attribute on an assembly or module
        /// </summary>
        GLOBALATTRSYM,

        ANONSCOPESYM,

        //  SYMBOLDEF_EXTRA マクロで処理

        /// <summary>
        /// <para>A fabricated AGGSYM to represent an imported type that we couldn't resolve.
        /// Used for error reporting.
        /// In the EE this is used as a place holder until the real AGGSYM is created.</para>
        /// <para>AGGSYM の派生クラス。
        /// インポートされた型のなかで解決できなかったものを表すためのシンボル。
        /// エラーをレポートするのに使用する。</para>
        /// </summary>
        UNRESAGGSYM,

        /// <summary>
        /// <para>an explicit method impl generated by the compiler
        /// usef for CMOD_OPT interop</para>
        /// <para>METHSYM の派生クラス。</para>
        /// </summary>
        IFACEIMPLMETHSYM,

        /// <summary>
        /// <para>INDEXERSYM - a symbol representing an indexed property.
        /// Parent is a struct, interface or class (aggregate). No children.</para>
        /// <para>PROPSYM の派生クラス。</para>
        /// </summary>
        INDEXERSYM,

        // CS3

        /// <summary>
        /// (CS3) implicit type
        /// </summary>
        IMPLICITTYPESYM,

        /// <summary>
        /// (CS3) implicitly typed array
        /// </summary>
        IMPLICITLYTYPEDARRAYSYM,

        /// <summary>
        /// (CS3) for EXPRLAMBDAEXPR
        /// </summary>
        LAMBDAEXPRSYM,

        /// <summary>
        /// (CS4) for dynamic type
        /// </summary>
        DYNAMICSYM,

        //------------------------------------------------------------
        // Special Purpose
        //------------------------------------------------------------
        LIM,

        UNDEFINED = -1,
    }

    //======================================================================
    // enum AggKindEnum
    //
    /// <summary>
    /// Class, Delegate, Interface, Struct, Enum
    /// (CSharp\SCComp\Symkind.cs)
    /// </summary>
    //======================================================================
    internal enum AggKindEnum : uint
    {
        Unknown,

        Class,
        Delegate,
        Interface,
        Struct,
        Enum,

        Lim
    }
}
