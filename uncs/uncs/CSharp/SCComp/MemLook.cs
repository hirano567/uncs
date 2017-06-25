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
// File: memlook.h
//
// Handles member lookup - lookup within a type and its base types.
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
// File: memlook.cpp
//
// Member lookup
// ===========================================================================

//============================================================================
//  MemLook.cs
//
//  2015/02/27 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // enum MemLookFlagsEnum
    //
    /// <summary>
    /// (CSharp\SCComp\MemLook.cs)
    /// </summary>
    //======================================================================
    [Flags]
    internal enum MemLookFlagsEnum : int
    {
        None = 0,

        Ctor = EXPRFLAG.CTOR,
        NewObj = EXPRFLAG.NEWOBJCALL,
        Operator = EXPRFLAG.OPERATOR,
        Indexer = EXPRFLAG.INDEXER,
        UserCallable = EXPRFLAG.USERCALLABLE,
        BaseCall = EXPRFLAG.BASECALL,

        // All EXF flags are < 0x01000000
        TypeVarsAllowed = 0x40000000,

        All = Ctor | NewObj | Operator | Indexer | UserCallable | BaseCall | TypeVarsAllowed,
    }

    //======================================================================
    // class MemberLookup
    //
    /// <summary>
    /// <para>MemberLookup class handles looking for a member within a type and its base types.
    /// This only handles AGGTYPESYMs and TYVARSYMs.</para>
    /// <para>Lookup must be called before any other methods.</para>
    /// </summary>
    //======================================================================
    internal partial class MemberLookup
    {
        //------------------------------------------------------------
        // MemberLookup Fields and Properties (1)
        //
        // The inputs to Lookup.
        //------------------------------------------------------------
        private COMPILER compiler = null;           // * m_compiler;

        internal COMPILER Compiler
        {
            get { return this.compiler; }           // compiler()
        }

        private TYPESYM srcTypeSym = null;          // * m_typeSrc;
        private EXPR expr = null;                   // * m_obj;
        private TYPESYM qualTypeSym = null;         // * m_typeQual;
        private PARENTSYM whereParentSym = null;    // * m_symWhere;
        private string name = null;                 // NAME * m_name;
        private int arity = 0;                      // m_arity;
        private MemLookFlagsEnum flags = 0;         // m_flags;

        //------------------------------------------------------------
        // MemberLookup Fields and Properties (2)
        //
        // For maintaining the type array. We throw the first 8 or so here.
        //------------------------------------------------------------
        private List<AGGTYPESYM> startAggTypeList = new List<AGGTYPESYM>();  // * m_rgtypeStart[8];
        private int maxTypeCount = 0;   // m_ctypeMax;

        //------------------------------------------------------------
        // MemberLookup Fields and Properties (3)
        //
        // Results of the lookup.
        //------------------------------------------------------------
        /// <summary>
        /// <para>All the types containing relevant symbols.</para>
        /// <para>A chain of AGGTYPESYM instances including appropriate syms.</para>
        /// </summary>
        private List<AGGTYPESYM> containingTypeList = null;    // AGGTYPESYM ** m_prgtype;

        /// <summary>
        /// (R) Number of types in which symbols were found.
        /// </summary>
        internal int TypeCount    // m_ctype, TypeCount()
        {
            get { return (containingTypeList != null ? containingTypeList.Count : 0); }
        }

        /// <summary>
        /// Number of syms found.
        /// </summary>
        private int foundSymCount = 0;  // m_csym;

        /// <summary>
        /// (R) The number of symbols found.
        /// </summary>
        internal int FoundSymCount
        {
            get // SymCount()
            {
#if DEBUG
                DebugUtil.Assert(this.debug_isValid);
#endif
                return this.foundSymCount;
            }
        }

        /// <summary>
        /// The first symbol found.
        /// </summary>
        private SymWithType firstSymWithType = null;    // m_swtFirst;

        /// <summary>
        /// (R) The first symbol found.
        /// </summary>
        internal SYM FirstSym
        {
            get // SymFirst()
            {
#if DEBUG
                DebugUtil.Assert(this.debug_isValid);
#endif
                return (this.firstSymWithType != null ? firstSymWithType.Sym : null);
            }
        }

        /// <summary>
        /// (R) The first symbol found.
        /// </summary>
        internal SymWithType FirstSymWithType
        {
            get // SwtFirst()
            {
#if DEBUG
                DebugUtil.Assert(this.debug_isValid);
#endif
                return this.firstSymWithType;
            }
        }

        /// <summary>
        /// Whether symFirst is of a kind for which we collect multiples (methods and indexers).
        /// </summary>
        private bool isMulti = false;   // m_fMulti;

        /// <summary>
        /// (R) Whether the kind of symbol found is method, aggregate, or indexer.
        /// </summary>
        internal bool IsMultiKind
        {
            get // FMultiKind()
            {
#if DEBUG
                DebugUtil.Assert(this.debug_isValid);
#endif
                return this.isMulti;
            }
        }

        //------------------------------------------------------------
        // MemberLookup Fields and Properties (4)
        //
        // These are for error reporting.
        //------------------------------------------------------------

        /// <summary>
        /// An ambiguous symbol.
        /// </summary>
        private SymWithType ambiguousSymWithType = null;   // m_swtAmbig;

        /// <summary>
        /// An inaccessible symbol.
        /// </summary>
        private SymWithType inaccessSymWithType = null;   // m_swtInaccess;

        /// <summary>
        /// If we're looking for a constructor or indexer,
        /// this matched on name, but isn't the right thing.
        /// </summary>
        private SymWithType badSymWithType = null;    // m_swtBad;

        /// <summary>
        /// A bogus member - such as an indexed property.
        /// </summary>
        private SymWithType bogusSymWithType = null;// m_swtBogus;

        /// <summary></summary>
        private SymWithType badAritySymWithType = null; // m_swtBadArity;

        /// <summary>
        /// An ambiguous symbol, but only warn.
        /// </summary>
        private SymWithType ambiguousWarnSymWithType = null;    // m_swtAmbigWarn;

        /// <summary>
        /// (R) Whether we can give an error better than "member not found".
        /// </summary>
        internal bool HasIntelligentErrorInfo
        {
            get // HasIntelligentErrorInfo()
            {
#if DEBUG
                DebugUtil.Assert(this.debug_isValid);
#endif
                return (
                    this.ambiguousSymWithType != null ||
                    this.inaccessSymWithType != null ||
                    this.badSymWithType != null ||
                    this.bogusSymWithType != null ||
                    this.badAritySymWithType != null);
            }
        }

#if DEBUG
        private bool debug_isValid = false; // m_fValid;
#endif

        //------------------------------------------------------------
        // MemberLookup Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal MemberLookup() { }

        //------------------------------------------------------------
        // MemberLookup Constructor (2)
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        //internal MemberLookup(COMPILER comp)
        //{
        //    this.compiler = comp;
        //}

        //------------------------------------------------------------
        // MemberLookup.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            this.srcTypeSym = null;
            this.expr = null;
            this.qualTypeSym = null;
            this.whereParentSym = null;
            this.name = null;
            this.arity = 0;
            this.flags = 0;

            this.startAggTypeList = new List<AGGTYPESYM>();
            this.maxTypeCount = 0;

            this.containingTypeList = null;
            this.foundSymCount = 0;

            this.firstSymWithType = null;
            this.isMulti = false;

            this.ambiguousSymWithType = null;
            this.inaccessSymWithType = null;
            this.badSymWithType = null;
            this.bogusSymWithType = null;
            this.badAritySymWithType = null;
            this.ambiguousWarnSymWithType = null;
        }

        //private:

        //------------------------------------------------------------
        // MemberLookup.RecordType
        //
        /// <summary>
        /// <para>Another match was found.
        /// Increment the count of syms and add the type to our list
        /// if it's not already there.</para>
        /// <para>If argument sym is new, add this to MemberLookup.aggTypeList,
        /// and if MemberLookup.firstSymWithType is null, set sym to it.</para>
        /// </summary>
        /// <param name="aggTypeSym"></param>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        private void RecordType(AGGTYPESYM aggTypeSym, SYM sym)
        {
            DebugUtil.Assert(aggTypeSym != null && sym != null);
            DebugUtil.Assert(
                firstSymWithType == null ||
                firstSymWithType.Sym.Kind == sym.Kind);

            // This type shouldn't already be there.
            if (this.TypeCount == 0 ||
                this.containingTypeList[this.TypeCount - 1] != aggTypeSym)
            {
                // Add the type.
                if (this.containingTypeList == null)
                {
                    this.containingTypeList = new List<AGGTYPESYM>();
                }
                this.containingTypeList.Add(aggTypeSym);
            }

            // Now record the sym....

            this.foundSymCount++;

            // If it is first, record it.
            if (this.firstSymWithType == null)
            {
                this.firstSymWithType
                    = SymWithType.CreateCorrespondingInstance(sym, aggTypeSym);

                DebugUtil.Assert(this.foundSymCount == 1);
                DebugUtil.Assert(this.foundSymCount == 1 && containingTypeList[0] == aggTypeSym);

                this.isMulti = sym.IsMETHSYM || sym.IsPROPSYM && (sym as PROPSYM).IsIndexer;
            }
        }

        //------------------------------------------------------------
        // MemberLookup.SearchSingleType
        //
        /// <summary>
        /// <para>Search just the given type (not any bases).
        /// Returns true iff it finds something
        /// (which will have been recorded by RecordType).</para>
        /// <para>*pfHideByName is set to true
        /// iff something was found that hides all members of base types
        /// (eg, a hidebyname method).</para>
        /// <para>Search a valid SYM instance in the specifiled AGGTYPESYM,
        /// whose name is this.name, whose arity is equal to this.arity
        /// and which is accessible from this.whereParentSym.</para>
        /// <para>If found, the SYM and AGGTYPESYM is set to
        /// this.firstSymWithType and this.aggTypeList.</para>
        /// </summary>
        /// <param name="currentAggTypeSym"></param>
        /// <param name="hideByName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool SearchSingleType(AGGTYPESYM currentAggTypeSym, out bool hideByName)
        {
            hideByName = false;
            bool foundSome = false;

            // Make sure this type is accessible.
            // It may not be due to private inheritance or friend assemblies.
            bool isInaccess = !Compiler.ClsDeclRec.CheckTypeAccess(
                currentAggTypeSym,
                this.whereParentSym);
            if (isInaccess && (this.foundSymCount > 0 || inaccessSymWithType != null))
            {
                return false;
            }

            //--------------------------------------------------------
            // Loop through symbols.
            //--------------------------------------------------------
            SYM currentSym = null;

            for (currentSym = Compiler.MainSymbolManager.LookupAggMember(
                    this.name,
                    currentAggTypeSym.GetAggregate(),
                    SYMBMASK.ALL);
                currentSym != null;
                currentSym = BSYMMGR.LookupNextSym(
                    currentSym,
                    currentAggTypeSym.GetAggregate(),
                    SYMBMASK.ALL))
            {
                //----------------------------------------------------
                // Loop (1) Check for arity.
                //----------------------------------------------------
                bool badArity = false;

                switch (currentSym.Kind)
                {
                    case SYMKIND.METHSYM:
                        // For non-zero arity,
                        // only methods of the correct arity are considered.
                        // For zero arity,
                        // don't filter out any methods since we do type argument inferencing.
                        if (this.arity > 0 &&
                            (currentSym as METHSYM).TypeVariables.Count != this.arity)
                        {
                            //goto LBadArity;
                            badArity = true;
                            goto default;
                        }
                        break;

                    case SYMKIND.AGGSYM:
                        // For types, always filter on arity.
                        if ((currentSym as AGGSYM).TypeVariables.Count != this.arity)
                        {
                            //goto LBadArity;
                            badArity = true;
                            goto default;
                        }
                        break;

                    case SYMKIND.TYVARSYM:
                        if ((this.flags & MemLookFlagsEnum.TypeVarsAllowed) == 0)
                        {
                            continue;
                        }
                        // Fall through
                        goto default;

                    default:
                        // All others are only considered when arity is zero.
                        if (this.arity > 0 || badArity) // badArity is in lieu of the label.
                        {
                            //LBadArity:
                            if (this.badAritySymWithType == null)
                            {
                                this.badAritySymWithType = new SymWithType();
                                this.badAritySymWithType.Set(currentSym, currentAggTypeSym);
                            }
                            continue;
                        }
                        break;
                } // switch (currentSym.Kind)

                //----------------------------------------------------
                // Loop (2) Check for user callability.
                //----------------------------------------------------
                if (currentSym.IsOverride() && !currentSym.IsHideByName())
                {
                    continue;
                }

                if ((this.flags & MemLookFlagsEnum.UserCallable) != 0 &&
                    currentSym.IsMETHPROPSYM &&
                    !(currentSym as METHPROPSYM).IsUserCallable())
                {
                    if (this.inaccessSymWithType == null)
                    {
                        this.inaccessSymWithType = new SymWithType();
                        this.inaccessSymWithType.Set(currentSym, currentAggTypeSym);
                    }
                    continue;
                }

                if (isInaccess ||
                    !Compiler.ClsDeclRec.CheckAccess(
                        currentSym,
                        currentAggTypeSym,
                        this.whereParentSym,
                        this.qualTypeSym))
                {
                    // Not accessible so get the next sym.
                    if (this.inaccessSymWithType == null)
                    {
                        this.inaccessSymWithType = new SymWithType();
                        this.inaccessSymWithType.Set(currentSym, currentAggTypeSym);
                    }
                    if (isInaccess)
                    {
                        return false;
                    }
                    continue;
                }

                //----------------------------------------------------
                // Loop (3) 
                // Make sure that whether we're seeing a ctor, operator, or indexer
                // is consistent with the flags.
                //----------------------------------------------------
                if (((this.flags & MemLookFlagsEnum.Ctor) == 0) !=
                        (!currentSym.IsMETHSYM || !(currentSym as METHSYM).IsCtor) ||
                    ((this.flags & MemLookFlagsEnum.Operator) == 0) !=
                        (!currentSym.IsMETHSYM || !(currentSym as METHSYM).IsOperator) ||
                    ((this.flags & MemLookFlagsEnum.Indexer) == 0) !=
                        (!currentSym.IsPROPSYM || !(currentSym as PROPSYM).IsIndexer))
                {
                    if (this.badSymWithType == null)
                    {
                        this.badSymWithType = new SymWithType();
                        this.badSymWithType.Set(currentSym, currentAggTypeSym);
                    }
                    continue;
                }

                if (!currentSym.IsMETHSYM &&
                    (this.flags & MemLookFlagsEnum.Indexer) == 0 &&
                    Compiler.CheckBogus(currentSym))
                {
                    // A bogus member - we can't use these,
                    // so only record them for error reporting.
                    if (this.bogusSymWithType == null)
                    {
                        this.bogusSymWithType = new SymWithType();
                        this.bogusSymWithType.Set(currentSym, currentAggTypeSym);
                    }
                    continue;
                }

                // We have a visible symbol.

                foundSome = true;

                //----------------------------------------------------
                // Loop (4) If we have already found another SYM
                //----------------------------------------------------
                if (this.firstSymWithType != null)
                {
                    DebugUtil.Assert(this.TypeCount > 0);

                    if (!currentAggTypeSym.IsInterfaceType())
                    {
                        // Non-interface case.
                        DebugUtil.Assert(
                            this.isMulti ||
                            currentAggTypeSym == this.containingTypeList[0]);

                        if (!this.isMulti)
                        {
                            if (this.firstSymWithType.Sym.IsMEMBVARSYM &&
                                currentSym.IsEVENTSYM &&
                                // This is not a problem for the compiler
                                // because the field is only accessible
                                // in the scope in whcih it is declared,
                                // but in the EE we ignore accessibility...
                                this.firstSymWithType.FieldSym.IsEvent)
                            {
                                // m_swtFirst is just the field behind the event currentSym
                                // so ignore currentSym.
                                continue;
                            }
                            goto LAmbig;
                        }
                        if (this.firstSymWithType.Sym.Kind != currentSym.Kind)
                        {
                            if (currentAggTypeSym == this.containingTypeList[0])
                            {
                                goto LAmbig;
                            }
                            // This one is hidden by the first one.
                            // This one also hides any more in base types.
                            hideByName = true;
                            continue;
                        }
                    }
                    else if (!this.isMulti) // if (!currentAggTypeSym.IsInterfaceType())
                    {
                        // In sscli, the condition below is also specified.
                        //     !compiler().options.fLookupHack ||
                        if (!currentSym.IsMETHSYM)
                        {
                            goto LAmbig;
                        }
                        this.ambiguousWarnSymWithType = this.firstSymWithType;
                        // Erase previous results so we'll record this method as the first.
                        if (this.containingTypeList != null)
                        {
                            this.containingTypeList.Clear();
                        }
                        this.foundSymCount = 0;
                        this.firstSymWithType.Clear();
                        this.ambiguousSymWithType.Clear();
                    }
                    else if (this.firstSymWithType.Sym.Kind != currentSym.Kind)
                        // if (!currentAggTypeSym.IsInterfaceType())
                    {
                        if (!currentAggTypeSym.DiffHidden)
                        {
                            // In sscli, the condition below is comment outed.
                            // !compiler().options.fLookupHack || !m_swtFirst.Sym().isMETHSYM()
                            if (!this.firstSymWithType.Sym.IsMETHSYM)
                            {
                                goto LAmbig;
                            }
                            if (this.ambiguousWarnSymWithType == null)
                            {
                                this.ambiguousWarnSymWithType = new SymWithType();
                                this.ambiguousWarnSymWithType.Set(
                                    currentSym,
                                    currentAggTypeSym);
                            }
                        }
                        // This one is hidden by another.
                        // This one also hides any more in base types.
                        hideByName = true;
                        continue;
                    }   // if (!currentAggTypeSym.IsInterfaceType())
                }   // if (this.firstSymWithType != null)

                //----------------------------------------------------
                // Loop (5) Recode currentSym instance and currentAggTypeSym
                //----------------------------------------------------
                RecordType(currentAggTypeSym, currentSym);

                if (currentSym.IsMETHPROPSYM && (currentSym as METHPROPSYM).IsHideByName())
                {
                    hideByName = true;
                }
                // We've found a symbol in this type
                // but need to make sure there aren't any conflicting
                // syms here, so keep searching the type.

            } // for (currentSym = Compiler.MainSymbolManager.LookupAggMember(

            DebugUtil.Assert(!isInaccess || !foundSome);

            return foundSome;

        LAmbig:
            // Ambiguous!
            if (this.ambiguousSymWithType == null)
            {
                this.ambiguousSymWithType = new SymWithType();
                this.ambiguousSymWithType.Set(currentSym, currentAggTypeSym);
            }
            hideByName = true;
            return true;
        }

        //------------------------------------------------------------
        // MemberLookup.LookupInClass
        //
        /// <summary>
        /// <para>Lookup in a class and its bases (until *ptypeEnd is hit).</para>
        /// <para>If found, return false.
        /// If this method returns true, it means that we must go on searching.</para>
        /// </summary>
        /// <param name="startAggTypeSym"></param>
        /// <param name="endAggTypeSym">(sscli)
        /// ptypeEnd [in/out] - *ptypeEnd should be either NULL or object.
        /// If we find something here that would hide members of object,
        /// this sets *ptypeEnd to NULL.</param>
        /// <returns>(sscli)
        /// Returns true when searching should continue to the interfaces.
        /// </returns>
        //------------------------------------------------------------
        private bool LookupInClass(AGGTYPESYM startAggTypeSym, ref AGGTYPESYM endAggTypeSym)
        {
            DebugUtil.Assert(this.firstSymWithType == null || this.isMulti);
            DebugUtil.Assert(
                startAggTypeSym != null &&
                !startAggTypeSym.IsInterfaceType() &&
                (endAggTypeSym == null || startAggTypeSym != endAggTypeSym));

            AGGTYPESYM endSym = endAggTypeSym;
            AGGTYPESYM currentSym;

            //--------------------------------------------------
            // Loop through types. Loop until we hit endSym (object or NULL).
            //--------------------------------------------------
            for (currentSym = startAggTypeSym;
                currentSym != endSym && currentSym != null;
                currentSym = currentSym.GetBaseClass())
            {
                DebugUtil.Assert(!currentSym.IsInterfaceType());

                Compiler.EnsureState(currentSym, AggStateEnum.Prepared);

                bool hideByName = false;

                bool foundSome = SearchSingleType(currentSym, out hideByName);
                this.flags &= ~MemLookFlagsEnum.TypeVarsAllowed;

                if (this.firstSymWithType != null &&
                    this.firstSymWithType.IsNotNull &&
                    !this.isMulti)
                {
                    // Everything below this type and in interfaces is hidden.
                    return false;
                }

                if (hideByName)
                {
                    // This hides everything below it and in object, but not in the interfaces!
                    if (endAggTypeSym != null)
                    {
                        endAggTypeSym = null;
                    }
                    // Return true to indicate that it's ok to search additional types.
                    return true;
                }

                if ((this.flags & MemLookFlagsEnum.Ctor) != 0)
                {
                    // If we're looking for a constructor, don't check base classes or interfaces.
                    return false;
                }
            }

            DebugUtil.Assert(currentSym == endSym);
            return true;
        }

        //------------------------------------------------------------
        // MemberLookup.LookupInInterfaces
        //
        /// <summary>
        /// Returns true if searching should continue to object.
        /// </summary>
        /// <param name="startAggTypeSym"></param>
        /// <param name="typeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool LookupInInterfaces(
            AGGTYPESYM startAggTypeSym,
            TypeArray typeArray)
        {
            DebugUtil.Assert(this.firstSymWithType == null || this.isMulti);
            DebugUtil.Assert(startAggTypeSym == null || startAggTypeSym.IsInterfaceType());
            DebugUtil.Assert(startAggTypeSym != null || typeArray.Count > 0);
            DebugUtil.Assert((this.flags &
                (MemLookFlagsEnum.Ctor | MemLookFlagsEnum.Operator | MemLookFlagsEnum.BaseCall))
                == 0);

            // Clear all the hidden flags. Anything found in a class hides any other
            // kind of member in all the interfaces.
            if (startAggTypeSym != null)
            {
                startAggTypeSym.AllHidden = false;
                startAggTypeSym.DiffHidden = (this.firstSymWithType != null);
            }

            for (int i = 0; i < typeArray.Count; ++i)
            {
                AGGTYPESYM type = typeArray[i] as AGGTYPESYM;
                DebugUtil.Assert(type.IsInterfaceType());

                type.AllHidden = false;
                type.DiffHidden = (this.firstSymWithType != null);
            }

            if (startAggTypeSym != null)
            {
                Compiler.EnsureState(startAggTypeSym, AggStateEnum.Prepared);
            }
            if (typeArray != null)
            {
                Compiler.EnsureState(typeArray, AggStateEnum.Prepared);
            }

            bool hideObject = false;
            AGGTYPESYM currentSym = startAggTypeSym;
            int index = 0;

            if (currentSym == null)
            {
                currentSym = typeArray[index++] as AGGTYPESYM;
            }
            DebugUtil.Assert(currentSym != null);

            // Loop through the interfaces.
            for (; ; )
            {
                DebugUtil.Assert(currentSym != null && currentSym.IsInterfaceType());
                bool hideByName = false;

                if (!currentSym.AllHidden && SearchSingleType(currentSym, out hideByName))
                {
                    hideByName |= !this.isMulti;

                    // Mark base interfaces appropriately.
                    TypeArray interfaceArray = currentSym.GetIfacesAll();
                    for (int i = 0; i < interfaceArray.Count; ++i)
                    {
                        AGGTYPESYM sym = interfaceArray[i] as AGGTYPESYM;
                        DebugUtil.Assert(sym.IsInterfaceType());

                        if (hideByName)
                        {
                            sym.AllHidden = true;
                        }
                        sym.DiffHidden = true;
                    }

                    // If we hide all base types, that includes object!
                    if (hideByName)
                    {
                        hideObject = true;
                    }
                }
                this.flags &= ~MemLookFlagsEnum.TypeVarsAllowed;

                if (index >= typeArray.Count)
                {
                    return !hideObject;
                }

                // Substitution has already been done.
                currentSym = typeArray[index++] as AGGTYPESYM;
            }
        }

        //------------------------------------------------------------
        // MemberLookup.ReportBogus
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="symWithType"></param>
        //------------------------------------------------------------
        private void ReportBogus(BASENODE treeNode, SymWithType symWithType)
        {
            DebugUtil.Assert(symWithType.Sym.HasBogus && symWithType.Sym.CheckBogus());

            METHSYM methSym1;
            METHSYM methSym2;

            switch (symWithType.Sym.Kind)
            {
                case SYMKIND.EVENTSYM:
                    if (symWithType.EventSym.UseMethodInstead)
                    {
                        methSym1 = symWithType.EventSym.AddMethodSym;
                        methSym2 = symWithType.EventSym.RemoveMethodSym;
                        goto LUseAccessors;
                    }
                    break;

                case SYMKIND.PROPSYM:
                    if (symWithType.PropSym.UseMethodInstead)
                    {
                        methSym1 = symWithType.PropSym.GetMethodSym;
                        methSym2 = symWithType.PropSym.SetMethodSym;
                        goto LUseAccessors;
                    }
                    break;

                case SYMKIND.METHSYM:
                    if (symWithType.MethSym.Name == compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE) &&
                        symWithType.MethSym.ClassSym.IsDelegate)
                    {
                        symWithType.Set(
                            symWithType.AggTypeSym != null ?
                                (symWithType.AggTypeSym) as SYM :
                                symWithType.MethSym.ClassSym,
                            null);
                    }
                    break;

                default:
                    if (symWithType.Sym.IsTYPESYM)
                    {
                        compiler.Error(treeNode, CSCERRID.ERR_BogusType, new ErrArg(symWithType));
                        return;
                    }
                    break;
            }

            // Generic bogus error.
            compiler.ErrorRef(treeNode, CSCERRID.ERR_BindToBogus, new ErrArgRef(symWithType));
            return;

        LUseAccessors:
            if (methSym1 != null && methSym2 != null)
            {
                compiler.Error(
                    treeNode,
                    CSCERRID.ERR_BindToBogusProp2,
                    new ErrArg(symWithType.Sym.Name),
                    new ErrArg(new SymWithType(methSym1, symWithType.AggTypeSym)),
                    new ErrArg(new SymWithType(methSym2, symWithType.AggTypeSym)),
                    new ErrArgRefOnly(symWithType.Sym));
                return;
            }
            if (methSym1 != null || methSym2 != null)
            {
                compiler.Error(
                    treeNode,
                    CSCERRID.ERR_BindToBogusProp1,
                    new ErrArg(symWithType.Sym.Name),
                    new ErrArg(new SymWithType(
                        (methSym1 != null ? methSym1 : methSym2), symWithType.AggTypeSym)),
                    new ErrArgRefOnly(symWithType.Sym));
                return;
            }
            DebugUtil.Assert(false, "useMethInstead is set, but there are no accessors to use?");
            return;
        }

        //public:

        //------------------------------------------------------------
        // MemberLookup.Lookup
        //
        /// <summary>
        /// <para>Returns false iff there was no symbol found or an ambiguity.</para>
        /// <para>Lookup must be called before anything else can be called.</para>
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="srcTypeSym">
        /// <para>(sscil) typeSrc - Must be an AGGTYPESYM or TYVARSYM.</para>
        /// </param>
        /// <param name="expr">
        /// <para>(sscli) obj - the expression through which the member is being accessed.
        /// This is used for accessibility of protected members and for constructing a MEMGRP
        /// from the results of the lookup. It is legal for obj to be an EK_CLASS,
        /// in which case it may be used for accessibility, but will not be used for MEMGRP construction.</para>
        /// </param>
        /// <param name="whereSym">
        /// <para>(sscli) symWhere - the symbol from with the name is being accessed
        /// (for checking accessibility).</para>
        /// </param>
        /// <param name="name">
        /// <para>(sscli) name - the name to look for.</para>
        /// </param>
        /// <param name="arity">
        /// <para>(sscli) arity - the number of type args specified.
        /// Only members that support this arity are found.
        /// Note that when arity is zero, all methods are considered since we do type argument inferencing.</para>
        /// </param>
        /// <param name="flags">
        /// <para>(sscli) flags - See MemLookFlags.
        /// TypeVarsAllowed only applies to the most derived type (not base types).</para>
        /// </param>
        //------------------------------------------------------------
        internal bool Lookup(
            COMPILER compiler,
            TYPESYM srcTypeSym,
            EXPR expr,
            PARENTSYM whereSym,
            string name,
            int arity,
            MemLookFlagsEnum flags)
        {
            DebugUtil.Assert((flags & ~MemLookFlagsEnum.All) == 0);
            DebugUtil.Assert(expr == null || expr.TypeSym != null);
            DebugUtil.Assert(srcTypeSym.IsAGGTYPESYM || srcTypeSym.IsTYVARSYM);

            //--------------------------------------------------
            // Clear out the results.
            //--------------------------------------------------
            //memset(this, 0, sizeof(*this));
            this.Clear();

#if DEBUG
            debug_isValid = true;
#endif

            this.containingTypeList = this.startAggTypeList;
            this.maxTypeCount = this.startAggTypeList.Count;

            //--------------------------------------------------
            // Save the inputs for error handling, etc.
            //--------------------------------------------------
            this.compiler = compiler;
            this.srcTypeSym = srcTypeSym;
            this.expr = (expr != null && expr.Kind != EXPRKIND.CLASS) ? expr : null;
            this.whereParentSym = whereSym;
            this.name = name;
            this.arity = arity;
            this.flags = flags;

            if ((this.flags & MemLookFlagsEnum.BaseCall) != 0)
            {
                this.qualTypeSym = null;
            }
            else if ((this.flags & MemLookFlagsEnum.Ctor) != 0)
            {
                this.qualTypeSym = srcTypeSym;
            }
            else if (expr != null)
            {
                // (CS3)
                switch (expr.TypeSym.Kind)
                {
                    default:
                        this.qualTypeSym = expr.TypeSym;
                        break;

                    case SYMKIND.LAMBDAEXPRSYM:
                        EXPRLAMBDAEXPR lambdaExpr = expr as EXPRLAMBDAEXPR;
                        if (lambdaExpr != null)
                        {
                            this.qualTypeSym = lambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
                        }
                        break;

                    case SYMKIND.ANONMETHSYM:
                        EXPRANONMETH anonMethExpr = expr as EXPRANONMETH;
                        if (anonMethExpr != null)
                        {
                            this.qualTypeSym = anonMethExpr.AnonymousMethodInfo.ReturnTypeSym;
                        }
                        break;
                }
            }
            else
            {
                this.qualTypeSym = null;
            }

            //--------------------------------------------------
            // Determine what to search.
            //--------------------------------------------------
            AGGTYPESYM classTypeSym1 = null;
            AGGTYPESYM interfaceTypeSym = null;
            TypeArray interfaceArray = BSYMMGR.EmptyTypeArray;
            AGGTYPESYM classTypeSym2 = null;

            // We need to do EnsureState before fetching the interfaceArray and cls bound
            // because these may change as a result (in the EE anyway).

            if (srcTypeSym.IsTYVARSYM)
            {
                // This may be a little paranoid, but may be necessary for the EE....
                DebugUtil.Assert(srcTypeSym.ParentSym.IsAGGSYM || srcTypeSym.ParentSym.IsMETHSYM);

                AGGSYM parentAggSym = srcTypeSym.ParentSym.IsAGGSYM ?
                    srcTypeSym.ParentSym as AGGSYM :
                    (srcTypeSym.ParentSym as METHSYM).ClassSym;
                compiler.EnsureState(parentAggSym, AggStateEnum.Prepared);
            }

            compiler.EnsureState(srcTypeSym, AggStateEnum.Prepared);

            if (srcTypeSym.IsTYVARSYM)
            {
                DebugUtil.Assert((this.flags & (
                    MemLookFlagsEnum.Ctor |
                    MemLookFlagsEnum.NewObj |
                    MemLookFlagsEnum.Operator |
                    MemLookFlagsEnum.BaseCall |
                    MemLookFlagsEnum.TypeVarsAllowed)) == 0);

                this.flags &= ~MemLookFlagsEnum.TypeVarsAllowed;
                TYVARSYM tvSym = srcTypeSym as TYVARSYM;
                interfaceArray = tvSym.AllInterfaces;
                classTypeSym1 = tvSym.BaseClassSym;
                if (interfaceArray.Count > 0 && classTypeSym1.IsPredefType(PREDEFTYPE.OBJECT))
                {
                    classTypeSym1 = null;
                }
            }
            else if (!srcTypeSym.IsInterfaceType())
            {
                classTypeSym1 = (srcTypeSym as AGGTYPESYM);
            }
            else
            {
                DebugUtil.Assert(srcTypeSym.IsInterfaceType());
                DebugUtil.Assert((this.flags & (
                    MemLookFlagsEnum.Ctor |
                    MemLookFlagsEnum.NewObj |
                    MemLookFlagsEnum.Operator |
                    MemLookFlagsEnum.BaseCall)) == 0);

                interfaceTypeSym = srcTypeSym as AGGTYPESYM;
                interfaceArray = interfaceTypeSym.GetIfacesAll();
            }

            if (interfaceTypeSym != null || interfaceArray.Count > 0)
            {
                classTypeSym2 = compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true);
            }

            //--------------------------------------------------
            // Search the class first (except possibly object).
            // If LookupInClass returns true, we should continue searching.
            //--------------------------------------------------
            if (classTypeSym1 == null ||
                LookupInClass(classTypeSym1, ref classTypeSym2))
            {
                // Search the interfaces.
                if ((interfaceTypeSym != null || interfaceArray.Count > 0) &&
                    LookupInInterfaces(interfaceTypeSym, interfaceArray) &&
                    classTypeSym2 != null)
                {
                    // Search object last.
                    DebugUtil.Assert(classTypeSym2 != null && classTypeSym2.IsPredefType(PREDEFTYPE.OBJECT));

                    AGGTYPESYM tempSym = null;
                    LookupInClass(classTypeSym2, ref tempSym);
                }
                // (CS3) Search the extension method.
                else
                {
                    LookupExtensionMethodInInterfaces(classTypeSym1);
                }
            }

            DebugUtil.Assert(
                (this.firstSymWithType == null) == (this.TypeCount == 0) &&
                (this.firstSymWithType == null || this.containingTypeList[0] == this.firstSymWithType.AggTypeSym));
            DebugUtil.Assert(
                this.isMulti == (this.firstSymWithType != null && (this.firstSymWithType.Sym.IsMETHSYM ||
                this.firstSymWithType.Sym.IsPROPSYM && this.firstSymWithType.PropSym.IsIndexer)));

            return !FError();
        }

        //
        //    Results of the lookup.
        //

        //------------------------------------------------------------
        // MemberLookup.FError
        //
        /// <summary>
        /// Whether there were errors.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FError()
        {
#if DEBUG
            DebugUtil.Assert(this.debug_isValid);
#endif
            return (firstSymWithType == null || ambiguousSymWithType != null);
        }

        //------------------------------------------------------------
        // MemberLookup.
        //------------------------------------------------------------
        //// Whether we can give an error better than "member not found".
        //bool HasIntelligentErrorInfo() {
        //    ASSERT(m_fValid); return m_swtAmbig || m_swtInaccess || m_swtBad || m_swtBogus || m_swtBadArity; }

        //------------------------------------------------------------
        // MemberLookup.
        //------------------------------------------------------------
        //// The number of symbols found.
        //int SymCount() { ASSERT(m_fValid); return m_csym; }

        //------------------------------------------------------------
        // MemberLookup.
        //------------------------------------------------------------
        //// The first symbol found.
        //SYM * SymFirst() { ASSERT(m_fValid); return m_swtFirst.Sym(); }
        //SymWithType & SwtFirst() { ASSERT(m_fValid); return m_swtFirst; }

        //------------------------------------------------------------
        // MemberLookup.FirstSymAsAggTypeSym
        //
        /// <summary>
        /// <para>It's only valid to call this when there is no error and
        /// the first element found is an AGGSYM. The size of typeArgs must match the arity.</para>
        /// <para>(SymFirstAsAts in sscli.)</para>
        /// </summary>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM FirstSymAsAggTypeSym(TypeArray typeArgs)    // SymFirstAsAts(TypeArray typeArgs)
        {
            DebugUtil.Assert(typeArgs != null);
#if DEBUG
            DebugUtil.Assert(this.debug_isValid);
#endif
            DebugUtil.Assert(
                this.firstSymWithType != null &&
                this.firstSymWithType.Sym.IsAGGSYM &&
                (this.firstSymWithType.Sym as AGGSYM).TypeVariables.Count == this.arity);
            DebugUtil.Assert(this.arity== typeArgs.Count);
            DebugUtil.Assert(
                this.containingTypeList[0] != null &&
                this.containingTypeList[0].GetAggregate() ==
                    (this.firstSymWithType.Sym as AGGSYM).GetOuterAgg());

            return compiler.MainSymbolManager.GetInstAgg(
                this.firstSymWithType.Sym as AGGSYM,
                containingTypeList[0],
                typeArgs,
                null);
        }

        //------------------------------------------------------------
        // MemberLookup.
        //------------------------------------------------------------
        //// Whether the kind of symbol found is method, aggregate, or indexer.
        //bool FMultiKind() { ASSERT(m_fValid); return m_fMulti; }

        //------------------------------------------------------------
        // MemberLookup.
        //------------------------------------------------------------
        //// Number of types in which symbols were found.
        //int TypeCount() { ASSERT(m_fValid); return m_ctype; }

        //------------------------------------------------------------
        // MemberLookup.GetTypeSym
        //
        /// <summary>
        /// <para>Retrieve the i'th type.</para>
        /// <para>AGGTYPESYM * Type(int i) in sscli.</para>
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetContainingTypeSym(int i)
        {
#if DEBUG
            DebugUtil.Assert(this.debug_isValid);
#endif
            DebugUtil.Assert(0 <= i && i < containingTypeList.Count);
            return containingTypeList[i];
        }

        //------------------------------------------------------------
        // MemberLookup.GetAllTypes
        //
        /// <summary>
        /// <para>Put all the types in a type array.</para>
        /// <para>Return a type array holding all the types that contain results.
        /// Overload resolution uses this.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray GetAllTypes()
        {
#if DEBUG
            DebugUtil.Assert(this.debug_isValid);
#endif
            return compiler.MainSymbolManager.AllocParams(
                containingTypeList.ConvertAll<TYPESYM>(SystemConverter.AggTypeSymToTypeSym));
        }

        //
        //    Operations after the lookup.
        //

        //------------------------------------------------------------
        // MemberLookup.ReportErrors
        //
        /// <summary>
        /// Reports errors. Only call this if FError() is true.
        /// </summary>
        /// <param name="treeNode"></param>
        //------------------------------------------------------------
        internal void ReportErrors(BASENODE treeNode)
        {
#if DEBUG
            DebugUtil.Assert(debug_isValid);
#endif
            DebugUtil.Assert(FError());

            // Report error.
            // NOTE: If the definition of FError changes, this code will need to change.
            DebugUtil.Assert(this.firstSymWithType == null || this.ambiguousSymWithType != null);
            if (this.firstSymWithType != null)
            {
                // Ambiguous lookup.
                compiler.ErrorRef(
                    treeNode,
                    CSCERRID.ERR_AmbigMember,
                    new ErrArgRef(this.firstSymWithType),
                    new ErrArgRef(this.ambiguousSymWithType));
            }
            else if (
                this.srcTypeSym.IsDelegateType() &&
                this.name == compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE))
            {
                compiler.Error(
                    treeNode != null && treeNode.Kind == NODEKIND.DOT ?
                        treeNode.AsANYBINOP.Operand2 : treeNode,
                    CSCERRID.ERR_DontUseInvoke);
            }
            else if (this.inaccessSymWithType != null)
            {
                if (!this.inaccessSymWithType.Sym.IsUserCallable() &&
                    (this.flags & MemLookFlagsEnum.UserCallable) != 0)
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_CantCallSpecialMethod,
                        new ErrArg(this.inaccessSymWithType));
                }
                else
                {
                    compiler.ClsDeclRec.ReportAccessError(
                        treeNode,
                        this.inaccessSymWithType,
                        this.whereParentSym,
                        this.qualTypeSym);
                }
            }
            else if ((this.flags & MemLookFlagsEnum.Ctor) != 0)
            {
                compiler.Error(
                    treeNode,
                    CSCERRID.ERR_NoConstructors,
                    new ErrArg(this.srcTypeSym.GetAggregate()));
            }
            else if ((this.flags & MemLookFlagsEnum.Operator) != 0)
            {
                compiler.Error(
                    treeNode,
                    CSCERRID.ERR_NoSuchMember,
                    new ErrArg(this.srcTypeSym),
                    new ErrArg(this.name));
            }
            else if ((this.flags & MemLookFlagsEnum.Indexer) != 0)
            {
                compiler.Error(treeNode, CSCERRID.ERR_BadIndexLHS, new ErrArg(this.srcTypeSym));
            }
            else if (this.badSymWithType != null)
            {
                compiler.Error(
                    treeNode,
                    CSCERRID.ERR_CantCallSpecialMethod,
                    new ErrArg(this.badSymWithType));
            }
            else if (this.bogusSymWithType != null)
            {
                ReportBogus(treeNode, this.bogusSymWithType);
            }
            else if (this.badAritySymWithType != null)
            {
                int cvar;

                switch (this.badAritySymWithType.Sym.Kind)
                {
                    case SYMKIND.METHSYM:
                        DebugUtil.Assert(this.arity > 0);
                        cvar = (this.badAritySymWithType.Sym as METHSYM).TypeVariables.Count;
                        compiler.ErrorRef(
                            treeNode,
                            cvar > 0 ? CSCERRID.ERR_BadArity : CSCERRID.ERR_HasNoTypeVars,
                            new ErrArgRef(this.badAritySymWithType),
                            new ErrArgSymKind(badAritySymWithType.Sym),
                            new ErrArgRef(cvar));
                        break;

                    case SYMKIND.AGGSYM:
                        cvar = (this.badAritySymWithType.Sym as AGGSYM).TypeVariables.Count;
                        compiler.ErrorRef(
                            treeNode,
                            cvar > 0 ? CSCERRID.ERR_BadArity : CSCERRID.ERR_HasNoTypeVars,
                            new ErrArgRef(this.badAritySymWithType),
                            new ErrArgSymKind(this.badAritySymWithType.Sym),
                            new ErrArgRef(cvar));
                        break;

                    default:
                        DebugUtil.Assert(this.arity > 0);
                        compiler.FuncBRec.ReportTypeArgsNotAllowedError(
                            treeNode,
                            this.arity,
                            new ErrArgRef(badAritySymWithType),
                            new ErrArgSymKind(this.badAritySymWithType.Sym));
                        break;
                }
            }
            else
            {
                if (this.srcTypeSym != null &&
                    this.srcTypeSym.ParentSym != null &&
                    !String.IsNullOrEmpty(this.srcTypeSym.ParentSym.Name) &&
                    !Char.IsLetter(this.srcTypeSym.ParentSym.Name[0]))
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_NameNotInContext,
                        new ErrArg(this.name));
                }
                else
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_NoSuchMember,
                        new ErrArg(this.srcTypeSym),
                        new ErrArg(this.name));
                }
            }
        }

        //------------------------------------------------------------
        // MemberLookup.ReportWarnings
        //
        /// <summary>
        /// <para>Reports warnings after a successful lookup.</para>
        /// <para>Only call this if FError() is false.</para>
        /// </summary>
        /// <param name="tree"></param>
        //------------------------------------------------------------
        internal void ReportWarnings(BASENODE tree)
        {
#if DEBUG
            DebugUtil.Assert(this.debug_isValid);
            DebugUtil.Assert(!this.FError());
#endif
            if (this.ambiguousWarnSymWithType != null)
            {
                // Ambiguous lookup.
                compiler.ErrorRef(
                    tree,
                    CSCERRID.WRN_AmbigLookupMeth,
                    new ErrArgRef(this.firstSymWithType),
                    new ErrArgRef(this.ambiguousWarnSymWithType));
            }
        }

        //------------------------------------------------------------
        // MemberLookup.FillGroup
        //
        /// <summary>
        /// <para>Fills in a member group from the results of the lookup.</para>
        /// <para>Only call this if the result is a method group or indexer group.</para>
        /// <para>Assume no type arguments.</para>
        /// <para>EXPRMEMGRP.MethPropSym is set null.</para>
        /// </summary>
        /// <param name="grpExpr"></param>
        //------------------------------------------------------------
        internal void FillGroup(EXPRMEMGRP grpExpr)
        {
#if DEBUG
            DebugUtil.Assert(this.debug_isValid);
            DebugUtil.Assert(!FError());
#endif
            // This should only be called if we found a method group or indexer group.
            DebugUtil.Assert(
                this.isMulti &&
                this.firstSymWithType != null &&
                (this.firstSymWithType.Sym.IsMETHSYM ||
                this.firstSymWithType.Sym.IsPROPSYM && this.firstSymWithType.PropSym.IsIndexer));

            grpExpr.Name = this.name;
            grpExpr.TypeArguments = BSYMMGR.EmptyTypeArray;
            grpExpr.SymKind = this.firstSymWithType.Sym.Kind;
            grpExpr.ParentTypeSym = this.srcTypeSym;
            grpExpr.MethPropSym = null;
            grpExpr.ObjectExpr = this.expr;
            grpExpr.ContainingTypeArray = compiler.MainSymbolManager.AllocParams(
                containingTypeList.ConvertAll<TYPESYM>(SystemConverter.AggTypeSymToTypeSym));
            grpExpr.Flags |= (EXPRFLAG)this.flags;
        }
    }
}
