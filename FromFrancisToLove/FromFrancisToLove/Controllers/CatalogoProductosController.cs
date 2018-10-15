using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FromFrancisToLove.Data;
using System.Xml.Serialization;
using FromFrancisToLove.Requests;
using System.IO;
using Tadenor;
using Diestel;
using System.Xml;
using FromFrancisToLove.Requests.ModuleDiestel;
using FromFrancisToLove.Models;
using System.Net;
using System.Xml.Linq;
using FromFrancisToLove.ExtensionMethods;

namespace FromFrancisToLove.Controllers
{
    //[Produces("application/xml")]
    [Route("api/CatalogoProductos")]
    public class CatalogoProductosController : Controller
    {
        protected const string EnvelopeBody_begin_xml = "<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><soap:Body><Ejecuta xmlns='http://www.pagoexpress.com.mx/pxUniversal'>";
        protected const string EnvelopeBody_end_xml = "</Ejecuta></soap:Body></soap:Envelope>";

        private readonly HouseOfCards_Context _context;
        //
        public CatalogoProductosController(HouseOfCards_Context context)
        {
            _context = context;
        }

        // GET: api/CatalogoProductos
        //[HttpGet]
        //public IActionResult Get()
        //{
            //try
            //{
            //    var config = _context.conexion_Configs.Find(1);
            //    var modulo = new ModuleTDV();
            //    string pago = TipoPago.Efectivo.AsText();

                //modulo.Grupo = module.Grupo;
                //modulo.Cadena = module.Cadena;
                //modulo.Tienda = module.Tienda;
                //modulo.POS = module.POS;
                //modulo.Cajero = module.Cajero;

                //modulo.SKU = "8469760000187";
                //modulo.EncriptionKey = config.CrypKey;
                //modulo.TokenValor = "1020304050";

                //var list = new List<Requests.DiestelMio.cCampo>();
                //list.AddRange(new Requests.DiestelMio.cCampo[]
                //{
                //    new Requests.DiestelMio.cCampo("IDGRUPO", 7),
                //    new Requests.DiestelMio.cCampo("IDCADENA", 1),
                //    new Requests.DiestelMio.cCampo("IDTIENDA", 1),
                //    new Requests.DiestelMio.cCampo("IDPOS", 1),
                //    new Requests.DiestelMio.cCampo("IDCAJERO", 1),
                //    new Requests.DiestelMio.cCampo("FECHALOCAL", DateTime.Now.ToString("dd/MM/yyyy")),
                //    new Requests.DiestelMio.cCampo("HORALOCAL", DateTime.Now.ToString("HH:mm:ss")),
                //    new Requests.DiestelMio.cCampo("FECHACONTABLE", DateTime.Now.ToString("dd/MM/yyyy")),
                //    new Requests.DiestelMio.cCampo("TRANSACCION", modulo.NextTicket),
                //    new Requests.DiestelMio.cCampo("SKU", modulo.SKU),
                //    new Requests.DiestelMio.cCampo("TIPOPAGO", pago),

                //    new Requests.DiestelMio.cCampo("REFERENCIA", "9A4E5ADBAE1E3E0DBA9A83", true),
                //    new Requests.DiestelMio.cCampo("MONTO", 20.00)
                //});

            //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Requests.DiestelMio.cCampo>), new XmlRootAttribute("cArrayCampos"));
            //    StringWriter sw = new StringWriter();
            //    XmlWriter writer = XmlWriter.Create(sw);
            //    xmlSerializer.Serialize(writer, list);

            //    var sXml = RemoveAllNamespaces(sw.ToString());

            //    HttpWebRequest webRequest = CreateWebRequest(config.Url, "http://www.pagoexpress.com.mx/pxUniversal/Ejecuta", config.Usr, config.Pwd);
            //    XmlDocument soapEnvelopeXml = CreateSoapEnvelope(sXml);

            //    InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            //    IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            //    asyncResult.AsyncWaitHandle.WaitOne();

            //    string soapResult;
            //    using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            //    {
            //        using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
            //        {
            //            soapResult = rd.ReadToEnd();
            //        }

            //        return Content(soapResult);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex);
            //}
        //}

        //private static HttpWebRequest CreateWebRequest(string url, string action, string Usr, string Pwd)
        //{
        //    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        //    webRequest.Credentials = new NetworkCredential(Usr, Pwd);
        //    webRequest.Headers.Add("SOAPAction", action);
        //    webRequest.ContentType = "text/xml;charset=\"utf-8\"";
        //    webRequest.Method = "POST";

        //    return webRequest;
        //}

        //private static XmlDocument CreateSoapEnvelope(string sXml)
        //{
        //    XmlDocument soapEnvelopeDocument = new XmlDocument();
        //    soapEnvelopeDocument.LoadXml($"{EnvelopeBody_begin_xml}{sXml}{EnvelopeBody_end_xml}");

        //    return soapEnvelopeDocument;
        //}

        //private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        //{
        //    using (Stream stream = webRequest.GetRequestStream())
        //    {
        //        soapEnvelopeXml.Save(stream);
        //    }
        //}

        //public static string RemoveAllNamespaces(string xmlDocument)
        //{
        //    XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

        //    return xmlDocumentWithoutNs.ToString();
        //}

        //private static XElement RemoveAllNamespaces(XElement xmlDocument)
        //{
        //    if (!xmlDocument.HasElements)
        //    {
        //        XElement xElement = new XElement(xmlDocument.Name.LocalName);
        //        xElement.Value = xmlDocument.Value;

        //        foreach (XAttribute attribute in xmlDocument.Attributes())
        //            xElement.Add(attribute);

        //        return xElement;
        //    }
        //    return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        //}

        // GET: api/CatalogoProductos/5
        //[HttpGet("{id}", Name = "Get")]
        //public IActionResult Get(string value)
        //{
        //    if (value != "")
        //    {
        //        var response = _context.catalogos_Productos.Select(n => new Catalogos_Producto
        //        {
        //            SKU = n.SKU,
        //            Nombre = n.Nombre
        //        }).Where(n => n.Tipo == value);

        //        return Ok(response);
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
            
        //}
        
        // POST: api/CatalogoProductos
        //[HttpPost()]
        //public IActionResult Post()
        //{
        //    try
        //    {            

        //        var query = new ReloadRequest();

        //        query.ID_GRP = 7;
        //        query.ID_CHAIN = 1;
        //        query.ID_MERCHANT = 1;
        //        query.ID_POS = 1;
        //        query.DateTime = DateTime.Now.ToString();
        //        query.SKU = "8469760101006";
        //        query.PhoneNumber = "1020304050";
        //        query.TransNumber = 1020;
        //        //query.ID_Product = "SBH001";
        //        //query.ID_COUNTRY = 0;
        //        query.TC = 0;

        //        //Serialización
        //        XmlSerializer xmlSerializer = new XmlSerializer(query.GetType());
        //        StringWriter sw = new StringWriter();
        //        XmlWriter writer = XmlWriter.Create(sw);
        //        xmlSerializer.Serialize(writer, query);
        //        var xml = sw.ToString();

        //        //Se mandan las credenciales
        //        ServicePXSoapClient client = new ServicePXSoapClient(ServicePXSoapClient.EndpointConfiguration.ServicePXSoap);

        //        client.ClientCredentials.UserName.UserName = Connected_Services.Tadenor.CredentialsTadenor.Usr;
        //        client.ClientCredentials.UserName.Password = Connected_Services.Tadenor.CredentialsTadenor.Psw;
                
        //        client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                
        //        //Se almacena la respuesta del ws
        //        var response = client.getReloadClassAsync(xml.ToString()).Result;

        //        //// Deserealización
        //        //var response_des = XmlToObject(response, typeof(ReloadResponse));

        //        //if (response_des == null)
        //        //{
        //        //    //return NotFound();
        //        //}


        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex);
        //    }
        //}

        public static object XmlToObject(string xml, Type objectType)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            object obj = null;
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(objectType);
                xmlReader = new XmlTextReader(strReader);
                obj = serializer.Deserialize(xmlReader);
            }
            catch (Exception exp)
            {
                //Handle Exception Code
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }

        // PUT: api/CatalogoProductos/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {

        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}
