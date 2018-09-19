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

namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/TN")]
    public class TNController : Controller
    {
        public readonly HouseOfCards_Context _context;
        public TNController(HouseOfCards_Context context)
        {
            _context = context;
        }

        
       
        [HttpPost("Recarga_Celular")]
        public IActionResult CPA_TN(ReloadRequest xmlData)
        {

            // Se valida los Campos
            string x = ValidacionRecarga(xmlData);

            // Se Genera XML
            XmlSerializer xmls = new XmlSerializer(xmlData.GetType()); 
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlData);
            var sXML = sw.ToString();

            //Se genera XMLs en String (Escapado)
            sXML = @"<?xml version=""1.0"" encoding=""utf-8""?>" + sXML;
            sXML = ScapeXML(sXML);

            //Consulta credenciales en la BD 
            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/getReloadClass", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope("getReloadClass", sXML);


            webRequest.Timeout = 50000;

           

            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(100000);
            string soapResult;

            try
            {
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {

                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

          

            // Si no regresa nada o es codigo 6 o 71  o sin respuesta se realiza la consulta al WS
            //para verificar si se realizo la recarga 
            ReloadResponse ResponseXml = new ReloadResponse();
            ResponseXml = GetReloadResponse(soapResult, "getReloadClassResult");
          
            if (ResponseXml.ResponseCode == null || ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71" || ResponseXml.ResponseCode == "0")
            {
                //6 - No es posible establecer conexión 
                //71 - El switch no está contestando las solicitudes de abono
                ResponseXml.TC = xmlData.TC;
                ResponseXml.SKU = xmlData.SKU;
                CR_TN(ResponseXml);
                //Se realiza la consulta
                
                //Si regresa algunc odigo que no sea 0 entonces se muestra ticket 
                if (true)//Codigo 
                {

                }
            }
            

                //Se emprime Recibo 2 Veces
                string Ticket =
                    "NO. TRANSACCIÓN:  " + ResponseXml.TransNumber + Environment.NewLine +
                    "NO. AUTORIZACIÓN: " + ResponseXml.AutoNo + Environment.NewLine +
                    "MONTO:            " + "100" + Environment.NewLine +//Se consulta el SKU para traer el monto
                    "TELEFONO:         " + ResponseXml.PhoneNumber + Environment.NewLine +
                    Environment.NewLine +
                    "TELCEL" + Environment.NewLine +
                    "ESTIMADO CLIENTE EN CASO DE PRESENTARSE ALGUN PROBLEMA CON SU TIEMPO AIRE FAVOR DE" + Environment.NewLine +
                    "COMUNICARSE A ATENCION A CLIENTES TELCEL *264 DESDE SU TELCEL O DESDE EL INTERIOR DE LA REPUBLICA" + Environment.NewLine +
                    "AL 01800-710-5687" + Environment.NewLine +
                    Environment.NewLine +
                    "FECHA Y HORA DE LA TRANSACCIÓN: " + ResponseXml.DateTime + Environment.NewLine +
                    "TIENDA:" + Environment.NewLine +
                    "LO ATENDIO";






                return Content(Ticket);
            

        }

        public IActionResult CR_TN(ReloadResponse xmlReloadResponse)
        {
            QueryRequest xmlQueryRequest = new QueryRequest();
            xmlQueryRequest.ID_CHAIN = xmlReloadResponse.ID_CHAIN;
            xmlQueryRequest.ID_GRP = xmlReloadResponse.ID_GRP;
            xmlQueryRequest.ID_MERCHANT = xmlReloadResponse.ID_MERCHANT;
            xmlQueryRequest.ID_POS = xmlReloadResponse.ID_POS;
            xmlQueryRequest.PhoneNumber = xmlReloadResponse.PhoneNumber;
            xmlQueryRequest.SKU = xmlReloadResponse.SKU;
            xmlQueryRequest.TC = xmlReloadResponse.TC;
            xmlQueryRequest.TransNumber = xmlReloadResponse.TransNumber;
            xmlQueryRequest.DateTime = xmlReloadResponse.DateTime;

            XmlSerializer xmls = new XmlSerializer(xmlQueryRequest.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlQueryRequest);
            var sXML = sw.ToString();


            sXML = @"<?xml version=""1.0"" encoding=""utf-8""?>" + sXML;
            sXML = ScapeXML(sXML);

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
                return Content(soapResult);
            }
        }
















        //RECARGA DE DATOS
    
        [HttpPost("Recarga_Datos")]
        public IActionResult RTA_TN(DataRequest xmlData)
        {
            XmlSerializer xmls = new XmlSerializer(xmlData.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;         
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlData);
            var sXML = sw.ToString();

            sXML = @"<?xml version=""1.0"" encoding=""utf-8""?>"+ sXML;
            sXML = ScapeXML(sXML);

            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = CreateWebRequest(item.Url,"http://www.pagoexpress.com.mx/ServicePX/getReloadClass", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope("getReloadData",sXML);

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
                return Content(soapResult);
            }
        }

        
        [HttpPost("Consulta_Datos")]
        public IActionResult RPD_TN(DataQueryRequest xmlData)
        {
            XmlSerializer xmls = new XmlSerializer(xmlData.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlData);
            var sXML = sw.ToString();

            sXML = @"<?xml version=""1.0"" encoding=""utf-8""?>" + sXML;
            sXML = ScapeXML(sXML);

            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/getReloadClass", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope("getQueryDatClass", sXML);

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
                return Content(soapResult);
            }
        }






        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public ReloadResponse GetReloadResponse(string xml,string path)
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
        
        public string ValidacionRecarga(ReloadRequest xml)
        {
            int v;
            if (!(int.TryParse(xml.ID_GRP, out v))) { return "ID_GRP No es Entero"; }
            if (xml.ID_GRP.ToString().Length > 4) { return "ID_GRP lenght es mayor a 4"; }

           
            if (!(int.TryParse(xml.ID_CHAIN, out v))) { return "ID_CHAIN No es Entero"; }
            if (xml.ID_CHAIN.ToString().Length > 4) { return "ID_CHAIN lenght es mayor a 4"; }

           
            if (!(int.TryParse(xml.ID_MERCHANT, out v))) { return "ID_MERCHANT No es Entero"; }
            if (xml.ID_MERCHANT.ToString().Length >= 4) { return "ID_MERCHANT lenght es mayor a 4"; }

            
            if (!(int.TryParse(xml.ID_POS, out v))) { return "ID_POS No es Entero"; }
            if (xml.ID_POS.ToString().Length > 4) { return "ID_POS lenght es mayor a 4"; }

 
            if (!(xml.DateTime.ToString().Length > 19)) { return "DateTime lenght es mayor a 19"; }
            DateTime temp;
            if (DateTime.TryParse(xml.DateTime, out temp)){ return "El formato de Date no es correcto"; }


            if (xml.SKU.ToString().Length > 13) { return "SKU lenght es mayor a 4"; }


            if (xml.PhoneNumber.ToString().Length > 10) { return "PhoneNumber lenght es mayor a 4"; }


            if (!(int.TryParse(xml.TransNumber, out v))) { return "TransNumber  No es Entero"; }
            if (xml.TransNumber.ToString().Length > 5) { return "TransNumber lenght es mayor a 4"; }


            if (!(int.TryParse(xml.TC, out v))) { return "TC  No es Entero"; }
            if (xml.TC.ToString().Length > 1) { return "TC lenght es mayor a 4"; }

            return "Validacion de Campos Correcta";
        }




        public static string TN_Cod(TN_COD cod)
        {
            switch (cod) {
                
                case TN_COD.Cod_0:
                    return "NO FUE POSIBLE REGISTRAR LA TERMINAL";
                case TN_COD.Cod_5:
                    return "SWITCH NO DISPONIBLE";
                case TN_COD.Cod_6:
                    return "SERVICIO NO DISPONIBLE";
                case TN_COD.Cod_8:
                    return "SERVIDOR DE CARRIER ABAJO, INTENTE MAS TARDE";
             
                        }
            return "";
        
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
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

    }
    public enum TN_COD
    {
        Cod_0 = 0,
        Cod_1 = 1,
        Cod_2 = 2,
        Cod_3 = 3,
        Cod_4 = 4,
        Cod_5 = 5,
        Cod_6 = 6,
        Cod_7 = 7,
        Cod_8 = 8,
        Cod_9 = 9,
        Cod_10 =10 

    }

}
