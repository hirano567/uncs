//============================================================================
// ReflectionUtil.cs
//
// 2015/09/03 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    internal static partial class ReflectionUtil
    {
        //internal static Type typeOfDelegate = typeof(System.Delegate);
        internal static Type typeOfAttribute = typeof(System.Attribute);
        internal static Type typeofSecurityCodeAccessPermission
            = typeof(System.Security.CodeAccessPermission);

        //------------------------------------------------------------
        // ReflectionUtil.CreateVersion
        //
        /// <summary></summary>
        /// <param name="versionStr"></param>
        /// <param name="versionObj"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool CreateVersion(
            string versionStr,
            out Version versionObj,
            out Exception excp)
        {
            versionObj = null;
            excp = null;

            try
            {
                versionObj = new Version(versionStr);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (FormatException ex)
            {
                excp = ex;
            }
            catch (OverflowException ex)
            {
                excp = ex;
            }
            return false;
        }

        //------------------------------------------------------------
        // ReflectionUtil.IsAllowMultiple
        //
        /// <summary>
        /// </summary>
        //------------------------------------------------------------
        internal static bool IsAllowMultiple(Type attrType)
        {
            if (attrType == null || !attrType.IsSubclassOf(typeof(System.Attribute))) return false;
            object[] cas = attrType.GetCustomAttributes(true);
            foreach (object obj in cas)
            {
                AttributeUsageAttribute usage = obj as AttributeUsageAttribute;
                if (usage != null)
                {
                    return usage.AllowMultiple;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // ReflectionUtil.ComputeGenericArityFromName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static int ComputeGenericArityFromName(string name)
        {
            int idx;
            int arity = 0;
            try
            {
                idx = name.LastIndexOf('`');
            }
            catch (ArgumentException)
            {
                return 0;
            }
            if (idx < 0)
            {
                return 0;
            }

            string arityStr = name.Substring(idx + 1);
            if (!Int32.TryParse(arityStr, out arity))
            {
                return 0;
            }
            if ((0x0000FFFF | arity) == 0x0000FFFF)
            {
                return arity;
            }
            return 0;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetFullNameOfType
        //
        /// <summary>
        /// <para>Join Type.Namespace and Type.Name by ".".
        /// This creates a name consisting of namespace, type name and arity.</para>
        /// <para>Type.FullName may be null, or may has the information of instanciation.</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string GetFullNameOfType(Type type)
        {
            DebugUtil.Assert(type != null);
            return String.Format("{0}.{1}", type.Namespace, type.Name);
        }

        //------------------------------------------------------------
        // ReflectionUtil.ParseCLSFullName
        //
        /// <summary>
        /// <para>Parse a full name of a common library object.
        /// Return its name with arity and set type argument names to typeArgNames</para>
        /// <para>We assume that fullName is of the form below:
        ///   name-with-arity "[" "[" type-argument-name "," assembly-full-name "]" ... "]"
        /// </para>
        /// <para>If no type argument, set a empty list to typeArgNames, not null.</para>
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="typeArgNames"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string ParseCLSFullName(string fullName, out List<string> typeArgNames)
        {
            typeArgNames = new List<string>();
            if (String.IsNullOrEmpty(fullName))
            {
                return fullName;
            }

            int length = fullName.Length;
            int index = fullName.IndexOf('[');
            DebugUtil.Assert(index != 0, "No character before [");
            if (index < 0)
            {
                return fullName;
            }
            string name = fullName.Substring(0, index);

            ++index;
            while (index < length && fullName[index] != '[')
            {
                ++index;
            }

            while (index < length && fullName[index] != ']')
            {
                int start = index;
                GetBracketedRange(fullName, ref index);
                DebugUtil.Assert(start < index);
                ++start;

                int pos = fullName.IndexOfAny(parseCLSFullName_Delims, start);
                // parseCLSFullName_Delims is after this method.
                DebugUtil.Assert(pos != 0);

                if (pos < 0)
                {
                    typeArgNames.Add(fullName.Substring(start, index - start));
                }
                else if (fullName[pos] == ',')
                {
                    typeArgNames.Add(fullName.Substring(start, pos-start));
                }
                else
                {
                    GetBracketedRange(fullName, ref pos);
                    if (pos < fullName.Length) ++pos;
                    typeArgNames.Add(fullName.Substring(start, pos - start));
                }

                while (index < length && fullName[index] != '[')
                {
                    ++index;
                }
            }

            return name;
        }

        static private char[] parseCLSFullName_Delims = { ',', '[' };

        //------------------------------------------------------------
        // ReflectionUtil.CreateTypeSearchName
        //
        /// <summary>
        /// Create a type name with the names of namespaces and arity.
        /// </summary>
        /// <param name="nsSym"></param>
        /// <param name="name"></param>
        /// <param name="arity"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string CreateTypeSearchName(NSSYM nsSym, string name, int arity)
        {
            DebugUtil.Assert(!String.IsNullOrEmpty(name));

            StringBuilder sb = new StringBuilder();
            Stack<string> nameStack = new Stack<string>();

            nameStack.Push(null);
            while (nsSym != null)
            {
                nameStack.Push(nsSym.Name);
                nsSym = nsSym.ParentNsSym;
            }

            string temp = null;
            while ((temp = nameStack.Pop()) != null)
            {
                sb.Append(temp);
                if (sb.Length > 0) sb.Append('.');
            }

            sb.Append(name);

            if (arity > 0 && name.IndexOf('`') < 0)
            {
                sb.AppendFormat("`{0}", arity);
            }

            return sb.ToString();
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetBracketedRange
        //
        /// <summary>
        /// <para>Return the substring between "[" and "]" (coupling "[" and "]").</para>
        /// <para>pos must be the index of "[",
        /// and shall be set the index of "]" or the end of source.</para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static void GetBracketedRange(string source, ref int index)
        {
            DebugUtil.Assert(index < source.Length - 1);
            DebugUtil.Assert(source[index] == '[');

            int start = ++index;
            int depth = 0;

            while (index < source.Length)
            {
                if (source[index] == ']' && depth == 0)
                {
                    break;
                }
                if (source[index] == '[')
                {
                    ++depth;
                }
                else if (source[index] == ']')
                {
                    --depth;
                }
                ++index;
            }
        }

        //------------------------------------------------------------
        // ReflectionUtil.CreateInstanciatedName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="typeNames"></param>
        /// <param name="typeCount"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string CreateInstanciatedName(
            string name,
            IList<string> typeNames,
            int typeCount)
        {
            StringBuilder sb = new StringBuilder();
            if (name != null)
            {
                sb.Append(name);
            }
            if (typeNames != null && typeCount > 0)
            {
                sb.Append('[');
                for (int i = 0; i < typeCount; ++i)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    try
                    {
                        sb.AppendFormat("[{0}]", typeNames[i]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        sb.Append("[]");
                    }
                    catch (ArgumentException)
                    {
                        // ArgumentNullException, ArgumentOutOfRangeException
                        sb.Append("[]");
                    }
                    catch (FormatException)
                    {
                        sb.Append("[]");
                    }
                }
                sb.Append(']');
            }
            return sb.ToString();
        }

        //------------------------------------------------------------
        // ReflectionUtil.CreateCustomAttributeBuilder (1)
        //
        /// <summary></summary>
        /// <param name="cInfo"></param>
        /// <param name="posArgs"></param>
        /// <param name="propInfos"></param>
        /// <param name="propValues"></param>
        /// <param name="fieldInfos"></param>
        /// <param name="fieldValues"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static CustomAttributeBuilder CreateCustomAttributeBuilder(
            ConstructorInfo cInfo,
            Object[] posArgs,
            PropertyInfo[] propInfos,
            Object[] propValues,
            FieldInfo[] fieldInfos,
            Object[] fieldValues)
        {
            DebugUtil.Assert(cInfo != null);
            if (posArgs == null) posArgs = new Object[0];
            if (propInfos == null) propInfos = new PropertyInfo[0];
            if (propValues == null) propValues = new Object[0];
            if (fieldInfos == null) fieldInfos = new FieldInfo[0];
            if (fieldValues == null) fieldValues = new Object[0];

            try
            {
                return new CustomAttributeBuilder(
                    cInfo,
                    posArgs,
                    propInfos,
                    propValues,
                    fieldInfos,
                    fieldValues);
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.CreateCustomAttributeBuilder (2)
        //
        /// <summary></summary>
        /// <param name="cInfo"></param>
        /// <param name="posArgs"></param>
        /// <param name="propInfos"></param>
        /// <param name="propValues"></param>
        /// <param name="fieldInfos"></param>
        /// <param name="fieldValues"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static CustomAttributeBuilder CreateCustomAttributeBuilder(
            ConstructorInfo cInfo,
            List<Object> posArgs,
            List<PropertyInfo> propInfos,
            List<Object> propValues,
            List<FieldInfo> fieldInfos,
            List<Object> fieldValues)
        {
            if (cInfo == null) return null;
            Object[] posArgsArray;
            PropertyInfo[] propInfoArray;
            Object[] propValueArray;
            FieldInfo[] fieldInfoArray;
            Object[] fieldValueArray;

            if (posArgs != null&&posArgs.Count>0)
            {
                posArgsArray = new Object[posArgs.Count];
                posArgs.CopyTo(posArgsArray);
            }
            else
            {
                posArgsArray = new Object[0];
            }

            if (propInfos != null && propInfos.Count > 0)
            {
                propInfoArray = new PropertyInfo[propInfos.Count];
                propInfos.CopyTo(propInfoArray);
            }
            else
            {
                propInfoArray = new PropertyInfo[0];
            }

            if (propValues != null && propValues.Count > 0)
            {
                propValueArray = new Object[propValues.Count];
                propValues.CopyTo(propValueArray);
            }
            else
            {
                propValueArray = new Object[0];
            }

            if (fieldInfos != null && fieldInfos.Count > 0)
            {
                fieldInfoArray = new FieldInfo[fieldInfos.Count];
                fieldInfos.CopyTo(fieldInfoArray);
            }
            else
            {
                fieldInfoArray = new FieldInfo[0];
            }

            if (fieldValues != null && fieldValues.Count > 0)
            {
                fieldValueArray = new Object[fieldValues.Count];
                fieldValues.CopyTo(fieldValueArray);
            }
            else
            {
                fieldValueArray = new Object[0];
            }

            return CreateCustomAttributeBuilder(
                cInfo,
                posArgsArray,
                propInfoArray,
                propValueArray,
                fieldInfoArray,
                fieldValueArray);
        }

        //------------------------------------------------------------
        // ReflectionUtil.IsTypeBuilder
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsTypeBuilder(Type type)
        {
            return (type is TypeBuilder || type is GenericTypeParameterBuilder);
        }

        //------------------------------------------------------------
        // ReflectionUtil.IsTypeBuilderIn
        //
        /// <summary></summary>
        /// <param name="types"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsTypeBuilderIn(Type[] types)
        {
            int len = 0;
            if (types == null || (len = types.Length) == 0)
            {
                return false;
            }

            for (int i = 0; i < len; ++i)
            {
                if (IsTypeBuilder(types[i]))
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetMethodInfo (1)
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="paramTypes"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static MethodInfo GetMethodInfo(
            Type type,
            string name,
            Type[] paramTypes,
            out Exception excp)
        {
            excp = null;
            try
            {
                return type.GetMethod(
                    name,
                    paramTypes);
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (AmbiguousMatchException ex)
            {
                excp = ex;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetMethodInfo (2)
        //
        /// <summary>
        /// Get a MethodInfo from a TypeBuilderInstration.
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="constructedType"></param>
        /// <param name="name"></param>
        /// <param name="paramTypes"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static MethodInfo GetMethodInfo(
            Type constructedType,
            MethodInfo methInfo,
            out Exception excp)
        {
            MethodInfo defInfo = null;
            excp = null;

            if (!methInfo.IsGenericMethod ||
                methInfo.IsGenericMethodDefinition)
            {
                defInfo = methInfo;
            }
            else
            {
                defInfo = methInfo.GetGenericMethodDefinition();
            }
            DebugUtil.Assert(defInfo != null);

            try
            {
                return TypeBuilder.GetMethod(constructedType, defInfo);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetConstructorInfo (1)
        //
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="paramTypes"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static ConstructorInfo GetConstructorInfo(
            Type type,
            Type[] paramTypes,
            out Exception excp)
        {
            excp = null;

            try
            {
                return type.GetConstructor(paramTypes);
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (AmbiguousMatchException ex)
            {
                excp = ex;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetConstructorInfo (2)
        //
        /// <summary>
        /// Get a ConstructorInfo from a TypeBuilderInstration.
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="constructedType"></param>
        /// <param name="paramTypes"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static ConstructorInfo GetConstructorInfo(
            Type constructedType,
            ConstructorInfo cnstInfo,
            out Exception excp)
        {
            excp = null;

            try
            {
                return TypeBuilder.GetConstructor(constructedType, cnstInfo);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetConstructedConstructorInfo
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="methodSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static ConstructorInfo GetConstructedConstructorInfo(
            COMPILER compiler,
            METHSYM methodSym,
            AGGTYPESYM aggTypeSym)
        {
            DebugUtil.Assert(compiler != null && compiler.Emitter != null);
            EMITTER emitter = compiler.Emitter;

            DebugUtil.Assert(emitter != null && methodSym != null && aggTypeSym != null);

            SymUtil.EmitParentSym(emitter, aggTypeSym);
            SymUtil.GetSystemTypeFromSym(aggTypeSym, null, null);
            emitter.EmitMethodDef(methodSym);

            Type parentType = aggTypeSym.Type;
            ConstructorInfo cnstrDefInfo = methodSym.ConstructorInfo;
            DebugUtil.Assert(cnstrDefInfo != null);

            bool isGenericType = parentType.IsGenericType;

            //--------------------------------------------------------
            // (1) Non-generic Type
            //--------------------------------------------------------
            if (!isGenericType)
            {
                return cnstrDefInfo;
            }

            //--------------------------------------------------------
            // (2) Generic Type
            //--------------------------------------------------------
            TypeArray paramTypeArray = methodSym.ParameterTypes;
            TypeArray paramTypeArray2 = null;
            Type[] paramTypes = null;

            if (paramTypeArray != null && paramTypeArray.Count > 0)
            {
                paramTypeArray2 = compiler.MainSymbolManager.SubstTypeArray(
                    paramTypeArray,
                    aggTypeSym.AllTypeArguments,
                    null,
                    SubstTypeFlagsEnum.NormNone);

                paramTypes = SymUtil.GetSystemTypesFromTypeArray(
                    paramTypeArray2,
                    methodSym.ClassSym,
                    methodSym);
            }
            else
            {
                paramTypes = Type.EmptyTypes;
            }

            ConstructorInfo constructedInfo = null;
            Exception ex = null;

            try
            {
                constructedInfo = parentType.GetConstructor(paramTypes);
            }
            catch (NotSupportedException)
            {
                constructedInfo = GetConstructorInfo(parentType, cnstrDefInfo, out ex);
            }
            return constructedInfo;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetFieldInfo (1)
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static FieldInfo GetFieldInfo(
            Type type,
            string name,
            out Exception excp)
        {
            excp = null;
            try
            {
                return type.GetField(name);
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetFieldInfo (2)
        //
        /// <summary></summary>
        /// <param name="constructedType"></param>
        /// <param name="fldInfo"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static FieldInfo GetFieldInfo(
            Type constructedType,
            FieldInfo fldInfo,
            out Exception excp)
        {
            excp = null;
            try
            {
                return TypeBuilder.GetField(constructedType, fldInfo);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetConstructedFieldInfo
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="fieldSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static FieldInfo GetConstructedFieldInfo(
            COMPILER compiler,
            MEMBVARSYM fieldSym,
            AGGTYPESYM aggTypeSym)
        {
            DebugUtil.Assert(compiler != null && compiler.Emitter != null);
            EMITTER emitter = compiler.Emitter;

            DebugUtil.Assert(emitter != null && fieldSym != null && aggTypeSym != null);

            SymUtil.EmitParentSym(emitter, aggTypeSym);
            SymUtil.GetSystemTypeFromSym(aggTypeSym, null, null);
            emitter.EmitMembVarDef(fieldSym);

            Type parentType = aggTypeSym.Type;
            FieldInfo fieldDefInfo = fieldSym.FieldInfo;
            Exception ex = null;

            if (!parentType.IsGenericType)
            {
                return fieldDefInfo;
            }

            FieldInfo cstrFieldInfo = null;
            try
            {
                cstrFieldInfo = parentType.GetField(fieldDefInfo.Name);
            }
            catch(NotSupportedException)
            {
                cstrFieldInfo = ReflectionUtil.GetFieldInfo(
                    parentType,
                    fieldDefInfo,
                    out ex);
            }
            return cstrFieldInfo;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetGenericType
        //
        /// <summary>
        /// Call Type.MakeGenericType. Catch exceptions.
        /// </summary>
        /// <param name="methInfo"></param>
        /// <param name="typeArguments"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
            //------------------------------------------------------------
        internal static Type GetGenericType(
            Type type,
            Type[] typeArguments,
            out Exception excp)
        {
            excp = null;
            try
            {
                return type.MakeGenericType(typeArguments);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetGenericMethod
        //
        /// <summary>
        /// Call MethodInfo.MakeGenericMethod. Catch exceptions.
        /// </summary>
        /// <param name="methInfo"></param>
        /// <param name="typeArguments"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static MethodInfo GetGenericMethod(
            MethodInfo methInfo,
            Type[] typeArguments,
            out Exception excp)
        {
            excp = null;
            try
            {
                return methInfo.MakeGenericMethod(typeArguments);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.SubstTypes
        //
        /// <summary></summary>
        /// <param name="srcTypes"></param>
        /// <param name="typeGenericArguments"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static Type[] SubstTypes(
            Type[] srcTypes,
            Type[] typeGenericArguments)
        {
            if (srcTypes == null || srcTypes.Length == 0)
            {
                return new Type[] { };
            }

            Type[] substTypes = new Type[srcTypes.Length];
            for (int i = 0; i < srcTypes.Length; ++i)
            {
                Type currentType = srcTypes[i];
                if (!currentType.IsGenericParameter)
                {
                    substTypes[i] = currentType;
                    continue;
                }

                int index = currentType.GenericParameterPosition;

                if (typeGenericArguments != null &&
                    index < typeGenericArguments.Length &&
                    0 <= index)
                {
                    substTypes[i] = typeGenericArguments[index];
                }
                else
                {
                    substTypes[i] = currentType;
                }
            }
            return substTypes;
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetConstructedMethodInfo
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="methodSym"></param>
        /// <param name="aggTypeSym"></param>
        /// <param name="methTypeArguments"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static MethodInfo GetConstructedMethodInfo(
            COMPILER compiler,
            METHSYM methodSym,
            AGGTYPESYM aggTypeSym,
            TypeArray methTypeArguments)
        {
            DebugUtil.Assert(compiler != null && compiler.Emitter != null);
            EMITTER emitter = compiler.Emitter;

            DebugUtil.Assert(emitter != null && methodSym != null && aggTypeSym != null);

            SymUtil.EmitParentSym(emitter, aggTypeSym);
            SymUtil.GetSystemTypeFromSym(aggTypeSym, null, null);
            emitter.EmitMethodDef(methodSym);

            Type parentType = aggTypeSym.Type;
            MethodInfo methodDefInfo = methodSym.MethodInfo;

            bool isGenericType = parentType.IsGenericType;
            bool isGenericMethod = methodDefInfo.IsGenericMethod;

            //--------------------------------------------------------
            // (1-1) Non-generic Type, non-generic method
            //--------------------------------------------------------
            if (!isGenericType && !isGenericMethod)
            {
                return methodDefInfo;
            }

            //--------------------------------------------------------
            // Parameter types
            //--------------------------------------------------------
            TypeArray paramTypeArray = methodSym.ParameterTypes;
            TypeArray paramTypeArray2 = null;
            Type[] paramTypes = null;

            if (paramTypeArray != null && paramTypeArray.Count > 0)
            {
                paramTypeArray2 = compiler.MainSymbolManager.SubstTypeArray(
                    paramTypeArray,
                    aggTypeSym.AllTypeArguments,
                    methTypeArguments,
                    SubstTypeFlagsEnum.NormNone);

                paramTypes = SymUtil.GetSystemTypesFromTypeArray(
                    paramTypeArray2,
                    methodSym.ClassSym,
                    methodSym);
            }
            else
            {
                paramTypes = Type.EmptyTypes;
            }

            //--------------------------------------------------------
            // (1-2) Non-generic Type, generic method
            //--------------------------------------------------------
            if (!isGenericType)
            {
                DebugUtil.Assert(methTypeArguments != null);

                Exception ex = null;
                Type[] typeArgs = SymUtil.GetSystemTypesFromTypeArray(
                    methTypeArguments,
                    methodSym.ClassSym,
                    methodSym);
                return ReflectionUtil.GetGenericMethod(
                    methodDefInfo,
                    typeArgs,
                    out ex);
            }
            //--------------------------------------------------------
            // (2) Generic Type
            //--------------------------------------------------------
            else
            {
                string methodName = methodDefInfo.Name;
                MethodInfo cstrMethInfo = null;
                Exception ex = null;

                try
                {
                    cstrMethInfo = parentType.GetMethod(methodName, paramTypes);
                }
                catch(NotSupportedException)
                {
                    cstrMethInfo = GetMethodInfo(parentType, methodDefInfo, out ex);
                }

                if (!isGenericMethod)
                {
                    return cstrMethInfo;
                }

                DebugUtil.Assert(methTypeArguments != null);
                Type[] typeArgs = SymUtil.GetSystemTypesFromTypeArray(
                    methTypeArguments,
                    methodSym.ClassSym,
                    methodSym);

                return ReflectionUtil.GetGenericMethod(
                    cstrMethInfo,
                    typeArgs,
                    out ex);
            }
        }

        //------------------------------------------------------------
        // ReflectionUtil.GetConstructedMethodInfo2 (obsolete)
        //
        /// <summary></summary>
        /// <param name="methInfo"></param>
        /// <param name="aggTypeSym"></param>
        /// <param name="methTypeArguments"></param>
        /// <returns></returns>
            //------------------------------------------------------------
        internal static MethodInfo GetConstructedMethodInfo2(
            MethodInfo methInfo,
            AGGTYPESYM aggTypeSym,
            TypeArray methTypeArguments)
        {
            DebugUtil.Assert(methInfo != null);

            bool isGenericType = false;
            Type type = null;
            MethodInfo methInfo2 = null;

            if (aggTypeSym != null)
            {
                type = aggTypeSym.GetConstructedType(null, null, false);
                if (type != null && type.IsGenericType)
                {
                    isGenericType = true;
                }
            }

            if (!methInfo.IsGenericMethod && !isGenericType)
            {
                return methInfo;
            }

            if (isGenericType)
            {
                string name = methInfo.Name;
                Type[] paramTypes = SubstParameterTypes(
                    methInfo.GetParameters(),
                    type.GetGenericArguments());

                try
                {
                    methInfo2 = type.GetMethod(
                        name,
                        paramTypes);
                }
                catch (NotSupportedException)
                {
                    methInfo2 = System.Reflection.Emit.TypeBuilder.GetMethod(
                        type,
                        methInfo);
                }
            }
            else
            {
                methInfo2 = methInfo;
            }

            if (methInfo.IsGenericMethod)
            {
                throw new NotImplementedException("");
            }

            return methInfo2;
        }

        //------------------------------------------------------------
        // ReflectionUtil.SubstParameterTypes
        //
        /// <summary></summary>
        /// <param name="paramInfos"></param>
        /// <param name="typeGenericArguments"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static Type[] SubstParameterTypes(
            ParameterInfo[] paramInfos,
            Type[] typeGenericArguments)
        {
            if (paramInfos == null || paramInfos.Length == 0)
            {
                return new Type[] { };
            }

            Type[] paramTypes = null;
            Convert(paramInfos, out paramTypes);

            return SubstTypes(
                paramTypes,
                typeGenericArguments);
        }

        //------------------------------------------------------------
        // ReflectionUtil.Convert ParameterInfo[] -> Type[]
        //
        /// <summary></summary>
        /// <param name="pis"></param>
        /// <param name="types"></param>
        //------------------------------------------------------------
        internal static void Convert(ParameterInfo[] pis, out Type[] types)
        {
            int size = (pis != null ? pis.Length : 0);
            types = new Type[size];

            if (size > 0)
            {
                for (int i = 0; i < size; ++i)
                {
                    types[i] = pis[i].ParameterType;
                }
            }
        }

        //------------------------------------------------------------
        // ReflectionUtil.IsTypeBuilderInstruction
        //
        /// <summary></summary>
        /// <param name="typeSym"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsTypeBuilderInstruction(TYPESYM typeSym)
        {
            if (typeSym == null)
            {
                return false;
            }
            Type type = null;
            int count = 0;
            int index = 0;

            switch (typeSym.Kind)
            {
                //----------------------------------------------------
                // AGGTYPESYM
                //----------------------------------------------------
                case SYMKIND.AGGTYPESYM:
                    AGGTYPESYM ats = typeSym as AGGTYPESYM;
                    DebugUtil.Assert(ats != null);
                    if (ats.AllTypeArguments == null ||
                        ats.AllTypeArguments.Count == 0)
                    {
                        return false;
                    }

                    AGGSYM aggSym = ats.GetAggregate();
                    DebugUtil.Assert(aggSym != null);
                    if (aggSym.TypeBuilder != null)
                    {
                        return true;
                    }
                    TypeArray typeArgs = ats.AllTypeArguments;
                    for (int i = 0; i < typeArgs.Count; ++i)
                    {
                        if (IsTypeBuilderInstruction(typeArgs[i]))
                        {
                            return true;
                        }
                    }
                    return false;

                //----------------------------------------------------
                // ARRAYSYM
                //----------------------------------------------------
                case SYMKIND.ARRAYSYM:
                    return IsTypeBuilderInstruction(typeSym.ParentSym as TYPESYM);

                //----------------------------------------------------
                // VOIDSYM
                //----------------------------------------------------
                case SYMKIND.VOIDSYM:
                    return false;

                //----------------------------------------------------
                // PARAMMODSYM
                //----------------------------------------------------
                case SYMKIND.PARAMMODSYM:
                    return IsTypeBuilderInstruction(typeSym.ParentSym as TYPESYM);

                //----------------------------------------------------
                // TYVARSYM
                //----------------------------------------------------
                case SYMKIND.TYVARSYM:
                    return true;

                //----------------------------------------------------
                // PTRSYM
                //----------------------------------------------------
                case SYMKIND.PTRSYM:
                    return IsTypeBuilderInstruction((typeSym as PTRSYM).BaseTypeSym);

                //----------------------------------------------------
                // NUBSYM
                //----------------------------------------------------
                case SYMKIND.NUBSYM:
                    return IsTypeBuilderInstruction((typeSym as NUBSYM).BaseTypeSym);

                //----------------------------------------------------
                // otherwise
                //----------------------------------------------------
                case SYMKIND.NULLSYM:
                case SYMKIND.ERRORSYM:
                    break;

                case SYMKIND.MODOPTTYPESYM:
                    throw new NotImplementedException("SymbolUtil.MakeSystemType: MODOPTTYPESYM");

                case SYMKIND.ANONMETHSYM:
                case SYMKIND.METHGRPSYM:
                case SYMKIND.UNITSYM:
                    break;

                default:
                    break;
            }
            return false;
        }

        //------------------------------------------------------------
        // ReflectionUtil.CreateInstance
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static object CreateInstance(
            Type type,
            Object[] args,
            out Exception excp)
        {
            excp = null;
            try
            {
                return Activator.CreateInstance(type, args);
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (TargetInvocationException ex)
            {
                excp = ex;
            }
            catch (MethodAccessException ex)
            {
                excp = ex;
            }
            catch (System.Runtime.InteropServices.InvalidComObjectException ex)
            {
                excp = ex;
            }
            catch (MissingMethodException ex)
            {
                excp = ex;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                excp = ex;
            }
            catch (TypeLoadException ex)
            {
                excp = ex;
            }
            return null;
        }

        //------------------------------------------------------------
        // ReflectionUtil.InvokeMember
        //
        /// <summary></summary>
        /// <param name="declaringType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="binder"></param>
        /// <param name="target"></param>
        /// <param name="args"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool InvokeMember(
            Type declaringType,
            string name,
            BindingFlags flags,
            Binder binder,
            Object target,
            Object[] args,
            out Exception excp)
        {
            excp = null;
            try
            {
                declaringType.InvokeMember(name, flags, binder, target, args);
                return true;
            }
            catch (ArgumentException ex)
            {
                excp = ex;
            }
            catch (MethodAccessException ex)
            {
                excp = ex;
            }
            catch (MissingFieldException ex)
            {
                excp = ex;
            }
            catch (MissingMemberException ex)
            {
                excp = ex;
            }
            catch (TargetException ex)
            {
                excp = ex;
            }
            catch (AmbiguousMatchException ex)
            {
                excp = ex;
            }
            catch (NotSupportedException ex)
            {
                excp = ex;
            }
            catch (InvalidOperationException ex)
            {
                excp = ex;
            }
            return false;
        }

#if DEBUG
        //------------------------------------------------------------
        // ReflectionUtil.DebugCodeList
        //------------------------------------------------------------
        internal static void DebugCodeList(StringBuilder sb, BBLOCK firstBBlock)
        {
            for (BBLOCK block = firstBBlock; block != null; block = block.Next)
            {
                sb.Append("----------------------------------------\n");
                block.Debug(sb);
                sb.Append("----------------------------------------\n");
            }
        }
#endif
    }

    //======================================================================
    // NestedTypeResolver
    //
    /// <summary>
    /// <para>For AppDomain.TypeResolve event.</para>
    /// <para>(Defined in ReflectionUtil.cs)</para>
    /// </summary>
    //======================================================================
    internal class NestedTypeResolver
    {
        private COMPILER compiler = null;
        internal AGGSYM EnclosingAggSym = null;

        internal NestedTypeResolver(COMPILER comp, AGGSYM sym)
        {
            this.compiler = comp;
            this.EnclosingAggSym = sym;
        }

        //------------------------------------------------------------
        // NestedTypeResolver.ResolveType
        //
        /// <summary>
        /// Find the class specified in args and create its Type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal Assembly ResolveType(object sender, ResolveEventArgs args)
        {
            if (args == null || String.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            string[] names = args.Name.Split('.');
            AGGSYM aggSym = null;
            AGGSYM parentSym = this.EnclosingAggSym;
            Exception excp = null;

            for (int i = 0; i < names.Length; ++i)
            {
                aggSym = compiler.LookupGlobalSym(names[i], parentSym, SYMBMASK.AGGSYM) as AGGSYM;
                if (aggSym == null)
                {
                    return null;
                }
                parentSym = aggSym;
            }

            aggSym.CreateType(out excp);
            OUTFILESYM outfileSym = aggSym.GetOutputFile();
            if (outfileSym != null && outfileSym.AssemblyBuilderEx != null)
            {
                return outfileSym.AssemblyBuilderEx.AssemblyBuilder;
            }
            return null;
        }
    }
}
