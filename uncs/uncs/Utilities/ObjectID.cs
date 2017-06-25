//============================================================================
//  ObjectID.cs
//
//  2015/03/11
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // class CObjectID
    //
    /// <summary>
    /// <para>Class to generate the serial numbers.</para>
    /// </summary>
    //======================================================================
    internal static class CObjectID
    {
        static private int counter = 0;

        internal static int GenerateID()
        {
            if (counter == Int32.MaxValue)
            {
                throw new OverflowException("Cannot create more objects.");
            }
            return unchecked(++counter);
        }
    }

#if false
    //----------------------------------------------------------------------
    // class CSymID
    //----------------------------------------------------------------------
    internal class CSymID : CObjectID
    {
        internal CSymID() : base() { }
    }

    //----------------------------------------------------------------------
    // class CNodeID
    //----------------------------------------------------------------------
    internal class CNodeID : CObjectID
    {
        internal CNodeID() : base() { }
    }
#endif
}
