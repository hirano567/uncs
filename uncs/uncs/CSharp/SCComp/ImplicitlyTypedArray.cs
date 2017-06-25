//============================================================================
// ImplicitlyTypedArray.cs
//
// 2016/01/11 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
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
        // FUNCBREC.BindImplicitlyTypedArrayInit
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="arrayTypeSym"></param>
        /// <param name="argListExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindImplicitlyTypedArrayInit(
            UNOPNODE treeNode,
            ref TYPESYM arrayTypeSym)
        {
            EXPRARRINIT arrayInitExpr = NewExpr(
                treeNode,
                EXPRKIND.ARRINIT,
                null) as EXPRARRINIT;
            arrayInitExpr.Flags |= EXPRFLAG.CANTBENULL;

            TYPESYM elementTypeSym = null;

            arrayInitExpr.DimSizes = new List<int>();
            arrayInitExpr.DimSizes.Add(-1);

            //bool isConstant =
            //    elementTypeSym.IsSimpleType() &&
            //    BSYMMGR.GetAttrArgSize(elementTypeSym.GetPredefType()) > 0;
            int nonConstCount = 0;
            int constCount = 0;
            bool hasSideEffects = false;

            BindImplicitlyTypedArrayInitCore(
                treeNode,
                ref elementTypeSym,
                arrayInitExpr.DimSizes,
                0,
                ref arrayInitExpr.ArgumentsExpr);

            arrayTypeSym = Compiler.MainSymbolManager.GetArray(
                elementTypeSym,
                1,
                null);
            arrayInitExpr.TypeSym = arrayTypeSym;

            BindImplicitlyTypedArrayInitConvert(
                elementTypeSym,
                arrayInitExpr.DimSizes,
                0,
                ref arrayInitExpr.ArgumentsExpr,
                ref nonConstCount,
                ref constCount,
                ref hasSideEffects);

            return arrayInitExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.BindImplicitlyTypedArrayInitCore
        //
        /// <summary></summary>
        /// <param name="treeUnOpNode"></param>
        /// <param name="elementTypeSym"></param>
        /// <param name="dimList"></param>
        /// <param name="dimIndex"></param>
        /// <param name="topArgList"></param>
        //------------------------------------------------------------
        internal void BindImplicitlyTypedArrayInitCore(
            UNOPNODE treeUnOpNode,
            ref TYPESYM elementTypeSym,
            List<int> dimList,
            int dimIndex,
            ref EXPR topArgList)
        {
            int count = 0;
            EXPR lastArgList = null;

            BASENODE node = treeUnOpNode.Operand;
            while (node != null)
            {
                BASENODE itemNode;
                if (node.Kind == NODEKIND.LIST)
                {
                    itemNode = node.AsLIST.Operand1.AsBASE;
                    node = node.AsLIST.Operand2;
                }
                else
                {
                    itemNode = node.AsBASE;
                    node = null;
                }
                count++;

                EXPR expr = BindExpr(itemNode, BindFlagsEnum.RValueRequired);
                if (elementTypeSym == null ||
                    elementTypeSym.Kind == SYMKIND.IMPLICITTYPESYM)
                {
                    elementTypeSym = expr.TypeSym;
                }
                else if (CanConvert(elementTypeSym, expr.TypeSym, ConvertTypeEnum.STANDARD))
                {
                    elementTypeSym = expr.TypeSym;
                }
                else if (!CanConvert(expr.TypeSym, elementTypeSym, ConvertTypeEnum.STANDARD))
                {
                    // do nothing here.
                }

                NewList(expr, ref topArgList, ref lastArgList);
                //exprList.Add(expr);
            }

            if (dimList[dimIndex] != -1)
            {
                if (dimList[dimIndex] != count)
                {
                    Compiler.Error(treeUnOpNode, CSCERRID.ERR_InvalidArray);
                }
            }
            else
            {
                dimList[dimIndex] = count;
            }
        }

        //------------------------------------------------------------
        // FUNCBREC.BindImplicitlyTypedArrayInitConvert
        //
        /// <summary></summary>
        /// <param name="elementTypeSym"></param>
        /// <param name="dimList"></param>
        /// <param name="dimIndex"></param>
        /// <param name="argListExpr"></param>
        /// <param name="nonConstCount"></param>
        /// <param name="constCount"></param>
        /// <param name="hasSideEffects"></param>
        //------------------------------------------------------------
        internal void BindImplicitlyTypedArrayInitConvert(
            TYPESYM elementTypeSym,
            List<int> dimList,
            int dimIndex,
            ref EXPR argListExpr,
            ref int nonConstCount,
            ref int constCount,
            ref bool hasSideEffects)
        {
            int count = 0;

            EXPR nodeExpr = argListExpr;
            EXPR topArgList = null, lastArgList = null;

            while (nodeExpr != null)
            {
                EXPR argExpr;
                if (nodeExpr.Kind == EXPRKIND.LIST)
                {
                    EXPRBINOP listExpr = nodeExpr as EXPRBINOP;
                    argExpr = listExpr.Operand1;
                    nodeExpr = listExpr.Operand2;
                }
                else
                {
                    argExpr = nodeExpr;
                    nodeExpr = null;
                }
                count++;

                EXPR expr = null;
#if false
                expr = MustConvert(
                    argExpr,
                    elementTypeSym,
                    0);
#endif
                if (!BindImplicitConversion(
                        argExpr.TreeNode,
                        argExpr,
                        argExpr.TypeSym,
                        elementTypeSym,
                        ref expr,
                        0))
                {
                    Compiler.Error(argExpr.TreeNode, CSCERRID.ERR_NoBestTypeForArray);
                    expr = NewError(treeNode, elementTypeSym);
                }
 
                EXPR constExpr = expr.GetConst();
                if (constExpr != null)
                {
                    if (!constExpr.IsZero(true))
                    {
                        ++constCount;
                    }
                    if (expr.HasSideEffects(Compiler))
                    {
                        hasSideEffects = true;
                    }
                }
                else
                {
                    nonConstCount++;
                }
                NewList(expr, ref topArgList, ref lastArgList);
                //exprList.Add(expr);
            }
            argListExpr = topArgList;
        }
    }
}
