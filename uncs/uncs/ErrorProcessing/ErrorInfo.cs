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
// File: scc.cpp
//
// The command line driver for the C# compiler.
// ===========================================================================

//============================================================================
// ErrorInfo.cs
//
// Definition of CSCErrorInfo.
// In sscli, this definition is in CSharp\SCC\SCC.cpp.
//
// 2015/10/18 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // enum ERRORKIND   (ERROR_)
    //
    /// <summary>
    /// <para>(uncs\ErrorProcessing\ErrorInfo.cs.
    /// In sscli, with prefix ERROR_ defined in prebuilt\idl\csiface.h)</para>
    /// </summary>
    //======================================================================
    internal enum ERRORKIND : int
    {
        NONE = -1,  // add

        FATAL = 0,
        ERROR = FATAL + 1,
        WARNING = ERROR + 1
    }

    //======================================================================
    // enum ERRORCATEGORY   (EC_)
    //
    /// <summary>
    /// <para>TOKENIZATION, TOPLEVELPARSE, METHODPARSE, COMPILATION</para>
    /// <para>(uncs\ErrorProcessing\ErrorInfo.cs.
    /// In sscli, with prefix EC_ defined in prebuilt\idl\csiface.h)</para>
    /// </summary>
    //======================================================================
    internal enum ERRORCATEGORY : int
    {
        TOKENIZATION = 0,
        TOPLEVELPARSE = TOKENIZATION + 1,
        METHODPARSE = TOPLEVELPARSE + 1,
        COMPILATION = METHODPARSE + 1
    }

    //======================================================================
    // class BCErrorInfoManager (abstract)
    //
    /// <summary></summary>
    //======================================================================
    internal class BCErrorInfoManager<ERRID>
    {
        //======================================================================
        // class BCErrorInfoManager.ERRORINFO
        //
        // NOTE:  Warnings, errors, and fatal errors are determined by the
        //        various macros.  ERRORDEF = ERROR_ERROR, FATALDEF = ERROR_FATAL
        //        WARNDEF and OLDWARN = ERROR_WARNING
        //        warning level value as follows:
        //
        //          0   = ERROR_ERROR
        //          >0  = ERROR_WARNING
        //          -1  = ERROR_FATAL
        //
        /// <summary>
        /// <para>ERROR_INFO in sscli.</para>
        /// <para>Map error numbers to their levels and string resources.</para>
        /// <para>(CSharp\SCC\CSCErrorInfo.cs)</para>
        /// </summary>
        //======================================================================
        internal class ERRORINFO
        {
            /// <summary>
            /// Error ID (for internal processing)
            /// </summary>
            internal ERRID ErrorID;

            /// <summary>
            /// Error number (for display)
            /// </summary>
            internal int ErrorNumber;

            /// <summary>
            /// warning level; 0 means error
            /// </summary>
            internal int WarningLevel;

            /// <summary>
            /// resource id.
            /// </summary>
            internal ResNo ResourceNumber;

            //------------------------------------------------------------
            // BCErrorInfoManager.ERRORINFO Constructor
            //
            /// <summary></summary>
            /// <param name="errID"></param>
            /// <param name="errNo"></param>
            /// <param name="warnLevel"></param>
            /// <param name="resID"></param>
            /// <param name="argCount"></param>
            //------------------------------------------------------------
            internal ERRORINFO(
                ERRID errID,
                int errNo,
                int warnLevel,
                ResNo resNo)
            {
                this.ErrorID = errID;
                this.ErrorNumber = errNo;
                this.WarningLevel = warnLevel;
                this.ResourceNumber = resNo;
            }
        }

        //------------------------------------------------------------
        // BCErrorInfoManager Fields and Properties
        //------------------------------------------------------------
        internal Dictionary<ERRID, ERRORINFO> ErrorInfoDic
            = new Dictionary<ERRID, ERRORINFO>();

        internal Dictionary<int, ERRID> ErrorNumberDic
            = new Dictionary<int, ERRID>();

        // These fields are set in the derived classes.
        internal string Prefix = null;
        internal ERRID InvalidErrorID = default(ERRID);

        //------------------------------------------------------------
        // BCErrorInfoManager Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal BCErrorInfoManager()
        {
            InitErrorInfoDic();
            InitErrorNumberDic();
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.GetInfo (1)
        //
        /// <summary>
        /// Return the ERRORINFO instance for a given CSCERRID.
        /// </summary>
        /// <param name="errID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ERRORINFO GetInfo(ERRID errID)
        {
            ERRORINFO info;
            try
            {
                if (ErrorInfoDic.TryGetValue(errID, out info))
                {
                    return info;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return null;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.GetErrorInfo (2)
        //
        /// <summary></summary>
        /// <param name="errID"></param>
        /// <param name="errNo"></param>
        /// <param name="warnLevel"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetInfo(ERRID errID, out int errNo, out int warnLevel)
        {
            ERRORINFO info = GetInfo(errID);
            if (info != null)
            {
                errNo = info.ErrorNumber;
                warnLevel = info.WarningLevel;
                return true;
            }
            errNo = -1;
            warnLevel = 4;
            return false;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.GetResourceNumber (1)
        //
        /// <summary>
        /// <para>Return the resource ID for a given CSCERRID.</para>
        /// </summary>
        /// <param name="errID"></param>
        /// <param name="resNo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetResourceNumber(
            ERRID errID,
            out ResNo resNo)
        {
            ERRORINFO info = GetInfo(errID);

            if (info != null)
            {
                resNo = info.ResourceNumber;
                return true;
            }

            resNo = ResNo.Invalid;
            return false;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.GetResourceNumber (2)
        //
        /// <summary>
        /// <para>Return CSCSTRID for a given CSCERRID.</para>
        /// <para>If not found, return 0 (no string is assigned to 0).</para>
        /// </summary>
        /// <param name="errID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ResNo GetResourceNumber(ERRID errID)
        {
            ERRORINFO info = GetInfo(errID);
            return (info != null ? info.ResourceNumber : ResNo.Invalid);
        }

        // NOTE:  Warnings, errors, and fatal errors are determined by the
        //        various macros.  ERRORDEF = ERROR_ERROR, FATALDEF = ERROR_FATAL
        //        WARNDEF and OLDWARN = ERROR_WARNING
        //        warning level value as follows:
        //
        //          0   = ERROR_ERROR
        //          >0  = ERROR_WARNING
        //          -1  = ERROR_FATAL

        //------------------------------------------------------------
        // BCErrorInfoManager.GetWarningLevel
        //
        /// <summary>
        /// Return CSCSTRID for a given CSCERRID.
        /// </summary>
        /// <param name="errID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetWarningLevel(ERRID errID, out int level)
        {
            level = -2;
            ERRORINFO info = GetInfo(errID);
            if (info != null)
            {
                level = info.WarningLevel;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.GetErrorKind
        //
        /// <summary>
        /// Get an ERROORKIND value from a CSCERRID value.
        /// </summary>
        /// <param name="errID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ERRORKIND GetErrorKind(ERRID errID)
        {
            int level;

            if (GetWarningLevel(errID, out level))
            {
                return ErrorUtil.GetErrorKind(level);
            }
            return ERRORKIND.NONE;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.GetErrorNumber
        //
        /// <summary></summary>
        /// <param name="errID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetErrorNumber(ERRID errID)
        {
            ERRORINFO info = GetInfo(errID);
            if (info != null)
            {
                return info.ErrorNumber;
            }
            return -1;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.GetIDfromNumber
        //
        /// <summary></summary>
        /// <param name="num"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ERRID GetIDfromNumber(int num)
        {
            ERRID id;
            if (ErrorNumberDic.TryGetValue(num, out id))
            {
                return id;
            }
            return InvalidErrorID;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.IsValidWarningNumber
        //
        /// <summary>
        /// determine if a number is a valid warning number.
        /// </summary>
        /// <param name="errID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsValidWarningNumber(ERRID errID)
        {
            int level;

            if (GetWarningLevel(errID, out level))
            {
                return (level > 0);
            }
            return false;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.FormatErrorMessage (1)
        //
        /// <summary>
        /// <para>Get the format string by id,
        /// create an error message with args</para>
        /// </summary>
        /// <param name="formatted"></param>
        /// <param name="excp"></param>
        /// <param name="errID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FormatErrorMessage(
           out string formatted,
           out Exception excp,
           ERRID errID,
           params Object[] args)
        {
            ResNo resNo;

            formatted = null;
            excp = null;

            if (GetResourceNumber(errID, out resNo))
            {
                return ErrorUtil.FormatErrorMessage(
                    out formatted,
                    out excp,
                    resNo,
                    args);
            }
            return false;
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.FormatErrorMessage (2)
        //
        /// <summary>
        /// <para>Get the format string by id,
        /// create an error message with args and locations</para>
        /// </summary>
        /// <param name="excp"></param>
        /// <param name="errID"></param>
        /// <param name="errKind"></param>
        /// <param name="fileName"></param>
        /// <param name="lineStart"></param>
        /// <param name="colStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="colEnd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string FormatErrorMessage(
           out Exception excp,
           ERRID errID,
           ERRORKIND errKind,
           string fileName,
           int lineStart,
           int colStart,
           int lineEnd,
           int colEnd,
           params Object[] args)
        {
            int errorNo = -1;
            ResNo resNo = ResNo.Invalid;

            ERRORINFO eInfo = GetInfo(errID);
            if (eInfo != null)
            {
                errorNo = eInfo.ErrorNumber;
                resNo = eInfo.ResourceNumber;
                if (!ErrorUtil.IsValidErrorKind(errKind))
                {
                    errKind = ErrorUtil.GetErrorKind(eInfo.WarningLevel);
                }
            }
            else
            {
                resNo = ResNo.CSCSTR_InternalError;
                args = new Object[] { "invalid error ID in CSResources.FormatErrorMessage" };
            }

            return ErrorUtil.FormatErrorMessage(
                out excp,
                errKind,
                this.Prefix,
                errorNo,
                resNo,
                fileName,
                lineStart,
                colStart,
                lineEnd,
                colEnd,
                args);
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.FormatErrorMessage (3)
        //
        /// <summary></summary>
        /// <param name="excp"></param>
        /// <param name="errID"></param>
        /// <param name="errKind"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string FormatErrorMessage(
           out Exception excp,
           ERRORKIND errKind,
           ERRID errID,
           params Object[] args)
        {
            return FormatErrorMessage(
                out excp,
                errID,
                errKind,
                null,
                -1,
                -1,
                -1,
                -1,
                args);
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.InitErrorInfoDic (virtual)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        virtual internal void InitErrorInfoDic()
        {
            throw new NotImplementedException("BCErrorInfoManager.initErrorInfoDic must be overridden.");
        }

        //------------------------------------------------------------
        // BCErrorInfoManager.InitErrorNumberDic (virtual)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        virtual internal void InitErrorNumberDic()
        {
            foreach (KeyValuePair<ERRID, ERRORINFO> kv in ErrorInfoDic)
            {
                ErrorNumberDic.Add(kv.Value.ErrorNumber, kv.Value.ErrorID);
            }
        }
    }
}

