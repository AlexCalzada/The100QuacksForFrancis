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
        // GET: api/TN
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "" };
        }

        // GET: api/TN/5
        [HttpPost("Consulta_Tiempo_Aire")]
        public string CTA_TN(ReloadRequest xmlData)
        {
            XmlSerializer xmlPrueba = new XmlSerializer(xmlData.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmlPrueba.Serialize(writer, xmlData, ns);

            var xmls = sw.ToString();

            string x =
                  "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
  "<soap:Envelope xmlns:xsi =\"http://www.w3.org/2001/XMLSchema-instance\"" +
  "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"" +
  "xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +

    "<soap:Body>" +

      "<getReloadData xmlns =\"http://www.pagoexpress.com.mx/ServicePX\">" +
             "<sXML>" + xmls + "</sXML>" +

           "</getReloadData>" +

         "</soap:Body>" +
        "</soap:Envelope>";


             return xmls;


            


        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private static HttpWebRequest CreateWebRequest(string url, string action, string Usr, string Pwd)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = new System.Net.NetworkCredential(Usr, Pwd);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";

            webRequest.Method = "POST";
            return webRequest;
        }


        private static XmlDocument CreateSoapEnvelope(string xml)
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(xml);
            return soapEnvelopeDocument;
        }

    }
}
