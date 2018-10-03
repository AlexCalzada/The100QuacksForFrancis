using Diestel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel.Clases
{
    public class CancelLog
    {
        private string sFilePrefix;
        private string sFileExt;
        private string sFilePath;
        private LOGFrecuency LFrecuency;

        public CancelLog()
        {
            sFilePrefix = "PX_Reversos_";
            sFilePath = @"C:\ApiService\Client\";
            sFileExt = "txt";
            LFrecuency = LOGFrecuency.Daily;
        }

        public CancelLog(string LOGFilePrefix, string LOGFileExtension, string LOGFilePath, LOGFrecuency LOGFrecuencyFormat)
        {
            sFilePrefix = LOGFilePrefix;
            sFilePath = LOGFilePath;
            sFileExt = LOGFileExtension;
            LFrecuency = LOGFrecuencyFormat;
        }

        public bool WriteCancelLog(cCampo [] campo, int NumberofTry, string ResponseCode, string ResponseDescription)
        {
            string sLineToWrite;
            int I;
            string sDatePart;

            if (ResponseCode == "0")
            {
                sLineToWrite = "[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + "] REVERSO EXISTOSO EN INTENTO # " + NumberofTry + Environment.NewLine +
                               "CON CODIGO DE RESPUESTA: (" + ResponseCode + ") " + ResponseDescription + Environment.NewLine +
                               "DATOS DE LA TRANSACCION: "; 
            }
            else
            {
                sLineToWrite = "[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + "] REVERSO FALLIDO EN INTENTO # " + NumberofTry + Environment.NewLine +
                               "CON CODIGO DE RESPUESTA: (" + ResponseCode + ") " + ResponseDescription + Environment.NewLine +
                               "DATOS DE LA TRANSACCION: ";
            }

            try
            {
                for (I = 0; I < campo.GetUpperBound(0); I++)
                {
                    if (I == campo.GetUpperBound(0))
                    {
                        sLineToWrite = sLineToWrite + campo[I].sCampo + "=" + campo[I].sValor + Environment.NewLine; 
                    }
                    else
                    {
                        sLineToWrite = sLineToWrite + campo[I].sCampo + "=" + campo[I].sValor + "|";
                    }
                }

                if (LFrecuency == LOGFrecuency.Daily)
                {
                    sDatePart = DateTime.Now.ToString("yyyy/MM/dd");
                }
                else
                {
                    sDatePart = DateTime.Now.ToString("yyyy/MM");
                }

                if (LeftMidRight.Left(sFilePath, 1) != @"\")
                {
                    sFilePath += @"\";
                }

                if (!System.IO.Directory.Exists(sFilePath))
                {
                    System.IO.Directory.CreateDirectory(sFilePath);
                }

                var file = new System.IO.StreamWriter(sFilePath + sFilePrefix + sDatePart + "." + sFileExt, true);
                file.WriteLine(sLineToWrite);
                file.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
        }

        public bool WriteTXData(string ResponseCode, string ResponseDescription, string Tx)
        {
            string sLineToWrite;
            string sDatePart;

            sLineToWrite = "[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + "] (" + ResponseCode + ":" + ResponseDescription + "/ PARA LA TRANSACCION # " + Tx + ")";
            try
            {
                if (LFrecuency == LOGFrecuency.Daily)
                {
                    sDatePart = DateTime.Now.ToString("yyyy/MM/dd");
                }
                else
                {
                    sDatePart = DateTime.Now.ToString("yyyy/MM");
                }

                if (LeftMidRight.Left(sFilePath, 1) != @"\")
                {
                    sFilePath += @"\";
                }

                if (!System.IO.Directory.Exists(sFilePath))
                {
                    System.IO.Directory.CreateDirectory(sFilePath);
                }

                var file = new System.IO.StreamWriter(sFilePath + sFilePrefix + sDatePart + "." + sFileExt, true);
                file.WriteLine(sLineToWrite);
                file.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
        }

    }

    public enum LOGFrecuency
    {
        Daily = 1,
        Monthly = 2
    }

    public class LeftMidRight
    {
        public LeftMidRight() { }

        public static string Left(string param, int length)
        {
            string result = param.Substring(0, length);
            return result;
        }

        public static string Mid(string param, int startIndex, int length)
        {
            string result = param.Substring(startIndex, length);
            return result;
        }

        public static string Mid(string param, int startIndex)
        {
            string result = param.Substring(startIndex);
            return result;
        }

        public static string Right(string param, int length)
        {
            int value = param.Length - length;
            string result = param.Substring(value, length);
            return result;
        }
    }
}
