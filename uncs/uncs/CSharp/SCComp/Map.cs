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
// File: map.h
//
// Map for #line PREPROCs
// and #PREPROC warnings
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
// File: map.cpp
//
// Map for #line PREPROCs
// ===========================================================================

//============================================================================
// SCComp_Map.cs
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
    // class MAPABLE
    //
    /// <summary>
    /// This class relates a PREPROC instance to a line.
    /// </summary>
    //======================================================================
    internal class MAPABLE<PREPROC> where PREPROC : ICOPYFROM<PREPROC>, new()
    {
        internal PREPROC proc = new PREPROC();
        internal int mapLine = 0;

        //------------------------------------------------------------
        // MAPABLE.CopyFrom
        //------------------------------------------------------------
        internal void CopyFrom(MAPABLE<PREPROC> src)
        {
            this.mapLine = src.mapLine;
            this.proc.CopyFrom(src.proc);
        }
    }

    //======================================================================
    // class PPLINE
    //
    /// <summary>
    /// This class stores a #line directive.
    /// </summary>
    //======================================================================
    internal class PPLINE : ICOPYFROM<PPLINE>
    {
        /// <summary>
        /// true if #line default
        /// </summary>
        internal bool DefaultLine = true;

        /// <summary>
        /// <para>New line index.</para>
        /// <para>-1 means hidden.</para>
        /// </summary>
        internal int NewLineIndex = 0;

        internal string FileName = null;

        //------------------------------------------------------------
        // PPLINE.CopyFrom
        //------------------------------------------------------------
        public void CopyFrom(PPLINE src)
        {
            this.DefaultLine = src.DefaultLine;
            this.NewLineIndex = src.NewLineIndex;
            this.FileName = src.FileName;
        }
    }

    //======================================================================
    // PPWARNING
    //======================================================================
    internal class PPWARNING : ICOPYFROM<PPWARNING>
    {
        /// <summary>
        /// true for #PREPROC warning disable, false for #PREPROC warning restore
        /// </summary>
        internal bool isDisable = false;

        // INVARIANT(!bDssable || cntWarnings > 0)
        //internal ushort cntWarnings = 0;

        // INVARIANT(lengthof(WarningList) == cntWarnings)
        internal List<int> WarningList = null;

        internal int WarningCount
        {
            get { return (WarningList != null ? WarningList.Count : 0); }
        }

        //------------------------------------------------------------
        // PPWARNING.CopyFrom
        //------------------------------------------------------------
        public void CopyFrom(PPWARNING src)
        {
            this.isDisable = src.isDisable;
            this.WarningList = new List<int>(src.WarningList);
        }
    }

    //======================================================================
    // XMLMAP
    //======================================================================
    internal class XMLMAP : ICOPYFROM<XMLMAP>
    {
        internal int srcLine = 0;
        internal int colAdjust = 0;

        //------------------------------------------------------------
        // XMLMAP.CopyFrom
        //------------------------------------------------------------
        public void CopyFrom(XMLMAP src)
        {
            this.srcLine = src.srcLine;
            this.colAdjust = src.colAdjust;
        }
    }

    //======================================================================
    // CMapBase
    //
    /// <summary>
    /// This class holds and handles a List<MAPABLE<PREPROC>> instance.
    /// </summary>
    //======================================================================
    internal class CMapBase<PREPROC> where PREPROC : ICOPYFROM<PREPROC>, new()
    {
        //------------------------------------------------------------
        //  CMapBase    Fields and Properties
        //------------------------------------------------------------
        protected List<MAPABLE<PREPROC>> mapList = new List<MAPABLE<PREPROC>>();

        internal int Count
        {
            get { return mapList.Count; }
        }
        internal bool IsEmpty
        {
            get { return (mapList.Count == 0); }
        }

        //------------------------------------------------------------
        //  CMapBase    Constructor
        //------------------------------------------------------------
        internal CMapBase() { }

        //------------------------------------------------------------
        // CMapBase.Clear
        //
        /// <summary>
        /// Frees memory and does other cleanup
        /// </summary>
        //------------------------------------------------------------
        virtual internal void Clear()
        {
            this.mapList.Clear();
        }

        //------------------------------------------------------------
        // CMapBase.GetCountBefore
        //
        /// <summary></summary>
        /// <param name="beforeLine"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetCountBefore(int beforeLine)
        {
            return FindClosestIndexBefore(beforeLine) + 1;
        }

        //------------------------------------------------------------
        // CMapBase.GetLastMapLine
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int GetLastMapLine()
        {
            SortList();
            if (mapList == null || mapList.Count == 0)
            {
                return -1;
            }
            return mapList[mapList.Count - 1].mapLine;
        }

        //------------------------------------------------------------
        // CMapBase.Copy (1)
        //
        /// <summary>
        /// Copy PREPROC instance from oldMap to this.
        /// </summary>
        //------------------------------------------------------------
        internal void Copy(CMapBase<PREPROC> oldMap, int startLine, int endLine, int delta)
        {
            if (startLine > endLine)
            {
                return;
            }

            foreach (MAPABLE<PREPROC> elm in oldMap.mapList)
            {
                if (elm.mapLine >= startLine && elm.mapLine <= endLine)
                {
                    MAPABLE<PREPROC> newElm = new MAPABLE<PREPROC>();
                    newElm.CopyFrom(elm);
                    this.mapList.Add(newElm);
                }
            }
            SortList();
        }

        //------------------------------------------------------------
        // CMapBase.Copy (2)
        //
        /// <summary>
        /// Copy PREPROC instance from oldMap to this.
        /// </summary>
        //------------------------------------------------------------
        internal void Copy(CMapBase<PREPROC> oldMap, int startLine, int endLine)
        {
            Copy(oldMap, startLine, endLine, 0);
        }

        //------------------------------------------------------------
        // CMapBase.Copy (3)
        //
        /// <summary>
        /// Copy PREPROC instance from oldMap to this.
        /// </summary>
        //------------------------------------------------------------
        internal void Copy(CMapBase<PREPROC> oldMap, int startLine)
        {
            Copy(oldMap, startLine, System.Int32.MaxValue, 0);
        }

        //------------------------------------------------------------
        // CMapBase.Copy (4)
        //
        /// <summary>
        /// Copy PREPROC instance from oldMap to this.
        /// </summary>
        //------------------------------------------------------------
        internal void Copy(CMapBase<PREPROC> oldMap)
        {
            foreach (MAPABLE<PREPROC> elm in oldMap.mapList)
            {
                MAPABLE<PREPROC> newElm = new MAPABLE<PREPROC>();
                newElm.CopyFrom(elm);
                this.mapList.Add(newElm);
            }
            SortList();
        }

        //------------------------------------------------------------
        // CMapBase.Remove (1)
        //
        // (sscli)Does not free any memory or do any cleanup!!!
        /// <summary></summary>
        /// <param name="startLine"></param>
        /// <param name="endLine"></param>
        //------------------------------------------------------------
        internal void Remove(int startLine, int endLine)
        {
            SortList();

            int startIndex = FindClosestIndexAfter(startLine);
            if (startIndex == -1 || mapList[startIndex].mapLine > endLine)
            {
                return;
            }

            int endIndex = FindClosestIndexBefore(endLine);
            if (endIndex == -1 || mapList[endIndex].mapLine <= startLine)
            {
                //VSFAIL("Bad map");
                return;
            }

            for (int i = startIndex; i <= endIndex; ++i)
            {
                mapList[i] = null;
            }

            foreach (MAPABLE<PREPROC> m in mapList)
            {
                if (m == null)
                {
                    mapList.Remove(m);
                }
            }

#if DEBUG
            //memset( &m_array[m_iLast + 1], 0, sizeof(m_array[0]) * (m_iCount - m_iLast - 1));
#endif
        }

        //------------------------------------------------------------
        // CMapBase.Remove (2)
        //
        /// <summary></summary>
        /// <param name="startLine"></param>
        //------------------------------------------------------------
        internal void Remove(int startLine)
        {
            Remove(startLine, System.Int32.MaxValue);
        }

        //------------------------------------------------------------
        // CMapBase.AppendMap (1)
        //
        /// <summary></summary>
        /// <param name="data"></param>
        //------------------------------------------------------------
        protected void AppendMap(MAPABLE<PREPROC> data)
        {
            if (data == null)
            {
                return;
            }
            this.mapList.Add(data);
            IsSorted = false;
        }

        //------------------------------------------------------------
        // CMapBase.AppendMap (2)
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <param name="proc"></param>
        //------------------------------------------------------------
        protected void AppendMap(int mapLine, PREPROC proc)
        {
            MAPABLE<PREPROC> data = new MAPABLE<PREPROC>();
            data.proc.CopyFrom(proc);
            data.mapLine = mapLine;
            this.mapList.Add(data);
            IsSorted = false;
        }

        //------------------------------------------------------------
        // CMapBase.FindIndex
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int FindIndex(int mapLine)
        {
            SortList();
            int i = this.FindClosestIndexBefore(mapLine);
            if (i >= 0)
            {
                return i;
            }
            return -1;
        }

        //------------------------------------------------------------
        // CMapBase.FindClosestIndexBefore
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int FindClosestIndexBefore(int mapLine)
        {
            if (mapList.Count == 0)
            {
                return -1;
            }
            SortList();

            if (mapList.Count <= 12)
            {
                return FindClosestIndexBeforeLinear(mapLine, 0, mapList.Count - 1);
            }
            else
            {
                return FindClosestIndexBeforeBSearch(mapLine, 0, mapList.Count - 1);
            }
        }

        //------------------------------------------------------------
        // CMapBase.FindClosestIndexBeforeLinear
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int FindClosestIndexBeforeLinear(int mapLine, int min, int max)
        {
            for (int i = max; i >= min; --i)
            {
                if (mapList[i].mapLine <= mapLine)
                {
                    return i;
                }
            }
            return -1;
        }

        //------------------------------------------------------------
        // CMapBase.FindClosestIndexBeforeBSearch
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int FindClosestIndexBeforeBSearch(int mapLine, int min, int max)
        {
            if (max - min <= 12)
            {
                return FindClosestIndexBeforeLinear(mapLine, min, max);
            }

            int mid = (max - min) / 2 + min;
            if (mapList[mid].mapLine > mapLine)
            {
                return FindClosestIndexBeforeBSearch(mapLine, min, mid);
            }
            else if (mapList[mid].mapLine < mapLine)
            {
                return FindClosestIndexBeforeBSearch(mapLine, mid, max);
            }
            else
            {
                return mid;
            }
        }

        //------------------------------------------------------------
        // CMapBase.FindClosestIndexAfter
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int FindClosestIndexAfter(int mapLine)
        {
            SortList();
            if (mapList.Count <= 0)
            {
                return -1;
            }
            int ie = mapList[mapList.Count - 1].mapLine;

            if (mapList.Count <= 12)
            {
                return FindClosestIndexAfterLinear(mapLine, 0, ie);
            }
            else
            {
                return FindClosestIndexAfterBSearch(mapLine, 0, ie);
            }
        }

        //------------------------------------------------------------
        // CMapBase.FindClosestIndexAfterLinear
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int FindClosestIndexAfterLinear(int mapLine, int min, int max)
        {
            for (int i = min; i <= max; ++i)
            {
                if (mapList[i].mapLine >= mapLine)
                {
                    return i;
                }
            }
            return -1;
        }

        //------------------------------------------------------------
        // CMapBase.FindClosestIndexAfterBSearch
        //
        /// <summary></summary>
        /// <param name="mapLine"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int FindClosestIndexAfterBSearch(int mapLine, int min, int max)
        {
            if (max - min <= 12)
            {
                return FindClosestIndexAfterLinear(mapLine, min, max);
            }

            int mid = (max - min) / 2 + min;
            if (mapList[mid].mapLine > mapLine)
            {
                return FindClosestIndexAfterBSearch(mapLine, min, mid);
            }
            else if (mapList[mid].mapLine < mapLine)
            {
                return FindClosestIndexAfterBSearch(mapLine, mid, max);
            }
            else
            {
                return mid;
            }
        }

        //------------------------------------------------------------
        // CMapBase Sort the list by MAPABLE.mapLine.
        //------------------------------------------------------------
        protected bool IsSorted = false;

        protected class CompareMap : IComparer<MAPABLE<PREPROC>>
        {
            public int Compare(MAPABLE<PREPROC> m1, MAPABLE<PREPROC> m2)
            {
                return (m1.mapLine - m2.mapLine);
            }
        }

        protected void SortList()
        {
            if (IsSorted == true)
            {
                return;
            }
            mapList.Sort(new CompareMap());
            IsSorted = true;
        }
    }

    //======================================================================
    // CLineMap
    //
    /// <summary>
    /// <para>Map of {Line # in source file => PPLINE entry}
    /// Whenever a #line directive is encountered in a source file, we add an entry
    /// to the map.
    /// <list type="bullet">
    /// <item>
    /// <term>#line default:</term>
    /// <description>Add an entry (curLine, &lt;curLine + 1&gt;, NULL)</description>
    /// </item>
    /// <item>
    /// <term>#line hidden :</term>
    /// <description>Add an entry (curLine, -1, NULL)</description>
    /// </item>
    /// <item>
    /// <term>#line &lt;filename&gt; &lt;line#&gt;:</term>
    /// <description>Add an entry (curLine, &lt;line#&gt;, &lt;filename&gt;)</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// This class holds the informations of #line directives in mapList field
    /// of type List&lt;MAPABLE&lt;PREPROC&gt;&gt;
    /// </para>
    /// </summary>
    //======================================================================
    internal class CLineMap : CMapBase<PPLINE>
    {
        //------------------------------------------------------------
        // CLineMap Constructor
        //------------------------------------------------------------
        internal CLineMap() : base() {}

        //------------------------------------------------------------
        // CLineMap.AddMap
        //
        /// <summary></summary>
        /// <param name="oldLine"></param>
        /// <param name="defaultLine"></param>
        /// <param name="newLine"></param>
        /// <param name="file"></param>
        //------------------------------------------------------------
        internal void AddMap(int oldLine, bool defaultLine, int newLine, string file)
        {
            PPLINE ppdata = new PPLINE();
            ppdata.DefaultLine = defaultLine;
            ppdata.NewLineIndex = newLine;
            ppdata.FileName = file;
            this.AppendMap(oldLine, ppdata);
        }

        //------------------------------------------------------------
        // CLineMap.HideLines
        //
        /// <summary></summary>
        /// <param name="oldLine"></param>
        //------------------------------------------------------------
        internal void HideLines(int oldLine)
        {
            AddMap(oldLine, false, -1, null);
        }

        //------------------------------------------------------------
        // CLineMap.Map (1)
        //
        /// <summary>
        /// <para>Search a PPLINE instance for a specified old line number
        /// and return its data.</para>
        /// </summary>
        /// <param name="oldLine"></param>
        /// <param name="fileName"></param>
        /// <param name="isHidden"></param>
        /// <param name="isMapped"></param>
        /// <returns>If found, return a new line number. Otherwise, return old line number.</returns>
        //------------------------------------------------------------
        internal int Map(int oldLine, out string fileName, out bool isHidden, out bool isMapped)
        {
            fileName = null;
            isHidden = false;
            isMapped = false;

            if (mapList == null)
            {
                return oldLine;
            }
            else
            {
                return InternalMap(oldLine, out fileName, out isHidden, out isMapped);
            }
        }

        //------------------------------------------------------------
        // CLineMap.Map (2)
        //
        /// <summary>
        /// <para>Search a PPLINE instance for the line number of pos, and return its data.</para>
        /// <para>Set the line found to pos.</para>
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="fileName"></param>
        /// <param name="isHidden"></param>
        /// <param name="isMapped"></param>
        //------------------------------------------------------------
        internal void Map(POSDATA pos, out string fileName, out bool isHidden, out bool isMapped)
        {
            fileName = null;
            isHidden = false;
            isMapped = false;

            if (mapList != null)
            {
                pos.LineIndex =
                    InternalMap(pos.LineIndex, out fileName, out isHidden, out isMapped);
            }
        }

        //------------------------------------------------------------
        // CLineMap.ContainsEntry
        //
        /// <summary></summary>
        /// <param name="startLine"></param>
        /// <param name="endLine"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ContainsEntry(int startLine, int endLine)
        {
            int idx1 = FindClosestIndexBefore(endLine);
            if (idx1 >= 0 && mapList[idx1].mapLine >= startLine)
            {
                return true;
            }

            int idx2 = FindClosestIndexAfter(startLine);
            if (idx2 >= 0 && mapList[idx2].mapLine <= endLine)
            {
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CLineMap.RemoveFrom
        //
        /// <summary></summary>
        /// <param name="iStartLine"></param>
        //------------------------------------------------------------
        internal void RemoveFrom(int iStartLine)
        {
            int lastLine = GetLastMapLine();
            if (lastLine >= 0)
            {
                Remove(iStartLine, lastLine);
            }
        }

        //------------------------------------------------------------
        // CLineMap.ApplyDelta
        //
        /// <summary></summary>
        /// <param name="startLine"></param>
        /// <param name="deltaLine"></param>
        //------------------------------------------------------------
        internal void ApplyDelta(int startLine, int deltaLine)
        {
            MAPABLE<PPLINE> ppline;

            int idx = FindClosestIndexAfter(startLine);
            if (idx >= 0)
            {
                for (int i = idx; i < mapList.Count; i++)
                {
                    ppline = mapList[i];
                    ppline.mapLine += deltaLine;
                    if (ppline.proc.DefaultLine)
                    {
                        ppline.proc.NewLineIndex += deltaLine;
                    }
                }
            }
        }

        //------------------------------------------------------------
        // CLineMap.InternalMap
        //
        /// <summary>
        /// <para>InternalMap - the real work horse, maps an oldLine to a new line and filename
        /// Faster if ppFilename is NULL because it doesn't find the mapped filename
        /// also if there is no filename specified in a previous mapping it
        /// does NOT change the value of ppFilename</para>
        /// <para>Search a PPLINE instance for the line number of pos, and return its data.</para>
        /// <para>Set the line found to pos.</para>
        /// </summary>
        /// <param name="oldLine"></param>
        /// <param name="fileName"></param>
        /// <param name="isHidden"></param>
        /// <param name="isMapped"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private int InternalMap(int oldLine, out string fileName, out bool isHidden, out bool isMapped)
        {
            fileName = null;
            isHidden = false;
            isMapped = false;

            int index = FindClosestIndexBefore(oldLine);
            // if no #line is before the specified line, return oldLine.
            if (index == -1)
            {
                return oldLine;
            }

            // Are we asking for the line that had the "#line"?
            // If the line of oldLine is #line directive, examine #line directives before. 
            if (oldLine == mapList[index].mapLine)
            {
                // backup to the previous one if it exists
                if (index > 0)
                {
                    index--;
                }
                else
                {
                    // If it doesn't, then the line must be unmapped
                    // index == 0 means that this line is the first #line directive.
                    // Therefore, this line is not mapped and return oldLine.
                    return oldLine;
                }
            }

            // newLine == -1 means this section is 'hidden'
            if (mapList[index].proc.NewLineIndex == -1)
            {
                isHidden = true;
                // Backup to get the previous line mapping for the correct line #
                while (mapList[index].proc.NewLineIndex == -1)
                {
                    if (index == 0)
                    {
                        // If a previous one doesn't exist, then the line must be unmapped (hidden, but unmapped)
                        return oldLine;
                    }
                    index--;
                }
            }

            // Calculate the new line number
            int newLine = (oldLine - (mapList[index].mapLine + 1)) + mapList[index].proc.NewLineIndex;
            if (newLine < 0)
            {
                return oldLine;
            }

            // Now get the filename in effect for this section
            while (index > 0 && mapList[index].proc.FileName == null)
            {
                index--;
            }
            if (index >= 0)
            {
                fileName = mapList[index].proc.FileName;
            }

            if (!mapList[index].proc.DefaultLine)
            {
                isMapped = true;
            }

            return newLine;
        }
    }

    //======================================================================
    // class CWarningMap
    //======================================================================
    internal class CWarningMap : CMapBase<PPWARNING>
    {
        //------------------------------------------------------------
        // CWarningMap.Clear
        //------------------------------------------------------------
        override internal void Clear()
        {
            this.mapList.Clear();
        }

        //------------------------------------------------------------
        // CWarningMap.AddWarning
        //
        /// <summary>
        /// <para>Adds a #pragma warning to the end of the list.</para>
        /// <para>ASSERTs if a warning for the given line already exists.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void AddWarning(int srcLine, bool isDisable, List<int> WarningList)
        {
            MAPABLE<PPWARNING> map = new MAPABLE<PPWARNING>();
            map.mapLine = srcLine;
            map.proc.isDisable = isDisable;
            map.proc.WarningList = WarningList;

            this.AppendMap(map);
        }

        //------------------------------------------------------------
        // CWarningMap.IsWarningDisabled
        //
        /// <summary>
        /// Checks to see if the warning is restored anywhere before the end,
        /// and disabled anywhere before the start.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsWarningDisabled(int number, int startLine, int endLine)
        {
            int i = FindClosestIndexBefore(endLine);
            // There's no pragmas anywhere before the endLine, so the warning can't be disabled!
            if (i < 0)
            {
                return false;
            }

            while (i >= 0)
            {
                if (mapList[i].proc.isDisable)
                {
                    if (mapList[i].mapLine > startLine)
                    {
                        continue;
                        // Doesn't matter if it was disabled in the middle of the range
                    }
                    // mapList[i].mapLine <= startLine
                    if (mapList[i].proc.WarningList.Count == 0)
                    {
                        // This means disable everything, so the warning is disabled
                        return true;
                    }
                    else
                    {
                        // If argument number is in WarningList, return true.
                        for (int j = 0; j < mapList[i].proc.WarningList.Count; j++)
                        {
                            // linear (but sorted) search because we expect these lists to be short
                            if (mapList[i].proc.WarningList[j] == number)
                            {
                                return true;
                            }
                            else if (mapList[i].proc.WarningList[j] > number)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (mapList[i].proc.WarningList.Count == 0)
                    {
                        // this means restore everything, so the warning is enabled
                        return false;
                    }
                    else
                    {
                        for (int j = 0; j < mapList[i].proc.WarningList.Count; j++)
                        {
                            // linear (but sorted) search because we expect these lists to be short
                            if (mapList[i].proc.WarningList[j] == number)
                            {
                                return false;
                            }
                            else if (mapList[i].proc.WarningList[j] > number)
                            {
                                break;
                            }
                        }
                    }
                }
                --i;
            }
            return false;
        }

        //------------------------------------------------------------
        // CWarningMap.IsWarningChanged
        //
        /// <summary>
        /// Checks to see if the warnings are restored or disabled anywhere between the start and end (inclusive).
        /// Assumes that numbers is sorted (little-to-big) warning numbers and terminated with 0.
        /// </summary>
        //------------------------------------------------------------
        internal bool IsWarningChanged(int[] number, int startLine, int endLine)
        {
            int index = FindClosestIndexAfter(startLine);

            // there's no map after the startLine, or it's after the endLine!!!
            if (index == -1 || mapList[index].mapLine > endLine)
            {
                return false;
            }
            int ie = mapList.Count - 1;

            while (index <= ie && mapList[index].mapLine <= endLine)
            {
                if (mapList[index].proc.WarningList.Count == 0)
                {
                    return true;
                }

                // linear (but sorted) search because we expect these lists to be short
                foreach (CSCERRID j in mapList[index].proc.WarningList)
                {
                    foreach (CSCERRID k in number)
                    {
                        if (j == k) return true;
                    }
                }
                index++;
            }
            return false;
        }
    }

    //======================================================================
    // class CXMLMap
    //======================================================================
    internal class CXMLMap : CMapBase<XMLMAP>
    {
        //internal:
        //internal CXMLMap() : base(null) {};
        //internal CXMLMap(MEMHEAP *allocator) : CMapBase<XMLMAP>(allocator) {};
        //internal ~CXMLMap() { Clear(); }

        //internal void Init(MEMHEAP * allocator) { m_allocator = allocator; }

        //------------------------------------------------------------
        // CXMLMap.AddMap
        //
        // AddMap - adds a mapping from xmlLine to srcLine (plus a column offset)
        // Assuming there is sufficient memory, this always succeeds
        // ASSERTs if a map for the given line already exists
        //------------------------------------------------------------
        internal void AddMap(int xmlLine, int srcLine, int colAdjust)
        //void CXMLMap::AddMap(long xmlLine, long srcLine, long colAdjust)
        {
            throw new NotImplementedException("CXMLMap.AddMap");
            //XMLMAP data = {srcLine, colAdjust};
            //CMapBase<XMLMAP, 10>::AppendMap(xmlLine, data);
        }

        internal POSDATA Map(int xmlLine, int xmlColumn)
        {
            int index = FindIndex(xmlLine);
            if (index == -1)
                return new POSDATA(); // Mapping a line that somehow has no info!!!!
            else
                return new POSDATA(
                    mapList[index].proc.srcLine,
                    xmlColumn + mapList[index].proc.colAdjust);
        }
        new internal int Count()
        {
            return mapList.Count;
        }
    }
}
