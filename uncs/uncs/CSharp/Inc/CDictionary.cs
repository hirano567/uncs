// sscli20_20060311

// ==++==
//
//  
//   Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//  
//   The use and distribution terms for this software are contained in the file
//   named license.txt, which can be found in the root of this distribution.
//   By using this software in any fashion, you are agreeing to be bound by the
//   terms of this license.
//  
//   You must not remove this notice, or any other, from this software.
//  
//
// ==--==
// ===========================================================================
// File: table.h
//
// ===========================================================================

// ==++==
//
//  
//   Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//  
//   The use and distribution terms for this software are contained in the file
//   named license.txt, which can be found in the root of this distribution.
//   By using this software in any fashion, you are agreeing to be bound by the
//   terms of this license.
//  
//   You must not remove this notice, or any other, from this software.
//  
//
// ==--==
// ===========================================================================
// File: table.cpp
//
// ===========================================================================

//============================================================================
// CDictionary.cs
//
// 2013/10/08
//============================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // CDictionary
    //
    /// <summary>
    /// <para>Use in place of CTable in sscli.</para>
    /// <para>(sscli)
    /// This is a templatized implementation of CTableImpl.  You must use this one;
    /// you cannot instanciate a CTableImpl alone.
    /// A CTable object stores pointers to T's.  You can perform special handling on
    /// addition and removal of items by overriding OnAdd() and OnRemove().
    /// In order to be case insensitive, the caller must use the *NoCase functions
    /// to create NAME *'s, and only use those in lookups/adds.  The table will not
    /// match two names with different case, period.</para>
    /// <para>This class has a Dictionary&lt;string, CDATA&gt; instance and
    /// methods which catch and process exceptions.</para>
    /// </summary>
    //======================================================================
    internal class CDictionary<CDATA> where CDATA : class
    {
        //------------------------------------------------------------
        // CDictionary Fields and Properties
        //------------------------------------------------------------
        protected Dictionary<string, CDATA> cdataDictionary = null;

        internal Dictionary<string, CDATA> Map
        {
            get { return cdataDictionary; }
        }
        internal Dictionary<string, CDATA> Table
        {
            get { return cdataDictionary; }
        }
        internal Dictionary<string, CDATA> Dictionary
        {
            get { return cdataDictionary; }
        }

        internal int Count
        {
            get
            {
                DebugUtil.Assert(this.cdataDictionary != null);
                return cdataDictionary.Count;
            }
        }

        //------------------------------------------------------------
        // CDictionary Constructor (1)
        //
        /// <summary>
        /// Create a new dictionary.
        /// </summary>
        //------------------------------------------------------------
        internal CDictionary()
        {
            SetDictionary();
        }

        //------------------------------------------------------------
        // CDictionary Constructor (2)
        //
        /// <summary>
        /// Set a existing dictionary.
        /// </summary>
        //------------------------------------------------------------
        internal CDictionary(Dictionary<string, CDATA> dictionary)
        {
            SetDictionary(dictionary);
        }

        //------------------------------------------------------------
        // CDictionary.SetNameTable
        //
        /// <summary>Do nothing.</summary>
        /// <param name="table"></param>
        /// <returns>Always return true.</returns>
        //------------------------------------------------------------
        internal bool SetNameTable(CNameManager table)
        {
            // dummy
            return true;
        }

        //------------------------------------------------------------
        // CDictionary.SetDictionary (1)
        //
        /// <summary>
        /// Create a empty Dictionary.
        /// </summary>
        //------------------------------------------------------------
        virtual internal void SetDictionary()
        {
            this.cdataDictionary = new Dictionary<string, CDATA>();
        }

        //------------------------------------------------------------
        // CDictionary.SetDictionary (2)
        //
        /// <summary>
        /// Set an existing dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        //------------------------------------------------------------
        internal void SetDictionary(Dictionary<string, CDATA> dictionary)
        {
            this.cdataDictionary = dictionary;
        }

        //------------------------------------------------------------
        // CDictionary.Clear
        //
        /// <summary>
        /// Clear the dictionary.
        /// </summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            if (cdataDictionary != null) cdataDictionary.Clear();
        }

        //------------------------------------------------------------
        // CDictionary Indexer
        //
        /// <summary>
        /// If not found or key is invalid, return null.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal CDATA this[string key]
        {
            get
            {
                if (key != null)
                {
                    CDATA data;
                    if (this.cdataDictionary.TryGetValue(key, out data))
                    {
                        return data;
                    }
                }
                return default(CDATA);
            }
        }

        //------------------------------------------------------------
        // CDictionary.Find
        //
        /// <summary>
        /// <para>(sscli) Simple lookup.  Returns the pointer stored on the original add.</para>
        /// <para>If name is not in keys of Dictionary, return false.</para>
        /// </summary>
        /// <param name="key">Search key.</param>
        /// <param name="data">If found, the data is set to this argument.
        /// If not found, the default value of CDATA is set.</param>
        /// <returns>If found, return true.</returns>
        //------------------------------------------------------------
        virtual internal bool Find(string key, out CDATA data)
        {
            data = default(CDATA);
            if (key != null)
            {
                if (this.cdataDictionary.TryGetValue(key, out data))
                {
                    return true;
                }
            }
            return false;
        }

        // Not implement FindNoCase. Use CDictionaryNoCase.

        //------------------------------------------------------------
        // CDictionary.Contains
        //
        /// <summary>
        /// Determine whether a key already exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>If exists, return true.</returns>
        //------------------------------------------------------------
        virtual internal bool Contains(string key)
        {
            CDATA data;
            return Find(key, out data);
        }

        //------------------------------------------------------------
        // CDictionary.Add
        //
        /// <summary>
        /// <para>Simple add.  Returns S_OK if added, S_FALSE if already present</para>
        /// <para>if name already exists, return false.</para>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns>If failed to add, return false.</returns>
        //------------------------------------------------------------
        virtual internal bool Add(string key, CDATA data)
        {
            if (key != null)
            {
                try
                {
                    if (!this.cdataDictionary.ContainsKey(key))
                    {
                        cdataDictionary.Add(key, data);
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    DebugUtil.Assert(false);
                }
            }
            return false;
        }

        // Not implement AddNoCase. Use CDictionaryNoCase.

        //------------------------------------------------------------
        // CDictionary.Remove
        //
        /// <summary>
        /// Removal.  Note that there's no mechanism for removing based on data,
        /// since we have no way to search other than linearly.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>If key not found, return false.</returns>
        //------------------------------------------------------------
        virtual internal bool Remove(string key)
        {
            try
            {
                return cdataDictionary.Remove(key);
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
                return false;
            }
        }

        // Not implement RemoveNoCase. Use CDictionaryNoCase.

        //------------------------------------------------------------
        // CDictionary.CopyContents (1)
        //
        /// <summary>
        /// <para>Table enumeration is done by copying the values into an array (optionally the names as well).
        /// Caller must allocate the arrays to the appropriate size first -- assumptions are made!</para>
        ///	<para>add all data in this.cdataDictionary to dest.</para>
        ///	<para>If dup is valid, add data with duplicate keys to dup.</para>
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="dup"></param>
        //------------------------------------------------------------
        internal void CopyContents(Dictionary<string, CDATA> dest, Dictionary<string, CDATA> dup)
        {
            if (dest == null)
            {
                return;
            }

            foreach (KeyValuePair<string, CDATA> kv in cdataDictionary)
            {
                string key = kv.Key;
                if (!dest.ContainsKey(key))
                {
                    dest.Add(key, kv.Value);
                }
                else
                {
                    if (dup != null && !dup.ContainsKey(key))
                    {
                        dup.Add(key, kv.Value);
                    }
                }
            }
        }

        //------------------------------------------------------------
        // CDictionary.CopyContents (2)
        //
        /// <summary>
        ///	<para>add all data in this.cdataDictionary to dest.</para>
        ///	<para>Not add data with duplicate keys to dup.</para>
        /// </summary>
        /// <param name="dest"></param>
        //------------------------------------------------------------
        internal void CopyContents(Dictionary<string, CDATA> dest)
        {
            CopyContents(dest, null);
        }

        //------------------------------------------------------------
        // CDictionary.CopyContentsFrom (1)
        //
        /// <summary>
        /// <para>Use this to copy the contents from the given table to this table</para>
        ///	<para>Add all date in other.cdataDictionary to this.cdataDictionary.</para>
        ///	<para>If dup is valid, add data with duplicate keys to dup.</para>
        /// </summary>
        /// <param name="other"></param>
        /// <param name="dup"></param>
        //------------------------------------------------------------
        virtual internal void CopyContentsFrom(CDictionary<CDATA> other, Dictionary<string, CDATA> dup)
        {
            if (other.cdataDictionary == null)
            {
                return;
            }
            other.CopyContents(this.cdataDictionary, dup);
        }

        //------------------------------------------------------------
        // CDictionary.CopyContentsFrom (2)
        //
        /// <summary>
        /// <para>Use this to copy the contents from the given table to this table</para>
        ///	<para>Add all date in other.cdataDictionary to this.cdataDictionary.</para>
        ///	<para>Not add data with duplicate keys.</para>
        /// </summary>
        /// <param name="other"></param>
        //------------------------------------------------------------
        internal void CopyContentsFrom(CDictionary<CDATA> other)
        {
            if (other.cdataDictionary == null)
            {
                return;
            }
            other.CopyContents(this.cdataDictionary, null);
        }

        //------------------------------------------------------------
        // CDictionary.DuplicateDictionary
        //
        /// <summary>
        ///	<para>Duplicate this.cdataDictionary and set to destDic.</para>
        /// </summary>
        /// <param name="destDic"></param>
        //------------------------------------------------------------
        virtual internal void DuplicateDictionary(out Dictionary<string, CDATA> destDic)
        {
            destDic = new Dictionary<string, CDATA>(this.cdataDictionary);
        }

        // Override these to perform special handling on add/remove

        //------------------------------------------------------------
        // CDictionary.OnAdd
        //
        /// <summary></summary>
        /// <param name="pData"></param>
        //------------------------------------------------------------
        virtual internal void OnAdd(CDATA pData) { }

        //------------------------------------------------------------
        // CDictionary.OnRemove
        //
        /// <summary></summary>
        /// <param name="pData"></param>
        //------------------------------------------------------------
        virtual internal void OnRemove(CDATA pData) { }
    }

    //======================================================================
    // CDictionaryNoCase
    //
    /// <summary>
    /// <para>Has Dictionary&lt;string, CDATA&gt; and methods to handle the Dictionary.</para>
    /// <para>Ignore case when compare keys.</para>
    /// </summary>
    //======================================================================
    internal class CDictionaryNoCase<CDATA> : CDictionary<CDATA> where CDATA : class
    {
        //------------------------------------------------------------
        // CDictionaryNoCase Constructor
        //------------------------------------------------------------
        internal CDictionaryNoCase()
        {
            SetDictionary();
        }

        //------------------------------------------------------------
        // CDictionary.SetDictionary (2)
        //
        /// <summary>
        /// Create a empty Dictionary.
        /// </summary>
        //------------------------------------------------------------
        override internal void SetDictionary()
        {
            cdataDictionary = new Dictionary<string, CDATA>(new StringEqualityComparerIgnoreCase());
        }

        //------------------------------------------------------------
        // CDictionaryNoCase.DuplicateDictionary
        //
        /// <summary>
        ///	<para>Duplicate this.cdataDictionary and set to destDic.</para>
        /// </summary>
        /// <param name="destDic"></param>
        //------------------------------------------------------------
        override internal void DuplicateDictionary(out Dictionary<string, CDATA> destDic)
        {
            destDic = new Dictionary<string, CDATA>(
                this.cdataDictionary,
                new StringEqualityComparerIgnoreCase());
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //// CAutoRefTable
    ////
    //// A derivation of CTable that automatically calls AddRef()/Release() through
    //// the data stored in it.
    //
    //template <class T>
    //class CAutoRefTable : internal CTable<T>
    //{
    //internal:
    //    CAutoRefTable (ICSNameTable *pTable) : CTable<T> (pTable) {}
    //    ~CAutoRefTable() { this->RemoveAll(); }   // Must do this to ensure our override of OnRemove is called
    //    void    OnAdd (T *pData) { pData->AddRef(); }
    //    void    OnRemove (T *pData) { pData->Release(); }
    //};

    //======================================================================
    // CAutoDelTable
    //
    // A derivation of CTable that automatically deletes the data when removed.  Use
    // this only for data that is allocated with 'new'!
    //
    // do not have to define this class in .Net.
    //======================================================================
    //internal class CAutoDelTable<CDATA> : CTable<CDATA> where CDATA : class
    //{
    //}

    //======================================================================
    // CIdentTable
    //
    /// <summary>
    /// <para>This is a simple table of names, keyed by themselves.</para>
    /// <para>Derives from CTable&lt;string&gt;.</para>
    /// </summary>
    //======================================================================
    internal class CIdentDictionary : CDictionary<string>
    {
    }
}

