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
// File: posdata.h
//
// ===========================================================================

// ===========================================================================
// File: shared\positiondata.cpp
//
// ===========================================================================

//============================================================================
//  Inc_PosData.cs
//
//  2013/10/04 hirano567@hotmail.co.jp
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    //	POSDATA
    //
    /// <summary>
    /// <para>(sscli)
    /// This class holds line and character index data in a 64-bit structure, using
    ///	only 52 bits (leaving a byte-and-a-half for user-defined storage).</para>
    /// </summary>
    //======================================================================
    internal class POSDATA
    {
        //------------------------------------------------------------
        // Constants
        //------------------------------------------------------------

        /// <summary>
        /// 24 bits of size data per line, minus one to distinquish from POSDATA(-1)
        /// </summary>
        internal const int MAX_POS_LINE_LEN = 0x00fffffe;

        /// <summary>
        /// 28 bits of line data, minus one to distinquish from POSDATA(-1)
        /// </summary>
        internal const int MAX_POS_LINE = 0x0ffffffe;

        /// <summary>
        /// 0x0fffffff (28 bits)
        /// </summary>
        internal const int LINEMASK = 0x0fffffff;

        /// <summary>
        /// 0x00ffffff (24 bits)
        /// </summary>
        internal const int CHARMASK = 0x00ffffff;

        //------------------------------------------------------------
        // POSDATA  Fields
        //------------------------------------------------------------
        // The alignment optimizes accesses to CharIndex and LineIndex such that no
        // shifting is necessary.

        /// <summary>
        /// <para>16,777,214 characters per line max</para>
        /// </summary>
        internal int CharIndex;

        /// <summary>
        /// <para>Unused "user" data (used for the token value by the token stream, etc)</para>
        /// </summary>
        internal int UserByte;

        /// <summary>
        /// <para>268,435,454 lines per file max</para>
        /// </summary>
        internal int LineIndex;

        /// <summary>
        /// <para>Unused "user" bits</para>
        /// </summary>
        internal int UserBits;

        //------------------------------------------------------------
        // POSDATA  Constructor (1)
        //
        /// <summary>
        /// Constructor. Set all fields undefined.
        /// </summary>
        //------------------------------------------------------------
        internal POSDATA()
        {
            CharIndex = CHARMASK;
            UserByte = 0xff;
            LineIndex = LINEMASK;
            UserBits = 0xf;
        }

        //------------------------------------------------------------
        // POSDATA  Constructor (2)
        //
        /// <summary>
        /// Constructor. Set indice of line and character.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="c"></param>
        //------------------------------------------------------------
        internal POSDATA(int i, int c)
        {
            CharIndex = c;
            UserByte = 0;
            LineIndex = i;
            UserBits = 0;
        }

        //------------------------------------------------------------
        // POSDATA  Constructor (3)
        //
        /// <summary>
        /// Constructor. Copy from other POSDATA instance.
        /// </summary>
        /// <param name="p"></param>
        //------------------------------------------------------------
        internal POSDATA(POSDATA p)
        {
            CharIndex = p.CharIndex;
            UserByte = p.UserByte;
            LineIndex = p.LineIndex;
            UserBits = p.UserBits;
        }

        //------------------------------------------------------------
        // POSDATA.Clone
        //
        /// <summary>
        /// Duplicate this.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal POSDATA Clone()
        {
            POSDATA pd = new POSDATA(this);
            return pd;
        }

        //------------------------------------------------------------
        // POSDATA.CopyFrom
        //
        /// <summary>
        /// Set fields by the values which other holds.
        /// </summary>
        /// <param name="other"></param>
        //------------------------------------------------------------
        internal void CopyFrom(POSDATA other)
        {
            if (other == null)
            {
                return;
            }
            this.CharIndex = other.CharIndex;
            this.UserByte = other.UserByte;
            this.LineIndex = other.LineIndex;
            this.UserBits = other.UserBits;
        }

        //------------------------------------------------------------
        // POSDATA.Adjust
        //
        /// <summary>
        /// Move this by reference to posOld and posNew.
        /// </summary>
        /// <param name="posOld"></param>
        /// <param name="posNew"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>Like string insertion of editors.</para>
        /// <para>Why do nothing when this and old are equal?</para>
        /// </remarks>
        //------------------------------------------------------------
        internal bool Adjust(POSDATA posOld, POSDATA posNew)
        {
            DebugUtil.Assert(posOld != null && posNew != null);
            DebugUtil.Assert(!this.IsUninitialized && !posOld.IsUninitialized && !posNew.IsUninitialized);

            // Nothing to adjust if the change is below us, or isn't really a change
            if (posOld.LineIndex > LineIndex || posOld.Equals(posNew))
            {
                return false;
            }

            if (posOld.LineIndex == LineIndex)
            {
                // The old position is on the same line as us.  If the
                // char position is before us, update it.
                if (posOld.CharIndex < CharIndex)
                {
                    CharIndex += (posNew.CharIndex - posOld.CharIndex);
                    LineIndex = posNew.LineIndex;
                    return true;
                }
                return false;
            }
            // The line must be above us, so just update our line
            LineIndex += (posNew.LineIndex - posOld.LineIndex);
            return ((posNew.LineIndex - posOld.LineIndex) != 0) ? true : false;
        }

        //------------------------------------------------------------
        // POSDATA.Compare
        //
        /// <summary>
        /// Compare position with other instance and return a integer.
        /// <list type="bullet">
        /// <item>&lt; 0  : this precedes p.</item>
        /// <item>= 0  : same position.</item>
        /// <item>&gt; 0  : p precedes this.</item>
        /// </list>
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int Compare(POSDATA p)
        {
            DebugUtil.Assert(!this.IsUninitialized && p != null && !p.IsUninitialized);

            int ld = this.LineIndex - p.LineIndex;
            return (ld != 0) ? ld: this.CharIndex - p.CharIndex;
        }

        //------------------------------------------------------------
        // POSDATA.Before
        //
        /// <summary></summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Before(POSDATA pos)
        {
            DebugUtil.Assert(pos != null);

            int ld = this.LineIndex - pos.LineIndex;
            return (ld != 0) ? ld < 0 : (this.CharIndex < pos.CharIndex);
        }

        //------------------------------------------------------------
        // POSDATA.After
        //
        /// <summary></summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool After(POSDATA pos)
        {
            DebugUtil.Assert(pos != null);

            int ld = this.LineIndex - pos.LineIndex;
            return (ld != 0) ? ld > 0 : (this.CharIndex > pos.CharIndex);
        }

        //------------------------------------------------------------
        // POSDATA.Equals   (override)
        //
        /// <summary>
        /// <para>Two POSDATA instance are equal
        /// if both LineIndex and CharIndex are same.</para>
        /// <para>Override Object.Equals().</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public override bool Equals(object obj)
        {
            POSDATA pd = null;
            if (obj == null || (pd = obj as POSDATA) == null)
            {
                return false;
            }
            return (this.LineIndex == pd.LineIndex && this.CharIndex == pd.CharIndex);
        }

        //------------------------------------------------------------
        // POSDATA.GetHashCode   (override)
        //
        /// <summary>Override Object.GetHashCode().</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override int GetHashCode()
        {
            return ((this.LineIndex.GetHashCode() ^ this.CharIndex.GetHashCode()) & 0x7FFFFFFF);
        }

        //------------------------------------------------------------
        // POSDATA Operator <
        //
        /// <summary></summary>
        //------------------------------------------------------------
        static public bool operator <(POSDATA p1, POSDATA p2)
        {
            return p1.Compare(p2) < 0;
        }

        //------------------------------------------------------------
        // POSDATA Operator >
        //
        /// <summary></summary>
        //------------------------------------------------------------
        static public bool operator >(POSDATA p1, POSDATA p2)
        {
            return p1.Compare(p2) > 0;
        }

        //------------------------------------------------------------
        // POSDATA Operator <=
        //
        /// <summary></summary>
        //------------------------------------------------------------
        static public bool operator <=(POSDATA p1, POSDATA p2)
        {
            return p1.Compare(p2) <= 0;
        }

        //------------------------------------------------------------
        // POSDATA Operator >=
        //
        /// <summary></summary>
        //------------------------------------------------------------
        static public bool operator >=(POSDATA p1, POSDATA p2)
        {
            return p1.Compare(p2) >= 0;
        }

        //------------------------------------------------------------
        // POSDATA.SetUninitialized
        //
        /// <summary>
        /// Set instance uninitialized.
        /// The POSDATA instance is unitialized
        /// if both CharIndex and LineIndex are undefined.
        /// </summary>
        //------------------------------------------------------------
        internal void SetUninitialized()
        {
            CharIndex = CHARMASK;
            LineIndex = LINEMASK;
        }

        //------------------------------------------------------------
        // POSDATA.SetZero
        //
        /// <summary>
        /// Set all fields zero.
        /// </summary>
        //------------------------------------------------------------
        internal void SetZero()
        {
            this.CharIndex = 0;
            this.UserByte = 0;
            this.LineIndex = 0;
            this.UserBits = 0;
        }

        //------------------------------------------------------------
        // POSDATA.IsUninitialized  (Property)
        //
        /// <summary>
        /// Return true if this is uninitialized.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsUninitialized
        {
            get
            {
                return (
                    ((CharIndex & CHARMASK) == CHARMASK) &&
                    ((LineIndex & LINEMASK) == LINEMASK));
            }
        }

        //------------------------------------------------------------
        // POSDATA.IsZero   (Property)
        //
        /// <summary>
        /// Return true if both LineIndex and CharIndex are 0.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsZero
        {
            get
            {
                return ((LineIndex == 0) && (CharIndex == 0));
            }
        }

        //------------------------------------------------------------
        // POSDATA Operator +
        //
        /// <summary>
        /// Add i to p.CharIndex.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static public POSDATA operator +(POSDATA p, int i)
        {
            DebugUtil.Assert(!p.IsUninitialized);

            POSDATA pr = new POSDATA(p);
            pr.CharIndex += i;
            return pr;
        }

        //------------------------------------------------------------
        // POSDATA Operator -
        //
        /// <summary>
        /// Subtract i from p.CharIndex.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static public POSDATA operator -(POSDATA p, int i)
        {
            DebugUtil.Assert(!p.IsUninitialized);

            POSDATA pr = new POSDATA(p);
            pr.CharIndex -= i;
            return pr;
        }

        //------------------------------------------------------------
        // POSDATA Operator -
        //
        /// <summary>
        /// If two POSDATA instance are in the same line,
        /// return the deference of CharIndex.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static public int operator -(POSDATA p1, POSDATA p2)
        {
            DebugUtil.Assert(!p1.IsUninitialized && !p2.IsUninitialized);
            DebugUtil.Assert(p1.LineIndex == p2.LineIndex);
            DebugUtil.Assert(p1.CharIndex >= p2.CharIndex);

            return (int)(p1.CharIndex - p2.CharIndex);
        }

        //------------------------------------------------------------
        // override POSDATA.ToString
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("({0}, {1})", LineIndex, CharIndex);
        }
    }
}
