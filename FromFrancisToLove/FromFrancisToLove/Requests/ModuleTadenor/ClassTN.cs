using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FromFrancisToLove.Data;
using System.Net;
using System.Xml;
using FromFrancisToLove.Requests;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace FromFrancisToLove.Requests.ModuleTadenor
{
    public class ClassTN
    {


        public string GetXMLs(object xmlData, string x)
        {
            XmlSerializer xmls = new XmlSerializer(xmlData.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlData);
            string y = sw.ToString().Replace("MyRelReq", x);
            return ScapeXML(@"<?xml version=""1.0"" encoding=""utf-8""?>" + y);
        }

        public MyRelReq GetRespuesta(string xml, string path, string response)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(path);

            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xml = xml.Replace(response, "MyRelReq");
            xmldoc.LoadXml(Des_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            XmlSerializer xmls = new XmlSerializer(typeof(MyRelReq));

            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);

            StreamReader sr = new StreamReader(stream);
            MyRelReq rr = (MyRelReq)xmls.Deserialize(sr);
            return rr;
        }

        public string ReloadValidation(MyRelReq xml)
        {

            if (xml.PhoneNumber.ToString().Length > 10) { return "PhoneNumber lenght es mayor a 10"; }

            return null;
        }

        public  HttpWebRequest CreateWebRequest(string url, string action, string Usr, string Pwd)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = new System.Net.NetworkCredential(Usr, Pwd);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            webRequest.Method = "POST";
            return webRequest;
        }

        public  string ScapeXML(string sXML)
        {
            sXML = sXML.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            return sXML;
        }

        public  string Des_ScapeXML(string sXML)
        {
            sXML = sXML.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
            return sXML;
        }

        public  XmlDocument CreateSoapEnvelope(string servicio, string sXML)
        {
            string xml =
             @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">" +
             "<soap:Body>" +
             "<" + servicio + @" xmlns=""http://www.pagoexpress.com.mx/ServicePX"">" +
             @"<sXML>" + sXML + "</sXML>" +
             "</" + servicio + ">" +
             "</soap:Body>" +
             "</soap:Envelope>";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(xml);
            return soapEnvelopeDocument;
        }

        public  void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            Stream stream = webRequest.GetRequestStream();
            soapEnvelopeXml.Save(stream);
        }

    }

    public static class EnumPrueba
    {
        public static string TN_Cod(string x)
        {
            switch (Int32.Parse(x))
            {

                case 2: return "NO FUE POSIBLE REGISTRAR LA TERMINALS";
                case 5: return "SWITCH NO DISPONIBLE";
                case 6: case 70: return "Servicio no disponible";
                case 8: return "SERVIDOR DE CARRIER ABAJO, INTENTE MAS TARDE";
                case 19: return "EL NUMERO DE TRANSACCION DE LA CAJA ES INVALIDO, DEBE SER MAYOR QUE 0";
                case 30: return "ERROR DE FORMATO EN EL MENSAJE";
                case 34: return "NUMERO DE TELÉFONO INVALIDO";
                case 35: return "Su terminal ha sido bloqueada por exceder el monto permitido";
                case 66: case 67: return "EN ESTOS MOMENTOS NO CUENTAS CON CREDITO DISPONIBLE";
                case 71: return "NO HAY RESPUESTA DEL SWITCH";
                case 72: return "NO HAY RESPUESTA DEL CARRIER";
                case 87: return "TELEFONO NO SUCEPTIBLE DE ABONO";
                case 88: return "MONTO INVALIDO";
                case 65:
                case 91:
                case 92:
                case 93:
                case 96:
                case 98:
                case 99: return "MANTENIMIENTO EN PROGRESO, INTENTE MAS TARDE";
            }
            return "";
        }

    }
}
