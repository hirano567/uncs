// sscli20_20060311

// ==++==
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
// File: withtype.h
//
//  Defines structs that package an aggregate member together with
//  generic type argument information.
//
//  This file is included twice:
//  1) Before symbol.h with WITHTYPE_INLINES _not_ defined
//  2) After symbol.h with WITHTYPE_INLINES defined
// ===========================================================================

//============================================================================
//  WithType.cs
//
//  2015/04/27
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    // SymWithType and its cousins.
    //
    // These package an aggregate member (field, prop, event, or meth) together
    // with the particular instantiation of the aggregate (the AGGTYPESYM).
    //
    // The default constructor does nothing so these are not safe to use uninitialized.
    // Note that when they are used as member of an EXPR
    // they are automatically zero filled by newExpr.

    //======================================================================
    // class SymWithType
    //
    /// <summary>
    /// <para>Represents overridden members or implemented members.</para>
    /// <para>Has a field Sym of type SYM and a field AggTypeSym of type AGGTYPESYM.
    /// if AggTypeSym is not null, they should have the same AGGSYM parent.</para>
    /// </summary>
    //======================================================================
    internal class SymWithType
    {
        //------------------------------------------------------------
        // SymWithType Fields and Properties
        //------------------------------------------------------------

        /// <summary>
        /// <para>(Use in place of Sym() in sslic.)</para>
        /// </summary>
        internal SYM Sym = null;    // SYM * sym; SYM * Sym();

        /// <summary>
        /// <para>Use in place of Type().</para>
        /// </summary>
        internal AGGTYPESYM AggTypeSym = null;  // AGGTYPESYM * ats

        /// <summary>
        /// (R) True if Sym == null.
        /// </summary>
        internal bool IsNull
        {
            get { return Sym == null; }
        }

        /// <summary>
        /// (R) True if Sym != null.
        /// </summary>
        /// <remarks>(2014/12/27 hirano567@hotmail.co.jp)</remarks>
        internal bool IsNotNull
        {
            get { return Sym != null; }
        }

        // These assert that the SYM is of the correct type.

        /// <summary>
        /// (R) Return field Sym as METHPROPSYM.
        /// </summary>
        internal METHPROPSYM MethPropSym
        {
            get { return this.Sym as METHPROPSYM; } // MethProp()
        }

        /// <summary>
        /// (R) Return field Sym as METHSYM.
        /// </summary>
        internal METHSYM MethSym
        {
            get { return this.Sym as METHSYM; } // Meth()
        }

        /// <summary>
        /// (R) Return field Sym as PROPSYM.
        /// </summary>
        internal PROPSYM PropSym
        {
            get { return this.Sym as PROPSYM; } // Prop()
        }

        /// <summary>
        /// (R) Return field Sym as MEMBVARSYM.
        /// </summary>
        internal MEMBVARSYM FieldSym
        {
            get { return this.Sym as MEMBVARSYM; }  // Field()
        }

        /// <summary>
        /// (R) Return field Sym as EVENTSYM.
        /// </summary>
        internal EVENTSYM EventSym
        {
            get { return this.Sym as EVENTSYM; }    // Event()
        }

        //------------------------------------------------------------
        // SymWithType Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal SymWithType() { }

        //------------------------------------------------------------
        // SymWithType Constructor (2)
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal SymWithType(SYM sym, AGGTYPESYM ats)
        {
            Set(sym, ats);
        }

        //------------------------------------------------------------
        // SymWithType.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Clear()
        {
            Sym = null;
            AggTypeSym = null;
        }

        //------------------------------------------------------------
        // SymWithType.Set (1)
        //
        /// <summary>
        /// <para>If sym is null, ats is set null.</para>
        /// <para>If sym is not null, sym and ats should have the same parent.</para>
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal void Set(SYM sym, AGGTYPESYM ats)
        {
            if (sym == null)
            {
                ats = null;
            }
            DebugUtil.Assert(ats == null || sym.ParentSym == ats.GetAggregate());

            if (ats != null && sym.ParentSym != ats.GetAggregate())
            {
                return;
            }

            this.Sym = sym;
            this.AggTypeSym = ats;
        }

        //------------------------------------------------------------
        // SymWithType.Set (2)
        //
        /// <summary></summary>
        /// <param name="swt"></param>
        //------------------------------------------------------------
        internal void Set(SymWithType swt)
        {
            if (swt != null)
            {
                this.Set(swt.Sym, swt.AggTypeSym);
            }
            else
            {
                this.Set(null, null);
            }
        }

        //------------------------------------------------------------
        // SymWithType.Equals
        //
        /// <summary>Override.</summary>
        /// <param name="obj"></param>
        //------------------------------------------------------------
        public override bool Equals(object obj)
        {
            SymWithType swt = obj as SymWithType;
            if (swt != null)
            {
                return (this.Sym == swt.Sym && this.AggTypeSym == swt.AggTypeSym);
            }
            return false;
        }

        //------------------------------------------------------------
        // SymWithType.GetHashCode
        //
        /// <summary>Override.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override int GetHashCode()
        {
            return (Sym.GetHashCode() ^ AggTypeSym.GetHashCode()) & 0x7FFFFFFF;
        }

        //------------------------------------------------------------
        // SymWithType.operator bool
        //
        /// <summary>
        /// <para>The SymWithType is considered null iff the SYM is null.
        /// If field sym is not null, return true.</para>
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public static explicit operator bool(SymWithType swt)
        {
            return swt.Sym != null;
        }

        //------------------------------------------------------------
        // SymWithType.CreateCorrespondingInstance (static)
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="ats"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal SymWithType CreateCorrespondingInstance(SYM sym, AGGTYPESYM ats)
        {
            if (sym == null)
            {
                return new SymWithType();
            }

            SymWithType swt = null;
            switch (sym.Kind)
            {
                case SYMKIND.METHSYM:
                    return new MethWithType(sym as METHSYM, ats) as SymWithType;

                case SYMKIND.PROPSYM:
                    return new PropWithType(sym as PROPSYM, ats) as SymWithType;

                case SYMKIND.EVENTSYM:
                    return new EventWithType(sym as EVENTSYM, ats) as SymWithType;

                case SYMKIND.MEMBVARSYM:
                    return new FieldWithType(sym as MEMBVARSYM, ats) as SymWithType;

                default:
                    break;
            }
            return new SymWithType(sym, ats);
        }

#if DEBUG
        //------------------------------------------------------------
        // SymWithType.Debug
        //------------------------------------------------------------
        internal string Debug()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Sym != null)
            {
                sb.AppendFormat("No.{0} ({1})", Sym.SymID, Sym.Kind);
                if (this.AggTypeSym != null)
                {
                    sb.AppendFormat(" with No.{0}", AggTypeSym.SymID);
                }
            }
            return sb.ToString();
        }
#endif
    }

    ///******************************************************************************
    //    In debug these types assert that the symbol is of the correct type.
    //    In non-debug they are just SymWithType.
    //******************************************************************************/
    //#ifndef DEBUG
    //    typedef SymWithType MethPropWithType;
    //    typedef SymWithType MethWithType;
    //    typedef SymWithType PropWithType;
    //    typedef SymWithType EventWithType;
    //    typedef SymWithType FieldWithType;
    //#elif !defined(WITHTYPE_INLINES) // && DEBUG

    //======================================================================
    // class MethPropWithType
    //
    /// <summary>
    /// <para>Derives from SymWithType.
    /// Represents overridden members or implemented methods and properties.</para>
    /// <para>Has only two fields Sym and AggTypeSym which derives from SymWithType.</para>
    /// </summary>
    //======================================================================
    internal class MethPropWithType : SymWithType
    {
        //------------------------------------------------------------
        // MethPropWithType Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal MethPropWithType() { }

        //------------------------------------------------------------
        // MethPropWithType Constructor (2)
        //
        /// <summary></summary>
        /// <param name="mps"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal MethPropWithType(METHPROPSYM mps, AGGTYPESYM ats)
        {
            Set(mps, ats);
        }

        //------------------------------------------------------------
        // MethPropWithType Constructor (3)
        //
        /// <summary></summary>
        /// <param name="swt"></param>
        //------------------------------------------------------------
        internal MethPropWithType(SymWithType swt)
        {
            if (swt != null && swt.Sym != null)
            {
                Set(swt.Sym as METHPROPSYM, swt.AggTypeSym);
            }
            else
            {
                Set(null, null);
            }
        }

        //------------------------------------------------------------
        // MethPropWithType.Set (1)
        //
        /// <summary></summary
        /// <param name="mps"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal void Set(METHPROPSYM mps, AGGTYPESYM ats)
        {
            base.Set(mps, ats);
        }

        //------------------------------------------------------------
        // MethPropWithType.Set (2)
        //
        /// <summary></summary>
        /// <param name="mpwt"></param>
        //------------------------------------------------------------
        internal void Set(MethPropWithType mpwt)
        {
            if (mpwt != null)
            {
                this.Set(mpwt.MethPropSym, mpwt.AggTypeSym);
            }
            else
            {
                this.Set(null, null);
            }
        }

        //------------------------------------------------------------
        // MethPropWithType.Convert
        //
        /// <summary>
        /// If swt is MethPropWithType, convert to MethPropWithType and return it.
        /// Otherwise, create a MethPropWithType instance by swt and return it.
        /// </summary>
        /// <param name="swt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal MethPropWithType Convert(SymWithType swt)
        {
            MethPropWithType mpwt = swt as MethPropWithType;
            if (mpwt != null) return mpwt;
            return new MethPropWithType(swt);
        }
    }

    //======================================================================
    // class MethWithType
    //
    /// <summary>
    /// <para>Derives from MethPropWithType. (So derives from SymWithType.)
    /// Represents overridden members or implemented methods.</para>
    /// <para>Has only two fields Sym and AggTypeSym which derives from SymWithType.</para>
    /// </summary>
    //======================================================================
    internal class MethWithType : MethPropWithType
    {
        //------------------------------------------------------------
        // MethWithType Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal MethWithType() { }

        //------------------------------------------------------------
        // MethWithType Constructor (2)
        //
        /// <summary></summary>
        /// <param name="meth"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal MethWithType(METHSYM meth, AGGTYPESYM ats)
        {
            Set(meth, ats);
        }

        //------------------------------------------------------------
        // MethWithType Constructor (3)
        //
        /// <summary></summary>
        /// <param name="swt"></param>
        //------------------------------------------------------------
        internal MethWithType(SymWithType swt)
        {
            if (swt != null && swt.Sym != null)
            {
                Set(swt.Sym as METHSYM, swt.AggTypeSym);
            }
            else
            {
                Set(null, null);
            }
        }

        //------------------------------------------------------------
        // MethWithType.Set (1)
        //
        /// <summary></summary>
        /// <param name="meth"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal void Set(METHSYM meth, AGGTYPESYM ats)
        {
            base.Set(meth, ats);
        }

        //------------------------------------------------------------
        // MethWithType.Set (2)
        //
        /// <summary></summary>
        /// <param name="mwt"></param>
        //------------------------------------------------------------
        internal void Set(MethWithType mwt)
        {
            if (mwt != null)
            {
                this.Set(mwt.MethSym, mwt.AggTypeSym);
            }
            else
            {
                this.Set(null, null);
            }
        }

        //------------------------------------------------------------
        // MethWithType.Convert
        //
        /// <summary>
        /// If swt is MethWithType, convert to MethWithType and return it.
        /// Otherwise, create a MethWithType instance by swt and return it.
        /// </summary>
        /// <param name="swt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static new internal MethWithType Convert(SymWithType swt)
        {
            MethWithType mwt = swt as MethWithType;
            if (mwt != null) return mwt;
            return new MethWithType(swt);
        }
    }

    //======================================================================
    // class PropWithType
    //
    /// <summary>
    /// <para>Derives from MethPropWithType. (So derives from SymWithType.)
    /// Represents overridden members or implemented properties.</para>
    /// <para>Has only two fields Sym and AggTypeSym which derives from SymWithType.</para>
    /// </summary>
    //======================================================================
    internal class PropWithType : MethPropWithType
    {
        //------------------------------------------------------------
        // PropWithType Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal PropWithType() { }

        //------------------------------------------------------------
        // PropWithType Constructor (2)
        //
        /// <summary></summary>
        /// <param name="prop"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal PropWithType(PROPSYM prop, AGGTYPESYM ats)
        {
            Set(prop, ats);
        }

        //------------------------------------------------------------
        // PropWithType Constructor (3)
        //
        /// <summary></summary>
        /// <param name="swt"></param>
        //------------------------------------------------------------
        internal PropWithType(SymWithType swt)
        {
            if (swt != null && swt.Sym != null)
            {
                Set(swt.Sym as PROPSYM, swt.AggTypeSym);
            }
            else
            {
                Set(null, null);
            }
        }

        //------------------------------------------------------------
        // PropWithType.Set (1)
        //
        /// <summary></summary>
        /// <param name="prop"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal void Set(PROPSYM prop, AGGTYPESYM ats)
        {
            base.Set(prop, ats);
        }

        //------------------------------------------------------------
        // PropWithType.Set (2)
        //
        /// <summary></summary>
        /// <param name="pwt"></param>
        //------------------------------------------------------------
        internal void Set(PropWithType pwt)
        {
            if (pwt != null)
            {
                this.Set(pwt.PropSym, pwt.AggTypeSym);
            }
            else
            {
                this.Set(null, null);
            }
        }

        //------------------------------------------------------------
        // PropWithType.Convert
        //
        /// <summary>
        /// If swt is PropWithType, convert to PropWithType and return it.
        /// Otherwise, create a PropWithType instance by swt and return it.
        /// </summary>
        /// <param name="swt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static new internal PropWithType Convert(SymWithType swt)
        {
            PropWithType pwt = swt as PropWithType;
            if (pwt != null) return pwt;
            return new PropWithType(swt);
        }
    }

    //======================================================================
    // class EventWithType
    //
    /// <summary>
    /// <para>Derives from SymWithType.
    /// Represents overridden members or implemented events.</para>
    /// <para>Has only two fields Sym and AggTypeSym which derives from SymWithType.</para>
    /// </summary>
    //======================================================================
    internal class EventWithType : SymWithType
    {
        //------------------------------------------------------------
        // EventWithType Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal EventWithType() { }

        //------------------------------------------------------------
        // EventWithType Constructor (2)
        //
        /// <summary></summary>
        /// <param name="evt"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal EventWithType(EVENTSYM evt, AGGTYPESYM ats)
        {
            Set(evt, ats);
        }

        //------------------------------------------------------------
        // EventWithType Constructor (3)
        //
        /// <summary></summary>
        /// <param name="swt"></param>
        //------------------------------------------------------------
        internal EventWithType(SymWithType swt)
        {
            if (swt != null && swt.Sym != null)
            {
                Set(swt.Sym as EVENTSYM, swt.AggTypeSym);
            }
            else
            {
                Set(null, null);
            }
        }

        //------------------------------------------------------------
        // EventWithType.Set (1)
        //
        /// <summary></summary>
        /// <param name="evt"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal void Set(EVENTSYM evt, AGGTYPESYM ats)
        {
            base.Set(evt, ats);
        }

        //------------------------------------------------------------
        // EventWithType.Set (2)
        //
        /// <summary></summary>
        /// <param name="ewt"></param>
        //------------------------------------------------------------
        internal void Set(EventWithType ewt)
        {
            if (ewt != null)
            {
                this.Set(ewt.EventSym, ewt.AggTypeSym);
            }
            else
            {
                this.Set(null, null);
            }
        }

        //------------------------------------------------------------
        // EventWithType.Convert
        //
        /// <summary>
        /// If swt is EventWithType, convert to EventWithType and return it.
        /// Otherwise, create a EventWithType instance by swt and return it.
        /// </summary>
        /// <param name="swt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal EventWithType Convert(SymWithType swt)
        {
            EventWithType ewt = swt as EventWithType;
            if (ewt != null) return ewt;
            return new EventWithType(swt);
        }
    }

    //======================================================================
    // class FieldWithType
    //
    /// <summary>
    /// <para>Derives from SymWithType.
    /// Represents overridden members or implemented fields.</para>
    /// <para>Has only two fields Sym and AggTypeSym which derives from SymWithType.</para>
    /// </summary>
    //======================================================================
    internal class FieldWithType : SymWithType
    {
        //------------------------------------------------------------
        // FieldWithType Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal FieldWithType() { }

        //------------------------------------------------------------
        // FieldWithType Constructor (2)
        //
        /// <summary></summary>
        /// <param name="field"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal FieldWithType(MEMBVARSYM field, AGGTYPESYM ats)
        {
            Set(field, ats);
        }

        //------------------------------------------------------------
        // FieldWithType Constructor (3)
        //
        /// <summary></summary>
        /// <param name="swt"></param>
        //------------------------------------------------------------
        internal FieldWithType(SymWithType swt)
        {
            if (swt != null && swt.Sym != null)
            {
                MEMBVARSYM fieldSym = swt.Sym as MEMBVARSYM;
                if (fieldSym != null)
                {
                    Set(fieldSym, swt.AggTypeSym);
                    return;
                }
            }
            Set(null, null);
        }

        //------------------------------------------------------------
        // FieldWithType.Set (1)
        //
        /// <summary></summary>
        /// <param name="field"></param>
        /// <param name="ats"></param>
        //------------------------------------------------------------
        internal void Set(MEMBVARSYM field, AGGTYPESYM ats)
        {
            base.Set(field, ats);
        }

        //------------------------------------------------------------
        // FieldWithType.Set (2)
        //
        /// <summary></summary>
        /// <param name="fwt"></param>
        //------------------------------------------------------------
        internal void Set(FieldWithType fwt)
        {
            if (fwt != null)
            {
                this.Set(fwt.FieldSym, fwt.AggTypeSym);
            }
            else
            {
                this.Set(null, null);
            }
        }

        //------------------------------------------------------------
        // FieldWithType.Convert
        //
        /// <summary>
        /// If swt is FieldWithType, convert to FieldWithType and return it.
        /// Otherwise, create a FieldWithType instance by swt and return it.
        /// </summary>
        /// <param name="swt"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal FieldWithType Convert(SymWithType swt)
        {
            FieldWithType fwt = swt as FieldWithType;
            if (fwt != null)
            {
                return fwt;
            }
            return new FieldWithType(swt);
        }
    }

    //======================================================================
    // MethPropWithInst
    //
    /// <summary>
    /// <para>Extends MethPropWithType with the method type arguments.
    /// Properties will never have type args,
    /// but methods and properties share a lot of code so it's convenient to allow both here.</para>
    /// <para>Has field TypeArguments of type TypeArray.</para>
    /// </summary>
    /// <remarks>
    /// The default constructor does nothing so these are not safe to use uninitialized.
    /// Note that when they are used as member of an EXPR they are automatically zero filled by newExpr.
    /// </remarks>
    //======================================================================
    internal class MethPropWithInst : MethPropWithType
    {
        //------------------------------------------------------------
        // MethPropWithInst Fields and Properties
        //------------------------------------------------------------
        internal TypeArray TypeArguments = null;    // * typeArgs, * TypeArgs()

        //------------------------------------------------------------
        // MethPropWithInst Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal MethPropWithInst() { }

        //------------------------------------------------------------
        // MethPropWithInst Constructor (2)
        //
        /// <summary></summary>
        /// <param name="mps"></param>
        /// <param name="ats"></param>
        /// <param name="TypeArguments"></param>
        //------------------------------------------------------------
        internal MethPropWithInst(
            METHPROPSYM mps,
            AGGTYPESYM ats,
            TypeArray TypeArguments)    // = NULL)
        {
            Set(mps, ats, TypeArguments);
        }

        //------------------------------------------------------------
        // MethPropWithInst.Clear
        //
        /// <summary></summary>
        //------------------------------------------------------------
        new internal void Clear()
        {
            Sym = null;
            AggTypeSym = null;
            TypeArguments = null;
        }

        //------------------------------------------------------------
        // MethPropWithInst.Set (1)
        //
        /// <summary></summary>
        /// <param name="mps"></param>
        /// <param name="ats"></param>
        /// <param name="TypeArguments"></param>
        //------------------------------------------------------------
        internal void Set(METHPROPSYM mps, AGGTYPESYM ats, TypeArray TypeArguments)
        {
            if (mps == null)
            {
                ats = null;
                TypeArguments = null;
            }

            DebugUtil.Assert(
                ats == null ||
                mps != null && mps.ClassSym == ats.GetAggregate());
            DebugUtil.Assert(
                TypeArguments == null ||
                TypeArguments.Count == 0 ||
                mps != null && mps.IsMETHSYM);
            DebugUtil.Assert(
                TypeArguments == null ||
                !mps.IsMETHSYM ||
                (mps as METHSYM).TypeVariables.Count == TypeArguments.Count);

            this.Sym = mps;
            this.AggTypeSym = ats;
            this.TypeArguments = TypeArguments;
        }

        //------------------------------------------------------------
        // MethPropWithInst.Set (2)
        //
        /// <summary></summary>
        /// <param name="mpwi"></param>
        //------------------------------------------------------------
        internal void Set(MethPropWithInst mpwi)
        {
            if (mpwi != null)
            {
                this.Set(mpwi.MethPropSym, mpwi.AggTypeSym, mpwi.TypeArguments);
            }
            else
            {
                this.Set(null, null, null);
            }
        }

        //------------------------------------------------------------
        // MethPropWithInst.Equals
        //
        /// <summary>
        /// Override.
        /// Return true if all fields are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        public override bool Equals(object obj)
        {
            MethPropWithInst mpwi = obj as MethPropWithInst;
            if (mpwi != null)
            {
                return (
                    this.Sym == mpwi.Sym &&
                    this.AggTypeSym == mpwi.AggTypeSym &&
                    this.TypeArguments == mpwi.TypeArguments
                    );
            }
            return false;
        }

        //------------------------------------------------------------
        // MethPropWithInst.GetHashCode
        //
        /// <summary>Override.</summary>
        /// <returns></returns>
        //------------------------------------------------------------
        public override int GetHashCode()
        {
            return (
                this.Sym.GetHashCode() ^
                this.AggTypeSym.GetHashCode() ^
                this.TypeArguments.GetHashCode()) & 0x7FFFFFFF;
        }
    }

    ///******************************************************************************
    //    In debug this type asserts that the symbol is a METHSYM.
    //    In retail it is just MethPropWithInst.
    //******************************************************************************/
    //#ifndef DEBUG
    //    typedef MethPropWithInst MethWithInst;
    //#elif !defined(WITHTYPE_INLINES) // && DEBUG

    //======================================================================
    // class MethWithInst
    //
    /// <summary>
    /// <para>Extends MethPropWithType with the method type arguments.</para>
    /// <para>Has field TypeArguments of type TypeArray.</para>
    /// </summary>
    //======================================================================
    internal class MethWithInst : MethPropWithInst
    {
        //------------------------------------------------------------
        // MethWithInst Constructor (1)
        //
        /// <summary>Does nothing.</summary>
        //------------------------------------------------------------
        internal MethWithInst() { }

        //------------------------------------------------------------
        // MethWithInst Constructor (2)
        //
        /// <summary></summary>
        /// <param name="meth"></param>
        /// <param name="ats"></param>
        /// <param name="typeArgs"></param>
        //------------------------------------------------------------
        internal MethWithInst(
            METHSYM meth,
            AGGTYPESYM ats,
            TypeArray typeArgs)    // = NULL)
        {
            Set(meth, ats, typeArgs);
        }

        //------------------------------------------------------------
        // MethWithInst Constructor (3)
        //
        /// <summary></summary>
        /// <param name="mpwi"></param>
        //------------------------------------------------------------
        internal MethWithInst(MethPropWithInst mpwi)
        {
            if (mpwi != null && mpwi.Sym != null)
            {
                Set(mpwi.Sym as METHSYM, mpwi.AggTypeSym, mpwi.TypeArguments);
            }
            else
            {
                Set(null, null, null);
            }
        }

        //------------------------------------------------------------
        // MethWithInst.Set (1)
        //
        /// <summary></summary>
        /// <param name="meth"></param>
        /// <param name="ats"></param>
        /// <param name="typeArgs"></param>
        //------------------------------------------------------------
        internal void Set(METHSYM meth, AGGTYPESYM ats, TypeArray typeArgs)
        {
            base.Set(meth, ats, typeArgs);
        }

        //------------------------------------------------------------
        // MethWithInst.Set (2)
        //
        /// <summary></summary>
        /// <param name="mwi"></param>
        //------------------------------------------------------------
        internal void Set(MethWithInst mwi)
        {
            if (mwi != null)
            {
                this.Set(mwi.MethSym, mwi.AggTypeSym, mwi.TypeArguments);
            }
            else
            {
                this.Set(null, null, null);
            }
        }

        //------------------------------------------------------------
        // MethWithInst.Convert
        //
        /// <summary>
        /// If mpwi is MethWithInst, convert it to MethWithInst and return it.
        /// Otherwise, create a MethWithInst instance by mpwi and return it.
        /// </summary>
        /// <param name="mpwi"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        static internal MethWithInst Convert(MethPropWithInst mpwi)
        {
            MethWithInst mwi = mpwi as MethWithInst;
            if (mwi != null) return mwi;
            return new MethWithInst(mpwi);
        }
    }
}
