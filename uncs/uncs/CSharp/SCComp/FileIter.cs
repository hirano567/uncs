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
// File: fileiter.h
//
// Defines various iterators for C# files
// ===========================================================================

//============================================================================
// FileIter.cs
//
// 2013/11/11
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    // Iterators for OUTFILESYMs

    //======================================================================
    // OutFileIteratorBase
    //
    /// <summary>
    /// <para>Abstract method. Must implement IsValid property.</para>
    /// <para>Return OUTFULESYMs of child list of compiler.MainSymbolManager.FileRootSym</para>
    /// </summary>
    //======================================================================
    abstract internal class OutFileIteratorBase
    {
        //------------------------------------------------------------
        // OutFileIteratorBase Fields and Properties
        //------------------------------------------------------------
        protected OUTFILESYM currentOutFileSym;

        internal OUTFILESYM Current
        {
            get { return currentOutFileSym; }
        }

        //------------------------------------------------------------
        // OutFileIteratorBase.IsValid (abstract property)
        //
        /// <summary>
        /// Abstract method. Determine if currentOutFileSym is to be processed.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        abstract protected bool IsValid { get; }

        //------------------------------------------------------------
        // OutFileIteratorBase Constructor
        //------------------------------------------------------------
        protected OutFileIteratorBase()
        {
            currentOutFileSym = null;
        }

        //------------------------------------------------------------
        // OutFileIteratorBase.Reset
        //
        /// <summary>
        /// OUTFILESYMs are elements of the child list
        /// of compiler.MainSymbolManager.FileRootSym.
        /// Search the first valid OUTFILESYM in the list
        /// and set it currentOutFileSym.
        /// </summary>
        /// <param name="compiler"></param>
        //------------------------------------------------------------
        virtual internal OUTFILESYM Reset(COMPILER compiler)
        {
            currentOutFileSym = compiler.GetFirstOutFile();
            if (currentOutFileSym == null)
            {
                return null;
            }

            AdvanceToValid();
            return Current;
        }

        //------------------------------------------------------------
        // OutFileIteratorBase.Next
        //
        /// <summary>
        /// (R) Find the next valid OUTFILESYM with AdvanceToValid method recursively.
        /// </summary>
        //------------------------------------------------------------
        virtual internal OUTFILESYM Next()
        {
            if (currentOutFileSym == null)
            {
                return null;
            }
            currentOutFileSym = currentOutFileSym.NextOutputFile();
            AdvanceToValid();
            return Current;
        }

        //------------------------------------------------------------
        // OutFileIteratorBase.AdvanceToValid
        //
        /// <summary>
        /// Find the next valid OUTFILESYM with Next method recursively.
        /// </summary>
        //------------------------------------------------------------
        protected void AdvanceToValid()
        {
            if (currentOutFileSym != null && !IsValid)
            {
                Next();
            }
        }
    }

    //======================================================================
    // OutFileIterator
    //
    /// <summary>
    /// <para>Derives from OutFileIteratorBase.</para>
    /// <para>All OUTFILESYMs are valid.</para>
    /// </summary>
    //======================================================================
    internal class OutFileIterator : OutFileIteratorBase
    {
        override protected bool IsValid
        {
            get { return true; }
        }
    }

    //======================================================================
    // SourceOutFileIterator
    //
    /// <summary>
    /// <para>Derives from OutFileIteratorBase.</para>
    /// <para>the OUTFILESYMs of resource or the OUTFILESYMs
    /// without a name are not valid.</para>
    /// </summary>
    //======================================================================
    internal class SourceOutFileIterator : OutFileIteratorBase
    {
        override protected bool IsValid
        {
            get
            {
                return (
                    !currentOutFileSym.IsResource &&
                    !String.IsNullOrEmpty(currentOutFileSym.Name));
            }
        }
    }

    // Iterators for INFILESYMs

    //======================================================================
    // IInfileIterator
    //======================================================================
    internal interface IInfileIterator
    {
        INFILESYM Next();
        INFILESYM Current { get; }
    }

    //======================================================================
    // InFileIteratorBase
    //
    /// <summary>
    /// <para>Abstract class implementing IInfileIterator Must define IsValid method.</para>
    /// <para>Return the INFILESYMs of the child list of an OUTFILESYM</para>
    /// </summary>
    //======================================================================
    abstract internal class InFileIteratorBase : IInfileIterator
    {
        //------------------------------------------------------------
        // InFileIteratorBase Fields and Properties
        //------------------------------------------------------------
        protected INFILESYM currentInFileSym;

        public INFILESYM Current
        {
            get { return currentInFileSym; }
        }

        //------------------------------------------------------------
        // InFileIteratorBase Constructor
        //------------------------------------------------------------
        protected InFileIteratorBase()
        {
            currentInFileSym = null;
        }

        //------------------------------------------------------------
        // InFileIteratorBase.IsValid (abstract property)
        //
        /// <summary>
        /// Abstract method. Determine if currentInFileSym is valid.
        /// </summary>
        //------------------------------------------------------------
        abstract protected bool IsValid { get; }

        //------------------------------------------------------------
        // InFileIteratorBase.Reset
        //
        /// <summary>
        /// <para>Set the first valid INFILESYM of the child list
        /// of outfile to currentInFileSym.</para>
        /// </summary>
        //------------------------------------------------------------
        internal INFILESYM Reset(OUTFILESYM outfile)
        {
            currentInFileSym = outfile.FirstInFileSym();
            if (currentInFileSym == null)
            {
                return null;
            }
            AdvanceToValid();
            return Current;
        }

        //------------------------------------------------------------
        // InFileIteratorBase.Next
        //
        /// <summary>
        /// Find the next valid INFILESYM with AdvanceToValid method recursively.
        /// </summary>
        //------------------------------------------------------------
        public INFILESYM Next()
        {
            currentInFileSym = currentInFileSym.NextInFileSym();
            if (currentInFileSym == null)
            {
                return null;
            }
            AdvanceToValid();
            return Current;
        }

        //------------------------------------------------------------
        // InFileIteratorBase.AdvanceToValid
        //
        /// <summary>
        /// Find the next valid INFILESYM with Next method recursively.
        /// </summary>
        //------------------------------------------------------------
        protected void AdvanceToValid()
        {
            if (currentInFileSym != null && !IsValid)
            {
                Next();
            }
        }
    }

    //======================================================================
    // InFileIterator
    //
    /// <summary>
    /// <para>Derives from InFileIteratorBase.</para>
    /// <para>All INFILESYMs are valid.</para>
    /// </summary>
    //======================================================================
    internal class InFileIterator : InFileIteratorBase
    {
        override protected bool IsValid
        {
            get { return true; }
        }
    }

    //  Combined Out/In iterator

    //======================================================================
    // CombinedFileIterator
    //
    /// <summary>
    /// Returns all valid INFILESYMs in order, which a COMPILER instance has.
    /// </summary>
    //======================================================================
    internal class CombinedFileIterator : IInfileIterator
    {
        //------------------------------------------------------------
        // CombinedFileIterator Fields and Properties
        //------------------------------------------------------------
        private OutFileIteratorBase outIteratorBase = null;
        private InFileIteratorBase inIteratorBase = null;

        public INFILESYM Current
        {
            get
            {
                DebugUtil.Assert(inIteratorBase != null);
                return inIteratorBase.Current;
            }
        }

        //------------------------------------------------------------
        // CombinedFileIterator Constructor (1)
        //------------------------------------------------------------
        internal CombinedFileIterator() { }

        //------------------------------------------------------------
        // CombinedFileIterator Constructor (2)
        //------------------------------------------------------------
        internal CombinedFileIterator(
            OutFileIteratorBase outIter,
            InFileIteratorBase inIter)
        {
            this.Set(outIter, inIter);
        }

        //------------------------------------------------------------
        // CombinedFileIterator.Set
        //------------------------------------------------------------
        internal void Set(OutFileIteratorBase outIter,
            InFileIteratorBase inIter)
        {
            this.outIteratorBase = outIter;
            this.inIteratorBase = inIter;
        }

        //------------------------------------------------------------
        // CombinedFileIterator.Reset
        //------------------------------------------------------------
        internal INFILESYM Reset(COMPILER compiler)
        {
            outIteratorBase.Reset(compiler);
            inIteratorBase.Reset(outIteratorBase.Current);
            return Current;
        }

        //------------------------------------------------------------
        // CombinedFileIterator.Next
        //
        /// <summary>
        /// <para>Returns the next valid INFILESYM of ths current OUTFILESYM.
        /// If the current OUTFILESYM has no next valid INFILESYM,
        /// return the first valid INFILESYM of the next OUTFILESYM.</para>
        /// </summary>
        //------------------------------------------------------------
        public INFILESYM Next()
        {
            inIteratorBase.Next();
            if (inIteratorBase.Current == null)
            {
                outIteratorBase.Next();
                if (outIteratorBase.Current != null)
                {
                    inIteratorBase.Reset(outIteratorBase.Current);
                }
            }
            return Current;
        }
    }

    //======================================================================
    // SourceFileIterator
    //
    /// <summary>
    /// <para>Derives from CombinedFileIterator.</para>
    /// <para>Skips the INFILESYMs of the OUTFILESYMs of resource files
    /// or of the OUTFILESYM without a name.</para>
    /// <para>(CSharp\SCComp\FileIter.cs)</para>
    /// </summary>
    //======================================================================
    internal class SourceFileIterator : CombinedFileIterator
    {
        private SourceOutFileIterator outIterator = null;
        private InFileIterator inIterator = null;

        internal SourceFileIterator()
        {
            outIterator = new SourceOutFileIterator();
            inIterator = new InFileIterator();
            base.Set(outIterator, inIterator);
        }
    }

    //======================================================================
    // AllInFileIterator
    //======================================================================
    internal class AllInFileIterator : CombinedFileIterator
    {
        private OutFileIterator outIterator = null;
        private InFileIterator inIterator = null;

        internal AllInFileIterator()
        {
            outIterator = new OutFileIterator();
            inIterator = new InFileIterator();
            base.Set(outIterator, inIterator);
        }
    }

    //
    // Iterator for AGGSYMs
    //

    //======================================================================
    // AggIterator
    //
    /// <summary>
    /// (CSharp\SCComp\FileIter.cs)
    /// </summary>
    //======================================================================
    internal class AggIterator
    {
        //------------------------------------------------------------
        // AggIterator Fields and Properties
        //------------------------------------------------------------
        protected AGGSYM aggCur;

        internal AGGSYM Current
        {
            get { return aggCur; }
        }

        //------------------------------------------------------------
        // AggIterator
        //------------------------------------------------------------
        internal AGGSYM Reset(INFILESYM infile)
        {
            DebugUtil.Assert(infile.RootNsDeclSym != null);

            aggCur = GetFirstInListNsDecl(infile.RootNsDeclSym.FirstChildSym);
            return aggCur;
        }

        //------------------------------------------------------------
        // AggIterator.Next
        //------------------------------------------------------------
        internal AGGSYM Next()
        {
            aggCur = GetNext(aggCur);
            return aggCur;
        }

        //------------------------------------------------------------
        // AggIterator.GetFirstInListNsDecl
        //------------------------------------------------------------
        static protected AGGSYM GetFirstInListNsDecl(SYM sym)
        {
            // Only use this for child lists in an NSDECL.
            DebugUtil.Assert(sym == null || sym.ParentSym.IsNSDECLSYM);

            AGGSYM agg;
            for (; sym != null; sym = sym.NextSym)
            {
                switch (sym.Kind)
                {
                    case SYMKIND.AGGDECLSYM:
                        if ((sym as AGGDECLSYM).IsFirst)
                        {
                            return (sym as AGGDECLSYM).AggSym;
                        }
                        break;

                    case SYMKIND.NSDECLSYM:
                        agg = GetFirstInListNsDecl((sym as NSDECLSYM).FirstChildSym);
                        if (agg != null)
                        {
                            return agg;
                        }
                        break;

                    case SYMKIND.GLOBALATTRSYM:
                        break;

                    default:
                        DebugUtil.Assert(false, "Bad SK");
                        break;
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // AggIterator.GetFirstInListAgg
        //------------------------------------------------------------
        static protected AGGSYM GetFirstInListAgg(SYM sym)
        {
            // Only use this for child lists in an AGG.
            DebugUtil.Assert(sym == null || sym.ParentSym.IsAGGSYM);

            for (; sym != null; sym = sym.NextSym)
            {
                if (sym.IsAGGSYM)
                {
                    return (sym as AGGSYM);
                }
            }
            return null;
        }

        //------------------------------------------------------------
        // AggIterator.GetNext
        //------------------------------------------------------------
        static protected AGGSYM GetNext(AGGSYM agg)
        {
            if (agg == null)
            {
                return null;
            }

            // Children first. We process nested AGGs with the outer AGG.
            AGGSYM aggNext = GetFirstInListAgg(agg.FirstChildSym);
            if (aggNext != null)
            {
                return aggNext;
            }

            // Check siblings. If none found move up a level. Once agg's parent is a NS
            // the processing is different (following this loop).
            for (; agg.ParentBagSym.IsAGGSYM; agg = agg.ParentBagSym as AGGSYM)
            {
                aggNext = GetFirstInListAgg(agg.NextSym);
                if (aggNext != null)
                {
                    return aggNext;
                }
            }

            // Agg's parent is a NS. Switch to searching DECLs.
            // ASSERT(agg && agg.Parent().IsNSSYM);
            if (agg == null || !agg.ParentBagSym.IsNSSYM)
            {
                return null;
            }
            for (DECLSYM decl = agg.FirstDeclSym; decl != null; decl = decl.ParentDeclSym)
            {
                aggNext = GetFirstInListNsDecl(decl.NextSym);
                if (aggNext != null)
                {
                    return aggNext;
                }
            }
            return null;
        }
    }
}
