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
// File: clsdrec.h
//
// Defines the structure which contains information necessary to declare
// a single class
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
// File: clsdrec.cpp
//
// Routines for declaring a class
// ===========================================================================

//============================================================================
// ClsDrec.cs
//
// 2015/04/20 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //======================================================================
    // enum CONVTYPE	(CONV_)
    //
    /// <summary>
    /// (CSharp\SCComp\ClsDrec.cs)
    /// </summary>
    //======================================================================
    internal enum CONVTYPE : int
    {
        IMPL,
        EXPL,
        NONE,

        /// <summary>
        /// for example, struct to byte,
        /// </summary>
        OTHER,

        /// <summary>
        /// meaning an ICE if we ever encounter it
        /// </summary>
        ERROR,
    };

    //======================================================================
    // class CLSDREC
    //======================================================================
    internal partial class CLSDREC
    {
        //============================================================
        // class CLSDREC.LayoutFrame
        //============================================================
        private class LayoutFrame
        {
            internal SymWithType SymWithType = new SymWithType();   // swt
            internal AGGTYPESYM FieldAggTypeSym = null;             // * atsField
            internal LayoutFrame OwnerFrame = null;                 // * pframeOwner
            internal LayoutFrame ChildFrame = null;                 // * pframeChild
        }

        //------------------------------------------------------------
        // CLSDREC.accessTokens
        //
        /// <summary>
        /// Tokens which correspond to the parser flags for the various item modifiers
        /// This needs to be kept in ssync with the list in nodes.h
        /// </summary>
        //------------------------------------------------------------
        static private TOKENID[] accessTokens =
        {
            TOKENID.ABSTRACT,
            TOKENID.NEW,
            TOKENID.OVERRIDE,
            TOKENID.PRIVATE,
            TOKENID.PROTECTED,
            TOKENID.INTERNAL,
            TOKENID.PUBLIC,
            TOKENID.SEALED,
            TOKENID.STATIC,
            TOKENID.VIRTUAL,
            TOKENID.EXTERN,
            TOKENID.READONLY,
            TOKENID.VOLATILE,
            TOKENID.UNSAFE,
        };

        //------------------------------------------------------------
        // CLSDREC.operatorNames
        //------------------------------------------------------------
        internal static PREDEFNAME[] operatorNames =
        {
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.OPBITWISEOR,
            PREDEFNAME.OPXOR,
            PREDEFNAME.OPBITWISEAND,
            PREDEFNAME.OPEQUALITY,
            PREDEFNAME.OPINEQUALITY,
            PREDEFNAME.OPLESSTHAN,
            PREDEFNAME.OPLESSTHANOREQUAL,
            PREDEFNAME.OPGREATERTHAN,
            PREDEFNAME.OPGREATERTHANOREQUAL,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.OPLEFTSHIFT,
            PREDEFNAME.OPRIGHTSHIFT,
            PREDEFNAME.OPPLUS,
            PREDEFNAME.OPMINUS,
            PREDEFNAME.OPMULTIPLY,
            PREDEFNAME.OPDIVISION,
            PREDEFNAME.OPMODULUS,
            PREDEFNAME.COUNT,
            PREDEFNAME.OPUNARYPLUS,
            PREDEFNAME.OPUNARYMINUS,
            PREDEFNAME.OPCOMPLEMENT,
            PREDEFNAME.OPNEGATION,
            PREDEFNAME.OPINCREMENT,
            PREDEFNAME.OPDECREMENT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,                   
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,                   
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.OPTRUE,
            PREDEFNAME.OPFALSE,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,
            PREDEFNAME.OPIMPLICITMN,
            PREDEFNAME.OPEXPLICITMN,
            PREDEFNAME.OPEQUALS,
            PREDEFNAME.OPCOMPARE,
            PREDEFNAME.COUNT,
            PREDEFNAME.COUNT,   // (CS3) LAMBDA
        };

        //------------------------------------------------------------
        // CLSDREC  Fields and Properties
        //------------------------------------------------------------
        private COMPILER compiler = null;

        private COMPILER Compiler
        {
            get { return this.compiler; }   // compiler()
        }

        private CController Controller
        {
            get // controller()
            {
                return (this.compiler != null ? this.compiler.Controller : null);
            }
        }

        // cached values for GetLayoutKindValue();
        private int explicitLayoutValue = -1;   // int m_explicitLayoutValue;
        private int sequentialLayoutValue = -1; // int m_sequentialLayoutValue;

        //------------------------------------------------------------
        // CLSDREC Constructor
        //
        /// <summary>
        ///class level init
        /// </summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        internal CLSDREC(COMPILER comp)
        {
            DebugUtil.Assert(comp != null);
            this.compiler = comp;

            MEMBER_OPERATION_EmitMemberDef
                = new MEMBER_OPERATION(EmitMemberDef);
            MEMBER_OPERATION_CompileMember
                = new MEMBER_OPERATION(CompileMember);
            MEMBER_OPERATION_CompileMemberSkeleton
                = new MEMBER_OPERATION(CompileMemberSkeleton);
        }

        // CLSDREC Methods

        //------------------------------------------------------------
        // CLSDREC.IsAtLeastAsVisibleAs
        //
        /// <summary>
        /// Check to see if sym1 is "at least as visible" as sym2.
        /// </summary>
        /// <param name="sym1"></param>
        /// <param name="sym2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsAtLeastAsVisibleAs(SYM sym1, SYM sym2)
        {
            SYM parentSym1, parentSym2;

            DebugUtil.Assert(
                sym2 != null &&
                sym2.ParentSym != null &&
                !sym2.IsARRAYSYM &&
                !sym2.IsPTRSYM &&
                !sym2.IsPARAMMODSYM &&
                !sym2.IsAGGTYPESYM);

            // quick check -- everything is at least as visible as private 
            if (sym2.Access == ACCESS.PRIVATE)
            {
                return true;
            }

            if (sym1.IsTYPESYM)
            {
                if ((sym1 as TYPESYM).HasErrors)
                {
                    // Error syms have already generated an error - don't generate another.
                    return true;
                }

            LRecurse:
                // If sym1 is a pointer, array, or byref type, convert to underlying type. 
                switch (sym1.Kind)
                {
                    case SYMKIND.ARRAYSYM:
                    case SYMKIND.PTRSYM:
                    case SYMKIND.NUBSYM:
                    case SYMKIND.PARAMMODSYM:
                    case SYMKIND.MODOPTTYPESYM:
                        sym1 = sym1.ParentSym as TYPESYM;
                        goto LRecurse;

                    case SYMKIND.VOIDSYM:
                        // void is completely visible.
                        return true;

                    case SYMKIND.TYVARSYM:
                        // in the current model tyvar's are completely visible.
                        // This may not be what we desire in the long run -
                        // it is possible to imagine that a tyvar is private,
                        // in the sense that the generic class must promise not to reveal anything
                        // more about it than the user of the generic class already knows.
                        return true;

                    case SYMKIND.AGGTYPESYM:
                        // GENERICS: check visibility of argument types (e.g. [String] in List<String>), as
                        // well as the root type which we check via the loop.
                        // Check all type parameters here (including ones from outer types).
                        // aggregates are then checked below
                        for (int i = 0; i < (sym1 as AGGTYPESYM).AllTypeArguments.Count; ++i)
                        {
                            if (!IsAtLeastAsVisibleAs((sym1 as AGGTYPESYM).AllTypeArguments[i], sym2))
                                return false;
                        }
                        sym1 = (sym1 as AGGTYPESYM).GetAggregate();
                        break;

                    default:
                        break;
                }
            }

            // Algorithm:
            // The only way that sym1 is NOT at least as visible as sym2 is
            // if it has a access restriction that sym2 does not have.
            // So, we simply go up the parent chain on sym1.
            // For each access modifier found, we check to see that the same access modifier,
            // or a more restrictive one, is somewhere is sym2's parent chain.

            for (SYM s1 = sym1; s1 != null && !s1.IsNSSYM; s1 = s1.ParentSym)
            {
                ACCESS acc1 = s1.Access;
                if (acc1 == ACCESS.PUBLIC)
                {
                    continue;
                }
                bool asRestrictive = false;

                for (SYM s2 = sym2; s2 != null && !s2.IsNSSYM; s2 = s2.ParentSym)
                {
                    ACCESS acc2 = s2.Access;

                    switch (acc1)
                    {
                        case ACCESS.INTERNAL:
                            // If s2 is private or internal, and within the same assembly as s1,
                            // then this is at least as restrictive as s1's internal. 
                            if ((acc2 == ACCESS.PRIVATE || acc2 == ACCESS.INTERNAL) && s2.SameAssemOrFriend(s1))
                            {
                                asRestrictive = true;
                            }
                            break;

                        case ACCESS.PROTECTED:
                            parentSym1 = s1.ParentSym;

                            if (acc2 == ACCESS.PRIVATE)
                            {
                                // if s2 is private and within s1's parent or within a subclass of s1's parent,
                                // then this is at least as restrictive as s1's protected. 
                                for (parentSym2 = s2.ParentSym; !parentSym2.IsNSSYM; parentSym2 = parentSym2.ParentSym)
                                {
                                    if (Compiler.IsBaseAggregate(parentSym2 as AGGSYM, parentSym1 as AGGSYM))
                                    {
                                        asRestrictive = true;
                                    }
                                }
                            }
                            else if (acc2 == ACCESS.PROTECTED)
                            {
                                // if s2 is protected, and it's parent is a subclass (or the same as) s1's parent
                                // then this is at least as restrictive as s1's protected
                                if (Compiler.IsBaseAggregate(s2.ParentSym as AGGSYM, parentSym1 as AGGSYM))
                                {
                                    asRestrictive = true;
                                }
                            }
                            break;

                        case ACCESS.INTERNALPROTECTED:
                            parentSym1 = s1.ParentSym;

                            if (acc2 == ACCESS.PRIVATE)
                            {
                                // if s2 is private and within a subclass of s1's parent,
                                // or withing the same assembly as s1
                                // then this is at least as restrictive as s1's internal protected. 
                                if (s2.SameAssemOrFriend(s1))
                                {
                                    asRestrictive = true;
                                }
                                else
                                {
                                    for (parentSym2 = s2.ParentSym; !parentSym2.IsNSSYM; parentSym2 = parentSym2.ParentSym)
                                        if (Compiler.IsBaseAggregate(parentSym2 as AGGSYM, parentSym1 as AGGSYM))
                                        {
                                            asRestrictive = true;
                                        }
                                }
                            }
                            else if (acc2 == ACCESS.INTERNAL)
                            {
                                // If s2 is in the same assembly as s1, then this is more restrictive
                                // than s1's internal protected.
                                if (s2.SameAssemOrFriend(s1))
                                    asRestrictive = true;
                            }
                            else if (acc2 == ACCESS.PROTECTED)
                            {
                                // if s2 is protected, and it's parent is a subclass (or the same as) s1's parent
                                // then this is at least as restrictive as s1's internal protected
                                if (Compiler.IsBaseAggregate(s2.ParentSym as AGGSYM, parentSym1 as AGGSYM))
                                {
                                    asRestrictive = true;
                                }
                            }
                            else if (acc2 == ACCESS.INTERNALPROTECTED)
                            {
                                // if s2 is internal protected, and it's parent is a subclass (or the same as) s1's parent
                                // and its in the same assembly and s1, then this is at least as restrictive as s1's protected
                                if (s2.SameAssemOrFriend(s1) &&
                                    Compiler.IsBaseAggregate(s2.ParentSym as AGGSYM, parentSym1 as AGGSYM))
                                {
                                    asRestrictive = true;
                                }
                            }
                            break;

                        case ACCESS.PRIVATE:
                            if (acc2 == ACCESS.PRIVATE)
                            {
                                // if s2 is private, and it is withing s1's parent, then this is at
                                // least as restrictive of s1's private.
                                parentSym1 = s1.ParentSym;
                                for (parentSym2 = s2.ParentSym; !parentSym2.IsNSSYM; parentSym2 = parentSym2.ParentSym)
                                {
                                    if (parentSym2 == parentSym1) asRestrictive = true;
                                }
                            }
                            break;

                        default:
                            DebugUtil.Assert(false);
                            break;
                    }
                }
                if (!asRestrictive)
                {
                    return false;  // no modifier on sym2 was as restrictive as s1
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // CLSDREC.CheckConstituentVisibility
        //
        /// <summary>
        /// Check a symbol and a constituent symbol to make sure the constituent is at least 
        /// as visible as the main symbol. If not, report an error of the given code.
        /// </summary>
        /// <param name="main"></param>
        /// <param name="constituent"></param>
        /// <param name="errCode"></param>
        //------------------------------------------------------------
        private void CheckConstituentVisibility(SYM main, SYM constituent, CSCERRID errCode)
        {
            if (!IsAtLeastAsVisibleAs(constituent, main))
            {
                Compiler.ErrorRef(null, errCode, new ErrArgRef(main), new ErrArgRef(constituent));
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckUnsafe
        //
        /// <summary>
        /// <para>Issue an error if the given type is unsafe.</para>
        /// <para>This function should NOT be called if the type was used in an unsafe context
        /// -- this function doesn't check that we're in an unsafe context.</para>
        /// <para>(Argument errCode has the default value ERR_UnsafeNeeded in sscli.)</para>
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="type"></param>
        /// <param name="unsafeContext"></param>
        /// <param name="errCode"></param>
        //------------------------------------------------------------
        private void CheckUnsafe(
            BASENODE tree,
            TYPESYM type,
            bool unsafeContext,
            CSCERRID errCode)  // = ERR_UnsafeNeeded
        {
            if (type.IsUnsafe() && !unsafeContext)
            {
                Compiler.Error(tree, errCode);
            }
        }

        // Normally in all of the following "created" is NULL.  However, in some cases we have to create
        // a member before we can reasonably carry out the following checks.  For example, when we
        // have a generic static method, we have to create the symbol for the method BEFORE we
        // parse the argument types, because the argument types might refer to the type parameters.
        //
        // Thus, we sometimes pass in a "created" value to indicate that we might find a symbol matching the given types
        // but that it's not a problem if it == created.

        //------------------------------------------------------------
        // CLSDREC.CheckConstituentVisibility
        //
        /// <summary>
        /// <para>This method just strictly checks for name conflicts and
        /// does not take into account method params, </para>
        /// <para>(Argument created has the default value NULL in sscli.)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parseTree"></param>
        /// <param name="cls"></param>
        /// <param name="created"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool CheckForBadMemberSimple(
            string name,
            BASENODE parseTree,
            AGGSYM cls,
            SYM created)    //= NULL
        {
            // check for name same as that of parent aggregate
            // Name same as type is only an error for classes and structs
            if (name != null && name == cls.Name && (cls.IsClass || cls.IsStruct))
            {
                Compiler.Error(parseTree, CSCERRID.ERR_MemberNameSameAsType,
                    new ErrArg(name), new ErrArgRefOnly(cls));
                return false;
            }

            return CheckForDuplicateSymbol(name, parseTree, cls, created);
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForBadMember
        //
        /// <summary>
        /// <para>(In sscli, created has the default value null.)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="symkind"></param>
        /// <param name="paramTypes"></param>
        /// <param name="parseTree"></param>
        /// <param name="cls"></param>
        /// <param name="typeVarsMeth"></param>
        /// <param name="created"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool CheckForBadMember(
            string name,
            SYMKIND symkind,
            TypeArray paramTypes,
            BASENODE parseTree,
            AGGSYM cls,
            TypeArray typeVarsMeth,
            SYM created)    //= NULL
        {
            DebugUtil.Assert(symkind == SYMKIND.PROPSYM || symkind == SYMKIND.METHSYM);

            // check for name same as that of parent aggregate
            // Name same as type is only an error for classes and structs
            if (name != null && name == cls.Name && (cls.IsClass || cls.IsStruct))
            {
                Compiler.Error(
                    parseTree,
                    CSCERRID.ERR_MemberNameSameAsType,
                    new ErrArg(name),
                    new ErrArgRefOnly(cls));
                return false;
            }

            return CheckForDuplicateSymbol(
                name,
                symkind,
                paramTypes,
                parseTree,
                cls,
                typeVarsMeth,
                created);
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForDuplicateSymbol
        //
        /// <summary>
        /// Return false if the name is already registered in the specified AGGSYM instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parseTree"></param>
        /// <param name="cls"></param>
        /// <param name="created"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool CheckForDuplicateSymbol(
            string name,
            BASENODE parseTree,
            AGGSYM cls,
            SYM created)    // = NULL
        {
            if (name != null)
            {
                SYM present = Compiler.MainSymbolManager.LookupAggMember(
                    name,
                    cls,
                    SYMBMASK.ALL);

                if (present != null && present != created)
                {
                    Compiler.Error(
                        parseTree,
                        CSCERRID.ERR_DuplicateNameInClass,
                        new ErrArg(name),
                        new ErrArg(cls),
                        new ErrArgRefOnly(present));
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForDuplicateSymbol
        //
        /// <summary>
        /// <para>Returns false if there is a duplicate,
        /// true if there is no duplicate.</para>
        /// <para>(In sscli, createSym has the default value null.)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="symKind"></param>
        /// <param name="paramTypes"></param>
        /// <param name="parseTreeNode"></param>
        /// <param name="aggSym"></param>
        /// <param name="typeVarsMeth"></param>
        /// <param name="createdSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool CheckForDuplicateSymbol(
            string name,
            SYMKIND symKind,
            TypeArray paramTypes,
            BASENODE parseTreeNode,
            AGGSYM aggSym,
            TypeArray typeVarsMeth,
            SYM createdSym)    // = null
        {
            DebugUtil.Assert(symKind == SYMKIND.PROPSYM || symKind == SYMKIND.METHSYM);
            DebugUtil.Assert(symKind == SYMKIND.METHSYM || TypeArray.Size(typeVarsMeth) == 0);
            DebugUtil.Assert(paramTypes != null);

            if (name == null || paramTypes.HasErrors)
            {
                return true;
            }

            CSCERRID errId;
            SYM currentSym;
            METHSYM createdMethSym = null;

            // (CS3) partial method
            bool isPartialMethod = false;
            List<METHSYM> partialMethList = null;
            METHSYM partialMethImplSym = null;

            if (createdSym != null && createdSym.Kind == SYMKIND.METHSYM)
            {
                createdMethSym = createdSym as METHSYM;
                isPartialMethod = createdMethSym.IsPartialMethod;
                if (isPartialMethod)
                {
                    partialMethList = new List<METHSYM>();
                    if (createdMethSym.HasNoBody)
                    {
                        partialMethList.Add(createdMethSym);
                    }
                    else
                    {
                        partialMethImplSym = createdMethSym;
                    }
                }
            }

            for (currentSym = Compiler.MainSymbolManager.LookupAggMember(name, aggSym, SYMBMASK.ALL);
                currentSym != null;
                currentSym = BSYMMGR.LookupNextSym(currentSym, aggSym, SYMBMASK.ALL))
            {
                if (currentSym == createdSym)
                {
                    // Don't count this one as a duplicate!
                    continue;
                }

                if (currentSym.Kind != symKind)
                {
                    // Different kinds of members so can't overload.
                    errId = CSCERRID.ERR_DuplicateNameInClass;
                    goto LError;
                }

                // Same kind of symbol.
                METHPROPSYM currentMethPropSym = currentSym as METHPROPSYM;

                if (currentMethPropSym.ParameterTypes.Count != paramTypes.Count)
                {
                    // Different number of parameters so not a duplicate.
                    continue;
                }

                if (symKind == SYMKIND.METHSYM &&
                    (currentSym as METHSYM).TypeVariables.Count != TypeArray.Size(typeVarsMeth))
                {
                    // Overloading on arity is fine.
                    continue;
                }

                // Substitute method level type parameters.
                // Note that there is no need to substitute class level type variables
                // since we're looking in the same class.
                TypeArray currentParams = Compiler.MainSymbolManager.SubstTypeArray(
                    currentMethPropSym.ParameterTypes,
                    (TypeArray)null,
                    typeVarsMeth,
                    SubstTypeFlagsEnum.NormNone);

                if (currentParams == paramTypes)
                {
                    // (CS3) partial method
                    if (createdSym != null &&
                        symKind == SYMKIND.METHSYM &&
                        (createdSym as METHSYM).IsPartialMethod)
                    {
                        if (!CheckForDulicatePartialMethod(
                                parseTreeNode,
                                createdSym as METHSYM,
                                currentMethPropSym as METHSYM))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        errId = CSCERRID.ERR_MemberAlreadyExists;
                        goto LError;
                    }
                }

                if (symKind == SYMKIND.METHSYM)
                {
                    // Check for overloading on ref and out.
                    for (int i = 0; ; ++i)
                    {
                        if (i >= paramTypes.Count)
                        {
                            // (CS3) partial method
                            if ((createdSym as METHSYM).IsPartialMethod)
                            {
                                if (!CheckForDulicatePartialMethod(
                                        parseTreeNode,
                                        createdSym as METHSYM,
                                        currentMethPropSym as METHSYM))
                                {
                                    return false;
                                }
                                break;
                            }
                            else
                            {
                                Compiler.Error(
                                    parseTreeNode,
                                    CSCERRID.ERR_OverloadRefOut,
                                    new ErrArg(name),
                                    new ErrArgRefOnly(currentSym));
                                return false;
                            }
                        }

                        TYPESYM typeSym1 = paramTypes[i];
                        TYPESYM typeSym2 = currentParams[i];
                        if (typeSym1 != typeSym2 &&
                            !(typeSym1.IsPARAMMODSYM &&
                            typeSym2.IsPARAMMODSYM &&
                            (typeSym1 as PARAMMODSYM).ParamTypeSym == (typeSym2 as PARAMMODSYM).ParamTypeSym))
                        {
                            // typeSym1 != typeSym2 && typeSym1 and typeSym2 differ by more than out/ref-ness.
                            // So these signatures differ by more than out/ref-ness.
                            break;
                        }
                    }
                }

                // (CS3) partial method
                if (isPartialMethod &&
                    currentSym.Kind == SYMKIND.METHSYM)
                {
                    METHSYM curMethSym = currentSym as METHSYM;
                    if (curMethSym.IsPartialMethod)
                    {
                        if (curMethSym.HasNoBody)
                        {
                            partialMethList.Add(curMethSym);
                        }
                        else
                        {
                            DebugUtil.Assert(partialMethImplSym == null);
                            partialMethImplSym = curMethSym;
                        }
                    }
                }
            }

            // (CS3) partial method
            if (isPartialMethod &&
                partialMethImplSym != null &&
                partialMethList.Count > 0)
            {
                for (int i = 0; i < partialMethList.Count; ++i)
                {
                    partialMethList[i].PartialMethodImplSym = partialMethImplSym;
                }
            }

            return true;

        LError:
            if (name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR) ||
                name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.STATCTOR))
            {
                name = aggSym.Name;
            }
            else if (parseTreeNode.Kind == NODEKIND.DTOR && currentSym.IsMETHSYM && (currentSym as METHSYM).IsDtor)
            {
                // If we have 2 destructors, report it as a duplicate destructor
                // otherwise report it as a duplicate Finalize()
                //size_t temp_len = wcslen(aggSym.name.text) + 2;
                //WCHAR* temp = STACK_ALLOC(WCHAR, temp_len);
                //temp[0] = L'~';
                //HRESULT hr;
                //hr = StringCchCopyW(temp + 1, temp_len - 1, aggSym.name.text);
                //DebugUtil.Assert (SUCCEEDED (hr));
                name = String.Concat("~", aggSym.Name);
                Compiler.NameManager.AddString(name);
            }

            Compiler.Error(parseTreeNode, errId,
                new ErrArg(name), new ErrArg(aggSym), new ErrArgRefOnly(currentSym));

            return false;
        }

        //------------------------------------------------------------
        // CLSDREC.CopyMethTyVarsToClass
        //
        /// <summary></summary>
        /// <param name="methpropSym"></param>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void CopyMethTyVarsToClass(METHPROPSYM methpropSym, AGGSYM aggSym)
        {
            DebugUtil.Assert(!aggSym.IsArityInName);
            List<TYPESYM> formalTypeArray = null;

            if (methpropSym != null &&
                methpropSym.IsMETHSYM &&
                (methpropSym as METHSYM).TypeVariables.Count > 0)
            {
                METHSYM outerMethSym = methpropSym as METHSYM;
                int formalTypeCount = outerMethSym.TypeVariables.Count;
                int parentParamCount = (aggSym.ParentBagSym as AGGSYM).AllTypeVariables.Count;

                // Make a TypeArray for the TYVARs copied from the method
                //TYVARSYM ** ppTypeFormals = STACK_ALLOC(TYVARSYM*, formalTypeCount);
                formalTypeArray = new List<TYPESYM>();
                for (int i = 0; i < formalTypeCount; ++i)
                {
                    TYVARSYM outerMethTypeVar = outerMethSym.TypeVariables.ItemAsTYVARSYM(i);
                    TYVARSYM currentTypeVar = CreateTypeVar(
                        aggSym,
                        outerMethTypeVar.ParseTreeNode,
                        i,
                        formalTypeArray,
                        0,
                        parentParamCount + i);
                    currentTypeVar.AttributeList = outerMethTypeVar.AttributeList;
                    //currentTypeVar.AttributeListTail = null;
                    // setting to null, because nothing should be adding more nodes on this list
                    formalTypeArray.Add(currentTypeVar);
                }
                aggSym.TypeVariables = compiler.MainSymbolManager.AllocParams(formalTypeArray);
                aggSym.AllTypeVariables = compiler.MainSymbolManager.ConcatParams(
                    (aggSym.ParentBagSym as AGGSYM).AllTypeVariables,
                    aggSym.TypeVariables);

                // Now fixup the bounds to point to the class' TYVARs
                // ... and copy the special bit constraints as well
                for (int i = 0; i < formalTypeCount; ++i)
                {
                    TYVARSYM outerMethTypeVar = outerMethSym.TypeVariables.ItemAsTYVARSYM(i);
                    compiler.SetBounds(
                        formalTypeArray[i] as TYVARSYM,
                        compiler.MainSymbolManager.SubstTypeArray(
                            outerMethTypeVar.BoundArray,
                            (TypeArray)null,
                            aggSym.TypeVariables,
                            SubstTypeFlagsEnum.NormNone),
                            false);
                    (formalTypeArray[i] as TYVARSYM).Constraints = outerMethTypeVar.Constraints;
                }
                for (int i = 0; i < formalTypeCount; ++i)
                {
                    DebugUtil.VsVerify(
                        compiler.ResolveBounds(formalTypeArray[i] as TYVARSYM, true),
                        "ResolveBounds failed!");
                }
                aggSym.IsArityInName = true;
            }
            else
            {
                DebugUtil.Assert(aggSym.TypeVariables == BSYMMGR.EmptyTypeArray);
                DebugUtil.Assert(
                    aggSym.AllTypeVariables == (aggSym.ParentBagSym as AGGSYM).AllTypeVariables);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckCLSnaming
        //
        /// <summary>
        /// checks for case-insensitive collisions in public members of any AGGSYM.
        /// Also checks that we don't overload solely by ref or out.
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void CheckCLSnaming(AGGSYM aggSym)
        {
            DebugUtil.Assert(Compiler.AllowCLSErrors());
            DebugUtil.Assert(aggSym != null && aggSym.HasExternalAccess());

            //WCHAR buffer[2 * MAX_IDENT_SIZE];
            //WCBuffer wcbuffer(buffer);
            string tempStr = null;
            UNITSYM clsRootSym = null;
            CACHESYM tempCacheSym = null;
            string name = null;

            // Create a local symbol table root for this stuff - UNITSYM has no meaning here.
            // It's just a convenient parent sym type.
            name = "$clsnamecheck$";
            clsRootSym = Compiler.LocalSymbolManager.CreateLocalSym(SYMKIND.UNITSYM, name, null) as UNITSYM;
            Compiler.NameManager.AddString(name);

            // notice that for all these 'fake' members we're adding the sym.sym
            // is not the actual type of the 'fake' member, but a pointer back to
            // the orignal member

            // add all externally visible members of interfaces
            for (int i = 0; i < aggSym.AllInterfaces.Count; ++i)
            {
                AGGTYPESYM baseAts = aggSym.AllInterfaces[i] as AGGTYPESYM;
                AGGSYM baseAgg = baseAts.GetAggregate();
                DebugUtil.Assert(baseAgg != null);

                for (SYM member = baseAgg.FirstChildSym; member != null; member = member.NextSym)
                {
                    if (member.HasExternalAccess() && (!member.IsMETHPROPSYM || !(member as METHPROPSYM).IsOverride))
                    {
                        // Add a lower-case symbol to our list (we don't care about collisions here)
                        //SafeToLowerCase( member.Name, wcbuffer);
                        tempStr = member.Name.ToLower();
                        tempCacheSym = Compiler.LocalSymbolManager.CreateLocalSym(
                            SYMKIND.CACHESYM, tempStr, clsRootSym) as CACHESYM;
                        Compiler.NameManager.AddString(tempStr);
                        tempCacheSym.EntrySym = member;
                    }
                }
            }

            // add all the externally visible members of base classes
            AGGTYPESYM tempAts = aggSym.BaseClassSym;
            while (tempAts != null)
            {
                AGGTYPESYM baseAts = tempAts;
                DebugUtil.Assert(baseAts != null);

                if (!Compiler.IsCLS_Type(aggSym.GetOuterAgg(), baseAts))
                {
                    Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadBase,
                        new ErrArgRef(aggSym), new ErrArgRef(baseAts));
                }

                for (SYM member = baseAts.GetAggregate().FirstChildSym; member != null; member = member.NextSym)
                {
                    if (member.HasExternalAccess() &&
                        (!member.IsMETHPROPSYM || !(member as METHPROPSYM).IsOverride))
                    {
                        // Add a lower-case symbol to our list (we don't care about collisions here)
                        //SafeToLowerCase( member.name.text, wcbuffer);
                        tempStr = member.Name.ToLower();
                        tempCacheSym = Compiler.LocalSymbolManager.CreateLocalSym(
                            SYMKIND.CACHESYM, tempStr, clsRootSym) as CACHESYM;
                        Compiler.NameManager.AddString(tempStr);
                        tempCacheSym.EntrySym = member;
                    }
                }
                tempAts = tempAts.GetBaseClass();
            }

            // Also check the underlying type of Enums
            if (aggSym.IsEnum && !Compiler.IsCLS_Type(aggSym.GetOuterAgg(), aggSym.UnderlyingTypeSym))
            {
                Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadBase,
                    new ErrArgRef(aggSym), new ErrArgRef(aggSym.UnderlyingTypeSym));
            }

            // We don't check typeVarsAll becuase the outer-type will check those
            if (aggSym.TypeVariables.Count > 0)
            {
                // check bounds...
                for (int i = 0; i < aggSym.TypeVariables.Count; ++i)
                {
                    TYVARSYM tvSym = aggSym.TypeVariables.ItemAsTYVARSYM(i);
                    TypeArray bnds = tvSym.BoundArray;
                    for (int j = 0; j < bnds.Count; ++j)
                    {
                        TYPESYM bndSym = bnds[j];
                        if (!Compiler.IsCLS_Type(aggSym.GetOuterAgg(), bndSym))
                        {
                            Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadTypeVar, new ErrArgRef(bndSym));
                        }
                    }
                }
            }

            for (SYM member = aggSym.FirstChildSym; member != null; member = member.NextSym)
            {
                if (member.IsARRAYSYM ||
                    member.IsTYVARSYM ||
                    member.IsAGGTYPESYM ||
                    member.IsPTRSYM ||
                    member.IsPARAMMODSYM ||
                    member.IsPINNEDSYM ||
                    member.IsFAKEMETHSYM)
                {
                    continue;
                }

                if (!Compiler.CheckSymForCLS(member, false))
                {
                    if (aggSym.IsInterface)
                    {
                        // CLS Compliant Interfaces can't have non-compliant members
                        Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadInterfaceMember, new ErrArgRef(member));
                    }
                    else if (member.IsMETHSYM && (member as METHSYM).IsAbstract)
                    {
                        // CLS Compliant types can't have non-compliant abstract members
                        Compiler.ErrorRef(null, CSCERRID.WRN_CLS_NoAbstractMembers, new ErrArgRef(member));
                    }
                }
                else if (
                    member.HasExternalAccess() &&
                    (!member.IsMETHPROPSYM || !(member as METHPROPSYM).IsOverride))
                {
                    //SafeToLowerCase( member.name.text, wcbuffer);
                    tempStr = member.Name.ToLower();
                    if (tempStr[0] == (char)0x005F || tempStr[0] == (char)0xFF3F)
                    {
                        // According to CLS Spec these are '_'
                        Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadIdentifier, new ErrArgRef(member));
                    }
                    //PNAME nameLower = Compiler.NameManager.AddString(buffer);
                    Compiler.NameManager.AddString(tempStr);

                    // Check for colliding names
                    tempCacheSym = Compiler.LocalSymbolManager.LookupLocalSym(
                        tempStr, clsRootSym, SYMBMASK.CACHESYM) as CACHESYM;
                    if (tempCacheSym != null)
                    {
                        // If names are different, then they must differ only in case.
                        if (member.Name != tempCacheSym.EntrySym.Name)
                        {
                            Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadIdentifierCase,
                                new ErrArgRef(member), new ErrArgRef(tempCacheSym.EntrySym));
                        }

                        // Check for colliding signatures (but don't check the accessors)
                        if (member.IsMETHPROPSYM &&
                            (!member.IsMETHSYM || !(member as METHSYM).IsAnyAccessor))
                        {
                            TYPESYM unnamedArray = Compiler.MainSymbolManager.GetArray(
                                Compiler.MainSymbolManager.VoidSym, 1, null);
                            TypeArray reducedSig = CLSReduceSignature((member as METHPROPSYM).ParameterTypes);
                            bool hasUnnamedArray = reducedSig.Contains(unnamedArray);
                            while (tempCacheSym != null)
                            {
                                SYM sym = tempCacheSym.EntrySym;
                                if (sym.IsMETHPROPSYM &&
                                    (member as METHPROPSYM).ParameterTypes != (sym as METHPROPSYM).ParameterTypes &&
                                    (member as METHPROPSYM).ParameterTypes.Count ==
                                        (sym as METHPROPSYM).ParameterTypes.Count)
                                {
                                    TypeArray otherReducedSig = CLSReduceSignature((sym as METHPROPSYM).ParameterTypes);
                                    if (reducedSig == otherReducedSig && !hasUnnamedArray)
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.WRN_CLS_OverloadRefOut,
                                            new ErrArgRef(member), new ErrArgRef(sym));
                                    }
                                    else if (hasUnnamedArray || otherReducedSig.Contains(unnamedArray))
                                    {
                                        // If the reduced signatures are the same (and the non-reduced ones are different)
                                        // and one of the reduced signatures contains an unnamed array, then they both do
                                        // so the overload is based on the unnamed array (and possibly something else)
                                        if (reducedSig != otherReducedSig)
                                        {
                                            //TYPESYM ** m1 = reducedSig.ItemPtr(0);
                                            //TYPESYM ** m2 = otherReducedSig.ItemPtr(0);
                                            TYPESYM m1, m2;
                                            for (int i = 0; i < reducedSig.Count; ++i)	//, m1++, m2++)
                                            {
                                                m1 = reducedSig[i];
                                                m2 = otherReducedSig[i];
                                                //if (*m1 != *m2 &&
                                                //    (!(*m1).isARRAYSYM() ||
                                                //    !(*m2).isARRAYSYM() ||
                                                //    (*m1 != unnamedArray && *m2 != unnamedArray)))
                                                if (m1 != m2 &&
                                                    (!m1.IsARRAYSYM ||
                                                    !m2.IsARRAYSYM ||
                                                    (m1 != unnamedArray && m2 != unnamedArray)))
                                                {
                                                    // The 2 types lists are different and the difference
                                                    // is not based on an unnamed array
                                                    goto CONTINUE;
                                                }
                                            }
                                        }
                                        Compiler.ErrorRef(null, CSCERRID.WRN_CLS_OverloadUnnamed,
                                            new ErrArgRef(member), new ErrArgRef(sym));
                                    }
                                }

                            CONTINUE:
                                tempCacheSym = BSYMMGR.LookupNextSym(
                                    tempCacheSym, clsRootSym, SYMBMASK.CACHESYM) as CACHESYM;
                            }
                        }
                    }

                    // Add to the table.
                    tempCacheSym = Compiler.LocalSymbolManager.CreateLocalSym(
                        SYMKIND.CACHESYM, tempStr, clsRootSym) as CACHESYM;
                    tempCacheSym.EntrySym = member;
                }
            }

            //Cleanup
            Compiler.DiscardLocalState();
        }

        //------------------------------------------------------------
        // CLSDREC.checkCLSnaming
        //
        /// <summary>
        /// checks for case-insensitive collisions in
        /// public members of any NSSYM
        /// </summary>
        /// <param name="nsSym"></param>
        //------------------------------------------------------------
        private void CheckCLSnaming(NSSYM nsSym)
        {
            DebugUtil.Assert(Compiler.AllowCLSErrors());
            DebugUtil.Assert(nsSym != null && nsSym.HasExternalAccess());

            //WCHAR buffer[2 * MAX_IDENT_SIZE];
            // import.h: #define MAX_IDENT_SIZE 512
            // Maximum identifier size we allow.  This is the max the compiler allows
            //WCBuffer wcbuffer(buffer);
            UNITSYM clsRootSym;
            CACHESYM tempCacheSym;
            string tempStr;

            // Create a local symbol table root for this stuff - UNITSYM has no meaning here.
            // It's just a convenient parent sym type.
            tempStr = "$clsnamecheck$";
            clsRootSym = Compiler.LocalSymbolManager.CreateLocalSym(
                SYMKIND.UNITSYM, tempStr, null) as UNITSYM;
            Compiler.NameManager.AddString(tempStr);

            // notice that for all these 'fake' members we're adding the sym.sym 
            // is not the actual type of the 'fake' member, but a pointer back to
            // the orignal member

            for (NSDECLSYM decl = nsSym.FirstDeclSym; decl != null; decl = decl.NextDeclSym)
            {
                for (SYM member = decl.FirstChildSym; member != null; member = member.NextSym)
                {
                    if ((member.Mask & (SYMBMASK.GLOBALATTRSYM | SYMBMASK.ALIASSYM)) != 0)
                    {
                        continue;
                    }

                    // If an AGGSYM isn't declared, declare it, unless it's a mere metadata reference
                    // that can't be declared, in which case it should be skipped.
                    // passing 'true' into CheckSymForCLS (unlike the default false) will do this for
                    // for us by Declaring everything that is can be, and returning false if it bumps
                    // into something that can't be declared.
                    if (!member.HasExternalAccess() || !Compiler.CheckSymForCLS(member, true))
                    {
                        continue;
                    }

                    if (member.IsAGGDECLSYM)
                    {
                        if (!(member as AGGDECLSYM).IsFirst)
                        {
                            continue;
                        }

                        AGGSYM aggSym = (member as AGGDECLSYM).AggSym;

                        //SafeToLowerCase( aggSym.name.text, wcbuffer);
                        tempStr = aggSym.Name.ToLower();

                        //Only warn on problems with source that is being compiled.
                        if (aggSym.IsSource && (tempStr[0] == (char)0x005F || tempStr[0] == (char)0xFF3F))
                        {
                            // According to CLS Spec these are '_'
                            Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadIdentifier,
                                new ErrArgRef(aggSym));
                        }

                        // Check for colliding names
                        tempCacheSym = Compiler.LocalSymbolManager.LookupLocalSym(
                            tempStr, clsRootSym, SYMBMASK.CACHESYM) as CACHESYM;
                        Compiler.NameManager.AddString(tempStr);
                        if (tempCacheSym != null && aggSym.Name != tempCacheSym.EntrySym.Name)
                        {
                            if (aggSym.IsSource ||
                                (tempCacheSym.EntrySym.IsAGGSYM && (tempCacheSym.EntrySym as AGGSYM).IsSource) ||
                                (tempCacheSym.EntrySym.IsNSSYM && (tempCacheSym.EntrySym as NSSYM).IsDefinedInSource))
                            {
                                Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadIdentifierCase,
                                    new ErrArgRef(aggSym), new ErrArgRef(tempCacheSym.EntrySym));
                            }
                        }
                        else
                        {
                            tempCacheSym = Compiler.LocalSymbolManager.CreateLocalSym(
                                SYMKIND.CACHESYM, tempStr, clsRootSym) as CACHESYM;
                            // Compiler.NameManager.AddString( buffer);	// double registration
                            tempCacheSym.EntrySym = aggSym;
                        }
                    }
                    else if (member.IsNSDECLSYM)
                    {
                        NSSYM nestedNsSym = (member as NSDECLSYM).NamespaceSym;

                        // Only check namespaces that we haven't already visited
                        if (!nestedNsSym.CheckingForCLS)
                        {
                            nestedNsSym.CheckingForCLS = true;

                            //SafeToLowerCase( nestedNsSym.name.text, wcbuffer);
                            tempStr = nestedNsSym.Name.ToLower();

                            //Only warn on problems with source that is being compiled.
                            if (nestedNsSym.IsDefinedInSource &&
                                (tempStr[0] == (char)0x005F || tempStr[0] == (char)0xFF3F))
                            {
                                // According to CLS Spec these are '_'
                                Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadIdentifier,
                                    new ErrArgRef(member));
                            }

                            // Check for colliding names
                            tempCacheSym = Compiler.LocalSymbolManager.LookupLocalSym(
                                tempStr, clsRootSym, SYMBMASK.CACHESYM) as CACHESYM;
                            Compiler.NameManager.AddString(tempStr);
                            if (tempCacheSym != null &&
                                nestedNsSym.Name != tempCacheSym.EntrySym.Name)
                            {
                                if (tempCacheSym.EntrySym.IsNSSYM &&
                                    ((tempCacheSym.EntrySym as NSSYM).IsDefinedInSource ||
                                    nestedNsSym.IsDefinedInSource) ||
                                    tempCacheSym.EntrySym.IsAGGSYM &&
                                    (tempCacheSym.EntrySym as AGGSYM).IsSource)
                                {
                                    Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadIdentifierCase,
                                        new ErrArgRef(tempCacheSym.EntrySym), new ErrArgRef(nestedNsSym));
                                }
                            }
                            else
                            {
                                tempCacheSym = Compiler.LocalSymbolManager.CreateLocalSym(
                                    SYMKIND.CACHESYM, tempStr, clsRootSym) as CACHESYM;
                                //Compiler.NameManager.AddString( tempStr);	// double registration
                                tempCacheSym.EntrySym = nestedNsSym;
                            }
                        }
                    }
                    else
                    {
                        DebugUtil.Assert(!member.IsAGGSYM);
                    }
                }
            }
            nsSym.CheckingForCLS = true;

            //Cleanup
            Compiler.DiscardLocalState();
        }

        //------------------------------------------------------------
        // CLSDREC.CheckSigForCLS
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <param name="errorSym"></param>
        /// <param name="paramTreeNode"></param>
        //------------------------------------------------------------
        private void CheckSigForCLS(METHSYM methodSym, SYM errorSym, BASENODE paramTreeNode)
        {
            DebugUtil.Assert(Compiler.AllowCLSErrors());
            DebugUtil.Assert(
                errorSym == methodSym ||
                ((errorSym as AGGSYM).IsDelegate && methodSym.IsInvoke));

            if (methodSym.IsVarargs)
            {
                Compiler.ErrorRef(null, CSCERRID.WRN_CLS_NoVarArgs, new ErrArgRef(errorSym));
            }
            if (methodSym.ReturnTypeSym != null &&
                !Compiler.IsCLS_Type(methodSym.ParentSym, methodSym.ReturnTypeSym))
            {
                Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadReturnType, new ErrArgRef(errorSym));
            }
            if (methodSym.TypeVariables.Count > 0)
            {
                // check bounds...
                for (int i = 0, n = methodSym.TypeVariables.Count; i < n; i++)
                {
                    TYVARSYM var = methodSym.TypeVariables.ItemAsTYVARSYM(i);
                    TypeArray bnds = var.BoundArray;
                    for (int j = 0; j < bnds.Count; j++)
                    {
                        TYPESYM typeBnd = bnds[j];
                        if (!Compiler.IsCLS_Type(methodSym.ParentSym, typeBnd))
                        {
                            Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadTypeVar, new ErrArgRef(typeBnd));
                        }
                    }
                }
            }

            int index = 0;
            BASENODE node = paramTreeNode;
            while (node != null)
            {
                PARAMETERNODE param;
                if (node.Kind == NODEKIND.LIST)
                {
                    param = node.AsLIST.Operand1 as PARAMETERNODE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    param = node as PARAMETERNODE;
                    node = null;
                }

                if (!Compiler.IsCLS_Type(methodSym.ParentSym, methodSym.ParameterTypes[index]))
                {
                    Compiler.Error(
                        param,
                        CSCERRID.WRN_CLS_BadArgType,
                        new ErrArg(methodSym.ParameterTypes[index]));
                }
                ++index;
            }
            DebugUtil.Assert(paramTreeNode == null || index == methodSym.ParameterTypes.Count);
        }


        //------------------------------------------------------------
        // CLSDREC.CLSReduceSignature
        //
        /// <summary>
        /// <para>Reduces a signature to "CLS reduced form", which
        /// <list type="number">
        /// <item>Removes ref or out from all parameters in a signature.</item>
        /// <item>Changes arrays of rank > 1 to rank == 1</item>
        /// <item>Changes array of arrays to arrays of System.Void.</item>
        /// </list>
        /// Two methods with the same name cannot have the same CLS reduced form.</para>
        /// </summary>
        /// <param name="srcTypeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private TypeArray CLSReduceSignature(TypeArray srcTypeArray)
        {
            int count = srcTypeArray.Count;
            //PTYPESYM * newTypes = STACK_ALLOC(PTYPESYM, count);
            TypeArray newArray = new TypeArray();

            for (int i = 0; i < count; ++i)
            {
                TYPESYM typeSym = srcTypeArray[i];
                if (typeSym.IsPARAMMODSYM)
                {
                    newArray.Add((typeSym as PARAMMODSYM).ParamTypeSym);
                }
                else if (typeSym.IsARRAYSYM)
                {
                    ARRAYSYM arrayType = typeSym as ARRAYSYM;
                    if (arrayType.ElementTypeSym.IsARRAYSYM)
                    {
                        arrayType = Compiler.MainSymbolManager.GetArray(Compiler.MainSymbolManager.VoidSym, 1, null);
                    }
                    else if (arrayType.Rank > 1)
                    {
                        arrayType = Compiler.MainSymbolManager.GetArray(arrayType.ElementTypeSym, 1, null);
                    }
                    newArray.Add(arrayType);
                }
                else
                {
                    newArray.Add(typeSym);
                }
            }

            return Compiler.MainSymbolManager.AllocParams(newArray);
        }

        //------------------------------------------------------------
        // CLSDREC.CheckLinkDemandOnOverride
        //
        /// <summary></summary>
        /// <param name="methSym"></param>
        /// <param name="baseMwt"></param>
        //------------------------------------------------------------
        private void CheckLinkDemandOnOverride(METHSYM methSym, MethWithType baseMwt)
        {
            if (methSym == null || baseMwt == null) return; // 2014/12/27 hirano567@hotmail.co.jp

            if (methSym.HasLinkDemand &&
                !baseMwt.MethSym.HasLinkDemand &&
                !baseMwt.MethSym.ClassSym.HasLinkDemand)
            {
                Compiler.ErrorRef(null, CSCERRID.WRN_LinkDemandOnOverride,
                    new ErrArgRef(methSym), new ErrArgRef(baseMwt));
            }
        }

        //------------------------------------------------------------
        // CLSDREC.FindDuplicateConversion
        //
        /// <summary>
        /// finds a duplicate user defined conversion operator
        /// or a regular member hidden by a new conversion operator
        /// </summary>
        /// <param name="conversionsOnly"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="returnType"></param>
        /// <param name="name"></param>
        /// <param name="cls"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private SYM FindDuplicateConversion(
            bool conversionsOnly,
            TypeArray parameterTypes,
            TYPESYM returnType,
            string name,
            AGGSYM cls)
        {
            SYM present = Compiler.MainSymbolManager.LookupAggMember(name, cls, SYMBMASK.ALL);
            while (present != null)
            {
                if ((!conversionsOnly && !present.IsMETHSYM) ||
                    ((present.IsMETHSYM) &&
                     ((present as METHSYM).ParameterTypes == parameterTypes) &&

                     // we have a method which matches name and parameters
                    // we're looking for 2 cases:

                     // 1) any conversion operator with matching return type

                     ((((present as METHSYM).ReturnTypeSym == returnType) &&
                        (present as METHSYM).IsConversionOperator) ||

                     // 2) a non-conversion operator, and we have an exact name match

                       (!(present as METHSYM).IsConversionOperator &&
                        !conversionsOnly))))
                {
                    return present;
                }

                present = BSYMMGR.LookupNextSym(present, present.ParentSym, SYMBMASK.ALL);
            }

            return null;
        }

        //------------------------------------------------------------
        // CLSDREC.DeclareNamespace
        //
        /// <summary>
        /// <para>declare a namespace by entering it into the symbol table,
        /// and recording it in the parsetree...
        /// nspace is the parent namespace.</para>
        /// <para>Parse the node list starting from NSDECLSYM.ParseTreeNode.ElementsNode.</para>
        /// </summary>
        /// <param name="nsdeclSym"></param>
        //------------------------------------------------------------
        private void DeclareNamespace(NSDECLSYM nsdeclSym)
        {
            // declare members

            BASENODE node = nsdeclSym.ParseTreeNode.ElementsNode;
            while (node != null)
            {
                BASENODE elementNode;
                if (node.Kind == NODEKIND.LIST)
                {
                    elementNode = node.AsLIST.Operand1.AsBASE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    elementNode = node.AsBASE;
                    node = null;
                }

                NODELOCATION nodeLocation = new NODELOCATION(Compiler.Location, elementNode);

                switch (elementNode.Kind)
                {
                    case NODEKIND.NAMESPACE:
                        NAMESPACENODE nsNode = elementNode as NAMESPACENODE;
                        NSDECLSYM newDeclSym = null;

                        // we can not be the global namespace
                        DebugUtil.Assert(nsNode.NameNode != null);
                        BASENODE nameNode = nsNode.NameNode;

                        // Simple name
                        if (nameNode.Kind == NODEKIND.NAME)
                        {
                            newDeclSym = AddNamespaceDeclaration(
                                nameNode as NAMENODE,
                                nsNode,
                                nsdeclSym);
                        }
                        // Qualified name
                        // Register each qualified identifier
                        else
                        {
                            // get the first component of the dotted name
                            BASENODE first = nameNode;
                            while (nameNode.AsDOT.Operand1.Kind == NODEKIND.DOT)
                            {
                                nameNode = nameNode.AsDOT.Operand1;
                            }
                            // nameNode is the first dot or colon-colon of a qualified name.

                            // add all the namespaces in the dotted name
                            newDeclSym = AddNamespaceDeclaration(
                                nameNode.AsDOT.Operand1 as NAMENODE, nsNode, nsdeclSym);
                            while (newDeclSym != null)
                            {
                                // don't add using clauses for the declaration
                                // to any but the last namespace declaration in the list.
                                newDeclSym.UsingClausesResolved = true;
                                DebugUtil.Assert(nameNode.Kind == NODEKIND.DOT);

                                newDeclSym = AddNamespaceDeclaration(
                                    nameNode.AsDOT.Operand2 as NAMENODE,
                                    nsNode,
                                    newDeclSym);
                                if (newDeclSym == null)
                                {
                                    break;
                                }
                                if (nameNode == first)
                                {
                                    break;
                                }
                                nameNode = nameNode.ParentNode;
                            }
                        }

                        if (newDeclSym != null)
                        {
                            DeclareNamespace(newDeclSym);
                        }
                        break;

                    case NODEKIND.CLASS:
                    case NODEKIND.STRUCT:
                    case NODEKIND.INTERFACE:
                    case NODEKIND.ENUM:
                        DeclareAggregate(elementNode.AsAGGREGATE, nsdeclSym);
                        break;

                    case NODEKIND.DELEGATE:
                        DELEGATENODE delegateNode = elementNode as DELEGATENODE;
                        AddAggregate(delegateNode, delegateNode.NameNode, nsdeclSym);
                        break;

                    default:
                        DebugUtil.Assert(false, "unknown namespace member");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DeclareGlobalAttribute
        //
        /// <summary>
        /// creates a global attribute symbol  for attributes on modules and assemblies
        /// </summary>
        /// <param name="attrDeclNode"></param>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        private void DeclareGlobalAttribute(ATTRDECLNODE attrDeclNode, NSDECLSYM nsDeclSym)
        {
            // create the attribute
            GLOBALATTRSYM attr = Compiler.GlobalSymbolManager.CreateGlobalAttribute(
                attrDeclNode.NameNode.Name,
                nsDeclSym);
            attr.ParseTreeNode = attrDeclNode;

            // check the global attribute's name
            if (attrDeclNode.Target == ATTRTARGET.ASSEMBLY)
            {
                attr.ElementKind = AttributeTargets.Assembly;
                AddGlobalAttribute(
                    attr,
                    ref Compiler.AssemblyAttributes,
                    ref Compiler.LastAssemblyAttributes);
            }
            else if (attrDeclNode.Target == ATTRTARGET.MODULE)
            {
                attr.ElementKind = AttributeTargets.Module;
                OUTFILESYM outfileSym = nsDeclSym.GetInputFile().GetOutFileSym();
                AddGlobalAttribute(
                    attr,
                    ref outfileSym.AttributesSym,
                    ref outfileSym.LastAttributesSym);
            }
            else
            {
                DebugUtil.Assert(attrDeclNode.Target == ATTRTARGET.UNKNOWN);

                // System.AttributeTargets has FlagsAttribute.
                attr.ElementKind = (AttributeTargets)0;
                AddGlobalAttribute(
                    attr,
                    ref Compiler.UnknownGlobalAttributes,
                    ref Compiler.LastUnknownGlobalAttributes);
            }

            nsDeclSym.GetInputFile().HasGlobalAttr = true;
        }

        //------------------------------------------------------------
        // CLSDREC.AddGlobalAttribute
        //
        /// <summary>
        /// Add a new GLOBALATTRSYM instance to the specified list.
        /// </summary>
        /// <param name="newAttr"></param>
        /// <param name="firstAttr"></param>
        /// <param name="lastAttr"></param>
        //------------------------------------------------------------
        private void AddGlobalAttribute(
            GLOBALATTRSYM newAttr,
            ref GLOBALATTRSYM firstAttr,
            ref GLOBALATTRSYM lastAttr)
        {
            if (newAttr == null)
            {
                return;
            }

            if (firstAttr == null)
            {
                firstAttr = newAttr;
                lastAttr = newAttr;
            }
            else
            {
                DebugUtil.Assert(lastAttr != null);
                lastAttr.NextAttributeSym = newAttr;
                lastAttr = newAttr;
            }
            newAttr.NextAttributeSym = null;
        }

        //------------------------------------------------------------
        // CLSDREC.DeclareAggregate
        //
        /// <summary>
        /// <para>Declares a class or struct.</para>
        /// <para>This means that this aggregate and any contained aggregates
        /// have their symbols entered in the symbol table.</para> 
        /// <para>The access modifier on the type is also checked,
        /// and the type variables get created (in AddAggregate).</para>
        /// </summary>
        /// <param name="aggregateNode"></param>
        /// <param name="parentDeclSym"></param>
        //------------------------------------------------------------
        private void DeclareAggregate(AGGREGATENODE aggregateNode, DECLSYM parentDeclSym)
        {
            DebugUtil.Assert(parentDeclSym.IsAGGDECLSYM || parentDeclSym.IsNSDECLSYM);
            AGGDECLSYM aggDeclSym = AddAggregate(aggregateNode, aggregateNode.NameNode, parentDeclSym);

            if (aggDeclSym != null)
            {
                AGGSYM aggSym = aggDeclSym.AggSym;

                // declare all nested types
                // parser guarantees that enums don't contain nested types

                if (!aggSym.IsEnum && !aggSym.IsDelegate)
                {
                    for (
                        MEMBERNODE memberNode = aggregateNode.MembersNode;
                        memberNode != null;
                        memberNode = memberNode.NextMemberNode)
                    {
                        Compiler.SetLocation(memberNode);  //SETLOCATIONNODE(memberNode);
                        NESTEDTYPENODE nestedTypeNode = memberNode as NESTEDTYPENODE;
                        if (nestedTypeNode == null)
                        {
                            continue;
                        }

                        if (!aggSym.IsInterface)
                        {
                            switch (nestedTypeNode.TypeNode.Kind)
                            {
                                case NODEKIND.CLASS:
                                case NODEKIND.STRUCT:
                                case NODEKIND.INTERFACE:
                                case NODEKIND.ENUM:
                                    DeclareAggregate(nestedTypeNode.TypeNode.AsAGGREGATE, aggDeclSym);
                                    break;

                                case NODEKIND.DELEGATE:
                                    DELEGATENODE delegateNode = nestedTypeNode.TypeNode as DELEGATENODE;
                                    DebugUtil.Assert(delegateNode != null);
                                    AddAggregate(delegateNode, delegateNode.NameNode, aggDeclSym);
                                    break;

                                default:
                                    DebugUtil.Assert(false, "Unknown aggregate type");
                                    break;
                            }
                        }
                        else
                        {
                            NAMENODE name;

                            if ((memberNode as NESTEDTYPENODE).TypeNode.Kind == NODEKIND.DELEGATE)
                            {
                                name = ((memberNode as NESTEDTYPENODE).TypeNode as DELEGATENODE).NameNode;
                            }
                            else
                            {
                                name = (memberNode as NESTEDTYPENODE).TypeNode.AsAGGREGATE.NameNode;
                            }
                            compiler.Error(name, CSCERRID.ERR_InterfacesCannotContainTypes, new ErrArg(name));
                        }
                    }
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.ensureBasesAreResolved
        //------------------------------------------------------------
        //private void ensureBasesAreResolved(AGGSYM cls);

        //------------------------------------------------------------
        // CLSDREC.DefineAggregateMembers
        //
        /// <summary>
        /// define a types members, does not do base classes
        /// and implemented interfaces
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void DefineAggregateMembers(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.AggState >= AggStateEnum.DefiningMembers);

            //--------------------------------------------------
            // define nested types, interfaces will have no members at this point
            //--------------------------------------------------
            for (SYM sym = aggSym.FirstChildSym; sym != null; sym = sym.NextSym)
            {
                Compiler.SetLocation(sym); //SETLOCATIONSYM(sym);

                // should only have types at this point
                switch (sym.Kind)
                {
                    case SYMKIND.AGGSYM:
                        DebugUtil.Assert(
                            (sym as AGGSYM).AggKind > AggKindEnum.Unknown &&
                            (sym as AGGSYM).AggKind < AggKindEnum.Lim);
                        DefineAggregate(sym as AGGSYM);
                        break;

                    case SYMKIND.AGGTYPESYM:
                    case SYMKIND.TYVARSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown member");
                        break;
                }
            }

            //--------------------------------------------------
            // define members
            // We never generate a non-static constructor for structs...
            //--------------------------------------------------
            bool seenConstructor = aggSym.IsStruct;
            bool needStaticConstructor = false;
            bool mustMatch = false;

            // Iterate all members of all declarations.
            for (AGGDECLSYM aggDeclSym = aggSym.FirstDeclSym;
                aggDeclSym != null;
                aggDeclSym = aggDeclSym.NextDeclSym)
            {
                MEMBERNODE memberNode = aggDeclSym.ParseTreeNode.AsAGGREGATE.MembersNode;
                while (memberNode != null)
                {
                    Compiler.SetLocation(memberNode);  //SETLOCATIONNODE(memberNode);

                    switch (memberNode.Kind)
                    {
                        case NODEKIND.DTOR:
                        case NODEKIND.METHOD:
                            DefineMethod(memberNode.AsANYMETHOD, aggSym, aggDeclSym);
                            break;

                        case NODEKIND.CTOR:
                            METHSYM methodSym = DefineMethod(memberNode as CTORMETHODNODE, aggSym, aggDeclSym);
                            if (methodSym != null)
                            {
                                if (methodSym.IsStatic)
                                {
                                    if (!aggSym.IsInterface)
                                    {
                                        aggSym.HasUserDefinedStaticCtor = true;
                                    }
                                }
                                else
                                {
                                    seenConstructor = true;
                                    if (methodSym.ParameterTypes.Count == 0)
                                    {
                                        aggSym.HasNoArgCtor = true;
                                        if (methodSym.Access == ACCESS.PUBLIC)
                                        {
                                            aggSym.HasPubNoArgCtor = true;
                                        }
                                    }
                                }
                            }
                            break;

                        case NODEKIND.OPERATOR:
                            mustMatch |= DefineOperator(memberNode as OPERATORMETHODNODE, aggSym, aggDeclSym);
                            break;

                        case NODEKIND.FIELD:
                            needStaticConstructor |= DefineFields(memberNode as FIELDNODE, aggSym, aggDeclSym);
                            break;

                        case NODEKIND.NESTEDTYPE:
                            // seperate loop for nested types
                            break;

                        case NODEKIND.PROPERTY:
                            DefineProperty(memberNode as PROPERTYNODE, aggSym, aggDeclSym);
                            break;

                        case NODEKIND.INDEXER:
                            DefineProperty(memberNode.AsINDEXER, aggSym, aggDeclSym);
                            break;

                        case NODEKIND.CONST:
                            needStaticConstructor |= DefineFields(memberNode.AsCONST, aggSym, aggDeclSym);
                            break;

                        case NODEKIND.PARTIALMEMBER:
                            // Ignore this codesense artifact...
                            break;

                        default:
                            DebugUtil.Assert(false, "Unknown node type");
                            break;
                    }
                    memberNode = memberNode.NextMemberNode;
                }
            }

            METHSYM equalsSym = null;
            METHSYM getHashCodeSym = null;
            if (!aggSym.IsInterface)
            {
                //----------------------------------------------------
                // find public override bool Equals(object)
                //----------------------------------------------------
                equalsSym = Compiler.MainSymbolManager.LookupAggMember(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.EQUALS),
                    aggSym,
                    SYMBMASK.METHSYM) as METHSYM;

                for (;
                    equalsSym != null;
                    equalsSym = BSYMMGR.LookupNextSym(equalsSym, aggSym, SYMBMASK.METHSYM) as METHSYM)
                {
                    if (equalsSym.IsOverride &&
                        equalsSym.Access == ACCESS.PUBLIC &&
                        equalsSym.ParameterTypes.Count == 1 &&
                        equalsSym.ParameterTypes[0].IsPredefType(PREDEFTYPE.OBJECT) &&
                        equalsSym.ReturnTypeSym.IsPredefType(PREDEFTYPE.BOOL))
                    {
                        break;
                    }
                }

                //----------------------------------------------------
                // find public override int GetHashCode()
                //----------------------------------------------------
                getHashCodeSym = Compiler.MainSymbolManager.LookupAggMember(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.GETHASHCODE),
                    aggSym,
                    SYMBMASK.METHSYM) as METHSYM;

                for (;
                    getHashCodeSym != null;
                    getHashCodeSym = BSYMMGR.LookupNextSym(getHashCodeSym, aggSym, SYMBMASK.METHSYM) as METHSYM)
                {
                    if (getHashCodeSym.IsOverride &&
                        getHashCodeSym.Access == ACCESS.PUBLIC &&
                        getHashCodeSym.ParameterTypes.Count == 0 &&
                        getHashCodeSym.ReturnTypeSym.IsPredefType(PREDEFTYPE.INT))
                    {
                        break;
                    }
                }

                if (equalsSym != null && getHashCodeSym == null)
                {
                    Compiler.ErrorRef(null, CSCERRID.WRN_EqualsWithoutGetHashCode, new ErrArgRef(aggSym));
                }
            }

            if (mustMatch)
            {
                CheckMatchingOperator(PREDEFNAME.OPEQUALITY, aggSym);
                CheckMatchingOperator(PREDEFNAME.OPINEQUALITY, aggSym);
                CheckMatchingOperator(PREDEFNAME.OPGREATERTHAN, aggSym);
                CheckMatchingOperator(PREDEFNAME.OPLESSTHAN, aggSym);
                CheckMatchingOperator(PREDEFNAME.OPGREATERTHANOREQUAL, aggSym);
                CheckMatchingOperator(PREDEFNAME.OPLESSTHANOREQUAL, aggSym);
                CheckMatchingOperator(PREDEFNAME.OPTRUE, aggSym);
                CheckMatchingOperator(PREDEFNAME.OPFALSE, aggSym);

                if (!aggSym.IsInterface && (equalsSym == null || getHashCodeSym == null))
                {
                    bool foundEqualityOp = false;
                    for (METHSYM opSym = Compiler.MainSymbolManager.LookupAggMember(
                            Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPEQUALITY),
                            aggSym,
                            SYMBMASK.METHSYM) as METHSYM;
                        opSym != null;
                        opSym = BSYMMGR.LookupNextSym(opSym, aggSym, SYMBMASK.METHSYM) as METHSYM)
                    {
                        if (opSym.IsOperator)
                        {
                            foundEqualityOp = true;
                            break;
                        }
                    }

                    if (!foundEqualityOp)
                    {
                        for (METHSYM opSym = Compiler.MainSymbolManager.LookupAggMember(
                                Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPINEQUALITY),
                                aggSym,
                                SYMBMASK.METHSYM) as METHSYM;
                            opSym != null;
                            opSym = BSYMMGR.LookupNextSym(opSym, aggSym, SYMBMASK.METHSYM) as METHSYM)
                        {
                            if (opSym.IsOperator)
                            {
                                foundEqualityOp = true;
                                break;
                            }
                        }
                    }

                    if (foundEqualityOp)
                    {
                        if (equalsSym == null)
                        {
                            Compiler.ErrorRef(null, CSCERRID.WRN_EqualityOpWithoutEquals, new ErrArgRef(aggSym));
                        }
                        if (getHashCodeSym == null)
                        {
                            Compiler.ErrorRef(null, CSCERRID.WRN_EqualityOpWithoutGetHashCode, new ErrArgRef(aggSym));
                        }
                    }
                }
            }

            //--------------------------------------------------
            // Synthetize the static and non static default constructors if necessary...
            //--------------------------------------------------
            if (!aggSym.IsInterface)
            {
                if (!aggSym.HasUserDefinedStaticCtor && needStaticConstructor)
                {
                    SynthesizeConstructor(aggSym, true);
                }
                if (!seenConstructor && !aggSym.IsStatic)
                {
                    SynthesizeConstructor(aggSym, false);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineEnumMembers
        //
        /// <summary>
        /// define all the enumerators in an enum type
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void DefineEnumMembers(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.IsEnum);
            // we do NOT need default & copy constructors

            // Enums can not be defined in multiple places, so DeclOnly is OK here.
            AGGDECLSYM aggDeclSym = aggSym.DeclOnly();

            //--------------------------------------------------
            // define members
            //--------------------------------------------------
            MEMBVARSYM previousEnumerator = null;
            for (ENUMMBRNODE memberNode = aggDeclSym.ParseTreeNode.AsAGGREGATE.MembersNode as ENUMMBRNODE;
                 memberNode != null;
                 memberNode = memberNode.NextNode as ENUMMBRNODE)
            {
                Compiler.SetLocation(memberNode);   //SETLOCATIONNODE(memberNode);

                NAMENODE nameNode = memberNode.NameNode;
                string name = nameNode.Name;

                // check for name same as that of parent aggregate
                // check for conflicting enumerator name
                if (!CheckForBadMemberSimple(name, nameNode, aggSym, null))
                {
                    continue;
                }

                // Check for same names as the reserved "value" enumerators.
                if (name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.ENUMVALUE))
                {
                    Compiler.Error(nameNode, CSCERRID.ERR_ReservedEnumerator, new ErrArg(nameNode));
                    continue;
                }

                //----------------------------------------
                // create the symbol
                //----------------------------------------
                MEMBVARSYM memberSym = Compiler.MainSymbolManager.CreateMembVar(name, aggSym, aggDeclSym);

                memberSym.TypeSym = aggSym.GetThisType();
                memberSym.Access = ACCESS.PUBLIC;

                // set link to previous enumerator

                memberSym.SetPreviousEnumerator(previousEnumerator);
                previousEnumerator = memberSym;

                // set the value parse tree:

                memberSym.ParseTreeNode = memberNode;
                memberSym.ContainingAggDeclSym = aggDeclSym;

                // set the flags to indicate an unevaluated constant

                memberSym.IsUnevaled = true;
                memberSym.IsStatic = true; // consts are implicitly static.
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineDelegateMembers
        //
        /// <summary>
        /// <para>define all the members in a delegate type
        /// <list type="bullet">
        /// <item>.ctor</item>
        /// <item>Invoke</item>
        /// <item>BeginInvoke</item>
        /// <item>EndInvoke</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void DefineDelegateMembers(AGGSYM aggSym)
        {
            // A delegate can be defined in only one place, so DeclOnly is OK here.
            AGGDECLSYM aggDeclSym = aggSym.DeclOnly();
            TypeArray typeArray = null;

            //--------------------------------------------------------
            // constructor taking an object and an uintptr
            //--------------------------------------------------------
            METHSYM ctorSym = Compiler.MainSymbolManager.CreateMethod(
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR),
                aggSym,
                aggDeclSym);

            ctorSym.Access = ACCESS.PUBLIC;
            ctorSym.MethodKind = MethodKindEnum.Ctor;
            ctorSym.ReturnTypeSym = Compiler.MainSymbolManager.VoidSym;
            ctorSym.ParseTreeNode = aggDeclSym.ParseTreeNode;
            ctorSym.ContainingAggDeclSym = aggDeclSym;

            typeArray = new TypeArray();
            typeArray.Add(Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, false));
            typeArray.Add(Compiler.GetReqPredefType(PREDEFTYPE.INTPTR, false));
            ctorSym.ParameterTypes = Compiler.MainSymbolManager.AllocParams(typeArray);
            ctorSym.TypeVariables = BSYMMGR.EmptyTypeArray;

            BASENODE treeNode = aggDeclSym.ParseTreeNode;

            // Parser should enforce this.
            DebugUtil.Assert((treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_VARARGS) == 0);

            //--------------------------------------------------------
            // 'Invoke' methodSym
            //--------------------------------------------------------
            // bind return type

            DELEGATENODE delegateNode = aggDeclSym.ParseTreeNode as DELEGATENODE;
            bool unsafeContext = ((delegateNode.Flags & NODEFLAGS.MOD_UNSAFE) != 0 || aggDeclSym.IsUnsafe);

            TYPESYM returnTypeSym = TypeBind.BindTypeAggDeclExt(
                Compiler,
                delegateNode.ReturnTypeNode,
                aggDeclSym,
                TypeBindFlagsEnum.None);
            DebugUtil.Assert(returnTypeSym != null);

            if (returnTypeSym.IsSpecialByRefType())
            {
                Compiler.Error(
                    delegateNode.ReturnTypeNode,
                    CSCERRID.ERR_MethodReturnCantBeRefAny,
                    new ErrArg(returnTypeSym));
            }

            CheckUnsafe(delegateNode.ReturnTypeNode, returnTypeSym, unsafeContext, CSCERRID.ERR_UnsafeNeeded);

            // Bind parameter types

            TypeArray parameters = null;
            bool hasParams = ((treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0);
            DefineParameters(
                aggDeclSym,
                delegateNode.ParametersNode,
                unsafeContext,
                ref parameters,
                ref hasParams);

            // Check return and parameter types for correct visibility.
            CheckConstituentVisibility(aggSym, returnTypeSym, CSCERRID.ERR_BadVisDelegateReturn);

            for (int i = 0; i < parameters.Count; ++i)
            {
                CheckConstituentVisibility(aggSym, parameters[i], CSCERRID.ERR_BadVisDelegateParam);

                // (sscli) checkConstituentVisibility(cls, params->Item(0), ERR_BadVisDelegateParam);
                // bug?
            }

            // create virtual public 'Invoke' methodSym

            METHSYM invokeSym = Compiler.MainSymbolManager.CreateMethod(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE),
                    aggSym,
                    aggDeclSym);
            invokeSym.Access = ACCESS.PUBLIC;
            invokeSym.ReturnTypeSym = returnTypeSym;
            invokeSym.IsVirtual = true;
            invokeSym.IsMetadataVirtual = true;
            invokeSym.MethodKind = MethodKindEnum.Invoke;
            invokeSym.ParseTreeNode = aggDeclSym.ParseTreeNode;
            invokeSym.ParameterTypes = parameters;
            invokeSym.IsParameterArray = hasParams;
            invokeSym.TypeVariables = BSYMMGR.EmptyTypeArray;

            //--------------------------------------------------------
            // BeginInvoke and EndInvoke
            //--------------------------------------------------------
            // These two types might not exist on some platforms.
            // If they don't, then don't create a BeginInvoke/EndInvoke.

            TYPESYM asynchResult = Compiler.GetOptPredefType(PREDEFTYPE.IASYNCRESULT, false);
            TYPESYM asynchCallback = Compiler.GetOptPredefType(PREDEFTYPE.ASYNCCBDEL, false);

            if (asynchResult != null && asynchCallback != null)
            {
                //----------------------------------------------------
                // 'BeginInvoke' methodSym
                //----------------------------------------------------
                typeArray = new TypeArray();
                for (int i = 0; i < invokeSym.ParameterTypes.Count; ++i)
                {
                    typeArray.Add(invokeSym.ParameterTypes[i]);
                }
                typeArray.Add(asynchCallback);
                typeArray.Add(Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, false));

                // Create virtual public 'BeginInvoke' methodSym
                METHSYM beginInvokeSym = Compiler.MainSymbolManager.CreateMethod(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.BEGININVOKE),
                    aggSym,
                    aggDeclSym);
                beginInvokeSym.Access = ACCESS.PUBLIC;
                beginInvokeSym.ReturnTypeSym = asynchResult;
                beginInvokeSym.IsVirtual = true;
                beginInvokeSym.ParseTreeNode = aggDeclSym.ParseTreeNode;
                beginInvokeSym.ParameterTypes = Compiler.MainSymbolManager.AllocParams(typeArray);
                beginInvokeSym.TypeVariables = BSYMMGR.EmptyTypeArray;

                //----------------------------------------------------
                // 'EndInvoke' methodSym
                //----------------------------------------------------
                typeArray = new TypeArray();
                for (int i = 0; i < invokeSym.ParameterTypes.Count; ++i)
                {
                    if (invokeSym.ParameterTypes[i].IsPARAMMODSYM)
                    {
                        typeArray.Add(invokeSym.ParameterTypes[i]);
                    }
                }
                typeArray.Add(asynchResult);

                // Create virtual public 'EndInvoke' methodSym
                METHSYM endInvokeSym = Compiler.MainSymbolManager.CreateMethod(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.ENDINVOKE),
                    aggSym,
                    aggDeclSym);
                endInvokeSym.Access = ACCESS.PUBLIC;
                endInvokeSym.ReturnTypeSym = returnTypeSym;
                endInvokeSym.IsVirtual = true;
                endInvokeSym.ParseTreeNode = aggDeclSym.ParseTreeNode;
                endInvokeSym.ParameterTypes = Compiler.MainSymbolManager.AllocParams(typeArray);
                endInvokeSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineProperty
        //
        /// <summary>
        /// defines a property by entering it into the symbol table,
        /// also checks for duplicates, and makes sure that the flags don't conflict.
        /// also adds methods for the property's accessors
        /// </summary>
        /// <param name="propertyNode"></param>
        /// <param name="aggSym"></param>
        /// <param name="aggDeclSym"></param>
        //------------------------------------------------------------
        private void DefineProperty(PROPERTYNODE propertyNode, AGGSYM aggSym, AGGDECLSYM aggDeclSym)
        {
            TypeArray parameterTypes = null;

            BASENODE nameTree = propertyNode.NameNode;
            BASENODE nodeName = nameTree;
            bool isEvent = (propertyNode.NodeFlagsEx & NODEFLAGS.EX_EVENT) != 0;
            // is it really an event?
            bool isIndexer = propertyNode.Kind == NODEKIND.INDEXER;

            //--------------------------------------------------
            // Get the propertySym name
            //--------------------------------------------------
            string name;

            bool isExplicitImpl = (isIndexer ? nameTree != null : nameTree.Kind != NODEKIND.NAME);
            if (!isExplicitImpl)
            {
                if (isIndexer)
                {
                    // name defaults to "Item", may be overwritten later during prepareProperty
                    // when we bind the IndexerNameAttribute
                    name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.INDEXER);
                }
                else
                {
                    name = (nameTree as NAMENODE).Name;
                    parameterTypes = BSYMMGR.EmptyTypeArray;
                }
            }
            else
            {
                DebugUtil.Assert(isIndexer || nameTree.Kind == NODEKIND.DOT);

                // explicit interface implementation
                if (!aggSym.IsClass && !aggSym.IsStruct)
                {
                    Compiler.Error(
                        nameTree,
                        CSCERRID.ERR_ExplicitInterfaceImplementationInNonClassOrStruct,
                        new ErrArgNameNode(nameTree, ErrArgFlagsEnum.None));
                    return;
                }

                // we handle explicit interface implementations in the prepare stage
                // when we have the complete list of interfaces
                name = null;
                parameterTypes = BSYMMGR.EmptyTypeArray;

                if (!isIndexer)
                {
                    nodeName = nameTree.AsDOT.Operand2;
                }
            }

            //--------------------------------------------------
            // get the propertySym's typeSym. It can't be void.
            //--------------------------------------------------
            TYPESYM typeSym = TypeBind.BindType(
                Compiler,
                propertyNode.TypeNode,
                aggDeclSym,
                TypeBindFlagsEnum.None);
            DebugUtil.Assert(typeSym != null);

            if (typeSym.IsVOIDSYM)
            {
                if (isIndexer)
                {
                    Compiler.Error(
                        propertyNode,
                        CSCERRID.ERR_IndexerCantHaveVoidType);
                }
                else
                {
                    if (nameTree != null)
                    {
                        Compiler.Error(
                            nameTree,
                            CSCERRID.ERR_PropertyCantHaveVoidType,
                            new ErrArgNameNode(nameTree, ErrArgFlagsEnum.None));
                    }
                    else
                    {
                        Compiler.Error(
                            propertyNode,
                            CSCERRID.ERR_PropertyCantHaveVoidType,
                            new ErrArg(name));
                    }
                }
            }

            if (typeSym.IsSpecialByRefType())
            {
                Compiler.Error(
                    propertyNode.TypeNode,
                    CSCERRID.ERR_FieldCantBeRefAny,
                    new ErrArg(typeSym));
            }

            bool isUnsafe = (
                (propertyNode.Flags & NODEFLAGS.MOD_UNSAFE) != 0 ||
                aggDeclSym.IsUnsafe);
            bool hasParams;

            // define indexer parameters
            if (isIndexer)
            {
                hasParams = ((propertyNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0);
                DefineParameters(
                    aggDeclSym,
                    propertyNode.ParametersNode,
                    isUnsafe,
                    ref parameterTypes,
                    ref hasParams);
                CheckForBadMember(
                    name != null ?
                        Compiler.NameManager.GetPredefinedName(PREDEFNAME.INDEXERINTERNAL) :
                        null,
                    SYMKIND.PROPSYM,
                    parameterTypes,
                    propertyNode,
                    aggSym,
                    null,
                    null);
            }
            else
            {
                hasParams = false;
                // all we need to check is whether or not the name is valid
                // (no parameters / typeSym vars).
                CheckForBadMemberSimple(name, nameTree, aggSym, null);
            }

            //--------------------------------------------------
            // create the symbol for the propertySym
            //--------------------------------------------------
            PROPSYM propertySym = null;
            PROPSYM fakePropSym = new PROPSYM();
            if (isEvent)
            {
                // A new-style event does not declare an underlying propertySym. In order to keep
                // the rest of this function simpler, we declare a "fake" propertySym on the stack
                // and use it.
                //MEM_SET_ZERO(fakePropSym);
                propertySym = fakePropSym;
                propertySym.SetName(name);
                propertySym.ParentSym = aggSym;
                propertySym.ContainingAggDeclSym = aggDeclSym;
                propertySym.Kind = SYMKIND.PROPSYM;
            }
            else
            {
                if (isIndexer)
                {
                    propertySym = Compiler.MainSymbolManager.CreateIndexer(
                        name,
                        aggSym,
                        aggDeclSym);
                }
                else
                {
                    propertySym = Compiler.MainSymbolManager.CreateProperty(
                        name,
                        aggSym,
                        aggDeclSym);
                }
                DebugUtil.Assert(isIndexer || nodeName != null && nodeName.IsAnyName);
            }
            DebugUtil.Assert(name != null || propertySym.GetRealName() == null);
            propertySym.ParseTreeNode = propertyNode;
            propertySym.ReturnTypeSym = typeSym;

            // indexers should have at least one paramter
            propertySym.ParameterTypes = parameterTypes;
            propertySym.IsParameterArray = hasParams;

            // indexers are the only propertySym kind that can have param arrays
            DebugUtil.Assert(!propertySym.IsParameterArray || isIndexer);
            propertySym.IsUnsafe = isUnsafe;

            CheckUnsafe(
                propertySym.ParseTreeNode,
                propertySym.ReturnTypeSym,
                propertySym.IsUnsafe,
                CSCERRID.ERR_UnsafeNeeded);

            if ((propertyNode.Flags & NODEFLAGS.MOD_STATIC) != 0)
            {
                propertySym.IsStatic = true;
            }
            propertySym.IsOverride = ((propertyNode.Flags & NODEFLAGS.MOD_OVERRIDE) != 0);
            if ((propertyNode.Flags & NODEFLAGS.MOD_SEALED) != 0 && !propertySym.IsOverride)
            {
                Compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_SealedNonOverride,
                    new ErrArgRef(propertySym));
            }

            //--------------------------------------------------
            // check the flags. Assigns access.
            //--------------------------------------------------
            NODEFLAGS allowableFlags;
            if (propertySym.Name != null && aggSym.IsClass)
            {
                allowableFlags =
                    NODEFLAGS.MOD_ABSTRACT |
                    NODEFLAGS.MOD_EXTERN |
                    NODEFLAGS.MOD_VIRTUAL |
                    NODEFLAGS.MOD_OVERRIDE |
                    NODEFLAGS.MOD_SEALED |
                    NODEFLAGS.MOD_UNSAFE;
            }
            else
            {
                // interface member or explicit interface impl
                allowableFlags = NODEFLAGS.MOD_UNSAFE;
            }
            if (name != null && (aggSym.IsClass || aggSym.IsStruct))
            {
                allowableFlags |= (
                    aggSym.AllowableMemberAccess() |
                    NODEFLAGS.MOD_NEW |
                    NODEFLAGS.MOD_UNSAFE);

                // indexers can't be static
                if (!isIndexer)
                {
                    allowableFlags |= NODEFLAGS.MOD_STATIC;
                }
            }
            else if (aggSym.IsInterface)
            {
                // interface members 
                // have no allowable flags
                allowableFlags |= NODEFLAGS.MOD_NEW;
            }
            else
            {
                // explicit interface impls
                DebugUtil.Assert(name == null);
                allowableFlags |= NODEFLAGS.MOD_UNSAFE | NODEFLAGS.MOD_EXTERN;
            }
            CheckFlags(propertySym, allowableFlags, propertyNode.Flags);
            propertySym.Access = GetAccessFromFlags(aggSym, allowableFlags, propertyNode.Flags);

            // Check return and parameter types for correct visibility.
            CheckConstituentVisibility(
                propertySym,
                typeSym,
                isIndexer ? CSCERRID.ERR_BadVisIndexerReturn : CSCERRID.ERR_BadVisPropertyType);

            for (int i = 0; i < parameterTypes.Count; ++i)
            {
                CheckConstituentVisibility(
                    propertySym,
                    parameterTypes[i],
                    CSCERRID.ERR_BadVisIndexerParam);
            }

            if (aggSym.IsStatic)
            {
                if (isIndexer)
                {
                    //Indexers are not allowed in static classes
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_IndexerInStaticClass,
                        new ErrArgRef(propertySym));
                }
                else if ((propertyNode.Flags & NODEFLAGS.MOD_STATIC) == 0)
                {
                    //Instance properties not allowed in static classes
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_InstanceMemberInStaticClass,
                        new ErrArgRef(propertySym));
                }
            }

            if (isEvent)
            {
                // events must have both accessors
                if (propertyNode.GetNode == null || propertyNode.SetNode == null)
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_EventNeedsBothAccessors,
                        new ErrArgRef(propertySym));
                }

                // If this is an event propertySym, define the corresponding event symbol.
                EVENTSYM eventSym = DefineEvent(propertySym, propertyNode);
                eventSym.EventImplementSym = null; // remove link to "fake" propertySym symbol.
                DebugUtil.Assert(nodeName != null && nodeName.IsAnyName);
            }
            else
            {
                // must have at least one accessor
                if (propertyNode.GetNode == null && propertyNode.SetNode == null)
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_PropertyWithNoAccessors,
                        new ErrArgRef(propertySym));
                }

                // disallow private virtual properties
                if (((propertyNode.Flags &
                        (NODEFLAGS.MOD_VIRTUAL | NODEFLAGS.MOD_ABSTRACT | NODEFLAGS.MOD_OVERRIDE)) != 0) &&
                    propertySym.Access == ACCESS.PRIVATE)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_VirtualPrivate, new ErrArgRef(propertySym));
                }
            }
            DebugUtil.Assert(propertySym.ParameterTypes != null);

            // CS3
            // If this property is auto-implemented, create the back field.
            if ((aggSym.AggKind == AggKindEnum.Class || aggSym.AggKind == AggKindEnum.Struct) &&
                !aggSym.IsAbstract)
            {
                if (IsValidAutoImplementedProperty(propertyNode))
                {
                    propertySym.BackFieldSym = CreateBackField(
                        propertyNode,
                        name,
                        typeSym,
                        aggSym,
                        aggDeclSym);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineEvent
        //
        /// <summary>
        /// define an event by entering it into the symbol table.
        /// the underlying implementation of the event, a field or property, has already been defined,
        /// and we do not duplicate checks performed there. We do check that the event
        /// is of a delegate type. The access modifier on the implementation is changed to 
        /// private and the event inherits the access modifier of the underlying field. The event
        /// accessor methods are defined.
        /// </summary>
        /// <param name="implementingSym"></param>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EVENTSYM DefineEvent(SYM implementingSym, BASENODE treeNode)
        {
            EVENTSYM eventSym = null;
            bool isField;
            bool isStatic;
            AGGSYM aggSym;
            AGGDECLSYM aggDeclSym;

            DebugUtil.Assert(implementingSym.IsPROPSYM || implementingSym.IsMEMBVARSYM);
            bool isPropertyEvent = (treeNode.Kind == NODEKIND.PROPERTY);

            // An event must be implemented by a property or field symbol.
            DebugUtil.Assert(implementingSym.IsPROPSYM || implementingSym.IsMEMBVARSYM);

            isField = (implementingSym.IsMEMBVARSYM);
            DebugUtil.Assert((isField && !isPropertyEvent) || (isPropertyEvent && !isField));
            isStatic = isField ? (implementingSym as MEMBVARSYM).IsStatic : (implementingSym as PROPSYM).IsStatic;

            // Create the event symbol.
            aggSym = implementingSym.ParentSym as AGGSYM;
            aggDeclSym = implementingSym.ContainingDeclaration() as AGGDECLSYM;
            eventSym = Compiler.MainSymbolManager.CreateEvent(implementingSym.Name, aggSym, aggDeclSym);
            eventSym.EventImplementSym = implementingSym;
            eventSym.IsStatic = isStatic;
            eventSym.IsUnsafe = implementingSym.IsUnsafe();
            eventSym.ParseTreeNode = treeNode;
            if (isField)
            {
                eventSym.TypeSym = (implementingSym as MEMBVARSYM).TypeSym;
                (implementingSym as MEMBVARSYM).IsEvent = true;
            }
            else
            {
                eventSym.TypeSym = (implementingSym as PROPSYM).ReturnTypeSym;
                (implementingSym as PROPSYM).IsEvent = true;
            }

            // The event and accessors get the access modifiers of the implementing symbol;
            // the implementing symbol becomes private.
            eventSym.Access = implementingSym.Access;
            implementingSym.Access = ACCESS.PRIVATE;

            eventSym.IsOverride = ((eventSym.GetParseFlags() & NODEFLAGS.MOD_OVERRIDE) != 0);

            if ((eventSym.GetParseFlags() & (NODEFLAGS.MOD_VIRTUAL | NODEFLAGS.MOD_ABSTRACT | NODEFLAGS.MOD_OVERRIDE)) != 0 &&
                eventSym.Access == ACCESS.PRIVATE)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_VirtualPrivate, new ErrArgRef(eventSym));
            }

            return eventSym;
        }

        //------------------------------------------------------------
        // CLSDREC.CreateAccessorName
        //
        /// <summary>
        /// creates an internal name for user defined property accessor methods
        /// just prepends the name with "Get" or "Set"
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAccessorName(string propertyName, string prefix)
        {
            if (propertyName != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(propertyName);
                string accName = sb.ToString();
                Compiler.NameManager.AddString(accName);
                return accName;
            }
            else
            {
                // explicit interface implementation accessors have no name
                // just like the properties they are contained on.
                return null;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckParamsType
        //
        /// <summary>
        /// <para>Check a "params" style signature to make sure the last argument is of correct type.</para>
        /// <para>typeSym must be of an array of rank 1.</para>
        /// <para>If typeSym is with "ref" or "out" modifiers, remove it.</para>
        /// </summary>
        //------------------------------------------------------------
        private bool CheckParamsType(BASENODE tree, ref TYPESYM typeSym)
        {
            DebugUtil.Assert(typeSym != null);

            if (typeSym.IsPARAMMODSYM)
            {
                Compiler.Error(tree, CSCERRID.ERR_ParamsCantBeRefOut);
                typeSym = (typeSym as PARAMMODSYM).ParamTypeSym;
            }
            if (!typeSym.IsARRAYSYM || (typeSym as ARRAYSYM).Rank != 1)
            {
                Compiler.Error(tree, CSCERRID.ERR_ParamsMustBeArray);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineMethod
        //
        /// <summary>
        /// defines a method by entering it into the symbol table,
        /// also checks for duplicates, and makes sure that the flags don't conflict.
        /// </summary>
        /// <param name="methodTreeNode"></param>
        /// <param name="aggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM DefineMethod(METHODBASENODE methodTreeNode, AGGSYM aggSym, AGGDECLSYM aggDeclSym)
        {
            DebugUtil.Assert(aggSym.IsSource);

            string name;
            NAMENODE methodNameTree = null;

            bool isStaticCtor = false;

            //--------------------------------------------------
            // figure out the name... (1) Constructor
            //--------------------------------------------------
            if (methodTreeNode.Kind == NODEKIND.CTOR)
            {
                if (aggSym.IsInterface)
                {
                    Compiler.Error(methodTreeNode, CSCERRID.ERR_InterfacesCantContainConstructors);
                    return null;
                }

                if (aggSym.IsStatic && ((methodTreeNode.Flags & NODEFLAGS.MOD_STATIC) == 0))
                {
                    // Static class must contain a static constructor
                    Compiler.Error(methodTreeNode, CSCERRID.ERR_ConstructorInStaticClass);
                }

                if ((methodTreeNode.Flags & NODEFLAGS.MOD_STATIC) != 0)
                {
                    name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.STATCTOR);
                    isStaticCtor = true;
                }
                else
                {
                    name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR);
                }
            }
            //--------------------------------------------------
            // figure out the name... (2) Destructor
            //--------------------------------------------------
            else if (methodTreeNode.Kind == NODEKIND.DTOR)
            {
                if (!aggSym.IsClass)
                {
                    Compiler.Error(methodTreeNode, CSCERRID.ERR_OnlyClassesCanContainDestructors);
                    return null;
                }

                if (aggSym.IsStatic)
                {
                    Compiler.Error(methodTreeNode, CSCERRID.ERR_DestructorInStaticClass);
                }

                name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.DTOR);
            }
            //--------------------------------------------------
            // figure out the name... (3) Otherwise
            //--------------------------------------------------
            else
            {
                BASENODE nameTree = (methodTreeNode as METHODNODE).NameNode;

                if (aggSym.IsStatic && ((methodTreeNode.Flags & NODEFLAGS.MOD_STATIC) == 0))
                {
                    // Methods in a static class must be declared static.
                    Compiler.Error(
                        nameTree,
                        CSCERRID.ERR_InstanceMemberInStaticClass,
                        new ErrArgNameNode(nameTree, ErrArgFlagsEnum.None));
                }

                if (nameTree.IsAnyName)
                {
                    name = nameTree.AsANYNAME.Name;
                    methodNameTree = nameTree.AsANYNAME;
                }
                else
                {
                    DebugUtil.Assert(nameTree.Kind == NODEKIND.DOT);
                    methodNameTree = nameTree.AsDOT.Operand2.AsANYNAME;

                    if (!aggSym.IsClass && !aggSym.IsStruct)
                    {
                        Compiler.Error(
                            nameTree,
                            CSCERRID.ERR_ExplicitInterfaceImplementationInNonClassOrStruct,
                            new ErrArgNameNode(nameTree, ErrArgFlagsEnum.None));
                        return null;
                    }

                    // we handle explicit interface implementations in the prepare stage
                    // when we have the complete list of interfaces
                    name = null;
                }
            }

            //--------------------------------------------------
            // Create a METHSYM instance
            //--------------------------------------------------
            METHSYM methodSym = Compiler.MainSymbolManager.CreateMethod(name, aggSym, aggDeclSym);
            methodSym.ParseTreeNode = methodTreeNode;

            // (CS3) If it is a partial method, set some flags.
            SetForPartialMethod(methodTreeNode, methodSym);

            //--------------------------------------------------
            // GENERICS: declare type parameters - these may be used in the types below.
            //--------------------------------------------------
            DefineMethodTypeVars(
                methodSym,
                (methodNameTree != null && methodNameTree.Kind == NODEKIND.GENERICNAME)
                ? (methodNameTree as GENERICNAMENODE).ParametersNode : null);

            if (methodTreeNode.Kind == NODEKIND.METHOD)
            {
                DefineBounds((methodTreeNode as METHODNODE).ConstraintsNode, methodSym, true);
            }

            methodSym.IsUnsafe = (
                ((methodTreeNode.Flags & NODEFLAGS.MOD_UNSAFE) != 0) ||
                methodSym.ContainingDeclaration().IsUnsafe);

            //--------------------------------------------------
            // bind all the parameters and do some error checking
            //--------------------------------------------------
            if ((methodSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_VARARGS) != 0)
            {
                // Old style varargs. Not allowed in generics or with params.
                if (((methodSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0) ||
                    methodSym.TypeVariables.Count > 0 || aggSym.AllTypeVariables.Count > 0)
                {
                    Compiler.Error(methodSym.ParseTreeNode, CSCERRID.ERR_BadVarargs);
                }
                else
                {
                    methodSym.IsVarargs = true;
                }
            }

            bool hasParams = ((methodSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_PARAMS) != 0);
            DefineParameters(
                methodSym,
                methodTreeNode.ParametersNode,
                methodSym.IsUnsafe,
                ref methodSym.ParameterTypes,
                ref hasParams);
            methodSym.IsParameterArray = hasParams;

            if (methodSym.IsVarargs)
            {
                // Tack on the extra type.
                methodSym.ParameterTypes = Compiler.MainSymbolManager.ConcatParams(
                    methodSym.ParameterTypes,
                    Compiler.MainSymbolManager.ArgListSym);
            }

            //--------------------------------------------------
            // bind the return type, or assume void for constructors...
            //--------------------------------------------------
            TYPESYM returnTypeSym;
            if (methodTreeNode.ReturnTypeNode != null)
            {
                returnTypeSym = TypeBind.BindType(
                    Compiler,
                    methodTreeNode.ReturnTypeNode,
                    methodSym,
                    TypeBindFlagsEnum.None);
                DebugUtil.Assert(returnTypeSym != null);

                CheckUnsafe(
                    methodTreeNode.ReturnTypeNode,
                    returnTypeSym,
                    methodSym.IsUnsafe,
                    CSCERRID.ERR_UnsafeNeeded);

                if (returnTypeSym.IsSpecialByRefType() && !aggSym.GetThisType().IsSpecialByRefType())
                {
                    // Allow return type for themselves
                    Compiler.Error(
                        methodTreeNode.ReturnTypeNode,
                        CSCERRID.ERR_MethodReturnCantBeRefAny,
                        new ErrArg(returnTypeSym));
                }

                if (methodSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.DTOR) &&
                    methodSym.ParameterTypes.Count == 0 &&
                    methodSym.TypeVariables.Count == 0 &&
                    returnTypeSym.IsVOIDSYM)
                {
                    Compiler.Error(methodTreeNode, CSCERRID.WRN_FinalizeMethod);
                }
            }
            else // if (methodTreeNode.ReturnTypeNode != null)
            {
                DebugUtil.Assert(
                    methodTreeNode.Kind == NODEKIND.CTOR ||
                    methodTreeNode.Kind == NODEKIND.DTOR);
                returnTypeSym = Compiler.MainSymbolManager.VoidSym;
            }

            if (name != null)
            {
                if (aggSym.IsStruct &&
                    (methodTreeNode.Kind == NODEKIND.CTOR) &&
                    (methodSym.ParameterTypes.Count == 0) &&
                    !isStaticCtor)
                {
                    Compiler.Error(methodTreeNode, CSCERRID.ERR_StructsCantContainDefaultContructor);
                }
                else
                {
                    // for non-explicit interface implementations
                    // find another method with the same signature
                    CheckForBadMember(
                        name,
                        SYMKIND.METHSYM,
                        methodSym.ParameterTypes,
                        methodTreeNode,
                        aggSym,
                        methodSym.TypeVariables,
                        methodSym);
                }
            }
            else
            {
                // method is an explicit interface implementation
            }

            methodSym.ReturnTypeSym = returnTypeSym;

            if ((methodTreeNode.Flags & NODEFLAGS.MOD_STATIC) != 0)
            {
                methodSym.IsStatic = true;
            }

            //--------------------------------------------------
            // constructors have a much simpler set of allowed flags:
            //--------------------------------------------------
            if (methodTreeNode.Kind == NODEKIND.CTOR)
            {
                methodSym.MethodKind = MethodKindEnum.Ctor;

                // NOTE:
                //
                // NODEFLAGS.CTOR_BASE && NODEFLAGS.CTOR_THIS have the same value as
                // NODEFLAGS.MOD_ABSTRACT && NODEFLAGS.MOD_NEW we mask them out so that
                // we don't get spurious conflicts with NODEFLAGS.MOD_STATIC

                CheckFlags(
                    methodSym,
                    aggSym.AllowableMemberAccess() |
                    NODEFLAGS.MOD_STATIC |
                    NODEFLAGS.MOD_EXTERN |
                    NODEFLAGS.MOD_UNSAFE,
                    methodTreeNode.Flags);
                methodSym.Access = GetAccessFromFlags(aggSym, aggSym.AllowableMemberAccess(), methodTreeNode.Flags);

                if (methodSym.IsStatic)
                {
                    // static constructors can't have explicit constructor calls,
                    // access modifiers or parameters
                    if ((methodTreeNode.NodeFlagsEx & (NODEFLAGS.EX_CTOR_BASE | NODEFLAGS.EX_CTOR_THIS)) != 0)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_StaticConstructorWithExplicitConstructorCall,
                            new ErrArgRef(methodSym));
                    }
                    if ((methodTreeNode.Flags & NODEFLAGS.MOD_ACCESSMODIFIERS) != 0)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_StaticConstructorWithAccessModifiers,
                            new ErrArgRef(methodSym));
                    }
                    if (methodSym.ParameterTypes.Count > 0)
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_StaticConstParam, new ErrArgRef(methodSym));
                    }
                }

                if ((methodTreeNode.Flags & NODEFLAGS.MOD_EXTERN) != 0)
                {
                    methodSym.IsExternal = true;
                }
            }
            else if (methodTreeNode.Kind == NODEKIND.DTOR)
            {
                // no modifiers are allowed on destructors
                CheckFlags(methodSym, NODEFLAGS.MOD_UNSAFE | NODEFLAGS.MOD_EXTERN, methodTreeNode.Flags);

                methodSym.Access = ACCESS.PROTECTED;
                methodSym.MethodKind = MethodKindEnum.Dtor;
                methodSym.IsVirtual = true;
                methodSym.IsMetadataVirtual = true;
                methodSym.IsOverride = true;

                if ((methodTreeNode.Flags & NODEFLAGS.MOD_EXTERN) != 0)
                {
                    methodSym.IsExternal = true;
                }
            }
            else if (name != null)
            {
                // methods a somewhat more complicated one...
                if ((methodTreeNode.Flags & NODEFLAGS.MOD_ABSTRACT) != 0 || aggSym.IsInterface)
                {
                    methodSym.IsAbstract = true;
                }
                if ((methodTreeNode.Flags & NODEFLAGS.MOD_OVERRIDE) != 0)
                {
                    methodSym.IsOverride = true;
                }
                if ((methodTreeNode.Flags & NODEFLAGS.MOD_EXTERN) != 0 && !methodSym.IsAbstract)
                {
                    methodSym.IsExternal = true;
                }
                if ((methodTreeNode.Flags & NODEFLAGS.MOD_VIRTUAL) != 0 ||
                    methodSym.IsOverride ||
                    methodSym.IsAbstract)
                {
                    // abstract implies virtual
                    methodSym.IsVirtual = true;
                    methodSym.IsMetadataVirtual = true;
                }
                if ((methodTreeNode.Flags & NODEFLAGS.MOD_SEALED) != 0)
                {
                    if (!methodSym.IsOverride)
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_SealedNonOverride, new ErrArgRef(methodSym));
                    }
                    else
                    {
                        // Note: a sealed override is marked with isOverride=true, isVirtual=false.
                        methodSym.IsVirtual = false;
                        DebugUtil.Assert(methodSym.IsMetadataVirtual);
                    }
                }

                NODEFLAGS allowableFlags = aggSym.AllowableMemberAccess();
                switch (aggSym.AggKind)
                {
                    case AggKindEnum.Class:
                        allowableFlags |=
                            NODEFLAGS.MOD_ABSTRACT |
                            NODEFLAGS.MOD_VIRTUAL |
                            NODEFLAGS.MOD_NEW |
                            NODEFLAGS.MOD_OVERRIDE |
                            NODEFLAGS.MOD_SEALED |
                            NODEFLAGS.MOD_STATIC |
                            NODEFLAGS.MOD_UNSAFE |
                            NODEFLAGS.MOD_EXTERN;
                        break;

                    case AggKindEnum.Struct:
                        allowableFlags |=
                            NODEFLAGS.MOD_NEW |
                            NODEFLAGS.MOD_OVERRIDE |
                            NODEFLAGS.MOD_STATIC |
                            NODEFLAGS.MOD_UNSAFE |
                            NODEFLAGS.MOD_EXTERN;
                        break;

                    case AggKindEnum.Interface:
                        // interface members can only have new and unsafe
                        allowableFlags |= NODEFLAGS.MOD_NEW | NODEFLAGS.MOD_UNSAFE;
                        break;

                    default:
                        DebugUtil.Assert(false);
                        break;
                }

                CheckFlags(methodSym, allowableFlags, methodTreeNode.Flags);
                methodSym.Access = GetAccessFromFlags(aggSym, allowableFlags, methodTreeNode.Flags);

                if (methodSym.IsVirtual && methodSym.Access == ACCESS.PRIVATE)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_VirtualPrivate, new ErrArgRef(methodSym));
                }
            }
            else // if (methodTreeNode.Kind == NODEKIND.CTOR)
            {
                // explicit interface implementation
                // can't have any flags
                CheckFlags(methodSym, NODEFLAGS.MOD_UNSAFE | NODEFLAGS.MOD_EXTERN, methodTreeNode.Flags);
                methodSym.Access = ACCESS.PRIVATE;
                methodSym.IsMetadataVirtual = true; // explicit impls need to be virtual when emitted
                if ((methodTreeNode.Flags & NODEFLAGS.MOD_EXTERN) != 0)
                {
                    methodSym.IsExternal = true;
                }
            }

            CheckForProtectedInSealed(methodSym);
            DebugUtil.Assert(methodSym.TypeVariables != null && methodSym.ParameterTypes != null);

            // Check return type, parameter types and constraints for correct visibility.
            CheckConstituentVisibility(methodSym, returnTypeSym, CSCERRID.ERR_BadVisReturnType);

            int cParam = methodSym.ParameterTypes.Count;
            if (methodSym.IsVarargs)
            {
                // don't count the sentinel
                cParam--;
            }
            for (int i = 0; i < cParam; ++i)
            {
                CheckConstituentVisibility(methodSym, methodSym.ParameterTypes[i], CSCERRID.ERR_BadVisParamType);
            }

            if (methodSym.TypeVariables.Count > 0)
            {
                CheckBoundsVisibility(methodSym, methodSym.TypeVariables);
            }

            // (CS3) Extension method
            if (methodTreeNode.Kind == NODEKIND.METHOD &&
                (methodTreeNode as METHODNODE).IsExtensionMethod)
            {
                DefineExtensionMethod(
                    methodTreeNode as METHODNODE,
                    methodSym);
            }

            // (CS3) partial method
            if (methodSym.IsPartialMethod)
            {
                CheckFlagsAndSigOfPartialMethod(
                    methodTreeNode,
                    methodTreeNode.Flags,
                    methodSym);
            }

            return methodSym;
        }

        //------------------------------------------------------------
        // CLSDREC.MakeIterator
        //
        /// <summary>
        /// Make the aggregate nested type, the Current, MoveNext, and Reset methods
        /// and the few known fields ($__current, THIS and $__state)
        /// more fields will get added when MoveNext is compiled
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM MakeIterator(METHINFO methodInfo)
        {
            DebugUtil.Assert(methodInfo.IteratorInfo != null);

            // The class we're building up
            AGGSYM iteratorClassSym = null;

            // It's one and only decl (which is the same as it's method)
            AGGDECLSYM iteratorClassDeclSym = null;

            int ifaceCount;
            //AGGTYPESYM * rgtype[5]; // Interface list
            TypeArray ifaceTypeArray = new TypeArray();

            // Converted to use the new classes type parameters
            TypeArray classTypePrameters = null;

            // The 'real' move next method that we'll pass out
            METHSYM moveNextMethodSym = null;

            // Temp used for some fields
            MEMBVARSYM tempFieldSym = null;

            // Temp
            string tempName = null;

            // Force these up front
            AGGTYPESYM ienumerableAts = compiler.GetOptPredefType(PREDEFTYPE.G_IENUMERABLE, true);
            AGGTYPESYM ienumeratorAts = compiler.GetOptPredefType(PREDEFTYPE.G_IENUMERATOR, true);

            if (!methodInfo.MethodSym.ReturnTypeSym.IsAGGTYPESYM ||
                methodInfo.MethodSym.ReturnTypeSym.HasErrors ||
                methodInfo.MethodSym.ParameterTypes.HasErrors)
            {
                return null;
            }

            // figure out what we need to implement
            methodInfo.IteratorInfo.IsGeneric = (ienumerableAts != null && ienumeratorAts != null);
            if (!methodInfo.IteratorInfo.IsGeneric)
            {
                // Clear these in case we got one without the other
                ienumerableAts = null;
                ienumeratorAts = null;
            }

            methodInfo.IteratorInfo.IsEnumerable = (
                ienumerableAts != null &&
                methodInfo.MethodSym.ReturnTypeSym.GetAggregate() == ienumerableAts.GetAggregate() ||
                methodInfo.MethodSym.ReturnTypeSym.IsPredefType(PREDEFTYPE.IENUMERABLE));

            // Get a unique base name (derived from the method name)
            // to use as a prefix for all the generated classes
            string baseName = null;
            if (methodInfo.MethodSym.Name == null)
            {
                //StringBldrNrHeap str(compiler.getLocalSymHeap());
                StringBuilder sb = new StringBuilder();
                MetaDataHelper hlpr = new MetaDataHelper();
                hlpr.GetExplicitImplName(methodInfo.MethodSym, sb);
                baseName = sb.ToString();
            }
            else
            {
                baseName = methodInfo.MethodSym.Name;
            }

            //--------------------------------------------------------
            // Create the class
            //--------------------------------------------------------
            iteratorClassDeclSym = AddSynthAgg(
                baseName,
                SpecialNameKindEnum.IteratorClass,
                methodInfo.MethodSym.ContainingDeclaration());
            iteratorClassSym = iteratorClassDeclSym.AggSym;

            // We also need to add in any possible method TyVars
            CopyMethTyVarsToClass(methodInfo.MethodSym, iteratorClassSym);

            // Get the yield type by substituting method TyVars with class TyVars
            TYPESYM yieldTypeSym = compiler.MainSymbolManager.SubstType(
                methodInfo.YieldTypeSym,
                (TypeArray)null,
                iteratorClassSym.TypeVariables,
                SubstTypeFlagsEnum.NormNone);

            // Use the yield type to create type argument list for IEnumerable<T> and IEnumerator<T>
            classTypePrameters = compiler.MainSymbolManager.AllocParams(yieldTypeSym);

            //--------------------------------------------------------
            // Get the interface list IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
            //--------------------------------------------------------
            if (methodInfo.IteratorInfo.IsEnumerable)
            {
                if (methodInfo.IteratorInfo.IsGeneric)
                {
                    ifaceTypeArray.Add(compiler.MainSymbolManager.SubstType(
                        ienumerableAts,
                        classTypePrameters,
                        (TypeArray)null,
                        SubstTypeFlagsEnum.NormNone) as AGGTYPESYM);
                }
                ifaceTypeArray.Add(compiler.GetReqPredefType(PREDEFTYPE.IENUMERABLE, true));
            }
            if (methodInfo.IteratorInfo.IsGeneric)
            {
                ifaceTypeArray.Add(compiler.MainSymbolManager.SubstType(
                    ienumeratorAts,
                    classTypePrameters,
                    (TypeArray)null,
                    SubstTypeFlagsEnum.NormNone) as AGGTYPESYM);
            }
            ifaceTypeArray.Add(compiler.GetReqPredefType(PREDEFTYPE.IENUMERATOR, true));
            ifaceTypeArray.Add(compiler.GetReqPredefType(PREDEFTYPE.IDISPOSABLE, true));
            ifaceCount = ifaceTypeArray.Count;
            compiler.SetIfaces(iteratorClassSym, ifaceTypeArray);

            //--------------------------------------------------------
            // Now bring the class up to the define state
            //--------------------------------------------------------
            compiler.SetBaseType(iteratorClassSym, compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true));
            DefineAggregate(iteratorClassSym);
            iteratorClassSym.SetBogus(false);
            iteratorClassSym.AggState = AggStateEnum.PreparedMembers;
            methodInfo.IteratorInfo.IteratorAggSym = iteratorClassSym;

            if (methodInfo.IteratorInfo.IsEnumerable)
            {
                if (methodInfo.IteratorInfo.IsGeneric)
                {
                    // IEnumerable<T>.GetEnumerator()
                    FabricateExplicitImplMethod(
                        PREDEFNAME.GETENUMERATOR,
                        ifaceTypeArray[0] as AGGTYPESYM,
                        iteratorClassDeclSym);
                }
                // IEnumerable.GetEnumerator()
                FabricateExplicitImplMethod(
                    PREDEFNAME.GETENUMERATOR,
                    compiler.GetReqPredefType(PREDEFTYPE.IENUMERABLE, true),
                    iteratorClassDeclSym);
            }

            //--------------------------------------------------------
            // IEnumerator.MoveNext (Give it the "MoveNext" name, but then make it an explicit impl
            //--------------------------------------------------------
            moveNextMethodSym = FabricateSimpleMethod(
                PREDEFNAME.MOVENEXT,
                iteratorClassDeclSym,
                compiler.GetReqPredefType(PREDEFTYPE.BOOL, true));

            if (moveNextMethodSym != null)
            {
                METHSYM ifaceMethodSym = compiler.FuncBRec.FindPredefMeth(
                    null,
                    PREDEFNAME.MOVENEXT,
                    ifaceTypeArray[ifaceCount - 2] as AGGTYPESYM,
                    BSYMMGR.EmptyTypeArray,
                    true,
                    MemLookFlagsEnum.None);

                if (ifaceMethodSym != null)
                {
                    moveNextMethodSym.Access = ACCESS.PRIVATE;

                    // Make a public method symbol representing a method of type retType, with a given parent.

                    moveNextMethodSym.SlotSymWithType.Set(ifaceMethodSym, ifaceTypeArray[ifaceCount - 2] as AGGTYPESYM);
                    moveNextMethodSym.NeedsMethodImp = true;
                    moveNextMethodSym.IsMetadataVirtual = true;
                }
            }

            if (methodInfo.IteratorInfo.IsGeneric)
            {
                methodInfo.YieldTypeSym = yieldTypeSym;

                // IEnumerator<T>.Current
                FabricateExplicitImplPropertyRO(
                    PREDEFNAME.CURRENT,
                    ifaceTypeArray[ifaceCount - 3] as AGGTYPESYM,
                    iteratorClassDeclSym);
            }
            else
            {
                DebugUtil.Assert(
                    methodInfo.YieldTypeSym == compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true));
            }

            //--------------------------------------------------------
            // $__current
            //--------------------------------------------------------
            tempName = compiler.NameManager.GetPredefinedName(PREDEFNAME.ITERCURRENT);
            tempFieldSym = compiler.MainSymbolManager.CreateMembVar(
                tempName,
                iteratorClassSym,
                iteratorClassDeclSym);

            tempFieldSym.Access = ACCESS.PRIVATE;
            tempFieldSym.IsReferenced = true;
            tempFieldSym.IsAssigned = true;
            tempFieldSym.IsFabricated = true;
            tempFieldSym.TypeSym = methodInfo.YieldTypeSym;

            //--------------------------------------------------------
            // IEnumerator.Reset (this will always throw)
            //--------------------------------------------------------
            FabricateExplicitImplMethod(
                PREDEFNAME.RESET,
                ifaceTypeArray[ifaceCount - 2] as AGGTYPESYM,
                iteratorClassDeclSym);

            //--------------------------------------------------------
            // Dispose() - explicit impl
            //--------------------------------------------------------
            FabricateExplicitImplMethod(
                PREDEFNAME.DISPOSE,
                ifaceTypeArray[ifaceCount - 1] as AGGTYPESYM,
                iteratorClassDeclSym);

            TYPESYM intAts = compiler.GetReqPredefType(PREDEFTYPE.INT, true);

            //--------------------------------------------------------
            // $__state - field to track state info
            //--------------------------------------------------------
            tempName = compiler.NameManager.GetPredefinedName(PREDEFNAME.ITERSTATE);
            tempFieldSym = compiler.MainSymbolManager.CreateMembVar(
                tempName,
                iteratorClassSym,
                iteratorClassDeclSym);

            tempFieldSym.Access = ACCESS.PRIVATE;
            tempFieldSym.IsReferenced = true;
            tempFieldSym.IsAssigned = true;
            tempFieldSym.IsFabricated = true;
            tempFieldSym.TypeSym = intAts;

            //--------------------------------------------------------
            // IEnumerator.Current properties
            //--------------------------------------------------------
            FabricateExplicitImplPropertyRO(
                PREDEFNAME.CURRENT,
                ifaceTypeArray[ifaceCount - 2] as AGGTYPESYM,
                iteratorClassDeclSym);

            //--------------------------------------------------------
            // add the .ctor
            //--------------------------------------------------------
            METHSYM ctor = FabricateSimpleMethod(
                PREDEFNAME.CTOR,
                iteratorClassDeclSym,
                compiler.MainSymbolManager.VoidSym);

            ctor.MethodKind = MethodKindEnum.Ctor;
            ctor.ParameterTypes = compiler.MainSymbolManager.AllocParams(intAts);

            return moveNextMethodSym;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineFields
        //
        /// <summary>
        /// <para>define the fields in a given field node,
        /// this involves checking that the flags for the decls have been specified correctly,
        /// and actually entering the field into the symbol table...</para>
        /// <para>returns true if any of the fields need initialization in the static constructor:
        /// i.e., are static, non-const, and have an explicit initializer (or are of struct type).</para>
        /// </summary>
        /// <param name="fieldTreeNode"></param>
        /// <param name="aggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool DefineFields(FIELDNODE fieldTreeNode, AGGSYM aggSym, AGGDECLSYM aggDeclSym)
        {
            DebugUtil.Assert(!aggSym.IsEnum);
            bool isEvent = ((fieldTreeNode.NodeFlagsEx & NODEFLAGS.EX_EVENT) != 0);

            if (aggSym.IsInterface && !isEvent)
            {
                Compiler.Error(fieldTreeNode.DeclarationsNode, CSCERRID.ERR_InterfacesCantContainFields);
                return false;
            }

            TYPESYM typeSym = TypeBind.BindType(
                Compiler,
                fieldTreeNode.TypeNode,
                aggDeclSym,
                TypeBindFlagsEnum.None);
            DebugUtil.Assert(typeSym != null);

            if (typeSym.IsSpecialByRefType())
            {
                Compiler.Error(fieldTreeNode.TypeNode, CSCERRID.ERR_FieldCantBeRefAny, new ErrArg(typeSym));
            }
            if (typeSym.IsVOIDSYM)
            {
                Compiler.Error(fieldTreeNode.TypeNode, CSCERRID.ERR_FieldCantHaveVoidType);
            }

            NODEFLAGS flags = fieldTreeNode.Flags;
            bool disallowConst = false;

            if (fieldTreeNode.Kind == NODEKIND.CONST && !typeSym.CanBeConst())
            {
                Compiler.Error(fieldTreeNode, CSCERRID.ERR_BadConstType, new ErrArg(typeSym));
                disallowConst = true;
            }

            // check unsafe on the field so that we give only one erro^r message for all vardecls
            CheckUnsafe(
                fieldTreeNode,
                typeSym,
                (aggDeclSym.IsUnsafe || (flags & NODEFLAGS.MOD_UNSAFE) != 0),
                CSCERRID.ERR_UnsafeNeeded);

            bool isError = true;
            bool needStaticCtor = false;

            MEMBVARSYM fakeFieldSym = null;
            // used only for abstract events

            BASENODE node = fieldTreeNode.DeclarationsNode;
            while (node != null)
            {
                VARDECLNODE varDeclNode;
                if (node.Kind == NODEKIND.LIST)
                {
                    varDeclNode = (node.AsLIST.Operand1) as VARDECLNODE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    varDeclNode = node as VARDECLNODE;
                    node = null;
                }

                if (varDeclNode == null)
                {
                    continue;
                }

                string name = varDeclNode.NameNode.Name;

                // check for name same as that of parent aggregate
                // check for conflicting field...
                CheckForBadMemberSimple(name, varDeclNode.NameNode, aggSym, null);

                MEMBVARSYM fieldSym = null;

                if (aggSym.IsInterface ||
                    (isEvent && (flags & (NODEFLAGS.MOD_ABSTRACT | NODEFLAGS.MOD_EXTERN)) != 0))
                {
                    // An abstract or extern event does not declare an underlying field.
                    // In order to keep the rest of this function simpler,
                    // we declare a "fake" field on the stack and use it.
                    DebugUtil.Assert(isEvent);
                    fieldSym = (fakeFieldSym = new MEMBVARSYM());
                    fieldSym.SetName(name);
                    fieldSym.ParentSym = aggSym;
                    fieldSym.Kind = SYMKIND.MEMBVARSYM;
                    fieldSym.ContainingAggDeclSym = aggDeclSym;
                }
                else
                {
                    // The usual case.
                    fieldSym = Compiler.MainSymbolManager.CreateMembVar(name, aggSym, aggDeclSym);
                }

                bool isFixed = ((varDeclNode.Flags & NODEFLAGS.VARDECL_ARRAY) != 0);

                fieldSym.TypeSym = typeSym;

                // set the value parse tree:
                fieldSym.ParseTreeNode = varDeclNode;
                NODEFLAGS allowableFlags = aggSym.AllowableMemberAccess() | NODEFLAGS.MOD_UNSAFE | NODEFLAGS.MOD_NEW;

                if (aggSym.IsInterface)
                {
                    // event in interface can't have initializer.
                    if (varDeclNode.ArgumentsNode != null)
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_InterfaceEventInitializer, new ErrArgRef(fieldSym));
                    }
                }
                else if (!isFixed)
                {
                    allowableFlags |= NODEFLAGS.MOD_STATIC;
                }

                // abstract event can't have initializer
                if (isEvent && (flags & NODEFLAGS.MOD_ABSTRACT) != 0 && varDeclNode.ArgumentsNode != null)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_AbstractEventInitializer, new ErrArgRef(fieldSym));
                }

                // parser guarantees this
                DebugUtil.Assert(
                    fieldTreeNode.Kind != NODEKIND.CONST ||
                    varDeclNode.ArgumentsNode != null ||
                    Compiler.ErrorCount() > 0);

                if (fieldTreeNode.Kind == NODEKIND.CONST &&
                    varDeclNode.ArgumentsNode != null &&
                    !disallowConst)
                {
                    DebugUtil.Assert(varDeclNode.ArgumentsNode.AsANYBINOP.Operator == OPERATOR.ASSIGN);

                    fieldSym.IsUnevaled = true;
                    fieldSym.IsStatic = true; // consts are implicitly static.

                    allowableFlags &= ~NODEFLAGS.MOD_UNSAFE; // consts can't be marked unsafe.

                    // can't have static modifier on constant delcaration
                    if (isError && ((flags & NODEFLAGS.MOD_STATIC) != 0))
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_StaticConstant, new ErrArgRef(fieldSym));
                    }

                    // constants of decimal type can't be expressed in COM+ as a literal
                    // so we need to initialize them in the static constructor
                    if (fieldSym.TypeSym.IsPredefType(PREDEFTYPE.DECIMAL))
                    {
                        needStaticCtor = true;
                    }
                }
                else if (isEvent)
                {
                    // events can be virtual,
                    // but in interfaces the modifier is implied and can't be specified directly.
                    if (!aggSym.IsInterface)
                    {
                        allowableFlags |=
                            NODEFLAGS.MOD_VIRTUAL |
                            NODEFLAGS.MOD_ABSTRACT |
                            NODEFLAGS.MOD_EXTERN |
                            NODEFLAGS.MOD_OVERRIDE |
                            NODEFLAGS.MOD_SEALED;
                    }
                }
                else if (isFixed)
                {
                    fieldSym.TypeSym = Compiler.MainSymbolManager.GetPtrType(typeSym);
                    fieldSym.IsReferenced = fieldSym.IsAssigned = true;

                    if (!typeSym.IsSimpleType() || typeSym.GetPredefType() == PREDEFTYPE.DECIMAL)
                    {
                        // Fixed sized buffers can only be
                        //   bool, byte, short, int, long, char, sbyte,
                        //   ushort, uint, ulong, single and double
                        // Not IntPtr, UIntPtr, Decimal or any other type
                        Compiler.Error(fieldTreeNode.TypeNode, CSCERRID.ERR_IllegalFixedType, new ErrArg(typeSym));
                        // Just use int.
                        typeSym = Compiler.GetReqPredefType(PREDEFTYPE.INT, true);
                        fieldSym.TypeSym = Compiler.MainSymbolManager.GetPtrType(typeSym);
                    }
                    else if (!aggDeclSym.IsUnsafe && (flags & NODEFLAGS.MOD_UNSAFE) == 0)
                    {
                        DebugUtil.Assert(!typeSym.IsUnsafe()); // all the fixed buffer types are 'safe'
                        // But fixed buffers themselves are unsafe, and thus must be in an unsafe context
                        Compiler.Error(fieldTreeNode, CSCERRID.ERR_UnsafeNeeded);
                    }
                    else if (!aggSym.IsStruct)
                    {
                        // Fixed sized buffers can only be in structs
                        Compiler.ErrorRef(null, CSCERRID.ERR_FixedNotInStruct, new ErrArgRef(fieldSym));
                    }
                    else if (Compiler.GetOptPredefTypeErr(PREDEFTYPE.FIXEDBUFFER, true) != null)
                    {
                        fieldSym.IsUnevaled = true;
                        fieldSym.FixedAggSym = MakeFixedBufferStruct(fieldSym, typeSym);
                    }
                }
                else
                {
                    // events and consts can't be readonly or volatile.
                    allowableFlags |= NODEFLAGS.MOD_READONLY | NODEFLAGS.MOD_VOLATILE;
                }

                fieldSym.IsReadOnly = ((flags & NODEFLAGS.MOD_READONLY) != 0);
                fieldSym.IsUnsafe = (aggDeclSym.IsUnsafe || (flags & NODEFLAGS.MOD_UNSAFE) != 0);

                if ((flags & NODEFLAGS.MOD_ABSTRACT) != 0 && !isEvent)
                {
                    DebugUtil.Assert((allowableFlags & NODEFLAGS.MOD_ABSTRACT) == 0);
                    Compiler.ErrorRef(null, CSCERRID.ERR_AbstractField, new ErrArgRef(fieldSym));
                    flags &= ~NODEFLAGS.MOD_ABSTRACT;
                }
                if (isError)
                {
                    CheckFlags(fieldSym, allowableFlags, flags);
                }

                fieldSym.Access = GetAccessFromFlags(aggSym, allowableFlags, flags);

                if ((flags & NODEFLAGS.MOD_STATIC) != 0)
                {
                    fieldSym.IsStatic = true;

                    // If it had an initializer and isn't a const, 
                    // then we need a static initializer.
                    if (varDeclNode.ArgumentsNode != null && !fieldSym.IsUnevaled)
                    {
                        needStaticCtor = true;
                    }
                }
                else if (!fieldSym.IsStatic && aggSym.IsStruct && varDeclNode.ArgumentsNode != null && !isFixed)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_FieldInitializerInStruct, new ErrArgRef(fieldSym));
                    // Assume it should have been static. We do this for isError checking.
                    fieldSym.IsStatic = true;
                    needStaticCtor = true;
                }

                if ((flags & NODEFLAGS.MOD_VOLATILE) != 0)
                {
                    if (typeSym.IsAGGTYPESYM)
                    {
                        DebugUtil.VsVerify(
                            ResolveInheritanceRec(typeSym.GetAggregate()),
                            "ResolveInheritanceRec failed in defineFields!");
                    }

                    if (!typeSym.CanBeVolatile())
                    {
                        Compiler.Error(CSCERRID.ERR_VolatileStruct,
                            new ErrArg(fieldSym), new ErrArg(typeSym));
                    }
                    else if (fieldSym.IsReadOnly)
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_VolatileAndReadonly, new ErrArgRef(fieldSym));
                    }
                    else
                    {
                        fieldSym.IsVolatile = true;
                    }
                }

                CheckForProtectedInSealed(fieldSym);

                if (isEvent && (flags & NODEFLAGS.MOD_SEALED) != 0 && (flags & NODEFLAGS.MOD_OVERRIDE) == 0)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_SealedNonOverride, new ErrArgRef(fieldSym));
                }

                // Check that the field type is as accessible as the field itself.
                CheckConstituentVisibility(fieldSym, typeSym, CSCERRID.ERR_BadVisFieldType);

                // If this is an event field, define the corresponding event symbol.
                if (isEvent)
                {
                    EVENTSYM eventSym = DefineEvent(fieldSym, varDeclNode);
                    if (aggSym.IsInterface ||
                        (isEvent && (flags & (NODEFLAGS.MOD_ABSTRACT | NODEFLAGS.MOD_EXTERN)) != 0))
                    {
                        eventSym.EventImplementSym = null;
                    }
                }

                if (aggSym.IsStatic && !fieldSym.IsStatic)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_InstanceMemberInStaticClass, new ErrArgRef(fieldSym));
                    fieldSym.IsStatic = true;
                    if (varDeclNode.ArgumentsNode != null)
                    {
                        needStaticCtor = true;
                    }
                }

                // don't want to give duplicate errors on subsequent decls...
                isError = false;
                DebugUtil.Assert(fieldSym.ParseTreeNode.Kind == NODEKIND.VARDECL);
            } // while (node != null)

            return needStaticCtor;
        }

        //------------------------------------------------------------
        // CLSDREC.MakeFixedBufferStruct
        //
        /// <summary></summary>
        /// <param name="fieldMemVarSym"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private AGGSYM MakeFixedBufferStruct(MEMBVARSYM fieldMemVarSym, TYPESYM typeSym)
        {
            AGGDECLSYM aggDeclSym = AddSynthAgg(
                fieldMemVarSym.Name,
                SpecialNameKindEnum.FixedBufferStruct,
                fieldMemVarSym.ContainingDeclaration());
            AGGSYM aggSym = aggDeclSym.AggSym;
            aggSym.AggKind = AggKindEnum.Struct;
            aggSym.IsSealed = true;

            aggSym.Access = ACCESS.PUBLIC;
            // other languages need to get at this guy, so make him as visible as his parent

            aggSym.HasExternReference = true;
            aggSym.IsFixedBufferStruct = true;

            // Now bring the class up to the define state
            Compiler.SetBaseType(aggSym, Compiler.GetReqPredefType(PREDEFTYPE.VALUE, true));
            Compiler.SetIfaces(aggSym, (AGGTYPESYM[])null);
            DefineAggregate(aggSym);
            aggSym.SetBogus(false);
            aggSym.AggState = AggStateEnum.PreparedMembers;

            // FixedElementField - fieldMemVarSym to store the actual underlying array typeSym
            string predefName = Compiler.NameManager.GetPredefinedName(PREDEFNAME.FIXEDELEMENT);
            MEMBVARSYM mvSym = Compiler.MainSymbolManager.CreateMembVar(predefName, aggSym, aggDeclSym);
            mvSym.Access = ACCESS.PUBLIC;
            mvSym.IsReferenced = true;
            mvSym.IsAssigned = true;
            mvSym.IsFabricated = true;
            mvSym.TypeSym = typeSym;

            return aggSym;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineOperator
        //
        /// <summary>
        /// <para>define a user defined conversion operator, this involves checking that the
        /// flags for the decls have be specified correctly, and actually entering the
        /// conversion into the symbol table...</para>
        /// <para>returns whether an operator requiring a matching operator has been seen</para>
        /// </summary>
        /// <param name="operatorNode"></param>
        /// <param name="aggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool DefineOperator(OPERATORMETHODNODE operatorNode, AGGSYM aggSym, AGGDECLSYM aggDeclSym)
        {
            DebugUtil.Assert(!aggSym.IsEnum);

            if (aggSym.IsInterface)
            {
                Compiler.Error(operatorNode, CSCERRID.ERR_InterfacesCantContainOperators);
                return false;
            }

            bool isConversion = false;
            bool isImplicit = false;
            string name;
            string otherName = null;

            switch (operatorNode.Operator)
            {
                case OPERATOR.IMPLICIT:
                    isImplicit = true;
                    isConversion = true;
                    name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPIMPLICITMN);
                    otherName = Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPEXPLICITMN);
                    break;

                case OPERATOR.EXPLICIT:
                    isConversion = true;
                    otherName = Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPIMPLICITMN);
                    name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.OPEXPLICITMN);
                    break;

                case OPERATOR.NONE:
                    // Just use the token string as the name and make it a regular method, not an operator.
                    DebugUtil.Assert(Compiler.ErrorCount() > 0);
                    name = CParser.GetTokenInfo(operatorNode.TokenId).Text;
                    Compiler.NameManager.AddString(name);
                    break;

                default:
                    PREDEFNAME predefName = operatorNames[(int)operatorNode.Operator];
                    DebugUtil.Assert(predefName > 0 && predefName < PREDEFNAME.COUNT);
                    name = Compiler.NameManager.GetPredefinedName(predefName);
                    break;
            }

            AGGTYPESYM thisAts = aggSym.GetThisType();
            bool isUnsafe = (operatorNode.Flags & NODEFLAGS.MOD_UNSAFE) != 0 || aggDeclSym.IsUnsafe;
            bool hasParams = false;   // dummy

            TypeArray paramArray = null;
            DefineParameters(
                aggDeclSym,
                operatorNode.ParametersNode,
                isUnsafe,
                ref paramArray,
                ref hasParams);

            // Get return type.
            TYPESYM returnTypeSym = TypeBind.BindType(
                Compiler,
                operatorNode.ReturnTypeNode,
                aggDeclSym,
                TypeBindFlagsEnum.None);
            DebugUtil.Assert(returnTypeSym != null);

            CheckUnsafe(operatorNode, returnTypeSym, isUnsafe, CSCERRID.ERR_UnsafeNeeded);
            TYPESYM rawReturnTypeSym = StripNubs(returnTypeSym, aggSym);

            bool mustMatch = false;

            int paramCount = paramArray.Count;
            DebugUtil.Assert(
                paramCount == 1 ||
                paramCount == 2 ||
                operatorNode.Operator == OPERATOR.NONE);

            TypeArray rawParamArray = paramArray;
            if (paramCount > 0 && paramArray[0].IsNUBSYM ||
                paramCount > 1 && paramArray[1].IsNUBSYM)
            {
                if (paramCount > 1)
                {
                    rawParamArray = Compiler.MainSymbolManager.AllocParams(
                        StripNubs(paramArray[0], aggSym),
                        StripNubs(paramArray[1], aggSym));
                }
                else
                {
                    rawParamArray = Compiler.MainSymbolManager.AllocParams(
                        StripNubs(paramArray[0], aggSym));
                }
            }

            // check argument restrictions. Note that the parser has
            // already checked the number of arguments is correct.

            switch (operatorNode.Operator)
            {

                // conversions
                case OPERATOR.IMPLICIT:
                case OPERATOR.EXPLICIT:
                    DebugUtil.Assert(paramCount == 1);
                    // Check for the identity conversion here.
                    if (rawReturnTypeSym == thisAts && rawParamArray[0] == thisAts)
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_IdentityConversion);
                    }
                    // Either the source or the destination must be the containing type.
                    else if (rawReturnTypeSym != thisAts && rawParamArray[0] != thisAts)
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_ConversionNotInvolvingContainedType);
                    }
                    isConversion = true;
                    break;

                // unary operators
                case OPERATOR.PREINC:
                case OPERATOR.PREDEC:
                    DebugUtil.Assert(paramCount == 1);
                    // The source must be the containing type or nullable of that type. It's also bad if the source
                    // is nullable and the destination isn't.
                    if (rawParamArray[0] != thisAts || paramArray[0].IsNUBSYM && returnTypeSym == thisAts)
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_BadIncDecsignature);
                    }

                    // The destination must be the containing type, nullable of that type or a derived type.
                    // It's also bad if the destination is nullable and the source isn't.
                    if (rawReturnTypeSym != thisAts && !Compiler.IsBaseType(rawReturnTypeSym, thisAts) ||
                        returnTypeSym.IsNUBSYM && paramArray[0] == thisAts)
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_BadIncDecRetType);
                    }
                    break;

                case OPERATOR.TRUE:
                case OPERATOR.FALSE:
                    DebugUtil.Assert(paramCount == 1);
                    mustMatch = true;
                    if (!returnTypeSym.IsPredefType(PREDEFTYPE.BOOL))
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_OpTFRetType);
                    }
                    // Fall through
                    goto case OPERATOR.LOGNOT;

                case OPERATOR.UPLUS:
                case OPERATOR.NEG:
                case OPERATOR.BITNOT:
                case OPERATOR.LOGNOT:
                    DebugUtil.Assert(paramCount == 1);
                    // The source must be the containing type
                    if (rawParamArray[0] != thisAts)
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_BadUnaryOperatorSignature);
                    }
                    break;

                // binary operators
                case OPERATOR.EQ:
                case OPERATOR.NEQ:
                case OPERATOR.GT:
                case OPERATOR.LT:
                case OPERATOR.GE:
                case OPERATOR.LE:
                    mustMatch = true;
                    // Fall through.
                    goto case OPERATOR.BITOR;

                case OPERATOR.ADD:
                case OPERATOR.SUB:
                case OPERATOR.MUL:
                case OPERATOR.DIV:
                case OPERATOR.MOD:
                case OPERATOR.BITXOR:
                case OPERATOR.BITAND:
                case OPERATOR.BITOR:
                    DebugUtil.Assert(paramCount == 2);
                    // At least one of the parameter types must be the containing type.
                    if (rawParamArray[0] != thisAts && rawParamArray[1] != thisAts)
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_BadBinaryOperatorSignature);
                    }
                    else if (operatorNode.Operator == OPERATOR.EQ && rawParamArray[0] == rawParamArray[1])
                    {
                        aggSym.HasSelfEquality = true;
                    }
                    else if (operatorNode.Operator == OPERATOR.NEQ && rawParamArray[0] == rawParamArray[1])
                    {
                        aggSym.HasSelfNonEquality = true;
                    }

                    break;

                // shift operators
                case OPERATOR.LSHIFT:
                case OPERATOR.RSHIFT:
                    DebugUtil.Assert(paramCount == 2);
                    if (rawParamArray[0] != thisAts || !rawParamArray[1].IsPredefType(PREDEFTYPE.INT))
                    {
                        Compiler.Error(operatorNode, CSCERRID.ERR_BadShiftOperatorSignature);
                    }
                    break;

                case OPERATOR.NONE:
                    break;

                default:
                    DebugUtil.Assert(false, "Unknown operator parse tree");
                    break;
            }

            // Return type of user defined operators can't be VOID
            if (rawReturnTypeSym.IsVoidType)    // == Compiler.MainSymbolManager.VoidSym)
            {
                Compiler.Error(operatorNode, CSCERRID.ERR_OperatorCantReturnVoid);
            }

            // Check for duplicate and get name.
            if (isConversion)
            {
                // For operators we check for conflicts with implicit vs. explicit as well
                // create both implicit and explicit names here because they can conflict
                // with each other.

                // check for name same as that of parent aggregate

                if (name == aggSym.Name)
                {
                    Compiler.Error(operatorNode, CSCERRID.ERR_MemberNameSameAsType,
                        new ErrArg(name), new ErrArgRefOnly(aggSym));
                }

                if (!paramArray.HasErrors && !returnTypeSym.HasErrors)
                {
                    // Check for duplicate conversion and conflicting names.
                    SYM dupSym;
                    if ((dupSym = FindDuplicateConversion(false, paramArray, returnTypeSym, name, aggSym)) != null ||
                        (dupSym = FindDuplicateConversion(true, paramArray, returnTypeSym, otherName, aggSym)) != null)
                    {
                        if ((dupSym.IsMETHSYM) && (dupSym as METHSYM).IsConversionOperator)
                        {
                            Compiler.Error(operatorNode, CSCERRID.ERR_DuplicateConversionInClass,
                                new ErrArg(aggSym), new ErrArgRefOnly(dupSym));
                        }
                        else
                        {
                            Compiler.Error(operatorNode, CSCERRID.ERR_DuplicateNameInClass,
                                new ErrArg(name),
                                new ErrArg(aggSym),
                                new ErrArgRefOnly(dupSym));
                        }
                        return false;
                    }
                }
            }
            else if (!CheckForBadMember(
                name,
                SYMKIND.METHSYM,
                paramArray,
                operatorNode,
                aggSym,
                BSYMMGR.EmptyTypeArray,
                null))
            {
                return false;
            }

            //
            // create the operator
            //
            METHSYM methSym = Compiler.MainSymbolManager.CreateMethod(name, aggSym, aggDeclSym);
            methSym.ParseTreeNode = operatorNode;
            // If iOp is OPERATOR.NONE - indicating an error - just make it a regular method
            methSym.IsOperator = (operatorNode.Operator != OPERATOR.NONE);
            DebugUtil.Assert(paramArray != null && paramArray.Count == paramCount);
            methSym.ParameterTypes = paramArray;
            methSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            methSym.IsUnsafe = isUnsafe;
            methSym.ReturnTypeSym = returnTypeSym;
            methSym.IsStatic = true;
            methSym.IsExternal = ((operatorNode.Flags & NODEFLAGS.MOD_EXTERN) != 0);

            if (isConversion)
            {
                if (isImplicit)
                {
                    methSym.MethodKind = MethodKindEnum.ImplicitConv;
                }
                else
                {
                    methSym.MethodKind = MethodKindEnum.ExplicitConv;
                }

                // Add it to the list of conversions operators.
                methSym.NextConvertMethSym = aggSym.FirstConversionMethSym;
                aggSym.FirstConversionMethSym = methSym;

                // The flag is set if this class or any of its base classes has any conversions.
                aggSym.HasConversion = true;
            }

            // check flags. Conversions must be public.
            CheckFlags(
                methSym,
                NODEFLAGS.MOD_UNSAFE | NODEFLAGS.MOD_PUBLIC | NODEFLAGS.MOD_STATIC | NODEFLAGS.MOD_EXTERN,
                operatorNode.Flags);

            // operators must be explicitly declared public and static
            if (((operatorNode.Flags & (NODEFLAGS.MOD_PUBLIC | NODEFLAGS.MOD_STATIC)) != 0)
                != ((NODEFLAGS.MOD_PUBLIC | NODEFLAGS.MOD_STATIC) != 0))
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_OperatorsMustBeStatic, new ErrArgRef(methSym));
            }

            // Set access -- Conversions must be public.
            methSym.Access = ACCESS.PUBLIC;

            // operators are not allowed on a static class
            if (aggSym.IsStatic)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_OperatorInStaticClass, new ErrArgRef(methSym));
            }

            // Check constituent types
            CheckConstituentVisibility(methSym, returnTypeSym, CSCERRID.ERR_BadVisOpReturn);

            for (int i = 0; i < paramArray.Count; ++i)
            {
                CheckConstituentVisibility(methSym, paramArray[i], CSCERRID.ERR_BadVisOpParam);
            }

            return mustMatch;
        }

        //------------------------------------------------------------
        // CLSDREC.SynthesizeConstructor
        //
        /// <summary>
        /// Create a SYM instance of a constructor.
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="isStatic"></param>
        //------------------------------------------------------------
        private void SynthesizeConstructor(AGGSYM aggSym, bool isStatic)
        {
            // There are synthesize method that can be put in any declaration, so DeclFirst is OK here.
            METHSYM methodSym = Compiler.MainSymbolManager.CreateMethod(
                Compiler.NameManager.GetPredefinedName(isStatic ? PREDEFNAME.STATCTOR : PREDEFNAME.CTOR),
                aggSym,
                aggSym.FirstDeclSym);

            if (isStatic)
            {
                // static ctor is always private
                methodSym.Access = ACCESS.PRIVATE;
            }
            else if (aggSym.IsAbstract)
            {
                // default constructor for abstract classes is protected.
                methodSym.Access = ACCESS.PROTECTED;
            }
            else
            {
                methodSym.Access = ACCESS.PUBLIC;
            }

            methodSym.MethodKind = MethodKindEnum.Ctor;
            methodSym.ReturnTypeSym = Compiler.MainSymbolManager.VoidSym;
            methodSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            methodSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            methodSym.ParseTreeNode = aggSym.FirstDeclSym.ParseTreeNode;
            methodSym.IsStatic = isStatic;

            if (!isStatic)
            {
                aggSym.HasNoArgCtor = true;
                if (methodSym.Access == ACCESS.PUBLIC) aggSym.HasPubNoArgCtor = true;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckMatchingOperator
        //
        /// <summary>
        /// Check that all operators w/ the given name have a match
        /// </summary>
        /// <param name="predefName"></param>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void CheckMatchingOperator(PREDEFNAME predefName, AGGSYM aggSym)
        {
            PREDEFNAME opposite = PREDEFNAME.OPFALSE;	// "op_False", false as operator.
            TOKENID oppToken = TOKENID.FALSE;

            switch (predefName)
            {
                case PREDEFNAME.OPEQUALITY:
                    opposite = PREDEFNAME.OPINEQUALITY;
                    oppToken = TOKENID.NOTEQUAL;
                    break;

                case PREDEFNAME.OPINEQUALITY:
                    opposite = PREDEFNAME.OPEQUALITY;
                    oppToken = TOKENID.EQUALEQUAL;
                    break;

                case PREDEFNAME.OPGREATERTHAN:
                    opposite = PREDEFNAME.OPLESSTHAN;
                    oppToken = TOKENID.LESS;
                    break;

                case PREDEFNAME.OPGREATERTHANOREQUAL:
                    opposite = PREDEFNAME.OPLESSTHANOREQUAL;
                    oppToken = TOKENID.LESSEQUAL;
                    break;

                case PREDEFNAME.OPLESSTHAN:
                    opposite = PREDEFNAME.OPGREATERTHAN;
                    oppToken = TOKENID.GREATER;
                    break;

                case PREDEFNAME.OPLESSTHANOREQUAL:
                    opposite = PREDEFNAME.OPGREATERTHANOREQUAL;
                    oppToken = TOKENID.GREATEREQUAL;
                    break;

                case PREDEFNAME.OPTRUE:
                    opposite = PREDEFNAME.OPFALSE;
                    oppToken = TOKENID.FALSE;
                    break;

                case PREDEFNAME.OPFALSE:
                    opposite = PREDEFNAME.OPTRUE;
                    oppToken = TOKENID.TRUE;
                    break;

                default:
                    DebugUtil.Assert(false, "bad predefName in checkMatchingOperator");
                    break;
            }

            string name = Compiler.NameManager.GetPredefinedName(predefName);
            string oppositeName = Compiler.NameManager.GetPredefinedName(opposite);

            for (METHSYM original = Compiler.MainSymbolManager.LookupAggMember(
                    name, aggSym, SYMBMASK.METHSYM) as METHSYM;
                original != null;
                original = BSYMMGR.LookupNextSym(original, aggSym, SYMBMASK.METHSYM) as METHSYM)
            {
                if (original.IsOperator)
                {
                    for (METHSYM match = Compiler.MainSymbolManager.LookupAggMember(
                            oppositeName, aggSym, SYMBMASK.METHSYM) as METHSYM;
                        match != null;
                        match = BSYMMGR.LookupNextSym(match, aggSym, SYMBMASK.METHSYM) as METHSYM)
                    {
                        if (match.IsOperator && match.TypeVariables.Count == original.TypeVariables.Count &&
                            Compiler.MainSymbolManager.SubstEqualTypes(
                                original.ReturnTypeSym,
                                match.ReturnTypeSym,
                                (TypeArray)null,
                                original.TypeVariables,
                                SubstTypeFlagsEnum.NormNone) &&
                            Compiler.MainSymbolManager.SubstEqualTypeArrays(
                                original.ParameterTypes,
                                match.ParameterTypes,
                                (TypeArray)null,
                                original.TypeVariables,
                                SubstTypeFlagsEnum.NormNone))
                        {
                            goto MATCHED;
                        }
                    }

                    string operatorName = CParser.GetTokenInfo(oppToken).Text;
                    DebugUtil.Assert(operatorName[0] != 0);
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_OperatorNeedsMatch,
                        new ErrArgRef(original),
                        new ErrArgRef(operatorName));
                    return;
                }

            MATCHED: ;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.StripNubs (1)
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        /// <param name="contextAggSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private TYPESYM StripNubs(TYPESYM typeSym, AGGSYM contextAggSym)
        {
            if (!typeSym.IsNUBSYM)
            {
                return typeSym;
            }

            if (!contextAggSym.IsPredefAgg(PREDEFTYPE.G_OPTIONAL))
            {
                return typeSym.StripNubs();
            }

            // Just convert to an AGGTYPESYM.
            return (typeSym as NUBSYM).GetAggTypeSym();
        }

        //------------------------------------------------------------
        // CLSDREC.StripNubs (2)
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        /// <param name="contextAggSym"></param>
        /// <param name="stripCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private TYPESYM StripNubs(TYPESYM typeSym, AGGSYM contextAggSym, out int stripCount)
        {
            stripCount = 0;
            if (!typeSym.IsNUBSYM)
            {
                return typeSym;
            }

            if (!contextAggSym.IsPredefAgg(PREDEFTYPE.G_OPTIONAL))
            {
                return typeSym.StripNubs(out stripCount);
            }

            // Just convert to an AGGTYPESYM.
            return (typeSym as NUBSYM).GetAggTypeSym();
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForProtectedInSealed
        //
        /// <summary>
        /// If "sealed" modifier is invalid, show an error message.
        /// </summary>
        /// <param name="memberSym"></param>
        //------------------------------------------------------------
        private void CheckForProtectedInSealed(SYM memberSym)
        {
            if ((memberSym.Access == ACCESS.PROTECTED || memberSym.Access == ACCESS.INTERNALPROTECTED)
                && memberSym.ParentSym.IsAGGSYM
                && !memberSym.IsOverride())
            {
                AGGSYM aggSym = memberSym.ParentSym as AGGSYM;
                if (aggSym.IsSealed)
                {
                    CSCERRID id;
                    if (aggSym.IsStruct)
                    {
                        id = CSCERRID.ERR_ProtectedInStruct;
                    }
                    else if (aggSym.IsStatic)
                    {
                        id = CSCERRID.ERR_ProtectedInStatic;
                    }
                    else
                    {
                        id = CSCERRID.WRN_ProtectedInSealed;
                    }
                    Compiler.ErrorRef(null, id, new ErrArgRef(memberSym));
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.AddAggregate
        //
        /// <summary>
        /// <para>Add an aggregate symbol to the given parent.
        /// Checks for collisions and displays an error if so.
        /// Does NOT declare nested types, or other aggregate members.</para>
        /// </summary>
        /// <param name="aggregateNode">either a CLASSNODE or a DELEGATENODE</param>
        /// <param name="nameNode">the NAMENODE of the aggregate to be declared</param>
        /// <param name="parentDeclSym">the containing declaration for the new aggregate
        /// (an NSDECL or an AGGDECL)</param>
        /// <returns></returns>
        //------------------------------------------------------------
        private AGGDECLSYM AddAggregate(
            BASENODE aggregateNode,
            NAMENODE nameNode,
            DECLSYM parentDeclSym)
        {
            DebugUtil.Assert(
                aggregateNode.Kind == NODEKIND.CLASS ||
                aggregateNode.Kind == NODEKIND.DELEGATE ||
                aggregateNode.Kind == NODEKIND.STRUCT ||
                aggregateNode.Kind == NODEKIND.INTERFACE ||
                aggregateNode.Kind == NODEKIND.ENUM);
            DebugUtil.Assert(parentDeclSym.IsAGGDECLSYM || parentDeclSym.IsNSDECLSYM);
            DebugUtil.Assert(parentDeclSym.GetAssemblyID() == Kaid.ThisAssembly);

            string ident = nameNode.Name;
            BAGSYM parentBagSym = parentDeclSym.BagSym;
            AGGSYM aggSym = null;
            AGGDECLSYM aggDeclSym = null;

            //--------------------------------------------------
            // Get the nodes of the type variables.
            //--------------------------------------------------
            BASENODE typeParamsNode;

            switch (aggregateNode.Kind)
            {
                case NODEKIND.CLASS:
                case NODEKIND.STRUCT:
                case NODEKIND.INTERFACE:
                    typeParamsNode = aggregateNode.AsAGGREGATE.TypeParametersNode;
                    break;

                case NODEKIND.DELEGATE:
                    typeParamsNode = (aggregateNode as DELEGATENODE).TypeParametersNode;
                    break;

                default:
                    typeParamsNode = null;
                    break;
            }

            //--------------------------------------------------
            // Count the type variables.
            //--------------------------------------------------
            int typeParamsCount = 0;
            BASENODE tpNode = typeParamsNode;
            while (tpNode != null)
            {
                if (tpNode.Kind == NODEKIND.LIST)
                {
                    tpNode = tpNode.AsLIST.Operand2;
                }
                else
                {
                    tpNode = null;
                }
                ++typeParamsCount;
            }

            //--------------------------------------------------
            // Determine the new AggKind.
            //--------------------------------------------------
            AggKindEnum newAggKind;

            switch (aggregateNode.Kind)
            {
                case NODEKIND.CLASS:
                    newAggKind = AggKindEnum.Class;
                    break;

                case NODEKIND.STRUCT:
                    newAggKind = AggKindEnum.Struct;
                    break;

                case NODEKIND.INTERFACE:
                    newAggKind = AggKindEnum.Interface;
                    break;

                case NODEKIND.ENUM:
                    newAggKind = AggKindEnum.Enum;
                    break;

                case NODEKIND.DELEGATE:
                    newAggKind = AggKindEnum.Delegate;
                    break;

                default:
                    DebugUtil.Assert(false, "Unrecognized aggregate parse node");
                    return null;
            }

            //--------------------------------------------------
            // Get any conflicting bagSym. (1)
            //
            // Search syms with the same name and the same parent
            // in MainSymbolManager.
            //--------------------------------------------------
            bool isMulti = false;
            AGGSYM aggSymExisting = null;
            NSSYM nsSymExisting = null;
            SYM sym = null;
            BAGSYM bagSym = null;
            SYMBMASK mask = typeParamsCount > 0 ? SYMBMASK.AGGSYM : SYMBMASK.AGGSYM | SYMBMASK.NSSYM;

            // Search syms which has the same name, same parent, same assembly id.
            bagSym = Compiler.LookupInBagAid(
                ident,
                parentBagSym,
                typeParamsCount,
                Kaid.ThisAssembly,
                mask) as BAGSYM;
            while (bagSym != null)
            {
                // Try to match up the kind as well.
                if (bagSym.IsNSSYM)
                {
                    // If a namespace symbol matches, exit loop.
                    DebugUtil.Assert(nsSymExisting == null);
                    nsSymExisting = bagSym as NSSYM;
                }
                // If bagSym is not NSSYM, bagSym is AGGSYM.
                // The number of type parameters must be same.
                else if ((bagSym as AGGSYM).TypeVariables.Count == typeParamsCount)
                {
                    // If a aggregate symbol matche and has type parameters of the same number,
                    if ((bagSym as AGGSYM).AggKind == newAggKind)
                    {
                        // If it is the same kind, exit loop.
                        aggSymExisting = bagSym as AGGSYM;
                        break;
                    }
                    else if (aggSymExisting == null)
                    {
                        // If it is not the same kind, save it.
                        aggSymExisting = bagSym as AGGSYM;
                    }
                }
                bagSym = Compiler.LookupNextInAid(bagSym, Kaid.ThisAssembly, mask) as BAGSYM;
            }

            //--------------------------------------------------
            // Get any conflicting bagSym. (2-1)
            //
            // If the parent bag is not a namespace, (aggSym is or will be nested)
            //--------------------------------------------------
            if (parentBagSym.IsAGGSYM)
            {
                // Nested.
                if (aggSymExisting != null)
                {
                    // Have an existing aggsym with same name and same or conflicting arity.
                    // Might be OK if both are declared partial.
                    DebugUtil.Assert(aggSymExisting.HasParseTree);
                    isMulti = true;
                }

                // Check for same name as parent.
                if (ident == parentBagSym.Name)
                {
                    Compiler.Error(
                        nameNode,
                        CSCERRID.ERR_MemberNameSameAsType,
                        new ErrArg(ident),
                        new ErrArgRefOnly(parentBagSym));
                }

                // There shouldn't be other non-agg members yet.
                SYM otherSym = Compiler.MainSymbolManager.LookupAggMember(
                    ident,
                    parentBagSym as AGGSYM,
                    SYMBMASK.ALL & ~SYMBMASK.AGGSYM);
                DebugUtil.Assert(otherSym == null || otherSym.IsTYVARSYM);

                if (otherSym != null && otherSym.IsTYVARSYM)
                {
                    Compiler.Error(
                        nameNode,
                        CSCERRID.ERR_DuplicateNameInClass,
                        new ErrArg(ident),
                        new ErrArg(parentBagSym),
                        new ErrArgRefOnly(parentBagSym));
                }
            }
            //--------------------------------------------------
            // Get any conflicting bagSym. (2-2)
            //
            // If the parent bag is a namespace
            // and the same aggSym has already registered,
            //--------------------------------------------------
            else if (aggSymExisting != null)
            {
                // We have multiple declarations, might be OK if all declared partial.
                // We haven't imported any added modules yet.
                DebugUtil.Assert(aggSymExisting.HasParseTree);
                isMulti = true;
            }
            //--------------------------------------------------
            // Get any conflicting bagSym. (2-3)
            //
            // Conflicting with a namespce,
            //--------------------------------------------------
            else if (nsSymExisting != null)
            {
                // class is clashing with namespace.
                DebugUtil.Assert(nsSymExisting.FirstDeclSym.InFileSym.IsSource);
                Compiler.Error(
                    nameNode,
                    CSCERRID.ERR_DuplicateNameInNS,
                    new ErrArg(nameNode),
                    new ErrArg(parentBagSym),
                    new ErrArgRefOnly(nsSymExisting.FirstDeclSym));
            }

            //--------------------------------------------------
            // Get any conflicting bagSym. (2-4)
            //
            // This is the second or later declaration. We already have the AGGSYM,
            // just create a new declaration, after verifying that both are marked partial.
            // First look through the old decl's for at least one marked partial.
            //--------------------------------------------------
            if (isMulti)
            {
                AGGDECLSYM oldDeclSym = aggSymExisting.FirstDeclSym;
                if (!oldDeclSym.IsPartial)
                {
                    // See if any decl's are partial, so we don't keep giving errors if only the
                    // first is not marked partial.
                    for (AGGDECLSYM adsT = oldDeclSym; (adsT = adsT.NextDeclSym) != null; )
                    {
                        if (adsT.IsPartial)
                        {
                            oldDeclSym = adsT;
                            break;
                        }
                    }
                }

                bool isPartialOk = (
                    aggregateNode.Kind == NODEKIND.CLASS ||
                    aggregateNode.Kind == NODEKIND.STRUCT ||
                    aggregateNode.Kind == NODEKIND.INTERFACE);
                bool isPartial = isPartialOk && (aggregateNode.Flags & NODEFLAGS.MOD_PARTIAL) != 0;

                // Merge the types if possible.
                if (newAggKind != aggSymExisting.AggKind || !isPartialOk)
                {
                    if (isPartial && oldDeclSym.IsPartial)
                    {
                        // If both are marked partial, assume the user got confused on the kind.
                        Compiler.ErrorRef(
                            nameNode,
                            CSCERRID.ERR_PartialTypeKindConflict,
                            new ErrArgRef(aggSym));
                    }
                    else
                    {
                        Compiler.Error(
                            nameNode,
                            parentBagSym.IsAGGSYM ? CSCERRID.ERR_DuplicateNameInClass : CSCERRID.ERR_DuplicateNameInNS,
                            new ErrArg(nameNode),
                            new ErrArg(parentBagSym),
                            new ErrArgRefOnly(oldDeclSym));
                    }
                    isMulti = false;

                    // goto LCreateNew; (sscli)
                    // Show error message, and not stop but continue processing.
                    // In C#, cannot go to LCreateNew, process here.

                    // create new aggregate and it's declaration.
                    aggSym = Compiler.MainSymbolManager.CreateAgg(ident, parentDeclSym);
                    aggDeclSym = Compiler.MainSymbolManager.CreateAggDecl(aggSym, parentDeclSym);
                    aggSym.IsArityInName = (typeParamsCount > 0);
                    aggSym.AggKind = newAggKind;
                    goto CREATE_AGGSYM_END;
                }

                if (!isPartial && !oldDeclSym.IsPartial)
                {
                    // Neither marked partial: duplicate name error, then treat as partial.
                    Compiler.Error(
                        nameNode,
                        parentBagSym.IsAGGSYM ? CSCERRID.ERR_DuplicateNameInClass : CSCERRID.ERR_DuplicateNameInNS,
                        new ErrArg(nameNode),
                        new ErrArg(parentBagSym),
                        new ErrArgRefOnly(oldDeclSym));
                }
                else if (!isPartial)
                {
                    // New declaration should have been marked partial.
                    Compiler.ErrorRef(
                        nameNode,
                        CSCERRID.ERR_MissingPartial,
                        new ErrArgRef(aggSym));
                }
                else if (!oldDeclSym.IsPartial)
                {
                    // Existing declaration should have been marked partial.
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_MissingPartial,
                        new ErrArgRef(aggSym),
                        new ErrArgRefOnly(nameNode));
                }

                // both new and old declarations are partial. Add the new declaration to the old symbol.
                aggSym = aggSymExisting;
                aggDeclSym = Compiler.MainSymbolManager.CreateAggDecl(aggSym, parentDeclSym);

                DebugUtil.Assert(aggSym.AggState == AggStateEnum.Declared);
                DebugUtil.Assert(aggSym.AggKind == newAggKind);
            } // if (isMulti)
            //--------------------------------------------------
            // If not, create a new aGGSYM.
            //--------------------------------------------------
            else
            {
                //LCreateNew:

                // create new aggregate and it's declaration.
                aggSym = Compiler.MainSymbolManager.CreateAgg(ident, parentDeclSym);
                aggDeclSym = Compiler.MainSymbolManager.CreateAggDecl(aggSym, parentDeclSym);
                aggSym.IsArityInName = (typeParamsCount > 0);
                aggSym.AggKind = newAggKind;
            }
        CREATE_AGGSYM_END: ;

            // Make sure the aggSym and aggDeclSym are hooked together right,
            // and aggDeclSym is the last declaration.
            DebugUtil.Assert(aggDeclSym.AggSym == aggSym);
            DebugUtil.Assert(aggDeclSym.NextDeclSym == null);

            aggDeclSym.ParseTreeNode = aggregateNode;
            aggSym.HasParseTree = true;

            //--------------------------------------------------
            // check modifiers, set flags
            //--------------------------------------------------
            NODEFLAGS allowableFlags = NODEFLAGS.MOD_PUBLIC | NODEFLAGS.MOD_INTERNAL;

            switch (aggregateNode.Kind)
            {
                case NODEKIND.CLASS:
                    DebugUtil.Assert(aggSym.AggKind == AggKindEnum.Class);
                    allowableFlags |= (
                        NODEFLAGS.MOD_SEALED |
                        NODEFLAGS.MOD_ABSTRACT |
                        NODEFLAGS.MOD_UNSAFE |
                        NODEFLAGS.MOD_PARTIAL |
                        NODEFLAGS.MOD_STATIC
                        );

                    // Sealed, abstract and static are additive.
                    // Abstract can only be specified if neither of the other two flags are.
                    DebugUtil.Assert(isMulti || !aggSym.IsAbstract && !aggSym.IsSealed);

                    if ((aggregateNode.Flags & (NODEFLAGS.MOD_STATIC | NODEFLAGS.MOD_ABSTRACT | NODEFLAGS.MOD_SEALED)) != 0)
                    {
                        NODEFLAGS flags = 0;
                        for (AGGDECLSYM decl = aggSym.FirstDeclSym; decl != null; decl = decl.NextDeclSym)
                        {
                            flags |= decl.ParseTreeNode.Flags;
                        }

                        if ((flags & NODEFLAGS.MOD_ABSTRACT) != 0 &&
                            (flags & (NODEFLAGS.MOD_SEALED | NODEFLAGS.MOD_STATIC)) != 0)
                        {
                            Compiler.ErrorRef(
                                null,
                                CSCERRID.ERR_AbstractSealedStatic,
                                new ErrArgRef(aggSym));
                        }
                        if ((flags & NODEFLAGS.MOD_STATIC) != 0 && (flags & NODEFLAGS.MOD_SEALED) != 0)
                        {
                            Compiler.ErrorRef(
                                null,
                                CSCERRID.ERR_SealedStaticClass,
                                new ErrArgRef(aggSym));
                        }
                        if ((flags & NODEFLAGS.MOD_STATIC) != 0)
                        {
                            // static classes are represented as sealed abstract classes
                            aggSym.IsAbstract = true;
                            aggSym.IsSealed = true;
                        }
                        if ((flags & NODEFLAGS.MOD_ABSTRACT) != 0)
                        {
                            aggSym.IsAbstract = true;
                        }
                        if ((flags & NODEFLAGS.MOD_SEALED) != 0)
                        {
                            aggSym.IsSealed = true;
                        }
                    }
                    break;

                case NODEKIND.STRUCT:
                    DebugUtil.Assert(aggSym.AggKind == AggKindEnum.Struct);
                    DebugUtil.Assert(!aggSym.IsAbstract);
                    aggSym.IsSealed = true;
                    allowableFlags |= NODEFLAGS.MOD_UNSAFE | NODEFLAGS.MOD_PARTIAL;
                    break;

                case NODEKIND.INTERFACE:
                    DebugUtil.Assert(aggSym.AggKind == AggKindEnum.Interface);
                    DebugUtil.Assert(!aggSym.IsSealed);
                    aggSym.IsAbstract = true;
                    allowableFlags |= NODEFLAGS.MOD_UNSAFE | NODEFLAGS.MOD_PARTIAL;
                    break;

                case NODEKIND.ENUM:
                    DebugUtil.Assert(aggSym.AggKind == AggKindEnum.Enum);
                    DebugUtil.Assert(!isMulti);
                    aggSym.IsSealed = true;
                    DebugUtil.Assert(!aggSym.IsAbstract);
                    break;

                case NODEKIND.DELEGATE:
                    DebugUtil.Assert(aggSym.AggKind == AggKindEnum.Delegate);
                    DebugUtil.Assert(!isMulti);
                    aggSym.IsSealed = true;
                    DebugUtil.Assert(!aggSym.IsAbstract);
                    allowableFlags |= NODEFLAGS.MOD_UNSAFE;
                    break;

                default:
                    DebugUtil.Assert(false, "Unrecognized aggregate parse node");
                    return null;
            }

            //--------------------------------------------------
            // Nested in a aggregate type
            //--------------------------------------------------
            if (parentBagSym.IsAGGSYM)
            {
                // nested classes can have private access
                // classes in a namespace can only have public or assembly access
                //
                // also nested classes can be marked new
                allowableFlags |= (NODEFLAGS.MOD_NEW | NODEFLAGS.MOD_PRIVATE);

                // only class members can be protected
                if ((parentBagSym as AGGSYM).IsClass)
                {
                    allowableFlags |= NODEFLAGS.MOD_PROTECTED;
                }
            }

            //--------------------------------------------------
            // partial
            //--------------------------------------------------
            if (isMulti)
            {
                // Check consistency of accessibility.
                // If a partial declaration has no accessibility modifier, it means same to othes.
                NODEFLAGS flags = aggregateNode.Flags & allowableFlags & NODEFLAGS.MOD_ACCESSMODIFIERS;
                if (flags != 0)
                {
                    NODEFLAGS flagsPrev = 0;

                    for (AGGDECLSYM decl = aggSym.FirstDeclSym; decl != null; decl = decl.NextDeclSym)
                    {
                        if (decl == aggDeclSym) continue;

                        NODEFLAGS flagsDecl = decl.ParseTreeNode.Flags & allowableFlags & NODEFLAGS.MOD_ACCESSMODIFIERS;
                        if (flagsDecl != 0)
                        {
                            if ((flagsPrev != 0) && (flagsPrev != flagsDecl))
                            {
                                // We previously reported the error.
                                flagsPrev = 0;
                                break;
                            }
                            flagsPrev = flagsDecl;
                        }
                    }

                    if ((flagsPrev != 0) && flags != flagsPrev)
                    {
                        // Conflicting accessibilities.
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_PartialModifierConflict,
                            new ErrArgRef(aggSym));
                    }
                }
            }

            ACCESS oldAccess = ACCESS.UNKNOWN;
            if (isMulti)
            {
                // This ASSERTs that access is not ACC_UNKNOWN ... If this is marked as a multiple definition,
                // then the previous instance should have set the access.
                oldAccess = aggSym.Access;
            }

            CheckFlags(aggSym, allowableFlags, aggregateNode.Flags);
            aggSym.Access = GetAccessFromFlags(parentBagSym, allowableFlags, aggregateNode.Flags);

            aggDeclSym.IsPartial = (aggregateNode.Flags & NODEFLAGS.MOD_PARTIAL & allowableFlags) != 0;

            if (isMulti && (aggregateNode.Flags & allowableFlags & NODEFLAGS.MOD_ACCESSMODIFIERS) == 0)
            {
                // This partial didn't specify accessibility so restore the previous one.
                aggSym.Access = oldAccess;
            }

            //--------------------------------------------------
            // unsafe if declared unsafe or container is unsafe.
            //--------------------------------------------------
            aggDeclSym.IsUnsafe = (
                (aggregateNode.Flags & NODEFLAGS.MOD_UNSAFE) != 0 ||
                (parentDeclSym.IsAGGDECLSYM && (parentDeclSym as AGGDECLSYM).IsUnsafe));

            //--------------------------------------------------
            // GENERICS: declare type parameters, but not bounds. These come later.
            // The type params come first because they will be needed
            // whenever instantiating   generic types, e.g. when resolving inheritance.
            //--------------------------------------------------
            if (!isMulti)
            {
                // Usual case: first or only declaration of this type.
                DefineClassTypeVars(aggSym, aggDeclSym, typeParamsNode);
            }
            else
            {
                // Make sure that the type variables agree in number and name.
                DebugUtil.Assert(typeParamsCount == aggSym.TypeVariables.Count);
                int ivar = 0;
                bool fError = false;

                {
                    BASENODE tva = typeParamsNode;
                    for (; tva != null; tva = tva.NextNode)
                    {
                        TYPEBASENODE node = tva.AsTYPEBASE;
                        if (node == null)
                        {
                            continue;
                        }
                        DebugUtil.Assert(ivar < typeParamsCount);

                        TYVARSYM var = aggSym.TypeVariables.ItemAsTYVARSYM(ivar++);
                        BASENODE nodeName;

                        if (node.Kind == NODEKIND.TYPEWITHATTR)
                        {
                            // Add the attributes of node to the TYVARSYM of TypeVariables of aggSym with the same index.
                            BASENODE nodeAttr = (node as TYPEWITHATTRNODE).AttributesNode;
                            DebugUtil.Assert(nodeAttr != null);
                            Compiler.MainSymbolManager.AddToGlobalAttrList(nodeAttr, aggDeclSym, var.AttributeList);
                            nodeName = (node as TYPEWITHATTRNODE).TypeBaseNode;
                        }
                        else
                        {
                            nodeName = node;
                        }

                        if (nodeName.Kind == NODEKIND.NAMEDTYPE)
                        {
                            nodeName = (nodeName as NAMEDTYPENODE).NameNode;
                        }

                        if (nodeName.Kind != NODEKIND.NAME)
                        {
                            DebugUtil.Assert(nodeName.IsAnyType);
                            Compiler.Error(nodeName, CSCERRID.ERR_TypeParamMustBeIdentifier);
                        }
                        else if (var.Name != (nodeName as NAMENODE).Name)
                        {
                            if (!fError)
                            {
                                Compiler.ErrorRef(
                                    null,
                                    CSCERRID.ERR_PartialWrongTypeParams,
                                    new ErrArgRef(aggSym));
                            }
                            fError = true;
                        }
                        else
                        {
                        }
                    }
                }
                DebugUtil.Assert(ivar == typeParamsCount);
            }
            DebugUtil.Assert(aggSym.TypeVariables != null && aggSym.AllTypeVariables != null);
            aggSym.AggState = AggStateEnum.Declared;
            return aggDeclSym;
        }

        //------------------------------------------------------------
        // CLSDREC.AddSynthAgg
        //
        /// <summary>
        /// Add a synthesised aggregate symbol to the given parent. Does NOT check for collisions.
        /// Does NOT declare nested types, or other aggregate members.
        /// </summary>
        /// <param name="baseName">
        /// <para>(sscli) szBasename is the user-supplied portion of the synthesized aggregate name</para>
        /// </param>
        /// <param name="specNameKind">
        /// <para>(sscli) snk is the particular kind of aggregate we're syntehesizing</para>
        /// </param>
        /// <param name="parentAggDeclSym">
        /// <para>(sscli) declPar is the containing declaration for the new aggregate</para>
        /// </param>
        /// <returns></returns>
        //------------------------------------------------------------
        private AGGDECLSYM AddSynthAgg(
            string baseName,
            SpecialNameKindEnum specNameKind,
            AGGDECLSYM parentAggDeclSym)
        {
            string ident = null;
            AGGSYM aggSym = null;
            AGGDECLSYM aggDeclSym = null;

            ident = Compiler.FuncBRec.CreateSpecialName(specNameKind, baseName);

            // Create new aggregate and associated decl.
            aggSym = Compiler.MainSymbolManager.CreateAgg(ident, parentAggDeclSym);
            aggDeclSym = Compiler.MainSymbolManager.CreateAggDecl(aggSym, parentAggDeclSym);

            aggSym.IsArityInName = false;
            aggSym.HasParseTree = false;
            aggDeclSym.ParseTreeNode = null;
            aggSym.AggKind = AggKindEnum.Class;
            aggSym.IsSealed = true;
            aggSym.Access = ACCESS.PRIVATE;
            aggSym.IsFabricated = true;
            aggSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            aggSym.AllTypeVariables = parentAggDeclSym.AggSym.AllTypeVariables;

            aggSym.AggState = AggStateEnum.Bounds;

            return aggDeclSym;
        }

        //------------------------------------------------------------
        // CLSDREC.FabricateExplicitImplPropertyRO
        //
        /// <summary>
        /// FabricateExplicitImplPropertyRO creates an Explicit Interface Implementation
        /// of a read-only property
        /// </summary>
        /// <param name="predefName"></param>
        /// <param name="ifaceAggTypeSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private PROPSYM FabricateExplicitImplPropertyRO(
            PREDEFNAME predefName,
            AGGTYPESYM ifaceAggTypeSym,
            AGGDECLSYM aggDeclSym)
        {
            string name = compiler.NameManager.GetPredefinedName(predefName);
            PROPSYM ifacePropSym = compiler.MainSymbolManager.LookupAggMember(
                name,
                ifaceAggTypeSym.GetAggregate(),
                SYMBMASK.PROPSYM) as PROPSYM;

            if (ifacePropSym == null ||
                ifacePropSym.GetMethodSym == null ||
                ifacePropSym.ParameterTypes.Count > 0)
            {
                compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_MissingPredefinedMember,
                    new ErrArgRef(ifaceAggTypeSym),
                    new ErrArgRef(name));
                return null;
            }

            // Make a private property symbol pointing to the iface property, with a given parent.
            PROPSYM propSym = compiler.MainSymbolManager.CreateProperty(null, aggDeclSym.AggSym, aggDeclSym);
            propSym.Access = ACCESS.PRIVATE;
            propSym.ReturnTypeSym = compiler.MainSymbolManager.SubstType(
                ifacePropSym.ReturnTypeSym,
                ifaceAggTypeSym,
                null);
            propSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            propSym.SlotSymWithType.Set(ifacePropSym, ifaceAggTypeSym);

            // Make the accessor
            name = CreateAccessorName(propSym.GetRealName(), "get_");
            METHSYM getMethodSym = compiler.MainSymbolManager.CreateMethod(name, aggDeclSym.AggSym, aggDeclSym);
            getMethodSym.Access = ACCESS.PRIVATE;
            getMethodSym.MethodKind = MethodKindEnum.PropAccessor;
            getMethodSym.PropertySym = propSym;
            getMethodSym.ReturnTypeSym = propSym.ReturnTypeSym;
            getMethodSym.IsMetadataVirtual = true;
            getMethodSym.IsFabricated = true;
            getMethodSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            getMethodSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            getMethodSym.SlotSymWithType.Set(ifacePropSym.GetMethodSym, ifaceAggTypeSym);
            getMethodSym.NeedsMethodImp = true;

            propSym.GetMethodSym = getMethodSym;

            return propSym;
        }

        //------------------------------------------------------------
        // CLSDREC.FabricateExplicitImplMethod
        //
        /// <summary>
        /// FabricateExplicitImplMethod creates an Explicit Interface Implementation method
        /// </summary>
        /// <param name="predefName"></param>
        /// <param name="ifaceAggTypeSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM FabricateExplicitImplMethod(
            PREDEFNAME predefName,
            AGGTYPESYM ifaceAggTypeSym,
            AGGDECLSYM aggDeclSym)
        {
            METHSYM ifaceMethodSym = compiler.FuncBRec.FindPredefMeth(
                null,
                predefName,
                ifaceAggTypeSym,
                BSYMMGR.EmptyTypeArray,
                true,
                MemLookFlagsEnum.None);

            if (ifaceMethodSym == null)
            {
                // FindPredefMeth already reported the error....
                return null;
            }

            // Make sure FindPredefMeth did its job....
            DebugUtil.Assert(ifaceMethodSym.ClassSym == ifaceAggTypeSym.GetAggregate());
            DebugUtil.Assert(ifaceMethodSym.TypeVariables.Count == 0 && ifaceMethodSym.ParameterTypes.Count == 0);

            // Make a public method symbol representing a method of type retType, with a given parent.
            METHSYM methodSym = compiler.MainSymbolManager.CreateMethod(null, aggDeclSym.AggSym, aggDeclSym);

            methodSym.Access = ACCESS.PRIVATE;
            methodSym.ReturnTypeSym = compiler.MainSymbolManager.SubstType(
                ifaceMethodSym.ReturnTypeSym,
                ifaceAggTypeSym,
                null);
            methodSym.ParameterTypes = BSYMMGR.EmptyTypeArray;
            methodSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            methodSym.SlotSymWithType.Set(ifaceMethodSym, ifaceAggTypeSym);
            methodSym.NeedsMethodImp = true;
            methodSym.IsMetadataVirtual = true;
            methodSym.IsFabricated = true;

            return methodSym;
        }

        //------------------------------------------------------------
        // CLSDREC.FabricateSimpleMethod
        //
        /// <summary>
        /// FabricateSimpleMethod creates a simple method
        /// </summary>
        /// <param name="predefName"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="returnTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM FabricateSimpleMethod(
            PREDEFNAME predefName,
            AGGDECLSYM aggDeclSym,
            TYPESYM returnTypeSym)
        {
            // Make a public method symbol representing a method of type retType name(), with a given parent.
            METHSYM meth = compiler.MainSymbolManager.CreateMethod(
                compiler.NameManager.GetPredefinedName(predefName),
                aggDeclSym.AggSym,
                aggDeclSym);

            meth.Access = ACCESS.PUBLIC;
            meth.ReturnTypeSym = returnTypeSym;
            meth.ParameterTypes = BSYMMGR.EmptyTypeArray;
            meth.TypeVariables = BSYMMGR.EmptyTypeArray;
            meth.IsFabricated = true;

            return meth;
        }

        //------------------------------------------------------------
        // CLSDREC.AddNamespace
        //
        /// <summary>
        /// <para>Add a namespace symbol to the given parent.
        /// Checks for collisions with classes, and displays an error if so.
        /// Returns a namespace regardless.</para>
        /// <para>If a namespace with the same name and the same parent is registered, return it.
        /// Or create a namespace symbol.</para>
        /// </summary>
        /// <param name="nameNode"></param>
        /// <param name="parentDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private NSSYM AddNamespace(NAMENODE nameNode, NSDECLSYM parentDeclSym)
        {
            string ident = nameNode.Name;

            // Check for existing namespace.
            for (SYM sym = Compiler.LookupGlobalSym(ident, parentDeclSym.NamespaceSym, SYMBMASK.ALL);
                sym != null;
                sym = sym.NextSameNameSym)
            {
                if (sym.IsNSSYM)
                {
                    return (sym as NSSYM);
                }

                DebugUtil.Assert(
                    !sym.IsAGGSYM ||
                    !(sym as AGGSYM).IsArityInName ||
                    (sym as AGGSYM).TypeVariables.Count > 0);

                if (sym.IsAGGSYM &&
                    (sym as AGGSYM).InAlias(Kaid.ThisAssembly) &&
                    !(sym as AGGSYM).IsArityInName)
                {
                    Compiler.Error(
                        nameNode,
                        CSCERRID.ERR_DuplicateNameInNS,
                        new ErrArgRefOnly(nameNode),
                        new ErrArgRefOnly(parentDeclSym.NamespaceSym),
                        new ErrArgRefOnly(sym));
                    return null;
                }
            }

            // If not found, create new namespace.
            return Compiler.MainSymbolManager.CreateNamespace(ident, parentDeclSym.NamespaceSym);
        }

        //------------------------------------------------------------
        // CLSDREC.AddNamespaceDeclaration
        //
        /// <summary>
        /// <para>If the namespace symbol with argument name is not found, create it.</para>
        /// Create a NSDECLSYM instance and register it to the namespace with argument name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parseTree"></param>
        /// <param name="containingDeclaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private NSDECLSYM AddNamespaceDeclaration(
            NAMENODE name,
            NAMESPACENODE parseTree,
            NSDECLSYM containingDeclaration)
        {
            // create the namespace symbol

            NSSYM nsSym = AddNamespace(name, containingDeclaration);
            if (nsSym == null)
            {
                return null;
            }

            // create the new declaration and link it in

            return Compiler.MainSymbolManager.CreateNamespaceDecl(
                        nsSym,
                        containingDeclaration,
                        containingDeclaration.InFileSym,
                        parseTree);
        }

        //------------------------------------------------------------
        // CLSDREC.CheckHiddenSymbol
        //
        /// <summary>
        /// <para>checks if we are hiding an abstract method in a bad way</para>
        /// <para>this happens when the symNew is in an abstract class and
        /// it will prevent the declaration of non-abstract derived classes</para>
        /// </summary>
        /// <param name="newSym"></param>
        /// <param name="hiddenSymWithType"></param>
        //------------------------------------------------------------
        private void CheckHiddenSymbol(SYM newSym, SymWithType hiddenSymWithType)
        {
            // We are interested in hiding abstract methods in abstract classes
            if (hiddenSymWithType.Sym.IsMETHSYM &&
                hiddenSymWithType.MethSym.IsAbstract &&
                (newSym.ParentSym as AGGSYM).IsAbstract)
            {
                switch (newSym.Access)
                {
                    case ACCESS.INTERNAL:
                        // derived classes outside this assembly will be OK
                        break;

                    case ACCESS.PUBLIC:
                    case ACCESS.PROTECTED:
                    case ACCESS.INTERNALPROTECTED:
                        // the new symbol will always hide the abstract symbol
                        Compiler.ErrorRef(null, CSCERRID.ERR_HidingAbstractMethod,
                            new ErrArgRef(newSym), new ErrArgRef(hiddenSymWithType));
                        break;

                    case ACCESS.PRIVATE:
                        // no problem since the new member won't hide the abstract method in derived classes
                        break;

                    default:
                        // bad access
                        DebugUtil.Assert(false);
                        break;
                }
            }
            else if (hiddenSymWithType.Sym.IsPROPSYM)
            {
                if (hiddenSymWithType.PropSym.GetMethodSym != null)
                {
                    CheckHiddenSymbol(
                        newSym,
                        new SymWithType(hiddenSymWithType.PropSym.GetMethodSym, hiddenSymWithType.AggTypeSym));
                }
                if (hiddenSymWithType.PropSym.SetMethodSym != null)
                {
                    CheckHiddenSymbol(
                        newSym,
                        new SymWithType(hiddenSymWithType.PropSym.SetMethodSym, hiddenSymWithType.AggTypeSym));
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckFlags
        //
        /// <summary>
        /// <para>check the provided flags for item.</para>
        /// </summary>
        /// <param name="item">symbol to check</param>
        /// <param name="allowedFlags">what flags to allow</param>
        /// <param name="actualFlags">provided flags</param>
        //------------------------------------------------------------
        private void CheckFlags(SYM sym, NODEFLAGS allowedFlags, NODEFLAGS actualFlags)
        {
            //--------------------------------------------------
            // if any flags are disallowed, tell which:
            //--------------------------------------------------
            if ((actualFlags & ~allowedFlags) != 0)
            {
                NODEFLAGS flags = (NODEFLAGS)1;
                int index = 0;
                for (; flags <= NODEFLAGS.MOD_LAST_KWD; flags = (NODEFLAGS)((uint)flags << 1))
                {
                    if ((flags & actualFlags & ~allowedFlags) != 0)
                    {
                        TOKENID id = accessTokens[index];
                        TOKINFO info = CParser.GetTokenInfo(id);

                        // Types declared in namespaces can have public or internal access modifiers.
                        if (sym.ParentSym.IsNSSYM &&
                            (flags & (NODEFLAGS.MOD_PRIVATE | NODEFLAGS.MOD_PROTECTED)) != 0)
                        {
                            // give a better error for namespace elements
                            Compiler.ErrorRef(
                                null,
                                CSCERRID.ERR_NoNamespacePrivate,
                                new ErrArgRef(sym));
                        }
                        else if (sym.ParentSym.IsNSSYM && (flags & NODEFLAGS.MOD_NEW) != 0)
                        {
                            Compiler.ErrorRef(
                                null,
                                CSCERRID.ERR_NoNewOnNamespaceElement,
                                new ErrArgRef(sym));
                        }
                        else
                        {
                            Compiler.ErrorRef(
                                null,
                                CSCERRID.ERR_BadMemberFlag,
                                new ErrArgRef(info.Text),
                                new ErrArgRefOnly(sym)); // Item is only used for location
                        }
                    }
                    ++index;
                }
                actualFlags &= allowedFlags;
            } // if ((actualFlags & ~allowedFlags) != 0)

            //--------------------------------------------------
            // check unsafe modifier.
            //--------------------------------------------------
            if ((actualFlags & allowedFlags & NODEFLAGS.MOD_UNSAFE) != 0 &&
                !(Compiler.OptionManager.Unsafe))
            {
                Compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_IllegalUnsafe,
                    new ErrArgRef(sym));
            }

            //--------------------------------------------------
            // Check conflicts of modifiers.
            //--------------------------------------------------
            if (!sym.IsAGGSYM)
            {
                // AddAggregate handles these for aggs
                // check for conflict with abstract and sealed modifiers
                if ((actualFlags & (NODEFLAGS.MOD_ABSTRACT | NODEFLAGS.MOD_SEALED)) ==
                    (NODEFLAGS.MOD_ABSTRACT | NODEFLAGS.MOD_SEALED))
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_AbstractAndSealed,
                        new ErrArgRef(sym));
                }

                // Check for conclict between virtual, new, static, override, abstract
                if ((allowedFlags &
                    (NODEFLAGS.MOD_VIRTUAL | NODEFLAGS.MOD_OVERRIDE | NODEFLAGS.MOD_NEW | NODEFLAGS.MOD_ABSTRACT)) != 0)
                {
                    if ((actualFlags & NODEFLAGS.MOD_STATIC) != 0 &&
                        (actualFlags & (NODEFLAGS.MOD_VIRTUAL | NODEFLAGS.MOD_OVERRIDE | NODEFLAGS.MOD_ABSTRACT)) != 0)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_StaticNotVirtual,
                            new ErrArgRef(sym));
                    }
                    else if (
                        (actualFlags & NODEFLAGS.MOD_OVERRIDE) != 0 &&
                        (actualFlags & (NODEFLAGS.MOD_VIRTUAL | NODEFLAGS.MOD_NEW)) != 0)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_OverrideNotNew,
                            new ErrArgRef(sym));
                    }
                    else if (
                        (actualFlags & NODEFLAGS.MOD_ABSTRACT) != 0 &&
                        (actualFlags & NODEFLAGS.MOD_VIRTUAL) != 0)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_AbstractNotVirtual,
                            new ErrArgRef(sym));
                    }

                    if ((actualFlags & NODEFLAGS.MOD_EXTERN) != 0 &&
                        (actualFlags & NODEFLAGS.MOD_ABSTRACT) != 0)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_AbstractAndExtern,
                            new ErrArgRef(sym));
                    }
                }
            }// if (!sym.IsAGGSYM)
        }

        //------------------------------------------------------------
        // CLSDREC.GetAccessFromFlags
        //
        /// <summary>
        /// Return access level on the item based on the given flags.
        /// Does not do error checking, so callers should call CheckFlags() as well.
        /// <param name="container">parent to get default access from,
        /// is one of NSSYM, AGGSYM, or PROPSYM (for accessors).</param>
        /// <param name="allowedFlags">will not set access
        /// unless the specified access is one of those allowed.</param>
        /// <param name="actualFlags">provided flags</param>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="allowedFlags"></param>
        /// <param name="actualFlags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private ACCESS GetAccessFromFlags(SYM container, NODEFLAGS allowedFlags, NODEFLAGS actualFlags)
        {
            NODEFLAGS protFlags = (actualFlags & allowedFlags & (NODEFLAGS.MOD_ACCESSMODIFIERS));
            switch (protFlags)
            {
                default:
                    DebugUtil.Assert(false, "Invalid protection modifiers");
                    // fallthrough
                    goto case 0;

                case 0:
                    // get default protection level from our container
                    switch (container.Kind)
                    {
                        case SYMKIND.NSSYM:
                            return ACCESS.INTERNAL;
                        case SYMKIND.AGGSYM:
                            if (!(container as AGGSYM).IsInterface)
                            {
                                return ACCESS.PRIVATE;
                            }
                            else
                            {
                                return ACCESS.PUBLIC;
                            }
                        case SYMKIND.PROPSYM:
                            return container.Access;

                        default:
                            DebugUtil.Assert(false, "Unknown parent");
                            return ACCESS.PRIVATE;
                    }

                case NODEFLAGS.MOD_INTERNAL:
                    return ACCESS.INTERNAL;

                case NODEFLAGS.MOD_PRIVATE:
                    return ACCESS.PRIVATE;

                case NODEFLAGS.MOD_PROTECTED:
                    return ACCESS.PROTECTED;

                case NODEFLAGS.MOD_PUBLIC:
                    return ACCESS.PUBLIC;

                case NODEFLAGS.MOD_PROTECTED | NODEFLAGS.MOD_INTERNAL:
                    return ACCESS.INTERNALPROTECTED;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineEventAccessors
        //
        /// <summary></summary>
        /// <param name="eventSym"></param>
        /// <param name="baseEventWithType"></param>
        //------------------------------------------------------------
        private void DefineEventAccessors(EVENTSYM eventSym, EventWithType baseEventWithType)
        {
            BASENODE treeNode = eventSym.ParseTreeNode;

            // flags: static, abstract, override, virtual, extern, sealed

            bool isPropertyEvent = (treeNode.Kind == NODEKIND.PROPERTY);
            NODEFLAGS flags = eventSym.GetParseFlags();
            AGGSYM aggSym = null;
            aggSym = eventSym.ClassSym;

            string addName = null;
            string removeName = null;
            if (baseEventWithType.IsNotNull &&
                baseEventWithType.EventSym.AddMethodSym != null &&
                baseEventWithType.EventSym.AddMethodSym != null)
            {
                addName = baseEventWithType.EventSym.AddMethodSym.Name;
                removeName = baseEventWithType.EventSym.RemoveMethodSym.Name;
            }
            if (addName == null || removeName == null)
            {
                addName = CreateAccessorName(eventSym.Name, "add_");
                removeName = CreateAccessorName(eventSym.Name, "remove_");
            }

            TypeArray paramArray = Compiler.MainSymbolManager.AllocParams(eventSym.TypeSym);

            // Create Accessor for Add.

            CreateAccessor(ref eventSym.AddMethodSym,
                addName,
                isPropertyEvent ? (treeNode as PROPERTYNODE).SetNode : treeNode,
                paramArray,
                Compiler.MainSymbolManager.VoidSym,
                flags,
                eventSym);
            DebugUtil.Assert(eventSym.AddMethodSym != null);

            // Create accessor for Remove.
            CreateAccessor(ref eventSym.RemoveMethodSym,
                removeName,
                isPropertyEvent ? (treeNode as PROPERTYNODE).GetNode : treeNode,
                paramArray,
                Compiler.MainSymbolManager.VoidSym,
                flags,
                eventSym);
            DebugUtil.Assert(eventSym.RemoveMethodSym != null);

            // Need to pick-up any LinkDemands (NOTE: event non-Property events can have CAs on the accessors)
            MethAttrBind.CompileEarly(Compiler, eventSym.AddMethodSym);
            MethAttrBind.CompileEarly(Compiler, eventSym.RemoveMethodSym);
        }

        //------------------------------------------------------------
        // CLSDREC.DefinePropertyAccessor
        //
        /// <summary>
        /// Defines a property accessor:
        /// First creates the METHSYM and then
        /// sets access based on any access modifiers found on the accessor 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parseTreeNode"></param>
        /// <param name="paramArray"></param>
        /// <param name="returnTypeSym"></param>
        /// <param name="propertyFlags"></param>
        /// <param name="propertySym"></param>
        /// <param name="accessorMethSym"></param>
        //------------------------------------------------------------
        private void DefinePropertyAccessor(
            string name,
            BASENODE parseTreeNode,
            TypeArray paramArray,
            TYPESYM returnTypeSym,
            NODEFLAGS propertyFlags,
            PROPSYM propertySym,
            ref METHSYM accessorMethSym)
        {
            DebugUtil.Assert(paramArray != null);
            DebugUtil.Assert(propertySym.ClassSym.IsSource);

            accessorMethSym = null;
            AGGSYM aggSym = propertySym.ClassSym;

            CreateAccessor(
                ref accessorMethSym,
                name,
                parseTreeNode,
                paramArray,
                returnTypeSym,
                propertyFlags,
                propertySym);

            METHSYM accSym = accessorMethSym;
            DebugUtil.Assert(accSym != null);
            DebugUtil.Assert(accSym.ClassSym == aggSym);
            MethAttrBind.CompileEarly(Compiler, accSym);

            NODEFLAGS allowableFlags = propertySym.Name != null ? NODEFLAGS.MOD_ACCESSMODIFIERS : 0;
            CheckFlags(accSym, allowableFlags, parseTreeNode.Flags);

            accSym.Access = GetAccessFromFlags(propertySym, allowableFlags, parseTreeNode.Flags);

            bool hasAccessModifier = ((parseTreeNode.Flags & NODEFLAGS.MOD_ACCESSMODIFIERS) != 0);
            DebugUtil.Assert(hasAccessModifier || accSym.Access == propertySym.Access);
            if (hasAccessModifier)
            {
                // We allow private accessors on virtual properties but don't emit them as virtual.
                if (accSym.Access == ACCESS.PRIVATE)
                {
                    accSym.IsVirtual = false;
                    accSym.IsMetadataVirtual = false;
                    if (accSym.IsAbstract)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_PrivateAbstractAccessor,
                            new ErrArgRef(accSym));
                    }
                }

                if (propertySym.ClassSym.IsInterface)
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_PropertyAccessModInInterface,
                        new ErrArgRef(accSym));
                }
                else if (
                    propertySym.Name != null &&
                    (propertySym.Access <= accSym.Access ||
                    // Accessor visibility must be less visible than the property.
                      propertySym.Access == ACCESS.PROTECTED && accSym.Access == ACCESS.INTERNAL))
                    // There is no explicit more/less visible ordering between protected and internal,
                    // so we do not allow that combination.
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_InvalidPropertyAccessMod,
                        new ErrArgRef(accSym), new ErrArgRef(propertySym));
                }

                CheckForProtectedInSealed(accSym);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CreateAccessor
        //
        /// <summary>
        /// Create an accessor for either a property or an event. The symPar parameter must be either a
        /// PROPSYM or an EVENTSYM, and the generated methsym will inherit flags from it.
        /// NOTE: The methsym is an out parameter so that the accessor can be attached to the property
        /// or event immediately after creation (so prop.methGet/prop.methSet/evt.methAdd/evt.methRemove
        /// is set immediately). This is so METHSYM::getProperty works in case of an error inside
        /// CreateAccessor....
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="name"></param>
        /// <param name="parseTreeNode"></param>
        /// <param name="paramArray"></param>
        /// <param name="returnTypeSym"></param>
        /// <param name="ownerFlags"></param>
        /// <param name="parentSym"></param>
        //------------------------------------------------------------
        private void CreateAccessor(
            ref METHSYM methodSym,
            string name,
            BASENODE parseTreeNode,
            TypeArray paramArray,
            TYPESYM returnTypeSym,
            NODEFLAGS ownerFlags,
            SYM parentSym)
        {
            DebugUtil.Assert(parentSym.IsPROPSYM || parentSym.IsEVENTSYM);
            DebugUtil.Assert(parentSym.ParentSym.IsAGGSYM);
            AGGDECLSYM aggdecl = parentSym.ContainingDeclaration() as AGGDECLSYM;

            if (parseTreeNode != null)
            {
                // check for duplicate member name, or name same as class
                CheckForBadMember(name, SYMKIND.METHSYM, paramArray, parseTreeNode, aggdecl.AggSym, null, null);
            }

            // create accessor
            METHSYM acc = Compiler.MainSymbolManager.CreateMethod(name, aggdecl.AggSym, aggdecl);

            if (parseTreeNode == null)
            {
                acc.ParseTreeNode = parentSym.GetParseTree();
                acc.IsFabricated = true;
            }
            else
            {
                acc.ParseTreeNode = parseTreeNode;
            }

            if (parentSym.IsPROPSYM)
            {
                acc.MethodKind = MethodKindEnum.PropAccessor;
                acc.PropertySym = parentSym as PROPSYM;
            }
            else
            {
                acc.MethodKind = MethodKindEnum.EventAccessor;
                acc.EventSym = parentSym as EVENTSYM;
            }
            acc.ReturnTypeSym = returnTypeSym;
            acc.ParameterTypes = paramArray;
            acc.TypeVariables = BSYMMGR.EmptyTypeArray;
            acc.HasCLSAttribute = parentSym.HasCLSAttribute;
            acc.IsCLS = parentSym.IsCLS;
            acc.IsUnsafe = parentSym.IsUnsafe();
            acc.Access = parentSym.Access;
            acc.IsStatic = ((ownerFlags & NODEFLAGS.MOD_STATIC) != 0);
            acc.IsAbstract = aggdecl.AggSym.IsInterface || (ownerFlags & NODEFLAGS.MOD_ABSTRACT) != 0;
            acc.IsExternal = (ownerFlags & NODEFLAGS.MOD_EXTERN) != 0 && !acc.IsAbstract;
            acc.IsOverride = parentSym.IsOverride();

            // Attach the accessor to the property or event. NOTE: This MUST be done before any
            // errors can be reported on the accessor (so acc.getProperty() works).
            methodSym = acc;

            if (parentSym.IsDeprecated())
            {
                acc.CopyDeprecatedFrom(parentSym);
            }

            if (acc.IsAbstract || acc.IsOverride || (ownerFlags & NODEFLAGS.MOD_VIRTUAL) != 0)
            {
                acc.IsVirtual = ((ownerFlags & NODEFLAGS.MOD_SEALED) == 0);
                acc.IsMetadataVirtual = true;
            }

            if (parentSym.IsPROPSYM)
            {
                acc.IsParameterArray = (parentSym as PROPSYM).IsParameterArray;
                if ((parentSym as PROPSYM).IsEvent) acc.Access = ACCESS.PRIVATE;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefinePropertyAccessors
        //
        /// <summary></summary>
        /// <param name="propertySym"></param>
        /// <param name="basePropWithType"></param>
        //------------------------------------------------------------
        private void DefinePropertyAccessors(PROPSYM propertySym, PropWithType basePropWithType)
        {
            PROPERTYNODE propertyNode = propertySym.ParseTreeNode.AsANYPROPERTY;

            // Overriden properties use inherited names for accessors.
            string accessorName;

            // Create the get accessor.
            if (basePropWithType.IsNotNull && basePropWithType.PropSym.GetMethodSym != null)
            {
                accessorName = basePropWithType.PropSym.GetMethodSym.Name;
            }
            else
            {
                accessorName = CreateAccessorName(propertySym.GetRealName(), "get_");
            }

            if (propertyNode.GetNode != null)
            {
                DefinePropertyAccessor(
                    accessorName,
                    propertyNode.GetNode,
                    propertySym.ParameterTypes,
                    propertySym.ReturnTypeSym,
                    propertyNode.Flags,
                    propertySym,
                    ref propertySym.GetMethodSym);

                // CS3
                propertySym.GetMethodSym.AccessorKind = AccessorKindEnum.Get;

                bool hasAccessModifier = ((propertyNode.GetNode.Flags & NODEFLAGS.MOD_ACCESSMODIFIERS) != 0);
                if (hasAccessModifier)
                {
                    if (propertyNode.SetNode == null && !propertySym.IsOverride)
                    {
                        // a property must have both accessors to specify an access modifier on an accessor,
                        // unless it is overriding a base accessor
                        Compiler.ErrorRef(null, CSCERRID.ERR_AccessModMissingAccessor, new ErrArgRef(propertySym));
                    }
                    else if (propertyNode.SetNode != null &&
                        (propertyNode.SetNode.Flags & NODEFLAGS.MOD_ACCESSMODIFIERS) != 0)
                    {
                        // both get and set cannot have modifiiers
                        Compiler.ErrorRef(null, CSCERRID.ERR_DuplicatePropertyAccessMods, new ErrArgRef(propertySym));
                    }
                }
            }
            else if (
                basePropWithType.IsNotNull &&
                basePropWithType.PropSym.GetMethodSym != null &&
                propertySym.IsOverride &&
                (propertyNode.Flags & NODEFLAGS.MOD_SEALED) != 0)
            {
                CreateAccessor(ref propertySym.GetMethodSym,
                    accessorName,
                    null,
                    propertySym.ParameterTypes,
                    propertySym.ReturnTypeSym,
                    propertyNode.Flags,
                    propertySym);
                DebugUtil.Assert(propertySym.GetMethodSym.IsFabricated);
            }
            else
            {
                // According to ECMA 17.2.7, we must check for an get_XXX accessor name
                // even if the get accessor isn't defined.
                CheckForBadMember(
                    accessorName,
                    SYMKIND.METHSYM,
                    propertySym.ParameterTypes,
                    propertyNode,
                    propertySym.ClassSym,
                    null,
                    null);
            }

            // create set accessor

            if (basePropWithType.PropSym != null && basePropWithType.PropSym.SetMethodSym != null)
            {
                accessorName = basePropWithType.PropSym.SetMethodSym.Name;
            }
            else
            {
                accessorName = CreateAccessorName(propertySym.GetRealName(), "set_");
            }

            // build the signature for the set accessor
            //PTYPESYM *paramTypes = STACK_ALLOC(PTYPESYM, propertySym.params.size + 1);
            List<TYPESYM> paramTypes = new List<TYPESYM>();

            propertySym.ParameterTypes.CopyItems(0, propertySym.ParameterTypes.Count, paramTypes);
            paramTypes.Add(propertySym.ReturnTypeSym);
            TypeArray paramArray = Compiler.MainSymbolManager.AllocParams(paramTypes);

            if (propertyNode.SetNode != null)
            {
                DefinePropertyAccessor(
                    accessorName,
                    propertyNode.SetNode,
                    paramArray,
                    Compiler.MainSymbolManager.VoidSym,
                    propertyNode.Flags,
                    propertySym,
                    ref propertySym.SetMethodSym);

                // CS3
                propertySym.SetMethodSym.AccessorKind = AccessorKindEnum.Set;

                bool hasAccessModifier = ((propertyNode.SetNode.Flags & NODEFLAGS.MOD_ACCESSMODIFIERS) != 0);
                if (hasAccessModifier && propertyNode.GetNode == null && !propertySym.IsOverride)
                {
                    // a property must have both accessors to specify an access modifier on an accessor,
                    // unless it is overriding a base accessor.
                    Compiler.ErrorRef(null, CSCERRID.ERR_AccessModMissingAccessor, new ErrArgRef(propertySym));
                }
            }
            else if (
                basePropWithType.PropSym != null &&
                basePropWithType.PropSym.SetMethodSym != null &&
                propertySym.IsOverride &&
                (propertyNode.Flags & NODEFLAGS.MOD_SEALED) != 0)
            {
                CreateAccessor(ref propertySym.SetMethodSym,
                    accessorName,
                    null,
                    paramArray,
                    Compiler.MainSymbolManager.VoidSym,
                    propertyNode.Flags,
                    propertySym);
                DebugUtil.Assert(propertySym.SetMethodSym.IsFabricated);
            }
            else
            {
                // According to ECMA 17.2.7, we must check for an set_XXX accessor name
                // even if the set accessor isn't defined.
                CheckForBadMember(
                    accessorName,
                    SYMKIND.METHSYM,
                    paramArray,
                    propertyNode,
                    propertySym.ClassSym,
                    null,
                    null);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.FindSameSignature
        //
        /// <summary>
        /// <para>Searches the class [atsSearch] to see if it contains a method which is sufficient to implement [mwt].
        /// Does not search base classes. [mwt] is typically a method in some interface.
        /// We may be implementing this interface at some particular type, e.g. IList<String>,
        /// and so the required signature is the instantiation (i.e. substitution) of [mwt] for that instance.
        /// Similarly, the implementation may be provided by some base class that exists via polymorphic inheritance,
        /// e.g. Foo : List<String>, and so we must instantiate the parameters for each potential implementation.
        /// [atsSearch] may thus be an instantiated type.</para>
        /// <para>If fOverride is true, this checks for a method with swtSlot set to the particular method.</para>
        /// </summary>
        /// <param name="methWithType"></param>
        /// <param name="searchAggTypeSym"></param>
        /// <param name="fOverride"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM FindSameSignature(MethWithType methWithType, AGGTYPESYM searchAggTypeSym, bool fOverride)
        {
            TypeArray paramArray = Compiler.MainSymbolManager.SubstTypeArray(
                methWithType.MethSym.ParameterTypes,
                methWithType.AggTypeSym,
                null);

            for (SYM sym = Compiler.MainSymbolManager.LookupAggMember(
                methWithType.MethSym.Name, searchAggTypeSym.GetAggregate(), SYMBMASK.ALL);
                sym != null;
                sym = BSYMMGR.LookupNextSym(sym, sym.ParentSym, SYMBMASK.ALL))
            {
                METHSYM meth = sym as METHSYM;
                if (meth == null) continue;

                if (meth.TypeVariables.Count != methWithType.MethSym.TypeVariables.Count ||
                    !Compiler.MainSymbolManager.SubstEqualTypeArrays(
                        paramArray, meth.ParameterTypes, searchAggTypeSym, methWithType.MethSym.TypeVariables))
                {
                    DebugUtil.Assert(meth.SlotSymWithType.MethSym != methWithType.MethSym);
                    continue;
                }

                if (!fOverride ||
                    meth.SlotSymWithType.MethSym == methWithType.MethSym ||
                    // This condition handles when methWithType.Meth() is an abstract override and meth overrides the same method.
                    meth.SlotSymWithType.MethSym == methWithType.MethSym.SlotSymWithType.MethSym &&
                    meth.SlotSymWithType != null)
                {
                    return meth;
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // CLSDREC.FindSameSignature
        //
        /// <summary>
        /// See findSameSignature for methods above.
        /// </summary>
        /// <param name="propertyAggTypeSym"></param>
        /// <param name="propSym"></param>
        /// <param name="searchInAggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private PROPSYM FindSameSignature(
            AGGTYPESYM propertyAggTypeSym,
            PROPSYM propSym,
            AGGTYPESYM searchInAggTypeSym)
        {
            SYM symbol = Compiler.MainSymbolManager.LookupAggMember(
                propSym.Name,
                searchInAggTypeSym.GetAggregate(),
                SYMBMASK.ALL);
            TypeArray neededArray = Compiler.MainSymbolManager.SubstTypeArray(
                propSym.ParameterTypes,
                propertyAggTypeSym,
                null);
            while (symbol != null && (!symbol.IsPROPSYM ||
                !Compiler.MainSymbolManager.SubstEqualTypeArrays(
                    neededArray,
                    (symbol as PROPSYM).ParameterTypes,
                    searchInAggTypeSym,
                    null)))
            {
                symbol = BSYMMGR.LookupNextSym(symbol, symbol.ParentSym, SYMBMASK.ALL);
            }
            return (symbol as PROPSYM);
        }

        //------------------------------------------------------------
        // CLSDREC.BuildOrCheckAbstractMethodsList
        //
        /// <summary>
        /// For abstract classes, build list of abstract methods.
        /// For non-abstract classes, report errors on inherited abstract methods.
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void BuildOrCheckAbstractMethodsList(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.AbstractMethodSymList == null);

            if (aggSym.IsInterface)
            {
                return;
            }

            //PSYMLIST *addToList = &aggSym.abstractMethods;
            List<SYM> methodList = new List<SYM>();

            if (aggSym.IsAbstract)
            {
                DebugUtil.Assert(aggSym.IsClass);

                // Add all new abstract methods.
                for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
                {
                    if (child.IsMETHSYM && (child as METHSYM).IsAbstract)
                    {
                        Compiler.MainSymbolManager.AddToGlobalSymList(child, methodList);
                    }
                }
            }
            else if (!aggSym.IsSource)
            {
                goto END_PROCESSING;
            }

            // Deal with inherited abstract methods that we don't implement.
            // NOTE: this deals with property accessors as well.

            if (aggSym.BaseClassSym == null)
            {
                goto END_PROCESSING;
            }

            AGGTYPESYM methodAts = aggSym.BaseClassSym;
            List<SYM> baseMethList = methodAts.GetAggregate().AbstractMethodSymList;
            if (baseMethList != null)
            {
                //for (SYMLIST * listMeth = methodAts.getAggregate().abstractMethods; listMeth; listMeth = listMeth.next)
                foreach (SYM sym in baseMethList)
                {
                    //METHSYM * meth = listMeth.sym.AsMETHSYM;
                    METHSYM meth = sym as METHSYM;
                    if (meth == null) continue;

                    methodAts = methodAts.FindBaseType(meth.ClassSym);
                    DebugUtil.Assert(methodAts != null && methodAts.GetAggregate() == meth.ClassSym);
                    METHSYM methImpl;
                    MethWithType mwt = new MethWithType(meth, methodAts);

                    if ((methImpl = FindSameSignature(mwt, aggSym.GetThisType(), true)) == null)
                    {
                        if (!aggSym.IsAbstract)
                        {
                            Compiler.ErrorRef(null, CSCERRID.ERR_UnimplementedAbstractMethod,
                                new ErrArgRef(aggSym), new ErrArgRef(mwt));
                        }
                        else
                        {
                            Compiler.MainSymbolManager.AddToGlobalSymList(meth, methodList);
                        }
                    }
                }
            }

        END_PROCESSING:
            aggSym.AbstractMethodSymList = methodList;
        }

        //------------------------------------------------------------
        // CLSDREC.CheckInterfaceImplementations
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void CheckInterfaceImplementations(AGGSYM aggSym)
        {
#if DEBUG
            if (aggSym.SymID == 671)
            {
                ;
            }
#endif
            MethWithType methWithType = new MethWithType();
            PropWithType propWithType = new PropWithType();
            EventWithType eventWithType = new EventWithType();
            bool foundImport;
            AGGTYPESYM currentAts;
            METHSYM foundMethSym;

            // Check that all interface methods are implemented.
            for (int i = 0; i < aggSym.AllInterfaces.Count; ++i)
            {
                AGGTYPESYM ifaceAts = aggSym.AllInterfaces[i] as AGGTYPESYM;
                //FOREACHCHILD(ifaceAts.GetAggregate(), member)
                for (SYM member = ifaceAts.GetAggregate().FirstChildSym; member != null; member = member.NextSym)
                {
                    switch (member.Kind)
                    {
                        case SYMKIND.METHSYM:
                            METHSYM methSym = member as METHSYM;

                            if (methSym.IsAnyAccessor)
                            {
                                // property accessor implementations are checked
                                // by their property declaration
                                continue;
                            }
                            // (CS3) Extension method
                            if (methSym.IsInstanceExtensionMethod &&
                                methSym.StaticExtensionMethodSym != null)
                            {
                                continue;
                            }

                            METHSYM found;
                            MethWithType closeMwt = new MethWithType(null, null);

                            foundImport = false;
                            currentAts = aggSym.GetThisType();
                            methWithType.Set(methSym, ifaceAts);
                            do
                            {
                                // Check for explicit interface implementation.
                                found = FindExplicitInterfaceImplementation(currentAts, methWithType) as METHSYM;
                                if (found != null) break;

                                // Check for imported class which implements this interface.
                                if (!currentAts.GetAggregate().HasParseTree &&
                                    currentAts.GetIfacesAll().Contains(ifaceAts))
                                {
                                    foundImport = true;
                                    break;
                                }

                                // Check for implicit interface implementation.
                                found = FindSameSignature(methWithType, currentAts, false);
                                if (found != null)
                                {
                                    if (!found.IsStatic &&
                                        found.Access == ACCESS.PUBLIC &&
                                        Compiler.MainSymbolManager.SubstType(
                                        found.ReturnTypeSym, currentAts, methWithType.MethSym.TypeVariables) ==
                                        Compiler.MainSymbolManager.SubstType(
                                        methWithType.MethSym.ReturnTypeSym,
                                        ifaceAts,
                                        null))
                                    {
                                        // Found a match.
                                        if (found.IsAnyAccessor)
                                        {
                                            Compiler.ErrorRef(null, CSCERRID.ERR_AccessorImplementingMethod,
                                                new ErrArgRef(new MethWithType(found, currentAts)),
                                                new ErrArgRef(methWithType),
                                                new ErrArgRef(aggSym));
                                            goto LDoneMeth;
                                        }
                                        if (methWithType.MethSym.TypeVariables.Count > 0)
                                        {
                                            DebugUtil.Assert(
                                                methWithType.MethSym.TypeVariables.Count == found.TypeVariables.Count);
                                            CheckImplicitImplConstraints(
                                                new MethWithType(found, currentAts), methWithType);
                                        }
                                        break;
                                    }

                                    // Found a close match, save it for error reporting
                                    // and continue checking.
                                    if (closeMwt.IsNull) closeMwt.Set(found, currentAts);
                                }

                                found = null;
                                currentAts = currentAts.GetBaseClass();
                            } while (currentAts != null);

                            if (found != null)
                            {
                                List<string> symList = GetConditionalSymbols(found);
                                if (symList != null && symList.Count > 0)
                                {
                                    Compiler.ErrorRef(null, CSCERRID.ERR_InterfaceImplementedByConditional,
                                        new ErrArgRef(new MethWithType(found, currentAts)),
                                        new ErrArgRef(methWithType),
                                        new ErrArgRef(aggSym));
                                }
                                if (!found.IsExplicitImplementation)
                                {
                                    found = NeedExplicitImpl(methWithType, found, aggSym);
                                }

                                // If the implementing method can't be set to isMetadataVirtual
                                // then we should have added a compiler generated explicit impl in NeedExplicitImpl
                                DebugUtil.Assert(found.IsMetadataVirtual);
                            }
                            else if (!foundImport)
                            {
                                // Didn't find an implementation.
                                if (closeMwt.IsNull)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_UnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(methWithType));
                                }
                                else
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_CloseUnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(methWithType),
                                        new ErrArgRef(closeMwt));
                                }
                            }
                        LDoneMeth:
                            break;

                        case SYMKIND.PROPSYM:
                            if (member.HasBogus && member.CheckBogus())
                            {
                                // don't need implementation of bogus properties
                                // just need implementation of accessors as regular methods
                                break;
                            }

                            bool isExplicitImpl = false;
                            PROPSYM foundPropSym;
                            PROPSYM closePropSym = null;

                            foundImport = false;
                            currentAts = aggSym.GetThisType();
                            propWithType.Set(member as PROPSYM, ifaceAts);
                            do
                            {
                                // Check for explicit interface implementation.
                                foundPropSym = FindExplicitInterfaceImplementation(currentAts, propWithType) as PROPSYM;
                                if (foundPropSym != null)
                                {
                                    isExplicitImpl = true;
                                    break;
                                }

                                // Check for imported class which implements this interface.
                                if (!currentAts.GetAggregate().HasParseTree &&
                                    currentAts.GetIfacesAll().Contains(ifaceAts))
                                {
                                    foundImport = true;
                                    break;
                                }

                                // Check for implicit interface implementation.
                                foundPropSym = FindSameSignature(ifaceAts, propWithType.PropSym, currentAts);
                                if (foundPropSym != null)
                                {
                                    if (!foundPropSym.IsStatic &&
                                        foundPropSym.Access == ACCESS.PUBLIC &&
                                        Compiler.MainSymbolManager.SubstType(foundPropSym.ReturnTypeSym, currentAts,null) ==
                                        Compiler.MainSymbolManager.SubstType(propWithType.PropSym.ReturnTypeSym, ifaceAts,null) &&
                                        !foundPropSym.IsOverride)
                                    {
                                        // found a match
                                        break;
                                    }

                                    // Found a close match, save it for error reporting
                                    // and continue checking.
                                    if (closePropSym == null) closePropSym = foundPropSym;
                                    foundPropSym = null;
                                }
                                else
                                {
                                    // Check for methods that collide with the accessor names.
                                    if (propWithType.PropSym.GetMethodSym != null)
                                    {
                                        methWithType.Set(propWithType.PropSym.GetMethodSym, propWithType.AggTypeSym);
                                        foundMethSym = FindSameSignature(methWithType, currentAts, false);
                                        if (foundMethSym != null)
                                        {
                                            Compiler.ErrorRef(
                                                null,
                                                CSCERRID.ERR_MethodImplementingAccessor,
                                                new ErrArgRef(new MethWithType(foundMethSym, currentAts)),
                                                new ErrArgRef(methWithType),
                                                new ErrArgRef(aggSym));
                                            goto LDoneProp;
                                        }
                                    }
                                    if (propWithType.PropSym.SetMethodSym != null)
                                    {
                                        methWithType.Set(propWithType.PropSym.SetMethodSym, propWithType.AggTypeSym);
                                        foundMethSym = FindSameSignature(methWithType, currentAts, false);
                                        if (foundMethSym != null)
                                        {
                                            Compiler.ErrorRef(
                                                null,
                                                CSCERRID.ERR_MethodImplementingAccessor,
                                                new ErrArgRef(new MethWithType(foundMethSym, currentAts)),
                                                new ErrArgRef(methWithType),
                                                new ErrArgRef(aggSym));
                                            goto LDoneProp;
                                        }
                                    }
                                }
                                currentAts = currentAts.GetBaseClass();
                            } while (currentAts != null);

                            if (foundPropSym == null && !foundImport)
                            {
                                // Didn't find an implementation.
                                if (closePropSym == null)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_UnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(propWithType));
                                }
                                else
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_CloseUnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(propWithType),
                                        new ErrArgRef(closePropSym));
                                }
                            }
                            else if (foundPropSym != null)
                            {
                                // Check that all accessors are implemented.
                                methWithType.Set((member as PROPSYM).GetMethodSym, ifaceAts);
                                if (methWithType.IsNotNull && foundPropSym.GetMethodSym == null)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_UnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(methWithType));
                                }
                                else if (methWithType.IsNotNull)
                                {
                                    if (!isExplicitImpl &&
                                        foundPropSym.GetMethodSym.Access != methWithType.MethSym.Access)
                                    {
                                        DebugUtil.Assert(methWithType.MethSym.Access == ACCESS.PUBLIC);
                                        Compiler.ErrorRef(
                                            null,
                                            CSCERRID.ERR_UnimplementedInterfaceAccessor,
                                            new ErrArgRef(aggSym),
                                            new ErrArgRef(methWithType),
                                            new ErrArgRef(foundPropSym.GetMethodSym));
                                    }
                                    METHSYM getMethSym;
                                    getMethSym = NeedExplicitImpl(methWithType, foundPropSym.GetMethodSym, aggSym);
                                    // If the implementing method can't be set to isMetadataVirtual
                                    // then we should have added a compiler generated explicit impl in NeedExplicitImpl.
                                    DebugUtil.Assert(getMethSym.IsMetadataVirtual);
                                }
                                methWithType.Set((member as PROPSYM).SetMethodSym, ifaceAts);
                                if (methWithType.IsNotNull && foundPropSym.SetMethodSym == null)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_UnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(methWithType));
                                }
                                else if (methWithType.IsNotNull)
                                {
                                    if (!isExplicitImpl &&
                                        foundPropSym.SetMethodSym.Access != methWithType.MethSym.Access)
                                    {
                                        DebugUtil.Assert(methWithType.MethSym.Access == ACCESS.PUBLIC);
                                        Compiler.ErrorRef(
                                            null,
                                            CSCERRID.ERR_UnimplementedInterfaceAccessor,
                                            new ErrArgRef(aggSym),
                                            new ErrArgRef(methWithType),
                                            new ErrArgRef(foundPropSym.SetMethodSym));
                                    }
                                    METHSYM setMethSym;
                                    setMethSym = NeedExplicitImpl(methWithType, foundPropSym.SetMethodSym, aggSym);
                                    // If the implementing method can't be set to isMetadataVirtual
                                    // then we should have added a compiler generated explcit impl in NeedExplicitImpl.
                                    DebugUtil.Assert(setMethSym.IsMetadataVirtual);
                                }
                            }
                        LDoneProp:
                            break;

                        case SYMKIND.EVENTSYM:
                        {
                            if (member.HasBogus && member.CheckBogus())
                            {
                                // don't need implementation of bogus events
                                // just need implementation of accessors as regular methods
                                break;
                            }

                            EVENTSYM foundEventSym;
                            EVENTSYM closeEventSym = null;

                            foundImport = false;
                            currentAts = aggSym.GetThisType();
                            eventWithType.Set(member as EVENTSYM, ifaceAts);
                            do
                            {
                                // Check for explicit interface implementation.
                                foundEventSym = FindExplicitInterfaceImplementation(currentAts, eventWithType) as EVENTSYM;
                                if (foundEventSym != null) break;

                                // Check for imported base class which implements this interface.
                                if (!currentAts.GetAggregate().HasParseTree &&
                                    currentAts.GetIfacesAll().Contains(ifaceAts))
                                {
                                    foundImport = true;
                                    break;
                                }

                                // Check for implicit interface implementation.
                                foundEventSym = Compiler.MainSymbolManager.LookupAggMember(
                                    eventWithType.EventSym.Name,
                                    currentAts.GetAggregate(),
                                    SYMBMASK.EVENTSYM) as EVENTSYM;
                                if (foundEventSym != null)
                                {
                                    if (!foundEventSym.IsStatic &&
                                        foundEventSym.Access == ACCESS.PUBLIC &&
                                        Compiler.MainSymbolManager.SubstType(foundEventSym.TypeSym, currentAts, null) ==
                                            Compiler.MainSymbolManager.SubstType(eventWithType.EventSym.TypeSym, ifaceAts, null))
                                    {
                                        // Found a match.
                                        break;
                                    }

                                    // Found a close match, save it for error reporting
                                    // and continue checking.
                                    if (closeEventSym == null) closeEventSym = foundEventSym;
                                    foundEventSym = null;
                                }
                                // Check for methods that collide with the accessor names.
                                else
                                {
                                    methWithType.Set(eventWithType.EventSym.AddMethodSym, eventWithType.AggTypeSym);
                                    foundMethSym = FindSameSignature(methWithType, currentAts, false);
                                    if (foundMethSym != null)
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.ERR_MethodImplementingAccessor,
                                            new ErrArgRef(new MethWithType(foundMethSym, currentAts)),
                                            new ErrArgRef(methWithType),
                                            new ErrArgRef(aggSym));
                                        goto LDoneEvent;
                                    }
                                    methWithType.Set(eventWithType.EventSym.RemoveMethodSym, eventWithType.AggTypeSym);
                                    foundMethSym = FindSameSignature(methWithType, currentAts, false);
                                    if (foundMethSym != null)
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.ERR_MethodImplementingAccessor,
                                            new ErrArgRef(new MethWithType(foundMethSym, currentAts)),
                                            new ErrArgRef(methWithType),
                                            new ErrArgRef(aggSym));
                                        goto LDoneEvent;
                                    }
                                }

                                currentAts = currentAts.GetBaseClass();
                            } while (currentAts != null);

                            if (foundEventSym == null && !foundImport)
                            {
                                // Didn't find an implementation.
                                if (closeEventSym == null)
                                {
                                    Compiler.ErrorRef(null, CSCERRID.ERR_UnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym), new ErrArgRef(eventWithType));
                                }
                                else
                                {
                                    Compiler.ErrorRef(null, CSCERRID.ERR_CloseUnimplementedInterfaceMember,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(eventWithType),
                                        new ErrArgRef(closeEventSym));
                                }
                            }
                            else if (foundEventSym != null)
                            {
                                methWithType.Set(eventWithType.EventSym.AddMethodSym, ifaceAts);
                                NeedExplicitImpl(methWithType, foundEventSym.AddMethodSym, aggSym);
                                methWithType.Set(eventWithType.EventSym.RemoveMethodSym, ifaceAts);
                                NeedExplicitImpl(methWithType, foundEventSym.RemoveMethodSym, aggSym);
                            }
                        LDoneEvent:
                            break;
                        }
                        case SYMKIND.TYVARSYM:
                        break;
                        default:
                        DebugUtil.Assert(false, "Unknown interface member");
                        break;
                    }
                }
                //ENDFOREACHCHILD
            }
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareClassOrStruct
        //
        /// <summary>
        /// prepares a class for compilation by preparing all of its elements...
        /// This should also verify that a class actually implements its interfaces...
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void PrepareClassOrStruct(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.IsPreparing);
            DebugUtil.Assert(aggSym.IsSource);
            DebugUtil.Assert(
                aggSym.BaseClassSym != null && aggSym.BaseClassSym.GetAggregate().IsPrepared ||
                 aggSym.IsPredefAgg(PREDEFTYPE.OBJECT) && aggSym.BaseClassSym == null);

            if (aggSym.IsAttribute)
            {
                DebugUtil.Assert(aggSym.BaseClassSym != null);
                AGGSYM baseAggSym = aggSym.GetBaseAgg();
                DebugUtil.Assert(
                    baseAggSym.AttributeClass != 0 ||
                    aggSym.IsPredefAgg(PREDEFTYPE.ATTRIBUTE));

                // Attributes that don't have usage set should inherit it from their baseAggSym class.
                if (aggSym.AttributeClass == 0)
                {
                    aggSym.AttributeClass = baseAggSym.AttributeClass;
                    aggSym.IsMultipleAttribute = baseAggSym.IsMultipleAttribute;
                }
            }

            aggSym.SetBogus(false);
            aggSym.AggState = AggStateEnum.Prepared;

            // check that there isn't a cycle in the members
            // of this struct and other structs

            if (aggSym.IsStruct && !aggSym.LayoutChecked)
            {
                CheckStructLayout(aggSym.GetThisType(), null);
            }

            // prepare members

            for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
            {
                Compiler.SetLocation(child);   //SETLOCATIONSYM(child);

                switch (child.Kind)
                {
                    case SYMKIND.MEMBVARSYM:
                        PrepareFields(child as MEMBVARSYM);
                        break;

                    case SYMKIND.AGGSYM:
                        // need to do this after fully preparing members
                        break;

                    case SYMKIND.TYVARSYM:
                        break;

                    case SYMKIND.METHSYM:
                        PrepareMethod(child as METHSYM);
                        break;

                    case SYMKIND.AGGTYPESYM:
                        break;

                    case SYMKIND.PROPSYM:
                        PrepareProperty(child as PROPSYM);
                        break;

                    case SYMKIND.EVENTSYM:
                        PrepareEvent(child as EVENTSYM);
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown node type");
                        break;
                }
            }

            //
            // for abstract classes, build list of abstract methods
            // NOTE: must come after preparing members for abstract property accessors
            //       and before preparing nested types so they can properly check themselves
            //
            BuildOrCheckAbstractMethodsList(aggSym);

            // prepare agggregate members

            for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
            {
                Compiler.SetLocation(child);   //SETLOCATIONSYM(child);

                switch (child.Kind)
                {
                    case SYMKIND.AGGSYM:
                        PrepareAggregate(child as AGGSYM);
                        break;

                    default:
                        break;
                }
            }

            CheckInterfaceImplementations(aggSym);

            aggSym.AggState = AggStateEnum.PreparedMembers;
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareInterfaceMember
        //
        /// <summary></summary>
        /// <param name="member"></param>
        //------------------------------------------------------------
        private void PrepareInterfaceMember(METHPROPSYM member)
        {
            // check that new members don't hide inherited members
            // we've built up a list of all inherited interfaces
            // search all of them for a member we will hide

            // for all inherited interfaces
            CheckIfaceHiding(member, member.GetParseTree().Flags);

            // shouldn't have a body on interface members

            if (member.IsMETHSYM)
            {
                if (((member as METHSYM).GetParseTree().NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_InterfaceMemberHasBody, new ErrArgRef(member));
                }
            }
            else
            {
                DebugUtil.Assert(member.IsPROPSYM);
                PROPSYM property = member as PROPSYM;

                if (property.IsIndexer)
                {
                    // bind IndexerName attribute in order to resolve the actual name of the indexer.
                    IndexerNameAttrBind.Compile(Compiler, property.AsINDEXERSYM);
                    CheckForBadMemberSimple(
                        property.GetRealName(),
                        property.GetParseTree(),
                        property.ClassSym,
                        null);
                }

                PropWithType basePwt = new PropWithType(null, null);
                DefinePropertyAccessors(property, basePwt);

                // didn't find a conflict between this property and anything else
                // check that the accessors don't have a conflict with other methods

                if (property.GetMethodSym != null)
                {
                    PrepareInterfaceMember(property.GetMethodSym);
                }
                if (property.SetMethodSym != null)
                {
                    PrepareInterfaceMember(property.SetMethodSym);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareMethod
        //
        /// <summary>
        /// <para>prepares a method for compilation.
        /// the parsetree is obtained from the method symbol.
        /// verify means whether to verify that the right override/new/etc...
        /// flags were specified on the method...</para>
        /// </summary>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        private void PrepareMethod(METHSYM methodSym)
        {
            DebugUtil.Assert(methodSym.ClassSym.IsSource);
            DebugUtil.Assert(methodSym.SlotSymWithType != null || methodSym.IsAnyAccessor);

            if (methodSym.IsInterfaceImpl)
            {
                return;
            }

            // Check for static parameters.

            TypeArray paramArray = methodSym.ParameterTypes;
            for (int i = 0; i < paramArray.Count; ++i)
            {
                // If static, shows an error message, otherwise returns false.
                Compiler.CheckForStaticClass(
                    null,
                    methodSym,
                    paramArray[i],
                    CSCERRID.ERR_ParameterIsStaticClass);
            }

            // Check for static return type.

            Compiler.CheckForStaticClass(
                null,
                methodSym,
                methodSym.ReturnTypeSym,
                CSCERRID.ERR_ReturnTypeIsStaticClass);

            // conversions are special

            if (methodSym.IsConversionOperator)
            {
                PrepareConversion(methodSym);
            }
            // operators and dtors just need some simple checking
            else if (methodSym.IsOperator || methodSym.IsDtor)
            {
                PrepareOperator(methodSym);
            }
            // for constructors the basic stuff(valid flags) is checked
            // in defineMethod(). The more serious stuff(base constructor calls)
            // is checked when we actually compile the method and we can
            // evaluate the arg list
            //
            // property/event accessors are handled in prepareProperty/prepareEvent by their
            // associated property/event
            //
            else if (!methodSym.IsAnyAccessor)
            {
                // The rules are as follows:
                // Of new, virtual, override, you can't do both override and virtual as well as
                // override and new.  So that leaves:
                // [new]
                //     - accept the method w/o any checks, regardless if base exists
                // [virtual]
                //     - mark method as new, if base contains previous method, then complain
                //     about missing [new] or [override]
                // [override]
                //     - check that base contains a virtual method
                // [new virtual]
                //     - mark method as new, make no checks  (same as [new])
                // [nothing]
                //     - mark method as new, check base class, give warning if previous exists

                // First, find out if a method w/ that name & args exists in the baseclasses:
                AGGSYM aggSym = methodSym.ClassSym;
                BASENODE treeNode = methodSym.ParseTreeNode;

                // check new, virtual & override flags

                if (!methodSym.IsExplicitImplementation)
                {
                    // for explicit interface implementation these flags can't be set
                    DebugUtil.Assert(methodSym.SlotSymWithType.IsNull);

                    if (!methodSym.IsCtor)
                    {
                        // find hidden member in a base class

                        if (!methodSym.IsOverride)
                        {
                            // not an override, just do the simple checks

                            CheckSimpleHiding(methodSym, treeNode.Flags);
                        }
                        else
                        {
                            // We have an override method.
                            SymWithType hiddenSwt = new SymWithType();
                            SymWithType ambigSwt = new SymWithType();
                            bool needMethImpl = false;

                            if (FindSymHiddenByMethPropAgg(
                                methodSym,
                                aggSym.BaseClassSym,
                                aggSym,
                                hiddenSwt,
                                ambigSwt,
                                ref needMethImpl))
                            {
                                if (!hiddenSwt.Sym.IsMETHSYM)
                                {
                                    // Found a non-method we will hide. We should not have 'override'.
                                    methodSym.IsOverride = false;
                                    Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideNonFunction,
                                        new ErrArgRef(methodSym), new ErrArgRef(hiddenSwt));
                                    CheckHiddenSymbol(methodSym, hiddenSwt);
                                }
                                else
                                {
                                    // The signatures and arity should match.
                                    DebugUtil.Assert(Compiler.MainSymbolManager.SubstEqualTypeArrays(
                                        methodSym.ParameterTypes,
                                        hiddenSwt.MethSym.ParameterTypes,
                                        hiddenSwt.AggTypeSym,
                                        methodSym.TypeVariables));
                                    DebugUtil.Assert(hiddenSwt.MethSym.TypeVariables.Count == methodSym.TypeVariables.Count);
                                    DebugUtil.Assert(!hiddenSwt.MethSym.IsAnyAccessor);

                                    // Found a method of same signature that we will hide. If it is an override also, set
                                    // slotSwt to its slotSwt.
                                    DebugUtil.Assert(
                                        (!hiddenSwt.MethSym.IsOverride) == (hiddenSwt.MethSym.SlotSymWithType.IsNull));

                                    if (hiddenSwt.MethSym.SlotSymWithType.IsNotNull)
                                    {
                                        methodSym.SlotSymWithType.Set(
                                            hiddenSwt.MethSym.SlotSymWithType.MethSym,
                                            Compiler.MainSymbolManager.SubstType(
                                            hiddenSwt.MethSym.SlotSymWithType.AggTypeSym,
                                            hiddenSwt.AggTypeSym,
                                            null) as AGGTYPESYM);
                                    }
                                    else
                                    {
                                        methodSym.SlotSymWithType = hiddenSwt;
                                    }

                                    if (needMethImpl)
                                    {
                                        methodSym.NeedsMethodImp = true;
                                        methodSym.IsNewSlot = true;
                                    }

                                    SymWithType slotSwt = methodSym.SlotSymWithType;

                                    if (ambigSwt != null && ambigSwt.IsNotNull)
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.ERR_AmbigOverride,
                                            new ErrArgRefOnly(methodSym),
                                            new ErrArgRef(hiddenSwt.MethSym),
                                            new ErrArgRef(ambigSwt.MethSym),
                                            new ErrArgRef(aggSym));
                                    }

                                    // If this is an override of Finalize on object, then give a warning...
                                    if (methodSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.DTOR) &&
                                        methodSym.ParameterTypes.Count == 0 &&
                                        methodSym.TypeVariables.Count == 0 &&
                                        slotSwt.AggTypeSym.IsPredefType(PREDEFTYPE.OBJECT))
                                    {
                                        Compiler.Error(methodSym.ParseTreeNode, CSCERRID.ERR_OverrideFinalizeDeprecated);
                                    }

                                    // Compute the constraints.
                                    DebugUtil.Assert(
                                        methodSym.TypeVariables != null &&
                                        slotSwt.MethSym.TypeVariables != null &&
                                        methodSym.TypeVariables.Count == slotSwt.MethSym.TypeVariables.Count);

                                    if (methodSym.TypeVariables.Count > 0)
                                    {
                                        SetOverrideConstraints(methodSym);
                                    }

                                    if (slotSwt.MethSym.HasBogus && slotSwt.MethSym.CheckBogus())
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideBogusMethod,
                                            new ErrArgRef(methodSym), new ErrArgRef(slotSwt));
                                    }

                                    if (!hiddenSwt.MethSym.IsVirtual)
                                    {
                                        if (hiddenSwt.MethSym.IsOverride)
                                        {
                                            Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideSealed,
                                                new ErrArgRef(methodSym), new ErrArgRef(hiddenSwt));
                                        }
                                        else
                                        {
                                            Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideNonVirtual,
                                                new ErrArgRef(methodSym), new ErrArgRef(hiddenSwt));
                                        }
                                    }

                                    // Access must match.
                                    if (slotSwt.MethSym.Access != methodSym.Access)
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.ERR_CantChangeAccessOnOverride,
                                            new ErrArgRef(methodSym),
                                            new ErrArgRef(Compiler.ErrAccess(slotSwt.MethSym.Access)),
                                            new ErrArgRef(slotSwt));
                                    }

                                    // Return type must match.
                                    if (!Compiler.MainSymbolManager.SubstEqualTypes(
                                        methodSym.ReturnTypeSym,
                                        slotSwt.MethSym.ReturnTypeSym,
                                        slotSwt.AggTypeSym,
                                        methodSym.TypeVariables))
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.ERR_CantChangeReturnTypeOnOverride,
                                            new ErrArgRef(methodSym), new ErrArgRef(slotSwt),
                                            new ErrArgNoRef(Compiler.MainSymbolManager.SubstType(
                                            slotSwt.MethSym.ReturnTypeSym,
                                            slotSwt.AggTypeSym,
                                            methodSym.TypeVariables)));
                                    }

                                    methodSym.CModifierCount = slotSwt.MethSym.CModifierCount;

                                    if (!methodSym.IsDeprecated() && slotSwt.MethSym.IsDeprecated())
                                    {
                                        Compiler.ErrorRef(null, CSCERRID.WRN_NonObsoleteOverridingObsolete,
                                            new ErrArgRef(methodSym), new ErrArgRef(slotSwt));
                                    }

                                    CheckLinkDemandOnOverride(methodSym, MethWithType.Convert(slotSwt));
                                }
                            }
                            else
                            {
                                // didn't find hidden base member
                                methodSym.IsOverride = false;
                                Compiler.ErrorRef(null, CSCERRID.ERR_OverrideNotExpected, new ErrArgRef(methodSym));
                            }
                        }

                        //
                        // check that abstract methods are in abstract classes
                        //
                        if (methodSym.IsAbstract && !aggSym.IsAbstract)
                        {
                            Compiler.ErrorRef(null, CSCERRID.ERR_AbstractInConcreteClass,
                                new ErrArgRef(methodSym), new ErrArgRef(aggSym));
                        }

                        // check that new virtual methods aren't in sealed classes

                        if (aggSym.IsSealed && methodSym.IsVirtual && !methodSym.IsOverride)
                        {
                            Compiler.ErrorRef(null, CSCERRID.ERR_NewVirtualInSealed,
                                new ErrArgRef(methodSym), new ErrArgRef(aggSym));
                        }
                    }

                    //
                    // check that the method body existence matches with the abstractness
                    // attribute of the method
                    //
                    if (methodSym.IsAbstract || methodSym.IsExternal)
                    {
                        if ((treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                        {
                            // found an abstract method with a body
                            Compiler.ErrorRef(null, methodSym.IsAbstract ?
                                CSCERRID.ERR_AbstractHasBody : CSCERRID.ERR_ExternHasBody,
                                new ErrArgRef(methodSym));
                        }
                    }
                    else
                    {
                        if ((treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0)
                        {
                            // found non-abstract method without body
                            if (!methodSym.IsPartialMethod)
                            {
                                Compiler.ErrorRef(
                                    null,
                                    CSCERRID.ERR_ConcreteMissingBody,
                                    new ErrArgRef(methodSym));
                            }
                        }
                    }

                }
                else  // if (!methodSym.IsExplicitImplementation)
                {
                    // method is an explicit interface implementation

                    // get the interface
                    CheckExplicitImpl(methodSym);

                    if (aggSym.IsStruct)
                    {
                        aggSym.HasExplicitImpl = true;
                    }

                    // must have a body

                    if ((treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0 && !methodSym.IsExternal)
                    {
                        // found non-abstract method without body
                        Compiler.ErrorRef(null, CSCERRID.ERR_ConcreteMissingBody, new ErrArgRef(methodSym));
                    }
                    else if (methodSym.IsExternal && (treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                    {
                        // found an extern method with a body
                        Compiler.ErrorRef(null, CSCERRID.ERR_ExternHasBody, new ErrArgRef(methodSym));
                    }
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareFields
        //
        /// <summary>
        /// prepare a field for compilation by setting its constant field,
        /// if present and veryfing that field shadowing is explicit...
        /// </summary>
        /// <param name="fieldSym"></param>
        //------------------------------------------------------------
        private void PrepareFields(MEMBVARSYM fieldSym)
        {
            DebugUtil.Assert(fieldSym.ClassSym.IsSource);

            if (!fieldSym.IsEvent)
            {
                // Check that the hiding flags are correct.

                // (CS3) The back fields of the auto-implemented properties have no ParseTreeNode.
                //CheckSimpleHiding(fieldSym, fieldSym.ParseTreeNode.ParentNode.Flags);

                NODEFLAGS nodeFlags = 0;
                if (fieldSym.ParseTreeNode != null &&
                    fieldSym.ParseTreeNode.ParentNode != null)
                {
                    nodeFlags = fieldSym.ParseTreeNode.ParentNode.Flags;
                }
                CheckSimpleHiding(fieldSym, nodeFlags);

                Compiler.CheckForStaticClass(
                    null,
                    fieldSym,
                    fieldSym.TypeSym,
                    CSCERRID.ERR_VarDeclIsStaticClass);
            }

            DebugUtil.Assert(!fieldSym.IsUnevaled);
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareConversion
        //
        /// <summary>
        /// prepare checks for a user defined conversion operator
        /// checks that the conversion doesn't override a compiler generated conversion
        /// checks extern and body don't conflict
        /// </summary>
        /// <param name="conversionMethSym"></param>
        //------------------------------------------------------------
        private void PrepareConversion(METHSYM conversionMethSym)
        {
            DebugUtil.Assert(conversionMethSym.ClassSym.IsSource);
            AGGSYM convAggSym = conversionMethSym.ClassSym;
            AGGTYPESYM convAggTypeSym = convAggSym.GetThisType();

            if (convAggSym.IsPredefAgg(PREDEFTYPE.G_OPTIONAL))
            {
                return;
            }

            // what are we converting from/to

            int retNubCount;
            int argNubCount;
            TYPESYM retBareTypeSym = StripNubs(
                conversionMethSym.ReturnTypeSym,
                conversionMethSym.ClassSym,
                out retNubCount);
            TYPESYM argBareTypeSym = StripNubs(
                conversionMethSym.ParameterTypes[0],
                conversionMethSym.ClassSym,
                out argNubCount);

            TYPESYM otherBareTypeSym;

            if (retBareTypeSym == convAggTypeSym)
            {
                otherBareTypeSym = argBareTypeSym;
                if (retNubCount > 0)
                {
                    // Make sure the other type isn't a base of nullable.
                    AGGTYPESYM nullableAggTypeSym = (conversionMethSym.ReturnTypeSym as NUBSYM).GetAggTypeSym();
                    if (nullableAggTypeSym != null && Compiler.IsBaseType(nullableAggTypeSym, argBareTypeSym))
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_ConversionWithBase, new ErrArgRef(conversionMethSym));
                        goto LErrBase;
                    }
                }
            }
            else
            {
                otherBareTypeSym = retBareTypeSym;
                if (convAggTypeSym != argBareTypeSym)
                {
                    DebugUtil.Assert(Compiler.ErrorCount() > 0 && !Compiler.FAbortEarly(0, null));
                    return;
                }
                if (argNubCount > 0)
                {
                    // Make sure the other type isn't a base of nullable.
                    AGGTYPESYM nullableAggTypeSym = (conversionMethSym.ParameterTypes[0] as NUBSYM).GetAggTypeSym();
                    if (nullableAggTypeSym != null && Compiler.IsBaseType(nullableAggTypeSym, argBareTypeSym))
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_ConversionWithBase, new ErrArgRef(conversionMethSym));
                        goto LErrBase;
                    }
                }
            }

            // Can't convert from/to a base or derived class or interface.
            if (otherBareTypeSym.IsAGGTYPESYM)
            {
                AGGTYPESYM outerAggTypeSym = otherBareTypeSym as AGGTYPESYM;

                // Can't convert to/from an interface.
                if (outerAggTypeSym.IsInterfaceType())
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_ConversionWithInterface, new ErrArgRef(conversionMethSym));
                }
                else if ((convAggSym.IsClass || convAggSym.IsStruct) && outerAggTypeSym.IsClassType())
                {
                    // Check that we aren't converting to/from a base class.
                    if (convAggTypeSym.FindBaseType(outerAggTypeSym))
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_ConversionWithBase, new ErrArgRef(conversionMethSym));
                    }
                    else if (outerAggTypeSym.FindBaseType(convAggTypeSym))
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_ConversionWithDerived, new ErrArgRef(conversionMethSym));
                    }
                }
            }

        LErrBase:

            // operators don't hide

            if (conversionMethSym.IsExternal)
            {
                if ((conversionMethSym.GetParseTree().NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                {
                    // found an abstract method with a body
                    Compiler.ErrorRef(null, CSCERRID.ERR_ExternHasBody, new ErrArgRef(conversionMethSym));
                }
            }
            else
            {
                if ((conversionMethSym.GetParseTree().NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0)
                {
                    // found non-abstract method without body
                    Compiler.ErrorRef(null, CSCERRID.ERR_ConcreteMissingBody, new ErrArgRef(conversionMethSym));
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareOperator
        //
        /// <summary>
        /// prepare checks for a user defined operator (not conversion operators)
        /// checks extern and body don't conflict
        /// </summary>
        /// <param name="operatorSym"></param>
        //------------------------------------------------------------
        private void PrepareOperator(METHSYM operatorSym)
        {
            DebugUtil.Assert(operatorSym.ClassSym.IsSource);

            // If this operator is defined in this project, it should have a body,
            // and if external, it should have no body.
            if (operatorSym.IsExternal)
            {
                if ((operatorSym.GetParseTree().NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                {
                    // found an abstract method with a body
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_ExternHasBody,
                        new ErrArgRef(operatorSym));
                }
            }
            else
            {
                if ((operatorSym.GetParseTree().NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0)
                {
                    // found non-abstract method without body
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_ConcreteMissingBody,
                        new ErrArgRef(operatorSym));
                }
            }

            if (operatorSym.IsDtor)
            {
                // Check for a sealed dtor in a base type.
                SymWithType hiddenSwt = new SymWithType();
                AGGSYM aggSym = operatorSym.ClassSym;

                bool needImpl = false;
                bool isHidden = FindSymHiddenByMethPropAgg(
                    operatorSym,
                    aggSym.BaseClassSym,
                    aggSym,
                    hiddenSwt,
                    null,
                    ref needImpl);

                // This should hide something....
                DebugUtil.Assert(isHidden || aggSym.IsPredefAgg(PREDEFTYPE.OBJECT));

                if (isHidden &&
                    hiddenSwt.Sym.IsMETHSYM &&
                    hiddenSwt.MethSym.IsDtor &&
                    !hiddenSwt.MethSym.IsVirtual)
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_CantOverrideSealed,
                        new ErrArgRef(operatorSym),
                        new ErrArgRef(hiddenSwt));
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareProperty
        //
        /// <summary>
        /// do prepare stage for a property in a class or struct
        /// verifies new, override, abstract, virtual against
        /// inherited members.
        /// </summary>
        /// <param name="propertySym"></param>
        //------------------------------------------------------------
        private void PrepareProperty(PROPSYM propertySym)
        {
            DebugUtil.Assert(propertySym.ClassSym.IsSource);
            DebugUtil.Assert(!propertySym.SlotSymWithType.IsNotNull);
            AGGSYM aggTypeSym = propertySym.ClassSym;

            if (propertySym.IsIndexer)
            {
                // bind IndexerName attribute in order to resolve the actual name of the indexer.
                IndexerNameAttrBind.Compile(Compiler, propertySym.AsINDEXERSYM);
                CheckForBadMemberSimple(
                    propertySym.GetRealName(),
                    propertySym.GetParseTree(),
                    aggTypeSym,
                    null);
            }

            // First, find out if a property w/ that name & args exists in the baseclasses:
            PROPERTYNODE treeNode = propertySym.ParseTreeNode.AsANYPROPERTY;

            // basePwt is where to look for accessor names.
            PropWithType basePwt = new PropWithType(null, null);

            if (propertySym.IsExplicitImplementation)
            {
                // Property is an explicit interface implementation
                DefinePropertyAccessors(propertySym, basePwt);

                CheckExplicitImpl(propertySym);
                if (propertySym.SlotSymWithType.IsNotNull)
                {
                    PROPSYM implPropSym = propertySym.SlotSymWithType.PropSym;
                    PrepareAccessor(propertySym.GetMethodSym, propertySym, implPropSym.GetMethodSym);
                    PrepareAccessor(propertySym.SetMethodSym, propertySym, implPropSym.SetMethodSym);
                }
                else
                {
                    if (propertySym.GetMethodSym != null)
                    {
                        string name = CreateAccessorName(propertySym.ExpImplErrorSym.ErrorName, "get_");
                        propertySym.GetMethodSym.ExpImplErrorSym =
                            Compiler.MainSymbolManager.GetErrorType(propertySym.ExpImplErrorSym.ParentSym, name, null);
                    }
                    if (propertySym.SetMethodSym != null)
                    {
                        string name = CreateAccessorName(propertySym.ExpImplErrorSym.ErrorName, "set_");
                        propertySym.SetMethodSym.ExpImplErrorSym =
                            Compiler.MainSymbolManager.GetErrorType(propertySym.ExpImplErrorSym.ParentSym, name, null);
                    }
                }

                DebugUtil.Assert(
                    propertySym.GetMethodSym== null ||
                    propertySym.GetMethodSym.SlotSymWithType.IsNotNull ||
                    propertySym.GetMethodSym.ExpImplErrorSym != null);
                DebugUtil.Assert(
                    propertySym.SetMethodSym== null ||
                    propertySym.SetMethodSym.SlotSymWithType.IsNotNull ||
                    propertySym.SetMethodSym.ExpImplErrorSym != null);

                return;
            }

            SymWithType hiddenSwt = new SymWithType();
            SymWithType ambigSwt = new SymWithType();
            bool needImpl = false;

            if (!propertySym.IsOverride)
            {
                CheckSimpleHiding(propertySym, treeNode.Flags);
                CheckForProtectedInSealed(propertySym);
                DefinePropertyAccessors(propertySym, basePwt);
            }
            else if (!FindSymHiddenByMethPropAgg(
                propertySym, aggTypeSym.BaseClassSym, aggTypeSym, hiddenSwt, ambigSwt,ref needImpl))
            {
                // Didn't find a hidden base symbol to override.
                propertySym.IsOverride = false;
                Compiler.ErrorRef(null, CSCERRID.ERR_OverrideNotExpected, new ErrArgRef(propertySym));
                DefinePropertyAccessors(propertySym, basePwt);
            }
            else if (!hiddenSwt.Sym.IsPROPSYM)
            {
                // Found a non-property we will hide. We should not have 'override'.
                propertySym.IsOverride = false;
                Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideNonProperty,
                    new ErrArgRef(propertySym), new ErrArgRef(hiddenSwt));
                CheckHiddenSymbol(propertySym, hiddenSwt);
                DefinePropertyAccessors(propertySym, basePwt);
            }
            else
            {
                // Found a property of same signature that we will override.
                DebugUtil.Assert(
                    (!hiddenSwt.PropSym.IsOverride) == (hiddenSwt.PropSym.SlotSymWithType.IsNull));

                if (hiddenSwt.PropSym.SlotSymWithType.IsNotNull)
                {
                    propertySym.SlotSymWithType.Set(
                        hiddenSwt.PropSym.SlotSymWithType.PropSym,
                        Compiler.MainSymbolManager.SubstType(
                            hiddenSwt.PropSym.SlotSymWithType.AggTypeSym,
                            hiddenSwt.AggTypeSym,
                            null) as AGGTYPESYM);
                }
                else
                {
                    propertySym.SlotSymWithType = hiddenSwt;
                }

                if (ambigSwt.IsNotNull)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_AmbigOverride,
                        new ErrArgRefOnly(propertySym),
                        new ErrArgRef(hiddenSwt),
                        new ErrArgRef(ambigSwt),
                        new ErrArgRef(aggTypeSym));
                }

                SymWithType slotSwt = propertySym.SlotSymWithType;
                DebugUtil.Assert(!slotSwt.PropSym.IsOverride);
                basePwt = PropWithType.Convert(slotSwt);

                // Check for bogus.
                if (slotSwt.PropSym.HasBogus && slotSwt.PropSym.CheckBogus())
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideBogusMethod,
                        new ErrArgRef(propertySym), new ErrArgRef(slotSwt));
                    // Don't look for accessors.
                    basePwt.Clear();
                }

                // Access must match.
                if (slotSwt.PropSym.Access != propertySym.Access)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_CantChangeAccessOnOverride,
                        new ErrArgRef(propertySym),
                        new ErrArgRef(Compiler.ErrAccess(slotSwt.PropSym.Access)),
                        new ErrArgRef(slotSwt));
                    // Don't look for accessors.
                    basePwt.Clear();
                }

                // Return type must match.
                if (!Compiler.MainSymbolManager.SubstEqualTypes(
                    propertySym.ReturnTypeSym, slotSwt.PropSym.ReturnTypeSym, slotSwt.AggTypeSym,null))
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_CantChangeTypeOnOverride,
                        new ErrArgRef(propertySym),
                        new ErrArgRef(slotSwt),
                        new ErrArgNoRef(Compiler.MainSymbolManager.SubstType(
                            slotSwt.PropSym.ReturnTypeSym, slotSwt.AggTypeSym,null)));
                    // Don't look for accessors.
                    basePwt.Clear();
                }

                if (!propertySym.IsDeprecated() && slotSwt.PropSym.IsDeprecated())
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.WRN_NonObsoleteOverridingObsolete,
                        new ErrArgRef(propertySym),
                        new ErrArgRef(slotSwt));
                }

                DefinePropertyAccessors(propertySym, basePwt);

                // Check overriden get is a valid match to override.
                if (propertySym.GetMethodSym != null)
                {
                    if (basePwt.IsNotNull)
                    {
                        CheckValidAccessorOverride(
                            propertySym.GetMethodSym,
                            propertySym,
                            slotSwt.PropSym.GetMethodSym,
                            CSCERRID.ERR_NoGetToOverride);
                    }
                    else
                    {
                        propertySym.GetMethodSym.IsOverride = false;
                    }
                }

                // Check overridden set is a valid match to override.
                if (propertySym.SetMethodSym != null)
                {
                    if (basePwt.IsNotNull)
                    {
                        CheckValidAccessorOverride(
                            propertySym.SetMethodSym,
                            propertySym,
                            slotSwt.PropSym.SetMethodSym,
                            CSCERRID.ERR_NoSetToOverride);
                    }
                    else
                    {
                        propertySym.SetMethodSym.IsOverride = false;
                    }
                }
            }

            // NOTE: inner classes can derive from outer classes so private virtual properties are OK.

            // Check that new virtual accessors aren't in sealed classes.
            if (aggTypeSym.IsSealed && !propertySym.IsOverride)
            {
                if (propertySym.GetMethodSym != null && propertySym.GetMethodSym.IsVirtual)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_NewVirtualInSealed,
                        new ErrArgRef(propertySym.GetMethodSym), new ErrArgRef(aggTypeSym));
                }
                if (propertySym.SetMethodSym != null && propertySym.SetMethodSym.IsVirtual)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_NewVirtualInSealed,
                        new ErrArgRef(propertySym.SetMethodSym), new ErrArgRef(aggTypeSym));
                }
            }

            // Check that abstract properties are in abstract classes.
            if (!aggTypeSym.IsAbstract)
            {
                if (propertySym.GetMethodSym != null && propertySym.GetMethodSym.IsAbstract)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_AbstractInConcreteClass,
                        new ErrArgRef(propertySym.GetMethodSym), new ErrArgRef(aggTypeSym));
                }
                if (propertySym.SetMethodSym != null && propertySym.SetMethodSym.IsAbstract)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_AbstractInConcreteClass,
                        new ErrArgRef(propertySym.SetMethodSym), new ErrArgRef(aggTypeSym));
                }
            }

            // check that the property body existence matches with the abstractness
            // attribute of the property
            PrepareAccessor(propertySym.GetMethodSym, propertySym, null);
            PrepareAccessor(propertySym.SetMethodSym, propertySym, null);
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareAccessor
        //
        /// <summary></summary>
        /// <param name="accessorMethSym"></param>
        /// <param name="propertySym"></param>
        /// <param name="implAccessorMethSym"></param>
        //------------------------------------------------------------
        private void PrepareAccessor(
            METHSYM accessorMethSym,
            PROPSYM propertySym,
            METHSYM implAccessorMethSym)
        {
            DebugUtil.Assert(propertySym != null);
            DebugUtil.Assert(propertySym.ClassSym.IsSource);

            if (propertySym.Name != null)
            {
                if (accessorMethSym == null)
                {
                    return;
                }

                ACCESSORNODE accNode = accessorMethSym.ParseTreeNode as ACCESSORNODE;

                if (accessorMethSym.IsAbstract || accessorMethSym.IsExternal)
                {
                    //if ((accessorMethSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                    if ((accNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                    {
                        // found an abstract property with a body
                        Compiler.ErrorRef(
                            null,
                            accessorMethSym.IsAbstract ?
                                CSCERRID.ERR_AbstractHasBody : CSCERRID.ERR_ExternHasBody,
                            new ErrArgRef(accessorMethSym));
                    }
                }
                else
                {
                    //if ((accessorMethSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0)
                    if ((accNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0 &&
                        !accNode.IsAutoImplemented)
                    {
                        // found non-abstract property without body
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_ConcreteMissingBody,
                            new ErrArgRef(accessorMethSym));
                    }
                }
                return;
            }

            // Explicit interface impl.
            DebugUtil.Assert(propertySym.SlotSymWithType.IsNotNull);
            SymWithType implSwt = propertySym.SlotSymWithType;

            if (accessorMethSym == null)
            {
                if (implAccessorMethSym != null)
                {
                    SymWithType swt = new SymWithType(implAccessorMethSym, implSwt.AggTypeSym);
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.ERR_ExplicitPropertyMissingAccessor,
                        new ErrArgRef(propertySym),
                        new ErrArgRef(swt));
                }
                return;
            }

            accessorMethSym.IsMetadataVirtual = true;

            // Must have a body.
            if ((accessorMethSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0 &&
                !accessorMethSym.IsExternal)
            {
                Compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_ConcreteMissingBody,
                    new ErrArgRef(accessorMethSym));
            }
            else if (
                accessorMethSym.IsExternal &&
                (accessorMethSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
            {
                // found an extern method with a body
                Compiler.ErrorRef(null, CSCERRID.ERR_ExternHasBody, new ErrArgRef(accessorMethSym));
            }

            // Must have same accessors as interface member.
            if (implAccessorMethSym == null)
            {
                Compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_ExplicitPropertyAddingAccessor,
                    new ErrArgRef(accessorMethSym),
                    new ErrArgRef(implSwt));

                string name = CreateAccessorName(
                    implSwt.Sym.Name,
                    accessorMethSym == propertySym.GetMethodSym ? "get_" : "set_");
                accessorMethSym.ExpImplErrorSym = Compiler.MainSymbolManager.GetErrorType(
                    implSwt.AggTypeSym, name, null);
            }
            else
            {
                accessorMethSym.SlotSymWithType.Set(implAccessorMethSym, implSwt.AggTypeSym);
                accessorMethSym.NeedsMethodImp = true;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckValidAccessorOverride
        //
        /// <summary></summary>
        /// <param name="accessorMethSym"></param>
        /// <param name="propSym"></param>
        /// <param name="baseMethSym"></param>
        /// <param name="errNone"></param>
        //------------------------------------------------------------
        private void CheckValidAccessorOverride(
            METHSYM accessorMethSym,
            PROPSYM propSym,
            METHSYM baseMethSym,
            CSCERRID errNone)
        {
            DebugUtil.Assert(propSym != null);
            DebugUtil.Assert(propSym.SlotSymWithType.IsNotNull);
            DebugUtil.Assert(accessorMethSym != null && accessorMethSym.IsAnyAccessor);
            DebugUtil.Assert(!propSym.IsExplicitImplementation);  // cannot be explicit impl.
            DebugUtil.Assert(propSym.IsOverride && accessorMethSym.IsOverride);

            if (baseMethSym == null ||
                !CheckAccess(baseMethSym, propSym.SlotSymWithType.AggTypeSym, propSym.ClassSym, null))
            {
                Compiler.ErrorRef(
                    null,
                    errNone,
                    new ErrArgRef(accessorMethSym),
                    new ErrArgRef(propSym.SlotSymWithType));
                accessorMethSym.IsOverride = false;
                return;
            }

            accessorMethSym.SlotSymWithType.Set(baseMethSym, propSym.SlotSymWithType.AggTypeSym);

            // Can't change access on override.
            if (accessorMethSym.Access != baseMethSym.Access)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_CantChangeAccessOnOverride,
                    new ErrArgRef(accessorMethSym),
                    new ErrArgRef(Compiler.ErrAccess(baseMethSym.Access)),
                    new ErrArgRef(accessorMethSym.SlotSymWithType));
            }

            accessorMethSym.CModifierCount = baseMethSym.CModifierCount;
            propSym.CModifierCount += accessorMethSym.CModifierCount;

            // Check that accessor doesn't hide a sealed accessor....
            SymWithType tmpSwt = new SymWithType();

            AGGSYM aggSym = propSym.ClassSym;
            bool needMethImpl = false;

            if (FindSymHiddenByMethPropAgg(
                    accessorMethSym,
                    aggSym.BaseClassSym,
                    aggSym,
                    tmpSwt,
                    null,
                    ref needMethImpl))
            {
                // FindSymHiddenByMethProp filters out non-accessors (but sets needMethImpl when found).
                DebugUtil.Assert(tmpSwt.Sym.IsMETHSYM && tmpSwt.MethSym.IsAnyAccessor);

                // it must be virtual
                if (!tmpSwt.MethSym.IsVirtual)
                {
                    if (tmpSwt.MethSym.IsOverride)
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_CantOverrideSealed,
                            new ErrArgRef(accessorMethSym),
                            new ErrArgRef(tmpSwt));
                    }
                    else
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_CantOverrideNonVirtual,
                            new ErrArgRef(accessorMethSym),
                            new ErrArgRef(tmpSwt));
                    }
                }
                else
                {
                    // Walk the inheritance tree and check that there are no sealed properties in between.
                    // This will catch the case where a type in the inheritance hierarchy seals the property
                    // but that property does not implement this accessor.
                    // In that case, sealing the property seals both accessors.

                    // Note:  this is just one half of the fix,
                    // we should also emit the second accessor on sealed properties
                    // which only override one of the accessors.

                    SYM currentSym = propSym;
                    AGGTYPESYM currentAts = aggSym.GetThisType();
                    while ((currentSym = FindNextName(propSym.Name, ref currentAts, currentSym))!=null)
                    {
                        if (!currentSym.IsPROPSYM) continue;

                        PROPSYM currentPropSym = currentSym as PROPSYM;
                        if ((currentPropSym.GetMethodSym != null &&
                            !currentPropSym.GetMethodSym.IsVirtual &&
                            currentPropSym.GetMethodSym.IsOverride) ||
                            (currentPropSym.SetMethodSym != null &&
                            !currentPropSym.SetMethodSym.IsVirtual &&
                            currentPropSym.SetMethodSym.IsOverride))
                        {
                            Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideSealed,
                                new ErrArgRef(accessorMethSym),
                                new ErrArgRef(tmpSwt),
                                new ErrArgRef(currentPropSym));
                            break;
                        }

                        if (currentPropSym == tmpSwt.MethSym.PropertySym)
                            break;
                    }
                    DebugUtil.Assert(currentSym != null);

                    // if this is an override then we should always find the accessor
                    // it is overriding in one of the base types.

                    if (needMethImpl)
                    {
                        accessorMethSym.NeedsMethodImp = true;
                        accessorMethSym.IsNewSlot = true;
                    }
                }
            }

            CheckLinkDemandOnOverride(accessorMethSym, MethWithType.Convert(accessorMethSym.SlotSymWithType));
        }

        //------------------------------------------------------------
        // CLSDREC.SetAccessorOverride
        //
        /// <summary></summary>
        /// <param name="accessorMethSym"></param>
        /// <param name="eventSym"></param>
        /// <param name="baseMethSym"></param>
        //------------------------------------------------------------
        private void SetAccessorOverride(METHSYM accessorMethSym, EVENTSYM eventSym, METHSYM baseMethSym)
        {
            DebugUtil.Assert(eventSym != null);
            DebugUtil.Assert(eventSym.SlotEventWithType.IsNotNull);
            DebugUtil.Assert(accessorMethSym != null && accessorMethSym.IsAnyAccessor);
            DebugUtil.Assert(baseMethSym != null && baseMethSym.IsAnyAccessor);
            DebugUtil.Assert(!eventSym.IsExpImpl);  // cannot be explicit impl.
            DebugUtil.Assert(eventSym.IsOverride);

            accessorMethSym.IsOverride = true;
            accessorMethSym.SlotSymWithType.Set(baseMethSym, eventSym.SlotEventWithType.AggTypeSym);
            accessorMethSym.CModifierCount = baseMethSym.CModifierCount;

            AGGSYM aggSym = eventSym.ClassSym;
            bool needMethImpl = false;
            SymWithType tmpSwt = new SymWithType();
            bool fT;
            fT = FindSymHiddenByMethPropAgg(
                accessorMethSym,
                aggSym.BaseClassSym,
                aggSym,
                tmpSwt,
                null,
                ref needMethImpl);

            DebugUtil.Assert(
                fT &&
                tmpSwt.Sym.IsMETHSYM &&
                tmpSwt.MethSym.IsAnyAccessor &&
                (tmpSwt.MethSym == baseMethSym || tmpSwt.MethSym.SlotSymWithType.Sym == baseMethSym));

            if (needMethImpl)
            {
                accessorMethSym.NeedsMethodImp = true;
                accessorMethSym.IsNewSlot= true;
            }
            CheckLinkDemandOnOverride(accessorMethSym, MethWithType.Convert(accessorMethSym.SlotSymWithType));
        }

        //------------------------------------------------------------
        // CLSDREC.prepareEvent
        //
        /// <summary>
        /// do prepare stage for an event in a class or struct (not interface)
        /// </summary>
        /// <param name="eventSym"></param>
        //------------------------------------------------------------
        private void PrepareEvent(EVENTSYM eventSym)
        {
            DebugUtil.Assert(eventSym.ClassSym.IsSource);
            DebugUtil.Assert(eventSym.SlotEventWithType.IsNull);

            AGGSYM aggSym = eventSym.ClassSym;
            BASENODE treeNode = eventSym.ParseTreeNode;

            // baseEwt is where to look for accessor names.
            EventWithType baseEwt = new EventWithType(null, null);

            // For imported types we don't really know if it's a delegate until we've imported members.
            Compiler.EnsureState(eventSym.TypeSym, AggStateEnum.DefinedMembers);

            // Issue error if the eventSym type is not a delegate.
            if (!eventSym.TypeSym.IsDelegateType())
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_EventNotDelegate, new ErrArgRef(eventSym));
            }

            if (eventSym.IsExpImpl)
            {
                // event must be an explicit implementation. 
                DefineEventAccessors(eventSym, baseEwt);

                CheckExplicitImpl(eventSym);
                if (eventSym.SlotEventWithType.IsNotNull)
                {
                    // Set up the accessors as explicit interfaces.
                    eventSym.AddMethodSym.SlotSymWithType.Set(
                        eventSym.SlotEventWithType.EventSym.AddMethodSym,
                        eventSym.SlotEventWithType.AggTypeSym);
                    eventSym.AddMethodSym.NeedsMethodImp = true;
                    eventSym.AddMethodSym.IsMetadataVirtual = true;
                    if (eventSym.AddMethodSym.SlotSymWithType == null)
                    {
                        string name = CreateAccessorName(eventSym.SlotEventWithType.Sym.Name, "add_");
                        eventSym.AddMethodSym.ExpImplErrorSym = Compiler.MainSymbolManager.GetErrorType(
                            eventSym.SlotEventWithType.AggTypeSym, name, null);
                    }
                    eventSym.RemoveMethodSym.SlotSymWithType.Set(
                        eventSym.SlotEventWithType.EventSym.RemoveMethodSym, eventSym.SlotEventWithType.AggTypeSym);
                    eventSym.RemoveMethodSym.NeedsMethodImp = true;
                    eventSym.RemoveMethodSym.IsMetadataVirtual = true;
                    if (eventSym.RemoveMethodSym.SlotSymWithType == null)
                    {
                        string name = CreateAccessorName(eventSym.SlotEventWithType.Sym.Name, "remove_");
                        eventSym.RemoveMethodSym.ExpImplErrorSym = Compiler.MainSymbolManager.GetErrorType(
                            eventSym.SlotEventWithType.AggTypeSym, name, null);
                    }
                }
                else
                {
                    string name = CreateAccessorName(eventSym.ExpImplErrorSym.ErrorName, "add_");
                    eventSym.AddMethodSym.ExpImplErrorSym = Compiler.MainSymbolManager.GetErrorType(
                        eventSym.ExpImplErrorSym.ParentSym, name, null);
                    name = CreateAccessorName(eventSym.ExpImplErrorSym.ErrorName, "remove_");
                    eventSym.RemoveMethodSym.ExpImplErrorSym = Compiler.MainSymbolManager.GetErrorType(
                        eventSym.ExpImplErrorSym.ParentSym, name, null);
                }

                DebugUtil.Assert(
                    eventSym.AddMethodSym.SlotSymWithType.IsNotNull ||
                    eventSym.AddMethodSym.ExpImplErrorSym != null);
                DebugUtil.Assert(
                    eventSym.RemoveMethodSym.SlotSymWithType.IsNotNull ||
                    eventSym.RemoveMethodSym.ExpImplErrorSym != null);
            }
            else
            {
                SymWithType hiddenSwt = new SymWithType();

                // not explicit event implementation.
                if (!eventSym.IsOverride)
                {
                    CheckSimpleHiding(eventSym, eventSym.GetParseFlags());
                    CheckForProtectedInSealed(eventSym);
                    DefineEventAccessors(eventSym, baseEwt);
                }
                else if (!FindAnyHiddenSymbol(eventSym.Name, aggSym.BaseClassSym, aggSym, hiddenSwt))
                {
                    // didn't find base member to override.
                    eventSym.IsOverride = false;
                    Compiler.ErrorRef(null, CSCERRID.ERR_OverrideNotExpected, new ErrArgRef(eventSym));
                    DefineEventAccessors(eventSym, baseEwt);
                }
                else if (!hiddenSwt.Sym.IsEVENTSYM)
                {
                    // Found a non-event. "override" is in error.
                    eventSym.IsOverride = false;
                    Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideNonEvent,
                        new ErrArgRef(eventSym), new ErrArgRef(hiddenSwt));
                    DefineEventAccessors(eventSym, baseEwt);
                }
                else
                {
                    // Found an event that we will override.
                    DebugUtil.Assert(
                        !hiddenSwt.EventSym.IsOverride == hiddenSwt.EventSym.SlotEventWithType.IsNull);

                    if (hiddenSwt.EventSym.SlotEventWithType.IsNotNull)
                    {
                        eventSym.SlotEventWithType.Set(
                            hiddenSwt.EventSym.SlotEventWithType.EventSym,
                            Compiler.MainSymbolManager.SubstType(
                                hiddenSwt.EventSym.SlotEventWithType.AggTypeSym,
                                hiddenSwt.AggTypeSym,
                                null) as AGGTYPESYM);
                    }
                    else
                    {
                        eventSym.SlotEventWithType = EventWithType.Convert(hiddenSwt);
                    }

                    EventWithType slotEwt = eventSym.SlotEventWithType;
                    baseEwt = slotEwt;

                    if (slotEwt.EventSym.HasBogus && slotEwt.EventSym.CheckBogus())
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideBogusMethod,
                            new ErrArgRef(eventSym), new ErrArgRef(slotEwt));
                        // Don't look for accessors.
                        baseEwt.Clear();
                    }
                    else if (hiddenSwt.EventSym != slotEwt.EventSym &&
                        hiddenSwt.EventSym.HasBogus &&
                        hiddenSwt.EventSym.CheckBogus())
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideBogusMethod,
                            new ErrArgRef(eventSym), new ErrArgRef(hiddenSwt));
                        // Don't look for accessors.
                        baseEwt.Clear();
                    }
                    else
                    {
                        // Check that access is the same.
                        if (slotEwt.EventSym.Access != eventSym.Access)
                        {
                            Compiler.ErrorRef(null, CSCERRID.ERR_CantChangeAccessOnOverride,
                                new ErrArgRef(eventSym),
                                new ErrArgRef(Compiler.ErrAccess(slotEwt.EventSym.Access)),
                                new ErrArgRef(slotEwt));
                            // Don't look for accessors.
                            baseEwt.Clear();
                        }

                        // Make sure both accessors are virtual.
                        if (!hiddenSwt.EventSym.AddMethodSym.IsVirtual ||
                            !hiddenSwt.EventSym.RemoveMethodSym.IsVirtual)
                        {
                            if (hiddenSwt.EventSym.AddMethodSym.IsOverride ||
                                hiddenSwt.EventSym.RemoveMethodSym.IsOverride)
                            {
                                Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideSealed,
                                    new ErrArgRef(eventSym), new ErrArgRef(hiddenSwt));
                            }
                            else
                            {
                                Compiler.ErrorRef(null, CSCERRID.ERR_CantOverrideNonVirtual,
                                    new ErrArgRef(eventSym), new ErrArgRef(hiddenSwt));
                            }
                        }

                        if (!eventSym.IsDeprecated() && slotEwt.EventSym.IsDeprecated())
                        {
                            Compiler.ErrorRef(null, CSCERRID.WRN_NonObsoleteOverridingObsolete,
                                new ErrArgRef(eventSym), new ErrArgRef(slotEwt));
                        }

                        // Make sure type of the event is the same.
                        if (!Compiler.MainSymbolManager.SubstEqualTypes(
                            eventSym.TypeSym,
                            slotEwt.EventSym.TypeSym,
                            slotEwt.AggTypeSym,
                            null))
                        {
                            Compiler.ErrorRef(null, CSCERRID.ERR_CantChangeTypeOnOverride,
                                new ErrArgRef(eventSym),
                                new ErrArgRef(slotEwt),
                                new ErrArgNoRef(Compiler.MainSymbolManager.SubstType(
                                    slotEwt.EventSym.TypeSym,
                                    slotEwt.AggTypeSym,
                                    null)));
                            // Don't look for accessors.
                            baseEwt.Clear();
                        }
                    }

                    DefineEventAccessors(eventSym, baseEwt);
                    DebugUtil.Assert(
                        eventSym.AddMethodSym != null && eventSym.RemoveMethodSym != null);

                    if (baseEwt.IsNotNull)
                    {
                        // Set the override bits.
                        SetAccessorOverride(
                            eventSym.AddMethodSym,
                            eventSym,
                            baseEwt.EventSym.AddMethodSym);
                        SetAccessorOverride(
                            eventSym.RemoveMethodSym,
                            eventSym,
                            baseEwt.EventSym.RemoveMethodSym);
                    }
                }
            }
            DebugUtil.Assert(eventSym.AddMethodSym != null && eventSym.RemoveMethodSym != null);

            // Check that abstract-ness matches with whether have a body for each accessor.
            if (treeNode.Kind != NODEKIND.VARDECL)
            {
                if (eventSym.AddMethodSym.IsAbstract || eventSym.AddMethodSym.IsExternal)
                {
                    if ((eventSym.AddMethodSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                    {
                        // found an abstract eventSym with a body
                        Compiler.ErrorRef(
                            null,
                            eventSym.AddMethodSym.IsAbstract ? CSCERRID.ERR_AbstractHasBody : CSCERRID.ERR_ExternHasBody,
                            new ErrArgRef(eventSym.AddMethodSym));
                    }
                }
                else
                {
                    if ((eventSym.AddMethodSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0)
                    {
                        // found non-abstract event without body
                        Compiler.ErrorRef(null, CSCERRID.ERR_ConcreteMissingBody, new ErrArgRef(eventSym.AddMethodSym));
                    }
                }
                if (eventSym.RemoveMethodSym.IsAbstract || eventSym.RemoveMethodSym.IsExternal)
                {
                    if ((eventSym.RemoveMethodSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) == 0)
                    {
                        // found an abstract event with a body
                        Compiler.ErrorRef(null,
                            eventSym.RemoveMethodSym.IsAbstract ? CSCERRID.ERR_AbstractHasBody : CSCERRID.ERR_ExternHasBody,
                            new ErrArgRef(eventSym.RemoveMethodSym));
                    }
                }
                else
                {
                    if ((eventSym.RemoveMethodSym.ParseTreeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0)
                    {
                        // found non-abstract event without body
                        Compiler.ErrorRef(null, CSCERRID.ERR_ConcreteMissingBody, new ErrArgRef(eventSym.RemoveMethodSym));
                    }
                }
            }

            // Check that virtual/abstractness is OK.
            if (aggSym.IsSealed &&
                eventSym.AddMethodSym.IsVirtual &&
                !eventSym.AddMethodSym.IsOverride)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_NewVirtualInSealed,
                    new ErrArgRef(eventSym), new ErrArgRef(aggSym));
            }

            if (!aggSym.IsAbstract && eventSym.AddMethodSym.IsAbstract)
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_AbstractInConcreteClass,
                    new ErrArgRef(eventSym), new ErrArgRef(aggSym));
            }
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareInterface
        //
        /// <summary>
        /// does the prepare stage for an interface.
        /// This checks for conflicts between members in inherited interfaces
        /// and checks for conflicts between members in this interface
        /// and inherited members.
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void PrepareInterface(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.IsPreparing);

            // this must be done before preparing our members
            // because they may derive from us, which requires that we
            // are prepared.

            aggSym.SetBogus(false);
            aggSym.AggState = AggStateEnum.Prepared;

            if (aggSym.IsSource)
            {
                // prepare members

                for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
                {

                    Compiler.SetLocation(child);//SETLOCATIONSYM(child);

                    switch (child.Kind)
                    {
                        case SYMKIND.METHSYM:
                            if ((child as METHSYM).IsAnyAccessor)
                            {
                                // accessors are handled by their property/event
                                break;
                            }
                            // fallthrough
                            goto case SYMKIND.PROPSYM;

                        case SYMKIND.PROPSYM:
                            PrepareInterfaceMember(child as METHPROPSYM);
                            break;

                        case SYMKIND.TYVARSYM:
                        case SYMKIND.AGGTYPESYM:
                            // type based on this type.
                            break;

                        case SYMKIND.EVENTSYM:
                            DefineEventAccessors(child as EVENTSYM, new EventWithType(null, null));
                            CheckIfaceHiding(child, (child as EVENTSYM).ParseTreeNode.ParentNode.Flags);
                            // For imported types we don't really know if it's a delegate until we've imported members. 
                            Compiler.EnsureState((child as EVENTSYM).TypeSym, AggStateEnum.DefinedMembers);
                            // Issue error if the event type is not a delegate.
                            if (!(child as EVENTSYM).TypeSym.IsDelegateType())
                            {
                                Compiler.ErrorRef(null, CSCERRID.ERR_EventNotDelegate, new ErrArgRef(child as EVENTSYM));
                            }
                            break;

                        default:
                            DebugUtil.Assert(false, "Unknown node type");
                            break;
                    }
                }
            }
            aggSym.AggState = AggStateEnum.PreparedMembers;
        }

        //------------------------------------------------------------
        // CLSDREC.CheckExplicitImpl
        //
        /// <summary>
        /// The sym must be a property, method or event that was specified in source as an explicit
        /// interface member implementation. This is signalled by the sym.name == NULL. This sets the
        /// SymWithType member with the actual interface member being implemented (if one is found).
        /// If none is found, an ERRORSYM is constructed and stashed so we can manufacture the member name
        /// at will.
        /// </summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        private void CheckExplicitImpl(SYM sym)
        {
            DebugUtil.Assert(sym.Name == null);

            string name = null;
            BASENODE ifaceNode = null;
            TYPESYM returnTypeSym = null;
            TypeArray paramArray = null;
            SymWithType ifaceSymWithType = null;
            AGGDECLSYM aggDeclSym = null;
            SYMBMASK symMask = sym.Mask;

            bool isMethProp = false;

            switch (sym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad sym kind in CheckExplicitImpl");
                    return;

                case SYMKIND.PROPSYM:
                    if ((sym as PROPSYM).IsEvent)
                    {
                        symMask = SYMBMASK.EVENTSYM;
                    }
                    ifaceNode = (sym as PROPSYM).ParseTreeNode.AsANYPROPERTY.NameNode;
                    //goto LMethProp;
                    isMethProp = true;
                    break;

                case SYMKIND.METHSYM:
                    // Accessors shouldn't come through here!
                    DebugUtil.Assert(!(sym as METHSYM).IsAnyAccessor);

                    ifaceNode = ((sym as METHSYM).ParseTreeNode as METHODNODE).NameNode;
                    (sym as METHPROPSYM).NeedsMethodImp = true;
                    isMethProp = true;
                    break;

                case SYMKIND.EVENTSYM:
                    EVENTSYM eventSym = sym as EVENTSYM;
                    ifaceNode = eventSym.ParseTreeNode.AsANYPROPERTY.NameNode;
                    paramArray = null;
                    returnTypeSym = eventSym.TypeSym;
                    ifaceSymWithType = eventSym.SlotEventWithType;
                    aggDeclSym = eventSym.ContainingAggDeclSym;
                    DebugUtil.Assert(eventSym.ExpImplErrorSym == null);
                    break;
            }

            if (isMethProp)
            {
                //LMethProp:
                METHPROPSYM mpSym = sym as METHPROPSYM;
                paramArray = mpSym.ParameterTypes;
                if (paramArray.Count > 0 && sym.IsPROPSYM)
                {
                    name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.INDEXERINTERNAL);
                }
                returnTypeSym = mpSym.ReturnTypeSym;
                ifaceSymWithType = mpSym.SlotSymWithType;
                aggDeclSym = mpSym.ContainingAggDeclSym;
                DebugUtil.Assert(mpSym.ExpImplErrorSym == null);
            }

            // If the name isn't set above, get it from ifaceNode.
            if (name == null)
            {
                name = ifaceNode.AsDOT.Operand2.AsANYNAME.Name;
                ifaceNode = ifaceNode.AsDOT.Operand1;
            }
            DebugUtil.Assert(ifaceSymWithType.Sym == null && ifaceSymWithType.AggTypeSym == null);

            TYPESYM ifaceTypeSym = TypeBind.BindNameToType(
                Compiler,
                ifaceNode,
                aggDeclSym,
                TypeBindFlagsEnum.None);
            DebugUtil.Assert(ifaceTypeSym != null);

            if (ifaceTypeSym.IsERRORSYM)
            {
                // Interface didn't exist, error already reported.
                goto LMakeErrorSym;
            }
            if (!ifaceTypeSym.IsInterfaceType())
            {
                Compiler.Error(ifaceNode, CSCERRID.ERR_ExplicitInterfaceImplementationNotInterface,
                    new ErrArgNameNode(ifaceNode, ErrArgFlagsEnum.None), new ErrArgRefOnly(ifaceTypeSym));
                goto LMakeErrorSym;
            }

            AGGTYPESYM ifaceAggTypeSym = ifaceTypeSym as AGGTYPESYM;
            AGGSYM ifaceAggSym = ifaceAggTypeSym.GetAggregate();

            // Check that we implement the interface in question.
            if (!aggDeclSym.AggSym.AllInterfaces.Contains(ifaceAggTypeSym))
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_ClassDoesntImplementInterface,
                    new ErrArgRef(sym), new ErrArgRef(ifaceAggTypeSym));
                PrepareAggregate(ifaceAggSym);
            }
            DebugUtil.Assert(ifaceAggSym.IsPrepared);

            // For a method we need to map method type variables.
            TypeArray methTypeArray = sym.IsMETHSYM ? (sym as METHSYM).TypeVariables : null;

            SYM ifaceMemberSym;
            for (ifaceMemberSym = Compiler.MainSymbolManager.LookupAggMember(name, ifaceAggSym, symMask);
                ifaceMemberSym != null;
                ifaceMemberSym = BSYMMGR.LookupNextSym(ifaceMemberSym, ifaceMemberSym.ParentSym, symMask))
            {
                DebugUtil.Assert(ifaceMemberSym.Kind == sym.Kind);

                if (ifaceMemberSym.IsEVENTSYM)
                {
                    if (Compiler.MainSymbolManager.SubstEqualTypes(
                        returnTypeSym,
                        (ifaceMemberSym as EVENTSYM).TypeSym,
                        ifaceAggTypeSym,
                        null))
                    {
                        break;
                    }
                }
                else
                {
                    // Need the correct arity, param types and return type.
                    if ((!ifaceMemberSym.IsMETHSYM || (ifaceMemberSym as METHSYM).TypeVariables.Count == methTypeArray.Count) &&
                        Compiler.MainSymbolManager.SubstEqualTypeArrays(
                            paramArray, (ifaceMemberSym as METHPROPSYM).ParameterTypes, ifaceAggTypeSym, methTypeArray) &&
                        Compiler.MainSymbolManager.SubstEqualTypes(
                            returnTypeSym, (ifaceMemberSym as METHPROPSYM).ReturnTypeSym, ifaceAggTypeSym, methTypeArray))
                    {
                        break;
                    }
                }
            }

            if (ifaceMemberSym == null)
            {
                if (ifaceNode.ParentNode.Kind == NODEKIND.DOT)
                {
                    Compiler.Error(ifaceNode.ParentNode, CSCERRID.ERR_InterfaceMemberNotFound,
                        new ErrArgNameNode(ifaceNode.ParentNode, ErrArgFlagsEnum.None), new ErrArgRefOnly(ifaceAggTypeSym));
                }
                else
                {
                    Compiler.Error(ifaceNode, CSCERRID.ERR_InterfaceMemberNotFound,
                        new ErrArgRef(name), new ErrArgRefOnly(ifaceAggTypeSym));
                }
                goto LMakeErrorSym;
            }

            SymWithType ifaceSwt = new SymWithType(ifaceMemberSym, ifaceAggTypeSym);

            // Check for duplicate explicit implementation.
            SYM dupSym = FindExplicitInterfaceImplementation(aggDeclSym.AggSym.GetThisType(), ifaceSwt);
            if (dupSym != null)
            {
                if (ifaceNode.ParentNode.Kind == NODEKIND.DOT)
                {
                    Compiler.Error(ifaceNode.ParentNode, CSCERRID.ERR_MemberAlreadyExists,
                        new ErrArgNameNode(ifaceNode.ParentNode, ErrArgFlagsEnum.None),
                        new ErrArg(aggDeclSym.AggSym),
                        new ErrArgRefOnly(dupSym));
                }
                else
                {
                    Compiler.Error(ifaceNode, CSCERRID.ERR_MemberAlreadyExists,
                        new ErrArg(name),
                        new ErrArg(aggDeclSym.AggSym),
                        new ErrArgRefOnly(dupSym));
                }
                goto LMakeErrorSym;
            }

            if (sym.IsMETHSYM && ifaceSwt.MethSym.IsAnyAccessor)
            {
                (sym as METHSYM).ExpImplErrorSym = Compiler.MainSymbolManager.GetErrorType(ifaceTypeSym, name, null);
                Compiler.ErrorRef(null, CSCERRID.ERR_ExplicitMethodImplAccessor,
                    new ErrArgRef(sym), new ErrArgRef(ifaceSwt));
                return;
            }

            ifaceSymWithType = ifaceSwt;
            switch (sym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad sym kind in CheckExplicitImpl");
                    return;

                case SYMKIND.PROPSYM:
                case SYMKIND.METHSYM:
                    (sym as METHPROPSYM).SlotSymWithType = ifaceSwt;
                    break;

                case SYMKIND.EVENTSYM:
                    (sym as EVENTSYM).SlotEventWithType = EventWithType.Convert(ifaceSwt);
                    break;
            }

            if (ifaceMemberSym.HasBogus && ifaceMemberSym.CheckBogus())
            {
                Compiler.ErrorRef(null, CSCERRID.ERR_BogusExplicitImpl,
                    new ErrArgRef(sym), new ErrArgRef(ifaceSymWithType));
                return;
            }

            if (sym.IsMETHSYM)
            {
                if ((sym as METHSYM).IsParameterArray && !(ifaceMemberSym as METHSYM).IsParameterArray)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_ExplicitImplParams,
                        new ErrArgRef(sym), new ErrArgRef(ifaceSymWithType));
                }

                DebugUtil.Assert(
                    (sym as METHSYM).TypeVariables != null &&
                    (ifaceMemberSym as METHSYM).TypeVariables != null &&
                    (sym as METHSYM).TypeVariables.Count == (ifaceMemberSym as METHSYM).TypeVariables.Count);

                if ((sym as METHSYM).TypeVariables.Count > 0)
                {
                    SetOverrideConstraints(sym as METHSYM);
                }
            }
            return;

        LMakeErrorSym:
            DebugUtil.Assert(ifaceSymWithType.IsNull);
            ERRORSYM err = Compiler.MainSymbolManager.GetErrorType(ifaceTypeSym, name, null);
            switch (sym.Kind)
            {
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                    (sym as METHPROPSYM).ExpImplErrorSym = err;
                    break;

                case SYMKIND.EVENTSYM:
                    (sym as EVENTSYM).ExpImplErrorSym = err;
                    break;

                default:
                    DebugUtil.Assert(false, "Shouldn't be here");
                    break;
            }
            return;
        }

        //------------------------------------------------------------
        // CLSDREC.SetOverrideConstraints
        //
        /// <summary>
        /// Set the constraints for this override or explicit impl method
        /// from the constraints for the base method.
        /// </summary>
        /// <param name="overMethSym"></param>
        //------------------------------------------------------------
        private void SetOverrideConstraints(METHSYM overMethSym)
        {
            DebugUtil.Assert(overMethSym.TypeVariables.Count > 0);
            DebugUtil.Assert(
                overMethSym.SlotSymWithType.IsNotNull &&
                overMethSym.TypeVariables.Count == overMethSym.SlotSymWithType.MethSym.TypeVariables.Count);

            MethWithType baseMwt = MethWithType.Convert(overMethSym.SlotSymWithType);
            int index;

            for (index = 0; index < overMethSym.TypeVariables.Count; ++index)
            {
                TYVARSYM dstTvSym = overMethSym.TypeVariables[index] as TYVARSYM;
                TYVARSYM srcTvSym = baseMwt.MethSym.TypeVariables[index] as TYVARSYM;

                dstTvSym.Constraints = srcTvSym.Constraints;
                TypeArray boundArray = srcTvSym.BoundArray;
                boundArray = Compiler.MainSymbolManager.SubstTypeArray(
                    boundArray,
                    baseMwt.AggTypeSym,
                    overMethSym.TypeVariables);
                Compiler.EnsureState(boundArray, AggStateEnum.Bounds);
                Compiler.SetBounds(dstTvSym, boundArray, true);
                DebugUtil.Assert(!dstTvSym.FResolved());
            }

            for (index = 0; index < overMethSym.TypeVariables.Count; ++index)
            {
                TYVARSYM dstTvSym = overMethSym.TypeVariables[index] as TYVARSYM;
                Compiler.ResolveBounds(dstTvSym, true);
                TYPESYM baseTypeSym = dstTvSym.AbsoluteBaseTypeSym;

                if (dstTvSym.IsValueType() && dstTvSym.IsReferenceType())
                {
                    DebugUtil.Assert(
                        !baseTypeSym.IsValueType() ||
                        dstTvSym.HasReferenceConstraint());
                    Compiler.Error(
                        CSCERRID.ERR_BaseConstraintConflict,
                        new ErrArgRef(dstTvSym),
                        new ErrArg(baseTypeSym),
                        new ErrArg(baseTypeSym.IsValueType() ? "class" : "struct"));
                }
                else if (
                    baseTypeSym.IsNUBSYM && (dstTvSym.HasValueConstraint() ||
                    dstTvSym.HasReferenceConstraint()))
                {
                    Compiler.Error(
                        CSCERRID.ERR_BaseConstraintConflict,
                        new ErrArgRef(dstTvSym),
                        new ErrArg(baseTypeSym),
                        new ErrArg(dstTvSym.HasReferenceConstraint() ? "class" : "struct"));
                }
            }
        }

        //------------------------------------------------------------
        // bool CLSDREC::CheckStructLayout(AGGTYPESYM * ats, LayoutFrame * pframeOwner)
        //------------------------------------------------------------
        // Check the layout of the struct type for cycles. Return false iff there are cycles in the layout
        // (indicating an error condition).
        //
        // Here is the rule that we "would like to" implement:
        //
        //     Let G be the smallest directed graph such that:
        //
        //     (1) ats is in G.
        //     (2) Whenever S is in G, the instance type of S is in G and there is a directed edge from S
        //         to the instance type of S.
        //     (3) Whenever S is in G and S has a struct valued field of type T then T is in G.
        //     (4) Whenever S is in G and S has a struct valued _instance_ field of type T then there is
        //         a (directed) edge from S to T.
        //
        //     It is an error for G to be infinite or for G to contain a (directed) cycle.
        //
        // Unfortunately, the runtime disallows more:
        //
        //     Let G* be the smallest directed graph such that:
        //
        //     (1) ats is in G*.
        //     (2) Whenever S is in G*, the instance type of S is in G* and there is a directed edge from S
        //         to the instance type of S.
        //     (3) Whenever S is in G* and S has a struct valued field of type T then T is in G* and
        //         there is a (directed) edge from S to T.
        //
        //     It is an error for G* to be infinite or for G* to contain a (directed) cycle.
        //
        // Note that the vertices of G* are the same as the vertices of G. Thus G* is infinite iff
        // G is infinite. Note also that the edges of G are all edges of G*. So G* produces an
        // error whenever G produces an error.
        //
        // It is actually much easier to implement G* than to implement G. This is because of the
        // following facts (proof left to the reader):
        //
        //     (1) There is a path from ats to any vertex of G*.
        //     (2) If G* is infinite then G* contains a cycle (follows from 1).
        //     (3) If G* contains a cycle then it contains a cycle starting (and ending) at an instance type.
        //
        // By walking the instance type edge from S before walking any other edges from S, we are
        // guaranteed to detect infinite graphs in finite time (since the set of instance types is
        // finite).
        //
        // We walk G* as follows:
        //
        //     (1) If S is not an instance type, visit the instance type of S first.
        //     (2) If S is an instance type, check whether we're already processing this type
        //         in a calling stack frame. If so, we have a cycle in G*.
        //     (3) For each struct valued field of S, visit the type of the field.
        //
        // To implement G instead of G* we'd have to do the following:
        //
        //     Once you find a cycle in G* determine whether it is an error:
        //
        //         * If there are no static fields in the cycle then it is also a cycle in G (an error).
        //         * If there are static fields in the cycle but no "instance type" edges then we have
        //           a cycle in G* but not an infinite graph yet. So keep looking.
        //         * If there are static fields and instance type edges in the cycle, then G is infinite
        //           (an error).
        //
        // The problem is when we keep looking, we don't know when to stop and where to look.
        // It's easy to end up in infinite recursion.... So we're taking the simpler approach (since
        // that's all the runtime supports anyway).
        //
        // Here are some examples of the strange possibilities one can get with generics:
        //
        //     // (1) Infinite recursion detectable just by checking AGGSYMs.
        //     A<T> { A<A<T>> }
        //
        //     // (2) Infinite recursion detectable just by checking AGGSYMs.
        //     A<T> { B<A<T>> }
        //     B<T> { A<B<T>> }
        //
        //     // (3) Requires tracking AGGTYPESYM and doing substitutions.
        //     A<T> { B<A<T>> }
        //     B<T> { T }
        //
        //     // (4) Valid code where checking for duplicate AGGSYMs gives a false error.
        //     A<T> { B<B<T>> }
        //     B<T> { T }
        //
        //     // (5) Infinite recursion not detectable by just checking AGGSYMs.
        //     A<T> { B<A<A<T>>> }
        //     B<T> { T }
        //
        // Here's an alternate statement of the algorithm:
        //
        //     * If ats is an instance type, check whether we're already processing ats in a calling
        //       stack frame. If so, we have a cycle in G*. Note that we check for the AGGTYPESYM,
        //       not just AGGSYM to avoid a false error for example (4). Note that we could perform this
        //       check even if the ats is not an instance type, but doing so would never find any errors.
        //       If the type is already being checked then when we checked the instance type we would
        //       have seen a cycle as well.
        //
        //     * For each field, get the actual field type (after substitution). If it's not a struct type
        //       ignore the field. If it is a struct type:
        //
        //         * Check the associated instance type first. This avoids infinite recusion in examples
        //           (1) and (2).
        //
        //         * Check the actual type. This finds cycles caused by substitution as in examples (3).
        //           This together with checking the instance type first catches example (5). For that
        //           example we first check A<T>, then recurse to B<T> which returns true, then recurse
        //           to B<A<A<T>>>, then recurse to A<T> (the instance type of A<A<T>>) and detect the
        //           error.

        //------------------------------------------------------------
        // CLSDREC.CheckStructLayout
        //
        /// <summary>
        /// Check the layout of the struct type for cycles.
        /// Return false iff there are cycles in the layout.
        /// </summary>
        /// <param name="aggTypeSym"></param>
        /// <param name="ownerFrame"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool CheckStructLayout(AGGTYPESYM aggTypeSym, LayoutFrame ownerFrame)
        {
            DebugUtil.Assert(aggTypeSym != null && aggTypeSym.IsStructType());

            Compiler.EnsureState(aggTypeSym, AggStateEnum.Prepared);
            if (aggTypeSym.IsPredefined() && aggTypeSym.AllTypeArguments.Count == 0)
            {
                return true;
            }

            AGGSYM aggSym = aggTypeSym.GetAggregate();

            // This should always be called on the instance type before any other instantiation.
            DebugUtil.Assert(aggSym.LayoutChecked || aggTypeSym == aggSym.GetThisType());

            // If we haven't checked the layout yet, fLayoutError should be clear.
            DebugUtil.Assert(!aggSym.LayoutErrorOccurred || aggSym.LayoutChecked);

            if (aggTypeSym == aggSym.GetThisType())
            {
                if (aggSym.LayoutChecked)
                {
                    return !aggSym.LayoutErrorOccurred;
                }

                // See if we're already checking this type.
                LayoutFrame prevFrame = null;
                for (LayoutFrame curFrame = ownerFrame; curFrame != null; curFrame = curFrame.OwnerFrame)
                {
                    curFrame.ChildFrame = prevFrame;
                    prevFrame = curFrame;
                    if (aggTypeSym == curFrame.SymWithType.AggTypeSym)
                    {
                        // Found a cycle. Spit out all the fields in the chain.
                        for (; curFrame != null; curFrame = curFrame.ChildFrame)
                        {
                            Compiler.Error(
                                CSCERRID.ERR_StructLayoutCycle,
                                new ErrArgRef(curFrame.SymWithType),
                                new ErrArg(curFrame.FieldAggTypeSym));
                        }
                        // We'll set the fLayoutChecked and fLayoutError bits in the calling frame.
                        return false;
                    }
                }
            }

            bool isError = true; // Assume an error. Cleared below.

            LayoutFrame layoutFrame = new LayoutFrame();
            layoutFrame.OwnerFrame = ownerFrame;
            layoutFrame.SymWithType.AggTypeSym = aggTypeSym;

            // Resolve the layout for all of our non-static fields.
            for (
                layoutFrame.SymWithType.Sym = aggSym.FirstChildSym;
                layoutFrame.SymWithType.Sym != null;
                layoutFrame.SymWithType.Sym = layoutFrame.SymWithType.Sym.NextSym)
            {
                // Ignore non-fields and statics.
                if (!(layoutFrame.SymWithType.Sym is MEMBVARSYM))
                {
                    continue;
                }

                // Get the true type and ignore non-structs.
                TYPESYM fieldTypeSym = Compiler.MainSymbolManager.SubstType(
                    (layoutFrame.SymWithType.Sym as MEMBVARSYM).TypeSym,
                    aggTypeSym,
                    null);
                if (fieldTypeSym is NUBSYM)
                {
                    fieldTypeSym = (fieldTypeSym as NUBSYM).GetAggTypeSym();
                    if (fieldTypeSym == null)
                    {
                        continue;
                    }
                }
                if (!fieldTypeSym.IsStructType())
                {
                    continue;
                }

                // Static fields of the exact same type are apparently OK.
                if ((layoutFrame.SymWithType.Sym as MEMBVARSYM).IsStatic && fieldTypeSym == aggTypeSym)
                {
                    continue;
                }

                layoutFrame.FieldAggTypeSym = fieldTypeSym as AGGTYPESYM;
                fieldTypeSym = layoutFrame.FieldAggTypeSym.GetAggregate().GetThisType();

                if (!CheckStructLayout(fieldTypeSym as AGGTYPESYM, layoutFrame))
                {
                    goto LDone;
                }
                if (layoutFrame.FieldAggTypeSym != fieldTypeSym &&
                    !CheckStructLayout(layoutFrame.FieldAggTypeSym, layoutFrame))
                {
                    goto LDone;
                }
            }

            isError = false;

        LDone:
            if (aggTypeSym == aggSym.GetThisType())
            {
                DebugUtil.Assert(!aggSym.LayoutChecked && !aggSym.LayoutErrorOccurred);
                aggSym.LayoutChecked = true;
                aggSym.LayoutErrorOccurred = isError;
            }
            return !isError;
        }

        //private string getMethodConditional(METHSYM method);

        //------------------------------------------------------------
        // CLSDREC.ReportHiding
        //
        /// <summary>
        /// <para>reports the results of a symbol hiding another symbol</para>
        /// <para>Report that "new" or "override" is required or not.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="hiddenSymWithType"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void ReportHiding(SYM sym, SymWithType hiddenSymWithType, NODEFLAGS flags)
        {
            // Pretend like Destructors are not called "Finalize"
            if (hiddenSymWithType != null &&
                (hiddenSymWithType.Sym == null ||
                (hiddenSymWithType.Sym.IsMETHSYM && hiddenSymWithType.MethSym.IsDtor)))
            {
                hiddenSymWithType = null;
            }

            // check the shadowing flags are correct

            if (hiddenSymWithType == null)
            {
                if (sym.IsAGGSYM)
                {
                    // Warn on any that have new set.
                    for (AGGDECLSYM decl = (sym as AGGSYM).FirstDeclSym;
                        decl != null;
                        decl = decl.NextDeclSym)
                    {
                        if ((decl.ParseTreeNode.Flags & NODEFLAGS.MOD_NEW) != 0)
                        {
                            Compiler.ErrorRef(
                                null,
                                CSCERRID.WRN_NewNotRequired,
                                new ErrArgRef(decl));
                        }
                    }
                }
                else if ((flags & NODEFLAGS.MOD_NEW) != 0)
                {
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.WRN_NewNotRequired,
                        new ErrArgRef(sym));
                }
                return;
            }

            if (sym.IsAGGSYM)
            {
                // Warn if none have new set.
                for (AGGDECLSYM decl = (sym as AGGSYM).FirstDeclSym; ; decl = decl.NextDeclSym)
                {
                    if (decl == null)
                    {
                        Compiler.ErrorRef(null, CSCERRID.WRN_NewRequired,
                            new ErrArgRef(sym), new ErrArgRef(hiddenSymWithType));
                        break;
                    }
                    if ((decl.ParseTreeNode.Flags & NODEFLAGS.MOD_NEW) != 0)
                    {
                        break;
                    }
                }
            }
            else if ((flags & NODEFLAGS.MOD_NEW) == 0)
            {
                Compiler.ErrorRef(null,
                    (hiddenSymWithType.Sym.IsVirtual() &&
                    sym.Kind == hiddenSymWithType.Sym.Kind &&
                    !(sym.ParentSym as AGGSYM).IsInterface) ?
                        CSCERRID.WRN_NewOrOverrideExpected : CSCERRID.WRN_NewRequired,
                    new ErrArgRef(sym),
                    new ErrArgRef(hiddenSymWithType));
            }
            if (!(sym.ParentSym as AGGSYM).IsInterface)
            {
                CheckHiddenSymbol(sym, hiddenSymWithType);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckSimpleHiding
        //
        /// <summary>
        /// checks the simple hiding issues for non-override members
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void CheckSimpleHiding(SYM sym, NODEFLAGS flags)
        {
            // find an accessible member with the same name in
            // a base class of our enclosing class

            AGGSYM aggSym = sym.ParentSym as AGGSYM;
            SymWithType swtHid = new SymWithType();

            if (sym.IsMETHPROPSYM || sym.IsAGGSYM)
            {
                bool needsMethImpl = false;
                FindSymHiddenByMethPropAgg(
                    sym,
                    aggSym.BaseClassSym,
                    aggSym,
                    swtHid,
                    null,
                    ref needsMethImpl);
            }
            else
            {
                FindAnyHiddenSymbol(sym.Name, aggSym.BaseClassSym, aggSym, swtHid);
            }

            // report the results
            ReportHiding(sym, swtHid, flags);
        }

        //------------------------------------------------------------
        // CLSDREC.CheckIfaceHiding
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void CheckIfaceHiding(SYM sym, NODEFLAGS flags)
        {
            if (!sym.IsUserCallable()) return;

            // Ctors don't hide and shouldn't be in interfaces anyway.
            DebugUtil.Assert(!sym.IsMETHSYM || !(sym as METHSYM).IsCtor && !(sym as METHSYM).IsOperator);
            AGGSYM aggSym = sym.ParentSym as AGGSYM;
            TypeArray allIfaceArray = aggSym.AllInterfaces;

            if (allIfaceArray.Count == 0)
            {
                // Check for gratuitous new.
                ReportHiding(sym, null, flags);
                return;
            }

            SYMKIND symKind = sym.Kind;
            bool isIndexer = (symKind == SYMKIND.PROPSYM) && (sym as PROPSYM).IsIndexer;
            TypeArray paramArray = (isIndexer || symKind == SYMKIND.METHSYM) ? (sym as METHPROPSYM).ParameterTypes : null;

            // Initialize the hide bits.
            for (int i = 0; i < allIfaceArray.Count; i++)
            {
                AGGTYPESYM iface = allIfaceArray[i] as AGGTYPESYM;
                DebugUtil.Assert(iface.IsInterfaceType());
                iface.AllHidden = false;
                iface.DiffHidden = false;
            }

            // Loop through the interfaces.
            for (int index = 0; index < allIfaceArray.Count; ++index)
            {
                AGGTYPESYM currentAts = allIfaceArray[index] as AGGTYPESYM;
                DebugUtil.Assert(currentAts != null && currentAts.IsInterfaceType());

                if (currentAts.AllHidden)
                {
                    continue;
                }

                bool hideAll = false;
                bool hideDiff = false;

                // Loop through the symbols.
                for (SYM currentSym = Compiler.MainSymbolManager.LookupAggMember(
                    sym.Name, currentAts.GetAggregate(), SYMBMASK.ALL);
                    currentSym != null;
                    currentSym = BSYMMGR.LookupNextSym(currentSym, currentAts.GetAggregate(), SYMBMASK.ALL))
                {
                    if (symKind != currentSym.Kind)
                    {
                        // Some things (ctors, operators and indexers) don't interact based on name!
                        DebugUtil.Assert(
                            !currentSym.IsMETHSYM ||
                            !(currentSym as METHSYM).IsCtor && !(currentSym as METHSYM).IsOperator);

                        if (isIndexer || currentSym.IsPROPSYM && (currentSym as PROPSYM).IsIndexer)
                        {
                            continue;
                        }
                        if (currentAts.DiffHidden)
                        {
                            // This one hides everything in base types.
                            hideAll = true;
                            continue;
                        }
                    }
                    else
                    {
                        switch (symKind)
                        {
                            case SYMKIND.AGGSYM:
                                if ((currentSym as AGGSYM).TypeVariables.Count != (sym as AGGSYM).TypeVariables.Count)
                                {
                                    hideDiff = true;
                                    continue;
                                }
                                break;

                            case SYMKIND.METHSYM:
                                DebugUtil.Assert(
                                    !(currentSym as METHSYM).IsCtor &&
                                    !(currentSym as METHSYM).IsOperator);
                                if ((currentSym as METHSYM).TypeVariables.Count != (sym as METHSYM).TypeVariables.Count ||
                                    !Compiler.MainSymbolManager.SubstEqualTypeArrays(
                                        paramArray,
                                        (currentSym as METHSYM).ParameterTypes,
                                        currentAts.AllTypeArguments,
                                        (sym as METHSYM).TypeVariables,
                                        SubstTypeFlagsEnum.NormNone))
                                {
                                    hideDiff = true;
                                    continue;
                                }
                                break;

                            case SYMKIND.PROPSYM:
                                if (isIndexer && (!(currentSym as PROPSYM).IsIndexer ||
                                    !Compiler.MainSymbolManager.SubstEqualTypeArrays(
                                        paramArray,
                                        (currentSym as PROPSYM).ParameterTypes,
                                        currentAts.AllTypeArguments,
                                        null,
                                        SubstTypeFlagsEnum.NormNone)))
                                {
                                    continue;
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    SymWithType hiddenSwt = new SymWithType(currentSym, currentAts);
                    ReportHiding(sym, hiddenSwt, flags);
                    return;
                }

                // Done with the current type. Mark base interfaces appropriately.
                if (hideAll || hideDiff)
                {
                    // Mark base interfaces appropriately.
                    TypeArray tmpIfaces = currentAts.GetIfacesAll();
                    for (int i = 0; i < tmpIfaces.Count; i++)
                    {
                        AGGTYPESYM tmpType = tmpIfaces[i] as AGGTYPESYM;
                        DebugUtil.Assert(tmpType.IsInterfaceType());

                        if (hideAll)
                        {
                            tmpType.AllHidden = true;
                        }
                        tmpType.DiffHidden = true;
                    }
                }
            }

            // Check for gratuitous new.
            ReportHiding(sym, null, flags);
        }

        //------------------------------------------------------------
        // CLSDREC.FindAnyHiddenSymbol
        //
        /// <summary>
        /// find an inherited member which is hidden by name
        /// </summary>
        /// <param name="name">
        ///  (sscli) name - name to find hidden member
        /// </param>
        /// <param name="startAggTypeSym">
        /// (sscli) typeStart - class to start the search in
        /// </param>
        /// <param name="aggSym">
        /// (sscli) agg - context to search from for access checks
        /// </param>
        /// <param name="symWithType">
        /// (sscli) ptypeFound - out parameter indicating where the sym was found
        /// </param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool FindAnyHiddenSymbol(
            string name,
            AGGTYPESYM startAggTypeSym,
            AGGSYM aggSym,
            SymWithType symWithType)
        {
            for (SYM curSym = null;
                (curSym = FindNextAccessibleName(name, ref startAggTypeSym, aggSym, curSym, true, false)) != null;
                )
            {
                switch (curSym.Kind)
                {
                    case SYMKIND.METHSYM:
                        if ((curSym as METHSYM).TypeVariables.Count > 0)
                        {
                            continue;
                        }
                        // Accessors don't count. They're not really there....
                        if ((curSym as METHSYM).IsAnyAccessor)
                        {
                            continue;
                        }
                        break;

                    case SYMKIND.AGGSYM:
                        if ((curSym as AGGSYM).TypeVariables.Count > 0) continue;
                        break;

                    default:
                        break;
                }
                symWithType.Set(curSym, startAggTypeSym);
                return true;
            }

            symWithType.Set(null, null);
            return false;
        }

        //------------------------------------------------------------
        // CLSDREC.NeedExplicitImpl
        //
        /// <summary>
        /// checks if we need a compiler generated explicit method impl
        /// returns the actual method implementing the interface method
        /// </summary>
        /// <param name="ifaceMethWithType"></param>
        /// <param name="implMethSym"></param>
        /// <param name="aggSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM NeedExplicitImpl(
            MethWithType ifaceMethWithType,
            METHSYM implMethSym,
            AGGSYM aggSym)
        {
            DebugUtil.Assert(
                ifaceMethWithType.AggTypeSym != null &&
                ifaceMethWithType.MethSym.ClassSym == ifaceMethWithType.AggTypeSym.GetAggregate());
            CheckLinkDemandOnOverride(implMethSym, ifaceMethWithType);

            if (!implMethSym.IsExplicitImplementation &&
                (ifaceMethWithType.MethSym.CModifierCount > 0 || implMethSym.CModifierCount > 0 ||   // differing signatures
                    ifaceMethWithType.MethSym.Name != implMethSym.Name ||              // can happen when implementing properties
                    !implMethSym.IsMetadataVirtual && !implMethSym.GetInputFile().IsSource))
            {

                // This is a compiler-generated method, so it doesn't matter which class declaration we use that it was declared
                // in, hence it is OK and reasonable to use DeclFirst here.
                IFACEIMPLMETHSYM impl = Compiler.MainSymbolManager.CreateIfaceImplMethod(aggSym, aggSym.FirstDeclSym);

                ifaceMethWithType.MethSym.CopyInto(impl, ifaceMethWithType.AggTypeSym, Compiler);
                impl.Access = ACCESS.PRIVATE;
                impl.IsOverride = false;
                impl.CModifierCount = ifaceMethWithType.MethSym.CModifierCount;
                impl.SlotSymWithType = ifaceMethWithType;
                impl.NeedsMethodImp = true;
                impl.ImplMethSym = implMethSym;
                impl.ParseTreeNode = aggSym.FirstDeclSym.ParseTreeNode; // DeclFirst is OK, see comment above.
                impl.IsAbstract = false;
                impl.IsMetadataVirtual = true;
                impl.ContainingAggDeclSym = aggSym.FirstDeclSym;

                return impl;
            }
            else
            {
                implMethSym.IsMetadataVirtual = true;
                return implMethSym;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.FindEntryPoint
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void FindEntryPoint(AGGSYM aggSym)
        {
            // We've found the specified class, now let's look for a Main
            METHSYM methSym;

            methSym = Compiler.MainSymbolManager.LookupAggMember(
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.MAIN),
                aggSym,
                SYMBMASK.METHSYM) as METHSYM;

            while (methSym != null)
            {
                // Must be public, static, void/int Main ()
                // with no args or String[] args
                // If you change this code also change the code in EMITTER::FindEntryPointInClass
                // (it does basically the same thing)
                if (methSym.IsStatic && !methSym.IsPropertyAccessor)
                {
                    // Check the signature.
                    if ((methSym.ReturnTypeSym.IsVoidType ||    //  == Compiler.MainSymbolManager.VoidSym ||
                        methSym.ReturnTypeSym.IsPredefType(PREDEFTYPE.INT)) &&
                        (methSym.ParameterTypes.Count == 0 ||
                        (methSym.ParameterTypes.Count == 1 && methSym.ParameterTypes[0].IsARRAYSYM &&
                            (methSym.ParameterTypes[0] as ARRAYSYM).ElementTypeSym.IsPredefType(PREDEFTYPE.STRING))))
                    {
                        // Make sure it's not generic.
                        if (methSym.TypeVariables.Count == 0 && aggSym.AllTypeVariables.Count == 0)
                        {
                            if (!methSym.IsPartialMethod || !methSym.HasNoBody) // (CS3)
                            {
                                Compiler.Emitter.SetEntryPoint(methSym);
                            }
                        }
                        else
                        {
                            Compiler.ErrorRef(null, CSCERRID.WRN_MainCantBeGeneric, new ErrArgRef(methSym));
                        }
                    }
                    else
                    {
                        Compiler.ErrorRef(null, CSCERRID.WRN_InvalidMainSig, new ErrArgRef(methSym));
                    }
                }

                SYM nextSym = methSym.NextSameNameSym;
                methSym = null;
                while (nextSym != null)
                {
                    if (nextSym.IsMETHSYM)
                    {
                        methSym = nextSym as METHSYM;
                        break;
                    }
                    nextSym = nextSym.NextSameNameSym;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.EmitTypedefsAggregate
        //
        /// <summary>
        /// Emit typedefs for this aggregates 
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void EmitTypedefsAggregate(AGGSYM aggSym)
        {
            OUTFILESYM outfileSym;
            AGGSYM baseAggSym;

            Compiler.SetLocation(aggSym);

            // If we've already hit this one (because it was a base of someone earlier),
            // then nothing more to do.
            if (aggSym.IsTypeDefEmitted)
            {
                //DebugUtil.Assert(Compiler.OptionManager.NoCodeGen || aggSym.Type != null);
                DebugUtil.Assert(aggSym.Type != null);
                return;             // already did it.
            }

            //DebugUtil.Assert(aggSym.TypeBuilder == null);
            if (aggSym.TypeBuilder != null)
            {
                return;
            }

            // Do base classes and base interfaces, if they are in the same output scope.
            outfileSym = aggSym.GetOutputFile();

            // Do the base class 
            baseAggSym = aggSym.GetBaseAgg();
            if (baseAggSym != null && baseAggSym.GetOutputFile() == outfileSym)
            {
                EmitTypedefsAggregate(baseAggSym);
            }

            // Iterate the base interfaces.
            for (int i = 0; i < aggSym.Interfaces.Count; i++)
            {
                AGGTYPESYM baseIface = aggSym.Interfaces[i] as AGGTYPESYM;
                baseAggSym = baseIface.GetAggregate();
                if (baseAggSym.GetOutputFile() == outfileSym)
                {
                    EmitTypedefsAggregate(baseAggSym);
                }
            }

            // we need to do outer classes before we do the nested classes
            if (aggSym.ParentBagSym.IsAGGSYM &&
                !(aggSym.ParentBagSym as AGGSYM).IsTypeDefEmitted &&
                (aggSym.ParentBagSym as AGGSYM).IsFabricated == aggSym.IsFabricated)
            {
                EmitTypedefsAggregate(aggSym.ParentBagSym as AGGSYM);
                DebugUtil.Assert(aggSym.IsTypeDefEmitted);
                return;
            }

            // It's possible that in doing our base classes or interfaces
            // They emitted our outer class, which in turn caused us to get
            // emitted, and so we're done
            if (aggSym.IsTypeDefEmitted)
            {
                DebugUtil.Assert(
                    aggSym.ParentBagSym.IsAGGSYM &&
                    (aggSym.ParentBagSym as AGGSYM).IsTypeDefEmitted);
                return;
            }

            // Do this aggregate.
            //if (!Compiler.Options.NoCodeGen)
            {
                Compiler.Emitter.EmitAggregateDef(aggSym);
            }
            aggSym.IsTypeDefEmitted = true;

            // Do child aggregates.
            for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
            {
                if (child is AGGSYM)
                {
                    EmitTypedefsAggregate(child as AGGSYM);
                }
            }

            DebugUtil.Assert(
                //Compiler.Options.NoCodeGen ||
                //aggSym.EmittedTypeToken != 0 ||
                aggSym.Type != null ||
                Compiler.ErrorCount() > 0);
        }

        //------------------------------------------------------------
        // CLSDREC.EmitMemberdefsAggregate
        //
        /// <summary>
        /// <para>Emit memberdefs for this aggregate</para>
        /// <para>The ordering of items here must EXACTLY match that in compileAggregate.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void EmitMemberdefsAggregate(AGGSYM aggSym)
        {
            OUTFILESYM outfileSym;
            AGGSYM baseAggSym;

            Compiler.SetLocation(aggSym);

            // If we've already hit this one (because it was a base of someone earlier),
            // then nothing more to do.
            if (aggSym.IsMemberDefsEmitted)
            {
                return;             // already did it.
            }

            // If we couldn't emit the class don't try to emit the members just exit.
            if (aggSym.TypeBuilder == null)
            {
                return;
            }

            // Do base classes and base interfaces, if they are in the same output scope.
            outfileSym = aggSym.GetOutputFile();

            // Do the base class
            baseAggSym = aggSym.GetBaseAgg();
            if (baseAggSym != null && baseAggSym.GetOutputFile() == outfileSym)
            {
                EmitMemberdefsAggregate(baseAggSym);
            }

            // Iterate the base interfaces.
            for (int i = 0; i < aggSym.Interfaces.Count; ++i)
            {
                AGGTYPESYM baseIface = aggSym.Interfaces[i] as AGGTYPESYM;
                baseAggSym = baseIface.GetAggregate();
                if (baseAggSym.GetOutputFile() == outfileSym)
                {
                    EmitMemberdefsAggregate(baseAggSym);
                }
            }

            // To do this in the same order as the Aggregate defs
            // we need to do outer classes before we do the nested classes
            if ((aggSym.ParentBagSym is AGGSYM) && !(aggSym.ParentBagSym as AGGSYM).IsMemberDefsEmitted)
            {
                EmitMemberdefsAggregate(aggSym.ParentBagSym as AGGSYM);
                DebugUtil.Assert(aggSym.IsMemberDefsEmitted);
                return;
            }

            if (aggSym.IsMemberDefsEmitted)
            {
                return;
            }

            // Do any special fields of this aggregate.
            Compiler.Emitter.EmitAggregateSpecialFields(aggSym);
            DebugUtil.Assert(!aggSym.IsMemberDefsEmitted);

            // Emit the children.
            EnumMembersInEmitOrder(aggSym, 0, MEMBER_OPERATION_EmitMemberDef);
            DebugUtil.Assert(!aggSym.IsMemberDefsEmitted);
            aggSym.IsMemberDefsEmitted = true;

            // Do child aggregates.

            for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
            {
                if (child.IsAGGSYM)
                {
                    EmitMemberdefsAggregate(child as AGGSYM);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.EmitBasesAggregate
        //
        /// <summary>
        /// Emit bases for this aggregates 
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void EmitBasesAggregate(AGGSYM aggSym)
        {
            OUTFILESYM outfileSym;
            AGGSYM baseAggSym;

            // If we've already hit this one (because it was a baseAggSym of someone earlier),
            // then nothing more to do.
            if (aggSym.IsBasesEmitted)
            {
                return;
            }

            Compiler.SetLocation(aggSym);

            // the class should already have a token generated for it...

            //DebugUtil.Assert(Compiler.Options.NoCodeGen || aggSym.Type != null);
            DebugUtil.Assert(aggSym.Type != null);
            // aggSym.tokenEmit);

            // Do baseAggSym classes and baseAggSym interfaces, if they are in the same output scope.
            outfileSym = aggSym.GetOutputFile();

            // Do the baseAggSym class 
            baseAggSym = aggSym.GetBaseAgg();
            if (baseAggSym != null && baseAggSym.GetOutputFile() == outfileSym)
            {
                EmitBasesAggregate(baseAggSym);
            }

            // Iterate the baseAggSym interfaces.
            for (int i = 0; i < aggSym.Interfaces.Count; ++i)
            {
                AGGTYPESYM baseIface = aggSym.Interfaces[i] as AGGTYPESYM;
                baseAggSym = baseIface.GetAggregate();
                if (baseAggSym.GetOutputFile() == outfileSym)
                {
                    EmitBasesAggregate(baseAggSym);
                }
            }

            // we need to do outer classes before we do the nested classes
            if ((aggSym.ParentBagSym is AGGSYM) &&
                !(aggSym.ParentBagSym as AGGSYM).IsBasesEmitted &&
                (aggSym.ParentBagSym as AGGSYM).IsFabricated == aggSym.IsFabricated)
            {
                EmitBasesAggregate(aggSym.ParentBagSym as AGGSYM);
                DebugUtil.Assert(aggSym.IsBasesEmitted);
                return;
            }

            if (aggSym.IsBasesEmitted)
            {
                return;
            }

            // Do this aggregate.
            //if (!Compiler.Options.NoCodeGen)
            {
                Compiler.Emitter.EmitAggregateBases(aggSym);
            }
            aggSym.IsBasesEmitted = true;

            // Do child aggregates.
            for (SYM childSym = aggSym.FirstChildSym; childSym != null; childSym = childSym.NextSym)
            {
                if (childSym is AGGSYM)
                {
                    EmitBasesAggregate(childSym as AGGSYM);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileAggregate
        //
        /// <summary>
        /// <para>compile all memebers of a class, this emits the class md,
        /// and loops through its children...</para>
        /// <para>The ordering of items here must EXACTLY match that in EmitMemberDefsAggregate.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        private void CompileAggregate(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.IsSource);
            Compiler.SetLocation(aggSym);

            TypeBuilder typeBuilder = null;

            // If we've already hit this one (because it was a base of someone earlier),
            // then nothing more to do.
            if (aggSym.IsCompiled || aggSym.IsFabricated)
            {
                typeBuilder = aggSym.TypeBuilder;
                // Fabricated types need to go through attrbind
                // to get the CompilerGeneratedAttribute emitted.
                DebugUtil.Assert(typeBuilder != null || Compiler.ErrorCount() > 0);
                // we may not have an emit token if there were errors

                if (aggSym.IsFabricated && typeBuilder != null)
                {
                    CompilerGeneratedAttrBind.EmitAttribute(Compiler, typeBuilder);
                }
                if (aggSym.IsFixedBufferStruct && typeBuilder != null)
                {
                    UnsafeValueTypeAttrBind.EmitAttribute(Compiler);    //, typeBuilder);
                }

                // Already did it (or shouldn't ever do it).
                //return;
                goto END_PROCESSING;
            }
            DebugUtil.Assert(aggSym.HasParseTree);

            // If we couldn't emit the class don't try to compile it (unless codegen is off, of course, in which case
            // we won't have a token even if everything was good).
            if (//!Compiler.Options.NoCodeGen &&
                typeBuilder != null)
            //(TypeFromToken(aggSym.tokenEmit) != mdtTypeDef || aggSym.tokenEmit == mdTypeDefNil)
            {
                //return;
                goto END_PROCESSING;
            }


            // Do base classes and base interfaces, if they are in the same output scope.
            OUTFILESYM outfileSym = aggSym.GetOutputFile();
            AGGSYM baseAggSym;

            // Do the base class
            baseAggSym = aggSym.GetBaseAgg();
            if (baseAggSym != null && baseAggSym.GetOutputFile() == outfileSym)
            {
                CompileAggregate(baseAggSym);
            }

            // Iterate the base interfaces.
            for (int i = 0; i < aggSym.Interfaces.Count; ++i)
            {
                AGGTYPESYM baseIface = aggSym.Interfaces[i] as AGGTYPESYM;
                baseAggSym = baseIface.GetAggregate();
                if (baseAggSym.GetOutputFile() == outfileSym)
                {
                    CompileAggregate(baseAggSym);
                }
            }

            // Do outer classes before nested classes
            if (aggSym.IsNested && !aggSym.GetOuterAgg().IsCompiled)
            {
                CompileAggregate(aggSym.GetOuterAgg());
            }

            if (aggSym.IsCompiled)
            {
                //return;
                goto END_PROCESSING;
            }

            AGGINFO aggInfo = new AGGINFO();
            //MEM_SET_ZERO(aggInfo);

            //--------------------------------------------------
            // Emit security attributes
            //--------------------------------------------------
            Dictionary<SecurityAction, PermissionSet> permissionSets
                = new Dictionary<SecurityAction, PermissionSet>();
            AggAttrBind.Compile(Compiler, aggSym, aggInfo, permissionSets);

            if (permissionSets.Count > 0)
            {
                Exception excp;
                if (!SecurityUtil.EmitSecurityAttributes(aggSym, permissionSets, out excp))
                {
                    compiler.Error(ERRORKIND.ERROR, excp);
                }
            }

            //--------------------------------------------------
            // Emit non-security attributes
            //--------------------------------------------------
            AggAttrBind.Compile(Compiler, aggSym, aggInfo, null);

            //--------------------------------------------------
            // compile & emit members
            //--------------------------------------------------
            EnumMembersInEmitOrder(aggSym, aggInfo, MEMBER_OPERATION_CompileMember);

            DebugUtil.Assert(
                AggStateEnum.Prepared <= aggSym.AggState &&
                aggSym.AggState < AggStateEnum.Compiled);
            aggSym.AggState = AggStateEnum.Compiled;

            //--------------------------------------------------
            // Nested classes must be done after other members.
            //--------------------------------------------------
            for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
            {
                if (child.IsAGGSYM)
                {
                    Compiler.SetLocation(child);
                    CompileAggregate(child as AGGSYM);
                }
            }

            // Do CLS name checking, after compiling members so we know who is Compliant
            if (Compiler.CheckForCLS &&
                Compiler.CheckSymForCLS(aggSym, false) &&
                aggSym.HasExternalAccess())
            {
                DebugUtil.Assert(Compiler.AllowCLSErrors());
                CheckCLSnaming(aggSym);

                if (aggSym.IsInterface)
                {
                    //EDMAURER Enforce rule that all CLS compliant interfaces
                    //derive only from CLS compliant interfaces. 
                    for (int i = 0; i < aggSym.Interfaces.Count; ++i)
                    {
                        AGGTYPESYM baseAts = aggSym.Interfaces[i] as AGGTYPESYM;
                        DebugUtil.Assert(baseAts.IsInterfaceType());
                        if (!Compiler.IsCLS_Type(aggSym, baseAts))
                        {
                            Compiler.Error(aggSym.GetSomeParseTree(), CSCERRID.WRN_CLS_BadInterface,
                                new ErrArg(aggSym), new ErrArg(baseAts));
                        }
                    }
                }

                if (aggSym.IsAttribute)
                {
                    bool foundCLSCtor = false;

                    for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
                    {
                        if (child.IsMETHSYM &&
                            child.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR) &&
                            child.HasExternalAccess() &&
                            (!child.HasCLSAttribute || child.IsCLS))
                        {
                            METHPROPSYM member = child as METHPROPSYM;
                            int i;
                            for (i = 0; i < member.ParameterTypes.Count; ++i)
                            {
                                if (member.ParameterTypes[i].IsARRAYSYM ||
                                    !IsAttributeType(member.ParameterTypes[i]))
                                {
                                    break;
                                }
                            }
                            if (i == member.ParameterTypes.Count)
                            {
                                foundCLSCtor = true;
                                break;
                            }
                        }
                    }

                    if (!foundCLSCtor)
                    {
                        Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadAttributeType, new ErrArgRef(aggSym));
                    }
                }
            }

            Compiler.FuncBRec.ResetUnsafe();

        END_PROCESSING: ;
        }

        //------------------------------------------------------------
        // CLSDREC.CompileMember
        //
        /// <summary>
        /// <para>The argument info must be AGGINFO type.</para>
        /// </summary>
        /// <remarks>
        ///  (sscli) private void CompileMember(SYM member, AGGINFO info)
        /// </remarks>
        /// <param name="member"></param>
        /// <param name="info"></param>
        //------------------------------------------------------------
        private void CompileMember(SYM member, object info)
        {
            AGGINFO aggInfo = info as AGGINFO;
            Compiler.SetLocation(member);
            switch (member.Kind)
            {
                case SYMKIND.MEMBVARSYM:
                    CompileField(member as MEMBVARSYM, aggInfo);
                    break;

                case SYMKIND.METHSYM:
                    CompileMethod(member as METHSYM, aggInfo);
                    Compiler.DiscardLocalState();
                    break;

                case SYMKIND.PROPSYM:
                    CompileProperty(member as PROPSYM, aggInfo);
                    break;

                case SYMKIND.EVENTSYM:
                    CompileEvent(member as EVENTSYM, aggInfo);
                    break;

                default:
                    DebugUtil.Assert(false, "Invalid member sym");
                    break;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileMethod
        //
        /// <summary>
        /// Compile a method...
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        private void CompileMethod(METHSYM methodSym, AGGINFO aggInfo)
        {
#if DEBUG || DEBUG2
            string debugClassName = methodSym.ClassSym.Name;
            string debugMethodName = methodSym.Name;
#endif
#if DEBUG
            int db_methodID = methodSym.SymID;
            bool debug_stop = false;
            if (db_methodID == -1)
            {
                debug_stop = true;
            }
            StringBuilder sb = new StringBuilder();
#endif
            // (CS3) partial method
            if (methodSym.IsPartialMethod && methodSym.HasNoBody)
            {
                return;
            }

            DebugUtil.Assert(Compiler.CompilationPhase >= CompilerPhaseEnum.CompileMembers);
            //DebugUtil.Assert(methodSym.MethodInfo == null);
            bool encBuild = Compiler.FEncBuild();

            if (encBuild && methodSym.IsExternal)
            {
                return;
            }

            BASENODE treeNode = methodSym.ParseTreeNode;
            DebugUtil.Assert(treeNode == null || !methodSym.ClassSym.IsFabricated);
            Compiler.FuncBRec.SetUnsafe(methodSym.IsUnsafe);

            METHINFO methInfo = new METHINFO();
            // methodSym.ParameterTypes is created by the initializer.
            if (methodSym.ParameterTypes != null && methodSym.ParameterTypes.Count > 0)
            {
                int pcount = methodSym.IsVarargs
                    ? methodSym.ParameterTypes.Count - 1
                    : methodSym.ParameterTypes.Count;
                for (int i = 0; i < pcount; ++i)
                {
                    methInfo.ParameterInfos.Add(new PARAMINFO());
                }
            }
            FillMethInfoCommon(methodSym, methInfo, aggInfo, false);

            int errNow = Compiler.ErrorCount();

            //--------------------------------------------------
            // parse the method body
            //--------------------------------------------------
            CInteriorTree interiorTree = null;

            if (treeNode != null)
            {
                Compiler.SetLocation(COMPILER.STAGE.INTERIORPARSE);
                //if ((treeNode.InGroup(NODEGROUP.INTERIOR)) &&
                //    FAILED(methodSym.GetInputFile().SourceData.GetInteriorParseTree (treeNode, &interiorTree)))
                //{
                //    Compiler.Error(treeNode, CSCERRID.FTL_NoMemory);     // The only possible failure
                //}
                //else if (interiorTree != null)
                //{
                //    CComPtr<ICSErrorContainer>  spErrors;
                //    // See if there were any errors in the interior parse
                //    if (SUCCEEDED(interiorTree.GetErrors(&spErrors)))
                //        controller().ReportErrorsToHost(spErrors);
                //    else
                //        Compiler.Error(treeNode, CSCERRID.FTL_NoMemory);  // Again, the only possible failure
                //}

                // OutOfMemoryException is catched in COMPILER.Compile method.

                if (treeNode.InGroup(NODEGROUP.INTERIOR) &&
                    !IsAutoImplementedAccessor(treeNode))   // CS3
                {
                    // Parse the method body. (call CParser.ParseBlock)
                    interiorTree = methodSym.GetInputFile().SourceData.GetInteriorParseTree(treeNode);
                }

                if (interiorTree != null)
                {
                    Controller.ReportErrors(interiorTree.GetErrors());
                }
            }

            CErrorSuppression es = new CErrorSuppression();
            bool isStaticCGCtor;

            if (Compiler.FAbortEarly(errNow, es))
            {
                goto LERROR;
            }

            isStaticCGCtor = (methodSym.IsStatic && methodSym.IsCompilerGeneratedCtor);

            if (!encBuild && !isStaticCGCtor)
            {
                // Emit the method info.
                Compiler.Emitter.EmitMethodInfo(methodSym, methInfo);
            }

            // Get the expr tree for methodSym body.
            EXPR bodyExpr;

            if (methodSym.ClassSym.IsDelegate)
            {
                // Delegate members are 'magic'. Their implementation is provided by the VOS.
                DebugUtil.Assert(methInfo.IsMagicImpl);
                bodyExpr = null;
            }
            else
            {
                Compiler.SetLocation(COMPILER.STAGE.COMPILE);

                if (!encBuild && !isStaticCGCtor)
                {
                    Dictionary<SecurityAction, PermissionSet> permissionSets
                        = new Dictionary<SecurityAction, PermissionSet>();
                    MethAttrBind.CompileAndEmit(
                        Compiler,
                        methodSym,
                        methInfo.EmitDebuggerHiddenAttribute,
                        permissionSets);

                    if (permissionSets.Count > 0)
                    {
                        Exception excp;
                        if (!SecurityUtil.EmitSecurityAttributes(
                            methodSym,
                            permissionSets,
                            out excp))
                        {
                            compiler.Error(ERRORKIND.ERROR, excp);
                        }
                    }
                }

#if DEBUG
                if (debug_stop)
                {
                    ;
                }
                sb.Length = 0;
                DebugUtil.DebugNodesOutput(sb);
                sb.Length = 0;
                DebugUtil.DebugSymsOutput(sb);
                sb.Length = 0;
                Compiler.MainSymbolManager.GlobalSymbolTable.Debug(sb);
                sb.Length = 0;
#endif
                //----------------------------------------------------
                // Compile the method body.
                //----------------------------------------------------
                Compiler.SetLocation(COMPILER.STAGE.BIND);

                // CS4 (RuntimeBinding)
                Compiler.FuncBRec.runtimeBindAnonAggSym = null;
#if DEBUG2
                DebugUtil.PrintBegin("Compile", debugClassName, debugMethodName);
#endif
                bodyExpr = Compiler.FuncBRec.CompileMethod(treeNode, methInfo, aggInfo, null);
#if DEBUG2
                DebugUtil.PrintEnd("Compile", debugClassName, debugMethodName);
#endif
#if DEBUG
                sb.Length = 0;
                DebugUtil.DebugNodesOutput(sb);
                sb.Length = 0;
                DebugUtil.DebugSymsOutput(sb);
                sb.Length = 0;
                Compiler.MainSymbolManager.GlobalSymbolTable.Debug(sb);
                sb.Length = 0;
                DebugUtil.DebugExprsOutput(sb);
#endif
                //----------------------------------------------------
                // (CS4) Emit Compiler.FuncBRec.runtimeBindAnonAggSym
                //----------------------------------------------------
                if (Compiler.FuncBRec.runtimeBindAnonAggSym != null)
                {
                    Compiler.FuncBRec.EmitRuntimeBindAnonymousClass();
                }

                //----------------------------------------------------
                // emit method def's for compiler generated constructors late
                //----------------------------------------------------
                if (isStaticCGCtor)
                {
                    if (bodyExpr == null &&
                        Compiler.OptionManager.Optimize) // if optimizing don't emit an empty cctor
                    {
                        goto LERROR;
                    }

                    Compiler.Emitter.EmitMethodDef(methodSym);
                    if (!encBuild)
                    {
                        // Emit the method info.
                        Compiler.Emitter.EmitMethodInfo(methodSym, methInfo);

                        Dictionary<SecurityAction, PermissionSet> permissionSets
                            = new Dictionary<SecurityAction, PermissionSet>();
                        MethAttrBind.CompileAndEmit(
                            Compiler,
                            methodSym,
                            methInfo.EmitDebuggerHiddenAttribute,
                            permissionSets);
                    }
                }

                if (Compiler.FAbortEarly(errNow, es))
                {
                    goto LERROR;
                }

                //----------------------------------------------------
                // Compile the anonymouse methods.
                //
                // Don't bother with anonymous methods if there have been any errors at all.
                // If this changes, fix up CompileAnonMeths to handle null params.
                //----------------------------------------------------
#if DEBUG
                sb.Length = 0;
                DebugUtil.DebugSymsOutput(sb);
                sb.Length = 0;
                DebugUtil.DebugExprsOutput(sb);
#endif
                if (methInfo.FirstAnonymousMethodInfo != null && !Compiler.FAbortCodeGen(0))
                {
                    bodyExpr = Compiler.FuncBRec.RewriteAnonDelegateBodies(
                        methodSym,
                        methInfo.OuterScopeSym,
                        methInfo.FirstAnonymousMethodInfo,
                        bodyExpr);
#if DEBUG
                    sb.Length = 0;
                    DebugUtil.DebugSymsOutput(sb);
                    sb.Length = 0;
                    DebugUtil.DebugExprsOutput(sb);
#endif

                    CompileAnonMeths(methInfo, treeNode, bodyExpr);
                }

                //----------------------------------------------------
                // Compile the iterators.
                //
                // Iterators are a little more tolerant of errors, but they can't handle unwritten nested
                // anonymous methods.  So don't do them if #1 there were errors in this method, or #2 there
                // were anonymous methods in this method that we didn't transform because of errors
                //----------------------------------------------------
                if (methInfo.IsIterator &&
                    !Compiler.FAbortCodeGen(methInfo.FirstAnonymousMethodInfo != null ? 0 : errNow))
                {
#if DEBUG
                    sb.Length = 0;
                    DebugUtil.DebugSymsOutput(sb);
                    sb.Length = 0;
                    DebugUtil.DebugExprsOutput(sb);
#endif

                    bodyExpr = CompileIterator(treeNode, bodyExpr, methInfo, aggInfo);
                    DebugUtil.Assert(methInfo.IteratorInfo == null);
                    // info.piin is local to CompileIterator

                    if (bodyExpr == null || Compiler.FAbortEarly(errNow, es))
                    {
                        goto LERROR;
                    }
                }
            }

            // compiling the method reset the flag...
            Compiler.FuncBRec.SetUnsafe(methodSym.IsUnsafe);

            if (!encBuild)
            {
                int errorCount = Compiler.ErrorCount();

                //----------------------------------------------------
                // Emit attributes on parameters.
                //----------------------------------------------------
                ParamAttrBind.CompileParamList(Compiler, methInfo);

                if (errorCount != Compiler.ErrorCount())
                {
                    DebugUtil.Assert(Compiler.ErrorCount() > errorCount);

                    // Mark indexer as having attribute error
                    // so we don't give the same error on the parameters for the second accessor
                    if (methInfo.MethodSym.IsPropertyAccessor && methInfo.MethodSym.PropertySym.IsIndexer)
                    {
                        methInfo.MethodSym.PropertySym.HadAttributeError = true;
                    }
                    else if (methInfo.MethodSym.ClassSym.IsDelegate)
                    {
                        // Mark delegates so we don't give the same errors on each fabricated method
                        methInfo.MethodSym.ClassSym.HadAttributeError = true;
                    }
                }

                // Check CLS compliance of signature (varargs, return type, and parameters)
                // NOTE: Do this AFTER compiling parameter-level attributes so that if they
                //   accidentally put the CLSCompliant attribute on a parameter, we give
                //   the warning about it being ignored/meaningless before giving the error
                //   about the non-CLS parameter.

                if (Compiler.CheckForCLS &&
                    Compiler.CheckSymForCLS(methodSym, false) &&
                    methodSym.HasExternalAccess())
                {
                    if (methodSym.ClassSym.IsDelegate)
                    {
                        // Only check the invoke method for delegates 
                        // BeginInvoke will just cause duplicate errors,
                        // and the other methods are all known OK.
                        if (methodSym.IsInvoke)
                        {
                            CheckSigForCLS(methodSym, methodSym.ClassSym, methInfo.ParametersNode);
                        }
                    }
                    else if (!methodSym.IsPropertyAccessor && !methodSym.IsEventAccessor)
                    {
                        // Don't check property/event accessors because we check the property itself.
                        CheckSigForCLS(methodSym, methodSym, methInfo.ParametersNode);
                    }
                }

                if (aggInfo.IsComImport && !methodSym.IsAbstract && !methodSym.IsExternal)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_ComImportWithImpl,
                        new ErrArgRef(methodSym), new ErrArgRef(methodSym.ClassSym));
                }
            }

            // Generate il only if no errors...
            if (Compiler.FAbortCodeGen(0))
            {
                goto LERROR;
            }

            //--------------------------------------------------------
            // Generate IL codes.
            //--------------------------------------------------------
            //if (!Compiler.Options.NoCodeGen)
            {
                if (bodyExpr != null)
                {
                    DebugUtil.Assert(!methodSym.IsAbstract && !methodSym.IsExternal);
                    Compiler.SetLocation(COMPILER.STAGE.CODEGEN);
#if DEBUG
                    if (debug_stop)
                    {
                        Console.WriteLine("stop");
                    }
                    sb.Length = 0;
                    DebugUtil.DebugNodesOutput(sb);
                    sb.Length = 0;
                    DebugUtil.DebugSymsOutput(sb);
                    sb.Length = 0;
                    Compiler.MainSymbolManager.GlobalSymbolTable.Debug(sb);
                    sb.Length = 0;
                    DebugUtil.DebugExprsOutput(sb);
                    sb.Length = 0;
#endif
#if DEBUG2
                    DebugUtil.PrintBegin("ILGen", methodSym.ClassSym.Name, methodSym.Name);
#endif
                    Compiler.IlGenRec.Compile(methodSym, methInfo, bodyExpr);
#if DEBUG2
                    DebugUtil.PrintEnd("ILGen", methodSym.ClassSym.Name, methodSym.Name);
#endif
                }
                else
                {
                    DebugUtil.Assert(
                        methodSym.IsAbstract ||
                        methodSym.ClassSym.IsDelegate ||
                        methodSym.IsExternal);
                }
            }

            if (methodSym.IsExternal)
            {
                BASENODE tempNode = methodSym.GetAttributesNode();
                if (tempNode == null && !methInfo.IsMagicImpl && !aggInfo.IsComImport)
                {
                    // An extern method, property accessor or event with NO attributes!
                    Compiler.ErrorRef(
                        null,
                        CSCERRID.WRN_ExternMethodNoImplementation,
                        new ErrArgRef(methodSym));
                }
            }

        LERROR:

            interiorTree = null;
            Compiler.FuncBRec.ResetUnsafe();
        }

        //------------------------------------------------------------
        // CLSDREC.CompileIterator
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="bodyExpr"></param>
        /// <param name="methodInfo"></param>
        /// <param name="aggInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR CompileIterator(
            BASENODE treeNode,
            EXPR bodyExpr,
            METHINFO methodInfo,
            AGGINFO aggInfo)
        {
            DebugUtil.Assert(methodInfo.IteratorInfo == null);

            IterInfo iterInfo = new IterInfo();
            //memset(&iterInfo, 0, sizeof(iterInfo));
            methodInfo.IteratorInfo = iterInfo;
            Exception excp = null;

            int prevErrorCount = compiler.ErrorCount();

            //--------------------------------------------------------
            // First create the symbols (MoveNext)
            //--------------------------------------------------------
            METHSYM iterMethodSym = MakeIterator(methodInfo);

            if (iterMethodSym == null || compiler.FAbortCodeGen(prevErrorCount))
            {
                DebugUtil.Assert(compiler.ErrorCount() > 0);
                methodInfo.IteratorInfo = null;
                return null;
            }

            METHSYM methodSym = iterMethodSym;

            //METHINFO  newMethodInfo = (METHINFO *)STACK_ALLOC_ZERO(byte, METHINFO::Size(1));
            METHINFO newMethodInfo = new METHINFO();

            newMethodInfo.InitFromIterInfo(methodInfo, methodSym, 0);

            // This will add all the fields for locals to both the able and etor classes
            bodyExpr = compiler.FuncBRec.RewriteMoveNext(methodInfo.MethodSym, bodyExpr, newMethodInfo);

            // Bail if there were errors
            if (compiler.FAbortCodeGen(prevErrorCount))
            {
                methodInfo.IteratorInfo = null;
                return null;
            }

            //--------------------------------------------------------
            // So now we can emit all the metadata for the fabricated classes
            //--------------------------------------------------------
            EmitTypedefsAggregate(methodInfo.IteratorInfo.IteratorAggSym);
            EmitBasesAggregate(methodInfo.IteratorInfo.IteratorAggSym);
            EmitMemberdefsAggregate(methodInfo.IteratorInfo.IteratorAggSym);

            //--------------------------------------------------------
            // code-gen MoveNext
            //--------------------------------------------------------
            DebugUtil.Assert(newMethodInfo.MethodSym.ParameterTypes.Count == 0);
            CompileFabricatedMethod(treeNode, bodyExpr, newMethodInfo, aggInfo);
            compiler.Emitter.ResetIterator();

            //--------------------------------------------------------
            // Now do the Current properties, IDispose.Dispose, IEnumerator.Reset,
            // IEnumerator.MoveNext (if not already done), and both IEnumerable.GetEnumerator methods
            //--------------------------------------------------------
            TYPESYM idisposableTypeSym = compiler.GetReqPredefType(PREDEFTYPE.IDISPOSABLE, true);
            METHSYM getEnumeratorMethodSym = null;
            TYPESYM ienumeratorTypeSym = null;
            ienumeratorTypeSym = compiler.GetReqPredefType(PREDEFTYPE.IENUMERATOR, true);

            for (methodSym = compiler.MainSymbolManager.LookupAggMember(
                    null,
                    methodInfo.IteratorInfo.IteratorAggSym,
                    SYMBMASK.METHSYM) as METHSYM;
                methodSym != null;
                methodSym = BSYMMGR.LookupNextSym(
                    methodSym,
                    methodInfo.IteratorInfo.IteratorAggSym,
                    SYMBMASK.METHSYM) as METHSYM)
            {
                //----------------------------------------------------
                // We've already code-gen'd the real MoveNext method so don't do it again
                //----------------------------------------------------
                if (methodSym == iterMethodSym)
                {
                    continue;
                }

                //----------------------------------------------------
                // get_Current
                //----------------------------------------------------
                if (methodSym.IsPropertyAccessor)
                {
                    newMethodInfo.InitFromIterInfo(methodInfo, methodSym, 0);
                    newMethodInfo.EmitDebuggerHiddenAttribute = true;
                    newMethodInfo.NoDebugInfo = true;

                    bodyExpr = compiler.FuncBRec.MakeIterCur(treeNode, newMethodInfo, aggInfo);
                    CompileFabricatedMethod(treeNode, bodyExpr, newMethodInfo, aggInfo);
                }
                //----------------------------------------------------
                // IDisposable.Dispose
                //----------------------------------------------------
                else if (methodSym.SlotSymWithType.AggTypeSym == idisposableTypeSym)
                {
                    DebugUtil.Assert(
                        methodSym.SlotSymWithType.MethSym.Name
                        == compiler.NameManager.GetPredefinedName(PREDEFNAME.DISPOSE));
                    newMethodInfo.InitFromIterInfo(methodInfo, methodSym, 0);

                    bodyExpr = compiler.FuncBRec.MakeIterDispose(treeNode, newMethodInfo, aggInfo);
                    CompileFabricatedMethod(treeNode, bodyExpr, newMethodInfo, aggInfo);
                }
                //----------------------------------------------------
                // IEnumerator.Reset
                //----------------------------------------------------
                else if (
                    methodSym.ReturnTypeSym == compiler.MainSymbolManager.VoidSym||
                    methodSym.ReturnTypeSym.IsVoidType)
                {
                    DebugUtil.Assert(methodSym.SlotSymWithType.AggTypeSym == ienumeratorTypeSym);
                    DebugUtil.Assert(
                        methodSym.SlotSymWithType.MethSym.Name
                        == compiler.NameManager.GetPredefinedName(PREDEFNAME.RESET));

                    newMethodInfo.InitFromIterInfo(methodInfo, methodSym, 0);
                    newMethodInfo.EmitDebuggerHiddenAttribute = true;
                    newMethodInfo.NoDebugInfo = true;

                    bodyExpr = compiler.FuncBRec.MakeIterReset(treeNode, newMethodInfo, aggInfo);
                    CompileFabricatedMethod(treeNode, bodyExpr, newMethodInfo, aggInfo);
                }
                //----------------------------------------------------
                // GetEnumerator
                //----------------------------------------------------
                else
                {
                    DebugUtil.Assert(
                        methodSym.SlotSymWithType.MethSym.Name
                        == compiler.NameManager.GetPredefinedName(PREDEFNAME.GETENUMERATOR));
                    DebugUtil.Assert(methodInfo.IteratorInfo.IsEnumerable);

                    newMethodInfo.InitFromIterInfo(methodInfo, methodSym, 0);
                    newMethodInfo.EmitDebuggerHiddenAttribute = true;
                    newMethodInfo.NoDebugInfo = true;

                    bodyExpr = compiler.FuncBRec.MakeIterGetEnumerator(
                        treeNode,
                        newMethodInfo,
                        aggInfo,
                        ref getEnumeratorMethodSym);	// &getEnumeratorMethodSym);
                    CompileFabricatedMethod(treeNode, bodyExpr, newMethodInfo, aggInfo);
                }
            }
            compiler.Emitter.EndIterator();

            //--------------------------------------------------------
            // .Ctor
            //--------------------------------------------------------
            methodSym = compiler.MainSymbolManager.LookupAggMember(
                compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR),
                methodInfo.IteratorInfo.IteratorAggSym,
                SYMBMASK.METHSYM) as METHSYM;
            newMethodInfo.InitFromIterInfo(methodInfo, methodSym, 1);
            newMethodInfo.EmitDebuggerHiddenAttribute = true;
            newMethodInfo.NoDebugInfo = true;
            //newMethodInfo.cpin = 1;

            //newMethodInfo.rgpin[0].SetName(compiler.namemgr.GetPredefName(PREDEFNAME.ITERSTATE));
            PARAMINFO paramInfo = new PARAMINFO();
            paramInfo.Name = compiler.NameManager.GetPredefinedName(PREDEFNAME.ITERSTATE);
            newMethodInfo.ParameterInfos.Add(paramInfo);

            bodyExpr = compiler.FuncBRec.MakeIterCtor(treeNode, newMethodInfo, aggInfo);
            CompileFabricatedMethod(null, bodyExpr, newMethodInfo, aggInfo);

            //--------------------------------------------------------
            // dummy up a fake bodyExpr for the real iterator method
            //--------------------------------------------------------
            compiler.Emitter.EmitForwardedIteratorDebugInfo(methodInfo.MethodSym, iterMethodSym);
            bodyExpr = compiler.FuncBRec.MakeIterGet(treeNode, methodInfo, aggInfo);

            DebugUtil.Assert(!compiler.FAbortCodeGen(prevErrorCount));
            // This code should never report any errors!

            TypeBuilder typeBuilder = methodInfo.IteratorInfo.IteratorAggSym.TypeBuilder;
            if (typeBuilder != null)
            {
                try
                {
                    typeBuilder.CreateType();
                }
                catch (TypeLoadException ex)
                {
                    excp = ex;
                }
                catch (InvalidOperationException ex)
                {
                    excp = ex;
                }
                catch (NotSupportedException ex)
                {
                    excp = ex;
                }

                if (excp != null)
                {
                    // show an error message and increment the error count.
                }
            }

            methodInfo.IteratorInfo = null;
            methodInfo.NoDebugInfo = true;
            return bodyExpr;
        }

        //------------------------------------------------------------
        // CLSDREC.CompileFabricatedMethod
        //
        /// <summary>
        /// Compile a fabricated method...
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="bodyExpr"></param>
        /// <param name="methInfo"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        private void CompileFabricatedMethod(
            BASENODE treeNode,
            EXPR bodyExpr,
            METHINFO methInfo,
            AGGINFO aggInfo)
        {
            DebugUtil.Assert(methInfo.MethodSym != null);
            // Either the entire class is fabricated or at least this method is an anonymous method

            DebugUtil.Assert(methInfo.MethodSym.ClassSym.IsFabricated || methInfo.MethodSym.IsAnonymous);
            //DebugUtil.Assert(!methInfo.MethSym.tokenImport);

            // No fabricated method will ever be a param array!
            DebugUtil.Assert(!methInfo.MethodSym.IsParameterArray);

            // emit method info

            compiler.Emitter.EmitMethodInfo(methInfo.MethodSym, methInfo);

            // emit attributes on the method and parameters
            // The method should get the DebuggerHiddenAttribute and the CompilerGeneratedAttribute
            // And the parameters just get their names

            Dictionary<SecurityAction, PermissionSet> permissionSets
                = new Dictionary<SecurityAction, PermissionSet>();

            MethAttrBind.CompileAndEmit(
                compiler,
                methInfo.MethodSym,
                methInfo.EmitDebuggerHiddenAttribute,
                permissionSets);

            if (methInfo.MethodSym.ParameterTypes.Count > 0)
            {
                ParamAttrBind.CompileParamList(compiler, methInfo);
            }

            // Recursively compile any nested anonymous delegates
            CompileAnonMeths(methInfo, treeNode, bodyExpr);

            // generate il
            //if (!compiler.Options.NoCodeGen)
            {
                DebugUtil.Assert(bodyExpr != null);
                DebugUtil.Assert(
                    !methInfo.MethodSym.IsAbstract &&
                    !methInfo.MethodSym.IsExternal);

                Compiler.SetLocation(COMPILER.STAGE.CODEGEN);
                compiler.IlGenRec.Compile(methInfo.MethodSym, methInfo, bodyExpr);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileAnonMeths
        //
        /// <summary>
        /// Optionally rewrites any anonymous methods contained in this method
        /// and emits/compiles the nested classes/methods.
        /// </summary>
        /// <param name="outerMethInfo"></param>
        /// <param name="treeNode"></param>
        /// <param name="bodyExpr"></param>
        //------------------------------------------------------------
        private void CompileAnonMeths(METHINFO outerMethInfo, BASENODE treeNode, EXPR bodyExpr)
        {
            if (outerMethInfo.FirstAnonymousMethodInfo == null)
            {
                return;
            }
            DebugUtil.Assert(treeNode != null);

            // This method had some anonymous delegates inside it.
            // It's already been rewritten, so just emit the metadata and compile the bodies.
            // compileFabricatedMethod will recurse into nested anonymous delegates.

            // Get the maximum number of parameters.
            int cpinMax = 0;
            AnonMethInfo anonMethInfo = null;

            for (anonMethInfo = outerMethInfo.FirstAnonymousMethodInfo;
                anonMethInfo != null;
                anonMethInfo = anonMethInfo.NextInfo)
            {
                DebugUtil.Assert(anonMethInfo.Seen);

                // Update the max number of parameters.
                if (cpinMax < anonMethInfo.MethodSym.ParameterTypes.Count)
                {
                    cpinMax = anonMethInfo.MethodSym.ParameterTypes.Count;
                }
            }

            // Allocate the METHINFO.
            //METHINFO  methInfo = (METHINFO *)STACK_ALLOC_ZERO(byte, METHINFO::Size(cpinMax));
            METHINFO methInfo = new METHINFO();

            AGGINFO newAggInfo = new AGGINFO();
            //memset(&newAggInfo, 0, sizeof(newAggInfo));

            // Emit all the metadata for the fabricated classes and methods
            // and compile the .ctors

            for (SYM child = outerMethInfo.MethodSym.ClassSym.FirstChildSym;
                child != null;
                child = child.NextSym)
            {
                // Only do un-compiled, fabricated classes
                if (!child.IsAGGSYM || !(child as AGGSYM).IsFabricated || (child as AGGSYM).IsCompiled)
                {
                    continue;
                }

                AGGSYM aggSym = child as AGGSYM;
                METHSYM ctorSym = compiler.MainSymbolManager.LookupAggMember(
                    compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR),
                    aggSym,
                    SYMBMASK.METHSYM) as METHSYM;

                DebugUtil.Assert(
                    ctorSym == null ||
                    (ctorSym.IsCtor && ctorSym.NextSameNameSym == null));

                // If it doesn't have a ctorSym,
                // then it must be a fixed buffer struct, which also means we shouldn't touch it
                if (ctorSym == null)
                {
#if DEBUG
                    DebugUtil.Assert(
                        aggSym.IsMemberDefsEmitted &&
                        aggSym.IsBasesEmitted &&
                        aggSym.IsTypeDefEmitted);
                    for (SYM nested = aggSym.FirstChildSym; nested != null; nested = nested.NextSym)
                    {
                        DebugUtil.Assert(!nested.IsMETHPROPSYM);
                    }
#endif // DEBUG
                    aggSym.AggState = AggStateEnum.Compiled;
                    continue;
                }

                // If the ctorSym has arguments, it must be for an iterator, which means we shouldn't touch this class
                if (ctorSym.ParameterTypes.Count != 0)
                {
                    continue;
                }

                EmitTypedefsAggregate(aggSym);
                EmitBasesAggregate(aggSym);
                EmitMemberdefsAggregate(aggSym);

                // It's safe to compile all the .ctors here because they're just simple .ctors
                // with no dependencies
                //memset(methInfo, 0, METHINFO::Size(cpinMax));
                methInfo.MethodSym = ctorSym;
                methInfo.NoDebugInfo = true;

                EXPR ctorBodyExpr = compiler.FuncBRec.MakeAnonCtor(treeNode, methInfo, newAggInfo);
                CompileFabricatedMethod(treeNode, ctorBodyExpr, methInfo, newAggInfo);
                aggSym.AggState = AggStateEnum.Compiled;
            }

            // Emit all the metadata for any anonymous methods that were pushed up to the user's class
            for (anonMethInfo = outerMethInfo.FirstAnonymousMethodInfo;
                anonMethInfo != null;
                anonMethInfo = anonMethInfo.NextInfo)
            {
                //if (IsNilToken(anonMethInfo.MethodSym.tokenEmit))
                if (anonMethInfo.MethodSym.MethodBuilder == null)
                {
                    DebugUtil.Assert(anonMethInfo.MethodSym.ClassSym.IsTypeDefEmitted);

                    // an optimized anonymous method that is a member
                    // of a user type that has already been emitted
                    // so just emit this method (this is out of order)
                    DebugUtil.Assert(!anonMethInfo.MethodSym.ClassSym.IsFabricated);
                    compiler.Emitter.EmitMethodDef(anonMethInfo.MethodSym);
                }

                if (anonMethInfo.DelegateCacheExpr != null &&
                    anonMethInfo.DelegateCacheExpr.Kind == EXPRKIND.FIELD)
                {
                    MEMBVARSYM field =
                        (anonMethInfo.DelegateCacheExpr as EXPRFIELD).FieldWithType.FieldSym;

                    //DebugUtil.Assert(IsNilToken(field.tokenEmit));
                    DebugUtil.Assert(field.FieldBuilder == null);
                    DebugUtil.Assert(field.ClassSym.IsTypeDefEmitted);

                    compiler.Emitter.EmitMembVarDef(field);
                    CompileField(field, newAggInfo);
                }
            }

            // Compile all the anonymous methods
            for (anonMethInfo = outerMethInfo.FirstAnonymousMethodInfo;
                anonMethInfo != null;
                anonMethInfo = anonMethInfo.NextInfo)
            {
                DebugUtil.Assert(
                    !anonMethInfo.MethodSym.ClassSym.IsFabricated ||
                    anonMethInfo.MethodSym.ClassSym.IsCompiled);

                //memset(methInfo, 0, METHINFO::Size(cpinMax));
                methInfo.Clear();
                methInfo.MethodSym = anonMethInfo.MethodSym;
                methInfo.FirstAnonymousMethodInfo = anonMethInfo.ChildInfo;
                methInfo.OuterScopeSym = anonMethInfo.ParametersScopeSym;
                methInfo.HasReturnAsLeave = anonMethInfo.HasReturnAsLeave;

                ANONBLOCKNODE anonBlockNode
                    = methInfo.MethodSym.ParseTreeNode as ANONBLOCKNODE;
                methInfo.ParametersNode = anonBlockNode.ArgumentsNode;

                //int i = 0;
                BASENODE node = anonBlockNode.ArgumentsNode;
                while (node != null)
                {
                    PARAMETERNODE param;
                    if (node.Kind == NODEKIND.LIST)
                    {
                        param = node.AsLIST.Operand1 as PARAMETERNODE;
                        node = node.AsLIST.Operand2;
                    }
                    else
                    {
                        param = node as PARAMETERNODE;
                        node = null;
                    }

                    //DebugUtil.Assert(i < cpinMax && i < methInfo.MethSym.ParameterTypes.Count);
                    //methInfo.ParameterInfos[i].ParameterNode = param;
                    //methInfo.ParameterInfos[i++].SetName(param.NameNode.Name);
                    PARAMINFO paramInfo = new PARAMINFO();
                    paramInfo.ParameterNode = param;
                    paramInfo.Name = param.NameNode.Name;
                    methInfo.ParameterInfos.Add(paramInfo);
                    
                }

                //DebugUtil.Assert(
                //    i == methInfo.MethSym.ParameterTypes.Count ||
                //    i == 0 &&
                //    anonBlockNode.CloseParenIndex == -1);
                DebugUtil.Assert(methInfo.ParameterInfos.Count > 0 || anonBlockNode.CloseParenIndex == -1);

                //methInfo.cpin = i;
                CompileFabricatedMethod(treeNode, anonMethInfo.BodyBlockExpr, methInfo, newAggInfo);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileField
        //
        /// <summary>
        /// Compile all decls in a field node... all this does is emit the md...
        /// </summary>
        /// <param name="fieldSym"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        private void CompileField(MEMBVARSYM fieldSym, AGGINFO aggInfo)
        {
            DebugUtil.Assert(fieldSym != null);

            MEMBVARINFO membvarInfo = new MEMBVARINFO();
            //MEM_SET_ZERO(membvarInfo);
            FieldAttrBind.Compile(Compiler, fieldSym, membvarInfo, aggInfo);
            if (Compiler.CheckForCLS &&
                Compiler.CheckSymForCLS(fieldSym, false) &&
                fieldSym.HasExternalAccess())
            {
                if (!Compiler.IsCLS_Type(fieldSym.ClassSym, fieldSym.TypeSym))
                {
                    Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadFieldPropType, new ErrArgRef(fieldSym));
                }
                if (fieldSym.IsVolatile)
                {
                    Compiler.ErrorRef(null, CSCERRID.WRN_CLS_VolatileField, new ErrArgRef(fieldSym));
                }
            }

            if (fieldSym.FixedAggSym != null)
            {
                int length = fieldSym.ConstVal.GetInt();
                if (length < 1) length = 1;
                // Previous errors might have set this to 0
                int size = BSYMMGR.GetAttrArgSize(
                    (fieldSym.TypeSym as PTRSYM).BaseTypeSym.GetPredefType());
                DebugUtil.Assert(size == 1 || size == 2 || size == 4 || size == 8);

                // Check for overflow
                if (length > (System.Int32.MaxValue / size))
                {
                    // dwLength * size would be greater than what metadata can handle
                    Compiler.Error(
                        fieldSym.GetConstExprTree(),
                        CSCERRID.ERR_FixedOverflow,
                        new ErrArg(length),
                        new ErrArg((fieldSym.TypeSym as PTRSYM).BaseTypeSym));
                }
                else
                {
                    //Set the layout and the class
                    //DWORD dwLength = (DWORD)(length * size);
                    //DebugUtil.Assert(dwLength <= LONG_MAX);
                    //bool br = Compiler.CurrentPEFile.GetEmit().SetClassLayout(
                    //	fieldSym.fixedAgg.tokenEmit, 0, null, dwLength);
                    //if (!br)
                    //{
                    //    Compiler.Error(
                    //    	CSCERRID.FTL_MetadataEmitFailure,
                    //        new ErrArg(Compiler.ErrHR(hr)),
                    //        new ErrArg(fieldSym.GetInputFile().GetOutputFile()),
                    //        new ErrArgRefOnly(fieldSym));
                    //}
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileProperty
        //
        /// <summary></summary>
        /// <param name="propertySym"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        private void CompileProperty(PROPSYM propertySym, AGGINFO aggInfo)
        {
            PropAttrBind.Compile(Compiler, propertySym);

            PROPINFO propInfo = new PROPINFO();

            // Accumulate parameter names for indexed properties, if any.
            if (TypeArray.Size(propertySym.ParameterTypes) > 0)
            {
                propInfo.ParmeterInfos = new PARAMINFO[propertySym.ParameterTypes.Count];
                int i = 0;
                BASENODE node = propertySym.GetParseTree().AsANYPROPERTY.ParametersNode;
                while (node != null)
                {
                    PARAMETERNODE paramNode;
                    if (node.Kind == NODEKIND.LIST)
                    {
                        paramNode = node.AsLIST.Operand1 as PARAMETERNODE;
                        node = node.AsLIST.Operand2;
                    }
                    else
                    {
                        paramNode = node as PARAMETERNODE;
                        node = null;
                    }
                    propInfo.ParmeterInfos[i] = new PARAMINFO();
                    propInfo.ParmeterInfos[i].ParameterNode = paramNode;
                    propInfo.ParmeterInfos[i].Name = paramNode.NameNode.Name;

                    if (Compiler.CheckForCLS &&
                        Compiler.CheckSymForCLS(propertySym, false) &&
                        propertySym.HasExternalAccess() &&
                        !Compiler.IsCLS_Type(propertySym.ParentSym, propertySym.ParameterTypes[i]))
                    {
                        Compiler.Error(paramNode, CSCERRID.WRN_CLS_BadArgType,
                            new ErrArg(propertySym.ParameterTypes[i]));
                    }
                    ++i;
                }
            }

            if (Compiler.CheckForCLS &&
                Compiler.CheckSymForCLS(propertySym, false) &&
                propertySym.HasExternalAccess())
            {
                if (!Compiler.IsCLS_Type(propertySym.ParentSym, propertySym.ReturnTypeSym))
                {
                    Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadFieldPropType, new ErrArgRef(propertySym));
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileEvent
        //
        /// <summary></summary>
        /// <param name="eventSym"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        private void CompileEvent(EVENTSYM eventSym, AGGINFO aggInfo)
        {
            DefaultAttrBind.CompileAndEmit(Compiler, eventSym);

            if (Compiler.CheckForCLS &&
                Compiler.CheckSymForCLS(eventSym, false) &&
                eventSym.HasExternalAccess() &&
                !Compiler.IsCLS_Type(eventSym.ParentSym, eventSym.TypeSym))
            {
                Compiler.ErrorRef(null, CSCERRID.WRN_CLS_BadFieldPropType, new ErrArgRef(eventSym));
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileAggSkeleton
        //
        /// Compile the skeleton for a class. Basically we need all predefined attributes of all members.
        //------------------------------------------------------------
        private void CompileAggSkeleton(AGGSYM aggSym)
        {
            //DebugUtil.Assert(Compiler.Options.CompileSkeleton);
            DebugUtil.Assert(aggSym.IsSource);

            // If we've already hit this one (because it was a base of someone earlier),
            // then nothing more to do.
            if (aggSym.IsCompiled || aggSym.IsFabricated)
            {
                // Already did it (or shouldn't ever do it).
                return;
            }
            DebugUtil.Assert(aggSym.HasParseTree);

            // If we couldn't emit the class don't try to compile it
            // (unless codegen is off, of course,
            // in which case we won't have a token even if everything was good).
            if (//!Compiler.Options.NoCodeGen &&
                //(TypeFromToken(aggSym.tokenEmit) != mdtTypeDef || aggSym.tokenEmit == mdTypeDefNil))
                aggSym.TypeBuilder == null)
            {
                return;
            }

            Compiler.SetLocation(aggSym);

            // Do base classes and base interfaces, if they are in the same output scope.
            OUTFILESYM outfileSym = aggSym.GetOutputFile();

            // Do the base class
            AGGSYM baseAggSym = aggSym.GetBaseAgg();
            if (baseAggSym != null && baseAggSym.GetOutputFile() == outfileSym)
            {
                CompileAggSkeleton(baseAggSym);
            }

            // Iterate the base interfaces.
            for (int i = 0; i < aggSym.Interfaces.Count; i++)
            {
                AGGSYM ifaceBase = (aggSym.Interfaces[i] as AGGTYPESYM).GetAggregate();
                if (ifaceBase.GetOutputFile() == outfileSym)
                {
                    CompileAggSkeleton(ifaceBase);
                }
            }

            // Do outer classes before nested classes.
            if (aggSym.IsNested && !aggSym.GetOuterAgg().IsCompiled)
            {
                CompileAggSkeleton(aggSym.GetOuterAgg());
            }

            if (aggSym.IsCompiled)
            {
                return;
            }

            AGGINFO info = new AGGINFO();
            //MEM_SET_ZERO(info);

            Dictionary<SecurityAction, PermissionSet> permissionSets
                = new Dictionary<SecurityAction, PermissionSet>();

            AggAttrBind.Compile(Compiler, aggSym, info, permissionSets);

            EnumMembersInEmitOrder(aggSym, info, MEMBER_OPERATION_CompileMemberSkeleton);

            DebugUtil.Assert(
                AggStateEnum.Prepared <= aggSym.AggState &&
                aggSym.AggState < AggStateEnum.Compiled);
            aggSym.AggState = AggStateEnum.Compiled;

            // Nested classes must be done after our state is AggState::Compiled.
            for (SYM childSym = aggSym.FirstChildSym; childSym != null; childSym = childSym.NextSym)
            {
                if (childSym.IsAGGSYM)
                {
                    CompileAggSkeleton(childSym as AGGSYM);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileMemberSkeleton
        //
        /// <summary></summary>
        /// <remarks>
        /// (sscli) private void CompileMemberSkeleton(SYM sym, AGGINFO info)
        /// </remarks>
        /// <param name="sym"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        private void CompileMemberSkeleton(SYM sym, object aggInfo)
        {
            throw new NotImplementedException("CLSDREC.CompileAggSkeleton");
        }

        //------------------------------------------------------------
        // CLSDREC.FillMethInfoCommon
        //
        /// <summary></summary>
        /// <param name="methSym"></param>
        /// <param name="methInfo"></param>
        /// <param name="aggInfo"></param>
        /// <param name="noError"></param>
        //------------------------------------------------------------
        internal void FillMethInfoCommon(
            METHSYM methSym,
            METHINFO methInfo,
            AGGINFO aggInfo,
            bool noError)
        {
            DebugUtil.Assert(methSym != null && methInfo != null);
            DebugUtil.Assert(methSym.ParseTreeNode != null);
            methInfo.MethodSym = methSym;

            if (methSym.IsExternal && methSym.IsCtor && aggInfo.IsComImport)
            {
                DebugUtil.Assert(
                    methSym.ClassSym.IsClass &&
                    !methSym.IsStatic && methSym.IsSysNative);
                methInfo.IsMagicImpl = true;
            }

            BASENODE methNode = methSym.ParseTreeNode;
            BASENODE paramsNode = null;
            bool skipParams = false;

            switch (methNode.Kind)
            {
                case NODEKIND.METHOD:
                case NODEKIND.CTOR:
                case NODEKIND.OPERATOR:
                case NODEKIND.DTOR:
                    paramsNode = methNode.AsANYMETHOD.ParametersNode;
                    methInfo.AttributesNode = methNode.AsANYMETHOD.AttributesNode;
                    break;

                case NODEKIND.ACCESSOR:
                    DebugUtil.Assert(methSym.IsAnyAccessor);
                    if (methNode.ParentNode.InGroup(NODEGROUP.PROPERTY))
                    {
                        paramsNode = methNode.ParentNode.AsANYPROPERTY.ParametersNode;
                        methInfo.AttributesNode
                            = (methSym.GetParseTree() as ACCESSORNODE).AttributesNode;
                    }
                    break;

                // Compiler fabricated accessors
                case NODEKIND.VARDECL:  // generated for field-style event declarations
                case NODEKIND.PROPERTY: // generated for sealed property overrides that only implement one accessor
                case NODEKIND.INDEXER:
                    DebugUtil.Assert(methSym.IsAnyAccessor);
                    if (methNode.InGroup(NODEGROUP.PROPERTY))
                    {
                        paramsNode = methNode.AsANYPROPERTY.ParametersNode;
                        methInfo.AttributesNode = methNode.AsANYPROPERTY.AttributesNode;
                    }
                    break;

                case NODEKIND.DELEGATE:
                    if (methSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE) ||
                        methSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.BEGININVOKE) ||
                        methSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.ENDINVOKE))
                    {
                        paramsNode = (methSym.ClassSym.DeclOnly().GetParseTree() as DELEGATENODE).ParametersNode;
                        if (methSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.ENDINVOKE))
                        {
                            skipParams = true;
                        }
                        if (methSym.Name != Compiler.NameManager.GetPredefinedName(PREDEFNAME.BEGININVOKE))
                        {
                            methInfo.AttributesNode = (methSym.ClassSym.DeclOnly().GetParseTree() as DELEGATENODE).AttributesNode;
                        }
                    }
                    break;

                case NODEKIND.STRUCT:
                case NODEKIND.CLASS:
                    DebugUtil.Assert(methSym.IsCtor || methSym.IsInterfaceImpl);
                    break;

                default:
                    DebugUtil.VsFail("Bad parse tree kind in FillMethInfoCommon");
                    break;
            }

            int ipin = 0;
            List<PARAMINFO> paramInfos = methInfo.ParameterInfos;
            // Does methInfo.ParameterInfos have any elements?

            if (!skipParams)
            {
                BASENODE nd1 = paramsNode;
                while (nd1 != null)
                {
                    PARAMETERNODE parameterNode1;
                    if (nd1.Kind == NODEKIND.LIST)
                    {
                        parameterNode1 = nd1.AsLIST.Operand1 as PARAMETERNODE;
                        nd1 = nd1.AsLIST.Operand2;
                    }
                    else
                    {
                        parameterNode1 = nd1 as PARAMETERNODE;
                        nd1 = null;
                    }

                    DebugUtil.Assert(paramInfos.Count <= methSym.ParameterTypes.Count);
                    //PARAMINFO pInfo = new PARAMINFO();
                    PARAMINFO pInfo = paramInfos[ipin];
                    pInfo.ParameterNode = parameterNode1;
                    pInfo.AttrBaseNode = parameterNode1.AttributesNode;
                    pInfo.Name = parameterNode1.NameNode.Name;
                    //paramInfos.Add(pInfo);
                    ++ipin;
                }
            }
            DebugUtil.Assert(ipin <= methSym.ParameterTypes.Count);

            if (methSym.IsParameterArray)
            {
                DebugUtil.Assert(ipin > 0);
                paramInfos[ipin - 1].IsParametersArray = true;
            }

            if (methSym.IsInterfaceImpl)
            {
                DebugUtil.Assert(ipin == 0);
            }
            else if (methSym.IsAnyAccessor)
            {
                DebugUtil.Assert(!methSym.IsEventAccessor || methSym.ParameterTypes.Count == 1);
                if (methSym.IsEventAccessor || !methSym.IsGetAccessor)
                {
                    DebugUtil.Assert(ipin == methSym.ParameterTypes.Count - 1);
                    DebugUtil.Assert(paramInfos[ipin].ParameterNode == null);
                    paramInfos[ipin].AttrBaseNode = methInfo.AttributesNode;
                    paramInfos[ipin++].Name
                        = Compiler.NameManager.GetPredefinedName(PREDEFNAME.VALUE);
                }
                if (methNode.Kind != NODEKIND.ACCESSOR)
                {
                    // This is an auto-generated event accessor, not a user-defined one.
                    if (!methSym.ClassSym.IsStruct && !methSym.IsExternal)
                    {
                        methInfo.IsSynchronized = true;
                        // synchronize this method if not on a struct.
                    }
                    methInfo.NoDebugInfo = true;
                    // Don't generate debug info for this method, since there is no source code.
                }
            }
            else if (methSym.ClassSym.IsDelegate)
            {
                // all delegate members are implemented by the runtime
                methInfo.IsMagicImpl = true;

                if (methSym.IsCtor)
                {
                    DebugUtil.Assert(ipin == 0);
                    DebugUtil.Assert(
                        paramInfos[ipin].ParameterNode == null &&
                        paramInfos[ipin].AttrBaseNode == null);

                    paramInfos[ipin++].Name
                        = Compiler.NameManager.GetPredefinedName(PREDEFNAME.DELEGATECTORPARAM0);

                    DebugUtil.Assert(
                        paramInfos[ipin].ParameterNode == null &&
                        paramInfos[ipin].AttrBaseNode == null);

                    paramInfos[ipin++].Name
                        = Compiler.NameManager.GetPredefinedName(PREDEFNAME.DELEGATECTORPARAM1);
                }
                else if (methSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE))
                {
                    // Make sure there aren't any duplicates.
                    if (!noError)
                    {
                        for (int i = 1; i < ipin; i++)
                        {
                            string name = paramInfos[i].Name;
                            for (int j = 0; j < i; j++)
                            {
                                if (name == paramInfos[j].Name)
                                {
                                    Compiler.Error(
                                        paramInfos[i].ParameterNode.NameNode,
                                        CSCERRID.ERR_DuplicateParamName,
                                        new ErrArg(name));
                                    goto LDoneChecking;
                                }
                            }
                        }
                    LDoneChecking: ;
                    }
                }
                else if (methSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.BEGININVOKE))
                {
                    DebugUtil.Assert(ipin == methSym.ParameterTypes.Count - 2);
                    DebugUtil.Assert(
                        paramInfos[ipin].ParameterNode == null &&
                        paramInfos[ipin].AttrBaseNode == null);

                    paramInfos[ipin++].Name
                        = Compiler.NameManager.GetPredefinedName(PREDEFNAME.DELEGATEBIPARAM0);

                    DebugUtil.Assert(
                        paramInfos[ipin].ParameterNode == null &&
                        paramInfos[ipin].AttrBaseNode == null);

                    paramInfos[ipin++].Name
                        = Compiler.NameManager.GetPredefinedName(PREDEFNAME.DELEGATEBIPARAM1);
                }
                else if (methSym.Name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.ENDINVOKE))
                {
                    DebugUtil.Assert(ipin == 0 && skipParams);
                    // we need the type of the invoke method:
                    METHSYM invoke = Compiler.MainSymbolManager.LookupInvokeMeth(methSym.ClassSym);

                    int j = 0;
                    bool fResultParam = false;
                    BASENODE nd2 = paramsNode;
                    while (nd2 != null)
                    {
                        PARAMETERNODE parameterNode2;
                        if (nd2.Kind == NODEKIND.LIST)
                        {
                            parameterNode2 = nd2.AsLIST.Operand1 as PARAMETERNODE;
                            nd2 = nd2.AsLIST.Operand2;
                        }
                        else
                        {
                            parameterNode2 = nd2 as PARAMETERNODE;
                            nd2 = null;
                        }
                        if (invoke.ParameterTypes[j].IsPARAMMODSYM)
                        {
                            DebugUtil.Assert(ipin < methSym.ParameterTypes.Count - 1);

                            string name = parameterNode2.NameNode.Name;
                            if (name == Compiler.NameManager.GetPredefinedName(PREDEFNAME.DELEGATEEIPARAM0))
                            {
                                fResultParam = true;
                            }
                            paramInfos[ipin].ParameterNode = parameterNode2;
                            paramInfos[ipin].AttrBaseNode = parameterNode2.AttributesNode;
                            paramInfos[ipin].Name = name;
                            ++ipin;
                        }
                        ++j;
                    }

                    DebugUtil.Assert(j == invoke.ParameterTypes.Count);
                    DebugUtil.Assert(ipin == methSym.ParameterTypes.Count - 1);
                    DebugUtil.Assert(
                        paramInfos[ipin].ParameterNode == null &&
                        paramInfos[ipin].AttrBaseNode == null);

                    paramInfos[ipin++].Name = Compiler.NameManager.GetPredefinedName(
                        fResultParam ? PREDEFNAME.DELEGATEEIPARAM0ALT : PREDEFNAME.DELEGATEEIPARAM0);
                }
                else
                {
                    DebugUtil.VsFail("Unknown method in delegate type");
                }
            }

            DebugUtil.Assert(
                ipin == methSym.ParameterTypes.Count
                ||
                ipin == methSym.ParameterTypes.Count - 1 &&
                methSym.ParameterTypes[ipin] == Compiler.MainSymbolManager.ArgListSym
                ||
                ipin == 0 &&
                methSym.IsInterfaceImpl);
            //methInfo.ParameterInfoCount = ipin;

            // Store the params in the METHINFO for downstream clients.
            // Note that for EndInvoke on a delegate type, this has the full set of parameters.
            methInfo.ParametersNode = paramsNode;
        }

        //------------------------------------------------------------
        // CLSDREC.
        //
        //------------------------------------------------------------
        //private PARAMINFO ReallocParams(int cntNeeded, int * maxAlreadAlloced, PARAMINFO ** ppParams);

        //------------------------------------------------------------
        // CLSDREC.EmitMemberdef
        //
        /// <summary></summary>
        /// <param name="memberSym"></param>
        /// <param name="unused"></param>
        //------------------------------------------------------------
        private void EmitMemberDef(SYM memberSym, object unused)
        {
            Compiler.SetLocation(memberSym);
            switch (memberSym.Kind)
            {
                case SYMKIND.MEMBVARSYM:
                    Compiler.Emitter.EmitMembVarDef(memberSym as MEMBVARSYM);
                    break;

                case SYMKIND.METHSYM:
                    METHSYM methSym = memberSym as METHSYM;
                    if (!(methSym.IsStatic && methSym.IsCompilerGeneratedCtor))
                    {
                        Compiler.Emitter.EmitMethodDef(memberSym as METHSYM);

                        // static and compiler generated constructors are emitted
                        // in CLSDREC.compileMethod later.
                    }
                    break;

                case SYMKIND.PROPSYM:
                    Compiler.Emitter.EmitPropertyDef(memberSym as PROPSYM);
                    break;

                case SYMKIND.EVENTSYM:
                    Compiler.Emitter.EmitEventDef(memberSym as EVENTSYM);
                    break;

                default:
                    DebugUtil.Assert(false, "Invalid memberSym sym");
                    break;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.compiler
        //------------------------------------------------------------
        //private COMPILER compiler()
        //{
        //    if (this.compilerObject == null) throw new LogicError("CLSDREC.compilerObject");
        //    return this.compilerObject;
        //}

        //private CController controller();

        //private static  TOKENID accessTokens[];

        // cached values for GetLayoutKindValue();

        //------------------------------------------------------------
        // CLSDREC.setOverrideBits
        //
        /// <summary>
        /// for each method in a class, set the override bit correctly
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void SetOverrideBits(AGGSYM aggSym)
        {
            DebugUtil.Assert(!aggSym.IsSource);

            if (aggSym.BaseClassSym == null)
            {
                return;
            }
            DebugUtil.Assert(aggSym.BaseClassSym.GetAggregate().IsPrepared);

            for (SYM sym = aggSym.FirstChildSym; sym != null; sym = sym.NextSym) //FOREACHCHILD(aggSym, sym)
            {
                if (sym.IsMETHSYM && (sym as METHSYM).IsOverride)
                {
                    METHSYM methSym = sym as METHSYM;
                    methSym.IsOverride = false;

                    // this met^hod could be an override
                    // NOTE: that we must set the isOverride flag accurately
                    // because it affects:
                    //      - lookup rules(override methods are ignored)
                    //      - list of inherited abstract methods

                    SymWithType hiddenSwt = new SymWithType();

                    bool needImpl = false;
                    if (FindSymHiddenByMethPropAgg(
                            methSym,
                            aggSym.BaseClassSym,
                            aggSym,
                            hiddenSwt,
                            null,
                            ref needImpl) &&
                        hiddenSwt.Sym.IsMETHSYM)
                    {
                        if (Compiler.MainSymbolManager.SubstEqualTypes(
                                methSym.ReturnTypeSym,
                                hiddenSwt.MethSym.ReturnTypeSym,
                                hiddenSwt.AggTypeSym,
                                methSym.TypeVariables) &&
                            hiddenSwt.MethSym.Access == methSym.Access &&
                            hiddenSwt.MethSym.IsMetadataVirtual)
                        {
                            methSym.IsOverride = true;
                            DebugUtil.Assert(
                                !hiddenSwt.MethSym.IsOverride == hiddenSwt.MethSym.SlotSymWithType.IsNull);

                            if (hiddenSwt.MethSym.SlotSymWithType.IsNotNull)
                            {
                                methSym.SlotSymWithType.Set(
                                    hiddenSwt.MethSym.SlotSymWithType.MethSym,
                                    Compiler.MainSymbolManager.SubstType(
                                        hiddenSwt.MethSym.SlotSymWithType.AggTypeSym,
                                        hiddenSwt.AggTypeSym,
                                        null) as AGGTYPESYM);
                            }
                            else
                            {
                                methSym.SlotSymWithType = hiddenSwt;
                            }
                        }

                        if (hiddenSwt.MethSym.IsDtor)
                        {
                            methSym.MethodKind = MethodKindEnum.Dtor;
                        }
                    }
                }
            } // ENDFOREACHCHILD

            // Now set the override bits on the property itself
            for (SYM sym = aggSym.FirstChildSym; sym != null; sym = sym.NextSym) // FOREACHCHILD(aggSym, sym)
            {
                if (!sym.IsPROPSYM || sym.IsBogus) continue;

                PROPSYM propSym = sym as PROPSYM;
                DebugUtil.Assert(!propSym.IsOverride);
                PROPSYM basePropSym;
                AGGTYPESYM baseAts;

                if (propSym.GetMethodSym != null && propSym.SetMethodSym != null)
                {
                    if (!propSym.GetMethodSym.IsOverride && !propSym.SetMethodSym.IsOverride) continue;

                    if (propSym.GetMethodSym.IsOverride != propSym.SetMethodSym.IsOverride ||
                        propSym.GetMethodSym.SlotSymWithType.IsNull ||
                        propSym.SetMethodSym.SlotSymWithType.IsNull ||
                        !propSym.GetMethodSym.SlotSymWithType.MethSym.IsPropertyAccessor ||
                        !propSym.SetMethodSym.SlotSymWithType.MethSym.IsPropertyAccessor ||
                        (baseAts = propSym.GetMethodSym.SlotSymWithType.AggTypeSym)
                            != propSym.SetMethodSym.SlotSymWithType.AggTypeSym ||
                        (basePropSym = propSym.GetMethodSym.SlotSymWithType.MethSym.PropertySym)
                            != propSym.SetMethodSym.SlotSymWithType.MethSym.PropertySym)
                    {
                        propSym.SetBogus(true);
                        propSym.UseMethodInstead = true;
                        continue;
                    }
                }
                else
                {
                    METHSYM accessorMethSym;

                    if (propSym.GetMethodSym != null)
                    {
                        accessorMethSym = propSym.GetMethodSym;
                    }
                    else if (propSym.SetMethodSym != null)
                    {
                        accessorMethSym = propSym.SetMethodSym;
                    }
                    else
                    {
                        DebugUtil.Assert(false, "A property without accessors?");
                        continue;
                    }

                    if (!accessorMethSym.IsOverride)
                    {
                        continue;
                    }

                    if (accessorMethSym.SlotSymWithType.IsNull ||
                        !accessorMethSym.SlotSymWithType.MethSym.IsPropertyAccessor)
                    {
                        propSym.SetBogus(true);
                        propSym.UseMethodInstead = true;
                        continue;
                    }

                    baseAts = accessorMethSym.SlotSymWithType.AggTypeSym;
                    basePropSym = accessorMethSym.SlotSymWithType.MethSym.PropertySym;
                }

                propSym.IsOverride = true;
                propSym.SlotSymWithType.Set(basePropSym, baseAts);
            } // ENDFOREACHCHILD
        }

        //------------------------------------------------------------
        // CLSDREC.DeclareInputfile
        //
        /// <summary>
        /// <para>declare all the types in an input file</para>
        /// <para>Create NSDECLSYM instance for inputfile,</para>
        /// </summary>
        /// <param name="parseTree"></param>
        /// <param name="inputfile"></param>
        //------------------------------------------------------------
        internal void DeclareInputfile(NAMESPACENODE parseTree, INFILESYM inputfile)
        {
            DebugUtil.Assert(
                parseTree != null &&
                inputfile.IsSource &&
                (inputfile.RootNsDeclSym == null || inputfile.RootNsDeclSym.ParseTreeNode == null));
            Compiler.SetLocation(inputfile);   //SETLOCATIONFILE(inputfile);
            Compiler.SetLocation(parseTree);   //SETLOCATIONNODE(parseTree);

            // create the new delcaration
            // Create the new NSDECLSYM instance with the name of RootNamespaceSym.
            // This NSDECLSYM instance is registered to the DECLSYM list of RootNamespaceSym.
            inputfile.RootNsDeclSym = Compiler.MainSymbolManager.CreateNamespaceDecl(
                        Compiler.MainSymbolManager.RootNamespaceSym,
                        null,                               // no declaration parent
                        inputfile,
                        parseTree);

            // declare everything in the declaration
            DeclareNamespace(inputfile.RootNsDeclSym);
        }

        //------------------------------------------------------------
        // CLSDREC.ResolveInheritance (1) NSDECLSYM
        //
        /// <summary>
        /// <para>Resolves the inheritance hierarchy for a namespace's types.</para>
        /// </summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void ResolveInheritance(NSDECLSYM nsDeclSym)
        {
            if (nsDeclSym == null)
            {
                return;
            }
            Compiler.SetLocation(nsDeclSym);   //SETLOCATIONSYM(nsDeclSym);

            // ensure the using clauses have been done
            // Note that this can happen before we get here
            // if a class in another namespace which is derived
            // from a class in this namespace is defined before us.

            EnsureUsingClausesAreResolved(nsDeclSym);

            // and define contained types and namespaces...

            for (SYM elem = nsDeclSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                switch (elem.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        ResolveInheritance(elem as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        AGGDECLSYM ats = elem as AGGDECLSYM;
                        DebugUtil.Assert(ats != null);
                        if (ats.IsFirst)
                        {
                            ResolveInheritance(ats.AggSym);
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.ResolveInheritance (2) AGGSYM
        //
        /// <summary>
        /// Call ResolveInheritanceRec method.
        /// And process child AGGSYM instances.
        /// </summary>
        /// <param name="cls"></param>
        //------------------------------------------------------------
        internal void ResolveInheritance(AGGSYM aggSym)
        {
            DebugUtil.Assert(
                aggSym.AggState == AggStateEnum.Declared ||
                aggSym.AggState == AggStateEnum.Inheritance);

            DebugUtil.VsVerify(
                ResolveInheritanceRec(aggSym),
                "ResolveInheritanceRec failed!");

            DebugUtil.Assert(aggSym.AggState == AggStateEnum.Inheritance);

            //--------------------------------------------------------
            // resolve nested types, interfaces will have no members at this point
            //--------------------------------------------------------
            for (SYM elem = aggSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                Compiler.SetLocation(elem);    //SETLOCATIONSYM(elem);

                // should only have types at this point
                switch (elem.Kind)
                {
                    case SYMKIND.AGGSYM:
                        DebugUtil.Assert(
                            (elem as AGGSYM).AggKind > AggKindEnum.Unknown &&
                            (elem as AGGSYM).AggKind < AggKindEnum.Lim);
                        ResolveInheritance(elem as AGGSYM);
                        break;

                    case SYMKIND.AGGTYPESYM:
                    case SYMKIND.TYVARSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown member");
                        break;
                }
            }
            DebugUtil.Assert(aggSym.AggState == AggStateEnum.Inheritance);
        }

        //------------------------------------------------------------
        // CLSDREC.ResolveInheritanceRec
        //
        /// <summary>
        /// <para>Returns false if an error was detected that could not be fixed (faked away).</para>
        /// <para>For example:
        /// <list type="bullet">
        /// <item>if the AGGSYM is already having its inheritance resolved this just returns false.</item>
        /// <item>if the AGGSYM is nested in a type having its inheritacne resolved,
        /// this resolves the inheritance of the inner type and returns false.
        /// In this case the error is reported on the inner type.
        /// The dependance can't be fixed there since the outer type is still having its inheritance resolved.</item>
        /// </list>
        /// </para>
        /// <para>Set the fields of aggSym.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ResolveInheritanceRec(AGGSYM aggSym)
        {
            bool returnValue = true;

            // Check if we're done already.
            if (aggSym.HasResolvedBaseClasses)
            {
                return true;
            }
            DebugUtil.Assert(!aggSym.IsPredefAgg(PREDEFTYPE.OBJECT));

            // Check for cycles. The error messages are reported elsewhere.
            if (aggSym.IsResolvingBaseClasses)
            {
                return false;
            }

            Compiler.SetLocation(aggSym);  //SETLOCATIONSYM(aggSym);

            //--------------------------------------------------------
            // !IsSource
            //
            // If nested, process the parent AGGSYM instance by ResolveInheritanceRec.
            // Then, process this AGGSYM instanace by IMPORTER.ResolveInheritance.
            //--------------------------------------------------------
            if (!aggSym.IsSource)
            {
                // Imported
                DebugUtil.Assert(aggSym.AggState <= AggStateEnum.Declared);

                if (aggSym.IsUnresolved)
                {
                    // This class appears to be neither imported nor defined in source.
                    Compiler.UndeclarableType(aggSym.AsUnresolved());
                    DebugUtil.Assert(aggSym.HasResolvedBaseClasses);
                    return returnValue;
                }
                DebugUtil.Assert(aggSym.AggState == AggStateEnum.Declared);

                //----------------------------------------------------
                // For imported types we need to allow an outer type to depend on an inner type,
                // so we don't advance to the resolving state until after we've resolved the outer type.
                //----------------------------------------------------
                if (aggSym.IsNested)
                {
                    AGGSYM parentAggSym = aggSym.ParentSym as AGGSYM;
                    if (!parentAggSym.IsResolvingBaseClasses)
                    {
                        returnValue &= ResolveInheritanceRec(parentAggSym);
                        if (aggSym.HasResolvedBaseClasses)
                        {
                            return returnValue;
                        }
                    }
                }
                DebugUtil.Assert(aggSym.AggState == AggStateEnum.Declared);
                aggSym.AggState = AggStateEnum.ResolvingInheritance;

                Compiler.Importer.ResolveInheritance(aggSym);
                DebugUtil.Assert(aggSym.HasResolvedBaseClasses);
            }
            //--------------------------------------------------------
            // IsSource
            //
            // If nested, process the parent AGGSYM instance
            // by ResolveInheritanceRec.
            //--------------------------------------------------------
            else // if (!aggSym.IsSource)
            {
                // For source types it's an error for an outer type to depend on an inner type.
                DebugUtil.Assert(aggSym.AggState == AggStateEnum.Declared);
                aggSym.AggState = AggStateEnum.ResolvingInheritance;

                //----------------------------------------------------
                // IsSource: Resolve our containing class.
                //----------------------------------------------------
                if (aggSym.IsNested)
                {
                    AGGSYM parentAggSym = aggSym.ParentSym as AGGSYM;
                    DebugUtil.Assert(parentAggSym != null);

                    if (!parentAggSym.IsResolvingBaseClasses)
                    {
                        returnValue &= ResolveInheritanceRec(parentAggSym);
                    }
                    else
                    {
                        Compiler.ErrorRef(
                            null,
                            CSCERRID.ERR_CircularBase,
                            new ErrArgRef(aggSym.ParentSym),
                            new ErrArgRef(aggSym));
                        // Need to break the dependence to avoid infinite recursion elsewhere.
                        returnValue = false;
                    }
                }

                //----------------------------------------------------
                // IsSource: Fabricated
                //----------------------------------------------------
                if (aggSym.IsFabricated)
                {
                    DebugUtil.Assert(!aggSym.HasParseTree);
                    DebugUtil.Assert(aggSym.InterfaceCount > 0 && aggSym.InterfaceCountAll > 0);
                    DebugUtil.Assert(aggSym.BaseClassSym != null);
                }
                //----------------------------------------------------
                // IsSource: Enum
                //----------------------------------------------------
                else if (aggSym.IsEnum) // if (aggSym.IsFabricated)
                {
                    // Get the underlying type. Enums can't be partial, so using DeclOnly is OK here.
                    BASENODE baseTypeNode = aggSym.DeclOnly().ParseTreeNode.AsAGGREGATE.BasesNode;
                    PREDEFTYPE basePredefType = PREDEFTYPE.INT; // Default type of enum is int.

                    if (baseTypeNode != null)
                    {
                        if (baseTypeNode.Kind == NODEKIND.PREDEFINEDTYPE)
                        {
                            basePredefType = (PREDEFTYPE)(baseTypeNode as PREDEFINEDTYPENODE).Type;
                        }
                        else
                        {
                            // Parser should have generated an error.
                            DebugUtil.Assert(Compiler.ErrorCount() > 0);

                            TYPESYM typeBase = TypeBind.BindTypeAggDeclExt(
                                Compiler,
                                baseTypeNode.AsANYTYPE,
                                aggSym.DeclOnly(),
                                TypeBindFlagsEnum.None);
                            if (typeBase.IsPredefined())
                            {
                                basePredefType = typeBase.GetAggregate().PredefinedTypeID;
                            }
                        }

                        switch (basePredefType)
                        {
                            case PREDEFTYPE.BYTE:
                            case PREDEFTYPE.SHORT:
                            case PREDEFTYPE.INT:
                            case PREDEFTYPE.LONG:
                            case PREDEFTYPE.SBYTE:
                            case PREDEFTYPE.USHORT:
                            case PREDEFTYPE.UINT:
                            case PREDEFTYPE.ULONG:
                                break;

                            default:
                                // Parser should have errored on this already.
                                DebugUtil.Assert(Compiler.ErrorCount() > 0);
                                basePredefType = PREDEFTYPE.INT;
                                break;
                        }
                    }

                    AGGSYM baseAggSym = Compiler.GetReqPredefAgg(basePredefType, false);

                    DebugUtil.VsVerify(
                        ResolveInheritanceRec(baseAggSym),
                        "ResolveInheritanceRec failed on a predefined type!");
                    aggSym.UnderlyingTypeSym = baseAggSym.GetThisType();

                    // Enums should derive from "Enum".
                    AGGSYM enumAggSym = Compiler.GetReqPredefAgg(PREDEFTYPE.ENUM, false);
                    DebugUtil.VsVerify(
                        ResolveInheritanceRec(enumAggSym),
                        "ResolveInheritanceRec failed on PREDEFTYPE.ENUM!");
                    Compiler.SetBaseType(aggSym, enumAggSym.GetThisType());
                    Compiler.SetIfaces(aggSym, (AGGTYPESYM[])null);
                }
                //----------------------------------------------------
                // IsSource: Delegate
                //----------------------------------------------------
                else if (aggSym.IsDelegate) // if (aggSym.IsFabricated) else if (aggSym.IsEnum)
                {
                    // all delegates in C# are multicast
                    AGGSYM aggDel = Compiler.GetReqPredefAgg(PREDEFTYPE.MULTIDEL, false);
                    DebugUtil.VsVerify(
                        ResolveInheritanceRec(aggDel),
                        "ResolveInheritanceRec failed on PREDEFTYPE.MUTLIDEL!");
                    Compiler.SetBaseType(aggSym, aggDel.GetThisType());
                    Compiler.SetIfaces(aggSym, (TypeArray)null);
                }
                //----------------------------------------------------
                // IsSource: Otherwise (Not fabricated, enum, delegate)
                //----------------------------------------------------
                else // if (aggSym.IsFabricated) else if (aggSym.IsEnum) else if (aggSym.IsDelegate)
                {
                    DebugUtil.Assert(aggSym.HasParseTree);

                    // Note - we do not declare the types and other symbols 
                    // involved in the constraint until
                    // right at the very end when we are trying to satisfy the constraints.
                    // That is, the type symbol gets fully created by the step below, but 
                    // not all the aggregate types involved in the type symbol get
                    // prepared/declared, because we won't need that information until 
                    // we try to satisfy the constraints (e.g. then we might need to know
                    // subtyping information etc.)

                    TypeArray ifaceArray = new TypeArray();
                    TypeArray thisIfaceArray = new TypeArray();
                    AGGTYPESYM baseClassSym = null;
                    bool nextLoop;

                    //------------------------------------------------
                    // IsSource: Otherwise (1)
                    //
                    // Make sure that the base is set for structs.
                    //------------------------------------------------
                    if (aggSym.IsStruct)
                    {
                        baseClassSym = Compiler.GetReqPredefType(PREDEFTYPE.VALUE, false);
                        DebugUtil.VsVerify(
                            ResolveInheritanceRec(baseClassSym.GetAggregate()),
                            "ResolveInheritanceRec failed on PREDEFTYPE.VALUE!");
                    }

                    //------------------------------------------------
                    // IsSource: Otherwise (2)
                    //
                    // Examine all the base class/interface declaration,
                    // from all partial classes, and combine them, giving an error on any duplicates.
                    //------------------------------------------------
                    for (AGGDECLSYM aggDeclSym = aggSym.FirstDeclSym;
                        aggDeclSym != null;
                        aggDeclSym = aggDeclSym.NextDeclSym)
                    {
                        BASENODE listNode = aggDeclSym.ParseTreeNode.AsAGGREGATE.BasesNode;
                        TYPEBASENODE baseNode = null;

                        bool isFirst = aggSym.IsClass;
                        bool seenBaseOnThisDecl = false;
                        int cifaceThis = 0;

                        for (; listNode != null; isFirst = false)
                        {
                            if (listNode.Kind == NODEKIND.LIST)
                            {
                                baseNode = listNode.AsLIST.Operand1.AsTYPEBASE;
                                listNode = listNode.AsLIST.Operand2;
                            }
                            else
                            {
                                baseNode = listNode.AsTYPEBASE;
                                listNode = null;
                            }

                            //----------------------------------------
                            // IsSource: Otherwise (2-1)
                            //
                            // Search in type variables of aggSym first,
                            // then the containing declaration.
                            //----------------------------------------
                            TYPESYM typeBaseSym = TypeBind.BindTypeAggDeclExt(
                                Compiler,
                                baseNode,
                                aggDeclSym,
                                TypeBindFlagsEnum.None);
                            DebugUtil.Assert(typeBaseSym != null);

                            if (typeBaseSym.IsERRORSYM)
                            {
                                // Already reported error in BindType....
                                continue;
                            }

                            if (!typeBaseSym.IsAGGTYPESYM)
                            {
                                if (typeBaseSym.IsTYVARSYM)
                                {
                                    // A base class cannot be a type parameter on its own.
                                    Compiler.Error(
                                        baseNode,
                                        CSCERRID.ERR_DerivingFromATyVar,
                                        new ErrArgTypeNode(baseNode, ErrArgFlagsEnum.None));
                                }
                                else
                                {
                                    DebugUtil.Assert(!typeBaseSym.IsERRORSYM);
                                    Compiler.Error(baseNode, CSCERRID.ERR_BadBaseType);
                                }
                                continue;
                            }

                            AGGTYPESYM baseAggTypeSym = typeBaseSym as AGGTYPESYM;
                            AGGSYM baseAggSym = baseAggTypeSym.GetAggregate();

                            if (!ResolveInheritanceRec(baseAggSym) && !baseAggSym.IsResolvingBaseClasses)
                            {
                                // Error should have already been reported.
                                // Fix the dependency by not using this base.
                                continue;
                            }

                            //----------------------------------------
                            // base class
                            //----------------------------------------
                            if (!baseAggSym.IsInterface)
                            {
                                if (!isFirst)
                                {
                                    // Found non-interface where interface was expected.
                                    if (aggSym.IsClass && baseAggSym.IsClass)
                                    {
                                        if (!seenBaseOnThisDecl)
                                        {
                                            // The base class wasn't first in the list. Give an error, fix it up, and go on.
                                            Compiler.Error(
                                                baseNode,
                                                CSCERRID.ERR_BaseClassMustBeFirst,
                                                new ErrArgTypeNode(baseNode, ErrArgFlagsEnum.None),
                                                new ErrArgRefOnly(baseAggSym));
                                        }
                                        else
                                        {
                                            // We have multiple base classes. Ignore the second one.
                                            Compiler.Error(
                                                baseNode,
                                                CSCERRID.ERR_NoMultipleInheritance,
                                                new ErrArg(aggSym),
                                                new ErrArg(baseClassSym),
                                                new ErrArgTypeNode(baseNode, ErrArgFlagsEnum.None),
                                                new ErrArgRefOnly(baseAggSym));
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // This is either a struct or an interface or the base type is not a class.
                                        Compiler.Error(
                                            baseNode,
                                            CSCERRID.ERR_NonInterfaceInInterfaceList,
                                            new ErrArgTypeNode(baseNode, ErrArgFlagsEnum.None),
                                            new ErrArgRefOnly(baseAggSym));
                                        continue;
                                    }
                                }

                                if (baseAggSym.IsPredefinedType &&
                                    (baseAggSym.PredefinedTypeID == PREDEFTYPE.ENUM ||
                                    baseAggSym.PredefinedTypeID == PREDEFTYPE.VALUE ||
                                    baseAggSym.PredefinedTypeID == PREDEFTYPE.DELEGATE ||
                                    baseAggSym.PredefinedTypeID == PREDEFTYPE.MULTIDEL ||
                                    (baseAggSym.PredefinedTypeID == PREDEFTYPE.ARRAY && !Compiler.IsBuildingMSCORLIB())) &&
                                    !aggSym.IsPredefinedType)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_DeriveFromEnumOrValueType,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(baseAggSym));
                                    continue;
                                }

                                if (baseAggSym.IsResolvingBaseClasses)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_CircularBase,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(baseAggTypeSym));
                                    continue;
                                }

                                // Base class.
                                if (baseClassSym == baseAggTypeSym)
                                {
                                    seenBaseOnThisDecl = true;
                                    continue;
                                }

                                if (baseClassSym != null)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_PartialMultipleBases,
                                        new ErrArgRef(aggSym));
                                    continue;
                                }

                                // A static class may not include a class-base specification (§10.1.4)
                                // and cannot explicitly specify a base class or a list of implemented interfaces.
                                if (aggSym.IsStatic && !baseAggTypeSym.IsPredefType(PREDEFTYPE.OBJECT))
                                {
                                    Compiler.Error(
                                        baseNode,
                                        CSCERRID.ERR_StaticDerivedFromNonObject,
                                        new ErrArg(aggSym), new ErrArg(baseAggTypeSym));
                                    continue;
                                }

                                // Generics cannot be attributes.
                                if (aggSym.AllTypeVariables.Count > 0 && baseAggSym.IsAttribute)
                                {
                                    Compiler.Error(
                                        baseNode,
                                        CSCERRID.ERR_GenericDerivingFromAttribute,
                                        new ErrArgTypeNode(baseNode, ErrArgFlagsEnum.None));
                                    // Set it anyway.
                                }

                                // Cannot derive from static or sealed types
                                if (!Compiler.CheckForStaticClass(
                                        null,
                                        aggSym,
                                        typeBaseSym,
                                        CSCERRID.ERR_StaticBaseClass)
                                    && baseAggSym.IsSealed)
                                {
                                    Compiler.ErrorRef(
                                        null,
                                        CSCERRID.ERR_CantDeriveFromSealedType,
                                        new ErrArgRef(aggSym),
                                        new ErrArgRef(baseAggTypeSym));
                                    // Set it anyway.
                                }

                                // make sure base class is at least as visible as derived class.
                                CheckConstituentVisibility(aggSym, baseAggTypeSym, CSCERRID.ERR_BadVisBaseClass);

                                seenBaseOnThisDecl = true;
                                baseClassSym = baseAggTypeSym;

                                continue;
                            } // if (!baseAggSym.IsInterface)

                            //----------------------------------------
                            // Interfaces. From here, process the interfaces.
                            //----------------------------------------
                            DebugUtil.Assert(baseAggSym.IsInterface);

                            if (aggSym.IsStatic)
                            {
                                Compiler.ErrorRef(
                                    null,
                                    CSCERRID.ERR_StaticClassInterfaceImpl,
                                    new ErrArgRef(baseAggTypeSym),
                                    new ErrArgRef(aggSym));
                                //LBadIface:
                                continue;
                            }

                            // found an interface, check that it wasn't listed twice.
                            // We check for unification later - when we build the combined list.
                            nextLoop = false;
                            for (int i = 0; i < thisIfaceArray.Count; i++)
                            {
                                if (thisIfaceArray[i] == baseAggTypeSym)
                                {
                                    // Only report the error on this decl.
                                    Compiler.Error(
                                        baseNode,
                                        CSCERRID.ERR_DuplicateInterfaceInBaseList,
                                        new ErrArgTypeNode(baseNode, ErrArgFlagsEnum.None));
                                    //goto LBadIface;
                                    nextLoop = true;
                                    break;
                                }
                            }
                            if (nextLoop)
                            {
                                continue;
                            }

                            if (baseAggSym.IsResolvingBaseClasses)
                            {
                                // Found a cycle, report error and don't add interface to list.
                                Compiler.ErrorRef(
                                    null,
                                    CSCERRID.ERR_CycleInInterfaceInheritance,
                                    new ErrArgRef(baseAggTypeSym),
                                    new ErrArgRef(aggSym));
                                continue;
                            }

                            // found an interface, check that it wasn't listed twice.
                            // We check for unification later - when we build the combined list.
                            for (int i = 0; ; i++)
                            {
                                if (i >= ifaceArray.Count)
                                {
                                    // If this sym is not in array, add it.
                                    ifaceArray.Add(baseAggTypeSym);
                                    break;
                                }
                                if (ifaceArray[i] == baseAggTypeSym)
                                {
                                    // Already in the list - don't add it to the total list.
                                    break;
                                }
                            }

                            thisIfaceArray.Add(baseAggTypeSym);

                            // Make sure base interface is at least as visible as derived interface (don't check
                            // for derived CLASS, though).
                            if (aggSym.IsInterface)
                            {
                                CheckConstituentVisibility(aggSym, baseAggTypeSym, CSCERRID.ERR_BadVisBaseInterface);
                            }
                        } // for (; listNode != null; isFirst = false)
                    }
                    // for (AGGDECLSYM  aggDeclSym = aggSym.FirstDeclSym; aggDeclSym!=null; aggDeclSym = aggDeclSym.NextDeclSym)


                    if (baseClassSym == null && aggSym.IsClass)
                    {
                        baseClassSym = Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, false);
                    }

                    Compiler.SetIfaces(aggSym, ifaceArray);
                    Compiler.SetBaseType(aggSym, baseClassSym);
                } // if (aggSym.IsFabricated)

                DebugUtil.Assert(aggSym.AggState == AggStateEnum.ResolvingInheritance);
                aggSym.AggState = AggStateEnum.Inheritance;
            } // if (!aggSym.IsSource)
            DebugUtil.Assert(aggSym.Interfaces != null && aggSym.AllInterfaces != null);

            // Set inherited bits.
            if (aggSym.BaseClassSym != null)
            {
                AGGSYM baseAggSym = aggSym.BaseClassSym.GetAggregate();
                DebugUtil.Assert(baseAggSym.HasResolvedBaseClasses);

                if (baseAggSym.IsAttribute)
                {
                    aggSym.IsAttribute = true;
                }
                if (baseAggSym.IsSecurityAttribute)
                {
                    aggSym.IsSecurityAttribute = true;
                }
                if (baseAggSym.IsMarshalByRef)
                {
                    aggSym.IsMarshalByRef = true;
                }
            }

            return returnValue;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineBounds
        //
        /// <summary>
        /// define bounds for all types in a namespace Declaration
        /// </summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void DefineBounds(NSDECLSYM nsDeclSym)
        {
            if (nsDeclSym == null)
            {
                return;
            }

            Compiler.SetLocation(nsDeclSym);   //SETLOCATIONSYM(nsDeclSym);

            for (SYM sym = nsDeclSym.FirstChildSym; sym != null; sym = sym.NextSym)
            {
                switch (sym.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        DefineBounds(sym as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        if ((sym as AGGDECLSYM).IsFirst)
                        {
                            DefineBounds((sym as AGGDECLSYM).AggSym);
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineBounds
        //
        /// <summary>
        /// define bounds for this type and it's nested types
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void DefineBounds(AGGSYM aggSym)
        {
            //--------------------------------------------------
            // determine bounds for this type
            //--------------------------------------------------
            if (aggSym.TypeVariables.Count > 0)
            {
                if (aggSym.HasParseTree)
                {
                    if (aggSym.IsDelegate)
                    {
                        // OK to get DeclOnly, because delegates can't be partial.
                        DELEGATENODE delegateNode = aggSym.DeclOnly().ParseTreeNode as DELEGATENODE;
                        DefineBounds(delegateNode.ConstraintsNode, aggSym.DeclOnly(), true);
                    }
                    else
                    {
                        DebugUtil.Assert(
                            aggSym.AggKind == AggKindEnum.Class ||
                            aggSym.AggKind == AggKindEnum.Struct ||
                            aggSym.AggKind == AggKindEnum.Interface);
                        DebugUtil.Assert(!aggSym.IsFabricated);

                        // Handle bounds in each declaration and check that they are the same.
                        // Just call DefineBounds for each. DefineBounds checks for consistency.
                        bool isFirst = true;

                        for (AGGDECLSYM aggDeclSym = (aggSym).FirstDeclSym;
                            aggDeclSym != null;
                            aggDeclSym = aggDeclSym.NextDeclSym)
                        {
                            BASENODE boundNode = aggDeclSym.ParseTreeNode.AsAGGREGATE.ConstraintsNode;
                            if (boundNode != null)
                            {
                                DefineBounds(boundNode, aggDeclSym, isFirst);
                                isFirst = false;
                            }
                        }

                        if (isFirst)
                        {
                            DefineBounds(null, aggSym.FirstDeclSym, isFirst);
                        }
                    }

                    CheckBoundsVisibility(aggSym, aggSym.TypeVariables);
                }
                else // if (aggSym.HasParseTree)
                {
                    DebugUtil.Assert(aggSym.TypeVariables.ItemAsTYVARSYM(0).FResolved());
                }
            } // if (aggSym.TypeVariables.Count>0)

            //--------------------------------------------------
            // determine bounds for nested types
            //--------------------------------------------------
            for (SYM elem = (aggSym).FirstChildSym; elem != null; elem = elem.NextSym)
            {
                Compiler.SetLocation(elem);    //SETLOCATIONSYM(elem);

                switch (elem.Kind)
                {
                    case SYMKIND.AGGSYM:
                        DebugUtil.Assert(
                            (elem as AGGSYM).AggKind > AggKindEnum.Unknown &&
                            (elem as AGGSYM).AggKind < AggKindEnum.Lim);
                        DefineBounds(elem as AGGSYM);
                        break;

                    case SYMKIND.AGGTYPESYM:
                    case SYMKIND.TYVARSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown member");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.prepareNamespace
        //
        /// <summary>
        /// prepare a namespace for compilation.
        /// this merely involves preparing all the namespace elements.
        /// nspace is the symbol for THIS namespace, not its parent...
        /// </summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void PrepareNamespace(NSDECLSYM nsDeclSym)
        {
            if (nsDeclSym == null)
            {
                return;
            }
            DebugUtil.Assert(nsDeclSym.IsDefined);
            Compiler.SetLocation(nsDeclSym);	//SETLOCATIONSYM(nsDeclSym);

            // prepare members

            for (SYM elem = nsDeclSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                switch (elem.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        PrepareNamespace(elem as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        if ((elem as AGGDECLSYM).IsFirst)
                        {
                            PrepareAggregate((elem as AGGDECLSYM).AggSym);
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown namespace member");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForTypeErrors (1)
        //
        /// <summary></summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void CheckForTypeErrors(NSDECLSYM nsDeclSym)
        {
            if (nsDeclSym == null)
            {
                return;
            }

            Compiler.SetLocation(nsDeclSym);    //SETLOCATIONSYM(nsDeclSym);

            for (SYM elem = nsDeclSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                switch (elem.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        CheckForTypeErrors(elem as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        AGGDECLSYM declSym = elem as AGGDECLSYM;
                        if (declSym.IsFirst)
                        {
                            CheckForTypeErrors(declSym.AggSym);
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForTypeErrors (2)
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void CheckForTypeErrors(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.IsPrepared);

            if (!aggSym.HasParseTree)
            {
                return;
            }

            Compiler.SetLocation(aggSym);   // SETLOCATIONSYM(aggSym);

            // We would really like to report the errors only on the partial class that actually declared
            // the element that is in error, but there doesn't seem to be a way to find out this information,
            // so report the error on all the declarations by passing null as the treeNode.

            AGGDECLSYM aggDeclSym = aggSym.FirstDeclSym;
            BASENODE treeNode = aggDeclSym.NextDeclSym != null ? null : aggDeclSym.ParseTreeNode;

            if (aggSym.BaseClassSym != null)
            {
                CheckForTypeErrors(treeNode, aggSym, aggSym.BaseClassSym);
            }

            for (int i = 0; i < aggSym.Interfaces.Count; ++i)
            {
                AGGTYPESYM iface = aggSym.Interfaces[i] as AGGTYPESYM;
                CheckForTypeErrors(treeNode, aggSym, iface);
            }

            for (int i = 0; i < aggSym.TypeVariables.Count; ++i)
            {
                TypeArray bnds = (aggSym.TypeVariables[i] as TYVARSYM).BoundArray;
                for (int j = 0; j < bnds.Count; ++j)
                {
                    CheckForTypeErrors(treeNode, aggSym, bnds[j]);
                }
            }

            if (aggSym.IsDelegate)
            {
                METHSYM invokeMethod = Compiler.MainSymbolManager.LookupInvokeMeth(aggSym);
                CheckForTypeErrors(treeNode, aggSym, invokeMethod);
            }

            for (SYM elem = aggSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                Compiler.SetLocation(elem);// SETLOCATIONSYM(elem);

                switch (elem.Kind)
                {
                    case SYMKIND.AGGSYM:
                        CheckForTypeErrors(elem as AGGSYM);
                        break;

                    case SYMKIND.METHSYM:
                        switch ((elem as METHSYM).MethodKind)
                        {
                            default:
                                DebugUtil.Assert(false);
                                goto case MethodKindEnum.Anonymous;

                            case MethodKindEnum.None:
                            case MethodKindEnum.Ctor:
                            case MethodKindEnum.Dtor:
                            case MethodKindEnum.ExplicitConv:
                            case MethodKindEnum.ImplicitConv:
                            case MethodKindEnum.Anonymous:
                                METHPROPSYM mpSym = elem as METHPROPSYM;
                                CheckForTypeErrors(mpSym.ParseTreeNode, elem, mpSym);
                                break;

                            case MethodKindEnum.PropAccessor:
                            case MethodKindEnum.EventAccessor:
                            case MethodKindEnum.Invoke:
                                break;
                        }
                        break;

                    case SYMKIND.PROPSYM:
                        {
                            METHPROPSYM mpSym = elem as METHPROPSYM;
                            CheckForTypeErrors(mpSym.ParseTreeNode, elem, mpSym);
                        }
                        break;

                    case SYMKIND.EVENTSYM:
                        {
                            EVENTSYM eventSym = elem as EVENTSYM;
                            CheckForTypeErrors(eventSym.ParseTreeNode, elem, eventSym.TypeSym);
                        }
                        break;

                    case SYMKIND.MEMBVARSYM:
                        {
                            if (!(elem as MEMBVARSYM).IsEvent)
                            {
                                MEMBVARSYM fieldSym = elem as MEMBVARSYM;
                                CheckForTypeErrors(fieldSym.ParseTreeNode, elem, fieldSym.TypeSym);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForTypeErrors (3)
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="contextSym"></param>
        /// <param name="methPropSym"></param>
        //------------------------------------------------------------
        internal void CheckForTypeErrors(
            BASENODE treeNode,
            SYM contextSym,
            METHPROPSYM methPropSym)
        {
            if (methPropSym.IsMETHSYM)
            {
                // Check the type variable bounds.
                METHSYM methodSym = methPropSym as METHSYM;
                for (int i = 0; i < methodSym.TypeVariables.Count; ++i)
                {
                    TypeArray bnds = (methodSym.TypeVariables.ItemAsTYVARSYM(i)).BoundArray;
                    for (int j = 0; j < bnds.Count; ++j)
                    {
                        CheckForTypeErrors(treeNode, contextSym, bnds[j]);
                    }
                }
            }

            // Check the return type.
            CheckForTypeErrors(treeNode, contextSym, methPropSym.ReturnTypeSym);

            // Check the parameter types.
            for (int i = 0; i < methPropSym.ParameterTypes.Count; ++i)
            {
                CheckForTypeErrors(treeNode, contextSym, methPropSym.ParameterTypes[i]);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckForTypeErrors (4)
        //
        /// <summary>
        /// Checks the TYPESYM for bogus, deprecated and constraints.
        /// If tree is NULL, symCtx is also used for reporting the error.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="contextSym"></param>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void CheckForTypeErrors(BASENODE treeNode, SYM contextSym, TYPESYM typeSym)
        {
            DebugUtil.Assert(Compiler.CompilationPhase >= CompilerPhaseEnum.PostPrepare);
            Compiler.EnsureState(typeSym, AggStateEnum.Prepared);
            DebugUtil.Assert(typeSym.IsPrepared);
            TYPESYM nakedTypeSym = typeSym.GetNakedType(false);

            // Convert NUBSYM to AGGTYPESYM.
            if (nakedTypeSym.IsNUBSYM)
            {
                nakedTypeSym = (nakedTypeSym as NUBSYM).GetAggTypeSym();
                if (nakedTypeSym == null)
                {
                    DebugUtil.VsFail("Why is there a NUBSYM here when Nullable<T> doesn't exist?");
                    return;
                }
            }

            if (nakedTypeSym.IsAGGTYPESYM && (nakedTypeSym as AGGTYPESYM).AllTypeArguments.Count > 0)
            {
                // Check the type arguments first.
                TypeArray typeArgs = (nakedTypeSym as AGGTYPESYM).AllTypeArguments;
                for (int i = 0; i < typeArgs.Count; ++i)
                {
                    CheckForTypeErrors(treeNode, contextSym, typeArgs[i]);
                }
            }

            if (Compiler.CheckBogus(typeSym))
            {
                if (treeNode == null)
                {
                    Compiler.ErrorRef(null, CSCERRID.ERR_BogusType,
                        new ErrArgRefOnly(contextSym), new ErrArgRef(typeSym));
                }
                else
                {
                    Compiler.ErrorRef(treeNode, CSCERRID.ERR_BogusType, new ErrArgRef(typeSym));
                }
            }
            else
            {
                if (typeSym.IsPTRSYM)
                {
                    CheckUnmanaged(treeNode, typeSym);
                }
                // Don't report deprecated warning if any of our containing classes are deprecated.
                if (typeSym.IsDeprecated() && !contextSym.IsContainedInDeprecated())
                {
                    ReportDeprecated(treeNode, contextSym, new SymWithType(typeSym, null));
                }
                TypeBind.CheckConstraints(Compiler, treeNode, typeSym, CheckConstraintsFlagsEnum.Outer);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineObject
        //
        /// <summary>
        /// <para>handles the special case of bringing object up to defined state</para>
        /// <para>Imports all members of System.Object, and set its state declared.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void DefineObject()    // for our friend the symmgr
        {
            AGGSYM objectSym = Compiler.GetReqPredefAgg(PREDEFTYPE.OBJECT, false);

            DebugUtil.Assert(objectSym != null);
            DebugUtil.Assert(
                AggStateEnum.Inheritance <= objectSym.AggState &&
                objectSym.AggState < AggStateEnum.DefiningMembers);

            Compiler.SetLocation(objectSym);   //SETLOCATIONSYM(objectSym);

            if (objectSym.IsSource)
            {
                // we are defining object. Better not have a base class.
                for (AGGDECLSYM aggDeclSym = objectSym.FirstDeclSym;
                    aggDeclSym != null;
                    aggDeclSym = aggDeclSym.NextDeclSym)
                {
                    BASENODE node = aggDeclSym.ParseTreeNode.AsAGGREGATE.BasesNode;
                    for (; node != null; node = node.NextNode)
                    {
                        Compiler.ErrorRef(
                            aggDeclSym.ParseTreeNode,
                            CSCERRID.ERR_ObjectCantHaveBases,
                            new ErrArgRef(objectSym));
                    }
                }

                objectSym.AggState = AggStateEnum.DefinedMembers;
                DefineAggregateMembers(objectSym);
            }
            else // if (objectSym.IsSource)
            {
                // import all of its members from the metadata file
                Compiler.Importer.DefineImportedType(objectSym);

                DebugUtil.Assert(objectSym.IsDefined);
                DebugUtil.Assert(objectSym.DeclOnly().GetInputFile().IsBaseClassLibrary);

                METHSYM dtor = Compiler.MainSymbolManager.LookupAggMember(
                    Compiler.NameManager.GetPredefinedName(PREDEFNAME.DTOR),
                    objectSym,
                    SYMBMASK.METHSYM) as METHSYM;
                if (dtor != null)
                {
                    dtor.MethodKind = MethodKindEnum.Dtor;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineNamespace
        //
        /// <summary>
        /// define a namespace by binding its name to a symbol and
        /// resolving the uses clauses in this namespace declaration.
        /// nspace indicates the PARENT namaspace
        /// </summary>
        /// <param name="nsDeclaration"></param>
        //------------------------------------------------------------
        internal void DefineNamespace(NSDECLSYM nsDeclSym)
        {
            Compiler.SetLocation(nsDeclSym);

            EnsureUsingClausesAreResolved(nsDeclSym);
            DebugUtil.Assert(nsDeclSym.UsingClausesResolved);

            //--------------------------------------------------
            // force binding of using aliases. Up to this point they are lazily bound
            //--------------------------------------------------
            foreach (SYM usingSym in nsDeclSym.UsingClauseSymList)
            {
                if (usingSym.IsALIASSYM)
                {
                    TypeBind.BindUsingAlias(Compiler, usingSym as ALIASSYM);
                }
            }

            //--------------------------------------------------
            // handle global attributes
            //--------------------------------------------------
            BASENODE node = nsDeclSym.ParseTreeNode.GlobalAttributeNode;
            while (node != null)
            {
                ATTRDECLNODE attr;
                if (node.Kind == NODEKIND.LIST)
                {
                    attr = node.AsLIST.Operand1 as ATTRDECLNODE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    attr = node as ATTRDECLNODE;
                    node = null;
                }
                DeclareGlobalAttribute(attr, nsDeclSym);
            }

            //--------------------------------------------------
            // and define contained types and namespaces...
            //--------------------------------------------------
            for (SYM elem = nsDeclSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                switch (elem.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        DefineNamespace(elem as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        if ((elem as AGGDECLSYM).IsFirst)
                        {
                            DefineAggregate((elem as AGGDECLSYM).AggSym);
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }

            nsDeclSym.IsDefined = true;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineAggregate
        //
        /// <summary>
        /// <para>define a class.  this means bind its base class and implemented interface
        /// list as well as define the class elements.</para>
        /// <para>gives an error if this type is involved in a cycle in the inheritance chain</para>
        /// </summary>
        /// <remarks>
        /// NOTE: this does not work for the predefined 'object' type. </STRIP>
        /// </remarks>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void DefineAggregate(AGGSYM aggSym)
        {
            DebugUtil.Assert(Compiler.CompilationPhase >= CompilerPhaseEnum.DefineMembers);
            DebugUtil.Assert(aggSym.AggKind > AggKindEnum.Unknown);
            DebugUtil.Assert(aggSym.IsSource);
            DebugUtil.Assert(aggSym.HasParseTree || aggSym.IsFabricated);

            // check if we're done already
            if (aggSym.AggState >= AggStateEnum.DefiningMembers)
            {
                DebugUtil.Assert(aggSym.AggState >= AggStateEnum.DefinedMembers);
                return;
            }

#if DEBUG
            // object shouldn't come through here.
            DebugUtil.Assert(!aggSym.IsPredefAgg(PREDEFTYPE.OBJECT));

            Compiler.HaveDefinedAnyType = true;

            // This used to call resolve inheritance. I (ShonK) don't think it needed to....
            DebugUtil.Assert(aggSym.HasResolvedBaseClasses);
#endif
            Compiler.SetLocation(aggSym);  //SETLOCATIONSYM(aggSym);
            aggSym.AggState = AggStateEnum.DefiningMembers;

            CheckForProtectedInSealed(aggSym);

            //--------------------------------------------------
            // do the members
            //--------------------------------------------------
            if (aggSym.IsEnum)
            {
                DefineEnumMembers(aggSym);
            }
            else if (aggSym.IsDelegate)
            {
                DefineDelegateMembers(aggSym);
            }
            else if (!aggSym.IsFabricated)
            {
                DefineAggregateMembers(aggSym);
            }

            DebugUtil.Assert(aggSym.AggState == AggStateEnum.DefiningMembers);
            aggSym.AggState = AggStateEnum.DefinedMembers;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineParameters
        //
        /// <summary>
        /// Create the parameter array from the nodeParams. *pfParams is an in/out parameter. This will
        /// be cleared if the last type is bad.
        /// </summary>
        /// <param name="contextSym"></param>
        /// <param name="parametersNode"></param>
        /// <param name="isUnsafe"></param>
        /// <param name="parametersArray"></param>
        /// <param name="hasParams"></param>
        //------------------------------------------------------------
        internal void DefineParameters(
            PARENTSYM contextSym,
            BASENODE parametersNode,
            bool isUnsafe,
            ref TypeArray parametersArray,
            ref bool hasParams)
        {
            int paramsCount = NodeUtil.CountBinOpListNode(parametersNode);
            BASENODE nodeLast = null;
            List<TYPESYM> typeList = new List<TYPESYM>();

            BASENODE node = parametersNode;
            while (node != null)
            {
                PARAMETERNODE paramNode;
                if (node.Kind == NODEKIND.LIST)
                {
                    paramNode = node.AsLIST.Operand1 as PARAMETERNODE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    paramNode = node as PARAMETERNODE;
                    node = null;
                }

                TYPESYM nodeTypeSym = TypeBind.BindType(
                    Compiler,
                    paramNode.TypeNode,
                    contextSym,
                    TypeBindFlagsEnum.None);
                DebugUtil.Assert(nodeTypeSym != null);
                CheckUnsafe(paramNode.TypeNode, nodeTypeSym, isUnsafe, CSCERRID.ERR_UnsafeNeeded);

                // Wrap it if the variable is byref.
                if ((paramNode.Flags & (NODEFLAGS.PARMMOD_REF | NODEFLAGS.PARMMOD_OUT)) != 0)
                {
                    nodeTypeSym = Compiler.MainSymbolManager.GetParamModifier(
                        nodeTypeSym,
                        (paramNode.Flags & NODEFLAGS.PARMMOD_OUT) != 0);

                    if ((nodeTypeSym as PARAMMODSYM).ParamTypeSym.IsSpecialByRefType())
                    {
                        Compiler.Error(
                            paramNode.TypeNode,
                            CSCERRID.ERR_MethodArgCantBeRefAny,
                            new ErrArg(nodeTypeSym));
                    }
                }
                typeList.Add(nodeTypeSym);
                DebugUtil.Assert(typeList.Count <= paramsCount);
                nodeLast = paramNode;
            }

            if (hasParams)
            {
                TYPESYM tempSym = typeList[typeList.Count - 1];
                hasParams = paramsCount > 0 && CheckParamsType(nodeLast, ref tempSym);
                typeList[typeList.Count - 1] = tempSym;
            }

            parametersArray = Compiler.MainSymbolManager.AllocParams(typeList);
        }

        // Once the "prepare" stage is done, classes must be compiled and metadata emitted
        // in three distinct stages in order to make sure that the metadata emitting is done
        // most efficiently. The stages are:
        //   EmitTypeDefs -- typedefs must be created for each aggregate types
        //   EmitMemberDefs -- memberdefs must be created for each member of aggregate types
        //   Compile -- Compile method bodies.
        //
        // To conform to the most efficient metadata emitting scheme, each of the three stages
        // must be done in the exact same order. Furthermore, the first stage must be done
        // in an order that does base classes and interfaces before derived classes and interfaces --
        // e.g., a topological sort of the classes. So every stage must go in this same topological
        // order.

        // Emit typedefs for all aggregates in this namespace declaration...  

        //------------------------------------------------------------
        // CLSDREC.EmitEnumdefsNamespace
        //
        /// <summary></summary>
        /// <param name="nsDeclSym"></param>
        /// <param name="aggQueue"></param>
        //------------------------------------------------------------
        internal void EmitEnumdefsNamespace(
            NSDECLSYM nsDeclSym,
            Queue<AGGSYM> aggQueue)
        {
            Compiler.SetLocation(nsDeclSym);

            // emit typedefs for each aggregate type.

            for (SYM elem = nsDeclSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                switch (elem.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        //EmitTypedefsNamespace(elem as NSDECLSYM);
                        EmitEnumdefsNamespace(elem as NSDECLSYM, aggQueue);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        if ((elem as AGGDECLSYM).IsFirst)
                        {
                            AGGSYM aggSym = (elem as AGGDECLSYM).AggSym;
                            if (aggSym.AggKind == AggKindEnum.Enum)
                            {
                                EmitTypedefsAggregate(aggSym);
                            }
                            else
                            {
                                aggQueue.Enqueue(aggSym);
                            }
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.EmitTypedefsNamespace
        //
        /// <summary></summary>
        /// <param name="aggQueue"></param>
        //------------------------------------------------------------
        internal void EmitTypedefsNamespace(Queue<AGGSYM> aggQueue)
        {
            while (aggQueue.Count > 0)
            {
                EmitTypedefsAggregate(aggQueue.Dequeue());
            }
        }

        //------------------------------------------------------------
        // CLSDREC.EmitMemberdefsNamespace
        //
        /// <summary>
        /// Emit memberdefs for all aggregates in this namespace...  
        /// </summary>
        /// <param name="nsDeclSym"></param>
            //------------------------------------------------------------
        internal void EmitMemberdefsNamespace(NSDECLSYM nsDeclSym)
        {
            Compiler.SetLocation(nsDeclSym);

            // emit memberdefs for each aggregate type.

            for (SYM elem = nsDeclSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                switch (elem.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        EmitMemberdefsNamespace(elem as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        if ((elem as AGGDECLSYM).IsFirst)
                        {
                            AGGSYM aggSym = (elem as AGGDECLSYM).AggSym;
                            EmitMemberdefsAggregate(aggSym);
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.EmitBasesNamespace
        //
        /// <summary>
        /// <para>Emit typerefs/defs/specs for the inheritance hierarchy.
        /// Make sure we traverse in the same order as above, just in case.</para>
        /// <para>This is structured as a seperate phase because there are corner cases
        /// where recursion between the base class and the current class is allowed, 
        /// e.g. class C : IList&lt;C&gt; { ... }</para>
        /// </summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void EmitBasesNamespace(NSDECLSYM nsDeclSym)
        {
            Compiler.SetLocation(nsDeclSym);

            // emit memberdefs for each aggregate type.

            for (SYM elem = nsDeclSym.FirstChildSym; elem != null; elem = elem.NextSym)
            {
                switch (elem.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        EmitBasesNamespace(elem as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        if ((elem as AGGDECLSYM).IsFirst)
                        {
                            EmitBasesAggregate((elem as AGGDECLSYM).AggSym);
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CompileNamespace
        //
        /// <summary>
        /// Compile all members of this namespace...  nspace is this namespace...
        /// </summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void CompileNamespace(NSDECLSYM nsDeclSym)
        {
            Compiler.SetLocation(nsDeclSym);

            // compile members

            for (SYM elementSym = nsDeclSym.FirstChildSym;
                elementSym != null;
                elementSym = elementSym.NextSym)
            {
                switch (elementSym.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        CompileNamespace(elementSym as NSDECLSYM);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        DebugUtil.Assert(
                            (elementSym as AGGDECLSYM).AggSym.AggKind > AggKindEnum.Unknown &&
                            (elementSym as AGGDECLSYM).AggSym.AggKind < AggKindEnum.Lim);
                        if (!(elementSym as AGGDECLSYM).IsFirst)
                        {
                            break;
                        }
                        //if (Compiler.Options.CompileSkeleton)
                        //{
                        //    CompileAggSkeleton((elementSym as AGGDECLSYM).AggSym);
                        //}
                        //else
                        {
                            AGGSYM aggSym = (elementSym as AGGDECLSYM).AggSym;
                            CompileAggregate(aggSym);
                            //CreateType(aggSym);
                        }
                        Compiler.FuncBRec.ResetUniqueNames();
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }

            // Do CLS name checking, after compiling members so we know who is Compliant
            DebugUtil.Assert(Compiler.AllowCLSErrors() || !Compiler.CheckForCLS);

            if (Compiler.CheckForCLS &&
                nsDeclSym.HasExternalAccess() && !nsDeclSym.NamespaceSym.CheckedForCLS)
            {
                CheckCLSnaming(nsDeclSym.NamespaceSym);
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CreateEnumTypesInNamespace
        //
        /// <summary>
        /// Call Type.CreateType() method for each class in the specified namespace.
        /// </summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void CreateEnumTypesInNamespace(
            NSDECLSYM nsDeclSym,
            Queue<AGGSYM> aggQueue)
        {
            Compiler.SetLocation(nsDeclSym);

            // compile members

            for (SYM elementSym = nsDeclSym.FirstChildSym;
                elementSym != null;
                elementSym = elementSym.NextSym)
            {
                switch (elementSym.Kind)
                {
                    case SYMKIND.NSDECLSYM:
                        CreateEnumTypesInNamespace(elementSym as NSDECLSYM, aggQueue);
                        break;

                    case SYMKIND.AGGDECLSYM:
                        DebugUtil.Assert(
                            (elementSym as AGGDECLSYM).AggSym.AggKind > AggKindEnum.Unknown &&
                            (elementSym as AGGDECLSYM).AggSym.AggKind < AggKindEnum.Lim);
                        if (!(elementSym as AGGDECLSYM).IsFirst)
                        {
                            break;
                        }
                        //if (Compiler.Options.CompileSkeleton)
                        //{
                        //    break;
                        //}
                        //else
                        {
                            AGGSYM aggSym = (elementSym as AGGDECLSYM).AggSym;
                            if (aggSym.AggKind == AggKindEnum.Enum)
                            {
                                CreateType(aggSym);
                            }
                            else
                            {
                                aggQueue.Enqueue(aggSym);
                            }
                        }
                        //Compiler.FuncBRec.ResetUniqueNames();
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown type");
                        break;
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CreateTypesInNamespace
        //
        /// <summary>
        /// Call Type.CreateType() method for each class in the specified namespace.
        /// </summary>
        /// <param name="aggQueue"></param>
        //------------------------------------------------------------
        internal void CreateTypesInNamespace(Queue<AGGSYM> aggQueue)
        {
            while (aggQueue.Count > 0)
            {
                CreateType(aggQueue.Dequeue());
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CreateType (1)
        //
        /// <summary>
        /// Call TypeBuilder.CreateType method on the specified AGGSYM and nested AGGSYMs
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void CreateType(AGGSYM aggSym)
        {
            if (aggSym == null || Compiler.FAbortCodeGen(0))
            {
                return;
            }
            if (aggSym.TypeCreated)
            {
                return;
            }

            CreateType(aggSym.BaseClassSym);

            TypeArray ifaces = aggSym.AllInterfaces;
            if (ifaces != null && ifaces.Count > 0)
            {
                for (int i = 0; i < ifaces.Count; ++i)
                {
                    CreateType(ifaces[i]);
                }
            }

            TypeArray typeVars = aggSym.TypeVariables;
            if (typeVars != null && typeVars.Count > 0)
            {
                for (int i = 0; i < typeVars.Count; ++i)
                {
                    CreateType(typeVars[i]);
                }
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            NestedTypeResolver resolver = new NestedTypeResolver(this.compiler, aggSym);
            ResolveEventHandler handler = new ResolveEventHandler(resolver.ResolveType);
            Exception excp;

            try
            {
                currentDomain.TypeResolve += handler;

                if (!aggSym.CreateType(out excp))
                {
                    this.compiler.Error(ERRORKIND.ERROR, excp);
                }
            }
            catch (TypeLoadException)
            {
                ;
            }
#if DEBUG
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
            }
#endif
            finally
            {
                currentDomain.TypeResolve -= handler;
            }

            // Nested classes must be done after other members.

            for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
            {
                AGGSYM childAggSym = child as AGGSYM;
                if (childAggSym != null)
                {
                    CreateType(childAggSym);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CreateType (2)
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void CreateType(TYPESYM typeSym)
        {
            if (typeSym == null)
            {
                return;
            }

            switch (typeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                case SYMKIND.DYNAMICSYM:    // CS4
                    CreateType((typeSym as AGGTYPESYM).GetAggregate());
                    return;

                case SYMKIND.ARRAYSYM:
                    CreateType((typeSym as ARRAYSYM).ElementTypeSym);
                    return;

                case SYMKIND.VOIDSYM:
                    return;

                case SYMKIND.PARAMMODSYM:
                    CreateType((typeSym as PARAMMODSYM).ParamTypeSym);
                    return;

                case SYMKIND.TYVARSYM:
                    TypeArray bnds = (typeSym as TYVARSYM).BoundArray;
                    if (bnds != null && bnds.Count > 0)
                    {
                        for (int i = 0; i < bnds.Count; ++i)
                        {
                            CreateType(bnds[i]);
                        }
                    }
                    return;

                case SYMKIND.PTRSYM:
                    CreateType((typeSym as PTRSYM).BaseTypeSym);
                    return;

                case SYMKIND.NUBSYM:
                    CreateType((typeSym as NUBSYM).BaseTypeSym);
                    return;

                case SYMKIND.NULLSYM:
                case SYMKIND.ERRORSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.ANONMETHSYM:
                case SYMKIND.METHGRPSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.IMPLICITTYPESYM:           // CS3
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM:   // CS3
                case SYMKIND.LAMBDAEXPRSYM:             // CS3
                default:
                    return;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.PrepareAggregate
        //
        /// <summary>
        /// <para>prepares a class for compilation by preparing all of its elements...</para>
        /// <para>This should also verify that a class actually implements its interfaces...</para>
        /// <para>This also calls defineAggregate() for the class, resolves its inheritance,
        /// creates its nested types &amp; prepares them
        /// - basically everything is brought up to fully-declared state except the code.</para>
        /// <para>Its type variables will already have been created.</para>
        /// <para>We do not have to force the declaration of the types involved in the constraints
        /// for the type variables until we actually try to satisfy the constraints.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void PrepareAggregate(AGGSYM aggSym)
        {
            // We shouldn't call this before we're in the Prepare stage.
            DebugUtil.Assert(Compiler.CompilationPhase >= CompilerPhaseEnum.Prepare);

            if (aggSym.IsFabricated)
            {
                DebugUtil.Assert(!aggSym.HasParseTree);
                DebugUtil.Assert(aggSym.IsPrepared);
                aggSym.SetBogus(false);
                return;
            }
            DebugUtil.Assert(!aggSym.IsSource == !aggSym.HasParseTree);

            // If this assert fires it probably means that
            // there's recursion in the base agg or base interface aggs.
            DebugUtil.Assert(aggSym.AggState != AggStateEnum.Preparing);

            if (aggSym.AggState >= AggStateEnum.Preparing)
            {
                DebugUtil.Assert(aggSym.AggKind > AggKindEnum.Unknown);
                return;
            }

            // This takes care of calling ResolveInheritanceRec if needed.
            if (!aggSym.HasParseTree && aggSym.AggState < AggStateEnum.DefinedMembers)
            {
                Compiler.Importer.DefineImportedType(aggSym);
                if (aggSym.AggState < AggStateEnum.DefinedMembers)
                {
                    return;
                }
                if (aggSym.AggState >= AggStateEnum.Prepared)
                {
                    return;
                }
            }

            DebugUtil.Assert(aggSym.AggKind > AggKindEnum.Unknown);
            DebugUtil.Assert(
                AggStateEnum.DefinedMembers <= aggSym.AggState &&
                aggSym.AggState < AggStateEnum.Preparing);

            aggSym.AggState = AggStateEnum.Preparing;
            Compiler.SetLocation(aggSym);  //SETLOCATIONSYM(aggSym);

            if (aggSym.BaseClassSym != null)
            {
                // Bring the base aggregate up to prepared - not the whole AGGTYPESYM.
                PrepareAggregate(aggSym.BaseClassSym.GetAggregate());
                // If our base class has conversion operators, we have conversion operators....
                if (aggSym.IsClass)
                {
                    aggSym.HasConversion |= aggSym.BaseClassSym.GetAggregate().HasConversion;
                }
            }

            for (int i = 0; i < aggSym.Interfaces.Count; ++i)
            {
                // Bring the iface aggregate up to prepared - not the whole AGGTYPESYM.
                AGGTYPESYM iface = aggSym.Interfaces[i] as AGGTYPESYM;
                PrepareAggregate(iface.GetAggregate());
            }

            if (aggSym.HasParseTree)
            {
                // Source type.

                if (aggSym.IsNested)
                {
                    // Look for an accessible member with the same name in a base class of our enclosing class.
                    // Check that the shadowing flags are correct.
                    NODEFLAGS flags = 0;
                    for (AGGDECLSYM decl = aggSym.FirstDeclSym; decl != null; decl = decl.NextDeclSym)
                    {
                        flags |= decl.ParseTreeNode.Flags;
                    }
                    CheckSimpleHiding(aggSym, flags);
                }

                switch (aggSym.AggKind)
                {
                    default:
                        DebugUtil.Assert(false);
                        goto case AggKindEnum.Struct;

                    case AggKindEnum.Class:
                    case AggKindEnum.Struct:
                        PrepareClassOrStruct(aggSym);
                        break;

                    case AggKindEnum.Delegate:
                    case AggKindEnum.Enum:
                        // Nothing to prepare for enums or delegates
                        aggSym.SetBogus(false);
                        aggSym.AggState = AggStateEnum.Prepared;
                        break;

                    case AggKindEnum.Interface:
                        PrepareInterface(aggSym);
                        break;
                }
                DebugUtil.Assert(aggSym.IsPrepared);
            }
            else
            {
                SetOverrideBits(aggSym);

                aggSym.SetBogus(aggSym.IsBogus);
                BuildOrCheckAbstractMethodsList(aggSym);
                aggSym.AggState = AggStateEnum.Prepared;
            }
            if (aggSym.IsSource &&
                (aggSym.IsClass || aggSym.IsStruct) &&
                aggSym.GetOutputFile().EntryClassName == null)
            {
                FindEntryPoint(aggSym);
            }
        }

        //------------------------------------------------------------
        // delegate CLSDREC.MEMBER_OPERATION
        //
        // typedef void (CLSDREC::*MEMBER_OPERATION)(SYM aggregateMember, VOID *info);
        //------------------------------------------------------------
        internal delegate void MEMBER_OPERATION(SYM aggregateMemberSym, object info);

        // defined in constructor.
        internal MEMBER_OPERATION MEMBER_OPERATION_EmitMemberDef = null;
        internal MEMBER_OPERATION MEMBER_OPERATION_CompileMember = null;
        internal MEMBER_OPERATION MEMBER_OPERATION_CompileMemberSkeleton = null;

        //------------------------------------------------------------
        // CLSDREC.EnumMembersInEmitOrder
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        /// <param name="info"></param>
        /// <param name="doMember"></param>
        //------------------------------------------------------------
        internal void EnumMembersInEmitOrder(AGGSYM aggSym, object info, MEMBER_OPERATION doMember)
        {
            // Constant fields go first
            for (SYM childSym = aggSym.FirstChildSym; childSym != null; childSym = childSym.NextSym)
            {
                if ((childSym is MEMBVARSYM) && (childSym as MEMBVARSYM).IsConst)
                {
                    doMember(childSym, info);
                }
            }

            // Emit the other children.
            for (SYM childSym = aggSym.FirstChildSym; childSym != null; childSym = childSym.NextSym)
            {

                Compiler.SetLocation(childSym);

                switch (childSym.Kind)
                {
                    case SYMKIND.MEMBVARSYM:
                        if (!(childSym as MEMBVARSYM).IsConst)
                        {
                            doMember(childSym, info);
                        }
                        break;

                    case SYMKIND.PROPSYM:
                        {
                            PROPSYM propertySym = childSym as PROPSYM;
                            if (propertySym.SetMethodSym != null && propertySym.GetMethodSym != null)
                            {
                                DebugUtil.Assert(
                                    propertySym.SetMethodSym.ParseTreeNode.Kind == NODEKIND.ACCESSOR ||
                                    propertySym.GetMethodSym.ParseTreeNode.Kind == NODEKIND.ACCESSOR);

                                // emit accessors in declared order
                                if (propertySym.SetMethodSym.ParseTreeNode.Kind != NODEKIND.ACCESSOR ||
                                    (propertySym.GetMethodSym.ParseTreeNode.Kind == NODEKIND.ACCESSOR &&
                                    (propertySym.GetMethodSym.ParseTreeNode as ACCESSORNODE).TokenIndex <
                                    (propertySym.SetMethodSym.ParseTreeNode as ACCESSORNODE).TokenIndex))
                                {
                                    doMember(propertySym.GetMethodSym, info);
                                    doMember(propertySym.SetMethodSym, info);
                                }
                                else
                                {
                                    doMember(propertySym.SetMethodSym, info);
                                    doMember(propertySym.GetMethodSym, info);
                                }
                            }
                            else if (propertySym.GetMethodSym != null)
                            {
                                doMember(propertySym.GetMethodSym, info);
                            }
                            else if (propertySym.SetMethodSym != null)
                            {
                                doMember(propertySym.SetMethodSym, info);
                            }
                        }
                        break;

                    case SYMKIND.METHSYM:
                        // accessors are done off of the property
                        if (!(childSym as METHSYM).IsAnyAccessor)
                        {
                            doMember(childSym, info);
                        }
                        break;

                    case SYMKIND.EVENTSYM:
                        {
                            // Emit accessors in declaration order.
                            // Note that if accessors weren't specified
                            // they have the same parse tree and we emit Add first.
                            EVENTSYM eventSym = childSym as EVENTSYM;
                            if (eventSym.RemoveMethodSym.ParseTreeNode.TokenIndex <
                                eventSym.AddMethodSym.ParseTreeNode.TokenIndex)
                            {
                                doMember(eventSym.RemoveMethodSym, info);
                                doMember(eventSym.AddMethodSym, info);
                            }
                            else
                            {
                                doMember(eventSym.AddMethodSym, info);
                                doMember(eventSym.RemoveMethodSym, info);
                            }
                        }
                        break;

                    case SYMKIND.TYVARSYM:
                    case SYMKIND.AGGTYPESYM:
                        break;

                    default:
                        break;
                }
            }

            // Properties/events must be done after methods.
            for (SYM childSym = aggSym.FirstChildSym; childSym != null; childSym = childSym.NextSym)
            {
                if (childSym.IsPROPSYM || childSym.IsEVENTSYM)
                {
                    doMember(childSym, info);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.EnsureUsingClausesAreResolved
        //
        /// <summary>
        /// ensures that the using clauses for this namespace declaration have been resolved.
        /// This can happen between declare and define steps.
        /// </summary>
        /// <param name="nsDeclSym"></param>
        //------------------------------------------------------------
        internal void EnsureUsingClausesAreResolved(NSDECLSYM nsDeclSym)
        {
            if (nsDeclSym.UsingClausesResolved)
            {
                return;
            }
            nsDeclSym.UsingClausesResolved = true;
            DebugUtil.Assert(nsDeclSym.UsingClauseSymList.Count == 0);
            List<SYM> addList = nsDeclSym.UsingClauseSymList;

            // First time through the loop, add all the extern aliases.
            // Second time add the normal using clauses and aliases.
            // This is so the normal ones can use the externs.

            for (bool ensuringExternAlias = true; ; )
            {
                BASENODE node = nsDeclSym.ParseTreeNode.UsingNode;
                USINGNODE usingNode = null;
                SYM symAdd = null;

                while (node != null)
                {
                    if (node.Kind == NODEKIND.LIST)
                    {
                        usingNode = node.AsLIST.Operand1 as USINGNODE;
                        node = node.AsLIST.Operand2;
                    }
                    else
                    {
                        usingNode = node as USINGNODE;
                        node = null;
                    }
                    if (usingNode == null)
                    {
                        continue;
                    }

                    //------------------------------
                    // "using" namespace-name ";"
                    //------------------------------
                    if (usingNode.AliasNode == null)
                    {
                        if (ensuringExternAlias)
                        {
                            continue;
                        }

                        // Find the type or namespace for the using clause.
                        BASENODE nameNode = usingNode.NameNode;
                        symAdd = TypeBind.BindName(Compiler, nameNode, nsDeclSym, TypeBindFlagsEnum.None);
                        DebugUtil.Assert(symAdd != null);

                        if (symAdd.IsERRORSYM)
                        {
                            // couldn't find the type/namespace
                            // error already reported
                            continue;
                        }

                        DebugUtil.Assert(symAdd.IsAGGTYPESYM || symAdd.IsNSAIDSYM);
                        if (!symAdd.IsNSAIDSYM)
                        {
                            Compiler.Error(
                                nameNode,
                                CSCERRID.ERR_BadUsingNamespace,
                                new ErrArgNameNode(nameNode, ErrArgFlagsEnum.None),
                                new ErrArgRefOnly(symAdd));
                            continue;
                        }

                        // Check for duplicate using clauses.
                        if (addList.Contains(symAdd))
                        {
                            Compiler.Error(
                                nameNode,
                                CSCERRID.WRN_DuplicateUsing,
                                new ErrArgNameNode(nameNode, ErrArgFlagsEnum.None));
                            continue;
                        }
                    }
                    //------------------------------
                    // "extern"  "alias"  identifier  ";"
                    // "using" identifier "=" namespace-or-type-name ";"
                    //------------------------------
                    else // if (usingNode.AliasNode == null)
                    {
                        // NameNode == null if an extern alias
                        if ((usingNode.NameNode == null) != ensuringExternAlias) continue;

                        string name = usingNode.AliasNode.Name;
                        SYM symDup = null;

                        foreach (SYM symTemp in addList)
                        {
                            if (symTemp.IsALIASSYM && symTemp.Name == name)
                            {
                                symDup = symTemp;
                                break;
                            }
                        }
                        if (symDup == null)
                        {
                            foreach (SYM symNext in addList)
                            {
                                SYM symTemp = symNext;
                                if (symTemp.IsALIASSYM && symTemp.Name == name)
                                {
                                    symDup = symTemp;
                                    break;
                                }
                            }
                        }
                        if (symDup != null)
                        {
                            Compiler.Error(
                                usingNode.AliasNode,
                                CSCERRID.ERR_DuplicateAlias,
                                new ErrArg(usingNode.AliasNode),
                                new ErrArgRefOnly(symDup));
                            continue;
                        }

                        for (symDup = Compiler.LookupInBagAid(name, nsDeclSym.NamespaceSym, 0, Kaid.Global, SYMBMASK.ALL);
                            symDup != null;
                            symDup = Compiler.LookupNextInAid(symDup, Kaid.Global, SYMBMASK.ALL))
                        {
                            if (symDup.IsAGGSYM && (symDup as AGGSYM).TypeVariables.Count > 0)
                            {
                                continue;
                            }
                            if (CheckAccess(symDup, null, nsDeclSym, null))
                            {
                                break;
                            }
                        }

                        ALIASSYM aliasSym = Compiler.MainSymbolManager.CreateAlias(name);
                        aliasSym.ParseTreeNode = usingNode;
                        aliasSym.ParentSym = nsDeclSym;
                        aliasSym.IsExtern = ensuringExternAlias;
                        aliasSym.DuplicatedSym = symDup;

                        symAdd = aliasSym;

                    } // if (usingNode.AliasNode == null)

                    Compiler.CompileCallback.ResolvedUsingNode(nsDeclSym, usingNode, symAdd);
                    addList.Add(symAdd);

                } // while (node != null)

                if (!ensuringExternAlias)
                {
                    break;
                }
                ensuringExternAlias = false;

            } // for (bool ensuringExternAlias = true; ; )
        }

        //------------------------------------------------------------
        // CLSDREC.CreateAnonymousMethodClass
        //
        /// <summary>
        /// Creates all the containing class and ctor to hold an anonymous method
        /// pOuterMeth is the method declaring the anonymous delegate
        /// </summary>
        /// <param name="otherMethodSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM CreateAnonymousMethodClass(METHSYM otherMethodSym)
        {
            // Create the class
            AGGDECLSYM clsdecl = AddSynthAgg(
                null,
                SpecialNameKindEnum.AnonymousMethodDisplayClass,
                otherMethodSym.ContainingDeclaration());
            AGGSYM cls = clsdecl.AggSym;
            DebugUtil.Assert(cls.DeclOnly() == clsdecl);

            // We also need to add in any possible method TyVars
            CopyMethTyVarsToClass(otherMethodSym, cls);

            compiler.SetIfaces(cls, (TypeArray)null);

            compiler.SetBaseType(cls, compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true));
            DefineAggregate(cls);
            cls.SetBogus(false);
            cls.AggState = AggStateEnum.PreparedMembers;

            // .ctor

            METHSYM ctorSym = FabricateSimpleMethod(PREDEFNAME.CTOR, clsdecl, compiler.MainSymbolManager.VoidSym);
            ctorSym.MethodKind = MethodKindEnum.Ctor;

            DebugUtil.Assert(ctorSym.ParameterTypes == BSYMMGR.EmptyTypeArray);
            DebugUtil.Assert(ctorSym.TypeVariables == BSYMMGR.EmptyTypeArray);

            return cls;
        }

        //------------------------------------------------------------
        // CLSDREC.EvaluateFieldConstant
        //
        /// <summary>
        /// <para>evaluate a field constant for fieldcurrent,</para>
        /// <para>fieldFirst is given only so that you know what to display
        /// in case of a circular definition error.</para>
        /// <para>returns true on success</para>
        /// </summary>
        /// <param name="firstFieldSym"></param>
        /// <param name="currentFieldSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool EvaluateFieldConstant(MEMBVARSYM firstFieldSym, MEMBVARSYM currentFieldSym)
        {
            if (firstFieldSym == null)
            {
                firstFieldSym = currentFieldSym;
            }
            DebugUtil.Assert(currentFieldSym.IsUnevaled);
            DebugUtil.Assert(firstFieldSym.IsUnevaled);

            // If IsConst field of MEMBVARSYM instances is true,
            // it is second time to start processing the instance.
            if (currentFieldSym.IsConst)
            {
                // Circular field definition.
                DebugUtil.Assert(
                    firstFieldSym.ClassSym.IsEnum ||
                    (firstFieldSym.ParseTreeNode as VARDECLNODE).ArgumentsNode != null);
                Compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_CircConstValue,
                    new ErrArgRef(firstFieldSym));
                currentFieldSym.IsConst = false;
                currentFieldSym.IsUnevaled = false;
                return false;
            }

            // Set the flag to mean we are evaling this field...
            currentFieldSym.IsConst = true;

            // const fields cannot be unsafe
            Compiler.FuncBRec.SetUnsafe(false);

            // and compile the parse tree:
            BASENODE expressionTreeNode = currentFieldSym.GetConstExprTree();

            //--------------------------------------------------------
            // If no value is specified, this should be enum-member-declaration.
            //--------------------------------------------------------
            if (expressionTreeNode == null)
            {
                DebugUtil.Assert(currentFieldSym.ClassSym.IsEnum);
                currentFieldSym.ConstVal = new CONSTVAL();

                // we have an enum member with no expression

                PREDEFTYPE fieldPredefType = currentFieldSym.TypeSym.UnderlyingEnumType().GetPredefType();
                MEMBVARSYM previousEnumeratorSym = currentFieldSym.GetPreviousEnumerator();

                // The previous enumerator must be present and evaled.
                // Evaluation may have resulted in an error in which case constVal may contain a null ptr.

                //----------------------------------------------------
                // Enum members, not first without values.
                //----------------------------------------------------
                if (previousEnumeratorSym != null &&
                    (!previousEnumeratorSym.IsUnevaled ||
                    EvaluateFieldConstant(firstFieldSym, previousEnumeratorSym)))
                {
                    Exception excp = null;
                    bool isOverFlow = false;

                    //------------------------------------------------
                    // we've evaluated our previous enumerator
                    // add one and check for overflow
                    //------------------------------------------------
                    switch (fieldPredefType)
                    {
                        case PREDEFTYPE.BYTE:
                            byte byteVal = previousEnumeratorSym.ConstVal.GetByte(out excp);
                            if (byteVal < System.Byte.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetByte((byte)(byteVal + 1));
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetByte((byte)0);
                                isOverFlow = true;
                            }
                            break;

                        case PREDEFTYPE.SHORT:
                            short shortVal = previousEnumeratorSym.ConstVal.GetShort(out excp);
                            if (shortVal < System.Int16.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetShort((short)(shortVal + 1));
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetShort((short)0);
                                isOverFlow = true;
                            }
                            break;

                        case PREDEFTYPE.INT:
                            int intVal = previousEnumeratorSym.ConstVal.GetInt(out excp);
                            if (intVal < System.Int32.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetInt(intVal + 1);
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetInt(0);
                                isOverFlow = true;
                            }
                            break;

                        case PREDEFTYPE.LONG:
                            long longVal = previousEnumeratorSym.ConstVal.GetLong(out excp);
                            if (longVal < System.Int64.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetLong(longVal + 1);
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetLong(0);
                                isOverFlow = true;
                            }
                            break;

                        case PREDEFTYPE.SBYTE:
                            sbyte sbyteVal = previousEnumeratorSym.ConstVal.GetSByte(out excp);
                            if (sbyteVal < System.SByte.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetSByte((sbyte)(sbyteVal + 1));
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetSByte((sbyte)0);
                                isOverFlow = true;
                            }
                            break;

                        case PREDEFTYPE.USHORT:
                            ushort ushortVal = previousEnumeratorSym.ConstVal.GetUShort(out excp);
                            if (ushortVal < System.UInt16.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetUShort((ushort)(ushortVal + 1));
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetUShort((ushort)0);
                                isOverFlow = true;
                            }
                            break;

                        case PREDEFTYPE.UINT:
                            uint uintVal = previousEnumeratorSym.ConstVal.GetUInt(out excp);
                            if (uintVal < System.UInt32.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetUInt(uintVal + 1);
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetUInt(0);
                                isOverFlow = true;
                            }
                            break;

                        case PREDEFTYPE.ULONG:
                            ulong ulongVal = previousEnumeratorSym.ConstVal.GetULong(out excp);
                            if (ulongVal < System.UInt64.MaxValue)
                            {
                                currentFieldSym.ConstVal.SetULong(ulongVal + 1);
                            }
                            else
                            {
                                currentFieldSym.ConstVal.SetULong(0);
                                isOverFlow = true;
                            }
                            break;

                        default:
                            DebugUtil.Assert(false);
                            break;
                    }

                    if (isOverFlow || excp != null)
                    {
                        Compiler.ErrorRef(null, CSCERRID.ERR_EnumeratorOverflow, new ErrArgRef(currentFieldSym));
                    }
                }
                else	// if (previousEnumeratorSym!=null && ...
                {
                    // we are the first enumerator, or we failed to evaluate our previos enumerator
                    // set constVal to the appropriate type of zero

                    currentFieldSym.ConstVal.SetObject(
                        Compiler.MainSymbolManager.GetPredefZero(fieldPredefType));
                }
            }
            //--------------------------------------------------
            // If constant values are specified
            //--------------------------------------------------
            else	// if (expressionTreeNode==null)
            {
                EXPR rval;
                if (currentFieldSym == firstFieldSym)
                {
                    rval = Compiler.FuncBRec.CompileFirstField(currentFieldSym, expressionTreeNode);
                }
                else
                {
                    rval = Compiler.FuncBRec.CompileNextField(currentFieldSym, expressionTreeNode);
                }

                // check that we really got a constant value

                if (rval.Kind != EXPRKIND.CONSTANT)
                {
                    // error reported by compile already...
                    if (rval.Kind != EXPRKIND.ERROR)
                    {
                        //EDMAURER Give a better error message in the following case:
                        //  const object x = "some_string"
                        //The error here is that an implicit cast cannot be performed. All ref types
                        //except strings must be initialized with null.
                        if (currentFieldSym.TypeSym.IsReferenceType())
                        {
                            Compiler.Error(
                                expressionTreeNode,
                                CSCERRID.ERR_NotNullConstRefField,
                                new ErrArg(currentFieldSym),
                                new ErrArg(currentFieldSym.TypeSym));
                        }
                        else
                        {
                            Compiler.Error(
                                expressionTreeNode,
                                CSCERRID.ERR_NotConstantExpression,
                                new ErrArg(currentFieldSym));
                        }
                    }	// if (rval.Kind != EXPRKIND.ERROR)

                    currentFieldSym.IsConst = false;
                    currentFieldSym.IsUnevaled = false;
                    if (currentFieldSym.TypeSym.IsPredefined())
                    {
                        currentFieldSym.ConstVal.SetObject(
                            Compiler.MainSymbolManager.GetPredefZero(
                            currentFieldSym.TypeSym.GetPredefType()));
                    }
                    else
                    {
                        currentFieldSym.ConstVal.SetObject(
                            Compiler.MainSymbolManager.GetPredefZero(PREDEFTYPE.INT));
                    }
                    return false;
                }	// if (rval.Kind != EXPRKIND.CONSTANT)

                // We have to copy what rval.getVal() actually points to, as well as val itself, because
                // it may be allocated with a short-lived allocator.

                currentFieldSym.ConstVal = new CONSTVAL((rval as EXPRCONSTANT).ConstVal);
            }	// if (expressionTreeNode==null)

            Compiler.FuncBRec.ResetUnsafe();
            currentFieldSym.IsUnevaled = false;

            if (currentFieldSym.FixedAggSym != null)
            {
                currentFieldSym.IsConst = false;
                // Fixed fields really aren't const they just get evaluated like consts
            }

            return true;
        }

        //------------------------------------------------------------
        // CLSDREC.EvaluateConstants
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void EvaluateConstants(AGGSYM aggSym)
        {
            DebugUtil.Assert(aggSym.IsDefined && (aggSym.HasParseTree || aggSym.IsFabricated));

            if (aggSym.IsFabricated)
            {
                // Must be compiler synthesised
                return;
            }

            for (SYM child = aggSym.FirstChildSym; child != null; child = child.NextSym)
            {
                Compiler.SetLocation(child);   //SETLOCATIONSYM(child);

                switch (child.Kind)
                {
                    case SYMKIND.MEMBVARSYM:
                        // Evaluate constants
                        if ((child as MEMBVARSYM).IsUnevaled)
                        {
                            EvaluateFieldConstant(null, child as MEMBVARSYM);
                            Compiler.DiscardLocalState();
                        }
                        DebugUtil.Assert(!(child as MEMBVARSYM).IsUnevaled);
                        goto case SYMKIND.EVENTSYM;

                    case SYMKIND.PROPSYM:
                    case SYMKIND.EVENTSYM:
                        DefaultAttrBind.CompileEarly(Compiler, child);
                        break;

                    case SYMKIND.METHSYM:
                        MethAttrBind.CompileEarly(Compiler, child as METHSYM);
                        break;

                    case SYMKIND.AGGSYM:
                    case SYMKIND.AGGTYPESYM:
                    case SYMKIND.TYVARSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Unknown node type");
                        break;
                }
            }

            if (aggSym.IsClass || aggSym.IsStruct || aggSym.IsInterface)
            {
                EarlyAggAttrBind.Compile(Compiler, aggSym);
            }
            else
            {
                DefaultAttrBind.CompileEarly(Compiler, aggSym);
            }
        }

        //internal // Check a pointer type to make sure it's legal.

        //------------------------------------------------------------
        // CLSDREC.CheckUnmanaged
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="typeSym"></param>
        //------------------------------------------------------------
        internal void CheckUnmanaged(BASENODE treeNode, TYPESYM typeSym)
        {
            if (typeSym == null || !typeSym.IsPTRSYM) return;

            if (Compiler.CompilationPhase < CompilerPhaseEnum.Prepare)
            {
                // It will get checked later.
                return;
            }

            typeSym = (typeSym as PTRSYM).GetMostBaseType();
            DebugUtil.Assert(typeSym != null);
            if (Compiler.FuncBRec.IsManagedType(typeSym))
            {
                Compiler.ErrorRef(treeNode, CSCERRID.ERR_ManagedAddr, new ErrArgRef(typeSym));
            }
        }

        //------------------------------------------------------------
        // CLSDREC.FindNextName
        //
        /// <summary>
        /// <para>Searches for a name in a class and its base classes (though _not_ its interfaces!)</para>
        /// <para>This method can be used to iterate over all base members of a given name,
        /// by updating current with the previous return value.
        /// pTypeToSearchIn will automatically be given the correct next value
        /// (i.e. the containing type for current).</para>
        /// <para>This method never reports any errors</para>
        /// </summary>
        /// <param name="name">
        /// (sscli) name - the name to search for
        /// </param>
        /// <param name="searchInAggTypeSym">
        /// (sscli) pTypeToSearchIn -
        /// points to the typeToSearchIn, and after the call will point to the type
        /// where the member was found, and also, coincidentally,
        /// the information needed to continue the search if you want to search for more members.
        /// Normally evaluates to an AGGSYM but perhaps an instantiated class.
        /// May be updated if the search moves to a new type, i.e. the base class.
        /// </param>
        /// <param name="currentSym">
        /// (sscli) current -
        /// if specified, then the search looks for members in the class
        /// with the same name after current in the symbol table
        /// if specified current.parent  must equal (*pTypeToSearchIn).getAggregate()
        /// </param>
        /// <returns>
        /// Returns NULL if there are no remaining members in a base class with the given name,
        /// in which case pTypeToSearchIn will also be set to NULL.
        /// </returns>
        //------------------------------------------------------------
        internal SYM FindNextName(string name, ref AGGTYPESYM searchInAggTypeSym, SYM currentSym)
        {
            // check for next in same class

            if (currentSym != null)
            {
                DebugUtil.Assert(currentSym.ParentSym == searchInAggTypeSym.GetAggregate());
                currentSym = BSYMMGR.LookupNextSym(currentSym, currentSym.ParentSym, SYMBMASK.ALL);
                if (currentSym != null)
                {
                    return currentSym;
                }

                // didn't find any more in this class
                // start with the base class

                searchInAggTypeSym = searchInAggTypeSym.GetBaseClass();
            }

            // check base class

            AGGTYPESYM tempAts = searchInAggTypeSym;
            SYM hiddenSymbol = null;

            while (tempAts != null)
            {
                AGGTYPESYM typeToSearch = tempAts;

                searchInAggTypeSym = typeToSearch;
                Compiler.EnsureState(searchInAggTypeSym.GetAggregate(), AggStateEnum.DefinedMembers);
                hiddenSymbol = Compiler.MainSymbolManager.LookupAggMember(
                    name,
                    searchInAggTypeSym.GetAggregate(),
                    SYMBMASK.ALL);
                if (hiddenSymbol != null)
                {
                    return hiddenSymbol;
                }

                tempAts = tempAts.GetBaseClass();
            }

            return null;
        }

        //------------------------------------------------------------
        // CLSDREC.FindNextAccessibleName
        //
        /// <summary>
        /// <para>Searches for an accessible name in a class and its base classes</para>
        /// <para>This method can be used to iterate over all accessible base members
        /// of a given name, by updating current with the previous return value
        /// and classToSearchIn with current.parent</para>
        /// <para>This method never reports any errors</para>
        /// </summary>
        /// <param name="name">
        /// (sscli) name - the name to search for
        /// </param>
        /// <param name="searchInAggTypeSym">
        /// (sscli) classToSearchIn - the classToSearchIn
        /// </param>
        /// <param name="context">
        /// (sscli) context - the class we are searching from
        /// </param>
        /// <param name="current">
        /// (sscli) current - if specified, then the search looks for members in the class
        /// with the same name after current in the symbol table
        /// if specified current.parent  must equal classToSearchIn
        /// </param>
        /// <param name="allowAllProtected"></param>
        /// <param name="ignoreSpecialMethods"></param>
        /// <returns>
        /// <para>Returns NULL if there are no remaining members in a base class
        /// with the given name.</para>
        /// </returns>
        //------------------------------------------------------------
        internal SYM FindNextAccessibleName(
            string name,
            ref AGGTYPESYM searchInAggTypeSym,
            PARENTSYM context,
            SYM current,
            bool allowAllProtected,
            bool ignoreSpecialMethods)
        {
            AGGTYPESYM qualifier = allowAllProtected ? null : searchInAggTypeSym;

            //while (
            //    (current = FindNextName(name, ref searchInAggTypeSym, current)) != null &&
            //    (!CheckAccess(current, searchInAggTypeSym, context, qualifier) ||
            //    (ignoreSpecialMethods && !current.IsUserCallable())))
            //{
            //}
            while (true)
            {
                current = FindNextName(name, ref searchInAggTypeSym, current);
                if (current == null)
                {
                    break;
                }
                if (CheckAccess(current, searchInAggTypeSym, context, qualifier) &&
                    (!ignoreSpecialMethods || current.IsUserCallable()))
                {
                    break;
                }
            }
            return current;
        }

        //------------------------------------------------------------
        // CLSDREC.FindAnyAccessSymHiddenByMeth
        //
        //------------------------------------------------------------
        //internal SYM FindAnyAccessSymHiddenByMeth(METHSYM meth, AGGTYPESYM typeToSearchIn, AGGTYPESYM *pWhereDefined = NULL);

        //------------------------------------------------------------
        // CLSDREC.FindExplicitInterfaceImplementation
        //
        /// <summary>
        /// finds an explicit interface implementation on a class or struct
        /// cls is the type to look in
        /// swt is the interface member to look for
        /// </summary>
        /// <param name="aggTypeSym"></param>
        /// <param name="symWithType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM FindExplicitInterfaceImplementation(AGGTYPESYM aggTypeSym, SymWithType symWithType)
        {
            if (symWithType.Sym.IsEVENTSYM)
            {
                for (EVENTSYM evt = Compiler.MainSymbolManager.LookupAggMember(
                    null, aggTypeSym.GetAggregate(), SYMBMASK.EVENTSYM) as EVENTSYM;
                    evt != null;
                    evt = BSYMMGR.LookupNextSym(evt, evt.ClassSym, SYMBMASK.EVENTSYM) as EVENTSYM)
                {
                    if (symWithType.Sym == evt.SlotEventWithType.EventSym &&
                        evt.IsExpImpl &&
                        Compiler.MainSymbolManager.SubstEqualTypes(
                            symWithType.AggTypeSym, evt.SlotEventWithType.AggTypeSym, aggTypeSym, null))
                    {
                        return evt;
                    }
                }
                return null;
            }

            for (METHPROPSYM mps = Compiler.MainSymbolManager.LookupAggMember(
                null, aggTypeSym.GetAggregate(), SYMBMASK.METHSYM | SYMBMASK.PROPSYM) as METHPROPSYM;
                mps != null;
                mps = BSYMMGR.LookupNextSym(mps, mps.ClassSym, SYMBMASK.METHSYM | SYMBMASK.PROPSYM) as METHPROPSYM)
            {
                if (symWithType.Sym == mps.SlotSymWithType.MethPropSym &&
                    mps.IsExplicitImplementation &&
                    Compiler.MainSymbolManager.SubstEqualTypes(
                        symWithType.AggTypeSym, mps.SlotSymWithType.AggTypeSym, aggTypeSym, null))
                {
                    return mps;
                }
            }

            return null;
        }

        //------------------------------------------------------------
        // CLSDREC.FindSymHiddenByMethPropAgg
        //
        /// <summary>
        /// <para>(In sscli, argument ambigSymWithType has the default value null.)</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="startAggTypeSym"></param>
        /// <param name="aggSym"></param>
        /// <param name="symWithType"></param>
        /// <param name="ambigSymWithType"></param>
        /// <param name="needMethodImpl"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FindSymHiddenByMethPropAgg(
            SYM sym,
            AGGTYPESYM startAggTypeSym,
            AGGSYM aggSym,
            SymWithType symWithType,
            SymWithType ambigSymWithType,   // = null
            ref bool needMethodImpl)        // = null
        {
            DebugUtil.Assert(symWithType != null);
            DebugUtil.Assert(sym.IsMETHSYM || sym.IsPROPSYM || sym.IsAGGSYM);
            DebugUtil.Assert(!sym.IsMETHSYM || !(sym as METHSYM).IsCtor);

            SYMKIND symKind = sym.Kind;
            bool isIndexer = (symKind == SYMKIND.PROPSYM) && (sym as PROPSYM).IsIndexer;
            TypeArray paramArray = (isIndexer || symKind == SYMKIND.METHSYM) ? (sym as METHPROPSYM).ParameterTypes : null;

            AGGTYPESYM currentAggTypeSym = startAggTypeSym;
            bool allowDifferent = false;
            AGGTYPESYM lastAggTypeSym = null;
            AGGTYPESYM foundAggTypeSym = null;
            SYM foundSym = null;
            SYM ambigSym = null;
            bool needMethodImpl2 = false;

            int arity;
            switch (symKind)
            {
                case SYMKIND.METHSYM:
                    arity = (sym as METHSYM).TypeVariables.Count;
                    break;

                case SYMKIND.AGGSYM:
                    arity = (sym as AGGSYM).TypeVariables.Count;
                    break;

                default:
                    arity = 0;
                    break;
            }

            // Main Loop
            for (SYM curSym = null;
                (curSym = FindNextAccessibleName(sym.Name, ref currentAggTypeSym, aggSym, curSym, true, false)) != null;)
            {
                if (lastAggTypeSym != null && lastAggTypeSym != currentAggTypeSym)
                {
                    // lastAggTypeSym contained a member that hid everything beneath it and we've past lastAggTypeSym.
                    break;
                }

                if (symKind != curSym.Kind)
                {
                    // Some things (ctors, operators, indexers, accessors) don't interact based on name!
                    if (isIndexer ||
                        curSym.IsPROPSYM && (curSym as PROPSYM).IsIndexer ||
                        curSym.IsMETHSYM &&
                            ((curSym as METHSYM).IsOperator || (curSym as METHSYM).IsCtor || (curSym as METHSYM).IsAnyAccessor) ||
                        sym.IsMETHSYM && (sym as METHSYM).IsAnyAccessor)
                    {
                        continue;
                    }

                    switch (curSym.Kind)
                    {
                        case SYMKIND.METHSYM:
                            if (arity == (curSym as METHSYM).TypeVariables.Count)
                            {
                                break;
                            }
                            continue;

                        case SYMKIND.AGGSYM:
                            if (arity == (curSym as AGGSYM).TypeVariables.Count ||
                                symKind == SYMKIND.METHSYM && (curSym as AGGSYM).TypeVariables.Count == 0)
                            {
                                break;
                            }
                            continue;

                        default:
                            if (arity == 0 || symKind == SYMKIND.METHSYM) break;
                            continue;
                    }

                    // Different kind of member is always hidden. But if fAllowDifferent is true, this member is already
                    // hidden by another method/indexer/ag^g at a lower level than sym.
                    if (allowDifferent)
                    {
                        // Don't look past this type since this guy hides all further down.
                        lastAggTypeSym = currentAggTypeSym;
                        continue;
                    }
                }
                else	// if (symKind != curSym.Kind)
                {
                    switch (symKind)
                    {
                        case SYMKIND.AGGSYM:
                            if ((curSym as AGGSYM).TypeVariables.Count != arity) continue;
                            break;

                        case SYMKIND.METHSYM:
                            if ((curSym as METHSYM).IsCtor || (!(curSym as METHSYM).IsOperator) != (!(sym as METHSYM).IsOperator))
                            {
                                continue;
                            }
                            if ((curSym as METHSYM).TypeVariables.Count != arity)
                            {
                                continue;
                            }
                            if (!Compiler.MainSymbolManager.SubstEqualTypeArrays(
                                    paramArray,
                                    (curSym as METHSYM).ParameterTypes,
                                    currentAggTypeSym.AllTypeArguments,
                                    (sym as METHSYM).TypeVariables,
                                    SubstTypeFlagsEnum.NormNone))
                            {
                                allowDifferent = true;
                                continue;
                            }
                            if (needMethodImpl && (!(curSym as METHSYM).IsAnyAccessor) != (!(sym as METHSYM).IsAnyAccessor))
                            {
                                needMethodImpl2 = true;
                                continue;
                            }
                            break;

                        case SYMKIND.PROPSYM:
                            if (isIndexer && (!(curSym as PROPSYM).IsIndexer ||
                                !Compiler.MainSymbolManager.SubstEqualTypeArrays(
                                    paramArray,
                                    (curSym as PROPSYM).ParameterTypes,
                                    currentAggTypeSym.AllTypeArguments,
                                    null,
                                    SubstTypeFlagsEnum.NormNone)))
                            {
                                continue;
                            }
                            break;

                        default:
                            break;
                    }
                }	// if (symKind != curSym.Kind)

                DebugUtil.Assert(
                    foundSym == null ||
                    foundSym.IsMETHPROPSYM && foundAggTypeSym == lastAggTypeSym &&
                    foundAggTypeSym == currentAggTypeSym);

                if (!curSym.IsMETHPROPSYM)
                {
                    foundSym = curSym;
                    foundAggTypeSym = currentAggTypeSym;
                    ambigSym = null;
                    break;
                }
                DebugUtil.Assert(curSym.IsMETHPROPSYM);

                //EDMAURER If the symbol being examined this iteration has fewer MODOPTS than 
                //the previously found symbol, it is a better match. So replace foundSym.

                if (foundSym == null ||
                    (curSym as METHPROPSYM).CModifierCount < (foundSym as METHPROPSYM).CModifierCount)
                {
                    DebugUtil.Assert(ambigSym == null || foundSym == null);

                    foundSym = curSym;
                    foundAggTypeSym = currentAggTypeSym;
                    lastAggTypeSym = currentAggTypeSym;

                    //EDMAURER If previous best symbol had an ambiguity, it doesn't matter anymore.
                    ambigSym = null;
                }
                else if ((curSym as METHPROPSYM).CModifierCount == (foundSym as METHPROPSYM).CModifierCount && ambigSym == null)
                {
                    ambigSym = curSym;
                }
            }	// Main Loop

            symWithType.Set(foundSym, foundAggTypeSym);
            if (ambigSymWithType != null)
            {
                ambigSymWithType.Set(ambigSym, foundAggTypeSym);
            }

            needMethodImpl = needMethodImpl2;

            return (symWithType.Sym != null);
        }

        //------------------------------------------------------------
        // CLSDREC.GetConditionalSymbols
        //
        /// <summary>
        /// returns the conditional symbols for the method or what it overrides
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal List<string> GetConditionalSymbols(METHSYM method)
        {
            if (!method.CheckedConditionalSymbols && Compiler.CompilationPhase >= CompilerPhaseEnum.Prepare)
            {
                method.CheckedConditionalSymbols = true;
                if (method.IsOverride && method.ConditionalSymbolNameList == null)
                {
                    method.ConditionalSymbolNameList = method.SlotSymWithType.MethSym.ConditionalSymbolNameList;
                }
            }
            return method.ConditionalSymbolNameList;
        }

        //------------------------------------------------------------
        // CLSDREC.GetConditionalSymbols
        //
        /// <summary>
        ///  Returns the conditional symbols for attributes,
        ///  including all those inherited from base classes  
        /// </summary>
        /// <param name="attributeAggSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal List<string> GetConditionalSymbols(AGGSYM attributeAggSym)
        {
            if (!attributeAggSym.IsAttribute)
            {
                DebugUtil.Assert(attributeAggSym.ConditionalSymbolNameList == null);
                return null;
            }

            if (attributeAggSym.CheckedConditionalSymbols)
            {
                return attributeAggSym.ConditionalSymbolNameList;
            }

            Compiler.EnsureState(attributeAggSym.BaseClassSym, AggStateEnum.DefinedMembers);

            //NAMELIST** pConditionalListTail = &attributeAggSym.conditionalSymbols;
            //while(*pConditionalListTail)
            //    pConditionalListTail = &(*pConditionalListTail).next;
            //
            //for (NAMELIST * list = GetConditionalSymbols(attributeAggSym.baseClass.getAggregate()); list; list = list.next) {
            //    compiler().getMainSymbolManager().AddToGlobalNameList(list.name, &pConditionalListTail);
            //}

            if (attributeAggSym.ConditionalSymbolNameList == null)
            {
                attributeAggSym.ConditionalSymbolNameList = new List<string>();
            }

            AGGTYPESYM ats = attributeAggSym.BaseClassSym;
            if (ats != null)
            {
                AGGSYM agg = ats.GetAggregate();
                DebugUtil.Assert(agg != null);
                List<string> list = GetConditionalSymbols(agg);
                if (list != null)
                {
                    attributeAggSym.ConditionalSymbolNameList.AddRange(list);
                }
            }

            attributeAggSym.CheckedConditionalSymbols = true;
            return attributeAggSym.ConditionalSymbolNameList;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineClassTypeVars
        //
        /// <summary>
        /// Create symbols of type variables and set them to aggSym.
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="typeParamsNode"></param>
        //------------------------------------------------------------
        internal void DefineClassTypeVars(
            AGGSYM aggSym,
            AGGDECLSYM aggDeclSym,
            BASENODE typeParamsNode)
        {
            // Count nodes of list starting from typeParamsNode.
            int tvCount = NodeUtil.CountBinOpListNode(typeParamsNode);
            TypeArray outerTypeVars;
            if (aggSym.IsNested)
            {
                DebugUtil.Assert(aggSym.ParentSym != null);
                outerTypeVars = (aggSym.ParentSym as AGGSYM).AllTypeVariables;
            }
            else
            {
                outerTypeVars = BSYMMGR.EmptyTypeArray;
                outerTypeVars.Init();
            }

            //--------------------------------------------------
            // If aggSym has type variables,
            //--------------------------------------------------
            if (tvCount > 0)
            {
                int outerTvCount = outerTypeVars.Count;
                int ivar = 0;
                List<TYPESYM> allTvList = new List<TYPESYM>();

                // Add outer type variables in first.
                if (outerTvCount > 0)
                {
                    allTvList.AddRange(outerTypeVars.List);
                }

                // Create a TYVARSYM instance and add it to tvList.
                BASENODE arg = typeParamsNode;
                while (arg != null)
                {
                    TYPEBASENODE node;
                    if (arg.Kind == NODEKIND.LIST)
                    {
                        node = arg.AsLIST.Operand1 as TYPEBASENODE;
                        arg = arg.AsLIST.Operand2;
                    }
                    else
                    {
                        node = arg as TYPEBASENODE;
                        arg = null;
                    }

                    TYVARSYM tvSym = CreateTypeVar(
                            aggSym,
                            node,
                            ivar,
                            allTvList,
                            outerTvCount,
                            ivar + outerTvCount);
                    allTvList.Add(tvSym);

                    if (node.Kind == NODEKIND.TYPEWITHATTR && tvSym != null)
                    {
                        Compiler.MainSymbolManager.AddToGlobalAttrList(
                            (node as TYPEWITHATTRNODE).AttributesNode,
                            aggDeclSym,
                            tvSym.AttributeList);
                    }
                    ++ivar;
                }
                DebugUtil.Assert(ivar == tvCount);

                if (outerTypeVars.Count > 0)
                {
                    DebugUtil.Assert(allTvList.Count == tvCount + outerTvCount);
                    aggSym.AllTypeVariables = Compiler.MainSymbolManager.AllocParams(allTvList);
                    List<TYPESYM> tvList = allTvList.GetRange(outerTvCount, tvCount);
                    aggSym.TypeVariables = Compiler.MainSymbolManager.AllocParams(tvList);
                }
                else
                {
                    aggSym.TypeVariables = Compiler.MainSymbolManager.AllocParams(allTvList);
                    aggSym.AllTypeVariables = aggSym.TypeVariables;
                }
            }
            //--------------------------------------------------
            // No type valiable
            //--------------------------------------------------
            else
            {
                aggSym.TypeVariables = BSYMMGR.EmptyTypeArray;
                aggSym.AllTypeVariables = outerTypeVars;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.DefineMethodTypeVars
        //
        /// <summary>
        /// Create methodSym.TypeVariables from typeParameters.
        /// </summary>
        /// <param name="methodSym"></param>
        /// <param name="typeParameters"></param>
        //------------------------------------------------------------
        internal void DefineMethodTypeVars(METHSYM methodSym, BASENODE typeParameters)
        {
            // The nodes of type parameters are linked by the NextNode fields.
            // Argument typeParameter is the first typeBaseNode of the linked nodes.
            if (typeParameters != null)
            {
                int index = 0;
                List<TYVARSYM> tyvarList = new List<TYVARSYM>();

                BASENODE node = typeParameters;
                while (node != null)
                {
                    TYPEBASENODE typeBaseNode;
                    if (node.Kind == NODEKIND.LIST)
                    {
                        typeBaseNode = node.AsLIST.Operand1.AsTYPEBASE;
                        node = node.AsLIST.Operand2;
                    }
                    else
                    {
                        typeBaseNode = node.AsTYPEBASE;
                        node = null;
                    }
                    if (typeBaseNode == null) continue;

                    TYVARSYM tvSym = CreateTypeVar(
                        methodSym,
                        typeBaseNode,
                        index,
                        tyvarList.ConvertAll<TYPESYM>(SystemConverter.TyVarSymToTypeSym),
                        0,
                        index);
                    tyvarList.Add(tvSym);
                    if (typeBaseNode.Kind == NODEKIND.TYPEWITHATTR)
                    {
                        Compiler.MainSymbolManager.AddToGlobalAttrList(
                            (typeBaseNode as TYPEWITHATTRNODE).AttributesNode,
                            methodSym,
                            tvSym.AttributeList);
                    }
                    ++index;
                }
                methodSym.TypeVariables = Compiler.MainSymbolManager.AllocParams(
                    tyvarList.ConvertAll<TYPESYM>(SystemConverter.TyVarSymToTypeSym));

            }
            else	// if (typeParameters!=null)
            {
                methodSym.TypeVariables = BSYMMGR.EmptyTypeArray;
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CreateTypeVar
        //
        /// <summary>
        /// Create a TYVARSYM instance representing a type variable.
        /// </summary>
        /// <param name="parentSym">Parent SYM</param>
        /// <param name="treeNode"></param>
        /// <param name="index">Index of type variable in this type variables.</param>
        /// <param name="varList"></param>
        /// <param name="startIndex">Start index of duplication check in this type valiables</param>
        /// <param name="totalIndex">Index of type variable in all type variables.</param>
        //------------------------------------------------------------
        internal TYVARSYM CreateTypeVar(
            PARENTSYM parentSym,
            TYPEBASENODE treeNode,
            int index,
            List<TYPESYM> varList,
            int outerCount,
            int totalIndex)
        {
            DebugUtil.Assert(parentSym.IsAGGSYM || parentSym.IsMETHSYM);
            DebugUtil.Assert(index <= totalIndex);

            if (treeNode.Kind == NODEKIND.TYPEWITHATTR)
            {
                DebugUtil.Assert((treeNode as TYPEWITHATTRNODE).AttributesNode != null);
                treeNode = (treeNode as TYPEWITHATTRNODE).TypeBaseNode;
                DebugUtil.Assert(treeNode != null && treeNode.Kind != NODEKIND.TYPEWITHATTR);
            }

            string ident;
            BASENODE nameNode = treeNode;

            if (nameNode.Kind == NODEKIND.NAMEDTYPE)
            {
                nameNode = (nameNode as NAMEDTYPENODE).NameNode;
            }

            if (nameNode.Kind == NODEKIND.NAME)
            {
                ident = (nameNode as NAMENODE).Name;
                if ((nameNode.Flags & NODEFLAGS.NAME_MISSING) != 0)
                {
                    goto LDblBreak;
                }

                // Check name duplication.
                for (int i = outerCount; i < outerCount + index; ++i)
                {
                    if (varList[i].Name == ident)
                    {
                        Compiler.Error(nameNode, CSCERRID.ERR_DuplicateTypeParameter, new ErrArg(ident));
                        goto LDblBreak;
                    }
                }

                if (ident == parentSym.Name)
                {
                    Compiler.Error(nameNode, CSCERRID.ERR_TypeVariableSameAsParent, new ErrArg(ident));
                    goto LDblBreak;
                }

                for (PARENTSYM symOuter = parentSym.ParentSym; symOuter.IsAGGSYM; symOuter = symOuter.ParentSym)
                {
                    AGGSYM outerAggSym = symOuter as AGGSYM;
                    for (int i = 0; i < outerAggSym.TypeVariables.Count; ++i)
                    {
                        if (ident == outerAggSym.TypeVariables[i].Name)
                        {
                            Compiler.Error(
                                nameNode,
                                CSCERRID.WRN_TypeParameterSameAsOuterTypeParameter,
                                new ErrArg(ident),
                                new ErrArg(symOuter),
                                new ErrArgRefOnly((symOuter as AGGSYM).TypeVariables[i]));
                            goto LDblBreak;
                        }
                    }
                }
            LDblBreak:
                // Continue processing.
                ;
            }
            else	// if (nameNode.Kind == NODEKIND.NAME)
            {
                Compiler.Error(nameNode, CSCERRID.ERR_TypeParamMustBeIdentifier);
                // Manufacture a bogus name.
                ident = String.Format("?T{0}?", totalIndex);
                Compiler.NameManager.AddString(ident);
            }

            TYVARSYM tvSym = Compiler.MainSymbolManager.CreateTyVar(ident, parentSym);
            DebugUtil.Assert(tvSym.IsMethodTypeVariable == parentSym.IsMETHSYM);

            tvSym.Access = ACCESS.PRIVATE;
            tvSym.Index = index;
            tvSym.TotalIndex = totalIndex;
            tvSym.ParseTreeNode = treeNode;
            //tvSym.attributeListTail = &tvSym.attributeList;

            // bounds and ifacesAll get set later.
            DebugUtil.Assert(
                tvSym.BaseClassSym == null &&
                tvSym.BoundArray == null &&
                tvSym.AllInterfaces == null);
            return tvSym;
        }

        //------------------------------------------------------------
        // CLSDREC.DefineBounds
        //
        /// <summary>
        /// Parse the bounds defined in contraints for the type variables of parent.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="parentSym"></param>
        /// <param name="isFirst"></param>
        //------------------------------------------------------------
        internal void DefineBounds(BASENODE treeNode, PARENTSYM parentSym, bool isFirst)
        {
            DebugUtil.Assert(parentSym.IsAGGDECLSYM || parentSym.IsMETHSYM);

            TypeArray typeVarsArray;
            PARENTSYM typeVarsSym;
            PARENTSYM lookupSym;

            switch (parentSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SK in DefineBounds!");
                    return;

                case SYMKIND.AGGDECLSYM:
                    typeVarsSym = (parentSym as AGGDECLSYM).AggSym;
                    typeVarsArray = (typeVarsSym as AGGSYM).TypeVariables;
                    if (typeVarsArray.Count == 0)
                    {
                        return;
                    }
                    lookupSym = (parentSym as AGGDECLSYM).ParentDeclSym;
                    break;

                case SYMKIND.METHSYM:
                    typeVarsArray = (parentSym as METHSYM).TypeVariables;
                    if (typeVarsArray.Count == 0)
                    {
                        return;
                    }
                    typeVarsSym = parentSym;
                    lookupSym = (parentSym as METHSYM).ContainingAggDeclSym;
                    break;
            }

            DebugUtil.Assert(typeVarsArray.Count > 0);
            DebugUtil.Assert(lookupSym != null && typeVarsSym != null);

            // Clear all the bits.
            for (int i = 0; i < typeVarsArray.Count; ++i)
            {
                typeVarsArray.ItemAsTYVARSYM(i).SeenWhere = false;
            }

            //TYPESYM * rgtype[8];
            //TYPESYM ** prgtype = rgtype;
            ///int ctypeMax = lengthof(rgtype);
            List<TYPESYM> typeList = new List<TYPESYM>();

            BASENODE node1 = treeNode;
            while (node1 != null)
            {
                CONSTRAINTNODE constraintNode;
                if (node1.Kind == NODEKIND.LIST)
                {
                    constraintNode = node1.AsLIST.Operand1 as CONSTRAINTNODE;
                    node1 = node1.AsLIST.Operand2;
                }
                else
                {
                    constraintNode = node1 as CONSTRAINTNODE;
                    node1 = null;
                }
                if (constraintNode == null)
                {
                    continue;
                }

                // find type par index
                int varIndex;

                for (varIndex = 0; varIndex < typeVarsArray.Count; ++varIndex)
                {
                    if (typeVarsArray[varIndex].Name == constraintNode.NameNode.Name)
                    {
                        break;
                    }
                }

                if (varIndex >= typeVarsArray.Count)
                {
                    Compiler.Error(
                        constraintNode.NameNode,
                        CSCERRID.ERR_TyVarNotFoundInConstraint,
                        new ErrArg(constraintNode.NameNode),
                        new ErrArg(parentSym));
                    continue;
                }

                TYVARSYM tvSym = typeVarsArray.ItemAsTYVARSYM(varIndex);
                bool fAdd = true;

                AGGTYPESYM boundAggTypeSym = null;
                int ctype = 0;
                bool isInterface = false;
                SpecialConstraintEnum specialConstraint = SpecialConstraintEnum.None;

                if ((constraintNode.Flags & NODEFLAGS.CONSTRAINT_REFTYPE) != 0)
                {
                    specialConstraint |= SpecialConstraintEnum.Reference;
                }
                if ((constraintNode.Flags & NODEFLAGS.CONSTRAINT_VALTYPE) != 0)
                {
                    specialConstraint |= SpecialConstraintEnum.Value;
                }
                if (((constraintNode.Flags & NODEFLAGS.CONSTRAINT_NEWABLE) != 0) &&
                    ((specialConstraint & SpecialConstraintEnum.Value) == 0))
                {
                    specialConstraint |= SpecialConstraintEnum.New;
                }

                if (tvSym.SeenWhere)
                {
                    // We've already seen a where clause for this type variable.
                    Compiler.Error(
                        constraintNode,
                        CSCERRID.ERR_DuplicateConstraintClause,
                        new ErrArg(tvSym));

                    // Keep binding but don't add to the tvSym - for error reporting.
                    fAdd = false;
                }

                // Reserve the first slot for boundAggTypeSym.
                //prgtype[ctype++] = null;
                typeList.Add(null);

                // When the value type constraint is specified, boundAggTypeSym is System.ValueType.
                if ((specialConstraint & SpecialConstraintEnum.Value) != 0)
                {
                    boundAggTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.VALUE, true);
                }

                BASENODE node2 = constraintNode.BoundsNode;
                while (node2 != null)
                {
                    TYPEBASENODE typeBaseNode;
                    if (node2.Kind == NODEKIND.LIST)
                    {
                        typeBaseNode = node2.AsLIST.Operand1.AsTYPEBASE;
                        node2 = node2.AsLIST.Operand2;
                    }
                    else
                    {
                        typeBaseNode = node2.AsTYPEBASE;
                        node2 = null;
                    }
                    if (typeBaseNode == null)
                    {
                        continue;
                    }

                    TYPESYM boundTypeSym = TypeBind.BindTypeWithTypeVars(
                        Compiler,
                        typeBaseNode,
                        lookupSym,
                        parentSym,
                        typeVarsSym,
                        TypeBindFlagsEnum.None);
                    DebugUtil.Assert(boundTypeSym != null);

                    if (boundTypeSym.IsAGGTYPESYM)
                    {
                        AGGSYM boundAggSym = (boundTypeSym as AGGTYPESYM).GetAggregate();

                        DebugUtil.VsVerify(
                            ResolveInheritanceRec(boundAggSym),
                            "ResolveInheritanceRec failed in DefineBounds!");

                        if (Compiler.CheckForStaticClass(
                            typeBaseNode,
                            null,
                            boundTypeSym,
                            CSCERRID.ERR_ConstraintIsStaticClass))
                        {
                            continue;
                        }

                        if (!boundAggSym.IsInterface && (!boundAggSym.IsClass || boundAggSym.IsSealed))
                        {
                            Compiler.Error(
                                typeBaseNode,
                                CSCERRID.ERR_BadBoundType,
                                new ErrArg(boundTypeSym));
                            continue;
                        }

                        if (boundAggSym.IsPredefinedType)
                        {
                            PREDEFTYPE pt = (PREDEFTYPE)boundAggSym.PredefinedTypeID;

                            if (pt == PREDEFTYPE.OBJECT ||
                                pt == PREDEFTYPE.ARRAY ||
                                pt == PREDEFTYPE.VALUE ||
                                pt == PREDEFTYPE.ENUM ||
                                pt == PREDEFTYPE.DELEGATE ||
                                pt == PREDEFTYPE.MULTIDEL)
                            {
                                Compiler.Error(
                                    typeBaseNode,
                                    CSCERRID.ERR_SpecialTypeAsBound,
                                    new ErrArg(boundTypeSym));
                                continue;
                            }
                        }
                    }
                    else if (!boundTypeSym.IsTYVARSYM)
                    {
                        if (!boundTypeSym.IsERRORSYM)
                        {
                            Compiler.Error(
                                typeBaseNode,
                                CSCERRID.ERR_BadBoundType,
                                new ErrArg(boundTypeSym));
                        }
                        continue;
                    }

                    if (!fAdd)
                    {
                        continue;
                    }

                    if (boundTypeSym.IsClassType())
                    {
                        // There can only be one class bound and it should come first.
                        if ((specialConstraint & (SpecialConstraintEnum.Reference | SpecialConstraintEnum.Value)) != 0)
                        {
                            Compiler.Error(
                                typeBaseNode,
                                CSCERRID.ERR_RefValBoundWithClass,
                                new ErrArg(boundTypeSym));

                            if (boundAggTypeSym != null)
                            {
                                continue;
                            }
                        }
                        if (boundAggTypeSym != null || ctype > 1)
                        {
                            Compiler.Error(typeBaseNode,
                                (boundAggTypeSym == boundTypeSym) ?
                                    CSCERRID.ERR_DuplicateBound :
                                    CSCERRID.ERR_ClassBoundNotFirst,
                                new ErrArg(boundTypeSym),
                                new ErrArg(tvSym));

                            if (boundAggTypeSym != null)
                            {
                                continue;
                            }
                        }
                        boundAggTypeSym = boundTypeSym as AGGTYPESYM;
                    }
                    else
                    {
                        DebugUtil.Assert(boundTypeSym.IsInterfaceType() || boundTypeSym.IsTYVARSYM);

                        // Make sure that we don't have a duplicate.
                        for (int itype = 1; itype < typeList.Count; ++itype)
                        {
                            if (typeList[itype] == boundTypeSym)
                            {
                                Compiler.Error(
                                    typeBaseNode,
                                    CSCERRID.ERR_DuplicateBound,
                                    new ErrArg(boundTypeSym),
                                    new ErrArg(tvSym));
                                goto LNextBnd;
                            }
                        }

                        // Add the bound to the array.
                        //if (ctype >= ctypeMax)
                        //{
                        //    // Need more space.
                        //    DebugUtil.Assert(ctype == ctypeMax);
                        //    int ctypeMaxNew = ctypeMax * 2;
                        //    TYPESYM ** prgtypeNew = STACK_ALLOC(TYPESYM *, ctypeMaxNew);
                        //    memcpy(prgtypeNew, prgtype, ctypeMax * sizeof(prgtype[0]));
                        //    prgtype = prgtypeNew;
                        //    ctypeMax = ctypeMaxNew;
                        //}
                        //DebugUtil.Assert(ctype < ctypeMax);

                        //prgtype[ctype++] = boundTypeSym;
                        typeList.Add(boundTypeSym);

                        if (boundTypeSym.IsInterfaceType())
                        {
                            isInterface = true;
                        }
                    }

                LNextBnd: ;
                }

                if (!fAdd)
                {
                    continue;
                }

                //DebugUtil.Assert(ctype < ctypeMax);
                //TYPESYM ** prgtypeUse = prgtype;
                DebugUtil.Assert(typeList.Count > 0 && typeList[0] == null);

                if (boundAggTypeSym != null)
                {
                    typeList[0] = boundAggTypeSym;
                }
                else
                {
                    //ctype--;
                    //prgtypeUse++;
                    typeList.RemoveAt(0);
                    // typeList is not used after this.
                }

                TypeArray boundArray = Compiler.MainSymbolManager.AllocParams(typeList);

#if DEBUG
                // Check that they are all distinct.
                for (int i = 1; i < boundArray.Count; ++i)
                {
                    for (int j = 0; j < i; ++j)
                    {
                        DebugUtil.Assert(boundArray[i] != boundArray[j]);
                    }
                }
#endif

                if (isFirst)
                {
                    DebugUtil.Assert(tvSym.BoundArray == null);
                    tvSym.Constraints = specialConstraint;
                    Compiler.SetBounds(tvSym, boundArray, false);
                }
                else
                {
                    // Verify that nothing has changed.
                    DebugUtil.Assert(parentSym.IsAGGDECLSYM);
                    DebugUtil.Assert(tvSym.FResolved());
                    bool badPartial = false;

                    if (tvSym.Constraints != specialConstraint)
                    {
                        //LBadPartial:
                        //    Compiler.Error(CSCERRID.ERR_PartialWrongConstraints,
                        //        new ErrArgRef(typeVarsSym), new ErrArg(tvSym));
                        badPartial = true;
                    }
                    else if (tvSym.BoundArray != boundArray)
                    {
                        // See if one is a permutation of the other.
                        if (tvSym.BoundArray.Count != boundArray.Count)
                        {
                            //goto LBadPartial;
                            badPartial = true;
                        }
                        else
                        {
                            for (int i = 0; i < boundArray.Count; ++i)
                            {
                                if (!tvSym.BoundArray.Contains(boundArray[i]))
                                {
                                    //goto LBadPartial;
                                    badPartial = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (badPartial)
                    {
                        Compiler.Error(
                            CSCERRID.ERR_PartialWrongConstraints,
                            new ErrArgRef(typeVarsSym),
                            new ErrArg(tvSym));
                    }
                }
                tvSym.SeenWhere = true;
            }

            // For any that we didn't see, set bounds to the default.
            for (int varIndex = 0; varIndex < typeVarsArray.Count; ++varIndex)
            {
                TYVARSYM tvTempSym = typeVarsArray.ItemAsTYVARSYM(varIndex);

                // All of these should have parse trees.
                DebugUtil.Assert(tvTempSym.ParseTreeNode != null);
                if (tvTempSym.SeenWhere)
                {
                    continue;
                }

                if (isFirst)
                {
                    DebugUtil.Assert(tvTempSym.BoundArray == null);
                    DebugUtil.Assert(!tvTempSym.FResolved());
                    Compiler.SetBounds(tvTempSym, BSYMMGR.EmptyTypeArray, false);
                }
                else
                {
                    // Verify that nothing has changed.
                    DebugUtil.Assert(parentSym.IsAGGDECLSYM);
                    DebugUtil.Assert(tvTempSym.FResolved());
                    DebugUtil.Assert(
                        tvTempSym.BoundArray != BSYMMGR.EmptyTypeArray ||
                        tvTempSym.AllInterfaces == BSYMMGR.EmptyTypeArray);

                    if (tvTempSym.BoundArray != BSYMMGR.EmptyTypeArray ||
                        tvTempSym.Constraints != SpecialConstraintEnum.None)
                    {
                        Compiler.Error(
                            CSCERRID.ERR_PartialWrongConstraints,
                            new ErrArgRef(typeVarsSym),
                            new ErrArg(tvTempSym));
                    }
                }
            }

            if (isFirst)
            {
                // Process any type variable bounds. Check for cycles and build all recursive info.
                for (int varIndex = 0; varIndex < typeVarsArray.Count; ++varIndex)
                {
                    DebugUtil.VsVerify(
                        Compiler.ResolveBounds(typeVarsArray.ItemAsTYVARSYM(varIndex), false),
                        "ResolveBounds failed!");
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckBoundsVisibility
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="typeVars"></param>
        //------------------------------------------------------------
        internal void CheckBoundsVisibility(SYM sym, TypeArray typeVars)
        {
            for (int i = 0; i < typeVars.Count; ++i)
            {
                TYVARSYM tvSym = typeVars.ItemAsTYVARSYM(i);
                if (tvSym == null)
                {
                    continue;
                }
                TypeArray bounds = tvSym.BoundArray;

                for (int j = 0; j < bounds.Count; ++j)
                {
                    CheckConstituentVisibility(sym, bounds[j], CSCERRID.ERR_BadVisBound);
                }
            }
        }

        //------------------------------------------------------------
        // CLSDREC.CheckImplicitImplConstraints
        //
        /// <summary>
        /// Make sure the constraints match.
        /// </summary>
        /// <param name="implMethWithType"></param>
        /// <param name="baseMethWithType"></param>
        //------------------------------------------------------------
        internal void CheckImplicitImplConstraints(
            MethWithType implMethWithType,
            MethWithType baseMethWithType)
        {
            DebugUtil.Assert(
                implMethWithType.MethSym.TypeVariables != null &&
                baseMethWithType.MethSym.TypeVariables != null &&
                implMethWithType.MethSym.TypeVariables.Count == baseMethWithType.MethSym.TypeVariables.Count);

            if (implMethWithType.MethSym.TypeVariables.Count == 0)
            {
                return;
            }

            for (int i = 0; i < implMethWithType.MethSym.TypeVariables.Count; ++i)
            {
                TYVARSYM tvSym1 = implMethWithType.MethSym.TypeVariables.ItemAsTYVARSYM(i);
                TYVARSYM tvSym2 = baseMethWithType.MethSym.TypeVariables.ItemAsTYVARSYM(i);

                bool mismatch = false;
                if (tvSym1.Constraints != tvSym2.Constraints)
                {
                    mismatch = true;
                    goto LMismatch;
                }

                TypeArray boundArray1 = tvSym1.BoundArray;
                TypeArray boundArray2 = tvSym2.BoundArray;

                // boundArray1 and boundArray2 must be equal (as sets) after substition.
                // Some members of the bounds may unify during substitution.
                // Hence, it's possible for the bounds to have a different number of items and still match,
                // so the following tempting code would be a bug:
                // "if (boundArray1.size != boundArray2.size) goto LMismatch;"

                // Substitute.
                boundArray1 = Compiler.MainSymbolManager.SubstTypeArray(
                    boundArray1,
                    implMethWithType.AggTypeSym,
                    null);
                boundArray2 = Compiler.MainSymbolManager.SubstTypeArray(
                    boundArray2,
                    baseMethWithType.AggTypeSym,
                    implMethWithType.MethSym.TypeVariables);

                // Check for the easy case.
                if (boundArray1 == boundArray2) continue;

                // Make sure boundArray1 is a subset of boundArray2.
                for (int j = 0; j < boundArray1.Count; ++j)
                {
                    TYPESYM typeSym = boundArray1[j];
                    if (boundArray2.Contains(typeSym) || typeSym.IsPredefType(PREDEFTYPE.OBJECT)) continue;
                    // See if boundArray1.Item(j) is a base type of something in boundArray2.
                    for (int k = 0; ; ++k)
                    {
                        if (k >= boundArray2.Count)
                        {
                            mismatch = true;
                            goto LMismatch;
                        }
                        if (Compiler.IsBaseType(boundArray2[k], typeSym)) break;
                    }
                }

                // Make sure boundArray2 is a subset of boundArray1.
                for (int j = 0; j < boundArray2.Count; ++j)
                {
                    TYPESYM typeSym = boundArray2[j];
                    if (boundArray1.Contains(typeSym) || typeSym.IsPredefType(PREDEFTYPE.OBJECT)) continue;
                    // See if boundArray2.Item(j) is a base type of something in boundArray1.
                    for (int k = 0; ; ++k)
                    {
                        if (k >= boundArray1.Count)
                        {
                            mismatch = true;
                            goto LMismatch;
                        }
                        if (Compiler.IsBaseType(boundArray1[k], typeSym)) break;
                    }
                }

            LMismatch:
                if (mismatch)
                {
                    Compiler.Error(CSCERRID.ERR_ImplBadConstraints,
                        new ErrArgRef(tvSym1),
                        new ErrArgRef(implMethWithType),
                        new ErrArgRef(tvSym2),
                        new ErrArgRef(baseMethWithType));
                    continue;
                }
            } // for (int i = 0; i < implMethWithType.MethSym.TypeVariables.Count; ++i)
        }

        //------------------------------------------------------------
        // CLSDREC.OperatorOfName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal OPERATOR OperatorOfName(string name)
        {
            int i;
            for (i = 0;
                i < (int)OPERATOR.LAST &&
                ((operatorNames[i] == PREDEFNAME.COUNT) ||
                (name != Compiler.NameManager.GetPredefinedName(operatorNames[i])));
                i += 1)
            {
                // nothing
            }
            return (OPERATOR)i;
        }

        //------------------------------------------------------------
        // CLSDREC.isAttributeType
        //
        /// <summary>
        /// <para>returns true if a type is an attribute type</para>
        /// <para>For System.Object and System.Type, this method returns false,
        /// but we may use them for the type of parameters of attributes.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsAttributeType(TYPESYM typeSym)
        {
            Compiler.EnsureState(typeSym, AggStateEnum.Prepared);

            switch (typeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    if (typeSym.GetAggregate().IsEnum)
                    {
                        return true;
                    }
                    if (typeSym.GetAggregate().IsPredefinedType)
                    {
                        switch (typeSym.GetAggregate().PredefinedTypeID)
                        {
                            case PREDEFTYPE.BYTE:
                            case PREDEFTYPE.SHORT:
                            case PREDEFTYPE.INT:
                            case PREDEFTYPE.LONG:
                            case PREDEFTYPE.FLOAT:
                            case PREDEFTYPE.DOUBLE:
                            case PREDEFTYPE.CHAR:
                            case PREDEFTYPE.BOOL:
                            case PREDEFTYPE.OBJECT:
                            case PREDEFTYPE.STRING:
                            case PREDEFTYPE.TYPE:

                            case PREDEFTYPE.SBYTE:
                            case PREDEFTYPE.USHORT:
                            case PREDEFTYPE.UINT:
                            case PREDEFTYPE.ULONG:
                                return true;
                        }
                    }
                    break;

                case SYMKIND.ARRAYSYM:
                    if ((typeSym as ARRAYSYM).Rank == 1 &&
                        IsAttributeType((typeSym as ARRAYSYM).ElementTypeSym) &&
                        !(typeSym as ARRAYSYM).ElementTypeSym.IsARRAYSYM)
                    {
                        return true;
                    }
                    break;

                default:
                    break;
            }
            return false;
        }

        //------------------------------------------------------------
        // CLSDREC.GetLayoutKindValue
        //
        /// <summary>
        /// Get the value for the specific field in the LayoutKind enum.
        /// First time through we look this up, then cache the result for any subsequent queries.
        /// </summary>
        /// <param name="pnLayoutKind"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetLayoutKindValue(PREDEFNAME pnLayoutKind)
        {
            DebugUtil.Assert(
                (pnLayoutKind == PREDEFNAME.EXPLICIT) ||
                (pnLayoutKind == PREDEFNAME.SEQUENTIAL));

            // first check for the cached value
            int val = 0;

            if (pnLayoutKind == PREDEFNAME.EXPLICIT)
            {
                if (this.explicitLayoutValue >= 0)
                {
                    return this.explicitLayoutValue;
                }
                val = 2;
            }
            else if (pnLayoutKind == PREDEFNAME.SEQUENTIAL)
            {
                if (this.sequentialLayoutValue >= 0)
                {
                    return this.sequentialLayoutValue;
                }
                val = 0;
            }

            // Otherwise we need to look it up
            AGGTYPESYM aggTypeSym = Compiler.GetOptPredefType(PREDEFTYPE.LAYOUTKIND, true);
            DebugUtil.Assert(aggTypeSym != null);
            string name = Compiler.NameManager.GetPredefinedName(pnLayoutKind);
            MEMBVARSYM fieldSym = Compiler.MainSymbolManager.LookupAggMember(
                name,
                aggTypeSym.GetAggregate(),
                SYMBMASK.MEMBVARSYM) as MEMBVARSYM;

            if (fieldSym == null || !fieldSym.IsConst)
            {
                Compiler.Error(CSCERRID.ERR_MissingPredefinedMember,
                    new ErrArg(aggTypeSym.Name), new ErrArg(name));
            }
            else
            {
                DebugUtil.Assert(fieldSym.ConstVal.GetInt() >= 0);
                val = fieldSym.ConstVal.GetInt();
            }

            // cache the result
            if (pnLayoutKind == PREDEFNAME.EXPLICIT)
            {
                this.explicitLayoutValue = val;
            }
            else if (pnLayoutKind == PREDEFNAME.SEQUENTIAL)
            {
                this.sequentialLayoutValue = val;
            }
            return val;
        }

        //------------------------------------------------------------
        // CLSDREC.CheckAccess
        //
        /// <summary>
        /// <para>Return whether target can be accessed from within a given place.</para>
        /// <list type="bullet">
        /// <item>
        /// <term>symCheck</term>
        /// <description>The symbol to check access on. Typically this is a member of an AGGSYM.
        /// The only TYPESYM that is legal is a TYVARSYM.</description>
        /// </item>
        /// <item>
        /// <term>atsCheck</term>
        /// <description>The type containing symCheck.
        /// When symCheck is a member of an AGGSYM,
        /// this should be the particular instantiation of the AGGSYM and should NOT be NULL.</description>
        /// </item>
        /// <item>
        /// <term>aggWhere</term>
        /// <description>The location from which symCheck is being referenced.
        /// This may be NULL indicating global access
        /// (eg, inside an attribute on a top level element).</description>
        /// </item>
        /// <item>
        /// <term>typeThru</term>
        /// <description>The type through which symCheck is being accessed.
        /// This is used for checking protected access.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="checkSym">The symbol to check access on.</param>
        /// <param name="checkAggTypeSym">The type containing checkSym.</param>
        /// <param name="fromSym">The location from which checkSym is being referenced.</param>
        /// <param name="throughSym">The type through which checkSym is being accessed.</param>
        /// <remarks>
        /// <para>IMPORTANT: IF YOU CHANGE THIS MAKE SURE THAT YOU KEEP CDirectTypeRef::IsSymbolAccessible UP TO DATE!!</para>
        /// <para>Never reports an error.</para>
        /// </remarks>
        //------------------------------------------------------------
        internal bool CheckAccess(
            SYM checkSym,
            AGGTYPESYM checkAggTypeSym,
            SYM fromSym,
            TYPESYM throughSym)
        {
#if DEBUG
            DebugUtil.Assert(checkSym != null);
            DebugUtil.Assert(!checkSym.IsTYPESYM || checkSym.IsTYVARSYM);
            DebugUtil.Assert(
                checkAggTypeSym == null ||
                checkSym.ParentSym == checkAggTypeSym.GetAggregate());
            DebugUtil.Assert(
                throughSym == null ||
                throughSym.IsAGGTYPESYM ||
                throughSym.IsTYVARSYM ||
                throughSym.IsARRAYSYM ||
                throughSym.IsNUBSYM);

            switch (checkSym.Kind)
            {
                default:
                    break;
                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                case SYMKIND.FAKEMETHSYM:
                case SYMKIND.MEMBVARSYM:
                case SYMKIND.EVENTSYM:
                    DebugUtil.Assert(checkAggTypeSym != null);
                    break;
            }
#endif // DEBUG

            if (!CheckAccessCore(checkSym, checkAggTypeSym, fromSym, throughSym))
            {
                return false;
            }

            // Check the accessibility of the return type.
            TYPESYM type;

            switch (checkSym.Kind)
            {
                default:
                    return true;

                case SYMKIND.METHSYM:
                case SYMKIND.PROPSYM:
                case SYMKIND.FAKEMETHSYM:
                    type = (checkSym as METHPROPSYM).ReturnTypeSym;
                    break;

                case SYMKIND.MEMBVARSYM:
                    type = (checkSym as MEMBVARSYM).TypeSym;
                    break;

                case SYMKIND.EVENTSYM:
                    type = (checkSym as EVENTSYM).TypeSym;
                    break;
            }

            // For members of AGGSYMs, checkAggTypeSym should always be specified!
            DebugUtil.Assert(checkAggTypeSym != null);
            if (checkAggTypeSym != null && checkAggTypeSym.GetAggregate().IsSource)
            {
                return true;
            }

            // Substitute on the type.
            if (checkAggTypeSym != null && checkAggTypeSym.AllTypeArguments.Count > 0)
            {
                type = Compiler.MainSymbolManager.SubstType(type, checkAggTypeSym, null);
            }

            return CheckTypeAccess(type, fromSym);
        }

        //------------------------------------------------------------
        // CLSDREC.CheckTypeAccess
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        /// <param name="fromSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckTypeAccess(TYPESYM typeSym, SYM fromSym)
        {
            DebugUtil.Assert(typeSym != null);

            // Array, Ptr, Nub, etc don't matter.
            typeSym = typeSym.GetNakedType(true);

            if (!typeSym.IsAGGTYPESYM)
            {
                DebugUtil.Assert(typeSym.IsVOIDSYM || typeSym.IsERRORSYM || typeSym.IsTYVARSYM);
                return true;
            }

            for (AGGTYPESYM aggTypeSym = typeSym as AGGTYPESYM;
                aggTypeSym != null;
                aggTypeSym = aggTypeSym.OuterTypeSym)
            {
                if (!CheckAccessCore(
                        aggTypeSym.GetAggregate(),
                        aggTypeSym.OuterTypeSym,
                        fromSym,
                        null))
                {
                    return false;
                }
            }

            TypeArray typeArgs = (typeSym as AGGTYPESYM).AllTypeArguments;
            for (int i = 0; i < typeArgs.Count; i++)
            {
                if (!CheckTypeAccess(typeArgs[i], fromSym)) return false;
            }

            return true;
        }

        //------------------------------------------------------------
        // CLSDREC.CheckAccessCore
        //
        /// <summary></summary>
        /// <param name="checkSym"></param>
        /// <param name="checkAggTypeSym"></param>
        /// <param name="fromSym"></param>
        /// <param name="throughTypeSym"></param>
        /// <returns></returns>
        /// <remarks>
        /// IMPORTANT: IF YOU CHANGE THIS MAKE SURE THAT YOU KEEP CDirectTypeRef::IsSymbolAccessible UP TO DATE!!
        /// </remarks>
        //------------------------------------------------------------
        private bool CheckAccessCore(
            SYM checkSym,
            AGGTYPESYM checkAggTypeSym,
            SYM fromSym,
            TYPESYM throughTypeSym)
        {
            DebugUtil.Assert(checkSym != null);
            DebugUtil.Assert(!checkSym.IsTYPESYM || checkSym.IsTYVARSYM);
            // TYVARSYM derives from TYPESYM.

            DebugUtil.Assert(
                checkAggTypeSym == null ||
                checkSym.ParentSym == checkAggTypeSym.GetAggregate());
            DebugUtil.Assert(
                throughTypeSym == null ||
                throughTypeSym.IsAGGTYPESYM ||
                throughTypeSym.IsTYVARSYM ||
                throughTypeSym.IsARRAYSYM ||
                throughTypeSym.IsNUBSYM);

            switch (checkSym.Access)
            {
                default:
                    DebugUtil.Assert(false, "Bad access level");
                    goto case ACCESS.UNKNOWN;

                case ACCESS.UNKNOWN:
                    return false;

                case ACCESS.PUBLIC:
                    return true;

                case ACCESS.PRIVATE:
                case ACCESS.PROTECTED:
                    if (fromSym == null)
                    {
                        return false;
                    }
                    break;

                case ACCESS.INTERNAL:
                case ACCESS.INTERNALPROTECTED:  // Check internal, then protected.
                    if (fromSym == null) return false;
                    if (fromSym.SameAssemOrFriend(checkSym))
                    {
                        // Log friend use
                        if (checkSym.GetAssemblyID() != Kaid.ThisAssembly && fromSym.GetAssemblyID() == Kaid.ThisAssembly)
                        {
                            Compiler.MarkUsedFriendAssemblyRef(checkSym.GetSomeInputFile());
                        }
                        return true;
                    }
                    if (checkSym.Access != ACCESS.INTERNALPROTECTED) return false;
                    break;
            }

            // Now, checkSym is valid and is not public, fromSym is not null.

            // Should always have checkAggTypeSym for private and protected access check.
            // We currently don't need it since access doesn't respect instantiation.
            // We just use fromSym.parent.AsAGGSYM instead.
            DebugUtil.Assert(checkAggTypeSym != null);
            AGGSYM checkAggSym = checkSym.ParentSym as AGGSYM;

            // Find the inner-most enclosing AGGSYM.
            AGGSYM fromAggSym = null;

            for (SYM tempSym = fromSym; tempSym != null; tempSym = tempSym.ParentSym)
            {
                if (tempSym.IsAGGSYM)
                {
                    fromAggSym = tempSym as AGGSYM;
                    break;
                }
                if (tempSym.IsAGGDECLSYM)
                {
                    fromAggSym = (tempSym as AGGDECLSYM).AggSym;
                    break;
                }
            }

            if (fromAggSym == null) return false;

            //--------------------------------------------------
            // First check for private access.
            //
            // If fromAggSym is nested in checkAggSym, we can accesse checkSym in fromAggSym.
            // Otherwise, if checkSym is private, cannot access.
            //--------------------------------------------------
            for (AGGSYM aggSym = fromAggSym; aggSym != null; aggSym = aggSym.GetOuterAgg())
            {
                if (aggSym == checkAggSym) return true;
            }

            if (checkSym.Access == ACCESS.PRIVATE) return false;

            //--------------------------------------------------
            // From here, fromAggSym is not nested in checkAggSym.
            //
            // Handle the protected case - which is the only real complicated one.
            //--------------------------------------------------
            DebugUtil.Assert(checkSym.Access == ACCESS.PROTECTED || checkSym.Access == ACCESS.INTERNALPROTECTED);

            // Check if checkSym is in fromAggSym or a base of fromAggSym,
            // or in an outer agg of fromAggSym or a base of an outer agg of fromAggSym.

            AGGTYPESYM throughAggTypeSym = null;

            if (throughTypeSym != null)
            {
                bool isStatic = true;

                switch (checkSym.Kind)
                {
                    case SYMKIND.MEMBVARSYM:
                        isStatic = (checkSym as MEMBVARSYM).IsStatic;
                        break;

                    case SYMKIND.EVENTSYM:
                        isStatic = (checkSym as EVENTSYM).IsStatic;
                        break;

                    case SYMKIND.AGGSYM:
                        isStatic = true;
                        break;

                    default:
                        isStatic = (checkSym as METHPROPSYM).IsStatic;
                        break;
                }

                if (!isStatic)
                {
                    // Get the AGGTYPESYM through which the symbol is accessed.
                    switch (throughTypeSym.Kind)
                    {
                        default:
                            DebugUtil.Assert(false, "Bad throughTypeSym!");
                            break;

                        case SYMKIND.AGGTYPESYM:
                            throughAggTypeSym = throughTypeSym as AGGTYPESYM;
                            break;

                        case SYMKIND.ARRAYSYM:
                            throughAggTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.ARRAY, true);
                            break;

                        case SYMKIND.TYVARSYM:
                            throughAggTypeSym = (throughTypeSym as TYVARSYM).BaseClassSym;
                            break;

                        case SYMKIND.NUBSYM:
                            throughAggTypeSym = (throughTypeSym as NUBSYM).GetAggTypeSym();
                            break;
                    }
                }
            }   // if (throughTypeSym != null)

            //--------------------------------------------------
            // Look for checkAggSym among the base classes of fromAggSym and outer aggs.
            //--------------------------------------------------
            for (AGGSYM aggSym = fromAggSym; aggSym != null; aggSym = aggSym.GetOuterAgg())
            {
                DebugUtil.Assert(aggSym != checkAggSym); // We checked for this above.

                // Look for checkAggSym among the base classes of aggSym.
                if (aggSym.FindBaseAgg(checkAggSym))
                {
                    // checkAggSym is a base class of agg. Check throughAggTypeSym.
                    // For non-static protected access to be legal,
                    // throughAggTypeSym must be an instantiation of agg
                    // or a type derived from an instantiation of agg.
                    // In this case all that matters is that
                    // agg is in the base AGGSYM chain of throughAggTypeSym.
                    // The actual AGGTYPESYMs involved don't matter.
                    if (throughAggTypeSym == null ||
                        throughAggTypeSym.GetAggregate().FindBaseAgg(aggSym))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //------------------------------------------------------------
        // CLSDREC.ReportAccessError
        //
        /// <summary></summary>
        /// <param name="tree"></param>
        /// <param name="swtBad"></param>
        /// <param name="symWhere"></param>
        /// <param name="typeQual"></param>
        //------------------------------------------------------------
        internal void ReportAccessError(BASENODE tree, SymWithType swtBad, SYM symWhere, TYPESYM typeQual)
        {
            DebugUtil.Assert(
                !CheckAccess(swtBad.Sym, swtBad.AggTypeSym, symWhere, typeQual) ||
                !CheckTypeAccess(swtBad.AggTypeSym, symWhere));

            if (typeQual != null &&
                (swtBad.Sym.Access == ACCESS.PROTECTED || swtBad.Sym.Access == ACCESS.INTERNALPROTECTED) &&
                CheckAccess(swtBad.Sym, swtBad.AggTypeSym, symWhere, null) &&
                !CheckAccess(swtBad.Sym, swtBad.AggTypeSym, symWhere, typeQual))
            {
                Compiler.Error(
                    tree,
                    CSCERRID.ERR_BadProtectedAccess,
                    new ErrArg(swtBad),
                    new ErrArg(typeQual),
                    new ErrArg(symWhere));
            }
            else
            {
                Compiler.ErrorRef(tree, CSCERRID.ERR_BadAccess, new ErrArgRef(swtBad));
            }
        }

        //------------------------------------------------------------
        // CLSDREC.ReportDeprecated
        //
        /// <summary>
        /// Report a deprecation error on a symbol.
        /// If tree is NULL, refContext is used as a location to report the error.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="refContextSym"></param>
        /// <param name="symWithType"></param>
        //------------------------------------------------------------
        internal void ReportDeprecated(BASENODE treeNode, SYM refContextSym, SymWithType symWithType)
        {
            DebugUtil.Assert(symWithType.Sym.IsDeprecated());

            if (refContextSym.IsContainedInDeprecated())
            {
                return;
            }

            if (symWithType.Sym.IsTYPESYM)
            {
                symWithType.Set((symWithType.Sym as TYPESYM).GetNakedType(true), symWithType.AggTypeSym);
            }
            string depMsg = symWithType.Sym.DeprecatedMessage();

            if (depMsg != null)
            {
                // A message is associated with this deprecated symbol: use that.
                CSCERRID errId = symWithType.Sym.IsDeprecatedError() ?
                    CSCERRID.ERR_DeprecatedSymbolStr : CSCERRID.WRN_DeprecatedSymbolStr;

                if (treeNode == null)
                {
                    Compiler.Error(
                        errId,
                        new ErrArg(symWithType),
                        new ErrArg(depMsg),
                        new ErrArgRefOnly(refContextSym));
                }
                else
                {
                    Compiler.Error(treeNode, errId, new ErrArg(symWithType), new ErrArg(depMsg));
                }
            }
            else
            {
                // No message
                if (treeNode == null)
                {
                    Compiler.Error(CSCERRID.WRN_DeprecatedSymbol, new ErrArg(symWithType), new ErrArgRefOnly(refContextSym));
                }
                else
                {
                    Compiler.Error(treeNode, CSCERRID.WRN_DeprecatedSymbol, new ErrArg(symWithType));
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    // clsdrec.cpp
    //
    ////////////////////////////////////////////////////////////////////////////////

    //void CLSDREC::CompileMemberSkeleton(SYM * sym, AGGINFO * pai)
    //{
    //    ASSERT(compiler().CompPhase() >= CompilerPhase::CompileMembers);
    //
    //    SETLOCATIONSYM(sym);
    //    switch (sym.getKind())
    //    {
    //    case SK_MEMBVARSYM:
    //        {
    //            MEMBVARINFO info;
    //            MEM_SET_ZERO(info);
    //            FieldAttrBind::Compile(compiler(), sym.AsMEMBVARSYM, &info, pai);
    //        }
    //        break;
    //
    //    case SK_METHSYM:
    //        {
    //            METHSYM * meth = sym.AsMETHSYM;
    //            ASSERT(!meth.tokenImport);
    //
    //            METHINFO * info = (METHINFO *)STACK_ALLOC_ZERO(byte, METHINFO::Size(meth.params.size));
    //            FillMethInfoCommon(meth, info, pai, true);
    //
    //            SETLOCATIONSTAGE(COMPILE);
    //            if (!meth.getClass().IsDelegate())
    //                MethAttrBind::CompileAndEmit(compiler(), meth, info.debuggerHidden);
    //            ParamAttrBind::CompileParamList(compiler(), info);
    //
    //        }
    //        break;
    //
    //    case SK_PROPSYM:
    //        PropAttrBind::Compile(compiler(), sym.AsPROPSYM);
    //        break;
    //
    //    case SK_EVENTSYM:
    //        DefaultAttrBind::CompileAndEmit(compiler(), sym);
    //        break;
    //
    //    default:
    //        VSFAIL("Invalid member sym");
    //        break;
    //    }
    //}

    //PARAMINFO * CLSDREC::ReallocParams( int cntNeeded, int * maxAlreadyAlloced, PARAMINFO ** ppParams)
    //{
    //    if (cntNeeded == 0 && *maxAlreadyAlloced == 0 && *ppParams == NULL) {
    //        *ppParams = (PARAMINFO *)compiler().localSymAlloc.AllocZero(0 * sizeof (PARAMINFO));
    //    } else if (cntNeeded > *maxAlreadyAlloced) {
    //        if (*maxAlreadyAlloced == 0)
    //            *maxAlreadyAlloced = 4;
    //
    //        *maxAlreadyAlloced = max(cntNeeded * 2, *maxAlreadyAlloced * 2);
    //        *ppParams = (PARAMINFO *)compiler().localSymAlloc.AllocZero(*maxAlreadyAlloced * sizeof (PARAMINFO));
    //    } else if (cntNeeded > 0) {
    //        // clear out any old junk
    //        memset(*ppParams, 0, sizeof(PARAMINFO) * cntNeeded);
    //    }
    //
    //    return *ppParams;
    //}
}
