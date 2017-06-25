//============================================================================
// FncBindExt.cs
//
// 2016/08/29 (hirano567@hotmail.co.jp)
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
        private NSSYM systemNsSym = null;
        private NSSYM reflectionNsSym = null;
        private AGGSYM systemTypeAggSym = null;
        private AGGTYPESYM systemTypeAggTypeSym = null;
        private ARRAYSYM systemTypeArraySym = null;

        //------------------------------------------------------------
        // FUNCBREC.SetFncBrecExtFields
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool SetFncBrecExtFields()
        {
            if (systemNsSym == null)
            {
                systemNsSym = Compiler.LookupInBagAid(
                    "System",
                    Compiler.MainSymbolManager.RootNamespaceSym,
                    0,
                    0,
                    SYMBMASK.NSSYM) as NSSYM;
            }
            if (systemNsSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("System"));
                return false;
            }

            if (reflectionNsSym == null)
            {
                reflectionNsSym = Compiler.LookupInBagAid(
                    "Reflection",
                    systemNsSym,
                    0,
                    0,
                    SYMBMASK.NSSYM) as NSSYM;
            }
            if (reflectionNsSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("Reflection"));
                return false;
            }

            if (systemTypeAggTypeSym == null)
            {
                systemTypeAggSym = Compiler.LookupInBagAid(
                    "Type",
                    systemNsSym,
                    0,
                    0,
                    SYMBMASK.AGGSYM) as AGGSYM;
                systemTypeAggTypeSym = systemTypeAggSym.GetThisType();
            }
            if (systemTypeAggTypeSym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("Type"));
                return false;
            }
            if (systemTypeArraySym == null)
            {
                systemTypeArraySym = Compiler.MainSymbolManager.GetArray(
                    systemTypeAggTypeSym,
                    1,
                    systemTypeAggTypeSym.Type.MakeArrayType());
            }
            if (systemTypeArraySym == null)
            {
                Compiler.Error(CSCERRID.ERR_SingleTypeNameNotFound, new ErrArg("Type[]"));
                return false;
            }

            return true;
        }
    }
}
