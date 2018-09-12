using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using FromFrancisToLove.Data;
using FromFrancisToLove.Connected_Services.Tadenor;
using Tadenor;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FromFrancisToLove.Requests;
using System.Text;

namespace FromFrancisToLove.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly HouseOfCards_Context _context;

        public ValuesController(HouseOfCards_Context context)
        {
           
            _context = context;
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                ServicePXSoapClient.EndpointConfiguration endpoint = new ServicePXSoapClient.EndpointConfiguration();
                ServicePXSoapClient client = new ServicePXSoapClient(endpoint);

                client.ClientCredentials.UserName.UserName = CredentialsTadenor.Usr;
                client.ClientCredentials.UserName.Password = CredentialsTadenor.Psw;
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                var saldo = client.SaldoDisponibleAsync(7, 1, 1).Result;


                return Ok(saldo);
            }
            catch (Exception ex)
            {
                return Ok($"{ex}");
            }
        }

        [HttpGet("0")]
        public IActionResult GetBD()
        {
            return Json(_context.conexion_Configs.ToList());
        }

        [HttpGet("1")]
        public IActionResult GetById(int id)
        {


            var item = _context.conexion_Configs.Find(id);
            if (item == null)
            {
                return NotFound();
            }

            ServicePXSoapClient.EndpointConfiguration endpoint = new ServicePXSoapClient.EndpointConfiguration();
            ServicePXSoapClient client = new ServicePXSoapClient(endpoint, item.Url);

            client.ClientCredentials.UserName.UserName = item.Usr;
            client.ClientCredentials.UserName.Password = item.Pwd;
            client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

            var saldo = client.SaldoDisponibleAsync(7, 1, 1).Result;
            return Ok(saldo);
        }

        // GET api/values/5
        [HttpGet("2")]
        public string Getxml(int id)
        {
            var item = _context.conexion_Configs.Find(2);

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope();
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/SaldoDisponible", item.Usr, item.Pwd);
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
                return soapResult;

            }

        }


        [HttpGet("3")]
        public IActionResult Post(/*int idGroup, int idChain, int idMerchant,int idPos*/)
        {

            Envelope_SD x = new Envelope_SD
            {
                body = new Body_SD
                {
                    SaldoDisponible = new Campos_SD
                    {
                        lGrupo = 7,
                        lCadena = 1,
                        lTienda = 1,
                    }
                }
            };

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("xsd", "http://www.w3.org/2001/XMLSchema");

            XmlSerializer xmlPrueba = new XmlSerializer(typeof(Envelope_SD));

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw,settings);
            xmlPrueba.Serialize(writer, x, ns);
            
            var xml = sw.ToString();


            var item = _context.conexion_Configs.Find(2);
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/SaldoDisponible", item.Usr, item.Pwd);
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope2(xml);

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


        private static HttpWebRequest CreateWebRequest(string url, string action, string Usr, string Pwd)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = new System.Net.NetworkCredential(Usr, Pwd);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";

            webRequest.Method = "POST";
            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope()
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
          soapEnvelopeDocument.LoadXml(@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""><soap:Body><SaldoDisponible xmlns=""http://www.pagoexpress.com.mx/ServicePX""><lGrupo>7</lGrupo><lCadena>1</lCadena><lTienda>1</lTienda></SaldoDisponible></soap:Body></soap:Envelope>"); return soapEnvelopeDocument;
            
        }

        private static XmlDocument CreateSoapEnvelope2(string xml)
        {
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






        //[HttpGet("4")]
        //public IActionResult Xml(/*int idGroup, int idChain, int idMerchant,int idPos*/)
        //{
         

        //    XmlPrueba xmlTest = new XmlPrueba();
        //    xmlTest.Name = "IT";
        //    xmlTest.Employees.Add(new Employee("Empleado|",new DataEmployee("H")));
        //    xmlTest.Employees.Add(new Employee("Empleado2"));
        //    xmlTest.Employees.Add(new Employee("eMPLEADO3"));
        //    xmlTest.Employees.Add(new Employee("Empleado4"));

        //    XmlSerializer xmlPrueba = new XmlSerializer(xmlTest.GetType());

           
        //    var settings = new XmlWriterSettings();
        //    settings.Indent = true;
        //    settings.OmitXmlDeclaration = true;
        //    StringWriter sw = new StringWriter();
        //    XmlWriter writer = XmlWriter.Create(sw, settings);
        //    xmlPrueba.Serialize(writer, xmlTest);

        //    var xml = sw.ToString();

        //    return Ok();
        //}









        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
  
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
