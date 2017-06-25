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
// File: error.h
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
// File: error.cpp
//
// Error handling for the compiler
// ===========================================================================

//============================================================================
// Error.cs
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Uncs
{
    // This encapsulates a variable set of arguments that can be processed by
    // FormatMessage using the FORMAT_MESSAGE_ARGUMENT_ARRAY option. Each
    // argument occupies sizeof(INT_PTR) bytes. Slots can contain either pointers
    // or scalars. Some slots MAY NOT BE pointers!

    //internal class VarArgList
    //{
    //    internal string args;
    //
    //    internal VarArgList(string args) { this.args = args; }
    //    internal string Args() { return args; }
    //}

    // New Error API.
    // ErrArgTyped - a pair consisting of a SYM and a containing AGGTYPESYM.   // not used.

    //======================================================================
    // enum ErrArgKindEnum
    //
    /// <summary>
    /// ErrArgKind - the kind of argument value.
    /// (CSharp\SCComp\Error.cs)
    /// </summary>
    //======================================================================
    internal enum ErrArgKindEnum : int
    {
        Null,

        Int,
        ErrID,
        ResNo,   //Ids,
        SymKind,
        AggKind,
        Sym,
        Name,
        Str,
        PredefName,
        LocNode,
        NameNode,
        TypeNode,
        Ptr,
        SymWithType,
        MethWithInst,

        Lim
    }

    //======================================================================
    // enum ErrArgFlagsEnum
    //
    /// <summary>
    /// None, Ref, NoStr, RefOnly, Unique (uncs\CSharp\SCComp\Error.cs)
    /// (CSharp\SCComp\Error.cs)
    /// </summary>
    //======================================================================
    [Flags]
    internal enum ErrArgFlagsEnum : int
    {
        None = 0x0000,

        /// <summary>
        /// The arg's location should be included in the error message
        /// </summary>
        Ref = 0x0001,

        /// <summary>
        /// The arg should NOT be included in the error message, just the location
        /// </summary>
        NoStr = 0x0002,

        /// <summary>
        /// Ref or NoStr. 
        /// Just the arg's location should be included in the error message
        /// </summary>
        RefOnly = Ref | NoStr,

        /// <summary>
        /// The string should be distinct from other args marked with Unique
        /// </summary>
        Unique = 0x0004,
    }

    //======================================================================
    // class ErrArg
    //
    /// <summary>
    /// <para>ErrArg - the argument value and kind of value.
    /// This has constructors that provide implicit conversions from typical types to ErrArg.</para>
    /// <para>This class is for arguments of error handling methods.</para>
    /// <para>(CSharp\SCComp\Error.cs)</para>
    /// </summary>
    //======================================================================
    internal class ErrArg
    {
        //------------------------------------------------------------
        // ErrArg   Fields
        //------------------------------------------------------------

        /// <summary>
        /// Type of data.
        /// </summary>
        internal ErrArgKindEnum ErrorArgumentKind = ErrArgKindEnum.Null;

        /// <summary>
        /// Flags.
        /// </summary>
        internal ErrArgFlagsEnum ErrorArgumentFlags = 0;


        /// <summary>
        /// Data of error information.
        /// </summary>
        /// <remarks>
        /// Use in place of the union (in remark below).
        /// <code>
        ///union {
        ///    int n;
        ///    int ids;
        ///    SYMKIND sk;
        ///    AggKindEnum ak;
        ///    SYM * sym;
        ///    NAME * name;
        ///    PCWSTR psz;
        ///    PREDEFNAME pdn;
        ///    BASENODE * node;
        ///    NAMENODE * nameNode;
        ///    TYPEBASENODE * typeNode;
        ///    const SymWithType * pswt;
        ///    const MethPropWithInst * pmpwi;
        ///    void * ptr;
        ///};
        /// </code>
        /// </remarks>
        protected object data = null;

        //------------------------------------------------------------
        // ErrArg   Properties
        //------------------------------------------------------------

        /// <summary>
        /// (RW) Use data as type of int.
        /// </summary>
        internal int Int
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.Int);
                return (int)data;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.Int;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as type of CSCERRID.
        /// </summary>
        internal CSCERRID ErrID
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.ErrID);
                return (CSCERRID)data;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.ErrID;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as type of string.
        /// </summary>
        internal ResNo ResNo
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.ResNo);
                return (ResNo)data;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.ResNo;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as type of SYMKIND.
        /// </summary>
        internal SYMKIND SymKind
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.SymKind);
                return (SYMKIND)data;
            }
            set
            {
                // public NSAIDSYMs are treated differently based on the SYM not the SK
                if (value != SYMKIND.NSAIDSYM)
                {
                    this.ErrorArgumentKind = ErrArgKindEnum.SymKind;
                    this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                    data = value;
                }
            }
        }

        /// <summary>
        /// (RW) Use data as type of AggKindEnum.
        /// </summary>
        internal AggKindEnum AggKind
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.AggKind);
                return (AggKindEnum)data;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.AggKind;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as SYM instance.
        /// </summary>
        internal SYM Sym
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.Sym);
                return data as SYM;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.Sym;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as type of string.
        /// </summary>
        internal string Name
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.Str);
                return data as string;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.Str;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as type of string.
        /// </summary>
        internal string Str
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.Str);
                return data as string;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.Str;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as type of PREDEFNAME.
        /// </summary>
        internal PREDEFNAME PredefinedName
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.PredefName);
                return (PREDEFNAME)data;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.PredefName;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        internal BASENODE BaseNode
        {
            get { return data as BASENODE; }
            set { data = value; }
        }

        /// <summary>
        /// (RW) Use data as BASENODE instance.
        /// </summary>
        internal BASENODE LocNode
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.LocNode);
                return data as BASENODE;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.LocNode;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as NAMENODE instance.
        /// </summary>
        internal NAMENODE NameNode
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.NameNode);
                return data as NAMENODE;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.NameNode;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as TYPENODE instance.
        /// </summary>
        internal TYPEBASENODE TypeNode
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.TypeNode);
                return data as TYPEBASENODE;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.TypeNode;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as SymWithType instance.
        /// </summary>
        internal SymWithType SymWithType
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.SymWithType);
                return data as SymWithType;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.SymWithType;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as MethPropWithInst instance.
        /// </summary>
        internal MethPropWithInst MethPropWithInst
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.MethWithInst);
                return data as MethPropWithInst;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.MethWithInst;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (RW) Use data as type of Object.
        /// </summary>
        internal object Ptr
        {
            get
            {
                DebugUtil.Assert(this.ErrorArgumentKind == ErrArgKindEnum.Ptr);
                return data;
            }
            set
            {
                this.ErrorArgumentKind = ErrArgKindEnum.Ptr;
                this.ErrorArgumentFlags = ErrArgFlagsEnum.None;
                data = value;
            }
        }

        /// <summary>
        /// (R) the raw data object.
        /// </summary>
        internal object DataObject
        {
            get { return this.data; }
        }

        //------------------------------------------------------------
        // ErrArg   Constructor
        //------------------------------------------------------------
        /// <summary>
        /// Constructor. Does nothing.
        /// </summary>
        internal ErrArg() { }

        /// <summary>
        /// Constructer. Set a value of type int to data.
        /// </summary>
        /// <param name="n"></param>
        internal ErrArg(int n)
        {
            this.Int = n;
        }

        /// <summary>
        /// Constructer. Set a value of type CSCERRID to data.
        /// </summary>
        /// <param name="id"></param>
        internal ErrArg(CSCERRID id)
        {
            this.ErrID = id;
        }

        /// <summary>
        /// Constructer. Set a value of type CSCSTRID to data.
        /// </summary>
        /// <param name="id"></param>
        //internal ErrArg(CSCSTRID id)
        //{
        //    this.StrID = id;
        //}

        /// <summary>
        /// <para>Constructer. Set a value of type SYMKIND to data.</para>
        /// <para>Do not specify SYMKIND.NSAIDSYM.
        /// NSAIDSYMs are treated differently based on the SYM not the SK</para>
        /// </summary>
        /// <param name="sk"></param>
        internal ErrArg(SYMKIND sk)
        {
            DebugUtil.Assert(sk != SYMKIND.NSAIDSYM);
            this.SymKind = sk;
        }

        /// <summary>
        /// Constructer. Set a value of type string to data.
        /// </summary>
        /// <param name="psz"></param>
        internal ErrArg(string str)
        {
            this.Str = str;
        }

        /// <summary>
        /// Constructer. Set a value of type PREDEFNAME to data.
        /// </summary>
        /// <param name="pdn"></param>
        internal ErrArg(PREDEFNAME pdn)
        {
            this.PredefinedName = pdn;
        }

        /// <summary>
        /// Constructer. Set a SYM instance to data.
        /// </summary>
        /// <param name="sym"></param>
        internal ErrArg(SYM sym)
        {
            this.Sym = sym;
        }

        /// <summary>
        /// Constructer. Set a NAMENODE instance to data.
        /// </summary>
        /// <param name="nameNode"></param>
        internal ErrArg(NAMENODE nameNode)
        {
            this.NameNode = nameNode;
        }

        /// <summary>
        /// Constructer. Set a TYPENODE instance to data.
        /// </summary>
        /// <param name="typeNode"></param>
        internal ErrArg(TYPEBASENODE typeNode)
        {
            this.TypeNode = typeNode;
        }

        /// <summary>
        /// Constructer. Set a SymWithType instance to data.
        /// </summary>
        /// <param name="swt"></param>
        internal ErrArg(SymWithType swt)
        {
            this.SymWithType = swt;
        }

        /// <summary>
        /// Constructer. Set a MethPropWithInst instance to data.
        /// </summary>
        /// <param name="mpwi"></param>
        internal ErrArg(MethPropWithInst mpwi)
        {
            this.MethPropWithInst = mpwi;
        }

        /// <summary>
        /// Constructor. Arguments are SYM instance and a value of type ErrArgKindEnum.
        /// </summary>
        internal ErrArg(SYM sym, ErrArgFlagsEnum eaf)
        {
            this.ErrorArgumentKind = ErrArgKindEnum.Sym;
            this.ErrorArgumentFlags = eaf;
            this.data = sym;
        }

        /// <summary>
        /// Constructor. Arguments are NAMENODE instance and a value of type ErrArgKindEnum.
        /// </summary>
        internal ErrArg(NAMENODE nameNode, ErrArgFlagsEnum eaf)
        {
            this.ErrorArgumentKind = ErrArgKindEnum.NameNode;
            this.ErrorArgumentFlags = eaf;
            this.data = nameNode;
        }

        /// <summary>
        /// Constructor. Arguments are TYPEBASENODE instance and a value of type ErrArgKindEnum.
        /// </summary>
        internal ErrArg(TYPEBASENODE typeNode, ErrArgFlagsEnum eaf)
        {
            this.ErrorArgumentKind = ErrArgKindEnum.TypeNode;
            this.ErrorArgumentFlags = eaf;
            this.data = typeNode;
        }

        /// <summary>
        /// Constructor. Arguments are SymWithType instance and a value of type ErrArgKindEnum.
        /// </summary>
        internal ErrArg(SymWithType swt, ErrArgFlagsEnum eaf)
        {
            this.ErrorArgumentKind = ErrArgKindEnum.SymWithType;
            this.ErrorArgumentFlags = eaf;
            this.data = swt;
        }

        /// <summary>
        /// Constructor. Arguments are MethPropWithInst instance and a value of type ErrArgKindEnum.
        /// </summary>
        internal ErrArg(MethPropWithInst mpwi, ErrArgFlagsEnum eaf)
        {
            this.ErrorArgumentKind = ErrArgKindEnum.MethWithInst;
            this.ErrorArgumentFlags = eaf;
            this.data = mpwi;
        }

        //------------------------------------------------------------
        // ErrArg.ConvertBasic
        //
        /// <summary>
        /// <para>Returns true if the pointer produced is really a string.</para>
        /// <para>Convert data to string and set it to dataToStr.</para>
        /// </summary>
        //------------------------------------------------------------
        internal bool ConvertBasic(out string dataToStr)
        {
            DebugUtil.Assert(this.ErrorArgumentFlags == 0);

            switch (this.ErrorArgumentKind)
            {
                //case ErrArgKindEnum.Int:
                //    //dataToStr = (PCWSTR)(INT_PTR)this.n;
                //    return false;

                case ErrArgKindEnum.Str:
                    dataToStr = this.data as string;
                    return true;

                case ErrArgKindEnum.Ptr:
                    //dataToStr = (PCWSTR)(INT_PTR)this.ptr;
                    dataToStr = this.data.ToString();
                    return false;

                case ErrArgKindEnum.Name:
                    dataToStr = this.data as string;
                    return true;

                default:
                    //VSFAIL("Unhandled ErrArg kind in ErrArg::ConvertBasic");
                    dataToStr = "";
                    return false;
            }
        }

        //------------------------------------------------------------
        // ErrArg.ConvertAndCleanArgToStack (1)
        //
        /// <summary>
        /// Convert data to string and replace invalid characters with '?'. Then output it.
        /// </summary>
        //------------------------------------------------------------
        internal string ConvertAndCleanArgToStack()
        {
            string temp = null;
            this.ConvertBasic(out temp);
            CharUtil.ClobberBadChars(ref temp);
            return temp;
        }

        //------------------------------------------------------------
        // ErrArg.ToString
        //------------------------------------------------------------
        public override string ToString()
        {
            return this.data.ToString();
        }
    }

    //======================================================================
    // class ErrArgRef
    //
    /// <summary>
    /// <para>Derives from ErrArg.</para>
    /// <para>Set Ref flag in ErrorArgumentFlags except int, SYMKIND, string, PREDEFNAME.</para>
    /// <para>Ref flag means that the arg's location should be included in the error message</para>
    /// </summary>
    //======================================================================
    internal class ErrArgRef : ErrArg
    {
        /// <summary>
        /// Constructor. Does nothing.
        /// </summary>
        internal ErrArgRef() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(int n) : base(n) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(SYMKIND sk) : base(sk) { }

        //internal ErrArgRef(NAME  name) : base(name) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(string psz) : base(psz) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(PREDEFNAME pdn) : base(pdn) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(SYM sym)
            : base(sym)
        {
            this.ErrorArgumentFlags = ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(NAMENODE nodeName)
            : base(nodeName)
        {
            this.ErrorArgumentFlags = ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(TYPEBASENODE nodeType)
            : base(nodeType)
        {
            this.ErrorArgumentFlags = ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(SymWithType swt)
            : base(swt)
        {
            this.ErrorArgumentFlags = ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(MethPropWithInst mpwi)
            : base(mpwi)
        {
            this.ErrorArgumentFlags = ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(SYM sym, ErrArgFlagsEnum eaf)
            : base(sym)
        {
            this.ErrorArgumentFlags = eaf | ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(NAMENODE nodeName, ErrArgFlagsEnum eaf)
            : base(nodeName)
        {
            this.ErrorArgumentFlags = eaf | ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(TYPEBASENODE nodeType, ErrArgFlagsEnum eaf)
            : base(nodeType)
        {
            this.ErrorArgumentFlags = eaf | ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(SymWithType swt, ErrArgFlagsEnum eaf)
            : base(swt)
        {
            this.ErrorArgumentFlags = eaf | ErrArgFlagsEnum.Ref;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgRef(MethPropWithInst mpwi, ErrArgFlagsEnum eaf)
            : base(mpwi)
        {
            this.ErrorArgumentFlags = eaf | ErrArgFlagsEnum.Ref;
        }
    }

    //======================================================================
    // class ErrArgRefOnly
    //
    /// <summary>
    /// <para>Derives from ErrArgRef.</para>
    /// <para>Set RefOnly flag in ErrorArgumentFlags for SYM and BASENODE.
    /// And then, Set ErrArgKindEnum.LocNode to Kind for BASENODE.</para>
    /// </summary>
    //======================================================================
    internal class ErrArgRefOnly : ErrArgRef
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sym"></param>
        internal ErrArgRefOnly(SYM sym)
            : base(sym)
        {
            this.ErrorArgumentFlags = ErrArgFlagsEnum.RefOnly;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node"></param>
        internal ErrArgRefOnly(BASENODE node)
        {
            this.ErrorArgumentKind = ErrArgKindEnum.LocNode;
            this.ErrorArgumentFlags = ErrArgFlagsEnum.RefOnly;
            this.data = node;
        }
    }

    //======================================================================
    // class ErrArgNoRef
    //
    /// <summary>
    /// This is used with COMPILER.ErrorRef method to indicate no reference.
    /// </summary>
    //======================================================================
    internal class ErrArgNoRef : ErrArgRef
    {
        internal ErrArgNoRef(SYM sym)
        {
            this.Sym = sym;
        }
        internal ErrArgNoRef(SymWithType swt)
        {
            this.SymWithType = swt;
        }
        internal ErrArgNoRef(MethPropWithInst mpwi)
        {
            this.MethPropWithInst = mpwi;
        }
    }

    //======================================================================
    // class ErrArgResNo (ErrArgIds in sscli)
    //
    /// <summary>
    /// Derives from ErrArgRef. Have a value of type CSCERRID.
    /// </summary>
    //======================================================================
    internal class ErrArgResNo : ErrArgRef
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgResNo(ResNo resNo)
        {
            this.ResNo = resNo;
        }
    }

    //======================================================================
    // class ErrArgPtr
    //
    /// <summary>
    /// Derives from ErrArgRef. Have a data of reference type.
    /// </summary>
    //======================================================================
    internal class ErrArgPtr : ErrArgRef
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgPtr(Object ptr)
        {
            this.Ptr = ptr;
        }
    }

    //======================================================================
    // class ErrArgLocNode
    //
    /// <summary>
    /// Derives from ErrArgRef. Have a BASENODE instance.
    /// </summary>
    //======================================================================
    internal class ErrArgLocNode : ErrArgRef
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgLocNode(BASENODE node)
        {
            this.LocNode = node;
            this.ErrorArgumentFlags = ErrArgFlagsEnum.RefOnly;
        }
    }

    //======================================================================
    // class ErrArgNameNode
    //
    /// <summary>
    /// <para>Derives from ErrArgRef. Have a NAMENODE instance.</para>
    /// <para>(In sscli, eaf has defalt value ErrArgFlags::None.)</para>
    /// </summary>
    //======================================================================
    internal class ErrArgNameNode : ErrArgRef
    {
        /// <summary>
        /// <para>Constructor.</para>
        /// <para>(In sscli, eaf has defalt value ErrArgFlags::None.)</para>
        /// </summary>
        internal ErrArgNameNode(
            BASENODE node,
            ErrArgFlagsEnum eaf)    // = ErrArgFlags::None
        {
            this.BaseNode = node;
            this.ErrorArgumentKind = ErrArgKindEnum.NameNode;
            this.ErrorArgumentFlags = eaf;
        }
    }

    //======================================================================
    // class ErrArgTypeNode
    //
    /// <summary>
    /// Derives from ErrArgRef. Have a TYPENODE instance.
    /// </summary>
    //======================================================================
    internal class ErrArgTypeNode : ErrArgRef
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgTypeNode(BASENODE node, ErrArgFlagsEnum eaf)
        {
            TYPEBASENODE tbNode = node as TYPEBASENODE;
            DebugUtil.Assert(tbNode != null);
            this.TypeNode = tbNode;
            this.ErrorArgumentFlags = eaf;
        }
    }

    //======================================================================
    // class ErrArgSymKind
    //
    /// <summary>
    /// Derives from ErrArgRef. Have a SYM instance.
    /// </summary>
    //======================================================================
    internal class ErrArgSymKind : ErrArgRef
    {
        // Can't be inlined because of SYM not being defined yet
        /// <summary>
        /// Constructor.
        /// </summary>
        internal ErrArgSymKind(SYM sym)
        {
            DebugUtil.Assert(sym != null);

            this.SymKind = sym.Kind;
            if (this.SymKind == SYMKIND.NSAIDSYM)
            {
                if ((sym as NSAIDSYM).NamespaceSym.Name.Length > 0)
                {
                    // Non-empty namespace name means it's not the root
                    // so treat it like a namespace instead of an alias
                    this.SymKind = SYMKIND.NSSYM;
                }
                else
                {
                    // An empty namespace name means it's just an alias for the root
                    this.SymKind = SYMKIND.EXTERNALIASSYM;
                }
            }
        }
    }

    //======================================================================
    // class ErrArgAggKind
    //======================================================================
    internal class ErrArgAggKind : ErrArgRef
    {
        // Can't be inlined because of AGGSYM not being defined yet
        internal ErrArgAggKind(AGGSYM agg)
        {
            this.AggKind = agg.AggKind;
        }

        internal ErrArgAggKind(AGGTYPESYM ats)
        {
            this.AggKind = ats.GetAggregate().AggKind;
        }
    }

    //======================================================================
    // CError
    //
    /// <summary>
    /// <para>This object is the implementation of ISCError for all compiler errors,
    /// including lexer, parser, and compiler errors.</para>
    /// </summary>
    //======================================================================
    internal class CError
    {
        //============================================================
        // class CError.LOCATION
        //
        /// <summary>
        /// This class stores a range in source files.
        /// This has a start POSDATA, end POSDATA, and file name.
        /// (Other class with the same name is defined in compiler.cs. Do not confuse.)
        /// </summary>
        //============================================================
        private class LOCATION
        {
            internal string FileName = null;
            internal POSDATA Start = null;
            internal POSDATA End = null;

            //--------------------------------------------------------
            // CError.LOCATION.SameFile (1)
            //--------------------------------------------------------
            internal bool SameFile(string fileName)
            {
                if (String.IsNullOrEmpty(this.FileName) ||
                    String.IsNullOrEmpty(fileName))
                {
                    return false;
                }
                return (String.Compare(this.FileName, fileName, true) == 0);
            }

            //--------------------------------------------------------
            // CError.LOCATION.SameFile (2)
            //--------------------------------------------------------
            internal bool SameFile(LOCATION loc)
            {
                if (loc == null)
                {
                    return false;
                }
                return SameFile(loc.FileName);
            }
        }

        //------------------------------------------------------------
        // CError Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// The kind of error, one of FATAL, ERROR, WARNING.
        /// </summary>
        private ERRORKIND errorKind = ERRORKIND.NONE;

        /// <summary>
        /// (R) The kind of error, one of FATAL, ERROR, WARNING.
        /// </summary>
        internal ERRORKIND Kind
        {
            get { return errorKind; }
        }

        /// <summary>
        /// Error messaga.
        /// </summary>
        private string errorText = null;

        /// <summary>
        /// (R) Error messaga.
        /// </summary>
        internal string Text
        {
            get { return errorText; }
        }

        /// <summary>
        /// Error ID of type CSCERRID.
        /// </summary>
        private CSCERRID errorID = CSCERRID.Invalid;

        /// <summary>
        /// (R) Error ID of type CSCERRID.
        /// </summary>
        internal CSCERRID ErrorID
        {
            get { return errorID; } // ID()
        }

        /// <summary></summary>
        private bool wasWarning = false;
        internal bool WasWarning
        {
            get { return wasWarning; }
        }

        /// <summary>
        /// System.Exception object.
        /// </summary>
        private Exception exception = null;

        /// <summary>
        /// (R)System.Exception object.
        /// </summary>
        internal Exception Exception
        {
            get { return this.exception; }
        }

        private List<LOCATION> locationList = new List<LOCATION>();

        /// <summary>
        /// (R) locationList.Count
        /// </summary>
        internal int LocationCount
        {
            get { return locationList.Count; }
        }

        private List<LOCATION> mappedLocationList = new List<LOCATION>();

        private List<string> errorMessages = new List<string>();

        /// <summary>
        /// Set true when counted by Controller.
        /// </summary>
        internal bool Counted = false;

        //------------------------------------------------------------
        // CError   Constructor
        //
        /// <summary>Constructor, Does nothing.</summary>
        //------------------------------------------------------------
        internal CError() { }

        //internal bool ComputeString(out string buffer, CSCERRID id, params Object[] args) { return false; }
        // Use Util.FormatErrorMessage.

        //------------------------------------------------------------
        // CError.Initialize    (1)
        //
        /// <summary>
        /// Set fields and create an error messages.
        /// </summary>
        /// <param name="kind">Error level</param>
        /// <param name="warning"></param>
        /// <param name="id">CSCERRID value of error.</param>
        /// <param name="args">Arguments to error message format.</param>
        /// <returns>Return false if failed to make the error text. </returns>
        //------------------------------------------------------------
        internal bool Initialize(
            ERRORKIND kind,
            bool warning,
            CSCERRID id,
            params Object[] args)
        {
            this.errorKind = kind;
            this.errorID = id;
            this.wasWarning = warning;

            Exception excp = null;
            if (CSCErrorInfo.Manager.FormatErrorMessage(out this.errorText, out excp, id, args))
            {
                return true;
            }

            if (excp != null)
            {
                this.errorText = excp.Message;
            }
            else
            {
                this.errorText = ExceptionUtil.InternalErrorMessage("CError.Initialize");
            }
            return false;
        }

        //------------------------------------------------------------
        // CError.Initialize    (2)
        //
        /// <summary>
        /// Set fields and create an error messages.
        /// Get the error lever from id and set it to errorKind field.
        /// </summary>
        /// <param name="id">CSCERRID value of error.</param>
        /// <param name="args">Arguments to error message format.</param>
        /// <returns>Return false if failed to make the error text. </returns>
        //------------------------------------------------------------
        internal bool Initialize(CSCERRID id, params Object[] args)
        {
            ERRORKIND kind;
            int level;

            if (CSCErrorInfo.Manager.GetWarningLevel(id, out level))
            {
                if (level == 0)
                {
                    kind = ERRORKIND.ERROR;
                }
                else if (level > 0)
                {
                    kind = ERRORKIND.WARNING;
                }
                else
                {
                    kind = ERRORKIND.FATAL;
                }

                return Initialize(kind, (level > 0), id, args);
            }
            DebugUtil.Assert(false, "CError.Initialize");
            return false;
        }

        //------------------------------------------------------------
        // CError.Initialize    (3)
        //
        /// <summary>Set fields.</summary>
        /// <param name="kind">Error level.</param>
        /// <param name="text">Error message.</param>
        /// <returns>Return true.</returns>
        //------------------------------------------------------------
        internal bool Initialize(ERRORKIND kind, string text)
        {
            this.errorKind = kind;
            this.errorID = CSCERRID.Invalid;
            this.errorText = text;
            return true;
        }

        //------------------------------------------------------------
        // CError.Initialize    (4)
        //
        /// <summary>Set fields with the data of a given Exception instance.</summary>
        /// <param name="excp">System.Exception instance.</param>
        /// <returns>Return false if excp is null.</returns>
        //------------------------------------------------------------
        internal bool Initialize(ERRORKIND kind, Exception excp)
        {
            if (excp != null)
            {
                this.errorKind = kind;
                this.errorID = CSCERRID.Invalid;
                this.exception = excp;
                this.errorText = excp.Message;
                return true;
            }
            this.errorKind = ERRORKIND.NONE;
            this.errorText = null;
            return false;
        }

        //------------------------------------------------------------
        // CError.AddLocation
        //
        /// <summary>
        /// Save a error location and its mapped location.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <param name="mapFileName"></param>
        /// <param name="mapStart"></param>
        /// <param name="mapEnd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AddLocation(
            string fileName,
            POSDATA posStart,
            POSDATA posEnd,
            string mapFileName,
            POSDATA mapStart,
            POSDATA mapEnd)
        {
            if (fileName == null)
            {
                return false;
            }
            LOCATION loc, maploc;

            // Check for duplicates before adding the location
            // if the same location is already saved, does nothing and return true.

            for (int i = 0; i < locationList.Count; ++i)
            {
                loc = locationList[i];
                maploc = mappedLocationList[i];
                // Match up the locations and then the filenames (hope for an early out)
                if (
                    // Both locations (start and end positions) are same, or all are undefined.
                    ((posStart == null && loc.Start.IsUninitialized && loc.End.IsUninitialized) ||
                    (loc.Start.Equals(posStart) && loc.End.Equals(posEnd)))
                    &&
                    // Both mapped locations are same, or all are undefined.
                    ((mapStart == null && maploc.Start.IsUninitialized && maploc.End.IsUninitialized) ||
                    (maploc.Start == mapStart && maploc.End == mapEnd))
                    &&
                    // Source files are same.
                    loc.SameFile(fileName)
                    &&
                    // Mapped files are same.
                    maploc.SameFile(mapFileName)
                    )
                {
                    // We've already got this location
                    return true;
                }
            }

            // If the location is not saved,
            // save it in locationList and its mapped location in mappedLocationList.

            loc = new LOCATION();
            loc.FileName = fileName;
            if (posStart != null)
            {
                // Passing one means passing both!
                //if (posEnd == null) return false;
                loc.Start = posStart;
                if (posEnd != null)
                {
                    loc.End = posEnd;
                }
                else
                {
                    loc.End = new POSDATA();
                    loc.End.SetUninitialized();
                }
            }
            else
            {
                //DebugUtil.Assert(posEnd == null);
                loc.Start = new POSDATA();
                loc.End = new POSDATA();
                loc.Start.SetUninitialized();
                loc.End.SetUninitialized();
            }

            maploc = new LOCATION();
            maploc.FileName = mapFileName;
            if (mapStart != null)
            {
                // Passing one means passing both!
                //if (mapEnd == null) return false;
                maploc.Start = mapStart;
                if (mapEnd != null)
                {
                    maploc.End = mapEnd;
                }
                else
                {
                    maploc.End = new POSDATA();
                    maploc.End.SetUninitialized();
                }
            }
            else
            {
                //ASSERT (mapEnd == null);
                maploc.Start = new POSDATA();
                maploc.End = new POSDATA();
                maploc.Start.SetUninitialized();
                maploc.End.SetUninitialized();
            }

            locationList.Add(loc);
            mappedLocationList.Add(maploc);
            return true;
        }

        //------------------------------------------------------------
        // CError.UpdateLocations
        //
        /// <summary>
        /// Move locations in locationList and mappedLocationList by reference to posOld and posNew.
        /// </summary>
        /// <param name="posOld"></param>
        /// <param name="posNew"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool UpdateLocations(POSDATA posOld, POSDATA posNew)
        {
            bool fChanged = false;
            LOCATION loc, maploc;

            for (int i = 0; i < locationList.Count; i++)
            {
                loc = locationList[i];
                maploc = mappedLocationList[i];

                // Assert mapped location is sthg we can handle (even in the presence of line maps)
                // VSASSERT (p.posStart.iChar == pMap.posStart.iChar,
                //  "A line map should not imply a character position change");
                // VSASSERT (p.posEnd.iChar   == pMap.posEnd.iChar,
                //  "A line map should not imply a character position change");
                // VSASSERT ((p.posStart.iLine - pMap.posStart.iLine) == (p.posEnd.iLine - pMap.posEnd.iLine),
                //  "The line delta between a mapped position and the original position should not "
                //  "be different for the start and end position, even in the presence of a linemap "
                //  "(that would mean an error can span over a #line directive)");

                // Update the location only if it falls after posOld
                if (loc == null ||
                    loc.Start.IsUninitialized ||
                    loc.End.IsUninitialized ||
                    loc.End < posOld)
                    continue;

                // If the error spans the old, this function is being misused.
                if (loc.Start < posOld)
                {
                    //VSFAIL ("Misuse of CError::UpdateLocations!
                    //  Can't update an error spanning a change...");
                    continue;
                }

                // Adjust both positions, including the mapped ones
                {
                    // Cheat when updating the mapped line
                    // (we need to revert back to original line #)
                    // Note: This code is valid
                    // only if you don't change the 3 asserts above in this method

                    int iLineDelta = (loc.Start.LineIndex - maploc.Start.LineIndex);
                    // Lie
                    maploc.Start.LineIndex = loc.Start.LineIndex;
                    // Adjust
                    maploc.Start.Adjust(posOld, posNew);
                    // Reapply delta to adjusted position
                    maploc.Start.LineIndex += iLineDelta;
                }

                {
                    // Cheat when updating the mapped line
                    // (we need to revert back to original line #)
                    // Note: This code is valid
                    // only if you don't change the 3 asserts above in this method
                    int iLineDelta = (loc.End.LineIndex - maploc.End.LineIndex);
                    // Lie
                    maploc.End.LineIndex = loc.End.LineIndex;
                    // Adjust
                    maploc.End.Adjust(posOld, posNew);
                    // Reapply delta to adjusted position
                    maploc.End.LineIndex += iLineDelta;
                }

                if (loc.Start.Adjust(posOld, posNew))
                    fChanged = true;

                if (loc.End.Adjust(posOld, posNew))
                    fChanged = true;
            }
            return fChanged;
        }

        //------------------------------------------------------------
        // CError.WarnAsError
        //
        /// <summary>
        /// <para>Handle a warning as an error.</para>
        /// <para>Set ERRORKIND.ERROR to errorKind.</para>
        /// </summary>
        /// <returns>Return false if failed to make WarnAsError message.</returns>
        //------------------------------------------------------------
        internal bool WarnAsError()
        {
            DebugUtil.Assert(errorKind == ERRORKIND.WARNING);

            errorKind = ERRORKIND.ERROR;
            Exception excp = null;
            if (ErrorUtil.FormatErrorMessage(
                    out errorText,
                    out excp,
                    ResNo.CSCSTR_WarnAsError,
                    1,
                    errorText))
            {
                return true;
            }

            if (excp != null)
            {
                this.errorText = excp.Message;
            }
            else
            {
                this.errorText = ExceptionUtil.DefaultMessage;
            }
            return false;
        }

        //------------------------------------------------------------
        // CError.GetErrorInfo
        //
        /// <summary>
        /// Return this.errorID, this.errorKind, this.errorText.
        /// </summary>
        /// <param name="id">(out) Error ID which this holds.</param>
        /// <param name="kind">(out) Error level which this holds.</param>
        /// <param name="text">(out) Error text which this holds.</param>
        //------------------------------------------------------------
        virtual internal void GetErrorInfo(out CSCERRID id, out ERRORKIND kind, out string text)
        {
            id = errorID;
            kind = errorKind;
            text = errorText;
        }

        //------------------------------------------------------------
        // CError.GetLocationAt
        //
        /// <summary>
        /// Return the information of the specified location in mappedLocationList.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="file"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool GetLocationAt(
            int idx,
            out string file,
            out POSDATA start,
            out POSDATA end)
        {
            if (this.mappedLocationList != null &&
                idx >= 0 &&
                idx < this.mappedLocationList.Count)
            {
                LOCATION loc = this.mappedLocationList[idx];
                if (loc != null)
                {
                    file = loc.FileName;
                    start = loc.Start;
                    end = loc.End;
                    return true;
                }
            }

            file = null;
            start = null;
            end = null;
            return false;
        }

        //------------------------------------------------------------
        // CError.GetUnmappedLocationAt (ICSError)
        //
        /// <summary>
        /// <para>returns the actual input location</para>
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="file"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal bool GetUnmappedLocationAt(
            int idx,
            out string file,
            out POSDATA start,
            out POSDATA end)
        {
            if (this.locationList != null)
            {
                try
                {
                    LOCATION loc = this.locationList[idx];
                    if (loc != null)
                    {
                        file = loc.FileName;
                        start = loc.Start;
                        end = loc.End;
                        return true;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            file = null;
            start = null;
            end = null;
            return false;
        }

        //------------------------------------------------------------
        // CError.GetErrorMessages
        //
        /// <summary></summary>
        /// <param name="remake"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal List<string> GetErrorMessages(bool remake)
        {
            if (this.errorID < 0 && String.IsNullOrEmpty(this.errorText))
            {
                this.errorMessages.Clear();
                return this.errorMessages;
            }

            if (this.errorMessages != null && this.errorMessages.Count > 0 && !remake)
            {
                return this.errorMessages;
            }

            int count = this.locationList.Count;
            LOCATION tempLoc;
            string fileName;
            POSDATA posStart = null;
            POSDATA posEnd = null;
            bool noFileName = false;
            string msg = null;

            //--------------------------------------------------------
            // (1-1) Has the location data
            //--------------------------------------------------------
            if (count > 0)
            {
                this.errorMessages.Clear();

                for (int i = 0; i < count; ++i)
                {
                    fileName = null;
                    if (i < this.mappedLocationList.Count)
                    {
                        tempLoc = this.mappedLocationList[i];
                        fileName = tempLoc.FileName;
                        posStart = tempLoc.Start;
                        posEnd = tempLoc.End;
                    }

                    if (fileName == null)
                    {
                        tempLoc = this.locationList[i];
                        fileName = tempLoc.FileName;
                        posStart = tempLoc.Start;
                        posEnd = tempLoc.End;
                    }

                    // If the primary message has no file name,
                    if (i == 0 && String.IsNullOrEmpty(fileName))
                    {
                        noFileName = true;
                        break;
                    }

                    if (!String.IsNullOrEmpty(fileName))
                    {
                        int startLine = -1;
                        int endLine = -1;
                        int startColumn = -1;
                        int endColumn = -1;

                        if (posStart != null && !posStart.IsUninitialized)
                        {
                            startLine = posStart.LineIndex + 1;
                            startColumn = posStart.CharIndex + 1;
                        }
                        if (posEnd != null && !posEnd.IsUninitialized)
                        {
                            endLine = posEnd.LineIndex + 1;
                            endColumn = posEnd.CharIndex + 1;
                        }

                        if (i == 0)
                        {
                            msg = FormatStringUtil.FormatErrorMessageCore(
                                fileName,
                                startLine,
                                startColumn,
                                endLine,
                                endColumn,
                                errorKind,
                                "CS",
                                CSCErrorInfo.Manager.GetErrorNumber(this.errorID),
                                this.errorText);
                        }
                        else
                        {
                            msg = FormatRelatedLocationMessage(
                                Kind,
                                fileName,
                                startLine,
                                startColumn,
                                endLine,
                                endColumn);
                        }

                        this.errorMessages.Add(msg);
                    }

                } // for (int i = 0; i < count; ++i)
            }
            //--------------------------------------------------------
            // (1-2) If no location data, goto (2)
            //--------------------------------------------------------
            else
            {
                noFileName = true;
            }

            //--------------------------------------------------------
            // (2) No location data
            //--------------------------------------------------------
            if (noFileName)
            {
                this.errorMessages.Clear();

                msg = FormatStringUtil.FormatErrorMessageCore(
                    null,
                    -1,
                    0,
                    -1,
                    0,
                    errorKind,
                    "CS",
                    CSCErrorInfo.Manager.GetErrorNumber(this.errorID),
                    this.errorText);

                if (!String.IsNullOrEmpty(msg))
                {
                    this.errorMessages.Add(msg);
                }
            }

            return this.errorMessages;
        }

        //------------------------------------------------------------
        // CError.FormatRelatedLocationMessage
        //
        /// <summary></summary>
        /// <param name="kind"></param>
        /// <param name="fileName"></param>
        /// <param name="startLine"></param>
        /// <param name="startColumn"></param>
        /// <param name="endLine"></param>
        /// <param name="endColumn"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string FormatRelatedLocationMessage(
            ERRORKIND kind,
            string fileName,
            int startLine,
            int startColumn,
            int endLine,
            int endColumn)
        {
            string msg = (kind == ERRORKIND.WARNING
                ? "(Location of symbol related to previous warning)"
                : "(Location of symbol related to previous error)");

            return FormatStringUtil.FormatErrorMessageCore(
                fileName,
                startLine,
                startColumn,
                endLine,
                endColumn,
                errorKind,
                CSCErrorInfo.Manager.Prefix,
                CSCErrorInfo.Manager.GetErrorNumber(errorID),
                msg);
        }
    }

    //======================================================================
    // CErrorContainer
    //
    /// <summary>
    /// <para>This is a standard implementation of ICSErrorContainer.</para>
    /// <para>It can either accumulate errors in order of addition, or maintain order by line number.</para>
    /// <para>It also has a range replacement mode, which allows new incoming errors
    /// to replace any existing ones (used by incremental tokenization).</para>
    /// </summary>
    //======================================================================
    internal class CErrorContainer
    {
        //------------------------------------------------------------
        // CErrorContainer  Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// Don't ever hold more than this many errors.
        /// </summary>
        private const int MAX_ERROR_COUNT = 200;

        private ERRORCATEGORY errorCategory;    // m_iCategory

        private uint m_dwID;    // DWORD_PTR m_dwID;

        /// <summary>
        /// CError List.
        /// </summary>
        private List<CError> errorList = new List<CError>();    // CError **m_ppErrors;

        /// <summary>
        /// (R) CError List.
        /// </summary>
        internal List<CError> ErrorList
        {
            get { return this.errorList; }
        }

        /// <summary>
        /// (R) The number of CError instances in errorList.
        /// </summary>
        /// <remarks>(sscli)
        /// long m_iCount;
        /// long Count ()
        /// </remarks>
        internal int Count
        {
            get { return errorList.Count; }
        }

        private int errorCount = 0;         // long m_iErrors;
        private int warningCount = 0;       // long m_iWarnings;
        private int warnAsErrorCount = 0;   // long m_iWarnAsErrors;

        internal int WarnAsErrorCount
        {
            get { return warnAsErrorCount; }    // GetWarnAsErrorCount
        }

        private CErrorContainer replacementContainer = null;    // *m_pReplacements;

#if DEBUG
        internal readonly int ErrorContainerID = CObjectID.GenerateID();
#endif

        //------------------------------------------------------------
        // CErrorContainer  Constructor
        //
        /// <summary>
        /// Constructor. Does nothing.
        /// </summary>
        //------------------------------------------------------------
        internal CErrorContainer() { }

        //------------------------------------------------------------
        // CErrorContainer.ClearSomeErrors
        //
        /// <summary>
        /// Remove CError instances of the specified range from errorList.
        /// </summary>
        /// <param name="ierrMin">The first index of the range to remove.</param>
        /// <param name="ierrLim">The end of the range to remove. (not in the range.)</param>
        //------------------------------------------------------------
        private void ClearSomeErrors(int ierrMin, int ierrLim)
        {
            if (ierrMin < 0 || ierrMin > ierrLim)
            {
                return;
            }
            if (ierrLim > errorList.Count)
            {
                ierrLim = errorList.Count;
            }

            CSCERRID id;
            ERRORKIND kind;
            string text;

            // Release the errors that are being replaced,
            // keeping track of warning/error/fatal counts.
            for (int ierr = ierrMin; ierr < ierrLim; ++ierr)
            {
                errorList[ierr].GetErrorInfo(out id, out kind, out text);
                if (kind == ERRORKIND.ERROR)
                {
                    if (errorCount > 0) --errorCount;
                    if (errorList[ierr].WasWarning) --warnAsErrorCount;
                }
                else if (kind == ERRORKIND.WARNING)
                {
                    if (warningCount > 0) --warningCount;
                }
                errorList[ierr] = null;
            }
            // Remove them.
            errorList = CharUtil.RemoveElementsFromList<CError>(errorList, null);
        }

        //------------------------------------------------------------
        // CErrorContainer.CreateInstance
        //
        /// <summary>Create a CErrorContainer instance and initialize it.</summary>
        /// <param name="category"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static CErrorContainer CreateInstance(
            ERRORCATEGORY category,
            uint id)
        {
            CErrorContainer ec = null;

            ec = new CErrorContainer();
            ec.Initialize(category, id);
            return ec;
        }

        //------------------------------------------------------------
        // CErrorContainer.Initialize
        //
        /// <summary>Set the category and id.</summary>
        /// <param name="category"></param>
        /// <param name="id"></param>
        //------------------------------------------------------------
        internal void Initialize(ERRORCATEGORY category, uint id)
        {
            errorCategory = category;
            m_dwID = id;
        }

        //------------------------------------------------------------
        // CErrorContainer.Clone
        //
        /// <summary>
        /// <para>Duplicate this.</para>
        /// <para>Do not call this method in replacement mode, or return null.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal CErrorContainer Clone()
        {
            // Can't clone a container while it is in replacement mode!
            if (replacementContainer != null)
            {
                DebugUtil.Assert(false, "Can't clone an error container while it is in replace mode!");
                return null;
            }
            CErrorContainer cloneContainer = new CErrorContainer();
            cloneContainer.errorCategory = this.errorCategory;
            cloneContainer.m_dwID = this.m_dwID;
            for (int i = 0; i < this.errorList.Count; ++i)
            {
                cloneContainer.errorList.Add(this.errorList[i]);
            }
            cloneContainer.errorCount = this.errorCount;
            cloneContainer.warningCount = this.warningCount;
            cloneContainer.warnAsErrorCount = this.warnAsErrorCount;
            //cloneContainer.replacementContainer = this.replacementContainer;
            return cloneContainer;
        }

        //------------------------------------------------------------
        // CErrorContainer.AddError
        //
        /// <summary>Add a CError instance to errorList.</summary>
        /// <param name="newError"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool AddError(CError newError)
        {
            // If we're in replacement mode, this goes in our replacement container
            if (replacementContainer != null)
            {
                return replacementContainer.AddError(newError);
            }

            string file1, file2;
            POSDATA start1, end1, start2, end2;
            int idx = errorList.Count;

            if (newError.GetLocationAt(0, out file1, out start1, out end1))
            {
                // Search backwards to see if we have to slide any errors down.
                // This doesn't happen very often, but just in case...
                for (int i = errorList.Count - 1; i >= 0; --i)
                {
                    if (errorList[i] == null)
                    {
                        continue;
                    }
                    if (!errorList[i].GetLocationAt(0, out file2, out start2, out end2))
                    {
                        continue;
                    }

                    if (start2 <= start1)
                    {
                        idx = i + 1;
                        break;
                    }
                }
            }

            try
            {
                if (idx < errorList.Count)
                {
                    errorList.Insert(idx, newError);
                }
                else
                {
                    errorList.Add(newError);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            CSCERRID id;
            ERRORKIND kind;
            string text;

            // Keep track of errors/warnings/fatals (fatals are calculated)
            newError.GetErrorInfo(out id, out kind, out text);
            if (kind == ERRORKIND.WARNING)
            {
                ++warningCount;
            }
            else if (kind == ERRORKIND.ERROR)
            {
                if (newError.WasWarning)
                {
                    ++warnAsErrorCount;
                }
                ++errorCount;
            }
            return true;
        }

        //------------------------------------------------------------
        // CErrorContainer.BeginReplacement
        //
        /// <summary>
        /// This function turns "replace mode" on for this container.
        /// All incoming errors get added to the replacement container instead of this one,
        /// and get retrofitted into this when EndReplacement is called
        /// (replacing errors that fall within the range supplied).
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool BeginReplacement()
        {
            if (replacementContainer != null)
            {
                return false;
            }
            replacementContainer = CreateInstance(errorCategory, m_dwID);
            return (replacementContainer != null);
        }

        //------------------------------------------------------------
        // CErrorContainer.EndReplacement
        //
        /// <summary>
        /// <para>This function is called to terminate "replace mode"
        /// and place all errors (if any) in the replacement container into this container,
        /// first removing all existing errors that fall within the given range.</para>
        /// <para>Returns S_FALSE if nothing changed. (in sscli)</para>
        /// </summary>
        /// <param name="posStart"></param>
        /// <param name="posOldEnd"></param>
        /// <param name="posNewEnd"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool EndReplacement(POSDATA posStart, POSDATA posOldEnd, POSDATA posNewEnd)
        {
            if (replacementContainer == null)
            {
                DebugUtil.Assert(false, "Can't terminate a replacement that hasn't started!");
                return false;
            }

            // ASSERT(errorList.Count <= cerrMax);

            int firstDeleteIndex = errorList.Count;    // The first current error to delete.
            int lastDeleteIndex = errorList.Count;     // One past the last current error to delete.
            string file;
            POSDATA start, end;
            bool wasListChanged = false;

            // Okay, find the span of errors that fall within the given range.
            // It MUST be a contiguous range (because errors are sorted by position when added).
            // Note that this is a linear search -- tolerable because the number of errors
            // coming from EC_TOKENIZATION (the only category that uses this so far)
            // is expected to be small, and because replacement doesn't happen often.

            // Find firstDeleteIndex and lastDeleteIndex of the range to remove.
            if (posOldEnd.IsZero)
            {
                // In this case, we replace all existing errors
                firstDeleteIndex = 0;
            }
            else
            {
                for (int ierr = 0; ierr < errorList.Count; ierr++)
                {
                    if (errorList[ierr] == null)
                    {
                        continue;
                    }
                    if (!errorList[ierr].GetUnmappedLocationAt(0, out file, out start, out end))
                    {
                        continue;
                    }

                    if ((firstDeleteIndex == errorList.Count) && (start >= posStart))
                    {
                        firstDeleteIndex = ierr;
                    }

                    if (start > posOldEnd)
                    {
                        // ASSERT(firstDeleteIndex < errorList.Count);
                        // If this fires, then posStart > posOldEnd -- how'd that happen?!?
                        lastDeleteIndex = ierr;
                        break;
                    }
                }
            }

            if (firstDeleteIndex < 0 ||
                firstDeleteIndex > lastDeleteIndex ||
                lastDeleteIndex > errorList.Count)
                return false;   // HRESULT.S_FALSE;

            // If we're adding anything or deleting anything we're changing.
            wasListChanged =
                (replacementContainer.errorList.Count | (lastDeleteIndex - firstDeleteIndex)) != 0;

            if (firstDeleteIndex == 0 && lastDeleteIndex == errorList.Count)
            {
                // Just swap the contents of the two containers.

                Util.Swap<List<CError>>(ref errorList, ref replacementContainer.errorList);
                Util.Swap<int>(ref errorCount, ref replacementContainer.errorCount);
                Util.Swap<int>(ref warningCount, ref replacementContainer.warningCount);
                Util.Swap<int>(ref warnAsErrorCount, ref replacementContainer.warnAsErrorCount);
            }
            else
            {
                // Release the errors that are being replaced,
                // keeping track of warning/error/fatal counts.

                for (int i = lastDeleteIndex; i < errorList.Count; ++i)
                {
                    errorList[i].UpdateLocations(posOldEnd, posNewEnd);
                    wasListChanged = true;
                }

                // Remove CError instances.
                ClearSomeErrors(firstDeleteIndex, lastDeleteIndex);

                // Copy the errors from the replacement container into the array, keeping counts of
                // warnings/errors/fatals.
                // Insert CError instances of replacementContainer and count the number of each category at the time.
                for (int ierr = 0; ierr < replacementContainer.errorList.Count; ierr++)
                {
                    CSCERRID id;
                    ERRORKIND kind;
                    string text;
                    CError errRep = replacementContainer.errorList[ierr];
                    if (errRep == null) continue;
                    try
                    {
                        errorList.Insert(firstDeleteIndex + ierr, errRep);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        errorList.Add(errRep);
                    }
                    errRep.GetErrorInfo(out id, out kind, out text);
                    if (kind == ERRORKIND.ERROR)
                    {
                        errorCount++;
                        if (errRep.WasWarning) warnAsErrorCount++;
                    }
                    else if (kind == ERRORKIND.WARNING)
                    {
                        warningCount++;
                    }
                }

#if DEBUG
                //for (long ierr = 0; ierr < cerrNew; ierr++)
                //{
                //  ASSERT(errorList[ierr]);
                //}
#endif // !DEBUG
            }

            // Get rid of the replacement container
            replacementContainer.errorList.Clear();

            // Fin!
            return wasListChanged;
        }

        //------------------------------------------------------------
        // CErrorContainer.ReleaseAllErrors
        //
        /// <summary>Clear all CError instances.</summary>
        //------------------------------------------------------------
        internal void ReleaseAllErrors()
        {
            this.errorList.Clear();
            this.errorCount = 0;
            this.warningCount = 0;
            this.warnAsErrorCount = 0;
        }

        //------------------------------------------------------------
        // CErrorContainer.GetContainerInfo
        //
        /// <summary>Get the category and id of this.</summary>
        /// <param name="category"></param>
        /// <param name="id"></param>
        //------------------------------------------------------------
        virtual internal void GetContainerInfo(out ERRORCATEGORY category, out uint id)
        {
            category = this.errorCategory;
            id = this.m_dwID;
        }

        //------------------------------------------------------------
        // CErrorContainer.GetErrorCount
        //
        /// <param name="warnings"></param>
        /// <summary>Get the number of each category.</summary>
        /// <param name="errors"></param>
        /// <param name="fatals"></param>
        /// <param name="total"></param>
        //------------------------------------------------------------
        virtual internal void GetErrorCount(
            out int warnings,
            out int errors,
            out int fatals,
            out int total)
        {
            warnings = this.warningCount;
            errors = this.errorCount;
            fatals = this.errorList.Count - (this.errorCount + this.warningCount);
            total = this.errorList.Count;
        }

        //------------------------------------------------------------
        // CErrorContainer.GetErrorAt
        //
        /// <summary>
        /// Return the specified CError instance.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        virtual internal CError GetErrorAt(int idx)
        {
            try
            {
                return this.errorList[idx];
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            return null;
        }
    }

    //======================================================================
    // Global Utility functions.
    //======================================================================
    // IsValidWarningNumber is move to CSCErrorInfo (CSharp\SCC\CSErrorInfo.cs).
#if false
    static internal partial class Util
    {
        //------------------------------------------------------------
        // Util.IsValidWarningNumber
        //
        /// <summary>
        /// determine if a number is a valid warning number.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal bool IsValidWarningNumber(CSCERRID id)
        {
            int level;

            if (CSCErrorInfo.GetWarningLevel(id, out level))
            {
                return (level > 0);
            }
            return false;
        }
    }
#endif

    //extern bool IsValidWarningNumber(int id);
    //extern short ErrorNumberFromID(long iErrorIndex);
    //extern bool __cdecl LoadAndFormatMessage(HINSTANCE hModuleMessages, int resid, __out_ecount(cchMax) PWSTR buffer, int cchMax, ...);
    //extern bool __cdecl LoadAndFormatMessage(int resid, __out_ecount(cchMax) PWSTR buffer, int cchMax, ...);
    //#ifdef DEBUG
    //extern bool IsWarningID(long iErrorIndex);
    //extern void CheckErrorMessageInfo(HINSTANCE hModuleMessages, MEMHEAP * heap, bool dumpAll);
    //#endif //DEBUG

    //inline bool HasBadChars(PCWSTR psz) {                     // Util.cs へ
    //inline void ClobberBadChars(PWCH pchMin, PWCH pchLim) {   // PCSUtility へ
    //#define ConvertAndCleanArgToStack(parg, ppsz) \           // ErrArg のメソッドとする。

    ///*
    // * Information about each error or warning.
    // */
    //struct ERROR_INFO {
    //    short number;       // error number
    //    short level;        // warning level; 0 means error
    //    int   resid;        // resource id.
    //};
    //
    //#define ERRORDEF(num, name, strid)       {num,     0, strid},
    //#define WARNDEF(num, level, name, strid) {num, level, strid},
    //#define OLDWARN(num, name)               {num,    99,    -1},
    //#define FATALDEF(num, name, strid)       {num,    -1, strid},
    //
    //static const ERROR_INFO errorInfo[ERR_COUNT] = {
    //    {0000, -1, 0},          // ERR_NONE - no error.
    //    #include "errors.h"
    //};
    //
    //#undef ERRORDEF
    //#undef WARNDEF
    //#undef OLDWARN
    //#undef FATALDEF

    //short ErrorNumberFromID(long iErrorIndex) { return errorInfo[iErrorIndex].number; }

    //#define LoadStringA(hInstance,uID,lpBuffer,cchBufferMax) \
    //    PAL_LoadSatelliteStringA((HSATELLITE)hInstance, uID, lpBuffer, cchBufferMax)
    //#define LoadStringW(hInstance,uID,lpBuffer,cchBufferMax) \
    //    PAL_LoadSatelliteStringW((HSATELLITE)hInstance, uID, lpBuffer, cchBufferMax)
    //
    //#ifdef UNICODE
    //#define LoadString LoadStringW
    //#else
    //#define LoadString LoadStringA
    //#endif // UNICODE

#if DEBUG
    ////
    //// Wrapper for LoadString that works on Win9x.
    ////
    //int LoadStringWide(HINSTANCE hInstance, UINT uID, WCBuffer lpBuffer);

    //bool IsWarningID(long iErrorIndex) { return errorInfo[iErrorIndex].level > 0; }

    //// DEBUG-only function to check the integrity of the error info -- 
    //// check that all error messages exist, and that no error numbers or
    //// messages are duplicated. This detects common mistakes when editing
    //// errors.h
    //void CheckErrorMessageInfo(HINSTANCE hModuleMessages, MEMHEAP * heap, bool dumpAll)
    //{
    //    int * messageIds = (int *) heap.AllocZero(sizeof(int *) * 0x10000);
    //    int * errorNos = (int *) heap.AllocZero(sizeof(int *) * 0x10000);
    //    wchar_t dummy[4096];
    //    WCBuffer dummyBuffer(dummy);
    //
    //    for (int iErr = 1; iErr < ERR_COUNT; ++iErr) {
    //        if (errorInfo[iErr].level == 99) {
    //            ASSERT(errorInfo[iErr].resid == -1);
    //            // These warnings are no longer used, but kept to prevent
    //            // spurious errors on the "/nowarn" option
    //            continue;
    //        }
    //
    //        //  a few messages are duplicated intentionally.
    //        if (iErr != ERR_DeprecatedSymbolStr &&
    //            iErr != WRN_InvalidNumber &&
    //            iErr != WRN_FileNameTooLong && 
    //            iErr != WRN_EndOfPPLineExpected &&
    //            errorInfo[iErr].resid != IDS_FeatureDeprecated) 
    //        {
    //            ASSERT(messageIds[errorInfo[iErr].resid] == 0); // duplicate message ID
    //            messageIds[errorInfo[iErr].resid] = iErr;
    //        }
    //
    //        ASSERT(errorInfo[iErr].number > errorInfo[iErr - 1].number); // They need to stay sorted!
    //
    //        if (iErr != FTL_NoMessagesDLL) // intentionally no mesage for this one!
    //            ASSERT(LoadStringWide(hModuleMessages, errorInfo[iErr].resid, dummyBuffer) != 0); // missing message
    //
    //        ASSERT(errorNos[errorInfo[iErr].number] == 0); // duplicate error number
    //        errorNos[errorInfo[iErr].number] = iErr;
    //
    //        if (dumpAll) {
    //            dummy[0] = L'\0';
    //            LoadStringWide(hModuleMessages, errorInfo[iErr].resid, dummyBuffer);
    //            wprintf(L"%d\t%d\t%s\n", errorInfo[iErr].number, errorInfo[iErr].level, dummy);
    //        }
    //    }
    //
    //    ASSERT(errorInfo[ERR_ErrorDirective].number == 1029);
    //    ASSERT(errorInfo[ERR_NonECMAFeature].number == 1644);
    //    ASSERT(errorInfo[ERR_EndRegionDirectiveExpected].number == 1038);
    //    ASSERT(errorInfo[ERR_ExpectedEndTry].number == 1524);
    //    heap.Free(messageIds);
    //    heap.Free(errorNos);
    //}

#endif //DEBUG

    //////////////////////////////////////////////////////////////////////////////////
    //// CAutoFree -- helper class to allocate memory from the heap if possible, and
    //// free on destruction.  Intended to use in possible low-stack conditions; if
    //// allocation fails, attempt allocation on the stack, as in the following:
    ////
    //// CAutoFree<WCHAR>    f;
    //// PWSTR               p;
    ////
    //// p = SAFEALLOC (f, 128);
    ////
    //
    //template <class T>
    //class CAutoFree;

    //#define SAFEALLOC(mem,size) (mem.AFAlloc(size) ? mem.Mem() : mem.Cast(_alloca(size*mem.ElementSize())))

    ///*
    // * Helper function and load and format a message. Uses Unicode
    // * APIs, so it won't work on Win95/98.
    // */
    //static bool LoadAndFormatW(
    //  HINSTANCE hModuleMessages,
    //  int resid, va_list args, VarArgList args2, __out_ecount(cchMax) PWSTR buffer, int cchMax);

    ///*
    // * Helper function to load and format a message using ANSI functions.
    // * Used as a backup when Unicode ones are not available.
    // */
    //static bool LoadAndFormatA(
    //  HINSTANCE hModuleMessages, int resid,
    //  va_list args, VarArgList args2, __out_ecount(cchMax) PWSTR buffer, int cchMax);

    //bool __cdecl LoadAndFormatMessage(int resid, __out_ecount(cchMax) PWSTR buffer, int cchMax, ...);

    //bool __cdecl LoadAndFormatMessage(
    //  HINSTANCE hModuleMessages, int resid, __out_ecount(cchMax) PWSTR buffer, int cchMax, ...);

    //
    ///*
    // * Code has caught an exception. Handle it. If the exception code is
    // * FATAL_EXCEPTION_CODE, this is the result of a previously reported
    // * fatal error and we do nothing. Otherwise, we report an internal
    // * compiler error.
    // */
    //void COMPILER::HandleException(DWORD exceptionCode)
    //{
    //    Error (NULL, FTL_InternalError, exceptionCode);
    //}

    ///*
    // * Create a fill-in string describing the last Win32 error.
    // */
    //PCWSTR COMPILER::ErrGetLastError()
    //{
    //    return ErrHR(HRESULT_FROM_WIN32(GetLastError()), false);
    //}
    //



    //PCWSTR COMPILER::ErrParamList(TypeArray *params, bool isVarargs, bool isParamArray)
    //{
    //    START_ERR_STRING(this);
    //    ErrAppendParamList(params, isVarargs, isParamArray);
    //    return END_ERR_STRING(this);
    //}

    ///*
    // * This controls how we handle all fatal errors, asserts, and exceptions
    // */
    //LONG CompilerExceptionFilter(EXCEPTION_POINTERS* exceptionInfo, LPVOID pvData)
    //{
    //    COMPILER *compiler = (COMPILER *)pvData;
    //    DWORD exceptionCode = exceptionInfo.ExceptionRecord.ExceptionCode;
    //
    //    // Don't stop here for fatal errors
    //    if (exceptionCode == FATAL_EXCEPTION_CODE)
    //        return EXCEPTION_CONTINUE_SEARCH;
    //
    //    // If it's an AV in our error buffer range, it might be because we need to grow our error buffer.
    //    // If so, then just commit another page an allow execution to continue
    //    if (exceptionCode == EXCEPTION_ACCESS_VIOLATION && compiler && compiler.errBuffer &&
    //        (ULONG_PTR)compiler.errBuffer < exceptionInfo.ExceptionRecord.ExceptionInformation[1] && 
    //        (ULONG_PTR)(compiler.errBuffer + ERROR_BUFFER_MAX_WCHARS) >= exceptionInfo.ExceptionRecord.ExceptionInformation[1])
    //    {
    //        void * temp = NULL;
    //        if (((compiler.errBufferNextPage - (BYTE*)compiler.errBuffer) < (int) ERROR_BUFFER_MAX_BYTES-1) &&
    //            (NULL != (temp = VirtualAlloc( compiler.errBufferNextPage, compiler.pageheap.pageSize, MEM_COMMIT, PAGE_READWRITE)))) {
    //            compiler.errBufferNextPage += compiler.pageheap.pageSize;
    //            return EXCEPTION_CONTINUE_EXECUTION;
    //        } else {
    //            // We either ran out of reserved memory, or couldn't commit what we've already reserved!?!?!?
    //            // Normally we shouldn't throw another exception inside the exception filter
    //            // but this really is a fatal condition
    //            compiler.Error(NULL, FTL_NoMemory);
    //        }
    //    }
    //
    //    if (compiler && compiler.pController)
    //        compiler.pController.SetExceptionData (exceptionInfo);
    //
    //#ifdef _DEBUG
    //
    //    if (COMPILER::GetRegDWORD("GPF"))
    //        return EXCEPTION_CONTINUE_SEARCH;
    //
    //#endif
    //
    //    WatsonOperationKindEnum howToReportWatsons = WatsonOperationKind::Queue;
    //    WCHAR bugreport[MAX_PATH];
    //    bugreport[0] = L'\0';
    //    if (compiler) {
    //        howToReportWatsons = compiler.options.m_howToReportWatsons;
    //        if (compiler.cmdHost) {
    //            if (FAILED(compiler.cmdHost.GetBugReportFileName(bugreport, lengthof(bugreport))))
    //                bugreport[0] = '\0';
    //        }
    //    }
    //
    //    LONG result = EXCEPTION_EXECUTE_HANDLER;
    //
    //    if (compiler && result == EXCEPTION_EXECUTE_HANDLER) {
    //        PAL_TRY
    //        {
    //            compiler.ReportICE(exceptionInfo);
    //        }
    //        PAL_EXCEPT(EXCEPTION_EXECUTE_HANDLER)
    //        {
    //        }
    //        PAL_ENDTRY
    //    }
    //    return result;
    //}

    ///*
    // *  Set a breakpoint here to control 2nd chance exceptions
    // */
    //LONG GenericExceptionFilter(EXCEPTION_POINTERS * exceptionInfo, LPVOID pv)
    //{
    //    // If you want to do something different during debugging, slam the appropriate
    //    // value into eax.
    //    // EXCEPTION_CONTINUE_EXECUTION is -1
    //    // EXCEPTION_CONTINUE_SEARCH is 0
    //    // EXCEPTION_EXECUTE_HANDLER is 1
    //    return EXCEPTION_EXECUTE_HANDLER;
    //}

    //int __cdecl compareErrorId(const void * id, const void * err) { return *(const int *)id - ((const ERROR_INFO*)err).number; }
    //

    //////////////////////////////////////////////////////////////////////////////////
    //
    //ErrArgSymKind::ErrArgSymKind(SYM * sym)
    //{
    //    eak = ErrArgKindEnum.SymKind;
    //    ErrorArgumentFlags = ErrArgFlagsEnum.None;
    //    sk = sym.getKind();
    //    if (sk == SK_NSAIDSYM) {
    //        if (sym.AsNSAIDSYM.GetNS().name.text[0]) {
    //            // Non-empty namespace name means it's not the root
    //            // so treat it like a namespace instead of an alias
    //            sk = SK_NSSYM;
    //        }
    //        else {
    //            // An empty namespace name means it's just an alias for the root
    //            sk = SK_EXTERNALIASSYM;
    //        }
    //    }
    //}
    //
    //ErrArgAggKind::ErrArgAggKind(AGGSYM * agg)
    //{
    //    eak = ErrArgKindEnum.AggKind;
    //    ErrorArgumentFlags = ErrArgFlagsEnum.None;
    //    ak = agg.AggKind();
    //}
    //
    //ErrArgAggKind::ErrArgAggKind(AGGTYPESYM * ats)
    //{
    //    eak = ErrArgKindEnum.AggKind;
    //    ErrorArgumentFlags = ErrArgFlagsEnum.None;
    //    ak = ats.getAggregate().AggKind();
    //}
}
