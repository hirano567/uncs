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
// File: metaattr.h
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
// File: metaattr.cpp
//
// Routines for converting the attribute information of an item
// ===========================================================================

//============================================================================
// MetaAttr.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Reflection.Emit;

namespace Uncs
{
    // CorAttributeTargets is defined in clr\src\inc\corhdr.h, but do not use this enum.
    // Use System.AttributeTargets instead. Note that
    //     System.AttributeTargets does not has ClassMembers defined in CorAttributeTargets
    //     System.AttributeTargets defines ReturnValue (0x2000).

    //const CorAttributeTargets catReturnValue = (CorAttributeTargets) 0x2000;
    //const CorAttributeTargets catAllOld = catAll;
    //#define catAll ((CorAttributeTargets) (catAllOld | catReturnValue))

    //======================================================================
    // class AttrBind
    //
    /// <summary>
    /// Base class for all attribute binders.
    /// </summary>
    //======================================================================
    internal class AttrBind
    {
        //------------------------------------------------------------
        // AttrBind Fields and Properties
        //------------------------------------------------------------
        // stuff for the symbol being attributed

        protected SYM sym = null;
        protected System.AttributeTargets attributeTargets = 0; // CorAttributeTargets ek;
        protected ATTRTARGET attrTarget = ATTRTARGET.UNKNOWN;   // ATTRLOC attrloc;
        protected PARENTSYM contextSym = null;                  // context;
        protected List<SYM> customAttributeList = null;         // SYMLIST customAttributeList;
        protected bool hasLinkDemand = false;
        protected bool isEarly = false;                         // fEarly;

        // stuff for a single attribute

        protected AGGTYPESYM attributeTypeSym = null;   // attributeType;
        protected EXPR ctorExpr = null;                 // ctorExpression;
        protected EXPR namedArgumentsExpr = null;       // namedArguments;
        protected PREDEFATTR predefinedAttribute = PREDEFATTR.COUNT;

        protected COMPILER compiler = null;

        //------------------------------------------------------------
        // AttrBind Constructor
        //------------------------------------------------------------
        protected AttrBind(COMPILER comp, bool early)
        {
            this.isEarly = early;
            this.compiler = comp;
        }

        //------------------------------------------------------------
        // AttrBind.Init (1)
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        protected void Init(SYM sym)
        {
            DebugUtil.Assert(!sym.IsAGGDECLSYM);

            this.sym = sym;
            Init(sym.GetElementKind(), sym.IsAGGSYM ? null : sym.ContainingDeclaration());
            // AGGSYMs don't have a unique context.
        }

        //------------------------------------------------------------
        // AttrBind.Init (2)
        //
        /// <summary></summary>
        /// <param name="target"></param>
        /// <param name="context"></param>
        //------------------------------------------------------------
        protected void Init(System.AttributeTargets target, PARENTSYM context)
        {
            DebugUtil.Assert(context == null || !context.IsAGGSYM);

            this.attributeTargets = target;
            this.contextSym = context;

            // set the attrloc
            switch (target)
            {
                case AttributeTargets.ReturnValue:
                    this.attrTarget = ATTRTARGET.RETURN;
                    break;

                case AttributeTargets.Parameter:
                    this.attrTarget = ATTRTARGET.PARAMETER;
                    break;

                case AttributeTargets.Event:
                    this.attrTarget = ATTRTARGET.EVENT;
                    break;

                case AttributeTargets.Field:
                    this.attrTarget = ATTRTARGET.FIELD;
                    break;

                case AttributeTargets.Property:
                    this.attrTarget = ATTRTARGET.PROPERTY;
                    break;

                case AttributeTargets.Constructor:
                case AttributeTargets.Method:
                    this.attrTarget = ATTRTARGET.METHOD;
                    break;

                case AttributeTargets.Struct:
                case AttributeTargets.Class:
                case AttributeTargets.Enum:
                case AttributeTargets.Interface:
                case AttributeTargets.Delegate:
                    this.attrTarget = ATTRTARGET.TYPE;
                    break;

                case AttributeTargets.Module:
                    this.attrTarget = ATTRTARGET.MODULE;
                    break;

                case AttributeTargets.Assembly:
                    this.attrTarget = ATTRTARGET.ASSEMBLY;
                    break;

                case AttributeTargets.GenericParameter:
                    this.attrTarget = ATTRTARGET.TYPEVAR;
                    break;

                default:
                    DebugUtil.Assert(false, "Bad CorAttributeTargets");
                    break;
            }
        }

        //------------------------------------------------------------
        // AttrBind.ProcessAll (1)
        //
        /// <summary>
        /// <para>Call ProcessSingleAttr method for each attribute.</para>
        /// <para>Then call PostProcess method.</para>
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="permissionSets"></param>
        //------------------------------------------------------------
        protected void ProcessAll(
            BASENODE attributes,
            Dictionary<SecurityAction, PermissionSet> permissionSets)
        {
            if (attributes == null)
            {
                return;
            }

            BASENODE node1 = attributes;
            while (node1 != null)
            {
                ATTRDECLNODE attrDeclNode;
                if (node1.Kind == NODEKIND.LIST)
                {
                    attrDeclNode = node1.AsLIST.Operand1 as ATTRDECLNODE;
                    node1 = node1.AsLIST.Operand2;
                }
                else
                {
                    attrDeclNode = node1 as ATTRDECLNODE;
                    node1 = null;
                }

                if (attrDeclNode.Target == this.attrTarget)
                {
                    BASENODE node2 = attrDeclNode.AttributesNode;
                    while (node2 != null)
                    {
                        ATTRNODE attrNode;
                        if (node2.Kind == NODEKIND.LIST)
                        {
                            attrNode = node2.AsLIST.Operand1 as ATTRNODE;
                            node2 = node2.AsLIST.Operand2;
                        }
                        else
                        {
                            attrNode = node2 as ATTRNODE;
                            node2 = null;
                        }
                        ProcessSingleAttr(attrNode, permissionSets);
                    }
                }
            }
            PostProcess(attributes);
        }

        //------------------------------------------------------------
        // AttrBind.ProcessAll (2)
        //
        /// <summary>
        /// Compile all the attributes on all declarations of an aggsym.
        /// </summary>
        /// <param name="aggSym"></param>
        //------------------------------------------------------------
        protected void ProcessAll(
            AGGSYM aggSym,
            Dictionary<SecurityAction, PermissionSet> permissionSets)
        {
            BASENODE errorLocationNode = null;

            for (AGGDECLSYM aggDeclSym = aggSym.FirstDeclSym;
                aggDeclSym != null;
                aggDeclSym = aggDeclSym.NextDeclSym)
            {
                this.contextSym = aggDeclSym;

                BASENODE attributesNode = aggDeclSym.GetAttributesNode();

                if (errorLocationNode == null && attributesNode != null)
                {
                    errorLocationNode = attributesNode;
                    // Pick first attribute location for error reporting
                }

                BASENODE node1 = attributesNode;
                while (node1 != null)
                {
                    ATTRDECLNODE attrDeclNode;
                    if (node1.Kind == NODEKIND.LIST)
                    {
                        attrDeclNode = node1.AsLIST.Operand1 as ATTRDECLNODE;
                        node1 = node1.AsLIST.Operand2;
                    }
                    else
                    {
                        attrDeclNode = node1 as ATTRDECLNODE;
                        node1 = null;
                    }

                    if (attrDeclNode.Target == this.attrTarget)
                    {
                        BASENODE node2 = attrDeclNode.AttributesNode;
                        while (node2 != null)
                        {
                            ATTRNODE attrNode;
                            if (node2.Kind == NODEKIND.LIST)
                            {
                                attrNode = node2.AsLIST.Operand1 as ATTRNODE;
                                node2 = node2.AsLIST.Operand2;
                            }
                            else
                            {
                                attrNode = node2 as ATTRNODE;
                                node2 = null;
                            }
                            ProcessSingleAttr(attrNode, permissionSets);
                        }
                    }
                }
            }

            PostProcess(errorLocationNode);
        }

        //------------------------------------------------------------
        // AttrBind.ProcessSingleAttr
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected void ProcessSingleAttr(
            ATTRNODE attrNode,
            Dictionary<SecurityAction, PermissionSet> permissionSets)
        {
            DebugUtil.Assert(this.contextSym != null);
            DebugUtil.Assert(attrNode != null);
            // if this hits you want to call VerifyAndEmitCore() instead

            this.attributeTypeSym = null;
            this.ctorExpr = null;
            this.namedArgumentsExpr = null;

            //--------------------------------------------------
            // need to resolve the attribute as regular class
            // Get the TYPESYM instance of the attribute class.
            //--------------------------------------------------
            BASENODE nameNode = attrNode.NameNode;

            TYPESYM typeSym = TypeBind.BindAttributeType(
                this.compiler,
                nameNode,
                this.contextSym,
                TypeBindFlagsEnum.None);

            DebugUtil.Assert(typeSym != null);
            if (typeSym.IsERRORSYM)
            {
                return;
            }

            this.attributeTypeSym = typeSym as AGGTYPESYM;
            if (this.attributeTypeSym.GetAggregate().IsAbstract)
            {
                this.compiler.Error(
                    nameNode,
                    CSCERRID.ERR_AbstractAttributeClass,
                    new ErrArgNameNode(nameNode, ErrArgFlagsEnum.None));
            }

            // map from the attribute class back to a possible predefined attribute

            this.predefinedAttribute
                = this.compiler.MainSymbolManager.GetPredefAttr(this.attributeTypeSym);

            // Get the attribute arguments and
            if (BindAttr(attrNode) && !IsConditionalFalse())
            {
                // If permissionSets is not null,
                // process only security attributes.

                if (permissionSets != null)
                {
                    if (this.attributeTypeSym.IsSecurityAttribute())
                    {
                        Exception excp = null;
                        if (!VerifyAndAppendSecurtyAttribute(
                                attrNode,
                                permissionSets,
                                out excp))
                        {
                            if (excp != null)
                            {
                                this.compiler.Error(attrNode, ERRORKIND.ERROR, excp);
                            }
                        }
                    }
                    return;
                }

                if (!this.attributeTypeSym.IsSecurityAttribute())
                {
                    VerifyAndEmit(attrNode);
                }
            }
        }

        //------------------------------------------------------------
        // AttrBind.PostProcess
        //
        /// <summary>
        /// This is stuff that is done after compiling all the individual attributes.
        /// </summary>
        /// <param name="errorLocation"></param>
        //------------------------------------------------------------
        protected void PostProcess(BASENODE errorLocation)
        {
            ValidateAttrs();

            // GetToken() forces us to emit a token,
            // even in cases where none is needed like for param tokens.
            // So if there are no security attributes, don't emit a token!
            if ((sym == null || !sym.IsGLOBALATTRSYM) &&
                compiler.Emitter.HasSecurityAttributes())
            {
                compiler.Emitter.EmitSecurityAttributes(errorLocation, GetToken());
            }
        }

        //------------------------------------------------------------
        // AttrBind.ProcessSynthAttr
        //
        /// <summary></summary>
        /// <param name="attrTypeSym"></param>
        /// <param name="ctorExpr"></param>
        /// <param name="namedArgsExpr"></param>
        //------------------------------------------------------------
        protected void ProcessSynthAttr(AGGTYPESYM attrTypeSym, EXPR ctorExpr, EXPR namedArgsExpr)
        {
            this.attributeTypeSym = attrTypeSym;
            this.ctorExpr = ctorExpr;
            this.namedArgumentsExpr = namedArgsExpr;

            VerifyAndEmitCore(null);
        }

        //------------------------------------------------------------
        // AttrBind.CompileFabricatedAttr
        //
        /// <summary></summary>
        //------------------------------------------------------------
        protected void CompileFabricatedAttr()
        {
            if ((sym.IsFabricated || (sym.IsMETHSYM && (sym as METHSYM).IsAnonymous)) &&
                !sym.ParentSym.IsFabricated &&
                sym.GetBuilder() != null)
            {
#if DEBUG
                // Verify that none of the parents are fabricated.
                for (SYM temp = sym.ParentSym; temp != null; temp = temp.ParentSym)
                {
                    DebugUtil.Assert(!temp.IsFabricated);
                }
#endif
                CompilerGeneratedAttrBind.EmitAttribute(compiler, this.sym);
            }
        }

        //------------------------------------------------------------
        // AttrBind.ValidateAttrs
        //
        ///<summary>
        /// default implementation does nothing may be overridden  
        ///</summary>
        //------------------------------------------------------------
        protected virtual void ValidateAttrs()
        {
        }

        // the main loop methods

        //------------------------------------------------------------
        // AttrBind.BindAttr
        //
        /// <summary>
        /// <para>Create a EXPRCALL instance of the constructor of the specified atttribute class,
        /// and set it to this.ctorExpr.</para>
        /// <para>The Name arguments are set to this.namedArgumentsExpr.</para>
        /// </summary>
        /// <param name="attrNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual protected bool BindAttr(ATTRNODE attrNode)
        {
            DebugUtil.Assert(this.attributeTypeSym != null);
            DebugUtil.Assert(attrNode != null);

            EXPRCALL callExpr;
            EXPR firstNamedArgExpr = null;
            EXPR lastNamedArgExpr = null;

            AGGSYM aggSym = this.attributeTypeSym.GetAggregate();
            compiler.EnsureState(aggSym, AggStateEnum.Prepared);

            if (!aggSym.IsAttribute)
            {
                compiler.ErrorRef(
                    attrNode.NameNode,
                    CSCERRID.ERR_NotAnAttributeClass,
                    new ErrArgRef(aggSym));
                return false;
            }

            // Don't check for allowable targets and multiple before the prepare stage.
            // This is since the attributeClass might not be set yet and we should always
            // go through the attributes during the compile phase anyway, so we should
            // catch all the errors then.

            if (compiler.AggStateMax >= AggStateEnum.Prepared)
            {
                // Check whether the attribute can target this symbol type.
                if ((aggSym.AttributeClass & this.attributeTargets) == 0)
                {
                    DebugUtil.Assert(aggSym.AttributeClass != 0);
                    ErrorBadSymbolKind(attrNode.NameNode);
                    return false;
                }

                //----------------------------------------------------
                // Check for invalid duplicate attributes.
                //----------------------------------------------------
                if (compiler.AggStateMax >= AggStateEnum.Prepared &&
                    !aggSym.IsMultipleAttribute)
                {
                    if (this.customAttributeList == null)
                    {
                        this.customAttributeList = new List<SYM>();
                    }
                    List<SYM> symList = this.customAttributeList;

                    foreach (SYM sym in symList)
                    {
                        if (sym == this.attributeTypeSym)
                        {
                            compiler.Error(
                                attrNode.NameNode,
                                CSCERRID.ERR_DuplicateAttribute,
                                new ErrArgNameNode(attrNode.NameNode, ErrArgFlagsEnum.None));
                            return false;
                        }
                    }

                    // Not found, add it to our list of attributes for this symbol.
                    compiler.LocalSymbolManager.AddToLocalSymList(this.attributeTypeSym, symList);
                }
            } // if (compiler.AggStateMax >= AggStateEnum.Prepared)

            //--------------------------------------------------
            // bind ctor args
            //--------------------------------------------------
            callExpr = compiler.FuncBRec.BindAttribute(
                this.contextSym,
                this.attributeTypeSym,
                attrNode,
                out firstNamedArgExpr,
                out lastNamedArgExpr);

            //----------------------------------------------------
            // verify that args are constants
            //----------------------------------------------------
            bool badArg = false;
            EXPR expr;
            if (callExpr != null && callExpr.MethodWithInst.MethSym != null)
            {
                expr = callExpr.ArgumentsExpr;
                while (expr != null)
                {
                    EXPR arg;
                    if (expr.Kind == EXPRKIND.LIST)
                    {
                        arg = expr.AsBIN.Operand1;
                        expr = expr.AsBIN.Operand2;
                    }
                    else
                    {
                        arg = expr;
                        expr = null;
                    }
                    if (!VerifyAttrArg(arg)) badArg = true;
                }
            }

            //----------------------------------------------------
            // verify named args
            //----------------------------------------------------
            expr = firstNamedArgExpr;
            while (expr != null)
            {
                EXPR arg;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg = expr;
                    expr = null;
                }
                if (!VerifyAttrArg((arg as EXPRBINOP).Operand2)) badArg = true;
            }

            if (badArg || callExpr == null || callExpr.MethodWithInst.MethSym == null)
            {
                return false;
            }

            this.ctorExpr = callExpr;
            this.namedArgumentsExpr = firstNamedArgExpr;

            this.hasLinkDemand |= IsLinkDemand(attrNode);

            return true;
        }

        //------------------------------------------------------------
        // AttrBind.VerifyAndEmit
        //
        /// <summary></summary>
        /// <param name="attr"></param>
        //------------------------------------------------------------
        protected void VerifyAndEmit(ATTRNODE attr)
        {
            // If this assert fires you probably want to call VerifyAndEmitCore() instead.
            DebugUtil.Assert(attr != null);

            if (predefinedAttribute != PREDEFATTR.COUNT)
            {
                VerifyAndEmitPredef(attr);
            }
            else
            {
                if (!attr.Emitted)
                {
                    VerifyAndEmitCore(attr);
                }
            }
        }

        //------------------------------------------------------------
        // AttrBind.VerifyAndEmitCore
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected virtual void VerifyAndEmitCore(ATTRNODE attrNode)
        {
            if (this.isEarly)
            {
                return;
            }
            if (this.ctorExpr == null)
            {
                return;
            }

            EXPRCALL callCtorExpr = this.ctorExpr as EXPRCALL;
            if (callCtorExpr == null ||
                callCtorExpr.MethodWithInst == null ||
                callCtorExpr.MethodWithInst.MethSym == null)
            {
                return;
            }

            ConstructorInfo constructorInfo = callCtorExpr.MethodWithInst.MethSym.ConstructorInfo;
            if (constructorInfo == null)
            {
                return;
            }

            //BYTE rgb[256];
            //BlobBldrNrHeap blob(&this->compiler->localSymAlloc, rgb, lengthof(rgb));

            List<Object> positionalArgs = new List<object>();
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            List<Object> fieldValues = new List<object>();
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            List<Object> propertyValues = new List<object>();

            // Serialize the ctor arguments into the buffer
            //StoreUSHORT(blob, 1);

            EXPR expr = callCtorExpr.ArgumentsExpr;
            while (expr != null)
            {
                EXPR arg;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg = expr;
                    expr = null;
                }
                AddAttrArg(arg, positionalArgs);
            }

            // Serialize the named arguments to the buffer
            //StoreUSHORT(blob, (USHORT) CountNamedArgs());

            expr = this.namedArgumentsExpr;
            while (expr != null)
            {
                EXPR arg;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg = expr;
                    expr = null;
                }
                EXPRBINOP assignExpr = arg as EXPRBINOP;
                string name = null;
                TYPESYM typeSym = null;

                // Get the name, typeSym and store the field/property byte
                if (assignExpr.Operand1.Kind == EXPRKIND.FIELD)
                {
                    MEMBVARSYM fieldSym = (assignExpr.Operand1 as EXPRFIELD).FieldWithType.FieldSym;
                    name = fieldSym.Name;
                    typeSym = fieldSym.TypeSym;
                    //blob.Add((byte)CorSerializationType.FIELD);
                    fieldInfos.Add(fieldSym.FieldInfo);
                    AddAttrArg(assignExpr.Operand2, fieldValues);
                }
                else
                {
                    PROPSYM propertySym = (assignExpr.Operand1 as EXPRPROP).SlotPropWithType.PropSym;
                    name = propertySym.Name;
                    typeSym = propertySym.ReturnTypeSym;
                    //blob.Add((byte)CorSerializationType.PROPERTY);
                    propertyInfos.Add(propertySym.PropertyInfo);
                    AddAttrArg(assignExpr.Operand2, propertyValues);
                }

                // Member type, name and value
                //Util.StoreEncodedType(blob, typeSym, compiler);
                //StoreString(blob, name);
                //blob.Add(name);
                //AddAttrArg(assignExpr.Operand2, blob);
            }

            // Write the attribute to the metadata
            //if (!compiler.Options.NoCodeGen &&
            //    (!compiler.Options.CompileSkeleton || this.attributeTypeSym.IsPredefined()))
            {
                //compiler.Emitter.EmitCustomAttribute(
                //    attrNode,
                //    GetToken(),
                //    this.ctorExpr.AsCALL.MethodWithInst.MethSym,
                //    //blob.Buffer(),
                //    //blob.Length());
                //    blob.BlobList);

                CustomAttributeBuilder caBuilder = ReflectionUtil.CreateCustomAttributeBuilder(
                    constructorInfo,
                    positionalArgs,
                    propertyInfos,
                    propertyValues,
                    fieldInfos,
                    fieldValues);

                if (caBuilder != null)
                {
                    //compiler.Emitter.EmitCustomAttribute(this.sym, caBuilder);
                    this.EmitCustomAttribute(caBuilder);
                }
            }
        }

        //------------------------------------------------------------
        // AttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected virtual void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            switch (this.predefinedAttribute)
            {
                //----------------------------------------------------
                // KEYFILE
                //----------------------------------------------------
                case PREDEFATTR.KEYFILE:
                    {
                        //if (!compiler.Options.CompileSkeleton || this.ctorExpr.Kind != EXPRKIND.CALL)
                        {
                            break;
                        }
                        EXPR argExpr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
                        EXPRCONSTANT constArgExpr = argExpr as EXPRCONSTANT;

                        if (argExpr == null ||
                            !argExpr.TypeSym.IsPredefType(PREDEFTYPE.STRING) ||
                            argExpr.Kind != EXPRKIND.CONSTANT ||
                            constArgExpr.IsNull() ||
                            constArgExpr.ConstVal.GetString() == null ||
                            constArgExpr.ConstVal.GetString().Length == 0)
                        {
                            break;
                        }

                        string strTemp = constArgExpr.ConstVal.GetString();
                        if (Path.IsPathRooted(strTemp))
                        {
                            break;
                        }
                        StringBuilder sbPrefixed = new StringBuilder("..\\");
                        sbPrefixed.Append(strTemp);
                        constArgExpr.ConstVal.SetString(sbPrefixed.ToString());
                        break;
                    }

                //----------------------------------------------------
                // OBSOLETE
                //----------------------------------------------------
                case PREDEFATTR.OBSOLETE:
                    if (this.isEarly)
                    {
                        ProcessObsoleteEarly(attrNode);
                    }
                    break;

                //----------------------------------------------------
                // CLSCOMPLIANT
                //----------------------------------------------------
                case PREDEFATTR.CLSCOMPLIANT:
                    if (this.isEarly)
                    {
                        ProcessCLSEarly(attrNode);
                    }
                    else
                    {
                        VerifyCLS(attrNode);
                    }
                    break;

                //----------------------------------------------------
                // CONDITIONAL
                //----------------------------------------------------
                case PREDEFATTR.CONDITIONAL:
                    if (this.isEarly)
                    {
                        ProcessConditionalEarly(attrNode);
                    }
                    break;

                //----------------------------------------------------
                // TYPEFORWARDER
                //----------------------------------------------------
                case PREDEFATTR.TYPEFORWARDER:
                    {
                        // if a TypeForwardedToAttribute is specified,
                        // then emit an entry to the exported typeSym table,
                        // instead of emitting the assembly level attribute.

                        EXPR argExpr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
                        DebugUtil.Assert(argExpr.TypeSym.IsPredefType(PREDEFTYPE.TYPE) || !argExpr.IsOK);

                        TYPESYM typeSym = null;
                        if (!GetValue(argExpr, out typeSym) || typeSym == null || !typeSym.IsAGGTYPESYM)
                        {
                            compiler.Error(attrNode, CSCERRID.ERR_InvalidFwdType);
                            return;
                        }

                        AGGSYM aggSym = (typeSym as AGGTYPESYM).GetAggregate();
                        if (aggSym.InAlias(Kaid.ThisAssembly))
                        {
                            // cannot emit a typeSym forwarder to a typeSym that is defined in this assembly.
                            CError err = new CError();
                            err = compiler.MakeError(
                                attrNode,
                                CSCERRID.ERR_ForwardedTypeInThisAssembly,
                                new ErrArg(typeSym));
                            if (err == null)
                            {
                                return;
                            }

                            if (aggSym.IsSource)
                            {
                                compiler.AddLocationToError(
                                    err,
                                    new ERRLOC(
                                        compiler.MainSymbolManager,
                                        aggSym.FirstDeclSym.GetParseTree(),
                                        compiler.OptionManager.FullPaths));
                            }
                            else
                            {
                                compiler.AddLocationToError(
                                    err,
                                    new ERRLOC(
                                        aggSym.DeclOnly().GetInputFile(),
                                        compiler.OptionManager.FullPaths));
                            }
                            compiler.SubmitError(err);
                        }
                        else if (aggSym.IsNested)
                        {
                            compiler.Error(
                                attrNode,
                                CSCERRID.ERR_ForwardedTypeIsNested,
                                new ErrArg(aggSym),
                                new ErrArg(aggSym.ParentSym));
                        }
                        else if (aggSym.TypeVariables.Count > 0)
                        {
                            compiler.Error(attrNode, CSCERRID.ERR_FwdedGeneric, new ErrArg(typeSym));
                        }
                        else
                        {
                            compiler.Emitter.EmitTypeForwarder(typeSym as AGGTYPESYM);
                        }
                        // We do not emit the actual attribute, so we just return instead.
                        return;
                    }

                //----------------------------------------------------
                // default
                //----------------------------------------------------
                default:
                    break;
            }

            VerifyAndEmitCore(attrNode);
        }

        //------------------------------------------------------------
        // AttrBind.VerifyAndAppendSecurtyAttribute
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attrNode"></param>
        /// <param name="permissionSets"></param>
        //------------------------------------------------------------
        protected bool VerifyAndAppendSecurtyAttribute(
            ATTRNODE attrNode,
            Dictionary<SecurityAction, PermissionSet> permissionSets,
            out Exception excp)
        {
            excp = null;
            if (this.isEarly)
            {
                return true;
            }
            if (attrNode == null ||
                permissionSets == null)
            {
                return false;
            }
            if (this.ctorExpr == null)
            {
                return false;
            }

            EXPRCALL callCtorExpr = this.ctorExpr as EXPRCALL;
            if (callCtorExpr == null)
            {
                return false;
            }

            Type attrType = callCtorExpr.TypeSym.GetAggregate().Type;

            EXPR expr = callCtorExpr.ArgumentsExpr;
            if (expr == null)
            {
                return false;
            }

            List<Object> positionalArgList = new List<object>();
            Object[] positionalArgs = null;
            List<string> fieldNames = new List<string>();
            List<Object> fieldValues = new List<object>();
            List<string> propertyNames = new List<string>();
            List<Object> propertyValues = new List<object>();

            // Serialize the ctor arguments into the buffer

            while (expr != null)
            {
                EXPR arg;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg = expr;
                    expr = null;
                }
                AddAttrArg(arg, positionalArgList);
            }

            if (positionalArgList.Count > 0)
            {
                positionalArgs = new object[positionalArgList.Count];
                positionalArgList.CopyTo(positionalArgs);
            }
            else
            {
                positionalArgs = new object[0];
            }

            // Serialize the named arguments to the buffer

            expr = this.namedArgumentsExpr;
            while (expr != null)
            {
                EXPR arg;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg = expr;
                    expr = null;
                }
                EXPRBINOP assignExpr = arg as EXPRBINOP;
                string name = null;

                // Get the name, typeSym and store the field/property byte
                if (assignExpr.Operand1.Kind == EXPRKIND.FIELD)
                {
                    MEMBVARSYM fieldSym = (assignExpr.Operand1 as EXPRFIELD).FieldWithType.FieldSym;
                    name = fieldSym.Name;
                    fieldNames.Add(fieldSym.FieldInfo.Name);
                    AddAttrArg(assignExpr.Operand2, fieldValues);
                }
                else if (assignExpr.Operand1.Kind == EXPRKIND.PROP)
                {
                    PROPSYM propertySym = (assignExpr.Operand1 as EXPRPROP).SlotPropWithType.PropSym;
                    name = propertySym.Name;
                    propertyNames.Add(propertySym.PropertyInfo.Name);
                    AddAttrArg(assignExpr.Operand2, propertyValues);
                }
                else
                {
                    DebugUtil.Assert(false);
                }
            }

            return SecurityUtil.AppendSecurityAttribute(
                attrType,
                positionalArgs,
                fieldNames,
                fieldValues,
                propertyNames,
                propertyValues,
                permissionSets,
                out excp);
        }

        //------------------------------------------------------------
        // AttrBind.GetToken
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        protected virtual int GetToken()    // mdToken GetToken()
        {
            int token = sym.GetTokenEmit();
            DebugUtil.Assert(token != 0);
            return token;
        }

        //------------------------------------------------------------
        // AttrBind.GetBuilder
        //
        /// <summary></summary>
        /// <remarks>
        /// in place of GetToken().
        /// </remarks>
        /// <returns></returns>
        //------------------------------------------------------------
        protected virtual Object GetBuilder()
        {
            return (this.sym != null ? this.sym.GetBuilder() : null);
        }

        //------------------------------------------------------------
        // AttrBind.AddAttrArg
        //
        /// <summary>
        /// Store a value of an EXPR instace to List&lt;Object&gt;
        /// </summary>
        /// <param name="argExpr"></param>
        /// <param name="valueList"></param>
        //------------------------------------------------------------
        protected void AddAttrArg(EXPR argExpr, List<Object> valueList)
        {
            TYPESYM typeSym = argExpr.TypeSym;
            PREDEFTYPE predefTypeID;

            switch (typeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    if (typeSym.IsEnumType())
                    {
                        predefTypeID = typeSym.UnderlyingEnumType().GetPredefType();
                    }
                    else
                    {
                        DebugUtil.Assert(typeSym.IsPredefined());
                        predefTypeID = typeSym.GetPredefType();
                    }

                    DebugUtil.Assert(BSYMMGR.GetAttrArgSize(predefTypeID) != 0);
                    switch (predefTypeID)
                    {
                        case PREDEFTYPE.BOOL:
                        case PREDEFTYPE.SBYTE:
                        case PREDEFTYPE.BYTE:
                        case PREDEFTYPE.SHORT:
                        case PREDEFTYPE.CHAR:
                        case PREDEFTYPE.USHORT:
                        case PREDEFTYPE.INT:
                        case PREDEFTYPE.UINT:
                        case PREDEFTYPE.FLOAT:
                        case PREDEFTYPE.LONG:
                        case PREDEFTYPE.ULONG:
                        case PREDEFTYPE.DOUBLE:
                        case PREDEFTYPE.STRING:
                            valueList.Add((argExpr as EXPRCONSTANT).ConstVal.GetObject());
                            break;

                        case PREDEFTYPE.OBJECT:
                            // This must be a constant argument to an Variant.OpImplicit call or a null.
                            if (argExpr.Kind == EXPRKIND.CAST)
                            {
                                // implicit cast of something to object
                                // need to encode the underlying object(enum, typeSym, string)
                                argExpr = (argExpr as EXPRCAST).Operand;
                                AddAttrArg(argExpr, valueList);
                            }
                            else
                            {
                                DebugUtil.Assert(argExpr.IsNull());
                                valueList.Add((object)null);
                            }
                            break;

                        case PREDEFTYPE.TYPE:
                            switch (argExpr.Kind)
                            {
                                default:
                                    DebugUtil.Assert(false, "Unknown constant of typeSym typeSym");
                                    // Fall through
                                    goto case EXPRKIND.CONSTANT;

                                case EXPRKIND.CONSTANT:
                                    // this handles the 'null' typeSym constant
                                    //Util.StoreTypeName(blob, null, compiler, false);
                                    valueList.Add(typeSym != null ?
                                        SymUtil.GetSystemTypeFromSym(typeSym, null, null) :
                                        null);
                                    break;

                                case EXPRKIND.TYPEOF:
                                    TYPESYM tySym = (argExpr as EXPRTYPEOF).SourceTypeSym;
                                    if ((argExpr.Flags & EXPRFLAG.OPENTYPE) != 0)
                                    {
                                        int typeCount = (tySym as AGGTYPESYM).AllTypeArguments.Count;
                                        DebugUtil.Assert(tySym.IsAGGTYPESYM && typeCount > 0 &&
                                            (tySym as AGGTYPESYM).AllTypeArguments[typeCount - 1].IsUNITSYM);
                                    }
                                    else
                                    {
                                        DebugUtil.Assert(!BSYMMGR.TypeContainsTyVars(tySym, null));
                                    }
                                    //Util.StoreTypeName(blob, tySym, compiler, (argExpr.Flags & EXPRFLAG.OPENTYPE) != 0);
                                    valueList.Add(tySym != null ?
                                        SymUtil.GetSystemTypeFromSym(tySym, null, null) :
                                        null);
                                    break;
                            }
                            break;

                        default:
                            DebugUtil.Assert(false, "bad variable size attribute argument typeSym");
                            break;
                    }
                    break;

                case SYMKIND.ARRAYSYM:
                    throw new NotImplementedException("AttrBind.AddAttrArg");

                    ARRAYSYM arraySym;
                    arraySym = typeSym as ARRAYSYM;

                    DebugUtil.Assert(
                        arraySym.Rank == 1 &&
                        (arraySym.ElementTypeSym.IsPredefined() ||
                        arraySym.ElementTypeSym.IsEnumType()));

                    if (argExpr.Kind == EXPRKIND.CONSTANT)
                    {
                        //Util.StoreDWORD(blob, 0xFFFFFFFF);
                    }
                    else
                    {
                        //Util.StoreDWORD(blob, (uint)argExpr.AsARRINIT.DimSizes[0]);
                        EXPR param;
                        EXPR next = (argExpr as EXPRARRINIT).ArgumentsExpr;
                        while (next != null)
                        {
                            if (next.Kind == EXPRKIND.LIST)
                            {
                                param = next.AsBIN.Operand1;
                                next = next.AsBIN.Operand2;
                            }
                            else
                            {
                                param = next;
                                next = null;
                            }
                            //AddAttrArg(param, tempList);
                        }
                    }
                    break;

                default:
                    DebugUtil.Assert(false, "unexpected attribute argument typeSym");
                    break;
            }
        }

        //------------------------------------------------------------
        // AttrBind.IsConditionalFalse
        //
        /// <summary>
        /// <para>Tests whether this is a conditional attribute,
        /// and if so whether those conditional symbols are defined for the current input file.</para>
        /// <para>Returns true iff this is a conditional attribute which should not be emitted
        /// (i.e. the attribute is conditional on some symbols but none of them are defined at the current location).</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool IsConditionalFalse()
        {
            INFILESYM inFileSym = contextSym.GetInputFile();
            DebugUtil.Assert(inFileSym != null);
            // only way this can fire is if the context is set incorrectly

            bool isConditional = false;
            List<string> nameList = compiler.ClsDeclRec.GetConditionalSymbols(
                this.attributeTypeSym.GetAggregate());
            foreach (string name in nameList)
            {
                isConditional = true;
                if (inFileSym.IsSymbolDefined(name)) return false;
            }

            // return true only in the case
            // where we actually saw some conditional symbols
            // and none were defined in the current scope.
            return isConditional;
        }

        //------------------------------------------------------------
        // AttrBind.CountNamedArgs
        //
        /// <summary>
        /// Count the elements of the EXPR list
        /// starting at AttrBind.namedArgumentsExpr.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        protected int CountNamedArgs()
        {
            int numberOfNamedArguments = 0;
            EXPR expr = this.namedArgumentsExpr;
            while (expr != null)
            {
                EXPR arg;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    arg = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    arg = expr;
                    expr = null;
                }
                ++numberOfNamedArguments;
            }

            return numberOfNamedArguments;
        }

        // helpers for predefined attributes which can occur on more than one SK

        //------------------------------------------------------------
        // AttrBind.ProcessObsoleteEarly
        //
        /// <summary>
        /// Set a deprecated message to AttrBind.sym.
        /// </summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected void ProcessObsoleteEarly(ATTRNODE attrNode)
        {
            DebugUtil.Assert(this.isEarly);

            // crack the ctor arguments to get the obsolete information

            string msg = null;
            bool isError = false;
            bool isFirst = true;

            EXPR expr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
            while (expr != null)
            {
                EXPR argument;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    argument = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    argument = expr;
                    expr = null;
                }

                if (isFirst)
                {
                    string message = null;
                    if (!GetValue(argument, out message))
                    {
                        break;
                    }
                    if (message != null)
                    {
                        // add string to global heap so that it doesn't go away
                        msg = message;
                        compiler.NameManager.AddString(message);
                    }
                    isFirst = false;
                }
                else
                {
                    GetValue(argument, out isError);
                    break;
                }
            }
            sym.SetDeprecated(true, isError, msg);
        }

        //------------------------------------------------------------
        // AttrBind.ProcessCLSEarly
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected void ProcessCLSEarly(ATTRNODE attrNode)
        {
            DebugUtil.Assert(this.isEarly);

            sym.HasCLSAttribute = true;

            // Mark it according to the argument
            EXPR expr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
            while (expr != null)
            {
                EXPR argument;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    argument = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    argument = expr;
                    expr = null;
                }
                if (argument.TypeSym.IsPredefType(PREDEFTYPE.BOOL))
                {
                    bool value;
                    if (!GetValue(argument, out value))
                    {
                        break;
                    }
                    sym.IsCLS = value;
                }
            }
        }

        //------------------------------------------------------------
        // AttrBind.VerifyCLS
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected void VerifyCLS(ATTRNODE attrNode)
        {
            if (sym.IsGLOBALATTRSYM || !compiler.AllowCLSErrors()) return;

            OUTFILESYM outFileSym = this.GetOutputFile();

            if (!outFileSym.HasCLSAttribute)
            {
                if (this.sym != null && this.sym.IsCLS)
                {
                    // It's illegal to have CLSCompliant(true)
                    // when the assembly is not marked
                    compiler.Error(
                        attrNode,
                        CSCERRID.WRN_CLS_AssemblyNotCLS,
                        new ErrArg(this.sym));
                }
                else
                {
                    compiler.Error(
                        attrNode,
                        CSCERRID.WRN_CLS_AssemblyNotCLS2,
                        new ErrArg(this.sym));
                }
            }
            else if (!this.sym.HasExternalAccess())
            {
                // Why put CLSCompliant on a private thing?
                compiler.Error(
                    attrNode,
                    CSCERRID.WRN_CLS_MeaninglessOnPrivateType,
                    new ErrArg(this.sym));
            }
            else if (sym.IsCLS)
            {
                PARENTSYM parentSym = sym.ParentSym;
                if (parentSym.IsAGGSYM && !compiler.CheckSymForCLS(parentSym, false))
                {
                    // It's illegal to have CLSCompliant(true) inside a non-compliant type
                    // We know the parentSym is non-compliant because the assembly/module is.
                    compiler.Error(
                        attrNode,
                        CSCERRID.WRN_CLS_IllegalTrueInFalse,
                        new ErrArg(sym),
                        new ErrArg(parentSym));
                    sym.IsCLS = false;
                }
            }
        }

        //------------------------------------------------------------
        // AttrBind.IsLinkDemand
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool IsLinkDemand(ATTRNODE attrNode)
        {
            if (this.attributeTypeSym.IsSecurityAttribute())
            {
                EXPR expr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
                while (expr != null)
                {
                    EXPR argument;
                    if (expr.Kind == EXPRKIND.LIST)
                    {
                        argument = expr.AsBIN.Operand1;
                        expr = expr.AsBIN.Operand2;
                    }
                    else
                    {
                        argument = expr;
                        expr = null;
                    }

                    if (argument.TypeSym.IsPredefType(PREDEFTYPE.SECURITYACTION))
                    {
                        int value;
                        if (!GetValue(argument, out value))
                        {
                            break;
                        }
                        return (value == (int)CorDeclSecurity.LinktimeCheck);
                    }
                    break;
                }
            }
            return false;
        }

        //------------------------------------------------------------
        // AttrBind.ProcessConditionalEarly
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected void ProcessConditionalEarly(ATTRNODE attrNode)
        {
            DebugUtil.Assert(this.isEarly);

            // Validate arguments ...
            string conditionalValue = null;
            EXPR expr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
            while (expr != null)
            {
                EXPR argument;
                if (expr.Kind == EXPRKIND.LIST)
                {
                    argument = expr.AsBIN.Operand1;
                    expr = expr.AsBIN.Operand2;
                }
                else
                {
                    argument = expr;
                    expr = null;
                }

                if (conditionalValue != null)
                {
                    DebugUtil.Assert(false,
                        "We bound to a ConditionalAttribute constructor "
                        + "that takes more than 1 argument.");
                    break;
                }
                if (!GetValue(argument, out conditionalValue))
                {
                    break;
                }

                // Convert the string to a name and return it
                if (compiler.CheckForValidIdentifier(
                    conditionalValue,
                    true,
                    argument.TreeNode,
                    CSCERRID.ERR_BadArgumentToAttribute,
                    new ErrArg(this.attributeTypeSym)))
                {
                    compiler.NameManager.AddString(conditionalValue);
                    AddConditionalName(conditionalValue);
                }
            }
        }

        //------------------------------------------------------------
        // AttrBind.AddConditionalName
        //
        /// <summary>
        /// Virtual method. Does nothing. 
        /// </summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        protected virtual void AddConditionalName(string name)
        {
        }

        //------------------------------------------------------------
        // AttrBind.AddDefaultCharSet
        //
        /// <summary></summary>
        //------------------------------------------------------------
        protected void AddDefaultCharSet()
        {
            OUTFILESYM outfileSym = GetOutputFile();
            if ((int)outfileSym.DefaultCharSet > 1)
            {
                // This means it is CharSet.Ansi, CharSet.Unicode, or CharSet.Auto

                // Can't use EXPRLOOP because we want to keep the end-of-list pointer
                // so we can add the CharSet property if needed
                EXPR cur = namedArgumentsExpr;
                //EXPR ** lastArg = &namedArguments;
                EXPR arg = namedArgumentsExpr;

                while (cur != null)
                {
                    if (cur.Kind == EXPRKIND.LIST)
                    {
                        arg = cur.AsBIN.Operand1;
                        //lastArg = &cur.asBIN().p2;
                        cur = cur.AsBIN.Operand2;
                    }
                    else
                    {
                        arg = cur;
                        cur = null;
                    }

                    string name = GetNamedArgumentName(arg);
                    if (name == GetPredefName(PREDEFNAME.CharSet))
                    {
                        return;
                    }
                }

                compiler.FuncBRec.BindCharSetNamedArg(
                    this.attributeTypeSym,
                    outfileSym.DefaultCharSet,
                    ref namedArgumentsExpr,
                    ref arg);
            }
        }

        //------------------------------------------------------------
        // AttrBind.GetOutputFile
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        protected OUTFILESYM GetOutputFile()
        {
            return this.sym.GetSomeInputFile().GetOutFileSym();
        }

        //------------------------------------------------------------
        // AttrBind.ErrorBadSymbolKind
        //
        /// <summary>
        /// Show an error message that a target of an attributes is invalid.
        /// </summary>
        /// <param name="tree"></param>
        //------------------------------------------------------------
        protected void ErrorBadSymbolKind(BASENODE tree)
        {
            // get valid targets

            //CorAttributeTargets validTargets; 
            System.AttributeTargets validTargets;

            validTargets = this.attributeTypeSym.GetAggregate().AttributeClass;
        
            // Check to make sure we aren't incorrectly reporting an error
            DebugUtil.Assert((this.attributeTargets & validTargets) == 0);
        
            // convert valid targets to string

            compiler.Error(
                tree,
                CSCERRID.ERR_AttributeOnBadSymbolType,
                new ErrArgNameNode(tree,ErrArgFlagsEnum.None),
                new ErrArg(AttrUtil.BuildAttrTargetString(validTargets)));
        }

        // return the given predefined type (including void)

        //------------------------------------------------------------
        // AttrBind.GetPredefName
        //
        /// <summary>
        /// Call CNameManager.GetPredefinedName.
        /// </summary>
        /// <param name="pn"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected string GetPredefName(PREDEFNAME pn)
        {
            return compiler.NameManager.GetPredefinedName(pn);
        }

        //------------------------------------------------------------
        // AttrBind.
        //
        //------------------------------------------------------------
        //protected bool isAttributeType(TYPESYM * type);

        //------------------------------------------------------------
        // AttrBind.GetValue (1)
        //
        /// <summary></summary>
        /// <param name="argument"></param>
        /// <param name="rval"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool GetValue(EXPR argument, out bool rval)
        {
            return compiler.FuncBRec.GetAttributeValue(this.contextSym, argument, out rval);
        }

        //------------------------------------------------------------
        // AttrBind.GetValue (2)
        //
        /// <summary></summary>
        /// <param name="argument"></param>
        /// <param name="rval"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool GetValue(EXPR argument, out int rval)
        {
            return compiler.FuncBRec.GetAttributeValue(this.contextSym, argument, out rval);
        }

        //------------------------------------------------------------
        // AttrBind.GetValue (3)
        //
        /// <summary></summary>
        /// <param name="argument"></param>
        /// <param name="rval"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected bool GetValue(EXPR argument, out string rval)
        {
            return compiler.FuncBRec.GetAttributeValue(this.contextSym, argument, out rval);
        }

        //------------------------------------------------------------
        // AttrBind.GetValue (4)
        //------------------------------------------------------------
        protected bool GetValue(EXPR argument,out TYPESYM type)
        {
            return compiler.FuncBRec.GetAttributeValue(this.contextSym, argument, out type);
        }

        //------------------------------------------------------------
        // AttrBind.GetNamedArgumentName
        //
        /// <summary>
        /// Return SYM.Name of MEMBVARSYM or PROPSYM.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected static string GetNamedArgumentName(EXPR expr)
        {
            DebugUtil.Assert(expr.Kind == EXPRKIND.ASSG);

            expr = expr.AsBIN.Operand1;

            switch (expr.Kind)
            {
                case EXPRKIND.FIELD:
                    return (expr as EXPRFIELD).FieldWithType.FieldSym.Name;

                case EXPRKIND.PROP:
                    return (expr as EXPRPROP).SlotPropWithType.PropSym.Name;

                default:
                    DebugUtil.Assert(false, "bad expr kind");
                    return null;
            };
        }

        //------------------------------------------------------------
        // AttrBind.GetNamedArgumentValue
        //
        /// <summary>
        /// Return EXPRBINOP.Operand2
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        protected static EXPR GetNamedArgumentValue(EXPR expr)
        {
            DebugUtil.Assert(expr.Kind == EXPRKIND.ASSG);
            return expr.AsBIN.Operand2;
        }

        //protected    static STRCONST *   getKnownString(EXPR *expr);
        //protected    static bool         getKnownBool(EXPR *expr);

        //------------------------------------------------------------
        // AttrBind.VerifyAttrArg
        //
        /// <summary>
        /// <para>Returns false on error.</para>
        /// </summary>
        /// <param name="argExpr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        private bool VerifyAttrArg(EXPR argExpr)
        {
            TYPESYM typeSym = argExpr.TypeSym;

        REDO:
            switch (typeSym.Kind)
            {
                case SYMKIND.AGGTYPESYM:
                    {
                        if ((typeSym as AGGTYPESYM).AllTypeArguments.Count > 0)
                        {
                            compiler.Error(argExpr.TreeNode, CSCERRID.ERR_BadAttributeParam);
                            return false;
                        }

                        AGGSYM aggSym = typeSym.GetAggregate();
                        if (aggSym.IsEnum)
                        {
                            // REVIEW ShonK: What are these UNDONEs about?
                            // Where do we verify that the enum is public as stated in section 17.1.3?

                            typeSym = typeSym.UnderlyingType();
                            goto REDO;
                        }
                        else if (
                            !aggSym.IsPredefinedType ||
                            BSYMMGR.GetAttrArgSize(aggSym.PredefinedTypeID) == 0)
                        {
                            compiler.Error(argExpr.TreeNode, CSCERRID.ERR_BadAttributeParam);
                            return false;
                        }

                        if (BSYMMGR.GetAttrArgSize(aggSym.PredefinedTypeID) < 0)
                        {
                            // one of System.Object, System.String, System.Type

                            switch (aggSym.PredefinedTypeID)
                            {
                                case PREDEFTYPE.STRING:
                                    if (argExpr.Kind != EXPRKIND.CONSTANT)
                                    {
                                        compiler.Error(argExpr.TreeNode, CSCERRID.ERR_BadAttributeParam);
                                        return false;
                                    }
                                    break;

                                case PREDEFTYPE.OBJECT:
                                    if (argExpr.Kind != EXPRKIND.CAST)
                                    {
                                        if (argExpr.Kind == EXPRKIND.CONSTANT)
                                        {
                                            DebugUtil.Assert(argExpr.IsNull());
                                            break;
                                        }
                                        compiler.Error(argExpr.TreeNode, CSCERRID.ERR_BadAttributeParam);
                                        return false;
                                    }

                                    // implicit cast of something to object
                                    // need to encode the underlying object(enum, typeSym, string)
                                    return VerifyAttrArg((argExpr as EXPRCAST).Operand);

                                case PREDEFTYPE.TYPE:
                                    switch (argExpr.Kind)
                                    {
                                        case EXPRKIND.TYPEOF:
                                            {
                                                TYPESYM srcTypeSym
                                                    = (argExpr as EXPRTYPEOF).SourceTypeSym;
                                                // Can't contain type variables!
                                                if ((argExpr.Flags & EXPRFLAG.OPENTYPE) == 0 &&
                                                    BSYMMGR.TypeContainsTyVars(srcTypeSym, null))
                                                {
                                                    compiler.ErrorRef(
                                                        argExpr.TreeNode,
                                                        CSCERRID.ERR_AttrArgWithTypeVars,
                                                        new ErrArgRef(srcTypeSym));
                                                    return false;
                                                }
                                            }
                                            break;

                                        case EXPRKIND.CONSTANT:
                                            break;

                                        default:
                                            compiler.Error(
                                                argExpr.TreeNode,
                                                CSCERRID.ERR_BadAttributeParam);
                                            return false;
                                    }
                                    break;

                                default:
                                    DebugUtil.Assert(false, "bad variable size attribute argument type");
                                    break;
                            }
                        }
                        else if (argExpr.Kind != EXPRKIND.CONSTANT)
                        {
                            compiler.Error(argExpr.TreeNode, CSCERRID.ERR_BadAttributeParam);
                            return false;
                        }
                        break;
                    }

                case SYMKIND.ARRAYSYM:
                    {
                        ARRAYSYM arraySym;
                        arraySym = typeSym as ARRAYSYM;

                        if (arraySym.Rank != 1 ||                   // Single Dimension
                            arraySym.ElementTypeSym.IsARRAYSYM ||	// Of non-array
                            !compiler.ClsDeclRec.IsAttributeType(arraySym.ElementTypeSym) ||    // valid attribute argument type
                            (argExpr.Kind != EXPRKIND.ARRINIT && argExpr.Kind != EXPRKIND.CONSTANT))	// which is constant
                        {
                            compiler.Error(argExpr.TreeNode, CSCERRID.ERR_BadAttributeParam);
                            return false;
                        }

                        if (argExpr.Kind == EXPRKIND.ARRINIT)
                        {
                            EXPR  paramExpr;
                            EXPR nextExpr = (argExpr as EXPRARRINIT).ArgumentsExpr;
                            while (nextExpr!=null)
                            {
                                if (nextExpr.Kind == EXPRKIND.LIST)
                                {
                                    paramExpr = nextExpr.AsBIN.Operand1;
                                    nextExpr = nextExpr.AsBIN.Operand2;
                                }
                                else
                                {
                                    paramExpr = nextExpr;
                                    nextExpr = null;
                                }
                                if (!VerifyAttrArg(paramExpr)) return false;
                            }
                        }
                        else
                        {
                            DebugUtil.Assert(argExpr.Kind == EXPRKIND.CONSTANT);
                        }

                        // can't use array types in CLS compliant attributes
                        if (!isEarly &&
                            compiler.AllowCLSErrors() &&
                            compiler.CheckSymForCLS(sym, false))
                        {
                            compiler.Error(
                                argExpr.TreeNode,
                                CSCERRID.WRN_CLS_ArrayArgumentToAttribute);
                        }
                        break;
                    }

                default:
                    compiler.Error(argExpr.TreeNode, CSCERRID.ERR_BadAttributeParam);
                    return false;
            }

            return true;
        }

        //------------------------------------------------------------
        // AttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        virtual internal void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            throw new Exception("AttrBind.EmitCustomAttribute must be overridden.");
        }
    }

    //======================================================================
    // class DefaultAttrBind
    //
    /// <summary>
    /// Default attribute binder. CompileEarly handles obsolete and cls.
    /// </summary>
    //======================================================================
    internal class DefaultAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // DefaultAttrBind.CompileEarly
        //------------------------------------------------------------
        public static void CompileEarly(COMPILER compiler, SYM sym)
        {
            BASENODE nodeAttrs = null;

            if (!sym.IsAGGSYM)
            {
                nodeAttrs = sym.GetAttributesNode();
                if (nodeAttrs == null) return;
            }

            DefaultAttrBind attrbind = new DefaultAttrBind(compiler, sym, true);

            if (sym.IsAGGSYM)
            {
                // AGGSYMs must accumulate from all the declarations.
                attrbind.ProcessAll(sym as AGGSYM, null);
            }
            else
            {
                attrbind.ProcessAll(nodeAttrs, null);
            }
        }

        //------------------------------------------------------------
        // DefaultAttrBind.CompileAndEmit (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal static void CompileAndEmit(COMPILER compiler, SYM sym)
        {
            DebugUtil.Assert(!sym.IsAGGSYM); // Can't call getAttributesNode on AGGSYMs.

            BASENODE nodeAttrs = sym.GetAttributesNode();
            if (nodeAttrs == null)
            {
                return;
            }

            UnknownAttrBind.Compile(compiler, sym.ContainingDeclaration(), nodeAttrs);
            DefaultAttrBind attrbind = new DefaultAttrBind(compiler, sym, false);
            attrbind.ProcessAll(nodeAttrs, null);
        }

        //------------------------------------------------------------
        // DefaultAttrBind Constructor
        //------------------------------------------------------------
        protected DefaultAttrBind(COMPILER compiler, SYM sym, bool early)
            : base(compiler, early)
        {
            Init(sym);
        }

        //------------------------------------------------------------
        // DefaultAttrBind.BindAttr
        //
        /// <summary></summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override protected bool BindAttr(ATTRNODE attr)
        {
            if (!this.isEarly ||
                this.predefinedAttribute == PREDEFATTR.OBSOLETE ||
                this.predefinedAttribute == PREDEFATTR.CLSCOMPLIANT)
            {
                return base.BindAttr(attr);
            }
            return false;
        }

        //------------------------------------------------------------
        // DefaultAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (sym is EVENTSYM)
            {
                EventBuilder eventBuilder = (sym as EVENTSYM).EventBuilder;
                if (caBuilder != null && eventBuilder != null)
                {
                    try
                    {
                        eventBuilder.SetCustomAttribute(caBuilder);
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            else
            {
                throw new NotImplementedException("DefaultAttrBind.EmitCustomAttribute");
            }
        }
    }

    //======================================================================
    // class EarlyGlobalAttrBind
    //
    /// <summary>
    /// Early attribute binder for global attributes.
    /// </summary>
    //======================================================================
    internal class EarlyGlobalAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // EarlyGlobalAttrBind.Compile (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="globalAttributes"></param>
        //------------------------------------------------------------
        public static void Compile(COMPILER compiler, GLOBALATTRSYM globalAttributes)
        {
            // do we have attributes

            if (globalAttributes == null)
            {
                return;
            }

            EarlyGlobalAttrBind attrbind = new EarlyGlobalAttrBind(compiler, globalAttributes);
            while (true)
            {
                int ec = compiler.ErrorCount();
                // Only compile the assembly-level attributes
                // and the ones that don't already have errors
                if (!globalAttributes.HadAttributeError)
                {
                    attrbind.ProcessAll(globalAttributes.ParseTreeNode, null);
                }
                if (ec != compiler.ErrorCount())
                {
                    // Set to indicate that we got an error
                    // processing this attribute and we shouldn't process it again
                    DebugUtil.Assert(!globalAttributes.HadAttributeError);
                    globalAttributes.HadAttributeError = true;
                }
                globalAttributes = globalAttributes.NextAttributeSym;
                if (globalAttributes == null)
                {
                    break;
                }
                attrbind.Init(globalAttributes);
            }
        }

        //------------------------------------------------------------
        // EarlyGlobalAttrBind Constructor (protected)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="globalAttributes"></param>
        //------------------------------------------------------------
        protected EarlyGlobalAttrBind(COMPILER compiler, GLOBALATTRSYM globalAttributes) :
            base(compiler, true)
        {
            Init(globalAttributes);
        }

        //------------------------------------------------------------
        // EarlyGlobalAttrBind.BindAttr
        //
        /// <summary></summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override protected bool BindAttr(ATTRNODE attr)
        {
            if (predefinedAttribute == PREDEFATTR.CLSCOMPLIANT ||
                predefinedAttribute == PREDEFATTR.DEFAULTCHARSET ||
                predefinedAttribute == PREDEFATTR.COMPILATIONRELAXATIONS ||
                predefinedAttribute == PREDEFATTR.RUNTIMECOMPATIBILITY ||
                predefinedAttribute == PREDEFATTR.FRIENDASSEMBLY /*||
                predefinedAttribute == PREDEFATTR.KEYFILE ||
                predefinedAttribute == PREDEFATTR.KEYNAME ||
                predefinedAttribute == PREDEFATTR.DELAYSIGN*/)
            {
                return base.BindAttr(attr);
            }

            CAssemblyInitialAttributes asmInitAttrs = this.compiler.AssemblyInitialAttributes;
            if (asmInitAttrs != null)
            {
                CAssemblyInitialAttributes.Kind kind =
                    CAssemblyInitialAttributes.NeedToInit(this.attributeTypeSym.GetAggregate().Type);

                if (kind != CAssemblyInitialAttributes.Kind.NotInitialData)
                {
                    if (!base.BindAttr(attr))
                    {
                        return false;
                    }

                    EXPRCALL callExpr = this.ctorExpr as EXPRCALL;
                    EXPR argExpr = (callExpr != null) ? callExpr.ArgumentsExpr : null;
                    EXPRCONSTANT constExpr = argExpr as EXPRCONSTANT;
                    bool br = false;
                    Exception excp = null;
                    BASENODE errorNode = null;

                    switch (kind)
                    {
                        case CAssemblyInitialAttributes.Kind.HashAlgorithm:
                            if (constExpr != null &&
                                constExpr.ConstVal != null)
                            {
                                object obj = constExpr.ConstVal.GetObject();
                                if (obj.GetType() == typeof(System.Configuration.Assemblies.AssemblyHashAlgorithm))
                                {
                                    br = asmInitAttrs.SetHashAlgorithm(
                                        (System.Configuration.Assemblies.AssemblyHashAlgorithm)obj);
                                    if (br)
                                    {
                                        attr.Emitted = true;
                                    }
                                    else
                                    {
                                        errorNode = constExpr.TreeNode;
                                    }
                                }
                            }
                            break;

                        case CAssemblyInitialAttributes.Kind.Culture:
                            if (constExpr != null &&
                                constExpr.ConstVal != null &&
                                constExpr.ConstVal.IsString)
                            {
                                br = asmInitAttrs.SetCulture(constExpr.ConstVal.GetString(), out excp);
                                if (br)
                                {
                                    attr.Emitted = true;
                                }
                                else
                                {
                                    errorNode = constExpr.TreeNode;
                                }
                            }
                            break;

                        case CAssemblyInitialAttributes.Kind.DelaySign:
                            if (constExpr != null &&
                                constExpr.ConstVal != null &&
                                constExpr.ConstVal.IsBool)
                            {
                                br = asmInitAttrs.SetDelaySign(constExpr.ConstVal.GetBool(), out excp);
                                if (br)
                                {
                                    //attr.Emitted = true;
                                }
                                else
                                {
                                    errorNode = constExpr.TreeNode;
                                }
                            }
                            break;

                        case CAssemblyInitialAttributes.Kind.KeyFile:
                            if (constExpr != null &&
                                constExpr.ConstVal != null &&
                                constExpr.ConstVal.IsString)
                            {
                                br = asmInitAttrs.SetKeyFile(constExpr.ConstVal.GetString(), out excp);
                                if (br)
                                {
                                    //attr.Emitted = true;
                                }
                                else
                                {
                                    errorNode = constExpr.TreeNode;
                                }
                            }
                            break;

                        case CAssemblyInitialAttributes.Kind.KeyName:
                            if (constExpr != null &&
                                constExpr.ConstVal != null &&
                                constExpr.ConstVal.IsString)
                            {
                                br = asmInitAttrs.SetKeyName(constExpr.ConstVal.GetString(), out excp);
                                if (br)
                                {
                                    //attr.Emitted = true;
                                }
                                else
                                {
                                    errorNode = constExpr.TreeNode;
                                }
                            }
                            break;

                        case CAssemblyInitialAttributes.Kind.Version:
                            if (constExpr != null &&
                                constExpr.ConstVal != null &&
                                constExpr.ConstVal.IsString)
                            {
                                br = asmInitAttrs.SetVersion(constExpr.ConstVal.GetString(), out excp);
                                if (br)
                                {
                                    attr.Emitted = true;
                                }
                                else
                                {
                                    errorNode = constExpr.TreeNode;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                    if (!br)
                    {
                        // show an error message.
                        if (excp != null)
                        {
                            this.compiler.Error(errorNode, ERRORKIND.ERROR, excp);
                        }
                        else
                        {
                            this.compiler.Error(errorNode, ERRORKIND.ERROR, "internal compiler error");
                        }
                    }
                }
            }

            return false;
        }

        //------------------------------------------------------------
        // EarlyGlobalAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            OUTFILESYM outfileSym;
            EXPR nodeExpr;
            EXPR argExpr;

            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.DEFAULTCHARSET:
                    outfileSym = sym.GetInputFile().GetOutFileSym();

                    DebugUtil.Assert(attrTarget == ATTRTARGET.MODULE);
                    DebugUtil.Assert(outfileSym != null);

                    nodeExpr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
                    while (nodeExpr != null)
                    {
                        if (nodeExpr.Kind == EXPRKIND.LIST)
                        {
                            argExpr = nodeExpr.AsBIN.Operand1;
                            nodeExpr = nodeExpr.AsBIN.Operand2;
                        }
                        else
                        {
                            argExpr = nodeExpr;
                            nodeExpr = null;
                        }
                        int charSet;
                        GetValue(argExpr, out charSet);
                        if (charSet < 0 || charSet > 4)
                        {
                            compiler.Error(
                                argExpr.TreeNode,
                                CSCERRID.ERR_InvalidDefaultCharSetValue);
                            outfileSym.DefaultCharSet = 0;
                        }
                        outfileSym.DefaultCharSet
                            = (System.Runtime.InteropServices.CharSet)charSet;
                        break;
                    }
                    break;

                case PREDEFATTR.COMPILATIONRELAXATIONS:
                    if (attrTarget == ATTRTARGET.ASSEMBLY)
                    {
                        compiler.SuppressRelaxations();
                    }
                    break;

                case PREDEFATTR.RUNTIMECOMPATIBILITY:
                    if (attrTarget == ATTRTARGET.ASSEMBLY)
                    {
                        compiler.SuppressRuntimeCompatibility();
                        nodeExpr = this.namedArgumentsExpr;
                        while (nodeExpr != null)
                        {
                            if (nodeExpr.Kind == EXPRKIND.LIST)
                            {
                                argExpr = nodeExpr.AsBIN.Operand1;
                                nodeExpr = nodeExpr.AsBIN.Operand2;
                            }
                            else
                            {
                                argExpr = nodeExpr;
                                nodeExpr = null;
                            }
                            string name = GetNamedArgumentName(argExpr);
                            EXPR value = GetNamedArgumentValue(argExpr);
                            bool val = true;

                            if (name == GetPredefName(PREDEFNAME.WRAPNONEXCEPTIONTHROWS) &&
                                GetValue(value, out val) &&
                                val == false)
                            {
                                compiler.SuppressWrapNonExceptionThrows();
                            }
                        }
                    }
                    break;

                case PREDEFATTR.FRIENDASSEMBLY:
                    compiler.IsFriendDeclared = true;
                    break;

                case PREDEFATTR.KEYFILE:
                case PREDEFATTR.KEYNAME:
                case PREDEFATTR.DELAYSIGN:
                    // If there is a manifest then we set the flag on it
                    // since any module linked into that manifest with
                    // any of these attributes will cause ALink to sign the final manifest.
                    if (compiler.GetManifestOutFile()!=null)
                    {
                        compiler.GetManifestOutFile().HasSigningAttribute = true;
                    }
                    else
                    {
                        outfileSym = sym.GetInputFile().GetOutFileSym();
                        outfileSym.HasSigningAttribute = true;
                    }
                    break;

                default:
                    // does nothing for other attributes early
                    break;
            }

            base.VerifyAndEmitPredef(attrNode);
        }
    }

    //======================================================================
    // class GlobalAttrBind
    //
    /// <summary>
    /// Attribute binder for global attributes.
    /// </summary>
    //======================================================================
    internal class GlobalAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // GlobalAttrBind Fields and Properties
        //------------------------------------------------------------
        protected GLOBALATTRSYM globalAttributeSym = null;  // * globalAttribute;
        protected bool isModule = false;                    //protected mdToken tokenEmit;
        internal bool HadDebuggable = false;                // fHadDebuggable;

        //------------------------------------------------------------
        // GlobalAttrBind.Compile (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="contextSym"></param>
        /// <param name="globalAttrSym"></param>
        /// <param name="isModule"></param>
        //------------------------------------------------------------
        static internal void Compile(
            COMPILER compiler,
            PARENTSYM contextSym,
            GLOBALATTRSYM globalAttrSym,
            bool isModule, 	//mdToken tokenEmit
            Dictionary<SecurityAction,PermissionSet> permissionSets)
        {
            GlobalAttrBind attrbind = null;
            bool hadDebuggable = false;

            // do we have attributes

            if (globalAttrSym != null)
            {
                attrbind = new GlobalAttrBind(compiler, globalAttrSym, isModule);
                while (true)
                {
                    int ec = compiler.ErrorCount();
                    if (!globalAttrSym.HadAttributeError)
                    {
                        if (globalAttrSym.ParseTreeNode != null)
                        {
                            attrbind.ProcessAll(globalAttrSym.ParseTreeNode, permissionSets);
                        }
                        else if (!String.IsNullOrEmpty(globalAttrSym.AttributeName))
                        {
                            if (!compiler.OptionManager.NoStdLib)
                            {
                                attrbind.ProcessAssemblyAttribute(
                                    globalAttrSym.AttributeName,
                                    globalAttrSym.PositionalArguments);
                            }
                        }
                    }

                    if (ec != compiler.ErrorCount())
                    {
                        // mark that we got an error processing this attribute
                        // and we shouldn't process it again
                        // NOTE: this interacts with similar loop in EarlyGlobalAttrBind::Compile()
                        DebugUtil.Assert(!globalAttrSym.HadAttributeError);
                        globalAttrSym.HadAttributeError = true;
                    }

                    globalAttrSym = globalAttrSym.NextAttributeSym;
                    if (globalAttrSym == null)
                    {
                        break;
                    }
                    attrbind.Init(globalAttrSym);
                }
                hadDebuggable = attrbind.HadDebuggable;
            }

            if (permissionSets != null)
            {
                return;
            }

            //--------------------------------------------------
            // synthetized attributes
            //--------------------------------------------------
            attrbind = new GlobalAttrBind(compiler, isModule, contextSym);
            if (compiler.OptionManager.Unsafe)
            {
                //if (TypeFromToken(tokenEmit) == mdtModule)
                if (isModule)
                {
                    if (compiler.GetOptPredefType(PREDEFTYPE.UNVERIFCODEATTRIBUTE, false) != null)
                    {
                        attrbind.ProcessSynthAttr(
                            compiler.GetOptPredefType(PREDEFTYPE.UNVERIFCODEATTRIBUTE, true),
                            compiler.FuncBRec.BindSimplePredefinedAttribute(
                                PREDEFTYPE.UNVERIFCODEATTRIBUTE),
                            null);
                    }
                }
                else
                {
                    if (compiler.GetOptPredefType(PREDEFTYPE.UNVERIFCODEATTRIBUTE, false) != null &&
                        compiler.GetOptPredefType(PREDEFTYPE.SECURITYPERMATTRIBUTE, false) != null &&
                        compiler.GetOptPredefType(PREDEFTYPE.SECURITYACTION, false) != null)
                    {
                        attrbind.ProcessSynthAttr(
                            compiler.GetOptPredefType(PREDEFTYPE.SECURITYPERMATTRIBUTE, true),
                            compiler.FuncBRec.BindSimplePredefinedAttribute(
                                PREDEFTYPE.SECURITYPERMATTRIBUTE,
                                compiler.FuncBRec.BindSkipVerifyArgs()),
                            compiler.FuncBRec.BindSkipVerifyNamedArgs());
                    }
                }
            }

            //if (TypeFromToken(tokenEmit) != mdtModule)
            if (!isModule)
            {
                if (!hadDebuggable && compiler.BuildAssembly)
                {
                    EXPR args = compiler.FuncBRec.BindDebuggableArgs();
                    // This returns null if the args are the default values (meaning no attribute is needed)
                    if (args != null)
                    {
                        attrbind.ProcessSynthAttr(
                            compiler.GetOptPredefType(PREDEFTYPE.DEBUGGABLEATTRIBUTE, true),
                            compiler.FuncBRec.BindSimplePredefinedAttribute(PREDEFTYPE.DEBUGGABLEATTRIBUTE, args),
                            null);
                    }
                }

                // Emit the CompilationRelaxationsAttribute
                if (compiler.EmitRelaxations() && compiler.GetOptPredefType(PREDEFTYPE.COMPILATIONRELAXATIONS, false) != null)
                {
                    attrbind.ProcessSynthAttr(
                        compiler.GetOptPredefType(PREDEFTYPE.COMPILATIONRELAXATIONS, true),
                        compiler.FuncBRec.BindSimplePredefinedAttribute(
                            PREDEFTYPE.COMPILATIONRELAXATIONS,
                            compiler.FuncBRec.BindCompilationRelaxationsAttributeArgs()),
                        null);
                }

                // Emit the RuntimeCompatibilityAttribute
                if (compiler.EmitRuntimeCompatibility())
                {
                    attrbind.ProcessSynthAttr(
                        compiler.GetOptPredefType(PREDEFTYPE.RUNTIMECOMPATIBILITY, true),
                        compiler.FuncBRec.BindSimplePredefinedAttribute(PREDEFTYPE.RUNTIMECOMPATIBILITY, null),
                        compiler.FuncBRec.BindRuntimeCompatibilityAttributeNamedArgs());
                }
            }
        }

        //------------------------------------------------------------
        // GlobalAttrBind Constructor (1)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="globalAttributes"></param>
        /// <param name="isModule"></param>
        //------------------------------------------------------------
        protected GlobalAttrBind(
            COMPILER compiler,
            GLOBALATTRSYM globalAttributes,
            bool isModule   //mdToken tokenEmit
            ) :
            base(compiler, false)
        {
            this.globalAttributeSym = globalAttributes;
            this.isModule = isModule;   //this.tokenEmit = tokenEmit;
            HadDebuggable = false;
            Init(globalAttributeSym);
        }

        //------------------------------------------------------------
        // GlobalAttrBind Constructor (2)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="isModule"></param>
        /// <param name="context"></param>
        //------------------------------------------------------------
        protected GlobalAttrBind(
            COMPILER compiler,
            bool isModule,   //mdToken tokenEmit,
            PARENTSYM context) :
            base(compiler, false)
        {
            globalAttributeSym = null;
            HadDebuggable = false;
            this.isModule = isModule;  //this.tokenEmit = tokenEmit;
            //DebugUtil.Assert(TypeFromToken(tokenEmit) == mdtAssembly || TypeFromToken(tokenEmit) == mdtModule);
            //Init((TypeFromToken(tokenEmit) == mdtAssembly) ? catAssembly : catModule, context);
            Init((this.isModule ? AttributeTargets.Module : AttributeTargets.Assembly), context);
        }

        //------------------------------------------------------------
        // GlobalAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            BASENODE nameNode = attrNode.NameNode;
            string optionName = null;
            EXPR nodeExpr;
            EXPR argExpr;

            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.KEYFILE:
                    optionName = "keyfile";
                    //optionName = COptionData.GetOptionDef2(
                    //    COptionData.GetOptionIndex(CommandOptionID.CSC_KeyFile)).DescSwitch;
                    break;

                case PREDEFATTR.KEYNAME:
                    optionName = "keycontainer";
                    //optionName = COptionData.GetOptionDef2(
                    //    COptionData.GetOptionIndex(CommandOptionID.CSC_KeyContainer)).DescSwitch;
                    break;

                case PREDEFATTR.DELAYSIGN:
                    optionName = "delaysign";
                    //optionName = COptionData.GetOptionDef2(
                    //    COptionData.GetOptionIndex(CommandOptionID.CSC_DelaySign)).DescSwitch;
                    break;

                case PREDEFATTR.DEBUGGABLE:
                    HadDebuggable = true;
                    goto SKIP_WARNING;

                case PREDEFATTR.FRIENDASSEMBLY:
                    // Verify the attribute contains a well-formed assembly reference
                    string strValue = null;
                    nodeExpr = (this.ctorExpr as EXPRCALL).ArgumentsExpr;
                    while (nodeExpr != null)
                    {
                        if (nodeExpr.Kind == EXPRKIND.LIST)
                        {
                            argExpr = nodeExpr.AsBIN.Operand1;
                            nodeExpr = nodeExpr.AsBIN.Operand2;
                        }
                        else
                        {
                            argExpr = nodeExpr;
                            nodeExpr = null;
                        }
                        DebugUtil.Assert(strValue == null);
                        // how did we get two parameters to the InternalsVisibleToAttribute?
                        GetValue(argExpr, out strValue);
                        //WCHAR *szValue = STACK_ALLOC (WCHAR, strValue.length + 1);
                        //StringCchCopyNW(szValue, strValue.length+1, strValue.text, strValue.length);
                        // the Strsafe API will null terminate.
                        if (!compiler.Importer.CheckFriendAssemblyName(
                            argExpr.TreeNode, strValue, sym.GetInputFile().GetOutFileSym()))
                        {
                            return;
                        }
                        goto SKIP_WARNING;
                    }
                    goto SKIP_WARNING;

                default:
                    goto SKIP_WARNING;
            }

            // Only report the warning if they have non-default values
            nodeExpr = (ctorExpr as EXPRCALL).ArgumentsExpr;
            while (nodeExpr != null)
            {
                if (nodeExpr.Kind == EXPRKIND.LIST)
                {
                    argExpr = nodeExpr.AsBIN.Operand1;
                    nodeExpr = nodeExpr.AsBIN.Operand2;
                }
                else
                {
                    argExpr = nodeExpr;
                    nodeExpr = null;
                }
                if (argExpr.TypeSym.IsPredefType(PREDEFTYPE.BOOL))
                {
                    bool value;
                    if (!GetValue(argExpr, out value) || !value)
                    {
                        goto SKIP_WARNING;
                    }
                }
                else if (argExpr.TypeSym.IsPredefType(PREDEFTYPE.STRING))
                {
                    string value = null;
                    if (!GetValue(argExpr, out value) || String.IsNullOrEmpty(value))
                    {
                        goto SKIP_WARNING;
                    }
                }
            }
            compiler.Error(
                nameNode,
                CSCERRID.WRN_UseSwitchInsteadOfAttribute,
                new ErrArg(optionName),
                new ErrArgNameNode(nameNode, ErrArgFlagsEnum.None));

        SKIP_WARNING:
            if (!attrNode.Emitted)
            {
                base.VerifyAndEmitPredef(attrNode);
            }
        }

        //------------------------------------------------------------
        // GlobalAttrBind.GetToken
        //------------------------------------------------------------
        //protected mdToken GetToken() { return tokenEmit; }

        //------------------------------------------------------------
        // GlobalAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null)
            {
                return;
            }
            OUTFILESYM outfileSym;

            switch (this.contextSym.Kind)
            {
                case SYMKIND.NSDECLSYM:
                    outfileSym = this.contextSym.GetInputFile().GetOutFileSym();
                    break;

                default:
                    outfileSym = this.contextSym as OUTFILESYM;
                    break;
            }

            if (outfileSym == null || outfileSym.AssemblyBuilderEx == null)
            {
                return;
            }
            outfileSym.AssemblyBuilderEx.SetCustomAttribute(caBuilder);
        }

        //------------------------------------------------------------
        // GlobalAttrBind.ProcessAssemblyAttribute
        //
        /// <summary>
        /// Emit assembly attributes specified by options.
        /// </summary>
        /// <param name="attrName"></param>
        /// <param name="posArgs"></param>
        //------------------------------------------------------------
        internal void ProcessAssemblyAttribute(
            string attrName,
            object[] posArgs)
        {
            CAssemblyEx msCorLib =compiler.MainSymbolManager.MsCorLibSym.AssemblyEx;
            Type type = msCorLib.GetType(attrName);
            if (type == null)
            {
                return;
            }
            Type[] argTypes = null;
            if (posArgs.Length > 0)
            {
                argTypes = new Type[posArgs.Length];
                for (int i = 0; i < posArgs.Length; ++i)
                {
                    argTypes[i] = posArgs[i].GetType();
                }
            }

            ConstructorInfo cInfo = null;
            try
            {
                cInfo = type.GetConstructor(argTypes);
            }
            catch (ArgumentException ex)
            {
                compiler.Controller.ReportError(ERRORKIND.ERROR, ex);
                return;
            }

            CustomAttributeBuilder caBuilder =
                ReflectionUtil.CreateCustomAttributeBuilder(
                    cInfo,
                    posArgs,
                    null, null, null, null);
            if (caBuilder != null)
            {
                EmitCustomAttribute(caBuilder);
            }
        }
    }

    //======================================================================
    // class EarlyAggAttrBind
    //
    /// <summary>
    /// <para>Early attribute binder for AGGSYMs.</para>
    /// <para>This handles: usage, obsolete, cls, coclass, comimport, conditional</para>
    /// </summary>
    //======================================================================
    internal class EarlyAggAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // EarlyAggAttrBind Fields and Properties
        //------------------------------------------------------------
        protected AGGSYM AggSym = null;                 //* cls;
        protected List<string> ConditionList = null;    //NAMELIST** pnlstCond;

        //------------------------------------------------------------
        // EarlyAggAttrBind.Compile (static)
        //------------------------------------------------------------
        static public void Compile(COMPILER compiler, AGGSYM cls)
        {
            // do we have attributes

            EarlyAggAttrBind attrbind = new EarlyAggAttrBind(compiler, cls);
            attrbind.ProcessAll(cls, null);
        }

        //------------------------------------------------------------
        // EarlyAggAttrBind Constructor
        //------------------------------------------------------------
        protected EarlyAggAttrBind(COMPILER compiler, AGGSYM agg)
            : base(compiler, true)
        {
            this.AggSym = agg;
            this.ConditionList = this.AggSym.ConditionalSymbolNameList;
            Init(agg);
        }

        //------------------------------------------------------------
        // EarlyAggAttrBind.BindAttr
        //
        /// <summary></summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override protected bool BindAttr(ATTRNODE attr)
        {
            // Primarily attributes on attributes.
            switch (this.predefinedAttribute)
            {
                default:
                    if (this.attributeTypeSym == null ||
                        !attributeTypeSym.IsSecurityAttribute())
                    {
                        return false;
                    }
                    break;

                case PREDEFATTR.ATTRIBUTEUSAGE:
                case PREDEFATTR.OBSOLETE:
                case PREDEFATTR.CLSCOMPLIANT:
                case PREDEFATTR.COCLASS:
                case PREDEFATTR.COMIMPORT:
                case PREDEFATTR.CONDITIONAL:
                    break;
            }

            return base.BindAttr(attr);
        }

        //------------------------------------------------------------
        // EarlyAggAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            EXPR nodeExpr;
            EXPR argExpr;

            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.COCLASS:
                    ProcessCoClass(attrNode);
                    return;

                case PREDEFATTR.COMIMPORT:
                    this.AggSym.IsComImport = true;
                    return;

                case PREDEFATTR.CONDITIONAL:
                    if (!this.AggSym.IsAttribute)
                    {
                        compiler.Error(
                            attrNode.NameNode,
                            CSCERRID.ERR_ConditionalOnNonAttributeClass,
                            new ErrArg(this.attributeTypeSym));
                    }
                    break;

                case PREDEFATTR.ATTRIBUTEUSAGE:
                    BASENODE nameNode = attrNode.NameNode;
                    if (this.AggSym.AttributeClass != 0)
                    {
                        if (this.AggSym.PredefinedTypeID != PREDEFTYPE.ATTRIBUTEUSAGE &&
                            this.AggSym.PredefinedTypeID != PREDEFTYPE.CONDITIONAL &&
                            this.AggSym.PredefinedTypeID != PREDEFTYPE.OBSOLETE &&
                            this.AggSym.PredefinedTypeID != PREDEFTYPE.CLSCOMPLIANT)
                        {
                            compiler.Error(nameNode, CSCERRID.ERR_DuplicateAttribute,
                                new ErrArgNameNode(nameNode, ErrArgFlagsEnum.None));
                            return;
                        }

                        // attributeusage, conditional and obsolete attributes are 'special'
                        this.AggSym.AttributeClass = (AttributeTargets)0;
                    }
                    bool foundAllowMultiple = false;

                    nodeExpr = (ctorExpr as EXPRCALL).ArgumentsExpr;
                    while (nodeExpr != null)
                    {
                        if (nodeExpr.Kind == EXPRKIND.LIST)
                        {
                            argExpr = nodeExpr.AsBIN.Operand1;
                            nodeExpr = nodeExpr.AsBIN.Operand2;
                        }
                        else
                        {
                            argExpr = nodeExpr;
                            nodeExpr = null;
                        }
                        if (this.AggSym.AttributeClass != 0)
                        {
                            // using a constructor which should be removed
                            compiler.Error(
                                attrNode,
                                CSCERRID.ERR_DeprecatedSymbolStr,
                                new ErrArg((this.ctorExpr as EXPRCALL).MethodWithInst),
                                new ErrArg("Use single argument contsructor instead"));
                            return;
                        }
                        int val;
                        if (GetValue(argExpr, out val))
                        {
                            if (val == 0 || (val & ~((int)AttributeTargets.All)) != 0)
                            {
                                compiler.Error(
                                    argExpr.TreeNode,
                                    CSCERRID.ERR_InvalidAttributeArgument,
                                    new ErrArgNameNode(attrNode.NameNode, ErrArgFlagsEnum.None));
                                this.AggSym.AttributeClass = AttributeTargets.All;
                            }
                            else
                            {
                                this.AggSym.AttributeClass = (AttributeTargets)val;
                            }
                        }
                    }

                    nodeExpr = this.namedArgumentsExpr;
                    while (nodeExpr != null)
                    {
                        if (nodeExpr.Kind == EXPRKIND.LIST)
                        {
                            argExpr = nodeExpr.AsBIN.Operand1;
                            nodeExpr = nodeExpr.AsBIN.Operand2;
                        }
                        else
                        {
                            argExpr = nodeExpr;
                            nodeExpr = null;
                        }
                        string nameStr = GetNamedArgumentName(argExpr);
                        EXPR valueExpr = GetNamedArgumentValue(argExpr);
                        int val;

                        if (nameStr == GetPredefName(PREDEFNAME.VALIDON))
                        {
                            if (this.AggSym.AttributeClass != 0)
                            {
                                compiler.Error(
                                    argExpr.TreeNode,
                                    CSCERRID.ERR_DuplicateNamedAttributeArgument,
                                    new ErrArg(nameStr));
                            }
                            else if (!GetValue(valueExpr, out val))
                            {
                                // error already reported
                            }
                            else if (val == 0 || (val & ~((int)AttributeTargets.All)) != 0)
                            {
                                compiler.Error(argExpr.TreeNode, CSCERRID.ERR_InvalidNamedArgument,
                                    new ErrArg(nameStr));
                                this.AggSym.AttributeClass = AttributeTargets.All;
                            }
                            else
                            {
                                this.AggSym.AttributeClass = (AttributeTargets)val;
                            }
                        }
                        else if (nameStr == GetPredefName(PREDEFNAME.ALLOWMULTIPLE))
                        {
                            bool isMultiple = false;

                            if (foundAllowMultiple)
                            {
                                compiler.Error(
                                    argExpr.TreeNode,
                                    CSCERRID.ERR_DuplicateNamedAttributeArgument,
                                    new ErrArg(nameStr));
                            }
                            else if (!GetValue(valueExpr, out isMultiple))
                            {
                                // error already reported
                            }
                            else
                            {
                                this.AggSym.IsMultipleAttribute = isMultiple;
                                foundAllowMultiple = true;
                            }
                        }
                        else if (nameStr == GetPredefName(PREDEFNAME.INHERITED))
                        {
                        }
                        else
                        {
                            DebugUtil.Assert(false, "unknown named argExpr to attributeusage attribute");
                        }
                    }

                    // Set default allow on
                    if (this.AggSym.AttributeClass == 0)
                    {
                        this.AggSym.AttributeClass = AttributeTargets.All;
                    }
                    return;

                default:
                    break;
            }

            base.VerifyAndEmitPredef(attrNode);
        }

        //------------------------------------------------------------
        // EarlyAggAttrBind.ValidateAttrs
        //
        /// <summary></summary>
        //------------------------------------------------------------
        override protected void ValidateAttrs()
        {
            this.AggSym.HasLinkDemand = this.hasLinkDemand;

            if (!this.AggSym.IsComImport &&
                this.AggSym.IsInterface &&
                this.AggSym.ComImportCoClass != null)
            {
                compiler.ErrorRef(
                    null,
                    CSCERRID.WRN_CoClassWithoutComImport,
                    new ErrArgRef(this.AggSym));
                this.AggSym.UnderlyingTypeSym = null;
                this.AggSym.ComImportCoClass = null;
            }
        }

        //------------------------------------------------------------
        // EarlyAggAttrBind.AddConditionalName
        //
        /// <summary>
        /// Add a name to List&lt;string&gt;.
        /// </summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        override protected void AddConditionalName(string name)
        {
            this.compiler.MainSymbolManager.AddToGlobalNameList(name, this.ConditionList);
        }

        //------------------------------------------------------------
        // EarlyAggAttrBind.ProcessCoClass
        //
        /// <summary></summary>
        /// <param name="attr"></param>
        //------------------------------------------------------------
        protected void ProcessCoClass(ATTRNODE attr)
        {
            // if the attribute is on an interface, then we do some special processing

            if (this.AggSym.IsInterface)
            {
                // set the baseClass to point to the CoClass
                // and fill in the comImportCoClass string

                EXPR nodeExpr = (ctorExpr as EXPRCALL).ArgumentsExpr;
                while (nodeExpr != null)
                {
                    EXPR argExpr;
                    if (nodeExpr.Kind == EXPRKIND.LIST)
                    {
                        argExpr = nodeExpr.AsBIN.Operand1;
                        nodeExpr = nodeExpr.AsBIN.Operand2;
                    }
                    else
                    {
                        argExpr = nodeExpr;
                        nodeExpr = null;
                    }

                    if (argExpr.TypeSym.IsPredefType(PREDEFTYPE.TYPE))
                    {
                        TYPESYM value = null;
                        if (!GetValue(argExpr, out value))
                        {
                            break;
                        }
                        if (value != null && value.IsClassType())
                        {
                            string name = null;
                            if (MetaDataHelper.GetFullName(value.GetAggregate(), out name))
                            {
                                this.AggSym.UnderlyingTypeSym = value as AGGTYPESYM;
                                this.AggSym.ComImportCoClass = name;
                            }
                        }
                    }
                }
            }
        }
    }

    //======================================================================
    // class AggAttrBind
    //
    //    Attribute binder for AGGSYMs.
    //======================================================================
    internal class AggAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // AggAttrBind Fields and Properties
        //------------------------------------------------------------
        protected AGGSYM aggSym = null;	            // * cls;
        protected AGGINFO aggInfo = null;	        // * info;
        protected string defaultMemberName = null;	// NAME * defaultMemberName;

        //------------------------------------------------------------
        // AggAttrBind.Compile (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="aggSym"></param>
        /// <param name="aggInfo"></param>
        //------------------------------------------------------------
        static internal void Compile(
            COMPILER compiler,
            AGGSYM aggSym,
            AGGINFO aggInfo,
            Dictionary<SecurityAction,PermissionSet> permissionSets)
        {
            string defaultMemberName = null;
            // Do we have any indexers? If so, we must emit a default member name.
            if (aggSym.IsStruct || aggSym.IsClass || aggSym.IsInterface)
            {
                PROPSYM indexer = compiler.MainSymbolManager.LookupAggMember(
                    compiler.NameManager.GetPredefinedName(PREDEFNAME.INDEXERINTERNAL),
                    aggSym,
                    SYMBMASK.PROPSYM) as PROPSYM;
                while (indexer != null)
                {
                    string indexerName = indexer.GetRealName();

                    // All indexers better have the same metadata name. 
                    if (defaultMemberName != null && indexerName != defaultMemberName)
                    {
                        compiler.Error(
                            indexer.GetParseTree(),
                            CSCERRID.ERR_InconsistantIndexerNames);
                    }
                    defaultMemberName = indexerName;

                    indexer = BSYMMGR.LookupNextSym(
                        indexer,
                        aggSym,
                        SYMBMASK.PROPSYM) as PROPSYM;
                }
            }

            // do we have attributes

            // Handle unknown attribute locations.
            for (AGGDECLSYM aggDeclSym = aggSym.FirstDeclSym;
                aggDeclSym != null;
                aggDeclSym = aggDeclSym.NextDeclSym)
            {
                BASENODE attributes = aggDeclSym.GetAttributesNode();
                UnknownAttrBind.Compile(compiler, aggDeclSym.ParentDeclSym, attributes);
            }

            // Bind attributes.
            AggAttrBind attrbind = new AggAttrBind(compiler, aggSym, aggInfo, defaultMemberName);
            attrbind.ProcessAll(aggSym, permissionSets);

            // If we have any indexers in us,
            // emit the "DefaultMember" attribute with the name of the indexer.
            if (defaultMemberName != null)
            {
                attrbind.defaultMemberName = null;
                attrbind.ProcessSynthAttr(
                    compiler.GetReqPredefType(PREDEFTYPE.DEFAULTMEMBER, true),
                    compiler.FuncBRec.BindStringPredefinedAttribute(
                        PREDEFTYPE.DEFAULTMEMBER,
                        defaultMemberName),
                    null);
            }

            //bind attributes on type variables
            if (aggSym.AllTypeVariables.Count > 0)
            {
                TypeVarAttrBind.CompileParamList(
                    compiler,
                    aggSym.FirstDeclSym,
                    aggSym.AllTypeVariables);
                //,aggSym.toksEmitTypeVars);
            }

            attrbind.CompileFabricatedAttr();
        }

        //------------------------------------------------------------
        // AggAttrBind Constructor
        //------------------------------------------------------------
        protected internal AggAttrBind(
            COMPILER compiler,
            AGGSYM agg,
            AGGINFO info,
            string name)
            : base(compiler, false)
        {
            this.aggSym = agg;
            this.aggInfo = info;
            this.defaultMemberName = name;
            Init(this.aggSym);
        }

        //------------------------------------------------------------
        // AggAttrBind.VerifyAndEmitPredef
        //-----------------------------------------------------------
        protected override void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            BASENODE nameNode = attrNode.NameNode;

            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.DEFAULTMEMBER:
                    if (this.defaultMemberName != null)
                    {
                        compiler.Error(
                            attrNode.NameNode,
                            CSCERRID.ERR_DefaultMemberOnIndexedType);
                        return;
                    }
                    break;

                case PREDEFATTR.ATTRIBUTEUSAGE:
                    if (!this.aggSym.IsAttribute)
                    {
                        compiler.Error(
                            nameNode,
                            CSCERRID.ERR_AttributeUsageOnNonAttributeClass,
                            new ErrArgNameNode(nameNode, ErrArgFlagsEnum.None));
                        return;
                    }
                    break;

                case PREDEFATTR.STRUCTLAYOUT:
                    ProcessStructLayout(attrNode);
                    return;

                case PREDEFATTR.COMIMPORT:
                    this.aggInfo.IsComImport = true;
                    this.aggSym.IsComImport = true;
                    if (this.aggSym.IsClass)
                    {
                        if (!this.aggSym.BaseClassSym.IsPredefType(PREDEFTYPE.OBJECT))
                        {
                            compiler.ErrorRef(
                                null,
                                CSCERRID.ERR_ComImportWithBase,
                                new ErrArgRef(this.aggSym));
                        }

                        // Can only have a compiler generated constructor
                        for (METHSYM ctor = compiler.MainSymbolManager.LookupAggMember(
                                GetPredefName(PREDEFNAME.CTOR),
                                this.aggSym,
                                SYMBMASK.METHSYM) as METHSYM;
                            ctor != null;
                            ctor = ctor.NextSameNameSym as METHSYM)
                        {
                            if (ctor.IsCompilerGeneratedCtor)
                            {
                                // Whose implementation is supplied by COM+
                                ctor.IsExternal = true;
                                ctor.IsSysNative = true;

                                // We need to reset the method's flags
                                compiler.Emitter.ResetMethodFlags(ctor);
                            }
                            else
                            {
                                compiler.ErrorRef(
                                    null,
                                    CSCERRID.ERR_ComImportWithUserCtor,
                                    new ErrArgRef(ctor));
                            }
                        }
                    }
                    break;

                case PREDEFATTR.GUID:
                    this.aggInfo.HasUUID = true;
                    break;

                case PREDEFATTR.REQUIRED:
                    // Predefined attribute which is not allowed in C#
                    compiler.Error(
                        nameNode,
                        CSCERRID.ERR_CantUseRequiredAttribute,
                        new ErrArgNameNode(nameNode, ErrArgFlagsEnum.None));
                    return;

                case PREDEFATTR.UNMANAGEDFUNCTIONPOINTER:
                    AddDefaultCharSet();
                    break;

                default:
                    break;
            }

            base.VerifyAndEmitPredef(attrNode);
        }

        //------------------------------------------------------------
        // AggAttrBind.ValidateAttrs
        //
        /// <summary></summary>
        //------------------------------------------------------------
        override protected void ValidateAttrs()
        {
            if (this.aggInfo.IsComImport && !this.aggInfo.HasUUID)
            {
                compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_ComImportWithoutUuidAttribute,
                    new ErrArgRef(this.aggSym));
            }

            // struct layout defaults to sequential for structs,
            // if the user didn't explicitly specify
            if (!this.aggInfo.HasStructLayout && this.aggSym.IsStruct)
            {
                if (this.aggSym.FirstDeclSym.IsPartial)
                {
                    CheckSequentialOnPartialType(null);
                }
                // structs with 0 instance fields must be given an explicit size of 0
                bool hasInstanceVar = false;

                for (SYM child = this.aggSym.FirstChildSym; child != null; child = child.NextSym)
                {
                    if (child.IsMEMBVARSYM && !(child as MEMBVARSYM).IsStatic)
                    {
                        hasInstanceVar = true;
                        break;
                    }
                }

                AGGTYPESYM symStructLayout = compiler.GetOptPredefType(PREDEFTYPE.STRUCTLAYOUT, true);
                if (symStructLayout != null)
                {
                    ProcessSynthAttr(
                        symStructLayout,
                        compiler.FuncBRec.BindSimplePredefinedAttribute(
                            PREDEFTYPE.STRUCTLAYOUT, compiler.FuncBRec.BindStructLayoutArgs()),
                        compiler.FuncBRec.BindStructLayoutNamedArgs(hasInstanceVar));
                }
            }
        }

        //------------------------------------------------------------
        // AggAttrBind.ProcessStructLayout
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected void ProcessStructLayout(ATTRNODE attrNode)
        {
            if (!this.aggSym.IsClass && !this.aggSym.IsStruct)
            {
                ErrorBadSymbolKind(attrNode.NameNode);
                return;
            }

            this.aggInfo.HasStructLayout = true;

            int layoutKind = 0;
            EXPR nodeExpr = (ctorExpr as EXPRCALL).ArgumentsExpr;
            while (nodeExpr != null)
            {
                EXPR argExpr;
                if (nodeExpr.Kind == EXPRKIND.LIST)
                {
                    argExpr = nodeExpr.AsBIN.Operand1;
                    nodeExpr = nodeExpr.AsBIN.Operand2;
                }
                else
                {
                    argExpr = nodeExpr;
                    nodeExpr = null;
                }
                GetValue(argExpr, out layoutKind);
                break;
            }

            if (layoutKind == compiler.ClsDeclRec.GetLayoutKindValue(PREDEFNAME.EXPLICIT))
            {
                this.aggInfo.HasExplicitLayout = true;
            }
            else if (
                (this.aggSym.FirstDeclSym.IsPartial) &&
                (layoutKind == compiler.ClsDeclRec.GetLayoutKindValue(PREDEFNAME.SEQUENTIAL)))
            {
                CheckSequentialOnPartialType(attrNode);
            }

            // if the class has structlayout then it is
            // expected to be used in Interop
            //
            // Disable warnings for unassigned members for all interop structs here

            this.aggSym.HasExternReference = true;

            VerifyAndEmitCore(attrNode);
        }

        //------------------------------------------------------------
        // AggAttrBind.CheckSequentialOnPartialType
        //
        /// <summary>
        /// Check for SequentialLayout attribute on a partial class,
        /// and give a warning if it exists.
        /// </summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        private void CheckSequentialOnPartialType(ATTRNODE attrNode)
        {
            AGGDECLSYM firstDeclSym = null;
            bool giveWarning = false;
            for (SYM memb = this.aggSym.FirstChildSym; memb != null; memb = memb.NextSym)
            {
                if (memb.IsMEMBVARSYM && !(memb as MEMBVARSYM).IsStatic)
                {
                    AGGDECLSYM decl = memb.ContainingDeclaration() as AGGDECLSYM;
                    DebugUtil.Assert(decl != null);
                    if (firstDeclSym == null)
                    {
                        firstDeclSym = decl;
                    }
                    else if (firstDeclSym != decl)
                    {
                        giveWarning = true;
                        break;
                    }
                }
            }

            // only give a warning if there are multiple aggdecls with fields in them.
            if (!giveWarning)
            {
                return;
            }

            CError err = null;
            err = compiler.MakeError(
                (attrNode != null ? attrNode.NameNode : null),
                CSCERRID.WRN_SequentialOnPartialClass,
                new ErrArg(this.aggSym));
            if (err == null)
            {
                return;
            }

            for (AGGDECLSYM decl = this.aggSym.FirstDeclSym; decl != null; decl = decl.NextDeclSym)
            {
                DebugUtil.Assert(decl.IsPartial);
                compiler.AddLocationToError(
                    err,
                    new ERRLOC(
                        decl.GetInputFile(),
                        decl.GetParseTree(),
                        compiler.OptionManager.FullPaths));
            }
            compiler.SubmitError(err);
        }

        //------------------------------------------------------------
        // AggAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null)
            {
                return;
            }
            if (this.aggSym == null || this.aggSym.TypeBuilder == null)
            {
                return;
            }

            try
            {
                this.aggSym.TypeBuilder.SetCustomAttribute(caBuilder);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    //======================================================================
    // class TypeVarAttrBind
    //
    /// <summary>
    /// Attribute binder for a set of type variables.
    /// Note that one TYVARSYM may map to many generic parameters in meta-data,
    /// since in C# nested types inherit outer type parameters
    /// but in metadata nested types do not inherit generic parameters from the outer type.
    /// </summary>
    //======================================================================
    internal class TypeVarAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // TypeVarAttrBind Fields and Properties
        //------------------------------------------------------------
        //private mdToken tok;
        internal GenericTypeParameterBuilder TypeParameterBuilder = null;

        //------------------------------------------------------------
        // TypeVarAttrBind.CompileParamList (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="firstDeclSym"></param>
        /// <param name="typeVariables"></param>
        //------------------------------------------------------------
        static public void CompileParamList(
            COMPILER compiler,
            PARENTSYM firstDeclSym,
            TypeArray typeVariables
            //, mdToken tokens
            )
        {
            // loop through typeVariables
            for (int i = 0; i < typeVariables.Count; ++i)
            {
                TYVARSYM tvSym = typeVariables.ItemAsTYVARSYM(i);

                TypeVarAttrBind attrbind = new TypeVarAttrBind(compiler, tvSym, null);
                //, tokens[var->indexTotal]);

                // loop through all attribute nodes for each tyvar
                foreach (ATTRINFO attrInfo in tvSym.AttributeList)
                {
                    // there was an error when we tried to bind this attribute initially,
                    // so we will not attempt again.
                    if (attrInfo.HadError) continue;

                    attrbind.contextSym = attrInfo.ContextSym;

                    int ec = compiler.ErrorCount();
                    attrbind.ProcessAll(attrInfo.AttributeNode, null);
                    if (ec != compiler.ErrorCount())
                    {
                        // there was an error,
                        // mark it so we don't try and bind this attribute again for nested types
                        attrInfo.HadError = true;
                    }
                }
            }
        }

        //------------------------------------------------------------
        // TypeVarAttrBind Constructor
        //------------------------------------------------------------
        protected TypeVarAttrBind(
            COMPILER compiler,
            TYVARSYM var,
            PARENTSYM context)
            : base(compiler, false)
        {
            //this.tok = tok;
            this.sym = var;
            Init(AttributeTargets.GenericParameter, context);
        }

        //------------------------------------------------------------
        // TypeVarAttrBind.GetToken
        //------------------------------------------------------------
        //protected virtual mdToken GetToken()
        //{
        //    DebugUtil.Assert(tok != 0);
        //    return tok;
        //}

        //------------------------------------------------------------
        // TypeVarAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null)
            {
                return;
            }
            if (this.TypeParameterBuilder == null)
            {
                return;
            }

            try
            {
                this.TypeParameterBuilder.SetCustomAttribute(caBuilder);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    //======================================================================
    // class IndexerNameAttrBind
    //
    /// <summary>
    /// <para>Early attribute binder for INDEXERSYMs. Handles indexer-name.</para>
    /// <para>Derives from AttrBind.</para>
    /// </summary>
    //======================================================================
    internal class IndexerNameAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // IndexerNameAttrBind Fields and Properties
        //------------------------------------------------------------
        protected INDEXERSYM indexerSym = null; // * prop;

        //------------------------------------------------------------
        // IndexerNameAttrBind.Compile (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="indexerSym"></param>
        //------------------------------------------------------------
        internal static void Compile(COMPILER compiler, INDEXERSYM indexerSym)
        {
            BASENODE attributes = indexerSym.GetAttributesNode();
            if (attributes == null) return;

            IndexerNameAttrBind attrbind = new IndexerNameAttrBind(compiler, indexerSym);
            attrbind.ProcessAll(attributes, null);
        }

        //------------------------------------------------------------
        // IndexerNameAttrBind Constructor
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="indexer"></param>
        //------------------------------------------------------------
        protected IndexerNameAttrBind(COMPILER compiler, INDEXERSYM indexer)
            : base(compiler, true)
        {
            this.indexerSym = indexer;
            Init(indexer);
        }

        //------------------------------------------------------------
        // IndexerNameAttrBind.BindAttr
        //
        /// <summary></summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override protected bool BindAttr(ATTRNODE attr)
        {
            switch (this.predefinedAttribute)
            {
                default:
                    return false;

                case PREDEFATTR.NAME:
                    break;
            }
            return base.BindAttr(attr);
        }

        //------------------------------------------------------------
        // IndexerNameAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.NAME:
                    DebugUtil.Assert(this.namedArgumentsExpr == null);

                    if (this.indexerSym.IsOverride)
                    {
                        compiler.Error(
                            attrNode.NameNode,
                            CSCERRID.ERR_NameAttributeOnOverride,
                            new ErrArgNameNode(attrNode.NameNode, ErrArgFlagsEnum.None));
                    }
                    string nameValue = null;

                    EXPR nodeExpr = (ctorExpr as EXPRCALL).ArgumentsExpr;
                    while (nodeExpr != null)
                    {
                        EXPR argExpr;
                        if (nodeExpr.Kind == EXPRKIND.LIST)
                        {
                            argExpr = nodeExpr.AsBIN.Operand1;
                            nodeExpr = nodeExpr.AsBIN.Operand2;
                        }
                        else
                        {
                            argExpr = nodeExpr;
                            nodeExpr = null;
                        }
                        DebugUtil.Assert(nameValue == null);  // there can be only one
                        GetValue((ctorExpr as EXPRCALL).ArgumentsExpr, out nameValue);

                        if (compiler.CheckForValidIdentifier(
                            nameValue,
                            false,
                            argExpr.TreeNode,
                            CSCERRID.ERR_BadArgumentToAttribute,
                            new ErrArg(this.attributeTypeSym)))
                        {
                            // convert the string to a name
                            this.indexerSym.RealName = nameValue;
                            compiler.NameManager.AddString(nameValue);
                        }
                    }
                    return;

                default:
                    // Unlike most early binders, this shouldn't call the base VerifyAndEmitPredef.
                    // This is because another early binder has already been invoked on this property
                    // to handle cls, obsolete, etc.
                    return;
            }
        }

        //------------------------------------------------------------
        // IndexerNameAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null)
            {
                return;
            }
            if (this.indexerSym == null || this.indexerSym.PropertyBuilder == null)
            {
                return;
            }

            try
            {
                this.indexerSym.PropertyBuilder.SetCustomAttribute(caBuilder);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    //======================================================================
    // class ParamAttrBind
    //
    /// <summary>
    /// Attribute binder for parameters.
    /// </summary>
    //======================================================================
    internal class ParamAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // ParamAttrBind Fields and Properties
        //------------------------------------------------------------
        protected METHSYM methodSym = null;	    // * method;
        protected PARAMINFO paramInfo = null;	// * paramInfo;
        protected TYPESYM paramTypeSym = null;	// * parameterType;
        protected int index;
        protected bool hasDefaultValue;
        protected int etDefaultValue;           // DWORD etDefaultValue;
        protected byte[] rgb = new byte[256];
        //protected BlobBldrNrHeap  blob;
        protected List<object> blob = new List<object>();

        //------------------------------------------------------------
        // ParamAttrBind.CompileParamList (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="methodInfo"></param>
        //------------------------------------------------------------
        static public void CompileParamList(COMPILER compiler, METHINFO methodInfo)
        {
            ParamAttrBind attrbind = new ParamAttrBind(compiler, methodInfo.MethodSym);

            // If the delegate previously had attribute errors,
            // then do not continue processing attributes on each of the delegate methods
            // because they will bind the same attribute and give duplicate error messages.
            // This method is not called during refactoring because we use the RefactoringAttrBind instead.
            // If this ever changes, this statement should be updated
            // because it will keep us from binding attributes on delegate Invoke methods in error cases.

            if (methodInfo.MethodSym.ClassSym.HadAttributeError)
            {
                DebugUtil.Assert(methodInfo.MethodSym.ClassSym.IsDelegate && compiler.ErrorCount() > 0);
                // it is safe to return because the only way we can put attributes
                // on any of the parameters of this method is via the delegate,
                // so they will all be duplicates and at least one of them will have given an error.
                return;
            }

            if (methodInfo.AttributesNode != null)
            {
                attrbind.Init(methodInfo.MethodSym.ReturnTypeSym, methodInfo.ReturnValueInfo, 0);
                attrbind.ProcessAll(methodInfo.AttributesNode, null);
            }

            //PARAMINFO *ppin = methodInfo.rgpin;
            List<PARAMINFO> paramInfos = methodInfo.ParameterInfos;
            //for (int i = 0; i < methodInfo.cpin; i++, ppin++)
            for (int i = 0; i < paramInfos.Count; ++i)
            {

                // if the property previously had bad attributes on the parameters,
                // don't give another error here
                // We can still parse non-indexer accessors because the only way to get attributes
                // on the parameters is with the param: attribute target

                if (methodInfo.MethodSym.IsPropertyAccessor &&
                    methodInfo.MethodSym.PropertySym.HadAttributeError)
                {
                    DebugUtil.Assert(
                        methodInfo.MethodSym.PropertySym.IsIndexer &&
                        compiler.ErrorCount() > 0);
                    continue;
                }

                PARAMINFO info = paramInfos[i];
                attrbind.Init(methodInfo.MethodSym.ParameterTypes[i], info, i + 1);

                if (info.AttrBaseNode != null)
                {
                    UnknownAttrBind.Compile(
                        compiler,
                        methodInfo.MethodSym.ContainingDeclaration(),
                        info.AttrBaseNode);
                    attrbind.ProcessAll(info.AttrBaseNode, null);
                }

                // Emit predefined parameter properties(name, marshaling).
                attrbind.EmitPredefAttrs();
            }
        }

        // for regular parameter lists

        //------------------------------------------------------------
        // ParamAttrBind Constructor
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="method"></param>
        //------------------------------------------------------------
        protected ParamAttrBind(COMPILER compiler, METHSYM method)
            : base(compiler, false)
        {
            this.methodSym = method;
        }

        //------------------------------------------------------------
        // ParamAttrBind.Init
        //
        /// <summary></summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <param name="index"></param>
        //------------------------------------------------------------
        protected void Init(TYPESYM type, PARAMINFO info, int index)
        {
            this.paramTypeSym = type;
            this.paramInfo = info;
            this.index = index;
            this.customAttributeList = null;
            this.sym = this.methodSym;
            this.hasDefaultValue = false;
            this.etDefaultValue = 0;
            //this.blob.Reset();
            this.blob.Clear();
            base.Init(
                (index == 0 ? AttributeTargets.ReturnValue : AttributeTargets.Parameter),
                this.methodSym.ContainingAggDeclSym);
            EmitParamProps();
        }

        //------------------------------------------------------------
        // ParamAttrBind.GetToken
        //------------------------------------------------------------
        //protected mdToken GetToken();

        //------------------------------------------------------------
        // ParamAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.IN:
                    if (this.paramTypeSym.IsPARAMMODSYM && (this.paramTypeSym as PARAMMODSYM).IsOut)
                    {
                        // "out" parameter cannot have the "in" attribute.
                        compiler.Error(attrNode, CSCERRID.ERR_InAttrOnOutParam);
                        return;
                    }
                    paramInfo.IsIn = true;
                    break;

                case PREDEFATTR.OUT:
                    paramInfo.IsOut = true;
                    break;

                case PREDEFATTR.PARAMARRAY:
                    compiler.Error(attrNode, CSCERRID.ERR_ExplicitParamArray);
                    break;

                case PREDEFATTR.CLSCOMPLIANT:
                    if ((!methodSym.ClassSym.IsDelegate || methodSym.IsInvoke) && compiler.AllowCLSErrors())
                    {
                        if (this.attrTarget == ATTRTARGET.PARAMETER)
                        {
                            compiler.Error(attrNode, CSCERRID.WRN_CLS_MeaninglessOnParam);
                        }
                        else if (this.attrTarget == ATTRTARGET.RETURN)
                        {
                            compiler.Error(attrNode, CSCERRID.WRN_CLS_MeaninglessOnReturn);
                        }
                    }
                    break;

                case PREDEFATTR.DEFAULTVALUE:
                    EXPR nodeExpr = (ctorExpr as EXPRCALL).ArgumentsExpr;
                    while (nodeExpr != null)
                    {
                        EXPR argExpr;
                        if (nodeExpr.Kind == EXPRKIND.LIST)
                        {
                            argExpr = nodeExpr.AsBIN.Operand1;
                            nodeExpr = nodeExpr.AsBIN.Operand2;
                        }
                        else
                        {
                            argExpr = nodeExpr;
                            nodeExpr = null;
                        }
                        DebugUtil.Assert(!this.hasDefaultValue);
                        TYPESYM underlyingParameterType = this.paramTypeSym.IsPARAMMODSYM ?
                            (this.paramTypeSym as PARAMMODSYM).ParamTypeSym : this.paramTypeSym;

                        if (argExpr.Kind == EXPRKIND.CAST)
                        {
                            EXPR valueExpr = (argExpr as EXPRCAST).Operand;
                            TYPESYM valueTypeSym = valueExpr.TypeSym;

                            if (valueTypeSym != underlyingParameterType &&
                                !underlyingParameterType.IsPredefType(PREDEFTYPE.OBJECT))
                            {
                                compiler.Error(attrNode, CSCERRID.ERR_DefaultValueTypeMustMatch);
                            }
                            else if (valueTypeSym.Kind == SYMKIND.ARRAYSYM || valueTypeSym.IsPredefType(PREDEFTYPE.TYPE))
                            {
                                if (underlyingParameterType.IsPredefType(PREDEFTYPE.OBJECT))
                                {
                                    compiler.Error(attrNode, CSCERRID.ERR_DefaultValueBadValueType, new ErrArg(valueTypeSym));
                                }
                                else
                                {
                                    compiler.Error(attrNode, CSCERRID.ERR_DefaultValueBadParamType,
                                        new ErrArg(underlyingParameterType));
                                }
                            }
                            else
                            {
                                this.hasDefaultValue = true;

                                if (valueTypeSym.IsPredefType(PREDEFTYPE.STRING))
                                {
                                    //blob.Add(
                                    //	valueExpr.AsCONSTANT.GetStringValue,
                                    //	valueExpr.AsCONSTANT.getSVal().strVal.length * sizeof(WCHAR));
                                    blob.Add((valueExpr as EXPRCONSTANT).GetStringValue());
                                }
                                else
                                {
                                    AddAttrArg(valueExpr, blob);
                                }
                                this.etDefaultValue = compiler.MainSymbolManager.GetElementType(
                                    valueTypeSym.UnderlyingType() as AGGTYPESYM);
                            }
                        }
                        else
                        {
                            // null va^lue of object type
                            if (underlyingParameterType.Kind == SYMKIND.ARRAYSYM ||
                                underlyingParameterType.IsPredefType(PREDEFTYPE.TYPE))
                            {
                                compiler.Error(attrNode, CSCERRID.ERR_DefaultValueBadParamType,
                                    new ErrArg(underlyingParameterType));
                            }
                            else if (
                                underlyingParameterType.Kind != SYMKIND.AGGTYPESYM ||
                                !(underlyingParameterType as AGGTYPESYM).GetAggregate().IsReferenceType)
                            {
                                compiler.Error(attrNode, CSCERRID.ERR_DefaultValueTypeMustMatch);
                            }
                            else
                            {
                                this.hasDefaultValue = true;
                                this.etDefaultValue = (int)CorElementType.CLASS;	// ELEMENT_TYPE_CLASS;
                            }
                        }
                    }
                    return; // don't emit real attribute to the metadata

                default:
                    break;
            }

            base.VerifyAndEmitPredef(attrNode);
        }

        //------------------------------------------------------------
        // ParamAttrBind.ValidateAttrs
        //
        /// <summary></summary>
        //------------------------------------------------------------
        override protected void ValidateAttrs()
        {
            if (this.paramInfo.IsOut &&
                !this.paramInfo.IsIn &&
                this.paramTypeSym.IsPARAMMODSYM &&
                (this.paramTypeSym as PARAMMODSYM).IsRef)
            {
                compiler.ErrorRef(null, CSCERRID.ERR_OutAttrOnRefParam,
                    new ErrArgRef(this.methodSym));
            }
        }

        //------------------------------------------------------------
        // ParamAttrBind.EmitParamProps
        //
        /// <summary></summary>
        //------------------------------------------------------------
        protected void EmitParamProps()
        {
            compiler.Emitter.EmitParamProp(
                this.methodSym.MethodBuilder,
                index,
                this.paramTypeSym,
                this.paramInfo,
                this.hasDefaultValue,
                this.etDefaultValue,
                this.blob);
        }

        //------------------------------------------------------------
        // ParamAttrBind.EmitPredefAttrs
        //
        /// <summary></summary>
        //------------------------------------------------------------
        protected void EmitPredefAttrs()
        {
            // emit the synthetized parameter...
            if (this.paramInfo.IsParametersArray)
            {
                base.ProcessSynthAttr(
                    compiler.GetReqPredefType(PREDEFTYPE.PARAMS, true),
                    compiler.FuncBRec.BindSimplePredefinedAttribute(PREDEFTYPE.PARAMS),
                    null);
            }

            if (this.paramTypeSym.IsPARAMMODSYM &&
                (this.paramTypeSym as PARAMMODSYM).IsOut)
            {
                base.ProcessSynthAttr(
                    compiler.GetReqPredefType(PREDEFTYPE.OUT, true),
                    compiler.FuncBRec.BindSimplePredefinedAttribute(PREDEFTYPE.OUT),
                    null);
            }

            if (this.hasDefaultValue)
            {
                EmitParamProps();
            }
        }

        //------------------------------------------------------------
        // ParamAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (paramInfo.ParameterBuilder == null)
            {
                if (this.methodSym != null)
                {
                    if (methodSym.MethodBuilder!= null)
                    {
                        this.paramInfo.ParameterBuilder = methodSym.MethodBuilder.DefineParameter(
                            this.index,
                            this.paramInfo.GetParameterAttributes(),
                            this.paramInfo.Name);
                    }
                    else if (methodSym.ConstructorBuilder != null)
                    {
                        this.paramInfo.ParameterBuilder = methodSym.ConstructorBuilder.DefineParameter(
                            this.index,
                            this.paramInfo.GetParameterAttributes(),
                            this.paramInfo.Name);
                    }
                }
            }

            if (paramInfo.ParameterBuilder == null)
            {
                return;
            }

            this.paramInfo.ParameterBuilder.SetCustomAttribute(caBuilder);
        }
    }

    //======================================================================
    // class MethAttrBind
    //
    /// <summary>
    /// <para>Attribute binder for methods.</para>
    /// <para>Derives from AttrBind.</para>
    /// </summary>
    //======================================================================
    internal class MethAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // MethAttrBind Fields and Properties
        //------------------------------------------------------------
        protected METHSYM methodSym = null;                 // meth
        protected List<string> conditionNameList = null;    // NAMELIST ** pnlstCond;

        //------------------------------------------------------------
        // MethAttrBind.CompileEarly
        //
        /// <summary></summary>
        /// <param name="cmp"></param>
        /// <param name="meth"></param>
        //------------------------------------------------------------
        public static void CompileEarly(COMPILER cmp, METHSYM meth)
        {
            BASENODE nodeAttr = meth.GetAttributesNode();
            if (nodeAttr == null)
            {
                return;
            }

            // (CS3) partial method
            if (meth.IsPartialMethod && meth.HasNoBody)
            {
                meth = meth.PartialMethodImplSym;
            }
            if (meth == null)
            {
                return;
            }

            MethAttrBind attrbind = new MethAttrBind(cmp, meth, true);
            attrbind.ProcessAll(nodeAttr, null);
        }

        //------------------------------------------------------------
        // MethAttrBind.CompileAndEmit (static)
        //
        /// <summary></summary>
        /// <param name="cmp"></param>
        /// <param name="meth"></param>
        /// <param name="debuggerHidden"></param>
        //------------------------------------------------------------
        static public void CompileAndEmit(
            COMPILER cmp,
            METHSYM meth,
            bool debuggerHidden,
            Dictionary<SecurityAction,PermissionSet> permissionSets)
        {
            BASENODE attrNode = meth.GetAttributesNode();

            // (CS3) partial method
            if (meth.IsPartialMethod && meth.HasNoBody)
            {
                meth = meth.PartialMethodImplSym;
            }
            if (meth == null)
            {
                return;
            }

            if (attrNode != null || debuggerHidden || meth.IsFabricated || meth.IsAnonymous)
            {
                MethAttrBind attrbind = new MethAttrBind(cmp, meth, false);

                if (attrNode != null)
                {
                    UnknownAttrBind.Compile(cmp, meth.ContainingDeclaration(), attrNode);
                    attrbind.ProcessAll(attrNode, permissionSets);
                }

                // Sometimes we need to really hide some methods.
                if (debuggerHidden)
                {
                    AGGTYPESYM atsHidden = cmp.GetOptPredefType(PREDEFTYPE.DEBUGGERHIDDEN, true);
                    if (atsHidden != null)
                    {
                        attrbind.ProcessSynthAttr(
                            atsHidden,
                            cmp.FuncBRec.BindSimplePredefinedAttribute(PREDEFTYPE.DEBUGGERHIDDEN),
                            null);
                    }
                }

                if (meth.IsFabricated || meth.IsAnonymous || debuggerHidden)
                {
                    attrbind.CompileFabricatedAttr();
                }
            }

            if (meth.TypeVariables.Count > 0)
            {
                TypeVarAttrBind.CompileParamList(cmp, meth, meth.TypeVariables);
                //, meth.toksEmitTypeVars);
            }
        }

        //------------------------------------------------------------
        // MethAttrBind Constructor
        //
        /// <summary></summary>
        /// <param name="cmp"></param>
        /// <param name="meth"></param>
        /// <param name="early"></param>
        //------------------------------------------------------------
        protected MethAttrBind(COMPILER cmp, METHSYM meth, bool early)
            : base(cmp, early)
        {
            this.methodSym = meth;
            //this.conditionNameList = early ? methodSym.ConditionalSymbolNameList : null;
            if (early)
            {
                if (this.methodSym.ConditionalSymbolNameList == null)
                {
                    this.methodSym.ConditionalSymbolNameList = new List<string>();
                }
                this.conditionNameList = this.methodSym.ConditionalSymbolNameList;
            }
            Init(meth);
        }

        //------------------------------------------------------------
        // MethAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            BASENODE name = attrNode.NameNode;
            bool badAccAttr = false;

            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.OBSOLETE:
                case PREDEFATTR.CLSCOMPLIANT:
                    if (this.methodSym.IsAnyAccessor)
                    {
                        //goto LBadAccAttr;
                        badAccAttr = true;
                        goto case PREDEFATTR.CONDITIONAL;
                    }
                    break;

                case PREDEFATTR.CONDITIONAL:
                    if (this.methodSym.IsAnyAccessor || badAccAttr == true)
                    {
                        //LBadAccAttr:
                        if (this.isEarly)
                        {
                            return;
                        }

                        // Predefined attribute which is not valid on accessors
                        //WCHAR buffer[1024];
                        string targets = AttrUtil.BuildAttrTargetString(
                            this.attributeTypeSym.GetAggregate().AttributeClass);
                        compiler.Error(
                            attrNode.NameNode,
                            CSCERRID.ERR_AttributeNotOnAccessor,
                            new ErrArgNameNode(attrNode.NameNode, ErrArgFlagsEnum.None),
                            new ErrArg(targets));
                        return;
                    }

                    if (!this.isEarly)
                    {
                        // Call base VerifyAndEmitPredef.
                        break;
                    }

                    if (this.methodSym.ClassSym.IsInterface)
                    {
                        compiler.Error(attrNode, CSCERRID.ERR_ConditionalOnInterfaceMethod);
                    }
                    else if (
                        !this.methodSym.IsUserCallable() ||
                        this.methodSym.Name == null ||
                        this.methodSym.IsCtor ||
                        this.methodSym.IsDtor)
                    {
                        compiler.Error(
                            attrNode,
                            CSCERRID.ERR_ConditionalOnSpecialMethod,
                            new ErrArg(this.methodSym));
                    }
                    else if (this.methodSym.IsOverride)
                    {
                        compiler.Error(
                            attrNode,
                            CSCERRID.ERR_ConditionalOnOverride,
                            new ErrArg(this.methodSym));
                    }
                    else if (!this.methodSym.ReturnTypeSym.IsVoidType)
                        // != compiler.MainSymbolManager.VoidSym)
                    {
                        compiler.Error(
                            attrNode,
                            CSCERRID.ERR_ConditionalMustReturnVoid,
                            new ErrArg(this.methodSym));
                    }
                    else
                    {
                        // Conditional method cannot have out parameters.
                        for (int i = 0; i < this.methodSym.ParameterTypes.Count; ++i)
                        {
                            if (this.methodSym.ParameterTypes[i].IsPARAMMODSYM &&
                                (this.methodSym.ParameterTypes[i] as PARAMMODSYM).IsOut)
                            {
                                compiler.Error(
                                    attrNode,
                                    CSCERRID.ERR_ConditionalWithOutParam,
                                    new ErrArg(this.methodSym));
                            }
                        }
                        // Call base VerifyAndEmitPredef.
                        break;
                    }
                    // In error cases, don't call base VerifyAndEmitPredef
                    return;

                case PREDEFATTR.DLLIMPORT:
                    if (!this.isEarly)
                    {
                        ProcessDllImport(attrNode);
                    }
                    return;

                default:
                    break;
            }

            base.VerifyAndEmitPredef(attrNode);
        }

        //------------------------------------------------------------
        // MethAttrBind.BindAttr
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        override protected bool BindAttr(ATTRNODE attrNode)
        {
            // For early processing only do security, conditonal, obsolete and cls.
            if (!this.isEarly ||
                (this.attributeTypeSym != null && this.attributeTypeSym.IsSecurityAttribute()) ||
                (this.predefinedAttribute == PREDEFATTR.CONDITIONAL ||
                this.predefinedAttribute == PREDEFATTR.OBSOLETE ||
                this.predefinedAttribute == PREDEFATTR.CLSCOMPLIANT))
            {
                return base.BindAttr(attrNode);
            }

            return false;
        }

        //------------------------------------------------------------
        // MethAttrBind.ValidateAttrs
        //
        /// <summary></summary>
        //------------------------------------------------------------
        override protected void ValidateAttrs()
        {
            if (this.isEarly)
            {
                this.methodSym.HasLinkDemand = this.hasLinkDemand;
            }
        }

        //------------------------------------------------------------
        // MethAttrBind.AddConditionalName
        //
        /// <summary></summary>
        /// <param name="name"></param>
        //------------------------------------------------------------
        override protected void AddConditionalName(string name)
        {
            if (this.conditionNameList != null)
            {
                compiler.MainSymbolManager.AddToGlobalNameList(name, this.conditionNameList);
            }
        }

        //------------------------------------------------------------
        // MethAttrBind.ProcessDllImport
        //
        /// <summary>
        /// compiles a dllimport attribute on a method
        /// </summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        protected void ProcessDllImport(ATTRNODE attrNode)
        {
            if (!this.methodSym.IsStatic || !this.methodSym.IsExternal)
            {
                compiler.Error(attrNode.NameNode, CSCERRID.ERR_DllImportOnInvalidMethod);
                return;
            }

            AddDefaultCharSet();

            VerifyAndEmitCore(attrNode);
        }

        //------------------------------------------------------------
        // MethAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null || this.methodSym == null)
            {
                return;
            }

            try
            {
                if (this.methodSym.ConstructorBuilder != null)
                {
                    this.methodSym.ConstructorBuilder.SetCustomAttribute(caBuilder);
                }
                else if (this.methodSym.MethodBuilder != null)
                {
                    this.methodSym.MethodBuilder.SetCustomAttribute(caBuilder);
                }
                else
                {
                    // show error messages.
                }
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    //======================================================================
    // class PropAttrBind
    //
    /// <summary>
    /// Attribute binder for properties.
    /// This does not deal with attributes that belong on the accessors.
    /// </summary>
    //======================================================================
    internal class PropAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // PropAttrBind Fields and Properties
        //------------------------------------------------------------
        protected PROPSYM propertySym = null;   // * prop;

        //------------------------------------------------------------
        // PropAttrBind.Compile (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="prop"></param>
        //------------------------------------------------------------
        static public void Compile(COMPILER compiler, PROPSYM prop)
        {
            BASENODE attributes = prop.GetAttributesNode();
            if (attributes == null)
            {
                return;
            }

            UnknownAttrBind.Compile(compiler, prop.ContainingDeclaration(), attributes);

            PropAttrBind attrbind = new PropAttrBind(compiler, prop);
            if (attributes != null)
            {
                attrbind.ProcessAll(attributes, null);
            }

            attrbind.CompileFabricatedAttr();
        }

        //------------------------------------------------------------
        // PropAttrBind Constructor
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="prop"></param>
        //------------------------------------------------------------
        protected PropAttrBind(COMPILER compiler, PROPSYM prop)
            : base(compiler, false)
        {
            this.propertySym = prop;
            Init(prop);
        }

        //------------------------------------------------------------
        // PropAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attr"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attr)
        {
            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.NAME:
                    // Name attributes are checked in the define stage for non-explicit impl indexers,
                    // otherwise they are an error.
                    if (!this.propertySym.IsIndexer || this.propertySym.IsExplicitImplementation)
                    {
                        compiler.Error(attr.NameNode, CSCERRID.ERR_BadIndexerNameAttr,
                            new ErrArgNameNode(attr.NameNode, ErrArgFlagsEnum.None));
                    }
                    return;

                default:
                    break;
            }

            base.VerifyAndEmitPredef(attr);
        }

        //------------------------------------------------------------
        // PropAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null)
            {
                return;
            }
            if (this.propertySym == null || this.propertySym.PropertyBuilder == null)
            {
                return;
            }

            try
            {
                this.propertySym.PropertyBuilder.SetCustomAttribute(caBuilder);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    //======================================================================
    // class FieldAttrBind
    //
    /// <summary>
    /// Attribute binder for fields.
    /// </summary>
    //======================================================================
    internal class FieldAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // FieldAttrBind Fields and Properties
        //------------------------------------------------------------
        protected MEMBVARSYM fieldSym = null;   // * field;
        protected MEMBVARINFO fieldInfo = null; // * info;
        protected AGGINFO aggInfo = null;       // * agginfo;

        //------------------------------------------------------------
        // FieldAttrBind.Compile (static)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="field"></param>
        /// <param name="info"></param>
        /// <param name="agginfo"></param>
        //------------------------------------------------------------
        static public void Compile(
            COMPILER compiler,
            MEMBVARSYM field,
            MEMBVARINFO info,
            AGGINFO agginfo)
        {
            BASENODE attributes = field.GetAttributesNode();

            if (attributes != null)
            {
                UnknownAttrBind.Compile(compiler, field.ContainingDeclaration(), attributes);
            }

            // Do this whether or not there are attributes, so ValidateAttributes gets called either way.
            FieldAttrBind attrbind = new FieldAttrBind(compiler, field, info, agginfo);
            attrbind.ProcessAll(attributes, null);
            attrbind.CompileFabricatedAttr();
        }

        //------------------------------------------------------------
        // FieldAttrBind Constructor
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="field"></param>
        /// <param name="info"></param>
        /// <param name="agginfo"></param>
        //------------------------------------------------------------
        protected FieldAttrBind(
            COMPILER compiler,
            MEMBVARSYM field,
            MEMBVARINFO info,
            AGGINFO agginfo)
            : base(compiler, false)
        {
            this.fieldSym = field;
            this.fieldInfo = info;
            this.aggInfo = agginfo;
            Init(this.fieldSym);
        }

        //------------------------------------------------------------
        // FieldAttrBind.VerifyAndEmitPredef
        //
        /// <summary></summary>
        /// <param name="attrNode"></param>
        //------------------------------------------------------------
        override protected void VerifyAndEmitPredef(ATTRNODE attrNode)
        {
            switch (this.predefinedAttribute)
            {
                case PREDEFATTR.STRUCTOFFSET:
                    // Must be explicit layout kind on aggregate.
                    if (!this.aggInfo.HasExplicitLayout)
                    {
                        compiler.Error(attrNode.NameNode, CSCERRID.ERR_StructOffsetOnBadStruct);
                        return;
                    }
                    if (this.fieldSym.IsConst || this.fieldSym.IsStatic)
                    {
                        compiler.Error(attrNode.NameNode, CSCERRID.ERR_StructOffsetOnBadField);
                        return;
                    }
                    this.fieldInfo.FoundOffset = true;
                    break;

                case PREDEFATTR.FIXED:
                    compiler.Error(attrNode.NameNode, CSCERRID.ERR_DoNotUseFixedBufferAttr);
                    return;

                default:
                    break;
            }

            base.VerifyAndEmitPredef(attrNode);
        }

        //------------------------------------------------------------
        // FieldAttrBind.ValidateAttrs
        //
        /// <summary></summary>
        //------------------------------------------------------------
        override protected void ValidateAttrs()
        {
            if (this.aggInfo.HasExplicitLayout &&
                !this.fieldSym.IsConst &&
                !this.fieldSym.IsStatic &&
                !this.fieldInfo.FoundOffset)
            {
                compiler.ErrorRef(
                    null,
                    CSCERRID.ERR_MissingStructOffset,
                    new ErrArgRef(this.fieldSym));
            }

            if (this.fieldSym.IsConst &&
                this.fieldSym.TypeSym.IsPredefType(PREDEFTYPE.DECIMAL))
            {
                //DecimalConstantBuffer buffer;

                //buffer.format   = VAL16(1);
                //buffer.scale    = DECIMAL_SCALE(*(this.fieldSym.constVal.decVal));
                //buffer.sign     = DECIMAL_SIGN(*(this.fieldSym.constVal.decVal));
                //buffer.hi       = VAL32(DECIMAL_HI32(*(this.fieldSym.constVal.decVal)));
                //buffer.mid      = VAL32(DECIMAL_MID32(*(this.fieldSym.constVal.decVal)));
                //buffer.low      = VAL32(DECIMAL_LO32(*(this.fieldSym.constVal.decVal)));
                //buffer.cNamedArgs = 0;

                AGGSYM aggDecConst = compiler.GetOptPredefAggErr(PREDEFTYPE.DECIMALCONSTANT, true);
                DebugUtil.Assert(aggDecConst != null);
                METHSYM ctor = compiler.MainSymbolManager.LookupAggMember(
                        compiler.NameManager.GetPredefinedName(PREDEFNAME.CTOR),
                        aggDecConst,
                        SYMBMASK.METHSYM) as METHSYM;
                DebugUtil.Assert(ctor != null);
                ConstructorInfo cInfo = ctor.ConstructorInfo;
                DebugUtil.Assert(cInfo != null);

                CONSTVAL constVal=this.fieldSym.ConstVal;
                if (constVal == null)
                {
                    return;
                }

                Decimal decVal = this.fieldSym.ConstVal.GetDecimal();
                Object[] args = Util.GetDecimalConstantAttributeArguments(cInfo, decVal);
                CustomAttributeBuilder caBuilder = ReflectionUtil.CreateCustomAttributeBuilder(
                    ctor.ConstructorInfo,
                    args,
                    null, null, null, null);

                Exception excp = null;
                this.fieldSym.SetCustomAttribute(caBuilder, out excp);
                if (excp != null)
                {
                    this.compiler.Error(ERRORKIND.ERROR, excp);
                }
            }
            else if (this.fieldSym.FixedAggSym != null && this.fieldSym.ConstVal.GetInt() > 0)
            {
                // Create an attribute to persist the element type and length of the fixed buffer
                AGGTYPESYM atsFixed = compiler.GetOptPredefTypeErr(PREDEFTYPE.FIXEDBUFFER, true);
                if (atsFixed != null)
                {
                    FieldAttrBind attrbind = new FieldAttrBind(compiler, this.fieldSym, this.fieldInfo, this.aggInfo);
                    attrbind.ProcessSynthAttr(
                        atsFixed,
                        compiler.FuncBRec.BindSimplePredefinedAttribute(
                            PREDEFTYPE.FIXEDBUFFER,
                            compiler.FuncBRec.BindFixedBufferArgs(this.fieldSym)),
                        null);
                }
            }
        }

        //------------------------------------------------------------
        // FieldAttrBind.EmitCustomAttribute
        //
        /// <summary></summary>
        /// <param name="caBuilder"></param>
        //------------------------------------------------------------
        internal override void EmitCustomAttribute(CustomAttributeBuilder caBuilder)
        {
            if (caBuilder == null)
            {
                return;
            }
            if (this.fieldSym == null || this.fieldSym.FieldBuilder == null)
            {
                return;
            }

            try
            {
                this.fieldSym.FieldBuilder.SetCustomAttribute(caBuilder);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    //======================================================================
    // class UnknownAttrBind
    //
    /// <summary>
    /// Attribute binder for attributes with an unknown target.
    /// The parser will have produced a warning on any of these.
    /// </summary>
    //======================================================================
    internal class UnknownAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // UnknownAttrBind.Compile (1)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="context"></param>
        /// <param name="attributes"></param>
        //------------------------------------------------------------
        static internal void Compile(COMPILER compiler, PARENTSYM context, BASENODE attributes)
        {
            if (attributes == null) return;

            UnknownAttrBind attrbind = new UnknownAttrBind(compiler, context);
            attrbind.ProcessAll(attributes, null);
        }

        //------------------------------------------------------------
        // UnknownAttrBind.Compile (2)
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        static internal void Compile(COMPILER compiler, GLOBALATTRSYM sym)
        {
            while (sym != null)
            {
                Compile(compiler, sym.ParentSym, sym.ParseTreeNode);
                sym = sym.NextAttributeSym;
            }
        }

        //------------------------------------------------------------
        // UnknownAttrBind Constructor
        //------------------------------------------------------------
        protected UnknownAttrBind(COMPILER compiler, PARENTSYM context) :
            base(compiler, false)
        {
            this.attributeTargets = AttributeTargets.All;
            // ReturnValue is not defined in CorAttributeTargets.
            this.contextSym = context;
            this.attrTarget = ATTRTARGET.UNKNOWN;
        }

        //------------------------------------------------------------
        // UnknownAttrBind.VerifyAndEmitPredef
        //------------------------------------------------------------
        new protected void VerifyAndEmitPredef(ATTRNODE attr) { }

        //------------------------------------------------------------
        // UnknownAttrBind.VerifyAndEmitCore
        //------------------------------------------------------------
        new protected void VerifyAndEmitCore(ATTRNODE attr) { }
    }

    //======================================================================
    // class CompilerGeneratedAttrBind
    //
    /// <summary>
    /// Attribute binder for emitting the CompilerGeneratedAttribute.
    /// </summary>
    //======================================================================
    internal class CompilerGeneratedAttrBind : AttrBind
    {
        static private CustomAttributeBuilder compilerGeneratedAttributeBuilder = null;

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind Constructor
        //------------------------------------------------------------
        internal CompilerGeneratedAttrBind(COMPILER compiler, bool isEarly)
            : base(compiler, isEarly)
        {
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.GetCompilerGeneratedAttribute (static)
        //
        /// <summary>
        /// Return the CustomAttributeBuilder
        /// of System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        //------------------------------------------------------------
        internal static CustomAttributeBuilder GetCompilerGeneratedAttribute(COMPILER compiler)
        {
            if (compilerGeneratedAttributeBuilder != null)
            {
                return compilerGeneratedAttributeBuilder;
            }

            //DWORD blob = 0x00000001;  // store the version number only in the blob (no attributes).
            EXPR expr = compiler.FuncBRec.BindSimplePredefinedAttribute(
                PREDEFTYPE.COMPILERGENERATED);
            EXPRCALL ctorExpr = expr as EXPRCALL;

            if (ctorExpr == null ||
                ctorExpr.MethodWithInst == null ||
                ctorExpr.MethodWithInst.IsNull ||
                ctorExpr.MethodWithInst.MethSym == null)
            {
                DebugUtil.VsFail("CompilerGeneratedAttrBind.GetCompilerGeneratedAttribute");
                return null;
            }

            ConstructorInfo ctorInfo = ctorExpr.MethodWithInst.MethSym.ConstructorInfo;
            if (ctorInfo == null)
            {
                DebugUtil.VsFail("CompilerGeneratedAttrBind.GetCompilerGeneratedAttribute");
                return null;
            }

            try
            {
                compilerGeneratedAttributeBuilder = new CustomAttributeBuilder(
                    ctorInfo,
                    new object[] { });
            }
            catch (ArgumentException)
            {
                DebugUtil.VsFail("CompilerGeneratedAttrBind.GetCompilerGeneratedAttribute");
                return null;
            }

            return compilerGeneratedAttributeBuilder;
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (1) Constructor (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, ConstructorBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (1)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (2) Event (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, EventBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (2)");
                }
                catch (InvalidOperationException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (2)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (3) Field (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, FieldBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (3)");
                }
                catch (InvalidOperationException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (3)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (4) GenericTypeParameter (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, GenericTypeParameterBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (4)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (5) Method (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, MethodBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (5)");
                }
                catch (InvalidOperationException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (5)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (6) Parameter (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, ParameterBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (6)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (7) Property (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, PropertyBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (7)");
                }
                catch (InvalidOperationException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (7)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute (8) Type (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, TypeBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                CustomAttributeBuilder caBuilder = GetCompilerGeneratedAttribute(compiler);
                DebugUtil.Assert(caBuilder != null);

                try
                {
                    builder.SetCustomAttribute(caBuilder);
                }
                catch (ArgumentException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (8)");
                }
                catch (InvalidOperationException)
                {
                    DebugUtil.VsFail("CompilerGeneratedAttrBind.EmitAttribute (8)");
                }
            }
        }

        //------------------------------------------------------------
        // CompilerGeneratedAttrBind.EmitAttribute () Sym (static)
        //
        /// <summary>
        /// Set System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="builder"></param>
        //------------------------------------------------------------
        internal static void EmitAttribute(COMPILER compiler, SYM sym)
        {
            if (sym== null)
            {
                return;
            }

            switch (sym.Kind)
            {
                case SYMKIND.METHSYM:
                    EmitAttribute(compiler, (sym as METHSYM).MethodBuilder);
                    return;

                case SYMKIND.PROPSYM:
                    EmitAttribute(compiler, (sym as PROPSYM).PropertyBuilder);
                    return;

                case SYMKIND.MEMBVARSYM:
                    EmitAttribute(compiler, (sym as MEMBVARSYM).FieldBuilder);
                    return;

                case SYMKIND.TYVARSYM:
                    EmitAttribute(
                        compiler,
                        ((sym as TYVARSYM).GetGenericParameterType()) as GenericTypeParameterBuilder);
                    return;

                case SYMKIND.EVENTSYM:
                    EmitAttribute(compiler, (sym as EVENTSYM).EventBuilder);
                    return;

                case SYMKIND.AGGSYM:
                    EmitAttribute(compiler, (sym as AGGSYM).TypeBuilder);
                    return;

                default:
                    DebugUtil.VsFail("Bad Symbol type");
                    break;
            }
        }
    }

    //======================================================================
    // class UnsafeValueTypeAttrBind
    //
    /// <summary>
    /// Attribute binder for emitting the UnsafeValueTypeAttribute.
    /// </summary>
    //======================================================================
    internal class UnsafeValueTypeAttrBind : AttrBind
    {
        //------------------------------------------------------------
        // UnsafeValueTypeAttrBind Constructor
        //------------------------------------------------------------
        public UnsafeValueTypeAttrBind(COMPILER compiler, bool isEarly)
            : base(compiler, isEarly)
        {
        }

        //------------------------------------------------------------
        // UnsafeValueTypeAttrBind.EmitAttribute
        //
        /// <summary></summary>
        /// <param name="compiler"></param>
        /// <param name="token"></param>
        //------------------------------------------------------------
        static internal void EmitAttribute(COMPILER compiler)//, mdToken token)
        {
            //DebugUtil.Assert(token);
            //if (!compiler.Options.NoCodeGen && !compiler.Options.CompileSkeleton)
            {
                //int blob = 0x00000001;  // store the version number only in the blob (no attributes).
                EXPR ctorExpr = compiler.FuncBRec.BindSimplePredefinedAttribute(
                    PREDEFTYPE.UNSAFEVALUETYPE);
                if (ctorExpr != null)
                {
                    throw new NotImplementedException("UnsafeValueTypeAttrBind.EmitAttribute");
                    //compiler.Emitter.EmitCustomAttribute(
                    //    null,
                    //    token,
                    //    ctorExpr.AsCALL.MethodWithInst.MethSym,
                    //    (BYTE*)&blob,
                    //    sizeof(DWORD));
                }
            }
        }
    }

    //======================================================================
    // class AttrUtil
    //======================================================================
    static internal class AttrUtil
    {
        //------------------------------------------------------------
        // AttrUtil.AttributeTargetStrings
        //------------------------------------------------------------
        static internal string[] AttributeTargetStrings =
        {
            "assembly",            // catAssembly      = 0x0001, 
            "module",              // catModule        = 0x0002,
            "class",               // catClass         = 0x0004,
            "struct",              // catStruct        = 0x0008,
            "enum",                // catEnum          = 0x0010,
            "constructor",         // catConstructor   = 0x0020,
            "method",              // catMethod        = 0x0040,
            "property, indexer",   // catProperty      = 0x0080,
            "field",               // catField         = 0x0100,
            "event",               // catEvent         = 0x0200,
            "interface",           // catInterface     = 0x0400,
            "param",               // catParameter     = 0x0800,
            "delegate",            // catDelegate      = 0x1000,
            "return",              // catReturn        = 0x2000,
            "type parameter",      // catGenericParameter = 0x4000,
            //null
        };

        //------------------------------------------------------------
        // AttrUtil.BuildAttrTargetString
        //
        /// <summary>
        /// convert valid targets to string
        /// </summary>
        /// <param name="validTargets"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static string BuildAttrTargetString(AttributeTargets validTargets)
        {
            StringBuilder sb = new StringBuilder();
            uint targetsFlag = (uint)validTargets;
            int length = AttributeTargetStrings.Length;

            for (int i = 0; targetsFlag != 0 && i < length; ++i)
            {
                if ((targetsFlag & 1) != 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(AttributeTargetStrings[i]);
                }
                targetsFlag >>= 1;
            }
            return sb.ToString();
        }

        //------------------------------------------------------------
        // AttrUtil.StoreTypeName
        //
        /// <summary>
        /// Stores a type name into a blob
        /// </summary>
        //------------------------------------------------------------
        static internal void StoreTypeName(
            List<object> blob,
            TYPESYM type,
            COMPILER compiler,
            bool fOpenType)
        {
            //if (type) {
            //    TypeNameSerializer tns(compiler);
            //    BSTR bstr = tns.GetAssemblyQualifiedTypeName(type, fOpenType);
            //
            //    StoreString(blob, bstr, SysStringLen(bstr));
            //    SysFreeString(bstr);
            //}
            //else
            //    StoreString(blob, NULL, 0);
        }

        //------------------------------------------------------------
        // AttrUtil.StoreEncodedType
        //------------------------------------------------------------
        static internal void StoreEncodedType(
            List<Object> valueList,
            TYPESYM typeSym,
            COMPILER compiler)
        {
            switch (typeSym.Kind)
            {
                case SYMKIND.ARRAYSYM:
                    ARRAYSYM arrSym = typeSym as ARRAYSYM;
                    DebugUtil.Assert(arrSym != null && arrSym.Rank == 1);
                    //blob.Add(CorSerializationType.SZARRAY);
                    StoreEncodedType(valueList, arrSym.ElementTypeSym, compiler);
                    break;

                case SYMKIND.AGGTYPESYM:
                    if (typeSym.IsEnumType())
                    {
                        DebugUtil.Assert((typeSym as AGGTYPESYM).AllTypeArguments.Count == 0);
                        //blob.Add(CorSerializationType.ENUM);
                        //StoreTypeName(valueList, typeSym, compiler, false);
                    }
                    else
                    {
                        DebugUtil.Assert(
                            typeSym.IsPredefined() &&
                            (typeSym as AGGTYPESYM).AllTypeArguments.Count == 0);
                        byte b = (byte)PredefType.SerializationTypeTable[(int)typeSym.GetPredefType()];
                        DebugUtil.Assert(b != 0);
                        valueList.Add(b);
                    }
                    break;

                default:
                    DebugUtil.Assert(false, "unrecognized attribute typeSym");
                    break;
            }
        }

        //------------------------------------------------------------
        // AttrUtil.StoreEncodedType
        //
        /// <summary></summary>
        /// <param name="blob"></param>
        /// <param name="x"></param>
        //------------------------------------------------------------
        static internal void StoreDWORD(List<object> blob, uint x)
        {
            //x = VAL32(x);
            blob.Add((object)x);
        }
    }
}
