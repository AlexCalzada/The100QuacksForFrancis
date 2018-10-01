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

namespace FromFrancisToLove.Controllers
{
    [Produces("application/json")]
    [Route("api/TN")]
    public class TadenorController : Controller
    {
        public readonly HouseOfCards_Context _context;
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.catalogos_Productos);
        }
        public TadenorController(HouseOfCards_Context context)
        {
            _context = context;
        }
        [HttpPost("Recarga_Servicio")] 

        public IActionResult TN_Service(MyRelReq xmlData)
        {
            
            var service = "getReloadClass";
            var response = "ReloadResponse";
            //Determinamos el tipo de servicio Saldo o Datos
            //catalogos_Producto
            var producto = _context.catalogos_Productos.First(a => a.SKU == xmlData.SKU && a.CONFIGID == 2);
            if (producto.IDProduct != "")
            {
              service = "getReloadData";
              response = "DataResponse";
            }


            var sXML = GetXMLs(xmlData, service);// Se Genera <XMLs> como String Escapado 

           
            
            var Credentials = _context.conexion_Configs.Find(2);

            HttpWebRequest webRequest = CreateWebRequest(Credentials.Url, "http://www.pagoexpress.com.mx/ServicePX/" + service, Credentials.Usr, Credentials.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(service, sXML);
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

            MyRelReq ResponseXml = new MyRelReq();
            ResponseXml = GetRespuesta(soapResult, service + "Result", response);

            //Null 0 o 16 
            if (ResponseXml.ResponseCode == null || ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71")
            {
                ResponseXml.TC = xmlData.TC;
                ResponseXml.SKU = xmlData.SKU;
                ResponseXml.ID_Product = xmlData.ID_Product;
                string x = EnumPrueba.TN_Cod(CR_TN_SERV(ResponseXml));
                if (x != "")
                {
                    return Content(x);
                }
            }
            if (ResponseXml.ResponseCode != "0")
            {
                return Content(ResponseXml.DescripcionCode);
            }

            //Tiket
            try
            {

                string Ticket =
           "///////////////////////////////////////////////////////////////////////////////////////////////////////////" + Environment.NewLine +
           "NO. TRANSACCIÓN:  " + ResponseXml.TransNumber + Environment.NewLine +
           "NO. AUTORIZACIÓN: " + ResponseXml.AutoNo + Environment.NewLine +
           "MONTO:            " + producto.Monto + Environment.NewLine +//Se consulta el SKU para traer el monto!!!!!!!!!!!!!
           "TELEFONO:         " + ResponseXml.PhoneNumber + Environment.NewLine +
           "///////////////////////////////////////////////////////////////////////////////////////////////////////////" + Environment.NewLine +
           "TELCEL" + Environment.NewLine +
           "ESTIMADO CLIENTE EN CASO DE PRESENTARSE ALGUN PROBLEMA CON SU TIEMPO AIRE FAVOR DE" + Environment.NewLine +
           "COMUNICARSE A ATENCION A CLIENTES TELCEL *264 DESDE SU TELCEL O DESDE EL INTERIOR DE LA REPUBLICA" + Environment.NewLine +
           "AL 01800-710-5687" + Environment.NewLine +
           "///////////////////////////////////////////////////////////////////////////////////////////////////////////" + Environment.NewLine +
           "FECHA Y HORA DE LA TRANSACCIÓN: " + ResponseXml.Datetime + Environment.NewLine +
           "TIENDA:" + Environment.NewLine +
           ResponseXml.ResponseCode + ":" +
           ResponseXml.DescripcionCode + Environment.NewLine +
           "///////////////////////////////////////////////////////////////////////////////////////////////////////////";



                return Content(Ticket);

            }
            catch (Exception)
            {

                throw;
            }
        }
        public string CR_TN_SERV(MyRelReq xmlReloadResponse)
        {
            MyRelReq xmlQueryRequest = new MyRelReq();

            xmlQueryRequest.PhoneNumber = xmlReloadResponse.PhoneNumber;
            xmlQueryRequest.SKU = xmlReloadResponse.SKU;
            xmlQueryRequest.TC = xmlReloadResponse.TC;
            xmlQueryRequest.TransNumber = xmlReloadResponse.TransNumber;

            var servicio = "getQueryClass";
           
            if (xmlReloadResponse.ID_Product!="")
            {
                xmlQueryRequest.ID_Product = xmlReloadResponse.ID_Product;
                servicio = "getQueryDatClass";
            }
          

            var sXML = GetXMLs(xmlQueryRequest, servicio);

            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/" + servicio, item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(servicio, sXML);

            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            string soapResult;

            WebResponse webResponse = webRequest.EndGetResponse(asyncResult);
            StreamReader rd = new StreamReader(webResponse.GetResponseStream());
            soapResult = rd.ReadToEnd();


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(soapResult);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(servicio + "Result");

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
            int v;

            if (xml.PhoneNumber.ToString().Length > 10) { return "PhoneNumber lenght es mayor a 10"; }

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
            sXML = sXML.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
            return sXML;
        }

        private static XmlDocument CreateSoapEnvelope(string servicio, string sXML)
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