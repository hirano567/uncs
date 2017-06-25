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
// FlowChecker.cs
//
// 2015/06/07 hirano567@hotmail.co.jp
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
    // class FlowChecker
    //======================================================================
    internal class FlowChecker
    {
        //------------------------------------------------------------
        // FlowChecker Fields and Properties
        //------------------------------------------------------------
        private COMPILER compiler = null;					// * m_comp;

        private COMPILER Compiler
        {
            get { return this.compiler; }
        }

        private ICompileCallback compileCallBack = null;	// * m_pccb;
        //private NRHEAP m_heap = null;						// * m_heap;
        private int uninitedVarCount = 0;					// m_cvarUninit;
        private METHINFO methInfo = null;					// * m_info;

        // Set before scanning each (anon or not) method body.

        private AnonMethInfo anonMethInfo = null;	// * m_pami;
        private int finallyScanCurrentTs = 0;		// m_tsFinallyScanCur; // 0 when not in a finally scan.
        private int finallyScanPreviousTs = 0;		// m_tsFinallyScanPrev;
        private LOCVARSYM thisLocVarSym = null;		// * m_locThis;
        private List<SYM> outParamList = null;		//private SYMLIST * m_listOutParams;


        //------------------------------------------------------------
        // FlowChecker.CheckDefiniteAssignment (static)
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        /// <param name="callback"></param>
        /// <param name="block"></param>
        /// <param name="info"></param>
        /// <param name="firstAnonInfo"></param>
        /// <param name="uninitCount"></param>
        //------------------------------------------------------------
        internal static void CheckDefiniteAssignment(
              COMPILER comp,
              ICompileCallback callback,
              EXPRBLOCK block,
              METHINFO info,
              AnonMethInfo firstAnonInfo,
              int uninitCount)
        {
            FlowChecker flow = new FlowChecker();
            flow.compiler = comp;
            flow.compileCallBack = callback;
            flow.methInfo = info;
            flow.uninitedVarCount = uninitCount;
            flow.ScanAll(block, firstAnonInfo);
        }

        // Get the number of bits needed for definite assignment checking.

        //------------------------------------------------------------
        // FlowChecker.GetCbit (static)
        //
        /// <summary>
        /// Returns the number of bits used to represent the type
        /// when doing definite assignment analysis.
        /// For non-structs this is always 1.
        /// For structs it is the sum of the cbits of the instance field types.
        /// Empty structs have cbit 0.
        /// Fixed size buffers have cbit 0.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static int GetCbit(COMPILER compiler, TYPESYM typeSym)
        {
            AGGTYPESYM aggTypeSym;

            switch (typeSym.Kind)
            {
                default:
                    return 1;

                case SYMKIND.AGGTYPESYM:
                    aggTypeSym = typeSym as AGGTYPESYM;
                    break;

                case SYMKIND.NUBSYM:
                    aggTypeSym = (typeSym as NUBSYM).GetAggTypeSym();
                    if (aggTypeSym == null) return 1;
                    break;
            }

            if (aggTypeSym.IsBitDefAssgCountSet)
            {
                DebugUtil.Assert(aggTypeSym.AggState >= AggStateEnum.DefinedMembers);
                return aggTypeSym.BitDefAssgCount;
            }

            // Make sure we have the fields!
            compiler.EnsureState(aggTypeSym, AggStateEnum.Prepared);
            DebugUtil.Assert(aggTypeSym.AggState >= AggStateEnum.DefinedMembers);

            if (!aggTypeSym.IsStructType() ||
                aggTypeSym.IsPredefined() && BSYMMGR.GetPredefFundType(aggTypeSym.GetPredefType()) != FUNDTYPE.STRUCT ||
                aggTypeSym.GetAggregate().LayoutErrorOccurred)
            {
                aggTypeSym.BitDefAssgCount = 1;
                return 1;
            }

            int cbit = 0;
            bool fSubst = !aggTypeSym.IsInstType();

            if (fSubst)
            {
                // Make sure the instance typ^e has been done, so we can compare ibit values.
                AGGTYPESYM atsInst = aggTypeSym.GetInstType();
                if (!atsInst.IsBitDefAssgCountSet)
                {
                    GetCbit(compiler, atsInst);
                }
            }

            for (SYM sym = aggTypeSym.GetAggregate().FirstChildSym; sym != null; sym = sym.NextSym)
            {
                if (!sym.IsMEMBVARSYM) continue;
                MEMBVARSYM fieldSym = sym as MEMBVARSYM;
                if (fieldSym.IsStatic || fieldSym.FixedAggSym != null) continue;

                TYPESYM fieldTypeSym = fieldSym.TypeSym;
                if (!fSubst)
                {
                    fieldSym.SetIbitInst(cbit);
                }
                else
                {
                    fieldTypeSym = compiler.MainSymbolManager.SubstType(fieldTypeSym, aggTypeSym, null);
                    if (cbit != fieldSym.GetIbitInst())
                    {
                        fieldSym.IbitVaries = true;
                    }
                }

                cbit += GetCbit(compiler, fieldTypeSym);
            }

            if (cbit == 0 && aggTypeSym.IsPredefined())
            {
                cbit = 1;
            }
            aggTypeSym.BitDefAssgCount = cbit;

            return cbit;
        }

        //------------------------------------------------------------
        // FlowChecker.GetIbit (1) (static)
        //
        /// <summary>
        /// Get the bit position for the given field within the given type.
        /// The type MUST be an AGGTYPESYM for the AGGSYM containing the fld.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="fieldSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static int GetIbit(COMPILER compiler, MEMBVARSYM fieldSym, AGGTYPESYM aggTypeSym)
        {
            DebugUtil.Assert(aggTypeSym.GetAggregate() == fieldSym.ClassSym);
            DebugUtil.Assert(aggTypeSym.IsStructType());
            DebugUtil.Assert(!fieldSym.IsStatic && fieldSym.FixedAggSym == null);

            // This should already be set, but just in case we assert it here
            // and call FlowChecker::GetCbit instead of AGGTYPESYM::GetCbitDefAssg.
            DebugUtil.Assert(aggTypeSym.IsBitDefAssgCountSet);

            if (GetCbit(compiler, aggTypeSym) == 1)
            {
                // We can get here for a variety of reasons.
                DebugUtil.Assert(
                    aggTypeSym.IsPredefined() &&
                    BSYMMGR.GetPredefFundType(aggTypeSym.GetPredefType()) != FUNDTYPE.STRUCT ||
                    aggTypeSym.GetAggregate().LayoutErrorOccurred ||
                    fieldSym.GetIbitInst() == 0 ||
                    (fieldSym.GetIbitInst() == 1 &&
                    GetCbit(compiler, compiler.MainSymbolManager.SubstType(
                        fieldSym.TypeSym, aggTypeSym, null)) == 0));
                return 0;
            }

            if (aggTypeSym.IsInstType() || !fieldSym.IbitVaries)
            {
                return fieldSym.GetIbitInst();
            }

            int ibit = 0;

            for (SYM sym = aggTypeSym.GetAggregate().FirstChildSym; sym != null; sym = sym.NextSym)
            {
                if (sym == fieldSym)
                {
                    DebugUtil.Assert(
                        ibit + GetCbit(compiler, compiler.MainSymbolManager.SubstType(
                        fieldSym.TypeSym, aggTypeSym, null))
                        <= aggTypeSym.BitDefAssgCount);
                    return ibit;
                }

                if (!sym.IsMEMBVARSYM) continue;

                MEMBVARSYM fldSym = sym as MEMBVARSYM;
                if (fldSym.IsStatic || fldSym.FixedAggSym != null) continue;

                TYPESYM fldTypeSym = compiler.MainSymbolManager.SubstType(
                    fldSym.TypeSym, aggTypeSym, null);
                ibit += GetCbit(compiler, fldTypeSym);
                DebugUtil.Assert(ibit <= aggTypeSym.BitDefAssgCount);
            }

            DebugUtil.Assert(ibit == aggTypeSym.BitDefAssgCount);
            DebugUtil.Assert(false, "Why didn't we find the field?");
            return 0;
        }

        //------------------------------------------------------------
        // FlowChecker.GetIbit (2) (static)
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        /// <param name="fwt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static int GetIbit(COMPILER comp, FieldWithType fwt)
        {
            return GetIbit(comp, fwt.FieldSym, fwt.AggTypeSym);
        }

        //------------------------------------------------------------
        // FlowChecker Constructor
        //------------------------------------------------------------
        private FlowChecker() { }

        //private FlowChecker(FlowChecker & flow) { VSFAIL("No Copy ctor"); }

        //------------------------------------------------------------
        // FlowChecker.ScanAll
        //
        /// <summary>
        /// Scan the main method body and all anonymous methods.
        /// This is the main entry point.
        /// </summary>
        /// <param name="blockExpr"></param>
        /// <param name="firstAnonMethInfo"></param>
        //------------------------------------------------------------
        private void ScanAll(EXPRBLOCK blockExpr, AnonMethInfo firstAnonMethInfo)
        {
            DebugUtil.Assert(this.methInfo != null);

            BitSet currentBitset = new BitSet(this.uninitedVarCount);
            BitSet errorBitset = new BitSet(this.uninitedVarCount);

            this.anonMethInfo = null;
            this.finallyScanCurrentTs = 0;
            this.finallyScanPreviousTs = 0;

            SCOPESYM paramsScopeSym = this.methInfo.OuterScopeSym;

            // Get the LOCVARSYM for "this" if we need to check definite assignment for it.
            this.thisLocVarSym = compiler.LocalSymbolManager.LookupLocalSym(
                compiler.NameManager.GetPredefinedName(PREDEFNAME.THIS),
                paramsScopeSym,
                SYMBMASK.LOCVARSYM) as LOCVARSYM;
            if (this.thisLocVarSym != null && this.thisLocVarSym.LocSlotInfo.JbitDefAssg() == 0)
            {
                this.thisLocVarSym = null;
            }

            BuildOutParamList(paramsScopeSym);

            if (ScanBlock(blockExpr, ref currentBitset, ref errorBitset))
            {
                ReportReturnNeeded(blockExpr, this.methInfo.MethodSym.ParseTreeNode, ref currentBitset);
            }

            // Do definite assignment analysis on the anon methods.
            ScanAnonMeths(firstAnonMethInfo, ref currentBitset, ref errorBitset);
        }

        //------------------------------------------------------------
        // FlowChecker.ScanAnonMeths
        //
        /// <summary>
        /// Scan the anonymous methods.
        /// The bitsets are just passed in to share the memory.
        /// </summary>
        /// <param name="anonMethInfo"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanAnonMeths(
            AnonMethInfo anonMethInfo,
            ref BitSet currentBitset,
            ref BitSet errorBitset)
        {
            for (; anonMethInfo != null; anonMethInfo = anonMethInfo.NextInfo)
            {
                if (!anonMethInfo.BSetEnter.FInited())
                {
                    currentBitset.SetBitRange(0, this.uninitedVarCount);
                }
                else
                {
                    currentBitset.SetBit(anonMethInfo.BSetEnter);
                }
                currentBitset.ClearBitRange(anonMethInfo.JBitMin - 1, anonMethInfo.JBitLim - 1);

                this.anonMethInfo = anonMethInfo;
                this.finallyScanCurrentTs = 0;
                this.finallyScanPreviousTs = 0;
                this.thisLocVarSym = null;
                BuildOutParamList(this.anonMethInfo.ParametersScopeSym);

                if (ScanBlock(anonMethInfo.BodyBlockExpr, ref currentBitset, ref errorBitset))
                {
                    ReportReturnNeeded(anonMethInfo.BodyBlockExpr, anonMethInfo.ParseTreeNode, ref currentBitset);
                }
                DebugUtil.Assert(this.anonMethInfo == anonMethInfo);

                ScanAnonMeths(anonMethInfo.ChildInfo, ref currentBitset, ref errorBitset);
            }
        }

        //------------------------------------------------------------
        // FlowChecker.ReportReturnNeeded
        //
        /// <summary>
        /// Report an error if the end of the block is reachable and the method returns a value.
        /// Sets the EXF_NEEDSRET flag as appropriate.
        /// Checks that all out parameters are assigned for methods with void return type.
        /// </summary>
        /// <param name="blockExpr"></param>
        /// <param name="treeNode"></param>
        /// <param name="currentBitset"></param>
        //------------------------------------------------------------
        private void ReportReturnNeeded(
            EXPRBLOCK blockExpr,
            BASENODE treeNode,
            ref BitSet currentBitset)
        {
            if (blockExpr == null || !blockExpr.ReachableEnd())
            {
                return;
            }

            if (this.anonMethInfo != null)
            {
                if (this.anonMethInfo.ReturnTypeSym != null &&
                    this.anonMethInfo.ReturnTypeSym.IsPredefType(PREDEFTYPE.VOID))
                {
                    CheckOutParams(treeNode, ref currentBitset);
                    blockExpr.Flags |= EXPRFLAG.NEEDSRET;
                }
                else if (this.anonMethInfo.DelegateTypeSym != null)
                {
                    compiler.Error(
                        treeNode,
                        CSCERRID.ERR_AnonymousReturnExpected,
                        new ErrArg(this.anonMethInfo.DelegateTypeSym));
                }
                return;
            }

            if (this.methInfo.MethodSym.ReturnTypeSym != null &&
                this.methInfo.MethodSym.ReturnTypeSym.IsPredefType(PREDEFTYPE.VOID) ||
                this.methInfo.IsIterator)
            {
                CheckOutParams(treeNode, ref currentBitset);
                blockExpr.Flags |= EXPRFLAG.NEEDSRET;
            }
            else
            {
                compiler.Error(
                    treeNode,
                    CSCERRID.ERR_ReturnExpected,
                    new ErrArg(this.methInfo.MethodSym));
            }
        }

        //------------------------------------------------------------
        // FlowChecker.CheckOutParams
        //
        /// <summary>
        /// Make sure all out parameters (including "this" for a struct ctor)
        /// are definitely assigned.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="currentBitset"></param>
        //------------------------------------------------------------
        private void CheckOutParams(BASENODE treeNode, ref BitSet currentBitset)
        {
            int ibitMin;

            if (this.thisLocVarSym != null &&
                (ibitMin = this.thisLocVarSym.LocSlotInfo.JbitDefAssg() - 1) >= 0 &&
                !currentBitset.TestAllRange(ibitMin, ibitMin + GetCbit(this.thisLocVarSym.TypeSym)))
            {
                TYPESYM thisTypeSym = this.thisLocVarSym.TypeSym;
                if (thisTypeSym.IsNUBSYM)
                {
                    thisTypeSym = (thisTypeSym as NUBSYM).GetAggTypeSym();
                }

                DebugUtil.Assert((thisTypeSym as AGGTYPESYM).IsInstType());

                // Determine which fields of this were unassigned, and report an error for each.
                for (SYM sym = thisTypeSym.GetAggregate().FirstChildSym; sym != null; sym = sym.NextSym)
                {
                    if (!sym.IsMEMBVARSYM ||
                        (sym as MEMBVARSYM).IsStatic ||
                        (sym as MEMBVARSYM).FixedAggSym!=null)
                    {
                        continue;
                    }
                    int ibitMinChd = ibitMin + GetIbit(sym as MEMBVARSYM, thisTypeSym as AGGTYPESYM);
                    int ibitLimChd = ibitMinChd + GetCbit(
                        compiler.MainSymbolManager.SubstType(
                            (sym as MEMBVARSYM).TypeSym,
                            thisTypeSym as AGGTYPESYM,
                            null));
                    if (!currentBitset.TestAllRange(ibitMinChd, ibitLimChd))
                    {
                        compiler.Error(treeNode, CSCERRID.ERR_UnassignedThis, new ErrArg(sym));
                    }
                }
            }

            foreach (SYM sym in this.outParamList)
            {
                LOCVARSYM locSym = sym as LOCVARSYM;
                if ((ibitMin = locSym.LocSlotInfo.JbitDefAssg() - 1) >= 0 &&
                    !currentBitset.TestAllRange(ibitMin, ibitMin + GetCbit(locSym.TypeSym)))
                {
                    compiler.Error(treeNode, CSCERRID.ERR_ParamUnassigned, new ErrArg(locSym));
                }
            }
        }

        //------------------------------------------------------------
        // FlowChecker.BuildOutParamList
        //
        /// <summary>
        /// Build a SYMLIST for the out parameters, given m_scopeParams.
        /// </summary>
        /// <param name="paramsScopeSym"></param>
        //------------------------------------------------------------
        private void BuildOutParamList(SCOPESYM paramsScopeSym)
        {
            this.outParamList = new List<SYM>();
            List<SYM> paramList = this.outParamList;
            for (SYM sym = paramsScopeSym.FirstChildSym; sym != null; sym = sym.NextSym)
            {
                int ibitMin;
                if (sym.IsLOCVARSYM &&
                    (ibitMin = (sym as LOCVARSYM).LocSlotInfo.JbitDefAssg() - 1) >= 0 &&
                    !(sym as LOCVARSYM).IsThis)
                {
                    compiler.LocalSymbolManager.AddToLocalSymList(sym, paramList);
                }
            }
        }

        //------------------------------------------------------------
        // FlowChecker.ScanStmts
        //
        /// <summary>
        /// Scan a list of statements.
        /// In general, the ending bsetCur is garbage, so don't use it!
        /// NOTE:  This may scan stuff outside the statement list
        /// if there are jumps out of the containing block (or other structure).
        /// </summary>
        /// <param name="statementsExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanStmts(
            EXPRSTMT statementsExpr,
            ref BitSet currentBitset,
            ref BitSet errorBitset)
        {
            if (statementsExpr == null)
            {
                DebugUtil.VsFail("Why is ScanStmts being called with null?");
                currentBitset.Trash();
                return;
            }

            EXPRLABEL topLabelExpr = null;
            ScanStmtsInner(
                statementsExpr,
                ref topLabelExpr,
                ref currentBitset,
                ref errorBitset);

            while (topLabelExpr != null)
            {
                EXPRLABEL currentLabelExpr = EXPRLABEL.PopFromStack(ref topLabelExpr);

                // No switch labels should come through here.
                DebugUtil.Assert(currentLabelExpr.Kind == EXPRKIND.LABEL);
                currentBitset.SetBit(currentLabelExpr.EnterBitSet);
                if (currentLabelExpr.NextStatement == null)
                {
                    ScanEndOfChain(currentLabelExpr, ref currentBitset);
                }
                else
                {
                    ScanStmtsInner(
                        currentLabelExpr.NextStatement,
                        ref topLabelExpr,
                        ref currentBitset,
                        ref errorBitset);
                }
            }
            currentBitset.Trash();
        }

        //------------------------------------------------------------
        // FlowChecker.ScanStmtsInner
        //
        /// <summary>
        /// Scan a list of statements.
        /// In general, the ending bsetCur is garbage, so don't use it!
        /// NOTE:  This may scan stuff outside the statement list
        /// if there are jumps out of the containing block (or other structure).
        /// </summary>
        /// <param name="statementExpr"></param>
        /// <param name="topLabelExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanStmtsInner(
            EXPRSTMT statementExpr,
            ref EXPRLABEL topLabelExpr,
            ref BitSet currentBitset,
            ref BitSet errorBitset)
        {
            DebugUtil.Assert(statementExpr != null);

            for (; ; )
            {
                DebugUtil.Assert(statementExpr != null && statementExpr.Reachable());

                switch (statementExpr.Kind)
                {
                    default:
                    case EXPRKIND.HANDLER:
                        DebugUtil.VsFail("Bad statementExpr expr kind");
                        return;

                    case EXPRKIND.DELIM:
                        DebugUtil.Assert(statementExpr.ReachableEnd());
                        if (this.finallyScanCurrentTs == 0)
                        {
                            if (!(statementExpr as EXPRDELIM).BitSet.FInited())
                            {
                                (statementExpr as EXPRDELIM).BitSet.SetBit(currentBitset);
                            }
                            else
                            {
                                (statementExpr as EXPRDELIM).BitSet.Intersect(currentBitset);
                            }
                        }
                        break;

                    case EXPRKIND.NOOP:
                    case EXPRKIND.DEBUGNOOP:
                        DebugUtil.Assert(statementExpr.ReachableEnd());
                        break;

                    case EXPRKIND.DECL:
                        DebugUtil.Assert(statementExpr.ReachableEnd());
                        ScanExpr((statementExpr as EXPRDECL).InitialExpr, ref currentBitset, ref errorBitset);
                        break;

                    case EXPRKIND.STMTAS:
                        DebugUtil.Assert(statementExpr.ReachableEnd());
                        ScanExpr((statementExpr as EXPRSTMTAS).Expr, ref currentBitset, ref errorBitset);
                        break;

                    case EXPRKIND.THROW:
                        DebugUtil.Assert(!statementExpr.ReachableEnd());
                        // NOTE: The resulting bitset is garbage!
                        ScanExpr((statementExpr as EXPRTHROW).ObjectExpr, ref currentBitset, ref errorBitset);
                        return;

                    case EXPRKIND.BLOCK:
                        // The resulting bitset is the continuation bitset (on true return).
                        if (!ScanBlock(statementExpr as EXPRBLOCK, ref currentBitset, ref errorBitset))
                        {
                            return;
                        }
                        break;

                    case EXPRKIND.GOTOIF:
                        // The resulting bitset is the continuation bitset (on true return).
                        if (!ScanGotoIf(statementExpr as EXPRGOTOIF, ref topLabelExpr, ref currentBitset, ref errorBitset))
                        {
                            return;
                        }
                        break;

                    case EXPRKIND.GOTO:
                        DebugUtil.Assert(!statementExpr.ReachableEnd());
                        if (!ScanGoto(statementExpr as EXPRGOTO, ref currentBitset, ref errorBitset))
                        {
                            return;
                        }
                        statementExpr = (statementExpr as EXPRGOTO).LabelExpr;
                        DebugUtil.Assert(statementExpr.Reachable());
                        continue;

                    case EXPRKIND.LABEL:
                        // The resulting bitset is the continuation bitset (on true return).
                        if (!ScanLabel(statementExpr as EXPRLABEL, ref currentBitset))
                        {
                            return;
                        }
                        break;

                    case EXPRKIND.RETURN:
                        // The resulting bitset is correct.
                        ScanExpr((statementExpr as EXPRRETURN).ObjectExpr, ref currentBitset, ref errorBitset);
                        if ((statementExpr.Flags & EXPRFLAG.ASFINALLYLEAVE) == 0 ||
                            ScanThroughFinallys(statementExpr, ref currentBitset, ref errorBitset))
                        {
                            CheckOutParams(statementExpr.TreeNode, ref currentBitset);
                        }
                        break;

                    case EXPRKIND.TRY:
                        ScanTry(statementExpr as EXPRTRY, ref currentBitset, ref errorBitset);
                        break;

                    case EXPRKIND.SWITCH:
                        // The resulting bitset just reflects the switch expression. The end of the switch is
                        // reachable iff it doesn't have a default, so this is the correct continuation set.
                        ScanSwitch(statementExpr as EXPRSWITCH, ref currentBitset, ref errorBitset);
                        break;

                    case EXPRKIND.SWITCHLABEL:
                        // Can get here via a goto.
                        while ((statementExpr as EXPRSWITCHLABEL).StatementsExpr == null)
                        {
                            statementExpr = statementExpr.NextStatement;
                            if (statementExpr == null || statementExpr.Kind != EXPRKIND.SWITCHLABEL)
                            {
                                DebugUtil.VsFail("Shouldn't get here!");
                                return;
                            }
                            DebugUtil.Assert(statementExpr.Reachable());
                        }

                        // The resulting bitset is the continuation bitset (on true return).
                        if (!ScanLabel(statementExpr as EXPRSWITCHLABEL, ref currentBitset))
                        {
                            return;
                        }
                        statementExpr = (statementExpr as EXPRSWITCHLABEL).StatementsExpr;
                        DebugUtil.Assert(statementExpr!=null);
                        continue;
                }

                if (!statementExpr.ReachableEnd())
                {
                    return;
                }
                if (statementExpr.NextStatement == null)
                {
                    break;
                }
                statementExpr = statementExpr.NextStatement;
            }

            ScanEndOfChain(statementExpr, ref currentBitset);
        }

        //------------------------------------------------------------
        // FlowChecker.ScanEndOfChain
        //
        /// <summary>
        /// After a statement with no "next" (but reachable end) is scanned
        /// this is called to update the parent's information.
        /// If the parent is a block, the exit bitset is set to bsetCur.
        /// </summary>
        /// <param name="statementExpr"></param>
        /// <param name="currentBitset"></param>
        //------------------------------------------------------------
        private void ScanEndOfChain(EXPRSTMT statementExpr, ref BitSet currentBitset)
        {
            // We come here when the end of statementExpr is reachable and
            // statementExpr is the end of a chain.
            // The bitset needs to be propogated to the parent.
            DebugUtil.Assert(
                statementExpr != null &&
                statementExpr.NextStatement == null &&
                statementExpr.ReachableEnd());

            EXPRSTMT parentStatementExpr = statementExpr.ParentStatement;
            if (parentStatementExpr == null)
            {
                return;
            }

            switch (parentStatementExpr.Kind)
            {
                default:
                case EXPRKIND.SWITCH:
                case EXPRKIND.HANDLER:
                case EXPRKIND.TRY:
                    DebugUtil.VsFail("Shouldn't get here!");
                    break;

                case EXPRKIND.BLOCK:
                    // We should be processing the parent somewhere up the stack.
                    DebugUtil.Assert((parentStatementExpr.Flags & EXPRFLAG.MARKING) != 0);
                    (parentStatementExpr as EXPRBLOCK).ExitBitSet.SetBit(currentBitset);
                    break;

                case EXPRKIND.SWITCHLABEL:
                    // Should have already reported an error.
                    break;
            }
        }

        //------------------------------------------------------------
        // FlowChecker.ScanBlock
        //
        /// <summary>
        /// <para>Scan a block. On exit, bsetCur is the exit bitset of the block.
        /// If the end of the block is not reachable, this is all 1's.
        /// Returns true iff the end is reachable.</para>
        /// <para>NOTE:  This may scan stuff outside the block if there are jumps out of the block.</para>
        /// </summary>
        /// <param name="blockExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ScanBlock(
            EXPRBLOCK blockExpr,
            ref BitSet currentBitset,
            ref BitSet errorBitset)
        {
            // If there is no blockExpr, let the end be reachable.
            // This can happen when working with a malformed parse tree while refactoring.
            if (blockExpr == null)
            {
                return true;
            }

            DebugUtil.Assert(blockExpr.Reachable());
            if (blockExpr.StatementsExpr == null)
            {
                DebugUtil.Assert(blockExpr.ReachableEnd());
                return true;
            }

            DebugUtil.Assert((blockExpr.Flags & EXPRFLAG.MARKING) == 0);
            blockExpr.Flags |= EXPRFLAG.MARKING;
            blockExpr.ExitBitSet.SetBitRange(0, this.uninitedVarCount);

            ScanStmts(blockExpr.StatementsExpr, ref currentBitset, ref errorBitset);

            // Set the current bitset to whatever was stored on the blockExpr,
            // NOT what came back from the recursive call (which is garbage).
            currentBitset.SetBit(blockExpr.ExitBitSet);
            DebugUtil.Assert((blockExpr.Flags & EXPRFLAG.MARKING) != 0);
            blockExpr.Flags &= ~EXPRFLAG.MARKING;

            return blockExpr.ReachableEnd();
        }

        //------------------------------------------------------------
        // FlowChecker.ScanGoto
        //
        /// Scan a goto. Returns true iff the target of the goto should be scanned.
        /// NOTE: On false return bsetCur is garbage, so don't use it!
        //------------------------------------------------------------
        private bool ScanGoto(EXPRGOTO gotoExpr, ref BitSet currentBitset, ref BitSet errorBitset)
        {
            DebugUtil.Assert(gotoExpr.Reachable() && !gotoExpr.ReachableEnd());
            DebugUtil.Assert(
                (gotoExpr.Flags & EXPRFLAG.UNREALIZEDGOTO) == 0 ||
                (gotoExpr.Flags & EXPRFLAG.BADGOTO) != 0);

            if ((gotoExpr.Flags & EXPRFLAG.BADGOTO) != 0 ||
                (gotoExpr.Flags & EXPRFLAG.ASFINALLYLEAVE) != 0 &&
                !ScanThroughFinallys(gotoExpr, ref currentBitset, ref errorBitset))
            {
                currentBitset.Trash();
                return false;
            }

            DebugUtil.Assert((gotoExpr.Flags & EXPRFLAG.GOTONOTBLOCKED) != 0);
            DebugUtil.Assert(gotoExpr.LabelExpr != null);
            return true;
        }

        //------------------------------------------------------------
        // FlowChecker.ScanGotoIf
        //
        /// <summary>
        /// Scan a goto-if and target of the goto.
        /// Returns false if the fall-through is not possible.
        /// When this returns true, bsetCur is set to the continuation set.
        /// NOTE: If this returns false, bsetCur is garbage, so don't use it!
        /// </summary>
        /// <param name="gotoifExpr"></param>
        /// <param name="topLabelExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ScanGotoIf(
            EXPRGOTOIF gotoifExpr,
            ref EXPRLABEL topLabelExpr,
            ref BitSet currentBitset,
            ref BitSet errorBitset)
        {
            BitSet trueBitset = new BitSet(currentBitset);
            BitSet falseBitset = new BitSet(currentBitset);

            ScanExpr(
                gotoifExpr.ConditionExpr,
                ref currentBitset,
                ref errorBitset,
                 trueBitset,
                 falseBitset,
                  gotoifExpr.HasSense);

            if (!gotoifExpr.FNeverJumps())
            {
                // NOTE: GotoIf never jumps out of try blocks.
                DebugUtil.Assert(
                    gotoifExpr.LabelExpr != null &&
                    gotoifExpr.LabelExpr.Kind == EXPRKIND.LABEL);

                if (ScanLabel(gotoifExpr.LabelExpr, ref trueBitset))
                {
                    gotoifExpr.LabelExpr.PushOnStack(ref topLabelExpr);
                }
                if (gotoifExpr.AlwaysJumps())
                {
                    DebugUtil.Assert(!gotoifExpr.ReachableEnd());
                    currentBitset.Trash();
                    return false;
                }
            }
            currentBitset.SetBit(falseBitset);
            return true;
        }

        //------------------------------------------------------------
        // FlowChecker.ScanThroughFinallys
        //
        /// <summary>
        /// Scans the finallys executed by the given GOTO or RETURN.
        /// Returns false iff a finally blocks the jump
        /// (because the end of the finally is unreachable).
        /// On exit, bsetCur is the resulting bitset,
        /// which may be larger than the input bitset
        /// since the finallys may assign some additional locals.
        /// </summary>
        /// <param name="statementExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ScanThroughFinallys(
            EXPRSTMT statementExpr,
            ref BitSet currentBitset,
            ref BitSet errorBitset)
        {
            DebugUtil.Assert(
                statementExpr.Kind == EXPRKIND.GOTO ||
                statementExpr.Kind == EXPRKIND.RETURN &&
                (statementExpr.Flags & EXPRFLAG.RETURNISYIELD) == 0);
            DebugUtil.Assert(statementExpr.Reachable());

            int tsFinallyScanSave = this.finallyScanCurrentTs;
            this.finallyScanCurrentTs = ++this.finallyScanPreviousTs;
            SCOPESYM srcScopeSym;
            SCOPESYM dstScopeSym;

            if (statementExpr.Kind == EXPRKIND.GOTO)
            {
                srcScopeSym = (statementExpr as EXPRGOTO).CurrentScopeSym;
                dstScopeSym = (statementExpr as EXPRGOTO).TargetScopeSym;
            }
            else
            {
                srcScopeSym = (statementExpr as EXPRRETURN).CurrentScopeSym;
                dstScopeSym = null;
            }

            for (SCOPESYM scopeSym = srcScopeSym;
                scopeSym != dstScopeSym;
                scopeSym = scopeSym.ParentSym as SCOPESYM)
            {
                if (scopeSym.FinallyScopeSym == null)
                {
                    continue;
                }

                EXPRBLOCK blockExpr = scopeSym.FinallyScopeSym.BlockExpr;
                DebugUtil.Assert(blockExpr.Reachable());
                if (!ScanBlock(blockExpr, ref currentBitset, ref errorBitset))
                {
                    DebugUtil.Assert((statementExpr.Flags & EXPRFLAG.FINALLYBLOCKED) != 0);
                    this.finallyScanCurrentTs = tsFinallyScanSave;
                    return false;
                }
            }

            DebugUtil.Assert((statementExpr.Flags & EXPRFLAG.FINALLYBLOCKED) == 0);
            this.finallyScanCurrentTs = tsFinallyScanSave;
            return true;
        }

        //------------------------------------------------------------
        // FlowChecker.ScanLabel
        //
        /// <summary>
        /// This handles both EK_LABEL and EK_SWITCHLABEL.
        /// Scan a label but NOT the subsequent statements.
        /// If the subsequent statement list should be scanned,
        /// this returns true (and bsetCur is valid on return).
        /// If the subsequent statement list should not be scanned,
        /// this returns false and bsetCur is garbarge on return.
        /// If the label is already in a label stack,
        /// this returns false (after updating the label's bsetEnter).
        /// </summary>
        /// <param name="labelExpr"></param>
        /// <param name="currentBitset"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ScanLabel(EXPRLABEL labelExpr, ref BitSet currentBitset)
        {
            DebugUtil.Assert(labelExpr.Reachable());

            if (!labelExpr.EnterBitSet.FInited() ||
                labelExpr.TsFinallyScan != this.finallyScanCurrentTs)
            {
                labelExpr.EnterBitSet.SetBit(currentBitset);
            }
            else if (labelExpr.EnterBitSet.FIntersectChanged(currentBitset))
            {
                currentBitset.SetBit(labelExpr.EnterBitSet);
            }
            else
            {
                currentBitset.Trash();
                return false;
            }

            labelExpr.TsFinallyScan = this.finallyScanCurrentTs;
            return !labelExpr.InStack();
        }

        //------------------------------------------------------------
        // FlowChecker.ScanTry
        //
        /// <summary>
        /// Scan a try-finally or try-catch.
        /// On exit, bsetCur is the exit bitset if the end of the try-blah is reachable.
        /// If the end is not reachable, bsetCur is garbage.
        /// NOTE:  This may scan stuff outside the try-blah if there are jumps out of the try.
        /// Nevertheless, the return bitset is accurate (if the end of the try-blah is reachable).
        /// </summary>
        /// <param name="tryExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanTry(EXPRTRY tryExpr, ref BitSet currentBitset, ref BitSet errorBitset)
        {
            BitSet saveBitset = new BitSet(currentBitset);

            if ((tryExpr.Flags & EXPRFLAG.ISFINALLY) != 0)
            {
                if (!ScanBlock(tryExpr.TryBlockExpr, ref currentBitset, ref errorBitset))
                {
                    currentBitset.ClearAll();
                }
                if (ScanBlock(tryExpr.HandlersExpr as EXPRBLOCK, ref saveBitset, ref errorBitset))
                {
                    currentBitset.Union(saveBitset);
                }
            }
            else
            {
                BitSet tempBitset = new BitSet();

                // ScanBlock sets all bits if the end of the block is not reachable.
                ScanBlock(tryExpr.TryBlockExpr, ref currentBitset, ref errorBitset);

                DebugUtil.Assert(
                    (tryExpr.TryBlockExpr != null && tryExpr.TryBlockExpr.ReachableEnd()) ||
                    currentBitset.TestAllRange(0, this.uninitedVarCount));

                // All handlers are reachable.
                //STMTLOOP(tryExpr.handlers, chdStmtExpr)
                EXPRSTMT expr = tryExpr.HandlersExpr;
                while (expr != null)
                {
                    EXPRSTMT chdStmtExpr = expr;
                    expr = expr.NextStatement;
                    tempBitset.SetBit(saveBitset);
                    if (ScanBlock((chdStmtExpr as EXPRHANDLER).HandlerBlock, ref tempBitset, ref errorBitset))
                    {
                        currentBitset.Intersect(tempBitset);
                    }
                }
            }
        }

        //------------------------------------------------------------
        // FlowChecker.ScanSwitch
        //
        /// <summary>
        /// Scan a switch statement.
        /// On return bsetCur just reflects the switch expression, no contained statements.
        /// NOTE:  This typically scans stuff outside the switch
        /// since case sections often end with a jump out of the switch.
        /// </summary>
        /// <param name="switchExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanSwitch(EXPRSWITCH switchExpr, ref BitSet currentBitset, ref BitSet errorBitset)
        {
            // For a constant, only a matching switch label is reachable. We put a goto before the switch
            // to the correct label. Scanning from the goto should have already hit the target and closure.
            DebugUtil.Assert(switchExpr.ArgumentExpr.Kind != EXPRKIND.CONSTANT);

            ScanExpr(switchExpr.ArgumentExpr, ref currentBitset, ref errorBitset);
            BitSet tempBitset = new BitSet();

            // All switch labels are reachable.
            EXPRSTMT expr = switchExpr.BodiesExpr;
            while (expr != null)
            {
                EXPRSTMT chdStmtExpr = expr;
                expr = expr.NextStatement;
                EXPRSWITCHLABEL labelExpr = chdStmtExpr as EXPRSWITCHLABEL;
                if (labelExpr.StatementsExpr != null)
                {
                    tempBitset.SetBit(currentBitset);
                    if (ScanLabel(labelExpr, ref tempBitset))
                    {
                        ScanStmts(labelExpr.StatementsExpr, ref tempBitset, ref errorBitset);
                    }
                }
            }
        }

        //------------------------------------------------------------
        // FlowChecker.ScanExpr
        //
        /// <summary>
        /// Scan an expression without a true-set and false-set.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanExpr(
            EXPR expr,
            ref BitSet currentBitset,
            ref BitSet errorBitset)
        {
        LRepeat:
            if (expr == null)
            {
                return;
            }

            // Callback for refactoring.
            if (this.compileCallBack != null)
            {
                this.compileCallBack.ProcessExpression(expr, currentBitset);
            }

            switch (expr.Kind)
            {
                case EXPRKIND.LDTMP:
                case EXPRKIND.FREETMP:
                case EXPRKIND.NOOP:
                case EXPRKIND.DELIM:
                case EXPRKIND.MULTIGET:
                case EXPRKIND.WRAP:
                case EXPRKIND.FUNCPTR:
                case EXPRKIND.TYPEOF:
                case EXPRKIND.SIZEOF:
                case EXPRKIND.CONSTANT:
                case EXPRKIND.SYSTEMTYPE:       // CS3
                case EXPRKIND.FIELDINFO:        // CS3
                case EXPRKIND.METHODINFO:       // CS3
                case EXPRKIND.CONSTRUCTORINFO:  // CS3
                case EXPRKIND.ERROR:
                    break;

                // These are simple ones that use tail recursion.
                case EXPRKIND.LOGNOT:
                    expr = expr.AsBIN.Operand1;
                    goto LRepeat;

                case EXPRKIND.ARRINIT:
                    expr = (expr as EXPRARRINIT).ArgumentsExpr;
                    goto LRepeat;

                case EXPRKIND.CAST:
                    expr = (expr as EXPRCAST).Operand;
                    goto LRepeat;

                case EXPRKIND.CONCAT:
                    if ((expr.Flags & EXPRFLAG.UNREALIZEDCONCAT) != 0)
                    {
                        compiler.FuncBRec.RealizeStringConcat(expr as EXPRCONCAT);
                    }
                    expr = (expr as EXPRCONCAT).List;
                    goto LRepeat;

                case EXPRKIND.MULTI:
                    ScanExpr((expr as EXPRMULTI).LeftExpr, ref currentBitset, ref errorBitset);
                    expr = (expr as EXPRMULTI).OperandExpr;
                    goto LRepeat;

                case EXPRKIND.STTMP:
                    expr = (expr as EXPRSTTMP).SourceExpr;
                    goto LRepeat;

                case EXPRKIND.PROP:
                    ScanExpr((expr as EXPRPROP).ObjectExpr, ref currentBitset, ref errorBitset);
                    expr = (expr as EXPRPROP).ArgumentsExpr;
                    goto LRepeat;

                case EXPRKIND.USERLOGOP:
                    ScanExpr((expr as EXPRUSERLOGOP).OpX, ref currentBitset, ref errorBitset);
                    expr = ((expr as EXPRUSERLOGOP).CallOp as EXPRCALL).ArgumentsExpr.AsBIN.Operand2;
                    goto LRepeat;

                case EXPRKIND.DBLQMARK:
                    ScanExpr((expr as EXPRDBLQMARK).TestExpr, ref currentBitset, ref errorBitset);
                    expr = (expr as EXPRDBLQMARK).ElseExpr;
                    goto LRepeat;

                case EXPRKIND.ZEROINIT:
                    expr = (expr as EXPRZEROINIT).Operand;
                    if (expr != null)
                    {
                        ScanAssign(expr, null, ref currentBitset, ref errorBitset, false);
                    }
                    break;

                case EXPRKIND.LOGOR:
                case EXPRKIND.LOGAND:
                case EXPRKIND.QMARK:
                    // These need to propogate true-set and false-set information.
                    ScanExprCond(expr, ref currentBitset, ref errorBitset);
                    break;

                case EXPRKIND.ANONMETH:
                case EXPRKIND.LAMBDAEXPR:
                    {
                        AnonMethInfo amInfo = (expr as EXPRANONMETH).AnonymousMethodInfo;
                        // Save the current state
                        if (!amInfo.BSetEnter.FInited())
                        {
                            amInfo.BSetEnter.SetBit(currentBitset);
                        }
                        else
                        {
                            amInfo.BSetEnter.Intersect(currentBitset);
                        }
                        amInfo.Seen = true;
                    }
                    break;

                case EXPRKIND.LOCAL:
                    ScanLocal(expr as EXPRLOCAL, ref currentBitset, ref errorBitset);
                    break;

                case EXPRKIND.CALL:
                    // This assigns to the object being called on.
                    if ((expr.Flags & (EXPRFLAG.NEWSTRUCTASSG | EXPRFLAG.IMPLICITSTRUCTASSG)) != 0)
                    {
                        ScanAssign(
                            (expr as EXPRCALL).ObjectExpr,
                            (expr as EXPRCALL).ArgumentsExpr,
                            ref currentBitset,
                            ref errorBitset,
                            false);
                    }
                    else
                    {
                        ScanExpr((expr as EXPRCALL).ObjectExpr, ref currentBitset, ref errorBitset);
                        ScanExpr((expr as EXPRCALL).ArgumentsExpr, ref currentBitset, ref errorBitset);
                    }
                    if ((expr.Flags & EXPRFLAG.HASREFPARAM) != 0)
                    {
                        // Look over the args again and assign to any out params...
                        EXPR temp = (expr as EXPRCALL).ArgumentsExpr;
                        while (temp != null)
                        {
                            EXPR arg;
                            if (temp.Kind == EXPRKIND.LIST)
                            {
                                arg = temp.AsBIN.Operand1;
                                temp = temp.AsBIN.Operand2;
                            }
                            else
                            {
                                arg = temp;
                                temp = null;
                            }
                            if (arg.TypeSym.IsPARAMMODSYM)
                            {
                                MarkAsAlias(arg);
                                ScanAssign(arg, null, ref currentBitset, ref errorBitset, false);
                            }
                        }
                    }
                    break;

                case EXPRKIND.FIELD:
                    if (ScanField(expr as EXPRFIELD, ref currentBitset, ref errorBitset))
                    {
                        expr = (expr as EXPRFIELD).ObjectExpr;
                        goto LRepeat;
                    }
                    break;

                case EXPRKIND.ADDR:
                    DebugUtil.Assert(expr.AsBIN.Operand2 == null);
                    MarkAsAlias(expr.AsBIN.Operand1);
                    ScanAssign(
                        expr.AsBIN.Operand1,
                        expr.AsBIN.Operand2,
                        ref currentBitset,
                        ref errorBitset,
                        true);
                    break;

                case EXPRKIND.ASSG:
                    ScanAssign(
                        expr.AsBIN.Operand1,
                        expr.AsBIN.Operand2,
                        ref currentBitset,
                        ref errorBitset,
                        false);
                    break;

                default:
                    if ((expr.Flags & EXPRFLAG.BINOP) == 0)
                    {
                        DebugUtil.Assert(compiler.ErrorCount() > 0);
                        break;
                    }
                    ScanExpr(expr.AsBIN.Operand1, ref currentBitset, ref errorBitset);
                    expr = expr.AsBIN.Operand2;
                    goto LRepeat;
            }
        }

        //------------------------------------------------------------
        // FlowChecker.ScanExpr
        //
        /// <summary>
        /// Scan an expression with a true-set and false-set.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        /// <param name="trueBitset"></param>
        /// <param name="falseBitset"></param>
        /// <param name="sense"></param>
        //------------------------------------------------------------
        private void ScanExpr(
            EXPR expr,
            ref BitSet currentBitset,
            ref BitSet errorBitset,
            BitSet trueBitset,
            BitSet falseBitset,
            bool sense)
        {
            DebugUtil.Assert(trueBitset != null && falseBitset != null);
            BitSet trueBitset2 = null;
            BitSet falseBitset2 = null;

        LRepeat:
            if (expr == null)
            {
                return;
            }

            if (this.compileCallBack != null)
            {
                this.compileCallBack.ProcessExpression(expr, currentBitset);
            }

            if (!sense)
            {
                Util.Swap<BitSet>(ref trueBitset, ref falseBitset);
                sense = true;
            }

            switch (expr.Kind)
            {
                default:
                    ScanExpr(expr, ref currentBitset, ref errorBitset);
                    break;

                case EXPRKIND.LOGNOT:
                    expr = expr.AsBIN.Operand1;
                    sense = !sense;
                    goto LRepeat;

                case EXPRKIND.CONSTANT:
                    if ((expr as EXPRCONSTANT).ConstVal.GetInt() == 0)
                    {
                        Util.Swap<BitSet>(ref trueBitset, ref falseBitset);
                    }
                    trueBitset.SetBit(currentBitset);
                    falseBitset.SetBitRange(0, this.uninitedVarCount);
                    return;

                case EXPRKIND.LOGOR:
                    DebugUtil.Assert(sense);
                    sense = false;
                    Util.Swap<BitSet>(ref trueBitset, ref falseBitset);
                    // Fall through.
                    goto case EXPRKIND.LOGAND;

                case EXPRKIND.LOGAND:
                    ScanExpr(
                        expr.AsBIN.Operand1,
                        ref currentBitset,
                        ref errorBitset,
                        trueBitset,
                        falseBitset,
                        sense);

                    falseBitset2 = new BitSet(trueBitset);
                    currentBitset.SetBit(trueBitset);
                    ScanExpr(
                        expr.AsBIN.Operand2,
                        ref currentBitset,
                        ref errorBitset,
                        trueBitset,
                        falseBitset2,
                        sense);
                    falseBitset.Intersect(falseBitset2);
                    return;

                case EXPRKIND.QMARK:
                    EXPR op1 = expr.AsBIN.Operand1;
                    EXPR op2 = expr.AsBIN.Operand2;
                    EXPR op3 = op2.AsBIN.Operand2;
                    op2 = op2.AsBIN.Operand1;

                    // We have op1 ? op2 : op3
                    ScanExpr(op1, ref currentBitset, ref errorBitset, trueBitset, falseBitset, true);
                    currentBitset.SetBit(trueBitset);
                    falseBitset2 = new BitSet(currentBitset);
                    ScanExpr(op2, ref currentBitset, ref errorBitset, trueBitset, falseBitset2, true);
                    currentBitset.SetBit(falseBitset);
                    trueBitset2 = new BitSet(currentBitset);
                    ScanExpr(op3, ref currentBitset, ref errorBitset, trueBitset2, falseBitset, true);
                    trueBitset.Intersect(trueBitset2);
                    falseBitset.Intersect(falseBitset2);
                    return;
            }

            trueBitset.SetBit(currentBitset);
            falseBitset.SetBit(currentBitset);
        }

        //------------------------------------------------------------
        // FlowChecker.ScanExprCond
        //
        /// <summary>
        /// Scan a conditional expression that needs to process a true-set and false-set.
        /// The resulting bitset is the intersection of the resulting true-set and false-set.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanExprCond(EXPR expr, ref BitSet currentBitset, ref BitSet errorBitset)
        {
            BitSet trueBitset = new BitSet(currentBitset);
            BitSet falseBitset = new BitSet(currentBitset);

            ScanExpr(expr, ref currentBitset, ref errorBitset, trueBitset, falseBitset, true);

            // Intersect the true and false sets.
            currentBitset.SetBit(trueBitset);
            currentBitset.Intersect(falseBitset);
        }

        //------------------------------------------------------------
        // FlowChecker.ScanLocal
        //
        /// <summary>
        /// Process a local usage (not assignment).
        /// Mark the local as used (and whether it needs to be hoisted) and
        /// check that it is definitely assigned.
        /// </summary>
        /// <param name="localExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        //------------------------------------------------------------
        private void ScanLocal(EXPRLOCAL localExpr, ref BitSet currentBitset, ref BitSet errorBitset)
        {
            int ibitMin;
            int ibitLim;

            MarkAsUsed(localExpr, true);

            LOCVARSYM localSym = localExpr.LocVarSym;
            DebugUtil.Assert(localSym != null && !localSym.IsConst);

            // Out params get assigned at the conclusion of the call expr.
#if DEBUG
            ibitMin = localSym.LocSlotInfo.JbitDefAssg() - 1;
            ibitLim = ibitMin + GetCbit(localSym.TypeSym);
#endif
            if ((ibitMin = localSym.LocSlotInfo.JbitDefAssg() - 1) >= 0 &&
                (!localExpr.TypeSym.IsPARAMMODSYM || (localExpr.TypeSym as PARAMMODSYM).IsRef) &&
                !currentBitset.TestAllRange(ibitMin, ibitLim = ibitMin + GetCbit(localSym.TypeSym)) &&
                !errorBitset.TestAllRange(ibitMin, ibitLim))
            {
                errorBitset.SetBitRange(ibitMin, ibitLim);
                if (localSym.IsThis)
                {
                    compiler.Error(localExpr.TreeNode, CSCERRID.ERR_UseDefViolationThis);
                }
                else if (
                    localSym.LocSlotInfo.IsParameter &&
                    localSym.LocSlotInfo.IsReferenceParameter)
                {
                    compiler.Error(
                        localExpr.TreeNode,
                        CSCERRID.ERR_UseDefViolationOut,
                        new ErrArg(localSym));
                }
                else
                {
                    compiler.Error(
                        localExpr.TreeNode,
                        CSCERRID.ERR_UseDefViolation,
                        new ErrArg(localSym));
                }
            }
        }

        //------------------------------------------------------------
        // FlowChecker.ScanField
        //
        /// <summary>
        /// Scan a field on use (not assignment).
        /// Mark any base local as used (and whether it should be hoisted).
        /// Check for definite assignment. Return true iff the object should be scanned.
        /// </summary>
        /// <param name="fieldExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool ScanField(EXPRFIELD fieldExpr, ref BitSet currentBitset, ref BitSet errorBitset)
        {
            int ibitMin;
            int ibitLim;

            bool isMarked = MarkAsUsed(fieldExpr.ObjectExpr, true);

            if ((ibitMin = fieldExpr.Offset - 1) >= 0)
            {
                if ((fieldExpr.Flags & EXPRFLAG.MEMBERSET) == 0 &&
                    !currentBitset.TestAllRange(ibitMin, ibitLim = ibitMin + GetCbit(
                        compiler.MainSymbolManager.SubstType(
                            fieldExpr.FieldWithType.FieldSym.TypeSym,
                            fieldExpr.FieldWithType.AggTypeSym,
                            null))) &&
                    !errorBitset.TestAllRange(ibitMin, ibitLim))
                {
                    errorBitset.SetBitRange(ibitMin, ibitLim);
                    compiler.Error(
                        fieldExpr.TreeNode,
                        CSCERRID.ERR_UseDefViolationField,
                        new ErrArg(fieldExpr.FieldWithType.FieldSym.Name));
                }
                return false;
            }

            return fieldExpr.FieldWithType.FieldSym.FixedAggSym == null || !isMarked;
        }

        //------------------------------------------------------------
        // FlowChecker.ScanAssign
        //
        /// <summary>
        /// Sets the appropriate definite assignment bits, if any.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="valueExpr"></param>
        /// <param name="currentBitset"></param>
        /// <param name="errorBitset"></param>
        /// <param name="valueUsed"></param>
        //------------------------------------------------------------
        private void ScanAssign(
            EXPR expr,
            EXPR valueExpr,
            ref BitSet currentBitset,
            ref BitSet errorBitset,
            bool valueUsed)
        {
            int jbit;
            TYPESYM typeSym;

            switch (expr.Kind)
            {
                case EXPRKIND.LOCAL:
                    MarkAsUsed(expr, valueUsed);
                    ScanExpr(valueExpr, ref currentBitset, ref errorBitset);
                    jbit = (expr as EXPRLOCAL).LocVarSym.LocSlotInfo.JbitDefAssg();
                    if (jbit == 0)
                    {
                        return;
                    }
                    typeSym = (expr as EXPRLOCAL).LocVarSym.TypeSym;
                    break;

                case EXPRKIND.FIELD:
                    jbit = (expr as EXPRFIELD).Offset;
                    if (jbit != 0)
                    {
                        // Shouldn't scan the expr, just the value.
                        MarkAsUsed(expr, valueUsed);
                        ScanExpr(valueExpr, ref currentBitset, ref errorBitset);
                        typeSym = compiler.MainSymbolManager.SubstType(
                            (expr as EXPRFIELD).FieldWithType.FieldSym.TypeSym,
                            (expr as EXPRFIELD).FieldWithType.AggTypeSym,
                            null);
                        break;
                    }
                    // Fall through.
                    goto default;

                default:
                    ScanExpr(expr, ref currentBitset, ref errorBitset);
                    ScanExpr(valueExpr, ref currentBitset, ref errorBitset);
                    return;
            }

            int ibit = jbit - 1;
            currentBitset.SetBitRange(ibit, ibit + GetCbit(typeSym));
        }

        //------------------------------------------------------------
        // FlowChecker.MarkAsUsed
        //
        /// <summary>
        /// Rips through any EK_FIELDs and EK_INDIRs looking for a base EK_LOCAL.
        /// Tracks usage of the local in anon meths.
        /// If fValUsed is true, marks the local as used.
        /// Returns true iff it finds a base local.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="valueUsed"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool MarkAsUsed(EXPR expr, bool valueUsed)
        {
            if (!valueUsed && this.anonMethInfo == null)
            {
                return false;
            }

        LRepeat:
            if (expr == null)
            {
                return false;
            }
            switch (expr.Kind)
            {
                default:
                    return false;

                case EXPRKIND.INDIR:
                    DebugUtil.Assert(expr.AsBIN.Operand2 == null);
                    expr = expr.AsBIN.Operand1;
                    goto LRepeat;

                case EXPRKIND.FIELD:
                    expr = (expr as EXPRFIELD).ObjectExpr;
                    goto LRepeat;

                case EXPRKIND.LOCAL:
                    break;
            }

            LOCVARSYM localSym = (expr as EXPRLOCAL).LocVarSym;
            DebugUtil.Assert(localSym != null && !localSym.IsConst);

            if (valueUsed)
            {
                localSym.LocSlotInfo.IsUsed = true;
            }

            // If we're inside an anonymous method block, we need to mark any
            // locals that get used so they can be moved from the localSym socpeSym
            // to a heap allocated object for the delegate to access
            SCOPESYM socpeSym;
            if (this.anonMethInfo != null &&
                localSym.UsedInAnonMeth &&
                (socpeSym = (localSym.ParentSym as SCOPESYM)).NestingOrder
                    < this.anonMethInfo.ParametersScopeSym.NestingOrder)
            {
                // The localSym is outside this anon meth so need to hoist it.
                // We checked for legality during bindToLocal.
                localSym.HoistForAnonMeth = true;

                // Every outer anonymous delegate between where it's used and where
                // it's declared must be made non-static, and moved to the $locals class
                for (AnonMethInfo amInfo = this.anonMethInfo;
                    amInfo != null && amInfo.ParametersScopeSym.NestingOrder > socpeSym.NestingOrder;
                    amInfo = amInfo.OuterInfo)
                {
                    if (localSym.IsThis)
                    {
                        amInfo.UsesThis = true;
                    }
                    else
                    {
                        amInfo.UsesLocals = true;
                    }
                    if (amInfo.ScopeSym.NestingOrder < socpeSym.NestingOrder)
                    {
                        amInfo.ScopeSym = socpeSym;
                    }
                }
            }

            return true;
        }

        //------------------------------------------------------------
        // FlowChecker.MarkAsAlias
        //
        /// <summary>
        /// Mark a local as aliased.
        /// </summary>
        /// <param name="expr"></param>
        //------------------------------------------------------------
        private void MarkAsAlias(EXPR expr)
        {
            if (expr != null && expr.Kind == EXPRKIND.LOCAL)
            {
                (expr as EXPRLOCAL).LocVarSym.LocSlotInfo.AliasPossible = true;
            }
        }

        //------------------------------------------------------------
        // FlowChecker.GetCbit
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetCbit(TYPESYM type)
        {
            return GetCbit(this.Compiler, type);
        }

        //------------------------------------------------------------
        // FlowChecker.GetIbit
        //
        /// <summary></summary>
        /// <param name="fld"></param>
        /// <param name="ats"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetIbit(MEMBVARSYM fld, AGGTYPESYM ats)
        {
            return GetIbit(this.Compiler, fld, ats);
        }

        //------------------------------------------------------------
        // FlowChecker.GetIbit
        //
        /// <summary></summary>
        /// <param name="fwt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int GetIbit(FieldWithType fwt)
        {
            return GetIbit(this.Compiler, fwt.FieldSym, fwt.AggTypeSym);
        }
    }
}
