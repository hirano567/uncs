//============================================================================
// CSException.cs  (uncs\Utilities\)
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // class ExceptionUtil
    //======================================================================
    internal static class ExceptionUtil
    {
        internal static string DefaultMessage = "Internal Compiler Error";

        internal static string InternalErrorMessage(string loc)
        {
            if (!String.IsNullOrEmpty(loc))
            {
                return String.Format("{0}: {1}", loc, DefaultMessage);
            }
            return DefaultMessage;
        }
    }

    //======================================================================
    // class BaseException
    //
    /// <summary>
    /// <para>The base class of exception of this project. abstract.</para>
    /// <para>(Defined in Utilities\Exception.cs)</para>
    /// </summary>
    /// <typeparam name="IDTYPE"></typeparam>
    //======================================================================
    abstract internal class BaseException<IDTYPE> : System.Exception
    {
        //------------------------------------------------------------
        // BaseException Fields
        //------------------------------------------------------------
        internal ERRORKIND Kind = ERRORKIND.ERROR;
        internal IDTYPE ErrorID = default(IDTYPE);
        internal uint Flags = 0;
        internal Object[] Arguments = null;
        internal new string Message = null;

        //------------------------------------------------------------
        // BaseException.GetErrorKind (abstract)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        abstract protected ERRORKIND GetErrorKind(IDTYPE id);

        //------------------------------------------------------------
        // BaseException.SetInvalidID (abstract)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        abstract protected void SetInvalidID();

        //------------------------------------------------------------
        // BaseException.IsInvalidID (abstract)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        abstract protected bool IsInvalidID();

        //------------------------------------------------------------
        // BaseException.FormatErrorMessage (abstract)
        //
        /// <summary></summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        abstract protected bool FormatErrorMessage(out string message);

        //------------------------------------------------------------
        // BaseException Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal BaseException()
        {
            this.SetInvalidID();
        }

        //------------------------------------------------------------
        // BaseException Constructor (2)
        //
        /// <summary></summary>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal BaseException(string msg)
            : base(msg)
        {
            Set(msg);
        }

        //------------------------------------------------------------
        // BaseException Constructor (3)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal BaseException(IDTYPE id, params Object[] args)
        {
            this.Set(id, args);
        }

        //------------------------------------------------------------
        // BaseException.Set (1)
        //
        /// <summary></summary>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal void Set(string msg)
        {
            this.SetInvalidID();
            this.Arguments = null;
            this.Message = msg;
        }

        //------------------------------------------------------------
        // BaseException.Set (2)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal void Set(IDTYPE id, params Object[] args)
        {
            this.ErrorID = id;
            this.Kind = GetErrorKind(id);
            this.Arguments = args;
            this.FormatErrorMessage(out this.Message);
        }

        //------------------------------------------------------------
        // BaseException.ToString (override)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(this.Message))
            {
                return this.Message;
            }
            else if (this.FormatErrorMessage(out this.Message))
            {
                return this.Message;
            }

            if (!String.IsNullOrEmpty(base.ToString()))
            {
                return base.ToString();
            }
            return ExceptionUtil.DefaultMessage;
        }
    }

    //======================================================================
    // class CSException
    //
    /// <summary>
    /// <para>Base exception class for C# compiler.</para>
    /// <para>(Defined in Utilities\Exception.cs)</para>
    /// </summary>
    //======================================================================
    internal class CSException : BaseException<CSCERRID>
    {
        //------------------------------------------------------------
        // CSException Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CSException()
            : base()
        {
        }

        //------------------------------------------------------------
        // CSException Constructor (2)
        //
        /// <summary></summary>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal CSException(string msg)
            : base(msg)
        {
        }

        //------------------------------------------------------------
        // CSException Constructor (3)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal CSException(CSCERRID id, params Object[] args)
            : base(id, args)
        {
        }

        //------------------------------------------------------------
        // CSException.GetErrorKind (override)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected override ERRORKIND GetErrorKind(CSCERRID id)
        {
            int level;
            if (CSCErrorInfo.Manager.GetWarningLevel(id, out level))
            {
                if (level == 0)
                {
                    return ERRORKIND.ERROR;
                }
                else if (level < 0)
                {
                    return ERRORKIND.FATAL;
                }
                return ERRORKIND.WARNING;
            }
            DebugUtil.Assert(false);
            return ERRORKIND.FATAL;
        }

        //------------------------------------------------------------
        // CSException.IsInvalidID (override)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        protected override bool IsInvalidID()
        {
            return (this.ErrorID == CSCERRID.Invalid);
        }

        //------------------------------------------------------------
        // CSException.SetInvalidID (override)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        protected override void SetInvalidID()
        {
            this.ErrorID = CSCERRID.Invalid;
        }

        //------------------------------------------------------------
        // CSException.FormatErrorMessage (override)
        //
        /// <summary></summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected override bool FormatErrorMessage(out string message)
        {
            Exception excp = null;
            if (CSCErrorInfo.Manager.FormatErrorMessage(
                out message,
                out excp,
                this.ErrorID,
                this.Arguments))
            {
                return true;
            }

            if (excp != null)
            {
                message = excp.Message;
            }
            else
            {
                message = ExceptionUtil.InternalErrorMessage("FormatErrorMessage");
            }
            return false;
        }
    }

    //======================================================================
    // class ALException
    //
    /// <summary>
    /// <para>Base exception class for C# linker.</para>
    /// <para>(Defined in Utilities\Exception.cs)</para>
    /// </summary>
    //======================================================================
    internal class ALException : BaseException<ALERRID>
    {
        //------------------------------------------------------------
        // ALException Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal ALException()
            : base()
        {
        }

        //------------------------------------------------------------
        // ALException Constructor (2)
        //
        /// <summary></summary>
        /// <param name="msg"></param>
        //------------------------------------------------------------
        internal ALException(string msg)
            : base(msg)
        {
        }

        //------------------------------------------------------------
        // ALException Constructor (3)
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        //------------------------------------------------------------
        internal ALException(ALERRID id, params Object[] args)
            : base(id, args)
        {
        }

        //------------------------------------------------------------
        // ALException.GetErrorKind
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected override ERRORKIND GetErrorKind(ALERRID id)
        {
            int level;
            if (ALErrorInfo.Manager.GetWarningLevel(id, out level))
            {
                if (level == 0)
                {
                    return ERRORKIND.ERROR;
                }
                else if (level < 0)
                {
                    return ERRORKIND.FATAL;
                }
                return ERRORKIND.WARNING;
            }
            DebugUtil.Assert(false);
            return ERRORKIND.FATAL;
        }

        //------------------------------------------------------------
        // ALException.IsInvalidID
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        protected override bool IsInvalidID()
        {
            return (this.ErrorID == ALERRID.Invalid);
        }

        //------------------------------------------------------------
        // ALException.SetInvalidID
        //
        /// <summary></summary>
        //------------------------------------------------------------
        protected override void SetInvalidID()
        {
            this.ErrorID = ALERRID.Invalid;
        }

        //------------------------------------------------------------
        // ALException.FormatErrorMessage
        //
        /// <summary></summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected override bool FormatErrorMessage(out string message)
        {
            Exception excp = null;
            if (ALErrorInfo.Manager.FormatErrorMessage(
                out message,
                out excp,
                this.ErrorID,
                this.Arguments))
            {
                return true;
            }

            if (excp != null)
            {
                message = excp.Message;
            }
            else
            {
                message = ExceptionUtil.InternalErrorMessage("FormatErrorMessage");
            }
            return false;
        }
    }

    //======================================================================
    // LogicError
    //
    // プログラムエラーを示すための例外。
    //======================================================================
    internal class LogicError : System.Exception
    {
        //internal LogicError() : base() { }
        internal LogicError(string msg) : base(msg) { }
    }
#if false
    //======================================================================
    // Exception for HRESULT values.
    //
    // S_OK	Success.
    // S_FALSE	Success.
    // E_PENDING	The data necessary to complete this operation is not yet available.
    // E_BOUNDS	The operation attempted to access data outside the valid range
    // E_CHANGED_STATE	A concurrent or interleaved operation changed the state of the object
    // E_ILLEGAL_STATE_CHANGE	An illegal state change was requested.
    // E_ILLEGAL_METHOD_CALL	A method was called at an unexpected time.
    // E_NOTIMPL	Not implemented
    // E_NOINTERFACE	No such interface supported
    // E_POINTER	Invalid pointer
    // E_ABORT	Operation aborted
    // E_FAIL	Unspecified error
    // E_UNEXPECTED	Catastrophic failure
    // E_ACCESSDENIED	General access denied error
    // E_HANDLE	Invalid handle
    // E_OUTOFMEMORY	Ran out of memory
    // E_INVALIDARG	One or more arguments are invalid
    //======================================================================
    internal class ComException : CSException
    {
        internal ComException(string msg) : base(msg) { }
        internal ComException(string msg, bool br) : base(msg) { succeeded = br; }

        internal bool succeeded = false;
        internal bool Succeeded { get { return succeeded; } }
        internal bool Failed { get { return !succeeded; } }
    }

    internal class ComSOkException : ComException
    {
        internal ComSOkException() : base("Success.", true) { }
    }

    internal class ComSFalseException : ComException
    {
        internal ComSFalseException() : base("Success.", true) { }
    }

    internal class ComEPendingException : ComException
    {
        internal ComEPendingException() : base("The data necessary to complete this operation is not yet available.") { }
    }

    internal class ComEBoundsException : ComException
    {
        internal ComEBoundsException() : base("The operation attempted to access data outside the valid range.") { }
    }

    internal class ComEChangedStateException : ComException
    {
        internal ComEChangedStateException() : base("A concurrent or interleaved operation changed the state of the object.") { }
    }

    internal class ComEIllegalStateChangeException : ComException
    {
        internal ComEIllegalStateChangeException() : base("An illegal state change was requested.") { }
    }

    internal class ComEIllegalMethodCallException : ComException
    {
        internal ComEIllegalMethodCallException() : base("A method was called at an unexpected time.") { }
    }

    internal class ComENotImplException : ComException
    {
        internal ComENotImplException() : base("No such interface supported.") { }
    }

    internal class ComENoInterfaceException : ComException
    {
        internal ComENoInterfaceException() : base("No such interface supported.") { }
    }

    internal class ComEPointerException : ComException
    {
        internal ComEPointerException() : base("Invalid pointer.") { }
    }

    internal class ComEAbortException : ComException
    {
        internal ComEAbortException() : base("Operation aborted.") { }
    }

    internal class ComEFailException : ComException
    {
        internal ComEFailException() : base("Unspecified error.") { }
    }

    internal class ComEUnexpectedException : ComException
    {
        internal ComEUnexpectedException() : base("Catastrophic failure.") { }
    }

    internal class ComEAccessDeniedException : ComException
    {
        internal ComEAccessDeniedException() : base("General access denied error.") { }
    }

    internal class ComEHandleException : ComException
    {
        internal ComEHandleException() : base("Invalid handle.") { }
    }

    internal class ComEOutOfMemoryException : ComException
    {
        internal ComEOutOfMemoryException() : base("Ran out of memory.") { }
    }

    internal class ComEInvalidArgException : ComException
    {
        internal ComEInvalidArgException() : base("One or more arguments are invalid") { }
    }
#endif
}
