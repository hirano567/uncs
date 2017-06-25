#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace temp
{
    public static class CErrorProcessing
    {
        //------------------------------------------------------------
        // CErrorProcessing.MakeCSCErrorInfos
        //------------------------------------------------------------
        internal static void MakeCSCErrorInfos()
        {
            int dummy = Uncs.CSCErrorInfo.Manager.GetErrorNumber(Uncs.CSCERRID.FTL_InternalError);
            Exception excp = null;

            Dictionary<Uncs.CSCERRID, Uncs.BCErrorInfoManager<Uncs.CSCERRID>.ERRORINFO> infoDic
                = Uncs.CSCErrorInfo.Manager.ErrorInfoDic;

            Uncs.BCErrorInfoManager<Uncs.CSCERRID>.ERRORINFO errInfo = null;

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            string indent1 = "\t\t\t";
            string indent2 = indent1 + "\t";
            string indent3 = indent2 + "\t";

            foreach (KeyValuePair<Uncs.CSCERRID, Uncs.BCErrorInfoManager<Uncs.CSCERRID>.ERRORINFO> kv
                in infoDic)
            {
                errInfo = kv.Value;
                Uncs.CSCERRID errID = errInfo.ErrorID;
                int errNo = errInfo.ErrorNumber;
                int warnLevel = errInfo.WarningLevel;
                Uncs.ResNo resNo = errInfo.ResourceNumber;

                string resStr = null;
                int carg2 = 0;
                if (Uncs.CResources.GetString(resNo, out resStr, out carg2, out excp))
                {
                    //
                }
                else
                {
                    sb2.AppendFormat("{0}: failed to get the resource.\n", resNo);
                }

                sb1.AppendFormat("{0}ErrorInfoDic.Add(\n", indent1);
                sb1.AppendFormat("{0}CSCERRID.{1},\n", indent2, errID);
                sb1.AppendFormat("{0}new ERRORINFO(\n", indent2);
                sb1.AppendFormat("{0}CSCERRID.{1},\n", indent3, errID);
                sb1.AppendFormat("{0}{1},\n", indent3, errNo);
                sb1.AppendFormat("{0}{1},\n", indent3, warnLevel);
                sb1.AppendFormat("{0}\"{1}\"));\n", indent3, resNo);
            }

            string output = sb1.ToString();
            string errlog = sb2.ToString();
        }

        //------------------------------------------------------------
        // CErrorProcessing.MakeALErrorInfos
        //------------------------------------------------------------
        internal static void MakeALErrorInfos()
        {
            Dictionary<Uncs.ALERRID, Uncs.BCErrorInfoManager<Uncs.ALERRID>.ERRORINFO> infoDic
                = Uncs.ALErrorInfo.Manager.ErrorInfoDic;

            Uncs.BCErrorInfoManager<Uncs.ALERRID>.ERRORINFO errInfo = null;

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            string indent1 = "\t\t\t";
            string indent2 = indent1 + "\t";
            string indent3 = indent2 + "\t";
            Exception excp = null;

            foreach (KeyValuePair<Uncs.ALERRID, Uncs.BCErrorInfoManager<Uncs.ALERRID>.ERRORINFO> kv
                in infoDic)
            {
                errInfo = kv.Value;
                Uncs.ALERRID errID = errInfo.ErrorID;
                int errNo = errInfo.ErrorNumber;
                int warnLevel = errInfo.WarningLevel;
                Uncs.ResNo resNo = errInfo.ResourceNumber;

                string resStr = null;
                int carg2 = 0;
                if (Uncs.CResources.GetString(resNo, out resStr, out carg2, out excp))
                {
                    //
                }
                else
                {
                    sb2.AppendFormat("{0}: failed to get the resource.\n", resNo);
                }

                sb1.AppendFormat("{0}ErrorInfoDic.Add(\n", indent1);
                sb1.AppendFormat("{0}ALERRID.{1},\n", indent2, errID);
                sb1.AppendFormat("{0}new ERRORINFO(\n", indent2);
                sb1.AppendFormat("{0}ALERRID.{1},\n", indent3, errID);
                sb1.AppendFormat("{0}{1},\n", indent3, errNo);
                sb1.AppendFormat("{0}{1},\n", indent3, warnLevel);
                sb1.AppendFormat("{0}\"{1}\"));\n", indent3, resNo);
            }

            string output = sb1.ToString();
            string errlog = sb2.ToString();
        }

        //------------------------------------------------------------
        // CErrorProcessing.Temp01
        //------------------------------------------------------------
        internal static void Temp01()
        {
            Uncs.CSCERRID[] warnNumbers =
            {
                Uncs.CSCERRID.WRN_WarningDirective,
                Uncs.CSCERRID.WRN_IllegalPragma,
                Uncs.CSCERRID.WRN_IllegalPPWarning,
                Uncs.CSCERRID.WRN_BadRestoreNumber,
                Uncs.CSCERRID.WRN_NonECMAFeature,
                Uncs.CSCERRID.WRN_BadWarningNumber,
                Uncs.CSCERRID.WRN_InvalidNumber,
                Uncs.CSCERRID.WRN_FileNameTooLong,
                Uncs.CSCERRID.WRN_IllegalPPChecksum,
                Uncs.CSCERRID.WRN_EndOfPPLineExpected,
                Uncs.CSCERRID.WRN_EmptyFileName,

                Uncs.CSCERRID.FTL_InternalError,

                Uncs.CSCERRID.WRN_LowercaseEllSuffix,
                Uncs.CSCERRID.WRN_PossibleMistakenNullStatement,
                Uncs.CSCERRID.WRN_EmptySwitch,
                Uncs.CSCERRID.WRN_NonECMAFeature,
                Uncs.CSCERRID.WRN_GlobalAliasDefn,
                Uncs.CSCERRID.WRN_AttributeLocationOnBadDeclaration,
                Uncs.CSCERRID.WRN_InvalidAttributeLocation,
            };

            StringBuilder sb = new StringBuilder();

            foreach (Uncs.CSCERRID id in warnNumbers)
            {
                int no = Uncs.CSCErrorInfo.Manager.GetErrorNumber(id);
                sb.AppendFormat("{0},// CSCERRID.{1}\n", no, id);
            }

            string str = sb.ToString();
        }
    }
}
#endif
