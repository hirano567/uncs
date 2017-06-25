//============================================================================
// Interfaces.cs
//
// 2015/03/11
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // interface ICOPY
    //
    /// <summary>
    /// CopyFrom(T src) メソッドを宣言している。
    /// </summary>
    //======================================================================
    internal interface ICOPYFROM<T>
    {
        void CopyFrom(T src);
    }
}
