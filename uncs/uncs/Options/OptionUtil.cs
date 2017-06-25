//============================================================================
// OptionUtil.cs
//
// 2015/11/02 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Uncs
{
    //======================================================================
    // class OptionUtil (static)
    //======================================================================
    internal static class OptionUtil
    {
        //------------------------------------------------------------
        // OptionUtil.WarningNumbers
        //------------------------------------------------------------
        static internal int[] WarningNumbers =  // long m_rgiWarningIDs[];
        {
            28,67,78,
            105,108,109,114,162,164,168,169,183,184,189,197,
            207,219,251,252,253,278,279,280,282,
            402,414,419,420,422,429,435,436,437,440,444,458,464,465,467,469,472,
            602,612,618,626,628,642,649,652,657,658,659,660,661,665,672,675,679,680,684,688,693,
            728,
            1030,1058,
            1200,1201,1202,1203,1204,
            1522,1570,1571,1572,1573,1574,1580,1581,1584,1587,1589,1590,1591,1592,1595,1596,1598,
            1607,1610,1616,1633,1634,1635,1645,1658,1659, 1668,1682,1683,1684,1685,1687,1690,
            1691,1692,1694,1695,1696,1697,1698,1699,
            1700,1701,1702,1707,1709,1710,1711,1712,1717,1718,1720,1723,
            1911,
            2002,2014,2023,2029,3000,
            3001,3002,3003,3004,3005,3006,3007,3008,3009,3010,
            3011,3012,3013,3014,3015,3016,3017,3018,3019,
            3021,3022,3023,3024,3026,3027,
            5000,
        };

        //------------------------------------------------------------
        // OptionUtil.IsVaildWarningID
        //
        /// <summary></summary>
        /// <param name="errNo"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsValidWarningNumber(int errNo, out int index)
        {
            DebugUtil.Assert(OptionUtil.WarningNumbers != null);

            index = Array.BinarySearch(WarningNumbers, errNo);
            return (index >= 0 && index < WarningNumbers.Length);
        }

        //------------------------------------------------------------
        // OptionUtil.IsVaildWarningID (2)
        //
        /// <summary></summary>
        /// <param name="errNo"></param>
        /// <param name="errId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static bool IsValidWarningNumber(int errNo, out CSCERRID errId, out int index)
        {
            DebugUtil.Assert(OptionUtil.WarningNumbers != null);

            errId = CSCErrorInfo.Manager.GetIDfromNumber(errNo);
            index = Array.BinarySearch(WarningNumbers, errNo);
            return (index >= 0 && index < WarningNumbers.Length);
        }

        //------------------------------------------------------------
        // ArgumentsToOptionList
        //
        /// <summary>
        /// Split each command argument to option switch and option arguments
        /// and add them to option list.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal static void ArgumentsToOptionList(
            IList<string> args,
            int argCount,
            List<string[]> optionList)
        {
            if (args == null || optionList == null)
            {
                return;
            }

            for (int i = 0; i < argCount; ++i)
            {
                string argStr = args[i];
                if (String.IsNullOrEmpty(argStr))
                {
                    continue;
                }

                int pos;
                int lastIndex = argStr.Length - 1;
                char fch = argStr[0];
                char lch = argStr[lastIndex];
                string[] opt = null;

                //----------------------------------------------------
                // (1) Option (starts with "/" or "-")
                //----------------------------------------------------
                if (fch == '/' || fch == '-')
                {
                    pos = argStr.IndexOf(':');
                    if (pos >= lastIndex)
                    {
                        opt = new string[4]
                        {
                            argStr,
                            "/",
                            argStr.Substring(1, lastIndex - 1),
                            null,
                        };
                    }
                    else if (pos > 0)
                    {
                        opt = new string[4]
                        {
                            argStr,
                            "/",
                            argStr.Substring(1, pos - 1),
                            argStr.Substring(pos + 1),
                        };
                    }
                    else if (lch == '+')
                    {
                        opt = new string[4]
                        {
                            argStr,
                            "/",
                            argStr.Substring(1, lastIndex - 1),
                            "+",
                        };
                    }
                    else if (lch == '-')
                    {
                        opt = new string[4]
                        {
                            argStr,
                            "/",
                            argStr.Substring(1, lastIndex - 1),
                            "-",
                        };
                    }
                    else
                    {
                        opt = new string[3]
                        {
                            argStr,
                            "/",
                            argStr.Substring(1),
                        };
                    }
                }
                //----------------------------------------------------
                // (2) Response file (starts with "@")
                //----------------------------------------------------
                else if (fch == '@')
                {
                    opt = new string[3]
                    {
                        argStr,
                        "@",
                        argStr.Substring(1),
                    };
                }
                //----------------------------------------------------
                // (3) Otherwise
                //----------------------------------------------------
                else
                {
                    opt = new string[2] { argStr, argStr };
                }

                optionList.Add(opt);
            }
        }
    }
}

