//============================================================================
// InferTypeArguments.cs
//
// 2016/04/24 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Uncs
{
    //======================================================================
    // class BSYMMGR
    //======================================================================
    internal partial class BSYMMGR
    {
        //------------------------------------------------------------
        // BSYMMGR.InferTypes
        //
        /// <summary>
        /// <para>(sscli)If typeDst doesn't contain method type variables this just returns true.</para>
        /// <para>Infer the types assigned to the type variables in dstTypeSym
        /// by the type arguments of srcTypeSym.</para>
        /// </summary>
        /// <param name="srcTypeSym">The type of the argument.</param>
        /// <param name="dstTypeSym">The type of the definition.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InferTypes(TYPESYM srcTypeSym, TYPESYM dstTypeSym, InferContext context)
        {
            // NOTE: We substitute class type variables in dstTypeSym with the types in context.typeArgsCls.

            // NOTE: We can't short circuit and test for srcTypeSym == dstTypeSym here since
            // if there are method type variables in dstTypeSym, we still need to map those to
            // themselves. This is so code like "void F<T>(T t) { F(t); }" will properly
            // infer that the type arg for the recursive call is <T>.

            // At the top level, the inference succeeds
            // if dstTypeSym doesn't contain any method type variables.
            // This is not true at recursive levels (handled by InferTypesEqual).

            if (!TypeContainsTyVars(dstTypeSym, context.MethodTypeVariables))
            {
                return true;
            }

            // Strip out / ref.
            if (dstTypeSym.Kind == SYMKIND.PARAMMODSYM)
            {
                dstTypeSym = (dstTypeSym as PARAMMODSYM).ParamTypeSym;
            }

        LAgain:
            // Check for null or bad source types.
            switch (srcTypeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad src SYM kind in InferTypes");
                    return false;

                case SYMKIND.PTRSYM:
                    // Can't bind type variables to these.
                    return false;

                case SYMKIND.PARAMMODSYM:
                    srcTypeSym = (srcTypeSym as PARAMMODSYM).ParamTypeSym;
                    goto LAgain;

                case SYMKIND.METHGRPSYM:
                case SYMKIND.ANONMETHSYM:
                case SYMKIND.NULLSYM:
                case SYMKIND.LAMBDAEXPRSYM:
                    // We don't want to prohibit inference. We just can't bind a type variable
                    // to these types. If the type variable is bound to a type through another
                    // parameter, verifyArgs() will take care of the conversion.
                    return true;

                case SYMKIND.ERRORSYM:
                case SYMKIND.ARRAYSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.AGGTYPESYM:
                case SYMKIND.TYVARSYM:
                    break;
            }

        LRecurse:
            DebugUtil.Assert(
                srcTypeSym.IsAGGTYPESYM ||
                srcTypeSym.IsARRAYSYM ||
                srcTypeSym.IsNUBSYM ||
                srcTypeSym.IsTYVARSYM ||
                srcTypeSym.IsERRORSYM);

            switch (dstTypeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad dst SYM kind in InferTypes");
                    return false;

                case SYMKIND.TYVARSYM:
                    return InferTypesTyVar(srcTypeSym, dstTypeSym as TYVARSYM, context);

                case SYMKIND.AGGTYPESYM:
                    if (srcTypeSym.IsERRORSYM)
                    {
                        return false;
                    }
                    return InferTypesAgg(srcTypeSym, dstTypeSym as AGGTYPESYM, context);

                case SYMKIND.ERRORSYM:
                    return InferTypesEqual(srcTypeSym, dstTypeSym, context);

                case SYMKIND.NUBSYM:
                case SYMKIND.ARRAYSYM:
                    if (srcTypeSym.Kind != dstTypeSym.Kind ||
                        srcTypeSym.IsARRAYSYM && (srcTypeSym as ARRAYSYM).Rank != (dstTypeSym as ARRAYSYM).Rank)
                    {
                        return false;
                    }

                    dstTypeSym = dstTypeSym.ParentSym as TYPESYM;
                    srcTypeSym = srcTypeSym.ParentSym as TYPESYM;

                    switch (srcTypeSym.Kind)
                    {
                        default:
                            DebugUtil.Assert(false, "Bad src SYM kind in InferTypes");
                            return false;

                        case SYMKIND.PTRSYM:
                            return false;

                        case SYMKIND.ERRORSYM:
                        case SYMKIND.ARRAYSYM:
                        case SYMKIND.NUBSYM:
                        case SYMKIND.AGGTYPESYM:
                        case SYMKIND.TYVARSYM:
                            break;
                    }
                    goto LRecurse;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.TypeContainsTyVars
        //
        /// <summary>
        /// If typeVars is empty, determines whether the type contains any type variables at all.
        /// If typeVars is non-empty,
        /// determines whether the type contains any of the type variables in "typeVars".
        /// The type variables in typeVars must be a "complete set",
        /// meaning that each one live in its "indexTotal" slot.
        /// That is, typeVars.ItemAsTYVARSYM(i).indexTotal == i must be true for all i.
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="typeVars"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool TypeContainsTyVars(TYPESYM typeSym, TypeArray typeVars)
        {
        LRecurse: // Label used for "tail" recursion.
            switch (typeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SYM kind in TypeContainsTyVars");
                    return false;

                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.METHGRPSYM:
                    return false;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.PINNEDSYM:
                    typeSym = typeSym.ParentSym as TYPESYM;
                    goto LRecurse;

                case SYMKIND.AGGTYPESYM:
                    { // BLOCK
                        AGGTYPESYM atSym = typeSym as AGGTYPESYM;

                        for (int i = 0; i < atSym.AllTypeArguments.Count; ++i)
                        {
                            if (TypeContainsTyVars(atSym.AllTypeArguments[i], typeVars))
                            {
                                return true;
                            }
                        }
                    }
                    return false;

                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym != null)
                    {
                        ERRORSYM errSym = typeSym as ERRORSYM;
                        DebugUtil.Assert(
                            errSym.ParentSym != null &&
                            errSym.ErrorName != null &&
                            errSym.TypeArguments != null);

                        for (int i = 0; i < errSym.TypeArguments.Count; ++i)
                        {
                            if (TypeContainsTyVars(errSym.TypeArguments[i], typeVars))
                            {
                                return true;
                            }
                        }
                        if (errSym.ParentSym.IsTYPESYM)
                        {
                            typeSym = errSym.ParentSym as TYPESYM;
                            goto LRecurse;
                        }
                    }
                    return false;

                case SYMKIND.TYVARSYM:
                    if (typeVars != null && typeVars.Count > 0)
                    {
                        int index = (typeSym as TYVARSYM).TotalIndex;
                        return (0 <= index && index < typeVars.Count && typeSym == typeVars[index]);
                    }
                    return true;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.InferTypesTyVar
        //
        /// <summary></summary>
        /// <param name="srcTypeSym">The type of the argument.</param>
        /// <param name="dstTyVarSym">The type of the parameter of the definition.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool InferTypesTyVar(
            TYPESYM srcTypeSym,
            TYVARSYM dstTyVarSym,
            InferContext context)
        {
            DebugUtil.Assert(
                srcTypeSym.IsAGGTYPESYM ||
                srcTypeSym.IsARRAYSYM ||
                srcTypeSym.IsNUBSYM ||
                srcTypeSym.IsTYVARSYM ||
                srcTypeSym.IsERRORSYM);

            if (!dstTyVarSym.IsMethodTypeVariable)
            {
                DebugUtil.Assert(dstTyVarSym.TotalIndex < context.ClassTypeArguments.Count);
                return (srcTypeSym == context.ClassTypeArguments[dstTyVarSym.TotalIndex]);
            }

            if (srcTypeSym.ParentSym == null)
            {
                return false;
            }

            //TYPESYM ** ptype = context.GetSlot(dstTyVarSym);
            //DebugUtil.Assert(ptype);
            TYPESYM typeSym = context.GetSlot(dstTyVarSym);

            if (typeSym != null)
            {
                // See if the inference is consistent.
                return srcTypeSym == typeSym;
            }

            // Only assign an array if its base type is an agg, tyvar or error (not ptr, for example).
            if (srcTypeSym.IsARRAYSYM)
            {
                TYPESYM type = (srcTypeSym as ARRAYSYM).GetMostBaseType();
                if (!type.IsAGGTYPESYM && !type.IsNUBSYM && !type.IsTYVARSYM && !type.IsERRORSYM)
                {
                    return false;
                }
            }

            // Make the inference.
            //*ptype = srcTypeSym;
            context.SetSlot(dstTyVarSym, srcTypeSym);
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.InferTypesAgg
        //
        /// <summary>
        // Called only by InferTypes to handle agg inference.
        // This is complicated by requiring that there be a unique inference.
        /// </summary>
        /// <param name="srcTypeSym"></param>
        /// <param name="dstAggTypeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InferTypesAgg(
            TYPESYM srcTypeSym,
            AGGTYPESYM dstAggTypeSym,
            InferContext context)
        {
            // If typeDst is A<...> we need to look for an instantiation of A to which srcTypeSym
            // is implicitly convertible (ie, a base type or interface).
            // If there is more than one such instantiation, the inferences need to be consistent.
            // If there is no such instantiation then inferencing fails.

            // Since dstAggTypeSym contains a method type variable, it can't be object.
            // Thus we don't have to worry about implicit convertibility of an interface
            // to object.
            DebugUtil.Assert(!dstAggTypeSym.IsPredefType(PREDEFTYPE.OBJECT));

            if (srcTypeSym.IsAGGTYPESYM || srcTypeSym.IsTYVARSYM)
            {
                bool found = false;
                return InferTypesAggCore(srcTypeSym, dstAggTypeSym, context, out found) && found;
            }

            if (!srcTypeSym.IsARRAYSYM ||
                dstAggTypeSym.AllTypeArguments.Count != 1 ||
                !dstAggTypeSym.IsPredefined())
            {
                return false;
            }

            switch (dstAggTypeSym.GetPredefType())
            {
                case PREDEFTYPE.G_IENUMERABLE:
                case PREDEFTYPE.G_ICOLLECTION:
                case PREDEFTYPE.G_ILIST:
                    return InferTypes(
                        (srcTypeSym as ARRAYSYM).ElementTypeSym, dstAggTypeSym.AllTypeArguments[0], context);

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.InferTypesAggCore
        //
        /// <summary>
        /// <para>WARNING: The return result of this function carries different semantics
        /// than that of the other Infer* functions.</para>
        /// <para>This infers the values of method type variables in atsDst from the type args in atsSrc.
        /// If the inference fails, this returns true and doesn't modify *pfInferred or pctx.prgtypeMeth.
        /// If the inference succeeds, pctx.prgtypeMeth is updated with the results.</para>
        /// <list type="bullet">
        /// <item>If the update is consistent this returns true.
        /// In this case *pfInferred is set to true and
        /// pctx.prgtypeMeth is updated with any new inference information.</item>
        /// <item>If the update is inconsistent
        /// with the existing contents of pctx.prgtypeMeth, this returns false.
        /// In this case, *pfInferred and pctx.prgtypeMeth may have been modified.</item>
        /// </list>
        /// </summary>
        /// <param name="srcTypeSym"></param>
        /// <param name="dstAggTypeSym"></param>
        /// <param name="context"></param>
        /// <param name="inferred"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InferTypesAggCore(
            TYPESYM srcTypeSym,
            AGGTYPESYM dstAggTypeSym,
            InferContext context,
            out bool inferred)
        {
            inferred = false;
            DebugUtil.Assert(srcTypeSym.IsAGGTYPESYM || srcTypeSym.IsTYVARSYM);

            if (srcTypeSym.IsAGGTYPESYM)
            {
                AGGTYPESYM srcAts = srcTypeSym as AGGTYPESYM;

                if (srcAts.GetAggregate() == dstAggTypeSym.GetAggregate())
                {
                    if (!InferTypesAggSingle(srcAts, dstAggTypeSym, context, out inferred))
                    {
                        return false;
                    }
                }
                else if (dstAggTypeSym.IsInterfaceType())
                {
                    // Check to see if srcAts or one of its base classes implements some instantiation of dstAggTypeSym.
                    for (AGGTYPESYM ats = srcAts; ats != null; ats = ats.GetBaseClass())
                    {
                        TypeArray ifaces = ats.GetIfacesAll();
                        for (int j = 0; j < ifaces.Count; j++)
                        {
                            AGGTYPESYM iface = ifaces[j] as AGGTYPESYM;
                            if (iface.GetAggregate() == dstAggTypeSym.GetAggregate() &&
                                !InferTypesAggSingle(iface, dstAggTypeSym, context, out inferred))
                            {
                                return false;
                            }
                        }
                    }
                }
                else if (!srcAts.IsInterfaceType() && !dstAggTypeSym.GetAggregate().IsSealed)
                {
                    // Check to see if srcAts derives from some instantiation of dstAggTypeSym.
                    for (AGGTYPESYM ats = srcAts; (ats = ats.GetBaseClass()) != null; )
                    {
                        if (ats.GetAggregate() == dstAggTypeSym.GetAggregate() &&
                            !InferTypesAggSingle(ats, dstAggTypeSym, context, out inferred))
                        {
                            return false;
                        }
                    }
                }
            }
            else if (srcTypeSym.IsTYVARSYM)
            {
                TypeArray bnds = (srcTypeSym as TYVARSYM).BoundArray;

                for (int i = 0; i < bnds.Count; i++)
                {
                    TYPESYM bnd = bnds[i];
                    if ((bnd.IsAGGTYPESYM || bnd.IsTYVARSYM) &&
                        !InferTypesAggCore(bnd, dstAggTypeSym, context, out inferred))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.InferTypesAggSingle
        //
        // WARNING: The return result of this function carries different semantics than that of the other
        // Infer* functions.
        //
        /// <summary>
        /// <para>This requires atsSrc.getAggregate() == atsDst.getAggregate().
        /// This infers the values of method type variables in atsDst from the type args in atsSrc.</para>
        /// <list type="bullet">
        /// <item>If the inference fails,
        /// this returns true and doesn't modify *pfInferred or pctx.prgtypeMeth.</item>
        /// <item>If the inference succeeds, pctx.prgtypeMeth is updated with the results.
        /// <list type="bullet">
        /// <item>If the update is consistent this returns true.
        /// In this case *pfInferred is set to true and
        /// pctx.prgtypeMeth is updated with any new inference information.</item>
        /// <item>If the update is inconsistent with the existing contents of pctx.prgtypeMeth,
        /// this returns false. In this case, *pfInferred and pctx.prgtypeMeth may have been modified.</item>
        /// </list>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="srcAggTypeSym"></param>
        /// <param name="dstAggTypeSym"></param>
        /// <param name="context"></param>
        /// <param name="inferred"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool InferTypesAggSingle(
            AGGTYPESYM srcAggTypeSym,
            AGGTYPESYM dstAggTypeSym,
            InferContext context,
            out bool inferred)
        {
            inferred = false;

            DebugUtil.Assert(srcAggTypeSym.GetAggregate() == dstAggTypeSym.GetAggregate());
            DebugUtil.Assert(dstAggTypeSym.AllTypeArguments.Count == srcAggTypeSym.AllTypeArguments.Count);

            // We need to start with an empty inference context.
            InferContext ctx = new InferContext(context);
            ctx.UnifiedMethodTypeVariables = new TypeArray();
            ctx.UnifiedMethodTypeVariables.Add(context.UnifiedMethodTypeVariables);

            for (int i = 0; i < srcAggTypeSym.AllTypeArguments.Count; i++)
            {
                if (!InferTypesEqual(
                    srcAggTypeSym.AllTypeArguments[i],
                    dstAggTypeSym.AllTypeArguments[i],
                    ctx))
                {
                    // Don't touch inferred or context.prgtypeMeth! Just return true to indicate that nothing
                    // inconsistent was found.
                    return true;
                }
            }

            // Inferencing succeeded. Now make sure the results are consistent.
            for (int i = 0; i < ctx.MethodTypeVariables.Count; i++)
            {
                if (context.UnifiedMethodTypeVariables[i] == null)
                {
                    context.UnifiedMethodTypeVariables[i] = ctx.UnifiedMethodTypeVariables[i];
                }
                else if (
                    context.UnifiedMethodTypeVariables[i] != ctx.UnifiedMethodTypeVariables[i] &&
                    ctx.UnifiedMethodTypeVariables[i] != null)
                {
                    // Inconsistent.
                    return false;
                }
            }

            inferred = true;
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.InferTypesEqual
        //
        /// <summary>
        /// This is used when matching type args for agg type syms.
        /// It requires typeSrc to match typeDst without any conversion.
        /// This assumes that typeSrc and typeDst came from type arguments.
        /// </summary>
        /// <param name="srcTypeSym"></param>
        /// <param name="dstTypeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool InferTypesEqual(
            TYPESYM srcTypeSym,
            TYPESYM dstTypeSym,
            InferContext context)
        {
        LRecurse:
            DebugUtil.Assert(
                srcTypeSym.IsAGGTYPESYM ||
                srcTypeSym.IsARRAYSYM ||
                srcTypeSym.IsNUBSYM ||
                srcTypeSym.IsTYVARSYM ||
                srcTypeSym.IsERRORSYM);

            switch (dstTypeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SYM kind in InferTypesEqual");
                    return false;

                case SYMKIND.TYVARSYM:
                    return InferTypesTyVar(srcTypeSym, dstTypeSym as TYVARSYM, context);

                case SYMKIND.AGGTYPESYM:
                    if (!srcTypeSym.IsAGGTYPESYM ||
                        (srcTypeSym as AGGTYPESYM).GetAggregate() != (dstTypeSym as AGGTYPESYM).GetAggregate())
                    {
                        return false;
                    }
                    for (int i = 0; i < (srcTypeSym as AGGTYPESYM).AllTypeArguments.Count; i++)
                    {
                        if (!InferTypesEqual(
                            (srcTypeSym as AGGTYPESYM).AllTypeArguments[i],
                            (dstTypeSym as AGGTYPESYM).AllTypeArguments[i],
                            context))
                        {
                            return false;
                        }
                    }
                    return true;

                case SYMKIND.ERRORSYM:
                    if (!srcTypeSym.IsERRORSYM || dstTypeSym.ParentSym == null || srcTypeSym.ParentSym == null)
                    {
                        return false;
                    }
                    {
                        ERRORSYM srcErrorSym = srcTypeSym as ERRORSYM;
                        ERRORSYM dstErrorSym = dstTypeSym as ERRORSYM;

                        DebugUtil.Assert(
                            srcErrorSym.ParentSym != null &&
                            srcErrorSym.ErrorName != null &&
                            srcErrorSym.TypeArguments != null);
                        DebugUtil.Assert(
                            dstErrorSym.ParentSym != null &&
                            dstErrorSym.ErrorName != null &&
                            dstErrorSym.TypeArguments != null);

                        if (srcErrorSym.ErrorName != dstErrorSym.ErrorName ||
                            srcErrorSym.TypeArguments.Count != dstErrorSym.TypeArguments.Count)
                        {
                            return false;
                        }
                        if (!srcErrorSym.ParentSym.IsTYPESYM != !dstErrorSym.ParentSym.IsTYPESYM)
                        {
                            return false;
                        }
                        if (srcErrorSym.ParentSym.IsTYPESYM &&
                            !InferTypesEqual(srcErrorSym.ParentSym as TYPESYM, dstErrorSym.ParentSym as TYPESYM, context))
                        {
                            return false;
                        }
                        for (int i = 0; i < srcErrorSym.TypeArguments.Count; i++)
                        {
                            if (!InferTypesEqual(
                                srcErrorSym.TypeArguments[i],
                                dstErrorSym.TypeArguments[i],
                                context))
                            {
                                return false;
                            }
                        }
                    }
                    return true;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.NUBSYM:
                    if (srcTypeSym.Kind != dstTypeSym.Kind ||
                        srcTypeSym.IsARRAYSYM && (srcTypeSym as ARRAYSYM).Rank != (dstTypeSym as ARRAYSYM).Rank)
                    {
                        return false;
                    }

                    dstTypeSym = dstTypeSym.ParentSym as TYPESYM;
                    srcTypeSym = srcTypeSym.ParentSym as TYPESYM;

                    switch (srcTypeSym.Kind)
                    {
                        default:
                            DebugUtil.Assert(false, "Bad src SYM kind in InferTypesEqual");
                            return false;

                        case SYMKIND.PTRSYM:
                            return false;

                        case SYMKIND.ERRORSYM:
                        case SYMKIND.ARRAYSYM:
                        case SYMKIND.NUBSYM:
                        case SYMKIND.AGGTYPESYM:
                        case SYMKIND.TYVARSYM:
                            break;
                    }
                    goto LRecurse;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeReportError
        //
        /// <summary></summary>
        /// <param name="srcTypeSym"></param>
        /// <param name="classTypeArgs"></param>
        /// <param name="methTypeArgs"></param>
        /// <param name="flags"></param>
        /// <param name="hasError"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstTypeReportError(
            TYPESYM srcTypeSym,
            TypeArray classTypeArgs,
            TypeArray methTypeArgs,   // = NULL,
            SubstTypeFlagsEnum flags, // = SubstTypeFlags::NormNone);
            ref bool hasError)
        {
            if (srcTypeSym == null)
            {
                return null;
            }

            SubstContext context = new SubstContext(classTypeArgs, methTypeArgs, flags);
            return (context.FNop()
                ? srcTypeSym
                : SubstTypeCoreReportError(srcTypeSym, context, ref hasError));
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeCoreReportError
        //
        /// <summary>
        /// <para>Return the type created by substituting type arguments.</para>
        /// <para>If null is substituted to a type variable, report it.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstTypeCoreReportError(
            TYPESYM typeSym,
            SubstContext context,
            ref bool hasError)
        {
            TYPESYM srcTypeSym = null;
            TYPESYM dstTypeSym = null;

            switch (typeSym.Kind)
            {
                //--------------------------------------------------
                // If typeSym is unaffected by type arguments, return it as is.
                //--------------------------------------------------
                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.METHGRPSYM:
                case SYMKIND.ANONMETHSYM:
                case SYMKIND.IMPLICITTYPESYM:           // CS3
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM:   // CS3
                case SYMKIND.LAMBDAEXPRSYM:             // CS3
                case SYMKIND.DYNAMICSYM:                // CS4
                    return typeSym;

                //--------------------------------------------------
                // If typeSym is PARAMMODSYM (represents ref or out), process the modified type.
                // If the obtained type is same to the original modified type, return it.
                // Otherwise, create a PARAMMODSYM
                // which represents the ref or out modification of the obtained type.
                //--------------------------------------------------
                case SYMKIND.PARAMMODSYM:
                    srcTypeSym = (typeSym as PARAMMODSYM).ParamTypeSym;
                    dstTypeSym = SubstTypeCoreReportError(srcTypeSym, context, ref hasError);
                    return ((dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetParamModifier(dstTypeSym, (typeSym as PARAMMODSYM).IsOut));

                //--------------------------------------------------
                // If MODOPTYPESYM, process the modified type just like PARAMMODSYM.
                //--------------------------------------------------
                case SYMKIND.MODOPTTYPESYM:
                    srcTypeSym = (typeSym as MODOPTTYPESYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCoreReportError(srcTypeSym, context, ref hasError);
                    return ((dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetModOptType(dstTypeSym, (typeSym as MODOPTTYPESYM).ModOptSym));

                //--------------------------------------------------
                // If ARRAYSYM, process the element type.
                //--------------------------------------------------
                case SYMKIND.ARRAYSYM:
                    srcTypeSym = (typeSym as ARRAYSYM).ElementTypeSym;
                    dstTypeSym = SubstTypeCoreReportError(srcTypeSym, context, ref hasError);
                    return ((dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetArray(dstTypeSym, (typeSym as ARRAYSYM).Rank, null));

                //--------------------------------------------------
                // If PTRSYM, process the underlying type.
                //--------------------------------------------------
                case SYMKIND.PTRSYM:
                    srcTypeSym = (typeSym as PTRSYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCoreReportError(srcTypeSym, context, ref hasError);
                    return (dstTypeSym == srcTypeSym) ? typeSym : GetPtrType(dstTypeSym);

                //--------------------------------------------------
                // If NUBSYM, process the underlying type.
                //--------------------------------------------------
                case SYMKIND.NUBSYM:
                    srcTypeSym = (typeSym as NUBSYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCoreReportError(srcTypeSym, context, ref hasError);
                    return (dstTypeSym == srcTypeSym) ? typeSym : GetNubType(dstTypeSym);

                //--------------------------------------------------
                // If PINNEDSYM, process the base type.
                //--------------------------------------------------
                case SYMKIND.PINNEDSYM:
                    srcTypeSym = (typeSym as PINNEDSYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCoreReportError(srcTypeSym, context, ref hasError);
                    return (dstTypeSym == srcTypeSym) ? typeSym : GetPinnedType(dstTypeSym);

                //--------------------------------------------------
                // If AGGTYPESYM, substitute type arguments to type parameters.
                // If no type arguments, return typeSym as is.
                //--------------------------------------------------
                case SYMKIND.AGGTYPESYM:
                    AGGTYPESYM ats = typeSym as AGGTYPESYM;
                    if (TypeArray.Size(ats.AllTypeArguments) > 0)
                    {
                        TypeArray typeArgs
                            = SubstTypeArrayReportError(ats.AllTypeArguments, context, ref hasError);
                        if (ats.AllTypeArguments != typeArgs && !hasError)
                        {
                            return GetInstAgg(ats.GetAggregate(), typeArgs);
                        }
                    }
                    return typeSym;

                //--------------------------------------------------
                // IF ERRORSYM,
                //--------------------------------------------------
                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym != null)
                    {
                        ERRORSYM errorSym = typeSym as ERRORSYM;

                        DebugUtil.Assert(
                            errorSym.ParentSym != null &&
                            String.IsNullOrEmpty(errorSym.Name) &&
                            errorSym.TypeArguments != null);

                        PARENTSYM parentSym = errorSym.ParentSym;
                        if (parentSym.IsTYPESYM)
                        {
                            parentSym
                                = SubstTypeCoreReportError(parentSym as TYPESYM, context, ref hasError);
                        }
                        TypeArray typeArgs
                            = SubstTypeArrayReportError(errorSym.TypeArguments, context, ref hasError);
                        if (typeArgs != errorSym.TypeArguments || parentSym != errorSym.ParentSym)
                        {
                            return GetErrorType(parentSym, errorSym.ErrorName, typeArgs);
                        }
                    }
                    return typeSym;

                //--------------------------------------------------
                // If TYVARSYM, return the element of the specifined index in context.
                // If the index is invalid, return a default TYVARSYM instace.
                //--------------------------------------------------
                case SYMKIND.TYVARSYM:
                    {
                        TYVARSYM tyVarSym = typeSym as TYVARSYM;
                        int index = tyVarSym.TotalIndex;

                        //----------------------------------------
                        // Method
                        //----------------------------------------
                        if (tyVarSym.IsMethodTypeVariable)
                        {
                            if ((context.Flags & SubstTypeFlagsEnum.DenormMeth) != 0 && tyVarSym.ParentSym != null)
                            {
                                return typeSym;
                            }

                            DebugUtil.Assert(tyVarSym.Index == tyVarSym.TotalIndex);
                            DebugUtil.Assert(
                                context.MethodTypeArguments == null ||
                                context.MethodTypeArguments.Count == 0 ||
                                index < context.MethodTypeArguments.Count);

                            if (index < context.MethodTypeArguments.Count)
                            {
                                TYPESYM argTypeSym = context.MethodTypeArguments[index];
                                if (argTypeSym == null)
                                {
                                    hasError = true;
                                }
                                return argTypeSym;
                            }
                            else
                            {
                                hasError = true;
                                if ((context.Flags & SubstTypeFlagsEnum.NormMeth) != 0)
                                {
                                    return GetStdMethTypeVar(index);
                                }
                                else
                                {
                                    return typeSym;
                                }
                            }
                        }
                        //----------------------------------------
                        // Aggregate
                        //----------------------------------------
                        if ((context.Flags & SubstTypeFlagsEnum.DenormClass) != 0 && tyVarSym.ParentSym != null)
                        {
                            return typeSym;
                        }
                        if (index < context.ClassTypeArguments.Count)
                        {
                            TYPESYM argTypeSym = context.ClassTypeArguments[index];
                            if (argTypeSym == null)
                            {
                                hasError = true;
                            }
                            return argTypeSym;
                        }
                        else
                        {
                            hasError = true;
                            if ((context.Flags & SubstTypeFlagsEnum.NormClass) != 0)
                            {
                                return GetStdClsTypeVar(index);
                            }
                            else
                            {
                                return typeSym;
                            }
                        }
                    }
                //--------------------------------------------------
                // Otherwise, return typeSym as is.
                //--------------------------------------------------
                default:
                    DebugUtil.Assert(false);
                    // fall through by design...
                    hasError = true;
                    return typeSym;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeArrayReportError
        //
        /// <summary>
        /// <para>SubstContext のデータが有効なら TypeArray の各要素を処理したもので
        /// 新しい TypeArray を作成して返す。</para>
        /// <para>無効なら元の TypeArray を返す。</para>
        /// </summary>
        /// <param name="typeArraySrc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray SubstTypeArrayReportError(
            TypeArray srcTypeArray,
            SubstContext context,
            ref bool hasError)
        {
            if (srcTypeArray == null ||
                srcTypeArray.Count == 0 ||
                context == null ||
                context.FNop())
            {
                return srcTypeArray;
            }

            TypeArray typeArrayNew = new TypeArray();
            for (int i = 0; i < srcTypeArray.Count; ++i)
            {
                typeArrayNew.Add(SubstTypeCoreReportError(srcTypeArray[i], context, ref hasError));
            }
            return AllocParams(typeArrayNew);
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC.InferTypeArgs
        //
        /// <summary>
        /// paramsMeth may be either meth->params or an expanded version of meth->params
        /// (in the case when meth->isParamArray is true).
        /// argsMatch are the arguments being supplied to the call.
        /// This uses the argument types to infer type variable values.
        /// </summary>
        /// <param name="methSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <param name="methParamArray"></param>
        /// <param name="matchArgInfos"></param>
        /// <param name="typeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool InferTypeArgs(
            METHSYM methSym,
            AGGTYPESYM aggTypeSym,
            TypeArray methParamArray,
            ArgInfos matchArgInfos,
            EXPR objectExpr,    // (CS3) hirano567@hotmail.co.jp
            ref TypeArray typeArray)
        {
            DebugUtil.Assert(methSym.TypeVariables.Count > 0);
            DebugUtil.Assert(methSym.IsParameterArray || methSym.ParameterTypes == methParamArray);

            if (methParamArray.Count == 0 || methSym.CanInferState == METHSYM.CanInferStateEnum.No)
            {
                return false;
            }

            DebugUtil.Assert(matchArgInfos != null);
            DebugUtil.Assert(matchArgInfos.ArgumentCount == methParamArray.Count);

            int typeCount = methSym.TypeVariables.Count;
            BSYMMGR.InferContext context = new BSYMMGR.InferContext();

            // Fill in the Infer Context with an empty mapping.
            context.MethodTypeVariables = methSym.TypeVariables;
            int methTvCount = methSym.TypeVariables.Count;
            context.ClassTypeArguments = aggTypeSym.AllTypeArguments;

            //context.prgtypeMeth = STACK_ALLOC_ZERO(TYPESYM *, typeCount);
            context.UnifiedMethodTypeVariables = new TypeArray(methTvCount);

            Compiler.EnsureState(methParamArray, AggStateEnum.Prepared);

            //--------------------------------------------------------
            // (CS3) Extension methods
            //--------------------------------------------------------
            if (methSym.IsInstanceExtensionMethod)
            {
                METHSYM staticMethSym = methSym.StaticExtensionMethodSym;
                if (staticMethSym == null ||
                    staticMethSym.ParameterTypes == null ||
                    staticMethSym.ParameterTypes.Count == 0 ||
                    objectExpr == null ||
                    objectExpr.TypeSym == null)
                {
                    return false;
                }

                switch (objectExpr.Kind)
                {
                    case EXPRKIND.ANONMETH:
                        if (!InferTypesFunc(
                                objectExpr as EXPRANONMETH,
                                objectExpr.TypeSym,
                                staticMethSym.ParameterTypes[0],
                                context))
                        {
                            return false;
                        }
                        break;

                    case EXPRKIND.LAMBDAEXPR:
                        if (!InferTypesFunc(
                                objectExpr as EXPRLAMBDAEXPR,
                                objectExpr.TypeSym,
                                staticMethSym.ParameterTypes[0],
                                context))
                        {
                            return false;
                        }
                        break;

                    default:
                        if (!Compiler.MainSymbolManager.InferTypes(
                            objectExpr.TypeSym,
                            staticMethSym.ParameterTypes[0],
                            context))
                        {
                            return false;
                        }
                        break;
                }
            }

            //--------------------------------------------------------
            //
            //--------------------------------------------------------
            for (int i = 0; i < methParamArray.Count; i++)
            {
                EXPR argExpr = matchArgInfos.ExprList[i];
                TYPESYM argTypeSym = matchArgInfos.ArgumentTypes[i];

                // InferTypes requires the inheritance to be resolved.
                Compiler.EnsureState(argTypeSym, AggStateEnum.Prepared);

                switch (argExpr.Kind)
                {
                    case EXPRKIND.ANONMETH:
                        if (!InferTypesFunc(
                                argExpr as EXPRANONMETH,
                                argTypeSym,
                                methParamArray[i],
                                context))
                        {
                            return false;
                        }
                        break;

                    case EXPRKIND.LAMBDAEXPR:
                        if (!InferTypesFunc(
                                argExpr as EXPRLAMBDAEXPR,
                                argTypeSym,
                                methParamArray[i],
                                context))
                        {
                            return false;
                        }
                        break;

                    default:
                        if (!Compiler.MainSymbolManager.InferTypes(
                            argTypeSym,
                            methParamArray[i],
                            context))
                        {
                            return false;
                        }
                        break;
                }
            }

            for (int i = 0; i < typeCount; i++)
            {
                if (context.UnifiedMethodTypeVariables[i] == null)
                {
                    // The inference succeeded but not all type variables were mapped.
                    // This can happen if not all ty^pe variables appear in the signature or if
                    // some of the args are null. Generally methSym.cisCanInfer should be set to cisNo or cisMaybe
                    // in the former case, but this can still happen if this is a param method and
                    // we're in the zero extra args case.

                    if (methSym.CanInferState == METHSYM.CanInferStateEnum.Maybe)
                    {
                        BSYMMGR.SetCanInferState(methSym);
                    }
                    return false;
                }
            }

            typeArray = Compiler.MainSymbolManager.AllocParams(context.UnifiedMethodTypeVariables);
            methSym.CanInferState = METHSYM.CanInferStateEnum.Yes;
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.InferTypesFunc (1)
        //
        /// <summary></summary>
        /// <remarks>(CS3) hirano567@hotmail.co.jp</remarks>
        /// <param name="srcAnonMethExpr"></param>
        /// <param name="srcTypeSym"></param>
        /// <param name="dstTypeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InferTypesFunc(
            EXPRANONMETH srcAnonMethExpr,
            TYPESYM srcTypeSym,
            TYPESYM dstTypeSym,
            BSYMMGR.InferContext context)
        {
            AnonMethInfo anonInfo = srcAnonMethExpr.AnonymousMethodInfo;

            AGGTYPESYM dstAts = dstTypeSym as AGGTYPESYM;
            if (dstAts == null)
            {
                return false;
            }

            AGGSYM dstAggSym = dstAts.GetAggregate();
            AGGSYM funcAggSym = null;
            int paramCount = anonInfo.ParameterArray.Count;

            switch (paramCount)
            {
                case 0:
                    return true;

                case 1:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                    break;

                case 2:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G3_FUNC, true);
                    break;

                case 3:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G4_FUNC, true);
                    break;

                case 4:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G5_FUNC, true);
                    break;

                default:
                    return true;
            }
            if (dstAggSym != funcAggSym)
            {
                return false;
            }
            TypeArray dstTypeArgs = dstAts.TypeArguments;

            //--------------------------------------------------------
            // Parameter types.
            //--------------------------------------------------------
            for (int i = 0; i < paramCount; ++i)
            {
                TYPESYM argTypeSym = anonInfo.ParameterArray[i];

                if (argTypeSym != null &&
                    argTypeSym.Kind != SYMKIND.IMPLICITTYPESYM)
                {
                    if (!Compiler.MainSymbolManager.InferTypes(
                        argTypeSym,
                        dstTypeArgs[i],
                        context))
                    {
                        return false;
                    }
                }
            }

            //--------------------------------------------------------
            // Infer by the type of the return value.
            //--------------------------------------------------------
            TYPESYM retTypeSym = null;
            if (anonInfo.ReturnTypeSym != null)
            {
                retTypeSym = anonInfo.ReturnTypeSym;
            }
            else if (anonInfo.ReturnExprList != null)
            {
                EXPR expr2 = anonInfo.ReturnExprList;
                while (expr2 != null)
                {
                    EXPRRETURN returnExpr;
                    if (expr2.Kind == EXPRKIND.LIST)
                    {
                        returnExpr = expr2.AsBIN.Operand1 as EXPRRETURN;
                        expr2 = expr2.AsBIN.Operand2;
                    }
                    else
                    {
                        returnExpr = expr2 as EXPRRETURN;
                        expr2 = null;
                    }

                    if (retTypeSym == null)
                    {
                        retTypeSym = returnExpr.ObjectExpr.TypeSym;
                    }
                    else
                    {
                        if (CanConvert(
                            returnExpr.ObjectExpr,
                            retTypeSym,
                            0))
                        {
                            continue;
                        }
                        else if (CanConvert(
                            retTypeSym,
                            returnExpr.ObjectExpr.TypeSym,
                            0))
                        {
                            retTypeSym = returnExpr.ObjectExpr.TypeSym;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return true;
            }

            if (retTypeSym != null && retTypeSym.Kind != SYMKIND.IMPLICITTYPESYM)
            {
                TYPESYM resTypeSym = dstTypeArgs[paramCount];

                if (!Compiler.MainSymbolManager.InferTypes(
                    retTypeSym,
                    resTypeSym,
                    context))
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.InferTypesFunc (2)
        //
        /// <summary></summary>
        /// <remarks>(CS3) hirano567@hotmail.co.jp</remarks>
        /// <param name="srcLambdaExpr"></param>
        /// <param name="srcTypeSym"></param>
        /// <param name="dstTypeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InferTypesFunc(
            EXPRLAMBDAEXPR srcLambdaExpr,
            TYPESYM srcTypeSym,
            TYPESYM dstTypeSym,
            BSYMMGR.InferContext context)
        {
            AnonMethInfo anonInfo = srcLambdaExpr.AnonymousMethodInfo;

            AGGTYPESYM dstAts = dstTypeSym as AGGTYPESYM;
            if (dstAts == null)
            {
                return false;
            }

            AGGSYM dstAggSym = dstAts.GetAggregate();
            AGGSYM funcAggSym = null;
            int paramCount = anonInfo.ParameterArray.Count;

            switch (paramCount)
            {
                case 0:
                    return true;

                case 1:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G2_FUNC, true);
                    break;

                case 2:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G3_FUNC, true);
                    break;

                case 3:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G4_FUNC, true);
                    break;

                case 4:
                    funcAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G5_FUNC, true);
                    break;

                default:
                    return true;
            }
            if (dstAggSym != funcAggSym)
            {
                return false;
            }
            TypeArray dstTypeArgs = dstAts.TypeArguments;

            //--------------------------------------------------------
            // Prepare to bind the lambda expression: set the parameter types.
            //--------------------------------------------------------
            for (int i = 0; i < paramCount; ++i)
            {
                LOCVARSYM locSym = srcLambdaExpr.ParameterList[i];
                bool hasError = false;

                if (locSym.TypeSym == null ||
                    locSym.TypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                {
                    TYPESYM argTypeSym = Compiler.MainSymbolManager.SubstTypeReportError(
                        dstTypeArgs[i],
                        context.ClassTypeArguments,
                        context.UnifiedMethodTypeVariables,
                        SubstTypeFlagsEnum.NormNone,
                        ref hasError);

                    if (hasError ||
                        argTypeSym == null ||
                        argTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        return true;
                    }
                    locSym.TypeSym = argTypeSym;
                }
                else
                {
                    if (!Compiler.MainSymbolManager.InferTypes(
                        locSym.TypeSym,
                        dstTypeArgs[i],
                        context))
                    {
                        return false;
                    }
                }
            }

            //--------------------------------------------------------
            // Bind the lambda expression and infer by the type of the return value.
            //--------------------------------------------------------
            BindLambdaExpressionInner(anonInfo);
            TYPESYM retTypeSym = anonInfo.ReturnTypeSym;
            if (retTypeSym != null && retTypeSym.Kind != SYMKIND.IMPLICITTYPESYM)
            {
                TYPESYM resTypeSym = dstTypeArgs[paramCount];

                if (!Compiler.MainSymbolManager.InferTypes(
                    retTypeSym,
                    resTypeSym,
                    context))
                {
                    return false;
                }
            }
            return true;
        }

        //
        // System.Linq.Enumerable
        //

        //--------------------------------------------------
        // FUNCBREC.InferTypeLinqEnumerable
        //
        /// <summary></summary>
        /// <param name="grpExpr"></param>
        /// <param name="argInfos"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal bool InferTypeLinqEnumerable(
            EXPRMEMGRP grpExpr,
            METHSYM currentMethodSym,
            ArgInfos argInfos,
            ref TypeArray typeArguments)
        {
            if (typeArguments == null)
            {
                typeArguments = new TypeArray();
            }

            switch (grpExpr.Name)
            {
                //----------------------------------------------------
                // Count
                //----------------------------------------------------
                case "Count":
                    return InferTypeLinqEnumerableCount(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        ref typeArguments);

                //----------------------------------------------------
                // DefaultIfEmpty
                //
                // public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
                // 	this IEnumerable<TSource> source
                // )
                //
                // public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
                // 	this IEnumerable<TSource> source,
                // 	TSource defaultValue
                // )
                //----------------------------------------------------
                case "DefaultIfEmpty":
                    return InferTypeLinqEnumerableCount(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        ref typeArguments);

                //----------------------------------------------------
                // First
                //----------------------------------------------------
                case "First":
                    return InferTypeLinqEnumerableCount(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        ref typeArguments);

                //----------------------------------------------------
                // GroupBy
                //----------------------------------------------------
                case "GroupBy":
                    return InferTypeLinqEnumerableGroupBy(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        ref typeArguments);

                //----------------------------------------------------
                // Join, GroupJoin
                //----------------------------------------------------
                case "GroupJoin":
                case "Join":
                    return InferTypeLinqEnumerableJoin(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        (grpExpr.Name == "GroupJoin"),
                        ref typeArguments);

                //----------------------------------------------------
                // OrderBy, OrderByDescending, ThenBy, ThenByDescending
                //
                // public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector
                // )
                //
                // public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     IComparer<TKey> comparer
                // )
                //----------------------------------------------------
                case "OrderBy":
                case "OrderByDescending":
                case "ThenBy":
                case "ThenByDescending":
                    return InferTypeLinqEnumerableSelect(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        false,
                        ref typeArguments);

                //----------------------------------------------------
                // Select
                //----------------------------------------------------
                case "Select":
                    return InferTypeLinqEnumerableSelect(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        false,
                        ref typeArguments);

                //----------------------------------------------------
                // SelectMany
                //----------------------------------------------------
                case "SelectMany":
                    return InferTypeLinqEnumerableSelectMany(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        ref typeArguments);

                //----------------------------------------------------
                // ToList
                //----------------------------------------------------
                case "ToList":
                    return InferTypeLinqEnumerableCount(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        ref typeArguments);

                //----------------------------------------------------
                // Where
                //----------------------------------------------------
                case "Where":
                    return InferTypeLinqEnumerableWhere(
                        grpExpr,
                        currentMethodSym,
                        argInfos,
                        ref typeArguments);

                default:
                    break;
            }
            return false;
        }

        //--------------------------------------------------
        // FUNCBREC.InferTypeLinqEnumerableCount
        //
        //
        /// <summary></summary>
        /// <param name="grpExpr"></param>
        /// <param name="argInfos"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal bool InferTypeLinqEnumerableCount(
            EXPRMEMGRP grpExpr,
            METHSYM currentMethodSym,
            ArgInfos argInfos,
            ref TypeArray typeArguments)
        {
            // public static TSource First<TSource>(
            //     this IEnumerable<TSource> source
            // )

            if (currentMethodSym.TypeVariables.Count != 1)
            {
                return false;
            }

            TYPESYM collectionTypeSym = null;
            EXPR collectionExpr = grpExpr.ObjectExpr;

            if (collectionExpr != null)
            {
                if (argInfos.ExprList.Count != 0)
                {
                    return false;
                }
                collectionTypeSym = collectionExpr.TypeSym;
            }
            else
            {
                if (argInfos.ExprList.Count != 1)
                {
                    return false;
                }
                collectionExpr = argInfos.ExprList[0];
                collectionTypeSym = argInfos.ArgumentTypes[0];
            }

            TYPESYM srcTypeSym = GetElementType(
                null,
                collectionExpr.TreeNode,
                collectionTypeSym,
                collectionExpr);
            if (srcTypeSym == null ||
                srcTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            TypeArray ta = new TypeArray();
            ta.Add(srcTypeSym);
            ta = Compiler.MainSymbolManager.AllocParams(ta);
            typeArguments = ta;
            return true;
        }

        //--------------------------------------------------
        // FUNCBREC.InferTypeLinqEnumerableGroupBy
        //
        /// <summary></summary>
        /// <param name="grpExpr"></param>
        /// <param name="argInfos"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal bool InferTypeLinqEnumerableGroupBy(
            EXPRMEMGRP grpExpr,
            METHSYM currentMethodSym,
            ArgInfos argInfos,
            ref TypeArray typeArguments)
        {
            EXPR collectionExpr = grpExpr.ObjectExpr;
            TYPESYM collectionTypeSym = null;
            EXPR keyExpr = null;
            TYPESYM keyTypeSym = null;

            EXPR argExpr2 = null;
            EXPR argExpr3 = null;
            EXPR argExpr4 = null;
            TYPESYM argTypeSym2 = null;
            TYPESYM argTypeSym3 = null;
            TYPESYM argTypeSym4 = null;

            int argCount = 0;
            bool isStaticMethod = false;
            TypeArray inferredTypeArgs = null;
            TypeArray ta = null;    // temporary

            //--------------------------------------------------------
            // TSource
            //--------------------------------------------------------
            if (collectionExpr != null)
            {
                argCount = argInfos.ExprList.Count + 1;

                collectionTypeSym = collectionExpr.TypeSym;
                keyExpr = argInfos.ExprList[0];

                switch (argCount)
                {
                    case 2:
                        break;

                    case 3:
                        argExpr2 = argInfos.ExprList[1];
                        break;

                    case 4:
                        argExpr2 = argInfos.ExprList[1];
                        argExpr3 = argInfos.ExprList[2];
                        break;

                    case 5:
                        argExpr2 = argInfos.ExprList[1];
                        argExpr3 = argInfos.ExprList[2];
                        argExpr4 = argInfos.ExprList[3];
                        break;

                    default:
                        return false;
                }
            }
            else
            {
                argCount = argInfos.ExprList.Count;
                isStaticMethod = true;

                collectionExpr = argInfos.ExprList[0];
                collectionTypeSym = argInfos.ArgumentTypes[0];

                keyExpr = argInfos.ExprList[1];

                switch (argCount)
                {
                    case 2:
                        break;

                    case 3:
                        argExpr2 = argInfos.ExprList[2];
                        break;

                    case 4:
                        argExpr2 = argInfos.ExprList[2];
                        argExpr3 = argInfos.ExprList[3];
                        break;

                    case 5:
                        argExpr2 = argInfos.ExprList[2];
                        argExpr3 = argInfos.ExprList[3];
                        argExpr4 = argInfos.ExprList[4];
                        break;

                    default:
                        return false;
                }
            }
            if (keyExpr == null)
            {
                return false;
            }

            TYPESYM srcTypeSym = GetElementType(
                null,
                collectionExpr.TreeNode,
                collectionTypeSym,
                collectionExpr);
            if (srcTypeSym == null ||
                srcTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // TKey
            //--------------------------------------------------------
            if (keyExpr.Kind == EXPRKIND.LAMBDAEXPR)
            {
                EXPRLAMBDAEXPR keyLambdaExpr = keyExpr as EXPRLAMBDAEXPR;

                if (keyLambdaExpr.ParameterList.Count != 1)
                {
                    return false;
                }
                keyLambdaExpr.ParameterList[0].TypeSym = srcTypeSym;
                BindLambdaExpressionInner(keyLambdaExpr.AnonymousMethodInfo);

                keyTypeSym = keyLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
                if (keyTypeSym == null ||
                    keyTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                {
                    return false;
                }
            }
            else if (keyExpr.Kind == EXPRKIND.ANONMETH)
            {
                EXPRANONMETH keyAnonExpr = keyExpr as EXPRANONMETH;
                DebugUtil.Assert(keyAnonExpr != null);
                AnonMethInfo anonInfo = keyAnonExpr.AnonymousMethodInfo;

                if (anonInfo.ParameterArray.Count != 1)
                {
                    return false;
                }
                keyTypeSym = GetAnonymousMethodReturnType(anonInfo.ReturnExprList);
            }
            else
            {
                return false;
            }

            //--------------------------------------------------------
            //
            //--------------------------------------------------------
            switch (argCount)
            {
                //----------------------------------------------------
                // 2 arguments
                //----------------------------------------------------
                // public static IEnumerable<IGrouping<TKey, TSource>>
                // GroupBy<TSource, TKey>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector
                // )
                //----------------------------------------------------
                case 2:
                    if (currentMethodSym.TypeVariables.Count != 2)
                    {
                        return false;
                    }
                    inferredTypeArgs = new TypeArray();
                    inferredTypeArgs.Add(srcTypeSym);
                    inferredTypeArgs.Add(keyTypeSym);
                    inferredTypeArgs
                        = Compiler.MainSymbolManager.AllocParams(inferredTypeArgs);
                    typeArguments = inferredTypeArgs;
                    return true;

                //----------------------------------------------------
                // 3 arguments
                //----------------------------------------------------
                // public static IEnumerable<IGrouping<TKey, TSource>>
                // GroupBy<TSource, TKey>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     IEqualityComparer<TKey> comparer
                // )
                //
                // public static IEnumerable<IGrouping<TKey, TElement>>
                // GroupBy<TSource, TKey, TElement>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     Func<TSource, TElement> elementSelector
                // )
                //
                // public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     Func<TKey, IEnumerable<TSource>, TResult> resultSelector
                // )
                //----------------------------------------------------
                case 3:
                    switch (argExpr2.Kind)
                    {
                        //--------------------------------------------
                        // keySelector is a lambda expression
                        //--------------------------------------------
                        case EXPRKIND.LAMBDAEXPR:
                            if (currentMethodSym.TypeVariables.Count != 3)
                            {
                                return false;
                            }

                            EXPRLAMBDAEXPR lambdaExpr2 = argExpr2 as EXPRLAMBDAEXPR;

                            switch (lambdaExpr2.ParameterList.Count)
                            {
                                case 1:
                                    lambdaExpr2.ParameterList[0].TypeSym = srcTypeSym;
                                    BindLambdaExpressionInner(lambdaExpr2.AnonymousMethodInfo);

                                    argTypeSym2 = lambdaExpr2.AnonymousMethodInfo.ReturnTypeSym;
                                    if (argTypeSym2 == null ||
                                        argTypeSym2.Kind == SYMKIND.IMPLICITTYPESYM)
                                    {
                                        return false;
                                    }
                                    break;

                                case 2:
                                    lambdaExpr2.ParameterList[0].TypeSym = keyTypeSym;

                                    AGGSYM ienumAgg = Compiler.GetReqPredefAgg(
                                        PREDEFTYPE.G_IENUMERABLE,
                                        true);
                                    ta = new TypeArray();
                                    ta.Add(srcTypeSym);
                                    ta = Compiler.MainSymbolManager.AllocParams(ta);
                                    TYPESYM ienumAts = Compiler.MainSymbolManager.GetInstAgg(
                                        ienumAgg,
                                        ta);
                                    lambdaExpr2.ParameterList[1].TypeSym = ienumAts;

                                    BindLambdaExpressionInner(lambdaExpr2.AnonymousMethodInfo);

                                    argTypeSym2 = lambdaExpr2.AnonymousMethodInfo.ReturnTypeSym;
                                    if (argTypeSym2 == null ||
                                        argTypeSym2.Kind == SYMKIND.IMPLICITTYPESYM)
                                    {
                                        return false;
                                    }
                                    break;

                                default:
                                    return false;
                            }
                            break;

                        //--------------------------------------------
                        // keySelector is a lambda expression
                        //--------------------------------------------
                        case EXPRKIND.ANONMETH:
                            if (currentMethodSym.TypeVariables.Count != 3)
                            {
                                return false;
                            }

                            EXPRANONMETH anonExpr2 = argExpr2 as EXPRANONMETH;
                            DebugUtil.Assert(anonExpr2 != null);
                            AnonMethInfo anonInfo = anonExpr2.AnonymousMethodInfo;

                            if (anonInfo.ParameterArray.Count != 1 &&
                                anonInfo.ParameterArray.Count != 2)
                            {
                                return false;
                            }
                            argTypeSym2 = GetAnonymousMethodReturnType(anonInfo.ReturnExprList);
                            break;

                        //--------------------------------------------
                        // Otherwise, IEqualityComparer<TKey>.
                        //--------------------------------------------
                        default:
                            if (currentMethodSym.TypeVariables.Count != 2)
                            {
                                return false;
                            }
                            argTypeSym2 = argExpr2.TypeSym;
                            if (argTypeSym2 == null ||
                                argTypeSym2.Kind == SYMKIND.IMPLICITTYPESYM)
                            {
                                return false;
                            }

                            inferredTypeArgs = new TypeArray();
                            inferredTypeArgs.Add(srcTypeSym);
                            inferredTypeArgs.Add(keyTypeSym);
                            inferredTypeArgs
                                = Compiler.MainSymbolManager.AllocParams(inferredTypeArgs);
                            typeArguments = inferredTypeArgs;
                            return true;
                    }

                    //------------------------------------------------
                    // Return the type arguments.
                    //------------------------------------------------
                    inferredTypeArgs = new TypeArray();
                    inferredTypeArgs.Add(srcTypeSym);
                    inferredTypeArgs.Add(keyTypeSym);
                    inferredTypeArgs.Add(argTypeSym2);
                    inferredTypeArgs
                        = Compiler.MainSymbolManager.AllocParams(inferredTypeArgs);
                    typeArguments = inferredTypeArgs;
                    return true;

                //----------------------------------------------------
                // 4, 5 arguments
                //----------------------------------------------------
                // public static IEnumerable<IGrouping<TKey, TElement>>
                // GroupBy<TSource, TKey, TElement>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     Func<TSource, TElement> elementSelector,
                //     IEqualityComparer<TKey> comparer
                // )
                //
                // public static IEnumerable<TResult>
                // GroupBy<TSource, TKey, TResult>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
                //     IEqualityComparer<TKey> comparer
                // )
                //
                // public static IEnumerable<TResult>
                // GroupBy<TSource, TKey, TElement, TResult>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     Func<TSource, TElement> elementSelector,
                //     Func<TKey, IEnumerable<TElement>, TResult> resultSelector
                // )
                //----------------------------------------------------
                case 4:
                case 5:
                    //------------------------------------------------
                    // argExpr2
                    //------------------------------------------------
                    switch (argExpr2.Kind)
                    {
                        case EXPRKIND.LAMBDAEXPR:
                            EXPRLAMBDAEXPR lambdaArgExpr2 = argExpr2 as EXPRLAMBDAEXPR;
                            switch (lambdaArgExpr2.ParameterList.Count)
                            {
                                case 1:
                                    lambdaArgExpr2.ParameterList[0].TypeSym = srcTypeSym;
                                    break;

                                case 2:
                                    lambdaArgExpr2.ParameterList[0].TypeSym = keyTypeSym;
                                    lambdaArgExpr2.ParameterList[1].TypeSym
                                        = CreateIEnumerableType(srcTypeSym);
                                    break;

                                default:
                                    return false;
                            }

                            BindLambdaExpressionInner(lambdaArgExpr2.AnonymousMethodInfo);
                            argTypeSym2 = lambdaArgExpr2.AnonymousMethodInfo.ReturnTypeSym;
                            break;

                        case EXPRKIND.ANONMETH:
                            EXPRANONMETH anonMethExpr2 = argExpr2 as EXPRANONMETH;
                            DebugUtil.Assert(anonMethExpr2 != null);
                            AnonMethInfo anonInfo2 = anonMethExpr2.AnonymousMethodInfo;

                            argTypeSym2 = GetAnonymousMethodReturnType(anonInfo2.ReturnExprList);
                            break;

                        default:
                            return false;
                    }

                    //------------------------------------------------
                    // argExpr3
                    //------------------------------------------------
                    switch (argExpr3.Kind)
                    {
                        case EXPRKIND.LAMBDAEXPR:
                            if (currentMethodSym.TypeVariables.Count != 4)
                            {
                                return false;
                            }

                            EXPRLAMBDAEXPR lambdaArgExpr3 = argExpr3 as EXPRLAMBDAEXPR;
                            DebugUtil.Assert(lambdaArgExpr3 != null);
                            if (lambdaArgExpr3.ParameterList.Count != 2)
                            {
                                return false;
                            }
                            lambdaArgExpr3.ParameterList[0].TypeSym = keyTypeSym;
                            lambdaArgExpr3.ParameterList[1].TypeSym
                                = CreateIEnumerableType(argTypeSym2);
                            BindLambdaExpressionInner(lambdaArgExpr3.AnonymousMethodInfo);
                            argTypeSym3 = lambdaArgExpr3.AnonymousMethodInfo.ReturnTypeSym;
                            break;

                        case EXPRKIND.ANONMETH:
                            if (currentMethodSym.TypeVariables.Count != 4)
                            {
                                return false;
                            }
                            EXPRANONMETH anonMethExpr3 = argExpr3 as EXPRANONMETH;
                            DebugUtil.Assert(anonMethExpr3 != null);
                            AnonMethInfo anonInfo3 = anonMethExpr3.AnonymousMethodInfo;

                            argTypeSym3 = GetAnonymousMethodReturnType(anonInfo3.ReturnExprList);
                            break;

                        default:
                            if (currentMethodSym.TypeVariables.Count != 3)
                            {
                                return false;
                            }
                            if (argCount != 4)
                            {
                                return false;
                            }
                            inferredTypeArgs = new TypeArray();
                            inferredTypeArgs.Add(srcTypeSym);
                            inferredTypeArgs.Add(keyTypeSym);
                            inferredTypeArgs.Add(argTypeSym2);
                            inferredTypeArgs
                                = Compiler.MainSymbolManager.AllocParams(inferredTypeArgs);
                            typeArguments = inferredTypeArgs;
                            return true;
                    }

                    //------------------------------------------------
                    // Return the type arguments.
                    //------------------------------------------------
                    inferredTypeArgs = new TypeArray();
                    inferredTypeArgs.Add(srcTypeSym);
                    inferredTypeArgs.Add(keyTypeSym);
                    inferredTypeArgs.Add(argTypeSym2);
                    inferredTypeArgs.Add(argTypeSym3);
                    inferredTypeArgs
                        = Compiler.MainSymbolManager.AllocParams(inferredTypeArgs);
                    typeArguments = inferredTypeArgs;
                    return true;

                //----------------------------------------------------
                // 5 arguments -> processed in 4 argurments case.
                //----------------------------------------------------
                // public static IEnumerable<TResult>
                // GroupBy<TSource, TKey, TElement, TResult>(
                //     this IEnumerable<TSource> source,
                //     Func<TSource, TKey> keySelector,
                //     Func<TSource, TElement> elementSelector,
                //     Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
                //     IEqualityComparer<TKey> comparer
                // )
                //----------------------------------------------------
                //case 5:
                //    break;

                default:
                    break;
            }
            return false;
        }

        //--------------------------------------------------
        // FUNCBREC.InferTypeLinqEnumerableJoin
        //
        /// <summary></summary>
        /// <param name="grpExpr"></param>
        /// <param name="argInfos"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal bool InferTypeLinqEnumerableJoin(
            EXPRMEMGRP grpExpr,
            METHSYM currentMethodSym,
            ArgInfos argInfos,
            bool isGroupJoin,
            ref TypeArray typeArguments)
        {
            // public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            //     this IEnumerable<TOuter> outer,
            //     IEnumerable<TInner> inner,
            //     Func<TOuter, TKey> outerKeySelector,
            //     Func<TInner, TKey> innerKeySelector,
            //     Func<TOuter, TInner, TResult> resultSelector
            // )
            //
            // public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            //     this IEnumerable<TOuter> outer,
            //     IEnumerable<TInner> inner,
            //     Func<TOuter, TKey> outerKeySelector,
            //     Func<TInner, TKey> innerKeySelector,
            //     Func<TOuter, TInner, TResult> resultSelector,
            //     IEqualityComparer<TKey> comparer
            // )

            // public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            //     this IEnumerable<TOuter> outer,
            //     IEnumerable<TInner> inner,
            //     Func<TOuter, TKey> outerKeySelector,
            //     Func<TInner, TKey> innerKeySelector,
            //     Func<TOuter, IEnumerable<TInner>, TResult> resultSelector
            // )
            //
            // public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            //     this IEnumerable<TOuter> outer,
            //     IEnumerable<TInner> inner,
            //     Func<TOuter, TKey> outerKeySelector,
            //     Func<TInner, TKey> innerKeySelector,
            //     Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            //     IEqualityComparer<TKey> comparer
            // )

            EXPR outerColExpr = grpExpr.ObjectExpr;
            EXPR innerColExpr = null;
            TYPESYM outerColTypeSym = null;
            TYPESYM innerColTypeSym = null;
            EXPR outerKeyExpr = null;
            EXPR innerKeyExpr = null;
            EXPR resultExpr = null;

            if (outerColExpr != null)
            {
                if (argInfos.ExprList.Count != 4 &&
                    argInfos.ExprList.Count != 5)
                {
                    return false;
                }
                outerColTypeSym = outerColExpr.TypeSym;

                innerColExpr = argInfos.ExprList[0];
                innerColTypeSym = innerColExpr.TypeSym;

                outerKeyExpr = argInfos.ExprList[1];
                innerKeyExpr = argInfos.ExprList[2];
                resultExpr = argInfos.ExprList[3];
            }
            else
            {
                if (argInfos.ExprList.Count != 5 &&
                    argInfos.ExprList.Count != 6)
                {
                    return false;
                }
                outerColExpr = argInfos.ExprList[0];
                outerColTypeSym = argInfos.ArgumentTypes[0];

                innerColExpr = argInfos.ExprList[1];
                innerColTypeSym = innerColExpr.TypeSym;

                outerKeyExpr = argInfos.ExprList[2];
                innerKeyExpr = argInfos.ExprList[3];
                resultExpr = argInfos.ExprList[4];
            }
            if (outerKeyExpr == null)
            {
                return false;
            }

            //--------------------------------------------------------
            // TOuter
            //--------------------------------------------------------
            TYPESYM outerSrcTypeSym = GetElementType(
                null,
                outerColExpr.TreeNode,
                outerColTypeSym,
                outerColExpr);
            if (outerSrcTypeSym == null ||
                outerSrcTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // TInner
            //--------------------------------------------------------
            TYPESYM innerSrcTypeSym = GetElementType(
                null,
                innerColExpr.TreeNode,
                innerColTypeSym,
                innerColExpr);
            if (innerSrcTypeSym == null ||
                innerSrcTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // TKey
            //--------------------------------------------------------
            TYPESYM outerKeyTypeSym = null;
            TYPESYM innerKeyTypeSym = null;

            switch (outerKeyExpr.Kind)
            {
                case EXPRKIND.LAMBDAEXPR:
                    EXPRLAMBDAEXPR outerKeyLambdaExpr = outerKeyExpr as EXPRLAMBDAEXPR;
                    DebugUtil.Assert(outerKeyLambdaExpr != null);

                    outerKeyLambdaExpr.ParameterList[0].TypeSym = outerSrcTypeSym;
                    BindLambdaExpressionInner(outerKeyLambdaExpr.AnonymousMethodInfo);

                    outerKeyTypeSym = outerKeyLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
                    if (outerKeyTypeSym == null ||
                        outerKeyTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        return false;
                    }

                    break;

                case EXPRKIND.ANONMETH:
                    EXPRANONMETH outerKeyAnonExpr = outerKeyExpr as EXPRANONMETH;
                    DebugUtil.Assert(outerKeyAnonExpr != null);
                    AnonMethInfo outerAnonInfo = outerKeyAnonExpr.AnonymousMethodInfo;

                    outerKeyTypeSym = GetAnonymousMethodReturnType(outerAnonInfo.ReturnExprList);
                    break;

                default:
                    return false;
            }

            switch (innerKeyExpr.Kind)
            {
                case EXPRKIND.LAMBDAEXPR:
                    EXPRLAMBDAEXPR innerKeyLambdaExpr = innerKeyExpr as EXPRLAMBDAEXPR;
                    DebugUtil.Assert(innerKeyLambdaExpr != null);

                    innerKeyLambdaExpr.ParameterList[0].TypeSym = innerSrcTypeSym;
                    BindLambdaExpressionInner(innerKeyLambdaExpr.AnonymousMethodInfo);

                    innerKeyTypeSym = innerKeyLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
                    if (innerKeyTypeSym == null ||
                        innerKeyTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        return false;
                    }
                    break;

                case EXPRKIND.ANONMETH:
                    EXPRANONMETH innerKeyAnonExpr = innerKeyExpr as EXPRANONMETH;
                    DebugUtil.Assert(innerKeyAnonExpr != null);
                    AnonMethInfo innerAnonInfo = innerKeyAnonExpr.AnonymousMethodInfo;

                    innerKeyTypeSym = GetAnonymousMethodReturnType(innerAnonInfo.ReturnExprList);
                    break;

                default:
                    return false;
            }
            if (innerKeyTypeSym != outerKeyTypeSym)
            {
                return false;
            }

            //--------------------------------------------------------
            // TResult
            //--------------------------------------------------------
            TYPESYM resultTypeSym = null;

            switch (resultExpr.Kind)
            {
                case EXPRKIND.LAMBDAEXPR:
                    EXPRLAMBDAEXPR resultLambdaExpr = resultExpr as EXPRLAMBDAEXPR;
                    DebugUtil.Assert(resultLambdaExpr != null);

                    resultLambdaExpr.ParameterList[0].TypeSym = outerSrcTypeSym;
                    if (isGroupJoin)
                    {
                        AGGSYM ienumSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G_IENUMERABLE, true);
                        TypeArray ta = new TypeArray();
                        ta.Add(innerSrcTypeSym);
                        ta = Compiler.MainSymbolManager.AllocParams(ta);
                        resultLambdaExpr.ParameterList[1].TypeSym
                            = Compiler.MainSymbolManager.GetInstAgg(ienumSym, ta);
                    }
                    else
                    {
                        resultLambdaExpr.ParameterList[1].TypeSym = innerSrcTypeSym;
                    }
                    BindLambdaExpressionInner(resultLambdaExpr.AnonymousMethodInfo);

                    resultTypeSym = resultLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
                    if (resultTypeSym == null ||
                        resultTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                    {
                        return false;
                    }
                    break;

                case EXPRKIND.ANONMETH:
                    EXPRANONMETH resultAnonExpr = resultExpr as EXPRANONMETH;
                    DebugUtil.Assert(resultAnonExpr != null);
                    AnonMethInfo innerAnonInfo = resultAnonExpr.AnonymousMethodInfo;

                    resultTypeSym = GetAnonymousMethodReturnType(innerAnonInfo.ReturnExprList);
                    break;

                default:
                    return false;
            }

            //--------------------------------------------------------
            // Return the type arguments.
            //--------------------------------------------------------
            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(outerSrcTypeSym);
            typeArgs.Add(innerSrcTypeSym);
            typeArgs.Add(outerKeyTypeSym);
            typeArgs.Add(resultTypeSym);
            typeArguments = Compiler.MainSymbolManager.AllocParams(typeArgs);
            return true;
        }

        //--------------------------------------------------
        // FUNCBREC.InferTypeLinqEnumerableSelect
        //
        /// <summary></summary>
        /// <param name="grpExpr"></param>
        /// <param name="argInfos"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal bool InferTypeLinqEnumerableSelect(
            EXPRMEMGRP grpExpr,
            METHSYM currentMethodSym,
            ArgInfos argInfos,
            bool isSelectMany,
            ref TypeArray typeArguments)
        {
            // public static IEnumerable<TResult> Select<TSource, TResult>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, TResult> selector
            // )
            //
            // public static IEnumerable<TResult> Select<TSource, TResult>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, int, TResult> selector
            // )

            //--------------------------------------------------------
            // TSource
            //--------------------------------------------------------
            TYPESYM collectionTypeSym = null;
            EXPR collectionExpr = grpExpr.ObjectExpr;
            EXPR resExpr = null;
            TYPESYM resTypeSym = null;

            if (collectionExpr != null)
            {
                if (argInfos.ExprList.Count != 1)
                {
                    return false;
                }
                collectionTypeSym = collectionExpr.TypeSym;
                resExpr = argInfos.ExprList[0];
            }
            else
            {
                if (argInfos.ExprList.Count != 2)
                {
                    return false;
                }
                collectionExpr = argInfos.ExprList[0];
                collectionTypeSym = argInfos.ArgumentTypes[0];
                resExpr = argInfos.ExprList[1];
            }
            if (resExpr == null)
            {
                return false;
            }

            TYPESYM srcTypeSym = GetElementType(
                null,
                collectionExpr.TreeNode,
                collectionTypeSym,
                collectionExpr);
            if (srcTypeSym == null ||
                srcTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // TResult (1) Lambda Expression
            //--------------------------------------------------------
            if (resExpr.Kind == EXPRKIND.LAMBDAEXPR)
            {
                EXPRLAMBDAEXPR lambdaExpr = resExpr as EXPRLAMBDAEXPR;

                switch (lambdaExpr.ParameterList.Count)
                {
                    case 1:
                        lambdaExpr.ParameterList[0].TypeSym = srcTypeSym;
                        break;

                    case 2:
                        lambdaExpr.ParameterList[0].TypeSym = srcTypeSym;
                        lambdaExpr.ParameterList[1].TypeSym
                            = Compiler.GetOptPredefAgg(PREDEFTYPE.INT, true).GetThisType();
                        break;

                    default:
                        return false;
                }

                EXPRLAMBDAEXPR newLambdaExpr = BindLambdaExpressionInner(
                    lambdaExpr.AnonymousMethodInfo) as EXPRLAMBDAEXPR;
                newLambdaExpr.TypeSym = newLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;

                if (isSelectMany)
                {
                    resTypeSym = GetElementType(
                        null,
                        newLambdaExpr.TreeNode,
                        lambdaExpr.AnonymousMethodInfo.ReturnTypeSym,
                        newLambdaExpr);
                }
                else
                {
                    resTypeSym = lambdaExpr.AnonymousMethodInfo.ReturnTypeSym;
                }
            }
            //--------------------------------------------------------
            // TResult (2) Anonymous method
            //--------------------------------------------------------
            else if (resExpr.Kind == EXPRKIND.ANONMETH)
            {
                EXPRANONMETH anonMethExpr = resExpr as EXPRANONMETH;
                DebugUtil.Assert(anonMethExpr != null);
                AnonMethInfo anonInfo = anonMethExpr.AnonymousMethodInfo;

                resTypeSym = GetAnonymousMethodReturnType(anonInfo.ReturnExprList);
            }
            //--------------------------------------------------------
            // TResult (3) Otherwise, return false
            //--------------------------------------------------------
            else
            {
                return false;
            }

            if (resTypeSym == null ||
                resTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // Set the type arguments
            //--------------------------------------------------------
            TypeArray ta = new TypeArray();
            ta.Add(srcTypeSym);
            ta.Add(resTypeSym);
            ta = Compiler.MainSymbolManager.AllocParams(ta);
            typeArguments = ta;
            return true;
        }

        //--------------------------------------------------
        // FUNCBREC.InferTypeLinqEnumerableSelectMany
        //
        /// <summary></summary>
        /// <param name="grpExpr"></param>
        /// <param name="argInfos"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal bool InferTypeLinqEnumerableSelectMany(
            EXPRMEMGRP grpExpr,
            METHSYM currentMethodSym,
            ArgInfos argInfos,
            ref TypeArray typeArguments)
        {
            //--------------------------------------------------------
            // public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, IEnumerable<TResult>> selector
            // )
            //
            // public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, int, IEnumerable<TResult>> selector
            // )
            //--------------------------------------------------------
            if ((grpExpr.ObjectExpr != null && argInfos.ExprList.Count == 1) ||
                (grpExpr.ObjectExpr == null && argInfos.ExprList.Count == 2))
            {
                return InferTypeLinqEnumerableSelect(
                    grpExpr,
                    currentMethodSym,
                    argInfos,
                    true,
                    ref typeArguments);
            }
            //--------------------------------------------------------
            // public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, IEnumerable<TCollection>> collectionSelector,
            //     Func<TSource, TCollection, TResult> resultSelector
            // )
            //
            // public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            //     Func<TSource, TCollection, TResult> resultSelector
            // )
            //--------------------------------------------------------
            TYPESYM srcColTypeSym = null;
            EXPR srcColExpr = grpExpr.ObjectExpr;
            EXPRLAMBDAEXPR colLambdaExpr = null;
            EXPRLAMBDAEXPR resLambdaExpr = null;

            if (srcColExpr != null)
            {
                if (argInfos.ExprList.Count != 2)
                {
                    return false;
                }
                srcColTypeSym = srcColExpr.TypeSym;
                colLambdaExpr = argInfos.ExprList[0] as EXPRLAMBDAEXPR;
                resLambdaExpr = argInfos.ExprList[1] as EXPRLAMBDAEXPR;
            }
            else
            {
                if (argInfos.ExprList.Count != 3)
                {
                    return false;
                }
                srcColExpr = argInfos.ExprList[0];
                srcColTypeSym = argInfos.ArgumentTypes[0];
                colLambdaExpr = argInfos.ExprList[1] as EXPRLAMBDAEXPR;
                resLambdaExpr = argInfos.ExprList[2] as EXPRLAMBDAEXPR;
            }
            if (resLambdaExpr == null)
            {
                return false;
            }

            //--------------------------------------------------------
            // TSource
            //--------------------------------------------------------
            TYPESYM srcElemTypeSym = GetElementType(
                null,
                srcColExpr.TreeNode,
                srcColTypeSym,
                srcColExpr);
            if (srcElemTypeSym == null ||
                srcElemTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // TCollection
            //--------------------------------------------------------
            switch (colLambdaExpr.ParameterList.Count)
            {
                case 1:
                    colLambdaExpr.ParameterList[0].TypeSym = srcElemTypeSym;
                    break;

                case 2:
                    colLambdaExpr.ParameterList[0].TypeSym = srcElemTypeSym;
                    colLambdaExpr.ParameterList[1].TypeSym
                        = Compiler.GetOptPredefAgg(PREDEFTYPE.INT, true).GetThisType();
                    break;

                default:
                    return false;
            }

            EXPRLAMBDAEXPR newColLambdaExpr = BindLambdaExpressionInner(
                colLambdaExpr.AnonymousMethodInfo) as EXPRLAMBDAEXPR;
            newColLambdaExpr.TypeSym = newColLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;

            TYPESYM colTypeSym = GetElementType(
                null,
                newColLambdaExpr.TreeNode,
                newColLambdaExpr.AnonymousMethodInfo.ReturnTypeSym,
                newColLambdaExpr);

            if (colTypeSym == null ||
                colTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // TResult
            //--------------------------------------------------------
            if (resLambdaExpr.ParameterList.Count != 2)
            {
                return false;
            }
            resLambdaExpr.ParameterList[0].TypeSym = srcElemTypeSym;
            resLambdaExpr.ParameterList[1].TypeSym = colTypeSym;

            EXPRLAMBDAEXPR newResLambdaExpr = BindLambdaExpressionInner(
                resLambdaExpr.AnonymousMethodInfo) as EXPRLAMBDAEXPR;
            newResLambdaExpr.TypeSym = newResLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;

            TYPESYM resTypeSym
                = resLambdaExpr.AnonymousMethodInfo.ReturnTypeSym;

            if (resTypeSym == null ||
                resTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            //--------------------------------------------------------
            // Create the type array.
            //--------------------------------------------------------
            TypeArray ta = new TypeArray();
            ta.Add(srcElemTypeSym);
            ta.Add(colTypeSym);
            ta.Add(resTypeSym);
            ta = Compiler.MainSymbolManager.AllocParams(ta);
            typeArguments = ta;
            return true;
        }

        //--------------------------------------------------
        // FUNCBREC.InferTypeLinqEnumerableWhere
        //
        /// <summary></summary>
        /// <param name="grpExpr"></param>
        /// <param name="argInfos"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal bool InferTypeLinqEnumerableWhere(
            EXPRMEMGRP grpExpr,
            METHSYM currentMethodSym,
            ArgInfos argInfos,
            ref TypeArray typeArguments)
        {
            // public static IEnumerable<TSource> Where<TSource>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, bool> predicate
            // )
            //
            // public static IEnumerable<TSource> Where<TSource>(
            //     this IEnumerable<TSource> source,
            //     Func<TSource, int, bool> predicate
            // )

            if (currentMethodSym.TypeVariables.Count != 1)
            {
                return false;
            }

            TYPESYM collectionTypeSym = null;
            EXPR collectionExpr = grpExpr.ObjectExpr;

            if (collectionExpr != null)
            {
                if (argInfos.ExprList.Count != 1)
                {
                    return false;
                }
                collectionTypeSym = collectionExpr.TypeSym;
            }
            else
            {
                if (argInfos.ExprList.Count != 2)
                {
                    return false;
                }
                collectionExpr = argInfos.ExprList[0];
                collectionTypeSym = argInfos.ArgumentTypes[0];
            }

            TYPESYM srcTypeSym = GetElementType(
                null,
                collectionExpr.TreeNode,
                collectionTypeSym,
                collectionExpr);
            if (srcTypeSym == null ||
                srcTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                return false;
            }

            TypeArray ta = new TypeArray();
            ta.Add(srcTypeSym);
            ta = Compiler.MainSymbolManager.AllocParams(ta);
            typeArguments = ta;
            return true;
        }

        //--------------------------------------------------
        // FUNCBREC.GetAnonymousMethodReturnType
        //
        /// <summary></summary>
        /// <param name="returnExprList"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal TYPESYM GetAnonymousMethodReturnType(EXPR returnExprList)
        {
            EXPR expr = returnExprList;
            TYPESYM retTypeSym = null;
            TYPESYM retTypeSym2 = null;

            while (expr != null)
            {
                EXPRRETURN returnExpr;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    returnExpr = expr.AsBIN.Operand1 as EXPRRETURN;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    returnExpr = expr as EXPRRETURN;
                    expr = null;
                }

                if (returnExpr == null)
                {
                    continue;
                }
                if (returnExpr.ObjectExpr == null)
                {
                    return null;
                }

                retTypeSym2 = returnExpr.ObjectExpr.TypeSym;
                if (retTypeSym2 == null ||
                    retTypeSym2.Kind == SYMKIND.IMPLICITTYPESYM)
                {
                    continue;
                }

                if (retTypeSym == null)
                {
                    retTypeSym = retTypeSym2;
                    continue;
                }

                if (CanConvert(retTypeSym2, retTypeSym, 0))
                {
                    continue;
                }
                else if (CanConvert(retTypeSym, retTypeSym2, 0))
                {
                    retTypeSym = retTypeSym2;
                    continue;
                }
                else
                {
                    // return types are inconsistent.
                    return null;
                }
            }
            return retTypeSym;
        }

        //--------------------------------------------------
        // FUNCBREC.CreateIEnumerableType
        //
        /// <summary></summary>
        /// <param name="elementTypeSym"></param>
        /// <returns></returns>
        //--------------------------------------------------
        internal TYPESYM CreateIEnumerableType(TYPESYM elementTypeSym)
        {
            if (ienumerableAggSym == null)
            {
                ienumerableAggSym = Compiler.GetOptPredefAgg(PREDEFTYPE.G_IENUMERABLE, true);
            }
            DebugUtil.Assert(ienumerableAggSym != null);

            TypeArray ta = new TypeArray();
            ta.Add(elementTypeSym);
            ta = Compiler.MainSymbolManager.AllocParams(ta);

            return Compiler.MainSymbolManager.GetInstAgg(ienumerableAggSym, ta);
        }

        private static AGGSYM ienumerableAggSym = null;
    }
}

