//============================================================================
// PartialMethods.cs
//
// 2016/06/17 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // class CLSDREC
    //======================================================================
    internal partial class CLSDREC
    {
        //------------------------------------------------------------
        // CLSDREC.IsPartialMethod
        //
        /// <summary></summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsPartialMethod(BASENODE treeNode)
        {
            DebugUtil.Assert(treeNode != null);

            if (treeNode.Kind != NODEKIND.METHOD)
            {
                return false;
            }

            return (treeNode.Flags & NODEFLAGS.MOD_PARTIAL) != 0;
        }

        //------------------------------------------------------------
        // CLSDREC.SetForPartialMethod
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="methodSym"></param>
        //------------------------------------------------------------
        internal void SetForPartialMethod(BASENODE treeNode, METHSYM methodSym)
        {
            if (!IsPartialMethod(treeNode))
            {
                return;
            }

            methodSym.IsPartialMethod = true;
            methodSym.HasNoBody = ((treeNode.NodeFlagsEx & NODEFLAGS.EX_METHOD_NOBODY) != 0);
        }

        //------------------------------------------------------------
        // CLSDREC.SetForPartialMethod
        //
        /// <summary></summary>
        /// <param name="treeNode"></param>
        /// <param name="partialMethSym">the partial method</param>
        /// <param name="otherMethSym">the other method with the same signiture</param>
        /// <param name="errorID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckForDulicatePartialMethod(
            BASENODE treeNode,
            METHSYM partialMethSym,
            METHSYM otherMethSym)
        {
            if (partialMethSym == null || otherMethSym == null)
            {
                return true;
            }

            if (!otherMethSym.IsPartialMethod)
            {
                Compiler.Error(
                    treeNode,
                    CSCERRID.ERR_MemberAlreadyExists,
                    new ErrArg(partialMethSym),
                    new ErrArg(partialMethSym.ClassSym));
                return false;
            }

            if (!partialMethSym.HasNoBody && !otherMethSym.HasNoBody)
            {
                Compiler.Error(
                    treeNode,
                    CSCERRID.ERR_MultiplePartialMethodImplementation);
                return false;
            }

            return true;
        }

        //------------------------------------------------------------
        // CLSDREC.CheckFlagsAndSigOfPartialMethod
        //
        /// <summary></summary>
        /// <param name="nodeFlags"></param>
        /// <param name="methodSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal void CheckFlagsAndSigOfPartialMethod(
            BASENODE treeNode,
            NODEFLAGS nodeFlags,
            METHSYM methodSym)
        {
            const NODEFLAGS forbidden = 0
                | NODEFLAGS.MOD_ABSTRACT
                | NODEFLAGS.MOD_NEW
                | NODEFLAGS.MOD_OVERRIDE
                | NODEFLAGS.MOD_PRIVATE
                | NODEFLAGS.MOD_PROTECTED
                | NODEFLAGS.MOD_INTERNAL
                | NODEFLAGS.MOD_PUBLIC
                | NODEFLAGS.MOD_SEALED
                | NODEFLAGS.MOD_VIRTUAL
                | NODEFLAGS.MOD_EXTERN;

            if ((nodeFlags & forbidden) != 0)
            {
                Compiler.Error(treeNode, CSCERRID.ERR_BadModifierForPartialMethod);
            }

            TypeArray paramTypes = methodSym.ParameterTypes;
            if (paramTypes != null && paramTypes.Count > 0)
            {
                for (int i = 0; i < paramTypes.Count; ++i)
                {
                    TYPESYM typeSym = paramTypes[i];
                    if (typeSym.Kind == SYMKIND.PARAMMODSYM&&
                        (typeSym as PARAMMODSYM).IsOut)
                    {
                        Compiler.Error(
                            treeNode,
                            CSCERRID.ERR_PartialMethodHasOutParameter);
                    }
                }
            }
        }
    }

    //======================================================================
    // class FUNCBREC
    //======================================================================
    internal partial class FUNCBREC
    {
    }
}
