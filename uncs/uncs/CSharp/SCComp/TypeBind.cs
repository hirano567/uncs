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
// File: typebind.h
//
// Defines the structure used when binding types
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
// File: typebind.cpp
//
// Type lookup and binding.
// ===========================================================================

//============================================================================
// TypeBind.cs
//
// 2015/02/26
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //======================================================================
    // TypeBindFlagsEnum
    //
    /// <summary>
    /// Type bind flags.
    /// (CSharp\SCComp\TypeBind.cs)
    /// </summary>
    //======================================================================
    [Flags]
    internal enum TypeBindFlagsEnum : int
    {
        None = 0x00,

        /// <summary>
        /// Call PreLookup before doing normal lookup
        /// - used for XML references.
        /// </summary>
        CallPreHook = 0x01,

        /// <summary>
        /// If an accessible item isn't found, use an inaccessible one instead
        /// - used for XML references.
        /// </summary>
        AllowInaccessible = 0x02,

        /// <summary>
        /// Don't check for deprecated.
        /// </summary>
        NoDeprecated = 0x04,

        /// <summary>
        /// Suppress errors.
        /// This may cause NULL to be returned when non-NULL (with errors) would have been returned.
        /// </summary>
        SuppressErrors = 0x08,

        /// <summary>
        /// If the lookout failed because the name was missing,
        /// just return NULL (don't error).
        /// </summary>
        AllowMissing = 0x10,

        /// <summary>
        /// Don't check for bogus when deciding accessibility.
        /// </summary>
        NoBogusCheck = 0x20,

        /// <summary>
        /// Avoid calling EnsureState at all.
        /// </summary>
        AvoidEnsureState = 0x40,

        /// <summary>
        /// <para>(CS3) "var" is regarded as the implicit type.</para>
        /// </summary>
        AllowImplicitType = 0x80,
    }

    //======================================================================
    // enum CheckConstraintsFlagsEnum
    //
    /// <summary>
    /// <para>CheckConstraints options.</para>
    /// <para>Has four members: None, Outer, NoDupErrors, NoErrors</para>
    /// (CSharp\SCComp\TypeBind.cs)
    /// </summary>
    //======================================================================
    [Flags]
    internal enum CheckConstraintsFlagsEnum : int
    {
        None = 0x00,
        Outer = 0x01,
        NoDupErrors = 0x02,
        NoErrors = 0x04,
    }

    //======================================================================
    // enum KRankEnum
    //
    /// <summary>
    /// TypeImp &lt; NamespaceImp &lt; NamespaceThis &lt; TypeThis
    /// </summary>
    //======================================================================
    enum KRankEnum : int
    {
        None = 0,

        /// <summary>
        /// Public type in an imported assembly.
        /// </summary>
        TypeImp,

        /// <summary>
        /// Namespace used in imported assemblies.
        /// </summary>
        NamespaceImp,

        /// <summary>
        /// Namespace used in this assembly.
        /// </summary>
        NamespaceThis,

        /// <summary>
        /// Type in source or an added module (this assembly).
        /// </summary>
        TypeThis,
    }


    //======================================================================
    // class TypeBind
    //
    /// <summary>
    /// <para>TypeBind has static methods to accomplish most tasks.</para>
    /// <para>For some of these tasks there are also instance methods.
    /// The instance method versions don't report not found errors (they may report others)
    /// but instead record error information in the TypeBind instance.
    /// Call the ReportErrors method to report recorded errors.</para>
    /// <para>This class has instance fields which represents context to bind.</para>
    /// </summary>
    //======================================================================
    internal class TypeBind
    {
        //------------------------------------------------------------
        // TypeBind Fields and Properties
        //------------------------------------------------------------

        // Outputs for error reporting. These are relevant only if the symbol wasn't found.

        protected SYM inaccessSym = null;   // SYM * m_symInaccess;
        protected SYM bogusSym = null;      // SYM * m_symBogus;
        protected SYM badKindSym = null;    // SYM * m_symBadKind;
        protected SYM badAritySym = null;   // SYM * m_symBadArity;

        // Inputs.

        protected COMPILER compiler = null;     // COMPILER * m_cmp;

        /// <summary>
        /// Context for error messages.
        /// </summary>
        protected SYM errorContextSym = null;   // SYM * m_symCtx;

        /// <summary>
        /// Lookup on a single name starts here (unless alias qualified).
        /// </summary>
        protected SYM startSym = null;          // SYM * m_symStart;

        /// <summary>
        /// Accessibility is checked from here.
        /// </summary>
        protected SYM accessFromSym = null;         // SYM * m_symAccess;

        /// <summary>
        /// Type variables belong to this SYM are checked first.
        /// </summary>
        protected SYM typeVariablesSym = null;       // SYM * m_symTypeVars;

        protected TypeBindFlagsEnum flags = 0;  // TypeBindFlagsEnum m_flags : 16;

        /// <summary>
        /// Currently resolving an alias ?
        /// </summary>
        protected bool resolvingUsingAlias = false;      // bool m_fUsingAlias;

#if DEBUG
        protected bool isValid = false; // m_fValid
#endif
        protected COMPILER Compiler
        {
            get
            {
#if DEBUG
                DebugUtil.Assert(isValid);
#endif
                return this.compiler;
            }
        }

        protected bool AllowImplicitType
        {
            get { return ((this.flags & TypeBindFlagsEnum.AllowImplicitType) != 0); }
        }

        protected void ClearAllowImplicitType()
        {
            this.flags &= ~TypeBindFlagsEnum.AllowImplicitType;
        }

        //------------------------------------------------------------
        // TypeBind Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal TypeBind()
        {
#if DEBUG
            isValid = false;
#endif
        }

        //------------------------------------------------------------
        // TypeBind Constructor (2)
        //
        /// <summary></summary>
        /// <param name="cmp"></param>
        /// <param name="errorContextSym"></param>
        /// <param name="startSym"></param>
        /// <param name="accessSym"></param>
        /// <param name="typeVarsSym"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        protected TypeBind(
            COMPILER cmp,
            SYM errorContextSym,
            SYM startSym,
            SYM accessSym,
            SYM typeVarsSym,
            TypeBindFlagsEnum flags)
        {
            Init(cmp, errorContextSym, startSym, accessSym, typeVarsSym, flags);
        }

        //------------------------------------------------------------
        // TypeBind.Init
        //
        /// <summary>
        /// Initialize the TypeBind instance.
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="symCtx"></param>
        /// <param name="symStart"></param>
        /// <param name="symAccess"></param>
        /// <param name="symTypeVars"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        protected void Init(
            COMPILER cmp,
            SYM errorContextSym,
            SYM startSym,
            SYM accessFromSym,
            SYM typeVarsSym,
            TypeBindFlagsEnum flags)
        {
            // symAccess may be NULL (indicating that only publics are accessible).
            DebugUtil.Assert(cmp != null && errorContextSym != null && startSym != null);
            DebugUtil.Assert(typeVarsSym == null || typeVarsSym.IsMETHSYM || typeVarsSym.IsAGGSYM);

            this.compiler = cmp;
            this.errorContextSym = errorContextSym;
            this.startSym = startSym;
            this.accessFromSym = accessFromSym;
            this.typeVariablesSym = typeVarsSym;
            this.flags = flags;
#if DEBUG
            this.isValid = true;
#endif

            // In define members or before, we shouldn't check deprecated.
            if (Compiler.AggStateMax < AggStateEnum.Prepared)
            {
                this.flags |= TypeBindFlagsEnum.NoDeprecated;
            }
        }

        /////// Instance Methods /////

        //------------------------------------------------------------
        // TypeBind.SearchNamespacesInst
        //
        /// <summary>
        /// <para>SearchNamespacesInst searches the namespaces starting at decl.
        /// If decl is an AGGDECLSYM, the search starts at the NSDECLSYM containing decl.</para>
        /// <para>Search a namespace declaration and its containing declarations (including using claues)
        /// for the name. This is an instance method.</para>
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <param name="declSym"></param>
        /// <param name="accessSym"></param>
        /// <param name="bindFlags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM SearchNamespacesInst(
            COMPILER cmp,
            BASENODE node,
            string name,
            TypeArray typeArgs,
            DECLSYM declSym,
            SYM accessSym,
            TypeBindFlagsEnum bindFlags)    //  = TypeBindFlags::None
        {
            DebugUtil.Assert(declSym.IsAGGDECLSYM || declSym.IsNSDECLSYM);

            // Find the NSDECL
            DECLSYM declStart;
            for (declStart = declSym; !declStart.IsNSDECLSYM; declStart = declStart.ParentDeclSym)
            {
                ;
            }

            Init(cmp, declSym, declStart, accessSym, null, bindFlags);
            ClearErrorInfo();
            return SearchNamespacesCore(declStart as NSDECLSYM, node, name, typeArgs);
        }

        //------------------------------------------------------------
        // TypeBind.HasIntelligentErrorInfo
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool HasIntelligentErrorInfo()
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            return (
                this.inaccessSym != null ||
                this.bogusSym != null ||
                this.badKindSym != null ||
                this.badAritySym != null);
        }

        //------------------------------------------------------------
        // TypeBind.ReportErrors
        //
        /// <summary>
        /// Report the errors found.
        /// May morph *psym from NULL to non-NULL after reporting an error.
        /// If the compiler's AggStateMax is at least Prepared
        /// then this prepares any type that is returned.
        /// </summary>
        /// <remarks>In sscli,
        /// void TypeBind::ReportErrors(
        ///     BASENODE * node,
        ///     NAME * name,
        ///     SYM * symLeft,
        ///     TypeArray * typeArgs,
        ///     SYM ** psym)
        /// </remarks>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="leftSym"></param>
        /// <param name="typeArgs"></param>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal SYM ReportErrors(
            BASENODE node,
            string name,
            SYM leftSym,
            TypeArray typeArgs,
            SYM oldSym)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(node != null && name != null);
            DebugUtil.Assert(
                leftSym == null ||
                leftSym.IsAGGTYPESYM ||
                leftSym.IsNSAIDSYM ||
                leftSym.IsERRORSYM);
            DebugUtil.Assert(
                this.inaccessSym == null ||
                this.inaccessSym != this.badKindSym);

            SYM newSym = oldSym;

            if (FSuppressErrors())
            {
                if (newSym == null && (this.flags & TypeBindFlagsEnum.AllowInaccessible) != 0)
                {
                    if (this.inaccessSym != null)
                    {
                        newSym = this.inaccessSym;
                    }
                    else if (this.bogusSym != null)
                    {
                        newSym = this.bogusSym;
                    }
                }
                if (newSym == null && FAllowMissing())
                {
                    return newSym;
                }
                goto LDone;
            }

            if (newSym != null)
            {
                if (newSym.IsTYPESYM)
                {
                    CheckType(Compiler, node, newSym as TYPESYM, this.errorContextSym, this.flags);
                    // CheckType already called EnsureState so just return.
                }
                return newSym;
            }

            if (FAllowMissing())
            {
                return newSym;
            }

            if (this.inaccessSym != null)
            {
                // found an inaccessible name or an uncallable name
                if (!this.inaccessSym.IsUserCallable())
                {
                    Compiler.Error(
                        node,
                        CSCERRID.ERR_CantCallSpecialMethod,
                        new ErrArg(name),
                        new ErrArgRefOnly(this.inaccessSym));
                }
                else
                {
                    Compiler.Error(node, CSCERRID.ERR_BadAccess, new ErrArg(this.inaccessSym));
                }
                newSym = this.inaccessSym;
            }
            else if (this.bogusSym != null)
            {
                Compiler.ErrorRef(node, CSCERRID.ERR_BogusType, new ErrArgRef(this.bogusSym));
                newSym = this.bogusSym;
            }
            else if (this.badKindSym != null || this.badAritySym != null)
            {
                if (this.badKindSym != null)
                {
                    Compiler.Error(
                        node,
                        CSCERRID.ERR_BadSKknown,
                        new ErrArg(this.badKindSym),
                        new ErrArgSymKind(this.badKindSym),
                        new ErrArg(SYMKIND.AGGSYM));
                }

                if (this.badAritySym != null)
                {
                    if (this.badAritySym.IsAGGSYM)
                    {
                        int cvar = (this.badAritySym as AGGSYM).TypeVariables.Count;
                        Compiler.ErrorRef(
                            node,
                            cvar > 0 ? CSCERRID.ERR_BadArity : CSCERRID.ERR_HasNoTypeVars,
                           new ErrArgRef(this.badAritySym),
                           new ErrArgSymKind(this.badAritySym),
                           new ErrArgRef(cvar));
                    }
                    else
                    {
                        Compiler.ErrorRef(
                            node,
                            CSCERRID.ERR_TypeArgsNotAllowed,
                            new ErrArgRef(this.badAritySym),
                            new ErrArgSymKind(this.badAritySym));
                    }
                }
            }
            // Didn't find anything at all.
            else if (leftSym != null)
            {
                if (leftSym == Compiler.MainSymbolManager.GetGlobalNsAid())
                {
                    Compiler.Error(node, CSCERRID.ERR_GlobalSingleTypeNameNotFound, new ErrArg(name));
                }
                else
                {
                    CSCERRID err = leftSym.IsNSAIDSYM ?
                       CSCERRID.ERR_DottedTypeNameNotFoundInNS :
                       CSCERRID.ERR_DottedTypeNameNotFoundInAgg;
                    Compiler.Error(node, err, new ErrArg(name), new ErrArg(leftSym));
                }
            }
            else
            {
                Compiler.Error(
                    node,
                    CSCERRID.ERR_SingleTypeNameNotFound,
                    new ErrArg(name),
                    new ErrArg(this.errorContextSym));
            }

        LDone:
            if (newSym == null)
            {
                newSym = Compiler.MainSymbolManager.GetErrorType(leftSym as PARENTSYM, name, typeArgs);
            }
            else if (
                newSym.IsTYPESYM &&
                Compiler.AggStateMax >= AggStateEnum.Prepared &&
                (this.flags & TypeBindFlagsEnum.AvoidEnsureState) == 0)
            {
                Compiler.EnsureState(newSym as TYPESYM, AggStateEnum.Prepared);
            }
            return newSym;
        }

        ////// Static Methods //////

        //------------------------------------------------------------
        // TypeBind.BindName (static)
        //
        /// <summary>
        /// <para>BindName resolves an NK_DOT, NK_NAME, NK_GENERICNAME or NK_ALIASNAME
        /// to a NSAIDSYM or AGGTYPESYM.</para>
        /// <para>symCtx is used for accessibility checking and the place to search.</para>
        /// <para>Bind the given node to an NSAIDSYM or AGGTYPESYM.
        /// The node should be an NK_DOT, NK_NAME, NK_GENERICNAME or NK_ALIASQUALNAME.</para>
        /// <para>Create a TypeBind instance and call BindNameCore method.</para>
        /// </summary> 
        /// <param name="cmp"></param>
        /// <param name="node"></param>
        /// <param name="contextSym"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        static internal SYM BindName(
            COMPILER cmp,
            BASENODE node,
            SYM contextSym,
            TypeBindFlagsEnum flags)    // = TypeBindFlags::None
        {
            DebugUtil.Assert(
                contextSym.IsNSDECLSYM ||
                contextSym.IsAGGDECLSYM ||
                contextSym.IsMETHSYM);

            TypeBind tb = new TypeBind(cmp, contextSym, contextSym, contextSym, null, flags);
            return tb.BindNameCore(node);
        }

        //------------------------------------------------------------
        // TypeBind.BindNameToType (static)
        //
        /// <summary>
        /// <para>BindNameToType calls BindName then errors (if not TypeBindFlags::SuppressErrors)
        /// and returns NULL if the result is not a type.
        /// symCtx is used for accessibility checking and the place to search.</para>
        /// <para>Bind the given node to a type.
        /// If the name resolves to something other than a type,
        /// reports an error (if !SuppressErrors) and returns ERRORSYM.
        /// The node should be an NK_DOT, NK_NAME, NK_GENERICNAME or NK_ALIASQUALNAME
        /// (the latter will always produce an error).</para>
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="node"></param>
        /// <param name="contextSym"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal TYPESYM BindNameToType(
            COMPILER cmp,
            BASENODE node,
            SYM contextSym,
            TypeBindFlagsEnum flags)	//  = TypeBindFlags::None
        {
            DebugUtil.Assert(
                contextSym.IsNSDECLSYM ||
                contextSym.IsAGGDECLSYM ||
                contextSym.IsMETHSYM);

            TypeBind tb = new TypeBind(cmp, contextSym, contextSym, contextSym, null, flags);
            return tb.BindNameToTypeCore(node);
        }

        //------------------------------------------------------------
        // TypeBind.BindType (static)
        //
        /// <summary>
        /// <para>Bind the given node to a type.</para>
        /// <para>BindType resolves a TYPEBASENODE to a type.
        /// symCtx is used for accessibility checking and the place to search.</para>
        /// <para>(argument flags has default value TypeBindFlags::None in sscli.)</para>
        /// </summary>
        //------------------------------------------------------------
        internal static TYPESYM BindType(
            COMPILER cmp,
            TYPEBASENODE node,
            SYM contextSym,
            TypeBindFlagsEnum flags)    // = TypeBindFlags::None
        {
            DebugUtil.Assert(
                contextSym.IsNSDECLSYM ||
                contextSym.IsAGGDECLSYM ||
                contextSym.IsMETHSYM);

            TypeBind tb = new TypeBind(
                cmp,
                contextSym,
                contextSym,
                contextSym,
                null,
                flags);
            return tb.BindTypeCore(node);
        }

        //------------------------------------------------------------
        // TypeBind.BindTypeAggDeclExt (static)
        //
        /// <summary>
        /// <para>BindTypeAggDeclExt resolves a TYPEBASENODE to a type,
        /// searching only the "exterior" of a particular AGGDECLSYM.
        /// This means, type variables of the AGG are considered, but members are not.
        /// The AGGSYM is use for accessibility. The search starts with the type variables and
        /// continues to the enclosing types and namespaces.</para>
        /// <para>Bind the given node to a type. Only the "exterior" of the agg is in scope
        /// - type variables are in scope but members are not.</para>
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="aggDeclSym"></param>
        /// <param name="node"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal TYPESYM BindTypeAggDeclExt(
            COMPILER cmp,
            TYPEBASENODE node,
            AGGDECLSYM aggDeclSym,
            TypeBindFlagsEnum flags)    // = TypeBindFlags::None)
        {
            TypeBind tb = new TypeBind(
                cmp,
                aggDeclSym,
                aggDeclSym.ParentDeclSym,
                aggDeclSym,
                aggDeclSym.AggSym,
                flags);
            return tb.BindTypeCore(node);
        }

        //------------------------------------------------------------
        // TypeBind.BindTypeWithTypeVars (static)
        //
        /// <summary>
        /// <para>BindTypeWithTypeVars first looks in the type variables of symTypeVars,
        /// then looks in symStart and its base and outer contexts.</para>
        /// <para>Bind the given node to a type.
        /// The search starts with the type variables of symTypeVars, then continues with symStart.</para>
        /// <para>The default value of flags is TypeBindFlagsEnum.None</para>
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="node"></param>
        /// <param name="startSym"></param>
        /// <param name="accessSym"></param>
        /// <param name="typeVarsSym"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        static internal TYPESYM BindTypeWithTypeVars(
            COMPILER cmp,
            TYPEBASENODE node,
            SYM startSym,
            SYM accessSym,
            SYM typeVarsSym,
            TypeBindFlagsEnum flags) // = TypeBindFlags::None
        {
            TypeBind tb = new TypeBind(cmp, startSym, startSym, accessSym, typeVarsSym, flags);
            return tb.BindTypeCore(node);
        }

        //------------------------------------------------------------
        // TypeBind.BindTypeArgs (static)
        //
        /// <summary>
        /// <para>BindTypeArgs resolves type arguments for a generic name. It accepts ANYNAME.</para>
        /// <para>Bind the generic args of the given NAMENODE to a type array.
        /// If the NAMENODE is not generic, return the empty type array (not NULL).
        /// Returns NULL iff SuppressErrors is specified and
        /// there were errors in binding one or more type argument.</para>
        /// </summary>
        //------------------------------------------------------------
        static internal TypeArray BindTypeArgs(
            COMPILER cmp,
            NAMENODE node,
            SYM contextSym,
            TypeBindFlagsEnum flags) //  = TypeBindFlags::None
        {
            if (node.Kind != NODEKIND.GENERICNAME)
            {
                return BSYMMGR.EmptyTypeArray;
            }
            TypeBind tb = new TypeBind(cmp, contextSym, contextSym, contextSym, null, flags);
            return tb.BindTypeArgsCore(node);
        }

        //------------------------------------------------------------
        // TypeBind.BindAttributeType (static)
        //
        /// <summary>
        /// <para>Looks for an attribute type.</para>
        /// <para>Bind the node as an attribute type.
        /// The resulting types actually name may have "Attribute" appended
        /// to the last identifier.</para>
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="node"></param>
        /// <param name="contextSym"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static TYPESYM BindAttributeType(
            COMPILER cmp,
            BASENODE node,
            SYM contextSym,
            TypeBindFlagsEnum flags) // = TypeBindFlags::None
        {
            TypeBind tb = new TypeBind(cmp, contextSym, contextSym, contextSym, null, flags);
            return tb.BindAttributeTypeCore(node);
        }

        //------------------------------------------------------------
        // TypeBind.BindUsingAlias (static)
        //
        /// <summary>
        /// <para>Gets the SYM associated with a using alias.</para>
        /// <para>Return the type or namespace that the using alias references.
        /// Resolves the alias if it hasn't been resolved yet.
        /// May return NULL indicating that there was an error resolving the alias.
        /// Can only return an NSAIDSYM or AGGTYPESYM.</para>
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="aliasSym"></param>
        //------------------------------------------------------------
        static internal SYM  BindUsingAlias(COMPILER  cmp, ALIASSYM aliasSym)
        {
            if (!aliasSym.HasBeenBound)
            {
                if (aliasSym.IsExtern)
                {
                    // Extern aliasSym.
                    DebugUtil.Assert((aliasSym.ParseTreeNode as USINGNODE).NameNode == null);

                    EXTERNALIASSYM  exAliasSym = cmp.LookupGlobalSym(
                        aliasSym.Name,
                        cmp.ExternalAilasParentSym,
                        SYMBMASK.EXTERNALIASSYM) as EXTERNALIASSYM;
                    if (exAliasSym==null)
                    {
                        cmp.Error(
                            aliasSym.ParseTreeNode,
                            CSCERRID.ERR_BadExternAlias,
                            new ErrArg( aliasSym));
                    }
                    else
                    {
                        aliasSym.Sym= exAliasSym.NsAidSym;
                    }
                }
                else
                {
                    NSDECLSYM  nsdPar = aliasSym.ParentSym as NSDECLSYM;
                    BASENODE  nodeRight = (aliasSym.ParseTreeNode as USINGNODE).NameNode;
                    DebugUtil.Assert(nodeRight!=null);
        
                    // Right hand side of aliasSym isn't bound yet.
                    // Check enclosing namespace (but not using clauses)
                    // then parent namespace declarations.
        
                    TypeBind tb=new TypeBind(
                        cmp,
                        nsdPar,
                        nsdPar,
                        nsdPar,
                        null,
                        TypeBindFlagsEnum.None);
                    tb.resolvingUsingAlias = true;
        
                    aliasSym.Sym = tb.BindNameCore(nodeRight);
        
                    DebugUtil.Assert(
                        aliasSym.Sym==null ||
                        aliasSym.Sym.IsNSAIDSYM ||
                        aliasSym.Sym.IsAGGTYPESYM ||
                        aliasSym.Sym.IsERRORSYM);
                }
                aliasSym.HasBeenBound= true;
            }
        
            return aliasSym.Sym;
        }

        //------------------------------------------------------------
        // TypeBind.CheckType (static)
        //
        /// <summary>
        /// <para>Checks deprecated, bogus, constraints, etc.</para>
        /// <para>Static method to check for type errors - like deprecated, constraints, etc.</para>
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="node"></param>
        /// <param name="typeSym"></param>
        /// <param name="contextSym"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        static internal void CheckType(
            COMPILER compiler,
            BASENODE node,
            TYPESYM typeSym,
            SYM contextSym,
            TypeBindFlagsEnum flags)    // = TypeBindFlags::None
        {
            if (typeSym == null || typeSym.IsERRORSYM)
            {
                return;
            }

            if (compiler.AggStateMax >= AggStateEnum.Prepared)
            {
                compiler.EnsureState(typeSym, AggStateEnum.Prepared);
            }

            if ((flags & TypeBindFlagsEnum.SuppressErrors) != 0)
            {
                return;
            }

            if (typeSym.IsAGGTYPESYM || typeSym.IsNUBSYM)
            {
                if (compiler.CompilationPhase >= CompilerPhaseEnum.EvalConstants)
                {
                    CheckConstraints(compiler, node, typeSym, CheckConstraintsFlagsEnum.None);
                }
            }
            if (typeSym.IsDeprecated() && (flags & TypeBindFlagsEnum.NoDeprecated) == 0)
            {
                compiler.ClsDeclRec.ReportDeprecated(node, contextSym, new SymWithType(typeSym, null));
            }
            if (typeSym.IsPredefType(PREDEFTYPE.SYSTEMVOID))
            {
                compiler.Error(node, CSCERRID.ERR_SystemVoid);
            }
        }

        //------------------------------------------------------------
        // TypeBind.SearchNamespaces
        //
        // SearchNamespaces looks for the given identifier
        // in the namespace declaration and any enclosing declarations.
        // Using clauses are honored.
        // symAccess is used for accessibility checking.
        // The typeArgs specify the arity (and are applied to any found type).
        // node is just used for error reporting.
        //------------------------------------------------------------
        //static SYM * SearchNamespaces(
        //    COMPILER * cmp,
        //    BASENODE * node,
        //    NAME * name,
        //    TypeArray * typeArgs,
        //    NSDECLSYM * nsd,
        //    SYM * symAccess,
        //    TypeBindFlagsEnum flags = TypeBindFlags::None);
        //
        //------------------------------------------------------------
        // Static method to search a namespace declaration and its containing declarations
        // (including using claues) for the name.
        //------------------------------------------------------------
        //SYM * TypeBind::SearchNamespaces(
        //    COMPILER * cmp,
        //    BASENODE * node,
        //    NAME * name,
        //    TypeArray * typeArgs,
        //    NSDECLSYM * nsd,
        //    SYM * symAccess,
        //    TypeBindFlagsEnum flags)
        //{
        //    TypeBind tb(cmp, nsd, nsd, symAccess, NULL, flags);
        //    tb.ClearErrorInfo();
        //
        //    SYM * sym = tb.SearchNamespacesCore(nsd, node, name, typeArgs);
        //
        //    tb.ReportErrors(node, name, NULL, typeArgs, &sym);
        //
        //    return sym;
        //}

        //------------------------------------------------------------
        // TypeBind.SearchSingleNamespace (static)
        //
        /// <summary>
        /// <para>Search a single namespace for the name.</para>
        /// <para>SearchSingleNamespace looks for the given identifier in the namespace.
        /// symAccess is used for accessibility checking.
        /// The typeArgs specify the arity (and are applied to any found type).</para>
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <param name="nsa"></param>
        /// <param name="symAccess"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal SYM SearchSingleNamespace(
            COMPILER cmp,
            BASENODE node,
            string name,
            TypeArray typeArgs,
            NSAIDSYM nsa,
            SYM symAccess,
            TypeBindFlagsEnum flags)	// = TypeBindFlags::None)
        {
            TypeBind tb = new TypeBind(
                cmp,
                nsa.NamespaceSym,
                nsa.NamespaceSym,
                symAccess,
                null,
                flags);
            tb.ClearErrorInfo();

            SYM sym = tb.SearchSingleNamespaceCore(nsa, node, name, typeArgs, false);
            sym = tb.ReportErrors(node, name, nsa, typeArgs, sym);
            return sym;
        }

        //------------------------------------------------------------
        // TypeBind.SearchNamespacesForAlias
        //
        // SearchNamespacesForAlias looks for an alias with the given name
        // in the namespace declaration and any enclosing declarations.
        //------------------------------------------------------------
        //static SYM * SearchNamespacesForAlias(
        //    COMPILER * cmp,
        //    BASENODE * node,
        //    NAME * name,
        //    NSDECLSYM * nsd,
        //    TypeBindFlagsEnum flags = TypeBindFlags::None);
        //
        //------------------------------------------------------------
        // Static method to search a namespace declaration and its containing declarations for a
        // using/extern alias.
        //------------------------------------------------------------
        //SYM * TypeBind::SearchNamespacesForAlias(
        //    COMPILER * cmp,
        //    BASENODE * node,
        //    NAME * name,
        //    NSDECLSYM * nsd,
        //    TypeBindFlagsEnum flags)
        //{
        //    TypeBind tb(cmp, nsd, nsd, nsd, NULL, flags);
        //    tb.ClearErrorInfo();
        //
        //    SYM * sym = tb.SearchNamespacesForAliasCore(nsd, node, name);
        //
        //    // SearchNamespaceForAliasCore already reported an error if the alias wasn't found.
        //    // Don't set the node sym. It's already been set to the alias sym.
        //    // tb.ReportErrors(node, name, NULL, NULL, &sym);
        //
        //    return sym;
        //}

        //------------------------------------------------------------
        // TypeBind.CheckConstraints (static)
        //
        /// <summary>
        /// Check the constraints of any type arguments in the given TYPESYM.
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="tree"></param>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool CheckConstraints(
            COMPILER compiler,
            BASENODE treeNode,
            TYPESYM typeSym,
            CheckConstraintsFlagsEnum flags)
        {
            typeSym = typeSym.GetNakedType(false);

            if (typeSym.IsNUBSYM)
            {
                TYPESYM tempSym = (typeSym as NUBSYM).GetAggTypeSym();
                if (tempSym != null)
                {
                    typeSym = tempSym;
                }
                else
                {
                    typeSym = typeSym.GetNakedType(true);
                }
            }

            if (!typeSym.IsAGGTYPESYM)
            {
                return true;
            }

            AGGTYPESYM aggTypeSym = typeSym as AGGTYPESYM;

            if (aggTypeSym.AllTypeArguments.Count == 0)
            {
                // Common case: there are no type vars, so there are no constraints.
                aggTypeSym.ConstraintsChecked = true;
                aggTypeSym.HasConstraintError = false;
                return true;
            }

            if (aggTypeSym.ConstraintsChecked)
            {
                // Already checked.
                if (!aggTypeSym.HasConstraintError ||
                    (flags & CheckConstraintsFlagsEnum.NoDupErrors) != 0)
                {
                    // No errors or no need to report errors again.
                    return !aggTypeSym.HasConstraintError;
                }
            }

            TypeArray typeVars = aggTypeSym.GetAggregate().TypeVariables;
            TypeArray typeArgs = aggTypeSym.TypeArguments;
            TypeArray typeArgsAll = aggTypeSym.AllTypeArguments;
            DebugUtil.Assert(typeVars.Count == typeArgs.Count);

            compiler.EnsureState(aggTypeSym, AggStateEnum.Prepared);
            compiler.EnsureState(typeVars, AggStateEnum.Prepared);
            compiler.EnsureState(typeArgsAll, AggStateEnum.Prepared);
            compiler.EnsureState(typeArgs, AggStateEnum.Prepared);

            if (aggTypeSym.AggState < AggStateEnum.DefinedMembers ||
                typeVars.AggState < AggStateEnum.DefinedMembers ||
                typeArgsAll.AggState < AggStateEnum.DefinedMembers ||
                typeArgs.AggState < AggStateEnum.DefinedMembers)
            {
                // Too early to check anything.
                DebugUtil.Assert(compiler.AggStateMax < AggStateEnum.DefinedMembers);
                return true;
            }

            if (!aggTypeSym.ConstraintsChecked)
            {
                aggTypeSym.ConstraintsChecked = true;
                aggTypeSym.HasConstraintError = false;
            }

            // Check the outer type first. If CheckConstraintsFlagsEnum.Outer is not specified and the
            // outer type has already been checked then don't bother checking it.
            if (aggTypeSym.OuterTypeSym != null &&
                ((flags & CheckConstraintsFlagsEnum.Outer) != 0 || !aggTypeSym.OuterTypeSym.ConstraintsChecked))
            {
                CheckConstraints(compiler, treeNode, aggTypeSym.OuterTypeSym, flags);
                aggTypeSym.HasConstraintError |= aggTypeSym.OuterTypeSym.HasConstraintError;
            }
#if DEBUG
            if (typeSym.SymID == 5920)
            {
                ;
            }
#endif
            if (typeVars.Count > 0)
            {
                aggTypeSym.HasConstraintError |= !CheckConstraintsCore(
                    compiler,
                    treeNode,
                    aggTypeSym.GetAggregate(),
                    typeVars,
                    typeArgs,
                    typeArgsAll,
                    null,
                    (flags & CheckConstraintsFlagsEnum.NoErrors));
            }

            // Now check type args themselves.
            for (int i = 0; i < typeArgs.Count; i++)
            {
                TYPESYM arg = typeArgs[i].GetNakedType(true);
                if (arg.IsAGGTYPESYM && !(arg as AGGTYPESYM).ConstraintsChecked)
                {
                    CheckConstraints(
                        compiler,
                        treeNode,
                        arg as AGGTYPESYM,
                        flags | CheckConstraintsFlagsEnum.Outer);
                    if ((arg as AGGTYPESYM).HasConstraintError)
                    {
                        aggTypeSym.HasConstraintError = true;
                    }
                }
            }

            // Nullable should have the value type constraint!
            DebugUtil.Assert(
                !aggTypeSym.IsPredefType(PREDEFTYPE.G_OPTIONAL) ||
                typeVars.ItemAsTYVARSYM(0).HasValueConstraint());

            return !aggTypeSym.HasConstraintError;
        }

        //------------------------------------------------------------
        // TypeBind.CheckMethConstraints (static)
        //
        /// <summary>
        /// Check the constraints on the method instantiation.
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="tree"></param>
        /// <param name="mwi"></param>
        //------------------------------------------------------------
        internal static void CheckMethConstraints(
            COMPILER cmp,
            BASENODE tree,
            MethWithInst mwi)
        {
            DebugUtil.Assert(mwi.MethSym != null && mwi.AggTypeSym != null && mwi.TypeArguments != null);
            DebugUtil.Assert(mwi.MethSym.TypeVariables.Count == mwi.TypeArguments.Count);
            DebugUtil.Assert(mwi.AggTypeSym.GetAggregate() == mwi.MethSym.ClassSym);

            if (mwi.TypeArguments.Count > 0)
            {
                cmp.EnsureState(mwi.MethSym.TypeVariables,AggStateEnum.Prepared);
                cmp.EnsureState(mwi.TypeArguments, AggStateEnum.Prepared);

                DebugUtil.Assert(mwi.MethSym.TypeVariables.AggState >= AggStateEnum.DefinedMembers);
                DebugUtil.Assert(mwi.TypeArguments.AggState >= AggStateEnum.DefinedMembers);

                CheckConstraintsCore(
                    cmp,
                    tree,
                    mwi.MethSym,
                    mwi.MethSym.TypeVariables,
                    mwi.TypeArguments,
                    mwi.AggTypeSym.AllTypeArguments,
                    mwi.TypeArguments,
                    CheckConstraintsFlagsEnum.None);
            }
        }

        //protected:

        //------------------------------------------------------------
        // TypeBind.ClearErrorInfo
        //
        /// <summary></summary>
        //------------------------------------------------------------
        protected void ClearErrorInfo()
        {
            // Outputs for error reporting.
            this.inaccessSym = null;
            this.bogusSym = null;
            this.badKindSym = null;
            this.badAritySym = null;
        }

        //------------------------------------------------------------
        // TypeBind.FSuppressErrors
        //
        /// <summary>
        /// Return true if SuppressErrors flag is set in this.flags field.
        /// </summary>
        //------------------------------------------------------------
        protected bool FSuppressErrors()
        {
            return (this.flags & TypeBindFlagsEnum.SuppressErrors) != 0;
        }

        //------------------------------------------------------------
        // TypeBind.FAllowMissing
        //
        /// <summary>
        /// <para>Determine that TypeBindFlagsEnum.AllowMissing is set in flags field.</para>
        /// <para>The return value true means that if names are not found, reutrn null.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool FAllowMissing()
        {
            return (this.flags & TypeBindFlagsEnum.AllowMissing) != 0;
        }

        // These update the error info and modify *psym.

        //------------------------------------------------------------
        // TypeBind.CheckAccess
        //
        /// <summary>
        /// <para>Check access and bogosity of the given symbol.
        /// Sets *psym to NULL if it's not accessible or is bogus.
        /// Updates m_symInaccess and m_symBogus as appropriate.</para>
        /// <para>Generally this should be called after CheckArity
        /// so only SYMs with the correct arity are put in m_symInaccess and m_symBogus.
        /// ReportErrors may return m_symInaccess or m_symBogus after reporting the errors!</para>
        /// <para>Call CLSDREC.CheckAccess to check the accessibility of argument sym from this.accessFromSym.
        /// If cannot access, set sym to this.inaccessSym and set null to the argument sym.</para>
        /// </summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        protected void CheckAccess(ref SYM sym)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            SYM symTemp = sym;

            // namespace are public.
            if (symTemp == null || symTemp.IsNSSYM)
            {
                return;
            }
            DebugUtil.Assert(symTemp.IsAGGTYPESYM);

            AGGTYPESYM checkAts = null;

            // CLSDREC::CheckAccess wants an AGGSYM and containing AGGTYPESYM.
            if (symTemp.IsAGGTYPESYM)
            {
                checkAts = symTemp as AGGTYPESYM;
                symTemp = checkAts.GetAggregate();  // Assign sym the parent sym of sym.(AGGSYM)
                checkAts = checkAts.OuterTypeSym;
            }

            if (!Compiler.ClsDeclRec.CheckAccess(symTemp, checkAts, this.accessFromSym, null))
            {
                if (this.inaccessSym == null)
                {
                    this.inaccessSym = sym; // Set the original.
                }
                sym = null;
                return;
            }

            if ((this.flags & TypeBindFlagsEnum.NoBogusCheck) == 0 && Compiler.CheckBogus(symTemp))
            {
                if (this.bogusSym == null)
                {
                    this.bogusSym = sym; // Set the original.
                }
                sym = null;
            }
        }

        //------------------------------------------------------------
        // TypeBind.CheckArity
        //
        /// <summary>
        /// <para>Check the arity of the given symbol.
        /// If the arity doesn't match, sets *psym to NULL.
        /// If the arity does match (and it's a type), sets *psym to the AGGTYPESYM.</para>
        /// <para>Call this before CheckAccess!</para>
        /// <para>Check the count of type parameters and type arguments.
        /// In the case of AGGSYM, If match, create a AGGTYPESYM instance.</para>
        /// <para>If not match, save it to this.badAritySym and  set null to sym.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="typeArgs"></param>
        /// <param name="typeOuter"></param>
        //------------------------------------------------------------
        protected void CheckArity(ref SYM sym, TypeArray typeArgs, AGGTYPESYM typeOuter)
        {
            if (sym == null)
            {
                return;
            }
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(typeArgs != null);
            SYM symTemp = sym;
            DebugUtil.Assert(symTemp.IsAGGSYM || symTemp.IsNSSYM || symTemp.IsTYVARSYM);

            switch (symTemp.Kind)
            {
                case SYMKIND.AGGSYM:
                    if (typeArgs.Count != (symTemp as AGGSYM).TypeVariables.Count)
                    {
                        if (this.badAritySym == null)
                        {
                            this.badAritySym = symTemp;
                        }
                        sym = null;
                        return;
                    }
                    // 
                    sym = Compiler.MainSymbolManager.GetInstAgg(
                        symTemp as AGGSYM,
                        typeOuter,
                        typeArgs,
                        null);
                    break;

                default:
                    if (typeArgs.Count > 0)
                    {
                        if (this.badAritySym == null)
                        {
                            this.badAritySym = symTemp;
                        }
                        sym = null;
                    }
                    break;
            }
        }

        // These are instance methods and call ClearErrorInfo and ReportErrors as appropriate.

        //------------------------------------------------------------
        // TypeBind.BindNameCore (1)
        //
        /// <summary>
        /// <para>Bind a dotted name, a single name (generic or not),
        /// or an alias qualified name.</para>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM BindNameCore(BASENODE node)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(node != null);

            switch (node.Kind)
            {
                case NODEKIND.DOT:
                    return BindDottedNameCore(
                        node.AsANYBINOP,
                        node.AsANYBINOP.Operand2.AsANYNAME.Name);

                case NODEKIND.NAME:
                case NODEKIND.GENERICNAME:
                case NODEKIND.OPENNAME:
                    return BindSingleNameCore(node.AsANYNAME, node.AsANYNAME.Name, null);

                case NODEKIND.ALIASNAME:
                    return BindAliasNameCore(node.AsALIASNAME);

                default:
                    DebugUtil.Assert(false, "Bad node kind in BindNameCore");
                    return null;
            }
            //return null;
        }

        //------------------------------------------------------------
        // TypeBind.BindNameCore (2)
        //
        /// <summary>
        /// <para>Bind a dotted name or a single name (generic or not).
        /// This is only called by BindAttributeTypeCore
        /// so it doesn't need to deal with OPENNAME and ALIASNAME.</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM BindNameCore(BASENODE node, string name)
        {
#if DEBUG
            DebugUtil.Assert(isValid);
#endif
            DebugUtil.Assert(name != null);

            switch (node.Kind)
            {
                case NODEKIND.DOT:
                    return BindDottedNameCore(node.AsANYBINOP, name);

                case NODEKIND.NAME:
                case NODEKIND.GENERICNAME:
                    return BindSingleNameCore(node.AsANYNAME, name, null);

                default:
                    DebugUtil.Assert(false, "Bad node kind in BindNameCore");
                    return null;
            }
        }

        //------------------------------------------------------------
        // TypeBind.BindNameToTypeCore
        //
        /// <summary>
        /// Bind a dotted name, a single name (generic or not),
        /// or an alias qualified name (A::B<...>) to a type.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM BindNameToTypeCore(BASENODE node)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(node != null);

            SYM sym = null;

            switch (node.Kind)
            {
                case NODEKIND.DOT:
                    sym = BindDottedNameCore(node.AsANYBINOP, node.AsANYBINOP.Operand2.AsANYNAME.Name);

                    if (sym != null &&
                        sym.IsTYPESYM &&
                        !sym.IsERRORSYM &&
                        Compiler.CompileCallback.CheckForNameSimplification())
                    {
                        if ((sym as TYPESYM).IsPredefined() &&
                            PredefType.InfoTable[(int)(sym as TYPESYM).GetPredefType()].niceName != null)
                        {
                            Compiler.CompileCallback.CanSimplifyNameToPredefinedType(
                                this.errorContextSym.GetInputFile().SourceData, node, sym as TYPESYM);
                        }
                        else
                        {
                            NAMENODE lastName = node.AsANYBINOP.Operand2.AsANYNAME;
                            if (sym == BindSingleNameCore(lastName, lastName.Name, null))
                            {
                                Compiler.CompileCallback.CanSimplifyName(
                                    this.errorContextSym.GetInputFile().SourceData, node.AsDOT);
                            }
                        }
                    }
                    break;

                case NODEKIND.NAME:
                case NODEKIND.OPENNAME:
                case NODEKIND.GENERICNAME:
                    sym = BindSingleNameCore(node.AsANYNAME, node.AsANYNAME.Name, null);
                    if (sym != null &&
                        sym.IsTYPESYM &&
                        !sym.IsERRORSYM &&
                        (sym as TYPESYM).IsPredefined() &&
                        Compiler.CompileCallback.CheckForNameSimplification())
                    {
                        Compiler.CompileCallback.CanSimplifyNameToPredefinedType(
                           this.errorContextSym.GetInputFile().SourceData, node, sym as TYPESYM);
                    }
                    break;

                case NODEKIND.ALIASNAME:
                    sym = BindAliasNameCore(node.AsALIASNAME);
                    break;

                default:
                    DebugUtil.Assert(false, "Bad node kind in BindNameToTypeCore");
                    break;
            }

            DebugUtil.Assert(sym != null || FAllowMissing());
            if (sym == null)
            {
                return null;
            }

            if (sym.IsTYPESYM)
            {
                return (sym as TYPESYM);
            }

            DebugUtil.Assert(sym.IsNSAIDSYM);
            if (!FSuppressErrors())
            {
                Compiler.Error(
                    node,
                    CSCERRID.ERR_BadSKknown,
                    new ErrArg(sym),
                    new ErrArgSymKind(sym),
                    new ErrArg(SYMKIND.AGGSYM));
            }

            // Construct the error sym.
            switch (node.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Unexpected node kind in BindNameToTypeCore");
                    goto case NODEKIND.DOT;

                case NODEKIND.DOT:
                    break;

                case NODEKIND.NAME:
                case NODEKIND.ALIASNAME:
                    return Compiler.MainSymbolManager.GetErrorType(null, node.AsSingleName.Name, null);
            }

            // Construct the parent NSAIDSYM.
            NSAIDSYM nsa = sym as NSAIDSYM;
            DebugUtil.Assert(nsa.NamespaceSym.ParentSym != null);
            NSAIDSYM nsaPar = Compiler.MainSymbolManager.GetNsAid(nsa.NamespaceSym.ParentNsSym, nsa.GetAssemblyID());

            return Compiler.MainSymbolManager.GetErrorType(nsaPar, nsa.NamespaceSym.Name, null);
        }

        //------------------------------------------------------------
        // TypeBind.BindSingleNameCore
        //
        /// <summary>
        /// <para></para>
        /// <para>(In sscli, typeArgs has the default value null.)</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM BindSingleNameCore(
            NAMENODE node,
            string name,
            TypeArray typeArgs) // = null (sscli)
        {
#if DEBUG
            DebugUtil.Assert(isValid);
#endif
            DebugUtil.Assert(node != null);

            SYM sym;

            if (typeArgs == null)
            {
                typeArgs = BindTypeArgsCore(node);
            }
            DebugUtil.Assert(typeArgs != null);
            this.ClearErrorInfo();

            if (this.resolvingUsingAlias)
            {
                // We're binding a using alias.
                DebugUtil.Assert(this.startSym is NSDECLSYM);
                DebugUtil.Assert(this.typeVariablesSym == null);

                NSDECLSYM nsDeclSym = startSym as NSDECLSYM;
                sym = SearchNamespacesCore(nsDeclSym, node, name, typeArgs);
            }
            else
            {
                sym = null;
                SYM searchSym = this.startSym;
                SYM typeValsSym = this.typeVariablesSym;

                // PreLookup method always returns null.
                if ((this.flags & TypeBindFlagsEnum.CallPreHook) != 0 &&
                    (sym = PreLookup(name, typeArgs)) != null)
                {
                    goto LDone;
                }

                // Search the type variables in the method
                if (searchSym is METHSYM)
                {
                    DebugUtil.Assert(typeValsSym == null || typeValsSym == searchSym);

                    typeValsSym = searchSym;
                    searchSym = (searchSym as METHSYM).ContainingAggDeclSym;
                }

                // check for type parameters while resolving base types, where clauses, XML comments, etc.
                // if the sym which has argument name is not AGGSYM and typeArgs is not empty, SearchTypeVarsCore returns null.
                if (typeValsSym != null && (sym = SearchTypeVarsCore(typeValsSym, name, typeArgs)) != null)
                {
                    goto LDone;
                }

                // search class
                if (searchSym is AGGDECLSYM)
                {
                    AGGSYM agg = (searchSym as AGGDECLSYM).AggSym;
                    // Include outer types and type vars.
                    if ((sym = SearchClassCore(agg.GetThisType(), name, typeArgs, true)) != null)
                    {
                        goto LDone;
                    }
                }

                while (!searchSym.IsNSDECLSYM)
                {
                    searchSym = searchSym.ContainingDeclaration();
                }
                sym = SearchNamespacesCore(searchSym as NSDECLSYM, node, name, typeArgs);

                // "var" (CS3) or "dynamic" (CS4)
                if (sym == null)
                {
                    if (this.AllowImplicitType &&
                        name == "var")
                    {
                        sym = Compiler.MainSymbolManager.ImplicitTypeSym;
                    }
                    else if (name == "dynamic")
                    {
                        sym = Compiler.MainSymbolManager.DynamicSym;
                    }
                }
            }

        LDone:
            sym = ReportErrors(node, name, null, typeArgs, sym);
            return sym;
        }

        //------------------------------------------------------------
        // TypeBind.BindAliasNameCore
        //
        /// <summary>
        /// Bind the alias name.
        /// Returns an NSAIDSYM or AGGTYPESYM (the latter only after reporting an error).
        /// Doesn't call ClearErrorInfo or ReportErrors.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM BindAliasNameCore(NAMENODE nameNode)
        {
#if DEBUG
            DebugUtil.Assert(isValid);
#endif
            DebugUtil.Assert(nameNode.Kind == NODEKIND.ALIASNAME && nameNode.Name != null);
            SYM sym = null;
            string name = nameNode.Name;

            if ((nameNode.Flags & NODEFLAGS.GLOBAL_QUALIFIER) != 0)
            {
                sym = compiler.MainSymbolManager.GetGlobalNsAid();
                return sym;
            }

            SYM searchSym = this.startSym;

            while (!searchSym.IsNSDECLSYM)
            {
                searchSym = searchSym.ContainingDeclaration();
            }

            for (NSDECLSYM nsd = searchSym as NSDECLSYM;
                nsd != null;
                nsd = nsd.ParentDeclSym as NSDECLSYM)
            {
                // Check the using aliases.
                compiler.ClsDeclRec.EnsureUsingClausesAreResolved(nsd);

                foreach (SYM useSym in nsd.UsingClauseSymList)
                {
                    if (useSym.Name != name ||
                        !useSym.IsALIASSYM ||
                        !FAliasAvailable(useSym as ALIASSYM))
                    {
                        continue;
                    }

                    sym = BindUsingAlias(compiler, useSym as ALIASSYM);
                    if (sym == null)
                    {
                        continue;
                    }

                    //Found something within this using.  it is not "unused"
                    compiler.CompileCallback.BoundToUsing(nsd, useSym);

                    switch (sym.Kind)
                    {
                        case SYMKIND.ERRORSYM:
                            return sym;

                        case SYMKIND.NSAIDSYM:
                            return sym;

                        case SYMKIND.AGGTYPESYM:
                            // Should have used . not ::
                            if (FSuppressErrors())
                            {
                                return compiler.MainSymbolManager.GetErrorType(null, name, null);
                            }
                            // Can't use type aliases with ::.
                            compiler.ErrorRef(
                                nameNode,
                                CSCERRID.ERR_ColColWithTypeAlias,
                                new ErrArgRef(useSym as ALIASSYM));
                            return sym;

                        default:
                            DebugUtil.VsFail("Alias resolved to bad kind");
                            break;
                    }
                }
            }

            if (FAllowMissing())
            {
                return null;
            }

            if (!FSuppressErrors())
            {
                compiler.Error(nameNode, CSCERRID.ERR_AliasNotFound, new ErrArg(name));
            }

            sym = compiler.MainSymbolManager.GetErrorType(null, name, null);

            return sym;
        }

        //------------------------------------------------------------
        // TypeBind.BindTypeArgsCore
        //
        /// <summary>
        /// <para>Bind the type arguments on a generic name.</para>
        /// <list type="bullet">
        /// <item><term>GENERICNAME:</term>
        /// <description>Return a TypeArray instance whose elements are binded TypeSyms.</description>
        /// </item>
        /// <item><term>OPENNAME:</term>
        /// <description>Return a TypeArray instance whose elements are BSYMMGR.UnitSyms.</description>
        /// </item>
        /// <item><term>Otherwise:</term>
        /// <description>Return BSYMMGR.EmptyTypeArray.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected TypeArray BindTypeArgsCore(NAMENODE nameNode)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif

            TypeArray typeArray = null;
            int count = 0;

            switch (nameNode.Kind)
            {
                case NODEKIND.GENERICNAME:
                    DebugUtil.Assert((nameNode as GENERICNAMENODE).ParametersNode != null);
                    // Turn off AllowMissing when binding type parameters.
                    TypeBindFlagsEnum flagsOrig = this.flags;
                    this.flags &= ~TypeBindFlagsEnum.AllowMissing;

                    typeArray = new TypeArray();
                    BASENODE node = (nameNode as GENERICNAMENODE).ParametersNode;
                    TYPEBASENODE argNode;
                    while (node != null)
                    {
                        if (node.Kind == NODEKIND.LIST)
                        {
                            argNode = node.AsLIST.Operand1.AsTYPEBASE;
                            node = node.AsLIST.Operand2;
                        }
                        else
                        {
                            argNode = node.AsTYPEBASE;
                            node = null;
                        }
                        // bind the typeSym, and wrap it if the variable is byref
                        TYPESYM typeSym = BindTypeCore(argNode);
                        DebugUtil.Assert(typeSym != null);
                        typeArray.Add(typeSym);
                    }

                    this.flags = flagsOrig;
                    break;

                case NODEKIND.OPENNAME:
                    count = (nameNode as OPENNAMENODE).CountOfBlankParameters;
                    DebugUtil.Assert(count > 0);
                    typeArray = new TypeArray();
                    TYPESYM typeUnit = this.Compiler.MainSymbolManager.UnitSym;
                    for (int i = 0; i < count; ++i)
                    {
                        typeArray.Add(typeUnit);
                    }
                    break;

                default:
                    return BSYMMGR.EmptyTypeArray;
            }

            return this.Compiler.MainSymbolManager.AllocParams(typeArray);
        }

        //------------------------------------------------------------
        // TypeBind.BindDottedNameCore
        //
        /// <summary>
        /// Bind a dotted name.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM BindDottedNameCore(BINOPNODE node, string name)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(node != null && node.Kind == NODEKIND.DOT);

            SYM symLeft = BindNameCore(node.Operand1);
            if (symLeft == null)
            {
                return null;
            }
            return BindRightSideCore(symLeft, node, node.Operand2.AsANYNAME, name, null);
        }

        //------------------------------------------------------------
        // TypeBind.BindRightSideCore
        //
        /// <summary>
        /// Bind the right identifier of a dotted name.
        /// </summary>
        /// <param name="symLeft"></param>
        /// <param name="nodePar"></param>
        /// <param name="nodeName"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM BindRightSideCore(
            SYM symLeft,
            BASENODE nodePar,
            NAMENODE nodeName,
            string name,
            TypeArray typeArgs) // = NULL
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            if (typeArgs == null)
            {
                typeArgs = BindTypeArgsCore(nodeName);
            }
            DebugUtil.Assert(typeArgs != null);
            ClearErrorInfo();

            SYM sym = null;

            switch (symLeft.Kind)
            {
                case SYMKIND.ERRORSYM:
                    sym = Compiler.MainSymbolManager.GetErrorType(symLeft as ERRORSYM, name, typeArgs);
                    break;

                case SYMKIND.NUBSYM:
                    symLeft = (symLeft as NUBSYM).GetAggTypeSym();
                    // The parser shouldn't allo T?.x - only Nullable<T>.x
                    // - so we know Nullable exists.
                    DebugUtil.Assert(symLeft != null);
                    DebugUtil.Assert(symLeft.IsAGGTYPESYM);

                    // Fall through.
                    goto case SYMKIND.AGGTYPESYM;

                case SYMKIND.AGGTYPESYM:
                    AGGTYPESYM aggTypeSym = symLeft as AGGTYPESYM;
                    sym = SearchClassCore(aggTypeSym, name, typeArgs, false);   // no outer and no type vars
                    break;

                case SYMKIND.NSAIDSYM:
                    sym = SearchSingleNamespaceCore(symLeft as NSAIDSYM, nodeName, name, typeArgs, false);
                    break;

                case SYMKIND.TYVARSYM:
                    // Can't lookup in a type variable.
                    if (!FSuppressErrors())
                    {
                        Compiler.Error(nodePar, CSCERRID.ERR_LookupInTypeVariable, new ErrArg(symLeft));
                    }
                    // Note: don't fall through and report more errors 
                    sym = Compiler.MainSymbolManager.GetErrorType(symLeft as PARENTSYM, name, typeArgs);
                    return sym;

                default:
                    DebugUtil.Assert(false, "Bad symbol kind in BindRightSideCore");
                    return null;
            }
            sym = ReportErrors(nodeName, name, symLeft, typeArgs, sym);
            return sym;
        }

        //------------------------------------------------------------
        // TypeBind.BindTypeCore
        //
        /// <summary>
        /// <para>Bind a TYPEBASENODE to a type.</para>
        /// <para>Create/Find a TYPESYM instance for a given TYPEBASENODE instance.</para>
        /// </summary>
        /// <param name="typeBaseNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected TYPESYM BindTypeCore(TYPEBASENODE typeBaseNode)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif

            TYPESYM typeSym = null;
            TYPESYM innerTypeSym = null;

            switch (typeBaseNode.Kind)
            {
                case NODEKIND.PREDEFINEDTYPE:
                    // VOID: Compiler.MainSymbolManager.VoidTypeSym
                    // other predefined type:
                    // Get from AGGSYM[] predefSyms field of BSYMMGR.
                    if ((typeBaseNode as PREDEFINEDTYPENODE).Type == PREDEFTYPE.VOID)
                    {
                        typeSym = Compiler.MainSymbolManager.VoidSym;
                    }
                    else
                    {
                        PREDEFTYPE pt = (typeBaseNode as PREDEFINEDTYPENODE).Type;
                        typeSym = Compiler.GetOptPredefType(pt, false);
                        if (typeSym == null)
                        {
                            // If decimal is missing we'll get here. Use the nice name in the error symbol.
                            if (!FSuppressErrors())
                            {
                                Compiler.MainSymbolManager.ReportMissingPredefTypeError(pt);
                            }
                            string name = BSYMMGR.GetNiceName(pt);
                            typeSym = Compiler.MainSymbolManager.GetErrorType(null, name, null);
                        }
                    }
                    break;

                case NODEKIND.ARRAYTYPE:
                    TYPESYM elementTypeSym = BindTypeCore((typeBaseNode as ARRAYTYPENODE).ElementTypeNode);
                    if (elementTypeSym == null)
                    {
                        return null;
                    }
                    if (!FSuppressErrors())
                    {
                        if (elementTypeSym.IsSpecialByRefType())
                        {
                            Compiler.Error(typeBaseNode, CSCERRID.ERR_ArrayElementCantBeRefAny, new ErrArg(elementTypeSym));
                        }
                        if (Compiler.AggStateMax >= AggStateEnum.DefinedMembers)
                        {
                            Compiler.CheckForStaticClass(typeBaseNode, null, elementTypeSym, CSCERRID.ERR_ArrayOfStaticClass);
                        }
                    }

                    int rank = (typeBaseNode as ARRAYTYPENODE).Dimensions;
                    DebugUtil.Assert(rank > 0);
                    typeSym = Compiler.MainSymbolManager.GetArray(elementTypeSym, rank, null);
                    break;

                case NODEKIND.NAMEDTYPE:
                    typeSym = BindNameToTypeCore((typeBaseNode as NAMEDTYPENODE).NameNode);
                    break;

                case NODEKIND.OPENTYPE:
                    typeSym = BindNameToTypeCore(typeBaseNode.AsOPENTYPE.NameNode);
                    DebugUtil.Assert(
                        typeSym == null ||
                        typeSym.IsERRORSYM ||
                        typeSym.IsAGGTYPESYM && (typeSym as AGGTYPESYM).AllTypeArguments.Count > 0);
                    break;

                case NODEKIND.POINTERTYPE:
                    innerTypeSym = BindTypeCore((typeBaseNode as POINTERTYPENODE).ElementTypeNode);
                    if (innerTypeSym == null)
                    {
                        return null;
                    }

                    typeSym = Compiler.MainSymbolManager.GetPtrType(innerTypeSym);
                    if (!innerTypeSym.IsPTRSYM)
                    {
                        Compiler.ClsDeclRec.CheckUnmanaged(typeBaseNode, typeSym);
                    }
                    break;

                case NODEKIND.NULLABLETYPE:
                    innerTypeSym = BindTypeCore((typeBaseNode as NULLABLETYPENODE).ElementTypeNode);
                    if (innerTypeSym == null)
                    {
                        return null;
                    }

                    // Convert to NUBSYM.
                    typeSym = Compiler.MainSymbolManager.GetNubTypeOrError(innerTypeSym);
                    if (typeSym.IsNUBSYM && Compiler.CompilationPhase >= CompilerPhaseEnum.EvalConstants)
                    {
                        CheckConstraints(
                            Compiler,
                            typeBaseNode,
                            (typeSym as NUBSYM).GetAggTypeSym(),
                            CheckConstraintsFlagsEnum.None);
                    }
                    break;

                case NODEKIND.TYPEWITHATTR:
                    Compiler.Error(typeBaseNode, CSCERRID.ERR_AttrOnTypeArg);
                    typeSym = BindTypeCore((typeBaseNode as TYPEWITHATTRNODE).TypeBaseNode);
                    break;

                case NODEKIND.IMPLICITTYPE: // CS3
                    typeSym = Compiler.MainSymbolManager.ImplicitTypeSym;
                    break;

                default:
                    DebugUtil.Assert(false, "BAD type typeBaseNode kind");
                    return null;
            }
            return typeSym;
        }

        //------------------------------------------------------------
        // TypeBind.BindAttributeTypeCore
        //
        /// <summary>
        /// <para>Bind the node as an attribute type.
        /// The resulting type's actual name may have "Attribute" appended to the last identifier.</para>
        /// <para>Find the TYPESYM instance by the name of argument node.</para>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected TYPESYM BindAttributeTypeCore(BASENODE node)
        {
            // ALIASNAME and OPENNAME shouldn't come through here.
            DebugUtil.Assert(
                node.Kind == NODEKIND.DOT ||
                node.Kind == NODEKIND.NAME ||
                node.Kind == NODEKIND.GENERICNAME);
            DebugUtil.Assert(!FSuppressErrors());
            NAMENODE nodeName;

            switch (node.Kind)
            {
                case NODEKIND.DOT:
                    nodeName = node.AsDOT.Operand2 as NAMENODE;
                    break;
                case NODEKIND.NAME:
                    nodeName = node as NAMENODE;
                    break;
                default:
                    DebugUtil.Assert(false, "Bad node kind in BindAttributeTypeCore");
                    return null;
            }

            // lookup both names but don't report errors when doing so
            string shortName = nodeName.Name;
            string longName = AppendAttrSuffix(nodeName);

            // Suppress the errors for the initial lookup.
            TypeBindFlagsEnum flagsOrig = this.flags;
            this.flags = flagsOrig | TypeBindFlagsEnum.AllowMissing;

            SYM shortSym = BindNameCore(node, shortName);
            SYM longSym = (longName != null ? BindNameCore(node, longName) : null);

            // Reset the SuppressErrors bit.
            this.flags = flagsOrig;

            if (FAllowMissing() && shortSym == null && longSym == null)
            {
                return null;
            }

            // Check results.
            bool isShort = IsAttributeType(shortSym);
            bool isLong = IsAttributeType(longSym);
            TYPESYM returnTypeSym;

            if (isShort == isLong)
            {
                if (isShort)
                {
                    Compiler.ErrorRef(
                        nodeName,
                        CSCERRID.ERR_AmbigousAttribute,
                        new ErrArgRef(shortName),
                        new ErrArgRef(shortSym),
                        new ErrArgRef(longSym));
                    // Use the short one.
                    returnTypeSym = shortSym as AGGTYPESYM;
                    goto LDone;
                }

                if (shortSym != null && (shortSym is ERRORSYM))
                {
                    returnTypeSym = shortSym as ERRORSYM;
                    goto LDone;
                }
                if (longSym != null && (longSym is ERRORSYM))
                {
                    returnTypeSym = longSym as ERRORSYM;
                    goto LDone;
                }

                // Need to generate at least one error.
                if (shortSym == null && longSym == null)
                {
                    BindNameCore(node, shortName); // Generate missing error.
                }
                else
                {
                    if (shortSym != null)
                    {
                        Compiler.ErrorRef(nodeName, CSCERRID.ERR_NotAnAttributeClass, new ErrArgRef(shortSym));
                    }
                    if (longSym != null)
                    {
                        Compiler.ErrorRef(nodeName, CSCERRID.ERR_NotAnAttributeClass, new ErrArgRef(longSym));
                    }
                }
                returnTypeSym = Compiler.MainSymbolManager.GetErrorType(null, shortName, null);
            }
            else
            {
                returnTypeSym = isShort ? (shortSym as AGGTYPESYM) : (longSym as AGGTYPESYM);
            }

        LDone:
            return returnTypeSym;
        }

        //------------------------------------------------------------
        // TypeBind.SearchClassCore
        //
        /// <summary>
        /// <para>These are instance methods to perform the indicated search.
        /// They don't call ClearErrorInfo or ReportErrors.
        /// They update the error info (m_symInaccess, etc).</para>
        /// <para>Search a class for the name (as a nested type).</para>
        /// <para>If fOuterAndTypeVars is true, this searches first for type variables,
        /// then for nested types in this class and its base classes,
        /// then for nested types in outer types and base classes of the outer types.</para>
        /// <para>If fOuterAndTypeVars is false,
        /// this searches for nested types just in this type and its base classes.</para>
        /// </summary>
        /// <param name="ats"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <param name="fOuterAndTypeVars"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM SearchClassCore(
            AGGTYPESYM aggTypeSym,
            string name,
            TypeArray typeArgs,
            bool outerAndTypeVars)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(typeArgs != null);

            // Check this class and all classes that this class is nested in.
            for (; aggTypeSym != null; aggTypeSym = aggTypeSym.OuterTypeSym)
            {
                AGGSYM aggSym = aggTypeSym.GetAggregate();

                if (aggSym.AggState < AggStateEnum.Prepared)
                {
                    if (Compiler.AggStateMax >= AggStateEnum.Prepared)
                    {
                        Compiler.EnsureState(aggSym, AggStateEnum.Prepared);
                    }
                    else if (aggSym.AggState < AggStateEnum.Inheritance)
                    {
                        // Note: we cannot DebugUtil.Assert that this succeeds.
                        // If it fails, we're in ResolveInheritanceRec
                        // and it will be detected by ResolveInheritanceRec (eventually).
                        Compiler.ClsDeclRec.ResolveInheritanceRec(aggSym);
                    }
                }

                if (outerAndTypeVars)
                {
                    SYM sym = SearchTypeVarsCore(aggTypeSym.GetAggregate(), name, typeArgs);
                    if (sym != null)
                    {
                        return sym;
                    }
                }

                // Check this class and all base classes.
                for (AGGTYPESYM curAts = aggTypeSym; curAts != null; curAts = curAts.GetBaseClass())
                {
                    AGGSYM curAgg = curAts.GetAggregate();
                    for (SYM sym = Compiler.MainSymbolManager.LookupAggMember(name, curAgg, SYMBMASK.ALL);
                        sym != null;
                        sym = sym.NextSameNameSym)
                    {
                        if (sym is AGGSYM)
                        {
                            // Arity must be checked before access!
                            SYM retSym = sym;
                            CheckArity(ref retSym, typeArgs, curAts);
                            CheckAccess(ref retSym);
                            if (retSym != null)
                            {
                                return retSym;
                            }
                        }
                        else if (this.badKindSym == null && !(sym is TYVARSYM))
                        {
                            badKindSym = sym;
                        }
                    }
                }
                if (!outerAndTypeVars) return null;
            }
            return null;
        }

        //------------------------------------------------------------
        // TypeBind.SearchTypeVarsCore
        //
        /// <summary>
        /// <para>Searches for the name among the type variables of symOwner.</para>
        /// </summary>
        //------------------------------------------------------------
        internal TYVARSYM SearchTypeVarsCore(
            SYM ownerSym,
            string name,
            TypeArray typeArgs)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(ownerSym.IsMETHSYM || ownerSym.IsAGGSYM);
            DebugUtil.Assert(typeArgs != null);

            SYM tvSym = Compiler.LookupGlobalSym(name, ownerSym as PARENTSYM, SYMBMASK.TYVARSYM) as TYVARSYM;
            if (tvSym != null)
            {
                CheckArity(ref tvSym, typeArgs, null);
                // If typeArgs is not empty, CheckArity method assigns tvSym null.
            }
            return (tvSym as TYVARSYM);
        }

        //------------------------------------------------------------
        // TypeBind.SearchSingleNamespaceCore (1)
        //
        /// <summary>
        /// Searches for the name among the members of a single (filtered) namespace.
        /// Returns either an NSAIDSYM, AGGTYPESYM or ERRORSYM.
        /// </summary>
        /// <param name="nsaidSym"></param>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <param name="onlyAggSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM SearchSingleNamespaceCore(
            NSAIDSYM nsaidSym,
            BASENODE node,
            string name,
            TypeArray typeArgs,
            bool onlyAggSym)  // = false
        {
            SYM sym = SearchSingleNamespaceCore(
                nsaidSym.NamespaceSym,
                nsaidSym.GetAssemblyID(),
                node,
                name,
                typeArgs,
                onlyAggSym);

            if (sym == null || sym.IsERRORSYM)
            {
                return sym;
            }

            // if sym is NSSYM, create a NSAIDSYM instance and return it.
            if (sym.IsNSSYM)
            {
                return Compiler.MainSymbolManager.GetNsAid(sym as NSSYM, nsaidSym.GetAssemblyID());
            }

            // Translate "Nullable<T>" to "T?".
            if ((sym as AGGTYPESYM).IsPredefType(PREDEFTYPE.G_OPTIONAL) &&
                !(sym as AGGTYPESYM).AllTypeArguments[0].IsUNITSYM)
            {
                return Compiler.MainSymbolManager.GetNubFromNullable(sym as AGGTYPESYM);
            }
            return sym as AGGTYPESYM;
        }

        //------------------------------------------------------------
        // TypeBind.SearchSingleNamespaceCore (2)
        //
        /// <summary>
        /// <para>Searches for the name among the members of a single (filtered) namespace.
        /// Returns either an NSSYM (not NSAIDSYM) or AGGTYPESYM.</para>
        /// </summary>
        /// <param name="nsSym"></param>
        /// <param name="assemblyId"></param>
        /// <param name="node"></param>
        /// <param name="typeArgs"></param>
        /// <param name="name"></param>
        /// <param name="onlyAggSym"></param>
        //------------------------------------------------------------
        internal SYM SearchSingleNamespaceCore(
            NSSYM nsSym,
            int assemblyId,
            BASENODE node,
            string name,
            TypeArray typeArgs,
            bool onlyAggSym) // = false
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(typeArgs != null);

            SYMBMASK mask = onlyAggSym ? SYMBMASK.AGGSYM : SYMBMASK.NSSYM | SYMBMASK.AGGSYM;

            SYM bestSym = null;
            KRankEnum bestRank = KRankEnum.None;
            SYM ambigSym = null;
            KRankEnum ambigRank = KRankEnum.None;
            SYM sym = null;
            BAGSYM bagSym = null;

            //--------------------------------------------------
            // Enumerate all SYM instances in nsSym
            // which has the specified name, assembly ID, kind.
            //--------------------------------------------------
            for (bagSym = Compiler.LookupInBagAid(name, nsSym, typeArgs.Count, assemblyId, mask) as BAGSYM;
                bagSym != null;
                bagSym = Compiler.LookupNextInAid(bagSym, assemblyId, mask) as BAGSYM)
            {
                SYM newSym = bagSym;

                // Arity must be checked before access!
                CheckArity(ref newSym, typeArgs, null);
                CheckAccess(ref newSym);

                if (newSym == null)
                {
                    continue;
                }

                DebugUtil.Assert((newSym is NSSYM) || (newSym is AGGTYPESYM));

                // If it's ambiguous, at least one of them should be a type.
                DebugUtil.Assert(bestSym == null || (bestSym is AGGTYPESYM) || (newSym is AGGTYPESYM));
                KRankEnum newRank = RankSym(newSym);

                if (newRank > bestRank)
                {
                    ambigSym = bestSym;
                    ambigRank = bestRank;
                    bestSym = newSym;
                    bestRank = newRank;
                }
                else if (newRank > ambigRank)
                {
                    ambigSym = newSym;
                    ambigRank = newRank;
                }
            } // for (bagSym = Compiler.LookupInBagAid(name, nsSym, typeArgs.Count, assemblyId, mask) as BAGSYM;
            DebugUtil.Assert(bestRank >= ambigRank);

            //--------------------------------------------------
            // If found only one sym, return it.
            // Or If not found, return null.
            //--------------------------------------------------
            if (ambigSym == null)
            {
                return bestSym;
            }
            DebugUtil.Assert(bestSym != null && bestRank == RankSym(bestSym));
            DebugUtil.Assert(ambigSym != null && ambigRank == RankSym(ambigSym));

            // Handle errors and warnings.
            // NOTE: Representing (bestRank,ambigRank) as a pair (x,y), the following should be true:
            // * If (x,y) is a warning then (z,y) must be a warning for any z >= x.
            // * If (x,y) is a warning then (x,z) must be a warning for any z <= y.

            CSCERRID err;

            switch (ambigRank)
            {
                case KRankEnum.TypeImp:
                    if (FSuppressErrors())
                    {
                        if (bestRank >= KRankEnum.NamespaceThis)
                        {
                            return bestSym;
                        }
                        //goto LRetErrorSym;
                        NSAIDSYM nsa = Compiler.MainSymbolManager.GetNsAid(nsSym, assemblyId);
                        return Compiler.MainSymbolManager.GetErrorType(nsa, name, typeArgs);
                    }

                    switch (bestRank)
                    {
                        case KRankEnum.TypeImp:
                            err = CSCERRID.ERR_SameFullNameAggAgg;
                            break;

                        case KRankEnum.NamespaceImp:
                            err = CSCERRID.ERR_SameFullNameNsAgg;
                            break;

                        case KRankEnum.NamespaceThis:
                            err = CSCERRID.WRN_SameFullNameThisNsAgg;
                            break;

                        case KRankEnum.TypeThis:
                            err = CSCERRID.WRN_SameFullNameThisAggAgg;
                            break;

                        default:
                            DebugUtil.Assert(false, "Bad rank");
                            err = CSCERRID.ERR_SameFullNameAggAgg;
                            break;
                    }
                    break;

                case KRankEnum.NamespaceImp:
                    DebugUtil.Assert(bestRank == KRankEnum.TypeThis);
                    if (FSuppressErrors())
                    {
                        return bestSym;
                    }
                    err = CSCERRID.WRN_SameFullNameThisAggNs;
                    break;

                case KRankEnum.NamespaceThis:
                    DebugUtil.Assert(bestRank == KRankEnum.TypeThis);
                    if (FSuppressErrors())
                    {
                        //goto LRetErrorSym;
                        NSAIDSYM nsa = Compiler.MainSymbolManager.GetNsAid(nsSym, assemblyId);
                        return Compiler.MainSymbolManager.GetErrorType(nsa, name, typeArgs);
                    }
                    err = CSCERRID.ERR_SameFullNameThisAggThisNs;
                    break;

                case KRankEnum.TypeThis:
                    DebugUtil.Assert(bestRank == KRankEnum.TypeThis);
                    if (FSuppressErrors())
                    {
                        //LRetErrorSym:
                        NSAIDSYM nsa = Compiler.MainSymbolManager.GetNsAid(nsSym, assemblyId);
                        return Compiler.MainSymbolManager.GetErrorType(nsa, name, typeArgs);
                    }
                    Compiler.ErrorRef(node, CSCERRID.ERR_SameFullNameThisAggThisAgg,
                       new ErrArgAggKind(bestSym as AGGTYPESYM),
                       new ErrArgRef(bestSym),
                       new ErrArgAggKind(ambigSym as AGGTYPESYM),
                       new ErrArgRef(ambigSym));
                    return bestSym;

                default:
                    DebugUtil.Assert(false, "Bad rank");
                    err = CSCERRID.ERR_SameFullNameAggAgg;
                    break;
            }

            DECLSYM bestDeclSym;
            DECLSYM ambigDeclSym;

            if (bestSym.IsAGGTYPESYM)
            {
                bestDeclSym = ((bestSym as AGGTYPESYM).GetAggregate().FirstDeclSym) as DECLSYM;
            }
            else
            {
                bestDeclSym = (bestSym as NSSYM).FirstDeclSym;
                if (bestRank == KRankEnum.NamespaceThis)
                {
                    while (!bestDeclSym.GetInputFile().InAlias(Kaid.ThisAssembly) && (bestDeclSym.NextDeclSym != null))
                    {
                        bestDeclSym = bestDeclSym.NextDeclSym;
                    }
                }
            }

            if (ambigSym.IsAGGTYPESYM)
            {
                ambigDeclSym = ((ambigSym as AGGTYPESYM).GetAggregate().FirstDeclSym) as DECLSYM;
            }
            else
            {
                ambigDeclSym = (ambigSym as NSSYM).FirstDeclSym;
                if (ambigRank == KRankEnum.NamespaceThis)
                {
                    while (!ambigDeclSym.GetInputFile().InAlias(Kaid.ThisAssembly) && (ambigDeclSym.NextDeclSym != null))
                    {
                        ambigDeclSym = ambigDeclSym.NextDeclSym;
                    }
                }
            }

            Compiler.ErrorRef(
                node,
                err,
                new ErrArgRef(bestDeclSym.GetInputFile().Name),
                new ErrArgRef(bestDeclSym),
                new ErrArgRef(ambigDeclSym.GetInputFile().Name),
                new ErrArgRef(ambigDeclSym));

            return bestSym;
        }

        //------------------------------------------------------------
        // TypeBind.SearchNamespacesCore
        //
        /// <summary>
        /// <para>Searches for the name in a namespace declaration and containing declarations.
        /// This searches using clauses as well.</para>
        /// <para>Find a SYM instance with the specified name in the given namespace.
        /// If not found, search in the parent namespace, and so on.</para>
        /// </summary>
        /// <param name="nsDeclSym"></param>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM SearchNamespacesCore(
            NSDECLSYM nsDeclSym,
            BASENODE node,
            string name,
            TypeArray typeArgs)
        {
#if DEBUG
            DebugUtil.Assert(isValid);
#endif
            DebugUtil.Assert(typeArgs != null);

            for (; nsDeclSym != null; nsDeclSym = nsDeclSym.ParentNsDeclSym)
            {
                SYM sym;

                // Check the using aliases and extern aliases.
                sym = SearchUsingAliasesCore(nsDeclSym, node, name, typeArgs);
                if (sym != null)
                {
                    return sym;
                }

                // Check the namespace.
                sym = SearchSingleNamespaceCore(
                    Compiler.MainSymbolManager.GetNsAid(nsDeclSym.NamespaceSym, Kaid.Global),
                    node,
                    name,
                    typeArgs,
                    false);
                if (sym != null)
                {
                    return sym;
                }

                // Check the using clauses for this declaration.
                sym = SearchUsingClausesCore(nsDeclSym, node, name, typeArgs);
                if (sym != null)
                {
                    return sym;
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // TypeBind.SearchNamespacesForAliasCore
        //
        // Searches for an alias with the given name in a namespace declaration and containing
        // declarations. Reports an error if not found.
        //------------------------------------------------------------
        //SYM * SearchNamespacesForAliasCore(NSDECLSYM * nsd, BASENODE * node, NAME * name);
        //SYM * TypeBind::SearchNamespacesForAliasCore(NSDECLSYM * nsd, BASENODE * node, NAME * name)
        //{
        //    ASSERT(m_fValid);
        //    ASSERT(!m_fUsingAlias); // This doesn't check for extern vs regular.
        //
        //    for ( ; nsd; nsd = nsd->DeclPar()) {
        //        // Check the using aliases.
        //        compiler()->clsDeclRec.ensureUsingClausesAreResolved(nsd);
        //
        //        FOREACHSYMLIST(nsd->usingClauses, symUse, SYM)
        //            if (symUse->name != name || !symUse->IsALIASSYM)
        //                continue;
        //
        //            SYM * sym = BindUsingAlias(compiler(), symUse->AsALIASSYM);
        //            if (sym) {
        //
        //                //Found something within this using.  it is not "unused"
        //                compiler()->compileCallback.BoundToUsing(nsd, symUse);
        //
        //                return sym;
        //            }
        //        ENDFOREACHSYMLIST
        //    }
        //
        //    if (FAllowMissing())
        //        return NULL;
        //
        //    if (!FSuppressErrors())
        //        compiler()->Error(node, ERR_AliasNotFound, name);
        //
        //    return compiler()->getBSymmgr().GetErrorType(NULL, name, NULL);
        //}

        //------------------------------------------------------------
        // TypeBind.SearchUsingAliasesCore
        //
        /// <summary>
        /// <para>Searches for the name in the using aliases of the given namespace declaration.</para>
        /// <para>If there is an ambiguity and SuppressErrors is set, this returns ERRORSYM.</para>
        /// <para>If there is an ambiguity and SuppressErrors is not set,
        /// this reports an error and returns the best symbol found.</para>
        /// </summary>
        /// <param name="nsDeclSym"></param>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
       //------------------------------------------------------------
        protected SYM SearchUsingAliasesCore(
            NSDECLSYM nsDeclSym,
            BASENODE node,
            string name,
            TypeArray typeArgs)
        {
#if DEBUG
            DebugUtil.Assert(isValid);
#endif
            DebugUtil.Assert(typeArgs != null);
            DebugUtil.Assert(!this.resolvingUsingAlias || this.startSym.IsNSDECLSYM);

            if (typeArgs.Count > 0 && this.badAritySym != null)
            {
                return null;
            }

            Compiler.ClsDeclRec.EnsureUsingClausesAreResolved(nsDeclSym);

            foreach (SYM usingSym in nsDeclSym.UsingClauseSymList)
            {
                if (usingSym.Name != name ||
                    !usingSym.IsALIASSYM ||
                    !FAliasAvailable(usingSym as ALIASSYM))
                {
                    continue;
                }

                ALIASSYM alias = usingSym as ALIASSYM;
                SYM sym = BindUsingAlias(this.Compiler, alias);
                if (sym == null)
                {
                    continue;
                }

                if (typeArgs.Count > 0)
                {
                    this.badAritySym = alias;
                    return null;
                }

                //Found something within this using.  it is not "unused"
                Compiler.CompileCallback.BoundToUsing(nsDeclSym, usingSym);

                if (alias.DuplicatedSym != null)
                {
                    // Error - alias conflicts with other element of the namespace.
                    if (FSuppressErrors())
                        return Compiler.MainSymbolManager.GetErrorType(null, name, null);
                    Compiler.Error(
                        node,
                        CSCERRID.ERR_ConflictAliasAndMember,
                        new ErrArg(name),
                        new ErrArg(alias.DuplicatedSym.ParentSym),
                        new ErrArgRefOnly(alias.DuplicatedSym));
                }
                return sym;
            }
            return null;
        }

        //------------------------------------------------------------
        // TypeBind.SearchUsingClausesCore
        //
        /// <summary>
        /// <para>Searches for the name in the using clauses of the given namespace declaration.
        /// Checks only normal using clauses - not aliases.</para>
        /// <para>If there is an ambiguity and SuppressErrors is set, this returns ERRORSYM.
        /// If there is an ambiguity and SuppressErrors is not set,
        /// this reports an error and returns the best symbol found.</para>
        /// </summary>
        /// <param name="nsd"></param>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected SYM SearchUsingClausesCore(
            NSDECLSYM nsd,
            BASENODE node,
            string name,
            TypeArray typeArgs)
        {
#if DEBUG
            DebugUtil.Assert(this.isValid);
#endif
            DebugUtil.Assert(typeArgs != null);
            DebugUtil.Assert(!this.resolvingUsingAlias || this.startSym.IsNSDECLSYM);
        
            // Don't check using namespaces when binding aliases in this decl.
            if (this.resolvingUsingAlias && this.startSym == nsd)
            {
                return null;
            }
        
            Compiler.ClsDeclRec.EnsureUsingClausesAreResolved(nsd);
        
            SYM  returnSym = null;

            foreach (SYM usingSym in nsd.UsingClauseSymList)
            {
                if (!usingSym.IsNSAIDSYM)
                {
                    continue;
                }

                // Only search for types.
                SYM sym = SearchSingleNamespaceCore(usingSym as NSAIDSYM, node, name, typeArgs, true);
                if (sym == null)
                {
                    continue;
                }

                //Found something within this using.  it is not "unused"
                Compiler.CompileCallback.BoundToUsing(nsd, usingSym);

                if (sym.IsERRORSYM)
                {
                    return sym;
                }

                // Check for ambiguity between different using namespaces.
                if (returnSym != null)
                {
                    if (FSuppressErrors())
                    {
                        return Compiler.MainSymbolManager.GetErrorType(null, name, null);
                    }

                    // After reporting the error just run with the first one.
                    Compiler.ErrorRef(
                        node,
                        CSCERRID.ERR_AmbigContext,
                        new ErrArgRef(name),
                        new ErrArgRef(returnSym),
                        new ErrArgRef(sym)
                        );
                    return returnSym;
                }
                returnSym = sym;
            }
            return returnSym;
        }

        //------------------------------------------------------------
        // TypeBind.FAliasAvailable
        //
        /// <summary>
        /// <para>Used to determine whether a using alias is currently available.</para>
        /// <para>This depends on whether m_fUsingAlias is set (indicating that we're currently resolving an alias),
        /// whether the alias is extern, and whether alias->parent == m_symStart.</para>
        /// </summary>
        /// <param name="aliasSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool FAliasAvailable(ALIASSYM aliasSym)
        {
            DebugUtil.Assert(!this.resolvingUsingAlias || this.startSym.IsNSDECLSYM);
            return (!this.resolvingUsingAlias || aliasSym.IsExtern || aliasSym.ParentSym != this.startSym);
        }

        //------------------------------------------------------------
        // TypeBind.CheckConstraintsCore
        //
        /// <summary>
        /// Check whether typeArgs satisfies the constraints of typeVars.
        /// The typeArgsCls and typeArgsMeth are used for substitution on the bounds.
        /// The tree and symErr are used for error reporting.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="treeNode"></param>
        /// <param name="errorSym"></param>
        /// <param name="typeVariables"></param>
        /// <param name="typeArguments"></param>
        /// <param name="classTypeArguments"></param>
        /// <param name="methodTypeArguments"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected static bool CheckConstraintsCore(
            COMPILER compiler,
            BASENODE treeNode,
            SYM errorSym,
            TypeArray typeVariables,
            TypeArray typeArguments,
            TypeArray classTypeArguments,
            TypeArray methodTypeArguments,
            CheckConstraintsFlagsEnum flags)
        {
            DebugUtil.Assert(typeVariables.Count == typeArguments.Count);
            DebugUtil.Assert(typeVariables.Count > 0);
            DebugUtil.Assert(typeVariables.AggState >= AggStateEnum.DefinedMembers);
            DebugUtil.Assert(typeArguments.AggState >= AggStateEnum.DefinedMembers);
            DebugUtil.Assert(
                flags == CheckConstraintsFlagsEnum.None ||
                flags == CheckConstraintsFlagsEnum.NoErrors);

            bool reportErrors = ((flags & CheckConstraintsFlagsEnum.NoErrors) == 0);
            bool isError = false;

            for (int i = 0; i < typeVariables.Count; ++i)
            {
                // Empty bounds should be set to object.
                TYVARSYM tvSym = typeVariables.ItemAsTYVARSYM(i);
                DebugUtil.Assert(tvSym.FResolved());

                TYPESYM argTypeSym = typeArguments[i];

                if (argTypeSym.IsUNITSYM)
                {
                    continue;
                }

                if (argTypeSym.IsERRORSYM)
                {
                    // Error should have been reported previously.
                    isError = true;
                    continue;
                }

                if (compiler.CheckBogus(argTypeSym))
                {
                    if (reportErrors)
                    {
                        compiler.ErrorRef(
                            treeNode,
                            CSCERRID.ERR_BogusType,
                            new ErrArgRef(argTypeSym));
                    }
                    isError = true;
                    continue;
                }

                if (argTypeSym.IsPTRSYM || argTypeSym.IsSpecialByRefType())
                {
                    if (reportErrors)
                    {
                        compiler.Error(
                            treeNode,
                            CSCERRID.ERR_BadTypeArgument,
                            new ErrArgRef(argTypeSym));
                    }
                    isError = true;
                    continue;
                }

                if (argTypeSym.IsStaticClass())
                {
                    if (reportErrors)
                    {
                        compiler.ReportStaticClassError(
                            treeNode,
                            null,
                            argTypeSym,
                            CSCERRID.ERR_GenericArgIsStaticClass);
                    }
                    isError = true;
                    continue;
                }

                if (tvSym.HasReferenceConstraint() && !argTypeSym.IsReferenceType())
                {
                    if (reportErrors)
                    {
                        compiler.ErrorRef(
                            treeNode,
                            CSCERRID.ERR_RefConstraintNotSatisfied,
                            new ErrArgRef(errorSym),
                            new ErrArgNoRef(tvSym),
                            new ErrArgRef(argTypeSym));
                    }
                    isError = true;
                }

                TypeArray boundArray = compiler.MainSymbolManager.SubstTypeArray(
                    tvSym.BoundArray,
                    classTypeArguments,
                    methodTypeArguments,
                    SubstTypeFlagsEnum.NormNone);
                int itypeMin = 0;

                if (tvSym.HasValueConstraint())
                {
                    if (!argTypeSym.IsValueType() || argTypeSym.IsNUBSYM)
                    {
                        if (reportErrors)
                        {
                            compiler.ErrorRef(
                                treeNode,
                                CSCERRID.ERR_ValConstraintNotSatisfied,
                                new ErrArgRef(errorSym),
                                new ErrArgNoRef(tvSym),
                                new ErrArgRef(argTypeSym));
                        }
                        isError = true;
                    }

                    // Since FValCon() is set it is redundant to check System.ValueType as well.
                    if (boundArray.Count > 0 && boundArray[0].IsPredefType(PREDEFTYPE.VALUE))
                    {
                        itypeMin = 1;
                    }
                }

                for (int j = itypeMin; j < boundArray.Count; j++)
                {
                    TYPESYM typeBnd = boundArray[j];
                    if (!SatisfiesBound(compiler, argTypeSym, typeBnd))
                    {
                        if (reportErrors)
                        {
                            compiler.Error(
                                treeNode,
                                CSCERRID.ERR_GenericConstraintNotSatisfied,
                                new ErrArgRef(errorSym),
                                new ErrArg(typeBnd, ErrArgFlagsEnum.Unique),
                                new ErrArg(tvSym),
                                new ErrArgRef(argTypeSym, ErrArgFlagsEnum.Unique));
                        }
                        isError = true;
                    }
                }

                // Check the newable constraint.
                if (!tvSym.HasNewConstraint() || argTypeSym.IsValueType())
                {
                    continue;
                }

                if (argTypeSym.IsClassType())
                {
                    AGGSYM agg = (argTypeSym as AGGTYPESYM).GetAggregate();
                    if (agg.HasPubNoArgCtor && !agg.IsAbstract)
                    {
                        continue;
                    }
                }
                else if (argTypeSym.IsTYVARSYM && (argTypeSym as TYVARSYM).HasNewConstraint())
                {
                    continue;
                }

                if (reportErrors)
                {
                    compiler.ErrorRef(
                        treeNode,
                        CSCERRID.ERR_NewConstraintNotSatisfied,
                        new ErrArgRef(errorSym),
                        new ErrArgNoRef(tvSym),
                        new ErrArgRef(argTypeSym));
                }
                isError = true;
            }

            return !isError;
        }

        //------------------------------------------------------------
        // TypeBind.SatisfiesBound (static)
        //
        /// <summary>
        /// Determine whether the arg type satisfies the typeBnd constraint.
        /// Note that typeBnd could be just about any type
        /// (since we added naked type parameter constraints).
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="argTypeSym"></param>
        /// <param name="boundTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static protected bool SatisfiesBound(
            COMPILER compiler,
            TYPESYM argTypeSym,
            TYPESYM boundTypeSym)
        {
            if (boundTypeSym == argTypeSym)
            {
                return true;
            }

            switch (boundTypeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false);
                    return false;

                case SYMKIND.PTRSYM:
                case SYMKIND.ERRORSYM:
                    return false;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.TYVARSYM:
                    break;

                case SYMKIND.NUBSYM:
                    boundTypeSym = (boundTypeSym as NUBSYM).GetAggTypeSym();
                    if (boundTypeSym == null)
                    {
                        return true;
                    }
                    break;

                case SYMKIND.AGGTYPESYM:
                    break;
            }

            DebugUtil.Assert(
                boundTypeSym.IsAGGTYPESYM ||
                boundTypeSym.IsTYVARSYM ||
                boundTypeSym.IsARRAYSYM);

            switch (argTypeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false);
                    return false;

                case SYMKIND.ERRORSYM:
                case SYMKIND.PTRSYM:
                    return false;

                case SYMKIND.NUBSYM:
                    argTypeSym = (argTypeSym as NUBSYM).GetAggTypeSym();
                    if (argTypeSym == null)
                    {
                        return true;
                    }
                    // Fall through.
                    goto case SYMKIND.AGGTYPESYM;

                case SYMKIND.TYVARSYM:
                case SYMKIND.ARRAYSYM: // IsBaseType handles IList<T>....
                case SYMKIND.AGGTYPESYM:
                    return compiler.IsBaseType(argTypeSym, boundTypeSym);
            }
        }

        // Misc. utililites.

        //------------------------------------------------------------
        // TypeBind.AppendAttrSuffix
        //
        /// <summary>
        /// Return the identifier in node concatenated with "Attribute".
        /// May return NULL indicating that appending Attribute is not appropriate
        /// (because the identifier was specified with the literal prefix @).
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected string AppendAttrSuffix(NAMENODE node)
        {
            if ((node.Flags & NODEFLAGS.NAME_LITERAL) != 0)
            {
                return null;
            }

            string name = String.Concat(node.Name, "Attribute");
            Compiler.NameManager.AddString(name);
            return name;
        }

        //------------------------------------------------------------
        // TypeBind.IsAttributeType
        //
        /// <summary>
        /// Determines whether the sym is an attribute type.
        /// This calls EnsureState before checking.
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool IsAttributeType(SYM sym)
        {
            if (sym == null || !sym.IsAGGTYPESYM)
            {
                return false;
            }
            AGGSYM agg = (sym as AGGTYPESYM).GetAggregate();
            Compiler.EnsureState(agg, AggStateEnum.Prepared);
            return agg.IsAttribute;
        }

        //------------------------------------------------------------
        // TypeBind.RankSym
        //
        /// <summary>
        /// <para>Private method for SearchSingleNamespaceCore.
        /// Determines what kind of SYM we're dealing with for ambiguity error / warning reporting.
        /// Returns a value from the enum above.
        /// Note that this is only called on NSSYMs and AGGTYPESYMs where the AGGSYM is NOT nested.</para>
        /// <para>Return a KRankEnum value which represents the kind of argument sym and
        /// where the sym is defined.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected KRankEnum RankSym(SYM sym)
        {
            DebugUtil.Assert(sym.IsNSSYM || sym.IsAGGTYPESYM);

            if (sym.IsNSSYM)
            {
                if ((sym as NSSYM).InAlias(Kaid.ThisAssembly))
                {
                    return KRankEnum.NamespaceThis;
                }
                return KRankEnum.NamespaceImp;
            }

            AGGSYM aggSym = (sym as AGGTYPESYM).GetAggregate();
            DebugUtil.Assert(!aggSym.IsNested);

            if (aggSym.IsSource || aggSym.InAlias(Kaid.ThisAssembly))
            {
                return KRankEnum.TypeThis;
            }

            // CheckAccess should have dealt with private types already.
            DebugUtil.Assert(
                aggSym.Access > ACCESS.INTERNAL ||
                aggSym.Access == ACCESS.INTERNAL && aggSym.InternalsVisibleTo(Kaid.ThisAssembly));
            return KRankEnum.TypeImp;
        }

        //------------------------------------------------------------
        // TypeBind.PreLookup
        //
        /// <summary>
        /// <para>(sscli) Only called if the CallPreHook flag is set.
        /// Called by BindSingleNameCore before any other lookup is performed.</para>
        /// <para>return null for now.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual protected SYM PreLookup(string name, TypeArray typeArgs)
        {
            return null;
        }
    }
}
