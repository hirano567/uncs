//============================================================================
// SecurityUtil.cs
//
// 2015/09/28
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
    static internal class SecurityUtil
    {
        //------------------------------------------------------------
        // SecurityUtil.AppendSecurityAttribute (1)
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="secAttrType"></param>
        /// <param name="positionalArgs"></param>
        /// <param name="argNames"></param>
        /// <param name="argValues"></param>
        /// <param name="permissionSets"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public static bool AppendSecurityAttribute(
            Type secAttrType,
            Object[] positionalArgs,
            String[] argNames,
            Object[] argValues,
            Dictionary<SecurityAction, PermissionSet> permissionSets,
            out Exception excp)
        {
            excp = null;
            if (permissionSets == null)
            {
                return false;
            }
            if (secAttrType == null ||
                positionalArgs == null || positionalArgs.Length == 0)
            {
                return false;
            }

            SecurityAction secAction;
            try
            {
                secAction = (SecurityAction)positionalArgs[0];
            }
            catch (InvalidCastException ex)
            {
                excp = ex;
                return false;
            }

            //switch (secAction)
            //{
            //    case SecurityAction.Assert:
            //    case SecurityAction.Demand:
            //    case SecurityAction.Deny:
            //    case SecurityAction.InheritanceDemand:
            //    case SecurityAction.LinkDemand:
            //    case SecurityAction.PermitOnly:
            //    case SecurityAction.RequestMinimum:
            //    case SecurityAction.RequestOptional:
            //    case SecurityAction.RequestRefuse:
            //        break;
            //    default:
            //        return false;
            //}

            // Create the attribute instance with its positional arguments

            Object activatedObj = ReflectionUtil.CreateInstance(
                secAttrType,
                positionalArgs,
                out excp);

            if (activatedObj == null || excp != null)
            {
                return false;
            }

            // Assign the named arguments to the fields or properties.

            int namedCount = 0;
            if (argNames != null && (namedCount = argNames.Length) > 0)
            {
                if (argValues == null || argValues.Length != namedCount)
                {
                    return false;
                }
                Object[] narg = new Object[1];

                for (int i = 0; i < argNames.Length; ++i)
                {
                    string name = argNames[i];
                    if (String.IsNullOrEmpty(name))
                    {
                        return false;
                    }

                    MemberInfo[] membs = secAttrType.GetMember(name);
                    if (membs.Length != 1)
                    {
                        return false;
                    }

                    narg[0] = argValues[i];
                    MemberTypes membType = membs[0].MemberType;
                    BindingFlags bindFlags = 0;
                    if (membType == MemberTypes.Property)
                    {
                        bindFlags = BindingFlags.SetProperty;
                    }
                    else if (membType == MemberTypes.Field)
                    {
                        bindFlags = BindingFlags.SetField;
                    }
                    else
                    {
                        return false;
                    }

                    if (!ReflectionUtil.InvokeMember(
                        secAttrType,
                        name,
                        bindFlags,
                        null,
                        activatedObj,
                        narg,
                        out excp))
                    {
                        return false;
                    }
                }
            }

            SecurityAttribute secAttr = activatedObj as SecurityAttribute;
            if (secAttr == null)
            {
                return false;
            }

            IPermission secPerm = secAttr.CreatePermission();
            if (secAttr != null)
            {
                PermissionSet permSet = null;
                if (!permissionSets.TryGetValue(secAction, out permSet))
                {
                    permSet = new PermissionSet(null);
                    permissionSets.Add(secAction, permSet);
                }
                else if (permSet == null)
                {
                    permSet = new PermissionSet(null);
                    permissionSets[secAction] = permSet;
                }
                DebugUtil.Assert(permSet != null);

                permSet.AddPermission(secPerm);
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // SecurityUtil.AppendSecurityAttribute (2)
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="secAttrType"></param>
        /// <param name="positionalArgs"></param>
        /// <param name="argNames"></param>
        /// <param name="argValues"></param>
        /// <param name="permissionSets"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public static bool AppendSecurityAttribute(
            Type secAttrType,
            Object[] positionalArgs,
            List<string> fieldNames,
            List<object> fieldValues,
            List<string> propertyNames,
            List<object> propertyValues,
            Dictionary<SecurityAction, PermissionSet> permissionSets,
            out Exception excp)
        {
            excp = null;
            if (permissionSets == null)
            {
                return false;
            }
            if (secAttrType == null ||
                positionalArgs == null || positionalArgs.Length == 0)
            {
                return false;
            }

            int fieldCount = fieldNames != null ? fieldNames.Count : 0;
            if (fieldCount != (fieldValues != null ? fieldValues.Count : 0))
            {
                return false;
            }
            int propertyCount = propertyNames != null ? propertyNames.Count : 0;
            if (propertyCount != (propertyValues != null ? propertyValues.Count : 0))
            {
                return false;
            }

            SecurityAction secAction;
            try
            {
                secAction = (SecurityAction)positionalArgs[0];
            }
            catch (InvalidCastException ex)
            {
                excp = ex;
                return false;
            }

            //switch (secAction)
            //{
            //    case SecurityAction.Assert:
            //    case SecurityAction.Demand:
            //    case SecurityAction.Deny:
            //    case SecurityAction.InheritanceDemand:
            //    case SecurityAction.LinkDemand:
            //    case SecurityAction.PermitOnly:
            //    case SecurityAction.RequestMinimum:
            //    case SecurityAction.RequestOptional:
            //    case SecurityAction.RequestRefuse:
            //        break;
            //    default:
            //        return false;
            //}

            // Create the attribute instance with its positional arguments

            Object activatedObj = ReflectionUtil.CreateInstance(
                secAttrType,
                positionalArgs,
                out excp);

            if (activatedObj == null || excp != null)
            {
                return false;
            }

            // Assign the named arguments to the fields.

            Object[] narg = new Object[1];

            if (fieldCount > 0)
            {
                for (int i = 0; i < fieldCount; ++i)
                {
                    narg[0] = fieldValues[i];
                    if (!ReflectionUtil.InvokeMember(
                        secAttrType,
                        fieldNames[i],
                        BindingFlags.SetField,
                        null,
                        activatedObj,
                        narg,
                        out excp))
                    {
                        return false;
                    }
                }
            }

            // Assign the named arguments to the properties.

            if (propertyCount > 0)
            {
                for (int i = 0; i < propertyCount; ++i)
                {
                    narg[0] = propertyValues[i];
                    if (!ReflectionUtil.InvokeMember(
                        secAttrType,
                        propertyNames[i],
                        BindingFlags.SetProperty,
                        null,
                        activatedObj,
                        narg,
                        out excp))
                    {
                        return false;
                    }
                }
            }

            SecurityAttribute secAttr = activatedObj as SecurityAttribute;
            if (secAttr == null)
            {
                return false;
            }

            IPermission secPerm = secAttr.CreatePermission();
            if (secAttr != null)
            {
                PermissionSet permSet = null;
                if (!permissionSets.TryGetValue(secAction, out permSet))
                {
                    permSet = new PermissionSet(null);
                    permissionSets.Add(secAction, permSet);
                }
                else if (permSet == null)
                {
                    permSet = new PermissionSet(null);
                    permissionSets[secAction] = permSet;
                }
                DebugUtil.Assert(permSet != null);

                permSet.AddPermission(secPerm);
                return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // SecurityUtil.EmitSecurityAttributes (1) Type
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <param name="permissionSets"></param>
        //------------------------------------------------------------
        internal static bool EmitSecurityAttributes(
            AGGSYM aggSym,
            Dictionary<SecurityAction, PermissionSet> permissionSets,
            out Exception excp)
        {
            excp = null;
            bool rval = true;

            if (aggSym == null ||
                aggSym.TypeBuilder == null)
            {
                return false;
            }
            if (permissionSets == null || permissionSets.Count == 0)
            {
                return true;
            }

            foreach (KeyValuePair<SecurityAction,PermissionSet> kv in permissionSets)
            {
                switch (kv.Key)
                {
                    case SecurityAction.Assert:
                    case SecurityAction.Demand:
                    case SecurityAction.Deny:
                    case SecurityAction.InheritanceDemand:
                    case SecurityAction.LinkDemand:
                    case SecurityAction.PermitOnly:
                    default:
                        try
                        {
                            aggSym.TypeBuilder.AddDeclarativeSecurity(kv.Key, kv.Value);
                        }
                        catch (ArgumentException ex)
                        {
                            if (excp == null)
                            {
                                excp = ex;
                            }
                            rval = false;
                        }
                        catch (InvalidOperationException ex)
                        {
                            if (excp == null)
                            {
                                excp = ex;
                            }
                            rval = false;
                        }
                        break;

                    case SecurityAction.RequestMinimum:
                    case SecurityAction.RequestOptional:
                    case SecurityAction.RequestRefuse:
                        break;
                }
            }
            return rval;
        }

        //------------------------------------------------------------
        // SecurityUtil.EmitSecurityAttributes (2) Method
        //
        /// <summary></summary>
        /// <param name="methodSym"></param>
        /// <param name="permissionSets"></param>
        //------------------------------------------------------------
        internal static bool EmitSecurityAttributes(
            METHSYM methodSym,
            Dictionary<SecurityAction, PermissionSet> permissionSets,
            out Exception excp)
        {
            excp = null;
            bool rval = true;

            if (methodSym == null)
            {
                return false;
            }
            if (permissionSets == null || permissionSets.Count == 0)
            {
                return true;
            }

            foreach (KeyValuePair<SecurityAction, PermissionSet> kv in permissionSets)
            {
                switch (kv.Key)
                {
                    case SecurityAction.Assert:
                    case SecurityAction.Demand:
                    case SecurityAction.Deny:
                    case SecurityAction.InheritanceDemand:
                    case SecurityAction.LinkDemand:
                    case SecurityAction.PermitOnly:
                    default:
                        try
                        {
                            if (methodSym.MethodBuilder != null)
                            {
                                methodSym.MethodBuilder.AddDeclarativeSecurity(kv.Key, kv.Value);
                            }
                            else if (methodSym.ConstructorBuilder != null)
                            {
                                methodSym.ConstructorBuilder.AddDeclarativeSecurity(kv.Key, kv.Value);
                            }
                            else
                            {
                                // to do: set the message to excp.
                                rval = false;
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            if (excp == null)
                            {
                                excp = ex;
                            }
                            rval = false;
                        }
                        catch (InvalidOperationException ex)
                        {
                            if (excp == null)
                            {
                                excp = ex;
                            }
                            rval = false;
                        }
                        break;

                    case SecurityAction.RequestMinimum:
                    case SecurityAction.RequestOptional:
                    case SecurityAction.RequestRefuse:
                        break;
                }
            }
            return rval;
        }
    }
}
