using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static FromFrancisToLove.Payments.Backend.Services.Finfit;

namespace FromFrancisToLove.Payments.Backend.Services
{
    public class Finfit
    {
        public class Field
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public int Length { get; set; }
            public int Class { get; set; }
            public object Value { get; set; }
            public bool Encrypt { get; set; }
            public string Checksum { get; set; }
        }

        public class ResponseService
        {
            public int AuthorizeCode { get; set; }
            public List<Field> Fields { get; set; } //Mezclados, los que se envia y los que se reciben 
            public string XML { get; set; } //OrException// el que se recibe
            public bool Success { get; set; }
            public int ResponseCode { get; set; } //1000+ for exceptions
        }

        public class PaymentsService
        {
            private readonly string Url;
            private readonly string User;
            private readonly string Password;
            private readonly string EncryptedKey;
            private int Group { get; set; }
            private int Chain { get; set; }
            private int Merchant { get; set; }
            private int POS { get; set; }
            private int Cashier { get; set; }
            private bool IsConfigured { get; set; } = false;


            public PaymentsService(string Url, string User, string Password, string EncryptedKey = "")
            {
                this.Url = Url;
                this.User = User;
                this.Password = Password;
                this.EncryptedKey = EncryptedKey;
            }

            public void Config(int Group, int Chain, int Merchant, int POS, int Cashier = 1)
            {
                IsConfigured = true;
                this.Group = Group;
                this.Chain = Chain;
                this.Merchant = Merchant;
                this.POS = POS;
                this.Cashier = Cashier;
            }

            public List<Field> PaymentInfo(string SKU, string Reference)
            {
            if (!IsConfigured) throw new Exception();

            List<Field> array = new List<Field>();

            array.Add(new Field() { Name = "ID_GRP", Value = Group, Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "ID_CHAIN", Value = Chain, Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "ID_MERCHANT", Value = Merchant, Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "ID_POS", Value = POS, Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "DateTime", Value = DateTime.Now.ToString("dd/MM/yyyy HH:MM:ss"), Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "SKU", Value = SKU, Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "PhoneNumber", Value = Reference, Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "TransNumber", Value = "", Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "ID_Product", Value = "", Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "ID_COUNTRY", Value = 0, Length = 0, Type = 0, Class = 0, Encrypt = false });
            array.Add(new Field() { Name = "TC",Value = 0, Length = 0,Type = 0, Class = 0,Encrypt = false});

            var a = array;
            return array;
              
            }

            public ResponseService Request(List<Field> Fields)
            {
                List<Field> Respuesta = new List<Field>();
                ResponseService R_Service = new ResponseService();
                Class_TN TN = new Class_TN();            
                string service = "getReloadClass";
                string response = "ReloadResponse";
                bool Datos = false;
                foreach (var item in Fields)
                {
                    if ((item.Name == "ID_Product") && (item.Value.ToString() != ""))
                    {
                        service = "getReloadData";
                        response = "DataResponse";
                        Datos = true;
                    }
                }
                string[] credentials = new string[] { Url, User, Password };
                var task = Task.Run(() => { return TN.Send_Request(service, credentials, Fields); });
         
                try
                {
                    var success = task.Wait(50000);
                    if (!success)
                    {
                        return TN.TN_Query(Fields,Datos,Url,User,Password);                    
                    }
                    else
                    {                 
                        R_Service.XML = TN.Response_Xml(task.Result, service + "Result", response);
                        Respuesta = TN.Response_Fields(R_Service.XML);
                        R_Service.Fields = Fields.Union(Respuesta).ToList();
                        foreach (var item in Respuesta)
                        {
                            if (item.Name=="Response_ResponseCode")
                            {
                                R_Service.ResponseCode = Convert.ToInt32(item.Value);
                            }
                            if (item.Name == "Response_AutoNo")
                            {
                                R_Service.AuthorizeCode = Convert.ToInt32(item.Value);
                            }
                            


                        }
                    }
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerException;
                }
                if (R_Service.ResponseCode ==6 || R_Service.ResponseCode==71)
                {
                    //RealizaConsulta
                    return TN.TN_Query(Fields, Datos, Url, User, Password);

                }
                else if (R_Service.ResponseCode==0)
                {
                    R_Service.Success = true;
                }
                return R_Service;

                if (!IsConfigured) throw new Exception();
                return new ResponseService();
            }

            public ResponseService Check(List<Field> Fields)
            {
                List<Field> Respuesta = new List<Field>();
                ResponseService R_Service = new ResponseService();
                Class_TN TN = new Class_TN();
                bool Datos = false;
                foreach (var item in Fields)
                {
                    if ((item.Name == "ID_Product") && (item.Value.ToString() != ""))
                    {
                        Datos = true;
                    }
                }
                return TN.TN_Query(Fields, Datos, Url, User, Password);
                //TN Consulta
                if (!IsConfigured) throw new Exception();
                return new ResponseService();

            }

            public ResponseService Cancel(List<Field> Fields)
            {

                if (!IsConfigured) throw new Exception();
                return new ResponseService();

            }
        }
    }

    public class Class_TN
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Uso del WebService
        public ResponseService TN_Query(List<Field> Fields,bool Datos, string Url, string Usr, string Pdw)
        {
            List<Field> Respuesta = new List<Field>();
            ResponseService R_Service = new ResponseService();

            string service = "getQueryClass";
            string response = "QueryResponse";
            //Si existe producto desde la BD lo agrega
            if (Datos)
            {
                service = "getQueryDatClass";
                response = "DataQueryResponse";

            }

            foreach (var item in Fields)
            {
                if (item.Name=="DateTime")
                {
                    item.Value = DateTime.Now.ToString("dd/MM/yyyy HH:MM:ss");
                }
            }
            //string xml = GetResponse(service, xmlData);

            string[] credentials = new string[] { Url,Usr,Pdw};
            var task = Task.Run(() => { return Send_Request(service, credentials, Fields); });
            try
            {
                var success = task.Wait(50000);
                if (!success)
                {
                    //error 1001 tiempo de espera agotado
                    R_Service.Success = false;
                    R_Service.XML = Get_Xml(Fields, service);
                    R_Service.Fields = Fields;
                    R_Service.ResponseCode = 1001;
                    // return "1001";
                }
                else
                {

                    R_Service.XML = Response_Xml(task.Result, service + "Result", response);
                    Respuesta = Response_Fields(R_Service.XML);

                    R_Service.Fields = Fields.Union(Respuesta).ToList();
                    foreach (var item in Respuesta)
                    {
                        if (item.Name == "Response_ResponseCode")
                        {
                            R_Service.ResponseCode = Convert.ToInt32(item.Value);              
                        }
                        if (item.Name == "Response_AutoNo")
                        {
                            R_Service.AuthorizeCode = Convert.ToInt32(item.Value);
                        }
                        R_Service.Success = false;
                        if (R_Service.ResponseCode == 0)
                        {
                            R_Service.Success = true;
                        }   
                    }
                    
                }
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            // return jString(ResponseXml);

            return R_Service;
        }
   
        public string Get_Xml(List<Field> Fields,string Nodo)
        {
            //Fields to String 
            string xml = "<"+Nodo+">";
            foreach (var item in Fields)
            {
                xml += "<"+item.Name+">"+item.Value+ "</" + item.Name + ">";
            }
            xml+= "</" + Nodo + ">";

           // xml= (@"<?xml version=""1.0"" encoding=""utf-8""?>" )+ xml;
            xml= ScapeXML(@"<?xml version=""1.0"" encoding=""utf-8""?>" + xml);
            return xml;
        }

        public string Send_Request(string service, string[] credentials, List<Field> Fields)
        {
            var sXML = Get_Xml(Fields,service);
            HttpWebRequest webRequest = CreateWebRequest(credentials[0], "http://www.pagoexpress.com.mx/ServicePX/" + service, credentials[1], credentials[2]);

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(service, sXML);
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
            }
            return soapResult;
        }

        public List<Field> Response_Fields(string xml)
        {
            List<Field> array = new List<Field>();
            string value = "";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName("ResponseCode");     
            foreach (XmlNode node in nodeList) {  value = node.InnerText; }
            array.Add(new Field() { Name = "Response_ResponseCode", Value = value, Length = 0, Type = 0, Class = 0, Encrypt = false });
            nodeList = xmldoc.GetElementsByTagName("Monto");
            foreach (XmlNode node in nodeList) { value = node.InnerText; }
            array.Add(new Field() { Name = "Response_Monto", Value = value, Length = 0, Type = 0, Class = 0, Encrypt = false });
            nodeList = xmldoc.GetElementsByTagName("PhoneNumber");
            foreach (XmlNode node in nodeList) { value = node.InnerText; }
            array.Add(new Field() { Name = "Response_PhoneNumber", Value = value, Length = 0, Type = 0, Class = 0, Encrypt = false });
            nodeList = xmldoc.GetElementsByTagName("TransNumber");
            foreach (XmlNode node in nodeList) { value = node.InnerText; }
            array.Add(new Field() { Name = "Response_TransNumber", Value = value, Length = 0, Type = 0, Class = 0, Encrypt = false });
            nodeList = xmldoc.GetElementsByTagName("AutoNo");
            foreach (XmlNode node in nodeList) { value = node.InnerText; }
            array.Add(new Field() { Name = "Response_AutoNo", Value = value, Length = 0, Type = 0, Class = 0, Encrypt = false });
            nodeList = xmldoc.GetElementsByTagName("DateTime");
            foreach (XmlNode node in nodeList) { value = node.InnerText; }
            array.Add(new Field() { Name = "Response_DateTime", Value = value, Length = 0, Type = 0, Class = 0, Encrypt = false });
            return array;
        }

        public string Response_Xml(string xml, string path, string response)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName(path);
            ResponseService responseService = new ResponseService();
            foreach (XmlNode node in nodeList)
            {
                xml = node.InnerText;
            }
            xmldoc.LoadXml(Un_ScapeXML(xml));
            return xml;
        }

        private XmlDocument CreateSoapEnvelope(string service, string sXML)
        {
            string xml =
             @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">" +
             "<soap:Body>" +
             "<" + service + @" xmlns=""http://www.pagoexpress.com.mx/ServicePX"">" +
             @"<sXML>" + sXML + "</sXML>" +
             "</" + service + ">" +
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
    }
}
