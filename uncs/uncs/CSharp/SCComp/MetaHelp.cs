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
// File: metahelp.h
//
// Helper routines for importing/emitting CLR metadata.
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
// File: metahelp.cpp
//
// Helper routines for importing/emitting CLR metadata.
// ===========================================================================

//============================================================================
//  MetaAttr.cs
//
//  2015/02/01
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // class MetaDataHelper
    //======================================================================
    internal class MetaDataHelper
    {
        //------------------------------------------------------------
        // MetaDataHelper Fields and Properties
        //------------------------------------------------------------
        protected string nested = null; // WCHAR m_chNested;

        //------------------------------------------------------------
        // MetaDataHelper Constructor (1)
        //
        /// <summary>
        /// <para>(In sscli, default value is "+".)</para>
        /// </summary>
        /// <param name="str"></param>
        /// <remarks>
        /// #define NESTED_CLASS_SEP (L'+') // Separator for nested classes. // import.h (26)
        /// </remarks>
        //------------------------------------------------------------
        internal MetaDataHelper(string str) // = NESTED_CLASS_SEP)
        {
            this.nested = str;
        }

        //------------------------------------------------------------
        // MetaDataHelper Constructor (2)
        //
        /// <summary>
        /// Set the class separator "+".
        /// </summary>
        //------------------------------------------------------------
        internal MetaDataHelper()
        {
            this.nested = "+";
        }

        //------------------------------------------------------------
        // MetaDataHelper.EscapeSpecialChars
        //
        /// <summary>
        /// Do nothig.
        /// </summary>
        /// <param name="str"></param>
        //------------------------------------------------------------
        protected virtual void EscapeSpecialChars(StringBuilder str) { }

        //------------------------------------------------------------
        // MetaDataHelper.GetTypeVars
        //
        /// <summary>
        /// Add the count of type variables to name, as "name`2".
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="strBuilder"></param>
        /// <param name="innerSym"></param>
        //------------------------------------------------------------
        protected virtual void GetTypeVars(SYM sym, StringBuilder strBuilder, SYM innerSym)
        {
            if (sym.IsAGGSYM && (sym as AGGSYM).IsArityInName)
            {
                TypeArray typeVars = (sym as AGGSYM).TypeVariables;
                DebugUtil.Assert(typeVars.Count > 0); // isArityInName shouldn't be set otherwise.
                strBuilder.Append('`');
                strBuilder.AppendFormat("{0}", typeVars.Count);
            }
        }

        //------------------------------------------------------------
        // MetaDataHelper.AddTypeModifiers
        //
        /// <summary>
        /// appends array and ptr modifiers
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="strBuilder"></param>
        //------------------------------------------------------------
        protected void AddTypeModifiers(TYPESYM typeSym, StringBuilder strBuilder)
        {
            switch (typeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                case SYMKIND.TYVARSYM:
                case SYMKIND.ERRORSYM:
                case SYMKIND.NUBSYM:
                    break;

                case SYMKIND.ARRAYSYM:
                    // NOTE: In C# a 2-dim array of 1-dim array of int is int[,][].
                    // This produces int[][,] as metadata requires.
                    AddTypeModifiers((typeSym as ARRAYSYM).ElementTypeSym, strBuilder);
                    strBuilder.Append('[');
                    for (int i = 1; i < (typeSym as ARRAYSYM).Rank; ++i)
                    {
                        strBuilder.Append(',');
                    }
                    strBuilder.Append(']');
                    break;

                case SYMKIND.PTRSYM:
                    AddTypeModifiers((typeSym as PTRSYM).BaseTypeSym, strBuilder);
                    strBuilder.Append('*');
                    break;

                default:
                    DebugUtil.Assert(false, "Unknown symbol typeSym");
                    break;
            }
        }

        // Instance methods.

        //------------------------------------------------------------
        // MetaDataHelper.GetMetaDataName (1) instance method
        //
        /// <summary>
        /// Returns a class/type name as it should appear in metadata
        /// Basically the same as GetFullName except inner classes
        /// are NOT qualified by their outer class or namespace
        /// </summary>
        /// <param name="bagSym"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal void GetMetaDataName(BAGSYM bagSym, StringBuilder builder)
        {
            DebugUtil.Assert(bagSym != null);

            // Special case -- the root namespace.
            if (bagSym.ParentSym == null)
            {
                // At the root namespace.
                return;
            }

            BAGSYM parentBagSym = bagSym.ParentSym as BAGSYM;

            // If Our parent isn't the root or an outer type,
            // get the parent name and seperator and advance beyond it.
            if (parentBagSym is NSSYM && parentBagSym.ParentSym != null)
            {
                GetMetaDataName(parentBagSym, builder);
                builder.Append('.');
            }

            // Get the current name and add it on
            builder.Append(bagSym.Name);

            EscapeSpecialChars(builder);
            GetTypeVars(bagSym, builder, null);
        }

        //------------------------------------------------------------
        // MetaDataHelper.GetFullName
        //
        /// <summary>
        /// Returns a fully-qualified name (open type, do not pass TYPESYMs)
        /// With inner types denoted with m_chNested (defaults to '+')
        /// And everything else separated by dots (.)
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="strBuilder"></param>
        /// <param name="innerSym"></param>
        //------------------------------------------------------------
        public void GetFullName(
            SYM sym,
            StringBuilder strBuilder,
            SYM innerSym)	// = null
        {
            // Special case -- the root namespace.
            if (sym.ParentSym == null)
            {
                // At the root namespace.
                return;
            }

            if (sym.IsTYPESYM)
            {
                DebugUtil.Assert(false, "MetaDataHelper::GetFullName should not be called on TYPESYMs");
                return;
            }

            PARENTSYM parentSym = sym.ParentSym;

            // If Our parent isn't the root, get the parent name and separator and advance beyond it.
            if (parentSym.ParentSym!=null)
            {
                GetFullName(parentSym, strBuilder, sym);
                strBuilder.Append(parentSym.IsAGGSYM ? this.nested : ".");
            }

            // Get the current name and add it on
            string name;
            if (sym.IsPROPSYM)
            {
                name = (sym as PROPSYM).GetRealName();
            }
            else
            {
                name = sym.Name;
            }

            int ichMin = strBuilder.Length;
            if (name == null)
            {
                GetExplicitImplName(sym, strBuilder);
            }
            else
            {
                strBuilder.Append(name);
            }

            EscapeSpecialChars(strBuilder);
            GetTypeVars(sym, strBuilder, innerSym);
        }

        //------------------------------------------------------------
        // MetaDataHelper.GetExplicitImplName
        //
        /// <summary>
        /// Get a synthesized name for explicit interface implementations.
        /// The name we use is: "InterfaceName.MethodName", where InterfaceName is the fully qualified name
        /// of the interface containing the implemented method.
        /// This name has a '.' in it, so it can't conflict with any "real" name or be confused with one.
        /// </summary>
        /// <remarks>
        /// Returns true if the buffer had enough space for the name.
        /// If not enough space, then adds as much of name as possible to buffer.
        /// Always NULL terminates buffer.
        /// </remarks>
        /// <param name="sym"></param>
        /// <param name="strBuilder"></param>
        //------------------------------------------------------------
        public void GetExplicitImplName(SYM sym, StringBuilder strBuilder)
        {
            DebugUtil.Assert(sym.IsEVENTSYM || sym.IsMETHPROPSYM);

            SymWithType swtImpl = new SymWithType();
            ERRORSYM errSym = null;
            string implName = null;
            string aliasName = null;

            switch (sym.Kind)
            {
                case SYMKIND.EVENTSYM:
                    EVENTSYM eventSym = sym as EVENTSYM;
                    DebugUtil.Assert(eventSym.IsExpImpl);
                    swtImpl = eventSym.SlotEventWithType;
                    errSym = eventSym.ExpImplErrorSym;
                    break;

                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    {
                        METHPROPSYM mpSym = sym as METHPROPSYM;
                        DebugUtil.Assert(mpSym.IsExplicitImplementation);
                        swtImpl = mpSym.SlotSymWithType;
                        errSym = mpSym.ExpImplErrorSym;
                        BASENODE nameNode = null;

                        if (sym.IsMETHSYM &&
                            (sym as METHSYM).ParseTreeNode != null &&
                            (sym as METHSYM).ParseTreeNode.Kind == NODEKIND.METHOD)
                        {
                            nameNode = ((sym as METHSYM).ParseTreeNode as METHODNODE).NameNode;
                        }
                        else if (
                            sym.IsPROPSYM &&
                            (sym as PROPSYM).ParseTreeNode != null &&
                            (sym as PROPSYM).ParseTreeNode.Kind == NODEKIND.PROPERTY)
                        {
                            nameNode = ((sym as PROPSYM).ParseTreeNode as PROPERTYNODE).NameNode;
                        }

                        while (nameNode != null && nameNode.Kind == NODEKIND.DOT)
                        {
                            nameNode = nameNode.AsDOT.Operand1;
                        }

                        if (nameNode != null && nameNode.Kind == NODEKIND.ALIASNAME)
                        {
                            aliasName = nameNode.AsANYNAME.Name;
                        }
                        if (!sym.IsPROPSYM || !(sym as PROPSYM).IsIndexer)
                        {
                            break;
                        }

                        implName = (swtImpl != null ? (swtImpl.Sym as PROPSYM).GetRealName() : "Item");
                        // fish out any user specified alias
                        break;
                    }

                default:
                    // gcc -Wall (all warnings enabled) complains if all cases
                    // aren't handled, so we explicitly handle default and assert
                    DebugUtil.Assert(false);
                    break;
            }

            DebugUtil.Assert(swtImpl != null || errSym != null);

            if (aliasName != null)
            {
                strBuilder.Append(aliasName);
                strBuilder.Append("::");
            }

            if (swtImpl != null)
            {
                GetExplicitImplTypeName(swtImpl.AggTypeSym, strBuilder);
                if (implName == null)
                {
                    implName = swtImpl.Sym.Name;
                }
            }
            else
            {
                GetExplicitImplTypeName(errSym, strBuilder);
            }

            if (implName != null)
            {
                // Add dot seperator.
                strBuilder.Append('.');
                strBuilder.Append(implName);
            }
        }

        //------------------------------------------------------------
        // MetaDataHelper.GetExplicitImplTypeName
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        /// <param name="strBuilder"></param>
        //------------------------------------------------------------
        public void GetExplicitImplTypeName(TYPESYM typeSym, StringBuilder strBuilder)
        {
#if DEBUG
            if (!(typeSym != null))
            {
                ;
            }
#endif
            DebugUtil.Assert(typeSym != null);

            TYPESYM nakedTypeSym = typeSym.GetNakedType(false);
            TypeArray typeArgs = null;

            switch (nakedTypeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Unhandled type in GetExplicitImplTypeName");
                    return;

                case SYMKIND.TYVARSYM:
                    strBuilder.Append(nakedTypeSym.Name);
                    break;

                case SYMKIND.NUBSYM:
                    nakedTypeSym = (nakedTypeSym as NUBSYM).GetAggTypeSym();
                    if (nakedTypeSym == null)
                    {
                        DebugUtil.Assert(false, "Why did GetAts return null?");
                        return;
                    }
                    // Fall through.
                    goto case SYMKIND.AGGTYPESYM;

                case SYMKIND.AGGTYPESYM:
                    {
                        AGGTYPESYM outerAggTypeSym = (nakedTypeSym as AGGTYPESYM).OuterTypeSym;
                        AGGSYM aggSym = nakedTypeSym.GetAggregate();

                        if (outerAggTypeSym != null)
                        {
                            GetExplicitImplTypeName(outerAggTypeSym, strBuilder);
                            strBuilder.Append('.');
                        }
                        else
                        {
                            DebugUtil.Assert(aggSym.ParentBagSym != null && !aggSym.ParentBagSym.IsAGGSYM);
                            int cch = strBuilder.Length;
                            GetFullName(aggSym.ParentBagSym, strBuilder, aggSym);
                            if (cch < strBuilder.Length) strBuilder.Append('.');
                        }
                        strBuilder.Append(aggSym.Name);

                        typeArgs = (nakedTypeSym as AGGTYPESYM).TypeArguments;
                    }
                    break;

                case SYMKIND.ERRORSYM:
                    {
                        ERRORSYM errSym = nakedTypeSym as ERRORSYM;
                        SYM parentSym = errSym.ParentSym;

                        if (parentSym != null && parentSym.IsTYPESYM)
                        {
                            GetExplicitImplTypeName(parentSym as TYPESYM, strBuilder);
                            strBuilder.Append('.');
                        }
                        else if (parentSym != null && parentSym.IsNSAIDSYM)
                        {
                            parentSym = (parentSym as NSAIDSYM).NamespaceSym;
                            int cch = strBuilder.Length;
                            GetFullName(parentSym, strBuilder, errSym);
                            if (cch < strBuilder.Length)
                                strBuilder.Append('.');
                        }
                        strBuilder.Append(errSym.ErrorName);

                        typeArgs = errSym.TypeArguments;
                    }
                    break;
            }

            if (typeArgs != null && typeArgs.Count > 0)
            {
                strBuilder.Append('<');
                for (int i = 0; i < typeArgs.Count; ++i)
                {
                    if (i > 0) strBuilder.Append(',');
                    GetExplicitImplTypeName(typeArgs[i], strBuilder);
                }
                strBuilder.Append('>');
            }

            // Add ptr and array modifiers
            AddTypeModifiers(typeSym, strBuilder);
        }

        // Static methods.

        //------------------------------------------------------------
        // MetaDataHelper.GetTypeAccessFlags
        //
        /// <summary>
        /// <para>Determine the visibility flags for a typedef definition in metadata
        /// Accepts TYPEDEFSYMs or AGGSYMs</para>
        /// <para>Use System.Reflection.TypeAttribuetes in place of CorTypeAttr of sscli.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static TypeAttributes GetTypeAccessFlags(SYM sym)
        {
            TypeAttributes flags = 0;

            DebugUtil.Assert(sym is AGGSYM);

            // Set access flags.
            if (sym.ParentSym is NSSYM)
            {
                // "Top-level" aggregate. Can only be public or internal.
                DebugUtil.Assert(sym.Access == ACCESS.PUBLIC || sym.Access == ACCESS.INTERNAL);
                if (sym.Access == ACCESS.PUBLIC)
                {
                    flags |= TypeAttributes.Public;
                }
                else
                {
                    flags |= TypeAttributes.NotPublic;
                }
            }
            else
            {
                // nested aggregate. Can be any access.
                switch (sym.Access)
                {
                    case ACCESS.PUBLIC:
                        flags |= TypeAttributes.NestedPublic;
                        break;

                    case ACCESS.INTERNAL:
                        flags |= TypeAttributes.NestedAssembly;
                        break;

                    case ACCESS.PROTECTED:
                        flags |= TypeAttributes.NestedFamily;
                        break;

                    case ACCESS.INTERNALPROTECTED:
                        flags |= TypeAttributes.NestedFamORAssem;
                        break;

                    case ACCESS.PRIVATE:
                        flags |= TypeAttributes.NestedPrivate;
                        break;

                    default:
                        DebugUtil.Assert(false, "Bad access flag");
                        break;
                }
            }
            return flags;
        }

        //------------------------------------------------------------
        // MetaDataHelper.GetAggregateFlags
        //
        /// <summary>
        /// <para>Determine the flags for a typedef definition in metadata</para>
        /// <para>Use System.Reflection.TypeAttribuetes in place of CorTypeAttr of sscli.</para>
        /// <para>CorTypeAttr has a member named "Forwarder" which TypeAttributes does not has.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static TypeAttributes GetAggregateFlags(AGGSYM aggSym)
        {
            TypeAttributes flags = 0;
            // Determine flags.

            // Set access flags.
            flags |= GetTypeAccessFlags(aggSym);

            // Set other flags
            switch (aggSym.AggKind)
            {
                case AggKindEnum.Class:
                    if (aggSym.IsSealed)
                    {
                        flags |= TypeAttributes.Sealed;
                    }
                    if (aggSym.IsAbstract)
                    {
                        flags |= TypeAttributes.Abstract;
                    }
                    if (!aggSym.HasUserDefinedStaticCtor)
                    {
                        flags |= TypeAttributes.BeforeFieldInit;
                    }
                    break;

                case AggKindEnum.Interface:
                    flags |= TypeAttributes.Interface | TypeAttributes.Abstract;
                    break;

                case AggKindEnum.Enum:
                    DebugUtil.Assert(aggSym.IsSealed);
                    flags |= TypeAttributes.Sealed;
                    break;

                case AggKindEnum.Struct:
                    DebugUtil.Assert(aggSym.IsSealed);
                    flags |= TypeAttributes.Sealed;
                    if (!aggSym.HasUserDefinedStaticCtor)
                    {
                        flags |= TypeAttributes.BeforeFieldInit;
                    }
                    if (aggSym.IsFabricated)
                    {
                        flags |= TypeAttributes.SequentialLayout; // Fabricated structs are always sequential
                    }
                    break;

                case AggKindEnum.Delegate:
                    DebugUtil.Assert(aggSym.IsSealed);
                    flags |= TypeAttributes.Sealed;
                    break;

                default:
                    DebugUtil.Assert(false);
                    break;
            }

            switch (aggSym.GetOutputFile().DefaultCharSet)
            {
                default:
                    DebugUtil.VsFail("A new value was added to System.Runtime.InteropServices.CharSet that we need to handle");
                    goto case 0;

                case (System.Runtime.InteropServices.CharSet) 0:    // Unset
                case System.Runtime.InteropServices.CharSet.None:   // 1:
                    break;

                case System.Runtime.InteropServices.CharSet.Ansi:   // 2:
                    flags |= TypeAttributes.AnsiClass;
                    break;

                case System.Runtime.InteropServices.CharSet.Unicode:    // 3:
                    flags |= TypeAttributes.UnicodeClass;
                    break;

                case System.Runtime.InteropServices.CharSet.Auto:   // 4:
                    flags |= TypeAttributes.AutoClass;
                    break;
            }

            return flags;
        }

        //------------------------------------------------------------
        // MetaDataHelper.GetMetaDataName (2) static method
        //
        /// <summary>
        /// <para>Return the name of class or type which appears in the metadata.</para>
        /// <para>Call the instance method GetMetaDataName.</para>
        /// </summary>
        /// <param name="bagSym"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool GetMetaDataName(BAGSYM bagSym, out string name)
        {
            MetaDataHelper help = new MetaDataHelper();
            StringBuilder sb = new StringBuilder();
            help.GetMetaDataName(bagSym, sb);
            name = sb.ToString();
            return true;
        }

        //------------------------------------------------------------
        // MetaDataHelper.GetFullName (2) static method
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public static bool GetFullName(SYM sym, out string name)
        {
            StringBuilder sb = new StringBuilder();
            MetaDataHelper help = new MetaDataHelper();
            help.GetFullName(sym, sb, null);
            name = sb.ToString();
            return true;
        }

        //------------------------------------------------------------
        // MetaDataHelper.GetExplicitImplName (2) static method
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool GetExplicitImplName(SYM sym, out string name)
        {
            StringBuilder sb = new StringBuilder();
            MetaDataHelper help = new MetaDataHelper();
            help.GetExplicitImplName(sym, sb);
            name = sb.ToString();
            return true;
        }
    }

    //======================================================================
    // class MetaDataHelperXml
    //======================================================================
    //class MetaDataHelperXml : public MetaDataHelper
    //{
    //protected:
    //    COMPILER * m_compiler;

    //    virtual void EscapeSpecialChars(StringBldr & str, int ichMin);
    //void MetaDataHelperXml::EscapeSpecialChars(StringBldr & str, int ichMin)
    //{
    //    // Replace:
    //    // . with #
    //    // , with @
    //    // < with {
    //    // > with }
    //    for (PWCH pch = str.Str() + ichMin; *pch; pch++) {
    //        switch (*pch) {
    //        case L'.':
    //            *pch = L'#';
    //            break;
    //        case L',':
    //            *pch = L'@';
    //            break;
    //        case L'<':
    //            *pch = L'{';
    //            break;
    //        case L'>':
    //            *pch = L'}';
    //            break;
    //        }
    //    }
    //}

    //    virtual void GetTypeVars(SYM * sym, StringBldr & str, SYM * symInner);
    //
    //public:
    //    MetaDataHelperXml(COMPILER * compiler, WCHAR chNested = L'.') : MetaDataHelper(chNested)
    //    {
    //        m_compiler = compiler;
    //    }
    //
    //    void GetTypeName(TYPESYM * type, StringBldr & str);
    //};

    //======================================================================
    // class TypeNameSerializer
    //======================================================================
    internal class TypeNameSerializer
    {
        //protected    COMPILER *                  m_compiler;
        //protected    CComPtr<ITypeNameBuilder>   m_qbldr;

        //protected    bool CheckHR(TYPESYM * type, HRESULT hr);
        //protected    bool GetAssemblyQualifiedTypeNameCore(TYPESYM * type, bool fOpenType);
        //protected    bool GetAggName(AGGSYM * agg);
        //protected    bool AddTypeModifiers(TYPESYM * type);

        //public    TypeNameSerializer(COMPILER * compiler);
        //public    ~TypeNameSerializer();
        //public    BSTR GetAssemblyQualifiedTypeName(TYPESYM * type, bool fOpenType);
    }

    //class CAggSymNameEncImportIter
    //{
    //private:
    //    // Immutable state
    //    COMPILER *  m_compiler;
    //    AGGSYM *    m_agg;
    //    NAME *      m_name;
    //
    //    // Enumerator state
    //    SYM *       m_curSym;
    //    bool        m_fSimpleName;
    //
    //private:
    //    bool IsExplImpl(SYM *sym);
    //    bool IsEplImplWithName(SYM *sym);
    //    SYM *NextExplImplWithName(SYM *pSym);
    //
    //public:
    //    CAggSymNameEncImportIter(COMPILER *compiler, AGGSYM *agg, NAME *name);
    //
    //    bool MoveNext();
    //    SYM *Current();
    //};
}

//void MetaDataHelperXml::GetTypeVars(SYM * sym, StringBldr & str, SYM * symInner)
//{
//    if (sym->isAGGSYM()) {
//        TypeArray * typeVars = sym->asAGGSYM()->typeVarsThis;
//        if (typeVars->size) {
//            str.Add(L'`');
//            str.AddNum(typeVars->size);
//        }
//    }
//    else if (sym->isMETHSYM()) {
//        TypeArray * typeVars = sym->asMETHSYM()->typeVars;
//        if (typeVars->size) {
//            str.Add(L"``");
//            str.AddNum(typeVars->size);
//        }
//    }
//}
//
//void MetaDataHelperXml::GetTypeName(TYPESYM * type, StringBldr & str)
//{
//    m_compiler->EnsureState(type, AggState::DefinedMembers);
//    ASSERT(type->AggState() >= AggState::DefinedMembers);
//
//    switch (type->getKind()) {
//    default:
//        ASSERT(0);
//        return;
//	case SK_NUBSYM:
//        GetTypeName(type->asNUBSYM()->GetAts(), str);
//        return;
//
//    case SK_ARRAYSYM:
//        GetTypeName(type->asARRAYSYM()->elementType(), str);
//        if (type->asARRAYSYM()->rank == 1) {
//            // Single dimensional array.
//            str.Add(L"[]");
//        } else {
//            // Known rank > 1
//            str.Add(L'[');
//            for (int i = 1; i < type->asARRAYSYM()->rank; ++i)    // Do 1 less
//                str.Add(L"0:,");
//            str.Add(L"0:]");
//        }
//        return;
//
//    case SK_PTRSYM:
//        GetTypeName(type->asPTRSYM()->baseType(), str);
//        str.Add(L'*');
//        return;
//
//    case SK_TYVARSYM:
//        str.Add(L'`');
//        if (type->asTYVARSYM()->isMethTyVar)
//            str.Add(L'`');
//        str.AddNum(type->asTYVARSYM()->indexTotal);
//        return;
//
//    case SK_PINNEDSYM:
//        GetTypeName(type->asPINNEDSYM()->baseType(), str);
//        str.Add(L'^');
//        return;
//
//    case SK_PARAMMODSYM:
//        GetTypeName(type->asPARAMMODSYM()->paramType(), str);
//        str.Add(L'@');
//        return;
//
//    case SK_MODOPTTYPESYM:
//        GetTypeName(type->asMODOPTTYPESYM()->baseType(), str);
//        return;
//
//    case SK_VOIDSYM:
//        type = m_compiler->GetReqPredefType(PT_SYSTEMVOID, false);
//        // fall-through to SK_AGGTYPESYM
//    case SK_AGGTYPESYM:
//        {
//            AGGSYM * agg = type->asAGGTYPESYM()->getAggregate();
//
//            if (!agg->Parent()) { 
//                // __arglist syms do not have a parent, so just return. There's no spec on how to handle these parameters
//                // in xml doc comments, but in v7.0 and v7.1 we have just left the parameter nbbame blank.
//                ASSERT(type == m_compiler->getBSymmgr().GetArglistSym());
//                return;
//            }
//
//            AGGTYPESYM * typeOuter = type->asAGGTYPESYM()->outerType;
//
//            if (typeOuter) {
//                GetTypeName(typeOuter, str);
//                str.Add(L'.');
//            }
//            else {
//                ASSERT(agg->Parent()->isNSSYM());
//                int cch = str.Length();
//                GetFullName(agg->Parent(), str, agg);
//                if (cch < str.Length())
//                    str.Add(L'.');
//            }
//            str.Add(agg->name->text);
//
//            TypeArray * typeArgs = type->asAGGTYPESYM()->typeArgsThis;
//
//            if (typeArgs->size > 0) {
//                str.Add(L'{');
//                for (int i = 0; i < typeArgs->size; i++) {
//                    if (i > 0)
//                        str.Add(L',');
//                    GetTypeName(typeArgs->Item(i), str);
//                }
//                str.Add(L'}');
//            }
//        }
//        return;
//    }
//}
//
//TypeNameSerializer::TypeNameSerializer(COMPILER * compiler)
//{
//    m_compiler = compiler;
//    m_qbldr = compiler->GetTypeNameBuilder();
//}
//
//TypeNameSerializer::~TypeNameSerializer()
//{
//    m_compiler->ReleaseTypeNameBuilder();
//}
//
//// returns NULL for failure (after reporting the error)
//BSTR TypeNameSerializer::GetAssemblyQualifiedTypeName(TYPESYM * type, bool fOpenType)
//{
//    BSTR bstr = NULL;
//    if (type->pszAssemblyQualifiedName != NULL)
//        return SysAllocString(type->pszAssemblyQualifiedName);
//
//
//    if (!GetAssemblyQualifiedTypeNameCore(type, fOpenType)) {
//        m_qbldr->Clear();
//        return NULL;
//    }
//
//    HRESULT hr = m_qbldr->ToString(&bstr);
//    
//    // Always call clear even if something else fails
//    HRESULT hr2 = m_qbldr->Clear();
//
//    if (CheckHR(type, hr) && CheckHR(type, hr2))
//    {
//        // cache this for later
//        type->pszAssemblyQualifiedName = m_compiler->getGlobalSymAlloc().AllocStr(bstr);
//        return bstr;
//    }
//
//    if (bstr)
//        SysFreeString(bstr);
//    return NULL;
//}
//
//bool TypeNameSerializer::CheckHR(TYPESYM * type, HRESULT hr)
//{
//    if (SUCCEEDED(hr))
//        return true;
//    m_compiler->Error( NULL, FTL_TypeNameBuilderError, type, m_compiler->ErrHR(hr, true));
//    return false;
//}
//
//bool TypeNameSerializer::GetAssemblyQualifiedTypeNameCore(TYPESYM * type, bool fOpenType)
//{
//    TYPESYM * temp = type->GetNakedType();
//    if (temp->isNUBSYM()) {
//        // Convert the NUBSYM to a Nullable AGGTYPESYM
//        temp = temp->asNUBSYM()->GetAts();
//    }
//    else if (temp->isVOIDSYM()) {
//        temp = m_compiler->GetReqPredefType(PT_SYSTEMVOID, false);
//    }
//    if (temp == NULL || !temp->isAGGTYPESYM()) {
//        ASSERT(temp && temp->isERRORSYM());
//        return false;
//    }
//    AGGTYPESYM * ats = temp->asAGGTYPESYM();
//    AGGSYM * agg = ats->getAggregate();
//    HRESULT hr;
//
//    if (!GetAggName(agg))
//        return false;
//
//    if (!fOpenType && ats->typeArgsAll->size > 0) {
//        hr = m_qbldr->OpenGenericArguments();
//        if (!CheckHR(type, hr))
//            return false;
//
//        for (int i = 0; i < ats->typeArgsAll->size; i++) {
//            hr = m_qbldr->OpenGenericArgument();
//            if (!CheckHR(type, hr) || !GetAssemblyQualifiedTypeNameCore( ats->typeArgsAll->Item(i), false))
//                return false;
//
//            hr = m_qbldr->CloseGenericArgument();
//            if (!CheckHR(type, hr))
//                return false;
//        }
//
//        hr = m_qbldr->CloseGenericArguments();
//        if (!CheckHR(type, hr))
//            return false;
//    }
//
//    if (!AddTypeModifiers(type))
//        return false;
//
//    PCWSTR pszAssem = m_compiler->importer.GetAssemblyName(agg);
//    if (!pszAssem)
//        return true;
//
//    hr = m_qbldr->AddAssemblySpec(pszAssem);
//    return CheckHR(type, hr);
//}
//
//bool TypeNameSerializer::GetAggName(AGGSYM * agg)
//{
//    WCHAR szName[MAX_FULLNAME_SIZE];
//
//    if (agg->isNested()) {
//        if (!GetAggName(agg->GetOuterAgg()))
//            return false;
//    }
//    if (!MetaDataHelper::GetMetaDataName(agg, szName, lengthof(szName)))
//        return false;
//
//    HRESULT hr = m_qbldr->AddName(szName);
//    return CheckHR(agg->getThisType(), hr);
//}
//
//bool TypeNameSerializer::AddTypeModifiers(TYPESYM * type)
//{
//    HRESULT hr = S_OK;
//
//    switch (type->getKind()) {
//    case SK_AGGTYPESYM:
//    case SK_TYVARSYM:
//    case SK_ERRORSYM:
//    case SK_NUBSYM:
//    case SK_VOIDSYM:
//        break;
//    case SK_ARRAYSYM:
//        // NOTE: In C# a 2-dim array of 1-dim array of int is int[,][].
//        // This produces int[][,] as metadata requires.
//        if (!AddTypeModifiers(type->asARRAYSYM()->elementType()))
//            return false;
//        if (type->asARRAYSYM()->rank == 1) {
//            hr = m_qbldr->AddSzArray();
//        }
//        else {
//            hr = m_qbldr->AddArray(type->asARRAYSYM()->rank);
//        }
//        break;
//
//    case SK_PTRSYM:
//        if (!AddTypeModifiers(type->asPTRSYM()->baseType()))
//            return false;
//        hr = m_qbldr->AddPointer();
//        break;
//
//    default:
//        VSFAIL("Unknown symbol type");
//        break;
//    }
//
//    return CheckHR(type, hr);
//}
//
//
//CAggSymNameEncImportIter::CAggSymNameEncImportIter(COMPILER *compiler, AGGSYM *agg, NAME *name) :
//    m_compiler(compiler), m_agg(agg), m_name(name), m_curSym(NULL), m_fSimpleName(true)
//{
//}
//
//bool CAggSymNameEncImportIter::MoveNext()
//{
//    if (m_curSym == NULL)
//    {
//        // First, try to look for the given name directly
//        SYM *sym = m_compiler->getBSymmgr().LookupAggMember(m_name, m_agg, MASK_ALL);
//
//        if (sym == NULL)
//        {
//            sym = NextExplImplWithName(m_agg->firstChild);
//            if (sym == NULL)
//            {
//                sym = m_compiler->getBSymmgr().LookupAggMember(m_compiler->namemgr->GetPredefName(PN_INDEXERINTERNAL), m_agg, MASK_ALL);
//            }
//            else
//            {
//                m_fSimpleName = false;
//            }
//        }
//
//        m_curSym = sym;
//    }
//    else
//    {
//        if (m_fSimpleName)
//        {
//            m_curSym = m_curSym->nextSameName;
//        }
//        else
//        {
//            m_curSym = NextExplImplWithName(m_curSym->nextChild);
//        }
//    }
//    return (m_curSym != NULL);
//}
//
//SYM *CAggSymNameEncImportIter::Current()
//{
//    ASSERT(m_curSym != NULL);
//    return m_curSym;
//}
//
//bool CAggSymNameEncImportIter::IsExplImpl(SYM *sym)
//{
//    switch (sym->getKind()) 
//    {
//        case SK_EVENTSYM:
//            return sym->asEVENTSYM()->IsExpImpl();
//
//        case SK_METHSYM:
//        case SK_PROPSYM:
//            return sym->asMETHPROPSYM()->IsExpImpl();
//
//        default:
//            break;
//    }
//    return false;
//}
//bool CAggSymNameEncImportIter::IsEplImplWithName(SYM *sym)
//{
//    if (!IsExplImpl(sym))
//        return false;
//
//    WCHAR nameBuffer[MAX_FULLNAME_SIZE];
//    MetaDataHelper::GetExplicitImplName(sym, nameBuffer, lengthof(nameBuffer));
//    return (wcscmp(m_name->text, nameBuffer) == 0);
//}
//
//SYM *CAggSymNameEncImportIter::NextExplImplWithName(SYM *pSym)
//{
//    for (SYM *curSym = pSym; curSym != NULL; curSym = curSym->nextChild)
//    {
//        if (IsEplImplWithName(curSym))
//            return curSym;
//    }
//    return NULL;
//}
