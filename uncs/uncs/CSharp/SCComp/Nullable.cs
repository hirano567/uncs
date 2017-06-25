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
// File: fncbind.h
//
// Defines the structure which contains information necessary to bind and generate
// code for a single function.
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
// File: fncpbind.cpp
//
// Routines for analyzing the bound body of a function
// ===========================================================================

//============================================================================
// Nullable.cs
//     (Separated from FncBind.cs)
//
// 2015/07/18 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // delegate FUNCBREC.bindNubConversion_Convert
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="srcExpr"></param>
        /// <param name="srcTypeSym"></param>
        /// <param name="dstAggTypeSym"></param>
        /// <param name="dstExpr"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected delegate bool bindNubConversion_Convert(
            BASENODE treeNode,
            EXPR srcExpr,
            TYPESYM srcTypeSym,
            TYPESYM dstAggTypeSym,
            ref EXPR dstExpr,
            ConvertTypeEnum flags);

        //------------------------------------------------------------
        // FUNCBREC.BindNubCondValBin
        //
        /// <summary>
        /// <para>Fill in the NubInfo for a unary or binary operator lifting.</para>
        /// <para>(In sscli, liftFlags has default value LiftFlags::LiftBoth.)</para>
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <param name="nubInfo"></param>
        /// <param name="liftFlags"></param>
        //------------------------------------------------------------
        private void BindNubCondValBin(
            BASENODE treeNode,
            EXPR expr1,
            EXPR expr2,
            ref NubInfo nubInfo,
            LiftFlagsEnum liftFlags)    // = LiftFlags::LiftBoth
        {
            DebugUtil.Assert((liftFlags & (LiftFlagsEnum.Lift1 | LiftFlagsEnum.Lift2)) != 0);
            DebugUtil.Assert(expr1 != null);
            DebugUtil.Assert(expr2 != null || (liftFlags & LiftFlagsEnum.Lift1) != 0);

            nubInfo.Init();

            bool[] rgfLift = { false, false };
            rgfLift[0] = ((liftFlags & LiftFlagsEnum.Lift1) != 0);
            rgfLift[1] = expr2 != null && ((liftFlags & LiftFlagsEnum.Lift2) != 0);

            EXPR[] rgexpr = { null, null };
            rgexpr[0] = rgfLift[0] ? StripNubCtor(expr1) : expr1;
            rgexpr[1] = rgfLift[1] ? StripNubCtor(expr2) : expr2;

            if ((!rgfLift[0] || !rgexpr[0].TypeSym.IsNUBSYM) && (!rgfLift[1] || !rgexpr[1].TypeSym.IsNUBSYM))
            {
                // All lifted params can't be null so we don't need temps.
                nubInfo.ValueExpr[0] = rgexpr[0];
                nubInfo.ValueExpr[1] = rgexpr[1];
                nubInfo.IsActive = true;
                nubInfo.IsAlwaysNonNull = true;
                return;
            }

            // Optimization: if they are the same local then we only need one temp.
            if (rgexpr[1] != null &&
                rgexpr[0].Kind == EXPRKIND.LOCAL &&
                rgexpr[1].Kind == EXPRKIND.LOCAL &&
                (rgexpr[0] as EXPRLOCAL).LocVarSym == (rgexpr[1] as EXPRLOCAL).LocVarSym &&
                rgfLift[0] == rgfLift[1])
            {
                BindNubSave(rgexpr[0], ref nubInfo, 0, rgfLift[0]);
                DebugUtil.Assert(nubInfo.TmpExpr[0].Kind == EXPRKIND.LDTMP);
                nubInfo.TmpExpr[1] = nubInfo.TmpExpr[0];
                nubInfo.IsSameTemp = true;
            }
            else
            {
                BindNubSave(rgexpr[0], ref nubInfo, 0, rgfLift[0]);
                if (rgexpr[1] != null)
                {
                    BindNubSave(rgexpr[1], ref nubInfo, 1, rgfLift[1]);
                }
            }

            for (int iexpr = 0; iexpr < 2 && rgexpr[iexpr] != null; iexpr++)
            {
                if (!rgfLift[iexpr] || !nubInfo.TmpExpr[iexpr].TypeSym.IsNUBSYM)
                {
                    nubInfo.ValueExpr[iexpr] = nubInfo.TmpExpr[iexpr];
                    continue;
                }
                nubInfo.ValueExpr[iexpr] = BindNubGetValOrDef(treeNode, nubInfo.TmpExpr[iexpr]);
                DebugUtil.Assert(!nubInfo.ValueExpr[iexpr].TypeSym.IsNUBSYM);
                if (nubInfo.FConst(iexpr))
                {
                    nubInfo.IsNull[iexpr] = nubInfo.IsAlwaysNull = true;
                }
                else if (nubInfo.IsSameTemp && iexpr != 0)
                {
                    nubInfo.ConditionExpr[iexpr] = nubInfo.ConditionExpr[0];
                }
                else
                {
                    nubInfo.ConditionExpr[iexpr] = BindNubHasValue(treeNode, nubInfo.TmpExpr[iexpr], true);
                }
            }

            if (!nubInfo.IsAlwaysNull)
            {
                if (nubInfo.ConditionExpr[0] == null)
                {
                    nubInfo.CombinedConditionExpr = nubInfo.ConditionExpr[1];
                }
                else if (nubInfo.ConditionExpr[1] == null || nubInfo.IsSameTemp)
                {
                    nubInfo.CombinedConditionExpr = nubInfo.ConditionExpr[0];
                }
                else
                {
                    nubInfo.CombinedConditionExpr = NewExprBinop(
                        treeNode,
                        EXPRKIND.BITAND,
                        GetRequiredPredefinedType(PREDEFTYPE.BOOL),
                        nubInfo.ConditionExpr[0],
                        nubInfo.ConditionExpr[1]);
                }
            }
            else
            {
                // One of the operands is null so the result will always be null and we
                // don't need the temps.
                if (nubInfo.PreExpr[0] != null && nubInfo.PreExpr[0].Kind == EXPRKIND.STTMP)
                {
                    nubInfo.PreExpr[0] = (nubInfo.PreExpr[0] as EXPRSTTMP).SourceExpr;
                }
                if (nubInfo.PreExpr[1] != null && nubInfo.PreExpr[1].Kind == EXPRKIND.STTMP)
                {
                    nubInfo.PreExpr[1] = (nubInfo.PreExpr[1] as EXPRSTTMP).SourceExpr;
                }
                nubInfo.PostExpr[0] = null;
                nubInfo.PostExpr[1] = null;
            }

            nubInfo.IsActive = true;
            DebugUtil.Assert(nubInfo.CombinedConditionExpr != null || nubInfo.IsAlwaysNull);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubOpRes (1)
        //
        /// <summary>
        /// <para>Combine the condition and value.</para>
        /// <para>(In sscli, warOnNull has the default value false.)</para>
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="nubSym"></param>
        /// <param name="dstTypeSym"></param>
        /// <param name="valueExpr"></param>
        /// <param name="nubInfo"></param>
        /// <param name="warnOnNull"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubOpRes(
            BASENODE treeNode,
            NUBSYM nubSym,
            TYPESYM dstTypeSym,
            EXPR valueExpr,
            ref NubInfo nubInfo,
            bool warnOnNull)    // = false
        {
            if (nubInfo.FAlwaysNull() && warnOnNull)
            {
                Compiler.Error(treeNode, CSCERRID.WRN_AlwaysNull, new ErrArg(nubSym));
            }

            return BindNubOpRes(
                treeNode,
                dstTypeSym,
                valueExpr,
                NewExprZero(treeNode, dstTypeSym.IsNUBSYM ? dstTypeSym : nubSym),
                ref nubInfo);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubOpRes (2)
        //
        /// <summary>
        /// Combine the condition and value.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="dstTypeSym"></param>
        /// <param name="valueExpr"></param>
        /// <param name="nullExpr"></param>
        /// <param name="nubInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubOpRes(
            BASENODE treeNode,
            TYPESYM dstTypeSym,
            EXPR valueExpr,
            EXPR nullExpr,
            ref NubInfo nubInfo)
        {
            EXPR resExpr;

            nullExpr = MustConvert(nullExpr, dstTypeSym, 0);
            valueExpr = MustConvert(valueExpr, dstTypeSym, 0);

            if (nubInfo.FAlwaysNonNull())
            {
                // Don't need nullExpr and there aren't any temps.
                resExpr = valueExpr;
            }
            else if (nubInfo.FAlwaysNull())
            {
                // Don't need valueExpr but do need side effects.
                resExpr = BindNubAddTmps(treeNode, nullExpr, ref nubInfo);
            }
            else
            {
                DebugUtil.Assert(nubInfo.CombinedConditionExpr != null);
                resExpr = BindQMark(
                    treeNode,
                    nubInfo.CombinedConditionExpr,
                    MustConvert(valueExpr, dstTypeSym, 0),
                    nullExpr,
                    false);
                resExpr = BindNubAddTmps(treeNode, resExpr, ref nubInfo);
            }

            return resExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubAddTmps
        //
        /// <summary>
        /// Combines the pre and post expressions of the NubInfo with exprRes.
        /// The pre and post exprs are typically to store values to temps and free the temps.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="resExpr"></param>
        /// <param name="nubInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubAddTmps(BASENODE treeNode, EXPR resExpr, ref NubInfo nubInfo)
        {
            if (nubInfo.PostExpr[1] != null)
            {
                resExpr = NewExprBinop(treeNode, EXPRKIND.SEQREV, resExpr.TypeSym, resExpr, nubInfo.PostExpr[1]);
            }
            if (nubInfo.PostExpr[0] != null)
            {
                resExpr = NewExprBinop(treeNode, EXPRKIND.SEQREV, resExpr.TypeSym, resExpr, nubInfo.PostExpr[0]);
            }

            if (nubInfo.PreExpr[1] != null)
            {
                resExpr = NewExprBinop(treeNode, EXPRKIND.SEQUENCE, resExpr.TypeSym, nubInfo.PreExpr[1], resExpr);
            }
            if (nubInfo.PreExpr[0] != null)
            {
                resExpr = NewExprBinop(treeNode, EXPRKIND.SEQUENCE, resExpr.TypeSym, nubInfo.PreExpr[0], resExpr);
            }

            return resExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubSave
        //
        /// <summary>
        /// Fill in the given slot (iexpr) of the NubInfo appropriately.
        /// If exprSrc is a constant (possibly with side effects),
        /// put the side effects in nin.rgexprPre[iexpr] and the constant value in nin.rgexprVal[iexpr].
        /// Otherwise, construct an expr to save the value of exprSrc in a temp,
        /// an expr to load the temp and an expr to free the temp.
        /// Store these in the appropriate places in the NubInfo.
        /// If fLift is true, the value saved in the temp will have at most one level of Nullable.
        /// </summary>
        /// <param name="srcExpr"></param>
        /// <param name="nubInfo"></param>
        /// <param name="exprIndex"></param>
        /// <param name="fLift"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool BindNubSave(EXPR srcExpr, ref NubInfo nubInfo, int exprIndex, bool fLift)
        {
            DebugUtil.Assert(exprIndex == 0 || exprIndex == 1);
            DebugUtil.Assert(
                nubInfo.PreExpr[exprIndex] == null &&
                nubInfo.PostExpr[exprIndex] == null &&
                nubInfo.ValueExpr[exprIndex] == null &&
                nubInfo.TmpExpr[exprIndex] == null &&
                nubInfo.ConditionExpr[exprIndex] == null);

            // If we're lifting the expr and it's a new T?(t), just operate on t. The caller should have
            // already taken care of calling StripNubCtor!
            DebugUtil.Assert(!srcExpr.TypeSym.IsNUBSYM || !fLift || !IsNubCtor(srcExpr));

            // Check for an EXPRKIND.CONSTANT or EXPRKIND.ZEROINIT inside EXPRKIND.SEQUENCE and EXPRKIND.SEQREV exprs.
            EXPR constExpr = srcExpr.GetConst();
            if (constExpr != null)
            {
                DebugUtil.Assert(constExpr.Kind == EXPRKIND.CONSTANT || constExpr.Kind == EXPRKIND.ZEROINIT);
                if (constExpr != srcExpr)
                {
                    // Keep the side effects.
                    nubInfo.PreExpr[exprIndex] = srcExpr;
                }
                if (!constExpr.TypeSym.IsNUBSYM || !fLift)
                {
                    nubInfo.TmpExpr[exprIndex] = constExpr;
                }
                else
                {
                    nubInfo.TmpExpr[exprIndex] = NewExprZero(
                        srcExpr.TreeNode, constExpr.TypeSym.StripAllButOneNub());
                }
                return false;
            }

            if (srcExpr.TypeSym.IsNUBSYM && fLift)
            {
                while (srcExpr.TypeSym.ParentSym.IsNUBSYM)
                {
                    srcExpr = BindNubGetValOrDef(srcExpr.TreeNode, srcExpr);
                }
            }

            if (srcExpr.Kind == EXPRKIND.LDTMP)
            {
                // The thing is already in a temp, no need to put it in another.
                nubInfo.PreExpr[exprIndex] = srcExpr;
                nubInfo.TmpExpr[exprIndex] = srcExpr;
                return true;
            }

            // Create the temp.
            EXPRSTTMP tmpStExpr = NewExpr(srcExpr.TreeNode, EXPRKIND.STTMP, srcExpr.TypeSym) as EXPRSTTMP;
            EXPRLDTMP tmpLdExpr = NewExpr(srcExpr.TreeNode, EXPRKIND.LDTMP, srcExpr.TypeSym) as EXPRLDTMP;
            EXPRFREETMP tmpFreeExpr = NewExpr(srcExpr.TreeNode, EXPRKIND.FREETMP, srcExpr.TypeSym) as EXPRFREETMP;

            tmpStExpr.SourceExpr = srcExpr;
            tmpStExpr.Flags |= EXPRFLAG.ASSGOP;
            tmpLdExpr.TmpExpr = tmpStExpr;
            tmpFreeExpr.TmpExpr = tmpStExpr;
            tmpFreeExpr.Flags |= EXPRFLAG.ASSGOP;

            nubInfo.PreExpr[exprIndex] = tmpStExpr;
            nubInfo.PostExpr[exprIndex] = tmpFreeExpr;
            nubInfo.TmpExpr[exprIndex] = tmpLdExpr;

            return true;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubFetchAndFree
        //
        /// <summary>
        /// If the given slot has a temp associated with it,
        /// constructs a reverse sequence for loading the value and freeing the temp.
        /// Otherwise, just returns the value expr.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="nubInfo"></param>
        /// <param name="iexpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubFetchAndFree(BASENODE treeNode, ref NubInfo nubInfo, int iexpr)
        {
            EXPR expr = nubInfo.TmpExpr[iexpr];
            DebugUtil.Assert(expr != null);

            if (nubInfo.PostExpr[iexpr] == null)
            {
                return expr;
            }

            return AddSideEffects(treeNode, expr, nubInfo.PostExpr[iexpr], false, true);
        }

        //------------------------------------------------------------
        // FUNCBREC.AddSideEffects
        //
        /// <summary>
        /// <para>Add sideExpr to baseExpr as a side effect.</para>
        /// <para>If isPre is true, sideExpr is evaluated before baseExpr.</para>
        /// <para>If sideExpr is NULL or contains no side effects, just returns baseExpr.</para>
        /// <para>If no side effect, return baseExpr.
        /// Otherwise create and return an EXPRBINOP instance of
        /// whose operands are sideExpr and baseExpr.</para>
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="baseExpr"></param>
        /// <param name="sideExpr"></param>
        /// <param name="isPre"></param>
        /// <param name="forceNonConst"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR AddSideEffects(
            BASENODE treeNode,
            EXPR baseExpr,
            EXPR sideExpr,
            bool isPre,
            bool forceNonConst) // = false
        {
            if (sideExpr == null)
            {
                return baseExpr;
            }

            if (!sideExpr.HasSideEffects(Compiler))
            {
                // Make sure it's not an lvalue or constant (unless sideExpr is a constant and !forceNonConst).
                if (((baseExpr.Flags & EXPRFLAG.LVALUE) == 0) &&
                    (baseExpr.Kind != EXPRKIND.CONSTANT || sideExpr.Kind == EXPRKIND.CONSTANT && !forceNonConst))
                {
                    return baseExpr;
                }
                return NewExprBinop(
                    treeNode,
                    EXPRKIND.SEQUENCE,
                    baseExpr.TypeSym,
                    NewExpr(treeNode, EXPRKIND.NOOP, GetVoidType()),
                    baseExpr);
            }
            if (isPre)
            {
                return NewExprBinop(treeNode, EXPRKIND.SEQUENCE, baseExpr.TypeSym, sideExpr, baseExpr);
            }
            return NewExprBinop(treeNode, EXPRKIND.SEQREV, baseExpr.TypeSym, baseExpr, sideExpr);
        }

        //------------------------------------------------------------
        // FUNCBREC.EnsureNonConstNonLvalue
        //
        /// <summary>
        /// If exprBase is a constant or lvalue,
        /// sequence it with EK_NOOP so it doesn't appear to be either.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="exprBase"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR EnsureNonConstNonLvalue(BASENODE tree, EXPR exprBase)
        {
            if ((exprBase.Flags & EXPRFLAG.LVALUE) == 0 && exprBase.Kind != EXPRKIND.CONSTANT)
            {
                return exprBase;
            }
            return NewExprBinop(
                tree,
                EXPRKIND.SEQUENCE,
                exprBase.TypeSym,
                NewExpr(tree, EXPRKIND.NOOP, GetVoidType()),
                exprBase);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubConstBool
        //
        /// <summary>
        /// Return an expr for "new bool?(fT)".
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="fT"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubConstBool(BASENODE tree, bool fT)
        {
            EXPR expr = NewExprConstant(tree, GetRequiredPredefinedType(PREDEFTYPE.BOOL), new ConstValInit(fT));
            return BindNubNew(tree, expr);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindQMark
        //
        /// <summary>
        /// <para>Constructs an expr for "exprCond ? exprLeft : exprRight". If fInvert is true, reverses
        /// exprLeft and exprRight.</para>
        /// <para>(In sscli, fInvert has the default value false.)</para>
        /// </summary>
        /// <remarks>
        /// REVIEW ShonK: Optimize BindQmark when the condition is a constant (with possible side effects).
        /// </remarks>
        /// <param name="tree"></param>
        /// <param name="condExpr"></param>
        /// <param name="leftExpr"></param>
        /// <param name="rightExpr"></param>
        /// <param name="fInvert"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindQMark(
            BASENODE tree,
            EXPR condExpr,
            EXPR leftExpr,
            EXPR rightExpr,
            bool fInvert)   // =false
        {
            DebugUtil.Assert(condExpr.TypeSym.IsPredefType(PREDEFTYPE.BOOL));
            DebugUtil.Assert(leftExpr.TypeSym == rightExpr.TypeSym);

            EXPR colonExpr;

            if (fInvert)
            {
                colonExpr = NewExprBinop(tree, EXPRKIND.BINOP, null, rightExpr, leftExpr);
            }
            else
            {
                colonExpr = NewExprBinop(tree, EXPRKIND.BINOP, null, leftExpr, rightExpr);
            }

            return NewExprBinop(tree, EXPRKIND.QMARK, leftExpr.TypeSym, condExpr, colonExpr);
        }

        //------------------------------------------------------------
        // FUNCBREC.EnsureNubHasValue
        //
        /// <summary>
        /// Make sure the HasValue property of System.Nullable&lt;T&gt; is appropriate (and return it).
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private PROPSYM EnsureNubHasValue(BASENODE treeNode)
        {
            PROPSYM propSym = Compiler.MainSymbolManager.NullableHasValuePropertySym;

            if (propSym == null)
            {
                AGGSYM nubAggSym = Compiler.GetOptPredefAggErr(PREDEFTYPE.G_OPTIONAL, true);
                if (nubAggSym == null)
                {
                    return null;
                }
                string name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.HASVALUE);

                SYM sym = Compiler.MainSymbolManager.LookupAggMember(
                    name, nubAggSym, SYMBMASK.PROPSYM);
                propSym = sym as PROPSYM;
                if (propSym == null ||
                    propSym.IsStatic ||
                    propSym.Access != ACCESS.PUBLIC ||
                    propSym.ParameterTypes.Count > 0 ||
                    !propSym.ReturnTypeSym.IsPredefType(PREDEFTYPE.BOOL) ||
                    propSym.GetMethodSym == null)
                {
                    Compiler.Error(treeNode, CSCERRID.ERR_MissingPredefinedMember,
                        new ErrArg(nubAggSym), new ErrArg(name));
                    return null;
                }
                Compiler.MainSymbolManager.NullableHasValuePropertySym = propSym;
            }

            return propSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.EnsureNubGetValOrDef
        //
        /// <summary>
        /// Make sure the HasValue property of System.Nullable&lt;T&gt; is appropriate (and return it).
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private METHSYM EnsureNubGetValOrDef(BASENODE treeNode)
        {
            METHSYM methSym = Compiler.MainSymbolManager.NullableGetValOrDefMethodSym;

            if (methSym == null)
            {
                AGGSYM nubAggSym = Compiler.GetOptPredefAggErr(PREDEFTYPE.G_OPTIONAL, true);
                if (nubAggSym == null)
                {
                    return null;
                }
                string name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.GET_VALUE_OR_DEF);

                for (SYM sym = Compiler.MainSymbolManager.LookupAggMember(name, nubAggSym, SYMBMASK.ALL);
                    ;
                    sym = sym.NextSameNameSym)
                {
                    if (sym == null)
                    {
                        Compiler.Error(treeNode, CSCERRID.ERR_MissingPredefinedMember,
                            new ErrArg(nubAggSym), new ErrArg(name));
                        return null;
                    }
                    if (sym.IsMETHSYM)
                    {
                        methSym = sym as METHSYM;
                        if (methSym.ParameterTypes.Count == 0 &&
                            methSym.ParameterTypes.Count == 0 &&
                            methSym.ReturnTypeSym.IsTYVARSYM &&
                            !methSym.IsStatic &&
                            methSym.Access == ACCESS.PUBLIC)
                        {
                            break;
                        }
                    }
                }
                Compiler.MainSymbolManager.NullableGetValOrDefMethodSym = methSym;
            }

            return methSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubHasValue
        //
        /// <summary>
        /// <para>Create an expr for exprSrc.HasValue where exprSrc->type is a NUBSYM.</para>
        /// <para>If fCheckTrue is false, invert the result.</para>
        /// <para>(In sscli, checkTrue has the default value true.)</para>
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="srcExpr"></param>
        /// <param name="checkTrue"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubHasValue(
            BASENODE treeNode,
            EXPR srcExpr,
            bool checkTrue) // = true
        {
            DebugUtil.Assert(srcExpr != null && srcExpr.TypeSym.IsNUBSYM);

            TYPESYM boolTypeSym = GetRequiredPredefinedType(PREDEFTYPE.BOOL);

            // When srcExpr is a null, the result is false
            if (srcExpr.GetConst() != null)
            {
                return AddSideEffects(
                    treeNode,
                    NewExprConstant(treeNode, boolTypeSym, new ConstValInit(!checkTrue)),
                    srcExpr,
                    true,
                    true);
            }

            // For new T?(x), the answer is true.
            if (IsNubCtor(srcExpr))
            {
                return AddSideEffects(
                    treeNode,
                    NewExprConstant(treeNode, boolTypeSym, new ConstValInit(checkTrue)),
                    StripNubCtor(srcExpr),
                    true,
                    true);
            }

            AGGTYPESYM aggTypeSym = (srcExpr.TypeSym as NUBSYM).GetAggTypeSym();
            if (aggTypeSym == null)
            {
                return NewError(treeNode, boolTypeSym);
            }
            Compiler.EnsureState(aggTypeSym, AggStateEnum.Prepared);

            PROPSYM propSym = EnsureNubHasValue(treeNode);
            if (propSym == null)
            {
                return NewError(treeNode, boolTypeSym);
            }

            CheckFieldUse(srcExpr, true);

            EXPRPROP resExpr = NewExpr(treeNode, EXPRKIND.PROP, boolTypeSym) as EXPRPROP;

            resExpr.SlotPropWithType.Set(propSym, aggTypeSym);
            resExpr.GetMethodWithType.Set(propSym.GetMethodSym, aggTypeSym);
            resExpr.ArgumentsExpr = null;
            resExpr.ObjectExpr = srcExpr;

            if (checkTrue)
            {
                return resExpr;
            }

            return NewExprBinop(treeNode, EXPRKIND.LOGNOT, resExpr.TypeSym, resExpr, null);
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubValue
        //
        /// <summary>
        /// Create an expr for exprSrc.Value where exprSrc->type is a NUBSYM.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="srcExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubValue(BASENODE treeNode, EXPR srcExpr)
        {
            DebugUtil.Assert(srcExpr != null && srcExpr.TypeSym.IsNUBSYM);

            // For new T?(x), the answer is x.
            if (IsNubCtor(srcExpr))
            {
                DebugUtil.Assert(
                    (srcExpr as EXPRCALL).ArgumentsExpr != null &&
                    (srcExpr as EXPRCALL).ArgumentsExpr.Kind != EXPRKIND.LIST);
                return (srcExpr as EXPRCALL).ArgumentsExpr;
            }

            TYPESYM baseTypeSym = (srcExpr.TypeSym as NUBSYM).BaseTypeSym;
            AGGTYPESYM ats = (srcExpr.TypeSym as NUBSYM).GetAggTypeSym();
            if (ats == null)
            {
                return NewError(treeNode, baseTypeSym);
            }
            compiler.EnsureState(ats, AggStateEnum.Prepared);

            PROPSYM propertySym = compiler.MainSymbolManager.NullableValuePropertySym;

            if (propertySym == null)
            {
                string name = compiler.NameManager.GetPredefinedName(PREDEFNAME.CAP_VALUE);

                propertySym = compiler.MainSymbolManager.LookupAggMember(
                    name,
                    ats.GetAggregate(),
                    SYMBMASK.PROPSYM) as PROPSYM;
                if (propertySym == null ||
                    propertySym.IsStatic ||
                    propertySym.Access != ACCESS.PUBLIC ||
                    propertySym.ParameterTypes.Count > 0 ||
                    !propertySym.ReturnTypeSym.IsTYVARSYM ||
                    propertySym.GetMethodSym == null)
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_MissingPredefinedMember,
                        new ErrArg(ats),
                        new ErrArg(name));
                    return NewError(treeNode, baseTypeSym);
                }
                compiler.MainSymbolManager.NullableValuePropertySym = propertySym;
            }

            CheckFieldUse(srcExpr, true);

            EXPRPROP propertyExpr = NewExpr(treeNode, EXPRKIND.PROP, baseTypeSym) as EXPRPROP;

            propertyExpr.SlotPropWithType.Set(propertySym, ats);
            propertyExpr.GetMethodWithType.Set(propertySym.GetMethodSym, ats);
            propertyExpr.ArgumentsExpr = null;
            propertyExpr.ObjectExpr = srcExpr;

            return propertyExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubGetValOrDef
        //
        /// <summary>
        /// Create an expr for exprSrc.GetValueOrDefault()
        /// where exprSrc->type is a NUBSYM.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="srcExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubGetValOrDef(BASENODE treeNode, EXPR srcExpr)
        {
            DebugUtil.Assert(srcExpr != null && srcExpr.TypeSym.IsNUBSYM);

            TYPESYM baseTypeSym = (srcExpr.TypeSym as NUBSYM).BaseTypeSym;

            // If srcExpr is null, just return the appropriate default value.
            if (srcExpr.GetConst() != null)
            {
                return AddSideEffects(treeNode, NewExprZero(treeNode, baseTypeSym), srcExpr, true, true);
            }

            // For new T?(x), the answer is x.
            if (IsNubCtor(srcExpr))
            {
                DebugUtil.Assert(
                    (srcExpr as EXPRCALL).ArgumentsExpr != null &&
                    (srcExpr as EXPRCALL).ArgumentsExpr.Kind != EXPRKIND.LIST);
                return (srcExpr as EXPRCALL).ArgumentsExpr;
            }

            AGGTYPESYM aggTypeSym = (srcExpr.TypeSym as NUBSYM).GetAggTypeSym();
            if (aggTypeSym == null)
            {
                return NewError(treeNode, baseTypeSym);
            }
            Compiler.EnsureState(aggTypeSym, AggStateEnum.Prepared);

            METHSYM methSym = EnsureNubGetValOrDef(treeNode);
            if (methSym == null)
            {
                return NewError(treeNode, baseTypeSym);
            }

            CheckFieldUse(srcExpr, true);

            EXPRCALL resExpr = NewExpr(treeNode, EXPRKIND.CALL, baseTypeSym) as EXPRCALL;

            resExpr.MethodWithInst.Set(methSym, aggTypeSym, BSYMMGR.EmptyTypeArray);
            resExpr.ArgumentsExpr = null;
            resExpr.ObjectExpr = srcExpr;

            return resExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubNew
        //
        /// <summary>
        /// Create an expr for new T?(exprSrc) where T is exprSrc-&gt;type.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="srcExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR BindNubNew(BASENODE treeNode, EXPR srcExpr)
        {
            DebugUtil.Assert(srcExpr != null);

            // Create a NUBSYM instance whose base bype is represented by srcExpr.TypeSym.
            NUBSYM nubSym = Compiler.MainSymbolManager.GetNubType(srcExpr.TypeSym);

            // Get a TYPESYM instance representing Nullable<> for nubSym.
            AGGTYPESYM aggTypeSym = nubSym.GetAggTypeSym();
            if (aggTypeSym == null)
            {
                return NewError(treeNode, nubSym);
            }
            Compiler.EnsureState(aggTypeSym, AggStateEnum.Prepared);

            METHSYM methSym = Compiler.MainSymbolManager.NullableCtorMethodSym;

            if (methSym == null)
            {
                string name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR);

                for (SYM sym = Compiler.MainSymbolManager.LookupAggMember(name, aggTypeSym.GetAggregate(), SYMBMASK.ALL);
                    ;
                    sym = sym.NextSameNameSym)
                {
                    if (sym == null)
                    {
                        Compiler.Error(treeNode, CSCERRID.ERR_MissingPredefinedMember,
                            new ErrArg(aggTypeSym), new ErrArg(name));
                        return NewError(treeNode, nubSym);
                    }
                    if (sym.IsMETHSYM)
                    {
                        methSym = sym as METHSYM;
                        if (methSym.ParameterTypes.Count == 1 && methSym.ParameterTypes[0].IsTYVARSYM &&
                            methSym.Access == ACCESS.PUBLIC)
                        {
                            break;
                        }
                    }
                }
                Compiler.MainSymbolManager.NullableCtorMethodSym = methSym;
            }

            EXPRCALL resExpr = NewExpr(treeNode, EXPRKIND.CALL, nubSym) as EXPRCALL;

            resExpr.MethodWithInst.Set(methSym, aggTypeSym, BSYMMGR.EmptyTypeArray);
            resExpr.ArgumentsExpr = srcExpr;
            resExpr.ObjectExpr = null;
            resExpr.Flags |= EXPRFLAG.NEWOBJCALL | EXPRFLAG.CANTBENULL;

            return resExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.StripNubCtor
        //
        /// <summary>
        /// If the expr is new T?(t) reduce it to t.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPR StripNubCtor(EXPR expr)
        {
            while (IsNubCtor(expr))
            {
                expr = (expr as EXPRCALL).ArgumentsExpr;
                DebugUtil.Assert(expr != null && expr.Kind != EXPRKIND.LIST);
            }
            return expr;
        }


        //------------------------------------------------------------
        // FUNCBREC.IsNubCtor (1)
        //
        /// <summary>
        /// Return true iff the method is the nullable ctor taking one parameter.
        /// </summary>
        /// <param name="meth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsNubCtor(METHSYM meth)
        {
            return (
                meth != null &&
                meth.ClassSym.IsPredefAgg(PREDEFTYPE.G_OPTIONAL) &&
                meth.ParameterTypes.Count == 1 &&
                meth.ParameterTypes[0].IsTYVARSYM &&
                meth.IsCtor);
        }

        //------------------------------------------------------------
        // FUNCBREC.IsNubCtor (2)
        //
        /// <summary>
        /// Return true iff the expr is an invocation of the nullable ctor.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool IsNubCtor(EXPR expr)
        {
            // Note: If the constructor call is used to init an object, we can't strip it off!
            return (
                expr.Kind == EXPRKIND.CALL &&
                (expr as EXPRCALL).ObjectExpr == null &&
                IsNubCtor((expr as EXPRCALL).MethodWithInst.MethSym));
        }

        //------------------------------------------------------------
        // FUNCBREC.BindNubConversion
        //
        // Called by bindImplicitConversion when the destination type is Nullable&lt;T&gt;. The following
        // conversions are handled by this method:
        //
        // * For S in { object, ValueType, interfaces implemented by underlying type} there is an explicit
        // unboxing conversion S =&gt; T?
        // * System.Enum =&gt; T? there is an unboxing conversion if T is an enum type
        // * null =&gt; T? implemented as default(T?)
        //
        // * Implicit T?* =&gt; T?+ implemented by either wrapping or calling GetValueOrDefault the
        // appropriate number of times.
        // * If imp/exp S =&gt; T then imp/exp S =&gt; T?+ implemented by converting to T then wrapping the
        // appropriate number of times.
        // * If imp/exp S =&gt; T then imp/exp S?+ =&gt; T?+ implemented by calling GetValueOrDefault (m-1) times
        // then calling HasValue, producing a null if it returns false, otherwise calling Value,
        // converting to T then wrapping the appropriate number of times.
        //
        // The 3 rules above can be summarized with the following recursive rules:
        //
        // * If imp/exp S =&gt; T? then imp/exp S? =&gt; T? implemented as
        // qs.HasValue ? (T?)(qs.Value) : default(T?)
        // * If imp/exp S =&gt; T then imp/exp S =&gt; T? implemented as new T?((T)s)
        //
        // This method also handles calling bindUserDefinedConverion. This method does NOT handle
        // the following conversions:
        //
        // * Implicit boxing conversion from S? to { object, ValueType, Enum, ifaces implemented by S }. (Handled by bindImplicitConversion.)
        // * If imp/exp S =&gt; T then explicit S?+ =&gt; T implemented by calling Value the appropriate number
        // of times. (Handled by bindExplicitConversion.)
        //
        // The recursive equivalent is:
        //
        // * If imp/exp S =&gt; T and T is not nullable then explicit S? =&gt; T implemented as qs.Value
        //
        // Some nullable conversion are NOT standard conversions. In particular, if S =&gt; T is implicit
        // then S? =&gt; T is not standard. Similarly if S =&gt; T is not implicit then S =&gt; T? is not standard.
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="srcExpr"></param>
        /// <param name="srcTypeSym"></param>
        /// <param name="dstNubSym"></param>
        /// <param name="dstExpr"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool BindNubConversion(
            BASENODE treeNode,
            EXPR srcExpr,
            TYPESYM srcTypeSym,
            NUBSYM dstNubSym,
            ref EXPR dstExpr,
            ConvertTypeEnum flags)
        {
            // This code assumes that STANDARD and ISEXPLICIT are never both set.
            // bindUserDefinedConversion should ensure this!
            DebugUtil.Assert((~flags & (ConvertTypeEnum.STANDARD | ConvertTypeEnum.ISEXPLICIT)) != 0);

            DebugUtil.Assert(srcExpr == null || srcExpr.TypeSym == srcTypeSym);
            DebugUtil.Assert(dstExpr == null || srcExpr != null);

            DebugUtil.Assert(srcTypeSym != dstNubSym);
            // bindImplicitConversion should have taken care of this already.

            AGGTYPESYM dstAggTypeSym = dstNubSym.GetAggTypeSym();
            if (dstAggTypeSym == null)
            {
                return false;
            }

            // Check for the unboxing conversion. This takes precedence over the wrapping conversions.
            if (Compiler.IsBaseType(dstNubSym.BaseTypeSym, srcTypeSym) && !FWrappingConv(srcTypeSym, dstNubSym))
            {
                // These should be different! Fix the caller if srcTypeSym is an AGGTYPESYM of Nullable.
                DebugUtil.Assert(dstAggTypeSym != srcTypeSym);

                // srcTypeSym is a base type of the destination nullable type so there is an explicit
                // unboxing conversion.
                if ((flags & ConvertTypeEnum.ISEXPLICIT) == 0)
                {
                    return false;
                }
                return BindSimpleCast(treeNode, srcExpr, dstNubSym, ref dstExpr, EXPRFLAG.UNBOX);
            }

            int dstNubStripCount;
            int srcNubStripCount;
            TYPESYM dstBaseTypeSym = dstNubSym.StripNubs(out dstNubStripCount);
            TYPESYM srcBaseTypeSym = srcTypeSym.StripNubs(out srcNubStripCount);

            bindNubConversion_Convert fnConvert;
            EXPR expr = null;   // temp

            if ((flags & ConvertTypeEnum.ISEXPLICIT) != 0)
            {
                fnConvert = new bindNubConversion_Convert(BindExplicitConversion);
            }
            else
            {
                fnConvert = new bindNubConversion_Convert(BindImplicitConversion);
            }

            //bool (FUNCBREC::*pfn)(BASENODE *, EXPR *, TYPESYM *, TYPESYM *, EXPR **, uint) =
            //    (flags & ISEXPLICIT) ? &FUNCBREC::bindExplicitConversion : &FUNCBREC::bindImplicitConversion;

            if (srcNubStripCount == 0)
            {
                DebugUtil.Assert(srcTypeSym == srcBaseTypeSym);

                // The null type can be implicitly converted to T? as the default value.
                if (srcTypeSym.IsNULLSYM)
                {
                    dstExpr = AddSideEffects(treeNode, NewExprZero(treeNode, dstNubSym), srcExpr, true, true);
                    return true;
                }

                EXPR tempExpr = srcExpr;

                // If there is an implicit/explicit S => T then there is an implicit/explicit S => T?
                if (srcTypeSym == dstBaseTypeSym ||
                    fnConvert(
                        treeNode,
                        srcExpr,
                        srcTypeSym,
                        dstBaseTypeSym,
                        ref tempExpr,
                        flags | ConvertTypeEnum.NOUDC))
                {
                    // srcTypeSym is not nullable so just wrap the required number of times.
                    for (int i = 0; i < dstNubStripCount; i++)
                    {
                        tempExpr = BindNubNew(treeNode, tempExpr);
                    }
                    DebugUtil.Assert(tempExpr.TypeSym == dstNubSym);
                    dstExpr = tempExpr;
                    return true;
                }

                // No builtin conversion. Maybe there is a user defined conversion....
                return (
                    (flags & ConvertTypeEnum.NOUDC) == 0 &&
                    BindUserDefinedConversion(
                        treeNode,
                        srcExpr,
                        srcTypeSym,
                        dstNubSym,
                        ref dstExpr,
                        (flags & ConvertTypeEnum.ISEXPLICIT) == 0));
            }

            // Both are Nullable so there is only a conversion if there is a conversion between the base types.
            // That is, if there is an implicit/explicit S => T then there is an implicit/explicit S?+ => T?+.
            if (srcBaseTypeSym != dstBaseTypeSym &&
                !fnConvert(treeNode, null, srcBaseTypeSym, dstBaseTypeSym, ref expr, flags | ConvertTypeEnum.NOUDC))
            {
                // No builtin conversion. Maybe there is a user defined conversion....
                return (
                    (flags & ConvertTypeEnum.NOUDC) == 0 &&
                    BindUserDefinedConversion(
                        treeNode,
                        srcExpr,
                        srcTypeSym,
                        dstNubSym,
                        ref dstExpr,
                        (flags & ConvertTypeEnum.ISEXPLICIT) == 0));
            }

            // We need to go all the way down to the base types, do the conversion, then come all the way back up.
            EXPR valExpr;
            NubInfo nubInfo = new NubInfo();

            BindNubCondValBin(
                treeNode,
                srcExpr,
                null,
                ref nubInfo,
                LiftFlagsEnum.LiftBoth);
            valExpr = nubInfo.Val(0);

            DebugUtil.Assert(valExpr.TypeSym == srcBaseTypeSym);

            if (!fnConvert(
                treeNode,
                valExpr,
                valExpr.TypeSym,
                dstBaseTypeSym,
                ref valExpr,
                flags | ConvertTypeEnum.NOUDC))
            {
                DebugUtil.Assert(false, "bind(Im|Ex)plicitConversion failed unexpectedly");
                return false;
            }

            for (int i = 0; i < dstNubStripCount; i++)
            {
                valExpr = BindNubNew(treeNode, valExpr);
            }
            DebugUtil.Assert(valExpr.TypeSym == dstNubSym);

            dstExpr = BindNubOpRes(treeNode, dstNubSym, dstNubSym, valExpr, ref nubInfo, false);

            return true;
        }
    }
}
