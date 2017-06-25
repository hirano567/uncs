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
// File: assembly.h
//
// This is the real work horse
// ===========================================================================

//============================================================================
// Assembly.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Uncs
{
    static internal class AssemblyUtil
    {
        internal static byte[] MscorlibPublicKey = { 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0 };
        internal static byte[] MscorlibToken = { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };

        /// <summary>
        /// <para>= mdAssemblyNil = (mdAssembly)mdtAssembly = 0x20000000 (corhdr.h)</para>
        /// <para>(Defined in CSharp\ALink\Inc\ALink.cs)</para>
        /// </summary>
        internal const uint AssemblyIsUBM = ((uint)Cor.mdAssemblyNil);
    }

    //================================================================
    // CNameToTypeDictionary
    //
    /// <summary>
    /// <para>Maps (parentSym, name without arity) to Sytem.Type instance.</para>
    /// <para>(Defined in CSharp\ALink\Dll\Assembly.cs)</para>
    /// </summary>
    //================================================================
    internal class CNameToTypeDictionary
    {
        //============================================================
        // class CNameToTypeDictionary.Key
        //============================================================
        internal class Key
        {
            internal SYM parentSym = null;
            internal string typeName = null;

            //--------------------------------------------------------
            // CNameToTypeDictionary.Key Constructor
            //
            /// <summary></summary>
            /// <param name="parent"></param>
            /// <param name="name"></param>
            //--------------------------------------------------------
            internal Key(SYM parent, string name)
            {
                this.parentSym = parent;
                this.typeName = name;
            }

            //--------------------------------------------------------
            // CNameToTypeDictionary.Key.Equals
            //
            /// <summary></summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            //--------------------------------------------------------
            public override bool Equals(object obj)
            {
                Key other = obj as Key;
                if (other == null)
                {
                    return false;
                }
                if (this.typeName == null || other.typeName == null)
                {
                    return false;
                }
                return ((this.typeName == other.typeName) && (this.parentSym == other.parentSym));
            }

            //--------------------------------------------------------
            // CNameToTypeDictionary.Key.GetHashCode
            //
            /// <summary></summary>
            /// <returns></returns>
            //--------------------------------------------------------
            public override int GetHashCode()
            {
                int hash = (this.typeName != null ? this.typeName.GetHashCode() : 0);
                if (this.parentSym != null)
                {
                    hash ^= this.parentSym.GetHashCode();
                }
                hash &= 0x7FFFFFFF;
                return hash;
            }
        }

        //============================================================
        // class CNameToTypeDictionary.TypeInfo
        //============================================================
        internal class TypeInfo
        {
            internal Type Type = null;
            internal SYM Sym = null;
            internal TypeInfo Next = null;

            internal TypeInfo(Type type)
            {
                this.Type = type;
            }

            internal bool Imported
            {
                get { return (Sym != null); }
            }
        }

        //------------------------------------------------------------
        // CNameToTypeDictionary Fields and Properties
        //------------------------------------------------------------
        private Dictionary<Key, TypeInfo> typeDictionary = new Dictionary<Key, TypeInfo>();

        internal Dictionary<Key, TypeInfo> TypeDictionary
        {
            get { return this.typeDictionary; }
        }

        //------------------------------------------------------------
        // CNameToTypeDictionary.Clear
        //------------------------------------------------------------
        internal void Clear()
        {
            this.typeDictionary.Clear();
        }

        //------------------------------------------------------------
        // CNameToTypeDictionary.Add
        //
        /// <summary>
        /// <para>If (parent, name) exists, store type in list.</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        //------------------------------------------------------------
        internal void Add(SYM parent, string name, Type type)
        {
            if (String.IsNullOrEmpty(name))
            {
                return;
            }

            Key key = new Key(parent, name);
            TypeInfo info = null;

            try
            {
                if (!this.typeDictionary.TryGetValue(key, out info))
                {
                    this.typeDictionary.Add(key, new TypeInfo(type));
                    return;
                }
            }
            catch (ArgumentException)
            {
                DebugUtil.Assert(false);
                return;
            }

            DebugUtil.Assert(info != null);
            while (info.Next != null)
            {
                info = info.Next;
            }
            info.Next = new TypeInfo(type);
        }

        //------------------------------------------------------------
        // CNameToTypeDictionary.Find
        //------------------------------------------------------------
        internal CNameToTypeDictionary.TypeInfo Find(SYM parent, string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            Key key = new Key(parent, name);
            TypeInfo info=null;

            try
            {
                if (this.typeDictionary.TryGetValue(key, out info))
                {
                    return info;
                }
            }
            catch (ArgumentException)
            {
            }
            return null;
        }
    }

    //================================================================
    // class CMetadataFile
    //
    /// <summary>
    /// rename from CFile ins sscli.
    /// </summary>
    //================================================================
    abstract internal class CMetadataFile
    {
        // friend class CAssembly;
        // friend class Win32Res;

        //------------------------------------------------------------
        // CMetadataFile Fields and Properties (1) Controller
        //------------------------------------------------------------

        protected CController controller = null;

        internal CController Controller
        {
            get { return controller; }
        }

        //------------------------------------------------------------
        // CMetadataFile Fields and Properties (2) Names
        //------------------------------------------------------------

        protected string name = null; // LPCWSTR m_Name;

        virtual internal string Name
        {
            get { return this.name; }
        }

        protected FileInfo fileInfo = null;

        internal FileInfo FileInfo
        {
            get { return this.fileInfo; }
        }

        virtual internal string FileName
        {
            get { return (fileInfo != null ? fileInfo.Name : null); }
        }

        virtual internal string FileFullName
        {
            get { return (fileInfo != null ? fileInfo.FullName : null); }
        }

        virtual internal String AssemblyNameString
        {
            get { return null; }
        }

        virtual internal AssemblyName AssemblyNameObject
        {
            get { return null; }
        }

        //------------------------------------------------------------
        // CMetadataFile Fields and Properties (3) etc.
        //------------------------------------------------------------

        /// <summary>
        /// <para>True when m_isImport and Embedded resources/ComTypes
        /// have been declared in the assembly</para>
        /// </summary>
        protected bool isDeclared = false; // bool m_isDeclared : 1;

        protected bool noMetaData = false;  // bool m_bNoMetaData : 1;

        protected CAsmLink linker = null;   // CAsmLink* m_pLinker;

        // cached value of the AssemblyImport interface. 
        // Do not use this, instead call GetAssemblyImport()
        // CComPtr<IMetaDataAssemblyImport> m_pAImportCached; 

        /// <summary>
        /// <para> In sscli, CStructArray<CA> m_CAs;</para>
        /// </summary>
        protected List<Attribute> customAttributeList = new List<Attribute>();

        internal List<Attribute> CustomAttributeList
        {
            get { return this.customAttributeList; }
        }
        internal int CustomAttributeCount
        {
            get { return customAttributeList.Count; }
        }

        protected CNameToTypeDictionary nameToTypeDictionary = new CNameToTypeDictionary();

        internal CNameToTypeDictionary NameToTypeDictionary
        {
            get { return this.nameToTypeDictionary; }
        }

        //------------------------------------------------------------
        // CMetadataFile Constructor
        //
        /// <summary></summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal CMetadataFile(CController cntr)
        {
            DebugUtil.Assert(cntr != null);
            this.controller = cntr;
        }

        //------------------------------------------------------------
        // CMetadataFile.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        virtual internal void Clear()
        {
            this.name = null;
            this.fileInfo = null;
            this.isDeclared = false;
            this.linker = null;
            this.customAttributeList.Clear();
            this.nameToTypeDictionary.Clear();
        }

        //------------------------------------------------------------
        // CMetadataFile.SetName
        //
        /// <summary></summary>
        /// <param name="str"></param>
        //------------------------------------------------------------
        internal virtual void SetName(string str)
        {
            this.name = str;
        }

        //------------------------------------------------------------
        // CMetadataFile.SetFile
        //
        /// <summary>
        /// <para>If FileInfo instance is not null and its name is not null or empty,
        /// set this.isImport true (if exists or not).</para>
        /// <para>Return this.isImport.</para>
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetFile(FileInfo fi)
        {
            DebugUtil.Assert(fi != null && !String.IsNullOrEmpty(fi.Name));
            this.fileInfo = fi;
            return true;
        }

        //------------------------------------------------------------
        // CMetadataFile.InitBase
        //
        /// <summary>
        /// Set fields except for files.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        //------------------------------------------------------------
        internal void InitBase(CAsmLink linker)
        {
            this.linker = linker;
        }

        //------------------------------------------------------------
        // CMetadataFile.Init
        //
        /// <summary>
        /// Return true if the given FileInfo instance has a valid file name.
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <returns>Always return true.</returns>
        //------------------------------------------------------------
        virtual internal bool Init(FileInfo fi, CAsmLink linker)
        {
            InitBase(linker);
            SetFile(fi);
            return true;
        }

        //------------------------------------------------------------
        // CMetadataFile.GetModules
        //------------------------------------------------------------
        internal virtual Module[] GetModules()
        {
            return null;
        }

        //------------------------------------------------------------
        // CMetadataFile.GetCustomAttributes
        //
        /// <summary>
        /// Dummy method.
        /// This must be overriden by methods which return an array of System.Attribute as Object[].
        /// </summary>
        /// <param name="includeInherit">set true to include inherited custom attributes.</param>
        /// <returns>An array of System.Attribute or its derived.</returns>
        //------------------------------------------------------------
        virtual internal Object[] GetCustomAttributes(bool includeInherited)
        {
            return null;
        }

        //------------------------------------------------------------
        // CMetadataFile.ReadCustomAttributes (CFile::ReadCAs)
        //
        /// <summary>
        /// Store the custom attributes to argument attrList.
        /// </summary>
        /// <param name="attrList">To which custom attributes are stored.</param>
        /// <returns>If attrList is null, return false.
        /// If other error occurs, throw an exception.</returns>
        //------------------------------------------------------------
        virtual internal bool ReadCustomAttributes(
            List<Attribute> attrList,
            bool includeInherited)
        {
            if (attrList == null)
            {
                return false;
            }

            Object[] objs = GetCustomAttributes(includeInherited);
            if (objs == null || objs.Length == 0)
            {
                return true;
            }

            foreach (Object obj in objs)
            {
                Attribute attr = obj as Attribute;
                Object prev = null;
                if (attr == null)
                {
                    continue;
                }
                foreach (Attribute other in attrList)
                {
                    if (attr.Equals(other))
                    {
                        prev = other;
                        break;
                    }
                }
                if (prev == null)
                {
                    attrList.Add(attr);
                }
            }
            return true;
        }

        //------------------------------------------------------------
        // CMetadataFile.ReadCustomAttributes (CFile::ReadCAs)
        //
        /// <summary>
        /// Store the custom attributes to this.customAttributeList.
        /// </summary>
        /// <returns>If attrList is null, return false.
        /// If other error occurs, throw an exception.</returns>
        //------------------------------------------------------------
        virtual internal bool ReadCustomAttributes(bool includeInherited)
        {
            return this.ReadCustomAttributes(this.customAttributeList, includeInherited);
        }

        //------------------------------------------------------------
        // CMetadataFile.GetType
        //
        /// <summary>
        /// <para>Find and return a System.Type instance with a specified name.</para>
        /// <para>If not found, return null.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal Type GetType(string name)
        {
            return null;
        }

        //------------------------------------------------------------
        // CMetadataFile.GetAllTypes
        //
        /// <summary>
        /// Dummy method.
        /// This must be overriden by methods which return an array of System.Type.
        /// </summary>
        /// <returns>An array of System.Type.</returns>
        //------------------------------------------------------------
        virtual internal Type[] GetAllTypes()
        {
            return null;
        }
    }

    //======================================================================
    // class CModuleEx
    //
    /// <summary>
    /// Handle the module which is specified by /addmodule: option.
    /// </summary>
    //======================================================================
    internal class CModuleEx : CMetadataFile
    {
        //------------------------------------------------------------
        // CModule Fields and Properties
        //------------------------------------------------------------
        protected Module module = null;

        internal Module Module
        {
            get { return this.module; }
        }

        internal bool IsManifestModule = false;

        /// <summary>
        /// When executing applications,
        /// module files must be in the same directory with the application.
        /// So, we copy the specified module file to the output directory and use it.
        /// This FileInfo instance represents the original module file.
        /// </summary>
        internal FileInfo SourceFileInfo = null;

        //protected Module[] moduleArray = null; // conform to CAssembly, CAssemblyBuilder

        /// <summary>
        /// Store a Module file image.
        /// </summary>
        protected byte[] image = null;

        internal Byte[] Image
        {
            get { return this.image; }
        }

        /// <summary>
        /// Use CMetadataFile.Name and CMetadataFile.SetName().
        /// </summary>
        internal string ModuleName
        {
            get { return (this.module != null ? this.module.Name : null); }
        }

        internal string ModuleFullyQualifiedName
        {
            get { return (this.module != null ? this.module.FullyQualifiedName : null); }
        }

        internal string ModuleScopeName
        {
            get { return (this.module != null ? this.module.ScopeName : null); }
        }

        internal virtual Guid VersionID
        {
            get
            {
                if (this.module != null)
                {
                    return this.module.ModuleVersionId;
                }
                return Guid.Empty;
            }
        }

        protected CAssemblyEx assemblyEx = null;

        internal CAssemblyEx AssemblyEx
        {
            get { return this.assemblyEx; }
        }

        /// <summary>
        /// tha AGGSYMs of Type instance defined in this module.
        /// </summary>
        protected List<AGGSYM> aggSymList = new List<AGGSYM>();

        /// <summary>
        /// (R) tha AGGSYMs of Type instance defined in this module.
        /// </summary>
        internal List<AGGSYM> AggSymList
        {
            get { return this.aggSymList; }
        }

        //------------------------------------------------------------
        // CModuleEx Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CModuleEx(CController cntr) : base(cntr) { }

        //------------------------------------------------------------
        // CModuleEx.CreateModuleFileInfo
        //
        /// <summary></summary>
        /// <param name="srcFi"></param>
        /// <param name="dstFi"></param>
        //------------------------------------------------------------
        internal FileInfo CreateModuleFileInfo(FileInfo srcFi)
        {
            if (srcFi != null && !String.IsNullOrEmpty(srcFi.Name))
            {
                return null;
            }
            FileInfo dstFi = null;
            Exception excp = null;

            // We assume that the current directory is the output directory for now.
            // Must modify for output: option.

            if (!IOUtil.CreateFileInfo(srcFi.Name, out dstFi, out excp))
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                dstFi= null;
            }
            return dstFi;
        }

        //------------------------------------------------------------
        // CModuleEx.SetAssemblyEx
        //
        /// <summary></summary>
        /// <param name="asm"></param>
        //------------------------------------------------------------
        internal void SetAssemblyEx(CAssemblyEx asm)
        {
            this.assemblyEx = asm;
        }

        //------------------------------------------------------------
        // CModuleEx.LoadImage
        //
        /// <summary></summary>
        /// <param name="srcFi"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool LoadImage(FileInfo srcFi)
        {
            if (srcFi == null ||
                String.IsNullOrEmpty(srcFi.Name))
            {
                return false;
            }
            this.SourceFileInfo = srcFi;

            Exception excp = null;

            if (IOUtil.ReadBinaryFile(
                    this.SourceFileInfo.FullName,
                    out this.image,
                    out excp))
            {
                return true;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return false;
        }

        //------------------------------------------------------------
        // CModuleEx.SetImageToAssemblyBuilder
        //
        /// <summary></summary>
        /// <param name="asmBuilder"></param>
        /// <param name="loadImage"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetImageToAssemblyBuilder(
            CAssemblyBuilderEx asmBuilderEx,
            bool reloadImage)
        {
            Exception excp = null;

            if (asmBuilderEx == null)
            {
                return false;
            }
            AssemblyBuilder asmBuilder = asmBuilderEx.AssemblyBuilder;
            if (asmBuilder == null)
            {
                return false;
            }

            if (reloadImage == true || this.image == null)
            {
                if (!this.LoadImage(this.SourceFileInfo))
                {
                    return false;
                }
            }

            try
            {
                asmBuilder.DefineDynamicModule(
                    this.fileInfo.Name,
                    this.fileInfo.Name);
                this.module = asmBuilder.LoadModule(
                    this.fileInfo.Name,
                    this.image);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (BadImageFormatException ex)
            {
                excp = ex;
            }
            catch (FileLoadException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return false;
        }

        //------------------------------------------------------------
        // CModuleEx.GetCustomAttributes (override)
        //
        /// <summary>
        /// Return an array of System.Attribute as Object[].
        /// </summary>
        /// <param name="includeInherit">set true to include inherited custom attributes.</param>
        /// <returns>An array of System.Attribute or its derived.</returns>
        //------------------------------------------------------------
        internal override object[] GetCustomAttributes(bool includeInherited)
        {
            return (this.module != null ? module.GetCustomAttributes(includeInherited) : null);
        }

        //------------------------------------------------------------
        // CModuleEx.GetType (override)
        //
        /// <summary>
        /// <para>Find and return a System.Type instance with a specified name.</para>
        /// <para>If not found, return null.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override Type GetType(string name)
        {
            if (this.module != null)
            {
                try
                {
                    return this.module.GetType(name);
                }
                catch (ArgumentException)
                {
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // CModuleEx.GetAllTypes (override)
        //
        /// <summary>
        /// Return an array of System.Type defined in this metadata file.
        /// with the one which has the same token.
        /// </summary>
        /// <returns>An array of System.Type.</returns>
        //------------------------------------------------------------
        internal override Type[] GetAllTypes()
        {
            return (this.module != null ? module.GetTypes() : null);
        }

        //------------------------------------------------------------
        // CModuleEx.ReplaceReflectionInfo
        //
        /// <summary>
        /// <para>Replace System.Type, System.Reflection.MethodInfo, ...
        /// with the ones defined in this.module.</para>
        /// <para>This method does not replace the types
        /// of the parameters of the methods.</para>
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        internal void ReplaceReflectionInfo(SYM sym)
        {
            if (sym == null || this.module == null)
            {
                return;
            }
            INFILESYM infileSym = sym.GetInputFile();
            DebugUtil.Assert(infileSym != null);
            CModuleEx modEx = infileSym.ModuleEx;
            if (modEx == null ||
                modEx.Module == null ||
                this.Module != modEx.Module)
            {
                return;
            }

            int token = 0;

            //--------------------------------------------------------
            // Types
            //--------------------------------------------------------
            if (sym.IsAGGSYM)
            {
                AGGSYM aggSym = sym as AGGSYM;
                if (aggSym.Type == null)
                {
                    return;
                }
                Type newType = null;

                try
                {
                    token = aggSym.Type.MetadataToken;
                    newType = this.module.ResolveType(token);
                }
                catch (ArgumentException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                    return;
                }
                catch (BadImageFormatException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                    return;
                }

                DebugUtil.Assert(newType != null);
                aggSym.Type = newType;

                for (SYM curSym = aggSym.FirstChildSym; curSym != null; sym = curSym.NextSym)
                {
                    ReplaceReflectionInfo(curSym);
                }

                if (aggSym.TypeVariables != null &&
                    aggSym.TypeVariables.Count > 0)
                {
                    for (int i = 0; i < aggSym.TypeVariables.Count; ++i)
                    {
                        ReplaceReflectionInfo(aggSym.TypeVariables[i]);
                    }
                }
            }
            //--------------------------------------------------------
            // Methods or Constructors
            //--------------------------------------------------------
            else if (sym.IsMETHSYM)
            {
                METHSYM methSym = sym as METHSYM;
                MethodBase mBase = null;

                try
                {
                    token = methSym.MethodInfo.MetadataToken;
                    mBase = this.module.ResolveMethod(token);
                }
                catch (ArgumentException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                    return;
                }
                catch (BadImageFormatException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                    return;
                }

                if (methSym.MethodInfo != null)
                {
                    MethodInfo mInfo = mBase as MethodInfo;
                    DebugUtil.Assert(mInfo != null);
                    methSym.MethodInfo = mInfo;
                }
                else if (methSym.ConstructorInfo != null)
                {
                    ConstructorInfo cInfo = mBase as ConstructorInfo;
                    DebugUtil.Assert(cInfo != null);
                    methSym.ConstructorInfo = cInfo;
                    return;
                }
                else
                {
                    return;
                }

                if (methSym.TypeVariables != null &&
                    methSym.TypeVariables.Count > 0)
                {
                    for (int i = 0; i < methSym.TypeVariables.Count; ++i)
                    {
                        ReplaceReflectionInfo(methSym.TypeVariables[i]);
                    }
                }
            }
            //--------------------------------------------------------
            // Field
            //--------------------------------------------------------
            else if (sym.IsMEMBVARSYM)
            {
                MEMBVARSYM fieldSym = sym as MEMBVARSYM;
                if (fieldSym == null || fieldSym.FieldInfo == null)
                {
                    return;
                }

                try
                {
                    token = fieldSym.FieldInfo.MetadataToken;
                    fieldSym.FieldInfo = this.module.ResolveField(token);
                }
                catch (ArgumentException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                catch (BadImageFormatException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                return;
            }
            //--------------------------------------------------------
            // Property
            //--------------------------------------------------------
            else if (sym.IsPROPSYM)
            {
                PROPSYM propSym = sym as PROPSYM;
                if (propSym == null || propSym.PropertyInfo == null)
                {
                    return;
                }

                try
                {
                    token = propSym.PropertyInfo.MetadataToken;
                    propSym.PropertyInfo
                        = this.Module.ResolveMember(token) as PropertyInfo;
                }
                catch (ArgumentException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                catch (BadImageFormatException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                return;
            }
            //--------------------------------------------------------
            // Event
            //--------------------------------------------------------
            else if (sym.IsEVENTSYM)
            {
                EVENTSYM eventSym = sym as EVENTSYM;
                if (eventSym == null || eventSym.EventInfo == null)
                {
                    return;
                }

                try
                {
                    token = eventSym.EventInfo.MetadataToken;
                    eventSym.EventInfo
                        = this.module.ResolveMember(token) as EventInfo;
                }
                catch (ArgumentException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                catch (BadImageFormatException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                return;
            }
            //--------------------------------------------------------
            // Generic Type Variables
            //--------------------------------------------------------
            else if (sym.IsTYVARSYM)
            {
                TYVARSYM tvSym = sym as TYVARSYM;
                if (tvSym == null || tvSym.Type == null)
                {
                    return;
                }

                try
                {
                    token = tvSym.Type.MetadataToken;
                    tvSym.SetSystemType(this.Module.ResolveType(token), true);
                }
                catch (ArgumentException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                catch (BadImageFormatException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                }
                return;
            }
        }

        //------------------------------------------------------------
        // CModuleEx.SaveImage
        //
        /// <summary>
        /// Save the image of this module into the current directory.
        /// </summary>
        //------------------------------------------------------------
        internal void SaveImage()
        {
            if (this.fileInfo == null ||
                String.IsNullOrEmpty(this.fileInfo.Name) ||
                this.image == null)
            {
                return;
            }

            Exception excp=null;
            if (!IOUtil.WriteBinaryFile(
                this.fileInfo.FullName,
                this.image,
                out excp))
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
            }
        }
    }

    //======================================================================
    // class CAssemblyEx
    //
    /// <summary></summary>
    //======================================================================
    internal class CAssemblyEx : CMetadataFile
    {
        //------------------------------------------------------------
        // enum CAssemblyEx.LoadMethod
        //------------------------------------------------------------
        internal enum LoadMethod
        {
            LoadFrom = 0,
            LoadFile,
            ReflectionOnlyLoadFrom,
        }

        //------------------------------------------------------------
        // CAssemblyEx Fields and Properties (1) Assembly
        //------------------------------------------------------------

        protected Assembly assembly = null;

        protected AssemblyName assemblyName = null;

        protected LoadMethod loadMethod = LoadMethod.LoadFrom;

        internal Assembly Assembly
        {
            get { return this.assembly; }
        }

        //------------------------------------------------------------
        // CAssemblyEx Fields and Properties (2) Names
        // we do not use CMetadataFile.Name.
        //------------------------------------------------------------

        internal override string Name
        {
            get { return (this.assemblyName != null ? this.assemblyName.Name : null); }
        }

        internal override string AssemblyNameString
        {
            get { return this.assemblyName.Name; }
        }

        internal string AssemblyFullName
        {
            get { return this.assemblyName.FullName; }
        }

        internal override AssemblyName AssemblyNameObject
        {
            get { return this.assemblyName; }
        }

        internal string AssemblyLocation
        {
            get { return (this.assembly != null ? this.assembly.Location : null); }
        }

        //------------------------------------------------------------
        // CAssemblyEx Fields and Properties (3) etc.
        //------------------------------------------------------------

        internal Version AssemblyVersion
        {
            get { return (this.assemblyName != null ? this.assemblyName.Version : null); }
        }

        internal virtual Guid ManifestModuleVersionID
        {
            get
            {
                if (this.assembly != null)
                {
                    return (this.assembly.GetModules())[0].ModuleVersionId;
                }
                return Guid.Empty;
            }
        }

        // From CFile of sscli

        /// <summary>
        /// True if this assembly is mscorlib.dll
        /// </summary>
        internal bool IsStdLib = false; // bool m_isStdLib : 1; (to CAssembly)

        /// <summary>
        /// True if this assembly is system.dll
        /// </summary>
        /// <remarks>(2015/01/14 hirano567@hotmail.co.jp)</remarks>
        internal bool IsSystemDll = false;

        //------------------------------------------------------------
        // CAssemblyEx.Equals (override)
        //
        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public override bool Equals(object obj)
        {
            CAssemblyEx otherAsm = obj as CAssemblyEx;
            if (otherAsm != null)
            {
                return this.assembly.Equals(otherAsm.assembly);
            }
            return false;
        }

        //------------------------------------------------------------
        // CAssemblyEx.GetHashCode (override)
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override int GetHashCode()
        {
            if (this.assembly != null)
            {
                return this.assembly.GetHashCode();
            }
            return 0;
        }

        //------------------------------------------------------------
        // CAssembly Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CAssemblyEx(CController cntr) : base(cntr) { }

        //------------------------------------------------------------
        // CAssemblyEx.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal override void Clear()
        {
            base.Clear();
            this.assembly = null;
            this.assemblyName = null;
            this.IsStdLib = false;
            this.IsSystemDll = false;
        }

        //------------------------------------------------------------
        // CAssemblyEx.SetIsStdLib
        //
        /// <summary>
        /// If the file name of metadata is "mscorlib.dll", set this.isStdLib true;
        /// </summary>
        //------------------------------------------------------------
        protected void SetIsStdLib()
        {
            this.IsStdLib = (String.Compare(fileInfo.Name, "mscorlib.dll", true) == 0);
        }

        //------------------------------------------------------------
        // CAssemblyEx.LoadAssembly
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Assembly LoadAssembly(string fileName)
        {
            Exception excp;
            Assembly asm = CAssemblyEx.LoadFrom(this.loadMethod, fileName, out excp);
            if (asm != null)
            {
                return asm;
            }
            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CAssemblyEx.SetAssembly
        //
        /// <summary></summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetAssembly(Assembly asm)
        {
            if (asm == null)
            {
                return false;
            }
            Exception excp;
            this.assembly = asm;
            this.assemblyName = CAssemblyEx.GetAssemblyName(this.assembly, out excp);
            if (this.assemblyName == null)
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                this.assembly = null;
                this.isDeclared = false;
                return false;
            }

            // If this assmbly is "mscorlib.dll", set isStdLib field true.
            if (String.Compare(this.Name, "mscorlib", true) == 0 ||
                String.Compare(this.Name, "mscorlib.dll", true) == 0)
            {
                this.IsStdLib = true;
                this.IsSystemDll = false;
            }
            else if (String.Compare(this.Name, "system", true) == 0 ||
                String.Compare(this.Name, "system.dll", true) == 0)
            {
                this.IsStdLib = false;
                this.IsSystemDll = true;
            }
            else
            {
                this.IsStdLib = false;
                this.IsSystemDll = false;
            }

            //string extension = Path.GetExtension(this.assembly.Location);
            //if (!String.IsNullOrEmpty(extension))
            //{
            //    this.isExe = (String.Compare(extension, ".exe", true) == 0);
            //}
            this.isDeclared = true;
            return true;
        }

        //------------------------------------------------------------
        // CAssemblyEx.Init (1)
        //
        /// <summary></summary>
        /// <param name="asm"></param>
        /// <param name="fi"></param>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <param name="meth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Init(
            Assembly asm,
            FileInfo fi,
            CAsmLink linker,
            LoadMethod meth)
        {
            DebugUtil.Assert(asm != null);
            this.loadMethod = meth;

            base.Init(fi, linker);
            if (SetAssembly(asm))
            {
                return true;
            }
            this.Clear();
            return false;
        }

        //------------------------------------------------------------
        // CAssemblyEx.Init (2-1)
        //
        /// <summary></summary>
        /// <param name="fi"></param>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override internal bool Init(
            FileInfo fi,
            CAsmLink linker)
        {
            base.Init(fi, linker);

            Assembly asm = this.LoadAssembly(fi.FullName);
            if (asm != null)
            {
                if (SetAssembly(asm))
                {
                    return true;
                }
            }
            this.Clear();
            return false;
        }

        //------------------------------------------------------------
        // CAssemblyEx.Init (2-2)
        //
        /// <summary></summary>
        /// <param name="fi"></param>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <param name="meth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Init(
            FileInfo fi,
            CAsmLink linker,
            LoadMethod meth)
        {
            this.loadMethod = meth;
            return Init(fi, linker);
        }

        //------------------------------------------------------------
        // CAssemblyEx.GetCustomAttributes (override)
        //
        /// <summary>
        /// Return an array of System.Attribute as Object[].
        /// </summary>
        /// <param name="includeInherit">set true to include inherited custom attributes.</param>
        /// <returns>An array of System.Attribute or its derived.</returns>
        //------------------------------------------------------------
        internal override object[] GetCustomAttributes(bool includeInherited)
        {
            return (this.assembly != null ? this.assembly.GetCustomAttributes(includeInherited) : null);
        }

        //------------------------------------------------------------
        // CAssemblyEx.GetType (override)
        //
        /// <summary><summary>
        /// <para>Find and return a System.Type instance with a specified name.</para>
        /// <para>If not found, return null.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override Type GetType(string name)
        {
            if (this.assembly != null)
            {
                try
                {
                    return this.assembly.GetType(name);
                }
                catch (ArgumentException)
                {
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // CAssemblyEx.GetAllTypes (override)
        //
        /// <summary>
        /// Return an array of System.Type defined in this metadata file.
        /// </summary>
        /// <returns>An array of System.Type.</returns>
        //------------------------------------------------------------
        internal override Type[] GetAllTypes()
        {
            return (this.assembly != null ? this.assembly.GetTypes() : null);
        }

        //------------------------------------------------------------
        // CAssemblyEx.ImportAssembly
        //
        /// <summary>
        /// <para>Import the custom attributes of this assembly.</para>
        /// </summary>
        /// <param name="importTypes"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool ImportAssembly(bool importTypes)
        {
            if (this.assembly == null)
            {
                return false;
            }

            this.ReadCustomAttributes(true);
            //this.AddTypes(true);
            this.isDeclared = true;
            return true;
        }
        //------------------------------------------------------------
        // CAssembly::ImportAssembly (sscli)
        //------------------------------------------------------------
        //HRESULT CAssembly::ImportAssembly(
        //    DWORD *pdwCountOfScopes,
        //    BOOL fImportTypes,
        //    DWORD dwOpenFlags,
        //    IMetaDataDispenserEx *pDisp)
        //{
        //    ASSERT(m_pAImport != NULL);
        //
        //    HRESULT hr = S_OK;
        //    DWORD chName = 0;
        //    void *pbOrig = NULL;
        //    DWORD cbOrig = 0;
        //    DWORD dwFlags = 0;
        //
        //    mdAssembly tkAsm;
        //    if (FAILED(hr = m_pAImport->GetAssemblyFromScope( &tkAsm)))
        //    {
        //        // Don't ReportError because this is a COM+ error!
        //        return hr;
        //    }
        //
        //    if (FAILED(hr = m_pAImport->GetAssemblyProps(tkAsm, NULL, NULL, NULL, NULL, 0, &chName, &m_adata, &dwFlags)))
        //    {
        //        // Don't ReportError because this is a COM+ error!
        //        return hr;
        //    }
        //
        //    m_dwFlags |= (dwFlags & ~afPublicKey);
        //    if ((m_szAsmName = new WCHAR[chName]) == NULL)
        //    {
        //        return ReportError(E_OUTOFMEMORY);
        //    }
        //
        //    if (m_adata.cbLocale)
        //    {
        //        if (NULL == (m_adata.szLocale = new WCHAR[m_adata.cbLocale]))
        //            return ReportError(E_OUTOFMEMORY);
        //    }
        //    else
        //    {
        //        m_adata.szLocale = NULL;
        //    }
        //
        //    if (m_adata.ulOS)
        //    {
        //        if (NULL == (m_adata.rOS = new OSINFO[m_adata.ulOS]))
        //            return ReportError(E_OUTOFMEMORY);
        //    }
        //    else
        //    {
        //        m_adata.ulOS = NULL;
        //    }
        //
        //    if (m_adata.ulProcessor)
        //    {
        //        if (NULL == (m_adata.rProcessor = new ULONG[m_adata.ulProcessor]))
        //            return ReportError(E_OUTOFMEMORY);
        //    }
        //    else
        //    {
        //        m_adata.rProcessor = NULL;
        //    }
        //
        //    if (FAILED(hr = m_pAImport->GetAssemblyProps(
        //        tkAsm, (const void**)&pbOrig, &cbOrig, NULL, (LPWSTR)m_szAsmName, chName, NULL, &m_adata, NULL)))
        //    {
        //        // Don't ReportError because this is a COM+ error!
        //        return hr;
        //    }
        //
        //    if (cbOrig > 0)
        //    {
        //        if (!StrongNameTokenFromPublicKey( (PBYTE)pbOrig, cbOrig, (PBYTE*)&pbOrig, &cbOrig))
        //            return ReportError(StrongNameErrorInfo());
        //    }
        //
        //    if ((m_pPubKey = (PublicKeyBlob*)new BYTE[cbOrig]) == NULL)
        //    {
        //        return ReportError(E_OUTOFMEMORY);
        //    }
        //    memcpy(m_pPubKey, pbOrig, cbOrig);
        //    m_cbPubKey = cbOrig;
        //    if (cbOrig > 0)
        //    {
        //        StrongNameFreeBuffer( (BYTE*)pbOrig);
        //    }
        //
        //    // Get the CAs
        //    // Get the custom attributes and add to this.m_CAs which is type of CStructArray<CA>;.
        //    if (FAILED(hr = ReadCAs(tkAsm, NULL, FALSE, FALSE)))
        //    {
        //    	// We don't know and we don't care for the import case.
        //        return hr;
        //    }
        //
        //    // The manifest file always imports it's types implicitly
        //    if (fImportTypes)
        //    {
        //        if (FAILED(hr = ImportFile(NULL, NULL)))
        //            return hr;
        //    }
        //
        //    HCORENUM enumFiles;
        //    mdFile filedefs[32];
        //    ULONG cFiledefs, iFiledef;
        //    WCHAR *FileName = NULL, *filepart = NULL;
        //    DWORD len, cchName;
        //
        //    cchName = (DWORD)wcslen(m_Path) + MAX_PATH;
        //    FileName = (LPWSTR)_alloca(sizeof(WCHAR) * cchName);
        //    hr = StringCchCopyW (FileName, cchName, m_Path);
        //    if (FAILED (hr)) {
        //        return hr;
        //    }
        //    filepart = wcsrchr(FileName, L'\\');
        //    if (filepart)
        //    {
        //        filepart++;
        //    }
        //    else
        //    {
        //        filepart = FileName;
        //    }
        //
        //    len = cchName - (DWORD)(filepart - FileName);
        //
        //    ASSERT(m_Files.Count() == 0);
        //
        //    // Enumeration all the Files in this assembly.
        //    enumFiles= 0;
        //    do
        //    {
        //        // Get next batch of files.
        //        hr = m_pAImport->EnumFiles(&enumFiles, filedefs, lengthof(filedefs), &cFiledefs);
        //        if (FAILED(hr))
        //        {
        //            // Don't ReportError because this is a COM+ error!
        //            break;
        //        }
        //
        //        // Process each file.
        //        for (iFiledef = 0; iFiledef < cFiledefs && SUCCEEDED(hr); ++iFiledef)
        //        {
        //            CFile *file = NULL;
        //            hr = m_pAImport->GetFileProps( filedefs[iFiledef], filepart, len, &cchName, NULL, NULL, &dwFlags);
        //            if (FAILED(hr))
        //            {
        //                // Don't ReportError because this is a COM+ error!
        //                break;
        //            }
        //
        //            if (IsFfContainsMetaData(dwFlags))
        //            {
        //                IMetaDataImport* pImport = NULL;
        //                hr = pDisp->OpenScope( FileName, dwOpenFlags, IID_IMetaDataImport, (IUnknown**)&pImport);
        //                if (SUCCEEDED(hr))
        //                {
        //                    if (NULL == (file = new CFile()))
        //                    {
        //                        hr = ReportError(E_OUTOFMEMORY);
        //                    }
        //                    else if (FAILED(hr = file->Init( FileName, pImport, m_pError, m_pLinker)))
        //                    {
        //                        hr = ReportError(hr);
        //                    }
        //                    else
        //                    {
        //                        pImport->Release();
        //                        if (FAILED(hr = m_Files.Add(file)))
        //                            hr = ReportError(hr);
        //                        if (fImportTypes)
        //                            hr = file->ImportFile( NULL, NULL);
        //                    }
        //                }
        //                else
        //                {
        //                    hr = ReportError(
        //                        ERR_AssemblyModuleImportError,
        //                        m_tkError,
        //                        NULL,
        //                        m_Path,
        //                        filepart,
        //                        ErrorHR(hr, pDisp, IID_IMetaDataDispenserEx));
        //                }
        //            }
        //            else
        //            {
        //                if (NULL == (file = new CFile()))
        //                {
        //                    hr = ReportError(E_OUTOFMEMORY);
        //                }
        //                else if (FAILED(hr = file->Init( FileName, (IMetaDataEmit*)NULL, m_pError, m_pLinker)))
        //                {
        //                    hr = ReportError(hr);
        //                }
        //                else
        //                {
        //                    file->m_bNoMetaData = file->m_isDeclared = file->m_isCTDeclared = true;
        //                    if (FAILED(hr = m_Files.Add(file)))
        //                        hr = ReportError(hr);
        //                }
        //            }
        //            if (FAILED(hr) && file)
        //            {
        //                delete file;
        //                file = NULL;
        //            }
        //            else if (file)
        //            {
        //                file->m_tkError = file->m_tkFile = filedefs[iFiledef];
        //                // We need to keep these guys sorted
        //                ASSERT(m_Files.Count() == 1 || m_Files[m_Files.Count() - 2]->m_tkFile < filedefs[iFiledef]);
        //            }
        //        }
        //    } while (cFiledefs > 0 && SUCCEEDED(hr));
        //
        //    m_pImport->CloseEnum(enumFiles);
        //    if (pdwCountOfScopes)
        //    {
        //        *pdwCountOfScopes = (DWORD)m_Files.Count() + 1;
        //    }
        //
        //#ifdef _DEBUG
        //    // We use bsearch on the files, so they better stay in token order
        //    for (DWORD debug_only_i = 1; debug_only_i < m_Files.Count(); debug_only_i++)
        //    {
        //        ASSERT(m_Files[debug_only_i-1]->m_tkFile < m_Files[debug_only_i]->m_tkFile);
        //    }
        //#endif
        //
        //    return hr;
        //}

        //------------------------------------------------------------
        // CAssemblyEx.LoadFrom (static)
        //
        /// <summary></summary>
        /// <param name="meth"></param>
        /// <param name="fullPath"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal Assembly LoadFrom(
            LoadMethod meth,
            string fullPath,
            out Exception excp)
        {
            excp = null;
            try
            {
                switch (meth)
                {
                    case LoadMethod.LoadFrom:
                        return Assembly.LoadFrom(fullPath);

                    case LoadMethod.LoadFile:
                        return Assembly.LoadFile(fullPath);

                    case LoadMethod.ReflectionOnlyLoadFrom:
                        return Assembly.ReflectionOnlyLoadFrom(fullPath);

                    default:
                        return null;
                }
            }
            catch (ArgumentNullException e)
            {
                excp = e;
                return null;
            }
            catch (ArgumentException e)
            {
                excp = e;
                return null;
            }
            catch (FileNotFoundException e)
            {
                excp = e;
                return null;
            }
            catch (BadImageFormatException e)
            {
                excp = e;
                return null;
            }
            catch (System.Security.SecurityException e)
            {
                excp = e;
                return null;
            }
        }

        //------------------------------------------------------------
        // CAssemblyEx.GetAssemblyName (static)
        //
        /// <summary></summary>
        /// <param name="asm"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal AssemblyName GetAssemblyName(Assembly asm, out Exception excp)
        {
            excp = null;

            try
            {
                return AssemblyName.GetAssemblyName(asm.Location);
            }
            catch (ArgumentNullException e)
            {
                excp = e;
                return null;
            }
            catch (ArgumentException e)
            {
                excp = e;
                return null;
            }
            catch (FileNotFoundException e)
            {
                excp = e;
                return null;
            }
            catch (BadImageFormatException e)
            {
                excp = e;
                return null;
            }
            catch (FileLoadException e)
            {
                excp = e;
                return null;
            }
            catch (System.Security.SecurityException e)
            {
                excp = e;
                return null;
            }
        }

        //------------------------------------------------------------
        // CAssemblyEx.CreateInstance (static)
        //
        /// <summary></summary>
        /// <param name="fullPath"></param>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal CAssemblyEx CreateInstance(
            CController cntr,
            FileInfo fInfo,
            CAsmLink linker)
        {
            CAssemblyEx casm = new CAssemblyEx(cntr);
            if (casm.Init(fInfo, linker))
            {
                return casm;
            }
            return null;
        }
    }

    //======================================================================
    // class CMetadataFileSet
    //
    /// <summary></summary>
    /// <typeparam name="MD"></typeparam>
    //======================================================================
    internal class CMetadataFileSet<MD> where MD : CMetadataFile
    {
        //------------------------------------------------------------
        // CMetadataFileSet Fields and Properties
        //------------------------------------------------------------
        protected Dictionary<string, MD> metadataDictionary = new Dictionary<string, MD>();

        //------------------------------------------------------------
        // CMetadataFileSet.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        void Clear()
        {
            metadataDictionary.Clear();
        }

        //------------------------------------------------------------
        // CMetadataFileSet.Add
        //
        /// <summary></summary>
        /// <param name="mdfile"></param>
        //------------------------------------------------------------
        virtual internal void Add(MD mdfile)
        {
            if (mdfile == null ||
                String.IsNullOrEmpty(mdfile.Name))
            {
                return;
            }

            try
            {
                if (!metadataDictionary.ContainsKey(mdfile.Name))
                {
                    metadataDictionary.Add(mdfile.FileName, mdfile);
                }
            }
            catch (ArgumentException)
            {
            }
        }

        //------------------------------------------------------------
        // CMetadataFileSet.Get
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal MD Get(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            MD data = null;
            try
            {
                if (metadataDictionary.TryGetValue(name, out data))
                {
                    return data;
                }
            }
            catch (ArgumentException)
            {
            }
            return null;
        }
    }

    //======================================================================
    // class CAssemblySet
    //======================================================================
    internal class CAssemblySet : CMetadataFileSet<CAssemblyEx>
    {
        //------------------------------------------------------------
        // CAssemblySet Fields and Properties
        //------------------------------------------------------------

        //------------------------------------------------------------
        // CAssemblySet.Add (static) use CMetadataFileSet.Add
        //------------------------------------------------------------

        //------------------------------------------------------------
        // CAssemblyEx.Get (static) use CMetadataFileSet.Get
        //------------------------------------------------------------

        //------------------------------------------------------------
        // CAssemblyEx.CreateInstance (static)
        //
        /// <summary>
        /// Find or create a CAssembly instance from a Assembly instance.
        /// </summary>
        /// <param name="cntr"></param>
        /// <param name="asm"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CAssemblyEx CreateInstance(CController cntr, Assembly asm)
        {
            if (asm == null)
            {
                return null;
            }
            CAssemblyEx casm = Get(asm.FullName);
            if (casm != null)
            {
                return casm;
            }
            casm = new CAssemblyEx(cntr);
            casm.SetAssembly(asm);
            Add(casm);
            return casm;
        }

        //------------------------------------------------------------
        // CAssemblyEx.CreateInstance (static)
        //
        /// <summary>
        /// Find or create a CAssembly instance from a file name.
        /// </summary>
        /// <param name="cntr"></param>
        /// <param name="fileName"></param>
        /// <param name="meth"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CAssemblyEx CreateInstance(
            CController cntr,
            string fileName,
            CAssemblyEx.LoadMethod meth)
        {
            Exception excp;
            Assembly asm = CAssemblyEx.LoadFrom(meth, fileName, out excp);
            if (asm == null)
            {
                return null;
            }
            return CreateInstance(cntr, asm);
        }
    }
}
