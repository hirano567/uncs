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

//============================================================================
// ErrorSuppression.cs
//
// 2015/03/11
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // class CErrorSuppression
    //
    /// <summary>
    /// <para>Save the old value of CController.SuppressErrors and set it.</para>
    /// <para>if CController.SuppressErrors is true, shows the FATAL errors only.</para>
    /// </summary>
    //======================================================================
    internal class CErrorSuppression
    {
        //------------------------------------------------------------
        // CErrorSuppression Fields and Properties
        //------------------------------------------------------------
        private CController controller = null;  // * m_pctr
        private bool isSet = false;             // m_fSet
        private bool suppressOld = false;       // m_fSuppressOld

        //------------------------------------------------------------
        // CErrorSuppression Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CErrorSuppression() { }

        //------------------------------------------------------------
        // CErrorSuppression Constructor (2)
        //
        /// <summary></summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal CErrorSuppression(CController cntr)
        {
            Suppress(cntr);
        }

        //------------------------------------------------------------
        // CErrorSuppression.Revert
        //
        /// <summary>
        /// If this stores old value false,
        /// restore to CController.SurpressErrors and clear this fields.
        /// </summary>
        //------------------------------------------------------------
        internal void Revert()
        {
            if (isSet)
            {
                DebugUtil.Assert(controller != null);

                if (!suppressOld)
                {
                    controller.SuppressErrors = false;
                }
                isSet = false;
                suppressOld = false;
                controller = null;
            }
            DebugUtil.Assert(!isSet && !suppressOld && controller == null);
        }

        //------------------------------------------------------------
        // CErrorSuppression.Suppress
        //
        /// <summary>
        /// <para>Save the value of CController.SurpressErrors,
        /// and set CController.SurpressErrors true.</para>
        /// <para>If this is already set, cannot set new value,
        /// need to call Revert method.</para>
        /// </summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal void Suppress(CController cntr)
        {
            DebugUtil.Assert(cntr != null);

            if (isSet)
            {
                DebugUtil.Assert(controller == cntr);
                return;
            }
            DebugUtil.Assert(controller == null && !suppressOld);
            controller = cntr;
            suppressOld = cntr.SuppressErrors;
            if (!suppressOld)
            {
                cntr.SuppressErrors = true;
            }
            isSet = true;
        }
    }
}
