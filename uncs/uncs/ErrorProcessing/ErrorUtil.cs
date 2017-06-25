
//============================================================================
// ErrorUtil.cs
//
// 2015/10/18 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // class ErrorUtil (static)
    //======================================================================
    internal static class ErrorUtil
    {
        //------------------------------------------------------------
        // ErrorUtil.GetErrorKind (1)
        //
        /// <summary>
        /// Get an ERROORKIND value from a warning level.
        /// </summary>
        /// <param name="eid"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static ERRORKIND GetErrorKind(int warningLevel)
        {
            if (warningLevel == 0)
            {
                return ERRORKIND.ERROR;
            }
            else if (warningLevel > 0)
            {
                return ERRORKIND.WARNING;
            }
            else if (warningLevel == -1)
            {
                return ERRORKIND.FATAL;
            }
            return ERRORKIND.NONE;
        }

        //------------------------------------------------------------
        // ErrorUtil.IsValidErrorKind
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsValidErrorKind(ERRORKIND kind)
        {
            switch (kind)
            {
                case ERRORKIND.FATAL:
                case ERRORKIND.ERROR:
                case ERRORKIND.WARNING:
                    return true;

                default:
                    return false;
            }
        }

        //------------------------------------------------------------
        // ErrorUtil.FormatErrorMessage (1)
        //
        /// <summary>
        /// <para>Get the format string by id, create an error message with args</para>
        /// </summary>
        /// <param name="formatted"></param>
        /// <param name="excp"></param>
        /// <param name="strID"></param>
        /// <param name="args"></param>
        /// <returns>Return false if failed to make an error message.</returns>
        //------------------------------------------------------------
        static internal bool FormatErrorMessage(
            out string formatted,
            out Exception excp,
            ResNo resNo,
            params Object[] args)
        {
            formatted = null;
            int argCount = 0;
            string format = null;
            excp = null;

            if (CResources.GetString(resNo, out format, out argCount, out excp))
            {
                return FormatStringUtil.FormatString(
                    out formatted,
                    out excp,
                    format,
                    argCount,
                    args);
            }
            return false;
        }

        //------------------------------------------------------------
        // CSResources.FormatErrorMessage (2)
        //
        /// <summary>
        /// <para>Get the format string by id, create an error message with args and locations</para>
        /// </summary>
        /// <param name="excp"></param>
        /// <param name="errorNo"></param>
        /// <param name="strID"></param>
        /// <param name="errorKind"></param>
        /// <param name="fileName"></param>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="lineEnd"></param>
        /// <param name="colEnd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string FormatErrorMessage(
            out Exception excp,
            ERRORKIND errorKind,
            string prefix,
            int errorNo,
            ResNo resNo,
            string fileName,
            int line,
            int col,
            int lineEnd,
            int colEnd,
            params Object[] args)
        {
            string message = null;

            if (!ErrorUtil.FormatErrorMessage(out message, out excp, resNo, args) &&
                String.IsNullOrEmpty(message))
            {
                // If failed to format, concatenate args to create a message.
                if (args.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < args.Length; ++i)
                    {
                        string str = args[i].ToString();
                        if (!String.IsNullOrEmpty(str))
                        {
                            if (sb.Length > 0) sb.Append(", ");
                            sb.Append(str);
                        }
                    }
                    if (sb.Length > 0)
                    {
                        message = sb.ToString();
                    }
                }
                if (String.IsNullOrEmpty(message))
                {
                    message = "Unknown error.";
                }
            }

            return FormatStringUtil.FormatErrorMessageCore(
                fileName,
                line,
                col,
                lineEnd,
                colEnd,
                errorKind,
                prefix,
                errorNo,
                message);
        }
    }
}
