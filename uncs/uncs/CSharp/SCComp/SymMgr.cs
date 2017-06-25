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
// File: symmgr.h
//
// Defines the symbol manager, which manages the storage and lookup of symbols
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
// File: symmgr.cpp
//
// Routines for storing and handling symbols.
// ===========================================================================

//============================================================================
// SymMgr.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
//#define PROP_TO_FIELD

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Uncs
{
    //======================================================================
    // class PredefTypeNameInfo
    //
    /// <summary>
    /// <para>Information for quickly checking
    /// whether a type name is the name of a predefined type.</para>
    /// <para>This is used by BSYMMGR::FPreLoad.</para>
    /// <para>(CSharp\SCComp\SymMgr.cs)</para>
    /// </summary>
    //======================================================================
    internal class PredefTypeNameInfo
    {
        //------------------------------------------------------------
        // PredefTypeNameInfo   Fields and Properties
        //------------------------------------------------------------
        internal NSSYM NamespaceSym = null;
        internal string Name = null;

        //------------------------------------------------------------
        // PredefTypeNameInfo Constructor (1)
        //------------------------------------------------------------
        internal PredefTypeNameInfo() { }

        //------------------------------------------------------------
        // PredefTypeNameInfo Constructor (2)
        //------------------------------------------------------------
        internal PredefTypeNameInfo(NSSYM ns, string name)
        {
            this.NamespaceSym = ns;
            this.Name = name;
        }

        //------------------------------------------------------------
        // PredefTypeNameInfo.Equals (override)
        //
        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public override bool Equals(object obj)
        {
            PredefTypeNameInfo other = obj as PredefTypeNameInfo;

            if (other != null)
            {
                return (
                    this.Name == other.Name &&
                    this.NamespaceSym == other.NamespaceSym);
            }
            return false;
        }

        //------------------------------------------------------------
        // PredefTypeNameInfo.GetHashCode (override)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override int GetHashCode()
        {
            return unchecked(
                (this.NamespaceSym != null ? this.NamespaceSym.GetHashCode() : 0) ^
                (this.Name != null ? this.Name.GetHashCode() : 0)) & 0x7FFFFFFF;
        }
    }

    // class PredefTypeInfo and PredefTypeInfoTable[] (predefTypeInfo[]) are defined in CSharp\Inc\PredefType.cs

    // The EE and type state:
    //
    // TYPESYMs and TypeArrays have two pieces of information that track their state for the EE.
    // One is whether the object is "Unresolved". The second is whether the object is "Dirty".
    // U(x) represents whether x is "Unresolved", D(x) represents whether x is "Dirty".
    //
    // * U(TypeArray ta) = Sum(U(T) : T in ta)
    // * U(AGGTYPESYM ats) = U(ats.agg) + U(ats.typeArgsAll)
    // * U(T*) = U(T) // T* represents any derived type (array, pointer, etc).
    // * U(TYVARSYM var) = var.parent.isAgg && U(var.parent)
    //
    // Note that U(T) is immutable for all TYPESYMs and TypeArrays. Resolving an unresolved type sym
    // produces a new type sym - it does not morph an existing type sym. Each TYPESYM and TypeArray
    // caches the last thing that it was resolved to and when it was resolved. The "when" is according
    // to the BSYMMGR::tsImport clock.
    //
    // * D(TypeArray ta) = Sum(D(T) : T in ta)
    // * D(AGGTYPESYM ats) = U(ats) + D(ats.agg)
    // * D(T*) = D(T)
    // * D(TYVARSYM var) = U(var) + D(var.bounds)
    // * D(AGGSYM agg) = D(agg.base) + D(agg.ifaces) + Sum(U(T) : T is used in a member of agg)
    //
    // Intuitively, dirty means that either the type is unresolved or its inheritance includes
    // unresolved types or its members (inherited or direct) reference unresolved types.
    // Note that a type argument being dirty does not cause the AGGTYPESYM to be dirty. If a type
    // argument is unresolved, the AGGTYPESYM will be dirty though.
    //
    // Each TYPESYM and TypeArray keeps track of when it was last "cleaned" (stored in "tsDirty"
    // according to the BSYMMGR::tsImport clock) and whether it is still dirty. If a object is known
    // to be clean for all time (never needs cleaning again because it can't become dirty later), it
    // stores ktsImportMax for its tsDirty value.

    //const int ktsImportMax = 0x7FFFFFFF;

    //======================================================================
    // class TypeArrayTable
    //
    /// <summary>
    /// <para>This holds the normalized TypeArrays.
    /// TypeArrays are used for signatures, generic type parameter lists, etc.
    /// This guarantees that two TypeArrays are equivalent iff their addresses are the same.</para>
    /// <para>Class that has Dictionary&lt;Key, TypeArray&gt; and some method.
    /// (Key is made from TypeArray.)</para>
    /// <para>(CSharp\SCComp\Symmgr.cs)</para>
    /// </summary>
    //
    //  internal interface IHashTableBase<K, E, T>
    //  {
    //      T Table();
    //      E FindEntry(K key, uint hash);
    //      void InsertEntry(Entry enNew, out Entry enHere);
    //      //E FirstEntry();
    //      //E FindNextEntry(K key, uint hash);
    //  }
    //======================================================================
    internal class TypeArrayTable
    {
        //------------------------------------------------------------
        // TypeArrayTable   Fields and Properties
        //------------------------------------------------------------
        private Dictionary<TypeArray, TypeArray> typeArrayDictionary
            = new Dictionary<TypeArray, TypeArray>();

        internal Dictionary<TypeArray, TypeArray> Table
        {
            get { return typeArrayDictionary; }
        }

        //------------------------------------------------------------
        // TypeArrayTable   Constructor
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal TypeArrayTable() { }

        //------------------------------------------------------------
        // TypeArrayTable.Init
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal void Init() { }

        //------------------------------------------------------------
        // TypeArrayTable.Term
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal void Term() { }

        //------------------------------------------------------------
        // TypeArrayTable.FindEntry
        //
        /// <summary></summary>
        /// <param name="tarr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray FindEntry(TypeArray tarr)
        {
            if (tarr == null)
            {
                return BSYMMGR.EmptyTypeArray;
            }
            TypeArray tarr2 = null;

            try
            {
                if (this.typeArrayDictionary.TryGetValue(tarr, out tarr2))
                {
                    return tarr2;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return null;
        }

        //------------------------------------------------------------
        // TypeArrayTable.InsertEntry
        //
        /// <summary>
        /// If argument arr exists in the dictionary, return false.
        /// </summary>
        /// <param name="tarr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InsertEntry(TypeArray tarr)
        {
            if (tarr == null)
            {
                return false;
            }

            if (!typeArrayDictionary.ContainsKey(tarr))
            {
                try
                {
                    typeArrayDictionary.Add(tarr, tarr);
                }
                catch (ArgumentException)
                {
                    DebugUtil.Assert(false);
                }
#if DEBUG
                tarr.Registerd = true;
#endif
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // TypeArrayTable.RemoveEntry
        //
        /// <summary></summary>
        /// <param name="tarr"></param>
        //------------------------------------------------------------
        internal void RemoveEntry(TypeArray tarr)
        {
            if (tarr == null)
            {
                return;
            }

            try
            {
                if (typeArrayDictionary.ContainsKey(tarr))
                {
                    typeArrayDictionary.Remove(tarr);
#if DEBUG
                    tarr.Registerd = false;
#endif
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
        }

        //------------------------------------------------------------
        // TypeArrayTable.EqualEntryKey
        //
        /// <summary>
        /// <para>(sscli) Methods for HashTableBase.</para>
        /// <para>Does nothing. Return true.</para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal bool EqualEntryKey(Key key)
        //{
        //    return true;
        //}

        //------------------------------------------------------------
        // TypeArrayTable.GetHash
        //
        /// <summary></summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal int GetHash(Key key)
        //{
        //    return key.GetHashCode();
        //}

        //------------------------------------------------------------
        // TypeArrayTable.AllocTypeArray (1)
        //
        /// <summary>
        /// <para>Gets the one and only TypeArray for this array of types.</para>
        /// <para>Determine if the specified typeArray is registered to typeArrayDictionary.
        /// If not, register it.</para>
        /// <para>Return the argument as is.</para>
        /// </summary>
        /// <param name="tarr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray AllocTypeArray(TypeArray tarr)
        {
            if (tarr == null)
            {
                return BSYMMGR.EmptyTypeArray;
            }

            TypeArray tarr2 = null;

            if (typeArrayDictionary.TryGetValue(tarr, out tarr2))
            {
                return tarr2;
            }

            try
            {
                typeArrayDictionary.Add(tarr, tarr);
#if DEBUG
                tarr.Registerd = true;
#endif
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return tarr;
        }

        //------------------------------------------------------------
        // TypeArrayTable.AllocTypeArray (2)
        //
        /// <summary>
        /// <para>Gets the one and only TypeArray for this array of types.</para>
        /// <para>Determine if the TypeArray instance which has the specified list
        /// is registered to typeArrayDictionary.
        /// If not, create the TypeArray instance and register it.</para>
        /// <para>Return the created TypeArray instance.</para>
        /// </summary>
        /// <param name="typeList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray AllocTypeArray(List<TYPESYM> typeList)
        {
            if (typeList == null)
            {
                return BSYMMGR.EmptyTypeArray;
            }
            return AllocTypeArray(new TypeArray(typeList));
        }

        //------------------------------------------------------------
        // TypeArrayTable.AllocTypeArray (3)
        //
        /// <summary>
        /// <para>Gets the one and only TypeArray for this array of types.</para>
        /// <para>Determine if the TypeArray instance which has the specified array
        /// is registered to typeArrayDictionary.
        /// If not, create the TypeArray instance and register it.</para>
        /// <para>Return the created TypeArray instance.</para>
        /// </summary>
        /// <param name="typeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray AllocTypeArray(TYPESYM[] types)
        {
            if (types == null || types.Length == 0)
            {
                return BSYMMGR.EmptyTypeArray;
            }
            return AllocTypeArray(new TypeArray(types));
        }

#if DEBUG 
        //------------------------------------------------------------
        // TypeArrayTable.Debug
        //------------------------------------------------------------
        internal string Debug()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<TypeArray, TypeArray> kv in this.typeArrayDictionary)
            {
                TypeArray tarr = kv.Key;
                sb.AppendFormat("No.{0} ({1,8:X}): {2}\n",
                    tarr.TypeArrayID,
                    tarr.GetHashCode(),
                    tarr.GetDebugString());
            }
            return sb.ToString();
        }
#endif
    }

    //======================================================================
    // class InfoToSymTable
    //
    /// <summary>
    /// <para>This maps Type, TypeBuilder, MethodInfo, MethodBuilder, ... to SYM.
    /// Use in place of TokenToSymTable.</para>
    /// <para>(CSharp\SCComp\SymMgr.cs)</para>
    /// </summary>
    //======================================================================
    internal class InfoToSymTable
    {
        //------------------------------------------------------------
        // InfoToSymTable  Fields and Properties
        //------------------------------------------------------------
        protected Dictionary<System.Object, SYM> infoToSymDictionary
            = new Dictionary<System.Object, SYM>();

        internal Dictionary<System.Object, SYM> Table
        {
            get { return this.infoToSymDictionary; }
        }

        //------------------------------------------------------------
        // TypeToSymTable  Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal InfoToSymTable() { }

        //------------------------------------------------------------
        // TypeToSymTable.Init
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Init() { }

        //------------------------------------------------------------
        // TypeToSymTable.Term
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Term() { }

        //------------------------------------------------------------
        // InfoToSymTable.GetSymFromInfo
        //
        /// <summary>
        /// Get the SYM for System.Type. Return false iff it's never been set.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetSymFromInfo(System.Object obj)
        {
            if (obj == null)
            {
                return null;
            }
            SYM sym = null;

            if (this.infoToSymDictionary.TryGetValue(obj, out sym))
            {
                return sym;
            }
            return null;
        }

        //------------------------------------------------------------
        // InfoToSymTable.SetSymForInfo
        //
        /// <summary>
        /// Set the SYM for System.Type.
        /// Asserts that there isn't already a mapping.
        /// The sym may be null indicating that there was a previous failure identifying the sym.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetSymForInfo(System.Object obj, SYM sym)
        {
            if (obj == null)
            {
                return false;
            }

            try
            {
                if (!this.infoToSymDictionary.ContainsKey(obj))
                {
                    this.infoToSymDictionary.Add(obj, sym);
                    return true;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return false;
        }

        //------------------------------------------------------------
        // InfoToSymTable.Contains
        //
        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Contains(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            try
            {
                return this.infoToSymDictionary.ContainsKey(obj);
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return false;
        }
    }

    //======================================================================
    // class NameToSymTable
    //
    /// <summary>
    /// <para>This maps from a NAME to a SYM.</para>
    /// <para>This is used (for example) to map from a full assembly name to the INFILESYM (see import).</para>
    /// <para>(CSharp\SCComp\SymMgr.cs)</para>
    /// </summary>
    //======================================================================
    internal class NameToSymTable
    {
        //------------------------------------------------------------
        // NameToSymTable   Fields and Properties
        //------------------------------------------------------------
        protected Dictionary<string, SYM> nameToSymDictionary
            = new Dictionary<string, SYM>();

        internal Dictionary<string, SYM> Table
        {
            get { return this.nameToSymDictionary; }
        }

        //------------------------------------------------------------
        // NameToSymTable   Methods
        //------------------------------------------------------------
        internal NameToSymTable() { }
        internal void Init() { }
        internal void Term() { }

        //------------------------------------------------------------
        // NameToSymTable.GetSymFromName
        //
        /// <summary>
        /// <para>Get the SYM for name. Return false iff it's never been set.</para>
        /// <para>Return null, if not found.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetSymFromName(string name)
        {
            if (name == null)
            {
                return null;
            }
            SYM sym = null;

            if (this.nameToSymDictionary.TryGetValue(name, out sym))
            {
                return sym;
            }
            return null;
        }

        //------------------------------------------------------------
        // NameToSymTable.SetSymForName
        //
        // Set the SYM for name. Asserts that there isn't already a mapping.
        // The sym may be NULL indicating that there was a previous failure identifying the sym.
        //------------------------------------------------------------
        internal void SetSymForName(string name, SYM sym)
        {
            if (name != null && !this.nameToSymDictionary.ContainsKey(name))
            {
                this.nameToSymDictionary.Add(name, sym);
            }
        }
    }

    //======================================================================
    // class SymSet
    //
    /// <summary>
    /// <para>This maps from an Aid to a SYM.</para>
    /// <para>Class has List&lt;SYM&gt; and some methods.
    /// Register SYM instances and assign them indice starting from 0.
    /// These indice are converted to assembly IDs.</para>
    /// <para>(CSharp\SCComp\SymMgr.cs)</para>
    /// </summary>
    //======================================================================
    internal class SymSet
    {
        //------------------------------------------------------------
        // SymSet Fields and Properties
        //------------------------------------------------------------
        protected List<SYM> symList = new List<SYM>();

        internal int Size
        {
            get { return symList.Count; }
        }

        //------------------------------------------------------------
        // SymSet Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal SymSet() { }

        //------------------------------------------------------------
        // SymSet.Init
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Init() { }

        //------------------------------------------------------------
        // SymSet.Term
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Term() { }

        //------------------------------------------------------------
        // SymSet.AddSym
        //
        /// <summary>
        /// Regisger a SYM instance and return its index.
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int AddSym(SYM sym)
        {
            int c = symList.Count;
            symList.Add(sym);
            return c;
        }

        //------------------------------------------------------------
        // SymSet.GetSym
        //
        /// <summary>
        /// Return SYM instance with the specified index.
        /// </summary>
        /// <param name="isym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetSym(int isym)
        {
            DebugUtil.Assert(this.symList != null);

            if (isym >= 0 && isym < this.symList.Count)
            {
                return this.symList[isym];
            }
            return null;
        }
    }

    //======================================================================
    // enum SubstTypeFlags
    //
    /// <summary>
    /// <para>Used to specify whether and which type variables should be normalized.</para>
    /// <para>(CSharp\SCComp\SymMgr.cs)</para>
    /// </summary>
    //======================================================================
    [Flags]
    internal enum SubstTypeFlagsEnum : int
    {
        NormNone = 0x00,

        /// <summary>
        /// Replace class type variables with the normalized (standard) ones.
        /// </summary>
        NormClass = 0x01,

        /// <summary>
        /// Replace method type variables with the normalized (standard) ones.
        /// </summary>
        NormMeth = 0x02,

        /// <summary>
        /// NormClass | NormMeth
        /// </summary>
        NormAll = NormClass | NormMeth,

        /// <summary>
        /// Replace normalized (standard) class type variables with the given class type args.
        /// </summary>
        DenormClass = 0x04,

        /// <summary>
        /// Replace normalized (standard) method type variables with the given method type args.
        /// </summary>
        DenormMeth = 0x08,

        /// <summary>
        /// DenormClass | DenormMeth
        /// </summary>
        DenormAll = DenormClass | DenormMeth
    }

    //======================================================================
    // class SubstContext
    //
    /// <summary>
    /// <para>Represents type arguments.</para>
    /// <para>Has two TypeArrays for a type and methods.</para>
    /// <para>(CSharp\SCComp\SymMgr.cs)</para>
    /// </summary>
    //======================================================================
    internal class SubstContext
    {
        //------------------------------------------------------------
        // SubstContext Fields and Properties
        //------------------------------------------------------------
        internal TypeArray ClassTypeArguments = null;   // TYPESYM ** prgtypeCls;
        internal TypeArray MethodTypeArguments = null;  // TYPESYM ** prgtypeMeth;
        internal SubstTypeFlagsEnum Flags;              // grfst

        internal int ClassTypeArgumentCount // ctypeCls
        {
            get { return (ClassTypeArguments != null ? ClassTypeArguments.Count : 0); }
        }

        internal int MethodTypeArgumentCount    // ctypeMeth
        {
            get { return (MethodTypeArguments != null ? MethodTypeArguments.Count : 0); }
        }

        //------------------------------------------------------------
        // SubstContext Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal SubstContext()
        {
            Init(null, null, SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // SubstContext Constructor (2)
        //
        /// <summary></summary>
        /// <param name="classArgs"></param>
        /// <param name="methodArgs"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal SubstContext(
            TypeArray classArgs,
            TypeArray methodArgs,       // = NULL,
            SubstTypeFlagsEnum flags)   // = SubstTypeFlags::NormNone)
        {
            Init(classArgs, methodArgs, flags);
        }

        //------------------------------------------------------------
        // SubstContext Constructor (3)
        //
        /// <summary>
        /// 引数 type の AllTypeArgs を TypeCls とする。
        /// </summary>
        //------------------------------------------------------------
        internal SubstContext(
            AGGTYPESYM aggTypeSym,
            TypeArray methodArgs,       // = NULL,
            SubstTypeFlagsEnum flags)   // = SubstTypeFlags::NormNone)
        {
            Init((aggTypeSym != null ? aggTypeSym.AllTypeArguments : null), methodArgs, flags);
        }

        //------------------------------------------------------------
        // SubstContext Constructor (4)
        //------------------------------------------------------------
        internal SubstContext(
            List<TYPESYM> classArgs,
            List<TYPESYM> methodArgs,
            SubstTypeFlagsEnum flags)   // = SubstTypeFlags::NormNone)
        {
            if (classArgs != null)
            {
                this.ClassTypeArguments = new TypeArray(classArgs);
            }
            else
            {
                this.ClassTypeArguments = new TypeArray();
            }

            if (methodArgs != null)
            {
                this.MethodTypeArguments = new TypeArray(methodArgs);
            }
            else
            {
                this.MethodTypeArguments = new TypeArray();
            }

            this.Flags = flags;
        }

        //------------------------------------------------------------
        // SubstContext Constructor (5)
        //------------------------------------------------------------
        internal SubstContext(
            TYPESYM[] classArgs,
            TYPESYM[] methodArgs,
            SubstTypeFlagsEnum flags)   // = SubstTypeFlags::NormNone)
        {
            if (classArgs != null)
            {
                this.ClassTypeArguments = new TypeArray(new List<TYPESYM>(classArgs));
            }
            else
            {
                this.ClassTypeArguments = new TypeArray();
            }

            if (methodArgs != null)
            {
                this.MethodTypeArguments = new TypeArray(new List<TYPESYM>(methodArgs));
            }
            else
            {
                this.MethodTypeArguments = new TypeArray();
            }

            this.Flags = flags;
        }

        //------------------------------------------------------------
        // SubstContext.FNop
        //
        /// <summary>
        /// No arguments and do not normalize.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FNop()
        {
            return (
                TypeArray.Size(ClassTypeArguments) == 0 &&
                TypeArray.Size(MethodTypeArguments) == 0 &&
                (Flags & SubstTypeFlagsEnum.NormAll) == 0);
        }

        //------------------------------------------------------------
        // SubstContext.Init
        //
        /// <summary>
        /// <para>Initializes a substitution context.</para>
        /// <para>Returns false iff no substitutions will ever be performed.</para>
        /// </summary>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal void Init(
            TypeArray classArgs,
            TypeArray methodArgs,       // = null,
            SubstTypeFlagsEnum flags)   // = SubstTypeFlags.NormNone)
        {
            if (classArgs != null)
            {
                classArgs.AssertValid();
                ClassTypeArguments = classArgs;
            }
            else
            {
                ClassTypeArguments = new TypeArray();   // = null;
            }

            if (methodArgs != null)
            {
                methodArgs.AssertValid();
                MethodTypeArguments = methodArgs;
            }
            else
            {
                MethodTypeArguments = new TypeArray();  // = null;
            }

            this.Flags = flags;
        }
    }

    //======================================================================
    // class SYMTBL
    //
    /// <summary>
    /// <para>A symbol table is a helper class used by the symbol manager.
    /// There are two symbol tables; a global and a local.</para> 
    /// <para>This class has a Dictionary whose key is (SYM.Name, SYM.ParentSym)
    /// and value is SYM instance.</para>
    /// <para>If there are SYM instances with same name and same parent, store them in list
    /// by SYM.NextSameNameSym.</para>
    /// <para>(CSharp\SCComp\SymMgr.cs)</para>
    /// </summary>
    //======================================================================
    internal class SYMTBL
    {
        //============================================================
        // struct SYMTBL.Key
        //============================================================
        private struct Key
        {
            internal string name;
            internal PARENTSYM parent;
            private readonly int hashCode;

            //--------------------------------------------------------
            // SYMTBL.Key Constructor
            //--------------------------------------------------------
            internal Key(string n, PARENTSYM p)
            {
                name = n;
                parent = p;
                this.hashCode = CalcHashCode(n, p);
            }

            //--------------------------------------------------------
            // SYMTBL.Key.Equals (override)
            //
            /// <summary></summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            //--------------------------------------------------------
            public override bool Equals(object obj)
            {
                if (obj != null)
                {
                    try
                    {
                        Key other = (Key)obj;
                        return (name == other.name && parent == other.parent);
                    }
                    catch (InvalidCastException)
                    {
                    }
                }
                return false;
            }

            //--------------------------------------------------------
            // SYMTBL.Key.CalcHashCode
            //--------------------------------------------------------
            internal static int CalcHashCode(string name, PARENTSYM sym)
            {
                int h1 = (name != null ? name.GetHashCode() : 0);
                int h2 = (sym != null ? sym.GetHashCode() : 0);
                return ((h1 ^ h2) & 0x7FFFFFFF);
            }

            //--------------------------------------------------------
            // SYMTBL.Key.GetHashCode (override)
            //--------------------------------------------------------
            public override int GetHashCode()
            {
                return this.hashCode;
            }

            //--------------------------------------------------------
            // SYMTBL.Key operator ==
            //--------------------------------------------------------
            public static bool operator ==(Key k1, Key k2)
            {
                // class -> struct
                //if (k1 == null || k2 == null)
                //{
                //    return false;
                //}
                return k1.Equals(k2);
            }

            //--------------------------------------------------------
            // SYMTBL.Key operator !=
            //--------------------------------------------------------
            public static bool operator !=(Key k1, Key k2)
            {
                // class -> struct
                //if (k1 == null || k2 == null)
                //{
                //    return true;
                //}
                return !(k1.Equals(k2));
            }

#if DEBUG
            //------------------------------------------------------------
            // SYMTBL.Key.Debug
            //------------------------------------------------------------
            internal void Debug(StringBuilder sb)
            {
                sb.Append("Name  : ");
                if (this.name != null)
                {
                    int start = this.name.LastIndexOf('_');
                    if (start < 0 || start == this.name.Length - 1)
                    {
                        start = 0;
                    }
                    int end = start + 4;
                    if (end > this.name.Length) end = this.name.Length;

                    sb.AppendFormat("{0} (", this.name);
                    int len = (this.name.Length > 4 ? 4 : this.name.Length);
                    for (int i = start; i < end; ++i)
                    {
                        if (i > 0) sb.Append(" ");
                        sb.AppendFormat("0x{0,0:X}", (int)this.name[i]);
                    }
                    sb.Append(")");
                }
                else
                {
                    sb.Append("(null)\n");
                }
                sb.Append("\n");
                if (this.parent != null)
                {
                    sb.AppendFormat("Parent: No.{0} {1} ({2})\n",
                        parent.SymID,
                        parent.Name != null ? parent.Name : "(null)",
                        parent.Kind);
                }
                else
                {
                }
            }
#endif
        }

        //------------------------------------------------------------
        // SYMTBL Fields and Properties
        //------------------------------------------------------------
        private System.Collections.Generic.Dictionary<Key, SYM> symbolDictionary
            = new Dictionary<Key, SYM>();

        //------------------------------------------------------------
        // SYMTBL   Constructor
        //------------------------------------------------------------
        internal SYMTBL() { }

        //------------------------------------------------------------
        // SYMTBL.LookupSym
        //
        /// <summary>
        /// <para>Look up a symbol by name and parent, filtering by mask.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="kindmask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM LookupSym(string name, PARENTSYM parent, SYMBMASK kindmask)
        {
            SYM child = null;
            Key key = new Key(name, parent);

            try
            {
                if (!symbolDictionary.TryGetValue(key, out child))
                {
                    return null;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
                return null;
            }

            while (child != null)
            {
                if ((child.Mask & kindmask) != 0)
                {
                    return child;
                }
                child = child.NextSameNameSym;
            }
            return null;
        }

        //------------------------------------------------------------
        // SYMTBL.LookupNextSym
        //------------------------------------------------------------
        internal SYM LookupNextSym(SYM symPrev, PARENTSYM parent, long kindmask)
        {
            // no method body (sscli)
            throw new NotImplementedException("SYMTBL.LookupNextSym");
        }

        //------------------------------------------------------------
        // SYMTBL.Clear
        //
        /// <summary>
        /// Clear the table of symbols.
        /// </summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            symbolDictionary.Clear();
        }

        //------------------------------------------------------------
        // SYMTBL.Term
        //
        // Terminate the table. Free the bucket array.
        //------------------------------------------------------------
        internal void Term()
        {
            this.Clear();
        }

        //------------------------------------------------------------
        // SYMTBL.InsertChild
        //
        /// <summary>
        /// <para>Add a named symbol to a parent scope, for later lookup.</para>
        /// <para>SYMTBL.InsertChildNoGrow もこちらにまとめる。</para>
        /// <para>Helper routine to place a child into the hash table.
        /// This hash table is organized to enable quick resolving of
        /// mapping a (name, parent) pair to a child symbol.
        /// If multiple child symbols of a parent have the same name,
        /// they are all chained together, so we need only find the first one.
        /// We place children in a hash table with the hash function a hashing of the 
        /// NAME and PARENT addresses. We use double hashing to handle collisions
        /// (the second hash providing the "jump"), and double the size of the table
        /// when it becomes 75% full.</para>
        /// <para>Insert a SYM into Dictionary.
        /// The key is the pair of child.Name and child.ParentSym, and the value is child.</para>
        /// <para>If multiple SYMs have a same name and a same ParentSym, link them and make a list. </para>
        /// </summary>
        //------------------------------------------------------------
        internal void InsertChild(PARENTSYM parentSym, SYM childSym)
        {
#if false
            if (child.Kind == SYMKIND.NSAIDSYM)
            {
                string name = child.Name;
            }
#endif
            // If child.NextSameNameSym is not null, this SYM instance is already registerd and return.
            // Or if the parent of child and argument parent are not same, not add.
            if (childSym.NextSameNameSym != null)
            {
                return;
            }
            if (childSym.ParentSym != null && childSym.ParentSym != parentSym)
            {
                return;
            }

            childSym.ParentSym = parentSym;
            Key key = new Key(childSym.SearchName, childSym.ParentSym);
            SYM sameNameSym = null;

            try
            {
                if (!symbolDictionary.TryGetValue(key, out sameNameSym))
                {
                    symbolDictionary.Add(key, childSym);

                    childSym.NextSameNameSym = null;
                    childSym.PrevSameNameSym = null;
                    childSym.LastSameNameSym = childSym;
                    return;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
                return;
            }

            // If the key is already registered, store child in list using SYM.NextSameNameSym field.
            if (sameNameSym == null)
            {
                // If the value is null, set child to value.
                //symbolDictionary.Add(key, childSym);
                DebugUtil.Assert(false);
                symbolDictionary[key] = childSym;

                childSym.NextSameNameSym = null;
                childSym.PrevSameNameSym = null;
                childSym.LastSameNameSym = childSym;
            }
            else
            {
                sameNameSym.LastSameNameSym = childSym;
                while (sameNameSym.NextSameNameSym != null)
                {
                    sameNameSym = sameNameSym.NextSameNameSym;
                }
                sameNameSym.NextSameNameSym = childSym;
                childSym.PrevSameNameSym = sameNameSym;
                childSym.NextSameNameSym = null;
            }
        }

        //------------------------------------------------------------
        // SYMTBL.ClearChildren
        //
        /// <summary>
        /// Remove all child symbols of parent that match kindmask.
        /// Note: this does not remove any nested children
        /// </summary>
        /// <param name="parentSym"></param>
        /// <param name="kindmask"></param>
        //------------------------------------------------------------
        internal void ClearChildren(PARENTSYM parentSym, SYMBMASK kindmask)
        {
            SYM child = parentSym.FirstChildSym;
            parentSym.FirstChildSym = parentSym.LastChildSym = null;
            while (child != null)
            {
                if ((child.Mask & kindmask) == 0)
                {
                    if (parentSym.LastChildSym == null)
                    {
                        parentSym.FirstChildSym = parentSym.LastChildSym = child;
                    }
                    else
                    {
                        parentSym.LastChildSym.NextSym = child;
                        parentSym.LastChildSym = child;
                    }
                    child = child.NextSym;
                }
                else
                {
                    SYM temp = child;
                    child = child.NextSym;
                    temp.ParentSym = null;
                    temp.NextSym = null;
                }
            }
        }

        // This special value markets a bucket as empty but previously occupied.
        // #define ksymDead ((SYM *)1)

        //------------------------------------------------------------
        // SYMTBL.RemoveChildFromBuckets
        //
        /// <summary>
        /// <para>Remove a symbol from SYM table and nextSameName list.
        /// Also sets the isDead bit</para>
        /// <para>Note: this does not remove any children of the SYM</para>
        /// </summary>
        /// <param name="orphanSym"></param>
        //------------------------------------------------------------
        private void RemoveChildFromBuckets(SYM orphanSym)
        {
            SYM sameNameSym = null;
            Key key = new Key(orphanSym.SearchName, orphanSym.ParentSym);

            if (!symbolDictionary.TryGetValue(key, out sameNameSym))
            {
                orphanSym.IsDead = true;
                return;
            }

            if (sameNameSym == orphanSym)
            {
                symbolDictionary[key] = sameNameSym.NextSameNameSym;
            }
            else
            {
                SYM nextChild = null;

                while ((nextChild = sameNameSym.NextSameNameSym) != null && nextChild != orphanSym)
                {
                    sameNameSym = nextChild;
                }
                if (nextChild != null)
                {
                    sameNameSym.NextSameNameSym = nextChild.NextSameNameSym;
                }
            }
            orphanSym.IsDead = true;
        }

        //private void GrowTable();
        //private int Bucket(string name, PARENTSYM parent, out uint jump)

#if DEBUG
        virtual internal void Debug(StringBuilder sb)
        {
            foreach (KeyValuePair<Key, SYM> kv in symbolDictionary)
            {
                sb.Append("==============================\n");
                kv.Key.Debug(sb);
                sb.Append("------------------------------\n");
                kv.Value.Debug(sb);
            }
        }
#endif
    }

    //======================================================================
    // class LSYMMGR
    //
    /// <summary>
    /// The local symbols manager
    /// </summary>
    //======================================================================
    internal class LSYMMGR
    {
        // friend class BSYMMGR; // friend クラスがあるのですべて internal とする。
        //------------------------------------------------------------
        // LSYMMGR Fields and Properties
        //------------------------------------------------------------
        private SYMTBL localSymbolTable = new SYMTBL();

        internal SYMTBL LocalSymbolTable
        {
            get { return localSymbolTable; }
        }

        //------------------------------------------------------------
        // LSYMMGR Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal LSYMMGR() { }

        //------------------------------------------------------------
        // LSYMMGR.Init
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Init() { }  // 元のソースでは、本体は定義されていない。

        //------------------------------------------------------------
        // LSYMMGR.Term
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Term()
        {
            localSymbolTable.Term();
        }

        //------------------------------------------------------------
        // LSYMMGR.DestroyLocalSymbols
        //
        /// <summary>
        /// Clear the local table in preperation for the next fuction
        /// </summary>
        //------------------------------------------------------------
        internal void DestroyLocalSymbols()
        {
            localSymbolTable.Clear();
        }

        //------------------------------------------------------------
        // LSYMMGR.RemoveChildSyms
        //
        /// <summary></summary>
        /// <param name="parent"></param>
        /// <param name="kindmask"></param>
        //------------------------------------------------------------
        internal void RemoveChildSyms(PARENTSYM parent, SYMBMASK kindmask)
        {
            localSymbolTable.ClearChildren(parent, kindmask);
        }

        //------------------------------------------------------------
        // LSYMMGR.CreateLocalSym
        //
        /// <summary>
        /// The main routine for creating a local symbol
        /// and putting it into the symbol table under a particular parent.
        /// Either name or parent can be NULL.
        /// </summary>
        /// <param name="symkind"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM CreateLocalSym(SYMKIND symkind, string name, PARENTSYM parent)
        {
            SYM lsym;
            bool sidLast = false;

#if DEBUG
            // Only some symbol kinds are valid as local symbols. Validate.
            switch (symkind)
            {
                case SYMKIND.LOCVARSYM:	// if (1) break; goto LFail;
                case SYMKIND.UNITSYM:	// if (1) break; goto LFail;
                case SYMKIND.SCOPESYM:	// if (1) break; goto LFail;
                case SYMKIND.CACHESYM:	// if (1) break; goto LFail;
                case SYMKIND.LABELSYM:	// if (1) break; goto LFail;
                case SYMKIND.ANONSCOPESYM:	// if (1) break; goto LFail;
                    break;

                case SYMKIND.NSSYM:	// if (0) break; goto LFail;
                case SYMKIND.NSDECLSYM:	// if (0) break; goto LFail;
                case SYMKIND.NSAIDSYM:	// if (0) break; goto LFail;
                case SYMKIND.AGGSYM:	// if (0) break; goto LFail;
                case SYMKIND.AGGDECLSYM:	// if (0) break; goto LFail;
                case SYMKIND.AGGTYPESYM:	// if (0) break; goto LFail;
                case SYMKIND.FWDAGGSYM:	// if (0) break; goto LFail;
                case SYMKIND.TYVARSYM:	// if (0) break; goto LFail;
                case SYMKIND.MEMBVARSYM:	// if (0) break; goto LFail;
                case SYMKIND.METHSYM:	// if (0) break; goto LFail;
                case SYMKIND.FAKEMETHSYM:	// if (0) break; goto LFail; 
                case SYMKIND.PROPSYM:	// if (0) break; goto LFail;
                case SYMKIND.EVENTSYM:	// if (0) break; goto LFail;
                case SYMKIND.VOIDSYM:	// if (0) break; goto LFail;
                case SYMKIND.NULLSYM:	// if (0) break; goto LFail;
                case SYMKIND.ANONMETHSYM:	// if (0) break; goto LFail;
                case SYMKIND.METHGRPSYM:	// if (0) break; goto LFail;
                case SYMKIND.ERRORSYM:	// if (0) break; goto LFail;
                case SYMKIND.ARRAYSYM:	// if (0) break; goto LFail;
                case SYMKIND.PTRSYM:	// if (0) break; goto LFail;
                case SYMKIND.PINNEDSYM:	// if (0) break; goto LFail;
                case SYMKIND.PARAMMODSYM:	// if (0) break; goto LFail;
                case SYMKIND.MODOPTSYM:	// if (0) break; goto LFail;
                case SYMKIND.MODOPTTYPESYM:	// if (0) break; goto LFail;
                case SYMKIND.NUBSYM:	// if (0) break; goto LFail; 
                case SYMKIND.INFILESYM:	// if (0) break; goto LFail;
                case SYMKIND.MODULESYM:	// if (0) break; goto LFail;
                case SYMKIND.RESFILESYM:	// if (0) break; goto LFail;
                case SYMKIND.OUTFILESYM:	// if (0) break; goto LFail;
                case SYMKIND.XMLFILESYM:	// if (0) break; goto LFail;
                case SYMKIND.SYNTHINFILESYM:	// if (0) break; goto LFail;
                case SYMKIND.ALIASSYM:	// if (0) break; goto LFail;
                case SYMKIND.EXTERNALIASSYM:	// if (0) break; goto LFail;
                case SYMKIND.MISCSYM:	// if (0) break; goto LFail;
                case SYMKIND.GLOBALATTRSYM:	// if (0) break; goto LFail;
                case SYMKIND.UNRESAGGSYM:	// if (0) break; goto LFail;
                case SYMKIND.IFACEIMPLMETHSYM:	// if (0) break; goto LFail;
                case SYMKIND.INDEXERSYM:	// if (0) break; goto LFail;
                case SYMKIND.IMPLICITTYPESYM:           // CS3
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM:   // CS3
                case SYMKIND.LAMBDAEXPRSYM:             // CS3
                case SYMKIND.DYNAMICSYM:                // CS4
                default:
                    //LFail:
                    DebugUtil.Assert(false, "Bad symkind in CreateLocalSym");
                    break;
            }
#endif
            // Allocate the symbol from the local allocator and fill in the name member.
            lsym = AllocSym(symkind, name, sidLast);
            lsym.IsLocal = true;
            if (parent != null)
            {
                AddChild(localSymbolTable, parent, lsym);
            }
            return lsym;
        }

        //------------------------------------------------------------
        // LSYMMGR.LookupLocalSym
        //
        /// <summary>
        /// <para>The main routine for looking up a local symbol by name.
        /// It's possible for there to be more than one symbol
        /// with a particular name in a particular parent;
        /// if you want to check for more, then use LookupNextSym.</para>
        /// <para>kindmask filters the result by particular symbol kinds.</para>
        /// <para>returns NULL if no match found.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="kindmask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM LookupLocalSym(string name, PARENTSYM parent, SYMBMASK kindmask)
        {
#if DEBUG
            if (!(name != null))
            {
                ;
            }
#endif
            DebugUtil.Assert(name != null);   // name can't be NULL.
            return localSymbolTable.LookupSym(name, parent, kindmask);
        }

        //------------------------------------------------------------
        // LSYMMGR.LookupNextSym (static)
        //
        /// <summary></summary>
        /// <param name="symPrev"></param>
        /// <param name="parent"></param>
        /// <param name="kindmask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static SYM LookupNextSym(SYM symPrev, PARENTSYM parent, SYMBMASK kindmask)
        {
            return BSYMMGR.LookupNextSym(symPrev, parent, kindmask);
        }

        //------------------------------------------------------------
        // LSYMMGR.AddToLocalSymList
        //
        /// <summary>
        /// <para>Add a sym to a symbol list.
        /// The memory for the list is allocated from the local symbol area,
        /// so this is appropriate only for local symbols.</para>
        /// <para>The calls should pass a pointer to a local that's initialized to point to the PSYMLIST
        /// that's the head of the list.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="symLink"></param>
        //------------------------------------------------------------
        internal void AddToLocalSymList(SYM sym, List<SYM> symLink)
        {
            AddToSymList(sym, symLink);
        }

        //------------------------------------------------------------
        // LSYMMGR.AddToLocalNameList
        //
        /// <summary>
        /// <para>Add a name to a symbol list.
        /// The memory for the list is allocated from the local symbol area,
        /// so this is appropriate only for local symbols.</para>
        /// <para>The calls should pass a pointer to a local that's initialized to
        /// point to the PNAMELIST that's the head of the list.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nameLink"></param>
        //------------------------------------------------------------
        internal void AddToLocalNameList(string name, List<string> nameLink)
        {
            AddToNameList(name, nameLink);
        }

        //------------------------------------------------------------
        // LSYMMGR.AddToLocalSymList
        //
        /// <summary>
        /// <para>Add an attribute to a symbol list.
        /// The memory for the list is allocated from the local symbol area.</para>
        /// <para>The calls should pass a pointer to a local that's initialized to point
        /// to the PATTRLIST that's the head of the list.</para>
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="context"></param>
        /// <param name="attrList"></param>
        //------------------------------------------------------------
        internal void AddToLocalAttrList(BASENODE attr, PARENTSYM context, List<ATTRINFO> attrList)
        {
            BSYMMGR.AddToAttrList(attr, context, attrList);
        }

        //------------------------------------------------------------
        // LSYMMGR.AddToSymList
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="symList"></param>
        //------------------------------------------------------------
        internal static void AddToSymList(SYM sym, List<SYM> symList)
        {
            BSYMMGR.AddToSymList(sym, symList);
        }

        //------------------------------------------------------------
        // LSYMMGR.AddChild
        //
        /// <summary>
        /// add a symbol in the regular way into a symbol table
        /// </summary>
        /// <param name="tabl"></param>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        //------------------------------------------------------------
        private static void AddChild(SYMTBL tabl, PARENTSYM parent, SYM child)
        {
            BSYMMGR.AddChild(tabl, parent, child);
        }

        //------------------------------------------------------------
        // LSYMMGR.AddToNameList
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="nameList"></param>
        //------------------------------------------------------------
        private static void AddToNameList(string name, List<string> nameList)
        {
            BSYMMGR.AddToNameList(name, nameList);
        }

        //------------------------------------------------------------
        // LSYMMGR.AllocSym
        //
        /// <summary></summary>
        /// <param name="symkind"></param>
        /// <param name="name"></param>
        /// <param name="sidLast"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private static SYM AllocSym(SYMKIND symkind, string name, bool sidLast)
        {
            return BSYMMGR.AllocSymWorker(symkind, name, sidLast);
        }
    }

    //======================================================================
    // class BSYMMGR
    //
    /// <summary>
    /// The main symbol manager,
    /// restricted to functionality needed only in csee + (intersection of csee, cscomp)
    /// </summary>
    //======================================================================
    internal partial class BSYMMGR
    {
        // friend void LSYMMGR::AddChild(SYMTBL *tabl, PPARENTSYM parent, PSYM child);
        // friend PSYM LSYMMGR::AllocSym(SYMKIND symkind, PNAME name, NRHEAP * allocator, int * psidLast);
        // friend void LSYMMGR::AddToSymList(NRHEAP *heap, PSYM sym, PSYMLIST * * symLink);
        // friend void LSYMMGR::AddToNameList(NRHEAP *heap, PNAME name, PNAMELIST * * nameLink);
        // friend void LSYMMGR::AddToLocalAttrList(BASENODE *attr, PARENTSYM *context, PATTRLIST * * nameLink);

        //============================================================
        // class BSYMMGR.UnifyContext
        //
        /// <summary>
        /// <para>Has two TypeArray instances of class and method.</para>
        /// <para>(CSharp\SCComp\Symmgr.cs)</para>
        /// </summary>
        //============================================================
        internal class UnifyContext : SubstContext
        {
            // TYPESYM ** prgtypeCls;   // [in, out] The unification mapping for the class type variables. Inherited.
            // TYPESYM ** prgtypeMeth;  // [in, out] The unification mapping for the method type variables. Inherited.
            internal TypeArray ClassTypeVariables;  // [in] The class type variables.
            internal TypeArray MethodTypeVariables; // [in] The method type variables.

            //--------------------------------------------------------
            // BSYMMGR.UnifyContext Constructor
            //
            /// <summary></summary>
            /// <param name="classVars"></param>
            /// <param name="methodVars"></param>
            //--------------------------------------------------------
            internal UnifyContext(
                TypeArray classVars,
                TypeArray methodVars)
            {
                this.ClassTypeVariables = classVars;
                this.MethodTypeVariables = methodVars;
                Clear();
            }

            //--------------------------------------------------------
            // Gets the map slot for the type var. Returns NULL if tvs is invalid.
            // Find the unification mapping slot for the given type variable.
            // Returns NULL if the type variable isn't a known one.
            //--------------------------------------------------------
            //TYPESYM ** BSYMMGR::UnifyContext::GetSlot(TYVARSYM *tvs)
            //{
            //    if (tvs->isMethTyVar) {
            //        if (tvs->indexTotal >= typeVarsMeth->size ||
            //            tvs != typeVarsMeth->Item(tvs->indexTotal))
            //        {
            //            return NULL;
            //        }
            //        return prgtypeMeth + tvs->indexTotal;
            //    }
            //
            //    if (tvs->indexTotal >= typeVarsCls->size ||
            //        tvs != typeVarsCls->Item(tvs->indexTotal))
            //    {
            //        return NULL;
            //    }
            //    return prgtypeCls + tvs->indexTotal;
            //}
            //--------------------------------------------------------
            // BSYMMGR.UnifyContext.GetSlot
            //
            /// <summary>
            /// <para>Return the element of MethodTypeVariables or ClassTypeVariables
            /// which has the same index to argument tvSym.</para>
            /// <para>In sscli, return TYPESYM **.
            /// But this method return the refernce to TYPESYM instance.</para>
            /// </summary>
            /// <param name="tvSym"></param>
            /// <returns></returns>
            //--------------------------------------------------------
            internal TYPESYM GetSlot(TYVARSYM tvSym)
            {
                if (tvSym.IsMethodTypeVariable)
                {
                   return this.MethodTypeVariables[tvSym.TotalIndex];
                }
                else
                {
                    return this.ClassTypeVariables[tvSym.TotalIndex];
                }
            }

            //--------------------------------------------------------
            // BSYMMGR.UnifyContext.Clear
            //
            /// <summary>
            /// Clear type arguments, instead set variables.
            /// </summary>
            //--------------------------------------------------------
            internal void Clear()
            {
                DebugUtil.Assert(this.ClassTypeVariables != null);
                DebugUtil.Assert(this.MethodTypeVariables != null);

                this.ClassTypeArguments = new TypeArray();
                this.ClassTypeArguments.Add(this.ClassTypeVariables);
                this.MethodTypeArguments = new TypeArray();
                this.MethodTypeArguments.Add(this.MethodTypeVariables);
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.UnifyTypes
        //------------------------------------------------------------
        //bool UnifyTypes(TYPESYM* t1, TYPESYM* t2, UnifyContext* pctx); // moved down

        //============================================================
        // class BSYMMGR.InferContext
        //
        /// <summary></summary>
        //============================================================
        internal class InferContext
        {
            /// <summary>
            /// [in] The method type variables.
            /// </summary>
            internal TypeArray MethodTypeVariables = null; // * typeVarsMeth

            /// <summary>
            /// [in] The class type arguments - used to map typeSrc as we go.
            /// </summary>
            internal TypeArray ClassTypeArguments = null;  // * typeArgsCls;

            /// <summary>
            /// [in, out] The unification mapping for the method type variables.
            /// </summary>
            //internal List<TYPESYM> UnifiedMethodTypeVariables = null;  // TYPESYM ** prgtypeMeth;
            internal TypeArray UnifiedMethodTypeVariables = null;  // TYPESYM ** prgtypeMeth;

            //--------------------------------------------------------
            // BSYMMGR.InferContext Constructor (1)
            //
            /// <summary>/// </summary>
            //--------------------------------------------------------
            internal InferContext() { }

            //--------------------------------------------------------
            // BSYMMGR.InferContext Constructor (2)
            //
            /// <summary></summary>
            /// <param name="other"></param>
            //--------------------------------------------------------
            internal InferContext(InferContext other)
            {
                this.MethodTypeVariables = other.MethodTypeVariables;
                this.ClassTypeArguments = other.ClassTypeArguments;
                this.UnifiedMethodTypeVariables = other.UnifiedMethodTypeVariables;
            }

            //--------------------------------------------------------
            // BSYMMGR.InferContext.GetSlot
            //
            /// <summary>
            /// <para>Gets the map slot for the type var. Returns NULL if tvs is invalid.</para>
            /// <para>Find the inference mapping slot for the given type variable.
            /// Returns NULL if the type variable isn't a known one.</para>
            /// <para>In this program, return TYPESYM which tvs is mapped.</para>
            /// </summary>
            /// <remarks>
            /// TYPESYM ** BSYMMGR::InferContext::GetSlot(TYVARSYM *tvs)
            /// </remarks>
            /// <param name="tvs"></param>
            /// <returns></returns>
            //--------------------------------------------------------
            internal TYPESYM GetSlot(TYVARSYM tvs)
            {
                if (!tvs.IsMethodTypeVariable ||
                    tvs.TotalIndex >= MethodTypeVariables.Count ||
                    tvs != MethodTypeVariables[tvs.TotalIndex])
                {
                    return null;
                }
                return UnifiedMethodTypeVariables[tvs.TotalIndex];
            }

            //--------------------------------------------------------
            // BSYMMGR.InferContext.SetSlot
            //
            /// <summary>
            /// Set type to UnifiedMethodTypeVariables at the index of tvs.
            /// </summary>
            /// <param name="tvs"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            //--------------------------------------------------------
            internal bool SetSlot(TYVARSYM tvs, TYPESYM type)
            {
                if (!tvs.IsMethodTypeVariable ||
                    tvs.TotalIndex >= MethodTypeVariables.Count ||
                    tvs != MethodTypeVariables[tvs.TotalIndex])
                {
                    return false;
                }

                UnifiedMethodTypeVariables[tvs.TotalIndex] = type;
                return true;
            }
        }

        //============================================================
        // class BSYMMGR.StdTypeVarColl
        //
        /// <summary>
        /// <para>Standard method and class type variables, ie, !0, !1, !!0, !!1, etc.</para>
        /// <para>Store TYVARSYM instances and
        /// return one of instances with the specified index.</para>
        /// </summary>
        //============================================================
        internal class StdTypeVarColl
        {
            //internal int ctvs;
            //internal TYVARSYM prgptvs;
            internal List<TYVARSYM> TypeVariableList = null;

            //--------------------------------------------------------
            // BSYMMGR.StdTypeVarColl Constructor
            //
            /// <summary></summary>
            //--------------------------------------------------------
            internal StdTypeVarColl()
            {
                Init();
            }

            //--------------------------------------------------------
            // BSYMMGR.StdTypeVarColl.Init
            //
            /// <summary></summary>
            //--------------------------------------------------------
            internal void Init()
            {
                TypeVariableList = new List<TYVARSYM>();
                for (int i = 0; i < 8; ++i)
                {
                    TypeVariableList.Add(null);
                }
            }

            //--------------------------------------------------------
            // BSYMMGR.StdTypeVarColl.GetTypeVarSym
            //
            /// <summary>
            /// <para>Get the standard type variable (eg, !0, !1, or !!0, !!1).</para>
            /// <para>Get the TYVARSYM instance of the specified index.
            /// If it is null, create a TYVARSYM instance whose name and parent are null and return it.</para>
            /// </summary>
            /// <param name="index">Index.</param>
            /// <param name="bSymMgr">Containing symbol manager.</param>
            /// <param name="isMethod">Whether this is a method type var or class type var.</param>
            /// <returns></returns>
            /// <remarks>
            /// The standard class type variables are useful during emit, but not for type comparison
            /// when binding. The standard method type variables are useful during binding for
            /// signature comparison.
            /// </remarks>
            //--------------------------------------------------------
            internal TYVARSYM GetTypeVarSym(int index, BSYMMGR symMgr, bool isMethod)
            {
                DebugUtil.Assert(index >= 0 && index < 0x00001000);

                if (TypeVariableList == null)
                {
                    Init();
                }

                SYM sym = null;
                TYVARSYM tyVarSym = null;

                while (index >= TypeVariableList.Count)
                {
                    TypeVariableList.Add(null);
                }

                tyVarSym = TypeVariableList[index];
                if (tyVarSym == null)
                {
                    sym = symMgr.CreateGlobalSym(SYMKIND.TYVARSYM, null, null);
                    if (sym != null)
                    {
                        tyVarSym = sym as TYVARSYM;
                    }
                    DebugUtil.Assert(tyVarSym != null);

                    tyVarSym.IsMethodTypeVariable = isMethod;
                    tyVarSym.Index = index;
                    tyVarSym.TotalIndex = index;
                    TypeVariableList[index] = tyVarSym;
                }
                return tyVarSym;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR  Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// Assemblies in the global alias.
        /// </summary>
        private BitSet globalAssemblyBitset = new BitSet();

        /// <summary>
        /// (R) Assemblies in the global alias.
        /// </summary>
        internal BitSet GlobalAssemblyBitset
        {
            get { return globalAssemblyBitset; }
        }

        // Special Helper METHSYMs. These are found and set by FNCBIND. They are just cached here.

        internal AGGTYPESYM DictionaryAggTypeSym = null;        // AGGTYPESYM * atsDictionary;
        internal METHSYM DictionaryCtorMethSym = null;          // METHSYM * methDictionaryCtor;
        internal METHSYM DictionaryAddMethSym = null;           // METHSYM * methDictionaryAdd;
        internal METHSYM DictionaryTryGetValueMethSym = null;   // METHSYM * methDictionaryTryGetValue;
        internal METHSYM StringEqualsMethSym = null;            // METHSYM * methStringEquals;
        internal METHSYM InitArrayMethSym = null;               // METHSYM * methInitArray;
        internal METHSYM StringOffsetMethSym = null;            // METHSYM * methStringOffset;

        // Special nullable members.

        internal PROPSYM NullableValuePropertySym = null;       // PROPSYM * propNubValue;
        internal PROPSYM NullableHasValuePropertySym = null;    // PROPSYM * propNubHasValue;
        internal METHSYM NullableGetValOrDefMethodSym = null;   // METHSYM * methNubGetValOrDef;
        internal METHSYM NullableCtorMethodSym = null;          // METHSYM * methNubCtor;

        /// <summary>
        /// <para>Global SYMTBL. SYMTBL has SYM Dictionary whose key is (name, parent).</para>
        /// </summary>
        protected SYMTBL globalSymbolTable = new SYMTBL();  // SYMTBL tableGlobal;

        /// <summary>
        /// <para>(R) Global SYMTBL. SYMTBL has SYM Dictionary whose key is (name, parent).</para>
        /// </summary>
        internal SYMTBL GlobalSymbolTable
        {
            get { return globalSymbolTable; }
        }

        // AGGTYPESYM
        protected AGGTYPESYM arglistSym = null; // PAGGTYPESYM arglistSym;

        internal AGGTYPESYM ArgListSym  // GetArglistSym()
        {
            get { return arglistSym; }
        }

        protected AGGTYPESYM naturalIntSym = null;  // PAGGTYPESYM naturalIntSym;

        internal AGGTYPESYM NaturalIntTypeSym
        {
            get { return naturalIntSym; }   // GetNaturalIntSym()
        }

        protected ERRORSYM errorSym = null; // PERRORSYM errorSym;

        internal ERRORSYM ErrorSym  // GetErrorSym()
        {
            get { return errorSym; }
        }
        
        //

        protected VOIDSYM voidSym = null;   // PVOIDSYM voidSym;

        internal VOIDSYM VoidSym    // GetVoid()
        {
            get { return voidSym; }
        }

        protected NULLSYM nullSym = null;  // PNULLSYM nullType;

        internal NULLSYM NullSym    // GetNullType()
        {
            get { return nullSym; }
        }

        protected UNITSYM unitSym;  // UNITSYM * typeUnit;

        internal UNITSYM UnitSym    // GetUnitType()
        {
            get { return unitSym; }
        }

        protected ANONMETHSYM anonymousMethodSym;   // ANONMETHSYM * typeAnonMeth;

        internal ANONMETHSYM AnonymousMethodSym // GetAnonMethType()
        {
            get { return anonymousMethodSym; }
        }

        protected LAMBDAEXPRSYM lambdaExpressionSym;

        internal LAMBDAEXPRSYM LambdaExpressionSym
        {
            get { return lambdaExpressionSym; }
        }

        protected METHGRPSYM methodGroupSym;    //METHGRPSYM * typeMethGrp;

        internal METHGRPSYM MethodGroupTypeSym  // GetMethGrpType()
        {
            get { return methodGroupSym; }
        }

        protected AGGSYM nullableAggSym = null;

        internal AGGSYM NullableAggSym
        {
            get { return nullableAggSym; }
        }

        // Some AGGTYPESYMs in System namespaces.
        // These syms are assigned by InitFundamentalTypes method.

        protected AGGTYPESYM objectTypeSym = null;

        internal AGGTYPESYM ObjectTypeSym
        {
            get { return objectTypeSym; }
        }

        protected AGGTYPESYM intTypeSym = null;

        internal AGGTYPESYM IntTypeSym
        {
            get { return intTypeSym; }
        }

        protected AGGTYPESYM boolTypeSym = null;

        internal AGGTYPESYM BoolTypeSym
        {
            get { return boolTypeSym; }
        }

        /// <summary>
        /// <para>(CS3)</para>
        /// </summary>
        protected IMPLICITTYPESYM implicitTypeSym = null;

        internal IMPLICITTYPESYM ImplicitTypeSym
        {
            get { return this.implicitTypeSym; }
        }

        /// <summary>
        /// (CS4)
        /// </summary>
        protected DYNAMICSYM dynamicSym = null;

        internal DYNAMICSYM DynamicSym
        {
            get { return this.dynamicSym; }
        }

        /// <remarks>
        /// 2016/03/30 hirano567@hotmail.co.jp
        /// </remarks>
        protected AGGSYM genericArraySym = null;

        /// <summary>
        /// Array types implement
        ///     System.Collections.Generic.IList`1,
        ///     System.Collections.Generic.ICollection`1,
        ///     System.Collections.Generic.IEnumerable`1.
        /// But, System.Array does not implement them.
        /// GenericArraySym is to implement these interfaces.
        /// </summary>
        internal AGGSYM GenericArraySym
        {
            get { return this.genericArraySym; }
        }

        /// <summary>
        /// <para>The "root" (unnamed) namespace.</para>
        /// </summary>
        protected NSSYM rootNamespaceSym = null;    // PNSSYM rootNS;

        /// <summary>
        /// <para>(R) The "root" (unnamed) namespace.</para>
        /// </summary>
        internal NSSYM RootNamespaceSym // GetRootNS()
        {
            get { return rootNamespaceSym; }
        }

        /// <summary>
        /// global::
        /// </summary>
        protected NSAIDSYM globalNsAidSym = null;   // NSAIDSYM * nsaGlobal;

        /// <summary>
        /// (R) global::
        /// </summary>
        internal NSAIDSYM GlobalNsAidSym
        {
            get { return globalNsAidSym; }
        }

        /// <summary>
        /// All output and input file symbols rooted here.
        /// </summary>
        protected SCOPESYM fileRootSym = null;  // PSCOPESYM   fileroot;

        /// <summary>
        /// <para>(R) 入出力ファイルのルートとなる SCOPESYM</para>
        /// <para>実際には出力ファイルがこの SYM の子となり、
        /// 入力ファイルは出力ファイルの子となっている。</para>
        /// </summary>
        internal SCOPESYM FileRootSym
        {
            get { return fileRootSym; }
        }

        protected Dictionary<PredefTypeNameInfo, PREDEFTYPE> predefTypeNameDictionary = null;   // PredefTypeNameInfo * prgptni;

        /// <summary>
        /// <para>Array of predefined symbol types created by the informations of mscorlib.</para>
        /// <para>Created in BSYMMGR.InitPredefinedTypes,
        /// which is called by COMPILER.CompileAll method.</para>
        /// </summary>
        protected AGGSYM[] predefSyms = null;   // PAGGSYM * predefSyms;

        protected AGGSYM arrayMethHolder = null;    // PAGGSYM * arrayMethHolder;

        /// <summary>
        /// The dummy output file for all imported metadata files
        /// </summary>
        protected OUTFILESYM metadataFileRootSym;    // POUTFILESYM mdfileroot;

        /// <summary>
        /// (R) OUTFILESYM instance to which register metadata files.
        /// </summary>
        internal OUTFILESYM MetadataFileRootSym
        {
            get { return metadataFileRootSym; } // GetMDFileRoot()
        }

        /// <summary>
        /// The dummy output file for all included XML files
        /// </summary>
        protected PARENTSYM xmlFileRootSym = null;  // PPARENTSYM  xmlfileroot;

        /// <summary>
        /// (R) The dummy output file for all included XML files
        /// </summary>
        internal PARENTSYM XmlFileRootSym
        {
            get { return xmlFileRootSym; }
        }

        /// <summary>
        /// <para>Map from assemblyIds to INFILESYMs and EXTERNALIASSYMs</para>
        /// <para>Has List&lt;SYM&gt; of INFILESYM and EXTERNALALIASSYM. 
        /// Its indice are converted to assemblyIDs.</para>
        /// </summary>
        protected SymSet infileSymSet = new SymSet();   // SymSet ssetAssembly;
        
        /// <summary>
        /// <para>Map from assemblyIds to MODULESYMs and OUTFILESYMs</para>
        /// <para>Has List&lt;SYM&gt; of MODULESYM and OUTFILESYM. 
        /// Its indice are converted to assemblyIDs.</para>
        /// </summary>
        protected SymSet moduleSymSet = new SymSet();   // SymSet ssetModule;

        /// <summary>
        /// <para>This Dictionary maps SystemReflection.Assembly instances
        /// to INFILESYM instances.</para>
        /// </summary>
        protected Dictionary<Assembly, INFILESYM> assemblyToInfileSymDictionary
            = new Dictionary<Assembly, INFILESYM>();

        /// <summary>
        /// <para>(R) This Dictionary maps SystemReflection.Assembly instances
        /// to INFILESYM instances.</para>
        /// </summary>
        internal Dictionary<Assembly, INFILESYM> AssemblyToInfileSymDictionary
        {
            get { return this.assemblyToInfileSymDictionary; }
        }

        /// <summary>
        /// The assembly ID for all predefined types.
        /// </summary>
        internal int MsCorLibAssemblyID = Kaid.Nil;   // int aidMsCorLib;

        /// <summary>
        /// The INFILESYM instance of mscorlib.dll.
        /// </summary>
        /// <remarks>(2015/01/14 hirano567@hotmail.co.jp)</remarks>
        protected INFILESYM msCorLibSym = null;

        /// <summary>
        /// (RW) The INFILESYM instance of mscorlib.dll.
        /// </summary>
        /// <remarks>(2015/01/14 hirano567@hotmail.co.jp)</remarks>
        internal INFILESYM MsCorLibSym
        {
            get { return msCorLibSym; }
            set
            {
                if (value != null)
                {
                    msCorLibSym = value;
                    MsCorLibAssemblyID = msCorLibSym.GetAssemblyID();
                }
                else
                {
                    msCorLibSym = null;
                    MsCorLibAssemblyID = Kaid.Nil;
                }
            }
        }

        /// <summary>
        /// The INFILESYM instance of system.dll.
        /// </summary>
        /// <remarks>(2015/01/14 hirano567@hotmail.co.jp)</remarks>
        internal INFILESYM SystemDllSym = null;

        /// <summary>
        /// The INFILESYM instance of system.core.dll.
        /// </summary>
        /// <remarks>(2016/02/03 hirano567@hotmail.co.jp)</remarks>
        internal INFILESYM SystemCoreDllSym = null;

        /// <summary>
        /// The INFILESYM instance of Microsoft.CSharp.dll.
        /// </summary>
        /// <remarks>(2016/02/03 hirano567@hotmail.co.jp)</remarks>
        internal INFILESYM MicrosoftCSharpDllSym = null;

        //protected TokenToSymTable tableTokenToSym = new TokenToSymTable();    // TokenToSymTable tableTokenToSym;
        //internal TokenToSymTable TokenToSymbolTable { get { return tableTokenToSym; } }

        protected InfoToSymTable infoToSymbolTable = new InfoToSymTable();

        internal InfoToSymTable InfoToSymbolTable
        {
            get { return infoToSymbolTable; }
        }

        protected NameToSymTable nameToSymbolTable = new NameToSymTable(); // NameToSymTable tableNameToSym;

        internal NameToSymTable NameToSymbolTable
        {
            get { return nameToSymbolTable; }
        }

        protected StdTypeVarColl stdMethodTypeVariables = new StdTypeVarColl(); // StdTypeVarColl stvcMethod;
        protected StdTypeVarColl stdClassTypeVariables = new StdTypeVarColl();  // StdTypeVarColl stvcClass;

        /// <summary>
        /// The hash table for type arrays.
        /// </summary>
        protected TypeArrayTable typeArrayTable = new TypeArrayTable();    // TypeArrayTable tableTypeArrays;

        /// <summary>
        /// (R) The hash table for type arrays.
        /// </summary>
        internal TypeArrayTable TypeArrayTable
        {
            get { return typeArrayTable; }
        }

        /// <summary>
        /// TypeArray with no element. Clear by Init() method before use just in case.
        /// </summary>
        private static readonly TypeArray emptyTypeArray = new TypeArray();    // static TypeArray taEmpty;

        /// <summary>
        /// (R) TypeArray with no element. Clear by Init() method before use just in case.
        /// </summary>
        internal static TypeArray EmptyTypeArray    // EmptyTypeArray()
        {
            get
            {
                emptyTypeArray.Init();
                return emptyTypeArray;
            }
        }

        protected COMPILER compiler = null;

        internal COMPILER Compiler
        {
            get { return compiler; }
        }

        //internal BSYMHOST Host
        //{
        //    get { return this.compiler; }   // BSYMHOST host()
        //}

        internal SYMMGR SymMgr
        {
            get { return this as SYMMGR; }  // getSymmgr()
        }

        //------------------------------------------------------------
        // BSYMMGR  Constructor (static)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        static BSYMMGR()
        {
#if DEBUG
            emptyTypeArray.debugComment = "BSYMMGR.emptyTypeArray";
#endif
        }

        //------------------------------------------------------------
        // BSYMMGR  Constructor
        //
        /// <summary></summary>
        /// <param name="comp"></param>
        //------------------------------------------------------------
        internal BSYMMGR(COMPILER comp)
        {
            // Fill in the rest of taEmpty.
            // ASSERT(!taEmpty.size);
            // ASSERT(!taEmpty.tok);
            this.compiler = comp;
            emptyTypeArray.AggState = AggStateEnum.Last;
            GlobalAssemblyBitset.SetBit((int)Kaid.ThisAssembly);
        }

        //------------------------------------------------------------
        // BSYMMGR.Init
        //
        /// <summary>
        /// Initialize a bunch of pre-defined symbols and such.
        /// </summary>
        //------------------------------------------------------------
        internal void Init()
        {
            TypeArrayTable.Init();
            //tableTokenToSym.Init();
            InfoToSymbolTable.Init();
            NameToSymbolTable.Init();
            infileSymSet.Init();
            moduleSymSet.Init();

            // 'void' and 'null' are special types with their own symbol kind.

            errorSym = CreateGlobalSym(SYMKIND.ERRORSYM, null, null) as ERRORSYM;
            errorSym.Access = ACCESS.PUBLIC;
            errorSym.AggState = AggStateEnum.PreparedMembers;
            errorSym.HasErrors = true;

            voidSym = CreateGlobalSym(SYMKIND.VOIDSYM, null, null) as VOIDSYM;
            voidSym.Access = ACCESS.PUBLIC;
            voidSym.AggState = AggStateEnum.PreparedMembers;

            nullSym = CreateGlobalSym(SYMKIND.NULLSYM, null, null) as NULLSYM;
            nullSym.Access = ACCESS.PUBLIC;
            nullSym.AggState = AggStateEnum.PreparedMembers;

            unitSym = CreateGlobalSym(SYMKIND.UNITSYM, null, null) as UNITSYM;
            unitSym.Access = ACCESS.PUBLIC;
            unitSym.AggState = AggStateEnum.PreparedMembers;

            anonymousMethodSym = CreateGlobalSym(SYMKIND.ANONMETHSYM, null, null) as ANONMETHSYM;
            anonymousMethodSym.Access = ACCESS.PUBLIC;
            anonymousMethodSym.AggState = AggStateEnum.PreparedMembers;

            lambdaExpressionSym = CreateGlobalSym(SYMKIND.LAMBDAEXPRSYM, null, null) as LAMBDAEXPRSYM;
            lambdaExpressionSym.Access = ACCESS.PUBLIC;
            lambdaExpressionSym.AggState = AggStateEnum.PreparedMembers;

            methodGroupSym = CreateGlobalSym(SYMKIND.METHGRPSYM, null, null) as METHGRPSYM;
            methodGroupSym.Access = ACCESS.PUBLIC;
            methodGroupSym.AggState = AggStateEnum.PreparedMembers;

            // CS3
            implicitTypeSym = CreateGlobalSym(SYMKIND.IMPLICITTYPESYM, null, null) as IMPLICITTYPESYM;
            implicitTypeSym.Access = ACCESS.PUBLIC;
            implicitTypeSym.AggState = AggStateEnum.PreparedMembers;

            // CS4
            //dynamicSym = CreateGlobalSym(SYMKIND.DYNAMICSYM, null, null) as DYNAMICSYM;
            //dynamicSym.Access = ACCESS.PUBLIC;
            //dynamicSym.AggState = AggStateEnum.PreparedMembers;

            //--------------------------------------------------
            // create the varargs type symbol:
            //--------------------------------------------------
            AGGSYM arglistAgg = CreateGlobalSym(SYMKIND.AGGSYM,
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.ARGLIST), null) as AGGSYM;
            arglistAgg.SymbolManager = this;
            arglistAgg.AggState = AggStateEnum.PreparedMembers;
            arglistAgg.IsSealed = true;
            arglistAgg.Access = ACCESS.PUBLIC;
            arglistAgg.SetBogus(false);
            arglistAgg.Interfaces = BSYMMGR.EmptyTypeArray;
            arglistAgg.AllInterfaces = BSYMMGR.EmptyTypeArray;
            arglistAgg.TypeVariables = BSYMMGR.EmptyTypeArray;
            arglistAgg.AllTypeVariables = BSYMMGR.EmptyTypeArray;
            arglistSym = arglistAgg.GetThisType();

            //--------------------------------------------------
            // create the natural int type symbol:
            //--------------------------------------------------
            AGGSYM naturalIntSymAgg = CreateGlobalSym(SYMKIND.AGGSYM,
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.NATURALINT), null) as AGGSYM;
            naturalIntSymAgg.SymbolManager = this;
            naturalIntSymAgg.AggState = AggStateEnum.PreparedMembers;
            naturalIntSymAgg.IsSealed = true;
            naturalIntSymAgg.Access = ACCESS.PUBLIC;
            naturalIntSymAgg.SetBogus(false);
            naturalIntSymAgg.Interfaces = BSYMMGR.EmptyTypeArray;
            naturalIntSymAgg.AllInterfaces = BSYMMGR.EmptyTypeArray;
            naturalIntSymAgg.TypeVariables = BSYMMGR.EmptyTypeArray;
            naturalIntSymAgg.AllTypeVariables = BSYMMGR.EmptyTypeArray;
            naturalIntSym = naturalIntSymAgg.GetThisType();

            //--------------------------------------------------
            // Some root symbols.
            //--------------------------------------------------
            string emptyName = ""; Compiler.NameManager.AddString(emptyName);

            // Root namespace
            rootNamespaceSym = CreateNamespace(emptyName, null);

            globalNsAidSym = GetNsAid(rootNamespaceSym, (int)Kaid.Global);

            // Root of file symbols.
            fileRootSym = CreateGlobalSym(SYMKIND.SCOPESYM, null, null) as SCOPESYM;

            // Create mdfileroot as a child of fileroot.
            metadataFileRootSym = CreateGlobalSym(SYMKIND.OUTFILESYM, emptyName, fileRootSym) as OUTFILESYM;

            // Root of predefined included XML files
            xmlFileRootSym = CreateGlobalSym(SYMKIND.SCOPESYM, null, null) as SCOPESYM;

            INFILESYM infileUnres = CreateGlobalSym(SYMKIND.INFILESYM, emptyName, null) as INFILESYM;
            infileUnres.IsSource = false;
            infileUnres.SetAssemblyID((int)Kaid.Unresolved);
            infileUnres.LocalAssemblyID = 0;    // mdTokenNil;
            int isym = infileSymSet.AddSym(infileUnres);
            DebugUtil.Assert(isym == 0);

            InitPreLoad();

#if DEBUG
            this.errorSym.DebugComment = "BSYMMGR.errorSym";
            this.VoidSym.DebugComment = "BSYMMGR.voidSym";
            this.nullSym.DebugComment = "BSYMMGR.nullSym";
            this.unitSym.DebugComment = "BSYMMGR.unitSym";
            this.anonymousMethodSym.DebugComment = "BSYMMGR.anonymousMethodSym";
            this.methodGroupSym.DebugComment = "BSYMMGR.methodGroupSym";
            this.rootNamespaceSym.DebugComment = "BSYMMGR.rootNamespaceSym";
            this.globalNsAidSym.DebugComment = "BSYMMGR.globalNsAidSym";
            this.fileRootSym.DebugComment = "BSYMMGR.fileRootSym";
            this.metadataFileRootSym.DebugComment = "BSYMMGR.metadataFileRootSym";
            this.xmlFileRootSym.DebugComment = "BSYMMGR.xmlFileRootSym";
#endif
        }

        //------------------------------------------------------------
        // BSYMMGR.Term
        //
        /// <summary>
        /// Free all memory associated with the symbol manager.
        /// </summary>
        //------------------------------------------------------------
        internal void Term()
        {
        
            //ReleaseXMLDocuments(xmlfileroot); // この関数は何もしない。
            xmlFileRootSym = null;

            GlobalSymbolTable.Term();
            InfoToSymbolTable.Term();
            NameToSymbolTable.Term();
            //tableTokenToSym.Term();
            TypeArrayTable.Term();
            infileSymSet.Term();
            moduleSymSet.Term();
        }

        //------------------------------------------------------------
        // BSYMMGR.AddToAssemblyToInfileSymDictionary
        //
        /// <summary></summary>
        /// <param name="asm"></param>
        /// <param name="infileSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AddToAssemblyToInfileSymDictionary(
            Assembly asm,
            INFILESYM infileSym)
        {
            try
            {
                this.assemblyToInfileSymDictionary.Add(asm, infileSym);
                return true;
            }
            catch (ArgumentException)
            {
                // error message
            }
            return false;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetInfileSymByAssembly
        //
        /// <summary></summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM GetInfileSymByAssembly(Assembly asm)
        {
            if (asm == null)
            {
                return null;
            }
            INFILESYM infileSym = null;

            try
            {
                if (this.assemblyToInfileSymDictionary.TryGetValue(asm, out infileSym))
                {
                    return infileSym;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
            }
            return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.InitPredefinedTypes
        //
        /// <summary>
        /// <para>Initialize the predefined types.</para>
        /// <para>If we haven't found mscorlib yet, we first look for System.Object in kaidGlobal
        /// and set aidMsCorLib to the assembly containing it.</para>
        /// <para>We look in both aidMsCorLib and kaidGlobal for all predefined types.
        /// If the type isn't in aidMsCorLib but is in kaidGlobal we produce a warning and use the one we found.
        /// If we find the type in aidMsCorLib and find another type with the same fully qualified name in kaidGlobal,
        /// we warn and use the one in aidMsCorLib.</para>
        /// <para>Returns true if all of the required types are found, false otherwise</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool InitPredefinedTypes()
        {
            DebugUtil.Assert(this.MsCorLibSym != null);

            if (Compiler.OptionManager.NoStdLib && MsCorLibSym.AssemblyEx == null)
            {
                Compiler.Error(CSCERRID.ERR_NoStdLib, new ErrArg("mscorlib.dll"));
                return false;
            }

            CAssemblyEx msCorLib = MsCorLibSym.AssemblyEx;
            CAssemblyEx systemDll = (this.SystemDllSym != null) ?
                this.SystemDllSym.AssemblyEx : null;
            CAssemblyEx systemCoreDll = (this.SystemCoreDllSym != null) ?
                this.SystemCoreDllSym.AssemblyEx : null;
            CAssemblyEx microsoftCSharpDll = (this.MicrosoftCSharpDllSym != null) ?
                this.MicrosoftCSharpDllSym.AssemblyEx : null;

            if (MsCorLibAssemblyID == Kaid.Nil)
            {
                // If we haven't found mscorlib yet, first look for System.Object.
                // Then use its assembly as the location for all other pre-defined types.
                AGGSYM aggObj = FindPredefinedType(
                    PredefType.InfoTable[(int)PREDEFTYPE.OBJECT].fullName,
                    Kaid.Global,
                    AggKindEnum.Class,
                    0,
                    true);
                if (aggObj == null)
                {
                    return false;
                }
                MsCorLibAssemblyID = aggObj.GetAssemblyID();
            }

            if (MsCorLibAssemblyID != Kaid.ThisAssembly)
            {
                GetInfileForAid(MsCorLibAssemblyID).IsBaseClassLibrary = true;
            }
        
            bool allRequiredFound = true;
            predefSyms = new AGGSYM[(int)PREDEFTYPE.COUNT];

            //--------------------------------------------------
            // For each predefined type,
            //--------------------------------------------------
            PredefTypeInfo ptInfo;
            AGGSYM sym;
            Type type;
            string searchName;

#if DEBUG
            StringBuilder debugSb = new StringBuilder();
            bool debugAdded;
#endif

            for (int i = 0; i < (int)PREDEFTYPE.COUNT; ++i)
            {
#if DEBUG
                if (i >= 129)
                {
                    ;
                }
#endif
                ptInfo = PredefType.InfoTable[i];
                if (ptInfo.fullName == null)
                {
                    continue;
                }

                sym = null;
                if (ptInfo.arity > 0)
                {
                    searchName = IMPORTER.CreateAggNameWithArity(ptInfo.fullName, ptInfo.arity);
                }
                else
                {
                    searchName = ptInfo.fullName;
                }

                type = msCorLib.GetType(searchName);
                if (type == null && systemDll != null)
                {
                    type = systemDll.GetType(searchName);
                }
                if (type == null && systemCoreDll != null)
                {
                    type = systemCoreDll.GetType(searchName);
                }
                if (type == null && microsoftCSharpDll != null)
                {
                    type = microsoftCSharpDll.GetType(searchName);
                }

                if (type != null)
                {
                    sym = Compiler.Importer.ImportOneType(msCorLibSym, type) as AGGSYM;
                }

                if (sym == null)
                {
                    // Not found in mscorlib. Look in all of global.
                    sym = FindPredefinedType(
                        ptInfo.fullName, Kaid.Global, ptInfo.aggKind, ptInfo.arity, ptInfo.isRequired);
                    if (sym != null && ptInfo.inmscorlib != 0)
                    {
                        // Warn that the type isn't where expected.
                        ErrArg arg;
                        if (MsCorLibAssemblyID == Kaid.ThisAssembly)
                        {
                            arg = new ErrArgResNo(ResNo.CSCSTR_ThisAssembly);
                            // CSCSTRID.ThisAssembly);
                        }
                        else
                        {
                            arg = new ErrArg(GetInfileForAid(MsCorLibAssemblyID));
                            //ASSERT(arg.sym);
                        }
                        Error(CSCERRID.WRN_UnexpectedPredefTypeLoc,
                            new ErrArg(sym),
                            arg,
                            new ErrArg(sym.FirstDeclSym.GetInputFile()));
                    }
                }

                if (sym != null)
                {
                    //ASSERT(sym.AggKind() == ptInfo.aggKind && sym.typeVarsAll.size == ptInfo.arity);

                    sym.IsPredefinedType = true;
                    sym.PredefinedTypeID = (PREDEFTYPE)i;
                    //ASSERT(sym.iPredef == (unsigned)i);  Assert that the bitfield is large enough.
                    sym.SkipUserDefinedOperators = (
                        i <= (int)PREDEFTYPE.ENUM &&
                        i != (int)PREDEFTYPE.INTPTR &&
                        i != (int)PREDEFTYPE.UINTPTR);
                }
                else if (ptInfo.isRequired)
                {
                    // We still want to report all of the missing required types!
                    allRequiredFound = false;
                }

                predefSyms[i] = sym;
            }
            if (!allRequiredFound) {
                // Don't bother continuing -- we're dead in the water.
                return false;
            }

            // set up the root of the attribute hierarchy
            sym = GetReqPredefAgg(PREDEFTYPE.ATTRIBUTE);
            sym.IsAttribute = true;
        
            // need to set this up because the attribute class has the attribute usage attribute on it
            sym = GetOptPredefAgg(PREDEFTYPE.ATTRIBUTEUSAGE);
            if (sym != null) sym.AttributeClass = AttributeTargets.Class;
            if (sym != null) sym.IsAttribute = true;
        
            sym = GetOptPredefAgg(PREDEFTYPE.ATTRIBUTETARGETS);
            if (sym != null) sym.AggKind = AggKindEnum.Enum;
        
            sym = GetOptPredefAgg(PREDEFTYPE.CONDITIONAL);
            if (sym != null) sym.IsAttribute = true;
            if (sym != null) sym.AttributeClass = AttributeTargets.Method;

            sym = GetOptPredefAgg(PREDEFTYPE.OBSOLETE);
            if (sym != null) sym.IsAttribute = true;
            if (sym != null) sym.AttributeClass = AttributeTargets.All;

            sym = GetOptPredefAgg(PREDEFTYPE.CLSCOMPLIANT);
            if (sym != null) sym.IsAttribute = true;
            if (sym != null)
                sym.AttributeClass = (AttributeTargets)(
                    AttributeTargets.Assembly | Cor.AttributeTargetClassMembers);

            // set up the root of the security attribute hierarchy
            sym = GetOptPredefAgg(PREDEFTYPE.SECURITYATTRIBUTE);
            if (sym != null) sym.IsSecurityAttribute = true;

            // set up the root of for marshalbyref types.
            sym = GetOptPredefAgg(PREDEFTYPE.MARSHALBYREF);
            if (sym != null) sym.IsMarshalByRef = true;
            sym = GetOptPredefAgg(PREDEFTYPE.CONTEXTBOUND);
            if (sym != null) sym.IsMarshalByRef = true;
        
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.FPreLoad
        //
        /// <summary>
        /// The importer calls this to determine whether a type needs to be loaded immediately.
        /// This is so predefined types are all loaded before InitPredefTypes is called.
        /// </summary>
        /// <param name="nsSym"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool FPreLoad(NSSYM nsSym, string typeName)
        {
            DebugUtil.Assert(this.predefTypeNameDictionary != null);

            if (!nsSym.HasPredefineds)
            {
                return false;
            }

            try
            {
                return this.predefTypeNameDictionary.ContainsKey(new PredefTypeNameInfo(nsSym, typeName));
            }
            catch (ArgumentException)
            {
            }
            return false;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateGlobalSym
        //
        /// <summary>
        /// <para>Core general creation and lookup of symbols.</para>
        /// <para>The main routine for creating a global symbol and
        /// putting it into the symbol table under a particular parent.
        /// Either name or parent can be NULL.</para>
        /// <para>Create SYM instance of the specified type (not search).
        /// <list type="bullet">
        /// <item>Set isLocal false.</item>
        /// <item>If parent is not null, register to globalSymbolTable and
        /// add to the child list of parent.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="symkind"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM CreateGlobalSym(SYMKIND symkind, string name, PARENTSYM parent)
        {
            SYM sym = null;

#if DEBUG
            // Only some symbol kinds are valid as global symbols. Validate.
            switch (symkind)
            {
                case SYMKIND.NSSYM:	// if (1) break; goto LFail;
                case SYMKIND.NSDECLSYM:	// if (1) break; goto LFail;
                case SYMKIND.NSAIDSYM:	// if (1) break; goto LFail;
                case SYMKIND.AGGSYM:	// if (1) break; goto LFail;
                case SYMKIND.AGGDECLSYM:	// if (1) break; goto LFail;
                case SYMKIND.AGGTYPESYM:	// if (1) break; goto LFail;
                case SYMKIND.FWDAGGSYM:	// if (1) break; goto LFail;
                case SYMKIND.TYVARSYM:	// if (1) break; goto LFail;
                case SYMKIND.MEMBVARSYM:	// if (1) break; goto LFail;
                case SYMKIND.METHSYM:	// if (1) break; goto LFail;
                case SYMKIND.FAKEMETHSYM:	// if (1) break; goto LFail; 
                case SYMKIND.PROPSYM:	// if (1) break; goto LFail;
                case SYMKIND.EVENTSYM:	// if (1) break; goto LFail;
                case SYMKIND.VOIDSYM:	// if (1) break; goto LFail;
                case SYMKIND.NULLSYM:	// if (1) break; goto LFail;
                case SYMKIND.UNITSYM:	// if (1) break; goto LFail;
                case SYMKIND.ANONMETHSYM:	// if (1) break; goto LFail;
                case SYMKIND.METHGRPSYM:	// if (1) break; goto LFail;
                case SYMKIND.ERRORSYM:	// if (1) break; goto LFail;
                case SYMKIND.ARRAYSYM:	// if (1) break; goto LFail;
                case SYMKIND.PTRSYM:	// if (1) break; goto LFail;
                case SYMKIND.PINNEDSYM:	// if (1) break; goto LFail;
                case SYMKIND.PARAMMODSYM:	// if (1) break; goto LFail;
                case SYMKIND.MODOPTSYM:	// if (1) break; goto LFail;
                case SYMKIND.MODOPTTYPESYM:	// if (1) break; goto LFail;
                case SYMKIND.NUBSYM:	// if (1) break; goto LFail; 
                case SYMKIND.INFILESYM:	// if (1) break; goto LFail;
                case SYMKIND.MODULESYM:	// if (1) break; goto LFail;
                case SYMKIND.RESFILESYM:	// if (1) break; goto LFail;
                case SYMKIND.OUTFILESYM:	// if (1) break; goto LFail;
                case SYMKIND.XMLFILESYM:	// if (1) break; goto LFail;
                case SYMKIND.SYNTHINFILESYM:	// if (1) break; goto LFail;
                case SYMKIND.ALIASSYM:	// if (1) break; goto LFail;
                case SYMKIND.EXTERNALIASSYM:	// if (1) break; goto LFail;
                case SYMKIND.SCOPESYM:	// if (1) break; goto LFail;
                case SYMKIND.MISCSYM:	// if (1) break; goto LFail;
                case SYMKIND.GLOBALATTRSYM:	// if (1) break; goto LFail;
                case SYMKIND.UNRESAGGSYM:	// if (1) break; goto LFail;
                case SYMKIND.IFACEIMPLMETHSYM:	// if (1) break; goto LFail;
                case SYMKIND.INDEXERSYM:	// if (1) break; goto LFail;
                // CS3
                case SYMKIND.IMPLICITTYPESYM:
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM:
                case SYMKIND.LAMBDAEXPRSYM:
                // CS4
                case SYMKIND.DYNAMICSYM:
                    break;

                case SYMKIND.LOCVARSYM:	// if (0) break; goto LFail;
                case SYMKIND.CACHESYM:	// if (0) break; goto LFail;
                case SYMKIND.LABELSYM:	// if (0) break; goto LFail;
                case SYMKIND.ANONSCOPESYM:	// if (0) break; goto LFail;
                default:
                    //LFail:
                    DebugUtil.Assert(false, "Bad symkind in CreateGlobalSym");
                    break;
            }
#endif

            // Allocate the symbol from the global allocator and fill in the name member.
            sym = AllocSym(symkind, name);
            DebugUtil.Assert(!sym.IsLocal);

            if (parent != null)
            {
                // Set the parent element of the child symbol.
                AddChild(GlobalSymbolTable, parent, sym);
            }
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.LookupAggMember
        //
        /// <summary>
        /// <para>Find a member of the given AGGSYM.
        /// This never has to be concerned about filtering on assemblyId.</para>
        /// <para>Search in globalSymbolTable.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="agg">Parent AGGSYM instance.</param>
        /// <param name="mask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM LookupAggMember(string name, AGGSYM agg, SYMBMASK mask)
        {
            return GlobalSymbolTable.LookupSym(name, agg, mask);
        }

        //------------------------------------------------------------
        // BSYMMGR.LookupGlobalSymCore
        //
        /// <summary>
        /// <para>The main routine for looking up a global symbol by name.
        /// It's possible for there to be more that one symbol
        /// with a particular name in a particular parent;
        /// if you want to check for more, then use LookupNextSym.</para>
        /// <para>kindmask filters the result by particular symbol kinds.</para>
        /// <para>returns NULL if no match found.</para>
        /// <para>Search in globalSymbolTable.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="kindmask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM LookupGlobalSymCore(string name, PARENTSYM parent, SYMBMASK kindmask)
        {
            return GlobalSymbolTable.LookupSym(name, parent, kindmask);
        }

        //------------------------------------------------------------
        // BSYMMGR.LookupNextSym
        //
        /// <summary>
        /// <para>Look up the next symbol with the same name and parent.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="parent"></param>
        /// <param name="kindmask"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal SYM LookupNextSym(SYM sym, PARENTSYM parent, SYMBMASK kindmask)
        {
            DebugUtil.Assert(sym.ParentSym == parent);

            sym = sym.NextSameNameSym;
            DebugUtil.Assert(sym == null || sym.ParentSym == parent);

            // Keep traversing the list of symbols with same name and parent.
            while (sym != null)
            {
                if ((kindmask & sym.Mask) != 0)
                {
                    return sym;
                }
                sym = sym.NextSameNameSym;
                DebugUtil.Assert(sym == null || sym.ParentSym == parent);
            }
            return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.LookupNextGlobalMiscSym
        //------------------------------------------------------------
        internal MISCSYM LookupNextGlobalMiscSym(
            MISCSYM prev, string name, PARENTSYM parent, MISCSYM.TYPE type)
        //MISCSYM * BSYMMGR::LookupNextGlobalMiscSym(
        //    MISCSYM * prev, NAME * name, PARENTSYM * parent, MISCSYM::TYPE type)
        {
            throw new NotImplementedException("BSYMMGR.LookupNextGlobalMiscSym");

            //    if (prev) {
            //AGAIN:
            //        prev = LookupNextSym(prev, parent, MASK_MISCSYM).AsMISCSYM;
            //    } else {
            //        prev = LookupGlobalSymCore(name, parent, MASK_MISCSYM).AsMISCSYM;
            //    }
            //    if (prev) {
            //        if (prev.miscKind == type) {
            //            return prev;
            //        }
            //        goto AGAIN;
            //    }
            //    return NULL;
        }

        //------------------------------------------------------------
        // BSYMMGR.LookupInvokeMeth
        //
        /// <summary>
        /// Find the invoke method of the delegate AGGSYM.
        /// </summary>
        /// <param name="delegateSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal METHSYM LookupInvokeMeth(AGGSYM delegateSym)
        {
            DebugUtil.Assert(delegateSym.AggKind == AggKindEnum.Delegate);

            SYM sym = LookupAggMember(
                Compiler.NameManager.GetPredefinedName(PREDEFNAME.INVOKE),
                delegateSym,
                SYMBMASK.ALL);

            for (; sym != null; sym = sym.NextSameNameSym)
            {
                if (sym.IsMETHSYM && (sym as METHSYM).IsInvoke)
                {
                    return (sym as METHSYM);
                }
            }
            return null;
        }

        // Specific routines for specific symbol types.

        //------------------------------------------------------------
        // BSYMMGR.CreateNamespace
        //
        /// <summary>
        /// <para>If a namespace with the same name and same parent is in globalSymbolTable, return it.
        /// Or create a namespace symbol, and register it if parent is not null.</para>
        /// <para>Do not call this method if this namespace with the same parent has already registered.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NSSYM CreateNamespace(string name, NSSYM parent)
        {
            // Caller should make sure the namespace doesn't already exist.
            DebugUtil.Assert(LookupGlobalSymCore(name, parent, SYMBMASK.NSSYM) == null);

            // create and initialize the symbol.
            //
            // NOTE: we don't use CreateGlobalSym() because we don't want
            //       to link the new SYM into its parent's list of children

            NSSYM ns = AllocSym(SYMKIND.NSSYM, name) as NSSYM;

            // namespaces all have public access
            ns.Access = ACCESS.PUBLIC;

            if (parent != null)
            {
                GlobalSymbolTable.InsertChild(parent, ns);
            }
            return ns;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateAlias
        //
        /// <summary>
        /// Create a symbol for a using alias clause.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ALIASSYM CreateAlias(string name)
        {
            // Create the new symbol.
            return CreateGlobalSym(SYMKIND.ALIASSYM, name, null) as ALIASSYM;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateAgg
        //
        /// <summary>
        /// <para>Create a symbol for an aggregate type: class, struct, interface, or enum.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="outerDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM CreateAgg(string name, DECLSYM outerDeclSym)
        {
            DebugUtil.Assert(outerDeclSym.IsNSDECLSYM || outerDeclSym.IsAGGDECLSYM);

            BAGSYM bag = outerDeclSym.BagSym;

            // Create the new symbol.
            AGGSYM newAggSym;

            if (outerDeclSym.GetAssemblyID() == Kaid.Unresolved)
            {
                // Unresolved aggs need extra storage.
                SYM sym = CreateGlobalSym(SYMKIND.UNRESAGGSYM, name, bag);
                sym.Kind = SYMKIND.AGGSYM;
                newAggSym = sym as AGGSYM;
            }
            else
            {
                newAggSym = CreateGlobalSym(SYMKIND.AGGSYM, name, bag) as AGGSYM;
            }

            newAggSym.SymbolManager = this;
            newAggSym.InitFromOuterDecl(outerDeclSym);

            DebugUtil.Assert(
                !outerDeclSym.IsAGGDECLSYM ||
                (outerDeclSym as AGGDECLSYM).AggSym.IsSource == newAggSym.IsSource);
            return newAggSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateFwdAgg
        //
        /// <summary>
        /// Create a symbol for a forwarded aggregate type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nsd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FWDAGGSYM CreateFwdAgg(string name, NSDECLSYM nsd)
        {
            //ASSERT(nsd.GetAssemblyID() != kaidUnresolved);
            //ASSERT(!nsd.getInputFile().isSource);
            if (nsd.GetAssemblyID() == Kaid.Unresolved)
            {
                return null;
            }
            if (nsd.GetInputFile().IsSource) return null;

            FWDAGGSYM fwd = CreateGlobalSym(SYMKIND.FWDAGGSYM, name, nsd.NamespaceSym) as FWDAGGSYM;
            fwd.InFileSym = nsd.GetInputFile();
            return fwd;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateAggDecl
        //
        /// <summary>
        /// <para>Create a AGGDECLSYM instance with agg.Name
        /// and register it to the list of agg and declOuter.</para>
        /// <para>Not register to BSYMMGR.globalSymbolTable.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="outerDeclSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGDECLSYM CreateAggDecl(AGGSYM aggSym, DECLSYM outerDeclSym)
        {
            DebugUtil.Assert(aggSym != null && outerDeclSym != null);

            AGGDECLSYM newDeclSym = AllocSym(SYMKIND.AGGDECLSYM, aggSym.Name) as AGGDECLSYM;
            outerDeclSym.AddToChildList(newDeclSym);
            aggSym.AddDeclSym(newDeclSym);
            return newDeclSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateNamespaceDecl
        //
        /// <summary>
        /// <para>Create a namespace declaration symbol.</para>
        /// <para>Create a NSDECLSYM instance with the name of nsSym.
        /// Register this sym to the DECLSYM list of nsSym
        /// and add to the child list of nsdParentSym.</para>
        /// <para>This method does not register this NSDECLSYM instance to the symbol table.</para>
        /// </summary>
        /// <param name="nsSym">nsSym is the namespace for the declaration to be created.</param>
        /// <param name="nsdParentSym">nsdParentSym is the containing declaration for the new declaration.</param>
        /// <param name="infileSym">INFILESYM instance where the namespace is declared.</param>
        /// <param name="parseTree">parseTree is the parseTree fro the new declaration.</param>
        /// <returns>Reference to the created NSDECLSYM instance.</returns>
        //------------------------------------------------------------
        internal NSDECLSYM CreateNamespaceDecl(
            NSSYM nsSym,
            NSDECLSYM nsdParentSym,
            INFILESYM infileSym,
            NAMESPACENODE parseTree)
        {
            DebugUtil.Assert(infileSym != null);

            // Input file must match parent's input file if we have a parent.
            DebugUtil.Assert(
                nsdParentSym == null ||
                infileSym != null && nsdParentSym.InFileSym == infileSym);

            // Namespace parent must match declaration's container.
            DebugUtil.Assert(
                nsdParentSym != null && nsdParentSym.NamespaceSym != null && nsdParentSym.NamespaceSym == nsSym.ParentSym ||
                nsdParentSym == null && nsSym.ParentSym == null && nsSym == RootNamespaceSym);

            // Create and initialize the symbol.
            //
            // NOTE: we don't use CreateGlobalSym() because we don't want
            //       to insert the new SYM into the global lookup table.
            //       This requires us to do a bunch of stuff manually.

            NSDECLSYM newDeclSym = AllocSym(SYMKIND.NSDECLSYM, nsSym.Name) as NSDECLSYM;
            newDeclSym.InFileSym = infileSym;
            newDeclSym.ParseTreeNode = parseTree;

            // Link into the parent's child list at the end.
            nsSym.AddDeclSym(newDeclSym);
            if (nsdParentSym != null)
            {
                nsdParentSym.AddToChildList(newDeclSym);
            }

            // Set some bits on the namespace.
            if (infileSym.IsSource)
            {
                nsSym.IsDefinedInSource = true;
            }

            return newDeclSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateMembVar
        //
        /// <summary>
        /// Create a symbol for an member variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="declaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MEMBVARSYM CreateMembVar(string name, AGGSYM parent, AGGDECLSYM declaration)
        {
            // Create the new symbol.
            MEMBVARSYM sym = CreateGlobalSym(SYMKIND.MEMBVARSYM, name, parent) as MEMBVARSYM;
            sym.ContainingAggDeclSym = declaration;
            sym.LocalIteratorIndex = -1;
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateTyVar
        //
        /// <summary>
        /// <para>Create a symbol for a type parameter declaration and/or use of that declaration.
        /// The parent is the class or method that is parameterized by the type parameter.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYVARSYM CreateTyVar(string name, PARENTSYM parent)
        {
            // Create the new symbol.
            TYVARSYM tvSym = CreateGlobalSym(SYMKIND.TYVARSYM, name, parent) as TYVARSYM;
            tvSym.IsMethodTypeVariable = ((parent != null) && parent.IsMETHSYM);

            DebugUtil.Assert(!tvSym.HasErrors);
            tvSym.Unresolved = (parent != null && parent.IsAGGSYM && (parent as AGGSYM).IsUnresolved);

            return tvSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateMethod
        //
        /// <summary>
        /// Create a symbol for a method. Does not check for existing symbols
        /// because methods are assumed to be overloadable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="declaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal METHSYM CreateMethod(string name, AGGSYM parent, AGGDECLSYM declaration)
        {
            // Create the new symbol.
            METHSYM sym = CreateGlobalSym(SYMKIND.METHSYM, name, parent) as METHSYM;
            sym.ContainingAggDeclSym = declaration;
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateIfaceImplMethod
        //
        /// <summary></summary>
        /// <param name="parent"></param>
        /// <param name="declaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal IFACEIMPLMETHSYM CreateIfaceImplMethod(AGGSYM parent, AGGDECLSYM declaration)
        {
            IFACEIMPLMETHSYM sym =
                CreateGlobalSym(SYMKIND.IFACEIMPLMETHSYM, null, parent) as IFACEIMPLMETHSYM;

            // these syms really want to be methods + a little bit
            sym.Kind = SYMKIND.METHSYM;
            sym.IsInterfaceImpl = true;
            sym.ContainingAggDeclSym = declaration;
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateProperty
        //
        /// <summary>
        /// Create a symbol for an property. Does not check for existing symbols
        /// because properties are assumed to be overloadable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="declaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PROPSYM CreateProperty(string name, AGGSYM parent, AGGDECLSYM declaration)
        {
            // Create the new symbol.
            PROPSYM sym = CreateGlobalSym(SYMKIND.PROPSYM, name, parent) as PROPSYM;
            sym.ContainingAggDeclSym = declaration;
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateIndexer
        //
        /// <summary>
        /// Create a symbol for an property. Does not check for existing symbols
        /// because properties are assumed to be overloadable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="declaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INDEXERSYM CreateIndexer(string name, AGGSYM parent, AGGDECLSYM declaration)
        {
            // Create the new symbol.
            INDEXERSYM indexer = CreateGlobalSym(
                SYMKIND.INDEXERSYM,
                ((name != null) ? Compiler.NameManager.GetPredefinedName(PREDEFNAME.INDEXERINTERNAL) : null),
                parent) as INDEXERSYM;
            indexer.RealName = name;
            indexer.Kind = SYMKIND.PROPSYM;
            indexer.IsOperator = true;
            indexer.ContainingAggDeclSym = declaration;
            return indexer;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateEvent
        //
        /// <summary>
        /// Create an event symbol. Does not check for existing symbols.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="declaration"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EVENTSYM CreateEvent(string name, AGGSYM parent, AGGDECLSYM declaration)
        {
            EVENTSYM sym = CreateGlobalSym(SYMKIND.EVENTSYM, name, parent) as EVENTSYM;
            sym.ContainingAggDeclSym = declaration;
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.CreateModule
        //
        /// <summary>
        /// Create a MODULESYM instance, and assign a new assembly id as a module id.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="infile"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MODULESYM CreateModule(string name, INFILESYM infile)
        {
            MODULESYM module = CreateGlobalSym(SYMKIND.MODULESYM, name, infile) as MODULESYM;
            int assemblyId = AllocateAssemblyID(module);
            module.SetModuleID(assemblyId);
            return module;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetArray
        //
        /// <summary>
        /// Create or return an existing array symbol.
        /// We use the lookup mechanism to find unique array symbols efficiently.
        /// The parent of an array symbol is the element type,
        /// and the name is "[X&lt;n+1&gt;", where the second character has the rank.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="args">Dimension of array.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ARRAYSYM GetArray(TYPESYM elementType, int args, Type type)
        {
            DebugUtil.Assert(args > 0 && args < 32767);

            // (CS3) implicitly typed array
            if (elementType.Kind == SYMKIND.IMPLICITTYPESYM)
            {
                IMPLICITLYTYPEDARRAYSYM impArraySym =
                    CreateGlobalSym(
                        SYMKIND.IMPLICITLYTYPEDARRAYSYM,
                        null,
                        null) as IMPLICITLYTYPEDARRAYSYM;
                DebugUtil.Assert(impArraySym != null);

                impArraySym.Rank = args;
                return impArraySym;
            }

            System.Text.StringBuilder nameString = new StringBuilder();  // char[4] in sscli.
            string name;
            SYM sym = null;
            ARRAYSYM arraySym = null;

            switch (args)
            {
                // There are values ARRAY1 and ARRAY2 in enum PREDEFNAME,
                // Name "[X\002" and "[X\003" are registerd at ARRAY1 and ARRAY2 respectively.
                // Name "[X\001" is for arrays of unknown ranks.
                case 1:
                case 2:
                    name = Compiler.NameManager.GetPredefinedName(
                        (PREDEFNAME)((int)PREDEFNAME.ARRAY0 + args));
                    break;
                // fall through
                default:
                    name = String.Format("[X{0}", args + 1);
                    Compiler.NameManager.AddString(name);
                    break;
            }

            // See if we already have a array symbol of this element type and rank.
            sym = LookupGlobalSymCore(name, elementType, SYMBMASK.ARRAYSYM);
            if (sym != null)
            {
                arraySym = sym as ARRAYSYM;
            }
            if (arraySym == null)
            {
                // No existing array symbol. Create a new one.
                sym = CreateGlobalSym(SYMKIND.ARRAYSYM, name, elementType);
                if (sym != null)
                {
                    arraySym = sym as ARRAYSYM;
                }
                DebugUtil.Assert(arraySym != null);
                arraySym.Rank = args;
                arraySym.InitFromParent();

                AGGSYM agg = CreateGlobalSym(SYMKIND.AGGSYM, name + "AGG", null) as AGGSYM;
            }
            else
            {
                DebugUtil.Assert(arraySym.HasErrors == elementType.HasErrors);
                DebugUtil.Assert(arraySym.Unresolved == elementType.Unresolved);
            }

            Type atype = null;
            Type etype = null;

            if (arraySym != null &&
                arraySym.Type == null &&
                (etype = SymUtil.GetSystemTypeFromSym(elementType, null, null)) != null)
            {
                if (type != null)
                {
                    atype = type;
                }
                else
                {
                    atype = null;

                    if (args == 1)
                    {
                        atype = etype.MakeArrayType();
                    }
                    else if (args > 1)
                    {
                        atype = etype.MakeArrayType(args);
                    }
                    else
                    {
                        DebugUtil.Assert(false);
                    }
                }

                if (atype != null)
                {
                    arraySym.SetSystemType(atype, false);
                    this.InfoToSymbolTable.SetSymForInfo(atype, arraySym);
                }
            }

            DebugUtil.Assert(arraySym.Rank == args);
            DebugUtil.Assert(arraySym.ElementTypeSym == elementType);
            return arraySym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetInstAgg (1)
        //
        /// <summary>
        /// <para>Create / fetch an instantiated aggregate, eg, List&lt;string&gt;.
        /// The parent is the AGGSYM. The name is a merge of the outer type and type args.
        /// The resulting AGGTYPESYM is NOT placed in the child list of the AGGSYM.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="aggTypeSymOuter"></param>
        /// <param name="typeArgs"></param>
        /// <param name="typeArgsAll">In sscli, has the default value - null.</param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetInstAgg(
            AGGSYM aggSym,
            AGGTYPESYM aggTypeSymOuter,
            TypeArray typeArgs,
            TypeArray typeArgsAll)   // = NULL);
        {
            DebugUtil.Assert(aggSym.SymbolManager == this);
            DebugUtil.Assert(
              aggTypeSymOuter == null && (aggSym.ParentSym == null || !aggSym.IsNested) ||
              aggTypeSymOuter.GetAggregate() == aggSym.ParentSym);

            if (typeArgs == null)
            {
                typeArgs = BSYMMGR.EmptyTypeArray;
            }

            DebugUtil.Assert(aggSym.TypeVariables.Count == typeArgs.Count);
            DebugUtil.Assert(typeArgsAll == null || aggSym.AllTypeVariables.Count == typeArgsAll.Count);

            //--------------------------------------------------------
            // Create a name by typeArgs and aggTypeSymOuter,
            // then check if an AGGTYPESYM instance with the same name has already been registerd.
            //--------------------------------------------------------
            string name = GetSymbolName(typeArgs, aggTypeSymOuter);

            AGGTYPESYM aggTypeSym = null;
            aggTypeSym = LookupAggMember(name, aggSym, SYMBMASK.AGGTYPESYM) as AGGTYPESYM;

            //--------------------------------------------------------
            // If not found, create it.
            //     Parent is AggSym,
            //     its type arguments are typeArgs,
            //     all type arguments are typeArgsAll,
            //     declaring AGGTYPESYM is aggTypeSymOuter.
            //--------------------------------------------------------
            if (aggTypeSym == null)
            {
                aggTypeSym = CreateGlobalSym(SYMKIND.AGGTYPESYM, name, null) as AGGTYPESYM;

                if (aggTypeSym != null)
                {
                    // Set the parent and add it to the hash table, but not to the child list.
                    aggTypeSym.ParentSym = aggSym;
                    GlobalSymbolTable.InsertChild(aggSym, aggTypeSym);

                    aggTypeSym.TypeArguments = typeArgs;
                    aggTypeSym.OuterTypeSym = aggTypeSymOuter;
                    DebugUtil.Assert(!aggTypeSym.ConstraintsChecked && !aggTypeSym.HasConstraintError);

                    // build list of all type parameters including ones from outer types
                    if (typeArgsAll != null)
                    {
                        // The caller gave us pArgsAll. Assert they did it right.
                        DebugUtil.Assert(aggTypeSymOuter != null || typeArgsAll == typeArgs);
                        DebugUtil.Assert(
                            aggTypeSymOuter == null ||
                            typeArgsAll == ConcatParams(aggTypeSymOuter.AllTypeArguments, typeArgs));

                        aggTypeSym.AllTypeArguments = typeArgsAll;
                    }
                    else if (
                        aggTypeSymOuter != null &&
                        aggTypeSymOuter.AllTypeArguments != null &&
                        aggTypeSymOuter.AllTypeArguments.Count > 0)
                    {
                        aggTypeSym.AllTypeArguments =
                            ConcatParams(aggTypeSymOuter.AllTypeArguments, aggTypeSym.TypeArguments);
                    }
                    else
                    {
                        aggTypeSym.AllTypeArguments = aggTypeSym.TypeArguments;
                    }

                    aggTypeSym.HasErrors = aggTypeSym.AllTypeArguments.HasErrors;
                    aggTypeSym.Unresolved = aggTypeSym.AllTypeArguments.Unresolved || aggSym.IsUnresolved;
                }
            }
            //--------------------------------------------------------
            // If aggTypeSym is found, confirm no discord.
            //--------------------------------------------------------
            else // if (aggTypeSym == null)
            {
                DebugUtil.Assert(aggTypeSym.HasErrors == aggTypeSym.AllTypeArguments.HasErrors);
                DebugUtil.Assert(aggTypeSym.Unresolved == (aggTypeSym.AllTypeArguments.Unresolved || aggSym.IsUnresolved));
            }

            DebugUtil.Assert(aggTypeSym.GetAggregate() == aggSym);
            DebugUtil.Assert(aggTypeSym.TypeArguments != null && aggTypeSym.AllTypeArguments != null);
            DebugUtil.Assert(aggTypeSym.TypeArguments.Equals(typeArgs));
            DebugUtil.Assert(typeArgsAll == null || aggTypeSym.AllTypeArguments.Equals(typeArgsAll));

            return aggTypeSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetInstAgg (2)
        //
        /// <summary>
        /// <para>Create / fetch an instantiated aggregate.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        /// <param name="allTypeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetInstAgg(AGGSYM aggSym, TypeArray allTypeArgs)
        {
            DebugUtil.Assert(
                allTypeArgs != null &&
                allTypeArgs.Count == aggSym.AllTypeVariables.Count);

            if (allTypeArgs.Count == 0)
            {
                return aggSym.GetThisType();
            }

            // 親 SYM を外側の AGGSYM とする。

            AGGSYM outerAggSym = aggSym.GetOuterAgg();
            if (outerAggSym == null)
            {
                return GetInstAgg(aggSym, null, allTypeArgs, allTypeArgs);
            }

            int outerVerCount = outerAggSym.AllTypeVariables.Count;
            DebugUtil.Assert(outerVerCount <= allTypeArgs.Count);

            // すべての型パラメータを外部と内部に分ける。
            TYPESYM[] typeArray;
            TypeArray outerTypeArgs = null;
            TypeArray innerTypeArgs = null;
            AGGTYPESYM outerAts = null;

            if (allTypeArgs.GetSubArray(0, outerVerCount, out typeArray))
            {
                outerTypeArgs = AllocParams(typeArray);
            }
            if (allTypeArgs.GetSubArray(outerVerCount, allTypeArgs.Count - outerVerCount, out typeArray))
            {
                innerTypeArgs = AllocParams(typeArray);
            }
            outerAts = GetInstAgg(outerAggSym, outerTypeArgs);

            if (outerAts != null)
            {
                return GetInstAgg(aggSym, outerAts, innerTypeArgs, allTypeArgs);
            }
            return GetInstAgg(aggSym, null, allTypeArgs, allTypeArgs);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetPtrType
        //
        /// <summary>
        /// Create or return an existing pointer symbol.
        /// The parent of a pointer symbol is the base type, and the name is "*"
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PTRSYM GetPtrType(TYPESYM baseType)
        {
            DebugUtil.Assert(baseType != null);

            SYM sym = null;
            PTRSYM ptrSym = null;
            string namePtr = Compiler.NameManager.GetPredefinedName(PREDEFNAME.PTR);

            // See if we already have a pointer symbol of this base type.
            sym = LookupGlobalSymCore(namePtr, baseType, SYMBMASK.PTRSYM);
            if (sym != null)
            {
                ptrSym = sym as PTRSYM;
            }
            if (ptrSym == null)
            {
                // No existing array symbol. Create a new one.
                sym = CreateGlobalSym(SYMKIND.PTRSYM, namePtr, baseType);
                if (sym != null)
                {
                    ptrSym = sym as PTRSYM;
                }
                if (ptrSym != null)
                {
                    ptrSym.InitFromParent();
                }
            }
            else
            {
                DebugUtil.Assert(ptrSym.HasErrors == baseType.HasErrors);
                DebugUtil.Assert(ptrSym.Unresolved == baseType.Unresolved);
            }
            DebugUtil.Assert(ptrSym != null);

            Type ptype = null;
            if (ptrSym.Type == null)
            {
                Type btype = SymUtil.GetSystemTypeFromSym(baseType, null, null);
                if (btype != null)
                {
                    ptype = btype.MakePointerType();
                    if (ptype != null)
                    {
                        ptrSym.SetSystemType(ptype, false);
                        this.InfoToSymbolTable.SetSymForInfo(ptype, ptrSym);
                    }
                }
            }

            DebugUtil.Assert(ptrSym.BaseTypeSym == baseType);
            return ptrSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetNubType
        //
        /// <summary>
        /// Get / create the NUBSYM for the given base type.
        /// The base type is the parent of the NUBSYM.
        /// </summary>
        /// <param name="baseTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NUBSYM GetNubType(TYPESYM baseTypeSym)
        {
            NUBSYM nubSym = null;
            string name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.NUB);

            // See if we already have a pointer symbol of this base type.
            nubSym = LookupGlobalSymCore(name, baseTypeSym, SYMBMASK.NUBSYM) as NUBSYM;
            if (nubSym == null)
            {
                nubSym = CreateGlobalSym(SYMKIND.NUBSYM, name, baseTypeSym) as NUBSYM;
                if (nubSym != null)
                {
                    nubSym.InitFromParent();
                    nubSym.SymbolManager = this;

                    AGGSYM nullableSym = GetNullable();
                    DebugUtil.Assert(nullableSym != null);

                    Type nullableType = nullableSym.Type;
                    DebugUtil.Assert(nullableType != null && nullableType.IsGenericTypeDefinition);

                    Type baseType = SymUtil.GetSystemTypeFromSym(baseTypeSym, null, null);
                    if (baseType != null)
                    {
                        Type ntype=nullableType.MakeGenericType(baseType);
                        if (ntype != null)
                        {
                            nubSym.SetSystemType(ntype, false);
                            this.InfoToSymbolTable.SetSymForInfo(ntype, nubSym);
                        }
                    }
                }
            }
            else
            {
                DebugUtil.Assert(nubSym.HasErrors == baseTypeSym.HasErrors);
                DebugUtil.Assert(nubSym.Unresolved == baseTypeSym.Unresolved);
                DebugUtil.Assert(nubSym.SymbolManager == this);
            }

            DebugUtil.Assert(nubSym.BaseTypeSym == baseTypeSym);
            return nubSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetNubTypeOrError
        //
        /// <summary></summary>
        /// <param name="typeBase"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM GetNubTypeOrError(TYPESYM typeBase)
        {
            if (GetNullable() == null)
            {
                ReportMissingPredefTypeError(PREDEFTYPE.G_OPTIONAL);
                TypeArray ta = AllocParams(typeBase);
                string name = "Nullable";
                string syst = "System";
                if (!Compiler.NameManager.Lookup(name) || !Compiler.NameManager.Lookup(syst))
                {
                    throw new LogicError("BSYMMGR.GetNubTypeOrError");
                }
                PARENTSYM symPar = LookupGlobalSymCore(
                    syst,
                    rootNamespaceSym,
                    SYMBMASK.NSSYM) as PARENTSYM;
                return GetErrorType(symPar, name, ta);
            }
            return GetNubType(typeBase);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetPinnedType
        //
        /// <summary>
        /// Create or return an existing pinned symbol. The parent of a pinned symbol
        /// is the base type, and the name is "@"
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PINNEDSYM GetPinnedType(TYPESYM baseTypeSym)
        {
            SYM sym = null;
            PINNEDSYM pinnedSym = null;

            string name = Compiler.NameManager.GetPredefinedName(PREDEFNAME.PINNED);

            // See if we already have a pointer symbol of this base type.
            sym = LookupGlobalSymCore(name, baseTypeSym, SYMBMASK.PINNEDSYM);
            if (sym != null)
            {
                pinnedSym = sym as PINNEDSYM;
            }
            if (pinnedSym == null)
            {
                pinnedSym = CreateGlobalSym(SYMKIND.PINNEDSYM, name, baseTypeSym) as PINNEDSYM;
                if (pinnedSym != null)
                {
                    pinnedSym.InitFromParent();

                    Type btype = SymUtil.GetSystemTypeFromSym(baseTypeSym, null, null);
                    Type ptype = null;
                    if (btype != null)
                    {
                        if (btype.IsPointer)
                        {
                            btype = btype.GetElementType();
                        }
                        ptype = btype.MakeByRefType();
                        pinnedSym.SetSystemType(ptype, false);
                        this.InfoToSymbolTable.SetSymForInfo(ptype, pinnedSym);
                    }
                }
            }
            else
            {
                DebugUtil.Assert(pinnedSym.HasErrors == baseTypeSym.HasErrors);
                DebugUtil.Assert(pinnedSym.Unresolved == baseTypeSym.Unresolved);
            }

            DebugUtil.Assert(pinnedSym.BaseTypeSym == baseTypeSym);
            return pinnedSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetParamModifier
        //
        /// <summary>
        /// <para>Create or return an param modifier symbol.
        /// This symbol represents the type of a ref or out param.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="isOut"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PARAMMODSYM GetParamModifier(TYPESYM typeSym, bool isOut)
        {
            PARAMMODSYM paramModSym = null;

            string name = Compiler.NameManager.GetPredefinedName(
                isOut ? PREDEFNAME.OUTPARAM : PREDEFNAME.REFPARAM);

            // See if we already have a parammod symbol of this base type.
            paramModSym = LookupGlobalSymCore(name, typeSym, SYMBMASK.PARAMMODSYM) as PARAMMODSYM;

            if (paramModSym == null)
            {
                // No existing parammod symbol. Create a new one.
                paramModSym = CreateGlobalSym(SYMKIND.PARAMMODSYM, name, typeSym) as PARAMMODSYM;
                if (paramModSym != null)
                {
                    if (isOut)
                    {
                        paramModSym.IsOut = true;
                    }
                    else
                    {
                        paramModSym.IsRef = true;
                    }

                    Type type = null;
                    if (typeSym != null &&
                        (type = SymUtil.GetSystemTypeFromSym(typeSym, null, null)) != null)
                    {
                        Type ptype=type.MakeByRefType();
                        paramModSym.SetSystemType(ptype, false);
                        this.InfoToSymbolTable.SetSymForInfo(ptype, paramModSym);
                    }
                }
            }
            else
            {
                DebugUtil.Assert(paramModSym.HasErrors == typeSym.HasErrors);
                DebugUtil.Assert(paramModSym.Unresolved == typeSym.Unresolved);
            }

            DebugUtil.Assert(paramModSym.ParamTypeSym == typeSym);
            return paramModSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetModOptType (1)
        //
        /// <summary>
        /// importedToken を持つ MODOPTSYM を ModOptSym フィールド の値とし、
        /// baseTypeSym を親とする MODOPTTYPESYM を探す。
        /// </summary>
        /// <param name="baseTypeSym"></param>
        /// <param name="importedToken"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MODOPTTYPESYM GetModOptType(TYPESYM baseTypeSym, int importedToken, MODULESYM scope)
        {
            SYM sym = null;
            MODOPTSYM modOptSym = null;

            string name = BSYMMGR.GetNameFromPtrs((int)importedToken, 0);
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }
            sym = LookupGlobalSymCore(name, scope, SYMBMASK.MODOPTSYM);
            if (sym != null)
            {
                modOptSym = sym as MODOPTSYM;
            }

            if (modOptSym == null)
            {
                // No existing parammod symbol. Create a new one.
                sym = CreateGlobalSym(SYMKIND.MODOPTSYM, name, scope);
                modOptSym = sym as MODOPTSYM;
                if (modOptSym != null)
                {
                    modOptSym.ImportedToken = importedToken;
                    modOptSym.EmittedToken = 0;
                }
            }

            return GetModOptType(baseTypeSym, modOptSym);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetModOptType (2)
        //
        /// <summary>
        /// ModOptSym フィールドが modOptSym で、親が baseTypeSym の MODOPTTYPESYM を探す。
        /// 見つからない場合は作成する。
        /// </summary>
        /// <param name="baseTypeSym"></param>
        /// <param name="modOptSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MODOPTTYPESYM GetModOptType(TYPESYM baseTypeSym, MODOPTSYM modOptSym)
        {
            MODOPTTYPESYM modOptTypeSym = null;

            // 元のプログラムでは modOptSym のアドレスから名前を作っているが、その方法は使えない。
            // 名前のほかに親と SYMBMASK も使用して検索するので、SymID から名前を作成することにする。
            string name = BSYMMGR.GetNameFromPtrs(modOptSym.SymID, 0);
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            // 名前が name、親が baseTypeSym の MODOPTTYPESYM を探す。
            modOptTypeSym = LookupGlobalSymCore(name, baseTypeSym, SYMBMASK.MODOPTTYPESYM) as MODOPTTYPESYM;

            if (modOptTypeSym == null)
            {
                // No existing parammod symbol. Create a new one.
                modOptTypeSym = CreateGlobalSym(SYMKIND.MODOPTTYPESYM, name, baseTypeSym) as MODOPTTYPESYM;
                DebugUtil.Assert(modOptTypeSym != null);
                modOptTypeSym.ModOptSym = modOptSym;
                modOptTypeSym.InitFromParent();
            }
            else
            {
                DebugUtil.Assert(modOptTypeSym.HasErrors == baseTypeSym.HasErrors);
                DebugUtil.Assert(modOptTypeSym.Unresolved == baseTypeSym.Unresolved);
            }

            DebugUtil.Assert(modOptTypeSym.BaseTypeSym == baseTypeSym);
            return modOptTypeSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetNsAid
        //
        /// <summary>
        /// <para>Find or create a NSAID instance whose AssemblyID is id in namespace ns.</para>
        /// <para>If create, register to BSYMMGR.GlobalSymbolTable.</para>
        /// </summary>
        /// <param name="ns">Parent NSSYM</param>
        /// <param name="id">Assembly ID</param>
        /// <returns>Refernce of the NSAIDSYM instance.</returns>
        //------------------------------------------------------------
        internal NSAIDSYM GetNsAid(NSSYM ns, int id)
        {
            string name = GetNameFromPtrs(id, 0);
            DebugUtil.Assert(name != null);

            SYM sym = null;
            NSAIDSYM nsa = null;
            sym = LookupGlobalSymCore(name, ns, SYMBMASK.NSAIDSYM);
            if (sym != null)
            {
                nsa = sym as NSAIDSYM;
            }
            if (nsa == null)
            {
                // Create a new one. its parent is ns.
                nsa = CreateGlobalSym(SYMKIND.NSAIDSYM, name, ns) as NSAIDSYM;
                nsa.SetAssemblyID(id);
            }

            DebugUtil.Assert(nsa.NamespaceSym == ns);
            return nsa;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetRootNsAid
        //
        /// <summary>
        /// <para>Find or create a NSAIDSYM instance
        /// whose AssemblyID is id in RootNamespaceSym.</para>
        /// <para>If create, register to BSYMMGR.GlobalSymbolTable.</para>
        /// </summary>
        /// <param name="assemblyID"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NSAIDSYM GetRootNsAid(int assemblyID)
        {
            return GetNsAid(RootNamespaceSym, assemblyID);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetNubFromNullable
        //
        /// <summary>
        /// Get the equivalent T? for a Nullable<T>.
        /// </summary>
        /// <para></para>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NUBSYM GetNubFromNullable(AGGTYPESYM ats)
        {
            DebugUtil.Assert(ats.IsPredefType(PREDEFTYPE.G_OPTIONAL));
            return GetNubType(ats.AllTypeArguments[0]);
        }

        //------------------------------------------------------------
        // BSYMMGR.MaybeConvertNullableToNub
        //
        /// <summary></summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM MaybeConvertNullableToNub(TYPESYM ts)
        {
            if (ts.IsAGGTYPESYM && (ts as AGGTYPESYM).IsPredefType(PREDEFTYPE.G_OPTIONAL))
            {
                return GetNubFromNullable(ts as AGGTYPESYM);
            }
            return ts;
        }

        //------------------------------------------------------------
        // BSYMMGR.MaybeConvertNubToNullable
        //
        /// <summary></summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM MaybeConvertNubToNullable(TYPESYM ts)
        {
            if (ts.IsNUBSYM)
            {
                return (ts as NUBSYM).GetAggTypeSym();
            }
            return ts;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetErrorType
        //
        /// <summary>
        /// <para>Find the ERRORSYM with name and typeArgs. If not found, create it.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentSym">Must be null or NSAIDSYM or TYPESYM.</param>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ERRORSYM GetErrorType(PARENTSYM parentSym, string errorName, TypeArray typeArgs)
        {
            DebugUtil.Assert(errorName != null);
            DebugUtil.Assert(parentSym == null || parentSym.IsNSAIDSYM || parentSym.IsTYPESYM);

            SYM sym = null;
            ERRORSYM errorSym = null;

            if (parentSym == null)
            {
                parentSym = GetRootNsAid((int)Kaid.Global);
            }
            if (typeArgs == null)
            {
                typeArgs = EmptyTypeArray;
            }
            string name = GetNameFromPtrs(
                this.compiler.NameManager.GetNameID(errorName),
                typeArgs.TypeArrayID);
            DebugUtil.Assert(name != null);

            sym = LookupGlobalSymCore(name, parentSym, SYMBMASK.ERRORSYM);
            if (sym != null)
            {
                errorSym = sym as ERRORSYM;
            }

            if (errorSym == null)
            {
                sym = CreateGlobalSym(SYMKIND.ERRORSYM, name, parentSym);
                if (sym != null)
                {
                    errorSym = sym as ERRORSYM;
                }
                DebugUtil.Assert(errorSym != null);
                errorSym.HasErrors = true;
                errorSym.ErrorName = errorName;
                errorSym.SetName(name);
                errorSym.TypeArguments = typeArgs;
            }
            else
            {
                DebugUtil.Assert(errorSym.HasErrors);
                DebugUtil.Assert(errorSym.Name == name);
                DebugUtil.Assert(errorSym.TypeArguments == typeArgs);
            }

            DebugUtil.Assert(!errorSym.Unresolved);
            return errorSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.AllocParams (1)
        //
        /// <summary>
        /// <para>Allocate a type array; used to represent a parameter list.
        /// We use a hash table to make sure that allocating the same type array
        /// twice returns the same value. This does two things:
        /// <list type="number">
        /// <item>Save a lot of memory.</item>
        /// <item>Make it so parameter lists can be compared by a simple pointer comparison.</item>
        /// <item>Allow us to associate a token with each signature for faster metadata emit.</item>
        /// </list>
        /// </para>
        /// <para>Create a TypeArray instance from TYPESYM[],
        /// register it to TypeArrayTable and return it.</para>
        /// </summary>
        /// <param name="typeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray AllocParams(params TYPESYM[] typeArray)
        {
            if (typeArray == null || typeArray.Length== 0)
            {
                // We have one standard empty TypeArray. It's not in the hash table.
                return BSYMMGR.EmptyTypeArray;
            }
            return TypeArrayTable.AllocTypeArray(typeArray);
        }

        //------------------------------------------------------------
        // BSYMMGR.AllocParams (2)
        //
        /// <summary>
        /// Find or create the TypeArray which holds a given List&lt;TYPESM&gt; instance.
        /// </summary>
        /// <param name="typeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray AllocParams(List<TYPESYM> typeArray)
        {
            if (typeArray == null || typeArray.Count == 0)
            {
                // We have one standard empty TypeArray. It's not in the hash table.
                return BSYMMGR.EmptyTypeArray;
            }
            return TypeArrayTable.AllocTypeArray(typeArray);
        }

        //------------------------------------------------------------
        // BSYMMGR.AllocParams (3)
        //
        /// <summary>
        /// <para>Register a TypeArray instance, and return itself.</para>
        /// </summary>
        /// <param name="typeArray"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray AllocParams(TypeArray typeArray)
        {
            if (typeArray == null || typeArray.Count == 0)
            {
                // We have one standard empty TypeArray. It's not in the hash table.
                return BSYMMGR.EmptyTypeArray;
            }
            return TypeArrayTable.AllocTypeArray(typeArray);
        }

        //------------------------------------------------------------
        // BSYMMGR.ConcatParams (1)
        //
        /// <summary>
        /// Concatenate two TypeArray instance to one.
        /// </summary>
        /// <param name="typeArray1"></param>
        /// <param name="typeArray2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray ConcatParams(TypeArray typeArray1, TypeArray typeArray2)
        {
            TypeArray typeArrayNew = new TypeArray();
            typeArrayNew.Add(typeArray1);
            typeArrayNew.Add(typeArray2);
            return AllocParams(typeArrayNew);
        }

        //------------------------------------------------------------
        // BSYMMGR.ConcatParams (2)
        //
        /// <summary></summary>
        /// <param name="pta"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray ConcatParams(TypeArray tarr, TYPESYM sym)
        {
            TypeArray newArray = new TypeArray();
            newArray.Add(tarr);
            newArray.Add(sym);
            return AllocParams(newArray);
        }
        //TypeArray * BSYMMGR::ConcatParams(TypeArray * pta, TYPESYM * type)
        //{
        //    if (!TypeArray::Size(pta))
        //        return AllocParams(1, &type);
        //    return ConcatParams(pta->size, pta->ItemPtr(0), 1, &type);
        //}

        //------------------------------------------------------------
        // BSYMMGR.ConcatParams (3)
        //
        /// <summary></summary>
        /// <param name="ctype1"></param>
        /// <param name="prgtype1"></param>
        /// <param name="ctype2"></param>
        /// <param name="prgtype2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray ConcatParams(
            int tCount1,
            List<TYPESYM> tList1,
            int tCount2,
            List<TYPESYM> tList2)
        {
            int c1 = (tList1 != null ? tList1.Count : 0);
            int c2 = (tList2 != null ? tList2.Count : 0);
            if (tCount1 > c1 || tCount2 > c2)
            {
                DebugUtil.Assert(false, "ConcatParams (3)");
                return null;
            }

            List<TYPESYM> newArray = new List<TYPESYM>();
            if (tCount1 > 0)
            {
                newArray.AddRange(tList1.GetRange(0, tCount1));
            }
            if (tCount2 > 0)
            {
                newArray.AddRange(tList2.GetRange(0, tCount2));
            }

            return AllocParams(newArray);
        }
        //TypeArray * BSYMMGR::ConcatParams(int ctype1, TYPESYM ** prgtype1, int ctype2, TYPESYM ** prgtype2)
        //{
        //    int ctype = ctype1 + ctype2;
        //    TYPESYM ** prgtype = STACK_ALLOC(TYPESYM *, ctype);
        //
        //    memcpy(prgtype, prgtype1, ctype1 * sizeof(TYPESYM *));
        //    memcpy(prgtype + ctype1, prgtype2, ctype2 * sizeof(TYPESYM *));
        //
        //    return AllocParams(ctype, prgtype);
        //}

        //------------------------------------------------------------
        // BSYMMGR.GetSubTypeArray
        //
        /// <summary></summary>
        /// <param name="typeArray"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray GetSubTypeArray(
            TypeArray typeArray,
            int start,
            int count)
        {
            int size = TypeArray.Size(typeArray);
            if (size == 0 || start >= size)
            {
                return BSYMMGR.emptyTypeArray;
            }

            DebugUtil.Assert(start >= 0);
            int end = start + count;
            end = (end <= size ? end : size);
            TypeArray sub = new TypeArray();

            for (int i = start; i < end; ++i)
            {
                sub.Add(typeArray[i]);
            }
            return AllocParams(sub);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetGlobalNsAid
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal NSAIDSYM GetGlobalNsAid()
        {
            return globalNsAidSym;
        }

        //internal SCOPESYM GetFileRoot() { return fileroot; }
        //internal OUTFILESYM GetMDFileRoot() { return mdfileroot; }
        //internal PARENTSYM GetXMLFileRoot() { return xmlfileroot; }
        //internal ERRORSYM GetErrorSym() { return errorSym; }
        //internal AGGTYPESYM GetArglistSym() { return arglistSym; }
        //internal AGGTYPESYM GetNaturalIntSym() { return naturalIntSym; }

        //------------------------------------------------------------
        // BSYMMGR.GetObject
        //
        /// <summary>
        /// return prefined object type
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetObject()
        {
            return GetReqPredefAgg(PREDEFTYPE.OBJECT);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetNullable
        //
        /// <summary>
        /// return prefined System.Nullable&lt;&gt; agg.
        /// </summary>
        //------------------------------------------------------------
        internal AGGSYM GetNullable()
        {
            if (this.nullableAggSym == null)
            {
                this.nullableAggSym = GetOptPredefAgg(PREDEFTYPE.G_OPTIONAL);
            }
            return this.nullableAggSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetReqPredefAgg
        //
        /// <summary>
        /// Return an existing AGGSYM instance representing a required predefined type.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetReqPredefAgg(PREDEFTYPE pt) // Never returns NULL.
        {
            AGGSYM sym = null;
            PredefTypeInfo info = null;
            try
            {
                sym = predefSyms[(int)pt];
                info = PredefType.InfoTable[(int)pt];
            }
            catch (IndexOutOfRangeException)
            {
                DebugUtil.Assert(false, "BSYMMGR.GetReqPredefAgg");
            }
            if (sym != null && info != null && info.isRequired == true)
            {
                return sym;
            }
            DebugUtil.Assert(false, "BSYMMGR.GetReqPredefAgg");
            return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetOptPredefAgg
        //
        /// <summary>
        /// <para>Return a AGGSYM instance in BSYMMGR.predefSyms
        /// for a given PREDEFTYPE value.</para>
        /// <para>predefSyms are created in BSYMMGR.InitPredefinedTypes,
        /// which is called by COMPILER.CompileAll method.</para>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM GetOptPredefAgg(PREDEFTYPE pt) // May return NULL.
        {
            try
            {
                return this.predefSyms[(int)pt];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.ReportMissingPredefTypeError
        //
        /// <summary></summary>
        /// <param name="pt"></param>
        //------------------------------------------------------------
        internal void ReportMissingPredefTypeError(PREDEFTYPE pt)
        {
            DebugUtil.Assert(
                0 <= pt &&
                pt < PREDEFTYPE.COUNT &&
                predefSyms[(int)pt] == null &&
                !PredefType.InfoTable[(int)pt].isRequired);

            string arg = null;
            try
            {
                arg = PredefType.InfoTable[(int)pt].fullName;
            }
            catch (IndexOutOfRangeException)
            {
                arg = "(unknown)";
            }
            Error(CSCERRID.ERR_PredefinedTypeNotFound, new ErrArg(arg));
        }

        //------------------------------------------------------------
        // BSYMMGR.AllocateAssemblyID
        //
        /// <summary>
        /// <para>Register SYM instances to infileSymSet or moduleSymSet,
        /// and assign assembly ID as follows.</para>
        /// <list type="bullet">
        /// <item>For INFILESYM and EXTERNALIASSYM,
        /// return Kaid.Unresolved + index. (Kaid.Unresolved = 3)</item>
        /// <item>For MODULESYM and OUTFILESYM,
        /// return Kaid.MinModule + index. (Kaid.MinModule = 0x10000000)</item>
        /// <item>Otherwise, return Kaid.Nil.</item>
        /// </list>
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int AllocateAssemblyID(SYM sym)
        {
            if (sym.IsINFILESYM || sym.IsEXTERNALIASSYM)
            {
                return infileSymSet.AddSym(sym) + Kaid.Unresolved;
            }
            DebugUtil.Assert(sym.IsMODULESYM || sym.IsOUTFILESYM);
            return moduleSymSet.AddSym(sym) + Kaid.MinModule;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetPredefZero
        //
        /// <summary>
        /// returns a constant value of zero for a predefined type
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Object GetPredefZero(PREDEFTYPE pt)
        {
            try
            {
                return PredefType.InfoTable[(int)pt].zero;
            }
            catch (IndexOutOfRangeException)
            {
            }
            throw new LogicError("BSYMMGR.GetPredefZero");
        }

        //------------------------------------------------------------
        // BSYMMGR.GetPredefAttr
        //
        /// <summary>
        /// PREDEFATTR.COUNT means "not attribute".
        /// </summary>
        /// <param name="ats"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PREDEFATTR GetPredefAttr(AGGTYPESYM ats)
        {
            AGGSYM agg = ats.GetAggregate();
            if (!agg.IsPredefinedType)
            {
                return PREDEFATTR.COUNT;
            }

            try
            {
                return PredefType.InfoTable[(int)agg.PredefinedTypeID].attr;
            }
            catch (IndexOutOfRangeException)
            {
                DebugUtil.Assert(false, "BSYMMGR.GetPredefAttr, IndexOutOfRangeException");
            }
            return PREDEFATTR.COUNT;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetObjectType
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGTYPESYM GetObjectType()
        {
            return GetObject().GetThisType();
        }

        //------------------------------------------------------------
        // BSYMMGR.FindInfileSym
        //
        /// <summary>
        /// Find the INFILESYM with a given name.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM FindInfileSym(string fileName)
        {
            OUTFILESYM outFile = null;

            // Check all outfile sym.
            for (outFile = this.FileRootSym.FirstChildSym as OUTFILESYM;
                outFile != null;
                outFile = outFile.NextOutputFile())
            {
                SYM sym = this.LookupGlobalSymCore(fileName, outFile, SYMBMASK.INFILESYM);
                if (sym != null)
                {
                    return (sym as INFILESYM);
                }
            }

            // This is much slower, but does case-insensitive, which is needed in some cases.
            for (outFile = this.FileRootSym.FirstChildSym as OUTFILESYM;
                outFile != null;
                outFile = outFile.NextOutputFile())
            {
                for (INFILESYM inFile = outFile.FirstInFileSym();
                    inFile != null;
                    inFile = inFile.NextInFileSym())
                {
                    if (String.Compare(inFile.Name, fileName, true) == 0 ||
                        String.Compare(inFile.FullName, fileName, true) == 0)
                    {
                        return inFile;
                    }
                }
            }
            return null;

            //    POUTFILESYM pOutfile;
            //
            //    // Check all outfile sym.
            //    for (pOutfile = fileroot.firstChild.AsOUTFILESYM; pOutfile != NULL; pOutfile = pOutfile.nextOutfile()) {
            //        SYM * sym = LookupGlobalSymCore( filename, pOutfile, MASK_INFILESYM);
            //        if (sym)
            //            return sym.AsINFILESYM;
            //    }
            //
            //    // This is much slower, but does case-insensitive, which is needed in some cases.
            //    for (pOutfile = fileroot.firstChild.AsOUTFILESYM; pOutfile != NULL; pOutfile = pOutfile.nextOutfile()) {
            //        for (PINFILESYM pInfile = pOutfile.firstInfile(); pInfile != NULL; pInfile = pInfile.nextInfile()) {
            //        if (CompareNoCase(pInfile.name.text, filename.text) == 0)
            //            return pInfile;
            //        }
            //    }
            //
            //    return NULL;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetInfileForAid
        //
        /// <summary></summary>
        /// <param name="assemblyId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal INFILESYM GetInfileForAid(int assemblyId)
        {
            DebugUtil.Assert(assemblyId > Kaid.ThisAssembly);

            SYM  sym = GetSymForAid(assemblyId);
            if (sym == null)
            {
                return null;
            }
            if (sym.IsINFILESYM)
            {
                return (sym as INFILESYM);
            }
            if (sym.IsMODULESYM)
            {
                return (sym as MODULESYM).GetInputFile();
            }
            return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetSymForAid
        //
        /// <summary>
        /// Return the sym with assemblyId.
        /// the sym is one of MODULESYM, OUTFILESYM, INFILESYM, EXTERNALALIASSYM.
        /// </summary>
        /// <param name="assemblyId"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal SYM GetSymForAid(int assemblyId)
        {
            // MODULESYM or OUTFILESYM
            if (assemblyId >= Kaid.MinModule)
            {
                SYM sym = moduleSymSet.GetSym(assemblyId - Kaid.MinModule);

                DebugUtil.Assert(sym != null);
                DebugUtil.Assert(sym.IsMODULESYM || sym.IsOUTFILESYM);
                DebugUtil.Assert(!sym.IsMODULESYM || (sym as MODULESYM).GetModuleID() == assemblyId);
                DebugUtil.Assert(!sym.IsOUTFILESYM || (sym as OUTFILESYM).GetModuleID() == assemblyId);

                return sym;
            }

            // INFILESYM or EXTERNALALIASSYM
            if (assemblyId >= Kaid.Unresolved)
            {
                SYM sym = infileSymSet.GetSym(assemblyId - Kaid.Unresolved);

                DebugUtil.Assert(sym != null);
                DebugUtil.Assert(sym.IsINFILESYM || sym.IsEXTERNALIASSYM);
                DebugUtil.Assert(sym.GetAssemblyID() == assemblyId);

                return sym;
            }

            DebugUtil.Assert(assemblyId >= 0);
            return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetElementType
        //
        // Get the COM+ signature element type from an aggregate type. 
        //------------------------------------------------------------
        internal byte GetElementType(AGGTYPESYM type)
        //BYTE BSYMMGR::GetElementType(PAGGTYPESYM type)
        {
            throw new NotImplementedException("BSYMMGR.GetElementType");

            //    if (type.isPredefined()) {
            //        BYTE et = predefTypeInfo[type.getPredefType()].et;
            //        if (et != ELEMENT_TYPE_END)
            //            return et;
            //    }
            //
            //    // Not a special type. Either a value or reference type.
            //    if (type.fundType() == FT_REF)
            //        return ELEMENT_TYPE_CLASS;
            //    else 
            //        return ELEMENT_TYPE_VALUETYPE;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetNiceName (1)
        //
        /// <summary>
        /// <para>Some of the predefined types have built-in names, like "int" or "string" or "object".
        /// This return the nice name if one exists; otherwise NULL is returned.</para>
        /// <para>predefTypeInfo[] の niceName フィールドを返す。</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string GetNiceName(AGGSYM type)
        {
            if (type.IsPredefinedType)
                return GetNiceName(type.PredefinedTypeID);
            else
                return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetNiceName (2)
        //
        /// <summary>
        /// <para>Some of the predefined types have built-in names, like "int"
        /// or "string" or "object". This return the nice name if one
        /// exists; otherwise NULL is returned.</para>
        /// <para>predefTypeInfo[] の niceName フィールドを返す。</para>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string GetNiceName(PREDEFTYPE pt)
        {
            return PredefType.InfoTable[(int)pt].niceName;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetAttrArgSize
        //
        /// <summary>
        /// <para>Return PredefTypeInfo.asize</para>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static int GetAttrArgSize(PREDEFTYPE pt)
        {
            return PredefType.InfoTable[(int)pt].asize;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetPredefinedFullName
        //
        /// <summary>
        /// <para>(sscli) PCWSTR BSYMMGR::GetFullName(PREDEFTYPE pt)</para>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string GetPredefinedFullName(PREDEFTYPE pt)
        {
            try
            {
                return PredefType.InfoTable[(int)pt].fullName;
            }
            catch (IndexOutOfRangeException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetPredefIndex
        //
        /// <summary>
        /// Return a PREDEFTYPE value for a given CorElementType value.
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static PREDEFTYPE GetPredefIndex(CorElementType elementType)
        {
            switch (elementType)
            {
                case CorElementType.BOOLEAN:
                    return PREDEFTYPE.BOOL;
                case CorElementType.CHAR:
                    return PREDEFTYPE.CHAR;
                case CorElementType.U1:
                    return PREDEFTYPE.BYTE;
                case CorElementType.I2:
                    return PREDEFTYPE.SHORT;
                case CorElementType.I4:
                    return PREDEFTYPE.INT;
                case CorElementType.I8:
                    return PREDEFTYPE.LONG;
                case CorElementType.R4:
                    return PREDEFTYPE.FLOAT;
                case CorElementType.R8:
                    return PREDEFTYPE.DOUBLE;
                case CorElementType.STRING:
                    return PREDEFTYPE.STRING;
                case CorElementType.OBJECT:
                    return PREDEFTYPE.OBJECT;
                case CorElementType.I1:
                    return PREDEFTYPE.SBYTE;
                case CorElementType.U2:
                    return PREDEFTYPE.USHORT;
                case CorElementType.U4:
                    return PREDEFTYPE.UINT;
                case CorElementType.U8:
                    return PREDEFTYPE.ULONG;
                case CorElementType.I:
                    return PREDEFTYPE.INTPTR;
                case CorElementType.U:
                    return PREDEFTYPE.UINTPTR;
                case CorElementType.TYPEDBYREF:
                    return PREDEFTYPE.REFANY;
                default:
                    //return (PREDEFTYPE)UNDEFINEDINDEX;
                    // csharp\sccomp\compiler.h(51): #define UNDEFINEDINDEX 0xffffffff
                    return PREDEFTYPE.UNDEFINED;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.GetPredefFundType
        //
        /// <summary></summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static FUNDTYPE GetPredefFundType(PREDEFTYPE pt)
        {
            return PredefType.InfoTable[(int)pt].ft;
        }

        //------------------------------------------------------------
        // BSYMMGR.GetStdMethTypeVar
        //
        /// <summary>
        /// <para>Return a prepared TYVARSYM instance for methods with specified index.</para>
        /// <para>The name and parent of this instance are null.</para>
        /// </summary>
        /// <param name="iv"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYVARSYM GetStdMethTypeVar(int iv)
        {
            return stdMethodTypeVariables.GetTypeVarSym(iv, this, true);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetStdClsTypeVar
        //
        /// <summary>
        /// <para>Return a prepared TYVARSYM instance for aggregate types with specified index.</para>
        /// <para>The name and parent of this instance are null.</para>
        /// </summary>
        /// <param name="iv"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYVARSYM GetStdClsTypeVar(int iv)
        {
            return stdClassTypeVariables.GetTypeVarSym(iv, this, false);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetStdTypeVar
        //
        /// <summary>
        /// <para>Return a prepared TYVARSYM instance for aggregate types or method
        /// with specified index.</para>
        /// <para>The name and parent of this instance are null.</para>
        /// </summary>
        /// <param name="iv"></param>
        /// <param name="isMethod"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYVARSYM GetStdTypeVar(int iv, bool isMethod)
        {
            return (isMethod ? GetStdMethTypeVar(iv) : GetStdClsTypeVar(iv));
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstType (1)
        //
        /// <summary>
        /// <para>Substitute an instantiation through a type, generating a new type.</para>
        /// <para>If argument context is valid, call BSYMMGR.SubContextCore,
        /// or return argument typeSym as is.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstType(
            TYPESYM typeSym,
            SubstContext context)
        {
            return (context == null || context.FNop()) ? typeSym : SubstTypeCore(typeSym, context);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstType (2)
        //
        /// <summary>
        /// <para>Substitute an instantiation through a type, generating a new type.</para>
        /// <para>Either or both of ppClassTypeArgs and ppMethTypeArgs can be NULL or empty,
        /// which indicates that we want the identity substitution for that collection of type variables,
        /// e.g. T . T, U . U.</para>
        /// <para>If non-null, the arrays must be of the appropriate size,
        /// and specify the substitution for that range of type parameters.</para>
        /// </summary>
        /// <param name="typeSrc"></param>
        /// <param name="typeArgsCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <param name="grfst"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstType(
            TYPESYM typeSrc,
            TypeArray typeArgsCls,
            TypeArray typeArgsMeth,   // = NULL,
            SubstTypeFlagsEnum grfst) // = SubstTypeFlags::NormNone);
        {
            if (typeSrc == null)
            {
                return null;
            }

            SubstContext context = new SubstContext(typeArgsCls, typeArgsMeth, grfst);
            return (context.FNop() ? typeSrc : SubstTypeCore(typeSrc, context));
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstType (3)
        //
        /// <summary>
        /// <para>Substitute an instantiation through a type, generating a new type.</para>
        /// </summary>
        /// <param name="typeSrc"></param>
        /// <param name="atsCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstType(
            TYPESYM typeSrc,
            AGGTYPESYM atsCls,
            TypeArray typeArgsMeth) // = null
        {
            return SubstType(
                typeSrc,
                (atsCls != null ? atsCls.AllTypeArguments : null),
                typeArgsMeth,
                SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstType (4)
        //
        /// <summary>
        /// <para>Substitute an instantiation through a type, generating a new type.</para>
        /// </summary>
        /// <param name="typeSrc"></param>
        /// <param name="typeCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstType(
            TYPESYM typeSrc,
            TYPESYM typeCls,
            TypeArray typeArgsMeth)
        {
            return SubstType(
                typeSrc,
                (typeCls.IsAGGTYPESYM ? (typeCls as AGGTYPESYM).AllTypeArguments : null),
                typeArgsMeth,
                SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstType (5)
        //
        /// <summary>
        /// <para>Substitute an instantiation through a type, generating a new type.</para>
        /// </summary>
        /// <param name="typeSrc"></param>
        /// <param name="grfst"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstType(
            TYPESYM typeSrc,
            SubstTypeFlagsEnum grfst)
        {
            return SubstType(typeSrc, (TypeArray)null, (TypeArray)null, grfst);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypes (1)
        //
        /// <summary></summary>
        /// <param name="typeDst"></param>
        /// <param name="typeSrc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SubstEqualTypes(
            TYPESYM typeDst,
            TYPESYM typeSrc,
            SubstContext context)
        {
            if (typeDst == typeSrc)
            {
                DebugUtil.Assert(typeDst == SubstType(typeSrc, context));
                return true;
            }

            return (context != null && !context.FNop() && SubstEqualTypesCore(typeDst, typeSrc, context));
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypes (2)
        //
        /// <summary></summary>
        /// <param name="typeDst"></param>
        /// <param name="typeSrc"></param>
        /// <param name="typeArgsCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SubstEqualTypes(
            TYPESYM typeDst,
            TYPESYM typeSrc,
            TypeArray typeArgsCls,
            TypeArray typeArgsMeth,      // = NULL,
            SubstTypeFlagsEnum flags)    // = SubstTypeFlags::NormNone);
        {
            if (typeDst == typeSrc)
            {
                DebugUtil.Assert(typeDst == SubstType(typeSrc, typeArgsCls, typeArgsMeth, flags));
                return true;
            }

            SubstContext context = new SubstContext(typeArgsCls, typeArgsMeth, flags);
            return !context.FNop() && SubstEqualTypesCore(typeDst, typeSrc, context);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypes (3)
        //
        /// <summary></summary>
        /// <param name="typeDst"></param>
        /// <param name="typeSrc"></param>
        /// <param name="atsCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SubstEqualTypes(
            TYPESYM typeDst,
            TYPESYM typeSrc,
            AGGTYPESYM atsCls,
            TypeArray typeArgsMeth) // = null
        {
            return SubstEqualTypes(
                typeDst,
                typeSrc,
                (atsCls != null ? atsCls.AllTypeArguments : null),
                typeArgsMeth,
                SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypes (4)
        //
        /// <summary></summary>
        /// <param name="typeDst"></param>
        /// <param name="typeSrc"></param>
        /// <param name="typeCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SubstEqualTypes(
            TYPESYM typeDst,
            TYPESYM typeSrc,
            TYPESYM typeCls,
            TypeArray typeArgsMeth) // = null
        {
            return SubstEqualTypes(
                typeDst,
                typeSrc,
                typeCls.IsAGGTYPESYM ? (typeCls as AGGTYPESYM).AllTypeArguments : null,
                typeArgsMeth,
                SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeArray (1)
        //
        /// <summary>
        /// <para>SubstContext のデータが有効なら TypeArray の各要素を処理したもので
        /// 新しい TypeArray を作成して返す。</para>
        /// <para>無効なら元の TypeArray を返す。</para>
        /// </summary>
        /// <param name="typeArraySrc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray SubstTypeArray(
            TypeArray srcTypeArray,
            SubstContext context)
        {
            if (srcTypeArray == null ||
                srcTypeArray.Count == 0 ||
                context == null ||
                context.FNop())
            {
                return srcTypeArray;
            }

            TypeArray typeArrayNew = new TypeArray();
            for (int i = 0; i < srcTypeArray.Count; ++i)
            {
                typeArrayNew.Add(SubstTypeCore(srcTypeArray[i], context));
            }
            return AllocParams(typeArrayNew);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeArray (2)
        //
        /// <summary>
        /// Create a new TypeArray instance by substituting
        /// type arguments of classArgs and methodArgs to srcTypeArray.
        /// </summary>
        /// <param name="srcTypeArray"></param>
        /// <param name="classArgs"></param>
        /// <param name="methodArgs"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray SubstTypeArray(
            TypeArray srcTypeArray,
            TypeArray classTypeArgs,
            TypeArray methodTypeArgs,   // = NULL,
            SubstTypeFlagsEnum flags)   // = SubstTypeFlags::NormNone);
        {
            if (srcTypeArray == null || srcTypeArray.Count == 0)
            {
                return srcTypeArray;
            }

            SubstContext context = new SubstContext(classTypeArgs, methodTypeArgs, flags);
            if (context.FNop())
            {
                return srcTypeArray;
            }

            TypeArray substArray = new TypeArray();
            for (int i = 0; i < srcTypeArray.Count; ++i)
            {
                substArray.Add(SubstTypeCore(srcTypeArray[i], context));
            }
            return AllocParams(substArray);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeArray (3)
        //
        /// <summary></summary>
        /// <param name="taSrc"></param>
        /// <param name="atsCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray SubstTypeArray(
            TypeArray srcTypeArray,
            AGGTYPESYM aggTypeSym,
            TypeArray methodTypeArgs) // = null)
        {
            return SubstTypeArray(
                srcTypeArray,
                aggTypeSym != null ? aggTypeSym.AllTypeArguments : null,
                methodTypeArgs,
                SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeArray (4)
        //
        /// <summary></summary>
        /// <param name="taSrc"></param>
        /// <param name="typeCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray SubstTypeArray(
            TypeArray srcTypeArray,
            TYPESYM typeSym,
            TypeArray methodTypeArgs) // = null)
        {
            return SubstTypeArray(
                srcTypeArray,
                typeSym.IsAGGTYPESYM ? (typeSym as AGGTYPESYM).AllTypeArguments : null,
                methodTypeArgs,
                SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeArray (5)
        //
        /// <summary></summary>
        /// <param name="taSrc"></param>
        /// <param name="grfst"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray SubstTypeArray(
            TypeArray srcTypeArray,
            SubstTypeFlagsEnum flags)
        {
            return SubstTypeArray(srcTypeArray, (TypeArray)null, (TypeArray)null, flags);
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypeArrays (1)
        //
        /// <summary>
        /// <para>Test equality of two type arrays after substituting in the second.</para>
        /// <para>Return true if dstTypeArray is equal to srcTypeArray processed by context.</para>
        /// </summary>
        /// <param name="dstTypeArray"></param>
        /// <param name="srcTypeArray"></param>
        /// <param name="context"></param>
        //------------------------------------------------------------
        internal bool SubstEqualTypeArrays(
            TypeArray dstTypeArray,
            TypeArray srcTypeArray,
            SubstContext context)
        {
            if (dstTypeArray == null || srcTypeArray == null)
            {
                return false;
            }
            if (TypeArray.Size(dstTypeArray) != TypeArray.Size(srcTypeArray))
            {
                return false;
            }
            if (TypeArray.Size(dstTypeArray) == 0)
            {
                return true;
            }

            // Handle the simple common cases first.
            if (dstTypeArray.Equals(srcTypeArray))
            {
                DebugUtil.Assert(dstTypeArray.Equals(SubstTypeArray(srcTypeArray, context)));
                return true;
            }

            if (context == null || context.FNop())
            {
                return false;
            }

            for (int i = 0; i < dstTypeArray.Count; ++i)
            {
                if (!SubstEqualTypesCore(dstTypeArray[i], srcTypeArray[i], context))
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypeArrays (2)
        //
        /// <summary></summary>
        /// <param name="dstTypeArray"></param>
        /// <param name="srcTypeArray"></param>
        /// <param name="typeArgsCls"></param>
        /// <param name="typeArgsMeth"></param>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        internal bool SubstEqualTypeArrays(
            TypeArray dstTypeArray,
            TypeArray srcTypeArray,
            TypeArray typeArgsCls,
            TypeArray typeArgsMeth,    // = NULL,
            SubstTypeFlagsEnum flags)  // = SubstTypeFlags::NormNone);
        {
            if (dstTypeArray == null || srcTypeArray == null)
            {
                return false;
            }
            if (TypeArray.Size(dstTypeArray) != TypeArray.Size(srcTypeArray))
            {
                return false;
            }
            if (TypeArray.Size(dstTypeArray) == 0)
            {
                return true;
            }

            // Handle the simple common cases first.
            if (dstTypeArray.Equals(srcTypeArray))
            {
                DebugUtil.Assert(
                    dstTypeArray.Equals(
                        SubstTypeArray(srcTypeArray, typeArgsCls, typeArgsMeth, flags)));
                return true;
            }

            SubstContext context = new SubstContext(typeArgsCls, typeArgsMeth, flags);

            if (context.FNop())
            {
                return false;
            }

            for (int i = 0; i < dstTypeArray.Count; i++)
            {
                if (!SubstEqualTypesCore(dstTypeArray[i], srcTypeArray[i], context))
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypeArrays (3)
        //
        /// <summary></summary>
        /// <param name="dstTypeArray"></param>
        /// <param name="srcTypeArray"></param>
        /// <param name="aggTypeSym"></param>
        /// <param name="methodTypeArguments"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SubstEqualTypeArrays(
            TypeArray dstTypeArray,
            TypeArray srcTypeArray,
            AGGTYPESYM aggTypeSym,
            TypeArray methodTypeArguments) // = null
        {
            return SubstEqualTypeArrays(
                dstTypeArray,
                srcTypeArray,
                aggTypeSym != null ? aggTypeSym.AllTypeArguments : null,
                methodTypeArguments,
                SubstTypeFlagsEnum.NormNone);
        }

        //------------------------------------------------------------
        // BSYMMGR.TypeContainsType
        //
        /// <summary>
        /// <para>Looks for typeFind in type. Returns true iff it appears.</para>
        /// <para>If both are same, return true.</para>
        /// <para>In case that typeSym is of AGGTYPESYM, 
        /// if findTypeSym is equal to a type argument of typeSym, return true.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="findTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool TypeContainsType(TYPESYM typeSym, TYPESYM findTypeSym)
        {
        LRecurse: // Label used for "tail" recursion.

            if (typeSym == findTypeSym)
            {
                return true;
            }

            switch (typeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SYM kind in TypeContainsType");
                    return false;

                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.UNITSYM:
                    // There should only be a single instance of these.
                    DebugUtil.Assert(findTypeSym.Kind != typeSym.Kind);
                    return false;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.NUBSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.PTRSYM:
                    typeSym = typeSym.ParentSym as TYPESYM;
                    goto LRecurse;

                case SYMKIND.AGGTYPESYM:
                    { // BLOCK
                        AGGTYPESYM ats = typeSym as AGGTYPESYM;

                        for (int i = 0; i < ats.AllTypeArguments.Count; ++i)
                        {
                            if (TypeContainsType(ats.AllTypeArguments[i], findTypeSym))
                            {
                                return true;
                            }
                        }
                    }
                    return false;

                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym != null)
                    {
                        ERRORSYM errorSym = typeSym as ERRORSYM;
                        DebugUtil.Assert(
                            errorSym.ParentSym != null &&
                            !String.IsNullOrEmpty(errorSym.Name) && errorSym.TypeArguments != null);

                        for (int i = 0; i < errorSym.TypeArguments.Count; ++i)
                        {
                            if (TypeContainsType(errorSym.TypeArguments[i], findTypeSym))
                            {
                                return true;
                            }
                        }
                        if (errorSym.ParentSym.IsTYPESYM)
                        {
                            typeSym = errorSym.ParentSym as TYPESYM;
                            goto LRecurse;
                        }
                    }
                    return false;

                case SYMKIND.TYVARSYM:
                    return false;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.TypeContainsGenerics
        //
        /// <summary>
        // Determines whether the type contains any generic types
        // - either constructed types or type variables.
        /// </summary>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool TypeContainsGenerics(TYPESYM typeSym)
        {
        LRecurse: // Label used for "tail" recursion.
            switch (typeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SYM kind in TypeContainsGenerics");
                    return false;

                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                    return false;

                case SYMKIND.ARRAYSYM:
                case SYMKIND.PARAMMODSYM:
                case SYMKIND.MODOPTTYPESYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.PINNEDSYM:
                    typeSym = typeSym.ParentSym as TYPESYM;
                    goto LRecurse;

                case SYMKIND.NUBSYM:
                    return true;

                case SYMKIND.AGGTYPESYM:
                    return (typeSym as AGGTYPESYM).AllTypeArguments.Count > 0;

                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym != null)
                    {
                        ERRORSYM err = typeSym as ERRORSYM;
                        DebugUtil.Assert(
                            err.ParentSym != null &&
                            err.Name != null &&
                            err.TypeArguments != null);

                        if (err.TypeArguments.Count > 0)
                        {
                            return true;
                        }
                        if (err.ParentSym.IsTYPESYM)
                        {
                            typeSym = err.ParentSym as TYPESYM;
                            goto LRecurse;
                        }
                    }
                    return false;

                case SYMKIND.TYVARSYM:
                    return true;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.UnifyTypes
        //
        /// <summary>
        /// <para>Determine if two TYPESYM instances are identical.</para>
        /// </summary>
        /// <param name="typeSym1"></param>
        /// <param name="typeSym2"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool UnifyTypes(TYPESYM typeSym1, TYPESYM typeSym2, UnifyContext context)
        {
            // First substitute using the current unifications.
            typeSym1 = SubstType(typeSym1, context);
            typeSym2 = SubstType(typeSym2, context);

            // Note: skipping the SubstType calls when we goto LRecurse is an optimization.
            // They're not needed because we never modify the mapping before jumping here.
        LRecurse: ; // Label used for "tail" recursion.

#if DEBUG
            // typeSym1 and typeSym2 should not contain any type variables that are already mapped.
            for (int i = 0; i < context.ClassTypeVariables.Count; ++i)
            {
                TYVARSYM var = context.ClassTypeVariables.ItemAsTYVARSYM(i);
                if (context.ClassTypeArguments[i] != var)
                {
                    DebugUtil.Assert(!TypeContainsType(typeSym1, var));
                    DebugUtil.Assert(!TypeContainsType(typeSym2, var));
                }
            }
            for (int i = 0; i < context.MethodTypeVariables.Count; ++i)
            {
                TYVARSYM var = context.MethodTypeVariables.ItemAsTYVARSYM(i);
                if (context.MethodTypeArguments[i] != var)
                {
                    DebugUtil.Assert(!TypeContainsType(typeSym1, var));
                    DebugUtil.Assert(!TypeContainsType(typeSym2, var));
                }
            }
#endif // DEBUG

            if (typeSym1 == typeSym2) return true;

            if (typeSym2.IsTYVARSYM)
            {
                // Swap them.
                TYPESYM t = typeSym2;
                typeSym2 = typeSym1;
                typeSym1 = t;
            }
            // typeSym1 and typeSym2 are TYVARSYM or not:
            //   yes          yes
            //   yes          no
            //   no           yes  <- impossible
            //   no           no

            // The only way typeSym2 is a type variable is if both are type variables.
            DebugUtil.Assert(!typeSym2.IsTYVARSYM || typeSym1.IsTYVARSYM);

            switch (typeSym1.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SYM kind in UnifyTypes");
                    return false;

                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                    // There should only be a single instance of these.
                    DebugUtil.Assert(typeSym2.Kind != typeSym1.Kind);
                    return false;

                case SYMKIND.PARAMMODSYM:
                // If byref doesn't match up, we don't unify.
                // Note that unification does not distinguish between out and ref (since the CLR can't)
                // so we don't require that typeSym1.AsPARAMMODSYM.isOut == typeSym2.AsPARAMMODSYM.isOut.
                case SYMKIND.NUBSYM:
                case SYMKIND.PTRSYM:
                case SYMKIND.ARRAYSYM:
                    if (typeSym2.Kind != typeSym1.Kind ||
                        typeSym1.IsARRAYSYM && (typeSym2 as ARRAYSYM).Rank != (typeSym1 as ARRAYSYM).Rank)
                    {
                        return false;
                    }
                    typeSym1 = typeSym1.ParentSym as TYPESYM;
                    typeSym2 = typeSym2.ParentSym as TYPESYM;
                    goto LRecurse;

                case SYMKIND.AGGTYPESYM:
                    if (typeSym2.Kind != SYMKIND.AGGTYPESYM) return false;
                    { // BLOCK
                        AGGTYPESYM ats1 = typeSym1 as AGGTYPESYM;
                        AGGTYPESYM ats2 = typeSym2 as AGGTYPESYM;

                        if (ats1.GetAggregate() != ats2.GetAggregate())
                        {
                            return false;
                        }
                        DebugUtil.Assert(ats1.AllTypeArguments.Count == ats2.AllTypeArguments.Count);

                        // All the args must unify.
                        for (int i = 0; i < ats1.AllTypeArguments.Count; ++i)
                        {
                            if (!UnifyTypes(ats1.AllTypeArguments[i], ats2.AllTypeArguments[i], context))
                            {
                                return false;
                            }
                        }
                    }
                    return true;

                case SYMKIND.ERRORSYM:
                    if (!typeSym2.IsERRORSYM || typeSym1.ParentSym == null || typeSym2.ParentSym == null)
                    {
                        return false;
                    }
                    {
                        ERRORSYM err1 = typeSym1 as ERRORSYM;
                        ERRORSYM err2 = typeSym2 as ERRORSYM;

                        DebugUtil.Assert(err1.ParentSym != null && err1.Name != null && err1.TypeArguments != null);
                        DebugUtil.Assert(err2.ParentSym != null && err2.Name != null && err2.TypeArguments != null);

                        if (err1.Name != err2.Name || err1.TypeArguments.Count != err2.TypeArguments.Count)
                        {
                            return false;
                        }

                        if (err1.ParentSym != err2.ParentSym)
                        {
                            if (!err1.ParentSym.IsTYPESYM != !err2.ParentSym.IsTYPESYM)
                            {
                                return false;
                            }
                            if (err1.ParentSym.IsTYPESYM &&
                                !UnifyTypes(err1.ParentSym as TYPESYM, err2.ParentSym as TYPESYM, context))
                            {
                                return false;
                            }
                        }

                        // All the args must unify.
                        for (int i = 0; i < err1.TypeArguments.Count; ++i)
                        {
                            if (!UnifyTypes(err1.TypeArguments[i], err2.TypeArguments[i], context))
                            {
                                return false;
                            }
                        }
                    }
                    return true;

                case SYMKIND.TYVARSYM:
                    { // BLOCK
                        //TYPESYM ** ptype1; ptype1 = context.GetSlot(typeSym1.AsTYVARSYM);
                        //DebugUtil.Assert(ptype1);
                        TYPESYM type1 = context.GetSlot(typeSym1 as TYVARSYM);

                        // Since we substituted at the top, typeSym1 should always be unmapped.
                        //DebugUtil.Assert(*ptype1 == typeSym1);
                        DebugUtil.Assert(type1 == typeSym1);

                        switch (typeSym2.Kind)
                        {
                            default:
                                DebugUtil.Assert(false, "Bad SYM kind in UnifyTypes");
                                return false;

                            case SYMKIND.NULLSYM:
                            case SYMKIND.VOIDSYM:
                            case SYMKIND.PARAMMODSYM:
                            case SYMKIND.PTRSYM:
                                // Can't assign these to a type variable.
                                return false;

                            case SYMKIND.ERRORSYM:
                                if (typeSym2.ParentSym == null)
                                {
                                    return false;
                                }
                                break;

                            case SYMKIND.NUBSYM:
                            case SYMKIND.ARRAYSYM:
                            case SYMKIND.AGGTYPESYM:
                                break;

                            case SYMKIND.TYVARSYM:
                                // Since we substituted at the top, typeSym2 should always be unmapped.
                                DebugUtil.Assert(typeSym2 == context.GetSlot(typeSym2 as TYVARSYM));
                                break;
                        }

                        // If typeSym1 is in typeSym2 then we can't unify - we have no recursive types.
                        if (TypeContainsType(typeSym2, typeSym1))
                        {
                            return false;
                        }

                        // Eliminate typeSym1 from the mappings. In particular, this will set *ptype1 to typeSym2.
                        for (int i = 0; i < context.ClassTypeVariables.Count; ++i)
                        {
                            context.ClassTypeArguments[i] = SubstTypeSingle(context.ClassTypeArguments[i], typeSym1, typeSym2);
                        }
                        for (int i = 0; i < context.MethodTypeVariables.Count; ++i)
                        {
                            context.MethodTypeArguments[i] = SubstTypeSingle(context.MethodTypeArguments[i], typeSym1, typeSym2);
                        }
#if DEBUG
                        DebugUtil.Assert(type1 == typeSym2);
                        // Any type variables that are mapped should not appear anywhere in the mappings.
                        for (int i = 0; i < context.ClassTypeVariables.Count; ++i)
                        {
                            TYVARSYM var = context.ClassTypeVariables.ItemAsTYVARSYM(i);
                            if (context.ClassTypeArguments[i] != var)
                            {
                                for (int j = 0; j < context.ClassTypeVariables.Count; ++j)
                                {
                                    DebugUtil.Assert(!TypeContainsType(context.ClassTypeArguments[j], var));
                                }
                                for (int j = 0; j < context.MethodTypeVariables.Count; ++j)
                                {
                                    DebugUtil.Assert(!TypeContainsType(context.MethodTypeArguments[j], var));
                                }
                            }
                        }
                        for (int i = 0; i < context.MethodTypeVariables.Count; ++i)
                        {
                            TYVARSYM var = context.MethodTypeVariables.ItemAsTYVARSYM(i);
                            if (context.MethodTypeArguments[i] != var)
                            {
                                for (int j = 0; j < context.ClassTypeVariables.Count; ++j)
                                {
                                    DebugUtil.Assert(!TypeContainsType(context.ClassTypeArguments[j], var));
                                }
                                for (int j = 0; j < context.MethodTypeVariables.Count; ++j)
                                {
                                    DebugUtil.Assert(!TypeContainsType(context.MethodTypeArguments[j], var));
                                }
                            }
                        }
#endif // DEBUG
                    }
                    return true;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.SetCanInferState
        //
        /// <summary></summary>
        /// <param name="methSym"></param>
        //------------------------------------------------------------
        static internal void SetCanInferState(METHSYM methSym)
        {
            DebugUtil.Assert(methSym.CanInferState == METHSYM.CanInferStateEnum.Maybe);

            // Determine whether we'll ever be able to infer.
            for (int ivar = 0; ivar < methSym.TypeVariables.Count; ivar++)
            {
                TYVARSYM tvSym = methSym.TypeVariables.ItemAsTYVARSYM(ivar);

                // See if type var is used in a parameter.
                for (int ipar = 0; ; ipar++)
                {
                    if (ipar >= methSym.ParameterTypes.Count)
                    {
                        // This type variable is not in any parameters.
                        methSym.CanInferState = METHSYM.CanInferStateEnum.No;
                        return;
                    }
                    if (TypeContainsType(methSym.ParameterTypes[ipar], tvSym))
                    {
                        break;
                    }
                }
            }

            // All type variables are used in a parameter.
            methSym.CanInferState = METHSYM.CanInferStateEnum.Yes;
        }

        //------------------------------------------------------------
        // BSYMMGR.CompareTypes
        //
        /// <summary>
        /// Determine which set of types is more specific.
        /// Returns +1 if ta1 is more specific, -1 if ta2
        /// is more specific or 0 if neither.
        /// </summary>
        /// <remarks>
        /// IF YOU CHANGE THIS METHOD
        /// BE SURE TO UPDATE CMethodMemberRef::CompareSignature IN THE LANGAUGE SERVICE!!!
        /// </remarks>
        /// <param name="typeArray1"></param>
        /// <param name="typeArray2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int CompareTypes(TypeArray typeArray1, TypeArray typeArray2)
        {
            // IF YOU CHANGE THIS METHOD BE SURE TO UPDATE CMethodMemberRef::CompareSignature
            // IN THE LANGAUGE SERVICE!!!

            if (typeArray1 == typeArray2) return 0;
            if (typeArray1.Count != typeArray2.Count)
            {
                // The one with more parameters is more specific.
                return typeArray1.Count > typeArray2.Count ? +1 : -1;
            }

            int nTot = 0;

            for (int i = 0; i < typeArray1.Count; i++)
            {
                TYPESYM type1 = typeArray1[i];
                TYPESYM type2 = typeArray2[i];
                int nParam = 0;

            LAgain:
                if (type1.Kind != type2.Kind)
                {
                    // TYVARSYM means that tha parameter is unspecified.
                    if (type1.IsTYVARSYM)
                    {
                        nParam = -1;
                    }
                    else if (type2.IsTYVARSYM)
                    {
                        nParam = +1;
                    }
                }
                else // if (type1.Kind != type2.Kind)
                {
                    switch (type1.Kind)
                    {
                        default:
                            DebugUtil.Assert(false, "Bad kind in CompareTypes");
                            break;

                        case SYMKIND.TYVARSYM:
                        case SYMKIND.ERRORSYM:
                            break;

                        case SYMKIND.PTRSYM:
                        case SYMKIND.PARAMMODSYM:
                        case SYMKIND.ARRAYSYM:
                        case SYMKIND.NUBSYM:
                            type1 = type1.ParentSym as TYPESYM;
                            type2 = type2.ParentSym as TYPESYM;
                            goto LAgain;

                        case SYMKIND.AGGTYPESYM:
                            nParam = CompareTypes(
                                (type1 as AGGTYPESYM).AllTypeArguments,
                                (type2 as AGGTYPESYM).AllTypeArguments);
                            break;
                    }
                } // if (type1.Kind != type2.Kind)

                if (nParam != 0)
                {
                    if (nTot == 0)
                    {
                        nTot = nParam;
                    }
                    else if (nParam != nTot)
                    {
                        // TypeArray which lost before wins on the current parameter.
                        return 0;
                    }
                }
            } // for (int i = 0; i < typeArray1.Count; i++)

            return nTot;

            // IF YOU CHANGE THIS METHOD BE SURE TO UPDATE CMethodMemberRef::CompareSignature
            // IN THE LANGAUGE SERVICE!!!
        }

        //------------------------------------------------------------
        // BSYMMGR.BuildIfacesAll
        //
        /// <summary></summary>
        /// <param name="errorSym"></param>
        /// <param name="ifaceSymList"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeArray BuildIfacesAll(SYM errorSym, List<AGGTYPESYM> ifaceSymList, UnifyContext context)
        {
            if (ifaceSymList == null || ifaceSymList.Count == 0)
            {
                return BSYMMGR.EmptyTypeArray;
            }

            int ifaceCount = ifaceSymList.Count;
            TypeArray allIfaces;
            bool hasErrors = false;

            Stack<TYPESYM> ifaceStack = new Stack<TYPESYM>();

            // Always add to the front so process the interfaces in the reverse order.
            for (int i = ifaceCount; --i >= 0; )
            {
                AGGTYPESYM ifaceSym = ifaceSymList[i];

                hasErrors |= !AddUniqueInterfaces(
                    errorSym,
                    ifaceStack,
                    ifaceSym,
                    context);
            }

            TypeArray ifaceArray = new TypeArray();
            ifaceArray.Add(ifaceStack);
            allIfaces = AllocParams(ifaceArray);

#if DEBUG
            // Check that we indeed added them all and in the correct order!
            for (int i = 0; i < ifaceCount; i++)
            {
                DbgCheckIfaceListOrder(ifaceSymList[i], allIfaces, !hasErrors);
            }
#endif
            return allIfaces;
        }

        //------------------------------------------------------------
        // BSYMMGR.CheckForUnifyingInstantiation
        //
        /// <summary>
        /// <para>*pfErrors is [in / out]. This routine never sets it to false.
        /// Returns true iff there was a duplicate or conflict.</para>
        /// <para>Return true if aggTypeSym equals to one in the specified range of aggTypeList.
        /// If context is not null, substitute type arguments and check.</para>
        /// </summary>
        /// <param name="errorSym"></param>
        /// <param name="aggTypeStack"></param>
        /// <param name="aggTypeSym"></param>
        /// <param name="context"></param>
        /// <param name="hasErrors"></param>
        /// <returns>True if the same one is in.</returns>
        //------------------------------------------------------------
        internal bool CheckForUnifyingInstantiation(
            SYM errorSym,
            IEnumerable<TYPESYM> aggTypeList,
            AGGTYPESYM aggTypeSym,
            UnifyContext context,
            out bool hasErrors
            )
        {
            // Check for more than one instantiation of same interface.
            // Allow distinct intstantiations as long as they can never unify.

            hasErrors = false;
            DebugUtil.Assert(aggTypeList != null && aggTypeSym != null);

            foreach (TYPESYM sym in aggTypeList)
            {
                AGGTYPESYM currentSym = sym as AGGTYPESYM;
                if (currentSym == null) continue;
                if (currentSym == aggTypeSym) return true;

                if (context != null && currentSym.GetAggregate() == aggTypeSym.GetAggregate())
                {
                    // The aggs are the same but the types aren't, so the agg MUST be generic.
                    DebugUtil.Assert(currentSym.GetAggregate().AllTypeVariables.Count > 0);
                    DebugUtil.Assert(
                        context.ClassTypeVariables.Count > 0 ||
                        context.MethodTypeVariables.Count > 0);

                    context.Clear();
                    if (UnifyTypes(currentSym, aggTypeSym, context))
                    {
                        if (errorSym.IsAGGSYM && (errorSym as AGGSYM).IsSource || errorSym.IsTYVARSYM)
                        {
                            Error(
                                CSCERRID.ERR_UnifyingInterfaceInstantiations,
                                new ErrArg(errorSym),
                                new ErrArg(currentSym));
                        }
                        else
                        {
                            errorSym.SetBogus(true);
                        }
                        // Add it anyway.
                        return false;
                    }
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // BSYMMGR.AddUniqueInterfaces
        //
        /// <summary>
        /// Builds the recursive closure of an interface list.
        /// <para>For example, 
        /// <code>
        /// interface A&lt;T&gt; { }
        /// interface B&lt;T&gt; : A&lt;A&lt;T&gt;&gt; { }
        /// interface C : B&lt;String&gt;, A&lt;String&gt;, B&lt;A&lt;String&gt;&gt; { }
        /// </code>
        /// C implements
        /// <code>
        /// B&lt;String&gt;, A&lt;A&lt;String&gt;&gt;, A&lt;String&gt;,
        /// B&lt;A&lt;String&gt;&gt;, A&lt;A&lt;A&lt;String&gt;&gt;&gt;
        /// </code>
        /// The substitution step is used as always when accessing information about a base class or
        /// interface which may be specialized to a particular type.
        /// Returns false if there were some errors.</para>
        /// </summary>
        //------------------------------------------------------------
        internal bool AddUniqueInterfaces(
            SYM errorSym,
            Stack<TYPESYM> aggTypeStack,
            AGGTYPESYM aggTypeSym,
            UnifyContext context)
        {
            // See if it's already there.
            bool hasErrors = false;

            // Check duplication. If found, not add and return.
            if (CheckForUnifyingInstantiation(
                errorSym,
                aggTypeStack,
                aggTypeSym,
                context,
                out hasErrors))
            {
                return !hasErrors;
            }

            // Add the children.
            TypeArray allIfaces = aggTypeSym.GetIfacesAll();
            for (int i = allIfaces.Count; --i >= 0; )
            {
                AGGTYPESYM childSym = allIfaces[i] as AGGTYPESYM;
                DebugUtil.Assert(childSym.IsInterfaceType());

                if (!CheckForUnifyingInstantiation(
                    errorSym,
                    aggTypeStack,
                    childSym,
                    context,
                    out hasErrors))
                {
                    aggTypeStack.Push(childSym);
                }
            }
            aggTypeStack.Push(aggTypeSym);
            return !hasErrors;
        }

#if DEBUG
        //------------------------------------------------------------
        // BSYMMGR.DbgCheckIfaceListOrder
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifaceSym"></param>
        /// <param name="ifaceArray"></param>
        /// <param name="checkTypes"></param>
        //------------------------------------------------------------
        internal void DbgCheckIfaceListOrder(
            AGGTYPESYM ifaceSym,
            TypeArray ifaceArray,
            bool checkTypes)
        {
            DebugUtil.Assert(ifaceSym.IsInterfaceType());

            //--------------------------------------------------
            // Make sure ifaceSym is in the list.
            //--------------------------------------------------
            bool isFound = false;
            for (int i = 0; i < ifaceArray.Count; ++i)
            {
                AGGTYPESYM tempAts = ifaceArray[i] as AGGTYPESYM;
                if (tempAts == ifaceSym)
                {
                    if (isFound)
                    {
                        DebugUtil.Assert(false, "ifaceSym is in the list twice!");
                        return;
                    }
                    if (tempAts != ifaceSym && checkTypes) // ?
                    {
                        DebugUtil.Assert(false, "wrong instantiation of the interface!");
                        return;
                    }
                    isFound = true;
                }
            }
            if (!isFound)
            {
                DebugUtil.Assert(false, "ifaceSym is not in the list!");
                return;
            }

            //--------------------------------------------------
            // Make sure all the baseAts interfaces are in the list and after ifaceSym.
            //--------------------------------------------------
            TypeArray baseIfacesAll = ifaceSym.GetIfacesAll();
            for (int i = 0; i < baseIfacesAll.Count; i++)
            {
                AGGTYPESYM baseAts = baseIfacesAll[i] as AGGTYPESYM;
                bool isBefore = true;
                isFound = false;

                for (int j = 0; j < ifaceArray.Count; j++)
                {
                    AGGTYPESYM tempAts = ifaceArray[j] as AGGTYPESYM;
                    if (tempAts == baseAts)
                    {
                        if (isFound)
                        {
                            DebugUtil.Assert(false, "baseAts is in the list twice!");
                            return;
                        }
                        if (isBefore)
                        {
                            DebugUtil.Assert(false, "baseAts is before ifaceSym!");
                            return;
                        }
                        if (tempAts != baseAts && checkTypes)
                        {
                            DebugUtil.Assert(false, "wrong instantiation of the baseAts interface!");
                            return;
                        }
                        isFound = true;
                    }
                    else if (tempAts == ifaceSym)
                    {
                        DebugUtil.Assert(isBefore);
                        isBefore = false;
                    }
                }
                if (!isFound)
                {
                    DebugUtil.Assert(false, "baseAts is not in the list!");
                    return;
                }
            }
        }
#endif // DEBUG

        //------------------------------------------------------------
        // BSYMMGR.AddToGlobalSymList
        //
        /// <summary>
        /// <para>Add a sym to a symbol list. The memory for the list is allocated from
        /// the global symbol area, so this is appropriate only for global symbols.</para>
        /// <para>The calls should pass a pointer to a local that's initialized to
        /// point to the PSYMLIST that's the head of the list.</para>
        /// <para>If arguments are valid, add sym to symList</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="symList"></param>
        //------------------------------------------------------------
        internal void AddToGlobalSymList(SYM sym, List<SYM> symList)
        {
            if (sym == null || symList == null) return;
            if (sym.IsLocal) return;
            symList.Add(sym);   // AddToSymList(sym, symList);
        }

        //------------------------------------------------------------
        // BSYMMGR.AddToGlobalNameList
        //
        /// <summary>
        /// <para>(sscli) 
        /// Add a name to a symbol list.
        /// The memory for the list is allocated from the global symbol area,
        /// so this is appropriate only for global symbols.
        /// The calls should pass a pointer to a local that's initialized to
        /// point to the PNAMELIST that's the head of the list.</para>
        /// <para>Only call List&lt;string&gt;.Add method.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void AddToGlobalNameList(string name, List<string> nameLink)
        {
            AddToNameList(name, nameLink);
        }

        //------------------------------------------------------------
        // BSYMMGR.AddToGlobalAttrList
        //
        /// <summary>
        /// <para>Add an attribute node to a list.
        /// The memory for the list is allocated from the global symbol area.</para>
        /// <para>The calls should pass a pointer to a local that's initialized to
        /// point to the PATTRLIST that's the head of the list.</para>
        /// </summary>
        //------------------------------------------------------------
        internal void AddToGlobalAttrList(BASENODE node, PARENTSYM context, List<ATTRINFO> attrLink)
        {
            AddToAttrList(node, context, attrLink);
        }

#if DEBUG
        // Dump a text representation of a symbol to stdout.
        internal void DumpSymbol(SYM sym, int indent)
        {
            throw new NotImplementedException("BSYMMGR.DumpSymbol");

            //    int i;
            //
            //    if (sym == NULL) {
            //        wprintf(L"*NULL*");
            //        return;
            //    }
            //
            //    switch (sym.getKind()) {
            //    case SK_NSSYM:
            //        {
            //            for (NSDECLSYM * decl = sym.AsNSSYM.DeclFirst(); decl; decl = decl.DeclNext()) {
            //                DumpSymbol(decl, indent);
            //            }
            //        }
            //        break;
            //    case SK_NSDECLSYM:
            //        PrintIndent(indent);
            //        // We gave the NSDECL the same name as the NS.
            //        wprintf(L"namespace %ls {\n", sym.name.text);
            //
            //        DumpChildren(sym.AsPARENTSYM, indent);
            //
            //        PrintIndent(indent);
            //        wprintf(L"}\n");
            //        break;
            //
            //    case SK_AGGDECLSYM:
            //        if (!sym.AsAGGDECLSYM.IsFirst())
            //            return;
            //
            //        sym = sym.AsAGGDECLSYM.Agg();
            //        // Fall through.
            //    case SK_AGGSYM:
            //        PrintIndent(indent);
            //
            //        DumpAccess(sym.AsAGGSYM.GetAccess());
            //
            //        switch (sym.AsAGGSYM.AggKind()) {
            //        case AggKind::Class:
            //            wprintf(L"class");
            //            break;
            //        case AggKind::Interface:
            //            wprintf(L"interface");
            //            break;
            //        case AggKind::Enum:
            //            wprintf(L"enum");
            //            break;
            //        case AggKind::Struct:
            //            wprintf(L"struct");
            //            break;
            //        case AggKind::Delegate:
            //            wprintf(L"delegate");
            //            break;
            //        default:
            //            wprintf(L"*unknown*");
            //            break;
            //        }
            //
            //        if (sym.AsAGGSYM.typeVarsThis.size > 0) {
            //            wprintf(L" %ls<", sym.name.text);
            //            AGGSYM * agg = sym.AsAGGSYM;
            //            for (int i = 0; i < agg.typeVarsThis.size; i++) {
            //                if (i > 0)
            //                    wprintf( L", ");
            //                DumpSymbol(agg.typeVarsThis.Item(i));
            //            }
            //            wprintf(L">");
            //        } else
            //            wprintf(L" %ls ", sym.name.text);
            //
            //        // Base class.
            //        if (sym.AsAGGSYM.baseClass) {
            //            wprintf(L": ");
            //            if (sym.AsAGGSYM.IsEnum())
            //                DumpType(sym.AsAGGSYM.underlyingType);
            //            else
            //                DumpType(sym.AsAGGSYM.baseClass);
            //        }
            //        if (TypeArray::Size(sym.AsAGGSYM.ifaces) > 0) {
            //            if (sym.AsAGGSYM.baseClass)
            //                wprintf(L", ");
            //            else
            //                wprintf(L": ");
            //
            //            TypeArray * ifaces = sym.AsAGGSYM.ifaces;
            //            for (int i = 0; i < ifaces.size; i++) {
            //                if (i > 0)
            //                    wprintf(L", ");
            //                DumpType(ifaces.Item(i));
            //            }
            //        }
            //
            //        wprintf(L"\n");
            //        PrintIndent(indent);
            //        wprintf(L"{\n");
            //
            //        DumpChildren(sym.AsPARENTSYM, indent);
            //
            //        PrintIndent(indent);
            //        wprintf(L"}\n\n");
            //        break;
            //
            //    case SK_MEMBVARSYM:
            //        PrintIndent(indent);
            //        DumpAccess(sym.AsMEMBVARSYM.GetAccess());
            //
            //        if (sym.AsMEMBVARSYM.isConst)
            //            wprintf(L"const ");
            //        else {
            //            if (sym.AsMEMBVARSYM.isStatic)
            //                wprintf(L"static ");
            //            if (sym.AsMEMBVARSYM.isReadOnly)
            //                wprintf(L"readonly ");
            //        }        
            //
            //        if (sym.AsMEMBVARSYM.fixedAgg) {
            //            wprintf(L"fixed ");
            //            DumpType(sym.AsVARSYM.type.AsPTRSYM.baseType());
            //        } else {
            //            DumpType(sym.AsVARSYM.type);
            //        }
            //
            //        wprintf(L" %ls", sym.AsVARSYM.name.text);
            //
            //        if (sym.AsMEMBVARSYM.isConst && !sym.AsVARSYM.type.IsERRORSYM) {
            //            wprintf(L" = ");
            //            DumpConst(sym.AsVARSYM.type, & sym.AsMEMBVARSYM.constVal);
            //        } else if (sym.AsMEMBVARSYM.fixedAgg) {
            //            wprintf(L"[");
            //            AGGSYM *aggInt = GetReqPredefAgg(PT_INT);
            //            if (aggInt && aggInt.getThisType())
            //                DumpConst(aggInt.getThisType(), & sym.AsMEMBVARSYM.constVal);
            //            wprintf(L"]");
            //        }
            //
            //        wprintf(L";");
            //
            //        if (sym.AsMEMBVARSYM.getBogus())
            //            wprintf(L"  /* UNSUPPORTED or BOGUS */");
            //
            //        wprintf(L"\n");
            //        break;
            //
            //    case SK_TYVARSYM:
            //        PrintIndent(indent);
            //        wprintf(L"%ls ", sym.name.text);
            //        if (sym.AsTYVARSYM != NULL) {
            //            wprintf(L": (");
            //            TypeArray * bnds = sym.AsTYVARSYM.GetBnds();
            //            if (bnds) {
            //                for (int i = 0; i < bnds.size; i++) {
            //                    if (i > 0)
            //                        wprintf(L", ");
            //                    DumpType(bnds.Item(i));
            //                }
            //            }
            //            wprintf(L")");
            //        }
            //        break;
            //
            //    case SK_AGGTYPESYM:
            //        PrintIndent(indent);
            //        DumpType(sym.AsAGGTYPESYM);
            //        wprintf(L"<...>\n");
            //        break;
            //
            //    case SK_LOCVARSYM:
            //        PrintIndent(indent);
            //        DumpType(sym.AsVARSYM.type);
            //        wprintf(L" %ls;\n", sym.AsVARSYM.name.text);
            //        break;
            //
            //    case SK_METHSYM:
            //        PrintIndent(indent);
            //        DumpAccess(sym.AsMETHSYM.GetAccess());
            //
            //        if (sym.AsMETHSYM.isStatic)
            //            wprintf(L"static ");
            //        if (sym.AsMETHSYM.isAbstract)
            //            wprintf(L"abstract ");
            //        if (sym.AsMETHSYM.isVirtual)
            //            wprintf(L"virtual ");
            //
            //        if (sym.AsMETHSYM.isCtor()) {
            //            wprintf(L"%ls", sym.AsMETHSYM.getClass().name.text);
            //        }
            //        else {
            //            DumpType(sym.AsMETHSYM.retType);
            //            wprintf(L" %ls", sym.AsMETHSYM.name ? sym.AsMETHSYM.name.text : L"impl");
            //        }
            //        if (sym.AsMETHSYM.typeVars.size)
            //            wprintf(L"<...>");
            //        wprintf(L"(");
            //
            //        for (i = 0; i < sym.AsMETHSYM.params.size; ++i) {
            //            if (i != 0)
            //                wprintf(L", ");
            //            DumpType(sym.AsMETHSYM.params.Item(i));
            //        }
            //        wprintf(L");");
            //
            //        if (sym.AsMETHSYM.getBogus())
            //            wprintf(L"  /* UNSUPPORTED or BOGUS */");
            //        wprintf(L"\n");
            //
            //        break;
            //
            //    case SK_PROPSYM:
            //        PrintIndent(indent);
            //        DumpAccess(sym.AsPROPSYM.GetAccess());
            //
            //        if (sym.AsPROPSYM.isStatic)
            //            wprintf(L"static ");
            //
            //        DumpType(sym.AsPROPSYM.retType);
            //        wprintf(L" %ls", sym.name ? sym.name.text : L"impl");
            //
            //        if (sym.AsPROPSYM.params.size > 0)
            //            wprintf(L"[");
            //
            //        for (i = 0; i < sym.AsPROPSYM.params.size; ++i) {
            //            if (i != 0)
            //                wprintf(L", ");
            //            DumpType(sym.AsPROPSYM.params.Item(i));
            //        }
            //        if (sym.AsPROPSYM.params.size > 0)
            //            wprintf(L"]");
            //
            //        if (sym.AsPROPSYM.getBogus())
            //            wprintf(L"  /* UNSUPPORTED or BOGUS */");
            //        wprintf(L"\n");
            //
            //        PrintIndent(indent);
            //        wprintf(L"{ ");
            //        if (sym.AsPROPSYM.methGet)
            //            wprintf(L"get { %ls } ", sym.AsPROPSYM.methGet.name ? sym.AsPROPSYM.methGet.name.text : L"getter");
            //        if (sym.AsPROPSYM.methSet)
            //            wprintf(L"set { %ls } ", sym.AsPROPSYM.methSet.name ? sym.AsPROPSYM.methSet.name.text : L"setter");
            //        wprintf(L"}\n");
            //
            //        break;
            //
            //    case SK_ERRORSYM:
            //        wprintf(L"*error*");
            //        break;
            //
            //    case SK_INFILESYM:
            //        if (sym.AsINFILESYM.isSource)
            //            wprintf(L"source file %ls", sym.AsINFILESYM.name.text);
            //        else
            //            wprintf(L"metadata file %ls", sym.AsINFILESYM.name.text);
            //        break;
            //
            //    case SK_SCOPESYM:
            //        PrintIndent(indent);
            //        wprintf(L"scope %p {\n", sym);
            //        DumpChildren(sym.AsPARENTSYM, indent);
            //        PrintIndent(indent);
            //        wprintf(L"}\n");
            //        break;
            //
            //    case SK_ANONSCOPESYM:
            //        PrintIndent(indent);
            //        wprintf(L"anon-scope %p\n", sym);
            //        DumpSymbol(sym.AsANONSCOPESYM.scope, indent);
            //        break;
            //
            //    case SK_CACHESYM:
            //    case SK_LABELSYM:
            //    case SK_OUTFILESYM:
            //    default:
            //        ASSERT(0);
            //        break;
            //    }
        }

        // Dump a text representation of a type to stdout.
        internal void DumpType(TYPESYM sym)
        {
            throw new NotImplementedException("BSYMMGR.DumpType");

            //    int i, rank;
            //
            //    if (sym == NULL) {
            //        wprintf(L"*NULL*");
            //        return;
            //    }
            //
            //    switch (sym.getKind()) {
            //    case SK_ARRAYSYM:
            //    {
            //        // Brackets go the reverse way that would be logical, so we need
            //        // to get down to the first non-array element type.
            //        PTYPESYM elementType = sym;
            //        while (elementType.IsARRAYSYM)
            //            elementType = elementType.AsARRAYSYM.elementType();
            //        DumpType(elementType);
            //
            //        do {
            //            rank = sym.AsARRAYSYM.rank;
            //
            //            if (rank == 0) {
            //                wprintf(L"[?]");
            //            }
            //            else if (rank == 1)
            //                wprintf(L"[]");
            //            else
            //            {
            //                // known rank > 1
            //                wprintf(L"[*");
            //                for (i = rank; i > 1; --i) {
            //                    wprintf(L",*");
            //                }
            //                wprintf(L"]");
            //            }
            //        } while ((sym = sym.AsARRAYSYM.elementType()).IsARRAYSYM);
            //        break;
            //    }
            //
            //    case SK_PTRSYM:
            //        DumpType(sym.AsPTRSYM.baseType());
            //        wprintf(L" *");
            //        break;
            //
            //    case SK_NUBSYM:
            //        DumpType(sym.AsNUBSYM.baseType());
            //        wprintf(L" ?");
            //        break;
            //
            //    case SK_PARAMMODSYM:
            //        if (sym.AsPARAMMODSYM.isOut)
            //            wprintf(L"out ");
            //        else
            //            wprintf(L"ref ");
            //        DumpType(sym.AsPARAMMODSYM.paramType());
            //        break;
            //
            //    case SK_VOIDSYM:
            //        wprintf(L"void");
            //        break;
            //
            //    case SK_NULLSYM:
            //        wprintf(L"null");
            //        break;
            //
            //    case SK_UNITSYM:
            //        wprintf(L"unit");
            //        break;
            //
            //    case SK_ANONMETHSYM:
            //        wprintf(L"<anonymous method>");
            //        break;
            //
            //    case SK_METHGRPSYM:
            //        wprintf(L"<method group>");
            //        break;
            //
            //    case SK_TYVARSYM:
            //        wprintf(L"%ls", sym.name.text);
            //        break;
            //
            //    case SK_AGGTYPESYM:
            //        if (sym.AsAGGTYPESYM.isPredefined()) {
            //            PREDEFTYPE pt = sym.AsAGGTYPESYM.getPredefType();
            //            PCWCH psz = GetNiceName(pt);
            //            if (psz) {
            //                wprintf(L"%ls", psz);
            //                break;
            //            }
            //        }
            //        wprintf(L"%ls", sym.getAggregate().name.text);
            //
            //        if (sym.AsAGGTYPESYM.typeArgsThis.size > 0)
            //            wprintf(L"{inst-type}");
            //        break;
            //
            //    case SK_ERRORSYM:
            //        wprintf(L"*error*");
            //        break;
            //
            //    default:
            //        ASSERT(0);
            //        break;
            //    }
        }
#endif

        //------------------------------------------------------------
        // BSYMMGR.FreeSymList
        //
        // Frees memory used by a symbol list, but not the symbols.
        //------------------------------------------------------------
        static internal void FreeSymList(List<SYM> symLink)
        //void BSYMMGR::FreeSymList(MEMHEAP *heap, PSYMLIST * symLink)
        {
            throw new NotImplementedException("BSYMMGR.FreeSymList");

            //    PSYMLIST list = *symLink;
            //
            //    while (list != NULL) {
            //        PSYMLIST next = list.next;
            //        heap.Free(list);
            //        list = next;        
            //    }
            //
            //    *symLink = NULL;
        }

        //------------------------------------------------------------
        // BSYMMGR.
        //------------------------------------------------------------
        //NRHEAP * getAlloc() { return allocGlobal; }
        //internal TokenToSymTable GetTokenToSymTable() { return tableTokenToSym; }
        //internal NameToSymTable GetNameToSymTable() { return tableNameToSym; }
        //void SetAidForMsCorLib(int aid)
        //{
        //    ASSERT(aidMsCorLib == kaidNil);
        //    aidMsCorLib = aid;
        //}

        //------------------------------------------------------------
        // BSYMMGR.GetNameFromPtrs
        //
        /// <summary>
        /// Create a string of five characters from two intergers through the following steps.
        /// <list type="number">
        /// <item>regard two integers as four unicode characters.</item>
        /// <item>Create a fifth charachter by collecting LSBs of four characters.
        /// (set some bits not to be a null character.)</item>
        /// <item>set 1 to the LSBs of four characters to exclude null characters.</item>
        /// <item>Output five characters as a string.</item>
        /// </list>
        /// </summary>
        /// <param name="u1"></param>
        /// <param name="u2"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetNameFromPtrs(int u1, int u2)
        {
            const int SIZE = 5;
            char[] buf = new char[SIZE];
            ushort lsbs = 1;    // store LSBs of chars of buf.
            buf[0] = (char)((u1 >> 16) & 0xFFFF);
            buf[1] = (char)(u1 & 0xFFFF);
            buf[2] = (char)((u2 >> 16) & 0xFFFF);
            buf[3] = (char)(u2 & 0xFFFF);
            buf[4] = (char)0;

            int ie = SIZE - 1;
            while (ie > 0 && buf[ie - 1] == 0) ie--;

            for (int ic = ie; --ic >= 0; )
            {
                lsbs = (ushort)(unchecked(lsbs << 1) | ((ushort)buf[ic] & (ushort)1));
                buf[ic] |= (char)1;
            }
            buf[ie] = (char)(unchecked(lsbs << 1) | 1);
#if false
            string sig = new String(buf, 0, ie + 1);
            return String.Format("Madeby{0}and{1}_{2}", u1, u2, sig);

#else
            return new String(buf, 0, ie + 1);
#endif
        }
        //NAME * BSYMMGR::GetNameFromPtrs(UINT_PTR u1, UINT_PTR u2)
        //{
        //    // We have to make sure none of the characters is zero, since NAME's don't store their length -
        //    // they rely on null termination.
        //    ASSERT(sizeof(UINT_PTR) % sizeof(WCHAR) == 0);
        //    const int cchPtr = sizeof(UINT_PTR) / sizeof(WCHAR);
        //    WCHAR rgchName[2 * cchPtr + 1];
        //
        //    *reinterpret_cast<UINT_PTR *>(rgchName) = u1;
        //    *reinterpret_cast<UINT_PTR *>(rgchName + cchPtr) = u2;
        //
        //    int cchRaw = cchPtr * 2;
        //    while (cchRaw > 0 && rgchName[cchRaw - 1] == 0)
        //        cchRaw--;
        //
        //    WCHAR chExtra = 1;
        //    for (int ich = cchRaw; --ich >= 0; ) {
        //        chExtra = (chExtra << 1) | rgchName[ich] & 1;
        //        rgchName[ich] |= 1;
        //    }
        //
        //    rgchName[cchRaw] = (chExtra << 1) | 1;
        //    return host()->getNamemgr()->AddString(rgchName, cchRaw + 1);
        //}

        //------------------------------------------------------------
        // BSYMMGR.GetSymbolName(TypeArray, AGGTYPESYM)
        //
        /// <summary>
        /// Make a symbol name for globalSymbolTable by GetNameFromPtrs method.
        /// </summary>
        /// <param name="typeArray"></param>
        /// <param name="aggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetSymbolName(TypeArray typeArray, AGGTYPESYM aggTypeSym)
        {
            DebugUtil.Assert(typeArray != null);

            string name = GetNameFromPtrs(
                typeArray.TypeArrayID,
                (aggTypeSym != null ? aggTypeSym.SymID : 0));

            DebugUtil.Assert(!String.IsNullOrEmpty(name));
            return name;
        }

        //------------------------------------------------------------
        // BSYMMGR.AddChild
        //
        /// <summary>
        /// <para>add a symbol in the regular way into a symbol table
        /// Add a named symbol to a parent scope, for later lookup.</para>
        /// <para>Add child to child list of parent.</para>
        /// <para>Register (name of child, parent) to table.</para>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        //------------------------------------------------------------
        internal static void AddChild(SYMTBL table, PARENTSYM parent, SYM child)
        {
            DebugUtil.Assert(child.NextSameNameSym == null);

            parent.AddToChildList(child);
            table.InsertChild(parent, child);
        }

        //------------------------------------------------------------
        // BSYMMGR.GetStdTypeVar
        //
        /// <summary></summary>
        /// <param name="iv"></param>
        /// <param name="pstvc"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYVARSYM GetStdTypeVar(int iv, StdTypeVarColl pstvc)
        {
            throw new NotImplementedException("BSYMMGR.GetStdTypeVar");
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeCore
        //
        /// <summary>
        /// <para>Return the type created by substituting type arguments.</para>
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstTypeCore(TYPESYM typeSym, SubstContext context)
        {
            TYPESYM srcTypeSym = null;
            TYPESYM dstTypeSym = null;

            switch (typeSym.Kind)
            {
                //--------------------------------------------------
                // If typeSym is unaffected by type arguments, return it as is.
                //--------------------------------------------------
                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.UNITSYM:
                case SYMKIND.METHGRPSYM:
                case SYMKIND.ANONMETHSYM:
                case SYMKIND.IMPLICITTYPESYM:           // CS3
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM:   // CS3
                case SYMKIND.LAMBDAEXPRSYM:             // CS3
                case SYMKIND.DYNAMICSYM:                // CS4
                    return typeSym;

                //--------------------------------------------------
                // If typeSym is PARAMMODSYM (represents ref or out), process the modified type.
                // If the obtained type is same to the original modified type, return it.
                // Otherwise, create a PARAMMODSYM
                // which represents the ref or out modification of the obtained type.
                //--------------------------------------------------
                case SYMKIND.PARAMMODSYM:
                    srcTypeSym = (typeSym as PARAMMODSYM).ParamTypeSym;
                    dstTypeSym = SubstTypeCore(srcTypeSym, context);
                    return ((dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetParamModifier(dstTypeSym, (typeSym as PARAMMODSYM).IsOut));

                //--------------------------------------------------
                // If MODOPTYPESYM, process the modified type just like PARAMMODSYM.
                //--------------------------------------------------
                case SYMKIND.MODOPTTYPESYM:
                    srcTypeSym = (typeSym as MODOPTTYPESYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCore(srcTypeSym, context);
                    return ((dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetModOptType(dstTypeSym, (typeSym as MODOPTTYPESYM).ModOptSym));

                //--------------------------------------------------
                // If ARRAYSYM, process the element type.
                //--------------------------------------------------
                case SYMKIND.ARRAYSYM:
                    srcTypeSym = (typeSym as ARRAYSYM).ElementTypeSym;
                    dstTypeSym = SubstTypeCore(srcTypeSym, context);
                    return ((dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetArray(dstTypeSym, (typeSym as ARRAYSYM).Rank, null));

                //--------------------------------------------------
                // If PTRSYM, process the underlying type.
                //--------------------------------------------------
                case SYMKIND.PTRSYM:
                    srcTypeSym = (typeSym as PTRSYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCore(srcTypeSym, context);
                    return (dstTypeSym == srcTypeSym) ? typeSym : GetPtrType(dstTypeSym);

                //--------------------------------------------------
                // If NUBSYM, process the underlying type.
                //--------------------------------------------------
                case SYMKIND.NUBSYM:
                    srcTypeSym = (typeSym as NUBSYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCore(srcTypeSym, context);
                    return (dstTypeSym == srcTypeSym) ? typeSym : GetNubType(dstTypeSym);

                //--------------------------------------------------
                // If PINNEDSYM, process the base type.
                //--------------------------------------------------
                case SYMKIND.PINNEDSYM:
                    srcTypeSym = (typeSym as PINNEDSYM).BaseTypeSym;
                    dstTypeSym = SubstTypeCore(srcTypeSym, context);
                    return (dstTypeSym == srcTypeSym) ? typeSym : GetPinnedType(dstTypeSym);

                //--------------------------------------------------
                // If AGGTYPESYM, substitute type arguments to type parameters.
                // If no type arguments, return typeSym as is.
                //--------------------------------------------------
                case SYMKIND.AGGTYPESYM:
                    AGGTYPESYM ats = typeSym as AGGTYPESYM;
                    if (TypeArray.Size(ats.AllTypeArguments) > 0)
                    {
                        TypeArray typeArgs = SubstTypeArray(ats.AllTypeArguments, context);
                        if (ats.AllTypeArguments != typeArgs)
                        {
                            return GetInstAgg(ats.GetAggregate(), typeArgs);
                        }
                    }
                    return typeSym;

                //--------------------------------------------------
                // IF ERRORSYM,
                //--------------------------------------------------
                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym != null)
                    {
                        ERRORSYM errorSym = typeSym as ERRORSYM;

                        DebugUtil.Assert(
                            errorSym.ParentSym != null &&
                            String.IsNullOrEmpty(errorSym.Name) &&
                            errorSym.TypeArguments != null);

                        PARENTSYM parentSym = errorSym.ParentSym;
                        if (parentSym.IsTYPESYM)
                        {
                            parentSym = SubstTypeCore(parentSym as TYPESYM, context);
                        }
                        TypeArray typeArgs = SubstTypeArray(errorSym.TypeArguments, context);
                        if (typeArgs != errorSym.TypeArguments || parentSym != errorSym.ParentSym)
                        {
                            return GetErrorType(parentSym, errorSym.ErrorName, typeArgs);
                        }
                    }
                    return typeSym;

                //--------------------------------------------------
                // If TYVARSYM, return the element of the specifined index in context.
                // If the index is invalid, return a default TYVARSYM instace.
                //--------------------------------------------------
                case SYMKIND.TYVARSYM:
                    {
                        TYVARSYM tyVarSym = typeSym as TYVARSYM;
                        int index = tyVarSym.TotalIndex;

                        //----------------------------------------
                        // Method
                        //----------------------------------------
                        if (tyVarSym.IsMethodTypeVariable)
                        {
                            if ((context.Flags & SubstTypeFlagsEnum.DenormMeth) != 0 && tyVarSym.ParentSym != null)
                            {
                                return typeSym;
                            }

                            DebugUtil.Assert(tyVarSym.Index == tyVarSym.TotalIndex);
                            DebugUtil.Assert(
                                context.MethodTypeArguments == null ||
                                context.MethodTypeArguments.Count == 0 ||
                                index < context.MethodTypeArguments.Count);

                            if (index < context.MethodTypeArguments.Count)
                            {
                                return context.MethodTypeArguments[index];
                            }
                            else
                            {
                                if ((context.Flags & SubstTypeFlagsEnum.NormMeth) != 0)
                                {
                                    return GetStdMethTypeVar(index);
                                }
                                else
                                {
                                    return typeSym;
                                }
                            }
                        }
                        //----------------------------------------
                        // Aggregate
                        //----------------------------------------
                        if ((context.Flags & SubstTypeFlagsEnum.DenormClass) != 0 && tyVarSym.ParentSym != null)
                        {
                            return typeSym;
                        }
                        if (index < context.ClassTypeArguments.Count)
                        {
                            return context.ClassTypeArguments[index];
                        }
                        else
                        {
                            if ((context.Flags & SubstTypeFlagsEnum.NormClass) != 0)
                            {
                                return GetStdClsTypeVar(index);
                            }
                            else
                            {
                                return typeSym;
                            }
                        }
                    }
                //--------------------------------------------------
                // Otherwise, return typeSym as is.
                //--------------------------------------------------
                default:
                    DebugUtil.Assert(false);
                    // fall through by design...
                    return typeSym;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstEqualTypesCore
        //
        /// <summary>
        /// </summary>
        /// <param name="dstTypeSym"></param>
        /// <param name="srcTypeSym"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SubstEqualTypesCore(TYPESYM dstTypeSym, TYPESYM srcTypeSym, SubstContext context)
        {
        LRecurse: // Label used for "tail" recursion.

            if (dstTypeSym == srcTypeSym)
            {
                DebugUtil.Assert(dstTypeSym == SubstTypeCore(srcTypeSym, context));
                return true;
            }

            switch (srcTypeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false, "Bad SYM kind in SubstEqualTypesCore");
                    return false;

                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.UNITSYM:
                    // There should only be a single instance of these.
                    DebugUtil.Assert(dstTypeSym.Kind != srcTypeSym.Kind);
                    return false;

                case SYMKIND.ARRAYSYM:
                    if (dstTypeSym.Kind != SYMKIND.ARRAYSYM ||
                        (dstTypeSym as ARRAYSYM).Rank != (srcTypeSym as ARRAYSYM).Rank)
                    {
                        return false;
                    }
                    goto LCheckBases;

                case SYMKIND.PARAMMODSYM:
                    if (dstTypeSym.Kind != SYMKIND.PARAMMODSYM ||
                        (dstTypeSym as PARAMMODSYM).IsOut != (srcTypeSym as PARAMMODSYM).IsOut)
                    {
                        return false;
                    }
                    goto LCheckBases;

                case SYMKIND.MODOPTTYPESYM:
                    // NOTE: This is not 100% correct because the same modopt in two different imports
                    // is represented by two different MODOPTSYMs.
                    // This shouldn't affect anything. In fact I doubt this code will ever be executed.
                    if (dstTypeSym.Kind != SYMKIND.MODOPTTYPESYM ||
                        (dstTypeSym as MODOPTTYPESYM).ModOptSym != (srcTypeSym as MODOPTTYPESYM).ModOptSym)
                    {
                        return false;
                    }
                    goto LCheckBases;

                case SYMKIND.PTRSYM:
                case SYMKIND.NUBSYM:
                    if (dstTypeSym.Kind != srcTypeSym.Kind)
                    {
                        return false;
                    }
                    goto LCheckBases;

                case SYMKIND.AGGTYPESYM:
                    if (dstTypeSym.Kind != SYMKIND.AGGTYPESYM)
                    {
                        return false;
                    }
                    { // BLOCK
                        AGGTYPESYM srcAts = srcTypeSym as AGGTYPESYM;
                        AGGTYPESYM dstAts = dstTypeSym as AGGTYPESYM;

                        if (srcAts.GetAggregate() != dstAts.GetAggregate())
                        {
                            return false;
                        }
                        DebugUtil.Assert(srcAts.AllTypeArguments.Count == dstAts.AllTypeArguments.Count);

                        // All the args must unify.
                        for (int i = 0; i < srcAts.AllTypeArguments.Count; ++i)
                        {
                            if (!SubstEqualTypesCore(dstAts.AllTypeArguments[i], srcAts.AllTypeArguments[i], context))
                            {
                                return false;
                            }
                        }
                    }
                    return true;

                case SYMKIND.ERRORSYM:
                    if (!dstTypeSym.IsERRORSYM || srcTypeSym.ParentSym == null || dstTypeSym.ParentSym == null)
                    {
                        return false;
                    }
                    {
                        ERRORSYM srcErrSym = srcTypeSym as ERRORSYM;
                        ERRORSYM dstErrSym = dstTypeSym as ERRORSYM;

                        DebugUtil.Assert(
                            srcErrSym.ParentSym != null &&
                            srcErrSym.ErrorName != null &&
                            srcErrSym.TypeArguments != null);
                        DebugUtil.Assert(
                            dstErrSym.ParentSym != null &&
                            dstErrSym.ErrorName != null &&
                            dstErrSym.TypeArguments != null);

                        if (srcErrSym.ErrorName != dstErrSym.ErrorName ||
                            srcErrSym.TypeArguments.Count != dstErrSym.TypeArguments.Count)
                        {
                            return false;
                        }

                        if (srcErrSym.ParentSym != dstErrSym.ParentSym)
                        {
                            if (!srcErrSym.ParentSym.IsTYPESYM != !dstErrSym.ParentSym.IsTYPESYM)
                            {
                                return false;
                            }
                            if (srcErrSym.ParentSym.IsTYPESYM &&
                                !SubstEqualTypesCore(dstErrSym.ParentSym as TYPESYM, srcErrSym.ParentSym as TYPESYM, context))
                            {
                                return false;
                            }
                        }

                        // All the args must unify.
                        for (int i = 0; i < srcErrSym.TypeArguments.Count; ++i)
                        {
                            if (!SubstEqualTypesCore(dstErrSym.TypeArguments[i], srcErrSym.TypeArguments[i], context))
                            {
                                return false;
                            }
                        }
                    }
                    return true;

                case SYMKIND.TYVARSYM:
                    { // BLOCK
                        TYVARSYM tvSym = srcTypeSym as TYVARSYM;
                        int index = tvSym.TotalIndex;

                        if (tvSym.IsMethodTypeVariable)
                        {
                            if ((context.Flags & SubstTypeFlagsEnum.DenormMeth) != 0 && tvSym.ParentSym != null)
                            {
                                // dstTypeSym == srcTypeSym was handled above.
                                DebugUtil.Assert(dstTypeSym != srcTypeSym);
                                return false;
                            }

                            DebugUtil.Assert(tvSym.Index == tvSym.TotalIndex);
                            DebugUtil.Assert(
                                context.MethodTypeArguments == null ||
                                tvSym.TotalIndex < context.MethodTypeArgumentCount);

                            if (index < context.MethodTypeArgumentCount)
                            {
                                return dstTypeSym == context.MethodTypeArguments[index];
                            }
                            if ((context.Flags & SubstTypeFlagsEnum.NormMeth) != 0)
                            {
                                return dstTypeSym == GetStdMethTypeVar(index);
                            }
                        }
                        else
                        {
                            if ((context.Flags & SubstTypeFlagsEnum.DenormClass) != 0 && tvSym.ParentSym != null)
                            {
                                // dstTypeSym == srcTypeSym was handled above.
                                DebugUtil.Assert(dstTypeSym != srcTypeSym);
                                return false;
                            }

                            DebugUtil.Assert(
                                context.ClassTypeArguments == null ||
                                tvSym.TotalIndex < context.ClassTypeArgumentCount);

                            if (index < context.ClassTypeArgumentCount)
                            {
                                return dstTypeSym == context.ClassTypeArguments[index];
                            }
                            if ((context.Flags & SubstTypeFlagsEnum.NormClass) != 0)
                            {
                                return dstTypeSym == GetStdClsTypeVar(index);
                            }
                        }
                    }
                    return false;
            } // switch (srcTypeSym.Kind)

        LCheckBases:
            srcTypeSym = srcTypeSym.ParentSym as TYPESYM;
            dstTypeSym = dstTypeSym.ParentSym as TYPESYM;
            goto LRecurse;
        }

        //------------------------------------------------------------
        // BSYMMGR.SubstTypeSingle
        //
        /// <summary>
        /// Search within type for instances of typeFind and replace them with typeReplace.
        /// </summary>
        /// <param name="typeSym"></param>
        /// <param name="findTypeSym"></param>
        /// <param name="replaceTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TYPESYM SubstTypeSingle(
            TYPESYM typeSym,
            TYPESYM findTypeSym,
            TYPESYM replaceTypeSym)
        {
            if (typeSym == findTypeSym) return replaceTypeSym;

            TYPESYM srcTypeSym;
            TYPESYM dstTypeSym;

            switch (typeSym.Kind)
            {
                default:
                    DebugUtil.Assert(false);
                    return null;

                case SYMKIND.NULLSYM:
                case SYMKIND.VOIDSYM:
                case SYMKIND.UNITSYM:
                    return typeSym;

                case SYMKIND.PARAMMODSYM:
                    dstTypeSym = SubstTypeSingle(
                        srcTypeSym = (typeSym as PARAMMODSYM).ParamTypeSym,
                        findTypeSym,
                        replaceTypeSym);
                    return (dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetParamModifier(dstTypeSym, (typeSym as PARAMMODSYM).IsOut);

                case SYMKIND.MODOPTTYPESYM:
                    dstTypeSym = SubstTypeSingle(
                        srcTypeSym = (typeSym as MODOPTTYPESYM).BaseTypeSym,
                        findTypeSym,
                        replaceTypeSym);
                    return (dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetModOptType(dstTypeSym, (typeSym as MODOPTTYPESYM).ModOptSym);

                case SYMKIND.ARRAYSYM:
                    dstTypeSym = SubstTypeSingle(
                        srcTypeSym = (typeSym as ARRAYSYM).ElementTypeSym,
                        findTypeSym,
                        replaceTypeSym);
                    return (dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetArray(dstTypeSym, (typeSym as ARRAYSYM).Rank, null);

                case SYMKIND.PTRSYM:
                    dstTypeSym = SubstTypeSingle(
                        srcTypeSym = (typeSym as PTRSYM).BaseTypeSym,
                        findTypeSym,
                        replaceTypeSym);
                    return (dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetPtrType(dstTypeSym);

                case SYMKIND.NUBSYM:
                    dstTypeSym = SubstTypeSingle(
                        srcTypeSym = (typeSym as NUBSYM).BaseTypeSym,
                        findTypeSym,
                        replaceTypeSym);
                    return (dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetNubType(dstTypeSym);

                case SYMKIND.PINNEDSYM:
                    dstTypeSym = SubstTypeSingle(
                        srcTypeSym = (typeSym as PINNEDSYM).BaseTypeSym,
                        findTypeSym,
                        replaceTypeSym);
                    return (dstTypeSym == srcTypeSym) ?
                        typeSym :
                        GetPinnedType(dstTypeSym);

                case SYMKIND.AGGTYPESYM:
                    if ((typeSym as AGGTYPESYM).AllTypeArguments.Count > 0)
                    {
                        AGGTYPESYM ats = typeSym as AGGTYPESYM;

                        //TYPESYM ** prgtype = STACK_ALLOC(TYPESYM *, ats.typeArgsAll.size);
                        TypeArray typeArray = new TypeArray();
                        for (int i = 0; i < ats.AllTypeArguments.Count; i++)
                        {
                            //prgtype[i] = SubstTypeSingle(ats.typeArgsAll.Item(i), findTypeSym, replaceTypeSym);
                            typeArray.Add(SubstTypeSingle(ats.AllTypeArguments[i], findTypeSym, replaceTypeSym));
                        }
                        TypeArray typeArgs = AllocParams(typeArray);
                        if (typeArgs != ats.AllTypeArguments)
                        {
                            return GetInstAgg(ats.GetAggregate(), typeArgs);
                        }
                    }
                    return typeSym;

                case SYMKIND.ERRORSYM:
                    if (typeSym.ParentSym != null)
                    {
                        ERRORSYM errorSym = typeSym as ERRORSYM;

                        DebugUtil.Assert(
                            errorSym.ParentSym != null &&
                            errorSym.ErrorName != null &&
                            errorSym.TypeArguments != null);

                        PARENTSYM parentSym = errorSym.ParentSym;
                        if (parentSym.IsTYPESYM)
                        {
                            parentSym = SubstTypeSingle(parentSym as TYPESYM, findTypeSym, replaceTypeSym);
                        }

                        TypeArray typeArgs = errorSym.TypeArguments;
                        if (typeArgs.Count > 0)
                        {
                            //TYPESYM ** prgtype = STACK_ALLOC(TYPESYM *, typeArgs.size);
                            TypeArray typeArray = new TypeArray();
                            for (int i = 0; i < typeArgs.Count; i++)
                            {
                                //prgtype[i] = SubstTypeSingle(typeArgs.Item(i), findTypeSym, replaceTypeSym);
                                typeArray.Add(SubstTypeSingle(typeArgs[i], findTypeSym, replaceTypeSym));
                            }
                            typeArgs = AllocParams(typeArray);
                        }
                        if (typeArgs != errorSym.TypeArguments || parentSym != errorSym.ParentSym)
                        {
                            return GetErrorType(parentSym, errorSym.ErrorName, typeArgs);
                        }
                    }
                    return typeSym;

                case SYMKIND.TYVARSYM:
                    return typeSym;
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.AllocSym
        //
        /// <summary>
        /// AllocSymWorker を呼び出して、指定された SYMKIND 型の SYM を作成する。
        /// </summary>
        //------------------------------------------------------------
        internal SYM AllocSym(SYMKIND symkind, string name)
        {
            SYM sym = AllocSymWorker(symkind, name, false);
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.AllocSymWorker
        //
        /// <summary>
        /// <para>Helper routine to allocator a symbol structor from the given heap,
        /// and assign the given name (if appropriate).
        /// This code is kind of repetitive,
        /// but its hard to do it in a generic way with the new operator.
        /// It's fast, which is most important.</para>
        /// <para>
        /// 指定された SYMKIND 型の SYM を作成する。
        /// </para>
        /// </summary>
        //------------------------------------------------------------
        internal static SYM AllocSymWorker(SYMKIND symkind, string name, bool hasSid)
        {
            SYM sym = null;

            switch (symkind)
            {
                //#define SYMBOLDEF(kind, global, local) \
                //    case SK_ ## kind: \
                //        sym = new(allocator, psidLast) kind; \
                //        sym.name = name; \
                //        sym.fHasSid = !!psidLast; \
                //        break;
                //#include "symkinds.h"

                case SYMKIND.NSSYM: sym = new NSSYM(); break;
                case SYMKIND.NSDECLSYM: sym = new NSDECLSYM(); break;
                case SYMKIND.NSAIDSYM: sym = new NSAIDSYM(); break;
                case SYMKIND.AGGSYM: sym = new AGGSYM(); break;
                case SYMKIND.AGGDECLSYM: sym = new AGGDECLSYM(); break;
                case SYMKIND.AGGTYPESYM: sym = new AGGTYPESYM(); break;
                case SYMKIND.FWDAGGSYM: sym = new FWDAGGSYM(); break;
                case SYMKIND.TYVARSYM: sym = new TYVARSYM(); break;
                case SYMKIND.MEMBVARSYM: sym = new MEMBVARSYM(); break;
                case SYMKIND.LOCVARSYM: sym = new LOCVARSYM(); break;
                case SYMKIND.METHSYM: sym = new METHSYM(); break;
                case SYMKIND.FAKEMETHSYM: sym = new FAKEMETHSYM(); break;
                case SYMKIND.PROPSYM: sym = new PROPSYM(); break;
                case SYMKIND.EVENTSYM: sym = new EVENTSYM(); break;
                case SYMKIND.VOIDSYM: sym = new VOIDSYM(); break;
                case SYMKIND.NULLSYM: sym = new NULLSYM(); break;
                case SYMKIND.UNITSYM: sym = new UNITSYM(); break;
                case SYMKIND.ANONMETHSYM: sym = new ANONMETHSYM(); break;
                case SYMKIND.METHGRPSYM: sym = new METHGRPSYM(); break;
                case SYMKIND.ERRORSYM: sym = new ERRORSYM(); break;
                case SYMKIND.ARRAYSYM: sym = new ARRAYSYM(); break;
                case SYMKIND.PTRSYM: sym = new PTRSYM(); break;
                case SYMKIND.PINNEDSYM: sym = new PINNEDSYM(); break;
                case SYMKIND.PARAMMODSYM: sym = new PARAMMODSYM(); break;
                case SYMKIND.MODOPTSYM: sym = new MODOPTSYM(); break;
                case SYMKIND.MODOPTTYPESYM: sym = new MODOPTTYPESYM(); break;
                case SYMKIND.NUBSYM: sym = new NUBSYM(); break;
                case SYMKIND.INFILESYM: sym = new INFILESYM(); break;
                case SYMKIND.MODULESYM: sym = new MODULESYM(); break;
                case SYMKIND.RESFILESYM: sym = new RESFILESYM(); break;
                case SYMKIND.OUTFILESYM: sym = new OUTFILESYM(); break;
                case SYMKIND.XMLFILESYM: sym = new XMLFILESYM(); break;
                case SYMKIND.SYNTHINFILESYM: sym = new SYNTHINFILESYM(); break;
                case SYMKIND.ALIASSYM: sym = new ALIASSYM(); break;
                case SYMKIND.EXTERNALIASSYM: sym = new EXTERNALIASSYM(); break;
                case SYMKIND.SCOPESYM: sym = new SCOPESYM(); break;
                case SYMKIND.CACHESYM: sym = new CACHESYM(); break;
                case SYMKIND.LABELSYM: sym = new LABELSYM(); break;
                case SYMKIND.MISCSYM: sym = new MISCSYM(); break;
                case SYMKIND.GLOBALATTRSYM: sym = new GLOBALATTRSYM(); break;
                case SYMKIND.ANONSCOPESYM: sym = new ANONSCOPESYM(); break;
                case SYMKIND.UNRESAGGSYM: sym = new UNRESAGGSYM(); break;
                case SYMKIND.IFACEIMPLMETHSYM: sym = new IFACEIMPLMETHSYM(); break;
                case SYMKIND.INDEXERSYM: sym = new INDEXERSYM(); break;

                    // CS3
                case SYMKIND.IMPLICITTYPESYM: sym = new IMPLICITTYPESYM(); break;
                case SYMKIND.IMPLICITLYTYPEDARRAYSYM: sym = new IMPLICITLYTYPEDARRAYSYM(); break;
                case SYMKIND.LAMBDAEXPRSYM: sym = new LAMBDAEXPRSYM(); break;
                // CS4
                case SYMKIND.DYNAMICSYM:sym = new DYNAMICSYM(); break;

                default:
                    // Illegal symbol kind. This shouldn't happen.
                    DebugUtil.Assert(false);
                    return null;
            }
            if (sym != null)
            {
                sym.SetName(name);
                sym.HasSID = hasSid;
                sym.Kind = symkind;
            }
            return sym;
        }

        //------------------------------------------------------------
        // BSYMMGR.AddToSymList
        //
        /// <summary>
        /// <para>Add a sym to a symbol list.
        /// The memory for the list is allocated from the provided heap.</para>
        /// <para>The calls should pass a pointer to a local that's initialized to
        /// point to the PSYMLIST that's the head of the list.</para>
        /// <para>symList.Add(sym) でよい。</para>
        /// </summary>
        //------------------------------------------------------------
        internal static void AddToSymList(SYM sym, List<SYM> symList)
        {
            if (sym != null && symList != null) symList.Add(sym);
        }

        //------------------------------------------------------------
        // BSYMMGR.AddToNameList
        //
        /// <summary>
        /// <para>(sscli)
        /// Add a name to a symbol list.
        /// The memory for the list is allocated from the provided heap.
        /// The calls should pass a pointer to a local
        /// that's initialized to point to the PNAMELIST that's the head of the list.</para>
        /// <para>Only call List&lt;string&gt;.Add method.</para>
        /// </summary>
        //------------------------------------------------------------
        internal static void AddToNameList(string name, List<string> nameList)
        {
            if (name != null && nameList != null) nameList.Add(name);
        }

        //------------------------------------------------------------
        // BSYMMGR.AddToAttrList
        //
        /// <summary>
        /// <para>Add an attribute to a list.
        /// The memory for the list is allocated from the provided heap.</para>
        /// <para>The calls should pass a pointer to a local
        /// that's initialized to point to the PATTRLIST that's the head of the list.</para>
        /// </summary>
        //------------------------------------------------------------
        internal static void AddToAttrList(BASENODE attr, PARENTSYM context, List<ATTRINFO> attrList)
        {
            if (attr == null && attrList == null) return;
            ATTRINFO info = new ATTRINFO(attr, context);
            attrList.Add(info);
        }

        //------------------------------------------------------------
        // BSYMMGR.InitPreLoad
        //
        /// <summary>
        /// <para>Build the data structures needed to make FPreLoad fast.</para>
        /// <para>Make sure the namespaces are created.</para>
        /// <para>Compute and sort hashes of the NSSYM * value and type name (sans arity indicator).</para>
        /// </summary>
        //------------------------------------------------------------
        internal void InitPreLoad()
        {
            this.predefTypeNameDictionary = new Dictionary<PredefTypeNameInfo, PREDEFTYPE>();

            for (int i = 0; i < (int)PREDEFTYPE.COUNT; ++i)
            {
                NSSYM nsCur = RootNamespaceSym;
                string name = null;

                string fullName = PredefType.InfoTable[i].fullName;
                if (String.IsNullOrEmpty(fullName))
                {
                    continue;
                }
                int fullNameLength = PredefType.InfoTable[i].fullName.Length;

                // Register all the namespaces in fullName.
                String[] parts = fullName.Split('.');
                int nsCount = parts.Length - 1;
                int idx = 0;
                for (; idx < nsCount; ++idx)
                {
                    // Get the next name component.
                    string nsName = parts[idx];
                    if (String.IsNullOrEmpty(nsName))
                    {
                        continue;
                    }

                    // Register this name to the name table.
                    Compiler.NameManager.AddString(nsName);

                    // This assumes all predefined types are not nested.
                    // Find the SYM instance for nsName with parent nsCur.
                    SYM sym = LookupGlobalSymCore(nsName, nsCur, SYMBMASK.NSSYM);
                    NSSYM nsNext = sym as NSSYM;

                    // if not found, create NSSYM.
                    if (nsNext == null)
                    {
                        nsNext = CreateNamespace(nsName, nsCur);
                    }
                    nsCur = nsNext;
                }

                // set predefTypeNameInfo
                nsCur.HasPredefineds = true;
                PredefTypeNameInfo info = new PredefTypeNameInfo();
                info.NamespaceSym = nsCur;
                name = parts[idx];
                info.Name = name;

                try
                {
                    this.predefTypeNameDictionary.Add(info, (PREDEFTYPE)i);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        //------------------------------------------------------------
        // BSYMMGR.FindPredefinedType
        //
        /// <summary>
        /// <para>finds an existing declaration for a predefined type</para>
        /// <para>returns NULL on failure.</para>
        /// <para>If isRequired is true, an error message is also given.</para>
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="assemblyId"></param>
        /// <param name="aggKind"></param>
        /// <param name="arity"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM FindPredefinedType(
            string typeName,
            int assemblyId,
            AggKindEnum aggKind,
            int arity,
            bool isRequired)
        {
            // Shouldn't be the empty string!
            if (String.IsNullOrEmpty(typeName)) return null;

            NSSYM currentNsSym = RootNamespaceSym;
            SYM tempSym = null;
            string name = null;

            // Divide typeName at each dot,
            // regard the last part as the name of type, and others as the names of namespaces.
            string[] parts = typeName.Split('.');

            int cns = parts.Length - 1; // count of namespaces

            for (int i = 0; i < cns; ++i)
            {
                name = parts[i];
                tempSym = LookupGlobalSymCore(name, currentNsSym, SYMBMASK.NSSYM);
                currentNsSym = (tempSym != null ? (tempSym as NSSYM) : null);
                if (currentNsSym == null || !currentNsSym.InAlias(assemblyId))
                {
                    // Didn't find the namespace in this assemblyId.
                    if (isRequired)
                    {
                        Error(CSCERRID.ERR_PredefinedTypeNotFound, new ErrArg(typeName));
                    }
                    return null;
                }
            }
            name = parts[cns];  // name of type

            AGGSYM ambigAggSym;
            AGGSYM badAggSym;
            AGGSYM foundAggSym = FindPredefinedTypeCore(
                name, currentNsSym, assemblyId, aggKind, arity, out ambigAggSym, out badAggSym);

            if (foundAggSym == null)
            {
                //  Didn't find the AGGSYM.
                if (badAggSym != null && (isRequired || assemblyId == Kaid.Global && badAggSym.IsSource))
                    ErrorRef(CSCERRID.ERR_PredefinedTypeBadType, new ErrArgRef(badAggSym));
                else if (isRequired)
                    Error(CSCERRID.ERR_PredefinedTypeNotFound, new ErrArg(typeName));
                return null;
            }

            if (ambigAggSym == null && assemblyId != Kaid.Global)
            {
                // Look in kaidGlobal to make sure there isn't a conflicting one.
                AGGSYM aggSym2 = FindPredefinedTypeCore(
                    name, currentNsSym, Kaid.Global, aggKind, arity, out ambigAggSym, out badAggSym);
                //ASSERT(agg2);
                if (aggSym2 != foundAggSym) ambigAggSym = aggSym2;
            }

            if (ambigAggSym != null)
            {
                Error(CSCERRID.WRN_MultiplePredefTypes,
                    new ErrArg(typeName),
                    new ErrArg(foundAggSym.FirstDeclSym.GetInputFile().Name));
            }
            return foundAggSym;
        }

        //------------------------------------------------------------
        // BSYMMGR.FindPredefinedTypeCore
        //
        /// <summary>
        /// Find the symbol of type which has the given name in the specified namespace.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nssym"></param>
        /// <param name="assemblyId"></param>
        /// <param name="aggKind"></param>
        /// <param name="arity"></param>
        /// <param name="aggAmbig"></param>
        /// <param name="aggBad"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal AGGSYM FindPredefinedTypeCore(
            string name,
            NSSYM nssym,
            int assemblyId,
            AggKindEnum aggKind,
            int arity,
            out AGGSYM aggAmbig,
            out AGGSYM aggBad)
        {
            AGGSYM aggFound = null;
            aggAmbig = null;
            aggBad = null;

            for (SYM sym = LookupGlobalSymCore(name, nssym, SYMBMASK.AGGSYM);
                sym != null;
                sym = LookupNextSym(sym, nssym, SYMBMASK.AGGSYM))
            {
                AGGSYM aggCur = sym as AGGSYM;
                if (aggCur == null ||
                    !aggCur.InAlias((int)assemblyId) ||
                    aggCur.AllTypeVariables.Count != arity)
                {
                    continue;
                }

                // if its kind is defferent, set it to aggBad and check the next sym.
                if (aggCur.AggKind != aggKind)
                {
                    if (aggBad == null) aggBad = aggCur;
                    continue;
                }
                if (aggFound != null)
                {
                    DebugUtil.Assert(aggAmbig == null);
                    aggAmbig = aggCur;
                    break;
                }
                aggFound = aggCur;
                // Is it possible that aggAmbig != null?
                if (aggAmbig == null)
                {
                    break;
                }
            }
            return aggFound;
        }

        //------------------------------------------------------------
        // BSYMMGR.Error
        //------------------------------------------------------------
        internal void Error(CSCERRID id, ErrArg arg)
        {
            ErrArg[] args = new ErrArg[1];
            args[0] = arg;
            //host().ErrorLocArgs(NULL, id, 1, &arg);
        }
        internal void Error(CSCERRID id, ErrArg arg1, ErrArg arg2)
        {
            ErrArg[] args = new ErrArg[2];
            args[0] = arg1;
            args[1] = arg2;
            //host().ErrorLocArgs(NULL, id, 1, &arg);
        }
        internal void Error(CSCERRID id, ErrArg arg1, ErrArg arg2, ErrArg arg3)
        {
            ErrArg[] args = new ErrArg[3];
            args[0] = arg1;
            args[0] = arg2;
            args[0] = arg3;
            //host().ErrorLocArgs(NULL, id, 1, &arg);
        }

        //------------------------------------------------------------
        // BSYMMGR.ErrorRef
        //------------------------------------------------------------
        internal void ErrorRef(CSCERRID id, ErrArgRef arg)
        {
            ErrArgRef[] args = new ErrArgRef[1];
            args[0] = arg;
            //host().ErrorLocArgs(NULL, id, 1, &arg);
        }
        internal void ErrorRef(CSCERRID id, ErrArgRef arg1, ErrArgRef arg2)
        {
            ErrArgRef[] args = new ErrArgRef[2];
            args[0] = arg1;
            args[1] = arg2;
            //host().ErrorLocArgs(NULL, id, 2, rgarg);
        }
        internal void ErrorRef(CSCERRID id, ErrArgRef arg1, ErrArgRef arg2, ErrArgRef arg3)
        {
            ErrArgRef[] args = new ErrArgRef[3];
            args[0] = arg1;
            args[1] = arg2;
            args[2] = arg3;
            //host().ErrorLocArgs(NULL, id, 3, rgarg);
        }

        //------------------------------------------------------------
        // BSYMMGR.FindAts
        //
        /// <summary>
        /// Searches an array of AGGTYPESYMs for a particular ats. Returns true if found.
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="symList"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool FindAts(AGGTYPESYM sym, IEnumerable<AGGTYPESYM> symList)
        {
            foreach (AGGTYPESYM ats in symList)
            {
                if (ats == sym) return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // BSYMMGR.InitFundamentalTypes
        //
        /// <summary></summary>
        /// <remarks>
        /// 2016/10/17 hirano567@hotmail.co.jp
        /// </remarks>
        //------------------------------------------------------------
        internal void InitFundamentalTypes()
        {
            this.objectTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.OBJECT, true);
            this.intTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.INT, true);
            this.boolTypeSym = Compiler.GetReqPredefType(PREDEFTYPE.BOOL, true);
        }

        //------------------------------------------------------------
        // BSYMMGR.InitAuxiliaryTypes
        //
        /// <summary></summary>
        /// <remarks>
        /// 2016/03/30 hirano567@hotmail.co.jp
        /// </remarks>
        //------------------------------------------------------------
        internal void InitAuxiliaryTypes()
        {
            INFILESYM infileSym = null;
            BAGSYM parentBagSym = null;
            DECLSYM parentDeclSym = null;

            string typeName = null;
            string typeFullName = null;
            string typeNameWithArity = null;

            bool isPrivateMetadata = false;

            //--------------------------------------------------------
            // this.genericArraySym
            //--------------------------------------------------------
            AGGSYM systemArraySym = GetOptPredefAgg(PREDEFTYPE.ARRAY);
            DebugUtil.Assert(systemArraySym != null);
            infileSym = systemArraySym.InFileSym;

            isPrivateMetadata = (
                infileSym.GetAssemblyID() != Kaid.ThisAssembly &&
                !infileSym.InternalsVisibleTo(Kaid.ThisAssembly)
                );

            parentBagSym = systemArraySym.ParentBagSym;
            NSSYM nsSym = parentBagSym as NSSYM;
            NSAIDSYM nsAidSym = GetNsAid(nsSym, infileSym.GetAssemblyID());
            NSDECLSYM nsDeclSym = null;
            for (nsDeclSym = nsSym.FirstDeclSym;
                nsDeclSym != null;
                nsDeclSym = nsDeclSym.NextDeclSym)
            {
                if (nsDeclSym.GetInputFile() == infileSym)
                {
                    break;
                }
            }

            typeName = "[GArray";
            typeFullName = "System."+typeName;
            typeNameWithArity = typeFullName + "`1";

            BAGSYM bagSym = Compiler.LookupInBagAid(
                typeName,
                parentBagSym,
                1,
                infileSym.GetAssemblyID(),
                SYMBMASK.AGGSYM) as BAGSYM;

            AGGSYM aggSym = CreateAgg(typeName, nsDeclSym);
            AGGDECLSYM aggDeclSym = CreateAggDecl(aggSym, nsDeclSym);
            aggSym.IsArityInName = false;
            aggSym.AggKind = AggKindEnum.Class;
            aggSym.Access = ACCESS.INTERNAL;
            aggSym.HasParseTree = false;

            TYVARSYM tvSym = CreateTyVar("T", aggSym);
            tvSym.Access = ACCESS.PRIVATE;
            tvSym.Index = 0;
            tvSym.TotalIndex = 0;
            tvSym.AggState = AggStateEnum.Compiled;

            TypeArray typeArgs = new TypeArray();
            typeArgs.Add(tvSym);
            typeArgs = AllocParams(typeArgs);

            aggSym.TypeVariables = typeArgs;
            aggSym.AllTypeVariables = typeArgs;

            aggSym.IsFabricated = true;
            aggSym.AggState = AggStateEnum.Compiled;

            Compiler.EnsureState(systemArraySym, AggStateEnum.Inheritance);
            Compiler.SetBaseType(aggSym, systemArraySym.GetThisType());

            TypeArray ifaces = new TypeArray();
            AGGSYM tempSym;

            tempSym = GetOptPredefAgg(PREDEFTYPE.G_ILIST);
            Compiler.EnsureState(tempSym, AggStateEnum.Inheritance);
            ifaces.Add(tempSym.GetThisType());

            tempSym = GetOptPredefAgg(PREDEFTYPE.G_ICOLLECTION);
            Compiler.EnsureState(tempSym, AggStateEnum.Inheritance);
            ifaces.Add(tempSym.GetThisType());

            tempSym = GetOptPredefAgg(PREDEFTYPE.G_IENUMERABLE);
            Compiler.EnsureState(tempSym, AggStateEnum.Inheritance);
            ifaces.Add(tempSym.GetThisType());

            Compiler.SetIfaces(aggSym, ifaces);

            this.genericArraySym = aggSym;
        }

#if DEBUG
        // Dump the children of a symbol to stdout.
        internal void DumpChildren(PARENTSYM sym, int indent)
        //void BSYMMGR::DumpChildren(PPARENTSYM sym, int indent)
        {
            throw new NotImplementedException("BSYMMGR.DumpChildren");

            //    FOREACHCHILD(sym, symChild)
            //        if (symChild.IsNSSYM || symChild.IsMEMBVARSYM ||
            //            symChild.IsMETHSYM || symChild.IsPROPSYM ||
            //            symChild.IsAGGDECLSYM || symChild.IsAGGSYM || symChild.IsERRORSYM ||
            //            symChild.IsNSDECLSYM || symChild.IsTYVARSYM ||
            //            symChild.IsLOCVARSYM || symChild.IsSCOPESYM || symChild.IsANONSCOPESYM)
            //        {
            //            DumpSymbol(symChild, indent + 2);
            //        }
            //    ENDFOREACHCHILD
        }

        // * Dump a text representation of an access level to stdout
        internal void DumpAccess(ACCESS acc)
        //void BSYMMGR::DumpAccess(ACCESS acc)
        {
            throw new NotImplementedException("BSYMMGR.DumpAccess");

            //    switch (acc) {
            //    case ACC_PRIVATE:
            //        wprintf(L"private "); break;
            //    case ACC_INTERNAL:
            //        wprintf(L"internal "); break;
            //    case ACC_PROTECTED:
            //        wprintf(L"protected "); break;
            //    case ACC_INTERNALPROTECTED:
            //        wprintf(L"internal protected "); break;
            //    case ACC_PUBLIC:
            //        wprintf(L"public "); break;
            //    default:
            //        wprintf(L"/* unknown access */"); break;
            //    }
        }

        // Dump a text representation of a constant value
        internal void DumpConst(TYPESYM type, Object constVal)
        //void BSYMMGR::DumpConst(PTYPESYM type, CONSTVAL * constVal)
        {
            throw new NotImplementedException("BSYMMGR.DumpConst");

            //    if (type.isPredefType(PT_BOOL)) {
            //        wprintf(L"%s", constVal.iVal ? L"true" : L"false");
            //        return;
            //    }   
            //    else if (type.isPredefType(PT_STRING)) {
            //        STRCONST * strConst = constVal.strVal;
            //        wprintf(L"\"");
            //        for (int i = 0; i < strConst.length; ++i) {
            //            wprintf(L"%lc", strConst.text[i]);
            //        }
            //        wprintf(L"\"");
            //        return;
            //    }
            //
            //    switch (type.fundType())
            //    {
            //    case FT_I1: case FT_I2: case FT_I4:
            //        wprintf(L"%d", constVal.iVal);
            //        break;
            //    case FT_U1: case FT_U2: case FT_U4:
            //        wprintf(L"%u", constVal.uiVal);
            //        break;
            //    case FT_I8:
            //        wprintf(L"%I64d", *constVal.longVal);
            //        break;
            //    case FT_U8:
            //        wprintf(L"%I64u", *constVal.ulongVal);
            //        break;
            //    case FT_R4:
            //    case FT_R8:
            //        wprintf(L"%g", *constVal.doubleVal);
            //        break;
            //    case FT_REF:
            //        ASSERT(constVal.init == 0);
            //        wprintf(L"null");
            //        break;
            //    case FT_STRUCT:
            //        wprintf(L"<struct>");
            //        break;
            //    default:
            //        ASSERT(0);
            //    }
        }

#if false
        //------------------------------------------------------------
        // BSYMMGR.DebugSyms
        //------------------------------------------------------------
        protected Dictionary<int, SYM> dicDebugSyms = new Dictionary<int, SYM>();

        internal void DebugSyms_Add(SYM sym)
        {
            if (sym == null) return;
            try
            {
                dicDebugSyms.Add(sym.SymID, sym);
            }
            catch (ArgumentException)
            {
            }
        }

        internal void DebugSyms(StringBuilder sb)
        {
            this.dicDebugSyms.Clear();

            foreach (KeyValuePair<int, SYM> kv in this.dicDebugSyms)
            {
                sb.Append("==============================\n");
                kv.Value.Debug(sb);
            }
        }
#endif
#endif //DEBUG
    }

    //======================================================================
    // SYMMGR
    //
    // This is the outersymbol manager used in the cscomp case.
    // It has methods not needed in the csee case.
    //======================================================================
    internal class SYMMGR : BSYMMGR
    {
        //private CController compiler = null;
        private CController controller = null;

        internal SYMMGR(CController cntr, COMPILER comp)
            : base(comp)
        {
            this.controller = cntr;
        }

        internal void SetCompiler(CController cntr) { this.controller = cntr; }
        //internal BSYMMGR getMainSymbolManager() { return (BSYMMGR)this; }
        //internal BSYMMGR MainSymbolManager { get { return this as BSYMMGR; } }
        //internal BSYMMGR AsBSYMMGR { get { return this as BSYMMGR; } }

        //------------------------------------------------------------
        // SYMMGR.CreateSourceFile
        //
        /// <summary>
        /// <para>Create a symbol representing an input file, which creates a given output file.
        /// All input files are placed as children of their output files.</para>
        /// </summary>
        //------------------------------------------------------------
        internal INFILESYM CreateSourceFile(string filename, OUTFILESYM outfile)
        {
            // Create the input file symbol.
            // Create INFILESYM with filename and outfile. outfile is parent.

            INFILESYM infileSym = CreateGlobalSym(SYMKIND.INFILESYM, filename, outfile) as INFILESYM;
            infileSym.IsSource = true;
            infileSym.LocalAssemblyID = Cor.mdTokenNil;
            DebugUtil.Assert(!infileSym.HasModuleCLSAttribute);
            infileSym.SetAssemblyID(Kaid.ThisAssembly);

            Compiler.NameManager.AddString(filename);
            ++(outfile.InputFileCount);
            ++(Compiler.InputFileCount);
            return infileSym;
        }

        //------------------------------------------------------------
        // SYMMGR.
        //------------------------------------------------------------
        //    PINFILESYM CreateSynthSourceFile(PCWSTR filename, OUTFILESYM *outfile);

        //------------------------------------------------------------
        // SYMMGR.CreateGlobalAttribute
        //
        /// <summary>
        /// Create a symbol for a global attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal GLOBALATTRSYM CreateGlobalAttribute(string name, NSDECLSYM parent)
        {
            // Create the new symbol.
            return CreateGlobalSym(SYMKIND.GLOBALATTRSYM, name, parent) as GLOBALATTRSYM;
        }

        //------------------------------------------------------------
        // SYMMGR.CreateOutFile
        //
        /// <summary>
        /// <para>Create a symbol representing an output file.
        /// All output files are placed as children of the "fileroot" symbol.</para>
        /// </summary>
        //------------------------------------------------------------
        internal OUTFILESYM CreateOutFile(
            string filename,
            TargetType target,
            string entryClass,
            FileInfo resource,
            FileInfo icon,
            string pdbfile)
        {
            if (String.IsNullOrEmpty(filename))
            {
                filename = OUTFILESYM.ProvisionalFileName;    // "*";
            }

            OUTFILESYM outSym = CreateGlobalSym(
                SYMKIND.OUTFILESYM,
                filename,
                this.fileRootSym) as OUTFILESYM;

            outSym.Target = target;
            outSym.IsResource = false;
            outSym.FileID = 0;
            outSym.ModRefID = 0;
            outSym.HasMultiEntryPointsErrorReported = false; // Always starts off as false

            if (resource != null)
            {
                outSym.MakeResFile = false;
                outSym.Win32ResourceFileInfo = resource;
                DebugUtil.Assert(icon == null);
            }
            else
            {
                outSym.MakeResFile = true;
                if (icon != null)
                {
                    outSym.Win32IconFileInfo = icon;
                }
                else
                {
                    DebugUtil.Assert(outSym.Win32IconFileInfo == null);
                }
            }

            if (entryClass != null && target!=TargetType.Library)
            {
                outSym.EntryClassName = entryClass;
            }
            else
            {
                DebugUtil.Assert(outSym.EntryClassName == null);
            }

            DebugUtil.Assert(outSym.EntryMethodSym == null);
            outSym.PublicKeyToken.Clear();    // = 0xFFFFFFFF;   // meaning uninitialized

            if (pdbfile != null)
            {
                outSym.PDBFileName = pdbfile;
            }
            else
            {
                DebugUtil.Assert(outSym.PDBFileName == null);
            }

            Compiler.NameManager.AddString(filename);
            ++(Compiler.OutputFileCount);
            return outSym;
        }

        //------------------------------------------------------------
        // SYMMGR.CreateSeperateResourceFile
        //
        /// <summary>
        /// <para>Create a symbol representing a resource file.
        /// If bEmbed is true it becomes a child of the default output file,
        /// otherwise a new output file is created for itself</para>
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="outfileSym"></param>
        /// <param name="ident"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal RESFILESYM CreateSeperateResourceFile(
            FileInfo fileInfo,
            OUTFILESYM outfileSym,
            string ident,
            ResourceAttributes access)
        {
            DebugUtil.Assert(outfileSym != null);

            // Check for duplicates
            // if the resource file with same name and same parent is already registered,
            // return null.

            OUTFILESYM outFile = fileRootSym.FirstChildSym != null ?
                (fileRootSym.FirstChildSym as OUTFILESYM) :
                null;
            for (; outFile != null; outFile = outFile.NextOutputFile())
            {
                if (LookupGlobalSymCore(ident, outFile, SYMBMASK.RESFILESYM) != null)
                {
                    return null;
                }
            }

            // Create the input file symbol.

            RESFILESYM resfileSym = CreateGlobalSym(SYMKIND.RESFILESYM, ident, outfileSym) as RESFILESYM;
            resfileSym.Set(fileInfo, ident);
            resfileSym.Accessibility = access;

            Compiler.NameManager.AddString(ident);
            Compiler.NotifyHostOfBinaryFile(fileInfo.FullName);
            return resfileSym;
        }

        //------------------------------------------------------------
        // SYMMGR.CreateEmbeddedResourceFile
        //
        /// <summary>
        /// <para>Create a symbol representing a resource file.
        /// If bEmbed is true it becomes a child of the default output file,
        /// otherwise a new output file is created for itself</para>
        /// <para>Create a RESFILESYM instance, and set mdfileroot to its parent.</para>
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="Ident"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal RESFILESYM CreateEmbeddedResourceFile(
            FileInfo fileInfo,
            string Ident,
            ResourceAttributes access)
        {
            RESFILESYM resfile = CreateSeperateResourceFile(
                fileInfo,
                metadataFileRootSym,
                Ident,
                access);
            resfile.IsEmbedded = true;
            return resfile;
        }

        //------------------------------------------------------------
        // SYMMGR.CreateMetadataFile
        //
        /// <summary>
        /// <para>Create a symbol representing an imported metadata input file. 
        /// All imported metadata input files are children of the MDFileRoot</para>
        /// <para>Create an INFILESYM of the imported metadata file.
        /// if already exists, return it. Or create it and register to globalSymbolTable.</para>
        /// <para>Its parent is generally SYMMGR.MetadataFileRootSym.</para>
        /// </summary>
        /// <param name="assemblyID">if Kaid.Nil, infileSymSet assigns an assemblyID.</param>
        /// <param name="filename">The file name of an metadata file.</param>
        /// <param name="parent">The parent SYM of INFILESYM.</param>
        /// <returns>A created INFILESYM.</returns>
        //------------------------------------------------------------
        internal INFILESYM CreateMetadataFile(string file, int assemblyID, PARENTSYM parent)
        {
            // Kaid.Nil means allocate the next Assembly ID.
            DebugUtil.Assert(assemblyID == Kaid.Nil || assemblyID >= Kaid.ThisAssembly);
            DebugUtil.Assert(!String.IsNullOrEmpty(file));

            string searchName = file.ToLower();
            INFILESYM infileSym;

            Compiler.NameManager.AddString(searchName);

            //--------------------------------------------------
            // Search the INFILESYM with (name, parent) in globalSymbolTable.
            // If found, return it.
            //--------------------------------------------------
            SYM sym = LookupGlobalSymCore(searchName, parent, SYMBMASK.INFILESYM);
            infileSym = (sym != null ? (sym as INFILESYM) : null);
            if (infileSym != null &&
                infileSym.IsSource == false &&
                infileSym.IsModule == (assemblyID == Kaid.ThisAssembly))
            {
                // If we have a match then just return it.
                DebugUtil.Assert(assemblyID == Kaid.Nil || assemblyID == infileSym.GetAssemblyID());
                return infileSym;
            }

            //--------------------------------------------------
            // Create the input file symbol, and register to globalSymbolTable.
            //--------------------------------------------------
            infileSym = CreateGlobalSym(SYMKIND.INFILESYM, searchName, parent) as INFILESYM;
            infileSym.IsSource = false;

            if (assemblyID == Kaid.Nil)
            {
                assemblyID = AllocateAssemblyID(infileSym);
            }
            infileSym.SetAssemblyID(assemblyID);

            infileSym.LocalAssemblyID = Cor.mdTokenNil;
            if (assemblyID == Kaid.ThisAssembly)
            {
                infileSym.IsModule = true;
            }
            else
            {
                infileSym.IsAssembly = true;
            }
            DebugUtil.Assert(!infileSym.HasModuleCLSAttribute);

            // Report bug. (not do anythig now.)
            Compiler.NotifyHostOfMetadataFile(file);

            // Increment file counts
            ++(infileSym.GetOutFileSym().InputFileCount);
            ++(Compiler.InputFileCount);

            return infileSym;
        }

        //------------------------------------------------------------
        // SYMMGR.SetOutFileName
        //
        /// <summary>
        /// <para>Sets the filename of an output file to that
        /// of the given input file</para>
        /// <para>Create an output file name by a given input file name.</para>
        /// </summary>
        /// <param name="infileSym"></param>
        //------------------------------------------------------------
        internal void SetOutFileName(INFILESYM infileSym)
        {
            DebugUtil.Assert(infileSym != null);

            string fileName = infileSym.Name;
            string filePart = null;
            string extentionStr = null;

            OUTFILESYM outfileSym = infileSym.GetOutFileSym();
            if (outfileSym.HasFileInfo &&
                !String.IsNullOrEmpty(outfileSym.FileInfo.FullName))
            {
                return;
            }

            if (!outfileSym.IsDLL)
            {
                extentionStr = ".exe";
            }
            else if (outfileSym.IsManifest)
            {
                extentionStr = ".dll";
            }
            else
            {
                extentionStr = ".netmodule";
            }

            // point to the file part.
            //
            // PathFindFileNameW: Returns a pointer to the last component of a path string.
            // palrt\inc\rotor_palrt.h(1350), palrt\src\path.cpp (683)

            filePart = Path.GetFileNameWithoutExtension(fileName) + extentionStr;

            string canonicalPath = null;
            bool exists = false;
            Exception excp = null;

            canonicalPath = IOUtil.GetCanonFilePath(filePart, true, out exists, out excp);
            if (String.IsNullOrEmpty(canonicalPath))
            {
                Compiler.Error(CSCERRID.ERR_OutputFileNameTooLong, new ErrArg(filePart));
                infileSym.GetOutFileSym().SetName(null);  // filePart;
                this.compiler.NameManager.AddString(filePart);
                return;
            }

            FileInfo fi = null;
            if (IOUtil.CreateFileInfo(canonicalPath, out fi, out excp))
            {
                infileSym.GetOutFileSym().SetFileInfo(fi);
                this.compiler.NameManager.AddString(canonicalPath);
            }
        }

        //------------------------------------------------------------
        // SYMMGR.Compiler
        //------------------------------------------------------------
        //internal COMPILER Compiler()
        //{
        //    if (compiler == null) throw new LogicError("");
        //    return base.compiler;
        //}

        //------------------------------------------------------------
        // SYMMGR.AddOrphanedChild
        //------------------------------------------------------------
        //    // add a symbol into a symbol table
        //    void AddOrphanedChild(PPARENTSYM parent, PSYM child) {
        //        ASSERT(parent.AsAGGSYM.isFabricated && child.parent == NULL);
        //        AddChild( &tableGlobal, parent, child);
        //    }
        //
        //#ifdef DEBUG
        //    PINFILESYM GetPredefInfile();
        //
        //private:
        //    PINFILESYM  predefInputFile;        // dummy inputfile for testing purposes only
        //
        //#endif //DEBUG
    }

    //// This special value markets a bucket (in SYMTBL) as available but previously occupied.
    //// This is so we know to keep looking past this bucket.
    //#define ksymDead ((SYM *)1)

    ///*
    // * Static array of info about the predefined types.
    // */
    //#define PREDEFTYPEDEF(id, name, isRequired, isSimple, isNumeric, kind, ft, et, nicenm, zero, qspec, asize, st, attr, arity, inmscorlib) \
    //                     { L##name, isRequired, isSimple, isNumeric, qspec, ft, et, AggKind::kind, nicenm, { { (INT_PTR)(zero) } }, attr, asize, arity, inmscorlib },
    //
    //const static double doubleZero = 0;
    //const static __int64 longZero = 0;
    //#if BIGENDIAN
    //const static DECIMAL decimalZero = { { { 0 } }, 0 };
    //#else   // BIGENDIAN
    //const static DECIMAL decimalZero = { 0, { { 0 } } };
    //#endif  // BIGENDIAN
    //
    //const PredefTypeInfo predefTypeInfo[] =
    //{
    //    #include "predeftype.h"
    //};
    //
    //TypeArray BSYMMGR::taEmpty;

    ///*
    // * Returns the matching EXF_PARAM flags for a type.  This tells whether this is
    // * a ref or an out param...
    // */
    ////int __fastcall TYPESYM::getParamFlags() {
    ////    if (IsPARAMMODSYM) {
    ////        ASSERT(EXF_REFPARAM << 1 == EXF_OUTPARAM);
    ////        return (EXF_REFPARAM << (int)AsPARAMMODSYM.isOut);
    ////    } else {
    ////        return 0;
    ////    }
    ////}

    ///***************************************************************************************************
    //    Compare methof for sorting the array of PredefTypeNameInfo's by hash value.
    //***************************************************************************************************/
    //int __cdecl ComparePredefTypeNameInfo(const void * pv1, const void * pv2)
    //{
    //    const PredefTypeNameInfo * p1 = (const PredefTypeNameInfo *)pv1;
    //    const PredefTypeNameInfo * p2 = (const PredefTypeNameInfo *)pv2;
    //    return (p1.hash < p2.hash) ? -1 : p1.hash > p2.hash;
    //}

    ///*
    // * Handling "derived types".
    // *
    // * There are a set of types that are "derived" types, in that they
    // * are derived from standard types. Pointer types and array types are
    // * the obvious examples. We don't want to allocate new symbols for these
    // * every time we come across them. We handle these by using the parent
    // * lookup mechanism do the work for us, we use the "base" or "element" type of the
    // * derived type being the parent type, with a special weird name for looking
    // * up the derived type by.
    // */

    ///*
    // * Create a symbol representing an synthetized input file (used if #line has a source file name)
    // */
    //PINFILESYM SYMMGR::CreateSynthSourceFile(PCWSTR filename, OUTFILESYM *outfile)
    //{
    //    PNAME name;
    //    PINFILESYM infileSym;
    //
    //    name = host().getNamemgr().AddString(filename);
    //
    //    // Create the input file symbol.
    //    infileSym = CreateGlobalSym(SK_SYNTHINFILESYM, name, outfile).AsANYINFILESYM;
    //
    //    return infileSym;
    //}

    //
    ///*
    // * Print <indent" spaces to stdout.
    // */
    //static void PrintIndent(int indent)
    //{
    //    for (int i = 0; i < indent; ++i)
    //        wprintf(L" ");
    //}

    //PINFILESYM SYMMGR::GetPredefInfile()
    //{
    //    if (!predefInputFile) {
    //        // Create a bogus inputfile
    //        // we use the bogus assemly id 0
    //
    //        predefInputFile = CreateSourceFile(L"", mdfileroot);
    //        predefInputFile.rootDeclaration = CreateNamespaceDecl(
    //                    rootNS, 
    //                    NULL,                               // no declaration parent
    //                    predefInputFile, 
    //                    NULL);                              // no parse tree
    //    }
    //    return predefInputFile;
    //}

    //#endif //DEBUG

    // FindAts is moved to BSYMMGR
}
