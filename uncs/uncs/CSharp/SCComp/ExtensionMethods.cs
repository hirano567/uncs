//============================================================================
// ExtensionMethods.cs
//
// 2016/01/20 hirano567@hotmail.co.jp
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
        // CLSDREC.DefineExtensionMethod
        //
        /// <summary></summary>
        /// <param name="methodNode"></param>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void DefineExtensionMethod(METHODNODE methodNode, METHSYM methodSym)
        {
            DebugUtil.Assert(
                methodNode != null && methodNode.IsExtensionMethod &&
                methodSym != null);

            //--------------------------------------------------------
            //
            //--------------------------------------------------------
            AGGSYM parentAggSym = methodSym.ParentAggSym;
            DebugUtil.Assert(parentAggSym != null);

            if (!methodSym.IsStatic)
            {
                Compiler.Error(
                    methodNode,
                    CSCERRID.ERR_NonStaticExtensionMethod);
                return;
            }
            if (!parentAggSym.IsStatic ||
                (parentAggSym.AllTypeVariables != null &&
                parentAggSym.AllTypeVariables.Count > 0))
            {
                Compiler.Error(
                    methodNode,
                    CSCERRID.ERR_ExtensionMethodInImproperClass);
                return;
            }
            if (parentAggSym.IsNested)
            {
                Compiler.Error(
                    methodNode,
                    CSCERRID.ERR_ExtensionMethodInNestedClass,
                    new ErrArg(parentAggSym.Name));
                return;
            }

            DefineExtensionMethodCore(methodSym);
        }

        //------------------------------------------------------------
        // CLSDREC.DefineExtensionMethodCore
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void DefineExtensionMethodCore(METHSYM methodSym)
        {
            DebugUtil.Assert(methodSym != null);

            DebugUtil.Assert(methodSym.ParameterTypes.Count > 0);
            TypeArray paramTypes = methodSym.ParameterTypes;
            AGGTYPESYM ats = paramTypes[0] as AGGTYPESYM;
            if (ats == null)
            {
                return;
            }
            AGGSYM targetAggSym = ats.GetAggregate();

            METHSYM instanceMethSym = Compiler.MainSymbolManager.CreateGlobalSym(
                SYMKIND.METHSYM,
                methodSym.Name,
                targetAggSym) as METHSYM;

            instanceMethSym.ContainingAggDeclSym = targetAggSym.FirstDeclSym;
            instanceMethSym.IsUnsafe = methodSym.IsUnsafe;

            instanceMethSym.TypeVariables = methodSym.TypeVariables;

            TypeArray instParamTypes = new TypeArray();
            for (int i = 1; i < paramTypes.Count; ++i)
            {
                instParamTypes.Add(paramTypes[i]);
            }
            instanceMethSym.ParameterTypes
                = Compiler.MainSymbolManager.AllocParams(instParamTypes);
            instanceMethSym.ReturnTypeSym = methodSym.ReturnTypeSym;
            instanceMethSym.Access = methodSym.Access;
            instanceMethSym.IsStatic = false;

            instanceMethSym.IsParameterArray = methodSym.IsParameterArray;
            instanceMethSym.IsVarargs = methodSym.IsVarargs;

            instanceMethSym.StaticExtensionMethodSym = methodSym;
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
        //------------------------------------------------------------
        // FUNCBREC.BindExtensionMethod
        //
        /// <summary></summary>
        /// <param name="callExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EXPR BindExtensionMethod(EXPRCALL instanceCallExpr)
        {
            DebugUtil.Assert(
                instanceCallExpr != null &&
                instanceCallExpr.MethodWithInst != null &&
                instanceCallExpr.MethodWithInst.MethSym != null);

            MethWithInst instanceMwi = instanceCallExpr.MethodWithInst;
            METHSYM instanceMethSym = instanceMwi.MethSym;
            METHSYM staticMethSym = instanceMethSym.StaticExtensionMethodSym;
            if (staticMethSym == null)
            {
                return instanceCallExpr;
            }

            MethWithInst staticMwi = new MethWithInst(
                staticMethSym,
                staticMethSym.ParentAggSym.GetThisType(),   // non-generic type.
                instanceMwi.TypeArguments);

            EXPR topArgExpr = instanceCallExpr.ObjectExpr;
            EXPR lastArgExpr = topArgExpr;
            if (instanceCallExpr.ArgumentsExpr != null)
            {
                NewList(instanceCallExpr.ArgumentsExpr, ref topArgExpr, ref lastArgExpr);
            }

            EXPRCALL staticCallExpr = NewExpr(
                treeNode,
                EXPRKIND.CALL,
                instanceCallExpr.TypeSym) as EXPRCALL;

            staticCallExpr.MethodWithInst = staticMwi;
            staticCallExpr.ArgumentsExpr = topArgExpr;
            staticCallExpr.ObjectExpr = null;

            return staticCallExpr;
        }
    }

    //======================================================================
    // class MemberLookup
    //======================================================================
    internal partial class MemberLookup
    {
        //------------------------------------------------------------
        // MemberLookup.LookupExtensionMethodInInterfaces
        //
        /// <summary></summary>
        /// <param name="startAggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool LookupExtensionMethodInInterfaces(AGGTYPESYM startAggTypeSym)
        {
            //DebugUtil.Assert(this.firstSymWithType == null || this.isMulti);
            //DebugUtil.Assert(startAggTypeSym != null);
            if (startAggTypeSym == null)
            {
                return false;
            }
            if (this.firstSymWithType != null && this.firstSymWithType.IsNotNull)
            {
                return false;
            }

            //DebugUtil.Assert(startAggTypeSym == null || startAggTypeSym.IsInterfaceType());
            //DebugUtil.Assert(startAggTypeSym != null || interfaceTypeArray.Count > 0);
            //DebugUtil.Assert((this.flags &
            //    (MemLookFlagsEnum.Ctor | MemLookFlagsEnum.Operator | MemLookFlagsEnum.BaseCall))
            //    == 0);

            TypeArray interfaceTypeArray = startAggTypeSym.GetIfacesAll();
            if (interfaceTypeArray == null || interfaceTypeArray.Count == 0)
            {
                return false;
            }

            // Clear all the hidden flags. Anything found in a class hides any other
            // kind of member in all the interfaces.
            if (startAggTypeSym != null)
            {
                startAggTypeSym.AllHidden = false;
                startAggTypeSym.DiffHidden = (this.firstSymWithType != null);
            }

            for (int i = 0; i < interfaceTypeArray.Count; ++i)
            {
                AGGTYPESYM type = interfaceTypeArray[i] as AGGTYPESYM;
                DebugUtil.Assert(type.IsInterfaceType());

                type.AllHidden = false;
                type.DiffHidden = (this.firstSymWithType != null);
            }

            if (startAggTypeSym != null)
            {
                Compiler.EnsureState(startAggTypeSym, AggStateEnum.Prepared);
            }
            if (interfaceTypeArray != null)
            {
                Compiler.EnsureState(interfaceTypeArray, AggStateEnum.Prepared);
            }

            //--------------------------------------------------------
            // Loop through the interfaces.
            //--------------------------------------------------------
            bool hideObject = false;
            int index = 0;
            AGGTYPESYM currentSym = interfaceTypeArray[index++] as AGGTYPESYM;
            DebugUtil.Assert(currentSym != null);

            for (; ; )
            {
                DebugUtil.Assert(currentSym != null && currentSym.IsInterfaceType());
                bool hideByName = false;

                if (!currentSym.AllHidden && SearchSingleType(currentSym, out hideByName))
                {
                    SYM fsym = this.firstSymWithType.Sym;
                    DebugUtil.Assert(fsym != null);

                    if (fsym.Kind == SYMKIND.METHSYM&&
                        (fsym as METHSYM).IsInstanceExtensionMethod)
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
                }
                this.flags &= ~MemLookFlagsEnum.TypeVarsAllowed;

                if (index >= interfaceTypeArray.Count)
                {
                    return !hideObject;
                }

                // Substitution has already been done.
                currentSym = interfaceTypeArray[index++] as AGGTYPESYM;
            }
        }
    }
}
