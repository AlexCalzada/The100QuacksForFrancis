﻿using System;
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

namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/TN")]
    public class TadenorController : Controller
    {
        
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
       
                return Ok("Skus");
            }
            catch (Exception)
            {
                return Ok($"Error 500 -- Internal Server Error");
            }
        }

        public readonly HouseOfCards_Context _context;
        public TadenorController(HouseOfCards_Context context)
        {
            _context = context;
        }

        [HttpPost("Recarga_Celular")]
        public IActionResult _TN(ReloadRequest xmlData)
        {
            if (ReloadValidation(xmlData) != null) // Se valida los Campos
            {
                return Content(ReloadValidation(xmlData));
            }
            var sXML = GetXMLs(xmlData);// Se Genera <XMLs> como String Escapado 
            var item = _context.conexion_Configs.Find(2);

            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/getReloadClass", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope("getReloadClass", sXML);
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            string soapResult;

            try
            {
                WebResponse webResponse = webRequest.EndGetResponse(asyncResult);

                StreamReader rd = new StreamReader(webResponse.GetResponseStream());
                soapResult = rd.ReadToEnd();
            }
            catch (Exception) { throw; }


            ReloadResponse ResponseXml = new ReloadResponse();
            ResponseXml = GetResponse(soapResult, "getReloadClassResult");

            if (ResponseXml.ResponseCode == null || ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71")
            {
                ResponseXml.TC = xmlData.TC;
                ResponseXml.SKU = xmlData.SKU;
                string x = EnumPrueba.TN_Cod(CR_TN(ResponseXml));
                if (x != "")
                {
                    return Content(x);
                }
            }
            if (ResponseXml.ResponseCode != "0")
            {
                return Content(ResponseXml.DescripcionCode);
            }

            string Ticket =  
            "NO. TRANSACCIÓN:  " + ResponseXml.TransNumber + Environment.NewLine +
            "NO. AUTORIZACIÓN: " + ResponseXml.AutoNo + Environment.NewLine +
            "MONTO:            " + "100" + Environment.NewLine +//Se consulta el SKU para traer el monto!!!!!!!!!!!!!
            "TELEFONO:         " + ResponseXml.PhoneNumber + Environment.NewLine +
            Environment.NewLine +
            "TELCEL" + Environment.NewLine +
            "ESTIMADO CLIENTE EN CASO DE PRESENTARSE ALGUN PROBLEMA CON SU TIEMPO AIRE FAVOR DE" + Environment.NewLine +
            "COMUNICARSE A ATENCION A CLIENTES TELCEL *264 DESDE SU TELCEL O DESDE EL INTERIOR DE LA REPUBLICA" + Environment.NewLine +
            "AL 01800-710-5687" + Environment.NewLine +
            Environment.NewLine +
            "FECHA Y HORA DE LA TRANSACCIÓN: " + ResponseXml.Datetime + Environment.NewLine +
            "TIENDA:" + Environment.NewLine +
            ResponseXml.ResponseCode + ":" +
            ResponseXml.DescripcionCode;

                return Content(Ticket);
            
            
        }

        public string CR_TN(ReloadResponse xmlReloadResponse)
        {
            QueryRequest xmlQueryRequest = new QueryRequest();

            xmlQueryRequest.PhoneNumber = xmlReloadResponse.PhoneNumber;
            xmlQueryRequest.SKU = xmlReloadResponse.SKU;
            xmlQueryRequest.TC = xmlReloadResponse.TC;
            xmlQueryRequest.TransNumber = xmlReloadResponse.TransNumber;

            var sXML = GetXMLs(xmlQueryRequest);

            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/getQueryClass", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope("getQueryClass", sXML);

            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            string soapResult;
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
            }

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(soapResult);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName("getQueryClassResult");

            string xml="";
            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xmldoc.LoadXml(Des_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            string x = string.Empty;
            foreach (XmlNode node in nodeList)
            {
                x = node.InnerText;
            }
            return x;    
        }



        [HttpPost("Recarga_Datos")]
        public IActionResult RD_TN(DataRequest xmlData)
        {
            if (DataValidation(xmlData) != null) // Se valida los Campos
            {
                return Content(DataValidation(xmlData));
            }
            var sXML = GetXMLs(xmlData);// Se Genera <XMLs> como String Escapado 
            var item = _context.conexion_Configs.Find(2);

            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/getReloadData", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope("getReloadData", sXML);
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            string soapResult;

            try
            {
                WebResponse webResponse = webRequest.EndGetResponse(asyncResult);

                StreamReader rd = new StreamReader(webResponse.GetResponseStream());
                soapResult = rd.ReadToEnd();
            }
            catch (Exception) { throw; }


            ReloadResponse ResponseXml = new ReloadResponse();
            ResponseXml = GetDataResponse(soapResult, "getReloadDataResult");

            if (ResponseXml.ResponseCode == null || ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71")
            {
                ResponseXml.TC = xmlData.TC;
                ResponseXml.SKU = xmlData.SKU;
                ResponseXml.ID_Product = xmlData.ID_Product;
                string x = EnumPrueba.TN_Cod(CD_TN(ResponseXml));
                if (x != "")
                {
                    return Content(x);
                }
            }
            //if (ResponseXml.ResponseCode != "0")
            //{
            //    return Content(ResponseXml.DescripcionCode);
            //}

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ResponseXml.ToString());

            string json = JsonConvert.SerializeXmlNode(doc);

            return Content(json);
        }

        public string CD_TN(ReloadResponse xmlReloadResponse)
        {
            QueryRequest xmlQueryRequest = new QueryRequest();

            xmlQueryRequest.PhoneNumber = xmlReloadResponse.PhoneNumber;
            xmlQueryRequest.SKU = xmlReloadResponse.SKU;
            xmlQueryRequest.TC = xmlReloadResponse.TC;
            xmlQueryRequest.TransNumber = xmlReloadResponse.TransNumber;

            var sXML = GetXMLs(xmlQueryRequest);

            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/getQueryClass", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope("getQueryClass", sXML);

            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            string soapResult;
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
            }

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(soapResult);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName("getQueryClassResult");

            string xml = "";
            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xmldoc.LoadXml(Des_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            string x = string.Empty;
            foreach (XmlNode node in nodeList)
            {
                x = node.InnerText;
            }
            return x;
        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public string GetXMLs(object xmlData)
        {
            XmlSerializer xmls = new XmlSerializer(xmlData.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlData);

            return ScapeXML(@"<?xml version=""1.0"" encoding=""utf-8""?>" +sw.ToString());
        }

        public ReloadResponse GetResponse(string xml,string path)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(path);

            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xmldoc.LoadXml(Des_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            XmlSerializer xmls = new XmlSerializer(typeof(ReloadResponse));

            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);

            StreamReader sr = new StreamReader(stream);
            ReloadResponse rr = (ReloadResponse)xmls.Deserialize(sr);
            return rr;
        }

        public DataResponse GetDataResponse(string xml, string path)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(path);

            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xmldoc.LoadXml(Des_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            XmlSerializer xmls = new XmlSerializer(typeof(DataResponse));

            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);

            StreamReader sr = new StreamReader(stream);
            DataResponse rr = (DataResponse)xmls.Deserialize(sr);
            return rr;
        }

        public string ReloadValidation(ReloadRequest xml)
        {
            int v;

            if (xml.SKU.ToString().Length > 13) { return "SKU lenght es mayor a 13"; }


            if (xml.PhoneNumber.ToString().Length > 10) { return "PhoneNumber lenght es mayor a 10"; }


            if (!(int.TryParse(xml.TransNumber, out v))) { return "TransNumber  No es Entero"; }
            if (xml.TransNumber.ToString().Length > 5) { return "TransNumber lenght es mayor a 5"; }


            if (!(int.TryParse(xml.TC, out v))) { return "TC  No es Entero"; }
            if (xml.TC.ToString().Length > 1) { return "TC lenght es mayor a 1"; }

            return null;
        }

        public string DataValidation(DataRequest xml)
        {
            int v;
            if (xml.ID_Product.ToString().Length > 6) { return "Caracteres de id Porducto es superior a 6 "; }


            if (xml.SKU.ToString().Length > 13) { return "SKU lenght es mayor a 13"; }


            if (xml.PhoneNumber.ToString().Length > 10) { return "PhoneNumber lenght es mayor a 10"; }


            if (!(int.TryParse(xml.TransNumber, out v))) { return "TransNumber  No es Entero"; }
            if (xml.TransNumber.ToString().Length > 5) { return "TransNumber lenght es mayor a 5"; }


            if (!(int.TryParse(xml.TC, out v))) { return "TC  No es Entero"; }
            if (xml.TC.ToString().Length > 1) { return "TC lenght es mayor a 1"; }

            return null;
        }

        private static HttpWebRequest CreateWebRequest(string url, string action, string Usr, string Pwd)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = new System.Net.NetworkCredential(Usr, Pwd);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static string ScapeXML(string sXML)
        {
            sXML = sXML.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            return sXML;
        }

        public static string Des_ScapeXML(string sXML)
        {
            sXML = sXML.Replace("&amp;", "&").Replace( "&lt;", "<").Replace( "&gt;",">").Replace( "&quot;", "\"").Replace("&apos;", "'");
            return sXML;
        }

        private static XmlDocument CreateSoapEnvelope(string servicio,string sXML)
        {
           string xml=
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

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
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

                case 2:  return "NO FUE POSIBLE REGISTRAR LA TERMINALS";
                case 5:  return "SWITCH NO DISPONIBLE";
                case 6: case 70: return "Servicio no disponible";
                case 8:  return "SERVIDOR DE CARRIER ABAJO, INTENTE MAS TARDE";
                case 19: return "EL NUMERO DE TRANSACCION DE LA CAJA ES INVALIDO, DEBE SER MAYOR QUE 0";
                case 30: return "ERROR DE FORMATO EN EL MENSAJE";
                case 34: return "NUMERO DE TELÉFONO INVALIDO";
                case 35: return "Su terminal ha sido bloqueada por exceder el monto permitido";
                case 66:  case 67: return "EN ESTOS MOMENTOS NO CUENTAS CON CREDITO DISPONIBLE";
                case 71: return "NO HAY RESPUESTA DEL SWITCH";
                case 72: return "NO HAY RESPUESTA DEL CARRIER";
                case 87: return "TELEFONO NO SUCEPTIBLE DE ABONO";
                case 88: return "MONTO INVALIDO";
                case 65: case 91: case 92: case 93: case 96:  case 98: 
                case 99: return "MANTENIMIENTO EN PROGRESO, INTENTE MAS TARDE";
            }
            return "";
        }

    }
}
