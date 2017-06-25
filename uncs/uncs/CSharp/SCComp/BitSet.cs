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
// File: BitSet.h
//
// BitSet implementation.
// ===========================================================================

//============================================================================
//  BitSet.cs
//
//  2013/10/17
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    // (sscli)
    //
    //    A BitSet can be in one of 3 states:
    //
    //    1) Uninited: m_bits == 0. The bitset is also considered to be empty. Any
    //                 bit test operations will behave as if the set is empty.
    //    2) Small:    (m_bits & 1) != 0. The bitset is (m_bits >> 1). Note that if
    //                 m_bits == 1 then the bitset is empty.
    //    3) Large:    m_bits is a pointer to a BitSetImp.
    //
    //    Bit indices are zero based.

    //======================================================================
    // class BitSet
    //
    /// <summary>
    /// Use a list of unsigned integers as a list of bits.
    /// </summary>
    //======================================================================
    internal class BitSet
    {
        // typedef ULONG_PTR Blob;
        protected const int BLOBBITS = sizeof(uint) * 8;
        protected const int MAXBITS = ((int)(System.Int32.MaxValue / BLOBBITS)) * BLOBBITS;

        //============================================================
        //  class BitSet.MaskData
        //
        // Used to get mask and blob index information for a bit range.
        //============================================================
        protected class MaskData
        {
            internal int iblobMin;
            internal int iblobLast;
            internal uint blobMaskMin;
            internal uint blobMaskLast;

            //--------------------------------------------------------
            // BitSet.MaskData Constructor
            //--------------------------------------------------------
            internal MaskData(int ibitMin, int ibitLim)
            {
                DebugUtil.Assert(0 <= ibitMin && ibitMin < ibitLim);

                // Get iblobMin and adjust ibitMin to be relative.
                iblobMin = (int)(ibitMin / BLOBBITS);
                ibitMin -= iblobMin * BLOBBITS;
                DebugUtil.Assert(0 <= ibitMin && ibitMin < BLOBBITS);
                blobMaskMin = (~(uint)0) << ibitMin;

                // Get iblobLast and adjust ibitLim to be relative.
                iblobLast = (int)((ibitLim - 1) / BLOBBITS);
                ibitLim -= iblobLast * BLOBBITS;
                DebugUtil.Assert(0 < ibitLim && ibitLim <= BLOBBITS);
                blobMaskLast = (~(uint)0) >> (BLOBBITS - ibitLim);
                if (iblobMin == iblobLast)
                {
                    blobMaskMin &= blobMaskLast;
                    blobMaskLast = blobMaskMin;
                }
            }
        };

        //------------------------------------------------------------
        // BitSet.AndBlobChanged (static)
        //
        /// <summary>
        /// If any bits are set in blob1 but not in blob2, clear them and return true.
        /// Otherwise return false.
        /// </summary>
        /// <param name="blob1"></param>
        /// <param name="blob2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool AndBlobChanged(ref uint blob1, ref uint blob2)
        {
            if ((blob1 & ~blob2) != 0)
            {
                blob1 &= blob2;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        //  BitSet.AreBitsZero (1) (static)
        //
        /// <summary>
        /// Return true iff the blobs are all zero.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool AreBitsZero(List<uint> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        //  BitSet.AreBitsZero (2) (static)
        //
        /// <summary>
        /// Return true iff the blobs are all zero.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool AreBitsZero(List<uint> list, int start, int count)
        {
            DebugUtil.Assert(list != null);

            if (start < 0)
            {
                start = 0;
            }

            int lim = start + count;
            if (lim > list.Count)
            {
                lim = list.Count;
            }

            for (int i = 0; i < lim; ++i)
            {
                if (list[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        //------------------------------------------------------------
        //  BitSet.AreBitsOne (static)
        //
        /// <summary>
        /// Return true iff the blobs have all bits set.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool AreBitsOne(List<uint> list, int start, int count)
        {
            DebugUtil.Assert(list != null);

            if (start < 0)
            {
                return false;
            }

            int lim = start + count;
            if (lim > list.Count)
            {
                return false;
            }

            for (int i = start; i < count; ++i)
            {
                if ((~list[i]) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        //  BitSet.SetBlobs (static)
        //
        /// <summary></summary>
        /// <param name="list"></param>
        /// <param name="bits"></param>
        //------------------------------------------------------------
        static internal void SetBlobs(List<uint> list, uint bits)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                list[i] = bits;
            }
        }

        //------------------------------------------------------------
        // BitSet   Fields
        //------------------------------------------------------------
        /// <summary>
        /// null means uninited.
        /// </summary>
        private List<uint> blobList = null;

        //private int usedBitsCount = 0;

        //------------------------------------------------------------
        // BitSet   Constructors (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal BitSet() { }

        //------------------------------------------------------------
        // BitSet   Constructors (2)
        //
        /// <summary>Allocate bits for the specified size.</summary>
        /// <param name="cbit"></param>
        //------------------------------------------------------------
        internal BitSet(int cbit)
        {
            Init(cbit);
        }

        //------------------------------------------------------------
        // BitSet   Constructors (3)
        //
        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal BitSet(BitSet src)
        {
            this.blobList = new List<uint>();
            this.blobList.AddRange(src.blobList);
            //this.usedBitsCount = src.usedBitsCount;
        }

        //------------------------------------------------------------
        // BitSet.Cbit
        //
        /// <summary>
        /// Return the number of allocated bits.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int Cbit()
        {
            return this.blobList != null ? BLOBBITS * blobList.Count : 0;
        }

        //------------------------------------------------------------
        // BitSet.FInited
        //
        /// <summary>
        /// Return true if some bits are assigned.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FInited()
        {
            return (blobList != null);
        }

        //------------------------------------------------------------
        // BitSet.Init
        //
        /// <summary></summary>
        /// <param name="cbit"></param>
        //------------------------------------------------------------
        internal void Init(int cbit)
        {
            Grow(cbit);
        }

        //------------------------------------------------------------
        // BitSet.Reset
        //
        /// <summary>
        /// Release all bits.
        /// </summary>
        //------------------------------------------------------------
        internal void Reset()
        {
            this.blobList = null;
        }

        //------------------------------------------------------------
        // BitSet.Grow
        //
        /// <summary>
        /// Add bits to a given bits number.
        /// </summary>
        /// <param name="cbit"></param>
        //------------------------------------------------------------
        internal void Grow(int cbit)
        {
            DebugUtil.Assert(0 <= cbit && cbit <= MAXBITS);

            if (this.blobList == null)
            {
                this.blobList = new List<uint>();
            }

            if (cbit > Cbit())
            {
                int cbs = (cbit + BLOBBITS - 1) / BLOBBITS;
                while (blobList.Count < cbs)
                {
                    blobList.Add(0);
                }
            }
        }

        //------------------------------------------------------------
        // BitSet.IndexToPosition
        //
        /// <summary></summary>
        /// <param name="index"></param>
        /// <param name="blob"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool IndexToPosition(
            int index,
            out int blob,
            out int bit)
        {
            if (0 <= index && index <= MAXBITS)
            {
                blob = (int)(index / BLOBBITS);
                bit = index % BLOBBITS;
                return true;
            }

            blob = -1;
            bit = -1;
            return false;
        }

        //------------------------------------------------------------
        // BitSet.RangeToPositions
        //
        /// <summary>
        /// <para>Get the indice of unsigned integers and positions of bits
        /// for the specified start bit and end bit.</para>
        /// <para>If start and/or end are invalid, return false.</para>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="set1"></param>
        /// <param name="pos1"></param>
        /// <param name="set2"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool RangeToPositions(
            int startIndex,
            int endIndex,
            out int startBlob,
            out int startBit,
            out int endBlob,
            out int endBit)
        {
            startBlob = startBit = endBlob = endBit = -1;
            if (startIndex > endIndex)
            {
                return false;
            }

            if (IndexToPosition(startIndex, out startBlob, out startBit))
            {
                return IndexToPosition(endIndex, out endBlob, out endBit);
            }
            return false;
        }

        //------------------------------------------------------------
        // BitSet.TestBit (1)
        //
        /// <summary>
        /// <para>determine if the bit of j-th bit of i-th uint value is set.</para>
        /// <para>Not chech that the arguments are valid.</para>
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool TestBit(int i, int j)
        {
            if (this.blobList == null)
            {
                return false;
            }

            try
            {
                return ((blobList[i] & ((uint)1 << j)) != 0);
            }
            catch (IndexOutOfRangeException)
            {
                DebugUtil.Assert(false);
            }
            return false;
        }

        //------------------------------------------------------------
        // BitSet.TestBit (2)
        //
        /// <summary>
        /// <para>determine if the ibit-th bit is set.</para>
        /// </summary>
        /// <param name="ibit"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool TestBit(int ibit)
        {
            if (this.blobList == null)
            {
                return false;
            }

            if (ibit >= Cbit() || ibit < 0)
            {
                return false;
            }
            int i = ibit / BLOBBITS;
            int j = ibit % BLOBBITS;
            return TestBit(i, j);
        }

        //------------------------------------------------------------
        // BitSet.TestAllRange
        //
        /// <summary>
        /// Return true iff all the bits in the range [ibitMin, ibitLim) are set.
        /// If ibitLim > Cbit(), returns false.
        /// If ibitMin == ibitLim, returns true (vacuous).
        /// </summary>
        /// <param name="ibitMin"></param>
        /// <param name="ibitLim"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool TestAllRange(int ibitMin, int ibitLim)
        {
            DebugUtil.Assert(0 <= ibitMin && ibitMin <= ibitLim);

            int count = ibitLim - ibitMin;
            if (count == 1)
            {
                return TestBit(ibitMin);
            }
            if (count <= 0)
            {
                return true;    // Vacuous
            }

            if (!FInited())
            {
                return false;
            }

            MaskData md = new MaskData(ibitMin, ibitLim);
            uint blob;

            if (md.iblobLast >= this.blobList.Count)
            {
                return false;
            }

            blob = this.blobList[md.iblobMin];
            if ((blob & md.blobMaskMin) != md.blobMaskMin)
            {
                return false;
            }
            if (md.iblobMin >= md.iblobLast)
            {
                return true;
            }

            blob = this.blobList[md.iblobLast];
            if ((blob & md.blobMaskLast) != md.blobMaskLast)
            {
                return false;
            }

            return AreBitsOne(this.blobList, md.iblobMin + 1, md.iblobLast - md.iblobMin - 1);
        }

        //------------------------------------------------------------
        // BitSet.TestAnyRange
        //
        /// <summary>
        /// Returns true iff any bits in the range [ibitMin, ibitLim) are set.
        /// </summary>
        /// <param name="ibitMin"></param>
        /// <param name="ibitLim"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool TestAnyRange(int ibitMin, int ibitLim)
        {
            DebugUtil.Assert(0 <= ibitMin && ibitMin <= ibitLim);

            int count = ibitLim - ibitMin;
            if (count == 1)
            {
                return TestBit(ibitMin);
            }
            if (count <= 0)
            {
                return false;
            }

            if (!FInited())
            {
                return false;
            }

            MaskData md = new MaskData(ibitMin, ibitLim);
            uint blob;

            if (md.iblobMin >= this.blobList.Count)
            {
                return false;
            }

            blob = this.blobList[md.iblobMin];
            if ((blob & md.blobMaskMin) != 0)
            {
                return true;
            }

            if (md.iblobLast >= this.blobList.Count)
            {
                md.iblobLast = this.blobList.Count;
            }
            else
            {
                blob = this.blobList[md.iblobLast];
                if ((blob & md.blobMaskLast) != 0)
                {
                    return true;
                }
            }

            return !AreBitsZero(this.blobList, md.iblobMin + 1, md.iblobLast - md.iblobMin - 1);
        }

        //------------------------------------------------------------
        // BitSet.TestAnyBits
        //
        /// <summary>
        /// <para>Determine whether there is a set bit.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool TestAnyBits()
        {
            if (FInited())
            {
                return !AreBitsZero(this.blobList);
            }
            return false;
        }

        //------------------------------------------------------------
        // BitSet.TestAnyBits
        //
        /// <summary>
        /// Return true if there is a same bit that is set in both this and other BitSet.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool TestAnyBits(BitSet other)
        {
            if (other == null || this.blobList == null)
            {
                return false;
            }

            int setCount = (blobList.Count <= other.blobList.Count) ?
                blobList.Count : other.blobList.Count;

            for (int i = 0; i < setCount; ++i)
            {
                if ((blobList[i] & other.blobList[i]) != 0) return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // BitSet.SetBit (1)
        //
        /// <summary></summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        //------------------------------------------------------------
        private void SetBit(int i, int j)
        {
            DebugUtil.Assert(this.blobList != null);

            try
            {
                blobList[i] |= ((uint)1 << j);
            }
            catch (IndexOutOfRangeException)
            {
                DebugUtil.Assert(false);
            }
        }

        //------------------------------------------------------------
        // BitSet.SetBit (2)
        //
        /// <summary></summary>
        /// <param name="ibit"></param>
        //------------------------------------------------------------
        internal void SetBit(int ibit)
        {
            DebugUtil.Assert(ibit >= 0);

            Grow(ibit + 1);
            int i = ibit / BLOBBITS;
            int j = ibit % BLOBBITS;
            SetBit(i, j);
        }

        //------------------------------------------------------------
        // BitSet.SetBit (3)
        //
        /// <summary></summary>
        /// <param name="other"></param>
        //------------------------------------------------------------
        internal void SetBit(BitSet other)
        {
            DebugUtil.Assert(other != null);

            if (this.blobList == null || this.Cbit() < other.Cbit())
            {
                this.Grow(other.Cbit());
            }

            int count = (other.blobList != null ? other.blobList.Count : 0);

            for (int i = 0; i < count; ++i)
            {
                blobList[i] = other.blobList[i];
            }

            for (int i = count; i < this.blobList.Count; ++i)
            {
                this.blobList[i] = 0;
            }
        }

        //------------------------------------------------------------
        // BitSet.ClearBit (1)
        //
        /// <summary></summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        //------------------------------------------------------------
        internal void ClearBit(int i, int j)
        {
            DebugUtil.Assert(this.blobList != null);

            try
            {
                blobList[i] &= ~((uint)1 << j);
            }
            catch (IndexOutOfRangeException)
            {
                DebugUtil.Assert(false);
            }
        }

        //------------------------------------------------------------
        // BitSet.ClearBit (2)
        //
        /// <summary></summary>
        /// <param name="ibit"></param>
        //------------------------------------------------------------
        internal void ClearBit(int ibit)
        {
            if (ibit >= Cbit() || ibit < 0)
            {
                return;
            }

            int i = ibit / BLOBBITS;
            int j = ibit % BLOBBITS;
            ClearBit(i, j);
        }

        //------------------------------------------------------------
        //  BitSet.SetOrClearRange
        //
        /// <summary></summary>
        /// <param name="min"></param>
        /// <param name="lim"></param>
        /// <param name="setBit"></param>
        //------------------------------------------------------------
        private void SetOrClearRange(int min, int lim, bool setBit)
        {
            DebugUtil.Assert(0 <= min && min <= lim);

            int set1 = 0, pos1 = 0, set2 = 0, pos2 = 0;
            if (!RangeToPositions(min, lim - 1, out set1, out pos1, out set2, out pos2))
            {
                return;
            }
            Grow(lim);
            DebugUtil.Assert(lim <= this.Cbit());

            for (int i = set1; i <= set2; ++i)
            {
                int j1 = (i == set1) ? pos1 : 0;
                int j2 = (i == set2) ? pos2 : BLOBBITS - 1;

                for (int j = j1; j <= j2; ++j)
                {
                    if (setBit)
                    {
                        SetBit(i, j);
                    }
                    else
                    {
                        ClearBit(i, j);
                    }
                }
            }
        }

        //------------------------------------------------------------
        //  BitSet.SetBitRange
        //
        /// <summary></summary>
        /// <param name="min"></param>
        /// <param name="lim"></param>
        //------------------------------------------------------------
        internal void SetBitRange(int min, int lim)
        {
            DebugUtil.Assert(0 <= min && min <= lim);

            int set1 = 0, pos1 = 0, set2 = 0, pos2 = 0;
            if (!RangeToPositions(min, lim - 1, out set1, out pos1, out set2, out pos2))
            {
                return;
            }
            Grow(lim);
            DebugUtil.Assert(lim <= this.Cbit());

            for (int i = set1; i <= set2; ++i)
            {
                int j1 = (i == set1) ? pos1 : 0;
                int j2 = (i == set2) ? pos2 : BLOBBITS - 1;

                for (int j = j1; j <= j2; ++j)
                {
                    SetBit(i, j);
                }
            }
        }

        //------------------------------------------------------------
        //  BitSet.ClearBitRange
        //
        /// <summary></summary>
        /// <param name="min"></param>
        /// <param name="lim"></param>
        //------------------------------------------------------------
        internal void ClearBitRange(int min, int lim)
        {
            int cbit = this.Cbit();
            if (lim > cbit)
            {
                lim = cbit;
            }
            if (min >= lim)
            {
                return;
            }
            DebugUtil.Assert(0 <= min && min <= lim && lim <= cbit);

            int set1 = 0, pos1 = 0, set2 = 0, pos2 = 0;
            if (!RangeToPositions(min, lim - 1, out set1, out pos1, out set2, out pos2))
            {
                return;
            }

            for (int i = set1; i <= set2; ++i)
            {
                int j1 = (i == set1) ? pos1 : 0;
                int j2 = (i == set2) ? pos2 : BLOBBITS - 1;

                for (int j = j1; j <= j2; ++j)
                {
                    ClearBit(i, j);
                }
            }
        }

        //------------------------------------------------------------
        //  BitSet.ClearAll
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void ClearAll()
        {
            for (int i = 0; i < blobList.Count; ++i)
            {
                blobList[i] = 0;
            }
        }

        //------------------------------------------------------------
        //  BitSet.Equals
        //
        /// <summary>
        /// <para>If both are empty, return true.</para>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Equals(BitSet other)
        {
            if (this.Cbit() == 0)
            {
                return !other.TestAnyBits();
            }
            if (other.Cbit() == 0)
            {
                return !this.TestAnyBits();
            }

            // this and other are not empty.

            //int fullUsedSetCount = Cbit() / BLOBBITS;

            int count = 0;
            if (this.blobList.Count == other.blobList.Count)
            {
                count = this.blobList.Count;
            }
            else if (this.blobList.Count > other.blobList.Count)
            {
                count = other.blobList.Count;
                for (int i = count; i < this.blobList.Count; ++i)
                {
                    if (this.blobList[i] != 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                count = this.blobList.Count;
                for (int i = count; i < other.blobList.Count; ++i)
                {
                    if (other.blobList[i] != 0)
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < count; ++i)
            {
                if (blobList[i] != other.blobList[i])
                {
                    return false;
                }
            }

            return true;
        }

        //------------------------------------------------------------
        //  BitSet.Union
        //
        /// <summary></summary>
        /// <param name="other"></param>
        //------------------------------------------------------------
        internal void Union(BitSet other)
        {
            DebugUtil.Assert(other != null);

            int count = other.Cbit();
            if (other.Cbit() == 0)
            {
                return;
            }

            if (this.Cbit() < count)
            {
                this.Grow(count);
            }

            for (int i = 0; i < other.blobList.Count; ++i)
            {
                blobList[i] |= other.blobList[i];
            }
        }

        //------------------------------------------------------------
        //  BitSet.FIntersectChanged
        //
        /// <summary></summary>
        /// <param name="bset2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FIntersectChanged(BitSet bset2)
        {
            if (this.Cbit() == 0)
            {
                return false;
            }

            List<uint> list1 = this.blobList;
            List<uint> list2 = bset2.blobList;
            bool changed = false;
            int count = list1.Count <= list2.Count ? list1.Count : list2.Count;

            for (int i = count; --i >= 0; )
            {
                uint bits1 = list1[i];
                uint bits2 = list2[i];

                changed |= AndBlobChanged(ref bits1, ref bits2);

                list1[i] = bits1;
                list2[i] = bits2; ;
            }

            int dif = list1.Count - count;
            if (dif == 0 || AreBitsZero(list1, count, dif))
            {
                return changed;
            }
            for (int i = count; i < list1.Count; ++i)
            {
                list1[i] = 0;
            }
            return true;
        }


        //------------------------------------------------------------
        //  BitSet.Intersect
        //
        /// <summary>
        /// Ensures that any bits that are clear in bset are also clear in this bitset.
        /// </summary>
        /// <param name="other"></param>
        //------------------------------------------------------------
        internal void Intersect(BitSet other)
        {
            DebugUtil.Assert(other != null);

            if (this.Cbit() == 0)
            {
                return;
            }
            if (other.Cbit() == 0)
            {
                ClearAll();
            }

            int count;
            if (this.blobList.Count > other.blobList.Count)
            {
                count = other.blobList.Count;
                for (int i = count; i < this.blobList.Count; ++i)
                {
                    this.blobList[i] = 0;
                }
            }
            else
            {
                count = this.blobList.Count;
            }

            for (int i = 0; i < count; ++i)
            {
                blobList[i] &= other.blobList[i];
            }
        }

        //------------------------------------------------------------
        //  BitSet.Trash
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal void Trash() { }

#if DEBUG
        //------------------------------------------------------------
        //  BitSet.Debug
        //------------------------------------------------------------
        internal string Debug()
        {
            System.Text.StringBuilder sb = new StringBuilder();
            //string LF = "\r\n";
            string LF = " ";

            for (int i = 0; i < Cbit(); ++i)
            {
                int j = i % 10;
                if (i > 0 && j == 0) sb.Append(LF);
                sb.Append((TestBit(i) ? "1" : "0"));
            }
            return sb.ToString();
        }
#endif
    }
}
