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
using System.Threading;

namespace FromFrancisToLove.Requests.ModuleTadenor
{
    public class ClassTN
    {

        public string Send_Request(string servicio, MyRelReq xmlQuery, string[] credencial)
        {
            // Crea el xml de acuerdo  al procesos, ya sea  Recarga o consulta
            var sXML = Get_XMLs(xmlQuery, servicio);

            HttpWebRequest webRequest = CreateWebRequest(credencial[0], "http://www.pagoexpress.com.mx/ServicePX/" +servicio, credencial[1], credencial[2]);

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(servicio, sXML);
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            string soapResult = "";
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
                //Thread.Sleep(20000);
            }
            return soapResult;
        }

        public MyRelReq Deserializer_Response(string xml, string path, string response)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(path);

            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xml = xml.Replace(response, "MyRelReq");
            xmldoc.LoadXml(Un_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            XmlSerializer xmls = new XmlSerializer(typeof(MyRelReq));

            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);

            StreamReader sr = new StreamReader(stream);
            MyRelReq modelo = (MyRelReq)xmls.Deserialize(sr);
            return modelo;
        }
        //Deserealiza el xml que sera enviado Saldo/Datos
        private string Get_XMLs(object xmlData, string x)
        {

            XmlSerializer xmls = new XmlSerializer(xmlData.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlData);
            string xml = sw.ToString().Replace("MyRelReq", x);
            return ScapeXML(@"<?xml version=""1.0"" encoding=""utf-8""?>" + xml);
        }
        //Serializa en Objeto el xml de respuesta

        private string ReloadValidation(MyRelReq xml)
        {

            if (xml.PhoneNumber.ToString().Length > 10) { return "PhoneNumber lenght es mayor a 10"; }

            return null;
        }

        private XmlDocument CreateSoapEnvelope(string servicio, string sXML)
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

        private void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            Stream stream = webRequest.GetRequestStream();
            soapEnvelopeXml.Save(stream);
        }

        private HttpWebRequest CreateWebRequest(string url, string action, string Usr, string Pwd)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = new System.Net.NetworkCredential(Usr, Pwd);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            webRequest.Method = "POST";
            return webRequest;
        }
        // Se encuentran en otra clase----------------------------------------------------------
        private string ScapeXML(string sXML)
        {
            sXML = sXML.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            return sXML;
        }

        private string Un_ScapeXML(string sXML)
        {
            sXML = sXML.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
            return sXML;
        }
        //----------------------------------------------------------

        public string jString(MyRelReq ResponseXml)
        {
            string jsonFormat =
        "[" +
        "{\"sCampo\":\"" + nameof(ResponseXml.ID_GRP) + "\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.ID_GRP + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"ID_CHAIN\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.ID_CHAIN + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"ID_MERCHANT\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.ID_MERCHANT + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"ID_POS\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.ID_POS + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"DateTime\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.Datetime + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"PhoneNumber\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.PhoneNumber + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"TransNumber\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.TransNumber + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"ID_Product\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.ID_Product + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"Brand\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.Brand + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"Instr1\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.Instr1 + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"Instr2\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.Instr2 + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"AutoNo\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.AutoNo + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"ResponseCode\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.ResponseCode + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"DescripcionCode\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.DescripcionCode + "\", \"bEncriptado\":false}," +
        "{\"sCampo\":\"Monto\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + ResponseXml.Monto + "\", \"bEncriptado\":false}" +
        "]";
            return jsonFormat;
        }

    }


}
