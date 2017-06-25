//============================================================================
// Utility  DebugUtil.cs
//
// 2015/04/12
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Uncs
{
    //======================================================================
    // class DebugUtil (static)
    //======================================================================
    static internal partial class DebugUtil
    {
        //------------------------------------------------------------
        // DebugUtil Constructor
        //------------------------------------------------------------
        static DebugUtil()
        {
#if DEBUG_STOPWATCH
            stopwatchList.Add(new Stopwatch());
            stopwatchDescList.Add("Main");
#endif
        }

#if DEBUG || DEBUG2
        //------------------------------------------------------------
        // DebugUtil.ShowSystemType
        //------------------------------------------------------------
        internal static string ShowSystemType(Type type)
        {
            StringBuilder sb = new StringBuilder();
            ShowSystemTypeCore(type, sb, "");
            return sb.ToString();
        }

        //------------------------------------------------------------
        // DebugUtil.ShowSystemType
        //------------------------------------------------------------
        internal static void ShowSystemTypeCore(Type type, StringBuilder sb, string indent)
        {
            if (type == typeof(object))
            {
                sb.AppendFormat("{0}{1}", indent, type.Name);
                return;
            }

            //--------------------------------------------------------
            // Generic Parameter
            //--------------------------------------------------------
            if (type.IsGenericParameter)
            {
                sb.AppendFormat("{0}{1} (Generic Parameter)\n", indent, type.Name);

                Type[] constraints = type.GetGenericParameterConstraints();
                if (constraints != null && constraints.Length > 0)
                {
                    indent += "  ";
                    for (int i = 0; i < constraints.Length; ++i)
                    {
                        sb.AppendFormat("[Constraint {0}]\n", i);
                        ShowSystemTypeCore(constraints[i], sb, indent);
                    }
                }
            }

            //--------------------------------------------------------
            // Array
            //--------------------------------------------------------
            if (type.IsArray)
            {
                sb.AppendFormat("{0}Array: rank{1}\n", indent, type.GetArrayRank());
                indent += "  ";
                sb.Append(indent + "[Element Type]\n");
                ShowSystemTypeCore(type.GetElementType(), sb, indent);
            }

            //--------------------------------------------------------
            // Otherwise
            //--------------------------------------------------------
            sb.AppendFormat("{0}{1}\n", indent, type.Name);
            indent += "  ";

            sb.Append("indent +[Base Type]\n");
            ShowSystemTypeCore(type.BaseType, sb, indent);

            Type[] ifaces = type.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                for (int i = 0; i < ifaces.Length; ++i)
                {
                    sb.AppendFormat(indent + "[Interface {0}]\n", i);
                    ShowSystemTypeCore(ifaces[i], sb, indent);
                }
            }


            Type[] genericArgs = type.GetGenericArguments();
            if (genericArgs != null && genericArgs.Length > 0)
            {
                for (int i = 0; i < genericArgs.Length; ++i)
                {
                    sb.AppendFormat(indent + "[Generic Argument {0}]\n", i);
                    ShowSystemTypeCore(genericArgs[i], sb, indent);
                }
            }
        }
#endif

#if DEBUG
        //------------------------------------------------------------
        // NodeDictionary
        //------------------------------------------------------------
        static internal Dictionary<int, BASENODE> NodeDictionary = new Dictionary<int, BASENODE>();

        static internal void DebugNodesAdd(BASENODE node)
        {
            if (node == null) return;
            try
            {
                NodeDictionary.Add(node.NodeID, node);
            }
            catch (ArgumentException)
            {
            }
        }

        static internal void DebugNodesOutput(StringBuilder sb)
        {
            foreach (KeyValuePair<int, BASENODE> kv in NodeDictionary)
            {
                sb.Append("==============================\n");
                int NodeID = kv.Value.NodeID;
                NODEKIND kind = kv.Value.Kind;
                if (NodeID >= 2262)
                {
                    ;
                }
                kv.Value.Debug1(sb);
                sb.Append("\n");
            }
        }

        //------------------------------------------------------------
        // SymDictionary
        //------------------------------------------------------------
        static internal Dictionary<int, SYM> SymDictionary = new Dictionary<int, SYM>();

        static internal void DebugSymsAdd(SYM sym)
        {
            if (sym == null) return;
            try
            {
                SymDictionary.Add(sym.SymID, sym);
            }
            catch (ArgumentException)
            {
            }
        }

        static internal void DebugSymsOutput(StringBuilder sb)
        {
            foreach (KeyValuePair<int, SYM> kv in SymDictionary)
            {
                sb.Append("==============================\n");
                kv.Value.Debug(sb);
            }
        }

        //------------------------------------------------------------
        // ExprDictionary
        //------------------------------------------------------------
        static internal Dictionary<int, EXPR> ExprDictionary = new Dictionary<int, EXPR>();

        static internal void DebugExprsAdd(EXPR expr)
        {
            if (expr == null) return;
            try
            {
                ExprDictionary.Add(expr.ExprID, expr);
            }
            catch (ArgumentException)
            {
            }
        }

        static internal void DebugExprsOutput(StringBuilder sb)
        {
            foreach (KeyValuePair<int, EXPR> kv in ExprDictionary)
            {
                sb.Append("==============================\n");
                kv.Value.Debug(sb);
                sb.Append("\n");
            }
        }
#endif

        //------------------------------------------------------------
        // DebugUtil.Assert
        //
        /// <summary>
        /// <para>If condition does not hold, call System.Diagnostics.Debug.Assert(bool...)</para>
        /// <para>(Utilities\\DebugUtil.cs, palrt\inc\vsassert.h (sscli))</para>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="messages"></param>
        //------------------------------------------------------------
        [Conditional("DEBUG")]
        static internal void Assert(bool condition, params string[] messages)
        {
            if (condition == true)
            {
                return;
            }

            switch (messages.Length)
            {
                case 0:
                    System.Diagnostics.Debug.Assert(condition);
                    break;
                case 1:
                    System.Diagnostics.Debug.Assert(condition, messages[0]);
                    break;
                case 2:
                    System.Diagnostics.Debug.Assert(condition, messages[0], messages[1]);
                    break;
                default:
                    StringBuilder sb = new StringBuilder();
                    for (int i = 1; i < messages.Length; ++i)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(messages[i]);
                    }
                    System.Diagnostics.Debug.Assert(condition, messages[0], sb.ToString());
                    break;
            }
        }

        //------------------------------------------------------------
        // DebugUtil.VsVerify
        //
        /// <summary>
        /// <para>(Utilities\\DebugUtil.cs, palrt\inc\vsassert.h (sscli))</para>
        /// </summary>
        //------------------------------------------------------------
        static internal void VsVerify(bool condition, string message)
        {
            Assert(condition, message);
        }

        //------------------------------------------------------------
        // DebugUtil.VsFail
        //
        /// <summary>
        /// <para>(Utilities\\DebugUtil.cs, palrt\inc\vsassert.h (sscli))</para>
        /// </summary>
        //------------------------------------------------------------
        static internal void VsFail(string message)
        {
            Assert(false, message);
        }

#if DEBUG2
        //------------------------------------------------------------
        // PrintBegin (1)
        //------------------------------------------------------------
        [Conditional("DEBUG2")]
        internal static void PrintBegin(string str)
        {
            Console.WriteLine("[begin] {0}", str);
        }

        //------------------------------------------------------------
        // PrintBegin (2)
        //------------------------------------------------------------
        [Conditional("DEBUG2")]
        internal static void PrintBegin(string str1, string str2)
        {
            Console.WriteLine("[begin] {0}: {1}", str1, str2);
        }

        //------------------------------------------------------------
        // PrintBegin (3)
        //------------------------------------------------------------
        [Conditional("DEBUG2")]
        internal static void PrintBegin(string str1, string str2, string str3)
        {
            Console.WriteLine("[begin] {0}: {1} {2}", str1, str2, str3);
        }

        //------------------------------------------------------------
        // PrintEnd (1)
        //------------------------------------------------------------
        [Conditional("DEBUG2")]
        internal static void PrintEnd(string str)
        {
            Console.WriteLine("[end] {0}", str);
        }

        //------------------------------------------------------------
        // PrintEnd (2)
        //------------------------------------------------------------
        [Conditional("DEBUG2")]
        internal static void PrintEnd(string str1, string str2)
        {
            Console.WriteLine("[end] {0}: {1}", str1, str2);
        }
#endif

        //------------------------------------------------------------
        // PrintEnd (3)
        //------------------------------------------------------------
        [Conditional("DEBUG2")]
        internal static void PrintEnd(string str1, string str2, string str3)
        {
#if DEBUG2
            Console.WriteLine("[end] {0}: {1} {2}", str1, str2, str3);
#endif
        }

        //------------------------------------------------------------
        // DEBUG_STOPWATCH
        //------------------------------------------------------------
#if DEBUG_STOPWATCH
        private static List<Stopwatch> stopwatchList = new List<Stopwatch>();
        private static List<string> stopwatchDescList = new List<string>();
#endif

        //------------------------------------------------------------
        // DebugUtil.CreateStopwatchs
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void CreateStopwatchs(int count)
        {
#if DEBUG_STOPWATCH
            if (count > 0)
            {
                while (stopwatchList.Count < count)
                {
                    stopwatchList.Add(new Stopwatch());
                }
                while (stopwatchDescList.Count < count)
                {
                    stopwatchDescList.Add(null);
                }
            }
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.StartStopwatch (1)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void StartStopwatch(int i, bool reset, string desc)
        {
#if DEBUG_STOPWATCH
            try
            {
                if (desc != null)
                {
                    stopwatchDescList[i] = desc;
                }
                if (reset)
                {
                    stopwatchList[i].Reset();
                }
                stopwatchList[i].Start();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.StartStopwatch (2)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void StartStopwatch(bool reset, string desc)
        {
#if DEBUG_STOPWATCH
            StartStopwatch(0, reset, desc);
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.StartStopwatch (3)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void StartStopwatch(bool reset)
        {
#if DEBUG_STOPWATCH
            StartStopwatch(0, reset, null);
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.StopStopwatch (1)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void StopStopwatch(int i, bool display)
        {
#if DEBUG_STOPWATCH
            try
            {
                stopwatchList[i].Stop();
                if (display)
                {
                    DisplayStopwatchTime(i);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.StopStopwatch (2)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void StopStopwatch(bool display)
        {
#if DEBUG_STOPWATCH
            StopStopwatch(0, display);
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.ResetStopwatch (1)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void ResetStopwatch(int i, string desc)
        {
#if DEBUG_STOPWATCH
            try
            {
                stopwatchList[i].Reset();
                if (desc != null)
                {
                    stopwatchDescList[i] = desc;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.ResetStopwatch (2)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void ResetStopwatch(int i)
        {
#if DEBUG_STOPWATCH
            ResetStopwatch(i, null);
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.ResetStopwatch (3)
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void ResetStopwatch()
        {
#if DEBUG_STOPWATCH
            ResetStopwatch(0, null);
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.GetStopwatchDesc
        //------------------------------------------------------------
#if DEBUG_STOPWATCH
        private static string GetStopwatchDesc(int i)
        {
            try
            {
                string desc= stopwatchDescList[i];
                if (desc != null)
                {
                    return desc;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            return i.ToString();
        }
#endif

        //------------------------------------------------------------
        // DebugUtil.DisplayStopwatchTime
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void DisplayStopwatchTime(int i)
        {
#if DEBUG_STOPWATCH
            try
            {
                Console.WriteLine("{0} : {1}",
                    GetStopwatchDesc(i),
                    stopwatchList[i].ElapsedMilliseconds);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.DisplayStopwatchTime
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void DisplayStopwatchTime()
        {
#if DEBUG_STOPWATCH
            DisplayStopwatchTime(0);
#endif
        }

        //------------------------------------------------------------
        // DebugUtil.DisplayAllStopwatchTimes
        //------------------------------------------------------------
        [Conditional("DEBUG_STOPWATCH")]
        internal static void DisplayAllStopwatchTimes()
        {
#if DEBUG_STOPWATCH
            for (int i = 0; i < stopwatchList.Count; ++i)
            {
                Stopwatch sw = null;
                if ((sw = stopwatchList[i]) != null)
                {
                    DisplayStopwatchTime(i);
                }
            }
#endif
        }
    }
}
