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
using FromFrancisToLove.Requests.ModuleTadenor;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using FromFrancisToLove.Models;
using Formatting = Newtonsoft.Json.Formatting;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;



namespace FromFrancisToLove.Controllers
{
    public class TNController : Controller
    {
        Class_TN ClassTN = new Class_TN();
        MyRelReq xmlData = new MyRelReq();
        public readonly HouseOfCards_Context _context;
        public TNController(HouseOfCards_Context context)
        {
            _context = context;
        }

        //Consulta de Sku
        public string Get_Sku(string sku, string referencia)
        {

           
            var item = _context.catalogos_Productos.First(b => b.SKU == sku);
            string respuesta =
                "[" +
                "{\"sCampo\":\"SKU\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.SKU + "\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"IdProduct\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.IDProduct + "\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"Monto\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.Monto + "\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"Marca\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.Marca + "\", \"bEncriptado\":true}," +
                "{\"sCampo\":\"CONFIGID\", \"iTipo\":0, \"iLongitud\":0, \"iClase\":0, \"sValor\":\"" + item.CONFIGID + "\", \"bEncriptado\":true}" +
                "]";
            return respuesta;

        }
       //Recarga/Datos
        public string TN_Service(string sku,string referencia)
        {
            //Sku="TN-8469761001749"
            xmlData.SKU = sku;
            xmlData.PhoneNumber = referencia;

            string service = "getReloadClass";
            string response = "ReloadResponse";
           
            string[] prefixSku = xmlData.SKU.Split("-");

            var producto = _context.catalogos_Productos.First(a => a.SKU == xmlData.SKU);
            if (producto.IDProduct != "")
            {
                service = "getReloadData";
                response = "DataResponse";
                xmlData.ID_Product = producto.IDProduct;
            }
            xmlData.SKU = prefixSku[1];
            var credenciales = _context.conexion_Configs.Find(2);
            var item = _context.conexion_Configs.Find(2);

            string[] credentials = new string[] { item.Url, item.Usr, item.Pwd };

            var task = Task.Run(() => { return ClassTN.Send_Request(service, xmlData, credentials); });
            MyRelReq ResponseXml = new MyRelReq();
            try
            {
                var success = task.Wait(50000);
                if (!success)
                {
                    return TN_Query(sku,referencia);
                }
                else
                {
                    ResponseXml = ClassTN.Deserializer_Response(task.Result, service + "Result", response);
                }
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            // 71 o 6 
            if (ResponseXml.ResponseCode == "6" || ResponseXml.ResponseCode == "71")
            {
                return TN_Query(sku,referencia );
            }

            return ClassTN.jString(ResponseXml);
        }
        //Consultar Recarga/Datos
        public string TN_Query(string sku,string referencia)
        {
            //Sku="TN-8469761001749"
            xmlData.SKU = sku;
            xmlData.PhoneNumber = referencia;
            // string a = Metodo(xmlData);
            string service = "getQueryClass";
            string response = "QueryResponse";
            //Si existe producto desde la BD lo agrega
            if (xmlData.ID_Product != null)
            {
                service = "getQueryDatClass";
                response = "DataQueryResponse";

            }
            MyRelReq ResponseXml = new MyRelReq();

            xmlData.Datetime = DateTime.Now;
            var item = _context.conexion_Configs.Find(2);
            //string xml = GetResponse(service, xmlData);

            string[] credentials = new string[] { item.Url, item.Usr, item.Pwd };
            var task = Task.Run(() => { return ClassTN.Send_Request(service, xmlData, credentials); });
            try
            {
                var success = task.Wait(50000);
                if (!success)
                {
                    return "Lo sentimos el servicio tardo mas de los esperado :( ";
                }
                else
                {
                    ResponseXml = ClassTN.Deserializer_Response(task.Result, service + "Result", response);
                }
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            return ClassTN.jString(ResponseXml);
        }
    }

    public class Class_TN
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Uso del WebService
        public string Send_Request(string servicio, MyRelReq xmlQuery, string[] credencial)
        {

            // Crea el xml de acuerdo  al procesos, ya sea  Recarga o consulta
            var sXML = Get_XMLs(xmlQuery, servicio);

            HttpWebRequest webRequest = CreateWebRequest(credencial[0], "http://www.pagoexpress.com.mx/ServicePX/" + servicio, credencial[1], credencial[2]);

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
                //  Thread.Sleep(20000);
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
            xmldoc.LoadXml(Des_ScapeXML(xml));
            nodeList = xmldoc.GetElementsByTagName("ResponseCode");

            XmlSerializer xmls = new XmlSerializer(typeof(MyRelReq));

            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);

            StreamReader sr = new StreamReader(stream);
            MyRelReq modelo = (MyRelReq)xmls.Deserialize(sr);
            return modelo;
        }
        //Deserealiza el xml que sera enviado Saldo/Datos
        private string Get_XMLs(object xmlData, string Nodo)
        {

            XmlSerializer xmls = new XmlSerializer(xmlData.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, settings);
            xmls.Serialize(writer, xmlData);
            string xml = sw.ToString().Replace("MyRelReq", Nodo);
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

        private string Des_ScapeXML(string sXML)
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


    public class MyRelReq
    {

        [XmlElement("ID_GRP")]

        public int ID_GRP
        {

            get { return 7; }
            set { }

        }
        [XmlElement("ID_CHAIN")]
        public int ID_CHAIN
        {
            get { return 1; }
            set { }
        }
        [XmlElement("ID_MERCHANT")]
        public int ID_MERCHANT
        {
            get { return 1; }
            set { }
        }
        [XmlElement("ID_POS")]
        public int ID_POS
        {
            get { return 1; }
            set { }
        }
        [XmlElement("DateTime")]
        public DateTime Datetime
        {
            get { return DateTime.Now;}
            set { }
        }
        [XmlElement("SKU")]
        public string SKU { get; set; }
        [XmlElement("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [XmlElement("TransNumber")]
        public int TransNumber { get { return 1020; } set { } }
        [XmlElement("TC")]
        public int TC { get { return 0; } set { } }

        [XmlElement("ID_Product")]
        public string ID_Product { get; set; }

        [XmlElement("ID_COUNTRY")]
        public int ID_COUNTRY { get; set; }

        public string Brand { get; set; }
        public string Instr1 { get; set; }
        public string Instr2 { get; set; }
        public int AutoNo { get; set; }
        public string ResponseCode { get; set; }
        public string DescripcionCode { get; set; }
        public string Monto { get; set; }

    }

    public class HouseOfCards_Context : DbContext
    {
        public HouseOfCards_Context(DbContextOptions<HouseOfCards_Context> options) : base(options) { }
        public DbSet<Conexion_Config> conexion_Configs { get; set; }
        public DbSet<Catalogos_Producto> catalogos_Productos { get; set; }
    }

    [Table("Conexion_Config", Schema = "HouseOfCards")]
    public class Conexion_Config
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigID { get; set; }
        public string Url { get; set; }
        public string Usr { get; set; }
        public string Pwd { get; set; }
        public string CrypKey { get; set; }
        public int WSGroupId { get; set; }

    }


    [Table("Catalogo_Producto", Schema = "HouseOfCards")]
    public class Catalogos_Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SKU { get; set; }
        public int CONFIGID { get; set; }
        public string IDProduct { get; set; }
        public string Monto { get; set; }
        public string Marca { get; set; }
    }
}

