//============================================================================
// Converter.cs  (uncs\Utilities)
//
// 2015/03/28
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // SystemConverter
    //
    // Define delegates which derive from System.Converter
    //======================================================================
    static internal class SystemConverter
    {
        //------------------------------------------------------------
        // AggTypeSymToTypeSym
        //------------------------------------------------------------
        static internal System.Converter<AGGTYPESYM, TYPESYM> AggTypeSymToTypeSym
            = delegate(AGGTYPESYM ats) { return (ats as TYPESYM); };

        //------------------------------------------------------------
        // TypeSymToAggTypeSym
        //------------------------------------------------------------
        static internal System.Converter<TYPESYM, AGGTYPESYM> TypeSymToAggTypeSym
            = delegate(TYPESYM agg) { return (agg as AGGTYPESYM); };

        //------------------------------------------------------------
        // TyVarSymToTypeSym
        //------------------------------------------------------------
        static internal System.Converter<TYVARSYM, TYPESYM> TyVarSymToTypeSym
            = delegate(TYVARSYM tvs) { return (tvs as TYPESYM); };
    }
}