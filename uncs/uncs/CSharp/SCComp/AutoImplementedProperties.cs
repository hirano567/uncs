//============================================================================
// AutoImplementedProperties.cs
//
// 2015/12/28 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    //======================================================================
    // class CLSDREC
    //======================================================================
    internal partial class CLSDREC
    {
        //------------------------------------------------------------
        // CLSDREC.IsValidAutoImplementedProperty
        //
        /// <summary></summary>
        /// <param name="propNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsValidAutoImplementedProperty(PROPERTYNODE propNode)
        {
            if (propNode == null ||
                propNode.GetNode == null ||
                propNode.GetNode.OpenCurlyIndex != propNode.GetNode.CloseCurlyIndex)
            {
                return false;
            }
            if (propNode.SetNode == null)
            {
                // this message should be added to the resources.
                //Compiler.Controller.ReportError(
                //    ERRORKIND.ERROR,
                //    "Automatically implemented properties must define both get and set accessors.");
                NAMENODE nNode = propNode.NameNode as NAMENODE;
                Compiler.Error(
                    propNode,
                    CSCERRID.ERR_PropertyAccessorHasNoBody,
                    new ErrArg(nNode != null ? nNode.Name : ""));
                return false;
            }
            if (propNode.SetNode.OpenCurlyIndex == propNode.SetNode.CloseCurlyIndex)
            {
                propNode.GetNode.IsAutoImplemented = true;
                propNode.SetNode.IsAutoImplemented = true;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CLSDREC.IsAutoImplementedAccessor
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsAutoImplementedAccessor(BASENODE node)
        {
            if (node == null ||
                node.Kind != NODEKIND.ACCESSOR)
            {
                return false;
            }
            return (node as ACCESSORNODE).IsAutoImplemented;
        }

        //------------------------------------------------------------
        // CLSDREC.GenerateBackFieldName
        //
        /// <summary></summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string GenerateBackFieldName(string propName)
        {
            return String.Format("<{0}>k__BackField", propName);
        }

        //------------------------------------------------------------
        // CLSDREC.CreateBackField
        //
        /// <summary></summary>
        /// <param name="aggSym"></param>
        /// <param name="propName"></param>
        //------------------------------------------------------------
        private MEMBVARSYM CreateBackField(
            PROPERTYNODE propNode,
            string propName,
            TYPESYM propTypeSym,
            AGGSYM aggSym,
            AGGDECLSYM aggDeclSym)
        {
            DebugUtil.Assert(aggSym.IsClass || aggSym.IsStruct);
            string backFieldName = GenerateBackFieldName(propName);

            NODEFLAGS propFlags = propNode.Flags;

            MEMBVARSYM backFieldSym = Compiler.MainSymbolManager.CreateMembVar(
                backFieldName,
                aggSym,
                aggDeclSym);
            backFieldSym.TypeSym = propTypeSym;
            backFieldSym.ParseTreeNode = null;
            backFieldSym.IsAssigned = true;

            NODEFLAGS allowableFlags =
                aggSym.AllowableMemberAccess() |
                NODEFLAGS.MOD_UNSAFE |
                NODEFLAGS.MOD_NEW |
                NODEFLAGS.MOD_STATIC;

            backFieldSym.IsUnsafe = (aggDeclSym.IsUnsafe || (propFlags & NODEFLAGS.MOD_UNSAFE) != 0);

            if ((propFlags & NODEFLAGS.MOD_ABSTRACT) != 0)
            {
                DebugUtil.Assert((allowableFlags & NODEFLAGS.MOD_ABSTRACT) == 0);
                Compiler.ErrorRef(null, CSCERRID.ERR_AbstractField, new ErrArgRef(backFieldSym));
                //flags &= ~NODEFLAGS.MOD_ABSTRACT;
            }

            backFieldSym.Access = ACCESS.PRIVATE;   // GetAccessFromFlags(aggSym, allowableFlags, flags);

            if ((propFlags & NODEFLAGS.MOD_STATIC) != 0)
            {
                backFieldSym.IsStatic = true;
            }

            CheckForProtectedInSealed(backFieldSym);

            // Check that the field type is as accessible as the field itself.
            CheckConstituentVisibility(backFieldSym, propTypeSym, CSCERRID.ERR_BadVisFieldType);

            return backFieldSym;
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC.CreateAutoImplementedGetAccessor
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR CreateAutoImplementedGetAccessor(METHSYM methodSym)
        {
            CreateNewScope();
            SCOPESYM scopeSym = this.currentScopeSym;
            this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

            this.currentBlockExpr = NewExprBlock(treeNode);
            this.currentBlockExpr.ScopeSym = this.currentScopeSym;

            SymWithType symWithType = new SymWithType();
            AGGTYPESYM parentAts = methodSym.ParentAggSym.GetThisType();
            symWithType.Set(
                methodSym.PropertySym.BackFieldSym,
                parentAts);

            EXPR fieldExpr = BindToField(
                null,
                BindThisImplicit(null),
                FieldWithType.Convert(symWithType),
                BindFlagsEnum.RValueRequired);

            //TYPESYM retTypeSym = methodSym.ReturnTypeSym;

            EXPRRETURN returnExpr = NewExpr(null, EXPRKIND.RETURN, null) as EXPRRETURN;
            returnExpr.ObjectExpr = fieldExpr;

            this.currentBlockExpr.StatementsExpr = returnExpr;
            EXPRBLOCK blockExpr = this.currentBlockExpr;
            this.currentBlockExpr = blockExpr.OwingBlockExpr;

            CloseScope();

            CorrectAnonMethScope(blockExpr.ScopeSym);

            return blockExpr;
        }

        //------------------------------------------------------------
        // FUNCBREC.CreateAutoImplementedSetAccessor
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR CreateAutoImplementedSetAccessor(METHSYM methodSym)
        {
            CreateNewScope();
            SCOPESYM scopeSym = this.currentScopeSym;
            this.currentScopeSym.ScopeFlags = SCOPEFLAGS.NONE;

            this.currentBlockExpr = NewExprBlock(treeNode);
            this.currentBlockExpr.ScopeSym = this.currentScopeSym;

            //--------------------------------------------------------
            // Back filed
            //--------------------------------------------------------
            SymWithType fieldSwt = new SymWithType();
            AGGTYPESYM parentAts = methodSym.ParentAggSym.GetThisType();
            fieldSwt.Set(
                methodSym.PropertySym.BackFieldSym,
                parentAts);

            EXPR fieldExpr = BindToField(
                null,
                BindThisImplicit(null),
                FieldWithType.Convert(fieldSwt),
                BindFlagsEnum.RValueRequired);

            //--------------------------------------------------------
            // Parameter "value"
            //--------------------------------------------------------
            // The SYM instance of parameter "value" is a LOCVARSYM instance,
            // and is created in CLSDREC.FillMethInfoCommon.
            LOCVARSYM locSym = Compiler.LocalSymbolManager.LookupLocalSym(
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.VALUE),
                this.OuterScopeSym,
                SYMBMASK.LOCVARSYM) as LOCVARSYM;
            DebugUtil.Assert(locSym != null);

            SymWithType valueSwt = new SymWithType();
            valueSwt.Set(locSym, null);

            EXPR valueExpr = BindToLocal(null, locSym, BindFlagsEnum.RValueRequired);

            //--------------------------------------------------------
            // BackField = value
            //--------------------------------------------------------
            EXPR assignExpr = NewExprBinop(
                null,
                EXPRKIND.ASSG,
                fieldExpr.TypeSym,
                fieldExpr,
                valueExpr);

            //--------------------------------------------------------
            // Statement
            //--------------------------------------------------------
            EXPRSTMTAS stmt = NewExpr(null, EXPRKIND.STMTAS, null) as EXPRSTMTAS;
            stmt.Expr = assignExpr;

            //--------------------------------------------------------
            // Block
            //--------------------------------------------------------
            this.currentBlockExpr.StatementsExpr = stmt;
            EXPRBLOCK blockExpr = this.currentBlockExpr;
            this.currentBlockExpr = blockExpr.OwingBlockExpr;

            CloseScope();

            CorrectAnonMethScope(blockExpr.ScopeSym);

            return blockExpr;
        }
    }
}
