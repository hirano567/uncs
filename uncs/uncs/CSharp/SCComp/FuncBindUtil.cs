//============================================================================
// FncBindUtil.cs
//
// 2016/09/16 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace Uncs
{
    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC.CreateAnonLocalName (1)
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAnonLocalName(string kind, string name)
        {
            if (String.IsNullOrEmpty(kind))
            {
                return CreateAnonLocalName(name);
            }
            else
            {
                if (this.localCount >= 0xffff)
                {
                    Compiler.Error(treeNode, CSCERRID.ERR_TooManyLocals);
                }
                return String.Format("<{0}:{1}>_local<{2}>", kind, name, this.localCount++);
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAnonLocalName (2)
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string CreateAnonLocalName(string name)
        {
            if (this.localCount >= 0xffff)
            {
                Compiler.Error(treeNode, CSCERRID.ERR_TooManyLocals);
            }
            return String.Format("<{0}>_local<{1}>", name, this.localCount++);
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateNewLocVarSym
        //
        /// <summary>Increments the following values:
        /// FUNCBREC.localCount,
        /// FUNCBREC.unreferencedVarCount and
        /// FUNCBREC.uninitedVarCount.
        /// And sets LOCVARSYM.LocSlotInfo.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeSym"></param>
        /// <param name="parentSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal LOCVARSYM CreateNewLocVarSym(
            string name,
            TYPESYM typeSym,
            PARENTSYM parentSym)
        {
            SYM sym = Compiler.LocalSymbolManager.LookupLocalSym(
                name,
                parentSym,
                SYMBMASK.LOCVARSYM);
            if (sym != null)
            {
                return sym as LOCVARSYM;
            }

            LOCVARSYM locSym = Compiler.LocalSymbolManager.CreateLocalSym(
                SYMKIND.LOCVARSYM,
                name,
                parentSym) as LOCVARSYM;
            locSym.TypeSym = typeSym;

            StoreInCache(null, name, locSym, null, true);

            ++this.localCount;
            if (this.localCount > 0xffff)
            {
                Compiler.Error(treeNode, CSCERRID.ERR_TooManyLocals);
            }
            ++this.unreferencedVarCount;

            locSym.LocSlotInfo.SetJbitDefAssg(this.uninitedVarCount + 1);
            int cbit = FlowChecker.GetCbit(Compiler, locSym.TypeSym);
            this.uninitedVarCount += cbit;

            return locSym;
        }

        //------------------------------------------------------------
        // For System.Func
        //------------------------------------------------------------
        private const int systemFuncDelegateCount = 17;
        private METHSYM[] systemFuncInvokeMethodSyms = null;

        //------------------------------------------------------------
        // FUNCBREC.InitSystemFuncInvokeMethodSyms
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void InitSystemFuncInvokeMethodSyms()
        {
            if (systemFuncInvokeMethodSyms != null)
            {
                return;
            }

            systemFuncInvokeMethodSyms = new METHSYM[systemFuncDelegateCount];
            for (int i = 0; i < systemFuncDelegateCount; i++)
            {
                systemFuncInvokeMethodSyms[i] = null;
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.GetFuncTypeSym
        //
        /// <summary></summary>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetFuncTypeSym(int arity)
        {
            PREDEFTYPE pt;

            switch (arity)
            {
                case 1: pt = PREDEFTYPE.G1_FUNC; break;
                case 2: pt = PREDEFTYPE.G2_FUNC; break;
                case 3: pt = PREDEFTYPE.G3_FUNC; break;
                case 4: pt = PREDEFTYPE.G4_FUNC; break;
                case 5: pt = PREDEFTYPE.G5_FUNC; break;

                default:
                    DebugUtil.Assert(false);
                    return null;
            }
            return Compiler.GetOptPredefType(pt, true);
        }

        //------------------------------------------------------------
        // FUNCBREC.GetFuncInvokeMethod
        //
        /// <summary></summary>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal METHSYM GetFuncInvokeMethod(int arity)
        {
            if (arity < 0 || arity >= systemFuncDelegateCount)
            {
                return null;
            }
            InitSystemFuncInvokeMethodSyms();

            METHSYM invokeSym = systemFuncInvokeMethodSyms[arity];
            if (invokeSym != null)
            {
                return invokeSym;
            }

            AGGTYPESYM funcAts = GetFuncTypeSym(arity);
            if (funcAts == null)
            {
                return null;
            }

            invokeSym = Compiler.MainSymbolManager.LookupAggMember(
                "Invoke",
                funcAts.GetAggregate(),
                SYMBMASK.METHSYM) as METHSYM;

            systemFuncInvokeMethodSyms[arity] = invokeSym;
            return invokeSym;
        }

        //------------------------------------------------------------
        // for System.Action
        //------------------------------------------------------------
        private const int systemActionDelegateCount = 17;
        private METHSYM[] systemActionInvokeMethodSyms = null;

        //------------------------------------------------------------
        // FUNCBREC.InitSystemActionInvokeMethodSyms
        //
        /// <summary></summary>
        //------------------------------------------------------------
        private void InitSystemActionInvokeMethodSyms()
        {
            if (systemActionInvokeMethodSyms != null)
            {
                return;
            }

            systemActionInvokeMethodSyms = new METHSYM[systemActionDelegateCount];
            for (int i = 0; i < systemActionDelegateCount; i++)
            {
                systemActionInvokeMethodSyms[i] = null;
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.GetActionTypeSym
        //
        /// <summary></summary>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetActionTypeSym(int arity)
        {
            PREDEFTYPE pt;

            switch (arity)
            {
                case 0: pt = PREDEFTYPE.ACTION; break;
                case 1: pt = PREDEFTYPE.G1_ACTION; break;
                case 2: pt = PREDEFTYPE.G2_ACTION; break;
                case 3: pt = PREDEFTYPE.G3_ACTION; break;
                case 4: pt = PREDEFTYPE.G4_ACTION; break;
                case 5: pt = PREDEFTYPE.G5_ACTION; break;
                case 6: pt = PREDEFTYPE.G6_ACTION; break;
                case 7: pt = PREDEFTYPE.G7_ACTION; break;
                case 8: pt = PREDEFTYPE.G8_ACTION; break;
                case 9: pt = PREDEFTYPE.G9_ACTION; break;
                case 10: pt = PREDEFTYPE.G10_ACTION; break;
                case 11: pt = PREDEFTYPE.G11_ACTION; break;
                case 12: pt = PREDEFTYPE.G12_ACTION; break;
                case 13: pt = PREDEFTYPE.G13_ACTION; break;
                case 14: pt = PREDEFTYPE.G14_ACTION; break;
                case 15: pt = PREDEFTYPE.G15_ACTION; break;
                case 16: pt = PREDEFTYPE.G16_ACTION; break;

                default:
                    DebugUtil.Assert(false);
                    return null;
            }
            return Compiler.GetOptPredefType(pt, true);
        }

        //------------------------------------------------------------
        // FUNCBREC.GetActionInvokeMethod
        //
        /// <summary></summary>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal METHSYM GetActionInvokeMethod(int arity)
        {
            if (arity < 0 || arity >= systemActionDelegateCount)
            {
                return null;
            }
            InitSystemActionInvokeMethodSyms();

            METHSYM invokeSym = systemActionInvokeMethodSyms[arity];
            if (invokeSym != null)
            {
                return invokeSym;
            }

            AGGTYPESYM actionAts = GetActionTypeSym(arity);
            if (actionAts == null)
            {
                return null;
            }

            invokeSym = Compiler.MainSymbolManager.LookupAggMember(
                "Invoke",
                actionAts.GetAggregate(),
                SYMBMASK.METHSYM) as METHSYM;

            systemActionInvokeMethodSyms[arity] = invokeSym;
            return invokeSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.GetTypeFromHandleMethodSym
        //
        /// <summary>
        /// <para>Defined in FuncBindUtil.cs</para>
        /// </summary>
        //------------------------------------------------------------
        internal static METHSYM GetTypeFromHandleMethodSym = null;

        //------------------------------------------------------------
        // CreateGetTypeFromHandleMethodSym
        //
        /// <summary>
        /// <para>Defined in FuncBindUtil.cs</para>
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal METHSYM CreateGetTypeFromHandleMethodSym(BASENODE treeNode)
        {
            if (GetTypeFromHandleMethodSym != null)
            {
                return GetTypeFromHandleMethodSym;
            }

            // System.RuntimeTypeHandle
            TYPESYM handleTypeSym = this.GetRequiredPredefinedType(PREDEFTYPE.TYPEHANDLE);
            DebugUtil.Assert(handleTypeSym != null);

            TypeArray paramArray = new TypeArray();
            paramArray.Add(handleTypeSym);
            paramArray = Compiler.MainSymbolManager.AllocParams(paramArray);

            GetTypeFromHandleMethodSym = FindPredefMeth(
                treeNode,
                PREDEFNAME.GETTYPEFROMHANDLE,
                this.GetRequiredPredefinedType(PREDEFTYPE.TYPE),
                paramArray,
                true,
                MemLookFlagsEnum.None);
            DebugUtil.Assert(GetTypeFromHandleMethodSym != null);

            return GetTypeFromHandleMethodSym;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindTypeOf
        //
        /// <summary>
        /// <para>Defined in FuncBindUtil.cs</para>
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private EXPRTYPEOF BindTypeOf(TYPESYM typeSym)
        {
            DebugUtil.Assert(typeSym != null);
            if (typeSym.IsERRORSYM)
            {
                goto LERROR;
            }

            if (typeSym == GetVoidType())
            {
                typeSym = this.GetRequiredPredefinedType(PREDEFTYPE.SYSTEMVOID);
            }

            EXPRTYPEOF typeofExpr = null;
            typeofExpr = NewExpr(
                treeNode,
                EXPRKIND.TYPEOF,
                this.GetRequiredPredefinedType(PREDEFTYPE.TYPE)) as EXPRTYPEOF;
            typeofExpr.MethodSym = CreateGetTypeFromHandleMethodSym(null);
            typeofExpr.SourceTypeSym = typeSym;
#if false
            if (operandNode.Kind == NODEKIND.OPENTYPE)
            {
                DebugUtil.Assert(
                    typeSym.IsAGGTYPESYM &&
                    (typeSym as AGGTYPESYM).AllTypeArguments.Count > 0 &&
                    (typeSym as AGGTYPESYM).AllTypeArguments[(typeSym as AGGTYPESYM).AllTypeArguments.Count - 1].IsUNITSYM);
                rval.Flags |= EXPRFLAG.OPENTYPE;
            }
#endif

            typeofExpr.Flags |= EXPRFLAG.CANTBENULL;
            return typeofExpr;

        LERROR:
            //return NewError(treeNode, this.GetRequiredPredefinedType(PREDEFTYPE.TYPE));
            return null;
        }

        //------------------------------------------------------------
        // FUNCBREC.OperatorToExpressionType
        //
        /// <summary></summary>
        /// <param name="op"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ExpressionType OperatorToExpressionType(OPERATOR op)
        {
            switch (op)
            {
                case OPERATOR.NONE:
                    //return ExpressionType;
                    break;

                case OPERATOR.ASSIGN:
                    return ExpressionType.Assign;
                case OPERATOR.ADDEQ:
                    return ExpressionType.AddAssign;
                case OPERATOR.SUBEQ:
                    return ExpressionType.SubtractAssign;
                case OPERATOR.MULEQ:
                    return ExpressionType.MultiplyAssign;
                case OPERATOR.DIVEQ:
                    return ExpressionType.DivideAssign;
                case OPERATOR.MODEQ:
                    return ExpressionType.ModuloAssign;
                case OPERATOR.ANDEQ:
                    return ExpressionType.AndAssign;
                case OPERATOR.XOREQ:
                    return ExpressionType.ExclusiveOrAssign;
                case OPERATOR.OREQ:
                    return ExpressionType.OrAssign;
                case OPERATOR.LSHIFTEQ:
                    return ExpressionType.LeftShiftAssign;
                case OPERATOR.RSHIFTEQ:
                    return ExpressionType.RightShiftAssign;

                case OPERATOR.QUESTION:
                    //return ExpressionType;
                    break;
                case OPERATOR.VALORDEF:
                    //return ExpressionType;
                    break;

                case OPERATOR.LOGOR:
                    return ExpressionType.Or;
                case OPERATOR.LOGAND:
                    return ExpressionType.And;

                case OPERATOR.BITOR:
                    //return ExpressionType;
                case OPERATOR.BITXOR:
                    //return ExpressionType;
                case OPERATOR.BITAND:
                    //return ExpressionType;
                    break;

                case OPERATOR.EQ:
                    return ExpressionType.Equal;
                case OPERATOR.NEQ:
                    return ExpressionType.NotEqual;
                case OPERATOR.LT:
                    return ExpressionType.LessThan;
                case OPERATOR.LE:
                    return ExpressionType.LessThanOrEqual;
                case OPERATOR.GT:
                    return ExpressionType.GreaterThan;
                case OPERATOR.GE:
                    return ExpressionType.GreaterThanOrEqual;
                case OPERATOR.IS:
                    return ExpressionType.TypeIs;
                case OPERATOR.AS:
                    return ExpressionType.TypeAs;
                case OPERATOR.LSHIFT:
                    return ExpressionType.LeftShift;
                case OPERATOR.RSHIFT:
                    return ExpressionType.RightShift;
                case OPERATOR.ADD:
                    return ExpressionType.Add;
                case OPERATOR.SUB:
                    return ExpressionType.Subtract;
                case OPERATOR.MUL:
                    return ExpressionType.Multiply;
                case OPERATOR.DIV:
                    return ExpressionType.Divide;
                case OPERATOR.MOD:
                    return ExpressionType.Modulo;

                case OPERATOR.NOP:
                    //return ExpressionType;
                    break;

                case OPERATOR.UPLUS:
                    return ExpressionType.UnaryPlus;
                case OPERATOR.NEG:
                    return ExpressionType.Negate;
                case OPERATOR.BITNOT:
                    return ExpressionType.OnesComplement;
                case OPERATOR.LOGNOT:
                    return ExpressionType.Not;
                case OPERATOR.PREINC:
                    return ExpressionType.PreIncrementAssign;
                case OPERATOR.PREDEC:
                    return ExpressionType.PreDecrementAssign;

                case OPERATOR.TYPEOF:
                    //return ExpressionType;
                case OPERATOR.SIZEOF:
                    //return ExpressionType;
                case OPERATOR.CHECKED:
                    //return ExpressionType;
                case OPERATOR.UNCHECKED:
                    //return ExpressionType;
                case OPERATOR.MAKEREFANY:
                    //return ExpressionType;
                case OPERATOR.REFVALUE:
                    //return ExpressionType;
                case OPERATOR.REFTYPE:
                    //return ExpressionType;
                case OPERATOR.ARGS:
                    //return ExpressionType;
                case OPERATOR.CAST:
                    //return ExpressionType;
                case OPERATOR.INDIR:
                    //return ExpressionType;
                case OPERATOR.ADDR:
                    //return ExpressionType;
                case OPERATOR.COLON:
                    //return ExpressionType;
                case OPERATOR.THIS:
                    //return ExpressionType;
                case OPERATOR.BASE:
                    //return ExpressionType;
                case OPERATOR.NULL:
                    //return ExpressionType;
                case OPERATOR.TRUE:
                    //return ExpressionType;
                case OPERATOR.FALSE:
                    //return ExpressionType;
                case OPERATOR.CALL:
                    //return ExpressionType;
                case OPERATOR.DEREF:
                    //return ExpressionType;
                case OPERATOR.PAREN:
                    //return ExpressionType;
                    break;

                case OPERATOR.POSTINC:
                    return ExpressionType.PostIncrementAssign;
                case OPERATOR.POSTDEC:
                    return ExpressionType.PostDecrementAssign;

                case OPERATOR.DOT:
                    //return ExpressionType;
                case OPERATOR.IMPLICIT:
                    //return ExpressionType;
                case OPERATOR.EXPLICIT:
                    //return ExpressionType;
                case OPERATOR.EQUALS:
                    //return ExpressionType;
                case OPERATOR.COMPARE:
                    //return ExpressionType;
                case OPERATOR.DEFAULT:
                    //return ExpressionType;
                    break;

                // CS3
                case OPERATOR.LAMBDA:
                    return ExpressionType.Lambda;

                default:
                    break;
            }
            DebugUtil.Assert(false);
            return (ExpressionType)(-1);
        }

        //------------------------------------------------------------
        // FUNCBREC.ExpressionTypeToChecked
        //
        /// <summary></summary>
        /// <param name="exType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ExpressionType ExpressionTypeToChecked(ExpressionType exType)
        {
            switch (exType)
            {
                case ExpressionType.Add:
                    return ExpressionType.AddChecked;
                case ExpressionType.AddAssign:
                    return ExpressionType.AddAssignChecked;
                case ExpressionType.Convert:
                    return ExpressionType.ConvertChecked;
                case ExpressionType.Multiply:
                    return ExpressionType.MultiplyChecked;
                case ExpressionType.MultiplyAssign:
                    return ExpressionType.MultiplyAssignChecked;
                case ExpressionType.Negate:
                    return ExpressionType.NegateChecked;
                case ExpressionType.Subtract:
                    return ExpressionType.SubtractChecked;
                case ExpressionType.SubtractAssign:
                    return ExpressionType.SubtractAssignChecked;

                default:
                    return exType;
            }
        }
    }
}
