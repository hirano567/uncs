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
// File: namemgr.h
//
// Defines the structures used to store and manage names.
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
// File: namemgr.cpp
//
// Routines for storing and handling names.
// ===========================================================================

//============================================================================
// NameMgr.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //#ifndef UInt32x32To64
    //#define UInt32x32To64(a, b) ((DWORDLONG)((DWORD)(a)) * (DWORDLONG)((DWORD)(b)))
    //#endif

    //======================================================================
    // A CNameManager stores names. An hash table is used with
    // buckets that lead to a linked list of names.
    //======================================================================
    //----------------------------------------------------------------------
    // flags to use when checking IsValidIdentifier
    //----------------------------------------------------------------------
    internal enum CheckIdentifierFlagsEnum : int
    {
        Simple = 0x00,              // No at-identifiers allowed and no keyword checking
        AllowAtIdentifiers = 0x01,  // allow at identifiers
        CheckKeywords = 0x02,       // check if identifier is a keyword (and return false if it is)
        StandardSource = AllowAtIdentifiers | CheckKeywords    // Standard source identifier
    }

    //======================================================================
    // class CNameManager
    //
    /// <summary>
    /// <para>This class stores keywords and identifiers of source texts in following tables.</para>
    /// <list type="bullet">
    /// <item><term>namesDictionary</term><description>Stores all names.</description></item>
    /// <item><term>keywordsDictionary</term><description>Stores Keywords.</description></item>
    /// <item><term>predefinedNameDictionary</term><description>Stores Defined names.</description></item>
    /// </list>
    /// <para>Does not store the preprocessor symbols.</para>
    /// </summary>
    //======================================================================
    internal class CNameManager
    {
        //------------------------------------------------------------
        // CNameManager  Fields and Properties
        //------------------------------------------------------------
        // This is the lock mechanism for thread safety.
        // NOTE:  All public operations on the name table currently acquire this lock
        // throughout the entire operation.
        // If a more granular solution is necessary, we could store an additional lock per bucket,
        // and release this 'global' lock as soon as the desired bucket head is determined
        // (locking that bucket first of course).
        // We'll not do this unless performance numbers indicate the need for it, however.

        //CTinyLock lock;
        object lockObject = new object();

        /// <summary>
        /// <para>name table</para>
        /// <para>Stores all names in source texts and assigns ID to each name.</para>
        /// </summary>
        private Dictionary<string, int> nameDictionary = new Dictionary<string, int>();
        
        // 
        /// <summary>
        /// <para>table of all keywords</para>
        /// </summary>
        private Dictionary<string, TOKENID> keywordsDictionary = new Dictionary<string, TOKENID>();
        
        /// <summary>
        /// <para>table of all predefined names</para>
        /// </summary>
        private Dictionary<string, Object> predefinedNameDictionary = new Dictionary<string, object>();

        /// <summary>
        /// <para>array with all the predefined names.</para>
        /// </summary>
        private string[] predefinedNames = null;

        /// <summary>
        /// <para>Names of preprocessor keywords</para>
        /// </summary>
        private string[] preprocessourKeywords =
        {
            null,   // UNDEFINED
            "define",
            "undef",
            "error",
            "warning",
            "if",
            "elif",
            "else",
            "endif",
            "region",
            "endregion",
            "line",
            "pragma",
            "true",
            "false",
            "hidden",
            "default",
            "disable",
            "restore",
            "checksum",
        };

        private Dictionary<string, int> preprocessorKeyWordDictionary = new Dictionary<string, int>();

        /// <summary>
        /// <para>Names of attribute targets</para>
        /// </summary>
        private string[] attributeTargets =
        {
            null,   // NONE
            "assembly",
            "module",
            "type",
            "method",
            "field",
            "property",
            "event",
            "param",
            "return",
            "typevar",
            "$AL_UNKNOWN$",
        };

        private Dictionary<string, ATTRTARGET> attributeTargetDictionary = new Dictionary<string, ATTRTARGET>();

        //------------------------------------------------------------
        // CNameManager.CreateInstance (static)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static CNameManager CreateInstance()
        {
            CNameManager manager = new CNameManager();
            DebugUtil.Assert(manager != null);
            manager.Init();
            return manager;
        }

        //------------------------------------------------------------
        // CNameManager  Constructor
        //
        /// <summary>
        /// Does nothing.
        /// </summary>
        //------------------------------------------------------------
        internal CNameManager() { }

        //------------------------------------------------------------
        // CNameManager.Init
        //
        /// <summary>
        /// Register keywords and predefined names to tables.
        /// </summary>
        //------------------------------------------------------------
        internal void Init()
        {
            AddKeywords();
            AddPredefNames();
        }

        // Do not define AddString method. Use Add and AddLen methods.

        //------------------------------------------------------------
        // CNameManager.AddString
        //
        /// <summary>
        /// <para>Register a string to dictionary.</para>
        /// <para>if the name has been already registered, return false.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool AddString(string name)
        {
            if (name == null)
            {
                return false;
            }

            if (!nameDictionary.ContainsKey(name))
            {
                nameDictionary.Add(name, CObjectID.GenerateID());
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CNameManager.AddLen
        //
        /// <summary>
        /// <para>Register a string whose start index and length are specified.</para>
        /// <para>if the name has been already registered, return false.</para>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool AddLen(string str, int start, int length)
        {
            if (str == null)
            {
                return false;
            }

            try
            {
                return AddString(str.Substring(start, length));
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            return false;
        }

        // Do not Define LookupString. Use Lookup and LookupLen methods.

        //------------------------------------------------------------
        // CNameManager.Lookup
        //
        /// <summary>
        /// Determine that the specified string is registered.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool Lookup(string name)
        {
            if (name == null)
            {
                return false;
            }

            try
            {
                return nameDictionary.ContainsKey(name);
            }
            catch (ArgumentException)
            {
                // Includes ArgumentNullException
                DebugUtil.Assert(false);
            }
            return false;
        }

        //------------------------------------------------------------
        // CNameManager.LookupLen
        //
        /// <summary>
        /// Determine that the string whose start index and length are specified is registered.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool LookupLen(string str, int start, int length)
        {
            if (str == null)
            {
                return false;
            }

            try
            {
                return Lookup(str.Substring(start, length));
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            return false;
        }

        //------------------------------------------------------------
        // CNameManager.GetNameID
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal int GetNameID(string name)
        {
            if (name == null)
            {
                return 0;   // id starts from 1;
            }

            int id = 0;
            if (this.nameDictionary.TryGetValue(name, out id))
            {
                return id;
            }

            id = CObjectID.GenerateID();
            try
            {
                this.nameDictionary.Add(name, id);
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
                return 0;
            }
            return id;
        }

        //------------------------------------------------------------
        // CNameManager.IsKeyword (1)
        //
        /// <summary>
        /// <para>Determine that the specified string is a keyword.</para>
        /// <para>This is IsNameKeyword in sscli.
        /// (In sscli, IsKeyword only calls IsNameKeyword.)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="langVer">Specifiy whether according to ECMA standard or not.</param>
        /// <param name="index">If the string is a keyword, its index is set to.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsKeyword(string name, LangVersionEnum langVer, out int index)
        {
            index = -1;
            TOKENID tid;

            if (name == null)
            {
                return false;
            }

            if (!keywordsDictionary.TryGetValue(name, out tid))
            {
                return false;
            }

            TOKINFO tok = CParser.GetTokenInfo(tid);
            if (tok == null)
            {
                return false;
            }

            if ((langVer != LangVersionEnum.ECMA1) || (tok.Flags & TOKFLAGS.F_MSKEYWORD) == 0)
            {
                index = (int)tid;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // CNameManager.IsPreprocessorKeyword
        //
        /// <summary>
        /// Determine that the specified string is a keyword of C# preprocessor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsPreprocessorKeyword(string name, out int index)
        {
            if (name != null)
            {
                if (preprocessorKeyWordDictionary.TryGetValue(name, out index))
                {
                    return true;
                }
            }
            index = -1;
            return false;
        }

        //------------------------------------------------------------
        // CNameManager.IsPreprocessorKeyword
        //
        /// <summary>
        /// Determine that the specified string is a keyword of C# preprocessor.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsPreprocessorKeyword(string name)
        {
            int i;
            return IsPreprocessorKeyword(name, out i);
        }

        //------------------------------------------------------------
        // CNameManager.IsAttributeTarget
        //
        /// <summary>
        /// Determine that the specified string is a target of attributes.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="location">If true,
        /// a value of type ATTRTARGET corresponding to the string is set to.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsAttributeTarget(string name, out ATTRTARGET location)
        {
            if (name != null)
            {
                if (this.attributeTargetDictionary.TryGetValue(name, out location))
                {
                    return true;
                }
            }
            location = ATTRTARGET.NONE;
            return false;
        }

        //------------------------------------------------------------
        // CNameManager.IsAttributeTarget
        //
        /// <summary>
        /// Determine that the specified string is a target of attributes.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsAttributeTarget(string name)
        {
            ATTRTARGET loc;
            return IsAttributeTarget(name, out loc);
        }

        //------------------------------------------------------------
        // CNameManager.GetAttributeTarget (GetAttrLoc in sscli)
        //
        /// <summary>
        /// Return the string of the target corresponding to the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetAttributeTarget(int index)
        {
            string loc = null;
            try
            {
                loc = attributeTargets[index];
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
            return loc;
        }

        //------------------------------------------------------------
        // CNameManager.KeywordName
        //
        /// <summary>
        /// Return the string of the keyword corresponding to the specified index.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string KeywordName(TOKENID keyword)
        {
            TOKINFO tok = CParser.GetTokenInfo(keyword);
            return (tok != null ? tok.Text : null);
        }

        //------------------------------------------------------------
        // CNameManager.GetPredefinedName
        //
        /// <summary>
        /// <para>Return the string of the predefined name
        /// corresponding to the specified index.</para>
        /// <para>(In sscli, GetPredefName)</para>
        /// </summary>
        /// <param name="pn"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal string GetPredefinedName(PREDEFNAME pn)
        {
            try
            {
                return predefinedNames[(int)pn];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // CNameManager.ClearNames
        //
        /// <summary>Clear all names.</summary>
        //------------------------------------------------------------
        internal void ClearNames()
        {
            lock (lockObject)
            {
                nameDictionary.Clear();
            }
        }

        //------------------------------------------------------------
        // CNameManager.ClearAll
        //
        /// <summary>
        /// Clear all names and all keywords.
        /// </summary>
        //------------------------------------------------------------
        internal void ClearAll()
        {
            ClearNames();

            lock (lockObject)
            {
                keywordsDictionary.Clear();
            }
        }

        //------------------------------------------------------------
        // CNameManager.Term
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal void Term() { }

        //------------------------------------------------------------
        // CNameManager.IsValidIdentifier (1)
        //
        /// <summary>
        /// <para>Checks for a valid identifier.
        /// The definition of a valid identifier can change depending on what flags are passed.</para>
        /// <para>Specifically,
        /// <list type="bullet">
        /// <item>If AllowAtIdentifiers is set in flags, '@' in the beginning is valid.</item>
        /// <item>Determine if an invalid character is in.</item>
        /// <item>Determine if the name is of a keyword.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="langVer">Specifiy whether according to ECMA standard or not.</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsValidIdentifier(
            string name,
            LangVersionEnum langVer,
            CheckIdentifierFlagsEnum flags)
        {
            if (name == null || name.Length == 0)
            {
                return false;
            }
            int idx = 0;

            bool atKeyword = false;
            if (name[idx] == '@')
            {
                if ((flags & CheckIdentifierFlagsEnum.AllowAtIdentifiers) == 0)
                {
                    return false;
                }
                atKeyword = true;
                if (name.Length <= 1)
                {
                    return false;
                }
                ++idx;
            }

            for (; idx < name.Length; ++idx)
            {
                if (!CharUtil.IsIdentifierCharOrDigit(name[idx]))
                {
                    return false;
                }
            }

            if ((flags & CheckIdentifierFlagsEnum.CheckKeywords) == 0 || atKeyword)
            {
                return true;
            }

            int dummy;
            return !IsKeyword(name, langVer, out dummy);
        }

        //------------------------------------------------------------
        // CNameManager.IsValidIdentifier (2)
        //
        /// <summary>
        /// Checks for a valid identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="langVer">Specifiy whether according to ECMA standard or not.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool IsValidIdentifier(string name, LangVersionEnum langVer)
        {
            return IsValidIdentifier(name, langVer, CheckIdentifierFlagsEnum.StandardSource);
        }

        //------------------------------------------------------------
        // CNameManager.GetTokenText
        //
        /// <summary>
        /// Return the string of the token.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal string GetTokenText(TOKENID tokenId)
        {
            TOKINFO tok = CParser.GetTokenInfo(tokenId);
            if (tok != null)
            {
                return tok.Text;
            }
            return null;
        }

        //------------------------------------------------------------
        // CNameManager.GetPreprocessorDirective
        //
        /// <summary>
        /// Return the index of the specified preprocessor directive.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index">The index is set to.</param>
        /// <returns>If found, return true.</returns>
        //------------------------------------------------------------
        virtual internal bool GetPreprocessorDirective(string name, out int index)
        {
            return IsPreprocessorKeyword(name, out index);
        }

        //------------------------------------------------------------
        // CNameManager.CheckCompilerVersion
        //
        /// <summary>
        /// <para>Return the compiler version.</para>
        /// <para>Set null and return true for now.</para>
        /// </summary>
        /// <param name="version"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool CheckCompilerVersion(uint version, out string error)
        {
            error = null;
            //return CFactory.StaticCheckCompilerVersion(version, error);
            // CFactory.CheckVersion always returns true;
            return true;
        }

        //------------------------------------------------------------
        // CNameManager.ValidateName
        //
        /// <summary>
        /// Checks if this name is contained within this name table
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool ValidateName(string name)
        {
            if (name == null)
            {
                return false;
            }

            try
            {
                return nameDictionary.ContainsKey(name);
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return false;
        }

#if DEBUG
        void PrintStats()
        {
            throw new NotImplementedException("CNameManager.PrintStats");
        }
#endif //DEBUG

        //------------------------------------------------------------
        // CNameManager.AddPredefNames
        //
        /// <summary>
        /// Register to predefinedNameDictionary.
        /// </summary>
        //------------------------------------------------------------
        private void AddPredefNames()
        {
            //predefinedNames = new string[CPredefinedName.InfoTable.Length];
            this.predefinedNames = CPredefinedName.InfoTable;

            for (int i = 0; i < CPredefinedName.InfoTable.Length; ++i)
            {
                //predefinedNames[i] = CPredefinedName.InfoTable[i];
                string name = this.predefinedNames[i];
                if (String.IsNullOrEmpty(name))
                {
                    continue;
                }
                AddString(name);

                try
                {
                    if (!predefinedNameDictionary.ContainsKey(name))
                    {
                        predefinedNameDictionary.Add(name, name);
                    }
                }
                catch (ArgumentException)
                {
                    DebugUtil.Assert(false);
                }
            }
        }
        //------------------------------------------------------------
        // CNameManager.AddKeywords
        //
        /// <summary>
        /// <para>Add all the keywords to the name table.
        /// To enable fast determination of whether a particular NAME is a keyword,
        /// a seperate keyword hash table is used that is indexed by the hash value of the name.
        /// At most one keyword has the same index,
        /// so we don't need a collision resolution scheme.</para>
        /// <para>
        /// <list type="bullet">
        /// <item>Get keywords form CParser,
        /// and Register them to namesDictionary and keywordsDictionary.</item>
        /// <item>Register keywords of preprocessor to namesDictionary.</item>
        /// <item>Register predefined names to namesDictionary.</item>
        /// </list>
        /// </para>
        /// </summary>
        //------------------------------------------------------------
        private void AddKeywords()
        {
            TOKINFO tok = null;

            // TOKENID.ARGS = 1 (TOKENID == 0 means UNDEFINED.)
            for (TOKENID id = TOKENID.ARGS; id < TOKENID.IDENTIFIER; ++id)
            {
                tok = CParser.GetTokenInfo(id);
                DebugUtil.Assert(tok != null);

                AddString(tok.Text);
                keywordsDictionary.Add(tok.Text, id);
            }

            for (int i = 1; i < preprocessourKeywords.Length; ++i)
            {
                string kw = preprocessourKeywords[i];
                AddString(kw);
                this.preprocessorKeyWordDictionary.Add(kw, i);

            }

            for (int i = 1; i < attributeTargets.Length; ++i)
            {
                string target = attributeTargets[i];
                AddString(target);
                attributeTargetDictionary.Add(target, (ATTRTARGET)i);
            }
        }

        //------------------------------------------------------------
        // CNameManager.AppendPossibleKeyword
        //
        /// <summary>
        /// <para>name が nameManager にまだ登録されていないなら登録する。</para>
        /// <para>name がキーワードなら name に接頭辞 '@' を付けて strinBuildere に追加する。
        /// キーワードでないならそのまま追加する。</para>
        /// <para>(In sscli, csharp\inc\strbuild.h)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stringBuilder"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AppendPossibleKeyword(string name, StringBuilder stringBuilder)
        {
            this.AddString(name);

            // Check to see if the text is a keyword
            int dummy;
            if (this.IsKeyword(name, LangVersionEnum.Default, out dummy))
            {
                stringBuilder.Append('@');
            }
            stringBuilder.Append(name);
            return true;
        }
    }

    // __forceinline unsigned HashUInt(UINT_PTR u) // moved to PCSUtility.cs


    //======================================================================
    // NAMETABLE
    //
    /// <summary>
    /// List&lt;string&gt; and methods handling it.
    /// </summary>
    //======================================================================
    internal class NAMETABLE
    {
        //------------------------------------------------------------
        // NAMETABLE Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// The defined names 
        /// </summary>
        private List<string> nameList = new List<string>();

        internal int Count
        {
            get { return nameList.Count; }
        }

        //------------------------------------------------------------
        // NAMETABLE.Define
        //
        /// <summary>
        /// if argument name is not in the list, add it.
        /// </summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        internal void Define(string name)
        {
            if (name == null)
            {
                return;
            }
            if (nameList.Contains(name))
            {
                return;
            }
            nameList.Add(name);
        }

        //------------------------------------------------------------
        // NAMETABLE.Undef
        //
        /// <summary>
        /// Remove a string from the list.
        /// </summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        internal void Undef(string name)
        {
            nameList.Remove(name);
        }

        //------------------------------------------------------------
        // NAMETABLE.IsDefined
        //
        /// <summary>
        /// Determine if argument name is in the list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool IsDefined(string name)
        {
            if (name != null)
            {
                return nameList.Contains(name);
            }
            return false;
        }

        //------------------------------------------------------------
        // NAMETABLE.ClearAll
        //
        /// <summary>
        /// Clear all strings in the list.
        /// </summary>
        //------------------------------------------------------------
        internal void ClearAll()
        {
            nameList.Clear();
        }

        //------------------------------------------------------------
        // NAMETABLE.GetAt
        //
        /// <summary>
        /// Return the string of the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string GetAt(int index)
        {
            if (index >= 0 && index < nameList.Count)
            {
                return nameList[index];
            }
            return null;
        }
    }
}
