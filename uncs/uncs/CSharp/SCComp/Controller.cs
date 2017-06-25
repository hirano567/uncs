// sscli20_20060311

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
// File: controller.h
//
// ===========================================================================

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
// File: controller.cpp
//
// ===========================================================================

//============================================================================
// Controller.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Uncs
{
    //======================================================================
    // CController
    //
    /// <summary>
    /// <para>This is the "controller" for compiler objects.
    /// The controller is the object that exposes/implements ICSCompiler for external consumption.
    /// Compiler options are configured through this object,
    /// and for an actual compilation, this object instanciates a COMPILER,
    /// feeds it the appropriate information, tells it to compile, and then destroys it.</para>
    /// </summary>
    /// <remarks>
    /// <para>Has the instances of
    ///     COptionData
    ///     List&lt;CInputSet&gt;
    ///</para>
    ///<para>Has the references of
    ///     CNameManager
    ///     ICSCompilerHost
    /// </para>
    /// </remarks>
    //======================================================================
    internal class CController
    {
        //------------------------------------------------------------
        // CController Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// <para>NOTE:  Referenced pointer!</para>
        /// <para>Set in CFactory.CreateController method.</para>
        /// </summary>
        private CNameManager nameManager = null;    // NAMEMGR *m_pNameMgr;

        /// <summary>
        /// <para>(R) NOTE:  Referenced pointer!</para>
        /// </summary>
        internal CNameManager NameManager
        {
            get { return nameManager; } // NAMEMGR *GetNameMgr()
        }

        private System.Text.Encoding encoding = System.Text.Encoding.Default;

        /// <summary>
        /// Use ConsoleOutput in place of CompilerHost in sscli.
        /// </summary>
        private ConsoleOutput consoleOutput = null;

        /// <summary>
        /// (R) Use ConsoleOutput in place of CompilerHost in sscli.
        /// </summary>
        internal ConsoleOutput ConsoleOutput
        {
            get { return consoleOutput; }
        }

        /// <summary>
        /// <para>Protect the options</para>
        /// </summary>
        private Object lockOptions = new Object();  // CTinyLock m_lockOptions;

        /// <summary>
        /// In place of optionData.
        /// </summary>
        internal COptionManager OptionManager = null;

        /// <summary>
        /// <para>List of input sets</para>
        /// </summary>
        private List<CInputSet> inputSetList = new List<CInputSet>();

        /// <summary>
        /// <para>(R)List of input sets</para>
        /// </summary>
        internal List<CInputSet> InputSetList
        {
            get { return this.inputSetList; }
        }

        /// <summary>
        /// (R) Count of InputSet.
        /// </summary>
        internal int InputSetCount
        {
            get { return inputSetList.Count; }
        }

        // Next slot in input set list  // Index
        //private int nextInputSetIndex = 0;    

        /// <summary>
        /// Container for reporting errors
        /// </summary>
        private CErrorContainer compilerErrors = null;  // *m_pCompilerErrors

        /// <summary>
        /// <para>Creation flags</para> 
        /// </summary>
        private CompilerCreationFlags creationFlags = 0;    // DWORD m_dwFlags;

        /// <summary>
        /// <para>Address of exception</para> 
        /// </summary>
        private uint exceptionAddress = 0;  // void *m_pExceptionAddr;

        /// <summary>
        /// (R) Address of exception
        /// </summary>
        internal uint ExcptionAddress
        {
            get { return exceptionAddress; }
        }

        /// <summary>
        /// <para>Number of non-warning errors reported to host</para> 
        /// </summary>
        private int countOfReportedErrors = 0;  // long m_iErrorsReported;

        /// <summary>
        /// (R) Number of non-warning errors reported to host
        /// </summary>
        internal int CountOfReportedErrors
        {
            get { return countOfReportedErrors; }   // long ErrorsReported ()
        }

        /// <summary></summary>
        internal bool HadError
        {
            get { return (countOfReportedErrors > 0); }
        }

        /// <summary>
        /// <para>Number of warnings that have been reported as errors due to /warnaserror+ </para>
        /// </summary>
        private int countOfWarnsReportedAsErrors = 0;   // long m_iWarnAsErrorsReported;

        /// <summary>
        /// (R) Number of warnings that have been reported as errors due to /warnaserror+
        /// </summary>
        internal int CountOfWarnsReportedAsErrors
        {
            get { return countOfWarnsReportedAsErrors; }    // long WarnAsErrorsReported()
        }

        private bool hadFatalError = false;

        internal bool HadFatalError
        {
            get { return this.hadFatalError; }
        }

        /// <summary>
        /// <para>If true, show only FATAL errors.</para>
        /// <para>The errors of ERRORKIND.ERROR still increment countOfReportedErrors if true.</para>
        /// </summary>
        /// <remarks>
        /// internal void SuppressErrors(bool fSuppress) { suppressErrors = fSuppress; }
        /// internal bool FErrorsSuppressed() { return suppressErrors; }
        /// </remarks>
        internal bool SuppressErrors = false;   // m_fSuppressErrors

        //------------------------------------------------------------
        // CController.CreateInstance (static)
        //
        /// <summary></summary>
        /// <param name="flags"></param>
        /// <param name="host"></param>
        /// <param name="nameManager"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static CController CreateInstance(
            CompilerCreationFlags flags,
            ConsoleOutput cout,
            CNameManager nameManager)
        {
            // Create a name table if we weren't given one
            if (nameManager == null)
            {
                nameManager = CNameManager.CreateInstance();
            }

            // Create a compiler controller object.  It's the one that exposes ICSCompiler,
            // and manages objects whose lifespan extend beyond a single compilation.

            CController cntr = new CController();
            cntr.Initialize(flags, cout, nameManager);
            return cntr;
        }

        //------------------------------------------------------------
        // CController  Constructor
        //
        /// <summary>does nothing.</summary>
        //------------------------------------------------------------
        internal CController()
        {
            this.OptionManager = new COptionManager(this);
        }

        //------------------------------------------------------------
        // CController.initialize
        //
        /// <summary>
        /// <para>Set creationFlags, compilerHost, NameManager.</para>
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="cons"></param>
        /// <param name="manager"></param>
        //------------------------------------------------------------
        internal void Initialize(
            CompilerCreationFlags flags,
            ConsoleOutput cout,
            CNameManager manager)
        {
            DebugUtil.Assert(manager != null);

            this.consoleOutput = cout;
            this.creationFlags = flags;
            this.nameManager = manager;
        }

        //------------------------------------------------------------
        // ConsoleOutput.GetCLRVersion
        //
        /// <summary>
        /// <para>Return version of the Common Language Runtime.</para>
        /// <para>(In sscli, csharp\inc\consoleoutput.h)</para>
        /// </summary>
        //------------------------------------------------------------
        protected string GetCLRVersion()
        {
            return (System.Environment.Version).ToString();
        }

        //------------------------------------------------------------
        // CController.Write (1)
        //
        /// <summary></summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Write(string str)
        {
            if (this.consoleOutput != null)
            {
                this.consoleOutput.Write(str);
            }
        }

        //------------------------------------------------------------
        // CController.Write (2)
        //
        /// <summary></summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Write(string fmt, params Object[] args)
        {
            if (this.consoleOutput != null)
            {
                this.consoleOutput.Write(fmt, args);
            }
        }

        //------------------------------------------------------------
        // CController.WriteLine (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void WriteLine()
        {
            if (this.consoleOutput != null)
            {
                this.consoleOutput.WriteLine();
            }
        }

        //------------------------------------------------------------
        // CController.WriteLine (2)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void WriteLine(string str)
        {
            if (this.consoleOutput != null)
            {
                this.consoleOutput.WriteLine(str);
            }
        }

        //------------------------------------------------------------
        // CController.WriteLine (3)
        //
        /// <summary></summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void WriteLine(string fmt, params Object[] args)
        {
            if (this.consoleOutput != null)
            {
                this.consoleOutput.WriteLine(fmt, args);
            }
        }

        //------------------------------------------------------------
        // CController.RemoveSetFromList
        //
        /// <summary>
        /// Not defined in sscli20_20060311. Do not call this method.
        /// </summary>
        /// <param name="pSet"></param>
        //------------------------------------------------------------
        internal void RemoveSetFromList(CInputSet pSet)
        {
            throw new NotImplementedException("CController.RemoveSetFromList");
        }

        //------------------------------------------------------------
        // CController.CheckFlags
        //
        /// <summary>
        /// Determine if argument flags matches CController.creationFlags
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckFlags(CompilerCreationFlags flags)
        {
            return (creationFlags & flags) != 0;
        }

        //------------------------------------------------------------
        // CController.CheckDisplayWarning
        //
        /// <summary>
        /// <para>This function determines whether a warning should be displayed or suppressed,
        /// taking into account the warning level and "no warn" list.</para>
        /// </summary>
        /// <param name="errorId"></param>
        /// <param name="warnLevel"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CheckDisplayWarning(CSCERRID errorId, int warnLevel)
        {
            // this must be a warning.
            DebugUtil.Assert(warnLevel > 0);
            int errNo;
            int level;

            // Get error level by errorId.
            if (!CSCErrorInfo.Manager.GetInfo(errorId, out errNo, out level))
            {
                return false;
            }

            // Error level 99 means that it is obsolete.
            if (level == 99)
            {
                return false;
            }

            // Not show errors whose level are lower than warnLevel.
            if (warnLevel > OptionManager.WarningLevel)
            {
                return false;
            }

            if (OptionManager.IsNoWarnNumber(errNo))
            {
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // CController.CloneOptions
        //
        /// <summary></summary>
        /// <param name="dest"></param>
        //------------------------------------------------------------
        internal void CloneOptions(COptionManager dest)
        {
            lock (lockOptions)
            {
                dest.CopyFrom(this.OptionManager);
            }
        }

        // The following methods comprise the error handling/COMPILER-to-CController hosting

        //------------------------------------------------------------
        // CController.CreateError (1)
        //
        /// <summary>
        /// <para>This function creates a CError object from the given information.
        /// The CError object is created with no location information.
        /// The CError object is returned with a ref count of 0.</para>
        /// </summary>
        /// <param name="errorId">Error ID.</param>
        /// <param name="args">Arguments for error message.</param>
        /// <param name="error">A created CError instance will be set.</param>
        /// <param name="warnOverride">if true, treats a error as a warning.</param>
        /// <returns>if failed to create a instance, return false.</returns>
        /// <remarks>
        /// <para>If "warnOverride" is true (defaults to false), and this error is usually
        /// an error (not warning or fatal), then the error is wrapped with warning WRN_ErrorOverride</para>
        /// </remarks>
        //------------------------------------------------------------
        internal bool CreateError(
            CSCERRID errorId,
            ErrArg[] args,
            out CError error,
            bool warnOverride)  // = false);
        {
            error = null;
            // Make some assertions...
            DebugUtil.Assert(errorId > 0 && errorId < CSCERRID.ERR_COUNT);
            DebugUtil.Assert(errorId != CSCERRID.WRN_ErrorOverride);

            CError errorObject = null;
            int errNo;
            int level1, level2;

            // when errorId is invalid, or has no level, return false; 
            if (!CSCErrorInfo.Manager.GetInfo(errorId, out errNo, out level1))
            {
                goto LERROR;
            }

            if (level1 == 0 && warnOverride)
            {
                // If errorId is of ERROR and should treat it as WARNING,
                // and if CSCERRID.WRN_ErrorOverride is lower than optionData.WarnLevel,
                // return false.
                if (CSCErrorInfo.Manager.GetWarningLevel(CSCERRID.WRN_ErrorOverride, out level2))
                {
                    if (!CheckDisplayWarning(CSCERRID.WRN_ErrorOverride, level2))
                    {
                        return true;
                    }
                }
            }
            else
            {
                // Clear this bit (since we clearly aren't overriding an error to a warning)
                warnOverride = false;

                // If it's a warning, does it meet the warning level criteria?
                if (level1 > 0 && !CheckDisplayWarning(errorId, level1))
                {
                    return true;
                }
            }

            // Create a CError instance and initialize it.
            errorObject = new CError();
            if (errorObject == null || !errorObject.Initialize((CSCERRID)errorId, args))
            {
                goto LERROR;
            }

            // Do we need to wrap this Error in a warning?
            if (warnOverride == true)
            {
                DebugUtil.Assert(errorObject.Kind == ERRORKIND.ERROR);
                if (!errorObject.Initialize(
                        CSCERRID.WRN_ErrorOverride,
                        new ErrArg(errorObject.Text),
                        new ErrArg(errorObject.ErrorID)))
                {
                    goto LERROR;
                }
            }

            // Now check to see if we need to promote this warning to an error
            if (errorObject.Kind == ERRORKIND.WARNING &&
                this.OptionManager.IsWarnAsError(errorObject.ErrorID) &&
                !errorObject.WarnAsError())
            {
                goto LERROR;
            }
            error = errorObject;
            return true;

        LERROR:
            if (errorObject != null &&
                !String.IsNullOrEmpty(errorObject.Text))
            {
                OnCatastrophicError(errorObject.Text);
            }
            else
            {
                OnCatastrophicError("CController.CreateError");
            }
            error = null;
            return false;
        }

        //------------------------------------------------------------
        // CController.CreateError (2)
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="text"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CreateError(
            ERRORKIND kind,
            string text,
            out CError error)
        {
            error = null;
            if (String.IsNullOrEmpty(text))
            {
                return false;
            }

            error = new CError();
            error.Initialize(kind, text);
            return true;
        }

        //------------------------------------------------------------
        // CController.CreateError (3)
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="excp"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CreateError(
            ERRORKIND kind,
            Exception excp,
            out CError error)
        {
            error = null;
            if (excp == null)
            {
                return false;
            }

            error = new CError();
            error.Initialize(kind, excp);
            return true;
        }

        //------------------------------------------------------------
        // CController.CountErrorObject
        //
        /// <summary></summary>
        /// <param name="errorObj"></param>
        //------------------------------------------------------------
        private void CountErrorObject(CError errorObj)
        {
            if (!errorObj.Counted)
            {
                switch (errorObj.Kind)
                {
                    case ERRORKIND.FATAL:
                    case ERRORKIND.ERROR:
                        if (errorObj.WasWarning)
                        {
                            ++(this.countOfWarnsReportedAsErrors);
                        }
                        ++(this.countOfReportedErrors);
                        break;

                    default:
                        break;
                }
                errorObj.Counted = true;
            }
        }

        //------------------------------------------------------------
        // CController.CountErrorObject
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        //------------------------------------------------------------
        private void CountReportedError(ERRORKIND kind)
        {
            switch (kind)
            {
                case ERRORKIND.FATAL:
                case ERRORKIND.ERROR:
                    ++(this.countOfReportedErrors);
                    //++(this.countOfWarnsReportedAsErrors);
                    break;

                default:
                    break;
            }
        }

        //------------------------------------------------------------
        // CController.SubmitError
        //
        /// <summary>
        /// <para>This function places a fully-constructed CError object into an error container
        /// and sends it to the compiler host (this would be the place to batch these guys
        /// up if we decide to.</para>
        /// </summary>
        /// <param name="error"></param>
        /// <remarks>
        /// Note that if the error can't be put into a container (if, for example, we
        /// can't create a container) the error is destroyed and the host is notified via
        /// OnCatastrophicError.
        /// </remarks>
        //------------------------------------------------------------
        internal void SubmitError(CError error)
        {
            // Allow NULL --
            // this is often called with a function that returns an error as an argument;
            // it may not actually be an error.
            if (error == null)
            {
                return;
            }

            // Remember that we had an error (if this isn't a warning)
            CountErrorObject(error);

            if (this.SuppressErrors && error.Kind != ERRORKIND.FATAL)
            {
                return;
            }

            // Make sure we have an error container we can use.  Note that we (somewhat hackily)
            // check the ref count on any existing container, and if 1, re-use it.  (If it's
            // 1, it means we have the only ref on it, so nobody will be hurt by re-using it).
            if (this.compilerErrors != null)
            {
                // This one can be re-used -- just empty it.
                this.compilerErrors.ReleaseAllErrors();
            }

            // Create a new container for the errors
            if (this.compilerErrors == null)
            {
                this.compilerErrors = CErrorContainer.CreateInstance(ERRORCATEGORY.COMPILATION, 0);
            }

            // We must have a container by now!  Add the error and push it to the host.
            DebugUtil.Assert(compilerErrors != null);
            if (this.compilerErrors.AddError(error))
            {
                this.ReportErrors(this.compilerErrors);
            }
        }
        
        //------------------------------------------------------------
        // CController.ReportError (1)
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="message"></param>
        //------------------------------------------------------------
        internal void ReportError(ERRORKIND kind, string message)
        {
            CountReportedError(kind);

            string buffer = FormatStringUtil.FormatErrorMessageCore(
                null, -1, -1, -1, -1,
                kind,
                null, -1,
                message);
            WriteLine(buffer);
        }

        //------------------------------------------------------------
        // CController.ReportError (2a)
        //
        /// <summary>
        /// <para>(ConsoleOutput::ShowErrorIdString in sscli.)</para>
        /// </summary>
        /// <param name="errorID"></param>
        /// <param name="kind"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ReportError(CSCERRID errorID, ERRORKIND kind, params Object[] args)
        {
            string msg = null;
            Exception excp = null;
            msg = CSCErrorInfo.Manager.FormatErrorMessage(
                out excp,
                kind,
                errorID,
                args);

            if (msg != null && excp == null)
            {
                CountReportedError(kind);
                WriteLine(msg);
                return;
            }

            if (excp != null)
            {
                OnCatastrophicError(excp.Message);
            }
        }

        //------------------------------------------------------------
        // CController.ReportError (2b)
        //
        /// <summary>
        /// <para>(ConsoleOutput::ShowErrorIdString in sscli.)</para>
        /// </summary>
        /// <param name="errorID"></param>
        /// <param name="kind"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void ReportError(ALERRID errorID, ERRORKIND kind, params Object[] args)
        {
            string msg = null;
            Exception excp = null;
            msg = ALErrorInfo.Manager.FormatErrorMessage(
                out excp,
                kind,
                errorID,
                args);

            if (msg != null && excp == null)
            {
                CountReportedError(kind);
                WriteLine(msg);
                return;
            }

            if (excp != null)
            {
                OnCatastrophicError(excp.Message);
            }
        }

        //------------------------------------------------------------
        // CController.ReportError (3)
        //
        /// <summary>
        /// <para>(In sscli, CompilerHost::ReportError)</para>
        /// </summary>
        /// <param name="errorObject"></param>
        //------------------------------------------------------------
        internal void ReportError(CError errorObject)
        {
            if (errorObject == null)
            {
                return;
            }

            List<string> messages = errorObject.GetErrorMessages(false);
            for (int i = 0; i < messages.Count; ++i)
            {
                this.WriteLine(messages[i]);
            }
            CountReportedError(errorObject.Kind);
        }

        //------------------------------------------------------------
        // CController.ReportErrors
        //
        /// <summary>
        /// <para>(In sscli, CompilerHost::ReportErrors
        /// and CController::ReportErrorsToHost)</para>
        /// </summary>
        /// <param name="errorContainer"></param>
        //------------------------------------------------------------
        internal void ReportErrors(CErrorContainer errorContainer)
        {
            for (int i = 0; i < errorContainer.Count; i++)
            {
                ReportError(errorContainer.GetErrorAt(i));
            }
        }

        //------------------------------------------------------------
        // CController.ReportError (4)
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void ReportError(ERRORKIND kind, Exception excp)
        {
            if (excp == null)
            {
                return;
            }
            ReportError(kind, excp.Message);
        }

        //------------------------------------------------------------
        // CController.ReportInternalCompilerError
        //
        /// <summary></summary>
        /// <param name="desc"></param>
        //------------------------------------------------------------
        internal void ReportInternalCompilerError(string desc)
        {
            ReportError(CSCERRID.FTL_InternalError, ERRORKIND.FATAL, desc);
        }

        //------------------------------------------------------------
        // CController.OnCatastrophicError (1)
        //
        /// <summary>
        /// <para>(CompilerHost in sscli)</para>
        /// </summary>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void OnCatastrophicError(string msg)
        {
            ReportError(ERRORKIND.FATAL, msg);
        }

        //------------------------------------------------------------
        // CController.OnCatastrophicError (2)
        //
        /// <summary></summary>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void OnCatastrophicError(Exception excp)
        {
            OnCatastrophicError(excp.Message);
        }

        //------------------------------------------------------------
        // CController.HandleException (2)
        //
        /// <summary></summary>
        /// <param name="excp"></param>
        //------------------------------------------------------------
        internal void HandleException(Exception excp)
        {
            OnCatastrophicError(excp);
            ++countOfReportedErrors;
        }

        //------------------------------------------------------------
        // CController.HandleException
        //------------------------------------------------------------
        //internal void SetExceptionData(EXCEPTION_POINTERS exceptionInfo)
        //{
        //    exceptionAddress = exceptionInfo.ExceptionRecord.ExceptionAddress;
        //}

        //------------------------------------------------------------
        // CController.HandleException
        //------------------------------------------------------------
        //internal Object GetExceptionAddress() { return exceptionAddress; }

        //------------------------------------------------------------
        // CController.ShowMessage (1)
        //
        /// <summary></summary>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal void ShowMessage(string msg)
        {
            WriteLine(msg);
        }

        //------------------------------------------------------------
        // CController.ShowMessage (2)
        //
        /// <summary></summary>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal void ShowMessage(string fmt, params Object[] args)
        {
            WriteLine(fmt, args);
        }

        //------------------------------------------------------------
        // CController.ShowMessage (3)
        //
        /// <summary></summary>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal void ShowMessage(ResNo resNo, params Object[] args)
        {
            string fmt=null;
            int count=0;
            Exception excp = null;

            if (CResources.GetString(resNo, out fmt, out count, out excp))
            {
                FormatStringUtil.FillupArguments(ref args, count);
                WriteLine(fmt, args);
            }
            else
            {
                if (excp != null)
                {
                    WriteLine(excp.Message);
                }
                else
                {
                    WriteLine("Unknow error in ShowMessage");
                }
            }
        }

        //------------------------------------------------------------
        // CController.CreateSourceModule
        //
        /// <summary>
        /// <para>Create a CSourceModule instance
        /// with an instance of a class implementing ICSSourceText.</para>
        /// <para>Call CSourceModule.CreateInstance and handle exceptions if throwed.</para>
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CSourceModule CreateSourceModule(CSourceText text)
        {
            try
            {
                return CSourceModule.CreateInstance(this, text);
            }
            catch (Exception excp)
            {
                OnCatastrophicError(excp);
            }
            return null;
        }

        //------------------------------------------------------------
        // CController.GetSourceModule
        //
        /// <summary>
        /// <para>(In sscli, CInMemoryCompilerHost::GetSourceModule)</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="needChecksum"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CSourceModule GetSourceModule(
            FileInfo srcFileInfo,
            bool needChecksum)
        {
            CSourceText sourceText = new CSourceText();
            // Read the source file.
            if (!sourceText.Initialize(srcFileInfo, needChecksum, encoding, this))
            {
                return null;
            }
            return CreateSourceModule(sourceText);//, out module);
        }


        //------------------------------------------------------------
        // CController.Shutdown
        //
        /// <summary>
        /// Not do anything.
        /// (In sscli, release com objects.)
        /// </summary>
        //------------------------------------------------------------
        virtual internal void Shutdown()
        {
        }

        //------------------------------------------------------------
        // CController.CreateNewInputSet
        //
        /// <summary>
        /// <para>Create a new CInputSet instance, register to this.inputSetList,
        /// and return it.</para>
        /// <para>(AddInputSet in sscli.)</para>
        /// </summary>
        /// <returns>A created CInputSet instance.</returns>
        //------------------------------------------------------------
        virtual internal CInputSet CreateNewInputSet(TargetType defaultTarget)
        {
            CInputSet inp = new CInputSet(this);
            inp.SetTargetType(defaultTarget);
            this.inputSetList.Add(inp);
            return inp;
        }

        //------------------------------------------------------------
        // CController.RemoveInputSet
        //
        /// <summary>Not implemented. Return false.</summary>
        /// <param name="pInputSet"></param>
        //------------------------------------------------------------
        virtual internal bool RemoveInputSet(CInputSet pInputSet)
        {
            return false;
        }

        //------------------------------------------------------------
        // CController.BeginNewInputSet
        //
        /// <summary>
        /// <para>(sscli)
        /// This function is called to make sure the given input set is "fresh",
        /// meaning it hasn't had any files added to it yet.</para>
        /// </summary>
        /// <param name="inputSet"></param>
        /// <param name="fileAdded"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BeginNewInputSet(ref CInputSet inputSet, ref bool fileAdded)
        {
            if (fileAdded)
            {
                fileAdded = false;
                // default for subsequent executables is DLL.
                inputSet = CreateNewInputSet(TargetType.Module);
                DebugUtil.Assert(inputSet != null);
                //inputSet.SetTargetType(TargetType.Module);
            }
            return true;
        }

        //------------------------------------------------------------
        // CController.AddInputFiles
        //
        /// <summary>
        /// <para>Check if searchString is valid.
        /// If it is valid, register all files matching to searchString to inputSet.
        /// If it is invalid, throw an exception.</para>
        /// <para>(In sscli, ProcessFileName in csharp\scc\scc.cpp)</para>
        /// </summary>
        /// <param name="pInputSet"></param>
        //------------------------------------------------------------
        internal void AddInputFiles(
            string fileStr,
            CInputSet inputSet,
            bool recursively,
            ref bool fileAdded)
        {
            DirectoryInfo dirInfo = null;
            string filePattern = null;
            Exception excp = null;

            // オプション文字列からディレクトリ名を取り出し、有効かどうか調べる。
            if (!IOUtil.CreateDirectoryInfoFromFilePath(fileStr, out dirInfo, out excp))
            {
                ReportError(ERRORKIND.ERROR, excp);
                return;
            }
            if (!dirInfo.Exists)
            {
                ReportError(
                    ERRORKIND.ERROR,
                    String.Format("{0} could not be found.", dirInfo.Name));
                return;
            }
            filePattern = Path.GetFileName(fileStr);

            // 指定されたディレクトリ内でファイル名にマッチするものすべてを探す。
            // ただし、hidden 属性を持つものとディレクトリは除く
            List<FileInfo> fileInfoList = new List<FileInfo>();
            IOUtil.EnumFileInfo(
                dirInfo,
                filePattern,
                0,
                FileAttributes.Hidden | FileAttributes.Directory,
                fileInfoList,
                recursively);

            // 見つかったファイルのフルパスを inputSet に登録する。
            if (fileInfoList.Count > 0)
            {
                foreach (FileInfo fi in fileInfoList)
                {
                    if (fi != null)
                    {
                        inputSet.AddSourceFile(fi);
                        fileAdded = true;
                    }
                }
            }
            else
            {
                ReportError(CSCERRID.ERR_FileNotFound, ERRORKIND.ERROR, fileStr);
            }
        }

        //------------------------------------------------------------
        // CController.Compile
        //
        /// <summary>
        /// <para>Call RunCompiler method, then call Cleanup method.</para>
        /// </summary>
        /// <param name="pProgress"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool Compile(CCompileProgress pProgress)
        {
            COMPILER compiler = null;   // dummy

            bool br = RunCompiler(pProgress, ref compiler, null);
            Cleanup();
            return br;
        }

        //------------------------------------------------------------
        // CController.BuildForEnc
        //
        /// <summary></summary>
        /// <param name="pProgress"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool BuildForEnc(CCompileProgress pProgress)
        {
            // Reset the error count here in case there's a problem
            // with one of the input-sets or imports
            this.countOfReportedErrors = 0;

            COMPILER comp = new COMPILER(this, NameManager);
            bool br = true;

            if (comp.Init())
            {
                for (int i = 0; i < inputSetList.Count; ++i)
                {
                    br = comp.AddInputSet(inputSetList[i]);
                    if (!br)
                    {
                        break;
                    }
                }
            }
            Cleanup();
            return br;
        }

        //------------------------------------------------------------
        // CController.GetOutputFileName
        //
        /// <summary>
        /// inputSetList の最後の CInputSet インスタンスから、出力ファイル名を取得する。
        /// </summary>
        /// <param name="fileName">出力ファイル名がセットされる。</param>
        /// <returns>取得できたら true を返す。</returns>
        //------------------------------------------------------------
        virtual internal bool GetOutputFileName(out string fileName)
        {
            fileName = null;

            if (this.inputSetList == null || this.inputSetList.Count == 0)
            {
                return false;
            }

            try
            {
                fileName = this.inputSetList[inputSetList.Count - 1].OutputFileName;
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // CController.CreateTextParser
        //
        /// <summary>Create a CTextParser instance.</summary>
        /// <returns>作成した CTextParser インスタンス。</returns>
        /// <remarks>
        /// STDMETHODIMP CController::CreateParser (ICSParser **ppParser)
        /// ICSParser is implermented only by CTextParser.
        /// </remarks>
        //------------------------------------------------------------
        virtual internal CTextParser CreateTextParser()
        {
            CTextParser parser = new CTextParser();
            parser.Initialize(this, this.OptionManager.LangVersion);
            return parser;
        }

        //------------------------------------------------------------
        // CController.RunCompiler
        //
        /// <summary>
        /// <list type="number">
        /// <item><description>Create a COMPILER instance.</description></item>
        /// <item><description>Register input/out files to the COMPILER instance.</description></item>
        /// <item><description>call COMPILER.Complie method.</description></item>
        /// </list>
        /// </summary>
        /// <param name="bindCallback"></param>
        /// <param name="compiler"></param>
        /// <param name="progress"></param>
        /// <returns>if succeeded, return true.returns>
        /// <remarks>
        /// ICSCompiler Helpers
        /// </remarks>
        //------------------------------------------------------------
        internal bool RunCompiler(
            CCompileProgress progress,
            ref COMPILER compiler,
            ICompileCallback bindCallback)
        {
            // Reset the error count here in case there's a problem
            // with one of the input-sets or imports
            this.countOfReportedErrors = 0;
            this.countOfWarnsReportedAsErrors = 0;
            //bool needsCleanUp = false;
            bool br = true;

            // Instantiate a compiler on the stack
            // Initialize it

            compiler = new COMPILER(this, NameManager);

            // For each imported assemblies and added modules, create INFILESYMs
            // and add then to compiler.MainSymbolManager.MetadataFileRootSym
            br = compiler.Init();
            if (br)
            {
                // For each input set, add the appropriate data to the compiler
                // For outputfile, create an OUTFILESYM and add it to GlobalSymbolManager.FileRootSym.
                // For source files and resource files, create INFILESYM and add then to the OUTFILESYM.

                foreach (CInputSet inpset in this.inputSetList)
                {
                    br = compiler.AddInputSet(inpset);
                    if (!br)
                    {
                        break;
                    }
                }
            }

            // Tell it to compile
            if (br && countOfReportedErrors == 0)
            {
                compiler.CompileCallback.Init(bindCallback);
                br = compiler.Compile(progress);
            }
            return br;
        }

        //------------------------------------------------------------
        // CController.Cleanup
        //------------------------------------------------------------
        internal void Cleanup()
        {
            // If we still have an error container, get rid of it
            if (compilerErrors != null)
            {
                // The container will have already been sent back to the host
                compilerErrors.ReleaseAllErrors();
                compilerErrors = null;
            }
        }
    }
}
