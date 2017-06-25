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
// File: inputset.h
//
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
// File: inputset.cpp
//
// ===========================================================================

//============================================================================
// InputSet.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // class CSourceFileInfo
    //
    /// <summary>
    /// Has a FileInfo instance of a source file and some members.
    /// </summary>
    //======================================================================
    internal class CSourceFileInfo
    {
        //------------------------------------------------------------
        // CSourceFileInfo Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// A FileInfo instance of a source file.
        /// </summary>
        protected FileInfo fileInfo = null;

        /// <summary>
        /// (R) A FileInfo instance of a source file.
        /// </summary>
        internal FileInfo FileInfo
        {
            get { return fileInfo; }
        }

        /// <summary>
        /// (R) File Name of a source file, not a full path.
        /// </summary>
        internal string FileName
        {
            get { return (this.fileInfo != null ? this.fileInfo.Name : null); }
        }

        /// <summary>
        /// (R) Full path of a source file.
        /// </summary>
        internal string FullName
        {
            get { return (this.fileInfo != null ? this.fileInfo.FullName : null); }
        }

        internal DateTime LastWriteTime
        {
            get
            {
                return (this.fileInfo != null ? this.fileInfo.LastWriteTime : DateTime.MinValue);
            }
        }

        protected string searchName = null;

        internal string SearchName
        {
            get { return this.searchName; }
        }

        //------------------------------------------------------------
        // CSourceFileInfo Constructor
        //
        /// <summary></summary>
        /// <param name="info"></param>
        //------------------------------------------------------------
        internal CSourceFileInfo(FileInfo info)
        {
            DebugUtil.Assert(
                info != null &&
                !String.IsNullOrEmpty(info.Name));

            this.fileInfo = info;
            this.searchName = info.FullName.ToLower();
        }

#if DEBUG
        //------------------------------------------------------------
        // CSourceFileInfo.Debug
        //------------------------------------------------------------
        internal void Debug(StringBuilder sb, string indent)
        {
            sb.AppendFormat("{0}{1}\n", indent, this.FileName);
        }
#endif
    }

    //======================================================================
    // class CResourceFileInfo
    //
    /// <summary>
    /// Has a FileInfo instance of a resource file and some members.
    /// </summary>
    //======================================================================
    internal class CResourceFileInfo
    {
        //------------------------------------------------------------
        // CResourceFileInfo Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// A FileInfo instance of resource file.
        /// </summary>
        protected FileInfo fileInfo = null;

        /// <summary>
        /// (R) A FileInfo instance of resource file.
        /// </summary>
        internal FileInfo FileInfo
        {
            get { return this.fileInfo; }
        }

        /// <summary>
        /// Name of a resource file, not a full name.
        /// </summary>
        internal string FileName
        {
            get { return (this.fileInfo != null ? this.fileInfo.Name : null); }
        }

        /// <summary>
        /// FullName of a resource file.
        /// </summary>
        internal string FullName
        {
            get { return (this.fileInfo != null ? this.fileInfo.FullName : null); }
        }

        protected string searchName = null;

        internal string SearchName
        {
            get { return this.searchName; }
        }

        /// <summary>
        /// Name of Identity.
        /// </summary>
        internal string LogicalName = null;

        /// <summary>
        /// true if resource is embedded
        /// </summary>
        internal bool Embed;

        /// <summary>
        /// true if resource is visible (public)
        /// </summary>
        /// <remarks>(sscli)
        /// BOOL m_fVisible; // true if resource is visible (public)
        /// </remarks>
        internal ResourceAttributes Access = ResourceAttributes.Public;

        //------------------------------------------------------------
        // CResourceFileInfo Constructor
        //
        /// <summary></summary>
        /// <param name="info"></param>
        /// <param name="id"></param>
        /// <param name="embed"></param>
        /// <param name="vis"></param>
        //------------------------------------------------------------
        internal CResourceFileInfo(
            FileInfo info,
            string logname,
            bool embed,
            bool pub)
        {
            this.fileInfo = info;
            this.LogicalName = logname;
            this.Embed = embed;
            this.Access = (pub ? ResourceAttributes.Public : ResourceAttributes.Private);

            this.searchName = fileInfo.FullName.ToLower();
        }

#if DEBUG
        //------------------------------------------------------------
        // CResourceFileInfo.Debug
        //------------------------------------------------------------
        internal void Debug(StringBuilder sb, string indent)
        {
            sb.AppendFormat("{0}{1}, ID:{2}, Embed:{3}, Public:{4}\n",
                indent,
                this.FileName,
                this.LogicalName,
                this.Embed,
                this.Access);
        }
#endif
    }

    //======================================================================
    // CInputSet
    //
    /// <summary>
    /// <para>Represents an input set for adding sources/resources to compile into a single output file</para>
    /// <para>Has an output file, some input files and some resource files which are compiled to the output file,
    /// and configurations.</para>
    /// </summary>
    //======================================================================
    internal class CInputSet
    {
        //------------------------------------------------------------
        // CInputSet Fields and Properties (1) Controller
        //------------------------------------------------------------
        /// <summary>
        /// Owning compiler controller
        /// </summary>
        private CController controller = null;  // compilerController = null;

        /// <summary>
        /// (R) Owning compiler controller
        /// </summary>
        virtual internal CController Controller
        {
            get { return controller; }
        }

        //------------------------------------------------------------
        // CInputSet Fields and Properties (2) Target
        //------------------------------------------------------------
        internal TargetType target = TargetType.None;

        /// <summary>
        /// (R) Target
        /// </summary>
        internal TargetType Target
        {
            get { return this.target; }
        }

        /// <summary>
        /// <para>(R) True if the target is Exe or WinExe.</para>
        /// <para>Cannot treat AppContainerExe yet.</para>
        /// </summary>
        internal bool IsExe
        {
            get
            {
                return (
                    this.Target == TargetType.Exe ||
                    this.Target == TargetType.WinExe);
            }
        }

        /// <summary>
        /// (R) true if creating a DLL
        /// </summary>
        internal bool IsDLL
        {
            get { return (this.Target == TargetType.Library); }
        }

        /// <summary>
        /// (R) true if creating nothing
        /// </summary>
        internal bool NoOutput
        {
            get { return (this.Target == TargetType.None); }
        }

        /// <summary>
        /// (R) true if creating a Windows exe
        /// </summary>
        internal bool IsWinApp
        {
            get { return (this.Target == TargetType.WinExe); }
        }

        /// <summary>
        /// true if creating an assembly
        /// </summary>
        //private bool createAssembly = true;

        /// <summary>
        /// <para>(R) True if the target is Exe, Library or WinExe.</para>
        /// <para>Cannot treat AppContainerExe yet.</para>
        /// </summary>
        internal bool IsAssembly
        {
            get
            {
                return (
                    this.Target == TargetType.Exe ||
                    this.Target == TargetType.Library ||
                    this.Target == TargetType.WinExe);
            }
        }

        //------------------------------------------------------------
        // CInputSet Fields and Properties (3) Output File
        //------------------------------------------------------------
        /// <summary>
        /// A FileInfo instance of a output file.
        /// </summary>
        private FileInfo outputFileInfo = null; // CComBSTR m_sbstrOutputName;

        /// <summary>
        /// (R) A FileInfo instance of a output file.
        /// </summary>
        internal FileInfo OutputFileInfo
        {
            get { return outputFileInfo; }
        }

        /// <summary>
        /// <para>(R) Output file name</para>
        /// </summary>
        internal string OutputFileName
        {
            get { return outputFileInfo != null ? outputFileInfo.FullName : null; }
        }

        //------------------------------------------------------------
        // CInputSet Fields and Properties (4) Source Files
        //------------------------------------------------------------
        /// <summary>
        /// <para>Source files</para>
        /// <para>CTableNoCase of (source file name, CSourceFileInfo)</para>
        /// </summary>
        private CDictionary<CSourceFileInfo> sourceFileDictionary
            = new CDictionary<CSourceFileInfo>();

        /// <summary>
        /// <para>(R) CDictionary of (source file name, CSourceFileInfo)</para>
        /// </summary>
        internal CDictionary<CSourceFileInfo> SourceFileDictionary
        {
            get { return sourceFileDictionary; }
        }

        //------------------------------------------------------------
        // CInputSet Fields and Properties (4) Resource Files
        //------------------------------------------------------------
        /// <summary>
        /// <para>Resource files</para>
        /// </summary>
        private CDictionary<CResourceFileInfo> resourceFileDictionary
            = new CDictionary<CResourceFileInfo>();

        /// <summary>
        /// <para>(R) Resource files</para>
        /// </summary>
        internal CDictionary<CResourceFileInfo> ResourceFileDictionary
        {
            get { return resourceFileDictionary; }
        }

        /// <summary>
        /// <para>Logical Name to FileInfo</para>
        /// </summary>
        private CDictionary<CResourceFileInfo> resourceLogicalNameDictionary
            = new CDictionary<CResourceFileInfo>();

        /// <summary>
        /// <para>(R) Logical Name to FileInfo</para>
        /// </summary>
        internal CDictionary<CResourceFileInfo> ResourceLogicalNameDictionary
        {
            get { return this.resourceLogicalNameDictionary; }
        }

        //------------------------------------------------------------
        // CInputSet Fields and Properties (5) Win32 Resource and icon
        //------------------------------------------------------------
        /// <summary>
        /// <para>Win32 Resource file</para>
        /// <para>A FileInfo instance of Win32 resource.</para>
        /// </summary>
        internal FileInfo Win32ResourceFileInfo = null;  // CComBSTR m_sbstrResourceFile;

        /// <summary>
        /// <para>Win32 Icon file</para>
        /// <para>A FileInfo instance of Win32 icon file.</para>
        /// </summary>
        internal FileInfo Win32IconFileInfo = null;  // CComBSTR m_sbstrIconFile;

        //------------------------------------------------------------
        // CInputSet Fields and Properties (7) Main Class
        //------------------------------------------------------------
        /// <summary>
        /// Fully Qualified class name to use for Main()
        /// </summary>
        private string mainClassName = null;

        /// <summary>
        /// <para>(R) Fully Qualified class name to use for Main()</para>
        /// <para>If the output file is a DLL, reutrn null.</para>
        /// </summary>
        internal string MainClassName
        {
            get { return IsDLL ? null : mainClassName; }
        }

        //------------------------------------------------------------
        // CInputSet Fields and Properties (8) PDB File
        //------------------------------------------------------------
        /// <summary>
        /// A FileInfo instance of PDB file.
        /// </summary>
        private FileInfo pdbFileInfo = null;

        /// <summary>
        /// (R) A FileInfo instance of PDB file.
        /// </summary>
        internal FileInfo PDBFileInfo
        {
            get { return pdbFileInfo; }
        }

        /// <summary>
        /// <para>(R) PDB file name.</para>
        /// <para>FullName of PDB file.</para>
        /// </summary>
        internal string PDBFileName
        {
            get { return pdbFileInfo != null ? pdbFileInfo.FullName : null; }
        }

        //------------------------------------------------------------
        // CInputSet Fields and Properties (10) Other Options
        //------------------------------------------------------------
        /// <summary>
        /// Image Base
        /// </summary>
        private ulong imageBaseAddress = 0;

        /// <summary>
        /// (R) Image Base
        /// </summary>
        public ulong ImageBaseAddress
        {
            get { return this.imageBaseAddress; }
        }

        /// <summary>
        /// File Alignment
        /// </summary>
        private uint fileAlignment = 0;

        /// <summary>
        /// (R) File Alignment
        /// </summary>
        internal uint FileAlignment
        {
            get { return fileAlignment; }
        }

        //internal CInputSet m_pNext;

        //------------------------------------------------------------
        // CInputSet Constructor
        //
        /// <summary></summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal CInputSet(CController cntr)
        {
            DebugUtil.Assert(cntr != null);
            this.controller = cntr;
        }

        //------------------------------------------------------------
        // CInputSet.IsSourceFileInInputSet
        //
        /// <summary>
        /// Do not call. In sscli not defined.
        /// </summary>
        /// <param name="pName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal int IsSourceFileInInputSet(string pName)
        {
            throw new NotImplementedException("CInputSet.IsSourceFileInInputSet");
        }

        //------------------------------------------------------------
        // CInputSet.CopySources
        //
        /// <summary>
        /// Copy the Dictionary in sourceFileTable.
        /// </summary>
        /// <param name="srcDic"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CopySources(out Dictionary<string, CSourceFileInfo> srcDic)
        {
            sourceFileDictionary.DuplicateDictionary(out srcDic);
            return true;
        }

        //------------------------------------------------------------
        // CInputSet.CopyResources
        //
        /// <summary>
        /// Copy the Dictionary in resourceFileTable.
        /// </summary>
        /// <param name="resDic"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool CopyResources(out Dictionary<string, CResourceFileInfo> resDic)
        {
            resourceFileDictionary.DuplicateDictionary(out resDic);
            return true;
        }

        //------------------------------------------------------------
        // CinputSet.AddSourceFile (1)
        //
        /// <summary>
        /// <para>Create a CSourceFileInfo instance and register it.</para>
        /// <para>If its file name is already registerd, return false.</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool AddSourceFile(string fileName)
        {
            FileInfo fi = null;
            Exception excp = null;

            if (!IOUtil.CreateFileInfo(fileName, out fi, out excp) ||
                fi == null ||
                String.IsNullOrEmpty(fi.FullName))
            {
                if (excp != null)
                {
                    this.Controller.ReportError(ERRORKIND.ERROR, excp);
                }
                return false;
            }

            CSourceFileInfo si = new CSourceFileInfo(fi);
            return sourceFileDictionary.Add(si.SearchName, si);
        }

        //------------------------------------------------------------
        // CinputSet.AddSourceFile (2)
        //
        /// <summary>
        /// <para>Register FileInfo instance.</para>
        /// <para>If already registered. return false.</para>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool AddSourceFile(FileInfo info)
        {
            if (info == null || String.IsNullOrEmpty(info.FullName))
            {
                return false;
            }

            CSourceFileInfo si = new CSourceFileInfo(info);
            return sourceFileDictionary.Add(si.SearchName, si);
        }

        //------------------------------------------------------------
        // CinputSet.RemoveSourceFile
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        //------------------------------------------------------------
        virtual internal void RemoveSourceFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return;
            }
            string fullName = null;

            if (FileNameToKeyString(fileName, out fullName))
            {
                sourceFileDictionary.Remove(fileName);
            }
        }

        //------------------------------------------------------------
        // CinputSet.RemoveAllSourceFiles
        //
        /// <summary></summary>
        //------------------------------------------------------------
        virtual internal void RemoveAllSourceFiles()
        {
            sourceFileDictionary.Clear();
        }

        //------------------------------------------------------------
        // CinputSet.AddResourceFile (1)
        //
        /// <summary>
        /// <para>Regisger the pair of id and Info.</para>
        /// <para>If id is already registerd, do not register.</para>
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="id"></param>
        /// <param name="embed"></param>
        /// <param name="pub"></param>
        //------------------------------------------------------------
        virtual internal void AddResourceFile(FileInfo fi, string id, bool embed, bool pub)
        {
            if (fi == null || String.IsNullOrEmpty(fi.Name))
            {
                return;
            }
            if (String.IsNullOrEmpty(id) || resourceFileDictionary.Contains(id))
            {
                return;
            }

            CResourceFileInfo resfInfo = new CResourceFileInfo(fi, id, embed, pub);
            resourceFileDictionary.Add(resfInfo.LogicalName, resfInfo);
        }

        //------------------------------------------------------------
        // CinputSet.AddResourceFile (2)
        //
        /// <summary>
        /// <para>Regisger the pair of id and Info created by fileName.</para>
        /// <para>If id is already registerd, do not register.</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="id"></param>
        /// <param name="embed"></param>
        /// <param name="vis"></param>
        //------------------------------------------------------------
        virtual internal void AddResourceFile(string fileName, string id, bool embed, bool vis)
        {
            if (String.IsNullOrEmpty(fileName)) return;

            // Create a FileInfo instance by fileName.
            FileInfo fi;
            Exception excp = null;
            if (IOUtil.CreateFileInfo(fileName, out fi, out excp) && fi != null)
            {
                AddResourceFile(fi, id, embed, vis);
            }
            else if (excp != null && this.Controller != null)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, excp);
            }
        }

        //------------------------------------------------------------
        // CinputSet.RemoveResourceFile
        //
        /// <summary></summary>
        /// <param name="id"></param>
        //------------------------------------------------------------
        virtual internal void RemoveResourceFile(string id)
        {
            resourceFileDictionary.Remove(id);
        }

        //------------------------------------------------------------
        // CinputSet.SetWin32Resource (1)
        // 
        /// <summary></summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetWin32Resource(FileInfo fileInfo)
        {
            this.Win32ResourceFileInfo = fileInfo;

            if (this.Win32ResourceFileInfo != null &&
                this.Win32IconFileInfo != null)
            {
                this.controller.ReportError(
                    CSCERRID.ERR_CantHaveWin32ResAndIcon,
                    ERRORKIND.ERROR);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // CinputSet.SetWin32Resource (2)
        // 
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetWin32Resource(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }

            FileInfo fileInfo = null;
            Exception excp = null;
            if (IOUtil.CreateFileInfo(fileName, out fileInfo, out excp))
            {
                return SetWin32Resource(fileInfo);
            }

            if (excp != null)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, excp);
            }
            else
            {
                this.controller.ReportInternalCompilerError("CinputSet.SetWin32Resource");
            }
            return false;
        }

        //------------------------------------------------------------
        // CinputSet.SetOutputFile (1)
        //
        /// <summary></summary>
        /// <param name="fi"></param>
        //------------------------------------------------------------
        virtual internal void SetOutputFile(FileInfo fi)
        {
            this.outputFileInfo = fi;
        }

        //------------------------------------------------------------
        // CinputSet.SetOutputFile (2)
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetOutputFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                this.outputFileInfo = null;
                this.target = TargetType.None;  //noOutput = true;
                return true;
            }
            //noOutput = false;

            Exception excp = null;
            if (IOUtil.CreateFileInfo(fileName, out outputFileInfo, out excp))
            {
                return true;
            }
            if (excp != null && this.Controller != null)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, excp);
            }
            return false;
        }

        //------------------------------------------------------------
        // CinputSet.SetOutputFileType
        //
        /// <summary>
        /// <para>(sscli) STDMETHOD(SetOutputFileType)(DWORD dwFileType)</para>
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetTargetType(TargetType target)
        {
            this.target = target;
            return true;
        }

        //------------------------------------------------------------
        // CinputSet.SetImageBase
        //
        /// <summary></summary>
        /// <param name="imageBase"></param>
        //------------------------------------------------------------
        virtual internal void SetImageBase(ulong baseAddr)
        {
            baseAddr =
                (baseAddr + (ulong)(0x00008000)) & (ulong)(0xFFFFFFFFFFFF0000);
            imageBaseAddress = baseAddr;
        }

        //------------------------------------------------------------
        // CinputSet.SetMainClass
        //
        /// <summary></summary>
        /// <param name="className"></param>
        //------------------------------------------------------------
        virtual internal void SetMainClass(string className)
        {
            if (IsExe)
            {
                mainClassName = className;
            }
            else
            {
                mainClassName = null;
            }
        }

        //------------------------------------------------------------
        // CinputSet.SetWin32Icon (1)
        // 
        /// <summary></summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetWin32Icon(FileInfo fileInfo)
        {
            this.Win32IconFileInfo = fileInfo;

            if (this.Win32ResourceFileInfo != null &&
                this.Win32IconFileInfo != null)
            {
                this.controller.ReportError(
                    CSCERRID.ERR_CantHaveWin32ResAndIcon,
                    ERRORKIND.ERROR);
                return false;
            }
            return true;
        }

        //------------------------------------------------------------
        // CinputSet.SetWin32Icon
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetWin32Icon(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return true;
            }

            if (this.Win32ResourceFileInfo != null)
            {
                return false;
            }

            FileInfo fileInfo = null;
            Exception excp = null;
            if (IOUtil.CreateFileInfo(fileName, out fileInfo, out excp))
            {
                return SetWin32Icon(fileInfo);
            }

            if (excp != null)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, excp);
            }
            else
            {
                this.controller.ReportInternalCompilerError("CinputSet.SetWin32Icon");
            }
            return false;
        }

        //------------------------------------------------------------
        // CinputSet.SetFileAlignment
        //
        /// <summary></summary>
        /// <param name="align"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetFileAlignment(uint align)
        {
            uint temp = (align >> 9);
            if (align < 0x0000200 || align > 0x0002000)
            {
                return false;
            }

            while ((temp & 0x0001) == 0)
            {
                temp >>= 1;
            }
            if (temp != 1)
            {
                return false;
            }

            this.fileAlignment = align;
            return true;
        }

        //------------------------------------------------------------
        // CinputSet.SetPDBFile (1)
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetPDBFile(FileInfo fileInfo)
        {
            this.pdbFileInfo = fileInfo;
            return true;
        }

        //------------------------------------------------------------
        // CinputSet.SetPDBFile (2)
        //
        /// <summary></summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool SetPDBFile(string fileName)
        {
            pdbFileInfo = null;
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }

            FileInfo fileInfo = null;
            Exception excp = null;
            if (IOUtil.CreateFileInfo(fileName, out fileInfo, out excp))
            {
                return SetPDBFile(fileInfo);
            }

            if (excp != null && this.Controller != null)
            {
                this.Controller.ReportError(ERRORKIND.ERROR, excp);
            }
            return false;
        }

        //------------------------------------------------------------
        // CinputSet.FileNameToKeyString
        //
        /// <summary>
        /// Get FullName from fileName. FullName is used as key of CSourceFileInfoTable.
        /// (CSourceFileInfoTable ignores case of key.）
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="keyStr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool FileNameToKeyString(string fileName, out string keyStr)
        {
            keyStr = null;
            FileInfo finfo = null;
            Exception excp = null;

            if (!IOUtil.CreateFileInfo(fileName, out finfo, out excp) || finfo == null)
            {
                if (excp != null && this.Controller != null)
                {
                    this.Controller.ReportError(ERRORKIND.ERROR, excp);
                }
                return false;
            }

            keyStr = finfo.FullName.ToLower();
            return true;
        }

#if DEBUG
        //------------------------------------------------------------
        // CinputSet.Debug
        //------------------------------------------------------------
        internal void Debug(StringBuilder sb)
        {
            string temp = null;
            string indent = "  ";

            sb.AppendFormat("Target        : {0}\n", this.target);

            temp = outputFileInfo != null ? outputFileInfo.Name : null;
            sb.AppendFormat("OutFile       : {0}\n", temp);

            sb.AppendFormat("Source Files  :\n");
            foreach (KeyValuePair<string, CSourceFileInfo> kv
                in sourceFileDictionary.Dictionary)
            {
                kv.Value.Debug(sb, indent);
            }

            sb.AppendFormat("Resource Files:\n");
            foreach (KeyValuePair<string, CResourceFileInfo> kv
                in resourceFileDictionary.Dictionary)
            {
                kv.Value.Debug(sb, indent);
            }

            temp = Win32ResourceFileInfo != null ? Win32ResourceFileInfo.Name : null;
            sb.AppendFormat("Win32Res      : {0}\n", temp);

            temp = Win32IconFileInfo != null ? Win32IconFileInfo.Name : null;
            sb.AppendFormat("Win32Icon     : {0}\n", temp);

            sb.AppendFormat("Main Class    : {0}\n", mainClassName);

            temp = pdbFileInfo != null ? pdbFileInfo.Name : null;
            sb.AppendFormat("PDB File      : {0}\n", temp);

            sb.AppendFormat("Base Address  : 0x{0,8:X8}\n", imageBaseAddress);
            sb.AppendFormat("File Alignment: 0x{0,8:X8}\n", fileAlignment);
        }
#endif
    }
}
