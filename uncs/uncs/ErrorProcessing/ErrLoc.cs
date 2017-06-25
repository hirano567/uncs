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
// File: compiler.h
//
// Defined the main compiler class, which contains all the other
// sub-parts of the compiler.
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
// File: compiler.cpp
//
// Defined the main compiler class.
// ===========================================================================

//============================================================================
// ErrLoc.cs
//
// 2015/11/10 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // class ERRLOC
    //
    /// <summary>
    /// <para>location for reporting errors</para>
    /// <para>Holds
    /// <list type="bullet">
    /// <item>Reference to CSourceData instance</item>
    /// <item>Name of map file</item>
    /// <item>Unmapped position</item>
    /// <item>Mapped position</item>
    /// </list></para>
    /// <para>(In sscli, defined in csharp\sscomp\compiler.h)</para>
    /// </summary>
    //======================================================================
    internal class ERRLOC
    {
        //------------------------------------------------------------
        // ERRLOC Fields and Properties
        //------------------------------------------------------------

        private CSourceData sourceData = null;

        /// <summary>
        /// (R) SourceData instance.
        /// </summary>
        internal CSourceData SourceData
        {
            get { return sourceData; }
        }

        private string sourceFileName = null;

        /// <summary>
        /// (R) Source file name used in error messages.
        /// </summary>
        internal string SourceFileName
        {
            get { return sourceFileName; }
        }

        private string sourceMapFileName = null;

        /// <summary>
        /// (R) Map file name used in error messages.
        /// </summary>
        internal string SourceMapFileName
        {
            get { return sourceMapFileName; }
        }

        private POSDATA startPos = null;
        private POSDATA endPos = null;
        private POSDATA mapStartPos = null;
        private POSDATA mapEndPos = null;

        /// <summary>
        /// (R) true if this has the informations of error locations.
        /// </summary>
        internal bool HasLocation
        {
            get { return (startPos != null && !startPos.IsUninitialized); }
        }

        /// <summary>
        /// (R) Line index where an error occured.
        /// </summary>
        internal int LineIndex
        {
            get { return (startPos == null || startPos.IsUninitialized) ? -1 : startPos.LineIndex; }
        }

        /// <summary>
        /// (R) Column index where an error occured.
        /// </summary>
        internal int CharIndex
        {
            get { return (startPos == null || startPos.IsUninitialized) ? -1 : startPos.CharIndex; }
        }

        /// <summary>
        /// (R) Lenght of the error if the error is in one line.
        /// </summary>
        internal int Extent
        {
            get
            {
                DebugUtil.Assert(this.startPos != null && !this.startPos.IsUninitialized);
                if (this.endPos != null && !this.endPos.IsUninitialized)
                {
                    return (startPos.LineIndex == endPos.LineIndex ? endPos.CharIndex - startPos.CharIndex : 1);
                }
                return 1;
            }
        }

        /// <summary>
        /// (R) The index of the mapped line.
        /// </summary>
        internal int MappedLineIndex
        {
            get { return (mapStartPos == null || mapStartPos.IsUninitialized) ? -1 : mapStartPos.LineIndex; }
        }


        //------------------------------------------------------------
        // ERRLOC Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal ERRLOC() { }

        //------------------------------------------------------------
        // ERRLOC Constructor (2)
        //
        /// <summary>
        /// <para>Constructor.</para>
        /// <para>Get the current position from the BASENODE argument and
        /// mapping information from INFILESYM.SourceData.</para>
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal ERRLOC(INFILESYM inputFile, BASENODE node, bool fullPath)
        {
            DebugUtil.Assert(inputFile != null);

            sourceData = inputFile.SourceData;

            sourceFileName = (fullPath ? inputFile.FullName : inputFile.Name);
            sourceMapFileName = inputFile.Name;

            if (node != null)
            {
                SetLine(node);
                SetStart(node);
                this.endPos = new POSDATA();    //endPos.SetUninitialized();
                SetEnd(node);
                mapStartPos = (startPos != null ? startPos.Clone() : null);
                mapEndPos = (endPos != null ? endPos.Clone() : null);

                string sd;      // dummy
                bool bd1, bd2;  // dummy

                // mapStart、mapEnd の行番号をマップ先のもので置き換え、ファイル名を取得する。
                sourceData.Module.MapLocation(mapStartPos, out sourceMapFileName, out bd1, out bd2);
                sourceData.Module.MapLocation(mapEndPos, out sd, out bd1, out bd2);
            }
        }

        //------------------------------------------------------------
        // ERRLOC Constructor (3)
        //
        /// <para>Constructor.</para>
        /// <para>Get the current position from the BASENODE argument and
        /// mapping information from INFILESYM.SourceData of BSYMMGR.</para>
        /// <param name="symmgr"></param>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal ERRLOC(BSYMMGR symmgr, BASENODE node, bool fullPath)
        {
            if (node != null)
            {
                DebugUtil.Assert(symmgr != null);

                string inputFileName = node.GetContainingFileName(fullPath);
                INFILESYM inputFile = symmgr.FindInfileSym(inputFileName);

                sourceFileName = inputFileName;
                sourceMapFileName = inputFileName;

                if (inputFile != null)
                {
                    sourceData = inputFile.SourceData;
                    SetLine(node);
                    SetStart(node);
                    SetEnd(node);
                    mapStartPos = (startPos != null ? startPos.Clone() : null);
                    mapEndPos = (endPos != null ? endPos.Clone() : null);

                    string sd;
                    bool bd1, bd2;
                    sourceData.Module.MapLocation(mapStartPos, out sourceMapFileName, out bd1, out bd2);
                    sourceData.Module.MapLocation(mapEndPos, out sd, out bd1, out bd2);
                    return;
                }
            }
        }

        //------------------------------------------------------------
        // ERRLOC Constructor (4)
        //
        /// <summary></summary>
        /// <param name="sym"></param>
        //------------------------------------------------------------
        internal ERRLOC(INFILESYM sym, bool fullPath)
        {
            DebugUtil.Assert(sym != null);

            sourceData = sym.SourceData;
            sourceFileName = (fullPath ? sym.FullName : sym.Name);
            sourceMapFileName = sourceFileName;
        }

        //------------------------------------------------------------
        // ERRLOC Constructor (5)
        //
        /// <summary></summary>
        /// <param name="scope"></param>
        //------------------------------------------------------------
        internal ERRLOC(ImportScope scope, bool fullPath)
        {
            DebugUtil.Assert(scope != null);

            sourceData = null;
            sourceFileName = scope.GetFileName(fullPath);
            sourceMapFileName = sourceFileName;
        }

        //------------------------------------------------------------
        // ERRLOC Constructor (6)
        //
        /// <summary></summary>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        //------------------------------------------------------------
        internal ERRLOC(CSourceData data, POSDATA start, POSDATA end)
        {
            DebugUtil.Assert(data != null);

            this.sourceData = data;
            this.startPos = start;
            this.endPos = end;
            mapStartPos = (start != null ? start.Clone() : null);
            mapEndPos = (end != null ? end.Clone() : null);

            InitFromSourceModule(this.sourceData.Module);
        }

        //------------------------------------------------------------
        // ERRLOC Constructor (7)
        //
        /// <summary></summary>
        /// <param name="sourceModule"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        //------------------------------------------------------------
        internal ERRLOC(CSourceModuleBase sourceModule, POSDATA start, POSDATA end)
        {
            DebugUtil.Assert(sourceModule != null);

            this.sourceData = null;
            this.startPos = start;
            this.endPos = end;
            this.mapStartPos = (start != null ? start.Clone() : null);
            this.mapEndPos = (end != null ? end.Clone() : null);

            InitFromSourceModule(sourceModule);
        }

        //------------------------------------------------------------
        // ERRLOC Constructor (8)
        //
        /// <summary></summary>
        /// <param name="filename"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        //------------------------------------------------------------
        internal ERRLOC(string filename, POSDATA start, POSDATA end)
        {
            sourceData = null;
            this.startPos = start;
            this.endPos = end;
            this.mapStartPos = (start != null ? start.Clone() : null);
            this.mapEndPos = (end != null ? end.Clone() : null);

            sourceMapFileName = sourceFileName = filename;
        }

        //------------------------------------------------------------
        // ERRLOC.InitFromSourceModule
        //
        /// <summary>
        /// <para>Get mapped position and mapped file name from CSourceModule.</para>
        /// <para>mapStartPos and mapEndPos are set to startPos and endPos respectively.</para>
        /// </summary>
        /// <param name="sourceModule"></param>
        //------------------------------------------------------------
        private void InitFromSourceModule(CSourceModuleBase sourceModule)
        {
            DebugUtil.Assert(sourceModule != null);

            string sd;      // dummy
            bool bd1, bd2;  // dummy

            //sourceFileName = sourceModule.GetFileName();
            sourceFileName = IOUtil.SelectFileName(
                sourceModule.SourceText.SourceFileInfo,
                sourceModule.OptionManager.FullPaths);
            sourceModule.MapLocation(mapStartPos, out sourceMapFileName, out bd1, out bd2);
            sourceModule.MapLocation(mapEndPos, out sd, out bd1, out bd2);
        }

        //------------------------------------------------------------
        // ERRLOC.SetStart
        //
        // UNDONE:  REVIEW THIS FUNCTION THOROUGHLY
        /// <summary>
        /// <para>Recursively searches parse-tree for correct left-most node</para>
        /// </summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void SetStart(BASENODE node)
        {
            //EDMAURER SetLine () usually initializes 'start', but may not in some circumstances.
            if (startPos.IsUninitialized)
            {
                return;
            }

            LEXDATA ld = sourceData.LexData;
            if (node == null || node.TokenIndex == -1 || ld == null)
            {
                return;
            }

            SetStartInternal(ld, node);

            switch (node.Kind)
            {
                case NODEKIND.ACCESSOR:
                    // Get and Set always are the token before the '{'
                    SetStartInternal(ld, (node as ACCESSORNODE).OpenCurlyIndex, -1);
                    return;

                case NODEKIND.ARROW:    // BINOPNODE
                    // "->": find the start position of left operand.
                    if (node.AsARROW.Operand1 != null)
                    {
                        SetStart(node.AsARROW.Operand1);
                    }
                    return;

                case NODEKIND.ATTR: // ATTRNODE
                    // attribute: find the start position of its name.
                    if ((node as ATTRNODE).NameNode != null)
                    {
                        SetStart((node as ATTRNODE).NameNode);
                    }
                    return;

                case NODEKIND.ATTRDECL:
                    // declaration of attribute: find the start position of its name.
                    if ((node as ATTRDECLNODE).NameNode != null)
                    {
                        SetStart((node as ATTRDECLNODE).NameNode);
                    }
                    return;

                case NODEKIND.LIST:
                case NODEKIND.CALL:
                case NODEKIND.DEREF:
                case NODEKIND.BINOP:
                    // find the start position of the left operand.
                    if (node.AsANYBINOP.Operand1 != null &&
                        node.AsANYBINOP.Operator != OPERATOR.CAST)
                    {
                        SetStart(node.AsANYBINOP.Operand1);
                    }
                    return;

                case NODEKIND.CLASS:
                case NODEKIND.INTERFACE:
                case NODEKIND.STRUCT:
                    // find the start position of its name.
                    if (node.AsAGGREGATE.NameNode != null)
                    {
                        SetStart(node.AsAGGREGATE.NameNode);
                    }
                    return;

                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                    {
                        int i;
                        // Find the indentifier which must be the ctor/dtor name
                        for (i = node.TokenIndex;
                            i < ld.TokenCount && ld.TokenAt((int)i).TokenID != TOKENID.IDENTIFIER;
                            i++)
                        {
                            ;
                        }

                        SetStartInternal(ld, i, 0);
                        return;
                    }

                case NODEKIND.DELEGATE:
                    // find the start position of its name.
                    if ((node as DELEGATENODE).NameNode != null)
                    {
                        SetStart((node as DELEGATENODE).NameNode);
                    }
                    return;

                case NODEKIND.DOT:  // BINOPNODE
                    if (node.AsDOT.Operand1 != null)
                    {
                        SetStart(node.AsDOT.Operand1);
                    }
                    return;

                case NODEKIND.ENUM:
                    if ((node as ENUMNODE).NameNode != null)
                    {
                        SetStart((node as ENUMNODE).NameNode);
                    }
                    return;

                case NODEKIND.INDEXER:
                    if (node.AsANYPROPERTY.NameNode != null)
                    {
                        SetStart(node.AsANYPROPERTY.NameNode);
                    }
                    else
                    {
                        int i;
                        // Find 'this'
                        for (i = node.TokenIndex;
                            i < ld.TokenCount && ld.TokenAt((int)i).TokenID != TOKENID.THIS;
                            i++)
                        {
                            ;
                        }
                        SetStartInternal(ld, i, 0);
                    }
                    return;

                case NODEKIND.METHOD:
                    // use the name to keep the the same as fields
                    if ((node as METHODNODE).NameNode != null)
                    {
                        SetStart((node as METHODNODE).NameNode);
                    }
                    return;

                case NODEKIND.NAMESPACE:
                    // use the name to keep the the same as fields
                    if ((node as NAMESPACENODE).NameNode != null)
                    {
                        SetStart((node as NAMESPACENODE).NameNode);
                    }
                    return;

                case NODEKIND.OPERATOR:
                    {
                        TOKENID tok;
                        int i;

                        for (i = node.TokenIndex;
                            i < ld.TokenCount &&
                            (tok = ld.TokenAt((int)i).TokenID) != TOKENID.OPERATOR &&
                            tok != TOKENID.EXPLICIT && tok != TOKENID.IMPLICIT;
                            i++)
                        {
                            ;
                        }
                        SetStartInternal(ld, i, 0);
                        return;
                    }

                case NODEKIND.PROPERTY:
                    if (node.AsANYPROPERTY.NameNode != null)
                    {
                        SetStart(node.AsANYPROPERTY.NameNode);
                    }
                    return;

                case NODEKIND.UNOP:
                    if ((node.Operator == OPERATOR.POSTINC ||
                        node.Operator == OPERATOR.POSTDEC) && (node as UNOPNODE).Operand != null)
                    {
                        SetStart((node as UNOPNODE).Operand);
                    }
                    return;

                case NODEKIND.POINTERTYPE:
                    SetStart((node as POINTERTYPENODE).ElementTypeNode);
                    return;

                case NODEKIND.NULLABLETYPE:
                    SetStart((node as NULLABLETYPENODE).ElementTypeNode);
                    return;

                default:
                    break;
            }
        }

        //------------------------------------------------------------
        // ERRLOC.SetLine
        //
        // UNDONE:  REVIEW THIS FUNCTION THOROUGHLY
        /// <summary>
        /// <para>Sets start to correct line for given node</para>
        /// </summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void SetLine(BASENODE node)
        {
            LEXDATA ld = sourceData.LexData;

            if (node == null || node.TokenIndex == -1 || ld == null)
            {
                return;
            }

            // default if no special processing below.
            startPos = ld.TokenAt(node.TokenIndex);

            switch (node.Kind)
            {
                case NODEKIND.ACCESSOR:
                    // Get and Set always are the token before the '{'
                    startPos = ld.TokenAt(ld.PeekTokenIndexFrom((int)(node as ACCESSORNODE).OpenCurlyIndex, -1));
                    return;

                case NODEKIND.CLASS:
                case NODEKIND.INTERFACE:
                case NODEKIND.STRUCT:
                    if (node.AsAGGREGATE.NameNode != null)
                    {
                        SetLine(node.AsAGGREGATE.NameNode);
                    }
                    return;

                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                    {
                        int i;
                        // Find the indentifier which must be the ctor/dtor name
                        for (i = node.TokenIndex;
                            i < ld.TokenCount && ld.TokenAt(i).TokenID != TOKENID.IDENTIFIER;
                            i++)
                        {
                            ;
                        }
                        startPos = ld.TokenAt(i);
                        return;
                    }

                case NODEKIND.DELEGATE:
                    if ((node as DELEGATENODE).NameNode != null)
                    {
                        SetLine((node as DELEGATENODE).NameNode);
                    }
                    return;

                case NODEKIND.ENUM:
                    if ((node as ENUMNODE).NameNode != null)
                    {
                        SetLine((node as ENUMNODE).NameNode);
                    }
                    return;

                case NODEKIND.INDEXER:
                    if (node.AsANYPROPERTY.NameNode != null)
                    {
                        SetLine(node.AsANYPROPERTY.NameNode);
                    }
                    else
                    {
                        long i;
                        for (i = node.TokenIndex;
                            i < ld.TokenCount && ld.TokenAt((int)i).TokenID != TOKENID.THIS;
                            i++)
                        {
                            ;
                        }
                        startPos = ld.TokenAt((int)i);
                    }
                    return;

                case NODEKIND.METHOD:             // METHODNODE
                    // use the name to keep the the same as fields
                    if ((node as METHODNODE).NameNode != null)
                    {
                        SetLine((node as METHODNODE).NameNode);
                    }
                    return;

                case NODEKIND.NAMESPACE:
                    // use the name to keep the the same as fields
                    if ((node as NAMESPACENODE).NameNode != null)
                    {
                        SetLine((node as NAMESPACENODE).NameNode);
                    }
                    else if ((node as NAMESPACENODE).UsingNode != null)
                    {
                        SetLine((node as NAMESPACENODE).UsingNode);
                    }
                    return;

                case NODEKIND.OPERATOR:
                    {
                        TOKENID tok;

                        long i;
                        for (i = node.TokenIndex;
                            i < ld.TokenCount &&
                            (tok = ld.TokenAt((int)i).TokenID) != TOKENID.OPERATOR &&
                            tok != TOKENID.EXPLICIT &&
                            tok != TOKENID.IMPLICIT;
                            i++)
                        {
                            ;
                        }
                        startPos = ld.TokenAt((int)i);
                        return;
                    }

                case NODEKIND.NEW:
                    SetLine((node as NEWNODE).TypeNode);
                    return;

                case NODEKIND.PROPERTY:
                    if (node.AsANYPROPERTY.NameNode != null)
                    {
                        SetLine(node.AsANYPROPERTY.NameNode);
                    }
                    return;

                case NODEKIND.PREDEFINEDTYPE:
                    return;

                case NODEKIND.ARRAYTYPE:
                    SetLine((node as ARRAYTYPENODE).ElementTypeNode);
                    return;

                case NODEKIND.NAMEDTYPE:
                    SetLine((node as NAMEDTYPENODE).NameNode);
                    return;

                case NODEKIND.POINTERTYPE:
                    SetLine((node as POINTERTYPENODE).ElementTypeNode);
                    return;

                case NODEKIND.NULLABLETYPE:
                    SetLine((node as NULLABLETYPENODE).ElementTypeNode);
                    return;

                case NODEKIND.ATTRDECL:
                    SetLine((node as ATTRDECLNODE).NameNode);
                    return;

                default:
                    break;
            }
        }

        //------------------------------------------------------------
        // ERRLOC.SetEnd
        //
        /// <summary>
        /// Recursively searches parse-tree for correct right-most node
        /// </summary>
        /// <param name="node"></param>
        //------------------------------------------------------------
        internal void SetEnd(BASENODE node)
        {
            if (startPos.IsUninitialized)
            {
                return;
            }

            LEXDATA ld = sourceData.LexData;
            if (node == null || node.TokenIndex == -1 || ld == null)
            {
                return;
            }

            SetEndInternal(ld, node);

            switch (node.Kind)
            {
                case NODEKIND.ACCESSOR:
                    // Get and Set always are the token before the '{'
                    SetEndInternal(ld, (node as ACCESSORNODE).OpenCurlyIndex, -1);
                    return;

                case NODEKIND.ARROW:    // BINOPNODE
                    if (node.AsARROW.Operand2 != null)
                    {
                        SetEnd(node.AsARROW.Operand2);
                    }
                    return;

                case NODEKIND.ATTR:               // ATTRNODE
                    if ((node as ATTRNODE).NameNode != null)
                    {
                        SetEnd((node as ATTRNODE).NameNode);
                    }
                    return;

                case NODEKIND.ATTRDECL:
                    if ((node as ATTRDECLNODE).NameNode != null)
                    {
                        SetEnd((node as ATTRDECLNODE).NameNode);
                    }
                    return;

                case NODEKIND.ANONBLOCK:
                    if ((node as ANONBLOCKNODE).CloseParenIndex != -1)
                    {
                        // Try to grab the parameters
                        SetEndInternal(ld, (node as ANONBLOCKNODE).CloseParenIndex, 0);
                        // Try to grab the open-curly
                        SetEndInternal(ld, (node as ANONBLOCKNODE).CloseParenIndex, 1);
                    }

                    // And possibly the entire block
                    SetEndInternal(
                        ld,
                        ((node as ANONBLOCKNODE).BodyNode as BLOCKNODE).CloseCurlyIndex,
                        0);
                    return;

                case NODEKIND.LAMBDAEXPR:
                    if ((node as LAMBDAEXPRNODE).BodyNode.Kind == NODEKIND.BLOCK)
                    {
                        SetEndInternal(
                            ld,
                            ((node as LAMBDAEXPRNODE).BodyNode as BLOCKNODE).CloseCurlyIndex,
                            0);
                    }
                    else
                    {
                        SetEnd((node as LAMBDAEXPRNODE).BodyNode);
                    }
                    return;

                case NODEKIND.CALL: // CALL
                case NODEKIND.DEREF:
                    SetEndInternal(ld, node.AsANYCALL.CloseParenIndex, 0);
                    return;

                case NODEKIND.BINOP:    // BINOPNODE
                    if ((node as BINOPNODE).Operand2 != null)
                    {
                        SetEnd((node as BINOPNODE).Operand2);
                    }
                    return;

                case NODEKIND.LIST:
                    SetEnd(node.AsANYBINOP.Operand2);
                    return;

                case NODEKIND.CLASS:    //CLASSNODE
                case NODEKIND.INTERFACE:
                case NODEKIND.STRUCT:
                    if (node.AsAGGREGATE.NameNode != null)
                    {
                        SetEnd(node.AsAGGREGATE.NameNode);
                    }
                    return;

                case NODEKIND.CTOR:
                case NODEKIND.DTOR:
                    {
                        int i;
                        for (i = node.TokenIndex;
                            i < ld.TokenCount && ld.TokenAt(i).TokenID != TOKENID.IDENTIFIER;
                            i++)
                        {
                            ;
                        }
                        SetEndInternal(ld, i, 0);
                        return;
                    }

                case NODEKIND.DELEGATE:
                    if ((node as DELEGATENODE).NameNode != null)
                    {
                        SetEnd((node as DELEGATENODE).NameNode);
                    }
                    return;

                case NODEKIND.DOT:  // BINOPNODE
                    if (node.AsDOT.Operand2 != null)
                    {
                        SetEnd(node.AsDOT.Operand2);
                    }
                    return;

                case NODEKIND.ENUM:
                    if ((node as ENUMNODE).NameNode != null)
                    {
                        SetEnd((node as ENUMNODE).NameNode);
                    }
                    return;

                case NODEKIND.GENERICNAME:
                    SetEndInternal(ld, (node as GENERICNAMENODE).CloseAngleIndex, 0);
                    return;

                case NODEKIND.INDEXER:
                    {
                        int i;
                        for (i = node.TokenIndex; i < ld.TokenCount; i++)
                        {
                            SetEndInternal(ld, i, 0);
                            if (ld.TokenAt(i).TokenID == TOKENID.THIS)
                            {
                                break;
                            }
                        }

                        return;
                    }

                case NODEKIND.METHOD:   // METHODNODE
                    // use the name to keep the the same as fields
                    if ((node as METHODNODE).NameNode != null)
                    {
                        SetEnd((node as METHODNODE).NameNode);
                    }
                    return;

                case NODEKIND.NAMESPACE:
                    // use the name to keep the the same as fields
                    if ((node as NAMESPACENODE).NameNode != null)
                    {
                        SetEnd((node as NAMESPACENODE).NameNode);
                    }
                    return;

                case NODEKIND.OPERATOR:
                    {
                        int i;
                        for (i = node.TokenIndex;
                            i < ld.TokenCount - 1 && ld.TokenAt(i).TokenID != TOKENID.OPERATOR;
                            i++)
                        {
                            SetEndInternal(ld, i, 0);
                        }

                        SetEndInternal(ld, i, 0);
                        if ((node as OPERATORMETHODNODE).Operator == OPERATOR.IMPLICIT ||
                            (node as OPERATORMETHODNODE).Operator == OPERATOR.EXPLICIT)
                        {
                            // for conversion operators the type is the end of the name
                            // (or the token before the open paren)
                            SetEndInternal(ld, (node as OPERATORMETHODNODE).OpenParenIndex, -1);
                        }
                        else
                        {
                            // For non-conversion operators the token after 'operator'
                            // which is the end of the 'name'
                            SetEndInternal(ld, i, 1);
                        }

                        return;
                    }

                case NODEKIND.NEW:
                    // The type is the most important
                    SetEnd((node as NEWNODE).TypeNode);
                    // But try to get the arguments/array indexes
                    SetEndInternal(ld, (node as NEWNODE).CloseParenIndex, 0);
                    return;

                case NODEKIND.PROPERTY:
                    if (node.AsANYPROPERTY.NameNode != null)
                    {
                        SetEnd(node.AsANYPROPERTY.NameNode);
                    }
                    return;

                case NODEKIND.PREDEFINEDTYPE:
                case NODEKIND.POINTERTYPE:
                case NODEKIND.NULLABLETYPE:
                    return;

                case NODEKIND.NAMEDTYPE:
                    SetEnd((node as NAMEDTYPENODE).NameNode);
                    return;

                case NODEKIND.ARRAYTYPE:
                    // For array types, the token index is the open '['.  Add the number of
                    // dimensions and you land on the ']' (1==[], 2==[,], 3==[,,], etc).
                    if ((node as ARRAYTYPENODE).Dimensions == -1)
                    {
                        SetEndInternal(ld, node.TokenIndex, 2); // unknown rank is [?]
                    }
                    else
                    {
                        SetEndInternal(ld, node.TokenIndex, (node as ARRAYTYPENODE).Dimensions);
                    }
                    return;

                case NODEKIND.UNOP:
                    switch (node.Operator)
                    {
                        case OPERATOR.PAREN:
                        case OPERATOR.PREINC:
                        case OPERATOR.PREDEC:
                            SetEnd((node as UNOPNODE).Operand);
                            break;
                        default:
                            break;
                    }
                    return;

                case NODEKIND.QUERYEXPR:
                    SetEnd((node as QUERYEXPRNODE).QueryBodyNode);
                    break;

                case NODEKIND.FROMCLAUSE:
                case NODEKIND.FROMCLAUSE2:
                    SetEnd((node as FROMCLAUSENODE).ExpressionNode);
                    break;

                case NODEKIND.LETCLAUSE:
                    SetEnd((node as LETCLAUSENODE).ExpressionNode);
                    break;

                case NODEKIND.WHERECLAUSE:
                    SetEnd((node as WHERECLAUSENODE).LambdaExpressionNode);
                    break;

                case NODEKIND.JOINCLAUSE:
                    JOINCLAUSENODE joinNode = node as JOINCLAUSENODE;
                    DebugUtil.Assert(joinNode != null);
                    if (joinNode.IntoNameNode != null)
                    {
                        SetEnd(joinNode.IntoNameNode);
                    }
                    else
                    {
                        if (joinNode.IntoNameNode != null)
                        {
                            SetEnd(joinNode.IntoNameNode);
                        }
                        else
                        {
                            SetEnd(joinNode.EqualRightLambdaExpressionNode);
                        }
                    }
                    break;

                case NODEKIND.ORDERBYCLAUSE:
                    ORDERBYCLAUSENODE obNode = node as ORDERBYCLAUSENODE;
                    DebugUtil.Assert(obNode!=null);
                    if (obNode.LastOrdering != null)
                    {
                        SetEnd(obNode.LastOrdering.LambdaExpressionNode);
                    }
                    else
                    {
                        return;
                    }
                    break;

                case NODEKIND.SELECTCLAUSE:
                    SetEnd((node as SELECTCLAUSENODE).LambdaExpressionNode);
                    break;

                case NODEKIND.GROUPCLAUSE:
                    SetEnd((node as GROUPCLAUSENODE).ByExpressionNode);
                    break;

                case NODEKIND.QUERYCONTINUATION:
                    SetEnd((node as QUERYCONTINUATIONNODE).QueryBodyNode);
                    break;

                default:
                    break;
            }
        }

        //------------------------------------------------------------
        // ERRLOC.SetStartInternal (1)
        //
        /// <summary>
        /// only set the start if it is on the same line as we want to report the error on
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="node"></param>
        //------------------------------------------------------------
        private void SetStartInternal(LEXDATA ld, BASENODE node)
        {
            DebugUtil.Assert(!startPos.IsUninitialized);
            SetStartInternal(ld, node.TokenIndex, 0);
        }

        //------------------------------------------------------------
        // ERRLOC.SetStartInternal (2)
        //
        /// <summary>
        /// <para>only set the start if it is on the same line
        /// as we want to report the error on</para>
        /// </summary>
        /// <param name="lexData"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="tokenOffset"></param>
        //------------------------------------------------------------
        private void SetStartInternal(
            LEXDATA lexData,
            int tokenIndex,
            int tokenOffset)	// = 0
        {
            DebugUtil.Assert(!startPos.IsUninitialized);

            if (tokenIndex >= 0)
            {
                //tokenIndex = CParser.PeekTokenIndexFrom(lexData, (int)tokenIndex, (int)tokenOffset);
                tokenIndex = lexData.PeekTokenIndexFrom(tokenIndex, tokenOffset);
                if (lexData.TokenAt((int)tokenIndex).LineIndex == startPos.LineIndex)
                {
                    startPos = lexData.TokenAt(tokenIndex);
                }
            }
        }

        //------------------------------------------------------------
        // ERRLOC.SetEndInternal (1)
        //
        /// <summary>
        /// only set the end if it is on the same line
        /// as we want to report the error on
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="node"></param>
        //------------------------------------------------------------
        private void SetEndInternal(LEXDATA ld, BASENODE node)
        {
            DebugUtil.Assert(!startPos.IsUninitialized);
            SetEndInternal(ld, (int)node.TokenIndex, 0);
        }

        //------------------------------------------------------------
        // ERRLOC.SetEndInternal (2)
        //
        /// <summary>
        /// If the endPos is on the same line to startPos,
        /// set the StopPosition of the error token to endPos.
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="tokidx"></param>
        /// <param name="tokOffset"></param>
        //------------------------------------------------------------
        private void SetEndInternal(
            LEXDATA ld,
            int tokidx,
            int tokOffset)  // = 0
        {
            DebugUtil.Assert(!startPos.IsUninitialized);

            if (tokidx >= 0)
            {
                //tokidx = CParser.PeekTokenIndexFrom(ld, (int)tokidx, (int)tokOffset);
                tokidx = ld.PeekTokenIndexFrom(tokidx, tokOffset);
                if (ld.TokenAt(tokidx).LineIndex == startPos.LineIndex)
                {
                    endPos = ld.TokenAt(tokidx).StopPosition();
                }
            }
        }
    }
}
