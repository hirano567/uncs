//============================================================================
// Utility  ExprUtil.cs
//
// 2015/04/12
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Uncs
{
    //======================================================================
    // class ExprUtil (static)
    //======================================================================
    static internal partial class ExprUtil
    {
        //--------------------------------------------------
        // ExprUtil.CountArguments
        //
        /// <summary>
        /// (Utilities\ExprUtil.cs)
        /// </summary>
        /// <param name="argsExpr"></param>
        /// <returns></returns>
        //--------------------------------------------------
        static internal int CountArguments(EXPR argsExpr)
        {
            int argCount = 0;
            for (EXPR list = argsExpr; list != null; )
            {
                ++argCount;
                if (list.Kind == EXPRKIND.LIST)
                {
                    list = list.AsBIN.Operand2;
                }
                else
                {
                    list = null;
                }
            }
            return argCount;
        }
    }
}
