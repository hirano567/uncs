//============================================================================
// AssemblyBuilderEx.cs
//
// 2015/11/20
//============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Globalization;
using System.Configuration.Assemblies;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;

namespace Uncs
{
    //======================================================================
    // class CModuleBuilderEx
    //
    /// <summary></summary>
    //======================================================================
    internal class CModuleBuilderEx : CModuleEx
    {
        //------------------------------------------------------------
        // CModuleBuilderEx Fields and Properties (1) ModuleBuilder
        //------------------------------------------------------------
        protected ModuleBuilder moduleBuilder = null;

        internal ModuleBuilder ModuleBuilder
        {
            get { return this.moduleBuilder; }
        }

        internal override Guid VersionID
        {
            get
            {
                if (this.moduleBuilder != null)
                {
                    return this.moduleBuilder.ModuleVersionId;
                }
                return Guid.Empty;
            }
        }

        protected CAssemblyBuilderEx assemblyBuilderEx = null;

        internal CAssemblyBuilderEx AssemblyBuilderEx
        {
            get { return this.assemblyBuilderEx; }
        }

        //------------------------------------------------------------
        // CModuleBuilderEx Constructor
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal CModuleBuilderEx(CController cntr) : base(cntr) { }

        //------------------------------------------------------------
        // CModuleBuilderEx.Init (1)
        //
        /// <summary></summary>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <returns></returns>
        /// <remarks>We have
        /// CMetadataFile.Init(FileInfo fi, IMetaDataError error, CAsmLink linker)
        /// CMetadataFile.Init(string fileName, IMetaDataError error, CAsmLink linker)
        /// </remarks>
        //------------------------------------------------------------
        internal bool Init(CAsmLink linker)
        {
            this.InitBase(linker);
            return true;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx SetModuleBuilder
        //
        /// <summary></summary>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal void SetModuleBuilder(ModuleBuilder builder)
        {
            DebugUtil.Assert(builder != null);

            this.module = builder;
            this.moduleBuilder = builder;
            this.SourceFileInfo = null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx SetAssemblyBuilderEx
        //
        /// <summary></summary>
        /// <param name="asm"></param>
        //------------------------------------------------------------
        internal void SetAssemblyBuilderEx(CAssemblyBuilderEx asm)
        {
            this.assemblyBuilderEx = asm;
            this.assemblyEx = asm as CAssemblyEx;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineType (1)
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeBuilder DefineType(
            string name,
            TypeAttributes flags)
        {
            DebugUtil.Assert(this.moduleBuilder != null);
            Exception excp = null;

            try
            {
                return this.moduleBuilder.DefineType(
                    name,
                    flags);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineType (2)
        //
        /// <summary>
        /// Create a TypeBuilder of this module.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="baseType"></param>
        /// <param name="ifaces"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeBuilder DefineType(
            string name,
            TypeAttributes flags,
            Type baseType,
            Type[] ifaces)
        {
            DebugUtil.Assert(this.moduleBuilder != null);
            Exception excp = null;

            try
            {
                return this.moduleBuilder.DefineType(
                    name,
                    flags,
                    baseType,
                    ifaces);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineNestedType (1)
        //
        /// <summary>
        /// Create a TypeBuilder of a nested type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="declaringType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeBuilder DefineNestedType(
            string name,
            TypeAttributes flags,
            TypeBuilder declaringType)
        {
            DebugUtil.Assert(this.moduleBuilder != null);
            DebugUtil.Assert(declaringType != null);
            Exception excp = null;

            try
            {
                return declaringType.DefineNestedType(
                    name,
                    flags);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineNestedType (2)
        //
        /// <summary>
        /// Create a TypeBuilder of a nested type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="declaringType"></param>
        /// <param name="baseType"></param>
        /// <param name="ifaces"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal TypeBuilder DefineNestedType(
            string name,
            TypeAttributes flags,
            TypeBuilder declaringType,
            Type baseType,
            Type[] ifaces)
        {
            DebugUtil.Assert(this.moduleBuilder != null);
            DebugUtil.Assert(declaringType != null);
            Exception excp = null;

            try
            {
                return declaringType.DefineNestedType(
                    name,
                    flags,
                    baseType,
                    ifaces);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineField
        //
        /// <summary>
        /// Create a FieldBuilder instance and return it.
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="flags"></param>
        /// <param name="constValue"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FieldBuilder DefineField(
            Type parentType,
            string name,
            Type fieldType,
            FieldAttributes flags,
            object constValue)
        {
            TypeBuilder typeBuilder = parentType as TypeBuilder;
            if (typeBuilder == null ||
                String.IsNullOrEmpty(name))
            {
                return null;
            }

            FieldBuilder fieldBuilder = null;
            Exception excp = null;

            try
            {
                fieldBuilder = typeBuilder.DefineField(
                     name,
                     fieldType,
                     flags);
            }
            catch (ArgumentException ex)
            {
                fieldBuilder = null;
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                fieldBuilder = null;
                excp = ex;
            }

            if (fieldBuilder == null)
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                return null;
            }
            DebugUtil.Assert(fieldBuilder != null);

            //--------------------------------------------------------
            // Set the constant value.
            //--------------------------------------------------------
            if (constValue != null)
            {
                try
                {
                    if (fieldType.IsEnum &&
                        fieldType != parentType)
                    {
                        object enumObject = Enum.ToObject(fieldType, constValue);
                        fieldBuilder.SetConstant(enumObject);
                    }
                    else
                    {
                        fieldBuilder.SetConstant(constValue);
                    }
                }
                catch (ArgumentException ex)
                {
                    fieldBuilder = null;
                    excp = ex;
                }
                catch (InvalidOperationException ex)
                {
                    fieldBuilder = null;
                    excp = ex;
                }

                if (fieldBuilder == null)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, excp);
                }
            }
            return fieldBuilder;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineMethod (1)
        //
        /// <summary></summary>
        /// <param name="parentType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="returnType"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MethodBuilder DefineMethod(
            Type parentType,
            string name,
            MethodAttributes flags,
            CallingConventions callConv)
        {
            TypeBuilder typeBuilder = parentType as TypeBuilder;
            if (typeBuilder == null ||
                String.IsNullOrEmpty(name))
            {
                return null;
            }

            Exception excp = null;

            try
            {
                return typeBuilder.DefineMethod(
                    name,
                    flags,
                    callConv);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineMethod (2)
        //
        /// <summary></summary>
        /// <param name="parentType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="returnType"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MethodBuilder DefineMethod(
            Type parentType,
            string name,
            MethodAttributes flags,
            CallingConventions callConv,
            Type returnType,
            Type[] parameterTypes)
        {
            TypeBuilder typeBuilder = parentType as TypeBuilder;
            if (typeBuilder == null ||
                String.IsNullOrEmpty(name))
            {
                return null;
            }

            Exception excp = null;

            try
            {
                return typeBuilder.DefineMethod(
                    name,
                    flags,
                    callConv,
                    returnType,
                    parameterTypes);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineMethodOverride
        //
        /// <summary></summary>
        /// <param name="parentType"></param>
        /// <param name="methodBody"></param>
        /// <param name="methodDeclaration"></param>
        //------------------------------------------------------------
        internal bool DefineMethodOverride(
            Type parentType,
            MethodInfo methodBody,
            MethodInfo methodDeclaration)
        {
            TypeBuilder typeBuilder = parentType as TypeBuilder;
            if (typeBuilder == null ||
                !(methodBody is MethodBuilder))
            {
                return false;
            }

            Exception excp = null;

            try
            {
                typeBuilder.DefineMethodOverride(
                    methodBody,
                    methodDeclaration);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return false;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineConstructor
        //
        /// <summary></summary>
        /// <param name="parentType"></param>
        /// <param name="flags"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ConstructorBuilder DefineConstructor(
            Type parentType,
            MethodAttributes flags,
            Type[] parameterTypes)
        {
            TypeBuilder typeBuilder = parentType as TypeBuilder;
            if (typeBuilder == null)
            {
                return null;
            }
            Exception excp = null;

            try
            {
                return typeBuilder.DefineConstructor(
                        flags,
                        CallingConventions.Standard,
                        parameterTypes);
            }
            catch (InvalidCastException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.GetArrayMethod
        //
        /// <summary></summary>
        /// <param name="arrayType"></param>
        /// <param name="name"></param>
        /// <param name="conventions"></param>
        /// <param name="returnType"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal MethodInfo GetArrayMethod(
            Type arrayType,
            string name,
            CallingConventions conventions,
            Type returnType,
            Type[] paramTypes)
        {
            Exception excp = null;
            try
            {
                return this.moduleBuilder.GetArrayMethod(
                    arrayType,
                    name,
                    conventions,
                    returnType,
                    paramTypes);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineProperty
        //
        /// <summary>
        /// Create a PropertyBuilder instance and return it.
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="propType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="getInfo"></param>
        /// <param name="setInfo"></param>
        /// <param name="indexParams"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal PropertyBuilder DefineProperty(
            Type parentType,
            string name,
            PropertyAttributes flags,
            Type propType,
            object defaultValue,
            MethodInfo getInfo,
            MethodInfo setInfo,
            Type[] indexParams)
        {
            TypeBuilder typeBuilder = parentType as TypeBuilder;
            if (typeBuilder == null ||
                String.IsNullOrEmpty(name))
            {
                return null;
            }

            PropertyBuilder propBuilder = null;
            Exception excp = null;

            try
            {
                propBuilder = typeBuilder.DefineProperty(
                    name,
                    flags,
                    propType,
                    indexParams);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }

            if (propBuilder == null)
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                return null;
            }
            DebugUtil.Assert(propBuilder != null);

            if (defaultValue != null)
            {
                propBuilder.SetConstant(defaultValue);
            }

            MethodBuilder accBuilder = getInfo as MethodBuilder;
            if (accBuilder != null)
            {
                propBuilder.SetGetMethod(accBuilder);
            }
            accBuilder = setInfo as MethodBuilder;
            if (accBuilder != null)
            {
                propBuilder.SetSetMethod(accBuilder);
            }

            return propBuilder;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineEvent
        //
        /// <summary>
        /// Create a EventBuilder instance and return it.
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal EventBuilder DefineEvent(
            Type parentType,
            string name,
            EventAttributes flags,
            Type eventType,
            MethodBuilder addMethBuilder,
            MethodBuilder removeMethBuilder)
        {
            TypeBuilder typeBuilder = parentType as TypeBuilder;
            if (typeBuilder == null ||
                String.IsNullOrEmpty(name))
            {
                return null;
            }

            EventBuilder eventBuilder = null;
            Exception excp = null;

            try
            {
                eventBuilder= typeBuilder.DefineEvent(
                        name,
                        flags,
                        eventType);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
                eventBuilder = null;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
                eventBuilder = null;
            }

            if (eventBuilder == null)
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                return null;
            }

            if (addMethBuilder != null)
            {
                eventBuilder.SetAddOnMethod(addMethBuilder);
            }
            if (removeMethBuilder != null)
            {
                eventBuilder.SetRemoveOnMethod(removeMethBuilder);
            }

            return eventBuilder;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineInitializedDataField
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FieldBuilder DefineInitializedDataField(
            string name,
            byte[] data,
            FieldAttributes flags)
        {
            Exception excp = null;

            try
            {
                return this.moduleBuilder.DefineInitializedData(
                    name,
                    data,
                    flags);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineUninitializedDataField
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal FieldBuilder DefineUninitializedDataField(
            string name,
            int size,
            FieldAttributes flags)
        {
            Exception excp = null;

            try
            {
                return this.moduleBuilder.DefineUninitializedData(
                    name,
                    size,
                    flags);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.CreateGlobalFunctions
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void CreateGlobalFunctions()
        {
            if (this.moduleBuilder != null)
            {
                this.moduleBuilder.CreateGlobalFunctions();
            }
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.GetMetadataToken
        //
        /// <summary></summary>
        /// <param name="methInfo"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetMetadataToken(MethodInfo methInfo, out int token)
        {
            Exception excp = null;
            try
            {
                MethodToken methToken = this.moduleBuilder.GetMethodToken(methInfo);
                token = methToken.Token;
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            token = 0;
            return false;
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineEmbeddedResources
        //
        /// <summary></summary>
        /// <param name="resourceSym"></param>
        //------------------------------------------------------------
        internal void DefineEmbeddedResources(RESFILESYM resourceSym)
        {
            if (this.moduleBuilder == null ||
                resourceSym == null ||
                !resourceSym.IsEmbedded)
            {
                return;
            }

            FileInfo resFileInfo = resourceSym.FileInfo;
            if (resFileInfo == null || !resFileInfo.Exists)
            {
                return;
            }

            ResourceReader resReader = null;
            IResourceWriter resWriter = null;
            Exception excp = null;

            try
            {
                resReader = new ResourceReader(resFileInfo.FullName);
                resWriter = this.moduleBuilder.DefineResource(
                    resFileInfo.Name,
                    resFileInfo.FullName,
                    resourceSym.Accessibility);

                foreach (DictionaryEntry reskv in resReader)
                {
                    string name = reskv.Key as string;
                    if (!String.IsNullOrEmpty(name))
                    {
                        resWriter.AddResource(name, reskv.Value);
                    }
                }
                return;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (FileNotFoundException ex)
            {
                excp = ex;
            }
            catch (IOException ex)
            {
                excp = ex;
            }
            catch (BadImageFormatException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            finally
            {
                if (resReader != null)
                {
                    resReader.Close(); // No ResourceReader.Dispose() in .Net 3.5 or before.
                }
                //if (resWriter != null)
                //{
                //    resWriter.Dispose();
                //}
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
        }

        //------------------------------------------------------------
        // CModuleBuilderEx.DefineUnmanagedEmbeddedResources
        //
        /// <summary></summary>
        /// <param name="resFileInfo"></param>
        //------------------------------------------------------------
        internal void DefineUnmanagedEmbeddedResources(FileInfo resFileInfo)
        {
            if (resFileInfo == null ||
                String.IsNullOrEmpty(resFileInfo.Name) ||
                this.moduleBuilder == null)
            {
                return;
            }
            if (!resFileInfo.Exists)
            {
                this.controller.ReportError(
                    CSCERRID.ERR_CantOpenWin32Res,
                    ERRORKIND.ERROR,
                    resFileInfo.Name,
                    "not found.");
                return;
            }

            Exception excp = null;
            byte[] resImage = null;

            if (!IOUtil.ReadBinaryFile(resFileInfo.FullName, out resImage, out excp))
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
            }

            try
            {
                this.moduleBuilder.DefineUnmanagedResource(resImage);
            }
            catch (ArgumentException ex)
            {
                this.controller.ReportError(ERRORKIND.ERROR, ex);
            }
        }
    }

    //======================================================================
    // class CAssemblyInitialAttributes
    //
    /// <summary>
    /// <para>Stores the arguments set to System.Reflection.AssemblyName instances.</para>
    /// <para>(CSharp\Alink\Dll\AssemblyBuilderEx.cs)</para>
    /// </summary>
    //======================================================================
    internal class CAssemblyInitialAttributes
    {
        internal enum Kind
        {
            NotInitialData = 0,
            Culture,
            DelaySign,
            HashAlgorithm,
            KeyFile,
            KeyName,
            Version,
        }

        internal static Type AssemblyAlgorithmType = typeof(AssemblyAlgorithmIdAttribute);
        internal static Type AssemblyCultureType = typeof(AssemblyCultureAttribute);
        internal static Type AssemblyDelaySignType = typeof(AssemblyDelaySignAttribute);
        internal static Type AssemblyKeyFileType = typeof(AssemblyKeyFileAttribute);
        internal static Type AssemblyKeyNameType = typeof(AssemblyKeyNameAttribute);
        internal static Type AssemblyVersionType = typeof(AssemblyVersionAttribute);

        //------------------------------------------------------------
        // CAssemblyInitialData.NeedToInit (static)
        //
        /// <summary></summary>
        /// <param name="attrType"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static Kind NeedToInit(Type attrType)
        {
            if (attrType == AssemblyAlgorithmType)
            {
                return Kind.HashAlgorithm;
            }
            else if (attrType == AssemblyCultureType)
            {
                return Kind.Culture;
            }
            else if (attrType == AssemblyDelaySignType)
            {
                return Kind.DelaySign;
            }
            else if (attrType == AssemblyKeyFileType)
            {
                return Kind.KeyFile;
            }
            else if (attrType == AssemblyKeyNameType)
            {
                return Kind.KeyName;
            }
            else if (attrType == AssemblyVersionType)
            {
                return Kind.Version;
            }
            return Kind.NotInitialData;
        }

        //------------------------------------------------------------
        // CAssemblyInitialData Fields and Properties
        //------------------------------------------------------------
        internal AssemblyHashAlgorithm? HashAlgorithm = null;

        internal CultureInfo Culture = null;

        internal bool? DelaySign = null;

        internal FileInfo KeyFileInfo = null;

        internal string KeyName = null;

        internal Version Version = null;

        //------------------------------------------------------------
        // CAssemblyInitialData.SetHashAlgorithm
        //
        /// <summary></summary>
        /// <param name="halgo"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetHashAlgorithm(AssemblyHashAlgorithm halgo)
        {
            this.HashAlgorithm = halgo;
            return true;
        }

        //------------------------------------------------------------
        // CAssemblyInitialData.SetCulture
        //
        /// <summary></summary>
        /// <param name="cultureStr"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetCulture(string cultureStr, out Exception excp)
        {
            excp = null;
            try
            {
                this.Culture = new CultureInfo(cultureStr);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // CAssemblyInitialData.SetDelaySign
        //
        /// <summary></summary>
        /// <param name="delay"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetDelaySign(bool delay, out Exception excp)
        {
            excp = null;
            this.DelaySign = delay;
            return true;
        }

        //------------------------------------------------------------
        // CAssemblyInitialData.SetKeyFile
        //
        /// <summary></summary>
        /// <param name="keyFile"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetKeyFile(string keyFile, out Exception excp)
        {
            excp = null;
            return IOUtil.FileExists(keyFile, out this.KeyFileInfo, out excp);
        }

        //------------------------------------------------------------
        // CAssemblyInitialData.SetKeyName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetKeyName(string name, out Exception excp)
        {
            excp = null;
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            this.KeyName = name;
            return true;
        }

        //------------------------------------------------------------
        // CAssemblyInitialData.SetVersion
        //
        /// <summary></summary>
        /// <param name="versionStr"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetVersion(string versionStr, out Exception excp)
        {
            return ReflectionUtil.CreateVersion(
                versionStr,
                out this.Version,
                out excp);
        }

        //------------------------------------------------------------
        // CAssemblyInitialData.SetToAssemblyName
        //
        /// <summary></summary>
        /// <param name="asmName"></param>
        /// <param name="options"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetToAssemblyName(
            AssemblyName asmName,
            COptionManager options,
            out Exception excp)
        {
            excp = null;
            bool br = true;

            //--------------------------------------------------------
            // CultureInfo
            //--------------------------------------------------------
            if (this.Culture != null)
            {
                asmName.CultureInfo = this.Culture;
            }

            //--------------------------------------------------------
            // HashAlgorithm
            //--------------------------------------------------------
            if (this.HashAlgorithm.HasValue)
            {
                asmName.HashAlgorithm = this.HashAlgorithm.Value;
            }

            //--------------------------------------------------------
            // KeyPair, KeyName, DelaySign (1)
            //
            // Determine the DelaySign first.
            //--------------------------------------------------------
            bool delaySign = false;

            if (options != null && options.DelaySign.HasValue)
            {
                delaySign = options.DelaySign.Value;
            }
            else if (this.DelaySign.HasValue)
            {
                delaySign = this.DelaySign.Value;
            }

            //--------------------------------------------------------
            // KeyPair, KeyName, DelaySign (2)
            //
            // If KeyName is specified, use KeyName.
            //--------------------------------------------------------
            bool setKeyPair = false;

            string keyName = null;

            if (options != null)
            {
                keyName = options.KeyContainer;
            }
            if (String.IsNullOrEmpty(keyName) && !String.IsNullOrEmpty(this.KeyName))
            {
                keyName = this.KeyName;
            }

            if (!String.IsNullOrEmpty(keyName))
            {
                //----------------------------------------------------
                // KeyName, Not DelaySign
                //----------------------------------------------------
                if (!delaySign)
                {
                    try
                    {
                        asmName.KeyPair = new StrongNameKeyPair(keyName);
                        setKeyPair = true;
                    }
                    catch (ArgumentException ex)
                    {
                        excp = ex;
                    }
                    catch (SecurityException ex)
                    {
                        excp = ex;
                    }
                }
                //----------------------------------------------------
                // KeyName, DelaySign
                //----------------------------------------------------
                else
                {
                    // delaysign
                    throw new NotImplementedException("CAssemblyInitialData.SetToAssemblyName");
                }
            }

            //--------------------------------------------------------
            // KeyPair, KeyName, DelaySign (3)
            //
            // If no KeyName, use KeyFile.
            //--------------------------------------------------------
            if (!setKeyPair)
            {
                string keyFile = null;

                if (options != null && options.KeyFileInfo != null)
                {
                    keyFile = options.KeyFileInfo.FullName;
                }
                if (String.IsNullOrEmpty(keyFile) && this.KeyFileInfo != null)
                {
                    keyFile = this.KeyFileInfo.FullName;
                }

                if (!String.IsNullOrEmpty(keyFile))
                {
                    byte[] bytes = null;
                    if (IOUtil.ReadBinaryFile(keyFile, out bytes, out excp))
                    {
                        //----------------------------------------
                        // KeyFile, Not DelaySign
                        //----------------------------------------
                        if (!delaySign)
                        {
                            try
                            {
                                asmName.KeyPair = new StrongNameKeyPair(bytes);
                            }
                            catch (ArgumentException ex)
                            {
                                excp = ex;
                                br = false;
                            }
                            catch (SecurityException ex)
                            {
                                excp = ex;
                                br = false;
                            }
                        }
                        //------------------------------------------------
                        // KeyFile, DelaySign
                        //------------------------------------------------
                        else
                        {
                            asmName.SetPublicKey(bytes);
                            // SetPublicKey throws no exception. (MSDN)
                        }
                    }
                    else
                    {
                        br = false;
                    }
                } // if (!String.IsNullOrEmpty(keyFile))
            }

            //--------------------------------------------------------
            // Version
            //--------------------------------------------------------
            if (this.Version != null)
            {
                asmName.Version = this.Version;
            }
            return br;
        }
    }

    //======================================================================
    // class CAssemblyBuilderEx
    //
    /// <summary></summary>
    //======================================================================
    internal class CAssemblyBuilderEx : CAssemblyEx //CMetadataFile
    {
        //------------------------------------------------------------
        // CAssemblyBuilderEx Fields and Properties (1) AssemblyBuilder
        //------------------------------------------------------------

        protected AssemblyBuilder assemblyBuilder = null;

        internal AssemblyBuilder AssemblyBuilder
        {
            get { return this.assemblyBuilder; }
        }

        // assemblyName field of type AssemblyName is defined in CAssemblyEx
        // assemblyName is used by AssemblyBuilder.Save() method.

        // Use CMetadata.FileInfo to the assmbly file name.

        protected AssemblyBuilderAccess accessFlag = AssemblyBuilderAccess.RunAndSave;

        internal override string Name
        {
            get { return (this.assemblyName != null ? assemblyName.Name : null); }
        }

        internal string FullName
        {
            get { return (this.assemblyName != null ? this.assemblyName.FullName : null); }
        }

        internal override string FileName
        {
            get { return this.FileInfo != null ? this.FileInfo.Name : null; }
        }

        internal AssemblyBuilderAccess AccessFlag
        {
            get { return this.accessFlag; }
        }

        internal bool SetToSave
        {
            get { return (this.accessFlag & AssemblyBuilderAccess.Save) != 0; }
        }


        protected object lockObject = new object();

        //------------------------------------------------------------
        // CAssemblyBuilderEx Fields and Properties (2) Module, types, members
        //------------------------------------------------------------

        protected CModuleBuilderEx manifestModuleBuilder = null;

        internal override Guid ManifestModuleVersionID
        {
            get
            {
                if (this.manifestModuleBuilder != null)
                {
                    return this.manifestModuleBuilder.VersionID;
                }
                return Guid.Empty;
            }
        }

        /// <summary>
        /// <para>for multi file assembly.</para>
        /// <para>(::CSimpleArray&lt;CFile&gt; m_Files;)</para>
        /// </summary>
        //protected CDictionary<CModuleEx> cmoduleDictionary = new CDictionary<CModuleEx>();

        protected const string defaultManifestModuleName = "__manifestModule";

        /// <summary>
        /// Dictionary of TypeBuilder and EnumBuilder
        /// </summary>
        //Dictionary<string, object> typeBuilderDic = new Dictionary<string, object>();

        //------------------------------------------------------------
        // CAssemblyBuilderEx Fields and Properties (3)
        //
        // from CFile of sscli
        //------------------------------------------------------------
        // State info

        protected bool isExe = false;                       // bool m_bIsExe : 1;

        protected bool hasPE = false;                       // bool m_bHasPE : 1;

        /// <summary>
        /// Only valid if m_bHasPE is set
        /// </summary>
        /// <remarks>
        /// (sscli) DWORD m_dwPEKind;
        /// </remarks>
        protected PortableExecutableKinds peKind = PortableExecutableKinds.ILOnly;

        /// <summary>
        /// Only valid if m_bHasPE is set
        /// </summary>
        /// <remarks>
        /// (sscli) DWORD m_dwMachine;
        /// </remarks>
        protected ImageFileMachine machine = ImageFileMachine.I386;

        protected bool isCheckedPE = false;                 // bool m_bCheckedPE : 1;

        protected bool hasEmittedPublicTypes = false;       // bool m_bEmittedPublicTypes : 1;

        /// <summary>
        /// Only set when m_bEmittedPublicTypes is set  (to CAssemblyBuilder)
        /// </summary>
        protected bool hasEmittedInternalTypes = false;     // bool m_bEmittedInternalTypes : 1;

        protected bool hasEmittedTypeForwarders = false;    // bool m_bEmittedTypeForwarders : 1;

        //------------------------------------------------------------
        // CAssemblyBuilderEx Fields and Properties (4)
        //
        // from CAssembly of sscli
        //------------------------------------------------------------

        // State info

        // bool m_isVersionSet; (to CAssemblyBuilder)
        // bool m_isAlgIdSet; (to CAssemblyBuilder)
        // bool m_isFlagsSet; (to CAssemblyBuilder)
        // bool m_isHalfSignSet; (to CAssemblyBuilder)
        // bool m_isAutoKeyName; (to CAssemblyBuilder)
        // bool m_bAllowExes; (to CAssemblyBuilder)
        // bool m_bIsInMemory; (to CAssemblyBuilder)
        protected bool isClean = false;               // bool m_bClean; (to CAssemblyBuilder)
        protected bool doHash = false;              // bool m_bDoHash; (to CAssemblyBuilder)
        protected bool doDupTypeCheck = false;      // bool m_bDoDupTypeCheck; (to CAssemblyBuilder)
        protected bool doDupeCheckTypeFwds = false; // bool m_bDoDupeCheckTypeFwds; (to CAssemblyBuilder)

        // PBYTE m_pbKeyPair; (to CAssemblyBuilder)
        // DWORD m_cbKeyPair; (to CAssemblyBuilder)
        // PublicKeyBlob * m_pPubKey; (to CAssemblyBuilder)
        // DWORD m_cbPubKey; (to CAssemblyBuilder)
        // HANDLE m_hKeyFile; (to CAssemblyBuilder)
        // HANDLE m_hKeyMap; (to CAssemblyBuilder)

        //------------------------------------------------------------
        // CAssemblyBuilderEx Constructor
        //
        /// <summary></summary>
        /// <param name="cntr"></param>
        //------------------------------------------------------------
        internal CAssemblyBuilderEx(CController cntr)
            : base(cntr)
        {
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.Init (1)
        //
        /// <summary></summary>
        /// <param name="flags"></param>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Init(
            AssemblyFlagsEnum flags,
            CAsmLink linker)
        {
            base.InitBase(linker);
            SetAssemblyFlags(flags);
            return true;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.Init (2)
        //
        /// <summary>
        /// Don't call base.Init
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="error"></param>
        /// <param name="linker"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal override bool Init(FileInfo fi, CAsmLink linker)
        {
            throw new Exception("CAssemblyBuilderEx.Init");
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal override void Clear()
        {
            base.Clear();
            this.assemblyBuilder = null;
            this.assemblyName = null;

            this.manifestModuleBuilder = null;
            //this.cmoduleDictionary.Clear();
            //this.typeBuilderDic.Clear();

            this.isExe = false;
            this.hasPE = false;
            this.peKind = 0;
            this.machine = 0;
            this.hasEmittedPublicTypes = false;
            this.hasEmittedTypeForwarders = false;
            this.isClean = false;
            this.doHash = false;
            this.doDupTypeCheck = false;
            this.doDupeCheckTypeFwds = false;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.CreateModuleBuilderEx
        //
        /// <summary></summary>
        /// <param name="id"></param>
        /// <param name="isManifestModule"></param>
        //------------------------------------------------------------
        internal bool CreateModuleBuilderEx(string id, bool isManifestModule)
        {
            if (String.IsNullOrEmpty(id))
            {
                if (isManifestModule)
                {
                    id = CAssemblyBuilderEx.defaultManifestModuleName;
                }
                else
                {
                    return false;
                }
            }
            if (this.linker.ImportedModuleDic.Count == 0)
            {
                isManifestModule = true;
            }

            CModuleBuilderEx newModule = new CModuleBuilderEx(this.controller);
            DebugUtil.Assert(newModule != null);
            newModule.Init(this.linker);

            if (!this.linker.ImportedModuleDic.Add(id, newModule))
            {
                return false;
            }

            if (isManifestModule)
            {
                this.manifestModuleBuilder = newModule;
            }
            return true;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetAssemblyFlags
        //
        /// <summary></summary>
        /// <param name="flags"></param>
        //------------------------------------------------------------
        private void SetAssemblyFlags(AssemblyFlagsEnum flags)
        {
            this.isClean = ((flags & AssemblyFlagsEnum.CleanModules) != 0);
            this.doHash = ((flags & AssemblyFlagsEnum.NoRefHash) == 0);
            this.doDupTypeCheck = ((flags & AssemblyFlagsEnum.NoDupTypeCheck) == 0);
            this.doDupeCheckTypeFwds =
                (this.doDupTypeCheck || (flags & AssemblyFlagsEnum.DupeCheckTypeFwds) != 0);
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetIsExe
        //
        /// <summary>
        /// If the extension of the assembly file name is ".exe",
        /// set this.IsExe true, otherwise set false.
        /// </summary>
        //------------------------------------------------------------
        private void SetIsExe()
        {
            if (this.fileInfo != null)
            {
                this.isExe =
                    (String.Compare(Path.GetExtension(this.fileInfo.Name), ".exe", true) == 0);
            }
            else
            {
                this.isExe = false;
            }
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetAssemblyFileName
        //
        /// <summary>
        /// Set the assembly file name.
        /// If the extension of argument name is ".exe" or ".dll",
        /// set name to the assembly file name.
        /// Otherwise, add extension ".dll" to name and set it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        //internal bool SetAssemblyFileName(string name)
        //{
        //    string fileName, extName;
        //    try
        //    {
        //        fileName = Path.GetFileName(name);
        //        extName = Path.GetExtension(fileName);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        if (this.metaDataError != null) this.metaDataError.OnError(ex);
        //        return false;
        //    }
        //
        //    if (String.Compare(extName, ".exe", true) == 0 ||
        //        String.Compare(extName, ".dll", true) == 0)
        //    {
        //        this.assemblyFileName = fileName;
        //    }
        //    else
        //    {
        //        this.assemblyFileName = fileName + ".dll";
        //    }
        //    return true;
        //}

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetPEKind
        //
        /// <summary></summary>
        /// <param name="meth"></param>
        //------------------------------------------------------------
        internal bool SetEntryMethod(MethodInfo meth)
        {
            if (this.assemblyBuilder == null || meth == null)
            {
                return false;
            }
            Exception excp = null;

            try
            {
                this.assemblyBuilder.SetEntryPoint(meth);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (System.Security.SecurityException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return false;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetPEKind
        //
        /// <summary></summary>
        /// <param name="peKind"></param>
        /// <param name="machine"></param>
        //------------------------------------------------------------
        internal void SetPEKind(
            PortableExecutableKinds peKind,
            ImageFileMachine machine)
        {
            this.peKind = peKind;
            this.machine = machine;
            this.hasPE = true;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.DefineAssembly
        //
        /// <summary>
        /// <para>Create a System.Reflection.AssemblyName instance and
        /// a Sysmtem.Reflection.Emit.AssemblyBuilder instance.</para>
        /// <para>If name is invalid, use tha default name "__temporaryAssemblyBuilder".</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="access"></param>
        /// <param name="requiredSet"></param>
        /// <param name="optionalSet"></param>
        /// <param name="refusedSet"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool DefineAssembly(
            string asmNameStr,
            FileInfo fInfo,
            AssemblyBuilderAccess access,
            COptionManager options,
            CAssemblyInitialAttributes initalAttrs,
            PermissionSet requiredSet,
            PermissionSet optionalSet,
            PermissionSet refusedSet,
            out Exception excp)
        {
            DebugUtil.Assert(!String.IsNullOrEmpty(asmNameStr));
            //DebugUtil.Assert(fInfo != null && !String.IsNullOrEmpty(fInfo.Name));

            excp = null;
            string asmFileStr = null;

            if (fInfo != null)
            {
                asmFileStr = fInfo.Name;
                base.SetFile(fInfo);
            }
            this.accessFlag = access;

            //--------------------------------------------------
            // Create an AssemblyName instance
            //--------------------------------------------------
            try
            {
                this.assemblyName = new AssemblyName(asmNameStr);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (FileLoadException ex)
            {
                excp = ex;
            }
            if (this.assemblyName == null)
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                this.Clear();
                return false;
            }

            if (initalAttrs != null)
            {
                if (!initalAttrs.SetToAssemblyName(this.assemblyName, options, out excp))
                {
                    return false;
                }
            }

            //--------------------------------------------------
            // Define an AssemblyBuilder instance
            //--------------------------------------------------
            AppDomain domain = Thread.GetDomain();
            try
            {
                this.assemblyBuilder = domain.DefineDynamicAssembly(
                    this.assemblyName,
                    access,
                    requiredSet,
                    optionalSet,
                    refusedSet);
            }
            catch(ArgumentException ex)
            {
                excp = ex;
            }
            catch(AppDomainUnloadedException ex)
            {
                excp = ex;
            }

            if (this.assemblyBuilder == null)
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                this.Clear();
                return false;
            }

            return true;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.CreateModuleBuilderEx
        //
        /// <summary></summary>
        /// <param name="moduleName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CModuleBuilderEx CreateModuleBuilderEx(
            string moduleName,
            FileInfo fInfo)
        {
            DebugUtil.Assert(!String.IsNullOrEmpty(moduleName));
            DebugUtil.Assert(fInfo != null && !String.IsNullOrEmpty(fInfo.Name));

            ModuleBuilder builder = null;
            Exception excp = null;

            try
            {
                builder = this.assemblyBuilder.DefineDynamicModule(
                    moduleName,
                    fInfo.Name);

                CModuleBuilderEx modBuilderEx = new CModuleBuilderEx(this.controller);
                modBuilderEx.Init(this.linker);
                modBuilderEx.SetModuleBuilder(builder);
                modBuilderEx.SetName(moduleName);
                modBuilderEx.SetFile(fileInfo);
                modBuilderEx.SetAssemblyBuilderEx(this);

                this.linker.ImportedModuleDic.Add(modBuilderEx.Name, modBuilderEx);
                return modBuilderEx;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (SecurityException ex)
            {
                excp = ex;
            }
            catch (ExecutionEngineException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.CreateManifestModuleBuilderEx
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CModuleBuilderEx CreateManifestModuleBuilderEx()
        {
            this.manifestModuleBuilder = CreateModuleBuilderEx(
                this.fileInfo.Name,
                this.fileInfo);
            return this.manifestModuleBuilder;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SignAssembly
        //
        /// <summary>
        /// <para>Under construction, return true only.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SignAssembly()
        {
            // Under construction.
            return true;
        }

        //------------------------------------------------------------
        // CAssembly::SignAssembly (ssclil)
        //------------------------------------------------------------
        //HRESULT CAssembly::SignAssembly()
        //{
        //    HRESULT hr = S_FALSE;
        //
        //    if (!m_doHalfSign && (m_pbKeyPair != NULL || (m_KeyName && *m_KeyName && !m_isAutoKeyName))){
        //        PAL_TRY {
        //            if (IsInMemory()) {
        //                VSFAIL("You can't fully sign an InMemory assembly!");
        //                hr = E_INVALIDARG;
        //            } else if (m_pbKeyPair == NULL || SUCCEEDED(hr = ReadCryptoFile())) {
        //                if (FALSE != StrongNameSignatureGeneration( m_Path, m_KeyName,
        //                    m_pbKeyPair, m_cbKeyPair, NULL, NULL)) {
        //                    hr = S_OK;
        //                    ASSERT(StrongNameSignatureVerification(
        //                        m_Path, SN_INFLAG_FORCE_VER | SN_INFLAG_INSTALL | SN_INFLAG_ALL_ACCESS, NULL));
        //                } else {
        //                    DWORD err = StrongNameErrorInfo();
        //                    if (!FAILED(err)) err = HRESULT_FROM_WIN32(err);
        //                    hr = ReportError(err);
        //                }
        //            }
        //        } PAL_FINALLY {
        //            HRESULT hr2 = ClearCryptoFile();
        //            if (SUCCEEDED(hr) && FAILED(hr2))
        //                hr = hr2;
        //        } PAL_ENDTRY
        //    }
        //
        //    return hr;
        //}

        //------------------------------------------------------------
        // CAssemblyBuilderEx.DefineModule
        //
        /// <summary></summary>
        /// <param name="scopeName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal ModuleBuilder DefineModule(string scopeName, string fileName)
        {
            DebugUtil.Assert(this.assemblyBuilder != null);
            Exception excp = null;

            try
            {
                return this.assemblyBuilder.DefineDynamicModule(scopeName, fileName);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (SecurityException ex)
            {
                excp = ex;
            }
            catch (ExecutionEngineException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return null;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.LoadModule
        //
        /// <summary>
        /// Load an image of module file and create CModuleEx with the image.
        /// ModuleBuilder is not created yet.
        /// </summary>
        /// <param name="scopeName"></param>
        /// <param name="srcFi"></param>
        /// <param name="dstFi"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CModuleEx LoadModule(
            string scopeName,
            FileInfo srcFi,
            FileInfo dstFi)
        {
            if (this.assemblyBuilder == null)
            {
                return null;
            }
            if (srcFi == null || String.IsNullOrEmpty(srcFi.Name))
            {
                return null;
            }

            CModuleEx cmod = new CModuleEx(controller);
            if (dstFi == null || String.IsNullOrEmpty(dstFi.Name))
            {
                dstFi = cmod.CreateModuleFileInfo(srcFi);
            }
            if (String.IsNullOrEmpty(scopeName))
            {
                scopeName = dstFi.Name;
            }
            cmod.SetName(scopeName);
            cmod.Init(dstFi, this.linker);
            if (cmod.LoadImage(srcFi))
            {
                return cmod;
            }
            return null;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.AddModule
        //
        /// <summary>
        /// <para>Create a CModule instance of this.assemblyBuilder from a given INFILESYM data.
        /// Set it to INFILESYM.MetadataFile.</para>
        /// <para>If the CModule instance for infileSym has already been created, return it.</para>
        /// <para>If not created.
        /// <list type="bullet">
        /// <item>Read the binary image of the module.</item>
        /// <item>Load the module by AssemblyBuilder.LoadModule method with its image.</item>
        /// <item>Create a CModule instance and set the module and its image to it.</item>
        /// <item>Register it to this.cmoduleDictionary.</item>
        /// <item>Register all types in the module to typeDictionary or internalTypeDictionary.</item>
        /// </list>
        /// </para>
        /// </summary>
        //------------------------------------------------------------
        internal CModuleEx AddModule(
            INFILESYM infileSym,
            string scopeName,
            string newFileName,
            bool recreate)
        {
            DebugUtil.Assert(this.assemblyBuilder != null);
            if (infileSym == null || !infileSym.IsModule)
            {
                return null;
            }

            FileInfo srcFileInfo = infileSym.FileInfo;
            FileInfo dstFileInfo = null;

            if (srcFileInfo == null ||
                String.IsNullOrEmpty(srcFileInfo.Name))
            {
                return null;
            }

            CModuleEx cmod = null;
            string searchName = null;
            //string fileName = null;
            Exception excp = null;

            // Modules of a multifile assembly must be in the same directory,
            if (String.IsNullOrEmpty(newFileName))
            {
                newFileName = infileSym.FileInfo.Name;
            }
            else
            {
                newFileName = Path.GetFileName(newFileName);
            }

            if (String.IsNullOrEmpty(scopeName))
            {
                //scopeName = Path.GetFileNameWithoutExtension(newFileName);
                scopeName = newFileName;
            }
            searchName = newFileName.ToLower();

            if (!IOUtil.CreateFileInfo(newFileName, out dstFileInfo, out excp))
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
                return null;
            }

            lock (this.lockObject)
            {
                try
                {
                    // if key not found, cmoduleDictionary returns null.
                    cmod = this.linker.ImportedModuleDic[searchName];
                }
                catch (ArgumentException ex)
                {
                    this.controller.ReportError(ERRORKIND.ERROR, ex);
                    return null;
                }

                if (cmod != null)
                {
                    if (recreate == false)
                    {
                        goto SET_INFILESYM;
                    }
                    this.linker.ImportedModuleDic.Remove(searchName);
                }

                //----------------------------------------------------
                // Load the module image and create CModuleBuilderEx
                //----------------------------------------------------
                cmod = this.LoadModule(scopeName, srcFileInfo, dstFileInfo);
                if (cmod == null)
                {
                    return null;
                }

                //----------------------------------------------------
                // Create the ModuleBuilder and set the image to it.
                //----------------------------------------------------
                if (!cmod.SetImageToAssemblyBuilder(this, false))
                {
                    return null;
                }

                cmod.ReadCustomAttributes(true);
                //cmod.AddTypes(true);
                this.linker.ImportedModuleDic.Add(searchName, cmod);

            SET_INFILESYM:
                infileSym.SetMetadataFile(cmod);
                return cmod;
            }
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.EmitInternalTypes
        //
        // HRESULT EmitInternalTypes(); // to CAssemblyBuilder
        //------------------------------------------------------------
        internal bool EmitInternalTypes()
        {
            throw new NotImplementedException("CAssemblyBuilderEx.EmitInternalTypes");
        }

        //------------------------------------------------------------
        // CAssemblyBuilder::EmitInternalTypes (sscli)
        //------------------------------------------------------------
        //HRESULT CAssembly::EmitInternalTypes()
        //{
        //    HRESULT hr = S_FALSE;
        //    for (DWORD i = 0, l = m_Files.Count(); i < l; i++) {
        //        CFile *temp = m_Files.GetAt(i);
        //        if (temp->GetImportScope()) {
        //            // Emit the internal types to the ExportedType table now that we know
        //            if (FAILED(hr = temp->EmitComTypes( this, true)))
        //                break;
        //        }
        //    }
        //
        //    return hr;
        //}

        //------------------------------------------------------------
        // CAssemblyBuilderEx.GetAssemblyNameWithoutExtension
        //
        /// <summary>
        /// If a file has an extension ".exe" or ".dll", remove it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal string GetAssemblyNameWithoutExtension(string name)
        {
            if (String.IsNullOrEmpty(name)) return name;
            string[] extensions = { ".exe", ".dll" };
            foreach (string ex in extensions)
            {
                if (name.Length <= ex.Length) continue;
                int dotIndex = name.Length - ex.Length;
                if (dotIndex != name.LastIndexOf('.')) continue;
                if (String.Compare(name, dotIndex, ex, 0, 4, true) == 0)
                {
                    return name.Substring(0, name.Length - 4);
                }
            }
            return name;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetCustomAttribute (1)
        //
        //------------------------------------------------------------
        internal bool SetCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (this.assemblyBuilder == null || caBuilder == null)
            {
                return false;
            }
            Exception excp = null;

            try
            {
                this.assemblyBuilder.SetCustomAttribute(caBuilder);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (System.Security.SecurityException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return false;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetCustomAttribute (2)
        //
        /// <summary>
        /// Set a custom attribute to this assembly.
        /// </summary>
        /// <param name="caConstInfo"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetCustomAttribute(
            ConstructorInfo cInfo,
            params object[] args)
        {
            DebugUtil.Assert(this.assemblyBuilder != null);
            DebugUtil.Assert(cInfo != null);

            CustomAttributeBuilder caBuilder = null;
            Exception excp = null;
            try
            {
                caBuilder = new CustomAttributeBuilder(cInfo, args);
                return this.SetCustomAttribute(caBuilder);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return false;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.CreateGlobalFunctions
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void CreateGlobalFunctions()
        {
            if (this.manifestModuleBuilder != null)
            {
                this.manifestModuleBuilder.CreateGlobalFunctions();
            }
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.SetFieldOffset
        //
        /// <summary>
        /// Do nothing for now.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool SetFieldOffset(FieldBuilder builder, int offset)
        {
            return true;
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.GetMetadataToken
        //
        /// <summary></summary>
        /// <param name="methInfo"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetMetadataToken(MethodInfo methInfo, out int token)
        {
            DebugUtil.Assert(this.manifestModuleBuilder != null);
            return this.manifestModuleBuilder.GetMetadataToken(methInfo, out token);
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.AddResource
        //
        /// <summary></summary>
        /// <param name="resourceSym"></param>
        //------------------------------------------------------------
        internal void AddResource(RESFILESYM resourceSym)
        {
            DebugUtil.Assert(this.manifestModuleBuilder != null);

            if (resourceSym == null)
            {
                return;
            }

            FileInfo resFileInfo = resourceSym.FileInfo;
            if (resFileInfo == null || !resFileInfo.Exists)
            {
                return;
            }

            if (resourceSym.IsEmbedded)
            {
                if (this.manifestModuleBuilder != null)
                {
                    this.manifestModuleBuilder.DefineEmbeddedResources(resourceSym);
                }
                return;
            }

            string logicalName = String.IsNullOrEmpty(resourceSym.LogicalName)
                ? resFileInfo.Name
                : resourceSym.LogicalName;
            Exception excp = null;

            try
            {
                this.assemblyBuilder.AddResourceFile(
                    logicalName,
                    resFileInfo.Name,
                    resourceSym.Accessibility);
                return;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (SecurityException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.DefineUnmanagedEmbeddedResources
        //
        /// <summary></summary>
        /// <param name="resFileInfo"></param>
        //------------------------------------------------------------
        internal void DefineUnmanagedEmbeddedResources(FileInfo resFileInfo)
        {
            if (resFileInfo == null ||
                String.IsNullOrEmpty(resFileInfo.Name) ||
                this.assemblyBuilder == null)
            {
                return;
            }
            if (!resFileInfo.Exists)
            {
                this.controller.ReportError(
                    CSCERRID.ERR_CantOpenWin32Res,
                    ERRORKIND.ERROR,
                    resFileInfo.Name,
                    "not found.");
                return;
            }

            Exception excp = null;
            byte[] resImage = null;

            if (!IOUtil.ReadBinaryFile(resFileInfo.FullName, out resImage, out excp))
            {
                this.controller.ReportError(ERRORKIND.ERROR, excp);
            }

            try
            {
                this.assemblyBuilder.DefineUnmanagedResource(resImage);
            }
            catch (ArgumentException ex)
            {
                this.controller.ReportError(ERRORKIND.ERROR, ex);
            }
            catch (System.Security.SecurityException ex)
            {
                this.controller.ReportError(ERRORKIND.ERROR, ex);
            }
        }

        //------------------------------------------------------------
        // CAssemblyBuilderEx.Save
        //
        /// <summary>
        /// Save the assembly.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool Save()
        {
            if (this.assemblyBuilder == null)
            {
                return false;
            }
            Exception excp = null;

            try
            {
                this.CreateGlobalFunctions();
                this.assemblyBuilder.Save(this.FileName, this.peKind, this.machine);

                if (this.linker != null)
                {
                    this.linker.SaveModuleImages();
                }
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            catch (IOException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }

            this.controller.ReportError(ERRORKIND.ERROR, excp);
            return false;
        }
    }
}
