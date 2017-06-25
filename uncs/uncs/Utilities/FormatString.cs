//============================================================================
// PCSFormatString.cs  (uncs\Utilites)
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    internal static class FormatStringUtil
    {
        //------------------------------------------------------------
        // FormatStringUtil.FillupArguments
        //
        /// <summary>
        /// If an array of objects has not enough elements, fill with null elements.
        /// </summary>
        /// <param name="args">An array of objects.</param>
        /// <param name="count">a required number of elements.</param>
        /// <remarks>For the arguments of Format method.</remarks>
        //------------------------------------------------------------
        static internal void FillupArguments(ref Object[] args, int count)
        {
            DebugUtil.Assert(count >= 0);
            if (args != null && args.Length >= count)
            {
                return;
            }

            Object[] args2 = new Object[count];
            int i = 0;
            if (args != null)
            {
                for (; i < args.Length; ++i)
                {
                    args2[i] = args[i];
                }
            }
            for (; i < count; ++i)
            {
                args2[i] = null;
            }
            args = args2;
        }

        //------------------------------------------------------------
        // FormatStringUtil.FormatString (1)
        //
        /// <summary>
        /// Create a string from a format and arguments.
        /// If arguments are not enough, return the format string as is.
        /// </summary>
        /// <param name="args">an array of objects which are embedded in the format.</param>
        /// <param name="buffer">The formatted string will be set.</param>
        /// <param name="format">Format string.</param>
        /// <returns>If failed to format, return false.</returns>
        //------------------------------------------------------------
        static internal bool FormatString(
            out string buffer,
            out Exception excp,
            string format,
            params Object[] args)
        {
            buffer = null;
            excp = null;

            if (format == null)
            {
                return false;
            }
            if (args == null)
            {
                args = new Object[0];
            }

            try
            {
                buffer = String.Format(format, args);
                return true;
            }
            catch (FormatException ex)
            {
                excp = ex;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            buffer = format;
            return false;
        }

        //------------------------------------------------------------
        // FormatStringUtil.FormatString (2)
        //
        /// <summary>
        /// Create a string from a format and arguments.
        /// If arguments are not enough, use null.
        /// </summary>
        /// <param name="args">an array of objects which are embedded in the format.</param>
        /// <param name="buffer">The formatted string will be set.</param>
        /// <param name="count">a required number of args.</param>
        /// <param name="format">Format string.</param>
        /// <returns>If failed to format, return false.</returns>
        //------------------------------------------------------------
        static internal bool FormatString(
            out string buffer,
            out Exception excp,
            string format,
            int count,
            params Object[] args)
        {
            FillupArguments(ref args, count);
            return FormatString(out buffer, out excp, format, args);
        }

        //------------------------------------------------------------
        // FormatStringUtil.GetErrorKindString (1)
        //
        /// <summary>
        /// return a string representing an ERRORKIND value.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetErrorKindString(ERRORKIND kind)
        {
            switch (kind)
            {
                case ERRORKIND.FATAL:
                    return "fatal error";

                case ERRORKIND.ERROR:
                    return "error";

                case ERRORKIND.WARNING:
                    return "warning";

                //case ERRORKIND.NONE:
                default:
                    return null;
            }
        }

        //------------------------------------------------------------
        // FormatStringUtil.FormatErrorMessageCore
        //
        /// <summary>
        /// <para>Unlike FormatErrorMessage, do not use the format for id.
        /// Output argument message itself.</para>
        /// </summary>
        /// <param name="errorNo"></param>
        /// <param name="kind"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="lineEnd"></param>
        /// <param name="colEnd"></param>
        /// <param name="prefix"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string FormatErrorMessageCore(
            string fileName,
            int line,
            int col,
            int lineEnd,
            int colEnd,
            ERRORKIND errorKind,
            string prefix,
            int errorNo,
            string message)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            string errorKindStr = GetErrorKindString(errorKind);
            
            //--------------------------------------------------
            // location (file and line, column
            //--------------------------------------------------
            if (!String.IsNullOrEmpty(fileName))
            {
                sb.Append(fileName);

                if (line > 0 && lineEnd > 0)
                {
                    sb.AppendFormat("({0},{1})-({2},{3})", line, col, lineEnd, colEnd);
                }
                else if (line > 0)
                {
                    sb.AppendFormat("({0},{1})", line, col);
                }
            }

            //--------------------------------------------------
            // kind
            // Print ": error prefix####" -- only if not a related symbol location (id == -1)
            //--------------------------------------------------
            if (!String.IsNullOrEmpty(errorKindStr))
            {
                if (sb.Length > 0)
                {
                    sb.Append(": ");
                }

                if (errorNo >= 0)
                {
                    sb.AppendFormat("{0} {1}{2:d4}", errorKindStr, prefix, errorNo);
                }
                else
                {
                    sb.AppendFormat("{0}", errorKindStr);
                }
            }
            else if (errorNo >= 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(": ");
                }

                sb.AppendFormat("{0}{1:d4}", prefix, errorNo);
            }

            //--------------------------------------------------
            // message
            //--------------------------------------------------
            if (!String.IsNullOrEmpty(message))
            {
                if (sb.Length > 0)
                {
                    sb.Append(": ");
                }

                sb.Append(message);
            }

            return sb.ToString();
        }
    }
}
