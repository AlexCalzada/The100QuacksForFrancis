using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FromFrancisToLove.Models;
using FromFrancisToLove.Data;
using FromFrancisToLove.Connected_Services.Tadenor;
using Tadenor;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;

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
            return Json( _context.conexion_Configs.ToList());
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
        public string Get(int id)
        {
            var item = _context.conexion_Configs.Find(2);

            //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(item.Url);
            //webRequest.Credentials = new System.Net.NetworkCredential(item.Usr, item.Pwd);
            //webRequest.Headers.Add("SOAPAction", item.Url+"/SaldoDisponible");
            //webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            //webRequest.Accept = "text/xml";
            //webRequest.Method = "POST";



            //IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);


            //return null;}

     

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope();
            HttpWebRequest webRequest = CreateWebRequest(item.Url, "http://www.pagoexpress.com.mx/ServicePX/SaldoDisponible",item.Usr,item.Pwd);
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

            // begin async call to web request.
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
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
        private static HttpWebRequest CreateWebRequest(string url, string action, string Usr, string Pwd)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = new System.Net.NetworkCredential(Usr,Pwd);
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

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        // POST api/values
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
